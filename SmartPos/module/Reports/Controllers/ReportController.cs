using System;
using System.Collections.Generic;
using System.Data;
using SmartPos.Module.Reports.Backend;
using SmartPos.Module.Reports.Models;

namespace SmartPos.Module.Reports.Controllers
{
    public class ReportController
    {
        private readonly ReportBackend _backend;

        public ReportController()
        {
            _backend = new ReportBackend();
        }

        public KpiData GetDashboardKpis() => _backend.GetDashboardKpis();
        public List<ChartDataPoint> GetRevenueChart(int days) => _backend.GetRevenueChart(days);
        public List<ChartDataPoint> GetTopProducts() => _backend.GetTopProducts();
        public List<ChartDataPoint> GetPaymentMethods() => _backend.GetPaymentMethods();
        public DataTable GetRecentInvoices() => _backend.GetRecentInvoices();
        public DataTable GetLowStockAlert() => _backend.GetLowStockAlert();
        public List<ProductReportItem> GetProductPerformance(DateTime from, DateTime to) => _backend.GetProductPerformance(from, to);
        public List<ProductReportItem> GetNearExpiryItems(int days) => _backend.GetNearExpiryItems(days);
        public List<CustomerReportItem> GetCustomerReport() => _backend.GetCustomerReport();
        public List<ProfitReportItem> GetProfitReport(DateTime from, DateTime to) => _backend.GetProfitReport(from, to);
        
        // Wrapper cho Lô & Hạn sử dụng
        public List<BatchReportItem> GetAllBatches(int warehouseID = 0) => _backend.GetAllBatches(warehouseID);
        public List<BatchReportItem> GetBatchesByProduct(int productID) => _backend.GetBatchesByProduct(productID);
    }
}
