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
        private Button _btnSearch;
        private Button _btnClose;
        private Label _lblStatus;
        private ListBoxItemsView cameraListBoxItemsView;
        private TabControl tabControl_Vision; // Reused as Pattern Matching tab container
        private TabPage tabPageROI;
        private TabPage tabPageParam;
        private TabPage tabPageResult;
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
        private ListView listViewResults; // NEW: multi result list
        private ColumnHeader colIdx;
        private ColumnHeader colX;
        private ColumnHeader colY;
        private ColumnHeader colT;
        private ColumnHeader colScore;
        private GroupBox groupSearchMode;
        private RadioButton radioSingle;
        private RadioButton radioMulti;
        private Label lblAvgX;
        private Label lblAvgY;
        private Label lblAvgT;
        private TextBox txtAvgX;
        private TextBox txtAvgY;
        private TextBox txtAvgT;
        private CheckBox chkShowIndexes;
        private CheckBox chkHighlightRef;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this._btnSearch = new System.Windows.Forms.Button();
            this._btnClose = new System.Windows.Forms.Button();
            this._lblStatus = new System.Windows.Forms.Label();
            this.tabControl_Vision = new System.Windows.Forms.TabControl();
            this.tabPageROI = new System.Windows.Forms.TabPage();
            this.maintROIControl = new QMC.Common.MaintROIControl();
            this.tabPageParam = new System.Windows.Forms.TabPage();
            this.patternMatchingParamControl = new QMC.Common.MultiPatternMatchingParameterControl();
            this.tabPageResult = new System.Windows.Forms.TabPage();
            this.patternMatchingResultControl = new QMC.Common.PatternMatchingResultControl();
            this._btnSaveParam = new System.Windows.Forms.Button();
            this._btnLoadParam = new System.Windows.Forms.Button();
            this.txtResultX = new System.Windows.Forms.TextBox();
            this.txtResultY = new System.Windows.Forms.TextBox();
            this.txtResultT = new System.Windows.Forms.TextBox();
            this.lblRX = new System.Windows.Forms.Label();
            this.lblRY = new System.Windows.Forms.Label();
            this.lblRT = new System.Windows.Forms.Label();
            this.cameraListBoxItemsView = new QMC.Common.ListBoxItemsView();
            this.listViewResults = new System.Windows.Forms.ListView();
            this.colIdx = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colX = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colY = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colT = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colScore = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._viewer = new QMC.Common.Vision.VisionImageViewer();
            this.groupSearchMode = new System.Windows.Forms.GroupBox();
            this.radioSingle = new System.Windows.Forms.RadioButton();
            this.radioMulti = new System.Windows.Forms.RadioButton();
            this.lblAvgX = new System.Windows.Forms.Label();
            this.lblAvgY = new System.Windows.Forms.Label();
            this.lblAvgT = new System.Windows.Forms.Label();
            this.txtAvgX = new System.Windows.Forms.TextBox();
            this.txtAvgY = new System.Windows.Forms.TextBox();
            this.txtAvgT = new System.Windows.Forms.TextBox();
            this.chkShowIndexes = new System.Windows.Forms.CheckBox();
            this.chkHighlightRef = new System.Windows.Forms.CheckBox();
            this.tabControl_Vision.SuspendLayout();
            this.tabPageROI.SuspendLayout();
            this.tabPageParam.SuspendLayout();
            this.tabPageResult.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._viewer)).BeginInit();
            this.groupSearchMode.SuspendLayout();
            this.SuspendLayout();
            // 
            // _btnSearch
            // 
            this._btnSearch.Location = new System.Drawing.Point(6, 249);
            this._btnSearch.Name = "_btnSearch";
            this._btnSearch.Size = new System.Drawing.Size(212, 100);
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
            this.tabControl_Vision.Location = new System.Drawing.Point(325, 444);
            this.tabControl_Vision.Name = "tabControl_Vision";
            this.tabControl_Vision.SelectedIndex = 0;
            this.tabControl_Vision.Size = new System.Drawing.Size(530, 500);
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
            // maintROIControl
            // 
            this.maintROIControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.maintROIControl.Location = new System.Drawing.Point(3, 3);
            this.maintROIControl.Name = "maintROIControl";
            this.maintROIControl.Size = new System.Drawing.Size(546, 468);
            this.maintROIControl.TabIndex = 0;
            // 
            // tabPageParam
            // 
            this.tabPageParam.Controls.Add(this.patternMatchingParamControl);
            this.tabPageParam.Location = new System.Drawing.Point(4, 22);
            this.tabPageParam.Name = "tabPageParam";
            this.tabPageParam.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageParam.Size = new System.Drawing.Size(522, 474);
            this.tabPageParam.TabIndex = 1;
            this.tabPageParam.Text = "Parameter";
            this.tabPageParam.UseVisualStyleBackColor = true;
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
            this.patternMatchingParamControl.Size = new System.Drawing.Size(516, 468);
            this.patternMatchingParamControl.TabIndex = 0;
            this.patternMatchingParamControl.Tolerance = 0D;
            this.patternMatchingParamControl.TrainImage = null;
            this.patternMatchingParamControl.UseMaskImage = false;
            this.patternMatchingParamControl.Load += new System.EventHandler(this.patternMatchingParamControl_Load);
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
            // patternMatchingResultControl
            // 
            this.patternMatchingResultControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.patternMatchingResultControl.Location = new System.Drawing.Point(0, 0);
            this.patternMatchingResultControl.Name = "patternMatchingResultControl";
            this.patternMatchingResultControl.Size = new System.Drawing.Size(552, 474);
            this.patternMatchingResultControl.TabIndex = 0;
            // 
            // _btnSaveParam
            // 
            this._btnSaveParam.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._btnSaveParam.Location = new System.Drawing.Point(611, 411);
            this._btnSaveParam.Name = "_btnSaveParam";
            this._btnSaveParam.Size = new System.Drawing.Size(240, 28);
            this._btnSaveParam.TabIndex = 6;
            this._btnSaveParam.Text = "Save Param";
            // 
            // _btnLoadParam
            // 
            this._btnLoadParam.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._btnLoadParam.Location = new System.Drawing.Point(325, 411);
            this._btnLoadParam.Name = "_btnLoadParam";
            this._btnLoadParam.Size = new System.Drawing.Size(240, 28);
            this._btnLoadParam.TabIndex = 7;
            this._btnLoadParam.Text = "Load Param";
            // 
            // txtResultX
            // 
            this.txtResultX.Location = new System.Drawing.Point(32, 388);
            this.txtResultX.Name = "txtResultX";
            this.txtResultX.ReadOnly = true;
            this.txtResultX.Size = new System.Drawing.Size(86, 21);
            this.txtResultX.TabIndex = 5;
            // 
            // txtResultY
            // 
            this.txtResultY.Location = new System.Drawing.Point(155, 388);
            this.txtResultY.Name = "txtResultY";
            this.txtResultY.ReadOnly = true;
            this.txtResultY.Size = new System.Drawing.Size(86, 21);
            this.txtResultY.TabIndex = 4;
            // 
            // txtResultT
            // 
            this.txtResultT.Location = new System.Drawing.Point(32, 415);
            this.txtResultT.Name = "txtResultT";
            this.txtResultT.ReadOnly = true;
            this.txtResultT.Size = new System.Drawing.Size(86, 21);
            this.txtResultT.TabIndex = 3;
            // 
            // lblRX
            // 
            this.lblRX.AutoSize = true;
            this.lblRX.Location = new System.Drawing.Point(6, 392);
            this.lblRX.Name = "lblRX";
            this.lblRX.Size = new System.Drawing.Size(17, 12);
            this.lblRX.TabIndex = 2;
            this.lblRX.Text = "X:";
            // 
            // lblRY
            // 
            this.lblRY.AutoSize = true;
            this.lblRY.Location = new System.Drawing.Point(129, 392);
            this.lblRY.Name = "lblRY";
            this.lblRY.Size = new System.Drawing.Size(17, 12);
            this.lblRY.TabIndex = 1;
            this.lblRY.Text = "Y:";
            // 
            // lblRT
            // 
            this.lblRT.AutoSize = true;
            this.lblRT.Location = new System.Drawing.Point(6, 419);
            this.lblRT.Name = "lblRT";
            this.lblRT.Size = new System.Drawing.Size(17, 12);
            this.lblRT.TabIndex = 0;
            this.lblRT.Text = "T:";
            // 
            // cameraListBoxItemsView
            // 
            this.cameraListBoxItemsView.BorderColor = System.Drawing.Color.White;
            this.cameraListBoxItemsView.BorderWidth = 2;
            this.cameraListBoxItemsView.GroupBackColor = System.Drawing.Color.White;
            this.cameraListBoxItemsView.GroupForeColor = System.Drawing.Color.Black;
            this.cameraListBoxItemsView.GroupName = "Camera";
            this.cameraListBoxItemsView.ItemBackColor = System.Drawing.Color.Black;
            this.cameraListBoxItemsView.ItemForeColor = System.Drawing.Color.Lime;
            this.cameraListBoxItemsView.ListBackColor = System.Drawing.Color.Black;
            this.cameraListBoxItemsView.ListForeColor = System.Drawing.Color.Lime;
            this.cameraListBoxItemsView.Location = new System.Drawing.Point(6, 33);
            this.cameraListBoxItemsView.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.cameraListBoxItemsView.Name = "cameraListBoxItemsView";
            this.cameraListBoxItemsView.SelectedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(255)))), ((int)(((byte)(0)))));
            this.cameraListBoxItemsView.SelectedForeColor = System.Drawing.Color.Black;
            this.cameraListBoxItemsView.SelectedIndex = -1;
            this.cameraListBoxItemsView.Size = new System.Drawing.Size(212, 210);
            this.cameraListBoxItemsView.TabIndex = 3;
            // 
            // listViewResults
            // 
            this.listViewResults.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colIdx,
            this.colX,
            this.colY,
            this.colT,
            this.colScore});
            this.listViewResults.FullRowSelect = true;
            this.listViewResults.GridLines = true;
            this.listViewResults.HideSelection = false;
            this.listViewResults.Location = new System.Drawing.Point(6, 444);
            this.listViewResults.MultiSelect = false;
            this.listViewResults.Name = "listViewResults";
            this.listViewResults.Size = new System.Drawing.Size(313, 180);
            this.listViewResults.TabIndex = 8;
            this.listViewResults.UseCompatibleStateImageBehavior = false;
            this.listViewResults.View = System.Windows.Forms.View.Details;
            this.listViewResults.SelectedIndexChanged += new System.EventHandler(this.listViewResults_SelectedIndexChanged);
            // 
            // colIdx
            // 
            this.colIdx.Text = "#";
            this.colIdx.Width = 30;
            // 
            // colX
            // 
            this.colX.Text = "X";
            this.colX.Width = 70;
            // 
            // colY
            // 
            this.colY.Text = "Y";
            this.colY.Width = 70;
            // 
            // colT
            // 
            this.colT.Text = "T";
            this.colT.Width = 70;
            // 
            // colScore
            // 
            this.colScore.Text = "Score";
            this.colScore.Width = 70;
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
            this._viewer.Location = new System.Drawing.Point(325, 33);
            this._viewer.Name = "_viewer";
            this._viewer.OperatingType = QMC.Common.Vision.VisionImageViewer.OperatingTypes.Center;
            this._viewer.Simulated = false;
            this._viewer.Size = new System.Drawing.Size(526, 372);
            this._viewer.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this._viewer.TabIndex = 0;
            this._viewer.TabStop = false;
            this._viewer.UpdateDelayTime = 80;
            this._viewer.VisibleCrossLine = true;
            // 
            // groupSearchMode
            // 
            this.groupSearchMode.Controls.Add(this.radioSingle);
            this.groupSearchMode.Controls.Add(this.radioMulti);
            this.groupSearchMode.Location = new System.Drawing.Point(6, 630);
            this.groupSearchMode.Name = "groupSearchMode";
            this.groupSearchMode.Size = new System.Drawing.Size(313, 50);
            this.groupSearchMode.TabIndex = 9;
            this.groupSearchMode.TabStop = false;
            this.groupSearchMode.Text = "Search Mode";
            // 
            // radioSingle
            // 
            this.radioSingle.AutoSize = true;
            this.radioSingle.Location = new System.Drawing.Point(14, 22);
            this.radioSingle.Name = "radioSingle";
            this.radioSingle.Size = new System.Drawing.Size(71, 16);
            this.radioSingle.TabIndex = 0;
            this.radioSingle.TabStop = true;
            this.radioSingle.Text = "FirstOnly";
            this.radioSingle.UseVisualStyleBackColor = true;
            // 
            // radioMulti
            // 
            this.radioMulti.AutoSize = true;
            this.radioMulti.Location = new System.Drawing.Point(120, 22);
            this.radioMulti.Name = "radioMulti";
            this.radioMulti.Size = new System.Drawing.Size(69, 16);
            this.radioMulti.TabIndex = 1;
            this.radioMulti.TabStop = true;
            this.radioMulti.Text = "All (Avg)";
            this.radioMulti.UseVisualStyleBackColor = true;
            // 
            // lblAvgX
            // 
            this.lblAvgX.AutoSize = true;
            this.lblAvgX.Location = new System.Drawing.Point(6, 690);
            this.lblAvgX.Name = "lblAvgX";
            this.lblAvgX.Size = new System.Drawing.Size(42, 12);
            this.lblAvgX.TabIndex = 10;
            this.lblAvgX.Text = "Avg X:";
            // 
            // lblAvgY
            // 
            this.lblAvgY.AutoSize = true;
            this.lblAvgY.Location = new System.Drawing.Point(6, 717);
            this.lblAvgY.Name = "lblAvgY";
            this.lblAvgY.Size = new System.Drawing.Size(42, 12);
            this.lblAvgY.TabIndex = 11;
            this.lblAvgY.Text = "Avg Y:";
            // 
            // lblAvgT
            // 
            this.lblAvgT.AutoSize = true;
            this.lblAvgT.Location = new System.Drawing.Point(6, 744);
            this.lblAvgT.Name = "lblAvgT";
            this.lblAvgT.Size = new System.Drawing.Size(42, 12);
            this.lblAvgT.TabIndex = 12;
            this.lblAvgT.Text = "Avg T:";
            // 
            // txtAvgX
            // 
            this.txtAvgX.Location = new System.Drawing.Point(60, 686);
            this.txtAvgX.Name = "txtAvgX";
            this.txtAvgX.ReadOnly = true;
            this.txtAvgX.Size = new System.Drawing.Size(86, 21);
            this.txtAvgX.TabIndex = 13;
            // 
            // txtAvgY
            // 
            this.txtAvgY.Location = new System.Drawing.Point(60, 713);
            this.txtAvgY.Name = "txtAvgY";
            this.txtAvgY.ReadOnly = true;
            this.txtAvgY.Size = new System.Drawing.Size(86, 21);
            this.txtAvgY.TabIndex = 14;
            // 
            // txtAvgT
            // 
            this.txtAvgT.Location = new System.Drawing.Point(60, 740);
            this.txtAvgT.Name = "txtAvgT";
            this.txtAvgT.ReadOnly = true;
            this.txtAvgT.Size = new System.Drawing.Size(86, 21);
            this.txtAvgT.TabIndex = 15;
            // 
            // chkShowIndexes
            // 
            this.chkShowIndexes.AutoSize = true;
            this.chkShowIndexes.Location = new System.Drawing.Point(6, 770);
            this.chkShowIndexes.Name = "chkShowIndexes";
            this.chkShowIndexes.Size = new System.Drawing.Size(112, 16);
            this.chkShowIndexes.TabIndex = 16;
            this.chkShowIndexes.Text = "Show Indexes";
            this.chkShowIndexes.UseVisualStyleBackColor = true;
            // 
            // chkHighlightRef
            // 
            this.chkHighlightRef.AutoSize = true;
            this.chkHighlightRef.Location = new System.Drawing.Point(140, 770);
            this.chkHighlightRef.Name = "chkHighlightRef";
            this.chkHighlightRef.Size = new System.Drawing.Size(128, 16);
            this.chkHighlightRef.TabIndex = 17;
            this.chkHighlightRef.Text = "Highlight Center";
            this.chkHighlightRef.UseVisualStyleBackColor = true;
            // 
            // PatternMatchingDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(894, 950);
            this.Controls.Add(this.chkShowIndexes);
            this.Controls.Add(this.chkHighlightRef);
            this.Controls.Add(this.groupSearchMode);
            this.Controls.Add(this.lblAvgX);
            this.Controls.Add(this.lblAvgY);
            this.Controls.Add(this.lblAvgT);
            this.Controls.Add(this.txtAvgX);
            this.Controls.Add(this.txtAvgY);
            this.Controls.Add(this.txtAvgT);
            this.Controls.Add(this.listViewResults);
            this.Controls.Add(this.lblRT);
            this.Controls.Add(this.lblRY);
            this.Controls.Add(this.lblRX);
            this.Controls.Add(this.txtResultT);
            this.Controls.Add(this.txtResultY);
            this.Controls.Add(this.txtResultX);
            this.Controls.Add(this._btnLoadParam);
            this.Controls.Add(this._btnSaveParam);
            this.Controls.Add(this.tabControl_Vision);
            this.Controls.Add(this.cameraListBoxItemsView);
            this.Controls.Add(this._viewer);
            this.Controls.Add(this._lblStatus);
            this.Controls.Add(this._btnClose);
            this.Controls.Add(this._btnSearch);
            this.MinimumSize = new System.Drawing.Size(910, 900);
            this.Name = "PatternMatchingDialog";
            this.Text = "Pattern Matching";
            this.tabControl_Vision.ResumeLayout(false);
            this.tabPageROI.ResumeLayout(false);
            this.tabPageParam.ResumeLayout(false);
            this.tabPageResult.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._viewer)).EndInit();
            this.groupSearchMode.ResumeLayout(false);
            this.groupSearchMode.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}