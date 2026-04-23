using System;
using System.Collections.Generic;
namespace SmartPos.Module.Pos
{
    public class PosController
    {
        private readonly PosBackend _backend;

        public PosController()
        {
            _backend = new PosBackend();
        }

        public List<CartItem> FindProducts(string term)
        {
            if (string.IsNullOrWhiteSpace(term)) return new List<CartItem>();
            return _backend.FindProducts(term);
        }

        public CustomerInfo FindCustomer(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return null;
            return _backend.FindCustomerByPhone(phone);
        }

        public int Checkout(CheckoutRequest request)
        {
            if (request.Items == null || request.Items.Count == 0)
                throw new InvalidOperationException("Gio hang dang trong.");
            
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

        public VoucherInfo GetVoucher(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return null;
            return _backend.GetVoucher(code);
        }
    }
}
