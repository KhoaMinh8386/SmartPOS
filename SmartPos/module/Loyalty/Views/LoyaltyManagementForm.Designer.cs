using System.Windows.Forms;
using System.Drawing;

namespace SmartPos.Module.Loyalty.Views
{
    partial class LoyaltyManagementForm
    {
        private System.ComponentModel.IContainer components = null;

        private TabControl tabControlMain;
        private TabPage tabThanThiet;
        private TabPage tabVip;
        private TabPage tabNearTier;

        private DataGridView dgvThanThiet;
        private DataGridView dgvVip;
        private DataGridView dgvNearTier;

        private Button btnRefresh;
        private Button btnSendEmail;

        private Label lblStatThanThiet;
        private Label lblStatVip;
        private Label lblStatNearTier;
        private Label lblStatus;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.tabControlMain = new System.Windows.Forms.TabControl();
            this.tabThanThiet = new System.Windows.Forms.TabPage();
            this.tabVip = new System.Windows.Forms.TabPage();
            this.tabNearTier = new System.Windows.Forms.TabPage();
            
            this.dgvThanThiet = new System.Windows.Forms.DataGridView();
            this.dgvVip = new System.Windows.Forms.DataGridView();
            this.dgvNearTier = new System.Windows.Forms.DataGridView();
            
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnSendEmail = new System.Windows.Forms.Button();
            
            this.lblStatThanThiet = new System.Windows.Forms.Label();
            this.lblStatVip = new System.Windows.Forms.Label();
            this.lblStatNearTier = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();

            this.tabControlMain.SuspendLayout();
            this.tabThanThiet.SuspendLayout();
            this.tabVip.SuspendLayout();
            this.tabNearTier.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvThanThiet)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvVip)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvNearTier)).BeginInit();
            this.SuspendLayout();

            // 
            // tabControlMain
            // 
            this.tabControlMain.Controls.Add(this.tabThanThiet);
            this.tabControlMain.Controls.Add(this.tabVip);
            this.tabControlMain.Controls.Add(this.tabNearTier);
            this.tabControlMain.Location = new System.Drawing.Point(12, 50);
            this.tabControlMain.Name = "tabControlMain";
            this.tabControlMain.SelectedIndex = 0;
            this.tabControlMain.Size = new System.Drawing.Size(960, 450);
            this.tabControlMain.TabIndex = 0;
            this.tabControlMain.SelectedIndexChanged += new System.EventHandler(this.tabControlMain_SelectedIndexChanged);

            // 
            // tabThanThiet
            // 
            this.tabThanThiet.Controls.Add(this.dgvThanThiet);
            this.tabThanThiet.Location = new System.Drawing.Point(4, 25);
            this.tabThanThiet.Name = "tabThanThiet";
            this.tabThanThiet.Padding = new System.Windows.Forms.Padding(3);
            this.tabThanThiet.Size = new System.Drawing.Size(952, 421);
            this.tabThanThiet.TabIndex = 0;
            this.tabThanThiet.Text = "Thân Thiết";
            this.tabThanThiet.UseVisualStyleBackColor = true;

            // 
            // tabVip
            // 
            this.tabVip.Controls.Add(this.dgvVip);
            this.tabVip.Location = new System.Drawing.Point(4, 25);
            this.tabVip.Name = "tabVip";
            this.tabVip.Padding = new System.Windows.Forms.Padding(3);
            this.tabVip.Size = new System.Drawing.Size(952, 421);
            this.tabVip.TabIndex = 1;
            this.tabVip.Text = "VIP";
            this.tabVip.UseVisualStyleBackColor = true;

            // 
            // tabNearTier
            // 
            this.tabNearTier.Controls.Add(this.dgvNearTier);
            this.tabNearTier.Location = new System.Drawing.Point(4, 25);
            this.tabNearTier.Name = "tabNearTier";
            this.tabNearTier.Padding = new System.Windows.Forms.Padding(3);
            this.tabNearTier.Size = new System.Drawing.Size(952, 421);
            this.tabNearTier.TabIndex = 2;
            this.tabNearTier.Text = "Sắp lên hạng";
            this.tabNearTier.UseVisualStyleBackColor = true;

            // 
            // dgvThanThiet
            // 
            this.dgvThanThiet.AllowUserToAddRows = false;
            this.dgvThanThiet.AllowUserToDeleteRows = false;
            this.dgvThanThiet.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvThanThiet.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvThanThiet.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvThanThiet.Location = new System.Drawing.Point(3, 3);
            this.dgvThanThiet.Name = "dgvThanThiet";
            this.dgvThanThiet.ReadOnly = true;
            this.dgvThanThiet.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvThanThiet.Size = new System.Drawing.Size(946, 415);
            this.dgvThanThiet.TabIndex = 0;
            this.dgvThanThiet.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dataGridView_CellFormatting);
            this.dgvThanThiet.SelectionChanged += new System.EventHandler(this.dataGridView_SelectionChanged);

            // 
            // dgvVip
            // 
            this.dgvVip.AllowUserToAddRows = false;
            this.dgvVip.AllowUserToDeleteRows = false;
            this.dgvVip.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvVip.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvVip.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvVip.Location = new System.Drawing.Point(3, 3);
            this.dgvVip.Name = "dgvVip";
            this.dgvVip.ReadOnly = true;
            this.dgvVip.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvVip.Size = new System.Drawing.Size(946, 415);
            this.dgvVip.TabIndex = 0;
            this.dgvVip.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dataGridView_CellFormatting);
            this.dgvVip.SelectionChanged += new System.EventHandler(this.dataGridView_SelectionChanged);

            // 
            // dgvNearTier
            // 
            this.dgvNearTier.AllowUserToAddRows = false;
            this.dgvNearTier.AllowUserToDeleteRows = false;
            this.dgvNearTier.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvNearTier.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvNearTier.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvNearTier.Location = new System.Drawing.Point(3, 3);
            this.dgvNearTier.Name = "dgvNearTier";
            this.dgvNearTier.ReadOnly = true;
            this.dgvNearTier.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvNearTier.Size = new System.Drawing.Size(946, 415);
            this.dgvNearTier.TabIndex = 0;
            this.dgvNearTier.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dataGridView_CellFormatting);
            this.dgvNearTier.SelectionChanged += new System.EventHandler(this.dataGridView_SelectionChanged);

            // 
            // btnRefresh
            // 
            this.btnRefresh.Location = new System.Drawing.Point(12, 12);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(120, 30);
            this.btnRefresh.TabIndex = 1;
            this.btnRefresh.Text = "Làm mới";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);

            // 
            // btnSendEmail
            // 
            this.btnSendEmail.Location = new System.Drawing.Point(138, 12);
            this.btnSendEmail.Name = "btnSendEmail";
            this.btnSendEmail.Size = new System.Drawing.Size(150, 30);
            this.btnSendEmail.TabIndex = 2;
            this.btnSendEmail.Text = "Gửi Email Thủ Công";
            this.btnSendEmail.UseVisualStyleBackColor = true;
            this.btnSendEmail.Click += new System.EventHandler(this.btnSendEmail_Click);

            // 
            // lblStatThanThiet
            // 
            this.lblStatThanThiet.AutoSize = true;
            this.lblStatThanThiet.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStatThanThiet.Location = new System.Drawing.Point(12, 510);
            this.lblStatThanThiet.Name = "lblStatThanThiet";
            this.lblStatThanThiet.Size = new System.Drawing.Size(100, 18);
            this.lblStatThanThiet.TabIndex = 3;
            this.lblStatThanThiet.Text = "Thân Thiết: 0";

            // 
            // lblStatVip
            // 
            this.lblStatVip.AutoSize = true;
            this.lblStatVip.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStatVip.Location = new System.Drawing.Point(150, 510);
            this.lblStatVip.Name = "lblStatVip";
            this.lblStatVip.Size = new System.Drawing.Size(50, 18);
            this.lblStatVip.TabIndex = 4;
            this.lblStatVip.Text = "VIP: 0";

            // 
            // lblStatNearTier
            // 
            this.lblStatNearTier.AutoSize = true;
            this.lblStatNearTier.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStatNearTier.Location = new System.Drawing.Point(250, 510);
            this.lblStatNearTier.Name = "lblStatNearTier";
            this.lblStatNearTier.Size = new System.Drawing.Size(120, 18);
            this.lblStatNearTier.TabIndex = 5;
            this.lblStatNearTier.Text = "Sắp lên hạng: 0";

            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.ForeColor = System.Drawing.Color.Green;
            this.lblStatus.Location = new System.Drawing.Point(300, 19);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(0, 17);
            this.lblStatus.TabIndex = 6;

            // 
            // LoyaltyManagementForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(984, 541);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.lblStatNearTier);
            this.Controls.Add(this.lblStatVip);
            this.Controls.Add(this.lblStatThanThiet);
            this.Controls.Add(this.btnSendEmail);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.tabControlMain);
            this.Name = "LoyaltyManagementForm";
            this.Text = "Quản lý Khách Hàng Thân Thiết";
            this.Load += new System.EventHandler(this.LoyaltyManagementForm_Load);
            this.tabControlMain.ResumeLayout(false);
            this.tabThanThiet.ResumeLayout(false);
            this.tabVip.ResumeLayout(false);
            this.tabNearTier.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvThanThiet)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvVip)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvNearTier)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
