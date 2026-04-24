using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SmartPos.Module.Reports.Controllers;
using SmartPos.Module.Reports.Models;

namespace SmartPos.Module.Reports.Views
{
    public class frmProductDetail : Form
    {
        private readonly int _productID;
        private readonly ReportController _controller;

        private Label lblProductCode, lblProductName, lblTotalStock;
        private Label lblCostPrice, lblRevenue, lblSoldQuantity;
        private DataGridView dgvProductBatches;

        public frmProductDetail(int productID)
        {
            _productID = productID;
            _controller = new ReportController();
            
            InitializeUi();
            LoadData();
        }

        private void InitializeUi()
        {
            Text = "Chi tiết sản phẩm & Lô hàng";
            Width = 900;
            Height = 600;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.White;

            // Panel Top: Product Info
            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 120, Padding = new Padding(20) };
            
            lblProductCode = CreateLabel("Mã SP: ", 20, 20);
            lblProductName = CreateLabel("Tên sản phẩm: ", 20, 50);
            lblTotalStock = CreateLabel("Tồn kho tổng: ", 20, 80);

            lblCostPrice = CreateLabel("Giá vốn: ", 400, 20);
            lblSoldQuantity = CreateLabel("Đã bán (30 ngày): ", 400, 50);
            lblRevenue = CreateLabel("Doanh thu (30 ngày): ", 400, 80);

            pnlTop.Controls.AddRange(new Control[] { 
                lblProductCode, lblProductName, lblTotalStock,
                lblCostPrice, lblSoldQuantity, lblRevenue 
            });

            // Panel Middle: Header for Grid
            var pnlMiddle = new Panel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(20, 10, 20, 0) };
            var lblGridTitle = new Label { 
                Text = "DANH SÁCH LÔ CỦA SẢN PHẨM NÀY", 
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = true,
                ForeColor = Color.FromArgb(41, 128, 185)
            };
            pnlMiddle.Controls.Add(lblGridTitle);

            // DataGridView
            dgvProductBatches = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            dgvProductBatches.CellPainting += DgvProductBatches_CellPainting;

            var pnlFill = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 0, 20, 20) };
            pnlFill.Controls.Add(dgvProductBatches);

            Controls.Add(pnlFill);
            Controls.Add(pnlMiddle);
            Controls.Add(pnlTop);
        }

        private Label CreateLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 10)
            };
        }

        private void LoadData()
        {
            try
            {
                // Get Product Info (Assuming last 30 days for performance metrics to match the main report)
                var perf = _controller.GetProductPerformance(DateTime.Now.AddDays(-30), DateTime.Now)
                                      .FirstOrDefault(x => x.ProductID == _productID);
                
                if (perf != null)
                {
                    lblProductCode.Text = $"Mã SP: {perf.ProductCode}";
                    lblProductName.Text = $"Tên sản phẩm: {perf.ProductName}";
                    lblTotalStock.Text = $"Tồn kho tổng: {perf.CurrentStock:N2}";
                    lblCostPrice.Text = $"Giá vốn: {perf.CostPrice:N0} đ";
                    lblSoldQuantity.Text = $"Đã bán (30 ngày): {perf.SoldQuantity:N2}";
                    lblRevenue.Text = $"Doanh thu (30 ngày): {perf.Revenue:N0} đ";
                }
                else
                {
                    // If not found in 30-day performance, it might be a new or inactive product.
                    // We can still try to get the name from the batches.
                    lblProductName.Text = "Tên sản phẩm: (Không có giao dịch gần đây)";
                }

                // Get Batches
                var batches = _controller.GetBatchesByProduct(_productID);
                dgvProductBatches.DataSource = batches;

                if (perf == null && batches.Count > 0)
                {
                    lblProductCode.Text = $"Mã SP: {batches[0].ProductCode}";
                    lblProductName.Text = $"Tên sản phẩm: {batches[0].ProductName}";
                }

                FormatGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải chi tiết sản phẩm: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FormatGrid()
        {
            if (dgvProductBatches.Columns["ProductID"] != null) dgvProductBatches.Columns["ProductID"].Visible = false;
            if (dgvProductBatches.Columns["ProductCode"] != null) dgvProductBatches.Columns["ProductCode"].Visible = false;
            if (dgvProductBatches.Columns["ProductName"] != null) dgvProductBatches.Columns["ProductName"].Visible = false;
            if (dgvProductBatches.Columns["DaysToExpiry"] != null) dgvProductBatches.Columns["DaysToExpiry"].Visible = false;

            if (dgvProductBatches.Columns["BatchNumber"] != null) dgvProductBatches.Columns["BatchNumber"].HeaderText = "Số lô";
            if (dgvProductBatches.Columns["ManufactureDate"] != null) dgvProductBatches.Columns["ManufactureDate"].HeaderText = "Từ ngày (NSX)";
            if (dgvProductBatches.Columns["ExpiryDate"] != null) dgvProductBatches.Columns["ExpiryDate"].HeaderText = "Đến ngày (HSD)";
            if (dgvProductBatches.Columns["Quantity"] != null) dgvProductBatches.Columns["Quantity"].HeaderText = "Tồn lô";
            if (dgvProductBatches.Columns["ShelfLocation"] != null) dgvProductBatches.Columns["ShelfLocation"].HeaderText = "Kệ";
            if (dgvProductBatches.Columns["WarehouseName"] != null) dgvProductBatches.Columns["WarehouseName"].HeaderText = "Kho";

            if (dgvProductBatches.Columns["StatusBadge"] == null)
            {
                dgvProductBatches.Columns.Add(new DataGridViewTextBoxColumn { Name = "StatusBadge", HeaderText = "Trạng thái", ReadOnly = true });
            }
        }

        private void DgvProductBatches_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && dgvProductBatches.Columns[e.ColumnIndex].Name == "StatusBadge")
            {
                e.PaintBackground(e.CellBounds, true);

                var row = dgvProductBatches.Rows[e.RowIndex];
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

                    int padding = 4;
                    var badgeRect = new Rectangle(e.CellBounds.X + padding, e.CellBounds.Y + padding, e.CellBounds.Width - padding * 2, e.CellBounds.Height - padding * 2);
                    using (Brush bgBrush = new SolidBrush(bgColor))
                    {
                        e.Graphics.FillRectangle(bgBrush, badgeRect);
                    }

                    TextRenderer.DrawText(e.Graphics, text, e.CellStyle.Font, badgeRect, fgColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    e.Handled = true;
                }
            }
        }
    }
}
