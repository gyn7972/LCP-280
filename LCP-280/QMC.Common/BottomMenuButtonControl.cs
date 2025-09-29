using QMC.Common.Account;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QMC.Common
{
    public enum MenuButtonType
    {
        Main,
        Working,
        Recipe,
        Config,
        Setup,
        Log,
        Login,
        Exit
    }
    public delegate void MenuButtonClickEventHandler(MenuButtonType type);

    public partial class BottomMenuButtonControl : UserControl
    {
        private List<IndividualMenuButton> _listMenuButtons;
        public event MenuButtonClickEventHandler ClickBottomMenuButton;
        public int ControlGap { set; get; }

        // 추가: 좌/우 패널
        private FlowLayoutPanel leftPanel;
        private FlowLayoutPanel rightPanel;

        public BottomMenuButtonControl()
        {
            InitializeComponent();
            _listMenuButtons = new List<IndividualMenuButton>();
            this.BackColor = Color.White;
            ControlGap = 10;
            tableLayoutMenuButtonPanel.BackColor = Color.White;
            tableLayoutMenuButtonPanel.Dock = DockStyle.Fill;
            tableLayoutMenuButtonPanel.AutoSize = false;
            tableLayoutMenuButtonPanel.CellBorderStyle = TableLayoutPanelCellBorderStyle.None;

            // 좌/우 패널 생성 및 추가
            leftPanel = new FlowLayoutPanel();
            leftPanel.Dock = DockStyle.Left;
            leftPanel.FlowDirection = FlowDirection.LeftToRight;
            leftPanel.WrapContents = false;
            leftPanel.AutoSize = true;
            leftPanel.Margin = new Padding(0);
            leftPanel.Padding = new Padding(0);

            rightPanel = new FlowLayoutPanel();
            rightPanel.Dock = DockStyle.Right;
            rightPanel.FlowDirection = FlowDirection.LeftToRight;
            rightPanel.WrapContents = false;
            rightPanel.AutoSize = true;
            rightPanel.Margin = new Padding(0);
            rightPanel.Padding = new Padding(0);

            this.Controls.Add(leftPanel);
            this.Controls.Add(rightPanel);

            CreateButton();
        }

        private void CreateButton()
        {
            if (leftPanel == null || rightPanel == null)
                return;

            leftPanel.Controls.Clear();
            rightPanel.Controls.Clear();
            _listMenuButtons.Clear();

            var leftButtons = new[] { MenuButtonType.Main, MenuButtonType.Working, MenuButtonType.Recipe, MenuButtonType.Config, MenuButtonType.Setup, MenuButtonType.Log };
            var rightButtons = new[] { MenuButtonType.Login, MenuButtonType.Exit };

            int btnWidth = 120;
            int btnHeight = 48;
            int panelHeight = this.Height > 0 ? this.Height : 60;
            int marginY = Math.Max(0, (panelHeight - btnHeight) / 2);
            Padding btnMargin = new Padding(6, marginY, 6, marginY);
            Size btnSize = new Size(btnWidth, btnHeight);

            foreach (var menuButtonType in leftButtons)
            {
                var btn = CreateMenuButton(menuButtonType, btnSize, btnMargin);
                leftPanel.Controls.Add(btn);
            }
            foreach (var menuButtonType in rightButtons)
            {
                var btn = CreateMenuButton(menuButtonType, btnSize, btnMargin);
                rightPanel.Controls.Add(btn);
            }
        }

        private IndividualMenuButton CreateMenuButton(MenuButtonType menuButtonType, Size btnSize, Padding btnMargin)
        {
            IndividualMenuButton menuButton = new IndividualMenuButton();
            menuButton.Parent = this;
            menuButton.Size = btnSize;
            menuButton.Margin = btnMargin;
            menuButton.Name = menuButtonType.ToString();
            menuButton.Text = menuButtonType.ToString();
            menuButton.Click += Button_Click;
            if (!(menuButtonType == MenuButtonType.Login || menuButtonType == MenuButtonType.Exit))
                menuButton.MouseUp += ButtonUpImageChange;
            menuButton.TabStop = false;
            menuButton.Tag = menuButtonType;
            menuButton.SetButtonState(false);
            _listMenuButtons.Add(menuButton);
            return menuButton;
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
            this.Width = width;
            this.Height = height;
            this.Size = new Size(width, height);
        }

        public void Button_Click(object sender, EventArgs e)
        {
            IndividualMenuButton button = sender as IndividualMenuButton;
            if (button != null)
            {
                MenuButtonType menuButtons = (MenuButtonType)button.Tag;

                if (menuButtons == MenuButtonType.Exit)
                {
                    Application.Exit();
                    return;
                }
                else if(menuButtons == MenuButtonType.Login)
                {
                    FormLogin loginDialog = new FormLogin();
                    loginDialog.ShowDialog();
                    return;
                }
                
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

        public override Size MinimumSize
        {
            get { return new Size(400, 60); }
            set { base.MinimumSize = value; }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            CreateButton(); // 크기 변경 시 버튼 재생성(여백 재계산)
        }
    }
}
