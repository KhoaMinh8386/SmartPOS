using System;
using System.Collections.Generic;
using SmartPos.Module.Pos.Backend;
using SmartPos.Module.Pos.Models;

namespace SmartPos.Module.Pos.Controllers
{
    public class PosController
    {
        private readonly PosBackend _backend;

        public PosController()
        {
            _backend = new PosBackend();
        }

        public List<CartItem> SearchProducts(string term)
        {
            if (string.IsNullOrWhiteSpace(term)) return new List<CartItem>();
            return _backend.FindProducts(term);
        }

        public CustomerInfo GetCustomer(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return null;
            return _backend.FindCustomerByPhone(phone);
        }

        public int RegisterCustomer(string name, string phone, string address)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(phone))
                throw new ArgumentException("Ten va So dien thoai khong duoc de trong.");
            return _backend.CreateCustomer(name, phone, address);
        }

        public string Checkout(CheckoutRequest request)
        {
            if (request.Items == null || request.Items.Count == 0)
                throw new InvalidOperationException("Gio hang dang trong.");
            
            if (request.PaidAmount < request.TotalAmount)
                throw new InvalidOperationException("So tien khach dua khong du.");

            return _backend.Checkout(request);
        }

        public List<InvoiceListItem> GetInvoiceHistory(string search = null)
        {
            return _backend.GetInvoices(search);
        }

        public InvoiceDetail GetInvoiceDetail(int invoiceId)
        {
            return _backend.GetInvoiceDetail(invoiceId);
        }
    }
}
