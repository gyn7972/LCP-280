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
    public enum MenuButtonType
    {
        Main,
        Working,
        Recipe,
        Config,
        Setup,
        Log
    }
    public delegate void MenuButtonClickEventHandler(MenuButtonType type);
    #endregion

    public partial class BottomMenuButtonControl : UserControl
    {
        #region Field
        private List<IndividualMenuButton> _listMenuButtons;
        public event MenuButtonClickEventHandler ClickBottomMenuButton;
        #endregion
        #region Property
        public int ControlGap { set; get; }
        #endregion

        public BottomMenuButtonControl()
        {
            InitializeComponent();
            _listMenuButtons = new List<IndividualMenuButton>();
            this.BackColor = Color.White;
            ControlGap = 10;
            tableLayoutMenuButtonPanel.BackColor = Color.White;

            // 모든 테두리 제거
            this.tableLayoutMenuButtonPanel.CellBorderStyle = TableLayoutPanelCellBorderStyle.None;
            tableLayoutMenuButtonPanel.Location = new Point(0, 0);
            tableLayoutMenuButtonPanel.Dock = DockStyle.None;
            tableLayoutMenuButtonPanel.Anchor = AnchorStyles.None;
            tableLayoutMenuButtonPanel.AutoSize = false;

            // MenuButtonType enum의 모든 값으로 버튼 생성
            CreateButton(GetDefaultMenuButtons());
        }
        #region Method
        protected List<MenuButtonType> GetDefaultMenuButtons()
        {
            List<MenuButtonType> menus = new List<MenuButtonType>();

            foreach (MenuButtonType menuButtonType in Enum.GetValues(typeof(MenuButtonType)))
            {
                menus.Add(menuButtonType);
            }

            return menus;
        }
        public void CreateButton(List<MenuButtonType> menus)
        {
            if (menus.Count == 0)
            {
                menus = GetDefaultMenuButtons();
            }

            // 기존 컬럼 스타일 및 컨트롤 초기화
            tableLayoutMenuButtonPanel.Controls.Clear();
            tableLayoutMenuButtonPanel.ColumnStyles.Clear();
            _listMenuButtons.Clear();

            int buttonCount = menus.Count;

            // 열 개수 및 각 열의 비율 설정
            tableLayoutMenuButtonPanel.ColumnCount = buttonCount;
            for (int i = 0; i < buttonCount; i++)
            {
                tableLayoutMenuButtonPanel.ColumnStyles.Add(
                    new ColumnStyle(SizeType.Percent, 100f / buttonCount));
            }

            // 버튼 너비 계산
            int buttonWidth = tableLayoutMenuButtonPanel.Size.Width / buttonCount;
            int buttonHeight = tableLayoutMenuButtonPanel.Size.Height;

            foreach (MenuButtonType menuButtonType in menus)
            {
                IndividualMenuButton menuButton = new IndividualMenuButton();
                menuButton.Parent = this;
                menuButton.Size = new Size(buttonWidth, buttonHeight);
                menuButton.Dock = DockStyle.Fill;
                menuButton.Name = menuButtonType.ToString();
                menuButton.Text = menuButtonType.ToString();
                menuButton.Click += Button_Click;
                menuButton.MouseUp += ButtonUpImageChange;
                menuButton.TabStop = false;
                menuButton.Tag = menuButtonType;
                menuButton.SetButtonState(false);
                _listMenuButtons.Add(menuButton);
                tableLayoutMenuButtonPanel.Controls.Add(menuButton);
            }
        }

        public void Init()
        {
            foreach (IndividualMenuButton menuButton in _listMenuButtons)
            {
                menuButton.SetButtonState(false);
            }
        }

        public void EnableButton(bool bEnable)
        {
            foreach (IndividualMenuButton button in _listMenuButtons)
            {
                button.Enabled = bEnable;
            }
        }

        public void SetPanelSize(int width, int height)
        {
            // UserControl 전체 크기 조정
            this.Width = width;
            this.Height = height;
            this.Size = new Size(width, height);

            // 비율 적용
            int panelWidth = (int)(width * 0.6);
            int panelHeight = (int)(height * 0.9);

            // tableLayoutMenuButtonPanel 크기 조정
            this.Size = new Size(panelWidth, panelHeight);
            tableLayoutMenuButtonPanel.Size = new Size(panelWidth, panelHeight);

            // 중앙 정렬 (좌우 중앙, 상단 기준)
            tableLayoutMenuButtonPanel.Location = new Point(
                (this.Width - panelWidth) / 2,
                0
            );

            // 필요시 레이아웃 갱신
            tableLayoutMenuButtonPanel.Invalidate();
            this.Invalidate();
        }
        #endregion

        #region EventHandler
        public void Button_Click(object sender, EventArgs e)
        {
            IndividualMenuButton button = sender as IndividualMenuButton;
            if (button != null)
            {
                MenuButtonType menuButtons = (MenuButtonType)button.Tag;
                ClickBottomMenuButton?.Invoke(menuButtons);
            }
        }

        public void ButtonUpImageChange(object sender, MouseEventArgs e)
        {
            IndividualMenuButton button = sender as IndividualMenuButton;
            if (button != null)
            {
                Init();
                button.SetButtonState(true);
            }
        }
        #endregion
    }
}
