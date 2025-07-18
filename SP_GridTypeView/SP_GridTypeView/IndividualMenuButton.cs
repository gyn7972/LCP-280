using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SP_GridTypeView
{
    public partial class IndividualMenuButton : Button
    {
        protected Image m_imgOnImage;
        protected Image m_imgOffImage;
        private bool m_bButtonState;

        public Size ImageSize { set; get; }
        public IndividualMenuButton()
        {
            this.BackgroundImageLayout = ImageLayout.Center;
            this.Font = new Font("Arial", 10, FontStyle.Bold);
            this.ImageSize = new Size(45, 45);
            this.TabStop = false;
        }
        public void SetImage(Image onImage, Image offImage)
        {
            this.m_imgOnImage = resizeImage(onImage, ImageSize);
            this.m_imgOffImage = resizeImage(offImage, ImageSize);
        }

        public void SetButtonState(bool bOn)
        {
            if (bOn)
            {
                this.Image = m_imgOnImage;
                this.ForeColor = Color.White;
                this.BackColor = Color.FromArgb(127, 127, 127);
            }
            else
            {
                this.Image = m_imgOffImage;
                this.ForeColor = Color.Black;
                this.BackColor = Color.FromArgb(217, 217, 217);
            }
            this.m_bButtonState = bOn;
        }

        public bool GetButtonState()
        {
            return m_bButtonState;
        }

        public Image resizeImage(Image image, Size size)
        {
            return (Image)new Bitmap(image, size);
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            base.OnPaint(pevent);

            // 검은색 테두리 그리기
            using (Pen pen = new Pen(Color.Black, 1))
            {
                Rectangle rect = this.ClientRectangle;
                rect.Width -= 1;
                rect.Height -= 1;
                pevent.Graphics.DrawRectangle(pen, rect);
            }
        }
    }
}
