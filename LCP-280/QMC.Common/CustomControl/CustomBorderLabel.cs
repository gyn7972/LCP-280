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
            //Dock = DockStyle.Fill;
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