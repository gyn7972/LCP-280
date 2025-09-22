using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QMC.Common.PKGTester
{
    public class CieChromaticityControl : UserControl
    {
        private const float XMin = 0f, XMax = 0.8f;
        private const float YMin = 0f, YMax = 0.9f;
        private readonly Padding _pad = new Padding(60, 20, 20, 50);
        private Bitmap _bgBitmap;
        private bool _bgDirty = true;

        private readonly (int nm, float x, float y)[] _spectral = CIEChromaticity.CIE1931_ColorMatchFunctionValue.Select(item =>
        {
            float sum = item.xbar + item.ybar + item.zbar;
            float x = item.xbar / sum;
            float y = item.ybar / sum;
            return (item.nm, x, y);
        })
        .ToArray();

        public CieChromaticityControl()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
            BackColor = Color.White;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            _bgDirty = true;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var plot = PlotArea(ClientRectangle, _pad);

            if (_bgDirty) BuildBackground(plot);
            if (_bgBitmap != null) e.Graphics.DrawImage(_bgBitmap, plot);

            DrawAxes(e.Graphics, plot);
            DrawSpectralLocus(e.Graphics, plot);
        }

        private RectangleF PlotArea(Rectangle client, Padding pad)
        {
            return new RectangleF(client.Left + pad.Left, client.Top + pad.Top,
                client.Width - pad.Left - pad.Right, client.Height - pad.Top - pad.Bottom);
        }

        private void BuildBackground(RectangleF plot)
        {
            _bgBitmap?.Dispose();
            _bgBitmap = new Bitmap((int)plot.Width, (int)plot.Height, PixelFormat.Format32bppArgb);

            var locusAbs = _spectral.Select(s => XYToPixel(new PointF(s.x, s.y), plot)).ToList();
            locusAbs.Add(locusAbs[0]);
            var polyLocal = locusAbs.Select(p => new PointF(p.X - plot.Left, p.Y - plot.Top)).ToArray();

            var rect = new Rectangle(0, 0, _bgBitmap.Width, _bgBitmap.Height);
            var data = _bgBitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            unsafe
            {
                byte* ptr = (byte*)data.Scan0;
                int width = _bgBitmap.Width;
                int height = _bgBitmap.Height;
                float left = plot.Left;
                float top = plot.Top;
                System.Threading.Tasks.Parallel.For(0, height, j =>
                {
                    for (int i = 0; i < width; i++)
                    {
                        if (!PointInPolygon(polyLocal, i, j))
                        {
                            SetPixel(ptr, data.Stride, i, j, Color.White);
                            continue;
                        }
                        var abs = new PointF(left + i, top + j);
                        var xy = PixelToXY(abs, plot);
                        var col = XYtoSRGB(xy.X, xy.Y);
                        SetPixel(ptr, data.Stride, i, j, col);
                    }
                });
            }
            _bgBitmap.UnlockBits(data);
            _bgDirty = false;
        }

        private void DrawAxes(Graphics g, RectangleF plot)
        {
            var axisPen = new Pen(Color.Black, 1.2f);
            var gridPen = new Pen(Color.Gainsboro, 1f) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dot };
            var font = new Font("Segoe UI", 8.5f);
            var gray = new SolidBrush(Color.DimGray);

            // Draw grid lines (격자, 점선)
            for (float x = 0.0f; x <= 0.8f; x += 0.1f)
            {
                var p = XYToPixel(new PointF(x, 0), plot);
                g.DrawLine(gridPen, p.X, plot.Top, p.X, plot.Bottom);
            }
            for (float y = 0.0f; y <= 0.9f; y += 0.1f)
            {
                var p = XYToPixel(new PointF(0, y), plot);
                g.DrawLine(gridPen, plot.Left, p.Y, plot.Right, p.Y);
            }

            // Draw axes (테두리)
            g.DrawRectangle(axisPen, plot.X, plot.Y, plot.Width, plot.Height);

            // Draw axis ticks and labels (눈금)
            for (float x = 0.0f; x <= 0.8f; x += 0.1f)
            {
                var p = XYToPixel(new PointF(x, 0), plot);
                var label = x.ToString("0.0");
                var sz = g.MeasureString(label, font);
                g.DrawString(label, font, gray, p.X - sz.Width / 2f, plot.Bottom + 4);
            }
            for (float y = 0.0f; y <= 0.9f; y += 0.1f)
            {
                var p = XYToPixel(new PointF(0, y), plot);
                var label = y.ToString("0.0");
                var sz = g.MeasureString(label, font);
                g.DrawString(label, font, gray, plot.Left - sz.Width - 6, p.Y - sz.Height / 2f);
            }

            // Draw axis labels
            var labelFont = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            var xLbl = "x";
            var yLbl = "y";
            var xSize = g.MeasureString(xLbl, labelFont);
            var ySize = g.MeasureString(yLbl, labelFont);
            g.DrawString(xLbl, labelFont, Brushes.Black, plot.Right - xSize.Width, plot.Bottom + 22);
            g.TranslateTransform(plot.Left - 30, plot.Top + ySize.Width);
            g.RotateTransform(-90);
            g.DrawString(yLbl, labelFont, Brushes.Black, 0, 0);
            g.ResetTransform();
        }

        private void DrawSpectralLocus(Graphics g, RectangleF plot)
        {
            var pts = _spectral.Select(s => XYToPixel(new PointF(s.x, s.y), plot)).ToArray();
            var pen = new Pen(Color.Black, 2f);
            g.DrawLines(pen, pts);

            var purple = new Pen(Color.MediumVioletRed, 2f);
            g.DrawLine(purple, pts[pts.Length - 1], pts[0]);

            //var font = new Font("Segoe UI", 8);
            //foreach (var s in _spectral.Where(s => s.nm % 20 == 0))
            //{
            //    var p = XYToPixel(new PointF(s.x, s.y), plot);
            //    var offset = new PointF((p.X - plot.Width / 2) * 0.05f, (p.Y - plot.Height / 2) * 0.05f);
            //    g.DrawString($"{s.nm}", font, Brushes.Black, p.X + offset.X, p.Y + offset.Y);
            //}
        }

        private PointF XYToPixel(PointF xy, RectangleF plot)
        {
            float px = plot.Left + (xy.X - XMin) / (XMax - XMin) * plot.Width;
            float py = plot.Bottom - (xy.Y - YMin) / (YMax - YMin) * plot.Height;
            return new PointF(px, py);
        }

        private PointF PixelToXY(PointF pAbs, RectangleF plot)
        {
            float x = XMin + (pAbs.X - plot.Left) / plot.Width * (XMax - XMin);
            float y = YMin + (plot.Bottom - pAbs.Y) / plot.Height * (YMax - YMin);
            return new PointF(x, y);
        }

        private static bool PointInPolygon(PointF[] poly, float x, float y)
        {
            bool inside = false;
            for (int i = 0, j = poly.Length - 1; i < poly.Length; j = i++)
            {
                if (((poly[i].Y > y) != (poly[j].Y > y)) &&
                    (x < (poly[j].X - poly[i].X) * (y - poly[i].Y) / (poly[j].Y - poly[i].Y) + poly[i].X))
                    inside = !inside;
            }
            return inside;
        }

        private static unsafe void SetPixel(byte* basePtr, int stride, int x, int y, Color c)
        {
            byte* px = basePtr + y * stride + x * 4;
            px[0] = c.B;
            px[1] = c.G;
            px[2] = c.R;
            px[3] = c.A;
        }

        private static Color XYtoSRGB(float x, float y)
        {
            if (y <= 0 || x <= 0 || (x + y) >= 1.0f)
                return Color.White;

            float Y = 1f;
            float Xx = (x / y) * Y;
            float Zz = ((1 - x - y) / y) * Y;

            double Rlin = 3.2406 * Xx - 1.5372 * Y - 0.4986 * Zz;
            double Glin = -0.9689 * Xx + 1.8758 * Y + 0.0415 * Zz;
            double Blin = 0.0557 * Xx - 0.2040 * Y + 1.0570 * Zz;

            Rlin = Math.Max(0, Rlin);
            Glin = Math.Max(0, Glin);
            Blin = Math.Max(0, Blin);

            double max = Math.Max(1e-6, Math.Max(Rlin, Math.Max(Glin, Blin)));
            Rlin /= max; Glin /= max; Blin /= max;

            byte R = ToSrgbByte(Rlin);
            byte G = ToSrgbByte(Glin);
            byte B = ToSrgbByte(Blin);

            return Color.FromArgb(255, R, G, B);
        }

        private static byte ToSrgbByte(double c)
        {
            double v = c <= 0.0031308
                ? 12.92 * c
                : (1.055 * Math.Pow(c, 1.0 / 2.4) - 0.055);

            int iv = (int)Math.Round(255.0 * Math.Max(0, Math.Min(1, v)));
            return (byte)iv;
        }
    }
}