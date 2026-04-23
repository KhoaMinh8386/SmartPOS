using System;
using System.Collections.Generic;

namespace SmartPos.Module.Suppliers.Models
{
    public class SupplierListItem
    {
        public int SupplierID { get; set; }
        public string SupplierName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string ImageUrl { get; set; }
        public decimal TotalDebt { get; set; }
    }

    public class SupplierOrderItem
    {
        public int PurchaseOrderID { get; set; }
        public string InvoiceCode { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal DebtAmount { get; set; }
        public string StatusText { get; set; }
    }

    public class SupplierPaymentRequest
    {
        public int SupplierID { get; set; }
        public int PurchaseOrderID { get; set; }
        public decimal Amount { get; set; }
        public byte PaymentMethod { get; set; }
        public string Note { get; set; }
        public int? CreatedByUserID { get; set; }
    }

    public class SupplierModuleState
    {
        public List<SupplierListItem> Suppliers { get; set; }
        public List<SupplierOrderItem> Orders { get; set; }
    }
}
