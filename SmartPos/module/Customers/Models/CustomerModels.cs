using System;
using System.Collections.Generic;

namespace SmartPos.Module.Customers.Models
{
    public class CustomerListItem
    {
        public int CustomerID { get; set; }
        public string CustomerCode { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public int TotalPoints { get; set; }
        public decimal TotalSpent { get; set; }
        public string CustomerType { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CustomerDetail
    {
        public int CustomerID { get; set; }
        public string CustomerCode { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public int TotalPoints { get; set; }
        public decimal TotalSpent { get; set; }
        public string CustomerType { get; set; }
        public string Note { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CustomerSaveRequest
    {
        public int? CustomerID { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Note { get; set; }
    }

    public class PointsHistoryItem
    {
        public int ID { get; set; }
        public int Points { get; set; }
        public string Type { get; set; }       // earn / redeem / manual
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AdjustPointsRequest
    {
        public int CustomerID { get; set; }
        public int Points { get; set; }          // positive = add, negative = deduct
        public string Type { get; set; }         // "manual_add" / "manual_redeem"
        public string Description { get; set; }
    }

    public class CustomerInvoiceItem
    {
        public int InvoiceID { get; set; }
        public string InvoiceCode { get; set; }
        public DateTime InvoiceDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethodText { get; set; }
    }
}
