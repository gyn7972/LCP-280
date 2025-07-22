using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SP_GridTypeView
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

        private int borderWidth = 1;
        [Category("Appearance")]
        public int BorderWidth
        {
            get => borderWidth;
            set { borderWidth = value; Invalidate(); }
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
            base.OnPaint(e);
            using (Pen pen = new Pen(borderColor, borderWidth))
            {
                Rectangle rect = this.ClientRectangle;
                rect.Width -= borderWidth;
                rect.Height -= borderWidth;
                e.Graphics.DrawRectangle(pen, rect);
            }
        }
    }
}