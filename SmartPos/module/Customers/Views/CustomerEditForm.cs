using System;
using System.Drawing;
using System.Windows.Forms;
using SmartPos.Module.Customers.Controllers;
using SmartPos.Module.Customers.Models;

namespace SmartPos.Module.Customers.Views
{
    public class CustomerEditForm : Form
    {
        private readonly CustomerController _ctrl;
        private readonly int? _customerId;

        private TextBox txtName, txtPhone, txtEmail, txtAddress, txtNote;
        private ComboBox cboGender;
        private DateTimePicker dtpBirthday;
        private CheckBox chkNoBirthday;

        private static readonly Color C_BLUE  = Color.FromArgb(25, 118, 210);
        private static readonly Color C_WHITE = Color.White;

        public CustomerEditForm(int? customerId, CustomerController ctrl)
        {
            _customerId = customerId;
            _ctrl = ctrl;
            BuildUI();
            if (customerId.HasValue) LoadData(customerId.Value);
        }

        private void BuildUI()
        {
            Text = _customerId.HasValue ? "✏ Sửa khách hàng" : "➕ Thêm khách hàng";
            Size = new Size(480, 500);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = MinimizeBox = false;
            BackColor = C_WHITE;
            Font = new Font("Segoe UI", 10F);

            var tbl = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 9,
                Padding = new Padding(18, 14, 18, 10)
            };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            for (int i = 0; i < 8; i++) tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            int row = 0;
            txtName    = AddRow(tbl, "Tên KH *",   new TextBox(), row++);
            txtPhone   = AddRow(tbl, "SĐT *",      new TextBox(), row++);
            txtEmail   = AddRow(tbl, "Email",       new TextBox(), row++);
            txtAddress = AddRow(tbl, "Địa chỉ",    new TextBox(), row++);

            // Gender
            tbl.Controls.Add(Lbl("Giới tính"), 0, row);
            cboGender = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            cboGender.Items.AddRange(new[] { "Nam", "Nữ", "Khác" });
            cboGender.SelectedIndex = 0;
            tbl.Controls.Add(cboGender, 1, row++);

            // Birthday
            tbl.Controls.Add(Lbl("Ngày sinh"), 0, row);
            var bdPanel = new Panel { Dock = DockStyle.Fill };
            dtpBirthday = new DateTimePicker { Location = new Point(0,6), Width = 200, Format = DateTimePickerFormat.Short };
            chkNoBirthday = new CheckBox { Text = "Không xác định", Location = new Point(210, 8), AutoSize = true };
            chkNoBirthday.CheckedChanged += (s,e) => dtpBirthday.Enabled = !chkNoBirthday.Checked;
            chkNoBirthday.Checked = true;
            bdPanel.Controls.AddRange(new Control[] { dtpBirthday, chkNoBirthday });
            tbl.Controls.Add(bdPanel, 1, row++);

            txtNote = AddRow(tbl, "Ghi chú", new TextBox(), row++);

            // Buttons row
            var btnPanel = new Panel { Dock = DockStyle.Fill };
            var btnSave = new Button
            {
                Text = "💾 Lưu", Location = new Point(0, 6), Width = 120, Height = 36,
                BackColor = C_BLUE, ForeColor = C_WHITE, FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;
            var btnCancel = new Button
            {
                Text = "Hủy", Location = new Point(130, 6), Width = 90, Height = 36,
                FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(240,240,240)
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s,e) => { DialogResult = DialogResult.Cancel; Close(); };
            btnPanel.Controls.AddRange(new Control[] { btnSave, btnCancel });
            tbl.Controls.Add(btnPanel, 1, row);

            Controls.Add(tbl);
        }

        private void LoadData(int id)
        {
            var d = _ctrl.GetDetail(id);
            if (d == null) return;
            txtName.Text    = d.FullName;
            txtPhone.Text   = d.Phone ?? "";
            txtEmail.Text   = d.Email ?? "";
            txtAddress.Text = d.Address ?? "";
            txtNote.Text    = d.Note ?? "";
            if (d.Gender != null) cboGender.SelectedItem = d.Gender;
            if (d.DateOfBirth.HasValue)
            {
                chkNoBirthday.Checked = false;
                dtpBirthday.Value = d.DateOfBirth.Value;
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            { MessageBox.Show("Tên khách hàng không được để trống!"); txtName.Focus(); return; }
            if (string.IsNullOrWhiteSpace(txtPhone.Text))
            { MessageBox.Show("Số điện thoại không được để trống!"); txtPhone.Focus(); return; }

            try
            {
                _ctrl.Save(new CustomerSaveRequest
                {
                    CustomerID  = _customerId,
                    FullName    = txtName.Text.Trim(),
                    Phone       = txtPhone.Text.Trim(),
                    Email       = txtEmail.Text.Trim(),
                    Address     = txtAddress.Text.Trim(),
                    Gender      = cboGender.SelectedItem?.ToString(),
                    DateOfBirth = chkNoBirthday.Checked ? (DateTime?)null : dtpBirthday.Value.Date,
                    Note        = txtNote.Text.Trim()
                });
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex) { MessageBox.Show("Lỗi lưu: " + ex.Message); }
        }

        private TextBox AddRow(TableLayoutPanel tbl, string label, TextBox txt, int row)
        {
            txt.Dock = DockStyle.Fill;
            tbl.Controls.Add(Lbl(label), 0, row);
            tbl.Controls.Add(txt, 1, row);
            return txt;
        }

        private Label Lbl(string text) => new Label
        {
            Text = text, Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = Color.FromArgb(80, 80, 80),
            Font = new Font("Segoe UI", 10F)
        };
    }
}
