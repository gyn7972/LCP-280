using System;
using System.Drawing;
using System.Windows.Forms;
using static QMC.Common.Material;

namespace QMC.LCP_280.Process.Component
{
    public partial class WaferMapView : UserControl
    {
        private MaterialCassette _materialCassette;
        private int _cellSize = 20;

        public WaferMapView()
        {
            InitializeComponent();
            // 고성능 / 깜빡임 최소화 스타일
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw,
                     true);
            UpdateStyles();

            if (groupBox != null)
            {
                groupBox.Paint -= GroupBox_Paint;
                groupBox.Paint += GroupBox_Paint;
            }
        }

        /// <summary>
        /// MaterialCassette 데이터를 설정합니다.
        /// </summary>
        /// <param name="materialCassette">MaterialCassette 객체</param>
        public void SetMaterialCassette(MaterialCassette materialCassette)
        {
            if (materialCassette == null) throw new ArgumentNullException(nameof(materialCassette));
            _materialCassette = materialCassette;
            AdjustCellSize();
            SafeInvalidate();
        }

        public void NotifyCassetteChanged()
        {
            // 외부에서 내부 슬롯 상태가 바뀐 후 호출
            AdjustCellSize();
            SafeInvalidate();
        }

        public void RefreshMapImmediate()
        {
            if (InvokeRequired)
            {
                try { BeginInvoke((Action)RefreshMapImmediate); } catch { }
                return;
            }
            AdjustCellSize();
            if (groupBox != null)
            {
                groupBox.Invalidate();
                groupBox.Update();
            }
            else
            {
                Invalidate();
                Update();
            }
        }

        /// <summary>
        /// 셀 크기를 조정합니다.
        /// </summary>
        private void AdjustCellSize()
        {
            if (_materialCassette == null || _materialCassette.SlotCount <= 0) return;
            int h = (groupBox?.ClientSize.Height ?? ClientSize.Height);
            if (h <= 0) return;
            _cellSize = Math.Max(1, h / _materialCassette.SlotCount);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            AdjustCellSize();
            SafeInvalidate();
        }

        private void SafeInvalidate()
        {
            if (!IsHandleCreated) return;
            if (InvokeRequired)
            {
                try { BeginInvoke((Action)SafeInvalidate); } catch { }
                return;
            }
            if (groupBox != null) groupBox.Invalidate();
            Invalidate(); // 부모도 함께
        }

        // 그룹박스 위에 직접 그리기
        private void GroupBox_Paint(object sender, PaintEventArgs e)
        {
            DrawMap(e.Graphics, ((Control)sender).ClientRectangle);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // 여전히 투명 영역 등에 대비해 부모에도 그리되, 실제 표시 영역은 groupBox에서 처리
            base.OnPaint(e);
            if (groupBox == null) DrawMap(e.Graphics, ClientRectangle);
        }

        private void DrawMap(Graphics g, Rectangle bounds)
        {
            // groupBox 텍스트 영역 보정 (단순 상단 마진 14 픽셀 가정)
            int topMargin = 14; // 폰트 높이에 따라 조정 가능
            var drawRect = new Rectangle(bounds.X + 2, bounds.Y + topMargin, bounds.Width - 4, bounds.Height - topMargin - 2);
            if (drawRect.Width <= 0 || drawRect.Height <= 0)
            {
                return;
            }

            g.Clear(Color.Black);

            if (_materialCassette == null || _materialCassette.SlotCount <= 0)
            {
                DrawCenterMessage(g, drawRect, "No Data");
                return;
            }

            int total = _materialCassette.SlotCount;
            if (_cellSize <= 0) return;

            int width = drawRect.Width;
            int height = drawRect.Height;
            for (int i = 0; i < total; i++)
            {
                var wafer = _materialCassette.GetWafer(i);
                Color cellColor = Color.Gray;
                if (wafer != null)
                {
                    switch (wafer.Presence)
                    {
                        case MaterialPresence.Exist: cellColor = Color.LimeGreen; break;
                        case MaterialPresence.NotExist: cellColor = Color.Red; break;
                        case MaterialPresence.Unknown: cellColor = Color.Yellow; break;
                    }
                    switch (wafer.ProcessSatate)
                    {
                        case MaterialProcessSatate.Ready: cellColor = Color.Blue; break;
                        case MaterialProcessSatate.Processing: cellColor = Color.Orange; break;
                        case MaterialProcessSatate.Completed: cellColor = Color.Green; break;
                        case MaterialProcessSatate.Unknown: break;
                    }
                }
                int y = drawRect.Bottom - (i + 1) * _cellSize;
                var rect = new Rectangle(drawRect.Left, y, width, _cellSize);
                using (var brush = new SolidBrush(cellColor)) g.FillRectangle(brush, rect);
                using (var pen = new Pen(Color.Black)) g.DrawRectangle(pen, rect);
            }
        }

        private void DrawCenterMessage(Graphics g, Rectangle area, string text)
        {
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            using (var f = new Font(Font.FontFamily, 9f, FontStyle.Italic))
            using (var b = new SolidBrush(Color.DarkGray))
            {
                g.DrawString(text, f, b, area, sf);
            }
        }
    }
}