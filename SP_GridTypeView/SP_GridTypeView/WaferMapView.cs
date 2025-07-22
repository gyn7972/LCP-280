using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SP_GridTypeView
{
    public class WaferMapView : UserControl
    {
        private List<WaferMapItem> _items = new List<WaferMapItem>();
        private int _cellSize = 20; // 기본 셀 크기
        private int _gridWidth = 0; // 그리드의 가로 크기
        private int _gridHeight = 0; // 그리드의 세로 크기

        public WaferMapView()
        {
            DoubleBuffered = true; // 깜빡임 방지
        }

        /// <summary>
        /// WaferMapItem 컬렉션을 설정합니다.
        /// </summary>
        /// <param name="items">WaferMapItem 리스트</param>
        public void SetItems(List<WaferMapItem> items)
        {
            _items = items ?? throw new ArgumentNullException(nameof(items));

            // 그리드 크기 계산
            if (_items.Count > 0)
            {
                _gridWidth = _items.Max(item => item.X) + 1;
                _gridHeight = _items.Max(item => item.Y) + 1;
            }

            AdjustCellSize(); // 셀 크기 조정
            Invalidate(); // 다시 그리기
        }

        /// <summary>
        /// 셀 크기를 조정합니다.
        /// </summary>
        private void AdjustCellSize()
        {
            if (_gridWidth == 0 || _gridHeight == 0)
                return;

            // 가로 세로 비율 유지
            float aspectRatio = (float)_gridWidth / _gridHeight;

            // View 크기 기반으로 셀 크기 계산
            int availableWidth = ClientSize.Width;
            int availableHeight = ClientSize.Height;

            // 가로 세로 비율에 따라 셀 크기 조정
            if (availableWidth / (float)availableHeight > aspectRatio)
            {
                _cellSize = availableHeight / _gridHeight;
            }
            else
            {
                _cellSize = availableWidth / _gridWidth;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (_items == null || _items.Count == 0)
                return;

            var graphics = e.Graphics;
            graphics.Clear(Color.Black); // 배경색 설정

            foreach (var item in _items)
            {
                // 셀 색상 설정
                var cellColor = item.IsProcessed ? Color.LimeGreen : Color.Gray;

                // 셀 위치 계산
                var rect = new Rectangle(item.X * _cellSize, item.Y * _cellSize, _cellSize, _cellSize);

                using (var brush = new SolidBrush(cellColor))
                {
                    graphics.FillRectangle(brush, rect);
                }

                // 셀 외곽선 그리기
                using (var pen = new Pen(Color.Black))
                {
                    graphics.DrawRectangle(pen, rect);
                }
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            AdjustCellSize(); // 크기 변경 시 셀 크기 조정
            Invalidate(); // 다시 그리기
        }
    }
}