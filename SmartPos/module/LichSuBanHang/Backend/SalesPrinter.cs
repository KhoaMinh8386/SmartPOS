using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using SmartPos.Module.SalesHistory.Models;

namespace SmartPos.Module.SalesHistory.Backend
{
    public class SalesPrinter
    {
        private SalesOrderDetail _detail;
        private Font _fontRegular = new Font("Arial", 9);
        private Font _fontBold = new Font("Arial", 9, FontStyle.Bold);
        private Font _fontHeader = new Font("Arial", 14, FontStyle.Bold);
        private Font _fontSmall = new Font("Arial", 8);

        public void PrintInvoice(SalesOrderDetail detail)
        {
            _detail = detail;
            PrintDocument pd = new PrintDocument();
            pd.PrintPage += new PrintPageEventHandler(pd_PrintPage);
            
            PrintDialog pdi = new PrintDialog();
            pdi.Document = pd;
            
            // For demo, show preview or just print to default
            pd.Print();
        }

        private void pd_PrintPage(object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            float y = 20;
            float margin = 20;
            float width = e.PageSettings.PrintableArea.Width - 2 * margin;

            // 1. Header - Store Info
            g.DrawString("SMART POS RETAIL", _fontHeader, Brushes.Black, new RectangleF(margin, y, width, 30), new StringFormat { Alignment = StringAlignment.Center });
            y += 30;
            g.DrawString("123 Duong ABC, Quan 1, TP. HCM", _fontSmall, Brushes.Black, new RectangleF(margin, y, width, 20), new StringFormat { Alignment = StringAlignment.Center });
            y += 15;
            g.DrawString("Hotline: 1900 xxxx | Website: www.smartpos.vn", _fontSmall, Brushes.Black, new RectangleF(margin, y, width, 20), new StringFormat { Alignment = StringAlignment.Center });
            y += 25;

            // Separator
            g.DrawLine(Pens.Black, margin, y, margin + width, y);
            y += 10;

            // 2. Invoice Info
            g.DrawString("HOA DON BAN HANG", _fontBold, Brushes.Black, new RectangleF(margin, y, width, 20), new StringFormat { Alignment = StringAlignment.Center });
            y += 25;
            
            g.DrawString($"Ma HD: {_detail.InvoiceCode}", _fontRegular, Brushes.Black, margin, y);
            y += 18;
            g.DrawString($"Ngay: {_detail.InvoiceDate:dd/MM/yyyy HH:mm}", _fontRegular, Brushes.Black, margin, y);
            y += 18;
            g.DrawString($"Thu ngan: {_detail.StaffName}", _fontRegular, Brushes.Black, margin, y);
            y += 18;
            g.DrawString($"Khach hang: {_detail.CustomerName}", _fontRegular, Brushes.Black, margin, y);
            y += 25;

            // 3. Items Table Header
            g.DrawLine(Pens.Black, margin, y, margin + width, y);
            y += 5;
            g.DrawString("Ten SP", _fontBold, Brushes.Black, margin, y);
            g.DrawString("SL", _fontBold, Brushes.Black, margin + 150, y, new StringFormat { Alignment = StringAlignment.Far });
            g.DrawString("Don gia", _fontBold, Brushes.Black, margin + 220, y, new StringFormat { Alignment = StringAlignment.Far });
            g.DrawString("T.Tien", _fontBold, Brushes.Black, margin + width, y, new StringFormat { Alignment = StringAlignment.Far });
            y += 20;
            g.DrawLine(Pens.Black, margin, y, margin + width, y);
            y += 5;

            // 4. Items List
            foreach (var item in _detail.Items)
            {
                string name = item.ProductName;
                if (name.Length > 25) name = name.Substring(0, 22) + "...";

                g.DrawString(name, _fontRegular, Brushes.Black, margin, y);
                g.DrawString(item.Quantity.ToString("N0"), _fontRegular, Brushes.Black, margin + 150, y, new StringFormat { Alignment = StringAlignment.Far });
                g.DrawString(item.UnitPrice.ToString("N0"), _fontRegular, Brushes.Black, margin + 220, y, new StringFormat { Alignment = StringAlignment.Far });
                g.DrawString(item.SubTotal.ToString("N0"), _fontRegular, Brushes.Black, margin + width, y, new StringFormat { Alignment = StringAlignment.Far });
                y += 18;
            }

            // 5. Financial Summary
            y += 10;
            g.DrawLine(Pens.Black, margin, y, margin + width, y);
            y += 10;

            DrawSummaryLine(g, "Tong tien hang:", _detail.TotalAmount, ref y, margin, width);
            if (_detail.DiscountAmount > 0)
                DrawSummaryLine(g, "Giam gia:", _detail.DiscountAmount, ref y, margin, width);
            if (_detail.VoucherDiscount > 0)
                DrawSummaryLine(g, "Voucher:", _detail.VoucherDiscount, ref y, margin, width);
            if (_detail.LoyaltyDiscount > 0)
                DrawSummaryLine(g, "Diem doi:", _detail.LoyaltyDiscount, ref y, margin, width);

            y += 5;
            g.DrawString("THANH TOAN:", _fontBold, Brushes.Black, margin, y);
            g.DrawString(_detail.PaidAmount.ToString("N0"), _fontHeader, Brushes.Black, margin + width, y - 5, new StringFormat { Alignment = StringAlignment.Far });
            y += 35;

            // 6. Footer
            g.DrawString("Cam on Quy khach. Hen gap lai!", _fontRegular, Brushes.Black, new RectangleF(margin, y, width, 20), new StringFormat { Alignment = StringAlignment.Center });
            y += 20;
            g.DrawString("Vui long kiem tra hang truoc khi roi quay.", _fontSmall, Brushes.Black, new RectangleF(margin, y, width, 20), new StringFormat { Alignment = StringAlignment.Center });
            
        }

        private void DrawSummaryLine(Graphics g, string label, decimal value, ref float y, float margin, float width)
        {
            g.DrawString(label, _fontRegular, Brushes.Black, margin + 120, y);
            g.DrawString(value.ToString("N0"), _fontRegular, Brushes.Black, margin + width, y, new StringFormat { Alignment = StringAlignment.Far });
            y += 18;
        }
    }
}
