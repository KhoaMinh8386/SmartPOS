-- Script khởi tạo cấu hình cho dự án SmartPOS
-- Chạy bằng sqlcmd: sqlcmd -S <Tên_Server> -d SmartPosDb -E -i InitSettings.sql

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Settings')
BEGIN
    CREATE TABLE dbo.Settings (
        SettingKey NVARCHAR(50) PRIMARY KEY,
        SettingValue NVARCHAR(255)
    );
END;

DELETE FROM dbo.Settings 
WHERE SettingKey IN ('SmtpServer', 'SmtpPort', 'SmtpUser', 'SmtpPass', 'SmtpEnableSsl');

-- Thêm cấu hình SMTP của Gmail (Sử dụng App Password)
INSERT INTO dbo.Settings (SettingKey, SettingValue) VALUES
('SmtpServer', 'smtp.gmail.com'),
('SmtpPort', '587'),
('SmtpUser', 'lethanhtinh357@gmail.com'),  -- Đổi thành Email Gmail của bạn
('SmtpPass', 'rgiqzfwpepnmsnyb'),         -- App password bạn vừa cung cấp (đã xóa khoảng trắng)
('SmtpEnableSsl', 'true');

PRINT 'Da khoi tao thanh cong bang Settings va cau hinh SMTP!';
