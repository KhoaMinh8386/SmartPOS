using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using SmartPos.Module.Reports.Controllers;

namespace SmartPos.Module.Reports.Views
{
    public class frmProfitReport : Form
    {
        private readonly ReportController _controller;
        private DateTimePicker dtpFrom, dtpTo;
        private Chart chartProfit;
        private DataGridView dgvProfit;
        private Label lblTotalRev, lblTotalCost, lblTotalProfit;

        public frmProfitReport()
        {
            _controller = new ReportController();
            InitializeUi();
            LoadData();
        }

        private void InitializeUi()
        {
            Text = "BÁO CÁO PHÂN TÍCH LỢI NHUẬN THEO DANH MỤC";
            Width = 1150;
            Height = 850;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(248, 250, 252);

            var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = Color.White, Padding = new Padding(20, 15, 20, 15) };
            pnlHeader.Paint += (s, e) => {
                ControlPaint.DrawBorder(e.Graphics, pnlHeader.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);
            };

            var lblFrom = new Label { Text = "Từ ngày:", Location = new Point(20, 28), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            dtpFrom = new DateTimePicker { Location = new Point(85, 25), Width = 130, Format = DateTimePickerFormat.Short, Font = new Font("Segoe UI", 10F) };
            
            var lblTo = new Label { Text = "Đến ngày:", Location = new Point(230, 28), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            dtpTo = new DateTimePicker { Location = new Point(300, 25), Width = 130, Format = DateTimePickerFormat.Short, Font = new Font("Segoe UI", 10F) };
            
            var btnView = new Button 
            { 
                Text = "LẤY DỮ LIỆU", 
                Location = new Point(450, 22), 
                Width = 130, Height = 36, 
                BackColor = Color.FromArgb(51, 65, 85), 
                ForeColor = Color.White, 
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnView.Click += (s, e) => LoadData();
            pnlHeader.Controls.AddRange(new Control[] { lblFrom, dtpFrom, lblTo, dtpTo, btnView });

            // Summary Cards
            var pnlKpi = new TableLayoutPanel { Dock = DockStyle.Top, Height = 100, ColumnCount = 3, RowCount = 1, Padding = new Padding(20, 10, 20, 10) };
            pnlKpi.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            pnlKpi.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            pnlKpi.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));

            lblTotalRev = CreateKpiCard(pnlKpi, "TỔNG DOANH THU", Color.FromArgb(59, 130, 246), 0);
            lblTotalCost = CreateKpiCard(pnlKpi, "TỔNG GIÁ VỐN", Color.FromArgb(100, 116, 139), 1);
            lblTotalProfit = CreateKpiCard(pnlKpi, "LỢI NHUẬN RÒNG", Color.FromArgb(16, 185, 129), 2);

            var pnlMain = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(20) };
            pnlMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            pnlMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));

            // Chart Card
            var pnlChartCard = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(15), Margin = new Padding(0, 0, 10, 0) };
            pnlChartCard.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlChartCard.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);
            
            var lblChartTitle = new Label { Text = "TỶ LỆ LỢI NHUẬN", Font = new Font("Segoe UI", 10F, FontStyle.Bold), Dock = DockStyle.Top, Height = 30 };
            chartProfit = new Chart { Dock = DockStyle.Fill, BackColor = Color.White };
            var area = new ChartArea("Main") { BackColor = Color.White };
            area.AxisX.MajorGrid.Enabled = false;
            area.AxisY.MajorGrid.LineColor = Color.FromArgb(241, 245, 249);
            area.AxisX.LabelStyle.Font = new Font("Segoe UI", 8.5F);
            area.AxisY.LabelStyle.Format = "N0";
            chartProfit.ChartAreas.Add(area);
            var series = new Series("Profit") { ChartType = SeriesChartType.Bar, CustomProperties = "PointWidth=0.7, BarLabelStyle=Outside" };
            chartProfit.Series.Add(series);
            series.IsValueShownAsLabel = true;
            series.LabelFormat = "N0";
            series.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            
            pnlChartCard.Controls.Add(chartProfit);
            pnlChartCard.Controls.Add(lblChartTitle);

            // Grid Card
            var pnlGridCard = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(1) };
            dgvProfit = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true,
                EnableHeadersVisualStyles = false,
                RowTemplate = { Height = 35 },
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                GridColor = Color.FromArgb(241, 245, 249)
            };
            dgvProfit.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(51, 65, 85);
            dgvProfit.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvProfit.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvProfit.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);

            pnlGridCard.Controls.Add(dgvProfit);

            pnlMain.Controls.Add(pnlChartCard, 0, 0);
            pnlMain.Controls.Add(pnlGridCard, 1, 0);

            Controls.Add(pnlMain);
            Controls.Add(pnlKpi);
            Controls.Add(pnlHeader);
        }

        private Label CreateKpiCard(TableLayoutPanel parent, string title, Color color, int col)
        {
            var pnl = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Margin = new Padding(5) };
            pnl.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnl.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);
            
            var lblTitle = new Label { Text = title, Dock = DockStyle.Top, Height = 25, Font = new Font("Segoe UI", 8F, FontStyle.Bold), ForeColor = Color.Gray, TextAlign = ContentAlignment.BottomCenter };
            var lblVal = new Label { Text = "0", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 16F, FontStyle.Bold), ForeColor = color, TextAlign = ContentAlignment.MiddleCenter };
            
            pnl.Controls.Add(lblVal);
            pnl.Controls.Add(lblTitle);
            parent.Controls.Add(pnl, col, 0);
            return lblVal;
        }

        private void LoadData()
        {
            var data = _controller.GetProfitReport(dtpFrom.Value, dtpTo.Value);
            dgvProfit.DataSource = data;
            FormatGrid();

            decimal totalRev = data.Sum(x => x.Revenue);
            decimal totalCost = data.Sum(x => x.Cost);
            decimal totalProfit = totalRev - totalCost;

            lblTotalRev.Text = totalRev.ToString("N0");
            lblTotalCost.Text = totalCost.ToString("N0");
            lblTotalProfit.Text = totalProfit.ToString("N0");

            chartProfit.Series[0].Points.Clear();
            Color[] colors = { Color.FromArgb(59, 130, 246), Color.FromArgb(16, 185, 129), Color.FromArgb(245, 158, 11), Color.FromArgb(239, 68, 68), Color.FromArgb(139, 92, 246) };
            int i = 0;
            foreach (var item in data.Where(x => x.Profit > 0).OrderByDescending(x => x.Profit))
            {
                var p = chartProfit.Series[0].Points.AddXY(item.CategoryName, item.Profit);
                chartProfit.Series[0].Points[p].Color = colors[i % colors.Length];
                i++;
            }
        }

        private void FormatGrid()
        {
            if (dgvProfit.Columns["CategoryName"] != null) dgvProfit.Columns["CategoryName"].HeaderText = "Danh mục";
            if (dgvProfit.Columns["Revenue"] != null) dgvProfit.Columns["Revenue"].HeaderText = "Doanh thu";
            if (dgvProfit.Columns["Cost"] != null) dgvProfit.Columns["Cost"].HeaderText = "Giá vốn";
            if (dgvProfit.Columns["Profit"] != null) dgvProfit.Columns["Profit"].HeaderText = "Lợi nhuận";

            if (dgvProfit.Columns["Revenue"] != null) dgvProfit.Columns["Revenue"].DefaultCellStyle.Format = "N0";
            if (dgvProfit.Columns["Cost"] != null) dgvProfit.Columns["Cost"].DefaultCellStyle.Format = "N0";
            if (dgvProfit.Columns["Profit"] != null) dgvProfit.Columns["Profit"].DefaultCellStyle.Format = "N0";
            
            foreach(DataGridViewColumn col in dgvProfit.Columns)
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            if (dgvProfit.Columns["CategoryName"] != null) 
                dgvProfit.Columns["CategoryName"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
        }
    }
}
