using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SmartPos.Module.XuatHang.Backend;
using SmartPos.Module.XuatHang.Models;

namespace SmartPos.Module.XuatHang.Views
{
    public class frmStockOut : Form
    {
        private readonly StockOutBackend _backend;
        private List<WarehouseLookup> _warehouses;
        private List<ProductInventoryItem> _currentInventory;
        private readonly BindingSource _detailsBinding;
        private List<StockOutDetail> _details;

        private ComboBox cboWarehouse;
        private ComboBox cboReason;
        private DateTimePicker dtpDate;
        private TextBox txtNotes;
        
        private ComboBox cboProduct;
        private ComboBox cboBatch;
        private TextBox txtUnit;
        private NumericUpDown numQuantity;
        private Label lblAvailable;
        
        private DataGridView dgvItems;
        private Button btnAddItem;
        private Button btnSave;
        private Label lblError;

        public frmStockOut()
        {
            _backend = new StockOutBackend();
            _details = new List<StockOutDetail>();
            _detailsBinding = new BindingSource { DataSource = _details };
            
            InitializeUi();
            Load += frmStockOut_Load;
        }

        private void InitializeUi()
        {
            Text = "PHIẾU XUẤT KHO";
            StartPosition = FormStartPosition.CenterParent;
            Width = 1000;
            Height = 700;
            BackColor = Color.FromArgb(245, 247, 250);
            Font = new Font("Segoe UI", 10F);

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 4,
                ColumnCount = 1,
                Padding = new Padding(20)
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 150)); // Header
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 100)); // Input
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // Grid
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));  // Footer

            root.Controls.Add(BuildHeaderPanel(), 0, 0);
            root.Controls.Add(BuildInputPanel(), 0, 1);
            root.Controls.Add(BuildGridPanel(), 0, 2);
            root.Controls.Add(BuildFooterPanel(), 0, 3);

            Controls.Add(root);
        }

        private Control BuildHeaderPanel()
        {
            var panel = new GroupBox { Text = "Thông tin chung", Dock = DockStyle.Fill, Padding = new Padding(10) };
            var tlp = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 2 };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            tlp.Controls.Add(new Label { Text = "Kho xuất:", Anchor = AnchorStyles.Left }, 0, 0);
            cboWarehouse = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            cboWarehouse.SelectedIndexChanged += (s, e) => LoadInventory();
            tlp.Controls.Add(cboWarehouse, 1, 0);

            tlp.Controls.Add(new Label { Text = "Ngày xuất:", Anchor = AnchorStyles.Left }, 2, 0);
            dtpDate = new DateTimePicker { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Short };
            tlp.Controls.Add(dtpDate, 3, 0);

            tlp.Controls.Add(new Label { Text = "Lý do:", Anchor = AnchorStyles.Left }, 0, 1);
            cboReason = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            cboReason.Items.AddRange(new[] { "Hư hỏng", "Hết hạn", "Điều chuyển", "Khác" });
            cboReason.SelectedIndex = 0;
            tlp.Controls.Add(cboReason, 1, 1);

            tlp.Controls.Add(new Label { Text = "Ghi chú:", Anchor = AnchorStyles.Left }, 2, 1);
            txtNotes = new TextBox { Dock = DockStyle.Fill };
            tlp.Controls.Add(txtNotes, 3, 1);

            panel.Controls.Add(tlp);
            return panel;
        }

        private Control BuildInputPanel()
        {
            var panel = new GroupBox { Text = "Chọn sản phẩm", Dock = DockStyle.Fill, Padding = new Padding(10) };
            var tlp = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 6, RowCount = 2 };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35)); // Product
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25)); // Batch
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80)); // Unit
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100)); // Qty
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120)); // Add Btn

            tlp.Controls.Add(new Label { Text = "Sản phẩm", Font = new Font("Segoe UI", 9F, FontStyle.Bold) }, 0, 0);
            tlp.Controls.Add(new Label { Text = "Lô hàng", Font = new Font("Segoe UI", 9F, FontStyle.Bold) }, 1, 0);
            tlp.Controls.Add(new Label { Text = "ĐVT", Font = new Font("Segoe UI", 9F, FontStyle.Bold) }, 2, 0);
            tlp.Controls.Add(new Label { Text = "Số lượng", Font = new Font("Segoe UI", 9F, FontStyle.Bold) }, 3, 0);

            cboProduct = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            cboProduct.SelectedIndexChanged += cboProduct_SelectedIndexChanged;
            tlp.Controls.Add(cboProduct, 0, 1);

            cboBatch = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            cboBatch.SelectedIndexChanged += cboBatch_SelectedIndexChanged;
            tlp.Controls.Add(cboBatch, 1, 1);

            txtUnit = new TextBox { Dock = DockStyle.Fill, ReadOnly = true };
            tlp.Controls.Add(txtUnit, 2, 1);

            numQuantity = new NumericUpDown { Dock = DockStyle.Fill, DecimalPlaces = 2, Maximum = 1000000 };
            tlp.Controls.Add(numQuantity, 3, 1);

            btnAddItem = new Button { Text = "Thêm", Dock = DockStyle.Fill, BackColor = Color.FromArgb(59, 130, 246), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnAddItem.Click += btnAddItem_Click;
            tlp.Controls.Add(btnAddItem, 4, 1);

            lblAvailable = new Label { Text = "Tồn: 0", ForeColor = Color.Gray, AutoSize = true, Font = new Font("Segoe UI", 8F, FontStyle.Italic) };
            tlp.Controls.Add(lblAvailable, 1, 0); // Position it next to "Lô hàng" label or below

            panel.Controls.Add(tlp);
            return panel;
        }

        private Control BuildGridPanel()
        {
            dgvItems = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowTemplate = { Height = 35 }
            };

            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Sản phẩm", DataPropertyName = "ProductName", Width = 250, ReadOnly = true });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Lô hàng", DataPropertyName = "BatchNumber", Width = 150, ReadOnly = true });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "HSD", DataPropertyName = "ExpiryDate", Width = 100, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy" }, ReadOnly = true });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "ĐVT", DataPropertyName = "UnitName", Width = 80, ReadOnly = true });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Số lượng", DataPropertyName = "Quantity", Width = 100, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
            
            var btnDelete = new DataGridViewButtonColumn { Text = "Xóa", UseColumnTextForButtonValue = true, Width = 60 };
            dgvItems.Columns.Add(btnDelete);
            dgvItems.CellContentClick += dgvItems_CellContentClick;

            dgvItems.DataSource = _detailsBinding;
            return dgvItems;
        }

        private Control BuildFooterPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 10, 20, 10) };
            lblError = new Label { Text = "", ForeColor = Color.Red, Dock = DockStyle.Left, AutoSize = true };
            
            btnSave = new Button 
            { 
                Text = "LƯU PHIẾU", 
                Width = 150,
                Dock = DockStyle.Right,
                BackColor = Color.FromArgb(34, 197, 94),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold)
            };
            btnSave.Click += btnSave_Click;

            panel.Controls.Add(lblError);
            panel.Controls.Add(btnSave);
            return panel;
        }

        private void frmStockOut_Load(object sender, EventArgs e)
        {
            try
            {
                _backend.EnsureSchema();
                _warehouses = _backend.GetWarehouses();
                cboWarehouse.DataSource = _warehouses;
                if (_warehouses.Count > 0) cboWarehouse.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }

        private void LoadInventory()
        {
            if (cboWarehouse.SelectedItem is WarehouseLookup selected)
            {
                try
                {
                    _currentInventory = _backend.GetProductInventory(selected.WarehouseID, null);
                    var products = _currentInventory
                        .GroupBy(x => new { x.ProductID, x.ProductName })
                        .Select(g => new { g.Key.ProductID, g.Key.ProductName })
                        .ToList();
                    
                    cboProduct.DisplayMember = "ProductName";
                    cboProduct.ValueMember = "ProductID";
                    cboProduct.DataSource = products;
                }
                catch (Exception ex)
                {
                    lblError.Text = ex.Message;
                }
            }
        }

        private void cboProduct_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboProduct.SelectedValue is int productId)
            {
                var batches = _currentInventory.Where(x => x.ProductID == productId).ToList();
                cboBatch.DataSource = batches;
                cboBatch.DisplayMember = "BatchNumber";
                
                if (batches.Count > 0)
                {
                    txtUnit.Text = batches[0].UnitName;
                    numQuantity.Value = 0;
                }
            }
        }

        private void cboBatch_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboBatch.SelectedItem is ProductInventoryItem batch)
            {
                lblAvailable.Text = $"Tồn: {batch.Quantity:N2}";
                numQuantity.Maximum = batch.Quantity;
            }
        }

        private void btnAddItem_Click(object sender, EventArgs e)
        {
            if (cboBatch.SelectedItem is ProductInventoryItem batch)
            {
                if (numQuantity.Value <= 0)
                {
                    MessageBox.Show("Số lượng phải lớn hơn 0.");
                    return;
                }

                var existing = _details.FirstOrDefault(x => x.ProductID == batch.ProductID && x.BatchNumber == batch.BatchNumber);
                if (existing != null)
                {
                    existing.Quantity += numQuantity.Value;
                }
                else
                {
                    _details.Add(new StockOutDetail
                    {
                        ProductID = batch.ProductID,
                        ProductName = batch.ProductName,
                        BatchNumber = batch.BatchNumber,
                        ExpiryDate = batch.ExpiryDate,
                        UnitName = batch.UnitName,
                        Quantity = numQuantity.Value,
                        AvailableQuantity = batch.Quantity
                    });
                }
                _detailsBinding.ResetBindings(false);
                numQuantity.Value = 0;
            }
        }

        private void dgvItems_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 5 && e.RowIndex >= 0) // Delete column
            {
                _details.RemoveAt(e.RowIndex);
                _detailsBinding.ResetBindings(false);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (_details.Count == 0)
            {
                MessageBox.Show("Vui lòng thêm sản phẩm vào phiếu.");
                return;
            }

            if (cboWarehouse.SelectedItem is WarehouseLookup warehouse)
            {
                try
                {
                    var request = new StockOutRequest
                    {
                        WarehouseID = warehouse.WarehouseID,
                        Reason = cboReason.Text,
                        Notes = txtNotes.Text.Trim(),
                        // CreatedByUserID = ... (get from session)
                        Details = _details.Select(d => new StockOutDetailRequest
                        {
                            ProductID = d.ProductID,
                            BatchNumber = d.BatchNumber,
                            ExpiryDate = d.ExpiryDate,
                            Quantity = d.Quantity
                        }).ToList()
                    };

                    _backend.SaveStockOut(request);
                    MessageBox.Show("Lưu phiếu xuất kho thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _details.Clear();
                    _detailsBinding.ResetBindings(false);
                    txtNotes.Clear();
                    LoadInventory();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi lưu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
