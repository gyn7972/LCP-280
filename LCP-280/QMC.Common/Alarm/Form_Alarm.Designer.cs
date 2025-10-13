using System.Drawing;
using System.Windows.Forms;

namespace QMC.Common.Alarm

{
    partial class Form_Alarm
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.groupBoxSelectedAlarmDetails = new System.Windows.Forms.GroupBox();
            this.baseTextBoxAlarmTitle = new System.Windows.Forms.TextBox();
            this.baseLabelGrade = new System.Windows.Forms.Label();
            this.baseLabelAlarmTitle = new System.Windows.Forms.Label();
            this.baseTextBoxSource = new System.Windows.Forms.TextBox();
            this.baseTextBoxCode = new System.Windows.Forms.TextBox();
            this.baseLabelCode = new System.Windows.Forms.Label();
            this.baseTextBoxCause = new System.Windows.Forms.TextBox();
            this.baseLabelCause = new System.Windows.Forms.Label();
            this.baseTextBoxGrade = new System.Windows.Forms.TextBox();
            this.groupBoxCellFocusOption = new System.Windows.Forms.GroupBox();
            this.radioButtonLastCell = new System.Windows.Forms.RadioButton();
            this.radioButtonUserSelectedCell = new System.Windows.Forms.RadioButton();
            this.groupBoxRecovery = new System.Windows.Forms.GroupBox();
            this.button_Alarm_Buzz_Off = new System.Windows.Forms.Button();
            this.panelComfirm = new System.Windows.Forms.Panel();
            this.baseLabelSource = new System.Windows.Forms.Label();
            this.baseDataGridViewAlarm = new System.Windows.Forms.DataGridView();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel6 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel7 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel8 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel10 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel11 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel9 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel12 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel13 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBoxSelectedAlarmDetails.SuspendLayout();
            this.groupBoxCellFocusOption.SuspendLayout();
            this.groupBoxRecovery.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.baseDataGridViewAlarm)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            this.tableLayoutPanel6.SuspendLayout();
            this.tableLayoutPanel7.SuspendLayout();
            this.tableLayoutPanel8.SuspendLayout();
            this.tableLayoutPanel10.SuspendLayout();
            this.tableLayoutPanel11.SuspendLayout();
            this.tableLayoutPanel9.SuspendLayout();
            this.tableLayoutPanel12.SuspendLayout();
            this.tableLayoutPanel13.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxSelectedAlarmDetails
            // 
            this.groupBoxSelectedAlarmDetails.Controls.Add(this.tableLayoutPanel1);
            this.groupBoxSelectedAlarmDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxSelectedAlarmDetails.Font = new System.Drawing.Font("Tahoma", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBoxSelectedAlarmDetails.ForeColor = System.Drawing.Color.Black;
            this.groupBoxSelectedAlarmDetails.Location = new System.Drawing.Point(0, 0);
            this.groupBoxSelectedAlarmDetails.Margin = new System.Windows.Forms.Padding(4);
            this.groupBoxSelectedAlarmDetails.Name = "groupBoxSelectedAlarmDetails";
            this.groupBoxSelectedAlarmDetails.Padding = new System.Windows.Forms.Padding(4);
            this.groupBoxSelectedAlarmDetails.Size = new System.Drawing.Size(869, 501);
            this.groupBoxSelectedAlarmDetails.TabIndex = 5;
            this.groupBoxSelectedAlarmDetails.TabStop = false;
            this.groupBoxSelectedAlarmDetails.Text = " Selected Alarm Details ";
            // 
            // baseTextBoxAlarmTitle
            // 
            this.baseTextBoxAlarmTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.baseTextBoxAlarmTitle.BackColor = System.Drawing.Color.White;
            this.baseTextBoxAlarmTitle.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.baseTextBoxAlarmTitle.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.baseTextBoxAlarmTitle.ForeColor = System.Drawing.Color.Black;
            this.baseTextBoxAlarmTitle.Location = new System.Drawing.Point(112, 4);
            this.baseTextBoxAlarmTitle.Margin = new System.Windows.Forms.Padding(4);
            this.baseTextBoxAlarmTitle.Multiline = true;
            this.baseTextBoxAlarmTitle.Name = "baseTextBoxAlarmTitle";
            this.baseTextBoxAlarmTitle.Size = new System.Drawing.Size(427, 33);
            this.baseTextBoxAlarmTitle.TabIndex = 19;
            // 
            // baseLabelGrade
            // 
            this.baseLabelGrade.Dock = System.Windows.Forms.DockStyle.Fill;
            this.baseLabelGrade.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.baseLabelGrade.ForeColor = System.Drawing.Color.Black;
            this.baseLabelGrade.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.baseLabelGrade.Location = new System.Drawing.Point(3, 3);
            this.baseLabelGrade.Margin = new System.Windows.Forms.Padding(3);
            this.baseLabelGrade.Name = "baseLabelGrade";
            this.baseLabelGrade.Size = new System.Drawing.Size(102, 35);
            this.baseLabelGrade.TabIndex = 5;
            this.baseLabelGrade.Text = "Grade";
            this.baseLabelGrade.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // baseLabelAlarmTitle
            // 
            this.baseLabelAlarmTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.baseLabelAlarmTitle.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.baseLabelAlarmTitle.ForeColor = System.Drawing.Color.Black;
            this.baseLabelAlarmTitle.Location = new System.Drawing.Point(3, 3);
            this.baseLabelAlarmTitle.Margin = new System.Windows.Forms.Padding(3);
            this.baseLabelAlarmTitle.Name = "baseLabelAlarmTitle";
            this.baseLabelAlarmTitle.Size = new System.Drawing.Size(102, 35);
            this.baseLabelAlarmTitle.TabIndex = 4;
            this.baseLabelAlarmTitle.Text = "Title";
            this.baseLabelAlarmTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // baseTextBoxSource
            // 
            this.baseTextBoxSource.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.baseTextBoxSource.BackColor = System.Drawing.Color.White;
            this.baseTextBoxSource.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.baseTextBoxSource.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.baseTextBoxSource.ForeColor = System.Drawing.Color.Black;
            this.baseTextBoxSource.Location = new System.Drawing.Point(112, 4);
            this.baseTextBoxSource.Margin = new System.Windows.Forms.Padding(4);
            this.baseTextBoxSource.Multiline = true;
            this.baseTextBoxSource.Name = "baseTextBoxSource";
            this.baseTextBoxSource.Size = new System.Drawing.Size(427, 64);
            this.baseTextBoxSource.TabIndex = 17;
            // 
            // baseTextBoxCode
            // 
            this.baseTextBoxCode.BackColor = System.Drawing.Color.White;
            this.baseTextBoxCode.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.baseTextBoxCode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.baseTextBoxCode.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.baseTextBoxCode.ForeColor = System.Drawing.Color.Black;
            this.baseTextBoxCode.Location = new System.Drawing.Point(66, 4);
            this.baseTextBoxCode.Margin = new System.Windows.Forms.Padding(4);
            this.baseTextBoxCode.Multiline = true;
            this.baseTextBoxCode.Name = "baseTextBoxCode";
            this.baseTextBoxCode.Size = new System.Drawing.Size(139, 21);
            this.baseTextBoxCode.TabIndex = 19;
            // 
            // baseLabelCode
            // 
            this.baseLabelCode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.baseLabelCode.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.baseLabelCode.ForeColor = System.Drawing.Color.Black;
            this.baseLabelCode.Location = new System.Drawing.Point(3, 3);
            this.baseLabelCode.Margin = new System.Windows.Forms.Padding(3);
            this.baseLabelCode.Name = "baseLabelCode";
            this.baseLabelCode.Size = new System.Drawing.Size(56, 23);
            this.baseLabelCode.TabIndex = 14;
            this.baseLabelCode.Text = "Code";
            this.baseLabelCode.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // baseTextBoxCause
            // 
            this.baseTextBoxCause.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.baseTextBoxCause.BackColor = System.Drawing.Color.White;
            this.baseTextBoxCause.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.baseTextBoxCause.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.baseTextBoxCause.ForeColor = System.Drawing.Color.Black;
            this.baseTextBoxCause.Location = new System.Drawing.Point(112, 4);
            this.baseTextBoxCause.Margin = new System.Windows.Forms.Padding(4);
            this.baseTextBoxCause.Multiline = true;
            this.baseTextBoxCause.Name = "baseTextBoxCause";
            this.baseTextBoxCause.Size = new System.Drawing.Size(427, 129);
            this.baseTextBoxCause.TabIndex = 16;
            // 
            // baseLabelCause
            // 
            this.baseLabelCause.Dock = System.Windows.Forms.DockStyle.Fill;
            this.baseLabelCause.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.baseLabelCause.ForeColor = System.Drawing.Color.Black;
            this.baseLabelCause.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.baseLabelCause.Location = new System.Drawing.Point(3, 3);
            this.baseLabelCause.Margin = new System.Windows.Forms.Padding(3);
            this.baseLabelCause.Name = "baseLabelCause";
            this.baseLabelCause.Size = new System.Drawing.Size(102, 131);
            this.baseLabelCause.TabIndex = 7;
            this.baseLabelCause.Text = "Cause";
            this.baseLabelCause.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // baseTextBoxGrade
            // 
            this.baseTextBoxGrade.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.baseTextBoxGrade.BackColor = System.Drawing.Color.White;
            this.baseTextBoxGrade.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.baseTextBoxGrade.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.baseTextBoxGrade.ForeColor = System.Drawing.Color.Black;
            this.baseTextBoxGrade.Location = new System.Drawing.Point(4, 4);
            this.baseTextBoxGrade.Margin = new System.Windows.Forms.Padding(4);
            this.baseTextBoxGrade.Multiline = true;
            this.baseTextBoxGrade.Name = "baseTextBoxGrade";
            this.baseTextBoxGrade.Size = new System.Drawing.Size(206, 27);
            this.baseTextBoxGrade.TabIndex = 18;
            // 
            // groupBoxCellFocusOption
            // 
            this.groupBoxCellFocusOption.Controls.Add(this.tableLayoutPanel13);
            this.groupBoxCellFocusOption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxCellFocusOption.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBoxCellFocusOption.ForeColor = System.Drawing.Color.Black;
            this.groupBoxCellFocusOption.Location = new System.Drawing.Point(4, 224);
            this.groupBoxCellFocusOption.Margin = new System.Windows.Forms.Padding(4);
            this.groupBoxCellFocusOption.Name = "groupBoxCellFocusOption";
            this.groupBoxCellFocusOption.Padding = new System.Windows.Forms.Padding(4);
            this.groupBoxCellFocusOption.Size = new System.Drawing.Size(286, 87);
            this.groupBoxCellFocusOption.TabIndex = 3;
            this.groupBoxCellFocusOption.TabStop = false;
            this.groupBoxCellFocusOption.Text = " Cell Focus Option ";
            // 
            // radioButtonLastCell
            // 
            this.radioButtonLastCell.AutoSize = true;
            this.radioButtonLastCell.Dock = System.Windows.Forms.DockStyle.Fill;
            this.radioButtonLastCell.Location = new System.Drawing.Point(4, 33);
            this.radioButtonLastCell.Margin = new System.Windows.Forms.Padding(4);
            this.radioButtonLastCell.Name = "radioButtonLastCell";
            this.radioButtonLastCell.Size = new System.Drawing.Size(270, 22);
            this.radioButtonLastCell.TabIndex = 1;
            this.radioButtonLastCell.TabStop = true;
            this.radioButtonLastCell.Text = "Last Cell";
            this.radioButtonLastCell.UseVisualStyleBackColor = true;
            // 
            // radioButtonUserSelectedCell
            // 
            this.radioButtonUserSelectedCell.AutoSize = true;
            this.radioButtonUserSelectedCell.Dock = System.Windows.Forms.DockStyle.Fill;
            this.radioButtonUserSelectedCell.Location = new System.Drawing.Point(4, 4);
            this.radioButtonUserSelectedCell.Margin = new System.Windows.Forms.Padding(4);
            this.radioButtonUserSelectedCell.Name = "radioButtonUserSelectedCell";
            this.radioButtonUserSelectedCell.Size = new System.Drawing.Size(270, 21);
            this.radioButtonUserSelectedCell.TabIndex = 0;
            this.radioButtonUserSelectedCell.TabStop = true;
            this.radioButtonUserSelectedCell.Text = "User Selected Cell";
            this.radioButtonUserSelectedCell.UseVisualStyleBackColor = true;
            // 
            // groupBoxRecovery
            // 
            this.groupBoxRecovery.Controls.Add(this.tableLayoutPanel12);
            this.groupBoxRecovery.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxRecovery.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBoxRecovery.ForeColor = System.Drawing.Color.Black;
            this.groupBoxRecovery.Location = new System.Drawing.Point(4, 4);
            this.groupBoxRecovery.Margin = new System.Windows.Forms.Padding(4);
            this.groupBoxRecovery.Name = "groupBoxRecovery";
            this.groupBoxRecovery.Padding = new System.Windows.Forms.Padding(4);
            this.groupBoxRecovery.Size = new System.Drawing.Size(286, 212);
            this.groupBoxRecovery.TabIndex = 2;
            this.groupBoxRecovery.TabStop = false;
            this.groupBoxRecovery.Text = " Recovery ";
            // 
            // button_Alarm_Buzz_Off
            // 
            this.button_Alarm_Buzz_Off.Dock = System.Windows.Forms.DockStyle.Fill;
            this.button_Alarm_Buzz_Off.Font = new System.Drawing.Font("Tahoma", 15F, System.Drawing.FontStyle.Bold);
            this.button_Alarm_Buzz_Off.Location = new System.Drawing.Point(3, 131);
            this.button_Alarm_Buzz_Off.Name = "button_Alarm_Buzz_Off";
            this.button_Alarm_Buzz_Off.Size = new System.Drawing.Size(272, 50);
            this.button_Alarm_Buzz_Off.TabIndex = 1;
            this.button_Alarm_Buzz_Off.Text = "부저 정지";
            this.button_Alarm_Buzz_Off.UseVisualStyleBackColor = true;
            this.button_Alarm_Buzz_Off.Click += new System.EventHandler(this.button_Alarm_Buzz_Off_Click);
            // 
            // panelComfirm
            // 
            this.panelComfirm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelComfirm.Location = new System.Drawing.Point(4, 4);
            this.panelComfirm.Margin = new System.Windows.Forms.Padding(4);
            this.panelComfirm.Name = "panelComfirm";
            this.panelComfirm.Size = new System.Drawing.Size(270, 120);
            this.panelComfirm.TabIndex = 0;
            // 
            // baseLabelSource
            // 
            this.baseLabelSource.Dock = System.Windows.Forms.DockStyle.Fill;
            this.baseLabelSource.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.baseLabelSource.ForeColor = System.Drawing.Color.Black;
            this.baseLabelSource.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.baseLabelSource.Location = new System.Drawing.Point(3, 3);
            this.baseLabelSource.Margin = new System.Windows.Forms.Padding(3);
            this.baseLabelSource.Name = "baseLabelSource";
            this.baseLabelSource.Size = new System.Drawing.Size(102, 66);
            this.baseLabelSource.TabIndex = 6;
            this.baseLabelSource.Text = "Source";
            this.baseLabelSource.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // baseDataGridViewAlarm
            // 
            this.baseDataGridViewAlarm.AllowUserToAddRows = false;
            this.baseDataGridViewAlarm.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.baseDataGridViewAlarm.BackgroundColor = System.Drawing.Color.White;
            this.baseDataGridViewAlarm.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(78)))), ((int)(((byte)(78)))), ((int)(((byte)(78)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Tahoma", 12F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(78)))), ((int)(((byte)(78)))), ((int)(((byte)(78)))));
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.baseDataGridViewAlarm.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.baseDataGridViewAlarm.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Tahoma", 12F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.baseDataGridViewAlarm.DefaultCellStyle = dataGridViewCellStyle2;
            this.baseDataGridViewAlarm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.baseDataGridViewAlarm.Font = new System.Drawing.Font("Tahoma", 12F);
            this.baseDataGridViewAlarm.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(3)))), ((int)(((byte)(3)))), ((int)(((byte)(3)))));
            this.baseDataGridViewAlarm.Location = new System.Drawing.Point(4, 4);
            this.baseDataGridViewAlarm.Margin = new System.Windows.Forms.Padding(4);
            this.baseDataGridViewAlarm.Name = "baseDataGridViewAlarm";
            this.baseDataGridViewAlarm.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(78)))), ((int)(((byte)(78)))), ((int)(((byte)(78)))));
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Tahoma", 12F);
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(78)))), ((int)(((byte)(78)))), ((int)(((byte)(78)))));
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.baseDataGridViewAlarm.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.baseDataGridViewAlarm.RowHeadersVisible = false;
            this.baseDataGridViewAlarm.RowHeadersWidth = 62;
            this.baseDataGridViewAlarm.RowTemplate.Height = 23;
            this.baseDataGridViewAlarm.Size = new System.Drawing.Size(847, 127);
            this.baseDataGridViewAlarm.TabIndex = 6;
            this.baseDataGridViewAlarm.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.baseDataGridViewAlarm_CellClick);
            this.baseDataGridViewAlarm.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.baseDataGridViewAlarm_CellContentClick);
            this.baseDataGridViewAlarm.RowsAdded += new System.Windows.Forms.DataGridViewRowsAddedEventHandler(this.baseDataGridViewAlarm_RowsAdded);
            this.baseDataGridViewAlarm.RowsRemoved += new System.Windows.Forms.DataGridViewRowsRemovedEventHandler(this.baseDataGridViewAlarm_RowsRemoved);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel3, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(4, 29);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(861, 468);
            this.tableLayoutPanel1.TabIndex = 7;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.baseDataGridViewAlarm, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 330);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(855, 135);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 65F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 35F));
            this.tableLayoutPanel3.Controls.Add(this.tableLayoutPanel4, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.tableLayoutPanel9, 1, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(855, 321);
            this.tableLayoutPanel3.TabIndex = 1;
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 1;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.Controls.Add(this.tableLayoutPanel8, 0, 2);
            this.tableLayoutPanel4.Controls.Add(this.tableLayoutPanel7, 0, 1);
            this.tableLayoutPanel4.Controls.Add(this.tableLayoutPanel6, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.tableLayoutPanel5, 0, 3);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 4;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 45F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(549, 315);
            this.tableLayoutPanel4.TabIndex = 8;
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 2;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 80F));
            this.tableLayoutPanel5.Controls.Add(this.baseLabelCause, 0, 0);
            this.tableLayoutPanel5.Controls.Add(this.baseTextBoxCause, 1, 0);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(3, 175);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 1;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(543, 137);
            this.tableLayoutPanel5.TabIndex = 0;
            // 
            // tableLayoutPanel6
            // 
            this.tableLayoutPanel6.ColumnCount = 2;
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 80F));
            this.tableLayoutPanel6.Controls.Add(this.baseTextBoxAlarmTitle, 1, 0);
            this.tableLayoutPanel6.Controls.Add(this.baseLabelAlarmTitle, 0, 0);
            this.tableLayoutPanel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel6.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel6.Name = "tableLayoutPanel6";
            this.tableLayoutPanel6.RowCount = 1;
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel6.Size = new System.Drawing.Size(543, 41);
            this.tableLayoutPanel6.TabIndex = 1;
            // 
            // tableLayoutPanel7
            // 
            this.tableLayoutPanel7.ColumnCount = 2;
            this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 80F));
            this.tableLayoutPanel7.Controls.Add(this.tableLayoutPanel10, 1, 0);
            this.tableLayoutPanel7.Controls.Add(this.baseLabelGrade, 0, 0);
            this.tableLayoutPanel7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel7.Location = new System.Drawing.Point(3, 50);
            this.tableLayoutPanel7.Name = "tableLayoutPanel7";
            this.tableLayoutPanel7.RowCount = 1;
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel7.Size = new System.Drawing.Size(543, 41);
            this.tableLayoutPanel7.TabIndex = 2;
            // 
            // tableLayoutPanel8
            // 
            this.tableLayoutPanel8.ColumnCount = 2;
            this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 80F));
            this.tableLayoutPanel8.Controls.Add(this.baseLabelSource, 0, 0);
            this.tableLayoutPanel8.Controls.Add(this.baseTextBoxSource, 1, 0);
            this.tableLayoutPanel8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel8.Location = new System.Drawing.Point(3, 97);
            this.tableLayoutPanel8.Name = "tableLayoutPanel8";
            this.tableLayoutPanel8.RowCount = 1;
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel8.Size = new System.Drawing.Size(543, 72);
            this.tableLayoutPanel8.TabIndex = 3;
            // 
            // tableLayoutPanel10
            // 
            this.tableLayoutPanel10.ColumnCount = 2;
            this.tableLayoutPanel10.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel10.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel10.Controls.Add(this.tableLayoutPanel11, 1, 0);
            this.tableLayoutPanel10.Controls.Add(this.baseTextBoxGrade, 0, 0);
            this.tableLayoutPanel10.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel10.Location = new System.Drawing.Point(111, 3);
            this.tableLayoutPanel10.Name = "tableLayoutPanel10";
            this.tableLayoutPanel10.RowCount = 1;
            this.tableLayoutPanel10.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel10.Size = new System.Drawing.Size(429, 35);
            this.tableLayoutPanel10.TabIndex = 5;
            // 
            // tableLayoutPanel11
            // 
            this.tableLayoutPanel11.ColumnCount = 2;
            this.tableLayoutPanel11.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel11.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.tableLayoutPanel11.Controls.Add(this.baseLabelCode, 0, 0);
            this.tableLayoutPanel11.Controls.Add(this.baseTextBoxCode, 1, 0);
            this.tableLayoutPanel11.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel11.Location = new System.Drawing.Point(217, 3);
            this.tableLayoutPanel11.Name = "tableLayoutPanel11";
            this.tableLayoutPanel11.RowCount = 1;
            this.tableLayoutPanel11.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel11.Size = new System.Drawing.Size(209, 29);
            this.tableLayoutPanel11.TabIndex = 5;
            // 
            // tableLayoutPanel9
            // 
            this.tableLayoutPanel9.ColumnCount = 1;
            this.tableLayoutPanel9.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel9.Controls.Add(this.groupBoxRecovery, 0, 0);
            this.tableLayoutPanel9.Controls.Add(this.groupBoxCellFocusOption, 0, 1);
            this.tableLayoutPanel9.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel9.Location = new System.Drawing.Point(558, 3);
            this.tableLayoutPanel9.Name = "tableLayoutPanel9";
            this.tableLayoutPanel9.RowCount = 2;
            this.tableLayoutPanel9.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.tableLayoutPanel9.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel9.Size = new System.Drawing.Size(294, 315);
            this.tableLayoutPanel9.TabIndex = 9;
            // 
            // tableLayoutPanel12
            // 
            this.tableLayoutPanel12.ColumnCount = 1;
            this.tableLayoutPanel12.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel12.Controls.Add(this.panelComfirm, 0, 0);
            this.tableLayoutPanel12.Controls.Add(this.button_Alarm_Buzz_Off, 0, 1);
            this.tableLayoutPanel12.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel12.Location = new System.Drawing.Point(4, 24);
            this.tableLayoutPanel12.Name = "tableLayoutPanel12";
            this.tableLayoutPanel12.RowCount = 2;
            this.tableLayoutPanel12.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.tableLayoutPanel12.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel12.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel12.Size = new System.Drawing.Size(278, 184);
            this.tableLayoutPanel12.TabIndex = 0;
            // 
            // tableLayoutPanel13
            // 
            this.tableLayoutPanel13.ColumnCount = 1;
            this.tableLayoutPanel13.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel13.Controls.Add(this.radioButtonLastCell, 0, 1);
            this.tableLayoutPanel13.Controls.Add(this.radioButtonUserSelectedCell, 0, 0);
            this.tableLayoutPanel13.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel13.Location = new System.Drawing.Point(4, 24);
            this.tableLayoutPanel13.Name = "tableLayoutPanel13";
            this.tableLayoutPanel13.RowCount = 2;
            this.tableLayoutPanel13.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel13.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel13.Size = new System.Drawing.Size(278, 59);
            this.tableLayoutPanel13.TabIndex = 2;
            // 
            // Form_Alarm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(869, 501);
            this.Controls.Add(this.groupBoxSelectedAlarmDetails);
            this.Font = new System.Drawing.Font("Tahoma", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form_Alarm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Alarm Dialog";
            this.Activated += new System.EventHandler(this.FormNew_Alarm_Activated);
            this.Load += new System.EventHandler(this.FormNew_Alarm_Load);
            this.groupBoxSelectedAlarmDetails.ResumeLayout(false);
            this.groupBoxCellFocusOption.ResumeLayout(false);
            this.groupBoxRecovery.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.baseDataGridViewAlarm)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel5.ResumeLayout(false);
            this.tableLayoutPanel5.PerformLayout();
            this.tableLayoutPanel6.ResumeLayout(false);
            this.tableLayoutPanel6.PerformLayout();
            this.tableLayoutPanel7.ResumeLayout(false);
            this.tableLayoutPanel8.ResumeLayout(false);
            this.tableLayoutPanel8.PerformLayout();
            this.tableLayoutPanel10.ResumeLayout(false);
            this.tableLayoutPanel10.PerformLayout();
            this.tableLayoutPanel11.ResumeLayout(false);
            this.tableLayoutPanel11.PerformLayout();
            this.tableLayoutPanel9.ResumeLayout(false);
            this.tableLayoutPanel12.ResumeLayout(false);
            this.tableLayoutPanel13.ResumeLayout(false);
            this.tableLayoutPanel13.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private GroupBox groupBoxSelectedAlarmDetails;
        private TextBox baseTextBoxAlarmTitle;
        private TextBox baseTextBoxCode;
        private TextBox baseTextBoxGrade;
        private TextBox baseTextBoxSource;
        private TextBox baseTextBoxCause;
        private Label baseLabelCode;
        private GroupBox groupBoxCellFocusOption;
        private RadioButton radioButtonLastCell;
        private RadioButton radioButtonUserSelectedCell;
        private GroupBox groupBoxRecovery;
        private Panel panelComfirm;
        private Label baseLabelCause;
        private Label baseLabelSource;
        private Label baseLabelGrade;
        private Label baseLabelAlarmTitle;
        private DataGridView baseDataGridViewAlarm;
        private Button button_Alarm_Buzz_Off;
        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel3;
        private TableLayoutPanel tableLayoutPanel2;
        private TableLayoutPanel tableLayoutPanel4;
        private TableLayoutPanel tableLayoutPanel8;
        private TableLayoutPanel tableLayoutPanel7;
        private TableLayoutPanel tableLayoutPanel10;
        private TableLayoutPanel tableLayoutPanel11;
        private TableLayoutPanel tableLayoutPanel6;
        private TableLayoutPanel tableLayoutPanel5;
        private TableLayoutPanel tableLayoutPanel9;
        private TableLayoutPanel tableLayoutPanel12;
        private TableLayoutPanel tableLayoutPanel13;
    }
}
