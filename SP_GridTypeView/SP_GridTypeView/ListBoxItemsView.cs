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

        public event EventHandler<int> ItemSelected;
        // 글꼴 설정
        [Browsable(true)]
        public override Font Font
        {
            get => base.Font;
            set
            {
                base.Font = value;
                if (listBox != null)
                    listBox.Font = value;
                // FontFamily, FontSize도 동기화
                _fontFamily = value.FontFamily.Name;
                _fontSize = value.Size;
            }
        }

        
        [Browsable(true)]
        [Category("Appearance")]
        [Description("ListBox 테두리 두께")]
        public int BorderWidth
        {
            get => _borderWidth;
            set
            {
                _borderWidth = Math.Max(1, value);
                listBox.Invalidate();
            }
        }

        [Browsable(true)]
        [Category("Appearance")]
        [Description("ListBox 글씨체")]
        public string FontFamily
        {
            get => _fontFamily;
            set
            {
                _fontFamily = value;
                UpdateListBoxFont();
            }
        }

        [Browsable(true)]
        [Category("Appearance")]
        [Description("ListBox 글씨 크기")]
        public float FontSize
        {
            get => _fontSize;
            set
            {
                _fontSize = value;
                UpdateListBoxFont();
            }
        }

        public ListBoxItemsView()
        {
            InitializeComponent();
            InitializeListBox();
        }

        private void InitializeListBox()
        {
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
            this.Controls.Add(listBox);
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
