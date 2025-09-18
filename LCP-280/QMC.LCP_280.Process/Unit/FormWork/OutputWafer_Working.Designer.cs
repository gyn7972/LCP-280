namespace QMC.LCP_280.Process.Unit.FormWork
{
    partial class OutputWafer_Working
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
            this.dioControl = new QMC.LCP_280.Process.Component.DIOControl();
            this.teachingPositionControl = new QMC.LCP_280.Process.Component.TeachingPositionControl();
            this.manualSequenceControl = new QMC.LCP_280.Process.Component.ManualSequenceControl();
            this.SuspendLayout();
            // 
            // dioControl
            // 
            this.dioControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dioControl.IoSortMode = QMC.LCP_280.Process.Component.DIOControl.SortingMode.AlphabeticalKey;
            this.dioControl.Location = new System.Drawing.Point(14, 460);
            this.dioControl.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.dioControl.Name = "dioControl";
            this.dioControl.RefreshIntervalMs = 400;
            this.dioControl.Size = new System.Drawing.Size(726, 329);
            this.dioControl.TabIndex = 15;
            // 
            // teachingPositionControl
            // 
            this.teachingPositionControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.teachingPositionControl.ButtonSize = new System.Drawing.Size(90, 32);
            this.teachingPositionControl.Location = new System.Drawing.Point(14, 15);
            this.teachingPositionControl.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.teachingPositionControl.Name = "teachingPositionControl";
            this.teachingPositionControl.ShowCancelButton = true;
            this.teachingPositionControl.ShowSaveButton = true;
            this.teachingPositionControl.Size = new System.Drawing.Size(591, 438);
            this.teachingPositionControl.TabIndex = 14;
            this.teachingPositionControl.UnitName = null;
            // 
            // manualSequenceControl
            // 
            this.manualSequenceControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.manualSequenceControl.Location = new System.Drawing.Point(1176, 15);
            this.manualSequenceControl.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.manualSequenceControl.MinimumSize = new System.Drawing.Size(297, 250);
            this.manualSequenceControl.Name = "manualSequenceControl";
            this.manualSequenceControl.Size = new System.Drawing.Size(390, 438);
            this.manualSequenceControl.TabIndex = 13;
            // 
            // OutputWafer_Working
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1580, 939);
            this.Controls.Add(this.dioControl);
            this.Controls.Add(this.teachingPositionControl);
            this.Controls.Add(this.manualSequenceControl);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "OutputWafer_Working";
            this.Text = "WaferBin_Working";
            this.ResumeLayout(false);

        }

        #endregion
        private Component.DIOControl dioControl;
        private Component.TeachingPositionControl teachingPositionControl;
        private Component.ManualSequenceControl manualSequenceControl;
    }
}