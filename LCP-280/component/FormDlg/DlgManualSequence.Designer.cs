namespace QMC.LCP_280.Process.Component.FormDlg
{
    partial class DlgManualSequence
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
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DlgManualSequence));
            this.panelManualSequence = new System.Windows.Forms.Panel();
            this.manualSequenceControl1 = new QMC.LCP_280.Process.Component.FormDlg.ManualSequenceControl();
            this.lstIndexSocket = new System.Windows.Forms.ListBox();
            this.lblSelectedIndex = new System.Windows.Forms.Label();
            this.panelManualSequence.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelManualSequence
            // 
            this.panelManualSequence.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelManualSequence.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelManualSequence.Controls.Add(this.manualSequenceControl1);
            this.panelManualSequence.Location = new System.Drawing.Point(190, 20);
            this.panelManualSequence.Name = "panelManualSequence";
            this.panelManualSequence.Size = new System.Drawing.Size(632, 437);
            this.panelManualSequence.TabIndex = 1;
            // 
            // manualSequenceControl1
            // 
            this.manualSequenceControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.manualSequenceControl1.IndexProvider = ((System.Func<int>)(resources.GetObject("manualSequenceControl1.IndexProvider")));
            this.manualSequenceControl1.Location = new System.Drawing.Point(0, 0);
            this.manualSequenceControl1.Name = "manualSequenceControl1";
            this.manualSequenceControl1.ParentUnit = null;
            this.manualSequenceControl1.Size = new System.Drawing.Size(630, 435);
            this.manualSequenceControl1.TabIndex = 0;
            // 
            // lstIndexSocket
            // 
            this.lstIndexSocket.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.lstIndexSocket.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lstIndexSocket.FormattingEnabled = true;
            this.lstIndexSocket.ItemHeight = 26;
            this.lstIndexSocket.Location = new System.Drawing.Point(8, 63);
            this.lstIndexSocket.Name = "lstIndexSocket";
            this.lstIndexSocket.Size = new System.Drawing.Size(175, 394);
            this.lstIndexSocket.TabIndex = 0;
            this.lstIndexSocket.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.lstIndexSocket_DrawItem);
            this.lstIndexSocket.SelectedIndexChanged += new System.EventHandler(this.lstIndexSocket_SelectedIndexChanged);
            // 
            // lblSelectedIndex
            // 
            this.lblSelectedIndex.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(36)))));
            this.lblSelectedIndex.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblSelectedIndex.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblSelectedIndex.ForeColor = System.Drawing.Color.Lime;
            this.lblSelectedIndex.Location = new System.Drawing.Point(8, 20);
            this.lblSelectedIndex.Name = "lblSelectedIndex";
            this.lblSelectedIndex.Size = new System.Drawing.Size(175, 40);
            this.lblSelectedIndex.TabIndex = 2;
            this.lblSelectedIndex.Text = "INDEX : -";
            this.lblSelectedIndex.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // DlgManualSequence
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(834, 479);
            this.Controls.Add(this.lblSelectedIndex);
            this.Controls.Add(this.lstIndexSocket);
            this.Controls.Add(this.panelManualSequence);
            this.Name = "DlgManualSequence";
            this.Text = "Manual Sequence";
            this.panelManualSequence.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelManualSequence;
        private ManualSequenceControl manualSequenceControl1;
        private System.Windows.Forms.ListBox lstIndexSocket;
        private System.Windows.Forms.Label lblSelectedIndex;
    }
}

