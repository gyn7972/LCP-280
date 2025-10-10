using QMC.Common.Alarm;
using System;
using System.Drawing;
using System.Windows.Forms;

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
        private TableLayoutPanel _tableLayoutPanelFormTop;

        public FormTop()
        {
            InitializeComponent();

            this.BackColor = Color.White; // 폼 배경색을 하얀색으로 설정

            // 컨트롤 생성
            _topContentsEquipmentControl = new TopContentsEquipmentControl();
            _topContentsEquipmentControl.Dock = DockStyle.None;
            this.Controls.Add(_topContentsEquipmentControl);
            

            _topContentsStatusControl = new TopContentsStatusControl();
            _topContentsStatusControl.Dock = DockStyle.None;
            this.Controls.Add(_topContentsStatusControl);

            _topContentsLoginModeControl = new TopContentsLoginModeControl();
            _topContentsLoginModeControl.Dock = DockStyle.None;
            this.Controls.Add(_topContentsLoginModeControl);

            _topContentsStatusControl.ClickTopAlarmClearButton += GetTopContentsStatusControl_ClickTopAlarmClearButton;
        }

        private void GetTopContentsStatusControl_ClickTopAlarmClearButton()
        {
            if (true)
            {
                var alarms = AlarmManager.Instance.Alarms;
                if (alarms != null && alarms.Count > 0)
                {
                    AlarmManager.Instance.ClearAllAlarms();
                    //CommonModule.Instance.TowerLamp_BuzzerStop = true;

                    // UI 갱신이나 로그 기록
                    Log.Write("LCP_280", "Alarm", "모든 알람이 수동으로 해제되었습니다.");
                }
            }

            var mb = new MessageBoxOk();
            mb.ShowDialog("Clear!", $"Alarm Clear");
        }


        public void SetPanelSize(int width, int height)
        {
            this.Width = width;
            this.Height = height;
            this.ClientSize = new System.Drawing.Size(width, height);

            _topContentsEquipmentControl.SetPanelSize(230, height);
            _topContentsStatusControl.SetPanelSize(896, height);
            _topContentsLoginModeControl.SetPanelSize(150, height);

            _topContentsEquipmentControl.Location = new Point(0, 3);
            _topContentsStatusControl.Location = new Point(
                _topContentsEquipmentControl.Location.X - 1 + _topContentsEquipmentControl.Width, _topContentsEquipmentControl.Location.Y);
            _topContentsLoginModeControl.Location = new Point(
                _topContentsStatusControl.Location.X - 1 + _topContentsStatusControl.Width, _topContentsStatusControl.Location.Y);
        }
    }
}
