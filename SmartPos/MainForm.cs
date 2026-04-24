using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
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
using SmartPos.Module.XuatHang.Views;
using SmartPos.Module.LichSuKiemXuat.Views;


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
        private bool isSidebarCollapsed = false;
        private const int SIDEBAR_WIDTH_EXPANDED = 260;
        private const int SIDEBAR_WIDTH_COLLAPSED = 70;

        private class MenuSubItem
        {
            public string Text { get; set; }
            public string Tag { get; set; }
            public bool Visible { get; set; }
            public string Badge { get; set; }
            public MenuSubItem(string text, string tag, bool visible, string badge = null)
            {
                Text = text;
                Tag = tag;
                Visible = visible;
                Badge = badge;
            }
        }

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
                Width = SIDEBAR_WIDTH_EXPANDED,
                BackColor = Color.FromArgb(15, 23, 42), // Slate 900
                Padding = new Padding(0, 0, 0, 0),
                AutoScroll = false // Disable scroll to manage collapse better
            };

            // Sidebar Header / Logo
            Panel pnlSidebarHeader = new Panel { Dock = DockStyle.Top, Height = 70, Padding = new Padding(15, 0, 15, 0) };
            Label lblLogoIcon = new Label { Text = "🚀", Dock = DockStyle.Left, Width = 40, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 18), ForeColor = Color.White };
            Label lblLogoText = new Label { Text = "SmartPOS", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.White };
            pnlSidebarHeader.Controls.Add(lblLogoText);
            pnlSidebarHeader.Controls.Add(lblLogoIcon);
            panelSidebar.Controls.Add(pnlSidebarHeader);

            AddMenuButton("🏠 Dashboard", "Dashboard", true);

            // 1. Giao dịch
            AddMenuDropdown("Giao dịch", "💳", new List<MenuSubItem>
            {
                new MenuSubItem("Bán hàng (POS)", "POS", UserSession.IsCashier),
                new MenuSubItem("Lịch sử bán hàng", "SalesHistory", UserSession.IsCashier)
            });

            // 2. Kho hàng
            AddMenuDropdown("Kho hàng", "📦", new List<MenuSubItem>
            {
                new MenuSubItem("Sản phẩm", "Products", UserSession.IsManager),
                new MenuSubItem("Nhập hàng", "PurchaseOrders", UserSession.IsManager),
                new MenuSubItem("Xuất hàng", "StockOut", UserSession.IsManager),
                new MenuSubItem("Kiểm kê kho", "Inventory", UserSession.IsManager),
                new MenuSubItem("Lịch sử kiểm xuất", "StockHistory", UserSession.IsManager),
                new MenuSubItem("Nhà cung cấp (NCC)", "Suppliers", UserSession.IsManager)
            });

            // 3. Marketing
            AddMenuDropdown("Marketing", "📢", new List<MenuSubItem>
            {
                new MenuSubItem("Khuyến mãi", "Promotions", UserSession.IsManager),
                new MenuSubItem("Khách hàng thân thiết", "Loyalty", true)
            });

            // 4. Báo cáo & Thống kê
            AddMenuDropdown("Báo cáo & Thống kê", "📊", new List<MenuSubItem>
            {
                new MenuSubItem("Báo cáo lợi nhuận", "ProfitReport", UserSession.IsAdmin),
                new MenuSubItem("Báo cáo khách hàng", "CustomerReport", UserSession.IsAdmin),
                new MenuSubItem("Báo cáo sản phẩm", "ProductReport", UserSession.IsAdmin),
                new MenuSubItem("Báo cáo doanh thu", "RevenueReport", UserSession.IsAdmin),
                new MenuSubItem("Thống kê báo cáo", "Reports", UserSession.IsAdmin)
            });

            // 5. Tài khoản
            AddMenuDropdown("Tài khoản", "👤", new List<MenuSubItem>
            {
                new MenuSubItem("Quản lý nhân viên", "Users", UserSession.IsAdmin),
                new MenuSubItem("Quản lý khách hàng", "Customers", true)
            });


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
                isSidebarCollapsed = !isSidebarCollapsed;
                panelSidebar.Width = isSidebarCollapsed ? SIDEBAR_WIDTH_COLLAPSED : SIDEBAR_WIDTH_EXPANDED;
                lblLogoText.Visible = !isSidebarCollapsed;
                
                foreach (Control c in panelSidebar.Controls)
                {
                    if (c.Tag?.ToString() == "MenuButton" || c.Tag?.ToString() == "MenuGroup")
                    {
                        foreach (Control child in c.Controls)
                        {
                            if (child.Name == "lblText") child.Visible = !isSidebarCollapsed;
                        }
                    }
                    if (c is Panel p && p.Name == "pnlSubMenu") p.Visible = false; // Close all on toggle
                }
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
            btnLogout.Click += (s, e) => { 
                UserSession.Clear(); 
                this.Tag = "Logout"; 
                this.Close(); 
            };

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

            Panel pnl = new Panel { Dock = DockStyle.Top, Height = 50, Cursor = Cursors.Hand, Tag = "MenuButton", BackColor = Color.Transparent };
            pnl.AccessibleDescription = tag; // Use for loading module

            string icon = "";
            string label = text;
            if (text.Contains(" "))
            {
                var parts = text.Split(new[] { ' ' }, 2);
                icon = parts[0];
                label = parts[1];
            }

            Panel pnlIndicator = new Panel { Width = 4, Dock = DockStyle.Left, BackColor = Color.FromArgb(59, 130, 246), Visible = false, Name = "pnlIndicator" };
            Label lblIcon = new Label { Text = icon, Dock = DockStyle.Left, Width = SIDEBAR_WIDTH_COLLAPSED - 4, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.FromArgb(148, 163, 184), Font = new Font("Segoe UI", 12), Cursor = Cursors.Hand };
            Label lblText = new Label { Text = label, Name = "lblText", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, ForeColor = Color.FromArgb(148, 163, 184), Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand };

            pnl.Controls.Add(lblText);
            pnl.Controls.Add(lblIcon);
            pnl.Controls.Add(pnlIndicator);

            Action click = () => { ResetMenuSelection(panelSidebar); pnl.BackColor = Color.FromArgb(30, 41, 59); pnlIndicator.Visible = true; lblText.ForeColor = Color.White; lblIcon.ForeColor = Color.White; LoadModule(tag); };
            pnl.Click += (s, e) => click();
            lblIcon.Click += (s, e) => click();
            lblText.Click += (s, e) => click();

            pnl.MouseEnter += (s, e) => { if (!pnlIndicator.Visible) pnl.BackColor = Color.FromArgb(30, 41, 59); };
            pnl.MouseLeave += (s, e) => { if (!pnlIndicator.Visible) pnl.BackColor = Color.Transparent; };

            panelSidebar.Controls.Add(pnl);
            pnl.BringToFront();
        }

        private void AddMenuDropdown(string groupText, string icon, List<MenuSubItem> items)
        {
            var visibleItems = items.Where(i => i.Visible).ToList();
            if (visibleItems.Count == 0) return;

            Panel pnlSubMenu = new Panel { Name = "pnlSubMenu", Dock = DockStyle.Top, AutoSize = true, Visible = false, BackColor = Color.FromArgb(15, 23, 42) };

            Panel pnlGroup = new Panel { Dock = DockStyle.Top, Height = 50, Cursor = Cursors.Hand, Tag = "MenuGroup", BackColor = Color.Transparent };
            Label lblIcon = new Label { Text = icon, Dock = DockStyle.Left, Width = SIDEBAR_WIDTH_COLLAPSED, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.FromArgb(148, 163, 184), Font = new Font("Segoe UI", 12), Cursor = Cursors.Hand };
            Label lblText = new Label { Text = groupText, Name = "lblText", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, ForeColor = Color.FromArgb(148, 163, 184), Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand };

            pnlGroup.Controls.Add(lblText);
            pnlGroup.Controls.Add(lblIcon);

            Action toggle = () => {
                if (isSidebarCollapsed) {
                    isSidebarCollapsed = false;
                    panelSidebar.Width = SIDEBAR_WIDTH_EXPANDED;
                    foreach (Control c in panelSidebar.Controls) if (c.Tag != null) foreach (Control child in c.Controls) if (child.Name == "lblText") child.Visible = true;
                }
                pnlSubMenu.Visible = !pnlSubMenu.Visible;
                lblText.ForeColor = pnlSubMenu.Visible ? Color.White : Color.FromArgb(148, 163, 184);
            };

            pnlGroup.Click += (s, e) => toggle();
            lblIcon.Click += (s, e) => toggle();
            lblText.Click += (s, e) => toggle();

            foreach (var item in visibleItems)
            {
                Panel pnlSub = new Panel { Dock = DockStyle.Top, Height = 40, Cursor = Cursors.Hand, Tag = "MenuButton", BackColor = Color.Transparent };
                pnlSub.AccessibleDescription = item.Tag;
                
                Panel pnlInd = new Panel { Width = 4, Dock = DockStyle.Left, BackColor = Color.FromArgb(59, 130, 246), Visible = false, Name = "pnlIndicator" };
                Label lblSubIcon = new Label { Text = "•", Dock = DockStyle.Left, Width = SIDEBAR_WIDTH_COLLAPSED - 4, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.FromArgb(100, 116, 139), Font = new Font("Segoe UI", 12), Cursor = Cursors.Hand };
                Label lblSubText = new Label { Text = item.Text, Name = "lblText", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, ForeColor = Color.FromArgb(148, 163, 184), Font = new Font("Segoe UI", 9), Cursor = Cursors.Hand };
                
                if (!string.IsNullOrEmpty(item.Badge))
                {
                    Label lblBadge = new Label { 
                        Text = item.Badge, 
                        Dock = DockStyle.Right, 
                        Width = 30, 
                        Height = 20, 
                        BackColor = Color.FromArgb(239, 68, 68), 
                        ForeColor = Color.White, 
                        Font = new Font("Segoe UI", 7, FontStyle.Bold), 
                        TextAlign = ContentAlignment.MiddleCenter,
                        Margin = new Padding(0, 10, 10, 10)
                    };
                    pnlSub.Controls.Add(lblBadge);
                }

                pnlSub.Controls.Add(lblSubText);
                pnlSub.Controls.Add(lblSubIcon);
                pnlSub.Controls.Add(pnlInd);

                Action subClick = () => { ResetMenuSelection(panelSidebar); pnlSub.BackColor = Color.FromArgb(30, 41, 59); pnlInd.Visible = true; lblSubText.ForeColor = Color.White; LoadModule(item.Tag); };
                pnlSub.Click += (s, e) => subClick();
                lblSubText.Click += (s, e) => subClick();

                pnlSubMenu.Controls.Add(pnlSub);
                pnlSub.BringToFront();
            }

            panelSidebar.Controls.Add(pnlSubMenu);
            pnlSubMenu.BringToFront();
            panelSidebar.Controls.Add(pnlGroup);
            pnlGroup.BringToFront();
        }

        private void ResetMenuSelection(Control parent)
        {
            foreach (Control c in parent.Controls)
            {
                if (c.Tag?.ToString() == "MenuButton")
                {
                    c.BackColor = Color.Transparent;
                    foreach (Control child in c.Controls)
                    {
                        if (child.Name == "pnlIndicator") child.Visible = false;
                        if (child is Label l) l.ForeColor = Color.FromArgb(148, 163, 184);
                    }
                }
                if (c.HasChildren) ResetMenuSelection(c);
            }
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
                case "stockout":
                    moduleControl = new frmStockOut { TopLevel = false, FormBorderStyle = FormBorderStyle.None, Dock = DockStyle.Fill };
                    break;
                case "stockhistory":
                    moduleControl = new frmStockHistory { TopLevel = false, FormBorderStyle = FormBorderStyle.None, Dock = DockStyle.Fill };
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
