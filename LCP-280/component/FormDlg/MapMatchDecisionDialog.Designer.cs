using System;
using System.Drawing;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Component
{
    partial class MapMatchDecisionDialog
    {
        private System.ComponentModel.IContainer components = null;

        private Label _lblTitle;
        private Label _lblDetails;
        private Panel _panelButtons;
        private Button _btnContinue;
        private Button _btnStop;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this._lblTitle = new System.Windows.Forms.Label();
            this._lblDetails = new System.Windows.Forms.Label();
            this._panelButtons = new System.Windows.Forms.Panel();
            this._btnStop = new System.Windows.Forms.Button();
            this._btnContinue = new System.Windows.Forms.Button();
            this._panelButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // _lblTitle
            // 
            this._lblTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this._lblTitle.Font = new System.Drawing.Font("맑은 고딕", 11F, System.Drawing.FontStyle.Bold);
            this._lblTitle.Location = new System.Drawing.Point(0, 0);
            this._lblTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this._lblTitle.Name = "_lblTitle";
            this._lblTitle.Padding = new System.Windows.Forms.Padding(14, 12, 14, 0);
            this._lblTitle.Size = new System.Drawing.Size(731, 56);
            this._lblTitle.TabIndex = 2;
            this._lblTitle.Text = "스캔 맵 vs 다운로드 맵 매칭 결과";
            this._lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _lblDetails
            // 
            this._lblDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this._lblDetails.Font = new System.Drawing.Font("맑은 고딕", 10F);
            this._lblDetails.Location = new System.Drawing.Point(0, 56);
            this._lblDetails.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this._lblDetails.Name = "_lblDetails";
            this._lblDetails.Padding = new System.Windows.Forms.Padding(14, 9, 14, 9);
            this._lblDetails.Size = new System.Drawing.Size(731, 387);
            this._lblDetails.TabIndex = 0;
            // 
            // _panelButtons
            // 
            this._panelButtons.Controls.Add(this._btnStop);
            this._panelButtons.Controls.Add(this._btnContinue);
            this._panelButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._panelButtons.Location = new System.Drawing.Point(0, 443);
            this._panelButtons.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this._panelButtons.Name = "_panelButtons";
            this._panelButtons.Padding = new System.Windows.Forms.Padding(14, 12, 14, 12);
            this._panelButtons.Size = new System.Drawing.Size(731, 87);
            this._panelButtons.TabIndex = 1;
            this._panelButtons.Resize += new System.EventHandler(this._panelButtons_Resize);
            // 
            // _btnStop
            // 
            this._btnStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._btnStop.DialogResult = System.Windows.Forms.DialogResult.No;
            this._btnStop.Location = new System.Drawing.Point(918, 19);
            this._btnStop.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this._btnStop.Name = "_btnStop";
            this._btnStop.Size = new System.Drawing.Size(120, 50);
            this._btnStop.TabIndex = 0;
            this._btnStop.Text = "중단";
            // 
            // _btnContinue
            // 
            this._btnContinue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._btnContinue.DialogResult = System.Windows.Forms.DialogResult.Yes;
            this._btnContinue.Location = new System.Drawing.Point(786, 19);
            this._btnContinue.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this._btnContinue.Name = "_btnContinue";
            this._btnContinue.Size = new System.Drawing.Size(120, 50);
            this._btnContinue.TabIndex = 1;
            this._btnContinue.Text = "계속";
            // 
            // MapMatchDecisionDialog
            // 
            this.AcceptButton = this._btnContinue;
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 28F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._btnStop;
            this.ClientSize = new System.Drawing.Size(731, 530);
            this.Controls.Add(this._lblDetails);
            this.Controls.Add(this._panelButtons);
            this.Controls.Add(this._lblTitle);
            this.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MapMatchDecisionDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "맵매칭 결과 확인";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.MapMatchDecisionDialog_Load);
            this._panelButtons.ResumeLayout(false);
            this.ResumeLayout(false);

        }
    }
}