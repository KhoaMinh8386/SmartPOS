using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
namespace SmartPos.Module.Pos
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
            using (SqlCommand cmd = new SqlCommand(PosSqlTemplate.FindProduct, conn))
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
                            UnitPrice = (decimal)rdr["FinalPrice"],
                            UnitID = (int)rdr["BaseUnitID"],
                            UnitName = rdr["UnitName"]?.ToString() ?? "Cai",
                            Quantity = 1,
                            ImageUrl = rdr["ImageUrl"]?.ToString(),
                            TotalStock = rdr["TotalStock"] != DBNull.Value ? Convert.ToDecimal(rdr["TotalStock"]) : 0,
                            BatchNumber = rdr["BatchNumber"]?.ToString(),
                            ExpiryDate = rdr["ExpiryDate"] != DBNull.Value ? (DateTime?)rdr["ExpiryDate"] : null
                        });
                    }
                }
            }
            return result;
        }

        public List<CustomerInfo> FindCustomers(string term)
        {
            var result = new List<CustomerInfo>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(PosSqlTemplate.FindCustomerByPhone, conn))
            {
                cmd.Parameters.AddWithValue("@Term", "%" + term + "%");
                conn.Open();
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        result.Add(new CustomerInfo
                        {
                            CustomerID = (int)rdr["CustomerID"],
                            FullName = rdr["FullName"].ToString(),
                            Phone = rdr["Phone"].ToString(),
                            Address = rdr["Address"]?.ToString(),
                            TotalPoints = (int)rdr["TotalPoints"]
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
                cmd.Parameters.AddWithValue("@Term", phone); // Vẫn giữ tìm chính xác nếu gọi hàm này
                conn.Open();
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    if (rdr.Read())
                    {
                        return new CustomerInfo
                        {
                            CustomerID = (int)rdr["CustomerID"],
                            FullName = rdr["FullName"].ToString(),
                            Phone = rdr["Phone"].ToString(),
                            Address = rdr["Address"]?.ToString(),
                            TotalPoints = (int)rdr["TotalPoints"]
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

        public int Checkout(CheckoutRequest request)
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
                            cmdInv.Parameters.AddWithValue("@SubTotal", request.SubTotal);
                            cmdInv.Parameters.AddWithValue("@TotalAmount", request.TotalAmount);
                            cmdInv.Parameters.AddWithValue("@VoucherDiscount", request.VoucherDiscount);
                            cmdInv.Parameters.AddWithValue("@PointsDiscount", request.PointsDiscount);
                            cmdInv.Parameters.AddWithValue("@UsedPoints", request.UsedPoints);
                            cmdInv.Parameters.AddWithValue("@EarnedPoints", request.EarnedPoints);
                            cmdInv.Parameters.AddWithValue("@VoucherCode", request.VoucherCode ?? (object)DBNull.Value);
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
                        return invoiceId;
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
                            FullName = rdr["FullName"].ToString(),
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
                                FullName = rdr["FullName"].ToString(),
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
        public VoucherInfo GetVoucher(string code)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(@"
SELECT VoucherID, VoucherCode, DiscountType, DiscountValue, MinOrderValue, MaxDiscount, AllowStackDiscount
FROM dbo.Vouchers
WHERE VoucherCode = @Code AND IsActive = 1 AND (GETDATE() >= StartDate AND GETDATE() <= EndDate)", conn))
            {
                cmd.Parameters.AddWithValue("@Code", code);
                conn.Open();
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    if (rdr.Read())
                    {
                        return new VoucherInfo
                        {
                            VoucherID = (int)rdr["VoucherID"],
                            VoucherCode = rdr["VoucherCode"].ToString(),
                            DiscountType = (byte)rdr["DiscountType"],
                            DiscountValue = (decimal)rdr["DiscountValue"],
                            MinOrderValue = (decimal)rdr["MinOrderValue"],
                            MaxDiscount = rdr["MaxDiscount"] as decimal?,
                            AllowStackDiscount = (bool)rdr["AllowStackDiscount"]
                        };
                    }
                }
            }
            return null;
        }
    }
}
