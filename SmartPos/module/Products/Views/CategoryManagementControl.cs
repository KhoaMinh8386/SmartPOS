using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SmartPos.Module.Products.Controllers;
using SmartPos.Module.Products.Models;

namespace SmartPos.Module.Products.Views
{
    public class CategoryManagementControl : UserControl
    {
        private readonly CategoryService _service;
        private TreeView tvCategories;
        private TextBox txtName, txtDescription;
        private ComboBox cboParent;
        private CheckBox chkActive;
        private Button btnAdd, btnUpdate, btnDelete, btnRefresh;
        private int selectedCategoryId = 0;

        public CategoryManagementControl()
        {
            _service = new CategoryService();
            InitializeComponent();
            RefreshData();
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 9F);

            // Split Container
            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                SplitterDistance = 350,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Left: TreeView
            var pnlLeft = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            var lblTree = new Label { Text = "Sơ đồ danh mục", Dock = DockStyle.Top, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Height = 30 };
            tvCategories = new TreeView { Dock = DockStyle.Fill, BorderStyle = BorderStyle.None, ShowLines = true, ShowPlusMinus = true };
            tvCategories.AfterSelect += TvCategories_AfterSelect;
            
            pnlLeft.Controls.Add(tvCategories);
            pnlLeft.Controls.Add(lblTree);
            split.Panel1.Controls.Add(pnlLeft);

            // Right: Form
            var pnlRight = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            var lblForm = new Label { Text = "Thông tin chi tiết", Dock = DockStyle.Top, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Height = 30 };
            
            var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false };
            
            flow.Controls.Add(new Label { Text = "Tên danh mục (*)", AutoSize = true, Margin = new Padding(0, 10, 0, 0) });
            txtName = new TextBox { Width = 350 }; flow.Controls.Add(txtName);

            flow.Controls.Add(new Label { Text = "Danh mục cha", AutoSize = true, Margin = new Padding(0, 10, 0, 0) });
            cboParent = new ComboBox { Width = 350, DropDownStyle = ComboBoxStyle.DropDownList }; flow.Controls.Add(cboParent);

            flow.Controls.Add(new Label { Text = "Mô tả", AutoSize = true, Margin = new Padding(0, 10, 0, 0) });
            txtDescription = new TextBox { Width = 350, Multiline = true, Height = 60 }; flow.Controls.Add(txtDescription);

            chkActive = new CheckBox { Text = "Đang hoạt động", Checked = true, Margin = new Padding(0, 15, 0, 0) }; flow.Controls.Add(chkActive);

            // Toolbar
            var pnlActions = new FlowLayoutPanel { Height = 50, Margin = new Padding(0, 20, 0, 0), Width = 400 };
            btnAdd = new Button { Text = "Thêm mới", Size = new Size(90, 32), BackColor = Color.FromArgb(76, 175, 80), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnUpdate = new Button { Text = "Cập nhật", Size = new Size(90, 32), BackColor = Color.FromArgb(33, 150, 243), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnDelete = new Button { Text = "Xóa", Size = new Size(80, 32), BackColor = Color.FromArgb(244, 67, 54), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnRefresh = new Button { Text = "Làm mới", Size = new Size(80, 32) };

            btnAdd.Click += BtnAdd_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnDelete.Click += BtnDelete_Click;
            btnRefresh.Click += (s, e) => ResetForm();

            pnlActions.Controls.AddRange(new Control[] { btnAdd, btnUpdate, btnDelete, btnRefresh });
            flow.Controls.Add(pnlActions);

            pnlRight.Controls.Add(flow);
            pnlRight.Controls.Add(lblForm);
            split.Panel2.Controls.Add(pnlRight);

            this.Controls.Add(split);
        }

        private void RefreshData()
        {
            tvCategories.Nodes.Clear();
            var tree = _service.GetCategoryTree();
            PopulateTree(tree, tvCategories.Nodes);
            tvCategories.ExpandAll();

            var flat = _service.GetFlatListForDropdown();
            var comboList = new List<CategoryDTO> { new CategoryDTO { CategoryID = 0, CategoryName = "(Danh mục gốc)" } };
            comboList.AddRange(flat);
            cboParent.DataSource = comboList;
            cboParent.DisplayMember = "CategoryName";
            cboParent.ValueMember = "CategoryID";
        }

        private void PopulateTree(List<CategoryNode> nodes, TreeNodeCollection treeNodes)
        {
            foreach (var node in nodes)
            {
                var tn = new TreeNode(node.Data.CategoryName) { Tag = node.Data };
                if (!node.Data.IsActive) tn.ForeColor = Color.Gray;
                treeNodes.Add(tn);
                if (node.Children.Any()) PopulateTree(node.Children, tn.Nodes);
            }
        }

        private void TvCategories_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node?.Tag is CategoryDTO dto)
            {
                selectedCategoryId = dto.CategoryID;
                txtName.Text = dto.CategoryName;
                txtDescription.Text = dto.Description;
                cboParent.SelectedValue = dto.ParentID ?? 0;
                chkActive.Checked = dto.IsActive;
                btnUpdate.Enabled = true;
                btnDelete.Enabled = true;
            }
        }

        private void ResetForm()
        {
            selectedCategoryId = 0;
            txtName.Clear();
            txtDescription.Clear();
            cboParent.SelectedIndex = 0;
            chkActive.Checked = true;
            btnUpdate.Enabled = false;
            btnDelete.Enabled = false;
            tvCategories.SelectedNode = null;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var dto = new CategoryDTO
            {
                CategoryName = txtName.Text.Trim(),
                ParentID = (int)cboParent.SelectedValue == 0 ? (int?)null : (int)cboParent.SelectedValue,
                Description = txtDescription.Text.Trim(),
                IsActive = chkActive.Checked
            };
            var error = _service.AddCategory(dto);
            if (error == null)
            {
                MessageBox.Show("Thêm danh mục thành công!");
                RefreshData();
                ResetForm();
            }
            else MessageBox.Show(error, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (selectedCategoryId == 0) return;
            var dto = new CategoryDTO
            {
                CategoryID = selectedCategoryId,
                CategoryName = txtName.Text.Trim(),
                ParentID = (int)cboParent.SelectedValue == 0 ? (int?)null : (int)cboParent.SelectedValue,
                Description = txtDescription.Text.Trim(),
                IsActive = chkActive.Checked
            };
            var error = _service.UpdateCategory(dto);
            if (error == null)
            {
                MessageBox.Show("Cập nhật thành công!");
                RefreshData();
            }
            else MessageBox.Show(error, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (selectedCategoryId == 0) return;
            if (MessageBox.Show("Xác nhận xóa danh mục này?", "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                var error = _service.DeleteCategory(selectedCategoryId);
                if (error == null || error.Contains("chuyển sang trạng thái Ngừng hoạt động"))
                {
                    if (error != null) MessageBox.Show(error);
                    RefreshData();
                    ResetForm();
                }
                else MessageBox.Show(error, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
