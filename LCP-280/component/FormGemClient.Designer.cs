namespace QMC.LCP_280.Process.Component
{
    partial class FormGemClient
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
            this.lblStatus = new System.Windows.Forms.Label();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.btnConnect = new System.Windows.Forms.Button();
            this.grpConnection = new System.Windows.Forms.GroupBox();
            this.grpState = new System.Windows.Forms.GroupBox();
            this.btnControlState = new System.Windows.Forms.Button();
            this.btnEqState = new System.Windows.Forms.Button();
            this.grpRecipe = new System.Windows.Forms.GroupBox();
            this.txtRecipe = new System.Windows.Forms.TextBox();
            this.btnPPSelect = new System.Windows.Forms.Button();
            this.btnReportTray = new System.Windows.Forms.Button();
            this.grpTransfer = new System.Windows.Forms.GroupBox();
            this.lblPort = new System.Windows.Forms.Label();
            this.txtPortId = new System.Windows.Forms.TextBox();
            this.lblLot = new System.Windows.Forms.Label();
            this.txtLotId = new System.Windows.Forms.TextBox();
            this.btnPortLoad = new System.Windows.Forms.Button();
            this.btnPortUnload = new System.Windows.Forms.Button();
            this.btnWaferLoad = new System.Windows.Forms.Button();
            this.grpProcess = new System.Windows.Forms.GroupBox();
            this.btnProcStart = new System.Windows.Forms.Button();
            this.btnProcEnd = new System.Windows.Forms.Button();
            this.grpAlarm = new System.Windows.Forms.GroupBox();
            this.lblAid = new System.Windows.Forms.Label();
            this.txtAlarmId = new System.Windows.Forms.TextBox();
            this.lblAText = new System.Windows.Forms.Label();
            this.txtAlarmText = new System.Windows.Forms.TextBox();
            this.btnAlarmSet = new System.Windows.Forms.Button();
            this.btnAlarmClear = new System.Windows.Forms.Button();
            this.grpConnection.SuspendLayout();
            this.grpState.SuspendLayout();
            this.grpRecipe.SuspendLayout();
            this.grpTransfer.SuspendLayout();
            this.grpProcess.SuspendLayout();
            this.grpAlarm.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblStatus
            // 
            this.lblStatus.BackColor = System.Drawing.Color.Red;
            this.lblStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblStatus.ForeColor = System.Drawing.Color.White;
            this.lblStatus.Location = new System.Drawing.Point(14, 30);
            this.lblStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(256, 36);
            this.lblStatus.TabIndex = 0;
            this.lblStatus.Text = "GEM Disconnected";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtLog
            // 
            this.txtLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLog.Location = new System.Drawing.Point(17, 338);
            this.txtLog.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(997, 366);
            this.txtLog.TabIndex = 6;
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(14, 75);
            this.btnConnect.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(257, 34);
            this.btnConnect.TabIndex = 1;
            this.btnConnect.Text = "Connect / Disconnect";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // grpConnection
            // 
            this.grpConnection.Controls.Add(this.lblStatus);
            this.grpConnection.Controls.Add(this.btnConnect);
            this.grpConnection.Location = new System.Drawing.Point(17, 18);
            this.grpConnection.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.grpConnection.Name = "grpConnection";
            this.grpConnection.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.grpConnection.Size = new System.Drawing.Size(286, 120);
            this.grpConnection.TabIndex = 0;
            this.grpConnection.TabStop = false;
            this.grpConnection.Text = "Connection";
            // 
            // grpState
            // 
            this.grpState.Controls.Add(this.btnControlState);
            this.grpState.Controls.Add(this.btnEqState);
            this.grpState.Location = new System.Drawing.Point(314, 18);
            this.grpState.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.grpState.Name = "grpState";
            this.grpState.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.grpState.Size = new System.Drawing.Size(257, 120);
            this.grpState.TabIndex = 1;
            this.grpState.TabStop = false;
            this.grpState.Text = "State Control";
            // 
            // btnControlState
            // 
            this.btnControlState.Location = new System.Drawing.Point(14, 30);
            this.btnControlState.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnControlState.Name = "btnControlState";
            this.btnControlState.Size = new System.Drawing.Size(229, 34);
            this.btnControlState.TabIndex = 0;
            this.btnControlState.Text = "Control State (Remote)";
            this.btnControlState.UseVisualStyleBackColor = true;
            this.btnControlState.Click += new System.EventHandler(this.btnControlState_Click);
            // 
            // btnEqState
            // 
            this.btnEqState.Location = new System.Drawing.Point(14, 75);
            this.btnEqState.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnEqState.Name = "btnEqState";
            this.btnEqState.Size = new System.Drawing.Size(229, 34);
            this.btnEqState.TabIndex = 1;
            this.btnEqState.Text = "EQ State (Run)";
            this.btnEqState.UseVisualStyleBackColor = true;
            this.btnEqState.Click += new System.EventHandler(this.btnEqState_Click);
            // 
            // grpRecipe
            // 
            this.grpRecipe.Controls.Add(this.txtRecipe);
            this.grpRecipe.Controls.Add(this.btnPPSelect);
            this.grpRecipe.Controls.Add(this.btnReportTray);
            this.grpRecipe.Location = new System.Drawing.Point(586, 18);
            this.grpRecipe.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.grpRecipe.Name = "grpRecipe";
            this.grpRecipe.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.grpRecipe.Size = new System.Drawing.Size(286, 120);
            this.grpRecipe.TabIndex = 2;
            this.grpRecipe.TabStop = false;
            this.grpRecipe.Text = "Recipe (PP)";
            // 
            // txtRecipe
            // 
            this.txtRecipe.Location = new System.Drawing.Point(14, 33);
            this.txtRecipe.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtRecipe.Name = "txtRecipe";
            this.txtRecipe.Size = new System.Drawing.Size(141, 28);
            this.txtRecipe.TabIndex = 0;
            this.txtRecipe.Text = "RECIPE_001";
            // 
            // btnPPSelect
            // 
            this.btnPPSelect.Location = new System.Drawing.Point(164, 30);
            this.btnPPSelect.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnPPSelect.Name = "btnPPSelect";
            this.btnPPSelect.Size = new System.Drawing.Size(107, 38);
            this.btnPPSelect.TabIndex = 1;
            this.btnPPSelect.Text = "Select";
            this.btnPPSelect.UseVisualStyleBackColor = true;
            this.btnPPSelect.Click += new System.EventHandler(this.btnPPSelect_Click);
            // 
            // btnReportTray
            // 
            this.btnReportTray.Location = new System.Drawing.Point(14, 75);
            this.btnReportTray.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnReportTray.Name = "btnReportTray";
            this.btnReportTray.Size = new System.Drawing.Size(257, 34);
            this.btnReportTray.TabIndex = 2;
            this.btnReportTray.Text = "Tray Report";
            this.btnReportTray.UseVisualStyleBackColor = true;
            this.btnReportTray.Click += new System.EventHandler(this.btnReportTray_Click);
            // 
            // grpTransfer
            // 
            this.grpTransfer.Controls.Add(this.lblPort);
            this.grpTransfer.Controls.Add(this.txtPortId);
            this.grpTransfer.Controls.Add(this.lblLot);
            this.grpTransfer.Controls.Add(this.txtLotId);
            this.grpTransfer.Controls.Add(this.btnPortLoad);
            this.grpTransfer.Controls.Add(this.btnPortUnload);
            this.grpTransfer.Controls.Add(this.btnWaferLoad);
            this.grpTransfer.Location = new System.Drawing.Point(17, 150);
            this.grpTransfer.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.grpTransfer.Name = "grpTransfer";
            this.grpTransfer.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.grpTransfer.Size = new System.Drawing.Size(400, 180);
            this.grpTransfer.TabIndex = 3;
            this.grpTransfer.TabStop = false;
            this.grpTransfer.Text = "Transfer (Port/Lot)";
            // 
            // lblPort
            // 
            this.lblPort.Location = new System.Drawing.Point(14, 38);
            this.lblPort.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(71, 30);
            this.lblPort.TabIndex = 0;
            this.lblPort.Text = "Port ID:";
            // 
            // txtPortId
            // 
            this.txtPortId.Location = new System.Drawing.Point(86, 33);
            this.txtPortId.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtPortId.Name = "txtPortId";
            this.txtPortId.Size = new System.Drawing.Size(113, 28);
            this.txtPortId.TabIndex = 1;
            this.txtPortId.Text = "PORT1";
            // 
            // lblLot
            // 
            this.lblLot.Location = new System.Drawing.Point(14, 75);
            this.lblLot.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblLot.Name = "lblLot";
            this.lblLot.Size = new System.Drawing.Size(71, 30);
            this.lblLot.TabIndex = 2;
            this.lblLot.Text = "Lot ID:";
            // 
            // txtLotId
            // 
            this.txtLotId.Location = new System.Drawing.Point(86, 70);
            this.txtLotId.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtLotId.Name = "txtLotId";
            this.txtLotId.Size = new System.Drawing.Size(113, 28);
            this.txtLotId.TabIndex = 3;
            this.txtLotId.Text = "LOT_ABC";
            // 
            // btnPortLoad
            // 
            this.btnPortLoad.Location = new System.Drawing.Point(214, 30);
            this.btnPortLoad.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnPortLoad.Name = "btnPortLoad";
            this.btnPortLoad.Size = new System.Drawing.Size(171, 34);
            this.btnPortLoad.TabIndex = 4;
            this.btnPortLoad.Text = "Port Load";
            this.btnPortLoad.UseVisualStyleBackColor = true;
            this.btnPortLoad.Click += new System.EventHandler(this.btnPortLoad_Click);
            // 
            // btnPortUnload
            // 
            this.btnPortUnload.Location = new System.Drawing.Point(214, 70);
            this.btnPortUnload.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnPortUnload.Name = "btnPortUnload";
            this.btnPortUnload.Size = new System.Drawing.Size(171, 34);
            this.btnPortUnload.TabIndex = 5;
            this.btnPortUnload.Text = "Port Unload";
            this.btnPortUnload.UseVisualStyleBackColor = true;
            this.btnPortUnload.Click += new System.EventHandler(this.btnPortUnload_Click);
            // 
            // btnWaferLoad
            // 
            this.btnWaferLoad.Location = new System.Drawing.Point(214, 111);
            this.btnWaferLoad.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnWaferLoad.Name = "btnWaferLoad";
            this.btnWaferLoad.Size = new System.Drawing.Size(171, 34);
            this.btnWaferLoad.TabIndex = 6;
            this.btnWaferLoad.Text = "Wafer Load";
            this.btnWaferLoad.UseVisualStyleBackColor = true;
            this.btnWaferLoad.Click += new System.EventHandler(this.btnWaferLoad_Click);
            // 
            // grpProcess
            // 
            this.grpProcess.Controls.Add(this.btnProcStart);
            this.grpProcess.Controls.Add(this.btnProcEnd);
            this.grpProcess.Location = new System.Drawing.Point(429, 150);
            this.grpProcess.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.grpProcess.Name = "grpProcess";
            this.grpProcess.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.grpProcess.Size = new System.Drawing.Size(214, 180);
            this.grpProcess.TabIndex = 4;
            this.grpProcess.TabStop = false;
            this.grpProcess.Text = "Processing";
            // 
            // btnProcStart
            // 
            this.btnProcStart.Location = new System.Drawing.Point(14, 30);
            this.btnProcStart.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnProcStart.Name = "btnProcStart";
            this.btnProcStart.Size = new System.Drawing.Size(186, 45);
            this.btnProcStart.TabIndex = 0;
            this.btnProcStart.Text = "Process Start";
            this.btnProcStart.UseVisualStyleBackColor = true;
            this.btnProcStart.Click += new System.EventHandler(this.btnProcStart_Click);
            // 
            // btnProcEnd
            // 
            this.btnProcEnd.Location = new System.Drawing.Point(14, 90);
            this.btnProcEnd.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnProcEnd.Name = "btnProcEnd";
            this.btnProcEnd.Size = new System.Drawing.Size(186, 45);
            this.btnProcEnd.TabIndex = 1;
            this.btnProcEnd.Text = "Process End";
            this.btnProcEnd.UseVisualStyleBackColor = true;
            this.btnProcEnd.Click += new System.EventHandler(this.btnProcEnd_Click);
            // 
            // grpAlarm
            // 
            this.grpAlarm.Controls.Add(this.lblAid);
            this.grpAlarm.Controls.Add(this.txtAlarmId);
            this.grpAlarm.Controls.Add(this.lblAText);
            this.grpAlarm.Controls.Add(this.txtAlarmText);
            this.grpAlarm.Controls.Add(this.btnAlarmSet);
            this.grpAlarm.Controls.Add(this.btnAlarmClear);
            this.grpAlarm.Location = new System.Drawing.Point(657, 150);
            this.grpAlarm.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.grpAlarm.Name = "grpAlarm";
            this.grpAlarm.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.grpAlarm.Size = new System.Drawing.Size(357, 180);
            this.grpAlarm.TabIndex = 5;
            this.grpAlarm.TabStop = false;
            this.grpAlarm.Text = "Alarm";
            // 
            // lblAid
            // 
            this.lblAid.Location = new System.Drawing.Point(14, 38);
            this.lblAid.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblAid.Name = "lblAid";
            this.lblAid.Size = new System.Drawing.Size(43, 30);
            this.lblAid.TabIndex = 0;
            this.lblAid.Text = "ID:";
            // 
            // txtAlarmId
            // 
            this.txtAlarmId.Location = new System.Drawing.Point(57, 33);
            this.txtAlarmId.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtAlarmId.Name = "txtAlarmId";
            this.txtAlarmId.Size = new System.Drawing.Size(84, 28);
            this.txtAlarmId.TabIndex = 1;
            this.txtAlarmId.Text = "1001";
            // 
            // lblAText
            // 
            this.lblAText.Location = new System.Drawing.Point(157, 38);
            this.lblAText.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblAText.Name = "lblAText";
            this.lblAText.Size = new System.Drawing.Size(50, 30);
            this.lblAText.TabIndex = 2;
            this.lblAText.Text = "Msg:";
            // 
            // txtAlarmText
            // 
            this.txtAlarmText.Location = new System.Drawing.Point(207, 33);
            this.txtAlarmText.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtAlarmText.Name = "txtAlarmText";
            this.txtAlarmText.Size = new System.Drawing.Size(134, 28);
            this.txtAlarmText.TabIndex = 3;
            this.txtAlarmText.Text = "Error Occurred";
            // 
            // btnAlarmSet
            // 
            this.btnAlarmSet.Location = new System.Drawing.Point(14, 90);
            this.btnAlarmSet.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnAlarmSet.Name = "btnAlarmSet";
            this.btnAlarmSet.Size = new System.Drawing.Size(157, 45);
            this.btnAlarmSet.TabIndex = 4;
            this.btnAlarmSet.Text = "Set Alarm";
            this.btnAlarmSet.UseVisualStyleBackColor = true;
            this.btnAlarmSet.Click += new System.EventHandler(this.btnAlarmSet_Click);
            // 
            // btnAlarmClear
            // 
            this.btnAlarmClear.Location = new System.Drawing.Point(186, 90);
            this.btnAlarmClear.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnAlarmClear.Name = "btnAlarmClear";
            this.btnAlarmClear.Size = new System.Drawing.Size(157, 45);
            this.btnAlarmClear.TabIndex = 5;
            this.btnAlarmClear.Text = "Clear Alarm";
            this.btnAlarmClear.UseVisualStyleBackColor = true;
            this.btnAlarmClear.Click += new System.EventHandler(this.btnAlarmClear_Click);
            // 
            // FormGemClient
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1031, 717);
            this.Controls.Add(this.grpAlarm);
            this.Controls.Add(this.grpProcess);
            this.Controls.Add(this.grpTransfer);
            this.Controls.Add(this.grpRecipe);
            this.Controls.Add(this.grpState);
            this.Controls.Add(this.grpConnection);
            this.Controls.Add(this.txtLog);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "FormGemClient";
            this.Text = "GEM Client Simulator";
            this.grpConnection.ResumeLayout(false);
            this.grpState.ResumeLayout(false);
            this.grpRecipe.ResumeLayout(false);
            this.grpRecipe.PerformLayout();
            this.grpTransfer.ResumeLayout(false);
            this.grpTransfer.PerformLayout();
            this.grpProcess.ResumeLayout(false);
            this.grpAlarm.ResumeLayout(false);
            this.grpAlarm.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.GroupBox grpConnection;
        private System.Windows.Forms.GroupBox grpState;
        private System.Windows.Forms.Button btnControlState;
        private System.Windows.Forms.Button btnEqState;
        private System.Windows.Forms.GroupBox grpRecipe;
        private System.Windows.Forms.TextBox txtRecipe;
        private System.Windows.Forms.Button btnPPSelect;
        private System.Windows.Forms.Button btnReportTray;
        private System.Windows.Forms.GroupBox grpTransfer;
        private System.Windows.Forms.Label lblPort;
        private System.Windows.Forms.TextBox txtPortId;
        private System.Windows.Forms.Label lblLot;
        private System.Windows.Forms.TextBox txtLotId;
        private System.Windows.Forms.Button btnPortLoad;
        private System.Windows.Forms.Button btnPortUnload;
        private System.Windows.Forms.Button btnWaferLoad;
        private System.Windows.Forms.GroupBox grpProcess;
        private System.Windows.Forms.Button btnProcStart;
        private System.Windows.Forms.Button btnProcEnd;
        private System.Windows.Forms.GroupBox grpAlarm;
        private System.Windows.Forms.Label lblAid;
        private System.Windows.Forms.TextBox txtAlarmId;
        private System.Windows.Forms.Label lblAText;
        private System.Windows.Forms.TextBox txtAlarmText;
        private System.Windows.Forms.Button btnAlarmSet;
        private System.Windows.Forms.Button btnAlarmClear;
    }
}