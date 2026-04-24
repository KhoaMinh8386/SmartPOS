using System;
using System.Collections.Generic;

namespace SmartPos.Module.Products.Models
{
    public class CategoryListItem
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
    }

    public class ProductListItem
    {
        public int ProductID { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string CategoryName { get; set; }
        public decimal RetailPrice { get; set; }
        public string Location { get; set; }
        public decimal StockQuantity { get; set; }
        public string BatchNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsActive { get; set; }
    }

    public class ProductDetail
    {
        public int ProductID { get; set; }
        public int CategoryID { get; set; }
        public int? SupplierID { get; set; }
        public int BaseUnitID { get; set; }
        public string ProductCode { get; set; }
        public string Barcode { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public decimal CostPrice { get; set; }
        public decimal RetailPrice { get; set; }
        public decimal? WholesalePrice { get; set; }
        public decimal? Weight { get; set; }
        public string Location { get; set; }
        public string UnitName { get; set; }
        public bool IsActive { get; set; }
        public bool HasExpiry { get; set; }
    }

    public class SupplierLookupItem
    {
        public int SupplierID { get; set; }
        public string SupplierName { get; set; }
    }

    public class UnitLookupItem
    {
        public int UnitID { get; set; }
        public string UnitName { get; set; }
    }

    public class ProductModuleState
    {
        public List<CategoryListItem> Categories { get; set; }
        public List<ProductListItem> Products { get; set; }
    }
}
