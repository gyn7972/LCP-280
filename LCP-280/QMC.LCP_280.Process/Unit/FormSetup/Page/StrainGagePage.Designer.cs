namespace QMC.LCP_280.Process.Unit.FormSetup.Page
{
    partial class StrainGagePage
    {
        /// <summary> 
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 구성 요소 디자이너에서 생성한 코드

        /// <summary> 
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.lbivSelect = new QMC.Common.ListBoxItemsView();
            this.pcvConfig = new QMC.Common.PropertyCollectionView();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btnSave = new QMC.Common.IndividualMenuButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel7 = new System.Windows.Forms.TableLayoutPanel();
            this.btnShowDialog = new QMC.Common.IndividualMenuButton();
            this.btnDeviceInfo = new QMC.Common.IndividualMenuButton();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.btnStart = new QMC.Common.IndividualMenuButton();
            this.btnResetZeroSet = new QMC.Common.IndividualMenuButton();
            this.btnZeroSet = new QMC.Common.IndividualMenuButton();
            this.btnStop = new QMC.Common.IndividualMenuButton();
            this.strainGageDataGridViewer1 = new QMC.Common.StrainGage.StrainGageDataGridViewer();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.tableLayoutPanel7.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 29.52F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70.48F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1250, 700);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.lbivSelect, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.pcvConfig, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.panel2, 0, 2);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 3;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(363, 694);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // lbivSelect
            // 
            this.lbivSelect.BorderColor = System.Drawing.Color.White;
            this.lbivSelect.BorderWidth = 1;
            this.lbivSelect.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbivSelect.GroupBackColor = System.Drawing.Color.White;
            this.lbivSelect.GroupForeColor = System.Drawing.Color.Black;
            this.lbivSelect.GroupName = "Select Item";
            this.lbivSelect.ItemBackColor = System.Drawing.Color.Black;
            this.lbivSelect.ItemForeColor = System.Drawing.Color.Lime;
            this.lbivSelect.ListBackColor = System.Drawing.Color.Black;
            this.lbivSelect.ListForeColor = System.Drawing.Color.Lime;
            this.lbivSelect.Location = new System.Drawing.Point(3, 3);
            this.lbivSelect.Name = "lbivSelect";
            this.lbivSelect.SelectedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(255)))), ((int)(((byte)(0)))));
            this.lbivSelect.SelectedForeColor = System.Drawing.Color.Black;
            this.lbivSelect.SelectedIndex = -1;
            this.lbivSelect.Size = new System.Drawing.Size(357, 316);
            this.lbivSelect.TabIndex = 0;
            this.lbivSelect.ItemSelected += new System.EventHandler<int>(this.lbivSelect_ItemSelected);
            // 
            // pcvConfig
            // 
            this.pcvConfig.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pcvConfig.FastBuild = true;
            this.pcvConfig.GroupName = "Property";
            this.pcvConfig.Location = new System.Drawing.Point(3, 325);
            this.pcvConfig.Name = "pcvConfig";
            this.pcvConfig.Size = new System.Drawing.Size(357, 316);
            this.pcvConfig.SuppressResizeInvalidation = true;
            this.pcvConfig.TabIndex = 1;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.btnSave);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(3, 647);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(357, 44);
            this.panel2.TabIndex = 2;
            // 
            // btnSave
            // 
            this.btnSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnSave.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnSave.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnSave.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnSave.CustomForeColor = System.Drawing.Color.Black;
            this.btnSave.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnSave.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSave.ForeColor = System.Drawing.Color.Black;
            this.btnSave.ImageSize = new System.Drawing.Size(45, 45);
            this.btnSave.Location = new System.Drawing.Point(259, 0);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(98, 44);
            this.btnSave.TabIndex = 29;
            this.btnSave.TabStop = false;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.tableLayoutPanel3);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(372, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(875, 694);
            this.panel1.TabIndex = 1;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 83.33334F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66666F));
            this.tableLayoutPanel3.Controls.Add(this.tableLayoutPanel4, 0, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 4;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(875, 694);
            this.tableLayoutPanel3.TabIndex = 34;
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 2;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 83F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 17F));
            this.tableLayoutPanel4.Controls.Add(this.tableLayoutPanel7, 0, 1);
            this.tableLayoutPanel4.Controls.Add(this.tableLayoutPanel5, 1, 0);
            this.tableLayoutPanel4.Controls.Add(this.strainGageDataGridViewer1, 0, 0);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 2;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(723, 341);
            this.tableLayoutPanel4.TabIndex = 0;
            // 
            // tableLayoutPanel7
            // 
            this.tableLayoutPanel7.ColumnCount = 11;
            this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15.38461F));
            this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12.82052F));
            this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12.82051F));
            this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12.82051F));
            this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tableLayoutPanel7.Controls.Add(this.btnShowDialog, 0, 1);
            this.tableLayoutPanel7.Controls.Add(this.btnDeviceInfo, 0, 0);
            this.tableLayoutPanel7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel7.Location = new System.Drawing.Point(3, 241);
            this.tableLayoutPanel7.Name = "tableLayoutPanel7";
            this.tableLayoutPanel7.RowCount = 2;
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel7.Size = new System.Drawing.Size(594, 97);
            this.tableLayoutPanel7.TabIndex = 30;
            // 
            // btnShowDialog
            // 
            this.btnShowDialog.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnShowDialog.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnShowDialog.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnShowDialog.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnShowDialog.CustomForeColor = System.Drawing.Color.Black;
            this.btnShowDialog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnShowDialog.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnShowDialog.ForeColor = System.Drawing.Color.Black;
            this.btnShowDialog.ImageSize = new System.Drawing.Size(45, 45);
            this.btnShowDialog.Location = new System.Drawing.Point(3, 51);
            this.btnShowDialog.Name = "btnShowDialog";
            this.btnShowDialog.Size = new System.Drawing.Size(121, 43);
            this.btnShowDialog.TabIndex = 31;
            this.btnShowDialog.TabStop = false;
            this.btnShowDialog.Text = "Show Monitoring Dialog";
            this.btnShowDialog.UseVisualStyleBackColor = false;
            this.btnShowDialog.Click += new System.EventHandler(this.btnShowDialog_Click);
            // 
            // btnDeviceInfo
            // 
            this.btnDeviceInfo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnDeviceInfo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnDeviceInfo.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnDeviceInfo.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnDeviceInfo.CustomForeColor = System.Drawing.Color.Black;
            this.btnDeviceInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnDeviceInfo.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDeviceInfo.ForeColor = System.Drawing.Color.Black;
            this.btnDeviceInfo.ImageSize = new System.Drawing.Size(45, 45);
            this.btnDeviceInfo.Location = new System.Drawing.Point(3, 3);
            this.btnDeviceInfo.Name = "btnDeviceInfo";
            this.btnDeviceInfo.Size = new System.Drawing.Size(121, 42);
            this.btnDeviceInfo.TabIndex = 28;
            this.btnDeviceInfo.TabStop = false;
            this.btnDeviceInfo.Text = "Show Device";
            this.btnDeviceInfo.UseVisualStyleBackColor = false;
            this.btnDeviceInfo.Click += new System.EventHandler(this.btnDeviceInfo_Click);
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 1;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.Controls.Add(this.btnStart, 0, 1);
            this.tableLayoutPanel5.Controls.Add(this.btnResetZeroSet, 0, 4);
            this.tableLayoutPanel5.Controls.Add(this.btnZeroSet, 0, 3);
            this.tableLayoutPanel5.Controls.Add(this.btnStop, 0, 2);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(603, 3);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 5;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 8F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(117, 232);
            this.tableLayoutPanel5.TabIndex = 0;
            // 
            // btnStart
            // 
            this.btnStart.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnStart.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnStart.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnStart.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnStart.CustomForeColor = System.Drawing.Color.Black;
            this.btnStart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnStart.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStart.ForeColor = System.Drawing.Color.Black;
            this.btnStart.ImageSize = new System.Drawing.Size(45, 45);
            this.btnStart.Location = new System.Drawing.Point(3, 11);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(111, 50);
            this.btnStart.TabIndex = 25;
            this.btnStart.TabStop = false;
            this.btnStart.Text = "Monitoring Start";
            this.btnStart.UseVisualStyleBackColor = false;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnResetZeroSet
            // 
            this.btnResetZeroSet.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnResetZeroSet.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnResetZeroSet.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnResetZeroSet.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnResetZeroSet.CustomForeColor = System.Drawing.Color.Black;
            this.btnResetZeroSet.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnResetZeroSet.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnResetZeroSet.ForeColor = System.Drawing.Color.Black;
            this.btnResetZeroSet.ImageSize = new System.Drawing.Size(45, 45);
            this.btnResetZeroSet.Location = new System.Drawing.Point(3, 179);
            this.btnResetZeroSet.Name = "btnResetZeroSet";
            this.btnResetZeroSet.Size = new System.Drawing.Size(111, 50);
            this.btnResetZeroSet.TabIndex = 32;
            this.btnResetZeroSet.TabStop = false;
            this.btnResetZeroSet.Text = "Reset ZeroSet";
            this.btnResetZeroSet.UseVisualStyleBackColor = false;
            this.btnResetZeroSet.Click += new System.EventHandler(this.btnResetZeroSet_Click);
            // 
            // btnZeroSet
            // 
            this.btnZeroSet.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnZeroSet.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnZeroSet.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnZeroSet.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnZeroSet.CustomForeColor = System.Drawing.Color.Black;
            this.btnZeroSet.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnZeroSet.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnZeroSet.ForeColor = System.Drawing.Color.Black;
            this.btnZeroSet.ImageSize = new System.Drawing.Size(45, 45);
            this.btnZeroSet.Location = new System.Drawing.Point(3, 123);
            this.btnZeroSet.Name = "btnZeroSet";
            this.btnZeroSet.Size = new System.Drawing.Size(111, 50);
            this.btnZeroSet.TabIndex = 33;
            this.btnZeroSet.TabStop = false;
            this.btnZeroSet.Text = "ZeroSet";
            this.btnZeroSet.UseVisualStyleBackColor = false;
            this.btnZeroSet.Click += new System.EventHandler(this.btnZeroSet_Click);
            // 
            // btnStop
            // 
            this.btnStop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnStop.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnStop.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnStop.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnStop.CustomForeColor = System.Drawing.Color.Black;
            this.btnStop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnStop.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStop.ForeColor = System.Drawing.Color.Black;
            this.btnStop.ImageSize = new System.Drawing.Size(45, 45);
            this.btnStop.Location = new System.Drawing.Point(3, 67);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(111, 50);
            this.btnStop.TabIndex = 26;
            this.btnStop.TabStop = false;
            this.btnStop.Text = "Monitoring Stop";
            this.btnStop.UseVisualStyleBackColor = false;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // strainGageDataGridViewer1
            // 
            this.strainGageDataGridViewer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.strainGageDataGridViewer1.Location = new System.Drawing.Point(3, 3);
            this.strainGageDataGridViewer1.Name = "strainGageDataGridViewer1";
            this.strainGageDataGridViewer1.Size = new System.Drawing.Size(594, 232);
            this.strainGageDataGridViewer1.TabIndex = 29;
            // 
            // StrainGagePage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "StrainGagePage";
            this.Size = new System.Drawing.Size(1250, 700);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel7.ResumeLayout(false);
            this.tableLayoutPanel5.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private Common.ListBoxItemsView lbivSelect;
        private Common.PropertyCollectionView pcvConfig;
        private System.Windows.Forms.Panel panel1;
        private Common.IndividualMenuButton btnStop;
        private Common.IndividualMenuButton btnStart;
        private System.Windows.Forms.Panel panel2;
        private Common.IndividualMenuButton btnSave;
        private Common.StrainGage.StrainGageDataGridViewer strainGageDataGridViewer1;
        private Common.IndividualMenuButton btnZeroSet;
        private Common.IndividualMenuButton btnResetZeroSet;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel7;
        private Common.IndividualMenuButton btnDeviceInfo;
        private Common.IndividualMenuButton btnShowDialog;
    }
}
