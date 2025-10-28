/*
 * Purpose
 *      QMC 회사만의 Vision Image Interface에 대해서 정의한다.
 *      
 * Remark
 *      Vision Library 제공하는 대부분의 이미지포맷을 대응하는 구조로 설계한다.
 *      
 * Reference
 *      
 * Revision
 *      1. Created: 2017.09.18 LIM.WT
 *      2. Modified: 2017.11.15 LEE.SH
 *      3. Modified: 2018.08.22 Jung.CY
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace QMC.Common.Vision
{
    #region ICustomizedVisionImage
    public interface ICustomizedVisionImage : IDisposable
    {
        /// <summary>
        /// 3rd Party Vision Library에서 제공하는 포맷의 image를 가져오거나 설정한다.
        /// </summary>
        object Image { get; set; }

        /// <summary>
        /// Customized된 이미지가 해제되었는지의 여부를 가져오거나 설정한다.
        /// </summary>
        bool Disposed { get; }

        /// <summary>
        /// Customized된 이미지에서 System.Drawing.Image 형식의 이미지를 반환한다.
        /// </summary>
        /// <returns></returns>
        Image GetImage();

        void SetImage(Image image);

        VisionImage GetVisionImage();

        VisionImage GetRotateVisionImage(double angle, PointD point);

        Image GetRotateImage(double angle, PointD point);

        void Save(string fileName);

        int Load(string fileName);
    }
    #endregion

    #region DefaultVisionImage
    public class DefaultVisionImage : ICustomizedVisionImage
    {
        #region Define
        #endregion

        #region Field
        private bool m_Disposed;
        private Image m_Image;
        //private FrameVisionImageOverlay m_Region;
        #endregion

        #region Constructor
        public DefaultVisionImage()
        {
        }
        #endregion

        #region Finalizer
        ~DefaultVisionImage()
        {
            this.Dispose();
        }
        #endregion

        #region Property
        //public FrameVisionImageOverlay Region
        //{
        //    get { return this.m_Region; }
        //    set { this.m_Region = value; }
        //}
        #endregion

        #region Method
        public static int Create(ref VisionImage source)
        {
            int ret = 0;
            DefaultVisionImage customized = null;
            Image image = null;

            if (source.RawData == null || source.Header.Width == 0 || source.Header.Height == 0)
            {
                throw new Exception("Vision image is null.");
            }

            customized = new DefaultVisionImage();
            image = CopyUtility.GetDeepCopy(source.GetImage()) as Image;

            source.CustomizedData = customized;
            source.CustomizedData.Image = image;

            return ret;
        }
        #endregion

        #region ICustomizedVisionImage Method
        public object Image
        {
            get { return this.m_Image; }
            set { this.m_Image = value as Image; }
        }

        public bool Disposed
        {
            get { return this.m_Disposed; }
            private set { this.m_Disposed = value; }
        }

        public Image GetImage()
        {
            return this.Image as Image;
        }

        public void SetImage(Image image)
        {
            this.Image = image;
        }

        public Image GetRotateImage(double angle, PointD point)
        {
            Bitmap bitmap = null;
            Bitmap rotateImage = null;
            bitmap = (Bitmap)this.GetImage();

            rotateImage = new Bitmap(bitmap.Width, bitmap.Height);
            rotateImage.SetResolution(bitmap.HorizontalResolution, bitmap.VerticalResolution);

            Graphics g = Graphics.FromImage(rotateImage);
            g.TranslateTransform((float)point.X, (float)point.Y);
            g.RotateTransform((float)angle);
            g.TranslateTransform((float)-point.X, (float)-point.Y);
            g.DrawImage(bitmap, new PointF(0, 0));

            return rotateImage;
        }

        public VisionImage GetRotateVisionImage(double angle, PointD point)
        {
            Bitmap rotateImage = null;
            VisionImage visionImage = null;

            rotateImage = (Bitmap)this.GetRotateImage(angle, point);

            visionImage = VisionImage.CreateInstance(rotateImage);

            return visionImage;
        }

        public VisionImage GetVisionImage()
        {
            return this.Image as Image;
        }

        public int Load(string fileName)
        {
            int ret = 0;

            this.Image = System.Drawing.Image.FromFile(fileName);
            
            return ret;
        }

        public void Save(string fileName)
        {
            Image image = this.Image as Image;

            image.Save(fileName);
        }
        #endregion

        #region IDisposable Members
        public void Dispose()
        {
            ((Image)this.Image).Dispose();
            this.Disposed = true;
        }
        #endregion
    }
    #endregion
}