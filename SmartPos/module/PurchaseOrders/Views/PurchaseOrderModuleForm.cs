using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using SmartPos.Module.PurchaseOrders.Controllers;
using SmartPos.Module.PurchaseOrders.Models;

namespace SmartPos.Module.PurchaseOrders.Views
{
    public class PurchaseOrderModuleForm : Form
    {
        private readonly PurchaseOrderController _controller;

        private ComboBox cboSupplier;
        private ComboBox cboUser;
        private ComboBox cboWarehouse;
        private ComboBox cboPaymentStatus;
        private DateTimePicker dtpOrderDate;
        private TextBox txtNotes;

        private ComboBox cboProduct;
        private TextBox txtBatch;
        private TextBox txtShelfLocation;
        private DateTimePicker dtpManufacture;
        private DateTimePicker dtpExpiry;
        private NumericUpDown numQuantity;
        private NumericUpDown numCost;

        private Button btnAddItem;
        private Button btnCreateOrder;
        private Button btnViewFefo;

        private DataGridView dgvItems;
        private DataGridView dgvFefo;
        private Label lblError;
        private Label lblTotalAmount;

        private PurchaseOrderModuleData _moduleData;
        private readonly BindingSource _itemsBinding;

        public PurchaseOrderModuleForm()
        {
            _controller = new PurchaseOrderController();
            _itemsBinding = new BindingSource();
            InitializeUi();
            Load += PurchaseOrderModuleForm_Load;
        }

        private void PurchaseOrderModuleForm_Load(object sender, EventArgs e)
        {
            try
            {
                _moduleData = _controller.InitializeModule();
                BindMasterData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Khong the khoi tao module phieu nhap.\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeUi()
        {
            Text = "Phieu nhap hang + Quan ly lo";
            StartPosition = FormStartPosition.CenterParent;
            Width = 1300;
            Height = 780;
            BackColor = Color.White;

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 4,
                ColumnCount = 1
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 160));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 165));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 45));

            var header = BuildHeaderPanel();
            var detail = BuildDetailInputPanel();
            var itemsPanel = BuildItemsPanel();
            var fefoPanel = BuildFefoPanel();

            root.Controls.Add(header, 0, 0);
            root.Controls.Add(detail, 0, 1);
            root.Controls.Add(itemsPanel, 0, 2);
            root.Controls.Add(fefoPanel, 0, 3);

            Controls.Add(root);
        }

        private Control BuildHeaderPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12), BorderStyle = BorderStyle.FixedSingle };

            var lblSupplier = new Label { Text = "Nha cung cap", Location = new Point(10, 14), AutoSize = true };
            cboSupplier = new ComboBox { Location = new Point(110, 10), Width = 280, DropDownStyle = ComboBoxStyle.DropDownList };

            var lblUser = new Label { Text = "Nguoi nhap", Location = new Point(420, 14), AutoSize = true };
            cboUser = new ComboBox { Location = new Point(500, 10), Width = 260, DropDownStyle = ComboBoxStyle.DropDownList };

            var lblOrderDate = new Label { Text = "Ngay nhap", Location = new Point(790, 14), AutoSize = true };
            dtpOrderDate = new DateTimePicker { Location = new Point(860, 10), Width = 180, Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy HH:mm" };

            var lblWarehouse = new Label { Text = "Kho nhap", Location = new Point(10, 52), AutoSize = true };
            cboWarehouse = new ComboBox { Location = new Point(110, 48), Width = 280, DropDownStyle = ComboBoxStyle.DropDownList };

            var lblPaymentStatus = new Label { Text = "Trang thai TT", Location = new Point(420, 52), AutoSize = true };
            cboPaymentStatus = new ComboBox { Location = new Point(500, 48), Width = 260, DropDownStyle = ComboBoxStyle.DropDownList };
            cboPaymentStatus.Items.Add(new PaymentStatusItem { Id = 1, Name = "Chua thanh toan" });
            cboPaymentStatus.Items.Add(new PaymentStatusItem { Id = 2, Name = "Thanh toan mot phan" });
            cboPaymentStatus.Items.Add(new PaymentStatusItem { Id = 3, Name = "Da thanh toan" });
            cboPaymentStatus.SelectedIndex = 0;

            var lblNotes = new Label { Text = "Ghi chu", Location = new Point(10, 90), AutoSize = true };
            txtNotes = new TextBox { Location = new Point(110, 86), Width = 650 };

            btnCreateOrder = new Button
            {
                Text = "Tao phieu nhap",
                Location = new Point(860, 80),
                Size = new Size(180, 38),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(25, 118, 210),
                ForeColor = Color.White
            };
            btnCreateOrder.Click += btnCreateOrder_Click;

            lblError = new Label
            {
                ForeColor = Color.Firebrick,
                Location = new Point(10, 125),
                Width = 1200,
                Height = 26,
                Visible = false
            };

            panel.Controls.Add(lblSupplier);
            panel.Controls.Add(cboSupplier);
            panel.Controls.Add(lblUser);
            panel.Controls.Add(cboUser);
            panel.Controls.Add(lblOrderDate);
            panel.Controls.Add(dtpOrderDate);
            panel.Controls.Add(lblWarehouse);
            panel.Controls.Add(cboWarehouse);
            panel.Controls.Add(lblPaymentStatus);
            panel.Controls.Add(cboPaymentStatus);
            panel.Controls.Add(lblNotes);
            panel.Controls.Add(txtNotes);
            panel.Controls.Add(btnCreateOrder);
            panel.Controls.Add(lblError);

            return panel;
        }

        private Control BuildDetailInputPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12), BorderStyle = BorderStyle.FixedSingle };

            var lblProduct = new Label { Text = "San pham", Location = new Point(10, 14), AutoSize = true };
            cboProduct = new ComboBox { Location = new Point(110, 10), Width = 360, DropDownStyle = ComboBoxStyle.DropDownList };

            var lblBatch = new Label { Text = "So lo", Location = new Point(500, 14), AutoSize = true };
            txtBatch = new TextBox { Location = new Point(550, 10), Width = 160 };

            var lblMfg = new Label { Text = "NSX", Location = new Point(730, 14), AutoSize = true };
            dtpManufacture = new DateTimePicker { Location = new Point(765, 10), Width = 130, Format = DateTimePickerFormat.Short, ShowCheckBox = true };

            var lblExp = new Label { Text = "HSD", Location = new Point(910, 14), AutoSize = true };
            dtpExpiry = new DateTimePicker { Location = new Point(945, 10), Width = 130, Format = DateTimePickerFormat.Short, ShowCheckBox = true };

            var lblQty = new Label { Text = "So luong", Location = new Point(10, 52), AutoSize = true };
            numQuantity = new NumericUpDown
            {
                Location = new Point(110, 48),
                Width = 160,
                DecimalPlaces = 2,
                Maximum = 1000000,
                Minimum = 0,
                ThousandsSeparator = true
            };

            var lblCost = new Label { Text = "Gia nhap", Location = new Point(290, 52), AutoSize = true };
            numCost = new NumericUpDown
            {
                Location = new Point(350, 48),
                Width = 170,
                DecimalPlaces = 0,
                Maximum = 1000000000,
                Minimum = 0,
                ThousandsSeparator = true
            };

            var lblShelf = new Label { Text = "Kệ", Location = new Point(530, 52), AutoSize = true };
            txtShelfLocation = new TextBox { Location = new Point(560, 48), Width = 160 };

            btnAddItem = new Button
            {
                Text = "Them dong",
                Location = new Point(730, 44),
                Size = new Size(160, 34),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            btnAddItem.Click += btnAddItem_Click;

            btnViewFefo = new Button
            {
                Text = "Xem FEFO",
                Location = new Point(900, 44),
                Size = new Size(160, 34),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            btnViewFefo.Click += btnViewFefo_Click;

            lblTotalAmount = new Label
            {
                Text = "Tong tien: 0",
                Location = new Point(1070, 52),
                AutoSize = true,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 102, 68)
            };

            panel.Controls.Add(lblProduct);
            panel.Controls.Add(cboProduct);
            panel.Controls.Add(lblBatch);
            panel.Controls.Add(txtBatch);
            panel.Controls.Add(lblMfg);
            panel.Controls.Add(dtpManufacture);
            panel.Controls.Add(lblExp);
            panel.Controls.Add(dtpExpiry);
            panel.Controls.Add(lblQty);
            panel.Controls.Add(numQuantity);
            panel.Controls.Add(lblCost);
            panel.Controls.Add(numCost);
            panel.Controls.Add(lblShelf);
            panel.Controls.Add(txtShelfLocation);
            panel.Controls.Add(btnAddItem);
            panel.Controls.Add(btnViewFefo);
            panel.Controls.Add(lblTotalAmount);

            return panel;
        }

        private Control BuildItemsPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12), BorderStyle = BorderStyle.FixedSingle };

            var lbl = new Label
            {
                Text = "Chi tiet san pham theo lo",
                Dock = DockStyle.Top,
                Height = 24,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };

            dgvItems = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoGenerateColumns = false
            };
            dgvItems.KeyDown += dgvItems_KeyDown;

            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "San pham", DataPropertyName = "ProductDisplay", Width = 280 });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "So lo", DataPropertyName = "BatchNumber", Width = 110 });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Kệ/Vị trí", DataPropertyName = "ShelfLocation", Width = 100 });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "NSX", DataPropertyName = "ManufactureDate", Width = 90, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy" } });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "HSD", DataPropertyName = "ExpiryDate", Width = 90, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy" } });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "So luong", DataPropertyName = "Quantity", Width = 110, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Gia nhap", DataPropertyName = "CostPrice", Width = 130, DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" } });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Thanh tien", DataPropertyName = "LineTotal", Width = 130, DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" } });

            _itemsBinding.DataSource = new List<PurchaseOrderDraftItem>();
            dgvItems.DataSource = _itemsBinding;

            panel.Controls.Add(dgvItems);
            panel.Controls.Add(lbl);
            return panel;
        }

        private Control BuildFefoPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12), BorderStyle = BorderStyle.FixedSingle };

            var lbl = new Label
            {
                Text = "FEFO Preview (Het han truoc xuat truoc)",
                Dock = DockStyle.Top,
                Height = 24,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };

            dgvFefo = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoGenerateColumns = false
            };

            dgvFefo.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Sản phẩm", DataPropertyName = "ProductName", Width = 220 });
            dgvFefo.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Số lô", DataPropertyName = "BatchNumber", Width = 120 });
            dgvFefo.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Kệ/Vị trí", DataPropertyName = "ShelfLocation", Width = 100 });
            dgvFefo.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "HSD", DataPropertyName = "ExpiryDate", Width = 110, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy" } });
            dgvFefo.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Số lượng", DataPropertyName = "Quantity", Width = 120, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
            dgvFefo.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Nhà kho", DataPropertyName = "WarehouseName", Width = 220 });

            panel.Controls.Add(dgvFefo);
            panel.Controls.Add(lbl);
            return panel;
        }

        private void BindMasterData()
        {
            cboSupplier.DataSource = _moduleData.Suppliers;
            cboUser.DataSource = _moduleData.Users;
            cboWarehouse.DataSource = _moduleData.Warehouses;
            cboProduct.DataSource = _moduleData.Products;

            if (UserSession.CurrentUser != null)
            {
                UserOption matched = _moduleData.Users.FirstOrDefault(x => x.UserID == UserSession.CurrentUser.UserID);
                if (matched != null)
                {
                    cboUser.SelectedItem = matched;
                }
            }
        }

        private void btnAddItem_Click(object sender, EventArgs e)
        {
            lblError.Visible = false;

            ProductOption selectedProduct = cboProduct.SelectedItem as ProductOption;
            if (selectedProduct == null)
            {
                ShowError("Vui long chon san pham.");
                return;
            }

            string batchNumber = txtBatch.Text.Trim();
            if (string.IsNullOrWhiteSpace(batchNumber))
            {
                ShowError("Vui long nhap so lo.");
                txtBatch.Focus();
                return;
            }

            decimal quantity = numQuantity.Value;
            decimal costPrice = numCost.Value;

            if (quantity <= 0)
            {
                ShowError("So luong phai lon hon 0.");
                return;
            }

            DateTime? mfg = dtpManufacture.Checked ? (DateTime?)dtpManufacture.Value.Date : null;
            DateTime? exp = dtpExpiry.Checked ? (DateTime?)dtpExpiry.Value.Date : null;

            if (mfg.HasValue && exp.HasValue && exp.Value <= mfg.Value)
            {
                ShowError("Ngay het han phai lon hon ngay san xuat.");
                return;
            }

            var items = _itemsBinding.DataSource as List<PurchaseOrderDraftItem>;
            items.Add(new PurchaseOrderDraftItem
            {
                ProductID = selectedProduct.ProductID,
                UnitID = selectedProduct.BaseUnitID,
                ProductDisplay = selectedProduct.DisplayText,
                BatchNumber = batchNumber,
                ShelfLocation = txtShelfLocation.Text.Trim(),
                ManufactureDate = mfg,
                ExpiryDate = exp,
                Quantity = quantity,
                CostPrice = costPrice
            });

            _itemsBinding.ResetBindings(false);
            UpdateTotalAmount();

            txtBatch.Clear();
            txtShelfLocation.Clear();
            numQuantity.Value = 0;
            numCost.Value = 0;
        }

        private void btnCreateOrder_Click(object sender, EventArgs e)
        {
            lblError.Visible = false;

            var supplier = cboSupplier.SelectedItem as SupplierOption;
            var user = cboUser.SelectedItem as UserOption;
            var warehouse = cboWarehouse.SelectedItem as WarehouseOption;
            var paymentStatus = cboPaymentStatus.SelectedItem as PaymentStatusItem;
            var items = _itemsBinding.DataSource as List<PurchaseOrderDraftItem>;

            try
            {
                var request = new CreatePurchaseOrderRequest
                {
                    SupplierID = supplier == null ? 0 : supplier.SupplierID,
                    CreatedByUserID = user == null ? 0 : user.UserID,
                    WarehouseID = warehouse == null ? 0 : warehouse.WarehouseID,
                    PaymentStatus = paymentStatus == null ? (byte)1 : paymentStatus.Id,
                    OrderDate = dtpOrderDate.Value,
                    Notes = txtNotes.Text.Trim(),
                    Items = items.ToList()
                };

                int orderId = _controller.CreatePurchaseOrder(request);

                MessageBox.Show("Tao phieu nhap thanh cong. ID: " + orderId, "Thong bao", MessageBoxButtons.OK, MessageBoxIcon.Information);

                items.Clear();
                _itemsBinding.ResetBindings(false);
                dgvFefo.DataSource = null;
                UpdateTotalAmount();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private void btnViewFefo_Click(object sender, EventArgs e)
        {
            lblError.Visible = false;

            ProductOption selectedProduct = cboProduct.SelectedItem as ProductOption;
            if (selectedProduct == null)
            {
                ShowError("Vui long chon san pham de xem FEFO.");
                return;
            }

            try
            {
                List<FefoBatchItem> fefoBatches = _controller.GetFefoBatches(selectedProduct.ProductID);
                dgvFefo.DataSource = fefoBatches;
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private void dgvItems_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Delete)
            {
                return;
            }

            if (dgvItems.CurrentRow == null)
            {
                return;
            }

            var items = _itemsBinding.DataSource as List<PurchaseOrderDraftItem>;
            var item = dgvItems.CurrentRow.DataBoundItem as PurchaseOrderDraftItem;
            if (items == null || item == null)
            {
                return;
            }

            items.Remove(item);
            _itemsBinding.ResetBindings(false);
            UpdateTotalAmount();
        }

        private void UpdateTotalAmount()
        {
            var items = _itemsBinding.DataSource as List<PurchaseOrderDraftItem>;
            decimal total = 0m;

            for (int i = 0; i < items.Count; i++)
            {
                total += items[i].LineTotal;
            }

            lblTotalAmount.Text = "Tong tien: " + total.ToString("N0", CultureInfo.InvariantCulture);
        }

        private void ShowError(string message)
        {
            lblError.Text = message;
            lblError.Visible = true;
        }

        private class PaymentStatusItem
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
