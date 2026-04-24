using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using SmartPos.Module.XuatHang.Models;
using SmartPos.Module.XuatHang.Templates;

namespace SmartPos.Module.XuatHang.Backend
{
    public class StockOutBackend
    {
        private readonly string _connectionString;

        public StockOutBackend()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["SmartPosDb"]?.ConnectionString;
            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                throw new InvalidOperationException("Missing SmartPosDb connection string in App.config.");
            }
        }

        public void EnsureSchema()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand(StockOutSqlTemplate.EnsureSchema, connection))
            {
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public List<WarehouseLookup> GetWarehouses()
        {
            var result = new List<WarehouseLookup>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(StockOutSqlTemplate.GetWarehouses, conn))
            {
                conn.Open();
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        result.Add(new WarehouseLookup
                        {
                            WarehouseID = (int)rdr["WarehouseID"],
                            WarehouseName = rdr["WarehouseName"].ToString()
                        });
                    }
                }
            }
            return result;
        }

        public List<ProductInventoryItem> GetProductInventory(int warehouseId, string search)
        {
            var result = new List<ProductInventoryItem>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(StockOutSqlTemplate.GetProductInventory, conn))
            {
                cmd.Parameters.AddWithValue("@WarehouseID", warehouseId);
                cmd.Parameters.AddWithValue("@Search", string.IsNullOrWhiteSpace(search) ? (object)DBNull.Value : "%" + search + "%");
                conn.Open();
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        result.Add(new ProductInventoryItem
                        {
                            ProductID = (int)rdr["ProductID"],
                            ProductCode = rdr["ProductCode"].ToString(),
                            ProductName = rdr["ProductName"].ToString(),
                            UnitName = rdr["UnitName"]?.ToString(),
                            BatchNumber = rdr["BatchNumber"]?.ToString(),
                            ExpiryDate = rdr["ExpiryDate"] == DBNull.Value ? (DateTime?)null : (DateTime)rdr["ExpiryDate"],
                            Quantity = (decimal)rdr["Quantity"]
                        });
                    }
                }
            }
            return result;
        }

        public void SaveStockOut(StockOutRequest request)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 1. Generate Code
                        string code = GenerateCode(connection, transaction);

                        // 2. Insert Header
                        int stockOutId;
                        using (SqlCommand cmd = new SqlCommand(StockOutSqlTemplate.InsertStockOut, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@StockOutCode", code);
                            cmd.Parameters.AddWithValue("@WarehouseID", request.WarehouseID);
                            cmd.Parameters.AddWithValue("@Reason", request.Reason);
                            cmd.Parameters.AddWithValue("@Notes", (object)request.Notes ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@CreatedByUserID", (object)request.CreatedByUserID ?? DBNull.Value);
                            stockOutId = (int)cmd.ExecuteScalar();
                        }

                        // 3. Insert Details and Update Inventory
                        foreach (var detail in request.Details)
                        {
                            // Validate quantity
                            decimal currentQty = GetCurrentQuantity(connection, transaction, request.WarehouseID, detail.ProductID, detail.BatchNumber);
                            if (detail.Quantity > currentQty)
                            {
                                throw new Exception($"Sản phẩm {detail.ProductID} vượt quá tồn kho hiện tại ({currentQty}).");
                            }

                            // Insert Detail
                            using (SqlCommand cmd = new SqlCommand(StockOutSqlTemplate.InsertStockOutDetail, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@StockOutID", stockOutId);
                                cmd.Parameters.AddWithValue("@ProductID", detail.ProductID);
                                cmd.Parameters.AddWithValue("@BatchNumber", (object)detail.BatchNumber ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@ExpiryDate", (object)detail.ExpiryDate ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@Quantity", detail.Quantity);
                                cmd.ExecuteNonQuery();
                            }

                            // Update Inventory
                            using (SqlCommand cmd = new SqlCommand(StockOutSqlTemplate.UpdateInventory, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@WarehouseID", request.WarehouseID);
                                cmd.Parameters.AddWithValue("@ProductID", detail.ProductID);
                                cmd.Parameters.AddWithValue("@BatchNumber", (object)detail.BatchNumber ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@Quantity", detail.Quantity);
                                cmd.ExecuteNonQuery();
                            }

                            // Insert Stock Transaction
                            using (SqlCommand cmd = new SqlCommand(StockOutSqlTemplate.InsertStockTransaction, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@WarehouseID", request.WarehouseID);
                                cmd.Parameters.AddWithValue("@ProductID", detail.ProductID);
                                cmd.Parameters.AddWithValue("@Quantity", detail.Quantity);
                                cmd.Parameters.AddWithValue("@BatchNumber", (object)detail.BatchNumber ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@ExpiryDate", (object)detail.ExpiryDate ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@ReferenceID", stockOutId);
                                cmd.Parameters.AddWithValue("@Note", "Xuất kho: " + request.Reason);
                                cmd.Parameters.AddWithValue("@CreatedByUserID", (object)request.CreatedByUserID ?? DBNull.Value);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private string GenerateCode(SqlConnection conn, SqlTransaction trans)
        {
            using (SqlCommand cmd = new SqlCommand(StockOutSqlTemplate.GetNextCode, conn, trans))
            {
                int nextId = (int)cmd.ExecuteScalar();
                return "XK" + DateTime.Now.ToString("yyMMdd") + nextId.ToString("D3");
            }
        }

        private decimal GetCurrentQuantity(SqlConnection conn, SqlTransaction trans, int warehouseId, int productId, string batchNumber)
        {
            string sql = "SELECT Quantity FROM dbo.Inventory WHERE WarehouseID = @WarehouseID AND ProductID = @ProductID AND ISNULL(BatchNumber, '') = ISNULL(@BatchNumber, '')";
            using (SqlCommand cmd = new SqlCommand(sql, conn, trans))
            {
                cmd.Parameters.AddWithValue("@WarehouseID", warehouseId);
                cmd.Parameters.AddWithValue("@ProductID", productId);
                cmd.Parameters.AddWithValue("@BatchNumber", (object)batchNumber ?? DBNull.Value);
                object result = cmd.ExecuteScalar();
                return result == null || result == DBNull.Value ? 0 : (decimal)result;
            }
        }
    }

    public class WarehouseLookup
    {
        public int WarehouseID { get; set; }
        public string WarehouseName { get; set; }
        public override string ToString() => WarehouseName;
    }
}
