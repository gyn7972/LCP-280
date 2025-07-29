using System;
using System.Drawing;
using System.Windows.Forms;

namespace SP_GridTypeView
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
            menuControl.Anchor = AnchorStyles.Left; // 좌측 정렬
            this.Controls.Add(menuControl);
            menuControl.ClickBottomMenuButton += BottomMenuButtonControl_ClickBottomMenuButton;
        }

        private void BottomMenuButtonControl_ClickBottomMenuButton(MenuButtonType type)
        {
            // 이벤트를 상위로 전달
            MenuButtonClicked?.Invoke(type);
        }

        private void AlignMenuControl()
        {
            // 좌측(0), 위아래 가운데 정렬
            int x = 0;
            int y = (this.ClientSize.Height - menuControl.Height) / 2;
            menuControl.Location = new System.Drawing.Point(x, y);
        }

        public void SetPanelSize(int width, int height)
        {
            this.Width = width;
            this.Height = height;
            this.ClientSize = new System.Drawing.Size(width, height);

            // menuControl 및 내부 패널 크기 동기화
            menuControl.SetPanelSize(width, height);
            AlignMenuControl();
        }
    }
}
