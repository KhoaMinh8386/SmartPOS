using System;
using System.Collections.Generic;

namespace SmartPos.Module.XuatHang.Models
{
    public class StockOutHeader
    {
        public int StockOutID { get; set; }
        public string StockOutCode { get; set; }
        public DateTime StockOutDate { get; set; }
        public int WarehouseID { get; set; }
        public string Reason { get; set; } // Hư hỏng / Hết hạn / Điều chuyển / Khác
        public string Notes { get; set; }
        public int? CreatedByUserID { get; set; }
        public string CreatedByName { get; set; }
        public decimal TotalQuantity { get; set; }
    }

    public class StockOutDetail
    {
        public int DetailID { get; set; }
        public int StockOutID { get; set; }
        public int ProductID { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string UnitName { get; set; }
        public string BatchNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public decimal Quantity { get; set; }
        public decimal AvailableQuantity { get; set; }
    }

    public class StockOutRequest
    {
        public int WarehouseID { get; set; }
        public string Reason { get; set; }
        public string Notes { get; set; }
        public int? CreatedByUserID { get; set; }
        public List<StockOutDetailRequest> Details { get; set; }
    }

    public class StockOutDetailRequest
    {
        public int ProductID { get; set; }
        public string BatchNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public decimal Quantity { get; set; }
    }

    public class ProductInventoryItem
    {
        public int ProductID { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string UnitName { get; set; }
        public string BatchNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public decimal Quantity { get; set; }
    }
}
