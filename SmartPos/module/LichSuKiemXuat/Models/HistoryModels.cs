using System;
using System.Collections.Generic;

namespace SmartPos.Module.LichSuKiemXuat.Models
{
    // --- Stock Out Models ---
    public class StockOutHistoryListItem
    {
        public int StockOutID { get; set; }
        public string StockOutCode { get; set; }
        public DateTime StockOutDate { get; set; }
        public string Reason { get; set; }
        public string WarehouseName { get; set; }
        public int ItemCount { get; set; }
        public string CreatedByName { get; set; }
        public string Notes { get; set; }
        // For display logic in UI
        public string ReceiverName { get; set; } // Only for "Điều chuyển"
    }

    public class StockOutHistoryDetail
    {
        public int ProductID { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string Barcode { get; set; }
        public string UnitName { get; set; }
        public string BatchNumber { get; set; }
        public string ShelfLocation { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public decimal Quantity { get; set; }
        public decimal BaseQuantity { get; set; }
        public decimal StockBefore { get; set; }
        public string Note { get; set; }
    }

    public class StockOutStats
    {
        public int TotalVouchers { get; set; }
        public int DamageCount { get; set; }
        public int ExpiredCount { get; set; }
        public int TransferCount { get; set; }
        public int OtherCount { get; set; }
        public int TotalItems { get; set; }
    }

    // --- Inventory Audit Models ---
    public class AuditHistoryListItem
    {
        public int CheckID { get; set; }
        public string CheckCode { get; set; }
        public DateTime CheckDate { get; set; }
        public string CategoryName { get; set; }
        public string AuditorName { get; set; }
        public string ApproverName { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public int TotalItems { get; set; }
        public int MatchCount { get; set; }
        public int OverCount { get; set; }
        public int UnderCount { get; set; }
    }

    public class AuditHistoryDetail
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string UnitName { get; set; }
        public decimal SystemQuantity { get; set; }
        public decimal ActualQuantity { get; set; }
        public decimal Difference { get; set; }
        public string ResultText { get; set; } // Khớp, Thừa, Thiếu
    }

    public class AuditComparisonItem
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public decimal DiffPeriod1 { get; set; }
        public decimal DiffPeriod2 { get; set; }
        public string Trend { get; set; } // ↑, ↓, →
    }

    // --- Purchase History Models ---
    public class PurchaseHistoryListItem
    {
        public int PurchaseOrderID { get; set; }
        public string POCode { get; set; }
        public DateTime OrderDate { get; set; }
        public string SupplierName { get; set; }
        public string WarehouseName { get; set; }
        public decimal TotalAmount { get; set; }
        public int ItemCount { get; set; }
        public string CreatedByName { get; set; }
        public byte Status { get; set; }
        public byte PaymentStatus { get; set; }
        public string StatusText => Status == 1 ? "Nháp" : Status == 2 ? "Hoàn thành" : "Hủy";
        public string PaymentStatusText => PaymentStatus == 1 ? "Chưa TT" : PaymentStatus == 2 ? "TT 1 phần" : "Đã TT";
    }

    public class PurchaseHistoryDetail
    {
        public int ProductID { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string UnitName { get; set; }
        public string BatchNumber { get; set; }
        public string ShelfLocation { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public decimal Quantity { get; set; }
        public decimal CostPrice { get; set; }
        public decimal LineTotal => Quantity * CostPrice;
    }

    public class PurchaseStats
    {
        public int TotalVouchers { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class UserLookup
    {
        public int UserID { get; set; }
        public string FullName { get; set; }
        public override string ToString() => FullName;
    }

    public class CategoryLookup
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public override string ToString() => CategoryName;
    }
}
