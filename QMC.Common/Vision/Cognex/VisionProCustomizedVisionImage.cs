/*
 * Purpose
 *      Congex에서 사용하는 이미지 포멧에 대응하는 Customized Vision Image Class에 대해서 정의한다.
 * 
 * Revision
 *      1. Created: 2018.01.09 by JUNG.CY
 *      
 */

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;

using Cognex.VisionPro;
using Cognex.VisionPro.ImageProcessing;
using Cognex.VisionPro.ImageFile;

namespace QMC.Common.Vision.Cognex
{
    #region VisionProCustomizedVisionImage
    public class VisionProCustomizedVisionImage : ICustomizedVisionImage
    {
        #region Define
        #endregion

        #region Field
        private bool m_Disposed;
        private object m_Image;
        private ICogRegion m_Region;
        #endregion

        #region Constructor
        public VisionProCustomizedVisionImage()
        {
        }
        #endregion

        #region Finalizer
        ~VisionProCustomizedVisionImage()
        {
            this.Dispose();
        }
        #endregion

        #region Property
        public ICogRegion Region
        {
            get { return this.m_Region; }
            set { this.m_Region = value; }
        }
        #endregion

        #region Method
        public static int Create(ref VisionImage source)
        {
            int ret = 0;
            VisionProCustomizedVisionImage customized = null;
            ICogImage cognexImage = null;

            if (source.RawData == null || source.Header.Width == 0 || source.Header.Height == 0)
            {
                throw new Exception("Vision image is null.");
            }

            switch (source.Header.PixelFormat)
            {
                case PixelFormat.Format1bppIndexed:
                    throw new NotImplementedException();

                case PixelFormat.Format4bppIndexed:
                    throw new NotImplementedException();

                case PixelFormat.Format8bppIndexed:
                    //if (source.Header.Pointer == IntPtr.Zero)
                    cognexImage = new CogImage8Grey((Bitmap)source.GetImage());
                    //else
                    //    cognexImage = new CogImage8Grey(source.Header.Pointer);

                    customized = new VisionProCustomizedVisionImage();

                    source.CustomizedData = customized;
                    source.CustomizedData.Image = cognexImage;
                    break;

                case PixelFormat.Format16bppGrayScale:
                    //if (source.Header.Pointer == IntPtr.Zero)
                    cognexImage = new CogImage16Grey((Bitmap)source.GetImage());
                    //else
                    //    cognexImage = new CogImage16Grey(source.Header.Pointer);

                    customized = new VisionProCustomizedVisionImage();
                    source.CustomizedData = customized;
                    source.CustomizedData.Image = cognexImage;
                    break;

                case PixelFormat.Format16bppRgb555:
                    break;

                case PixelFormat.Format16bppRgb565:
                    break;

                case PixelFormat.Format16bppArgb1555:
                    break;

                case PixelFormat.Format24bppRgb:
                    //if (source.Header.Pointer == IntPtr.Zero)
                    cognexImage = new CogImage8Grey((Bitmap)source.GetImage());
                    //else
                    //    cognexImage = new CogImage8Grey(source.Header.Pointer);

                    customized = new VisionProCustomizedVisionImage();
                    source.CustomizedData = customized;

                    source.CustomizedData.Image = cognexImage;
                    break;

                case PixelFormat.Format32bppRgb:
                    //if (source.Header.Pointer == IntPtr.Zero)
                    cognexImage = new CogImage8Grey((Bitmap)source.GetImage());
                    //else
                    //    cognexImage = new CogImage8Grey(source.Header.Pointer);

                    customized = new VisionProCustomizedVisionImage();
                    source.CustomizedData = customized;

                    source.CustomizedData.Image = cognexImage;
                    break;

                case PixelFormat.Format32bppArgb:
                    //if (source.Header.Pointer == IntPtr.Zero)
                    cognexImage = new CogImage8Grey((Bitmap)source.GetImage());
                    //else
                    //    cognexImage = new CogImage8Grey(source.Header.Pointer);

                    customized = new VisionProCustomizedVisionImage();
                    source.CustomizedData = customized;

                    source.CustomizedData.Image = cognexImage;
                    break;

                case PixelFormat.Format32bppPArgb:
                    throw new NotImplementedException();

                case PixelFormat.Format48bppRgb:
                    throw new NotImplementedException();

                case PixelFormat.Format64bppArgb:
                    throw new NotImplementedException();

                default:
                    throw new Exception("Invalid Vision Image");
            }

            return ret;
        }
        #endregion

        #region ICustomizedVisionImage Method
        public object Image
        {
            get { return this.m_Image; }
            set { this.m_Image = value; }
        }

        public bool Disposed
        {
            get { return this.m_Disposed; }
            private set { this.m_Disposed = value; }
        }

        public Image GetImage()
        {
            Bitmap bmp = null;

            if (this.Image == null)
            {
                throw new Exception("Customized image is null.");
            }

            if (this.Image is ICogImage)
            {
                ICogImage image = this.Image as ICogImage;

                bmp = image.ToBitmap();
                return bmp;
            }
            else
            {
                throw new Exception("Invalid VisionPro Image");
            }
        }

        public void SetImage(Image image)
        {
            ICogImage cognexImage = null;

            switch (image.PixelFormat)
            {
                case PixelFormat.Format1bppIndexed:
                    throw new NotImplementedException();

                case PixelFormat.Format4bppIndexed:
                    throw new NotImplementedException();

                case PixelFormat.Format8bppIndexed:
                    Bitmap format8bppIndexedImage = (Bitmap)image;

                    cognexImage = new CogImage8Grey(format8bppIndexedImage);

                    this.Image = cognexImage;
                    break;

                case PixelFormat.Format16bppGrayScale:
                    cognexImage = new CogImage16Grey((Bitmap)image);

                    this.Image = cognexImage;
                    break;

                case PixelFormat.Format16bppRgb555:
                    break;

                case PixelFormat.Format16bppRgb565:
                    break;

                case PixelFormat.Format16bppArgb1555:
                    break;

                case PixelFormat.Format24bppRgb:
                    Bitmap format24bppRgbImage = (Bitmap)image;

                    cognexImage = new CogImage8Grey(format24bppRgbImage);

                    this.Image = cognexImage;
                    break;

                case PixelFormat.Format32bppRgb:
                    Bitmap format32bppRgbImage = (Bitmap)image;

                    cognexImage = new CogImage8Grey(format32bppRgbImage);

                    this.Image = cognexImage;
                    break;

                case PixelFormat.Format32bppArgb:
                    Bitmap format32bppArgbImage = (Bitmap)image;

                    cognexImage = new CogImage8Grey(format32bppArgbImage);

                    this.Image = cognexImage;
                    break;

                case PixelFormat.Format32bppPArgb:
                    throw new NotImplementedException();

                case PixelFormat.Format48bppRgb:
                    throw new NotImplementedException();

                case PixelFormat.Format64bppArgb:
                    throw new NotImplementedException();

                default:
                    throw new Exception("Invalid Vision Image");
            }
        }

        public VisionImage GetVisionImage()
        {
            VisionImage visionImage = new VisionImage();

            if (this.Image is CogImage8Grey)
            {
                CogImage8Grey image = this.Image as CogImage8Grey;
                ICogImage8PixelMemory memory = null;
                float stride;
                
                memory = image.Get8GreyPixelMemory(CogImageDataModeConstants.Read, 0, 0, image.Width, image.Height);
                stride = memory.Stride;

                visionImage.Header.Width = memory.Width;
                visionImage.Header.Height = memory.Height;

                visionImage.Header.PixelFormat = PixelFormat.Format8bppIndexed;
                visionImage.Header.Stride = memory.Stride;
                visionImage.Header.BufferSize = memory.Stride * memory.Height;
                visionImage.Header.BitsPerPixel = (int)Math.Round((8 * stride - 7) / memory.Width);
                
                if (visionImage.RawData == null)
                {
                    visionImage.RawData = new byte[memory.Stride * memory.Height];
                }
                else if (visionImage.RawData.Length != visionImage.Header.BufferSize)
                {
                    visionImage.RawData = null;
                    visionImage.RawData = new byte[memory.Stride * memory.Height];
                }

                Marshal.Copy(memory.Scan0, visionImage.RawData, 0, visionImage.Header.BufferSize);

                return visionImage;
            }

            if (this.Image is ICogImage)
                {
                    ICogImage image = this.Image as ICogImage;
                    Bitmap bitmap = null;
                    BitmapData data = null;

                    bitmap = image.ToBitmap();
                    float stride;

                    data = bitmap.LockBits(
                        new Rectangle(new Point(0, 0), new Size(bitmap.Width, bitmap.Height)),
                        ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
                    stride = data.Stride;

                    visionImage.Header.Width = data.Width;
                    visionImage.Header.Height = data.Height;
                    visionImage.Header.PixelFormat = data.PixelFormat;
                    visionImage.Header.Stride = data.Stride;
                    visionImage.Header.BufferSize = data.Stride * data.Height;
                    visionImage.Header.BitsPerPixel = (int)Math.Round((8 * stride - 7) / data.Width);

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

                    return visionImage;
                }
                else
                {
                    throw new Exception("Invalid VisionPro Image");
                }
            }

        public Image GetRotateImage(double angle, PointD point)
        {
            VisionImage visionImage = new VisionImage();

            if (this.Image is ICogImage)
            {
                CogAffineTransformTool tool = new CogAffineTransformTool();
                CogRectangleAffine region = new CogRectangleAffine();
                ICogImage image = this.Image as ICogImage;

                region.CenterX = point.X;
                region.CenterY = point.Y;
                region.SideXLength = image.Width;
                region.SideYLength = image.Height;
                region.Rotation = angle / 180.0 * Math.PI;

                tool.InputImage = image;
                tool.Region = region;
                tool.Run();

                Bitmap bitmap = null;

                bitmap = tool.OutputImage.ToBitmap();

                return bitmap;
            }

            return null;
        }

        public VisionImage GetRotateVisionImage(double angle, PointD point)
        {
            VisionImage visionImage = new VisionImage();

            if (this.Image is CogImage16Grey)
            {
                throw new NotImplementedException();
            }
            else if (this.Image is ICogImage)
            {
                Bitmap bitmap = null;
                BitmapData data = null;

                bitmap = (Bitmap)this.GetRotateImage(angle, point);
                float stride;

                data = bitmap.LockBits(
                    new Rectangle(new Point(0, 0), new Size(bitmap.Width, bitmap.Height)),
                    ImageLockMode.ReadOnly, bitmap.PixelFormat);
                stride = data.Stride;

                visionImage.Header.Width = data.Width;
                visionImage.Header.Height = data.Height;
                visionImage.Header.PixelFormat = data.PixelFormat;
                visionImage.Header.Stride = data.Stride;
                visionImage.Header.BufferSize = data.Stride * data.Height;
                visionImage.Header.BitsPerPixel = (int)Math.Round((8 * stride - 7) / data.Width);

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

                return visionImage;
            }

            return null;
        }

        public void Save(string fileName)
        {
            string extension = "";
            ICogImageFileProvider provider = null;

            if (fileName == string.Empty) return;

            extension = Path.GetExtension(fileName);
            extension = extension.Split('.')[1];

            if (extension == VisionImage.FileFilter.bmp.ToString())
            {
                provider = new CogImageFileBMP();
            }
            else if (extension == VisionImage.FileFilter.tif.ToString())
            {
                provider = new CogImageFileTIFF();
            }
            else if(extension == VisionImage.FileFilter.jpg.ToString())
            {
                provider = new CogImageFileJPEG();
            }
            
            provider.Open(fileName, CogImageFileModeConstants.Write);
            provider.Append(this.Image as ICogImage);
            provider.Close();
        }

        public int Load(string fileName)
        {
            int ret = 0;
            string extension = "";
            this.Image = null;
            ICogImageFileProvider provider = null;

            GC.Collect();

            if (fileName == string.Empty) return ret;

            extension = Path.GetExtension(fileName);
            extension = extension.Split('.')[1];

            if (extension == VisionImage.FileFilter.bmp.ToString())
            {
                provider = new CogImageFileBMP();
            }
            else if (extension == VisionImage.FileFilter.tif.ToString())
            {
                provider = new CogImageFileTIFF();
            }
            else
                throw new InvalidOperationException("Unsupported format");

            provider.Open(fileName, CogImageFileModeConstants.Read);

            if (provider.Count < 0)
            {
                provider.Close();
                return ret;
            }
            this.Image = provider[0];
            provider.Close();

            return ret;
        }
        #endregion

        #region IDisposable Members
        public void Dispose()
        {
            GC.SuppressFinalize(this);

            this.Disposed = true;
        }
        #endregion
    }
    #endregion
}
