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

        private void InitializeUi()
        {
            Text = "Báo cáo Sản phẩm";
            Width = 1000;
            Height = 700;
            StartPosition = FormStartPosition.CenterParent;

            tabMain = new TabControl { Dock = DockStyle.Fill };
            
            var tabOverview = new TabPage("Tổng quan SP");
            dgvOverview = CreateGrid();
            tabOverview.Controls.Add(dgvOverview);

            var tabLowStock = new TabPage("Sắp hết hàng");
            dgvLowStock = CreateGrid();
            tabLowStock.Controls.Add(dgvLowStock);

            var tabExpiry = new TabPage("Sắp hết hạn");
            dgvExpiry = CreateGrid();
            tabExpiry.Controls.Add(dgvExpiry);

            // Tab 4: Lô & Hạn Sử Dụng
            var tabBatch = new TabPage("Lô & Hạn Sử Dụng");
            var pnlTopBatch = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(10) };
            
            cboWarehouse = new ComboBox { Width = 150, DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(10, 12) };
            // Fake data or load from somewhere. Just fake "Tất cả kho" for now.
            cboWarehouse.Items.AddRange(new object[] { new { ID = 0, Name = "Tất cả kho" } });
            cboWarehouse.DisplayMember = "Name";
            cboWarehouse.ValueMember = "ID";
            cboWarehouse.SelectedIndex = 0;

            cboStatus = new ComboBox { Width = 150, DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(170, 12) };
            cboStatus.Items.AddRange(new string[] { "Tất cả", "Còn hạn", "Cần chú ý", "Sắp hết", "Hết hạn" });
            cboStatus.SelectedIndex = 0;

            btnRefreshBatch = new Button { Text = "🔄 Làm mới", Width = 100, Location = new Point(330, 10), FlatStyle = FlatStyle.Flat };
            
            pnlTopBatch.Controls.Add(cboWarehouse);
            pnlTopBatch.Controls.Add(cboStatus);
            pnlTopBatch.Controls.Add(btnRefreshBatch);

            dgvBatch = CreateGrid();
            dgvBatch.CellPainting += DgvBatch_CellPainting;

            tabBatch.Controls.Add(dgvBatch);
            tabBatch.Controls.Add(pnlTopBatch);

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
                    new frmProductDetail(productID).ShowDialog();
                }
            };
        }

        private DataGridView CreateGrid()
        {
            return new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true
            };
        }

        private void LoadData()
        {
            dgvOverview.DataSource = _controller.GetProductPerformance(DateTime.Now.AddDays(-30), DateTime.Now);
            FormatOverviewGrid();
            
            dgvLowStock.DataSource = _controller.GetLowStockAlert();
            FormatLowStockGrid();

            // dgvExpiry.DataSource = ...
        }

        private void TabMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabMain.SelectedTab.Text == "Lô & Hạn Sử Dụng")
            {
                LoadBatchData();
            }
        }

        private void LoadBatchData()
        {
            try
            {
                int warehouseId = 0; // if we bind true object, get it here
                _allBatches = _controller.GetAllBatches(warehouseId);
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

            // Add Status Column if not exists
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
                    string text = "";
                    Color bgColor = Color.White;
                    Color fgColor = Color.White;

                    if (item.DaysToExpiry <= 0)
                    {
                        text = "Hết hạn";
                        bgColor = ColorTranslator.FromHtml("#E74C3C");
                    }
                    else if (item.DaysToExpiry >= 1 && item.DaysToExpiry <= 7)
                    {
                        text = "Sắp hết";
                        bgColor = ColorTranslator.FromHtml("#E67E22");
                    }
                    else if (item.DaysToExpiry >= 8 && item.DaysToExpiry <= 30)
                    {
                        text = "Cần chú ý";
                        bgColor = ColorTranslator.FromHtml("#F39C12");
                        fgColor = Color.Black;
                    }
                    else
                    {
                        text = "Còn hạn";
                        bgColor = ColorTranslator.FromHtml("#27AE60");
                    }

                    // Draw badge
                    int padding = 4;
                    var badgeRect = new Rectangle(e.CellBounds.X + padding, e.CellBounds.Y + padding, e.CellBounds.Width - padding * 2, e.CellBounds.Height - padding * 2);
                    using (Brush bgBrush = new SolidBrush(bgColor))
                    {
                        e.Graphics.FillRectangle(bgBrush, badgeRect);
                    }

                    // Draw text
                    TextRenderer.DrawText(e.Graphics, text, e.CellStyle.Font, badgeRect, fgColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    e.Handled = true;
                }
            }
        }

        private void FormatOverviewGrid()
        {
            if (dgvOverview.Columns["ProductID"] != null) dgvOverview.Columns["ProductID"].HeaderText = "Mã SP";
            if (dgvOverview.Columns["ProductName"] != null) dgvOverview.Columns["ProductName"].HeaderText = "Tên sản phẩm";
            if (dgvOverview.Columns["SoldQuantity"] != null) dgvOverview.Columns["SoldQuantity"].HeaderText = "Đã bán";
            if (dgvOverview.Columns["Revenue"] != null) dgvOverview.Columns["Revenue"].HeaderText = "Doanh thu";
            if (dgvOverview.Columns["CostPrice"] != null) dgvOverview.Columns["CostPrice"].HeaderText = "Giá vốn";
            
            if (dgvOverview.Columns["Revenue"] != null) dgvOverview.Columns["Revenue"].DefaultCellStyle.Format = "N0";
            if (dgvOverview.Columns["SoldQuantity"] != null) dgvOverview.Columns["SoldQuantity"].DefaultCellStyle.Format = "N2";
        }

        private void FormatLowStockGrid()
        {
            if (dgvLowStock.Columns["ProductID"] != null) dgvLowStock.Columns["ProductID"].HeaderText = "Mã SP";
            if (dgvLowStock.Columns["ProductName"] != null) dgvLowStock.Columns["ProductName"].HeaderText = "Tên sản phẩm";
            if (dgvLowStock.Columns["CurrentStock"] != null) dgvLowStock.Columns["CurrentStock"].HeaderText = "Tồn kho";
            if (dgvLowStock.Columns["MinStockAlert"] != null) dgvLowStock.Columns["MinStockAlert"].HeaderText = "Mức cảnh báo";
            
            if (dgvLowStock.Columns["CurrentStock"] != null) dgvLowStock.Columns["CurrentStock"].DefaultCellStyle.Format = "N2";
        }
    }
}
