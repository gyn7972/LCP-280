using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace QMC.Common.CustomControl
{
    public class CustomBorderLabel : Label
    {
        private Color borderColor;
        [Category("Appearance")]
        public Color BorderColor
        {
            get => borderColor;
            set { borderColor = value; Invalidate(); }
        }

        private int _labelSize = 8;
        private int borderWidth = 1;
        [Category("Appearance")]
        public int BorderWidth
        {
            get => borderWidth;
            set { borderWidth = value; Invalidate(); }
        }

        public CustomBorderLabel()
        {
            TextAlign = ContentAlignment.MiddleCenter;
            AutoSize = false;
            Font = new Font("Arial", _labelSize, FontStyle.Bold);
            BorderColor = Color.FromArgb(208, 206, 206);
        }

        /// <summary>
        /// 라벨의 크기를 직접 설정합니다.
        /// </summary>
        public void SetLabelSize(int width, int height)
        {
            this.AutoSize = false;
            this.Size = new Size(width, height);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // 배경
            using (var back = new SolidBrush(this.BackColor))
                e.Graphics.FillRectangle(back, this.ClientRectangle);

            // 텍스트가 있을 때만 그리기
            if (!string.IsNullOrEmpty(this.Text))
            {
                // 텍스트 영역 (보더 두께만큼 안쪽)
                var textRect = Rectangle.Inflate(this.ClientRectangle, -borderWidth, -borderWidth);
                
                // StringFormat으로 정확한 중앙 정렬
                using (var format = new StringFormat())
                {
                    format.Alignment = StringAlignment.Center;        // 수평 중앙
                    format.LineAlignment = StringAlignment.Center;    // 수직 중앙
                    format.Trimming = StringTrimming.EllipsisCharacter;
                    
                    using (var brush = new SolidBrush(this.ForeColor))
                    {
                        e.Graphics.DrawString(this.Text, this.Font, brush, textRect, format);
                    }
                }
            }

            // 보더
            using (Pen pen = new Pen(borderColor, borderWidth))
            {
                Rectangle b = this.ClientRectangle;
                b.Width -= borderWidth;
                b.Height -= borderWidth;
                e.Graphics.DrawRectangle(pen, b);
            }
        }
    }
}