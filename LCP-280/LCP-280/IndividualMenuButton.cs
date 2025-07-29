using System;
using System.Drawing;
using System.Windows.Forms;

namespace SP_GridTypeView
{
    public partial class IndividualMenuButton : Button
    {
        protected Image m_imgOnImage;
        protected Image m_imgOffImage;
        private bool m_bButtonState;

        public Size ImageSize { get; set; }

        // 사용자 지정 글씨색/배경색 속성 추가
        private Color _customForeColor = Color.Black;
        public Color CustomForeColor
        {
            get => _customForeColor;
            set
            {
                _customForeColor = value;
                this.ForeColor = value;
                Invalidate();
            }
        }

        private Color _customBackColor = Color.FromArgb(217, 217, 217);
        public Color CustomBackColor
        {
            get => _customBackColor;
            set
            {
                _customBackColor = value;
                this.BackColor = value;
                Invalidate();
            }
        }

        // 사용자 지정 폰트 속성 추가
        private Font _customFont = new Font("Arial", 10, FontStyle.Bold);
        public Font CustomFont
        {
            get => _customFont;
            set
            {
                _customFont = value;
                this.Font = value;
                Invalidate();
            }
        }

        public IndividualMenuButton()
        {
            this.BackgroundImageLayout = ImageLayout.Center;
            this.ImageSize = new Size(45, 45);
            this.TabStop = false;
            // 기본 상태 적용
            this.Font = _customFont;
            this.ForeColor = _customForeColor;
            this.BackColor = _customBackColor;
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
                this.ForeColor = _customForeColor;
                this.BackColor = _customBackColor;
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
