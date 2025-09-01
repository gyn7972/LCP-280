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
        #endregion
        #region Property
        #endregion

        public TopContentsStatusControl()
        {
            InitializeComponent();
            this.BackColor = Color.White;
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            InitTableLayoutPanel();
            SetControlValue();
            CreateAlarmClearButton();
        }

        #region Method

        private void InitTableLayoutPanel()
        {
            tableLayoutContentsStatusPanel.BackColor = Color.White;
            tableLayoutContentsStatusPanel.Dock = DockStyle.None;
            tableLayoutContentsStatusPanel.Anchor = AnchorStyles.None;
            tableLayoutContentsStatusPanel.AutoSize = false;

            tableLayoutContentsStatusPanel.RowCount = 3;
            tableLayoutContentsStatusPanel.ColumnCount = 3;
            tableLayoutContentsStatusPanel.ColumnStyles.Clear();
            tableLayoutContentsStatusPanel.RowStyles.Clear();

            for (int i = 0; i < 3; i++)
                tableLayoutContentsStatusPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / 3));

            tableLayoutContentsStatusPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10f / 3));
            tableLayoutContentsStatusPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 80f / 3));
            tableLayoutContentsStatusPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10f / 3));
        }

        private void SetControlValue()
        {
            this.SuspendLayout();
            tableLayoutContentsStatusPanel.SuspendLayout();
            try
            {
                _mesMessageTitleLabel = new CustomBorderLabel { Text = "MES MSG.", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter };
                tableLayoutContentsStatusPanel.Controls.Add(_mesMessageTitleLabel, 0, 0);
                _mesMessageTitleLabel.Margin = new Padding(_labelMargin);

                _systemMessageTitleLabel = new CustomBorderLabel { Text = "SYSTEM", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter };
                tableLayoutContentsStatusPanel.Controls.Add(_systemMessageTitleLabel, 0, 1);
                _systemMessageTitleLabel.Margin = new Padding(_labelMargin);

                _operationRecipeTitleLabel = new CustomBorderLabel { Text = "OP Recipe", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter };
                tableLayoutContentsStatusPanel.Controls.Add(_operationRecipeTitleLabel, 0, 2);
                _operationRecipeTitleLabel.Margin = new Padding(_labelMargin);

                _mesMessageLabel = new CustomBorderLabel
                {
                    Text = "MES Message",
                    TextAlign = ContentAlignment.MiddleLeft,
                    Dock = DockStyle.Fill,
                    Font = new Font("Arial", _labelSize, FontStyle.Bold),
                    ForeColor = Color.Lime,
                    BackColor = Color.Black,
                    TabStop = false
                };
                tableLayoutContentsStatusPanel.Controls.Add(_mesMessageLabel, 1, 0);
                _mesMessageLabel.Margin = new Padding(_labelMargin);

                _systemMessageLabel = new CustomBorderLabel
                {
                    Text = "System Message",
                    TextAlign = ContentAlignment.MiddleLeft,
                    Dock = DockStyle.Fill,
                    Font = new Font("Arial", _labelSize, FontStyle.Bold),
                    ForeColor = Color.Lime,
                    BackColor = Color.Black,
                    TabStop = false
                };
                tableLayoutContentsStatusPanel.Controls.Add(_systemMessageLabel, 1, 1);
                _systemMessageLabel.Margin = new Padding(_labelMargin);

                _operationRecipeLabel = new CustomBorderLabel
                {
                    Text = "Operation Recipe",
                    TextAlign = ContentAlignment.MiddleLeft,
                    Dock = DockStyle.Fill,
                    Font = new Font("Arial", _labelSize, FontStyle.Bold),
                    ForeColor = Color.Lime,
                    BackColor = Color.Black,
                    TabStop = false
                };
                tableLayoutContentsStatusPanel.Controls.Add(_operationRecipeLabel, 1, 2);
                _operationRecipeLabel.Margin = new Padding(_labelMargin);
            }
            finally
            {
                tableLayoutContentsStatusPanel.ResumeLayout();
                this.ResumeLayout();
            }
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

                // TableLayoutPanel 크기 설정
                tableLayoutContentsStatusPanel.Size = new Size(panelWidth, panelHeight);

                // 좌측 정렬, 위아래 중앙 정렬
                int x = 0; // 좌측
                int y = (this.Height - tableLayoutContentsStatusPanel.Height) / 2; // 위아래 중앙
                tableLayoutContentsStatusPanel.Location = new Point(x, y);
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
        #endregion
    }
}

