using System;
using System.Drawing;
using System.Windows.Forms;
using SmartPos.Module.Reports.Controllers;
using SmartPos.Module.Reports.Models;
using System.Collections.Generic;
using System.Linq;

namespace SmartPos.Module.Reports.Views
{
    public class frmProductReport : Form
    {
        private readonly ReportController _controller;
        private TabControl tabMain;
        private DataGridView dgvOverview, dgvLowStock, dgvExpiry, dgvBatch;
        private ComboBox cboWarehouse, cboStatus;
        private Button btnRefreshBatch;
        private List<BatchReportItem> _allBatches;

        public frmProductReport()
        {
            _controller = new ReportController();
            InitializeUi();
            LoadData();
        }

        private NumericUpDown numExpiryDays;
        private DateTimePicker dtpOverviewFrom, dtpOverviewTo;

        private void InitializeUi()
        {
            Text = "BÁO CÁO QUẢN LÝ SẢN PHẨM & TỒN KHO";
            Width = 1200;
            Height = 850;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(248, 250, 252);

            tabMain = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10F) };
            
            // --- Tab 1: Overview ---
            var tabOverview = new TabPage("TỔNG QUAN SP");
            var pnlOverviewTop = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.White };
            pnlOverviewTop.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlOverviewTop.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);
            
            var lblFrom = new Label { Text = "Từ:", Location = new Point(20, 22), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            dtpOverviewFrom = new DateTimePicker { Location = new Point(55, 18), Width = 120, Format = DateTimePickerFormat.Short };
            var lblTo = new Label { Text = "Đến:", Location = new Point(190, 22), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            dtpOverviewTo = new DateTimePicker { Location = new Point(230, 18), Width = 120, Format = DateTimePickerFormat.Short };
            var btnOverviewRefresh = new Button { Text = "XEM BÁO CÁO", Location = new Point(370, 15), Width = 120, Height = 32, BackColor = Color.FromArgb(51, 65, 85), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            btnOverviewRefresh.Click += (s, e) => LoadOverviewData();
            pnlOverviewTop.Controls.AddRange(new Control[] { lblFrom, dtpOverviewFrom, lblTo, dtpOverviewTo, btnOverviewRefresh });

            dgvOverview = CreateStyledGrid();
            tabOverview.Controls.Add(dgvOverview);
            tabOverview.Controls.Add(pnlOverviewTop);

            // --- Tab 2: Low Stock ---
            var tabLowStock = new TabPage("SẮP HẾT HÀNG");
            dgvLowStock = CreateStyledGrid();
            tabLowStock.Controls.Add(dgvLowStock);

            // --- Tab 3: Near Expiry ---
            var tabExpiry = new TabPage("SẮP HẾT HẠN");
            var pnlExpiryTop = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.White };
            pnlExpiryTop.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlExpiryTop.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);
            
            var lblExp = new Label { Text = "Hết hạn trong vòng (ngày):", Location = new Point(20, 22), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            numExpiryDays = new NumericUpDown { Location = new Point(180, 20), Width = 80, Value = 30, Minimum = 1, Maximum = 365, Font = new Font("Segoe UI", 10F) };
            var btnExpiryRefresh = new Button { Text = "LỌC DỮ LIỆU", Location = new Point(280, 15), Width = 120, Height = 32, BackColor = Color.FromArgb(59, 130, 246), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            btnExpiryRefresh.Click += (s, e) => LoadExpiryData();
            pnlExpiryTop.Controls.AddRange(new Control[] { lblExp, numExpiryDays, btnExpiryRefresh });

            dgvExpiry = CreateStyledGrid();
            tabExpiry.Controls.Add(dgvExpiry);
            tabExpiry.Controls.Add(pnlExpiryTop);

            // --- Tab 4: Batch & Expiry ---
            var tabBatch = new TabPage("LÔ & HẠN SỬ DỤNG");
            var pnlBatchTop = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.White };
            pnlBatchTop.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlBatchTop.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);
            
            cboWarehouse = new ComboBox { Width = 180, DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(20, 18), Font = new Font("Segoe UI", 10F) };
            cboWarehouse.Items.AddRange(new object[] { new { ID = 0, Name = "Tất cả kho" } });
            cboWarehouse.DisplayMember = "Name";
            cboWarehouse.ValueMember = "ID";
            cboWarehouse.SelectedIndex = 0;

            cboStatus = new ComboBox { Width = 180, DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(210, 18), Font = new Font("Segoe UI", 10F) };
            cboStatus.Items.AddRange(new string[] { "Tất cả trạng thái", "Còn hạn", "Cần chú ý", "Sắp hết", "Hết hạn" });
            cboStatus.SelectedIndex = 0;

            btnRefreshBatch = new Button { Text = "LÀM MỚI", Width = 120, Height = 32, Location = new Point(410, 15), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(16, 185, 129), ForeColor = Color.White, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            
            pnlBatchTop.Controls.AddRange(new Control[] { cboWarehouse, cboStatus, btnRefreshBatch });

            dgvBatch = CreateStyledGrid();
            dgvBatch.CellPainting += DgvBatch_CellPainting;

            tabBatch.Controls.Add(dgvBatch);
            tabBatch.Controls.Add(pnlBatchTop);

            tabMain.TabPages.AddRange(new TabPage[] { tabOverview, tabLowStock, tabExpiry, tabBatch });
            tabMain.SelectedIndexChanged += TabMain_SelectedIndexChanged;
            Controls.Add(tabMain);

            cboWarehouse.SelectedIndexChanged += (s, e) => LoadBatchData();
            cboStatus.SelectedIndexChanged += (s, e) => FilterBatchData();
            btnRefreshBatch.Click += (s, e) => LoadBatchData();

            dgvOverview.CellDoubleClick += (s, e) => {
                if (e.RowIndex >= 0)
                {
                    var productID = (int)dgvOverview.Rows[e.RowIndex].Cells["ProductID"].Value;
                    new SmartPos.Module.Products.Views.frmProductDetail(productID).ShowDialog();
                }
            };

            // Set default dates
            dtpOverviewFrom.Value = DateTime.Now.AddDays(-30);
            dtpOverviewTo.Value = DateTime.Now;
        }

        private DataGridView CreateStyledGrid()
        {
            var dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true,
                EnableHeadersVisualStyles = false,
                RowTemplate = { Height = 35 },
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                GridColor = Color.FromArgb(241, 245, 249),
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = 45
            };
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(51, 65, 85);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            return dgv;
        }

        private void LoadData()
        {
            LoadOverviewData();
            dgvLowStock.DataSource = _controller.GetLowStockAlert();
            FormatLowStockGrid();
            LoadExpiryData();
        }

        private void LoadOverviewData()
        {
            dgvOverview.DataSource = _controller.GetProductPerformance(dtpOverviewFrom.Value, dtpOverviewTo.Value);
            FormatOverviewGrid();
        }

        private void LoadExpiryData()
        {
            int days = (int)numExpiryDays.Value;
            dgvExpiry.DataSource = _controller.GetNearExpiryItems(days);
            FormatExpiryGrid();
        }

        private void TabMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabMain.SelectedTab.Text.Contains("LÔ & HẠN"))
            {
                LoadBatchData();
            }
        }

        private void LoadBatchData()
        {
            try
            {
                _allBatches = _controller.GetAllBatches(0);
                FilterBatchData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu lô: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FilterBatchData()
        {
            if (_allBatches == null) return;
            var filtered = _allBatches.AsEnumerable();
            
            string status = cboStatus.SelectedItem?.ToString();
            if (status == "Hết hạn") filtered = filtered.Where(x => x.DaysToExpiry <= 0);
            else if (status == "Sắp hết") filtered = filtered.Where(x => x.DaysToExpiry >= 1 && x.DaysToExpiry <= 7);
            else if (status == "Cần chú ý") filtered = filtered.Where(x => x.DaysToExpiry >= 8 && x.DaysToExpiry <= 30);
            else if (status == "Còn hạn") filtered = filtered.Where(x => x.DaysToExpiry > 30);

            dgvBatch.DataSource = filtered.ToList();
            FormatBatchGrid();
        }

        private void FormatBatchGrid()
        {
            if (dgvBatch.Columns["ProductID"] != null) dgvBatch.Columns["ProductID"].Visible = false;
            if (dgvBatch.Columns["ProductCode"] != null) dgvBatch.Columns["ProductCode"].HeaderText = "Mã SP";
            if (dgvBatch.Columns["ProductName"] != null) dgvBatch.Columns["ProductName"].HeaderText = "Tên SP";
            if (dgvBatch.Columns["BatchNumber"] != null) dgvBatch.Columns["BatchNumber"].HeaderText = "Số lô";
            if (dgvBatch.Columns["ManufactureDate"] != null) dgvBatch.Columns["ManufactureDate"].HeaderText = "NSX";
            if (dgvBatch.Columns["ExpiryDate"] != null) dgvBatch.Columns["ExpiryDate"].HeaderText = "HSD";
            if (dgvBatch.Columns["Quantity"] != null) dgvBatch.Columns["Quantity"].HeaderText = "Tồn";
            if (dgvBatch.Columns["ShelfLocation"] != null) dgvBatch.Columns["ShelfLocation"].HeaderText = "Kệ";
            if (dgvBatch.Columns["WarehouseName"] != null) dgvBatch.Columns["WarehouseName"].HeaderText = "Kho";
            if (dgvBatch.Columns["DaysToExpiry"] != null) dgvBatch.Columns["DaysToExpiry"].Visible = false;

            if (dgvBatch.Columns["Quantity"] != null) dgvBatch.Columns["Quantity"].DefaultCellStyle.Format = "N0";
            if (dgvBatch.Columns["ExpiryDate"] != null) dgvBatch.Columns["ExpiryDate"].DefaultCellStyle.Format = "dd/MM/yyyy";

            if (dgvBatch.Columns["StatusBadge"] == null)
            {
                dgvBatch.Columns.Add(new DataGridViewTextBoxColumn { Name = "StatusBadge", HeaderText = "Trạng thái", ReadOnly = true });
            }
        }

        private void DgvBatch_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && dgvBatch.Columns[e.ColumnIndex].Name == "StatusBadge")
            {
                e.PaintBackground(e.CellBounds, true);

                var row = dgvBatch.Rows[e.RowIndex];
                if (row.DataBoundItem is BatchReportItem item)
                {
                    string text = ""; Color bgColor = Color.White; Color fgColor = Color.White;
                    if (item.DaysToExpiry <= 0) { text = "Hết hạn"; bgColor = ColorTranslator.FromHtml("#EF4444"); }
                    else if (item.DaysToExpiry <= 7) { text = "Sắp hết"; bgColor = ColorTranslator.FromHtml("#F59E0B"); }
                    else if (item.DaysToExpiry <= 30) { text = "Chú ý"; bgColor = ColorTranslator.FromHtml("#10B981"); }
                    else { text = "An toàn"; bgColor = ColorTranslator.FromHtml("#3B82F6"); }

                    var badgeRect = new Rectangle(e.CellBounds.X + 4, e.CellBounds.Y + 4, e.CellBounds.Width - 8, e.CellBounds.Height - 8);
                    using (Brush bgBrush = new SolidBrush(bgColor))
                    {
                        e.Graphics.FillRectangle(bgBrush, badgeRect);
                    }

                    TextRenderer.DrawText(e.Graphics, text, e.CellStyle.Font, badgeRect, fgColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    e.Handled = true;
                }
            }
        }

        private void FormatOverviewGrid()
        {
            if (dgvOverview.Columns["ProductID"] != null) dgvOverview.Columns["ProductID"].Visible = false;
            if (dgvOverview.Columns["ProductCode"] != null) dgvOverview.Columns["ProductCode"].HeaderText = "Mã SP";
            if (dgvOverview.Columns["ProductName"] != null) dgvOverview.Columns["ProductName"].HeaderText = "Tên sản phẩm";
            if (dgvOverview.Columns["SoldQuantity"] != null) dgvOverview.Columns["SoldQuantity"].HeaderText = "Đã bán";
            if (dgvOverview.Columns["Revenue"] != null) dgvOverview.Columns["Revenue"].HeaderText = "Doanh thu";
            if (dgvOverview.Columns["CostPrice"] != null) dgvOverview.Columns["CostPrice"].HeaderText = "Giá vốn";
            if (dgvOverview.Columns["CurrentStock"] != null) dgvOverview.Columns["CurrentStock"].HeaderText = "Tồn kho";
            if (dgvOverview.Columns["StockValue"] != null) dgvOverview.Columns["StockValue"].HeaderText = "Giá trị tồn";
            if (dgvOverview.Columns["MinStockAlert"] != null) dgvOverview.Columns["MinStockAlert"].HeaderText = "Cảnh báo";
            
            if (dgvOverview.Columns["Revenue"] != null) dgvOverview.Columns["Revenue"].DefaultCellStyle.Format = "N0";
            if (dgvOverview.Columns["CostPrice"] != null) dgvOverview.Columns["CostPrice"].DefaultCellStyle.Format = "N0";
            if (dgvOverview.Columns["StockValue"] != null) dgvOverview.Columns["StockValue"].DefaultCellStyle.Format = "N0";
            if (dgvOverview.Columns["SoldQuantity"] != null) dgvOverview.Columns["SoldQuantity"].DefaultCellStyle.Format = "N0";
            if (dgvOverview.Columns["CurrentStock"] != null) dgvOverview.Columns["CurrentStock"].DefaultCellStyle.Format = "N0";
        }

        private void FormatLowStockGrid()
        {
            if (dgvLowStock.Columns["ProductCode"] != null) dgvLowStock.Columns["ProductCode"].HeaderText = "Mã SP";
            if (dgvLowStock.Columns["ProductName"] != null) dgvLowStock.Columns["ProductName"].HeaderText = "Tên sản phẩm";
            if (dgvLowStock.Columns["CurrentStock"] != null) dgvLowStock.Columns["CurrentStock"].HeaderText = "Tồn kho hiện tại";
            if (dgvLowStock.Columns["MinStockLevel"] != null) dgvLowStock.Columns["MinStockLevel"].HeaderText = "Mức tồn tối thiểu";
            
            if (dgvLowStock.Columns["CurrentStock"] != null) dgvLowStock.Columns["CurrentStock"].DefaultCellStyle.Format = "N0";
            if (dgvLowStock.Columns["MinStockLevel"] != null) dgvLowStock.Columns["MinStockLevel"].DefaultCellStyle.Format = "N0";
        }

        private void FormatExpiryGrid()
        {
            if (dgvExpiry.Columns["ProductID"] != null) dgvExpiry.Columns["ProductID"].Visible = false;
            if (dgvExpiry.Columns["ProductCode"] != null) dgvExpiry.Columns["ProductCode"].HeaderText = "Mã SP";
            if (dgvExpiry.Columns["ProductName"] != null) dgvExpiry.Columns["ProductName"].HeaderText = "Tên sản phẩm";
            if (dgvExpiry.Columns["CurrentStock"] != null) dgvExpiry.Columns["CurrentStock"].HeaderText = "Tồn kho";
            if (dgvExpiry.Columns["ExpiryDate"] != null) 
            {
                dgvExpiry.Columns["ExpiryDate"].HeaderText = "Ngày hết hạn";
                dgvExpiry.Columns["ExpiryDate"].DefaultCellStyle.Format = "dd/MM/yyyy";
            }
            if (dgvExpiry.Columns["CurrentStock"] != null) dgvExpiry.Columns["CurrentStock"].DefaultCellStyle.Format = "N0";
        }
    }
}
