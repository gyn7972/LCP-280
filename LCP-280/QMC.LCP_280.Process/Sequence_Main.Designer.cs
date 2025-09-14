using System;
using System.Drawing;
using System.Windows.Forms;

namespace QMC.LCP_280.Process
{
    /// <summary>
    /// Sequence_Main 폼 디자이너 (UI 전담)
    ///  - 컨트롤 생성 및 레이아웃만 포함
    ///  - 이벤트, 데이터/기능은 Sequence_Main.cs에서 처리
    /// </summary>
    public partial class Sequence_Main : Form
    {
        // ===== UI Controls =====
        private Button      btnStartAll;
        private Button      btnStopAll;
        private Button      btnSaveAllConfigs;
        private Button      btnLoadAllConfigs;
        private Button      btnSaveAllRecipes;
        private Button      btnLoadAllRecipes;

        private Label       lblEquipmentState;
        private ListView    lstUnitStatus;
        private RichTextBox rtbLog;

        private ComboBox    cmbUnits;
        private Button      btnStartUnit;
        private Button      btnStopUnit;

        private TableLayoutPanel mainPanel;

        /// <summary>
        /// 디자이너 초기화 (수동 작성)
        /// </summary>
        private void InitializeComponent()
        {
            this.mainPanel = new System.Windows.Forms.TableLayoutPanel();
            this.grpEquipment = new System.Windows.Forms.GroupBox();
            this.tlpEquip = new System.Windows.Forms.TableLayoutPanel();
            this.btnStartAll = new System.Windows.Forms.Button();
            this.btnStopAll = new System.Windows.Forms.Button();
            this.btnSaveAllConfigs = new System.Windows.Forms.Button();
            this.btnLoadAllConfigs = new System.Windows.Forms.Button();
            this.btnSaveAllRecipes = new System.Windows.Forms.Button();
            this.btnLoadAllRecipes = new System.Windows.Forms.Button();
            this.grpUnitCtrl = new System.Windows.Forms.GroupBox();
            this.tlpUnitCtrl = new System.Windows.Forms.TableLayoutPanel();
            this.lblUnit = new System.Windows.Forms.Label();
            this.cmbUnits = new System.Windows.Forms.ComboBox();
            this.btnStartUnit = new System.Windows.Forms.Button();
            this.btnStopUnit = new System.Windows.Forms.Button();
            this.grpStatus = new System.Windows.Forms.GroupBox();
            this.lblEquipmentInfo = new System.Windows.Forms.Label();
            this.lblEquipmentState = new System.Windows.Forms.Label();
            this.grpUnitList = new System.Windows.Forms.GroupBox();
            this.lstUnitStatus = new System.Windows.Forms.ListView();
            this.grpLog = new System.Windows.Forms.GroupBox();
            this.rtbLog = new System.Windows.Forms.RichTextBox();
            this.mainPanel.SuspendLayout();
            this.grpEquipment.SuspendLayout();
            this.tlpEquip.SuspendLayout();
            this.grpUnitCtrl.SuspendLayout();
            this.tlpUnitCtrl.SuspendLayout();
            this.grpStatus.SuspendLayout();
            this.grpUnitList.SuspendLayout();
            this.grpLog.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainPanel
            // 
            this.mainPanel.ColumnCount = 2;
            this.mainPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.mainPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.mainPanel.Controls.Add(this.grpEquipment, 0, 0);
            this.mainPanel.Controls.Add(this.grpUnitCtrl, 0, 1);
            this.mainPanel.Controls.Add(this.grpStatus, 0, 2);
            this.mainPanel.Controls.Add(this.grpUnitList, 1, 2);
            this.mainPanel.Controls.Add(this.grpLog, 0, 3);
            this.mainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainPanel.Location = new System.Drawing.Point(0, 0);
            this.mainPanel.Margin = new System.Windows.Forms.Padding(10);
            this.mainPanel.Name = "mainPanel";
            this.mainPanel.RowCount = 4;
            this.mainPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.mainPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 75F));
            this.mainPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.mainPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.mainPanel.Size = new System.Drawing.Size(1264, 751);
            this.mainPanel.TabIndex = 0;
            // 
            // grpEquipment
            // 
            this.mainPanel.SetColumnSpan(this.grpEquipment, 2);
            this.grpEquipment.Controls.Add(this.tlpEquip);
            this.grpEquipment.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpEquipment.Location = new System.Drawing.Point(3, 3);
            this.grpEquipment.Name = "grpEquipment";
            this.grpEquipment.Padding = new System.Windows.Forms.Padding(10);
            this.grpEquipment.Size = new System.Drawing.Size(1258, 74);
            this.grpEquipment.TabIndex = 0;
            this.grpEquipment.TabStop = false;
            this.grpEquipment.Text = "Equipment Control";
            // 
            // tlpEquip
            // 
            this.tlpEquip.ColumnCount = 6;
            this.tlpEquip.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.67F));
            this.tlpEquip.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.67F));
            this.tlpEquip.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.67F));
            this.tlpEquip.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.67F));
            this.tlpEquip.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.67F));
            this.tlpEquip.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.67F));
            this.tlpEquip.Controls.Add(this.btnStartAll, 0, 0);
            this.tlpEquip.Controls.Add(this.btnStopAll, 1, 0);
            this.tlpEquip.Controls.Add(this.btnSaveAllConfigs, 2, 0);
            this.tlpEquip.Controls.Add(this.btnLoadAllConfigs, 3, 0);
            this.tlpEquip.Controls.Add(this.btnSaveAllRecipes, 4, 0);
            this.tlpEquip.Controls.Add(this.btnLoadAllRecipes, 5, 0);
            this.tlpEquip.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpEquip.Location = new System.Drawing.Point(10, 24);
            this.tlpEquip.Name = "tlpEquip";
            this.tlpEquip.RowCount = 1;
            this.tlpEquip.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpEquip.Size = new System.Drawing.Size(1238, 40);
            this.tlpEquip.TabIndex = 0;
            // 
            // btnStartAll
            // 
            this.btnStartAll.BackColor = System.Drawing.Color.LightGreen;
            this.btnStartAll.Enabled = false;
            this.btnStartAll.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold);
            this.btnStartAll.Location = new System.Drawing.Point(3, 3);
            this.btnStartAll.Name = "btnStartAll";
            this.btnStartAll.Size = new System.Drawing.Size(200, 34);
            this.btnStartAll.TabIndex = 0;
            this.btnStartAll.Text = "Start All";
            this.btnStartAll.UseVisualStyleBackColor = false;
            this.btnStartAll.Click += new System.EventHandler(this.BtnStartAll_Click);
            // 
            // btnStopAll
            // 
            this.btnStopAll.BackColor = System.Drawing.Color.LightCoral;
            this.btnStopAll.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnStopAll.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold);
            this.btnStopAll.Location = new System.Drawing.Point(209, 3);
            this.btnStopAll.Name = "btnStopAll";
            this.btnStopAll.Size = new System.Drawing.Size(200, 34);
            this.btnStopAll.TabIndex = 1;
            this.btnStopAll.Text = "Stop All";
            this.btnStopAll.UseVisualStyleBackColor = false;
            // 
            // btnSaveAllConfigs
            // 
            this.btnSaveAllConfigs.BackColor = System.Drawing.Color.LightBlue;
            this.btnSaveAllConfigs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnSaveAllConfigs.Location = new System.Drawing.Point(415, 3);
            this.btnSaveAllConfigs.Name = "btnSaveAllConfigs";
            this.btnSaveAllConfigs.Size = new System.Drawing.Size(200, 34);
            this.btnSaveAllConfigs.TabIndex = 2;
            this.btnSaveAllConfigs.Text = "Save Configs";
            this.btnSaveAllConfigs.UseVisualStyleBackColor = false;
            // 
            // btnLoadAllConfigs
            // 
            this.btnLoadAllConfigs.BackColor = System.Drawing.Color.LightYellow;
            this.btnLoadAllConfigs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnLoadAllConfigs.Location = new System.Drawing.Point(621, 3);
            this.btnLoadAllConfigs.Name = "btnLoadAllConfigs";
            this.btnLoadAllConfigs.Size = new System.Drawing.Size(200, 34);
            this.btnLoadAllConfigs.TabIndex = 3;
            this.btnLoadAllConfigs.Text = "Load Configs";
            this.btnLoadAllConfigs.UseVisualStyleBackColor = false;
            // 
            // btnSaveAllRecipes
            // 
            this.btnSaveAllRecipes.BackColor = System.Drawing.Color.LightCyan;
            this.btnSaveAllRecipes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnSaveAllRecipes.Location = new System.Drawing.Point(827, 3);
            this.btnSaveAllRecipes.Name = "btnSaveAllRecipes";
            this.btnSaveAllRecipes.Size = new System.Drawing.Size(200, 34);
            this.btnSaveAllRecipes.TabIndex = 4;
            this.btnSaveAllRecipes.Text = "Save Recipes";
            this.btnSaveAllRecipes.UseVisualStyleBackColor = false;
            // 
            // btnLoadAllRecipes
            // 
            this.btnLoadAllRecipes.BackColor = System.Drawing.Color.LightPink;
            this.btnLoadAllRecipes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnLoadAllRecipes.Location = new System.Drawing.Point(1033, 3);
            this.btnLoadAllRecipes.Name = "btnLoadAllRecipes";
            this.btnLoadAllRecipes.Size = new System.Drawing.Size(202, 34);
            this.btnLoadAllRecipes.TabIndex = 5;
            this.btnLoadAllRecipes.Text = "Load Recipes";
            this.btnLoadAllRecipes.UseVisualStyleBackColor = false;
            // 
            // grpUnitCtrl
            // 
            this.mainPanel.SetColumnSpan(this.grpUnitCtrl, 2);
            this.grpUnitCtrl.Controls.Add(this.tlpUnitCtrl);
            this.grpUnitCtrl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpUnitCtrl.Location = new System.Drawing.Point(3, 83);
            this.grpUnitCtrl.Name = "grpUnitCtrl";
            this.grpUnitCtrl.Padding = new System.Windows.Forms.Padding(10);
            this.grpUnitCtrl.Size = new System.Drawing.Size(1258, 69);
            this.grpUnitCtrl.TabIndex = 1;
            this.grpUnitCtrl.TabStop = false;
            this.grpUnitCtrl.Text = "Individual Unit Control";
            // 
            // tlpUnitCtrl
            // 
            this.tlpUnitCtrl.ColumnCount = 4;
            this.tlpUnitCtrl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpUnitCtrl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 35F));
            this.tlpUnitCtrl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpUnitCtrl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpUnitCtrl.Controls.Add(this.lblUnit, 0, 0);
            this.tlpUnitCtrl.Controls.Add(this.cmbUnits, 1, 0);
            this.tlpUnitCtrl.Controls.Add(this.btnStartUnit, 2, 0);
            this.tlpUnitCtrl.Controls.Add(this.btnStopUnit, 3, 0);
            this.tlpUnitCtrl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpUnitCtrl.Location = new System.Drawing.Point(10, 24);
            this.tlpUnitCtrl.Name = "tlpUnitCtrl";
            this.tlpUnitCtrl.RowCount = 1;
            this.tlpUnitCtrl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpUnitCtrl.Size = new System.Drawing.Size(1238, 35);
            this.tlpUnitCtrl.TabIndex = 0;
            // 
            // lblUnit
            // 
            this.lblUnit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblUnit.Location = new System.Drawing.Point(3, 0);
            this.lblUnit.Name = "lblUnit";
            this.lblUnit.Size = new System.Drawing.Size(303, 35);
            this.lblUnit.TabIndex = 0;
            this.lblUnit.Text = "Select Unit:";
            this.lblUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cmbUnits
            // 
            this.cmbUnits.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmbUnits.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbUnits.Location = new System.Drawing.Point(312, 3);
            this.cmbUnits.Name = "cmbUnits";
            this.cmbUnits.Size = new System.Drawing.Size(427, 20);
            this.cmbUnits.TabIndex = 1;
            // 
            // btnStartUnit
            // 
            this.btnStartUnit.BackColor = System.Drawing.Color.PaleGreen;
            this.btnStartUnit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnStartUnit.Location = new System.Drawing.Point(745, 3);
            this.btnStartUnit.Name = "btnStartUnit";
            this.btnStartUnit.Size = new System.Drawing.Size(241, 29);
            this.btnStartUnit.TabIndex = 2;
            this.btnStartUnit.Text = "Start Unit";
            this.btnStartUnit.UseVisualStyleBackColor = false;
            // 
            // btnStopUnit
            // 
            this.btnStopUnit.BackColor = System.Drawing.Color.PaleVioletRed;
            this.btnStopUnit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnStopUnit.Location = new System.Drawing.Point(992, 3);
            this.btnStopUnit.Name = "btnStopUnit";
            this.btnStopUnit.Size = new System.Drawing.Size(243, 29);
            this.btnStopUnit.TabIndex = 3;
            this.btnStopUnit.Text = "Stop Unit";
            this.btnStopUnit.UseVisualStyleBackColor = false;
            // 
            // grpStatus
            // 
            this.grpStatus.Controls.Add(this.lblEquipmentInfo);
            this.grpStatus.Controls.Add(this.lblEquipmentState);
            this.grpStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpStatus.Location = new System.Drawing.Point(3, 158);
            this.grpStatus.Name = "grpStatus";
            this.grpStatus.Padding = new System.Windows.Forms.Padding(10);
            this.grpStatus.Size = new System.Drawing.Size(626, 351);
            this.grpStatus.TabIndex = 2;
            this.grpStatus.TabStop = false;
            this.grpStatus.Text = "Equipment Status";
            // 
            // lblEquipmentInfo
            // 
            this.lblEquipmentInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblEquipmentInfo.Font = new System.Drawing.Font("맑은 고딕", 9F);
            this.lblEquipmentInfo.Location = new System.Drawing.Point(10, 54);
            this.lblEquipmentInfo.Name = "lblEquipmentInfo";
            this.lblEquipmentInfo.Size = new System.Drawing.Size(606, 287);
            this.lblEquipmentInfo.TabIndex = 0;
            this.lblEquipmentInfo.Text = "Equipment: LCP-280\nManufacturer: QMC\nRegistered Units: 0";
            // 
            // lblEquipmentState
            // 
            this.lblEquipmentState.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblEquipmentState.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold);
            this.lblEquipmentState.Location = new System.Drawing.Point(10, 24);
            this.lblEquipmentState.Name = "lblEquipmentState";
            this.lblEquipmentState.Size = new System.Drawing.Size(606, 30);
            this.lblEquipmentState.TabIndex = 1;
            this.lblEquipmentState.Text = "State: Ready";
            this.lblEquipmentState.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // grpUnitList
            // 
            this.grpUnitList.Controls.Add(this.lstUnitStatus);
            this.grpUnitList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpUnitList.Location = new System.Drawing.Point(635, 158);
            this.grpUnitList.Name = "grpUnitList";
            this.grpUnitList.Padding = new System.Windows.Forms.Padding(10);
            this.grpUnitList.Size = new System.Drawing.Size(626, 351);
            this.grpUnitList.TabIndex = 3;
            this.grpUnitList.TabStop = false;
            this.grpUnitList.Text = "Unit Status";
            // 
            // lstUnitStatus
            // 
            this.lstUnitStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstUnitStatus.Font = new System.Drawing.Font("Consolas", 9F);
            this.lstUnitStatus.FullRowSelect = true;
            this.lstUnitStatus.GridLines = true;
            this.lstUnitStatus.HideSelection = false;
            this.lstUnitStatus.Location = new System.Drawing.Point(10, 24);
            this.lstUnitStatus.Name = "lstUnitStatus";
            this.lstUnitStatus.Size = new System.Drawing.Size(606, 317);
            this.lstUnitStatus.TabIndex = 0;
            this.lstUnitStatus.UseCompatibleStateImageBehavior = false;
            this.lstUnitStatus.View = System.Windows.Forms.View.Details;
            // 
            // grpLog
            // 
            this.mainPanel.SetColumnSpan(this.grpLog, 2);
            this.grpLog.Controls.Add(this.rtbLog);
            this.grpLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpLog.Location = new System.Drawing.Point(3, 515);
            this.grpLog.Name = "grpLog";
            this.grpLog.Padding = new System.Windows.Forms.Padding(10);
            this.grpLog.Size = new System.Drawing.Size(1258, 233);
            this.grpLog.TabIndex = 4;
            this.grpLog.TabStop = false;
            this.grpLog.Text = "Operation Log";
            // 
            // rtbLog
            // 
            this.rtbLog.BackColor = System.Drawing.Color.Black;
            this.rtbLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbLog.Font = new System.Drawing.Font("Consolas", 9F);
            this.rtbLog.ForeColor = System.Drawing.Color.LimeGreen;
            this.rtbLog.Location = new System.Drawing.Point(10, 24);
            this.rtbLog.Name = "rtbLog";
            this.rtbLog.ReadOnly = true;
            this.rtbLog.Size = new System.Drawing.Size(1238, 199);
            this.rtbLog.TabIndex = 0;
            this.rtbLog.Text = "";
            // 
            // Sequence_Main
            // 
            this.ClientSize = new System.Drawing.Size(1264, 751);
            this.Controls.Add(this.mainPanel);
            this.MinimumSize = new System.Drawing.Size(800, 600);
            this.Name = "Sequence_Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Equipment Control Panel - LCP-280";
            this.mainPanel.ResumeLayout(false);
            this.grpEquipment.ResumeLayout(false);
            this.tlpEquip.ResumeLayout(false);
            this.grpUnitCtrl.ResumeLayout(false);
            this.tlpUnitCtrl.ResumeLayout(false);
            this.grpStatus.ResumeLayout(false);
            this.grpUnitList.ResumeLayout(false);
            this.grpLog.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private GroupBox grpEquipment;
        private TableLayoutPanel tlpEquip;
        private GroupBox grpUnitCtrl;
        private TableLayoutPanel tlpUnitCtrl;
        private Label lblUnit;
        private GroupBox grpStatus;
        private Label lblEquipmentInfo;
        private GroupBox grpUnitList;
        private GroupBox grpLog;
    }
}
