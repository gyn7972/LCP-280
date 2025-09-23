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
            this._btnVisionSetting = new System.Windows.Forms.Button();
            this.buttonDataManual = new System.Windows.Forms.Button();
            this.groupBoxImageView = new System.Windows.Forms.GroupBox();
            this._ChipLoadingCameraviewer = new QMC.Common.Vision.VisionImageViewer();
            this.groupBoxManual = new System.Windows.Forms.GroupBox();
            this.btnPickUpDn = new QMC.Common.IndividualMenuButton();
            this.btnReleaseVacuumAndPlaceUp = new QMC.Common.IndividualMenuButton();
            this.btnPlaceChipDown = new QMC.Common.IndividualMenuButton();
            this.btnDieTrReady = new QMC.Common.IndividualMenuButton();
            this.btnSyncPickPinRetreat = new QMC.Common.IndividualMenuButton();
            this.btnEjecterZUp = new QMC.Common.IndividualMenuButton();
            this.btnSafetyZ = new QMC.Common.IndividualMenuButton();
            this.btnPickUp = new QMC.Common.IndividualMenuButton();
            this.btnPickUpNiddleMove = new QMC.Common.IndividualMenuButton();
            this.btnWaferUnloading = new QMC.Common.IndividualMenuButton();
            this.btnAlignXY = new QMC.Common.IndividualMenuButton();
            this.btnAlignT = new QMC.Common.IndividualMenuButton();
            this.btnWaferLoading = new QMC.Common.IndividualMenuButton();
            this.buttonTest2 = new System.Windows.Forms.Button();
            this.buttonTest = new System.Windows.Forms.Button();
            this.buttonPickUpNiddle_Move = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.dioControl = new QMC.LCP_280.Process.Component.DIOControl();
            this.panel2 = new System.Windows.Forms.Panel();
            this.teachingPositionControl = new QMC.LCP_280.Process.Component.TeachingPositionControl();
            this.manualSequenceControl = new QMC.LCP_280.Process.Component.ManualSequenceControl();
            this.groupBoxManualTrSeq = new System.Windows.Forms.GroupBox();
            this.btnTest = new QMC.Common.IndividualMenuButton();
            this.groupBoxImageView.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._ChipLoadingCameraviewer)).BeginInit();
            this.groupBoxManual.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.groupBoxManualTrSeq.SuspendLayout();
            this.SuspendLayout();
            // 
            // _btnVisionSetting
            // 
            this._btnVisionSetting.Location = new System.Drawing.Point(629, 374);
            this._btnVisionSetting.Name = "_btnVisionSetting";
            this._btnVisionSetting.Size = new System.Drawing.Size(110, 35);
            this._btnVisionSetting.TabIndex = 13;
            this._btnVisionSetting.Text = "VisionSetting";
            this._btnVisionSetting.UseVisualStyleBackColor = true;
            this._btnVisionSetting.Click += new System.EventHandler(this._btnVisionSetting_Click);
            // 
            // buttonDataManual
            // 
            this.buttonDataManual.Location = new System.Drawing.Point(755, 374);
            this.buttonDataManual.Name = "buttonDataManual";
            this.buttonDataManual.Size = new System.Drawing.Size(95, 35);
            this.buttonDataManual.TabIndex = 14;
            this.buttonDataManual.Text = "ManualData";
            this.buttonDataManual.UseVisualStyleBackColor = true;
            this.buttonDataManual.Click += new System.EventHandler(this.buttonDataManual_Click);
            // 
            // groupBoxImageView
            // 
            this.groupBoxImageView.Controls.Add(this._ChipLoadingCameraviewer);
            this.groupBoxImageView.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.groupBoxImageView.Location = new System.Drawing.Point(627, 9);
            this.groupBoxImageView.Name = "groupBoxImageView";
            this.groupBoxImageView.Size = new System.Drawing.Size(392, 400);
            this.groupBoxImageView.TabIndex = 15;
            this.groupBoxImageView.TabStop = false;
            this.groupBoxImageView.Text = "ImageView";
            // 
            // _ChipLoadingCameraviewer
            // 
            this._ChipLoadingCameraviewer.BackColor = System.Drawing.Color.Black;
            this._ChipLoadingCameraviewer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._ChipLoadingCameraviewer.Camera = null;
            this._ChipLoadingCameraviewer.CameraSwitch = null;
            this._ChipLoadingCameraviewer.Dock = System.Windows.Forms.DockStyle.Top;
            this._ChipLoadingCameraviewer.FrameRate = 1D;
            this._ChipLoadingCameraviewer.InputImage = null;
            this._ChipLoadingCameraviewer.IsViewCustomizedImage = false;
            this._ChipLoadingCameraviewer.Location = new System.Drawing.Point(3, 26);
            this._ChipLoadingCameraviewer.Name = "_ChipLoadingCameraviewer";
            this._ChipLoadingCameraviewer.OperatingType = QMC.Common.Vision.VisionImageViewer.OperatingTypes.Center;
            this._ChipLoadingCameraviewer.Simulated = false;
            this._ChipLoadingCameraviewer.Size = new System.Drawing.Size(386, 324);
            this._ChipLoadingCameraviewer.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this._ChipLoadingCameraviewer.TabIndex = 12;
            this._ChipLoadingCameraviewer.TabStop = false;
            this._ChipLoadingCameraviewer.UpdateDelayTime = 80;
            this._ChipLoadingCameraviewer.VisibleCrossLine = true;
            // 
            // groupBoxManual
            // 
            this.groupBoxManual.Controls.Add(this.btnTest);
            this.groupBoxManual.Controls.Add(this.btnPickUpDn);
            this.groupBoxManual.Controls.Add(this.btnReleaseVacuumAndPlaceUp);
            this.groupBoxManual.Controls.Add(this.btnPlaceChipDown);
            this.groupBoxManual.Controls.Add(this.btnDieTrReady);
            this.groupBoxManual.Controls.Add(this.btnSyncPickPinRetreat);
            this.groupBoxManual.Controls.Add(this.btnEjecterZUp);
            this.groupBoxManual.Controls.Add(this.btnSafetyZ);
            this.groupBoxManual.Controls.Add(this.btnPickUp);
            this.groupBoxManual.Controls.Add(this.btnPickUpNiddleMove);
            this.groupBoxManual.Controls.Add(this.btnWaferUnloading);
            this.groupBoxManual.Controls.Add(this.btnAlignXY);
            this.groupBoxManual.Controls.Add(this.btnAlignT);
            this.groupBoxManual.Controls.Add(this.btnWaferLoading);
            this.groupBoxManual.Controls.Add(this.buttonTest2);
            this.groupBoxManual.Controls.Add(this.buttonTest);
            this.groupBoxManual.Controls.Add(this.buttonPickUpNiddle_Move);
            this.groupBoxManual.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold);
            this.groupBoxManual.Location = new System.Drawing.Point(630, 420);
            this.groupBoxManual.Name = "groupBoxManual";
            this.groupBoxManual.Size = new System.Drawing.Size(389, 400);
            this.groupBoxManual.TabIndex = 16;
            this.groupBoxManual.TabStop = false;
            this.groupBoxManual.Text = "Manual";
            // 
            // btnPickUpDn
            // 
            this.btnPickUpDn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnPickUpDn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnPickUpDn.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnPickUpDn.CustomFont = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPickUpDn.CustomForeColor = System.Drawing.Color.Black;
            this.btnPickUpDn.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPickUpDn.ForeColor = System.Drawing.Color.Black;
            this.btnPickUpDn.ImageSize = new System.Drawing.Size(45, 45);
            this.btnPickUpDn.Location = new System.Drawing.Point(6, 346);
            this.btnPickUpDn.Name = "btnPickUpDn";
            this.btnPickUpDn.Size = new System.Drawing.Size(130, 35);
            this.btnPickUpDn.TabIndex = 32;
            this.btnPickUpDn.TabStop = false;
            this.btnPickUpDn.Text = "PickUpDn";
            this.btnPickUpDn.UseVisualStyleBackColor = false;
            this.btnPickUpDn.Visible = false;
            this.btnPickUpDn.Click += new System.EventHandler(this.btnPickUpDn_Click);
            // 
            // btnReleaseVacuumAndPlaceUp
            // 
            this.btnReleaseVacuumAndPlaceUp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnReleaseVacuumAndPlaceUp.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnReleaseVacuumAndPlaceUp.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnReleaseVacuumAndPlaceUp.CustomFont = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnReleaseVacuumAndPlaceUp.CustomForeColor = System.Drawing.Color.Black;
            this.btnReleaseVacuumAndPlaceUp.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnReleaseVacuumAndPlaceUp.ForeColor = System.Drawing.Color.Black;
            this.btnReleaseVacuumAndPlaceUp.ImageSize = new System.Drawing.Size(45, 45);
            this.btnReleaseVacuumAndPlaceUp.Location = new System.Drawing.Point(6, 346);
            this.btnReleaseVacuumAndPlaceUp.Name = "btnReleaseVacuumAndPlaceUp";
            this.btnReleaseVacuumAndPlaceUp.Size = new System.Drawing.Size(130, 35);
            this.btnReleaseVacuumAndPlaceUp.TabIndex = 31;
            this.btnReleaseVacuumAndPlaceUp.TabStop = false;
            this.btnReleaseVacuumAndPlaceUp.Text = "PlaceUp";
            this.btnReleaseVacuumAndPlaceUp.UseVisualStyleBackColor = false;
            this.btnReleaseVacuumAndPlaceUp.Visible = false;
            this.btnReleaseVacuumAndPlaceUp.Click += new System.EventHandler(this.btnReleaseVacuumAndPlaceUp_Click);
            // 
            // btnPlaceChipDown
            // 
            this.btnPlaceChipDown.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnPlaceChipDown.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnPlaceChipDown.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnPlaceChipDown.CustomFont = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPlaceChipDown.CustomForeColor = System.Drawing.Color.Black;
            this.btnPlaceChipDown.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPlaceChipDown.ForeColor = System.Drawing.Color.Black;
            this.btnPlaceChipDown.ImageSize = new System.Drawing.Size(45, 45);
            this.btnPlaceChipDown.Location = new System.Drawing.Point(6, 346);
            this.btnPlaceChipDown.Name = "btnPlaceChipDown";
            this.btnPlaceChipDown.Size = new System.Drawing.Size(130, 35);
            this.btnPlaceChipDown.TabIndex = 30;
            this.btnPlaceChipDown.TabStop = false;
            this.btnPlaceChipDown.Text = "PlaceDown";
            this.btnPlaceChipDown.UseVisualStyleBackColor = false;
            this.btnPlaceChipDown.Visible = false;
            this.btnPlaceChipDown.Click += new System.EventHandler(this.btnPlaceChipDown_Click);
            // 
            // btnDieTrReady
            // 
            this.btnDieTrReady.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnDieTrReady.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnDieTrReady.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnDieTrReady.CustomFont = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDieTrReady.CustomForeColor = System.Drawing.Color.Black;
            this.btnDieTrReady.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDieTrReady.ForeColor = System.Drawing.Color.Black;
            this.btnDieTrReady.ImageSize = new System.Drawing.Size(45, 45);
            this.btnDieTrReady.Location = new System.Drawing.Point(6, 346);
            this.btnDieTrReady.Name = "btnDieTrReady";
            this.btnDieTrReady.Size = new System.Drawing.Size(130, 35);
            this.btnDieTrReady.TabIndex = 29;
            this.btnDieTrReady.TabStop = false;
            this.btnDieTrReady.Text = "DieTrReady";
            this.btnDieTrReady.UseVisualStyleBackColor = false;
            this.btnDieTrReady.Visible = false;
            this.btnDieTrReady.Click += new System.EventHandler(this.btnDieTrReady_Click);
            // 
            // btnSyncPickPinRetreat
            // 
            this.btnSyncPickPinRetreat.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnSyncPickPinRetreat.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnSyncPickPinRetreat.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnSyncPickPinRetreat.CustomFont = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSyncPickPinRetreat.CustomForeColor = System.Drawing.Color.Black;
            this.btnSyncPickPinRetreat.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSyncPickPinRetreat.ForeColor = System.Drawing.Color.Black;
            this.btnSyncPickPinRetreat.ImageSize = new System.Drawing.Size(45, 45);
            this.btnSyncPickPinRetreat.Location = new System.Drawing.Point(6, 346);
            this.btnSyncPickPinRetreat.Name = "btnSyncPickPinRetreat";
            this.btnSyncPickPinRetreat.Size = new System.Drawing.Size(130, 35);
            this.btnSyncPickPinRetreat.TabIndex = 28;
            this.btnSyncPickPinRetreat.TabStop = false;
            this.btnSyncPickPinRetreat.Text = "SyncRetreat";
            this.btnSyncPickPinRetreat.UseVisualStyleBackColor = false;
            this.btnSyncPickPinRetreat.Visible = false;
            this.btnSyncPickPinRetreat.Click += new System.EventHandler(this.btnSyncPickPinRetreat_Click);
            // 
            // btnEjecterZUp
            // 
            this.btnEjecterZUp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnEjecterZUp.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnEjecterZUp.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnEjecterZUp.CustomFont = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnEjecterZUp.CustomForeColor = System.Drawing.Color.Black;
            this.btnEjecterZUp.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnEjecterZUp.ForeColor = System.Drawing.Color.Black;
            this.btnEjecterZUp.ImageSize = new System.Drawing.Size(45, 45);
            this.btnEjecterZUp.Location = new System.Drawing.Point(6, 346);
            this.btnEjecterZUp.Name = "btnEjecterZUp";
            this.btnEjecterZUp.Size = new System.Drawing.Size(130, 35);
            this.btnEjecterZUp.TabIndex = 27;
            this.btnEjecterZUp.TabStop = false;
            this.btnEjecterZUp.Text = "EjecterZUp";
            this.btnEjecterZUp.UseVisualStyleBackColor = false;
            this.btnEjecterZUp.Visible = false;
            this.btnEjecterZUp.Click += new System.EventHandler(this.btnEjecterZUp_Click);
            // 
            // btnSafetyZ
            // 
            this.btnSafetyZ.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnSafetyZ.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnSafetyZ.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnSafetyZ.CustomFont = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSafetyZ.CustomForeColor = System.Drawing.Color.Black;
            this.btnSafetyZ.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSafetyZ.ForeColor = System.Drawing.Color.Black;
            this.btnSafetyZ.ImageSize = new System.Drawing.Size(45, 45);
            this.btnSafetyZ.Location = new System.Drawing.Point(6, 29);
            this.btnSafetyZ.Name = "btnSafetyZ";
            this.btnSafetyZ.Size = new System.Drawing.Size(130, 35);
            this.btnSafetyZ.TabIndex = 26;
            this.btnSafetyZ.TabStop = false;
            this.btnSafetyZ.Text = "Tr SafetyZ";
            this.btnSafetyZ.UseVisualStyleBackColor = false;
            this.btnSafetyZ.Click += new System.EventHandler(this.btnSafetyZ_Click);
            // 
            // btnPickUp
            // 
            this.btnPickUp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnPickUp.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnPickUp.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnPickUp.CustomFont = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPickUp.CustomForeColor = System.Drawing.Color.Black;
            this.btnPickUp.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPickUp.ForeColor = System.Drawing.Color.Black;
            this.btnPickUp.ImageSize = new System.Drawing.Size(45, 45);
            this.btnPickUp.Location = new System.Drawing.Point(6, 346);
            this.btnPickUp.Name = "btnPickUp";
            this.btnPickUp.Size = new System.Drawing.Size(130, 35);
            this.btnPickUp.TabIndex = 25;
            this.btnPickUp.TabStop = false;
            this.btnPickUp.Text = "PickDown";
            this.btnPickUp.UseVisualStyleBackColor = false;
            this.btnPickUp.Visible = false;
            this.btnPickUp.Click += new System.EventHandler(this.btnPickUp_Click);
            // 
            // btnPickUpNiddleMove
            // 
            this.btnPickUpNiddleMove.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnPickUpNiddleMove.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnPickUpNiddleMove.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnPickUpNiddleMove.CustomFont = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPickUpNiddleMove.CustomForeColor = System.Drawing.Color.Black;
            this.btnPickUpNiddleMove.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPickUpNiddleMove.ForeColor = System.Drawing.Color.Black;
            this.btnPickUpNiddleMove.ImageSize = new System.Drawing.Size(45, 45);
            this.btnPickUpNiddleMove.Location = new System.Drawing.Point(6, 346);
            this.btnPickUpNiddleMove.Name = "btnPickUpNiddleMove";
            this.btnPickUpNiddleMove.Size = new System.Drawing.Size(130, 35);
            this.btnPickUpNiddleMove.TabIndex = 24;
            this.btnPickUpNiddleMove.TabStop = false;
            this.btnPickUpNiddleMove.Text = "PickUp&&Niddle";
            this.btnPickUpNiddleMove.UseVisualStyleBackColor = false;
            this.btnPickUpNiddleMove.Visible = false;
            this.btnPickUpNiddleMove.Click += new System.EventHandler(this.btnPickUpNiddleMove_Click);
            // 
            // btnWaferUnloading
            // 
            this.btnWaferUnloading.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnWaferUnloading.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnWaferUnloading.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnWaferUnloading.CustomFont = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnWaferUnloading.CustomForeColor = System.Drawing.Color.Black;
            this.btnWaferUnloading.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnWaferUnloading.ForeColor = System.Drawing.Color.Black;
            this.btnWaferUnloading.ImageSize = new System.Drawing.Size(45, 45);
            this.btnWaferUnloading.Location = new System.Drawing.Point(6, 346);
            this.btnWaferUnloading.Name = "btnWaferUnloading";
            this.btnWaferUnloading.Size = new System.Drawing.Size(130, 35);
            this.btnWaferUnloading.TabIndex = 23;
            this.btnWaferUnloading.TabStop = false;
            this.btnWaferUnloading.Text = "WaferUnloading";
            this.btnWaferUnloading.UseVisualStyleBackColor = false;
            this.btnWaferUnloading.Visible = false;
            this.btnWaferUnloading.Click += new System.EventHandler(this.btnWaferUnloading_Click);
            // 
            // btnAlignXY
            // 
            this.btnAlignXY.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnAlignXY.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnAlignXY.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnAlignXY.CustomFont = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAlignXY.CustomForeColor = System.Drawing.Color.Black;
            this.btnAlignXY.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAlignXY.ForeColor = System.Drawing.Color.Black;
            this.btnAlignXY.ImageSize = new System.Drawing.Size(45, 45);
            this.btnAlignXY.Location = new System.Drawing.Point(6, 346);
            this.btnAlignXY.Name = "btnAlignXY";
            this.btnAlignXY.Size = new System.Drawing.Size(130, 35);
            this.btnAlignXY.TabIndex = 22;
            this.btnAlignXY.TabStop = false;
            this.btnAlignXY.Text = "AlignXY";
            this.btnAlignXY.UseVisualStyleBackColor = false;
            this.btnAlignXY.Visible = false;
            this.btnAlignXY.Click += new System.EventHandler(this.btnAlignXY_Click);
            // 
            // btnAlignT
            // 
            this.btnAlignT.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnAlignT.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnAlignT.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnAlignT.CustomFont = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAlignT.CustomForeColor = System.Drawing.Color.Black;
            this.btnAlignT.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAlignT.ForeColor = System.Drawing.Color.Black;
            this.btnAlignT.ImageSize = new System.Drawing.Size(45, 45);
            this.btnAlignT.Location = new System.Drawing.Point(6, 346);
            this.btnAlignT.Name = "btnAlignT";
            this.btnAlignT.Size = new System.Drawing.Size(130, 35);
            this.btnAlignT.TabIndex = 21;
            this.btnAlignT.TabStop = false;
            this.btnAlignT.Text = "AlignT";
            this.btnAlignT.UseVisualStyleBackColor = false;
            this.btnAlignT.Visible = false;
            this.btnAlignT.Click += new System.EventHandler(this.btnAlignT_Click);
            // 
            // btnWaferLoading
            // 
            this.btnWaferLoading.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnWaferLoading.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnWaferLoading.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnWaferLoading.CustomFont = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnWaferLoading.CustomForeColor = System.Drawing.Color.Black;
            this.btnWaferLoading.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnWaferLoading.ForeColor = System.Drawing.Color.Black;
            this.btnWaferLoading.ImageSize = new System.Drawing.Size(45, 45);
            this.btnWaferLoading.Location = new System.Drawing.Point(6, 346);
            this.btnWaferLoading.Name = "btnWaferLoading";
            this.btnWaferLoading.Size = new System.Drawing.Size(130, 35);
            this.btnWaferLoading.TabIndex = 20;
            this.btnWaferLoading.TabStop = false;
            this.btnWaferLoading.Text = "WaferLoading";
            this.btnWaferLoading.UseVisualStyleBackColor = false;
            this.btnWaferLoading.Visible = false;
            this.btnWaferLoading.Click += new System.EventHandler(this.btnWaferLoading_Click);
            // 
            // buttonTest2
            // 
            this.buttonTest2.Location = new System.Drawing.Point(6, 346);
            this.buttonTest2.Name = "buttonTest2";
            this.buttonTest2.Size = new System.Drawing.Size(62, 35);
            this.buttonTest2.TabIndex = 19;
            this.buttonTest2.Text = "Test2";
            this.buttonTest2.UseVisualStyleBackColor = true;
            this.buttonTest2.Visible = false;
            this.buttonTest2.Click += new System.EventHandler(this.buttonTest2_Click);
            // 
            // buttonTest
            // 
            this.buttonTest.Location = new System.Drawing.Point(6, 346);
            this.buttonTest.Name = "buttonTest";
            this.buttonTest.Size = new System.Drawing.Size(104, 35);
            this.buttonTest.TabIndex = 18;
            this.buttonTest.Text = "SEQ STOP";
            this.buttonTest.UseVisualStyleBackColor = true;
            this.buttonTest.Visible = false;
            this.buttonTest.Click += new System.EventHandler(this.btnSeqStop_Click);
            // 
            // buttonPickUpNiddle_Move
            // 
            this.buttonPickUpNiddle_Move.Location = new System.Drawing.Point(6, 346);
            this.buttonPickUpNiddle_Move.Name = "buttonPickUpNiddle_Move";
            this.buttonPickUpNiddle_Move.Size = new System.Drawing.Size(62, 35);
            this.buttonPickUpNiddle_Move.TabIndex = 17;
            this.buttonPickUpNiddle_Move.Text = "PickUp && Niddle Move";
            this.buttonPickUpNiddle_Move.UseVisualStyleBackColor = true;
            this.buttonPickUpNiddle_Move.Visible = false;
            this.buttonPickUpNiddle_Move.Click += new System.EventHandler(this.buttonPickUpNiddle_Move_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.dioControl);
            this.panel1.Location = new System.Drawing.Point(8, 420);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(615, 400);
            this.panel1.TabIndex = 20;
            // 
            // dioControl
            // 
            this.dioControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dioControl.IoSortMode = QMC.LCP_280.Process.Component.DIOControl.SortingMode.AlphabeticalKey;
            this.dioControl.Location = new System.Drawing.Point(0, 0);
            this.dioControl.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dioControl.Name = "dioControl";
            this.dioControl.RefreshIntervalMs = 400;
            this.dioControl.Size = new System.Drawing.Size(615, 400);
            this.dioControl.TabIndex = 17;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.teachingPositionControl);
            this.panel2.Location = new System.Drawing.Point(8, 9);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(615, 400);
            this.panel2.TabIndex = 21;
            // 
            // teachingPositionControl
            // 
            this.teachingPositionControl.ButtonSize = new System.Drawing.Size(90, 32);
            this.teachingPositionControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.teachingPositionControl.Location = new System.Drawing.Point(0, 0);
            this.teachingPositionControl.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.teachingPositionControl.Name = "teachingPositionControl";
            this.teachingPositionControl.ShowCancelButton = true;
            this.teachingPositionControl.ShowSaveButton = true;
            this.teachingPositionControl.Size = new System.Drawing.Size(615, 400);
            this.teachingPositionControl.TabIndex = 18;
            this.teachingPositionControl.UnitName = null;
            // 
            // manualSequenceControl
            // 
            this.manualSequenceControl.Dock = System.Windows.Forms.DockStyle.Top;
            this.manualSequenceControl.Location = new System.Drawing.Point(3, 26);
            this.manualSequenceControl.Margin = new System.Windows.Forms.Padding(2);
            this.manualSequenceControl.Name = "manualSequenceControl";
            this.manualSequenceControl.ParentUnit = null;
            this.manualSequenceControl.Size = new System.Drawing.Size(389, 309);
            this.manualSequenceControl.TabIndex = 19;
            // 
            // groupBoxManualTrSeq
            // 
            this.groupBoxManualTrSeq.Controls.Add(this.manualSequenceControl);
            this.groupBoxManualTrSeq.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.groupBoxManualTrSeq.Location = new System.Drawing.Point(1025, 9);
            this.groupBoxManualTrSeq.Name = "groupBoxManualTrSeq";
            this.groupBoxManualTrSeq.Size = new System.Drawing.Size(395, 335);
            this.groupBoxManualTrSeq.TabIndex = 23;
            this.groupBoxManualTrSeq.TabStop = false;
            this.groupBoxManualTrSeq.Text = "ChipLoading Seq.";
            // 
            // btnTest
            // 
            this.btnTest.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnTest.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnTest.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnTest.CustomFont = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnTest.CustomForeColor = System.Drawing.Color.Black;
            this.btnTest.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnTest.ForeColor = System.Drawing.Color.Black;
            this.btnTest.ImageSize = new System.Drawing.Size(45, 45);
            this.btnTest.Location = new System.Drawing.Point(253, 29);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(130, 35);
            this.btnTest.TabIndex = 33;
            this.btnTest.TabStop = false;
            this.btnTest.Text = "test1";
            this.btnTest.UseVisualStyleBackColor = false;
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // ChipLoading_Working
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1580, 939);
            this.Controls.Add(this.groupBoxManualTrSeq);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.groupBoxManual);
            this.Controls.Add(this.groupBoxImageView);
            this.Controls.Add(this.buttonDataManual);
            this.Controls.Add(this._btnVisionSetting);
            this.Name = "ChipLoading_Working";
            this.Text = "ChipLoading Working";
            this.groupBoxImageView.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._ChipLoadingCameraviewer)).EndInit();
            this.groupBoxManual.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.groupBoxManualTrSeq.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private ManualSequenceControl manualSequenceControl;
        private TeachingPositionControl teachingPositionControl;
        private DIOControl dioControl;
        private Common.Vision.VisionImageViewer _ChipLoadingCameraviewer;
        private Button _btnVisionSetting;
        private Button buttonDataManual;
        private GroupBox groupBoxImageView;
        private GroupBox groupBoxManual;
        private IndividualMenuButton btnSafetyZ;
        private Panel panel1;
        private Panel panel2;
        private GroupBox groupBoxManualTrSeq;
        private IndividualMenuButton btnPickUpDn;
        private IndividualMenuButton btnReleaseVacuumAndPlaceUp;
        private IndividualMenuButton btnPlaceChipDown;
        private IndividualMenuButton btnDieTrReady;
        private IndividualMenuButton btnSyncPickPinRetreat;
        private IndividualMenuButton btnEjecterZUp;
        private IndividualMenuButton btnPickUp;
        private IndividualMenuButton btnPickUpNiddleMove;
        private IndividualMenuButton btnWaferUnloading;
        private IndividualMenuButton btnAlignXY;
        private IndividualMenuButton btnAlignT;
        private IndividualMenuButton btnWaferLoading;
        private Button buttonTest2;
        private Button buttonTest;
        private Button buttonPickUpNiddle_Move;
        private IndividualMenuButton btnTest;
    }
}