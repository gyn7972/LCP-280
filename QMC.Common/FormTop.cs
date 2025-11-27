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