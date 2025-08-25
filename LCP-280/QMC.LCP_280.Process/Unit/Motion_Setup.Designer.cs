using QMC.Common;
using QMC.Common.CustomControl;
using QMC.Common.Motions;
using QMC.LCP_280.Process;
using QMC.LCP_280.Process.Unit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace QMC.LCP_280.Process.Unit
{
    partial class Motion_Setup
    {
        Equipment equipment = Equipment.Instance;
        private MotionAxisManager _axisManager;

        // Axis 목록 (추가)
        private ListBoxItemsView axisListBoxItemsView;
        private ListBoxItemsView selectAxisListBoxItemsView;
        private IOPropertyCollectionView motorStateIoPropertyCollectionView;
        private IOPropertyCollectionView motorIoPropertyCollectionView;
        private PropertyCollectionView positionVelocityPropertyCollectionView;
        private ListBoxItemsView speedListBoxItemsView;
        //private ListBoxItemsView configurationListBoxItemsView;
        private PropertyCollectionView configurationListBoxItemsView;

        // 선택된 축의 속성(편집용) 캐시
        private PropertyCollection _editorPropertiesConfig;

        private GroupBox gbAxisProperty;
        private GroupBox gbAxisPositions;

        private System.ComponentModel.IContainer components = null;

        // Actual Position 주기 업데이트 타이머
        private Timer _axisPosTimer;

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
            this.gbAxisProperty = new System.Windows.Forms.GroupBox();
            this.speedListBoxItemsView = new QMC.Common.ListBoxItemsView();
            this.configurationListBoxItemsView = new QMC.Common.PropertyCollectionView();
            this.gbAxisPositions = new System.Windows.Forms.GroupBox();
            this.motorStateIoPropertyCollectionView = new QMC.Common.IOPropertyCollectionView();
            this.motorIoPropertyCollectionView = new QMC.Common.IOPropertyCollectionView();
            this.positionVelocityPropertyCollectionView = new QMC.Common.PropertyCollectionView();
            this.axisListBoxItemsView = new QMC.Common.ListBoxItemsView();
            this.selectAxisListBoxItemsView = new QMC.Common.ListBoxItemsView();
            this.btn_Save_Setup_Motion_Configuration = new QMC.Common.IndividualMenuButton();
            this.gbAxisProperty.SuspendLayout();
            this.gbAxisPositions.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbAxisProperty
            // 
            this.gbAxisProperty.BackColor = System.Drawing.Color.White;
            this.gbAxisProperty.Controls.Add(this.btn_Save_Setup_Motion_Configuration);
            this.gbAxisProperty.Controls.Add(this.speedListBoxItemsView);
            this.gbAxisProperty.Controls.Add(this.configurationListBoxItemsView);
            this.gbAxisProperty.Font = new System.Drawing.Font("맑은 고딕", 10F);
            this.gbAxisProperty.Location = new System.Drawing.Point(643, 12);
            this.gbAxisProperty.Name = "gbAxisProperty";
            this.gbAxisProperty.Size = new System.Drawing.Size(609, 722);
            this.gbAxisProperty.TabIndex = 10;
            this.gbAxisProperty.TabStop = false;
            this.gbAxisProperty.Text = "Axis Property";
            // 
            // speedListBoxItemsView
            // 
            this.speedListBoxItemsView.BorderWidth = 2;
            this.speedListBoxItemsView.GroupName = "Speed";
            this.speedListBoxItemsView.Location = new System.Drawing.Point(313, 27);
            this.speedListBoxItemsView.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.speedListBoxItemsView.Name = "speedListBoxItemsView";
            this.speedListBoxItemsView.SelectedIndex = -1;
            this.speedListBoxItemsView.Size = new System.Drawing.Size(290, 688);
            this.speedListBoxItemsView.TabIndex = 1;
            // 
            // configurationListBoxItemsView
            // 
            this.configurationListBoxItemsView.GroupName = "Configuration";
            this.configurationListBoxItemsView.Location = new System.Drawing.Point(6, 37);
            this.configurationListBoxItemsView.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.configurationListBoxItemsView.Name = "configurationListBoxItemsView";
            this.configurationListBoxItemsView.Size = new System.Drawing.Size(290, 631);
            this.configurationListBoxItemsView.TabIndex = 0;
            // 
            // gbAxisPositions
            // 
            this.gbAxisPositions.BackColor = System.Drawing.Color.White;
            this.gbAxisPositions.Controls.Add(this.motorStateIoPropertyCollectionView);
            this.gbAxisPositions.Controls.Add(this.motorIoPropertyCollectionView);
            this.gbAxisPositions.Controls.Add(this.positionVelocityPropertyCollectionView);
            this.gbAxisPositions.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.gbAxisPositions.Location = new System.Drawing.Point(326, 12);
            this.gbAxisPositions.Name = "gbAxisPositions";
            this.gbAxisPositions.Size = new System.Drawing.Size(305, 722);
            this.gbAxisPositions.TabIndex = 13;
            this.gbAxisPositions.TabStop = false;
            this.gbAxisPositions.Text = "Axis Status";
            // 
            // motorStateIoPropertyCollectionView
            // 
            this.motorStateIoPropertyCollectionView.GroupName = "Motor State";
            this.motorStateIoPropertyCollectionView.Location = new System.Drawing.Point(6, 495);
            this.motorStateIoPropertyCollectionView.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.motorStateIoPropertyCollectionView.Name = "motorStateIoPropertyCollectionView";
            this.motorStateIoPropertyCollectionView.Size = new System.Drawing.Size(293, 220);
            this.motorStateIoPropertyCollectionView.TabIndex = 2;
            // 
            // motorIoPropertyCollectionView
            // 
            this.motorIoPropertyCollectionView.GroupName = "Motor I/O";
            this.motorIoPropertyCollectionView.Location = new System.Drawing.Point(6, 265);
            this.motorIoPropertyCollectionView.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.motorIoPropertyCollectionView.Name = "motorIoPropertyCollectionView";
            this.motorIoPropertyCollectionView.Size = new System.Drawing.Size(293, 220);
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
            // axisListBoxItemsView
            // 
            this.axisListBoxItemsView.BorderWidth = 2;
            this.axisListBoxItemsView.GroupName = "";
            this.axisListBoxItemsView.Location = new System.Drawing.Point(8, 18);
            this.axisListBoxItemsView.Name = "axisListBoxItemsView";
            this.axisListBoxItemsView.SelectedIndex = -1;
            this.axisListBoxItemsView.Size = new System.Drawing.Size(234, 124);
            this.axisListBoxItemsView.TabIndex = 0;
            this.axisListBoxItemsView.ItemSelected += new System.EventHandler<int>(this.OnAxisSelected);
            // 
            // selectAxisListBoxItemsView
            // 
            this.selectAxisListBoxItemsView.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.selectAxisListBoxItemsView.BorderWidth = 2;
            this.selectAxisListBoxItemsView.GroupName = "Select Axis";
            this.selectAxisListBoxItemsView.Location = new System.Drawing.Point(12, 12);
            this.selectAxisListBoxItemsView.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.selectAxisListBoxItemsView.Name = "selectAxisListBoxItemsView";
            this.selectAxisListBoxItemsView.SelectedIndex = -1;
            this.selectAxisListBoxItemsView.Size = new System.Drawing.Size(305, 722);
            this.selectAxisListBoxItemsView.TabIndex = 2;
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
            this.btn_Save_Setup_Motion_Configuration.Location = new System.Drawing.Point(196, 675);
            this.btn_Save_Setup_Motion_Configuration.Name = "btn_Save_Setup_Motion_Configuration";
            this.btn_Save_Setup_Motion_Configuration.Size = new System.Drawing.Size(100, 40);
            this.btn_Save_Setup_Motion_Configuration.TabIndex = 4;
            this.btn_Save_Setup_Motion_Configuration.TabStop = false;
            this.btn_Save_Setup_Motion_Configuration.Text = "Save";
            this.btn_Save_Setup_Motion_Configuration.UseVisualStyleBackColor = false;
            this.btn_Save_Setup_Motion_Configuration.Click += new System.EventHandler(this.btn_Save_Setup_Motion_Configuration_Click);
            // 
            // Motion_Setup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1264, 752);
            this.Controls.Add(this.gbAxisPositions);
            this.Controls.Add(this.gbAxisProperty);
            this.Controls.Add(this.selectAxisListBoxItemsView);
            this.Name = "Motion_Setup";
            this.Text = "Motion Setup";
            this.gbAxisProperty.ResumeLayout(false);
            this.gbAxisPositions.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private void InitializeUI()
        {
            try
            {
                // 🚀 PropertyPosition을 사용하여 Position Item들을 listBoxItemsView에 설정
                SetAxisDefinitionsToAxisListBox();

                // 🚀 Position Item 선택 이벤트 연결
                SetupAxisItemSelectionEvent();

                InitializeRadioButtonView();
            }
            catch (Exception ex)
            {

            }
        }
    /// <summary>
    /// CassetteElevator + WaferTransferArm 의 AxisDefinition DisplayName 을 axisListBoxItemsView 에 설정
    /// </summary>
    private void SetAxisDefinitionsToAxisListBox()
        {
            try
            {
                if (_axisManager == null)
                {
                    MessageBox.Show("AxisManager가 초기화되지 않았습니다.", "알림",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                const string UNIT_NAME = "unit";

                // 1) 유닛 목록 바인딩: GetKeys() → "Unit||Axis" 에서 Unit만 추출
                var unitNames = _axisManager
                    .GetKeys()
                    .Select(k => {
                        int idx = k.IndexOf("||", StringComparison.Ordinal);
                        return (idx >= 0) ? k.Substring(0, idx) : k;
                    })
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(s => s, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var axisNames = _axisManager.GetAxisNames(UNIT_NAME) ?? Array.Empty<string>();

                if (axisNames.Length > 0)
                {
                    selectAxisListBoxItemsView?.SetItems(axisNames);
                }
                else
                {
                    Log.Write("LCP-280", "⚠️ PropertyPosition에 Position 항목이 없습니다.");
                    selectAxisListBoxItemsView?.SetItems();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ PropertyPosition 설정 중 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// 🚀 Position Item 선택 이벤트 설정
        /// </summary>
        private void SetupAxisItemSelectionEvent()
        {
            if (selectAxisListBoxItemsView != null)
            {
                // 기존 이벤트 핸들러 제거 (중복 방지)
                selectAxisListBoxItemsView.ItemSelected -= OnPositionItemSelected;

                // 새 이벤트 핸들러 등록
                selectAxisListBoxItemsView.ItemSelected += OnPositionItemSelected;

                Console.WriteLine("✅ Position Item 선택 이벤트 설정 완료");
            }
        }
        /// <summary>
        /// 🚀 Position Item 선택 이벤트 처리
        /// </summary>
        private void OnPositionItemSelected(object sender, int selectedIndex)
        {
            try
            {
                const string UNIT_NAME = "unit";
                string selectedAxis = string.Empty;
                if (selectedIndex >= 0)
                {
                    selectedAxis = selectAxisListBoxItemsView.SelectedItemName;
                }
                var axis = _axisManager.Get(UNIT_NAME, selectedAxis);

                // ▶ 편집용 컬렉션을 필드에 유지
                _editorPropertiesConfig = new PropertyCollection();

                DoubleProperty doubleProperty;
                BoolProperty boolProperty;
                _editorPropertiesConfig.Add(new TitleOnlyProperty("Common"));
                doubleProperty = new DoubleProperty("Axis Scale", axis.Setup.AxisScale);
                _editorPropertiesConfig.Add(doubleProperty);
                doubleProperty = new DoubleProperty("Axis Power", axis.Setup.AxisPowerPercent);
                _editorPropertiesConfig.Add(doubleProperty);
                _editorPropertiesConfig.Add(new TitleOnlyProperty("Config"));
                doubleProperty = new DoubleProperty("Output Mode", (double)axis.Setup.OutputMode); //임시
                _editorPropertiesConfig.Add(doubleProperty);
                doubleProperty = new DoubleProperty("Input Mode", (double)axis.Setup.InputMode); //임시
                _editorPropertiesConfig.Add(doubleProperty);
                doubleProperty = new DoubleProperty("Input Source", (double)axis.Setup.InputSource); //임시
                _editorPropertiesConfig.Add(doubleProperty);
                doubleProperty = new DoubleProperty("Z Phase Level", (double)axis.Setup.ZPhaseLevel); //임시
                _editorPropertiesConfig.Add(doubleProperty);
                doubleProperty = new DoubleProperty("Servo Level", (double)axis.Setup.ServoLevel); //임시
                _editorPropertiesConfig.Add(doubleProperty);
                _editorPropertiesConfig.Add(new TitleOnlyProperty("Emergency Signal"));
                doubleProperty = new DoubleProperty("Level", (double)axis.Setup.EmergencyLevel); //임시
                _editorPropertiesConfig.Add(doubleProperty);
                doubleProperty = new DoubleProperty("Stop Mode", (double)axis.Setup.StopMode); //임시
                _editorPropertiesConfig.Add(doubleProperty);
                _editorPropertiesConfig.Add(new TitleOnlyProperty("Inposition"));
                doubleProperty = new DoubleProperty("Level", (double)axis.Setup.InpositionLevel); //임시
                _editorPropertiesConfig.Add(doubleProperty);
                boolProperty = new BoolProperty("Software", axis.Setup.SoftwareLimitEnable); //임시
                _editorPropertiesConfig.Add(boolProperty);
                doubleProperty = new DoubleProperty("Software Length", (double)axis.Setup.SoftwareLength); //임시
                _editorPropertiesConfig.Add(doubleProperty);
                _editorPropertiesConfig.Add(new TitleOnlyProperty("Home"));
                doubleProperty = new DoubleProperty("Signal", (double)axis.Setup.HomeSignalLevel); //임시
                _editorPropertiesConfig.Add(doubleProperty);
                doubleProperty = new DoubleProperty("Mode", (double)axis.Setup.HomeMode); //임시
                _editorPropertiesConfig.Add(doubleProperty);
                _editorPropertiesConfig.Add(new TitleOnlyProperty("Alarm"));
                doubleProperty = new DoubleProperty("Reset Signal", (double)axis.Setup.AlarmResetSignal); //임시
                _editorPropertiesConfig.Add(doubleProperty);
                doubleProperty = new DoubleProperty("Level", (double)axis.Setup.AlarmLevel); //임시
                _editorPropertiesConfig.Add(doubleProperty);
                _editorPropertiesConfig.Add(new TitleOnlyProperty("Limit"));
                doubleProperty = new DoubleProperty("Soft Limit -", (double)axis.Setup.SoftLimitMin); //임시
                _editorPropertiesConfig.Add(doubleProperty);
                doubleProperty = new DoubleProperty("Soft Limit +", (double)axis.Setup.SoftLimitMax); //임시
                _editorPropertiesConfig.Add(doubleProperty);

                // PropertyCollectionView에 Editor 내용 설정
                configurationListBoxItemsView?.SetProperties(_editorPropertiesConfig);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Position Item 선택 처리 중 오류: {ex.Message}");
            }
        }

        private void OnAxisSelected(object sender, int index)
        {
           
        }

        private void UpdateAxisActualPosition()
        {
           
        }

        private void btnMovePosition_Click(object sender, EventArgs e)
        {
           
        }  

        private void InitializeRadioButtonView()
        {
            try
            {
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RadioButtonView 오류: {ex.Message}");
            }
        }

        #region Save / Cancel


      #region Save / Cancel

        private void btnCancel_Click(object sender, EventArgs e)
        {
            
        }
           

        #endregion  #region Paint / Resize override (기존)

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


        #endregion

        private IndividualMenuButton btn_Save_Setup_Motion_Configuration;
    }
}