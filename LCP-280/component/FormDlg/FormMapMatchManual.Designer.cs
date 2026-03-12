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
            this.pnlDownload = new System.Windows.Forms.Panel();
            this.tlpDown = new System.Windows.Forms.TableLayoutPanel();
            this.viewDownload = new QMC.LCP_280.Process.Component.DieScanMapControl();
            this.pnlScan = new System.Windows.Forms.Panel();
            this.tlpScan = new System.Windows.Forms.TableLayoutPanel();
            this.viewScan = new QMC.LCP_280.Process.Component.DieScanMapControl();
            this.pnlManual = new System.Windows.Forms.Panel();
            this.tlpManual = new System.Windows.Forms.TableLayoutPanel();
            this.btnPickScan = new System.Windows.Forms.Button();
            this.btnPickDownload = new System.Windows.Forms.Button();
            this.btnAddPair = new System.Windows.Forms.Button();
            this.btnRemovePair = new System.Windows.Forms.Button();
            this.lbPairs = new System.Windows.Forms.ListBox();
            this.gbTransform = new System.Windows.Forms.GroupBox();
            this.tlpTf = new System.Windows.Forms.TableLayoutPanel();
            this.lblMatchRate = new System.Windows.Forms.Label();
            this.btnApply = new System.Windows.Forms.Button();
            this.nudDx = new System.Windows.Forms.NumericUpDown();
            this.nudDy = new System.Windows.Forms.NumericUpDown();
            this.cbRotate = new System.Windows.Forms.ComboBox();
            this.chkMirrorX = new System.Windows.Forms.CheckBox();
            this.chkMirrorY = new System.Windows.Forms.CheckBox();
            this.lblDx = new System.Windows.Forms.Label();
            this.lblDy = new System.Windows.Forms.Label();
            this.lblRotate = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.btnSelectLogin = new System.Windows.Forms.Button();
            this.textBoxUserID = new System.Windows.Forms.TextBox();
            this.pnlClear = new System.Windows.Forms.Panel();
            this.btnClearPairs = new System.Windows.Forms.Button();
            this.pnlCamera = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.pbCamera = new System.Windows.Forms.PictureBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.btnUp = new System.Windows.Forms.Button();
            this.btnDown = new System.Windows.Forms.Button();
            this.btnLeft = new System.Windows.Forms.Button();
            this.btnRight = new System.Windows.Forms.Button();
            this.tlpRoot.SuspendLayout();
            this.tlpGrid.SuspendLayout();
            this.pnlDownload.SuspendLayout();
            this.tlpDown.SuspendLayout();
            this.pnlScan.SuspendLayout();
            this.tlpScan.SuspendLayout();
            this.pnlManual.SuspendLayout();
            this.tlpManual.SuspendLayout();
            this.gbTransform.SuspendLayout();
            this.tlpTf.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudDx)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudDy)).BeginInit();
            this.tableLayoutPanel3.SuspendLayout();
            this.pnlClear.SuspendLayout();
            this.pnlCamera.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbCamera)).BeginInit();
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
            this.tlpRoot.Size = new System.Drawing.Size(1143, 921);
            this.tlpRoot.TabIndex = 0;
            // 
            // tlpGrid
            // 
            this.tlpGrid.ColumnCount = 2;
            this.tlpGrid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpGrid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpGrid.Controls.Add(this.pnlDownload, 0, 0);
            this.tlpGrid.Controls.Add(this.pnlScan, 1, 0);
            this.tlpGrid.Controls.Add(this.pnlManual, 0, 1);
            this.tlpGrid.Controls.Add(this.pnlCamera, 1, 1);
            this.tlpGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpGrid.Location = new System.Drawing.Point(0, 0);
            this.tlpGrid.Margin = new System.Windows.Forms.Padding(0);
            this.tlpGrid.Name = "tlpGrid";
            this.tlpGrid.RowCount = 2;
            this.tlpGrid.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 44.90532F));
            this.tlpGrid.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 55.09468F));
            this.tlpGrid.Size = new System.Drawing.Size(1143, 921);
            this.tlpGrid.TabIndex = 0;
            // 
            // pnlDownload
            // 
            this.pnlDownload.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(160)))), ((int)(((byte)(160)))));
            this.pnlDownload.Controls.Add(this.tlpDown);
            this.pnlDownload.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlDownload.Location = new System.Drawing.Point(3, 3);
            this.pnlDownload.Name = "pnlDownload";
            this.pnlDownload.Padding = new System.Windows.Forms.Padding(10);
            this.pnlDownload.Size = new System.Drawing.Size(565, 407);
            this.pnlDownload.TabIndex = 1;
            // 
            // tlpDown
            // 
            this.tlpDown.ColumnCount = 1;
            this.tlpDown.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpDown.Controls.Add(this.viewDownload, 0, 0);
            this.tlpDown.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpDown.Location = new System.Drawing.Point(10, 10);
            this.tlpDown.Name = "tlpDown";
            this.tlpDown.RowCount = 1;
            this.tlpDown.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpDown.Size = new System.Drawing.Size(545, 387);
            this.tlpDown.TabIndex = 0;
            // 
            // viewDownload
            // 
            this.viewDownload.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(160)))), ((int)(((byte)(160)))));
            this.viewDownload.CenterOnPivot = true;
            this.viewDownload.Dock = System.Windows.Forms.DockStyle.Fill;
            this.viewDownload.Location = new System.Drawing.Point(2, 2);
            this.viewDownload.Margin = new System.Windows.Forms.Padding(2);
            this.viewDownload.Name = "viewDownload";
            this.viewDownload.Size = new System.Drawing.Size(541, 383);
            this.viewDownload.TabIndex = 1;
            // 
            // pnlScan
            // 
            this.pnlScan.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(160)))), ((int)(((byte)(160)))));
            this.pnlScan.Controls.Add(this.tlpScan);
            this.pnlScan.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlScan.Location = new System.Drawing.Point(574, 3);
            this.pnlScan.Name = "pnlScan";
            this.pnlScan.Padding = new System.Windows.Forms.Padding(10);
            this.pnlScan.Size = new System.Drawing.Size(566, 407);
            this.pnlScan.TabIndex = 0;
            // 
            // tlpScan
            // 
            this.tlpScan.ColumnCount = 1;
            this.tlpScan.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpScan.Controls.Add(this.viewScan, 0, 0);
            this.tlpScan.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpScan.Location = new System.Drawing.Point(10, 10);
            this.tlpScan.Name = "tlpScan";
            this.tlpScan.RowCount = 1;
            this.tlpScan.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpScan.Size = new System.Drawing.Size(546, 387);
            this.tlpScan.TabIndex = 0;
            // 
            // viewScan
            // 
            this.viewScan.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(160)))), ((int)(((byte)(160)))));
            this.viewScan.CenterOnPivot = true;
            this.viewScan.Dock = System.Windows.Forms.DockStyle.Fill;
            this.viewScan.Location = new System.Drawing.Point(2, 2);
            this.viewScan.Margin = new System.Windows.Forms.Padding(2);
            this.viewScan.Name = "viewScan";
            this.viewScan.Size = new System.Drawing.Size(542, 383);
            this.viewScan.TabIndex = 1;
            // 
            // pnlManual
            // 
            this.pnlManual.BackColor = System.Drawing.Color.White;
            this.pnlManual.Controls.Add(this.tlpManual);
            this.pnlManual.Controls.Add(this.pnlClear);
            this.pnlManual.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlManual.Location = new System.Drawing.Point(3, 416);
            this.pnlManual.Name = "pnlManual";
            this.pnlManual.Padding = new System.Windows.Forms.Padding(10);
            this.pnlManual.Size = new System.Drawing.Size(565, 502);
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
            this.tlpManual.Controls.Add(this.label1, 0, 4);
            this.tlpManual.Controls.Add(this.btnOk, 0, 5);
            this.tlpManual.Controls.Add(this.btnClose, 1, 5);
            this.tlpManual.Controls.Add(this.tableLayoutPanel3, 1, 4);
            this.tlpManual.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpManual.Location = new System.Drawing.Point(10, 10);
            this.tlpManual.Name = "tlpManual";
            this.tlpManual.RowCount = 7;
            this.tlpManual.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 7.827789F));
            this.tlpManual.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 7.436399F));
            this.tlpManual.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 7.240705F));
            this.tlpManual.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 49.31507F));
            this.tlpManual.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 7.632094F));
            this.tlpManual.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 15.06849F));
            this.tlpManual.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 7.04501F));
            this.tlpManual.Size = new System.Drawing.Size(545, 461);
            this.tlpManual.TabIndex = 0;
            // 
            // btnPickScan
            // 
            this.btnPickScan.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnPickScan.Location = new System.Drawing.Point(3, 3);
            this.btnPickScan.Name = "btnPickScan";
            this.btnPickScan.Size = new System.Drawing.Size(266, 29);
            this.btnPickScan.TabIndex = 0;
            this.btnPickScan.Text = "Pick Scan";
            this.btnPickScan.Click += new System.EventHandler(this.btnPickScan_Click);
            // 
            // btnPickDownload
            // 
            this.btnPickDownload.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnPickDownload.Location = new System.Drawing.Point(275, 3);
            this.btnPickDownload.Name = "btnPickDownload";
            this.btnPickDownload.Size = new System.Drawing.Size(267, 29);
            this.btnPickDownload.TabIndex = 1;
            this.btnPickDownload.Text = "Pick Download";
            this.btnPickDownload.Click += new System.EventHandler(this.btnPickDownload_Click);
            // 
            // btnAddPair
            // 
            this.btnAddPair.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnAddPair.Location = new System.Drawing.Point(3, 38);
            this.btnAddPair.Name = "btnAddPair";
            this.btnAddPair.Size = new System.Drawing.Size(266, 27);
            this.btnAddPair.TabIndex = 2;
            this.btnAddPair.Text = "Add Pair";
            this.btnAddPair.Click += new System.EventHandler(this.btnAddPair_Click);
            // 
            // btnRemovePair
            // 
            this.btnRemovePair.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnRemovePair.Location = new System.Drawing.Point(275, 38);
            this.btnRemovePair.Name = "btnRemovePair";
            this.btnRemovePair.Size = new System.Drawing.Size(267, 27);
            this.btnRemovePair.TabIndex = 3;
            this.btnRemovePair.Text = "Remove";
            this.btnRemovePair.Click += new System.EventHandler(this.btnRemovePair_Click);
            // 
            // lbPairs
            // 
            this.tlpManual.SetColumnSpan(this.lbPairs, 2);
            this.lbPairs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbPairs.ItemHeight = 25;
            this.lbPairs.Location = new System.Drawing.Point(3, 71);
            this.lbPairs.Name = "lbPairs";
            this.lbPairs.Size = new System.Drawing.Size(539, 26);
            this.lbPairs.TabIndex = 4;
            // 
            // gbTransform
            // 
            this.tlpManual.SetColumnSpan(this.gbTransform, 2);
            this.gbTransform.Controls.Add(this.tlpTf);
            this.gbTransform.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbTransform.Location = new System.Drawing.Point(3, 103);
            this.gbTransform.Name = "gbTransform";
            this.gbTransform.Size = new System.Drawing.Size(539, 217);
            this.gbTransform.TabIndex = 5;
            this.gbTransform.TabStop = false;
            this.gbTransform.Text = "Transform (Manual)";
            // 
            // tlpTf
            // 
            this.tlpTf.ColumnCount = 2;
            this.tlpTf.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpTf.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpTf.Controls.Add(this.lblMatchRate, 0, 4);
            this.tlpTf.Controls.Add(this.btnApply, 0, 3);
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
            this.tlpTf.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tlpTf.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 22.22222F));
            this.tlpTf.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 22.68518F));
            this.tlpTf.Size = new System.Drawing.Size(533, 187);
            this.tlpTf.TabIndex = 0;
            // 
            // lblMatchRate
            // 
            this.lblMatchRate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMatchRate.Font = new System.Drawing.Font("맑은 고딕", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblMatchRate.Location = new System.Drawing.Point(3, 142);
            this.lblMatchRate.Name = "lblMatchRate";
            this.lblMatchRate.Size = new System.Drawing.Size(260, 45);
            this.lblMatchRate.TabIndex = 10;
            this.lblMatchRate.Text = "매칭율: 0.0%";
            this.lblMatchRate.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnApply
            // 
            this.btnApply.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnApply.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnApply.Location = new System.Drawing.Point(3, 105);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(260, 34);
            this.btnApply.TabIndex = 9;
            this.btnApply.Text = "Apply";
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // nudDx
            // 
            this.nudDx.DecimalPlaces = 3;
            this.nudDx.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nudDx.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.nudDx.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.nudDx.Location = new System.Drawing.Point(269, 3);
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
            this.nudDx.Size = new System.Drawing.Size(261, 31);
            this.nudDx.TabIndex = 1;
            // 
            // nudDy
            // 
            this.nudDy.DecimalPlaces = 3;
            this.nudDy.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nudDy.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.nudDy.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.nudDy.Location = new System.Drawing.Point(269, 39);
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
            this.nudDy.Size = new System.Drawing.Size(261, 31);
            this.nudDy.TabIndex = 3;
            // 
            // cbRotate
            // 
            this.cbRotate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbRotate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbRotate.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.cbRotate.Items.AddRange(new object[] {
            "0",
            "90",
            "180",
            "270"});
            this.cbRotate.Location = new System.Drawing.Point(269, 75);
            this.cbRotate.Name = "cbRotate";
            this.cbRotate.Size = new System.Drawing.Size(261, 33);
            this.cbRotate.TabIndex = 5;
            // 
            // chkMirrorX
            // 
            this.chkMirrorX.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkMirrorX.Location = new System.Drawing.Point(269, 105);
            this.chkMirrorX.Name = "chkMirrorX";
            this.chkMirrorX.Size = new System.Drawing.Size(261, 34);
            this.chkMirrorX.TabIndex = 6;
            this.chkMirrorX.Text = "Mirror X";
            // 
            // chkMirrorY
            // 
            this.chkMirrorY.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkMirrorY.Location = new System.Drawing.Point(269, 145);
            this.chkMirrorY.Name = "chkMirrorY";
            this.chkMirrorY.Size = new System.Drawing.Size(261, 39);
            this.chkMirrorY.TabIndex = 7;
            this.chkMirrorY.Text = "Mirror Y";
            // 
            // lblDx
            // 
            this.lblDx.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDx.Location = new System.Drawing.Point(3, 0);
            this.lblDx.Name = "lblDx";
            this.lblDx.Size = new System.Drawing.Size(260, 36);
            this.lblDx.TabIndex = 0;
            this.lblDx.Text = "dX";
            // 
            // lblDy
            // 
            this.lblDy.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDy.Location = new System.Drawing.Point(3, 36);
            this.lblDy.Name = "lblDy";
            this.lblDy.Size = new System.Drawing.Size(260, 36);
            this.lblDy.TabIndex = 2;
            this.lblDy.Text = "dY";
            // 
            // lblRotate
            // 
            this.lblRotate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblRotate.Location = new System.Drawing.Point(3, 72);
            this.lblRotate.Name = "lblRotate";
            this.lblRotate.Size = new System.Drawing.Size(260, 30);
            this.lblRotate.TabIndex = 4;
            this.lblRotate.Text = "Rotate";
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label1.Location = new System.Drawing.Point(3, 323);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(266, 34);
            this.label1.TabIndex = 11;
            this.label1.Text = "User ID";
            // 
            // btnOk
            // 
            this.btnOk.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnOk.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnOk.Location = new System.Drawing.Point(3, 360);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(266, 62);
            this.btnOk.TabIndex = 10;
            this.btnOk.Text = "OK";
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnClose
            // 
            this.btnClose.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnClose.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnClose.Location = new System.Drawing.Point(275, 360);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(267, 62);
            this.btnClose.TabIndex = 7;
            this.btnClose.Text = "Cancle";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 77.62238F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 22.37762F));
            this.tableLayoutPanel3.Controls.Add(this.btnSelectLogin, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.textBoxUserID, 0, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(275, 326);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(267, 28);
            this.tableLayoutPanel3.TabIndex = 13;
            // 
            // btnSelectLogin
            // 
            this.btnSelectLogin.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnSelectLogin.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnSelectLogin.Location = new System.Drawing.Point(210, 3);
            this.btnSelectLogin.Name = "btnSelectLogin";
            this.btnSelectLogin.Size = new System.Drawing.Size(54, 22);
            this.btnSelectLogin.TabIndex = 13;
            this.btnSelectLogin.Text = "---";
            this.btnSelectLogin.Click += new System.EventHandler(this.btnSelectLogin_Click);
            // 
            // textBoxUserID
            // 
            this.textBoxUserID.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxUserID.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.textBoxUserID.Location = new System.Drawing.Point(3, 3);
            this.textBoxUserID.Name = "textBoxUserID";
            this.textBoxUserID.Size = new System.Drawing.Size(201, 34);
            this.textBoxUserID.TabIndex = 12;
            // 
            // pnlClear
            // 
            this.pnlClear.Controls.Add(this.btnClearPairs);
            this.pnlClear.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlClear.Location = new System.Drawing.Point(10, 471);
            this.pnlClear.Name = "pnlClear";
            this.pnlClear.Padding = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.pnlClear.Size = new System.Drawing.Size(545, 21);
            this.pnlClear.TabIndex = 1;
            // 
            // btnClearPairs
            // 
            this.btnClearPairs.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnClearPairs.Location = new System.Drawing.Point(420, 4);
            this.btnClearPairs.Name = "btnClearPairs";
            this.btnClearPairs.Size = new System.Drawing.Size(125, 17);
            this.btnClearPairs.TabIndex = 0;
            this.btnClearPairs.Text = "Clear Pairs";
            this.btnClearPairs.Click += new System.EventHandler(this.btnClearPairs_Click);
            // 
            // pnlCamera
            // 
            this.pnlCamera.BackColor = System.Drawing.Color.Transparent;
            this.pnlCamera.Controls.Add(this.tableLayoutPanel1);
            this.pnlCamera.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlCamera.Location = new System.Drawing.Point(574, 416);
            this.pnlCamera.Name = "pnlCamera";
            this.pnlCamera.Padding = new System.Windows.Forms.Padding(10);
            this.pnlCamera.Size = new System.Drawing.Size(566, 502);
            this.pnlCamera.TabIndex = 2;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.pbCamera, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(10, 10);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(546, 482);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // pbCamera
            // 
            this.pbCamera.BackColor = System.Drawing.Color.Black;
            this.pbCamera.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbCamera.Location = new System.Drawing.Point(3, 123);
            this.pbCamera.Name = "pbCamera";
            this.pbCamera.Size = new System.Drawing.Size(540, 356);
            this.pbCamera.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbCamera.TabIndex = 0;
            this.pbCamera.TabStop = false;
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
            this.tableLayoutPanel2.Size = new System.Drawing.Size(540, 114);
            this.tableLayoutPanel2.TabIndex = 1;
            // 
            // btnUp
            // 
            this.btnUp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnUp.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnUp.Location = new System.Drawing.Point(183, 3);
            this.btnUp.Name = "btnUp";
            this.btnUp.Size = new System.Drawing.Size(174, 51);
            this.btnUp.TabIndex = 1;
            this.btnUp.Text = "Up";
            this.btnUp.Click += new System.EventHandler(this.btnUp_Click);
            // 
            // btnDown
            // 
            this.btnDown.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnDown.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnDown.Location = new System.Drawing.Point(183, 60);
            this.btnDown.Name = "btnDown";
            this.btnDown.Size = new System.Drawing.Size(174, 51);
            this.btnDown.TabIndex = 2;
            this.btnDown.Text = "Down";
            this.btnDown.Click += new System.EventHandler(this.btnDown_Click);
            // 
            // btnLeft
            // 
            this.btnLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnLeft.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnLeft.Location = new System.Drawing.Point(3, 60);
            this.btnLeft.Name = "btnLeft";
            this.btnLeft.Size = new System.Drawing.Size(174, 51);
            this.btnLeft.TabIndex = 3;
            this.btnLeft.Text = "Left";
            this.btnLeft.Click += new System.EventHandler(this.btnLeft_Click);
            // 
            // btnRight
            // 
            this.btnRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnRight.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnRight.Location = new System.Drawing.Point(363, 60);
            this.btnRight.Name = "btnRight";
            this.btnRight.Size = new System.Drawing.Size(174, 51);
            this.btnRight.TabIndex = 4;
            this.btnRight.Text = "Right";
            this.btnRight.Click += new System.EventHandler(this.btnRight_Click);
            // 
            // FormMapMatchManual
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1143, 921);
            this.Controls.Add(this.tlpRoot);
            this.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MinimizeBox = false;
            this.Name = "FormMapMatchManual";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Map Match.";
            this.tlpRoot.ResumeLayout(false);
            this.tlpGrid.ResumeLayout(false);
            this.pnlDownload.ResumeLayout(false);
            this.tlpDown.ResumeLayout(false);
            this.pnlScan.ResumeLayout(false);
            this.tlpScan.ResumeLayout(false);
            this.pnlManual.ResumeLayout(false);
            this.tlpManual.ResumeLayout(false);
            this.gbTransform.ResumeLayout(false);
            this.tlpTf.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.nudDx)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudDy)).EndInit();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.pnlClear.ResumeLayout(false);
            this.pnlCamera.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbCamera)).EndInit();
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
        private System.Windows.Forms.Label lblMatchRate;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.TextBox textBoxUserID;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Button btnSelectLogin;
    }
}