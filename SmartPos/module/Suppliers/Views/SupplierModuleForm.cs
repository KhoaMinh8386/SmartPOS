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
        private Button btnRefresh, btnAdd, btnEdit, btnDeleteSupplier;
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
                MessageBox.Show("Không thể khởi tạo module nhà cung cấp.\n\n" + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeUi()
        {
            Text = "QUẢN LÝ CÔNG NỢ NHÀ CUNG CẤP";
            StartPosition = FormStartPosition.CenterParent;
            Width = 1300;
            Height = 800;
            BackColor = Color.FromArgb(240, 242, 245);
            Font = new Font("Segoe UI", 10F);

            // Giao diện chính chia làm 2 cột: Danh sách (Trái) và Chi tiết (Phải)
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(15)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28F));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 72F));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            // PANEL BÊN TRÁI: Danh sách Nhà cung cấp
            var leftPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(1) };
            
            var leftHeader = new TableLayoutPanel 
            { 
                Dock = DockStyle.Top, 
                Height = 45, 
                ColumnCount = 4, 
                RowCount = 1,
                BackColor = Color.FromArgb(241, 245, 249),
                Padding = new Padding(2)
            };
            leftHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            leftHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            leftHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            leftHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));

            btnRefresh = CreateHeaderButton("🔄", Color.White, Color.Black);
            btnRefresh.Click += btnRefresh_Click;
            
            btnAdd = CreateHeaderButton("➕", Color.FromArgb(34, 197, 94), Color.White);
            btnAdd.Click += btnAdd_Click;

            btnEdit = CreateHeaderButton("📝", Color.FromArgb(59, 130, 246), Color.White);
            btnEdit.Click += btnEdit_Click;

            btnDeleteSupplier = CreateHeaderButton("🗑️", Color.FromArgb(239, 68, 68), Color.White);
            btnDeleteSupplier.Click += btnDeleteSupplier_Click;

            leftHeader.Controls.Add(btnRefresh, 0, 0);
            leftHeader.Controls.Add(btnAdd, 1, 0);
            leftHeader.Controls.Add(btnEdit, 2, 0);
            leftHeader.Controls.Add(btnDeleteSupplier, 3, 0);

            dgvSuppliers = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoGenerateColumns = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowTemplate = { Height = 45 },
                GridColor = Color.FromArgb(230, 233, 237),
                EnableHeadersVisualStyles = false,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None
            };
            dgvSuppliers.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
            dgvSuppliers.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvSuppliers.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvSuppliers.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvSuppliers.ColumnHeadersHeight = 45;
            dgvSuppliers.SelectionChanged += dgvSuppliers_SelectionChanged;

            dgvSuppliers.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Nhà cung cấp", DataPropertyName = "SupplierName", Width = 180 });
            dgvSuppliers.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tổng nợ", DataPropertyName = "TotalDebt", Width = 110, DefaultCellStyle = new DataGridViewCellStyle { Format = "N0", Alignment = DataGridViewContentAlignment.MiddleRight } });
            dgvSuppliers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            leftPanel.Controls.Add(dgvSuppliers);
            leftPanel.Controls.Add(leftHeader);

            // PANEL BÊN PHẢI: Thông tin chi tiết & Thanh toán
            var rightContainer = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Margin = new Padding(10, 0, 0, 0) };
            rightContainer.RowStyles.Add(new RowStyle(SizeType.Absolute, 200)); // Info Card (Tăng lên 200 để tránh che chữ)
            rightContainer.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));  // Orders Grid
            rightContainer.RowStyles.Add(new RowStyle(SizeType.Absolute, 130)); // Payment Card (Giảm xuống 130 cho gọn)

            // 1. Info Card (Top)
            var infoCard = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(20), Margin = new Padding(0, 0, 0, 10) };
            picSupplier = new PictureBox
            {
                Location = new Point(25, 25),
                Size = new Size(130, 130),
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(248, 250, 252)
            };
            lblSupplierName = new Label { Location = new Point(180, 25), AutoSize = true, Font = new Font("Segoe UI", 18F, FontStyle.Bold), ForeColor = Color.FromArgb(30, 41, 59) };
            lblPhone = new Label { Location = new Point(180, 70), AutoSize = true, Font = new Font("Segoe UI", 11F), ForeColor = Color.FromArgb(71, 85, 105) };
            lblAddress = new Label { Location = new Point(180, 100), Size = new Size(600, 45), Font = new Font("Segoe UI", 11F), ForeColor = Color.FromArgb(71, 85, 105) };
            lblTotalDebt = new Label { Location = new Point(180, 145), AutoSize = true, Font = new Font("Segoe UI", 15F, FontStyle.Bold), ForeColor = Color.FromArgb(220, 38, 38) };

            infoCard.Controls.Add(picSupplier);
            infoCard.Controls.Add(lblSupplierName);
            infoCard.Controls.Add(lblPhone);
            infoCard.Controls.Add(lblAddress);
            infoCard.Controls.Add(lblTotalDebt);
            rightContainer.Controls.Add(infoCard, 0, 0);

            // 2. Orders Grid (Center)
            var ordersCard = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(1), Margin = new Padding(0, 0, 0, 15) };
            dgvOrders = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoGenerateColumns = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowTemplate = { Height = 42 },
                GridColor = Color.FromArgb(230, 233, 237),
                EnableHeadersVisualStyles = false,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None
            };
            dgvOrders.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(71, 85, 105);
            dgvOrders.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvOrders.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvOrders.ColumnHeadersHeight = 45;
            dgvOrders.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);

            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Mã hóa đơn", DataPropertyName = "InvoiceCode", Width = 150 });
            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Ngày nhập", DataPropertyName = "OrderDate", Width = 150, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy HH:mm" } });
            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tổng tiền", DataPropertyName = "TotalAmount", Width = 120, DefaultCellStyle = new DataGridViewCellStyle { Format = "N0", Alignment = DataGridViewContentAlignment.MiddleRight } });
            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Đã trả", DataPropertyName = "PaidAmount", Width = 120, DefaultCellStyle = new DataGridViewCellStyle { Format = "N0", Alignment = DataGridViewContentAlignment.MiddleRight } });
            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Còn nợ", DataPropertyName = "DebtAmount", Width = 120, DefaultCellStyle = new DataGridViewCellStyle { Format = "N0", Alignment = DataGridViewContentAlignment.MiddleRight, ForeColor = Color.Red } });
            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Trạng thái", DataPropertyName = "StatusText", Width = 140 });
            dgvOrders.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            ordersCard.Controls.Add(dgvOrders);
            rightContainer.Controls.Add(ordersCard, 0, 1);

            // 3. Payment Section (Bottom)
            var paymentCard = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(15) };
            var tlpPay = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 2 };
            tlpPay.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
            tlpPay.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
            tlpPay.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpPay.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));

            tlpPay.Controls.Add(new Label { Text = "SỐ TIỀN THANH TOÁN:", Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), AutoSize = true }, 0, 0);
            numPaymentAmount = new NumericUpDown { Width = 170, Font = new Font("Segoe UI", 13F, FontStyle.Bold), Maximum = 1000000000, ThousandsSeparator = true };
            tlpPay.Controls.Add(numPaymentAmount, 0, 1);

            tlpPay.Controls.Add(new Label { Text = "PHƯƠNG THỨC:", Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), AutoSize = true }, 1, 0);
            cboPaymentMethod = new ComboBox { Width = 160, Font = new Font("Segoe UI", 10F), DropDownStyle = ComboBoxStyle.DropDownList };
            cboPaymentMethod.Items.Add(new PaymentMethodItem { Id = 1, Name = "Tiền mặt" });
            cboPaymentMethod.Items.Add(new PaymentMethodItem { Id = 2, Name = "Chuyển khoản" });
            cboPaymentMethod.SelectedIndex = 0;
            tlpPay.Controls.Add(cboPaymentMethod, 1, 1);

            tlpPay.Controls.Add(new Label { Text = "GHI CHÚ:", Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), AutoSize = true }, 2, 0);
            txtPaymentNote = new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10F) };
            tlpPay.Controls.Add(txtPaymentNote, 2, 1);

            btnPay = new Button
            {
                Text = "THANH TOÁN",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnPay.FlatAppearance.BorderSize = 0;
            btnPay.Click += btnPay_Click;
            tlpPay.SetRowSpan(btnPay, 2);
            tlpPay.Controls.Add(btnPay, 3, 0);

            lblError = new Label { ForeColor = Color.Red, Font = new Font("Segoe UI", 9F, FontStyle.Italic), AutoSize = true, Visible = false };
            paymentCard.Controls.Add(tlpPay);
            paymentCard.Controls.Add(lblError);
            
            rightContainer.Controls.Add(paymentCard, 0, 2);

            root.Controls.Add(leftPanel, 0, 0);
            root.Controls.Add(rightContainer, 1, 0);
            Controls.Add(root);
        }

        private Button CreateHeaderButton(string text, Color backColor, Color foreColor)
        {
            return new Button
            {
                Text = text,
                Dock = DockStyle.Fill,
                FlatStyle = FlatStyle.Flat,
                BackColor = backColor,
                ForeColor = foreColor,
                Font = new Font("Segoe UI Semibold", 12F),
                Cursor = Cursors.Hand,
                Margin = new Padding(2)
            };
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            using (var f = new SupplierEditForm())
            {
                if (f.ShowDialog() == DialogResult.OK)
                {
                    LoadSuppliers();
                }
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dgvSuppliers.CurrentRow?.DataBoundItem is SupplierListItem supplier)
            {
                using (var f = new SupplierEditForm(supplier))
                {
                    if (f.ShowDialog() == DialogResult.OK)
                    {
                        LoadSuppliers();
                    }
                }
            }
        }

        private void btnDeleteSupplier_Click(object sender, EventArgs e)
        {
            if (dgvSuppliers.CurrentRow?.DataBoundItem is SupplierListItem supplier)
            {
                if (MessageBox.Show($"Bạn có chắc chắn muốn xóa nhà cung cấp '{supplier.SupplierName}'?", "Xác nhận xóa", 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    try
                    {
                        _controller.DeleteSupplier(supplier.SupplierID);
                        LoadSuppliers();
                        MessageBox.Show("Đã xóa nhà cung cấp thành công.");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi khi xóa: " + ex.Message);
                    }
                }
            }
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
                lblSupplierName.Text = "Không có nhà cung cấp";
                lblPhone.Text = "Điện thoại:";
                lblAddress.Text = "Địa chỉ:";
                lblTotalDebt.Text = "Tổng cộng nợ: 0";
                picSupplier.Image = null;
                return;
            }

            lblSupplierName.Text = supplier.SupplierName;
            lblPhone.Text = "Điện thoại: " + supplier.Phone;
            lblAddress.Text = "Địa chỉ: " + supplier.Address;
            lblTotalDebt.Text = "Tổng cộng nợ: " + supplier.TotalDebt.ToString("N0", CultureInfo.InvariantCulture);

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
                ShowError("Vui lòng chọn nhà cung cấp.");
                return;
            }

            if (order == null)
            {
                ShowError("Vui lòng chọn phiếu nhập cần thanh toán.");
                return;
            }

            decimal amount = numPaymentAmount.Value;
            if (amount <= 0)
            {
                ShowError("Số tiền thanh toán phải lớn hơn 0.");
                return;
            }

            if (amount > order.DebtAmount)
            {
                ShowError("Số tiền thanh toán vượt quá công nợ của phiếu nhập.");
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

                MessageBox.Show("Thanh toán thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
