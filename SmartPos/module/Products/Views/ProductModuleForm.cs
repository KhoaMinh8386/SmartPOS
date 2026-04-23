using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SmartPos.Module.Products.Controllers;
using SmartPos.Module.Products.Models;

namespace SmartPos.Module.Products.Views
{
    public class ProductModuleForm : Form
    {
        private readonly ProductController _controller;
        private List<ProductListItem> _products;
        private List<CategoryListItem> _categories;
        private List<SupplierLookupItem> _suppliers;
        private List<UnitLookupItem> _units;

        private DataGridView dgvProducts;
        private TextBox txtSearch;
        private ComboBox cboCategoryFilter;
        private Button btnSearch;
        private Button btnAdd;
        private Button btnDelete;
        private Button btnManageCategories;

        // Editor Controls
        private Panel pnlEditor, container;
        private TextBox txtName, txtSKU, txtBarcode, txtDescription, txtLocation, txtImageUrl;
        private ComboBox cboCategory, cboSupplier, cboUnit;
        private NumericUpDown numCost, numRetail, numWholesale, numWeight;
        private CheckBox chkActive, chkExpiry;
        private Button btnSave, btnCancel, btnUpload;
        private Label lblEditorTitle;
        private PictureBox picProduct;

        private int _currentProductId = 0;

        public ProductModuleForm()
        {
            _controller = new ProductController();
            InitializeUi();
            LoadData();
            ApplySecurity();
        }

        private void InitializeUi()
        {
            Text = "Quản lý sản phẩm";
            Width = 1280;
            Height = 800;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.White;

            var mainSplit = new SplitContainer
            {
                Dock = DockStyle.Fill,
                SplitterDistance = 850,
                FixedPanel = FixedPanel.Panel2
            };

            // Left Panel - Grid and Search
            var leftPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            
            var searchPanel = new Panel { Dock = DockStyle.Top, Height = 60 };
            txtSearch = new TextBox { Location = new Point(0, 15), Width = 250, Font = new Font("Segoe UI", 10F) };
            // PlaceholderText is not supported in .NET Framework 4.6.1
            
            cboCategoryFilter = new ComboBox { Location = new Point(260, 15), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            
            btnSearch = new Button { Text = "Tìm kiếm", Location = new Point(420, 12), Size = new Size(80, 32), BackColor = Color.LightGray };
            btnSearch.Click += (s, e) => LoadProducts();

            btnAdd = new Button { Text = "Thêm mới", Location = new Point(510, 12), Size = new Size(100, 32), BackColor = Color.FromArgb(76, 175, 80), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnAdd.Click += (s, e) => ShowEditor(0);

            btnManageCategories = new Button { Text = "Danh mục", Location = new Point(620, 12), Size = new Size(100, 32) };
            btnManageCategories.Click += (s, e) => {
                using (var f = new Form())
                {
                    f.Text = "Quản lý danh mục đa cấp";
                    f.Size = new Size(1000, 700);
                    f.StartPosition = FormStartPosition.CenterParent;
                    var ctrl = new CategoryManagementControl();
                    ctrl.Dock = DockStyle.Fill;
                    f.Controls.Add(ctrl);
                    f.ShowDialog(this);
                }
                LoadData();
            };

            searchPanel.Controls.AddRange(new Control[] { txtSearch, cboCategoryFilter, btnSearch, btnAdd, btnManageCategories });

            dgvProducts = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            dgvProducts.DoubleClick += (s, e) => { 
                if (dgvProducts.CurrentRow != null && UserSession.IsAdmin) 
                    ShowEditor(((ProductListItem)dgvProducts.CurrentRow.DataBoundItem).ProductID); 
                else if (dgvProducts.CurrentRow != null)
                    ShowEditor(((ProductListItem)dgvProducts.CurrentRow.DataBoundItem).ProductID); // Vẫn cho mở để xem detail nhưng Editor sẽ bị disable
            };

            leftPanel.Controls.Add(dgvProducts);
            leftPanel.Controls.Add(searchPanel);

            // Right Panel - Editor
            pnlEditor = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20), Visible = false, BackColor = Color.FromArgb(248, 250, 252), BorderStyle = BorderStyle.FixedSingle };
            lblEditorTitle = new Label { Text = "CHI TIẾT SẢN PHẨM", Font = new Font("Segoe UI", 14F, FontStyle.Bold), Dock = DockStyle.Top, Height = 40, ForeColor = Color.FromArgb(30, 41, 59) };
            
            container = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            
            // Image Preview Card
            var pnlImageCard = new Panel { Width = 360, Height = 220, BackColor = Color.White, Location = new Point(0, 0), BorderStyle = BorderStyle.FixedSingle };
            picProduct = new PictureBox { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.FromArgb(241, 245, 249) };
            btnUpload = new Button { Text = "📷 TẢI ẢNH LÊN CLOUD", Dock = DockStyle.Bottom, Height = 40, BackColor = Color.FromArgb(51, 65, 85), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnUpload.Click += btnUpload_Click;
            pnlImageCard.Controls.Add(picProduct);
            pnlImageCard.Controls.Add(btnUpload);
            container.Controls.Add(pnlImageCard);

            // Inputs
            int startY = 230;
            AddInputPair(container, "Tên sản phẩm *", txtName = CreateTextBox(360), 0, startY);
            AddInputPair(container, "Mã SKU *", txtSKU = CreateTextBox(175), 0, startY + 65);
            AddInputPair(container, "Mã vạch (Barcode)", txtBarcode = CreateTextBox(175), 185, startY + 65);
            
            AddInputPair(container, "Mô tả", txtDescription = CreateTextBox(360), 0, startY + 130);
            
            AddInputPair(container, "Danh mục", cboCategory = CreateComboBox(360), 0, startY + 195);
            AddInputPair(container, "Nhà cung cấp", cboSupplier = CreateComboBox(360), 0, startY + 260);
            
            AddInputPair(container, "Giá vốn", numCost = CreateNumeric(115), 0, startY + 325);
            AddInputPair(container, "Giá lẻ", numRetail = CreateNumeric(115), 122, startY + 325);
            AddInputPair(container, "Giá sỉ", numWholesale = CreateNumeric(115), 244, startY + 325);

            AddInputPair(container, "Đơn vị tính", cboUnit = CreateComboBox(175), 0, startY + 390);
            AddInputPair(container, "Trọng lượng", numWeight = CreateNumeric(175), 185, startY + 390);

            AddInputPair(container, "Vị trí kệ", txtLocation = CreateTextBox(175), 0, startY + 455);
            AddInputPair(container, "URL Hình ảnh", txtImageUrl = CreateTextBox(175), 185, startY + 455);
            txtImageUrl.TextChanged += (s, e) => LoadProductImage(txtImageUrl.Text);

            chkActive = new CheckBox { Text = "Đang kinh doanh", Checked = true, Location = new Point(0, startY + 520), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            chkExpiry = new CheckBox { Text = "Có hạn sử dụng", Location = new Point(150, startY + 520), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            container.Controls.AddRange(new Control[] { chkActive, chkExpiry });

            var buttonPanel = new Panel { Height = 50, Width = 360, Location = new Point(0, startY + 565) };
            btnSave = new Button { Text = "LƯU THÔNG TIN", Size = new Size(115, 45), Location = new Point(0, 0), BackColor = Color.FromArgb(16, 185, 129), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnSave.Click += btnSave_Click;
            
            btnDelete = new Button { Text = "XÓA", Size = new Size(115, 45), Location = new Point(122, 0), BackColor = Color.FromArgb(239, 68, 68), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnDelete.Click += btnDelete_Click;

            btnCancel = new Button { Text = "HỦY", Size = new Size(115, 45), Location = new Point(244, 0), BackColor = Color.FromArgb(226, 232, 240), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnCancel.Click += (s, e) => pnlEditor.Visible = false;
            
            buttonPanel.Controls.AddRange(new Control[] { btnSave, btnDelete, btnCancel });
            container.Controls.Add(buttonPanel);

            pnlEditor.Controls.Add(container);
            pnlEditor.Controls.Add(lblEditorTitle);

            mainSplit.Panel1.Controls.Add(leftPanel);
            mainSplit.Panel2.Controls.Add(pnlEditor);
            Controls.Add(mainSplit);
        }

        private void ApplySecurity()
        {
            bool isAdmin = UserSession.IsAdmin;
            
            btnAdd.Visible = isAdmin;
            btnManageCategories.Visible = isAdmin;
            btnSave.Visible = isAdmin; // Ẩn luôn nút Lưu
            btnDelete.Visible = isAdmin; // Ẩn luôn nút Xóa
            
            if (!isAdmin)
            {
                lblEditorTitle.Text = "Chi tiết sản phẩm (Chế độ Xem)";
                // Vô hiệu hóa toàn bộ panel nhập liệu
                pnlEditor.Enabled = true; // Panel chính mở để thấy thông tin
                foreach (Control c in container.Controls) 
                {
                    if (c is TextBox || c is ComboBox || c is NumericUpDown || c is CheckBox)
                        c.Enabled = false;
                }
                btnUpload.Enabled = false;
            }
        }

        private void LoadData()
        {
            _categories = _controller.GetCategories();
            _suppliers = _controller.GetSuppliers();
            _units = _controller.GetUnits();

            cboCategoryFilter.Items.Clear();
            cboCategoryFilter.Items.Add(new { CategoryID = 0, CategoryName = "Tất cả danh mục" });
            foreach (var c in _categories) cboCategoryFilter.Items.Add(c);
            cboCategoryFilter.DisplayMember = "CategoryName";
            cboCategoryFilter.ValueMember = "CategoryID";
            cboCategoryFilter.SelectedIndex = 0;

            cboCategory.DataSource = new List<CategoryListItem>(_categories);
            cboCategory.DisplayMember = "CategoryName";
            cboCategory.ValueMember = "CategoryID";

            cboSupplier.DataSource = new List<SupplierLookupItem>(_suppliers);
            cboSupplier.DisplayMember = "SupplierName";
            cboSupplier.ValueMember = "SupplierID";

            cboUnit.DataSource = new List<UnitLookupItem>(_units);
            cboUnit.DisplayMember = "UnitName";
            cboUnit.ValueMember = "UnitID";

            LoadProducts();
        }

        private void LoadProducts()
        {
            int catId = 0;
            if (cboCategoryFilter.SelectedItem != null)
            {
                var val = cboCategoryFilter.SelectedItem;
                var prop = val.GetType().GetProperty("CategoryID");
                catId = (int)(prop?.GetValue(val) ?? 0);
            }
            _products = _controller.GetProducts(txtSearch.Text.Trim(), catId);
            dgvProducts.DataSource = _products;
            FormatGrid();
        }

        private void FormatGrid()
        {
            if (dgvProducts.Columns["ProductID"] != null) dgvProducts.Columns["ProductID"].HeaderText = "ID";
            if (dgvProducts.Columns["ProductCode"] != null) dgvProducts.Columns["ProductCode"].HeaderText = "Mã SP";
            if (dgvProducts.Columns["ProductName"] != null) dgvProducts.Columns["ProductName"].HeaderText = "Tên sản phẩm";
            if (dgvProducts.Columns["CategoryName"] != null) dgvProducts.Columns["CategoryName"].HeaderText = "Danh mục";
            if (dgvProducts.Columns["RetailPrice"] != null) dgvProducts.Columns["RetailPrice"].HeaderText = "Giá bán";
            if (dgvProducts.Columns["Location"] != null) dgvProducts.Columns["Location"].HeaderText = "Vị trí";
            if (dgvProducts.Columns["StockQuantity"] != null) dgvProducts.Columns["StockQuantity"].HeaderText = "Tồn kho";
            if (dgvProducts.Columns["IsActive"] != null) dgvProducts.Columns["IsActive"].HeaderText = "Hoạt động";

            if (dgvProducts.Columns["RetailPrice"] != null) dgvProducts.Columns["RetailPrice"].DefaultCellStyle.Format = "N0";
            if (dgvProducts.Columns["StockQuantity"] != null) dgvProducts.Columns["StockQuantity"].DefaultCellStyle.Format = "N2";
        }

        private void ShowEditor(int productId)
        {
            _currentProductId = productId;
            pnlEditor.Visible = true;
            btnDelete.Visible = productId > 0 && UserSession.IsAdmin;

            if (productId == 0)
            {
                lblEditorTitle.Text = "Thêm sản phẩm mới";
                txtName.Clear(); txtSKU.Clear(); txtBarcode.Clear(); txtDescription.Clear(); txtLocation.Clear(); txtImageUrl.Clear();
                numCost.Value = 0; numRetail.Value = 0; numWholesale.Value = 0; numWeight.Value = 0;
                chkActive.Checked = true; chkExpiry.Checked = false;
            }
            else
            {
                lblEditorTitle.Text = "Sửa sản phẩm";
                var p = _controller.GetProductDetail(productId);
                if (p != null)
                {
                    txtName.Text = p.ProductName;
                    txtSKU.Text = p.ProductCode;
                    txtBarcode.Text = p.Barcode;
                    txtDescription.Text = p.Description;
                    cboCategory.SelectedValue = p.CategoryID;
                    cboSupplier.SelectedValue = p.SupplierID ?? 0;
                    cboUnit.SelectedValue = p.BaseUnitID;
                    numCost.Value = p.CostPrice;
                    numRetail.Value = p.RetailPrice;
                    numWholesale.Value = p.WholesalePrice ?? 0;
                    numWeight.Value = p.Weight ?? 0;
                    txtLocation.Text = p.Location;
                    txtImageUrl.Text = p.ImageUrl;
                    chkActive.Checked = p.IsActive;
                    chkExpiry.Checked = p.HasExpiry;
                    LoadProductImage(p.ImageUrl);
                }
            }
        }

        private void LoadProductImage(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                picProduct.Image = null;
                return;
            }
            try { picProduct.LoadAsync(url); } catch { picProduct.Image = null; }
        }

        private async void btnUpload_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog { Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif" })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        btnUpload.Text = "⏳ ĐANG TẢI LÊN...";
                        btnUpload.Enabled = false;
                        var service = new Common.Services.CloudinaryService();
                        var url = await service.UploadImageAsync(ofd.FileName);
                        txtImageUrl.Text = url;
                        LoadProductImage(url);
                        MessageBox.Show("Tải ảnh lên thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Lỗi upload", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        btnUpload.Text = "📷 TẢI ẢNH LÊN CLOUD";
                        btnUpload.Enabled = true;
                    }
                }
            }
        }

        private void AddInputPair(Control parent, string labelText, Control inputControl, int x, int y)
        {
            var lbl = CreateLabel(labelText);
            lbl.Location = new Point(x, y);
            inputControl.Location = new Point(x, y + 25);
            parent.Controls.Add(lbl);
            parent.Controls.Add(inputControl);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                var product = new ProductDetail
                {
                    ProductID = _currentProductId,
                    ProductName = txtName.Text.Trim(),
                    ProductCode = txtSKU.Text.Trim(),
                    Barcode = txtBarcode.Text.Trim(),
                    CategoryID = (int)cboCategory.SelectedValue,
                    SupplierID = (int)cboSupplier.SelectedValue == 0 ? (int?)null : (int)cboSupplier.SelectedValue,
                    BaseUnitID = (int)cboUnit.SelectedValue,
                    CostPrice = numCost.Value,
                    RetailPrice = numRetail.Value,
                    WholesalePrice = numWholesale.Value == 0 ? (decimal?)null : numWholesale.Value,
                    Location = txtLocation.Text.Trim(),
                    ImageUrl = txtImageUrl.Text.Trim(),
                    IsActive = chkActive.Checked,
                    HasExpiry = chkExpiry.Checked,
                    Weight = numWeight.Value == 0 ? (decimal?)null : numWeight.Value
                };

                _controller.SaveProduct(product);
                MessageBox.Show("Lưu sản phẩm thành công.");
                pnlEditor.Visible = false;
                LoadProducts();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Bạn có chắc chắn muốn xóa sản phẩm này không?", "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                _controller.DeleteProduct(_currentProductId);
                pnlEditor.Visible = false;
                LoadProducts();
            }
        }

        // Helper methods for UI creation
        private Label CreateLabel(string text) => new Label { Text = text, AutoSize = true, Margin = new Padding(0, 10, 0, 0), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
        private TextBox CreateTextBox(int width) => new TextBox { Width = width, Font = new Font("Segoe UI", 10F) };
        private ComboBox CreateComboBox(int width) => new ComboBox { Width = width, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10F) };
        private NumericUpDown CreateNumeric(int width) => new NumericUpDown { Width = width, Maximum = 1000000000, ThousandsSeparator = true, Font = new Font("Segoe UI", 10F) };
    }
}
