using System;
using System.Drawing;
using System.Windows.Forms;

namespace QMC.Common
{
    public partial class FormBottom : Form
    {
        private BottomMenuButtonControl menuControl;
        
        // FormMain이 구독할 수 있는 이벤트 추가
        public event MenuButtonClickEventHandler MenuButtonClicked;

        public FormBottom()
        {
            InitializeComponent();

            this.BackColor = Color.White; // 폼 배경색을 하얀색으로 설정

            menuControl = new BottomMenuButtonControl();
            menuControl.Dock = DockStyle.Fill; // 전체 영역에 맞춤
            this.Controls.Add(menuControl);
            menuControl.ClickBottomMenuButton += BottomMenuButtonControl_ClickBottomMenuButton;
        }

        private void BottomMenuButtonControl_ClickBottomMenuButton(MenuButtonType type)
        {
            // 이벤트를 상위로 전달
            MenuButtonClicked?.Invoke(type);
        }

        public void SetPanelSize(int width, int height)
        {
            this.Width = width;
            this.Height = height;
            this.ClientSize = new System.Drawing.Size(width, height);
            // menuControl의 위치/크기 조정은 필요 없음 (DockStyle.Fill)
        }
    }
}
