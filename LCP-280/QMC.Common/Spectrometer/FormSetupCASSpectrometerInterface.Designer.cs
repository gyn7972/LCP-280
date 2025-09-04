namespace QMC.Common.Spectrometer
{
    partial class FormSetupCASSpectrometerInterface
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lbxIntType = new System.Windows.Forms.ListBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel15 = new System.Windows.Forms.TableLayoutPanel();
            this.btnSelectCalibFile = new QMC.Common.IndividualMenuButton();
            this.btnSelectConfigFile = new QMC.Common.IndividualMenuButton();
            this.btnRefresh = new QMC.Common.IndividualMenuButton();
            this.btnApply = new QMC.Common.IndividualMenuButton();
            this.lbCalibFilePathValue = new QMC.Common.CustomControl.CustomBorderLabel();
            this.customBorderLabel7 = new QMC.Common.CustomControl.CustomBorderLabel();
            this.lbInterfaceOptionValue = new QMC.Common.CustomControl.CustomBorderLabel();
            this.lbConfigFilePathValue = new QMC.Common.CustomControl.CustomBorderLabel();
            this.customBorderLabel2 = new QMC.Common.CustomControl.CustomBorderLabel();
            this.customBorderLabel3 = new QMC.Common.CustomControl.CustomBorderLabel();
            this.customBorderLabel4 = new QMC.Common.CustomControl.CustomBorderLabel();
            this.lbInterfaceTypeValue = new QMC.Common.CustomControl.CustomBorderLabel();
            this.lbxIntOption = new System.Windows.Forms.ListBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.tableLayoutPanel15.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel2, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(10, 10);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 65.64417F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 34.35583F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(750, 489);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 3;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 152F));
            this.tableLayoutPanel2.Controls.Add(this.groupBox2, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.groupBox1, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel3, 2, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(744, 315);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lbxIntType);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(10);
            this.groupBox1.Size = new System.Drawing.Size(290, 309);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Interface Type";
            // 
            // lbxIntType
            // 
            this.lbxIntType.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbxIntType.FormattingEnabled = true;
            this.lbxIntType.ItemHeight = 17;
            this.lbxIntType.Location = new System.Drawing.Point(10, 28);
            this.lbxIntType.Name = "lbxIntType";
            this.lbxIntType.Size = new System.Drawing.Size(270, 271);
            this.lbxIntType.TabIndex = 0;
            this.lbxIntType.SelectedValueChanged += new System.EventHandler(this.lbxIntType_SelectedValueChanged);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.btnApply);
            this.panel2.Controls.Add(this.tableLayoutPanel15);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(3, 324);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(744, 162);
            this.panel2.TabIndex = 1;
            // 
            // tableLayoutPanel15
            // 
            this.tableLayoutPanel15.ColumnCount = 2;
            this.tableLayoutPanel15.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 19.08127F));
            this.tableLayoutPanel15.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 80.91873F));
            this.tableLayoutPanel15.Controls.Add(this.lbCalibFilePathValue, 1, 3);
            this.tableLayoutPanel15.Controls.Add(this.customBorderLabel7, 0, 3);
            this.tableLayoutPanel15.Controls.Add(this.lbInterfaceOptionValue, 1, 1);
            this.tableLayoutPanel15.Controls.Add(this.lbConfigFilePathValue, 1, 2);
            this.tableLayoutPanel15.Controls.Add(this.customBorderLabel2, 0, 2);
            this.tableLayoutPanel15.Controls.Add(this.customBorderLabel3, 0, 0);
            this.tableLayoutPanel15.Controls.Add(this.customBorderLabel4, 0, 1);
            this.tableLayoutPanel15.Controls.Add(this.lbInterfaceTypeValue, 1, 0);
            this.tableLayoutPanel15.Location = new System.Drawing.Point(13, 14);
            this.tableLayoutPanel15.Name = "tableLayoutPanel15";
            this.tableLayoutPanel15.RowCount = 4;
            this.tableLayoutPanel15.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel15.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel15.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel15.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel15.Size = new System.Drawing.Size(566, 135);
            this.tableLayoutPanel15.TabIndex = 22;
            // 
            // btnSelectCalibFile
            // 
            this.btnSelectCalibFile.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnSelectCalibFile.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnSelectCalibFile.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnSelectCalibFile.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnSelectCalibFile.CustomForeColor = System.Drawing.Color.Black;
            this.btnSelectCalibFile.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSelectCalibFile.ForeColor = System.Drawing.Color.Black;
            this.btnSelectCalibFile.ImageSize = new System.Drawing.Size(45, 45);
            this.btnSelectCalibFile.Location = new System.Drawing.Point(9, 80);
            this.btnSelectCalibFile.Name = "btnSelectCalibFile";
            this.btnSelectCalibFile.Size = new System.Drawing.Size(123, 42);
            this.btnSelectCalibFile.TabIndex = 26;
            this.btnSelectCalibFile.TabStop = false;
            this.btnSelectCalibFile.Text = "Select Calib File";
            this.btnSelectCalibFile.UseVisualStyleBackColor = false;
            this.btnSelectCalibFile.Click += new System.EventHandler(this.btnSelectCalibFile_Click);
            // 
            // btnSelectConfigFile
            // 
            this.btnSelectConfigFile.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnSelectConfigFile.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnSelectConfigFile.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnSelectConfigFile.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnSelectConfigFile.CustomForeColor = System.Drawing.Color.Black;
            this.btnSelectConfigFile.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSelectConfigFile.ForeColor = System.Drawing.Color.Black;
            this.btnSelectConfigFile.ImageSize = new System.Drawing.Size(45, 45);
            this.btnSelectConfigFile.Location = new System.Drawing.Point(9, 31);
            this.btnSelectConfigFile.Name = "btnSelectConfigFile";
            this.btnSelectConfigFile.Size = new System.Drawing.Size(123, 42);
            this.btnSelectConfigFile.TabIndex = 25;
            this.btnSelectConfigFile.TabStop = false;
            this.btnSelectConfigFile.Text = "Select Config File";
            this.btnSelectConfigFile.UseVisualStyleBackColor = false;
            this.btnSelectConfigFile.Click += new System.EventHandler(this.btnSelectConfigFile_Click);
            // 
            // btnRefresh
            // 
            this.btnRefresh.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnRefresh.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnRefresh.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnRefresh.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnRefresh.CustomForeColor = System.Drawing.Color.Black;
            this.btnRefresh.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRefresh.ForeColor = System.Drawing.Color.Black;
            this.btnRefresh.ImageSize = new System.Drawing.Size(45, 45);
            this.btnRefresh.Location = new System.Drawing.Point(9, 28);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(123, 42);
            this.btnRefresh.TabIndex = 24;
            this.btnRefresh.TabStop = false;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = false;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // btnApply
            // 
            this.btnApply.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnApply.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnApply.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnApply.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnApply.CustomForeColor = System.Drawing.Color.Black;
            this.btnApply.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnApply.ForeColor = System.Drawing.Color.Black;
            this.btnApply.ImageSize = new System.Drawing.Size(45, 45);
            this.btnApply.Location = new System.Drawing.Point(607, 109);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(123, 42);
            this.btnApply.TabIndex = 25;
            this.btnApply.TabStop = false;
            this.btnApply.Text = "Apply";
            this.btnApply.UseVisualStyleBackColor = false;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // lbCalibFilePathValue
            // 
            this.lbCalibFilePathValue.BackColor = System.Drawing.Color.Black;
            this.lbCalibFilePathValue.BorderColor = System.Drawing.Color.Black;
            this.lbCalibFilePathValue.BorderWidth = 1;
            this.lbCalibFilePathValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbCalibFilePathValue.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold);
            this.lbCalibFilePathValue.ForeColor = System.Drawing.Color.Lime;
            this.lbCalibFilePathValue.Location = new System.Drawing.Point(110, 102);
            this.lbCalibFilePathValue.Margin = new System.Windows.Forms.Padding(3);
            this.lbCalibFilePathValue.Name = "lbCalibFilePathValue";
            this.lbCalibFilePathValue.Size = new System.Drawing.Size(453, 30);
            this.lbCalibFilePathValue.TabIndex = 24;
            this.lbCalibFilePathValue.Text = " - ";
            this.lbCalibFilePathValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // customBorderLabel7
            // 
            this.customBorderLabel7.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(206)))), ((int)(((byte)(206)))));
            this.customBorderLabel7.BorderWidth = 1;
            this.customBorderLabel7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.customBorderLabel7.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            this.customBorderLabel7.Location = new System.Drawing.Point(0, 102);
            this.customBorderLabel7.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.customBorderLabel7.Name = "customBorderLabel7";
            this.customBorderLabel7.Size = new System.Drawing.Size(107, 30);
            this.customBorderLabel7.TabIndex = 23;
            this.customBorderLabel7.Text = "Calib File Path";
            this.customBorderLabel7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbInterfaceOptionValue
            // 
            this.lbInterfaceOptionValue.BackColor = System.Drawing.Color.Black;
            this.lbInterfaceOptionValue.BorderColor = System.Drawing.Color.Black;
            this.lbInterfaceOptionValue.BorderWidth = 1;
            this.lbInterfaceOptionValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbInterfaceOptionValue.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold);
            this.lbInterfaceOptionValue.ForeColor = System.Drawing.Color.Lime;
            this.lbInterfaceOptionValue.Location = new System.Drawing.Point(110, 36);
            this.lbInterfaceOptionValue.Margin = new System.Windows.Forms.Padding(3);
            this.lbInterfaceOptionValue.Name = "lbInterfaceOptionValue";
            this.lbInterfaceOptionValue.Size = new System.Drawing.Size(453, 27);
            this.lbInterfaceOptionValue.TabIndex = 20;
            this.lbInterfaceOptionValue.Text = " - ";
            this.lbInterfaceOptionValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lbConfigFilePathValue
            // 
            this.lbConfigFilePathValue.BackColor = System.Drawing.Color.Black;
            this.lbConfigFilePathValue.BorderColor = System.Drawing.Color.Black;
            this.lbConfigFilePathValue.BorderWidth = 1;
            this.lbConfigFilePathValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbConfigFilePathValue.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold);
            this.lbConfigFilePathValue.ForeColor = System.Drawing.Color.Lime;
            this.lbConfigFilePathValue.Location = new System.Drawing.Point(110, 69);
            this.lbConfigFilePathValue.Margin = new System.Windows.Forms.Padding(3);
            this.lbConfigFilePathValue.Name = "lbConfigFilePathValue";
            this.lbConfigFilePathValue.Size = new System.Drawing.Size(453, 27);
            this.lbConfigFilePathValue.TabIndex = 22;
            this.lbConfigFilePathValue.Text = " - ";
            this.lbConfigFilePathValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // customBorderLabel2
            // 
            this.customBorderLabel2.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(206)))), ((int)(((byte)(206)))));
            this.customBorderLabel2.BorderWidth = 1;
            this.customBorderLabel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.customBorderLabel2.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            this.customBorderLabel2.Location = new System.Drawing.Point(0, 69);
            this.customBorderLabel2.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.customBorderLabel2.Name = "customBorderLabel2";
            this.customBorderLabel2.Size = new System.Drawing.Size(107, 27);
            this.customBorderLabel2.TabIndex = 21;
            this.customBorderLabel2.Text = "Config File Path";
            this.customBorderLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // customBorderLabel3
            // 
            this.customBorderLabel3.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(206)))), ((int)(((byte)(206)))));
            this.customBorderLabel3.BorderWidth = 1;
            this.customBorderLabel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.customBorderLabel3.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            this.customBorderLabel3.Location = new System.Drawing.Point(0, 3);
            this.customBorderLabel3.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.customBorderLabel3.Name = "customBorderLabel3";
            this.customBorderLabel3.Size = new System.Drawing.Size(107, 27);
            this.customBorderLabel3.TabIndex = 17;
            this.customBorderLabel3.Text = "Interface Type";
            this.customBorderLabel3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // customBorderLabel4
            // 
            this.customBorderLabel4.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(206)))), ((int)(((byte)(206)))));
            this.customBorderLabel4.BorderWidth = 1;
            this.customBorderLabel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.customBorderLabel4.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            this.customBorderLabel4.Location = new System.Drawing.Point(0, 36);
            this.customBorderLabel4.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.customBorderLabel4.Name = "customBorderLabel4";
            this.customBorderLabel4.Size = new System.Drawing.Size(107, 27);
            this.customBorderLabel4.TabIndex = 19;
            this.customBorderLabel4.Text = "Interface Option";
            this.customBorderLabel4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbInterfaceTypeValue
            // 
            this.lbInterfaceTypeValue.BackColor = System.Drawing.Color.Black;
            this.lbInterfaceTypeValue.BorderColor = System.Drawing.Color.Black;
            this.lbInterfaceTypeValue.BorderWidth = 1;
            this.lbInterfaceTypeValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbInterfaceTypeValue.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold);
            this.lbInterfaceTypeValue.ForeColor = System.Drawing.Color.Lime;
            this.lbInterfaceTypeValue.Location = new System.Drawing.Point(110, 3);
            this.lbInterfaceTypeValue.Margin = new System.Windows.Forms.Padding(3);
            this.lbInterfaceTypeValue.Name = "lbInterfaceTypeValue";
            this.lbInterfaceTypeValue.Size = new System.Drawing.Size(453, 27);
            this.lbInterfaceTypeValue.TabIndex = 18;
            this.lbInterfaceTypeValue.Text = " - ";
            this.lbInterfaceTypeValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lbxIntOption
            // 
            this.lbxIntOption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbxIntOption.FormattingEnabled = true;
            this.lbxIntOption.ItemHeight = 17;
            this.lbxIntOption.Location = new System.Drawing.Point(10, 28);
            this.lbxIntOption.Name = "lbxIntOption";
            this.lbxIntOption.Size = new System.Drawing.Size(270, 271);
            this.lbxIntOption.TabIndex = 0;
            this.lbxIntOption.SelectedIndexChanged += new System.EventHandler(this.lbxIntOption_SelectedIndexChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.lbxIntOption);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.groupBox2.Location = new System.Drawing.Point(299, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(10);
            this.groupBox2.Size = new System.Drawing.Size(290, 309);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Interface Option";
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 1;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Controls.Add(this.groupBox4, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.groupBox3, 0, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(595, 3);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 2;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 195F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(146, 309);
            this.tableLayoutPanel3.TabIndex = 2;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.btnRefresh);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox3.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.groupBox3.Location = new System.Drawing.Point(3, 0);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(10);
            this.groupBox3.Size = new System.Drawing.Size(140, 111);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Interface List";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.btnSelectCalibFile);
            this.groupBox4.Controls.Add(this.btnSelectConfigFile);
            this.groupBox4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox4.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.groupBox4.Location = new System.Drawing.Point(3, 117);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Padding = new System.Windows.Forms.Padding(10);
            this.groupBox4.Size = new System.Drawing.Size(140, 189);
            this.groupBox4.TabIndex = 3;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Select File";
            // 
            // FormSetupCASSpectrometerInterface
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(770, 509);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "FormSetupCASSpectrometerInterface";
            this.Padding = new System.Windows.Forms.Padding(10);
            this.Text = "FormSetupCASSpectrometerInterface";
            this.Shown += new System.EventHandler(this.FormSetupCASSpectrometerInterface_Shown);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.tableLayoutPanel15.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ListBox lbxIntType;
        private IndividualMenuButton btnRefresh;
        private System.Windows.Forms.Panel panel2;
        private IndividualMenuButton btnApply;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel15;
        private CustomControl.CustomBorderLabel lbCalibFilePathValue;
        private CustomControl.CustomBorderLabel customBorderLabel7;
        private CustomControl.CustomBorderLabel lbConfigFilePathValue;
        private CustomControl.CustomBorderLabel customBorderLabel2;
        private CustomControl.CustomBorderLabel customBorderLabel3;
        private CustomControl.CustomBorderLabel customBorderLabel4;
        private CustomControl.CustomBorderLabel lbInterfaceTypeValue;
        private CustomControl.CustomBorderLabel lbInterfaceOptionValue;
        private IndividualMenuButton btnSelectCalibFile;
        private IndividualMenuButton btnSelectConfigFile;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ListBox lbxIntOption;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.GroupBox groupBox3;
    }
}