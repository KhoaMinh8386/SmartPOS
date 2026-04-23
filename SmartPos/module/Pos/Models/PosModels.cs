using System;
using System.Collections.Generic;

namespace SmartPos.Module.Pos.Models
{
    public class CustomerInfo
    {
        public int CustomerID { get; set; }
        public string CustomerName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public int Points { get; set; }
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
        public string CustomerName { get; set; }
        public string StaffName { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethodText { get; set; }
    }

    public class InvoiceDetail
    {
        public int InvoiceID { get; set; }
        public string InvoiceCode { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string CustomerName { get; set; }
        public string Phone { get; set; }
        public string StaffName { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal ChangeAmount { get; set; }
        public byte PaymentMethod { get; set; }
        public List<CartItem> Items { get; set; }
    }

    public class CheckoutRequest
    {
        public int? CustomerID { get; set; }
        public int UserID { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public byte PaymentMethod { get; set; }
        public string Note { get; set; }
        public List<CartItem> Items { get; set; }
    }
}
