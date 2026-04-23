using System;
using System.Drawing;
using System.Windows.Forms;
using SmartPos.Module.Reports.Controllers;

namespace SmartPos.Module.Reports.Views
{
    public class frmProductReport : Form
    {
        private readonly ReportController _controller;
        private TabControl tabMain;
        private DataGridView dgvOverview, dgvLowStock, dgvExpiry;

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

            tabMain.TabPages.AddRange(new TabPage[] { tabOverview, tabLowStock, tabExpiry });
            Controls.Add(tabMain);
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
