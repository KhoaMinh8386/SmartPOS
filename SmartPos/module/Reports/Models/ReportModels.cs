using System;
using System.Collections.Generic;

namespace SmartPos.Module.Reports.Models
{
    public class KpiData
    {
        public decimal TodayRevenue { get; set; }
        public decimal MonthRevenue { get; set; }
        public int TodayOrders { get; set; }
        public decimal TodayProfit { get; set; }
        public int LowStockCount { get; set; }
        public int NearExpiryCount { get; set; }
    }

    public class ChartDataPoint
    {
        public string Label { get; set; }
        public decimal Value { get; set; }
    }

    public class RevenueReportItem
    {
        public string Period { get; set; }
        public decimal Revenue { get; set; }
        public decimal Cost { get; set; }
        public decimal Profit => Revenue - Cost;
        public int OrderCount { get; set; }
    }

    public class ProductReportItem
    {
        public int ProductID { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public decimal SoldQuantity { get; set; }
        public decimal Revenue { get; set; }
        public decimal CurrentStock { get; set; }
        public decimal StockValue => CurrentStock * CostPrice;
        public decimal CostPrice { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int MinStockAlert { get; set; }
    }

    public class CustomerReportItem
    {
        public int CustomerID { get; set; }
        public string FullName { get; set; }
        public string Rank { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalSpent { get; set; }
        public int LoyaltyPoints { get; set; }
    }

    public class ProfitReportItem
    {
        public string CategoryName { get; set; }
        public decimal Revenue { get; set; }
        public decimal Cost { get; set; }
        public decimal Profit => Revenue - Cost;
    }

    public class BatchReportItem
    {
        public int ProductID { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string BatchNumber { get; set; }
        public DateTime? ManufactureDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public decimal Quantity { get; set; }
        public string ShelfLocation { get; set; }
        public string WarehouseName { get; set; }
        public int DaysToExpiry { get; set; }
    }
}
