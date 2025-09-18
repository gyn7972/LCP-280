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
            this._btnBack = new System.Windows.Forms.Button();
            this._btnRecover = new System.Windows.Forms.Button();
            this._btnStop = new System.Windows.Forms.Button();
            this._btnManual = new System.Windows.Forms.Button();
            this._cboSequence = new System.Windows.Forms.ComboBox();
            this._lstSteps = new System.Windows.Forms.ListBox();
            this._panelButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // _panelButtons
            // 
            this._panelButtons.Controls.Add(this._btnBack);
            this._panelButtons.Controls.Add(this._btnRecover);
            this._panelButtons.Controls.Add(this._btnStop);
            this._panelButtons.Controls.Add(this._btnManual);
            this._panelButtons.Location = new System.Drawing.Point(2, 26);
            this._panelButtons.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this._panelButtons.Name = "_panelButtons";
            this._panelButtons.Size = new System.Drawing.Size(285, 32);
            this._panelButtons.TabIndex = 0;
            // 
            // _btnBack
            // 
            this._btnBack.Location = new System.Drawing.Point(2, 4);
            this._btnBack.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this._btnBack.Name = "_btnBack";
            this._btnBack.Size = new System.Drawing.Size(56, 24);
            this._btnBack.TabIndex = 0;
            this._btnBack.Text = "Back";
            this._btnBack.UseVisualStyleBackColor = true;
            // 
            // _btnRecover
            // 
            this._btnRecover.Location = new System.Drawing.Point(201, 4);
            this._btnRecover.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this._btnRecover.Name = "_btnRecover";
            this._btnRecover.Size = new System.Drawing.Size(72, 24);
            this._btnRecover.TabIndex = 3;
            this._btnRecover.Text = "Recover";
            this._btnRecover.UseVisualStyleBackColor = true;
            // 
            // _btnStop
            // 
            this._btnStop.Location = new System.Drawing.Point(140, 4);
            this._btnStop.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this._btnStop.Name = "_btnStop";
            this._btnStop.Size = new System.Drawing.Size(56, 24);
            this._btnStop.TabIndex = 2;
            this._btnStop.Text = "Stop";
            this._btnStop.UseVisualStyleBackColor = true;
            // 
            // _btnManual
            // 
            this._btnManual.Location = new System.Drawing.Point(63, 4);
            this._btnManual.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this._btnManual.Name = "_btnManual";
            this._btnManual.Size = new System.Drawing.Size(72, 24);
            this._btnManual.TabIndex = 1;
            this._btnManual.Text = "Manual ▶";
            this._btnManual.UseVisualStyleBackColor = true;
            this._btnManual.Click += new System.EventHandler(this._btnManual_Click);
            // 
            // _cboSequence
            // 
            this._cboSequence.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cboSequence.FormattingEnabled = true;
            this._cboSequence.Location = new System.Drawing.Point(2, 5);
            this._cboSequence.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this._cboSequence.Name = "_cboSequence";
            this._cboSequence.Size = new System.Drawing.Size(286, 20);
            this._cboSequence.TabIndex = 4;
            // 
            // _lstSteps
            // 
            this._lstSteps.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._lstSteps.FormattingEnabled = true;
            this._lstSteps.ItemHeight = 12;
            this._lstSteps.Location = new System.Drawing.Point(2, 63);
            this._lstSteps.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this._lstSteps.Name = "_lstSteps";
            this._lstSteps.Size = new System.Drawing.Size(286, 160);
            this._lstSteps.TabIndex = 5;
            // 
            // ManualSequenceControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this._lstSteps);
            this.Controls.Add(this._cboSequence);
            this.Controls.Add(this._panelButtons);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "ManualSequenceControl";
            this.Size = new System.Drawing.Size(290, 236);
            this._panelButtons.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel _panelButtons;
        private System.Windows.Forms.Button _btnManual;
        private System.Windows.Forms.Button _btnStop;
        private System.Windows.Forms.Button _btnRecover;
        private System.Windows.Forms.ComboBox _cboSequence;
        private System.Windows.Forms.ListBox _lstSteps;
        private System.Windows.Forms.Button _btnBack;
    }
}
