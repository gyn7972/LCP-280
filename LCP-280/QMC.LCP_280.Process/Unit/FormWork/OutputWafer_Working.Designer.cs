namespace QMC.LCP_280.Process.Unit.FormWork
{
    partial class OutputWafer_Working
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.dioControl = new QMC.LCP_280.Process.Component.DIOControl();
            this.teachingPositionControl = new QMC.LCP_280.Process.Component.TeachingPositionControl();
            this.manualSequenceControlCassette = new QMC.LCP_280.Process.Component.ManualSequenceControl();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBoxImageView = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this._OutputWaferCameraviewer = new QMC.Common.Vision.VisionImageViewer();
            this.groupBoxManual = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.waferMapView = new QMC.LCP_280.Process.Component.WaferMapView();
            this.btnMapping = new QMC.Common.IndividualMenuButton();
            this.groupBoxOutputStageSeq = new System.Windows.Forms.GroupBox();
            this.manualSequenceControlOutputStage = new QMC.LCP_280.Process.Component.ManualSequenceControl();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox1.SuspendLayout();
            this.groupBoxImageView.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._OutputWaferCameraviewer)).BeginInit();
            this.groupBoxManual.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.groupBoxOutputStageSeq.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dioControl
            // 
            this.dioControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dioControl.IoSortMode = QMC.LCP_280.Process.Component.DIOControl.SortingMode.AlphabeticalKey;
            this.dioControl.Location = new System.Drawing.Point(2, 379);
            this.dioControl.Margin = new System.Windows.Forms.Padding(2, 4, 2, 4);
            this.dioControl.Name = "dioControl";
            this.dioControl.RefreshIntervalMs = 400;
            this.dioControl.Size = new System.Drawing.Size(501, 368);
            this.dioControl.TabIndex = 15;
            // 
            // teachingPositionControl
            // 
            this.teachingPositionControl.ButtonSize = new System.Drawing.Size(90, 32);
            this.teachingPositionControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.teachingPositionControl.Location = new System.Drawing.Point(2, 4);
            this.teachingPositionControl.Margin = new System.Windows.Forms.Padding(2, 4, 2, 4);
            this.teachingPositionControl.Name = "teachingPositionControl";
            this.teachingPositionControl.ShowCancelButton = true;
            this.teachingPositionControl.ShowSaveButton = true;
            this.teachingPositionControl.Size = new System.Drawing.Size(501, 367);
            this.teachingPositionControl.TabIndex = 14;
            this.teachingPositionControl.UnitName = null;
            // 
            // manualSequenceControlCassette
            // 
            this.manualSequenceControlCassette.Dock = System.Windows.Forms.DockStyle.Fill;
            this.manualSequenceControlCassette.Location = new System.Drawing.Point(2, 20);
            this.manualSequenceControlCassette.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.manualSequenceControlCassette.MinimumSize = new System.Drawing.Size(238, 200);
            this.manualSequenceControlCassette.Name = "manualSequenceControlCassette";
            this.manualSequenceControlCassette.ParentUnit = null;
            this.manualSequenceControlCassette.Size = new System.Drawing.Size(372, 349);
            this.manualSequenceControlCassette.TabIndex = 13;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.manualSequenceControlCassette);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Font = new System.Drawing.Font("맑은 고딕", 10F);
            this.groupBox1.Location = new System.Drawing.Point(886, 2);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox1.Size = new System.Drawing.Size(376, 371);
            this.groupBox1.TabIndex = 16;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Output Wafer Seq";
            // 
            // groupBoxImageView
            // 
            this.groupBoxImageView.Controls.Add(this.tableLayoutPanel5);
            this.groupBoxImageView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxImageView.Font = new System.Drawing.Font("맑은 고딕", 10F);
            this.groupBoxImageView.Location = new System.Drawing.Point(507, 2);
            this.groupBoxImageView.Margin = new System.Windows.Forms.Padding(2);
            this.groupBoxImageView.Name = "groupBoxImageView";
            this.groupBoxImageView.Padding = new System.Windows.Forms.Padding(2);
            this.groupBoxImageView.Size = new System.Drawing.Size(375, 371);
            this.groupBoxImageView.TabIndex = 22;
            this.groupBoxImageView.TabStop = false;
            this.groupBoxImageView.Text = "ImageView";
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 1;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.Controls.Add(this._OutputWaferCameraviewer, 0, 0);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(2, 20);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 2;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 80F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(371, 349);
            this.tableLayoutPanel5.TabIndex = 1;
            // 
            // _OutputWaferCameraviewer
            // 
            this._OutputWaferCameraviewer.BackColor = System.Drawing.Color.Black;
            this._OutputWaferCameraviewer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._OutputWaferCameraviewer.Camera = null;
            this._OutputWaferCameraviewer.CameraSwitch = null;
            this._OutputWaferCameraviewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._OutputWaferCameraviewer.FrameRate = 1D;
            this._OutputWaferCameraviewer.InputImage = null;
            this._OutputWaferCameraviewer.IsViewCustomizedImage = false;
            this._OutputWaferCameraviewer.Location = new System.Drawing.Point(2, 2);
            this._OutputWaferCameraviewer.Margin = new System.Windows.Forms.Padding(2);
            this._OutputWaferCameraviewer.Name = "_OutputWaferCameraviewer";
            this._OutputWaferCameraviewer.OperatingType = QMC.Common.Vision.VisionImageViewer.OperatingTypes.Center;
            this._OutputWaferCameraviewer.Simulated = false;
            this._OutputWaferCameraviewer.Size = new System.Drawing.Size(367, 275);
            this._OutputWaferCameraviewer.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this._OutputWaferCameraviewer.TabIndex = 12;
            this._OutputWaferCameraviewer.TabStop = false;
            this._OutputWaferCameraviewer.UpdateDelayTime = 80;
            this._OutputWaferCameraviewer.VisibleCrossLine = true;
            // 
            // groupBoxManual
            // 
            this.groupBoxManual.Controls.Add(this.tableLayoutPanel2);
            this.groupBoxManual.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxManual.Font = new System.Drawing.Font("맑은 고딕", 10F);
            this.groupBoxManual.Location = new System.Drawing.Point(507, 377);
            this.groupBoxManual.Margin = new System.Windows.Forms.Padding(2);
            this.groupBoxManual.Name = "groupBoxManual";
            this.groupBoxManual.Padding = new System.Windows.Forms.Padding(2);
            this.groupBoxManual.Size = new System.Drawing.Size(375, 372);
            this.groupBoxManual.TabIndex = 21;
            this.groupBoxManual.TabStop = false;
            this.groupBoxManual.Text = "Manual";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel4, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(2, 20);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 80F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(371, 350);
            this.tableLayoutPanel2.TabIndex = 21;
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 1;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel4.Controls.Add(this.waferMapView, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.btnMapping, 0, 1);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 2;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 87F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 13F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(179, 274);
            this.tableLayoutPanel4.TabIndex = 18;
            // 
            // waferMapView
            // 
            this.waferMapView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.waferMapView.Location = new System.Drawing.Point(2, 3);
            this.waferMapView.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.waferMapView.Name = "waferMapView";
            this.waferMapView.Size = new System.Drawing.Size(175, 232);
            this.waferMapView.TabIndex = 16;
            // 
            // btnMapping
            // 
            this.btnMapping.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnMapping.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnMapping.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnMapping.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnMapping.CustomForeColor = System.Drawing.Color.Black;
            this.btnMapping.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnMapping.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnMapping.ForeColor = System.Drawing.Color.Black;
            this.btnMapping.ImageSize = new System.Drawing.Size(45, 45);
            this.btnMapping.Location = new System.Drawing.Point(2, 241);
            this.btnMapping.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btnMapping.Name = "btnMapping";
            this.btnMapping.Size = new System.Drawing.Size(175, 30);
            this.btnMapping.TabIndex = 17;
            this.btnMapping.TabStop = false;
            this.btnMapping.Text = "Mapping";
            this.btnMapping.UseVisualStyleBackColor = false;
            // 
            // groupBoxOutputStageSeq
            // 
            this.groupBoxOutputStageSeq.Controls.Add(this.manualSequenceControlOutputStage);
            this.groupBoxOutputStageSeq.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxOutputStageSeq.Font = new System.Drawing.Font("맑은 고딕", 10F);
            this.groupBoxOutputStageSeq.Location = new System.Drawing.Point(886, 377);
            this.groupBoxOutputStageSeq.Margin = new System.Windows.Forms.Padding(2);
            this.groupBoxOutputStageSeq.Name = "groupBoxOutputStageSeq";
            this.groupBoxOutputStageSeq.Padding = new System.Windows.Forms.Padding(2);
            this.groupBoxOutputStageSeq.Size = new System.Drawing.Size(376, 372);
            this.groupBoxOutputStageSeq.TabIndex = 23;
            this.groupBoxOutputStageSeq.TabStop = false;
            this.groupBoxOutputStageSeq.Text = "OutputStage Manual Seq";
            // 
            // manualSequenceControlOutputStage
            // 
            this.manualSequenceControlOutputStage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.manualSequenceControlOutputStage.Location = new System.Drawing.Point(2, 20);
            this.manualSequenceControlOutputStage.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.manualSequenceControlOutputStage.MinimumSize = new System.Drawing.Size(238, 200);
            this.manualSequenceControlOutputStage.Name = "manualSequenceControlOutputStage";
            this.manualSequenceControlOutputStage.ParentUnit = null;
            this.manualSequenceControlOutputStage.Size = new System.Drawing.Size(372, 350);
            this.manualSequenceControlOutputStage.TabIndex = 13;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel1.Controls.Add(this.groupBoxImageView, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.teachingPositionControl, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.groupBoxOutputStageSeq, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.groupBoxManual, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.dioControl, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1264, 751);
            this.tableLayoutPanel1.TabIndex = 24;
            // 
            // OutputWafer_Working
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1264, 751);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.Name = "OutputWafer_Working";
            this.Text = "WaferBin_Working";
            this.groupBox1.ResumeLayout(false);
            this.groupBoxImageView.ResumeLayout(false);
            this.tableLayoutPanel5.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._OutputWaferCameraviewer)).EndInit();
            this.groupBoxManual.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.groupBoxOutputStageSeq.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private Component.DIOControl dioControl;
        private Component.TeachingPositionControl teachingPositionControl;
        private Component.ManualSequenceControl manualSequenceControlCassette;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBoxImageView;
        private Common.Vision.VisionImageViewer _OutputWaferCameraviewer;
        private System.Windows.Forms.GroupBox groupBoxManual;
        private Component.WaferMapView waferMapView;
        private Common.IndividualMenuButton btnMapping;
        private System.Windows.Forms.GroupBox groupBoxOutputStageSeq;
        private Component.ManualSequenceControl manualSequenceControlOutputStage;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
    }
}