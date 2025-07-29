using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SP_GridTypeView
{
    public partial class ListBoxItemsView : UserControl
    {
        private int _borderWidth = 2;
        private ListBox listBox;

        private string _fontFamily = "맑은 고딕";
        private float _fontSize = 10f;
        private string _groupName = "Group Title";

        public event EventHandler<int> ItemSelected;

        // GroupBox 이름 프로퍼티
        [Browsable(true)]
        [Category("Appearance")]
        [Description("GroupBox 이름")]
        public string GroupName
        {
            get => groupBox.Text;
            set
            {
                _groupName = value;
                if (groupBox != null)
                    groupBox.Text = value;
            }
        }

        // BorderWidth property for designer/code use
        [Browsable(true)]
        [Category("Appearance")]
        [Description("ListBox 테두리 두께")]
        public int BorderWidth
        {
            get => _borderWidth;
            set
            {
                _borderWidth = Math.Max(1, value);
                if (listBox != null)
                    listBox.Invalidate();
            }
        }

        // 생성자: 그룹 이름 지정 가능
        public ListBoxItemsView(string groupName = "Group Title")
        {
            InitializeComponent();
            GroupName = groupName;
            InitializeListBox();
            // GroupBox 스타일을 이미지처럼 보이게 조정
            groupBox.Font = new Font("맑은 고딕", 10f, FontStyle.Regular);
            groupBox.ForeColor = Color.Black;
            groupBox.BackColor = Color.White; // 배경색을 하얀색으로 설정
            groupBox.Padding = new Padding(8, 18, 8, 8); // 제목과 내용 간격 조정
            groupBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        }

        private void InitializeListBox()
        {
            // 기존 ListBox가 있으면 제거
            if (listBox != null && listBox.Parent != null)
                listBox.Parent.Controls.Remove(listBox);

            listBox = new ListBox
            {
                Dock = DockStyle.Fill,
                DrawMode = DrawMode.OwnerDrawVariable,
                Font = new Font(_fontFamily, _fontSize, FontStyle.Regular)
            };
            listBox.DrawItem += ListBox_DrawItem;
            listBox.MeasureItem += (s, e) => {
                e.ItemHeight = (int)Math.Ceiling(_fontSize * 1.8f);
            };
            listBox.SelectedIndexChanged += ListBox_SelectedIndexChanged;

            // groupBox에 ListBox 추가
            groupBox.Controls.Clear();
            groupBox.Controls.Add(listBox);
        }

        // Override OnResize to ensure GroupBox resizes properly
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            
            // GroupBox는 Dock = Fill로 설정되어 있어 자동으로 리사이즈되지만,
            // 추가적인 레이아웃 갱신이 필요한 경우를 위해
            if (groupBox != null)
            {
                groupBox.Size = this.ClientSize;
                groupBox.Invalidate();
            }
        }

        // Override SetBoundsCore to ensure proper sizing behavior
        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            base.SetBoundsCore(x, y, width, height, specified);
            
            // GroupBox 크기 동기화
            if (groupBox != null && (specified & BoundsSpecified.Size) != 0)
            {
                groupBox.Size = new Size(width, height);
            }
        }

        // params string[]로 항목 추가
        public void SetItems(params object[] items)
        {
            listBox.Items.Clear();
            if (items != null && items.Length == 1 && items[0] is Type enumType && enumType.IsEnum)
            {
                foreach (var value in Enum.GetValues(enumType))
                    listBox.Items.Add(value);
            }
            else if (items != null)
            {
                listBox.Items.AddRange(items);
            }
        }

        // 선택 인덱스 배경 노란색
        private void ListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            e.Graphics.FillRectangle(
                selected ? Brushes.Yellow : Brushes.White,
                e.Bounds);

            using (var brush = new SolidBrush(listBox.ForeColor))
            {
                e.Graphics.DrawString(
                    listBox.Items[e.Index].ToString(),
                    listBox.Font,
                    Brushes.Black,
                    e.Bounds.Left,
                    e.Bounds.Top);
            }

            // 테두리 그리기
            using (var pen = new Pen(Color.Black, _borderWidth))
            {
                Rectangle borderRect = listBox.ClientRectangle;
                borderRect.Width -= 1;
                borderRect.Height -= 1;
                e.Graphics.DrawRectangle(pen, borderRect);
            }

            e.DrawFocusRectangle();
        }

        // 선택 이벤트
        private void ListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ItemSelected?.Invoke(this, listBox.SelectedIndex);

            // 선택된 아이템의 이름 추출 및 메시지 박스 표시
            if (listBox.SelectedIndex >= 0)
            {
                var item = listBox.Items[listBox.SelectedIndex];
                string name = item as string;

                // Name 속성이 있으면 사용자
                if (name == null && item != null)
                {
                    var prop = item.GetType().GetProperty("Name");
                    if (prop != null)
                        name = prop.GetValue(item)?.ToString();
                }

                // 그래도 없으면 ToString()
                if (string.IsNullOrEmpty(name) && item != null)
                    name = item.ToString();

                MessageBox.Show($"선택된 항목: {name}", "ListBox 선택", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // 선택된 인덱스 접근
        public int SelectedIndex
        {
            get => listBox.SelectedIndex;
            set => listBox.SelectedIndex = value;
        }

        // 폰트 갱신 메서드
        private void UpdateListBoxFont()
        {
            if (listBox != null)
            {
                listBox.Font = new Font(_fontFamily, _fontSize, FontStyle.Regular);
                listBox.ItemHeight = (int)Math.Ceiling(_fontSize * 1.8f); // 폰트 크기에 따라 조정
            }
            this.Invalidate();
        }
    }
}
