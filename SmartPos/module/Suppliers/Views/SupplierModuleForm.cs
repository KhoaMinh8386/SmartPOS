using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using SmartPos.Module.Suppliers.Controllers;
using SmartPos.Module.Suppliers.Models;

namespace SmartPos.Module.Suppliers.Views
{
    public class SupplierModuleForm : Form
    {
        private readonly SupplierController _controller;

        private DataGridView dgvSuppliers;
        private DataGridView dgvOrders;
        private PictureBox picSupplier;
        private Label lblSupplierName;
        private Label lblPhone;
        private Label lblAddress;
        private Label lblTotalDebt;
        private NumericUpDown numPaymentAmount;
        private ComboBox cboPaymentMethod;
        private TextBox txtPaymentNote;
        private Label lblError;
        private Button btnPay;
        private Button btnRefresh;

        private List<SupplierListItem> _suppliers;
        private List<SupplierOrderItem> _orders;

        public SupplierModuleForm()
        {
            _controller = new SupplierController();
            InitializeUi();
            Load += SupplierModuleForm_Load;
        }

        private void SupplierModuleForm_Load(object sender, EventArgs e)
        {
            try
            {
                _controller.InitializeModule();
                LoadSuppliers();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Khong the khoi tao module nha cung cap.\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeUi()
        {
            Text = "Supplier Module - Quan ly cong no";
            StartPosition = FormStartPosition.CenterParent;
            Width = 1200;
            Height = 720;
            BackColor = Color.White;

            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                SplitterDistance = 380,
                FixedPanel = FixedPanel.Panel1
            };

            var leftPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12) };
            var rightPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12) };

            dgvSuppliers = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoGenerateColumns = false
            };
            dgvSuppliers.SelectionChanged += dgvSuppliers_SelectionChanged;

            dgvSuppliers.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Ten", DataPropertyName = "SupplierName", Width = 160 });
            dgvSuppliers.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Dien thoai", DataPropertyName = "Phone", Width = 110 });
            dgvSuppliers.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tong cong no", DataPropertyName = "TotalDebt", Width = 95, DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" } });

            btnRefresh = new Button
            {
                Text = "Lam moi",
                Dock = DockStyle.Top,
                Height = 34,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            btnRefresh.Click += btnRefresh_Click;

            leftPanel.Controls.Add(dgvSuppliers);
            leftPanel.Controls.Add(btnRefresh);

            var infoPanel = new Panel { Dock = DockStyle.Top, Height = 170, BorderStyle = BorderStyle.FixedSingle };

            picSupplier = new PictureBox
            {
                Location = new Point(12, 12),
                Size = new Size(120, 120),
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle
            };

            lblSupplierName = new Label { Location = new Point(145, 18), AutoSize = true, Font = new Font("Segoe UI", 12F, FontStyle.Bold) };
            lblPhone = new Label { Location = new Point(145, 52), AutoSize = true, Font = new Font("Segoe UI", 10F) };
            lblAddress = new Label { Location = new Point(145, 78), Size = new Size(600, 42), Font = new Font("Segoe UI", 10F) };
            lblTotalDebt = new Label { Location = new Point(145, 127), AutoSize = true, Font = new Font("Segoe UI", 11F, FontStyle.Bold), ForeColor = Color.Firebrick };

            infoPanel.Controls.Add(picSupplier);
            infoPanel.Controls.Add(lblSupplierName);
            infoPanel.Controls.Add(lblPhone);
            infoPanel.Controls.Add(lblAddress);
            infoPanel.Controls.Add(lblTotalDebt);

            dgvOrders = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoGenerateColumns = false
            };

            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Ma hoa don", DataPropertyName = "InvoiceCode", Width = 120 });
            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Ngay", DataPropertyName = "OrderDate", Width = 120, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy HH:mm" } });
            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tong tien", DataPropertyName = "TotalAmount", Width = 120, DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" } });
            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Da tra", DataPropertyName = "PaidAmount", Width = 120, DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" } });
            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Con no", DataPropertyName = "DebtAmount", Width = 120, DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" } });
            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Trang thai", DataPropertyName = "StatusText", Width = 150 });

            var paymentPanel = new Panel { Dock = DockStyle.Bottom, Height = 130, BorderStyle = BorderStyle.FixedSingle, Padding = new Padding(10) };

            var lblAmount = new Label { Text = "So tien thanh toan", Location = new Point(10, 14), AutoSize = true };
            numPaymentAmount = new NumericUpDown
            {
                Location = new Point(140, 10),
                Width = 160,
                DecimalPlaces = 0,
                Maximum = 1000000000,
                ThousandsSeparator = true
            };

            var lblMethod = new Label { Text = "Phuong thuc", Location = new Point(330, 14), AutoSize = true };
            cboPaymentMethod = new ComboBox
            {
                Location = new Point(410, 10),
                Width = 170,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboPaymentMethod.Items.Add(new PaymentMethodItem { Id = 1, Name = "Tien mat" });
            cboPaymentMethod.Items.Add(new PaymentMethodItem { Id = 2, Name = "Chuyen khoan" });
            cboPaymentMethod.SelectedIndex = 0;

            var lblNote = new Label { Text = "Ghi chu", Location = new Point(10, 48), AutoSize = true };
            txtPaymentNote = new TextBox { Location = new Point(140, 45), Width = 440 };

            btnPay = new Button
            {
                Text = "Thanh toan",
                Location = new Point(600, 10),
                Size = new Size(130, 60),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = Color.FromArgb(25, 118, 210),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnPay.Click += btnPay_Click;

            lblError = new Label
            {
                ForeColor = Color.Firebrick,
                Location = new Point(140, 80),
                Width = 700,
                Height = 30,
                Visible = false
            };

            paymentPanel.Controls.Add(lblAmount);
            paymentPanel.Controls.Add(numPaymentAmount);
            paymentPanel.Controls.Add(lblMethod);
            paymentPanel.Controls.Add(cboPaymentMethod);
            paymentPanel.Controls.Add(lblNote);
            paymentPanel.Controls.Add(txtPaymentNote);
            paymentPanel.Controls.Add(btnPay);
            paymentPanel.Controls.Add(lblError);

            rightPanel.Controls.Add(dgvOrders);
            rightPanel.Controls.Add(paymentPanel);
            rightPanel.Controls.Add(infoPanel);

            split.Panel1.Controls.Add(leftPanel);
            split.Panel2.Controls.Add(rightPanel);

            Controls.Add(split);
        }

        private void LoadSuppliers()
        {
            _suppliers = _controller.GetSuppliers();
            dgvSuppliers.DataSource = _suppliers;

            if (_suppliers.Count > 0)
            {
                dgvSuppliers.Rows[0].Selected = true;
                DisplaySupplier(_suppliers[0]);
                LoadOrders(_suppliers[0].SupplierID);
            }
            else
            {
                DisplaySupplier(null);
                dgvOrders.DataSource = null;
            }
        }

        private void LoadOrders(int supplierId)
        {
            _orders = _controller.GetOrders(supplierId);
            dgvOrders.DataSource = _orders;
        }

        private void DisplaySupplier(SupplierListItem supplier)
        {
            lblError.Visible = false;

            if (supplier == null)
            {
                lblSupplierName.Text = "Khong co nha cung cap";
                lblPhone.Text = "Dien thoai:";
                lblAddress.Text = "Dia chi:";
                lblTotalDebt.Text = "Tong cong no: 0";
                picSupplier.Image = null;
                return;
            }

            lblSupplierName.Text = supplier.SupplierName;
            lblPhone.Text = "Dien thoai: " + supplier.Phone;
            lblAddress.Text = "Dia chi: " + supplier.Address;
            lblTotalDebt.Text = "Tong cong no: " + supplier.TotalDebt.ToString("N0", CultureInfo.InvariantCulture);

            LoadSupplierImage(supplier.ImageUrl);
        }

        private void LoadSupplierImage(string imageUrl)
        {
            picSupplier.Image = null;

            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return;
            }

            try
            {
                picSupplier.Load(imageUrl);
            }
            catch
            {
                picSupplier.Image = null;
            }
        }

        private void dgvSuppliers_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvSuppliers.CurrentRow == null)
            {
                return;
            }

            var supplier = dgvSuppliers.CurrentRow.DataBoundItem as SupplierListItem;
            if (supplier == null)
            {
                return;
            }

            DisplaySupplier(supplier);
            LoadOrders(supplier.SupplierID);
        }

        private void btnPay_Click(object sender, EventArgs e)
        {
            lblError.Visible = false;

            var supplier = dgvSuppliers.CurrentRow == null ? null : dgvSuppliers.CurrentRow.DataBoundItem as SupplierListItem;
            var order = dgvOrders.CurrentRow == null ? null : dgvOrders.CurrentRow.DataBoundItem as SupplierOrderItem;

            if (supplier == null)
            {
                ShowError("Vui long chon nha cung cap.");
                return;
            }

            if (order == null)
            {
                ShowError("Vui long chon phieu nhap can thanh toan.");
                return;
            }

            decimal amount = numPaymentAmount.Value;
            if (amount <= 0)
            {
                ShowError("So tien thanh toan phai lon hon 0.");
                return;
            }

            if (amount > order.DebtAmount)
            {
                ShowError("So tien thanh toan vuot qua cong no cua phieu nhap.");
                return;
            }

            try
            {
                var selectedMethod = cboPaymentMethod.SelectedItem as PaymentMethodItem;
                byte paymentMethod = selectedMethod == null ? (byte)1 : selectedMethod.Id;
                int? userId = UserSession.CurrentUser == null ? (int?)null : UserSession.CurrentUser.UserID;

                var request = new SupplierPaymentRequest
                {
                    SupplierID = supplier.SupplierID,
                    PurchaseOrderID = order.PurchaseOrderID,
                    Amount = amount,
                    PaymentMethod = paymentMethod,
                    Note = txtPaymentNote.Text.Trim(),
                    CreatedByUserID = userId
                };

                _controller.AddPayment(request);

                numPaymentAmount.Value = 0;
                txtPaymentNote.Clear();

                int supplierId = supplier.SupplierID;
                int purchaseOrderId = order.PurchaseOrderID;

                LoadSuppliers();
                ReselectRows(supplierId, purchaseOrderId);

                MessageBox.Show("Thanh toan thanh cong.", "Thong bao", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private void ReselectRows(int supplierId, int purchaseOrderId)
        {
            if (_suppliers == null || _suppliers.Count == 0)
            {
                return;
            }

            SupplierListItem supplier = _suppliers.FirstOrDefault(x => x.SupplierID == supplierId) ?? _suppliers[0];
            int supplierIndex = _suppliers.FindIndex(x => x.SupplierID == supplier.SupplierID);
            if (supplierIndex >= 0 && supplierIndex < dgvSuppliers.Rows.Count)
            {
                dgvSuppliers.ClearSelection();
                dgvSuppliers.Rows[supplierIndex].Selected = true;
                dgvSuppliers.CurrentCell = dgvSuppliers.Rows[supplierIndex].Cells[0];
            }

            LoadOrders(supplier.SupplierID);

            if (_orders == null || _orders.Count == 0)
            {
                return;
            }

            int orderIndex = _orders.FindIndex(x => x.PurchaseOrderID == purchaseOrderId);
            if (orderIndex < 0)
            {
                orderIndex = 0;
            }

            if (orderIndex < dgvOrders.Rows.Count)
            {
                dgvOrders.ClearSelection();
                dgvOrders.Rows[orderIndex].Selected = true;
                dgvOrders.CurrentCell = dgvOrders.Rows[orderIndex].Cells[0];
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadSuppliers();
        }

        private void ShowError(string message)
        {
            lblError.Text = message;
            lblError.Visible = true;
        }

        private class PaymentMethodItem
        {
            public byte Id { get; set; }
            public string Name { get; set; }

            public override string ToString()
            {
                return Name;
            }
        }
    }
}
