namespace QMC.LCP_280.Process.Component
{
    partial class ProcessDataManual
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.GroupBox groupConfig;
        private System.Windows.Forms.TextBox txtConfigPath;
        private System.Windows.Forms.Button btnInit;

        private System.Windows.Forms.GroupBox groupWafer;
        private System.Windows.Forms.TextBox txtCarrierId;
        private System.Windows.Forms.Label lblCarrierId;
        private System.Windows.Forms.NumericUpDown nudSlot;
        private System.Windows.Forms.Label lblSlot;
        private System.Windows.Forms.TextBox txtLotId;
        private System.Windows.Forms.Label lblLotId;
        private System.Windows.Forms.TextBox txtRecipeKeys;
        private System.Windows.Forms.Label lblRecipeKeys;
        private System.Windows.Forms.Button btnLoadNewLot;
        private System.Windows.Forms.Button btnSetRecipeKeys;
        private System.Windows.Forms.Button btnAddSampleChips;

        private System.Windows.Forms.GroupBox groupChips;
        private System.Windows.Forms.ListView lvChips;
        private System.Windows.Forms.ColumnHeader chIndex;
        private System.Windows.Forms.ColumnHeader chMapX;
        private System.Windows.Forms.ColumnHeader chMapY;
        private System.Windows.Forms.ColumnHeader chState;
        private System.Windows.Forms.ColumnHeader chIsPass;
        private System.Windows.Forms.ColumnHeader chMeasures;
        private System.Windows.Forms.Button btnSimulateInspect;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Label lblStatus;

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
            this.components = new System.ComponentModel.Container();
            this.groupConfig = new System.Windows.Forms.GroupBox();
            this.txtConfigPath = new System.Windows.Forms.TextBox();
            this.btnInit = new System.Windows.Forms.Button();
            this.groupWafer = new System.Windows.Forms.GroupBox();
            this.btnAddSampleChips = new System.Windows.Forms.Button();
            this.btnSetRecipeKeys = new System.Windows.Forms.Button();
            this.btnLoadNewLot = new System.Windows.Forms.Button();
            this.lblRecipeKeys = new System.Windows.Forms.Label();
            this.txtRecipeKeys = new System.Windows.Forms.TextBox();
            this.lblLotId = new System.Windows.Forms.Label();
            this.txtLotId = new System.Windows.Forms.TextBox();
            this.lblSlot = new System.Windows.Forms.Label();
            this.nudSlot = new System.Windows.Forms.NumericUpDown();
            this.lblCarrierId = new System.Windows.Forms.Label();
            this.txtCarrierId = new System.Windows.Forms.TextBox();
            this.groupChips = new System.Windows.Forms.GroupBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnSimulateInspect = new System.Windows.Forms.Button();
            this.lvChips = new System.Windows.Forms.ListView();
            this.chIndex = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chMapX = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chMapY = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chState = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chIsPass = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chMeasures = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lblStatus = new System.Windows.Forms.Label();
            this.groupConfig.SuspendLayout();
            this.groupWafer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudSlot)).BeginInit();
            this.groupChips.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupConfig
            // 
            this.groupConfig.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupConfig.Controls.Add(this.txtConfigPath);
            this.groupConfig.Controls.Add(this.btnInit);
            this.groupConfig.Location = new System.Drawing.Point(12, 12);
            this.groupConfig.Name = "groupConfig";
            this.groupConfig.Size = new System.Drawing.Size(960, 58);
            this.groupConfig.TabIndex = 0;
            this.groupConfig.TabStop = false;
            this.groupConfig.Text = "Config";
            // 
            // txtConfigPath
            // 
            this.txtConfigPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtConfigPath.Location = new System.Drawing.Point(16, 22);
            this.txtConfigPath.Name = "txtConfigPath";
            this.txtConfigPath.ReadOnly = true;
            this.txtConfigPath.Size = new System.Drawing.Size(836, 21);
            this.txtConfigPath.TabIndex = 1;
            // 
            // btnInit
            // 
            this.btnInit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnInit.Location = new System.Drawing.Point(858, 20);
            this.btnInit.Name = "btnInit";
            this.btnInit.Size = new System.Drawing.Size(88, 24);
            this.btnInit.TabIndex = 0;
            this.btnInit.Text = "Initialize";
            this.btnInit.UseVisualStyleBackColor = true;
            this.btnInit.Click += new System.EventHandler(this.btnInit_Click);
            // 
            // groupWafer
            // 
            this.groupWafer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupWafer.Controls.Add(this.btnAddSampleChips);
            this.groupWafer.Controls.Add(this.btnSetRecipeKeys);
            this.groupWafer.Controls.Add(this.btnLoadNewLot);
            this.groupWafer.Controls.Add(this.lblRecipeKeys);
            this.groupWafer.Controls.Add(this.txtRecipeKeys);
            this.groupWafer.Controls.Add(this.lblLotId);
            this.groupWafer.Controls.Add(this.txtLotId);
            this.groupWafer.Controls.Add(this.lblSlot);
            this.groupWafer.Controls.Add(this.nudSlot);
            this.groupWafer.Controls.Add(this.lblCarrierId);
            this.groupWafer.Controls.Add(this.txtCarrierId);
            this.groupWafer.Location = new System.Drawing.Point(12, 76);
            this.groupWafer.Name = "groupWafer";
            this.groupWafer.Size = new System.Drawing.Size(960, 142);
            this.groupWafer.TabIndex = 1;
            this.groupWafer.TabStop = false;
            this.groupWafer.Text = "Wafer Setup";
            // 
            // btnAddSampleChips
            // 
            this.btnAddSampleChips.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddSampleChips.Location = new System.Drawing.Point(858, 102);
            this.btnAddSampleChips.Name = "btnAddSampleChips";
            this.btnAddSampleChips.Size = new System.Drawing.Size(88, 24);
            this.btnAddSampleChips.TabIndex = 10;
            this.btnAddSampleChips.Text = "+ Chips";
            this.btnAddSampleChips.UseVisualStyleBackColor = true;
            this.btnAddSampleChips.Click += new System.EventHandler(this.btnAddSampleChips_Click);
            // 
            // btnSetRecipeKeys
            // 
            this.btnSetRecipeKeys.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSetRecipeKeys.Location = new System.Drawing.Point(858, 62);
            this.btnSetRecipeKeys.Name = "btnSetRecipeKeys";
            this.btnSetRecipeKeys.Size = new System.Drawing.Size(88, 24);
            this.btnSetRecipeKeys.TabIndex = 9;
            this.btnSetRecipeKeys.Text = "Set Keys";
            this.btnSetRecipeKeys.UseVisualStyleBackColor = true;
            this.btnSetRecipeKeys.Click += new System.EventHandler(this.btnSetRecipeKeys_Click);
            // 
            // btnLoadNewLot
            // 
            this.btnLoadNewLot.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLoadNewLot.Location = new System.Drawing.Point(858, 22);
            this.btnLoadNewLot.Name = "btnLoadNewLot";
            this.btnLoadNewLot.Size = new System.Drawing.Size(88, 24);
            this.btnLoadNewLot.TabIndex = 8;
            this.btnLoadNewLot.Text = "Load Lot";
            this.btnLoadNewLot.UseVisualStyleBackColor = true;
            this.btnLoadNewLot.Click += new System.EventHandler(this.btnLoadNewLot_Click);
            // 
            // lblRecipeKeys
            // 
            this.lblRecipeKeys.AutoSize = true;
            this.lblRecipeKeys.Location = new System.Drawing.Point(14, 68);
            this.lblRecipeKeys.Name = "lblRecipeKeys";
            this.lblRecipeKeys.Size = new System.Drawing.Size(164, 12);
            this.lblRecipeKeys.TabIndex = 7;
            this.lblRecipeKeys.Text = "Recipe Keys (comma or NL)";
            // 
            // txtRecipeKeys
            // 
            this.txtRecipeKeys.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtRecipeKeys.Location = new System.Drawing.Point(184, 64);
            this.txtRecipeKeys.Multiline = true;
            this.txtRecipeKeys.Name = "txtRecipeKeys";
            this.txtRecipeKeys.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtRecipeKeys.Size = new System.Drawing.Size(660, 62);
            this.txtRecipeKeys.TabIndex = 6;
            // 
            // lblLotId
            // 
            this.lblLotId.AutoSize = true;
            this.lblLotId.Location = new System.Drawing.Point(474, 28);
            this.lblLotId.Name = "lblLotId";
            this.lblLotId.Size = new System.Drawing.Size(43, 12);
            this.lblLotId.TabIndex = 5;
            this.lblLotId.Text = "Lot ID";
            // 
            // txtLotId
            // 
            this.txtLotId.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLotId.Location = new System.Drawing.Point(523, 24);
            this.txtLotId.Name = "txtLotId";
            this.txtLotId.Size = new System.Drawing.Size(321, 21);
            this.txtLotId.TabIndex = 4;
            // 
            // lblSlot
            // 
            this.lblSlot.AutoSize = true;
            this.lblSlot.Location = new System.Drawing.Point(306, 28);
            this.lblSlot.Name = "lblSlot";
            this.lblSlot.Size = new System.Drawing.Size(27, 12);
            this.lblSlot.TabIndex = 3;
            this.lblSlot.Text = "Slot";
            // 
            // nudSlot
            // 
            this.nudSlot.Location = new System.Drawing.Point(339, 24);
            this.nudSlot.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.nudSlot.Name = "nudSlot";
            this.nudSlot.Size = new System.Drawing.Size(66, 21);
            this.nudSlot.TabIndex = 2;
            // 
            // lblCarrierId
            // 
            this.lblCarrierId.AutoSize = true;
            this.lblCarrierId.Location = new System.Drawing.Point(14, 28);
            this.lblCarrierId.Name = "lblCarrierId";
            this.lblCarrierId.Size = new System.Drawing.Size(61, 12);
            this.lblCarrierId.TabIndex = 1;
            this.lblCarrierId.Text = "Carrier ID";
            // 
            // txtCarrierId
            // 
            this.txtCarrierId.Location = new System.Drawing.Point(81, 24);
            this.txtCarrierId.Name = "txtCarrierId";
            this.txtCarrierId.Size = new System.Drawing.Size(209, 21);
            this.txtCarrierId.TabIndex = 0;
            // 
            // groupChips
            // 
            this.groupChips.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupChips.Controls.Add(this.btnSave);
            this.groupChips.Controls.Add(this.btnSimulateInspect);
            this.groupChips.Controls.Add(this.lvChips);
            this.groupChips.Location = new System.Drawing.Point(12, 224);
            this.groupChips.Name = "groupChips";
            this.groupChips.Size = new System.Drawing.Size(960, 375);
            this.groupChips.TabIndex = 2;
            this.groupChips.TabStop = false;
            this.groupChips.Text = "Chips";
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Location = new System.Drawing.Point(858, 343);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(88, 24);
            this.btnSave.TabIndex = 2;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnSimulateInspect
            // 
            this.btnSimulateInspect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSimulateInspect.Location = new System.Drawing.Point(710, 343);
            this.btnSimulateInspect.Name = "btnSimulateInspect";
            this.btnSimulateInspect.Size = new System.Drawing.Size(142, 24);
            this.btnSimulateInspect.TabIndex = 1;
            this.btnSimulateInspect.Text = "Simulate Inspect";
            this.btnSimulateInspect.UseVisualStyleBackColor = true;
            this.btnSimulateInspect.Click += new System.EventHandler(this.btnSimulateInspect_Click);
            // 
            // lvChips
            // 
            this.lvChips.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvChips.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chIndex,
            this.chMapX,
            this.chMapY,
            this.chState,
            this.chIsPass,
            this.chMeasures});
            this.lvChips.FullRowSelect = true;
            this.lvChips.GridLines = true;
            this.lvChips.HideSelection = false;
            this.lvChips.Location = new System.Drawing.Point(16, 22);
            this.lvChips.MultiSelect = false;
            this.lvChips.Name = "lvChips";
            this.lvChips.Size = new System.Drawing.Size(930, 315);
            this.lvChips.TabIndex = 0;
            this.lvChips.UseCompatibleStateImageBehavior = false;
            this.lvChips.View = System.Windows.Forms.View.Details;
            this.lvChips.SelectedIndexChanged += new System.EventHandler(this.lvChips_SelectedIndexChanged);
            // 
            // chIndex
            // 
            this.chIndex.Text = "Index";
            this.chIndex.Width = 60;
            // 
            // chMapX
            // 
            this.chMapX.Text = "MapX";
            this.chMapX.Width = 60;
            // 
            // chMapY
            // 
            this.chMapY.Text = "MapY";
            this.chMapY.Width = 60;
            // 
            // chState
            // 
            this.chState.Text = "State";
            this.chState.Width = 120;
            // 
            // chIsPass
            // 
            this.chIsPass.Text = "Pass";
            this.chIsPass.Width = 60;
            // 
            // chMeasures
            // 
            this.chMeasures.Text = "Measures";
            this.chMeasures.Width = 520;
            // 
            // lblStatus
            // 
            this.lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStatus.Location = new System.Drawing.Point(12, 602);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(960, 23);
            this.lblStatus.TabIndex = 3;
            this.lblStatus.Text = "Ready";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ProcessDataManual
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(984, 631);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.groupChips);
            this.Controls.Add(this.groupWafer);
            this.Controls.Add(this.groupConfig);
            this.MinimumSize = new System.Drawing.Size(800, 600);
            this.Name = "ProcessDataManual";
            this.Text = "Process Data Manual Test";
            this.Load += new System.EventHandler(this.ProcessDataManual_Load);
            this.groupConfig.ResumeLayout(false);
            this.groupConfig.PerformLayout();
            this.groupWafer.ResumeLayout(false);
            this.groupWafer.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudSlot)).EndInit();
            this.groupChips.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
    }
}