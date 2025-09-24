using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using QMC.Common.Vision;
using QMC.Common.VisionPart;
using QMC.Common;

namespace QMC.LCP_280.Process
{
    public partial class PatternMatchingControl
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
            this.listViewResults = new System.Windows.Forms.ListView();
            this.colIdx = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colX = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colY = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colT = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colScore = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
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
            this.groupBoxImageView = new System.Windows.Forms.GroupBox();
            this._viewer = new QMC.Common.Vision.VisionImageViewer();
            this.cameraListBoxItemsView = new QMC.Common.ListBoxItemsView();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.tabControl_Vision.SuspendLayout();
            this.tabPageROI.SuspendLayout();
            this.tabPageParam.SuspendLayout();
            this.tabPageResult.SuspendLayout();
            this.groupSearchMode.SuspendLayout();
            this.groupBoxImageView.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._viewer)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // _btnSearch
            // 
            this._btnSearch.Location = new System.Drawing.Point(6, 249);
            this._btnSearch.Name = "_btnSearch";
            this._btnSearch.Size = new System.Drawing.Size(212, 50);
            this._btnSearch.TabIndex = 1;
            this._btnSearch.Text = "Search";
            this._btnSearch.Click += new System.EventHandler(this.BtnSearch_Click);
            // 
            // _btnClose
            // 
            this._btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._btnClose.Location = new System.Drawing.Point(990, 5);
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
            this._lblStatus.Size = new System.Drawing.Size(966, 22);
            this._lblStatus.TabIndex = 0;
            this._lblStatus.Text = "Ready";
            this._lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tabControl_Vision
            // 
            this.tabControl_Vision.Controls.Add(this.tabPageROI);
            this.tabControl_Vision.Controls.Add(this.tabPageParam);
            this.tabControl_Vision.Controls.Add(this.tabPageResult);
            this.tabControl_Vision.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl_Vision.Location = new System.Drawing.Point(3, 21);
            this.tabControl_Vision.Name = "tabControl_Vision";
            this.tabControl_Vision.SelectedIndex = 0;
            this.tabControl_Vision.Size = new System.Drawing.Size(496, 571);
            this.tabControl_Vision.TabIndex = 4;
            // 
            // tabPageROI
            // 
            this.tabPageROI.Controls.Add(this.maintROIControl);
            this.tabPageROI.Location = new System.Drawing.Point(4, 25);
            this.tabPageROI.Name = "tabPageROI";
            this.tabPageROI.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageROI.Size = new System.Drawing.Size(488, 542);
            this.tabPageROI.TabIndex = 0;
            this.tabPageROI.Text = "ROI";
            this.tabPageROI.UseVisualStyleBackColor = true;
            // 
            // maintROIControl
            // 
            this.maintROIControl.Dock = System.Windows.Forms.DockStyle.Top;
            this.maintROIControl.Location = new System.Drawing.Point(3, 3);
            this.maintROIControl.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.maintROIControl.Name = "maintROIControl";
            this.maintROIControl.Size = new System.Drawing.Size(482, 532);
            this.maintROIControl.TabIndex = 0;
            // 
            // tabPageParam
            // 
            this.tabPageParam.Controls.Add(this.patternMatchingParamControl);
            this.tabPageParam.Location = new System.Drawing.Point(4, 25);
            this.tabPageParam.Name = "tabPageParam";
            this.tabPageParam.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageParam.Size = new System.Drawing.Size(478, 531);
            this.tabPageParam.TabIndex = 1;
            this.tabPageParam.Text = "Parameter";
            this.tabPageParam.UseVisualStyleBackColor = true;
            // 
            // patternMatchingParamControl
            // 
            this.patternMatchingParamControl.BackColor = System.Drawing.Color.Transparent;
            this.patternMatchingParamControl.Dock = System.Windows.Forms.DockStyle.Top;
            this.patternMatchingParamControl.DuplicateChecked = false;
            this.patternMatchingParamControl.LearnImage = null;
            this.patternMatchingParamControl.Location = new System.Drawing.Point(3, 3);
            this.patternMatchingParamControl.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.patternMatchingParamControl.MaxInstnce = 0;
            this.patternMatchingParamControl.MinScore = 0D;
            this.patternMatchingParamControl.Name = "patternMatchingParamControl";
            this.patternMatchingParamControl.SelectedIndex = 0;
            this.patternMatchingParamControl.Size = new System.Drawing.Size(472, 532);
            this.patternMatchingParamControl.TabIndex = 0;
            this.patternMatchingParamControl.Tolerance = 0D;
            this.patternMatchingParamControl.TrainImage = null;
            this.patternMatchingParamControl.UseMaskImage = false;
            this.patternMatchingParamControl.Load += new System.EventHandler(this.patternMatchingParamControl_Load);
            // 
            // tabPageResult
            // 
            this.tabPageResult.Controls.Add(this.patternMatchingResultControl);
            this.tabPageResult.Location = new System.Drawing.Point(4, 25);
            this.tabPageResult.Name = "tabPageResult";
            this.tabPageResult.Size = new System.Drawing.Size(478, 531);
            this.tabPageResult.TabIndex = 2;
            this.tabPageResult.Text = "Result";
            this.tabPageResult.UseVisualStyleBackColor = true;
            // 
            // patternMatchingResultControl
            // 
            this.patternMatchingResultControl.Dock = System.Windows.Forms.DockStyle.Top;
            this.patternMatchingResultControl.Location = new System.Drawing.Point(0, 0);
            this.patternMatchingResultControl.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.patternMatchingResultControl.Name = "patternMatchingResultControl";
            this.patternMatchingResultControl.Size = new System.Drawing.Size(478, 535);
            this.patternMatchingResultControl.TabIndex = 0;
            // 
            // _btnSaveParam
            // 
            this._btnSaveParam.Dock = System.Windows.Forms.DockStyle.Right;
            this._btnSaveParam.Location = new System.Drawing.Point(299, 21);
            this._btnSaveParam.Name = "_btnSaveParam";
            this._btnSaveParam.Size = new System.Drawing.Size(200, 52);
            this._btnSaveParam.TabIndex = 6;
            this._btnSaveParam.Text = "Save Param";
            this._btnSaveParam.Click += new System.EventHandler(this._btnSaveParam_Click);
            // 
            // _btnLoadParam
            // 
            this._btnLoadParam.Dock = System.Windows.Forms.DockStyle.Left;
            this._btnLoadParam.Location = new System.Drawing.Point(3, 21);
            this._btnLoadParam.Name = "_btnLoadParam";
            this._btnLoadParam.Size = new System.Drawing.Size(200, 52);
            this._btnLoadParam.TabIndex = 7;
            this._btnLoadParam.Text = "Load Param";
            this._btnLoadParam.Click += new System.EventHandler(this._btnLoadParam_Click);
            // 
            // txtResultX
            // 
            this.txtResultX.Location = new System.Drawing.Point(32, 31);
            this.txtResultX.Name = "txtResultX";
            this.txtResultX.ReadOnly = true;
            this.txtResultX.Size = new System.Drawing.Size(70, 25);
            this.txtResultX.TabIndex = 5;
            // 
            // txtResultY
            // 
            this.txtResultY.Location = new System.Drawing.Point(148, 31);
            this.txtResultY.Name = "txtResultY";
            this.txtResultY.ReadOnly = true;
            this.txtResultY.Size = new System.Drawing.Size(70, 25);
            this.txtResultY.TabIndex = 4;
            // 
            // txtResultT
            // 
            this.txtResultT.Location = new System.Drawing.Point(269, 31);
            this.txtResultT.Name = "txtResultT";
            this.txtResultT.ReadOnly = true;
            this.txtResultT.Size = new System.Drawing.Size(70, 25);
            this.txtResultT.TabIndex = 3;
            // 
            // lblRX
            // 
            this.lblRX.AutoSize = true;
            this.lblRX.Location = new System.Drawing.Point(6, 31);
            this.lblRX.Name = "lblRX";
            this.lblRX.Size = new System.Drawing.Size(21, 15);
            this.lblRX.TabIndex = 2;
            this.lblRX.Text = "X:";
            // 
            // lblRY
            // 
            this.lblRY.AutoSize = true;
            this.lblRY.Location = new System.Drawing.Point(122, 31);
            this.lblRY.Name = "lblRY";
            this.lblRY.Size = new System.Drawing.Size(20, 15);
            this.lblRY.TabIndex = 1;
            this.lblRY.Text = "Y:";
            // 
            // lblRT
            // 
            this.lblRT.AutoSize = true;
            this.lblRT.Location = new System.Drawing.Point(243, 31);
            this.lblRT.Name = "lblRT";
            this.lblRT.Size = new System.Drawing.Size(20, 15);
            this.lblRT.TabIndex = 0;
            this.lblRT.Text = "T:";
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
            this.listViewResults.Location = new System.Drawing.Point(6, 72);
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
            // groupSearchMode
            // 
            this.groupSearchMode.Controls.Add(this.radioSingle);
            this.groupSearchMode.Controls.Add(this.radioMulti);
            this.groupSearchMode.Location = new System.Drawing.Point(9, 331);
            this.groupSearchMode.Name = "groupSearchMode";
            this.groupSearchMode.Size = new System.Drawing.Size(212, 50);
            this.groupSearchMode.TabIndex = 9;
            this.groupSearchMode.TabStop = false;
            this.groupSearchMode.Text = "Search Mode";
            // 
            // radioSingle
            // 
            this.radioSingle.AutoSize = true;
            this.radioSingle.Location = new System.Drawing.Point(14, 22);
            this.radioSingle.Name = "radioSingle";
            this.radioSingle.Size = new System.Drawing.Size(86, 19);
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
            this.radioMulti.Size = new System.Drawing.Size(85, 19);
            this.radioMulti.TabIndex = 1;
            this.radioMulti.TabStop = true;
            this.radioMulti.Text = "All (Avg)";
            this.radioMulti.UseVisualStyleBackColor = true;
            // 
            // lblAvgX
            // 
            this.lblAvgX.AutoSize = true;
            this.lblAvgX.Location = new System.Drawing.Point(325, 72);
            this.lblAvgX.Name = "lblAvgX";
            this.lblAvgX.Size = new System.Drawing.Size(51, 15);
            this.lblAvgX.TabIndex = 10;
            this.lblAvgX.Text = "Avg X:";
            // 
            // lblAvgY
            // 
            this.lblAvgY.AutoSize = true;
            this.lblAvgY.Location = new System.Drawing.Point(325, 99);
            this.lblAvgY.Name = "lblAvgY";
            this.lblAvgY.Size = new System.Drawing.Size(50, 15);
            this.lblAvgY.TabIndex = 11;
            this.lblAvgY.Text = "Avg Y:";
            // 
            // lblAvgT
            // 
            this.lblAvgT.AutoSize = true;
            this.lblAvgT.Location = new System.Drawing.Point(325, 126);
            this.lblAvgT.Name = "lblAvgT";
            this.lblAvgT.Size = new System.Drawing.Size(50, 15);
            this.lblAvgT.TabIndex = 12;
            this.lblAvgT.Text = "Avg T:";
            // 
            // txtAvgX
            // 
            this.txtAvgX.Location = new System.Drawing.Point(379, 68);
            this.txtAvgX.Name = "txtAvgX";
            this.txtAvgX.ReadOnly = true;
            this.txtAvgX.Size = new System.Drawing.Size(86, 25);
            this.txtAvgX.TabIndex = 13;
            // 
            // txtAvgY
            // 
            this.txtAvgY.Location = new System.Drawing.Point(379, 95);
            this.txtAvgY.Name = "txtAvgY";
            this.txtAvgY.ReadOnly = true;
            this.txtAvgY.Size = new System.Drawing.Size(86, 25);
            this.txtAvgY.TabIndex = 14;
            // 
            // txtAvgT
            // 
            this.txtAvgT.Location = new System.Drawing.Point(379, 122);
            this.txtAvgT.Name = "txtAvgT";
            this.txtAvgT.ReadOnly = true;
            this.txtAvgT.Size = new System.Drawing.Size(86, 25);
            this.txtAvgT.TabIndex = 15;
            // 
            // chkShowIndexes
            // 
            this.chkShowIndexes.AutoSize = true;
            this.chkShowIndexes.Location = new System.Drawing.Point(325, 152);
            this.chkShowIndexes.Name = "chkShowIndexes";
            this.chkShowIndexes.Size = new System.Drawing.Size(123, 19);
            this.chkShowIndexes.TabIndex = 16;
            this.chkShowIndexes.Text = "Show Indexes";
            this.chkShowIndexes.UseVisualStyleBackColor = true;
            // 
            // chkHighlightRef
            // 
            this.chkHighlightRef.AutoSize = true;
            this.chkHighlightRef.Location = new System.Drawing.Point(459, 152);
            this.chkHighlightRef.Name = "chkHighlightRef";
            this.chkHighlightRef.Size = new System.Drawing.Size(131, 19);
            this.chkHighlightRef.TabIndex = 17;
            this.chkHighlightRef.Text = "Highlight Center";
            this.chkHighlightRef.UseVisualStyleBackColor = true;
            // 
            // groupBoxImageView
            // 
            this.groupBoxImageView.Controls.Add(this._viewer);
            this.groupBoxImageView.Location = new System.Drawing.Point(245, 33);
            this.groupBoxImageView.Name = "groupBoxImageView";
            this.groupBoxImageView.Size = new System.Drawing.Size(415, 351);
            this.groupBoxImageView.TabIndex = 18;
            this.groupBoxImageView.TabStop = false;
            this.groupBoxImageView.Text = "ImageView";
            // 
            // _viewer
            // 
            this._viewer.BackColor = System.Drawing.Color.Black;
            this._viewer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._viewer.Camera = null;
            this._viewer.CameraSwitch = null;
            this._viewer.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._viewer.FrameRate = 1D;
            this._viewer.InputImage = null;
            this._viewer.IsViewCustomizedImage = false;
            this._viewer.Location = new System.Drawing.Point(3, 26);
            this._viewer.Name = "_viewer";
            this._viewer.OperatingType = QMC.Common.Vision.VisionImageViewer.OperatingTypes.Center;
            this._viewer.Simulated = false;
            this._viewer.Size = new System.Drawing.Size(409, 322);
            this._viewer.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this._viewer.TabIndex = 0;
            this._viewer.TabStop = false;
            this._viewer.UpdateDelayTime = 80;
            this._viewer.VisibleCrossLine = true;
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
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this._btnLoadParam);
            this.groupBox1.Controls.Add(this._btnSaveParam);
            this.groupBox1.Location = new System.Drawing.Point(666, 643);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(502, 76);
            this.groupBox1.TabIndex = 19;
            this.groupBox1.TabStop = false;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.tabControl_Vision);
            this.groupBox2.Location = new System.Drawing.Point(666, 33);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(502, 595);
            this.groupBox2.TabIndex = 20;
            this.groupBox2.TabStop = false;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.listViewResults);
            this.groupBox3.Controls.Add(this.txtAvgT);
            this.groupBox3.Controls.Add(this.txtAvgY);
            this.groupBox3.Controls.Add(this.txtAvgX);
            this.groupBox3.Controls.Add(this.chkShowIndexes);
            this.groupBox3.Controls.Add(this.lblRT);
            this.groupBox3.Controls.Add(this.lblAvgT);
            this.groupBox3.Controls.Add(this.lblRY);
            this.groupBox3.Controls.Add(this.chkHighlightRef);
            this.groupBox3.Controls.Add(this.lblRX);
            this.groupBox3.Controls.Add(this.txtResultT);
            this.groupBox3.Controls.Add(this.lblAvgY);
            this.groupBox3.Controls.Add(this.txtResultY);
            this.groupBox3.Controls.Add(this.lblAvgX);
            this.groupBox3.Controls.Add(this.txtResultX);
            this.groupBox3.Location = new System.Drawing.Point(9, 390);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(648, 329);
            this.groupBox3.TabIndex = 21;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "groupBox3";
            // 
            // PatternMatchingControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBoxImageView);
            this.Controls.Add(this.groupSearchMode);
            this.Controls.Add(this.cameraListBoxItemsView);
            this.Controls.Add(this._lblStatus);
            this.Controls.Add(this._btnClose);
            this.Controls.Add(this._btnSearch);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "PatternMatchingControl";
            this.Size = new System.Drawing.Size(1250, 800);
            this.tabControl_Vision.ResumeLayout(false);
            this.tabPageROI.ResumeLayout(false);
            this.tabPageParam.ResumeLayout(false);
            this.tabPageResult.ResumeLayout(false);
            this.groupSearchMode.ResumeLayout(false);
            this.groupSearchMode.PerformLayout();
            this.groupBoxImageView.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._viewer)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        private GroupBox groupBoxImageView;
        private GroupBox groupBox1;
        private GroupBox groupBox2;
        private GroupBox groupBox3;
    }
}