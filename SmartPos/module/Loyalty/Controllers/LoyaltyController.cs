using System.Collections.Generic;
using System.Threading.Tasks;
using SmartPos.Module.Loyalty.Backend;
using SmartPos.Module.Loyalty.Models;

namespace SmartPos.Module.Loyalty.Controllers
{
    public class LoyaltyController
    {
        private readonly LoyaltyBackend _backend;

        public LoyaltyController()
        {
            _backend = new LoyaltyBackend();
        }

        public List<LoyaltyCustomerListItem> GetThanThietCustomers()
        {
            return _backend.GetCustomersByTier("Thân Thiết");
        }

        public List<LoyaltyCustomerListItem> GetVipCustomers()
        {
            return _backend.GetCustomersByTier("VIP");
        }

        public List<LoyaltyCustomerListItem> GetNearTierCustomers()
        {
            return _backend.GetCustomersNearTier();
        }

        public async Task SendManualEmailAsync(LoyaltyCustomerListItem customer)
        {
            await _backend.SendManualEmailAsync(customer);
        }
    }
}
