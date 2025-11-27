using QMC.Common.Account;
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

    public partial class TopContentsLoginModeControl : UserControl, IResizable
    {
        #region Field
        private Label _loginModeTitleLabel;
        private CustomBorderLabel _loginModeLabel;

        private int _labelSize;
        #endregion

        #region Property
        #endregion

        public TopContentsLoginModeControl()
        {
            InitializeComponent();
            _labelSize = 8;
            this.BackColor = Color.White;
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);

            // 디자인 모드에서는 런타임 의존 로직을 실행하지 않음
            if (!IsDesignMode())
            {
                ApplyRuntimeFonts();
                // 초기 표시
                try
                {
                    var userId = AccountManager.CurrentAccount != null
                        ? AccountManager.CurrentAccount.UserID
                        : AccountManager.GuestAccount.UserID;
                    setTopContentsLoginMode(userId);
                }
                catch { /* 디자인/테스트 환경 보호 */ }

                AccountManager.OnLoginStateChanged += (s, e) =>
                {
                    try { setTopContentsLoginMode(AccountManager.CurrentAccount.UserID); } catch { }
                };

                this.Disposed += (s, e) =>
                {
                    try { AccountManager.OnLoginStateChanged -= (s2, e2) => { }; } catch { }
                };
            }
        }

        private bool IsDesignMode()
        {
            return LicenseManager.UsageMode == LicenseUsageMode.Designtime
                   || (this.Site?.DesignMode ?? false)
                   || this.DesignMode;
        }

        #region Method

        // 디자이너에서 구성하므로 빈 메서드로 둠
        private void InitTableLayoutPanel() { }
        private void SetControlValue() { }

        private void ApplyRuntimeFonts()
        {
            var bold = FontStyle.Bold;
            if (_loginModeTitleLabel != null)
                _loginModeTitleLabel.Font = new Font(_loginModeTitleLabel.Font.FontFamily, _labelSize, bold);
            if (_loginModeLabel != null)
                _loginModeLabel.Font = new Font(_loginModeLabel.Font.FontFamily, _labelSize, bold);
        }

        private void setTopContentsLoginMode(string mode)
        {
            // 로그인 모드 설정
            if (_loginModeLabel != null)
                _loginModeLabel.Text = mode;
        }

        public void SetPanelSize(int width, int height)
        {
            this.SuspendLayout();
            tableLayoutContentsLoginModePanel.SuspendLayout();
            try
            {
                int panelWidth = (int)(width * 1.0);
                int panelHeight = (int)(height * 0.9);

                this.Size = new Size(panelWidth, panelHeight);

                // Dock=Fill이면 별도 위치/크기 조정 불필요
                if (tableLayoutContentsLoginModePanel.Dock == DockStyle.None)
                {
                    tableLayoutContentsLoginModePanel.Size = new Size(panelWidth, panelHeight);
                    int x = 0; // 좌측
                    int y = (this.Height - tableLayoutContentsLoginModePanel.Height) / 2; // 위아래 중앙
                    tableLayoutContentsLoginModePanel.Location = new Point(x, y);
                }
            }
            finally
            {
                tableLayoutContentsLoginModePanel.ResumeLayout();
                this.ResumeLayout();
            }

            tableLayoutContentsLoginModePanel.Invalidate();
            this.Invalidate();
        }
        #endregion

        #region EventHandler
        #endregion
    }
}