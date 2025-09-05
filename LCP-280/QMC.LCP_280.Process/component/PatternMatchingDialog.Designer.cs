using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using QMC.Common.Vision;
using QMC.Common.VisionPart;
using QMC.Common;

namespace QMC.LCP_280.Process
{
    public partial class PatternMatchingDialog
    {
        private IContainer components = null;
        private VisionImageViewer _viewer;
        private Button _btnLoadImage;
        private Button _btnSearch;
        private Button _btnClose;
        private Label _lblStatus;
        private ListBoxItemsView cameraListBoxItemsView;
        private TabControl tabControl_Vision; // Reused as Pattern Matching tab container
        private TabPage tabPageROI;
        private TabPage tabPageParam;
        private TabPage tabPageResult;
        private Button _btnSaveImage;
        private Button _btnSaveParam;   // Added: Save Parameter (Recipe)
        private Button _btnLoadParam;   // Added: Load Parameter (Recipe)
        private MaintROIControl maintROIControl;
        private MultiPatternMatchingParameterControl patternMatchingParamControl; // switched to MultiPatternMatchingParameterControl
        private PatternMatchingResultControl patternMatchingResultControl;
        private TextBox txtResultX; // Added result display controls
        private TextBox txtResultY;
        private TextBox txtResultT;
        private Label lblRX;
        private Label lblRY;
        private Label lblRT;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this._btnLoadImage = new System.Windows.Forms.Button();
            this._btnSearch = new System.Windows.Forms.Button();
            this._btnClose = new System.Windows.Forms.Button();
            this._lblStatus = new System.Windows.Forms.Label();
            this.tabControl_Vision = new System.Windows.Forms.TabControl();
            this.tabPageROI = new System.Windows.Forms.TabPage();
            this.tabPageParam = new System.Windows.Forms.TabPage();
            this.tabPageResult = new System.Windows.Forms.TabPage();
            this._btnSaveImage = new System.Windows.Forms.Button();
            this._btnSaveParam = new System.Windows.Forms.Button();
            this._btnLoadParam = new System.Windows.Forms.Button();
            this.txtResultX = new System.Windows.Forms.TextBox();
            this.txtResultY = new System.Windows.Forms.TextBox();
            this.txtResultT = new System.Windows.Forms.TextBox();
            this.lblRX = new System.Windows.Forms.Label();
            this.lblRY = new System.Windows.Forms.Label();
            this.lblRT = new System.Windows.Forms.Label();
            this.maintROIControl = new QMC.Common.MaintROIControl();
            this.patternMatchingParamControl = new QMC.Common.MultiPatternMatchingParameterControl();
            this.patternMatchingResultControl = new QMC.Common.PatternMatchingResultControl();
            this.cameraListBoxItemsView = new QMC.Common.ListBoxItemsView();
            this._viewer = new QMC.Common.Vision.VisionImageViewer();
            this.tabControl_Vision.SuspendLayout();
            this.tabPageROI.SuspendLayout();
            this.tabPageParam.SuspendLayout();
            this.tabPageResult.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._viewer)).BeginInit();
            this.SuspendLayout();
            // 
            // _btnLoadImage
            // 
            this._btnLoadImage.Location = new System.Drawing.Point(6, 252);
            this._btnLoadImage.Name = "_btnLoadImage";
            this._btnLoadImage.Size = new System.Drawing.Size(90, 28);
            this._btnLoadImage.TabIndex = 0;
            this._btnLoadImage.Text = "Load Image";
            // 
            // _btnSearch
            // 
            this._btnSearch.Location = new System.Drawing.Point(6, 286);
            this._btnSearch.Name = "_btnSearch";
            this._btnSearch.Size = new System.Drawing.Size(90, 28);
            this._btnSearch.TabIndex = 1;
            this._btnSearch.Text = "Search";
            this._btnSearch.Click += new System.EventHandler(this.BtnSearch_Click);
            // 
            // _btnClose
            // 
            this._btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._btnClose.Location = new System.Drawing.Point(814, 2);
            this._btnClose.Name = "_btnClose";
            this._btnClose.Size = new System.Drawing.Size(70, 28);
            this._btnClose.TabIndex = 2;
            this._btnClose.Text = "Close";
            // 
            // _lblStatus
            // 
            this._lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._lblStatus.Location = new System.Drawing.Point(6, 5);
            this._lblStatus.Name = "_lblStatus";
            this._lblStatus.Size = new System.Drawing.Size(610, 22);
            this._lblStatus.TabIndex = 0;
            this._lblStatus.Text = "Ready";
            this._lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tabControl_Vision
            // 
            this.tabControl_Vision.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_Vision.Controls.Add(this.tabPageROI);
            this.tabControl_Vision.Controls.Add(this.tabPageParam);
            this.tabControl_Vision.Controls.Add(this.tabPageResult);
            this.tabControl_Vision.Location = new System.Drawing.Point(224, 411);
            this.tabControl_Vision.Name = "tabControl_Vision";
            this.tabControl_Vision.SelectedIndex = 0;
            this.tabControl_Vision.Size = new System.Drawing.Size(560, 500);
            this.tabControl_Vision.TabIndex = 4;
            // 
            // tabPageROI
            // 
            this.tabPageROI.Controls.Add(this.maintROIControl);
            this.tabPageROI.Location = new System.Drawing.Point(4, 22);
            this.tabPageROI.Name = "tabPageROI";
            this.tabPageROI.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageROI.Size = new System.Drawing.Size(552, 474);
            this.tabPageROI.TabIndex = 0;
            this.tabPageROI.Text = "ROI";
            this.tabPageROI.UseVisualStyleBackColor = true;
            // 
            // tabPageParam
            // 
            this.tabPageParam.Controls.Add(this.patternMatchingParamControl);
            this.tabPageParam.Location = new System.Drawing.Point(4, 22);
            this.tabPageParam.Name = "tabPageParam";
            this.tabPageParam.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageParam.Size = new System.Drawing.Size(552, 474);
            this.tabPageParam.TabIndex = 1;
            this.tabPageParam.Text = "Parameter";
            this.tabPageParam.UseVisualStyleBackColor = true;
            // 
            // tabPageResult
            // 
            this.tabPageResult.Controls.Add(this.patternMatchingResultControl);
            this.tabPageResult.Location = new System.Drawing.Point(4, 22);
            this.tabPageResult.Name = "tabPageResult";
            this.tabPageResult.Size = new System.Drawing.Size(552, 474);
            this.tabPageResult.TabIndex = 2;
            this.tabPageResult.Text = "Result";
            this.tabPageResult.UseVisualStyleBackColor = true;
            // 
            // _btnSaveImage
            // 
            this._btnSaveImage.Location = new System.Drawing.Point(128, 252);
            this._btnSaveImage.Name = "_btnSaveImage";
            this._btnSaveImage.Size = new System.Drawing.Size(90, 28);
            this._btnSaveImage.TabIndex = 5;
            this._btnSaveImage.Text = "Save Image";
            // 
            // _btnSaveParam
            // 
            this._btnSaveParam.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._btnSaveParam.Location = new System.Drawing.Point(630, 2);
            this._btnSaveParam.Name = "_btnSaveParam";
            this._btnSaveParam.Size = new System.Drawing.Size(90, 28);
            this._btnSaveParam.TabIndex = 6;
            this._btnSaveParam.Text = "Save Param";
            // 
            // _btnLoadParam
            // 
            this._btnLoadParam.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._btnLoadParam.Location = new System.Drawing.Point(722, 2);
            this._btnLoadParam.Name = "_btnLoadParam";
            this._btnLoadParam.Size = new System.Drawing.Size(90, 28);
            this._btnLoadParam.TabIndex = 7;
            this._btnLoadParam.Text = "Load Param";
            // 
            // txtResultX
            // 
            this.txtResultX.Location = new System.Drawing.Point(32, 318);
            this.txtResultX.Name = "txtResultX";
            this.txtResultX.ReadOnly = true;
            this.txtResultX.Size = new System.Drawing.Size(86, 21);
            this.txtResultX.TabIndex = 5;
            // 
            // txtResultY
            // 
            this.txtResultY.Location = new System.Drawing.Point(32, 344);
            this.txtResultY.Name = "txtResultY";
            this.txtResultY.ReadOnly = true;
            this.txtResultY.Size = new System.Drawing.Size(86, 21);
            this.txtResultY.TabIndex = 4;
            // 
            // txtResultT
            // 
            this.txtResultT.Location = new System.Drawing.Point(32, 370);
            this.txtResultT.Name = "txtResultT";
            this.txtResultT.ReadOnly = true;
            this.txtResultT.Size = new System.Drawing.Size(86, 21);
            this.txtResultT.TabIndex = 3;
            // 
            // lblRX
            // 
            this.lblRX.AutoSize = true;
            this.lblRX.Location = new System.Drawing.Point(6, 322);
            this.lblRX.Name = "lblRX";
            this.lblRX.Size = new System.Drawing.Size(17, 12);
            this.lblRX.TabIndex = 2;
            this.lblRX.Text = "X:";
            // 
            // lblRY
            // 
            this.lblRY.AutoSize = true;
            this.lblRY.Location = new System.Drawing.Point(6, 348);
            this.lblRY.Name = "lblRY";
            this.lblRY.Size = new System.Drawing.Size(17, 12);
            this.lblRY.TabIndex = 1;
            this.lblRY.Text = "Y:";
            // 
            // lblRT
            // 
            this.lblRT.AutoSize = true;
            this.lblRT.Location = new System.Drawing.Point(6, 374);
            this.lblRT.Name = "lblRT";
            this.lblRT.Size = new System.Drawing.Size(17, 12);
            this.lblRT.TabIndex = 0;
            this.lblRT.Text = "T:";
            // 
            // maintROIControl
            // 
            this.maintROIControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.maintROIControl.Location = new System.Drawing.Point(3, 3);
            this.maintROIControl.Name = "maintROIControl";
            this.maintROIControl.Size = new System.Drawing.Size(546, 468);
            this.maintROIControl.TabIndex = 0;
            // 
            // patternMatchingParamControl
            // 
            this.patternMatchingParamControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.patternMatchingParamControl.DuplicateChecked = false;
            this.patternMatchingParamControl.LearnImage = null;
            this.patternMatchingParamControl.Location = new System.Drawing.Point(3, 3);
            this.patternMatchingParamControl.MaxInstnce = 0;
            this.patternMatchingParamControl.MinScore = 0D;
            this.patternMatchingParamControl.Name = "patternMatchingParamControl";
            this.patternMatchingParamControl.SelectedIndex = 0;
            this.patternMatchingParamControl.Size = new System.Drawing.Size(546, 468);
            this.patternMatchingParamControl.TabIndex = 0;
            this.patternMatchingParamControl.Tolerance = 0D;
            this.patternMatchingParamControl.TrainImage = null;
            this.patternMatchingParamControl.UseMaskImage = false;
            this.patternMatchingParamControl.Load += new System.EventHandler(this.patternMatchingParamControl_Load);
            // 
            // patternMatchingResultControl
            // 
            this.patternMatchingResultControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.patternMatchingResultControl.Location = new System.Drawing.Point(0, 0);
            this.patternMatchingResultControl.Name = "patternMatchingResultControl";
            this.patternMatchingResultControl.Size = new System.Drawing.Size(552, 474);
            this.patternMatchingResultControl.TabIndex = 0;
            // 
            // cameraListBoxItemsView
            // 
            this.cameraListBoxItemsView.BorderWidth = 2;
            this.cameraListBoxItemsView.GroupName = "Camera";
            this.cameraListBoxItemsView.Location = new System.Drawing.Point(6, 33);
            this.cameraListBoxItemsView.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.cameraListBoxItemsView.Name = "cameraListBoxItemsView";
            this.cameraListBoxItemsView.SelectedIndex = -1;
            this.cameraListBoxItemsView.Size = new System.Drawing.Size(212, 210);
            this.cameraListBoxItemsView.TabIndex = 3;
            // 
            // _viewer
            // 
            this._viewer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._viewer.BackColor = System.Drawing.Color.Black;
            this._viewer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._viewer.Camera = null;
            this._viewer.CameraSwitch = null;
            this._viewer.FrameRate = 1D;
            this._viewer.InputImage = null;
            this._viewer.IsViewCustomizedImage = false;
            this._viewer.Location = new System.Drawing.Point(224, 33);
            this._viewer.Name = "_viewer";
            this._viewer.OperatingType = QMC.Common.Vision.VisionImageViewer.OperatingTypes.Center;
            this._viewer.Simulated = false;
            this._viewer.Size = new System.Drawing.Size(560, 372);
            this._viewer.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this._viewer.TabIndex = 0;
            this._viewer.TabStop = false;
            this._viewer.UpdateDelayTime = 80;
            this._viewer.VisibleCrossLine = true;
            // 
            // PatternMatchingDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(894, 950);
            this.Controls.Add(this.lblRT);
            this.Controls.Add(this.lblRY);
            this.Controls.Add(this.lblRX);
            this.Controls.Add(this.txtResultT);
            this.Controls.Add(this.txtResultY);
            this.Controls.Add(this.txtResultX);
            this.Controls.Add(this._btnLoadParam);
            this.Controls.Add(this._btnSaveParam);
            this.Controls.Add(this._btnSaveImage);
            this.Controls.Add(this.tabControl_Vision);
            this.Controls.Add(this.cameraListBoxItemsView);
            this.Controls.Add(this._viewer);
            this.Controls.Add(this._lblStatus);
            this.Controls.Add(this._btnClose);
            this.Controls.Add(this._btnSearch);
            this.Controls.Add(this._btnLoadImage);
            this.MinimumSize = new System.Drawing.Size(910, 900);
            this.Name = "PatternMatchingDialog";
            this.Text = "Pattern Matching";
            this.tabControl_Vision.ResumeLayout(false);
            this.tabPageROI.ResumeLayout(false);
            this.tabPageParam.ResumeLayout(false);
            this.tabPageResult.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._viewer)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}