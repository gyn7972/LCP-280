using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace QMC.Common.Vision
{
    [Serializable]
    public class PatternMatchResultOverlay : VisionImageOverlay
    {
        public Color Color { get; set; } = Color.Lime;
        public float Thickness { get; set; } = 2f;
        public DashStyle DashStyle { get; set; } = DashStyle.Solid;

        public PointF Center { get; set; }         
        public float PatternWidth { get; set; }    
        public float PatternHeight { get; set; }    
        public float AngleDeg { get; set; }        
        public int CrossHalfLenPx { get; set; } = 16;
        public int Index { get; set; } = -1;
        public bool Highlight { get; set; }
        public RectangleF SourceRect { get; set; } = RectangleF.Empty;
        public SizeF DestSize { get; set; } = SizeF.Empty;

        public PatternMatchResultOverlay()
        {
            Visible = true;
            //this.Color = Color.Lime;
            //this.Thickness = 2;
            //this.Visible = true;
            //this.DashStyle = DashStyle.Solid;
        }

        protected override void OnDraw(Point offset, SizeD? sourceSize, SizeD? destinateSize, Graphics graphics)
        {
            DrawInternal(graphics);
        }

        protected override void OnDraw(Point offset, SizeD? sourceSize, SizeD? destinateSize, BufferedGraphics graphics)
        {
            DrawInternal(graphics?.Graphics);
        }

        private void DrawInternal(Graphics g)
        {
            if (!Visible || g == null) return;
            if (SourceRect.Width <= 0 || SourceRect.Height <= 0 || DestSize.Width <= 0 || DestSize.Height <= 0)
                return; // 아직 Viewer가 주입 안했거나 유효하지 않음

            // 절대 → 상대
            float relX = (Center.X - SourceRect.Left) / SourceRect.Width;   // 0~1
            float relY = (Center.Y - SourceRect.Top) / SourceRect.Height;   // 0~1
            if (relX < 0 || relX > 1 || relY < 0 || relY > 1)
                return; // 현재 잘라진 영역 밖 → 표시하지 않음

            // 화면 픽셀
            float cx = relX * DestSize.Width;
            float cy = relY * DestSize.Height;

            float sx = DestSize.Width / SourceRect.Width;
            float sy = DestSize.Height / SourceRect.Height;

            using (var penCross = new Pen(Color, Thickness) { DashStyle = DashStyle })
            using (var penRect = new Pen(Color, Math.Max(1, Thickness - 1)) { DashStyle = DashStyle })
            {
                // Cross
                int half = CrossHalfLenPx;
                g.DrawLine(penCross, cx - half, cy, cx + half, cy);
                g.DrawLine(penCross, cx, cy - half, cx, cy + half);

                // Pattern 사각형 (회전 가능)
                if (PatternWidth > 0 && PatternHeight > 0)
                {
                    float hw = PatternWidth * 0.5f * sx;
                    float hh = PatternHeight * 0.5f * sy;
                    var rectPts = new[]
                    {
                        new PointF(cx - hw, cy - hh),
                        new PointF(cx + hw, cy - hh),
                        new PointF(cx + hw, cy + hh),
                        new PointF(cx - hw, cy + hh)
                    };

                    if (Math.Abs(AngleDeg) > float.Epsilon)
                    {
                        using (var m = new Matrix())
                        {
                            m.RotateAt(AngleDeg, new PointF(cx, cy));
                            m.TransformPoints(rectPts);
                        }
                    }
                    g.DrawPolygon(penRect, rectPts);
                }

                // Highlight(대표)
                if (Highlight)
                {
                    using (var penHL = new Pen(Color, Thickness) { DashStyle = DashStyle.Dash })
                    {
                        float r = (Math.Max(PatternWidth, PatternHeight) * 0.5f) * ((sx + sy) * 0.5f) + 10f;
                        g.DrawEllipse(penHL, cx - r, cy - r, r * 2, r * 2);
                    }
                }

                // Index
                if (Index >= 0)
                {
                    using (var f = new Font("Consolas", 11f, FontStyle.Bold))
                    using (var b = new SolidBrush(Color.White))
                    {
                        g.DrawString(Index.ToString(), f, b, cx + half + 4, cy - half - 4);
                    }
                }
            }
        }
    }
}