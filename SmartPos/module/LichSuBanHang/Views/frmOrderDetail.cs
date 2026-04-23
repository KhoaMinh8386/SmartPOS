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
            Text = "Chi tiet hoa don";
            Width = 900;
            Height = 650;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.White;

            var mainSplit = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 150 };
            
            var pnlHeader = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            var lblInfo = new Label { Name = "lblInfo", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 11F) };
            pnlHeader.Controls.Add(lblInfo);

            var dgvItems = new DataGridView
            {
                Name = "dgvItems",
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                AllowUserToAddRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            dgvItems.Columns.Add("ProductCode", "Ma SP");
            dgvItems.Columns.Add("ProductName", "Ten san pham");
            dgvItems.Columns.Add("UnitName", "DVT");
            dgvItems.Columns.Add("Quantity", "SL");
            dgvItems.Columns.Add("UnitPrice", "Don gia");
            dgvItems.Columns.Add("SubTotal", "Thanh tien");

            var pnlFooter = new Panel { Dock = DockStyle.Bottom, Height = 180, BackColor = Color.FromArgb(245, 245, 245), Padding = new Padding(15) };
            var lblSummary = new Label { Name = "lblSummary", Dock = DockStyle.Left, Width = 400, Font = new Font("Segoe UI", 10F) };
            
            var pnlActions = new Panel { Dock = DockStyle.Right, Width = 200 };
            var btnPrint = new Button { Text = "In lai hoa don", Dock = DockStyle.Top, Height = 40, BackColor = Color.LightGray, FlatStyle = FlatStyle.Flat };
            btnPrint.Click += (s, e) => {
                if (_detail != null) {
                    var printer = new SalesPrinter();
                    printer.PrintInvoice(_detail);
                }
            };

            var btnCancel = new Button { Text = "HUY HOA DON", Dock = DockStyle.Bottom, Height = 40, BackColor = Color.MistyRose, ForeColor = Color.Red, FlatStyle = FlatStyle.Flat };
            btnCancel.Click += BtnCancel_Click;

            pnlActions.Controls.Add(btnPrint);
            pnlActions.Controls.Add(btnCancel);
            
            pnlFooter.Controls.Add(lblSummary);
            pnlFooter.Controls.Add(pnlActions);

            mainSplit.Panel1.Controls.Add(pnlHeader);
            mainSplit.Panel2.Controls.Add(dgvItems);
            Controls.Add(mainSplit);
            Controls.Add(pnlFooter);
        }

        private void LoadDetail()
        {
            _detail = _controller.GetOrderDetail(_invoiceId);
            if (_detail == null) return;

            var lblInfo = (Label)Controls.Find("lblInfo", true)[0];
            lblInfo.Text = $"MA HD: {_detail.InvoiceCode} | NGAY: {_detail.InvoiceDate:dd/MM/yyyy HH:mm}\n" +
                           $"KHACH HANG: {_detail.CustomerName} ({_detail.CustomerPhone})\n" +
                           $"THU NGAN: {_detail.StaffName}\n" +
                           $"GHI CHU: {_detail.Notes}";

            var dgvItems = (DataGridView)Controls.Find("dgvItems", true)[0];
            dgvItems.Rows.Clear();
            foreach (var item in _detail.Items)
            {
                dgvItems.Rows.Add(item.ProductCode, item.ProductName, item.UnitName, item.Quantity, item.UnitPrice.ToString("N0"), item.SubTotal.ToString("N0"));
            }

            var lblSummary = (Label)Controls.Find("lblSummary", true)[0];
            lblSummary.Text = $"Tong tien hang: {_detail.TotalAmount:N0}\n" +
                              $"Giam gia: {_detail.DiscountAmount:N0}\n" +
                              $"Voucher: {_detail.VoucherDiscount:N0}\n" +
                              $"Diem doi: {_detail.LoyaltyDiscount:N0} ({_detail.LoyaltyPointsUsed} diem)\n" +
                              $"---------------------------------\n" +
                              $"THUC THU: {_detail.PaidAmount:N0}\n" +
                              $"Diem tich luy don nay: {_detail.LoyaltyPointsEarned}";
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            // Simple Admin check for demo (should use UserSession)
            if (MessageBox.Show("Ban co chac chan muon HUY hoa don nay? He thong se hoan ton kho va tru diem khach hang.", "Xac nhan", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                string reason = Microsoft.VisualBasic.Interaction.InputBox("Nhap ly do huy hoa don:", "Huy hoa don", "Khach tra hang");
                if (string.IsNullOrWhiteSpace(reason)) return;

                try
                {
                    _controller.CancelOrder(_invoiceId, reason);
                    MessageBox.Show("Huy hoa don thanh cong!", "Thong bao");
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Loi: " + ex.Message);
                }
            }
        }
    }
}
