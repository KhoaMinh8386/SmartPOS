using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using SmartPos.Module.Products.Controllers;
using SmartPos.Module.Products.Models;

namespace SmartPos.Module.Products.Views
{
    public class CategoryModuleForm : Form
    {
        private readonly ProductController _controller;
        private List<CategoryListItem> _categories;
        private DataGridView dgvCategories;
        private TextBox txtName, txtDescription;
        private CheckBox chkActive;
        private Button btnSave, btnAdd;
        private int _currentCategoryId = 0;

        public CategoryModuleForm()
        {
            _controller = new ProductController();
            InitializeUi();
            LoadCategories();
        }

        private void InitializeUi()
        {
            Text = "Category Management";
            Width = 600;
            Height = 500;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.White;

            var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 250 };
            
            dgvCategories = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            dgvCategories.SelectionChanged += (s, e) => {
                if (dgvCategories.CurrentRow != null)
                {
                    var cat = (CategoryListItem)dgvCategories.CurrentRow.DataBoundItem;
                    _currentCategoryId = cat.CategoryID;
                    txtName.Text = cat.CategoryName;
                    txtDescription.Text = cat.Description;
                    chkActive.Checked = cat.IsActive;
                }
            };

            var editPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            txtName = new TextBox { Location = new Point(120, 20), Width = 250 };
            txtDescription = new TextBox { Location = new Point(120, 50), Width = 250, Multiline = true, Height = 60 };
            chkActive = new CheckBox { Text = "Is Active", Location = new Point(120, 120), Checked = true };
            
            btnSave = new Button { Text = "Save", Location = new Point(120, 150), Width = 80 };
            btnSave.Click += btnSave_Click;

            btnAdd = new Button { Text = "New", Location = new Point(210, 150), Width = 80 };
            btnAdd.Click += (s, e) => { _currentCategoryId = 0; txtName.Clear(); txtDescription.Clear(); chkActive.Checked = true; };

            editPanel.Controls.AddRange(new Control[] { 
                new Label { Text = "Name", Location = new Point(20, 23) }, txtName,
                new Label { Text = "Description", Location = new Point(20, 53) }, txtDescription,
                chkActive, btnSave, btnAdd
            });

            split.Panel1.Controls.Add(dgvCategories);
            split.Panel2.Controls.Add(editPanel);
            Controls.Add(split);
        }

        private void LoadCategories()
        {
            _categories = _controller.GetCategories();
            dgvCategories.DataSource = _categories;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                var cat = new CategoryListItem { CategoryID = _currentCategoryId, CategoryName = txtName.Text.Trim(), Description = txtDescription.Text.Trim(), IsActive = chkActive.Checked };
                _controller.SaveCategory(cat);
                LoadCategories();
                MessageBox.Show("Category saved.");
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
    }
}
