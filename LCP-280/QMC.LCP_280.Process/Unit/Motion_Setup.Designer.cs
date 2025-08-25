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
            this.btn_Save_Setup_Motion_Speed = new QMC.Common.IndividualMenuButton();
            this.btn_Save_Setup_Motion_Configuration = new QMC.Common.IndividualMenuButton();
            this.speedListBoxItemsView = new QMC.Common.PropertyCollectionView();
            this.configurationListBoxItemsView = new QMC.Common.PropertyCollectionView();
            this.gbAxisPositions = new System.Windows.Forms.GroupBox();
            this.motorStateIoPropertyCollectionView = new QMC.Common.IOPropertyCollectionView();
            this.motorIoPropertyCollectionView = new QMC.Common.IOPropertyCollectionView();
            this.positionVelocityPropertyCollectionView = new QMC.Common.PropertyCollectionView();
            this.selectAxisListBoxItemsView = new QMC.Common.ListBoxItemsView();
            this.gbAxisProperty.SuspendLayout();
            this.gbAxisPositions.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbAxisProperty
            // 
            this.gbAxisProperty.BackColor = System.Drawing.Color.White;
            this.gbAxisProperty.Controls.Add(this.btn_Save_Setup_Motion_Speed);
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
            this.btn_Save_Setup_Motion_Speed.Location = new System.Drawing.Point(503, 675);
            this.btn_Save_Setup_Motion_Speed.Name = "btn_Save_Setup_Motion_Speed";
            this.btn_Save_Setup_Motion_Speed.Size = new System.Drawing.Size(100, 40);
            this.btn_Save_Setup_Motion_Speed.TabIndex = 5;
            this.btn_Save_Setup_Motion_Speed.TabStop = false;
            this.btn_Save_Setup_Motion_Speed.Text = "Save";
            this.btn_Save_Setup_Motion_Speed.UseVisualStyleBackColor = false;
            this.btn_Save_Setup_Motion_Speed.Click += new System.EventHandler(this.btn_Save_Setup_Motion_Speed_Click);
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
            // speedListBoxItemsView
            // 
            this.speedListBoxItemsView.GroupName = "Speed";
            this.speedListBoxItemsView.Location = new System.Drawing.Point(313, 27);
            this.speedListBoxItemsView.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.speedListBoxItemsView.Name = "speedListBoxItemsView";
            this.speedListBoxItemsView.Size = new System.Drawing.Size(290, 641);
            this.speedListBoxItemsView.TabIndex = 1;
            // 
            // configurationListBoxItemsView
            // 
            this.configurationListBoxItemsView.GroupName = "Configuration";
            this.configurationListBoxItemsView.Location = new System.Drawing.Point(6, 27);
            this.configurationListBoxItemsView.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.configurationListBoxItemsView.Name = "configurationListBoxItemsView";
            this.configurationListBoxItemsView.Size = new System.Drawing.Size(290, 641);
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
            // Motion_Setup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1264, 752);
            this.Controls.Add(this.gbAxisPositions);
            this.Controls.Add(this.gbAxisProperty);
            this.Controls.Add(this.selectAxisListBoxItemsView);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Motion_Setup";
            this.Text = "Motion Setup";
            this.gbAxisProperty.ResumeLayout(false);
            this.gbAxisPositions.ResumeLayout(false);
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
            pc.Add(new DoubleProperty("Axis Scale", axis.Setup.AxisScale));
            pc.Add(new DoubleProperty("Axis Power", axis.Setup.AxisPowerPercent));

            // Config
            pc.Add(new TitleOnlyProperty("Config"));
            pc.Add(new DoubleProperty("Output Mode", (double)axis.Setup.OutputMode));       // TODO: Enum Editor
            pc.Add(new DoubleProperty("Input Mode", (double)axis.Setup.InputMode));         // TODO: Enum Editor
            pc.Add(new DoubleProperty("Input Source", (double)axis.Setup.InputSource));     // TODO: Enum Editor
            pc.Add(new DoubleProperty("Z Phase Level", (double)axis.Setup.ZPhaseLevel));    // TODO: Enum Editor
            pc.Add(new DoubleProperty("Servo Level", (double)axis.Setup.ServoLevel));       // TODO: Enum Editor

            // Emergency
            pc.Add(new TitleOnlyProperty("Emergency Signal"));
            pc.Add(new DoubleProperty("Level", (double)axis.Setup.EmergencyLevel));         // TODO: Enum Editor
            pc.Add(new DoubleProperty("Stop Mode", (double)axis.Setup.StopMode));           // TODO: Enum Editor

            // Inposition
            pc.Add(new TitleOnlyProperty("Inposition"));
            pc.Add(new DoubleProperty("Level", (double)axis.Setup.InpositionLevel));        // TODO: Enum Editor
            pc.Add(new BoolProperty("Software", axis.Setup.SoftwareLimitEnable));           // bool OK
            pc.Add(new DoubleProperty("Software Length", (double)axis.Setup.SoftwareLength));

            // Home
            pc.Add(new TitleOnlyProperty("Home"));
            pc.Add(new DoubleProperty("Signal", (double)axis.Setup.HomeSignalLevel));       // TODO: Enum Editor
            pc.Add(new DoubleProperty("Mode", (double)axis.Setup.HomeMode));                // TODO: Enum Editor

            // Alarm
            pc.Add(new TitleOnlyProperty("Alarm"));
            pc.Add(new DoubleProperty("Reset Signal", (double)axis.Setup.AlarmResetSignal));// TODO: Enum Editor
            pc.Add(new DoubleProperty("Level", (double)axis.Setup.AlarmLevel));             // TODO: Enum Editor

            // Limit
            pc.Add(new TitleOnlyProperty("Limit"));
            pc.Add(new DoubleProperty("Soft Limit -", (double)axis.Setup.SoftLimitMin));
            pc.Add(new DoubleProperty("Soft Limit +", (double)axis.Setup.SoftLimitMax));

            return pc;
        }
        private PropertyCollection BuildSpeedProperties(MotionAxis axis)
        {
            // NOTE: 현재 장치 값들이 enum/bit인 곳은 DoubleProperty 임시 사용 유지 (주석 표기)
            var pc = new PropertyCollection();

            // Home
            pc.Add(new TitleOnlyProperty("Home"));
            pc.Add(new DoubleProperty("Home Speed(mm/s)", axis.Config.HomeSpeed));
            pc.Add(new DoubleProperty("H-Return Speed(mm/s)", axis.Config.HomeReturnSpeed));
            pc.Add(new DoubleProperty("H-Recursion Speed(mm/s)", axis.Config.HomeRecursionSpeed));
            pc.Add(new DoubleProperty("Z-Phase Speed(mm/s)", axis.Config.ZPhaseSpeed));
            pc.Add(new DoubleProperty("Home Acc(mm/s^2)", axis.Config.HomeAcc));
            pc.Add(new DoubleProperty("H-Return Acc(mm/s^2)", axis.Config.HomeReturnAcc));

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
    }
}
