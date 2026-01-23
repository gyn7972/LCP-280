using System.Windows.Forms;

namespace QMC.LCP_280.Process.Unit
{
    partial class Gem_Setup
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.TableLayoutPanel tlpRoot;
        private System.Windows.Forms.GroupBox gbConfig;
        private System.Windows.Forms.GroupBox gbTest;
        private System.Windows.Forms.GroupBox gbLog;

        private System.Windows.Forms.TableLayoutPanel tlpConfig;
        private System.Windows.Forms.CheckBox chkEnable;
        private System.Windows.Forms.ComboBox cmbMode;
        private System.Windows.Forms.TextBox txtIp;
        private System.Windows.Forms.NumericUpDown nudPort;
        private System.Windows.Forms.NumericUpDown nudDevId;
        private System.Windows.Forms.TextBox txtModelName;
        private System.Windows.Forms.TextBox txtSoftRev;

        private System.Windows.Forms.NumericUpDown nudT3;
        private System.Windows.Forms.NumericUpDown nudT5;
        private System.Windows.Forms.NumericUpDown nudT6;
        private System.Windows.Forms.NumericUpDown nudT7;
        private System.Windows.Forms.NumericUpDown nudT8;
        private System.Windows.Forms.NumericUpDown nudLinkTest;
        private System.Windows.Forms.NumericUpDown nudEstablish;
        private System.Windows.Forms.ComboBox cmbTimeFormat; // 0/1/2

        private System.Windows.Forms.CheckBox chkLogEnabled;
        private System.Windows.Forms.TextBox txtLogPath;
        private System.Windows.Forms.TextBox txtLogPrefix;
        private System.Windows.Forms.NumericUpDown nudLogKeepDays;
        private System.Windows.Forms.Button btnBrowseLogPath;

        private System.Windows.Forms.FlowLayoutPanel flpConfigButtons;
        private System.Windows.Forms.Button btnLoadConfig;
        private System.Windows.Forms.Button btnSaveConfig;
        private System.Windows.Forms.Button btnApplyConfig;

        private System.Windows.Forms.TableLayoutPanel tlpTest;
        private System.Windows.Forms.FlowLayoutPanel flpConnButtons;
        private System.Windows.Forms.Button btnCreate;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnOffline;
        private System.Windows.Forms.Button btnOnlineLocal;
        private System.Windows.Forms.Button btnOnlineRemote;

        private System.Windows.Forms.FlowLayoutPanel flpDefButtons;
        private System.Windows.Forms.TextBox txtSecsDir;
        private System.Windows.Forms.Button btnBrowseSecsDir;
        private System.Windows.Forms.Button btnInitDefinitions;

        private System.Windows.Forms.FlowLayoutPanel flpTestOps;
        private System.Windows.Forms.NumericUpDown nudCeid;
        private System.Windows.Forms.Button btnSendCeid;
        private System.Windows.Forms.NumericUpDown nudSvid;
        private System.Windows.Forms.TextBox txtSvidValue;
        private System.Windows.Forms.Button btnSetSvid;
        private System.Windows.Forms.CheckBox chkDiagnostics;

        private System.Windows.Forms.TextBox txtLog;

        // ===== Labels (디자이너 호환을 위해 전부 필드로 선언) =====
        private System.Windows.Forms.Label lblMode;
        private System.Windows.Forms.Label lblIp;
        private System.Windows.Forms.Label lblPort;
        private System.Windows.Forms.Label lblDevId;
        private System.Windows.Forms.Label lblMdln;
        private System.Windows.Forms.Label lblSoftRev;
        private System.Windows.Forms.Label lblTimeFormat;
        private System.Windows.Forms.Label lblT3;
        private System.Windows.Forms.Label lblT5;
        private System.Windows.Forms.Label lblT6;
        private System.Windows.Forms.Label lblT7;
        private System.Windows.Forms.Label lblT8;
        private System.Windows.Forms.Label lblLinkTest;
        private System.Windows.Forms.Label lblEstablish;
        private System.Windows.Forms.Label lblLogPath;
        private System.Windows.Forms.Label lblLogPrefix;
        private System.Windows.Forms.Label lblLogKeepDays;

        private System.Windows.Forms.FlowLayoutPanel pnlLogPath;
        private System.Windows.Forms.Label lblCeid;
        private System.Windows.Forms.Label lblSvid;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tlpRoot = new System.Windows.Forms.TableLayoutPanel();
            this.gbLog = new System.Windows.Forms.GroupBox();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.gbConfig = new System.Windows.Forms.GroupBox();
            this.tlpConfig = new System.Windows.Forms.TableLayoutPanel();
            this.chkEnable = new System.Windows.Forms.CheckBox();
            this.lblMode = new System.Windows.Forms.Label();
            this.cmbMode = new System.Windows.Forms.ComboBox();
            this.lblIp = new System.Windows.Forms.Label();
            this.txtIp = new System.Windows.Forms.TextBox();
            this.lblPort = new System.Windows.Forms.Label();
            this.nudPort = new System.Windows.Forms.NumericUpDown();
            this.lblDevId = new System.Windows.Forms.Label();
            this.nudDevId = new System.Windows.Forms.NumericUpDown();
            this.lblMdln = new System.Windows.Forms.Label();
            this.txtModelName = new System.Windows.Forms.TextBox();
            this.lblSoftRev = new System.Windows.Forms.Label();
            this.txtSoftRev = new System.Windows.Forms.TextBox();
            this.lblTimeFormat = new System.Windows.Forms.Label();
            this.cmbTimeFormat = new System.Windows.Forms.ComboBox();
            this.lblT3 = new System.Windows.Forms.Label();
            this.nudT3 = new System.Windows.Forms.NumericUpDown();
            this.lblT5 = new System.Windows.Forms.Label();
            this.nudT5 = new System.Windows.Forms.NumericUpDown();
            this.lblT6 = new System.Windows.Forms.Label();
            this.nudT6 = new System.Windows.Forms.NumericUpDown();
            this.lblT7 = new System.Windows.Forms.Label();
            this.nudT7 = new System.Windows.Forms.NumericUpDown();
            this.lblT8 = new System.Windows.Forms.Label();
            this.nudT8 = new System.Windows.Forms.NumericUpDown();
            this.lblLinkTest = new System.Windows.Forms.Label();
            this.nudLinkTest = new System.Windows.Forms.NumericUpDown();
            this.lblEstablish = new System.Windows.Forms.Label();
            this.nudEstablish = new System.Windows.Forms.NumericUpDown();
            this.chkLogEnabled = new System.Windows.Forms.CheckBox();
            this.lblLogKeepDays = new System.Windows.Forms.Label();
            this.nudLogKeepDays = new System.Windows.Forms.NumericUpDown();
            this.lblLogPath = new System.Windows.Forms.Label();
            this.pnlLogPath = new System.Windows.Forms.FlowLayoutPanel();
            this.txtLogPath = new System.Windows.Forms.TextBox();
            this.btnBrowseLogPath = new System.Windows.Forms.Button();
            this.lblLogPrefix = new System.Windows.Forms.Label();
            this.txtLogPrefix = new System.Windows.Forms.TextBox();
            this.flpConfigButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.btnApplyConfig = new System.Windows.Forms.Button();
            this.btnSaveConfig = new System.Windows.Forms.Button();
            this.btnLoadConfig = new System.Windows.Forms.Button();
            this.gbTest = new System.Windows.Forms.GroupBox();
            this.tlpTest = new System.Windows.Forms.TableLayoutPanel();
            this.flpConnButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.btnOnlineRemote = new System.Windows.Forms.Button();
            this.btnOnlineLocal = new System.Windows.Forms.Button();
            this.btnOffline = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnCreate = new System.Windows.Forms.Button();
            this.flpTestOps = new System.Windows.Forms.FlowLayoutPanel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.lblCeid = new System.Windows.Forms.Label();
            this.chkDiagnostics = new System.Windows.Forms.CheckBox();
            this.nudSvid = new System.Windows.Forms.NumericUpDown();
            this.lblSvid = new System.Windows.Forms.Label();
            this.btnSendCeid = new System.Windows.Forms.Button();
            this.nudCeid = new System.Windows.Forms.NumericUpDown();
            this.txtSvidValue = new System.Windows.Forms.TextBox();
            this.btnSetSvid = new System.Windows.Forms.Button();
            this.flpDefButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.btnInitDefinitions = new System.Windows.Forms.Button();
            this.btnBrowseSecsDir = new System.Windows.Forms.Button();
            this.txtSecsDir = new System.Windows.Forms.TextBox();
            this.btnGEMDlg = new System.Windows.Forms.Button();
            this.tlpRoot.SuspendLayout();
            this.gbLog.SuspendLayout();
            this.gbConfig.SuspendLayout();
            this.tlpConfig.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudPort)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudDevId)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudT3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudT5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudT6)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudT7)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudT8)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudLinkTest)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudEstablish)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudLogKeepDays)).BeginInit();
            this.pnlLogPath.SuspendLayout();
            this.flpConfigButtons.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.gbTest.SuspendLayout();
            this.tlpTest.SuspendLayout();
            this.flpConnButtons.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.flpTestOps.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudSvid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCeid)).BeginInit();
            this.flpDefButtons.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpRoot
            // 
            this.tlpRoot.ColumnCount = 1;
            this.tlpRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRoot.Controls.Add(this.gbLog, 0, 2);
            this.tlpRoot.Controls.Add(this.gbConfig, 0, 0);
            this.tlpRoot.Controls.Add(this.gbTest, 0, 1);
            this.tlpRoot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpRoot.Location = new System.Drawing.Point(0, 0);
            this.tlpRoot.Name = "tlpRoot";
            this.tlpRoot.RowCount = 3;
            this.tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 355F));
            this.tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 296F));
            this.tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRoot.Size = new System.Drawing.Size(1171, 1028);
            this.tlpRoot.TabIndex = 0;
            // 
            // gbLog
            // 
            this.gbLog.Controls.Add(this.txtLog);
            this.gbLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbLog.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold);
            this.gbLog.Location = new System.Drawing.Point(3, 654);
            this.gbLog.Name = "gbLog";
            this.gbLog.Size = new System.Drawing.Size(1165, 371);
            this.gbLog.TabIndex = 2;
            this.gbLog.TabStop = false;
            this.gbLog.Text = "Log";
            // 
            // txtLog
            // 
            this.txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLog.Location = new System.Drawing.Point(3, 30);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtLog.Size = new System.Drawing.Size(1159, 338);
            this.txtLog.TabIndex = 0;
            // 
            // gbConfig
            // 
            this.gbConfig.Controls.Add(this.tlpConfig);
            this.gbConfig.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbConfig.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold);
            this.gbConfig.Location = new System.Drawing.Point(3, 3);
            this.gbConfig.Name = "gbConfig";
            this.gbConfig.Size = new System.Drawing.Size(1165, 349);
            this.gbConfig.TabIndex = 0;
            this.gbConfig.TabStop = false;
            this.gbConfig.Text = "Config";
            // 
            // tlpConfig
            // 
            this.tlpConfig.ColumnCount = 6;
            this.tlpConfig.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 11.67691F));
            this.tlpConfig.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 21.5101F));
            this.tlpConfig.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.92537F));
            this.tlpConfig.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 13.34504F));
            this.tlpConfig.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.63748F));
            this.tlpConfig.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 22.41681F));
            this.tlpConfig.Controls.Add(this.chkEnable, 0, 0);
            this.tlpConfig.Controls.Add(this.lblMode, 2, 0);
            this.tlpConfig.Controls.Add(this.cmbMode, 3, 0);
            this.tlpConfig.Controls.Add(this.lblIp, 4, 0);
            this.tlpConfig.Controls.Add(this.txtIp, 5, 0);
            this.tlpConfig.Controls.Add(this.lblPort, 0, 1);
            this.tlpConfig.Controls.Add(this.nudPort, 1, 1);
            this.tlpConfig.Controls.Add(this.lblDevId, 2, 1);
            this.tlpConfig.Controls.Add(this.nudDevId, 3, 1);
            this.tlpConfig.Controls.Add(this.lblMdln, 4, 1);
            this.tlpConfig.Controls.Add(this.txtModelName, 5, 1);
            this.tlpConfig.Controls.Add(this.lblSoftRev, 0, 2);
            this.tlpConfig.Controls.Add(this.txtSoftRev, 1, 2);
            this.tlpConfig.Controls.Add(this.lblTimeFormat, 2, 2);
            this.tlpConfig.Controls.Add(this.cmbTimeFormat, 3, 2);
            this.tlpConfig.Controls.Add(this.lblT3, 0, 3);
            this.tlpConfig.Controls.Add(this.nudT3, 1, 3);
            this.tlpConfig.Controls.Add(this.lblT5, 2, 3);
            this.tlpConfig.Controls.Add(this.nudT5, 3, 3);
            this.tlpConfig.Controls.Add(this.lblT6, 4, 3);
            this.tlpConfig.Controls.Add(this.nudT6, 5, 3);
            this.tlpConfig.Controls.Add(this.lblT7, 0, 4);
            this.tlpConfig.Controls.Add(this.nudT7, 1, 4);
            this.tlpConfig.Controls.Add(this.lblT8, 2, 4);
            this.tlpConfig.Controls.Add(this.nudT8, 3, 4);
            this.tlpConfig.Controls.Add(this.lblLinkTest, 4, 4);
            this.tlpConfig.Controls.Add(this.nudLinkTest, 5, 4);
            this.tlpConfig.Controls.Add(this.lblEstablish, 0, 5);
            this.tlpConfig.Controls.Add(this.nudEstablish, 1, 5);
            this.tlpConfig.Controls.Add(this.chkLogEnabled, 2, 5);
            this.tlpConfig.Controls.Add(this.lblLogKeepDays, 4, 5);
            this.tlpConfig.Controls.Add(this.nudLogKeepDays, 5, 5);
            this.tlpConfig.Controls.Add(this.lblLogPath, 0, 6);
            this.tlpConfig.Controls.Add(this.pnlLogPath, 1, 6);
            this.tlpConfig.Controls.Add(this.lblLogPrefix, 4, 6);
            this.tlpConfig.Controls.Add(this.txtLogPrefix, 5, 6);
            this.tlpConfig.Controls.Add(this.flpConfigButtons, 0, 7);
            this.tlpConfig.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpConfig.Location = new System.Drawing.Point(3, 30);
            this.tlpConfig.Name = "tlpConfig";
            this.tlpConfig.RowCount = 8;
            this.tlpConfig.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tlpConfig.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tlpConfig.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tlpConfig.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tlpConfig.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tlpConfig.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tlpConfig.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tlpConfig.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tlpConfig.Size = new System.Drawing.Size(1159, 316);
            this.tlpConfig.TabIndex = 0;
            // 
            // chkEnable
            // 
            this.chkEnable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkEnable.Location = new System.Drawing.Point(3, 3);
            this.chkEnable.Name = "chkEnable";
            this.chkEnable.Size = new System.Drawing.Size(128, 33);
            this.chkEnable.TabIndex = 0;
            this.chkEnable.Text = "Enable";
            // 
            // lblMode
            // 
            this.lblMode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMode.Location = new System.Drawing.Point(385, 0);
            this.lblMode.Name = "lblMode";
            this.lblMode.Size = new System.Drawing.Size(166, 39);
            this.lblMode.TabIndex = 1;
            this.lblMode.Text = "Mode";
            this.lblMode.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cmbMode
            // 
            this.cmbMode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmbMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbMode.Items.AddRange(new object[] {
            "Active",
            "Passive"});
            this.cmbMode.Location = new System.Drawing.Point(557, 3);
            this.cmbMode.Name = "cmbMode";
            this.cmbMode.Size = new System.Drawing.Size(147, 36);
            this.cmbMode.TabIndex = 2;
            // 
            // lblIp
            // 
            this.lblIp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblIp.Location = new System.Drawing.Point(710, 0);
            this.lblIp.Name = "lblIp";
            this.lblIp.Size = new System.Drawing.Size(185, 39);
            this.lblIp.TabIndex = 3;
            this.lblIp.Text = "IP";
            this.lblIp.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtIp
            // 
            this.txtIp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtIp.Location = new System.Drawing.Point(901, 3);
            this.txtIp.Name = "txtIp";
            this.txtIp.Size = new System.Drawing.Size(255, 34);
            this.txtIp.TabIndex = 4;
            // 
            // lblPort
            // 
            this.lblPort.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPort.Location = new System.Drawing.Point(3, 39);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(128, 39);
            this.lblPort.TabIndex = 5;
            this.lblPort.Text = "Port";
            this.lblPort.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudPort
            // 
            this.nudPort.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nudPort.Location = new System.Drawing.Point(137, 42);
            this.nudPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.nudPort.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudPort.Name = "nudPort";
            this.nudPort.Size = new System.Drawing.Size(242, 34);
            this.nudPort.TabIndex = 6;
            this.nudPort.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // lblDevId
            // 
            this.lblDevId.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDevId.Location = new System.Drawing.Point(385, 39);
            this.lblDevId.Name = "lblDevId";
            this.lblDevId.Size = new System.Drawing.Size(166, 39);
            this.lblDevId.TabIndex = 7;
            this.lblDevId.Text = "DevId";
            this.lblDevId.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudDevId
            // 
            this.nudDevId.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nudDevId.Location = new System.Drawing.Point(557, 42);
            this.nudDevId.Maximum = new decimal(new int[] {
            32767,
            0,
            0,
            0});
            this.nudDevId.Name = "nudDevId";
            this.nudDevId.Size = new System.Drawing.Size(147, 34);
            this.nudDevId.TabIndex = 8;
            // 
            // lblMdln
            // 
            this.lblMdln.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMdln.Location = new System.Drawing.Point(710, 39);
            this.lblMdln.Name = "lblMdln";
            this.lblMdln.Size = new System.Drawing.Size(185, 39);
            this.lblMdln.TabIndex = 9;
            this.lblMdln.Text = "MDLN";
            this.lblMdln.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtModelName
            // 
            this.txtModelName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtModelName.Location = new System.Drawing.Point(901, 42);
            this.txtModelName.Name = "txtModelName";
            this.txtModelName.Size = new System.Drawing.Size(255, 34);
            this.txtModelName.TabIndex = 10;
            // 
            // lblSoftRev
            // 
            this.lblSoftRev.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSoftRev.Location = new System.Drawing.Point(3, 78);
            this.lblSoftRev.Name = "lblSoftRev";
            this.lblSoftRev.Size = new System.Drawing.Size(128, 39);
            this.lblSoftRev.TabIndex = 11;
            this.lblSoftRev.Text = "SOFTREV";
            this.lblSoftRev.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtSoftRev
            // 
            this.txtSoftRev.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtSoftRev.Location = new System.Drawing.Point(137, 81);
            this.txtSoftRev.Name = "txtSoftRev";
            this.txtSoftRev.Size = new System.Drawing.Size(242, 34);
            this.txtSoftRev.TabIndex = 12;
            // 
            // lblTimeFormat
            // 
            this.lblTimeFormat.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTimeFormat.Location = new System.Drawing.Point(385, 78);
            this.lblTimeFormat.Name = "lblTimeFormat";
            this.lblTimeFormat.Size = new System.Drawing.Size(166, 39);
            this.lblTimeFormat.TabIndex = 13;
            this.lblTimeFormat.Text = "TimeFormat";
            this.lblTimeFormat.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cmbTimeFormat
            // 
            this.cmbTimeFormat.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmbTimeFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTimeFormat.Items.AddRange(new object[] {
            "0 (12)",
            "1 (16)",
            "2 (14)"});
            this.cmbTimeFormat.Location = new System.Drawing.Point(557, 81);
            this.cmbTimeFormat.Name = "cmbTimeFormat";
            this.cmbTimeFormat.Size = new System.Drawing.Size(147, 36);
            this.cmbTimeFormat.TabIndex = 14;
            // 
            // lblT3
            // 
            this.lblT3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblT3.Location = new System.Drawing.Point(3, 117);
            this.lblT3.Name = "lblT3";
            this.lblT3.Size = new System.Drawing.Size(128, 39);
            this.lblT3.TabIndex = 15;
            this.lblT3.Text = "T3";
            this.lblT3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudT3
            // 
            this.nudT3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nudT3.Location = new System.Drawing.Point(137, 120);
            this.nudT3.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.nudT3.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudT3.Name = "nudT3";
            this.nudT3.Size = new System.Drawing.Size(242, 34);
            this.nudT3.TabIndex = 16;
            this.nudT3.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // lblT5
            // 
            this.lblT5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblT5.Location = new System.Drawing.Point(385, 117);
            this.lblT5.Name = "lblT5";
            this.lblT5.Size = new System.Drawing.Size(166, 39);
            this.lblT5.TabIndex = 17;
            this.lblT5.Text = "T5";
            this.lblT5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudT5
            // 
            this.nudT5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nudT5.Location = new System.Drawing.Point(557, 120);
            this.nudT5.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.nudT5.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudT5.Name = "nudT5";
            this.nudT5.Size = new System.Drawing.Size(147, 34);
            this.nudT5.TabIndex = 18;
            this.nudT5.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // lblT6
            // 
            this.lblT6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblT6.Location = new System.Drawing.Point(710, 117);
            this.lblT6.Name = "lblT6";
            this.lblT6.Size = new System.Drawing.Size(185, 39);
            this.lblT6.TabIndex = 19;
            this.lblT6.Text = "T6";
            this.lblT6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudT6
            // 
            this.nudT6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nudT6.Location = new System.Drawing.Point(901, 120);
            this.nudT6.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.nudT6.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudT6.Name = "nudT6";
            this.nudT6.Size = new System.Drawing.Size(255, 34);
            this.nudT6.TabIndex = 20;
            this.nudT6.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // lblT7
            // 
            this.lblT7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblT7.Location = new System.Drawing.Point(3, 156);
            this.lblT7.Name = "lblT7";
            this.lblT7.Size = new System.Drawing.Size(128, 39);
            this.lblT7.TabIndex = 21;
            this.lblT7.Text = "T7";
            this.lblT7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudT7
            // 
            this.nudT7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nudT7.Location = new System.Drawing.Point(137, 159);
            this.nudT7.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.nudT7.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudT7.Name = "nudT7";
            this.nudT7.Size = new System.Drawing.Size(242, 34);
            this.nudT7.TabIndex = 22;
            this.nudT7.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // lblT8
            // 
            this.lblT8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblT8.Location = new System.Drawing.Point(385, 156);
            this.lblT8.Name = "lblT8";
            this.lblT8.Size = new System.Drawing.Size(166, 39);
            this.lblT8.TabIndex = 23;
            this.lblT8.Text = "T8";
            this.lblT8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudT8
            // 
            this.nudT8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nudT8.Location = new System.Drawing.Point(557, 159);
            this.nudT8.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.nudT8.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudT8.Name = "nudT8";
            this.nudT8.Size = new System.Drawing.Size(147, 34);
            this.nudT8.TabIndex = 24;
            this.nudT8.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // lblLinkTest
            // 
            this.lblLinkTest.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblLinkTest.Location = new System.Drawing.Point(710, 156);
            this.lblLinkTest.Name = "lblLinkTest";
            this.lblLinkTest.Size = new System.Drawing.Size(185, 39);
            this.lblLinkTest.TabIndex = 25;
            this.lblLinkTest.Text = "LinkTest";
            this.lblLinkTest.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudLinkTest
            // 
            this.nudLinkTest.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nudLinkTest.Location = new System.Drawing.Point(901, 159);
            this.nudLinkTest.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.nudLinkTest.Name = "nudLinkTest";
            this.nudLinkTest.Size = new System.Drawing.Size(255, 34);
            this.nudLinkTest.TabIndex = 26;
            // 
            // lblEstablish
            // 
            this.lblEstablish.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblEstablish.Location = new System.Drawing.Point(3, 195);
            this.lblEstablish.Name = "lblEstablish";
            this.lblEstablish.Size = new System.Drawing.Size(128, 39);
            this.lblEstablish.TabIndex = 27;
            this.lblEstablish.Text = "Establish";
            this.lblEstablish.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudEstablish
            // 
            this.nudEstablish.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nudEstablish.Location = new System.Drawing.Point(137, 198);
            this.nudEstablish.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.nudEstablish.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudEstablish.Name = "nudEstablish";
            this.nudEstablish.Size = new System.Drawing.Size(242, 34);
            this.nudEstablish.TabIndex = 28;
            this.nudEstablish.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // chkLogEnabled
            // 
            this.chkLogEnabled.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkLogEnabled.Location = new System.Drawing.Point(385, 198);
            this.chkLogEnabled.Name = "chkLogEnabled";
            this.chkLogEnabled.Size = new System.Drawing.Size(166, 33);
            this.chkLogEnabled.TabIndex = 29;
            this.chkLogEnabled.Text = "Log Enabled";
            // 
            // lblLogKeepDays
            // 
            this.lblLogKeepDays.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblLogKeepDays.Location = new System.Drawing.Point(710, 195);
            this.lblLogKeepDays.Name = "lblLogKeepDays";
            this.lblLogKeepDays.Size = new System.Drawing.Size(185, 39);
            this.lblLogKeepDays.TabIndex = 30;
            this.lblLogKeepDays.Text = "LogKeepDays";
            this.lblLogKeepDays.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudLogKeepDays
            // 
            this.nudLogKeepDays.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nudLogKeepDays.Location = new System.Drawing.Point(901, 198);
            this.nudLogKeepDays.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.nudLogKeepDays.Name = "nudLogKeepDays";
            this.nudLogKeepDays.Size = new System.Drawing.Size(255, 34);
            this.nudLogKeepDays.TabIndex = 31;
            // 
            // lblLogPath
            // 
            this.lblLogPath.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblLogPath.Location = new System.Drawing.Point(3, 234);
            this.lblLogPath.Name = "lblLogPath";
            this.lblLogPath.Size = new System.Drawing.Size(128, 39);
            this.lblLogPath.TabIndex = 32;
            this.lblLogPath.Text = "LogPath";
            this.lblLogPath.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlLogPath
            // 
            this.tlpConfig.SetColumnSpan(this.pnlLogPath, 3);
            this.pnlLogPath.Controls.Add(this.txtLogPath);
            this.pnlLogPath.Controls.Add(this.btnBrowseLogPath);
            this.pnlLogPath.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlLogPath.Location = new System.Drawing.Point(137, 237);
            this.pnlLogPath.Name = "pnlLogPath";
            this.pnlLogPath.Size = new System.Drawing.Size(567, 33);
            this.pnlLogPath.TabIndex = 33;
            this.pnlLogPath.WrapContents = false;
            // 
            // txtLogPath
            // 
            this.txtLogPath.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLogPath.Location = new System.Drawing.Point(3, 3);
            this.txtLogPath.Name = "txtLogPath";
            this.txtLogPath.Size = new System.Drawing.Size(260, 34);
            this.txtLogPath.TabIndex = 0;
            // 
            // btnBrowseLogPath
            // 
            this.btnBrowseLogPath.Location = new System.Drawing.Point(269, 3);
            this.btnBrowseLogPath.Name = "btnBrowseLogPath";
            this.btnBrowseLogPath.Size = new System.Drawing.Size(69, 34);
            this.btnBrowseLogPath.TabIndex = 1;
            this.btnBrowseLogPath.Text = "...";
            // 
            // lblLogPrefix
            // 
            this.lblLogPrefix.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblLogPrefix.Location = new System.Drawing.Point(710, 234);
            this.lblLogPrefix.Name = "lblLogPrefix";
            this.lblLogPrefix.Size = new System.Drawing.Size(185, 39);
            this.lblLogPrefix.TabIndex = 34;
            this.lblLogPrefix.Text = "LogPrefix";
            this.lblLogPrefix.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtLogPrefix
            // 
            this.txtLogPrefix.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLogPrefix.Location = new System.Drawing.Point(901, 237);
            this.txtLogPrefix.Name = "txtLogPrefix";
            this.txtLogPrefix.Size = new System.Drawing.Size(255, 34);
            this.txtLogPrefix.TabIndex = 35;
            // 
            // flpConfigButtons
            // 
            this.tlpConfig.SetColumnSpan(this.flpConfigButtons, 6);
            this.flpConfigButtons.Controls.Add(this.tableLayoutPanel4);
            this.flpConfigButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.flpConfigButtons.Location = new System.Drawing.Point(3, 276);
            this.flpConfigButtons.Name = "flpConfigButtons";
            this.flpConfigButtons.Size = new System.Drawing.Size(1153, 37);
            this.flpConfigButtons.TabIndex = 36;
            this.flpConfigButtons.WrapContents = false;
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 3;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel4.Controls.Add(this.btnApplyConfig, 2, 0);
            this.tableLayoutPanel4.Controls.Add(this.btnSaveConfig, 1, 0);
            this.tableLayoutPanel4.Controls.Add(this.btnLoadConfig, 0, 0);
            this.tableLayoutPanel4.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 1;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(1073, 46);
            this.tableLayoutPanel4.TabIndex = 7;
            // 
            // btnApplyConfig
            // 
            this.btnApplyConfig.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnApplyConfig.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold);
            this.btnApplyConfig.Location = new System.Drawing.Point(717, 3);
            this.btnApplyConfig.Name = "btnApplyConfig";
            this.btnApplyConfig.Size = new System.Drawing.Size(353, 40);
            this.btnApplyConfig.TabIndex = 2;
            this.btnApplyConfig.Text = "Apply";
            // 
            // btnSaveConfig
            // 
            this.btnSaveConfig.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnSaveConfig.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold);
            this.btnSaveConfig.Location = new System.Drawing.Point(360, 3);
            this.btnSaveConfig.Name = "btnSaveConfig";
            this.btnSaveConfig.Size = new System.Drawing.Size(351, 40);
            this.btnSaveConfig.TabIndex = 1;
            this.btnSaveConfig.Text = "Save";
            // 
            // btnLoadConfig
            // 
            this.btnLoadConfig.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnLoadConfig.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold);
            this.btnLoadConfig.Location = new System.Drawing.Point(3, 3);
            this.btnLoadConfig.Name = "btnLoadConfig";
            this.btnLoadConfig.Size = new System.Drawing.Size(351, 40);
            this.btnLoadConfig.TabIndex = 0;
            this.btnLoadConfig.Text = "Load";
            // 
            // gbTest
            // 
            this.gbTest.Controls.Add(this.tlpTest);
            this.gbTest.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbTest.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold);
            this.gbTest.Location = new System.Drawing.Point(3, 358);
            this.gbTest.Name = "gbTest";
            this.gbTest.Size = new System.Drawing.Size(1165, 290);
            this.gbTest.TabIndex = 1;
            this.gbTest.TabStop = false;
            this.gbTest.Text = "Test";
            // 
            // tlpTest
            // 
            this.tlpTest.ColumnCount = 1;
            this.tlpTest.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpTest.Controls.Add(this.flpConnButtons, 0, 0);
            this.tlpTest.Controls.Add(this.flpTestOps, 0, 2);
            this.tlpTest.Controls.Add(this.flpDefButtons, 0, 1);
            this.tlpTest.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpTest.Location = new System.Drawing.Point(3, 30);
            this.tlpTest.Name = "tlpTest";
            this.tlpTest.RowCount = 3;
            this.tlpTest.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 39.29961F));
            this.tlpTest.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30.7393F));
            this.tlpTest.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 29.57199F));
            this.tlpTest.Size = new System.Drawing.Size(1159, 257);
            this.tlpTest.TabIndex = 0;
            // 
            // flpConnButtons
            // 
            this.flpConnButtons.Controls.Add(this.tableLayoutPanel3);
            this.flpConnButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpConnButtons.Location = new System.Drawing.Point(3, 3);
            this.flpConnButtons.Name = "flpConnButtons";
            this.flpConnButtons.Size = new System.Drawing.Size(1153, 95);
            this.flpConnButtons.TabIndex = 0;
            this.flpConnButtons.WrapContents = false;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 6;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel3.Controls.Add(this.btnOnlineRemote, 5, 0);
            this.tableLayoutPanel3.Controls.Add(this.btnOnlineLocal, 4, 0);
            this.tableLayoutPanel3.Controls.Add(this.btnOffline, 3, 0);
            this.tableLayoutPanel3.Controls.Add(this.btnStop, 2, 0);
            this.tableLayoutPanel3.Controls.Add(this.btnStart, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.btnCreate, 0, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 74F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(1099, 74);
            this.tableLayoutPanel3.TabIndex = 6;
            // 
            // btnOnlineRemote
            // 
            this.btnOnlineRemote.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnOnlineRemote.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold);
            this.btnOnlineRemote.Location = new System.Drawing.Point(918, 3);
            this.btnOnlineRemote.Name = "btnOnlineRemote";
            this.btnOnlineRemote.Size = new System.Drawing.Size(178, 68);
            this.btnOnlineRemote.TabIndex = 5;
            this.btnOnlineRemote.Text = "Online Remote";
            // 
            // btnOnlineLocal
            // 
            this.btnOnlineLocal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnOnlineLocal.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold);
            this.btnOnlineLocal.Location = new System.Drawing.Point(735, 3);
            this.btnOnlineLocal.Name = "btnOnlineLocal";
            this.btnOnlineLocal.Size = new System.Drawing.Size(177, 68);
            this.btnOnlineLocal.TabIndex = 4;
            this.btnOnlineLocal.Text = "Online Local";
            // 
            // btnOffline
            // 
            this.btnOffline.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnOffline.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold);
            this.btnOffline.Location = new System.Drawing.Point(552, 3);
            this.btnOffline.Name = "btnOffline";
            this.btnOffline.Size = new System.Drawing.Size(177, 68);
            this.btnOffline.TabIndex = 3;
            this.btnOffline.Text = "Offline";
            // 
            // btnStop
            // 
            this.btnStop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnStop.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold);
            this.btnStop.Location = new System.Drawing.Point(369, 3);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(177, 68);
            this.btnStop.TabIndex = 2;
            this.btnStop.Text = "Stop";
            // 
            // btnStart
            // 
            this.btnStart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnStart.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold);
            this.btnStart.Location = new System.Drawing.Point(186, 3);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(177, 68);
            this.btnStart.TabIndex = 1;
            this.btnStart.Text = "Start";
            // 
            // btnCreate
            // 
            this.btnCreate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnCreate.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold);
            this.btnCreate.Location = new System.Drawing.Point(3, 3);
            this.btnCreate.Name = "btnCreate";
            this.btnCreate.Size = new System.Drawing.Size(177, 68);
            this.btnCreate.TabIndex = 0;
            this.btnCreate.Text = "Create";
            // 
            // flpTestOps
            // 
            this.flpTestOps.Controls.Add(this.tableLayoutPanel1);
            this.flpTestOps.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpTestOps.Location = new System.Drawing.Point(3, 183);
            this.flpTestOps.Name = "flpTestOps";
            this.flpTestOps.Size = new System.Drawing.Size(1153, 71);
            this.flpTestOps.TabIndex = 2;
            this.flpTestOps.WrapContents = false;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 8;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.037736F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 9.433962F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12.07547F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 7.264151F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 9.528302F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 21.80518F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 11.08132F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 22.69884F));
            this.tableLayoutPanel1.Controls.Add(this.lblCeid, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.chkDiagnostics, 7, 0);
            this.tableLayoutPanel1.Controls.Add(this.nudSvid, 4, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblSvid, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnSendCeid, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.nudCeid, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.txtSvidValue, 5, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnSetSvid, 6, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 57F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1119, 57);
            this.tableLayoutPanel1.TabIndex = 8;
            // 
            // lblCeid
            // 
            this.lblCeid.AutoSize = true;
            this.lblCeid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCeid.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblCeid.Location = new System.Drawing.Point(3, 0);
            this.lblCeid.Name = "lblCeid";
            this.lblCeid.Padding = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.lblCeid.Size = new System.Drawing.Size(61, 57);
            this.lblCeid.TabIndex = 0;
            this.lblCeid.Text = "CEID";
            // 
            // chkDiagnostics
            // 
            this.chkDiagnostics.AutoSize = true;
            this.chkDiagnostics.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkDiagnostics.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.chkDiagnostics.Location = new System.Drawing.Point(865, 3);
            this.chkDiagnostics.Name = "chkDiagnostics";
            this.chkDiagnostics.Size = new System.Drawing.Size(251, 51);
            this.chkDiagnostics.TabIndex = 7;
            this.chkDiagnostics.Text = "Diagnostics";
            // 
            // nudSvid
            // 
            this.nudSvid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nudSvid.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.nudSvid.Location = new System.Drawing.Point(391, 3);
            this.nudSvid.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this.nudSvid.Name = "nudSvid";
            this.nudSvid.Size = new System.Drawing.Size(100, 34);
            this.nudSvid.TabIndex = 4;
            // 
            // lblSvid
            // 
            this.lblSvid.AutoSize = true;
            this.lblSvid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSvid.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblSvid.Location = new System.Drawing.Point(310, 0);
            this.lblSvid.Name = "lblSvid";
            this.lblSvid.Padding = new System.Windows.Forms.Padding(12, 8, 0, 0);
            this.lblSvid.Size = new System.Drawing.Size(75, 57);
            this.lblSvid.TabIndex = 3;
            this.lblSvid.Text = "SVID";
            // 
            // btnSendCeid
            // 
            this.btnSendCeid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnSendCeid.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnSendCeid.Location = new System.Drawing.Point(175, 3);
            this.btnSendCeid.Name = "btnSendCeid";
            this.btnSendCeid.Size = new System.Drawing.Size(129, 51);
            this.btnSendCeid.TabIndex = 2;
            this.btnSendCeid.Text = "Send CEID";
            // 
            // nudCeid
            // 
            this.nudCeid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nudCeid.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.nudCeid.Location = new System.Drawing.Point(70, 3);
            this.nudCeid.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this.nudCeid.Name = "nudCeid";
            this.nudCeid.Size = new System.Drawing.Size(99, 34);
            this.nudCeid.TabIndex = 1;
            // 
            // txtSvidValue
            // 
            this.txtSvidValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtSvidValue.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.txtSvidValue.Location = new System.Drawing.Point(497, 3);
            this.txtSvidValue.Name = "txtSvidValue";
            this.txtSvidValue.Size = new System.Drawing.Size(238, 34);
            this.txtSvidValue.TabIndex = 5;
            // 
            // btnSetSvid
            // 
            this.btnSetSvid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnSetSvid.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnSetSvid.Location = new System.Drawing.Point(741, 3);
            this.btnSetSvid.Name = "btnSetSvid";
            this.btnSetSvid.Size = new System.Drawing.Size(118, 51);
            this.btnSetSvid.TabIndex = 6;
            this.btnSetSvid.Text = "Set SVID";
            // 
            // flpDefButtons
            // 
            this.flpDefButtons.Controls.Add(this.tableLayoutPanel2);
            this.flpDefButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpDefButtons.Location = new System.Drawing.Point(3, 104);
            this.flpDefButtons.Name = "flpDefButtons";
            this.flpDefButtons.Size = new System.Drawing.Size(1153, 73);
            this.flpDefButtons.TabIndex = 1;
            this.flpDefButtons.WrapContents = false;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 7;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 56.32615F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 5.810684F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.11996F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 2.343018F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 2.530459F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 2.249297F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.52671F));
            this.tableLayoutPanel2.Controls.Add(this.btnInitDefinitions, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnBrowseSecsDir, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.txtSecsDir, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnGEMDlg, 6, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(1119, 62);
            this.tableLayoutPanel2.TabIndex = 3;
            // 
            // btnInitDefinitions
            // 
            this.btnInitDefinitions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnInitDefinitions.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold);
            this.btnInitDefinitions.Location = new System.Drawing.Point(698, 3);
            this.btnInitDefinitions.Name = "btnInitDefinitions";
            this.btnInitDefinitions.Size = new System.Drawing.Size(174, 56);
            this.btnInitDefinitions.TabIndex = 2;
            this.btnInitDefinitions.Text = "Init Definitions";
            // 
            // btnBrowseSecsDir
            // 
            this.btnBrowseSecsDir.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnBrowseSecsDir.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold);
            this.btnBrowseSecsDir.Location = new System.Drawing.Point(633, 3);
            this.btnBrowseSecsDir.Name = "btnBrowseSecsDir";
            this.btnBrowseSecsDir.Size = new System.Drawing.Size(59, 56);
            this.btnBrowseSecsDir.TabIndex = 1;
            this.btnBrowseSecsDir.Text = "...";
            // 
            // txtSecsDir
            // 
            this.txtSecsDir.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtSecsDir.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold);
            this.txtSecsDir.Location = new System.Drawing.Point(3, 3);
            this.txtSecsDir.Name = "txtSecsDir";
            this.txtSecsDir.Size = new System.Drawing.Size(624, 34);
            this.txtSecsDir.TabIndex = 0;
            // 
            // btnGEMDlg
            // 
            this.btnGEMDlg.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnGEMDlg.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold);
            this.btnGEMDlg.Location = new System.Drawing.Point(957, 3);
            this.btnGEMDlg.Name = "btnGEMDlg";
            this.btnGEMDlg.Size = new System.Drawing.Size(159, 56);
            this.btnGEMDlg.TabIndex = 3;
            this.btnGEMDlg.Text = "GEM Dlg";
            this.btnGEMDlg.Click += new System.EventHandler(this.btnGEMDlg_Click);
            // 
            // Gem_Setup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1171, 1028);
            this.Controls.Add(this.tlpRoot);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Gem_Setup";
            this.Text = "Gem_Setup";
            this.Load += new System.EventHandler(this.GEM_Setup_Load);
            this.tlpRoot.ResumeLayout(false);
            this.gbLog.ResumeLayout(false);
            this.gbLog.PerformLayout();
            this.gbConfig.ResumeLayout(false);
            this.tlpConfig.ResumeLayout(false);
            this.tlpConfig.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudPort)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudDevId)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudT3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudT5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudT6)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudT7)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudT8)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudLinkTest)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudEstablish)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudLogKeepDays)).EndInit();
            this.pnlLogPath.ResumeLayout(false);
            this.pnlLogPath.PerformLayout();
            this.flpConfigButtons.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.gbTest.ResumeLayout(false);
            this.tlpTest.ResumeLayout(false);
            this.flpConnButtons.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.flpTestOps.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudSvid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCeid)).EndInit();
            this.flpDefButtons.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel2;
        private TableLayoutPanel tableLayoutPanel3;
        private TableLayoutPanel tableLayoutPanel4;
        private Button btnGEMDlg;
    }
}