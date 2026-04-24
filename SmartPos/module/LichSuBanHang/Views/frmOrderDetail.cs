using System;
using System.Drawing;
using System.Windows.Forms;
using SmartPos.Module.SalesHistory.Controllers;
using SmartPos.Module.SalesHistory.Models;
using SmartPos.Module.SalesHistory.Backend;

namespace SmartPos.Module.SalesHistory.Views
{
    public class frmOrderDetail : Form
    {
        private readonly SalesController _controller;
        private readonly int _invoiceId;
        private SalesOrderDetail _detail;

        public frmOrderDetail(int invoiceId)
        {
            _controller = new SalesController();
            _invoiceId = invoiceId;
            InitializeUi();
            LoadDetail();
        }

        private void InitializeUi()
        {
            Text = "Chi tiết hóa đơn";
            Width = 950;
            Height = 700;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(241, 245, 249); // Slate 100

            // Main Container with padding
            var pnlMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            
            // --- Top Card: Header Info ---
            var pnlHeaderCard = new Panel 
            { 
                Dock = DockStyle.Top, 
                Height = 140, 
                BackColor = Color.White, 
                Padding = new Padding(20) 
            };
            // Rounded corner simulation (simple)
            pnlHeaderCard.Paint += (s, e) => {
                ControlPaint.DrawBorder(e.Graphics, pnlHeaderCard.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);
            };

            var lblTitle = new Label { Text = "THÔNG TIN HÓA ĐƠN", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.FromArgb(30, 41, 59), Dock = DockStyle.Top, Height = 30 };
            
            var tableInfo = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2, Padding = new Padding(0, 10, 0, 0) };
            tableInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            tableInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            var lblInfoLeft = new Label { Name = "lblInfoLeft", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10), ForeColor = Color.FromArgb(71, 85, 105) };
            var lblInfoRight = new Label { Name = "lblInfoRight", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10), ForeColor = Color.FromArgb(71, 85, 105) };
            
            tableInfo.Controls.Add(lblInfoLeft, 0, 0);
            tableInfo.Controls.Add(lblInfoRight, 1, 0);
            
            pnlHeaderCard.Controls.Add(tableInfo);
            pnlHeaderCard.Controls.Add(lblTitle);

            // --- Middle Card: Product List ---
            var pnlGridCard = new Panel 
            { 
                Dock = DockStyle.Fill, 
                BackColor = Color.White, 
                Padding = new Padding(1, 40, 1, 1), 
                Margin = new Padding(0, 20, 0, 20) 
            };
            var lblGridTitle = new Label { Text = "DANH SÁCH SẢN PHẨM", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(30, 41, 59), Location = new Point(20, 12), AutoSize = true };
            pnlGridCard.Controls.Add(lblGridTitle);

            var dgvItems = new DataGridView
            {
                Name = "dgvItems",
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                EnableHeadersVisualStyles = false,
                AllowUserToAddRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowTemplate = { Height = 40 },
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                GridColor = Color.FromArgb(241, 245, 249)
            };
            dgvItems.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            dgvItems.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            dgvItems.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(100, 116, 139);

            dgvItems.Columns.Add("ProductCode", "Mã SP");
            dgvItems.Columns.Add("ProductName", "Tên sản phẩm");
            dgvItems.Columns.Add("UnitName", "ĐVT");
            dgvItems.Columns.Add("Quantity", "SL");
            dgvItems.Columns.Add("UnitPrice", "Đơn giá");
            dgvItems.Columns.Add("SubTotal", "Thành tiền");

            dgvItems.Columns["Quantity"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvItems.Columns["UnitPrice"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvItems.Columns["SubTotal"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            pnlGridCard.Controls.Add(dgvItems);

            // --- Bottom Section: Summary & Actions ---
            var pnlBottom = new Panel { Dock = DockStyle.Bottom, Height = 220, Padding = new Padding(0, 20, 0, 0) };
            
            // Left: Notes or other info
            var pnlNotes = new Panel { Dock = DockStyle.Left, Width = 300, BackColor = Color.White, Padding = new Padding(15) };
            var lblNoteTitle = new Label { Text = "Ghi chú:", Font = new Font("Segoe UI", 9, FontStyle.Bold), Dock = DockStyle.Top };
            var lblNotes = new Label { Name = "lblNotes", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9, FontStyle.Italic), ForeColor = Color.FromArgb(100, 116, 139) };
            pnlNotes.Controls.Add(lblNotes);
            pnlNotes.Controls.Add(lblNoteTitle);

            // Right: Totals
            var pnlSummary = new Panel { Dock = DockStyle.Right, Width = 350, BackColor = Color.White, Padding = new Padding(20) };
            var lblSummary = new Label { Name = "lblSummary", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10), ForeColor = Color.FromArgb(30, 41, 59) };
            pnlSummary.Controls.Add(lblSummary);

            // Actions (Fixed at very bottom)
            var pnlActions = new Panel { Dock = DockStyle.Bottom, Height = 60, Padding = new Padding(0, 10, 0, 0) };
            
            var btnPrint = new Button 
            { 
                Text = "🖨️ In lại hóa đơn", 
                Width = 150, 
                Height = 45, 
                BackColor = Color.FromArgb(71, 85, 105), 
                ForeColor = Color.White, 
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Location = new Point(0, 5)
            };
            btnPrint.FlatAppearance.BorderSize = 0;
            btnPrint.Click += (s, e) => {
                if (_detail != null) {
                    var printer = new SalesPrinter();
                    printer.PrintInvoice(_detail);
                }
            };

            var btnCancel = new Button 
            { 
                Text = "🚫 HỦY HÓA ĐƠN", 
                Width = 180, 
                Height = 45, 
                BackColor = Color.FromArgb(254, 226, 226), 
                ForeColor = Color.FromArgb(220, 38, 38), 
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Location = new Point(730, 5) // Position on right
            };
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(252, 165, 165);
            btnCancel.Click += BtnCancel_Click;

            pnlActions.Controls.Add(btnPrint);
            pnlActions.Controls.Add(btnCancel);
            
            pnlBottom.Controls.Add(pnlNotes);
            pnlBottom.Controls.Add(pnlSummary);

            pnlMain.Controls.Add(pnlGridCard);
            pnlMain.Controls.Add(pnlBottom);
            pnlMain.Controls.Add(pnlHeaderCard);
            
            Controls.Add(pnlMain);
            Controls.Add(pnlActions); // Actions outside main to stick to bottom if needed, or just part of main
            
            // Adjust actions position
            pnlActions.Dock = DockStyle.Bottom;
            pnlActions.SendToBack();
        }

        private void LoadDetail()
        {
            _detail = _controller.GetOrderDetail(_invoiceId);
            if (_detail == null) return;

            var lblLeft = (Label)Controls.Find("lblInfoLeft", true)[0];
            var lblRight = (Label)Controls.Find("lblInfoRight", true)[0];
            var lblNotes = (Label)Controls.Find("lblNotes", true)[0];

            lblLeft.Text = $"Mã HĐ: {_detail.InvoiceCode}\n" +
                           $"Ngày lập: {_detail.InvoiceDate:dd/MM/yyyy HH:mm}\n" +
                           $"Trạng thái: " + (_detail.Status == 2 ? "ĐÃ HỦY" : "Hoàn tất");
            
            lblRight.Text = $"Khách hàng: {_detail.FullName}\n" +
                            $"Điện thoại: {_detail.CustomerPhone}\n" +
                            $"Thu ngân: {_detail.StaffName}";

            lblNotes.Text = string.IsNullOrEmpty(_detail.Notes) ? "(Không có ghi chú)" : _detail.Notes;

            var dgvItems = (DataGridView)Controls.Find("dgvItems", true)[0];
            dgvItems.Rows.Clear();
            foreach (var item in _detail.Items)
            {
                dgvItems.Rows.Add(item.ProductCode, item.ProductName, item.UnitName, item.Quantity, item.UnitPrice.ToString("N0"), item.SubTotal.ToString("N0"));
            }

            var lblSummary = (Label)Controls.Find("lblSummary", true)[0];
            lblSummary.Text = $"Tổng tiền hàng:   {_detail.TotalAmount,15:N0}\n" +
                              $"Giảm giá:          {_detail.DiscountAmount,15:N0}\n" +
                              $"Voucher:           {_detail.VoucherDiscount,15:N0}\n" +
                              $"Điểm đổi:          {_detail.LoyaltyDiscount,15:N0}\n" +
                              $"------------------------------------------\n" +
                              $"THỰC THU:          {_detail.PaidAmount,15:N0}";
            
            // Style the summary label for alignment
            lblSummary.TextAlign = ContentAlignment.TopRight;
            lblSummary.Font = new Font("Consolas", 11);
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Bạn có chắc chắn muốn HỦY hóa đơn này? Hệ thống sẽ hoàn tồn kho và trừ điểm khách hàng.", "Xác nhận hủy", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                string reason = Microsoft.VisualBasic.Interaction.InputBox("Nhập lý do hủy hóa đơn:", "Hủy hóa đơn", "Khách trả hàng");
                if (string.IsNullOrWhiteSpace(reason)) return;

                try
                {
                    _controller.CancelOrder(_invoiceId, reason);
                    MessageBox.Show("Hủy hóa đơn thành công!", "Thông báo");
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi: " + ex.Message);
                }
            }
        }
    }
}
