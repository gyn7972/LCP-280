using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Drawing2D; // added for DashStyle
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using QMC.Common;
using QMC.Common.Cameras;
using QMC.Common.Vision;
using QMC.Common.Vision.Tools;
using QMC.Common.VisionPart;
using QMC.LCP_280.Process.Component; // added for MeasurementRecipe & RecipeManager

namespace QMC.LCP_280.Process
{
    /// <summary>
    /// PatternMatchingRunner
    ///  - 레시피 로드/검색/결과 저장/오버레이 표시/이벤트 제공
    ///  - 단일/전체 검색(SearchMode.First / SearchMode.All)
    ///  - Extremes(Score Max/Min) 제외 평균 계산
    ///  - 멀티 카메라 Runner 풀 (정적 GetOrCreateRunner)
    ///  - 전역 동시 검색 제한 (MaxGlobalConcurrency)
    ///  - 성능 통계 및 CSV 로그 (Rolling 평균/Min/Max/SuccessRate)
    ///  - 비동기 검색 / 외부 이미지 / 배치 검색
    /// </summary>
    public class PatternMatchingRunner : IDisposable
    {
        #region Enums / Options
        public enum SaveMode { None, OkOnly, NgOnly, All }
        public enum SearchMode { First, All }

        public class RunnerOptions
        {
            // Recipe / Search
            public bool AutoLoadRecipe = true;
            public string RecipeRootDirectory;
            public string RecipeName = "Default";
            public bool RetrainAlways = false;
            public bool UseInspectRoi = true;
            public SearchMode Mode = SearchMode.First;

            // Display
            public bool DrawCrossOnViewer = false;
            public Color CrossColor = Color.Lime;
            public int CrossHalfLength = 15;
            public float CrossPenWidth = 2f;

            // Save
            public bool EnableSaveImage = false;
            public bool SaveOverlay = true;
            public bool SaveRawAlso = false;
            public bool SaveJson = false;
            public SaveMode ImageSaveMode = SaveMode.None;
            public string SaveRootDirectory;
            public string FileNamePrefix = "PM";
            public int OverlayPenWidth = 2;
            public Color OverlayPenColor = Color.Red;

            // Errors
            public bool ThrowOnSearchError = false;

            // Performance logging / pooling
            public bool EnablePerformanceLog = false;
            public string PerformanceLogDirectory;          // default: SaveRootDirectory/Perf
            public int PerfRollingWindow = 50;              // recent N items rolling average
            public int PerfFlushEvery = 20;                 // flush every N records
            public int PerfMaxBufferedLines = 200;          // safety buffer flush
            public int MaxGlobalConcurrency = 4;            // global parallel search limit
            public int MaxPerCameraConcurrency = 1;         // per camera limit
            public bool AppendProcessIdToLog = true;        // separate per process

            // Highlighting / Annotations
            public bool HighlightReferenceMatch = true;        // 대표 매치 강조
            public bool ShowMatchIndexes = false;               // 매치 인덱스 번호 표시
            public Color IndexTextColor = Color.Yellow;
            public Color ReferenceMarkColor = Color.Cyan;       // 강조 원 색
            public int ReferenceMarkRadius = 25;                // 강조 원 반경
        }
        #endregion

        #region Result DTO
        public class PatternMatchRunResult
        {
            public bool Success;
            public double X;
            public double Y;
            public double R;
            public PatternMatchingResult RawResult;
            public string FailReason;
            public string SavedImagePath;
            public string SavedRawImagePath;
            public string SavedJsonPath;
            public TimeSpan Elapsed;
            public int TemplateTrainedCount;
            public List<PatternMatchingResult.PatternMatchingResultValue> Matches;
            public double? AvgXExcludingExtremes;
            public double? AvgYExcludingExtremes;
            public double? AvgRExcludingExtremes;
            public double? AvgScoreExcludingExtremes;
            public int ReferenceIndex = -1; // 대표 매치 인덱스
            public DateTime Timestamp = DateTime.Now;
        }
        #endregion

        #region Performance Stats
        private class PerfStats
        {
            public long Count;
            public long SuccessCount;
            public double TotalMs;
            public double MinMs = double.MaxValue;
            public double MaxMs;
            public double RollingSumMs;
            public readonly Queue<double> RollingQueue = new Queue<double>();
            public int RollingWindow;
            public List<string> PendingLines = new List<string>();
            public string CsvPath;
            public DateTime LastFlush = DateTime.Now;
            public object LockObj = new object();
            public void Update(double ms, bool success, int matches, PatternMatchRunResult r)
            {
                Count++;
                if (success) SuccessCount++;
                TotalMs += ms;
                if (ms < MinMs) MinMs = ms;
                if (ms > MaxMs) MaxMs = ms;
                RollingQueue.Enqueue(ms);
                RollingSumMs += ms;
                while (RollingQueue.Count > RollingWindow)
                    RollingSumMs -= RollingQueue.Dequeue();
                double rollingAvg = RollingQueue.Count == 0 ? 0 : RollingSumMs / RollingQueue.Count;
                double avgMs = TotalMs / Count;
                double successRate = SuccessCount == 0 ? 0 : (double)SuccessCount / Count;
                var line = string.Format(
                    "{0:O},{1},{2},{3:0.###},{4:0.###},{5:0.###},{6:0.###},{7:0.###},{8:0.###},{9:0.###},{10}",
                    r.Timestamp,
                    success ? "OK" : "NG",
                    matches,
                    ms,
                    avgMs,
                    rollingAvg,
                    MinMs == double.MaxValue ? 0 : MinMs,
                    MaxMs,
                    successRate * 100.0,
                    r.AvgScoreExcludingExtremes ?? -1,
                    r.FailReason ?? "");
                PendingLines.Add(line);
            }
        }
        #endregion

        #region Static Pool / Registry
        private static readonly ConcurrentDictionary<string, PatternMatchingRunner> s_runners = new ConcurrentDictionary<string, PatternMatchingRunner>();
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> s_cameraSemaphores = new ConcurrentDictionary<string, SemaphoreSlim>();
        private static SemaphoreSlim s_globalSemaphore = new SemaphoreSlim(Environment.ProcessorCount);

        public static PatternMatchingRunner GetOrCreateRunner(Camera camera, VisionImageViewer viewer, RunnerOptions options)
        {
            if (camera == null) throw new ArgumentNullException(nameof(camera));
            return s_runners.GetOrAdd(camera.Name ?? camera.GetHashCode().ToString(), _ =>
            {
                // Ensure global semaphore respects first constructed options (cannot resize later)
                if (options.MaxGlobalConcurrency > 0 && s_globalSemaphore == null)
                {
                    Interlocked.CompareExchange(ref s_globalSemaphore, new SemaphoreSlim(options.MaxGlobalConcurrency), null);
                }
                return new PatternMatchingRunner(camera, viewer, options);
            });
        }

        public static async Task<PatternMatchRunResult[]> SearchAllAsync(IEnumerable<Camera> cameras, Func<Camera, VisionImageViewer> viewerSelector, RunnerOptions options, CancellationToken ct = default)
        {
            if (cameras == null) return Array.Empty<PatternMatchRunResult>();
            var tasks = new List<Task<PatternMatchRunResult>>();
            foreach (var cam in cameras)
            {
                if (cam == null) continue;
                if (ct.IsCancellationRequested) break;
                var runner = GetOrCreateRunner(cam, viewerSelector?.Invoke(cam), options);
                tasks.Add(runner.SearchAsync(false, ct));
            }
            return await Task.WhenAll(tasks).ConfigureAwait(false);
        }
        #endregion

        #region Events
        public event Action<PatternMatchRunResult> AfterSearch;
        public event Action<PatternMatchRunResult> SearchSucceeded;
        public event Action<PatternMatchRunResult> SearchFailed;
        #endregion

        #region Multi-Viewer Support
        /// <summary>
        /// Per viewer display override (optional). If a value is null global RunnerOptions are used.
        /// </summary>
        public class ViewerDisplayOptions
        {
            public bool? DrawCrossOnViewer;   // null -> use global
            public bool? ShowMatchIndexes;    // null -> use global
            public bool? HighlightReference;  // null -> use global
        }
        #endregion
        #region Fields
        private readonly object _sync = new object();
        private readonly Camera _camera;
        private readonly VisionImageViewer _primaryViewer; // keep backward compatibility
        private readonly VisionImageViewer _viewer; // null 가능
        private readonly MultiPatternMatchingVisionPart _part;
        private MultiPatternMatchingParameters _parameters;
        private bool _recipeLoaded;
        private bool _disposed;
        private Point _lastPoint = Point.Empty;
        private double _lastAngle;
        private string _lastFailReason;
        private readonly RunnerOptions _opt;
        private Pen _crossPen;
        private string _lastTemplateHash = string.Empty;
        private string _dirOK;
        private string _dirNG;
        private string _dirRaw;
        private readonly System.Diagnostics.Stopwatch _sw = new System.Diagnostics.Stopwatch();
        private readonly PerfStats _perfStats = new PerfStats();
        private SemaphoreSlim _cameraSemaphore;

        private PatternMatchingResult _lastRawResult;          // 마지막 RawResult 저장 (재표시용)
        private int _lastReferenceIndex = -1;                   // 마지막 대표 매치 인덱스
        private Font _indexFont = new Font(FontFamily.GenericSansSerif, 15f, FontStyle.Bold);
        // Viewer management (restored)
        private readonly object _viewersLock = new object();
        private readonly HashSet<VisionImageViewer> _viewers = new HashSet<VisionImageViewer>();
        private readonly Dictionary<VisionImageViewer, ViewerDisplayOptions> _viewerOptions = new Dictionary<VisionImageViewer, ViewerDisplayOptions>();
        #endregion

        #region Ctor
        public PatternMatchingRunner(Camera camera, VisionImageViewer viewer, RunnerOptions options)
        {
            _camera = camera ?? throw new ArgumentNullException(nameof(camera));
            _primaryViewer = viewer; // store
            _opt = options ?? throw new ArgumentNullException(nameof(options));
            _viewer = viewer; // legacy field reference (kept for minimal change)
            if (string.IsNullOrWhiteSpace(_opt.RecipeRootDirectory))
                _opt.RecipeRootDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "PatternMatching");
            if (string.IsNullOrWhiteSpace(_opt.SaveRootDirectory))
                _opt.SaveRootDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ResultImages", "Pattern");
            if (string.IsNullOrWhiteSpace(_opt.PerformanceLogDirectory))
                _opt.PerformanceLogDirectory = Path.Combine(_opt.SaveRootDirectory, "Perf");
            _part = new TempVisionPart("PM_Runtime");
            _part.Create();
            _part.Simulated = false;
            _part.Camera = camera;
            BuildCrossPen();
            if (viewer != null)
                RegisterViewer(viewer); // instead of HookViewer
            if (_opt.AutoLoadRecipe)
                LoadRecipe();
            PrepareSaveDirectories();
            _perfStats.RollingWindow = Math.Max(1, _opt.PerfRollingWindow);
            if (_opt.EnablePerformanceLog)
                InitPerfLog();
            if (_opt.MaxGlobalConcurrency > 0)
                s_globalSemaphore = s_globalSemaphore ?? new SemaphoreSlim(_opt.MaxGlobalConcurrency);
            _cameraSemaphore = s_cameraSemaphores.GetOrAdd(_camera.Name ?? "__cam__", _ => new SemaphoreSlim(Math.Max(1, _opt.MaxPerCameraConcurrency)));
        }
        #endregion

        #region Public API
        public bool LoadRecipe()
        {
            lock (_sync)
            {
                try
                {
                    // Resolve recipe path from MeasurementRecipe (Vision settings) first
                    string path = ResolveVisionRecipePathFromMeasurement();

                    // Fallback to legacy per-camera location if not resolved
                    if (string.IsNullOrEmpty(path))
                    {
                        
                        string camFolder = Path.Combine(_opt.RecipeRootDirectory, _camera.Name ?? "NoCamera");
                        path = Path.Combine(camFolder, _opt.RecipeName + ".Vision.json");
                    }

                    var container = PatternMatchingRecipeStore.Load(path);
                    if (container == null)
                    {
                        _lastFailReason = "Recipe 없음: " + path;
                        _recipeLoaded = false;
                        return false;
                    }
                    _parameters = container.Parameters?.Clone();
                    if (_parameters == null)
                    {
                        _lastFailReason = "Recipe 파라미터 없음";
                        _recipeLoaded = false;
                        return false;
                    }
                    _part.SetPatternMatchingParameters(_parameters);
                    if (container.Roi != null)
                    {
                        try
                        {
                            _part.SetTrainStartPoint(container.Roi.TrainStart);
                            _part.SetTrainEndPoint(container.Roi.TrainEnd);
                            _part.SetInspectStartPoint(container.Roi.InspectStart);
                            _part.SetInspectEndPoint(container.Roi.InspectEnd);
                        }
                        catch { }
                    }
                    _recipeLoaded = true;
                    return true;
                }
                catch (Exception ex)
                {
                    _lastFailReason = "LoadRecipe 예외: " + ex.Message;
                    _recipeLoaded = false;
                    return false;
                }
            }
        }

        // Try resolve recipe path using MeasurementRecipe.UseVisionRecipe, VisionRecipeName, VisionRecipePath
        private string ResolveVisionRecipePathFromMeasurement()
        {
            try
            {
                // Determine currently opened measurement recipe name from Equipment (if available)
                string currentRecipeName = null;
                try 
                {
                    var eq = Equipment.Instance;
                    //_currentRecipeName = Equipment._CurrentRecipeName;
                    currentRecipeName = eq.EquipmentRecipe.CurrentRecipeName;
                    //currentRecipeName = Equipment._CurrentRecipeName; 
                } 
                catch { currentRecipeName = null; }

                if (string.IsNullOrWhiteSpace(currentRecipeName)) return null;

                var br = RecipeManager.LoadOrCreate(typeof(MeasurementRecipe), currentRecipeName) as QMC.Common.BaseRecipe;
                var mr = br as MeasurementRecipe;
                if (mr == null) return null;
                if (!mr.UseVisionRecipe) return null;

                string recipeName = mr.VisionRecipeName;
                string recipePath = mr.VisionRecipePath;

                // If VisionRecipePath directly points to a file, use it
                if (!string.IsNullOrWhiteSpace(recipePath))
                {
                    if (File.Exists(recipePath))
                    {
                        return recipePath;
                    }
                    // If it's a directory, try typical layouts
                    if (Directory.Exists(recipePath))
                    {
                        // 1) directory/<camera>/<name>.pmrecipe.json
                        if (!string.IsNullOrWhiteSpace(recipeName))
                        {
                            string p1 = Path.Combine(recipePath, _camera?.Name ?? "NoCamera", recipeName + ".Vision.json");
                            if (File.Exists(p1)) 
                                return p1;

                            // 2) directory/<name>.pmrecipe.json
                            string p2 = Path.Combine(recipePath, recipeName + ".Vision.json");
                            if (File.Exists(p2)) 
                                return p2;
                        }
                    }
                }

                // If only name provided, use default runner root directory structure
                if (!string.IsNullOrWhiteSpace(recipeName))
                {
                    string camFolder = Path.Combine(_opt.RecipeRootDirectory, _camera?.Name ?? "NoCamera");
                    string p = Path.Combine(camFolder, recipeName + ".Vision.json");
                    if (File.Exists(p))
                    {
                        // Also reflect chosen name for subsequent saves (optional)
                        _opt.RecipeName = recipeName;
                        return p;
                    }
                }
            }
            catch { }
            return null;
        }

        public void SetSearchMode(SearchMode mode)
        {
            _opt.Mode = mode;
            // 모드 변경 시 재표시
            RedrawLastOverlays();
        }
        public void SetShowMatchIndexes(bool show)
        {
            _opt.ShowMatchIndexes = show;
            RedrawLastOverlays();
        }
        public void SetHighlightReference(bool highlight)
        {
            _opt.HighlightReferenceMatch = highlight;
            RedrawLastOverlays();
        }
        private void RedrawLastOverlays()
        {
            if (_lastRawResult == null) return;
            UpdateViewerOverlays(_lastRawResult); // now iterates all viewers
        }

        public PatternMatchRunResult Search(bool save = false) => InternalSearchCore(null, save);
        public PatternMatchRunResult Search(VisionImage externalImage, bool save = false) => InternalSearchCore(externalImage, save);
        public Task<PatternMatchRunResult> SearchAsync(bool save = false, CancellationToken token = default) => Task.Run(() => InternalSearchCore(null, save, token), token);
        public Task<PatternMatchRunResult> SearchAsync(VisionImage image, bool save = false, CancellationToken token = default) => Task.Run(() => InternalSearchCore(image, save, token), token);

        public List<PatternMatchRunResult> BatchSearch(IEnumerable<VisionImage> images, bool saveEach)
        {
            var list = new List<PatternMatchRunResult>();
            if (images == null) return list;
            foreach (var img in images) list.Add(InternalSearchCore(img, saveEach));
            return list;
        }

        public Point GetLastPoint() => _lastPoint;
        public double GetLastAngle() => _lastAngle;
        public string GetLastFailReason() => _lastFailReason;
        public bool IsRecipeLoaded => _recipeLoaded;

        public (long Count, long Success, double AvgMs, double MinMs, double MaxMs, double RollingAvgMs, double SuccessRate) GetPerfSnapshot()
        {
            lock (_perfStats.LockObj)
            {
                double avg = _perfStats.Count == 0 ? 0 : _perfStats.TotalMs / _perfStats.Count;
                double roll = _perfStats.RollingQueue.Count == 0 ? 0 : _perfStats.RollingSumMs / _perfStats.RollingQueue.Count;
                double suc = _perfStats.Count == 0 ? 0 : (double)_perfStats.SuccessCount / _perfStats.Count;
                return (_perfStats.Count, _perfStats.SuccessCount, avg, _perfStats.MinMs == double.MaxValue ? 0 : _perfStats.MinMs, _perfStats.MaxMs, roll, suc);
            }
        }
        #endregion

        #region Core Search
        private PatternMatchRunResult InternalSearchCore(VisionImage externalImage, bool forceSave, CancellationToken ct = default)
        {
            var result = new PatternMatchRunResult();
            SemaphoreSlim global = s_globalSemaphore;
            var cameraSem = _cameraSemaphore;
            if (global != null) global.Wait(ct);
            cameraSem?.Wait(ct);
            try
            {
                lock (_sync)
                {
                    _sw.Restart();
                    try
                    {
                        if (!_recipeLoaded && !LoadRecipe())
                        {
                            result.Success = false; result.FailReason = _lastFailReason;
                            return FinalizeResult(result, externalImage, forceSave);
                        }
                        if (_parameters == null || _parameters.TrainImages == null || _parameters.TrainImages.Count == 0 ||
                            _parameters.TrainImages.All(ti => ti == null || ti.GetImage() == null))
                        {
                            result.Success = false; result.FailReason = "TrainImages 비어있음";
                            return FinalizeResult(result, externalImage, forceSave);
                        }
                        VisionImage src = externalImage ?? AcquireImage();
                        if (src == null || src.GetImage() == null)
                        {
                            result.Success = false; result.FailReason = "이미지 취득 실패";
                            return FinalizeResult(result, externalImage, forceSave);
                        }

                        int reTrained = 0;
                        if (_opt.RetrainAlways || IsTemplateModified())
                        {
                            reTrained = EnsureTemplatesTrained();
                            UpdateTemplateHash();
                        }
                        result.TemplateTrainedCount = reTrained;

                        Point searchStart = new Point(0, 0);
                        Point searchEnd = new Point(src.Header.Width - 1, src.Header.Height - 1);
                        if (_opt.UseInspectRoi)
                        {
                            try
                            {
                                var ispStart = _part.GetInspectStartPoint();
                                var ispEnd = _part.GetInspectEndPoint();
                                if (ispEnd.X > ispStart.X && ispEnd.Y > ispStart.Y)
                                {
                                    searchStart = ispStart;
                                    searchEnd = ispEnd;
                                }
                            }
                            catch { }
                        }
                        // VisionPart 가 내부에서 이미 절대좌표로 변환하므로 Runner 는 추가 처리 안 함
                        int ret = _part.OnSearch(searchStart, searchEnd, _parameters, null, src);
                        if (ret != 0)
                        {
                            result.Success = false; result.FailReason = "OnSearch 실패(ret=" + ret + ")";
                            return FinalizeResult(result, src, forceSave);
                        }
                        var raw = _part.GetResult();
                        if (raw == null || raw.Values == null || raw.Values.Count == 0)
                        {
                            result.Success = false; result.FailReason = "검색 결과 없음";
                            return FinalizeResult(result, src, forceSave);
                        }
                        // 절대좌표 그대로 사용
                        result.Matches = new List<PatternMatchingResult.PatternMatchingResultValue>(raw.Values);
                        PatternMatchingResult.PatternMatchingResultValue repr = raw.Values[0];
                        int refIdx = 0;

                        if (_opt.Mode == SearchMode.First && raw.Values.Count > 1 && src != null && src.Header != null)
                        {
                            double cx = src.Header.Width / 2.0;
                            double cy = src.Header.Height / 2.0;
                            double bestDist = double.MaxValue;
                            for (int i = 0; i < raw.Values.Count; i++)
                            {
                                var v = raw.Values[i];
                                double dx = v.X - cx; double dy = v.Y - cy; double d2 = dx * dx + dy * dy;
                                if (d2 < bestDist)
                                {
                                    bestDist = d2; repr = v; refIdx = i;
                                }
                            }
                            result.Matches = new List<PatternMatchingResult.PatternMatchingResultValue> { repr };
                        }
                        else if (_opt.Mode == SearchMode.All)
                        {
                            ComputeAverageExcludingExtremes(result);
                            // choose highest score as representative
                            double bestScore = double.MinValue;
                            for (int i = 0; i < raw.Values.Count; i++)
                            {
                                var v = raw.Values[i];
                                if (v.Score > bestScore)
                                {
                                    bestScore = v.Score; repr = v; refIdx = i;
                                }
                            }
                        }

                        result.ReferenceIndex = refIdx;
                        result.Success = true;
                        result.X = repr.X; result.Y = repr.Y; result.R = repr.R;
                        result.RawResult = raw;

                        _lastPoint = new Point((int)repr.X, (int)repr.Y);
                        _lastAngle = repr.R;
                        _lastReferenceIndex = refIdx;
                        _lastRawResult = raw; // cache

                        UpdateViewerOverlays(raw);
                        return FinalizeResult(result, src, forceSave);
                    }
                    catch (Exception ex)
                    {
                        result.Success = false;
                        result.FailReason = "예외: " + ex.Message;
                        if (_opt.ThrowOnSearchError) throw;
                        return FinalizeResult(result, externalImage, forceSave);
                    }
                }
            }
            finally
            {
                cameraSem?.Release();
                global?.Release();
            }
        }



        //private PatternMatchRunResult InternalSearchCore(VisionImage externalImage, bool forceSave, CancellationToken ct = default)
        //{
        //    var result = new PatternMatchRunResult();
        //    SemaphoreSlim global = s_globalSemaphore;
        //    var cameraSem = _cameraSemaphore;
        //    if (global != null) global.Wait(ct);
        //    cameraSem?.Wait(ct);
        //    try
        //    {
        //        lock (_sync)
        //        {
        //            _sw.Restart();
        //            try
        //            {
        //                if (!_recipeLoaded && !LoadRecipe())
        //                {
        //                    result.Success = false; result.FailReason = _lastFailReason; return FinalizeResult(result, externalImage, forceSave);
        //                }
        //                if (_parameters == null || _parameters.TrainImages == null || _parameters.TrainImages.Count == 0 || _parameters.TrainImages.All(ti => ti == null || ti.GetImage() == null))
        //                {
        //                    result.Success = false; result.FailReason = "TrainImages 비어있음"; return FinalizeResult(result, externalImage, forceSave);
        //                }
        //                VisionImage src = externalImage ?? AcquireImage();
        //                if (src == null || src.GetImage() == null)
        //                {
        //                    result.Success = false; result.FailReason = "이미지 취득 실패"; return FinalizeResult(result, externalImage, forceSave);
        //                }
        //                int reTrained = 0;
        //                if (_opt.RetrainAlways || IsTemplateModified())
        //                {
        //                    reTrained = EnsureTemplatesTrained();
        //                    UpdateTemplateHash();
        //                }
        //                result.TemplateTrainedCount = reTrained;
        //                Point searchStart = new Point(0, 0);
        //                Point searchEnd = new Point(src.Header.Width - 1, src.Header.Height - 1);
        //                if (_opt.UseInspectRoi)
        //                {
        //                    try
        //                    {
        //                        var ispStart = _part.GetInspectStartPoint();
        //                        var ispEnd = _part.GetInspectEndPoint();
        //                        if (ispEnd.X > ispStart.X && ispEnd.Y > ispStart.Y)
        //                        { searchStart = ispStart; searchEnd = ispEnd; }
        //                    }
        //                    catch { }
        //                }
        //                _lastRoiStart = searchStart; // ROI 시작 좌표 저장 (0,0 이면 전체)

        //                int ret = _part.OnSearch(searchStart, searchEnd, _parameters, null, src);
        //                if (ret != 0)
        //                {
        //                    result.Success = false; result.FailReason = "OnSearch 실패(ret=" + ret + ")"; return FinalizeResult(result, src, forceSave);
        //                }
        //                var raw = _part.GetResult();
        //                if (raw == null || raw.Values == null || raw.Values.Count == 0)
        //                {
        //                    result.Success = false; result.FailReason = "검색 결과 없음"; return FinalizeResult(result, src, forceSave);
        //                }


        //                // ROI Relative / Absolute 여부 추론: 결과 최소/최대가 ROI 폭/높이 범위 안이면 ROI 상대, 아니면 절대
        //                _lastValuesWereOffset = false;
        //                try
        //                {
        //                    if (_lastRoiStart != Point.Empty)
        //                    {
        //                        int roiW = Math.Max(1, searchEnd.X - searchStart.X + 1);
        //                        int roiH = Math.Max(1, searchEnd.Y - searchStart.Y + 1);
        //                        double minX = raw.Values.Min(v => v.X);
        //                        double maxX = raw.Values.Max(v => v.X);
        //                        double minY = raw.Values.Min(v => v.Y);
        //                        double maxY = raw.Values.Max(v => v.Y);
        //                        // 이미 (0~roiW),(0~roiH) 범위면 아직 offset 미적용 (ROI 상대)
        //                        bool looksRelative = minX >= -0.5 && minY >= -0.5 && maxX <= roiW + 0.5 && maxY <= roiH + 0.5;
        //                        _lastValuesWereOffset = !looksRelative; // relative가 아니면 이미 절대좌표
        //                    }
        //                    else
        //                    {
        //                        _lastValuesWereOffset = true; // ROI 사용 안하면 절대
        //                    }
        //                }
        //                catch { _lastValuesWereOffset = true; }

        //                // --- 기존 Heuristic ROI Offset 보정 블럭 유지 (단, 아직 offset 안된 것으로 판별된 경우만 수행) ---
        //                if (!_lastValuesWereOffset && _opt.UseInspectRoi && (searchStart.X != 0 || searchStart.Y != 0))
        //                {
        //                    try
        //                    {
        //                        for (int i = 0; i < raw.Values.Count; i++)
        //                        {
        //                            var v = raw.Values[i];
        //                            v.X += searchStart.X;
        //                            v.Y += searchStart.Y;
        //                            raw.Values[i] = v;
        //                        }
        //                        if (raw.ResultOverlays != null)
        //                        {
        //                            foreach (var ov in raw.ResultOverlays)
        //                            {
        //                                if (ov == null) continue;
        //                                try
        //                                {
        //                                    var t = ov.GetType();
        //                                    var sProp = t.GetProperty("StartLocation");
        //                                    var eProp = t.GetProperty("EndLocation");
        //                                    var cProp = t.GetProperty("CenterLocation");
        //                                    if (sProp != null && sProp.PropertyType == typeof(Point))
        //                                    {
        //                                        var p = (Point)sProp.GetValue(ov, null); p.Offset(searchStart); sProp.SetValue(ov, p, null);
        //                                    }
        //                                    if (eProp != null && eProp.PropertyType == typeof(Point))
        //                                    {
        //                                        var p = (Point)eProp.GetValue(ov, null); p.Offset(searchStart); eProp.SetValue(ov, p, null);
        //                                    }
        //                                    if (cProp != null && cProp.PropertyType == typeof(Point))
        //                                    {
        //                                        var p = (Point)cProp.GetValue(ov, null); p.Offset(searchStart); cProp.SetValue(ov, p, null);
        //                                    }
        //                                }
        //                                catch { }
        //                            }
        //                        }
        //                        _lastValuesWereOffset = true; // 이제 절대좌표
        //                    }
        //                    catch { }
        //                }
        //                // ------------------------------------------------------------


        //                result.Matches = new List<PatternMatchingResult.PatternMatchingResultValue>(raw.Values);
        //                PatternMatchingResult.PatternMatchingResultValue repr = raw.Values[0];
        //                int refIdx = 0;
        //                if (_opt.Mode == SearchMode.First && raw.Values.Count > 1 && src != null && src.Header != null)
        //                {
        //                    double cx = src.Header.Width / 2.0;
        //                    double cy = src.Header.Height / 2.0;
        //                    double bestDist = double.MaxValue;
        //                    for (int i = 0; i < raw.Values.Count; i++)
        //                    {
        //                        var v = raw.Values[i];
        //                        double dx = v.X - cx; double dy = v.Y - cy; double d2 = dx * dx + dy * dy;
        //                        if (d2 < bestDist)
        //                        {
        //                            bestDist = d2; repr = v; refIdx = i;
        //                        }
        //                    }
        //                    result.Matches = new List<PatternMatchingResult.PatternMatchingResultValue> { repr };
        //                }
        //                else if (_opt.Mode == SearchMode.All)
        //                {
        //                    ComputeAverageExcludingExtremes(result);
        //                    // choose highest score as representative
        //                    double bestScore = double.MinValue;
        //                    for (int i = 0; i < raw.Values.Count; i++)
        //                    {
        //                        var v = raw.Values[i];
        //                        if (v.Score > bestScore)
        //                        {
        //                            bestScore = v.Score; repr = v; refIdx = i;
        //                        }
        //                    }
        //                }
        //                result.ReferenceIndex = refIdx;
        //                result.Success = true; result.X = repr.X; result.Y = repr.Y; result.R = repr.R; result.RawResult = raw;
        //                _lastPoint = new Point((int)repr.X, (int)repr.Y); _lastAngle = repr.R;
        //                _lastReferenceIndex = refIdx;
        //                _lastRawResult = raw; // cache


        //                UpdateViewerOverlays(raw);
        //                return FinalizeResult(result, src, forceSave);
        //            }
        //            catch (Exception ex)
        //            {
        //                result.Success = false; result.FailReason = "예외: " + ex.Message; if (_opt.ThrowOnSearchError) throw; return FinalizeResult(result, externalImage, forceSave);
        //            }
        //        }
        //    }
        //    finally
        //    {
        //        cameraSem?.Release();
        //        global?.Release();
        //    }
        //}

        private void ComputeAverageExcludingExtremes(PatternMatchRunResult result)
        {
            var list = result.Matches; if (list == null || list.Count < 3) return;
            var max = list.Aggregate((a, b) => a.Score >= b.Score ? a : b);
            var min = list.Aggregate((a, b) => a.Score <= b.Score ? a : b);
            var middle = list.Where(v => !ReferenceEquals(v, max) && !ReferenceEquals(v, min)).ToList();
            if (middle.Count == 0) return;
            double sx = 0, sy = 0, sr = 0, ss = 0; foreach (var m in middle) { sx += m.X; sy += m.Y; sr += m.R; ss += m.Score; }
            int n = middle.Count; result.AvgXExcludingExtremes = sx / n; result.AvgYExcludingExtremes = sy / n; result.AvgRExcludingExtremes = sr / n; result.AvgScoreExcludingExtremes = ss / n;
        }
        #endregion

        #region Acquire Image
        private VisionImage AcquireImage()
        {
            VisionImage img = null;
            try
            {
                if (_camera.Opened && _camera.LatestImage?.RawData != null)
                    img = _camera.LatestImage;
                if (img == null && _camera.Opened)
                    _camera.GrabSync(out img);
            }
            catch (Exception ex)
            {
                _lastFailReason = "Grab 실패: " + ex.Message;
            }
            return img;
        }
        #endregion

        #region Training Hash / Training
        private bool IsTemplateModified()
        {
            if (_parameters?.TrainImages == null) return false;
            var sb = new StringBuilder();
            foreach (var vi in _parameters.TrainImages)
            {
                if (vi == null || vi.GetImage() == null) { sb.Append("NULL;"); continue; }
                try { var img = vi.GetImage(); sb.Append(img.Width).Append('x').Append(img.Height).Append(';'); } catch { sb.Append("ERR;"); }
            }
            return ComputeSha1(sb.ToString()) != _lastTemplateHash;
        }
        private void UpdateTemplateHash()
        {
            if (_parameters?.TrainImages == null) { _lastTemplateHash = string.Empty; return; }
            var sb = new StringBuilder();
            foreach (var vi in _parameters.TrainImages)
            {
                if (vi == null || vi.GetImage() == null) { sb.Append("NULL;"); continue; }
                try { var img = vi.GetImage(); sb.Append(img.Width).Append('x').Append(img.Height).Append(';'); } catch { sb.Append("ERR;"); }
            }
            _lastTemplateHash = ComputeSha1(sb.ToString());
        }
        private string ComputeSha1(string text)
        {
            using (var sha1 = SHA1.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(text ?? string.Empty);
                return BitConverter.ToString(sha1.ComputeHash(bytes)).Replace("-", "");
            }
        }
        private int EnsureTemplatesTrained()
        {
            int count = 0; if (_parameters?.TrainImages == null) return 0;
            for (int i = 0; i < _parameters.TrainImages.Count; i++)
            {
                var t = _parameters.TrainImages[i]; if (t == null || t.GetImage() == null) continue;
                try { _part.OnTrain(new Point(0, 0), new Point(t.Header.Width - 1, t.Header.Height - 1), _parameters, null, i); count++; } catch { }
            }
            return count;
        }
        #endregion

        #region Finalize / Save / Perf
        private PatternMatchRunResult FinalizeResult(PatternMatchRunResult r, VisionImage src, bool forceSave)
        {
            _sw.Stop();
            r.Elapsed = _sw.Elapsed;
            _lastFailReason = r.Success ? null : r.FailReason;

            // Performance update
            if (_opt.EnablePerformanceLog)
            {
                lock (_perfStats.LockObj)
                {
                    _perfStats.Update(r.Elapsed.TotalMilliseconds, r.Success, r.Matches?.Count ?? 0, r);
                    TryFlushPerfLocked();
                }
            }

            bool needSave = forceSave || ShouldSave(r.Success);
            if (needSave && _opt.EnableSaveImage && src != null && src.GetImage() != null)
            {
                try { SaveResultArtifacts(r, src); } catch (Exception ex) { Log.Write("PatternMatchingRunner", "Save 실패: " + ex.Message); }
            }
            try
            {
                AfterSearch?.Invoke(r);
                if (r.Success) SearchSucceeded?.Invoke(r); else SearchFailed?.Invoke(r);
            }
            catch { }
            return r;
        }

        private bool ShouldSave(bool success)
        {
            switch (_opt.ImageSaveMode)
            {
                case SaveMode.None: return false;
                case SaveMode.All: return true;
                case SaveMode.OkOnly: return success;
                case SaveMode.NgOnly: return !success;
                default: return false;
            }
        }

        private void PrepareSaveDirectories()
        {
            if (!_opt.EnableSaveImage && !_opt.EnablePerformanceLog) return;
            try
            {
                if (_opt.EnableSaveImage)
                {
                    _dirOK = Path.Combine(_opt.SaveRootDirectory, "OK");
                    _dirNG = Path.Combine(_opt.SaveRootDirectory, "NG");
                    _dirRaw = Path.Combine(_opt.SaveRootDirectory, "RAW");
                    Directory.CreateDirectory(_dirOK);
                    Directory.CreateDirectory(_dirNG);
                    Directory.CreateDirectory(_dirRaw);
                }
                if (_opt.EnablePerformanceLog)
                {
                    Directory.CreateDirectory(_opt.PerformanceLogDirectory);
                }
            }
            catch (Exception ex)
            {
                Log.Write("PatternMatchingRunner", "디렉터리 생성 실패: " + ex.Message);
            }
        }

        private void SaveResultArtifacts(PatternMatchRunResult r, VisionImage src)
        {
            if (src == null || src.GetImage() == null) return;
            string dir = r.Success ? _dirOK : _dirNG; if (string.IsNullOrEmpty(dir)) return;
            string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
            string baseName = string.Format("{0}_{1}_{2}_{3:0.##}_{4:0.##}", _opt.FileNamePrefix, stamp, r.Success ? "OK" : "NG", r.X, r.Y);
            string overlayPath = Path.Combine(dir, baseName + ".png");
            using (var bmp = new Bitmap(src.GetImage()))
            using (var g = Graphics.FromImage(bmp))
            {
                if (r.Success && _lastPoint != Point.Empty && _opt.DrawCrossOnViewer) DrawCrossRaw(g, _lastPoint);
                if (_opt.SaveOverlay && r.RawResult != null) DrawOverlays(g, r.RawResult);
                // custom annotations (indexes, highlight)
                DrawAnnotations(g, r);
                g.Flush(); bmp.Save(overlayPath, ImageFormat.Png);
            }
            r.SavedImagePath = overlayPath;
            if (_opt.SaveRawAlso)
            {
                string rawPath = Path.Combine(_dirRaw, baseName + "_raw.png");
                try { src.GetImage().Save(rawPath, ImageFormat.Png); r.SavedRawImagePath = rawPath; } catch { }
            }
            if (_opt.SaveJson)
            {
                string jsonPath = Path.Combine(dir, baseName + ".json");
                try
                {
                    var json = new
                    {
                        r.Success,
                        r.X,
                        r.Y,
                        r.R,
                        r.FailReason,
                        r.Elapsed.TotalMilliseconds,
                        r.TemplateTrainedCount,
                        r.AvgXExcludingExtremes,
                        r.AvgYExcludingExtremes,
                        r.AvgRExcludingExtremes,
                        r.AvgScoreExcludingExtremes,
                        r.ReferenceIndex,
                        MatchCount = r.Matches?.Count,
                        Time = r.Timestamp.ToString("O")
                    };
                    File.WriteAllText(jsonPath, Newtonsoft.Json.JsonConvert.SerializeObject(json, Newtonsoft.Json.Formatting.Indented), Encoding.UTF8);
                    r.SavedJsonPath = jsonPath;
                }
                catch { }
            }
        }
        #endregion

        #region Performance Log Helpers
        private void InitPerfLog()
        {
            try
            {
                string suffix = _opt.AppendProcessIdToLog ? ("_" + System.Diagnostics.Process.GetCurrentProcess().Id) : string.Empty;
                string file = string.Format("Perf_{0}_{1}{2}.csv", _camera.Name, _opt.RecipeName, suffix);
                string path = Path.Combine(_opt.PerformanceLogDirectory, file);
                _perfStats.CsvPath = path;
                if (!File.Exists(path))
                {
                    File.AppendAllLines(path, new[] { "Time,Result,Matches,ElapsedMs,AvgMs,RollingAvgMs,MinMs,MaxMs,SuccessRate(%),AvgScoreExExt,FailReason" });
                }
            }
            catch (Exception ex)
            {
                Log.Write("PatternMatchingRunner", "PerfLog init 실패: " + ex.Message);
                _opt.EnablePerformanceLog = false; // disable to avoid repeated errors
            }
        }

        private void TryFlushPerfLocked()
        {
            if (!_opt.EnablePerformanceLog) return;
            if (_perfStats.PendingLines.Count >= _opt.PerfFlushEvery ||
                _perfStats.PendingLines.Count >= _opt.PerfMaxBufferedLines ||
                (DateTime.Now - _perfStats.LastFlush).TotalSeconds > 10)
            {
                try
                {
                    File.AppendAllLines(_perfStats.CsvPath, _perfStats.PendingLines.ToArray(), Encoding.UTF8);
                    _perfStats.PendingLines.Clear();
                    _perfStats.LastFlush = DateTime.Now;
                }
                catch (Exception ex)
                {
                    Log.Write("PatternMatchingRunner", "PerfLog flush 실패: " + ex.Message);
                }
            }
        }
        #endregion

        #region Viewer & Drawing
        private void BuildCrossPen()
        {
            _crossPen?.Dispose();
            _crossPen = new Pen(_opt.CrossColor, _opt.CrossPenWidth);
        }

        /// <summary>
        /// Register an extra VisionImageViewer to show search overlays and annotations.
        /// </summary>
        public void RegisterViewer(VisionImageViewer viewer, ViewerDisplayOptions options = null)
        {
            if (viewer == null) return;
            lock (_viewersLock)
            {
                if (_viewers.Add(viewer))
                {
                    _viewerOptions[viewer] = options ?? new ViewerDisplayOptions();
                    viewer.Paint -= Viewer_PaintMulti; // ensure single subscription
                    viewer.Paint += Viewer_PaintMulti;
                }
                else if (options != null)
                {
                    _viewerOptions[viewer] = options; // update
                }
            }
            // immediate redraw if we already have last result
            if (_lastRawResult != null)
                UpdateViewerOverlays(_lastRawResult, viewerOnly: viewer);
        }
        /// <summary>
        /// Unregister viewer. Overlays will be cleared once (does not dispose viewer).
        /// </summary>
        public void UnregisterViewer(VisionImageViewer viewer, bool clearOverlays = true)
        {
            if (viewer == null) return;
            lock (_viewersLock)
            {
                if (_viewers.Remove(viewer))
                {
                    _viewerOptions.Remove(viewer);
                    viewer.Paint -= Viewer_PaintMulti;
                    if (clearOverlays)
                    {
                        try { viewer.ResultOverlays?.Clear(); viewer.Invalidate(); } catch { }
                    }
                }
            }
        }
        /// <summary>
        /// Get read-only snapshot of currently registered viewers.
        /// </summary>
        public IReadOnlyCollection<VisionImageViewer> GetRegisteredViewers()
        {
            lock (_viewersLock)
                return new ReadOnlyCollection<VisionImageViewer>(_viewers.ToList());
        }
        /// <summary>
        /// Update per-viewer display options.
        /// </summary>
        public void SetViewerDisplayOptions(VisionImageViewer viewer, ViewerDisplayOptions options)
        {
            if (viewer == null || options == null) return;
            lock (_viewersLock)
            {
                if (_viewers.Contains(viewer))
                    _viewerOptions[viewer] = options;
            }
            viewer.Invalidate();
        }
        private bool GetEffective(ViewerDisplayOptions opt, Func<RunnerOptions, bool> globalSelector, Func<ViewerDisplayOptions, bool?> localSelector)
        {
            if (opt == null) return globalSelector(_opt);
            var v = localSelector(opt);
            return v.HasValue ? v.Value : globalSelector(_opt);
        }
        private void Viewer_PaintMulti(object sender, PaintEventArgs e)
        {
            var viewer = sender as VisionImageViewer;
            if (viewer == null) return;
            // If viewer was unregistered silently skip
            lock (_viewersLock)
            {
                if (!_viewers.Contains(viewer)) return;
            }
            var img = viewer.Image; if (img == null) return;
            try
            {
                int imgW = img.Width, imgH = img.Height, boxW = viewer.ClientSize.Width, boxH = viewer.ClientSize.Height;
                double scale = Math.Min((double)boxW / imgW, (double)boxH / imgH);
                int drawW = (int)(imgW * scale), drawH = (int)(imgH * scale);
                int offX = (boxW - drawW) / 2, offY = (boxH - drawH) / 2;
                ViewerDisplayOptions localOpt;
                lock (_viewersLock) _viewerOptions.TryGetValue(viewer, out localOpt);
                bool drawCross = GetEffective(localOpt, o => o.DrawCrossOnViewer, o => o.DrawCrossOnViewer);
                bool showIdx = GetEffective(localOpt, o => o.ShowMatchIndexes, o => o.ShowMatchIndexes);
                bool highlightRef = GetEffective(localOpt, o => o.HighlightReferenceMatch, o => o.HighlightReference);
                if (drawCross && _lastPoint != Point.Empty)
                {
                    int cx = offX + (int)(_lastPoint.X * scale);
                    int cy = offY + (int)(_lastPoint.Y * scale);
                    int len = _opt.CrossHalfLength;
                    e.Graphics.DrawLine(_crossPen, cx - len, cy, cx + len, cy);
                    e.Graphics.DrawLine(_crossPen, cx, cy - len, cx, cy + len);
                }
                if (_lastRawResult != null && _lastRawResult.Values != null)
                {
                    if (showIdx)
                    {
                        int smallLen = Math.Max(3, _opt.CrossHalfLength / 2);
                        using (var smallPen = new Pen(_opt.IndexTextColor, 1))
                        using (var b = new SolidBrush(_opt.IndexTextColor))
                        {
                            for (int i = 0; i < _lastRawResult.Values.Count; i++)
                            {
                                var v = _lastRawResult.Values[i];
                                int px = offX + (int)(v.X * scale);
                                int py = offY + (int)(v.Y * scale);
                                e.Graphics.DrawLine(smallPen, px - smallLen, py, px + smallLen, py);
                                e.Graphics.DrawLine(smallPen, px, py - smallLen, px, py + smallLen);
                                string text = i.ToString();
                                var sz = e.Graphics.MeasureString(text, _indexFont);
                                e.Graphics.DrawString(text, _indexFont, b, px - sz.Width / 2f, py - sz.Height / 2f - smallLen - 2);
                            }
                        }
                    }
                    if (highlightRef && _lastReferenceIndex >= 0 && _lastReferenceIndex < _lastRawResult.Values.Count)
                    {
                        var v = _lastRawResult.Values[_lastReferenceIndex];
                        int px = offX + (int)(v.X * scale);
                        int py = offY + (int)(v.Y * scale);
                        int rad = Math.Max(5, _opt.ReferenceMarkRadius);
                        using (var pen = new Pen(_opt.ReferenceMarkColor, 2))
                        {
                            e.Graphics.DrawEllipse(pen, px - rad, py - rad, rad * 2, rad * 2);
                        }
                    }
                }
            }
            catch { }
        }
        private void DrawCrossRaw(Graphics g, Point center)
        {
            try
            {
                int len = _opt.CrossHalfLength;
                g.DrawLine(_crossPen, center.X - len, center.Y, center.X + len, center.Y);
                g.DrawLine(_crossPen, center.X, center.Y - len, center.X, center.Y + len);
            }
            catch { }
        }
        private void DrawOverlays(Graphics g, PatternMatchingResult raw)
        {
            if (raw == null || raw.ResultOverlays == null) return;
            using (var pen = new Pen(_opt.OverlayPenColor, _opt.OverlayPenWidth))
            {
                foreach (var ov in raw.ResultOverlays)
                {
                    if (ov == null) continue;
                    try
                    {
                        var type = ov.GetType();
                        var sProp = type.GetProperty("StartLocation");
                        var eProp = type.GetProperty("EndLocation");
                        if (sProp != null && eProp != null)
                        {
                            var s = (Point)sProp.GetValue(ov, null);
                            var e2 = (Point)eProp.GetValue(ov, null);
                            var rect = Rectangle.FromLTRB(Math.Min(s.X, e2.X), Math.Min(s.Y, e2.Y), Math.Max(s.X, e2.X), Math.Max(s.Y, e2.Y));
                            g.DrawRectangle(pen, rect);
                        }
                    }
                    catch { }
                }
            }
        }
        private void DrawAnnotations(Graphics g, PatternMatchRunResult r)
        {
            if (r == null || r.Matches == null) return;
            if (_opt.ShowMatchIndexes)
            {
                for (int i = 0; i < r.Matches.Count; i++)
                {
                    var m = r.Matches[i];
                    string text = i.ToString();
                    var sz = g.MeasureString(text, _indexFont);
                    float x = (float)m.X - sz.Width / 2f;
                    float y = (float)m.Y - sz.Height / 2f;
                    using (var b = new SolidBrush(_opt.IndexTextColor))
                        g.DrawString(text, _indexFont, b, x, y);
                }
            }
            if (_opt.HighlightReferenceMatch && r.ReferenceIndex >= 0 && r.ReferenceIndex < r.Matches.Count)
            {
                var m = r.Matches[r.ReferenceIndex];
                int rad = Math.Max(5, _opt.ReferenceMarkRadius);
                using (var pen = new Pen(_opt.ReferenceMarkColor, 2))
                {
                    g.DrawEllipse(pen, (float)m.X - rad, (float)m.Y - rad, rad * 2, rad * 2);
                }
            }
        }

        private void UpdateViewerOverlays(PatternMatchingResult raw, VisionImageViewer viewerOnly = null)
        {
            if (raw == null) return;
            List<VisionImageViewer> targets;
            lock (_viewersLock)
            {
                if (viewerOnly != null)
                    targets = _viewers.Contains(viewerOnly) ? new List<VisionImageViewer> { viewerOnly } : new List<VisionImageViewer>();
                else
                    targets = _viewers.ToList();
            }

            foreach (var vw in targets)
            {
                try
                {
                    vw.ResultOverlays?.Clear();

                    // 1) Tool에서 생성한 Overlay 복사
                    foreach (var ov in raw.ResultOverlays)
                    {
                        if (ov == null) continue;
                        ov.Visible = true;
                        vw.ResultOverlays.Add(ov);
                    }

                    // 2) Cross / Index 표시 (Values는 항상 절대좌표로 확정됨)
                    if (_lastRawResult != null && _lastRawResult.Values != null && _lastRawResult.Values.Count > 0)
                    {
                        int repIdx = (_lastReferenceIndex >= 0 && _lastReferenceIndex < _lastRawResult.Values.Count) ? _lastReferenceIndex : 0;
                        int crossLenBase = Math.Max(2, _opt.CrossHalfLength);

                        for (int i = 0; i < _lastRawResult.Values.Count; i++)
                        {
                            var v = _lastRawResult.Values[i];
                            double absX = v.X;
                            double absY = v.Y;

                            bool isRep = (i == repIdx);
                            int len = isRep ? (int)(crossLenBase * 1.4) : crossLenBase;
                            int thickness = isRep ? 2 : 1;
                            Color crossColor = _opt.CrossColor;

                            // 안해도됨. (지우지는마)
                            //try
                            //{
                            //    var h = new LineFrameVisionImageOverlay($"PM_M{i}_H")
                            //    {
                            //        StartLocation = new Point((int)Math.Round(absX) - len, (int)Math.Round(absY)),
                            //        EndLocation = new Point((int)Math.Round(absX) + len, (int)Math.Round(absY)),
                            //        Color = crossColor,
                            //        DashStyle = DashStyle.Solid,
                            //        Thickness = thickness,
                            //        Visible = true
                            //    };
                            //    vw.ResultOverlays.Add(h);
                            //}
                            //catch { }

                            //try
                            //{
                            //    var vert = new LineFrameVisionImageOverlay($"PM_M{i}_V")
                            //    {
                            //        StartLocation = new Point((int)Math.Round(absX), (int)Math.Round(absY) - len),
                            //        EndLocation = new Point((int)Math.Round(absX), (int)Math.Round(absY) + len),
                            //        Color = crossColor,
                            //        DashStyle = DashStyle.Solid,
                            //        Thickness = thickness,
                            //        Visible = true
                            //    };
                            //    vw.ResultOverlays.Add(vert);
                            //}
                            //catch { }

                            if (_opt.ShowMatchIndexes)
                            {
                                try
                                {
                                    // 기본 위치 (우측상단) 계산
                                    int tx = (int)Math.Round(absX) + (len + 3);
                                    int ty = (int)Math.Round(absY) - (len + 3);

                                    // 이미지 경계 안으로 클램프 (viewer 이미지 기준 절대좌표)
                                    // raw.ResultOverlays 는 원본 이미지 좌표계이므로 간단히 0 이상만 보정
                                    if (ty < 0) ty = (int)Math.Round(absY) + (len + 3); // 위로 벗어나면 아래쪽으로 배치
                                    if (tx < 0) tx = (int)Math.Round(absX) + 2;         // 왼쪽으로 벗어나면 살짝 오른쪽

                                    var startPt = new Point(tx, ty);
                                    var txtOv = new TextVisionImageOverlay($"PM_M{i}_IDX", startPt)
                                    {
                                        FontStyle = _indexFont,
                                        BrushColor = new SolidBrush(_opt.IndexTextColor),
                                        Text = i.ToString(),
                                        Color = _opt.IndexTextColor,
                                        Visible = true
                                    };
                                    vw.ResultOverlays.Add(txtOv);
                                }
                                catch { }
                            }
                        }

                        if (_opt.HighlightReferenceMatch && repIdx >= 0 && repIdx < _lastRawResult.Values.Count)
                        {
                            try
                            {
                                var rep = _lastRawResult.Values[repIdx];
                                double absX = rep.X;
                                double absY = rep.Y;

                                int rad = Math.Max(5, _opt.ReferenceMarkRadius);
                                var rectOv = new RectangleFrameVisionImageOverlay("PM_REF_RING")
                                {
                                    StartLocation = new Point((int)Math.Round(absX) - rad, (int)Math.Round(absY) - rad),
                                    EndLocation = new Point((int)Math.Round(absX) + rad, (int)Math.Round(absY) + rad),
                                    Color = _opt.ReferenceMarkColor,
                                    DashStyle = DashStyle.Dash,
                                    Thickness = 2,
                                    Visible = true
                                };
                                vw.ResultOverlays.Add(rectOv);
                            }
                            catch { }
                        }
                    }

                    vw.Invalidate();
                }
                catch { }
            }
        }


        //private void UpdateViewerOverlays(PatternMatchingResult raw, VisionImageViewer viewerOnly = null)
        //{
        //    if (raw == null) return;
        //    List<VisionImageViewer> targets;
        //    lock (_viewersLock)
        //    {
        //        if (viewerOnly != null)
        //            targets = _viewers.Contains(viewerOnly) ? new List<VisionImageViewer> { viewerOnly } : new List<VisionImageViewer>();
        //        else
        //            targets = _viewers.ToList();
        //    }
        //    foreach (var vw in targets)
        //    {
        //        try
        //        {
        //            vw.ResultOverlays?.Clear();
        //            1) 원본 툴에서 생성한 Frame 계열 오버레이 복사
        //            foreach (var ov in raw.ResultOverlays)
        //            {
        //                if (ov == null) continue;
        //                ov.Visible = true;
        //                vw.ResultOverlays.Add(ov);
        //            }

        //            2) 매치 결과 Cross / Index / 대표 강조 추가
        //            if (_lastRawResult != null && _lastRawResult.Values != null && _lastRawResult.Values.Count > 0)
        //            {
        //                int repIdx = (_lastReferenceIndex >= 0 && _lastReferenceIndex < _lastRawResult.Values.Count) ? _lastReferenceIndex : 0;
        //                int crossLenBase = Math.Max(2, _opt.CrossHalfLength);
        //                bool roiUsed = _lastRoiStart != Point.Empty;
        //                우리가 그릴 좌표는 항상 "절대좌표" 기준이어야 함. 만약 raw 값이 아직 offset 안된(relative) 상태였다면 ROI 시작을 더해 절대로 변환.
        //                for (int i = 0; i < _lastRawResult.Values.Count; i++)
        //                {
        //                    var v = _lastRawResult.Values[i];
        //                    double absX = v.X;
        //                    double absY = v.Y;

        //                    if (roiUsed && !_lastValuesWereOffset) // relative -> absolute 변환
        //                    {
        //                        absX += _lastRoiStart.X;
        //                        absY += _lastRoiStart.Y;
        //                    }

        //                    if (roiUsed)
        //                    {
        //                        absX -= _lastRoiStart.X;
        //                        absY -= _lastRoiStart.Y;
        //                    }

        //                    bool isRep = (i == repIdx);
        //                    int len = isRep ? (int)(crossLenBase * 1.4) : crossLenBase;
        //                    int thickness = isRep ? 2 : 1;
        //                    Color crossColor = _opt.CrossColor;
        //                    try
        //                    {
        //                        var h = new LineFrameVisionImageOverlay($"PM_M{i}_H")
        //                        {
        //                            StartLocation = new Point((int)Math.Round(absX) - len, (int)Math.Round(absY)),
        //                            EndLocation = new Point((int)Math.Round(absX) + len, (int)Math.Round(absY)),
        //                            Color = crossColor,
        //                            DashStyle = DashStyle.Solid,
        //                            Thickness = thickness,
        //                            Visible = true
        //                        };
        //                        vw.ResultOverlays.Add(h);
        //                    }
        //                    catch { }
        //                    try
        //                    {
        //                        var vert = new LineFrameVisionImageOverlay($"PM_M{i}_V")
        //                        {
        //                            StartLocation = new Point((int)Math.Round(absX), (int)Math.Round(absY) - len),
        //                            EndLocation = new Point((int)Math.Round(absX), (int)Math.Round(absY) + len),
        //                            Color = crossColor,
        //                            DashStyle = DashStyle.Solid,
        //                            Thickness = thickness,
        //                            Visible = true
        //                        };
        //                        vw.ResultOverlays.Add(vert);
        //                    }
        //                    catch { }
        //                    if (_opt.ShowMatchIndexes)
        //                    {
        //                        try
        //                        {
        //                            var txtOv = new TextVisionImageOverlay($"PM_M{i}_IDX")
        //                            {
        //                                FontStyle = _indexFont,
        //                                BrushColor = new SolidBrush(_opt.IndexTextColor),
        //                                StartLocation = new Point((int)Math.Round(absX) + (len + 3), (int)Math.Round(absY) - (len + 3)),
        //                                Text = i.ToString(),
        //                                Visible = true
        //                            };
        //                            vw.ResultOverlays.Add(txtOv);
        //                        }
        //                        catch { }
        //                    }
        //                }
        //                if (_opt.HighlightReferenceMatch && repIdx >= 0 && repIdx < _lastRawResult.Values.Count)
        //                {
        //                    try
        //                    {
        //                        var rep = _lastRawResult.Values[repIdx];
        //                        double absX = rep.X;
        //                        double absY = rep.Y;
        //                        if (roiUsed && !_lastValuesWereOffset)
        //                        {
        //                            absX += _lastRoiStart.X;
        //                            absY += _lastRoiStart.Y;
        //                        }
        //                        if (roiUsed)
        //                        {
        //                            absX -= _lastRoiStart.X;
        //                            absY -= _lastRoiStart.Y;
        //                        }

        //                        int rad = Math.Max(5, _opt.ReferenceMarkRadius);
        //                        var rectOv = new RectangleFrameVisionImageOverlay("PM_REF_RING")
        //                        {
        //                            StartLocation = new Point((int)Math.Round(absX) - rad, (int)Math.Round(absY) - rad),
        //                            EndLocation = new Point((int)Math.Round(absX) + rad, (int)Math.Round(absY) + rad),
        //                            Color = _opt.ReferenceMarkColor,
        //                            DashStyle = DashStyle.Dash,
        //                            Thickness = 2,
        //                            Visible = true
        //                        };
        //                        vw.ResultOverlays.Add(rectOv);
        //                    }
        //                    catch { }
        //                }
        //            }

        //            vw.Invalidate();
        //        }
        //        catch { }
        //    }
        //}
        #endregion

        #region Dispose
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            try
            {
                List<VisionImageViewer> viewers;
                lock (_viewersLock) viewers = _viewers.ToList();
                foreach (var v in viewers)
                {
                    try { v.Paint -= Viewer_PaintMulti; } catch { }
                }
            }
            catch { }
            try { _crossPen?.Dispose(); } catch { }
            try { _indexFont?.Dispose(); } catch { }
        }
        #endregion

        #region Internal Temp VisionPart
        private class TempVisionPart : MultiPatternMatchingVisionPart
        {
            private MultiPatternMatchingParameters _params = new MultiPatternMatchingParameters();
            public TempVisionPart(string name) : base(name) { Simulated = false; }
            public override MultiPatternMatchingParameters GetPatternMatchingParameters() => _params;
            public override void SetPatternMatchingParameters(MultiPatternMatchingParameters parameters)
            {
                _params = parameters ?? new MultiPatternMatchingParameters();
            }
            public override IlluminationDataSet GetIlluminationDataSet() => null;
        }
        #endregion
    }
}
