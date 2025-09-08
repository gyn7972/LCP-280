using QMC.Common;
using QMC.Common.Motion.Ajin;
using QMC.Common.Motions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Windows.Forms;
using QMC.LCP_280.Process; // AxisNames

namespace QMC.LCP_280.Process.Unit
{
    /// <summary>
    /// Equipment와 연동하여 Config 및 Recipe 관리
    /// </summary>
    public partial class Motion_Setup : Form
    {
        // AxisManager에서 사용하던 키 케이스에 맞춰 소문자 통일
        private const string UNIT_NAME = "unit";

        /// <summary>Equipment 인스턴스 참조</summary>
        private Equipment Equipment => Equipment.Instance;

        // 에디터 컬렉션(좌: configuration / 우: speed)
        //private PropertyCollection _editorPropertiesConfig;
        //private PropertyCollection _editorPropertiesSpeed;

        // 저장 시 빠른 조회용 인덱스: (section,title) → Property
        private Dictionary<(string section, string title), PropertyBase> _configIndex;
        private Dictionary<(string section, string title), PropertyBase> _speedIndex;

        public Motion_Setup()
        {
            InitializeComponent();
            SuspendLayout();

            _axisManager = Equipment.AxisManager;

            InitializeUI();

            ResumeLayout(true);
            Console.WriteLine("Motion_Setup 생성자 완료");
        }

        private void Motion_Setup_Load(object sender, EventArgs e)
        {
            // 필요시 폼 로드시 초기 바인딩 추가
        }

        /// <summary>향후 Unit 초기화가 필요하면 이곳에 작성</summary>
        private void InitializeUnit()
        {
        }

        // =========================
        // Save (설정 저장)
        // =========================
        private void btn_Save_Setup_Motion_Configuration_Click(object sender, EventArgs e)
        {
            try
            {
                // 1) 선택된 축 이름 확인
                string axisName = selectAxisListBoxItemsView?.SelectedItemName;
                if (string.IsNullOrWhiteSpace(axisName))
                {
                    MessageBox.Show("저장할 축을 먼저 선택하세요.", "알림",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 2) 축 객체 조회
                var axis = _axisManager?.Get(UNIT_NAME, axisName);
                if (axis == null)
                {
                    MessageBox.Show($"축 객체를 찾을 수 없습니다: {axisName}", "오류",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 3) 에디터 컬렉션 유효성
                if ((_editorPropertiesConfig == null || _editorPropertiesConfig.Count == 0) &&
                    (_editorPropertiesSpeed == null || _editorPropertiesSpeed.Count == 0))
                {
                    MessageBox.Show("저장할 항목이 없습니다.", "알림",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 4) 인덱스 빌드(한 번만 스캔 → O(1) 조회)
                _configIndex = BuildIndex(_editorPropertiesConfig);
                _speedIndex = BuildIndex(_editorPropertiesSpeed);

                // 5) 에디터 값 → axis.Setup 반영 (섹션 기준으로 안전 매핑)
                // --- Common
                axis.Setup.PulsesPerUnit = GetInt("Common", "Pulses Per Unit", axis.Setup.PulsesPerUnit);
                axis.Setup.AxisScale = GetInt("Common", "Axis Scale", axis.Setup.AxisScale);
                axis.Setup.AxisPowerPercent = GetInt("Common", "Axis Power", axis.Setup.AxisPowerPercent);

                // --- Config
                axis.Setup.PulseOutput = (PulseOutput)GetInt("Config", "Pulse Output", (int)axis.Setup.PulseOutput);
                axis.Setup.EncoderInput = (EncoderInput)GetInt("Config", "Enconder Input", (int)axis.Setup.EncoderInput);
                axis.Setup.InputSource = (InputSource)GetInt("Config", "Input Source", (int)axis.Setup.InputSource);
                axis.Setup.ZPhaseLevel = (ActiveLevel)GetInt("Config", "Z Phase Level", (int)axis.Setup.ZPhaseLevel);
                axis.Setup.ServoOnLevel = (ActiveLevel)GetInt("Config", "Servo On Level", (int)axis.Setup.ServoOnLevel);

                // --- Emergency Signal
                axis.Setup.EmergencyLevel = (ActiveLevel)GetInt("Emergency Signal", "Level", (int)axis.Setup.EmergencyLevel);
                axis.Setup.StopMode = (StopMode)GetInt("Emergency Signal", "Stop Mode", (int)axis.Setup.StopMode);

                // --- InPosition
                axis.Setup.InPosition = (InPosition)GetInt("InPosition", "Level", (int)axis.Setup.InPosition);
                axis.Setup.SoftwareLimitEnable = GetBool("InPosition", "Software", axis.Setup.SoftwareLimitEnable);
                axis.Setup.SoftwareLength = GetDouble("InPosition", "Software Length", axis.Setup.SoftwareLength);

                // --- Home
                axis.Setup.HomeSignalLevel = (ActiveLevel)GetInt("Home", "SignalLevel", (int)axis.Setup.HomeSignalLevel);
                axis.Setup.HomeMode = (HomeMode)GetInt("Home", "Mode", (int)axis.Setup.HomeMode);
                axis.Setup.HomeDirection = (HomeDirection)GetInt("Home", "Direction", (int)axis.Setup.HomeDirection);
                axis.Setup.HomeSignal = (HomeSignal)GetInt("Home", "Signal", (int)axis.Setup.HomeSignal);
                axis.Setup.HomeZPhase = (HomeZPhase)GetInt("Home", "Z Phase", (int)axis.Setup.HomeZPhase);
                axis.Setup.HomeClearTime = GetDouble("Home", "Clear Time(ms)", axis.Setup.HomeClearTime);
                axis.Setup.HomeOffset = GetDouble("Home", "Offset(mm)", axis.Setup.HomeOffset);

                // --- Alarm
                axis.Setup.AlarmResetLevel = (ActiveLevel)GetInt("Alarm", "Reset Signal", (int)axis.Setup.AlarmResetLevel);
                axis.Setup.AlarmLevel = (ActiveLevel)GetInt("Alarm", "Level", (int)axis.Setup.AlarmLevel);

                // --- Limit
                axis.Setup.PositiveLimitLevel = (ActiveLevel)GetInt("Limit", "+End Limit",(int)axis.Setup.PositiveLimitLevel);
                axis.Setup.NegativeLimitLevel = (ActiveLevel)GetInt("Limit", "-End Limit", (int)axis.Setup.NegativeLimitLevel);
                axis.Setup.SoftLimitMin = GetDouble("Limit", "Soft Limit -", axis.Setup.SoftLimitMin);
                axis.Setup.SoftLimitMax = GetDouble("Limit", "Soft Limit +", axis.Setup.SoftLimitMax);

                // 6) Speed(우측)도 있으면 반영
                if (_speedIndex?.Count > 0)
                {
                    // Home
                    axis.Config.HomeFirstSpeed = GetDoubleS("Home", "Vel. 1st(mm/s)", axis.Config.HomeFirstSpeed);
                    axis.Config.HomeSecondSpeed = GetDoubleS("Home", "Vel. 2nd(mm/s)", axis.Config.HomeSecondSpeed);
                    axis.Config.HomeThirdSpeed = GetDoubleS("Home", "Vel. 3rd(mm/s)", axis.Config.HomeThirdSpeed);
                    axis.Config.HomeLastSpeed = GetDoubleS("Home", "Vel. Last(mm/s)", axis.Config.HomeLastSpeed);
                    axis.Config.HomeFirstAcc = GetDoubleS("Home", "Accel. 1st(mm/s^2)", axis.Config.HomeFirstAcc);
                    axis.Config.HomeSecondAcc = GetDoubleS("Home", "Accel. 2nd(mm/s^2)", axis.Config.HomeSecondAcc);

                    // Jog
                    axis.Config.JogFineVelocity = GetDoubleS("Jog", "Fine Velocity(mm/s)", axis.Config.JogFineVelocity);
                    axis.Config.JogCoarseVelocity = GetDoubleS("Jog", "Coarse Velocity(mm/s)", axis.Config.JogCoarseVelocity);
                    axis.Config.JogAcc = GetDoubleS("Jog", "Accelerator(mm/s^2)", axis.Config.JogAcc);
                    axis.Config.JogDec = GetDoubleS("Jog", "Decelerator(mm/s^2)", axis.Config.JogDec);

                    // Run
                    axis.Config.MaxVelocity = GetDoubleS("Run", "Maximum Velocity(mm/s)", axis.Config.MaxVelocity);
                    axis.Config.RunAcc = GetDoubleS("Run", "Accelerator(mm/s^2)", axis.Config.RunAcc);
                    axis.Config.RunDec = GetDoubleS("Run", "Decelerator(mm/s^2)", axis.Config.RunDec);
                    axis.Config.ProfileMode = (ProfileMode)GetIntS("Run", "Profile", (int)axis.Config.ProfileMode);
                    axis.Config.AccJerkPercent = GetInt("Run", "Accelerator Jerk(%)", axis.Config.AccJerkPercent);
                    axis.Config.DecJerkPercent = GetInt("Run", "Decelerator Jerk(%)", axis.Config.DecJerkPercent);
                }

                // 7) 저장
                string axisRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "Axes", UNIT_NAME);
                Directory.CreateDirectory(axisRoot);
                string setupPath = Path.Combine(axisRoot, axisName + ".setup.json");
                axis.Setup.Save(setupPath);

                // 8) ★ 하드웨어에 설정 적용 ★
                int applyResult = axis.ApplyToDriver();
                if (applyResult != 0)
                {
                    MessageBox.Show($"설정 저장은 완료했으나 하드웨어 적용이 실패했습니다.\n오류코드: {applyResult}", "경고",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    Console.WriteLine($"축 {axisName}: 하드웨어 설정 적용 성공");
                }

                MessageBox.Show($"'{axisName}' 설정을 저장하고 하드웨어에 적용했습니다.", "완료",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"저장 중 오류: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_Save_Setup_Motion_Speed_Click(object sender, EventArgs e)
        {
            try
            {
                // 1) 선택된 축 이름 확인
                string axisName = selectAxisListBoxItemsView?.SelectedItemName;
                if (string.IsNullOrWhiteSpace(axisName))
                {
                    MessageBox.Show("저장할 축을 먼저 선택하세요.", "알림",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 2) 축 객체 조회
                var axis = _axisManager?.Get(UNIT_NAME, axisName);
                if (axis == null)
                {
                    MessageBox.Show($"축 객체를 찾을 수 없습니다: {axisName}", "오류",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 3) 에디터 컬렉션 유효성
                if ((_editorPropertiesSpeed == null || _editorPropertiesSpeed.Count == 0))
                {
                    MessageBox.Show("저장할 항목이 없습니다.", "알림",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 4) 인덱스 빌드(한 번만 스캔 → O(1) 조회)
                _speedIndex = BuildIndex(_editorPropertiesSpeed);

                // 6) Speed(우측)도 있으면 반영
                if (_speedIndex?.Count > 0)
                {
                    // Home
                    axis.Config.HomeFirstSpeed = GetDoubleS("Home", "Vel. 1st(mm/s)", axis.Config.HomeFirstSpeed);
                    axis.Config.HomeSecondSpeed = GetDoubleS("Home", "Vel. 2nd(mm/s)", axis.Config.HomeSecondSpeed);
                    axis.Config.HomeThirdSpeed = GetDoubleS("Home", "Vel. 3rd(mm/s)", axis.Config.HomeThirdSpeed);
                    axis.Config.HomeLastSpeed = GetDoubleS("Home", "Vel. Last(mm/s)", axis.Config.HomeLastSpeed);
                    axis.Config.HomeFirstAcc = GetDoubleS("Home", "Accel. 1st(mm/s^2)", axis.Config.HomeFirstAcc);
                    axis.Config.HomeSecondAcc = GetDoubleS("Home", "Accel. 2nd(mm/s^2)", axis.Config.HomeSecondAcc);

                    // Jog
                    axis.Config.JogFineVelocity = GetDoubleS("Jog", "Fine Velocity(mm/s)", axis.Config.JogFineVelocity);
                    axis.Config.JogCoarseVelocity = GetDoubleS("Jog", "Coarse Velocity(mm/s)", axis.Config.JogCoarseVelocity);
                    axis.Config.JogAcc = GetDoubleS("Jog", "Accelerator(mm/s^2)", axis.Config.JogAcc);
                    axis.Config.JogDec = GetDoubleS("Jog", "Decelerator(mm/s^2)", axis.Config.JogDec);

                    // Run
                    axis.Config.MaxVelocity = GetDoubleS("Run", "Maximum Velocity(mm/s)", axis.Config.MaxVelocity);
                    axis.Config.RunAcc = GetDoubleS("Run", "Accelerator(mm/s^2)", axis.Config.RunAcc);
                    axis.Config.RunDec = GetDoubleS("Run", "Decelerator(mm/s^2)", axis.Config.RunDec);
                    axis.Config.ProfileMode = (ProfileMode)GetIntS("Run", "Profile", (int)axis.Config.ProfileMode);
                    axis.Config.AccJerkPercent = GetInt("Run", "Accelerator Jerk(%)", axis.Config.AccJerkPercent);
                    axis.Config.DecJerkPercent = GetInt("Run", "Decelerator Jerk(%)", axis.Config.DecJerkPercent);
                }

                // 7) 저장
                string axisRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "Axes", UNIT_NAME);
                Directory.CreateDirectory(axisRoot);
                string setupPath = Path.Combine(axisRoot, axisName + ".config.json");
                axis.Config.Save(setupPath);

                // 8) ★ 하드웨어에 설정 적용 ★
                int applyResult = axis.ApplyToDriver();
                if (applyResult != 0)
                {
                    MessageBox.Show($"설정 저장은 완료했으나 하드웨어 적용이 실패했습니다.\n오류코드: {applyResult}", "경고",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    Console.WriteLine($"축 {axisName}: 하드웨어 속도 설정 적용 성공");
                }

                MessageBox.Show($"'{axisName}' 속도 설정을 저장하고 하드웨어에 적용했습니다.", "완료",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"저장 중 오류: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnServoOff_Click(object sender, EventArgs e)
        {
            try
            {
                // 1) 선택된 축 이름 확인
                string axisName = selectAxisListBoxItemsView?.SelectedItemName;
                if (string.IsNullOrWhiteSpace(axisName))
                {
                    MessageBox.Show("저장할 축을 먼저 선택하세요.", "알림",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 2) 축 객체 조회
                var axis = _axisManager?.Get(UNIT_NAME, axisName);
                if (axis == null)
                {
                    MessageBox.Show($"축 객체를 찾을 수 없습니다: {axisName}", "오류",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                int ret = axis.Servo(false);
                if (ret != 0)
                {
                    MessageBox.Show($"서보 OFF 명령이 실패했습니다. 오류코드: {ret}", "오류",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"서보 OFF 오류: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnServoOn_Click(object sender, EventArgs e)
        {
            try
            {
                // 1) 선택된 축 이름 확인
                string axisName = selectAxisListBoxItemsView?.SelectedItemName;
                if (string.IsNullOrWhiteSpace(axisName))
                {
                    MessageBox.Show("저장할 축을 먼저 선택하세요.", "알림",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 2) 축 객체 조회
                var axis = _axisManager?.Get(UNIT_NAME, axisName);
                if (axis == null)
                {
                    MessageBox.Show($"축 객체를 찾을 수 없습니다: {axisName}", "오류",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                int ret = axis.Servo(true);
                if (ret != 0)
                {
                    MessageBox.Show($"서보 ON 명령이 실패했습니다. 오류코드: {ret}", "오류",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"서보 ON 오류: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // --- 특정 축(Home 실행 전) 전용 인터락 Rule 동적 등록 Helper ---
        // InputStage X 축 홈 조건 예시:
        //  - DOWN 센서 (모듈: "InputStage", 표시번호: "X105") MUST ON
        //  - UP   센서 (모듈: "InputStage", 표시번호: "X104") MUST OFF
        // 필요 시 모듈/디스플레이 번호를 실제 IO 명세에 맞게 수정.
        private void EnsureAxisHomeInterlocks(MotionAxis axis)
        {
            if (axis == null) return;

            try
            {
                var il = QMC.LCP_280.Process.Component.InterlockManager.Instance;
                il.Start(); // 이미 시작되어 있으면 내부에서 무시

                // WaferStage X 축 홈 인터락 조건 등록 예시
                // TODO: 모듈명("WaferStageIO") / 표시번호("X201","X202") 는 실제 IO 맵에 맞게 수정하세요.
                //  - Vacuum OFF (센서가 1=ON 이라면 expected:false 로 설정하여 OFF 요구)
                //  - Clamp Open (Open 센서 ON 필요)
                if (axis.Name.Equals(AxisNames.WaferStageX, StringComparison.OrdinalIgnoreCase))
                {
                    var rules = il.GetRules();
                    bool Has(string n) => rules.Any(r => r.Name.Equals(n, StringComparison.OrdinalIgnoreCase));

                    if (!Has("WaferStageX_Home_VacuumOff"))
                        il.AddAxisIoRequire("WaferStageX_Home_VacuumOff", axis, "WaferStageIO", "X201", false); // Vacuum 센서 예상 OFF
                    if (!Has("WaferStageX_Home_ClampOpen"))
                        il.AddAxisIoRequire("WaferStageX_Home_ClampOpen", axis, "WaferStageIO", "X202", true);  // Clamp Open 센서 ON
                }

                // 축 이름 매칭 (프로젝트 실제 축 이름에 맞게 수정 필요)
                if (axis.Name.Equals(AxisNames.WaferStageX, StringComparison.OrdinalIgnoreCase))
                {
                    var rules = il.GetRules();
                    bool Has(string n) => rules.Any(r => r.Name.Equals(n, StringComparison.OrdinalIgnoreCase));

                    if (!Has("InputStageX_Home_DownMustOn"))
                        il.AddAxisIoRequire("InputStageX_Home_DownMustOn", axis, "InputStage", "X105", true);
                    if (!Has("InputStageX_Home_UpMustOff"))
                        il.AddAxisIoRequire("InputStageX_Home_UpMustOff", axis, "InputStage", "X104", false);
                }
            }
            catch { /* ignore registration errors to avoid UI block */ }
        }

        private void btnHome_Click(object sender, EventArgs e)
        {
            try
            {
                string axisName = selectAxisListBoxItemsView?.SelectedItemName;
                if (string.IsNullOrWhiteSpace(axisName))
                {
                    MessageBox.Show("저장할 축을 먼저 선택하세요.", "알림",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var axis = _axisManager?.Get(UNIT_NAME, axisName);
                if (axis == null)
                {
                    MessageBox.Show($"축 객체를 찾을 수 없습니다: {axisName}", "오류",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // (1) 축 전용 Home 조건 Rule 동적 등록 (중복 등록 방지 내부 처리)
                EnsureAxisHomeInterlocks(axis);

                // (2) === Interlock Check (Home 전) ===
                string reason;
                if (!QMC.LCP_280.Process.Component.InterlockManager.Instance.ValidateAxisForHome(axis, out reason))
                {
                    MessageBox.Show($"홈 인터락 차단:\r\n{reason}", "Interlock",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int ret = axis.HomeAsync();
                if (ret != 0)
                {
                    MessageBox.Show($"홈 이동 명령이 실패했습니다. 오류코드: {ret}", "오류",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"원점 구동 오류: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // =========================
        // 인덱스 빌드 & 조회 유틸
        // =========================

        /// <summary>
        /// TitleOnlyProperty로 섹션을 추적하면서 (section,title) 키로 인덱스 생성
        /// </summary>
        private static Dictionary<(string section, string title), PropertyBase> BuildIndex(PropertyCollection pc)
        {
            var map = new Dictionary<(string section, string title), PropertyBase>(StringTupleComparer.OrdinalIgnoreCase);
            if (pc == null || pc.Count == 0) return map;

            string currentSection = string.Empty;
            foreach (var p in pc)
            {
                if (p == null) continue;

                // 섹션 헤더(TitleOnlyProperty)는 섹션명으로만 사용
                if (p is TitleOnlyProperty)
                {
                    currentSection = GetName(p) ?? string.Empty;
                    continue;
                }

                var title = GetName(p);
                if (string.IsNullOrEmpty(title)) continue;

                var key = (currentSection, title);
                // 뒤에 같은 타이틀이 있어도 최초 1개를 신뢰(중복 방지)
                if (!map.ContainsKey(key))
                    map[key] = p;
            }
            return map;
        }

        private PropertyBase Find(string section, string title)
        {
            if (_configIndex != null && _configIndex.TryGetValue((section ?? string.Empty, title), out var p1))
                return p1;
            return null;
        }

        private PropertyBase FindS(string section, string title)
        {
            if (_speedIndex != null && _speedIndex.TryGetValue((section ?? string.Empty, title), out var p1))
                return p1;
            return null;
        }

        private double GetDouble(string section, string title, double fallback)
            => ReadDouble(Find(section, title), fallback);

        private double GetDoubleS(string section, string title, double fallback)
            => ReadDouble(FindS(section, title), fallback);

        private bool GetBool(string section, string title, bool fallback)
            => ReadBool(Find(section, title), fallback);

        private int GetInt(string section, string title, int fallback)
            => ReadInt(Find(section, title), fallback);

        private int GetIntS(string section, string title, int fallback)
            => ReadInt(FindS(section, title), fallback);

        private static double ReadDouble(PropertyBase p, double fallback)
        {
            if (p == null) return fallback;

            if (p is DoubleProperty dp) return dp.Value;
            if (p is BoolProperty bp) return bp.Value ? 1.0 : 0.0;

            var vProp = p.GetType().GetProperty("Value");
            if (vProp != null)
            {
                var v = vProp.GetValue(p);
                if (v is IConvertible)
                {
                    try { return Convert.ToDouble(v, CultureInfo.InvariantCulture); }
                    catch { }
                }
            }
            return fallback;
        }

        private static bool ReadBool(PropertyBase p, bool fallback)
        {
            if (p == null) return fallback;

            if (p is BoolProperty bp) return bp.Value;

            var vProp = p.GetType().GetProperty("Value");
            if (vProp != null)
            {
                var v = vProp.GetValue(p);
                if (v is bool b) return b;
                if (v is IConvertible)
                {
                    try { return Convert.ToDouble(v, CultureInfo.InvariantCulture) != 0.0; }
                    catch { }
                }
            }
            return fallback;
        }

        private static int ReadInt(PropertyBase p, int fallback)
        {
            if (p == null) return fallback;

            switch (p)
            {
                case IntProperty ip: return ip.Value;
                case LongProperty lp: try { checked { return (int)lp.Value; } } catch { return fallback; }
                case FloatProperty fp: return (int)Math.Round(fp.Value);
                case DoubleProperty dp: return (int)Math.Round(dp.Value);
                case BoolProperty bp: return bp.Value ? 1 : 0;
                case StringProperty sp:
                    if (int.TryParse(sp.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var i)) return i;
                    if (double.TryParse(sp.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)) return (int)Math.Round(d);
                    return fallback;
            }

            var vProp = p.GetType().GetProperty("Value");
            if (vProp != null)
            {
                var v = vProp.GetValue(p);
                if (v is int i) return i;
                if (v is long l) { try { checked { return (int)l; } } catch { return fallback; } }
                if (v is float f) return (int)Math.Round(f);
                if (v is double d) return (int)Math.Round(d);
                if (v is bool b) return b ? 1 : 0;
                if (v is string s)
                {
                    if (int.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var i2)) return i2;
                    if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var d2)) return (int)Math.Round(d2);
                }
                if (v is IConvertible)
                {
                    try { return Convert.ToInt32(v, CultureInfo.InvariantCulture); } catch { }
                }
            }
            return fallback;
        }

        private static string GetName(PropertyBase p)
        {
            if (p == null) return null;

            // Name 우선, 없으면 Title 시도
            var nameProp = p.GetType().GetProperty("Name");
            var titleProp = p.GetType().GetProperty("Title");
            var key = nameProp?.GetValue(p)?.ToString() ?? titleProp?.GetValue(p)?.ToString();
            return key;
        }

        // 섹션/타이틀 키의 대소문자 무시 튜플 비교자
        private sealed class StringTupleComparer : IEqualityComparer<(string section, string title)>
        {
            public static readonly StringTupleComparer OrdinalIgnoreCase = new StringTupleComparer();
            private readonly StringComparer _cmp = StringComparer.OrdinalIgnoreCase;

            public bool Equals((string section, string title) x, (string section, string title) y)
                => _cmp.Equals(x.section, y.section) && _cmp.Equals(x.title, y.title);

            public int GetHashCode((string section, string title) obj)
                => HashCode.Combine(_cmp.GetHashCode(obj.section ?? string.Empty), _cmp.GetHashCode(obj.title ?? string.Empty));
        }
        /// <summary>
        /// FormConfig 탭 호스트가 전달하는 가용 크기에 맞춰 자동으로 폼 크기를 조정합니다.
        /// </summary>
        /// <param name="width">가용 너비</param>
        /// <param name="height">가용 높이 (탭 헤더 제외)</param>
        public void SetPanelSize(int width, int height)
        {
            try
            {
                this.SuspendLayout();

                // 호스트(TabPage)의 클라이언트 영역을 그대로 사용
                this.Size = new Size(width, height);
                this.ClientSize = new Size(width, height);

                // 포함된 컨트롤들이 Dock=Fill 등으로 배치되었다면 자동으로 맞춰짐
                // 필요 시 내부 루트 컨테이너가 있다면 여기에서 Size/Dock 조정 가능

                this.Invalidate();
                this.Update();
            }
            finally
            {
                this.ResumeLayout(true);
            }

            Console.WriteLine($"📐 {nameof(InputCassetteLifterUnit_Config)}.SetPanelSize → {width}x{height}");
        }
    }
}
