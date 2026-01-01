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
        Menual,    //Working,
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

            // 로그인 상태 변경 이벤트 구독
            AccountManager.OnLoginStateChanged += OnLoginStateChanged;

            // 초기 버튼 상태 설정
            UpdateButtonAccessByPermission();
        }

        private void CreateButton()
        {
            if (leftPanel == null || rightPanel == null)
                return;

            leftPanel.Controls.Clear();
            rightPanel.Controls.Clear();
            _listMenuButtons.Clear();

            var leftButtons = new[] { MenuButtonType.Main, MenuButtonType.Menual, MenuButtonType.Recipe, MenuButtonType.Config, MenuButtonType.Setup, MenuButtonType.Log };
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

        // 로그인 상태 변경 시 호출되는 이벤트 핸들러
        private void OnLoginStateChanged(object sender, EventArgs e)
        {
            UpdateButtonAccessByPermission();
        }

        // 권한에 따라 버튼 활성화/비활성화
        private void UpdateButtonAccessByPermission()
        {
            bool hasPermission = AccountManager.HasParameterAccessPermission();

            foreach (IndividualMenuButton button in _listMenuButtons)
            {
                MenuButtonType buttonType = (MenuButtonType)button.Tag;

                // Config와 Setup 버튼은 Maintenance 이상 권한 필요
                if (buttonType == MenuButtonType.Config || buttonType == MenuButtonType.Setup)
                {
                    button.Enabled = hasPermission;
                }
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

                // Config/Setup 버튼 클릭 시 권한 재확인 (추가 보안)
                if (menuButtons == MenuButtonType.Config || menuButtons == MenuButtonType.Setup)
                {
                    if (!AccountManager.HasParameterAccessPermission())
                    {
                        MessageBox.Show("Maintenance 이상의 권한이 필요합니다.", "권한 없음",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                if (menuButtons == MenuButtonType.Exit)
                {
                    var ask = new MessageBoxYesNo();
                    if (ask.ShowDialog("확인", "Exit?") != DialogResult.Yes)
                        return;

                    Application.Exit();
                    return;
                }
                else if (menuButtons == MenuButtonType.Login)
                {
                    FormLogin loginDialog = new FormLogin();
                    loginDialog.ShowDialog();
                    // 로그인 다이얼로그를 닫은 후 버튼 상태 업데이트
                    // (OnLoginStateChanged 이벤트가 자동으로 호출되므로 별도 호출 불필요)
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
