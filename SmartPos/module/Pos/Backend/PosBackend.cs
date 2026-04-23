using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using SmartPos.Module.Pos.Models;
using SmartPos.Module.Pos.Templates;

namespace SmartPos.Module.Pos.Backend
{
    public class PosBackend
    {
        private readonly string _connectionString;

        public PosBackend()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["SmartPosDb"]?.ConnectionString;
        }

        public List<CartItem> FindProducts(string term)
        {
            var result = new List<CartItem>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(@"
SELECT TOP 10 
    p.ProductID, p.ProductCode, p.ProductName, p.RetailPrice, p.BaseUnitID, u.UnitName
FROM dbo.Products p
LEFT JOIN dbo.Units u ON p.BaseUnitID = u.UnitID
WHERE p.IsActive = 1 
  AND (p.ProductCode = @Term OR p.Barcode = @Term OR p.ProductName LIKE @SearchTerm)
ORDER BY p.ProductName;", conn))
            {
                cmd.Parameters.AddWithValue("@Term", term);
                cmd.Parameters.AddWithValue("@SearchTerm", "%" + term + "%");
                conn.Open();
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        result.Add(new CartItem
                        {
                            ProductID = (int)rdr["ProductID"],
                            ProductCode = rdr["ProductCode"].ToString(),
                            ProductName = rdr["ProductName"].ToString(),
                            UnitPrice = (decimal)rdr["RetailPrice"],
                            UnitID = (int)rdr["BaseUnitID"],
                            UnitName = rdr["UnitName"]?.ToString() ?? "Cai",
                            Quantity = 1
                        });
                    }
                }
            }
            return result;
        }

        public CustomerInfo FindCustomerByPhone(string phone)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(PosSqlTemplate.FindCustomerByPhone, conn))
            {
                cmd.Parameters.AddWithValue("@Phone", phone);
                conn.Open();
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    if (rdr.Read())
                    {
                        return new CustomerInfo
                        {
                            CustomerID = (int)rdr["CustomerID"],
                            CustomerName = rdr["CustomerName"].ToString(),
                            Phone = rdr["Phone"].ToString(),
                            Address = rdr["Address"]?.ToString(),
                            Points = (int)rdr["Points"]
                        };
                    }
                }
            }
            return null;
        }

        public int CreateCustomer(string name, string phone, string address)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(PosSqlTemplate.InsertCustomer, conn))
            {
                cmd.Parameters.AddWithValue("@Name", name);
                cmd.Parameters.AddWithValue("@Phone", phone);
                cmd.Parameters.AddWithValue("@Address", address ?? (object)DBNull.Value);
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public string Checkout(CheckoutRequest request)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        // Generate Invoice Code
                        int nextId;
                        using (SqlCommand cmdId = new SqlCommand(PosSqlTemplate.GetNextInvoiceCode, conn, trans))
                        {
                            nextId = Convert.ToInt32(cmdId.ExecuteScalar());
                        }
                        string invoiceCode = "HD" + DateTime.Now.ToString("yyMMdd") + nextId.ToString("D4");

                        // Insert Invoice
                        int invoiceId;
                        using (SqlCommand cmdInv = new SqlCommand(PosSqlTemplate.InsertInvoice, conn, trans))
                        {
                            cmdInv.Parameters.AddWithValue("@InvoiceCode", invoiceCode);
                            cmdInv.Parameters.AddWithValue("@CustomerID", (object)request.CustomerID ?? DBNull.Value);
                            cmdInv.Parameters.AddWithValue("@UserID", request.UserID);
                            cmdInv.Parameters.AddWithValue("@WarehouseID", 1); // Default to main warehouse
                            cmdInv.Parameters.AddWithValue("@TotalAmount", request.TotalAmount);
                            cmdInv.Parameters.AddWithValue("@PaidAmount", request.PaidAmount);
                            cmdInv.Parameters.AddWithValue("@PaymentMethod", request.PaymentMethod);
                            cmdInv.Parameters.AddWithValue("@Note", request.Note ?? (object)DBNull.Value);
                            invoiceId = Convert.ToInt32(cmdInv.ExecuteScalar());
                        }

                        // Insert Items
                        foreach (var item in request.Items)
                        {
                            using (SqlCommand cmdItem = new SqlCommand(PosSqlTemplate.InsertInvoiceItem, conn, trans))
                            {
                                cmdItem.Parameters.AddWithValue("@InvoiceID", invoiceId);
                                cmdItem.Parameters.AddWithValue("@ProductID", item.ProductID);
                                cmdItem.Parameters.AddWithValue("@Quantity", item.Quantity);
                                cmdItem.Parameters.AddWithValue("@UnitPrice", item.UnitPrice);
                                cmdItem.Parameters.AddWithValue("@UnitID", item.UnitID);
                                cmdItem.ExecuteNonQuery();
                            }
                        }

                        trans.Commit();
                        return invoiceCode;
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }

        public List<InvoiceListItem> GetInvoices(string search)
        {
            var result = new List<InvoiceListItem>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(PosSqlTemplate.GetInvoices, conn))
            {
                cmd.Parameters.AddWithValue("@Search", string.IsNullOrWhiteSpace(search) ? (object)DBNull.Value : "%" + search + "%");
                conn.Open();
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        result.Add(new InvoiceListItem
                        {
                            InvoiceID = (int)rdr["InvoiceID"],
                            InvoiceCode = rdr["InvoiceCode"].ToString(),
                            InvoiceDate = (DateTime)rdr["InvoiceDate"],
                            CustomerName = rdr["CustomerName"].ToString(),
                            StaffName = rdr["StaffName"].ToString(),
                            TotalAmount = (decimal)rdr["TotalAmount"],
                            PaymentMethodText = rdr["PaymentMethodText"].ToString()
                        });
                    }
                }
            }
            return result;
        }

        public InvoiceDetail GetInvoiceDetail(int invoiceId)
        {
            InvoiceDetail detail = null;
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(PosSqlTemplate.GetInvoiceDetail, conn))
                {
                    cmd.Parameters.AddWithValue("@InvoiceID", invoiceId);
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            detail = new InvoiceDetail
                            {
                                InvoiceID = (int)rdr["InvoiceID"],
                                InvoiceCode = rdr["InvoiceCode"].ToString(),
                                InvoiceDate = (DateTime)rdr["InvoiceDate"],
                                CustomerName = rdr["CustomerName"].ToString(),
                                Phone = rdr["Phone"]?.ToString(),
                                StaffName = rdr["StaffName"].ToString(),
                                TotalAmount = (decimal)rdr["TotalAmount"],
                                PaidAmount = (decimal)rdr["PaidAmount"],
                                ChangeAmount = (decimal)rdr["ChangeAmount"],
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
                                    UnitName = rdr["UnitName"]?.ToString()
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
