using QMC.LCP_280.Process.Component;

namespace QMC.LCP_280.Process.Unit.FormWork
{
    partial class InputWafer_Working
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
            this.manualSequenceControl = new QMC.LCP_280.Process.Sequences.ManualSequenceControl();
            this.waferMapView = new QMC.LCP_280.Process.Component.WaferMapView();
            this.btnMapping = new QMC.Common.IndividualMenuButton();
            this.SuspendLayout();
            // 
            // dioControl
            // 
            this.dioControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dioControl.IoSortMode = QMC.LCP_280.Process.Component.DIOControl.SortingMode.AlphabeticalKey;
            this.dioControl.Location = new System.Drawing.Point(12, 368);
            this.dioControl.Name = "dioControl";
            this.dioControl.RefreshIntervalMs = 400;
            this.dioControl.Size = new System.Drawing.Size(517, 263);
            this.dioControl.TabIndex = 15;
            // 
            // teachingPositionControl
            // 
            this.teachingPositionControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.teachingPositionControl.ButtonSize = new System.Drawing.Size(90, 32);
            this.teachingPositionControl.Location = new System.Drawing.Point(12, 12);
            this.teachingPositionControl.Name = "teachingPositionControl";
            this.teachingPositionControl.ShowCancelButton = true;
            this.teachingPositionControl.ShowSaveButton = true;
            this.teachingPositionControl.Size = new System.Drawing.Size(517, 350);
            this.teachingPositionControl.TabIndex = 14;
            this.teachingPositionControl.UnitName = null;
            // 
            // manualSequenceControl
            // 
            this.manualSequenceControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.manualSequenceControl.Location = new System.Drawing.Point(911, 12);
            this.manualSequenceControl.MinimumSize = new System.Drawing.Size(260, 200);
            this.manualSequenceControl.Name = "manualSequenceControl";
            this.manualSequenceControl.Size = new System.Drawing.Size(341, 350);
            this.manualSequenceControl.TabIndex = 13;
            // 
            // waferMapView
            // 
            this.waferMapView.Location = new System.Drawing.Point(911, 368);
            this.waferMapView.Name = "waferMapView";
            this.waferMapView.Size = new System.Drawing.Size(137, 164);
            this.waferMapView.TabIndex = 16;
            // 
            // btnMapping
            // 
            this.btnMapping.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnMapping.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnMapping.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnMapping.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnMapping.CustomForeColor = System.Drawing.Color.Black;
            this.btnMapping.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnMapping.ForeColor = System.Drawing.Color.Black;
            this.btnMapping.ImageSize = new System.Drawing.Size(45, 45);
            this.btnMapping.Location = new System.Drawing.Point(1054, 368);
            this.btnMapping.Name = "btnMapping";
            this.btnMapping.Size = new System.Drawing.Size(90, 35);
            this.btnMapping.TabIndex = 17;
            this.btnMapping.TabStop = false;
            this.btnMapping.Text = "Mapping";
            this.btnMapping.UseVisualStyleBackColor = false;
            this.btnMapping.Click += new System.EventHandler(this.btnMapping_Click);
            // 
            // InputWafer_Working
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1264, 751);
            this.Controls.Add(this.btnMapping);
            this.Controls.Add(this.dioControl);
            this.Controls.Add(this.teachingPositionControl);
            this.Controls.Add(this.manualSequenceControl);
            this.Controls.Add(this.waferMapView);
            this.Name = "InputWafer_Working";
            this.Text = "InputWafer_Working";
            this.ResumeLayout(false);

        }

        #endregion
        private Component.DIOControl dioControl;
        private Component.TeachingPositionControl teachingPositionControl;
        private Sequences.ManualSequenceControl manualSequenceControl;
        private WaferMapView waferMapView;
        private Common.IndividualMenuButton btnMapping;
    }
}