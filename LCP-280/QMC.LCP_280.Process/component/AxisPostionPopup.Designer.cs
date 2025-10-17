namespace QMC.LCP_280.Process.Component
{
    partial class AxisPostionPopup
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.ListView listViewAxis;
        private System.Windows.Forms.ColumnHeader colAxisName;
        private System.Windows.Forms.ColumnHeader colPosition;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AxisPostionPopup));
            this.listViewAxis = new System.Windows.Forms.ListView();
            this.colAxisName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colPosition = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // listViewAxis
            // 
            this.listViewAxis.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colAxisName,
            this.colPosition});
            this.listViewAxis.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewAxis.FullRowSelect = true;
            this.listViewAxis.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewAxis.HideSelection = false;
            this.listViewAxis.Location = new System.Drawing.Point(0, 0);
            this.listViewAxis.MultiSelect = false;
            this.listViewAxis.Name = "listViewAxis";
            this.listViewAxis.OwnerDraw = true;
            this.listViewAxis.Size = new System.Drawing.Size(304, 691);
            this.listViewAxis.TabIndex = 0;
            this.listViewAxis.UseCompatibleStateImageBehavior = false;
            this.listViewAxis.View = System.Windows.Forms.View.Details;
            this.listViewAxis.DrawColumnHeader += new System.Windows.Forms.DrawListViewColumnHeaderEventHandler(this.listViewAxis_DrawColumnHeader);
            this.listViewAxis.DrawItem += new System.Windows.Forms.DrawListViewItemEventHandler(this.listViewAxis_DrawItem);
            this.listViewAxis.DrawSubItem += new System.Windows.Forms.DrawListViewSubItemEventHandler(this.listViewAxis_DrawSubItem);
            // 
            // colAxisName
            // 
            this.colAxisName.Text = "Axis Name";
            this.colAxisName.Width = 180;
            // 
            // colPosition
            // 
            this.colPosition.Text = "Pos. (mm)";
            this.colPosition.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.colPosition.Width = 100;
            // 
            // AxisPostionPopup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(304, 691);
            this.Controls.Add(this.listViewAxis);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "AxisPostionPopup";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Axis Position Monitor";
            this.ResumeLayout(false);

        }
        #endregion
    }
}