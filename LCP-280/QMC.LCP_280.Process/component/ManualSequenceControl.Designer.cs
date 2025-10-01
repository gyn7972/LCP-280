//using System.Windows.Forms;
//using System.Drawing;
//using System.ComponentModel;

namespace QMC.LCP_280.Process.Component
{

    partial class ManualSequenceControl
    {
        /// <summary> 
        /// 필수 디자이너 변수
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 사용 중인 리소스 정리
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 컴포넌트 디자이너 생성 코드

        private void InitializeComponent()
        {
            this._lstSteps = new System.Windows.Forms.ListBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.btnPlay = new QMC.Common.IndividualMenuButton();
            this.btnStop = new QMC.Common.IndividualMenuButton();
            this._btnNext = new QMC.Common.IndividualMenuButton();
            this.btnRun = new QMC.Common.IndividualMenuButton();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // _lstSteps
            // 
            this._lstSteps.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._lstSteps.FormattingEnabled = true;
            this._lstSteps.ItemHeight = 12;
            this._lstSteps.Location = new System.Drawing.Point(2, 52);
            this._lstSteps.Margin = new System.Windows.Forms.Padding(2);
            this._lstSteps.Name = "_lstSteps";
            this._lstSteps.Size = new System.Drawing.Size(296, 244);
            this._lstSteps.TabIndex = 5;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this._lstSteps, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(300, 300);
            this.tableLayoutPanel1.TabIndex = 7;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 4;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel2.Controls.Add(this.btnPlay, 3, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnStop, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this._btnNext, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnRun, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(294, 44);
            this.tableLayoutPanel2.TabIndex = 8;
            // 
            // btnPlay
            // 
            this.button1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.button1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.button1.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.button1.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.button1.CustomForeColor = System.Drawing.Color.Black;
            this.button1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.button1.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.button1.ForeColor = System.Drawing.Color.Black;
            this.button1.ImageSize = new System.Drawing.Size(45, 45);
            this.button1.Location = new System.Drawing.Point(221, 3);
            this.button1.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(71, 48);
            this.button1.TabIndex = 21;
            this.button1.TabStop = false;
            this.button1.Text = "Play";
            this.button1.UseVisualStyleBackColor = false;
            // 
            // btnStop
            // 
            this.button3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.button3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.button3.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.button3.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.button3.CustomForeColor = System.Drawing.Color.Black;
            this.button3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.button3.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.button3.ForeColor = System.Drawing.Color.Black;
            this.button3.ImageSize = new System.Drawing.Size(45, 45);
            this.button3.Location = new System.Drawing.Point(148, 3);
            this.button3.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(69, 48);
            this.button3.TabIndex = 20;
            this.button3.TabStop = false;
            this.button3.Text = "Stop";
            this.button3.UseVisualStyleBackColor = false;
            // 
            // _btnNext
            // 
            this.button4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.button4.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.button4.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.button4.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.button4.CustomForeColor = System.Drawing.Color.Black;
            this.button4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.button4.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.button4.ForeColor = System.Drawing.Color.Black;
            this.button4.ImageSize = new System.Drawing.Size(45, 45);
            this.button4.Location = new System.Drawing.Point(75, 3);
            this.button4.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(69, 48);
            this.button4.TabIndex = 19;
            this.button4.TabStop = false;
            this.button4.Text = "Next ▶";
            this.button4.UseVisualStyleBackColor = false;
            // 
            // btnRun
            // 
            this.button2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.button2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.button2.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.button2.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.button2.CustomForeColor = System.Drawing.Color.Black;
            this.button2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.button2.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.button2.ForeColor = System.Drawing.Color.Black;
            this.button2.ImageSize = new System.Drawing.Size(45, 45);
            this.button2.Location = new System.Drawing.Point(2, 3);
            this.button2.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(69, 48);
            this.button2.TabIndex = 18;
            this.button2.TabStop = false;
            this.button2.Text = "Run";
            this.button2.UseVisualStyleBackColor = false;
            // 
            // ManualSequenceControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "ManualSequenceControl";
            this.Size = new System.Drawing.Size(300, 300);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ListBox _lstSteps;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private Common.IndividualMenuButton btnRun;
        private Common.IndividualMenuButton btnPlay;
        private Common.IndividualMenuButton btnStop;
        private Common.IndividualMenuButton _btnNext;
    }
}
