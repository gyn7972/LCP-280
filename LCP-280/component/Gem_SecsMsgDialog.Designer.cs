namespace QMC.LCP_280.Process.Unit
{
    partial class Gem_SecsMsgDialog
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.ListBox lstMsg;
        private System.Windows.Forms.Button btnRemote;
        private System.Windows.Forms.Button btnLocal;
        private System.Windows.Forms.Button btnOffline;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnFtpUpload;
        private System.Windows.Forms.TableLayoutPanel tlpRoot;
        private System.Windows.Forms.FlowLayoutPanel flpButtons;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.tlpRoot = new System.Windows.Forms.TableLayoutPanel();
            this.flpButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.btnRemote = new System.Windows.Forms.Button();
            this.btnLocal = new System.Windows.Forms.Button();
            this.btnOffline = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnFtpUpload = new System.Windows.Forms.Button();
            this.lstMsg = new System.Windows.Forms.ListBox();
            this.tlpRoot.SuspendLayout();
            this.flpButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpRoot
            // 
            this.tlpRoot.ColumnCount = 1;
            this.tlpRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRoot.Controls.Add(this.flpButtons, 0, 0);
            this.tlpRoot.Controls.Add(this.lstMsg, 0, 1);
            this.tlpRoot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpRoot.Location = new System.Drawing.Point(0, 0);
            this.tlpRoot.Name = "tlpRoot";
            this.tlpRoot.RowCount = 2;
            this.tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 86F));
            this.tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRoot.Size = new System.Drawing.Size(900, 420);
            this.tlpRoot.TabIndex = 0;
            // 
            // flpButtons
            // 
            this.flpButtons.Controls.Add(this.btnRemote);
            this.flpButtons.Controls.Add(this.btnLocal);
            this.flpButtons.Controls.Add(this.btnOffline);
            this.flpButtons.Controls.Add(this.btnClear);
            this.flpButtons.Controls.Add(this.btnFtpUpload);
            this.flpButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpButtons.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold);
            this.flpButtons.Location = new System.Drawing.Point(3, 3);
            this.flpButtons.Name = "flpButtons";
            this.flpButtons.Size = new System.Drawing.Size(894, 80);
            this.flpButtons.TabIndex = 0;
            this.flpButtons.WrapContents = false;
            // 
            // btnRemote
            // 
            this.btnRemote.Location = new System.Drawing.Point(3, 3);
            this.btnRemote.Name = "btnRemote";
            this.btnRemote.Size = new System.Drawing.Size(172, 67);
            this.btnRemote.TabIndex = 0;
            this.btnRemote.Text = "REMOTE";
            this.btnRemote.UseVisualStyleBackColor = true;
            // 
            // btnLocal
            // 
            this.btnLocal.Location = new System.Drawing.Point(181, 3);
            this.btnLocal.Name = "btnLocal";
            this.btnLocal.Size = new System.Drawing.Size(172, 67);
            this.btnLocal.TabIndex = 1;
            this.btnLocal.Text = "LOCAL";
            this.btnLocal.UseVisualStyleBackColor = true;
            // 
            // btnOffline
            // 
            this.btnOffline.Location = new System.Drawing.Point(359, 3);
            this.btnOffline.Name = "btnOffline";
            this.btnOffline.Size = new System.Drawing.Size(172, 67);
            this.btnOffline.TabIndex = 2;
            this.btnOffline.Text = "OFFLINE";
            this.btnOffline.UseVisualStyleBackColor = true;
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(537, 3);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(172, 67);
            this.btnClear.TabIndex = 3;
            this.btnClear.Text = "CLEAR";
            this.btnClear.UseVisualStyleBackColor = true;
            // 
            // btnFtpUpload
            // 
            this.btnFtpUpload.Location = new System.Drawing.Point(715, 3);
            this.btnFtpUpload.Name = "btnFtpUpload";
            this.btnFtpUpload.Size = new System.Drawing.Size(172, 67);
            this.btnFtpUpload.TabIndex = 4;
            this.btnFtpUpload.Text = "FTP FILE UPLOAD";
            this.btnFtpUpload.UseVisualStyleBackColor = true;
            // 
            // lstMsg
            // 
            this.lstMsg.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstMsg.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold);
            this.lstMsg.FormattingEnabled = true;
            this.lstMsg.ItemHeight = 28;
            this.lstMsg.Location = new System.Drawing.Point(3, 89);
            this.lstMsg.Name = "lstMsg";
            this.lstMsg.Size = new System.Drawing.Size(894, 328);
            this.lstMsg.TabIndex = 1;
            // 
            // Gem_SecsMsgDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(900, 420);
            this.Controls.Add(this.tlpRoot);
            this.Name = "Gem_SecsMsgDialog";
            this.Text = "SECS Message";
            this.tlpRoot.ResumeLayout(false);
            this.flpButtons.ResumeLayout(false);
            this.ResumeLayout(false);

        }
    }
}