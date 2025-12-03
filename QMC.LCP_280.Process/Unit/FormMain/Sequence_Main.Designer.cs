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

        private Label       lblEquipmentState;
        private ListView    lstUnitStatus;
        private RichTextBox rtbLog;

        private ComboBox    cmbUnits;

        /// <summary>
        /// 디자이너 초기화 (수동 작성)
        /// </summary>
        private void InitializeComponent()
        {
            this.grpEquipment = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.btnLoadAllRecipes = new QMC.Common.IndividualMenuButton();
            this.btnSaveAllRecipes = new QMC.Common.IndividualMenuButton();
            this.btnLoadAllConfigs = new QMC.Common.IndividualMenuButton();
            this.btnSaveAllConfigs = new QMC.Common.IndividualMenuButton();
            this.btnStopAll = new QMC.Common.IndividualMenuButton();
            this.btnStartAll = new QMC.Common.IndividualMenuButton();
            this.grpUnitCtrl = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.btnStopUnit = new QMC.Common.IndividualMenuButton();
            this.cmbUnits = new System.Windows.Forms.ComboBox();
            this.btnStartUnit = new QMC.Common.IndividualMenuButton();
            this.lblUnit = new System.Windows.Forms.Label();
            this.grpStatus = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel6 = new System.Windows.Forms.TableLayoutPanel();
            this.lblEquipmentState = new System.Windows.Forms.Label();
            this.lblEquipmentInfo = new System.Windows.Forms.Label();
            this.grpUnitList = new System.Windows.Forms.GroupBox();
            this.lstUnitStatus = new System.Windows.Forms.ListView();
            this.grpLog = new System.Windows.Forms.GroupBox();
            this.rtbLog = new System.Windows.Forms.RichTextBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.grpEquipment.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.grpUnitCtrl.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.grpStatus.SuspendLayout();
            this.tableLayoutPanel6.SuspendLayout();
            this.grpUnitList.SuspendLayout();
            this.grpLog.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpEquipment
            // 
            this.grpEquipment.Controls.Add(this.tableLayoutPanel2);
            this.grpEquipment.Location = new System.Drawing.Point(3, 3);
            this.grpEquipment.Name = "grpEquipment";
            this.grpEquipment.Size = new System.Drawing.Size(748, 63);
            this.grpEquipment.TabIndex = 0;
            this.grpEquipment.TabStop = false;
            this.grpEquipment.Text = "Equipment Control";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 6;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.333334F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.333334F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.333334F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.333334F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.333334F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.333334F));
            this.tableLayoutPanel2.Controls.Add(this.btnLoadAllRecipes, 5, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnSaveAllRecipes, 4, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnLoadAllConfigs, 3, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnSaveAllConfigs, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnStopAll, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnStartAll, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 24);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(742, 36);
            this.tableLayoutPanel2.TabIndex = 6;
            // 
            // btnLoadAllRecipes
            // 
            this.btnLoadAllRecipes.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnLoadAllRecipes.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnLoadAllRecipes.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnLoadAllRecipes.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnLoadAllRecipes.CustomForeColor = System.Drawing.Color.Black;
            this.btnLoadAllRecipes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnLoadAllRecipes.Enabled = false;
            this.btnLoadAllRecipes.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnLoadAllRecipes.ForeColor = System.Drawing.Color.Black;
            this.btnLoadAllRecipes.ImageSize = new System.Drawing.Size(45, 45);
            this.btnLoadAllRecipes.Location = new System.Drawing.Point(618, 3);
            this.btnLoadAllRecipes.Name = "btnLoadAllRecipes";
            this.btnLoadAllRecipes.Size = new System.Drawing.Size(121, 30);
            this.btnLoadAllRecipes.TabIndex = 23;
            this.btnLoadAllRecipes.TabStop = false;
            this.btnLoadAllRecipes.Text = "Load Recipes";
            this.btnLoadAllRecipes.UseVisualStyleBackColor = false;
            // 
            // btnSaveAllRecipes
            // 
            this.btnSaveAllRecipes.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnSaveAllRecipes.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnSaveAllRecipes.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnSaveAllRecipes.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnSaveAllRecipes.CustomForeColor = System.Drawing.Color.Black;
            this.btnSaveAllRecipes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnSaveAllRecipes.Enabled = false;
            this.btnSaveAllRecipes.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnSaveAllRecipes.ForeColor = System.Drawing.Color.Black;
            this.btnSaveAllRecipes.ImageSize = new System.Drawing.Size(45, 45);
            this.btnSaveAllRecipes.Location = new System.Drawing.Point(495, 3);
            this.btnSaveAllRecipes.Name = "btnSaveAllRecipes";
            this.btnSaveAllRecipes.Size = new System.Drawing.Size(117, 30);
            this.btnSaveAllRecipes.TabIndex = 22;
            this.btnSaveAllRecipes.TabStop = false;
            this.btnSaveAllRecipes.Text = "Save Recipes";
            this.btnSaveAllRecipes.UseVisualStyleBackColor = false;
            // 
            // btnLoadAllConfigs
            // 
            this.btnLoadAllConfigs.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnLoadAllConfigs.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnLoadAllConfigs.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnLoadAllConfigs.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnLoadAllConfigs.CustomForeColor = System.Drawing.Color.Black;
            this.btnLoadAllConfigs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnLoadAllConfigs.Enabled = false;
            this.btnLoadAllConfigs.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnLoadAllConfigs.ForeColor = System.Drawing.Color.Black;
            this.btnLoadAllConfigs.ImageSize = new System.Drawing.Size(45, 45);
            this.btnLoadAllConfigs.Location = new System.Drawing.Point(372, 3);
            this.btnLoadAllConfigs.Name = "btnLoadAllConfigs";
            this.btnLoadAllConfigs.Size = new System.Drawing.Size(117, 30);
            this.btnLoadAllConfigs.TabIndex = 21;
            this.btnLoadAllConfigs.TabStop = false;
            this.btnLoadAllConfigs.Text = "Load Configs";
            this.btnLoadAllConfigs.UseVisualStyleBackColor = false;
            // 
            // btnSaveAllConfigs
            // 
            this.btnSaveAllConfigs.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnSaveAllConfigs.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnSaveAllConfigs.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnSaveAllConfigs.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnSaveAllConfigs.CustomForeColor = System.Drawing.Color.Black;
            this.btnSaveAllConfigs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnSaveAllConfigs.Enabled = false;
            this.btnSaveAllConfigs.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnSaveAllConfigs.ForeColor = System.Drawing.Color.Black;
            this.btnSaveAllConfigs.ImageSize = new System.Drawing.Size(45, 45);
            this.btnSaveAllConfigs.Location = new System.Drawing.Point(249, 3);
            this.btnSaveAllConfigs.Name = "btnSaveAllConfigs";
            this.btnSaveAllConfigs.Size = new System.Drawing.Size(117, 30);
            this.btnSaveAllConfigs.TabIndex = 20;
            this.btnSaveAllConfigs.TabStop = false;
            this.btnSaveAllConfigs.Text = "Save Configs";
            this.btnSaveAllConfigs.UseVisualStyleBackColor = false;
            // 
            // btnStopAll
            // 
            this.btnStopAll.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnStopAll.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnStopAll.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnStopAll.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnStopAll.CustomForeColor = System.Drawing.Color.Black;
            this.btnStopAll.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnStopAll.Enabled = false;
            this.btnStopAll.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnStopAll.ForeColor = System.Drawing.Color.Black;
            this.btnStopAll.ImageSize = new System.Drawing.Size(45, 45);
            this.btnStopAll.Location = new System.Drawing.Point(126, 3);
            this.btnStopAll.Name = "btnStopAll";
            this.btnStopAll.Size = new System.Drawing.Size(117, 30);
            this.btnStopAll.TabIndex = 19;
            this.btnStopAll.TabStop = false;
            this.btnStopAll.Text = "Stop All";
            this.btnStopAll.UseVisualStyleBackColor = false;
            // 
            // btnStartAll
            // 
            this.btnStartAll.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnStartAll.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnStartAll.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnStartAll.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnStartAll.CustomForeColor = System.Drawing.Color.Black;
            this.btnStartAll.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnStartAll.Enabled = false;
            this.btnStartAll.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnStartAll.ForeColor = System.Drawing.Color.Black;
            this.btnStartAll.ImageSize = new System.Drawing.Size(45, 45);
            this.btnStartAll.Location = new System.Drawing.Point(3, 3);
            this.btnStartAll.Name = "btnStartAll";
            this.btnStartAll.Size = new System.Drawing.Size(117, 30);
            this.btnStartAll.TabIndex = 18;
            this.btnStartAll.TabStop = false;
            this.btnStartAll.Text = "Start All";
            this.btnStartAll.UseVisualStyleBackColor = false;
            // 
            // grpUnitCtrl
            // 
            this.grpUnitCtrl.Controls.Add(this.tableLayoutPanel3);
            this.grpUnitCtrl.Location = new System.Drawing.Point(757, 3);
            this.grpUnitCtrl.Name = "grpUnitCtrl";
            this.grpUnitCtrl.Size = new System.Drawing.Size(498, 63);
            this.grpUnitCtrl.TabIndex = 1;
            this.grpUnitCtrl.TabStop = false;
            this.grpUnitCtrl.Text = "Individual Unit Control";
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 4;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.333333F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.333333F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.333333F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.333333F));
            this.tableLayoutPanel3.Controls.Add(this.btnStopUnit, 3, 0);
            this.tableLayoutPanel3.Controls.Add(this.cmbUnits, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.btnStartUnit, 2, 0);
            this.tableLayoutPanel3.Controls.Add(this.lblUnit, 0, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 24);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(492, 36);
            this.tableLayoutPanel3.TabIndex = 6;
            // 
            // btnStopUnit
            // 
            this.btnStopUnit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnStopUnit.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnStopUnit.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnStopUnit.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnStopUnit.CustomForeColor = System.Drawing.Color.Black;
            this.btnStopUnit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnStopUnit.Enabled = false;
            this.btnStopUnit.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnStopUnit.ForeColor = System.Drawing.Color.Black;
            this.btnStopUnit.ImageSize = new System.Drawing.Size(45, 45);
            this.btnStopUnit.Location = new System.Drawing.Point(372, 3);
            this.btnStopUnit.Name = "btnStopUnit";
            this.btnStopUnit.Size = new System.Drawing.Size(117, 30);
            this.btnStopUnit.TabIndex = 20;
            this.btnStopUnit.TabStop = false;
            this.btnStopUnit.Text = "Stop Unit";
            this.btnStopUnit.UseVisualStyleBackColor = false;
            // 
            // cmbUnits
            // 
            this.cmbUnits.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbUnits.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbUnits.Enabled = false;
            this.cmbUnits.Location = new System.Drawing.Point(126, 3);
            this.cmbUnits.Name = "cmbUnits";
            this.cmbUnits.Size = new System.Drawing.Size(117, 26);
            this.cmbUnits.TabIndex = 1;
            // 
            // btnStartUnit
            // 
            this.btnStartUnit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnStartUnit.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnStartUnit.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnStartUnit.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnStartUnit.CustomForeColor = System.Drawing.Color.Black;
            this.btnStartUnit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnStartUnit.Enabled = false;
            this.btnStartUnit.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnStartUnit.ForeColor = System.Drawing.Color.Black;
            this.btnStartUnit.ImageSize = new System.Drawing.Size(45, 45);
            this.btnStartUnit.Location = new System.Drawing.Point(249, 3);
            this.btnStartUnit.Name = "btnStartUnit";
            this.btnStartUnit.Size = new System.Drawing.Size(117, 30);
            this.btnStartUnit.TabIndex = 19;
            this.btnStartUnit.TabStop = false;
            this.btnStartUnit.Text = "Start Unit";
            this.btnStartUnit.UseVisualStyleBackColor = false;
            // 
            // lblUnit
            // 
            this.lblUnit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblUnit.Location = new System.Drawing.Point(3, 3);
            this.lblUnit.Margin = new System.Windows.Forms.Padding(3);
            this.lblUnit.Name = "lblUnit";
            this.lblUnit.Size = new System.Drawing.Size(117, 30);
            this.lblUnit.TabIndex = 0;
            this.lblUnit.Text = "Select Unit:";
            this.lblUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // grpStatus
            // 
            this.grpStatus.Controls.Add(this.tableLayoutPanel6);
            this.grpStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpStatus.Location = new System.Drawing.Point(3, 3);
            this.grpStatus.Name = "grpStatus";
            this.grpStatus.Padding = new System.Windows.Forms.Padding(10);
            this.grpStatus.Size = new System.Drawing.Size(748, 438);
            this.grpStatus.TabIndex = 2;
            this.grpStatus.TabStop = false;
            this.grpStatus.Text = "Equipment Status";
            // 
            // tableLayoutPanel6
            // 
            this.tableLayoutPanel6.ColumnCount = 1;
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel6.Controls.Add(this.lblEquipmentState, 0, 0);
            this.tableLayoutPanel6.Controls.Add(this.lblEquipmentInfo, 0, 1);
            this.tableLayoutPanel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel6.Location = new System.Drawing.Point(10, 31);
            this.tableLayoutPanel6.Name = "tableLayoutPanel6";
            this.tableLayoutPanel6.RowCount = 2;
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 80F));
            this.tableLayoutPanel6.Size = new System.Drawing.Size(728, 397);
            this.tableLayoutPanel6.TabIndex = 2;
            // 
            // lblEquipmentState
            // 
            this.lblEquipmentState.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblEquipmentState.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold);
            this.lblEquipmentState.Location = new System.Drawing.Point(3, 3);
            this.lblEquipmentState.Margin = new System.Windows.Forms.Padding(3);
            this.lblEquipmentState.Name = "lblEquipmentState";
            this.lblEquipmentState.Size = new System.Drawing.Size(722, 73);
            this.lblEquipmentState.TabIndex = 1;
            this.lblEquipmentState.Text = "State: Ready";
            this.lblEquipmentState.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblEquipmentInfo
            // 
            this.lblEquipmentInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblEquipmentInfo.Font = new System.Drawing.Font("맑은 고딕", 9F);
            this.lblEquipmentInfo.Location = new System.Drawing.Point(3, 82);
            this.lblEquipmentInfo.Margin = new System.Windows.Forms.Padding(3);
            this.lblEquipmentInfo.Name = "lblEquipmentInfo";
            this.lblEquipmentInfo.Size = new System.Drawing.Size(722, 312);
            this.lblEquipmentInfo.TabIndex = 0;
            this.lblEquipmentInfo.Text = "Equipment: LCP-280\nManufacturer: QMC\nRegistered Units: 0";
            // 
            // grpUnitList
            // 
            this.grpUnitList.Controls.Add(this.lstUnitStatus);
            this.grpUnitList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpUnitList.Location = new System.Drawing.Point(757, 3);
            this.grpUnitList.Name = "grpUnitList";
            this.grpUnitList.Padding = new System.Windows.Forms.Padding(10);
            this.grpUnitList.Size = new System.Drawing.Size(498, 438);
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
            this.lstUnitStatus.Location = new System.Drawing.Point(10, 31);
            this.lstUnitStatus.Name = "lstUnitStatus";
            this.lstUnitStatus.Size = new System.Drawing.Size(478, 397);
            this.lstUnitStatus.TabIndex = 0;
            this.lstUnitStatus.UseCompatibleStateImageBehavior = false;
            this.lstUnitStatus.View = System.Windows.Forms.View.Details;
            // 
            // grpLog
            // 
            this.grpLog.Controls.Add(this.rtbLog);
            this.grpLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpLog.Location = new System.Drawing.Point(3, 528);
            this.grpLog.Name = "grpLog";
            this.grpLog.Padding = new System.Windows.Forms.Padding(10);
            this.grpLog.Size = new System.Drawing.Size(1258, 220);
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
            this.rtbLog.Location = new System.Drawing.Point(10, 31);
            this.rtbLog.Name = "rtbLog";
            this.rtbLog.ReadOnly = true;
            this.rtbLog.Size = new System.Drawing.Size(1238, 179);
            this.rtbLog.TabIndex = 0;
            this.rtbLog.Text = "";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel5, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel4, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.grpLog, 0, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1264, 751);
            this.tableLayoutPanel1.TabIndex = 5;
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 2;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutPanel5.Controls.Add(this.grpEquipment, 0, 0);
            this.tableLayoutPanel5.Controls.Add(this.grpUnitCtrl, 1, 0);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 1;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(1258, 69);
            this.tableLayoutPanel5.TabIndex = 2;
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 2;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutPanel4.Controls.Add(this.grpStatus, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.grpUnitList, 1, 0);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(3, 78);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 1;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(1258, 444);
            this.tableLayoutPanel4.TabIndex = 2;
            // 
            // Sequence_Main
            // 
            this.ClientSize = new System.Drawing.Size(1264, 751);
            this.Controls.Add(this.tableLayoutPanel1);
            this.MinimumSize = new System.Drawing.Size(800, 600);
            this.Name = "Sequence_Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Equipment Control Panel - LCP-280";
            this.Load += new System.EventHandler(this.Sequence_Main_Load);
            this.grpEquipment.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.grpUnitCtrl.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.grpStatus.ResumeLayout(false);
            this.tableLayoutPanel6.ResumeLayout(false);
            this.grpUnitList.ResumeLayout(false);
            this.grpLog.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel5.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private GroupBox grpEquipment;
        private GroupBox grpUnitCtrl;
        private Label lblUnit;
        private GroupBox grpStatus;
        private Label lblEquipmentInfo;
        private GroupBox grpUnitList;
        private GroupBox grpLog;
        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel4;
        private TableLayoutPanel tableLayoutPanel2;
        private Common.IndividualMenuButton btnLoadAllRecipes;
        private Common.IndividualMenuButton btnSaveAllRecipes;
        private Common.IndividualMenuButton btnLoadAllConfigs;
        private Common.IndividualMenuButton btnSaveAllConfigs;
        private Common.IndividualMenuButton btnStopAll;
        private Common.IndividualMenuButton btnStartAll;
        private TableLayoutPanel tableLayoutPanel3;
        private Common.IndividualMenuButton btnStartUnit;
        private Common.IndividualMenuButton btnStopUnit;
        private TableLayoutPanel tableLayoutPanel5;
        private TableLayoutPanel tableLayoutPanel6;
    }
}
