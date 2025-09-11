using QMC.Common;
using QMC.Common.Cameras;
using QMC.Common.Vision;
using QMC.Common.Vision.Tools;
using QMC.LCP_280.Process;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

        public static (bool ok, double dxMm, double dyMm, string error)
            SearchCenterOffset(string cameraKey,
                                double pixelSizeXmm,
                                double pixelSizeYmm,
                                double? originX = null,
                                double? originY = null,
                                bool useImageCenterIfNoOrigin = true)
        {
            var runner = GetOrCreate(cameraKey);
            if (runner == null) return (false, 0, 0, "Runner null");
            try
            {
                runner.SetSearchMode(PatternMatchingRunner.SearchMode.First);
                var res = runner.Search(false);
                if (!res.Success || res.Matches == null || res.Matches.Count == 0)
                    return (false, 0, 0, res.FailReason ?? "No match");

                var match = res.Matches[(res.ReferenceIndex >= 0 && res.ReferenceIndex < res.Matches.Count) ? res.ReferenceIndex : 0];
                var img = runner.GetLastPoint(); // center point not raw image, so recalc using last search image header 필요 → 간단화

                var cam = ResolveCamera(cameraKey);
                var latest = cam?.LatestImage;
                if (latest?.Header == null) return (false, 0, 0, "No image header");

                double ox, oy;
                if (useImageCenterIfNoOrigin || !originX.HasValue || !originY.HasValue ||
                    double.IsNaN(originX.Value) || double.IsNaN(originY.Value))
                {
                    ox = latest.Header.Width / 2.0;
                    oy = latest.Header.Height / 2.0;
                }
                else
                {
                    ox = originX.Value;
                    oy = originY.Value;
                }

                double dxPix = match.X - ox;
                double dyPix = match.Y - oy;
                return (true, dxPix * pixelSizeXmm, dyPix * pixelSizeYmm, null);
            }
            catch (Exception ex)
            {
                return (false, 0, 0, ex.Message);
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
    }
}