using System;
using System.Drawing;
using System.Windows.Forms;
using SmartPos.Module.Suppliers.Views;
using SmartPos.Module.Products.Views;
using SmartPos.Module.Pos.Views;

namespace SmartPos
{
    public class MainForm : Form
    {
        private Label lblWelcome;
        private Label lblUser;
        private Button btnLogout;
        private Button btnSuppliers;
        private Button btnProducts;
        private Button btnPos;
        private Button btnInvoices;

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
            btnProducts = new Button();
            btnPos = new Button();
            btnInvoices = new Button();

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

            btnProducts.Text = "Quan ly San pham";
            btnProducts.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnProducts.Size = new Size(220, 36);
            btnProducts.Location = new Point(400, 120);
            btnProducts.Click += btnProducts_Click;

            btnPos.Text = "POS BAN HANG";
            btnPos.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnPos.Size = new Size(250, 60);
            btnPos.Location = new Point(30, 180);
            btnPos.BackColor = Color.FromArgb(46, 125, 50);
            btnPos.ForeColor = Color.White;
            btnPos.FlatStyle = FlatStyle.Flat;
            btnPos.Click += btnPos_Click;

            btnInvoices.Text = "Lich su Hoa don";
            btnInvoices.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnInvoices.Size = new Size(220, 36);
            btnInvoices.Location = new Point(300, 195);
            btnInvoices.Click += btnInvoices_Click;

            Controls.Add(lblWelcome);
            Controls.Add(lblUser);
            Controls.Add(btnLogout);
            Controls.Add(btnSuppliers);
            Controls.Add(btnProducts);
            Controls.Add(btnPos);
            Controls.Add(btnInvoices);

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

        private void btnProducts_Click(object sender, EventArgs e)
        {
            using (var productModule = new ProductModuleForm())
            {
                productModule.ShowDialog(this);
            }
        }

        private void btnPos_Click(object sender, EventArgs e)
        {
            using (var posModule = new PosModuleForm())
            {
                posModule.ShowDialog(this);
            }
        }

        private void btnInvoices_Click(object sender, EventArgs e)
        {
            using (var historyForm = new InvoiceHistoryForm())
            {
                historyForm.ShowDialog(this);
            }
        }
    }
}
