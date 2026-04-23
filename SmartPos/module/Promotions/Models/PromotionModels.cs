using System;
using System.Collections.Generic;

namespace SmartPos.Module.Promotions.Models
{
    public class VoucherItem
    {
        public int VoucherID { get; set; }
        public string VoucherCode { get; set; }
        public string Description { get; set; }
        public byte DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal MinOrderValue { get; set; }
        public decimal? MaxDiscount { get; set; }
        public bool AllowStackDiscount { get; set; }
        public int Priority { get; set; }
        public bool IsActive { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class ProductSaleItem
    {
        public int SaleID { get; set; }
        public int ProductID { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string SaleName { get; set; }
        public byte DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal? SalePrice { get; set; }
        public bool AllowStackVoucher { get; set; }
        public int Priority { get; set; }
        public bool IsActive { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class ProductOption
    {
        public int ProductID { get; set; }
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

    public class PromotionDataBundle
    {
        public List<VoucherItem> Vouchers { get; set; }
        public List<ProductSaleItem> ProductSales { get; set; }
        public List<ProductOption> Products { get; set; }
    }

    public class PromotionPreviewRequest
    {
        public decimal OrderAmount { get; set; }
        public decimal ProductAmount { get; set; }
        public VoucherItem Voucher { get; set; }
        public ProductSaleItem ProductSale { get; set; }
    }

    public class PromotionPreviewResult
    {
        public decimal SaleDiscount        { get; set; }
        public decimal VoucherDiscount     { get; set; }
        public decimal TotalDiscount       { get; set; }
        public decimal FinalAmount         { get; set; }
        public string  AppliedRule         { get; set; }
        public string  PriorityExplanation { get; set; }
        /// <summary>true khi cả hai loại khuyến mãi được áp đồng thời.</summary>
        public bool    StackMode           { get; set; }
    }
}
