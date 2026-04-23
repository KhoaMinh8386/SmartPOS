using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
namespace SmartPos.Module.Pos
{
    public class InvoiceService
    {
        private readonly string _connectionString;

        public InvoiceService()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["SmartPosDb"].ConnectionString;
        }

        public StoreConfig GetStoreConfig()
        {
            // In real app, this would come from a Settings table
            return new StoreConfig
            {
                StoreName = "SMART POS SUPERMARKET",
                Address = "123 Ly Thuong Kiet, Q.10, TP.HCM",
                Phone = "0900.123.456",
                FooterMessage = "Cảm ơn quý khách! Hẹn gặp lại!",
                LoyaltyPointRate = 0.001m, // 1000đ = 1đ
                PointRedeemRate = 1m // 1đ = 1đ
            };
        }

        public string GenerateInvoiceCode()
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(PosSqlTemplate.GetNextInvoiceCode, conn))
            {
                conn.Open();
                int nextId = (int)cmd.ExecuteScalar();
                return "HD" + DateTime.Now.ToString("yyMMdd") + nextId.ToString("D4");
            }
        }

        public InvoiceDetail GetInvoiceDetail(int invoiceId)
        {
            InvoiceDetail detail = null;
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"
SELECT i.*, u.FullName as StaffName, c.CustomerName, c.Phone
FROM dbo.Invoices i
LEFT JOIN dbo.Users u ON i.CashierUserID = u.UserID
LEFT JOIN dbo.Customers c ON i.CustomerID = c.CustomerID
WHERE i.InvoiceID = @ID", conn))
                {
                    cmd.Parameters.AddWithValue("@ID", invoiceId);
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            detail = new InvoiceDetail
                            {
                                InvoiceID = (int)rdr["InvoiceID"],
                                InvoiceCode = rdr["InvoiceCode"].ToString(),
                                InvoiceDate = (DateTime)rdr["InvoiceDate"],
                                StaffName = rdr["StaffName"].ToString(),
                                CustomerName = rdr["CustomerName"]?.ToString(),
                                Phone = rdr["Phone"]?.ToString(),
                                SubTotal = (decimal)rdr["SubTotal"],
                                TotalAmount = (decimal)rdr["TotalAmount"],
                                VoucherDiscount = (decimal)rdr["VoucherDiscount"],
                                PointsDiscount = (decimal)rdr["PointsDiscount"],
                                UsedPoints = (int)rdr["UsedPoints"],
                                EarnedPoints = (int)rdr["EarnedPoints"],
                                PaidAmount = (decimal)rdr["PaidAmount"],
                                ChangeAmount = (decimal)rdr["PaidAmount"] - (decimal)rdr["TotalAmount"],
                                PaymentMethod = (byte)rdr["PaymentMethod"],
                                Items = new List<CartItem>()
                            };
                        }
                    }
                }

                if (detail != null)
                {
                    using (SqlCommand cmd = new SqlCommand(PosSqlTemplate.GetInvoiceItems, conn))
                    {
                        cmd.Parameters.AddWithValue("@InvoiceID", invoiceId);
                        using (SqlDataReader rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                detail.Items.Add(new CartItem
                                {
                                    ProductID = (int)rdr["ProductID"],
                                    ProductCode = rdr["ProductCode"].ToString(),
                                    ProductName = rdr["ProductName"].ToString(),
                                    UnitPrice = (decimal)rdr["UnitPrice"],
                                    Quantity = (decimal)rdr["Quantity"],
                                    UnitName = rdr["UnitName"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            return detail;
        }
    }
}
