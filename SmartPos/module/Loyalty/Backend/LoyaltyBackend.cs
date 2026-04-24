using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using SmartPos.Module.Loyalty.Models;
using SmartPos.Module.Loyalty.Templates;

namespace SmartPos.Module.Loyalty.Backend
{
    public class LoyaltyBackend
    {
        private readonly string _conn;
        private EmailService _emailService;

        public int ThanThietThreshold { get; set; } = 500;
        public int VipThreshold { get; set; } = 1000;

        public LoyaltyBackend()
        {
            _conn = ConfigurationManager.ConnectionStrings["SmartPosDb"]?.ConnectionString;
            InitEmailService();
        }

        private void InitEmailService()
        {
            try
            {
                var settings = new SmtpSettings();
                using (var con = new SqlConnection(_conn))
                using (var cmd = new SqlCommand(LoyaltySqlTemplate.GetSmtpSettings, con))
                {
                    con.Open();
                    using (var rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            var key = rdr["SettingKey"].ToString();
                            var val = rdr["SettingValue"].ToString();
                            switch (key)
                            {
                                case "SmtpServer": settings.Server = val; break;
                                case "SmtpPort": settings.Port = int.TryParse(val, out int port) ? port : 587; break;
                                case "SmtpUser": settings.Username = val; break;
                                case "SmtpPass": settings.Password = val; break;
                                case "SmtpEnableSsl": settings.EnableSsl = val.ToLower() == "true" || val == "1"; break;
                            }
                        }
                    }
                }
                _emailService = new EmailService(settings);
            }
            catch
            {
                // Fallback to default or empty
                _emailService = new EmailService(new SmtpSettings());
            }
        }

        public async Task ProcessPaymentSuccessAsync(int customerId, decimal totalAmount)
        {
            int pointsEarned = (int)(totalAmount / 10000); // Assume 1 point per 10k
            if (pointsEarned > 0)
            {
                using (var con = new SqlConnection(_conn))
                using (var cmd = new SqlCommand(LoyaltySqlTemplate.UpdateCustomerPoints, con))
                {
                    cmd.Parameters.AddWithValue("@PointsAdded", pointsEarned);
                    cmd.Parameters.AddWithValue("@AmountSpent", totalAmount);
                    cmd.Parameters.AddWithValue("@CustomerID", customerId);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            await CheckAndUpgradeTierAsync(customerId);
        }

        public async Task CheckAndUpgradeTierAsync(int customerId)
        {
            LoyaltyCustomerListItem customer = null;
            using (var con = new SqlConnection(_conn))
            using (var cmd = new SqlCommand(LoyaltySqlTemplate.GetCustomerById, con))
            {
                cmd.Parameters.AddWithValue("@CustomerID", customerId);
                con.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    if (rdr.Read())
                    {
                        customer = new LoyaltyCustomerListItem
                        {
                            CustomerID = (int)rdr["CustomerID"],
                            CustomerCode = rdr["CustomerCode"]?.ToString(),
                            FullName = rdr["FullName"].ToString(),
                            Phone = rdr["Phone"]?.ToString(),
                            Email = rdr["Email"]?.ToString(),
                            TotalPoints = (int)rdr["TotalPoints"],
                            TotalSpent = (decimal)rdr["TotalSpent"],
                            CustomerType = rdr["CustomerType"]?.ToString(),
                            CreatedAt = (DateTime)rdr["CreatedAt"]
                        };
                    }
                }
            }

            if (customer == null) return;

            string newTier = customer.CustomerType;
            bool upgraded = false;

            if (customer.TotalPoints >= VipThreshold && customer.CustomerType != "VIP")
            {
                newTier = "VIP";
                upgraded = true;
            }
            else if (customer.TotalPoints >= ThanThietThreshold && customer.TotalPoints < VipThreshold && customer.CustomerType != "Thân Thiết")
            {
                newTier = "Thân Thiết";
                upgraded = true;
            }

            if (upgraded)
            {
                using (var con = new SqlConnection(_conn))
                using (var cmd = new SqlCommand(LoyaltySqlTemplate.UpdateCustomerTier, con))
                {
                    cmd.Parameters.AddWithValue("@NewTier", newTier);
                    cmd.Parameters.AddWithValue("@CustomerID", customer.CustomerID);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                if (!string.IsNullOrEmpty(customer.Email))
                {
                    await _emailService.SendUpgradeEmailAsync(customer, newTier);
                }
            }
            else
            {
                // Check if near tier
                string currentTier = customer.CustomerType ?? "Thường";
                string nextTier = null;
                int pointsNeeded = 0;

                if (currentTier == "Thường" && customer.TotalPoints >= ThanThietThreshold - 2 && customer.TotalPoints < ThanThietThreshold)
                {
                    nextTier = "Thân Thiết";
                    pointsNeeded = ThanThietThreshold - customer.TotalPoints;
                }
                else if (currentTier == "Thân Thiết" && customer.TotalPoints >= VipThreshold - 2 && customer.TotalPoints < VipThreshold)
                {
                    nextTier = "VIP";
                    pointsNeeded = VipThreshold - customer.TotalPoints;
                }

                if (nextTier != null && pointsNeeded > 0)
                {
                    if (!string.IsNullOrEmpty(customer.Email))
                    {
                        await _emailService.SendNearTierEmailAsync(customer, nextTier, pointsNeeded);
                    }
                }
            }
        }

        public List<LoyaltyCustomerListItem> GetCustomersByTier(string tier)
        {
            var list = new List<LoyaltyCustomerListItem>();
            using (var con = new SqlConnection(_conn))
            using (var cmd = new SqlCommand(LoyaltySqlTemplate.GetCustomersByTier, con))
            {
                cmd.Parameters.AddWithValue("@Tier", tier);
                con.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        list.Add(new LoyaltyCustomerListItem
                        {
                            CustomerID = (int)rdr["CustomerID"],
                            CustomerCode = rdr["CustomerCode"]?.ToString(),
                            FullName = rdr["FullName"].ToString(),
                            Phone = rdr["Phone"]?.ToString(),
                            Email = rdr["Email"]?.ToString(),
                            TotalPoints = (int)rdr["TotalPoints"],
                            TotalSpent = (decimal)rdr["TotalSpent"],
                            CustomerType = rdr["CustomerType"]?.ToString(),
                            CreatedAt = (DateTime)rdr["CreatedAt"]
                        });
                    }
                }
            }
            return list;
        }

        public List<LoyaltyCustomerListItem> GetCustomersNearTier()
        {
            var list = new List<LoyaltyCustomerListItem>();
            using (var con = new SqlConnection(_conn))
            using (var cmd = new SqlCommand(LoyaltySqlTemplate.GetCustomersNearTier, con))
            {
                cmd.Parameters.AddWithValue("@ThanThietPointsThreshold", ThanThietThreshold);
                cmd.Parameters.AddWithValue("@VipPointsThreshold", VipThreshold);
                con.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        var customer = new LoyaltyCustomerListItem
                        {
                            CustomerID = (int)rdr["CustomerID"],
                            CustomerCode = rdr["CustomerCode"]?.ToString(),
                            FullName = rdr["FullName"].ToString(),
                            Phone = rdr["Phone"]?.ToString(),
                            Email = rdr["Email"]?.ToString(),
                            TotalPoints = (int)rdr["TotalPoints"],
                            TotalSpent = (decimal)rdr["TotalSpent"],
                            CustomerType = rdr["CustomerType"]?.ToString(),
                            CreatedAt = (DateTime)rdr["CreatedAt"]
                        };

                        if (customer.CustomerType == "Thường")
                        {
                            customer.NextTierName = "Thân Thiết";
                            customer.PointsToNextTier = ThanThietThreshold - customer.TotalPoints;
                        }
                        else if (customer.CustomerType == "Thân Thiết")
                        {
                            customer.NextTierName = "VIP";
                            customer.PointsToNextTier = VipThreshold - customer.TotalPoints;
                        }

                        list.Add(customer);
                    }
                }
            }
            return list;
        }

        // For manual sending from UI
        public async Task SendManualEmailAsync(LoyaltyCustomerListItem customer)
        {
            if (string.IsNullOrEmpty(customer.Email)) return;

            if (customer.PointsToNextTier > 0)
            {
                await _emailService.SendNearTierEmailAsync(customer, customer.NextTierName, customer.PointsToNextTier);
            }
            else
            {
                await _emailService.SendUpgradeEmailAsync(customer, customer.CustomerType);
            }
        }
    }
}
