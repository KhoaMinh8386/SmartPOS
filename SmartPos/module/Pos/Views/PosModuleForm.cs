using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SmartPos.Module.Pos.Controllers;
using SmartPos.Module.Pos.Models;

namespace SmartPos.Module.Pos.Views
{
    public class PosModuleForm : Form
    {
        private readonly PosController _controller;
        private List<CartItem> _cart = new List<CartItem>();
        private CustomerInfo _currentCustomer = null;

        private DataGridView dgvCart;
        private TextBox txtSearch;
        private ListBox lstSearchResults;
        private Label lblTotal;
        private TextBox txtCustomerPhone;
        private Label lblCustomerName;
        private NumericUpDown numPaid;
        private Label lblChange;
        private ComboBox cboPaymentMethod;
        private Button btnCheckout;

        public PosModuleForm()
        {
            _controller = new PosController();
            InitializeUi();
        }

        private void InitializeUi()
        {
            Text = "POS - Ban hang";
            Width = 1200;
            Height = 800;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(240, 242, 245);

            var mainSplit = new SplitContainer
            {
                Dock = DockStyle.Fill,
                SplitterDistance = 800,
                FixedPanel = FixedPanel.Panel2
            };

            // Left: Cart & Product Search
            var leftPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            
            var searchPanel = new Panel { Dock = DockStyle.Top, Height = 100 };
            var lblSearch = new Label { Text = "Tim san pham (Ten/SKU/Barcode)", Location = new Point(0, 10), AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            txtSearch = new TextBox { Location = new Point(0, 35), Width = 600, Font = new Font("Segoe UI", 14F) };
            txtSearch.TextChanged += TxtSearch_TextChanged;
            txtSearch.KeyDown += TxtSearch_KeyDown;

            lstSearchResults = new ListBox { Location = new Point(0, 70), Width = 600, Height = 200, Visible = false, Font = new Font("Segoe UI", 10F) };
            lstSearchResults.DoubleClick += LstSearchResults_DoubleClick;
            lstSearchResults.KeyDown += LstSearchResults_KeyDown;

            searchPanel.Controls.AddRange(new Control[] { lblSearch, txtSearch, lstSearchResults });

            dgvCart = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                AllowUserToAddRows = false,
                RowTemplate = { Height = 40 },
                Font = new Font("Segoe UI", 10F),
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            dgvCart.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Ten san pham", DataPropertyName = "ProductName", ReadOnly = true });
            dgvCart.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Don gia", DataPropertyName = "UnitPrice", ReadOnly = true, DefaultCellStyle = { Format = "N0" } });
            dgvCart.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "So luong", DataPropertyName = "Quantity" });
            dgvCart.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Don vi", DataPropertyName = "UnitName", ReadOnly = true });
            dgvCart.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Thanh tien", DataPropertyName = "SubTotal", ReadOnly = true, DefaultCellStyle = { Format = "N0" } });
            dgvCart.CellValueChanged += DgvCart_CellValueChanged;
            dgvCart.UserDeletingRow += DgvCart_UserDeletingRow;

            leftPanel.Controls.Add(dgvCart);
            leftPanel.Controls.Add(searchPanel);

            // Right: Checkout & Customer
            var rightPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20), BackColor = Color.White };
            
            var lblSectionCustomer = new Label { Text = "KHACH HANG", Font = new Font("Segoe UI", 12F, FontStyle.Bold), Dock = DockStyle.Top, Height = 30 };
            txtCustomerPhone = new TextBox { Dock = DockStyle.Top, Font = new Font("Segoe UI", 12F), Height = 35 };
            // PlaceholderText not supported in .NET Framework 4.6.1
            txtCustomerPhone.KeyDown += TxtCustomerPhone_KeyDown;
            
            lblCustomerName = new Label { Text = "Khach le", Font = new Font("Segoe UI", 10F, FontStyle.Italic), Dock = DockStyle.Top, Height = 40, ForeColor = Color.DimGray, TextAlign = ContentAlignment.MiddleLeft };

            var pnlCheckout = new Panel { Dock = DockStyle.Bottom, Height = 400 };
            
            lblTotal = new Label { Text = "0", Font = new Font("Segoe UI", 32F, FontStyle.Bold), ForeColor = Color.FromArgb(211, 47, 47), Dock = DockStyle.Top, Height = 80, TextAlign = ContentAlignment.MiddleRight };
            var lblTotalText = new Label { Text = "TONG CONG", Font = new Font("Segoe UI", 10F), Dock = DockStyle.Top, Height = 25, TextAlign = ContentAlignment.MiddleRight };

            var lblPayMethod = new Label { Text = "Phuong thuc", Location = new Point(0, 120), AutoSize = true };
            cboPaymentMethod = new ComboBox { Location = new Point(0, 145), Width = 340, Font = new Font("Segoe UI", 12F), DropDownStyle = ComboBoxStyle.DropDownList };
            cboPaymentMethod.Items.Add("Tien mat");
            cboPaymentMethod.Items.Add("Chuyen khoan");
            cboPaymentMethod.SelectedIndex = 0;

            var lblPaid = new Label { Text = "Khach dua", Location = new Point(0, 190), AutoSize = true };
            numPaid = new NumericUpDown { Location = new Point(0, 215), Width = 340, Font = new Font("Segoe UI", 18F, FontStyle.Bold), Maximum = 1000000000, ThousandsSeparator = true };
            numPaid.ValueChanged += (s, e) => CalculateChange();

            var lblChangeText = new Label { Text = "Tien thoi", Location = new Point(0, 270), AutoSize = true };
            lblChange = new Label { Text = "0", Location = new Point(100, 265), Size = new Size(240, 40), Font = new Font("Segoe UI", 18F, FontStyle.Bold), ForeColor = Color.FromArgb(46, 125, 50), TextAlign = ContentAlignment.MiddleRight };

            btnCheckout = new Button { Text = "THANH TOAN (F9)", Dock = DockStyle.Bottom, Height = 80, BackColor = Color.FromArgb(25, 118, 210), ForeColor = Color.White, Font = new Font("Segoe UI", 16F, FontStyle.Bold), FlatStyle = FlatStyle.Flat };
            btnCheckout.Click += BtnCheckout_Click;

            pnlCheckout.Controls.AddRange(new Control[] { lblTotal, lblTotalText, lblPayMethod, cboPaymentMethod, lblPaid, lblChangeText, lblChange, btnCheckout });

            rightPanel.Controls.Add(lblCustomerName);
            rightPanel.Controls.Add(txtCustomerPhone);
            rightPanel.Controls.Add(lblSectionCustomer);
            rightPanel.Controls.Add(pnlCheckout);

            mainSplit.Panel1.Controls.Add(leftPanel);
            mainSplit.Panel2.Controls.Add(rightPanel);
            Controls.Add(mainSplit);

            this.KeyPreview = true;
            this.KeyDown += PosModuleForm_KeyDown;
        }

        private void PosModuleForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F9) BtnCheckout_Click(null, null);
            if (e.KeyCode == Keys.F1) txtSearch.Focus();
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            string term = txtSearch.Text.Trim();
            if (term.Length < 2)
            {
                lstSearchResults.Visible = false;
                return;
            }

            var results = _controller.SearchProducts(term);
            if (results.Any())
            {
                lstSearchResults.DataSource = results;
                lstSearchResults.DisplayMember = "ProductName";
                lstSearchResults.Visible = true;
                lstSearchResults.BringToFront();
            }
            else
            {
                lstSearchResults.Visible = false;
            }
        }

        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down && lstSearchResults.Visible)
            {
                lstSearchResults.Focus();
                if (lstSearchResults.Items.Count > 0) lstSearchResults.SelectedIndex = 0;
            }
            if (e.KeyCode == Keys.Enter)
            {
                var term = txtSearch.Text.Trim();
                var products = _controller.SearchProducts(term);
                if (products.Count == 1)
                {
                    AddToCart(products[0]);
                    txtSearch.Clear();
                }
            }
        }

        private void LstSearchResults_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && lstSearchResults.SelectedItem != null)
            {
                AddToCart((CartItem)lstSearchResults.SelectedItem);
                txtSearch.Clear();
                txtSearch.Focus();
                lstSearchResults.Visible = false;
            }
        }

        private void LstSearchResults_DoubleClick(object sender, EventArgs e)
        {
            if (lstSearchResults.SelectedItem != null)
            {
                AddToCart((CartItem)lstSearchResults.SelectedItem);
                txtSearch.Clear();
                txtSearch.Focus();
                lstSearchResults.Visible = false;
            }
        }

        private void AddToCart(CartItem product)
        {
            var existing = _cart.FirstOrDefault(x => x.ProductID == product.ProductID);
            if (existing != null)
            {
                existing.Quantity += 1;
            }
            else
            {
                _cart.Add(product);
            }
            RefreshCart();
        }

        private void RefreshCart()
        {
            dgvCart.DataSource = null;
            dgvCart.DataSource = _cart;
            UpdateTotal();
        }

        private void UpdateTotal()
        {
            decimal total = _cart.Sum(x => x.SubTotal);
            lblTotal.Text = total.ToString("N0");
            numPaid.Value = total;
            CalculateChange();
        }

        private void CalculateChange()
        {
            decimal total = _cart.Sum(x => x.SubTotal);
            decimal change = numPaid.Value - total;
            lblChange.Text = change >= 0 ? change.ToString("N0") : "0";
        }

        private void DgvCart_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            UpdateTotal();
        }

        private void DgvCart_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            // Handled by DataSource binding usually, but we refresh anyway
            this.BeginInvoke(new MethodInvoker(UpdateTotal));
        }

        private void TxtCustomerPhone_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string phone = txtCustomerPhone.Text.Trim();
                var customer = _controller.GetCustomer(phone);
                if (customer != null)
                {
                    _currentCustomer = customer;
                    lblCustomerName.Text = customer.CustomerName + " (Diem: " + customer.Points + ")";
                }
                else
                {
                    if (MessageBox.Show("Khong tim thay khach hang. Them moi nhanh?", "Thong bao", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        // Quick add logic
                        _currentCustomer = new CustomerInfo { CustomerName = "Khach moi", Phone = phone };
                        int id = _controller.RegisterCustomer("Khach moi", phone, "");
                        _currentCustomer.CustomerID = id;
                        lblCustomerName.Text = "Khach moi (Vua tao)";
                    }
                }
            }
        }

        private void BtnCheckout_Click(object sender, EventArgs e)
        {
            if (!_cart.Any()) return;

            try
            {
                var request = new CheckoutRequest
                {
                    CustomerID = _currentCustomer?.CustomerID,
                    UserID = UserSession.CurrentUser?.UserID ?? 1,
                    TotalAmount = _cart.Sum(x => x.SubTotal),
                    PaidAmount = numPaid.Value,
                    PaymentMethod = (byte)(cboPaymentMethod.SelectedIndex + 1),
                    Items = _cart
                };

                string code = _controller.Checkout(request);
                MessageBox.Show("Thanh toan thanh cong! Ma HD: " + code, "Thanh cong", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // Reset POS
                _cart.Clear();
                _currentCustomer = null;
                txtCustomerPhone.Clear();
                lblCustomerName.Text = "Khach le";
                RefreshCart();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Loi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
