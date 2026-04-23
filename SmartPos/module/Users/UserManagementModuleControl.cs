using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace SmartPos
{
    public class UserManagementModuleControl : UserControl
    {
        private DataGridView dgvUsers;
        private TextBox txtUsername, txtFullName, txtEmail, txtPassword;
        private ComboBox cboRole;
        private CheckBox chkActive;
        private Button btnAdd, btnUpdate, btnDelete, btnResetPass;
        private int selectedUserId = 0;
        private string connectionString = ConfigurationManager.ConnectionStrings["SmartPosDb"]?.ConnectionString;

        public UserManagementModuleControl()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 9);

            Panel pnlInput = new Panel { Dock = DockStyle.Top, Height = 200, Padding = new Padding(20) };
            
            // Labels & Textboxes
            AddInput(pnlInput, "Username:", out txtUsername, 20, 20);
            AddInput(pnlInput, "Họ tên:", out txtFullName, 20, 60);
            AddInput(pnlInput, "Email:", out txtEmail, 350, 20);
            AddInput(pnlInput, "Mật khẩu:", out txtPassword, 350, 60, true);

            Label lblRole = new Label { Text = "Quyền hạn:", Location = new Point(20, 105), AutoSize = true };
            cboRole = new ComboBox { Location = new Point(100, 100), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cboRole.DataSource = Enum.GetValues(typeof(UserRole));
            
            chkActive = new CheckBox { Text = "Đang hoạt động", Location = new Point(350, 105), Checked = true, AutoSize = true };

            // Buttons
            btnAdd = new Button { Text = "Thêm mới", Location = new Point(20, 150), Size = new Size(100, 35), BackColor = Color.FromArgb(76, 175, 80), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnUpdate = new Button { Text = "Cập nhật", Location = new Point(130, 150), Size = new Size(100, 35), BackColor = Color.FromArgb(33, 150, 243), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnDelete = new Button { Text = "Xóa", Location = new Point(240, 150), Size = new Size(100, 35), BackColor = Color.FromArgb(244, 67, 54), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnResetPass = new Button { Text = "Reset Pass", Location = new Point(350, 150), Size = new Size(100, 35), BackColor = Color.Orange, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };

            btnAdd.Click += BtnAdd_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnDelete.Click += BtnDelete_Click;
            btnResetPass.Click += BtnResetPass_Click;

            pnlInput.Controls.AddRange(new Control[] { lblRole, cboRole, chkActive, btnAdd, btnUpdate, btnDelete, btnResetPass });

            dgvUsers = new DataGridView 
            { 
                Dock = DockStyle.Fill, 
                BackgroundColor = Color.WhiteSmoke,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            dgvUsers.SelectionChanged += DgvUsers_SelectionChanged;

            this.Controls.Add(dgvUsers);
            this.Controls.Add(pnlInput);
        }

        private void AddInput(Panel p, string label, out TextBox tb, int x, int y, bool isPass = false)
        {
            Label lbl = new Label { Text = label, Location = new Point(x, y + 5), AutoSize = true };
            tb = new TextBox { Location = new Point(x + 80, y), Width = 200 };
            if (isPass) tb.PasswordChar = '*';
            p.Controls.Add(lbl);
            p.Controls.Add(tb);
        }

        private void LoadUsers()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT UserID, Username, FullName, Email, RoleID, IsActive FROM dbo.Users ORDER BY UserID DESC";
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgvUsers.DataSource = dt;
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message); }
        }

        private void DgvUsers_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvUsers.SelectedRows.Count > 0)
            {
                var row = dgvUsers.SelectedRows[0];
                selectedUserId = (int)row.Cells["UserID"].Value;
                txtUsername.Text = row.Cells["Username"].Value.ToString();
                txtFullName.Text = row.Cells["FullName"].Value.ToString();
                txtEmail.Text = row.Cells["Email"].Value.ToString();
                cboRole.SelectedItem = (UserRole)row.Cells["RoleID"].Value;
                chkActive.Checked = (bool)row.Cells["IsActive"].Value;
                txtPassword.Enabled = false; // Khi edit không cho sửa trực tiếp pass ở đây
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtPassword.Text)) { MessageBox.Show("Vui lòng nhập mật khẩu cho user mới."); return; }
            ExecuteQuery("INSERT INTO dbo.Users (Username, FullName, Email, PasswordHash, RoleID, IsActive) VALUES (@User, @Full, @Email, @Pass, @Role, @Active)", true);
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (selectedUserId == 0) return;
            ExecuteQuery("UPDATE dbo.Users SET FullName = @Full, Email = @Email, RoleID = @Role, IsActive = @Active WHERE UserID = @ID", false);
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (selectedUserId == 0) return;
            if (MessageBox.Show("Xóa user này?", "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.Yes)
                ExecuteQuery("DELETE FROM dbo.Users WHERE UserID = @ID", false);
        }

        private void BtnResetPass_Click(object sender, EventArgs e)
        {
            if (selectedUserId == 0) return;
            string newPass = Microsoft.VisualBasic.Interaction.InputBox("Nhập mật khẩu mới:", "Reset Password", "123456");
            if (!string.IsNullOrEmpty(newPass))
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("UPDATE dbo.Users SET PasswordHash = @Pass WHERE UserID = @ID", conn);
                    cmd.Parameters.AddWithValue("@Pass", HashSHA256(newPass));
                    cmd.Parameters.AddWithValue("@ID", selectedUserId);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Đã reset mật khẩu.");
                }
            }
        }

        private void ExecuteQuery(string sql, bool isInsert)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@User", txtUsername.Text);
                    cmd.Parameters.AddWithValue("@Full", txtFullName.Text);
                    cmd.Parameters.AddWithValue("@Email", txtEmail.Text);
                    cmd.Parameters.AddWithValue("@Role", (int)cboRole.SelectedItem);
                    cmd.Parameters.AddWithValue("@Active", chkActive.Checked);
                    if (isInsert) cmd.Parameters.AddWithValue("@Pass", HashSHA256(txtPassword.Text));
                    cmd.Parameters.AddWithValue("@ID", selectedUserId);
                    
                    cmd.ExecuteNonQuery();
                    LoadUsers();
                    MessageBox.Show("Thành công!");
                    txtPassword.Enabled = true;
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }
        private string HashSHA256(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(text));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                    builder.Append(bytes[i].ToString("x2"));
                return builder.ToString();
            }
        }
    }
}
