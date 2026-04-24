using System;

namespace SmartPos.Module.Loyalty.Models
{
    public class LoyaltyCustomerListItem
    {
        public int CustomerID { get; set; }
        public string CustomerCode { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public int TotalPoints { get; set; }
        public decimal TotalSpent { get; set; }
        public string CustomerType { get; set; } // "Thường", "Thân Thiết", "VIP"
        public DateTime CreatedAt { get; set; }

        public int PointsToNextTier { get; set; } // Used for "Sắp lên hạng"
        public string NextTierName { get; set; }
    }

    public class SmtpSettings
    {
        public string Server { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool EnableSsl { get; set; }
    }
}
