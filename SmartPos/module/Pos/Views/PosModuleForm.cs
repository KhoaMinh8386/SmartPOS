using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
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
        private ListBox lstSuggestions;
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
            this.Text = "SMART POS - BÁN HÀNG";
            this.BackColor = Color.FromArgb(245, 245, 245);
            this.Font = new Font("Segoe UI", 10F);

            // ─── ROOT: 2 columns 65/35 ───────────────────────────────────────────
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(0)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
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
                BackColor = Color.FromArgb(245, 245, 245)
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

            // Row 1 – Search box (txtSearch Dock=Fill + floating lstSuggestions)
            var pnlSearch = new Panel { Dock = DockStyle.Fill };
            txtSearch = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 13F)
            };
            txtSearch.TextChanged += TxtSearch_TextChanged;
            txtSearch.KeyDown += TxtSearch_KeyDown;
            lstSuggestions = new ListBox
            {
                Visible = false,
                Width = 400, Height = 180,
                Font = new Font("Segoe UI", 11F),
                Location = new Point(0, 40)
            };
            lstSuggestions.DoubleClick += (s, e) => AddSelectedSuggestion();
            pnlSearch.Controls.Add(lstSuggestions);
            pnlSearch.Controls.Add(txtSearch);
            lstSuggestions.BringToFront();
            left.Controls.Add(pnlSearch, 0, 1);

            // Row 2 – Cart DataGridView
            dgvCart = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowTemplate = { Height = 36 },
                AllowUserToAddRows = false,
                GridColor = Color.FromArgb(220, 220, 220),
                Font = new Font("Segoe UI", 10F)
            };
            SetupCartGrid();
            left.Controls.Add(dgvCart, 0, 2);

            // Row 3 – Actions
            var pnlActions = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 6, 0, 6) };
            var btnClear = new Button
            {
                Text = "🗑Xóa tất cả",
                Height = 38, Width = 160,
                BackColor = Color.FromArgb(198, 40, 40),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            btnClear.FlatAppearance.BorderSize = 0;
            btnClear.Click += (s, e) => { _cart.Clear(); RefreshCart(); };
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
                Padding = new Padding(10, 8, 8, 8),
                BackColor = Color.White
            };
            right.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            right.RowStyles.Add(new RowStyle(SizeType.Absolute, 110)); // customer
            right.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // summary
            right.RowStyles.Add(new RowStyle(SizeType.Absolute, 130)); // payment input
            right.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));  // checkout btn

            // ── Row 0: Customer ──────────────────────────────────────────────────
            var grpCustomer = new GroupBox
            {
                Text = "KHÁCH HÀNG",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Padding = new Padding(6, 16, 6, 4)
            };
            var tlpCust = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(0)
            };
            tlpCust.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpCust.RowStyles.Add(new RowStyle(SizeType.Absolute, 28)); // txtPhone
            tlpCust.RowStyles.Add(new RowStyle(SizeType.Absolute, 22)); // lblCustomerInfo
            tlpCust.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // chkUsePoints

            txtPhone = new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 11F) };
            txtPhone.KeyDown += TxtPhone_KeyDown;
            lblCustomerInfo = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Khách lẻ",
                ForeColor = Color.FromArgb(33, 150, 243),
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                TextAlign = ContentAlignment.MiddleLeft
            };
            chkUsePoints = new CheckBox
            {
                Dock = DockStyle.Fill,
                Text = "Dùng điểm tích lũy",
                Visible = false,
                Font = new Font("Segoe UI", 9F)
            };
            chkUsePoints.CheckedChanged += (s, e) => UpdateTotal();

            tlpCust.Controls.Add(txtPhone, 0, 0);
            tlpCust.Controls.Add(lblCustomerInfo, 0, 1);
            tlpCust.Controls.Add(chkUsePoints, 0, 2);
            grpCustomer.Controls.Add(tlpCust);
            right.Controls.Add(grpCustomer, 0, 0);

            // ── Row 1: Summary (subtotal, voucher, points, total) ────────────────
            var tlpSummary = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 5,
                Padding = new Padding(4, 6, 4, 0)
            };
            tlpSummary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
            tlpSummary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
            tlpSummary.RowStyles.Add(new RowStyle(SizeType.Percent, 16F)); // subtotal
            tlpSummary.RowStyles.Add(new RowStyle(SizeType.Percent, 16F)); // voucher label
            tlpSummary.RowStyles.Add(new RowStyle(SizeType.Percent, 20F)); // voucher input
            tlpSummary.RowStyles.Add(new RowStyle(SizeType.Percent, 16F)); // points
            tlpSummary.RowStyles.Add(new RowStyle(SizeType.Percent, 32F)); // TOTAL

            // Row 0: Subtotal
            tlpSummary.Controls.Add(MakeLabel("Tạm tính:"), 0, 0);
            lblSubTotal = MakeValueLabel("0");
            tlpSummary.Controls.Add(lblSubTotal, 1, 0);

            // Row 1: Voucher label
            tlpSummary.Controls.Add(MakeLabel("Mã Voucher:"), 0, 1);
            lblVoucherDiscount = MakeValueLabel("-0", Color.FromArgb(198, 40, 40));
            tlpSummary.Controls.Add(lblVoucherDiscount, 1, 1);

            // Row 2: Voucher input
            txtVoucher = new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 11F) };
            txtVoucher.KeyDown += TxtVoucher_KeyDown;
            var lblVoucherHint = new Label
            {
                Dock = DockStyle.Fill,
                Text = "↵ Enter để áp dụng",
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 8F, FontStyle.Italic),
                TextAlign = ContentAlignment.MiddleLeft
            };
            tlpSummary.Controls.Add(txtVoucher, 0, 2);
            tlpSummary.Controls.Add(lblVoucherHint, 1, 2);

            // Row 3: Points
            tlpSummary.Controls.Add(MakeLabel("Giảm điểm:"), 0, 3);
            lblPointsDiscount = MakeValueLabel("-0", Color.FromArgb(198, 40, 40));
            tlpSummary.Controls.Add(lblPointsDiscount, 1, 3);

            // Row 4: TOTAL (span 2 cols via separate panel)
            var pnlTotal = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(237, 247, 237) };
            var lblTotalTitle = new Label
            {
                Text = "TỔNG CỘNG",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 33, 33),
                AutoSize = false,
                Dock = DockStyle.Left,
                Width = 110,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(6, 0, 0, 0)
            };
            lblTotal = new Label
            {
                Text = "0",
                Font = new Font("Segoe UI", 22F, FontStyle.Bold),
                ForeColor = Color.FromArgb(198, 40, 40),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                Padding = new Padding(0, 0, 8, 0)
            };
            pnlTotal.Controls.Add(lblTotal);
            pnlTotal.Controls.Add(lblTotalTitle);
            tlpSummary.SetColumnSpan(pnlTotal, 2);
            tlpSummary.Controls.Add(pnlTotal, 0, 4);

            right.Controls.Add(tlpSummary, 0, 1);

            // ── Row 2: Payment method + Paid amount ─────────────────────────────
            var tlpPay = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(4, 4, 4, 0)
            };
            tlpPay.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpPay.RowStyles.Add(new RowStyle(SizeType.Absolute, 20)); // label
            tlpPay.RowStyles.Add(new RowStyle(SizeType.Absolute, 30)); // combo
            tlpPay.RowStyles.Add(new RowStyle(SizeType.Absolute, 20)); // label
            tlpPay.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // numPaid + change

            tlpPay.Controls.Add(MakeLabel("Phương thức thanh toán:"), 0, 0);
            cboPaymentMethod = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F)
            };
            cboPaymentMethod.Items.AddRange(new[] { "Tiền mặt", "Chuyển khoản" });
            cboPaymentMethod.SelectedIndex = 0;
            tlpPay.Controls.Add(cboPaymentMethod, 0, 1);

            tlpPay.Controls.Add(MakeLabel("Tiền khách đưa:"), 0, 2);

            var pnlPaid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(0)
            };
            pnlPaid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            pnlPaid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            pnlPaid.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            numPaid = new NumericUpDown
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                Maximum = 1_000_000_000,
                ThousandsSeparator = true,
                TextAlign = HorizontalAlignment.Right
            };
            numPaid.ValueChanged += (s, e) => CalculateChange();
            lblChange = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Thối: 0đ",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(27, 94, 32),
                TextAlign = ContentAlignment.MiddleRight,
                Padding = new Padding(0, 0, 4, 0)
            };
            pnlPaid.Controls.Add(numPaid, 0, 0);
            pnlPaid.Controls.Add(lblChange, 1, 0);
            tlpPay.Controls.Add(pnlPaid, 0, 3);

            right.Controls.Add(tlpPay, 0, 2);

            // ── Row 3: CHECKOUT button ───────────────────────────────────────────
            var btnCheckout = new Button
            {
                Text = "THANH TOÁN",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(27, 94, 32),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
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
            dgvCart.Columns.Add("UnitName", "Đơn vị"); dgvCart.Columns["UnitName"].Width = 80; dgvCart.Columns["UnitName"].ReadOnly = true;
            dgvCart.Columns.Add("UnitPrice", "Đơn giá"); dgvCart.Columns["UnitPrice"].DefaultCellStyle.Format = "N0"; dgvCart.Columns["UnitPrice"].ReadOnly = true;
            dgvCart.Columns.Add("Quantity", "Số lượng"); dgvCart.Columns["Quantity"].Width = 80;
            dgvCart.Columns.Add("SubTotal", "Thành tiền"); dgvCart.Columns["SubTotal"].DefaultCellStyle.Format = "N0"; dgvCart.Columns["SubTotal"].ReadOnly = true;
            
            DataGridViewButtonColumn btnDel = new DataGridViewButtonColumn { Text = "X", Name = "Delete", UseColumnTextForButtonValue = true, Width = 40 };
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
            if (term.Length < 2) { lstSuggestions.Visible = false; return; }

            var products = _controller.FindProducts(term);
            if (products.Any())
            {
                lstSuggestions.DataSource = products;
                lstSuggestions.DisplayMember = "ProductName";
                lstSuggestions.Visible = true;
                lstSuggestions.BringToFront();
            }
            else { lstSuggestions.Visible = false; }
        }

        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down && lstSuggestions.Visible) lstSuggestions.Focus();
            if (e.KeyCode == Keys.Enter && !lstSuggestions.Visible) { /* Handle direct barcode */ }
            if (e.KeyCode == Keys.Enter && lstSuggestions.Visible) AddSelectedSuggestion();
        }

        private void AddSelectedSuggestion()
        {
            if (lstSuggestions.SelectedItem is CartItem product)
            {
                AddToCart(product);
                txtSearch.Clear();
                lstSuggestions.Visible = false;
                txtSearch.Focus();
            }
        }

        private void AddToCart(CartItem product)
        {
            var existing = _cart.FirstOrDefault(x => x.ProductID == product.ProductID);
            if (existing != null) existing.Quantity += 1;
            else _cart.Add(product);
            RefreshCart();
        }

        private void RefreshCart()
        {
            dgvCart.Rows.Clear();
            foreach (var item in _cart)
            {
                dgvCart.Rows.Add(item.ProductID, item.ProductName, item.UnitName, item.UnitPrice, item.Quantity, item.SubTotal);
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
                _usedPoints = _currentCustomer.Points;
                _pointsDiscount = _usedPoints; // 1 point = 1d
            }
            lblPointsDiscount.Text = "-" + _pointsDiscount.ToString("N0");

            decimal total = subTotal - _voucherDiscount - _pointsDiscount;
            if (total < 0) total = 0;

            lblTotal.Text = total.ToString("N0");
            numPaid.Value = total;
            CalculateChange();
        }

        private void CalculateChange()
        {
            decimal total = decimal.Parse(lblTotal.Text.Replace(",", ""));
            decimal change = numPaid.Value - total;
            lblChange.Text = "Tiền thối: " + (change >= 0 ? change.ToString("N0") : "0") + " đ";
        }

        private void TxtPhone_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                _currentCustomer = _controller.FindCustomer(txtPhone.Text.Trim());
                if (_currentCustomer != null)
                {
                    lblCustomerInfo.Text = $"{_currentCustomer.CustomerName} - Điểm: {_currentCustomer.Points:N0}";
                    chkUsePoints.Visible = true;
                    chkUsePoints.Text = $"Dùng {_currentCustomer.Points:N0} điểm (-{_currentCustomer.Points:N0}đ)";
                }
                else
                {
                    lblCustomerInfo.Text = "Không tìm thấy";
                    chkUsePoints.Visible = false;
                }
                UpdateTotal();
            }
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
                decimal qty = Convert.ToDecimal(dgvCart.Rows[e.RowIndex].Cells["Quantity"].Value);
                var item = _cart.First(x => x.ProductID == pid);
                item.Quantity = qty;
                RefreshCart();
            }
        }

        private void UpdateShiftInfo()
        {
            lblShiftInfo.Text = $"| NV: {UserSession.CurrentUser?.FullName ?? "Admin"} | {DateTime.Now:dd/MM/yyyy HH:mm}";
        }

        private void ProcessCheckout()
        {
            if (!_cart.Any()) { MessageBox.Show("Giỏ hàng trống!"); return; }
            if (numPaid.Value < decimal.Parse(lblTotal.Text.Replace(",", ""))) { MessageBox.Show("Tiền khách đưa chưa đủ!"); return; }

            var request = new CheckoutRequest
            {
                CustomerID = _currentCustomer?.CustomerID,
                UserID = UserSession.CurrentUser?.UserID ?? 1,
                SubTotal = _cart.Sum(x => x.SubTotal),
                TotalAmount = decimal.Parse(lblTotal.Text.Replace(",", "")),
                PaidAmount = numPaid.Value,
                PaymentMethod = (byte)(cboPaymentMethod.SelectedIndex + 1),
                VoucherCode = _appliedVoucher?.VoucherCode,
                VoucherDiscount = _voucherDiscount,
                PointsDiscount = _pointsDiscount,
                UsedPoints = _usedPoints,
                EarnedPoints = (int)(decimal.Parse(lblTotal.Text.Replace(",", "")) * 0.01m), // 1%
                Items = _cart
            };

            int invoiceId = _controller.Checkout(request);
            if (invoiceId > 0)
            {
                MessageBox.Show("Thanh toán thành công!");
                ShowInvoicePreview(invoiceId);
                ResetForm();
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
            lblCustomerInfo.Text = "Khách lẻ";
            RefreshCart();
        }
    }
}
