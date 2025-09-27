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
            this.btnResultSave = new QMC.Common.IndividualMenuButton();
            this.btnResultClear = new QMC.Common.IndividualMenuButton();
            this.btnLastClear = new QMC.Common.IndividualMenuButton();
            this.tableLayoutPanel7 = new System.Windows.Forms.TableLayoutPanel();
            this.btnTestStop = new QMC.Common.IndividualMenuButton();
            this.btnTestStart = new QMC.Common.IndividualMenuButton();
            this.lbMeasureTime = new System.Windows.Forms.Label();
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
            this.tableLayoutPanel7.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 139F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel7, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1250, 700);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.dataGridResult, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel3, 0, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 64.55331F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 35.44669F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(1105, 694);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // dataGridResult
            // 
            this.dataGridResult.AllowUserToResizeColumns = false;
            this.dataGridResult.AllowUserToResizeRows = false;
            this.dataGridResult.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridResult.Location = new System.Drawing.Point(3, 3);
            this.dataGridResult.MultiSelect = false;
            this.dataGridResult.Name = "dataGridResult";
            this.dataGridResult.ReadOnly = true;
            this.dataGridResult.RowHeadersWidth = 80;
            this.dataGridResult.RowTemplate.Height = 23;
            this.dataGridResult.Size = new System.Drawing.Size(1099, 441);
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
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 450);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(1099, 241);
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
            this.tableLayoutPanel4.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 1;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(755, 235);
            this.tableLayoutPanel4.TabIndex = 0;
            // 
            // casSpectrumViewer
            // 
            this.casSpectrumViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.casSpectrumViewer.Location = new System.Drawing.Point(3, 3);
            this.casSpectrumViewer.Name = "casSpectrumViewer";
            this.casSpectrumViewer.Size = new System.Drawing.Size(437, 229);
            this.casSpectrumViewer.TabIndex = 0;
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 1;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel5.Controls.Add(this.lbResultValue, 0, 0);
            this.tableLayoutPanel5.Controls.Add(this.tableLayoutPanel8, 0, 1);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(446, 3);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 2;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 27.07424F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 72.92577F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(306, 229);
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
            this.lbResultValue.Location = new System.Drawing.Point(3, 3);
            this.lbResultValue.Margin = new System.Windows.Forms.Padding(3);
            this.lbResultValue.Name = "lbResultValue";
            this.lbResultValue.Size = new System.Drawing.Size(300, 56);
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
            this.tableLayoutPanel8.Location = new System.Drawing.Point(3, 65);
            this.tableLayoutPanel8.Name = "tableLayoutPanel8";
            this.tableLayoutPanel8.RowCount = 2;
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 70F));
            this.tableLayoutPanel8.Size = new System.Drawing.Size(300, 161);
            this.tableLayoutPanel8.TabIndex = 23;
            // 
            // rbvOption
            // 
            this.rbvOption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rbvOption.Font = new System.Drawing.Font("맑은 고딕", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.rbvOption.GroupName = "Repeat Mode";
            this.rbvOption.Location = new System.Drawing.Point(4, 5);
            this.rbvOption.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.rbvOption.Name = "rbvOption";
            this.rbvOption.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.rbvOption.SelectedIndex = -1;
            this.rbvOption.Size = new System.Drawing.Size(292, 81);
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
            this.tableLayoutPanel9.Location = new System.Drawing.Point(0, 94);
            this.tableLayoutPanel9.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.tableLayoutPanel9.Name = "tableLayoutPanel9";
            this.tableLayoutPanel9.RowCount = 2;
            this.tableLayoutPanel9.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel9.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel9.Size = new System.Drawing.Size(300, 64);
            this.tableLayoutPanel9.TabIndex = 1;
            // 
            // nudIntervalDelay
            // 
            this.nudIntervalDelay.AutoSize = true;
            this.nudIntervalDelay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nudIntervalDelay.Font = new System.Drawing.Font("맑은 고딕", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nudIntervalDelay.Location = new System.Drawing.Point(150, 35);
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
            this.nudIntervalDelay.Size = new System.Drawing.Size(147, 27);
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
            this.customBorderLabel1.Location = new System.Drawing.Point(0, 35);
            this.customBorderLabel1.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.customBorderLabel1.Name = "customBorderLabel1";
            this.customBorderLabel1.Size = new System.Drawing.Size(147, 26);
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
            this.lbStatusCaption.Location = new System.Drawing.Point(0, 3);
            this.lbStatusCaption.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.lbStatusCaption.Name = "lbStatusCaption";
            this.lbStatusCaption.Size = new System.Drawing.Size(147, 26);
            this.lbStatusCaption.TabIndex = 20;
            this.lbStatusCaption.Text = "Repeat Count (cnt)";
            this.lbStatusCaption.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // nudRepeatCount
            // 
            this.nudRepeatCount.AutoSize = true;
            this.nudRepeatCount.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nudRepeatCount.Font = new System.Drawing.Font("맑은 고딕", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nudRepeatCount.Location = new System.Drawing.Point(150, 3);
            this.nudRepeatCount.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudRepeatCount.Name = "nudRepeatCount";
            this.nudRepeatCount.Size = new System.Drawing.Size(147, 27);
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
            this.panel1.Location = new System.Drawing.Point(764, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(332, 235);
            this.panel1.TabIndex = 1;
            // 
            // tableLayoutPanel6
            // 
            this.tableLayoutPanel6.ColumnCount = 2;
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tableLayoutPanel6.Controls.Add(this.btnResultSave, 1, 2);
            this.tableLayoutPanel6.Controls.Add(this.btnResultClear, 1, 1);
            this.tableLayoutPanel6.Controls.Add(this.btnLastClear, 1, 0);
            this.tableLayoutPanel6.Controls.Add(this.lbMeasureTime, 0, 0);
            this.tableLayoutPanel6.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanel6.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel6.Name = "tableLayoutPanel6";
            this.tableLayoutPanel6.RowCount = 3;
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel6.Size = new System.Drawing.Size(332, 136);
            this.tableLayoutPanel6.TabIndex = 3;
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
            this.btnResultSave.Location = new System.Drawing.Point(215, 93);
            this.btnResultSave.Name = "btnResultSave";
            this.btnResultSave.Size = new System.Drawing.Size(114, 40);
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
            this.btnResultClear.Location = new System.Drawing.Point(215, 48);
            this.btnResultClear.Name = "btnResultClear";
            this.btnResultClear.Size = new System.Drawing.Size(114, 39);
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
            this.btnLastClear.Location = new System.Drawing.Point(215, 3);
            this.btnLastClear.Name = "btnLastClear";
            this.btnLastClear.Size = new System.Drawing.Size(114, 39);
            this.btnLastClear.TabIndex = 0;
            this.btnLastClear.TabStop = false;
            this.btnLastClear.Text = "Last Clear";
            this.btnLastClear.UseVisualStyleBackColor = false;
            this.btnLastClear.Click += new System.EventHandler(this.btnLastClear_Click);
            // 
            // tableLayoutPanel7
            // 
            this.tableLayoutPanel7.ColumnCount = 1;
            this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel7.Controls.Add(this.btnTestStop, 0, 1);
            this.tableLayoutPanel7.Controls.Add(this.btnTestStart, 0, 0);
            this.tableLayoutPanel7.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanel7.Location = new System.Drawing.Point(1114, 3);
            this.tableLayoutPanel7.Name = "tableLayoutPanel7";
            this.tableLayoutPanel7.RowCount = 2;
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel7.Size = new System.Drawing.Size(133, 101);
            this.tableLayoutPanel7.TabIndex = 1;
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
            this.btnTestStop.Location = new System.Drawing.Point(5, 55);
            this.btnTestStop.Margin = new System.Windows.Forms.Padding(5);
            this.btnTestStop.Name = "btnTestStop";
            this.btnTestStop.Size = new System.Drawing.Size(123, 41);
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
            this.btnTestStart.Location = new System.Drawing.Point(5, 5);
            this.btnTestStart.Margin = new System.Windows.Forms.Padding(5);
            this.btnTestStart.Name = "btnTestStart";
            this.btnTestStart.Size = new System.Drawing.Size(123, 40);
            this.btnTestStart.TabIndex = 1;
            this.btnTestStart.TabStop = false;
            this.btnTestStart.Text = "Test Start";
            this.btnTestStart.UseVisualStyleBackColor = false;
            this.btnTestStart.Click += new System.EventHandler(this.btnTestStart_Click);
            // 
            // lbMeasureTime
            // 
            this.lbMeasureTime.AutoSize = true;
            this.lbMeasureTime.Dock = System.Windows.Forms.DockStyle.Top;
            this.lbMeasureTime.Location = new System.Drawing.Point(3, 0);
            this.lbMeasureTime.Name = "lbMeasureTime";
            this.lbMeasureTime.Padding = new System.Windows.Forms.Padding(0, 7, 0, 0);
            this.lbMeasureTime.Size = new System.Drawing.Size(206, 19);
            this.lbMeasureTime.TabIndex = 3;
            this.lbMeasureTime.Text = "Measure Time: -";
            // 
            // CellTesterPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "CellTesterPage";
            this.Size = new System.Drawing.Size(1250, 700);
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
            this.tableLayoutPanel6.PerformLayout();
            this.tableLayoutPanel7.ResumeLayout(false);
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
        private System.Windows.Forms.Label lbMeasureTime;
    }
}
