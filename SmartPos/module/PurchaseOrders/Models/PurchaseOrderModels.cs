using System;
using System.Collections.Generic;

namespace SmartPos.Module.PurchaseOrders.Models
{
    public class SupplierOption
    {
        public int SupplierID { get; set; }
        public string SupplierName { get; set; }

        public override string ToString()
        {
            return SupplierName;
        }
    }

    public class UserOption
    {
        public int UserID { get; set; }
        public string FullName { get; set; }

        public override string ToString()
        {
            return FullName;
        }
    }

    public class WarehouseOption
    {
        public int WarehouseID { get; set; }
        public string WarehouseName { get; set; }

        public override string ToString()
        {
            return WarehouseName;
        }
    }

    public class ProductOption
    {
        public int ProductID { get; set; }
        public int BaseUnitID { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }

        public string DisplayText
        {
            get { return ProductCode + " - " + ProductName; }
        }

        public override string ToString()
        {
            return DisplayText;
        }
    }

    public class PurchaseOrderDraftItem
    {
        public int ProductID { get; set; }
        public int UnitID { get; set; }
        public string ProductDisplay { get; set; }
        public string BatchNumber { get; set; }
        public string ShelfLocation { get; set; }
        public DateTime? ManufactureDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public decimal Quantity { get; set; }
        public decimal CostPrice { get; set; }

        public decimal LineTotal
        {
            get { return Quantity * CostPrice; }
        }
    }

    public class CreatePurchaseOrderRequest
    {
        public int SupplierID { get; set; }
        public int WarehouseID { get; set; }
        public int CreatedByUserID { get; set; }
        public DateTime OrderDate { get; set; }
        public byte PaymentStatus { get; set; }
        public string Notes { get; set; }
        public List<PurchaseOrderDraftItem> Items { get; set; }
    }

    public class FefoBatchItem
    {
        public int InventoryID { get; set; }
        public int ProductID { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string BatchNumber { get; set; }
        public string ShelfLocation { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public decimal Quantity { get; set; }
        public string WarehouseName { get; set; }
    }

    public class PurchaseOrderModuleData
    {
        public List<SupplierOption> Suppliers { get; set; }
        public List<UserOption> Users { get; set; }
        public List<WarehouseOption> Warehouses { get; set; }
        public List<ProductOption> Products { get; set; }
    }
}
