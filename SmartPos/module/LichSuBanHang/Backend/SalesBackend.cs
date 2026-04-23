using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using SmartPos.Module.SalesHistory.Models;
using SmartPos.Module.SalesHistory.Templates;

namespace SmartPos.Module.SalesHistory.Backend
{
    public class SalesBackend
    {
        private readonly string _connectionString;

        public SalesBackend()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["SmartPosDb"]?.ConnectionString;
        }

        public List<SalesOrderListItem> GetSalesHistory(DateTime from, DateTime to, int? staffId, string customerSearch, byte? payMethod, int? status)
        {
            var result = new List<SalesOrderListItem>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(SalesSqlTemplate.GetSalesHistory, conn))
            {
                cmd.Parameters.AddWithValue("@FromDate", from.Date);
                cmd.Parameters.AddWithValue("@ToDate", to.Date.AddDays(1).AddSeconds(-1));
                cmd.Parameters.AddWithValue("@StaffID", (object)staffId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@SearchCustomer", string.IsNullOrWhiteSpace(customerSearch) ? (object)DBNull.Value : "%" + customerSearch + "%");
                cmd.Parameters.AddWithValue("@PaymentMethod", (object)payMethod ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Status", (object)status ?? DBNull.Value);

                conn.Open();
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        result.Add(new SalesOrderListItem
                        {
                            InvoiceID = (int)rdr["InvoiceID"],
                            InvoiceCode = rdr["InvoiceCode"].ToString(),
                            InvoiceDate = (DateTime)rdr["InvoiceDate"],
                            CustomerName = rdr["CustomerName"].ToString(),
                            StaffName = rdr["StaffName"].ToString(),
                            TotalAmount = (decimal)rdr["TotalAmount"],
                            DiscountAmount = (decimal)rdr["DiscountAmount"],
                            PaymentMethodText = rdr["PaymentMethodText"].ToString(),
                            Status = Convert.ToInt32(rdr["Status"])
                        });
                    }
                }
            }
            return result;
        }

        public SalesOrderDetail GetOrderDetail(int invoiceId)
        {
            return GetOrderDetailInternal(invoiceId, null);
        }

        public SalesOrderDetail GetOrderDetailByCode(string code)
        {
            return GetOrderDetailInternal(0, code);
        }

        private SalesOrderDetail GetOrderDetailInternal(int invoiceId, string code)
        {
            SalesOrderDetail detail = null;
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(SalesSqlTemplate.GetOrderDetail, conn))
                {
                    cmd.Parameters.AddWithValue("@InvoiceID", invoiceId);
                    cmd.Parameters.AddWithValue("@InvoiceCode", (object)code ?? DBNull.Value);
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            detail = new SalesOrderDetail
                            {
                                InvoiceID = (int)rdr["InvoiceID"],
                                InvoiceCode = rdr["InvoiceCode"].ToString(),
                                InvoiceDate = (DateTime)rdr["InvoiceDate"],
                                CustomerName = rdr["CustomerName"].ToString(),
                                CustomerPhone = rdr["CustomerPhone"]?.ToString(),
                                StaffName = rdr["StaffName"].ToString(),
                                TotalAmount = (decimal)rdr["TotalAmount"],
                                DiscountAmount = (decimal)rdr["DiscountAmount"],
                                VoucherDiscount = (decimal)rdr["VoucherDiscount"],
                                LoyaltyPointsUsed = (int)rdr["LoyaltyPointsUsed"],
                                LoyaltyDiscount = (decimal)rdr["LoyaltyDiscount"],
                                FinalAmount = (decimal)rdr["PaidAmount"] + (decimal)rdr["ChangeAmount"], // Approx
                                PaidAmount = (decimal)rdr["PaidAmount"],
                                ChangeAmount = (decimal)rdr["ChangeAmount"],
                                PaymentMethod = (byte)rdr["PaymentMethod"],
                                LoyaltyPointsEarned = (int)rdr["LoyaltyPointsEarned"],
                                Status = Convert.ToInt32(rdr["Status"]),
                                Notes = rdr["Notes"]?.ToString(),
                                Items = new List<SalesOrderItem>()
                            };
                        }
                    }
                }

                if (detail != null)
                {
                    using (SqlCommand cmd = new SqlCommand(SalesSqlTemplate.GetOrderItems, conn))
                    {
                        cmd.Parameters.AddWithValue("@InvoiceID", invoiceId);
                        using (SqlDataReader rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                detail.Items.Add(new SalesOrderItem
                                {
                                    ProductID = (int)rdr["ProductID"],
                                    ProductCode = rdr["ProductCode"].ToString(),
                                    ProductName = rdr["ProductName"].ToString(),
                                    UnitPrice = (decimal)rdr["UnitPrice"],
                                    Quantity = (decimal)rdr["Quantity"],
                                    UnitName = rdr["UnitName"]?.ToString()
                                });
                            }
                        }
                    }
                }
            }
            return detail;
        }

        public void CancelOrder(int invoiceId, string reason)
        {
            var detail = GetOrderDetail(invoiceId);
            if (detail == null || detail.Status == 2) return;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. Update Status
                        using (SqlCommand cmd = new SqlCommand(SalesSqlTemplate.CancelInvoice, conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@InvoiceID", invoiceId);
                            cmd.Parameters.AddWithValue("@Reason", reason);
                            cmd.ExecuteNonQuery();
                        }

                        // 2. Revert Stock
                        foreach (var item in detail.Items)
                        {
                            using (SqlCommand cmd = new SqlCommand(SalesSqlTemplate.RevertStock, conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@ProductID", item.ProductID);
                                cmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                                cmd.Parameters.AddWithValue("@WarehouseID", 1); // Default
                                cmd.ExecuteNonQuery();
                            }
                        }

                        // 3. Revert Loyalty & TotalSpent
                        // Need CustomerID which is not in detail yet, we'll need to fetch it
                        int customerId = 0;
                        using (SqlCommand cmd = new SqlCommand("SELECT i.CustomerID FROM Invoices i INNER JOIN dbo.Users u ON i.CashierUserID = u.UserID WHERE i.InvoiceID = @InvoiceID OR i.InvoiceCode = @InvoiceCode;", conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@InvoiceID", invoiceId);
                            cmd.Parameters.AddWithValue("@InvoiceCode", (object)DBNull.Value);
                            var val = cmd.ExecuteScalar();
                            if (val != null && val != DBNull.Value) customerId = (int)val;
                        }

                        if (customerId > 0)
                        {
                            using (SqlCommand cmd = new SqlCommand(SalesSqlTemplate.RevertCustomerLoyalty, conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@CustomerID", customerId);
                                cmd.Parameters.AddWithValue("@PointsEarned", detail.LoyaltyPointsEarned);
                                cmd.Parameters.AddWithValue("@PointsUsed", detail.LoyaltyPointsUsed);
                                cmd.Parameters.AddWithValue("@Amount", detail.PaidAmount); // Should be total spent amount
                                cmd.ExecuteNonQuery();
                            }
                        }

                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }

        public Dictionary<int, string> GetUsers()
        {
            var result = new Dictionary<int, string>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(SalesSqlTemplate.GetUsers, conn))
            {
                conn.Open();
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        result.Add((int)rdr["UserID"], rdr["FullName"].ToString());
                    }
                }
            }
            return result;
        }
    }
}
