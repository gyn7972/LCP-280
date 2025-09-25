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
            this._lblStatus = new System.Windows.Forms.Label();
            this.tabControl_Vision = new System.Windows.Forms.TabControl();
            this.tabPageROI = new System.Windows.Forms.TabPage();
            this.maintROIControl = new QMC.Common.MaintROIControl();
            this.tabPageParam = new System.Windows.Forms.TabPage();
            this.patternMatchingParamControl = new QMC.Common.MultiPatternMatchingParameterControl();
            this.tabPageResult = new System.Windows.Forms.TabPage();
            this.patternMatchingResultControl = new QMC.Common.PatternMatchingResultControl();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBoxImageView = new System.Windows.Forms.GroupBox();
            this._viewer = new QMC.Common.Vision.VisionImageViewer();
            this.tableLayoutPanel6 = new System.Windows.Forms.TableLayoutPanel();
            this.groupSearchMode = new System.Windows.Forms.GroupBox();
            this.radioSingle = new System.Windows.Forms.RadioButton();
            this.radioMulti = new System.Windows.Forms.RadioButton();
            this.cameraListBoxItemsView = new QMC.Common.ListBoxItemsView();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel8 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel9 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel10 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel13 = new System.Windows.Forms.TableLayoutPanel();
            this.lblRT = new System.Windows.Forms.Label();
            this.txtResultT = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel12 = new System.Windows.Forms.TableLayoutPanel();
            this.lblRY = new System.Windows.Forms.Label();
            this.txtResultY = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel11 = new System.Windows.Forms.TableLayoutPanel();
            this.lblRX = new System.Windows.Forms.Label();
            this.txtResultX = new System.Windows.Forms.TextBox();
            this.listViewResults = new System.Windows.Forms.ListView();
            this.colIdx = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colX = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colY = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colT = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colScore = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tableLayoutPanel14 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel18 = new System.Windows.Forms.TableLayoutPanel();
            this.chkShowIndexes = new System.Windows.Forms.CheckBox();
            this.chkHighlightRef = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel17 = new System.Windows.Forms.TableLayoutPanel();
            this.txtAvgT = new System.Windows.Forms.TextBox();
            this.lblAvgY = new System.Windows.Forms.Label();
            this.tableLayoutPanel16 = new System.Windows.Forms.TableLayoutPanel();
            this.lblAvgT = new System.Windows.Forms.Label();
            this.txtAvgY = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel15 = new System.Windows.Forms.TableLayoutPanel();
            this.lblAvgX = new System.Windows.Forms.Label();
            this.txtAvgX = new System.Windows.Forms.TextBox();
            this._btnSaveParam = new System.Windows.Forms.Button();
            this._btnLoadParam = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel7 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBoxdieInputControl = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this._btnClose = new System.Windows.Forms.Button();
            this.tabControl_Vision.SuspendLayout();
            this.tabPageROI.SuspendLayout();
            this.tabPageParam.SuspendLayout();
            this.tabPageResult.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            this.groupBoxImageView.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._viewer)).BeginInit();
            this.tableLayoutPanel6.SuspendLayout();
            this.groupSearchMode.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.tableLayoutPanel8.SuspendLayout();
            this.tableLayoutPanel9.SuspendLayout();
            this.tableLayoutPanel10.SuspendLayout();
            this.tableLayoutPanel13.SuspendLayout();
            this.tableLayoutPanel12.SuspendLayout();
            this.tableLayoutPanel11.SuspendLayout();
            this.tableLayoutPanel14.SuspendLayout();
            this.tableLayoutPanel18.SuspendLayout();
            this.tableLayoutPanel17.SuspendLayout();
            this.tableLayoutPanel16.SuspendLayout();
            this.tableLayoutPanel15.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel7.SuspendLayout();
            this.groupBoxdieInputControl.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // _btnSearch
            // 
            this._btnSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            this._btnSearch.Location = new System.Drawing.Point(2, 211);
            this._btnSearch.Margin = new System.Windows.Forms.Padding(2);
            this._btnSearch.Name = "_btnSearch";
            this._btnSearch.Size = new System.Drawing.Size(233, 30);
            this._btnSearch.TabIndex = 1;
            this._btnSearch.Text = "Search";
            this._btnSearch.Click += new System.EventHandler(this.BtnSearch_Click);
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
            this.tabControl_Vision.Location = new System.Drawing.Point(2, 16);
            this.tabControl_Vision.Margin = new System.Windows.Forms.Padding(2);
            this.tabControl_Vision.Name = "tabControl_Vision";
            this.tabControl_Vision.SelectedIndex = 0;
            this.tabControl_Vision.Size = new System.Drawing.Size(511, 641);
            this.tabControl_Vision.TabIndex = 4;
            // 
            // tabPageROI
            // 
            this.tabPageROI.Controls.Add(this.maintROIControl);
            this.tabPageROI.Location = new System.Drawing.Point(4, 22);
            this.tabPageROI.Margin = new System.Windows.Forms.Padding(2);
            this.tabPageROI.Name = "tabPageROI";
            this.tabPageROI.Padding = new System.Windows.Forms.Padding(2);
            this.tabPageROI.Size = new System.Drawing.Size(503, 615);
            this.tabPageROI.TabIndex = 0;
            this.tabPageROI.Text = "ROI";
            this.tabPageROI.UseVisualStyleBackColor = true;
            // 
            // maintROIControl
            // 
            this.maintROIControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.maintROIControl.Location = new System.Drawing.Point(2, 2);
            this.maintROIControl.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.maintROIControl.Name = "maintROIControl";
            this.maintROIControl.Size = new System.Drawing.Size(499, 611);
            this.maintROIControl.TabIndex = 0;
            // 
            // tabPageParam
            // 
            this.tabPageParam.Controls.Add(this.patternMatchingParamControl);
            this.tabPageParam.Location = new System.Drawing.Point(4, 22);
            this.tabPageParam.Margin = new System.Windows.Forms.Padding(2);
            this.tabPageParam.Name = "tabPageParam";
            this.tabPageParam.Padding = new System.Windows.Forms.Padding(2);
            this.tabPageParam.Size = new System.Drawing.Size(503, 615);
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
            this.patternMatchingParamControl.Location = new System.Drawing.Point(2, 2);
            this.patternMatchingParamControl.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.patternMatchingParamControl.MaxInstnce = 0;
            this.patternMatchingParamControl.MinScore = 0D;
            this.patternMatchingParamControl.Name = "patternMatchingParamControl";
            this.patternMatchingParamControl.SelectedIndex = 0;
            this.patternMatchingParamControl.Size = new System.Drawing.Size(499, 426);
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
            this.tabPageResult.Margin = new System.Windows.Forms.Padding(2);
            this.tabPageResult.Name = "tabPageResult";
            this.tabPageResult.Size = new System.Drawing.Size(503, 615);
            this.tabPageResult.TabIndex = 2;
            this.tabPageResult.Text = "Result";
            this.tabPageResult.UseVisualStyleBackColor = true;
            // 
            // patternMatchingResultControl
            // 
            this.patternMatchingResultControl.Dock = System.Windows.Forms.DockStyle.Top;
            this.patternMatchingResultControl.Location = new System.Drawing.Point(0, 0);
            this.patternMatchingResultControl.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.patternMatchingResultControl.Name = "patternMatchingResultControl";
            this.patternMatchingResultControl.Size = new System.Drawing.Size(503, 428);
            this.patternMatchingResultControl.TabIndex = 0;
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 1;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.Controls.Add(this.tableLayoutPanel5, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.groupBox3, 0, 1);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(2, 16);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 2;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(615, 722);
            this.tableLayoutPanel4.TabIndex = 0;
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 2;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.tableLayoutPanel5.Controls.Add(this.groupBoxImageView, 1, 0);
            this.tableLayoutPanel5.Controls.Add(this.tableLayoutPanel6, 0, 0);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 1;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(609, 355);
            this.tableLayoutPanel5.TabIndex = 10;
            // 
            // groupBoxImageView
            // 
            this.groupBoxImageView.Controls.Add(this._viewer);
            this.groupBoxImageView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxImageView.Location = new System.Drawing.Point(245, 2);
            this.groupBoxImageView.Margin = new System.Windows.Forms.Padding(2);
            this.groupBoxImageView.Name = "groupBoxImageView";
            this.groupBoxImageView.Padding = new System.Windows.Forms.Padding(2);
            this.groupBoxImageView.Size = new System.Drawing.Size(362, 351);
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
            this._viewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._viewer.FrameRate = 1D;
            this._viewer.InputImage = null;
            this._viewer.IsViewCustomizedImage = false;
            this._viewer.Location = new System.Drawing.Point(2, 16);
            this._viewer.Margin = new System.Windows.Forms.Padding(2);
            this._viewer.Name = "_viewer";
            this._viewer.OperatingType = QMC.Common.Vision.VisionImageViewer.OperatingTypes.Center;
            this._viewer.Simulated = false;
            this._viewer.Size = new System.Drawing.Size(358, 333);
            this._viewer.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this._viewer.TabIndex = 0;
            this._viewer.TabStop = false;
            this._viewer.UpdateDelayTime = 80;
            this._viewer.VisibleCrossLine = true;
            // 
            // tableLayoutPanel6
            // 
            this.tableLayoutPanel6.ColumnCount = 1;
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel6.Controls.Add(this._btnSearch, 0, 1);
            this.tableLayoutPanel6.Controls.Add(this.groupSearchMode, 0, 2);
            this.tableLayoutPanel6.Controls.Add(this.cameraListBoxItemsView, 0, 0);
            this.tableLayoutPanel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel6.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel6.Name = "tableLayoutPanel6";
            this.tableLayoutPanel6.RowCount = 3;
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel6.Size = new System.Drawing.Size(237, 349);
            this.tableLayoutPanel6.TabIndex = 0;
            // 
            // groupSearchMode
            // 
            this.groupSearchMode.Controls.Add(this.radioSingle);
            this.groupSearchMode.Controls.Add(this.radioMulti);
            this.groupSearchMode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupSearchMode.Location = new System.Drawing.Point(2, 245);
            this.groupSearchMode.Margin = new System.Windows.Forms.Padding(2);
            this.groupSearchMode.Name = "groupSearchMode";
            this.groupSearchMode.Padding = new System.Windows.Forms.Padding(2);
            this.groupSearchMode.Size = new System.Drawing.Size(233, 102);
            this.groupSearchMode.TabIndex = 9;
            this.groupSearchMode.TabStop = false;
            this.groupSearchMode.Text = "Search Mode";
            // 
            // radioSingle
            // 
            this.radioSingle.AutoSize = true;
            this.radioSingle.Location = new System.Drawing.Point(11, 18);
            this.radioSingle.Margin = new System.Windows.Forms.Padding(2);
            this.radioSingle.Name = "radioSingle";
            this.radioSingle.Size = new System.Drawing.Size(73, 16);
            this.radioSingle.TabIndex = 0;
            this.radioSingle.TabStop = true;
            this.radioSingle.Text = "FirstOnly";
            this.radioSingle.UseVisualStyleBackColor = true;
            // 
            // radioMulti
            // 
            this.radioMulti.AutoSize = true;
            this.radioMulti.Location = new System.Drawing.Point(96, 18);
            this.radioMulti.Margin = new System.Windows.Forms.Padding(2);
            this.radioMulti.Name = "radioMulti";
            this.radioMulti.Size = new System.Drawing.Size(72, 16);
            this.radioMulti.TabIndex = 1;
            this.radioMulti.TabStop = true;
            this.radioMulti.Text = "All (Avg)";
            this.radioMulti.UseVisualStyleBackColor = true;
            // 
            // cameraListBoxItemsView
            // 
            this.cameraListBoxItemsView.BorderColor = System.Drawing.Color.White;
            this.cameraListBoxItemsView.BorderWidth = 2;
            this.cameraListBoxItemsView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cameraListBoxItemsView.GroupBackColor = System.Drawing.Color.White;
            this.cameraListBoxItemsView.GroupForeColor = System.Drawing.Color.Black;
            this.cameraListBoxItemsView.GroupName = "Camera";
            this.cameraListBoxItemsView.ItemBackColor = System.Drawing.Color.Black;
            this.cameraListBoxItemsView.ItemForeColor = System.Drawing.Color.Lime;
            this.cameraListBoxItemsView.ListBackColor = System.Drawing.Color.Black;
            this.cameraListBoxItemsView.ListForeColor = System.Drawing.Color.Lime;
            this.cameraListBoxItemsView.Location = new System.Drawing.Point(2, 5);
            this.cameraListBoxItemsView.Margin = new System.Windows.Forms.Padding(2, 5, 2, 5);
            this.cameraListBoxItemsView.Name = "cameraListBoxItemsView";
            this.cameraListBoxItemsView.SelectedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(255)))), ((int)(((byte)(0)))));
            this.cameraListBoxItemsView.SelectedForeColor = System.Drawing.Color.Black;
            this.cameraListBoxItemsView.SelectedIndex = -1;
            this.cameraListBoxItemsView.Size = new System.Drawing.Size(233, 199);
            this.cameraListBoxItemsView.TabIndex = 3;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.tableLayoutPanel8);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox3.Location = new System.Drawing.Point(2, 363);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox3.Size = new System.Drawing.Size(611, 357);
            this.groupBox3.TabIndex = 21;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "groupBox3";
            // 
            // tableLayoutPanel8
            // 
            this.tableLayoutPanel8.ColumnCount = 2;
            this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 57.89474F));
            this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 42.10526F));
            this.tableLayoutPanel8.Controls.Add(this.tableLayoutPanel9, 0, 0);
            this.tableLayoutPanel8.Controls.Add(this.tableLayoutPanel14, 1, 0);
            this.tableLayoutPanel8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel8.Location = new System.Drawing.Point(2, 16);
            this.tableLayoutPanel8.Name = "tableLayoutPanel8";
            this.tableLayoutPanel8.RowCount = 1;
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel8.Size = new System.Drawing.Size(607, 339);
            this.tableLayoutPanel8.TabIndex = 11;
            this.tableLayoutPanel8.Paint += new System.Windows.Forms.PaintEventHandler(this.tableLayoutPanel8_Paint);
            // 
            // tableLayoutPanel9
            // 
            this.tableLayoutPanel9.ColumnCount = 1;
            this.tableLayoutPanel9.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel9.Controls.Add(this.tableLayoutPanel10, 0, 0);
            this.tableLayoutPanel9.Controls.Add(this.listViewResults, 0, 1);
            this.tableLayoutPanel9.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel9.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel9.Name = "tableLayoutPanel9";
            this.tableLayoutPanel9.RowCount = 5;
            this.tableLayoutPanel9.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.tableLayoutPanel9.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 85.71429F));
            this.tableLayoutPanel9.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel9.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel9.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel9.Size = new System.Drawing.Size(345, 333);
            this.tableLayoutPanel9.TabIndex = 22;
            // 
            // tableLayoutPanel10
            // 
            this.tableLayoutPanel10.ColumnCount = 3;
            this.tableLayoutPanel10.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel10.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel10.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel10.Controls.Add(this.tableLayoutPanel13, 2, 0);
            this.tableLayoutPanel10.Controls.Add(this.tableLayoutPanel12, 1, 0);
            this.tableLayoutPanel10.Controls.Add(this.tableLayoutPanel11, 0, 0);
            this.tableLayoutPanel10.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel10.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel10.Name = "tableLayoutPanel10";
            this.tableLayoutPanel10.RowCount = 1;
            this.tableLayoutPanel10.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel10.Size = new System.Drawing.Size(339, 32);
            this.tableLayoutPanel10.TabIndex = 0;
            // 
            // tableLayoutPanel13
            // 
            this.tableLayoutPanel13.ColumnCount = 2;
            this.tableLayoutPanel13.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel13.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel13.Controls.Add(this.lblRT, 0, 0);
            this.tableLayoutPanel13.Controls.Add(this.txtResultT, 1, 0);
            this.tableLayoutPanel13.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel13.Location = new System.Drawing.Point(229, 3);
            this.tableLayoutPanel13.Name = "tableLayoutPanel13";
            this.tableLayoutPanel13.RowCount = 1;
            this.tableLayoutPanel13.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel13.Size = new System.Drawing.Size(107, 26);
            this.tableLayoutPanel13.TabIndex = 2;
            // 
            // lblRT
            // 
            this.lblRT.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblRT.AutoSize = true;
            this.lblRT.Location = new System.Drawing.Point(33, 3);
            this.lblRT.Margin = new System.Windows.Forms.Padding(3);
            this.lblRT.Name = "lblRT";
            this.lblRT.Size = new System.Drawing.Size(17, 20);
            this.lblRT.TabIndex = 0;
            this.lblRT.Text = "T:";
            // 
            // txtResultT
            // 
            this.txtResultT.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtResultT.Location = new System.Drawing.Point(55, 2);
            this.txtResultT.Margin = new System.Windows.Forms.Padding(2);
            this.txtResultT.Name = "txtResultT";
            this.txtResultT.ReadOnly = true;
            this.txtResultT.Size = new System.Drawing.Size(50, 21);
            this.txtResultT.TabIndex = 3;
            this.txtResultT.TextChanged += new System.EventHandler(this.txtResultT_TextChanged);
            // 
            // tableLayoutPanel12
            // 
            this.tableLayoutPanel12.ColumnCount = 2;
            this.tableLayoutPanel12.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel12.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel12.Controls.Add(this.lblRY, 0, 0);
            this.tableLayoutPanel12.Controls.Add(this.txtResultY, 1, 0);
            this.tableLayoutPanel12.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel12.Location = new System.Drawing.Point(116, 3);
            this.tableLayoutPanel12.Name = "tableLayoutPanel12";
            this.tableLayoutPanel12.RowCount = 1;
            this.tableLayoutPanel12.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel12.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel12.Size = new System.Drawing.Size(107, 26);
            this.tableLayoutPanel12.TabIndex = 1;
            // 
            // lblRY
            // 
            this.lblRY.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblRY.AutoSize = true;
            this.lblRY.Location = new System.Drawing.Point(33, 3);
            this.lblRY.Margin = new System.Windows.Forms.Padding(3);
            this.lblRY.Name = "lblRY";
            this.lblRY.Size = new System.Drawing.Size(17, 20);
            this.lblRY.TabIndex = 1;
            this.lblRY.Text = "Y:";
            // 
            // txtResultY
            // 
            this.txtResultY.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtResultY.Location = new System.Drawing.Point(55, 2);
            this.txtResultY.Margin = new System.Windows.Forms.Padding(2);
            this.txtResultY.Name = "txtResultY";
            this.txtResultY.ReadOnly = true;
            this.txtResultY.Size = new System.Drawing.Size(50, 21);
            this.txtResultY.TabIndex = 4;
            // 
            // tableLayoutPanel11
            // 
            this.tableLayoutPanel11.ColumnCount = 2;
            this.tableLayoutPanel11.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel11.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel11.Controls.Add(this.lblRX, 0, 0);
            this.tableLayoutPanel11.Controls.Add(this.txtResultX, 1, 0);
            this.tableLayoutPanel11.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel11.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel11.Name = "tableLayoutPanel11";
            this.tableLayoutPanel11.RowCount = 1;
            this.tableLayoutPanel11.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel11.Size = new System.Drawing.Size(107, 26);
            this.tableLayoutPanel11.TabIndex = 0;
            // 
            // lblRX
            // 
            this.lblRX.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblRX.AutoSize = true;
            this.lblRX.Location = new System.Drawing.Point(33, 3);
            this.lblRX.Margin = new System.Windows.Forms.Padding(3);
            this.lblRX.Name = "lblRX";
            this.lblRX.Size = new System.Drawing.Size(17, 20);
            this.lblRX.TabIndex = 2;
            this.lblRX.Text = "X:";
            this.lblRX.Click += new System.EventHandler(this.lblRX_Click);
            // 
            // txtResultX
            // 
            this.txtResultX.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtResultX.Location = new System.Drawing.Point(55, 2);
            this.txtResultX.Margin = new System.Windows.Forms.Padding(2);
            this.txtResultX.Name = "txtResultX";
            this.txtResultX.ReadOnly = true;
            this.txtResultX.Size = new System.Drawing.Size(50, 21);
            this.txtResultX.TabIndex = 5;
            // 
            // listViewResults
            // 
            this.listViewResults.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colIdx,
            this.colX,
            this.colY,
            this.colT,
            this.colScore});
            this.listViewResults.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewResults.FullRowSelect = true;
            this.listViewResults.GridLines = true;
            this.listViewResults.HideSelection = false;
            this.listViewResults.Location = new System.Drawing.Point(2, 40);
            this.listViewResults.Margin = new System.Windows.Forms.Padding(2);
            this.listViewResults.MultiSelect = false;
            this.listViewResults.Name = "listViewResults";
            this.listViewResults.Size = new System.Drawing.Size(341, 230);
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
            // tableLayoutPanel14
            // 
            this.tableLayoutPanel14.ColumnCount = 1;
            this.tableLayoutPanel14.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 49.26471F));
            this.tableLayoutPanel14.Controls.Add(this.tableLayoutPanel18, 0, 3);
            this.tableLayoutPanel14.Controls.Add(this.tableLayoutPanel17, 0, 2);
            this.tableLayoutPanel14.Controls.Add(this.tableLayoutPanel16, 0, 1);
            this.tableLayoutPanel14.Controls.Add(this.tableLayoutPanel15, 0, 0);
            this.tableLayoutPanel14.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel14.Location = new System.Drawing.Point(354, 3);
            this.tableLayoutPanel14.Name = "tableLayoutPanel14";
            this.tableLayoutPanel14.RowCount = 13;
            this.tableLayoutPanel14.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel14.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel14.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel14.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel14.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel14.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel14.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel14.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel14.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel14.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel14.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel14.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel14.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel14.Size = new System.Drawing.Size(250, 333);
            this.tableLayoutPanel14.TabIndex = 23;
            // 
            // tableLayoutPanel18
            // 
            this.tableLayoutPanel18.ColumnCount = 2;
            this.tableLayoutPanel18.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel18.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel18.Controls.Add(this.chkShowIndexes, 0, 0);
            this.tableLayoutPanel18.Controls.Add(this.chkHighlightRef, 1, 0);
            this.tableLayoutPanel18.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel18.Location = new System.Drawing.Point(3, 117);
            this.tableLayoutPanel18.Name = "tableLayoutPanel18";
            this.tableLayoutPanel18.RowCount = 1;
            this.tableLayoutPanel18.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel18.Size = new System.Drawing.Size(244, 32);
            this.tableLayoutPanel18.TabIndex = 5;
            // 
            // chkShowIndexes
            // 
            this.chkShowIndexes.AutoSize = true;
            this.chkShowIndexes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkShowIndexes.Location = new System.Drawing.Point(2, 2);
            this.chkShowIndexes.Margin = new System.Windows.Forms.Padding(2);
            this.chkShowIndexes.Name = "chkShowIndexes";
            this.chkShowIndexes.Size = new System.Drawing.Size(118, 28);
            this.chkShowIndexes.TabIndex = 16;
            this.chkShowIndexes.Text = "Show Indexes";
            this.chkShowIndexes.UseVisualStyleBackColor = true;
            // 
            // chkHighlightRef
            // 
            this.chkHighlightRef.AutoSize = true;
            this.chkHighlightRef.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkHighlightRef.Location = new System.Drawing.Point(124, 2);
            this.chkHighlightRef.Margin = new System.Windows.Forms.Padding(2);
            this.chkHighlightRef.Name = "chkHighlightRef";
            this.chkHighlightRef.Size = new System.Drawing.Size(118, 28);
            this.chkHighlightRef.TabIndex = 17;
            this.chkHighlightRef.Text = "Highlight Center";
            this.chkHighlightRef.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel17
            // 
            this.tableLayoutPanel17.ColumnCount = 2;
            this.tableLayoutPanel17.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel17.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 80F));
            this.tableLayoutPanel17.Controls.Add(this.txtAvgT, 1, 0);
            this.tableLayoutPanel17.Controls.Add(this.lblAvgY, 0, 0);
            this.tableLayoutPanel17.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel17.Location = new System.Drawing.Point(3, 79);
            this.tableLayoutPanel17.Name = "tableLayoutPanel17";
            this.tableLayoutPanel17.RowCount = 1;
            this.tableLayoutPanel17.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel17.Size = new System.Drawing.Size(244, 32);
            this.tableLayoutPanel17.TabIndex = 4;
            // 
            // txtAvgT
            // 
            this.txtAvgT.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtAvgT.Location = new System.Drawing.Point(50, 2);
            this.txtAvgT.Margin = new System.Windows.Forms.Padding(2);
            this.txtAvgT.Name = "txtAvgT";
            this.txtAvgT.ReadOnly = true;
            this.txtAvgT.Size = new System.Drawing.Size(192, 21);
            this.txtAvgT.TabIndex = 15;
            // 
            // lblAvgY
            // 
            this.lblAvgY.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblAvgY.AutoSize = true;
            this.lblAvgY.Location = new System.Drawing.Point(3, 3);
            this.lblAvgY.Margin = new System.Windows.Forms.Padding(3);
            this.lblAvgY.Name = "lblAvgY";
            this.lblAvgY.Size = new System.Drawing.Size(42, 26);
            this.lblAvgY.TabIndex = 11;
            this.lblAvgY.Text = "Avg Y:";
            // 
            // tableLayoutPanel16
            // 
            this.tableLayoutPanel16.ColumnCount = 2;
            this.tableLayoutPanel16.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel16.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 80F));
            this.tableLayoutPanel16.Controls.Add(this.lblAvgT, 0, 0);
            this.tableLayoutPanel16.Controls.Add(this.txtAvgY, 1, 0);
            this.tableLayoutPanel16.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel16.Location = new System.Drawing.Point(3, 41);
            this.tableLayoutPanel16.Name = "tableLayoutPanel16";
            this.tableLayoutPanel16.RowCount = 1;
            this.tableLayoutPanel16.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel16.Size = new System.Drawing.Size(244, 32);
            this.tableLayoutPanel16.TabIndex = 3;
            // 
            // lblAvgT
            // 
            this.lblAvgT.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblAvgT.AutoSize = true;
            this.lblAvgT.Location = new System.Drawing.Point(3, 3);
            this.lblAvgT.Margin = new System.Windows.Forms.Padding(3);
            this.lblAvgT.Name = "lblAvgT";
            this.lblAvgT.Size = new System.Drawing.Size(42, 26);
            this.lblAvgT.TabIndex = 12;
            this.lblAvgT.Text = "Avg T:";
            // 
            // txtAvgY
            // 
            this.txtAvgY.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtAvgY.Location = new System.Drawing.Point(50, 2);
            this.txtAvgY.Margin = new System.Windows.Forms.Padding(2);
            this.txtAvgY.Name = "txtAvgY";
            this.txtAvgY.ReadOnly = true;
            this.txtAvgY.Size = new System.Drawing.Size(192, 21);
            this.txtAvgY.TabIndex = 14;
            // 
            // tableLayoutPanel15
            // 
            this.tableLayoutPanel15.ColumnCount = 2;
            this.tableLayoutPanel15.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel15.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 80F));
            this.tableLayoutPanel15.Controls.Add(this.lblAvgX, 0, 0);
            this.tableLayoutPanel15.Controls.Add(this.txtAvgX, 1, 0);
            this.tableLayoutPanel15.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel15.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel15.Name = "tableLayoutPanel15";
            this.tableLayoutPanel15.RowCount = 1;
            this.tableLayoutPanel15.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel15.Size = new System.Drawing.Size(244, 32);
            this.tableLayoutPanel15.TabIndex = 2;
            // 
            // lblAvgX
            // 
            this.lblAvgX.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblAvgX.AutoSize = true;
            this.lblAvgX.Location = new System.Drawing.Point(3, 3);
            this.lblAvgX.Margin = new System.Windows.Forms.Padding(3);
            this.lblAvgX.Name = "lblAvgX";
            this.lblAvgX.Size = new System.Drawing.Size(42, 26);
            this.lblAvgX.TabIndex = 10;
            this.lblAvgX.Text = "Avg X:";
            // 
            // txtAvgX
            // 
            this.txtAvgX.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtAvgX.Location = new System.Drawing.Point(50, 2);
            this.txtAvgX.Margin = new System.Windows.Forms.Padding(2);
            this.txtAvgX.Name = "txtAvgX";
            this.txtAvgX.ReadOnly = true;
            this.txtAvgX.Size = new System.Drawing.Size(192, 21);
            this.txtAvgX.TabIndex = 13;
            // 
            // _btnSaveParam
            // 
            this._btnSaveParam.Dock = System.Windows.Forms.DockStyle.Fill;
            this._btnSaveParam.Location = new System.Drawing.Point(261, 5);
            this._btnSaveParam.Margin = new System.Windows.Forms.Padding(5);
            this._btnSaveParam.Name = "_btnSaveParam";
            this._btnSaveParam.Size = new System.Drawing.Size(247, 44);
            this._btnSaveParam.TabIndex = 6;
            this._btnSaveParam.Text = "Save Param";
            this._btnSaveParam.Click += new System.EventHandler(this._btnSaveParam_Click);
            // 
            // _btnLoadParam
            // 
            this._btnLoadParam.Dock = System.Windows.Forms.DockStyle.Fill;
            this._btnLoadParam.Location = new System.Drawing.Point(5, 5);
            this._btnLoadParam.Margin = new System.Windows.Forms.Padding(5);
            this._btnLoadParam.Name = "_btnLoadParam";
            this._btnLoadParam.Size = new System.Drawing.Size(246, 44);
            this._btnLoadParam.TabIndex = 7;
            this._btnLoadParam.Text = "Load Param";
            this._btnLoadParam.Click += new System.EventHandler(this._btnLoadParam_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.tabControl_Vision);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(2, 2);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox2.Size = new System.Drawing.Size(515, 659);
            this.groupBox2.TabIndex = 20;
            this.groupBox2.TabStop = false;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 54.54546F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45.45454F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel7, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1154, 750);
            this.tableLayoutPanel1.TabIndex = 22;
            // 
            // tableLayoutPanel7
            // 
            this.tableLayoutPanel7.ColumnCount = 1;
            this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel7.Controls.Add(this.groupBoxdieInputControl, 0, 0);
            this.tableLayoutPanel7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel7.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel7.Name = "tableLayoutPanel7";
            this.tableLayoutPanel7.RowCount = 1;
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel7.Size = new System.Drawing.Size(623, 744);
            this.tableLayoutPanel7.TabIndex = 1;
            // 
            // groupBoxdieInputControl
            // 
            this.groupBoxdieInputControl.Controls.Add(this.tableLayoutPanel4);
            this.groupBoxdieInputControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxdieInputControl.Location = new System.Drawing.Point(2, 2);
            this.groupBoxdieInputControl.Margin = new System.Windows.Forms.Padding(2);
            this.groupBoxdieInputControl.Name = "groupBoxdieInputControl";
            this.groupBoxdieInputControl.Padding = new System.Windows.Forms.Padding(2);
            this.groupBoxdieInputControl.Size = new System.Drawing.Size(619, 740);
            this.groupBoxdieInputControl.TabIndex = 1;
            this.groupBoxdieInputControl.TabStop = false;
            this.groupBoxdieInputControl.Text = "Ready";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.groupBox2, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel3, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this._btnClose, 0, 2);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(632, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 3;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 91.57895F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 8.421053F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(519, 744);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Controls.Add(this._btnLoadParam, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this._btnSaveParam, 1, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 666);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(513, 54);
            this.tableLayoutPanel3.TabIndex = 21;
            // 
            // _btnClose
            // 
            this._btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._btnClose.Location = new System.Drawing.Point(461, 725);
            this._btnClose.Margin = new System.Windows.Forms.Padding(2);
            this._btnClose.Name = "_btnClose";
            this._btnClose.Size = new System.Drawing.Size(56, 17);
            this._btnClose.TabIndex = 2;
            this._btnClose.Text = "Close";
            this._btnClose.Click += new System.EventHandler(this._btnClose_Click);
            // 
            // PatternMatchingControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this._lblStatus);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "PatternMatchingControl";
            this.Size = new System.Drawing.Size(1154, 750);
            this.tabControl_Vision.ResumeLayout(false);
            this.tabPageROI.ResumeLayout(false);
            this.tabPageParam.ResumeLayout(false);
            this.tabPageResult.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel5.ResumeLayout(false);
            this.groupBoxImageView.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._viewer)).EndInit();
            this.tableLayoutPanel6.ResumeLayout(false);
            this.groupSearchMode.ResumeLayout(false);
            this.groupSearchMode.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.tableLayoutPanel8.ResumeLayout(false);
            this.tableLayoutPanel9.ResumeLayout(false);
            this.tableLayoutPanel10.ResumeLayout(false);
            this.tableLayoutPanel13.ResumeLayout(false);
            this.tableLayoutPanel13.PerformLayout();
            this.tableLayoutPanel12.ResumeLayout(false);
            this.tableLayoutPanel12.PerformLayout();
            this.tableLayoutPanel11.ResumeLayout(false);
            this.tableLayoutPanel11.PerformLayout();
            this.tableLayoutPanel14.ResumeLayout(false);
            this.tableLayoutPanel18.ResumeLayout(false);
            this.tableLayoutPanel18.PerformLayout();
            this.tableLayoutPanel17.ResumeLayout(false);
            this.tableLayoutPanel17.PerformLayout();
            this.tableLayoutPanel16.ResumeLayout(false);
            this.tableLayoutPanel16.PerformLayout();
            this.tableLayoutPanel15.ResumeLayout(false);
            this.tableLayoutPanel15.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel7.ResumeLayout(false);
            this.groupBoxdieInputControl.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private GroupBox groupBoxImageView;
        private GroupBox groupBox2;
        private GroupBox groupBox3;
        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel2;
        private TableLayoutPanel tableLayoutPanel3;
        private TableLayoutPanel tableLayoutPanel4;
        private TableLayoutPanel tableLayoutPanel5;
        private TableLayoutPanel tableLayoutPanel6;
        private GroupBox groupBoxdieInputControl;
        private TableLayoutPanel tableLayoutPanel7;
        private TableLayoutPanel tableLayoutPanel8;
        private TableLayoutPanel tableLayoutPanel9;
        private TableLayoutPanel tableLayoutPanel10;
        private TableLayoutPanel tableLayoutPanel13;
        private TableLayoutPanel tableLayoutPanel12;
        private TableLayoutPanel tableLayoutPanel11;
        private TableLayoutPanel tableLayoutPanel14;
        private TableLayoutPanel tableLayoutPanel18;
        private TableLayoutPanel tableLayoutPanel17;
        private TableLayoutPanel tableLayoutPanel16;
        private TableLayoutPanel tableLayoutPanel15;
        private Button _btnClose;
    }
}