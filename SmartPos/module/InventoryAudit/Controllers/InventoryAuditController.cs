using System;
using System.Collections.Generic;
using SmartPos.Module.InventoryAudit.Backend;
using SmartPos.Module.InventoryAudit.Models;

namespace SmartPos.Module.InventoryAudit.Controllers
{
    public class InventoryAuditController
    {
        private readonly InventoryAuditBackend _backend;

        public InventoryAuditController()
        {
            _backend = new InventoryAuditBackend();
            _backend.EnsureSchema();
        }

        public List<WarehouseOption> GetWarehouses()
        {
            return _backend.GetWarehouses();
        }

        public List<InventoryCheckSummary> GetChecksByWarehouse(int warehouseId)
        {
            if (warehouseId <= 0)
            {
                throw new InvalidOperationException("Kho không hợp lệ.");
            }

            return _backend.GetChecksByWarehouse(warehouseId);
        }

        public List<StockBatchItem> GetStockByWarehouse(int warehouseId)
        {
            if (warehouseId <= 0)
            {
                throw new InvalidOperationException("Kho không hợp lệ.");
            }

            return _backend.GetStockByWarehouse(warehouseId);
        }

        public InventoryCheckDraft CreateCheckDraft(int warehouseId, int createdByUserId, string notes)
        {
            if (warehouseId <= 0)
            {
                throw new InvalidOperationException("Vui lòng chọn kho kiểm kê.");
            }

            if (createdByUserId <= 0)
            {
                throw new InvalidOperationException("Người tạo phiếu không hợp lệ.");
            }

            return _backend.CreateCheckDraft(warehouseId, createdByUserId, notes);
        }

        public List<InventoryCheckItemEdit> GetCheckItems(int checkId)
        {
            if (checkId <= 0)
            {
                throw new InvalidOperationException("Phiếu kiểm kê không hợp lệ.");
            }

            return _backend.GetCheckItems(checkId);
        }

        public InventoryCheckHeader GetCheckHeader(int checkId)
        {
            if (checkId <= 0)
            {
                throw new InvalidOperationException("Phiếu kiểm kê không hợp lệ.");
            }

            return _backend.GetCheckHeader(checkId);
        }

        public List<InventoryCheckItemHistory> GetCheckItemHistories(int checkId)
        {
            if (checkId <= 0)
            {
                throw new InvalidOperationException("Phiếu kiểm kê không hợp lệ.");
            }

            return _backend.GetCheckItemHistories(checkId);
        }

        public void SaveCheckDraftItems(int checkId, List<InventoryCheckItemEdit> items, int changedByUserId)
        {
            if (checkId <= 0)
            {
                throw new InvalidOperationException("Phiếu kiểm kê không hợp lệ.");
            }

            if (items == null || items.Count == 0)
            {
                throw new InvalidOperationException("Phiếu kiểm kê không có chi tiết để lưu.");
            }

            for (int i = 0; i < items.Count; i++)
            {
                InventoryCheckItemEdit item = items[i];
                decimal actual = item.ActualQuantity ?? item.SystemQuantity;
                if (actual < 0)
                {
                    throw new InvalidOperationException("Số lượng thực tế không được âm ở dòng " + (i + 1) + ".");
                }
            }

            _backend.SaveCheckDraftItems(checkId, items, changedByUserId);
        }

        public void ApproveCheck(ApproveInventoryCheckRequest request)
        {
            if (request == null || request.CheckID <= 0)
            {
                throw new InvalidOperationException("Dữ liệu duyệt kiểm kê không hợp lệ.");
            }

            if (request.ApprovedByUserID <= 0)
            {
                throw new InvalidOperationException("Người duyệt không hợp lệ.");
            }

            if (request.Items == null || request.Items.Count == 0)
            {
                throw new InvalidOperationException("Phiếu kiểm kê không có chi tiết.");
            }

            for (int i = 0; i < request.Items.Count; i++)
            {
                InventoryCheckItemEdit item = request.Items[i];
                decimal actual = item.ActualQuantity ?? item.SystemQuantity;
                if (actual < 0)
                {
                    throw new InvalidOperationException("Số lượng thực tế không được âm ở dòng " + (i + 1) + ".");
                }
            }

            _backend.ApproveCheck(request);
        }
    }
}
