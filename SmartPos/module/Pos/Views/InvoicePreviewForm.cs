using System;
using System.Drawing;
using System.Windows.Forms;
namespace SmartPos.Module.Pos
{
    public partial class InvoicePreviewForm : Form
    {
        private readonly InvoiceDetail _invoice;
        private readonly StoreConfig _config;
        private readonly PrintHelper _printHelper;

        public InvoicePreviewForm(InvoiceDetail invoice, StoreConfig config)
        {
            InitializeComponent();
            _invoice = invoice;
            _config = config;
            _printHelper = new PrintHelper();
            
            this.Text = "Xem trước hóa đơn - " + invoice.InvoiceCode;
            this.Size = new Size(500, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
        }

        private void InitializeComponent()
        {
            Panel pnlPreview = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.White };
            pnlPreview.Paint += PnlPreview_Paint;

            Panel pnlActions = new Panel { Dock = DockStyle.Right, Width = 150, BackColor = Color.FromArgb(240, 240, 240) };
            
            Button btnPrint = CreateActionButton("IN HÓA ĐƠN", Color.FromArgb(46, 125, 50), 20);
            btnPrint.Click += (s, e) => { _printHelper.PrintInvoice(_invoice, _config); this.Close(); };

            Button btnClose = CreateActionButton("ĐÓNG", Color.FromArgb(117, 117, 117), 80);
            btnClose.Click += (s, e) => this.Close();

            pnlActions.Controls.Add(btnPrint);
            pnlActions.Controls.Add(btnClose);

            this.Controls.Add(pnlPreview);
            this.Controls.Add(pnlActions);
        }

        private Button CreateActionButton(string text, Color color, int y)
        {
            return new Button
            {
                Text = text,
                Location = new Point(10, y),
                Size = new Size(130, 45),
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
        }

        private void PnlPreview_Paint(object sender, PaintEventArgs e)
        {
            _printHelper.DrawInvoice(e.Graphics, _invoice, _config, 300); // 300 for preview width
        }
    }
}
