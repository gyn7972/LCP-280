namespace QMC.Common
{
    partial class LanguageSetupForm
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
            this.grpScan = new System.Windows.Forms.GroupBox();
            this.btnScanEquipment = new System.Windows.Forms.Button();
            this.btnScanAlarms = new System.Windows.Forms.Button();
            this.btnScanAllForms = new System.Windows.Forms.Button();
            this.grpSave = new System.Windows.Forms.GroupBox();
            this.btnSaveKorean = new System.Windows.Forms.Button();
            this.btnSaveEnglish = new System.Windows.Forms.Button();
            this.grpLoad = new System.Windows.Forms.GroupBox();
            this.btnLoadKorean = new System.Windows.Forms.Button();
            this.btnLoadEnglish = new System.Windows.Forms.Button();
            this.grpCurrent = new System.Windows.Forms.GroupBox();
            this.lblCurrentLanguage = new System.Windows.Forms.Label();
            this.cboLanguage = new System.Windows.Forms.ComboBox();
            this.chkTopMost = new System.Windows.Forms.CheckBox();
            this.btnApplyCurrent = new System.Windows.Forms.Button();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.tvLang = new System.Windows.Forms.TreeView();
            this.btnRefreshTree = new System.Windows.Forms.Button();
            this.grpScan.SuspendLayout();
            this.grpSave.SuspendLayout();
            this.grpLoad.SuspendLayout();
            this.grpCurrent.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpScan
            // 
            this.grpScan.Controls.Add(this.btnScanEquipment);
            this.grpScan.Controls.Add(this.btnScanAlarms);
            this.grpScan.Controls.Add(this.btnScanAllForms);
            this.grpScan.Font = new System.Drawing.Font("¸¼Àº °íµñ", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.grpScan.Location = new System.Drawing.Point(14, 15);
            this.grpScan.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.grpScan.Name = "grpScan";
            this.grpScan.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.grpScan.Size = new System.Drawing.Size(703, 180);
            this.grpScan.TabIndex = 0;
            this.grpScan.TabStop = false;
            this.grpScan.Text = "1. Scan";
            // 
            // btnScanEquipment
            // 
            this.btnScanEquipment.Location = new System.Drawing.Point(14, 38);
            this.btnScanEquipment.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnScanEquipment.Name = "btnScanEquipment";
            this.btnScanEquipment.Size = new System.Drawing.Size(214, 52);
            this.btnScanEquipment.TabIndex = 0;
            this.btnScanEquipment.Text = "Scan Equipment";
            this.btnScanEquipment.UseVisualStyleBackColor = true;
            this.btnScanEquipment.Click += new System.EventHandler(this.btnScanEquipment_Click);
            // 
            // btnScanAlarms
            // 
            this.btnScanAlarms.Location = new System.Drawing.Point(243, 38);
            this.btnScanAlarms.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnScanAlarms.Name = "btnScanAlarms";
            this.btnScanAlarms.Size = new System.Drawing.Size(214, 52);
            this.btnScanAlarms.TabIndex = 1;
            this.btnScanAlarms.Text = "Scan Alarms";
            this.btnScanAlarms.UseVisualStyleBackColor = true;
            this.btnScanAlarms.Click += new System.EventHandler(this.btnScanAlarms_Click);
            // 
            // btnScanAllForms
            // 
            this.btnScanAllForms.Location = new System.Drawing.Point(471, 38);
            this.btnScanAllForms.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnScanAllForms.Name = "btnScanAllForms";
            this.btnScanAllForms.Size = new System.Drawing.Size(214, 52);
            this.btnScanAllForms.TabIndex = 2;
            this.btnScanAllForms.Text = "Scan All Forms";
            this.btnScanAllForms.UseVisualStyleBackColor = true;
            this.btnScanAllForms.Click += new System.EventHandler(this.btnScanAllForms_Click);
            // 
            // grpSave
            // 
            this.grpSave.Controls.Add(this.btnSaveKorean);
            this.grpSave.Controls.Add(this.btnSaveEnglish);
            this.grpSave.Font = new System.Drawing.Font("¸¼Àº °íµñ", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.grpSave.Location = new System.Drawing.Point(14, 210);
            this.grpSave.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.grpSave.Name = "grpSave";
            this.grpSave.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.grpSave.Size = new System.Drawing.Size(703, 120);
            this.grpSave.TabIndex = 1;
            this.grpSave.TabStop = false;
            this.grpSave.Text = "2. Save Language Files";
            // 
            // btnSaveKorean
            // 
            this.btnSaveKorean.Location = new System.Drawing.Point(14, 38);
            this.btnSaveKorean.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnSaveKorean.Name = "btnSaveKorean";
            this.btnSaveKorean.Size = new System.Drawing.Size(214, 60);
            this.btnSaveKorean.TabIndex = 0;
            this.btnSaveKorean.Text = "Save Korean";
            this.btnSaveKorean.UseVisualStyleBackColor = true;
            this.btnSaveKorean.Click += new System.EventHandler(this.btnSaveKorean_Click);
            // 
            // btnSaveEnglish
            // 
            this.btnSaveEnglish.Location = new System.Drawing.Point(243, 38);
            this.btnSaveEnglish.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnSaveEnglish.Name = "btnSaveEnglish";
            this.btnSaveEnglish.Size = new System.Drawing.Size(214, 60);
            this.btnSaveEnglish.TabIndex = 1;
            this.btnSaveEnglish.Text = "Save English";
            this.btnSaveEnglish.UseVisualStyleBackColor = true;
            this.btnSaveEnglish.Click += new System.EventHandler(this.btnSaveEnglish_Click);
            // 
            // grpLoad
            // 
            this.grpLoad.Controls.Add(this.btnLoadKorean);
            this.grpLoad.Controls.Add(this.btnLoadEnglish);
            this.grpLoad.Font = new System.Drawing.Font("¸¼Àº °íµñ", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.grpLoad.Location = new System.Drawing.Point(14, 345);
            this.grpLoad.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.grpLoad.Name = "grpLoad";
            this.grpLoad.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.grpLoad.Size = new System.Drawing.Size(703, 120);
            this.grpLoad.TabIndex = 2;
            this.grpLoad.TabStop = false;
            this.grpLoad.Text = "3. Load & Test";
            // 
            // btnLoadKorean
            // 
            this.btnLoadKorean.Location = new System.Drawing.Point(14, 38);
            this.btnLoadKorean.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnLoadKorean.Name = "btnLoadKorean";
            this.btnLoadKorean.Size = new System.Drawing.Size(214, 60);
            this.btnLoadKorean.TabIndex = 0;
            this.btnLoadKorean.Text = "Load Korean";
            this.btnLoadKorean.UseVisualStyleBackColor = true;
            this.btnLoadKorean.Click += new System.EventHandler(this.btnLoadKorean_Click);
            // 
            // btnLoadEnglish
            // 
            this.btnLoadEnglish.Location = new System.Drawing.Point(243, 38);
            this.btnLoadEnglish.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnLoadEnglish.Name = "btnLoadEnglish";
            this.btnLoadEnglish.Size = new System.Drawing.Size(214, 60);
            this.btnLoadEnglish.TabIndex = 1;
            this.btnLoadEnglish.Text = "Load English";
            this.btnLoadEnglish.UseVisualStyleBackColor = true;
            this.btnLoadEnglish.Click += new System.EventHandler(this.btnLoadEnglish_Click);
            // 
            // grpCurrent
            // 
            this.grpCurrent.Controls.Add(this.lblCurrentLanguage);
            this.grpCurrent.Controls.Add(this.cboLanguage);
            this.grpCurrent.Controls.Add(this.chkTopMost);
            this.grpCurrent.Controls.Add(this.btnApplyCurrent);
            this.grpCurrent.Font = new System.Drawing.Font("¸¼Àº °íµñ", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.grpCurrent.Location = new System.Drawing.Point(14, 480);
            this.grpCurrent.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.grpCurrent.Name = "grpCurrent";
            this.grpCurrent.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.grpCurrent.Size = new System.Drawing.Size(703, 135);
            this.grpCurrent.TabIndex = 3;
            this.grpCurrent.TabStop = false;
            this.grpCurrent.Text = "Current Language";
            // 
            // lblCurrentLanguage
            // 
            this.lblCurrentLanguage.AutoSize = true;
            this.lblCurrentLanguage.Location = new System.Drawing.Point(14, 38);
            this.lblCurrentLanguage.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblCurrentLanguage.Name = "lblCurrentLanguage";
            this.lblCurrentLanguage.Size = new System.Drawing.Size(212, 28);
            this.lblCurrentLanguage.TabIndex = 0;
            this.lblCurrentLanguage.Text = "Available Languages:";
            // 
            // cboLanguage
            // 
            this.cboLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboLanguage.FormattingEnabled = true;
            this.cboLanguage.Location = new System.Drawing.Point(230, 33);
            this.cboLanguage.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cboLanguage.Name = "cboLanguage";
            this.cboLanguage.Size = new System.Drawing.Size(284, 36);
            this.cboLanguage.TabIndex = 1;
            this.cboLanguage.SelectedIndexChanged += new System.EventHandler(this.cboLanguage_SelectedIndexChanged);
            // 
            // chkTopMost
            // 
            this.chkTopMost.AutoSize = true;
            this.chkTopMost.Location = new System.Drawing.Point(529, 36);
            this.chkTopMost.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.chkTopMost.Name = "chkTopMost";
            this.chkTopMost.Size = new System.Drawing.Size(123, 32);
            this.chkTopMost.TabIndex = 2;
            this.chkTopMost.Text = "TopMost";
            this.chkTopMost.UseVisualStyleBackColor = true;
            this.chkTopMost.CheckedChanged += new System.EventHandler(this.chkTopMost_CheckedChanged);
            // 
            // btnApplyCurrent
            // 
            this.btnApplyCurrent.Location = new System.Drawing.Point(14, 82);
            this.btnApplyCurrent.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnApplyCurrent.Name = "btnApplyCurrent";
            this.btnApplyCurrent.Size = new System.Drawing.Size(286, 42);
            this.btnApplyCurrent.TabIndex = 3;
            this.btnApplyCurrent.Text = "Apply to Open Forms";
            this.btnApplyCurrent.UseVisualStyleBackColor = true;
            this.btnApplyCurrent.Click += new System.EventHandler(this.btnApplyCurrent_Click);
            // 
            // txtLog
            // 
            this.txtLog.Font = new System.Drawing.Font("¸¼Àº °íµñ", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.txtLog.Location = new System.Drawing.Point(14, 630);
            this.txtLog.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(703, 343);
            this.txtLog.TabIndex = 4;
            // 
            // tvLang
            // 
            this.tvLang.Font = new System.Drawing.Font("¸¼Àº °íµñ", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.tvLang.Location = new System.Drawing.Point(725, 15);
            this.tvLang.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tvLang.Name = "tvLang";
            this.tvLang.ShowNodeToolTips = true;
            this.tvLang.Size = new System.Drawing.Size(713, 896);
            this.tvLang.TabIndex = 5;
            this.tvLang.DoubleClick += new System.EventHandler(this.tvLang_DoubleClick);
            // 
            // btnRefreshTree
            // 
            this.btnRefreshTree.Font = new System.Drawing.Font("¸¼Àº °íµñ", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnRefreshTree.Location = new System.Drawing.Point(725, 928);
            this.btnRefreshTree.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnRefreshTree.Name = "btnRefreshTree";
            this.btnRefreshTree.Size = new System.Drawing.Size(171, 45);
            this.btnRefreshTree.TabIndex = 6;
            this.btnRefreshTree.Text = "Refresh Tree";
            this.btnRefreshTree.UseVisualStyleBackColor = true;
            this.btnRefreshTree.Click += new System.EventHandler(this.btnRefreshTree_Click);
            // 
            // LanguageSetupForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1450, 1000);
            this.Controls.Add(this.btnRefreshTree);
            this.Controls.Add(this.tvLang);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.grpCurrent);
            this.Controls.Add(this.grpLoad);
            this.Controls.Add(this.grpSave);
            this.Controls.Add(this.grpScan);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "LanguageSetupForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Language Setup Utility";
            this.Load += new System.EventHandler(this.LanguageSetupForm_Load);
            this.grpScan.ResumeLayout(false);
            this.grpSave.ResumeLayout(false);
            this.grpLoad.ResumeLayout(false);
            this.grpCurrent.ResumeLayout(false);
            this.grpCurrent.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox grpScan;
        private System.Windows.Forms.Button btnScanEquipment;
        private System.Windows.Forms.Button btnScanAlarms;
        private System.Windows.Forms.Button btnScanAllForms;
        private System.Windows.Forms.GroupBox grpSave;
        private System.Windows.Forms.Button btnSaveKorean;
        private System.Windows.Forms.Button btnSaveEnglish;
        private System.Windows.Forms.GroupBox grpLoad;
        private System.Windows.Forms.Button btnLoadKorean;
        private System.Windows.Forms.Button btnLoadEnglish;
        private System.Windows.Forms.GroupBox grpCurrent;
        private System.Windows.Forms.Label lblCurrentLanguage;
        private System.Windows.Forms.ComboBox cboLanguage;
        private System.Windows.Forms.CheckBox chkTopMost;
        private System.Windows.Forms.Button btnApplyCurrent;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.TreeView tvLang;
        private System.Windows.Forms.Button btnRefreshTree;
    }
}