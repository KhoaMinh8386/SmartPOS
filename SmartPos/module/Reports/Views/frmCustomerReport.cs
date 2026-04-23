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
            Text = "Bao cao Khach hang";
            Width = 900;
            Height = 650;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.White;

            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 250, Padding = new Padding(10) };
            chartRank = new Chart { Dock = DockStyle.Fill };
            chartRank.ChartAreas.Add(new ChartArea("Main"));
            chartRank.Series.Add(new Series("Hạng khách hàng") { ChartType = SeriesChartType.Pie });
            chartRank.Legends.Add(new Legend("Default"));
            pnlTop.Controls.Add(chartRank);

            dgvCustomers = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true
            };

            Controls.Add(dgvCustomers);
            Controls.Add(pnlTop);
        }

        private void LoadData()
        {
            var data = _controller.GetCustomerReport();
            dgvCustomers.DataSource = data;

            chartRank.Series[0].Points.Clear();
            var ranks = data.GroupBy(x => x.Rank)
                           .Select(g => new { Rank = g.Key, Count = g.Count() });
            
            foreach (var r in ranks)
            {
                chartRank.Series[0].Points.AddXY(r.Rank, r.Count);
            }
        }
    }
}
