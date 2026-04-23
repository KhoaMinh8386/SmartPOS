using System.Collections.Generic;
using SmartPos.Module.Customers.Backend;
using SmartPos.Module.Customers.Models;

namespace SmartPos.Module.Customers.Controllers
{
    public class CustomerController
    {
        private readonly CustomerBackend _backend;

        public CustomerController()
        {
            _backend = new CustomerBackend();
            _backend.EnsureSchema();
        }

        public List<CustomerListItem> GetList(string search = null, string typeFilter = null)
            => _backend.GetList(search, typeFilter);

        public CustomerDetail GetDetail(int id)
            => _backend.GetDetail(id);

        public int Save(CustomerSaveRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.FullName))
                throw new System.ArgumentException("Tên khách hàng không được để trống.");
            return _backend.Save(req);
        }

        public void Delete(int id) => _backend.Delete(id);

        public List<PointsHistoryItem> GetPointsHistory(int customerId)
            => _backend.GetPointsHistory(customerId);

        public void AddPoints(int customerId, int points, string desc)
            => _backend.AdjustPoints(new AdjustPointsRequest
            {
                CustomerID  = customerId,
                Points      = points,
                Type        = "manual_add",
                Description = desc
            });

        public void RedeemPoints(int customerId, int points, string desc)
            => _backend.AdjustPoints(new AdjustPointsRequest
            {
                CustomerID  = customerId,
                Points      = -points,
                Type        = "manual_redeem",
                Description = desc
            });

        public List<CustomerInvoiceItem> GetInvoices(int customerId)
            => _backend.GetInvoices(customerId);
    }
}
