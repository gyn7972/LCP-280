using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using System.Reflection;

namespace QMC.Common
{
    public partial class IOPropertyCollectionView : PropertyCollectionView
    {
        private Label _selectedNameLabel = null;
        private const int IOMaxVisibleRows = 10;
        private const int IOGroupBoxHeaderHeight = 20;
        private const int IOGroupBoxPadding = 16;

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                // 폼 전체 컴포지팅 (깜빡임 크게 줄어듦 / 스크롤 성능 약간 저하 가능)
                cp.ExStyle |= 0x02000000; // WS_EX_COMPOSITED
                return cp;
            }
        }

        public IOPropertyCollectionView(string groupName = "IO Property Group") : base(groupName)
        {
            InitializeComponent();

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

        public IOPropertyCollectionView() : this("IO Property Group")
        {
            // (옵션) 깜빡임 줄이기
            this.DoubleBuffered = true;
        }

        public override void SetProperties(PropertyCollection properties)
        {
            // base의 필드 참조 (PropertyCollectionView와 동일한 방식)
            var tableLayoutPanelField = typeof(PropertyCollectionView).GetField("tableLayoutPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var scrollPanelField = typeof(PropertyCollectionView).GetField("scrollPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var textBoxFontField = typeof(PropertyCollectionView).GetField("_textBoxFont", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var groupBoxField = typeof(PropertyCollectionView).GetField("groupBox", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var tableLayoutPanel = tableLayoutPanelField.GetValue(this) as TableLayoutPanel;
            var scrollPanel = scrollPanelField.GetValue(this) as Panel;
            var textBoxFont = textBoxFontField.GetValue(this) as Font;
            var groupBox = groupBoxField.GetValue(this) as GroupBox;

            // PropertyCollectionView와 완전히 동일한 방식으로 초기화
            tableLayoutPanel.SuspendLayout();
            tableLayoutPanel.Controls.Clear();
            tableLayoutPanel.RowStyles.Clear();
            tableLayoutPanel.RowCount = 0;

            if (properties == null)
            {
                // 속성이 없을 때 최소 크기로 설정 (PropertyCollectionView와 동일)
                int minHeight = IOGroupBoxHeaderHeight + groupBox.Padding.Top + IOGroupBoxPadding;
                this.Height = minHeight;
                this.MinimumSize = new Size(this.Width, minHeight);
                this.MaximumSize = new Size(this.Width, minHeight);
                tableLayoutPanel.ResumeLayout();
                return;
            }

            int textBoxHeight = TextRenderer.MeasureText("A", textBoxFont).Height + 8;

            // TitleOnlyProperty와 PropertyState 처리
            var headerProp = properties.OfType<TitleOnlyProperty>().FirstOrDefault();
            var stateProps = properties.OfType<PropertyState>().ToList();
            bool hasNoColumn = stateProps.Any(p => properties.ShowNoColumn);
            int colCount = hasNoColumn ? 3 : 2;
            
            // 열 개수 및 스타일 설정 (IO 전용)
            tableLayoutPanel.ColumnCount = colCount;
            tableLayoutPanel.ColumnStyles.Clear();
            if (colCount == 2)
            {
                tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 80F));
                tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            }
            else
            {
                tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15F));
                tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
                tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15F));
            }

            int row = 0;

            // 헤더 행 추가 (PropertyCollectionView와 동일한 방식)
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

            // PropertyState 행 추가 (PropertyCollectionView의 foreach 방식과 동일)
            foreach (var prop in stateProps)
            {
                tableLayoutPanel.RowCount++;
                tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, textBoxHeight));
                
                int colIdx = 0;
                Label nameLabel = null;

                if (colCount == 3 && prop.ShowNoColumn)
                {
                    // 0번 열(No) 표시
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
                        Margin = new Padding(0), // 2에서 0으로 변경하여 노란색이 꽉 차게
                        Padding = new Padding(2), // 텍스트와 경계 사이의 여백은 Padding으로
                        BackColor = Color.White
                    };
                    nameLabel.Click += (s, e) =>
                    {
                        if (_selectedNameLabel != null)
                            _selectedNameLabel.BackColor = Color.White;
                        nameLabel.BackColor = Color.Yellow;
                        _selectedNameLabel = nameLabel;
                    };
                    tableLayoutPanel.Controls.Add(nameLabel, colIdx++, row);
                }
                else
                {
                    // 0번 열(No) 숨김 → 1번 열(Name)부터 시작
                    nameLabel = new Label
                    {
                        Text = prop.Value?.ToString() ?? "",
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleLeft,
                        AutoSize = false,
                        Margin = new Padding(0), // 2에서 0으로 변경하여 노란색이 꽉 차게
                        Padding = new Padding(2), // 텍스트와 경계 사이의 여백은 Padding으로
                        BackColor = Color.White
                    };
                    nameLabel.Click += (s, e) =>
                    {
                        if (_selectedNameLabel != null)
                            _selectedNameLabel.BackColor = Color.White;
                        nameLabel.BackColor = Color.Yellow;
                        _selectedNameLabel = nameLabel;
                    };
                    tableLayoutPanel.Controls.Add(nameLabel, colIdx++, row);
                }

                // State 열 (PropertyCollectionView의 TextBox와 유사하게 설정)
                var statePictureBox = new PictureBox
                {
                    Dock = DockStyle.Fill,
                    Margin = new Padding(0),
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    BorderStyle = BorderStyle.None,
                    BackColor = Color.Empty
                };

                // PropertyCollectionView의 TextBox와 동일한 크기 제약 적용
                statePictureBox.MinimumSize = new Size(0, textBoxHeight);
                statePictureBox.Height = textBoxHeight;

                string title = prop.Title ?? "";
                string titleAlpha = new string(title.Where(char.IsLetter).ToArray());

                if (titleAlpha.Contains("X"))
                {
                    statePictureBox.BackColor = prop.State ? Color.FromArgb(0, 176, 240) : Color.White;
                }
                else if (titleAlpha.Contains("Y"))
                {
                    statePictureBox.BackColor = prop.State ? Color.Red : Color.White;
                }
                else
                {
                    statePictureBox.BackColor = Color.Black;
                }

                tableLayoutPanel.Controls.Add(statePictureBox, colIdx, row);
                row++;
            }

            // PropertyCollectionView와 완전히 동일한 동적 크기 계산
            int totalRows = (headerProp != null ? 1 : 0) + stateProps.Count;
            int calculatedHeight = (totalRows * textBoxHeight) + IOGroupBoxHeaderHeight + groupBox.Padding.Top + IOGroupBoxPadding;
            int maxHeight = (IOMaxVisibleRows * textBoxHeight) + IOGroupBoxHeaderHeight + groupBox.Padding.Top + IOGroupBoxPadding;

            // PropertyCollectionView와 동일한 TableLayoutPanel 높이 설정
            tableLayoutPanel.Height = totalRows * textBoxHeight;

            if (totalRows > IOMaxVisibleRows)
            {
                this.Height = maxHeight;
                this.MinimumSize = new Size(this.Width, maxHeight);
                this.MaximumSize = new Size(this.Width, maxHeight);
                scrollPanel.AutoScroll = true;
                scrollPanel.VerticalScroll.Visible = true;
                scrollPanel.VerticalScroll.Value = scrollPanel.VerticalScroll.Maximum;
            }
            else
            {
                this.Height = calculatedHeight;
                this.MinimumSize = new Size(this.Width, calculatedHeight);
                this.MaximumSize = new Size(this.Width, calculatedHeight);
                scrollPanel.AutoScroll = false;
                scrollPanel.VerticalScroll.Visible = false;
            }

            tableLayoutPanel.ResumeLayout();

            // 부모 컨트롤에게 크기 변경 알림 (PropertyCollectionView와 동일)
            this.Invalidate();
            this.Parent?.PerformLayout();
        }
    }
}
