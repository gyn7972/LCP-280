namespace QMC.LCP_280.Process.Unit.FormMain
{
    partial class InputWaferCarrierControl
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblCarrierIdTitle;
        private System.Windows.Forms.Label lblWaferIdValue;
        private System.Windows.Forms.Label lblWaferCountTitle;
        private System.Windows.Forms.Label lblWaferCountValue;
        private QMC.LCP_280.Process.Component.WaferSelectMapView waferSelectMapView; // 수정: 네임스페이스 완전 지정
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
            this.lblCarrierIdTitle = new System.Windows.Forms.Label();
            this.lblWaferIdValue = new System.Windows.Forms.Label();
            this.lblWaferCountTitle = new System.Windows.Forms.Label();
            this.lblWaferCountValue = new System.Windows.Forms.Label();
            this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.waferSelectMapView = new QMC.LCP_280.Process.Component.WaferSelectMapView();
            this.tlpMain.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblCarrierIdTitle
            // 
            this.lblCarrierIdTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCarrierIdTitle.AutoSize = true;
            this.lblCarrierIdTitle.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblCarrierIdTitle.Location = new System.Drawing.Point(105, 3);
            this.lblCarrierIdTitle.Margin = new System.Windows.Forms.Padding(3);
            this.lblCarrierIdTitle.Name = "lblCarrierIdTitle";
            this.lblCarrierIdTitle.Size = new System.Drawing.Size(99, 25);
            this.lblCarrierIdTitle.TabIndex = 0;
            this.lblCarrierIdTitle.Text = "Carrier ID :";
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
            // lblWaferCountTitle
            // 
            this.lblWaferCountTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWaferCountTitle.AutoSize = true;
            this.lblWaferCountTitle.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblWaferCountTitle.Location = new System.Drawing.Point(81, 34);
            this.lblWaferCountTitle.Margin = new System.Windows.Forms.Padding(3);
            this.lblWaferCountTitle.Name = "lblWaferCountTitle";
            this.lblWaferCountTitle.Size = new System.Drawing.Size(123, 26);
            this.lblWaferCountTitle.TabIndex = 2;
            this.lblWaferCountTitle.Text = "Wafer Count :";
            // 
            // lblWaferCountValue
            // 
            this.lblWaferCountValue.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWaferCountValue.BackColor = System.Drawing.Color.Black;
            this.lblWaferCountValue.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblWaferCountValue.ForeColor = System.Drawing.Color.Lime;
            this.lblWaferCountValue.Location = new System.Drawing.Point(210, 34);
            this.lblWaferCountValue.Margin = new System.Windows.Forms.Padding(3);
            this.lblWaferCountValue.Name = "lblWaferCountValue";
            this.lblWaferCountValue.Size = new System.Drawing.Size(202, 26);
            this.lblWaferCountValue.TabIndex = 3;
            this.lblWaferCountValue.Text = "0";
            this.lblWaferCountValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tlpMain
            // 
            this.tlpMain.ColumnCount = 1;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Controls.Add(this.waferSelectMapView, 0, 1);
            this.tlpMain.Controls.Add(this.tableLayoutPanel1, 0, 0);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(0, 0);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 2;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 18.77843F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 81.22157F));
            this.tlpMain.Size = new System.Drawing.Size(421, 370);
            this.tlpMain.TabIndex = 19;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.lblWaferIdValue, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblCarrierIdTitle, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblWaferCountValue, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblWaferCountTitle, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(415, 63);
            this.tableLayoutPanel1.TabIndex = 5;
            // 
            // waferSelectMapView
            // 
            this.waferSelectMapView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.waferSelectMapView.Location = new System.Drawing.Point(4, 73);
            this.waferSelectMapView.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.waferSelectMapView.Name = "waferSelectMapView";
            this.waferSelectMapView.Size = new System.Drawing.Size(413, 293);
            this.waferSelectMapView.TabIndex = 18;
            // 
            // InputWaferCarrierControl
            // 
            this.Controls.Add(this.tlpMain);
            this.Name = "InputWaferCarrierControl";
            this.Size = new System.Drawing.Size(421, 370);
            this.tlpMain.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}
