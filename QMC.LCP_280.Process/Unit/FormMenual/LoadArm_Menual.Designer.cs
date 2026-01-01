using QMC.Common;
using QMC.Common.CustomControl;
using QMC.LCP_280.Process;
using QMC.LCP_280.Process.Unit;
using QMC.LCP_280.Process.Component; // added for TeachingPositionControl
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Unit
{
    partial class LoadArm_Menual
    {
        private IContainer components = null;

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.groupBoxImageView = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this._ChipLoadingCameraviewer = new QMC.Common.Vision.VisionImageViewer();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this._btnVisionSetting = new QMC.Common.IndividualMenuButton();
            this.buttonDataManual = new QMC.Common.IndividualMenuButton();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBoxManualTrSeq = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.manualSequenceControl = new QMC.LCP_280.Process.Component.ManualSequenceControl();
            this.groupBoxManual = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.buttonPickUpNiddle_Move = new QMC.Common.IndividualMenuButton();
            this.buttonTest2 = new QMC.Common.IndividualMenuButton();
            this.buttonTest = new QMC.Common.IndividualMenuButton();
            this.btnTest = new QMC.Common.IndividualMenuButton();
            this.btnDieUnloading = new QMC.Common.IndividualMenuButton();
            this.btnDieLoading = new QMC.Common.IndividualMenuButton();
            this.cbLoadIndex = new System.Windows.Forms.ComboBox();
            this.dioControl = new QMC.LCP_280.Process.Component.DIOControl();
            this.teachingPositionControl = new QMC.LCP_280.Process.Component.TeachingPositionControl();
            this.groupBoxImageView.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._ChipLoadingCameraviewer)).BeginInit();
            this.tableLayoutPanel5.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.groupBoxManualTrSeq.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.groupBoxManual.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxImageView
            // 
            this.groupBoxImageView.Controls.Add(this.tableLayoutPanel4);
            this.groupBoxImageView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxImageView.Font = new System.Drawing.Font("맑은 고딕", 10F);
            this.groupBoxImageView.Location = new System.Drawing.Point(760, 2);
            this.groupBoxImageView.Margin = new System.Windows.Forms.Padding(2);
            this.groupBoxImageView.Name = "groupBoxImageView";
            this.groupBoxImageView.Padding = new System.Windows.Forms.Padding(2);
            this.groupBoxImageView.Size = new System.Drawing.Size(564, 559);
            this.groupBoxImageView.TabIndex = 15;
            this.groupBoxImageView.TabStop = false;
            this.groupBoxImageView.Text = "ImageView";
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 1;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.Controls.Add(this._ChipLoadingCameraviewer, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.tableLayoutPanel5, 0, 1);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(2, 29);
            this.tableLayoutPanel4.Margin = new System.Windows.Forms.Padding(5);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 3;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 80F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(560, 528);
            this.tableLayoutPanel4.TabIndex = 15;
            // 
            // _ChipLoadingCameraviewer
            // 
            this._ChipLoadingCameraviewer.BackColor = System.Drawing.Color.Black;
            this._ChipLoadingCameraviewer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._ChipLoadingCameraviewer.Camera = null;
            this._ChipLoadingCameraviewer.CameraSwitch = null;
            this._ChipLoadingCameraviewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._ChipLoadingCameraviewer.FrameRate = 1D;
            this._ChipLoadingCameraviewer.InputImage = null;
            this._ChipLoadingCameraviewer.IsViewCustomizedImage = false;
            this._ChipLoadingCameraviewer.Location = new System.Drawing.Point(2, 2);
            this._ChipLoadingCameraviewer.Margin = new System.Windows.Forms.Padding(2);
            this._ChipLoadingCameraviewer.Name = "_ChipLoadingCameraviewer";
            this._ChipLoadingCameraviewer.OperatingType = QMC.Common.Vision.VisionImageViewer.OperatingTypes.Center;
            this._ChipLoadingCameraviewer.Simulated = false;
            this._ChipLoadingCameraviewer.Size = new System.Drawing.Size(556, 418);
            this._ChipLoadingCameraviewer.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this._ChipLoadingCameraviewer.TabIndex = 12;
            this._ChipLoadingCameraviewer.TabStop = false;
            this._ChipLoadingCameraviewer.UpdateDelayTime = 80;
            this._ChipLoadingCameraviewer.VisibleCrossLine = true;
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 2;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel5.Controls.Add(this._btnVisionSetting, 0, 0);
            this.tableLayoutPanel5.Controls.Add(this.buttonDataManual, 1, 0);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(5, 427);
            this.tableLayoutPanel5.Margin = new System.Windows.Forms.Padding(5);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 1;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(550, 69);
            this.tableLayoutPanel5.TabIndex = 15;
            // 
            // _btnVisionSetting
            // 
            this._btnVisionSetting.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._btnVisionSetting.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this._btnVisionSetting.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this._btnVisionSetting.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this._btnVisionSetting.CustomFont = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._btnVisionSetting.CustomForeColor = System.Drawing.Color.Black;
            this._btnVisionSetting.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._btnVisionSetting.ForeColor = System.Drawing.Color.Black;
            this._btnVisionSetting.ImageSize = new System.Drawing.Size(45, 45);
            this._btnVisionSetting.Location = new System.Drawing.Point(2, 2);
            this._btnVisionSetting.Margin = new System.Windows.Forms.Padding(2);
            this._btnVisionSetting.Name = "_btnVisionSetting";
            this._btnVisionSetting.Size = new System.Drawing.Size(271, 65);
            this._btnVisionSetting.TabIndex = 34;
            this._btnVisionSetting.TabStop = false;
            this._btnVisionSetting.Text = "VisionSetting";
            this._btnVisionSetting.UseVisualStyleBackColor = false;
            // 
            // buttonDataManual
            // 
            this.buttonDataManual.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDataManual.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.buttonDataManual.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.buttonDataManual.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.buttonDataManual.CustomFont = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonDataManual.CustomForeColor = System.Drawing.Color.Black;
            this.buttonDataManual.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonDataManual.ForeColor = System.Drawing.Color.Black;
            this.buttonDataManual.ImageSize = new System.Drawing.Size(45, 45);
            this.buttonDataManual.Location = new System.Drawing.Point(277, 2);
            this.buttonDataManual.Margin = new System.Windows.Forms.Padding(2);
            this.buttonDataManual.Name = "buttonDataManual";
            this.buttonDataManual.Size = new System.Drawing.Size(271, 65);
            this.buttonDataManual.TabIndex = 35;
            this.buttonDataManual.TabStop = false;
            this.buttonDataManual.Text = "ManualData";
            this.buttonDataManual.UseVisualStyleBackColor = false;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel1.Controls.Add(this.groupBoxManualTrSeq, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.groupBoxManual, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.dioControl, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.teachingPositionControl, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.groupBoxImageView, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(5);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1896, 1127);
            this.tableLayoutPanel1.TabIndex = 24;
            // 
            // groupBoxManualTrSeq
            // 
            this.groupBoxManualTrSeq.Controls.Add(this.tableLayoutPanel3);
            this.groupBoxManualTrSeq.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxManualTrSeq.Font = new System.Drawing.Font("맑은 고딕", 10F);
            this.groupBoxManualTrSeq.Location = new System.Drawing.Point(1328, 2);
            this.groupBoxManualTrSeq.Margin = new System.Windows.Forms.Padding(2);
            this.groupBoxManualTrSeq.Name = "groupBoxManualTrSeq";
            this.groupBoxManualTrSeq.Padding = new System.Windows.Forms.Padding(2);
            this.groupBoxManualTrSeq.Size = new System.Drawing.Size(566, 559);
            this.groupBoxManualTrSeq.TabIndex = 29;
            this.groupBoxManualTrSeq.TabStop = false;
            this.groupBoxManualTrSeq.Text = "ChipLoading Seq.";
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 1;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.manualSequenceControl, 0, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(2, 29);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(5);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(562, 528);
            this.tableLayoutPanel3.TabIndex = 20;
            // 
            // manualSequenceControl
            // 
            this.manualSequenceControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.manualSequenceControl.Location = new System.Drawing.Point(2, 2);
            this.manualSequenceControl.Margin = new System.Windows.Forms.Padding(2);
            this.manualSequenceControl.Name = "manualSequenceControl";
            this.manualSequenceControl.ParentUnit = null;
            this.manualSequenceControl.Size = new System.Drawing.Size(558, 524);
            this.manualSequenceControl.TabIndex = 19;
            // 
            // groupBoxManual
            // 
            this.groupBoxManual.Controls.Add(this.tableLayoutPanel2);
            this.groupBoxManual.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxManual.Font = new System.Drawing.Font("맑은 고딕", 10F);
            this.groupBoxManual.Location = new System.Drawing.Point(760, 565);
            this.groupBoxManual.Margin = new System.Windows.Forms.Padding(2);
            this.groupBoxManual.Name = "groupBoxManual";
            this.groupBoxManual.Padding = new System.Windows.Forms.Padding(2);
            this.groupBoxManual.Size = new System.Drawing.Size(564, 560);
            this.groupBoxManual.TabIndex = 25;
            this.groupBoxManual.TabStop = false;
            this.groupBoxManual.Text = "Manual";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 3;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel2.Controls.Add(this.buttonPickUpNiddle_Move, 2, 3);
            this.tableLayoutPanel2.Controls.Add(this.buttonTest2, 2, 2);
            this.tableLayoutPanel2.Controls.Add(this.buttonTest, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnTest, 2, 1);
            this.tableLayoutPanel2.Controls.Add(this.btnDieUnloading, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.btnDieLoading, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.cbLoadIndex, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(2, 29);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(5);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 10;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 11.11111F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 11.11111F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 11.11111F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 11.11111F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 11.11111F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 11.11111F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 11.11111F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 11.11111F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 11.11111F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(560, 529);
            this.tableLayoutPanel2.TabIndex = 34;
            // 
            // buttonPickUpNiddle_Move
            // 
            this.buttonPickUpNiddle_Move.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonPickUpNiddle_Move.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.buttonPickUpNiddle_Move.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.buttonPickUpNiddle_Move.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.buttonPickUpNiddle_Move.CustomFont = new System.Drawing.Font("Arial", 6.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonPickUpNiddle_Move.CustomForeColor = System.Drawing.Color.Black;
            this.buttonPickUpNiddle_Move.Font = new System.Drawing.Font("Arial", 6.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonPickUpNiddle_Move.ForeColor = System.Drawing.Color.Black;
            this.buttonPickUpNiddle_Move.ImageSize = new System.Drawing.Size(45, 45);
            this.buttonPickUpNiddle_Move.Location = new System.Drawing.Point(374, 167);
            this.buttonPickUpNiddle_Move.Margin = new System.Windows.Forms.Padding(2);
            this.buttonPickUpNiddle_Move.Name = "buttonPickUpNiddle_Move";
            this.buttonPickUpNiddle_Move.Size = new System.Drawing.Size(184, 51);
            this.buttonPickUpNiddle_Move.TabIndex = 36;
            this.buttonPickUpNiddle_Move.TabStop = false;
            this.buttonPickUpNiddle_Move.Text = "PickUp && Niddle Move";
            this.buttonPickUpNiddle_Move.UseVisualStyleBackColor = false;
            // 
            // buttonTest2
            // 
            this.buttonTest2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonTest2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.buttonTest2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.buttonTest2.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.buttonTest2.CustomFont = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonTest2.CustomForeColor = System.Drawing.Color.Black;
            this.buttonTest2.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonTest2.ForeColor = System.Drawing.Color.Black;
            this.buttonTest2.ImageSize = new System.Drawing.Size(45, 45);
            this.buttonTest2.Location = new System.Drawing.Point(374, 112);
            this.buttonTest2.Margin = new System.Windows.Forms.Padding(2);
            this.buttonTest2.Name = "buttonTest2";
            this.buttonTest2.Size = new System.Drawing.Size(184, 51);
            this.buttonTest2.TabIndex = 34;
            this.buttonTest2.TabStop = false;
            this.buttonTest2.Text = "Test2";
            this.buttonTest2.UseVisualStyleBackColor = false;
            // 
            // buttonTest
            // 
            this.buttonTest.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonTest.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.buttonTest.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.buttonTest.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.buttonTest.CustomFont = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonTest.CustomForeColor = System.Drawing.Color.Black;
            this.buttonTest.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonTest.ForeColor = System.Drawing.Color.Black;
            this.buttonTest.ImageSize = new System.Drawing.Size(45, 45);
            this.buttonTest.Location = new System.Drawing.Point(374, 2);
            this.buttonTest.Margin = new System.Windows.Forms.Padding(2);
            this.buttonTest.Name = "buttonTest";
            this.buttonTest.Size = new System.Drawing.Size(184, 51);
            this.buttonTest.TabIndex = 37;
            this.buttonTest.TabStop = false;
            this.buttonTest.Text = "SEQ STOP";
            this.buttonTest.UseVisualStyleBackColor = false;
            // 
            // btnTest
            // 
            this.btnTest.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnTest.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnTest.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnTest.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnTest.CustomFont = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnTest.CustomForeColor = System.Drawing.Color.Black;
            this.btnTest.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnTest.ForeColor = System.Drawing.Color.Black;
            this.btnTest.ImageSize = new System.Drawing.Size(45, 45);
            this.btnTest.Location = new System.Drawing.Point(374, 57);
            this.btnTest.Margin = new System.Windows.Forms.Padding(2);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(184, 51);
            this.btnTest.TabIndex = 33;
            this.btnTest.TabStop = false;
            this.btnTest.Text = "test1";
            this.btnTest.UseVisualStyleBackColor = false;
            // 
            // btnDieUnloading
            // 
            this.btnDieUnloading.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDieUnloading.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnDieUnloading.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnDieUnloading.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnDieUnloading.CustomFont = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDieUnloading.CustomForeColor = System.Drawing.Color.Black;
            this.btnDieUnloading.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDieUnloading.ForeColor = System.Drawing.Color.Black;
            this.btnDieUnloading.ImageSize = new System.Drawing.Size(45, 45);
            this.btnDieUnloading.Location = new System.Drawing.Point(2, 112);
            this.btnDieUnloading.Margin = new System.Windows.Forms.Padding(2);
            this.btnDieUnloading.Name = "btnDieUnloading";
            this.btnDieUnloading.Size = new System.Drawing.Size(182, 51);
            this.btnDieUnloading.TabIndex = 39;
            this.btnDieUnloading.TabStop = false;
            this.btnDieUnloading.Text = "Die PlaceDown";
            this.btnDieUnloading.UseVisualStyleBackColor = false;
            this.btnDieUnloading.Click += new System.EventHandler(this.btnDiePlaceDown_Click);
            // 
            // btnDieLoading
            // 
            this.btnDieLoading.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDieLoading.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnDieLoading.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnDieLoading.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnDieLoading.CustomFont = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDieLoading.CustomForeColor = System.Drawing.Color.Black;
            this.btnDieLoading.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDieLoading.ForeColor = System.Drawing.Color.Black;
            this.btnDieLoading.ImageSize = new System.Drawing.Size(45, 45);
            this.btnDieLoading.Location = new System.Drawing.Point(2, 57);
            this.btnDieLoading.Margin = new System.Windows.Forms.Padding(2);
            this.btnDieLoading.Name = "btnDieLoading";
            this.btnDieLoading.Size = new System.Drawing.Size(182, 51);
            this.btnDieLoading.TabIndex = 38;
            this.btnDieLoading.TabStop = false;
            this.btnDieLoading.Text = "Die PickUp";
            this.btnDieLoading.UseVisualStyleBackColor = false;
            this.btnDieLoading.Click += new System.EventHandler(this.btnDiePickUp_Click);
            // 
            // cbLoadIndex
            // 
            this.cbLoadIndex.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbLoadIndex.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8"});
            this.cbLoadIndex.FormattingEnabled = true;
            this.cbLoadIndex.Location = new System.Drawing.Point(3, 3);
            this.cbLoadIndex.Name = "cbLoadIndex";
            this.cbLoadIndex.Size = new System.Drawing.Size(180, 36);
            this.cbLoadIndex.TabIndex = 40;
            // 
            // dioControl
            // 
            this.dioControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dioControl.IoSortMode = QMC.LCP_280.Process.Component.DIOControl.SortingMode.AlphabeticalKey;
            this.dioControl.Location = new System.Drawing.Point(2, 568);
            this.dioControl.Margin = new System.Windows.Forms.Padding(2, 5, 2, 5);
            this.dioControl.Name = "dioControl";
            this.dioControl.RefreshIntervalMs = 400;
            this.dioControl.Size = new System.Drawing.Size(754, 554);
            this.dioControl.TabIndex = 24;
            // 
            // teachingPositionControl
            // 
            this.teachingPositionControl.ButtonSize = new System.Drawing.Size(90, 32);
            this.teachingPositionControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.teachingPositionControl.Location = new System.Drawing.Point(2, 5);
            this.teachingPositionControl.Margin = new System.Windows.Forms.Padding(2, 5, 2, 5);
            this.teachingPositionControl.Name = "teachingPositionControl";
            this.teachingPositionControl.ShowCancelButton = true;
            this.teachingPositionControl.ShowSaveButton = true;
            this.teachingPositionControl.Size = new System.Drawing.Size(754, 553);
            this.teachingPositionControl.TabIndex = 18;
            this.teachingPositionControl.UnitName = null;
            // 
            // ChipLoading_Working
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1896, 1127);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "ChipLoading_Working";
            this.Text = "ChipLoading Working";
            this.groupBoxImageView.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._ChipLoadingCameraviewer)).EndInit();
            this.tableLayoutPanel5.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.groupBoxManualTrSeq.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.groupBoxManual.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }
        private TeachingPositionControl teachingPositionControl;
        private Common.Vision.VisionImageViewer _ChipLoadingCameraviewer;
        private GroupBox groupBoxImageView;
        private TableLayoutPanel tableLayoutPanel1;
        private GroupBox groupBoxManual;
        private TableLayoutPanel tableLayoutPanel2;
        private IndividualMenuButton btnTest;
        private DIOControl dioControl;
        private TableLayoutPanel tableLayoutPanel4;
        private TableLayoutPanel tableLayoutPanel5;
        private IndividualMenuButton buttonDataManual;
        private IndividualMenuButton _btnVisionSetting;
        private IndividualMenuButton buttonTest;
        private IndividualMenuButton buttonTest2;
        private IndividualMenuButton buttonPickUpNiddle_Move;
        private GroupBox groupBoxManualTrSeq;
        private TableLayoutPanel tableLayoutPanel3;
        private ManualSequenceControl manualSequenceControl;
        private IndividualMenuButton btnDieUnloading;
        private IndividualMenuButton btnDieLoading;
        private ComboBox cbLoadIndex;
    }
}