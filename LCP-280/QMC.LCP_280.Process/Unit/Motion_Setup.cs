using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using QMC.Common;
using QMC.Common.Motions;

namespace QMC.LCP_280.Process.Unit
{
    /// <summary>
    /// CassetteLoadingElevator Unit의 Config 폼
    /// Equipment와 연동하여 Config 및 Recipe 관리
    /// </summary>
    public partial class Motion_Setup : Form
    {
        private const string UNIT_NAME = "CassetteLoadingElevator";
        
        /// <summary>
        /// Equipment 인스턴스 참조
        /// </summary>
        private Equipment Equipment => Equipment.Instance;

        /// <summary>
        /// 해당 Unit 인스턴스
        public Motion_Setup()
        {
            InitializeComponent();
            // 폼 로딩 중에는 화면 업데이트 중단
            this.SuspendLayout();

            _axisManager = Equipment.Instance.AxisManager;

            InitializeUI();

            // 모든 초기화가 완료된 후 화면 업데이트 재개
            this.ResumeLayout(true);
            
            Console.WriteLine($"✅ CassetteLoadingElevatorUnit_Config 생성자 완료");
        }

        private void Motion_Setup_Load(object sender, EventArgs e)
        {
            
        }

        ///// <summary>
        ///// Unit 초기화 및 Equipment에서 Unit 인스턴스 가져오기
        ///// </summary>
        private void InitializeUnit()
        {

        }

        private void btn_Save_Setup_Motion_Configuration_Click(object sender, EventArgs e)
        {
            try
            {
                const string UNIT_NAME = "unit";
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
                if (_editorPropertiesConfig == null || _editorPropertiesConfig.Count == 0)
                {
                    MessageBox.Show("저장할 항목이 없습니다.", "알림",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 4) 에디터 값 → axis.Setup에 반영
                axis.Setup.AxisScale = GetDouble("Axis Scale", axis.Setup.AxisScale);
                axis.Setup.AxisPowerPercent = GetInt("Axis Power", axis.Setup.AxisPowerPercent);

                axis.Setup.OutputMode = (OutputMode)GetInt("Output Mode", (int)axis.Setup.OutputMode);
                axis.Setup.InputMode = (InputMode)GetInt("Input Mode", (int)axis.Setup.InputMode);
                axis.Setup.InputSource = (InputSource)GetInt("Input Source", (int)axis.Setup.InputSource);
                axis.Setup.ZPhaseLevel = (Common.Motions.ActiveLevel)GetInt("Z Phase Level", (int)axis.Setup.ZPhaseLevel);
                axis.Setup.ServoLevel = (Common.Motions.ActiveLevel)GetInt("Servo Level", (int)axis.Setup.ServoLevel);

                axis.Setup.EmergencyLevel = (Common.Motions.ActiveLevel)GetInt("Level", (int)axis.Setup.EmergencyLevel);
                axis.Setup.StopMode = (StopMode)GetInt("Stop Mode", (int)axis.Setup.StopMode);

                axis.Setup.InpositionLevel = (Common.Motions.ActiveLevel)GetInt("Level", (int)axis.Setup.InpositionLevel);
                axis.Setup.SoftwareLimitEnable = GetBool("Software", axis.Setup.SoftwareLimitEnable);
                axis.Setup.SoftwareLength = GetDouble("Software Length", axis.Setup.SoftwareLength);

                axis.Setup.HomeSignalLevel = (Common.Motions.ActiveLevel)GetInt("Signal", (int)axis.Setup.HomeSignalLevel);
                axis.Setup.HomeMode = (HomeMode)GetInt("Mode", (int)axis.Setup.HomeMode);

                axis.Setup.AlarmResetSignal = (Common.Motions.ActiveLevel)GetInt("Reset Signal", (int)axis.Setup.AlarmResetSignal);
                axis.Setup.AlarmLevel = (Common.Motions.ActiveLevel)GetInt("Level", (int)axis.Setup.AlarmLevel);

                axis.Setup.SoftLimitMin = GetDouble("Soft Limit -", axis.Setup.SoftLimitMin);
                axis.Setup.SoftLimitMax = GetDouble("Soft Limit +", axis.Setup.SoftLimitMax);

                // 5) 영속화 (Axis 하나만 저장)
                // ⚠️ 프로젝트마다 다름: 아래 중 자신 프로젝트에 맞는 저장 루틴 호출
                // 예시 A) AxisManager가 축 단위 저장을 제공하는 경우
                //_axisManager.SaveAxisSetup(UNIT_NAME, axisName, axis.Setup);

                // 예시 B) Axis 객체에 자체 Save가 있는 경우
                string axisRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "Axes", UNIT_NAME);
                var setupPath = Path.Combine(axisRoot, axisName + ".setup.json");
                axis.Setup.Save(setupPath);

                // 예시 C) 공통 ConfigManager를 통해 저장하는 경우
                // ConfigManager.SaveAxisSetup(UNIT_NAME, axisName, axis.Setup);

                MessageBox.Show($"'{axisName}' 설정을 저장했습니다.", "완료",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"저장 중 오류: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private PropertyBase FindByTitle(string title)
        {
            if (_editorPropertiesConfig == null) return null;
            // PropertyBase에 Title 또는 Name 같은 식별 프로퍼티가 있다고 가정
            // 없으면 PropertyBase에 Title을 추가하거나, PropertyCollectionView에서 키/맵을 제공하도록 확장 필요.
            return _editorPropertiesConfig.FirstOrDefault(p =>
            {
                try
                {
                    var tProp = p.GetType().GetProperty("Title") ?? p.GetType().GetProperty("Name");
                    var key = tProp?.GetValue(p)?.ToString();
                    return string.Equals(key, title, StringComparison.OrdinalIgnoreCase);
                }
                catch { return false; }
            });
        }

        private double GetDouble(string title, double fallback, string section = null)
        {
            // section이 필요하면 TitleOnlyProperty로 섹션을 구분할 수 있지만
            // 여기서는 동일 제목 충돌이 없다는 전제에서 Title로 바로 검색
            var p = FindByTitle(title);
            if (p is DoubleProperty dp) return dp.Value;

            // Bool 등에서 들어오는 경우 숫자로 강제 변환 시도
            if (p is BoolProperty bp) return bp.Value ? 1.0 : 0.0;

            // 다른 타입이지만 Value가 숫자일 수 있는 경우
            var vProp = p?.GetType().GetProperty("Value");
            if (vProp != null)
            {
                var v = vProp.GetValue(p);
                if (v is IConvertible)
                {
                    try { return Convert.ToDouble(v); } catch { }
                }
            }
            return fallback;
        }

        private bool GetBool(string title, bool fallback)
        {
            var p = FindByTitle(title);
            if (p is BoolProperty bp) return bp.Value;

            // 숫자 → bool 변환 허용 (0=false, 그 외 true)
            var vProp = p?.GetType().GetProperty("Value");
            if (vProp != null)
            {
                var v = vProp.GetValue(p);
                if (v is bool b) return b;

                if (v is IConvertible)
                {
                    try { return Convert.ToDouble(v) != 0.0; } catch { }
                }
            }
            return fallback;
        }
        private int GetInt(string title, int fallback)
        {
            var p = FindByTitle(title);
            if (p == null) return fallback;

            // 타입별 빠른 경로
            switch (p)
            {
                case IntProperty ip: return ip.Value;
                case LongProperty lp:
                    try { checked { return (int)lp.Value; } } catch { return fallback; }
                case FloatProperty fp: return (int)Math.Round(fp.Value);
                case DoubleProperty dp: return (int)Math.Round(dp.Value);
                case BoolProperty bp: return bp.Value ? 1 : 0;
                case StringProperty sp:
                    if (int.TryParse(sp.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var i)) return i;
                    if (double.TryParse(sp.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)) return (int)Math.Round(d);
                    return fallback;
            }

            // 일반화 경로 (Value 리플렉션)
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
                    try { return Convert.ToInt32(v, CultureInfo.InvariantCulture); } catch { /* ignore */ }
                }
            }

            return fallback;
        }
    }
}