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
    #endregion

    public partial class TopContentsLoginModeControl : UserControl
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
            InitTableLayoutPanel();
            SetControlValue();
        }

        #region Method

        private void InitTableLayoutPanel()
        {
            // 테이블 레이아웃 패널 초기화
            tableLayoutContentsLoginModePanel.BackColor = Color.White;
            tableLayoutContentsLoginModePanel.Dock = DockStyle.None;
            tableLayoutContentsLoginModePanel.Anchor = AnchorStyles.None;
            tableLayoutContentsLoginModePanel.AutoSize = false;

            tableLayoutContentsLoginModePanel.RowCount = 2;
            tableLayoutContentsLoginModePanel.RowStyles.Clear();

            for (int i = 0; i < 2; i++)
                tableLayoutContentsLoginModePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / 2));
        }

        private void SetControlValue()
        {
            {
                _loginModeTitleLabel = new Label
                {
                    Text = "Login Mode",
                    TextAlign = ContentAlignment.MiddleLeft,
                    Dock = DockStyle.Bottom,
                    Font = new Font("Arial", _labelSize, FontStyle.Bold),
                };

                tableLayoutContentsLoginModePanel.Controls.Add(_loginModeTitleLabel, 0, 0);
            }
            {
                _loginModeLabel = new CustomBorderLabel
                {
                    Text = "Login Mode",
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill,
                    Font = new Font("Arial", _labelSize, FontStyle.Bold),
                    ForeColor = Color.Lime,
                    BackColor = Color.Black
                };
                tableLayoutContentsLoginModePanel.Controls.Add(_loginModeLabel, 0, 1);
                _loginModeLabel.Margin = new Padding(5);
            }
        }

        private void setTopContentsLoginMode(string mode)
        {
            // 로그인 모드 설정
            _loginModeLabel.Text = mode;
        }

        public void SetPanelSize(int width, int height)
        {
            // 비율 적용
            int panelWidth = (int)(width * 1.0);
            int panelHeight = (int)(height * 0.9);

            // tableLayoutMenuButtonPanel 크기 조정
            this.Size = new Size(panelWidth, panelHeight);
            tableLayoutContentsLoginModePanel.Size = new Size(panelWidth, panelHeight);

            // 좌측 정렬, 위아래 중앙 정렬
            int x = 0; // 좌측
            int y = (this.Height - tableLayoutContentsLoginModePanel.Height) / 2; // 위아래 중앙
            tableLayoutContentsLoginModePanel.Location = new Point(x, y);

            // 필요시 레이아웃 갱신
            tableLayoutContentsLoginModePanel.Invalidate();
            this.Invalidate();
        }
        #endregion

        #region EventHandler
        #endregion
    }
}

