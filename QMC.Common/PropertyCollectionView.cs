using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace QMC.Common
{
    [DefaultProperty("TextBoxFontSize")]
    public partial class PropertyCollectionView : UserControl
    {
        public sealed class PropertyComboChangedEventArgs : EventArgs
        {
            public string Title { get; }
            public string Value { get; }

            public PropertyComboChangedEventArgs(string title, string value)
            {
                Title = title;
                Value = value;
            }
        }

        public event EventHandler<PropertyComboChangedEventArgs> ComboSelectionChanged;

        // [ADD] 마우스 휠 스크롤 방지용 커스텀 콤보박스(내부 클래스로 정의)
        private class NoScrollComboBox : ComboBox
        {
            private const int WM_MOUSEWHEEL = 0x020A;

            protected override void WndProc(ref Message m)
            {
                // 드롭다운 리스트가 열려있지 않을 때 들어오는 휠 메시지는 무시
                if (m.Msg == WM_MOUSEWHEEL && !this.DroppedDown)
                {
                    return;
                }
                base.WndProc(ref m);
            }
        }
        public static bool GlobalVerboseLogging = false;
        [Browsable(false)] public bool FastBuild { get; set; } = true;
        [Browsable(false)] public bool SuppressResizeInvalidation { get; set; } = true;

        private TableLayoutPanel tableLayoutPanel;
        private Panel scrollPanel;
        private GroupBox groupBox;

        protected Font _textBoxFont = new Font("맑은 고딕", 10f, FontStyle.Bold);
        private HorizontalAlignment _textBoxTextAlign = HorizontalAlignment.Left;

        // [ADD] 특정 Title을 "Position Display" 스타일로 표시하기 위한 설정
        private HashSet<string> _displayStyleTitles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // [ADD] Display 스타일(= lblPosition 룩) 옵션
        private Font _displayFont = new Font("Consolas", 20f, FontStyle.Bold);
        private ContentAlignment _displayTextAlign = ContentAlignment.MiddleCenter;
        private Color _displayBackColor = Color.Black;
        private Color _displayForeColor = Color.Lime;

        private const int MinVisibleRows = 3;
        private const int MaxVisibleRows = 15;
        private const int GroupBoxHeaderHeight = 20;
        private const int GroupBoxPadding = 20;

        private HashSet<string> _readOnlyTitles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // [ADD] 행 메타정보 저장용 구조체 + 리스트
        private struct RowInfo
        {
            public string Title;
            public Control Editor;
            public Label TitleLabel;
        }
        private readonly List<RowInfo> _rows = new List<RowInfo>();

        [Browsable(true)]
        [Category("Appearance")]
        [Description("GroupBox 이름")]
        public string GroupName
        {
            get => groupBox?.Text ?? "";
            set { if (groupBox != null) groupBox.Text = value ?? ""; }
        }

        [Category("Appearance")]
        [Description("텍스트박스의 폰트 크기")]
        [DefaultValue(9f)]
        public float TextBoxFontSize
        {
            get => _textBoxFont.Size;
            set
            {
                if (value > 0 && Math.Abs(_textBoxFont.Size - value) > 0.01f)
                {
                    _textBoxFont = new Font(_textBoxFont.FontFamily, value);
                    RefreshProperties();
                }
            }
        }

        [Category("Appearance")]
        [Description("텍스트박스의 폰트")]
        [DefaultValue(typeof(Font), "맑은 고딕, 10pt")]
        public Font TextBoxFont
        {
            get => _textBoxFont;
            set { if (value != null && !_textBoxFont.Equals(value)) { _textBoxFont = value; RefreshProperties(); } }
        }

        [Category("Appearance")]
        [Description("텍스트박스의 텍스트 정렬")]
        [DefaultValue(HorizontalAlignment.Left)]
        public HorizontalAlignment TextBoxTextAlign
        {
            get => _textBoxTextAlign;
            set { if (_textBoxTextAlign != value) { _textBoxTextAlign = value; RefreshProperties(); } }
        }

        // ★ [ADD] Display용 행 높이 (큰 폰트 표시 보장)
        private int _displayRowHeight = 44;

        [Browsable(true)]
        [Category("Appearance")]
        [Description("Display 스타일(큰 글씨) 행 높이(px)")]
        [DefaultValue(44)]
        public int DisplayRowHeight
        {
            get => _displayRowHeight;
            set
            {
                _displayRowHeight = Math.Max(22, value);
                RefreshProperties();
            }
        }

        // ★ [ADD] Display 스타일 타이틀 라벨(좌측 축명)도 같이 꾸밀지
        private bool _displayStyleAffectsTitleLabel = true;

        [Browsable(true)]
        [Category("Appearance")]
        [Description("Display 스타일이 Title(Label)에도 적용될지 여부")]
        [DefaultValue(true)]
        public bool DisplayStyleAffectsTitleLabel
        {
            get => _displayStyleAffectsTitleLabel;
            set
            {
                _displayStyleAffectsTitleLabel = value;
                ApplyReadOnlyStateToEditors();
            }
        }

        private List<Tuple<TextBox, PropertyBase>> _textBoxPropertyMap = new List<Tuple<TextBox, PropertyBase>>();
        private PropertyCollection _currentProperties;

        private readonly List<Label> _titleLabelOrder = new List<Label>();
        private readonly List<Control> _editorOrder = new List<Control>();
        private readonly List<Type> _propTypeOrder = new List<Type>();
        private int _propertyCountWithoutHeaders = 0;

        public PropertyCollectionView(string groupName = "Property Group")
        {
            InitializeComponent();
            InitializeComponentUser(groupName);
        }

        public PropertyCollectionView() : this("Property Group") { }

        private void InitializeComponentUser(string groupName)
        {
            groupBox = new GroupBox
            {
                Text = groupName,
                Font = new Font("맑은 고딕", 10f, FontStyle.Regular),
                ForeColor = Color.Black,
                BackColor = Color.White,
                Padding = new Padding(8, 8, 8, 12),
                Dock = DockStyle.Fill
            };
            this.Controls.Add(groupBox);

            scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                AutoScrollMargin = new Size(0, 4),
                BackColor = Color.White,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };

            tableLayoutPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 2,
                RowCount = 0,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
            };

            tableLayoutPanel.ColumnStyles.Clear();

            // 옵션 1) 오른쪽이 전체의 40% (왼쪽 60%)
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));

            // 옵션 2) "오른쪽이 왼쪽보다 70~80% 더 큼"에 맞추려면 대략 36:64
            // tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36F));
            // tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 64F));

            EnableFlickerFree(this);
            EnableFlickerFree(scrollPanel);
            EnableFlickerFree(tableLayoutPanel);

            scrollPanel.Controls.Add(tableLayoutPanel);
            groupBox.Controls.Add(scrollPanel);

            scrollPanel.Resize += (s, e) =>
            {
                var sbw = SystemInformation.VerticalScrollBarWidth;
                tableLayoutPanel.Width = scrollPanel.ClientSize.Width - sbw;
            };

            this.DoubleBuffered = true;
            if (GlobalVerboseLogging)
                Console.WriteLine($"🔧 PropertyCollectionView 초기화: UserControl={this.Size}, GroupBox=Fill");
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (GlobalVerboseLogging)
                Console.WriteLine($"🔧 PropertyCollectionView OnResize: UserControl={this.Size}, DesignMode={this.DesignMode}");
            if (!SuppressResizeInvalidation)
            {
                scrollPanel?.Invalidate();
                tableLayoutPanel?.Invalidate();
                groupBox?.Invalidate();
            }
        }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            base.SetBoundsCore(x, y, width, height, specified);
            if (GlobalVerboseLogging && (specified & BoundsSpecified.Size) != 0)
                Console.WriteLine($"🔧 PropertyCollectionView SetBoundsCore: Size=({width}, {height}), DesignMode={this.DesignMode}");
        }

        public virtual void SetProperties(PropertyCollection properties)
        {
            if (properties != null && TryUpdateInPlace(properties))
            {
                _currentProperties = properties;
                // [UPDATE] 빠른 재바인딩 시에도 읽기전용 적용을 재반영
                ApplyReadOnlyStateToEditors();
                return;
            }

            if (FastBuild)
            {
                this.SuspendLayout();
                groupBox?.SuspendLayout();
                scrollPanel?.SuspendLayout();
                tableLayoutPanel?.SuspendLayout();
            }

            this.Visible = false;

            tableLayoutPanel.Controls.Clear();
            tableLayoutPanel.RowStyles.Clear();
            tableLayoutPanel.RowCount = 0;
            _textBoxPropertyMap.Clear();
            _currentProperties = properties;

            _titleLabelOrder.Clear();
            _editorOrder.Clear();
            _propTypeOrder.Clear();
            _propertyCountWithoutHeaders = 0;
            _rows.Clear(); // [ADD] 행 캐시 초기화

            if (properties == null || properties.Count == 0)
            {
                if (FastBuild)
                {
                    tableLayoutPanel.ResumeLayout(false);
                    scrollPanel.ResumeLayout(false);
                    groupBox.ResumeLayout(false);
                    this.ResumeLayout(true);
                }
                this.Visible = true;
                if (GlobalVerboseLogging)
                    Console.WriteLine($"🔧 SetProperties (빈 데이터): UserControl={this.Size}");
                return;
            }

            int textBoxHeight = TextRenderer.MeasureText("A", _textBoxFont).Height + 8;
            var controlsToAdd = new List<Tuple<Control, int, int>>();
            var columnSpansToSet = new List<Tuple<Control, int>>();

            int row = 0;
            foreach (var prop in properties)
            {
                tableLayoutPanel.RowCount++;
                tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, textBoxHeight));

                if (prop is TitleOnlyProperty titleOnlyProp)
                {
                    var titleLabel = new Label
                    {
                        Text = titleOnlyProp.Titles.Length == 1 ? titleOnlyProp.Titles[0] : string.Join(" / ", titleOnlyProp.Titles),
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleLeft,
                        AutoSize = false,
                        Margin = new Padding(0),
                        Padding = new Padding(2),
                        Font = new Font(_textBoxFont.FontFamily, _textBoxFont.Size, FontStyle.Bold),
                        BackColor = Color.LightGray,
                        Visible = true
                    };
                    controlsToAdd.Add(Tuple.Create((Control)titleLabel, 0, row));
                    columnSpansToSet.Add(Tuple.Create((Control)titleLabel, tableLayoutPanel.ColumnCount));
                }
                else if (prop is ComboBoxProperty comboBoxProperty)
                {
                    var titleLabel = new Label
                    {
                        Text = prop.Title,
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleLeft,
                        AutoSize = false,
                        Margin = new Padding(2),
                        Padding = new Padding(2),
                        Visible = true
                    };
                    var comboBox = new NoScrollComboBox     //var comboBox = new ComboBox
                    {
                        Dock = DockStyle.Fill,
                        Margin = new Padding(0),
                        Font = _textBoxFont,
                        DropDownStyle = ComboBoxStyle.DropDownList,
                        Visible = true
                    };
                    comboBox.Items.Clear();
                    comboBox.Items.AddRange(comboBoxProperty.Options.ToArray());
                    if (comboBoxProperty.Value != null)
                    {
                        int idx = comboBoxProperty.Options.IndexOf(comboBoxProperty.Value.ToString());
                        comboBox.SelectedIndex = idx >= 0 ? idx : 0;
                    }
                    else comboBox.SelectedIndex = 0;
                    //comboBox.SelectedIndexChanged += (sender, args) => comboBoxProperty.SetValue(comboBox.SelectedItem?.ToString());
                    comboBox.SelectedIndexChanged += (sender, args) =>
                    {
                        var selected = comboBox.SelectedItem?.ToString();
                        comboBoxProperty.SetValue(selected);
                        ComboSelectionChanged?.Invoke(this, new PropertyComboChangedEventArgs(prop.Title, selected));
                    };

                    controlsToAdd.Add(Tuple.Create((Control)titleLabel, 0, row));
                    controlsToAdd.Add(Tuple.Create((Control)comboBox, 1, row));

                    _titleLabelOrder.Add(titleLabel);
                    _editorOrder.Add(comboBox);
                    _propTypeOrder.Add(typeof(ComboBoxProperty));
                    _propertyCountWithoutHeaders++;

                    // [ADD] 행 캐시에 저장
                    _rows.Add(new RowInfo { Title = prop.Title, Editor = comboBox, TitleLabel = titleLabel });
                }
                else
                {
                    var titleLabel = new Label
                    {
                        Text = prop.Title,
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleLeft,
                        AutoSize = false,
                        Margin = new Padding(2),
                        Padding = new Padding(2),
                        Visible = true
                    };
                    controlsToAdd.Add(Tuple.Create((Control)titleLabel, 0, row));
                    Control editor;
                    if (prop is BoolProperty bp)
                    {
                        var cb = new CheckBox
                        {
                            Dock = DockStyle.Fill,
                            AutoSize = false,
                            Height = textBoxHeight,
                            Margin = new Padding(0),
                            Appearance = Appearance.Button,
                            FlatStyle = FlatStyle.Flat,
                            TextAlign = ContentAlignment.MiddleCenter,
                            UseVisualStyleBackColor = false,
                            Visible = true
                        };
                        BindCheckBoxToBool(cb, bp);
                        editor = cb;
                        _propTypeOrder.Add(typeof(BoolProperty));
                    }
                    else if (prop is IntProperty ip)
                    { var tb = MakeValueTextBox(textBoxHeight); BindTextBoxToInt(tb, ip); editor = tb; _propTypeOrder.Add(typeof(IntProperty)); }
                    else if (prop is LongProperty lp)
                    { var tb = MakeValueTextBox(textBoxHeight); BindTextBoxToLong(tb, lp); editor = tb; _propTypeOrder.Add(typeof(LongProperty)); }
                    else if (prop is FloatProperty fp)
                    { var tb = MakeValueTextBox(textBoxHeight); BindTextBoxToFloat(tb, fp); editor = tb; _propTypeOrder.Add(typeof(FloatProperty)); }
                    else if (prop is DoubleProperty dp)
                    { var tb = MakeValueTextBox(textBoxHeight); BindTextBoxToDouble(tb, dp); editor = tb; _propTypeOrder.Add(typeof(DoubleProperty)); }
                    else if (prop is StringProperty sp)
                    {
                        var tb = MakeValueTextBox(textBoxHeight); BindTextBoxToString(tb, sp);
                        if (ShouldUsePathPicker(prop.Title))
                        {
                            bool selectFolder = IsFolderTitle(prop.Title);
                            var panel = MakePathPickerPanel(tb, properties.IsInputParameter, selectFolder);
                            editor = panel;
                        }
                        else
                        {
                            editor = tb;
                        }
                        _propTypeOrder.Add(typeof(StringProperty));
                    }
                    else
                    { var tb = MakeValueTextBox(textBoxHeight); tb.TextChanged += (s, e) => prop.SetValue(tb.Text); tb.Tag = prop; editor = tb; _propTypeOrder.Add(prop.GetType()); }

                    // 기본 색/잠금 처리(입력 파라미터가 아닌 경우 잠금 스타일)
                    if (properties.IsInputParameter)
                    {
                        if (!(editor is Panel))
                        {
                            editor.ForeColor = Color.Black;
                            editor.BackColor = Color.White;
                        }
                    }
                    else
                    {
                        if (editor is TextBox tbRo)
                        { tbRo.ReadOnly = true; tbRo.TabStop = false; tbRo.ForeColor = Color.LimeGreen; tbRo.BackColor = Color.Black; }
                        else if (editor is Panel pnl && pnl.Controls.Count > 0 && pnl.Controls[0] is TextBox innerTb)
                        {
                            innerTb.ReadOnly = true; innerTb.TabStop = false; innerTb.ForeColor = Color.LimeGreen; innerTb.BackColor = Color.Black;
                            foreach (Control c in pnl.Controls) if (c is Button) c.Enabled = false;
                        }
                        else editor.Enabled = false;
                    }

                    editor.Visible = true;
                    controlsToAdd.Add(Tuple.Create(editor, 1, row));

                    _titleLabelOrder.Add(titleLabel);
                    _editorOrder.Add(editor);
                    _propertyCountWithoutHeaders++;

                    // [ADD] 행 캐시에 저장 (패널인 경우도 그대로 Editor 저장)
                    _rows.Add(new RowInfo { Title = prop.Title, Editor = editor, TitleLabel = titleLabel });
                }
                row++;
            }

            foreach (var controlInfo in controlsToAdd)
                tableLayoutPanel.Controls.Add(controlInfo.Item1, controlInfo.Item2, controlInfo.Item3);
            foreach (var spanInfo in columnSpansToSet)
                tableLayoutPanel.SetColumnSpan(spanInfo.Item1, spanInfo.Item2);

            if (FastBuild)
            {
                tableLayoutPanel.ResumeLayout(false);
                scrollPanel.ResumeLayout(false);
                groupBox.ResumeLayout(false);
                this.ResumeLayout(false);
            }

            this.Visible = true;
            tableLayoutPanel.PerformLayout();
            scrollPanel.PerformLayout();
            scrollPanel.AutoScrollMinSize = new Size(0, tableLayoutPanel.PreferredSize.Height + 2);
            if (scrollPanel.VerticalScroll.Maximum > 0)
                scrollPanel.VerticalScroll.Value = 0;

            // [NEW] 현재 지정된 읽기전용 타이틀 적용
            ApplyReadOnlyStateToEditors();

            if (GlobalVerboseLogging)
                Console.WriteLine($"🔧 SetProperties 완료(FastBuild={FastBuild}): UserControl={this.Size}, Items={properties.Count}");
        }

        private void BindCheckBoxToBool(CheckBox cb, BoolProperty p)
        {
            cb.Checked = p.Value;
            cb.Tag = p;
            UpdateBoolCheckBoxVisual(cb);
            cb.CheckedChanged += (s, e) =>
            {
                p.Value = cb.Checked;
                UpdateBoolCheckBoxVisual(cb);
            };
        }
        //private void BindCheckBoxToBool(CheckBox cb, BoolProperty p)
        //{ cb.Checked = p.Value; cb.CheckedChanged += (s, e) => p.Value = cb.Checked; cb.Tag = p; }

        private void UpdateBoolCheckBoxVisual(CheckBox cb)
        {
            if (cb == null) return;

            cb.Text = cb.Checked ? "ON" : "OFF";
            cb.ForeColor = Color.White;
            cb.BackColor = cb.Checked
                ? Color.FromArgb(46, 125, 50)   // ON: 녹색
                : Color.FromArgb(97, 97, 97);   // OFF: 회색

            cb.FlatAppearance.BorderSize = 1;
            cb.FlatAppearance.BorderColor = Color.DimGray;
        }

        private bool TryUpdateInPlace(PropertyCollection properties)
        {
            if (_editorOrder.Count == 0 || _propTypeOrder.Count == 0) return false;

            var newTypes = new List<Type>();
            int cnt = 0;
            foreach (var p in properties)
            {
                if (p is TitleOnlyProperty) continue;
                cnt++;
                if (p is ComboBoxProperty) newTypes.Add(typeof(ComboBoxProperty));
                else if (p is BoolProperty) newTypes.Add(typeof(BoolProperty));
                else if (p is IntProperty) newTypes.Add(typeof(IntProperty));
                else if (p is LongProperty) newTypes.Add(typeof(LongProperty));
                else if (p is FloatProperty) newTypes.Add(typeof(FloatProperty));
                else if (p is DoubleProperty) newTypes.Add(typeof(DoubleProperty));
                else if (p is StringProperty) newTypes.Add(typeof(StringProperty));
                else newTypes.Add(p.GetType());
            }

            if (cnt != _propertyCountWithoutHeaders) return false;
            for (int i = 0; i < newTypes.Count; i++)
            {
                if (newTypes[i] != _propTypeOrder[i]) return false;
            }

            _textBoxPropertyMap.Clear();
            _rows.Clear(); // [ADD] 빠른 재바인딩에서도 새 행을 다시 구성

            int index = 0;
            foreach (var p in properties)
            {
                if (p is TitleOnlyProperty) continue;

                if (index < _titleLabelOrder.Count)
                    _titleLabelOrder[index].Text = p.Title;

                var editor = _editorOrder[index];

                if (p is ComboBoxProperty cbp)
                {
                    var cb = editor as ComboBox;
                    if (cb != null)
                    {
                        if (cb.Items.Count != cbp.Options.Count || cb.Items.Cast<object>().Select(o => o.ToString()).SequenceEqual(cbp.Options) == false)
                        {
                            cb.Items.Clear();
                            cb.Items.AddRange(cbp.Options.ToArray());
                        }
                        int idx = cbp.Value != null ? cbp.Options.IndexOf(cbp.Value.ToString()) : -1;
                        cb.SelectedIndexChanged += (s, e) => cbp.SetValue(cb.SelectedItem?.ToString());
                        cb.SelectedIndex = idx >= 0 ? idx : (cb.Items.Count > 0 ? 0 : -1);
                    }
                }
                else if (p is BoolProperty bp)
                {
                    var cb = editor as CheckBox;
                    if (cb != null)
                    {
                        cb.CheckedChanged += (s, e) => bp.Value = cb.Checked;
                        cb.Checked = bp.Value;
                    }
                }
                else if (p is StringProperty sp)
                {
                    TextBox tb = editor as TextBox;
                    if (tb == null && editor is Panel pnl)
                        tb = pnl.Controls.OfType<TextBox>().FirstOrDefault();
                    if (tb != null)
                    {
                        tb.Text = sp.Value ?? string.Empty;
                        tb.Tag = sp;
                        _textBoxPropertyMap.Add(Tuple.Create(tb, (PropertyBase)sp));
                    }
                }
                else if (p is IntProperty ip)
                {
                    var tb = editor as TextBox;
                    if (tb != null)
                    {
                        tb.Text = ip.Value.ToString(CultureInfo.InvariantCulture);
                        tb.Tag = ip;
                        _textBoxPropertyMap.Add(Tuple.Create(tb, (PropertyBase)ip));
                    }
                }
                else if (p is LongProperty lp)
                {
                    var tb = editor as TextBox;
                    if (tb != null)
                    {
                        tb.Text = lp.Value.ToString(CultureInfo.InvariantCulture);
                        tb.Tag = lp;
                        _textBoxPropertyMap.Add(Tuple.Create(tb, (PropertyBase)lp));
                    }
                }
                else if (p is FloatProperty fp)
                {
                    var tb = editor as TextBox;
                    if (tb != null)
                    {
                        tb.Text = fp.Value.ToString(CultureInfo.InvariantCulture);
                        tb.Tag = fp;
                        _textBoxPropertyMap.Add(Tuple.Create(tb, (PropertyBase)fp));
                    }
                }
                else if (p is DoubleProperty dp)
                {
                    var tb = editor as TextBox;
                    if (tb != null)
                    {
                        tb.Text = dp.Value.ToString(CultureInfo.InvariantCulture);
                        tb.Tag = dp;
                        _textBoxPropertyMap.Add(Tuple.Create(tb, (PropertyBase)dp));
                    }
                }
                else
                {
                    var tb = editor as TextBox;
                    if (tb != null)
                    {
                        tb.Text = p.Value != null ? p.Value.ToString() : string.Empty;
                        tb.Tag = p;
                        _textBoxPropertyMap.Add(Tuple.Create(tb, p));
                    }
                }

                // [ADD] 행 캐시 갱신
                var titleLabel = (index < _titleLabelOrder.Count) ? _titleLabelOrder[index] : null;
                _rows.Add(new RowInfo { Title = p.Title, Editor = editor, TitleLabel = titleLabel });

                index++;
            }

            if (GlobalVerboseLogging)
                Console.WriteLine("⚡ 빠른 재바인딩 완료 (컨트롤 재사용)");
            return true;
        }

        private TextBox MakeValueTextBox(int textBoxHeight) => new TextBox
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0),
            BorderStyle = BorderStyle.FixedSingle,
            Font = _textBoxFont,
            TextAlign = _textBoxTextAlign,
            Visible = true,
            MinimumSize = new Size(0, textBoxHeight),
            Height = textBoxHeight
        };

        private static bool ShouldUsePathPicker(string title)
        {
            if (string.IsNullOrEmpty(title)) return false;
            return title.IndexOf("path", StringComparison.OrdinalIgnoreCase) >= 0
                   || title.IndexOf("file", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool IsFolderTitle(string title)
        {
            if (string.IsNullOrEmpty(title)) return false;
            return title.IndexOf("path", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private Control MakePathPickerPanel(TextBox tb, bool enable, bool selectFolder)
        {
            var panel = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0) };
            var btn = new Button
            {
                Text = "...",
                Width = 28,
                Dock = DockStyle.Right,
                Margin = new Padding(2),
                Enabled = enable
            };
            btn.Click += (s, e) =>
            {
                try
                {
                    if (selectFolder)
                    {
                        using (var dlg = new FolderBrowserDialog())
                        {
                            try
                            {
                                if (!string.IsNullOrWhiteSpace(tb.Text))
                                {
                                    var startDir = Directory.Exists(tb.Text) ? tb.Text : Path.GetDirectoryName(tb.Text);
                                    if (!string.IsNullOrWhiteSpace(startDir) && Directory.Exists(startDir))
                                        dlg.SelectedPath = startDir;
                                }
                            }
                            catch { }
                            if (dlg.ShowDialog(this) == DialogResult.OK)
                            {
                                tb.Text = dlg.SelectedPath;
                            }
                        }
                    }
                    else
                    {
                        using (var dlg = new OpenFileDialog())
                        {
                            dlg.Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*";
                            dlg.Multiselect = false;
                            try
                            {
                                if (!string.IsNullOrWhiteSpace(tb.Text))
                                {
                                    var dir = Path.GetDirectoryName(tb.Text);
                                    if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
                                        dlg.InitialDirectory = dir;
                                    dlg.FileName = Path.GetFileName(tb.Text);
                                }
                            }
                            catch { }
                            if (dlg.ShowDialog(this) == DialogResult.OK)
                            {
                                tb.Text = dlg.FileName;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    var mb = new MessageBoxOk();
                    mb.ShowDialog("Error!", $"파일/폴더 선택 중 오류: " + ex.Message);
                }
            };

            tb.Parent = panel;
            tb.Dock = DockStyle.Fill;
            panel.Controls.Add(tb);
            panel.Controls.Add(btn);
            return panel;
        }

        public void Apply()
        {
            foreach (var pair in _textBoxPropertyMap)
            {
                var textBox = pair.Item1;
                var property = pair.Item2;
                try
                {
                    var s = textBox.Text ?? string.Empty;
                    if (property is DoubleProperty dp)
                    {
                        if (double.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var v))
                            dp.Value = v;
                    }
                    else if (property is FloatProperty fp)
                    {
                        if (float.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var v))
                            fp.Value = v;
                    }
                    else if (property is IntProperty ip)
                    {
                        if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v))
                            ip.Value = v;
                    }
                    else if (property is LongProperty lp)
                    {
                        if (long.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v))
                            lp.Value = v;
                    }
                    else if (property is StringProperty sp)
                    {
                        sp.Value = s;
                    }
                    else if (property is BoolProperty bp)
                    {
                        if (bool.TryParse(s, out var v)) bp.Value = v;
                    }
                    else
                    {
                        property.SetValue(s);
                    }
                }
                catch
                {
                    // ignore
                }
            }
        }

        public PropertyCollection GetCurrentProperties() => _currentProperties;

        private void RefreshProperties() { if (_currentProperties != null) SetProperties(_currentProperties); }

        private void BindTextBoxToDouble(TextBox tb, DoubleProperty p)
        { tb.Text = p.Value.ToString(CultureInfo.InvariantCulture); tb.TextChanged += (s, e) => { if (double.TryParse(tb.Text, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var v)) p.Value = v; }; tb.Tag = p; _textBoxPropertyMap.Add(Tuple.Create(tb, (PropertyBase)p)); }
        private void BindTextBoxToFloat(TextBox tb, FloatProperty p)
        { tb.Text = p.Value.ToString(CultureInfo.InvariantCulture); tb.TextChanged += (s, e) => { if (float.TryParse(tb.Text, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var v)) p.Value = v; }; tb.Tag = p; _textBoxPropertyMap.Add(Tuple.Create(tb, (PropertyBase)p)); }
        private void BindTextBoxToInt(TextBox tb, IntProperty p)
        { tb.Text = p.Value.ToString(CultureInfo.InvariantCulture); tb.TextChanged += (s, e) => { if (int.TryParse(tb.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v)) p.Value = v; }; tb.Tag = p; _textBoxPropertyMap.Add(Tuple.Create(tb, (PropertyBase)p)); }
        private void BindTextBoxToLong(TextBox tb, LongProperty p)
        { tb.Text = p.Value.ToString(CultureInfo.InvariantCulture); tb.TextChanged += (s, e) => { if (long.TryParse(tb.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v)) p.Value = v; }; tb.Tag = p; _textBoxPropertyMap.Add(Tuple.Create(tb, (PropertyBase)p)); }
        private void BindTextBoxToString(TextBox tb, StringProperty p)
        { tb.Text = p.Value ?? string.Empty; tb.TextChanged += (s, e) => p.Value = tb.Text ?? string.Empty; tb.Tag = p; _textBoxPropertyMap.Add(Tuple.Create(tb, (PropertyBase)p)); }
        

        private static void EnableFlickerFree(Control c)
        {
            if (c == null) return;
            try
            {
                var piDB = typeof(Control).GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance);
                piDB?.SetValue(c, true, null);
                var miSetStyle = typeof(Control).GetMethod("SetStyle", BindingFlags.NonPublic | BindingFlags.Instance);
                var styles = ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint;
                miSetStyle?.Invoke(c, new object[] { styles, true });
                var miUpdateStyles = typeof(Control).GetMethod("UpdateStyles", BindingFlags.NonPublic | BindingFlags.Instance);
                miUpdateStyles?.Invoke(c, null);
            }
            catch { }
        }

        // [NEW] 읽기전용 타이틀 지정 + 즉시 적용
        public void SetReadOnlyTitles(params string[] titles)
        {
            _readOnlyTitles = new HashSet<string>(titles ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            ApplyReadOnlyStateToEditors();
        }

        private HorizontalAlignment ToHorizontalAlignment(ContentAlignment ca)
        {
            switch (ca)
            {
                case ContentAlignment.TopLeft:
                case ContentAlignment.MiddleLeft:
                case ContentAlignment.BottomLeft:
                    return HorizontalAlignment.Left;

                case ContentAlignment.TopRight:
                case ContentAlignment.MiddleRight:
                case ContentAlignment.BottomRight:
                    return HorizontalAlignment.Right;

                default:
                    return HorizontalAlignment.Center;
            }
        }
        // ★ [ADD] ContentAlignment -> Label TextAlign
        private ContentAlignment ToLabelAlignment(ContentAlignment ca)
        {
            // Label은 ContentAlignment를 그대로 사용 가능
            return ca;
        }

        // ★ [MOD] Display 스타일이 "값(TextBox)" 뿐 아니라 "축명(Label)"에도 적용되고,
        //         Display 행은 RowHeight도 크게 잡아주도록 보강
        private void ApplyReadOnlyStateToEditors()
        {
            try
            {
                for (int i = 0; i < _rows.Count; i++)
                {
                    var row = _rows[i];

                    bool ro = _readOnlyTitles.Contains(row.Title);
                    bool display = _displayStyleTitles.Contains(row.Title);

                    // (1) RowHeight 보정 (Display인 경우만)
                    try
                    {
                        if (tableLayoutPanel != null && tableLayoutPanel.RowStyles != null && i < tableLayoutPanel.RowStyles.Count)
                        {
                            var rs = tableLayoutPanel.RowStyles[i];
                            if (display)
                            {
                                rs.SizeType = SizeType.Absolute;
                                rs.Height = _displayRowHeight;
                            }
                            else
                            {
                                // 기존 기본 높이로 복귀: 현재 폰트 기준
                                int normal = TextRenderer.MeasureText("A", _textBoxFont).Height + 8;
                                rs.SizeType = SizeType.Absolute;
                                rs.Height = normal;
                            }
                        }
                    }
                    catch { }

                    // (2) TitleLabel 스타일도 적용 (축명까지 Display로 보이게)
                    try
                    {
                        if (_displayStyleAffectsTitleLabel && row.TitleLabel != null)
                        {
                            if (display)
                            {
                                row.TitleLabel.Font = _displayFont;                 // 큰 폰트
                                row.TitleLabel.BackColor = _displayBackColor;       // 검정
                                row.TitleLabel.ForeColor = _displayForeColor;       // 라임
                                row.TitleLabel.TextAlign = ToLabelAlignment(_displayTextAlign);
                            }
                            else
                            {
                                row.TitleLabel.Font = new Font(_textBoxFont.FontFamily, _textBoxFont.Size, FontStyle.Regular);
                                row.TitleLabel.BackColor = Color.White;
                                row.TitleLabel.ForeColor = Color.Black;
                                row.TitleLabel.TextAlign = ContentAlignment.MiddleLeft;
                            }
                        }
                    }
                    catch { }

                    var editor = row.Editor;

                    // ---- TextBox 직접 ----
                    if (editor is TextBox tb)
                    {
                        tb.ReadOnly = ro;
                        tb.TabStop = !ro;
                        tb.Enabled = !ro;

                        if (display)
                        {
                            tb.Font = _displayFont;
                            tb.BackColor = _displayBackColor;
                            tb.ForeColor = _displayForeColor;
                            tb.TextAlign = ToHorizontalAlignment(_displayTextAlign);
                            tb.Multiline = true;
                            tb.BorderStyle = BorderStyle.FixedSingle;
                        }
                        else
                        {
                            tb.Font = _textBoxFont;
                            tb.TextAlign = _textBoxTextAlign;
                            tb.Multiline = false;

                            if (ro)
                            {
                                tb.ForeColor = Color.LimeGreen;
                                tb.BackColor = Color.Black;
                            }
                            else
                            {
                                tb.ForeColor = Color.Black;
                                tb.BackColor = Color.White;
                            }
                        }
                        continue;
                    }

                    // ---- Panel ----
                    if (editor is Panel pnl)
                    {
                        foreach (Control c in pnl.Controls)
                        {
                            if (c is TextBox innerTb)
                            {
                                innerTb.ReadOnly = ro;
                                innerTb.TabStop = !ro;
                                innerTb.Enabled = !ro;

                                if (display)
                                {
                                    innerTb.Font = _displayFont;
                                    innerTb.BackColor = _displayBackColor;
                                    innerTb.ForeColor = _displayForeColor;
                                    innerTb.TextAlign = ToHorizontalAlignment(_displayTextAlign);
                                    innerTb.Multiline = true;
                                    innerTb.BorderStyle = BorderStyle.FixedSingle;
                                }
                                else
                                {
                                    innerTb.Font = _textBoxFont;
                                    innerTb.TextAlign = _textBoxTextAlign;
                                    innerTb.Multiline = false;

                                    if (ro)
                                    {
                                        innerTb.ForeColor = Color.LimeGreen;
                                        innerTb.BackColor = Color.Black;
                                    }
                                    else
                                    {
                                        innerTb.ForeColor = Color.Black;
                                        innerTb.BackColor = Color.White;
                                    }
                                }
                            }
                            else if (c is ComboBox cb)
                            {
                                cb.Enabled = !ro;
                                cb.DropDownStyle = ComboBoxStyle.DropDownList;
                            }
                            else if (c is Button btn)
                            {
                                btn.Enabled = !ro;
                            }
                            else
                            {
                                c.Enabled = !ro;
                            }
                        }
                        continue;
                    }

                    // ---- 기타 ----
                    if (editor is ComboBox cb2)
                    {
                        cb2.Enabled = !ro;
                        cb2.DropDownStyle = ComboBoxStyle.DropDownList;
                    }
                    else if (editor is CheckBox chk)
                    {
                        chk.Enabled = !ro;
                        UpdateBoolCheckBoxVisual(chk);
                    }
                    else
                    {
                        editor.Enabled = !ro;
                    }
                }

                tableLayoutPanel?.PerformLayout();
                scrollPanel?.PerformLayout();
                Invalidate();
            }
            catch
            {
                // ignore
            }
        }

        // [ADD] 특정 Title들을 "Display 전용(라벨 룩)"으로 표시
        public void SetDisplayStyleTitles(params string[] titles)
        {
            _displayStyleTitles = new HashSet<string>(titles ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            ApplyReadOnlyStateToEditors();
        }

        // [ADD] Display 스타일 커스터마이즈(필요 시)
        public void SetDisplayStyle(Font font, Color backColor, Color foreColor, ContentAlignment align)
        {
            if (font != null) _displayFont = font;
            _displayBackColor = backColor;
            _displayForeColor = foreColor;
            _displayTextAlign = align;
            ApplyReadOnlyStateToEditors();
        }
    }
}