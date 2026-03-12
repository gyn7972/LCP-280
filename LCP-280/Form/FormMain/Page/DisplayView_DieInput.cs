using QMC.LCP_280.Process;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D; // SmoothingMode 추가
using System.Linq;
using System.Windows.Forms;

namespace QMC.Common.Controls
{
    public partial class DisplayView_DieInput : UserControl
    {
        public enum ItemState
        {
            Empty,          // 흰색
            Present,        // 검은색
            AfterPickUp,    // 흰색
            PickedUp,       // 초록색
            Rejected,       // 빨강색
            Skip
        }

        public class DisplayItem
        {
            public PointD Position { get; set; }   // 중심 (0,0) 기준 좌표
            public PointD DieMap { get; set; }     // 다이 맵 좌표 (MapX, MapY)
            public ItemState State { get; set; }  // 상태
            public string Info { get; set; } = "";// 추가 정보
            public int DieId { get; set; }        // 다이 ID
            // 0=기본, 1=Download(파랑), 2=Scan(빨강)
            public int GroupId { get; set; }
        }

        public class DisplayItemEventArgs : EventArgs
        {
            public DisplayItem Item { get; set; }
            public PointD ScreenPosition { get; set; }
        }

        public event EventHandler<DisplayItemEventArgs> ItemDoubleClicked;
        public event EventHandler<DisplayItemEventArgs> ItemHovered;
        public event EventHandler<DisplayItemEventArgs> MotorMoveRequested;

        private List<DisplayItem> _items = new List<DisplayItem>();
        private float _scale = 1.0f;
        private PointD _offset = PointD.Empty;
        private bool _dragging = false;
        private PointD _lastMouse;
        private float _chipSizeRatio = 0.95f; // 기존 0.8f에서 0.95f로 변경하여 간격을 좁힘

        private ToolTip _toolTip;
        private DisplayItem _hoveredItem = null;
        private Timer _hoverTimer;

        // [Performance] 캐시된 경계값
        private double _cachedMaxAbsX = 1.0;
        private double _cachedMaxAbsY = 1.0;

        public DisplayView_DieInput()
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
        }

        public void SetItems(List<DisplayItem> items)
        {
            _items = items ?? new List<DisplayItem>();

            // [Performance] 경계값 미리 계산
            if (_items.Count > 0)
            {
                _cachedMaxAbsX = _items.Max(d => Math.Abs(d.Position.X));
                _cachedMaxAbsY = _items.Max(d => Math.Abs(d.Position.Y));
                if (_cachedMaxAbsX == 0) _cachedMaxAbsX = 1;
                if (_cachedMaxAbsY == 0) _cachedMaxAbsY = 1;
            }
            else
            {
                _cachedMaxAbsX = 1;
                _cachedMaxAbsY = 1;
            }

            this.Invalidate();
        }

        private DisplayItem GetItemAtPosition(PointD mousePos)
        {
            if (_items == null || _items.Count == 0) return null;

            int cx = this.Width / 2;
            int cy = this.Height / 2;

            double baseScale = Math.Min(
                (float)(this.Width / 2 - 20) / _cachedMaxAbsX,
                (float)(this.Height / 2 - 20) / _cachedMaxAbsY
            );

            int itemSize = Math.Max(2, (int)(baseScale * _scale * _chipSizeRatio));
            // [Performance] 제곱값 비교 사용
            int hitRadius = Math.Max(itemSize / 2, 3);
            long hitRadiusSq = hitRadius * hitRadius;

            foreach (var item in _items)
            {
                int x = (int)(cx + item.Position.X * baseScale * _scale + _offset.X);
                int y = (int)(cy - item.Position.Y * baseScale * _scale + _offset.Y);

                // [Performance] Bounding Box 1차 체크
                if (mousePos.X < x - hitRadius || mousePos.X > x + hitRadius ||
                    mousePos.Y < y - hitRadius || mousePos.Y > y + hitRadius)
                    continue;

                // [Performance] Sqrt 제거
                double dx = mousePos.X - x;
                double dy = mousePos.Y - y;
                double distSq = dx * dx + dy * dy;

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
            _scale = Math.Max(0.1f, Math.Min(50f, _scale)); // 줌 범위 확장
            this.Invalidate();
        }

        private void DisplayView_MouseDown(object sender, MouseEventArgs e)
        {
            _dragging = true;
            _lastMouse = (PointD)e.Location;
        }

        private void DisplayView_MouseMove(object sender, MouseEventArgs e)
        {
            if (_dragging)
            {
                _offset.X += (e.X - _lastMouse.X);
                _offset.Y += (e.Y - _lastMouse.Y);
                _lastMouse = (PointD)e.Location;
                this.Invalidate();
            }
            else
            {
                var item = GetItemAtPosition((PointD)e.Location);
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
            this.Invalidate();
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

                _toolTip.Show(tooltipText, this, Cursor.Position.X - this.PointToScreen(PointD.Empty).X + 10,
                             Cursor.Position.Y - this.PointToScreen(PointD.Empty).Y - 10);

                ItemHovered?.Invoke(this, new DisplayItemEventArgs
                {
                    Item = _hoveredItem,
                    ScreenPosition = (PointD)Cursor.Position
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
                    var item = GetItemAtPosition((PointD)e.Location);
                    if (item != null)
                    {
                        ShowMotorMovePopup(item);
                    }
                    break;
                case MouseButtons.Right:
                    _scale = 1.0f;
                    _offset = PointD.Empty;
                    this.Invalidate();
                    break;
            }
        }

        private void Display_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                var item = GetItemAtPosition((PointD)e.Location);
                if (item != null)
                {
                    ItemDoubleClicked?.Invoke(this, new DisplayItemEventArgs
                    {
                        Item = item,
                        ScreenPosition = (PointD)e.Location
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
                    ScreenPosition = PointD.Empty
                });
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // [Performance] 배경색으로 Clear
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
                (float)(this.Width / 2 - 20) / _cachedMaxAbsX,
                (float)(this.Height / 2 - 20) / _cachedMaxAbsY
            );

            // 미리 계산된 스케일 팩터
            double finalScale = baseScale * _scale;
            int itemSize = Math.Max(2, (int)(finalScale * _chipSizeRatio));
            int halfSize = itemSize / 2;

            // 화면 영역 (Culling 용)
            Rectangle clipRect = e.ClipRectangle;

            // [Performance] 하이라이트 펜 생성 (Loop 밖)
            using (Pen highlightPen = new Pen(Color.Red, 2))
            {
                foreach (var item in _items)
                {
                    int x = (int)(cx + item.Position.X * finalScale + _offset.X);
                    int y = (int)(cy - item.Position.Y * finalScale + _offset.Y);

                    // [Performance] Culling: 화면 밖 아이템 그리기 스킵
                    if (x + halfSize < clipRect.Left || x - halfSize > clipRect.Right ||
                        y + halfSize < clipRect.Top || y - halfSize > clipRect.Bottom)
                    {
                        continue;
                    }

                    Brush brush = Brushes.LightGray;
                    Pen pen = Pens.DarkGray;

                    switch (item.State)
                    {
                        case ItemState.Present:
                            brush = Brushes.Black;
                            pen = Pens.DarkGray;
                            break;
                        case ItemState.PickedUp:
                            brush = Brushes.LimeGreen;
                            pen = Pens.Green;
                            break;
                        case ItemState.AfterPickUp:
                            brush = Brushes.Green;
                            pen = Pens.DodgerBlue;
                            break;
                        case ItemState.Rejected:
                            brush = Brushes.Red;
                            pen = Pens.LightGray;
                            break;
                        case ItemState.Skip:
                            brush = Brushes.DimGray;
                            pen = Pens.DarkGoldenrod;
                            break;
                        case ItemState.Empty:
                        default:
                            brush = Brushes.White;
                            pen = Pens.LightGray;
                            break;
                    }

                    // ADD: 그룹별 색상 우선 적용
                    if (item.GroupId == 1) // Download
                    {
                        if (item.State == ItemState.Present || item.State == ItemState.Empty)
                        {
                            brush = Brushes.DodgerBlue;
                            pen = Pens.Navy;
                        }
                    }
                    else if (item.GroupId == 2) // Scan
                    {
                        if (item.State == ItemState.Present || item.State == ItemState.Empty)
                        {
                            brush = Brushes.Red;
                            pen = Pens.DarkRed;
                        }
                    }

                    var rect = new Rectangle(x - halfSize, y - halfSize, itemSize, itemSize);
                    g.FillRectangle(brush, rect);

                    if (itemSize > 4)
                    {
                        g.DrawRectangle(pen, rect);
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
    }
}