//using System.Windows.Forms;
//using System.Drawing;
//using System.ComponentModel;

namespace QMC.LCP_280.Process.Sequences
{

    partial class ManualSequenceControl
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.GroupBox _gbSequence;
        private System.Windows.Forms.ListBox _lstStep;
        private System.Windows.Forms.Panel _panelButtons;
        private System.Windows.Forms.Button _btnManual;
        private System.Windows.Forms.Button _btnStop;
        private System.Windows.Forms.Button _btnRecover;
        private System.Windows.Forms.ComboBox _cboSequence; // renamed

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this._gbSequence = new System.Windows.Forms.GroupBox();
            this._cboSequence = new System.Windows.Forms.ComboBox();
            this._lstStep = new System.Windows.Forms.ListBox();
            this._panelButtons = new System.Windows.Forms.Panel();
            this._btnRecover = new System.Windows.Forms.Button();
            this._btnStop = new System.Windows.Forms.Button();
            this._btnManual = new System.Windows.Forms.Button();
            this._gbSequence.SuspendLayout();
            this._panelButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // _gbSequence
            // 
            this._gbSequence.Controls.Add(this._cboSequence);
            this._gbSequence.Controls.Add(this._lstStep);
            this._gbSequence.Controls.Add(this._panelButtons);
            this._gbSequence.Dock = System.Windows.Forms.DockStyle.Fill;
            this._gbSequence.Location = new System.Drawing.Point(0, 0);
            this._gbSequence.Name = "_gbSequence";
            this._gbSequence.Padding = new System.Windows.Forms.Padding(6, 18, 6, 6);
            this._gbSequence.Size = new System.Drawing.Size(420, 380);
            this._gbSequence.TabIndex = 0;
            this._gbSequence.TabStop = false;
            this._gbSequence.Text = "Sequence";
            // 
            // _cboSequence
            // 
            this._cboSequence.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cboSequence.FormattingEnabled = true;
            this._cboSequence.Location = new System.Drawing.Point(9, 21);
            this._cboSequence.Name = "_cboSequence";
            this._cboSequence.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                                    | System.Windows.Forms.AnchorStyles.Right)));
            this._cboSequence.Size = new System.Drawing.Size(402, 20);
            this._cboSequence.TabIndex = 2;
            // 
            // _lstStep
            // 
            this._lstStep.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
                                    | System.Windows.Forms.AnchorStyles.Left) 
                                    | System.Windows.Forms.AnchorStyles.Right)));
            this._lstStep.Font = new System.Drawing.Font("Consolas", 9F);
            this._lstStep.HorizontalScrollbar = true;
            this._lstStep.IntegralHeight = false;
            this._lstStep.ItemHeight = 14;
            this._lstStep.Location = new System.Drawing.Point(9, 49);
            this._lstStep.Name = "_lstStep";
            this._lstStep.Size = new System.Drawing.Size(266, 322);
            this._lstStep.TabIndex = 0;
            // 
            // _panelButtons
            // 
            this._panelButtons.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
                                    | System.Windows.Forms.AnchorStyles.Right)));
            this._panelButtons.Controls.Add(this._btnRecover);
            this._panelButtons.Controls.Add(this._btnStop);
            this._panelButtons.Controls.Add(this._btnManual);
            this._panelButtons.Location = new System.Drawing.Point(281, 49);
            this._panelButtons.Name = "_panelButtons";
            this._panelButtons.Size = new System.Drawing.Size(130, 322);
            this._panelButtons.TabIndex = 1;
            // 
            // _btnRecover
            // 
            this._btnRecover.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._btnRecover.Location = new System.Drawing.Point(8, 118);
            this._btnRecover.Name = "_btnRecover";
            this._btnRecover.Size = new System.Drawing.Size(114, 40);
            this._btnRecover.TabIndex = 2;
            this._btnRecover.Text = "Recover";
            this._btnRecover.UseVisualStyleBackColor = true;
            this._btnRecover.Click += new System.EventHandler(this.OnRecoverClick);
            // 
            // _btnStop
            // 
            this._btnStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._btnStop.Location = new System.Drawing.Point(8, 72);
            this._btnStop.Name = "_btnStop";
            this._btnStop.Size = new System.Drawing.Size(114, 40);
            this._btnStop.TabIndex = 1;
            this._btnStop.Text = "Stop";
            this._btnStop.UseVisualStyleBackColor = true;
            this._btnStop.Click += new System.EventHandler(this.OnStopClick);
            // 
            // _btnManual
            // 
            this._btnManual.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._btnManual.Location = new System.Drawing.Point(8, 8);
            this._btnManual.Name = "_btnManual";
            this._btnManual.Size = new System.Drawing.Size(114, 58);
            this._btnManual.TabIndex = 0;
            this._btnManual.Text = "Manual\r\nAction";
            this._btnManual.UseVisualStyleBackColor = true;
            this._btnManual.Click += new System.EventHandler(this.OnManualClick);
            // 
            // ManualSequenceControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this._gbSequence);
            this.MinimumSize = new System.Drawing.Size(260, 200);
            this.Name = "ManualSequenceControl";
            this.Size = new System.Drawing.Size(420, 380);
            this._gbSequence.ResumeLayout(false);
            this._panelButtons.ResumeLayout(false);
            this.ResumeLayout(false);

        }
    }
}
