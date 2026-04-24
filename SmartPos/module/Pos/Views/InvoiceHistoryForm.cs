using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
namespace SmartPos.Module.Pos
{
    public class InvoiceHistoryForm : Form
    {
        private readonly PosController _controller;
        private DataGridView dgvInvoices;
        private TextBox txtSearch;
        private Panel pnlDetail;
        private Label lblDetailInfo;
        private DataGridView dgvItems;

        public InvoiceHistoryForm()
        {
            _controller = new PosController();
            InitializeUi();
            LoadInvoices();
        }

        private void InitializeUi()
        {
            this.Text = "LỊCH SỬ HÓA ĐƠN - POS";
            this.Size = new Size(1100, 750);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(248, 250, 252);
            this.Font = new Font("Segoe UI", 10F);

            // Root Layout
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(15)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F)); // Header/Filter
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Content

            // 1. Header & Filter
            var pnlHeader = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(10), Margin = new Padding(0, 0, 0, 10) };
            
            var lblTitle = new Label 
            { 
                Text = "QUẢN LÝ HÓA ĐƠN", 
                Font = new Font("Segoe UI", 14F, FontStyle.Bold), 
                ForeColor = Color.FromArgb(30, 41, 59),
                AutoSize = true,
                Location = new Point(15, 15)
            };
            
            txtSearch = new TextBox 
            { 
                Width = 350, 
                Height = 35, 
                Font = new Font("Segoe UI", 11F),
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(lblTitle.Right + 30, 18)
            };
            txtSearch.TextChanged += (s, e) => LoadInvoices();

            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(txtSearch);
            root.Controls.Add(pnlHeader, 0, 0);
            root.SetColumnSpan(pnlHeader, 2);

            // 2. Left: Invoices List
            var pnlList = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(1) };
            dgvInvoices = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowTemplate = { Height = 45 },
                AllowUserToAddRows = false,
                ReadOnly = true,
                EnableHeadersVisualStyles = false,
                ColumnHeadersHeight = 45,
                GridColor = Color.FromArgb(241, 245, 249)
            };
            dgvInvoices.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(241, 245, 249);
            dgvInvoices.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(71, 85, 105);
            dgvInvoices.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvInvoices.DefaultCellStyle.SelectionBackColor = Color.FromArgb(226, 232, 240);
            dgvInvoices.DefaultCellStyle.SelectionForeColor = Color.FromArgb(15, 23, 42);
            dgvInvoices.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            
            dgvInvoices.SelectionChanged += DgvInvoices_SelectionChanged;
            pnlList.Controls.Add(dgvInvoices);
            root.Controls.Add(pnlList, 0, 1);

            // 3. Right: Detail Card
            var pnlDetailRoot = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15, 0, 0, 0) };
            pnlDetail = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(20), Visible = false };
            
            var lblDetailHeader = new Label { Text = "CHI TIẾT HÓA ĐƠN", Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = Color.FromArgb(14, 165, 233), Dock = DockStyle.Top, Height = 40 };
            
            lblDetailInfo = new Label { Dock = DockStyle.Top, Height = 180, Font = new Font("Consolas", 10F), ForeColor = Color.FromArgb(51, 65, 85) };
            
            dgvItems = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                AllowUserToAddRows = false,
                ReadOnly = true,
                RowTemplate = { Height = 35 },
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            var btnPrint = new Button
            {
                Text = "IN LẠI HÓA ĐƠN (P)",
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = Color.FromArgb(15, 23, 42),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 15, 0, 0)
            };
            btnPrint.FlatAppearance.BorderSize = 0;
            btnPrint.Click += BtnPrint_Click;

            pnlDetail.Controls.Add(dgvItems);
            pnlDetail.Controls.Add(lblDetailInfo);
            pnlDetail.Controls.Add(btnPrint);
            pnlDetail.Controls.Add(lblDetailHeader);
            
            pnlDetailRoot.Controls.Add(pnlDetail);
            root.Controls.Add(pnlDetailRoot, 1, 1);

            this.Controls.Add(root);
        }

        private void LoadInvoices()
        {
            var data = _controller.GetInvoiceHistory(txtSearch.Text.Trim());
            dgvInvoices.DataSource = null;
            dgvInvoices.DataSource = data;
            
            if (dgvInvoices.Columns.Count > 0)
            {
                dgvInvoices.Columns["InvoiceID"].Visible = false;
                dgvInvoices.Columns["InvoiceCode"].HeaderText = "MÃ HÓA ĐƠN";
                dgvInvoices.Columns["InvoiceDate"].HeaderText = "NGÀY GIỜ";
                dgvInvoices.Columns["InvoiceDate"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
                dgvInvoices.Columns["FullName"].HeaderText = "KHÁCH HÀNG";
                dgvInvoices.Columns["StaffName"].HeaderText = "NHÂN VIÊN";
                dgvInvoices.Columns["TotalAmount"].HeaderText = "TỔNG TIỀN";
                dgvInvoices.Columns["TotalAmount"].DefaultCellStyle.Format = "N0";
                dgvInvoices.Columns["TotalAmount"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                dgvInvoices.Columns["PaymentMethodText"].HeaderText = "THANH TOÁN";
            }
        }

        private void DgvInvoices_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvInvoices.CurrentRow == null) { pnlDetail.Visible = false; return; }
            if (!(dgvInvoices.CurrentRow.DataBoundItem is InvoiceListItem inv)) return;
            
            var detail = _controller.GetInvoiceDetail(inv.InvoiceID);
            if (detail != null)
            {
                pnlDetail.Visible = true;
                lblDetailInfo.Text = 
                    $"────────────────────────────────────────\n" +
                    $"Mã HĐ: {detail.InvoiceCode}\n" +
                    $"Ngày:  {detail.InvoiceDate:dd/MM/yyyy HH:mm:ss}\n" +
                    $"NV:    {detail.StaffName}\n" +
                    $"Khách: {detail.FullName ?? "Khách lẻ"}\n" +
                    $"SĐT:   {detail.Phone}\n" +
                    $"────────────────────────────────────────\n" +
                    $"TỔNG CỘNG:      {detail.TotalAmount,15:N0}\n" +
                    $"TIỀN KHÁCH ĐƯA: {detail.PaidAmount,15:N0}\n" +
                    $"TIỀN THỐI:      {detail.ChangeAmount,15:N0}\n" +
                    $"────────────────────────────────────────";
                
                dgvItems.DataSource = detail.Items.Select(x => new {
                    x.ProductName,
                    x.Quantity,
                    Price = x.UnitPrice,
                    Total = x.SubTotal
                }).ToList();
                
                dgvItems.Tag = detail; // Store for printing
            }
        }

        private void BtnPrint_Click(object sender, EventArgs e)
        {
            if (dgvItems.Tag is InvoiceDetail detail)
            {
                var printer = new PrintHelper();
                var service = new InvoiceService();
                printer.PrintInvoice(detail, service.GetStoreConfig());
            }
        }
    }
}
