using QMC.Common.CustomControl;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace QMC.Common
{
    #region Define

    #endregion

    public partial class TopContentsEquipmentControl : UserControl, IResizable
    {
        #region Field
        private CustomBorderLabel _machineName;
        private CustomBorderLabel _dateLabel;
        private CustomBorderLabel _timeLabel;
        private CustomBorderLabel _buildVerLabel;
        private Timer _timer;
        private int _labelSize = 8;
        private int _labelMargin = 3;

        #endregion
        #region Property
        #endregion

        public TopContentsEquipmentControl()
        {
            InitializeComponent();
            this.BackColor = Color.White;
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);

            // 런타임 전용 초기화 (디자인 모드에서는 실행하지 않음)
            if (!IsDesignMode())
            {
                ApplyRuntimeFonts();

                // Timer 설정
                _timer = new Timer();
                _timer.Interval = 1000; // 1초
                _timer.Tick += Timer_Tick;
                _timer.Start();

                // 최초 값 갱신
                UpdateDateTime();

                // 수명주기 정리
                this.Disposed += (s, e) =>
                {
                    try { _timer?.Stop(); _timer?.Dispose(); } catch { }
                    _timer = null;
                };
            }
        }

        private bool IsDesignMode()
        {
            return LicenseManager.UsageMode == LicenseUsageMode.Designtime
                   || (this.Site?.DesignMode ?? false)
                   || this.DesignMode;
        }

        // 디자이너에서 구성하므로 빈 메서드로 유지
        private void InitTableLayoutPanel() { }
        private void SetControlValue() { }

        private void ApplyRuntimeFonts()
        {
            var bold = FontStyle.Bold;
            if (_machineName != null)
                _machineName.Font = new Font(_machineName.Font.FontFamily, _labelSize, bold);
            if (_dateLabel != null)
                _dateLabel.Font = new Font(_dateLabel.Font.FontFamily, _labelSize, bold);
            if (_timeLabel != null)
                _timeLabel.Font = new Font(_timeLabel.Font.FontFamily, _labelSize, bold);
            if (_buildVerLabel != null)
                _buildVerLabel.Font = new Font(_buildVerLabel.Font.FontFamily, _labelSize, bold);
        }

        public void SetTopContentsEquipmentValue(string machineName, string buildVersion)
        {
            _machineName.Text = machineName;
            _buildVerLabel.Text = buildVersion;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateDateTime();
        }

        private void UpdateDateTime()
        {
            _dateLabel.Text = DateTime.Now.ToString("yyyy-MM-dd");
            _timeLabel.Text = DateTime.Now.ToString("HH:mm:ss");
        }
        #region Method
        public void SetPanelSize(int width, int height)
        {
            this.SuspendLayout();
            tableLayoutContentsEquipmentPanel.SuspendLayout();
            try
            {
                // 비율 적용
                int panelWidth = (int)(width * 1.0);
                int panelHeight = (int)(height * 0.9);

                // UserControl 크기 조정
                this.Size = new Size(panelWidth, panelHeight);

                // Dock=Fill이면 별도 위치/크기 조정 불필요
                if (tableLayoutContentsEquipmentPanel.Dock == DockStyle.None)
                {
                    tableLayoutContentsEquipmentPanel.Size = new Size(panelWidth, panelHeight);
                    tableLayoutContentsEquipmentPanel.ClientSize = new Size(panelWidth, panelHeight);

                    // 좌측 정렬, 위아래 중앙 정렬
                    int x = 0; // 좌측
                    int y = (this.Height - tableLayoutContentsEquipmentPanel.Height) / 2; // 위아래 중앙
                    tableLayoutContentsEquipmentPanel.Location = new Point(x, y);
                }
            }
            finally
            {
                tableLayoutContentsEquipmentPanel.ResumeLayout();
                this.ResumeLayout();
            }

            // 필요시 레이아웃 갱신
            tableLayoutContentsEquipmentPanel.Invalidate();
            this.Invalidate();
        }
        #endregion

        #region EventHandler

        #endregion

        protected override void OnHandleDestroyed(EventArgs e)
        {
            try { _timer?.Stop(); _timer?.Dispose(); } catch { }
            _timer = null;
            base.OnHandleDestroyed(e);
        }
    }
}