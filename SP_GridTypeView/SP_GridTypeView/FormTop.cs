using System;
using System.Drawing;
using System.Windows.Forms;

namespace SP_GridTypeView
{
    public partial class FormTop : Form
    {
        private TopContentsEquipmentControl _topContentsEquipmentControl;
        private TopContentsStatusControl _topContentsStatusControl;
        private TopContentsLoginModeControl _topContentsLoginModeControl;
        private TableLayoutPanel _tableLayoutPanelFormTop;

        public FormTop()
        {
            InitializeComponent();

            this.BackColor = Color.White; // 폼 배경색을 하얀색으로 설정

            // TableLayoutPanel 생성 및 설정
            _tableLayoutPanelFormTop = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 1,
                ColumnCount = 3,
            };
            _tableLayoutPanelFormTop.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 230));
            _tableLayoutPanelFormTop.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 880));
            _tableLayoutPanelFormTop.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            _tableLayoutPanelFormTop.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            // 컨트롤 생성
            _topContentsEquipmentControl = new TopContentsEquipmentControl();
            _topContentsEquipmentControl.Dock = DockStyle.Left;
            _topContentsEquipmentControl.Margin = new Padding(0);

            _topContentsStatusControl = new TopContentsStatusControl();
            _topContentsStatusControl.Dock = DockStyle.Left;
            _topContentsStatusControl.Margin = new Padding(0);

            _topContentsLoginModeControl = new TopContentsLoginModeControl();
            _topContentsLoginModeControl.Dock = DockStyle.Left;
            _topContentsLoginModeControl.Margin = new Padding(0);

            // TableLayoutPanel에 컨트롤 추가 (왼쪽: 0, 오른쪽: 1)
            _tableLayoutPanelFormTop.Controls.Add(_topContentsEquipmentControl, 0, 0);
            _tableLayoutPanelFormTop.Controls.Add(_topContentsStatusControl, 1, 0);
            _tableLayoutPanelFormTop.Controls.Add(_topContentsLoginModeControl, 2, 0);

            this.Controls.Add(_tableLayoutPanelFormTop);
            _topContentsStatusControl.ClickTopAlarmClearButton += GetTopContentsStatusControl_ClickTopAlarmClearButton;
        }

        private void GetTopContentsStatusControl_ClickTopAlarmClearButton()
        {
            MessageBox.Show("Alarm Clear");
        }

        private void AlignMenuControl()
        {
            // 좌측(0), 위아래 가운데 정렬
            int x = 0;
            int y = (this.ClientSize.Height - _topContentsEquipmentControl.Height) / 2;
            _topContentsEquipmentControl.Location = new System.Drawing.Point(x, y);
        }

        public void SetPanelSize(int width, int height)
        {
            this.Width = width;
            this.Height = height;
            this.ClientSize = new System.Drawing.Size(width, height);

            // TableLayoutPanel의 각 열의 실제 크기 가져오기
            int[] colWidths = _tableLayoutPanelFormTop.GetColumnWidths();

            // 각 컨트롤에 실제 열 크기와 전체 높이 전달
            _topContentsEquipmentControl.SetPanelSize(colWidths[0], height);
            _topContentsStatusControl.SetPanelSize(colWidths[1], height);
            _topContentsLoginModeControl.SetPanelSize(colWidths[2], height);

            //AlignMenuControl();
        }
    }
}
