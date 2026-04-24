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
            string accentColor = "#FF6B35";
            string tierTitle = newTier.ToUpper();
            string tierDesc = $"Chào mừng bạn đến với hạng {newTier}!";
            string benefitsHtml = "";

            if (newTier == "Thân Thiết")
            {
                subject = $"🎉 Chúc mừng {customer.FullName} đạt hạng KHÁCH HÀNG THÂN THIẾT!";
                accentColor = "#FF6B35";
                benefitsHtml = @"
                    <li style='margin-bottom: 10px;'>🎁 <b>Giảm ngay 5%</b> cho mọi hóa đơn mua sắm.</li>
                    <li style='margin-bottom: 10px;'>🚀 <b>Ưu tiên</b> nhận thông báo các chương trình khuyến mãi sớm nhất.</li>
                    <li style='margin-bottom: 10px;'>🎂 <b>Quà tặng sinh nhật</b> bất ngờ dành riêng cho bạn.</li>
                    <li style='margin-bottom: 10px;'>⏳ <b>Đổi trả linh hoạt</b> trong vòng 30 ngày.</li>";
            }
            else if (newTier == "VIP")
            {
                subject = $"👑 CHÚC MỪNG {customer.FullName.ToUpper()} — BẠN ĐÃ TRỞ THÀNH THÀNH VIÊN VIP!";
                accentColor = "#FFD700";
                tierDesc = "Bạn đã chính thức gia nhập cộng đồng khách hàng cao cấp nhất của chúng tôi!";
                benefitsHtml = @"
                    <li style='margin-bottom: 10px;'>💎 <b>GIẢM TRỰC TIẾP 10%</b> cho tất cả đơn hàng.</li>
                    <li style='margin-bottom: 10px;'>🎈 <b>Quà tặng sinh nhật VIP</b> trị giá lên đến 500.000 VNĐ.</li>
                    <li style='margin-bottom: 10px;'>📞 <b>Đường dây nóng hỗ trợ riêng</b> phục vụ 24/7.</li>
                    <li style='margin-bottom: 10px;'>⚡ <b>Thanh toán ưu tiên</b>, không cần xếp hàng tại quầy.</li>
                    <li style='margin-bottom: 10px;'>🔥 <b>Đặc quyền tham gia</b> các sự kiện Private Sale kín.</li>";
            }
            else
            {
                // General Loyalty Info for other cases
                subject = $"👋 Chào {customer.FullName}, hãy cùng khám phá ưu đãi tại SmartPOS Supermarket!";
                accentColor = "#2193b0";
                tierTitle = "KHÁCH HÀNG THÂN QUEN";
                tierDesc = "Cảm ơn bạn đã luôn tin tưởng và mua sắm tại hệ thống của chúng tôi.";
                benefitsHtml = @"
                    <li style='margin-bottom: 10px;'>⭐ <b>Tích điểm đổi quà</b> với mỗi 10.000đ chi tiêu.</li>
                    <li style='margin-bottom: 10px;'>📱 <b>Quản lý chi tiêu</b> dễ dàng qua hệ thống SmartPOS.</li>
                    <li style='margin-bottom: 10px;'>🏷️ <b>Nhận ngay deal hot</b> mỗi tuần qua Email/SMS.</li>";
            }

            htmlBody = $@"
            <div style='font-family: ""Segoe UI"", Tahoma, Geneva, Verdana, sans-serif; max-width: 600px; margin: 20px auto; border-radius: 16px; overflow: hidden; box-shadow: 0 10px 30px rgba(0,0,0,0.1); border: 1px solid #eee;'>
                <div style='background: linear-gradient(135deg, {accentColor}, #000); padding: 50px 30px; text-align: center; color: white;'>
                    <div style='font-size: 50px; margin-bottom: 10px;'>✨</div>
                    <h1 style='margin: 0; font-size: 28px; letter-spacing: 2px; font-weight: 800;'>{tierTitle}</h1>
                    <p style='margin: 15px 0 0 0; font-size: 18px; opacity: 0.9;'>{tierDesc}</p>
                </div>
                <div style='padding: 40px 30px; background-color: #ffffff;'>
                    <p style='font-size: 16px; color: #333;'>Xin chào <b>{customer.FullName}</b>,</p>
                    <p style='font-size: 16px; color: #555; line-height: 1.6;'>Chúng tôi vô cùng trân trọng sự đồng hành của bạn. Hiện tại bạn đang có <span style='color: {accentColor}; font-weight: bold; font-size: 18px;'>{customer.TotalPoints:N0} điểm</span> tích lũy.</p>
                    
                    <div style='margin: 30px 0; padding: 25px; background-color: #f8fafc; border-radius: 12px; border-left: 6px solid {accentColor};'>
                        <h3 style='margin: 0 0 15px 0; color: #1e293b; font-size: 18px;'>🌟 ĐẶC QUYỀN CỦA BẠN:</h3>
                        <ul style='padding-left: 20px; margin: 0; color: #475569; font-size: 15px; line-height: 1.6;'>
                            {benefitsHtml}
                        </ul>
                    </div>

                    <div style='text-align: center; margin-top: 40px;'>
                        <a href='#' style='display: inline-block; background-color: {accentColor}; color: {(accentColor == "#FFD700" ? "#000" : "#fff")}; padding: 16px 40px; text-decoration: none; border-radius: 30px; font-weight: bold; font-size: 16px; box-shadow: 0 4px 15px rgba(0,0,0,0.15); transition: all 0.3s;'>MUA SẮM NGAY HÔM NAY</a>
                        <p style='margin-top: 20px; font-size: 14px; color: #94a3b8;'><i>* Ưu đãi áp dụng trên toàn hệ thống siêu thị SmartPOS</i></p>
                    </div>
                </div>
                <div style='background-color: #1e293b; color: #94a3b8; text-align: center; padding: 30px; font-size: 13px;'>
                    <p style='margin: 0; color: #fff; font-weight: bold; font-size: 15px;'>SmartPOS Supermarket</p>
                    <p style='margin: 10px 0;'>Hệ thống quản lý bán hàng chuyên nghiệp</p>
                    <p style='margin: 5px 0;'>Địa chỉ: TP. Hồ Chí Minh | Hotline: 1900 8888</p>
                    <div style='margin-top: 20px; padding-top: 20px; border-top: 1px solid #334155;'>
                        <p>Bạn nhận được thư này vì là thành viên thân thiết của SmartPOS.</p>
                    </div>
                </div>
            </div>";

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
                ? "Giảm 10% mọi đơn, Quà sinh nhật 500k, Hotline riêng 24/7, Ưu tiên thanh toán"
                : "Giảm 5% mọi đơn, Ưu tiên tin khuyến mãi, Quà sinh nhật, Đổi trả 30 ngày";

            string htmlBody = $@"
            <div style='font-family: ""Segoe UI"", Tahoma, Geneva, Verdana, sans-serif; max-width: 600px; margin: 20px auto; border-radius: 16px; overflow: hidden; box-shadow: 0 10px 30px rgba(0,0,0,0.1); border: 1px solid #eee;'>
                <div style='background: linear-gradient(135deg, #2193b0, #6dd5ed); padding: 50px 30px; text-align: center; color: white;'>
                    <div style='font-size: 50px; margin-bottom: 10px;'>⏳</div>
                    <h1 style='margin: 0; font-size: 28px; letter-spacing: 1px; font-weight: 800;'>CHỈ CÒN MỘT CHÚT NỮA!</h1>
                    <p style='margin: 15px 0 0 0; font-size: 18px; opacity: 0.9;'>Đặc quyền hạng <b>{nextTier}</b> đang chờ đón bạn</p>
                </div>
                <div style='padding: 40px 30px; background-color: #ffffff;'>
                    <p style='font-size: 16px; color: #333;'>Chào <b>{customer.FullName}</b>,</p>
                    <p style='font-size: 16px; color: #555; line-height: 1.6;'>Bạn đang sở hữu <span style='font-weight: bold; color: #2193b0;'>{currentPoints:N0} điểm</span>. Chỉ cần tích lũy thêm <b>{pointsNeeded:N0} điểm</b> nữa để nâng cấp lên hạng <b>{nextTier}</b>.</p>
                    
                    <div style='background-color: #f1f5f9; border-radius: 12px; padding: 25px; text-align: center; margin: 30px 0;'>
                        <div style='width: 100%; background-color: #e2e8f0; border-radius: 10px; height: 16px; overflow: hidden;'>
                            <div style='width: {percentage}%; background: linear-gradient(to right, #2193b0, #6dd5ed); height: 100%;'></div>
                        </div>
                        <p style='margin: 15px 0 0 0; font-weight: bold; color: #334155; font-size: 18px;'>{currentPoints:N0} / {targetPoints:N0} ĐIỂM ({percentage}%)</p>
                    </div>

                    <div style='background-color: #fffbeb; border: 1px solid #fde68a; padding: 20px; border-radius: 12px;'>
                        <p style='margin: 0; color: #92400e; font-size: 15px;'>🎁 <b>Đặc quyền sắp mở khóa:</b></p>
                        <p style='margin: 10px 0 0 0; color: #1e293b; font-weight: bold; font-size: 16px;'>✓ {benefits}</p>
                    </div>

                    <div style='background: linear-gradient(135deg, #fff3f3, #ffe9e9); padding: 25px; border-radius: 12px; text-align: center; border: 2px dashed #f87171; margin-top: 30px;'>
                        <h2 style='margin: 0; color: #dc2626; font-size: 20px;'>🔥 ƯU ĐÃI ĐẶC BIỆT 🔥</h2>
                        <p style='margin: 10px 0 0 0; color: #475569; line-height: 1.5;'>Ghé siêu thị mua sắm ngay hôm nay để nhận <b>GẤP ĐÔI ĐIỂM THƯỞNG</b> và thăng hạng tức thì!</p>
                    </div>
                </div>
                <div style='background-color: #1e293b; color: #94a3b8; text-align: center; padding: 30px; font-size: 13px;'>
                    <p style='margin: 0; color: #fff; font-weight: bold; font-size: 15px;'>SmartPOS Supermarket</p>
                    <p style='margin: 10px 0;'>Hệ thống quản lý bán hàng chuyên nghiệp</p>
                    <p style='margin: 5px 0;'>Địa chỉ: TP. Hồ Chí Minh | Hotline: 1900 8888</p>
                </div>
            </div>";

            await SendEmailAsync(customer.Email, subject, htmlBody);
        }
    }
}
