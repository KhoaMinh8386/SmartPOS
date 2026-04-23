using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using SmartPos.Module.Suppliers.Models;
using SmartPos.Module.Suppliers.Templates;

namespace SmartPos.Module.Suppliers.Backend
{
    public class SupplierBackend
    {
        private readonly string _connectionString;

        public SupplierBackend()
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
            using (SqlCommand command = new SqlCommand(SupplierSqlTemplate.EnsureSchema, connection))
            {
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public List<SupplierListItem> GetSuppliers()
        {
            var result = new List<SupplierListItem>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand(SupplierSqlTemplate.GetSuppliers, connection))
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new SupplierListItem
                        {
                            SupplierID = Convert.ToInt32(reader["SupplierID"]),
                            SupplierName = Convert.ToString(reader["SupplierName"]),
                            Phone = Convert.ToString(reader["Phone"]),
                            Address = Convert.ToString(reader["Address"]),
                            ImageUrl = Convert.ToString(reader["ImageUrl"]),
                            TotalDebt = Convert.ToDecimal(reader["TotalDebt"])
                        });
                    }
                }
            }

            return result;
        }

        public List<SupplierOrderItem> GetSupplierOrders(int supplierId)
        {
            var result = new List<SupplierOrderItem>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand(SupplierSqlTemplate.GetSupplierOrders, connection))
            {
                command.Parameters.AddWithValue("@SupplierID", supplierId);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new SupplierOrderItem
                        {
                            PurchaseOrderID = Convert.ToInt32(reader["PurchaseOrderID"]),
                            InvoiceCode = Convert.ToString(reader["POCode"]),
                            OrderDate = Convert.ToDateTime(reader["OrderDate"]),
                            TotalAmount = Convert.ToDecimal(reader["TotalAmount"]),
                            PaidAmount = Convert.ToDecimal(reader["PaidAmount"]),
                            DebtAmount = Convert.ToDecimal(reader["DebtAmount"]),
                            StatusText = Convert.ToString(reader["StatusText"])
                        });
                    }
                }
            }

            return result;
        }

        public void AddPayment(SupplierPaymentRequest request)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    decimal debtAmount = GetCurrentDebt(connection, transaction, request.SupplierID, request.PurchaseOrderID);
                    if (request.Amount > debtAmount)
                    {
                        throw new InvalidOperationException("Số tiền thanh toán vượt quá công nợ hiện tại.");
                    }

                    using (SqlCommand insertCommand = new SqlCommand(SupplierSqlTemplate.InsertPayment, connection, transaction))
                    {
                        insertCommand.Parameters.AddWithValue("@SupplierID", request.SupplierID);
                        insertCommand.Parameters.AddWithValue("@PurchaseOrderID", request.PurchaseOrderID);
                        insertCommand.Parameters.AddWithValue("@Amount", request.Amount);
                        insertCommand.Parameters.AddWithValue("@PaymentMethod", request.PaymentMethod);
                        insertCommand.Parameters.AddWithValue("@Note", string.IsNullOrWhiteSpace(request.Note) ? (object)DBNull.Value : request.Note);
                        insertCommand.Parameters.AddWithValue("@CreatedByUserID", request.CreatedByUserID.HasValue ? (object)request.CreatedByUserID.Value : DBNull.Value);
                        insertCommand.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
            }
        }

        private static decimal GetCurrentDebt(SqlConnection connection, SqlTransaction transaction, int supplierId, int purchaseOrderId)
        {
            using (SqlCommand debtCommand = new SqlCommand(SupplierSqlTemplate.GetCurrentDebtByOrder, connection, transaction))
            {
                debtCommand.Parameters.AddWithValue("@PurchaseOrderID", purchaseOrderId);
                debtCommand.Parameters.AddWithValue("@SupplierID", supplierId);

                object debtRaw = debtCommand.ExecuteScalar();
                if (debtRaw == null || debtRaw == DBNull.Value)
                {
                    throw new InvalidOperationException("Phiếu nhập hàng không hợp lệ.");
                }

                return Convert.ToDecimal(debtRaw);
            }
        }

        public void SaveSupplier(SupplierListItem supplier)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = supplier.SupplierID == 0 
                    ? "INSERT INTO dbo.Suppliers (SupplierName, Phone, Address, ImageUrl, IsActive) VALUES (@Name, @Phone, @Address, @ImageUrl, 1)"
                    : "UPDATE dbo.Suppliers SET SupplierName = @Name, Phone = @Phone, Address = @Address, ImageUrl = @ImageUrl WHERE SupplierID = @ID";
                
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    if (supplier.SupplierID > 0) command.Parameters.AddWithValue("@ID", supplier.SupplierID);
                    command.Parameters.AddWithValue("@Name", supplier.SupplierName);
                    command.Parameters.AddWithValue("@Phone", supplier.Phone ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Address", supplier.Address ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@ImageUrl", supplier.ImageUrl ?? (object)DBNull.Value);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void DeleteSupplier(int id)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("UPDATE dbo.Suppliers SET IsActive = 0 WHERE SupplierID = @ID", connection))
                {
                    command.Parameters.AddWithValue("@ID", id);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
