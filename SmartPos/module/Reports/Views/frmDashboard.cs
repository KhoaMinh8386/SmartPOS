using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using SmartPos.Module.Reports.Controllers;

namespace SmartPos.Module.Reports.Views
{
    public class frmDashboard : Form
    {
        private readonly ReportController _controller;
        private FlowLayoutPanel pnlKpis;
        private Chart chartRevenue;
        private Chart chartProducts;
        private Chart chartPayment;
        private DataGridView dgvRecentInvoices;
        private DataGridView dgvLowStock;

        public frmDashboard()
        {
            _controller = new ReportController();
            InitializeUi();
            LoadData();
        }

        private void InitializeUi()
        {
            Text = "Tổng quan Dashboard";
            Width = 1200;
            Height = 850;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(240, 242, 245);

            var mainScroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            
            // 1. KPI Cards
            pnlKpis = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 130, Padding = new Padding(10) };
            
            // 2. Charts Row 1
            var pnlCharts1 = new TableLayoutPanel { Dock = DockStyle.Top, Height = 350, ColumnCount = 2, Padding = new Padding(10) };
            pnlCharts1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            pnlCharts1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));

            chartRevenue = CreateChart("Doanh thu 7 ngày gần nhất", SeriesChartType.Line);
            chartProducts = CreateChart("Top 5 Sản phẩm bán chạy (Tháng)", SeriesChartType.Bar);
            
            pnlCharts1.Controls.Add(chartRevenue, 0, 0);
            pnlCharts1.Controls.Add(chartProducts, 1, 0);

            // 3. Charts Row 2 & Tables
            var pnlBottom = new TableLayoutPanel { Dock = DockStyle.Top, Height = 350, ColumnCount = 3, Padding = new Padding(10) };
            pnlBottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            pnlBottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
            pnlBottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));

            chartPayment = CreateChart("Phương thức thanh toán", SeriesChartType.Pie);
            
            dgvRecentInvoices = CreateGrid("10 Hóa đơn gần nhất");
            dgvLowStock = CreateGrid("Cảnh báo tồn kho thấp");

            pnlBottom.Controls.Add(chartPayment, 0, 0);
            pnlBottom.Controls.Add(dgvRecentInvoices, 1, 0);
            pnlBottom.Controls.Add(dgvLowStock, 2, 0);

            mainScroll.Controls.AddRange(new Control[] { pnlBottom, pnlCharts1, pnlKpis });
            Controls.Add(mainScroll);
        }

        private Chart CreateChart(string title, SeriesChartType type)
        {
            var chart = new Chart { Dock = DockStyle.Fill, BackColor = Color.White, Margin = new Padding(5) };
            chart.Titles.Add(new Title(title, Docking.Top, new Font("Segoe UI", 10, FontStyle.Bold), Color.Black));
            
            var area = new ChartArea("MainArea") { BackColor = Color.White };
            area.AxisX.MajorGrid.LineColor = Color.LightGray;
            area.AxisY.MajorGrid.LineColor = Color.LightGray;
            chart.ChartAreas.Add(area);

            var series = new Series("Data") { ChartType = type };
            chart.Series.Add(series);
            
            if (type == SeriesChartType.Pie)
            {
                series["PieLabelStyle"] = "Outside";
                chart.Legends.Add(new Legend("Default") { Docking = Docking.Bottom });
            }

            return chart;
        }

        private DataGridView CreateGrid(string title)
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(5), BackColor = Color.White };
            var lbl = new Label { Text = title, Dock = DockStyle.Top, Height = 30, Font = new Font("Segoe UI", 10, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft };
            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true,
                RowHeadersVisible = false
            };
            panel.Controls.Add(grid);
            panel.Controls.Add(lbl);
            return grid;
        }

        private void LoadData()
        {
            var kpis = _controller.GetDashboardKpis();
            pnlKpis.Controls.Clear();
            AddKpiCard("DOANH THU HÔM NAY", kpis.TodayRevenue.ToString("N0"), Color.FromArgb(25, 118, 210));
            AddKpiCard("DOANH THU THÁNG", kpis.MonthRevenue.ToString("N0"), Color.FromArgb(56, 142, 60));
            AddKpiCard("ĐƠN HÀNG HÔM NAY", kpis.TodayOrders.ToString(), Color.FromArgb(255, 160, 0));
            AddKpiCard("LỢI NHUẬN HÔM NAY", kpis.TodayProfit.ToString("N0"), Color.FromArgb(211, 47, 47));
            AddKpiCard("SẮP HẾT HÀNG", kpis.LowStockCount.ToString(), Color.FromArgb(123, 31, 162));

            // Revenue Chart
            var revData = _controller.GetRevenueChart(7);
            chartRevenue.Series[0].Points.Clear();
            foreach (var p in revData) chartRevenue.Series[0].Points.AddXY(p.Label, p.Value);

            // Products Chart
            var prodData = _controller.GetTopProducts();
            chartProducts.Series[0].Points.Clear();
            foreach (var p in prodData) chartProducts.Series[0].Points.AddXY(p.Label, p.Value);

            // Payment Chart
            var payData = _controller.GetPaymentMethods();
            chartPayment.Series[0].Points.Clear();
            foreach (var p in payData) chartPayment.Series[0].Points.AddXY(p.Label, p.Value);

            dgvRecentInvoices.DataSource = _controller.GetRecentInvoices();
            dgvLowStock.DataSource = _controller.GetLowStockAlert();
            FormatGrids();
        }

        private void FormatGrids()
        {
            // Recent Invoices
            if (dgvRecentInvoices.Columns["InvoiceNo"] != null) dgvRecentInvoices.Columns["InvoiceNo"].HeaderText = "Số HD";
            if (dgvRecentInvoices.Columns["CustomerName"] != null) dgvRecentInvoices.Columns["CustomerName"].HeaderText = "Khách hàng";
            if (dgvRecentInvoices.Columns["TotalAmount"] != null) dgvRecentInvoices.Columns["TotalAmount"].HeaderText = "Tổng tiền";
            if (dgvRecentInvoices.Columns["TimeAgo"] != null) dgvRecentInvoices.Columns["TimeAgo"].HeaderText = "Thời gian";

            if (dgvRecentInvoices.Columns["TotalAmount"] != null) dgvRecentInvoices.Columns["TotalAmount"].DefaultCellStyle.Format = "N0";

            // Low Stock
            if (dgvLowStock.Columns["ProductID"] != null) dgvLowStock.Columns["ProductID"].HeaderText = "Mã SP";
            if (dgvLowStock.Columns["ProductName"] != null) dgvLowStock.Columns["ProductName"].HeaderText = "Sản phẩm";
            if (dgvLowStock.Columns["CurrentStock"] != null) dgvLowStock.Columns["CurrentStock"].HeaderText = "Tồn";
            if (dgvLowStock.Columns["MinStockAlert"] != null) dgvLowStock.Columns["MinStockAlert"].HeaderText = "Mức báo";
            
            if (dgvLowStock.Columns["CurrentStock"] != null) dgvLowStock.Columns["CurrentStock"].DefaultCellStyle.Format = "N1";
        }

        private void AddKpiCard(string title, string value, Color color)
        {
            var card = new Panel { Width = 210, Height = 100, BackColor = Color.White, Margin = new Padding(5) };
            var lblTitle = new Label { Text = title, Dock = DockStyle.Top, Height = 35, TextAlign = ContentAlignment.BottomCenter, ForeColor = Color.Gray, Font = new Font("Segoe UI", 8, FontStyle.Bold) };
            var lblValue = new Label { Text = value, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, ForeColor = color, Font = new Font("Segoe UI", 16, FontStyle.Bold) };
            card.Controls.Add(lblValue);
            card.Controls.Add(lblTitle);
            pnlKpis.Controls.Add(card);
        }
    }
}
