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
            this._ChipLoadingCameraviewer = new QMC.Common.Vision.VisionImageViewer();
            this.groupBoxImageView = new System.Windows.Forms.GroupBox();
            this.groupBoxManual = new System.Windows.Forms.GroupBox();
            this.btnPickUpNiddleMove = new QMC.Common.IndividualMenuButton();
            this.btnWaferUnloading = new QMC.Common.IndividualMenuButton();
            this.btnAlignXY = new QMC.Common.IndividualMenuButton();
            this.btnAlignT = new QMC.Common.IndividualMenuButton();
            this.btnWaferLoading = new QMC.Common.IndividualMenuButton();
            this.buttonTest2 = new System.Windows.Forms.Button();
            this.buttonTest = new System.Windows.Forms.Button();
            this.buttonPickUpNiddle_Move = new System.Windows.Forms.Button();
            this.manualSequenceControl = new QMC.LCP_280.Process.Component.ManualSequenceControl();
            this.teachingPositionControl = new QMC.LCP_280.Process.Component.TeachingPositionControl();
            this.dioControl = new QMC.LCP_280.Process.Component.DIOControl();
            this.btnPickUp = new QMC.Common.IndividualMenuButton();
            ((System.ComponentModel.ISupportInitialize)(this._ChipLoadingCameraviewer)).BeginInit();
            this.groupBoxImageView.SuspendLayout();
            this.groupBoxManual.SuspendLayout();
            this.SuspendLayout();
            // 
            // _btnVisionSetting
            // 
            this._btnVisionSetting.Location = new System.Drawing.Point(1019, 296);
            this._btnVisionSetting.Name = "_btnVisionSetting";
            this._btnVisionSetting.Size = new System.Drawing.Size(110, 35);
            this._btnVisionSetting.TabIndex = 13;
            this._btnVisionSetting.Text = "VisionSetting";
            this._btnVisionSetting.UseVisualStyleBackColor = true;
            this._btnVisionSetting.Click += new System.EventHandler(this._btnVisionSetting_Click);
            // 
            // buttonDataManual
            // 
            this.buttonDataManual.Location = new System.Drawing.Point(1145, 296);
            this.buttonDataManual.Name = "buttonDataManual";
            this.buttonDataManual.Size = new System.Drawing.Size(95, 35);
            this.buttonDataManual.TabIndex = 14;
            this.buttonDataManual.Text = "ManualData";
            this.buttonDataManual.UseVisualStyleBackColor = true;
            this.buttonDataManual.Click += new System.EventHandler(this.buttonDataManual_Click);
            // 
            // _ChipLoadingCameraviewer
            // 
            this._ChipLoadingCameraviewer.BackColor = System.Drawing.Color.Black;
            this._ChipLoadingCameraviewer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._ChipLoadingCameraviewer.Camera = null;
            this._ChipLoadingCameraviewer.CameraSwitch = null;
            this._ChipLoadingCameraviewer.FrameRate = 1D;
            this._ChipLoadingCameraviewer.InputImage = null;
            this._ChipLoadingCameraviewer.IsViewCustomizedImage = false;
            this._ChipLoadingCameraviewer.Location = new System.Drawing.Point(6, 20);
            this._ChipLoadingCameraviewer.Name = "_ChipLoadingCameraviewer";
            this._ChipLoadingCameraviewer.OperatingType = QMC.Common.Vision.VisionImageViewer.OperatingTypes.Center;
            this._ChipLoadingCameraviewer.Simulated = false;
            this._ChipLoadingCameraviewer.Size = new System.Drawing.Size(358, 324);
            this._ChipLoadingCameraviewer.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this._ChipLoadingCameraviewer.TabIndex = 12;
            this._ChipLoadingCameraviewer.TabStop = false;
            this._ChipLoadingCameraviewer.UpdateDelayTime = 80;
            this._ChipLoadingCameraviewer.VisibleCrossLine = true;
            // 
            // groupBoxImageView
            // 
            this.groupBoxImageView.Controls.Add(this._ChipLoadingCameraviewer);
            this.groupBoxImageView.Location = new System.Drawing.Point(643, 12);
            this.groupBoxImageView.Name = "groupBoxImageView";
            this.groupBoxImageView.Size = new System.Drawing.Size(370, 350);
            this.groupBoxImageView.TabIndex = 15;
            this.groupBoxImageView.TabStop = false;
            this.groupBoxImageView.Text = "ImageView";
            // 
            // groupBoxManual
            // 
            this.groupBoxManual.Controls.Add(this.btnPickUp);
            this.groupBoxManual.Controls.Add(this.btnPickUpNiddleMove);
            this.groupBoxManual.Controls.Add(this.btnWaferUnloading);
            this.groupBoxManual.Controls.Add(this.btnAlignXY);
            this.groupBoxManual.Controls.Add(this.btnAlignT);
            this.groupBoxManual.Controls.Add(this.btnWaferLoading);
            this.groupBoxManual.Controls.Add(this.buttonTest2);
            this.groupBoxManual.Controls.Add(this.buttonTest);
            this.groupBoxManual.Controls.Add(this.buttonPickUpNiddle_Move);
            this.groupBoxManual.Location = new System.Drawing.Point(618, 370);
            this.groupBoxManual.Name = "groupBoxManual";
            this.groupBoxManual.Size = new System.Drawing.Size(395, 263);
            this.groupBoxManual.TabIndex = 16;
            this.groupBoxManual.TabStop = false;
            this.groupBoxManual.Text = "Manual";
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
            this.btnPickUpNiddleMove.Location = new System.Drawing.Point(142, 102);
            this.btnPickUpNiddleMove.Name = "btnPickUpNiddleMove";
            this.btnPickUpNiddleMove.Size = new System.Drawing.Size(130, 35);
            this.btnPickUpNiddleMove.TabIndex = 24;
            this.btnPickUpNiddleMove.TabStop = false;
            this.btnPickUpNiddleMove.Text = "PickUp&&Niddle";
            this.btnPickUpNiddleMove.UseVisualStyleBackColor = false;
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
            this.btnWaferUnloading.Location = new System.Drawing.Point(6, 184);
            this.btnWaferUnloading.Name = "btnWaferUnloading";
            this.btnWaferUnloading.Size = new System.Drawing.Size(130, 35);
            this.btnWaferUnloading.TabIndex = 23;
            this.btnWaferUnloading.TabStop = false;
            this.btnWaferUnloading.Text = "WaferUnloading";
            this.btnWaferUnloading.UseVisualStyleBackColor = false;
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
            this.btnAlignXY.Location = new System.Drawing.Point(6, 143);
            this.btnAlignXY.Name = "btnAlignXY";
            this.btnAlignXY.Size = new System.Drawing.Size(130, 35);
            this.btnAlignXY.TabIndex = 22;
            this.btnAlignXY.TabStop = false;
            this.btnAlignXY.Text = "AlignXY";
            this.btnAlignXY.UseVisualStyleBackColor = false;
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
            this.btnAlignT.Location = new System.Drawing.Point(6, 102);
            this.btnAlignT.Name = "btnAlignT";
            this.btnAlignT.Size = new System.Drawing.Size(130, 35);
            this.btnAlignT.TabIndex = 21;
            this.btnAlignT.TabStop = false;
            this.btnAlignT.Text = "AlignT";
            this.btnAlignT.UseVisualStyleBackColor = false;
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
            this.btnWaferLoading.Location = new System.Drawing.Point(6, 61);
            this.btnWaferLoading.Name = "btnWaferLoading";
            this.btnWaferLoading.Size = new System.Drawing.Size(130, 35);
            this.btnWaferLoading.TabIndex = 20;
            this.btnWaferLoading.TabStop = false;
            this.btnWaferLoading.Text = "WaferLoading";
            this.btnWaferLoading.UseVisualStyleBackColor = false;
            this.btnWaferLoading.Click += new System.EventHandler(this.btnWaferLoading_Click);
            // 
            // buttonTest2
            // 
            this.buttonTest2.Location = new System.Drawing.Point(302, 61);
            this.buttonTest2.Name = "buttonTest2";
            this.buttonTest2.Size = new System.Drawing.Size(62, 35);
            this.buttonTest2.TabIndex = 19;
            this.buttonTest2.Text = "Test2";
            this.buttonTest2.UseVisualStyleBackColor = true;
            this.buttonTest2.Click += new System.EventHandler(this.buttonTest2_Click);
            // 
            // buttonTest
            // 
            this.buttonTest.Location = new System.Drawing.Point(302, 20);
            this.buttonTest.Name = "buttonTest";
            this.buttonTest.Size = new System.Drawing.Size(62, 35);
            this.buttonTest.TabIndex = 18;
            this.buttonTest.Text = "Test1";
            this.buttonTest.UseVisualStyleBackColor = true;
            this.buttonTest.Click += new System.EventHandler(this.buttonTest_Click);
            // 
            // buttonPickUpNiddle_Move
            // 
            this.buttonPickUpNiddle_Move.Location = new System.Drawing.Point(6, 20);
            this.buttonPickUpNiddle_Move.Name = "buttonPickUpNiddle_Move";
            this.buttonPickUpNiddle_Move.Size = new System.Drawing.Size(191, 35);
            this.buttonPickUpNiddle_Move.TabIndex = 17;
            this.buttonPickUpNiddle_Move.Text = "PickUp && Niddle Move";
            this.buttonPickUpNiddle_Move.UseVisualStyleBackColor = true;
            this.buttonPickUpNiddle_Move.Click += new System.EventHandler(this.buttonPickUpNiddle_Move_Click);
            // 
            // manualSequenceControl
            // 
            this.manualSequenceControl.Location = new System.Drawing.Point(1019, 12);
            this.manualSequenceControl.Name = "manualSequenceControl";
            this.manualSequenceControl.Size = new System.Drawing.Size(362, 295);
            this.manualSequenceControl.TabIndex = 19;
            // 
            // teachingPositionControl
            // 
            this.teachingPositionControl.ButtonSize = new System.Drawing.Size(90, 32);
            this.teachingPositionControl.Location = new System.Drawing.Point(6, 12);
            this.teachingPositionControl.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.teachingPositionControl.Name = "teachingPositionControl";
            this.teachingPositionControl.ShowCancelButton = true;
            this.teachingPositionControl.ShowSaveButton = true;
            this.teachingPositionControl.Size = new System.Drawing.Size(631, 350);
            this.teachingPositionControl.TabIndex = 18;
            this.teachingPositionControl.UnitName = null;
            // 
            // dioControl
            // 
            this.dioControl.IoSortMode = QMC.LCP_280.Process.Component.DIOControl.SortingMode.AlphabeticalKey;
            this.dioControl.Location = new System.Drawing.Point(6, 370);
            this.dioControl.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dioControl.Name = "dioControl";
            this.dioControl.RefreshIntervalMs = 400;
            this.dioControl.Size = new System.Drawing.Size(606, 341);
            this.dioControl.TabIndex = 17;
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
            this.btnPickUp.Location = new System.Drawing.Point(142, 61);
            this.btnPickUp.Name = "btnPickUp";
            this.btnPickUp.Size = new System.Drawing.Size(130, 35);
            this.btnPickUp.TabIndex = 25;
            this.btnPickUp.TabStop = false;
            this.btnPickUp.Text = "PickUp";
            this.btnPickUp.UseVisualStyleBackColor = false;
            this.btnPickUp.Click += new System.EventHandler(this.btnPickUp_Click);
            // 
            // ChipLoading_Working
            // 
            this.ClientSize = new System.Drawing.Size(1389, 751);
            this.Controls.Add(this.groupBoxManual);
            this.Controls.Add(this.manualSequenceControl);
            this.Controls.Add(this.groupBoxImageView);
            this.Controls.Add(this.buttonDataManual);
            this.Controls.Add(this._btnVisionSetting);
            this.Controls.Add(this.dioControl);
            this.Controls.Add(this.teachingPositionControl);
            this.Name = "ChipLoading_Working";
            this.Text = "ChipLoading Working";
            ((System.ComponentModel.ISupportInitialize)(this._ChipLoadingCameraviewer)).EndInit();
            this.groupBoxImageView.ResumeLayout(false);
            this.groupBoxManual.ResumeLayout(false);
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
        private Button buttonPickUpNiddle_Move;
        private Button buttonTest;
        private Button buttonTest2;
        private IndividualMenuButton btnWaferLoading;
        private IndividualMenuButton btnWaferUnloading;
        private IndividualMenuButton btnAlignXY;
        private IndividualMenuButton btnAlignT;
        private IndividualMenuButton btnPickUpNiddleMove;
        private IndividualMenuButton btnPickUp;
    }
}