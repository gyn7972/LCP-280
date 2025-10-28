using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.ComponentModel; // for Browsable, Category, Description attributes

namespace QMC.Common
{
    public partial class IOPropertyCollectionView : PropertyCollectionView
    {
        // === 내부 상태 ===
        private readonly Dictionary<string, PictureBox> _statePicByKey = new Dictionary<string, PictureBox>(StringComparer.OrdinalIgnoreCase);
        public event EventHandler<string> ItemClicked;
        public event EventHandler<string> ItemRightClicked;

        private Label _selectedNameLabel = null;
        private int IOMaxVisibleRows = 10;
        private const int IOGroupBoxHeaderHeight = 20;
        private const int IOGroupBoxPadding = 16;

        // 키 숫자부 자릿수 (예: X003 -> 3). UI 구성 시 실제 키들 보고 자동 설정됨.
        private int _keyPadWidth = 3;

        // ===== 1번 열(Name 열) 색상 커스터마이징 =====
        private Color _listBackColor = Color.Black;           // 기본 배경
        private Color _listForeColor = Color.Lime;            // 기본 글자색
        private Color _selectedBackColor = Color.FromArgb(198, 255, 0); // 선택 배경
        private Color _selectedForeColor = Color.Black;       // 선택 글자색

        [Browsable(true)]
        [Category("Appearance")]
        [Description("1번 열(Name) 기본 배경색")] 
        public Color ListBackColor
        {
            get => _listBackColor;
            set { _listBackColor = value; UpdateNameColumnColors(); }
        }

        [Browsable(true)]
        [Category("Appearance")]
        [Description("1번 열(Name) 기본 글자색")] 
        public Color ListForeColor
        {
            get => _listForeColor;
            set { _listForeColor = value; UpdateNameColumnColors(); }
        }

        [Browsable(true)]
        [Category("Appearance")]
        [Description("1번 열(Name) 선택 배경색")] 
        public Color SelectedBackColor
        {
            get => _selectedBackColor;
            set { _selectedBackColor = value; UpdateNameColumnColors(); }
        }

        [Browsable(true)]
        [Category("Appearance")]
        [Description("1번 열(Name) 선택 글자색")] 
        public Color SelectedForeColor
        {
            get => _selectedForeColor;
            set { _selectedForeColor = value; UpdateNameColumnColors(); }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                // 폼 전체 컴포지팅 (깜빡임 줄이기)
                cp.ExStyle |= 0x02000000; // WS_EX_COMPOSITED
                return cp;
            }
        }

        // ====== 캐시(빠른 재바인딩) ======
        private readonly List<Label> _nameLabelsCache = new List<Label>();
        private readonly List<PictureBox> _statePicsCache = new List<PictureBox>();
        private readonly List<string> _rowDispCache = new List<string>(); // Name 셀 전체 텍스트(표시용)

        // 빠른 초기 페인트: true면 SetProperties에서 상태색 적용 생략하고 다음 Refresh에서 채움
        [Browsable(false)]
        public bool FastInitialPaint { get; set; } = true;

        public IOPropertyCollectionView(string groupName = "IO Property Group", int nRow = 10) : base(groupName)
        {
            InitializeComponent();

            IOMaxVisibleRows = nRow;

            // 이 컨트롤 자체
            EnableFlickerFree(this);

            // 비공개 필드 꺼내기
            var tlp = (TableLayoutPanel)typeof(PropertyCollectionView)
                .GetField("tableLayoutPanel", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(this);
            var scroll = (Panel)typeof(PropertyCollectionView)
                .GetField("scrollPanel", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(this);

            // 내부 컨트롤에도 적용
            EnableFlickerFree(tlp);
            EnableFlickerFree(scroll);
        }

        public IOPropertyCollectionView() : this("IO Property Group")
        {
            // (옵션) 깜빡임 줄이기
            this.DoubleBuffered = true;
        }

        // 깜빡임 최소화 공통 함수 (리플렉션으로 protected 멤버 호출)
        private static void EnableFlickerFree(Control c)
        {
            if (c == null) return;

            // 1) DoubleBuffered 강제 (protected 속성)
            var piDB = typeof(Control).GetProperty(
                "DoubleBuffered",
                BindingFlags.NonPublic | BindingFlags.Instance
            );
            piDB?.SetValue(c, true, null);

            // 2) SetStyle(ControlStyles, bool) 호출
            var miSetStyle = typeof(Control).GetMethod(
                "SetStyle",
                BindingFlags.NonPublic | BindingFlags.Instance
            );
            var styles =
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.UserPaint;
            miSetStyle?.Invoke(c, new object[] { styles, true });

            // 3) UpdateStyles() 호출
            var miUpdateStyles = typeof(Control).GetMethod(
                "UpdateStyles",
                BindingFlags.NonPublic | BindingFlags.Instance
            );
            miUpdateStyles?.Invoke(c, null);
        }

        // 1번 열 색상 즉시 반영
        private void UpdateNameColumnColors()
        {
            try
            {
                var tableLayoutPanelField = typeof(PropertyCollectionView).GetField("tableLayoutPanel", BindingFlags.NonPublic | BindingFlags.Instance);
                var tableLayoutPanel = tableLayoutPanelField?.GetValue(this) as TableLayoutPanel;
                if (tableLayoutPanel == null) return;

                foreach (Control ctrl in tableLayoutPanel.Controls)
                {
                    var lbl = ctrl as Label;
                    if (lbl == null) continue;
                    if (!(lbl.Tag is string tag) || tag != "IONameLabel") continue;

                    if (ReferenceEquals(lbl, _selectedNameLabel))
                    {
                        lbl.BackColor = _selectedBackColor;
                        lbl.ForeColor = _selectedForeColor;
                    }
                    else
                    {
                        lbl.BackColor = _listBackColor;
                        lbl.ForeColor = _listForeColor;
                    }
                }
                tableLayoutPanel.Invalidate();
            }
            catch { /* ignore */ }
        }

        public override void SetProperties(PropertyCollection properties)
        {
            _statePicByKey.Clear();

            // base의 필드 참조 (PropertyCollectionView와 동일한 방식)
            var tableLayoutPanelField = typeof(PropertyCollectionView).GetField("tableLayoutPanel", BindingFlags.NonPublic | BindingFlags.Instance);
            var scrollPanelField = typeof(PropertyCollectionView).GetField("scrollPanel", BindingFlags.NonPublic | BindingFlags.Instance);
            var textBoxFontField = typeof(PropertyCollectionView).GetField("_textBoxFont", BindingFlags.NonPublic | BindingFlags.Instance);
            var groupBoxField = typeof(PropertyCollectionView).GetField("groupBox", BindingFlags.NonPublic | BindingFlags.Instance);

            var tableLayoutPanel = tableLayoutPanelField.GetValue(this) as TableLayoutPanel;
            var scrollPanel = scrollPanelField.GetValue(this) as Panel;
            var textBoxFont = textBoxFontField.GetValue(this) as Font;
            var groupBox = groupBoxField.GetValue(this) as GroupBox;

            // Try fast in-place update (구조 동일 시 값/색만 갱신)
            if (TryUpdateInPlace(properties, tableLayoutPanel, textBoxFont))
            {
                scrollPanel.AutoScroll = true;
                return;
            }

            // 초기화(풀 리빌드)
            tableLayoutPanel.SuspendLayout();
            tableLayoutPanel.Controls.Clear();
            tableLayoutPanel.RowStyles.Clear();
            tableLayoutPanel.RowCount = 0;
            _nameLabelsCache.Clear();
            _statePicsCache.Clear();
            _rowDispCache.Clear();

            if (properties == null)
            {
                // 크기 변경하지 않음(디자이너 설정 유지). 스크롤만 초기화
                scrollPanel.AutoScroll = true;
                scrollPanel.AutoScrollMinSize = Size.Empty;
                tableLayoutPanel.ResumeLayout();
                return;
            }

            int textBoxHeight = TextRenderer.MeasureText("A", textBoxFont).Height + 8;

            // TitleOnlyProperty와 PropertyState 처리
            var headerProp = properties.OfType<TitleOnlyProperty>().FirstOrDefault();
            var stateProps = properties.OfType<PropertyState>().ToList();
            bool hasNoColumn = stateProps.Any(p => properties.ShowNoColumn);
            int colCount = hasNoColumn ? 3 : 2;

            // 열 개수 및 스타일
            tableLayoutPanel.ColumnCount = colCount;
            tableLayoutPanel.ColumnStyles.Clear();
            if (colCount == 2)
            {
                //tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 80F));
                //tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
                tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 90F));
                tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            }
            else
            {
                //tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15F));
                //tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
                //tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15F));
                tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
                tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 80F));
                tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            }

            int row = 0;

            // === 키 패딩 폭 계산 (UI 표시 문자열 기준) ===
            try
            {
                var widths = new List<int>();
                foreach (var prop in stateProps)
                {
                    var txt = (prop.Value?.ToString() ?? "").Trim().ToUpperInvariant();
                    var m = Regex.Match(txt, @"^(X|Y)\s*0*(\d+)\b");
                    if (m.Success)
                    {
                        var digits = m.Groups[2].Value;
                        widths.Add(Math.Max(1, digits.Length));
                    }
                }
                _keyPadWidth = widths.Count > 0 ? widths.Max() : 3;
            }
            catch
            {
                _keyPadWidth = 3;
            }

            // 헤더 행
            if (headerProp != null)
            {
                tableLayoutPanel.RowCount++;
                tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, textBoxHeight));

                for (int i = 0; i < colCount; i++)
                {
                    var titleLabel = new Label
                    {
                        Text = i < headerProp.Titles.Length ? headerProp.Titles[i] : "",
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleLeft,
                        AutoSize = false,
                        Margin = new Padding(0),
                        Padding = new Padding(0),
                        Font = new Font(textBoxFont.FontFamily, textBoxFont.Size, FontStyle.Bold),
                        BackColor = Color.LightGray
                    };
                    tableLayoutPanel.Controls.Add(titleLabel, i, row);
                }
                row++;
            }

            // 데이터 행
            foreach (var prop in stateProps)
            {
                tableLayoutPanel.RowCount++;
                tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, textBoxHeight));

                int colIdx = 0;
                Label nameLabel = null;

                if (colCount == 3 && prop.ShowNoColumn)
                {
                    // 0번 열(No)
                    var noLabel = new Label
                    {
                        Text = prop.Title,
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleLeft,
                        AutoSize = false,
                        Margin = new Padding(0),
                        Padding = new Padding(0),
                        BackColor = Color.LightGray
                    };
                    tableLayoutPanel.Controls.Add(noLabel, colIdx++, row);

                    // 1번 열(Name)
                    nameLabel = new Label
                    {
                        Text = prop.Value?.ToString() ?? "",
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleLeft,
                        AutoSize = false,
                        Margin = new Padding(0),
                        Padding = new Padding(2),
                        BackColor = _listBackColor,
                        ForeColor = _listForeColor,
                        Tag = "IONameLabel",
                        AutoEllipsis = true
                    };
                    _nameToolTip.SetToolTip(nameLabel, nameLabel.Text);
                    nameLabel.Click += (s, e) =>
                    {
                        if (_selectedNameLabel != null)
                        {
                            _selectedNameLabel.BackColor = _listBackColor;
                            _selectedNameLabel.ForeColor = _listForeColor;
                        }
                        nameLabel.BackColor = _selectedBackColor;
                        nameLabel.ForeColor = _selectedForeColor;
                        _selectedNameLabel = nameLabel;
                    };
                    tableLayoutPanel.Controls.Add(nameLabel, colIdx++, row);
                }
                else
                {
                    // 0번 열(Name)부터
                    nameLabel = new Label
                    {
                        Text = prop.Value?.ToString() ?? "",
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleLeft,
                        AutoSize = false,
                        Margin = new Padding(0),
                        Padding = new Padding(2),
                        BackColor = _listBackColor,
                        ForeColor = _listForeColor,
                        Tag = "IONameLabel",
                        AutoEllipsis = true
                    };
                    _nameToolTip.SetToolTip(nameLabel, nameLabel.Text);

                    nameLabel.Click += (s, e) =>
                    {
                        if (_selectedNameLabel != null)
                        {
                            _selectedNameLabel.BackColor = _listBackColor;
                            _selectedNameLabel.ForeColor = _listForeColor;
                        }
                        nameLabel.BackColor = _selectedBackColor;
                        nameLabel.ForeColor = _selectedForeColor;
                        _selectedNameLabel = nameLabel;
                    };
                    tableLayoutPanel.Controls.Add(nameLabel, colIdx++, row);
                }

                var key = ExtractKey(nameLabel.Text);            // 이미 NormalizeKey 적용됨
                var normKey = key;                               // 가독성

                // --- State 셀(PictureBox) ---
                var statePictureBox = new PictureBox
                {
                    Dock = DockStyle.Fill,
                    Margin = new Padding(0),
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    BorderStyle = BorderStyle.None,
                    BackColor = Color.Empty,
                    MinimumSize = new Size(0, textBoxHeight),
                    Height = textBoxHeight
                };

                if (!FastInitialPaint)
                {
                    ApplyStateColor(statePictureBox, normKey, prop.State);
                }

                // 🔹 맵 등록 + 이벤트
                if (!string.IsNullOrEmpty(normKey))
                {
                    _statePicByKey[normKey] = statePictureBox;

                    statePictureBox.Tag = normKey;
                    statePictureBox.Click += (s, e) =>
                    {
                        var k = (string)((PictureBox)s).Tag;
                        ItemClicked?.Invoke(this, k);
                    };
                    statePictureBox.MouseUp += (s, e) =>
                    {
                        if (e.Button == MouseButtons.Right)
                        {
                            var k = (string)((PictureBox)s).Tag;
                            ItemRightClicked?.Invoke(this, k);
                        }
                    };
                }

                tableLayoutPanel.Controls.Add(statePictureBox, colIdx, row);

                // 캐시
                _nameLabelsCache.Add(nameLabel);
                _statePicsCache.Add(statePictureBox);
                _rowDispCache.Add(nameLabel.Text ?? string.Empty);

                row++;
            }

            // 동적 컨텐츠 높이 계산 (컨트롤 자체 크기는 디자이너 설정 유지)
            int totalRows = (headerProp != null ? 1 : 0) + stateProps.Count;
            int contentHeight = totalRows * textBoxHeight;
            tableLayoutPanel.Height = contentHeight;

            // 스크롤 사용: 화면보다 컨텐츠가 커지면 자동으로 V-스크롤 노출
            scrollPanel.AutoScroll = true; // 항상 켜둠

            // 그룹박스 패딩 등을 고려한 AutoScrollMinSize 설정
            int verticalPadding = IOGroupBoxHeaderHeight + groupBox.Padding.Top + IOGroupBoxPadding;
            scrollPanel.AutoScrollMinSize = new Size(0, tableLayoutPanel.PreferredSize.Height + verticalPadding);

            if (scrollPanel.VerticalScroll.Maximum > 0)
                scrollPanel.VerticalScroll.Value = 0;

            tableLayoutPanel.ResumeLayout();

            // 부모 컨트롤에게 레이아웃 갱신 알림 (크기는 변경하지 않음)
            this.Invalidate();
            this.Parent?.PerformLayout();
        }

        // ====== 빠른 재바인딩: 구조 동일 시 컨트롤 재사용 ======
        private bool TryUpdateInPlace(PropertyCollection properties, TableLayoutPanel tableLayoutPanel, Font textBoxFont)
        {
            try
            {
                if (properties == null) return false;
                var stateProps = properties.OfType<PropertyState>().ToList();
                if (_nameLabelsCache.Count == 0 || _statePicsCache.Count == 0) return false;
                if (_nameLabelsCache.Count != stateProps.Count) return false;

                // 새 라벨 텍스트 시퀀스
                var newDisp = stateProps.Select(p => p.Value?.ToString() ?? string.Empty).ToList();
                if (newDisp.Count != _rowDispCache.Count) return false;

                for (int i = 0; i < newDisp.Count; i++)
                {
                    if (!string.Equals(newDisp[i], _rowDispCache[i], StringComparison.Ordinal))
                    {
                        // 순서/구성 변경 → 풀 리빌드 필요
                        return false;
                    }
                }

                // 동일 → 색/이벤트/키만 갱신
                _statePicByKey.Clear();

                for (int i = 0; i < stateProps.Count; i++)
                {
                    var p = stateProps[i];
                    var nameLabel = _nameLabelsCache[i];
                    var pb = _statePicsCache[i];

                    // 키 재설정 및 맵/이벤트 갱신
                    var key = ExtractKey(nameLabel.Text);
                    var normKey = key;
                    if (!string.IsNullOrEmpty(normKey))
                    {
                        pb.Tag = normKey;
                        _statePicByKey[normKey] = pb;
                    }

                    // 상태 색상(옵션): FastInitialPaint=true라도 in-place에서는 갱신 허용
                    ApplyStateColor(pb, normKey, p.State);
                }

                // 스크롤/레이아웃은 유지
                tableLayoutPanel.Invalidate();
                return true;
            }
            catch { return false; }
        }

        // === 키/상태 헬퍼 ===

        // "x3", "X03", " X003 " -> "X003" (자릿수는 _keyPadWidth 기준)
        // "VAC_SENSOR" / "Z1" 처럼 X/Y 숫자 계열이 아니면 원문(대문자/트림) 그대로 사용
        private string NormalizeKey(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;

            raw = raw.Trim().ToUpperInvariant();

            // 1) X/Y + 숫자
            var m = Regex.Match(raw, @"^(X|Y)\s*0*(\d+)$");
            if (!m.Success)
                m = Regex.Match(raw, @"^(X|Y)\s*0*(\d+)\b");
            if (m.Success)
            {
                string letter = m.Groups[1].Value; // X or Y
                string digits = m.Groups[2].Value; // 선행 0 제거된 숫자
                if (string.IsNullOrEmpty(digits)) digits = "0";

                int w = Math.Max(1, _keyPadWidth);
                if (digits.Length < w) digits = digits.PadLeft(w, '0');

                return letter + digits;            // 예: X003
            }

            // 2) 그 외: 그대로(대문자/트림) 키로 사용
            return raw;
        }

        // 이름 셀 텍스트의 첫 토큰을 키로 간주하고 정규화
        private string ExtractKey(string nameCellText)
        {
            if (string.IsNullOrWhiteSpace(nameCellText)) return null;

            // "X003 START ..." -> "X003", "VAC_SENSOR something" -> "VAC_SENSOR"
            string firstToken = nameCellText.Trim().Split(' ')[0];
            return NormalizeKey(firstToken);
        }

        // 상태 색 적용
        private static void ApplyStateColor(PictureBox pb, string key, bool on)
        {
            if (pb == null) return;

            // X..(DI): 파랑 / Y..(DO): 빨강 / 그 외: 초록
            if (!string.IsNullOrEmpty(key) && (key[0] == 'X'))
                pb.BackColor = on ? Color.FromArgb(0, 176, 240) : Color.White;
            else if (!string.IsNullOrEmpty(key) && (key[0] == 'Y'))
                pb.BackColor = on ? Color.Red : Color.White;
            else
                pb.BackColor = on ? Color.LimeGreen : Color.White; // fallback
        }

        // 공개 API: 키로 상태 업데이트 (정확 일치)
        public void SetStateByKey(string key, bool on)
        {
            key = NormalizeKey(key);
            if (string.IsNullOrEmpty(key)) return;

            if (_statePicByKey.TryGetValue(key, out var pb))
                ApplyStateColor(pb, key, on);
        }

        private readonly ToolTip _nameToolTip = new ToolTip
        {
            ShowAlways = true,
            InitialDelay = 300,
            ReshowDelay = 100,
            AutoPopDelay = 10000
        };


    }
}
