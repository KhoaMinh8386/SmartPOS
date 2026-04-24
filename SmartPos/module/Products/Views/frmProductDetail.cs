using System;
using System.Drawing;
using System.Windows.Forms;
using SmartPos.Module.Products.Controllers;
using SmartPos.Module.Products.Models;

namespace SmartPos.Module.Products.Views
{
    public class frmProductDetail : Form
    {
        private readonly int _productId;
        private readonly ProductController _controller;
        private ProductDetail _detail;

        public frmProductDetail(int productId)
        {
            _productId = productId;
            _controller = new ProductController();
            
            InitializeUi();
            LoadData();
        }

        private void InitializeUi()
        {
            Text = "CHI TIẾT SẢN PHẨM";
            Size = new Size(850, 600);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.FromArgb(248, 250, 252);
        }

        private void LoadData()
        {
            _detail = _controller.GetProductDetail(_productId);
            if (_detail == null)
            {
                MessageBox.Show("Không tìm thấy thông tin sản phẩm.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }
            RenderContent();
        }

        private void RenderContent()
        {
            var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 70, BackColor = Color.FromArgb(51, 65, 85), Padding = new Padding(20) };
            var lblTitle = new Label 
            { 
                Text = _detail.ProductName.ToUpper(), 
                ForeColor = Color.White, 
                Font = new Font("Segoe UI", 16F, FontStyle.Bold), 
                Dock = DockStyle.Fill, 
                TextAlign = ContentAlignment.MiddleLeft 
            };
            pnlHeader.Controls.Add(lblTitle);

            var mainContent = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(20) };
            mainContent.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 300));
            mainContent.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            // Left: Image
            var pnlImage = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10), BackColor = Color.White };
            pnlImage.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlImage.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);
            
            var pic = new PictureBox 
            { 
                Dock = DockStyle.Fill, 
                SizeMode = PictureBoxSizeMode.Zoom, 
                BackColor = Color.FromArgb(241, 245, 249) 
            };
            if (!string.IsNullOrEmpty(_detail.ImageUrl))
            {
                try { pic.LoadAsync(_detail.ImageUrl); } catch { /* ignore */ }
            }
            pnlImage.Controls.Add(pic);

            // Right: Info Cards
            var pnlInfoScroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(10, 0, 10, 0) };

            AddInfoCard(pnlInfoScroll, "THÔNG TIN CƠ BẢN", new[] {
                "Mã SKU", _detail.ProductCode,
                "Mã vạch", _detail.Barcode ?? "N/A",
                "Mô tả", _detail.Description ?? "(Trống)"
            });

            AddInfoCard(pnlInfoScroll, "GIÁ CẢ & KINH DOANH", new[] {
                "Giá nhập", _detail.CostPrice.ToString("N0") + " VNĐ",
                "Giá bán lẻ", _detail.RetailPrice.ToString("N0") + " VNĐ",
                "Giá sỉ", (_detail.WholesalePrice ?? 0).ToString("N0") + " VNĐ",
                "Trạng thái", _detail.IsActive ? "Đang kinh doanh" : "Ngừng kinh doanh"
            });

            AddInfoCard(pnlInfoScroll, "KHO HÀNG & VỊ TRÍ", new[] {
                "Vị trí kệ", _detail.Location ?? "Chưa xác định",
                "Đơn vị tính", _detail.UnitName ?? "N/A",
                "Hàng có hạn dùng", _detail.HasExpiry ? "Có" : "Không"
            });

            mainContent.Controls.Add(pnlImage, 0, 0);
            mainContent.Controls.Add(pnlInfoScroll, 1, 0);

            var pnlFooter = new Panel { Dock = DockStyle.Bottom, Height = 60, Padding = new Padding(20, 10, 20, 10) };
            var btnClose = new Button 
            { 
                Text = "ĐÓNG", 
                Dock = DockStyle.Right, 
                Width = 120, 
                BackColor = Color.FromArgb(100, 116, 139), 
                ForeColor = Color.White, 
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            btnClose.Click += (s, e) => Close();
            pnlFooter.Controls.Add(btnClose);

            Controls.Add(mainContent);
            Controls.Add(pnlFooter);
            Controls.Add(pnlHeader);
        }

        private void AddInfoCard(Panel parent, string title, string[] fields)
        {
            int fieldCount = fields.Length / 2;
            var card = new Panel { Dock = DockStyle.Top, Height = 45 + (fieldCount * 30), Padding = new Padding(0, 0, 0, 20) };
            
            var lblHeader = new Label { 
                Text = title, 
                Dock = DockStyle.Top, 
                Height = 30, 
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), 
                ForeColor = Color.FromArgb(71, 85, 105),
                TextAlign = ContentAlignment.BottomLeft
            };
            card.Controls.Add(lblHeader);

            int y = 40;
            for (int i = 0; i < fields.Length; i += 2)
            {
                string label = fields[i];
                string val = fields[i + 1];

                var lblL = new Label { 
                    Text = label + ":", 
                    Location = new Point(0, y), 
                    Width = 130, 
                    Font = new Font("Segoe UI", 9F), 
                    ForeColor = Color.FromArgb(100, 116, 139) 
                };
                var lblV = new Label { 
                    Text = val, 
                    Location = new Point(135, y), 
                    Width = 350, 
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold), 
                    ForeColor = Color.FromArgb(30, 41, 59),
                    AutoSize = true,
                    MaximumSize = new Size(350, 0)
                };
                card.Controls.Add(lblL);
                card.Controls.Add(lblV);
                y += Math.Max(25, lblV.PreferredHeight + 5);
            }
            
            // Adjust card height based on actual content
            card.Height = y + 10;
            parent.Controls.Add(card);
            card.BringToFront(); // Reverse order because of DockStyle.Top
        }
    }
}
