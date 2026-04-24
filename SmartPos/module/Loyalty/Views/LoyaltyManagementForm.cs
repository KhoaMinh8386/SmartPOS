using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using SmartPos.Module.Loyalty.Controllers;
using SmartPos.Module.Loyalty.Models;
using System.ComponentModel;

namespace SmartPos.Module.Loyalty.Views
{
    public partial class LoyaltyManagementForm : Form
    {
        private readonly LoyaltyController _controller;
        private BindingList<LoyaltyCustomerListItem> _thanThietList;
        private BindingList<LoyaltyCustomerListItem> _vipList;
        private BindingList<LoyaltyCustomerListItem> _nearTierList;

        public LoyaltyManagementForm()
        {
            InitializeComponent();
            _controller = new LoyaltyController();
        }

        private void LoyaltyManagementForm_Load(object sender, EventArgs e)
        {
            SetupDataGridViews();
            _ = LoadDataAsync();
        }

        private void SetupDataGridViews()
        {
            dgvThanThiet.AutoGenerateColumns = false;
            dgvThanThiet.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "FullName", HeaderText = "Tên Khách Hàng", Name = "FullName" });
            dgvThanThiet.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Phone", HeaderText = "SĐT", Name = "Phone" });
            dgvThanThiet.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Email", HeaderText = "Email", Name = "Email" });
            dgvThanThiet.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TotalPoints", HeaderText = "Điểm", Name = "TotalPoints" });
            dgvThanThiet.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CustomerType", HeaderText = "Hạng", Name = "CustomerType" });

            dgvVip.AutoGenerateColumns = false;
            dgvVip.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "FullName", HeaderText = "Tên Khách Hàng", Name = "FullName" });
            dgvVip.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Phone", HeaderText = "SĐT", Name = "Phone" });
            dgvVip.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Email", HeaderText = "Email", Name = "Email" });
            dgvVip.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TotalPoints", HeaderText = "Điểm", Name = "TotalPoints" });
            dgvVip.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CustomerType", HeaderText = "Hạng", Name = "CustomerType" });

            dgvNearTier.AutoGenerateColumns = false;
            dgvNearTier.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "FullName", HeaderText = "Tên Khách Hàng", Name = "FullName" });
            dgvNearTier.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Phone", HeaderText = "SĐT", Name = "Phone" });
            dgvNearTier.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Email", HeaderText = "Email", Name = "Email" });
            dgvNearTier.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TotalPoints", HeaderText = "Điểm", Name = "TotalPoints" });
            dgvNearTier.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CustomerType", HeaderText = "Hạng Hiện Tại", Name = "CustomerType" });
            dgvNearTier.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "PointsToNextTier", HeaderText = "Điểm Thiếu", Name = "PointsToNextTier" });
            dgvNearTier.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "NextTierName", HeaderText = "Hạng Tiếp Theo", Name = "NextTierName" });
        }

        private async Task LoadDataAsync()
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                btnRefresh.Enabled = false;
                lblStatus.Text = "Đang tải dữ liệu...";
                lblStatus.ForeColor = Color.Blue;

                var thanThietData = await Task.Run(() => _controller.GetThanThietCustomers());
                var vipData = await Task.Run(() => _controller.GetVipCustomers());
                var nearTierData = await Task.Run(() => _controller.GetNearTierCustomers());

                _thanThietList = new BindingList<LoyaltyCustomerListItem>(thanThietData);
                _vipList = new BindingList<LoyaltyCustomerListItem>(vipData);
                _nearTierList = new BindingList<LoyaltyCustomerListItem>(nearTierData);

                dgvThanThiet.DataSource = _thanThietList;
                dgvVip.DataSource = _vipList;
                dgvNearTier.DataSource = _nearTierList;

                lblStatThanThiet.Text = $"Thân Thiết: {_thanThietList.Count}";
                lblStatVip.Text = $"VIP: {_vipList.Count}";
                lblStatNearTier.Text = $"Sắp lên hạng: {_nearTierList.Count}";

                lblStatus.Text = "Tải dữ liệu thành công!";
                lblStatus.ForeColor = Color.Green;
                
                UpdateButtonState();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Lỗi tải dữ liệu.";
                lblStatus.ForeColor = Color.Red;
            }
            finally
            {
                this.Cursor = Cursors.Default;
                btnRefresh.Enabled = true;
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            _ = LoadDataAsync();
        }

        private void tabControlMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateButtonState();
        }

        private void dataGridView_SelectionChanged(object sender, EventArgs e)
        {
            UpdateButtonState();
        }

        private void UpdateButtonState()
        {
            DataGridView activeGrid = GetActiveDataGridView();
            if (activeGrid != null && activeGrid.CurrentRow != null)
            {
                var customer = activeGrid.CurrentRow.DataBoundItem as LoyaltyCustomerListItem;
                btnSendEmail.Enabled = customer != null && !string.IsNullOrEmpty(customer.Email);
            }
            else
            {
                btnSendEmail.Enabled = false;
            }
        }

        private DataGridView GetActiveDataGridView()
        {
            if (tabControlMain.SelectedTab == tabThanThiet) return dgvThanThiet;
            if (tabControlMain.SelectedTab == tabVip) return dgvVip;
            if (tabControlMain.SelectedTab == tabNearTier) return dgvNearTier;
            return null;
        }

        private async void btnSendEmail_Click(object sender, EventArgs e)
        {
            DataGridView activeGrid = GetActiveDataGridView();
            if (activeGrid == null || activeGrid.CurrentRow == null) return;

            var customer = activeGrid.CurrentRow.DataBoundItem as LoyaltyCustomerListItem;
            if (customer == null || string.IsNullOrEmpty(customer.Email)) return;

            try
            {
                this.Cursor = Cursors.WaitCursor;
                btnSendEmail.Enabled = false;
                lblStatus.Text = "Đang gửi email...";
                lblStatus.ForeColor = Color.Blue;

                await _controller.SendManualEmailAsync(customer);

                lblStatus.Text = "Gửi email thành công!";
                lblStatus.ForeColor = Color.Green;
                MessageBox.Show($"Đã gửi email thành công cho {customer.FullName} ({customer.Email}).", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Gửi email thất bại.";
                lblStatus.ForeColor = Color.Red;
                MessageBox.Show("Lỗi gửi email: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
                UpdateButtonState(); // Re-enable button if applicable
            }
        }

        private void dataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;
            if (dgv != null && e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                if (dgv.Columns[e.ColumnIndex].Name == "CustomerType" || dgv.Columns[e.ColumnIndex].Name == "NextTierName")
                {
                    string tier = e.Value as string;
                    if (tier == "Thường")
                    {
                        e.CellStyle.BackColor = ColorTranslator.FromHtml("#95A5A6");
                        e.CellStyle.ForeColor = Color.White;
                    }
                    else if (tier == "Thân Thiết")
                    {
                        e.CellStyle.BackColor = ColorTranslator.FromHtml("#FF6B35");
                        e.CellStyle.ForeColor = Color.White;
                    }
                    else if (tier == "VIP")
                    {
                        e.CellStyle.BackColor = ColorTranslator.FromHtml("#FFD700");
                        e.CellStyle.ForeColor = Color.Black;
                    }
                }
            }
        }
    }
}
