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
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.cameraListBoxItemsView = new QMC.Common.ListBoxItemsView();
            this.btn_JogPopup = new QMC.Common.IndividualMenuButton();
            this.btn_Camera_Setup = new QMC.Common.IndividualMenuButton();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBoxImageView = new System.Windows.Forms.GroupBox();
            this.visionImageViewer = new QMC.Common.Vision.VisionImageViewer();
            this.cameraPropertyCollectionView = new QMC.Common.PropertyCollectionView();
            this.btn_Save_Setup_Cylinder = new QMC.Common.IndividualMenuButton();
            this.btn_Save_Camera_Setup = new QMC.Common.IndividualMenuButton();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.groupBoxImageView.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.visionImageViewer)).BeginInit();
            this.cameraPropertyCollectionView.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel3, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(619, 745);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.btn_Camera_Setup, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.btn_JogPopup, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.cameraListBoxItemsView, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 3;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 7F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 23F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(179, 739);
            this.tableLayoutPanel2.TabIndex = 0;
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
            this.cameraListBoxItemsView.Location = new System.Drawing.Point(3, 6);
            this.cameraListBoxItemsView.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.cameraListBoxItemsView.Name = "cameraListBoxItemsView";
            this.cameraListBoxItemsView.SelectedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(255)))), ((int)(((byte)(0)))));
            this.cameraListBoxItemsView.SelectedForeColor = System.Drawing.Color.Black;
            this.cameraListBoxItemsView.SelectedIndex = -1;
            this.cameraListBoxItemsView.Size = new System.Drawing.Size(173, 505);
            this.cameraListBoxItemsView.TabIndex = 3;
            // 
            // btn_JogPopup
            // 
            this.btn_JogPopup.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_JogPopup.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_JogPopup.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_JogPopup.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_JogPopup.CustomForeColor = System.Drawing.Color.Black;
            this.btn_JogPopup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_JogPopup.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_JogPopup.ForeColor = System.Drawing.Color.Black;
            this.btn_JogPopup.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_JogPopup.Location = new System.Drawing.Point(3, 520);
            this.btn_JogPopup.Name = "btn_JogPopup";
            this.btn_JogPopup.Size = new System.Drawing.Size(173, 45);
            this.btn_JogPopup.TabIndex = 26;
            this.btn_JogPopup.TabStop = false;
            this.btn_JogPopup.Text = "Axis Jog";
            this.btn_JogPopup.UseVisualStyleBackColor = false;
            // 
            // btn_Camera_Setup
            // 
            this.btn_Camera_Setup.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Camera_Setup.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Camera_Setup.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Camera_Setup.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Camera_Setup.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Camera_Setup.Enabled = false;
            this.btn_Camera_Setup.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Camera_Setup.ForeColor = System.Drawing.Color.Black;
            this.btn_Camera_Setup.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Camera_Setup.Location = new System.Drawing.Point(3, 571);
            this.btn_Camera_Setup.Name = "btn_Camera_Setup";
            this.btn_Camera_Setup.Size = new System.Drawing.Size(83, 40);
            this.btn_Camera_Setup.TabIndex = 27;
            this.btn_Camera_Setup.TabStop = false;
            this.btn_Camera_Setup.Text = "Set up";
            this.btn_Camera_Setup.UseVisualStyleBackColor = false;
            this.btn_Camera_Setup.Visible = false;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 1;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.groupBoxImageView, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.cameraPropertyCollectionView, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.btn_Save_Camera_Setup, 0, 2);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(188, 3);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 3;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 43F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 7F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(428, 739);
            this.tableLayoutPanel3.TabIndex = 2;
            // 
            // groupBoxImageView
            // 
            this.groupBoxImageView.Controls.Add(this.visionImageViewer);
            this.groupBoxImageView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxImageView.Location = new System.Drawing.Point(3, 3);
            this.groupBoxImageView.Name = "groupBoxImageView";
            this.groupBoxImageView.Size = new System.Drawing.Size(422, 311);
            this.groupBoxImageView.TabIndex = 26;
            this.groupBoxImageView.TabStop = false;
            this.groupBoxImageView.Text = "ImageView";
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
            this.visionImageViewer.Location = new System.Drawing.Point(3, 17);
            this.visionImageViewer.Name = "visionImageViewer";
            this.visionImageViewer.OperatingType = QMC.Common.Vision.VisionImageViewer.OperatingTypes.Center;
            this.visionImageViewer.Simulated = false;
            this.visionImageViewer.Size = new System.Drawing.Size(416, 291);
            this.visionImageViewer.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.visionImageViewer.TabIndex = 16;
            this.visionImageViewer.TabStop = false;
            this.visionImageViewer.UpdateDelayTime = 80;
            this.visionImageViewer.VisibleCrossLine = true;
            // 
            // cameraPropertyCollectionView
            // 
            this.cameraPropertyCollectionView.Controls.Add(this.btn_Save_Setup_Cylinder);
            this.cameraPropertyCollectionView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cameraPropertyCollectionView.FastBuild = true;
            this.cameraPropertyCollectionView.GroupName = "Property";
            this.cameraPropertyCollectionView.Location = new System.Drawing.Point(3, 320);
            this.cameraPropertyCollectionView.Name = "cameraPropertyCollectionView";
            this.cameraPropertyCollectionView.Size = new System.Drawing.Size(422, 363);
            this.cameraPropertyCollectionView.SuppressResizeInvalidation = true;
            this.cameraPropertyCollectionView.TabIndex = 13;
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
            this.btn_Save_Camera_Setup.Location = new System.Drawing.Point(3, 689);
            this.btn_Save_Camera_Setup.Name = "btn_Save_Camera_Setup";
            this.btn_Save_Camera_Setup.Size = new System.Drawing.Size(422, 47);
            this.btn_Save_Camera_Setup.TabIndex = 23;
            this.btn_Save_Camera_Setup.TabStop = false;
            this.btn_Save_Camera_Setup.Text = "Save";
            this.btn_Save_Camera_Setup.UseVisualStyleBackColor = false;
            // 
            // VisionControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "VisionControl";
            this.Size = new System.Drawing.Size(619, 745);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.groupBoxImageView.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.visionImageViewer)).EndInit();
            this.cameraPropertyCollectionView.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private Common.ListBoxItemsView cameraListBoxItemsView;
        private Common.IndividualMenuButton btn_JogPopup;
        private Common.IndividualMenuButton btn_Camera_Setup;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.GroupBox groupBoxImageView;
        private Common.Vision.VisionImageViewer visionImageViewer;
        private Common.PropertyCollectionView cameraPropertyCollectionView;
        private Common.IndividualMenuButton btn_Save_Setup_Cylinder;
        private Common.IndividualMenuButton btn_Save_Camera_Setup;
    }
}
