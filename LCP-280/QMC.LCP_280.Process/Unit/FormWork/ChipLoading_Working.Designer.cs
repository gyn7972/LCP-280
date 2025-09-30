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
    partial class ChipLoading_Working
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
            this.groupBoxManual = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.buttonTest = new QMC.Common.IndividualMenuButton();
            this.btnSafetyZ = new QMC.Common.IndividualMenuButton();
            this.btnTest = new QMC.Common.IndividualMenuButton();
            this.btnAlignXY = new QMC.Common.IndividualMenuButton();
            this.btnPickUp = new QMC.Common.IndividualMenuButton();
            this.btnWaferUnloading = new QMC.Common.IndividualMenuButton();
            this.btnWaferLoading = new QMC.Common.IndividualMenuButton();
            this.btnReleaseVacuumAndPlaceUp = new QMC.Common.IndividualMenuButton();
            this.btnPlaceChipDown = new QMC.Common.IndividualMenuButton();
            this.btnPickUpNiddleMove = new QMC.Common.IndividualMenuButton();
            this.btnSyncPickPinRetreat = new QMC.Common.IndividualMenuButton();
            this.btnDieTrReady = new QMC.Common.IndividualMenuButton();
            this.btnAlignT = new QMC.Common.IndividualMenuButton();
            this.btnEjecterZUp = new QMC.Common.IndividualMenuButton();
            this.btnPickUpDn = new QMC.Common.IndividualMenuButton();
            this.buttonTest2 = new QMC.Common.IndividualMenuButton();
            this.buttonPickUpNiddle_Move = new QMC.Common.IndividualMenuButton();
            this.dioControl = new QMC.LCP_280.Process.Component.DIOControl();
            this.teachingPositionControl = new QMC.LCP_280.Process.Component.TeachingPositionControl();
            this.groupBoxManualTrSeq = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.manualSequenceControl = new QMC.LCP_280.Process.Component.ManualSequenceControl();
            this.groupBoxImageView.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._ChipLoadingCameraviewer)).BeginInit();
            this.tableLayoutPanel5.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.groupBoxManual.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.groupBoxManualTrSeq.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxImageView
            // 
            this.groupBoxImageView.Controls.Add(this.tableLayoutPanel4);
            this.groupBoxImageView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxImageView.Font = new System.Drawing.Font("맑은 고딕", 10F);
            this.groupBoxImageView.Location = new System.Drawing.Point(507, 2);
            this.groupBoxImageView.Margin = new System.Windows.Forms.Padding(2);
            this.groupBoxImageView.Name = "groupBoxImageView";
            this.groupBoxImageView.Padding = new System.Windows.Forms.Padding(2);
            this.groupBoxImageView.Size = new System.Drawing.Size(375, 371);
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
            this.tableLayoutPanel4.Location = new System.Drawing.Point(2, 20);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 3;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 80F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(371, 349);
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
            this._ChipLoadingCameraviewer.Size = new System.Drawing.Size(367, 275);
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
            this.tableLayoutPanel5.Location = new System.Drawing.Point(3, 282);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 1;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(365, 46);
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
            this._btnVisionSetting.Size = new System.Drawing.Size(178, 42);
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
            this.buttonDataManual.Location = new System.Drawing.Point(184, 2);
            this.buttonDataManual.Margin = new System.Windows.Forms.Padding(2);
            this.buttonDataManual.Name = "buttonDataManual";
            this.buttonDataManual.Size = new System.Drawing.Size(179, 42);
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
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1264, 751);
            this.tableLayoutPanel1.TabIndex = 24;
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
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.Controls.Add(this.buttonTest, 2, 1);
            this.tableLayoutPanel2.Controls.Add(this.btnSafetyZ, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnTest, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnAlignXY, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.btnPickUp, 1, 2);
            this.tableLayoutPanel2.Controls.Add(this.btnWaferUnloading, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.btnWaferLoading, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.btnReleaseVacuumAndPlaceUp, 0, 3);
            this.tableLayoutPanel2.Controls.Add(this.btnPlaceChipDown, 1, 3);
            this.tableLayoutPanel2.Controls.Add(this.btnPickUpNiddleMove, 0, 4);
            this.tableLayoutPanel2.Controls.Add(this.btnSyncPickPinRetreat, 1, 4);
            this.tableLayoutPanel2.Controls.Add(this.btnDieTrReady, 0, 5);
            this.tableLayoutPanel2.Controls.Add(this.btnAlignT, 1, 5);
            this.tableLayoutPanel2.Controls.Add(this.btnEjecterZUp, 0, 6);
            this.tableLayoutPanel2.Controls.Add(this.btnPickUpDn, 1, 6);
            this.tableLayoutPanel2.Controls.Add(this.buttonTest2, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.buttonPickUpNiddle_Move, 2, 2);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(2, 20);
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
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(371, 350);
            this.tableLayoutPanel2.TabIndex = 34;
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
            this.buttonTest.Location = new System.Drawing.Point(248, 38);
            this.buttonTest.Margin = new System.Windows.Forms.Padding(2);
            this.buttonTest.Name = "buttonTest";
            this.buttonTest.Size = new System.Drawing.Size(121, 32);
            this.buttonTest.TabIndex = 37;
            this.buttonTest.TabStop = false;
            this.buttonTest.Text = "SEQ STOP";
            this.buttonTest.UseVisualStyleBackColor = false;
            // 
            // btnSafetyZ
            // 
            this.btnSafetyZ.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSafetyZ.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnSafetyZ.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnSafetyZ.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnSafetyZ.CustomFont = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSafetyZ.CustomForeColor = System.Drawing.Color.Black;
            this.btnSafetyZ.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSafetyZ.ForeColor = System.Drawing.Color.Black;
            this.btnSafetyZ.ImageSize = new System.Drawing.Size(45, 45);
            this.btnSafetyZ.Location = new System.Drawing.Point(2, 2);
            this.btnSafetyZ.Margin = new System.Windows.Forms.Padding(2);
            this.btnSafetyZ.Name = "btnSafetyZ";
            this.btnSafetyZ.Size = new System.Drawing.Size(119, 32);
            this.btnSafetyZ.TabIndex = 26;
            this.btnSafetyZ.TabStop = false;
            this.btnSafetyZ.Text = "Tr SafetyZ";
            this.btnSafetyZ.UseVisualStyleBackColor = false;
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
            this.btnTest.Location = new System.Drawing.Point(125, 2);
            this.btnTest.Margin = new System.Windows.Forms.Padding(2);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(119, 32);
            this.btnTest.TabIndex = 33;
            this.btnTest.TabStop = false;
            this.btnTest.Text = "test1";
            this.btnTest.UseVisualStyleBackColor = false;
            // 
            // btnAlignXY
            // 
            this.btnAlignXY.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAlignXY.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnAlignXY.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnAlignXY.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnAlignXY.CustomFont = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAlignXY.CustomForeColor = System.Drawing.Color.Black;
            this.btnAlignXY.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAlignXY.ForeColor = System.Drawing.Color.Black;
            this.btnAlignXY.ImageSize = new System.Drawing.Size(45, 45);
            this.btnAlignXY.Location = new System.Drawing.Point(125, 38);
            this.btnAlignXY.Margin = new System.Windows.Forms.Padding(2);
            this.btnAlignXY.Name = "btnAlignXY";
            this.btnAlignXY.Size = new System.Drawing.Size(119, 32);
            this.btnAlignXY.TabIndex = 22;
            this.btnAlignXY.TabStop = false;
            this.btnAlignXY.Text = "AlignXY";
            this.btnAlignXY.UseVisualStyleBackColor = false;
            this.btnAlignXY.Visible = false;
            // 
            // btnPickUp
            // 
            this.btnPickUp.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPickUp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnPickUp.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnPickUp.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnPickUp.CustomFont = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPickUp.CustomForeColor = System.Drawing.Color.Black;
            this.btnPickUp.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPickUp.ForeColor = System.Drawing.Color.Black;
            this.btnPickUp.ImageSize = new System.Drawing.Size(45, 45);
            this.btnPickUp.Location = new System.Drawing.Point(125, 74);
            this.btnPickUp.Margin = new System.Windows.Forms.Padding(2);
            this.btnPickUp.Name = "btnPickUp";
            this.btnPickUp.Size = new System.Drawing.Size(119, 32);
            this.btnPickUp.TabIndex = 25;
            this.btnPickUp.TabStop = false;
            this.btnPickUp.Text = "PickDown";
            this.btnPickUp.UseVisualStyleBackColor = false;
            this.btnPickUp.Visible = false;
            // 
            // btnWaferUnloading
            // 
            this.btnWaferUnloading.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnWaferUnloading.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnWaferUnloading.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnWaferUnloading.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnWaferUnloading.CustomFont = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnWaferUnloading.CustomForeColor = System.Drawing.Color.Black;
            this.btnWaferUnloading.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnWaferUnloading.ForeColor = System.Drawing.Color.Black;
            this.btnWaferUnloading.ImageSize = new System.Drawing.Size(45, 45);
            this.btnWaferUnloading.Location = new System.Drawing.Point(2, 38);
            this.btnWaferUnloading.Margin = new System.Windows.Forms.Padding(2);
            this.btnWaferUnloading.Name = "btnWaferUnloading";
            this.btnWaferUnloading.Size = new System.Drawing.Size(119, 32);
            this.btnWaferUnloading.TabIndex = 23;
            this.btnWaferUnloading.TabStop = false;
            this.btnWaferUnloading.Text = "WaferUnloading";
            this.btnWaferUnloading.UseVisualStyleBackColor = false;
            this.btnWaferUnloading.Visible = false;
            // 
            // btnWaferLoading
            // 
            this.btnWaferLoading.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnWaferLoading.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnWaferLoading.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnWaferLoading.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnWaferLoading.CustomFont = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnWaferLoading.CustomForeColor = System.Drawing.Color.Black;
            this.btnWaferLoading.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnWaferLoading.ForeColor = System.Drawing.Color.Black;
            this.btnWaferLoading.ImageSize = new System.Drawing.Size(45, 45);
            this.btnWaferLoading.Location = new System.Drawing.Point(2, 74);
            this.btnWaferLoading.Margin = new System.Windows.Forms.Padding(2);
            this.btnWaferLoading.Name = "btnWaferLoading";
            this.btnWaferLoading.Size = new System.Drawing.Size(119, 32);
            this.btnWaferLoading.TabIndex = 20;
            this.btnWaferLoading.TabStop = false;
            this.btnWaferLoading.Text = "WaferLoading";
            this.btnWaferLoading.UseVisualStyleBackColor = false;
            this.btnWaferLoading.Visible = false;
            // 
            // btnReleaseVacuumAndPlaceUp
            // 
            this.btnReleaseVacuumAndPlaceUp.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnReleaseVacuumAndPlaceUp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnReleaseVacuumAndPlaceUp.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnReleaseVacuumAndPlaceUp.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnReleaseVacuumAndPlaceUp.CustomFont = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnReleaseVacuumAndPlaceUp.CustomForeColor = System.Drawing.Color.Black;
            this.btnReleaseVacuumAndPlaceUp.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnReleaseVacuumAndPlaceUp.ForeColor = System.Drawing.Color.Black;
            this.btnReleaseVacuumAndPlaceUp.ImageSize = new System.Drawing.Size(45, 45);
            this.btnReleaseVacuumAndPlaceUp.Location = new System.Drawing.Point(2, 110);
            this.btnReleaseVacuumAndPlaceUp.Margin = new System.Windows.Forms.Padding(2);
            this.btnReleaseVacuumAndPlaceUp.Name = "btnReleaseVacuumAndPlaceUp";
            this.btnReleaseVacuumAndPlaceUp.Size = new System.Drawing.Size(119, 32);
            this.btnReleaseVacuumAndPlaceUp.TabIndex = 31;
            this.btnReleaseVacuumAndPlaceUp.TabStop = false;
            this.btnReleaseVacuumAndPlaceUp.Text = "PlaceUp";
            this.btnReleaseVacuumAndPlaceUp.UseVisualStyleBackColor = false;
            this.btnReleaseVacuumAndPlaceUp.Visible = false;
            // 
            // btnPlaceChipDown
            // 
            this.btnPlaceChipDown.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPlaceChipDown.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnPlaceChipDown.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnPlaceChipDown.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnPlaceChipDown.CustomFont = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPlaceChipDown.CustomForeColor = System.Drawing.Color.Black;
            this.btnPlaceChipDown.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPlaceChipDown.ForeColor = System.Drawing.Color.Black;
            this.btnPlaceChipDown.ImageSize = new System.Drawing.Size(45, 45);
            this.btnPlaceChipDown.Location = new System.Drawing.Point(125, 110);
            this.btnPlaceChipDown.Margin = new System.Windows.Forms.Padding(2);
            this.btnPlaceChipDown.Name = "btnPlaceChipDown";
            this.btnPlaceChipDown.Size = new System.Drawing.Size(119, 32);
            this.btnPlaceChipDown.TabIndex = 30;
            this.btnPlaceChipDown.TabStop = false;
            this.btnPlaceChipDown.Text = "PlaceDown";
            this.btnPlaceChipDown.UseVisualStyleBackColor = false;
            this.btnPlaceChipDown.Visible = false;
            // 
            // btnPickUpNiddleMove
            // 
            this.btnPickUpNiddleMove.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPickUpNiddleMove.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnPickUpNiddleMove.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnPickUpNiddleMove.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnPickUpNiddleMove.CustomFont = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPickUpNiddleMove.CustomForeColor = System.Drawing.Color.Black;
            this.btnPickUpNiddleMove.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPickUpNiddleMove.ForeColor = System.Drawing.Color.Black;
            this.btnPickUpNiddleMove.ImageSize = new System.Drawing.Size(45, 45);
            this.btnPickUpNiddleMove.Location = new System.Drawing.Point(2, 146);
            this.btnPickUpNiddleMove.Margin = new System.Windows.Forms.Padding(2);
            this.btnPickUpNiddleMove.Name = "btnPickUpNiddleMove";
            this.btnPickUpNiddleMove.Size = new System.Drawing.Size(119, 32);
            this.btnPickUpNiddleMove.TabIndex = 24;
            this.btnPickUpNiddleMove.TabStop = false;
            this.btnPickUpNiddleMove.Text = "PickUp&&Niddle";
            this.btnPickUpNiddleMove.UseVisualStyleBackColor = false;
            this.btnPickUpNiddleMove.Visible = false;
            // 
            // btnSyncPickPinRetreat
            // 
            this.btnSyncPickPinRetreat.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSyncPickPinRetreat.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnSyncPickPinRetreat.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnSyncPickPinRetreat.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnSyncPickPinRetreat.CustomFont = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSyncPickPinRetreat.CustomForeColor = System.Drawing.Color.Black;
            this.btnSyncPickPinRetreat.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSyncPickPinRetreat.ForeColor = System.Drawing.Color.Black;
            this.btnSyncPickPinRetreat.ImageSize = new System.Drawing.Size(45, 45);
            this.btnSyncPickPinRetreat.Location = new System.Drawing.Point(125, 146);
            this.btnSyncPickPinRetreat.Margin = new System.Windows.Forms.Padding(2);
            this.btnSyncPickPinRetreat.Name = "btnSyncPickPinRetreat";
            this.btnSyncPickPinRetreat.Size = new System.Drawing.Size(119, 32);
            this.btnSyncPickPinRetreat.TabIndex = 28;
            this.btnSyncPickPinRetreat.TabStop = false;
            this.btnSyncPickPinRetreat.Text = "SyncRetreat";
            this.btnSyncPickPinRetreat.UseVisualStyleBackColor = false;
            this.btnSyncPickPinRetreat.Visible = false;
            // 
            // btnDieTrReady
            // 
            this.btnDieTrReady.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDieTrReady.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnDieTrReady.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnDieTrReady.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnDieTrReady.CustomFont = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDieTrReady.CustomForeColor = System.Drawing.Color.Black;
            this.btnDieTrReady.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDieTrReady.ForeColor = System.Drawing.Color.Black;
            this.btnDieTrReady.ImageSize = new System.Drawing.Size(45, 45);
            this.btnDieTrReady.Location = new System.Drawing.Point(2, 182);
            this.btnDieTrReady.Margin = new System.Windows.Forms.Padding(2);
            this.btnDieTrReady.Name = "btnDieTrReady";
            this.btnDieTrReady.Size = new System.Drawing.Size(119, 32);
            this.btnDieTrReady.TabIndex = 29;
            this.btnDieTrReady.TabStop = false;
            this.btnDieTrReady.Text = "DieTrReady";
            this.btnDieTrReady.UseVisualStyleBackColor = false;
            this.btnDieTrReady.Visible = false;
            // 
            // btnAlignT
            // 
            this.btnAlignT.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAlignT.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnAlignT.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnAlignT.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnAlignT.CustomFont = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAlignT.CustomForeColor = System.Drawing.Color.Black;
            this.btnAlignT.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAlignT.ForeColor = System.Drawing.Color.Black;
            this.btnAlignT.ImageSize = new System.Drawing.Size(45, 45);
            this.btnAlignT.Location = new System.Drawing.Point(125, 182);
            this.btnAlignT.Margin = new System.Windows.Forms.Padding(2);
            this.btnAlignT.Name = "btnAlignT";
            this.btnAlignT.Size = new System.Drawing.Size(119, 32);
            this.btnAlignT.TabIndex = 21;
            this.btnAlignT.TabStop = false;
            this.btnAlignT.Text = "AlignT";
            this.btnAlignT.UseVisualStyleBackColor = false;
            this.btnAlignT.Visible = false;
            // 
            // btnEjecterZUp
            // 
            this.btnEjecterZUp.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnEjecterZUp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnEjecterZUp.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnEjecterZUp.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnEjecterZUp.CustomFont = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnEjecterZUp.CustomForeColor = System.Drawing.Color.Black;
            this.btnEjecterZUp.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnEjecterZUp.ForeColor = System.Drawing.Color.Black;
            this.btnEjecterZUp.ImageSize = new System.Drawing.Size(45, 45);
            this.btnEjecterZUp.Location = new System.Drawing.Point(2, 218);
            this.btnEjecterZUp.Margin = new System.Windows.Forms.Padding(2);
            this.btnEjecterZUp.Name = "btnEjecterZUp";
            this.btnEjecterZUp.Size = new System.Drawing.Size(119, 32);
            this.btnEjecterZUp.TabIndex = 27;
            this.btnEjecterZUp.TabStop = false;
            this.btnEjecterZUp.Text = "EjecterZUp";
            this.btnEjecterZUp.UseVisualStyleBackColor = false;
            this.btnEjecterZUp.Visible = false;
            // 
            // btnPickUpDn
            // 
            this.btnPickUpDn.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPickUpDn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnPickUpDn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnPickUpDn.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnPickUpDn.CustomFont = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPickUpDn.CustomForeColor = System.Drawing.Color.Black;
            this.btnPickUpDn.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPickUpDn.ForeColor = System.Drawing.Color.Black;
            this.btnPickUpDn.ImageSize = new System.Drawing.Size(45, 45);
            this.btnPickUpDn.Location = new System.Drawing.Point(125, 218);
            this.btnPickUpDn.Margin = new System.Windows.Forms.Padding(2);
            this.btnPickUpDn.Name = "btnPickUpDn";
            this.btnPickUpDn.Size = new System.Drawing.Size(119, 32);
            this.btnPickUpDn.TabIndex = 32;
            this.btnPickUpDn.TabStop = false;
            this.btnPickUpDn.Text = "PickUpDn";
            this.btnPickUpDn.UseVisualStyleBackColor = false;
            this.btnPickUpDn.Visible = false;
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
            this.buttonTest2.Location = new System.Drawing.Point(248, 2);
            this.buttonTest2.Margin = new System.Windows.Forms.Padding(2);
            this.buttonTest2.Name = "buttonTest2";
            this.buttonTest2.Size = new System.Drawing.Size(121, 32);
            this.buttonTest2.TabIndex = 34;
            this.buttonTest2.TabStop = false;
            this.buttonTest2.Text = "Test2";
            this.buttonTest2.UseVisualStyleBackColor = false;
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
            this.buttonPickUpNiddle_Move.Location = new System.Drawing.Point(248, 74);
            this.buttonPickUpNiddle_Move.Margin = new System.Windows.Forms.Padding(2);
            this.buttonPickUpNiddle_Move.Name = "buttonPickUpNiddle_Move";
            this.buttonPickUpNiddle_Move.Size = new System.Drawing.Size(121, 32);
            this.buttonPickUpNiddle_Move.TabIndex = 36;
            this.buttonPickUpNiddle_Move.TabStop = false;
            this.buttonPickUpNiddle_Move.Text = "PickUp && Niddle Move";
            this.buttonPickUpNiddle_Move.UseVisualStyleBackColor = false;
            // 
            // dioControl
            // 
            this.dioControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dioControl.IoSortMode = QMC.LCP_280.Process.Component.DIOControl.SortingMode.AlphabeticalKey;
            this.dioControl.Location = new System.Drawing.Point(2, 378);
            this.dioControl.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.dioControl.Name = "dioControl";
            this.dioControl.RefreshIntervalMs = 400;
            this.dioControl.Size = new System.Drawing.Size(501, 370);
            this.dioControl.TabIndex = 24;
            // 
            // teachingPositionControl
            // 
            this.teachingPositionControl.ButtonSize = new System.Drawing.Size(90, 32);
            this.teachingPositionControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.teachingPositionControl.Location = new System.Drawing.Point(2, 3);
            this.teachingPositionControl.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.teachingPositionControl.Name = "teachingPositionControl";
            this.teachingPositionControl.ShowCancelButton = true;
            this.teachingPositionControl.ShowSaveButton = true;
            this.teachingPositionControl.Size = new System.Drawing.Size(501, 369);
            this.teachingPositionControl.TabIndex = 18;
            this.teachingPositionControl.UnitName = null;
            // 
            // groupBoxManualTrSeq
            // 
            this.groupBoxManualTrSeq.Controls.Add(this.tableLayoutPanel3);
            this.groupBoxManualTrSeq.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxManualTrSeq.Font = new System.Drawing.Font("맑은 고딕", 10F);
            this.groupBoxManualTrSeq.Location = new System.Drawing.Point(886, 2);
            this.groupBoxManualTrSeq.Margin = new System.Windows.Forms.Padding(2);
            this.groupBoxManualTrSeq.Name = "groupBoxManualTrSeq";
            this.groupBoxManualTrSeq.Padding = new System.Windows.Forms.Padding(2);
            this.groupBoxManualTrSeq.Size = new System.Drawing.Size(376, 371);
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
            this.tableLayoutPanel3.Location = new System.Drawing.Point(2, 20);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(372, 349);
            this.tableLayoutPanel3.TabIndex = 20;
            // 
            // manualSequenceControl
            // 
            this.manualSequenceControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.manualSequenceControl.Location = new System.Drawing.Point(2, 2);
            this.manualSequenceControl.Margin = new System.Windows.Forms.Padding(2);
            this.manualSequenceControl.Name = "manualSequenceControl";
            this.manualSequenceControl.ParentUnit = null;
            this.manualSequenceControl.Size = new System.Drawing.Size(368, 345);
            this.manualSequenceControl.TabIndex = 19;
            // 
            // ChipLoading_Working
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1264, 751);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "ChipLoading_Working";
            this.Text = "ChipLoading Working";
            this.groupBoxImageView.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._ChipLoadingCameraviewer)).EndInit();
            this.tableLayoutPanel5.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.groupBoxManual.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.groupBoxManualTrSeq.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
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
        private IndividualMenuButton btnSafetyZ;
        private IndividualMenuButton btnTest;
        private IndividualMenuButton btnAlignXY;
        private IndividualMenuButton btnPickUp;
        private IndividualMenuButton btnWaferUnloading;
        private IndividualMenuButton btnWaferLoading;
        private IndividualMenuButton btnReleaseVacuumAndPlaceUp;
        private IndividualMenuButton btnPlaceChipDown;
        private IndividualMenuButton btnPickUpNiddleMove;
        private IndividualMenuButton btnSyncPickPinRetreat;
        private IndividualMenuButton btnDieTrReady;
        private IndividualMenuButton btnAlignT;
        private IndividualMenuButton btnEjecterZUp;
        private IndividualMenuButton btnPickUpDn;
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
    }
}