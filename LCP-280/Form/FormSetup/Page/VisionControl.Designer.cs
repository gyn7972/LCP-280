namespace QMC.LCP_280.Process.Unit.FormSetup
{
    partial class VisionControl
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
            this.components = new System.ComponentModel.Container();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBoxImageView = new System.Windows.Forms.GroupBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.visionImageViewer = new QMC.Common.Vision.VisionImageViewer();
            this.btn_JogPopup = new QMC.Common.IndividualMenuButton();
            this.btn_Save_Camera_Setup = new QMC.Common.IndividualMenuButton();
            this.btn_Camera_Setup = new QMC.Common.IndividualMenuButton();
            this.cameraPropertyCollectionView = new QMC.Common.PropertyCollectionView();
            this.btn_Save_Setup_Cylinder = new QMC.Common.IndividualMenuButton();
            this.cameraListBoxItemsView = new QMC.Common.ListBoxItemsView();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.groupBoxImageView.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.panel2.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.visionImageViewer)).BeginInit();
            this.cameraPropertyCollectionView.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 0.9049774F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 99.09502F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel3, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(884, 1117);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 1;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.groupBoxImageView, 0, 2);
            this.tableLayoutPanel3.Controls.Add(this.panel1, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.panel2, 0, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(12, 5);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 3;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 39.02439F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 6.865402F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 54.11021F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(868, 1107);
            this.tableLayoutPanel3.TabIndex = 2;
            // 
            // groupBoxImageView
            // 
            this.groupBoxImageView.Controls.Add(this.visionImageViewer);
            this.groupBoxImageView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxImageView.Location = new System.Drawing.Point(4, 513);
            this.groupBoxImageView.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBoxImageView.Name = "groupBoxImageView";
            this.groupBoxImageView.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBoxImageView.Size = new System.Drawing.Size(860, 589);
            this.groupBoxImageView.TabIndex = 26;
            this.groupBoxImageView.TabStop = false;
            this.groupBoxImageView.Text = "ImageView";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.tableLayoutPanel4);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 435);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(862, 70);
            this.panel1.TabIndex = 27;
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 3;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel4.Controls.Add(this.btn_JogPopup, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.btn_Save_Camera_Setup, 2, 0);
            this.tableLayoutPanel4.Controls.Add(this.btn_Camera_Setup, 1, 0);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 1;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(862, 70);
            this.tableLayoutPanel4.TabIndex = 0;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.tableLayoutPanel5);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(3, 3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(862, 426);
            this.panel2.TabIndex = 28;
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 2;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.99072F));
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 66.00928F));
            this.tableLayoutPanel5.Controls.Add(this.cameraPropertyCollectionView, 1, 0);
            this.tableLayoutPanel5.Controls.Add(this.cameraListBoxItemsView, 0, 0);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 1;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(862, 426);
            this.tableLayoutPanel5.TabIndex = 0;
            // 
            // visionImageViewer
            // 
            this.visionImageViewer.BackColor = System.Drawing.Color.Black;
            this.visionImageViewer.Camera = null;
            this.visionImageViewer.CameraSwitch = null;
            this.visionImageViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.visionImageViewer.FrameRate = 1D;
            this.visionImageViewer.InputImage = null;
            this.visionImageViewer.IsViewCustomizedImage = false;
            this.visionImageViewer.Location = new System.Drawing.Point(4, 26);
            this.visionImageViewer.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.visionImageViewer.Name = "visionImageViewer";
            this.visionImageViewer.OperatingType = QMC.Common.Vision.VisionImageViewer.OperatingTypes.Center;
            this.visionImageViewer.Simulated = false;
            this.visionImageViewer.Size = new System.Drawing.Size(852, 558);
            this.visionImageViewer.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.visionImageViewer.TabIndex = 16;
            this.visionImageViewer.TabStop = false;
            this.visionImageViewer.UpdateDelayTime = 80;
            this.visionImageViewer.VisibleCrossLine = true;
            // 
            // btn_JogPopup
            // 
            this.btn_JogPopup.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_JogPopup.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_JogPopup.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_JogPopup.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_JogPopup.CustomForeColor = System.Drawing.Color.Black;
            this.btn_JogPopup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_JogPopup.Enabled = false;
            this.btn_JogPopup.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_JogPopup.ForeColor = System.Drawing.Color.Black;
            this.btn_JogPopup.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_JogPopup.Location = new System.Drawing.Point(4, 5);
            this.btn_JogPopup.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btn_JogPopup.Name = "btn_JogPopup";
            this.btn_JogPopup.Size = new System.Drawing.Size(279, 60);
            this.btn_JogPopup.TabIndex = 26;
            this.btn_JogPopup.TabStop = false;
            this.btn_JogPopup.Text = "Axis Jog";
            this.btn_JogPopup.UseVisualStyleBackColor = false;
            this.btn_JogPopup.Visible = false;
            // 
            // btn_Save_Camera_Setup
            // 
            this.btn_Save_Camera_Setup.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Save_Camera_Setup.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Save_Camera_Setup.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Save_Camera_Setup.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Save_Camera_Setup.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Save_Camera_Setup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_Save_Camera_Setup.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Save_Camera_Setup.ForeColor = System.Drawing.Color.Black;
            this.btn_Save_Camera_Setup.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Save_Camera_Setup.Location = new System.Drawing.Point(578, 5);
            this.btn_Save_Camera_Setup.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btn_Save_Camera_Setup.Name = "btn_Save_Camera_Setup";
            this.btn_Save_Camera_Setup.Size = new System.Drawing.Size(280, 60);
            this.btn_Save_Camera_Setup.TabIndex = 23;
            this.btn_Save_Camera_Setup.TabStop = false;
            this.btn_Save_Camera_Setup.Text = "Save";
            this.btn_Save_Camera_Setup.UseVisualStyleBackColor = false;
            // 
            // btn_Camera_Setup
            // 
            this.btn_Camera_Setup.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Camera_Setup.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Camera_Setup.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Camera_Setup.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Camera_Setup.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Camera_Setup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_Camera_Setup.Enabled = false;
            this.btn_Camera_Setup.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Camera_Setup.ForeColor = System.Drawing.Color.Black;
            this.btn_Camera_Setup.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Camera_Setup.Location = new System.Drawing.Point(291, 5);
            this.btn_Camera_Setup.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btn_Camera_Setup.Name = "btn_Camera_Setup";
            this.btn_Camera_Setup.Size = new System.Drawing.Size(279, 60);
            this.btn_Camera_Setup.TabIndex = 27;
            this.btn_Camera_Setup.TabStop = false;
            this.btn_Camera_Setup.Text = "Set up";
            this.btn_Camera_Setup.UseVisualStyleBackColor = false;
            this.btn_Camera_Setup.Visible = false;
            // 
            // cameraPropertyCollectionView
            // 
            this.cameraPropertyCollectionView.Controls.Add(this.btn_Save_Setup_Cylinder);
            this.cameraPropertyCollectionView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cameraPropertyCollectionView.FastBuild = true;
            this.cameraPropertyCollectionView.GroupName = "Property";
            this.cameraPropertyCollectionView.Location = new System.Drawing.Point(299, 6);
            this.cameraPropertyCollectionView.Margin = new System.Windows.Forms.Padding(6);
            this.cameraPropertyCollectionView.Name = "cameraPropertyCollectionView";
            this.cameraPropertyCollectionView.Size = new System.Drawing.Size(557, 414);
            this.cameraPropertyCollectionView.SuppressResizeInvalidation = true;
            this.cameraPropertyCollectionView.TabIndex = 13;
            this.cameraPropertyCollectionView.TextBoxFont = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold);
            this.cameraPropertyCollectionView.TextBoxFontSize = 10F;
            // 
            // btn_Save_Setup_Cylinder
            // 
            this.btn_Save_Setup_Cylinder.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Save_Setup_Cylinder.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Save_Setup_Cylinder.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Save_Setup_Cylinder.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Save_Setup_Cylinder.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Save_Setup_Cylinder.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Save_Setup_Cylinder.ForeColor = System.Drawing.Color.Black;
            this.btn_Save_Setup_Cylinder.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Save_Setup_Cylinder.Location = new System.Drawing.Point(330, 312);
            this.btn_Save_Setup_Cylinder.Name = "btn_Save_Setup_Cylinder";
            this.btn_Save_Setup_Cylinder.Size = new System.Drawing.Size(100, 40);
            this.btn_Save_Setup_Cylinder.TabIndex = 5;
            this.btn_Save_Setup_Cylinder.TabStop = false;
            this.btn_Save_Setup_Cylinder.Text = "Save";
            this.btn_Save_Setup_Cylinder.UseVisualStyleBackColor = false;
            // 
            // cameraListBoxItemsView
            // 
            this.cameraListBoxItemsView.BorderColor = System.Drawing.Color.White;
            this.cameraListBoxItemsView.BorderWidth = 2;
            this.cameraListBoxItemsView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cameraListBoxItemsView.GroupBackColor = System.Drawing.Color.White;
            this.cameraListBoxItemsView.GroupForeColor = System.Drawing.Color.Black;
            this.cameraListBoxItemsView.GroupName = "Camera";
            this.cameraListBoxItemsView.ItemBackColor = System.Drawing.Color.Black;
            this.cameraListBoxItemsView.ItemForeColor = System.Drawing.Color.Lime;
            this.cameraListBoxItemsView.ListBackColor = System.Drawing.Color.Black;
            this.cameraListBoxItemsView.ListForeColor = System.Drawing.Color.Lime;
            this.cameraListBoxItemsView.Location = new System.Drawing.Point(4, 10);
            this.cameraListBoxItemsView.Margin = new System.Windows.Forms.Padding(4, 10, 4, 10);
            this.cameraListBoxItemsView.Name = "cameraListBoxItemsView";
            this.cameraListBoxItemsView.SelectedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(255)))), ((int)(((byte)(0)))));
            this.cameraListBoxItemsView.SelectedForeColor = System.Drawing.Color.Black;
            this.cameraListBoxItemsView.SelectedIndex = -1;
            this.cameraListBoxItemsView.Size = new System.Drawing.Size(285, 406);
            this.cameraListBoxItemsView.TabIndex = 3;
            // 
            // VisionControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "VisionControl";
            this.Size = new System.Drawing.Size(884, 1117);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.groupBoxImageView.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.tableLayoutPanel5.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.visionImageViewer)).EndInit();
            this.cameraPropertyCollectionView.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Common.ListBoxItemsView cameraListBoxItemsView;
        private Common.IndividualMenuButton btn_JogPopup;
        private Common.IndividualMenuButton btn_Camera_Setup;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.GroupBox groupBoxImageView;
        private Common.Vision.VisionImageViewer visionImageViewer;
        private Common.PropertyCollectionView cameraPropertyCollectionView;
        private Common.IndividualMenuButton btn_Save_Setup_Cylinder;
        private Common.IndividualMenuButton btn_Save_Camera_Setup;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
    }
}
