using System;
using System.Collections.Generic;
using SmartPos.Module.PurchaseOrders.Backend;
using SmartPos.Module.PurchaseOrders.Models;

namespace SmartPos.Module.PurchaseOrders.Controllers
{
    public class PurchaseOrderController
    {
        private readonly PurchaseOrderBackend _backend;

        public PurchaseOrderController()
        {
            _backend = new PurchaseOrderBackend();
        }

        public PurchaseOrderModuleData InitializeModule()
        {
            _backend.EnsureSchema();
            return _backend.LoadMasterData();
        }

        public int CreatePurchaseOrder(CreatePurchaseOrderRequest request)
        {
            ValidateRequest(request);
            return _backend.CreatePurchaseOrder(request);
        }

        public List<FefoBatchItem> GetFefoBatches(int productId)
        {
            if (productId <= 0)
            {
                throw new InvalidOperationException("San pham khong hop le.");
            }

            return _backend.GetFefoBatches(productId);
        }

        private static void ValidateRequest(CreatePurchaseOrderRequest request)
        {
            if (request == null)
            {
                throw new InvalidOperationException("Du lieu phieu nhap khong hop le.");
            }

            if (request.SupplierID <= 0)
            {
                throw new InvalidOperationException("Vui long chon nha cung cap.");
            }

            if (request.CreatedByUserID <= 0)
            {
                throw new InvalidOperationException("Vui long chon nguoi nhap.");
            }

            if (request.WarehouseID <= 0)
            {
                throw new InvalidOperationException("Vui long chon kho nhap.");
            }

            if (request.Items == null || request.Items.Count == 0)
            {
                throw new InvalidOperationException("Phieu nhap phai co it nhat 1 san pham.");
            }

            for (int i = 0; i < request.Items.Count; i++)
            {
                PurchaseOrderDraftItem item = request.Items[i];
                if (item.ProductID <= 0)
                {
                    throw new InvalidOperationException("San pham o dong " + (i + 1) + " khong hop le.");
                }

                if (string.IsNullOrWhiteSpace(item.BatchNumber))
                {
                    throw new InvalidOperationException("Dong " + (i + 1) + " chua nhap so lo.");
                }

                if (item.Quantity <= 0)
                {
                    throw new InvalidOperationException("So luong o dong " + (i + 1) + " phai lon hon 0.");
                }

                if (item.CostPrice < 0)
                {
                    throw new InvalidOperationException("Gia nhap o dong " + (i + 1) + " khong hop le.");
                }

                if (item.ExpiryDate.HasValue && item.ManufactureDate.HasValue && item.ExpiryDate.Value.Date <= item.ManufactureDate.Value.Date)
                {
                    throw new InvalidOperationException("Ngay het han phai lon hon ngay san xuat o dong " + (i + 1) + ".");
                }
            }
        }
    }
}
