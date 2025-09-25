using QMC.LCP_280.Process.Component;

namespace QMC.LCP_280.Process.Unit.FormWork
{
    partial class InputWafer_Working
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
            this.groupBoxCassetteLifterSeq = new System.Windows.Forms.GroupBox();
            this.manualSequenceControlCassette = new QMC.LCP_280.Process.Component.ManualSequenceControl();
            this.groupBoxManual = new System.Windows.Forms.GroupBox();
            this.checkBoxSimulation = new System.Windows.Forms.CheckBox();
            this.checkBoxTest = new System.Windows.Forms.CheckBox();
            this.waferMapView = new QMC.LCP_280.Process.Component.WaferMapView();
            this.btnMapping = new QMC.Common.IndividualMenuButton();
            this.groupBoxInputStageSeq = new System.Windows.Forms.GroupBox();
            this.manualSequenceControlInputStage = new QMC.LCP_280.Process.Component.ManualSequenceControl();
            this.groupBoxImageView = new System.Windows.Forms.GroupBox();
            this._InputWaferCameraviewer = new QMC.Common.Vision.VisionImageViewer();
            this.dioControl = new QMC.LCP_280.Process.Component.DIOControl();
            this.teachingPositionControl = new QMC.LCP_280.Process.Component.TeachingPositionControl();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBoxCassetteLifterSeq.SuspendLayout();
            this.groupBoxManual.SuspendLayout();
            this.groupBoxInputStageSeq.SuspendLayout();
            this.groupBoxImageView.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._InputWaferCameraviewer)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxCassetteLifterSeq
            // 
            this.groupBoxCassetteLifterSeq.Controls.Add(this.manualSequenceControlCassette);
            this.groupBoxCassetteLifterSeq.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxCassetteLifterSeq.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold);
            this.groupBoxCassetteLifterSeq.Location = new System.Drawing.Point(886, 2);
            this.groupBoxCassetteLifterSeq.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBoxCassetteLifterSeq.Name = "groupBoxCassetteLifterSeq";
            this.groupBoxCassetteLifterSeq.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBoxCassetteLifterSeq.Size = new System.Drawing.Size(376, 371);
            this.groupBoxCassetteLifterSeq.TabIndex = 18;
            this.groupBoxCassetteLifterSeq.TabStop = false;
            this.groupBoxCassetteLifterSeq.Text = "Input Wafer Seq";
            // 
            // manualSequenceControlCassette
            // 
            this.manualSequenceControlCassette.Dock = System.Windows.Forms.DockStyle.Top;
            this.manualSequenceControlCassette.Location = new System.Drawing.Point(2, 21);
            this.manualSequenceControlCassette.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.manualSequenceControlCassette.MinimumSize = new System.Drawing.Size(238, 200);
            this.manualSequenceControlCassette.Name = "manualSequenceControlCassette";
            this.manualSequenceControlCassette.ParentUnit = null;
            this.manualSequenceControlCassette.Size = new System.Drawing.Size(372, 253);
            this.manualSequenceControlCassette.TabIndex = 13;
            // 
            // groupBoxManual
            // 
            this.groupBoxManual.Controls.Add(this.tableLayoutPanel2);
            this.groupBoxManual.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxManual.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold);
            this.groupBoxManual.Location = new System.Drawing.Point(507, 377);
            this.groupBoxManual.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBoxManual.Name = "groupBoxManual";
            this.groupBoxManual.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBoxManual.Size = new System.Drawing.Size(375, 372);
            this.groupBoxManual.TabIndex = 19;
            this.groupBoxManual.TabStop = false;
            this.groupBoxManual.Text = "Manual";
            // 
            // checkBoxSimulation
            // 
            this.checkBoxSimulation.AutoSize = true;
            this.checkBoxSimulation.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBoxSimulation.Location = new System.Drawing.Point(2, 34);
            this.checkBoxSimulation.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.checkBoxSimulation.Name = "checkBoxSimulation";
            this.checkBoxSimulation.Size = new System.Drawing.Size(175, 28);
            this.checkBoxSimulation.TabIndex = 19;
            this.checkBoxSimulation.Text = "Simulation";
            this.checkBoxSimulation.UseVisualStyleBackColor = true;
            this.checkBoxSimulation.Visible = false;
            this.checkBoxSimulation.CheckedChanged += new System.EventHandler(this.checkBoxSimulation_CheckedChanged);
            // 
            // checkBoxTest
            // 
            this.checkBoxTest.AutoSize = true;
            this.checkBoxTest.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBoxTest.Location = new System.Drawing.Point(2, 2);
            this.checkBoxTest.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.checkBoxTest.Name = "checkBoxTest";
            this.checkBoxTest.Size = new System.Drawing.Size(175, 28);
            this.checkBoxTest.TabIndex = 18;
            this.checkBoxTest.Text = "DryRun";
            this.checkBoxTest.UseVisualStyleBackColor = true;
            this.checkBoxTest.Visible = false;
            this.checkBoxTest.CheckedChanged += new System.EventHandler(this.checkBoxTest_CheckedChanged);
            // 
            // waferMapView
            // 
            this.waferMapView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.waferMapView.Location = new System.Drawing.Point(2, 3);
            this.waferMapView.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.waferMapView.Name = "waferMapView";
            this.waferMapView.Size = new System.Drawing.Size(175, 231);
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
            this.btnMapping.Location = new System.Drawing.Point(2, 240);
            this.btnMapping.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btnMapping.Name = "btnMapping";
            this.btnMapping.Size = new System.Drawing.Size(175, 30);
            this.btnMapping.TabIndex = 17;
            this.btnMapping.TabStop = false;
            this.btnMapping.Text = "Mapping";
            this.btnMapping.UseVisualStyleBackColor = false;
            this.btnMapping.Click += new System.EventHandler(this.btnMapping_Click);
            // 
            // groupBoxInputStageSeq
            // 
            this.groupBoxInputStageSeq.Controls.Add(this.manualSequenceControlInputStage);
            this.groupBoxInputStageSeq.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxInputStageSeq.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold);
            this.groupBoxInputStageSeq.Location = new System.Drawing.Point(886, 377);
            this.groupBoxInputStageSeq.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBoxInputStageSeq.Name = "groupBoxInputStageSeq";
            this.groupBoxInputStageSeq.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBoxInputStageSeq.Size = new System.Drawing.Size(376, 372);
            this.groupBoxInputStageSeq.TabIndex = 19;
            this.groupBoxInputStageSeq.TabStop = false;
            this.groupBoxInputStageSeq.Text = "InputStage Manual Seq";
            // 
            // manualSequenceControlInputStage
            // 
            this.manualSequenceControlInputStage.Dock = System.Windows.Forms.DockStyle.Top;
            this.manualSequenceControlInputStage.Location = new System.Drawing.Point(2, 21);
            this.manualSequenceControlInputStage.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.manualSequenceControlInputStage.MinimumSize = new System.Drawing.Size(238, 200);
            this.manualSequenceControlInputStage.Name = "manualSequenceControlInputStage";
            this.manualSequenceControlInputStage.ParentUnit = null;
            this.manualSequenceControlInputStage.Size = new System.Drawing.Size(372, 253);
            this.manualSequenceControlInputStage.TabIndex = 13;
            // 
            // groupBoxImageView
            // 
            this.groupBoxImageView.Controls.Add(this._InputWaferCameraviewer);
            this.groupBoxImageView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxImageView.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.groupBoxImageView.Location = new System.Drawing.Point(507, 2);
            this.groupBoxImageView.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBoxImageView.Name = "groupBoxImageView";
            this.groupBoxImageView.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBoxImageView.Size = new System.Drawing.Size(375, 371);
            this.groupBoxImageView.TabIndex = 20;
            this.groupBoxImageView.TabStop = false;
            this.groupBoxImageView.Text = "ImageView";
            // 
            // _InputWaferCameraviewer
            // 
            this._InputWaferCameraviewer.BackColor = System.Drawing.Color.Black;
            this._InputWaferCameraviewer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._InputWaferCameraviewer.Camera = null;
            this._InputWaferCameraviewer.CameraSwitch = null;
            this._InputWaferCameraviewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._InputWaferCameraviewer.FrameRate = 1D;
            this._InputWaferCameraviewer.InputImage = null;
            this._InputWaferCameraviewer.IsViewCustomizedImage = false;
            this._InputWaferCameraviewer.Location = new System.Drawing.Point(2, 21);
            this._InputWaferCameraviewer.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this._InputWaferCameraviewer.Name = "_InputWaferCameraviewer";
            this._InputWaferCameraviewer.OperatingType = QMC.Common.Vision.VisionImageViewer.OperatingTypes.Center;
            this._InputWaferCameraviewer.Simulated = false;
            this._InputWaferCameraviewer.Size = new System.Drawing.Size(371, 348);
            this._InputWaferCameraviewer.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this._InputWaferCameraviewer.TabIndex = 12;
            this._InputWaferCameraviewer.TabStop = false;
            this._InputWaferCameraviewer.UpdateDelayTime = 80;
            this._InputWaferCameraviewer.VisibleCrossLine = true;
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
            this.teachingPositionControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)));
            this.teachingPositionControl.ButtonSize = new System.Drawing.Size(90, 32);
            this.teachingPositionControl.Location = new System.Drawing.Point(2, 4);
            this.teachingPositionControl.Margin = new System.Windows.Forms.Padding(2, 4, 2, 4);
            this.teachingPositionControl.Name = "teachingPositionControl";
            this.teachingPositionControl.ShowCancelButton = true;
            this.teachingPositionControl.ShowSaveButton = true;
            this.teachingPositionControl.Size = new System.Drawing.Size(492, 367);
            this.teachingPositionControl.TabIndex = 14;
            this.teachingPositionControl.UnitName = null;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel1.Controls.Add(this.teachingPositionControl, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.groupBoxInputStageSeq, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.groupBoxImageView, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.groupBoxManual, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.groupBoxCassetteLifterSeq, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.dioControl, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1264, 751);
            this.tableLayoutPanel1.TabIndex = 21;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel4, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel3, 0, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(2, 21);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 80F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(371, 349);
            this.tableLayoutPanel2.TabIndex = 22;
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
            this.tableLayoutPanel4.Size = new System.Drawing.Size(179, 273);
            this.tableLayoutPanel4.TabIndex = 18;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 1;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Controls.Add(this.checkBoxTest, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.checkBoxSimulation, 0, 1);
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 282);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 2;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(179, 64);
            this.tableLayoutPanel3.TabIndex = 20;
            // 
            // InputWafer_Working
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1264, 751);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.Name = "InputWafer_Working";
            this.Text = "InputWafer_Working";
            this.groupBoxCassetteLifterSeq.ResumeLayout(false);
            this.groupBoxManual.ResumeLayout(false);
            this.groupBoxInputStageSeq.ResumeLayout(false);
            this.groupBoxImageView.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._InputWaferCameraviewer)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private Component.DIOControl dioControl;
        private Component.TeachingPositionControl teachingPositionControl;
        private Component.ManualSequenceControl manualSequenceControlCassette;
        private WaferMapView waferMapView;
        private Common.IndividualMenuButton btnMapping;
        private System.Windows.Forms.GroupBox groupBoxCassetteLifterSeq;
        private System.Windows.Forms.GroupBox groupBoxManual;
        private System.Windows.Forms.GroupBox groupBoxInputStageSeq;
        private ManualSequenceControl manualSequenceControlInputStage;
        private System.Windows.Forms.GroupBox groupBoxImageView;
        private Common.Vision.VisionImageViewer _InputWaferCameraviewer;
        private System.Windows.Forms.CheckBox checkBoxTest;
        private System.Windows.Forms.CheckBox checkBoxSimulation;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
    }
}