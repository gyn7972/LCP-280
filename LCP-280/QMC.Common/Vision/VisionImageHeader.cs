/*
 * Purpose
 *      QMC 회사만의 Vision Image Header Class에 대해서 정의한다.
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
using System.Linq;
using System.Text;
using System.Drawing.Imaging;
using System.ComponentModel;

namespace QMC.Common.Vision
{
	#region VisionImageHeader
	[Serializable]
    public class VisionImageHeader
    {
        #region Define
        #endregion

        #region Field
        private IntPtr m_Pointer;
        private int m_Width;
        private int m_Height;
        private int m_BufferSize;
        private int m_Stride;
        private int m_BitsPerPixel;
        private PixelFormat m_PixelFormat;
        private string m_OwnerName;
        private string m_FilePath;
        #endregion

        #region Constructor
        public VisionImageHeader()
        {
        }
        #endregion

        #region Property
        public IntPtr Pointer
        {
            get { return this.m_Pointer; }
            set { this.m_Pointer = value; }
        }

        /// <summary>
        /// Image의 Width를 가져오거나 설정한다.
        /// </summary>
        [DefaultValue(0)]
        public int Width
        {
            get { return this.m_Width; }
            set { this.m_Width = value; }
        }

        /// <summary>
        /// Image의 Height를 가져오거나 설정한다.
        /// </summary>
        [DefaultValue(0)]
        public int Height
        {
            get { return this.m_Height; }
            set { this.m_Height = value; }
        }

        /// <summary>
        /// Image의 BufferSize를 가져오거나 설정한다.
        /// </summary>
        [DefaultValue(0)]
        public int BufferSize
        {
            get { return this.m_BufferSize; }
            set { this.m_BufferSize = value; }
        }

        /// <summary>
        /// Image의 Stride를 가져오거나 설정한다.
        /// </summary>
        [DefaultValue(0)]
        public int Stride
        {
            get { return this.m_Stride; }
            set { this.m_Stride = value; }
        }

        /// <summary>
        /// Image의 Pixel당 Bits를 가져오거나 설정한다.
        /// </summary>
        [DefaultValue(0)]
        public int BitsPerPixel
        {
            get { return this.m_BitsPerPixel; }
            set { this.m_BitsPerPixel = value; }
        }

        /// <summary>
        /// Image의 Pixel의 Format을 가져오거나 설정한다.
        /// </summary>
        [DefaultValue(PixelFormat.Undefined)]
        public PixelFormat PixelFormat
        {
            get { return this.m_PixelFormat; }
            set { this.m_PixelFormat = value; }
        }

        public string OwnerName
        {
            get { return this.m_OwnerName; }
            set { this.m_OwnerName = value; }
        }
        
        /// <summary>
        /// Simulation 일경우, File 경로를 가져오거나 설정한다.
        /// </summary>
        public string FilePath
        {
            get { return this.m_FilePath; }
            set { this.m_FilePath = value; }
        }
        #endregion

        #region Method
        #endregion
    }
	#endregion

	#region GrayVisionImageHeader
	[Serializable]
    public class GrayVisionImageHeader : VisionImageHeader
    {
        #region Field
        #endregion

        #region Constructor
        public GrayVisionImageHeader()
        {
        }
        #endregion

        #region Property
        #endregion
    }
	#endregion

	#region ColorVisionImageHeader
	[Serializable]
    public class ColorVisionImageHeader : VisionImageHeader
    {
        #region Field
        #endregion

        #region Constructor
        public ColorVisionImageHeader()
        {
        }
        #endregion

        #region Property
        #endregion
    }
	#endregion
}
