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
        private Panel pnlEditor;
        private TextBox txtName, txtSKU, txtBarcode, txtDescription, txtLocation, txtImageUrl;
        private ComboBox cboCategory, cboSupplier, cboUnit;
        private NumericUpDown numCost, numRetail, numWholesale, numWeight;
        private CheckBox chkActive, chkExpiry;
        private Button btnSave, btnCancel;
        private Label lblEditorTitle;

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
            pnlEditor = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15), Visible = false, BorderStyle = BorderStyle.FixedSingle };
            lblEditorTitle = new Label { Text = "Chi tiết sản phẩm", Font = new Font("Segoe UI", 14F, FontStyle.Bold), Dock = DockStyle.Top, Height = 40 };
            
            var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, AutoScroll = true, WrapContents = false };
            
            flow.Controls.Add(CreateLabel("Tên sản phẩm *"));
            txtName = CreateTextBox(350); flow.Controls.Add(txtName);

            flow.Controls.Add(CreateLabel("Mã sản phẩm (SKU) *"));
            txtSKU = CreateTextBox(170); flow.Controls.Add(txtSKU);

            flow.Controls.Add(CreateLabel("Mã vạch (Barcode)"));
            txtBarcode = CreateTextBox(170); flow.Controls.Add(txtBarcode);

            flow.Controls.Add(CreateLabel("Mô tả"));
            txtDescription = CreateTextBox(350); flow.Controls.Add(txtDescription);

            flow.Controls.Add(CreateLabel("Danh mục"));
            cboCategory = CreateComboBox(250); flow.Controls.Add(cboCategory);

            flow.Controls.Add(CreateLabel("Nhà cung cấp"));
            cboSupplier = CreateComboBox(250); flow.Controls.Add(cboSupplier);

            flow.Controls.Add(CreateLabel("Đơn vị tính"));
            cboUnit = CreateComboBox(150); flow.Controls.Add(cboUnit);

            flow.Controls.Add(CreateLabel("Giá vốn"));
            numCost = CreateNumeric(150); flow.Controls.Add(numCost);

            flow.Controls.Add(CreateLabel("Giá bán lẻ"));
            numRetail = CreateNumeric(150); flow.Controls.Add(numRetail);

            flow.Controls.Add(CreateLabel("Giá bán sỉ"));
            numWholesale = CreateNumeric(150); flow.Controls.Add(numWholesale);

            flow.Controls.Add(CreateLabel("Trọng lượng"));
            numWeight = CreateNumeric(150); flow.Controls.Add(numWeight);

            flow.Controls.Add(CreateLabel("Vị trí kệ"));
            txtLocation = CreateTextBox(250); flow.Controls.Add(txtLocation);

            flow.Controls.Add(CreateLabel("Đường dẫn ảnh"));
            txtImageUrl = CreateTextBox(350); flow.Controls.Add(txtImageUrl);

            chkActive = new CheckBox { Text = "Đang kinh doanh", Checked = true, Margin = new Padding(0, 10, 0, 0) }; flow.Controls.Add(chkActive);
            chkExpiry = new CheckBox { Text = "Có hạn sử dụng" }; flow.Controls.Add(chkExpiry);

            var buttonPanel = new Panel { Height = 50, Margin = new Padding(0, 20, 0, 0), Width = 350 };
            btnSave = new Button { Text = "Lưu", Size = new Size(100, 35), Location = new Point(0, 0), BackColor = Color.FromArgb(33, 150, 243), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnSave.Click += btnSave_Click;
            btnCancel = new Button { Text = "Hủy", Size = new Size(100, 35), Location = new Point(110, 0) };
            btnCancel.Click += (s, e) => pnlEditor.Visible = false;
            btnDelete = new Button { Text = "Xóa", Size = new Size(100, 35), Location = new Point(220, 0), BackColor = Color.Firebrick, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnDelete.Click += btnDelete_Click;
            
            buttonPanel.Controls.AddRange(new Control[] { btnSave, btnCancel, btnDelete });
            flow.Controls.Add(buttonPanel);

            pnlEditor.Controls.Add(flow);
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
                foreach (Control c in pnlEditor.Controls) 
                {
                    if (c is FlowLayoutPanel flp) 
                    {
                        foreach (Control ctrl in flp.Controls) 
                        {
                            // Chỉ cho phép nút Cancel hoạt động
                            if (ctrl.Text != "Cancel") ctrl.Enabled = false;
                        }
                    }
                }
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
                }
            }
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
