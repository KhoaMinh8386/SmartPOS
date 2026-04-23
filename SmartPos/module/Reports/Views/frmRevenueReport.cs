using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using SmartPos.Module.Reports.Controllers;

namespace SmartPos.Module.Reports.Views
{
    public class frmRevenueReport : Form
    {
        private readonly ReportController _controller;
        private DateTimePicker dtpFrom, dtpTo;
        private Chart chartRevenue;
        private DataGridView dgvDetails;
        private Label lblTotalRev, lblTotalCost, lblTotalProfit, lblTotalOrders;

        public frmRevenueReport()
        {
            _controller = new ReportController();
            InitializeUi();
            LoadData();
        }

        private void InitializeUi()
        {
            Text = "Báo cáo Doanh thu";
            Width = 1000;
            Height = 700;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.White;

            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.FromArgb(240, 242, 245), Padding = new Padding(10) };
            dtpFrom = new DateTimePicker { Location = new Point(20, 20), Width = 150 };
            dtpTo = new DateTimePicker { Location = new Point(180, 20), Width = 150 };
            var btnView = new Button { Text = "Xem báo cáo", Location = new Point(350, 18), Width = 120, Height = 30, BackColor = Color.FromArgb(25, 118, 210), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnView.Click += (s, e) => LoadData();
            pnlTop.Controls.AddRange(new Control[] { dtpFrom, dtpTo, btnView });

            var pnlStats = new Panel { Dock = DockStyle.Top, Height = 100, Padding = new Padding(10) };
            lblTotalRev = CreateStatLabel("DOANH THU", new Point(20, 20));
            lblTotalCost = CreateStatLabel("GIÁ VỐN", new Point(220, 20));
            lblTotalProfit = CreateStatLabel("LỢI NHUẬN", new Point(420, 20));
            lblTotalOrders = CreateStatLabel("SỐ ĐƠN", new Point(620, 20));
            pnlStats.Controls.AddRange(new Control[] { lblTotalRev, lblTotalCost, lblTotalProfit, lblTotalOrders });

            chartRevenue = new Chart { Dock = DockStyle.Top, Height = 250 };
            chartRevenue.ChartAreas.Add(new ChartArea("Main"));
            chartRevenue.Series.Add(new Series("Doanh thu") { ChartType = SeriesChartType.Column });
            chartRevenue.Series.Add(new Series("Lợi nhuận") { ChartType = SeriesChartType.Line, BorderWidth = 3 });

            dgvDetails = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true
            };

            Controls.Add(dgvDetails);
            Controls.Add(chartRevenue);
            Controls.Add(pnlStats);
            Controls.Add(pnlTop);
        }

        private Label CreateStatLabel(string title, Point loc)
        {
            return new Label { Text = title + ": 0", Location = loc, AutoSize = true, Font = new Font("Segoe UI", 12, FontStyle.Bold) };
        }

        private void LoadData()
        {
            var data = _controller.GetProductPerformance(dtpFrom.Value, dtpTo.Value);
            
            decimal rev = data.Sum(x => x.Revenue);
            decimal cost = data.Sum(x => x.SoldQuantity * x.CostPrice);
            decimal profit = rev - cost;
            
            lblTotalRev.Text = "DOANH THU: " + rev.ToString("N0");
            lblTotalCost.Text = "GIÁ VỐN: " + cost.ToString("N0");
            lblTotalProfit.Text = "LỢI NHUẬN: " + profit.ToString("N0");
            lblTotalOrders.Text = "SỐ ĐƠN: " + data.Count(x => x.SoldQuantity > 0).ToString();
            
            chartRevenue.Series[0].Points.Clear();
            chartRevenue.Series[1].Points.Clear();
            
            foreach (var item in data.Take(10))
            {
                chartRevenue.Series[0].Points.AddXY(item.ProductName, item.Revenue);
                chartRevenue.Series[1].Points.AddXY(item.ProductName, item.Revenue - (item.SoldQuantity * item.CostPrice));
            }

            dgvDetails.DataSource = data;
            FormatGrid();
        }

        private void FormatGrid()
        {
            if (dgvDetails.Columns["ProductID"] != null) dgvDetails.Columns["ProductID"].HeaderText = "Mã SP";
            if (dgvDetails.Columns["ProductName"] != null) dgvDetails.Columns["ProductName"].HeaderText = "Tên sản phẩm";
            if (dgvDetails.Columns["SoldQuantity"] != null) dgvDetails.Columns["SoldQuantity"].HeaderText = "Đã bán";
            if (dgvDetails.Columns["Revenue"] != null) dgvDetails.Columns["Revenue"].HeaderText = "Doanh thu";
            if (dgvDetails.Columns["CostPrice"] != null) dgvDetails.Columns["CostPrice"].HeaderText = "Giá vốn";
            
            if (dgvDetails.Columns["Revenue"] != null) dgvDetails.Columns["Revenue"].DefaultCellStyle.Format = "N0";
            if (dgvDetails.Columns["CostPrice"] != null) dgvDetails.Columns["CostPrice"].DefaultCellStyle.Format = "N0";
            if (dgvDetails.Columns["SoldQuantity"] != null) dgvDetails.Columns["SoldQuantity"].DefaultCellStyle.Format = "N2";
        }
    }
}
