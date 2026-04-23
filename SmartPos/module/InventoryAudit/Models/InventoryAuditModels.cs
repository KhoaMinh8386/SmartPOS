using System;
using System.Collections.Generic;

namespace SmartPos.Module.InventoryAudit.Models
{
    public class WarehouseOption
    {
        public int WarehouseID { get; set; }
        public string WarehouseName { get; set; }

        public override string ToString()
        {
            return WarehouseName;
        }
    }

    public class StockBatchItem
    {
        public int ProductID { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string BatchNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public decimal SystemQuantity { get; set; }
    }

    public class InventoryCheckItemEdit
    {
        public int CheckID { get; set; }
        public int ProductID { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string BatchNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public decimal SystemQuantity { get; set; }
        public decimal? ActualQuantity { get; set; }
        public string Reason { get; set; }

        public decimal Difference
        {
            get { return ActualQuantity.HasValue ? ActualQuantity.Value - SystemQuantity : 0m; }
        }
    }

    public class InventoryCheckSummary
    {
        public int CheckID { get; set; }
        public string CheckCode { get; set; }
        public DateTime CheckDate { get; set; }
        public int Status { get; set; }
        public string StatusText { get; set; }
        public string CreatedByName { get; set; }
        public string ApprovedByName { get; set; }
        public DateTime? ApprovedAt { get; set; }
    }

    public class InventoryCheckHeader
    {
        public int CheckID { get; set; }
        public string CheckCode { get; set; }
        public int WarehouseID { get; set; }
        public string WarehouseName { get; set; }
        public DateTime CheckDate { get; set; }
        public int Status { get; set; }
        public string StatusText { get; set; }
        public string Notes { get; set; }
        public string CreatedByName { get; set; }
        public string ApprovedByName { get; set; }
        public DateTime? ApprovedAt { get; set; }
    }

    public class InventoryCheckItemHistory
    {
        public long HistoryID { get; set; }
        public int CheckID { get; set; }
        public int ProductID { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string BatchNumber { get; set; }
        public decimal? OldActualQuantity { get; set; }
        public decimal? NewActualQuantity { get; set; }
        public string OldReason { get; set; }
        public string NewReason { get; set; }
        public int? ChangedByUserID { get; set; }
        public string ChangedByName { get; set; }
        public DateTime ChangedAt { get; set; }
    }

    public class InventoryCheckDraft
    {
        public int CheckID { get; set; }
        public string CheckCode { get; set; }
    }

    public class ApproveInventoryCheckRequest
    {
        public int CheckID { get; set; }
        public int ApprovedByUserID { get; set; }
        public List<InventoryCheckItemEdit> Items { get; set; }
    }
}
