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
            this._lstSteps.Dock = System.Windows.Forms.DockStyle.Fill;
            this._lstSteps.FormattingEnabled = true;
            this._lstSteps.ItemHeight = 12;
            this._lstSteps.Location = new System.Drawing.Point(2, 52);
            this._lstSteps.Margin = new System.Windows.Forms.Padding(2);
            this._lstSteps.Name = "_lstSteps";
            this._lstSteps.Size = new System.Drawing.Size(296, 146);
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
            this.tableLayoutPanel1.Size = new System.Drawing.Size(300, 200);
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
            this.btnPlay.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnPlay.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnPlay.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnPlay.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnPlay.CustomForeColor = System.Drawing.Color.Black;
            this.btnPlay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnPlay.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnPlay.ForeColor = System.Drawing.Color.Black;
            this.btnPlay.ImageSize = new System.Drawing.Size(45, 45);
            this.btnPlay.Location = new System.Drawing.Point(221, 3);
            this.btnPlay.Name = "btnPlay";
            this.btnPlay.Size = new System.Drawing.Size(71, 40);
            this.btnPlay.TabIndex = 21;
            this.btnPlay.TabStop = false;
            this.btnPlay.Text = "Play";
            this.btnPlay.UseVisualStyleBackColor = false;
            this.btnPlay.Click += new System.EventHandler(this._btnPlay_Click);
            // 
            // btnStop
            // 
            this.btnStop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnStop.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnStop.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnStop.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnStop.CustomForeColor = System.Drawing.Color.Black;
            this.btnStop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnStop.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnStop.ForeColor = System.Drawing.Color.Black;
            this.btnStop.ImageSize = new System.Drawing.Size(45, 45);
            this.btnStop.Location = new System.Drawing.Point(148, 3);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(71, 40);
            this.btnStop.TabIndex = 20;
            this.btnStop.TabStop = false;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = false;
            this.btnStop.Click += new System.EventHandler(this._btnStop_Click);
            // 
            // _btnNext
            // 
            this._btnNext.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this._btnNext.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this._btnNext.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this._btnNext.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this._btnNext.CustomForeColor = System.Drawing.Color.Black;
            this._btnNext.Dock = System.Windows.Forms.DockStyle.Fill;
            this._btnNext.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this._btnNext.ForeColor = System.Drawing.Color.Black;
            this._btnNext.ImageSize = new System.Drawing.Size(45, 45);
            this._btnNext.Location = new System.Drawing.Point(75, 3);
            this._btnNext.Name = "_btnNext";
            this._btnNext.Size = new System.Drawing.Size(71, 40);
            this._btnNext.TabIndex = 19;
            this._btnNext.TabStop = false;
            this._btnNext.Text = "Next ▶";
            this._btnNext.UseVisualStyleBackColor = false;
            this._btnNext.Click += new System.EventHandler(this._btnNext_Click);
            // 
            // btnRun
            // 
            this.btnRun.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnRun.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnRun.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnRun.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnRun.CustomForeColor = System.Drawing.Color.Black;
            this.btnRun.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnRun.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnRun.ForeColor = System.Drawing.Color.Black;
            this.btnRun.ImageSize = new System.Drawing.Size(45, 45);
            this.btnRun.Location = new System.Drawing.Point(2, 3);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(71, 40);
            this.btnRun.TabIndex = 18;
            this.btnRun.TabStop = false;
            this.btnRun.Text = "Run";
            this.btnRun.UseVisualStyleBackColor = false;
            this.btnRun.Click += new System.EventHandler(this._btnRun_Click);
            // 
            // ManualSequenceControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "ManualSequenceControl";
            this.Size = new System.Drawing.Size(300, 200);
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
