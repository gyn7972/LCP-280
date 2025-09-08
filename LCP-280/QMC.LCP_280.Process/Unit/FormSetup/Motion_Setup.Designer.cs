using QMC.Common;
using QMC.Common.CustomControl;
using QMC.Common.Motion.Ajin;
using QMC.Common.Motions;
using QMC.LCP_280.Process;
using QMC.LCP_280.Process.Unit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Unit
{
    /// <summary>
    /// Motion_Setup (refactored: minimal-change, safer wiring, clearer structure)
    /// </summary>
    partial class Motion_Setup
    {
        // -------- Fields
        private readonly Equipment equipment = Equipment.Instance;
        private MotionAxisManager _axisManager;
        private MotionAxis _axis;

        // UI
        private ListBoxItemsView selectAxisListBoxItemsView;
        private IOPropertyCollectionView motorStateIoPropertyCollectionView;
        private IOPropertyCollectionView motorIoPropertyCollectionView;
        private PropertyCollectionView positionVelocityPropertyCollectionView;
        private PropertyCollectionView speedListBoxItemsView;
        private PropertyCollectionView configurationListBoxItemsView;

        private GroupBox gbAxisProperty;
        private GroupBox gbAxisPositions;
        private IndividualMenuButton btn_Save_Setup_Motion_Configuration;

        private System.ComponentModel.IContainer components = null;

        // Data
        private PropertyCollection _editorProrertiesPosition;
        private PropertyCollection _editorPropertiesConfig;
        private PropertyCollection _editorPropertiesSpeed;

        // Timers
        private Timer _axisPosTimer;

        // -------- Dispose
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_axisPosTimer != null)
                {
                    _axisPosTimer.Tick -= AxisPosTimer_Tick;
                    _axisPosTimer.Dispose();
                    _axisPosTimer = null;
                }

                if (components != null)
                    components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Designer (trimmed & corrected)
        private void InitializeComponent()
        {
            this.gbAxisProperty = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btn_Save_Setup_Motion_Speed = new QMC.Common.IndividualMenuButton();
            this.speedListBoxItemsView = new QMC.Common.PropertyCollectionView();
            this.configurationListBoxItemsView = new QMC.Common.PropertyCollectionView();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btn_Save_Setup_Motion_Configuration = new QMC.Common.IndividualMenuButton();
            this.gbAxisPositions = new System.Windows.Forms.GroupBox();
            this.btnHome1 = new QMC.Common.IndividualMenuButton();
            this.btnServoOn = new QMC.Common.IndividualMenuButton();
            this.btnServoOff = new QMC.Common.IndividualMenuButton();
            this.btnHome = new QMC.Common.IndividualMenuButton();
            this.motorStateIoPropertyCollectionView = new QMC.Common.IOPropertyCollectionView();
            this.motorIoPropertyCollectionView = new QMC.Common.IOPropertyCollectionView();
            this.positionVelocityPropertyCollectionView = new QMC.Common.PropertyCollectionView();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.selectAxisListBoxItemsView = new QMC.Common.ListBoxItemsView();
            this.gbAxisProperty.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.gbAxisPositions.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbAxisProperty
            // 
            this.gbAxisProperty.BackColor = System.Drawing.Color.White;
            this.gbAxisProperty.Controls.Add(this.tableLayoutPanel2);
            this.gbAxisProperty.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbAxisProperty.Font = new System.Drawing.Font("맑은 고딕", 10F);
            this.gbAxisProperty.Location = new System.Drawing.Point(584, 3);
            this.gbAxisProperty.Name = "gbAxisProperty";
            this.gbAxisProperty.Size = new System.Drawing.Size(677, 746);
            this.gbAxisProperty.TabIndex = 10;
            this.gbAxisProperty.TabStop = false;
            this.gbAxisProperty.Text = "Axis Property";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.panel2, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.speedListBoxItemsView, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.configurationListBoxItemsView, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.panel1, 0, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 21);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 57F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(671, 722);
            this.tableLayoutPanel2.TabIndex = 6;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.btn_Save_Setup_Motion_Speed);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(338, 668);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(330, 51);
            this.panel2.TabIndex = 3;
            // 
            // btn_Save_Setup_Motion_Speed
            // 
            this.btn_Save_Setup_Motion_Speed.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Save_Setup_Motion_Speed.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Save_Setup_Motion_Speed.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Save_Setup_Motion_Speed.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Save_Setup_Motion_Speed.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Save_Setup_Motion_Speed.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Save_Setup_Motion_Speed.ForeColor = System.Drawing.Color.Black;
            this.btn_Save_Setup_Motion_Speed.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Save_Setup_Motion_Speed.Location = new System.Drawing.Point(224, 6);
            this.btn_Save_Setup_Motion_Speed.Name = "btn_Save_Setup_Motion_Speed";
            this.btn_Save_Setup_Motion_Speed.Size = new System.Drawing.Size(100, 40);
            this.btn_Save_Setup_Motion_Speed.TabIndex = 5;
            this.btn_Save_Setup_Motion_Speed.TabStop = false;
            this.btn_Save_Setup_Motion_Speed.Text = "Save";
            this.btn_Save_Setup_Motion_Speed.UseVisualStyleBackColor = false;
            this.btn_Save_Setup_Motion_Speed.Click += new System.EventHandler(this.btn_Save_Setup_Motion_Speed_Click);
            // 
            // speedListBoxItemsView
            // 
            this.speedListBoxItemsView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.speedListBoxItemsView.GroupName = "Speed";
            this.speedListBoxItemsView.Location = new System.Drawing.Point(338, 4);
            this.speedListBoxItemsView.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.speedListBoxItemsView.Name = "speedListBoxItemsView";
            this.speedListBoxItemsView.Size = new System.Drawing.Size(330, 657);
            this.speedListBoxItemsView.TabIndex = 1;
            // 
            // configurationListBoxItemsView
            // 
            this.configurationListBoxItemsView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.configurationListBoxItemsView.GroupName = "Configuration";
            this.configurationListBoxItemsView.Location = new System.Drawing.Point(3, 4);
            this.configurationListBoxItemsView.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.configurationListBoxItemsView.Name = "configurationListBoxItemsView";
            this.configurationListBoxItemsView.Size = new System.Drawing.Size(329, 657);
            this.configurationListBoxItemsView.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btn_Save_Setup_Motion_Configuration);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 668);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(329, 51);
            this.panel1.TabIndex = 2;
            // 
            // btn_Save_Setup_Motion_Configuration
            // 
            this.btn_Save_Setup_Motion_Configuration.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Save_Setup_Motion_Configuration.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Save_Setup_Motion_Configuration.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Save_Setup_Motion_Configuration.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Save_Setup_Motion_Configuration.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Save_Setup_Motion_Configuration.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Save_Setup_Motion_Configuration.ForeColor = System.Drawing.Color.Black;
            this.btn_Save_Setup_Motion_Configuration.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Save_Setup_Motion_Configuration.Location = new System.Drawing.Point(224, 6);
            this.btn_Save_Setup_Motion_Configuration.Name = "btn_Save_Setup_Motion_Configuration";
            this.btn_Save_Setup_Motion_Configuration.Size = new System.Drawing.Size(100, 40);
            this.btn_Save_Setup_Motion_Configuration.TabIndex = 4;
            this.btn_Save_Setup_Motion_Configuration.TabStop = false;
            this.btn_Save_Setup_Motion_Configuration.Text = "Save";
            this.btn_Save_Setup_Motion_Configuration.UseVisualStyleBackColor = false;
            this.btn_Save_Setup_Motion_Configuration.Click += new System.EventHandler(this.btn_Save_Setup_Motion_Configuration_Click);
            // 
            // gbAxisPositions
            // 
            this.gbAxisPositions.BackColor = System.Drawing.Color.White;
            this.gbAxisPositions.Controls.Add(this.btnHome1);
            this.gbAxisPositions.Controls.Add(this.btnServoOn);
            this.gbAxisPositions.Controls.Add(this.btnServoOff);
            this.gbAxisPositions.Controls.Add(this.btnHome);
            this.gbAxisPositions.Controls.Add(this.motorStateIoPropertyCollectionView);
            this.gbAxisPositions.Controls.Add(this.motorIoPropertyCollectionView);
            this.gbAxisPositions.Controls.Add(this.positionVelocityPropertyCollectionView);
            this.gbAxisPositions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbAxisPositions.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.gbAxisPositions.Location = new System.Drawing.Point(271, 3);
            this.gbAxisPositions.Name = "gbAxisPositions";
            this.gbAxisPositions.Size = new System.Drawing.Size(307, 746);
            this.gbAxisPositions.TabIndex = 13;
            this.gbAxisPositions.TabStop = false;
            this.gbAxisPositions.Text = "Axis Status";
            // 
            // btnHome1
            // 
            this.btnHome1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnHome1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnHome1.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnHome1.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnHome1.CustomForeColor = System.Drawing.Color.Black;
            this.btnHome1.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnHome1.ForeColor = System.Drawing.Color.Black;
            this.btnHome1.ImageSize = new System.Drawing.Size(45, 45);
            this.btnHome1.Location = new System.Drawing.Point(6, 716);
            this.btnHome1.Name = "btnHome1";
            this.btnHome1.Size = new System.Drawing.Size(93, 24);
            this.btnHome1.TabIndex = 8;
            this.btnHome1.TabStop = false;
            this.btnHome1.Text = "Home1";
            this.btnHome1.UseVisualStyleBackColor = false;
            this.btnHome1.Click += new System.EventHandler(this.btnHome1_Click);
            // 
            // btnServoOn
            // 
            this.btnServoOn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnServoOn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnServoOn.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnServoOn.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnServoOn.CustomForeColor = System.Drawing.Color.Black;
            this.btnServoOn.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnServoOn.ForeColor = System.Drawing.Color.Black;
            this.btnServoOn.ImageSize = new System.Drawing.Size(45, 45);
            this.btnServoOn.Location = new System.Drawing.Point(6, 677);
            this.btnServoOn.Name = "btnServoOn";
            this.btnServoOn.Size = new System.Drawing.Size(93, 33);
            this.btnServoOn.TabIndex = 7;
            this.btnServoOn.TabStop = false;
            this.btnServoOn.Text = "Servo On";
            this.btnServoOn.UseVisualStyleBackColor = false;
            this.btnServoOn.Click += new System.EventHandler(this.btnServoOn_Click);
            // 
            // btnServoOff
            // 
            this.btnServoOff.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnServoOff.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnServoOff.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnServoOff.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnServoOff.CustomForeColor = System.Drawing.Color.Black;
            this.btnServoOff.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnServoOff.ForeColor = System.Drawing.Color.Black;
            this.btnServoOff.ImageSize = new System.Drawing.Size(45, 45);
            this.btnServoOff.Location = new System.Drawing.Point(102, 677);
            this.btnServoOff.Name = "btnServoOff";
            this.btnServoOff.Size = new System.Drawing.Size(93, 33);
            this.btnServoOff.TabIndex = 6;
            this.btnServoOff.TabStop = false;
            this.btnServoOff.Text = "Servo Off";
            this.btnServoOff.UseVisualStyleBackColor = false;
            this.btnServoOff.Click += new System.EventHandler(this.btnServoOff_Click);
            // 
            // btnHome
            // 
            this.btnHome.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnHome.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnHome.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnHome.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnHome.CustomForeColor = System.Drawing.Color.Black;
            this.btnHome.Enabled = false;
            this.btnHome.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnHome.ForeColor = System.Drawing.Color.Black;
            this.btnHome.ImageSize = new System.Drawing.Size(45, 45);
            this.btnHome.Location = new System.Drawing.Point(206, 677);
            this.btnHome.Name = "btnHome";
            this.btnHome.Size = new System.Drawing.Size(93, 33);
            this.btnHome.TabIndex = 5;
            this.btnHome.TabStop = false;
            this.btnHome.Text = "Home Test";
            this.btnHome.UseVisualStyleBackColor = false;
            this.btnHome.Click += new System.EventHandler(this.btnHome_Click);
            // 
            // motorStateIoPropertyCollectionView
            // 
            this.motorStateIoPropertyCollectionView.GroupName = "Motor State";
            this.motorStateIoPropertyCollectionView.ListBackColor = System.Drawing.Color.Black;
            this.motorStateIoPropertyCollectionView.ListForeColor = System.Drawing.Color.Lime;
            this.motorStateIoPropertyCollectionView.Location = new System.Drawing.Point(6, 448);
            this.motorStateIoPropertyCollectionView.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.motorStateIoPropertyCollectionView.Name = "motorStateIoPropertyCollectionView";
            this.motorStateIoPropertyCollectionView.SelectedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(255)))), ((int)(((byte)(0)))));
            this.motorStateIoPropertyCollectionView.SelectedForeColor = System.Drawing.Color.Black;
            this.motorStateIoPropertyCollectionView.Size = new System.Drawing.Size(293, 220);
            this.motorStateIoPropertyCollectionView.TabIndex = 2;
            // 
            // motorIoPropertyCollectionView
            // 
            this.motorIoPropertyCollectionView.GroupName = "Motor I/O";
            this.motorIoPropertyCollectionView.ListBackColor = System.Drawing.Color.Black;
            this.motorIoPropertyCollectionView.ListForeColor = System.Drawing.Color.Lime;
            this.motorIoPropertyCollectionView.Location = new System.Drawing.Point(6, 246);
            this.motorIoPropertyCollectionView.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.motorIoPropertyCollectionView.Name = "motorIoPropertyCollectionView";
            this.motorIoPropertyCollectionView.SelectedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(255)))), ((int)(((byte)(0)))));
            this.motorIoPropertyCollectionView.SelectedForeColor = System.Drawing.Color.Black;
            this.motorIoPropertyCollectionView.Size = new System.Drawing.Size(293, 198);
            this.motorIoPropertyCollectionView.TabIndex = 1;
            // 
            // positionVelocityPropertyCollectionView
            // 
            this.positionVelocityPropertyCollectionView.GroupName = "Position & Velocity";
            this.positionVelocityPropertyCollectionView.Location = new System.Drawing.Point(6, 25);
            this.positionVelocityPropertyCollectionView.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.positionVelocityPropertyCollectionView.Name = "positionVelocityPropertyCollectionView";
            this.positionVelocityPropertyCollectionView.Size = new System.Drawing.Size(293, 220);
            this.positionVelocityPropertyCollectionView.TabIndex = 0;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 21.28164F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 24.76266F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 53.9557F));
            this.tableLayoutPanel1.Controls.Add(this.selectAxisListBoxItemsView, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.gbAxisProperty, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.gbAxisPositions, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1264, 752);
            this.tableLayoutPanel1.TabIndex = 14;
            // 
            // selectAxisListBoxItemsView
            // 
            this.selectAxisListBoxItemsView.BorderColor = System.Drawing.Color.White;
            this.selectAxisListBoxItemsView.BorderWidth = 2;
            this.selectAxisListBoxItemsView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.selectAxisListBoxItemsView.GroupBackColor = System.Drawing.Color.White;
            this.selectAxisListBoxItemsView.GroupForeColor = System.Drawing.Color.Black;
            this.selectAxisListBoxItemsView.GroupName = "Select Axis";
            this.selectAxisListBoxItemsView.ItemBackColor = System.Drawing.Color.Black;
            this.selectAxisListBoxItemsView.ItemForeColor = System.Drawing.Color.Lime;
            this.selectAxisListBoxItemsView.ListBackColor = System.Drawing.Color.Black;
            this.selectAxisListBoxItemsView.ListForeColor = System.Drawing.Color.Lime;
            this.selectAxisListBoxItemsView.Location = new System.Drawing.Point(3, 3);
            this.selectAxisListBoxItemsView.Name = "selectAxisListBoxItemsView";
            this.selectAxisListBoxItemsView.SelectedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(255)))), ((int)(((byte)(0)))));
            this.selectAxisListBoxItemsView.SelectedForeColor = System.Drawing.Color.Black;
            this.selectAxisListBoxItemsView.SelectedIndex = -1;
            this.selectAxisListBoxItemsView.Size = new System.Drawing.Size(262, 746);
            this.selectAxisListBoxItemsView.TabIndex = 2;
            // 
            // Motion_Setup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1264, 752);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Motion_Setup";
            this.Text = "Motion Setup";
            this.gbAxisProperty.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.gbAxisPositions.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        // -------- Public/Init
        private void InitializeUI()
        {
            try
            {
                WireAxisSelectionEvent();
                BindAxisList();
                InitializeStatusTimer();     // 실제 위치 주기 갱신 (필요 시)
                InitializeRadioButtonView();
            }
            catch (Exception ex)
            {
                Log.Write("LCP-280", $"InitializeUI error: {ex}");
            }
        }

        // -------- Binding
        /// <summary>
        /// Axis 목록 바인딩 (UNIT_NAME 기준)
        /// </summary>
        private void BindAxisList()
        {
            try
            {
                if (_axisManager == null)
                {
                    MessageBox.Show("AxisManager가 초기화되지 않았습니다.", "알림",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    selectAxisListBoxItemsView?.SetItems();
                    return;
                }

                var axisNames = _axisManager.GetAxisNames(UNIT_NAME) ?? Array.Empty<string>();
                if (axisNames.Length > 0)
                    selectAxisListBoxItemsView?.SetItems(axisNames);
                else
                {
                    Log.Write("LCP-280", "PropertyPosition에 Position 항목이 없습니다.");
                    selectAxisListBoxItemsView?.SetItems();
                }
            }
            catch (Exception ex)
            {
                Log.Write("LCP-280", $"BindAxisList error: {ex}");
                selectAxisListBoxItemsView?.SetItems();
            }
        }

        // -------- Events
        private void WireAxisSelectionEvent()
        {
            if (selectAxisListBoxItemsView == null) return;

            // 중복 구독 방지
            selectAxisListBoxItemsView.ItemSelected -= OnPositionItemSelected;
            selectAxisListBoxItemsView.ItemSelected += OnPositionItemSelected;

            motorIoPropertyCollectionView.ItemClicked -= OnMotorIOItemClicked;
            motorIoPropertyCollectionView.ItemClicked += OnMotorIOItemClicked;
        }

        /// <summary>
        /// Select Axis 리스트에서 항목 선택 시 속성 에디터 구성
        /// </summary>
        private void OnPositionItemSelected(object sender, int selectedIndex)
        {
            try
            {
                if (selectedIndex < 0)
                {
                    _axis = null;
                    configurationListBoxItemsView?.SetProperties(null);
                    speedListBoxItemsView?.SetProperties(null);
                    return;
                }

                string selectedAxis = selectAxisListBoxItemsView?.SelectedItemName ?? string.Empty;
                if (string.IsNullOrWhiteSpace(selectedAxis))
                {
                    configurationListBoxItemsView?.SetProperties(null);
                    speedListBoxItemsView?.SetProperties(null);
                    return;
                }

                if (_axisManager == null)
                {
                    MessageBox.Show("AxisManager가 초기화되지 않았습니다.", "알림",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var axis = _axisManager.Get(UNIT_NAME, selectedAxis);
                if (axis == null)
                {
                    Log.Write("LCP-280", $"Axis not found: {UNIT_NAME} / {selectedAxis}");
                    configurationListBoxItemsView?.SetProperties(null);
                    speedListBoxItemsView?.SetProperties(null);
                    return;
                }

                _axis = axis;
                if (_axis.Name == "Index T Axis")
                {
                    btnHome.Enabled = true;
                }
                else
                {
                    btnHome.Enabled = true;
                }

                _editorProrertiesPosition = BuildPositionProperties(axis);
                _editorPropertiesConfig = BuildConfigProperties(axis);
                _editorPropertiesSpeed = BuildSpeedProperties(axis);
                positionVelocityPropertyCollectionView?.SetProperties(_editorProrertiesPosition);
                configurationListBoxItemsView?.SetProperties(_editorPropertiesConfig);
                speedListBoxItemsView?.SetProperties(_editorPropertiesSpeed);

                //motorIoPropertyCollectionView
                var ioProperties = new PropertyCollection();
                ioProperties.ShowNoColumn = false; // 0열 표시 옵션
                //ioProperties.Add(new TitleOnlyProperty("No", "Name", "State")); // title 행 표시
                ioProperties.Add(new PropertyState("01", "Servo On", axis.Status.IO.ServoOn));
                ioProperties.Add(new PropertyState("02", "Alarm", axis.Status.IO.Alarm));
                ioProperties.Add(new PropertyState("03", "Negative Limit Sensor", axis.Status.IO.NegativeLimitSensor));
                ioProperties.Add(new PropertyState("04", "Positive Limit Sensor", axis.Status.IO.PositiveLimitSensor));
                ioProperties.Add(new PropertyState("05", "Home Sensor", axis.Status.IO.HomeSensor));
                motorIoPropertyCollectionView?.SetProperties(ioProperties);
                

                var state = new PropertyCollection();
                state.ShowNoColumn = false; // 0열 표시 옵션
                state.Add(new PropertyState("01", "Done", axis.Status.State.Done));
                state.Add(new PropertyState("02", "Inposition", axis.Status.State.Inposition));
                state.Add(new PropertyState("03", "Inposition Done", axis.Status.State.InpositionDone));
                state.Add(new PropertyState("04", "Inposition Timeout", axis.Status.State.InpositionTimeout));
                state.Add(new PropertyState("05", "Home End", axis.Status.State.HomeEnd));
                state.Add(new PropertyState("06", "Home Timeout", axis.Status.State.HomeTimeout));
                state.IsInputParameter = false;    //출력
                motorStateIoPropertyCollectionView?.SetProperties(state);

            }
            catch (Exception ex)
            {
                Log.Write("LCP-280", $"OnPositionItemSelected error: {ex}");
            }
        }

        // -------- Builders
        private PropertyCollection BuildPositionProperties(MotionAxis axis)
        {
            // NOTE: 현재 장치 값들이 enum/bit인 곳은 DoubleProperty 임시 사용 유지 (주석 표기)
            var pc = new PropertyCollection();

            pc.Add(new DoubleProperty("Command Position", axis.Status.PV.CommandPosition));
            pc.Add(new DoubleProperty("Actual Position", axis.Status.PV.ActualPosition));
            pc.Add(new DoubleProperty("Error Position", axis.Status.PV.ErrorPosition));
            pc.Add(new DoubleProperty("Command Velocity", axis.Status.PV.CommandVelocity));
            pc.Add(new DoubleProperty("Actual Velocity", axis.Status.PV.ActualVelocity));
            pc.IsInputParameter = false;    //출력

            return pc;
        }
        private PropertyCollection BuildConfigProperties(MotionAxis axis)
        {
            // NOTE: 현재 장치 값들이 enum/bit인 곳은 DoubleProperty 임시 사용 유지 (주석 표기)
            var pc = new PropertyCollection();

            // Common
            pc.Add(new TitleOnlyProperty("Common"));
            pc.Add(new DoubleProperty("Pulses Per Unit", axis.Setup.PulsesPerUnit));
            pc.Add(new DoubleProperty("Axis Scale", axis.Setup.AxisScale));
            pc.Add(new DoubleProperty("Axis Power", axis.Setup.AxisPowerPercent));

            // Config
            pc.Add(new TitleOnlyProperty("Config"));
            pc.Add(new DoubleProperty("Pulse Output", (double)axis.Setup.PulseOutput));       // TODO: Enum Editor
            pc.Add(new DoubleProperty("Encoder Input", (double)axis.Setup.EncoderInput));         // TODO: Enum Editor
            pc.Add(new DoubleProperty("Input Source", (double)axis.Setup.InputSource));     // TODO: Enum Editor
            pc.Add(new DoubleProperty("Z Phase Level", (double)axis.Setup.ZPhaseLevel));    // TODO: Enum Editor
            pc.Add(new DoubleProperty("Servo Level", (double)axis.Setup.ServoOnLevel));       // TODO: Enum Editor

            // Emergency
            pc.Add(new TitleOnlyProperty("Emergency Signal"));
            pc.Add(new DoubleProperty("Level", (double)axis.Setup.EmergencyLevel));         // TODO: Enum Editor
            pc.Add(new DoubleProperty("Stop Mode", (double)axis.Setup.StopMode));           // TODO: Enum Editor

            // Inposition
            pc.Add(new TitleOnlyProperty("Inposition"));
            pc.Add(new DoubleProperty("Level", (double)axis.Setup.InPosition));        // TODO: Enum Editor
            pc.Add(new BoolProperty("Software", axis.Setup.SoftwareLimitEnable));           // bool OK
            pc.Add(new DoubleProperty("Software Length", (double)axis.Setup.SoftwareLength));
         
            // Alarm
            pc.Add(new TitleOnlyProperty("Alarm"));
            pc.Add(new DoubleProperty("Reset Signal", (double)axis.Setup.AlarmResetLevel));// TODO: Enum Editor
            pc.Add(new DoubleProperty("Level", (double)axis.Setup.AlarmLevel));             // TODO: Enum Editor

            // Limit
            pc.Add(new TitleOnlyProperty("Limit"));
            pc.Add(new DoubleProperty("+End Limit", (double)axis.Setup.PositiveLimitLevel));
            pc.Add(new DoubleProperty("-End Limit", (double)axis.Setup.NegativeLimitLevel));
            pc.Add(new DoubleProperty("Soft Limit -", (double)axis.Setup.SoftLimitMin));
            pc.Add(new DoubleProperty("Soft Limit +", (double)axis.Setup.SoftLimitMax));

            // Home
            pc.Add(new TitleOnlyProperty("Home"));
            pc.Add(new DoubleProperty("Mode", (double)axis.Setup.HomeMode));                // TODO: Enum Editor
            pc.Add(new DoubleProperty("SignalLevel", (double)axis.Setup.HomeSignalLevel));       // TODO: Enum Editor
            pc.Add(new DoubleProperty("Direction", (double)axis.Setup.HomeDirection));
            pc.Add(new DoubleProperty("Signal", (double)axis.Setup.HomeSignal));
            pc.Add(new DoubleProperty("Z Phase", (double)axis.Setup.HomeZPhase));
            pc.Add(new DoubleProperty("Clear Time(ms)", (double)axis.Setup.HomeClearTime));
            pc.Add(new DoubleProperty("Offset(mm)", (double)axis.Setup.HomeOffset));


            return pc;
        }
        private PropertyCollection BuildSpeedProperties(MotionAxis axis)
        {
            // NOTE: 현재 장치 값들이 enum/bit인 곳은 DoubleProperty 임시 사용 유지 (주석 표기)
            var pc = new PropertyCollection();

            // Home
            pc.Add(new TitleOnlyProperty("Home"));
            pc.Add(new DoubleProperty("Vel. 1st(mm/s)", axis.Config.HomeFirstSpeed));
            pc.Add(new DoubleProperty("Vel. 2nd(mm/s)", axis.Config.HomeSecondSpeed));
            pc.Add(new DoubleProperty("Vel. 3rd(mm/s)", axis.Config.HomeThirdSpeed));
            pc.Add(new DoubleProperty("Vel. Last(mm/s)", axis.Config.HomeLastSpeed));
            pc.Add(new DoubleProperty("Accel. 1st(mm/s^2)", axis.Config.HomeFirstAcc));
            pc.Add(new DoubleProperty("Accel. 2nd(mm/s^2)", axis.Config.HomeSecondAcc));

            // Jog
            pc.Add(new TitleOnlyProperty("Jog"));
            pc.Add(new DoubleProperty("Fine Velocity(mm/s)", axis.Config.JogFineVelocity));       // TODO: Enum Editor
            pc.Add(new DoubleProperty("Coarse Velocity(mm/s)", axis.Config.JogCoarseVelocity));         // TODO: Enum Editor
            pc.Add(new DoubleProperty("Accelerator(mm/s^2)", axis.Config.JogAcc));     // TODO: Enum Editor
            pc.Add(new DoubleProperty("Decelerator(mm/s^2)", axis.Config.JogDec));    // TODO: Enum Editor

            // Run
            pc.Add(new TitleOnlyProperty("Run"));
            pc.Add(new DoubleProperty("Maximum Velocity(mm/s)", axis.Config.MaxVelocity));
            pc.Add(new DoubleProperty("Accelerator(mm/s^2)", axis.Config.RunAcc));
            pc.Add(new DoubleProperty("Decelerator(mm/s^2)", axis.Config.RunDec));
            pc.Add(new DoubleProperty("Profile", (int)axis.Config.ProfileMode));
            pc.Add(new DoubleProperty("Accelerator Jerk(%)", axis.Config.AccJerkPercent));
            pc.Add(new DoubleProperty("Decelerator Jerk(%)", axis.Config.DecJerkPercent));

            return pc;
        }


        private void OnMotorIOItemClicked(object sender, string key)
        {
            var k = key?.Trim().ToUpperInvariant();
            if (string.IsNullOrEmpty(k)) return;

            try
            {
                if (k == "SERVO")
                {
                    if(_axis.Status.IO.ServoOn)
                    {
                        var ask = new MessageBoxYesNo();
                        if (ask.ShowDialog("Info", "Servo Off 하시겠습니까?") == DialogResult.No) return;
                    }

                    bool wantOn = !_axis.Status.IO.ServoOn;

                    int rc = _axis.Servo(wantOn);
                    if (rc != 0) MessageBox.Show($"Servo {(wantOn ? "On" : "Off")} 실패 (rc={rc})");
                    // 낙관적 UI 업데이트 (즉시 색 반영)
                    motorIoPropertyCollectionView.SetStateByKey("SERVO", wantOn);
                }
                else if (k == "ALARM")
                {
                    int rc = _axis.ClearAlarm();
                    if (rc != 0) MessageBox.Show($"Alarm Reset 실패 (rc={rc})");
                    // 일반적으로 리셋 후 알람 Off 기대
                    motorIoPropertyCollectionView.SetStateByKey("ALARM", false);
                }
                else
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            finally
            {
                // 실제 상태 재조회 후 다시 그려 정확도 보정
                RefreshMotorIO();
            }
        }

        private void RefreshMotorIO()
        {
            // axis.Status.IO 재읽기 → 위와 동일하게 ioProperties 다시 만들고 SetProperties 호출
            var ioProperties = new PropertyCollection();
            ioProperties.ShowNoColumn = false; // 0열 표시 옵션
            //ioProperties.Add(new TitleOnlyProperty("No", "Name", "State")); // title 행 표시
            ioProperties.Add(new PropertyState("01", "Servo On", _axis.Status.IO.ServoOn));
            ioProperties.Add(new PropertyState("02", "Alarm", _axis.Status.IO.Alarm));
            ioProperties.Add(new PropertyState("03", "Negative Limit Sensor", _axis.Status.IO.NegativeLimitSensor));
            ioProperties.Add(new PropertyState("04", "Positive Limit Sensor", _axis.Status.IO.PositiveLimitSensor));
            ioProperties.Add(new PropertyState("05", "Home Sensor", _axis.Status.IO.HomeSensor));
            motorIoPropertyCollectionView?.SetProperties(ioProperties);
        }





        // -------- Axis status polling (optional)
        private void InitializeStatusTimer()
        {
            // 필요한 경우만 쓰세요. (미사용이면 주석 처리 가능)
            if (_axisPosTimer != null)
            {
                _axisPosTimer.Tick -= AxisPosTimer_Tick;
                _axisPosTimer.Dispose();
            }

            _axisPosTimer = new Timer
            {
                Interval = 200 // ms
            };
            _axisPosTimer.Tick += AxisPosTimer_Tick;
            _axisPosTimer.Start();
        }

        private void AxisPosTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                UpdateAxisActualPosition();
            }
            catch (Exception ex)
            {
                Log.Write("LCP-280", $"AxisPosTimer_Tick error: {ex}");
            }
        }

        // -------- Stubs / Overrides
        private void UpdateAxisActualPosition()
        {
            // TODO: 실제 위치/속도/상태 갱신 바인딩
            // positionVelocityPropertyCollectionView.SetProperties(...); 등


        }

        private void btnMovePosition_Click(object sender, EventArgs e)
        {
            // TODO: 선택 포지션 이동 구현
        }

        private void InitializeRadioButtonView()
        {
            try
            {
                // TODO: 라디오버튼 초기화/바인딩
            }
            catch (Exception ex)
            {
                Log.Write("LCP-280", $"RadioButtonView 오류: {ex}");
            }
        }

        //private void btn_Save_Setup_Motion_Configuration_Click(object sender, EventArgs e)
        //{
        //    // TODO: _editorPropertiesConfig → axis.Setup 반영 & 저장
        //}

        private void btnCancel_Click(object sender, EventArgs e)
        {
            // TODO: 변경 취소 로직
        }

        // --- Paint / Resize (keep)
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            int centerX = this.ClientSize.Width / 2;
            using (Pen blackPen = new Pen(Color.Black, 2))
            {
                e.Graphics.DrawLine(blackPen, centerX, 0, centerX, this.ClientSize.Height);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            this.Invalidate();
        }

        private IndividualMenuButton btn_Save_Setup_Motion_Speed;
        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel2;
        private Panel panel2;
        private Panel panel1;
        private IndividualMenuButton btnHome;
        private IndividualMenuButton btnServoOn;
        private IndividualMenuButton btnServoOff;
        private IndividualMenuButton btnHome1;
    }
}
