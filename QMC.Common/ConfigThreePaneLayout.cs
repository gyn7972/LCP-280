using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace QMC.Common
{
    /// <summary>
    /// Reusable 3-pane layout host for Config forms.
    /// - Left area split into Top/Bottom rows.
    /// - Right area (single panel).
    /// - Optional Far-Right area (single panel).
    /// Uses TableLayoutPanel + Dock for stable DPI/resize behavior.
    /// </summary>
    public class ConfigThreePaneLayout : UserControl, IResizable
    {
        private TableLayoutPanel _root;
        private TableLayoutPanel _left;

        public Panel LeftTopPanel { get; }
        public Panel LeftBottomPanel { get; }
        public Panel RightPanel { get; }
        public Panel FarRightPanel { get; }

        private bool _initialized;

        private float _leftPercent = 55f;
        private float _rightPercent = 22f;
        private float _farRightPercent = 23f;
        private float _leftTopRowPercent = 45f; // bottom is 55%

        private int _paddingAll = 8;
        private int _spacing = 0;
        private bool _showFarRight = true;
        private bool _showLeftBottom = true;

        [Category("Layout"), Description("Left column width percent (0-100). Remaining is split to Right/FarRight.")]
        public float LeftPercent
        {
            get => _leftPercent; set { _leftPercent = value; ApplyColumnStyles(); }
        }
        [Category("Layout"), Description("Right column width percent (0-100).")]
        public float RightPercent
        {
            get => _rightPercent; set { _rightPercent = value; ApplyColumnStyles(); }
        }
        [Category("Layout"), Description("Far-right column width percent (0-100).")]
        public float FarRightPercent
        {
            get => _farRightPercent; set { _farRightPercent = value; ApplyColumnStyles(); }
        }
        [Category("Layout"), Description("Left side top row height percent (0-100). Bottom gets the remaining.")]
        public float LeftTopRowPercent
        {
            get => _leftTopRowPercent; set { _leftTopRowPercent = value; ApplyLeftRowStyles(); }
        }
        [Category("Layout"), Description("Outer padding around the layout.")]
        public int PaddingAll
        {
            get => _paddingAll; set { _paddingAll = value; ApplyPadding(); }
        }
        [Category("Layout"), Description("Spacing between columns/rows (currently used as TableLayoutPanel.Padding).")]
        public int Spacing
        {
            get => _spacing; set { _spacing = value; ApplyPadding(); }
        }
        [Category("Layout"), Description("Show the far-right panel.")]
        public bool ShowFarRight
        {
            get => _showFarRight; set { _showFarRight = value; RebuildRootColumns(); }
        }
        [Category("Layout"), Description("Show the left-bottom panel.")]
        public bool ShowLeftBottom
        {
            get => _showLeftBottom; set { _showLeftBottom = value; ApplyLeftRowStyles(); }
        }

        public ConfigThreePaneLayout()
        {
            BackColor = Color.White;

            LeftTopPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            LeftBottomPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            RightPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            FarRightPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };

            BuildLayout();
        }

        private void BuildLayout()
        {
            if (_initialized) return;

            _root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                ColumnCount = _showFarRight ? 3 : 2,
                RowCount = 1,
                Margin = new Padding(0),
                Padding = new Padding(_paddingAll)
            };
            _root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            _left = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                ColumnCount = 1,
                RowCount = 2,
                Margin = new Padding(_spacing),
                Padding = new Padding(0)
            };
            _left.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

            ApplyColumnStyles();
            ApplyLeftRowStyles();

            // compose
            _left.Controls.Add(LeftTopPanel, 0, 0);
            _left.Controls.Add(LeftBottomPanel, 0, 1);

            _root.Controls.Add(_left, 0, 0);
            _root.Controls.Add(RightPanel, 1, 0);
            if (_showFarRight)
            {
                _root.Controls.Add(FarRightPanel, 2, 0);
            }

            Controls.Add(_root);
            _initialized = true;
        }

        private void RebuildRootColumns()
        {
            if (_root == null) return;

            SuspendLayout();
            try
            {
                // Remove FarRight if hiding
                if (!_showFarRight && _root.Controls.Contains(FarRightPanel))
                {
                    _root.Controls.Remove(FarRightPanel);
                }
                // Add back if showing and not present
                if (_showFarRight && !_root.Controls.Contains(FarRightPanel))
                {
                    _root.Controls.Add(FarRightPanel, 2, 0);
                }

                _root.ColumnCount = _showFarRight ? 3 : 2;
                ApplyColumnStyles();
                _root.PerformLayout();
            }
            finally
            {
                ResumeLayout(true);
            }
        }

        private void ApplyColumnStyles()
        {
            if (_root == null) return;
            _root.ColumnStyles.Clear();

            float left = Math.Max(0, _leftPercent);
            float right = Math.Max(0, _rightPercent);
            float far = _showFarRight ? Math.Max(0, _farRightPercent) : 0f;

            float sum = left + right + far;
            if (sum <= 0.01f)
            {
                left = 55f; right = 22f; far = _showFarRight ? 23f : 0f;
                sum = left + right + far;
            }
            float nl = left / sum * 100f;
            float nr = right / sum * 100f;
            float nf = _showFarRight ? far / sum * 100f : 0f;

            _root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, nl));
            _root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, nr));
            if (_showFarRight)
            {
                _root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, nf));
            }
        }

        private void ApplyLeftRowStyles()
        {
            if (_left == null) return;
            _left.RowStyles.Clear();

            if (_showLeftBottom)
            {
                float top = Math.Max(0, _leftTopRowPercent);
                if (top <= 0f) top = 45f;
                float bottom = 100f - top;
                if (bottom < 0f) bottom = 0f;
                _left.RowStyles.Add(new RowStyle(SizeType.Percent, top));
                _left.RowStyles.Add(new RowStyle(SizeType.Percent, bottom));
                LeftBottomPanel.Visible = true;
            }
            else
            {
                _left.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
                _left.RowStyles.Add(new RowStyle(SizeType.Percent, 0f));
                LeftBottomPanel.Visible = false;
            }
        }

        private void ApplyPadding()
        {
            if (_root != null)
            {
                _root.Padding = new Padding(_paddingAll);
            }
            if (_left != null)
            {
                _left.Margin = new Padding(_spacing);
            }
        }

        public void SetPanelSize(int width, int height)
        {
            // No special handling necessary; Dock+TableLayoutPanel handles it.
            Size = new Size(width, height);
            ClientSize = new Size(width, height);
        }
    }
}
