using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using SmartPos.Module.InventoryAudit.Controllers;
using SmartPos.Module.InventoryAudit.Models;

namespace SmartPos.Module.InventoryAudit.Views
{
    public class InventoryAuditModuleForm : Form
    {
        private readonly InventoryAuditController _controller;

        private ComboBox cboWarehouse;
        private Button btnRefresh;
        private Button btnCreateCheck;
        private Button btnOpenCheck;
        private Button btnSaveCheck;
        private Button btnApprove;

        private Label lblError;
        private Label lblGuide;

        private Label lblCheckCode;
        private Label lblCheckStatus;
        private Label lblCheckDate;
        private Label lblCreatedBy;
        private Label lblApprovedBy;
        private Label lblNotes;

        private DataGridView dgvChecks;
        private DataGridView dgvStock;
        private DataGridView dgvCheckItems;
        private DataGridView dgvHistories;

        private int _currentCheckId;
        private bool _isCurrentCheckApproved;
        private bool _savedAfterLastEdit;

        private BindingList<InventoryCheckItemEdit> _checkItems;

        public InventoryAuditModuleForm()
        {
            _controller = new InventoryAuditController();
            InitializeUi();
            Load += InventoryAuditModuleForm_Load;
        }

        private void InventoryAuditModuleForm_Load(object sender, EventArgs e)
        {
            try
            {
                List<WarehouseOption> warehouses = _controller.GetWarehouses();
                cboWarehouse.DataSource = warehouses;
                if (warehouses.Count > 0)
                {
                    LoadWarehouseData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể tải dữ liệu kho.\n\n" + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeUi()
        {
            Text = "Kho + Tồn kho + Kiểm kê";
            StartPosition = FormStartPosition.CenterParent;
            Width = 1450;
            Height = 860;
            BackColor = Color.WhiteSmoke;

            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 136,
                Padding = new Padding(12),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblWarehouse = new Label { Text = "Kho:", Location = new Point(10, 17), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            cboWarehouse = new ComboBox { Location = new Point(48, 12), Width = 280, DropDownStyle = ComboBoxStyle.DropDownList };
            cboWarehouse.SelectedIndexChanged += cboWarehouse_SelectedIndexChanged;

            btnRefresh = new Button { Text = "Tải dữ liệu", Location = new Point(340, 11), Size = new Size(100, 30), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            btnRefresh.Click += btnRefresh_Click;

            btnCreateCheck = new Button { Text = "Tạo phiếu", Location = new Point(450, 11), Size = new Size(95, 30), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            btnCreateCheck.Click += btnCreateCheck_Click;

            btnOpenCheck = new Button { Text = "Mở phiếu", Location = new Point(555, 11), Size = new Size(95, 30), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            btnOpenCheck.Click += btnOpenCheck_Click;

            btnSaveCheck = new Button { Text = "Lưu phiếu", Location = new Point(660, 11), Size = new Size(95, 30), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            btnSaveCheck.Click += btnSaveCheck_Click;

            btnApprove = new Button
            {
                Text = "Duyệt phiếu",
                Location = new Point(765, 11),
                Size = new Size(100, 30),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(25, 118, 210),
                ForeColor = Color.White
            };
            btnApprove.Click += btnApprove_Click;

            lblGuide = new Label
            {
                Location = new Point(10, 52),
                Size = new Size(1400, 38),
                Font = new Font("Segoe UI", 9F),
                Text = "Quy trình chuẩn: 1) Chọn kho và tạo/mở phiếu. 2) Nhập SL thực tế + lý do chênh lệch. 3) Bấm Lưu phiếu. 4) Bấm Duyệt phiếu.\n"
                     + "Hệ thống chỉ cập nhật tồn kho khi duyệt. Mọi lần sửa SL đều được lưu lịch sử (người sửa, thời gian, trước/sau).",
                ForeColor = Color.FromArgb(45, 45, 45)
            };

            lblError = new Label
            {
                Location = new Point(10, 94),
                Size = new Size(1400, 32),
                ForeColor = Color.Firebrick,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Visible = false
            };

            topPanel.Controls.Add(lblWarehouse);
            topPanel.Controls.Add(cboWarehouse);
            topPanel.Controls.Add(btnRefresh);
            topPanel.Controls.Add(btnCreateCheck);
            topPanel.Controls.Add(btnOpenCheck);
            topPanel.Controls.Add(btnSaveCheck);
            topPanel.Controls.Add(btnApprove);
            topPanel.Controls.Add(lblGuide);
            topPanel.Controls.Add(lblError);

            var body = new SplitContainer
            {
                Dock = DockStyle.Fill,
                SplitterDistance = 500
            };

            body.Panel1.Controls.Add(BuildChecksPanel());
            body.Panel2.Controls.Add(BuildRightPanel());

            Controls.Add(body);
            Controls.Add(topPanel);
        }

        private Control BuildChecksPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10), BackColor = Color.White };
            var title = new Label
            {
                Text = "Danh sách phiếu kiểm kê",
                Dock = DockStyle.Top,
                Height = 28,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };

            dgvChecks = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoGenerateColumns = false
            };

            dgvChecks.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Mã phiếu", DataPropertyName = "CheckCode", Width = 130 });
            dgvChecks.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Ngày kiểm", DataPropertyName = "CheckDate", Width = 120, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy HH:mm" } });
            dgvChecks.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Trạng thái", DataPropertyName = "StatusText", Width = 95 });
            dgvChecks.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Người tạo", DataPropertyName = "CreatedByName", Width = 120 });
            dgvChecks.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Người duyệt", DataPropertyName = "ApprovedByName", Width = 120 });
            dgvChecks.SelectionChanged += dgvChecks_SelectionChanged;

            panel.Controls.Add(dgvChecks);
            panel.Controls.Add(title);
            return panel;
        }

        private Control BuildRightPanel()
        {
            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 460
            };

            split.Panel1.Controls.Add(BuildDetailPanel());
            split.Panel2.Controls.Add(BuildHistoryPanel());
            return split;
        }

        private Control BuildDetailPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10), BackColor = Color.White };

            var headerBox = new GroupBox
            {
                Dock = DockStyle.Top,
                Height = 120,
                Text = "Thông tin phiếu kiểm",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            lblCheckCode = new Label { Location = new Point(14, 28), AutoSize = true, Font = new Font("Segoe UI", 9F) };
            lblCheckStatus = new Label { Location = new Point(280, 28), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.DarkGreen };
            lblCheckDate = new Label { Location = new Point(14, 53), AutoSize = true, Font = new Font("Segoe UI", 9F) };
            lblCreatedBy = new Label { Location = new Point(280, 53), AutoSize = true, Font = new Font("Segoe UI", 9F) };
            lblApprovedBy = new Label { Location = new Point(14, 78), AutoSize = true, Font = new Font("Segoe UI", 9F) };
            lblNotes = new Label { Location = new Point(280, 78), Size = new Size(600, 20), Font = new Font("Segoe UI", 9F) };

            headerBox.Controls.Add(lblCheckCode);
            headerBox.Controls.Add(lblCheckStatus);
            headerBox.Controls.Add(lblCheckDate);
            headerBox.Controls.Add(lblCreatedBy);
            headerBox.Controls.Add(lblApprovedBy);
            headerBox.Controls.Add(lblNotes);

            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 180
            };

            dgvStock = BuildStockGrid();
            dgvCheckItems = BuildCheckItemsGrid();

            split.Panel1.Controls.Add(dgvStock);
            split.Panel2.Controls.Add(dgvCheckItems);

            panel.Controls.Add(split);
            panel.Controls.Add(headerBox);
            return panel;
        }

        private Control BuildHistoryPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10), BackColor = Color.White };
            var title = new Label
            {
                Text = "Lịch sử đã kiểm kê và sửa số lượng",
                Dock = DockStyle.Top,
                Height = 28,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };

            dgvHistories = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoGenerateColumns = false
            };

            dgvHistories.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Thời gian", DataPropertyName = "ChangedAt", Width = 130, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy HH:mm:ss" } });
            dgvHistories.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Người sửa", DataPropertyName = "ChangedByName", Width = 110 });
            dgvHistories.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Sản phẩm", DataPropertyName = "ProductName", Width = 170 });
            dgvHistories.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Batch", DataPropertyName = "BatchNumber", Width = 100 });
            dgvHistories.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "SL cũ", DataPropertyName = "OldActualQuantity", Width = 80, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
            dgvHistories.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "SL mới", DataPropertyName = "NewActualQuantity", Width = 80, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
            dgvHistories.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Lý do", DataPropertyName = "NewReason", Width = 260 });

            panel.Controls.Add(dgvHistories);
            panel.Controls.Add(title);
            return panel;
        }

        private DataGridView BuildStockGrid()
        {
            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoGenerateColumns = false
            };

            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Mã SP", DataPropertyName = "ProductCode", Width = 90 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Sản phẩm", DataPropertyName = "ProductName", Width = 170 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Batch", DataPropertyName = "BatchNumber", Width = 120 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "HSD", DataPropertyName = "ExpiryDate", Width = 90, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy" } });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "SL hệ thống", DataPropertyName = "SystemQuantity", Width = 100, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });

            return grid;
        }

        private DataGridView BuildCheckItemsGrid()
        {
            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoGenerateColumns = false
            };

            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Mã SP", DataPropertyName = "ProductCode", Width = 90, ReadOnly = true });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Sản phẩm", DataPropertyName = "ProductName", Width = 170, ReadOnly = true });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Batch", DataPropertyName = "BatchNumber", Width = 110, ReadOnly = true });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "SL hệ thống", DataPropertyName = "SystemQuantity", Width = 95, ReadOnly = true, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "SL thực tế", DataPropertyName = "ActualQuantity", Width = 95 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Chênh lệch", DataPropertyName = "Difference", Width = 95, ReadOnly = true, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Lý do", DataPropertyName = "Reason", Width = 230 });
            grid.CellEndEdit += dgvCheckItems_CellEndEdit;

            return grid;
        }

        private void cboWarehouse_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                LoadWarehouseData();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                LoadWarehouseData();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private void btnCreateCheck_Click(object sender, EventArgs e)
        {
            HideError();

            var selectedWarehouse = cboWarehouse.SelectedItem as WarehouseOption;
            if (selectedWarehouse == null)
            {
                ShowError("Vui lòng chọn kho trước khi tạo phiếu.");
                return;
            }

            if (UserSession.CurrentUser == null)
            {
                ShowError("Không tìm thấy thông tin người dùng đăng nhập.");
                return;
            }

            try
            {
                InventoryCheckDraft draft = _controller.CreateCheckDraft(selectedWarehouse.WarehouseID, UserSession.CurrentUser.UserID, string.Empty);
                LoadWarehouseData();
                SelectCheckInGrid(draft.CheckID);
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private void btnOpenCheck_Click(object sender, EventArgs e)
        {
            HideError();
            OpenSelectedCheck();
        }

        private void btnSaveCheck_Click(object sender, EventArgs e)
        {
            HideError();

            if (_currentCheckId <= 0 || _checkItems == null || _checkItems.Count == 0)
            {
                ShowError("Chưa có phiếu kiểm kê để lưu.");
                return;
            }

            if (_isCurrentCheckApproved)
            {
                ShowError("Phiếu đã duyệt chỉ được xem, không thể lưu chỉnh sửa.");
                return;
            }

            if (UserSession.CurrentUser == null)
            {
                ShowError("Không tìm thấy thông tin người dùng đăng nhập.");
                return;
            }

            try
            {
                _controller.SaveCheckDraftItems(_currentCheckId, new List<InventoryCheckItemEdit>(_checkItems), UserSession.CurrentUser.UserID);
                _savedAfterLastEdit = true;
                LoadCheckHistories(_currentCheckId);
                MessageBox.Show("Lưu phiếu kiểm kê thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private void btnApprove_Click(object sender, EventArgs e)
        {
            HideError();

            if (_currentCheckId <= 0 || _checkItems == null || _checkItems.Count == 0)
            {
                ShowError("Chưa có phiếu kiểm kê để duyệt.");
                return;
            }

            if (_isCurrentCheckApproved)
            {
                ShowError("Phiếu đã duyệt chỉ được xem, không thể duyệt lại.");
                return;
            }

            if (!_savedAfterLastEdit)
            {
                ShowError("Vui lòng bấm 'Lưu phiếu' trước khi duyệt.");
                return;
            }

            if (UserSession.CurrentUser == null)
            {
                ShowError("Không tìm thấy thông tin người dùng đăng nhập.");
                return;
            }

            try
            {
                var request = new ApproveInventoryCheckRequest
                {
                    CheckID = _currentCheckId,
                    ApprovedByUserID = UserSession.CurrentUser.UserID,
                    Items = new List<InventoryCheckItemEdit>(_checkItems)
                };

                _controller.ApproveCheck(request);
                MessageBox.Show("Duyệt kiểm kê thành công. Tồn kho hệ thống đã được cập nhật.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadWarehouseData();
                SelectCheckInGrid(request.CheckID);
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private void dgvChecks_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvChecks.Focused)
            {
                OpenSelectedCheck();
            }
        }

        private void OpenSelectedCheck()
        {
            if (dgvChecks.CurrentRow == null)
            {
                return;
            }

            var summary = dgvChecks.CurrentRow.DataBoundItem as InventoryCheckSummary;
            if (summary == null)
            {
                return;
            }

            try
            {
                LoadCheck(summary.CheckID);
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private void LoadCheck(int checkId)
        {
            InventoryCheckHeader header = _controller.GetCheckHeader(checkId);
            List<InventoryCheckItemEdit> items = _controller.GetCheckItems(checkId);

            _currentCheckId = checkId;
            _isCurrentCheckApproved = header.Status != 1;
            _savedAfterLastEdit = false;

            _checkItems = new BindingList<InventoryCheckItemEdit>(items);
            dgvCheckItems.DataSource = _checkItems;

            lblCheckCode.Text = "Mã phiếu: " + header.CheckCode;
            lblCheckStatus.Text = "Trạng thái: " + header.StatusText;
            lblCheckDate.Text = "Ngày kiểm: " + header.CheckDate.ToString("dd/MM/yyyy HH:mm");
            lblCreatedBy.Text = "Người tạo: " + header.CreatedByName;
            lblApprovedBy.Text = "Người duyệt: " + (string.IsNullOrWhiteSpace(header.ApprovedByName) ? "-" : header.ApprovedByName);
            lblNotes.Text = "Ghi chú: " + (string.IsNullOrWhiteSpace(header.Notes) ? "-" : header.Notes);

            SetCheckEditMode(!_isCurrentCheckApproved);
            LoadCheckHistories(checkId);
        }

        private void LoadWarehouseData()
        {
            var selectedWarehouse = cboWarehouse.SelectedItem as WarehouseOption;
            if (selectedWarehouse == null)
            {
                return;
            }

            dgvStock.DataSource = _controller.GetStockByWarehouse(selectedWarehouse.WarehouseID);
            dgvChecks.DataSource = _controller.GetChecksByWarehouse(selectedWarehouse.WarehouseID);

            if (dgvChecks.Rows.Count == 0)
            {
                ClearCheckDetail();
                return;
            }

            if (dgvChecks.CurrentRow == null)
            {
                dgvChecks.Rows[0].Selected = true;
                dgvChecks.CurrentCell = dgvChecks.Rows[0].Cells[0];
            }

            OpenSelectedCheck();
        }

        private void LoadCheckHistories(int checkId)
        {
            dgvHistories.DataSource = _controller.GetCheckItemHistories(checkId);
        }

        private void dgvCheckItems_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (_checkItems == null || e.RowIndex < 0 || e.RowIndex >= _checkItems.Count)
            {
                return;
            }

            InventoryCheckItemEdit item = _checkItems[e.RowIndex];
            if (item.ActualQuantity.HasValue && item.ActualQuantity.Value < 0)
            {
                item.ActualQuantity = 0;
            }

            _savedAfterLastEdit = false;
            dgvCheckItems.Refresh();
        }

        private void SetCheckEditMode(bool editable)
        {
            if (dgvCheckItems.Columns.Count >= 7)
            {
                dgvCheckItems.ReadOnly = !editable;
                dgvCheckItems.Columns[0].ReadOnly = true;
                dgvCheckItems.Columns[1].ReadOnly = true;
                dgvCheckItems.Columns[2].ReadOnly = true;
                dgvCheckItems.Columns[3].ReadOnly = true;
                dgvCheckItems.Columns[5].ReadOnly = true;
            }

            btnSaveCheck.Enabled = editable;
            btnApprove.Enabled = editable;

            if (!editable)
            {
                btnApprove.Text = "Phiếu đã duyệt";
            }
            else
            {
                btnApprove.Text = "Duyệt phiếu";
            }
        }

        private void SelectCheckInGrid(int checkId)
        {
            for (int i = 0; i < dgvChecks.Rows.Count; i++)
            {
                var summary = dgvChecks.Rows[i].DataBoundItem as InventoryCheckSummary;
                if (summary != null && summary.CheckID == checkId)
                {
                    dgvChecks.ClearSelection();
                    dgvChecks.Rows[i].Selected = true;
                    dgvChecks.CurrentCell = dgvChecks.Rows[i].Cells[0];
                    LoadCheck(checkId);
                    return;
                }
            }
        }

        private void ClearCheckDetail()
        {
            _currentCheckId = 0;
            _isCurrentCheckApproved = false;
            _savedAfterLastEdit = false;
            _checkItems = null;

            lblCheckCode.Text = "Mã phiếu: -";
            lblCheckStatus.Text = "Trạng thái: -";
            lblCheckDate.Text = "Ngày kiểm: -";
            lblCreatedBy.Text = "Người tạo: -";
            lblApprovedBy.Text = "Người duyệt: -";
            lblNotes.Text = "Ghi chú: -";

            dgvCheckItems.DataSource = null;
            dgvHistories.DataSource = null;
            SetCheckEditMode(false);
        }

        private void ShowError(string message)
        {
            lblError.Text = message;
            lblError.Visible = true;
        }

        private void HideError()
        {
            lblError.Visible = false;
        }
    }
}
