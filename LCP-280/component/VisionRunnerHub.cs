using QMC.Common;
using QMC.Common.Cameras;
using QMC.Common.Vision;
using QMC.Common.Vision.Tools;
using QMC.LCP_280.Process;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace QMC.LCP_280.Process.Component
{
    /// <summary>
    /// 카메라별 PatternMatchingRunner 재사용/검색 공용 허브
    ///  - Thread-Safe
    ///  - RunnerOptions Factory 주입 가능
    ///  - 필요 시 Recipe Reload
    /// </summary>
    public static class VisionRunnerHub
    {
        private static readonly ConcurrentDictionary<string, PatternMatchingRunner> _runners =
            new ConcurrentDictionary<string, PatternMatchingRunner>(StringComparer.OrdinalIgnoreCase);

        private static Func<Camera, PatternMatchingRunner.RunnerOptions> _optionsFactory;

        // [ADD] 1회 초기화 가드
        private static int _initialized;

        // [ADD] 기본 Viewer 표시 옵션 (원하면 외부에서 교체 가능)
        private static PatternMatchingRunner.ViewerDisplayOptions _defaultViewerOptions =
            new PatternMatchingRunner.ViewerDisplayOptions
            {
                DrawCrossOnViewer = false,
                HighlightReferenceMatch = true,
                ShowMatchIndexes = false
            };

        /// <summary>
        /// 앱 전체에서 1회만 호출 (권장: Equipment.InitializeEquipment() 이후)
        /// </summary>
        public static void InitializeOnce(
            Func<Camera, PatternMatchingRunner.RunnerOptions> optionsFactory,
            PatternMatchingRunner.ViewerDisplayOptions defaultViewerOptions = null)
        {
            if (System.Threading.Interlocked.Exchange(ref _initialized, 1) == 1)
                return;

            if (optionsFactory != null)
                _optionsFactory = optionsFactory;

            if (defaultViewerOptions != null)
                _defaultViewerOptions = defaultViewerOptions;
        }


        public static void ConfigureOptionsFactory(Func<Camera, PatternMatchingRunner.RunnerOptions> factory)
        {
            _optionsFactory = factory;
        }

        public static PatternMatchingRunner GetOrCreate(string cameraKey)
        {
            var cam = ResolveCamera(cameraKey);
            if (cam == null) return null;

            return _runners.GetOrAdd(cameraKey, k =>
            {
                var opt = _optionsFactory != null
                    ? _optionsFactory(cam)
                    : CreateDefaultOptions(cam);
                return new PatternMatchingRunner(cam, null, opt);
            });
        }

        // [ADD] 폼/컨트롤에서는 이거 한 줄만 호출하면 됨
        public static PatternMatchingRunner AttachViewer(
            string cameraKey,
            VisionImageViewer viewer,
            PatternMatchingRunner.ViewerDisplayOptions displayOptions = null)
        {
            if (viewer == null) 
                return null;

            var runner = GetOrCreate(cameraKey);
            if (runner == null) 
                return null;

            try
            {
                runner.RegisterViewer(viewer, displayOptions ?? _defaultViewerOptions);
            }
            catch { }

            return runner;
        }

        // [ADD] FormClosing/Disposed에서 호출(버그/메모리 누수 방지)
        public static void DetachViewer(string cameraKey, VisionImageViewer viewer, bool clearOverlays = true)
        {
            if (viewer == null) 
                return;

            try
            {
                var runner = GetOrCreate(cameraKey);
                runner?.UnregisterViewer(viewer, clearOverlays);
            }
            catch { }
        }


        public static void DisposeAll()
        {
            foreach (var kv in _runners)
            {
                try { kv.Value.Dispose(); } catch { }
            }
            _runners.Clear();
        }

        public static bool ReloadRecipe(string cameraKey)
        {
            var r = GetOrCreate(cameraKey);
            return r != null && r.LoadRecipe();
        }

        private static Camera ResolveCamera(string key)
        {
            try
            {
                var eq = Equipment.Instance;
                if (eq?.Cameras != null && eq.Cameras.TryGetValue(key, out var cam))
                    return cam;
            }
            catch { }
            return null;
        }

        private static PatternMatchingRunner.RunnerOptions CreateDefaultOptions(Camera cam)
        {
            return new PatternMatchingRunner.RunnerOptions
            {
                AutoLoadRecipe = true,
                RecipeName = "Default",
                UseInspectRoi = true,
                Mode = PatternMatchingRunner.SearchMode.All,
                DrawCrossOnViewer = false,
                EnableSaveImage = false,
            };
        }

        
        // ================= Domain Friendly Wrapper =================

        public static (bool ok, List<PatternMatchingResult.PatternMatchingResultValue> matches, string error)
            SearchAll(string cameraKey, VisionImage extImage = null)
        {
            var runner = GetOrCreate(cameraKey);
            if (runner == null) return (false, null, "Camera/Runner null");
            try
            {
                var res = extImage == null ? runner.Search(false) : runner.Search(extImage, false);

                if (!res.Success || res.Matches == null || res.Matches.Count == 0)
                    return (false, null, res.FailReason ?? "No match");
                return (true, res.Matches, null);
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }

        public static (bool ok, double representativeAngle, List<double> angles, double avg, double std, string error)
            SearchAngles(string cameraKey, bool excludeExtremes = true)
        {
            var (ok, matches, error) = SearchAll(cameraKey);
            if (!ok || matches == null || matches.Count == 0)
                return (false, 0, null, 0, 0, error);

            var angles = matches.Select(m => m.R).ToList();
            if (angles.Count == 1)
                return (true, angles[0], angles, angles[0], 0, null);

            var ordered = angles.OrderBy(a => a).ToList();
            IEnumerable<double> work = ordered;
            if (excludeExtremes && ordered.Count >= 3)
                work = ordered.Skip(1).Take(ordered.Count - 2);

            double avg = work.Average();
            double std = Math.Sqrt(work.Sum(a => (a - avg) * (a - avg)) / Math.Max(1, work.Count() - 1));

            // 대표값: avg에 가장 가까운 원본 각도
            double rep = angles.OrderBy(a => Math.Abs(a - avg)).First();

            return (true, rep, angles, avg, std, null);
        }



        // ================= Common Overlay (MarksFound) =================

        public sealed class MarksOverlayOptions
        {
            /// <summary>
            /// ROI fallback 계산 시 X축 기준 ROI를 사용(가로가 길고 세로가 짧음)
            /// </summary>
            public bool UseXAxisRoi { get; set; } = true;

            /// <summary>
            /// Recipe ROI를 직접 제공할 수 있으면 우선 사용. null이면 fallback(center ROI)
            /// </summary>
            public Func<(Point start, Point end)?> RecipeInspectRoiProvider { get; set; }

            // ROI 표시 스타일
            public Color RecipeRoiColor { get; set; } = Color.Orange;
            public Color FallbackRoiColor { get; set; } = Color.Cyan;

            // Mark 표시 스타일
            public Color RepresentativeColor { get; set; } = Color.Yellow;
            public Color NormalColor { get; set; } = Color.Lime;

            public bool ShowTextInfo { get; set; } = true;

            /// <summary>
            /// text overlay 위치(픽셀). viewer 좌상단 기준
            /// </summary>
            public Point TextLocation { get; set; } = new Point(10, 50);
        }

        /// <summary>
        /// 어디서든 OnMarksFound 이벤트에서 호출해서 동일 오버레이를 적용
        /// </summary>

        private static readonly Font _overlayFont = new Font(FontFamily.GenericMonospace, 35f, FontStyle.Bold);


        public static void ApplyMarksFoundOverlays(
            VisionImageViewer viewer,
            PatternMarksFoundEventArgs e,
            MarksOverlayOptions options = null)
        {
            if (viewer == null || viewer.IsDisposed) return;
            if (e == null) return;

            // =========================================================================
            // [최적화 핵심] 
            // e.Image는 곧 Dispose 될 것이므로 절대 UI 스레드(BeginInvoke)로 넘기지 않습니다.
            // 대신 그리는 데 필요한 원시 데이터(int, double, List)만 미리 빼냅니다.
            // 이렇게 하면 Image.Clone() 같은 무거운 작업 없이 즉시 스레드를 풀어줄 수 있습니다.
            // =========================================================================
            int imgW = e.Image?.Header?.Width ?? 0;
            int imgH = e.Image?.Header?.Height ?? 0;

            // Marks 리스트 스냅샷 복사 (얕은 복사라 매우 빠름)
            var safeMarks = e.Marks?.ToList() ?? new List<PatternMatchInfo>();
            int repIndex = e.RepresentativeIndex;

            // ROI 정보 미리 추출
            (Point start, Point end)? roiInfo = null;
            try { roiInfo = options.RecipeInspectRoiProvider?.Invoke(); } catch { }

            // scale 변수 미리 추출 (dynamic 비용을 백그라운드에서 미리 처리)
            double scaleX = 0.005;
            double scaleY = 0.005;
            try
            {
                dynamic cam = viewer.Camera;
                if (cam != null && cam.CameraConfig != null && cam.CameraConfig.Scale != null)
                {
                    scaleX = Math.Abs((double)cam.CameraConfig.Scale.X);
                    scaleY = Math.Abs((double)cam.CameraConfig.Scale.Y);
                }
            }
            catch { }

            // 추출한 순수 데이터들만 넘깁니다.
            Action apply = () => ApplyMarksFoundOverlaysCoreFast(
                viewer, safeMarks, repIndex, imgW, imgH, roiInfo, scaleX, scaleY, options);

            if (viewer.InvokeRequired) viewer.BeginInvoke(apply);
            else apply();


            //options = options ?? new MarksOverlayOptions();
            //Action apply = () => ApplyMarksFoundOverlaysCore(viewer, e, options);
            //if (viewer.InvokeRequired) viewer.BeginInvoke(apply);
            //else apply();
        }

        // e.Image에 의존하지 않는 Fast Core 함수 새로 작성
        private static void ApplyMarksFoundOverlaysCoreFast(
            VisionImageViewer viewer,
            List<PatternMatchInfo> marks,
            int rep,
            int imgW,
            int imgH,
            (Point start, Point end)? roi,
            double scaleX,
            double scaleY,
            MarksOverlayOptions options)
        {
            try
            {
                var list = viewer.ResultOverlays as System.Collections.IList;
                if (list == null)
                {
                    try { viewer.ResumeDisplay(); } catch { }
                    return;
                }

                if (rep < 0 || rep >= marks.Count)
                    rep = (marks.Count > 0) ? 0 : -1;

                // 1) ROI 결정
                bool hasRecipeRoi = false;
                Point roiStart = Point.Empty;
                Point roiEnd = Point.Empty;

                if (roi.HasValue)
                {
                    roiStart = roi.Value.start;
                    roiEnd = roi.Value.end;
                    hasRecipeRoi = !(roiStart.IsEmpty && roiEnd.IsEmpty);
                }

                if (hasRecipeRoi && imgW > 0 && imgH > 0)
                {
                    roiStart = new Point(
                        Math.Max(0, Math.Min(imgW - 1, roiStart.X)),
                        Math.Max(0, Math.Min(imgH - 1, roiStart.Y)));

                    roiEnd = new Point(
                        Math.Max(0, Math.Min(imgW - 1, roiEnd.X)),
                        Math.Max(0, Math.Min(imgH - 1, roiEnd.Y)));

                    int sx = Math.Min(roiStart.X, roiEnd.X);
                    int sy = Math.Min(roiStart.Y, roiEnd.Y);
                    int ex = Math.Max(roiStart.X, roiEnd.X);
                    int ey = Math.Max(roiStart.Y, roiEnd.Y);
                    roiStart = new Point(sx, sy);
                    roiEnd = new Point(ex, ey);

                    if (ex - sx < 2 || ey - sy < 2)
                        hasRecipeRoi = false;
                }

                lock (list)
                {
                    list.Clear();

                    // 0) ROI Overlay
                    if (imgW > 0 && imgH > 0)
                    {
                        Point sx0, ex0;

                        if (hasRecipeRoi)
                        {
                            sx0 = roiStart;
                            ex0 = roiEnd;
                        }
                        else
                        {
                            // fallback: center ROI (mm-based)
                            double roiWmm = options.UseXAxisRoi ? 5.0 : 1.5;
                            double roiHmm = options.UseXAxisRoi ? 1.5 : 5.0;

                            if (scaleX <= 0) scaleX = 0.005;
                            if (scaleY <= 0) scaleY = 0.005;

                            int roiWpx = Math.Max(2, (int)Math.Round(roiWmm / scaleX));
                            int roiHpx = Math.Max(2, (int)Math.Round(roiHmm / scaleY));

                            int cx = imgW / 2;
                            int cy = imgH / 2;

                            int halfW = Math.Max(1, roiWpx / 2);
                            int halfH = Math.Max(1, roiHpx / 2);

                            sx0 = new Point(Math.Max(0, cx - halfW), Math.Max(0, cy - halfH));
                            ex0 = new Point(Math.Min(imgW - 1, cx + halfW), Math.Min(imgH - 1, cy + halfH));
                        }

                        list.Add(new QMC.Common.Vision.RectangleFrameVisionImageOverlay("ROI", sx0, ex0)
                        {
                            Color = hasRecipeRoi ? options.RecipeRoiColor : options.FallbackRoiColor,
                            Thickness = 2,
                            DashStyle = System.Drawing.Drawing2D.DashStyle.Dash,
                            Visible = true
                        });
                    }

                    // 1) Mark overlays
                    for (int i = 0; i < marks.Count; i++)
                    {
                        var m = marks[i];
                        bool isRep = (i == rep);

                        list.Add(new QMC.Common.Vision.PatternMatchResultOverlay
                        {
                            Center = new PointD((float)m.X, (float)m.Y),
                            PatternWidth = m.TrainW > 0 ? m.TrainW : 40,
                            PatternHeight = m.TrainH > 0 ? m.TrainH : 40,
                            AngleDeg = (float)m.AngleDeg,
                            CrossHalfLenPx = isRep ? 24 : 16,
                            Highlight = isRep,
                            Index = i,
                            Color = isRep ? options.RepresentativeColor : options.NormalColor,
                            Thickness = isRep ? 2f : 1f,
                            Visible = true
                        });
                    }

                    // 2) Rep info text
                    if (options.ShowTextInfo && rep >= 0 && rep < marks.Count)
                    {
                        var m = marks[rep];
                        string roiMode = options.UseXAxisRoi ? "ROI:X" : "ROI:Y";
                        string roiSrc = hasRecipeRoi ? "RecipeROI" : "CenterROI";
                        string nl = Environment.NewLine;

                        string text =
                            $"{roiMode} {roiSrc}  REP[{rep}]{nl}" +
                            $"X : {m.X:0.###}{nl}" +
                            $"Y : {m.Y:0.###}{nl}" +
                            $"T : {m.AngleDeg:0.###} deg{nl}" +
                            $"Score : {m.Score:0.###}";

                        var loc = options.TextLocation;
                        loc = new Point(loc.X, Math.Max(loc.Y, 50));

                        list.Add(new QMC.Common.Vision.TextVisionImageOverlay(
                            "REP_INFO",
                            loc,
                            _overlayFont) // 매번 Font 객체 생성하지 않고 정적 객체 재사용
                        {
                            Text = text,
                            Color = Color.Yellow,
                            BrushColor = Brushes.Yellow,
                            Visible = true
                        });
                    }
                }

                viewer.ResumeDisplay();
            }
            catch
            {
                try { viewer?.ResumeDisplay(); } catch { }
            }
        }


        private static void ApplyMarksFoundOverlaysCore(
            VisionImageViewer viewer,
            PatternMarksFoundEventArgs e,
            MarksOverlayOptions options)
        {
            try
            {
                var list = viewer.ResultOverlays as System.Collections.IList;
                if (list == null)
                {
                    try { viewer.ResumeDisplay(); } catch { }
                    return;
                }

                int imgW = e.Image?.Header?.Width ?? 0;
                int imgH = e.Image?.Header?.Height ?? 0;

                int rep = e.RepresentativeIndex;
                if (rep < 0 || rep >= (e.Marks?.Count ?? 0))
                    rep = (e.Marks != null && e.Marks.Count > 0) ? 0 : -1;

                // 1) ROI 결정 (Recipe 우선, 없으면 center ROI)
                bool hasRecipeRoi = false;
                Point roiStart = Point.Empty;
                Point roiEnd = Point.Empty;

                try
                {
                    var roi = options.RecipeInspectRoiProvider != null
                        ? options.RecipeInspectRoiProvider()
                        : (ValueTuple<Point, Point>?)null;

                    if (roi.HasValue)
                    {
                        roiStart = roi.Value.Item1;
                        roiEnd = roi.Value.Item2;
                        hasRecipeRoi = !(roiStart.IsEmpty && roiEnd.IsEmpty);
                    }

                    if (hasRecipeRoi && imgW > 0 && imgH > 0)
                    {
                        roiStart = new Point(
                            Math.Max(0, Math.Min(imgW - 1, roiStart.X)),
                            Math.Max(0, Math.Min(imgH - 1, roiStart.Y)));

                        roiEnd = new Point(
                            Math.Max(0, Math.Min(imgW - 1, roiEnd.X)),
                            Math.Max(0, Math.Min(imgH - 1, roiEnd.Y)));

                        int sx = Math.Min(roiStart.X, roiEnd.X);
                        int sy = Math.Min(roiStart.Y, roiEnd.Y);
                        int ex = Math.Max(roiStart.X, roiEnd.X);
                        int ey = Math.Max(roiStart.Y, roiEnd.Y);
                        roiStart = new Point(sx, sy);
                        roiEnd = new Point(ex, ey);

                        if (ex - sx < 2 || ey - sy < 2)
                            hasRecipeRoi = false;
                    }
                }
                catch
                {
                    hasRecipeRoi = false;
                }

                lock (list)
                {
                    list.Clear();

                    // 0) ROI Overlay
                    if (imgW > 0 && imgH > 0)
                    {
                        Point sx0, ex0;

                        if (hasRecipeRoi)
                        {
                            sx0 = roiStart;
                            ex0 = roiEnd;
                        }
                        else
                        {
                            // fallback: center ROI (mm-based)
                            double roiWmm = options.UseXAxisRoi ? 5.0 : 1.5;
                            double roiHmm = options.UseXAxisRoi ? 1.5 : 5.0;

                            double mmPerPxX = 0.0;
                            double mmPerPxY = 0.0;
                            try
                            {
                                dynamic cam = viewer.Camera;
                                if (cam != null && cam.CameraConfig != null && cam.CameraConfig.Scale != null)
                                {
                                    mmPerPxX = Math.Abs((double)cam.CameraConfig.Scale.X);
                                    mmPerPxY = Math.Abs((double)cam.CameraConfig.Scale.Y);
                                }
                            }
                            catch { }

                            if (mmPerPxX <= 0) mmPerPxX = 0.005;
                            if (mmPerPxY <= 0) mmPerPxY = 0.005;

                            int roiWpx = Math.Max(2, (int)Math.Round(roiWmm / mmPerPxX));
                            int roiHpx = Math.Max(2, (int)Math.Round(roiHmm / mmPerPxY));

                            int cx = imgW / 2;
                            int cy = imgH / 2;

                            int halfW = Math.Max(1, roiWpx / 2);
                            int halfH = Math.Max(1, roiHpx / 2);

                            int sx = Math.Max(0, cx - halfW);
                            int sy = Math.Max(0, cy - halfH);
                            int ex = Math.Min(imgW - 1, cx + halfW);
                            int ey = Math.Min(imgH - 1, cy + halfH);

                            sx0 = new Point(sx, sy);
                            ex0 = new Point(ex, ey);
                        }

                        var roiOv = new QMC.Common.Vision.RectangleFrameVisionImageOverlay("ROI", sx0, ex0)
                        {
                            Color = hasRecipeRoi ? options.RecipeRoiColor : options.FallbackRoiColor,
                            Thickness = 2,
                            DashStyle = System.Drawing.Drawing2D.DashStyle.Dash,
                            Visible = true
                        };
                        list.Add(roiOv);
                    }

                    // 1) Mark overlays
                    if (e.Marks != null)
                    {
                        for (int i = 0; i < e.Marks.Count; i++)
                        {
                            var m = e.Marks[i];
                            bool isRep = (i == rep);

                            list.Add(new QMC.Common.Vision.PatternMatchResultOverlay
                            {
                                Center = new PointD((float)m.X, (float)m.Y),
                                PatternWidth = m.TrainW > 0 ? m.TrainW : 40,
                                PatternHeight = m.TrainH > 0 ? m.TrainH : 40,
                                AngleDeg = (float)m.AngleDeg,
                                CrossHalfLenPx = isRep ? 24 : 16,
                                Highlight = isRep,
                                Index = i,
                                Color = isRep ? options.RepresentativeColor : options.NormalColor,
                                Thickness = isRep ? 2f : 1f,
                                Visible = true
                            });
                        }
                    }

                    // 2) Rep info text
                    if (options.ShowTextInfo && rep >= 0 && e.Marks != null && rep < e.Marks.Count)
                    {
                        var m = e.Marks[rep];
                        string roiMode = options.UseXAxisRoi ? "ROI:X" : "ROI:Y";
                        string roiSrc = hasRecipeRoi ? "RecipeROI" : "CenterROI";

                        string nl = Environment.NewLine;
                        string text =
                            $"{roiMode} {roiSrc}  REP[{rep}]{nl}" +
                            $"X : {m.X:0.###}{nl}" +
                            $"Y : {m.Y:0.###}{nl}" +
                            $"T : {m.AngleDeg:0.###} deg{nl}" +
                            $"Score : {m.Score:0.###}";

                        var loc = options.TextLocation;
                        loc = new Point(loc.X, Math.Max(loc.Y, 50)); // 최소 25px 아래

                        list.Add(new QMC.Common.Vision.TextVisionImageOverlay(
                            "REP_INFO",
                            loc,
                            new Font(FontFamily.GenericMonospace, 35f, FontStyle.Bold))
                        {
                            Text = text,
                            Color = Color.Yellow,
                            BrushColor = Brushes.Yellow,
                            Visible = true
                        });
                    }
                }

                viewer.ResumeDisplay();
            }
            catch
            {
                try { viewer?.ResumeDisplay(); } catch { }
            }
        }

    }
}