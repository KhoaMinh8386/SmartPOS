using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using SmartPos.Module.LichSuKiemXuat.Models;
using SmartPos.Module.LichSuKiemXuat.Templates;

namespace SmartPos.Module.LichSuKiemXuat.Backend
{
    public class HistoryBackend
    {
        private readonly string _connectionString;

        public HistoryBackend()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["SmartPosDb"]?.ConnectionString;
            EnsureSchema();
        }

        private void EnsureSchema()
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(HistorySqlTemplate.EnsureSchema, conn))
            {
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // --- Lookups ---
        public List<UserLookup> GetUsers()
        {
            var result = new List<UserLookup>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(HistorySqlTemplate.GetUsersLookup, conn))
            {
                conn.Open();
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        result.Add(new UserLookup { UserID = (int)rdr["UserID"], FullName = rdr["FullName"].ToString() });
                    }
                }
            }
            return result;
        }

        public List<CategoryLookup> GetCategories()
        {
            var result = new List<CategoryLookup>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(HistorySqlTemplate.GetCategoriesLookup, conn))
            {
                conn.Open();
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        result.Add(new CategoryLookup { CategoryID = (int)rdr["CategoryID"], CategoryName = rdr["CategoryName"].ToString() });
                    }
                }
            }
            return result;
        }

        // --- Stock Out ---
        public List<StockOutHistoryListItem> GetStockOutHistory(DateTime from, DateTime to, string reason, int? userId, string search)
        {
            var result = new List<StockOutHistoryListItem>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(HistorySqlTemplate.GetStockOutHistory, conn))
            {
                cmd.Parameters.AddWithValue("@FromDate", from.Date);
                cmd.Parameters.AddWithValue("@ToDate", to.Date.AddDays(1).AddSeconds(-1));
                cmd.Parameters.AddWithValue("@Reason", reason ?? "Tất cả");
                cmd.Parameters.AddWithValue("@UserID", (object)userId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Search", string.IsNullOrWhiteSpace(search) ? (object)DBNull.Value : "%" + search + "%");

                conn.Open();
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        result.Add(new StockOutHistoryListItem
                        {
                            StockOutID = (int)rdr["StockOutID"],
                            StockOutCode = rdr["StockOutCode"].ToString(),
                            StockOutDate = (DateTime)rdr["StockOutDate"],
                            Reason = rdr["Reason"].ToString(),
                            WarehouseName = rdr["WarehouseName"].ToString(),
                            ItemCount = (int)rdr["ItemCount"],
                            CreatedByName = rdr["CreatedByName"].ToString(),
                            Notes = rdr["Notes"]?.ToString()
                        });
                    }
                }
            }
            return result;
        }

        public List<StockOutHistoryDetail> GetStockOutDetails(int stockOutId)
        {
            var result = new List<StockOutHistoryDetail>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(HistorySqlTemplate.GetStockOutDetails, conn))
            {
                cmd.Parameters.AddWithValue("@StockOutID", stockOutId);
                conn.Open();
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        result.Add(new StockOutHistoryDetail
                        {
                            ProductID = (int)rdr["ProductID"],
                            ProductCode = rdr["ProductCode"].ToString(),
                            ProductName = rdr["ProductName"].ToString(),
                            Barcode = rdr["Barcode"]?.ToString(),
                            UnitName = rdr["UnitName"]?.ToString(),
                            BatchNumber = rdr["BatchNumber"] == DBNull.Value ? null : rdr["BatchNumber"].ToString(),
                            ShelfLocation = rdr["ShelfLocation"] == DBNull.Value ? null : rdr["ShelfLocation"].ToString(),
                            ExpiryDate = rdr["ExpiryDate"] == DBNull.Value ? (DateTime?)null : (DateTime)rdr["ExpiryDate"],
                            Quantity = (decimal)rdr["Quantity"],
                            BaseQuantity = (decimal)rdr["BaseQuantity"],
                            StockBefore = (decimal)rdr["StockBefore"]
                        });
                    }
                }
            }
            return result;
        }

        public StockOutStats GetStockOutStats(DateTime from, DateTime to)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(HistorySqlTemplate.GetStockOutStats, conn))
            {
                cmd.Parameters.AddWithValue("@FromDate", from.Date);
                cmd.Parameters.AddWithValue("@ToDate", to.Date.AddDays(1).AddSeconds(-1));
                conn.Open();
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    if (rdr.Read())
                    {
                        return new StockOutStats
                        {
                            TotalVouchers = rdr["TotalVouchers"] == DBNull.Value ? 0 : (int)rdr["TotalVouchers"],
                            DamageCount = rdr["DamageCount"] == DBNull.Value ? 0 : (int)rdr["DamageCount"],
                            ExpiredCount = rdr["ExpiredCount"] == DBNull.Value ? 0 : (int)rdr["ExpiredCount"],
                            TransferCount = rdr["TransferCount"] == DBNull.Value ? 0 : (int)rdr["TransferCount"],
                            OtherCount = rdr["OtherCount"] == DBNull.Value ? 0 : (int)rdr["OtherCount"]
                        };
                    }
                }
            }
            return new StockOutStats();
        }

        // --- Audit ---
        public List<AuditHistoryListItem> GetAuditHistory(DateTime from, DateTime to, int? categoryId, int? userId, string search)
        {
            var result = new List<AuditHistoryListItem>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(HistorySqlTemplate.GetAuditHistory, conn))
            {
                cmd.Parameters.AddWithValue("@FromDate", from.Date);
                cmd.Parameters.AddWithValue("@ToDate", to.Date.AddDays(1).AddSeconds(-1));
                cmd.Parameters.AddWithValue("@UserID", (object)userId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Search", string.IsNullOrWhiteSpace(search) ? (object)DBNull.Value : "%" + search + "%");

                conn.Open();
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        result.Add(new AuditHistoryListItem
                        {
                            CheckID = (int)rdr["CheckID"],
                            CheckCode = rdr["CheckCode"].ToString(),
                            CheckDate = (DateTime)rdr["CheckDate"],
                            CategoryName = rdr["CategoryName"].ToString(),
                            AuditorName = rdr["AuditorName"].ToString(),
                            ApproverName = rdr["ApproverName"]?.ToString(),
                            ApprovedAt = rdr["ApprovedAt"] == DBNull.Value ? (DateTime?)null : (DateTime)rdr["ApprovedAt"],
                            TotalItems = (int)rdr["TotalItems"],
                            MatchCount = (int)rdr["MatchCount"],
                            OverCount = (int)rdr["OverCount"],
                            UnderCount = (int)rdr["UnderCount"]
                        });
                    }
                }
            }
            return result;
        }

        public List<AuditHistoryDetail> GetAuditDetails(int checkId)
        {
            var result = new List<AuditHistoryDetail>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(HistorySqlTemplate.GetAuditDetails, conn))
            {
                cmd.Parameters.AddWithValue("@CheckID", checkId);
                conn.Open();
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        result.Add(new AuditHistoryDetail
                        {
                            ProductID = (int)rdr["ProductID"],
                            ProductName = rdr["ProductName"].ToString(),
                            UnitName = rdr["UnitName"]?.ToString(),
                            SystemQuantity = (decimal)rdr["SystemQuantity"],
                            ActualQuantity = rdr["ActualQuantity"] == DBNull.Value ? 0 : (decimal)rdr["ActualQuantity"],
                            Difference = rdr["Difference"] == DBNull.Value ? 0 : (decimal)rdr["Difference"],
                            ResultText = rdr["ResultText"].ToString()
                        });
                    }
                }
            }
            return result;
        }
        // --- Purchase History ---
        public List<PurchaseHistoryListItem> GetPurchaseHistory(DateTime fromDate, DateTime toDate, int? userId, string search)
        {
            var list = new List<PurchaseHistoryListItem>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(HistorySqlTemplate.GetPurchaseHistory, conn))
            {
                cmd.Parameters.AddWithValue("@FromDate", fromDate.Date);
                cmd.Parameters.AddWithValue("@ToDate", toDate.Date.AddDays(1).AddSeconds(-1));
                cmd.Parameters.AddWithValue("@UserID", userId.HasValue ? (object)userId.Value : DBNull.Value);
                
                string searchPattern = string.IsNullOrWhiteSpace(search) ? null : $"%{search}%";
                cmd.Parameters.AddWithValue("@Search", searchPattern ?? (object)DBNull.Value);

                conn.Open();
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        list.Add(new PurchaseHistoryListItem
                        {
                            PurchaseOrderID = (int)rdr["PurchaseOrderID"],
                            POCode = rdr["POCode"].ToString(),
                            OrderDate = (DateTime)rdr["OrderDate"],
                            SupplierName = rdr["SupplierName"] == DBNull.Value ? "Khách vãng lai" : rdr["SupplierName"].ToString(),
                            WarehouseName = rdr["WarehouseName"].ToString(),
                            TotalAmount = (decimal)rdr["TotalAmount"],
                            Status = (byte)rdr["Status"],
                            PaymentStatus = (byte)rdr["PaymentStatus"],
                            CreatedByName = rdr["CreatedByName"]?.ToString(),
                            ItemCount = (int)rdr["ItemCount"]
                        });
                    }
                }
            }
            return list;
        }

        public List<PurchaseHistoryDetail> GetPurchaseDetails(int purchaseOrderId)
        {
            var list = new List<PurchaseHistoryDetail>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(HistorySqlTemplate.GetPurchaseDetails, conn))
            {
                cmd.Parameters.AddWithValue("@PurchaseOrderID", purchaseOrderId);
                conn.Open();
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        list.Add(new PurchaseHistoryDetail
                        {
                            ProductID = (int)rdr["ProductID"],
                            ProductCode = rdr["ProductCode"].ToString(),
                            ProductName = rdr["ProductName"].ToString(),
                            UnitName = rdr["UnitName"]?.ToString(),
                            Quantity = (decimal)rdr["Quantity"],
                            CostPrice = (decimal)rdr["CostPrice"],
                            BatchNumber = rdr["BatchNumber"] == DBNull.Value ? null : rdr["BatchNumber"].ToString(),
                            ShelfLocation = rdr["ShelfLocation"] == DBNull.Value ? null : rdr["ShelfLocation"].ToString(),
                            ExpiryDate = rdr["ExpiryDate"] == DBNull.Value ? (DateTime?)null : (DateTime)rdr["ExpiryDate"]
                        });
                    }
                }
            }
            return list;
        }

        public PurchaseStats GetPurchaseStats(DateTime fromDate, DateTime toDate)
        {
            var stats = new PurchaseStats();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(HistorySqlTemplate.GetPurchaseStats, conn))
            {
                cmd.Parameters.AddWithValue("@FromDate", fromDate.Date);
                cmd.Parameters.AddWithValue("@ToDate", toDate.Date.AddDays(1).AddSeconds(-1));
                conn.Open();
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    if (rdr.Read())
                    {
                        stats.TotalVouchers = (int)rdr["TotalVouchers"];
                        stats.TotalAmount = (decimal)rdr["TotalAmount"];
                    }
                }
            }
            return stats;
        }
    }
}
