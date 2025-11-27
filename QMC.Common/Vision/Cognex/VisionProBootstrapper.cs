using System;
using System.Drawing;
using System.Drawing.Imaging;
using Cognex.VisionPro;

namespace QMC.Common.Vision.Cognex
{
    public static class VisionProBootstrapper
    {
        private static volatile bool _initialized;
        private static readonly object _sync = new object();

        // 디버깅 시 첫 VisionPro 사용 전 한 번 호출
        public static void WarmUp()
        {
            if (_initialized) 
                return;

            lock (_sync)
            {
                if (_initialized) 
                    return;

                try
                {
                    // 최소 사용으로 VisionPro 네이티브 모듈 로드 유도
                    using (var bmp = new Bitmap(1, 1, PixelFormat.Format8bppIndexed))
                    {
                        var cogImg = new CogImage8Grey(bmp);
                        // 일부 경로를 더 로드하도록 ToBitmap까지 호출
                        using (var _ = cogImg.ToBitmap()) { }
                    }
                }
                catch
                {
                    // 워밍업 실패는 무시(실사용 시 로드됨)
                }
                _initialized = true;
            }
        }
    }
}
