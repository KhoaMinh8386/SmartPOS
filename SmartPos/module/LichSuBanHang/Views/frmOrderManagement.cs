using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using SmartPos.Module.SalesHistory.Controllers;
using SmartPos.Module.SalesHistory.Models;

namespace SmartPos.Module.SalesHistory.Views
{
    public class frmOrderManagement : Form
    {
        private readonly SalesController _controller;
        private DataGridView dgvOrders;
        private DateTimePicker dtpFrom, dtpTo;
        private ComboBox cboStaff, cboPaymentMethod, cboStatus;
        private TextBox txtCustomerSearch;
        private Button btnFilter;

        public frmOrderManagement()
        {
            _controller = new SalesController();
            InitializeUi();
            LoadData();
        }

        private void InitializeUi()
        {
            Text = "Quản lý Lịch sử Bán hàng";
            Width = 1200;
            Height = 750;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(248, 250, 252); // Slate 50

            // --- Header Panel ---
            var pnlHeader = new Panel 
            { 
                Dock = DockStyle.Top, 
                Height = 100, 
                BackColor = Color.White, 
                Padding = new Padding(20, 10, 20, 10) 
            };
            
            // Filter Title
            var lblTitle = new Label 
            { 
                Text = "Bộ lọc tìm kiếm", 
                Font = new Font("Segoe UI", 12, FontStyle.Bold), 
                ForeColor = Color.FromArgb(15, 23, 42),
                AutoSize = true,
                Location = new Point(20, 10)
            };
            pnlHeader.Controls.Add(lblTitle);

            // Filter Flow Layout
            var flowFilter = new FlowLayoutPanel 
            { 
                Location = new Point(20, 45),
                Size = new Size(1160, 50),
                BackColor = Color.Transparent
            };

            dtpFrom = new DateTimePicker { Width = 130, Format = DateTimePickerFormat.Short, Font = new Font("Segoe UI", 10) };
            dtpTo = new DateTimePicker { Width = 130, Format = DateTimePickerFormat.Short, Font = new Font("Segoe UI", 10) };
            
            cboStaff = new ComboBox { Width = 160, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };
            cboStaff.Items.Add("-- Nhân viên --");
            foreach (var user in _controller.GetUsers())
            {
                cboStaff.Items.Add(new KeyValuePair<int, string>(user.Key, user.Value));
            }
            cboStaff.SelectedIndex = 0;

            txtCustomerSearch = new TextBox { Width = 180, Font = new Font("Segoe UI", 10) };
            // Simulate placeholder
            txtCustomerSearch.ForeColor = Color.Gray;
            txtCustomerSearch.Text = "Tên khách / SĐT...";
            txtCustomerSearch.Enter += (s, e) => { if (txtCustomerSearch.Text == "Tên khách / SĐT...") { txtCustomerSearch.Text = ""; txtCustomerSearch.ForeColor = Color.Black; } };
            txtCustomerSearch.Leave += (s, e) => { if (string.IsNullOrWhiteSpace(txtCustomerSearch.Text)) { txtCustomerSearch.Text = "Tên khách / SĐT..."; txtCustomerSearch.ForeColor = Color.Gray; } };

            cboPaymentMethod = new ComboBox { Width = 140, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };
            cboPaymentMethod.Items.AddRange(new object[] { "-- PT Thanh toán --", "Tiền mặt", "Chuyển khoản" });
            cboPaymentMethod.SelectedIndex = 0;

            cboStatus = new ComboBox { Width = 130, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };
            cboStatus.Items.AddRange(new object[] { "-- Trạng thái --", "Hoàn tất", "Đã hủy" });
            cboStatus.SelectedIndex = 0;

            btnFilter = new Button 
            { 
                Text = "🔍 Lọc", 
                Width = 100, 
                Height = 32, 
                BackColor = Color.FromArgb(37, 99, 235), // Blue 600
                ForeColor = Color.White, 
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnFilter.FlatAppearance.BorderSize = 0;
            btnFilter.Click += (s, e) => LoadData();

            flowFilter.Controls.AddRange(new Control[] { dtpFrom, dtpTo, cboStaff, txtCustomerSearch, cboPaymentMethod, cboStatus, btnFilter });
            pnlHeader.Controls.Add(flowFilter);

            // --- Grid ---
            dgvOrders = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                EnableHeadersVisualStyles = false,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowTemplate = { Height = 45 },
                GridColor = Color.FromArgb(226, 232, 240)
            };

            // Grid Header Style
            dgvOrders.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(241, 245, 249);
            dgvOrders.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(71, 85, 105);
            dgvOrders.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvOrders.ColumnHeadersDefaultCellStyle.Padding = new Padding(10, 0, 10, 0);

            // Grid Row Style
            dgvOrders.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            dgvOrders.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            dgvOrders.DefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 58, 138);
            dgvOrders.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);

            dgvOrders.Columns.Add("InvoiceCode", "Mã HD");
            dgvOrders.Columns.Add("InvoiceDate", "Ngày lập");
            dgvOrders.Columns.Add("FullName", "Khách hàng");
            dgvOrders.Columns.Add("StaffName", "Thu ngân");
            dgvOrders.Columns.Add("TotalAmount", "Tổng tiền");
            dgvOrders.Columns.Add("DiscountAmount", "Giảm giá");
            dgvOrders.Columns.Add("FinalAmount", "Thực thu");
            dgvOrders.Columns.Add("PaymentMethodText", "PT Thanh toán");
            dgvOrders.Columns.Add("StatusText", "Trạng thái");

            dgvOrders.Columns["TotalAmount"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvOrders.Columns["DiscountAmount"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvOrders.Columns["FinalAmount"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            var btnColDetail = new DataGridViewButtonColumn 
            { 
                Text = "Chi tiết", 
                UseColumnTextForButtonValue = true, 
                HeaderText = "Thao tác", 
                Name = "btnDetail",
                FlatStyle = FlatStyle.Flat
            };
            dgvOrders.Columns.Add(btnColDetail);

            dgvOrders.CellContentClick += DgvOrders_CellContentClick;

            // Padding panel for grid to give it some breath
            var pnlGrid = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            pnlGrid.Controls.Add(dgvOrders);

            Controls.Add(pnlGrid);
            Controls.Add(pnlHeader);
        }

        private void LoadData()
        {
            int? staffId = null;
            if (cboStaff.SelectedItem is KeyValuePair<int, string> kvp) staffId = kvp.Key;

            byte? payMethod = null;
            if (cboPaymentMethod.SelectedIndex > 0) payMethod = (byte)cboPaymentMethod.SelectedIndex;

            int? status = null;
            if (cboStatus.SelectedIndex > 0) status = cboStatus.SelectedIndex;

            string searchText = txtCustomerSearch.Text == "Tên khách / SĐT..." ? "" : txtCustomerSearch.Text.Trim();

            var data = _controller.GetSalesHistory(dtpFrom.Value, dtpTo.Value, staffId, searchText, payMethod, status);
            dgvOrders.Rows.Clear();
            foreach (var item in data)
            {
                int rowIndex = dgvOrders.Rows.Add(
                    item.InvoiceCode,
                    item.InvoiceDate.ToString("dd/MM/yyyy HH:mm"),
                    item.FullName,
                    item.StaffName,
                    item.TotalAmount.ToString("N0"),
                    item.DiscountAmount.ToString("N0"),
                    item.FinalAmount.ToString("N0"),
                    item.PaymentMethodText,
                    item.StatusText
                );
                dgvOrders.Rows[rowIndex].Tag = item.InvoiceID;
                
                // Color status text
                if (item.Status == 2) // Cancelled
                {
                    dgvOrders.Rows[rowIndex].Cells["StatusText"].Style.ForeColor = Color.Red;
                    dgvOrders.Rows[rowIndex].Cells["StatusText"].Style.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                }
                else // Completed
                {
                    dgvOrders.Rows[rowIndex].Cells["StatusText"].Style.ForeColor = Color.Green;
                    dgvOrders.Rows[rowIndex].Cells["StatusText"].Style.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                }
            }
        }

        private void DgvOrders_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (dgvOrders.Columns[e.ColumnIndex].Name == "btnDetail")
            {
                int invoiceId = (int)dgvOrders.Rows[e.RowIndex].Tag;
                using (var detailForm = new frmOrderDetail(invoiceId))
                {
                    detailForm.ShowDialog(this);
                }
            }
        }
    }
}
