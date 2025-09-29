namespace QMC.LCP_280.Process.Unit.FormSetup
{
    partial class BarcoderControl
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
            this.listBoxItemsView = new QMC.Common.ListBoxItemsView();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel9 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel10 = new System.Windows.Forms.TableLayoutPanel();
            this.btnBarcoderScan = new QMC.Common.IndividualMenuButton();
            this.labelBarcoderData = new System.Windows.Forms.Label();
            this.btn_Save_Barcoder_Setup = new QMC.Common.IndividualMenuButton();
            this.propertyCollectionView = new QMC.Common.PropertyCollectionView();
            this.individualMenuButton3 = new QMC.Common.IndividualMenuButton();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel9.SuspendLayout();
            this.tableLayoutPanel10.SuspendLayout();
            this.propertyCollectionView.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.White;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 35F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 65F));
            this.tableLayoutPanel1.Controls.Add(this.listBoxItemsView, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(420, 745);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // listBoxItemsView
            // 
            this.listBoxItemsView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxItemsView.BorderColor = System.Drawing.Color.White;
            this.listBoxItemsView.BorderWidth = 2;
            this.listBoxItemsView.GroupBackColor = System.Drawing.Color.White;
            this.listBoxItemsView.GroupForeColor = System.Drawing.Color.Black;
            this.listBoxItemsView.GroupName = "Barcoder List";
            this.listBoxItemsView.ItemBackColor = System.Drawing.Color.Black;
            this.listBoxItemsView.ItemForeColor = System.Drawing.Color.Lime;
            this.listBoxItemsView.ListBackColor = System.Drawing.Color.Black;
            this.listBoxItemsView.ListForeColor = System.Drawing.Color.Lime;
            this.listBoxItemsView.Location = new System.Drawing.Point(3, 6);
            this.listBoxItemsView.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.listBoxItemsView.Name = "listBoxItemsView";
            this.listBoxItemsView.SelectedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(255)))), ((int)(((byte)(0)))));
            this.listBoxItemsView.SelectedForeColor = System.Drawing.Color.Black;
            this.listBoxItemsView.SelectedIndex = -1;
            this.listBoxItemsView.Size = new System.Drawing.Size(141, 733);
            this.listBoxItemsView.TabIndex = 19;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.groupBox1, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.btn_Save_Barcoder_Setup, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.propertyCollectionView, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(150, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 3;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 68F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 7F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(267, 739);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.White;
            this.groupBox1.Controls.Add(this.tableLayoutPanel9);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(3, 556);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(261, 180);
            this.groupBox1.TabIndex = 27;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Control";
            // 
            // tableLayoutPanel9
            // 
            this.tableLayoutPanel9.ColumnCount = 1;
            this.tableLayoutPanel9.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel9.Controls.Add(this.tableLayoutPanel10, 0, 1);
            this.tableLayoutPanel9.Controls.Add(this.labelBarcoderData, 0, 0);
            this.tableLayoutPanel9.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel9.Location = new System.Drawing.Point(3, 17);
            this.tableLayoutPanel9.Name = "tableLayoutPanel9";
            this.tableLayoutPanel9.RowCount = 2;
            this.tableLayoutPanel9.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.tableLayoutPanel9.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel9.Size = new System.Drawing.Size(255, 160);
            this.tableLayoutPanel9.TabIndex = 2;
            // 
            // tableLayoutPanel10
            // 
            this.tableLayoutPanel10.ColumnCount = 1;
            this.tableLayoutPanel10.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel10.Controls.Add(this.btnBarcoderScan, 0, 0);
            this.tableLayoutPanel10.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel10.Location = new System.Drawing.Point(3, 115);
            this.tableLayoutPanel10.Name = "tableLayoutPanel10";
            this.tableLayoutPanel10.RowCount = 1;
            this.tableLayoutPanel10.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel10.Size = new System.Drawing.Size(249, 42);
            this.tableLayoutPanel10.TabIndex = 1;
            // 
            // btnBarcoderScan
            // 
            this.btnBarcoderScan.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnBarcoderScan.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnBarcoderScan.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnBarcoderScan.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnBarcoderScan.CustomForeColor = System.Drawing.Color.Black;
            this.btnBarcoderScan.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnBarcoderScan.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnBarcoderScan.ForeColor = System.Drawing.Color.Black;
            this.btnBarcoderScan.ImageSize = new System.Drawing.Size(45, 45);
            this.btnBarcoderScan.Location = new System.Drawing.Point(3, 3);
            this.btnBarcoderScan.Name = "btnBarcoderScan";
            this.btnBarcoderScan.Size = new System.Drawing.Size(243, 36);
            this.btnBarcoderScan.TabIndex = 0;
            this.btnBarcoderScan.TabStop = false;
            this.btnBarcoderScan.Text = "Scan";
            this.btnBarcoderScan.UseVisualStyleBackColor = false;
            // 
            // labelBarcoderData
            // 
            this.labelBarcoderData.AutoSize = true;
            this.labelBarcoderData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelBarcoderData.Font = new System.Drawing.Font("돋움체", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.labelBarcoderData.Location = new System.Drawing.Point(3, 3);
            this.labelBarcoderData.Margin = new System.Windows.Forms.Padding(3);
            this.labelBarcoderData.Name = "labelBarcoderData";
            this.labelBarcoderData.Size = new System.Drawing.Size(249, 106);
            this.labelBarcoderData.TabIndex = 2;
            this.labelBarcoderData.Text = "label1";
            this.labelBarcoderData.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btn_Save_Barcoder_Setup
            // 
            this.btn_Save_Barcoder_Setup.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Save_Barcoder_Setup.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Save_Barcoder_Setup.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Save_Barcoder_Setup.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Save_Barcoder_Setup.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Save_Barcoder_Setup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_Save_Barcoder_Setup.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Save_Barcoder_Setup.ForeColor = System.Drawing.Color.Black;
            this.btn_Save_Barcoder_Setup.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Save_Barcoder_Setup.Location = new System.Drawing.Point(3, 505);
            this.btn_Save_Barcoder_Setup.Name = "btn_Save_Barcoder_Setup";
            this.btn_Save_Barcoder_Setup.Size = new System.Drawing.Size(261, 45);
            this.btn_Save_Barcoder_Setup.TabIndex = 26;
            this.btn_Save_Barcoder_Setup.TabStop = false;
            this.btn_Save_Barcoder_Setup.Text = "Save";
            this.btn_Save_Barcoder_Setup.UseVisualStyleBackColor = false;
            // 
            // propertyCollectionView
            // 
            this.propertyCollectionView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.propertyCollectionView.Controls.Add(this.individualMenuButton3);
            this.propertyCollectionView.FastBuild = true;
            this.propertyCollectionView.GroupName = "Property";
            this.propertyCollectionView.Location = new System.Drawing.Point(3, 3);
            this.propertyCollectionView.Name = "propertyCollectionView";
            this.propertyCollectionView.Size = new System.Drawing.Size(261, 496);
            this.propertyCollectionView.SuppressResizeInvalidation = true;
            this.propertyCollectionView.TabIndex = 20;
            // 
            // individualMenuButton3
            // 
            this.individualMenuButton3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.individualMenuButton3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.individualMenuButton3.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.individualMenuButton3.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.individualMenuButton3.CustomForeColor = System.Drawing.Color.Black;
            this.individualMenuButton3.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.individualMenuButton3.ForeColor = System.Drawing.Color.Black;
            this.individualMenuButton3.ImageSize = new System.Drawing.Size(45, 45);
            this.individualMenuButton3.Location = new System.Drawing.Point(330, 312);
            this.individualMenuButton3.Name = "individualMenuButton3";
            this.individualMenuButton3.Size = new System.Drawing.Size(100, 40);
            this.individualMenuButton3.TabIndex = 5;
            this.individualMenuButton3.TabStop = false;
            this.individualMenuButton3.Text = "Save";
            this.individualMenuButton3.UseVisualStyleBackColor = false;
            // 
            // BarcoderControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "BarcoderControl";
            this.Size = new System.Drawing.Size(420, 745);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.tableLayoutPanel9.ResumeLayout(false);
            this.tableLayoutPanel9.PerformLayout();
            this.tableLayoutPanel10.ResumeLayout(false);
            this.propertyCollectionView.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private Common.PropertyCollectionView propertyCollectionView;
        private Common.IndividualMenuButton individualMenuButton3;
        private Common.IndividualMenuButton btn_Save_Barcoder_Setup;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel9;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel10;
        private Common.IndividualMenuButton btnBarcoderScan;
        private System.Windows.Forms.Label labelBarcoderData;
        private Common.ListBoxItemsView listBoxItemsView;
    }
}
