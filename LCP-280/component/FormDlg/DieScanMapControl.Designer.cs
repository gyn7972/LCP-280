namespace QMC.LCP_280.Process.Component
{
    partial class DieScanMapControl
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.lblDieCountValue = new System.Windows.Forms.Label();
            this.lblWaferIdValue = new System.Windows.Forms.Label();
            this.lblWaferIdTitle = new System.Windows.Forms.Label();
            this.lblDieCountTitle = new System.Windows.Forms.Label();
            this.displayView1 = new QMC.Common.Controls.DisplayView_DieScanMap();
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
            this.tlpMain.RowCount = 3;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 19.43093F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 78.88631F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 1.856148F));
            this.tlpMain.Size = new System.Drawing.Size(494, 431);
            this.tlpMain.TabIndex = 6;
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
            this.tableLayoutPanel1.Size = new System.Drawing.Size(488, 77);
            this.tableLayoutPanel1.TabIndex = 5;
            // 
            // lblDieCountValue
            // 
            this.lblDieCountValue.BackColor = System.Drawing.Color.Black;
            this.lblDieCountValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDieCountValue.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblDieCountValue.ForeColor = System.Drawing.Color.Lime;
            this.lblDieCountValue.Location = new System.Drawing.Point(247, 41);
            this.lblDieCountValue.Margin = new System.Windows.Forms.Padding(3);
            this.lblDieCountValue.Name = "lblDieCountValue";
            this.lblDieCountValue.Size = new System.Drawing.Size(238, 33);
            this.lblDieCountValue.TabIndex = 3;
            this.lblDieCountValue.Text = "0";
            this.lblDieCountValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblWaferIdValue
            // 
            this.lblWaferIdValue.BackColor = System.Drawing.Color.Black;
            this.lblWaferIdValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblWaferIdValue.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblWaferIdValue.ForeColor = System.Drawing.Color.Lime;
            this.lblWaferIdValue.Location = new System.Drawing.Point(247, 3);
            this.lblWaferIdValue.Margin = new System.Windows.Forms.Padding(3);
            this.lblWaferIdValue.Name = "lblWaferIdValue";
            this.lblWaferIdValue.Size = new System.Drawing.Size(238, 32);
            this.lblWaferIdValue.TabIndex = 1;
            this.lblWaferIdValue.Text = "N/A";
            this.lblWaferIdValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblWaferIdTitle
            // 
            this.lblWaferIdTitle.AutoSize = true;
            this.lblWaferIdTitle.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblWaferIdTitle.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblWaferIdTitle.Location = new System.Drawing.Point(126, 3);
            this.lblWaferIdTitle.Margin = new System.Windows.Forms.Padding(3);
            this.lblWaferIdTitle.Name = "lblWaferIdTitle";
            this.lblWaferIdTitle.Size = new System.Drawing.Size(115, 32);
            this.lblWaferIdTitle.TabIndex = 0;
            this.lblWaferIdTitle.Text = "Wafer ID :";
            // 
            // lblDieCountTitle
            // 
            this.lblDieCountTitle.AutoSize = true;
            this.lblDieCountTitle.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblDieCountTitle.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblDieCountTitle.Location = new System.Drawing.Point(115, 41);
            this.lblDieCountTitle.Margin = new System.Windows.Forms.Padding(3);
            this.lblDieCountTitle.Name = "lblDieCountTitle";
            this.lblDieCountTitle.Size = new System.Drawing.Size(126, 33);
            this.lblDieCountTitle.TabIndex = 2;
            this.lblDieCountTitle.Text = "Die Count :";
            // 
            // displayView1
            // 
            this.displayView1.BackColor = System.Drawing.Color.White;
            this.displayView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.displayView1.Location = new System.Drawing.Point(3, 86);
            this.displayView1.Name = "displayView1";
            this.displayView1.Size = new System.Drawing.Size(488, 333);
            this.displayView1.TabIndex = 4;
            // 
            // DieScanMapControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.tlpMain);
            this.Name = "DieScanMapControl";
            this.Size = new System.Drawing.Size(494, 431);
            this.tlpMain.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label lblDieCountValue;
        private System.Windows.Forms.Label lblWaferIdValue;
        private System.Windows.Forms.Label lblWaferIdTitle;
        private System.Windows.Forms.Label lblDieCountTitle;
        private Common.Controls.DisplayView_DieScanMap displayView1;
    }
}