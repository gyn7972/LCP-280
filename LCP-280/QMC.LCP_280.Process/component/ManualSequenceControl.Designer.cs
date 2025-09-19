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
            this._panelButtons = new System.Windows.Forms.Panel();
            this._btnRun = new System.Windows.Forms.Button();
            this._btnPlay = new System.Windows.Forms.Button();
            this._btnStop = new System.Windows.Forms.Button();
            this._btnNext = new System.Windows.Forms.Button();
            this._lstSteps = new System.Windows.Forms.ListBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this._panelButtons.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // _panelButtons
            // 
            this._panelButtons.Controls.Add(this._btnRun);
            this._panelButtons.Controls.Add(this._btnPlay);
            this._panelButtons.Controls.Add(this._btnStop);
            this._panelButtons.Controls.Add(this._btnNext);
            this._panelButtons.Dock = System.Windows.Forms.DockStyle.Top;
            this._panelButtons.Location = new System.Drawing.Point(0, 0);
            this._panelButtons.Margin = new System.Windows.Forms.Padding(2);
            this._panelButtons.Name = "_panelButtons";
            this._panelButtons.Size = new System.Drawing.Size(380, 40);
            this._panelButtons.TabIndex = 0;
            // 
            // _btnRun
            // 
            this._btnRun.Location = new System.Drawing.Point(7, 5);
            this._btnRun.Margin = new System.Windows.Forms.Padding(2);
            this._btnRun.Name = "_btnRun";
            this._btnRun.Size = new System.Drawing.Size(70, 30);
            this._btnRun.TabIndex = 0;
            this._btnRun.Text = "Run";
            this._btnRun.UseVisualStyleBackColor = true;
            this._btnRun.Click += new System.EventHandler(this._btnRun_Click);
            // 
            // _btnPlay
            // 
            this._btnPlay.Location = new System.Drawing.Point(282, 5);
            this._btnPlay.Margin = new System.Windows.Forms.Padding(2);
            this._btnPlay.Name = "_btnPlay";
            this._btnPlay.Size = new System.Drawing.Size(90, 30);
            this._btnPlay.TabIndex = 3;
            this._btnPlay.Text = "Play";
            this._btnPlay.UseVisualStyleBackColor = true;
            this._btnPlay.Click += new System.EventHandler(this._btnPlay_Click);
            // 
            // _btnStop
            // 
            this._btnStop.Location = new System.Drawing.Point(197, 5);
            this._btnStop.Margin = new System.Windows.Forms.Padding(2);
            this._btnStop.Name = "_btnStop";
            this._btnStop.Size = new System.Drawing.Size(70, 30);
            this._btnStop.TabIndex = 2;
            this._btnStop.Text = "Stop";
            this._btnStop.UseVisualStyleBackColor = true;
            // 
            // _btnNext
            // 
            this._btnNext.Location = new System.Drawing.Point(92, 5);
            this._btnNext.Margin = new System.Windows.Forms.Padding(2);
            this._btnNext.Name = "_btnNext";
            this._btnNext.Size = new System.Drawing.Size(90, 30);
            this._btnNext.TabIndex = 1;
            this._btnNext.Text = "Next ▶";
            this._btnNext.UseVisualStyleBackColor = true;
            this._btnNext.Click += new System.EventHandler(this._btnNext_Click);
            // 
            // _lstSteps
            // 
            this._lstSteps.Dock = System.Windows.Forms.DockStyle.Fill;
            this._lstSteps.FormattingEnabled = true;
            this._lstSteps.ItemHeight = 15;
            this._lstSteps.Location = new System.Drawing.Point(0, 0);
            this._lstSteps.Margin = new System.Windows.Forms.Padding(2);
            this._lstSteps.Name = "_lstSteps";
            this._lstSteps.Size = new System.Drawing.Size(380, 270);
            this._lstSteps.TabIndex = 5;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this._lstSteps);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 40);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(380, 270);
            this.panel1.TabIndex = 6;
            // 
            // ManualSequenceControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this._panelButtons);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "ManualSequenceControl";
            this.Size = new System.Drawing.Size(380, 310);
            this._panelButtons.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel _panelButtons;
        private System.Windows.Forms.Button _btnNext;
        private System.Windows.Forms.Button _btnStop;
        private System.Windows.Forms.Button _btnPlay;
        private System.Windows.Forms.ListBox _lstSteps;
        private System.Windows.Forms.Button _btnRun;
        private System.Windows.Forms.Panel panel1;
    }
}
