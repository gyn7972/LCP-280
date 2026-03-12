using QMC.Common.Alarm;
using System;
using System.Drawing;
using System.Windows.Forms;
using QMC.LCP_280.Process;

namespace QMC.Common
{
    public enum TopButtons
    {
        Alarm,
        Buzzer,
        AlarmLog
    }

    public delegate void ButtonClickHandler(TopButtons sender);
    public partial class FormTop : Form
    {
        private TopContentsEquipmentControl _topContentsEquipmentControl;
        private TopContentsStatusControl _topContentsStatusControl;
        private TopContentsLoginModeControl _topContentsLoginModeControl;
        private TopContentsIOStatusControl _topContentsIOStatusControl;
        private TableLayoutPanel _tableLayoutPanelFormTop;

        public FormTop()
        {
            InitializeComponent();

            this.BackColor = Color.White;

            // 디자이너가 생성한 컨트롤 사용
            _topContentsStatusControl.ClickTopAlarmClearButton += GetTopContentsStatusControl_ClickTopAlarmClearButton;

            // [ADD] 초기 실행 시 장비명 및 버전 정보 표시
            InitializeMachineInfo();
        }

        private void InitializeMachineInfo()
        {
            try
            {
                // 1. 실행 파일(Entry Assembly)의 버전 가져오기
                var version = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Version;
                string versionStr = (version != null)
                    ? $"v{version.Major}.{version.Minor}.{version.Build}"
                    : "v1.0.0";

                // 2. 장비 이름 설정 (필요시 Config에서 로드)
                string machineName = "LCP-280";

                // [수정] Equipment 또는 EquipmentLocator를 통해 Config 접근
                dynamic eq = EquipmentLocator.Instance;
                if (eq != null)
                {
                    try
                    {
                        // 컴파일러가 런타임에 속성 존재 여부를 확인합니다.
                        // EquipmentConfig.EquipmentId (또는 EquipmentName) 접근
                        string val = eq.EquipmentConfig.EquipmentId;
                        if (!string.IsNullOrWhiteSpace(val))
                        {
                            machineName = val;
                        }
                    }
                    catch
                    {
                        // 속성이 없거나 접근 실패 시 무시 (기본값 유지)
                    }
                }

                //var propConfig = eq.GetType().GetProperty("EquipmentConfig");
                //var configObj = propConfig?.GetValue(eq);
                //var propName = configObj.GetType().GetProperty("EquipmentId");
                //var val = propName?.GetValue(configObj) as string;
                //if (!string.IsNullOrWhiteSpace(val))
                //{
                //    machineName = val;
                //}

                // 3. 컨트롤에 값 적용
                if (_topContentsEquipmentControl != null)
                {
                    _topContentsEquipmentControl.SetTopContentsEquipmentValue(machineName, versionStr);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FormTop] Version Init Fail: {ex.Message}");
            }
        }

        private void GetTopContentsStatusControl_ClickTopAlarmClearButton()
        {
            var alarms = AlarmManager.Instance.Alarms;
            if (alarms != null && alarms.Count > 0)
            {
                AlarmManager.Instance.ClearAllAlarms();
                Log.Write("LCP_280", "Alarm", "모든 알람이 수동으로 해제되었습니다.");
            }

            var mb = new MessageBoxOk();
            mb.ShowDialog("Clear!", $"Alarm Clear");
        }

        // 외부에서 호출하더라도 테이블 레이아웃이 Dock=Fill 이므로 크기만 반영
        public void SetPanelSize(int width, int height)
        {
            this.ClientSize = new Size(width, height);
        }
    }
}