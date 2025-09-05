using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using QMC.Common.Vision; // For VisionImage

namespace QMC.Common
{
    public class SimpleGroupBox : GroupBox
    {
        public int Radious { get; set; }
        public Color TitleBackColor { get; set; } = Color.SteelBlue;
        public float TitleFontSize { get; set; } = 9f;
        public bool UseExpand { get; set; }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (!string.IsNullOrEmpty(Text))
            {
                using (var b = new SolidBrush(TitleBackColor))
                {
                    var sz = TextRenderer.MeasureText(Text, new Font(Font.FontFamily, TitleFontSize, FontStyle.Bold));
                    var rect = new Rectangle(8, 0, sz.Width + 6, sz.Height);
                    e.Graphics.FillRectangle(b, rect);
                    TextRenderer.DrawText(e.Graphics, Text, new Font(Font.FontFamily, TitleFontSize, FontStyle.Bold), rect, Color.White);
                }
            }
        }
    }

    public class SimpleToggleButton : Button
    {
        private bool _status;
        public bool GetButtonStatus() => _status;
        public void UpdateToggleStatus(bool on)
        {
            _status = on;
            BackColor = on ? Color.DodgerBlue : SystemColors.Control;
            ForeColor = on ? Color.White : SystemColors.ControlText;
            Invalidate();
        }
    }

    // Delegate matching legacy parameterless ImageChanged pattern
    public delegate void ImageChangedEventHandler();

    public class SimpleTrainPictureBox : PictureBox
    {
        private VisionImage _visionImage;
        public event ImageChangedEventHandler ImageChanged;

        public SimpleTrainPictureBox()
        {
            SizeMode = PictureBoxSizeMode.Zoom;
            BorderStyle = BorderStyle.FixedSingle;
        }

        public void SetImage(System.Drawing.Image img)
        {
            Image = img;
            OnImageChanged();
        }

        public void SetImage(VisionImage vimg)
        {
            _visionImage = vimg;
            if (vimg != null)
                SetImage(vimg.GetImage());
            else
                SetImage((System.Drawing.Image)null);
        }

        public VisionImage GetImage() => _visionImage;

        protected virtual void OnImageChanged() => ImageChanged?.Invoke();
    }

    public class ParamTextControl : UserControl
    {
        private Label _label;
        private TextBox _textBox;
        private Param _param;
        private int _titleRatio = 50; // percent
        private bool _internalUpdating; // suppress re-entrant update

        public int TitleRatio
        {
            get { return _titleRatio; }
            set
            {
                if (value < 0) value = 0;
                if (value > 100) value = 100;
                if (_titleRatio == value) return;
                _titleRatio = value;
                if (!IsDesignModeSafe()) ResizeForTitleRatio();
            }
        }

        public event EventHandler ParamValueChanged; // optional notification

        public ParamTextControl()
        {
            try
            {
                Height = 30;
                _label = new Label { Dock = DockStyle.Left, TextAlign = ContentAlignment.MiddleLeft, Width = 100 };
                _textBox = new TextBox { Dock = DockStyle.Fill };
                _textBox.TextChanged += OnTextChanged;
                Controls.Add(_textBox);
                Controls.Add(_label);
            }
            catch { /* swallow design-time exceptions */ }
        }

        private bool IsDesignModeSafe()
        {
            try
            {
                return LicenseManager.UsageMode == LicenseUsageMode.Designtime || (Site != null && Site.DesignMode);
            }
            catch { return false; }
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            if (IsDesignModeSafe()) return; // skip runtime-only logic if any later added
        }

        public void InitControl(Param param)
        {
            _param = param;
            if (!IsDesignModeSafe()) RefreshDisplay();
        }

        public Param GetParamData() => _param;

        public void SetReadOnlyTextbox(bool readOnly)
        {
            if (_textBox == null) return;
            _textBox.ReadOnly = readOnly;
            if (readOnly)
            {
                _textBox.BackColor = Color.Black;
                _textBox.ForeColor = Color.Lime;
                _textBox.TabStop = false;
            }
            else
            {
                _textBox.BackColor = SystemColors.Window;
                _textBox.ForeColor = SystemColors.WindowText;
            }
        }

        public void SetValue(object v)
        {
            if (_param != null) _param.Value = v;
            SetTextboxTextSilently(v != null ? v.ToString() : string.Empty);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (!IsDesignModeSafe()) ResizeForTitleRatio();
        }

        private void ResizeForTitleRatio()
        {
            if (Width > 0 && _label != null)
            {
                _label.Width = (int)(Width * (TitleRatio / 100.0));
            }
        }

        private void RefreshDisplay()
        {
            if (_label == null || _textBox == null) return;
            if (_param == null)
            {
                SetTextboxTextSilently(string.Empty);
                _label.Text = string.Empty;
                return;
            }
            _label.Text = _param.Title ?? string.Empty;
            string display = _param.Value != null ? _param.Value.ToString() : string.Empty;
            if (IsCompositeDisplayType(_param))
            {
                display = CleanCompositeValue(display);
            }
            SetTextboxTextSilently(display);
        }

        private void SetTextboxTextSilently(string text)
        {
            if (_textBox == null) return;
            _internalUpdating = true;
            try { _textBox.Text = text; }
            finally { _internalUpdating = false; }
        }

        private void OnTextChanged(object sender, EventArgs e)
        {
            if (_internalUpdating || IsDesignModeSafe()) return;
            if (UpdateParamFromText())
            {
                var handler = ParamValueChanged;
                if (handler != null) handler(this, EventArgs.Empty);
            }
        }

        private bool UpdateParamFromText()
        {
            if (_param == null || _textBox == null) return false;
            string raw = _textBox.Text.Trim();
            if (raw.Length == 0) return false; // ignore empty
            try
            {
                if (_param.ValueType == null)
                {
                    _param.Value = raw;
                    return true;
                }
            }
            catch { }

            try
            {
                var vt = _param.ValueType;
                string vtName = vt != null ? vt.ToString() : string.Empty;

                if (vtName == "Int") { int n; if (int.TryParse(raw, out n)) { _param.Value = n; return true; } return false; }
                if (vtName == "Byte") { byte n; if (byte.TryParse(raw, out n)) { _param.Value = n; return true; } return false; }
                if (vtName == "Uint") { uint n; if (uint.TryParse(raw, out n)) { _param.Value = n; return true; } return false; }
                if (vtName == "Double") { double n; if (double.TryParse(raw, out n)) { _param.Value = n; return true; } return false; }
                if (vtName == "String" || vtName == "Path" || vtName == "TimeSpanInfo") { _param.Value = raw; return true; }
                if (vtName == "XY_Coordinate") { var arr = ParseDoubleArray(raw, 2); if (arr == null) return false; _param.Value = new XyCoordinate { X = arr[0], Y = arr[1] }; return true; }
                if (vtName == "XYT_Coordinate") { var arr = ParseDoubleArray(raw, 3); if (arr == null) return false; _param.Value = new XytCoordinate { X = arr[0], Y = arr[1], T = arr[2] }; return true; }
                if (vtName == "XYZ_Coordinate") { var arr = ParseDoubleArray(raw, 3); if (arr == null) return false; _param.Value = new XyzCoordinate { X = arr[0], Y = arr[1], Z = arr[2] }; return true; }
                if (vtName == "XYZT_Coordinate") { var arr = ParseDoubleArray(raw, 4); if (arr == null) return false; _param.Value = new XyztCoordinate { X = arr[0], Y = arr[1], Z = arr[2], T = arr[3] }; return true; }
                if (vtName == "Size") { var arr = ParseIntArray(raw, 2); if (arr == null) return false; _param.Value = new Size(arr[0], arr[1]); return true; }
                if (vtName == "Point") { var arr = ParseIntArray(raw, 2); if (arr == null) return false; _param.Value = new Point(arr[0], arr[1]); return true; }
                if (vtName == "SizeD") { var arr = ParseDoubleArray(raw, 2); if (arr == null) return false; _param.Value = new SizeD { Width = arr[0], Height = arr[1] }; return true; }
                if (vtName == "PointD") { var arr = ParseDoubleArray(raw, 2); if (arr == null) return false; _param.Value = new PointD { X = arr[0], Y = arr[1] }; return true; }
                if (vtName == "RangeD") { var arr = ParseDoubleArray(raw, 2); if (arr == null) return false; _param.Value = new RangeD { Minimum = arr[0], Maximum = arr[1] }; return true; }
                if (vtName == "RectangleD") { var arr = ParseDoubleArray(raw, 4); if (arr == null) return false; _param.Value = new RectangleD { X = arr[0], Y = arr[1], Width = arr[2], Height = arr[3] }; return true; }
            }
            catch { return false; }
            return false;
        }

        private static bool IsCompositeDisplayType(Param param)
        {
            if (param == null) return false;
            try
            {
                string vt = param.ValueType != null ? param.ValueType.ToString() : string.Empty;
                switch (vt)
                {
                    case "XY_Coordinate": case "XYT_Coordinate": case "XYZ_Coordinate": case "XYZT_Coordinate":
                    case "Size": case "SizeD": case "Point": case "PointD": case "RectangleD": case "RangeD":
                        return true;
                }
            }
            catch { }
            return false;
        }

        private static string CleanCompositeValue(string source)
        {
            if (string.IsNullOrEmpty(source)) return string.Empty;
            string[] keywords = { "Width", "Height", "Minimum", "Maximum", "X", "Y", "Z", "T", "=", "[", "]", "{", "}" };
            foreach (var k in keywords) source = source.Replace(k, string.Empty);
            System.Text.StringBuilder sb = new System.Text.StringBuilder(source.Length);
            foreach (char c in source)
            {
                if (char.IsDigit(c) || c == '.' || c == ',' || c == '-' || c == ' ') sb.Append(c);
            }
            string cleaned = string.Join(",", sb.ToString().Replace(" ", string.Empty).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
            return cleaned;
        }

        private static double[] ParseDoubleArray(string raw, int expectedCount)
        {
            string cleaned = CleanCompositeValue(raw);
            string[] parts = cleaned.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != expectedCount) return null;
            double[] arr = new double[expectedCount];
            for (int i = 0; i < expectedCount; i++) { double v; if (!double.TryParse(parts[i], out v)) return null; arr[i] = v; }
            return arr;
        }

        private static int[] ParseIntArray(string raw, int expectedCount)
        {
            string cleaned = CleanCompositeValue(raw);
            string[] parts = cleaned.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != expectedCount) return null;
            int[] arr = new int[expectedCount];
            for (int i = 0; i < expectedCount; i++) { int v; if (!int.TryParse(parts[i], out v)) return null; arr[i] = v; }
            return arr;
        }
    }

    public class ParamDualTextControl : UserControl
    {
        private Label _label;
        private TextBox _textBoxX;
        private TextBox _textBoxY;
        private Param _param;
        private int _titleRatio = 50; // percent of total width for label
        private bool _internalUpdating;

        public int TitleRatio
        {
            get { return _titleRatio; }
            set
            {
                if (value < 0) value = 0;
                if (value > 80) value = 80; // leave room for two boxes
                if (_titleRatio == value) return;
                _titleRatio = value;
                if (!IsDesignModeSafe()) ResizeForTitleRatio();
            }
        }

        public event EventHandler ParamValueChanged;

        public ParamDualTextControl()
        {
            try
            {
                Height = 30;
                _label = new Label { Dock = DockStyle.Left, TextAlign = ContentAlignment.MiddleLeft, Width = 120 };
                var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, Padding = new Padding(0), Margin = new Padding(0) };
                _textBoxX = new TextBox { Width = 60, Margin = new Padding(0, 3, 4, 3) };
                _textBoxY = new TextBox { Width = 60, Margin = new Padding(0, 3, 0, 3) };
                _textBoxX.TextChanged += OnTextChanged;
                _textBoxY.TextChanged += OnTextChanged;
                panel.Controls.Add(_textBoxX);
                panel.Controls.Add(_textBoxY);
                Controls.Add(panel);
                Controls.Add(_label);
            }
            catch { }
        }

        private bool IsDesignModeSafe()
        {
            try { return LicenseManager.UsageMode == LicenseUsageMode.Designtime || (Site != null && Site.DesignMode); } catch { return false; }
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            if (IsDesignModeSafe()) return;
        }

        public void InitControl(Param param)
        {
            _param = param;
            if (!IsDesignModeSafe()) RefreshDisplay();
        }

        public Param GetParamData() => _param;

        public void SetReadOnlyTextbox(bool readOnly)
        {
            if (_textBoxX == null || _textBoxY == null) return;
            _textBoxX.ReadOnly = readOnly;
            _textBoxY.ReadOnly = readOnly;
            if (readOnly)
            {
                _textBoxX.BackColor = Color.Black; _textBoxX.ForeColor = Color.Lime; _textBoxX.TabStop = false;
                _textBoxY.BackColor = Color.Black; _textBoxY.ForeColor = Color.Lime; _textBoxY.TabStop = false;
            }
            else
            {
                _textBoxX.BackColor = SystemColors.Window; _textBoxX.ForeColor = SystemColors.WindowText;
                _textBoxY.BackColor = SystemColors.Window; _textBoxY.ForeColor = SystemColors.WindowText;
            }
        }

        public void SetValues(object x, object y)
        {
            SetTextboxTextSilently(_textBoxX, x != null ? x.ToString() : string.Empty);
            SetTextboxTextSilently(_textBoxY, y != null ? y.ToString() : string.Empty);
            if (!IsDesignModeSafe()) UpdateParamFromText();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (!IsDesignModeSafe()) ResizeForTitleRatio();
        }

        private void ResizeForTitleRatio()
        {
            if (Width <= 0 || _label == null) return;
            int labelWidth = (int)(Width * (_titleRatio / 100.0));
            _label.Width = labelWidth;
        }

        private void RefreshDisplay()
        {
            if (_label == null || _textBoxX == null || _textBoxY == null) return;
            if (_param == null)
            {
                SetTextboxTextSilently(_textBoxX, string.Empty);
                SetTextboxTextSilently(_textBoxY, string.Empty);
                _label.Text = string.Empty;
                return;
            }
            _label.Text = _param.Title ?? string.Empty;
            if (NeedsSplit(_param))
            {
                string cleaned = CleanCompositeValue(_param.ToString());
                var parts = cleaned.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    SetTextboxTextSilently(_textBoxX, parts[0]);
                    SetTextboxTextSilently(_textBoxY, parts[1]);
                }
            }
        }

        private void OnTextChanged(object sender, EventArgs e)
        {
            if (_internalUpdating || IsDesignModeSafe()) return;
            if (UpdateParamFromText())
            {
                var h = ParamValueChanged; if (h != null) h(this, EventArgs.Empty);
            }
        }

        private bool UpdateParamFromText()
        {
            if (_param == null || _textBoxX == null || _textBoxY == null) return false;
            string rawX = _textBoxX.Text.Trim();
            string rawY = _textBoxY.Text.Trim();
            if (rawX.Length == 0 || rawY.Length == 0) return false;
            string vtName = _param.ValueType != null ? _param.ValueType.ToString() : string.Empty;
            try
            {
                if (vtName == "XY_Coordinate") { double dx, dy; if (!double.TryParse(rawX, out dx) || !double.TryParse(rawY, out dy)) return false; _param.Value = new XyCoordinate { X = dx, Y = dy }; return true; }
                if (vtName == "Size") { int ix, iy; if (!int.TryParse(rawX, out ix) || !int.TryParse(rawY, out iy)) return false; _param.Value = new Size(ix, iy); return true; }
                if (vtName == "Point") { int ix, iy; if (!int.TryParse(rawX, out ix) || !int.TryParse(rawY, out iy)) return false; _param.Value = new Point(ix, iy); return true; }
                if (vtName == "SizeD") { double dx, dy; if (!double.TryParse(rawX, out dx) || !double.TryParse(rawY, out dy)) return false; _param.Value = new SizeD { Width = dx, Height = dy }; return true; }
                if (vtName == "PointD") { double dx, dy; if (!double.TryParse(rawX, out dx) || !double.TryParse(rawY, out dy)) return false; _param.Value = new PointD { X = dx, Y = dy }; return true; }
            }
            catch { return false; }
            return false;
        }

        private static bool NeedsSplit(Param param)
        {
            if (param == null) return false;
            string vt = param.ValueType != null ? param.ValueType.ToString() : string.Empty;
            switch (vt)
            {
                case "XY_Coordinate": case "Size": case "SizeD": case "Point": case "PointD":
                    return true;
            }
            return false;
        }

        private static string CleanCompositeValue(string source)
        {
            if (string.IsNullOrEmpty(source)) return string.Empty;
            string[] keywords = { "Width", "Height", "X", "Y", "=", "[", "]", "{", "}" };
            foreach (var k in keywords) source = source.Replace(k, string.Empty);
            System.Text.StringBuilder sb = new System.Text.StringBuilder(source.Length);
            foreach (char c in source)
            {
                if (char.IsDigit(c) || c == '.' || c == ',' || c == '-' || c == ' ') sb.Append(c);
            }
            string cleaned = string.Join(",", sb.ToString().Replace(" ", string.Empty).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
            return cleaned;
        }

        private void SetTextboxTextSilently(TextBox tb, string text)
        {
            if (tb == null) return;
            _internalUpdating = true;
            try { tb.Text = text; }
            finally { _internalUpdating = false; }
        }
    }
}
