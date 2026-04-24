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
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Define receipt width (simulating 80mm paper)
            float receiptWidth = 300;
            float margin = 10;
            float contentWidth = receiptWidth - 2 * margin;
            
            // Center the receipt on the page if it's wider (like A4)
            float xOffset = (e.PageBounds.Width - receiptWidth) / 2;
            if (xOffset < 0) xOffset = 0;

            float y = 15;
            StringFormat center = new StringFormat { Alignment = StringAlignment.Center };
            StringFormat right = new StringFormat { Alignment = StringAlignment.Far };

            // 0. Logo
            string logoPath = @"c:\Users\Admin\Projects\SmartPos\SmartPos\access\img\logo.png";
            if (System.IO.File.Exists(logoPath))
            {
                using (Image logo = Image.FromFile(logoPath))
                {
                    float logoSize = 60;
                    g.DrawImage(logo, xOffset + (receiptWidth - logoSize) / 2, y, logoSize, logoSize);
                    y += logoSize + 10;
                }
            }

            // 1. Header - Store Info
            g.DrawString("SMART POS SUPERMARKET", _fontHeader, Brushes.Black, new RectangleF(xOffset + margin, y, contentWidth, 50), center);
            y += 45;
            g.DrawString("84 Phú Thọ, Quận 11, TP.HCM", _fontRegular, Brushes.Black, new RectangleF(xOffset + margin, y, contentWidth, 20), center);
            y += 18;
            g.DrawString("SĐT: 0900.123.456", _fontRegular, Brushes.Black, new RectangleF(xOffset + margin, y, contentWidth, 20), center);
            y += 25;

            // Title
            g.DrawString("HÓA ĐƠN BÁN HÀNG", _fontHeader, Brushes.Black, new RectangleF(xOffset + margin, y, contentWidth, 30), center);
            y += 35;

            // 2. Invoice Info
            g.DrawString($"Mã HĐ: {_detail.InvoiceCode}", _fontRegular, Brushes.Black, xOffset + margin, y);
            y += 18;
            g.DrawString($"Ngày: {_detail.InvoiceDate:dd/MM/yyyy HH:mm:ss}", _fontRegular, Brushes.Black, xOffset + margin, y);
            y += 18;
            g.DrawString($"Thu ngân: {_detail.StaffName}", _fontRegular, Brushes.Black, xOffset + margin, y);
            y += 18;
            g.DrawString($"Khách hàng: {(_detail.FullName ?? "Khách lẻ")}", _fontRegular, Brushes.Black, xOffset + margin, y);
            y += 25;

            // 3. Items Table Header
            g.DrawLine(Pens.Black, xOffset + margin, y, xOffset + receiptWidth - margin, y);
            y += 5;
            g.DrawString("Tên SP", _fontBold, Brushes.Black, xOffset + margin, y);
            g.DrawString("SL", _fontBold, Brushes.Black, xOffset + receiptWidth - 110, y, right);
            g.DrawString("Giá", _fontBold, Brushes.Black, xOffset + receiptWidth - 60, y, right);
            g.DrawString("T.Tiền", _fontBold, Brushes.Black, xOffset + receiptWidth - margin, y, right);
            y += 20;
            g.DrawLine(Pens.Black, xOffset + margin, y, xOffset + receiptWidth - margin, y);
            y += 5;

            // 4. Items List
            foreach (var item in _detail.Items)
            {
                string name = item.ProductName;
                if (name.Length > 22) name = name.Substring(0, 20) + "..";

                g.DrawString(name, _fontRegular, Brushes.Black, xOffset + margin, y);
                y += 15;
                // Draw quantity and price on next line if needed, but here we align them
                g.DrawString(item.Quantity.ToString("0.#"), _fontRegular, Brushes.Black, xOffset + receiptWidth - 110, y - 15, right);
                g.DrawString(item.UnitPrice.ToString("N0"), _fontRegular, Brushes.Black, xOffset + receiptWidth - 60, y - 15, right);
                g.DrawString(item.SubTotal.ToString("N0"), _fontRegular, Brushes.Black, xOffset + receiptWidth - margin, y - 15, right);
                // y += 5; // spacing between items
            }

            // 5. Financial Summary
            y += 10;
            g.DrawLine(Pens.Black, xOffset + margin, y, xOffset + receiptWidth - margin, y);
            y += 10;

            DrawSummaryLine(g, "Tạm tính:", _detail.TotalAmount + _detail.DiscountAmount + _detail.VoucherDiscount + _detail.LoyaltyDiscount, ref y, xOffset + margin, contentWidth);
            if (_detail.DiscountAmount > 0)
                DrawSummaryLine(g, "Giảm giá:", _detail.DiscountAmount, ref y, xOffset + margin, contentWidth);
            if (_detail.VoucherDiscount > 0)
                DrawSummaryLine(g, "Voucher:", _detail.VoucherDiscount, ref y, xOffset + margin, contentWidth);
            if (_detail.LoyaltyDiscount > 0)
                DrawSummaryLine(g, "Điểm đổi:", _detail.LoyaltyDiscount, ref y, xOffset + margin, contentWidth);

            y += 10;
            g.DrawString("TỔNG THANH TOÁN:", _fontBold, Brushes.Black, xOffset + margin, y);
            g.DrawString(_detail.FinalAmount.ToString("N0") + " đ", _fontHeader, Brushes.Black, xOffset + receiptWidth - margin, y - 5, right);
            y += 40;

            // 6. Footer
            g.DrawLine(Pens.Black, xOffset + margin, y, xOffset + receiptWidth - margin, y);
            y += 10;
            g.DrawString("Cảm ơn quý khách! Hẹn gặp lại!", _fontRegular, Brushes.Black, new RectangleF(xOffset + margin, y, contentWidth, 20), center);
            y += 20;
            g.DrawString("Vui lòng kiểm tra hàng trước khi rời quầy.", _fontSmall, Brushes.Black, new RectangleF(xOffset + margin, y, contentWidth, 20), center);
            
        }

        private void DrawSummaryLine(Graphics g, string label, decimal value, ref float y, float xStart, float width)
        {
            g.DrawString(label, _fontRegular, Brushes.Black, xStart, y);
            g.DrawString(value.ToString("N0"), _fontRegular, Brushes.Black, xStart + width, y, new StringFormat { Alignment = StringAlignment.Far });
            y += 18;
        }
    }
}
