using QMC.LCP_280.Process;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D; // SmoothingMode 추가
using System.Linq;
using System.Windows.Forms;

namespace QMC.Common.Controls
{
    public partial class DisplayView_DieOutput : UserControl
    {
        public enum ItemState
        {
            Empty,
            Present,
            Placed,
            Rejected,
            Error,
            Skip
        }

        public class DisplayItem
        {
            public PointD Position { get; set; }   // 중심 (0,0) 기준 좌표 (double)
            public Point DieMap { get; set; }      // 다이 맵 좌표 (int)
            public ItemState State { get; set; }   // 상태
            public string Info { get; set; } = ""; // 추가 정보
            public int DieId { get; set; }         // 다이 ID
            public Color? FillColor { get; set; }
            public Color? BorderColor { get; set; }
        }

        public class DisplayItemEventArgs : EventArgs
        {
            public DisplayItem Item { get; set; }
            public Point ScreenPosition { get; set; }
        }

        public event EventHandler<DisplayItemEventArgs> ItemDoubleClicked;
        public event EventHandler<DisplayItemEventArgs> ItemHovered;
        public event EventHandler<DisplayItemEventArgs> MotorMoveRequested;

        private List<DisplayItem> _items = new List<DisplayItem>();
        private float _scale = 1.0f;
        private PointF _offset = PointF.Empty;
        private bool _dragging = false;
        private Point _lastMouse;
        private float _chipSizeRatio = 0.95f; // 기존 0.8f에서 0.95f로 변경하여 간격을 좁힘

        private ToolTip _toolTip;
        private DisplayItem _hoveredItem = null;
        private Timer _hoverTimer;

        // [Performance] 캐시된 경계값 (OnPaint에서 매번 계산하지 않기 위함)
        private double _cachedMaxAbsX = 1.0;
        private double _cachedMaxAbsY = 1.0;

        // [Performance] GDI 객체 재사용을 위한 캐시
        private Dictionary<ItemState, SolidBrush> _stateBrushes = new Dictionary<ItemState, SolidBrush>();
        private Dictionary<ItemState, Pen> _statePens = new Dictionary<ItemState, Pen>();
        public IReadOnlyList<DisplayItem> Items => _items;
        public DisplayItem HitTest(Point clientPoint) => GetItemAtPosition(clientPoint);

        public DisplayView_DieOutput()
        {
            this.DoubleBuffered = true;
            this.BackColor = Color.LightGray;

            _toolTip = new ToolTip();
            _toolTip.AutoPopDelay = 5000;
            _toolTip.InitialDelay = 500;
            _toolTip.ReshowDelay = 100;

            _hoverTimer = new Timer();
            _hoverTimer.Interval = 100;
            _hoverTimer.Tick += HoverTimer_Tick;

            this.MouseWheel += DisplayView_MouseWheel;
            this.MouseDown += DisplayView_MouseDown;
            this.MouseMove += DisplayView_MouseMove;
            this.MouseUp += DisplayView_MouseUp;
            this.MouseDoubleClick += Display_MouseDoubleClick;
            this.MouseClick += Display_MouseClick;
            this.MouseLeave += DisplayView_MouseLeave;

            // [Performance] 기본 브러시/펜 초기화
            InitializeGdiResources();
        }

        public void SetItems(List<DisplayItem> items)
        {
            _items = items ?? new List<DisplayItem>();

            // [Performance] 데이터가 변경될 때 경계값을 미리 한 번만 계산
            if (_items.Count > 0)
            {
                _cachedMaxAbsX = _items.Max(d => Math.Abs(d.Position.X));
                _cachedMaxAbsY = _items.Max(d => Math.Abs(d.Position.Y));
                if (_cachedMaxAbsX <= 0) _cachedMaxAbsX = 1;
                if (_cachedMaxAbsY <= 0) _cachedMaxAbsY = 1;
            }
            else
            {
                _cachedMaxAbsX = 1;
                _cachedMaxAbsY = 1;
            }

            this.Invalidate();
        }

        private DisplayItem GetItemAtPosition(Point mousePos)
        {
            if (_items == null || _items.Count == 0) return null;

            int cx = this.Width / 2;
            int cy = this.Height / 2;

            double baseScale = Math.Min(
                ((this.Width / 2.0) - 20.0) / _cachedMaxAbsX,
                ((this.Height / 2.0) - 20.0) / _cachedMaxAbsY
            );

            int itemSize = Math.Max(2, (int)(baseScale * _scale * _chipSizeRatio));
            // [Performance] 제곱근 연산 제거를 위해 반지름의 제곱 사용
            int hitRadius = Math.Max(itemSize / 2, 3);
            long hitRadiusSq = hitRadius * hitRadius;

            // 역방향 루프나 QuadTree 등을 쓰면 더 빠르지만, 단순 리스트에선 루프 최적화만 적용
            foreach (var item in _items)
            {
                // 화면 좌표 계산
                int x = (int)(cx + item.Position.X * baseScale * _scale + (double)_offset.X);
                int y = (int)(cy - item.Position.Y * baseScale * _scale + (double)_offset.Y);

                // [Performance] Bounding Box 1차 체크 (매우 빠른 정수 연산)
                if (mousePos.X < x - hitRadius || mousePos.X > x + hitRadius ||
                    mousePos.Y < y - hitRadius || mousePos.Y > y + hitRadius)
                {
                    continue;
                }

                // [Performance] 거리 계산 시 Sqrt 제거
                long dx = mousePos.X - x;
                long dy = mousePos.Y - y;
                long distSq = dx * dx + dy * dy;

                if (distSq <= hitRadiusSq)
                {
                    return item;
                }
            }
            return null;
        }


        private void DisplayView_MouseWheel(object sender, MouseEventArgs e)
        {
            _scale *= (e.Delta > 0) ? 1.1f : 0.9f;
            _scale = Math.Max(0.1f, Math.Min(50f, _scale)); // 줌 최대 배율 좀 더 넉넉하게
            this.Invalidate();
        }

        private void DisplayView_MouseDown(object sender, MouseEventArgs e)
        {
            _dragging = true;
            _lastMouse = e.Location;
        }

        private void DisplayView_MouseMove(object sender, MouseEventArgs e)
        {
            if (_dragging)
            {
                _offset.X += (e.X - _lastMouse.X);
                _offset.Y += (e.Y - _lastMouse.Y);
                _lastMouse = e.Location;
                this.Invalidate();
            }
            else
            {
                var item = GetItemAtPosition(e.Location);
                if (item != _hoveredItem)
                {
                    _hoveredItem = item;
                    _hoverTimer.Stop();
                    _hoverTimer.Start();
                }
                this.Invalidate();
            }
        }

        private void DisplayView_MouseUp(object sender, MouseEventArgs e)
        {
            _dragging = false;
        }

        private void DisplayView_MouseLeave(object sender, EventArgs e)
        {
            _hoveredItem = null;
            _toolTip.Hide(this);
            _hoverTimer.Stop();
            this.Invalidate(); // 호버 박스 지우기 위해 갱신
        }

        private void HoverTimer_Tick(object sender, EventArgs e)
        {
            _hoverTimer.Stop();

            if (_hoveredItem != null)
            {
                string tooltipText = $"MapPos: ({_hoveredItem.DieMap.X}, {_hoveredItem.DieMap.Y})\n";
                tooltipText += $"State: {_hoveredItem.State}\n";
                tooltipText += $"Die ID: {_hoveredItem.DieId + 1}";

                if (!string.IsNullOrEmpty(_hoveredItem.Info))
                    tooltipText += $"\nInfo: {_hoveredItem.Info}";

                _toolTip.Show(tooltipText, this, Cursor.Position.X - this.PointToScreen(Point.Empty).X + 10,
                             Cursor.Position.Y - this.PointToScreen(Point.Empty).Y - 10);

                ItemHovered?.Invoke(this, new DisplayItemEventArgs
                {
                    Item = _hoveredItem,
                    ScreenPosition = Cursor.Position
                });
            }
            else
            {
                _toolTip.Hide(this);
            }
        }

        private void Display_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    var item = GetItemAtPosition(e.Location);
                    if (item != null)
                    {
                        //Out은 우선 움직이지 말자.
                        //ShowMotorMovePopup(item);
                    }
                    break;
                case MouseButtons.Right:
                    _scale = 1.0f;
                    _offset = PointF.Empty;
                    this.Invalidate();
                    break;
            }
        }

        private void Display_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                var item = GetItemAtPosition(e.Location);
                if (item != null)
                {
                    ItemDoubleClicked?.Invoke(this, new DisplayItemEventArgs
                    {
                        Item = item,
                        ScreenPosition = e.Location
                    });
                }
            }
        }

        private void ShowMotorMovePopup(DisplayItem item)
        {
            var mb = new MessageBoxOk();
            if (Equipment.Instance.EqState == EquipmentState.Starting ||
                Equipment.Instance.EqState == EquipmentState.AutoRunning)
            {
                mb.ShowDialog("Warring", "장비가 운전 중입니다. 정지 후 시도하세요.");
                return;
            }

            var ask = new MessageBoxYesNo();
            if (ask.ShowDialog("모터 이동 확인", $"Die:{item.DieId + 1}로 이동하시겠습니까?") == DialogResult.Yes)
            {
                MotorMoveRequested?.Invoke(this, new DisplayItemEventArgs
                {
                    Item = item,
                    ScreenPosition = Point.Empty
                });
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // base.OnPaint(e); // 배경색 자동처리보다 명시적 Clear가 깜빡임 제어에 유리할 수 있음
            var g = e.Graphics;
            g.Clear(this.BackColor);

            if (_items == null || _items.Count == 0) 
                return;

            if (_scale < 2.0f)
            {
                g.SmoothingMode = SmoothingMode.None;
            }
            else
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
            }

            int cx = this.Width / 2;
            int cy = this.Height / 2;

            // [Performance] 캐시된 경계값 사용
            double baseScale = Math.Min(
                ((this.Width / 2.0) - 20.0) / _cachedMaxAbsX,
                ((this.Height / 2.0) - 20.0) / _cachedMaxAbsY
            );

            // 미리 계산된 스케일 팩터
            double finalScale = baseScale * _scale; //
            int itemSize = Math.Max(2, (int)(finalScale * _chipSizeRatio));
            int halfSize = itemSize / 2;

            // 화면 영역 (Culling 용)
            Rectangle clipRect = e.ClipRectangle;

            // [Performance] 하이라이트 펜 생성 (Loop 밖)
            using (Pen highlightPen = new Pen(Color.Red, 2))
            {
                foreach (var item in _items)
                {
                    // 화면 좌표 계산
                    int x = (int)(cx + item.Position.X * finalScale + (double)_offset.X);
                    int y = (int)(cy - item.Position.Y * finalScale + (double)_offset.Y);

                    // [Performance] Culling: 화면 밖의 아이템은 그리지 않음 (매우 중요)
                    if (x + halfSize < clipRect.Left || x - halfSize > clipRect.Right ||
                        y + halfSize < clipRect.Top || y - halfSize > clipRect.Bottom)
                    {
                        continue;
                    }

                    // [Performance] 브러시/펜 선택 (커스텀 or 캐시)
                    Brush brushToUse;
                    Pen penToUse;
                    bool needDispose = false;

                    // 커스텀 색상이 있는지 확인
                    if (item.FillColor.HasValue || item.BorderColor.HasValue)
                    {
                        brushToUse = new SolidBrush(item.FillColor ?? Color.LightGray);
                        penToUse = new Pen(item.BorderColor ?? Color.DarkGray);
                        needDispose = true;
                    }
                    else
                    {
                        // 캐시된 리소스 사용
                        if (!_stateBrushes.TryGetValue(item.State, out var cachedBrush))
                            cachedBrush = _stateBrushes[ItemState.Empty];

                        if (!_statePens.TryGetValue(item.State, out var cachedPen))
                            cachedPen = _statePens[ItemState.Empty];

                        brushToUse = cachedBrush;
                        penToUse = cachedPen;
                    }

                    var rect = new Rectangle(x - halfSize, y - halfSize, itemSize, itemSize);
                    g.FillRectangle(brushToUse, rect);
                    if (itemSize > 4)
                        g.DrawRectangle(penToUse, rect);

                    // 커스텀 객체인 경우만 해제
                    if (needDispose)
                    {
                        brushToUse.Dispose();
                        penToUse.Dispose();
                    }

                    // 호버 효과
                    if (item == _hoveredItem && itemSize > 2)
                    {
                        var inflateRect = rect;
                        inflateRect.Inflate(2, 2);
                        g.DrawRectangle(highlightPen, inflateRect);
                    }
                }
            }
        }

        public void SetChipSizeRatio(float ratio)
        {
            _chipSizeRatio = Math.Max(0.1f, Math.Min(2.0f, ratio));
            this.Invalidate();
        }


        // [Performance] 상태별 브러시/펜을 미리 생성하여 재사용 (OnPaint에서 매번 생성하는 것을 방지)
        private void InitializeGdiResources()
        {
            // 상태별 기본 색상 미리 정의
            AddStateResource(ItemState.Empty, Color.LightGray, Color.DarkGray);
            AddStateResource(ItemState.Present, Color.Black, Color.DarkGray);
            AddStateResource(ItemState.Placed, Color.LimeGreen, Color.Green);
            AddStateResource(ItemState.Rejected, Color.Red, Color.DarkRed);
            AddStateResource(ItemState.Error, Color.Red, Color.IndianRed);
            AddStateResource(ItemState.Skip, Color.DimGray, Color.DarkGoldenrod);
        }

        // 상태별 브러시/펜을 생성하여 캐시에 추가
        private void AddStateResource(ItemState state, Color fill, Color border)
        {
            _stateBrushes[state] = new SolidBrush(fill);
            _statePens[state] = new Pen(border);
        }

        // [Performance] 폼이 닫힐 때 리소스 해제 필요
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var b in _stateBrushes.Values) b.Dispose();
                foreach (var p in _statePens.Values) p.Dispose();
                _stateBrushes.Clear();
                _statePens.Clear();
                _hoverTimer?.Dispose();
                _toolTip?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
