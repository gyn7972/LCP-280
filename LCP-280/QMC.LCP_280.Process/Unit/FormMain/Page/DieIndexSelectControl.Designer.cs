namespace QMC.LCP_280.Process.Unit.FormMain
{
    partial class DieIndexSelectControl
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblDieId;
        private System.Windows.Forms.Label lblDieIdValue;
        private System.Windows.Forms.Label lblDieNumber;
        private System.Windows.Forms.Label lblDieNumberValue;

        private System.Windows.Forms.Panel displayPanel; // 추후에 분리 해도 될듯..?

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _toolTip?.Dispose();
                _hoverTimer?.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DieIndexSelectControl));
            this.lblDieId = new System.Windows.Forms.Label();
            this.lblDieIdValue = new System.Windows.Forms.Label();
            this.lblDieNumber = new System.Windows.Forms.Label();
            this.lblDieNumberValue = new System.Windows.Forms.Label();
            this.displayPanel = new System.Windows.Forms.Panel();
            this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.btnRotateCounterClockwise = new QMC.Common.IndividualMenuButton();
            this.btnAutoSequence = new QMC.Common.IndividualMenuButton();
            this.btnReset = new QMC.Common.IndividualMenuButton();
            this.tlpMain.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblDieId
            // 
            resources.ApplyResources(this.lblDieId, "lblDieId");
            this.lblDieId.Name = "lblDieId";
            // 
            // lblDieIdValue
            // 
            resources.ApplyResources(this.lblDieIdValue, "lblDieIdValue");
            this.lblDieIdValue.BackColor = System.Drawing.Color.Black;
            this.lblDieIdValue.ForeColor = System.Drawing.Color.Lime;
            this.lblDieIdValue.Name = "lblDieIdValue";
            // 
            // lblDieNumber
            // 
            resources.ApplyResources(this.lblDieNumber, "lblDieNumber");
            this.lblDieNumber.Name = "lblDieNumber";
            // 
            // lblDieNumberValue
            // 
            resources.ApplyResources(this.lblDieNumberValue, "lblDieNumberValue");
            this.lblDieNumberValue.BackColor = System.Drawing.Color.Black;
            this.lblDieNumberValue.ForeColor = System.Drawing.Color.Lime;
            this.lblDieNumberValue.Name = "lblDieNumberValue";
            // 
            // displayPanel
            // 
            this.displayPanel.BackColor = System.Drawing.Color.White;
            resources.ApplyResources(this.displayPanel, "displayPanel");
            this.displayPanel.Name = "displayPanel";
            this.displayPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.DisplayPanel_Paint);
            this.displayPanel.MouseClick += new System.Windows.Forms.MouseEventHandler(this.DisplayPanel_MouseClick);
            this.displayPanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.DisplayPanel_MouseMove);
            this.displayPanel.Resize += new System.EventHandler(this.DisplayPanel_Resize);
            // 
            // tlpMain
            // 
            this.tlpMain.BackColor = System.Drawing.Color.White;
            resources.ApplyResources(this.tlpMain, "tlpMain");
            this.tlpMain.Controls.Add(this.displayPanel, 0, 1);
            this.tlpMain.Controls.Add(this.tableLayoutPanel1, 0, 0);
            this.tlpMain.Controls.Add(this.tableLayoutPanel2, 0, 2);
            this.tlpMain.Name = "tlpMain";
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.lblDieIdValue, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblDieId, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblDieNumberValue, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblDieNumber, 0, 1);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // tableLayoutPanel2
            // 
            resources.ApplyResources(this.tableLayoutPanel2, "tableLayoutPanel2");
            this.tableLayoutPanel2.Controls.Add(this.btnRotateCounterClockwise, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnAutoSequence, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnReset, 2, 0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            // 
            // btnRotateCounterClockwise
            // 
            this.btnRotateCounterClockwise.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            resources.ApplyResources(this.btnRotateCounterClockwise, "btnRotateCounterClockwise");
            this.btnRotateCounterClockwise.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnRotateCounterClockwise.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnRotateCounterClockwise.CustomForeColor = System.Drawing.Color.Black;
            this.btnRotateCounterClockwise.ForeColor = System.Drawing.Color.Black;
            this.btnRotateCounterClockwise.ImageSize = new System.Drawing.Size(45, 45);
            this.btnRotateCounterClockwise.Name = "btnRotateCounterClockwise";
            this.btnRotateCounterClockwise.TabStop = false;
            this.btnRotateCounterClockwise.UseVisualStyleBackColor = false;
            this.btnRotateCounterClockwise.Click += new System.EventHandler(this.btnRotateCounterClockwise_Click);
            // 
            // btnAutoSequence
            // 
            this.btnAutoSequence.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            resources.ApplyResources(this.btnAutoSequence, "btnAutoSequence");
            this.btnAutoSequence.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnAutoSequence.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnAutoSequence.CustomForeColor = System.Drawing.Color.Black;
            this.btnAutoSequence.ForeColor = System.Drawing.Color.Black;
            this.btnAutoSequence.ImageSize = new System.Drawing.Size(45, 45);
            this.btnAutoSequence.Name = "btnAutoSequence";
            this.btnAutoSequence.TabStop = false;
            this.btnAutoSequence.UseVisualStyleBackColor = false;
            this.btnAutoSequence.Click += new System.EventHandler(this.btnAutoSequence_Click);
            // 
            // btnReset
            // 
            this.btnReset.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            resources.ApplyResources(this.btnReset, "btnReset");
            this.btnReset.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnReset.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnReset.CustomForeColor = System.Drawing.Color.Black;
            this.btnReset.ForeColor = System.Drawing.Color.Black;
            this.btnReset.ImageSize = new System.Drawing.Size(45, 45);
            this.btnReset.Name = "btnReset";
            this.btnReset.TabStop = false;
            this.btnReset.UseVisualStyleBackColor = false;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // DieIndexSelectControl
            // 
            this.Controls.Add(this.tlpMain);
            resources.ApplyResources(this, "$this");
            this.Name = "DieIndexSelectControl";
            this.tlpMain.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        // Selected 레전드의 노란색 테두리를 그리기 위한 이벤트 핸들러
        private void LegendColorSelected_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            using (System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Color.Yellow, 3))
            {
                e.Graphics.DrawRectangle(pen, 1, 1, 20, 20);
            }
        }

        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Common.IndividualMenuButton btnReset;
        private Common.IndividualMenuButton btnAutoSequence;
        private Common.IndividualMenuButton btnRotateCounterClockwise;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
    }

}
