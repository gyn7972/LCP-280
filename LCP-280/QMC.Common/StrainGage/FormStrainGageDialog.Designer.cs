namespace QMC.Common.StrainGage
{
    partial class FormStrainGageDialog
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.strainGageDataGridViewer1 = new QMC.Common.StrainGage.StrainGageDataGridViewer();
            this.strainGageChart1 = new QMC.Common.StrainGage.StrainGageChart();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.strainGageChart2 = new QMC.Common.StrainGage.StrainGageChart();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tabControl, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(796, 740);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 625F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.strainGageDataGridViewer1, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(796, 200);
            this.tableLayoutPanel2.TabIndex = 1;
            // 
            // strainGageDataGridViewer1
            // 
            this.strainGageDataGridViewer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.strainGageDataGridViewer1.Location = new System.Drawing.Point(4, 4);
            this.strainGageDataGridViewer1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.strainGageDataGridViewer1.Name = "strainGageDataGridViewer1";
            this.strainGageDataGridViewer1.Size = new System.Drawing.Size(617, 192);
            this.strainGageDataGridViewer1.TabIndex = 1;
            // 
            // strainGageChart1
            // 
            this.strainGageChart1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.strainGageChart1.Location = new System.Drawing.Point(3, 3);
            this.strainGageChart1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.strainGageChart1.Name = "strainGageChart1";
            this.strainGageChart1.Size = new System.Drawing.Size(697, 442);
            this.strainGageChart1.TabIndex = 2;
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabPage1);
            this.tabControl.Controls.Add(this.tabPage2);
            this.tabControl.Location = new System.Drawing.Point(3, 203);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(711, 474);
            this.tabControl.TabIndex = 2;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.strainGageChart1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(703, 448);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "tabPage1";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.strainGageChart2);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(703, 448);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // strainGageChart2
            // 
            this.strainGageChart2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.strainGageChart2.Location = new System.Drawing.Point(3, 3);
            this.strainGageChart2.Margin = new System.Windows.Forms.Padding(4);
            this.strainGageChart2.Name = "strainGageChart2";
            this.strainGageChart2.Size = new System.Drawing.Size(697, 442);
            this.strainGageChart2.TabIndex = 3;
            // 
            // FormStrainGageDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(796, 740);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "FormStrainGageDialog";
            this.Text = "Strain Gage Monitor";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormStrainGageDialog_FormClosed);
            this.Load += new System.EventHandler(this.FormStrainGageDialog_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tabControl.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;


        private System.Windows.Forms.TabControl tabControl;

        private System.Windows.Forms.TabPage tabPage1;
        private StrainGageDataGridViewer strainGageDataGridViewer1;
        private StrainGageChart strainGageChart1;

        private System.Windows.Forms.TabPage tabPage2;
        private StrainGageDataGridViewer strainGageDataGridViewer2;
        private StrainGageChart strainGageChart2;
    }
}