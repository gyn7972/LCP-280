using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace QMC.Common
{
    public partial class ListBoxItemsView : UserControl
    {
        private int _borderWidth = 2;
        private Color _borderColor = Color.Black;
        private ListBox listBox;
        private GroupBox groupBox; // PropertyCollectionView처럼 private 필드로 선언

        private string _fontFamily = "맑은 고딕";
        private float _fontSize = 10f;
        private string _groupName = "Group Title";

        // 색상 테마 (디자이너에서 설정 가능하도록 노출)
        private Color _listBackColor = Color.White;
        private Color _listForeColor = Color.Black;
        private Color _itemBackColor = Color.White;
        private Color _itemForeColor = Color.Black;
        private Color _selectedBackColor = Color.Yellow;
        private Color _selectedForeColor = Color.Black;
        private Color _groupBackColor = Color.White;
        private Color _groupForeColor = Color.Black;

        public event EventHandler<int> ItemSelected;

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

        [Browsable(true)]
        [Category("Appearance")]
        [Description("ListBox 테두리 색상")]
        public Color BorderColor
        {
            get => _borderColor;
            set { _borderColor = value; listBox?.Invalidate(); }
        }

        // 리스트/아이템/선택 색상 (디자이너에서 설정 가능)
        [Browsable(true)]
        [Category("Appearance")]
        [Description("리스트 박스 배경색")]
        public Color ListBackColor
        {
            get => _listBackColor;
            set { _listBackColor = value; if (listBox != null) listBox.BackColor = value; Invalidate(); }
        }

        [Browsable(true)]
        [Category("Appearance")]
        [Description("리스트 박스 글자색")]
        public Color ListForeColor
        {
            get => _listForeColor;
            set { _listForeColor = value; if (listBox != null) listBox.ForeColor = value; Invalidate(); }
        }

        [Browsable(true)]
        [Category("Appearance")]
        [Description("아이템 배경색 (선택되지 않은 상태)")]
        public Color ItemBackColor
        {
            get => _itemBackColor;
            set { _itemBackColor = value; Invalidate(); }
        }

        [Browsable(true)]
        [Category("Appearance")]
        [Description("아이템 글자색 (선택되지 않은 상태)")]
        public Color ItemForeColor
        {
            get => _itemForeColor;
            set { _itemForeColor = value; Invalidate(); }
        }

        [Browsable(true)]
        [Category("Appearance")]
        [Description("선택된 아이템 배경색")]
        public Color SelectedBackColor
        {
            get => _selectedBackColor;
            set { _selectedBackColor = value; Invalidate(); }
        }

        [Browsable(true)]
        [Category("Appearance")]
        [Description("선택된 아이템 글자색")]
        public Color SelectedForeColor
        {
            get => _selectedForeColor;
            set { _selectedForeColor = value; Invalidate(); }
        }

        [Browsable(true)]
        [Category("Appearance")]
        [Description("GroupBox 배경색")]
        public Color GroupBackColor
        {
            get => _groupBackColor;
            set { _groupBackColor = value; if (groupBox != null) groupBox.BackColor = value; Invalidate(); }
        }

        [Browsable(true)]
        [Category("Appearance")]
        [Description("GroupBox 글자색")]
        public Color GroupForeColor
        {
            get => _groupForeColor;
            set { _groupForeColor = value; if (groupBox != null) groupBox.ForeColor = value; Invalidate(); }
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

        // 🔧 새로운 생성자: 그룹 이름과 크기를 함께 지정
        public ListBoxItemsView(string groupName, Size size)
        {
            InitializeComponent();
            InitializeUserControl(groupName, size);
        }

        // 🔧 새로운 생성자: 크기만 지정 (기본 그룹 이름 사용)
        public ListBoxItemsView(Size size)
        {
            InitializeComponent();
            InitializeUserControl("Group Title", size);
        }

        // PropertyCollectionView의 InitializeComponentUser와 동일한 초기화 메서드
        private void InitializeUserControl(string groupName)
        {
            ApplyGreenOnBlackTheme();
            InitializeUserControl(groupName, Size.Empty);
        }

        // 🔧 크기를 받는 InitializeUserControl 오버로드
        private void InitializeUserControl(string groupName, Size controlSize)
        {
            _groupName = groupName;

            // 🔧 Designer에서 설정된 크기가 있는지 확인 (Size.Empty가 아닌 경우)
            if (!controlSize.IsEmpty)
            {
                this.Size = controlSize;
                Console.WriteLine($"🔧 ListBoxItemsView 생성자에서 크기 설정: {controlSize}");
            }
            else if (this.Size != Size.Empty && this.Size != new Size(150, 150)) // 기본 크기가 아닌 경우
            {
                // Designer에서 이미 크기가 설정되어 있는 경우 그 크기 사용
                controlSize = this.Size;
                Console.WriteLine($"🔧 Designer에서 설정된 크기 감지: {controlSize}");
            }

            // GroupBox 생성 및 설정
            groupBox = new GroupBox
            {
                Text = groupName,
                Font = new Font("맑은 고딕", 10f, FontStyle.Regular),
                ForeColor = _groupForeColor,
                BackColor = _groupBackColor,
                Padding = new Padding(8, 8, 8, 8)
            };

            // 🔧 항상 Dock.Fill을 사용하여 UserControl 크기에 자동으로 맞춤
            groupBox.Dock = DockStyle.Fill;
            Console.WriteLine($"🔧 GroupBox를 Dock.Fill로 설정하여 UserControl 크기에 자동 맞춤");

            // UserControl에 GroupBox 추가
            this.Controls.Add(groupBox);

            // ListBox 초기화
            InitializeListBox();

            Console.WriteLine($"🔧 ListBoxItemsView 초기화: UserControl={this.Size}, GroupBox={groupBox.Size}");
            Console.WriteLine($"   Parent: {this.Parent?.GetType().Name ?? "null"}");
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
                Font = new Font(_fontFamily, _fontSize, FontStyle.Regular),
                BackColor = _listBackColor,
                ForeColor = _listForeColor
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

        // 디자이너에서 크기 조정시 로그 출력 (PropertyCollectionView와 동일)
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            
            Console.WriteLine($"🔧 ListBoxItemsView OnResize: UserControl={this.Size}, DesignMode={this.DesignMode}");
            if (groupBox != null)
            {
                Console.WriteLine($"   GroupBox 크기={groupBox.Size}, Dock={groupBox.Dock}");
                Console.WriteLine($"   GroupBox Location={groupBox.Location}");
                
                // 🔧 GroupBox가 Dock.Fill이 아닌 경우 수동으로 크기 동기화
                if (groupBox.Dock != DockStyle.Fill)
                {
                    groupBox.Size = this.ClientSize;
                    groupBox.Location = new Point(0, 0);
                    Console.WriteLine($"   📐 GroupBox 크기를 UserControl.ClientSize로 수동 동기화: {this.ClientSize}");
                }
            }
            if (listBox != null)
            {
                Console.WriteLine($"   ListBox 크기={listBox.Size}, Dock={listBox.Dock}");
            }
            Console.WriteLine($"   Parent: {this.Parent?.GetType().Name ?? "null"}");
            Console.WriteLine($"   Parent Size: {this.Parent?.Size.ToString() ?? "null"}");
            
            // 내부 컨트롤들 갱신
            if (listBox != null)
            {
                listBox.Invalidate();
            }
            if (groupBox != null)
            {
                groupBox.Invalidate();
            }
        }

        // Designer에서 크기 설정시 로그 출력 (PropertyCollectionView와 동일)
        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            base.SetBoundsCore(x, y, width, height, specified);
            
            if ((specified & BoundsSpecified.Size) != 0)
            {
                Console.WriteLine($"🔧 ListBoxItemsView SetBoundsCore: Size=({width}, {height}), DesignMode={this.DesignMode}");
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

        // 선택 인덱스 배경 노란색 -> 사용자 지정 색상으로 변경
        private void ListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || listBox == null) return;

            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

            using (var back = new SolidBrush(selected ? _selectedBackColor : _itemBackColor))
            {
                e.Graphics.FillRectangle(back, e.Bounds);
            }

            using (var brush = new SolidBrush(selected ? _selectedForeColor : _itemForeColor))
            {
                e.Graphics.DrawString(
                    listBox.Items[e.Index].ToString(),
                    listBox.Font,
                    brush,
                    e.Bounds.Left,
                    e.Bounds.Top);
            }

            // 테두리 그리기 (컨트롤 전체 테두리 1회만 그리면 좋지만, 간단히 매 아이템에서 덮어 그림)
            using (var pen = new Pen(_borderColor, _borderWidth))
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

#if DEBUG
            if (listBox.SelectedIndex >= 0)
            {
                var item = listBox.Items[listBox.SelectedIndex];
                string name = item as string;

                if (name == null && item != null)
                {
                    var prop = item.GetType().GetProperty("Name");
                    if (prop != null)
                        name = prop.GetValue(item)?.ToString();
                }

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

        // GroupBox 크기 정보 확인용 프로퍼티 추가
        [Browsable(false)]
        public Size GroupBoxSize => groupBox?.Size ?? Size.Empty;

        [Browsable(false)]
        public Size UserControlSize => this.Size;

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

        // GroupBox Size를 외부에서 설정할 수 있도록 public 메서드 추가
        public void SetGroupBoxSize(Size size)
        {
            Console.WriteLine($"🔧 ListBoxItemsView.SetGroupBoxSize 호출: 요청된 크기={size}");
            Console.WriteLine($"   현재 UserControl 크기={this.Size}");
            Console.WriteLine($"   현재 GroupBox 크기={(groupBox?.Size.ToString() ?? "null")} ");
            Console.WriteLine($"   현재 GroupBox Dock={groupBox?.Dock}");
            
            if (groupBox != null)
            {
                // 🔧 UserControl 크기 변경 - GroupBox는 Dock.Fill이므로 자동으로 따라옴
                this.Size = size;
                
                // 🔧 GroupBox가 Dock.Fill이 아닌 경우 수동으로 크기 설정
                if (groupBox.Dock != DockStyle.Fill)
                {
                    groupBox.Size = size;
                    groupBox.Location = new Point(0, 0);
                    Console.WriteLine($"🔧 GroupBox Dock이 Fill이 아니므로 수동으로 크기 설정");
                }
                
                // 강제 레이아웃 업데이트
                this.SuspendLayout();
                groupBox.SuspendLayout();
                
                // 레이아웃 강제 업데이트
                groupBox.PerformLayout();
                this.PerformLayout();
                
                this.ResumeLayout(true);
                groupBox.ResumeLayout(true);
                
                // 화면 갱신
                groupBox.Invalidate();
                this.Invalidate();
                
                Console.WriteLine($"✅ SetGroupBoxSize 완료:");
                Console.WriteLine($"   UserControl 최종 크기={this.Size}");
                Console.WriteLine($"   GroupBox 최종 크기={groupBox.Size}");
                Console.WriteLine($"   GroupBox Dock={groupBox.Dock}");
            }
            else
            {
                Console.WriteLine($"❌ GroupBox가 null입니다.");
            }
        }

        // ▼ 추가: 현재 ListBox 항목 문자열 배열 반환
        public string[] GetItems()
        {
            if (listBox == null) return Array.Empty<string>();
            return listBox.Items.Cast<object>()
                         .Select(o => o?.ToString() ?? string.Empty)
                         .ToArray();
        }

        // 현재 선택된 아이템 이름 가져오기
        [Browsable(false)]
        public string SelectedItemName
        {
            get
            {
                if (listBox == null || listBox.SelectedIndex < 0)
                    return string.Empty;

                var item = listBox.Items[listBox.SelectedIndex];

                // string이면 그대로 반환
                if (item is string s)
                    return s;

                // Name 속성이 있으면 그 값 반환
                var prop = item.GetType().GetProperty("Name");
                if (prop != null)
                    return prop.GetValue(item)?.ToString() ?? string.Empty;

                // fallback: ToString()
                return item?.ToString() ?? string.Empty;
            }
        }

        // ===== 편의 메서드: Green on Black 테마 적용 =====
        public void ApplyGreenOnBlackTheme()
        {
            // GroupBox
            GroupBackColor = Color.White;
            GroupForeColor = Color.Black;

            // ListBox 및 아이템/선택 색
            ListBackColor = Color.Black;
            ListForeColor = Color.Lime;
            ItemBackColor = Color.Black;
            ItemForeColor = Color.Lime;
            SelectedBackColor = Color.FromArgb(198, 255, 0);
            SelectedForeColor = Color.Black;
            BorderColor = Color.White;
            BorderWidth = 1;
        }
    }
}
