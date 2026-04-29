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
            this.manualControl = new QMC.LCP_280.Process.Component.ManualControl();
            this.groupBoxManual = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonTest = new QMC.Common.IndividualMenuButton();
            this.btnTest = new QMC.Common.IndividualMenuButton();
            this.buttonTest2 = new QMC.Common.IndividualMenuButton();
            this.buttonPickUpNeedle_Move = new QMC.Common.IndividualMenuButton();
            this.cbLoadIndex = new System.Windows.Forms.ComboBox();
            this.btnDieLoading = new QMC.Common.IndividualMenuButton();
            this.btnDieUnloading = new QMC.Common.IndividualMenuButton();
            this.dioControl = new QMC.LCP_280.Process.Component.DIOControl();
            this.teachingPositionControl = new QMC.LCP_280.Process.Component.MoveToPositionControl();
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
            this.groupBoxImageView.Location = new System.Drawing.Point(696, 3);
            this.groupBoxImageView.Name = "groupBoxImageView";
            this.groupBoxImageView.Size = new System.Drawing.Size(456, 557);
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
            this.tableLayoutPanel4.Location = new System.Drawing.Point(3, 30);
            this.tableLayoutPanel4.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 3;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 80F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(450, 524);
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
            this._ChipLoadingCameraviewer.Location = new System.Drawing.Point(3, 3);
            this._ChipLoadingCameraviewer.Name = "_ChipLoadingCameraviewer";
            this._ChipLoadingCameraviewer.OperatingType = QMC.Common.Vision.VisionImageViewer.OperatingTypes.Center;
            this._ChipLoadingCameraviewer.Simulated = false;
            this._ChipLoadingCameraviewer.Size = new System.Drawing.Size(444, 413);
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
            this.tableLayoutPanel5.Location = new System.Drawing.Point(4, 423);
            this.tableLayoutPanel5.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 1;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(442, 70);
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
            this._btnVisionSetting.Location = new System.Drawing.Point(3, 3);
            this._btnVisionSetting.Name = "_btnVisionSetting";
            this._btnVisionSetting.Size = new System.Drawing.Size(215, 64);
            this._btnVisionSetting.TabIndex = 34;
            this._btnVisionSetting.TabStop = false;
            this._btnVisionSetting.Text = "VisionSetting";
            this._btnVisionSetting.UseVisualStyleBackColor = false;
            this._btnVisionSetting.Visible = false;
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
            this.buttonDataManual.Location = new System.Drawing.Point(224, 3);
            this.buttonDataManual.Name = "buttonDataManual";
            this.buttonDataManual.Size = new System.Drawing.Size(215, 64);
            this.buttonDataManual.TabIndex = 35;
            this.buttonDataManual.TabStop = false;
            this.buttonDataManual.Text = "ManualData";
            this.buttonDataManual.UseVisualStyleBackColor = false;
            this.buttonDataManual.Visible = false;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.Controls.Add(this.groupBoxManualTrSeq, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.groupBoxManual, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.dioControl, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.teachingPositionControl, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.groupBoxImageView, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1540, 1126);
            this.tableLayoutPanel1.TabIndex = 24;
            // 
            // groupBoxManualTrSeq
            // 
            this.groupBoxManualTrSeq.Controls.Add(this.tableLayoutPanel3);
            this.groupBoxManualTrSeq.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxManualTrSeq.Font = new System.Drawing.Font("맑은 고딕", 10F);
            this.groupBoxManualTrSeq.Location = new System.Drawing.Point(1158, 3);
            this.groupBoxManualTrSeq.Name = "groupBoxManualTrSeq";
            this.groupBoxManualTrSeq.Size = new System.Drawing.Size(379, 557);
            this.groupBoxManualTrSeq.TabIndex = 29;
            this.groupBoxManualTrSeq.TabStop = false;
            this.groupBoxManualTrSeq.Text = "ChipLoading Seq.";
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 1;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.manualControl, 0, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 30);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(373, 524);
            this.tableLayoutPanel3.TabIndex = 20;
            // 
            // manualControl
            // 
            this.manualControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.manualControl.Location = new System.Drawing.Point(3, 3);
            this.manualControl.Name = "manualControl";
            this.manualControl.ParentUnit = null;
            this.manualControl.Size = new System.Drawing.Size(367, 518);
            this.manualControl.TabIndex = 19;
            // 
            // groupBoxManual
            // 
            this.groupBoxManual.Controls.Add(this.tableLayoutPanel2);
            this.groupBoxManual.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxManual.Font = new System.Drawing.Font("맑은 고딕", 10F);
            this.groupBoxManual.Location = new System.Drawing.Point(696, 566);
            this.groupBoxManual.Name = "groupBoxManual";
            this.groupBoxManual.Size = new System.Drawing.Size(456, 557);
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
            this.tableLayoutPanel2.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.buttonTest, 0, 8);
            this.tableLayoutPanel2.Controls.Add(this.btnTest, 1, 8);
            this.tableLayoutPanel2.Controls.Add(this.buttonTest2, 2, 8);
            this.tableLayoutPanel2.Controls.Add(this.buttonPickUpNeedle_Move, 2, 7);
            this.tableLayoutPanel2.Controls.Add(this.cbLoadIndex, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnDieLoading, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.btnDieUnloading, 1, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 30);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(4);
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
            this.tableLayoutPanel2.Size = new System.Drawing.Size(450, 524);
            this.tableLayoutPanel2.TabIndex = 34;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(144, 54);
            this.label1.TabIndex = 41;
            this.label1.Text = "Load Slot";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
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
            this.buttonTest.Location = new System.Drawing.Point(3, 435);
            this.buttonTest.Name = "buttonTest";
            this.buttonTest.Size = new System.Drawing.Size(144, 48);
            this.buttonTest.TabIndex = 37;
            this.buttonTest.TabStop = false;
            this.buttonTest.Text = "SEQ STOP";
            this.buttonTest.UseVisualStyleBackColor = false;
            this.buttonTest.Visible = false;
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
            this.btnTest.Location = new System.Drawing.Point(153, 435);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(144, 48);
            this.btnTest.TabIndex = 33;
            this.btnTest.TabStop = false;
            this.btnTest.Text = "test1";
            this.btnTest.UseVisualStyleBackColor = false;
            this.btnTest.Visible = false;
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
            this.buttonTest2.Location = new System.Drawing.Point(303, 435);
            this.buttonTest2.Name = "buttonTest2";
            this.buttonTest2.Size = new System.Drawing.Size(144, 48);
            this.buttonTest2.TabIndex = 34;
            this.buttonTest2.TabStop = false;
            this.buttonTest2.Text = "Test2";
            this.buttonTest2.UseVisualStyleBackColor = false;
            this.buttonTest2.Visible = false;
            // 
            // buttonPickUpNeedle_Move
            // 
            this.buttonPickUpNeedle_Move.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonPickUpNeedle_Move.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.buttonPickUpNeedle_Move.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.buttonPickUpNeedle_Move.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.buttonPickUpNeedle_Move.CustomFont = new System.Drawing.Font("Arial", 6.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonPickUpNeedle_Move.CustomForeColor = System.Drawing.Color.Black;
            this.buttonPickUpNeedle_Move.Font = new System.Drawing.Font("Arial", 6.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonPickUpNeedle_Move.ForeColor = System.Drawing.Color.Black;
            this.buttonPickUpNeedle_Move.ImageSize = new System.Drawing.Size(45, 45);
            this.buttonPickUpNeedle_Move.Location = new System.Drawing.Point(303, 381);
            this.buttonPickUpNeedle_Move.Name = "buttonPickUpNeedle_Move";
            this.buttonPickUpNeedle_Move.Size = new System.Drawing.Size(144, 48);
            this.buttonPickUpNeedle_Move.TabIndex = 36;
            this.buttonPickUpNeedle_Move.TabStop = false;
            this.buttonPickUpNeedle_Move.Text = "PickUp && Niddle Move";
            this.buttonPickUpNeedle_Move.UseVisualStyleBackColor = false;
            this.buttonPickUpNeedle_Move.Visible = false;
            // 
            // cbLoadIndex
            // 
            this.cbLoadIndex.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbLoadIndex.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbLoadIndex.FormattingEnabled = true;
            this.cbLoadIndex.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8"});
            this.cbLoadIndex.Location = new System.Drawing.Point(153, 3);
            this.cbLoadIndex.Name = "cbLoadIndex";
            this.cbLoadIndex.Size = new System.Drawing.Size(144, 36);
            this.cbLoadIndex.TabIndex = 40;
            // 
            // btnDieLoading
            // 
            this.btnDieLoading.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnDieLoading.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnDieLoading.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnDieLoading.CustomFont = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDieLoading.CustomForeColor = System.Drawing.Color.Black;
            this.btnDieLoading.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnDieLoading.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDieLoading.ForeColor = System.Drawing.Color.Black;
            this.btnDieLoading.ImageSize = new System.Drawing.Size(45, 45);
            this.btnDieLoading.Location = new System.Drawing.Point(3, 57);
            this.btnDieLoading.Name = "btnDieLoading";
            this.btnDieLoading.Size = new System.Drawing.Size(144, 48);
            this.btnDieLoading.TabIndex = 38;
            this.btnDieLoading.TabStop = false;
            this.btnDieLoading.Text = "Die PickUp";
            this.btnDieLoading.UseVisualStyleBackColor = false;
            this.btnDieLoading.Click += new System.EventHandler(this.btnDiePickUp_Click);
            // 
            // btnDieUnloading
            // 
            this.btnDieUnloading.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnDieUnloading.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnDieUnloading.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnDieUnloading.CustomFont = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDieUnloading.CustomForeColor = System.Drawing.Color.Black;
            this.btnDieUnloading.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnDieUnloading.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDieUnloading.ForeColor = System.Drawing.Color.Black;
            this.btnDieUnloading.ImageSize = new System.Drawing.Size(45, 45);
            this.btnDieUnloading.Location = new System.Drawing.Point(153, 57);
            this.btnDieUnloading.Name = "btnDieUnloading";
            this.btnDieUnloading.Size = new System.Drawing.Size(144, 48);
            this.btnDieUnloading.TabIndex = 39;
            this.btnDieUnloading.TabStop = false;
            this.btnDieUnloading.Text = "Die PlaceDown";
            this.btnDieUnloading.UseVisualStyleBackColor = false;
            this.btnDieUnloading.Click += new System.EventHandler(this.btnDiePlaceDown_Click);
            // 
            // dioControl
            // 
            this.dioControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dioControl.IoSortMode = QMC.LCP_280.Process.Component.DIOControl.SortingMode.AlphabeticalKey;
            this.dioControl.Location = new System.Drawing.Point(3, 567);
            this.dioControl.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dioControl.Name = "dioControl";
            this.dioControl.RefreshIntervalMs = 400;
            this.dioControl.Size = new System.Drawing.Size(687, 555);
            this.dioControl.TabIndex = 24;
            // 
            // teachingPositionControl
            // 
            this.teachingPositionControl.AxisDisplayFont = null;
            this.teachingPositionControl.ButtonSize = new System.Drawing.Size(90, 32);
            this.teachingPositionControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.teachingPositionControl.Location = new System.Drawing.Point(3, 4);
            this.teachingPositionControl.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.teachingPositionControl.Name = "teachingPositionControl";
            this.teachingPositionControl.ShowCancelButton = true;
            this.teachingPositionControl.ShowSaveButton = true;
            this.teachingPositionControl.Size = new System.Drawing.Size(687, 555);
            this.teachingPositionControl.TabIndex = 18;
            this.teachingPositionControl.UnitName = null;
            // 
            // LoadArm_Menual
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1540, 1126);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "LoadArm_Menual";
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
            this.tableLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }
        private MoveToPositionControl teachingPositionControl;
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
        private IndividualMenuButton buttonPickUpNeedle_Move;
        private GroupBox groupBoxManualTrSeq;
        private TableLayoutPanel tableLayoutPanel3;
        private ManualControl manualControl;
        private IndividualMenuButton btnDieUnloading;
        private IndividualMenuButton btnDieLoading;
        private ComboBox cbLoadIndex;
        private Label label1;
    }
}