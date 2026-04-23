using System;
using System.Collections.Generic;
using SmartPos.Module.SalesHistory.Backend;
using SmartPos.Module.SalesHistory.Models;

namespace SmartPos.Module.SalesHistory.Controllers
{
    public class SalesController
    {
        private readonly SalesBackend _backend;

        public SalesController()
        {
            _backend = new SalesBackend();
        }

        public List<SalesOrderListItem> GetSalesHistory(DateTime from, DateTime to, int? staffId, string customerSearch, byte? payMethod, int? status)
        {
            return _backend.GetSalesHistory(from, to, staffId, customerSearch, payMethod, status);
        }

        public SalesOrderDetail GetOrderDetail(int invoiceId)
        {
            return _backend.GetOrderDetail(invoiceId);
        }

        public void CancelOrder(int invoiceId, string reason)
        {
            _backend.CancelOrder(invoiceId, reason);
        }

        public Dictionary<int, string> GetUsers()
        {
            return _backend.GetUsers();
        }
    }
}
