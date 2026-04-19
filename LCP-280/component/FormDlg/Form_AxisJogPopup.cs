using QMC.Common;
using QMC.Common.DIO;
using QMC.Common.Motions;
using QMC.Common.Motions.CKD;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Unit
{
    public partial class Form_AxisJogPopup : Form
    {
        private const string UNIT_NAME = "unit";   // Motion_Setup 과 동일 키 사용
        private Equipment EquipmentInst { get { return Equipment.Instance; } }
        private MotionAxisManager _axisManager;
        private Timer _posTimer;

        // [ADD] 표시명(UI) -> 실제 축 이름(MotionAxis.Name) 매핑
        private readonly Dictionary<string, string> _axisDisplayToAxisName
            = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private string GetSelectedAxisName()
        {
            var displayName = selectAxisListBoxItemsView?.SelectedItemName;
            if (string.IsNullOrWhiteSpace(displayName))
                return null;

            string axisName;
            return _axisDisplayToAxisName.TryGetValue(displayName, out axisName) ? axisName : displayName;
        }

        // [ADD] Home(축 초기화) 전에는 모든 모션을 차단하는 공통 Guard
        private bool EnsureAxisReadyOrShowMessage(string actionName)
        {
            try
            {
                var eq = EquipmentInst;
                if (eq == null)
                {
                    var mb = new MessageBoxOk();
                    mb.ShowDialog("Error", "Equipment 인스턴스를 찾을 수 없습니다.");
                    return false;
                }

                bool bRet = eq.EnsureAxisReadyForAutoOrMove(actionName);
                return bRet;
            }
            catch (Exception ex)
            {
                var mb = new MessageBoxOk();
                mb.ShowDialog("초기화 필요", ex.Message);
                return false;
            }
        }

        public Form_AxisJogPopup()
        {
            InitializeComponent();

            // 디자이너에서 열릴 때는 장비 접근 금지
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) 
                return;

            // 작업표시줄에 아이콘 따로 표시
            this.ShowInTaskbar = true;
            // 독립 창으로 보이게 (메인폼에 종속 X)
            this.TopLevel = true;
            this.Owner = null;
            // 필요에 따라 창 스타일
            this.StartPosition = FormStartPosition.CenterScreen;

            _axisManager = EquipmentInst != null ? EquipmentInst.AxisManager : null;

            // Jog Panel
            this.Text = "JogPanel";   // ← 작업표시줄 툴팁에 보이는 문자열
            //this.Icon = Properties.Resources.JogPanel_ico;  // 내장된 아이콘 로드
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

                rdoFine.Checked = true;
                rdoStep.Checked = true;
            }
            catch (Exception ex)
            {
                Log.Write("LCP-280", "Form_AxisJogPopup.InitializeUI error: " + ex);
            }
        }

        // ===== 축 목록 바인딩: AllInOrder 순서 + DisplayNames 표시 =====
        private void BindAxisList()
        {
            try
            {
                _axisDisplayToAxisName.Clear();

                if (_axisManager == null)
                {
                    var mb = new MessageBoxOk();
                    mb.ShowDialog("Notification!", "AxisManager가 초기화되지 않았습니다.");

                    selectAxisListBoxItemsView.SetItems();
                    return;
                }

                // 실제 장비에 등록된 축 이름들(실제 MotionAxis.Name)
                var axisNames = _axisManager.GetAxisNames(UNIT_NAME) ?? new string[0];

                if (axisNames.Length == 0)
                {
                    selectAxisListBoxItemsView.SetItems();
                    return;
                }

                var exists = new HashSet<string>(axisNames, StringComparer.OrdinalIgnoreCase);
                var displayList = new List<string>();

                // 1) AllInOrder 순서대로 추가
                foreach (var axisName in AxisNames.AllInOrder)
                {
                    if (!exists.Contains(axisName))
                        continue;

                    var display = AxisNames.GetDisplayName(axisName);

                    // 표시명 충돌 방지 (동일 display가 있으면 실제명 붙여 유니크 처리)
                    if (_axisDisplayToAxisName.ContainsKey(display))
                        display = display + " [" + axisName + "]";

                    _axisDisplayToAxisName[display] = axisName;
                    displayList.Add(display);
                }

                // 2) AllInOrder에 없는 축은 뒤에 추가(이름순)
                var remaining = axisNames
                    .Where(n => !AxisNames.AllInOrder.Any(o => o.Equals(n, StringComparison.OrdinalIgnoreCase)))
                    .OrderBy(n => n, StringComparer.OrdinalIgnoreCase);

                foreach (var axisName in remaining)
                {
                    var display = AxisNames.GetDisplayName(axisName);
                    if (_axisDisplayToAxisName.ContainsKey(display))
                        display = display + " [" + axisName + "]";

                    _axisDisplayToAxisName[display] = axisName;
                    displayList.Add(display);
                }

                selectAxisListBoxItemsView.SetItems(displayList.ToArray());
                if (displayList.Count > 0)
                    selectAxisListBoxItemsView.SelectedIndex = 0;
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
                string axisName = GetSelectedAxisName(); // 실제 축 이름
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
            if (axisName != null && axisName.Equals(AxisNames.IndexT, StringComparison.OrdinalIgnoreCase))
            {
                btnXMinus.Enabled = btnXPlus.Enabled = false;
                btnYMinus.Enabled = btnYPlus.Enabled = false;
                btnZMinus.Enabled = btnZPlus.Enabled = false;
                btnTMinus.Enabled = btnTPlus.Enabled = false;
                btnPrevIndex.Enabled = btnNextIndex.Enabled = true;
                btnStop.Enabled = true;
                return;
            }

            bool x = HasAxisLetter(axisName, "X");
            bool y = HasAxisLetter(axisName, "Y");
            bool z = HasAxisLetter(axisName, "Z");
            bool t = HasAxisLetter(axisName, "T");

            btnXMinus.Enabled = btnXPlus.Enabled = x;
            btnYMinus.Enabled = btnYPlus.Enabled = y;
            btnZMinus.Enabled = btnZPlus.Enabled = z;
            btnTMinus.Enabled = btnTPlus.Enabled = t;
            btnPrevIndex.Enabled = btnNextIndex.Enabled = false;
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
            string axisName = GetSelectedAxisName(); // 실제 축 이름
            if (string.IsNullOrEmpty(axisName)) return;

            var axis = _axisManager.Get(UNIT_NAME, axisName);
            if (axis == null || axis.Status == null || axis.Status.PV == null) return;

            double pos = axis.Status.PV.ActualPosition;
            lblPosition.Text = pos.ToString("0.000", CultureInfo.InvariantCulture) + " mm";
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

        public void SetText(string value)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new System.Action(() =>
                    {
                        this.SetText(value);
                    }));
                }
                else
                {
                    lblPosition.Text = value;
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

        private void JogButton_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                if (!EnsureAxisReadyOrShowMessage("Jog")) return;

                if (_axisManager == null) return;
                //string axisName = selectAxisListBoxItemsView.SelectedItemName;
                string axisName = GetSelectedAxisName(); // 실제 축 이름
                if (string.IsNullOrEmpty(axisName)) return;

                MotionAxis axis = _axisManager.Get(UNIT_NAME, axisName);
                if (axis == null) return;

                JogCommand jc = ParseJogButton(sender);

                if (!CheckSafeToDrive(axis, jc))
                {
                    var mb = new MessageBoxOk();
                    mb.ShowDialog("Interlock!", "현재 상태에서 해당 축을 구동할 수 없습니다.");
                    return;
                }

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
            catch (Exception ex) { Log.Write(ex); }
        }

        private void JogButton_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                if (_axisManager == null) return;
                string axisName = GetSelectedAxisName(); // 실제 축 이름
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
                if (!EnsureAxisReadyOrShowMessage("Jog.IndexPrev")) 
                    return;

                // Rotary 유닛이 있으면 우선 사용(인터락 포함)
                Rotary rotary = null;
                if (EquipmentInst?.Units != null && EquipmentInst.Units.TryGetValue("Rotary", out var u))
                    rotary = u as Rotary;

                if (rotary != null)
                {
                    string reason;
                    if (!rotary.TryMoveIndexPrev(out reason) && !string.IsNullOrEmpty(reason))
                    {
                        //var mb = new MessageBoxOk();
                        //mb.ShowDialog("Interlock!", reason);
                    }

                    if(rotary.WaitIndexMoveDone() != 0)
                    {
                        var mb = new MessageBoxOk();
                        mb.ShowDialog("Move Fale!", reason);
                    }

                    return;
                }

                // 폴백: 기존 직접 축 호출
                if (_axisManager == null) return;
                string axisName = GetSelectedAxisName(); // 실제 축 이름
                if (string.IsNullOrEmpty(axisName)) 
                    return;

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
                if (!EnsureAxisReadyOrShowMessage("Jog.IndexNext")) return;

                // Rotary 유닛이 있으면 우선 사용(인터락 포함)
                Rotary rotary = null;
                if (EquipmentInst?.Units != null && EquipmentInst.Units.TryGetValue("Rotary", out var u))
                    rotary = u as Rotary;

                if (rotary != null)
                {
                    string reason;
                    if (!rotary.TryMoveIndexNext(out reason) && !string.IsNullOrEmpty(reason))
                    {
                        //var mb = new MessageBoxOk();
                        //mb.ShowDialog("Interlock!", reason);
                    }
                    else
                    {
                        btnNextIndex.Enabled = btnPrevIndex.Enabled = false;
                        // LoadIndexChanged 이벤트로 재활성화
                        rotary.LoadIndexChanged -= Rotary_LoadIndexChanged_ForJog;
                        rotary.LoadIndexChanged += Rotary_LoadIndexChanged_ForJog;
                    }

                    if (rotary.WaitIndexMoveDone() != 0)
                    {
                        var mb = new MessageBoxOk();
                        mb.ShowDialog("Move Fale!", reason);
                    }

                    return;
                }

                // 폴백: 기존 직접 축 호출
                if (_axisManager == null) return;
                string axisName = GetSelectedAxisName(); // 실제 축 이름
                if (string.IsNullOrEmpty(axisName))
                    return;

                MotionAxis axis = _axisManager.Get(UNIT_NAME, axisName);
                if (axis == null)
                    return;

                axis.MoveNextIndex();
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

        private void Rotary_LoadIndexChanged_ForJog(object sender, int idx)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<object, int>(Rotary_LoadIndexChanged_ForJog), sender, idx);
                return;
            }
            btnNextIndex.Enabled = btnPrevIndex.Enabled = true;
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            try
            {
                if (_axisManager == null) return;
                string axisName = GetSelectedAxisName(); // 실제 축 이름
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
                            IndexChipProbeController probeControl = EquipmentInst.GetUnit(Equipment.UnitKeys.IndexChipProber) as IndexChipProbeController;

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
                            IndexChipProbeController probeControl = EquipmentInst.GetUnit(Equipment.UnitKeys.IndexChipProber) as IndexChipProbeController;

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
            double vel = rdoFine.Checked ? axis.Config.JogFineVelocity : axis.Config.JogCoarseVelocity;
            double acc = axis.Config.JogAcc;
            double dec = axis.Config.JogDec;
            double jerk = axis.Config.AccJerkPercent;

            double signedStep = stepUnit;// * jc.Sign * dir;

            try
            {
                axis.MoveRel(signedStep, vel, acc, dec, jerk);
                return;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

        private void UpdateUIByAxisSelection(string axisName)
        {
            bool hasAxis = !string.IsNullOrEmpty(axisName);
            if (axisName != null && axisName.Equals(AxisNames.IndexT, StringComparison.OrdinalIgnoreCase))
            {
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

        private void btnStep1_Click(object sender, EventArgs e)
        {
            nudStep.Value = 1;
        }

        private void btnStep10_Click(object sender, EventArgs e)
        {
            nudStep.Value = 0.1M;  // decimal
        }

        private void btnStep100_Click(object sender, EventArgs e)
        {
            nudStep.Value = 0.01M;
        }

        private void btnStep1000_Click(object sender, EventArgs e)
        {
            nudStep.Value = 0.001M;
        }

        private void btnStepClear_Click(object sender, EventArgs e)
        {
            decimal target = 0M;
            if (target < nudStep.Minimum) target = nudStep.Minimum;
            if (target > nudStep.Maximum) target = nudStep.Maximum;
            nudStep.Value = target;
        }
    }
}
