using System;
using System.Collections.Generic;
using SmartPos.Module.Suppliers.Backend;
using SmartPos.Module.Suppliers.Models;

namespace SmartPos.Module.Suppliers.Controllers
{
    public class SupplierController
    {
        private readonly SupplierBackend _backend;

        public SupplierController()
        {
            _backend = new SupplierBackend();
        }

        public void InitializeModule()
        {
            _backend.EnsureSchema();
        }

        public List<SupplierListItem> GetSuppliers()
        {
            return _backend.GetSuppliers();
        }

        public List<SupplierOrderItem> GetOrders(int supplierId)
        {
            return _backend.GetSupplierOrders(supplierId);
        }

        public void AddPayment(SupplierPaymentRequest request)
        {
            if (request == null)
            {
                throw new InvalidOperationException("Payment request khong hop le.");
            }

            if (request.SupplierID <= 0)
            {
                throw new InvalidOperationException("Supplier khong hop le.");
            }

            if (request.PurchaseOrderID <= 0)
            {
                throw new InvalidOperationException("Phieu nhap khong hop le.");
            }

            if (request.Amount <= 0)
            {
                throw new InvalidOperationException("So tien thanh toan phai lon hon 0.");
            }

            if (request.PaymentMethod == 0)
            {
                request.PaymentMethod = 1;
            }

            _backend.AddPayment(request);
        }
    }
}
