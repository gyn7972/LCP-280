using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace QMC.Common
{
    public partial class ListBoxItemsView : UserControl
    {
        private int _borderWidth = 2;
        private ListBox listBox;
        private GroupBox groupBox; // PropertyCollectionView처럼 private 필드로 선언

        private string _fontFamily = "맑은 고딕";
        private float _fontSize = 10f;
        private string _groupName = "Group Title";
        
        // 자동/수동 크기 조정 옵션
        private bool _autoSizeGroupBox = true;
        private Size _manualGroupBoxSize = new Size(200, 150);

        public event EventHandler<int> ItemSelected;

        // GroupBox 자동 크기 조정 옵션
        [Browsable(true)]
        [Category("Layout")]
        [Description("GroupBox 크기를 자동으로 조정할지 여부")]
        [DefaultValue(true)]
        public bool AutoSizeGroupBox
        {
            get => _autoSizeGroupBox;
            set
            {
                if (_autoSizeGroupBox != value)
                {
                    _autoSizeGroupBox = value;
                    UpdateGroupBoxSizeMode();
                }
            }
        }

        // 수동 GroupBox 크기 설정
        [Browsable(true)]
        [Category("Layout")]
        [Description("수동 모드일 때 GroupBox의 크기")]
        public Size ManualGroupBoxSize
        {
            get => _manualGroupBoxSize;
            set
            {
                if (_manualGroupBoxSize != value)
                {
                    _manualGroupBoxSize = value;
                    if (!_autoSizeGroupBox)
                    {
                        UpdateGroupBoxSizeMode();
                    }
                }
            }
        }

        // GroupBox 이름 프로퍼티
        [Browsable(true)]
        [Category("Appearance")]
        [Description("GroupBox 이름")]
        public string GroupName
        {
            get => groupBox?.Text ?? _groupName;
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

        // 기본 생성자 추가 (PropertyCollectionView와 동일)
        public ListBoxItemsView()
        {
            InitializeComponent();
            InitializeUserControl("Group Title");
        }

        // 생성자: 그룹 이름 지정 가능
        public ListBoxItemsView(string groupName = "Group Title")
        {
            InitializeComponent();
            InitializeUserControl(groupName);
        }

        // PropertyCollectionView의 InitializeComponentUser와 유사한 초기화 메서드
        private void InitializeUserControl(string groupName)
        {
            _groupName = groupName;

            // GroupBox 생성 및 설정
            groupBox = new GroupBox
            {
                Text = groupName,
                Font = new Font("맑은 고딕", 10f, FontStyle.Regular),
                ForeColor = Color.Black,
                BackColor = Color.White,
                Padding = new Padding(8, 8, 8, 8)
            };

            // UserControl에 GroupBox 추가
            this.Controls.Add(groupBox);

            // 초기 크기 모드 설정
            UpdateGroupBoxSizeMode();

            // ListBox 초기화
            InitializeListBox();
        }

        // GroupBox 크기 모드 업데이트
        private void UpdateGroupBoxSizeMode()
        {
            if (groupBox == null) return;

            if (_autoSizeGroupBox)
            {
                // 자동 크기 조정 모드 - Designer에서 설정한 UserControl 크기를 GroupBox에 고정 적용
                groupBox.Dock = DockStyle.None; // Dock을 None으로 설정하여 고정 크기 사용
                groupBox.Anchor = AnchorStyles.Top | AnchorStyles.Left; // Anchor를 Top, Left만 설정하여 크기 고정
                groupBox.Location = Point.Empty; // (0, 0)에 위치
                groupBox.Size = this.Size; // UserControl의 전체 크기와 동일하게 고정 설정
            }
            else
            {
                // 수동 크기 조정 모드
                groupBox.Dock = DockStyle.None;
                groupBox.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                groupBox.Size = _manualGroupBoxSize;
                groupBox.Location = Point.Empty;
                // UserControl 크기도 수동 크기에 맞춤
                this.Size = _manualGroupBoxSize;
            }
            
            groupBox.Invalidate();
            this.Invalidate();
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
            if (groupBox != null)
            {
                groupBox.Controls.Clear();
                groupBox.Controls.Add(listBox);
            }
        }

        // Override OnResize to ensure GroupBox resizes properly
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            // 자동 크기 조정 모드일 때는 Designer에서 설정한 크기 유지
            if (_autoSizeGroupBox && groupBox != null)
            {
                // UserControl의 전체 크기를 GroupBox에 고정 적용
                groupBox.Size = this.Size;
                groupBox.Location = Point.Empty; // 항상 (0, 0)에 위치
                groupBox.Invalidate();
                // ListBox도 함께 업데이트
                if (listBox != null)
                {
                    listBox.Invalidate();
                }
            }
        }

        // Override SetBoundsCore to ensure proper sizing behavior
        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            base.SetBoundsCore(x, y, width, height, specified);
            // 자동 크기 조정 모드일 때 Designer에서 설정한 크기를 GroupBox에 고정 적용
            if (_autoSizeGroupBox && groupBox != null && (specified & BoundsSpecified.Size) != 0)
            {
                // UserControl의 전체 크기를 GroupBox에 적용
                groupBox.Size = new Size(width, height);
                groupBox.Location = Point.Empty; // GroupBox를 UserControl의 왼쪽 상단에 위치
                groupBox.Invalidate();
            }
        }

        // params string[]로 항목 추가
        public void SetItems(params object[] items)
        {
            if (listBox == null) return;

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
            if (e.Index < 0 || listBox == null) return;

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
            if (listBox == null) return;

            // 🚀 Position Item 선택 이벤트 발생
            ItemSelected?.Invoke(this, listBox.SelectedIndex);

            // 🚀 디버그 모드에서만 선택된 아이템 정보 출력
#if DEBUG
            if (listBox.SelectedIndex >= 0)
            {
                var item = listBox.Items[listBox.SelectedIndex];
                string name = item as string;

                // Name 속성이 있으면 사용
                if (name == null && item != null)
                {
                    var prop = item.GetType().GetProperty("Name");
                    if (prop != null)
                        name = prop.GetValue(item)?.ToString();
                }

                // 그래도 없으면 ToString()
                if (string.IsNullOrEmpty(name) && item != null)
                    name = item.ToString();

                Console.WriteLine($"📍 Position Item 선택: {name} (인덱스: {listBox.SelectedIndex})");
            }
#endif
        }

        // 선택된 인덱스 접근
        public int SelectedIndex
        {
            get => listBox?.SelectedIndex ?? -1;
            set
            {
                if (listBox != null)
                    listBox.SelectedIndex = value;
            }
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
