namespace QMC.LCP_280.Process.Unit.FormMain
{
    partial class SequenceAutoControl
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
            if (disposing)
            {
                // 이벤트 해제
                foreach (var button in _buttonCommands.Keys)
                {
                    button.Click -= OnAutoButtonClick;
                }

                if (components != null)
                {
                    components.Dispose();
                }
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
            this.btn_Auto_Reset = new QMC.Common.IndividualMenuButton();
            this.btn_Auto_CycleStop = new QMC.Common.IndividualMenuButton();
            this.btn_Auto_Stop = new QMC.Common.IndividualMenuButton();
            this.btn_Auto_Start = new QMC.Common.IndividualMenuButton();
            this.btn_Auto_Ready = new QMC.Common.IndividualMenuButton();
            this.tableLayoutPanel8.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel8
            // 
            this.tableLayoutPanel8.ColumnCount = 1;
            this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel8.Controls.Add(this.btn_Auto_Ready, 0, 0);
            this.tableLayoutPanel8.Controls.Add(this.btn_Auto_Start, 0, 1);
            this.tableLayoutPanel8.Controls.Add(this.btn_Auto_Stop, 0, 2);
            this.tableLayoutPanel8.Controls.Add(this.btn_Auto_CycleStop, 0, 3);
            this.tableLayoutPanel8.Controls.Add(this.btn_Auto_Reset, 0, 4);
            this.tableLayoutPanel8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel8.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel8.Name = "tableLayoutPanel8";
            this.tableLayoutPanel8.RowCount = 5;
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel8.Size = new System.Drawing.Size(100, 322);
            this.tableLayoutPanel8.TabIndex = 2;
            // 
            // btn_Auto_Reset
            // 
            this.btn_Auto_Reset.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Auto_Reset.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Auto_Reset.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Auto_Reset.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Auto_Reset.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Auto_Reset.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_Auto_Reset.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Auto_Reset.ForeColor = System.Drawing.Color.Black;
            this.btn_Auto_Reset.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Auto_Reset.Location = new System.Drawing.Point(3, 259);
            this.btn_Auto_Reset.Name = "btn_Auto_Reset";
            this.btn_Auto_Reset.Size = new System.Drawing.Size(94, 60);
            this.btn_Auto_Reset.TabIndex = 16;
            this.btn_Auto_Reset.TabStop = false;
            this.btn_Auto_Reset.Text = "Reset";
            this.btn_Auto_Reset.UseVisualStyleBackColor = false;
            // 
            // btn_Auto_CycleStop
            // 
            this.btn_Auto_CycleStop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Auto_CycleStop.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Auto_CycleStop.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Auto_CycleStop.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Auto_CycleStop.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Auto_CycleStop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_Auto_CycleStop.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Auto_CycleStop.ForeColor = System.Drawing.Color.Black;
            this.btn_Auto_CycleStop.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Auto_CycleStop.Location = new System.Drawing.Point(3, 195);
            this.btn_Auto_CycleStop.Name = "btn_Auto_CycleStop";
            this.btn_Auto_CycleStop.Size = new System.Drawing.Size(94, 58);
            this.btn_Auto_CycleStop.TabIndex = 15;
            this.btn_Auto_CycleStop.TabStop = false;
            this.btn_Auto_CycleStop.Text = "Cycle Stop";
            this.btn_Auto_CycleStop.UseVisualStyleBackColor = false;
            // 
            // btn_Auto_Stop
            // 
            this.btn_Auto_Stop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Auto_Stop.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Auto_Stop.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Auto_Stop.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Auto_Stop.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Auto_Stop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_Auto_Stop.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Auto_Stop.ForeColor = System.Drawing.Color.Black;
            this.btn_Auto_Stop.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Auto_Stop.Location = new System.Drawing.Point(3, 131);
            this.btn_Auto_Stop.Name = "btn_Auto_Stop";
            this.btn_Auto_Stop.Size = new System.Drawing.Size(94, 58);
            this.btn_Auto_Stop.TabIndex = 14;
            this.btn_Auto_Stop.TabStop = false;
            this.btn_Auto_Stop.Text = "Stop";
            this.btn_Auto_Stop.UseVisualStyleBackColor = false;
            // 
            // btn_Auto_Start
            // 
            this.btn_Auto_Start.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Auto_Start.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Auto_Start.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Auto_Start.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Auto_Start.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Auto_Start.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_Auto_Start.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Auto_Start.ForeColor = System.Drawing.Color.Black;
            this.btn_Auto_Start.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Auto_Start.Location = new System.Drawing.Point(3, 67);
            this.btn_Auto_Start.Name = "btn_Auto_Start";
            this.btn_Auto_Start.Size = new System.Drawing.Size(94, 58);
            this.btn_Auto_Start.TabIndex = 13;
            this.btn_Auto_Start.TabStop = false;
            this.btn_Auto_Start.Text = "Start";
            this.btn_Auto_Start.UseVisualStyleBackColor = false;
            // 
            // btn_Auto_Ready
            // 
            this.btn_Auto_Ready.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Auto_Ready.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Auto_Ready.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Auto_Ready.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Auto_Ready.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Auto_Ready.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_Auto_Ready.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Auto_Ready.ForeColor = System.Drawing.Color.Black;
            this.btn_Auto_Ready.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Auto_Ready.Location = new System.Drawing.Point(3, 3);
            this.btn_Auto_Ready.Name = "btn_Auto_Ready";
            this.btn_Auto_Ready.Size = new System.Drawing.Size(94, 58);
            this.btn_Auto_Ready.TabIndex = 11;
            this.btn_Auto_Ready.TabStop = false;
            this.btn_Auto_Ready.Text = "Ready";
            this.btn_Auto_Ready.UseVisualStyleBackColor = false;
            // 
            // SequenceAutoControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel8);
            this.Name = "SequenceAutoControl";
            this.Size = new System.Drawing.Size(100, 322);
            this.tableLayoutPanel8.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel8;
        private Common.IndividualMenuButton btn_Auto_Ready;
        private Common.IndividualMenuButton btn_Auto_Start;
        private Common.IndividualMenuButton btn_Auto_Stop;
        private Common.IndividualMenuButton btn_Auto_CycleStop;
        private Common.IndividualMenuButton btn_Auto_Reset;
    }
}
