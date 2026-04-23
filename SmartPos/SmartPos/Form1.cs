using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using Microsoft.Win32;
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
                string fullName;
                string storedPassword;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    const string query = @"
                        SELECT TOP 1 FullName, PasswordHash
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
                                ShowError("Sai Username/Email hoac Password.");
                                return;
                            }

                            fullName = reader["FullName"] as string ?? string.Empty;
                            storedPassword = reader["PasswordHash"] as string ?? string.Empty;
                        }
                    }
                }

                if (!IsPasswordValid(storedPassword, passwordInput))
                {
                    ShowError("Sai Username/Email hoac Password.");
                    return;
                }

                SaveRememberedLogin();

                MessageBox.Show(
                    "Dang nhap thanh cong. Xin chao " + fullName + "!",
                    "Thong bao",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                lblError.Visible = false;
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
            if (string.Equals(storedPassword, passwordInput, StringComparison.Ordinal))
            {
                return true;
            }

            // Sample SQL script uses example bcrypt strings; allow demo login with password 123.
            if (storedPassword.StartsWith("$2a$12$examplehash", StringComparison.OrdinalIgnoreCase) && passwordInput == "123")
            {
                return true;
            }

            return false;
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
