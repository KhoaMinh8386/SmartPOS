using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using SmartPos.Module.Pos.Controllers;
using SmartPos.Module.Pos.Models;

namespace SmartPos.Module.Pos.Views
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
            Text = "Lich su hoa don";
            Width = 1000;
            Height = 650;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.White;

            var split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 600 };

            var leftPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            txtSearch = new TextBox { Dock = DockStyle.Top, Height = 35, Font = new Font("Segoe UI", 12F) };
            // PlaceholderText not supported in .NET Framework 4.6.1
            txtSearch.TextChanged += (s, e) => LoadInvoices();

            dgvInvoices = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            dgvInvoices.SelectionChanged += DgvInvoices_SelectionChanged;

            leftPanel.Controls.Add(dgvInvoices);
            leftPanel.Controls.Add(txtSearch);

            pnlDetail = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15), Visible = false };
            lblDetailInfo = new Label { Dock = DockStyle.Top, Height = 150, Font = new Font("Consolas", 10F) };
            
            dgvItems = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            
            var btnPrint = new Button { Text = "In lai hoa don", Dock = DockStyle.Bottom, Height = 40, BackColor = Color.LightGray };
            btnPrint.Click += (s, e) => MessageBox.Show("Chuc nang in dang duoc ket noi...");

            pnlDetail.Controls.Add(dgvItems);
            pnlDetail.Controls.Add(lblDetailInfo);
            pnlDetail.Controls.Add(btnPrint);

            split.Panel1.Controls.Add(leftPanel);
            split.Panel2.Controls.Add(pnlDetail);
            Controls.Add(split);
        }

        private void LoadInvoices()
        {
            dgvInvoices.DataSource = _controller.GetInvoiceHistory(txtSearch.Text.Trim());
        }

        private void DgvInvoices_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvInvoices.CurrentRow == null) return;
            var inv = (InvoiceListItem)dgvInvoices.CurrentRow.DataBoundItem;
            var detail = _controller.GetInvoiceDetail(inv.InvoiceID);
            
            if (detail != null)
            {
                pnlDetail.Visible = true;
                lblDetailInfo.Text = $"MA HD: {detail.InvoiceCode}\n" +
                                     $"NGAY: {detail.InvoiceDate:dd/MM/yyyy HH:mm}\n" +
                                     $"KHACH: {detail.CustomerName} ({detail.Phone})\n" +
                                     $"NV: {detail.StaffName}\n" +
                                     $"TONG: {detail.TotalAmount:N0}\n" +
                                     $"TRA: {detail.PaidAmount:N0}\n" +
                                     $"THOI: {detail.ChangeAmount:N0}";
                dgvItems.DataSource = detail.Items;
            }
        }
    }
}
