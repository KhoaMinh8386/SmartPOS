using System;
using System.Collections.Generic;

namespace SmartPos.Module.SalesHistory.Models
{
    public class SalesOrderListItem
    {
        public int InvoiceID { get; set; }
        public string InvoiceCode { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string FullName { get; set; }
        public string StaffName { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount => TotalAmount - DiscountAmount;
        public string PaymentMethodText { get; set; }
        public int Status { get; set; }
        public string StatusText => Status == 1 ? "Hoan tat" : (Status == 2 ? "Da huy" : "Khac");
    }

    public class SalesOrderDetail
    {
        public int InvoiceID { get; set; }
        public string InvoiceCode { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string FullName { get; set; }
        public string CustomerPhone { get; set; }
        public string StaffName { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal VoucherDiscount { get; set; }
        public int LoyaltyPointsUsed { get; set; }
        public decimal LoyaltyDiscount { get; set; }
        public decimal FinalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal ChangeAmount { get; set; }
        public byte PaymentMethod { get; set; }
        public int LoyaltyPointsEarned { get; set; }
        public int Status { get; set; }
        public string Notes { get; set; }
        public List<SalesOrderItem> Items { get; set; }
    }

    public class SalesOrderItem
    {
        public int ProductID { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string UnitName { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal SubTotal => Quantity * UnitPrice;
    }
}
