using System.Drawing;

namespace QMC.LCP_280.Process.Unit.FormMain
{
    partial class DieInputControl
    {
        private System.ComponentModel.IContainer components = null;
        private QMC.Common.Controls.DisplayView displayView1;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.lblDieCountValue = new System.Windows.Forms.Label();
            this.lblWaferIdValue = new System.Windows.Forms.Label();
            this.lblWaferIdTitle = new System.Windows.Forms.Label();
            this.lblDieCountTitle = new System.Windows.Forms.Label();
            this.displayView1 = new QMC.Common.Controls.DisplayView();
            this.tlpMain.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpMain
            // 
            this.tlpMain.ColumnCount = 1;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Controls.Add(this.tableLayoutPanel1, 0, 0);
            this.tlpMain.Controls.Add(this.displayView1, 0, 1);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(0, 0);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 2;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 18.77843F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 81.22157F));
            this.tlpMain.Size = new System.Drawing.Size(421, 370);
            this.tlpMain.TabIndex = 5;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.White;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.lblDieCountValue, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblWaferIdValue, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblWaferIdTitle, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblDieCountTitle, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(415, 63);
            this.tableLayoutPanel1.TabIndex = 5;
            // 
            // lblDieCountValue
            // 
            this.lblDieCountValue.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblDieCountValue.BackColor = System.Drawing.Color.Black;
            this.lblDieCountValue.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblDieCountValue.ForeColor = System.Drawing.Color.Lime;
            this.lblDieCountValue.Location = new System.Drawing.Point(210, 34);
            this.lblDieCountValue.Margin = new System.Windows.Forms.Padding(3);
            this.lblDieCountValue.Name = "lblDieCountValue";
            this.lblDieCountValue.Size = new System.Drawing.Size(202, 26);
            this.lblDieCountValue.TabIndex = 3;
            this.lblDieCountValue.Text = "0";
            this.lblDieCountValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblWaferIdValue
            // 
            this.lblWaferIdValue.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWaferIdValue.BackColor = System.Drawing.Color.Black;
            this.lblWaferIdValue.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblWaferIdValue.ForeColor = System.Drawing.Color.Lime;
            this.lblWaferIdValue.Location = new System.Drawing.Point(210, 3);
            this.lblWaferIdValue.Margin = new System.Windows.Forms.Padding(3);
            this.lblWaferIdValue.Name = "lblWaferIdValue";
            this.lblWaferIdValue.Size = new System.Drawing.Size(202, 25);
            this.lblWaferIdValue.TabIndex = 1;
            this.lblWaferIdValue.Text = "N/A";
            this.lblWaferIdValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblWaferIdTitle
            // 
            this.lblWaferIdTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWaferIdTitle.AutoSize = true;
            this.lblWaferIdTitle.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblWaferIdTitle.Location = new System.Drawing.Point(126, 3);
            this.lblWaferIdTitle.Margin = new System.Windows.Forms.Padding(3);
            this.lblWaferIdTitle.Name = "lblWaferIdTitle";
            this.lblWaferIdTitle.Size = new System.Drawing.Size(78, 25);
            this.lblWaferIdTitle.TabIndex = 0;
            this.lblWaferIdTitle.Text = "Wafer ID :";
            // 
            // lblDieCountTitle
            // 
            this.lblDieCountTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblDieCountTitle.AutoSize = true;
            this.lblDieCountTitle.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblDieCountTitle.Location = new System.Drawing.Point(118, 34);
            this.lblDieCountTitle.Margin = new System.Windows.Forms.Padding(3);
            this.lblDieCountTitle.Name = "lblDieCountTitle";
            this.lblDieCountTitle.Size = new System.Drawing.Size(86, 26);
            this.lblDieCountTitle.TabIndex = 2;
            this.lblDieCountTitle.Text = "Die Count :";
            // 
            // displayView1
            // 
            this.displayView1.BackColor = System.Drawing.Color.White;
            this.displayView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.displayView1.Location = new System.Drawing.Point(3, 72);
            this.displayView1.Name = "displayView1";
            this.displayView1.Size = new System.Drawing.Size(415, 295);
            this.displayView1.TabIndex = 4;
            // 
            // DieInputControl
            // 
            this.Controls.Add(this.tlpMain);
            this.Name = "DieInputControl";
            this.Size = new System.Drawing.Size(421, 370);
            this.tlpMain.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label lblDieCountValue;
        private System.Windows.Forms.Label lblWaferIdValue;
        private System.Windows.Forms.Label lblWaferIdTitle;
        private System.Windows.Forms.Label lblDieCountTitle;
    }
}
