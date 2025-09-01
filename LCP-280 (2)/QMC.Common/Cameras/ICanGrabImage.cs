/*
 * Purpose
 *      이미지를 획득이 가능한 파트에 대해서 정의한다.
 * 
 * Revision
 *      1. Created: 2019.11.04 JUNG.CY
 * 
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using QMC.Common.Vision;
using QMC.Common.Component;

namespace QMC.Common.Cameras
{
    [Serializable]
    public enum Purpose
    {
        Processing,
        Display,
    }

    public interface ICanGrabImage
    {
        Size Resolution { get; }
        SizeD PixelResolution { get; }
        VisionImage LatestImage { get; }
        bool IsLiveOn { get; }
        bool AutoSleepEnable { get; }
        bool Sleep { get; }
        bool Opened { get; }
        /// <summary>
        /// ImageViewer에서 새로운 이미지 표시를 일시 중지 할지 여부를 가져오거나 설정한다.
        /// 원본 이미지를 가공하여 표시하고자 하는 경우 사용한다.
        /// </summary>
        bool SuspendedImageDisplay { get; set; }

        BaseComponent Owner { get; }

        //MethodCallerAsyncResult BeginGrab(Purpose purpose, out VisionImage image, MethodCallerAsyncCallback callback, object value);
        //MethodCallerAsyncResult BeginGrab(out VisionImage image, MethodCallerAsyncCallback callback, object value);
        //MethodCallerAsyncResult BeginGrab(Purpose purpose, MethodCallerAsyncCallback callback, object value);
        //MethodCallerAsyncResult BeginGrab(MethodCallerAsyncCallback callback, object value);
        //MethodCallerAsyncResult BeginGrab(Purpose purpose, out VisionImage image);
        //MethodCallerAsyncResult BeginGrab(out VisionImage image);
        //MethodCallerAsyncResult BeginGrab(Purpose purpose);
        //MethodCallerAsyncResult BeginGrab();
        //int EndGrab(MethodCallerAsyncResult ar, out VisionImage result);
        //int EndGrab(MethodCallerAsyncResult ar);
        int Grab(Purpose purpose, out VisionImage image);
        int Grab(Purpose purpose);
        int Grab(out VisionImage image);
        int Grab();
        //int GrabSync(Purpose purpose, out VisionImage image);
        //int GrabSync(Purpose purpose);
        //int GrabSync(out VisionImage image);
        //int GrabSync();

        //MethodCallerAsyncResult BeginStartLive(MethodCallerAsyncCallback callback, object value);
        //MethodCallerAsyncResult BeginStartLive();
        //int EndStartLive(MethodCallerAsyncResult ar);
        int StartLive();
        int StartLiveSync();

        //MethodCallerAsyncResult BeginStopLive(MethodCallerAsyncCallback callback, object value);
        //MethodCallerAsyncResult BeginStopLive();
        //int EndStopLive(MethodCallerAsyncResult ar);
        int StopLive();
        int StopLiveSync();

        //MethodCallerAsyncResult BeginOpen(MethodCallerAsyncCallback callback, object value);
        //MethodCallerAsyncResult BeginOpen();
        //int EndOpen(MethodCallerAsyncResult ar);
        int Open();
        int OpenSync();

        //MethodCallerAsyncResult BeginClose(MethodCallerAsyncCallback callback, object value);
        //MethodCallerAsyncResult BeginClose();
        //int EndClose(MethodCallerAsyncResult ar);
        int Close();
        int CloseSync();
    }
}
