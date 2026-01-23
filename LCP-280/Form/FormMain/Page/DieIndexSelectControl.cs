using QMC.Common;
using QMC.LCP_280.Process.Component;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Unit.FormMain
{
    public partial class DieIndexSelectControl : UserControl
    {
        private class SocketView
        {
            public Rotary.SocketInfo Socket { get; private set; }
            public int Number { get { return Socket.No + 1; } } // 1~8
            public PointF Position { get; set; }
            public SocketView(Rotary.SocketInfo s) { Socket = s; }
            public RectangleF GetBounds(float size)
            {
                return new RectangleF(
                    Position.X - size / 2f,
                    Position.Y - size / 2f,
                    size,
                    size);
            }
        }

        public event EventHandler<Rotary.SocketInfo> SocketClicked;
        public event EventHandler<Rotary.SocketInfo> SocketStateChanged;
        public event EventHandler<Rotary.SocketInfo> SocketSelected;
        public event EventHandler<Rotary.SocketInfo> SocketHovered;
        public event EventHandler<int> RotationRequested; // int = 회전 스텝(+1/-1)
        public event EventHandler<Rotary.SocketInfo> SelectIndexRequested;

        private Rotary _rotary;
        private readonly List<SocketView> _socketViews = new List<SocketView>();

        private SocketView _selectedSocketView;
        private SocketView _hoveredSocketView;
        private bool _isAutoSequencing;

        private ToolTip _toolTip;
        private Timer _hoverTimer;

        // View Transform
        private float _scale = 1.0f;
        private PointF _offset = PointF.Empty;
        private bool _isDragging;
        private Point _lastMousePos;
        private const float MIN_SCALE = 0.5f;
        private const float MAX_SCALE = 3.0f;
        private const float SCALE_FACTOR = 1.1f;
        private const float BASE_DIE_SIZE = 40f;

        private readonly Dictionary<int, PointF> _fixedLabelPositions = new Dictionary<int, PointF>();
        private bool _labelPositionsInitialized;
        private readonly SizeF _baseSize = new SizeF(400, 300);

        public DieIndexSelectControl()
        {
            InitializeComponent();
            InitializeToolTip();
            InitializeSocketsPlaceholder();
            SetupMouseHandlers();
            SetupDoubleBuffering();
        }

        public void BindRotary(Rotary rotary)
        {
            if (_rotary != null)
                _rotary.LoadIndexChanged -= Rotary_LoadIndexChanged;

            _rotary = rotary;
            RebuildSocketViews();

            if (_rotary != null)
            {
                _rotary.LoadIndexChanged += Rotary_LoadIndexChanged;
                SyncRotationFromLoadIndex(_rotary.GetLoadIndexNo());
            }

            CalculateSocketPositions();
            InitializeFixedLabelPositions();
            displayPanel.Invalidate();

        }

        public void Rotary_LoadIndexChanged(object sender, int loadIndex0Based)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<object, int>(Rotary_LoadIndexChanged), sender, loadIndex0Based);
                return;
            }
            SyncRotationFromLoadIndex(loadIndex0Based);
        }

        private void SyncRotationFromLoadIndex(int loadIndex0Based)
        {
            // offset 제거: 단순 재계산
            CalculateSocketPositions();
            displayPanel.Invalidate();
        }

        private void RebuildSocketViews()
        {
            _socketViews.Clear();
            if (_rotary == null) return;
            var sockets = _rotary.GetAllSockets();
            if (sockets == null) return;
            foreach (var s in sockets)
                _socketViews.Add(new SocketView(s));
        }

        private void InitializeSocketsPlaceholder()
        {
            _socketViews.Clear();
            for (int i = 0; i < 8; i++)
            {
                var dummy = new Rotary.SocketInfo(i, i * 45);
                _socketViews.Add(new SocketView(dummy));
            }
            CalculateSocketPositions();
            InitializeFixedLabelPositions();
        }

        private void SetupDoubleBuffering()
        {
            typeof(Panel).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic,
                null, displayPanel, new object[] { true });
        }

        private void InitializeToolTip()
        {
            _toolTip = new ToolTip
            {
                AutoPopDelay = 5000,
                InitialDelay = 300,
                ReshowDelay = 100
            };

            _hoverTimer = new Timer { Interval = 100 };
            _hoverTimer.Tick += HoverTimer_Tick;
        }

        private void SetupMouseHandlers()
        {
            // 디자이너에서 이미 구독된 이벤트: Paint, MouseMove, MouseClick, Resize
            // 여기서는 중복을 피하고 필요한 추가 이벤트만 연결합니다.
            displayPanel.MouseWheel += DisplayPanel_MouseWheel;
            displayPanel.MouseDown += DisplayPanel_MouseDown;
            displayPanel.MouseUp += DisplayPanel_MouseUp;
            displayPanel.MouseDoubleClick += DisplayPanel_MouseDoubleClick;
            displayPanel.MouseLeave += DisplayPanel_MouseLeave;

            // 휠 스크롤 안정화를 위해 포커스 부여
            displayPanel.MouseEnter += (s, e) => displayPanel.Focus();
            //displayPanel.MouseWheel += DisplayPanel_MouseWheel;
            //displayPanel.MouseDown += DisplayPanel_MouseDown;
            //displayPanel.MouseUp += DisplayPanel_MouseUp;
            //displayPanel.MouseMove += DisplayPanel_MouseMove;
            //displayPanel.MouseClick += DisplayPanel_MouseClick;
            //displayPanel.MouseDoubleClick += DisplayPanel_MouseDoubleClick;
            //displayPanel.MouseLeave += DisplayPanel_MouseLeave;
            //displayPanel.Paint += DisplayPanel_Paint;
            //displayPanel.Resize += DisplayPanel_Resize;
        }

        private void CalculateSocketPositions()
        {
            if (_socketViews.Count == 0) return;

            float centerX = _baseSize.Width / 2f;
            float centerY = _baseSize.Height / 2f;
            float radius = 80f;

            // GUI 기준 상대 각도 (Load 위치 = 상대 0 → 9시 방향 고정)
            int[] guiAngles = { 180, 225, 270, 315, 0, 45, 90, 135 };

            int loadIndex = 0;
            if (_rotary != null)
                loadIndex = _rotary.GetLoadIndexNo() & 7;

            foreach (var sv in _socketViews)
            {
                int phys = sv.Socket.No & 7;
                int relative = (phys - loadIndex + 8) & 7;
                //int angleDeg = guiAngles[relative]; //정방향
                int angleDeg = guiAngles[(8 - relative) & 7];       // 역방향 매핑 (핵심 변경)
                double rad = angleDeg * Math.PI / 180.0;

                float x = centerX + radius * (float)Math.Cos(rad);
                float y = centerY - radius * (float)Math.Sin(rad);
                sv.Position = new PointF(x, y);
            }
        }

        private void InitializeFixedLabelPositions()
        {
            if (_labelPositionsInitialized) return;

            _fixedLabelPositions.Clear();
            float centerX = _baseSize.Width / 2f;
            float centerY = _baseSize.Height / 2f;
            float radius = 80f;
            float dieSize = BASE_DIE_SIZE;

            for (int number = 1; number <= 8; number++)
            {
                int arrayIndex = number - 1;
                double startAngle = 180;
                double angleRadians = (startAngle + arrayIndex * 45) * Math.PI / 180;
                float x = centerX + radius * (float)Math.Cos(angleRadians);
                float y = centerY - radius * (float)Math.Sin(angleRadians);
                PointF diePos = new PointF(x, y);
                PointF labelPos = GetLabelPosition(diePos, number, dieSize);
                _fixedLabelPositions[number] = labelPos;
            }
            _labelPositionsInitialized = true;
        }

        private void HoverTimer_Tick(object sender, EventArgs e)
        {
            _hoverTimer.Stop();
            if (_hoveredSocketView != null)
            {
                var s = _hoveredSocketView.Socket;
                var die = s.GetMaterialDie();
                bool hasDie = HasPhysicalDie(s);

                string tooltipText = $"Socket: {_hoveredSocketView.Number}\nState: {s.State}";
                tooltipText += $"\nMaterial: {(hasDie ? "Exist" : "None")}";

                if (hasDie && die != null)
                {
                    tooltipText += $"\nDieState: {die.State}";
                }

                tooltipText += $"\nUpdated: {s.LastUpdated:HH:mm:ss}";

                Point mousePos = displayPanel.PointToClient(Cursor.Position);
                _toolTip.Show(tooltipText, displayPanel, mousePos.X + 10, mousePos.Y - 10);
                SocketHovered?.Invoke(this, s);
            }
            else
            {
                _toolTip.Hide(displayPanel);
            }

            //_hoverTimer.Stop();
            //if (_hoveredSocketView != null)
            //{
            //    var s = _hoveredSocketView.Socket;
            //    string tooltipText = $"Socket: {_hoveredSocketView.Number}\nState: {s.State}";
            //    var md = s.GetMaterialDie();
            //    if (md != null)
            //    {
            //        tooltipText += $"\nMaterial: {(md.Presence == Material.MaterialPresence.Exist ? "Exist" : "None")}";
            //        tooltipText += $"\nDieState: {md.State}";
            //    }
            //    tooltipText += $"\nUpdated: {s.LastUpdated:HH:mm:ss}";

            //    Point mousePos = displayPanel.PointToClient(Cursor.Position);
            //    _toolTip.Show(tooltipText, displayPanel, mousePos.X + 10, mousePos.Y - 10);
            //    SocketHovered?.Invoke(this, s);
            //}
            //else
            //{
            //    _toolTip.Hide(displayPanel);
            //}
        }
        //private void HoverTimer_Tick(object sender, EventArgs e)
        //{
        //    _hoverTimer.Stop();

        //    if (_hoveredSocketView != null)
        //    {
        //        var s = _hoveredSocketView.Socket;
        //        string tooltipText = $"Socket: {_hoveredSocketView.Number}\nState: {s.State}";
        //        var md = s.GetMaterialDie();
        //        if (md != null)
        //            tooltipText += $"\nMaterial: {(md.Presence == Material.MaterialPresence.Exist ? "Exist" : "None")}";
        //        tooltipText += $"\nUpdated: {s.LastUpdated:HH:mm:ss}";

        //        Point mousePos = displayPanel.PointToClient(Cursor.Position);
        //        _toolTip.Show(tooltipText, displayPanel, mousePos.X + 10, mousePos.Y - 10);
        //        SocketHovered?.Invoke(this, s);
        //    }
        //    else
        //    {
        //        _toolTip.Hide(displayPanel);
        //    }
        //}

        private void DisplayPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // 배경을 먼저 지우고 변환 적용
            g.Clear(Color.White);

            g.TranslateTransform(
                _offset.X + (displayPanel.Width - _baseSize.Width) / 2f,
                _offset.Y + (displayPanel.Height - _baseSize.Height) / 2f);
            g.ScaleTransform(_scale, _scale);

            float size = BASE_DIE_SIZE;

            // 최신 소켓 상태가 필요하다면 여기서 갱신 (참조 사용 시 생략 가능)
            if (_rotary != null)
            {
                var latest = _rotary.GetAllSockets();
                if (latest != null && latest.Length == _socketViews.Count)
                {
                    // 필요한 경우 상태 참조를 동기화할 수 있음 (현재는 참조 그대로 사용)
                }
            }

            foreach (var sv in _socketViews)
            {
                DrawSocket(g, sv, size);
            }

            DrawFixedLabels(g, size);
        }

        //private void DrawSocket(Graphics g, SocketView sv, float size)
        //{
        //    var socket = sv.Socket;
        //    Color fill, text, border;
        //    GetSocketColors(socket, out fill, out text, out border);

        //    RectangleF bounds = sv.GetBounds(size);

        //    if (sv == _selectedSocketView)
        //    {
        //        using (Pen hoverPen = new Pen(Color.FromArgb(200, Color.Gold), 2f / _scale))
        //        {
        //            float inflate = 2f / _scale;
        //            RectangleF hb = new RectangleF(bounds.X - inflate, bounds.Y - inflate,
        //                bounds.Width + inflate * 2, bounds.Height + inflate * 2);
        //            g.DrawEllipse(hoverPen, hb);
        //        }
        //    }

        //    using (Brush b = new SolidBrush(fill))
        //        g.FillEllipse(b, bounds);
        //    using (Pen p = new Pen(border, 1.5f / _scale))
        //        g.DrawEllipse(p, bounds);

        //    using (Brush tb = new SolidBrush(text))
        //    using (Font font = new Font("Arial", size * 0.5f, FontStyle.Bold))
        //    {
        //        string txt = sv.Number.ToString();
        //        var ts = g.MeasureString(txt, font);
        //        g.DrawString(txt, font, tb,
        //            sv.Position.X - ts.Width / 2f,
        //            sv.Position.Y - ts.Height / 2f);
        //    }
        //}


        // === Die 존재 판정 헬퍼 추가 ===
        private bool HasPhysicalDie(Rotary.SocketInfo socket)
        {
            if (socket == null) return false;
            var die = socket.GetMaterialDie();
            if (die == null) return false;

            // 소켓 상태가 Empty / Loading 이면 아직 물리 Die 없음으로 간주
            if (socket.State == Rotary.RotarySocketState.Empty ||
                socket.State == Rotary.RotarySocketState.Loading)
                return false;

            return die.Presence == Material.MaterialPresence.Exist;
        }

        private void DrawSocket(Graphics g, SocketView sv, float size)
        {
            var socket = sv.Socket;
            Color fill, text, border;
            GetSocketColors(socket, out fill, out text, out border);

            bool hasDie = HasPhysicalDie(socket);
            // 물리 Die 없으면 강제로 Empty 스타일 적용
            if (!hasDie)
            {
                fill = Color.FromArgb(170, 170, 170);
                text = Color.FromArgb(60, 60, 60);
                border = Color.FromArgb(100, 100, 100);
            }


            RectangleF bounds = sv.GetBounds(size);

            if (sv == _selectedSocketView)
            {
                using (Pen hoverPen = new Pen(Color.FromArgb(200, Color.Gold), 2f / _scale))
                {
                    float inflate = 2f / _scale;
                    RectangleF hb = new RectangleF(bounds.X - inflate, bounds.Y - inflate,
                        bounds.Width + inflate * 2, bounds.Height + inflate * 2);
                    g.DrawEllipse(hoverPen, hb);
                }
            }

            using (Brush b = new SolidBrush(fill))
                g.FillEllipse(b, bounds);
            using (Pen p = new Pen(border, 1.5f / _scale))
                g.DrawEllipse(p, bounds);

            // 번호
            using (Brush tb = new SolidBrush(text))
            using (Font font = new Font("Arial", size * 0.5f, FontStyle.Bold))
            {
                string txt = sv.Number.ToString();
                var ts = g.MeasureString(txt, font);
                g.DrawString(txt, font, tb,
                    sv.Position.X - ts.Width / 2f,
                    sv.Position.Y - ts.Height / 2f);
            }

            // 존재할 때만 배지
            if (hasDie)
            {
                var die = socket.GetMaterialDie();
                DrawDieBadge(g, bounds, die, socket.UseSocket);
            }
            //// [ADD] 소켓 내 Die 상태 배지 표시 (우하단)
            //var die = socket.GetMaterialDie();
            //if (die != null)
            //{
            //    DrawDieBadge(g, bounds, die, socket.UseSocket);
            //}
        }

        // [ADD] Die 상태를 작은 배지로 표시 (우하단 원형)
        private void DrawDieBadge(Graphics g, RectangleF socketBounds, MaterialDie die, bool useSocket)
        {
            if (die == null)
                return;
            // 존재하지 않으면 배지 숨김
            if (die.Presence != Material.MaterialPresence.Exist)
                return;

            float badgeSize = socketBounds.Width * 0.38f;
            float pad = socketBounds.Width * 0.06f;
            var badge = new RectangleF(
                socketBounds.Right - badgeSize - pad,
                socketBounds.Bottom - badgeSize - pad,
                badgeSize,
                badgeSize
            );

            var color = GetDieStateColor(die.State);
            var label = GetDieStateShortLabel(die.State);

            // 비사용 소켓이면 색상 투명도 낮춤
            if (!useSocket)
                color = Color.FromArgb(140, color);

            using (Brush b = new SolidBrush(color))
                g.FillEllipse(b, badge);

            using (Pen p = new Pen(Color.FromArgb(220, 30, 30, 30), 1.2f / _scale))
                g.DrawEllipse(p, badge);

            // 상태 약어 텍스트
            using (var f = new Font("Segoe UI", Math.Max(7f, badge.Width * 0.33f), FontStyle.Bold))
            using (var sb = new SolidBrush(Color.White))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                g.DrawString(label, f, sb, new PointF(badge.X + badge.Width / 2f, badge.Y + badge.Height / 2f), sf);
            }
        }

        // [ADD] Die 상태 → 대표 색상 맵핑
        private Color GetDieStateColor(DieProcessState state)
        {
            switch (state)
            {
                case DieProcessState.None: return Color.Gray;
                case DieProcessState.Mapped: return Color.SteelBlue;
                case DieProcessState.Picked: return Color.MediumSeaGreen;
                case DieProcessState.Inspected: return Color.MediumPurple;
                case DieProcessState.Rejected: return Color.IndianRed;
                case DieProcessState.Placed: return Color.Teal;
                default: return Color.DimGray;
            }
        }

        // [ADD] Die 상태 약어 표시
        private string GetDieStateShortLabel(DieProcessState state)
        {
            switch (state)
            {
                case DieProcessState.None: return "-";
                case DieProcessState.Mapped: return "M";
                case DieProcessState.Picked: return "P";
                case DieProcessState.Inspected: return "I";
                case DieProcessState.Rejected: return "R";
                case DieProcessState.Placed: return "C"; // Completed 의미로 C
                default: return "?";
            }
        }


        private void GetSocketColors(Rotary.SocketInfo s, out Color fill, out Color text, out Color border)
        {
            border = Color.FromArgb(70, 70, 70);
            text = Color.White;
            switch (s.State)
            {
                case Rotary.RotarySocketState.Empty:
                    fill = Color.FromArgb(170, 170, 170); 
                    text = Color.FromArgb(60, 60, 60); break;
                case Rotary.RotarySocketState.Loading: 
                    fill = Color.SkyBlue; break;
                case Rotary.RotarySocketState.Loaded: 
                    fill = Color.FromArgb(0, 160, 0); break;
                case Rotary.RotarySocketState.MAligning:
                    fill = Color.Orange; break;
                case Rotary.RotarySocketState.MAligned: 
                    fill = Color.DarkOrange; break;
                case Rotary.RotarySocketState.Probing: 
                    fill = Color.MediumPurple; break;
                case Rotary.RotarySocketState.Probed: 
                    fill = Color.Indigo; break;
                case Rotary.RotarySocketState.VAligning:
                    fill = Color.MediumOrchid; break;
                case Rotary.RotarySocketState.VAligned:
                    fill = Color.DarkOrchid; break;
                case Rotary.RotarySocketState.Unloading:
                    fill = Color.Goldenrod; break;
                case Rotary.RotarySocketState.Unloaded: 
                    fill = Color.Teal; break;
                case Rotary.RotarySocketState.Completed: 
                    fill = Color.DarkGreen; break;
                case Rotary.RotarySocketState.Error: 
                    fill = Color.Red; break;
                default: 
                    fill = Color.Gray; break;
            }
            if (!s.UseSocket)
            {
                fill = ControlPaint.Light(fill);
                text = Color.FromArgb(100, text);
            }
        }

        private void DrawFixedLabels(Graphics g, float dieSize)
        {
            if (!_labelPositionsInitialized) return;
            foreach (var kv in _fixedLabelPositions)
            {
                int number = kv.Key;
                PointF pos = kv.Value;
                string labelText = GetDieLabelText(number);
                if (string.IsNullOrEmpty(labelText)) continue;
                using (var brush = new SolidBrush(Color.Black))
                using (var font = new Font("맑은 고딕", 8f / _scale, FontStyle.Regular))
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                {
                    g.DrawString(labelText, font, brush, pos, sf);
                }
            }
        }

        private string GetDieLabelText(int dieNumber)
        {
            switch (dieNumber)
            {
                case 1: return "Loader";
                case 2: return "Align";
                case 3: return "Probe";
                case 5: return "VisionAlign\nUnloader";
                default: return "";
            }
        }

        private PointF GetLabelPosition(PointF diePosition, int dieNumber, float dieSize)
        {
            float d = dieSize * 1.8f;
            switch (dieNumber)
            {
                case 1: return new PointF(diePosition.X - d, diePosition.Y);
                case 2: return new PointF(diePosition.X - d * 0.7f, diePosition.Y + d * 0.7f);
                case 3: return new PointF(diePosition.X, diePosition.Y + d);
                case 5: return new PointF(diePosition.X + d, diePosition.Y);
                default: return diePosition;
            }
        }

        private void DisplayPanel_MouseWheel(object sender, MouseEventArgs e)
        {
            float centerOffsetX = (displayPanel.Width - _baseSize.Width) / 2f;
            float centerOffsetY = (displayPanel.Height - _baseSize.Height) / 2f;
            PointF mousePos = new PointF(e.X - centerOffsetX, e.Y - centerOffsetY);

            float old = _scale;
            _scale = e.Delta > 0 ? _scale * SCALE_FACTOR : _scale / SCALE_FACTOR;
            _scale = Math.Max(MIN_SCALE, Math.Min(MAX_SCALE, _scale));

            if (old != _scale)
            {
                float k = _scale / old;
                _offset.X = mousePos.X - (mousePos.X - _offset.X) * k;
                _offset.Y = mousePos.Y - (mousePos.Y - _offset.Y) * k;
            }
            displayPanel.Invalidate();
        }

        private void DisplayPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                var sv = GetSocketAtPoint(e.Location);
                if (sv != null)
                {
                    OnSocketClick(sv);
                }
                else
                {
                    _isDragging = true;
                    _lastMousePos = e.Location;
                    displayPanel.Cursor = Cursors.Hand;
                }
            }
        }

        private void DisplayPanel_MouseUp(object sender, MouseEventArgs e)
        {
            _isDragging = false;
            UpdateCursor(e.Location);
        }

        private void DisplayPanel_MouseClick(object sender, MouseEventArgs e) 
        {

        }

        private void DisplayPanel_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ResetView();
                return;
            }

            if (e.Button != MouseButtons.Left)
                return;

            var sv = GetSocketAtPoint(e.Location);
            if (sv == null || _rotary == null)
                return;

            // ★ 중요: UI가 가진 Socket이 아니라, Rotary 내부의 "실제 Socket"을 가져와서 수정해야 시퀀스와 연동됨
            var realSocket = _rotary.GetSocket(sv.Socket.No);
            if (realSocket == null)
                return;

            // Die 존재 여부 체크(실제 소켓 기준): 없으면 적용 금지
            if (!HasPhysicalDie(realSocket))
            {
                MessageBox.Show($"Socket {realSocket.No + 1}에 Die가 없습니다.", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var ask = new MessageBoxDieStatusSelect
            {
                Title = $"Update Data: {realSocket.LastUpdated:yyyy-MM-dd HH:mm:ss}",
                Message = $"Index '{realSocket.No}' is currently selected, and its status is '{realSocket.State}'."
            };

            if (ask.ShowDialog() != DialogResult.OK)
                return;

            // 상태 업데이트 (실제 소켓 객체 수정)
            switch (ask.RotateStatus)
            {
                case (int)Rotary.RotarySocketState.Empty:
                    realSocket.SetState(Rotary.RotarySocketState.Empty);
                    break;
                case (int)Rotary.RotarySocketState.Loading:
                    realSocket.SetState(Rotary.RotarySocketState.Loading);
                    break;
                case (int)Rotary.RotarySocketState.Loaded:
                    realSocket.SetState(Rotary.RotarySocketState.Loaded);
                    break;
                case (int)Rotary.RotarySocketState.MAligning:
                    realSocket.SetState(Rotary.RotarySocketState.MAligning);
                    break;
                case (int)Rotary.RotarySocketState.MAligned:
                    realSocket.SetState(Rotary.RotarySocketState.MAligned);
                    break;
                case (int)Rotary.RotarySocketState.Probing:
                    realSocket.SetState(Rotary.RotarySocketState.Probing);
                    break;
                case (int)Rotary.RotarySocketState.Probed:
                    realSocket.SetState(Rotary.RotarySocketState.Probed);
                    break;
                case (int)Rotary.RotarySocketState.VAligning:
                    realSocket.SetState(Rotary.RotarySocketState.VAligning);
                    break;
                case (int)Rotary.RotarySocketState.VAligned:
                    realSocket.SetState(Rotary.RotarySocketState.VAligned);
                    break;
                case (int)Rotary.RotarySocketState.Unloading:
                    realSocket.SetState(Rotary.RotarySocketState.Unloading);
                    break;
                case (int)Rotary.RotarySocketState.Unloaded:
                    realSocket.SetState(Rotary.RotarySocketState.Unloaded);
                    break;
                case (int)Rotary.RotarySocketState.Completed:
                    realSocket.SetState(Rotary.RotarySocketState.Completed);
                    break;
                case (int)Rotary.RotarySocketState.Error:
                    realSocket.SetState(Rotary.RotarySocketState.Error);
                    break;
            }

            // UI selection이 참조하는 SocketView도 realSocket을 바라보게 갱신(표시 일관성)
            try
            {
                // SocketView.Socket은 private set이라 교체는 못하지만,
                // 현재는 내부 배열의 같은 객체를 수정했으므로 UI도 자동으로 동일 객체 상태를 보게 됨.
                SocketStateChanged?.Invoke(this, realSocket);
            }
            catch { }

            UpdateInfoPanel();
            displayPanel.Invalidate();
        }
        private void DisplayPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                _offset.X += e.X - _lastMousePos.X;
                _offset.Y += e.Y - _lastMousePos.Y;
                _lastMousePos = e.Location;
                displayPanel.Invalidate();
            }
            else
            {
                var hovered = GetSocketAtPoint(e.Location);
                if (hovered != _hoveredSocketView)
                {
                    _hoveredSocketView = hovered;
                    _hoverTimer.Stop();
                    _hoverTimer.Start();
                }
                UpdateCursor(e.Location);
            }
        }

        private void DisplayPanel_MouseLeave(object sender, EventArgs e)
        {
            _hoveredSocketView = null;
            _toolTip.Hide(displayPanel);
            _hoverTimer.Stop();
        }

        private void UpdateCursor(Point loc)
        {
            var sv = GetSocketAtPoint(loc);
            displayPanel.Cursor = (sv != null) ? Cursors.Hand : Cursors.Default;
        }

        private SocketView GetSocketAtPoint(Point screenPoint)
        {
            PointF real = ScreenToReal(screenPoint);
            float size = BASE_DIE_SIZE;

            foreach (var sv in _socketViews)
            {
                float dx = real.X - sv.Position.X;
                float dy = real.Y - sv.Position.Y;
                float dist = (float)Math.Sqrt(dx * dx + dy * dy);
                if (dist <= size / 2f) return sv;
            }
            return null;
        }

        private PointF ScreenToReal(Point p)
        {
            float centerOffsetX = (displayPanel.Width - _baseSize.Width) / 2f;
            float centerOffsetY = (displayPanel.Height - _baseSize.Height) / 2f;

            float x = (p.X - _offset.X - centerOffsetX) / _scale;
            float y = (p.Y - _offset.Y - centerOffsetY) / _scale;
            return new PointF(x, y);
        }

        private void OnSocketClick(SocketView sv)
        {
            _selectedSocketView = sv;
            UpdateInfoPanel();
            SocketClicked?.Invoke(this, sv.Socket);
            SocketSelected?.Invoke(this, sv.Socket);
            displayPanel.Invalidate();
        }

        // ===== Rotation (offset 제거) =====
        public void RotateCounterClockwise()
        {
            // 필요 시 방향 반대면 step = -1;
            RequestRotationStep(+1);
        }

        private void RequestRotationStep(int step)
        {
            RotationRequested?.Invoke(this, step);
        }

        // 하위호환 (더 이상 offset 사용 안함)
        public void UpdateRotationUI(int ignored)
        {
            CalculateSocketPositions();
            displayPanel.Invalidate();
        }

        public void UpdateLoadingNumber(int ignored)
        {
            CalculateSocketPositions();
            displayPanel.Invalidate();
        }

        public Rotary.SocketInfo GetSelectedSocket()
            => _selectedSocketView != null ? _selectedSocketView.Socket : null;

        public void ResetView()
        {
            _scale = 1.0f;
            _offset = PointF.Empty;
            displayPanel.Invalidate();
        }

        public void ClearSelection()
        {
            _selectedSocketView = null;
            UpdateInfoPanel();
            displayPanel.Invalidate();
        }

        public void ResetLabelPositions()
        {
            _labelPositionsInitialized = false;
            _fixedLabelPositions.Clear();
            InitializeFixedLabelPositions();
            displayPanel.Invalidate();
        }

        private void UpdateInfoPanel()
        {
            if (_selectedSocketView != null)
            {
                var s = _selectedSocketView.Socket;
                lblDieNumberValue.Text = _selectedSocketView.Number.ToString();

                var die = s.GetMaterialDie();
                if (HasPhysicalDie(s) && die != null)
                {
                    lblDieIdValue.Text = BuildDieInfoText(die);
                }
                else
                {
                    lblDieIdValue.Text = "EMPTY";
                }
            }
            else
            {
                lblDieNumberValue.Text = "0";
                lblDieIdValue.Text = "N/A";
            }
        }

        private string BuildDieInfoText(MaterialDie die)
        {
            // 표시 우선순위: Index, SourceWaferId, TargetWaferId, TargetChipIndex, TargetSlot, State
            var parts = new List<string>();
            if (die.Index >= 0) 
                parts.Add($"Idx:{die.Index}");

            if (!string.IsNullOrEmpty(die.SourceWaferId)) 
                parts.Add($"Src:{die.SourceWaferId}");

            if (!string.IsNullOrEmpty(die.TargetWaferId)) 
                parts.Add($"Tgt:{die.TargetWaferId}");

            if (die.TargetChipIndex >= 0) 
                parts.Add($"Chip:{die.TargetChipIndex}");

            if (die.TargetSlot >= 0) 
                parts.Add($"Slot:{die.TargetSlot}");

            parts.Add($"State:{die.State}");

            return string.Join(" | ", parts);
        }


        private void DisplayPanel_Resize(object sender, EventArgs e)
        {
            displayPanel.Invalidate();
        }

        private async void btnAutoSequence_Click(object sender, EventArgs e)
        {
            return;

            if (_isAutoSequencing) 
                return;

            _isAutoSequencing = true;
            btnAutoSequence.Enabled = false;
            btnReset.Enabled = false;

            try
            {
                foreach (var sv in _socketViews.OrderBy(v => v.Number))
                {
                    _selectedSocketView = sv;
                    UpdateInfoPanel();
                    displayPanel.Invalidate();
                    await Task.Delay(300);
                }
            }
            finally
            {
                _isAutoSequencing = false;
                btnAutoSequence.Enabled = true;
                btnReset.Enabled = true;
            }
        }

        private void btnRotateCounterClockwise_Click(object sender, EventArgs e)
        {
            if (_isAutoSequencing) return;
            RotateCounterClockwise();
        }

        public void Reset()
        {
            // 1) Rotary 소켓 데이터 초기화 (Material/State/Tag + (실기면) IO OFF)
            try
            {
                // 전체 소켓 클리어
                _rotary?.ClearSocketData(socketNo: -1, offIo: true, resetState: true);

                // UseSocket(Enable) 설정 반영까지 포함해서 초기화하고 싶으면 아래도 선택적으로 호출 가능
                // _rotary?.ResetForNewRun(clearSockets: true, moveIndexToSafe: false);

                // UI 회전/표시도 현재 loadIndex 기준으로 즉시 갱신
                if (_rotary != null)
                    SyncRotationFromLoadIndex(_rotary.GetLoadIndexNo());
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }

            // 2) UI 선택/뷰 초기화
            _selectedSocketView = null;
            _hoveredSocketView = null;
            _toolTip?.Hide(displayPanel);
            _hoverTimer?.Stop();

            UpdateInfoPanel();
            displayPanel.Invalidate();
        }

        private void btnReset_Click_1(object sender, EventArgs e)
        {
            var ask = new MessageBoxYesNo();
            if (ask.ShowDialog("Question", "Index 정보를 초기화하시겠습니까?") != DialogResult.Yes)
            {
                return;
            }


            Reset();
        }
    }
}