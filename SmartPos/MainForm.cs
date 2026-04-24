using System;
using System.Drawing;
using System.Windows.Forms;
using SmartPos.Module.Pos;
using SmartPos.Module.Customers.Views;
using SmartPos.Module.SalesHistory.Views;
using SmartPos.Module.Products.Views;
using SmartPos.Module.Promotions.Views;
using SmartPos.Module.InventoryAudit.Views;
using SmartPos.Module.PurchaseOrders.Views;
using SmartPos.Module.Suppliers.Views;
using SmartPos.Module.Reports.Views;
using SmartPos.Module.Loyalty.Views;


namespace SmartPos
{
    public partial class MainForm : Form
    {
        private Panel panelSidebar;
        private Panel panelHeader;
        private Panel panelContent;
        private Label lblUserNav;
        private Label lblRoleNav;
        private Label lblTime;
        private Timer timerClock;

        public MainForm()
        {
            InitializeUiCustom();
            SetupTimer();
            LoadDefaultModule();
        }

        private void InitializeUiCustom()
        {
            this.Text = "SmartPOS - Hệ thống Quản lý Bán hàng";
            this.Size = new Size(1300, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(244, 247, 252);

            // 1. Sidebar
            panelSidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 240,
                BackColor = Color.FromArgb(33, 43, 54),
                Padding = new Padding(0, 20, 0, 0),
                AutoScroll = true
            };

            AddMenuButton("🏠 Dashboard", "Dashboard", true);
            AddMenuButton("🛒 Bán hàng (POS)", "POS", UserSession.IsCashier);
            AddMenuButton("📜 Lịch sử bán hàng", "SalesHistory", UserSession.IsCashier);
            AddMenuButton("📦 Sản phẩm", "Products", UserSession.IsManager);
            AddMenuButton("🏭 Nhà cung cấp", "Suppliers", UserSession.IsManager);
            AddMenuButton("📥 Nhập hàng", "PurchaseOrders", UserSession.IsManager);
            AddMenuButton("🔍 Kiểm kê kho", "Inventory", UserSession.IsManager);
            AddMenuButton("🎫 Khuyến mãi", "Promotions", UserSession.IsManager);
            AddMenuButton("👤 Khách hàng", "Customers", true);
            AddMenuButton("💖 Khách hàng thân thiết", "Loyalty", true);
            AddMenuButton("📊 Báo cáo lợi nhuận", "ProfitReport", UserSession.IsAdmin);
            AddMenuButton("👥 Báo cáo khách hàng", "CustomerReport", UserSession.IsAdmin);
            AddMenuButton("📦 Báo cáo sản phẩm", "ProductReport", UserSession.IsAdmin);
            AddMenuButton("💰 Báo cáo doanh thu", "RevenueReport", UserSession.IsAdmin);
            AddMenuButton("📊 Thống kê & Báo cáo", "Reports", UserSession.IsAdmin);
            AddMenuButton("👥 Quản lý nhân viên", "Users", UserSession.IsAdmin);


            // 2. Header
            panelHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            Button btnToggleSidebar = new Button
            {
                Text = "☰",
                Size = new Size(40, 40),
                Location = new Point(10, 15),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(33, 43, 54),
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnToggleSidebar.FlatAppearance.BorderSize = 0;
            btnToggleSidebar.Click += (s, e) => {
                panelSidebar.Visible = !panelSidebar.Visible;
            };

            lblUserNav = new Label { Text = UserSession.CurrentUser?.FullName, Font = new Font("Segoe UI", 11, FontStyle.Bold), Location = new Point(60, 15), AutoSize = true };
            lblRoleNav = new Label { Text = "Chức vụ: " + (UserSession.CurrentUser?.Role.ToString() ?? "Admin"), Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(100, 116, 139), Location = new Point(60, 38), AutoSize = true };
            
            // Container cho phần bên phải của header
            var pnlRight = new Panel
            {
                Height = panelHeader.Height,
                Width = 400,
                Dock = DockStyle.Right,
                BackColor = Color.Transparent
            };

            lblTime = new Label 
            { 
                Text = DateTime.Now.ToString("HH:mm:ss  dd/MM/yyyy"), 
                Font = new Font("Consolas", 12, FontStyle.Bold), 
                ForeColor = Color.FromArgb(30, 41, 59),
                TextAlign = ContentAlignment.MiddleRight,
                AutoSize = false,
                Width = 250,
                Height = 70,
                Location = new Point(20, 0)
            };

            Button btnLogout = new Button
            {
                Text = "Đăng xuất",
                Size = new Size(110, 40),
                Location = new Point(280, 15),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(239, 68, 68), // Đỏ hiện đại
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.Click += (s, e) => { UserSession.Clear(); this.Close(); };

            pnlRight.Controls.Add(lblTime);
            pnlRight.Controls.Add(btnLogout);

            panelHeader.Controls.Add(btnToggleSidebar);
            panelHeader.Controls.Add(lblUserNav);
            panelHeader.Controls.Add(lblRoleNav);
            panelHeader.Controls.Add(pnlRight);

            // 3. Content Area
            panelContent = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            this.Controls.Add(panelContent);
            this.Controls.Add(panelHeader);
            this.Controls.Add(panelSidebar);
        }

        private void AddMenuButton(string text, string tag, bool visible)
        {
            if (!visible) return;

            Button btn = new Button
            {
                Text = "    " + text,
                Tag = tag,
                Dock = DockStyle.Top,
                Height = 50,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(145, 158, 171),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(53, 63, 74);
            
            btn.Click += MenuButton_Click;
            panelSidebar.Controls.Add(btn);
            btn.BringToFront(); // Để Dashboard ở trên cùng
        }

        private void MenuButton_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            string module = btn.Tag.ToString();
            
            // Highlight button
            foreach (Control c in panelSidebar.Controls)
            {
                if (c is Button b)
                {
                    b.BackColor = Color.Transparent;
                    b.ForeColor = Color.FromArgb(145, 158, 171);
                }
            }
            btn.BackColor = Color.FromArgb(25, 118, 210);
            btn.ForeColor = Color.White;

            LoadModule(module);
        }

        private void LoadModule(string moduleName)
        {
            panelContent.Controls.Clear();
            
            Control moduleControl = null;

            switch (moduleName.ToLower())
            {
                case "promotions":
                    moduleControl = new PromotionModuleForm { TopLevel = false, FormBorderStyle = FormBorderStyle.None, Dock = DockStyle.Fill };
                    break;
                case "inventory":
                    moduleControl = new InventoryAuditModuleForm { TopLevel = false, FormBorderStyle = FormBorderStyle.None, Dock = DockStyle.Fill };
                    break;
                case "purchaseorders":
                    moduleControl = new PurchaseOrderModuleForm { TopLevel = false, FormBorderStyle = FormBorderStyle.None, Dock = DockStyle.Fill };
                    break;
                case "suppliers":
                    moduleControl = new SupplierModuleForm { TopLevel = false, FormBorderStyle = FormBorderStyle.None, Dock = DockStyle.Fill };
                    break;
                case "users":
                    if (UserSession.IsAdmin)
                        moduleControl = new UserManagementModuleControl();
                    break;
                case "products":
                    moduleControl = new ProductModuleForm { TopLevel = false, FormBorderStyle = FormBorderStyle.None, Dock = DockStyle.Fill };
                    break;
                case "pos":
                    moduleControl = new PosModuleForm { TopLevel = false, FormBorderStyle = FormBorderStyle.None, Dock = DockStyle.Fill };
                    break;
                case "customers":
                    moduleControl = new CustomerModuleForm { TopLevel = false, FormBorderStyle = FormBorderStyle.None, Dock = DockStyle.Fill };
                    break;
                case "loyalty":
                    moduleControl = new LoyaltyManagementForm { TopLevel = false, FormBorderStyle = FormBorderStyle.None, Dock = DockStyle.Fill };
                    break;
                case "saleshistory":
                    moduleControl = new frmOrderManagement { TopLevel = false, FormBorderStyle = FormBorderStyle.None, Dock = DockStyle.Fill };
                    break;
                case "dashboard":
                    moduleControl = new frmDashboard { TopLevel = false, FormBorderStyle = FormBorderStyle.None, Dock = DockStyle.Fill };
                    break;
                case "revenuereport":
                    moduleControl = new frmRevenueReport { TopLevel = false, FormBorderStyle = FormBorderStyle.None, Dock = DockStyle.Fill };
                    break;
                case "productreport":
                    moduleControl = new frmProductReport { TopLevel = false, FormBorderStyle = FormBorderStyle.None, Dock = DockStyle.Fill };
                    break;
                case "customerreport":
                    moduleControl = new frmCustomerReport { TopLevel = false, FormBorderStyle = FormBorderStyle.None, Dock = DockStyle.Fill };
                    break;
                case "profitreport":
                    moduleControl = new frmProfitReport { TopLevel = false, FormBorderStyle = FormBorderStyle.None, Dock = DockStyle.Fill };
                    break;
                case "reports":
                    // Redirect to dashboard as general entry for reports
                    moduleControl = new frmDashboard { TopLevel = false, FormBorderStyle = FormBorderStyle.None, Dock = DockStyle.Fill };
                    break;
                default:
                    moduleControl = CreatePlaceholder(moduleName);
                    break;
            }

            if (moduleControl != null)
            {
                if (moduleControl is Form f) f.Show();
                panelContent.Controls.Add(moduleControl);
            }
        }

        private Control CreatePlaceholder(string name)
        {
            Label lbl = new Label
            {
                Text = $"Module [{name}] đang được phát triển...",
                Font = new Font("Segoe UI", 14, FontStyle.Italic),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            return lbl;
        }

        private void SetupTimer()
        {
            timerClock = new Timer { Interval = 1000 };
            timerClock.Tick += (s, e) => lblTime.Text = DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy");
            timerClock.Start();
        }

        private void LoadDefaultModule()
        {
            LoadModule("Dashboard");
        }
    }

}
