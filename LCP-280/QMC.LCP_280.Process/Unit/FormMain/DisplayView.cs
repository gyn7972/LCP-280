using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace QMC.Common.Controls   // 공용 네임스페이스
{
    public partial class DisplayView : UserControl
    {
        public enum ItemState
        {
            Empty,   // 흰색
            Present, // 검은색
            Picked   // 초록색
        }

        public class DisplayItem
        {
            public Point Position { get; set; }   // 중심 (0,0) 기준 좌표
            public ItemState State { get; set; }  // 상태
        }

        private List<DisplayItem> _items = new List<DisplayItem>();
        private float _scale = 1.0f;
        private PointF _offset = PointF.Empty;
        private bool _dragging = false;
        private Point _lastMouse;

        public DisplayView()
        {
            this.DoubleBuffered = true;
            this.BackColor = Color.LightGray;

            this.MouseWheel += DisplayView_MouseWheel;
            this.MouseDown += DisplayView_MouseDown;
            this.MouseMove += DisplayView_MouseMove;
            this.MouseUp += DisplayView_MouseUp;
        }

        /// <summary>데이터 세팅</summary>
        public void SetItems(List<DisplayItem> items)
        {
            _items = items ?? new List<DisplayItem>();
            this.Invalidate();
        }

        /// <summary>줌 기능 (휠)</summary>
        private void DisplayView_MouseWheel(object sender, MouseEventArgs e)
        {
            _scale *= (e.Delta > 0) ? 1.1f : 0.9f;
            _scale = Math.Max(0.1f, Math.Min(10f, _scale));
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
        }

        private void DisplayView_MouseUp(object sender, MouseEventArgs e)
        {
            _dragging = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.Clear(Color.LightGray);

            if (_items == null || _items.Count == 0) return;

            int cx = this.Width / 2;
            int cy = this.Height / 2;

            // 데이터 범위 계산
            int maxX = _items.Max(d => Math.Abs(d.Position.X));
            int maxY = _items.Max(d => Math.Abs(d.Position.Y));
            if (maxX == 0) maxX = 1;
            if (maxY == 0) maxY = 1;

            // 전체 데이터가 화면에 들어오도록 기본 스케일
            float baseScale = Math.Min(
                (float)(this.Width / 2 - 20) / maxX,
                (float)(this.Height / 2 - 20) / maxY
            );

            // 칩 크기
            int itemSize = Math.Max(2, (int)(baseScale * _scale * 0.5f));

            foreach (var item in _items)
            {
                Brush brush = Brushes.White;
                switch (item.State)
                {
                    case ItemState.Present: brush = Brushes.Black; break;
                    case ItemState.Picked: brush = Brushes.LimeGreen; break;
                }

                int x = (int)(cx + item.Position.X * baseScale * _scale + _offset.X);
                int y = (int)(cy - item.Position.Y * baseScale * _scale + _offset.Y);

                g.FillEllipse(brush, x - itemSize / 2, y - itemSize / 2, itemSize, itemSize);
            }
        }

    }
}
