using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Drawing; // Point 사용
using QMC.Common;
using QMC.Common.Motions;

namespace QMC.LCP_280.Process.Unit
{
    public partial class JogControl : UserControl
    {
        private const string UNIT_NAME = "unit";   // Motion_Setup 과 동일 키 사용
        private Equipment EquipmentInst { get { return Equipment.Instance; } }
        private MotionAxisManager _axisManager;
        private Timer _posTimer;

        public JogControl()
        {
            InitializeComponent();

            // 디자이너에서 열릴 때는 장비 접근 금지
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;

            _axisManager = EquipmentInst != null ? EquipmentInst.AxisManager : null;

            InitializeUI();
        }

        // ===== 초기화 (Motion_Setup 스타일) =====
        private void InitializeUI()
        {
            try
            {
                WireAxisSelectionEvent();  // Select Axis 선택 이벤트 연결 (중복 방지)
                WireJogEvents();           // 조그 버튼/라디오 이벤트 연결
                BindAxisList();            // 축 목록 바인딩 (unit 기준)
                ApplyMoveModeUI();         // 스텝/연속 UI 스위치
                InitializePositionTimer(); // 위치표시 타이머

                FixMoveModeRadioGrouping(); // ★ 라디오 그룹 강제 수정

                rdoFine.Checked = true;
                rdoStep.Checked = true;
            }
            catch (Exception ex)
            {
                Log.Write("LCP-280", "Form_AxisJogPopup.InitializeUI error: " + ex);
            }
        }

        // === 그룹박스 내부에 서로 다른 컨테이너(stepLine)에 들어가 있어서 동시에 체크되는 문제 해결 ===
        private void FixMoveModeRadioGrouping()
        {
            try
            {
                if (rdoContinuous == null || rdoStep == null || grpMoveMode == null) return;

                // rdoStep이 stepLine(FlowLayoutPanel) 안에 있으면 GroupBox와 다른 논리 그룹이 됨 → 제거 후 GroupBox에 직접 부착
                if (rdoStep.Parent != grpMoveMode)
                {
                    if (rdoStep.Parent != null)
                        rdoStep.Parent.Controls.Remove(rdoStep);

                    // 위치: Continuous 오른쪽 또는 아래 원하는 좌표 재배치
                    // 동일 Y 라인에 두고 싶으면 아래처럼.
                    var baseLoc = rdoContinuous.Location;
                    rdoStep.Location = new Point(baseLoc.X + 120, baseLoc.Y); // 간격 120px
                    grpMoveMode.Controls.Add(rdoStep);

                    // stepLine에는 NumericUpDown만 남도록 보장
                    if (stepLine != null && stepLine.Controls.Contains(rdoStep))
                        stepLine.Controls.Remove(rdoStep);
                }

                // 멀티체크가 이미 발생했다면 하나만 남기기
                if (rdoContinuous.Checked && rdoStep.Checked)
                {
                    // 기본은 Continuous 유지
                    rdoStep.Checked = false;
                }

                // CheckedChanged 이벤트가 두 컨트롤 모두에서 모드 전환을 반영하도록 필요 시 추가 로직 확장 가능
            }
            catch (Exception ex)
            {
                Log.Write("LCP-280", "FixMoveModeRadioGrouping error: " + ex);
            }
        }

        // ===== 축 목록 바인딩 =====
        private void BindAxisList()
        {
            try
            {
                if (_axisManager == null)
                {
                    MessageBox.Show("AxisManager가 초기화되지 않았습니다.", "알림",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    selectAxisListBoxItemsView.SetItems();
                    return;
                }

                selectAxisListBoxItemsView.SetItems();

                // Axis를 전부 하고 싶을때 하자.
                //var axisNames = _axisManager.GetAxisNames(UNIT_NAME) ?? new string[0];
                //if (axisNames.Length > 0)
                //    selectAxisListBoxItemsView.SetItems(axisNames);
                //else
                //    selectAxisListBoxItemsView.SetItems();
            }
            catch (Exception ex)
            {
                Log.Write("LCP-280", "BindAxisList error: " + ex);
                selectAxisListBoxItemsView.SetItems();
            }
        }

        // === 외부에서 티칭 포지션 축 목록을 직접 주입해서 리스트 갱신 (Teaching 전용) ===
        public void SetTeachingAxisList(IEnumerable<string> axisNames)
        {
            try
            {
                if (axisNames == null)
                {
                    selectAxisListBoxItemsView.SetItems();
                    return;
                }
                var arr = axisNames.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToArray();
                if (arr.Length == 0)
                    selectAxisListBoxItemsView.SetItems();
                else
                    selectAxisListBoxItemsView.SetItems(arr);
            }
            catch (Exception ex)
            {
                Log.Write("LCP-280", "SetTeachingAxisList error: " + ex);
                selectAxisListBoxItemsView.SetItems();
            }
        }

        // ===== Axis 선택 이벤트 연결/해제 =====
        private void WireAxisSelectionEvent()
        {
            if (selectAxisListBoxItemsView == null) return;
            selectAxisListBoxItemsView.ItemSelected -= OnAxisSelected; // 중복 구독 방지 (Motion_Setup 동일) :contentReference[oaicite:4]{index=4}
            selectAxisListBoxItemsView.ItemSelected += OnAxisSelected;
        }

        private void OnAxisSelected(object sender, int selectedIndex)
        {
            try
            {
                string axisName = selectAxisListBoxItemsView.SelectedItemName;
                UpdateJogEnableByAxisName(axisName);
                UpdatePositionOnce();
                UpdateUIByAxisSelection(axisName);
            }
            catch (Exception ex)
            {
                Log.Write("LCP-280", "OnAxisSelected error: " + ex);
            }
        }

        // ===== 조그/라디오 이벤트 일괄 연결 =====
        private void WireJogEvents()
        {
            rdoStep.CheckedChanged -= MoveModeChanged;
            rdoContinuous.CheckedChanged -= MoveModeChanged;
            rdoStep.CheckedChanged += MoveModeChanged;
            rdoContinuous.CheckedChanged += MoveModeChanged;

            // MouseDown/Up — 연속조그/스텝 양쪽 처리
            btnXMinus.MouseDown += JogButton_MouseDown; btnXMinus.MouseUp += JogButton_MouseUp;
            btnXPlus.MouseDown += JogButton_MouseDown; btnXPlus.MouseUp += JogButton_MouseUp;
            btnYMinus.MouseDown += JogButton_MouseDown; btnYMinus.MouseUp += JogButton_MouseUp;
            btnYPlus.MouseDown += JogButton_MouseDown; btnYPlus.MouseUp += JogButton_MouseUp;
            btnZMinus.MouseDown += JogButton_MouseDown; btnZMinus.MouseUp += JogButton_MouseUp;
            btnZPlus.MouseDown += JogButton_MouseDown; btnZPlus.MouseUp += JogButton_MouseUp;
            btnTMinus.MouseDown += JogButton_MouseDown; btnTMinus.MouseUp += JogButton_MouseUp;
            btnTPlus.MouseDown += JogButton_MouseDown; btnTPlus.MouseUp += JogButton_MouseUp;

            // CKD Index Move 이벤트 처리
            btnPrevIndex.Click += btnPrevIndex_Click;
            btnNextIndex.Click += btnNextIndex_Click;

            btnStop.Click += btnStop_Click;

            // 절대 위치 이동 버튼
            btnCommandPositionMove.Click += BtnCommandPositionMove_Click;
        }

        // ===== 모드/축에 따른 UI 스위치 =====
        private void MoveModeChanged(object sender, EventArgs e) { ApplyMoveModeUI(); }

        private void ApplyMoveModeUI()
        {
            bool step = rdoStep.Checked;
            nudStep.Enabled = step;
        }

        private void UpdateJogEnableByAxisName(string axisName)
        {
            if (axisName == "Index T Axis")
            {
                // CKD DD Motor
                btnXMinus.Enabled = btnXPlus.Enabled = false;
                btnYMinus.Enabled = btnYPlus.Enabled = false;
                btnZMinus.Enabled = btnZPlus.Enabled = false;
                btnTMinus.Enabled = btnTPlus.Enabled = false;
                btnPrevIndex.Enabled = btnNextIndex.Enabled = true;
            }
            else
            {
                bool x = HasAxisLetter(axisName, "X");
                bool y = HasAxisLetter(axisName, "Y");
                bool z = HasAxisLetter(axisName, "Z");
                bool t = HasAxisLetter(axisName, "T");

                btnXMinus.Enabled = btnXPlus.Enabled = x;
                btnYMinus.Enabled = btnYPlus.Enabled = y;
                btnZMinus.Enabled = btnZPlus.Enabled = z;
                btnTMinus.Enabled = btnTPlus.Enabled = t;
                btnPrevIndex.Enabled = btnNextIndex.Enabled = false;
            }
            btnStop.Enabled = true;
        }
        private static bool HasAxisLetter(string name, string letter)
        {
            if (string.IsNullOrEmpty(name)) return false;
            string s = name.ToUpperInvariant();
            letter = letter.ToUpperInvariant();
            return s.Contains(" " + letter + " ") || s.EndsWith(" " + letter + " AXIS") || s.Contains(letter + " AXIS");
        }

        // ===== 위치 표시 타이머 =====
        private void InitializePositionTimer()
        {
            if (_posTimer != null)
            {
                _posTimer.Stop();
                _posTimer.Tick -= PosTimer_Tick;
                _posTimer.Dispose();
            }
            _posTimer = new Timer();
            _posTimer.Interval = 200;
            _posTimer.Tick += PosTimer_Tick;
            _posTimer.Start();
        }

        private void PosTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                UpdatePositionOnce();
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

        private void UpdatePositionOnce()
        {
            if (_axisManager == null) return;
            string axisName = selectAxisListBoxItemsView.SelectedItemName;
            if (string.IsNullOrEmpty(axisName)) 
                return;

            var axis = _axisManager.Get(UNIT_NAME, axisName);
            if (axis == null || axis.Status == null || axis.Status.PV == null) return;

            double pos = axis.Status.PV.ActualPosition;
            lblPosition.Text = pos.ToString("0.000", CultureInfo.InvariantCulture);
        }

        // ===== Jog 처리 =====
        private enum JogAxis { X, Y, Z, T }
        private struct JogCommand { public JogAxis Axis; public int Sign; }

        private JogCommand ParseJogButton(object sender)
        {
            if (sender == btnXMinus) return new JogCommand { Axis = JogAxis.X, Sign = -1 };
            if (sender == btnXPlus) return new JogCommand { Axis = JogAxis.X, Sign = +1 };
            if (sender == btnYMinus) return new JogCommand { Axis = JogAxis.Y, Sign = -1 };
            if (sender == btnYPlus) return new JogCommand { Axis = JogAxis.Y, Sign = +1 };
            if (sender == btnZMinus) return new JogCommand { Axis = JogAxis.Z, Sign = -1 };
            if (sender == btnZPlus) return new JogCommand { Axis = JogAxis.Z, Sign = +1 };
            if (sender == btnTMinus) return new JogCommand { Axis = JogAxis.T, Sign = -1 };
            if (sender == btnTPlus) return new JogCommand { Axis = JogAxis.T, Sign = +1 };
            return new JogCommand();
        }

        private void JogButton_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                if (_axisManager == null) return;
                string axisName = selectAxisListBoxItemsView.SelectedItemName;
                if (string.IsNullOrEmpty(axisName)) return;

                MotionAxis axis = _axisManager.Get(UNIT_NAME, axisName);
                if (axis == null) return;

                JogCommand jc = ParseJogButton(sender);

                if (!CheckSafeToDrive(axis, jc))
                {
                    MessageBox.Show(this, "현재 상태에서 해당 축을 구동할 수 없습니다.", "Interlock", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (rdoContinuous.Checked)
                {
                    double vel = rdoFine.Checked ? axis.Config.JogFineVelocity : axis.Config.JogCoarseVelocity; // :contentReference[oaicite:5]{index=5}

                    //test
                    vel = 10;
                    StartJogContinuous(axis, jc, vel);
                }
                else
                {
                    double step = (double)nudStep.Value * jc.Sign;
                    DoStepMove(axis, jc, step);
                }
            }
            catch (Exception ex) { Log.Write(ex); }
        }

        private void JogButton_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                if (_axisManager == null) return;
                string axisName = selectAxisListBoxItemsView.SelectedItemName;
                if (string.IsNullOrEmpty(axisName)) return;

                MotionAxis axis = _axisManager.Get(UNIT_NAME, axisName);
                if (axis == null) return;

                if (!rdoStep.Checked)
                    StopJog(axis);
            }
            catch (Exception ex) { Log.Write(ex); }
        }

        private void btnPrevIndex_Click(object sender, EventArgs e)
        {
            try
            {
                if (_axisManager == null) return;
                string axisName = selectAxisListBoxItemsView.SelectedItemName;
                if (string.IsNullOrEmpty(axisName)) return;

                MotionAxis axis = _axisManager.Get(UNIT_NAME, axisName);
                if (axis == null) return;

                axis.MovePrevIndex();
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

        private void btnNextIndex_Click(object sender, EventArgs e)
        {
            try
            {
                if (_axisManager == null) return;
                string axisName = selectAxisListBoxItemsView.SelectedItemName;
                if (string.IsNullOrEmpty(axisName)) return;

                MotionAxis axis = _axisManager.Get(UNIT_NAME, axisName);
                if (axis == null) return;

                axis.MoveNextIndex();
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            try
            {
                if (_axisManager == null) return;
                string axisName = selectAxisListBoxItemsView.SelectedItemName;
                if (string.IsNullOrEmpty(axisName)) return;

                MotionAxis axis = _axisManager.Get(UNIT_NAME, axisName);
                if (axis == null) return;

                StopJog(axis);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

        // ★ 절대 위치 이동 (Command Position)
        private void BtnCommandPositionMove_Click(object sender, EventArgs e)
        {
            try
            {
                if (_axisManager == null) return;
                string axisName = selectAxisListBoxItemsView.SelectedItemName;
                if (string.IsNullOrEmpty(axisName)) return;

                MotionAxis axis = _axisManager.Get(UNIT_NAME, axisName);
                if (axis == null) return;

                double target = (double)nudCommandPosition.Value;

                double vel = rdoFine.Checked ? axis.Config.JogFineVelocity : axis.Config.JogCoarseVelocity;
                double acc = axis.Config.JogAcc;
                double dec = axis.Config.JogDec;
                double jerk = axis.Config.AccJerkPercent;

                axis.MoveAbs(target, vel, acc, dec, jerk);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

        // [ADD] Axis Move Interlock
        private bool CheckSafeToDrive(MotionAxis axis, JogCommand jc)
        {
            // Interlock
            if (axis == null)
                return false;

            bool result = true;
            switch (axis.Name)
            {
                case AxisNames.ProbeZ:
                    {
                        if (jc.Sign == 1)
                        {
                            IndexChipProbeController probeControl = EquipmentInst.GetUnit("IndexChipProbeController") as IndexChipProbeController;

                            // Probe Z는 상부 컨택에서만 사용하므로, 하부 컨택 모드일 경우 상승 구동을 금지한다.
                            if (probeControl != null && !probeControl.IsTopRequired())
                            {
                                result = false;
                            }
                        }
                        break;
                    }
                case AxisNames.ProbeCardZ:
                    {
                        if (jc.Sign == 1)
                        {
                            IndexChipProbeController probeControl = EquipmentInst.GetUnit("IndexChipProbeController") as IndexChipProbeController;

                            // Probe Card Z는 하부 컨택에서만 사용하므로, 상부 컨택 모드일 경우 상승 구동을 금지한다.
                            if (probeControl != null && probeControl.IsTopRequired())
                            {
                                result = false;
                            }
                        }
                        break;
                    }
            }
            return result;
        }

        // ★ 연속조그 시작
        private void StartJogContinuous(MotionAxis axis, JogCommand jc, double velocity = 5)
        {
            int dir = 1;// (axis.Config?.ManualJogDir ?? +1); // +1 또는 -1
            double signedVel = velocity * jc.Sign * dir; // 방향 포함

            axis.JogStart(signedVel);
        }

        // ★ 연속조그 정지
        private void StopJog(MotionAxis axis)
        {
            axis.JogStop();  // 필요 시 급정지 버튼에서는 axis.JogEStop();
        }

        // ★ 스텝조그(1회 이동) — jerk 포함!
        private void DoStepMove(MotionAxis axis, JogCommand jc, double stepUnit)
        {
            // (선택) 장비 설정에 따른 수동조그 방향 반전이 있으면 곱해줌 (없으면 dir = +1)
            int dir = 1;// (axis.Config?.ManualJogDir ?? +1); // +1 또는 -1

            double vel = rdoFine.Checked ? axis.Config.JogFineVelocity : axis.Config.JogCoarseVelocity;
            double acc = axis.Config.JogAcc;
            double dec = axis.Config.JogDec;
            double jerk = axis.Config.AccJerkPercent;

            double signedStep = stepUnit;// * jc.Sign * dir;

            // ① 상대이동 우선
            try
            {
                axis.MoveRel(signedStep, vel, acc, dec, jerk);
                return;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }

            // ② 절대이동 폴백
            {
                //double cur = axis.GetPosition();
                //try
                //{
                //    axis.MoveAbs(cur + signedStep, vel, acc, dec, jerk);
                //    return;
                //}
                //catch (Exception ex)
                //{
                //    Log.Write(ex);
                //}
            }

        }
        private void rdoContinuous_CheckedChanged(object sender, EventArgs e)
        {
            btnStep1.Enabled = rdoStep.Checked;
            btnStep10.Enabled = rdoStep.Checked;
            btnStep100.Enabled = rdoStep.Checked;
            btnStep1000.Enabled = rdoStep.Checked;
            btnStepClear.Enabled = rdoStep.Checked;
        }

        private void rdoStep_CheckedChanged(object sender, EventArgs e)
        {
            btnStep1.Enabled = rdoStep.Checked;
            btnStep10.Enabled = rdoStep.Checked;
            btnStep100.Enabled = rdoStep.Checked;
            btnStep1000.Enabled = rdoStep.Checked;
            btnStepClear.Enabled = rdoStep.Checked;
        }

        private void btnStep1_Click(object sender, EventArgs e)
        {
            nudStep.Value += 1;
        }

        private void btnStep10_Click(object sender, EventArgs e)
        {
            nudStep.Value += 0.1M;  // decimal
        }

        private void btnStep100_Click(object sender, EventArgs e)
        {
            nudStep.Value += 0.01M;
        }

        private void btnStep1000_Click(object sender, EventArgs e)
        {
            nudStep.Value += 0.001M;
        }

        private void btnStepClear_Click(object sender, EventArgs e)
        {
            // 요청: Clear 시 값을 0으로 설정 (범위를 벗어나지 않도록 클램프)
            decimal target = 0M;
            if (target < nudStep.Minimum) target = nudStep.Minimum;
            if (target > nudStep.Maximum) target = nudStep.Maximum;
            nudStep.Value = target;
        }

        private void UpdateUIByAxisSelection(string axisName)
        {
            bool hasAxis = !string.IsNullOrEmpty(axisName);
            if (axisName == "Index T Axis")
            {
                // CKD DD Motor
                btnXMinus.Enabled = btnXPlus.Enabled = false;
                btnYMinus.Enabled = btnYPlus.Enabled = false;
                btnZMinus.Enabled = btnZPlus.Enabled = false;
                btnTMinus.Enabled = btnTPlus.Enabled = false;
                btnPrevIndex.Enabled = btnNextIndex.Enabled = true;
            }
            else
            {
                btnXMinus.Enabled = btnXPlus.Enabled = hasAxis && HasAxisLetter(axisName, "X");
                btnYMinus.Enabled = btnYPlus.Enabled = hasAxis && HasAxisLetter(axisName, "Y");
                btnZMinus.Enabled = btnZPlus.Enabled = hasAxis && HasAxisLetter(axisName, "Z");
                btnTMinus.Enabled = btnTPlus.Enabled = hasAxis && HasAxisLetter(axisName, "T");
                btnPrevIndex.Enabled = btnNextIndex.Enabled = false;
            }
            btnStop.Enabled = hasAxis;
            lblPosition.Text = hasAxis ? "----" : "축 미선택";
        }

    }
}
