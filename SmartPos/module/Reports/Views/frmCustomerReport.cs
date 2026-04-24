using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using SmartPos.Module.Reports.Controllers;

namespace SmartPos.Module.Reports.Views
{
    public class frmCustomerReport : Form
    {
        private readonly ReportController _controller;
        private Chart chartRank;
        private DataGridView dgvCustomers;

        public frmCustomerReport()
        {
            _controller = new ReportController();
            InitializeUi();
            LoadData();
        }

        private void InitializeUi()
        {
            Text = "BÁO CÁO PHÂN TÍCH KHÁCH HÀNG";
            Width = 1100;
            Height = 800;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(248, 250, 252);

            var pnlMain = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(20) };
            pnlMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 350));
            pnlMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // Chart Section
            var pnlChart = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(15) };
            pnlChart.Paint += (s, e) => {
                ControlPaint.DrawBorder(e.Graphics, pnlChart.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);
            };

            var lblChartTitle = new Label { Text = "PHÂN BỔ THÀNH VIÊN THEO HẠNG", Font = new Font("Segoe UI", 11F, FontStyle.Bold), Dock = DockStyle.Top, Height = 30, ForeColor = Color.FromArgb(30, 41, 59) };
            chartRank = new Chart { Dock = DockStyle.Fill, BackColor = Color.White };
            
            var area = new ChartArea("Main") { BackColor = Color.White };
            area.AxisX.MajorGrid.LineColor = Color.FromArgb(241, 245, 249);
            area.AxisY.MajorGrid.LineColor = Color.FromArgb(241, 245, 249);
            area.AxisX.LabelStyle.Font = new Font("Segoe UI", 9F);
            area.AxisY.LabelStyle.Font = new Font("Segoe UI", 9F);
            chartRank.ChartAreas.Add(area);

            var series = new Series("Hạng khách hàng") 
            { 
                ChartType = SeriesChartType.Column,
                CustomProperties = "PointWidth=0.6",
                IsValueShownAsLabel = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                LabelForeColor = Color.FromArgb(71, 85, 105)
            };
            chartRank.Series.Add(series);
            
            pnlChart.Controls.Add(chartRank);
            pnlChart.Controls.Add(lblChartTitle);
            pnlMain.Controls.Add(pnlChart, 0, 0);

            // Grid Section
            var pnlGrid = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(1), Margin = new Padding(0, 20, 0, 0) };
            dgvCustomers = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true,
                EnableHeadersVisualStyles = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowTemplate = { Height = 35 },
                GridColor = Color.FromArgb(241, 245, 249)
            };
            dgvCustomers.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(51, 65, 85);
            dgvCustomers.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvCustomers.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvCustomers.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);

            pnlGrid.Controls.Add(dgvCustomers);
            pnlMain.Controls.Add(pnlGrid, 0, 1);
            
            Controls.Add(pnlMain);
        }

        private void LoadData()
        {
            var data = _controller.GetCustomerReport();
            dgvCustomers.DataSource = data;
            FormatGrid();

            chartRank.Series[0].Points.Clear();
            var ranks = data.GroupBy(x => x.Rank)
                           .Select(g => new { Rank = g.Key, Count = g.Count() })
                           .OrderByDescending(x => x.Count);
            
            int colorIdx = 0;
            Color[] colors = { Color.FromArgb(59, 130, 246), Color.FromArgb(16, 185, 129), Color.FromArgb(245, 158, 11), Color.FromArgb(239, 68, 68) };

            foreach (var r in ranks)
            {
                var point = chartRank.Series[0].Points.AddXY(r.Rank, r.Count);
                chartRank.Series[0].Points[point].Color = colors[colorIdx % colors.Length];
                colorIdx++;
            }
        }

        private void FormatGrid()
        {
            if (dgvCustomers.Columns["CustomerID"] != null) dgvCustomers.Columns["CustomerID"].HeaderText = "ID";
            if (dgvCustomers.Columns["FullName"] != null) dgvCustomers.Columns["FullName"].HeaderText = "Họ tên";
            if (dgvCustomers.Columns["PhoneNumber"] != null) dgvCustomers.Columns["PhoneNumber"].HeaderText = "Số điện thoại";
            if (dgvCustomers.Columns["OrderCount"] != null) dgvCustomers.Columns["OrderCount"].HeaderText = "Số đơn";
            if (dgvCustomers.Columns["TotalSpent"] != null) dgvCustomers.Columns["TotalSpent"].HeaderText = "Tổng mua";
            if (dgvCustomers.Columns["Points"] != null) dgvCustomers.Columns["Points"].HeaderText = "Điểm";
            if (dgvCustomers.Columns["Rank"] != null) dgvCustomers.Columns["Rank"].HeaderText = "Hạng";

            if (dgvCustomers.Columns["TotalSpent"] != null) dgvCustomers.Columns["TotalSpent"].DefaultCellStyle.Format = "N0";
        }
    }
}
