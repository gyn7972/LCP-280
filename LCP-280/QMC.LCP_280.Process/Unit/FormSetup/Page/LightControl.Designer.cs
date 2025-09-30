namespace QMC.LCP_280.Process.Unit.FormSetup
{
    partial class LightControl
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
            this.iluminatorListBoxItemsView = new QMC.Common.ListBoxItemsView();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.iluminatorChannelListBoxItemsView = new QMC.Common.ListBoxItemsView();
            this.illuminatorPropertyCollectionView = new QMC.Common.PropertyCollectionView();
            this.individualMenuButton1 = new QMC.Common.IndividualMenuButton();
            this.btn_Save_Illuninator_Setup = new QMC.Common.IndividualMenuButton();
            this.gbIlluminatorControl = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel7 = new System.Windows.Forms.TableLayoutPanel();
            this.btn_All_On_Illuminator = new QMC.Common.IndividualMenuButton();
            this.btn_All_Off_Illuminator = new QMC.Common.IndividualMenuButton();
            this.btn_Connect_Illuminator = new QMC.Common.IndividualMenuButton();
            this.btn_Disconnect_Illuminator = new QMC.Common.IndividualMenuButton();
            this.btn_On_Illuminator = new QMC.Common.IndividualMenuButton();
            this.btn_Off_Illuminator = new QMC.Common.IndividualMenuButton();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.illuminatorPropertyCollectionView.SuspendLayout();
            this.gbIlluminatorControl.SuspendLayout();
            this.tableLayoutPanel7.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.tableLayoutPanel1.Controls.Add(this.iluminatorListBoxItemsView, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(621, 745);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // iluminatorListBoxItemsView
            // 
            this.iluminatorListBoxItemsView.BorderColor = System.Drawing.Color.White;
            this.iluminatorListBoxItemsView.BorderWidth = 2;
            this.iluminatorListBoxItemsView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.iluminatorListBoxItemsView.GroupBackColor = System.Drawing.Color.White;
            this.iluminatorListBoxItemsView.GroupForeColor = System.Drawing.Color.Black;
            this.iluminatorListBoxItemsView.GroupName = "Illuminator";
            this.iluminatorListBoxItemsView.ItemBackColor = System.Drawing.Color.Black;
            this.iluminatorListBoxItemsView.ItemForeColor = System.Drawing.Color.Lime;
            this.iluminatorListBoxItemsView.ListBackColor = System.Drawing.Color.Black;
            this.iluminatorListBoxItemsView.ListForeColor = System.Drawing.Color.Lime;
            this.iluminatorListBoxItemsView.Location = new System.Drawing.Point(3, 6);
            this.iluminatorListBoxItemsView.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.iluminatorListBoxItemsView.Name = "iluminatorListBoxItemsView";
            this.iluminatorListBoxItemsView.SelectedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(255)))), ((int)(((byte)(0)))));
            this.iluminatorListBoxItemsView.SelectedForeColor = System.Drawing.Color.Black;
            this.iluminatorListBoxItemsView.SelectedIndex = -1;
            this.iluminatorListBoxItemsView.Size = new System.Drawing.Size(180, 733);
            this.iluminatorListBoxItemsView.TabIndex = 18;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.gbIlluminatorControl, 0, 3);
            this.tableLayoutPanel2.Controls.Add(this.btn_Save_Illuninator_Setup, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.illuminatorPropertyCollectionView, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.iluminatorChannelListBoxItemsView, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(189, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 4;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 35F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(429, 739);
            this.tableLayoutPanel2.TabIndex = 19;
            // 
            // iluminatorChannelListBoxItemsView
            // 
            this.iluminatorChannelListBoxItemsView.BorderColor = System.Drawing.Color.White;
            this.iluminatorChannelListBoxItemsView.BorderWidth = 2;
            this.iluminatorChannelListBoxItemsView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.iluminatorChannelListBoxItemsView.GroupBackColor = System.Drawing.Color.White;
            this.iluminatorChannelListBoxItemsView.GroupForeColor = System.Drawing.Color.Black;
            this.iluminatorChannelListBoxItemsView.GroupName = "Channel";
            this.iluminatorChannelListBoxItemsView.ItemBackColor = System.Drawing.Color.Black;
            this.iluminatorChannelListBoxItemsView.ItemForeColor = System.Drawing.Color.Lime;
            this.iluminatorChannelListBoxItemsView.ListBackColor = System.Drawing.Color.Black;
            this.iluminatorChannelListBoxItemsView.ListForeColor = System.Drawing.Color.Lime;
            this.iluminatorChannelListBoxItemsView.Location = new System.Drawing.Point(3, 6);
            this.iluminatorChannelListBoxItemsView.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.iluminatorChannelListBoxItemsView.Name = "iluminatorChannelListBoxItemsView";
            this.iluminatorChannelListBoxItemsView.SelectedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(255)))), ((int)(((byte)(0)))));
            this.iluminatorChannelListBoxItemsView.SelectedForeColor = System.Drawing.Color.Black;
            this.iluminatorChannelListBoxItemsView.SelectedIndex = -1;
            this.iluminatorChannelListBoxItemsView.Size = new System.Drawing.Size(423, 246);
            this.iluminatorChannelListBoxItemsView.TabIndex = 20;
            // 
            // illuminatorPropertyCollectionView
            // 
            this.illuminatorPropertyCollectionView.Controls.Add(this.individualMenuButton1);
            this.illuminatorPropertyCollectionView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.illuminatorPropertyCollectionView.FastBuild = true;
            this.illuminatorPropertyCollectionView.GroupName = "Property";
            this.illuminatorPropertyCollectionView.Location = new System.Drawing.Point(3, 261);
            this.illuminatorPropertyCollectionView.Name = "illuminatorPropertyCollectionView";
            this.illuminatorPropertyCollectionView.Size = new System.Drawing.Size(423, 289);
            this.illuminatorPropertyCollectionView.SuppressResizeInvalidation = true;
            this.illuminatorPropertyCollectionView.TabIndex = 21;
            // 
            // individualMenuButton1
            // 
            this.individualMenuButton1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.individualMenuButton1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.individualMenuButton1.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.individualMenuButton1.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.individualMenuButton1.CustomForeColor = System.Drawing.Color.Black;
            this.individualMenuButton1.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.individualMenuButton1.ForeColor = System.Drawing.Color.Black;
            this.individualMenuButton1.ImageSize = new System.Drawing.Size(45, 45);
            this.individualMenuButton1.Location = new System.Drawing.Point(330, 312);
            this.individualMenuButton1.Name = "individualMenuButton1";
            this.individualMenuButton1.Size = new System.Drawing.Size(100, 40);
            this.individualMenuButton1.TabIndex = 5;
            this.individualMenuButton1.TabStop = false;
            this.individualMenuButton1.Text = "Save";
            this.individualMenuButton1.UseVisualStyleBackColor = false;
            // 
            // btn_Save_Illuninator_Setup
            // 
            this.btn_Save_Illuninator_Setup.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Save_Illuninator_Setup.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Save_Illuninator_Setup.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Save_Illuninator_Setup.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Save_Illuninator_Setup.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Save_Illuninator_Setup.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Save_Illuninator_Setup.ForeColor = System.Drawing.Color.Black;
            this.btn_Save_Illuninator_Setup.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Save_Illuninator_Setup.Location = new System.Drawing.Point(3, 556);
            this.btn_Save_Illuninator_Setup.Name = "btn_Save_Illuninator_Setup";
            this.btn_Save_Illuninator_Setup.Size = new System.Drawing.Size(423, 30);
            this.btn_Save_Illuninator_Setup.TabIndex = 25;
            this.btn_Save_Illuninator_Setup.TabStop = false;
            this.btn_Save_Illuninator_Setup.Text = "Save";
            this.btn_Save_Illuninator_Setup.UseVisualStyleBackColor = false;
            // 
            // gbIlluminatorControl
            // 
            this.gbIlluminatorControl.BackColor = System.Drawing.Color.White;
            this.gbIlluminatorControl.Controls.Add(this.tableLayoutPanel7);
            this.gbIlluminatorControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbIlluminatorControl.Location = new System.Drawing.Point(3, 592);
            this.gbIlluminatorControl.Name = "gbIlluminatorControl";
            this.gbIlluminatorControl.Size = new System.Drawing.Size(423, 144);
            this.gbIlluminatorControl.TabIndex = 26;
            this.gbIlluminatorControl.TabStop = false;
            this.gbIlluminatorControl.Text = "Control";
            // 
            // tableLayoutPanel7
            // 
            this.tableLayoutPanel7.ColumnCount = 2;
            this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.00001F));
            this.tableLayoutPanel7.Controls.Add(this.btn_All_On_Illuminator, 0, 1);
            this.tableLayoutPanel7.Controls.Add(this.btn_All_Off_Illuminator, 1, 1);
            this.tableLayoutPanel7.Controls.Add(this.btn_Connect_Illuminator, 0, 0);
            this.tableLayoutPanel7.Controls.Add(this.btn_Disconnect_Illuminator, 1, 0);
            this.tableLayoutPanel7.Controls.Add(this.btn_On_Illuminator, 0, 2);
            this.tableLayoutPanel7.Controls.Add(this.btn_Off_Illuminator, 1, 2);
            this.tableLayoutPanel7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel7.Location = new System.Drawing.Point(3, 17);
            this.tableLayoutPanel7.Name = "tableLayoutPanel7";
            this.tableLayoutPanel7.RowCount = 3;
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel7.Size = new System.Drawing.Size(417, 124);
            this.tableLayoutPanel7.TabIndex = 2;
            // 
            // btn_All_On_Illuminator
            // 
            this.btn_All_On_Illuminator.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_All_On_Illuminator.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_All_On_Illuminator.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_All_On_Illuminator.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_All_On_Illuminator.CustomForeColor = System.Drawing.Color.Black;
            this.btn_All_On_Illuminator.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_All_On_Illuminator.ForeColor = System.Drawing.Color.Black;
            this.btn_All_On_Illuminator.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_All_On_Illuminator.Location = new System.Drawing.Point(3, 44);
            this.btn_All_On_Illuminator.Name = "btn_All_On_Illuminator";
            this.btn_All_On_Illuminator.Size = new System.Drawing.Size(202, 33);
            this.btn_All_On_Illuminator.TabIndex = 12;
            this.btn_All_On_Illuminator.TabStop = false;
            this.btn_All_On_Illuminator.Text = "All On";
            this.btn_All_On_Illuminator.UseVisualStyleBackColor = false;
            // 
            // btn_All_Off_Illuminator
            // 
            this.btn_All_Off_Illuminator.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_All_Off_Illuminator.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_All_Off_Illuminator.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_All_Off_Illuminator.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_All_Off_Illuminator.CustomForeColor = System.Drawing.Color.Black;
            this.btn_All_Off_Illuminator.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_All_Off_Illuminator.ForeColor = System.Drawing.Color.Black;
            this.btn_All_Off_Illuminator.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_All_Off_Illuminator.Location = new System.Drawing.Point(211, 44);
            this.btn_All_Off_Illuminator.Name = "btn_All_Off_Illuminator";
            this.btn_All_Off_Illuminator.Size = new System.Drawing.Size(203, 34);
            this.btn_All_Off_Illuminator.TabIndex = 11;
            this.btn_All_Off_Illuminator.TabStop = false;
            this.btn_All_Off_Illuminator.Text = "All Off";
            this.btn_All_Off_Illuminator.UseVisualStyleBackColor = false;
            // 
            // btn_Connect_Illuminator
            // 
            this.btn_Connect_Illuminator.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Connect_Illuminator.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Connect_Illuminator.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Connect_Illuminator.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Connect_Illuminator.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Connect_Illuminator.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Connect_Illuminator.ForeColor = System.Drawing.Color.Black;
            this.btn_Connect_Illuminator.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Connect_Illuminator.Location = new System.Drawing.Point(3, 3);
            this.btn_Connect_Illuminator.Name = "btn_Connect_Illuminator";
            this.btn_Connect_Illuminator.Size = new System.Drawing.Size(202, 33);
            this.btn_Connect_Illuminator.TabIndex = 10;
            this.btn_Connect_Illuminator.TabStop = false;
            this.btn_Connect_Illuminator.Text = "Connect";
            this.btn_Connect_Illuminator.UseVisualStyleBackColor = false;
            // 
            // btn_Disconnect_Illuminator
            // 
            this.btn_Disconnect_Illuminator.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Disconnect_Illuminator.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Disconnect_Illuminator.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Disconnect_Illuminator.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Disconnect_Illuminator.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Disconnect_Illuminator.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Disconnect_Illuminator.ForeColor = System.Drawing.Color.Black;
            this.btn_Disconnect_Illuminator.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Disconnect_Illuminator.Location = new System.Drawing.Point(211, 3);
            this.btn_Disconnect_Illuminator.Name = "btn_Disconnect_Illuminator";
            this.btn_Disconnect_Illuminator.Size = new System.Drawing.Size(203, 34);
            this.btn_Disconnect_Illuminator.TabIndex = 9;
            this.btn_Disconnect_Illuminator.TabStop = false;
            this.btn_Disconnect_Illuminator.Text = "Disconnect";
            this.btn_Disconnect_Illuminator.UseVisualStyleBackColor = false;
            // 
            // btn_On_Illuminator
            // 
            this.btn_On_Illuminator.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_On_Illuminator.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_On_Illuminator.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_On_Illuminator.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_On_Illuminator.CustomForeColor = System.Drawing.Color.Black;
            this.btn_On_Illuminator.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_On_Illuminator.ForeColor = System.Drawing.Color.Black;
            this.btn_On_Illuminator.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_On_Illuminator.Location = new System.Drawing.Point(3, 85);
            this.btn_On_Illuminator.Name = "btn_On_Illuminator";
            this.btn_On_Illuminator.Size = new System.Drawing.Size(202, 33);
            this.btn_On_Illuminator.TabIndex = 6;
            this.btn_On_Illuminator.TabStop = false;
            this.btn_On_Illuminator.Text = "On";
            this.btn_On_Illuminator.UseVisualStyleBackColor = false;
            // 
            // btn_Off_Illuminator
            // 
            this.btn_Off_Illuminator.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Off_Illuminator.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Off_Illuminator.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Off_Illuminator.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Off_Illuminator.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Off_Illuminator.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Off_Illuminator.ForeColor = System.Drawing.Color.Black;
            this.btn_Off_Illuminator.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Off_Illuminator.Location = new System.Drawing.Point(211, 85);
            this.btn_Off_Illuminator.Name = "btn_Off_Illuminator";
            this.btn_Off_Illuminator.Size = new System.Drawing.Size(203, 34);
            this.btn_Off_Illuminator.TabIndex = 1;
            this.btn_Off_Illuminator.TabStop = false;
            this.btn_Off_Illuminator.Text = "Off";
            this.btn_Off_Illuminator.UseVisualStyleBackColor = false;
            // 
            // LightControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "LightControl";
            this.Size = new System.Drawing.Size(621, 745);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.illuminatorPropertyCollectionView.ResumeLayout(false);
            this.gbIlluminatorControl.ResumeLayout(false);
            this.tableLayoutPanel7.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Common.ListBoxItemsView iluminatorListBoxItemsView;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private Common.ListBoxItemsView iluminatorChannelListBoxItemsView;
        private Common.PropertyCollectionView illuminatorPropertyCollectionView;
        private Common.IndividualMenuButton individualMenuButton1;
        private Common.IndividualMenuButton btn_Save_Illuninator_Setup;
        private System.Windows.Forms.GroupBox gbIlluminatorControl;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel7;
        private Common.IndividualMenuButton btn_All_On_Illuminator;
        private Common.IndividualMenuButton btn_All_Off_Illuminator;
        private Common.IndividualMenuButton btn_Connect_Illuminator;
        private Common.IndividualMenuButton btn_Disconnect_Illuminator;
        private Common.IndividualMenuButton btn_On_Illuminator;
        private Common.IndividualMenuButton btn_Off_Illuminator;
    }
}
