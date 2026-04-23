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
            float y = 10;
            Font fontRegular = new Font("Arial", 8);
            Font fontBold = new Font("Arial", 8, FontStyle.Bold);
            Font fontTitle = new Font("Arial", 12, FontStyle.Bold);
            StringFormat center = new StringFormat { Alignment = StringAlignment.Center };
            RectangleF rect = new RectangleF(0, y, pageWidth, 20);

            // Header
            g.DrawString(config.StoreName.ToUpper(), fontTitle, Brushes.Black, new RectangleF(0, y, pageWidth, 25), center);
            y += 25;
            g.DrawString(config.Address, fontRegular, Brushes.Black, new RectangleF(0, y, pageWidth, 20), center);
            y += 15;
            g.DrawString("SĐT: " + config.Phone, fontRegular, Brushes.Black, new RectangleF(0, y, pageWidth, 20), center);
            y += 25;

            g.DrawString("HÓA ĐƠN BÁN HÀNG", fontTitle, Brushes.Black, new RectangleF(0, y, pageWidth, 25), center);
            y += 25;
            g.DrawString("Mã HĐ: " + invoice.InvoiceCode, fontRegular, Brushes.Black, 10, y);
            y += 15;
            g.DrawString("Ngày: " + invoice.InvoiceDate.ToString("dd/MM/yyyy HH:mm:ss"), fontRegular, Brushes.Black, 10, y);
            y += 15;
            g.DrawString("Thu ngân: " + invoice.StaffName, fontRegular, Brushes.Black, 10, y);
            y += 15;
            g.DrawString("Khách hàng: " + (invoice.CustomerName ?? "Khách lẻ"), fontRegular, Brushes.Black, 10, y);
            y += 20;

            g.DrawString(new string('-', 40), fontRegular, Brushes.Black, 10, y);
            y += 15;

            // Items Header
            g.DrawString("Tên SP", fontBold, Brushes.Black, 10, y);
            g.DrawString("SL", fontBold, Brushes.Black, pageWidth - 120, y);
            g.DrawString("Giá", fontBold, Brushes.Black, pageWidth - 90, y);
            g.DrawString("T.Tiền", fontBold, Brushes.Black, pageWidth - 50, y);
            y += 15;

            // Items
            foreach (var item in invoice.Items)
            {
                string name = item.ProductName;
                if (name.Length > 20) name = name.Substring(0, 18) + "..";
                
                g.DrawString(name, fontRegular, Brushes.Black, 10, y);
                g.DrawString(item.Quantity.ToString("0.#"), fontRegular, Brushes.Black, pageWidth - 120, y);
                g.DrawString((item.UnitPrice / 1000).ToString("N1") + "k", fontRegular, Brushes.Black, pageWidth - 90, y);
                g.DrawString((item.SubTotal / 1000).ToString("N1") + "k", fontRegular, Brushes.Black, pageWidth - 50, y);
                y += 15;
            }

            g.DrawString(new string('-', 40), fontRegular, Brushes.Black, 10, y);
            y += 15;

            // Totals
            DrawTotalLine(g, "Tạm tính:", invoice.TotalAmount + invoice.PaidAmount, fontRegular, pageWidth, ref y);
            // Note: TotalAmount here is already final, I'll fix this logic in InvoiceDetail
            y += 10;
            g.DrawString("TỔNG THANH TOÁN:", fontBold, Brushes.Black, 10, y);
            g.DrawString(invoice.TotalAmount.ToString("N0") + " đ", fontBold, Brushes.Black, pageWidth - 100, y);
            y += 20;

            g.DrawString(new string('=', 40), fontRegular, Brushes.Black, 10, y);
            y += 15;
            g.DrawString(config.FooterMessage, fontRegular, Brushes.Black, new RectangleF(0, y, pageWidth, 40), center);
        }

        private void DrawTotalLine(Graphics g, string label, decimal value, Font font, float width, ref float y)
        {
            g.DrawString(label, font, Brushes.Black, 10, y);
            g.DrawString(value.ToString("N0"), font, Brushes.Black, width - 80, y);
            y += 15;
        }
    }
}
