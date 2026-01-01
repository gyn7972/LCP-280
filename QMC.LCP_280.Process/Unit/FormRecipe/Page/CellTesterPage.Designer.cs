namespace QMC.LCP_280.Process.Unit.FormRecipe.Page
{
    partial class CellTesterPage
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.dataGridResult = new System.Windows.Forms.DataGridView();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.casSpectrumViewer = new QMC.Common.Spectrometer.CASSpectrumViewer();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.lbResultValue = new QMC.Common.CustomControl.CustomBorderLabel();
            this.tableLayoutPanel8 = new System.Windows.Forms.TableLayoutPanel();
            this.rbvOption = new QMC.Common.RadioButtonView();
            this.tableLayoutPanel9 = new System.Windows.Forms.TableLayoutPanel();
            this.nudIntervalDelay = new System.Windows.Forms.NumericUpDown();
            this.customBorderLabel1 = new QMC.Common.CustomControl.CustomBorderLabel();
            this.lbStatusCaption = new QMC.Common.CustomControl.CustomBorderLabel();
            this.nudRepeatCount = new System.Windows.Forms.NumericUpDown();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel6 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel10 = new System.Windows.Forms.TableLayoutPanel();
            this.lbCurrentIndexNo = new System.Windows.Forms.Label();
            this.lbMeasureTime = new System.Windows.Forms.Label();
            this.btnResultSave = new QMC.Common.IndividualMenuButton();
            this.btnResultClear = new QMC.Common.IndividualMenuButton();
            this.btnLastClear = new QMC.Common.IndividualMenuButton();
            this.tableLayoutPanel7 = new System.Windows.Forms.TableLayoutPanel();
            this.btnProbeSafety = new QMC.Common.IndividualMenuButton();
            this.cbProbeIndex = new System.Windows.Forms.ComboBox();
            this.btnTestStop = new QMC.Common.IndividualMenuButton();
            this.btnTestStart = new QMC.Common.IndividualMenuButton();
            this.grpContactMode = new System.Windows.Forms.GroupBox();
            this.rbTop = new System.Windows.Forms.RadioButton();
            this.rbBottom = new System.Windows.Forms.RadioButton();
            this.lblProbeIndex = new System.Windows.Forms.Label();
            this.btnProbeSeq = new QMC.Common.IndividualMenuButton();
            this.btnTestMotionStart = new QMC.Common.IndividualMenuButton();
            this.label1 = new System.Windows.Forms.Label();
            this.btnTestMotionStop = new QMC.Common.IndividualMenuButton();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridResult)).BeginInit();
            this.tableLayoutPanel3.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            this.tableLayoutPanel8.SuspendLayout();
            this.tableLayoutPanel9.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudIntervalDelay)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRepeatCount)).BeginInit();
            this.panel1.SuspendLayout();
            this.tableLayoutPanel6.SuspendLayout();
            this.tableLayoutPanel10.SuspendLayout();
            this.tableLayoutPanel7.SuspendLayout();
            this.grpContactMode.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 208F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel7, 1, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1875, 1050);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.dataGridResult, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel3, 0, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(4, 4);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 64.55331F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 35.44669F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(1659, 1042);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // dataGridResult
            // 
            this.dataGridResult.AllowUserToResizeColumns = false;
            this.dataGridResult.AllowUserToResizeRows = false;
            this.dataGridResult.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridResult.Location = new System.Drawing.Point(4, 4);
            this.dataGridResult.Margin = new System.Windows.Forms.Padding(4);
            this.dataGridResult.MultiSelect = false;
            this.dataGridResult.Name = "dataGridResult";
            this.dataGridResult.ReadOnly = true;
            this.dataGridResult.RowHeadersWidth = 80;
            this.dataGridResult.RowTemplate.Height = 23;
            this.dataGridResult.Size = new System.Drawing.Size(1651, 664);
            this.dataGridResult.TabIndex = 0;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 69.30894F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30.69106F));
            this.tableLayoutPanel3.Controls.Add(this.tableLayoutPanel4, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.panel1, 1, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(4, 676);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(1651, 362);
            this.tableLayoutPanel3.TabIndex = 1;
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 2;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 58.72781F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 41.27219F));
            this.tableLayoutPanel4.Controls.Add(this.casSpectrumViewer, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.tableLayoutPanel5, 1, 0);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(4, 4);
            this.tableLayoutPanel4.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 1;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(1136, 354);
            this.tableLayoutPanel4.TabIndex = 0;
            // 
            // casSpectrumViewer
            // 
            this.casSpectrumViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.casSpectrumViewer.Location = new System.Drawing.Point(6, 6);
            this.casSpectrumViewer.Margin = new System.Windows.Forms.Padding(6);
            this.casSpectrumViewer.Name = "casSpectrumViewer";
            this.casSpectrumViewer.Size = new System.Drawing.Size(655, 342);
            this.casSpectrumViewer.TabIndex = 0;
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 1;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel5.Controls.Add(this.lbResultValue, 0, 0);
            this.tableLayoutPanel5.Controls.Add(this.tableLayoutPanel8, 0, 1);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(671, 4);
            this.tableLayoutPanel5.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 2;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 27.07424F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 72.92577F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(461, 346);
            this.tableLayoutPanel5.TabIndex = 1;
            // 
            // lbResultValue
            // 
            this.lbResultValue.BackColor = System.Drawing.Color.Black;
            this.lbResultValue.BorderColor = System.Drawing.Color.Black;
            this.lbResultValue.BorderWidth = 1;
            this.lbResultValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbResultValue.Font = new System.Drawing.Font("Arial", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbResultValue.ForeColor = System.Drawing.Color.Lime;
            this.lbResultValue.Location = new System.Drawing.Point(4, 4);
            this.lbResultValue.Margin = new System.Windows.Forms.Padding(4);
            this.lbResultValue.Name = "lbResultValue";
            this.lbResultValue.Size = new System.Drawing.Size(453, 85);
            this.lbResultValue.TabIndex = 22;
            this.lbResultValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tableLayoutPanel8
            // 
            this.tableLayoutPanel8.ColumnCount = 1;
            this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel8.Controls.Add(this.rbvOption, 0, 0);
            this.tableLayoutPanel8.Controls.Add(this.tableLayoutPanel9, 0, 1);
            this.tableLayoutPanel8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel8.Location = new System.Drawing.Point(4, 97);
            this.tableLayoutPanel8.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel8.Name = "tableLayoutPanel8";
            this.tableLayoutPanel8.RowCount = 2;
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 105F));
            this.tableLayoutPanel8.Size = new System.Drawing.Size(453, 245);
            this.tableLayoutPanel8.TabIndex = 23;
            // 
            // rbvOption
            // 
            this.rbvOption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rbvOption.Font = new System.Drawing.Font("맑은 고딕", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.rbvOption.GroupName = "Repeat Mode";
            this.rbvOption.Location = new System.Drawing.Point(6, 8);
            this.rbvOption.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
            this.rbvOption.Name = "rbvOption";
            this.rbvOption.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.rbvOption.SelectedIndex = -1;
            this.rbvOption.Size = new System.Drawing.Size(441, 124);
            this.rbvOption.TabIndex = 0;
            // 
            // tableLayoutPanel9
            // 
            this.tableLayoutPanel9.ColumnCount = 2;
            this.tableLayoutPanel9.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 49.33333F));
            this.tableLayoutPanel9.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.66667F));
            this.tableLayoutPanel9.Controls.Add(this.nudIntervalDelay, 1, 1);
            this.tableLayoutPanel9.Controls.Add(this.customBorderLabel1, 0, 1);
            this.tableLayoutPanel9.Controls.Add(this.lbStatusCaption, 0, 0);
            this.tableLayoutPanel9.Controls.Add(this.nudRepeatCount, 1, 0);
            this.tableLayoutPanel9.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel9.Location = new System.Drawing.Point(0, 144);
            this.tableLayoutPanel9.Margin = new System.Windows.Forms.Padding(0, 4, 0, 4);
            this.tableLayoutPanel9.Name = "tableLayoutPanel9";
            this.tableLayoutPanel9.RowCount = 2;
            this.tableLayoutPanel9.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel9.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel9.Size = new System.Drawing.Size(453, 97);
            this.tableLayoutPanel9.TabIndex = 1;
            // 
            // nudIntervalDelay
            // 
            this.nudIntervalDelay.AutoSize = true;
            this.nudIntervalDelay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nudIntervalDelay.Font = new System.Drawing.Font("맑은 고딕", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nudIntervalDelay.Location = new System.Drawing.Point(227, 52);
            this.nudIntervalDelay.Margin = new System.Windows.Forms.Padding(4);
            this.nudIntervalDelay.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.nudIntervalDelay.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudIntervalDelay.Name = "nudIntervalDelay";
            this.nudIntervalDelay.Size = new System.Drawing.Size(222, 37);
            this.nudIntervalDelay.TabIndex = 23;
            this.nudIntervalDelay.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // customBorderLabel1
            // 
            this.customBorderLabel1.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(206)))), ((int)(((byte)(206)))));
            this.customBorderLabel1.BorderWidth = 1;
            this.customBorderLabel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.customBorderLabel1.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            this.customBorderLabel1.Location = new System.Drawing.Point(0, 52);
            this.customBorderLabel1.Margin = new System.Windows.Forms.Padding(0, 4, 0, 4);
            this.customBorderLabel1.Name = "customBorderLabel1";
            this.customBorderLabel1.Size = new System.Drawing.Size(223, 41);
            this.customBorderLabel1.TabIndex = 22;
            this.customBorderLabel1.Text = "Interval Delay (ms)";
            this.customBorderLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbStatusCaption
            // 
            this.lbStatusCaption.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(206)))), ((int)(((byte)(206)))));
            this.lbStatusCaption.BorderWidth = 1;
            this.lbStatusCaption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbStatusCaption.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            this.lbStatusCaption.Location = new System.Drawing.Point(0, 4);
            this.lbStatusCaption.Margin = new System.Windows.Forms.Padding(0, 4, 0, 4);
            this.lbStatusCaption.Name = "lbStatusCaption";
            this.lbStatusCaption.Size = new System.Drawing.Size(223, 40);
            this.lbStatusCaption.TabIndex = 20;
            this.lbStatusCaption.Text = "Repeat Count (cnt)";
            this.lbStatusCaption.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // nudRepeatCount
            // 
            this.nudRepeatCount.AutoSize = true;
            this.nudRepeatCount.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nudRepeatCount.Font = new System.Drawing.Font("맑은 고딕", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nudRepeatCount.Location = new System.Drawing.Point(227, 4);
            this.nudRepeatCount.Margin = new System.Windows.Forms.Padding(4);
            this.nudRepeatCount.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudRepeatCount.Name = "nudRepeatCount";
            this.nudRepeatCount.Size = new System.Drawing.Size(222, 37);
            this.nudRepeatCount.TabIndex = 21;
            this.nudRepeatCount.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.tableLayoutPanel6);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(1148, 4);
            this.panel1.Margin = new System.Windows.Forms.Padding(4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(499, 354);
            this.panel1.TabIndex = 1;
            // 
            // tableLayoutPanel6
            // 
            this.tableLayoutPanel6.ColumnCount = 2;
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 180F));
            this.tableLayoutPanel6.Controls.Add(this.tableLayoutPanel10, 0, 0);
            this.tableLayoutPanel6.Controls.Add(this.btnResultSave, 1, 2);
            this.tableLayoutPanel6.Controls.Add(this.btnResultClear, 1, 1);
            this.tableLayoutPanel6.Controls.Add(this.btnLastClear, 1, 0);
            this.tableLayoutPanel6.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanel6.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel6.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel6.Name = "tableLayoutPanel6";
            this.tableLayoutPanel6.RowCount = 3;
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel6.Size = new System.Drawing.Size(499, 204);
            this.tableLayoutPanel6.TabIndex = 3;
            // 
            // tableLayoutPanel10
            // 
            this.tableLayoutPanel10.ColumnCount = 1;
            this.tableLayoutPanel10.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel10.Controls.Add(this.lbCurrentIndexNo, 0, 1);
            this.tableLayoutPanel10.Controls.Add(this.lbMeasureTime, 0, 0);
            this.tableLayoutPanel10.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel10.Location = new System.Drawing.Point(4, 4);
            this.tableLayoutPanel10.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel10.Name = "tableLayoutPanel10";
            this.tableLayoutPanel10.RowCount = 2;
            this.tableLayoutPanel10.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel10.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel10.Size = new System.Drawing.Size(311, 60);
            this.tableLayoutPanel10.TabIndex = 5;
            // 
            // lbCurrentIndexNo
            // 
            this.lbCurrentIndexNo.AutoSize = true;
            this.lbCurrentIndexNo.Dock = System.Windows.Forms.DockStyle.Top;
            this.lbCurrentIndexNo.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lbCurrentIndexNo.Location = new System.Drawing.Point(4, 30);
            this.lbCurrentIndexNo.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbCurrentIndexNo.Name = "lbCurrentIndexNo";
            this.lbCurrentIndexNo.Size = new System.Drawing.Size(303, 25);
            this.lbCurrentIndexNo.TabIndex = 5;
            this.lbCurrentIndexNo.Text = "Index No: -";
            // 
            // lbMeasureTime
            // 
            this.lbMeasureTime.AutoSize = true;
            this.lbMeasureTime.Dock = System.Windows.Forms.DockStyle.Top;
            this.lbMeasureTime.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lbMeasureTime.Location = new System.Drawing.Point(4, 0);
            this.lbMeasureTime.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbMeasureTime.Name = "lbMeasureTime";
            this.lbMeasureTime.Size = new System.Drawing.Size(303, 25);
            this.lbMeasureTime.TabIndex = 4;
            this.lbMeasureTime.Text = "Measure Time: -";
            // 
            // btnResultSave
            // 
            this.btnResultSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnResultSave.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnResultSave.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnResultSave.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnResultSave.CustomForeColor = System.Drawing.Color.Black;
            this.btnResultSave.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnResultSave.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnResultSave.ForeColor = System.Drawing.Color.Black;
            this.btnResultSave.ImageSize = new System.Drawing.Size(45, 45);
            this.btnResultSave.Location = new System.Drawing.Point(323, 140);
            this.btnResultSave.Margin = new System.Windows.Forms.Padding(4);
            this.btnResultSave.Name = "btnResultSave";
            this.btnResultSave.Size = new System.Drawing.Size(172, 60);
            this.btnResultSave.TabIndex = 2;
            this.btnResultSave.TabStop = false;
            this.btnResultSave.Text = "Result Save";
            this.btnResultSave.UseVisualStyleBackColor = false;
            this.btnResultSave.Click += new System.EventHandler(this.btnResultSave_Click);
            // 
            // btnResultClear
            // 
            this.btnResultClear.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnResultClear.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnResultClear.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnResultClear.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnResultClear.CustomForeColor = System.Drawing.Color.Black;
            this.btnResultClear.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnResultClear.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnResultClear.ForeColor = System.Drawing.Color.Black;
            this.btnResultClear.ImageSize = new System.Drawing.Size(45, 45);
            this.btnResultClear.Location = new System.Drawing.Point(323, 72);
            this.btnResultClear.Margin = new System.Windows.Forms.Padding(4);
            this.btnResultClear.Name = "btnResultClear";
            this.btnResultClear.Size = new System.Drawing.Size(172, 60);
            this.btnResultClear.TabIndex = 1;
            this.btnResultClear.TabStop = false;
            this.btnResultClear.Text = "Result Clear";
            this.btnResultClear.UseVisualStyleBackColor = false;
            this.btnResultClear.Click += new System.EventHandler(this.btnResultClear_Click);
            // 
            // btnLastClear
            // 
            this.btnLastClear.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnLastClear.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnLastClear.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnLastClear.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnLastClear.CustomForeColor = System.Drawing.Color.Black;
            this.btnLastClear.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnLastClear.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnLastClear.ForeColor = System.Drawing.Color.Black;
            this.btnLastClear.ImageSize = new System.Drawing.Size(45, 45);
            this.btnLastClear.Location = new System.Drawing.Point(323, 4);
            this.btnLastClear.Margin = new System.Windows.Forms.Padding(4);
            this.btnLastClear.Name = "btnLastClear";
            this.btnLastClear.Size = new System.Drawing.Size(172, 60);
            this.btnLastClear.TabIndex = 0;
            this.btnLastClear.TabStop = false;
            this.btnLastClear.Text = "Last Clear";
            this.btnLastClear.UseVisualStyleBackColor = false;
            this.btnLastClear.Click += new System.EventHandler(this.btnLastClear_Click);
            // 
            // tableLayoutPanel7
            // 
            this.tableLayoutPanel7.ColumnCount = 1;
            this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel7.Controls.Add(this.btnProbeSafety, 0, 6);
            this.tableLayoutPanel7.Controls.Add(this.cbProbeIndex, 0, 4);
            this.tableLayoutPanel7.Controls.Add(this.btnTestStop, 0, 1);
            this.tableLayoutPanel7.Controls.Add(this.btnTestStart, 0, 0);
            this.tableLayoutPanel7.Controls.Add(this.grpContactMode, 0, 2);
            this.tableLayoutPanel7.Controls.Add(this.lblProbeIndex, 0, 3);
            this.tableLayoutPanel7.Controls.Add(this.btnProbeSeq, 0, 5);
            this.tableLayoutPanel7.Controls.Add(this.label1, 0, 7);
            this.tableLayoutPanel7.Controls.Add(this.btnTestMotionStart, 0, 8);
            this.tableLayoutPanel7.Controls.Add(this.btnTestMotionStop, 0, 9);
            this.tableLayoutPanel7.Location = new System.Drawing.Point(1671, 4);
            this.tableLayoutPanel7.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel7.Name = "tableLayoutPanel7";
            this.tableLayoutPanel7.RowCount = 12;
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 8.333333F));
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 8.333333F));
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 8.15739F));
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 4.03071F));
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 3.838772F));
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 7.677543F));
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 7.773512F));
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 4.126679F));
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 6.142035F));
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 6.046065F));
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 26.96737F));
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 8.333333F));
            this.tableLayoutPanel7.Size = new System.Drawing.Size(200, 1042);
            this.tableLayoutPanel7.TabIndex = 1;
            // 
            // btnProbeSafety
            // 
            this.btnProbeSafety.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnProbeSafety.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnProbeSafety.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnProbeSafety.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnProbeSafety.CustomForeColor = System.Drawing.Color.Black;
            this.btnProbeSafety.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnProbeSafety.ForeColor = System.Drawing.Color.Black;
            this.btnProbeSafety.ImageSize = new System.Drawing.Size(45, 45);
            this.btnProbeSafety.Location = new System.Drawing.Point(8, 429);
            this.btnProbeSafety.Margin = new System.Windows.Forms.Padding(8);
            this.btnProbeSafety.Name = "btnProbeSafety";
            this.btnProbeSafety.Size = new System.Drawing.Size(184, 60);
            this.btnProbeSafety.TabIndex = 19;
            this.btnProbeSafety.TabStop = false;
            this.btnProbeSafety.Text = "Safety Pos";
            this.btnProbeSafety.UseVisualStyleBackColor = false;
            this.btnProbeSafety.Click += new System.EventHandler(this.btnProbeSafety_Click);
            // 
            // cbProbeIndex
            // 
            this.cbProbeIndex.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbProbeIndex.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbProbeIndex.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8"});
            this.cbProbeIndex.Location = new System.Drawing.Point(3, 304);
            this.cbProbeIndex.Name = "cbProbeIndex";
            this.cbProbeIndex.Size = new System.Drawing.Size(194, 26);
            this.cbProbeIndex.TabIndex = 17;
            this.cbProbeIndex.SelectedIndexChanged += new System.EventHandler(this.cbProbeIndex_SelectedIndexChanged);
            // 
            // btnTestStop
            // 
            this.btnTestStop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnTestStop.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnTestStop.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnTestStop.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnTestStop.CustomForeColor = System.Drawing.Color.Black;
            this.btnTestStop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnTestStop.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnTestStop.ForeColor = System.Drawing.Color.Black;
            this.btnTestStop.ImageSize = new System.Drawing.Size(45, 45);
            this.btnTestStop.Location = new System.Drawing.Point(8, 95);
            this.btnTestStop.Margin = new System.Windows.Forms.Padding(8);
            this.btnTestStop.Name = "btnTestStop";
            this.btnTestStop.Size = new System.Drawing.Size(184, 71);
            this.btnTestStop.TabIndex = 2;
            this.btnTestStop.TabStop = false;
            this.btnTestStop.Text = "Test Stop";
            this.btnTestStop.UseVisualStyleBackColor = false;
            this.btnTestStop.Click += new System.EventHandler(this.btnTestStop_Click);
            // 
            // btnTestStart
            // 
            this.btnTestStart.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnTestStart.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnTestStart.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnTestStart.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnTestStart.CustomForeColor = System.Drawing.Color.Black;
            this.btnTestStart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnTestStart.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnTestStart.ForeColor = System.Drawing.Color.Black;
            this.btnTestStart.ImageSize = new System.Drawing.Size(45, 45);
            this.btnTestStart.Location = new System.Drawing.Point(8, 8);
            this.btnTestStart.Margin = new System.Windows.Forms.Padding(8);
            this.btnTestStart.Name = "btnTestStart";
            this.btnTestStart.Size = new System.Drawing.Size(184, 71);
            this.btnTestStart.TabIndex = 1;
            this.btnTestStart.TabStop = false;
            this.btnTestStart.Text = "Test Start";
            this.btnTestStart.UseVisualStyleBackColor = false;
            this.btnTestStart.Click += new System.EventHandler(this.btnTestStart_Click);
            // 
            // grpContactMode
            // 
            this.grpContactMode.Controls.Add(this.rbTop);
            this.grpContactMode.Controls.Add(this.rbBottom);
            this.grpContactMode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpContactMode.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.grpContactMode.Location = new System.Drawing.Point(3, 177);
            this.grpContactMode.Name = "grpContactMode";
            this.grpContactMode.Size = new System.Drawing.Size(194, 79);
            this.grpContactMode.TabIndex = 15;
            this.grpContactMode.TabStop = false;
            this.grpContactMode.Text = "Contact Mode";
            // 
            // rbTop
            // 
            this.rbTop.AutoSize = true;
            this.rbTop.Checked = true;
            this.rbTop.Location = new System.Drawing.Point(6, 30);
            this.rbTop.Name = "rbTop";
            this.rbTop.Size = new System.Drawing.Size(69, 29);
            this.rbTop.TabIndex = 0;
            this.rbTop.TabStop = true;
            this.rbTop.Text = "Top";
            this.rbTop.CheckedChanged += new System.EventHandler(this.rbTop_CheckedChanged);
            // 
            // rbBottom
            // 
            this.rbBottom.AutoSize = true;
            this.rbBottom.Location = new System.Drawing.Point(87, 30);
            this.rbBottom.Name = "rbBottom";
            this.rbBottom.Size = new System.Drawing.Size(101, 29);
            this.rbBottom.TabIndex = 1;
            this.rbBottom.Text = "Bottom";
            this.rbBottom.CheckedChanged += new System.EventHandler(this.rbBottom_CheckedChanged);
            // 
            // lblProbeIndex
            // 
            this.lblProbeIndex.AutoSize = true;
            this.lblProbeIndex.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblProbeIndex.Font = new System.Drawing.Font("맑은 고딕", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblProbeIndex.Location = new System.Drawing.Point(3, 259);
            this.lblProbeIndex.Name = "lblProbeIndex";
            this.lblProbeIndex.Size = new System.Drawing.Size(194, 42);
            this.lblProbeIndex.TabIndex = 16;
            this.lblProbeIndex.Text = "Socket";
            this.lblProbeIndex.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnProbeSeq
            // 
            this.btnProbeSeq.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnProbeSeq.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnProbeSeq.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnProbeSeq.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnProbeSeq.CustomForeColor = System.Drawing.Color.Black;
            this.btnProbeSeq.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnProbeSeq.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnProbeSeq.ForeColor = System.Drawing.Color.Black;
            this.btnProbeSeq.ImageSize = new System.Drawing.Size(45, 45);
            this.btnProbeSeq.Location = new System.Drawing.Point(8, 353);
            this.btnProbeSeq.Margin = new System.Windows.Forms.Padding(8);
            this.btnProbeSeq.Name = "btnProbeSeq";
            this.btnProbeSeq.Size = new System.Drawing.Size(184, 60);
            this.btnProbeSeq.TabIndex = 18;
            this.btnProbeSeq.TabStop = false;
            this.btnProbeSeq.Text = "Probe Pos";
            this.btnProbeSeq.UseVisualStyleBackColor = false;
            this.btnProbeSeq.Click += new System.EventHandler(this.btnProbeSeq_Click);
            // 
            // btnTestMotionStart
            // 
            this.btnTestMotionStart.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnTestMotionStart.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnTestMotionStart.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnTestMotionStart.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnTestMotionStart.CustomForeColor = System.Drawing.Color.Black;
            this.btnTestMotionStart.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnTestMotionStart.ForeColor = System.Drawing.Color.Black;
            this.btnTestMotionStart.ImageSize = new System.Drawing.Size(45, 45);
            this.btnTestMotionStart.Location = new System.Drawing.Point(8, 553);
            this.btnTestMotionStart.Margin = new System.Windows.Forms.Padding(8);
            this.btnTestMotionStart.Name = "btnTestMotionStart";
            this.btnTestMotionStart.Size = new System.Drawing.Size(184, 48);
            this.btnTestMotionStart.TabIndex = 20;
            this.btnTestMotionStart.TabStop = false;
            this.btnTestMotionStart.Text = "Test Start";
            this.btnTestMotionStart.UseVisualStyleBackColor = false;
            this.btnTestMotionStart.Click += new System.EventHandler(this.btnTestMotionStart_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.label1.Font = new System.Drawing.Font("맑은 고딕", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label1.Location = new System.Drawing.Point(3, 515);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(194, 30);
            this.label1.TabIndex = 21;
            this.label1.Text = "Motion+Measure";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnTestMotionStop
            // 
            this.btnTestMotionStop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnTestMotionStop.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnTestMotionStop.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnTestMotionStop.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnTestMotionStop.CustomForeColor = System.Drawing.Color.Black;
            this.btnTestMotionStop.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnTestMotionStop.ForeColor = System.Drawing.Color.Black;
            this.btnTestMotionStop.ImageSize = new System.Drawing.Size(45, 45);
            this.btnTestMotionStop.Location = new System.Drawing.Point(8, 617);
            this.btnTestMotionStop.Margin = new System.Windows.Forms.Padding(8);
            this.btnTestMotionStop.Name = "btnTestMotionStop";
            this.btnTestMotionStop.Size = new System.Drawing.Size(184, 47);
            this.btnTestMotionStop.TabIndex = 22;
            this.btnTestMotionStop.TabStop = false;
            this.btnTestMotionStop.Text = "Test Stop";
            this.btnTestMotionStop.UseVisualStyleBackColor = false;
            this.btnTestMotionStop.Click += new System.EventHandler(this.btnTestMotionStop_Click);
            // 
            // CellTesterPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "CellTesterPage";
            this.Size = new System.Drawing.Size(1875, 1050);
            this.Load += new System.EventHandler(this.CellTesterPage_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridResult)).EndInit();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel5.ResumeLayout(false);
            this.tableLayoutPanel8.ResumeLayout(false);
            this.tableLayoutPanel9.ResumeLayout(false);
            this.tableLayoutPanel9.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudIntervalDelay)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRepeatCount)).EndInit();
            this.panel1.ResumeLayout(false);
            this.tableLayoutPanel6.ResumeLayout(false);
            this.tableLayoutPanel10.ResumeLayout(false);
            this.tableLayoutPanel10.PerformLayout();
            this.tableLayoutPanel7.ResumeLayout(false);
            this.tableLayoutPanel7.PerformLayout();
            this.grpContactMode.ResumeLayout(false);
            this.grpContactMode.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.DataGridView dataGridResult;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private Common.Spectrometer.CASSpectrumViewer casSpectrumViewer;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private Common.RadioButtonView rbvOption;
        private Common.CustomControl.CustomBorderLabel lbResultValue;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel7;
        private Common.IndividualMenuButton btnTestStop;
        private Common.IndividualMenuButton btnTestStart;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel6;
        private Common.IndividualMenuButton btnResultSave;
        private Common.IndividualMenuButton btnResultClear;
        private Common.IndividualMenuButton btnLastClear;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel8;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel9;
        private Common.CustomControl.CustomBorderLabel lbStatusCaption;
        private System.Windows.Forms.NumericUpDown nudRepeatCount;
        private System.Windows.Forms.NumericUpDown nudIntervalDelay;
        private Common.CustomControl.CustomBorderLabel customBorderLabel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel10;
        private System.Windows.Forms.Label lbCurrentIndexNo;
        private System.Windows.Forms.Label lbMeasureTime;
        private System.Windows.Forms.GroupBox grpContactMode;
        private System.Windows.Forms.RadioButton rbTop;
        private System.Windows.Forms.RadioButton rbBottom;
        private System.Windows.Forms.Label lblProbeIndex;
        private System.Windows.Forms.ComboBox cbProbeIndex;
        private Common.IndividualMenuButton btnProbeSafety;
        private Common.IndividualMenuButton btnProbeSeq;
        private Common.IndividualMenuButton btnTestMotionStart;
        private System.Windows.Forms.Label label1;
        private Common.IndividualMenuButton btnTestMotionStop;
    }
}
