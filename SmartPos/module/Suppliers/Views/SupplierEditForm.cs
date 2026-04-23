using System;
using System.Drawing;
using System.Windows.Forms;
using SmartPos.Module.Suppliers.Controllers;
using SmartPos.Module.Suppliers.Models;
using SmartPos.Module.Common.Services;

namespace SmartPos.Module.Suppliers.Views
{
    public class SupplierEditForm : Form
    {
        private readonly SupplierController _controller;
        private readonly CloudinaryService _cloudinaryService;
        private readonly SupplierListItem _supplier;
        
        private TextBox txtName, txtPhone, txtAddress, txtImageUrl;
        private PictureBox picSupplier;
        private Button btnSave, btnCancel, btnUpload;

        public SupplierEditForm(SupplierListItem supplier = null)
        {
            _controller = new SupplierController();
            _cloudinaryService = new CloudinaryService();
            _supplier = supplier ?? new SupplierListItem { SupplierID = 0 };
            
            InitializeUi();
            if (supplier != null) LoadData();
        }

        private void InitializeUi()
        {
            Text = _supplier.SupplierID == 0 ? "Thêm nhà cung cấp mới" : "Sửa thông tin nhà cung cấp";
            Size = new Size(500, 600);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 10F);

            var pnlMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            // Image Preview
            picSupplier = new PictureBox
            {
                Size = new Size(150, 150),
                Location = new Point(175, 20),
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(248, 250, 252)
            };
            btnUpload = new Button
            {
                Text = "📷 TẢI ẢNH LÊN CLOUD",
                Size = new Size(200, 35),
                Location = new Point(150, 180),
                BackColor = Color.FromArgb(51, 65, 85),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnUpload.Click += btnUpload_Click;

            int startY = 240;
            AddInput(pnlMain, "Tên nhà cung cấp *", txtName = new TextBox { Width = 440 }, startY);
            AddInput(pnlMain, "Số điện thoại", txtPhone = new TextBox { Width = 440 }, startY + 65);
            AddInput(pnlMain, "Địa chỉ", txtAddress = new TextBox { Width = 440, Multiline = true, Height = 60 }, startY + 130);
            AddInput(pnlMain, "URL Hình ảnh (hoặc upload)", txtImageUrl = new TextBox { Width = 440 }, startY + 220);
            txtImageUrl.TextChanged += (s, e) => {
                if (!string.IsNullOrWhiteSpace(txtImageUrl.Text))
                    try { picSupplier.LoadAsync(txtImageUrl.Text); } catch { picSupplier.Image = null; }
            };

            btnSave = new Button
            {
                Text = "LƯU THÔNG TIN",
                Size = new Size(215, 45),
                Location = new Point(20, startY + 290),
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSave.Click += btnSave_Click;

            btnCancel = new Button
            {
                Text = "HỦY",
                Size = new Size(215, 45),
                Location = new Point(245, startY + 290),
                BackColor = Color.FromArgb(226, 232, 240),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCancel.Click += (s, e) => Close();

            pnlMain.Controls.AddRange(new Control[] { picSupplier, btnUpload, btnSave, btnCancel });
            Controls.Add(pnlMain);
        }

        private void AddInput(Panel p, string labelText, Control input, int y)
        {
            var lbl = new Label { Text = labelText, Location = new Point(20, y), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            input.Location = new Point(20, y + 25);
            p.Controls.Add(lbl);
            p.Controls.Add(input);
        }

        private void LoadData()
        {
            txtName.Text = _supplier.SupplierName;
            txtPhone.Text = _supplier.Phone;
            txtAddress.Text = _supplier.Address;
            txtImageUrl.Text = _supplier.ImageUrl;
            if (!string.IsNullOrWhiteSpace(_supplier.ImageUrl))
                try { picSupplier.LoadAsync(_supplier.ImageUrl); } catch { }
        }

        private async void btnUpload_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog { Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif" })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        btnUpload.Text = "⏳ ĐANG TẢI...";
                        btnUpload.Enabled = false;
                        var url = await _cloudinaryService.UploadImageAsync(ofd.FileName);
                        txtImageUrl.Text = url;
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

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Vui lòng nhập tên nhà cung cấp.");
                return;
            }

            try
            {
                _supplier.SupplierName = txtName.Text.Trim();
                _supplier.Phone = txtPhone.Text.Trim();
                _supplier.Address = txtAddress.Text.Trim();
                _supplier.ImageUrl = txtImageUrl.Text.Trim();

                _controller.SaveSupplier(_supplier);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
