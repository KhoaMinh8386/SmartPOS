using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using SmartPos.Module.Reports.Models;
using SmartPos.Module.Reports.Templates;

namespace SmartPos.Module.Reports.Backend
{
    public class ReportBackend
    {
        private readonly string _connectionString;

        public ReportBackend()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["SmartPosDb"]?.ConnectionString;
        }

        public KpiData GetDashboardKpis()
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(ReportSqlTemplate.GetDashboardKpis, conn))
            {
                conn.Open();
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    if (rdr.Read())
                    {
                        return new KpiData
                        {
                            TodayRevenue = (decimal)rdr["TodayRevenue"],
                            MonthRevenue = (decimal)rdr["MonthRevenue"],
                            TodayOrders = (int)rdr["TodayOrders"],
                            TodayProfit = (decimal)rdr["TodayProfit"],
                            LowStockCount = (int)rdr["LowStockCount"],
                            NearExpiryCount = (int)rdr["NearExpiryCount"]
                        };
                    }
                }
            }
            return new KpiData();
        }

        public List<ChartDataPoint> GetRevenueChart(int days)
        {
            return GetChartData(ReportSqlTemplate.GetRevenueChart, new SqlParameter("@Days", days));
        }

        public List<ChartDataPoint> GetTopProducts()
        {
            return GetChartData(ReportSqlTemplate.GetTopProducts);
        }

        public List<ChartDataPoint> GetPaymentMethods()
        {
            return GetChartData(ReportSqlTemplate.GetPaymentMethods);
        }

        private List<ChartDataPoint> GetChartData(string sql, params SqlParameter[] parameters)
        {
            var result = new List<ChartDataPoint>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                conn.Open();
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        result.Add(new ChartDataPoint
                        {
                            Label = rdr["Label"].ToString(),
                            Value = (decimal)rdr["Value"]
                        });
                    }
                }
            }
            return result;
        }

        public DataTable GetRecentInvoices()
        {
            return GetDataTable(ReportSqlTemplate.GetRecentInvoices);
        }

        public DataTable GetLowStockAlert()
        {
            return GetDataTable(ReportSqlTemplate.GetLowStockAlert);
        }

        public List<ProductReportItem> GetProductPerformance(DateTime from, DateTime to)
        {
            var result = new List<ProductReportItem>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(ReportSqlTemplate.GetProductPerformance, conn))
            {
                cmd.Parameters.AddWithValue("@FromDate", from.Date);
                cmd.Parameters.AddWithValue("@ToDate", to.Date.AddDays(1).AddSeconds(-1));

                conn.Open();
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        result.Add(new ProductReportItem
                        {
                            ProductID = (int)rdr["ProductID"],
                            ProductCode = rdr["ProductCode"].ToString(),
                            ProductName = rdr["ProductName"].ToString(),
                            SoldQuantity = (decimal)rdr["SoldQuantity"],
                            Revenue = (decimal)rdr["Revenue"],
                            CurrentStock = (decimal)rdr["CurrentStock"],
                            CostPrice = (decimal)rdr["CostPrice"],
                            MinStockAlert = (int)rdr["MinStockAlert"]
                        });
                    }
                }
            }
            return result;
        }

        public List<ProductReportItem> GetNearExpiryItems(int days)
        {
            var result = new List<ProductReportItem>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(ReportSqlTemplate.GetNearExpiryItems, conn))
            {
                cmd.Parameters.AddWithValue("@Days", days);
                conn.Open();
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        result.Add(new ProductReportItem
                        {
                            ProductID = (int)rdr["ProductID"],
                            ProductCode = rdr["ProductCode"].ToString(),
                            ProductName = rdr["ProductName"].ToString(),
                            CurrentStock = (decimal)rdr["CurrentStock"],
                            ExpiryDate = rdr["ExpiryDate"] as DateTime?
                        });
                    }
                }
            }
            return result;
        }

        public List<CustomerReportItem> GetCustomerReport()
        {
            var result = new List<CustomerReportItem>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(ReportSqlTemplate.GetCustomerReport, conn))
            {
                conn.Open();
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        result.Add(new CustomerReportItem
                        {
                            CustomerID = (int)rdr["CustomerID"],
                            FullName = rdr["FullName"].ToString(),
                            Rank = rdr["Rank"]?.ToString() ?? "Bronze",
                            TotalSpent = (decimal)rdr["TotalSpent"],
                            LoyaltyPoints = (int)rdr["LoyaltyPoints"],
                            OrderCount = (int)rdr["OrderCount"]
                        });
                    }
                }
            }
            return result;
        }

        public List<ProfitReportItem> GetProfitReport(DateTime from, DateTime to)
        {
            var result = new List<ProfitReportItem>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(ReportSqlTemplate.GetProfitReport, conn))
            {
                cmd.Parameters.AddWithValue("@FromDate", from.Date);
                cmd.Parameters.AddWithValue("@ToDate", to.Date.AddDays(1).AddSeconds(-1));

                conn.Open();
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        result.Add(new ProfitReportItem
                        {
                            CategoryName = rdr["CategoryName"].ToString(),
                            Revenue = (decimal)rdr["Revenue"],
                            Cost = (decimal)rdr["Cost"]
                        });
                    }
                }
            }
            return result;
        }

        private DataTable GetDataTable(string sql)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlDataAdapter adapter = new SqlDataAdapter(sql, conn))
            {
                adapter.Fill(dt);
            }
            return dt;
        }

        // ─── BATCHES ───────────────────────────────────────────────────────────

        /// <summary>
        /// Lấy toàn bộ lô từ Inventory, filter theo warehouseID nếu > 0.
        /// Tính DaysToExpiry và join với Products, Warehouses.
        /// </summary>
        public List<BatchReportItem> GetAllBatches(int warehouseID = 0)
        {
            var result = new List<BatchReportItem>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(ReportSqlTemplate.GetAllBatches, conn))
            {
                cmd.Parameters.AddWithValue("@WarehouseID", warehouseID);

                conn.Open();
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        result.Add(new BatchReportItem
                        {
                            ProductID = (int)rdr["ProductID"],
                            ProductCode = rdr["ProductCode"]?.ToString(),
                            ProductName = rdr["ProductName"]?.ToString(),
                            BatchNumber = rdr["BatchNumber"]?.ToString(),
                            ManufactureDate = rdr["ManufactureDate"] as DateTime?,
                            ExpiryDate = rdr["ExpiryDate"] as DateTime?,
                            Quantity = (decimal)rdr["Quantity"],
                            ShelfLocation = rdr["ShelfLocation"]?.ToString(),
                            WarehouseName = rdr["WarehouseName"]?.ToString(),
                            DaysToExpiry = (int)rdr["DaysToExpiry"]
                        });
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Lấy toàn bộ lô của 1 sản phẩm cụ thể.
        /// </summary>
        public List<BatchReportItem> GetBatchesByProduct(int productID)
        {
            var result = new List<BatchReportItem>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(ReportSqlTemplate.GetBatchesByProduct, conn))
            {
                cmd.Parameters.AddWithValue("@ProductID", productID);

                conn.Open();
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        result.Add(new BatchReportItem
                        {
                            ProductID = (int)rdr["ProductID"],
                            ProductCode = rdr["ProductCode"]?.ToString(),
                            ProductName = rdr["ProductName"]?.ToString(),
                            BatchNumber = rdr["BatchNumber"]?.ToString(),
                            ManufactureDate = rdr["ManufactureDate"] as DateTime?,
                            ExpiryDate = rdr["ExpiryDate"] as DateTime?,
                            Quantity = (decimal)rdr["Quantity"],
                            ShelfLocation = rdr["ShelfLocation"]?.ToString(),
                            WarehouseName = rdr["WarehouseName"]?.ToString(),
                            DaysToExpiry = (int)rdr["DaysToExpiry"]
                        });
                    }
                }
            }
            return result;
        }
    }
}
