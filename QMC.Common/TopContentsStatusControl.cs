using QMC.Common.CustomControl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace QMC.Common
{
    #region Define
    public delegate void TopAlarmClearClickEventHandler();
    #endregion

    public partial class TopContentsStatusControl : UserControl, IResizable
    {
        #region Field
        private CustomBorderLabel _mesMessageTitleLabel;
        private CustomBorderLabel _systemMessageTitleLabel;
        private CustomBorderLabel _operationRecipeTitleLabel;
        private CustomBorderLabel _mesMessageLabel;
        private CustomBorderLabel _systemMessageLabel;
        private CustomBorderLabel _operationRecipeLabel;

        private int _labelSize = 8;
        private int _labelMargin = 2;

        private IndividualMenuButton _AlarmClearButton;

        public event TopAlarmClearClickEventHandler ClickTopAlarmClearButton;

        private Timer _timer;

        #endregion
        #region Property
        #endregion

        public TopContentsStatusControl()
        {
            InitializeComponent();

            this.BackColor = Color.White;
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);

            // 런타임 전용 초기화 (디자인 모드에서는 실행하지 않음)
            if (!IsDesignMode())
            {
                // Timer 설정
                _timer = new Timer();
                _timer.Interval = 1000; // 1초
                _timer.Tick += Timer_Tick;
                _timer.Start();

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

        private void Timer_Tick(object sender, EventArgs e)
        {
            var eq = EquipmentLocator.Instance;
            string recipe = eq.ICurrentRecipe;
            SetTopoContentsOperationRecipeValue(recipe);

            RefreshSystemMessageFromEquipment();
        }

        #region Method

        // 기존 동적 생성 코드는 디자이너에서 생성하므로 더 이상 호출하지 않습니다.
        // 유지만 하고 미사용 상태로 둡니다(호출 시 중복 추가 방지).
        private void InitTableLayoutPanel()
        {
            // 디자이너에서 설정하므로 비워 둡니다.
        }

        private void SetControlValue()
        {
            // 디자이너에서 컨트롤을 생성하므로 비워 둡니다.
        }

        private void SetTopoContentsMesMessageValue(string mesMessage)
        {
            _mesMessageLabel.Text = mesMessage;
        }
        private void SetTopoContentsSystemMessageValue(string systemMessage)
        {
            _systemMessageLabel.Text = systemMessage;
        }
        private void SetTopoContentsOperationRecipeValue(string opRecipe)
        {
            _operationRecipeLabel.Text = opRecipe;
        }

        public void CreateAlarmClearButton()
        {
            // 디자이너에서 이미 생성됨
            if (_AlarmClearButton != null) return;

            _AlarmClearButton = new IndividualMenuButton();
            _AlarmClearButton.Parent = this;
            _AlarmClearButton.Dock = DockStyle.Fill;
            _AlarmClearButton.Name = "Alarm Clear";
            _AlarmClearButton.Text = "Alarm Clear";
            _AlarmClearButton.Click += Button_Click;
            _AlarmClearButton.TabStop = false;
            _AlarmClearButton.SetButtonState(false);
            tableLayoutContentsStatusPanel.Controls.Add(_AlarmClearButton, 2, 0);
            tableLayoutContentsStatusPanel.SetRowSpan(_AlarmClearButton, 3);
        }

        public void Init()
        {
            _AlarmClearButton?.SetButtonState(false);
        }

        public void SetPanelSize(int width, int height)
        {
            this.SuspendLayout();
            tableLayoutContentsStatusPanel.SuspendLayout();
            try
            {
                // UserControl 전체 크기 조정
                int panelWidth = (int)(width * 1.0);
                int panelHeight = (int)(height * 0.9);

                // UserControl 크기 설정
                this.Size = new Size(panelWidth, panelHeight);

                // TableLayoutPanel이 Dock=Fill 이면 크기/위치 계산 불필요
                if (tableLayoutContentsStatusPanel.Dock == DockStyle.None)
                {
                    tableLayoutContentsStatusPanel.Size = new Size(panelWidth, panelHeight);

                    // 좌측 정렬, 위아래 중앙 정렬
                    int x = 0; // 좌측
                    int y = (this.Height - tableLayoutContentsStatusPanel.Height) / 2; // 위아래 중앙
                    tableLayoutContentsStatusPanel.Location = new Point(x, y);
                }
            }
            finally
            {
                tableLayoutContentsStatusPanel.ResumeLayout();
                this.ResumeLayout();
            }

            // 필요시 레이아웃 갱신
            tableLayoutContentsStatusPanel.Invalidate();
            this.Invalidate();
        }

        // ========= 여기에 Public API 추가 =========

        // 간단: 메시지 라인 3개만 갱신
        public void SetStatusTexts(string mesMessage, string systemMessage, string operationRecipe)
        {
            SafeUI(() =>
            {
                if (_mesMessageLabel != null) _mesMessageLabel.Text = mesMessage ?? string.Empty;
                if (_systemMessageLabel != null) _systemMessageLabel.Text = systemMessage ?? string.Empty;
                if (_operationRecipeLabel != null) _operationRecipeLabel.Text = operationRecipe ?? string.Empty;
            });
        }

        // 타이틀/색상까지 한 번에 갱신 (필요한 항목만 전달하면 해당 항목만 변경)
        public void UpdateStatusPanel(
            string mesTitle = null,
            string systemTitle = null,
            string recipeTitle = null,
            string mesMessage = null,
            string systemMessage = null,
            string operationRecipe = null,
            Color? valueForeColor = null,
            Color? valueBackColor = null)
        {
            SafeUI(() =>
            {
                // 타이틀
                if (mesTitle != null && _mesMessageTitleLabel != null) _mesMessageTitleLabel.Text = mesTitle;
                if (systemTitle != null && _systemMessageTitleLabel != null) _systemMessageTitleLabel.Text = systemTitle;
                if (recipeTitle != null && _operationRecipeTitleLabel != null) _operationRecipeTitleLabel.Text = recipeTitle;

                // 값
                if (mesMessage != null && _mesMessageLabel != null) _mesMessageLabel.Text = mesMessage;
                if (systemMessage != null && _systemMessageLabel != null) _systemMessageLabel.Text = systemMessage;
                if (operationRecipe != null && _operationRecipeLabel != null) _operationRecipeLabel.Text = operationRecipe;

                // 색상(값 라인 공통 적용)
                if (valueForeColor.HasValue)
                {
                    if (_mesMessageLabel != null) _mesMessageLabel.ForeColor = valueForeColor.Value;
                    if (_systemMessageLabel != null) _systemMessageLabel.ForeColor = valueForeColor.Value;
                    if (_operationRecipeLabel != null) _operationRecipeLabel.ForeColor = valueForeColor.Value;
                }
                if (valueBackColor.HasValue)
                {
                    if (_mesMessageLabel != null) _mesMessageLabel.BackColor = valueBackColor.Value;
                    if (_systemMessageLabel != null) _systemMessageLabel.BackColor = valueBackColor.Value;
                    if (_operationRecipeLabel != null) _operationRecipeLabel.BackColor = valueBackColor.Value;
                }
            });
        }

        // 스레드 안전 UI 업데이트 도우미
        private void SafeUI(Action action)
        {
            if (this.IsDisposed) return;
            if (this.InvokeRequired)
            {
                try { this.BeginInvoke(new MethodInvoker(() => { if (!this.IsDisposed) action(); })); } catch { }
            }
            else
            {
                action();
            }
        }

        #endregion

        #region EventHandler
        public void Button_Click(object sender, EventArgs e)
        {
            IndividualMenuButton button = sender as IndividualMenuButton;
            if (button != null)
            {
                ClickTopAlarmClearButton?.Invoke();
            }
        }

        public void ButtonUpImageChange(object sender, MouseEventArgs e)
        {
            IndividualMenuButton button = sender as IndividualMenuButton;
            if (button != null)
            {
                Init();
                _AlarmClearButton.SetButtonState(true);
            }
        }

        private void RefreshSystemMessageFromEquipment()
        {
            try
            {
                var eq = EquipmentLocator.Instance;
                if (eq == null || _systemMessageLabel == null) return;

                string stateText = eq.EqState.ToString();

                // EqState 프로퍼티를 리플렉션으로 읽음 (공용 enum 타입 의존 제거)
                //var pi = eq.GetType().GetProperty("EqState");
                //var stateObj = pi != null ? pi.GetValue(eq, null) : null;
                //var stateText = (stateObj != null) ? stateObj.ToString() : "Unknown";

                // 텍스트 세팅
                SetTopoContentsSystemMessageValue(stateText);

                // 상태별 색상 매핑
                // Ready/Running -> Lime, Initializing/Starting/Stopping/CycleStop -> Yellow, 
                // Stopped/Unknown -> Silver, Error -> Red
                Color fore = Color.Lime;
                switch (stateText)
                {
                    case "Ready":
                    case "Running":
                        fore = Color.Lime;
                        break;
                    case "Initializing":
                    case "Starting":
                    case "Stopping":
                    case "CycleStop":
                    case "Warning":
                        fore = Color.Yellow;
                        break;
                    case "Stopped":
                    case "Unknown":
                        fore = Color.Silver;
                        break;
                    case "Error":
                        fore = Color.Red;
                        break;
                    default:
                        fore = Color.Silver;
                        break;
                }

                // UI 반영
                if (_systemMessageLabel.ForeColor != fore)
                    _systemMessageLabel.ForeColor = fore;
                // 배경은 기존 스타일 유지(검정). 필요 시 변경:
                // _systemMessageLabel.BackColor = Color.Black;
            }
            catch
            {
                // 안전: 디자인 모드/초기화 전 예외 무시
            }
        }

        #endregion
    }
}