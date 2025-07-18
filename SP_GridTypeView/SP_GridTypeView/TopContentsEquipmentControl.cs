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

namespace SP_GridTypeView
{
    #region Define
    
    #endregion

    public partial class TopContentsEquipmentControl : UserControl
    {
        #region Field
        private Label _machineName;
        private Label _dateLabel;
        private Label _timeLabel;
        private Label _buildVerLabel;
        private Timer _timer;
        private int _labelSize;
        #endregion
        #region Property
        #endregion

        public TopContentsEquipmentControl()
        {
            InitializeComponent();
            _labelSize = 8;
            this.BackColor = Color.White;
            InitTableLayoutPanel();
            SetControlValue();

            // Timer 설정
            _timer = new Timer();
            _timer.Interval = 1000; // 1초
            _timer.Tick += Timer_Tick;
            _timer.Start();

            // 최초 값 갱신
            UpdateDateTime();
        }

        private void InitTableLayoutPanel()
        {
            // 테이블 레이아웃 패널 초기화
            tableLayoutContentsEquipmentPanel.BackColor = Color.White;
            this.tableLayoutContentsEquipmentPanel.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            tableLayoutContentsEquipmentPanel.Width = 10;
            tableLayoutContentsEquipmentPanel.Dock = DockStyle.Fill;
            tableLayoutContentsEquipmentPanel.Location = new Point(0, 0);
            tableLayoutContentsEquipmentPanel.Anchor = AnchorStyles.None;
            tableLayoutContentsEquipmentPanel.AutoSize = false;
            tableLayoutContentsEquipmentPanel.RowCount = 3;
            tableLayoutContentsEquipmentPanel.ColumnCount = 2;
            tableLayoutContentsEquipmentPanel.ColumnStyles.Clear();
            tableLayoutContentsEquipmentPanel.RowStyles.Clear();

            for (int i = 0; i < 3; i++)
                tableLayoutContentsEquipmentPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / 3));
            for (int i = 0; i < 2; i++)
                tableLayoutContentsEquipmentPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 2));
        }

        private void SetControlValue()
        {
            // PictureBox 생성 및 리소스 이미지 할당
            var pictureBox = new PictureBox
            {
                Image = Properties.Resources.Logo,
                SizeMode = PictureBoxSizeMode.Zoom,
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            // (0,0)에 추가하고 2행 병합 (즉, (0,0)~(0,1) 병합)
            tableLayoutContentsEquipmentPanel.Controls.Add(pictureBox, 0, 0);
            tableLayoutContentsEquipmentPanel.SetRowSpan(pictureBox, 2);

            // (0, 0) Date Label
            _machineName = new Label
            {
                Text = "Machine Name",
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                Font = new Font("Arial", _labelSize, FontStyle.Bold),
                AutoSize = false,
                Size = new Size(5, 5)

            };
            tableLayoutContentsEquipmentPanel.Controls.Add(_machineName, 0, 2);

            // (1,0) Date Label
            _dateLabel = new Label
            {
                Text = "",
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                Font = new Font("Arial", _labelSize, FontStyle.Bold),
                AutoSize = false,
                Size = new Size(5, 5)

            };
            tableLayoutContentsEquipmentPanel.Controls.Add(_dateLabel, 1, 0);

            // (1,1) Time Label
            _timeLabel = new Label
            {
                Text = "",
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                Font = new Font("Arial", _labelSize, FontStyle.Bold)
            };
            tableLayoutContentsEquipmentPanel.Controls.Add(_timeLabel, 1, 1);

            // (1,2) BuildVer Label
            _buildVerLabel = new Label
            {
                Text = "Build Ver.",
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                Font = new Font("Arial", _labelSize, FontStyle.Bold)
            };
            tableLayoutContentsEquipmentPanel.Controls.Add(_buildVerLabel, 1, 2);
        }

        private void SetTopContentsEquipmentValue(string machineName, string buildVersion)
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
            // 비율 적용
            int panelWidth = (int)(width * 1.0);
            int panelHeight = (int)(height * 0.9);

            // tableLayoutMenuButtonPanel 크기 조정
            this.Size = new Size(panelWidth, panelHeight);
            tableLayoutContentsEquipmentPanel.Size = new Size(panelWidth, panelHeight);

            // 좌측 정렬, 위아래 중앙 정렬
            int x = 0; // 좌측
            int y = (this.Height - tableLayoutContentsEquipmentPanel.Height) / 2; // 위아래 중앙
            tableLayoutContentsEquipmentPanel.Location = new Point(x, y);

            // 필요시 레이아웃 갱신
            tableLayoutContentsEquipmentPanel.Invalidate();
            this.Invalidate();
        }
        #endregion

        #region EventHandler

        #endregion
    }
}
