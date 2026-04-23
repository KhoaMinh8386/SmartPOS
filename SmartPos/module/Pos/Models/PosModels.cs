using System;
using System.Collections.Generic;

namespace SmartPos.Module.Pos
{
    public class CustomerInfo
    {
        public int CustomerID { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public int TotalPoints { get; set; }
    }

    public class CartItem
    {
        public int ProductID { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Quantity { get; set; }
        public decimal SubTotal => UnitPrice * Quantity;
        public int UnitID { get; set; }
        public string UnitName { get; set; }
    }

    public class InvoiceListItem
    {
        public int InvoiceID { get; set; }
        public string InvoiceCode { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string FullName { get; set; }
        public string StaffName { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethodText { get; set; }
    }

    public class InvoiceDetail
    {
        public int InvoiceID { get; set; }
        public string InvoiceCode { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string StaffName { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal SubTotal { get; set; }
        public decimal VoucherDiscount { get; set; }
        public decimal PointsDiscount { get; set; }
        public int UsedPoints { get; set; }
        public int EarnedPoints { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal ChangeAmount { get; set; }
        public byte PaymentMethod { get; set; }
        public List<CartItem> Items { get; set; }
    }

    public class CheckoutRequest
    {
        public int? CustomerID { get; set; }
        public int UserID { get; set; }
        public decimal SubTotal { get; set; }
        public decimal VoucherDiscount { get; set; }
        public decimal PointsDiscount { get; set; }
        public int UsedPoints { get; set; }
        public int EarnedPoints { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public byte PaymentMethod { get; set; } // 1: Cash, 2: Bank, 3: Combined
        public string VoucherCode { get; set; }
        public string Note { get; set; }
        public List<CartItem> Items { get; set; }
    }

    public class StoreConfig
    {
        public string StoreName { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string FooterMessage { get; set; }
        public decimal LoyaltyPointRate { get; set; } // Ví dụ: 1000đ = 1 điểm
        public decimal PointRedeemRate { get; set; } // Ví dụ: 1 điểm = 100đ
    }

    public class ProductSaleInfo
    {
        public int SaleID { get; set; }
        public int ProductID { get; set; }
        public byte DiscountType { get; set; } // 1: %, 2: Cash
        public decimal DiscountValue { get; set; }
        public decimal? SalePrice { get; set; }
        public bool AllowStackVoucher { get; set; }
    }

    public class VoucherInfo
    {
        public int VoucherID { get; set; }
        public string VoucherCode { get; set; }
        public byte DiscountType { get; set; } // 1: %, 2: Cash
        public decimal DiscountValue { get; set; }
        public decimal MinOrderValue { get; set; }
        public decimal? MaxDiscount { get; set; }
        public bool AllowStackDiscount { get; set; }
    }
}
