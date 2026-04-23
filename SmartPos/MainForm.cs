using System;
using System.Drawing;
using System.Windows.Forms;
using SmartPos.Module.Suppliers.Views;

namespace SmartPos
{
    public class MainForm : Form
    {
        private Label lblWelcome;
        private Label lblUser;
        private Button btnLogout;
        private Button btnSuppliers;

        public MainForm()
        {
            InitializeUi();
            Load += MainForm_Load;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            if (UserSession.CurrentUser == null)
            {
                Close();
                return;
            }

            lblWelcome.Text = "SmartPOS - MainForm";
            lblUser.Text = "Xin chao, " + UserSession.CurrentUser.FullName + " (" + UserSession.CurrentUser.Username + ")";
        }

        private void InitializeUi()
        {
            lblWelcome = new Label();
            lblUser = new Label();
            btnLogout = new Button();
            btnSuppliers = new Button();

            SuspendLayout();

            BackColor = Color.White;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "SmartPOS - Main";
            ClientSize = new Size(900, 550);

            lblWelcome.AutoSize = true;
            lblWelcome.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblWelcome.Location = new Point(28, 25);

            lblUser.AutoSize = true;
            lblUser.Font = new Font("Segoe UI", 11F);
            lblUser.Location = new Point(30, 80);

            btnLogout.Text = "Dang xuat";
            btnLogout.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnLogout.Size = new Size(120, 36);
            btnLogout.Location = new Point(30, 120);
            btnLogout.Click += btnLogout_Click;

            btnSuppliers.Text = "Nha cung cap + Cong no";
            btnSuppliers.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnSuppliers.Size = new Size(220, 36);
            btnSuppliers.Location = new Point(165, 120);
            btnSuppliers.Click += btnSuppliers_Click;

            Controls.Add(lblWelcome);
            Controls.Add(lblUser);
            Controls.Add(btnLogout);
            Controls.Add(btnSuppliers);

            ResumeLayout(false);
            PerformLayout();
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            UserSession.Clear();
            Close();
        }

        private void btnSuppliers_Click(object sender, EventArgs e)
        {
            using (var supplierModule = new SupplierModuleForm())
            {
                supplierModule.ShowDialog(this);
            }
        }
    }
}
