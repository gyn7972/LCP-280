namespace QMC.LCP_280.Process.Unit.FormSetup.Page
{
    partial class SpectrometerPage
    {
        /// <summary> 
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 구성 요소 디자이너에서 생성한 코드

        /// <summary> 
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanel8 = new System.Windows.Forms.TableLayoutPanel();
            this.lbivSelectSpectrometer = new QMC.Common.ListBoxItemsView();
            this.tableLayoutPanel9 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnSpectrometerSetup = new QMC.Common.IndividualMenuButton();
            this.btnSpectrometerInitialize = new QMC.Common.IndividualMenuButton();
            this.tableLayoutPanel14 = new System.Windows.Forms.TableLayoutPanel();
            this.lbSpectrometerDeviceOption = new QMC.Common.CustomControl.CustomBorderLabel();
            this.lbSpectrometerDeviceInterfaceValue = new QMC.Common.CustomControl.CustomBorderLabel();
            this.customBorderLabel9 = new QMC.Common.CustomControl.CustomBorderLabel();
            this.customBorderLabel7 = new QMC.Common.CustomControl.CustomBorderLabel();
            this.customBorderLabel1 = new QMC.Common.CustomControl.CustomBorderLabel();
            this.lbSpectrometerStatusValue = new QMC.Common.CustomControl.CustomBorderLabel();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel15 = new System.Windows.Forms.TableLayoutPanel();
            this.customBorderLabel3 = new QMC.Common.CustomControl.CustomBorderLabel();
            this.customBorderLabel4 = new QMC.Common.CustomControl.CustomBorderLabel();
            this.lbSpectrometerSerialNoValue = new QMC.Common.CustomControl.CustomBorderLabel();
            this.lbSpectrometerModelValue = new QMC.Common.CustomControl.CustomBorderLabel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnMeasureDarkCurrent = new QMC.Common.IndividualMenuButton();
            this.btnMeasureTest = new QMC.Common.IndividualMenuButton();
            this.tableLayoutPanel16 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel17 = new System.Windows.Forms.TableLayoutPanel();
            this.btnSpectrometerConfigSave = new QMC.Common.IndividualMenuButton();
            this.pcvSpectrometerConfig = new QMC.Common.PropertyCollectionView();
            this.casSpectrumViewer = new QMC.Common.Spectrometer.CASSpectrumViewer();
            this.tableLayoutPanel8.SuspendLayout();
            this.tableLayoutPanel9.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tableLayoutPanel14.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.tableLayoutPanel15.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel16.SuspendLayout();
            this.tableLayoutPanel17.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel8
            // 
            this.tableLayoutPanel8.ColumnCount = 3;
            this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 301F));
            this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 52.54777F));
            this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 47.45223F));
            this.tableLayoutPanel8.Controls.Add(this.lbivSelectSpectrometer, 0, 0);
            this.tableLayoutPanel8.Controls.Add(this.tableLayoutPanel9, 1, 0);
            this.tableLayoutPanel8.Controls.Add(this.tableLayoutPanel16, 2, 0);
            this.tableLayoutPanel8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel8.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel8.Name = "tableLayoutPanel8";
            this.tableLayoutPanel8.RowCount = 1;
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel8.Size = new System.Drawing.Size(1250, 700);
            this.tableLayoutPanel8.TabIndex = 2;
            // 
            // lbivSelectSpectrometer
            // 
            this.lbivSelectSpectrometer.BorderColor = System.Drawing.Color.White;
            this.lbivSelectSpectrometer.BorderWidth = 2;
            this.lbivSelectSpectrometer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbivSelectSpectrometer.GroupBackColor = System.Drawing.Color.White;
            this.lbivSelectSpectrometer.GroupForeColor = System.Drawing.Color.Black;
            this.lbivSelectSpectrometer.GroupName = "Select Item";
            this.lbivSelectSpectrometer.ItemBackColor = System.Drawing.Color.Black;
            this.lbivSelectSpectrometer.ItemForeColor = System.Drawing.Color.Lime;
            this.lbivSelectSpectrometer.ListBackColor = System.Drawing.Color.Black;
            this.lbivSelectSpectrometer.ListForeColor = System.Drawing.Color.Lime;
            this.lbivSelectSpectrometer.Location = new System.Drawing.Point(4, 5);
            this.lbivSelectSpectrometer.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.lbivSelectSpectrometer.Name = "lbivSelectSpectrometer";
            this.lbivSelectSpectrometer.SelectedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(255)))), ((int)(((byte)(0)))));
            this.lbivSelectSpectrometer.SelectedForeColor = System.Drawing.Color.Black;
            this.lbivSelectSpectrometer.SelectedIndex = -1;
            this.lbivSelectSpectrometer.Size = new System.Drawing.Size(293, 690);
            this.lbivSelectSpectrometer.TabIndex = 0;
            this.lbivSelectSpectrometer.ItemSelected += new System.EventHandler<int>(this.lbivSelectSpectrometer_ItemSelected);
            // 
            // tableLayoutPanel9
            // 
            this.tableLayoutPanel9.ColumnCount = 1;
            this.tableLayoutPanel9.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel9.Controls.Add(this.groupBox2, 0, 1);
            this.tableLayoutPanel9.Controls.Add(this.groupBox3, 0, 0);
            this.tableLayoutPanel9.Controls.Add(this.groupBox1, 0, 2);
            this.tableLayoutPanel9.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel9.Location = new System.Drawing.Point(304, 3);
            this.tableLayoutPanel9.Name = "tableLayoutPanel9";
            this.tableLayoutPanel9.RowCount = 3;
            this.tableLayoutPanel9.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 121F));
            this.tableLayoutPanel9.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 209F));
            this.tableLayoutPanel9.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 115F));
            this.tableLayoutPanel9.Size = new System.Drawing.Size(492, 694);
            this.tableLayoutPanel9.TabIndex = 1;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnSpectrometerSetup);
            this.groupBox2.Controls.Add(this.btnSpectrometerInitialize);
            this.groupBox2.Controls.Add(this.tableLayoutPanel14);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox2.Location = new System.Drawing.Point(3, 124);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(7);
            this.groupBox2.Size = new System.Drawing.Size(486, 203);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Communication";
            // 
            // btnSpectrometerSetup
            // 
            this.btnSpectrometerSetup.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnSpectrometerSetup.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnSpectrometerSetup.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnSpectrometerSetup.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnSpectrometerSetup.CustomForeColor = System.Drawing.Color.Black;
            this.btnSpectrometerSetup.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSpectrometerSetup.ForeColor = System.Drawing.Color.Black;
            this.btnSpectrometerSetup.ImageSize = new System.Drawing.Size(45, 45);
            this.btnSpectrometerSetup.Location = new System.Drawing.Point(6, 153);
            this.btnSpectrometerSetup.Name = "btnSpectrometerSetup";
            this.btnSpectrometerSetup.Size = new System.Drawing.Size(90, 39);
            this.btnSpectrometerSetup.TabIndex = 23;
            this.btnSpectrometerSetup.TabStop = false;
            this.btnSpectrometerSetup.Text = "Setup";
            this.btnSpectrometerSetup.UseVisualStyleBackColor = false;
            this.btnSpectrometerSetup.Click += new System.EventHandler(this.btnSpectrometerSetup_Click);
            // 
            // btnSpectrometerInitialize
            // 
            this.btnSpectrometerInitialize.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnSpectrometerInitialize.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnSpectrometerInitialize.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnSpectrometerInitialize.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnSpectrometerInitialize.CustomForeColor = System.Drawing.Color.Black;
            this.btnSpectrometerInitialize.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSpectrometerInitialize.ForeColor = System.Drawing.Color.Black;
            this.btnSpectrometerInitialize.ImageSize = new System.Drawing.Size(45, 45);
            this.btnSpectrometerInitialize.Location = new System.Drawing.Point(390, 153);
            this.btnSpectrometerInitialize.Name = "btnSpectrometerInitialize";
            this.btnSpectrometerInitialize.Size = new System.Drawing.Size(90, 39);
            this.btnSpectrometerInitialize.TabIndex = 24;
            this.btnSpectrometerInitialize.TabStop = false;
            this.btnSpectrometerInitialize.Text = "Initialize";
            this.btnSpectrometerInitialize.UseVisualStyleBackColor = false;
            this.btnSpectrometerInitialize.Click += new System.EventHandler(this.btnSpectrometerInitialize_Click);
            // 
            // tableLayoutPanel14
            // 
            this.tableLayoutPanel14.ColumnCount = 2;
            this.tableLayoutPanel14.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 21.72996F));
            this.tableLayoutPanel14.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 78.27004F));
            this.tableLayoutPanel14.Controls.Add(this.lbSpectrometerDeviceOption, 1, 2);
            this.tableLayoutPanel14.Controls.Add(this.lbSpectrometerDeviceInterfaceValue, 1, 1);
            this.tableLayoutPanel14.Controls.Add(this.customBorderLabel9, 0, 2);
            this.tableLayoutPanel14.Controls.Add(this.customBorderLabel7, 0, 1);
            this.tableLayoutPanel14.Controls.Add(this.customBorderLabel1, 0, 0);
            this.tableLayoutPanel14.Controls.Add(this.lbSpectrometerStatusValue, 1, 0);
            this.tableLayoutPanel14.Location = new System.Drawing.Point(6, 25);
            this.tableLayoutPanel14.Name = "tableLayoutPanel14";
            this.tableLayoutPanel14.RowCount = 3;
            this.tableLayoutPanel14.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel14.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel14.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel14.Size = new System.Drawing.Size(474, 119);
            this.tableLayoutPanel14.TabIndex = 25;
            // 
            // lbSpectrometerDeviceOption
            // 
            this.lbSpectrometerDeviceOption.BackColor = System.Drawing.Color.Black;
            this.lbSpectrometerDeviceOption.BorderColor = System.Drawing.Color.Black;
            this.lbSpectrometerDeviceOption.BorderWidth = 1;
            this.lbSpectrometerDeviceOption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbSpectrometerDeviceOption.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold);
            this.lbSpectrometerDeviceOption.ForeColor = System.Drawing.Color.Lime;
            this.lbSpectrometerDeviceOption.Location = new System.Drawing.Point(106, 81);
            this.lbSpectrometerDeviceOption.Margin = new System.Windows.Forms.Padding(3);
            this.lbSpectrometerDeviceOption.Name = "lbSpectrometerDeviceOption";
            this.lbSpectrometerDeviceOption.Size = new System.Drawing.Size(365, 35);
            this.lbSpectrometerDeviceOption.TabIndex = 25;
            this.lbSpectrometerDeviceOption.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lbSpectrometerDeviceInterfaceValue
            // 
            this.lbSpectrometerDeviceInterfaceValue.BackColor = System.Drawing.Color.Black;
            this.lbSpectrometerDeviceInterfaceValue.BorderColor = System.Drawing.Color.Black;
            this.lbSpectrometerDeviceInterfaceValue.BorderWidth = 1;
            this.lbSpectrometerDeviceInterfaceValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbSpectrometerDeviceInterfaceValue.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold);
            this.lbSpectrometerDeviceInterfaceValue.ForeColor = System.Drawing.Color.Lime;
            this.lbSpectrometerDeviceInterfaceValue.Location = new System.Drawing.Point(106, 42);
            this.lbSpectrometerDeviceInterfaceValue.Margin = new System.Windows.Forms.Padding(3);
            this.lbSpectrometerDeviceInterfaceValue.Name = "lbSpectrometerDeviceInterfaceValue";
            this.lbSpectrometerDeviceInterfaceValue.Size = new System.Drawing.Size(365, 33);
            this.lbSpectrometerDeviceInterfaceValue.TabIndex = 24;
            this.lbSpectrometerDeviceInterfaceValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // customBorderLabel9
            // 
            this.customBorderLabel9.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(206)))), ((int)(((byte)(206)))));
            this.customBorderLabel9.BorderWidth = 1;
            this.customBorderLabel9.Dock = System.Windows.Forms.DockStyle.Fill;
            this.customBorderLabel9.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            this.customBorderLabel9.Location = new System.Drawing.Point(0, 81);
            this.customBorderLabel9.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.customBorderLabel9.Name = "customBorderLabel9";
            this.customBorderLabel9.Size = new System.Drawing.Size(103, 35);
            this.customBorderLabel9.TabIndex = 23;
            this.customBorderLabel9.Text = "Device Option";
            this.customBorderLabel9.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // customBorderLabel7
            // 
            this.customBorderLabel7.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(206)))), ((int)(((byte)(206)))));
            this.customBorderLabel7.BorderWidth = 1;
            this.customBorderLabel7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.customBorderLabel7.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            this.customBorderLabel7.Location = new System.Drawing.Point(0, 42);
            this.customBorderLabel7.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.customBorderLabel7.Name = "customBorderLabel7";
            this.customBorderLabel7.Size = new System.Drawing.Size(103, 33);
            this.customBorderLabel7.TabIndex = 21;
            this.customBorderLabel7.Text = "Device Interface";
            this.customBorderLabel7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // customBorderLabel1
            // 
            this.customBorderLabel1.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(206)))), ((int)(((byte)(206)))));
            this.customBorderLabel1.BorderWidth = 1;
            this.customBorderLabel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.customBorderLabel1.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            this.customBorderLabel1.Location = new System.Drawing.Point(0, 3);
            this.customBorderLabel1.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.customBorderLabel1.Name = "customBorderLabel1";
            this.customBorderLabel1.Size = new System.Drawing.Size(103, 33);
            this.customBorderLabel1.TabIndex = 19;
            this.customBorderLabel1.Text = "Status";
            this.customBorderLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbSpectrometerStatusValue
            // 
            this.lbSpectrometerStatusValue.BackColor = System.Drawing.Color.Black;
            this.lbSpectrometerStatusValue.BorderColor = System.Drawing.Color.Black;
            this.lbSpectrometerStatusValue.BorderWidth = 1;
            this.lbSpectrometerStatusValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbSpectrometerStatusValue.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold);
            this.lbSpectrometerStatusValue.ForeColor = System.Drawing.Color.Lime;
            this.lbSpectrometerStatusValue.Location = new System.Drawing.Point(106, 3);
            this.lbSpectrometerStatusValue.Margin = new System.Windows.Forms.Padding(3);
            this.lbSpectrometerStatusValue.Name = "lbSpectrometerStatusValue";
            this.lbSpectrometerStatusValue.Size = new System.Drawing.Size(365, 33);
            this.lbSpectrometerStatusValue.TabIndex = 20;
            this.lbSpectrometerStatusValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.tableLayoutPanel15);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox3.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox3.Location = new System.Drawing.Point(3, 3);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(7);
            this.groupBox3.Size = new System.Drawing.Size(486, 115);
            this.groupBox3.TabIndex = 0;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Information";
            // 
            // tableLayoutPanel15
            // 
            this.tableLayoutPanel15.ColumnCount = 2;
            this.tableLayoutPanel15.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 22.12766F));
            this.tableLayoutPanel15.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 77.87234F));
            this.tableLayoutPanel15.Controls.Add(this.customBorderLabel3, 0, 0);
            this.tableLayoutPanel15.Controls.Add(this.customBorderLabel4, 0, 1);
            this.tableLayoutPanel15.Controls.Add(this.lbSpectrometerSerialNoValue, 1, 1);
            this.tableLayoutPanel15.Controls.Add(this.lbSpectrometerModelValue, 1, 0);
            this.tableLayoutPanel15.Location = new System.Drawing.Point(6, 24);
            this.tableLayoutPanel15.Name = "tableLayoutPanel15";
            this.tableLayoutPanel15.RowCount = 2;
            this.tableLayoutPanel15.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel15.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel15.Size = new System.Drawing.Size(474, 77);
            this.tableLayoutPanel15.TabIndex = 21;
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
            this.customBorderLabel3.Size = new System.Drawing.Size(104, 32);
            this.customBorderLabel3.TabIndex = 17;
            this.customBorderLabel3.Text = "Model";
            this.customBorderLabel3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // customBorderLabel4
            // 
            this.customBorderLabel4.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(206)))), ((int)(((byte)(206)))));
            this.customBorderLabel4.BorderWidth = 1;
            this.customBorderLabel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.customBorderLabel4.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            this.customBorderLabel4.Location = new System.Drawing.Point(0, 41);
            this.customBorderLabel4.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.customBorderLabel4.Name = "customBorderLabel4";
            this.customBorderLabel4.Size = new System.Drawing.Size(104, 33);
            this.customBorderLabel4.TabIndex = 19;
            this.customBorderLabel4.Text = "Serial No.";
            this.customBorderLabel4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbSpectrometerSerialNoValue
            // 
            this.lbSpectrometerSerialNoValue.BackColor = System.Drawing.Color.Black;
            this.lbSpectrometerSerialNoValue.BorderColor = System.Drawing.Color.Black;
            this.lbSpectrometerSerialNoValue.BorderWidth = 1;
            this.lbSpectrometerSerialNoValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbSpectrometerSerialNoValue.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold);
            this.lbSpectrometerSerialNoValue.ForeColor = System.Drawing.Color.Lime;
            this.lbSpectrometerSerialNoValue.Location = new System.Drawing.Point(107, 41);
            this.lbSpectrometerSerialNoValue.Margin = new System.Windows.Forms.Padding(3);
            this.lbSpectrometerSerialNoValue.Name = "lbSpectrometerSerialNoValue";
            this.lbSpectrometerSerialNoValue.Size = new System.Drawing.Size(364, 33);
            this.lbSpectrometerSerialNoValue.TabIndex = 20;
            this.lbSpectrometerSerialNoValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lbSpectrometerModelValue
            // 
            this.lbSpectrometerModelValue.BackColor = System.Drawing.Color.Black;
            this.lbSpectrometerModelValue.BorderColor = System.Drawing.Color.Black;
            this.lbSpectrometerModelValue.BorderWidth = 1;
            this.lbSpectrometerModelValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbSpectrometerModelValue.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold);
            this.lbSpectrometerModelValue.ForeColor = System.Drawing.Color.Lime;
            this.lbSpectrometerModelValue.Location = new System.Drawing.Point(107, 3);
            this.lbSpectrometerModelValue.Margin = new System.Windows.Forms.Padding(3);
            this.lbSpectrometerModelValue.Name = "lbSpectrometerModelValue";
            this.lbSpectrometerModelValue.Size = new System.Drawing.Size(364, 32);
            this.lbSpectrometerModelValue.TabIndex = 18;
            this.lbSpectrometerModelValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnMeasureDarkCurrent);
            this.groupBox1.Controls.Add(this.btnMeasureTest);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(3, 333);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(486, 358);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Control";
            // 
            // btnMeasureDarkCurrent
            // 
            this.btnMeasureDarkCurrent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnMeasureDarkCurrent.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnMeasureDarkCurrent.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnMeasureDarkCurrent.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnMeasureDarkCurrent.CustomForeColor = System.Drawing.Color.Black;
            this.btnMeasureDarkCurrent.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnMeasureDarkCurrent.ForeColor = System.Drawing.Color.Black;
            this.btnMeasureDarkCurrent.ImageSize = new System.Drawing.Size(45, 45);
            this.btnMeasureDarkCurrent.Location = new System.Drawing.Point(20, 90);
            this.btnMeasureDarkCurrent.Name = "btnMeasureDarkCurrent";
            this.btnMeasureDarkCurrent.Size = new System.Drawing.Size(185, 43);
            this.btnMeasureDarkCurrent.TabIndex = 25;
            this.btnMeasureDarkCurrent.TabStop = false;
            this.btnMeasureDarkCurrent.Text = "Measure DarkCurrent";
            this.btnMeasureDarkCurrent.UseVisualStyleBackColor = false;
            this.btnMeasureDarkCurrent.Click += new System.EventHandler(this.btnMeasureDarkCurrent_Click);
            // 
            // btnMeasureTest
            // 
            this.btnMeasureTest.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnMeasureTest.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnMeasureTest.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnMeasureTest.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnMeasureTest.CustomForeColor = System.Drawing.Color.Black;
            this.btnMeasureTest.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnMeasureTest.ForeColor = System.Drawing.Color.Black;
            this.btnMeasureTest.ImageSize = new System.Drawing.Size(45, 45);
            this.btnMeasureTest.Location = new System.Drawing.Point(20, 38);
            this.btnMeasureTest.Name = "btnMeasureTest";
            this.btnMeasureTest.Size = new System.Drawing.Size(185, 43);
            this.btnMeasureTest.TabIndex = 24;
            this.btnMeasureTest.TabStop = false;
            this.btnMeasureTest.Text = "Measure";
            this.btnMeasureTest.UseVisualStyleBackColor = false;
            this.btnMeasureTest.Click += new System.EventHandler(this.btnMeasureTest_Click);
            // 
            // tableLayoutPanel16
            // 
            this.tableLayoutPanel16.ColumnCount = 1;
            this.tableLayoutPanel16.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel16.Controls.Add(this.tableLayoutPanel17, 0, 0);
            this.tableLayoutPanel16.Controls.Add(this.casSpectrumViewer, 0, 1);
            this.tableLayoutPanel16.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel16.Location = new System.Drawing.Point(802, 3);
            this.tableLayoutPanel16.Name = "tableLayoutPanel16";
            this.tableLayoutPanel16.RowCount = 2;
            this.tableLayoutPanel16.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 61.07193F));
            this.tableLayoutPanel16.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 38.92807F));
            this.tableLayoutPanel16.Size = new System.Drawing.Size(445, 694);
            this.tableLayoutPanel16.TabIndex = 2;
            // 
            // tableLayoutPanel17
            // 
            this.tableLayoutPanel17.ColumnCount = 1;
            this.tableLayoutPanel17.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel17.Controls.Add(this.btnSpectrometerConfigSave, 0, 1);
            this.tableLayoutPanel17.Controls.Add(this.pcvSpectrometerConfig, 0, 0);
            this.tableLayoutPanel17.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel17.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel17.Name = "tableLayoutPanel17";
            this.tableLayoutPanel17.RowCount = 2;
            this.tableLayoutPanel17.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 89.48787F));
            this.tableLayoutPanel17.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10.51213F));
            this.tableLayoutPanel17.Size = new System.Drawing.Size(439, 417);
            this.tableLayoutPanel17.TabIndex = 0;
            // 
            // btnSpectrometerConfigSave
            // 
            this.btnSpectrometerConfigSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnSpectrometerConfigSave.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnSpectrometerConfigSave.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnSpectrometerConfigSave.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnSpectrometerConfigSave.CustomForeColor = System.Drawing.Color.Black;
            this.btnSpectrometerConfigSave.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnSpectrometerConfigSave.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSpectrometerConfigSave.ForeColor = System.Drawing.Color.Black;
            this.btnSpectrometerConfigSave.ImageSize = new System.Drawing.Size(45, 45);
            this.btnSpectrometerConfigSave.Location = new System.Drawing.Point(346, 376);
            this.btnSpectrometerConfigSave.Name = "btnSpectrometerConfigSave";
            this.btnSpectrometerConfigSave.Size = new System.Drawing.Size(90, 38);
            this.btnSpectrometerConfigSave.TabIndex = 27;
            this.btnSpectrometerConfigSave.TabStop = false;
            this.btnSpectrometerConfigSave.Text = "Save";
            this.btnSpectrometerConfigSave.UseVisualStyleBackColor = false;
            this.btnSpectrometerConfigSave.Click += new System.EventHandler(this.btnSpectrometerConfigSave_Click);
            // 
            // pcvSpectrometerConfig
            // 
            this.pcvSpectrometerConfig.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pcvSpectrometerConfig.FastBuild = true;
            this.pcvSpectrometerConfig.GroupName = "Property";
            this.pcvSpectrometerConfig.Location = new System.Drawing.Point(0, 0);
            this.pcvSpectrometerConfig.Margin = new System.Windows.Forms.Padding(0);
            this.pcvSpectrometerConfig.Name = "pcvSpectrometerConfig";
            this.pcvSpectrometerConfig.Size = new System.Drawing.Size(439, 373);
            this.pcvSpectrometerConfig.SuppressResizeInvalidation = true;
            this.pcvSpectrometerConfig.TabIndex = 7;
            // 
            // casSpectrumViewer
            // 
            this.casSpectrumViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.casSpectrumViewer.Location = new System.Drawing.Point(4, 427);
            this.casSpectrumViewer.Margin = new System.Windows.Forms.Padding(4);
            this.casSpectrumViewer.Name = "casSpectrumViewer";
            this.casSpectrumViewer.Size = new System.Drawing.Size(437, 263);
            this.casSpectrumViewer.TabIndex = 1;
            // 
            // SpectrometerPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.tableLayoutPanel8);
            this.Name = "SpectrometerPage";
            this.Size = new System.Drawing.Size(1250, 700);
            this.tableLayoutPanel8.ResumeLayout(false);
            this.tableLayoutPanel9.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.tableLayoutPanel14.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.tableLayoutPanel15.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.tableLayoutPanel16.ResumeLayout(false);
            this.tableLayoutPanel17.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel8;
        private Common.ListBoxItemsView lbivSelectSpectrometer;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel9;
        private System.Windows.Forms.GroupBox groupBox2;
        private Common.IndividualMenuButton btnSpectrometerSetup;
        private Common.IndividualMenuButton btnSpectrometerInitialize;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel14;
        private Common.CustomControl.CustomBorderLabel lbSpectrometerDeviceOption;
        private Common.CustomControl.CustomBorderLabel lbSpectrometerDeviceInterfaceValue;
        private Common.CustomControl.CustomBorderLabel customBorderLabel9;
        private Common.CustomControl.CustomBorderLabel customBorderLabel7;
        private Common.CustomControl.CustomBorderLabel customBorderLabel1;
        private Common.CustomControl.CustomBorderLabel lbSpectrometerStatusValue;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel15;
        private Common.CustomControl.CustomBorderLabel customBorderLabel3;
        private Common.CustomControl.CustomBorderLabel customBorderLabel4;
        private Common.CustomControl.CustomBorderLabel lbSpectrometerSerialNoValue;
        private Common.CustomControl.CustomBorderLabel lbSpectrometerModelValue;
        private System.Windows.Forms.GroupBox groupBox1;
        private Common.IndividualMenuButton btnMeasureDarkCurrent;
        private Common.IndividualMenuButton btnMeasureTest;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel16;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel17;
        private Common.IndividualMenuButton btnSpectrometerConfigSave;
        private Common.PropertyCollectionView pcvSpectrometerConfig;
        private Common.Spectrometer.CASSpectrumViewer casSpectrumViewer;
    }
}
