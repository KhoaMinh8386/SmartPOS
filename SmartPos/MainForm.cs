using System;
using System.Drawing;
using System.Windows.Forms;
using SmartPos.Module.InventoryAudit.Views;
using SmartPos.Module.PurchaseOrders.Views;
using SmartPos.Module.Suppliers.Views;
using SmartPos.Module.Products.Views;
using SmartPos.Module.Pos.Views;
using SmartPos.Module.Promotions.Views;

namespace SmartPos
{
    public class MainForm : Form
    {
        private Label lblWelcome;
        private Label lblUser;
        private Button btnLogout;
        private Button btnSuppliers;
        private Button btnPurchaseOrders;
        private Button btnInventoryAudit;
        private Button btnProducts;
        private Button btnPos;
        private Button btnInvoices;
        private Button btnPromotions;

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
            btnPurchaseOrders = new Button();
            btnInventoryAudit = new Button();
            btnProducts = new Button();
            btnPos = new Button();
            btnInvoices = new Button();
            btnPromotions = new Button();

            SuspendLayout();

            BackColor = Color.White;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "SmartPOS - Main";
            ClientSize = new Size(950, 600);

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

            // Row 1
            btnSuppliers.Text = "Nha cung cap";
            btnSuppliers.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnSuppliers.Size = new Size(200, 36);
            btnSuppliers.Location = new Point(165, 120);
            btnSuppliers.Click += btnSuppliers_Click;

            btnProducts.Text = "San pham";
            btnProducts.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnProducts.Size = new Size(200, 36);
            btnProducts.Location = new Point(375, 120);
            btnProducts.Click += btnProducts_Click;

            btnPurchaseOrders.Text = "Phieu nhap + Batch";
            btnPurchaseOrders.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnPurchaseOrders.Size = new Size(200, 36);
            btnPurchaseOrders.Location = new Point(585, 120);
            btnPurchaseOrders.Click += btnPurchaseOrders_Click;

            btnInventoryAudit.Text = "Kho + Kiem ke";
            btnInventoryAudit.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnInventoryAudit.Size = new Size(200, 36);
            btnInventoryAudit.Location = new Point(30, 170);
            btnInventoryAudit.Click += btnInventoryAudit_Click;

            btnPromotions.Text = "Khuyen mai + Voucher";
            btnPromotions.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnPromotions.Size = new Size(200, 36);
            btnPromotions.Location = new Point(240, 170);
            btnPromotions.Click += btnPromotions_Click;

            btnInvoices.Text = "Lich su Hoa don";
            btnInvoices.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnInvoices.Size = new Size(200, 36);
            btnInvoices.Location = new Point(450, 170);
            btnInvoices.Click += btnInvoices_Click;

            // Big POS Button
            btnPos.Text = "POS BAN HANG";
            btnPos.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            btnPos.Size = new Size(300, 80);
            btnPos.Location = new Point(30, 230);
            btnPos.BackColor = Color.FromArgb(46, 125, 50);
            btnPos.ForeColor = Color.White;
            btnPos.FlatStyle = FlatStyle.Flat;
            btnPos.Click += btnPos_Click;

            Controls.Add(lblWelcome);
            Controls.Add(lblUser);
            Controls.Add(btnLogout);
            Controls.Add(btnSuppliers);
            Controls.Add(btnProducts);
            Controls.Add(btnPurchaseOrders);
            Controls.Add(btnInventoryAudit);
            Controls.Add(btnPromotions);
            Controls.Add(btnInvoices);
            Controls.Add(btnPos);

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

        private void btnPurchaseOrders_Click(object sender, EventArgs e)
        {
            using (var purchaseOrderModule = new PurchaseOrderModuleForm())
            {
                purchaseOrderModule.ShowDialog(this);
            }
        }

        private void btnInventoryAudit_Click(object sender, EventArgs e)
        {
            using (var inventoryAuditModule = new InventoryAuditModuleForm())
            {
                inventoryAuditModule.ShowDialog(this);
            }
        }

        private void btnPromotions_Click(object sender, EventArgs e)
        {
            using (var promotionModule = new PromotionModuleForm())
            {
                promotionModule.ShowDialog(this);
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
