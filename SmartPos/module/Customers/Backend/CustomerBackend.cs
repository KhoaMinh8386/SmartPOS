using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using SmartPos.Module.Customers.Models;
using SmartPos.Module.Customers.Templates;

namespace SmartPos.Module.Customers.Backend
{
    public class CustomerBackend
    {
        private readonly string _conn;

        public CustomerBackend()
        {
            _conn = ConfigurationManager.ConnectionStrings["SmartPosDb"]?.ConnectionString;
        }

        // ─── Schema ────────────────────────────────────────────────────────────
        public void EnsureSchema()
        {
            using (var con = new SqlConnection(_conn))
            using (var cmd = new SqlCommand(CustomerSqlTemplate.EnsureSchema, con))
            { con.Open(); cmd.ExecuteNonQuery(); }
        }

        // ─── List ──────────────────────────────────────────────────────────────
        public List<CustomerListItem> GetList(string search, string typeFilter)
        {
            var list = new List<CustomerListItem>();
            using (var con = new SqlConnection(_conn))
            using (var cmd = new SqlCommand(CustomerSqlTemplate.GetCustomerList, con))
            {
                cmd.Parameters.AddWithValue("@Search",
                    string.IsNullOrWhiteSpace(search) ? (object)DBNull.Value : search);
                cmd.Parameters.AddWithValue("@TypeFilter",
                    string.IsNullOrWhiteSpace(typeFilter) ? (object)DBNull.Value : typeFilter);
                con.Open();
                using (var rdr = cmd.ExecuteReader())
                    while (rdr.Read())
                        list.Add(new CustomerListItem
                        {
                            CustomerID   = (int)rdr["CustomerID"],
                            CustomerCode = rdr["CustomerCode"]?.ToString(),
                            FullName     = rdr["FullName"].ToString(),
                            Phone        = rdr["Phone"]?.ToString(),
                            Email        = rdr["Email"]?.ToString(),
                            TotalPoints  = (int)rdr["TotalPoints"],
                            TotalSpent   = (decimal)rdr["TotalSpent"],
                            CustomerType = rdr["CustomerType"]?.ToString(),
                            CreatedAt    = (DateTime)rdr["CreatedAt"]
                        });
            }
            return list;
        }

        // ─── Detail ────────────────────────────────────────────────────────────
        public CustomerDetail GetDetail(int id)
        {
            using (var con = new SqlConnection(_conn))
            using (var cmd = new SqlCommand(CustomerSqlTemplate.GetCustomerDetail, con))
            {
                cmd.Parameters.AddWithValue("@CustomerID", id);
                con.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    if (!rdr.Read()) return null;
                    return new CustomerDetail
                    {
                        CustomerID   = (int)rdr["CustomerID"],
                        CustomerCode = rdr["CustomerCode"]?.ToString(),
                        FullName     = rdr["FullName"].ToString(),
                        Phone        = rdr["Phone"]?.ToString(),
                        Email        = rdr["Email"]?.ToString(),
                        Address      = rdr["Address"]?.ToString(),
                        Gender       = rdr["Gender"]?.ToString(),
                        DateOfBirth  = rdr["DateOfBirth"] as DateTime?,
                        TotalPoints  = (int)rdr["TotalPoints"],
                        TotalSpent   = (decimal)rdr["TotalSpent"],
                        CustomerType = rdr["CustomerType"]?.ToString(),
                        Note         = rdr["Note"]?.ToString(),
                        CreatedAt    = (DateTime)rdr["CreatedAt"],
                        UpdatedAt    = rdr["UpdatedAt"] as DateTime?
                    };
                }
            }
        }

        // ─── Save (Insert / Update) ────────────────────────────────────────────
        public int Save(CustomerSaveRequest req)
        {
            if (req.CustomerID.HasValue)
            {
                using (var con = new SqlConnection(_conn))
                using (var cmd = new SqlCommand(CustomerSqlTemplate.UpdateCustomer, con))
                {
                    cmd.Parameters.AddWithValue("@CustomerID",  req.CustomerID.Value);
                    cmd.Parameters.AddWithValue("@FullName",    req.FullName);
                    cmd.Parameters.AddWithValue("@Phone",       (object)req.Phone       ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Email",       (object)req.Email       ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Address",     (object)req.Address     ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Gender",      (object)req.Gender      ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DateOfBirth", (object)req.DateOfBirth ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Note",        (object)req.Note        ?? DBNull.Value);
                    con.Open(); cmd.ExecuteNonQuery();
                    return req.CustomerID.Value;
                }
            }
            else
            {
                // Build auto code
                string code = GetNextCode();
                using (var con = new SqlConnection(_conn))
                using (var cmd = new SqlCommand(@"
INSERT INTO dbo.Customers (CustomerCode,FullName,Phone,Email,Address,Gender,DateOfBirth,Note,CreatedAt)
VALUES (@Code,@FullName,@Phone,@Email,@Address,@Gender,@DateOfBirth,@Note,GETDATE());
SELECT SCOPE_IDENTITY();", con))
                {
                    cmd.Parameters.AddWithValue("@Code",        code);
                    cmd.Parameters.AddWithValue("@FullName",    req.FullName);
                    cmd.Parameters.AddWithValue("@Phone",       (object)req.Phone       ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Email",       (object)req.Email       ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Address",     (object)req.Address     ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Gender",      (object)req.Gender      ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DateOfBirth", (object)req.DateOfBirth ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Note",        (object)req.Note        ?? DBNull.Value);
                    con.Open();
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        // ─── Delete ────────────────────────────────────────────────────────────
        public void Delete(int id)
        {
            using (var con = new SqlConnection(_conn))
            using (var cmd = new SqlCommand(CustomerSqlTemplate.DeleteCustomer, con))
            {
                cmd.Parameters.AddWithValue("@CustomerID", id);
                con.Open(); cmd.ExecuteNonQuery();
            }
        }

        // ─── Points ────────────────────────────────────────────────────────────
        public List<PointsHistoryItem> GetPointsHistory(int customerId)
        {
            var list = new List<PointsHistoryItem>();
            using (var con = new SqlConnection(_conn))
            using (var cmd = new SqlCommand(CustomerSqlTemplate.GetPointsHistory, con))
            {
                cmd.Parameters.AddWithValue("@CustomerID", customerId);
                con.Open();
                using (var rdr = cmd.ExecuteReader())
                    while (rdr.Read())
                        list.Add(new PointsHistoryItem
                        {
                            ID          = (int)rdr["ID"],
                            Points      = (int)rdr["Points"],
                            Type        = rdr["Type"]?.ToString(),
                            Description = rdr["Description"]?.ToString(),
                            CreatedAt   = (DateTime)rdr["CreatedAt"]
                        });
            }
            return list;
        }

        public void AdjustPoints(AdjustPointsRequest req)
        {
            using (var con = new SqlConnection(_conn))
            using (var cmd = new SqlCommand(CustomerSqlTemplate.AdjustPoints, con))
            {
                cmd.Parameters.AddWithValue("@CustomerID",  req.CustomerID);
                cmd.Parameters.AddWithValue("@Points",      req.Points);
                cmd.Parameters.AddWithValue("@Type",        req.Type);
                cmd.Parameters.AddWithValue("@Description", (object)req.Description ?? DBNull.Value);
                con.Open(); cmd.ExecuteNonQuery();
            }
        }

        // ─── Invoice History ───────────────────────────────────────────────────
        public List<CustomerInvoiceItem> GetInvoices(int customerId)
        {
            var list = new List<CustomerInvoiceItem>();
            using (var con = new SqlConnection(_conn))
            using (var cmd = new SqlCommand(CustomerSqlTemplate.GetCustomerInvoices, con))
            {
                cmd.Parameters.AddWithValue("@CustomerID", customerId);
                con.Open();
                using (var rdr = cmd.ExecuteReader())
                    while (rdr.Read())
                        list.Add(new CustomerInvoiceItem
                        {
                            InvoiceID         = (int)rdr["InvoiceID"],
                            InvoiceCode       = rdr["InvoiceCode"].ToString(),
                            InvoiceDate       = (DateTime)rdr["InvoiceDate"],
                            TotalAmount       = (decimal)rdr["TotalAmount"],
                            PaymentMethodText = rdr["PaymentMethodText"].ToString()
                        });
            }
            return list;
        }

        // ─── Helper ────────────────────────────────────────────────────────────
        private string GetNextCode()
        {
            using (var con = new SqlConnection(_conn))
            using (var cmd = new SqlCommand(CustomerSqlTemplate.GetNextCustomerCode, con))
            {
                con.Open();
                return cmd.ExecuteScalar()?.ToString() ?? "KH0001";
            }
        }
    }
}
