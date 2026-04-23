using System;
using System.Drawing;
using System.Windows.Forms;
using SmartPos.Module.Pos.Views;
using SmartPos.Module.Products.Views;
using SmartPos.Module.Promotions.Views;
using SmartPos.Module.InventoryAudit.Views;
using SmartPos.Module.PurchaseOrders.Views;
using SmartPos.Module.Suppliers.Views;

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
                Padding = new Padding(0, 20, 0, 0)
            };

            AddMenuButton("🏠 Dashboard", "Dashboard", true);
            AddMenuButton("🛒 Bán hàng (POS)", "POS", UserSession.IsCashier);
            AddMenuButton("📦 Sản phẩm", "Products", UserSession.IsManager);
            AddMenuButton("🏭 Nhà cung cấp", "Suppliers", UserSession.IsManager);
            AddMenuButton("📥 Nhập hàng", "PurchaseOrders", UserSession.IsManager);
            AddMenuButton("🔍 Kiểm kê kho", "Inventory", UserSession.IsManager);
            AddMenuButton("🎫 Khuyến mãi", "Promotions", UserSession.IsManager);
            AddMenuButton("📊 Báo cáo lợi nhuận", "Reports", UserSession.IsAdmin);
            AddMenuButton("👥 Quản lý nhân viên", "Users", UserSession.IsAdmin);

            // 2. Header
            panelHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            lblUserNav = new Label { Text = UserSession.CurrentUser?.FullName, Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(20, 15), AutoSize = true };
            lblRoleNav = new Label { Text = UserSession.CurrentUser?.Role.ToString(), Font = new Font("Segoe UI", 8), ForeColor = Color.Gray, Location = new Point(20, 38), AutoSize = true };
            
            lblTime = new Label { Text = DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy"), Font = new Font("Segoe UI", 10), Location = new Point(400, 25), AutoSize = true, Anchor = AnchorStyles.Top };

            Button btnLogout = new Button
            {
                Text = "Đăng xuất",
                Size = new Size(100, 35),
                Location = new Point(panelHeader.Width - 120, 17),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(255, 82, 82),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.Click += (s, e) => { UserSession.Clear(); this.Close(); };

            panelHeader.Controls.Add(lblUserNav);
            panelHeader.Controls.Add(lblRoleNav);
            panelHeader.Controls.Add(lblTime);
            panelHeader.Controls.Add(btnLogout);

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
