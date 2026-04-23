using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace SmartPos
{
    public partial class Form1 : Form
    {
        private const string RegistryPath = @"Software\SmartPos";
        private Button btnTogglePassword;
        private bool isPasswordHidden = true;

        public Form1()
        {
            InitializeComponent();
            EnsureDatabaseSchema(); // Tự động sửa lỗi thiếu cột
            SetupCustomControls();
            this.Load += Form1_Load;
            this.Shown += Form1_Shown;
            this.AcceptButton = btnLogin;
        }

        private void SetupCustomControls()
        {
            // Nút toggle password
            btnTogglePassword = new Button();
            btnTogglePassword.Size = new Size(25, 25);
            btnTogglePassword.Location = new Point(txtPassword.Width - 30, 0);
            btnTogglePassword.Cursor = Cursors.Hand;
            btnTogglePassword.FlatStyle = FlatStyle.Flat;
            btnTogglePassword.FlatAppearance.BorderSize = 0;
            btnTogglePassword.BackColor = Color.White;
            btnTogglePassword.Text = "👁";
            btnTogglePassword.Font = new Font("Segoe UI", 8);
            btnTogglePassword.Click += BtnTogglePassword_Click;
            txtPassword.Controls.Add(btnTogglePassword);
        }

        private void BtnTogglePassword_Click(object sender, EventArgs e)
        {
            isPasswordHidden = !isPasswordHidden;
            txtPassword.UseSystemPasswordChar = isPasswordHidden;
            btnTogglePassword.Text = isPasswordHidden ? "👁" : "🔒";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            EnsureDatabaseSchema();
            BuildLogo();
            LoadRememberedLogin();
            txtPassword.UseSystemPasswordChar = true;
        }

        private void EnsureDatabaseSchema()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["SmartPosDb"]?.ConnectionString;
            if (string.IsNullOrWhiteSpace(connectionString)) return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    // Thêm từng cột một để đảm bảo thành công
                    string[] sqls = new string[] {
                        "IF COL_LENGTH('dbo.Users', 'RoleID') IS NULL ALTER TABLE dbo.Users ADD RoleID INT NOT NULL DEFAULT 3;",
                        "IF COL_LENGTH('dbo.Users', 'FailedAttempts') IS NULL ALTER TABLE dbo.Users ADD FailedAttempts INT NOT NULL DEFAULT 0;",
                        "IF COL_LENGTH('dbo.Users', 'LockoutEnd') IS NULL ALTER TABLE dbo.Users ADD LockoutEnd DATETIME NULL;",
                        "IF COL_LENGTH('dbo.Users', 'IsActive') IS NULL ALTER TABLE dbo.Users ADD IsActive BIT NOT NULL DEFAULT 1;",
                        "IF COL_LENGTH('dbo.Invoices', 'VoucherCode') IS NULL ALTER TABLE dbo.Invoices ADD VoucherCode NVARCHAR(50) NULL;",
                        "IF COL_LENGTH('dbo.Invoices', 'VoucherDiscount') IS NULL ALTER TABLE dbo.Invoices ADD VoucherDiscount DECIMAL(18,2) NOT NULL DEFAULT 0;",
                        "IF COL_LENGTH('dbo.Invoices', 'SubTotal') IS NULL ALTER TABLE dbo.Invoices ADD SubTotal DECIMAL(18,2) NOT NULL DEFAULT 0;"
                    };
                    
                    foreach(var sql in sqls)
                    {
                        using (SqlCommand cmd = new SqlCommand(sql, conn))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex) 
            { 
                // Ghi log lỗi nếu cần, nhưng không chặn ứng dụng khởi động
                Console.WriteLine("Lỗi cập nhật DB: " + ex.Message);
            }
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            txtUsername.Focus();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            AttemptLogin();
        }

        private void Input_TextChanged(object sender, EventArgs e)
        {
            lblError.Visible = false;
        }

        private void AttemptLogin()
        {
            string loginInput = txtUsername.Text.Trim();
            string passwordInput = txtPassword.Text;

            if (string.IsNullOrWhiteSpace(loginInput) || string.IsNullOrWhiteSpace(passwordInput))
            {
                ShowError("Vui lòng nhập đầy đủ tài khoản và mật khẩu.");
                return;
            }

            string connectionString = ConfigurationManager.ConnectionStrings["SmartPosDb"]?.ConnectionString;
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                ShowError("Chưa cấu hình kết nối Database.");
                return;
            }

            btnLogin.Enabled = false;
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // 1. Kiểm tra tài khoản và trạng thái Lockout
                    const string checkQuery = @"
                        SELECT UserID, PasswordHash, RoleID, FullName, Username, FailedAttempts, LockoutEnd, IsActive
                        FROM dbo.Users 
                        WHERE Username = @Login OR Email = @Login";

                    DataRow userRow = null;
                    using (SqlCommand cmd = new SqlCommand(checkQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@Login", loginInput);
                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            if (dt.Rows.Count > 0) userRow = dt.Rows[0];
                        }
                    }

                    if (userRow == null)
                    {
                        ShowError("Tài khoản không tồn tại hoặc đã bị ngừng hoạt động.");
                        return;
                    }

                    int userId = (int)userRow["UserID"];
                    bool isActive = (bool)userRow["IsActive"];
                    int failedAttempts = (int)userRow["FailedAttempts"];
                    DateTime? lockoutEnd = userRow["LockoutEnd"] as DateTime?;

                    if (!isActive)
                    {
                        ShowError("Tài khoản này đã bị vô hiệu hóa.");
                        return;
                    }

                    // Kiểm tra Lockout
                    if (lockoutEnd.HasValue && lockoutEnd.Value > DateTime.Now)
                    {
                        TimeSpan remaining = lockoutEnd.Value - DateTime.Now;
                        ShowError($"Tài khoản đang bị khóa. Thử lại sau {Math.Ceiling(remaining.TotalMinutes)} phút.");
                        return;
                    }

                    string storedHash = userRow["PasswordHash"].ToString();
                    string inputHash = HashSHA256(passwordInput);

                    // Xác thực mật khẩu theo chuẩn SHA256
                    bool isPasswordCorrect = (inputHash.Equals(storedHash, StringComparison.OrdinalIgnoreCase)) 
                                          || (passwordInput == storedHash); // Hỗ trợ cả plaintext nếu có

                    if (isPasswordCorrect)

                    // 2. Xác thực mật khẩu
                    if (isPasswordCorrect)
                    {
                        // Thành công -> Reset FailedAttempts
                        ResetFailedAttempts(userId, conn);

                        UserSession.CurrentUser = new UserSessionInfo
                        {
                            UserID = userId,
                            RoleID = (int)userRow["RoleID"],
                            Username = userRow["Username"].ToString(),
                            FullName = userRow["FullName"].ToString()
                        };

                        SaveRememberedLogin();
                        this.Hide();
                        using (MainForm main = new MainForm())
                        {
                            main.ShowDialog();
                        }
                        this.Close();
                    }
                    else
                    {
                        // Thất bại -> Tăng FailedAttempts
                        HandleFailedLogin(userId, failedAttempts, conn);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Lỗi hệ thống: " + ex.Message);
            }
            finally
            {
                btnLogin.Enabled = true;
            }
        }

        private void HandleFailedLogin(int userId, int currentFailures, SqlConnection conn)
        {
            int newFailures = currentFailures + 1;
            DateTime? lockoutUntil = null;

            if (newFailures >= 5)
            {
                lockoutUntil = DateTime.Now.AddMinutes(5);
                ShowError("Nhập sai quá 5 lần. Tài khoản bị khóa 5 phút.");
            }
            else
            {
                ShowError($"Sai mật khẩu. Bạn còn {5 - newFailures} lần thử.");
            }

            const string updateQuery = "UPDATE dbo.Users SET FailedAttempts = @Fail, LockoutEnd = @Lock WHERE UserID = @ID";
            using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
            {
                cmd.Parameters.AddWithValue("@Fail", newFailures);
                cmd.Parameters.AddWithValue("@Lock", (object)lockoutUntil ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ID", userId);
                cmd.ExecuteNonQuery();
            }
        }

        private void ResetFailedAttempts(int userId, SqlConnection conn)
        {
            const string query = "UPDATE dbo.Users SET FailedAttempts = 0, LockoutEnd = NULL WHERE UserID = @ID";
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@ID", userId);
                cmd.ExecuteNonQuery();
            }
        }

        private bool IsPasswordValid(string storedPassword, string passwordInput)
        {
            if (string.IsNullOrWhiteSpace(storedPassword)) return false;

            // Kiểm tra BCrypt hash (thường bắt đầu bằng $2)
            if (storedPassword.StartsWith("$2"))
            {
                return VerifyBcrypt(storedPassword, passwordInput);
            }

            // Fallback plain text cho development
            return storedPassword == passwordInput;
        }

        private bool VerifyBcrypt(string hashedPassword, string plainPassword)
        {
            try
            {
                // Thử dùng BCrypt.Net qua reflection nếu có
                Type bcryptType = Type.GetType("BCrypt.Net.BCrypt, BCrypt.Net-Next") ?? Type.GetType("BCrypt.Net.BCrypt, BCrypt.Net");
                if (bcryptType != null)
                {
                    var method = bcryptType.GetMethod("Verify", new[] { typeof(string), typeof(string) });
                    return (bool)method.Invoke(null, new object[] { plainPassword, hashedPassword });
                }
            }
            catch { }
            return false;
        }

        private string HashSHA256(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(text));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private void ShowError(string message)
        {
            lblError.Text = message;
            lblError.Visible = true;
        }

        private void LoadRememberedLogin()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryPath))
            {
                if (key != null)
                {
                    chkRemember.Checked = Convert.ToString(key.GetValue("RememberLogin", "0")) == "1";
                    if (chkRemember.Checked) txtUsername.Text = Convert.ToString(key.GetValue("Username", ""));
                }
            }
        }

        private void SaveRememberedLogin()
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RegistryPath))
            {
                if (key != null)
                {
                    key.SetValue("RememberLogin", chkRemember.Checked ? "1" : "0");
                    if (chkRemember.Checked) key.SetValue("Username", txtUsername.Text.Trim());
                }
            }
        }

        private void BuildLogo()
        {
            try
            {
                string logoPath = System.IO.Path.Combine(Application.StartupPath, "access", "img", "logo.png");
                
                // Fallback for development if not in bin
                if (!System.IO.File.Exists(logoPath))
                {
                    logoPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "access", "img", "logo.png");
                }

                if (System.IO.File.Exists(logoPath))
                {
                    pictureBoxLogo.Image = Image.FromFile(logoPath);
                    pictureBoxLogo.SizeMode = PictureBoxSizeMode.Zoom;
                }
                else
                {
                    // Fallback to drawn logo if file missing
                    Bitmap bmp = new Bitmap(pictureBoxLogo.Width, pictureBoxLogo.Height);
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        g.SmoothingMode = SmoothingMode.AntiAlias;
                        g.Clear(Color.Transparent);
                        Rectangle rect = new Rectangle(20, 5, 60, 50);
                        using (LinearGradientBrush brush = new LinearGradientBrush(rect, Color.FromArgb(33, 150, 243), Color.FromArgb(21, 101, 192), 45f))
                        {
                            g.FillEllipse(brush, rect);
                        }
                        using (Font f = new Font("Segoe UI", 12, FontStyle.Bold))
                        {
                            g.DrawString("POS", f, Brushes.White, rect, new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                        }
                    }
                    pictureBoxLogo.Image = bmp;
                }
            }
            catch
            {
                // Silent fail if image loading errors
            }
        }
    }
}
