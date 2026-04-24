using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SmartPos.Module.LichSuKiemXuat.Backend;
using SmartPos.Module.LichSuKiemXuat.Models;

namespace SmartPos.Module.LichSuKiemXuat.Views
{
    public class frmStockHistory : Form
    {
        private readonly HistoryBackend _backend;
        private TabControl tabControl;
        
        // --- Tab 0 Purchase ---
        private DataGridView dgvPurchase;
        private DataGridView dgvPurchaseDetails;
        private DateTimePicker dtpPurchaseFrom, dtpPurchaseTo;
        private ComboBox cboPurchaseUser;
        private TextBox txtPurchaseSearch;
        private Label lblPurchaseStats;
        private Panel pnlPurchaseDetail;

        // --- Tab 1 Stock Out ---
        private DataGridView dgvStockOut;
        private DataGridView dgvStockOutDetails;
        private DateTimePicker dtpOutFrom, dtpOutTo;
        private ComboBox cboOutReason, cboOutUser;
        private TextBox txtOutSearch;
        private Label lblOutStats;
        private Panel pnlOutDetail;

        // --- Tab 2 Audit ---
        private DataGridView dgvAudit;
        private DataGridView dgvAuditDetails;
        private DateTimePicker dtpAuditFrom, dtpAuditTo;
        private ComboBox cboAuditCategory, cboAuditUser;
        private TextBox txtAuditSearch;
        private Panel pnlAuditDetail;

        public frmStockHistory()
        {
            _backend = new HistoryBackend();
            InitializeUi();
            LoadData();

            // Set placeholders via Win32 API for .NET Framework
            Win32Helper.SetPlaceholder(txtPurchaseSearch, "Mã phiếu / Ghi chú...");
            Win32Helper.SetPlaceholder(txtOutSearch, "Mã phiếu / Ghi chú...");
            Win32Helper.SetPlaceholder(txtAuditSearch, "Mã phiếu kiểm kê...");
        }

        private void InitializeUi()
        {
            Text = "LỊCH SỬ NHẬP, XUẤT, KIỂM KÊ";
            Size = new Size(1300, 850);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(244, 247, 252);
            Font = new Font("Segoe UI", 10F);

            tabControl = new TabControl { Dock = DockStyle.Fill, Padding = new Point(15, 5) };
            
            var tabPurchase = new TabPage("📥 Lịch sử Nhập hàng");
            tabPurchase.Controls.Add(BuildPurchaseTab());

            var tabStockOut = new TabPage("📤 Lịch sử Xuất kho");
            tabStockOut.Controls.Add(BuildStockOutTab());
            
            var tabAudit = new TabPage("🔍 Lịch sử Kiểm kê");
            tabAudit.Controls.Add(BuildAuditTab());

            tabControl.TabPages.Add(tabPurchase);
            tabControl.TabPages.Add(tabStockOut);
            tabControl.TabPages.Add(tabAudit);

            Controls.Add(tabControl);

            // Export Panel at Bottom
            var pnlExport = new Panel { Dock = DockStyle.Bottom, Height = 50, BackColor = Color.White, Padding = new Padding(10) };
            var btnExcel = new Button { Text = "📥 Xuất Excel", Dock = DockStyle.Right, Width = 120, BackColor = Color.FromArgb(34, 197, 94), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            var btnPdf = new Button { Text = "📥 Xuất PDF", Dock = DockStyle.Right, Width = 120, Margin = new Padding(0,0,10,0) };
            pnlExport.Controls.Add(btnPdf);
            pnlExport.Controls.Add(btnExcel);
            Controls.Add(pnlExport);
        }

        private Control BuildPurchaseTab()
        {
            var root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1 };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 60));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 40));

            // 1. Filter Bar
            var pnlFilter = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(10) };
            dtpPurchaseFrom = new DateTimePicker { Location = new Point(10, 25), Width = 110, Format = DateTimePickerFormat.Short, Value = DateTime.Now.AddYears(-2) };
            dtpPurchaseTo = new DateTimePicker { Location = new Point(130, 25), Width = 110, Format = DateTimePickerFormat.Short };
            cboPurchaseUser = new ComboBox { Location = new Point(250, 25), Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
            txtPurchaseSearch = new TextBox { Location = new Point(440, 25), Width = 200 };
            var btnSearch = new Button { Text = "🔍 Tìm", Location = new Point(650, 20), Height = 35, Width = 80, BackColor = Color.FromArgb(59, 130, 246), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnSearch.Click += (s, e) => LoadPurchase();

            pnlFilter.Controls.Add(new Label { Text = "Thời gian", Location = new Point(10, 5), AutoSize = true, Font = new Font("Segoe UI", 8F, FontStyle.Bold) });
            pnlFilter.Controls.Add(dtpPurchaseFrom); pnlFilter.Controls.Add(dtpPurchaseTo);
            pnlFilter.Controls.Add(cboPurchaseUser);
            pnlFilter.Controls.Add(txtPurchaseSearch); pnlFilter.Controls.Add(btnSearch);

            lblPurchaseStats = new Label { Dock = DockStyle.Right, Width = 300, TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI", 9F, FontStyle.Italic) };
            pnlFilter.Controls.Add(lblPurchaseStats);

            // 2. Grid
            dgvPurchase = CreateGrid();
            dgvPurchase.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Mã phiếu", DataPropertyName = "POCode", Width = 120 });
            dgvPurchase.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Ngày nhập", DataPropertyName = "OrderDate", Width = 150, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy HH:mm" } });
            dgvPurchase.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Nhà CC", DataPropertyName = "SupplierName", Width = 150 });
            dgvPurchase.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Kho", DataPropertyName = "WarehouseName", Width = 150 });
            dgvPurchase.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "SL loại", DataPropertyName = "ItemCount", Width = 80 });
            dgvPurchase.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tổng tiền", DataPropertyName = "TotalAmount", Width = 120, DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" } });
            dgvPurchase.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Người nhập", DataPropertyName = "CreatedByName", Width = 120 });
            dgvPurchase.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tình trạng", DataPropertyName = "StatusText", Width = 100 });
            dgvPurchase.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Thanh toán", DataPropertyName = "PaymentStatusText", Width = 100 });
            dgvPurchase.SelectionChanged += DgvPurchase_SelectionChanged;

            // 3. Detail
            pnlPurchaseDetail = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(10) };
            dgvPurchaseDetails = CreateGrid();
            dgvPurchaseDetails.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Sản phẩm", DataPropertyName = "ProductName", Width = 200 });
            dgvPurchaseDetails.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "ĐVT", DataPropertyName = "UnitName", Width = 80 });
            dgvPurchaseDetails.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Lô", DataPropertyName = "BatchNumber", Width = 100 });
            dgvPurchaseDetails.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Kệ", DataPropertyName = "ShelfLocation", Width = 100 });
            dgvPurchaseDetails.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "HSD", DataPropertyName = "ExpiryDate", Width = 100, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy" } });
            dgvPurchaseDetails.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "SL", DataPropertyName = "Quantity", Width = 80, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
            dgvPurchaseDetails.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Giá nhập", DataPropertyName = "CostPrice", Width = 100, DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" } });
            dgvPurchaseDetails.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Thành tiền", DataPropertyName = "LineTotal", Width = 100, DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" } });
            pnlPurchaseDetail.Controls.Add(dgvPurchaseDetails);

            root.Controls.Add(pnlFilter, 0, 0);
            root.Controls.Add(dgvPurchase, 0, 1);
            root.Controls.Add(pnlPurchaseDetail, 0, 2);
            return root;
        }

        private Control BuildStockOutTab()
        {
            var root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1 };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 70)); // Filter
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 60));  // Grid
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 40));  // Detail

            // 1. Filter Bar
            var pnlFilter = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(10) };
            dtpOutFrom = new DateTimePicker { Location = new Point(10, 25), Width = 110, Format = DateTimePickerFormat.Short, Value = DateTime.Now.AddYears(-2) };
            dtpOutTo = new DateTimePicker { Location = new Point(130, 25), Width = 110, Format = DateTimePickerFormat.Short };
            cboOutReason = new ComboBox { Location = new Point(250, 25), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cboOutReason.Items.AddRange(new[] { "Tất cả", "Hư hỏng", "Hết hạn", "Điều chuyển", "Khác" });
            cboOutReason.SelectedIndex = 0;
            cboOutUser = new ComboBox { Location = new Point(410, 25), Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
            txtOutSearch = new TextBox { Location = new Point(600, 25), Width = 200 };
            var btnSearch = new Button { Text = "🔍 Tìm", Location = new Point(810, 20), Height = 35, Width = 80, BackColor = Color.FromArgb(59, 130, 246), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnSearch.Click += (s, e) => LoadStockOut();

            pnlFilter.Controls.Add(new Label { Text = "Thời gian", Location = new Point(10, 5), AutoSize = true, Font = new Font("Segoe UI", 8F, FontStyle.Bold) });
            pnlFilter.Controls.Add(dtpOutFrom); pnlFilter.Controls.Add(dtpOutTo);
            pnlFilter.Controls.Add(cboOutReason); pnlFilter.Controls.Add(cboOutUser);
            pnlFilter.Controls.Add(txtOutSearch); pnlFilter.Controls.Add(btnSearch);

            // Stats Panel
            lblOutStats = new Label { Dock = DockStyle.Right, Width = 300, TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI", 9F, FontStyle.Italic) };
            pnlFilter.Controls.Add(lblOutStats);

            // 2. Grid
            dgvStockOut = CreateGrid();
            dgvStockOut.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Mã phiếu", DataPropertyName = "StockOutCode", Width = 120 });
            dgvStockOut.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Ngày xuất", DataPropertyName = "StockOutDate", Width = 150, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy HH:mm" } });
            dgvStockOut.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Lý do", DataPropertyName = "Reason", Width = 120 });
            dgvStockOut.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Kho", DataPropertyName = "WarehouseName", Width = 180 });
            dgvStockOut.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "SL loại", DataPropertyName = "ItemCount", Width = 80 });
            dgvStockOut.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Người lập", DataPropertyName = "CreatedByName", Width = 150 });
            dgvStockOut.SelectionChanged += DgvStockOut_SelectionChanged;

            // 3. Detail
            pnlOutDetail = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(10) };
            dgvStockOutDetails = CreateGrid();
            dgvStockOutDetails.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Sản phẩm", DataPropertyName = "ProductName", Width = 200 });
            dgvStockOutDetails.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "ĐVT", DataPropertyName = "UnitName", Width = 80 });
            dgvStockOutDetails.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Lô", DataPropertyName = "BatchNumber", Width = 100 });
            dgvStockOutDetails.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Kệ", DataPropertyName = "ShelfLocation", Width = 100 });
            dgvStockOutDetails.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "HSD", DataPropertyName = "ExpiryDate", Width = 100, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy" } });
            dgvStockOutDetails.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "SL", DataPropertyName = "Quantity", Width = 80, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
            dgvStockOutDetails.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tồn trước", DataPropertyName = "StockBefore", Width = 100, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
            pnlOutDetail.Controls.Add(dgvStockOutDetails);

            root.Controls.Add(pnlFilter, 0, 0);
            root.Controls.Add(dgvStockOut, 0, 1);
            root.Controls.Add(pnlOutDetail, 0, 2);
            return root;
        }

        private Control BuildAuditTab()
        {
            var root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1 };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 45));

            // 1. Filter
            var pnlFilter = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(10) };
            dtpAuditFrom = new DateTimePicker { Location = new Point(10, 25), Width = 110, Format = DateTimePickerFormat.Short, Value = DateTime.Now.AddYears(-2) };
            dtpAuditTo = new DateTimePicker { Location = new Point(130, 25), Width = 110, Format = DateTimePickerFormat.Short };
            cboAuditCategory = new ComboBox { Location = new Point(250, 25), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cboAuditUser = new ComboBox { Location = new Point(410, 25), Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
            txtAuditSearch = new TextBox { Location = new Point(600, 25), Width = 200 };
            var btnSearch = new Button { Text = "🔍 Tìm", Location = new Point(810, 20), Height = 35, Width = 80, BackColor = Color.FromArgb(59, 130, 246), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnSearch.Click += (s, e) => LoadAudit();

            pnlFilter.Controls.Add(dtpAuditFrom); pnlFilter.Controls.Add(dtpAuditTo);
            pnlFilter.Controls.Add(cboAuditCategory); pnlFilter.Controls.Add(cboAuditUser);
            pnlFilter.Controls.Add(txtAuditSearch); pnlFilter.Controls.Add(btnSearch);

            // 2. Grid
            dgvAudit = CreateGrid();
            dgvAudit.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Mã phiếu", DataPropertyName = "CheckCode", Width = 120 });
            dgvAudit.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Ngày kiểm", DataPropertyName = "CheckDate", Width = 150, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy HH:mm" } });
            dgvAudit.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Nhà kho", DataPropertyName = "CategoryName", Width = 180 });
            dgvAudit.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Người kiểm", DataPropertyName = "AuditorName", Width = 150 });
            dgvAudit.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Khớp", DataPropertyName = "MatchCount", Width = 60 });
            dgvAudit.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Thừa", DataPropertyName = "OverCount", Width = 60 });
            dgvAudit.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Thiếu", DataPropertyName = "UnderCount", Width = 60 });
            dgvAudit.SelectionChanged += DgvAudit_SelectionChanged;

            // 3. Detail
            pnlAuditDetail = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(10) };
            dgvAuditDetails = CreateGrid();
            dgvAuditDetails.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Sản phẩm", DataPropertyName = "ProductName", Width = 300 });
            dgvAuditDetails.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "ĐVT", DataPropertyName = "UnitName", Width = 80 });
            dgvAuditDetails.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tồn HT", DataPropertyName = "SystemQuantity", Width = 100, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
            dgvAuditDetails.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Thực tế", DataPropertyName = "ActualQuantity", Width = 100, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
            dgvAuditDetails.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Chênh lệch", DataPropertyName = "Difference", Width = 100, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
            dgvAuditDetails.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Kết quả", DataPropertyName = "ResultText", Width = 100 });
            pnlAuditDetail.Controls.Add(dgvAuditDetails);

            root.Controls.Add(pnlFilter, 0, 0);
            root.Controls.Add(dgvAudit, 0, 1);
            root.Controls.Add(pnlAuditDetail, 0, 2);
            return root;
        }

        private DataGridView CreateGrid()
        {
            return new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                ReadOnly = true,
                AutoGenerateColumns = false,
                RowTemplate = { Height = 35 },
                EnableHeadersVisualStyles = false,
                ColumnHeadersHeight = 40,
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(241, 245, 249), Font = new Font("Segoe UI", 9F, FontStyle.Bold) }
            };
        }

        private void LoadData()
        {
            var users = _backend.GetUsers();
            var allUser = new UserLookup { UserID = 0, FullName = "--- Tất cả nhân viên ---" };
            
            cboPurchaseUser.Items.Add(allUser);
            users.ForEach(u => cboPurchaseUser.Items.Add(u));
            cboPurchaseUser.SelectedIndex = 0;

            cboOutUser.Items.Add(allUser);
            users.ForEach(u => cboOutUser.Items.Add(u));
            cboOutUser.SelectedIndex = 0;

            cboAuditUser.Items.Add(allUser);
            users.ForEach(u => cboAuditUser.Items.Add(u));
            cboAuditUser.SelectedIndex = 0;

            var cats = _backend.GetCategories();
            cboAuditCategory.Items.Add("--- Tất cả nhóm ---");
            cats.ForEach(c => cboAuditCategory.Items.Add(c));
            cboAuditCategory.SelectedIndex = 0;

            LoadPurchase();
            LoadStockOut();
            LoadAudit();
        }

        private void LoadPurchase()
        {
            var selectedUser = cboPurchaseUser.SelectedItem as UserLookup;
            int? userId = (selectedUser == null || selectedUser.UserID == 0) ? (int?)null : selectedUser.UserID;

            var list = _backend.GetPurchaseHistory(dtpPurchaseFrom.Value, dtpPurchaseTo.Value, userId, txtPurchaseSearch.Text);
            dgvPurchase.DataSource = list;

            var stats = _backend.GetPurchaseStats(dtpPurchaseFrom.Value, dtpPurchaseTo.Value);
            lblPurchaseStats.Text = $"Tổng phiếu: {stats.TotalVouchers} | Tổng tiền: {stats.TotalAmount:N0}";
        }

        private void DgvPurchase_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvPurchase.CurrentRow?.DataBoundItem is PurchaseHistoryListItem item)
            {
                dgvPurchaseDetails.DataSource = _backend.GetPurchaseDetails(item.PurchaseOrderID);
            }
        }

        private void LoadStockOut()
        {
            var selectedUser = cboOutUser.SelectedItem as UserLookup;
            int? userId = (selectedUser == null || selectedUser.UserID == 0) ? (int?)null : selectedUser.UserID;

            var list = _backend.GetStockOutHistory(dtpOutFrom.Value, dtpOutTo.Value, cboOutReason.Text, userId, txtOutSearch.Text);
            dgvStockOut.DataSource = list;

            var stats = _backend.GetStockOutStats(dtpOutFrom.Value, dtpOutTo.Value);
            lblOutStats.Text = $"Tổng phiếu: {stats.TotalVouchers} | Hư hỏng: {stats.DamageCount} | Hết hạn: {stats.ExpiredCount} | D.Chuyển: {stats.TransferCount}";
        }

        private void DgvStockOut_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvStockOut.CurrentRow?.DataBoundItem is StockOutHistoryListItem item)
            {
                dgvStockOutDetails.DataSource = _backend.GetStockOutDetails(item.StockOutID);
            }
        }

        private void LoadAudit()
        {
            var selectedUser = cboAuditUser.SelectedItem as UserLookup;
            int? userId = (selectedUser == null || selectedUser.UserID == 0) ? (int?)null : selectedUser.UserID;

            var list = _backend.GetAuditHistory(dtpAuditFrom.Value, dtpAuditTo.Value, null, userId, txtAuditSearch.Text);
            dgvAudit.DataSource = list;
        }

        private void DgvAudit_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvAudit.CurrentRow?.DataBoundItem is AuditHistoryListItem item)
            {
                dgvAuditDetails.DataSource = _backend.GetAuditDetails(item.CheckID);
            }
        }
    }

    internal static class Win32Helper
    {
        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);
        private const int EM_SETCUEBANNER = 0x1501;

        public static void SetPlaceholder(TextBox textBox, string placeholder)
        {
            SendMessage(textBox.Handle, EM_SETCUEBANNER, (IntPtr)1, placeholder);
        }
    }
}
