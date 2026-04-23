using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using SmartPos.Module.Promotions.Controllers;
using SmartPos.Module.Promotions.Models;

namespace SmartPos.Module.Promotions.Views
{
    public class PromotionModuleForm : Form
    {
        private readonly PromotionController _controller;
        private PromotionDataBundle _data;

        // Vouchers
        private DataGridView dgvVouchers;
        private TextBox txtVoucherCode;
        private TextBox txtVoucherDesc;
        private ComboBox cboVoucherType;
        private NumericUpDown numVoucherValue;
        private NumericUpDown numVoucherMinOrder;
        private NumericUpDown numVoucherMaxDiscount;
        private CheckBox chkVoucherAllowStack;
        private NumericUpDown numVoucherPriority;
        private CheckBox chkVoucherActive;
        private DateTimePicker dtVoucherStart;
        private DateTimePicker dtVoucherEnd;
        private Button btnSaveVoucher;
        private Button btnNewVoucher;
        private Button btnDeleteVoucher;

        // Product Sales
        private DataGridView dgvSales;
        private ComboBox cboSaleProduct;
        private TextBox txtSaleName;
        private ComboBox cboSaleType;
        private NumericUpDown numSaleValue;
        private NumericUpDown numSalePrice;
        private CheckBox chkSaleAllowStack;
        private NumericUpDown numSalePriority;
        private CheckBox chkSaleActive;
        private DateTimePicker dtSaleStart;
        private DateTimePicker dtSaleEnd;
        private Button btnSaveSale;
        private Button btnNewSale;
        private Button btnDeleteSale;

        // Preview
        private ComboBox cboPreviewVoucher;
        private ComboBox cboPreviewSale;
        private NumericUpDown numPreviewOrderAmount;
        private NumericUpDown numPreviewProductAmount;
        private Button btnPreview;
        private Label lblPreviewResult;
        private Label lblError;

        public PromotionModuleForm()
        {
            _controller = new PromotionController();
            InitializeUi();
            Load += PromotionModuleForm_Load;
        }

        private void PromotionModuleForm_Load(object sender, EventArgs e)
        {
            ReloadData();
        }

        private void InitializeUi()
        {
            Text = "Quản lý Khuyến mãi - Voucher & Sale (SmartPOS)";
            StartPosition = FormStartPosition.CenterParent;
            Width = 1500;
            Height = 900;
            BackColor = Color.WhiteSmoke;

            var splitMain = new SplitContainer
            {
                Dock = DockStyle.Fill,
                SplitterDistance = 1000,
                FixedPanel = FixedPanel.Panel2
            };

            splitMain.Panel1.Controls.Add(BuildManagementPanel());
            splitMain.Panel2.Controls.Add(BuildPreviewPanel());

            lblError = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 34,
                ForeColor = Color.Firebrick,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Visible = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0)
            };

            Controls.Add(splitMain);
            Controls.Add(lblError);
        }

        private Control BuildManagementPanel()
        {
            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 420
            };

            split.Panel1.Controls.Add(BuildVoucherPanel());
            split.Panel2.Controls.Add(BuildSalePanel());
            return split;
        }

        private Control BuildVoucherPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10), BackColor = Color.White };
            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 40 };
            var title = new Label { Text = "Quản lý Voucher (Mã giảm giá)", Dock = DockStyle.Left, AutoSize = true, Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = Color.DarkBlue };
            
            btnNewVoucher = new Button { Text = "Thêm mới", Dock = DockStyle.Right, Width = 100, BackColor = Color.LightGreen, FlatStyle = FlatStyle.Flat };
            btnNewVoucher.Click += btnNewVoucher_Click;
            
            btnDeleteVoucher = new Button { Text = "Xóa", Dock = DockStyle.Right, Width = 80, BackColor = Color.LightCoral, FlatStyle = FlatStyle.Flat };
            btnDeleteVoucher.Click += btnDeleteVoucher_Click;

            pnlTop.Controls.Add(btnDeleteVoucher);
            pnlTop.Controls.Add(btnNewVoucher);
            pnlTop.Controls.Add(title);

            dgvVouchers = new DataGridView
            {
                Dock = DockStyle.Top,
                Height = 180,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            dgvVouchers.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "ID", DataPropertyName = "VoucherID", Width = 50 });
            dgvVouchers.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Mã", DataPropertyName = "VoucherCode", Width = 100 });
            dgvVouchers.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Mô tả", DataPropertyName = "Description", Width = 200 });
            dgvVouchers.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Giá trị", DataPropertyName = "DiscountValue", Width = 80, DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" } });
            dgvVouchers.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "Stack", DataPropertyName = "AllowStackDiscount", Width = 60 });
            dgvVouchers.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Priority", DataPropertyName = "Priority", Width = 70 });
            dgvVouchers.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "Active", DataPropertyName = "IsActive", Width = 60 });
            dgvVouchers.SelectionChanged += dgvVouchers_SelectionChanged;

            var pnlForm = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 10, 0, 0) };

            txtVoucherCode = new TextBox { Location = new Point(80, 10), Width = 120 };
            txtVoucherDesc = new TextBox { Location = new Point(280, 10), Width = 250 };
            cboVoucherType = new ComboBox { Location = new Point(620, 10), Width = 95, DropDownStyle = ComboBoxStyle.DropDownList };
            cboVoucherType.Items.Add(new DiscountTypeItem { Id = 1, Name = "%" });
            cboVoucherType.Items.Add(new DiscountTypeItem { Id = 2, Name = "Số tiền" });
            cboVoucherType.SelectedIndex = 0;

            numVoucherValue = new NumericUpDown { Location = new Point(800, 10), Width = 120, Maximum = 1000000000, DecimalPlaces = 0, ThousandsSeparator = true };
            
            numVoucherMinOrder = new NumericUpDown { Location = new Point(80, 46), Width = 120, Maximum = 1000000000, DecimalPlaces = 0, ThousandsSeparator = true };
            numVoucherMaxDiscount = new NumericUpDown { Location = new Point(280, 46), Width = 120, Maximum = 1000000000, DecimalPlaces = 0, ThousandsSeparator = true };
            chkVoucherAllowStack = new CheckBox { Text = "Allow Stack (Dùng chung Sale)", Location = new Point(450, 47), Width = 200 };
            
            numVoucherPriority = new NumericUpDown { Location = new Point(80, 82), Width = 120, Minimum = 1, Maximum = 999, Value = 100 };
            chkVoucherActive = new CheckBox { Text = "Kích hoạt", Location = new Point(280, 83), Width = 100, Checked = true };
            
            dtVoucherStart = new DateTimePicker { Location = new Point(450, 82), Width = 160, Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy HH:mm" };
            dtVoucherEnd = new DateTimePicker { Location = new Point(700, 82), Width = 160, Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy HH:mm" };
            
            btnSaveVoucher = new Button { Text = "Lưu Voucher", Location = new Point(800, 115), Size = new Size(120, 34), Font = new Font("Segoe UI", 9F, FontStyle.Bold), BackColor = Color.LightSkyBlue, FlatStyle = FlatStyle.Flat };
            btnSaveVoucher.Click += btnSaveVoucher_Click;

            pnlForm.Controls.Add(new Label { Text = "Mã (*)", Location = new Point(10, 13), AutoSize = true });
            pnlForm.Controls.Add(txtVoucherCode);
            pnlForm.Controls.Add(new Label { Text = "Mô tả", Location = new Point(220, 13), AutoSize = true });
            pnlForm.Controls.Add(txtVoucherDesc);
            pnlForm.Controls.Add(new Label { Text = "Loại", Location = new Point(570, 13), AutoSize = true });
            pnlForm.Controls.Add(cboVoucherType);
            pnlForm.Controls.Add(new Label { Text = "Giá trị (*)", Location = new Point(740, 13), AutoSize = true });
            pnlForm.Controls.Add(numVoucherValue);
            
            pnlForm.Controls.Add(new Label { Text = "Min đơn", Location = new Point(10, 49), AutoSize = true });
            pnlForm.Controls.Add(numVoucherMinOrder);
            pnlForm.Controls.Add(new Label { Text = "Max giảm", Location = new Point(220, 49), AutoSize = true });
            pnlForm.Controls.Add(numVoucherMaxDiscount);
            pnlForm.Controls.Add(chkVoucherAllowStack);
            
            pnlForm.Controls.Add(new Label { Text = "Priority", Location = new Point(10, 85), AutoSize = true });
            pnlForm.Controls.Add(numVoucherPriority);
            pnlForm.Controls.Add(chkVoucherActive);
            pnlForm.Controls.Add(new Label { Text = "Từ ngày", Location = new Point(390, 85), AutoSize = true });
            pnlForm.Controls.Add(dtVoucherStart);
            pnlForm.Controls.Add(new Label { Text = "Đến ngày", Location = new Point(630, 85), AutoSize = true });
            pnlForm.Controls.Add(dtVoucherEnd);
            
            pnlForm.Controls.Add(btnSaveVoucher);

            panel.Controls.Add(pnlForm);
            panel.Controls.Add(dgvVouchers);
            panel.Controls.Add(pnlTop);

            return panel;
        }

        private Control BuildSalePanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10), BackColor = Color.White };
            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 40 };
            var title = new Label { Text = "Quản lý Sale Sản phẩm", Dock = DockStyle.Left, AutoSize = true, Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = Color.DarkOrange };
            
            btnNewSale = new Button { Text = "Thêm mới", Dock = DockStyle.Right, Width = 100, BackColor = Color.LightGreen, FlatStyle = FlatStyle.Flat };
            btnNewSale.Click += btnNewSale_Click;
            
            btnDeleteSale = new Button { Text = "Xóa", Dock = DockStyle.Right, Width = 80, BackColor = Color.LightCoral, FlatStyle = FlatStyle.Flat };
            btnDeleteSale.Click += btnDeleteSale_Click;

            pnlTop.Controls.Add(btnDeleteSale);
            pnlTop.Controls.Add(btnNewSale);
            pnlTop.Controls.Add(title);

            dgvSales = new DataGridView
            {
                Dock = DockStyle.Top,
                Height = 180,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            dgvSales.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "ID", DataPropertyName = "SaleID", Width = 50 });
            dgvSales.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Sản phẩm", DataPropertyName = "ProductCode", Width = 90 });
            dgvSales.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tên sale", DataPropertyName = "SaleName", Width = 200 });
            dgvSales.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Giảm", DataPropertyName = "DiscountValue", Width = 80, DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" } });
            dgvSales.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Giá Fix", DataPropertyName = "SalePrice", Width = 80, DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" } });
            dgvSales.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "Stack V", DataPropertyName = "AllowStackVoucher", Width = 60 });
            dgvSales.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Priority", DataPropertyName = "Priority", Width = 70 });
            dgvSales.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "Active", DataPropertyName = "IsActive", Width = 60 });
            dgvSales.SelectionChanged += dgvSales_SelectionChanged;

            var pnlForm = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 10, 0, 0) };

            cboSaleProduct = new ComboBox { Location = new Point(80, 10), Width = 260, DropDownStyle = ComboBoxStyle.DropDownList };
            txtSaleName = new TextBox { Location = new Point(440, 10), Width = 200 };
            cboSaleType = new ComboBox { Location = new Point(740, 10), Width = 100, DropDownStyle = ComboBoxStyle.DropDownList };
            cboSaleType.Items.Add(new DiscountTypeItem { Id = 1, Name = "%" });
            cboSaleType.Items.Add(new DiscountTypeItem { Id = 2, Name = "Số tiền" });
            cboSaleType.SelectedIndex = 0;

            numSaleValue = new NumericUpDown { Location = new Point(80, 46), Width = 120, Maximum = 1000000000, DecimalPlaces = 0, ThousandsSeparator = true };
            numSalePrice = new NumericUpDown { Location = new Point(300, 46), Width = 120, Maximum = 1000000000, DecimalPlaces = 0, ThousandsSeparator = true };
            chkSaleAllowStack = new CheckBox { Text = "Allow Stack (Dùng chung Voucher)", Location = new Point(440, 47), Width = 250 };
            
            numSalePriority = new NumericUpDown { Location = new Point(80, 82), Width = 120, Minimum = 1, Maximum = 999, Value = 100 };
            chkSaleActive = new CheckBox { Text = "Kích hoạt", Location = new Point(230, 83), Width = 100, Checked = true };
            
            dtSaleStart = new DateTimePicker { Location = new Point(440, 82), Width = 160, Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy HH:mm" };
            dtSaleEnd = new DateTimePicker { Location = new Point(680, 82), Width = 160, Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy HH:mm" };
            
            btnSaveSale = new Button { Text = "Lưu Sale", Location = new Point(800, 115), Size = new Size(120, 34), Font = new Font("Segoe UI", 9F, FontStyle.Bold), BackColor = Color.LightSkyBlue, FlatStyle = FlatStyle.Flat };
            btnSaveSale.Click += btnSaveSale_Click;

            pnlForm.Controls.Add(new Label { Text = "Sản phẩm", Location = new Point(10, 13), AutoSize = true });
            pnlForm.Controls.Add(cboSaleProduct);
            pnlForm.Controls.Add(new Label { Text = "Tên Sale", Location = new Point(370, 13), AutoSize = true });
            pnlForm.Controls.Add(txtSaleName);
            pnlForm.Controls.Add(new Label { Text = "Loại", Location = new Point(690, 13), AutoSize = true });
            pnlForm.Controls.Add(cboSaleType);
            
            pnlForm.Controls.Add(new Label { Text = "Giá trị giảm", Location = new Point(10, 49), AutoSize = true });
            pnlForm.Controls.Add(numSaleValue);
            pnlForm.Controls.Add(new Label { Text = "Giá Fix(SalePrice)", Location = new Point(205, 49), AutoSize = true });
            pnlForm.Controls.Add(numSalePrice);
            pnlForm.Controls.Add(chkSaleAllowStack);
            
            pnlForm.Controls.Add(new Label { Text = "Priority", Location = new Point(10, 85), AutoSize = true });
            pnlForm.Controls.Add(numSalePriority);
            pnlForm.Controls.Add(chkSaleActive);
            pnlForm.Controls.Add(new Label { Text = "Từ ngày", Location = new Point(380, 85), AutoSize = true });
            pnlForm.Controls.Add(dtSaleStart);
            pnlForm.Controls.Add(new Label { Text = "Đến ngày", Location = new Point(610, 85), AutoSize = true });
            pnlForm.Controls.Add(dtSaleEnd);
            
            pnlForm.Controls.Add(btnSaveSale);

            panel.Controls.Add(pnlForm);
            panel.Controls.Add(dgvSales);
            panel.Controls.Add(pnlTop);

            return panel;
        }

        private Control BuildPreviewPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15), BackColor = Color.FromArgb(240, 248, 255) }; // AliceBlue
            var title = new Label { Text = "Mô phỏng Thanh toán", Dock = DockStyle.Top, Height = 40, Font = new Font("Segoe UI", 14F, FontStyle.Bold), ForeColor = Color.DarkSlateGray };

            var pnlInputs = new Panel { Dock = DockStyle.Top, Height = 180 };
            
            numPreviewOrderAmount = new NumericUpDown { Location = new Point(160, 10), Width = 200, Maximum = 1000000000, DecimalPlaces = 0, ThousandsSeparator = true, Value = 200000, Font = new Font("Segoe UI", 10F) };
            numPreviewProductAmount = new NumericUpDown { Location = new Point(160, 50), Width = 200, Maximum = 1000000000, DecimalPlaces = 0, ThousandsSeparator = true, Value = 50000, Font = new Font("Segoe UI", 10F) };
            
            cboPreviewVoucher = new ComboBox { Location = new Point(160, 90), Width = 300, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10F) };
            cboPreviewSale = new ComboBox { Location = new Point(160, 130), Width = 300, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10F) };
            
            btnPreview = new Button { Text = "Tính toán khuyến mãi", Location = new Point(160, 180), Size = new Size(200, 40), Font = new Font("Segoe UI", 10F, FontStyle.Bold), BackColor = Color.MediumSeaGreen, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnPreview.Click += btnPreview_Click;

            pnlInputs.Controls.Add(new Label { Text = "Tổng hóa đơn", Location = new Point(10, 13), AutoSize = true, Font = new Font("Segoe UI", 10F) });
            pnlInputs.Controls.Add(numPreviewOrderAmount);
            pnlInputs.Controls.Add(new Label { Text = "Giá trị SP đang Sale", Location = new Point(10, 53), AutoSize = true, Font = new Font("Segoe UI", 10F) });
            pnlInputs.Controls.Add(numPreviewProductAmount);
            pnlInputs.Controls.Add(new Label { Text = "Áp Voucher", Location = new Point(10, 93), AutoSize = true, Font = new Font("Segoe UI", 10F) });
            pnlInputs.Controls.Add(cboPreviewVoucher);
            pnlInputs.Controls.Add(new Label { Text = "Sản phẩm Sale", Location = new Point(10, 133), AutoSize = true, Font = new Font("Segoe UI", 10F) });
            pnlInputs.Controls.Add(cboPreviewSale);

            lblPreviewResult = new Label
            {
                Location = new Point(10, 240),
                Size = new Size(450, 450),
                Font = new Font("Segoe UI", 10F),
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(15),
                Text = "Nhấn 'Tính toán khuyến mãi' để xem kết quả.",
                BackColor = Color.White
            };

            panel.Controls.Add(lblPreviewResult);
            panel.Controls.Add(btnPreview);
            panel.Controls.Add(pnlInputs);
            panel.Controls.Add(title);

            return panel;
        }

        private void ReloadData()
        {
            try
            {
                HideError();
                _data = _controller.LoadData();

                dgvVouchers.DataSource = _data.Vouchers;
                dgvSales.DataSource = _data.ProductSales;
                
                cboSaleProduct.DataSource = _data.Products;

                // Load preview comboboxes (add null option)
                var vouchersForPreview = new List<VoucherItem>();
                vouchersForPreview.Add(new VoucherItem { VoucherID = 0, VoucherCode = "-- Không chọn --" });
                vouchersForPreview.AddRange(_data.Vouchers);
                cboPreviewVoucher.DataSource = vouchersForPreview;
                cboPreviewVoucher.DisplayMember = "VoucherCode";

                var salesForPreview = new List<ProductSaleItem>();
                salesForPreview.Add(new ProductSaleItem { SaleID = 0, SaleName = "-- Không chọn --" });
                salesForPreview.AddRange(_data.ProductSales);
                cboPreviewSale.DataSource = salesForPreview;
                cboPreviewSale.DisplayMember = "SaleName";

                ResetVoucherForm();
                ResetSaleForm();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        // ─────────────────────────────────────────────
        //  Voucher Actions
        // ─────────────────────────────────────────────
        private void btnNewVoucher_Click(object sender, EventArgs e)
        {
            dgvVouchers.ClearSelection();
            ResetVoucherForm();
        }

        private void ResetVoucherForm()
        {
            txtVoucherCode.Clear();
            txtVoucherDesc.Clear();
            cboVoucherType.SelectedIndex = 0;
            numVoucherValue.Value = 0;
            numVoucherMinOrder.Value = 0;
            numVoucherMaxDiscount.Value = 0;
            chkVoucherAllowStack.Checked = false;
            numVoucherPriority.Value = 100;
            chkVoucherActive.Checked = true;
            dtVoucherStart.Value = DateTime.Now;
            dtVoucherEnd.Value = DateTime.Now.AddDays(7);
            btnSaveVoucher.Text = "Thêm Voucher";
            txtVoucherCode.Focus();
        }

        private void dgvVouchers_SelectionChanged(object sender, EventArgs e)
        {
            var selected = dgvVouchers.CurrentRow?.DataBoundItem as VoucherItem;
            if (selected == null) return;

            txtVoucherCode.Text = selected.VoucherCode;
            txtVoucherDesc.Text = selected.Description;
            cboVoucherType.SelectedIndex = selected.DiscountType == 2 ? 1 : 0;
            numVoucherValue.Value = selected.DiscountValue;
            numVoucherMinOrder.Value = selected.MinOrderValue;
            numVoucherMaxDiscount.Value = selected.MaxDiscount ?? 0;
            chkVoucherAllowStack.Checked = selected.AllowStackDiscount;
            numVoucherPriority.Value = selected.Priority > 0 ? selected.Priority : 1;
            chkVoucherActive.Checked = selected.IsActive;
            dtVoucherStart.Value = selected.StartDate;
            dtVoucherEnd.Value = selected.EndDate;
            
            btnSaveVoucher.Text = "Cập nhật Voucher";
        }

        private void btnSaveVoucher_Click(object sender, EventArgs e)
        {
            try
            {
                HideError();
                var selected = dgvVouchers.CurrentRow?.DataBoundItem as VoucherItem;

                var voucher = new VoucherItem
                {
                    VoucherID = selected?.VoucherID ?? 0,
                    VoucherCode = txtVoucherCode.Text.Trim(),
                    Description = txtVoucherDesc.Text.Trim(),
                    DiscountType = ((DiscountTypeItem)cboVoucherType.SelectedItem).Id,
                    DiscountValue = numVoucherValue.Value,
                    MinOrderValue = numVoucherMinOrder.Value,
                    MaxDiscount = numVoucherMaxDiscount.Value <= 0 ? (decimal?)null : numVoucherMaxDiscount.Value,
                    AllowStackDiscount = chkVoucherAllowStack.Checked,
                    Priority = Convert.ToInt32(numVoucherPriority.Value),
                    IsActive = chkVoucherActive.Checked,
                    StartDate = dtVoucherStart.Value,
                    EndDate = dtVoucherEnd.Value
                };

                _controller.SaveVoucher(voucher);
                ReloadData();
                MessageBox.Show("Lưu voucher thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private void btnDeleteVoucher_Click(object sender, EventArgs e)
        {
            try
            {
                HideError();
                var selected = dgvVouchers.CurrentRow?.DataBoundItem as VoucherItem;
                if (selected == null)
                {
                    ShowError("Vui lòng chọn voucher để xóa.");
                    return;
                }

                if (MessageBox.Show($"Bạn có chắc muốn xóa voucher '{selected.VoucherCode}'?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    _controller.DeleteVoucher(selected.VoucherID);
                    ReloadData();
                }
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        // ─────────────────────────────────────────────
        //  Sale Actions
        // ─────────────────────────────────────────────
        private void btnNewSale_Click(object sender, EventArgs e)
        {
            dgvSales.ClearSelection();
            ResetSaleForm();
        }

        private void ResetSaleForm()
        {
            if (cboSaleProduct.Items.Count > 0) cboSaleProduct.SelectedIndex = 0;
            txtSaleName.Clear();
            cboSaleType.SelectedIndex = 0;
            numSaleValue.Value = 0;
            numSalePrice.Value = 0;
            chkSaleAllowStack.Checked = false;
            numSalePriority.Value = 100;
            chkSaleActive.Checked = true;
            dtSaleStart.Value = DateTime.Now;
            dtSaleEnd.Value = DateTime.Now.AddDays(7);
            btnSaveSale.Text = "Thêm Sale";
        }

        private void dgvSales_SelectionChanged(object sender, EventArgs e)
        {
            var selected = dgvSales.CurrentRow?.DataBoundItem as ProductSaleItem;
            if (selected == null) return;

            for (int i = 0; i < cboSaleProduct.Items.Count; i++)
            {
                if (((ProductOption)cboSaleProduct.Items[i]).ProductID == selected.ProductID)
                {
                    cboSaleProduct.SelectedIndex = i;
                    break;
                }
            }

            txtSaleName.Text = selected.SaleName;
            cboSaleType.SelectedIndex = selected.DiscountType == 2 ? 1 : 0;
            numSaleValue.Value = selected.DiscountValue;
            numSalePrice.Value = selected.SalePrice ?? 0;
            chkSaleAllowStack.Checked = selected.AllowStackVoucher;
            numSalePriority.Value = selected.Priority > 0 ? selected.Priority : 1;
            chkSaleActive.Checked = selected.IsActive;
            dtSaleStart.Value = selected.StartDate;
            dtSaleEnd.Value = selected.EndDate;
            
            btnSaveSale.Text = "Cập nhật Sale";
        }

        private void btnSaveSale_Click(object sender, EventArgs e)
        {
            try
            {
                HideError();
                var selected = dgvSales.CurrentRow?.DataBoundItem as ProductSaleItem;
                var product = cboSaleProduct.SelectedItem as ProductOption;
                
                if (product == null)
                {
                    ShowError("Vui lòng chọn sản phẩm.");
                    return;
                }

                var sale = new ProductSaleItem
                {
                    SaleID = selected?.SaleID ?? 0,
                    ProductID = product.ProductID,
                    SaleName = txtSaleName.Text.Trim(),
                    DiscountType = ((DiscountTypeItem)cboSaleType.SelectedItem).Id,
                    DiscountValue = numSaleValue.Value,
                    SalePrice = numSalePrice.Value <= 0 ? (decimal?)null : numSalePrice.Value,
                    AllowStackVoucher = chkSaleAllowStack.Checked,
                    Priority = Convert.ToInt32(numSalePriority.Value),
                    IsActive = chkSaleActive.Checked,
                    StartDate = dtSaleStart.Value,
                    EndDate = dtSaleEnd.Value
                };

                _controller.SaveProductSale(sale);
                ReloadData();
                MessageBox.Show("Lưu chương trình sale thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private void btnDeleteSale_Click(object sender, EventArgs e)
        {
            try
            {
                HideError();
                var selected = dgvSales.CurrentRow?.DataBoundItem as ProductSaleItem;
                if (selected == null)
                {
                    ShowError("Vui lòng chọn chương trình sale để xóa.");
                    return;
                }

                if (MessageBox.Show($"Bạn có chắc muốn xóa sale '{selected.SaleName}'?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    _controller.DeleteProductSale(selected.SaleID);
                    ReloadData();
                }
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        // ─────────────────────────────────────────────
        //  Preview Actions
        // ─────────────────────────────────────────────
        private void btnPreview_Click(object sender, EventArgs e)
        {
            try
            {
                HideError();
                var request = new PromotionPreviewRequest
                {
                    OrderAmount = numPreviewOrderAmount.Value,
                    ProductAmount = numPreviewProductAmount.Value,
                    Voucher = (cboPreviewVoucher.SelectedItem as VoucherItem)?.VoucherID > 0 ? cboPreviewVoucher.SelectedItem as VoucherItem : null,
                    ProductSale = (cboPreviewSale.SelectedItem as ProductSaleItem)?.SaleID > 0 ? cboPreviewSale.SelectedItem as ProductSaleItem : null
                };

                PromotionPreviewResult result = _controller.Preview(request);

                // Màu sắc trực quan
                if (result.StackMode)
                    lblPreviewResult.BackColor = Color.Honeydew; // Xanh nhạt
                else if (request.Voucher != null && request.ProductSale != null && !result.StackMode)
                    lblPreviewResult.BackColor = Color.Cornsilk; // Vàng nhạt (xung đột, chỉ chọn 1)
                else
                    lblPreviewResult.BackColor = Color.White;

                lblPreviewResult.Text =
                    "====== HÓA ĐƠN ======\n"
                    + $"Tổng tiền hàng: {request.OrderAmount:N0}\n\n"
                    + "====== KHUYẾN MÃI ======\n"
                    + $"Giảm từ Sale: -{result.SaleDiscount:N0}\n"
                    + $"Giảm từ Voucher: -{result.VoucherDiscount:N0}\n"
                    + $"Tổng giảm: -{result.TotalDiscount:N0}\n\n"
                    + "====== THÀNH TIỀN ======\n"
                    + $"PHẢI THANH TOÁN: {result.FinalAmount:N0}\n\n"
                    + "====== LOGIC ÁP DỤNG ======\n"
                    + $"{result.AppliedRule}\n\n"
                    + "====== GIẢI THÍCH ======\n"
                    + $"{result.PriorityExplanation}";
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private void ShowError(string message)
        {
            lblError.Text = " Lỗi: " + message;
            lblError.Visible = true;
        }

        private void HideError()
        {
            lblError.Visible = false;
        }

        private class DiscountTypeItem
        {
            public byte Id { get; set; }
            public string Name { get; set; }
            public override string ToString() => Name;
        }
    }
}
