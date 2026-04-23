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
            Text = "Quan ly Hoa don ban hang";
            Width = 1200;
            Height = 700;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.White;

            var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = Color.FromArgb(240, 242, 245), Padding = new Padding(10) };
            
            dtpFrom = new DateTimePicker { Location = new Point(10, 30), Width = 120, Format = DateTimePickerFormat.Short };
            dtpTo = new DateTimePicker { Location = new Point(140, 30), Width = 120, Format = DateTimePickerFormat.Short };
            
            cboStaff = new ComboBox { Location = new Point(270, 30), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cboStaff.Items.Add("-- Nhan vien --");
            foreach (var user in _controller.GetUsers())
            {
                cboStaff.Items.Add(new KeyValuePair<int, string>(user.Key, user.Value));
            }
            cboStaff.SelectedIndex = 0;

            txtCustomerSearch = new TextBox { Location = new Point(430, 30), Width = 150 };
            // Placeholder not supported in 4.6.1 easily, use label or just leave empty

            cboPaymentMethod = new ComboBox { Location = new Point(590, 30), Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
            cboPaymentMethod.Items.AddRange(new object[] { "-- PT Thanh toan --", "Tien mat", "Chuyen khoan" });
            cboPaymentMethod.SelectedIndex = 0;

            cboStatus = new ComboBox { Location = new Point(720, 30), Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
            cboStatus.Items.AddRange(new object[] { "-- Trang thai --", "Hoan tat", "Da huy" });
            cboStatus.SelectedIndex = 0;

            btnFilter = new Button { Text = "Loc", Location = new Point(850, 28), Width = 80, Height = 30, BackColor = Color.FromArgb(25, 118, 210), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnFilter.Click += (s, e) => LoadData();

            pnlHeader.Controls.AddRange(new Control[] { dtpFrom, dtpTo, cboStaff, txtCustomerSearch, cboPaymentMethod, cboStatus, btnFilter });
            
            dgvOrders = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowTemplate = { Height = 35 }
            };
            
            dgvOrders.Columns.Add("InvoiceCode", "Ma HD");
            dgvOrders.Columns.Add("InvoiceDate", "Ngay lap");
            dgvOrders.Columns.Add("CustomerName", "Khach hang");
            dgvOrders.Columns.Add("StaffName", "Thu ngan");
            dgvOrders.Columns.Add("TotalAmount", "Tong tien");
            dgvOrders.Columns.Add("DiscountAmount", "Giam gia");
            dgvOrders.Columns.Add("FinalAmount", "Thuc thu");
            dgvOrders.Columns.Add("PaymentMethodText", "PT Thanh toan");
            dgvOrders.Columns.Add("StatusText", "Trang thai");

            var btnColDetail = new DataGridViewButtonColumn { Text = "Chi tiet", UseColumnTextForButtonValue = true, HeaderText = "Thao tac", Name = "btnDetail" };
            dgvOrders.Columns.Add(btnColDetail);

            dgvOrders.CellContentClick += DgvOrders_CellContentClick;

            Controls.Add(dgvOrders);
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

            var data = _controller.GetSalesHistory(dtpFrom.Value, dtpTo.Value, staffId, txtCustomerSearch.Text.Trim(), payMethod, status);
            dgvOrders.Rows.Clear();
            foreach (var item in data)
            {
                int rowIndex = dgvOrders.Rows.Add(
                    item.InvoiceCode,
                    item.InvoiceDate.ToString("dd/MM/yyyy HH:mm"),
                    item.CustomerName,
                    item.StaffName,
                    item.TotalAmount.ToString("N0"),
                    item.DiscountAmount.ToString("N0"),
                    item.FinalAmount.ToString("N0"),
                    item.PaymentMethodText,
                    item.StatusText
                );
                dgvOrders.Rows[rowIndex].Tag = item.InvoiceID;
                if (item.Status == 2) dgvOrders.Rows[rowIndex].DefaultCellStyle.ForeColor = Color.Red;
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
