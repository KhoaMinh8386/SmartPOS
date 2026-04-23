using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using Microsoft.Win32;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace SmartPos
{
    public partial class Form1 : Form
    {
        private const string RegistryPath = @"Software\SmartPos";

        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;
            this.Shown += Form1_Shown;
            this.AcceptButton = btnLogin;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            BuildLogo();
            LoadRememberedLogin();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            txtUsername.Focus();
            txtUsername.SelectAll();
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

            if (string.IsNullOrWhiteSpace(loginInput))
            {
                ShowError("Vui long nhap Username hoac Email.");
                txtUsername.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(passwordInput))
            {
                ShowError("Vui long nhap Password.");
                txtPassword.Focus();
                return;
            }

            string connectionString = ConfigurationManager.ConnectionStrings["SmartPosDb"]?.ConnectionString;

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                ShowError("Chua cau hinh SmartPosDb trong App.config.");
                return;
            }

            btnLogin.Enabled = false;

            try
            {
                int userId;
                int roleId;
                string username;
                string fullName;
                string storedPassword;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    const string query = @"
                                                SELECT TOP 1 UserID, RoleID, Username, FullName, PasswordHash
                        FROM dbo.Users
                        WHERE IsActive = 1
                          AND (Username = @Login OR Email = @Login);";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Login", loginInput);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                ShowError("Sai tài khoản hoặc mật khẩu");
                                return;
                            }

                            userId = Convert.ToInt32(reader["UserID"]);
                            roleId = Convert.ToInt32(reader["RoleID"]);
                            username = reader["Username"] as string ?? loginInput;
                            fullName = reader["FullName"] as string ?? string.Empty;
                            storedPassword = reader["PasswordHash"] as string ?? string.Empty;
                        }
                    }
                }

                if (!IsPasswordValid(storedPassword, passwordInput))
                {
                    ShowError("Sai tài khoản hoặc mật khẩu");
                    return;
                }

                UserSession.CurrentUser = new UserSessionInfo
                {
                    UserID = userId,
                    RoleID = roleId,
                    Username = username,
                    FullName = fullName
                };

                SaveRememberedLogin();
                lblError.Visible = false;

                Hide();
                using (MainForm mainForm = new MainForm())
                {
                    mainForm.ShowDialog(this);
                }

                if (!UserSession.IsLoggedIn)
                {
                    txtPassword.Clear();
                    Show();
                    txtUsername.Focus();
                }
                else
                {
                    Close();
                }
            }
            catch (Exception ex)
            {
                ShowError("Khong the ket noi database. Chi tiet: " + ex.Message);
            }
            finally
            {
                btnLogin.Enabled = true;
            }
        }

        private bool IsPasswordValid(string storedPassword, string passwordInput)
        {
            if (string.IsNullOrWhiteSpace(storedPassword) || string.IsNullOrWhiteSpace(passwordInput))
            {
                return false;
            }

            // BCrypt hash starts with $2a/$2b/$2y. Verify via BCrypt.Net if installed.
            if (storedPassword.StartsWith("$2", StringComparison.Ordinal))
            {
                return VerifyBcrypt(storedPassword, passwordInput);
            }

            // SHA256 (hex 64 chars)
            if (storedPassword.Length == 64 && IsHexString(storedPassword))
            {
                string inputHash = ComputeSha256(passwordInput);
                return string.Equals(storedPassword, inputHash, StringComparison.OrdinalIgnoreCase);
            }

            // Backward compatibility for plain-text seeds in development data.
            if (string.Equals(storedPassword, passwordInput, StringComparison.Ordinal))
            {
                return true;
            }

            return false;
        }

        private static string ComputeSha256(string value)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(value);
                byte[] hashBytes = sha.ComputeHash(inputBytes);
                StringBuilder builder = new StringBuilder(hashBytes.Length * 2);

                foreach (byte b in hashBytes)
                {
                    builder.Append(b.ToString("x2", CultureInfo.InvariantCulture));
                }

                return builder.ToString();
            }
        }

        private static bool IsHexString(string value)
        {
            foreach (char c in value)
            {
                bool isHex =
                    (c >= '0' && c <= '9') ||
                    (c >= 'a' && c <= 'f') ||
                    (c >= 'A' && c <= 'F');

                if (!isHex)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool VerifyBcrypt(string hashedPassword, string plainPassword)
        {
            Type bcryptType = Type.GetType("BCrypt.Net.BCrypt, BCrypt.Net-Next")
                ?? Type.GetType("BCrypt.Net.BCrypt, BCrypt.Net");

            if (bcryptType == null)
            {
                return false;
            }

            MethodInfo verifyMethod = bcryptType.GetMethod("Verify", new[] { typeof(string), typeof(string) });
            if (verifyMethod == null)
            {
                return false;
            }

            object result = verifyMethod.Invoke(null, new object[] { plainPassword, hashedPassword });
            return result is bool && (bool)result;
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
                if (key == null)
                {
                    return;
                }

                string rememberValue = Convert.ToString(key.GetValue("RememberLogin", "0"));
                string usernameValue = Convert.ToString(key.GetValue("Username", string.Empty));

                chkRemember.Checked = rememberValue == "1";
                if (chkRemember.Checked)
                {
                    txtUsername.Text = usernameValue;
                }
            }
        }

        private void SaveRememberedLogin()
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RegistryPath))
            {
                if (key == null)
                {
                    return;
                }

                if (chkRemember.Checked)
                {
                    key.SetValue("RememberLogin", "1");
                    key.SetValue("Username", txtUsername.Text.Trim());
                }
                else
                {
                    key.SetValue("RememberLogin", "0");
                    key.DeleteValue("Username", false);
                }
            }
        }

        private void BuildLogo()
        {
            Bitmap bitmap = new Bitmap(pictureBoxLogo.Width, pictureBoxLogo.Height);

            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.Clear(Color.Transparent);

                Rectangle outer = new Rectangle(20, 5, 60, 50);
                using (Brush brush = new SolidBrush(Color.FromArgb(25, 118, 210)))
                {
                    graphics.FillEllipse(brush, outer);
                }

                using (Font font = new Font("Segoe UI", 12f, FontStyle.Bold))
                using (Brush textBrush = new SolidBrush(Color.White))
                {
                    StringFormat format = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    graphics.DrawString("POS", font, textBrush, outer, format);
                }
            }

            if (pictureBoxLogo.Image != null)
            {
                pictureBoxLogo.Image.Dispose();
            }

            pictureBoxLogo.Image = bitmap;
        }
    }
}
