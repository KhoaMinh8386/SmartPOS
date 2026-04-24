using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Collections.Generic;
using System.Windows.Forms;
namespace SmartPos.Module.Pos
{
    public class PrintHelper
    {
        public void PrintInvoice(InvoiceDetail invoice, StoreConfig config)
        {
            PrintDocument pd = new PrintDocument();
            pd.PrintPage += (s, e) => DrawInvoice(e.Graphics, invoice, config, e.PageSettings.PrintableArea.Width);
            
            PrintPreviewDialog ppd = new PrintPreviewDialog { Document = pd };
            ppd.ShowDialog();
        }

        public void DrawInvoice(Graphics g, InvoiceDetail invoice, StoreConfig config, float pageWidth)
        {
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Define receipt width (simulating 80mm paper)
            float receiptWidth = 300;
            float margin = 10;
            float contentWidth = receiptWidth - 2 * margin;
            
            // Center the receipt on the page if it's wider (like A4)
            float xOffset = (pageWidth - receiptWidth) / 2;
            if (xOffset < 0) xOffset = 0;

            float y = 10;
            Font fontRegular = new Font("Arial", 9);
            Font fontBold = new Font("Arial", 9, FontStyle.Bold);
            Font fontTitle = new Font("Arial", 14, FontStyle.Bold);
            Font fontSmall = new Font("Arial", 8);
            
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
            g.DrawString("SMART POS SUPERMARKET", fontTitle, Brushes.Black, new RectangleF(xOffset + margin, y, contentWidth, 50), center);
            y += 45;
            g.DrawString("84 Phú Thọ, Quận 11, TP.HCM", fontRegular, Brushes.Black, new RectangleF(xOffset + margin, y, contentWidth, 20), center);
            y += 18;
            g.DrawString("SĐT: 0900.123.456", fontRegular, Brushes.Black, new RectangleF(xOffset + margin, y, contentWidth, 20), center);
            y += 25;

            // Title
            g.DrawString("HÓA ĐƠN BÁN HÀNG", fontTitle, Brushes.Black, new RectangleF(xOffset + margin, y, contentWidth, 30), center);
            y += 35;

            // 2. Invoice Info
            g.DrawString($"Mã HĐ: {invoice.InvoiceCode}", fontRegular, Brushes.Black, xOffset + margin, y);
            y += 18;
            g.DrawString($"Ngày: {invoice.InvoiceDate:dd/MM/yyyy HH:mm:ss}", fontRegular, Brushes.Black, xOffset + margin, y);
            y += 18;
            g.DrawString($"Thu ngân: {invoice.StaffName}", fontRegular, Brushes.Black, xOffset + margin, y);
            y += 18;
            g.DrawString($"Khách hàng: {(invoice.FullName ?? "Khách lẻ")}", fontRegular, Brushes.Black, xOffset + margin, y);
            y += 25;

            // 3. Items Table Header
            g.DrawLine(Pens.Black, xOffset + margin, y, xOffset + receiptWidth - margin, y);
            y += 5;
            g.DrawString("Tên SP", fontBold, Brushes.Black, xOffset + margin, y);
            g.DrawString("SL", fontBold, Brushes.Black, xOffset + receiptWidth - 110, y, right);
            g.DrawString("Giá", fontBold, Brushes.Black, xOffset + receiptWidth - 60, y, right);
            g.DrawString("T.Tiền", fontBold, Brushes.Black, xOffset + receiptWidth - margin, y, right);
            y += 20;
            g.DrawLine(Pens.Black, xOffset + margin, y, xOffset + receiptWidth - margin, y);
            y += 5;

            // 4. Items List
            foreach (var item in invoice.Items)
            {
                string name = item.ProductName;
                if (name.Length > 22) name = name.Substring(0, 20) + "..";

                g.DrawString(name, fontRegular, Brushes.Black, xOffset + margin, y);
                y += 15;
                g.DrawString(item.Quantity.ToString("0.#"), fontRegular, Brushes.Black, xOffset + receiptWidth - 110, y - 15, right);
                g.DrawString(item.UnitPrice.ToString("N0"), fontRegular, Brushes.Black, xOffset + receiptWidth - 60, y - 15, right);
                g.DrawString(item.SubTotal.ToString("N0"), fontRegular, Brushes.Black, xOffset + receiptWidth - margin, y - 15, right);
            }

            // 5. Financial Summary
            y += 10;
            g.DrawLine(Pens.Black, xOffset + margin, y, xOffset + receiptWidth - margin, y);
            y += 10;

            DrawSummaryLine(g, "Tạm tính:", invoice.SubTotal, fontRegular, xOffset + margin, contentWidth, ref y);
            if (invoice.VoucherDiscount > 0)
                DrawSummaryLine(g, "Voucher:", invoice.VoucherDiscount, fontRegular, xOffset + margin, contentWidth, ref y);
            if (invoice.PointsDiscount > 0)
                DrawSummaryLine(g, "Điểm đổi:", invoice.PointsDiscount, fontRegular, xOffset + margin, contentWidth, ref y);

            y += 10;
            g.DrawString("TỔNG THANH TOÁN:", fontBold, Brushes.Black, xOffset + margin, y);
            g.DrawString(invoice.TotalAmount.ToString("N0") + " đ", fontTitle, Brushes.Black, xOffset + receiptWidth - margin, y - 5, right);
            y += 40;

            // 6. Footer
            g.DrawLine(Pens.Black, xOffset + margin, y, xOffset + receiptWidth - margin, y);
            y += 10;
            g.DrawString("Cảm ơn quý khách! Hẹn gặp lại!", fontRegular, Brushes.Black, new RectangleF(xOffset + margin, y, contentWidth, 20), center);
            y += 20;
            g.DrawString("Vui lòng kiểm tra hàng trước khi rời quầy.", fontSmall, Brushes.Black, new RectangleF(xOffset + margin, y, contentWidth, 20), center);
        }

        private void DrawSummaryLine(Graphics g, string label, decimal value, Font font, float xStart, float width, ref float y)
        {
            g.DrawString(label, font, Brushes.Black, xStart, y);
            g.DrawString(value.ToString("N0"), font, Brushes.Black, xStart + width, y, new StringFormat { Alignment = StringAlignment.Far });
            y += 18;
        }
    }
}
