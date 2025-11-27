/*
 * Purpose
 *      QMC 회사만의 Vision Image Class에 대해서 정의한다.
 *      
 * Remark
 *      Vision Library 제공하는 대부분의 이미지포맷을 대응하는 구조로 설계한다.
 *      
 * Reference
 *      
 * Revision
 *      1. Created: 2017.09.18 LIM.WT
 *      2. Modified: 2017.11.15 LEE.SH
 * 
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using System.Runtime.InteropServices;
using System.Collections.ObjectModel;
using System.Drawing.Imaging;

namespace QMC.Common.Vision
{
    [IncomparableAttribute()]
    [Serializable]
    public class VisionImage : IDisposable
    {
        #region Define
        public enum FileFilter
        {
            bmp,
            vif,
            jpg,
            tif,
        }

        public enum RotateDirection
        {
            Left,
            Right,
        }

        private delegate int SaveAsyncDelegate(string path, FileFilter filter);
        private delegate int LoadAsyncDelegate(string path, FileFilter filter);
        #endregion

        #region Field
        [NonSerialized]
        private bool m_Disposed;
        private VisionImageHeader m_Header;
        private byte[] m_RawData;
        private Bitmap m_bitmap;
        [NonSerialized]
        private ICustomizedVisionImage m_CustomizedData;
        [NonSerialized]
        //private MethodCaller m_Operational;
        private object m_Tag;
        #endregion

        #region Constructor
        public VisionImage()
        {
            this.Disposed = false;
            this.RawData = null;
            this.CustomizedData = null;
            this.Header = new VisionImageHeader();
            //this.Operational = new MethodCaller();
        }
        #endregion

        #region Finalizer
        ~VisionImage()
        {
            this.Dispose(false);
        }
        #endregion

        #region Property
        /// <summary>
        /// Dispose의 여부를 가져온다.
        /// </summary>
        public bool Disposed
        {
            get { return this.m_Disposed; }
            private set { this.m_Disposed = value; }
        }

        /// <summary>
        /// Image의 Header 정보를 가져오거나 설정한다.
        /// </summary>
        public VisionImageHeader Header
        {
            get { return this.m_Header; }
            set { this.m_Header = value; }
        }

        /// <summary>
        /// Image의 RawData 정보를 가져오거나 설정한다.
        /// </summary>
        public byte[] RawData
        {
            get { return this.m_RawData; }
            set { this.m_RawData = value; }
        }

        /// <summary>
        /// VisionImage의 Interface를 제공한다.
        /// </summary>
        public ICustomizedVisionImage CustomizedData
        {
            get { return this.m_CustomizedData; }
            set { this.m_CustomizedData = value; }
        }

        //protected MethodCaller Operational
        //{
        //    get { return this.m_Operational; }
        //    private set { this.m_Operational = value; }
        //}

        public object Tag
        {
            get { return this.m_Tag; }
            set { this.m_Tag = value; }
        }
        #endregion

        #region Method
        private string GetPath(FileFilter filter)
        {
            string path = string.Empty;

            path = string.Format("{0}\\{1} {2}\\", ConfigManager.GetConfigPath(), this.Header.OwnerName, System.DateTime.Now.ToString("yyyy-MM-dd"), filter.ToString());

            path += string.Format("{0}.{1}", System.DateTime.Now.ToString("hh_mm_ss_ffffff"), filter.ToString());

            return path;
        }

        #region GetImage()
        public Image GetImage()
        {

            //if(m_bitmap!=null)
            //{
            //    return m_bitmap;
            //}
            Bitmap bmp = null;
            BitmapData bmpData;
            IntPtr point = IntPtr.Zero;
            if (this.Header.Width == 0 && this.Header.Height == 0)
            {
                //Here create the Bitmap to the know height, width and format
                bmp = m_bitmap = new Bitmap(1, 1);
                return bmp;
            }
            else
            {
                //Here create the Bitmap to the know height, width and format
                bmp = m_bitmap = new Bitmap(this.Header.Width, this.Header.Height, this.Header.PixelFormat);
            }
            //Create a BitmapData and Lock all pixels to be written 
            if (this.RawData == null)
            {
                // 원본 데이터가 없으면 의미있는 이미지가 아님
                return null;
            }
            bmpData = bmp.LockBits(
                        new Rectangle(0, 0, bmp.Width, bmp.Height),
                        ImageLockMode.ReadWrite, this.Header.PixelFormat);

            //Copy the data from the byte array into BitmapData.Scan0
            Marshal.Copy(this.RawData, 0, bmpData.Scan0, RawData.Length);
            //Unlock the pixels
            bmp.UnlockBits(bmpData);

            ColorPalette GrayscalePalette = bmp.Palette;

            if (GrayscalePalette.Entries.Length == 0) return bmp;
            for (int i = 0; i < 255; i++)
            {
                GrayscalePalette.Entries[i] = Color.FromArgb(i, i, i);
            }

            bmp.Palette = GrayscalePalette;

            return bmp;
        }
        #endregion

        #region CutImage()
        public Image CutImage(Point startPosition, Point endPosition)
        {
            return this.CutImage(new Rectangle(startPosition.X, startPosition.Y, endPosition.X - startPosition.X, endPosition.Y - startPosition.Y));
        }

        public Image CutImage(Point centerPosition, Size size)
        {
            return this.CutImage(new Rectangle(new Point(centerPosition.X - size.Width / 2, centerPosition.Y - size.Height / 2), size));
        }

        public Image CutImage(Rectangle rectangle)
        {
            // 방어 코드: 이미지가 유효하지 않은 경우 null 반환
            if (this.Header == null || this.RawData == null || this.RawData.Length == 0 || this.Header.Width <= 0 || this.Header.Height <= 0)
            {
                return null;
            }

            int x = rectangle.X;
            int y = rectangle.Y;
            int w = rectangle.Width;
            int h = rectangle.Height;
            if (x < 0)
            {
                x = 0;
            }
            if (y < 0)
            {
                y = 0;
            }
            if (w > Header.Width)
            {
                w = Header.Width;
            }
            if (h > Header.Height)
            {
                h = Header.Height;
            }

            // ROI 가 이미지 범위 밖인 경우
            if (w <= 0 || h <= 0)
            {
                return null;
            }

            Bitmap bmp = null;
            BitmapData bmpData;

            IntPtr point = IntPtr.Zero;

            //Here create the Bitmap to the know height, width and format           
            //예외처리
            if (w != 0 && h != 0)
            {
                m_bitmap = bmp = new Bitmap(w, h, this.Header.PixelFormat);

                //Create a BitmapData and Lock all pixels to be written 
                bmpData = bmp.LockBits(
                            new Rectangle(0, 0, w, h),
                            ImageLockMode.ReadWrite, this.Header.PixelFormat);

                //Copy the data from the byte array into BitmapData.Scan0
                IntPtr p = bmpData.Scan0;
                int PixcelPerByte = bmpData.Stride / bmpData.Width;
                if (y + h > this.Header.Height)
                {
                    h += this.Header.Height - h - y - 1;
                }
                for (int iter = 0; iter < h; iter++)
                {
                    Marshal.Copy(RawData, (iter + y) * this.Header.Stride + x * PixcelPerByte
                        , p + (iter) * bmpData.Stride, bmpData.Stride);
                }

                //Unlock the pixels
                bmp.UnlockBits(bmpData);

                ColorPalette GrayscalePalette = bmp.Palette;

                if (GrayscalePalette.Entries.Length == 0) return bmp;
                for (int i = 0; i < 255; i++)
                {
                    GrayscalePalette.Entries[i] = Color.FromArgb(i, i, i);
                }

                bmp.Palette = GrayscalePalette;
            }

            return bmp;

        }
        public Image CutImageRaw(Rectangle rectangle)
        {
            Bitmap bitmap = null;
            Bitmap cutImage = null;
            bitmap = new Bitmap(rectangle.Width, rectangle.Height);

            cutImage = bitmap.Clone(rectangle, bitmap.PixelFormat);

            return cutImage;
        }
        public VisionImage CutVisionImage(Point startPosition, Point endPosition)
        {
            Bitmap cutImage = null;
            VisionImage visionImage = null;

            cutImage = (Bitmap)this.CutImage(startPosition, endPosition);
            if (cutImage == null) return null; // 방어

            visionImage = VisionImage.CreateInstance(cutImage);
            cutImage.Dispose();
            return visionImage;
        }

        public VisionImage CutVisionImage(Point centerPosition, Size size)
        {
            Bitmap cutImage = null;
            VisionImage visionImage = null;

            cutImage = (Bitmap)this.CutImage(centerPosition, size);
            if (cutImage == null) return null; // 방어

            visionImage = VisionImage.CreateInstance(cutImage);
            cutImage.Dispose();
            return visionImage;
        }

        public VisionImage CutVisionImage(Rectangle rectangle)
        {
            Bitmap cutImage = null;
            VisionImage visionImage = null;

            cutImage = (Bitmap)this.CutImage(rectangle);
            if (cutImage == null) return null; // 방어

            visionImage = VisionImage.CreateInstance(cutImage);
            cutImage.Dispose();
            return visionImage;
        }
        #endregion

        #region RotateImage()
        public Image RotateImage(double angle, PointD point)
        {
            Bitmap rotateImage = null;

            rotateImage = (Bitmap)this.CustomizedData.GetRotateImage(angle, point);

            return rotateImage;
        }
        public Image RotateImage(double angle)
        {
            Bitmap rotateImage = null;

            rotateImage = (Bitmap)this.RotateImage(angle, new PointD(this.Header.Width / 2, this.Header.Height / 2));

            return rotateImage;
        }
        public VisionImage RotateVisionImage(double angle, PointD point)
        {
            VisionImage rotateImage = null;

            rotateImage = this.CustomizedData.GetRotateVisionImage(angle, point);

            return rotateImage;
        }
        public VisionImage RotateVisionImage(double angle)
        {
            VisionImage rotateImage = null;

            rotateImage = this.RotateVisionImage(angle, new PointD(this.Header.Width / 2, this.Header.Height / 2));

            return rotateImage;
        }
        #endregion

        #region Save()

        //public MethodCallerAsyncResult BeginSave(string path, FileFilter filter, MethodCallerAsyncCallback callback, object value)
        //{
        //    return this.Operational.BeginInvoke(new SaveAsyncDelegate(this.SaveProcedure), new object[] { path, filter }, callback, value);
        //}

        //public MethodCallerAsyncResult BeginSave(FileFilter filter, MethodCallerAsyncCallback callback, object value)
        //{
        //    return this.BeginSave(this.GetPath(filter), filter, null, null);
        //}

        //public MethodCallerAsyncResult BeginSave(string path, FileFilter filter)
        //{
        //    return this.BeginSave(path, filter, null, null);
        //}

        //public MethodCallerAsyncResult BeginSave(FileFilter filter)
        //{
        //    return this.BeginSave(filter, null, null);
        //}

        //public int EndSave(MethodCallerAsyncResult ar)
        //{
        //    return (int)this.Operational.EndInvoke(ar);
        //}

        //public int Save(string path, FileFilter filter)
        //{
        //    MethodCallerAsyncResult ar = this.BeginSave(path, filter, null, null);
        //    return this.EndSave(ar);
        //}

        //public int Save(FileFilter filter)
        //{
        //    MethodCallerAsyncResult ar = this.BeginSave(this.GetPath(filter), filter, null, null);
        //    return this.EndSave(ar);
        //}

        //public int SaveSync(string path, FileFilter filter)
        //{
        //    return this.SaveProcedure(path, filter);
        //}

        //public int SaveSync(FileFilter filter)
        //{
        //    return this.SaveProcedure(this.GetPath(filter), filter);
        //}
        public void GetSumData(ref int[] Sum, int Count)
        {
            if (Count == m_RawData.Length)
            {
                for (int iter = 0; iter < Count; iter++)
                {
                    Sum[iter] += m_RawData[iter];
                }
            }
        }

        public void SetSumData(int[] Sum, int Count, int SumCount)
        {
            if (Count == m_RawData.Length)
            {
                for (int iter = 0; iter < Count; iter++)
                {
                    m_RawData[iter] = (byte)(Sum[iter] / SumCount);
                }
            }
        }
        public int Save(string path, FileFilter filter)
        {
            return this.SaveProcedure(path, filter);
        }

        public int Save(FileFilter filter)
        {
            return this.SaveProcedure(this.GetPath(filter), filter);
        }

        private int SaveProcedure(string path, FileFilter filter)
        {
            int ret = 0;
            if ((ret = this.OnSave(path, filter)) != 0) return ret;
            return ret;
        }

        protected virtual int OnSave(string path, FileFilter filter)
        {
            int ret = 0;
            byte[] bytes = new byte[0];
            string directory = "";
            FileStream stream = null;

            //if (this.Header == null || this.RawData == null) return ErrorManager.Register("VisionImage Data Empty");
            if (this.Header == null || this.RawData == null) return -1;

            directory = Path.GetDirectoryName(path);

            if (Directory.Exists(directory) == false)
                Directory.CreateDirectory(directory);

            try
            {
                stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                if (filter == FileFilter.bmp)
                {
                    Bitmap image = (Bitmap)this.GetImage();
                    if (image != null)
                        image.Save(stream, ImageFormat.Bmp);
                }
                else if (filter == FileFilter.vif)
                {
                    SaveManager.BinarySerialize(ref bytes, this);
                    File.WriteAllBytes(path, bytes);
                }
                else if (filter == FileFilter.jpg)
                {
                    Bitmap image = (Bitmap)this.GetImage();
                    if (image != null)
                        image.Save(stream, ImageFormat.Jpeg);
                }
                else if (filter == FileFilter.tif)
                {
                    Bitmap image = (Bitmap)this.GetImage();
                    if (image != null)
                        image.Save(stream, ImageFormat.Tiff);
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();

                }
            }
            return ret;
        }
        #endregion

        #region Load()
        //public MethodCallerAsyncResult BeginLoad(string path, FileFilter filter, MethodCallerAsyncCallback callback, object value)
        //{
        //    return this.Operational.BeginInvoke(new LoadAsyncDelegate(this.LoadProcedure), new object[] { path, filter }, callback, value);
        //}
        //public MethodCallerAsyncResult BeginLoad(string path, FileFilter filter)
        //{
        //    return this.BeginLoad(path, filter, null, null);
        //}

        //public int EndLoad(MethodCallerAsyncResult ar)
        //{
        //    return (int)this.Operational.EndInvoke(ar);
        //}

        //public int Load(string path, FileFilter filter)
        //{
        //    MethodCallerAsyncResult ar = this.BeginLoad(path, filter, null, null);
        //    return this.EndLoad(ar);
        //}

        //public int LoadSync(string path, FileFilter filter)
        //{
        //    return this.LoadProcedure(path, filter);
        //}

        public int Load(string path, FileFilter filter)
        {
            this.LoadProcedure(path, filter);

            return 1;
        }

        private int LoadProcedure(string path, FileFilter filter)
        {
            int ret = 0;
            if ((ret = this.OnLoad(path, filter)) != 0) return ret;
            return ret;
        }

        protected virtual int OnLoad(string path, FileFilter filter)
        {
            int ret = 0;
            byte[] bytes = null;
            VisionImage image = null;
            FileStream stream = null;

            try
            {
                if (File.Exists(path) == false) return 1;

                if (filter == FileFilter.bmp || filter == FileFilter.tif)
                {
                    stream = new FileStream(path, FileMode.Open, FileAccess.Read);
                    Bitmap bitmap = new Bitmap(stream);
                    BitmapData data;
                    float stride;

                    data = bitmap.LockBits(
                        new Rectangle(new Point(0, 0), new Size(bitmap.Width, bitmap.Height)),
                        ImageLockMode.ReadOnly, bitmap.PixelFormat);
                    stride = data.Stride;

                    this.Header.Width = data.Width;
                    this.Header.Height = data.Height;
                    this.Header.PixelFormat = data.PixelFormat;
                    this.Header.Stride = data.Stride;
                    this.Header.BufferSize = data.Stride * data.Height;
                    this.Header.BitsPerPixel = (int)Math.Round((8 * stride - 7) / data.Width);

                    if (this.RawData == null)
                    {
                        this.RawData = new byte[data.Stride * data.Height];
                    }
                    else if (this.RawData.Length != this.Header.BufferSize)
                    {
                        this.RawData = null;
                        this.RawData = new byte[data.Stride * data.Height];
                    }

                    Marshal.Copy(data.Scan0, this.RawData, 0, this.Header.BufferSize);

                    bitmap.UnlockBits(data);
                    stream.Close();
                }
                else if (filter == FileFilter.vif)
                {
                    bytes = File.ReadAllBytes(path);

                    if ((ret = SaveManager.BinaryDeserialize<VisionImage>(bytes, out image)) != 0)
                        return -1;//return ErrorManager.Register("VisionImage format error");

                    this.Header = image.Header;
                    this.RawData = image.RawData;
                }
                else if (filter == FileFilter.jpg)
                {
                    throw new Exception("jpg is not supported");
                }
            }
            catch (Exception ex)
            {
                //Log.Write("VisionImage", ex.Message);
                Log.Write(ex);
            }

            return ret;
        }
        #endregion

        #region CreateInstance()
        public static VisionImage CreateInstance(Image image)
        {
            VisionImage visionImage = new VisionImage();

            #region Bitmap
            if (image is Bitmap)
            {
                Bitmap bitmap = image as Bitmap;
                BitmapData data;
                float stride;

                data = bitmap.LockBits(
                    new Rectangle(new Point(0, 0), new Size(image.Width, image.Height)),
                    ImageLockMode.ReadOnly, image.PixelFormat);
                stride = data.Stride;

                visionImage.Header.Width = image.Width;
                visionImage.Header.Height = image.Height;
                visionImage.Header.PixelFormat = image.PixelFormat;
                visionImage.Header.Stride = data.Stride;
                visionImage.Header.BufferSize = data.Stride * image.Height;
                visionImage.Header.BitsPerPixel = (int)Math.Round((8 * stride - 7) / image.Width);

                if (visionImage.RawData == null)
                {
                    visionImage.RawData = new byte[data.Stride * data.Height];
                }
                else if (visionImage.RawData.Length != visionImage.Header.BufferSize)
                {
                    visionImage.RawData = null;
                    visionImage.RawData = new byte[data.Stride * data.Height];
                }

                Marshal.Copy(data.Scan0, visionImage.RawData, 0, visionImage.Header.BufferSize);

                bitmap.UnlockBits(data);
            }
            #endregion

            return visionImage;
        }
        public static VisionImage CreateInstance(VisionImage image, Rectangle rect)
        {
            VisionImage visionImage = new VisionImage();
            int bytePerPixel = 0;
            IntPtr targetImage;
            IntPtr sourceImage;

            #region Bitmap
            if (image is VisionImage)
            {
                visionImage.Header.Width = rect.Width;
                visionImage.Header.Height = rect.Height;
                visionImage.Header.PixelFormat = image.Header.PixelFormat;
                visionImage.Header.BufferSize = rect.Width * rect.Height;
                visionImage.Header.OwnerName = image.Header.OwnerName;
                visionImage.Header.BitsPerPixel = image.Header.BitsPerPixel;

                bytePerPixel = image.Header.Stride / image.Header.Width;
                visionImage.Header.Stride = image.Header.Stride - ((image.Header.Width - rect.Width) * bytePerPixel);
                visionImage.Header.BufferSize = visionImage.Header.Stride * visionImage.Header.Height;

                if (visionImage.RawData == null)
                {
                    visionImage.RawData = new byte[visionImage.Header.Stride * visionImage.Header.Height];
                }
                else if (visionImage.RawData.Length != visionImage.Header.BufferSize)
                {
                    visionImage.RawData = null;
                    visionImage.RawData = new byte[visionImage.Header.Stride * visionImage.Header.Height];
                }

                targetImage = Marshal.UnsafeAddrOfPinnedArrayElement(visionImage.RawData, 0);
                sourceImage = Marshal.UnsafeAddrOfPinnedArrayElement(image.RawData, 0);
                for (int iter = 0; iter < rect.Height; iter++)
                {
                    Marshal.Copy(image.RawData
                        , (iter + rect.Y) * (image.Header.Stride) + rect.X * bytePerPixel
                        , targetImage + (iter * visionImage.Header.Stride), visionImage.Header.Stride);
                }
            }
            #endregion

            return visionImage;
        }
        #endregion

        #region GetPixel
        public int GetPixel(Point point)
        {
            int count = this.Header.Width * (point.Y - 1) + (point.X - 1);

            if (this.RawData == null)
                return 0;
            else if (this.RawData.Length <= count || count < 0)
                return 0;
            else
                return this.RawData[count];
        }
        #endregion
        #endregion

        #region IDisposable Members
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.Disposed == true) return;
            if (disposing == true)
            {
                if (this.CustomizedData != null)
                {
                    this.CustomizedData.Dispose();
                }
            }
            this.Disposed = true;
        }
        #endregion

        #region Object Members
        public override bool Equals(object obj)
        {
            return this == obj as VisionImage;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion

        #region operator
        public static implicit operator VisionImage(Image image)
        {
            return VisionImage.CreateInstance(image);
        }

        public static bool operator ==(VisionImage previous, VisionImage next)
        {
            if (VisionImage.ReferenceEquals(previous, null) == false && VisionImage.ReferenceEquals(next, null) == false)
            {
                if (previous.RawData != next.RawData) return false;

                if (previous.Header != null && next.Header != null)
                {
                    if (previous.Header.Width != next.Header.Width) return false;
                    if (previous.Header.Height != next.Header.Height) return false;
                    if (previous.Header.BitsPerPixel != next.Header.BitsPerPixel) return false;
                    if (previous.Header.BufferSize != next.Header.BufferSize) return false;
                    if (previous.Header.PixelFormat != next.Header.PixelFormat) return false;
                    if (previous.Header.Stride != next.Header.Stride) return false;
                }

                if (previous.CustomizedData != null && next.CustomizedData != null)
                {
                    if (previous.CustomizedData.Image != next.CustomizedData.Image) return false;
                }
            }
            else if (VisionImage.ReferenceEquals(previous, null) == true && VisionImage.ReferenceEquals(next, null) == true) return true;
            else
                return false;

            return true;
        }

        public static bool operator !=(VisionImage previous, VisionImage next)
        {
            return !(previous == next);
        }
        #endregion
    }

    [Serializable]
    public class VisionImageCollection : Collection<VisionImage>
    {
        #region Constructor
        public VisionImageCollection()
        {
        }
        #endregion
    }

    [Serializable]
    public struct VisionImageInformation
    {
        #region Field

        private string m_Name;
        private VisionImage m_Image;
        #endregion

        #region Constructor
        public VisionImageInformation(string name, VisionImage image)
        {
            this.m_Name = name;
            this.m_Image = image;
        }

        public VisionImageInformation(string name) : this(name, null) { }
        #endregion

        #region Property
        public string Name
        {
            get { return this.m_Name; }
            set { this.m_Name = value; }
        }
        public VisionImage Image
        {
            get { return this.m_Image; }
            set { this.m_Image = value; }
        }
        #endregion

        #region Static
        public static VisionImageInformation Parse(string text)
        {
            return new VisionImageInformation(text, null);
        }
        #endregion

        #region Object Members
        public override string ToString()
        {
            return this.Name;
        }
        #endregion
    }

    [Serializable]
    public class VisionImageInformationCollection : List<VisionImageInformation> { }
}