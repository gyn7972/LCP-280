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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            this.groupBoxSelectedAlarmDetails = new System.Windows.Forms.GroupBox();
            this.baseTextBoxAlarmTitle = new System.Windows.Forms.TextBox();
            this.baseTextBoxCode = new System.Windows.Forms.TextBox();
            this.baseTextBoxGrade = new System.Windows.Forms.TextBox();
            this.baseTextBoxSource = new System.Windows.Forms.TextBox();
            this.baseTextBoxCause = new System.Windows.Forms.TextBox();
            this.baseLabelCode = new System.Windows.Forms.Label();
            this.groupBoxCellFocusOption = new System.Windows.Forms.GroupBox();
            this.radioButtonLastCell = new System.Windows.Forms.RadioButton();
            this.radioButtonUserSelectedCell = new System.Windows.Forms.RadioButton();
            this.groupBoxRecovery = new System.Windows.Forms.GroupBox();
            this.button_Alarm_Buzz_Off = new System.Windows.Forms.Button();
            this.panelComfirm = new System.Windows.Forms.Panel();
            this.baseLabelCause = new System.Windows.Forms.Label();
            this.baseLabelSource = new System.Windows.Forms.Label();
            this.baseLabelGrade = new System.Windows.Forms.Label();
            this.baseLabelAlarmTitle = new System.Windows.Forms.Label();
            this.baseDataGridViewAlarm = new System.Windows.Forms.DataGridView();
            this.groupBoxSelectedAlarmDetails.SuspendLayout();
            this.groupBoxCellFocusOption.SuspendLayout();
            this.groupBoxRecovery.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.baseDataGridViewAlarm)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBoxSelectedAlarmDetails
            // 
            this.groupBoxSelectedAlarmDetails.Controls.Add(this.baseTextBoxAlarmTitle);
            this.groupBoxSelectedAlarmDetails.Controls.Add(this.baseLabelGrade);
            this.groupBoxSelectedAlarmDetails.Controls.Add(this.baseLabelAlarmTitle);
            this.groupBoxSelectedAlarmDetails.Controls.Add(this.baseTextBoxSource);
            this.groupBoxSelectedAlarmDetails.Controls.Add(this.baseTextBoxCode);
            this.groupBoxSelectedAlarmDetails.Controls.Add(this.baseLabelCode);
            this.groupBoxSelectedAlarmDetails.Controls.Add(this.baseTextBoxCause);
            this.groupBoxSelectedAlarmDetails.Controls.Add(this.baseLabelCause);
            this.groupBoxSelectedAlarmDetails.Controls.Add(this.baseTextBoxGrade);
            this.groupBoxSelectedAlarmDetails.Controls.Add(this.groupBoxCellFocusOption);
            this.groupBoxSelectedAlarmDetails.Controls.Add(this.groupBoxRecovery);
            this.groupBoxSelectedAlarmDetails.Controls.Add(this.baseLabelSource);
            this.groupBoxSelectedAlarmDetails.Font = new System.Drawing.Font("Tahoma", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBoxSelectedAlarmDetails.ForeColor = System.Drawing.Color.Black;
            this.groupBoxSelectedAlarmDetails.Location = new System.Drawing.Point(6, 5);
            this.groupBoxSelectedAlarmDetails.Margin = new System.Windows.Forms.Padding(4);
            this.groupBoxSelectedAlarmDetails.Name = "groupBoxSelectedAlarmDetails";
            this.groupBoxSelectedAlarmDetails.Padding = new System.Windows.Forms.Padding(4);
            this.groupBoxSelectedAlarmDetails.Size = new System.Drawing.Size(855, 358);
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
            this.baseTextBoxAlarmTitle.Location = new System.Drawing.Point(65, 33);
            this.baseTextBoxAlarmTitle.Margin = new System.Windows.Forms.Padding(4);
            this.baseTextBoxAlarmTitle.Multiline = true;
            this.baseTextBoxAlarmTitle.Name = "baseTextBoxAlarmTitle";
            this.baseTextBoxAlarmTitle.Size = new System.Drawing.Size(319, 39);
            this.baseTextBoxAlarmTitle.TabIndex = 19;
            // 
            // baseTextBoxCode
            // 
            this.baseTextBoxCode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.baseTextBoxCode.BackColor = System.Drawing.Color.White;
            this.baseTextBoxCode.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.baseTextBoxCode.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.baseTextBoxCode.ForeColor = System.Drawing.Color.Black;
            this.baseTextBoxCode.Location = new System.Drawing.Point(463, 79);
            this.baseTextBoxCode.Margin = new System.Windows.Forms.Padding(4);
            this.baseTextBoxCode.Multiline = true;
            this.baseTextBoxCode.Name = "baseTextBoxCode";
            this.baseTextBoxCode.Size = new System.Drawing.Size(115, 40);
            this.baseTextBoxCode.TabIndex = 19;
            // 
            // baseTextBoxGrade
            // 
            this.baseTextBoxGrade.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.baseTextBoxGrade.BackColor = System.Drawing.Color.White;
            this.baseTextBoxGrade.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.baseTextBoxGrade.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.baseTextBoxGrade.ForeColor = System.Drawing.Color.Black;
            this.baseTextBoxGrade.Location = new System.Drawing.Point(65, 79);
            this.baseTextBoxGrade.Margin = new System.Windows.Forms.Padding(4);
            this.baseTextBoxGrade.Multiline = true;
            this.baseTextBoxGrade.Name = "baseTextBoxGrade";
            this.baseTextBoxGrade.Size = new System.Drawing.Size(319, 40);
            this.baseTextBoxGrade.TabIndex = 18;
            // 
            // baseTextBoxSource
            // 
            this.baseTextBoxSource.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.baseTextBoxSource.BackColor = System.Drawing.Color.White;
            this.baseTextBoxSource.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.baseTextBoxSource.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.baseTextBoxSource.ForeColor = System.Drawing.Color.Black;
            this.baseTextBoxSource.Location = new System.Drawing.Point(65, 127);
            this.baseTextBoxSource.Margin = new System.Windows.Forms.Padding(4);
            this.baseTextBoxSource.Multiline = true;
            this.baseTextBoxSource.Name = "baseTextBoxSource";
            this.baseTextBoxSource.Size = new System.Drawing.Size(513, 68);
            this.baseTextBoxSource.TabIndex = 17;
            // 
            // baseTextBoxCause
            // 
            this.baseTextBoxCause.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.baseTextBoxCause.BackColor = System.Drawing.Color.White;
            this.baseTextBoxCause.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.baseTextBoxCause.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.baseTextBoxCause.ForeColor = System.Drawing.Color.Black;
            this.baseTextBoxCause.Location = new System.Drawing.Point(65, 203);
            this.baseTextBoxCause.Margin = new System.Windows.Forms.Padding(4);
            this.baseTextBoxCause.Multiline = true;
            this.baseTextBoxCause.Name = "baseTextBoxCause";
            this.baseTextBoxCause.Size = new System.Drawing.Size(513, 139);
            this.baseTextBoxCause.TabIndex = 16;
            // 
            // baseLabelCode
            // 
            this.baseLabelCode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.baseLabelCode.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.baseLabelCode.ForeColor = System.Drawing.Color.Black;
            this.baseLabelCode.Location = new System.Drawing.Point(396, 85);
            this.baseLabelCode.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.baseLabelCode.Name = "baseLabelCode";
            this.baseLabelCode.Size = new System.Drawing.Size(66, 29);
            this.baseLabelCode.TabIndex = 14;
            this.baseLabelCode.Text = "Code";
            // 
            // groupBoxCellFocusOption
            // 
            this.groupBoxCellFocusOption.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxCellFocusOption.Controls.Add(this.radioButtonLastCell);
            this.groupBoxCellFocusOption.Controls.Add(this.radioButtonUserSelectedCell);
            this.groupBoxCellFocusOption.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBoxCellFocusOption.ForeColor = System.Drawing.Color.Black;
            this.groupBoxCellFocusOption.Location = new System.Drawing.Point(586, 242);
            this.groupBoxCellFocusOption.Margin = new System.Windows.Forms.Padding(4);
            this.groupBoxCellFocusOption.Name = "groupBoxCellFocusOption";
            this.groupBoxCellFocusOption.Padding = new System.Windows.Forms.Padding(4);
            this.groupBoxCellFocusOption.Size = new System.Drawing.Size(258, 88);
            this.groupBoxCellFocusOption.TabIndex = 3;
            this.groupBoxCellFocusOption.TabStop = false;
            this.groupBoxCellFocusOption.Text = " Cell Focus Option ";
            // 
            // radioButtonLastCell
            // 
            this.radioButtonLastCell.AutoSize = true;
            this.radioButtonLastCell.Location = new System.Drawing.Point(8, 56);
            this.radioButtonLastCell.Margin = new System.Windows.Forms.Padding(4);
            this.radioButtonLastCell.Name = "radioButtonLastCell";
            this.radioButtonLastCell.Size = new System.Drawing.Size(86, 23);
            this.radioButtonLastCell.TabIndex = 1;
            this.radioButtonLastCell.TabStop = true;
            this.radioButtonLastCell.Text = "Last Cell";
            this.radioButtonLastCell.UseVisualStyleBackColor = true;
            // 
            // radioButtonUserSelectedCell
            // 
            this.radioButtonUserSelectedCell.AutoSize = true;
            this.radioButtonUserSelectedCell.Location = new System.Drawing.Point(8, 25);
            this.radioButtonUserSelectedCell.Margin = new System.Windows.Forms.Padding(4);
            this.radioButtonUserSelectedCell.Name = "radioButtonUserSelectedCell";
            this.radioButtonUserSelectedCell.Size = new System.Drawing.Size(153, 23);
            this.radioButtonUserSelectedCell.TabIndex = 0;
            this.radioButtonUserSelectedCell.TabStop = true;
            this.radioButtonUserSelectedCell.Text = "User Selected Cell";
            this.radioButtonUserSelectedCell.UseVisualStyleBackColor = true;
            // 
            // groupBoxRecovery
            // 
            this.groupBoxRecovery.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxRecovery.Controls.Add(this.button_Alarm_Buzz_Off);
            this.groupBoxRecovery.Controls.Add(this.panelComfirm);
            this.groupBoxRecovery.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBoxRecovery.ForeColor = System.Drawing.Color.Black;
            this.groupBoxRecovery.Location = new System.Drawing.Point(586, 33);
            this.groupBoxRecovery.Margin = new System.Windows.Forms.Padding(4);
            this.groupBoxRecovery.Name = "groupBoxRecovery";
            this.groupBoxRecovery.Padding = new System.Windows.Forms.Padding(4);
            this.groupBoxRecovery.Size = new System.Drawing.Size(258, 201);
            this.groupBoxRecovery.TabIndex = 2;
            this.groupBoxRecovery.TabStop = false;
            this.groupBoxRecovery.Text = " Recovery ";
            // 
            // button_Alarm_Buzz_Off
            // 
            this.button_Alarm_Buzz_Off.Font = new System.Drawing.Font("Tahoma", 15F, System.Drawing.FontStyle.Bold);
            this.button_Alarm_Buzz_Off.Location = new System.Drawing.Point(7, 138);
            this.button_Alarm_Buzz_Off.Name = "button_Alarm_Buzz_Off";
            this.button_Alarm_Buzz_Off.Size = new System.Drawing.Size(241, 55);
            this.button_Alarm_Buzz_Off.TabIndex = 1;
            this.button_Alarm_Buzz_Off.Text = "부저 정지";
            this.button_Alarm_Buzz_Off.UseVisualStyleBackColor = true;
            this.button_Alarm_Buzz_Off.Click += new System.EventHandler(this.button_Alarm_Buzz_Off_Click);
            // 
            // panelComfirm
            // 
            this.panelComfirm.Location = new System.Drawing.Point(8, 28);
            this.panelComfirm.Margin = new System.Windows.Forms.Padding(4);
            this.panelComfirm.Name = "panelComfirm";
            this.panelComfirm.Size = new System.Drawing.Size(235, 80);
            this.panelComfirm.TabIndex = 0;
            // 
            // baseLabelCause
            // 
            this.baseLabelCause.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.baseLabelCause.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.baseLabelCause.ForeColor = System.Drawing.Color.Black;
            this.baseLabelCause.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.baseLabelCause.Location = new System.Drawing.Point(8, 206);
            this.baseLabelCause.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.baseLabelCause.Name = "baseLabelCause";
            this.baseLabelCause.Size = new System.Drawing.Size(57, 29);
            this.baseLabelCause.TabIndex = 7;
            this.baseLabelCause.Text = "Cause";
            this.baseLabelCause.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // baseLabelSource
            // 
            this.baseLabelSource.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.baseLabelSource.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.baseLabelSource.ForeColor = System.Drawing.Color.Black;
            this.baseLabelSource.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.baseLabelSource.Location = new System.Drawing.Point(8, 130);
            this.baseLabelSource.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.baseLabelSource.Name = "baseLabelSource";
            this.baseLabelSource.Size = new System.Drawing.Size(57, 29);
            this.baseLabelSource.TabIndex = 6;
            this.baseLabelSource.Text = "Source";
            this.baseLabelSource.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // baseLabelGrade
            // 
            this.baseLabelGrade.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.baseLabelGrade.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.baseLabelGrade.ForeColor = System.Drawing.Color.Black;
            this.baseLabelGrade.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.baseLabelGrade.Location = new System.Drawing.Point(8, 86);
            this.baseLabelGrade.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.baseLabelGrade.Name = "baseLabelGrade";
            this.baseLabelGrade.Size = new System.Drawing.Size(57, 29);
            this.baseLabelGrade.TabIndex = 5;
            this.baseLabelGrade.Text = "Grade";
            this.baseLabelGrade.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // baseLabelAlarmTitle
            // 
            this.baseLabelAlarmTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.baseLabelAlarmTitle.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.baseLabelAlarmTitle.ForeColor = System.Drawing.Color.Black;
            this.baseLabelAlarmTitle.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.baseLabelAlarmTitle.Location = new System.Drawing.Point(8, 35);
            this.baseLabelAlarmTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.baseLabelAlarmTitle.Name = "baseLabelAlarmTitle";
            this.baseLabelAlarmTitle.Size = new System.Drawing.Size(57, 29);
            this.baseLabelAlarmTitle.TabIndex = 4;
            this.baseLabelAlarmTitle.Text = "Title";
            this.baseLabelAlarmTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // baseDataGridViewAlarm
            // 
            this.baseDataGridViewAlarm.AllowUserToAddRows = false;
            this.baseDataGridViewAlarm.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.baseDataGridViewAlarm.BackgroundColor = System.Drawing.Color.White;
            this.baseDataGridViewAlarm.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(78)))), ((int)(((byte)(78)))), ((int)(((byte)(78)))));
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Tahoma", 12F);
            dataGridViewCellStyle4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(78)))), ((int)(((byte)(78)))), ((int)(((byte)(78)))));
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.baseDataGridViewAlarm.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.baseDataGridViewAlarm.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Tahoma", 12F);
            dataGridViewCellStyle5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(3)))), ((int)(((byte)(3)))), ((int)(((byte)(3)))));
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.baseDataGridViewAlarm.DefaultCellStyle = dataGridViewCellStyle5;
            this.baseDataGridViewAlarm.Font = new System.Drawing.Font("Tahoma", 12F);
            this.baseDataGridViewAlarm.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(3)))), ((int)(((byte)(3)))), ((int)(((byte)(3)))));
            this.baseDataGridViewAlarm.Location = new System.Drawing.Point(6, 371);
            this.baseDataGridViewAlarm.Margin = new System.Windows.Forms.Padding(4);
            this.baseDataGridViewAlarm.Name = "baseDataGridViewAlarm";
            this.baseDataGridViewAlarm.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(78)))), ((int)(((byte)(78)))), ((int)(((byte)(78)))));
            dataGridViewCellStyle6.Font = new System.Drawing.Font("Tahoma", 12F);
            dataGridViewCellStyle6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(78)))), ((int)(((byte)(78)))), ((int)(((byte)(78)))));
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.baseDataGridViewAlarm.RowHeadersDefaultCellStyle = dataGridViewCellStyle6;
            this.baseDataGridViewAlarm.RowHeadersVisible = false;
            this.baseDataGridViewAlarm.RowHeadersWidth = 62;
            this.baseDataGridViewAlarm.RowTemplate.Height = 23;
            this.baseDataGridViewAlarm.Size = new System.Drawing.Size(855, 124);
            this.baseDataGridViewAlarm.TabIndex = 6;
            this.baseDataGridViewAlarm.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.baseDataGridViewAlarm_CellClick);
            this.baseDataGridViewAlarm.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.baseDataGridViewAlarm_CellContentClick);
            this.baseDataGridViewAlarm.RowsAdded += new System.Windows.Forms.DataGridViewRowsAddedEventHandler(this.baseDataGridViewAlarm_RowsAdded);
            this.baseDataGridViewAlarm.RowsRemoved += new System.Windows.Forms.DataGridViewRowsRemovedEventHandler(this.baseDataGridViewAlarm_RowsRemoved);
            // 
            // Form_Alarm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(869, 501);
            this.Controls.Add(this.baseDataGridViewAlarm);
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
            this.groupBoxSelectedAlarmDetails.PerformLayout();
            this.groupBoxCellFocusOption.ResumeLayout(false);
            this.groupBoxCellFocusOption.PerformLayout();
            this.groupBoxRecovery.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.baseDataGridViewAlarm)).EndInit();
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
    }
}
