//using System.Windows.Forms;
//using System.Drawing;
//using System.ComponentModel;

namespace QMC.LCP_280.Process.Sequences
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
            this._lstStep = new System.Windows.Forms.ListBox();
            this._panelButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // _panelButtons
            // 
            this._panelButtons.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._panelButtons.Controls.Add(this._btnBack);
            this._panelButtons.Controls.Add(this._btnRecover);
            this._panelButtons.Controls.Add(this._btnStop);
            this._panelButtons.Controls.Add(this._btnManual);
            this._panelButtons.Location = new System.Drawing.Point(3, 33);
            this._panelButtons.Name = "_panelButtons";
            this._panelButtons.Size = new System.Drawing.Size(356, 40);
            this._panelButtons.TabIndex = 0;
            this._panelButtons.Paint += new System.Windows.Forms.PaintEventHandler(this._panelButtons_Paint);
            // 
            // _btnBack
            // 
            this._btnBack.Location = new System.Drawing.Point(3, 5);
            this._btnBack.Name = "_btnBack";
            this._btnBack.Size = new System.Drawing.Size(70, 30);
            this._btnBack.TabIndex = 0;
            this._btnBack.Text = "Back";
            this._btnBack.UseVisualStyleBackColor = true;
            this._btnBack.Click += new System.EventHandler(this.OnBackClick);
            // 
            // _btnManual
            // 
            this._btnManual.Location = new System.Drawing.Point(79, 5);
            this._btnManual.Name = "_btnManual";
            this._btnManual.Size = new System.Drawing.Size(90, 30);
            this._btnManual.TabIndex = 1;
            this._btnManual.Text = "Manual ▶";
            this._btnManual.UseVisualStyleBackColor = true;
            this._btnManual.Click += new System.EventHandler(this.OnManualClick);
            // 
            // _btnStop
            // 
            this._btnStop.Location = new System.Drawing.Point(175, 5);
            this._btnStop.Name = "_btnStop";
            this._btnStop.Size = new System.Drawing.Size(70, 30);
            this._btnStop.TabIndex = 2;
            this._btnStop.Text = "Stop";
            this._btnStop.UseVisualStyleBackColor = true;
            this._btnStop.Click += new System.EventHandler(this.OnStopClick);
            // 
            // _btnRecover
            // 
            this._btnRecover.Location = new System.Drawing.Point(251, 5);
            this._btnRecover.Name = "_btnRecover";
            this._btnRecover.Size = new System.Drawing.Size(90, 30);
            this._btnRecover.TabIndex = 3;
            this._btnRecover.Text = "Recover";
            this._btnRecover.UseVisualStyleBackColor = true;
            this._btnRecover.Click += new System.EventHandler(this.OnRecoverClick);
            // 
            // _cboSequence
            // 
            this._cboSequence.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cboSequence.FormattingEnabled = true;
            this._cboSequence.Location = new System.Drawing.Point(3, 6);
            this._cboSequence.Name = "_cboSequence";
            this._cboSequence.Size = new System.Drawing.Size(356, 20);
            this._cboSequence.TabIndex = 4;
            // 
            // _lstStep
            // 
            this._lstStep.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._lstStep.FormattingEnabled = true;
            this._lstStep.ItemHeight = 12;
            this._lstStep.Location = new System.Drawing.Point(3, 79);
            this._lstStep.Name = "_lstStep";
            this._lstStep.Size = new System.Drawing.Size(356, 208);
            this._lstStep.TabIndex = 5;
            // 
            // ManualSequenceControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this._lstStep);
            this.Controls.Add(this._cboSequence);
            this.Controls.Add(this._panelButtons);
            this.Name = "ManualSequenceControl";
            this.Size = new System.Drawing.Size(362, 295);
            this._panelButtons.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel _panelButtons;
        private System.Windows.Forms.Button _btnManual;
        private System.Windows.Forms.Button _btnStop;
        private System.Windows.Forms.Button _btnRecover;
        private System.Windows.Forms.ComboBox _cboSequence;
        private System.Windows.Forms.ListBox _lstStep;
        private System.Windows.Forms.Button _btnBack;
    }
}
