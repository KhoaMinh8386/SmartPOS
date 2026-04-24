using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using SmartPos.Module.Loyalty.Models;

namespace SmartPos.Module.Loyalty.Backend
{
    public class EmailService
    {
        private readonly SmtpSettings _settings;

        public EmailService(SmtpSettings settings)
        {
            _settings = settings;
        }

        private async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            try
            {
                if (_settings == null || string.IsNullOrEmpty(_settings.Server))
                {
                    Console.WriteLine("SMTP Settings not found. Cannot send email.");
                    return;
                }

                using (var client = new SmtpClient(_settings.Server, _settings.Port))
                {
                    client.Credentials = new NetworkCredential(_settings.Username, _settings.Password);
                    client.EnableSsl = _settings.EnableSsl;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(_settings.Username, "SmartPOS Supermarket"),
                        Subject = subject,
                        Body = htmlBody,
                        IsBodyHtml = true
                    };
                    mailMessage.To.Add(toEmail);

                    await client.SendMailAsync(mailMessage);
                }
            }
            catch (Exception ex)
            {
                // Ghi log lỗi, không crash app
                Console.WriteLine($"Error sending email to {toEmail}: {ex.Message}");
            }
        }

        public async Task SendUpgradeEmailAsync(LoyaltyCustomerListItem customer, string newTier)
        {
            if (string.IsNullOrEmpty(customer.Email)) return;

            string subject = "";
            string htmlBody = "";

            if (newTier == "Thân Thiết")
            {
                subject = $"🎉 Chúc mừng {customer.FullName} đạt hạng KHÁCH HÀNG THÂN THIẾT!";
                int pointsToVip = 1000 - customer.TotalPoints; // Assuming VIP is 1000 points
                if (pointsToVip < 0) pointsToVip = 0;

                htmlBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; border: 1px solid #ddd; border-radius: 10px; overflow: hidden;'>
                    <div style='background: linear-gradient(to right, #FF6B35, #F7931E); padding: 30px; text-align: center; color: white;'>
                        <h1 style='margin: 0; font-size: 24px;'>KHÁCH HÀNG THÂN THIẾT</h1>
                        <p style='margin-top: 10px; font-size: 16px;'>Chúc mừng {customer.FullName} đã thăng hạng!</p>
                    </div>
                    <div style='padding: 20px;'>
                        <p>Bạn đang có <strong>{customer.TotalPoints} điểm</strong> trong tài khoản.</p>
                        <h3>🎁 Quyền lợi dành riêng cho bạn:</h3>
                        <ul>
                            <li>Giảm 5% cho mọi đơn hàng</li>
                            <li>Ưu tiên thông báo khi có chương trình Sale</li>
                            <li>Quà tặng đặc biệt dịp Sinh Nhật</li>
                            <li>Đổi trả ưu tiên lên đến 30 ngày</li>
                        </ul>
                        <div style='background-color: #f9f9f9; padding: 15px; border-left: 5px solid #FF6B35; margin-top: 20px;'>
                            <p style='margin: 0;'>💡 <strong>Gợi ý:</strong> Bạn chỉ cần tích thêm <strong>{pointsToVip} điểm</strong> nữa để thăng hạng <strong>VIP</strong> với ưu đãi lên đến 10%!</p>
                        </div>
                    </div>
                    <div style='background-color: #333; color: white; text-align: center; padding: 15px; font-size: 12px;'>
                        <p><strong>SmartPOS Supermarket</strong></p>
                        <p>Huỳnh Minh Khoa, Lê Thanh Tịnh, Trần Đức</p>
                    </div>
                </div>";
            }
            else if (newTier == "VIP")
            {
                subject = $"👑 Chúc mừng {customer.FullName} đạt hạng VIP — Ưu đãi 10% mọi đơn!";
                htmlBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; border: 1px solid #ddd; border-radius: 10px; overflow: hidden;'>
                    <div style='background: linear-gradient(to right, #FFD700, #FFA500); padding: 30px; text-align: center; color: #333;'>
                        <h1 style='margin: 0; font-size: 28px;'>👑 HẠNG THẺ VIP</h1>
                        <p style='margin-top: 10px; font-size: 16px;'>Xin chào {customer.FullName}, chào mừng bạn đến với trải nghiệm cao cấp nhất!</p>
                    </div>
                    <div style='padding: 20px;'>
                        <p>Thật tuyệt vời! Bạn đã đạt <strong>{customer.TotalPoints} điểm</strong>.</p>
                        <h3>💎 Đặc quyền VIP của bạn:</h3>
                        <ul>
                            <li><strong>Giảm 10%</strong> cho toàn bộ đơn hàng</li>
                            <li>Quà sinh nhật cao cấp trị giá 500k</li>
                            <li>Hotline hỗ trợ riêng biệt 24/7</li>
                            <li>Ưu tiên phục vụ tại quầy thu ngân</li>
                            <li>Nhận thông tin Sale sớm 48h trước mọi người</li>
                        </ul>
                        <p style='margin-top: 20px; font-style: italic;'>Cảm ơn bạn đã luôn đồng hành và ủng hộ chúng tôi.</p>
                    </div>
                    <div style='background-color: #333; color: white; text-align: center; padding: 15px; font-size: 12px;'>
                        <p><strong>SmartPOS Supermarket</strong></p>
                        <p>Huỳnh Minh Khoa, Lê Thanh Tịnh, Trần Đức</p>
                    </div>
                </div>";
            }

            await SendEmailAsync(customer.Email, subject, htmlBody);
        }

        public async Task SendNearTierEmailAsync(LoyaltyCustomerListItem customer, string nextTier, int pointsNeeded)
        {
            if (string.IsNullOrEmpty(customer.Email)) return;

            string subject = $"⏰ {customer.FullName} ơi! Chỉ còn {pointsNeeded} điểm nữa là đạt hạng {nextTier}!";
            int currentPoints = customer.TotalPoints;
            int targetPoints = currentPoints + pointsNeeded;
            int percentage = (int)((double)currentPoints / targetPoints * 100);
            
            // Assume 1 point = 10,000 VND
            decimal amountNeeded = pointsNeeded * 10000;

            string benefits = nextTier == "VIP" 
                ? "Giảm 10% mọi đơn, quà sinh nhật cao cấp, hotline riêng, ưu tiên phục vụ, sale sớm 48h"
                : "Giảm 5% mọi đơn, ưu tiên thông báo sale, quà sinh nhật, đổi trả 30 ngày";

            string htmlBody = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; border: 1px solid #ddd; border-radius: 10px; overflow: hidden;'>
                <div style='background: linear-gradient(to right, #2193b0, #6dd5ed); padding: 30px; text-align: center; color: white;'>
                    <h1 style='margin: 0; font-size: 24px;'>CHÚT NỮA THÔI!</h1>
                    <p style='margin-top: 10px; font-size: 16px;'>Cơ hội thăng hạng {nextTier} đang ở rất gần</p>
                </div>
                <div style='padding: 20px;'>
                    <p>Chào <strong>{customer.FullName}</strong>,</p>
                    <p>Bạn chỉ còn thiếu đúng <strong>{pointsNeeded} điểm</strong> để nâng cấp lên hạng <strong>{nextTier}</strong>.</p>
                    
                    <div style='background-color: #f1f1f1; border-radius: 10px; padding: 15px; text-align: center; margin: 20px 0;'>
                        <div style='width: 100%; background-color: #ddd; border-radius: 5px; height: 20px; overflow: hidden;'>
                            <div style='width: {percentage}%; background-color: #2193b0; height: 100%;'></div>
                        </div>
                        <p style='margin: 10px 0 0 0; font-weight: bold;'>{currentPoints} / {targetPoints} điểm ({percentage}%)</p>
                    </div>

                    <p>Chỉ cần mua sắm thêm <strong>{amountNeeded:N0} VNĐ</strong>, bạn sẽ mở khóa ngay các đặc quyền:</p>
                    <p style='color: #2193b0; font-weight: bold;'>✓ {benefits}</p>

                    <div style='background-color: #ffeaa7; padding: 15px; border-radius: 5px; text-align: center; border: 2px dashed #fdcb6e; margin-top: 20px;'>
                        <h2 style='margin: 0; color: #d63031;'>🔥 DOUBLE POINTS 🔥</h2>
                        <p style='margin: 5px 0 0 0;'>Ghé siêu thị mua sắm ngay hôm nay để nhận gấp đôi điểm thưởng và thăng hạng tức thì!</p>
                    </div>
                </div>
                <div style='background-color: #333; color: white; text-align: center; padding: 15px; font-size: 12px;'>
                    <p><strong>SmartPOS Supermarket</strong></p>
                    <p>Huỳnh Minh Khoa, Lê Thanh Tịnh, Trần Đức</p>
                </div>
            </div>";

            await SendEmailAsync(customer.Email, subject, htmlBody);
        }
    }
}
