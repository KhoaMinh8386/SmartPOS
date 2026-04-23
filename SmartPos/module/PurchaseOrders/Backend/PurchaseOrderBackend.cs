using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using SmartPos.Module.PurchaseOrders.Models;
using SmartPos.Module.PurchaseOrders.Templates;

namespace SmartPos.Module.PurchaseOrders.Backend
{
    public class PurchaseOrderBackend
    {
        private readonly string _connectionString;

        public PurchaseOrderBackend()
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
            using (SqlCommand command = new SqlCommand(PurchaseOrderSqlTemplate.EnsureSchema, connection))
            {
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public PurchaseOrderModuleData LoadMasterData()
        {
            return new PurchaseOrderModuleData
            {
                Suppliers = QuerySuppliers(),
                Users = QueryUsers(),
                Warehouses = QueryWarehouses(),
                Products = QueryProducts()
            };
        }

        public int CreatePurchaseOrder(CreatePurchaseOrderRequest request)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        decimal totalAmount = 0m;
                        for (int i = 0; i < request.Items.Count; i++)
                        {
                            totalAmount += request.Items[i].LineTotal;
                        }

                        string poCode = "PN" + DateTime.Now.ToString("yyyyMMddHHmmssfff");

                        int purchaseOrderId;
                        using (SqlCommand command = new SqlCommand(PurchaseOrderSqlTemplate.CreatePurchaseOrder, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@POCode", poCode);
                            command.Parameters.AddWithValue("@SupplierID", request.SupplierID);
                            command.Parameters.AddWithValue("@WarehouseID", request.WarehouseID);
                            command.Parameters.AddWithValue("@CreatedByUserID", request.CreatedByUserID);
                            command.Parameters.AddWithValue("@OrderDate", request.OrderDate);
                            command.Parameters.AddWithValue("@TotalAmount", totalAmount);
                            command.Parameters.AddWithValue("@PaymentStatus", request.PaymentStatus);
                            command.Parameters.AddWithValue("@Notes", string.IsNullOrWhiteSpace(request.Notes) ? (object)DBNull.Value : request.Notes);

                            purchaseOrderId = Convert.ToInt32(command.ExecuteScalar());
                        }

                        for (int i = 0; i < request.Items.Count; i++)
                        {
                            PurchaseOrderDraftItem item = request.Items[i];
                            InsertPurchaseOrderItem(connection, transaction, purchaseOrderId, item);
                            UpsertInventoryBatch(connection, transaction, request.WarehouseID, item);
                        }

                        transaction.Commit();
                        return purchaseOrderId;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public List<FefoBatchItem> GetFefoBatches(int productId)
        {
            var result = new List<FefoBatchItem>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand(PurchaseOrderSqlTemplate.GetFefoBatches, connection))
            {
                command.Parameters.AddWithValue("@ProductID", productId);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new FefoBatchItem
                        {
                            InventoryID = Convert.ToInt32(reader["InventoryID"]),
                            ProductID = Convert.ToInt32(reader["ProductID"]),
                            ProductCode = Convert.ToString(reader["ProductCode"]),
                            ProductName = Convert.ToString(reader["ProductName"]),
                            BatchNumber = Convert.ToString(reader["BatchNumber"]),
                            ExpiryDate = reader["ExpiryDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["ExpiryDate"]),
                            Quantity = Convert.ToDecimal(reader["Quantity"]),
                            WarehouseName = Convert.ToString(reader["WarehouseName"])
                        });
                    }
                }
            }

            return result;
        }

        private static void InsertPurchaseOrderItem(SqlConnection connection, SqlTransaction transaction, int purchaseOrderId, PurchaseOrderDraftItem item)
        {
            using (SqlCommand command = new SqlCommand(PurchaseOrderSqlTemplate.AddPurchaseOrderItem, connection, transaction))
            {
                command.Parameters.AddWithValue("@PurchaseOrderID", purchaseOrderId);
                command.Parameters.AddWithValue("@ProductID", item.ProductID);
                command.Parameters.AddWithValue("@UnitID", item.UnitID);
                command.Parameters.AddWithValue("@Quantity", item.Quantity);
                command.Parameters.AddWithValue("@CostPrice", item.CostPrice);
                command.Parameters.AddWithValue("@BatchNumber", string.IsNullOrWhiteSpace(item.BatchNumber) ? (object)DBNull.Value : item.BatchNumber);
                command.Parameters.AddWithValue("@ManufactureDate", item.ManufactureDate.HasValue ? (object)item.ManufactureDate.Value.Date : DBNull.Value);
                command.Parameters.AddWithValue("@ExpiryDate", item.ExpiryDate.HasValue ? (object)item.ExpiryDate.Value.Date : DBNull.Value);
                command.ExecuteNonQuery();
            }
        }

        private static void UpsertInventoryBatch(SqlConnection connection, SqlTransaction transaction, int warehouseId, PurchaseOrderDraftItem item)
        {
            using (SqlCommand command = new SqlCommand(PurchaseOrderSqlTemplate.UpsertInventory, connection, transaction))
            {
                command.Parameters.AddWithValue("@WarehouseID", warehouseId);
                command.Parameters.AddWithValue("@ProductID", item.ProductID);
                command.Parameters.AddWithValue("@BatchNumber", string.IsNullOrWhiteSpace(item.BatchNumber) ? (object)DBNull.Value : item.BatchNumber);
                command.Parameters.AddWithValue("@ManufactureDate", item.ManufactureDate.HasValue ? (object)item.ManufactureDate.Value.Date : DBNull.Value);
                command.Parameters.AddWithValue("@ExpiryDate", item.ExpiryDate.HasValue ? (object)item.ExpiryDate.Value.Date : DBNull.Value);
                command.Parameters.AddWithValue("@Quantity", item.Quantity);
                command.Parameters.AddWithValue("@CostPrice", item.CostPrice);
                command.ExecuteNonQuery();
            }
        }

        private List<SupplierOption> QuerySuppliers()
        {
            var result = new List<SupplierOption>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand(PurchaseOrderSqlTemplate.GetSuppliers, connection))
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new SupplierOption
                        {
                            SupplierID = Convert.ToInt32(reader["SupplierID"]),
                            SupplierName = Convert.ToString(reader["SupplierName"])
                        });
                    }
                }
            }

            return result;
        }

        private List<UserOption> QueryUsers()
        {
            var result = new List<UserOption>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand(PurchaseOrderSqlTemplate.GetUsers, connection))
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new UserOption
                        {
                            UserID = Convert.ToInt32(reader["UserID"]),
                            FullName = Convert.ToString(reader["FullName"])
                        });
                    }
                }
            }

            return result;
        }

        private List<WarehouseOption> QueryWarehouses()
        {
            var result = new List<WarehouseOption>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand(PurchaseOrderSqlTemplate.GetWarehouses, connection))
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new WarehouseOption
                        {
                            WarehouseID = Convert.ToInt32(reader["WarehouseID"]),
                            WarehouseName = Convert.ToString(reader["WarehouseName"])
                        });
                    }
                }
            }

            return result;
        }

        private List<ProductOption> QueryProducts()
        {
            var result = new List<ProductOption>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand(PurchaseOrderSqlTemplate.GetProducts, connection))
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new ProductOption
                        {
                            ProductID = Convert.ToInt32(reader["ProductID"]),
                            BaseUnitID = Convert.ToInt32(reader["BaseUnitID"]),
                            ProductCode = Convert.ToString(reader["ProductCode"]),
                            ProductName = Convert.ToString(reader["ProductName"])
                        });
                    }
                }
            }

            return result;
        }
    }
}
