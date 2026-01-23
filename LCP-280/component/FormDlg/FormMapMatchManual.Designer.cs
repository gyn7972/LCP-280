namespace QMC.LCP_280.Process.Component
{
    partial class FormMapMatchManual
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.TableLayoutPanel tlpRoot;
        private System.Windows.Forms.TableLayoutPanel tlpGrid;

        private System.Windows.Forms.Panel pnlScan;
        private System.Windows.Forms.Panel pnlDownload;
        private System.Windows.Forms.Panel pnlCamera;
        private System.Windows.Forms.Panel pnlManual;

        private QMC.LCP_280.Process.Component.DieScanMapControl viewScan;
        private QMC.LCP_280.Process.Component.DieScanMapControl viewDownload;

        private System.Windows.Forms.PictureBox pbCamera;

        private System.Windows.Forms.ListBox lbPairs;

        private System.Windows.Forms.Button btnPickScan;
        private System.Windows.Forms.Button btnPickDownload;
        private System.Windows.Forms.Button btnAddPair;
        private System.Windows.Forms.Button btnRemovePair;
        private System.Windows.Forms.Button btnClearPairs;

        private System.Windows.Forms.GroupBox gbTransform;
        private System.Windows.Forms.Label lblDx;
        private System.Windows.Forms.Label lblDy;
        private System.Windows.Forms.NumericUpDown nudDx;
        private System.Windows.Forms.NumericUpDown nudDy;
        private System.Windows.Forms.Label lblRotate;
        private System.Windows.Forms.ComboBox cbRotate;
        private System.Windows.Forms.CheckBox chkMirrorX;
        private System.Windows.Forms.CheckBox chkMirrorY;

        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.Button btnClose;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.tlpRoot = new System.Windows.Forms.TableLayoutPanel();
            this.tlpGrid = new System.Windows.Forms.TableLayoutPanel();
            this.pnlScan = new System.Windows.Forms.Panel();
            this.tlpScan = new System.Windows.Forms.TableLayoutPanel();
            this.viewScan = new QMC.LCP_280.Process.Component.DieScanMapControl();
            this.pnlDownload = new System.Windows.Forms.Panel();
            this.tlpDown = new System.Windows.Forms.TableLayoutPanel();
            this.viewDownload = new QMC.LCP_280.Process.Component.DieScanMapControl();
            this.pnlCamera = new System.Windows.Forms.Panel();
            this.pbCamera = new System.Windows.Forms.PictureBox();
            this.pnlManual = new System.Windows.Forms.Panel();
            this.tlpManual = new System.Windows.Forms.TableLayoutPanel();
            this.btnPickScan = new System.Windows.Forms.Button();
            this.btnPickDownload = new System.Windows.Forms.Button();
            this.btnAddPair = new System.Windows.Forms.Button();
            this.btnRemovePair = new System.Windows.Forms.Button();
            this.lbPairs = new System.Windows.Forms.ListBox();
            this.gbTransform = new System.Windows.Forms.GroupBox();
            this.tlpTf = new System.Windows.Forms.TableLayoutPanel();
            this.nudDx = new System.Windows.Forms.NumericUpDown();
            this.nudDy = new System.Windows.Forms.NumericUpDown();
            this.cbRotate = new System.Windows.Forms.ComboBox();
            this.chkMirrorX = new System.Windows.Forms.CheckBox();
            this.chkMirrorY = new System.Windows.Forms.CheckBox();
            this.lblDx = new System.Windows.Forms.Label();
            this.lblDy = new System.Windows.Forms.Label();
            this.lblRotate = new System.Windows.Forms.Label();
            this.btnApply = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.pnlClear = new System.Windows.Forms.Panel();
            this.btnClearPairs = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.btnUp = new System.Windows.Forms.Button();
            this.btnDown = new System.Windows.Forms.Button();
            this.btnLeft = new System.Windows.Forms.Button();
            this.btnRight = new System.Windows.Forms.Button();
            this.tlpRoot.SuspendLayout();
            this.tlpGrid.SuspendLayout();
            this.pnlScan.SuspendLayout();
            this.tlpScan.SuspendLayout();
            this.pnlDownload.SuspendLayout();
            this.tlpDown.SuspendLayout();
            this.pnlCamera.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbCamera)).BeginInit();
            this.pnlManual.SuspendLayout();
            this.tlpManual.SuspendLayout();
            this.gbTransform.SuspendLayout();
            this.tlpTf.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudDx)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudDy)).BeginInit();
            this.pnlClear.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpRoot
            // 
            this.tlpRoot.ColumnCount = 1;
            this.tlpRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRoot.Controls.Add(this.tlpGrid, 0, 0);
            this.tlpRoot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpRoot.Location = new System.Drawing.Point(0, 0);
            this.tlpRoot.Name = "tlpRoot";
            this.tlpRoot.RowCount = 1;
            this.tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRoot.Size = new System.Drawing.Size(1009, 914);
            this.tlpRoot.TabIndex = 0;
            // 
            // tlpGrid
            // 
            this.tlpGrid.ColumnCount = 2;
            this.tlpGrid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpGrid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpGrid.Controls.Add(this.pnlCamera, 0, 1);
            this.tlpGrid.Controls.Add(this.pnlManual, 1, 1);
            this.tlpGrid.Controls.Add(this.pnlScan, 1, 0);
            this.tlpGrid.Controls.Add(this.pnlDownload, 0, 0);
            this.tlpGrid.Location = new System.Drawing.Point(0, 0);
            this.tlpGrid.Margin = new System.Windows.Forms.Padding(0);
            this.tlpGrid.Name = "tlpGrid";
            this.tlpGrid.RowCount = 2;
            this.tlpGrid.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 41.83314F));
            this.tlpGrid.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 58.16686F));
            this.tlpGrid.Size = new System.Drawing.Size(1009, 914);
            this.tlpGrid.TabIndex = 0;
            // 
            // pnlScan
            // 
            this.pnlScan.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(160)))), ((int)(((byte)(160)))));
            this.pnlScan.Controls.Add(this.tlpScan);
            this.pnlScan.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlScan.Location = new System.Drawing.Point(507, 3);
            this.pnlScan.Name = "pnlScan";
            this.pnlScan.Padding = new System.Windows.Forms.Padding(10);
            this.pnlScan.Size = new System.Drawing.Size(499, 376);
            this.pnlScan.TabIndex = 0;
            // 
            // tlpScan
            // 
            this.tlpScan.ColumnCount = 1;
            this.tlpScan.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpScan.Controls.Add(this.viewScan, 0, 0);
            this.tlpScan.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpScan.Location = new System.Drawing.Point(10, 10);
            this.tlpScan.Name = "tlpScan";
            this.tlpScan.RowCount = 1;
            this.tlpScan.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpScan.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpScan.Size = new System.Drawing.Size(479, 356);
            this.tlpScan.TabIndex = 0;
            // 
            // viewScan
            // 
            this.viewScan.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(160)))), ((int)(((byte)(160)))));
            this.viewScan.CenterOnPivot = true;
            this.viewScan.Dock = System.Windows.Forms.DockStyle.Fill;
            this.viewScan.Location = new System.Drawing.Point(3, 3);
            this.viewScan.Name = "viewScan";
            this.viewScan.Size = new System.Drawing.Size(473, 350);
            this.viewScan.TabIndex = 1;
            // 
            // pnlDownload
            // 
            this.pnlDownload.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(160)))), ((int)(((byte)(160)))));
            this.pnlDownload.Controls.Add(this.tlpDown);
            this.pnlDownload.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlDownload.Location = new System.Drawing.Point(3, 3);
            this.pnlDownload.Name = "pnlDownload";
            this.pnlDownload.Padding = new System.Windows.Forms.Padding(10);
            this.pnlDownload.Size = new System.Drawing.Size(498, 376);
            this.pnlDownload.TabIndex = 1;
            // 
            // tlpDown
            // 
            this.tlpDown.ColumnCount = 1;
            this.tlpDown.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpDown.Controls.Add(this.viewDownload, 0, 0);
            this.tlpDown.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpDown.Location = new System.Drawing.Point(10, 10);
            this.tlpDown.Name = "tlpDown";
            this.tlpDown.RowCount = 1;
            this.tlpDown.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpDown.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpDown.Size = new System.Drawing.Size(478, 356);
            this.tlpDown.TabIndex = 0;
            // 
            // viewDownload
            // 
            this.viewDownload.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(160)))), ((int)(((byte)(160)))));
            this.viewDownload.CenterOnPivot = true;
            this.viewDownload.Dock = System.Windows.Forms.DockStyle.Fill;
            this.viewDownload.Location = new System.Drawing.Point(3, 3);
            this.viewDownload.Name = "viewDownload";
            this.viewDownload.Size = new System.Drawing.Size(472, 350);
            this.viewDownload.TabIndex = 1;
            // 
            // pnlCamera
            // 
            this.pnlCamera.BackColor = System.Drawing.Color.Transparent;
            this.pnlCamera.Controls.Add(this.tableLayoutPanel1);
            this.pnlCamera.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlCamera.Location = new System.Drawing.Point(3, 385);
            this.pnlCamera.Name = "pnlCamera";
            this.pnlCamera.Padding = new System.Windows.Forms.Padding(10);
            this.pnlCamera.Size = new System.Drawing.Size(498, 526);
            this.pnlCamera.TabIndex = 2;
            // 
            // pbCamera
            // 
            this.pbCamera.BackColor = System.Drawing.Color.Black;
            this.pbCamera.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbCamera.Location = new System.Drawing.Point(3, 124);
            this.pbCamera.Name = "pbCamera";
            this.pbCamera.Size = new System.Drawing.Size(472, 379);
            this.pbCamera.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbCamera.TabIndex = 0;
            this.pbCamera.TabStop = false;
            // 
            // pnlManual
            // 
            this.pnlManual.BackColor = System.Drawing.Color.White;
            this.pnlManual.Controls.Add(this.tlpManual);
            this.pnlManual.Controls.Add(this.pnlClear);
            this.pnlManual.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlManual.Location = new System.Drawing.Point(507, 385);
            this.pnlManual.Name = "pnlManual";
            this.pnlManual.Padding = new System.Windows.Forms.Padding(10);
            this.pnlManual.Size = new System.Drawing.Size(499, 526);
            this.pnlManual.TabIndex = 3;
            // 
            // tlpManual
            // 
            this.tlpManual.ColumnCount = 2;
            this.tlpManual.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpManual.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpManual.Controls.Add(this.btnPickScan, 0, 0);
            this.tlpManual.Controls.Add(this.btnPickDownload, 1, 0);
            this.tlpManual.Controls.Add(this.btnAddPair, 0, 1);
            this.tlpManual.Controls.Add(this.btnRemovePair, 1, 1);
            this.tlpManual.Controls.Add(this.lbPairs, 0, 2);
            this.tlpManual.Controls.Add(this.gbTransform, 0, 3);
            this.tlpManual.Controls.Add(this.btnApply, 0, 4);
            this.tlpManual.Controls.Add(this.btnClose, 1, 4);
            this.tlpManual.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpManual.Location = new System.Drawing.Point(10, 10);
            this.tlpManual.Name = "tlpManual";
            this.tlpManual.RowCount = 6;
            this.tlpManual.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 9.794989F));
            this.tlpManual.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 11.16173F));
            this.tlpManual.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 17.68868F));
            this.tlpManual.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 46.46227F));
            this.tlpManual.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10.14151F));
            this.tlpManual.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 4.716981F));
            this.tlpManual.Size = new System.Drawing.Size(479, 461);
            this.tlpManual.TabIndex = 0;
            // 
            // btnPickScan
            // 
            this.btnPickScan.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnPickScan.Location = new System.Drawing.Point(3, 3);
            this.btnPickScan.Name = "btnPickScan";
            this.btnPickScan.Size = new System.Drawing.Size(233, 39);
            this.btnPickScan.TabIndex = 0;
            this.btnPickScan.Text = "Pick Scan";
            this.btnPickScan.Click += new System.EventHandler(this.btnPickScan_Click);
            // 
            // btnPickDownload
            // 
            this.btnPickDownload.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnPickDownload.Location = new System.Drawing.Point(242, 3);
            this.btnPickDownload.Name = "btnPickDownload";
            this.btnPickDownload.Size = new System.Drawing.Size(234, 39);
            this.btnPickDownload.TabIndex = 1;
            this.btnPickDownload.Text = "Pick Download";
            this.btnPickDownload.Click += new System.EventHandler(this.btnPickDownload_Click);
            // 
            // btnAddPair
            // 
            this.btnAddPair.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnAddPair.Location = new System.Drawing.Point(3, 48);
            this.btnAddPair.Name = "btnAddPair";
            this.btnAddPair.Size = new System.Drawing.Size(233, 45);
            this.btnAddPair.TabIndex = 2;
            this.btnAddPair.Text = "Add Pair";
            this.btnAddPair.Click += new System.EventHandler(this.btnAddPair_Click);
            // 
            // btnRemovePair
            // 
            this.btnRemovePair.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnRemovePair.Location = new System.Drawing.Point(242, 48);
            this.btnRemovePair.Name = "btnRemovePair";
            this.btnRemovePair.Size = new System.Drawing.Size(234, 45);
            this.btnRemovePair.TabIndex = 3;
            this.btnRemovePair.Text = "Remove";
            this.btnRemovePair.Click += new System.EventHandler(this.btnRemovePair_Click);
            // 
            // lbPairs
            // 
            this.tlpManual.SetColumnSpan(this.lbPairs, 2);
            this.lbPairs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbPairs.ItemHeight = 25;
            this.lbPairs.Location = new System.Drawing.Point(3, 99);
            this.lbPairs.Name = "lbPairs";
            this.lbPairs.Size = new System.Drawing.Size(473, 75);
            this.lbPairs.TabIndex = 4;
            // 
            // gbTransform
            // 
            this.tlpManual.SetColumnSpan(this.gbTransform, 2);
            this.gbTransform.Controls.Add(this.tlpTf);
            this.gbTransform.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbTransform.Location = new System.Drawing.Point(3, 180);
            this.gbTransform.Name = "gbTransform";
            this.gbTransform.Size = new System.Drawing.Size(473, 208);
            this.gbTransform.TabIndex = 5;
            this.gbTransform.TabStop = false;
            this.gbTransform.Text = "Transform (Manual)";
            // 
            // tlpTf
            // 
            this.tlpTf.ColumnCount = 2;
            this.tlpTf.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpTf.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpTf.Controls.Add(this.nudDx, 1, 0);
            this.tlpTf.Controls.Add(this.nudDy, 1, 1);
            this.tlpTf.Controls.Add(this.cbRotate, 1, 2);
            this.tlpTf.Controls.Add(this.chkMirrorX, 1, 3);
            this.tlpTf.Controls.Add(this.chkMirrorY, 1, 4);
            this.tlpTf.Controls.Add(this.lblDx, 0, 0);
            this.tlpTf.Controls.Add(this.lblDy, 0, 1);
            this.tlpTf.Controls.Add(this.lblRotate, 0, 2);
            this.tlpTf.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpTf.Location = new System.Drawing.Point(3, 27);
            this.tlpTf.Name = "tlpTf";
            this.tlpTf.RowCount = 5;
            this.tlpTf.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpTf.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpTf.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 21.34831F));
            this.tlpTf.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20.22472F));
            this.tlpTf.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 19.66292F));
            this.tlpTf.Size = new System.Drawing.Size(467, 178);
            this.tlpTf.TabIndex = 0;
            // 
            // nudDx
            // 
            this.nudDx.DecimalPlaces = 3;
            this.nudDx.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nudDx.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.nudDx.Location = new System.Drawing.Point(236, 3);
            this.nudDx.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this.nudDx.Minimum = new decimal(new int[] {
            999999,
            0,
            0,
            -2147483648});
            this.nudDx.Name = "nudDx";
            this.nudDx.Size = new System.Drawing.Size(228, 31);
            this.nudDx.TabIndex = 1;
            // 
            // nudDy
            // 
            this.nudDy.DecimalPlaces = 3;
            this.nudDy.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nudDy.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.nudDy.Location = new System.Drawing.Point(236, 38);
            this.nudDy.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this.nudDy.Minimum = new decimal(new int[] {
            999999,
            0,
            0,
            -2147483648});
            this.nudDy.Name = "nudDy";
            this.nudDy.Size = new System.Drawing.Size(228, 31);
            this.nudDy.TabIndex = 3;
            // 
            // cbRotate
            // 
            this.cbRotate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbRotate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbRotate.Items.AddRange(new object[] {
            "0",
            "90",
            "180",
            "270"});
            this.cbRotate.Location = new System.Drawing.Point(236, 73);
            this.cbRotate.Name = "cbRotate";
            this.cbRotate.Size = new System.Drawing.Size(228, 33);
            this.cbRotate.TabIndex = 5;
            // 
            // chkMirrorX
            // 
            this.chkMirrorX.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkMirrorX.Location = new System.Drawing.Point(236, 110);
            this.chkMirrorX.Name = "chkMirrorX";
            this.chkMirrorX.Size = new System.Drawing.Size(228, 29);
            this.chkMirrorX.TabIndex = 6;
            this.chkMirrorX.Text = "Mirror X";
            // 
            // chkMirrorY
            // 
            this.chkMirrorY.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkMirrorY.Location = new System.Drawing.Point(236, 145);
            this.chkMirrorY.Name = "chkMirrorY";
            this.chkMirrorY.Size = new System.Drawing.Size(228, 30);
            this.chkMirrorY.TabIndex = 7;
            this.chkMirrorY.Text = "Mirror Y";
            // 
            // lblDx
            // 
            this.lblDx.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDx.Location = new System.Drawing.Point(3, 0);
            this.lblDx.Name = "lblDx";
            this.lblDx.Size = new System.Drawing.Size(227, 35);
            this.lblDx.TabIndex = 0;
            this.lblDx.Text = "dX";
            // 
            // lblDy
            // 
            this.lblDy.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDy.Location = new System.Drawing.Point(3, 35);
            this.lblDy.Name = "lblDy";
            this.lblDy.Size = new System.Drawing.Size(227, 35);
            this.lblDy.TabIndex = 2;
            this.lblDy.Text = "dY";
            // 
            // lblRotate
            // 
            this.lblRotate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblRotate.Location = new System.Drawing.Point(3, 70);
            this.lblRotate.Name = "lblRotate";
            this.lblRotate.Size = new System.Drawing.Size(227, 37);
            this.lblRotate.TabIndex = 4;
            this.lblRotate.Text = "Rotate";
            // 
            // btnApply
            // 
            this.btnApply.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnApply.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnApply.Location = new System.Drawing.Point(3, 394);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(233, 40);
            this.btnApply.TabIndex = 6;
            this.btnApply.Text = "Apply";
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // btnClose
            // 
            this.btnClose.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnClose.Location = new System.Drawing.Point(242, 394);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(234, 40);
            this.btnClose.TabIndex = 7;
            this.btnClose.Text = "Close";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // pnlClear
            // 
            this.pnlClear.Controls.Add(this.btnClearPairs);
            this.pnlClear.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlClear.Location = new System.Drawing.Point(10, 471);
            this.pnlClear.Name = "pnlClear";
            this.pnlClear.Padding = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.pnlClear.Size = new System.Drawing.Size(479, 45);
            this.pnlClear.TabIndex = 1;
            // 
            // btnClearPairs
            // 
            this.btnClearPairs.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnClearPairs.Location = new System.Drawing.Point(369, 4);
            this.btnClearPairs.Name = "btnClearPairs";
            this.btnClearPairs.Size = new System.Drawing.Size(110, 41);
            this.btnClearPairs.TabIndex = 0;
            this.btnClearPairs.Text = "Clear Pairs";
            this.btnClearPairs.Click += new System.EventHandler(this.btnClearPairs_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Controls.Add(this.pbCamera, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(10, 10);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 23.91304F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 76.08696F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(478, 506);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 3;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel2.Controls.Add(this.btnUp, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnDown, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.btnLeft, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.btnRight, 2, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(472, 115);
            this.tableLayoutPanel2.TabIndex = 1;
            // 
            // btnUp
            // 
            this.btnUp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnUp.Location = new System.Drawing.Point(160, 3);
            this.btnUp.Name = "btnUp";
            this.btnUp.Size = new System.Drawing.Size(151, 51);
            this.btnUp.TabIndex = 1;
            this.btnUp.Text = "Up";
            this.btnUp.Click += new System.EventHandler(this.btnUp_Click);
            // 
            // btnDown
            // 
            this.btnDown.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnDown.Location = new System.Drawing.Point(160, 60);
            this.btnDown.Name = "btnDown";
            this.btnDown.Size = new System.Drawing.Size(151, 52);
            this.btnDown.TabIndex = 2;
            this.btnDown.Text = "Down";
            this.btnDown.Click += new System.EventHandler(this.btnDown_Click);
            // 
            // btnLeft
            // 
            this.btnLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnLeft.Location = new System.Drawing.Point(3, 60);
            this.btnLeft.Name = "btnLeft";
            this.btnLeft.Size = new System.Drawing.Size(151, 52);
            this.btnLeft.TabIndex = 3;
            this.btnLeft.Text = "Left";
            this.btnLeft.Click += new System.EventHandler(this.btnLeft_Click);
            // 
            // btnRight
            // 
            this.btnRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnRight.Location = new System.Drawing.Point(317, 60);
            this.btnRight.Name = "btnRight";
            this.btnRight.Size = new System.Drawing.Size(152, 52);
            this.btnRight.TabIndex = 4;
            this.btnRight.Text = "Right";
            this.btnRight.Click += new System.EventHandler(this.btnRight_Click);
            // 
            // FormMapMatchManual
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1009, 914);
            this.Controls.Add(this.tlpRoot);
            this.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormMapMatchManual";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Map Match.";
            this.tlpRoot.ResumeLayout(false);
            this.tlpGrid.ResumeLayout(false);
            this.pnlScan.ResumeLayout(false);
            this.tlpScan.ResumeLayout(false);
            this.pnlDownload.ResumeLayout(false);
            this.tlpDown.ResumeLayout(false);
            this.pnlCamera.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbCamera)).EndInit();
            this.pnlManual.ResumeLayout(false);
            this.tlpManual.ResumeLayout(false);
            this.gbTransform.ResumeLayout(false);
            this.tlpTf.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.nudDx)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudDy)).EndInit();
            this.pnlClear.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.TableLayoutPanel tlpScan;
        private System.Windows.Forms.TableLayoutPanel tlpDown;
        private System.Windows.Forms.TableLayoutPanel tlpManual;
        private System.Windows.Forms.TableLayoutPanel tlpTf;
        private System.Windows.Forms.Panel pnlClear;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Button btnUp;
        private System.Windows.Forms.Button btnDown;
        private System.Windows.Forms.Button btnLeft;
        private System.Windows.Forms.Button btnRight;
    }
}