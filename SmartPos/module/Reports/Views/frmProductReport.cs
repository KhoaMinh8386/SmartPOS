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
            Text = "Bao cao San pham";
            Width = 1000;
            Height = 700;
            StartPosition = FormStartPosition.CenterParent;

            tabMain = new TabControl { Dock = DockStyle.Fill };
            
            var tabOverview = new TabPage("Tong quan SP");
            dgvOverview = CreateGrid();
            tabOverview.Controls.Add(dgvOverview);

            var tabLowStock = new TabPage("Sap het hang");
            dgvLowStock = CreateGrid();
            tabLowStock.Controls.Add(dgvLowStock);

            var tabExpiry = new TabPage("Sap het han");
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
            // Overview: Performance for last 30 days
            dgvOverview.DataSource = _controller.GetProductPerformance(DateTime.Now.AddDays(-30), DateTime.Now);
            
            // Low Stock: Filtered from controller or separate method
            dgvLowStock.DataSource = _controller.GetLowStockAlert();

            // Expiry: Static sample for now or fetch
            // dgvExpiry.DataSource = ...
        }
    }
}
