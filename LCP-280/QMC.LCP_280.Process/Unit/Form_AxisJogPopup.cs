using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows.Forms;
using QMC.Common;
using QMC.Common.Motions;

namespace QMC.LCP_280.Process.Unit
{
    public partial class Form_AxisJogPopup : Form
    {
        private const string UNIT_NAME = "unit";   // Motion_Setup 과 동일 키 사용
        private Equipment EquipmentInst { get { return Equipment.Instance; } }
        private MotionAxisManager _axisManager;
        private Timer _posTimer;

        public Form_AxisJogPopup()
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
            }
            catch (Exception ex)
            {
                Log.Write("LCP-280", "Form_AxisJogPopup.InitializeUI error: " + ex);
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

                // Motion_Setup 과 동일 흐름
                var axisNames = _axisManager.GetAxisNames(UNIT_NAME) ?? new string[0]; // :contentReference[oaicite:3]{index=3}
                if (axisNames.Length > 0)
                    selectAxisListBoxItemsView.SetItems(axisNames);
                else
                    selectAxisListBoxItemsView.SetItems();
            }
            catch (Exception ex)
            {
                Log.Write("LCP-280", "BindAxisList error: " + ex);
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

            btnStop.Click += btnStop_Click;
        }

        // ===== 모드/축에 따른 UI 스위치 =====
        private void MoveModeChanged(object sender, EventArgs e) { ApplyMoveModeUI(); }

        private void ApplyMoveModeUI()
        {
            bool step = rdoStep.Checked;
            nudStep.Enabled = step;
            btnStepPreset.Enabled = step;
        }

        private void UpdateJogEnableByAxisName(string axisName)
        {
            bool x = HasAxisLetter(axisName, "X");
            bool y = HasAxisLetter(axisName, "Y");
            bool z = HasAxisLetter(axisName, "Z");
            bool t = HasAxisLetter(axisName, "T");

            btnXMinus.Enabled = btnXPlus.Enabled = x;
            btnYMinus.Enabled = btnYPlus.Enabled = y;
            btnZMinus.Enabled = btnZPlus.Enabled = z;
            btnTMinus.Enabled = btnTPlus.Enabled = t;
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
            if (string.IsNullOrEmpty(axisName)) return;

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

                if (rdoContinuous.Checked)
                {
                    double vel = rdoFine.Checked ? axis.Config.JogFineVelocity : axis.Config.JogCoarseVelocity; // :contentReference[oaicite:5]{index=5}
                    StartJogContinuous(axis, jc, vel);
                }
                else
                {
                    double step = (double)nudStep.Value * jc.Sign;
                    DoStepMove(axis, jc, step);
                }
            }
            catch (Exception ex)
            {
                Log.Write("LCP-280", "JogButton_MouseDown error: " + ex);
            }
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

                StopJog(axis);
            }
            catch { }
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
            catch { }
        }

        // ===== 드라이버 메서드 안전 호출 (이름 차이 흡수) =====
        private static bool TryCall(object target, string method, params object[] args)
        {
            if (target == null) return false;
            MethodInfo mi = target.GetType().GetMethod(method, BindingFlags.Public | BindingFlags.Instance);
            if (mi == null) return false;
            mi.Invoke(target, args);
            return true;
        }

        private void StartJogContinuous(MotionAxis axis, JogCommand jc, double velocity)
        {
            string letter = jc.Axis.ToString(); // "X","Y","Z","T"
            if (TryCall(axis, "StartJog", new object[] { letter, jc.Sign, velocity })) return;
            if (TryCall(axis, "JogStart", new object[] { letter, jc.Sign, velocity })) return;
            if (TryCall(axis, "Jog", new object[] { letter, jc.Sign, velocity })) return;

            // 연속조그 API가 없으면 스텝 이동으로 대체
            DoStepMove(axis, jc, Math.Max(velocity * 0.05, 0.001) * jc.Sign);
        }

        private void StopJog(MotionAxis axis)
        {
            if (TryCall(axis, "StopJog")) return;
            if (TryCall(axis, "JogStop")) return;
            if (TryCall(axis, "Stop")) return;
        }

        private void DoStepMove(MotionAxis axis, JogCommand jc, double stepMm)
        {
            string letter = jc.Axis.ToString();
            double vel = rdoFine.Checked ? axis.Config.JogFineVelocity : axis.Config.JogCoarseVelocity; // :contentReference[oaicite:6]{index=6}
            double acc = axis.Config.JogAcc;
            double dec = axis.Config.JogDec;

            if (TryCall(axis, "MoveRelative", new object[] { letter, stepMm, vel, acc, dec })) return;
            if (TryCall(axis, "RelMove", new object[] { letter, stepMm, vel, acc, dec })) return;
            if (TryCall(axis, "MoveRel", new object[] { letter, stepMm, vel, acc, dec })) return;
        }
    }
}
