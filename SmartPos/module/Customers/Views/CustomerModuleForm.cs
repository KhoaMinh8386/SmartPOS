using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SmartPos.Module.Customers.Controllers;
using SmartPos.Module.Customers.Models;

namespace SmartPos.Module.Customers.Views
{
    public class CustomerModuleForm : Form
    {
        private readonly CustomerController _ctrl;

        // Left toolbar
        private TextBox txtSearch;
        private ComboBox cboFilter;
        private DataGridView dgvList;
        private Button btnAdd, btnEdit, btnDelete, btnRefresh;

        // Right detail panel
        private Panel pnlDetail;
        private Label lblCode, lblName, lblPhone, lblEmail, lblType, lblPoints, lblSpent, lblBirthday;
        private TabControl tabDetail;
        private DataGridView dgvPoints, dgvInvoices;

        private List<CustomerListItem> _customers = new List<CustomerListItem>();
        private CustomerListItem _selected;

        // Colors
        private static readonly Color C_BG     = Color.FromArgb(245, 247, 250);
        private static readonly Color C_SIDEBAR = Color.FromArgb(33, 43, 54);
        private static readonly Color C_BLUE   = Color.FromArgb(25, 118, 210);
        private static readonly Color C_GREEN  = Color.FromArgb(27, 94, 32);
        private static readonly Color C_RED    = Color.FromArgb(198, 40, 40);
        private static readonly Color C_GOLD   = Color.FromArgb(255, 160, 0);
        private static readonly Color C_GRAY   = Color.FromArgb(117, 117, 117);
        private static readonly Color C_WHITE  = Color.White;

        public CustomerModuleForm()
        {
            _ctrl = new CustomerController();
            BuildUI();
            LoadList();
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  UI BUILD
        // ═══════════════════════════════════════════════════════════════════════
        private void BuildUI()
        {
            Text = "Quản lý Khách hàng";
            BackColor = C_BG;
            Font = new Font("Segoe UI", 10F);

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            root.Controls.Add(BuildLeft(),  0, 0);
            root.Controls.Add(BuildRight(), 1, 0);
            Controls.Add(root);
        }

        // ── LEFT: toolbar + grid ──────────────────────────────────────────────
        private Control BuildLeft()
        {
            var tbl = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3,
                Padding = new Padding(10), BackColor = C_BG
            };
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            // Row 0 – Toolbar buttons
            var toolbar = new Panel { Dock = DockStyle.Fill };
            btnAdd     = MakeBtn("➕ Thêm",   C_GREEN, 0,   110);
            btnEdit    = MakeBtn("✏ Sửa",    C_BLUE,  115, 100);
            btnDelete  = MakeBtn("🗑 Xóa",   C_RED,   220, 90);
            btnRefresh = MakeBtn("🔄",       C_GRAY,  315, 40);
            btnAdd.Click    += (s,e) => OpenForm(null);
            btnEdit.Click   += (s,e) => { if (_selected != null) OpenForm(_selected.CustomerID); };
            btnDelete.Click += (s,e) => DeleteSelected();
            btnRefresh.Click+= (s,e) => LoadList();
            toolbar.Controls.AddRange(new Control[]{ btnAdd, btnEdit, btnDelete, btnRefresh });
            tbl.Controls.Add(toolbar, 0, 0);

            // Row 1 – Search + Filter
            var searchBar = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1
            };
            searchBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F));
            searchBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            searchBar.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            txtSearch = new TextBox
            {
                Dock = DockStyle.Fill, Font = new Font("Segoe UI", 11F),
            };
            txtSearch.TextChanged += (s,e) => LoadList();

            cboFilter = new ComboBox
            {
                Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F)
            };
            cboFilter.Items.AddRange(new[] { "Tất cả", "Thường", "Thân thiết", "VIP" });
            cboFilter.SelectedIndex = 0;
            cboFilter.SelectedIndexChanged += (s,e) => LoadList();

            searchBar.Controls.Add(txtSearch, 0, 0);
            searchBar.Controls.Add(cboFilter, 1, 0);
            tbl.Controls.Add(searchBar, 0, 1);

            // Row 2 – Grid
            dgvList = new DataGridView
            {
                Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false,
                BackgroundColor = C_WHITE, BorderStyle = BorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowTemplate = { Height = 36 },
                Font = new Font("Segoe UI", 10F),
                GridColor = Color.FromArgb(224, 224, 224),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            dgvList.ColumnHeadersDefaultCellStyle.BackColor = C_SIDEBAR;
            dgvList.ColumnHeadersDefaultCellStyle.ForeColor = C_WHITE;
            dgvList.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvList.EnableHeadersVisualStyles = false;
            dgvList.RowPrePaint += DgvList_RowPrePaint;
            dgvList.SelectionChanged += DgvList_SelectionChanged;

            dgvList.Columns.Add("CustomerID", "ID"); dgvList.Columns["CustomerID"].Visible = false;
            dgvList.Columns.Add("CustomerCode", "Mã KH");   dgvList.Columns["CustomerCode"].FillWeight = 70;
            dgvList.Columns.Add("FullName",     "Tên KH");  dgvList.Columns["FullName"].FillWeight = 150;
            dgvList.Columns.Add("Phone",        "SĐT");     dgvList.Columns["Phone"].FillWeight = 90;
            dgvList.Columns.Add("TotalPoints",  "⭐ Điểm"); dgvList.Columns["TotalPoints"].FillWeight = 70;
            dgvList.Columns.Add("TotalSpent",   "💰 Chi tiêu"); dgvList.Columns["TotalSpent"].DefaultCellStyle.Format = "N0"; dgvList.Columns["TotalSpent"].FillWeight = 90;
            dgvList.Columns.Add("CustomerType", "Hạng");   dgvList.Columns["CustomerType"].FillWeight = 70;

            tbl.Controls.Add(dgvList, 0, 2);
            return tbl;
        }

        // ── RIGHT: detail panel ───────────────────────────────────────────────
        private Control BuildRight()
        {
            pnlDetail = new Panel
            {
                Dock = DockStyle.Fill, BackColor = C_WHITE,
                Padding = new Padding(14)
            };

            var tbl = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3
            };
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 200));
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
            tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            // Info card
            tbl.Controls.Add(BuildInfoCard(), 0, 0);

            // Points action bar
            tbl.Controls.Add(BuildPointsBar(), 0, 1);

            // Tabs
            tabDetail = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10F) };

            var tabPts = new TabPage("⭐ Lịch sử điểm") { BackColor = C_WHITE };
            dgvPoints = MakeSimpleGrid();
            dgvPoints.Columns.Add("Points",      "Điểm");
            dgvPoints.Columns.Add("Type",        "Loại");
            dgvPoints.Columns.Add("Description", "Mô tả");
            dgvPoints.Columns.Add("CreatedAt",   "Thời gian");
            dgvPoints.Columns["CreatedAt"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
            tabPts.Controls.Add(dgvPoints);

            var tabInv = new TabPage("🧾 Lịch sử mua hàng") { BackColor = C_WHITE };
            dgvInvoices = MakeSimpleGrid();
            dgvInvoices.Columns.Add("InvoiceCode", "Mã HĐ");
            dgvInvoices.Columns.Add("InvoiceDate", "Ngày"); dgvInvoices.Columns["InvoiceDate"].DefaultCellStyle.Format = "dd/MM/yyyy";
            dgvInvoices.Columns.Add("TotalAmount", "Tổng tiền"); dgvInvoices.Columns["TotalAmount"].DefaultCellStyle.Format = "N0";
            dgvInvoices.Columns.Add("PaymentMethodText", "TT");
            tabInv.Controls.Add(dgvInvoices);

            tabDetail.TabPages.Add(tabPts);
            tabDetail.TabPages.Add(tabInv);
            tbl.Controls.Add(tabDetail, 0, 2);

            pnlDetail.Controls.Add(tbl);
            return pnlDetail;
        }

        private Control BuildInfoCard()
        {
            var card = new Panel { Dock = DockStyle.Fill, BackColor = C_WHITE };

            // Avatar circle placeholder
            var avatar = new Panel
            {
                Size = new Size(56, 56), Location = new Point(0, 0),
                BackColor = C_BLUE
            };
            var lblAvatar = new Label
            {
                Text = "👤", Font = new Font("Segoe UI", 20F),
                Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = C_WHITE
            };
            avatar.Controls.Add(lblAvatar);
            card.Controls.Add(avatar);

            int x = 68, y = 0;
            lblName  = InfoLabel("-- Chọn khách hàng --", new Font("Segoe UI", 13F, FontStyle.Bold), Color.FromArgb(33,33,33), x, y, 300, 28); y += 30;
            lblCode  = InfoLabel("",  new Font("Segoe UI", 9F),  C_GRAY,  x, y, 200, 20); y += 22;
            lblPhone = InfoLabel("",  new Font("Segoe UI", 10F), Color.FromArgb(33,33,33), x, y, 200, 20); y += 22;
            lblEmail = InfoLabel("",  new Font("Segoe UI", 10F), Color.FromArgb(33,33,33), x, y, 260, 20); y += 22;
            lblBirthday = InfoLabel("",new Font("Segoe UI",9F), C_GRAY, x, y, 200, 20);

            // Stats row
            y = 140;
            lblType   = StatBadge("--", C_GRAY, 0,   y);
            lblPoints = StatBadge("⭐ 0 điểm", C_GOLD, 100, y);
            lblSpent  = StatBadge("💰 0đ",    C_GREEN, 220, y);

            card.Controls.AddRange(new Control[]{ lblName, lblCode, lblPhone, lblEmail, lblBirthday, lblType, lblPoints, lblSpent });
            return card;
        }

        private Control BuildPointsBar()
        {
            var bar = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(245,245,245) };
            var btnAddPts  = MakeBtn("➕ Cộng điểm",  C_GOLD,  0,   130);
            var btnRedPts  = MakeBtn("➖ Trừ điểm",   C_GRAY,  135, 120);
            btnAddPts.Height = btnRedPts.Height = 36;
            btnAddPts.Click += (s,e) => AdjustPoints(true);
            btnRedPts.Click += (s,e) => AdjustPoints(false);
            bar.Controls.AddRange(new Control[]{ btnAddPts, btnRedPts });
            return bar;
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  DATA LOGIC
        // ═══════════════════════════════════════════════════════════════════════
        private void LoadList()
        {
            string search = txtSearch.Text.Trim();
            string filter = cboFilter.SelectedIndex <= 0 ? null : cboFilter.SelectedItem.ToString();
            try
            {
                _customers = _ctrl.GetList(search, filter);
                dgvList.Rows.Clear();
                foreach (var c in _customers)
                {
                    dgvList.Rows.Add(c.CustomerID, c.CustomerCode, c.FullName, c.Phone,
                                     c.TotalPoints, c.TotalSpent, c.CustomerType);
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải danh sách: " + ex.Message); }
        }

        private void DgvList_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvList.CurrentRow == null) return;
            int id = (int)dgvList.CurrentRow.Cells["CustomerID"].Value;
            _selected = _customers.FirstOrDefault(c => c.CustomerID == id);
            if (_selected != null) LoadDetail(_selected.CustomerID);
        }

        private void LoadDetail(int id)
        {
            try
            {
                var d = _ctrl.GetDetail(id);
                if (d == null) return;

                lblName.Text     = d.FullName;
                lblCode.Text     = d.CustomerCode;
                lblPhone.Text    = "📞 " + (d.Phone ?? "–");
                lblEmail.Text    = "✉ "  + (d.Email ?? "–");
                lblBirthday.Text = d.DateOfBirth.HasValue ? "🎂 " + d.DateOfBirth.Value.ToString("dd/MM/yyyy") : "";

                lblType.Text      = d.CustomerType ?? "Thường";
                lblType.BackColor = TypeColor(d.CustomerType);
                lblPoints.Text    = $"⭐ {d.TotalPoints:N0} điểm";
                lblSpent.Text     = $"💰 {d.TotalSpent:N0}đ";

                // Points history
                dgvPoints.Rows.Clear();
                foreach (var p in _ctrl.GetPointsHistory(id))
                {
                    int row = dgvPoints.Rows.Add(p.Points > 0 ? "+" + p.Points : p.Points.ToString(),
                                                 p.Type, p.Description, p.CreatedAt);
                    dgvPoints.Rows[row].DefaultCellStyle.ForeColor = p.Points >= 0 ? C_GREEN : C_RED;
                }

                // Invoice history
                dgvInvoices.Rows.Clear();
                foreach (var inv in _ctrl.GetInvoices(id))
                    dgvInvoices.Rows.Add(inv.InvoiceCode, inv.InvoiceDate, inv.TotalAmount, inv.PaymentMethodText);
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải chi tiết: " + ex.Message); }
        }

        private void OpenForm(int? customerId)
        {
            using (var f = new CustomerEditForm(customerId, _ctrl))
            {
                if (f.ShowDialog() == DialogResult.OK)
                {
                    LoadList();
                    if (_selected != null) LoadDetail(_selected.CustomerID);
                }
            }
        }

        private void DeleteSelected()
        {
            if (_selected == null) return;
            if (MessageBox.Show($"Xóa khách hàng \"{_selected.FullName}\"?",
                "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            try { _ctrl.Delete(_selected.CustomerID); LoadList(); }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        private void AdjustPoints(bool isAdd)
        {
            if (_selected == null) { MessageBox.Show("Chọn khách hàng trước."); return; }
            string title = isAdd ? "Cộng điểm" : "Trừ điểm";
            string input = Microsoft.VisualBasic.Interaction.InputBox(
                "Nhập số điểm:", title, "0");
            if (!int.TryParse(input, out int pts) || pts <= 0) return;

            string desc = Microsoft.VisualBasic.Interaction.InputBox("Ghi chú:", title, "");
            try
            {
                if (isAdd) _ctrl.AddPoints(_selected.CustomerID, pts, desc);
                else       _ctrl.RedeemPoints(_selected.CustomerID, pts, desc);
                LoadDetail(_selected.CustomerID);
                LoadList();
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        // ── Row paint – color by type ─────────────────────────────────────────
        private void DgvList_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dgvList.Rows.Count) return;
            var row = dgvList.Rows[e.RowIndex];
            string type = row.Cells["CustomerType"].Value?.ToString();
            row.DefaultCellStyle.ForeColor = TypeColor(type);
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  HELPERS
        // ═══════════════════════════════════════════════════════════════════════
        private Button MakeBtn(string text, Color bg, int x, int w)
        {
            var b = new Button
            {
                Text = text, Location = new Point(x, 4), Width = w, Height = 36,
                BackColor = bg, ForeColor = C_WHITE, FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        private Label InfoLabel(string text, Font font, Color fore, int x, int y, int w, int h)
            => new Label { Text = text, Font = font, ForeColor = fore,
                           Location = new Point(x, y), Size = new Size(w, h), AutoSize = false };

        private Label StatBadge(string text, Color bg, int x, int y)
            => new Label
            {
                Text = text, Location = new Point(x, y), Size = new Size(110, 28),
                BackColor = bg, ForeColor = C_WHITE,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = false
            };

        private DataGridView MakeSimpleGrid()
        {
            var g = new DataGridView
            {
                Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false,
                BackgroundColor = C_WHITE, BorderStyle = BorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowTemplate = { Height = 32 }, Font = new Font("Segoe UI", 9F)
            };
            g.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(55, 71, 79);
            g.ColumnHeadersDefaultCellStyle.ForeColor = C_WHITE;
            g.EnableHeadersVisualStyles = false;
            return g;
        }

        private Color TypeColor(string type)
        {
            if (type == "VIP")        return C_GOLD;
            if (type == "Thân thiết") return C_BLUE;
            return C_GRAY;
        }
    }
}
