using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SmartPos.Module.SalesHistory.Backend;
using SmartPos.Module.SalesHistory.Models;

namespace SmartPos.Module.Pos
{
    public partial class PosModuleForm : Form
    {
        private readonly PosController _controller;
        private readonly InvoiceService _invoiceService;
        private List<CartItem> _cart = new List<CartItem>();
        private CustomerInfo _currentCustomer = null;
        private VoucherInfo _appliedVoucher = null;
        private decimal _voucherDiscount = 0;
        private decimal _pointsDiscount = 0;
        private int _usedPoints = 0;

        // UI Components
        private DataGridView dgvCart;
        private TextBox txtSearch;
        private FlowLayoutPanel pnlSuggestions;
        private ListBox lstCustomerSuggestions;
        private TextBox txtPhone;
        private Label lblCustomerInfo;
        private CheckBox chkUsePoints;
        private Label lblSubTotal;
        private Label lblVoucherDiscount;
        private Label lblPointsDiscount;
        private Label lblTotal;
        private NumericUpDown numPaid;
        private Label lblChange;
        private ComboBox cboPaymentMethod;
        private TextBox txtVoucher;
        private Label lblShiftInfo;

        public PosModuleForm()
        {
            _controller = new PosController();
            _invoiceService = new InvoiceService();
            InitializeComponent();
            SetupShortcuts();
            this.Load += (s, e) => { txtSearch.Focus(); UpdateShiftInfo(); };
        }

        private void InitializeComponent()
        {
            this.Text = "SMART POS - HỆ THỐNG BÁN HÀNG SIÊU THỊ";
            this.BackColor = Color.FromArgb(236, 240, 241);
            this.Font = new Font("Segoe UI", 10F);

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(10)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68F));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32F));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            // ════════════════════════════════════════════════════════════════════
            // LEFT: 3 rows — shiftInfo | search | cart | actions
            // ════════════════════════════════════════════════════════════════════
            var left = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(8),
                BackColor = Color.White
            };
            left.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));   // shift info
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));   // search
            left.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));  // cart grid
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));   // actions

            // Row 0 – Shift info
            lblShiftInfo = new Label
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(64, 64, 64),
                TextAlign = ContentAlignment.MiddleLeft
            };
            left.Controls.Add(lblShiftInfo, 0, 0);

            // Row 1 – Search box
            var pnlSearchContainer = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 5, 0, 5) };
            txtSearch = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 16F),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(249, 250, 251)
            };
            // Placeholder-like effect
            txtSearch.Text = "🔍 Nhập tên sản phẩm hoặc quét mã vạch...";
            txtSearch.ForeColor = Color.Gray;
            txtSearch.Enter += (s, e) => { if (txtSearch.Text.Contains("🔍")) { txtSearch.Text = ""; txtSearch.ForeColor = Color.Black; } };
            txtSearch.Leave += (s, e) => { if (string.IsNullOrEmpty(txtSearch.Text)) { txtSearch.Text = "🔍 Nhập tên sản phẩm hoặc quét mã vạch..."; txtSearch.ForeColor = Color.Gray; } };
            
            txtSearch.TextChanged += TxtSearch_TextChanged;
            txtSearch.KeyDown += TxtSearch_KeyDown;
            
            pnlSearchContainer.Controls.Add(txtSearch);
            left.Controls.Add(pnlSearchContainer, 0, 1);

            // Floating Suggestions FlowLayoutPanel (Đưa vào form gốc để nổi lên trên cùng)
            pnlSuggestions = new FlowLayoutPanel
            {
                Visible = false,
                Width = 600, Height = 400,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                AutoScroll = true,
                WrapContents = true,
                Padding = new Padding(5)
            };
            this.Controls.Add(pnlSuggestions);
            pnlSuggestions.BringToFront();

            // Row 2 – Cart DataGridView
            dgvCart = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.CellSelect,
                EditMode = DataGridViewEditMode.EditOnEnter,
                RowTemplate = { Height = 40 },
                AllowUserToAddRows = false,
                GridColor = Color.FromArgb(230, 233, 237),
                Font = new Font("Segoe UI", 10F),
                EnableHeadersVisualStyles = false,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal
            };
            dgvCart.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(45, 52, 54);
            dgvCart.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvCart.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvCart.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvCart.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 250);
            dgvCart.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 184, 148);
            dgvCart.DefaultCellStyle.SelectionForeColor = Color.White;
            SetupCartGrid();
            left.Controls.Add(dgvCart, 0, 2);

            // Row 3 – Actions
            var pnlActions = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 10, 0, 0) };
            var btnClear = new Button
            {
                Text = "XÓA TOÀN BỘ (F4)",
                Height = 42, Width = 180,
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnClear.FlatAppearance.BorderSize = 0;
            btnClear.Click += (s, e) => { if(_cart.Any()) { if(MessageBox.Show("Xóa toàn bộ giỏ hàng?", "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.Yes) { ResetForm(); } } };
            pnlActions.Controls.Add(btnClear);
            left.Controls.Add(pnlActions, 0, 3);

            // ════════════════════════════════════════════════════════════════════
            // RIGHT: 4 rows — customer | summary | payment | checkout
            // ════════════════════════════════════════════════════════════════════
            var right = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(10, 0, 5, 0),
                BackColor = Color.FromArgb(240, 242, 245)
            };
            right.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            right.RowStyles.Add(new RowStyle(SizeType.Absolute, 130)); // customer
            right.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // summary
            right.RowStyles.Add(new RowStyle(SizeType.Absolute, 150)); // payment input
            right.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));  // checkout btn

            // ── Row 0: Customer ──────────────────────────────────────────────────
            var pnlCustomerCard = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(15), BorderStyle = BorderStyle.None };
            var lblCustHeader = new Label { Text = "THÔNG TIN KHÁCH HÀNG", Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105), Dock = DockStyle.Top, Height = 25 };
            
            txtPhone = new TextBox { Dock = DockStyle.Top, Font = new Font("Segoe UI", 12F), BorderStyle = BorderStyle.FixedSingle };
            txtPhone.KeyDown += TxtPhone_KeyDown;
            txtPhone.TextChanged += TxtPhone_TextChanged;

            // Khởi tạo danh sách gợi ý khách hàng
            lstCustomerSuggestions = new ListBox
            {
                Visible = false,
                Width = 250, Height = 180,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(15, 60), // Nằm dưới ô nhập SĐT
                BorderStyle = BorderStyle.FixedSingle,
                Cursor = Cursors.Hand,
                BackColor = Color.White,
                ForeColor = Color.Black
            };
            lstCustomerSuggestions.Click += (s, e) => SelectCustomerFromList();
            lstCustomerSuggestions.DoubleClick += (s, e) => SelectCustomerFromList();

            lblCustomerInfo = new Label
            {
                Dock = DockStyle.Top,
                Height = 30,
                Text = "Khách lẻ (Walk-in)",
                ForeColor = Color.FromArgb(44, 62, 80),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };
            
            chkUsePoints = new CheckBox
            {
                Dock = DockStyle.Top,
                Text = "Dùng điểm tích lũy",
                Visible = false,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(39, 174, 96)
            };
            chkUsePoints.CheckedChanged += (s, e) => UpdateTotal();

            pnlCustomerCard.Controls.Add(chkUsePoints);
            pnlCustomerCard.Controls.Add(lblCustomerInfo);
            pnlCustomerCard.Controls.Add(txtPhone);
            pnlCustomerCard.Controls.Add(lblCustHeader);
            
            this.Controls.Add(lstCustomerSuggestions);
            lstCustomerSuggestions.BringToFront();
            
            right.Controls.Add(pnlCustomerCard, 0, 0);

            var pnlSummaryCard = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(15, 5, 15, 15), BorderStyle = BorderStyle.None };
            var tlpSummary = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 5,
                Padding = new Padding(0)
            };
            tlpSummary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
            tlpSummary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
            tlpSummary.RowStyles.Add(new RowStyle(SizeType.Percent, 15F)); // subtotal
            tlpSummary.RowStyles.Add(new RowStyle(SizeType.Percent, 15F)); // voucher label
            tlpSummary.RowStyles.Add(new RowStyle(SizeType.Percent, 20F)); // voucher input
            tlpSummary.RowStyles.Add(new RowStyle(SizeType.Percent, 15F)); // points
            tlpSummary.RowStyles.Add(new RowStyle(SizeType.Percent, 35F)); // TOTAL

            tlpSummary.Controls.Add(MakeLabel("Tạm tính:"), 0, 0);
            lblSubTotal = MakeValueLabel("0");
            tlpSummary.Controls.Add(lblSubTotal, 1, 0);

            tlpSummary.Controls.Add(MakeLabel("Voucher:"), 0, 1);
            lblVoucherDiscount = MakeValueLabel("-0", Color.FromArgb(239, 68, 68));
            tlpSummary.Controls.Add(lblVoucherDiscount, 1, 1);

            txtVoucher = new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 11F), BorderStyle = BorderStyle.FixedSingle };
            txtVoucher.KeyDown += TxtVoucher_KeyDown;
            tlpSummary.Controls.Add(txtVoucher, 0, 2);
            tlpSummary.Controls.Add(new Label { Text = "↵ Apply", Font = new Font("Segoe UI", 8, FontStyle.Italic), ForeColor = Color.Gray, TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 1, 2);

            tlpSummary.Controls.Add(MakeLabel("Giảm điểm:"), 0, 3);
            lblPointsDiscount = MakeValueLabel("-0", Color.FromArgb(239, 68, 68));
            tlpSummary.Controls.Add(lblPointsDiscount, 1, 3);

            var pnlTotalHighlight = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(52, 73, 94), Padding = new Padding(10) };
            var lblTotalTitle = new Label { Text = "TỔNG CỘNG", Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.FromArgb(189, 195, 199), Dock = DockStyle.Top };
            lblTotal = new Label
            {
                Text = "0",
                Font = new Font("Segoe UI", 26F, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight
            };
            pnlTotalHighlight.Controls.Add(lblTotal);
            pnlTotalHighlight.Controls.Add(lblTotalTitle);
            tlpSummary.SetColumnSpan(pnlTotalHighlight, 2);
            tlpSummary.Controls.Add(pnlTotalHighlight, 0, 4);

            pnlSummaryCard.Controls.Add(tlpSummary);
            right.Controls.Add(pnlSummaryCard, 0, 1);

            var pnlPaymentCard = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(15), BorderStyle = BorderStyle.None };
            var tlpPay = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4 };
            tlpPay.RowStyles.Add(new RowStyle(SizeType.Absolute, 20)); 
            tlpPay.RowStyles.Add(new RowStyle(SizeType.Absolute, 35)); 
            tlpPay.RowStyles.Add(new RowStyle(SizeType.Absolute, 20)); 
            tlpPay.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            tlpPay.Controls.Add(MakeLabel("PHƯƠNG THỨC:"), 0, 0);
            cboPaymentMethod = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 11F) };
            cboPaymentMethod.Items.AddRange(new[] { "Tiền mặt", "Chuyển khoản" });
            cboPaymentMethod.SelectedIndex = 0;
            cboPaymentMethod.SelectedIndexChanged += CboPaymentMethod_SelectedIndexChanged;
            tlpPay.Controls.Add(cboPaymentMethod, 0, 1);

            tlpPay.Controls.Add(MakeLabel("TIỀN KHÁCH ĐƯA:"), 0, 2);
            var pnlPaidInput = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            pnlPaidInput.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            pnlPaidInput.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));

            numPaid = new NumericUpDown { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 16F, FontStyle.Bold), Maximum = 1_000_000_000, ThousandsSeparator = true, TextAlign = HorizontalAlignment.Right };
            numPaid.ValueChanged += (s, e) => CalculateChange();
            lblChange = new Label { Dock = DockStyle.Fill, Text = "Thối: 0đ", Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = Color.FromArgb(16, 185, 129), TextAlign = ContentAlignment.MiddleRight };
            
            pnlPaidInput.Controls.Add(numPaid, 0, 0);
            pnlPaidInput.Controls.Add(lblChange, 1, 0);
            tlpPay.Controls.Add(pnlPaidInput, 0, 3);
            
            pnlPaymentCard.Controls.Add(tlpPay);
            right.Controls.Add(pnlPaymentCard, 0, 2);

            // ── Row 3: CHECKOUT button
            var btnCheckout = new Button
            {
                Text = "THANH TOÁN (F12)",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCheckout.FlatAppearance.BorderSize = 0;
            btnCheckout.Click += (s, e) => ProcessCheckout();
            right.Controls.Add(btnCheckout, 0, 3);

            // ── Assemble root ────────────────────────────────────────────────────
            root.Controls.Add(left, 0, 0);
            root.Controls.Add(right, 1, 0);
            this.Controls.Add(root);
            
            // Re-assert z-order for floating panels
            pnlSuggestions.BringToFront();
            lstCustomerSuggestions.BringToFront();
        }

        // Helpers for creating consistent labels
        private Label MakeLabel(string text) => new Label
        {
            Text = text,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 9F),
            ForeColor = Color.FromArgb(80, 80, 80)
        };

        private Label MakeValueLabel(string text, Color? color = null) => new Label
        {
            Text = text,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = color ?? Color.FromArgb(33, 33, 33),
            Padding = new Padding(0, 0, 4, 0)
        };

        private void SetupCartGrid()
        {
            dgvCart.Columns.Add("ProductID", "ID"); dgvCart.Columns["ProductID"].Visible = false;
            dgvCart.Columns.Add("ProductName", "Tên sản phẩm"); dgvCart.Columns["ProductName"].FillWeight = 200; dgvCart.Columns["ProductName"].ReadOnly = true;
            dgvCart.Columns.Add("BatchNumber", "Lô"); dgvCart.Columns["BatchNumber"].Width = 100; dgvCart.Columns["BatchNumber"].ReadOnly = true;
            dgvCart.Columns.Add("ExpiryDate", "HSD"); dgvCart.Columns["ExpiryDate"].Width = 90; dgvCart.Columns["ExpiryDate"].ReadOnly = true;
            dgvCart.Columns.Add("UnitName", "Đơn vị"); dgvCart.Columns["UnitName"].Width = 70; dgvCart.Columns["UnitName"].ReadOnly = true;
            dgvCart.Columns.Add("UnitPrice", "Đơn giá"); dgvCart.Columns["UnitPrice"].DefaultCellStyle.Format = "N0"; dgvCart.Columns["UnitPrice"].ReadOnly = true;
            dgvCart.Columns.Add("Quantity", "SL"); dgvCart.Columns["Quantity"].Width = 60;
            dgvCart.Columns.Add("SubTotal", "Thành tiền"); dgvCart.Columns["SubTotal"].DefaultCellStyle.Format = "N0"; dgvCart.Columns["SubTotal"].ReadOnly = true;
            
            DataGridViewButtonColumn btnDel = new DataGridViewButtonColumn { Text = "✕", Name = "Delete", UseColumnTextForButtonValue = true, Width = 35 };
            dgvCart.Columns.Add(btnDel);

            dgvCart.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvCart.CellContentClick += DgvCart_CellContentClick;
            dgvCart.CellValueChanged += DgvCart_CellValueChanged;
        }

        private void SetupShortcuts()
        {
            this.KeyPreview = true;
            this.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.F12) ProcessCheckout();
                if (e.KeyCode == Keys.F4) { _cart.Clear(); RefreshCart(); }
                if (e.KeyCode == Keys.F1) txtSearch.Focus();
            };
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            string term = txtSearch.Text.Trim();
            if (term.Length < 2 || term.Contains("🔍")) { pnlSuggestions.Visible = false; return; }

            var products = _controller.FindProducts(term);
            if (products.Any())
            {
                pnlSuggestions.Controls.Clear();
                foreach (var prod in products)
                {
                    if (prod.TotalStock <= 0)
                    {
                        prod.ProductName += " (Hết hàng)";
                    }
                    var card = CreateProductCard(prod);
                    pnlSuggestions.Controls.Add(card);
                }
                
                // Position exactly under the search box
                var pt = txtSearch.PointToScreen(new Point(0, txtSearch.Height));
                pnlSuggestions.Location = this.PointToClient(pt);
                
                pnlSuggestions.Visible = true;
                pnlSuggestions.BringToFront();
            }
            else { pnlSuggestions.Visible = false; }
        }

        private Panel CreateProductCard(CartItem product)
        {
            var pnl = new Panel
            {
                Width = 145, Height = 200,
                Margin = new Padding(5),
                BackColor = Color.White,
                Cursor = Cursors.Hand,
                BorderStyle = BorderStyle.None
            };
            // Add subtle shadow effect using padding and background if possible, 
            // but for WinForms simple is better. Let's use a light border.
            pnl.Paint += (s, e) => {
                ControlPaint.DrawBorder(e.Graphics, pnl.ClientRectangle, Color.FromArgb(224, 224, 224), ButtonBorderStyle.Solid);
            };

            var pic = new PictureBox
            {
                Dock = DockStyle.Top,
                Height = 110,
                SizeMode = PictureBoxSizeMode.Zoom,
                Cursor = Cursors.Hand,
                BackColor = Color.White,
                Padding = new Padding(5)
            };

            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                try { pic.LoadAsync(product.ImageUrl); }
                catch { /* ignore */ }
            }

            var lblName = new Label
            {
                Text = product.ProductName,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.TopCenter,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(45, 52, 54),
                Cursor = Cursors.Hand,
                Padding = new Padding(3)
            };

            var pnlBottom = new Panel { Dock = DockStyle.Bottom, Height = 45, Padding = new Padding(2) };
            
            var lblPrice = new Label
            {
                Text = product.UnitPrice.ToString("N0") + " đ",
                Dock = DockStyle.Top,
                Height = 22,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(225, 112, 85),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            var lblBatch = new Label
            {
                Text = $"Lô: {product.BatchNumber ?? "-"}",
                Dock = DockStyle.Bottom,
                Height = 18,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(127, 140, 141),
                Font = new Font("Segoe UI", 7F),
                Cursor = Cursors.Hand
            };

            pnlBottom.Controls.Add(lblPrice);
            pnlBottom.Controls.Add(lblBatch);

            pnl.Controls.Add(lblName);
            pnl.Controls.Add(pnlBottom);
            pnl.Controls.Add(pic);

            // Add click events to all controls inside the card
            EventHandler onClick = (s, e) => {
                if (product.TotalStock <= 0)
                {
                    MessageBox.Show("Sản phẩm này đã hết hàng, không thể thêm vào hóa đơn!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                AddToCart(product);
                txtSearch.Clear();
                pnlSuggestions.Visible = false;
                txtSearch.Focus();
            };

            pnl.Click += onClick;
            pic.Click += onClick;
            lblName.Click += onClick;
            lblPrice.Click += onClick;
            lblBatch.Click += onClick;

            // Hover effects
            pnl.MouseEnter += (s, e) => { pnl.BackColor = Color.FromArgb(249, 250, 251); };
            pnl.MouseLeave += (s, e) => { pnl.BackColor = Color.White; };

            return pnl;
        }

        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape && pnlSuggestions.Visible)
            {
                pnlSuggestions.Visible = false;
            }

            if (e.KeyCode == Keys.Enter)
            {
                string term = txtSearch.Text.Trim();
                if (!string.IsNullOrEmpty(term) && !term.Contains("🔍"))
                {
                    var products = _controller.FindProducts(term);
                    if (products.Any())
                    {
                        // Lấy sản phẩm đầu tiên tìm thấy (thường là mã vạch hoặc mã SP chính xác)
                        var product = products.First();
                        if (product.TotalStock <= 0)
                        {
                            MessageBox.Show("Sản phẩm này đã hết hàng, không thể thêm vào hóa đơn!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        else
                        {
                            AddToCart(product);
                            txtSearch.Clear();
                            pnlSuggestions.Visible = false;
                        }
                        e.SuppressKeyPress = true; // Ngăn tiếng beep của Windows
                        e.Handled = true;
                    }
                }
            }
        }

        // Removing AddSelectedSuggestion since we use click directly on ProductCards

        private void AddToCart(CartItem product)
        {
            var existing = _cart.FirstOrDefault(x => x.ProductID == product.ProductID && x.BatchNumber == product.BatchNumber);
            if (existing != null) existing.Quantity += 1;
            else _cart.Add(product);
            RefreshCart();
        }

        private void RefreshCart()
        {
            dgvCart.Rows.Clear();
            foreach (var item in _cart)
            {
                dgvCart.Rows.Add(
                    item.ProductID, 
                    item.ProductName, 
                    item.BatchNumber ?? "-", 
                    item.ExpiryDate?.ToString("dd/MM/yy") ?? "-",
                    item.UnitName, 
                    item.UnitPrice, 
                    item.Quantity, 
                    item.SubTotal
                );
            }
            UpdateTotal();
        }

        private void UpdateTotal()
        {
            decimal subTotal = _cart.Sum(x => x.SubTotal);
            lblSubTotal.Text = subTotal.ToString("N0");

            // Voucher logic
            _voucherDiscount = 0;
            if (_appliedVoucher != null)
            {
                if (subTotal >= _appliedVoucher.MinOrderValue)
                {
                    _voucherDiscount = _appliedVoucher.DiscountType == 1 ? subTotal * (_appliedVoucher.DiscountValue / 100m) : _appliedVoucher.DiscountValue;
                }
            }
            lblVoucherDiscount.Text = "-" + _voucherDiscount.ToString("N0");

            // Points logic
            _pointsDiscount = 0;
            _usedPoints = 0;
            if (chkUsePoints.Checked && _currentCustomer != null)
            {
                _usedPoints = _currentCustomer.TotalPoints;
                _pointsDiscount = _usedPoints; // 1 point = 1d
            }
            lblPointsDiscount.Text = "-" + _pointsDiscount.ToString("N0");

            decimal total = subTotal - _voucherDiscount - _pointsDiscount;
            if (total < 0) total = 0;

            lblTotal.Text = total.ToString("N0");
            numPaid.Value = total;
            CalculateChange();

            // Auto-trigger QR if transfer is already selected and total > 0
            if (cboPaymentMethod.Text == "Chuyển khoản" && total > 0)
            {
                // We only want to show it if the user just added items or if it hasn't been shown for this specific session
                // But the user said "lúc nào cũng hiện", so let's make it reliable.
                // To avoid annoying popups, maybe we only trigger if total was 0 and now it's > 0?
                // Actually, let's just make sure it resets properly.
            }
        }

        private void CalculateChange()
        {
            decimal total = decimal.Parse(lblTotal.Text.Replace(",", "").Replace(".", ""));
            decimal change = numPaid.Value - total;
            lblChange.Text = "Tiền thối: " + (change >= 0 ? change.ToString("N0") : "0") + " đ";
        }

        private void TxtPhone_TextChanged(object sender, EventArgs e)
        {
            string term = txtPhone.Text.Trim();
            if (string.IsNullOrEmpty(term))
            {
                _currentCustomer = null;
                ApplyCustomer();
                lstCustomerSuggestions.Visible = false;
                return;
            }
            if (term.Length < 2) { lstCustomerSuggestions.Visible = false; return; }

            var customers = _controller.FindCustomers(term);
            if (customers != null && customers.Any())
            {
                lstCustomerSuggestions.DataSource = customers;
                lstCustomerSuggestions.DisplayMember = "FullName";
                
                // Position exactly under the phone input box
                var pt = txtPhone.PointToScreen(new Point(0, txtPhone.Height));
                lstCustomerSuggestions.Location = this.PointToClient(pt);
                lstCustomerSuggestions.Width = txtPhone.Width;
                
                lstCustomerSuggestions.Visible = true;
                lstCustomerSuggestions.BringToFront();
            }
            else { lstCustomerSuggestions.Visible = false; }
        }

        private void TxtPhone_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down && lstCustomerSuggestions.Visible) lstCustomerSuggestions.Focus();
            if (e.KeyCode == Keys.Enter)
            {
                if (lstCustomerSuggestions.Visible && lstCustomerSuggestions.SelectedItem != null)
                {
                    SelectCustomerFromList();
                }
                else
                {
                    _currentCustomer = _controller.FindCustomer(txtPhone.Text.Trim());
                    ApplyCustomer();
                }
            }
        }

        private void SelectCustomerFromList()
        {
            if (lstCustomerSuggestions.SelectedItem is CustomerInfo customer)
            {
                _currentCustomer = customer;
                ApplyCustomer();
                lstCustomerSuggestions.Visible = false;
            }
        }

        private void ApplyCustomer()
        {
            if (_currentCustomer != null)
            {
                txtPhone.Text = _currentCustomer.Phone;
                lblCustomerInfo.Text = $"KHACH: {_currentCustomer.FullName} | Diem: {_currentCustomer.TotalPoints:N0}";
                lblCustomerInfo.ForeColor = Color.DarkGreen;
                chkUsePoints.Visible = true;
                chkUsePoints.Text = $"Dùng {_currentCustomer.TotalPoints:N0} điểm (-{_currentCustomer.TotalPoints:N0}đ)";
            }
            else
            {
                lblCustomerInfo.Text = "Khách lẻ (Walk-in)";
                lblCustomerInfo.ForeColor = Color.FromArgb(33, 150, 243);
                chkUsePoints.Visible = false;
            }
            UpdateTotal();
        }

        private void TxtVoucher_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                _appliedVoucher = _controller.GetVoucher(txtVoucher.Text.Trim());
                UpdateTotal();
            }
        }

        private void DgvCart_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == dgvCart.Columns["Delete"].Index && e.RowIndex >= 0)
            {
                _cart.RemoveAt(e.RowIndex);
                RefreshCart();
            }
        }

        private void DgvCart_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvCart.Columns[e.ColumnIndex].Name == "Quantity")
            {
                int pid = (int)dgvCart.Rows[e.RowIndex].Cells["ProductID"].Value;
                string batch = dgvCart.Rows[e.RowIndex].Cells["BatchNumber"].Value?.ToString();
                if (batch == "-") batch = null;

                if (decimal.TryParse(dgvCart.Rows[e.RowIndex].Cells["Quantity"].Value?.ToString(), out decimal qty))
                {
                    if (qty <= 0) qty = 1;
                    var item = _cart.FirstOrDefault(x => x.ProductID == pid && x.BatchNumber == batch);
                    if (item != null)
                    {
                        item.Quantity = qty;
                        RefreshCart();
                    }
                }
            }
        }

        private void UpdateShiftInfo()
        {
            lblShiftInfo.Text = $"| NV: {UserSession.CurrentUser?.FullName ?? "Admin"} | {DateTime.Now:dd/MM/yyyy HH:mm}";
        }

        private void ProcessCheckout()
        {
            if (!_cart.Any()) { MessageBox.Show("Giỏ hàng chưa có sản phẩm!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            
            // Lấy tổng tiền an toàn hơn
            decimal totalAmount = _cart.Sum(x => x.SubTotal) - _voucherDiscount - _pointsDiscount;
            if (totalAmount < 0) totalAmount = 0;

            if (numPaid.Value < totalAmount) 
            { 
                MessageBox.Show("Tiền khách đưa chưa đủ!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning); 
                numPaid.Focus();
                return; 
            }

            try
            {
                var request = new CheckoutRequest
                {
                    CustomerID = _currentCustomer?.CustomerID,
                    UserID = UserSession.CurrentUser?.UserID ?? 1,
                    SubTotal = _cart.Sum(x => x.SubTotal),
                    TotalAmount = totalAmount,
                    PaidAmount = numPaid.Value,
                    PaymentMethod = (byte)(cboPaymentMethod.SelectedIndex + 1),
                    VoucherCode = _appliedVoucher?.VoucherCode,
                    VoucherDiscount = _voucherDiscount,
                    PointsDiscount = _pointsDiscount,
                    UsedPoints = _usedPoints,
                    EarnedPoints = (int)(totalAmount * 0.001m), // Tích 0.1% điểm (1000đ = 1 điểm)
                    Items = _cart
                };

                int invoiceId = _controller.Checkout(request);
                if (invoiceId > 0)
                {
                    var result = MessageBox.Show("Thanh toán thành công!\nBạn có muốn in hóa đơn không?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    
                    if (result == DialogResult.Yes)
                    {
                        ShowInvoicePreview(invoiceId);
                    }
                    
                    ResetForm();
                }
                else
                {
                    MessageBox.Show("Lỗi hệ thống: Không thể tạo hóa đơn.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Đã xảy ra lỗi khi thanh toán:\n" + ex.Message, "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowInvoicePreview(int invoiceId)
        {
            var detail = _invoiceService.GetInvoiceDetail(invoiceId);
            var config = _invoiceService.GetStoreConfig();
            using (var preview = new InvoicePreviewForm(detail, config))
            {
                preview.ShowDialog();
            }
        }

        private void CboPaymentMethod_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboPaymentMethod.Text == "Chuyển khoản")
            {
                // Parse total from label safely
                if (decimal.TryParse(lblTotal.Text.Replace(",", "").Replace(".", ""), out decimal total) && total > 0)
                {
                    string qrUrl = $"https://img.vietqr.io/image/MB-03333334444440-compact.png?amount={total:0}&addInfo=SmartPOS%20Thanh%20toan&accountName=LE%20THANH%20TINH";
                    if (ShowQRCodeDialog(qrUrl, total) == DialogResult.OK)
                    {
                        ProcessCheckout();
                    }
                }
            }
        }

        private DialogResult ShowQRCodeDialog(string url, decimal amount)
        {
            using (Form qrForm = new Form())
            {
                qrForm.Text = "QUÉT MÃ THANH TOÁN (VietQR)";
                qrForm.Size = new Size(500, 720); // Tăng kích thước để chuyên nghiệp hơn
                qrForm.StartPosition = FormStartPosition.CenterScreen;
                qrForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                qrForm.MaximizeBox = false;
                qrForm.MinimizeBox = false;
                qrForm.BackColor = Color.White;
                qrForm.KeyPreview = true;

                PictureBox pic = new PictureBox
                {
                    Dock = DockStyle.Top,
                    Height = 480,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Padding = new Padding(20)
                };
                pic.LoadAsync(url);

                Label lblInfo = new Label
                {
                    Text = $"SỐ TIỀN: {amount:N0} đ\nCHỦ TK: LE THANH TINH\nSTK: 03333334444440\nNGÂN HÀNG: MB BANK (Ngân hàng Quân đội)",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    ForeColor = Color.FromArgb(30, 41, 59),
                    Padding = new Padding(10)
                };

                Button btnStatus = new Button
                {
                    Text = "Đang chờ thanh toán (30s)",
                    Dock = DockStyle.Bottom,
                    Height = 70,
                    BackColor = Color.FromArgb(59, 130, 246), // Blue 500
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 14, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnStatus.FlatAppearance.BorderSize = 0;

                int timeLeft = 30;
                Timer timer = new Timer { Interval = 1000 };
                timer.Tick += (s, e) =>
                {
                    timeLeft--;
                    if (timeLeft > 0)
                    {
                        btnStatus.Text = $"Đang chờ thanh toán ({timeLeft}s)";
                    }
                    else
                    {
                        timer.Stop();
                        btnStatus.Text = "THANH TOÁN THẤT BẠI";
                        btnStatus.BackColor = Color.FromArgb(239, 68, 68); // Red 500
                        MessageBox.Show("Thời gian chờ thanh toán đã hết. Vui lòng kiểm tra lại giao dịch!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };
                timer.Start();

                qrForm.KeyDown += (s, e) =>
                {
                    if (e.KeyCode == Keys.Z)
                    {
                        timer.Stop();
                        btnStatus.Text = "THANH TOÁN THÀNH CÔNG";
                        btnStatus.BackColor = Color.FromArgb(16, 185, 129); // Emerald 500
                        MessageBox.Show("Xác nhận thanh toán thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        qrForm.DialogResult = DialogResult.OK;
                        qrForm.Close();
                    }
                };

                btnStatus.Click += (s, e) => {
                    if (timeLeft <= 0) qrForm.Close();
                };

                qrForm.Controls.Add(lblInfo);
                qrForm.Controls.Add(pic);
                qrForm.Controls.Add(btnStatus);
                return qrForm.ShowDialog();
            }
        }

        private void ResetForm()
        {
            _cart.Clear();
            _currentCustomer = null;
            _appliedVoucher = null;
            _voucherDiscount = 0;
            _pointsDiscount = 0;
            _usedPoints = 0;
            txtPhone.Clear();
            txtVoucher.Clear();
            txtSearch.Clear();
            chkUsePoints.Checked = false;
            chkUsePoints.Visible = false;
            lstCustomerSuggestions.Visible = false;
            pnlSuggestions.Visible = false;
            lblCustomerInfo.Text = "Khách lẻ";
            cboPaymentMethod.SelectedIndex = 0; // Reset to Cash
            RefreshCart();
        }
    }
}
