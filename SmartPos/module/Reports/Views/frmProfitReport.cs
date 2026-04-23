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

        public frmProfitReport()
        {
            _controller = new ReportController();
            InitializeUi();
            LoadData();
        }

        private void InitializeUi()
        {
            Text = "Bao cao Loi nhuan";
            Width = 900;
            Height = 650;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.White;

            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.FromArgb(240, 242, 245), Padding = new Padding(10) };
            dtpFrom = new DateTimePicker { Location = new Point(20, 20), Width = 150 };
            dtpTo = new DateTimePicker { Location = new Point(180, 20), Width = 150 };
            var btnView = new Button { Text = "Xem bao cao", Location = new Point(350, 18), Width = 120, Height = 30, BackColor = Color.FromArgb(25, 118, 210), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnView.Click += (s, e) => LoadData();
            pnlTop.Controls.AddRange(new Control[] { dtpFrom, dtpTo, btnView });

            chartProfit = new Chart { Dock = DockStyle.Top, Height = 250 };
            chartProfit.ChartAreas.Add(new ChartArea("Main"));
            chartProfit.Series.Add(new Series("Loi nhuan gop") { ChartType = SeriesChartType.Pie });
            chartProfit.Legends.Add(new Legend("Default"));

            dgvProfit = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true
            };

            Controls.Add(dgvProfit);
            Controls.Add(chartProfit);
            Controls.Add(pnlTop);
        }

        private void LoadData()
        {
            var data = _controller.GetProfitReport(dtpFrom.Value, dtpTo.Value);
            dgvProfit.DataSource = data;

            chartProfit.Series[0].Points.Clear();
            foreach (var item in data)
            {
                chartProfit.Series[0].Points.AddXY(item.CategoryName, item.Profit);
            }
        }
    }
}
