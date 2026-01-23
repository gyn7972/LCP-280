namespace QMC.LCP_280.Process.Unit.FormMain
{
    partial class DieOutputControl
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblWaferIdTitle;
        private System.Windows.Forms.Label lblWaferIdValue;
        private System.Windows.Forms.Label lblDieCountTitle;
        private System.Windows.Forms.Label lblDieCountValue;
        private Common.Controls.DisplayView_DieOutput displayView1;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblWaferIdTitle = new System.Windows.Forms.Label();
            this.lblWaferIdValue = new System.Windows.Forms.Label();
            this.lblDieCountTitle = new System.Windows.Forms.Label();
            this.lblDieCountValue = new System.Windows.Forms.Label();
            this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
            this.displayView1 = new QMC.Common.Controls.DisplayView_DieOutput();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.btnReset = new QMC.Common.IndividualMenuButton();
            this.tlpMain.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblWaferIdTitle
            // 
            this.lblWaferIdTitle.AutoSize = true;
            this.lblWaferIdTitle.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblWaferIdTitle.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblWaferIdTitle.Location = new System.Drawing.Point(20, 3);
            this.lblWaferIdTitle.Margin = new System.Windows.Forms.Padding(3);
            this.lblWaferIdTitle.Name = "lblWaferIdTitle";
            this.lblWaferIdTitle.Size = new System.Drawing.Size(115, 25);
            this.lblWaferIdTitle.TabIndex = 0;
            this.lblWaferIdTitle.Text = "Wafer ID :";
            // 
            // lblWaferIdValue
            // 
            this.lblWaferIdValue.BackColor = System.Drawing.Color.Black;
            this.lblWaferIdValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblWaferIdValue.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblWaferIdValue.ForeColor = System.Drawing.Color.Lime;
            this.lblWaferIdValue.Location = new System.Drawing.Point(141, 3);
            this.lblWaferIdValue.Margin = new System.Windows.Forms.Padding(3);
            this.lblWaferIdValue.Name = "lblWaferIdValue";
            this.lblWaferIdValue.Size = new System.Drawing.Size(201, 25);
            this.lblWaferIdValue.TabIndex = 1;
            this.lblWaferIdValue.Text = "N/A";
            this.lblWaferIdValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblDieCountTitle
            // 
            this.lblDieCountTitle.AutoSize = true;
            this.lblDieCountTitle.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblDieCountTitle.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblDieCountTitle.Location = new System.Drawing.Point(9, 34);
            this.lblDieCountTitle.Margin = new System.Windows.Forms.Padding(3);
            this.lblDieCountTitle.Name = "lblDieCountTitle";
            this.lblDieCountTitle.Size = new System.Drawing.Size(126, 26);
            this.lblDieCountTitle.TabIndex = 2;
            this.lblDieCountTitle.Text = "Die Count :";
            // 
            // lblDieCountValue
            // 
            this.lblDieCountValue.BackColor = System.Drawing.Color.Black;
            this.lblDieCountValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDieCountValue.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblDieCountValue.ForeColor = System.Drawing.Color.Lime;
            this.lblDieCountValue.Location = new System.Drawing.Point(141, 34);
            this.lblDieCountValue.Margin = new System.Windows.Forms.Padding(3);
            this.lblDieCountValue.Name = "lblDieCountValue";
            this.lblDieCountValue.Size = new System.Drawing.Size(201, 26);
            this.lblDieCountValue.TabIndex = 3;
            this.lblDieCountValue.Text = "0";
            this.lblDieCountValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tlpMain
            // 
            this.tlpMain.ColumnCount = 1;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Controls.Add(this.displayView1, 0, 1);
            this.tlpMain.Controls.Add(this.tableLayoutPanel1, 0, 0);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(0, 0);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 2;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 18.77843F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 81.22157F));
            this.tlpMain.Size = new System.Drawing.Size(421, 370);
            this.tlpMain.TabIndex = 6;
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
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.White;
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel1.Controls.Add(this.btnReset, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblDieCountTitle, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblWaferIdTitle, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblDieCountValue, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblWaferIdValue, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(415, 63);
            this.tableLayoutPanel1.TabIndex = 5;
            // 
            // btnReset
            // 
            this.btnReset.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnReset.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnReset.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnReset.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnReset.CustomForeColor = System.Drawing.Color.Black;
            this.btnReset.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnReset.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnReset.ForeColor = System.Drawing.Color.Black;
            this.btnReset.ImageSize = new System.Drawing.Size(45, 45);
            this.btnReset.Location = new System.Drawing.Point(349, 4);
            this.btnReset.Margin = new System.Windows.Forms.Padding(4);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(62, 23);
            this.btnReset.TabIndex = 18;
            this.btnReset.TabStop = false;
            this.btnReset.Text = "Reset";
            this.btnReset.UseVisualStyleBackColor = false;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // DieOutputControl
            // 
            this.Controls.Add(this.tlpMain);
            this.Name = "DieOutputControl";
            this.Size = new System.Drawing.Size(421, 370);
            this.tlpMain.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Common.IndividualMenuButton btnReset;
    }
}
