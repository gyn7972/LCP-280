using QMC.Common;
using QMC.Common.Cameras;
using QMC.Common.Vision;
using QMC.Common.Vision.Tools;
using QMC.Common.VisionPart;
using QMC.LCP_280.Process.Component; // added for MeasurementRecipe & RecipeManager
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
using File = System.IO.File;

namespace QMC.LCP_280.Process
{
    public class PatternMatchingRunner : IDisposable
    {
        #region Enums / Options
        public enum SaveMode { None, OkOnly, NgOnly, All }
        public enum SearchMode { First, All }
        public enum ProcessMode
        {
            Prealign,      // 1개(센터 우선)
            MapMatching,   // 전체
            SecondAlign    // 전체
        }

        // [ADD] GUI 제어용 ROI 모드
        public enum RoiSourceMode
        {
            Recipe,     // 레시피 ROI 사용
            Profile     // 모드별 ROI 사용
        }

        // [ADD] GUI 제어용 실행 프로파일
        public enum RunnerProfileMode
        {
            Normal,
            Recheck
        }

        public class ModeRuntimeOptions
        {
            public SearchMode SearchMode;
            public bool PreferCenterMostMatch;
            public RoiSourceMode RoiSourceMode;
            public RunnerProfileMode ProfileMode;
        }

        public class RunnerOptions
        {
            // Recipe / Search
            public bool AutoLoadRecipe = true;
            public string RecipeRootDirectory;
            public string RecipeName = "Default";
            public bool RetrainAlways = false;
            public bool UseInspectRoi = true;
            public SearchMode Mode = SearchMode.First;

            // [ADD]
            public RoiSourceMode InspectRoiSourceMode = RoiSourceMode.Recipe;
            public RunnerProfileMode ProfileMode = RunnerProfileMode.Normal;

            // [ADD] 대표 매치 선택 정책(기본값 유지: 기존 동작)
            public bool PreferCenterMostMatch = false;

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
        
        public class ViewerDisplayOptions
        {
            public bool? DrawCrossOnViewer;   // null -> use global
            public bool? ShowMatchIndexes;    // null -> use global
            // [CHG] 기존 HighlightReference -> HighlightReferenceMatch 별칭 추가
            // PatternMatchingControl에서 사용한 HighlightReferenceMatch 와 컴파일 호환을 맞춘다.
            public bool? HighlightReference;  // null -> use global

            public bool? HighlightReferenceMatch
            {
                get { return HighlightReference; }
                set { HighlightReference = value; }
            }
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

        private class ProfileRoiSetting
        {
            public bool Enabled;
            public Point InspectStart;
            public Point InspectEnd;
        }

        private RunnerProfileMode _profileMode;
        private readonly Dictionary<RunnerProfileMode, ProfileRoiSetting> _profileRoiSettings
            = new Dictionary<RunnerProfileMode, ProfileRoiSetting>();


        private ProcessMode _processMode = ProcessMode.Prealign;
        private readonly Dictionary<ProcessMode, ModeRuntimeOptions> _modeOptions
            = new Dictionary<ProcessMode, ModeRuntimeOptions>
        {
            { ProcessMode.Prealign,  new ModeRuntimeOptions 
                { SearchMode = SearchMode.First, 
                PreferCenterMostMatch = true,  
                RoiSourceMode = RoiSourceMode.Profile, 
                ProfileMode = RunnerProfileMode.Normal } 
                },
            { ProcessMode.MapMatching,new ModeRuntimeOptions 
                { SearchMode = SearchMode.All,   
                PreferCenterMostMatch = false, 
                RoiSourceMode = RoiSourceMode.Profile, 
                ProfileMode = RunnerProfileMode.Normal } 
                },
            { ProcessMode.SecondAlign,new ModeRuntimeOptions 
                { SearchMode = SearchMode.All,   
                PreferCenterMostMatch = false, 
                RoiSourceMode = RoiSourceMode.Profile, 
                ProfileMode = RunnerProfileMode.Recheck } 
                },
        };

        #endregion

        public MultiPatternMatchingParameters Parameters
        {
            get
            {
                // 잠금은 필요 최소화 (읽기 용도)
                lock (_sync)
                    return _parameters;
            }
        }

        /// <summary>
        /// 첫 번째 학습(Train) 이미지의 Width/Height 반환 (없으면 0,0)
        /// </summary>
        public (int width, int height) GetFirstTrainImageSize()
        {
            try
            {
                var ti = _parameters?.TrainImages?
                    .FirstOrDefault(t => t != null && t.Header != null && t.Header.Width > 0 && t.Header.Height > 0);
                if (ti == null) return (0, 0);
                return (ti.Header.Width, ti.Header.Height);
            }
            catch { return (0, 0); }
        }




        #region Ctor
        public PatternMatchingRunner(Camera camera, VisionImageViewer viewer, RunnerOptions options)
        {
            _camera = camera ?? throw new ArgumentNullException(nameof(camera));
            _primaryViewer = viewer; // store
            _opt = options ?? throw new ArgumentNullException(nameof(options));
            _viewer = viewer; // legacy field reference (kept for minimal change)

            // PatternMatchingRunner(...) 생성자 내부, _viewer 할당 직후 추가
            _profileMode = _opt.ProfileMode;
            _profileRoiSettings[RunnerProfileMode.Normal] = new ProfileRoiSetting { Enabled = false };
            _profileRoiSettings[RunnerProfileMode.Recheck] = new ProfileRoiSetting { Enabled = false };

            if (string.IsNullOrWhiteSpace(_opt.RecipeRootDirectory))
            {
                _opt.RecipeRootDirectory = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Recipes",
                    PatternMatchingRecipeStore.NormalizeRecipeBaseName(_opt.RecipeName),
                    "PatternMatching");
            }

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

            // [ADD] 어떤 입력(ABC, ABC_Vision, ABC_Vision.json, ABC.Vision.json)도 "레시피명"으로 통일
            try
            {
                _opt.RecipeName = PatternMatchingRecipeStore.NormalizeRecipeName(_opt.RecipeName);
            }
            catch { }
        }
        #endregion

        #region Public API

        public PatternMatchingRoiJson _Roi { get; set; }

        public void SetRecipe(string recipeName, string recipeRootDirectory = null)
        {
            lock (_sync)
            {
                // 1) normalize
                string normalized;
                try { normalized = PatternMatchingRecipeStore.NormalizeRecipeName(recipeName); }
                catch { normalized = string.IsNullOrWhiteSpace(recipeName) ? "Default" : recipeName.Trim(); }

                _opt.RecipeName = normalized;

                // 2) root 정책 강제: D:\LCP-280\Recipes\{RecipeName}\PatternMatching
                if (string.IsNullOrWhiteSpace(recipeRootDirectory))
                {
                    _opt.RecipeRootDirectory = Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        "Recipes",
                        normalized,
                        "PatternMatching");
                }
                else
                {
                    _opt.RecipeRootDirectory = recipeRootDirectory;
                }

                // 3) recipe 상태 reset (다음 LoadRecipe에서 다시 읽도록)
                _recipeLoaded = false;
                _lastFailReason = null;
            }
        }


        public bool LoadRecipe()
        {
            lock (_sync)
            {
                try
                {
                    // 1) RecipeName 항상 최신 정규화
                    string normalizedRecipe;
                    try { normalizedRecipe = PatternMatchingRecipeStore.NormalizeRecipeName(_opt.RecipeName); }
                    catch { normalizedRecipe = string.IsNullOrWhiteSpace(_opt.RecipeName) ? "Default" : _opt.RecipeName.Trim(); }
                    _opt.RecipeName = normalizedRecipe;

                    // 2) Root 정책 강제 (Configs/PatternMatching 절대 쓰지 않음)
                    _opt.RecipeRootDirectory = Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        "Recipes",
                        normalizedRecipe,
                        "PatternMatching");

                    string camName = _camera?.Name ?? "NoCamera";

                    string path = PatternMatchingRecipeStore.ResolveRecipePath(
                        _opt.RecipeRootDirectory,
                        camName,
                        normalizedRecipe,
                        createDirectoryForSave: false);

                    //var container = PatternMatchingRecipeStore.Load(path);
                    var container = PatternMatchingRecipeStore.Load(path) ?? new PatternMatchingRecipeJson();
                    if (container == null)
                    {
                        _lastFailReason = $"Recipe 없음. camera={camName}, recipe={normalizedRecipe}, path={path}";
                        _recipeLoaded = false;
                        return false;
                    }

                    // 정규화된 이름을 runner 옵션에 반영(로그/저장 파일명도 일관)
                    _opt.RecipeName = normalizedRecipe;

                    _parameters = container.Parameters?.Clone();
                    _Roi = container.Roi;

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

        public void SetPreferCenterMostMatchMode(bool bOn)
        {
            _opt.PreferCenterMostMatch = bOn;
            RedrawLastOverlays();
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


        public void SetProcessMode(ProcessMode mode)
        {
            lock (_sync)
            {
                _processMode = mode;
                if (_modeOptions.TryGetValue(mode, out var opt))
                {
                    _opt.Mode = opt.SearchMode;
                    _opt.PreferCenterMostMatch = opt.PreferCenterMostMatch;
                    _opt.InspectRoiSourceMode = opt.RoiSourceMode;
                    _profileMode = opt.ProfileMode;
                    _opt.ProfileMode = opt.ProfileMode;
                }
            }
        }
        public ProcessMode GetProcessMode()
        {
            lock (_sync) return _processMode;
        }
        public PatternMatchRunResult SearchByCurrentMode(VisionImage externalImage = null, bool save = false, CancellationToken ct = default)
        {
            lock (_sync)
            {
                if (_processMode == ProcessMode.Prealign)
                    return SearchCenterMark(externalImage, save, ct);

                return InternalSearchCore(externalImage, save, ct); // MapMatching/SecondAlign
            }
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
                // [최적화] GetImage() 호출은 Bitmap 변환 등 무거운 작업을 유발할 수 있으므로,
                // 가벼운 Header 속성(Width/Height 등)만 검사하여 변경 여부를 확인합니다.
                if (vi == null || vi.Header == null || vi.Header.Width <= 0 || vi.Header.Height <= 0)
                {
                    sb.Append("NULL;");
                    continue;
                }

                sb.Append(vi.Header.Width).Append('x').Append(vi.Header.Height).Append(';');
            }
            return ComputeSha1(sb.ToString()) != _lastTemplateHash;
        }

        private void UpdateTemplateHash()
        {
            if (_parameters?.TrainImages == null) { _lastTemplateHash = string.Empty; return; }
            var sb = new StringBuilder();
            foreach (var vi in _parameters.TrainImages)
            {
                if (vi == null || vi.Header == null || vi.Header.Width <= 0 || vi.Header.Height <= 0)
                {
                    sb.Append("NULL;");
                    continue;
                }

                sb.Append(vi.Header.Width).Append('x').Append(vi.Header.Height).Append(';');
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
                // [최적화] UI 컨트롤 갱신은 무조건 비동기로 던져서 Search 스레드(연산)가 대기하지 않게 합니다.
                if (vw.IsHandleCreated)
                {
                    vw.BeginInvoke(new Action(() => ApplyOverlayToViewer(vw, raw)));
                }
            }
        }

        // [추가] 실제 오버레이 그리기 로직 분리
        private void ApplyOverlayToViewer(VisionImageViewer vw, PatternMatchingResult raw)
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

                        if (_opt.ShowMatchIndexes)
                        {
                            try
                            {
                                int tx = (int)Math.Round(absX) + (len + 3);
                                int ty = (int)Math.Round(absY) - (len + 3);

                                if (ty < 0) ty = (int)Math.Round(absY) + (len + 3);
                                if (tx < 0) tx = (int)Math.Round(absX) + 2;

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

        public void SearchTheta(VisionImage img, out double Angle)
        {
            var result = Search(img);
            if(result.Success)
            {
                Angle = result.Matches.Average(t=>t.R);
            }
            else
            {
                Angle = 0;
            }
        }
        #endregion


        public PatternMatchRunResult SearchWithTemporaryInspectRoi(
                                    VisionImage externalImage,
                                    Point inspectStart,
                                    Point inspectEnd,
                                    bool save = false)
        {
            // ROI 변경은 runner 내부 상태(_part)에 영향을 주므로 동기화 필요
            lock (_sync)
            {
                Point oldStart = Point.Empty;
                Point oldEnd = Point.Empty;

                // 기존 ROI 백업
                try
                {
                    oldStart = _part.GetInspectStartPoint();
                    oldEnd = _part.GetInspectEndPoint();
                }
                catch { }

                try
                {
                    // 임시 ROI 설정
                    try
                    {
                        _part.SetInspectStartPoint(inspectStart);
                        _part.SetInspectEndPoint(inspectEnd);
                    }
                    catch { }

                    // 기존 Search 로직 재사용(외부 이미지 전달)
                    return Search(externalImage, save);
                }
                finally
                {
                    // ROI 원복
                    try
                    {
                        _part.SetInspectStartPoint(oldStart);
                        _part.SetInspectEndPoint(oldEnd);
                    }
                    catch { }
                }
            }
        }


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

        #region [NEW] Center-Most selection helpers
        /// <summary>
        /// "무조건 이미지 중앙 마크" 선택:
        /// 결과 목록 중 이미지 중심점(W/2, H/2)에 가장 가까운 매치를 대표로 선택한다.
        /// </summary>
        private static bool TryPickCenterMostMatch(
            VisionImage image,
            IList<PatternMatchingResult.PatternMatchingResultValue> values,
            out PatternMatchingResult.PatternMatchingResultValue selected,
            out int selectedIndex)
        {
            selected = default(PatternMatchingResult.PatternMatchingResultValue);
            selectedIndex = -1;

            if (image == null || image.Header == null || image.Header.Width <= 0 || image.Header.Height <= 0)
                return false;

            if (values == null || values.Count == 0)
                return false;

            double cx = image.Header.Width * 0.5;
            double cy = image.Header.Height * 0.5;

            double bestD2 = double.MaxValue;

            for (int i = 0; i < values.Count; i++)
            {
                var v = values[i];
                double dx = v.X - cx;
                double dy = v.Y - cy;
                double d2 = dx * dx + dy * dy;

                if (d2 < bestD2)
                {
                    bestD2 = d2;
                    selected = v;
                    selectedIndex = i;
                }
            }

            return selectedIndex >= 0;
        }

        /// <summary>
        /// 외부 이미지(또는 카메라 이미지)에서 "무조건 가운데 마크"를 찾는다.
        /// - 반환값: 중앙에 가장 가까운 "대표 매치 1개"만 포함한 결과
        /// - UI/호출부에서 전체 매치 목록이 필요하면 Search(Mode=All) 사용
        /// </summary>
        public PatternMatchRunResult SearchCenterMark(VisionImage externalImage = null, bool save = false, CancellationToken ct = default)
        {
            // 후보를 받아야 하므로 All로 강제 (호출 후 원복)
            var prevMode = _opt.Mode;
            try
            {
                _opt.Mode = SearchMode.All;

                var r = InternalSearchCore(externalImage, save, ct);
                if (!r.Success || r.RawResult == null || r.RawResult.Values == null || r.RawResult.Values.Count == 0)
                    return r;

                // 어떤 이미지 기준으로 "센터"를 잡을지 결정
                // - externalImage가 들어오면 그걸 기준(권장)
                // - 아니면 이번 Search에서 취득한 RawResult 기준 이미지가 없으므로 camera latest로 fallback
                var centerRefImage = externalImage ?? _camera?.LatestImage;
                if (centerRefImage == null || centerRefImage.Header == null)
                {
                    r.Success = false;
                    r.FailReason = "Center reference image not available";
                    return r;
                }

                PatternMatchingResult.PatternMatchingResultValue selected;
                int selectedIndex;
                if (!TryPickCenterMostMatch(centerRefImage, r.RawResult.Values, out selected, out selectedIndex))
                {
                    r.Success = false;
                    r.FailReason = "Center-most selection failed";
                    return r;
                }

                // 대표값 강제 치환
                r.ReferenceIndex = 0;
                r.X = selected.X;
                r.Y = selected.Y;
                r.R = selected.R;

                // [CHG] "찾은 마크 1개만 반환" 정책
                r.Matches = new List<PatternMatchingResult.PatternMatchingResultValue> { selected };

                // Last cache들도 동일 정책으로 덮어쓰기 (뷰/오버레이 일관성)
                _lastPoint = new Point((int)selected.X, (int)selected.Y);
                _lastAngle = selected.R;
                _lastReferenceIndex = 0;

                // 오버레이는 1개만 기반으로 그리도록 RawResult도 축소(Values만 복사/대체)
                //  - ResultOverlays까지 줄일지 여부는 Tool 구현에 따라 다르므로 여기서는 Values만 1개로 제한
                try
                {
                    _lastRawResult = r.RawResult;
                    if (_lastRawResult.Values != null)
                    {
                        _lastRawResult.Values = new PatternMatchingResult.PatternMatchingResultValueCollection(
                            new List<PatternMatchingResult.PatternMatchingResultValue> { selected });
                    }
                }
                catch
                {
                    // Values 대체 실패 시에도 r.Matches는 1개로 유지
                    _lastRawResult = r.RawResult;
                }

                // Overlays 갱신
                UpdateViewerOverlays(_lastRawResult);

                return r;
            }
            finally
            {
                _opt.Mode = prevMode;
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

                        Point searchStart;
                        Point searchEnd;
                        TryResolveSearchRoi(src, out searchStart, out searchEnd);

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

                        result.Matches = new List<PatternMatchingResult.PatternMatchingResultValue>(raw.Values);
                        PatternMatchingResult.PatternMatchingResultValue repr = raw.Values[0];
                        int refIdx = 0;

                        // [CHG] 대표 선택 정책
                        // 1) CenterMark 강제(옵션 PreferCenterMostMatch)
                        // 2) 기존 SearchMode 정책 유지
                        if (_opt.PreferCenterMostMatch && src != null && src.Header != null)
                        {
                            PatternMatchingResult.PatternMatchingResultValue centerMost;
                            int centerMostIdx;
                            if (TryPickCenterMostMatch(src, raw.Values, out centerMost, out centerMostIdx))
                            {
                                repr = centerMost;
                                refIdx = centerMostIdx;
                            }
                        }
                        else if (_opt.Mode == SearchMode.First && raw.Values.Count > 1 && src != null && src.Header != null)
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
                        _lastRawResult = raw;

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

        private static void ClampPointToImage(ref Point p, int width, int height)
        {
            if (p.X < 0) p.X = 0;
            if (p.Y < 0) p.Y = 0;
            if (p.X > width - 1) p.X = width - 1;
            if (p.Y > height - 1) p.Y = height - 1;
        }

        private static bool IsValidRoi(Point s, Point e)
        {
            return e.X > s.X && e.Y > s.Y;
        }

        private bool TryResolveSearchRoi(VisionImage src, out Point searchStart, out Point searchEnd)
        {
            searchStart = new Point(0, 0);
            searchEnd = new Point(src.Header.Width - 1, src.Header.Height - 1);

            if (_opt.UseInspectRoi == false)
                return true;

            // 1) Profile ROI 우선
            if (_opt.InspectRoiSourceMode == RoiSourceMode.Profile)
            {
                ProfileRoiSetting cfg;
                if (_profileRoiSettings.TryGetValue(_profileMode, out cfg) && cfg != null && cfg.Enabled)
                {
                    var s = cfg.InspectStart;
                    var e = cfg.InspectEnd;
                    ClampPointToImage(ref s, src.Header.Width, src.Header.Height);
                    ClampPointToImage(ref e, src.Header.Width, src.Header.Height);

                    if (IsValidRoi(s, e))
                    {
                        searchStart = s;
                        searchEnd = e;
                        return true;
                    }
                }
            }

            // 2) Recipe ROI fallback
            try
            {
                var s = _part.GetInspectStartPoint();
                var e = _part.GetInspectEndPoint();

                ClampPointToImage(ref s, src.Header.Width, src.Header.Height);
                ClampPointToImage(ref e, src.Header.Width, src.Header.Height);

                if (IsValidRoi(s, e))
                {
                    searchStart = s;
                    searchEnd = e;
                }
            }
            catch
            {
                // full frame 유지
            }

            return true;
        }

        // Public API 영역에 추가 (GUI에서 현재값 확인용)
        public bool TryGetProfileInspectRoi(RunnerProfileMode mode, out Point inspectStart, out Point inspectEnd, out bool enabled)
        {
            lock (_sync)
            {
                ProfileRoiSetting cfg;
                if (_profileRoiSettings.TryGetValue(mode, out cfg) && cfg != null)
                {
                    inspectStart = cfg.InspectStart;
                    inspectEnd = cfg.InspectEnd;
                    enabled = cfg.Enabled;
                    return true;
                }

                inspectStart = Point.Empty;
                inspectEnd = Point.Empty;
                enabled = false;
                return false;
            }
        }
        #endregion

        public PatternMatchRunResult SearchCenterMarkWithTemporaryInspectRoi(
                                    VisionImage externalImage,
                                    Point inspectStart,
                                    Point inspectEnd,
                                    bool save = false,
                                    CancellationToken ct = default)
        {
            lock (_sync)
            {
                Point oldStart = Point.Empty, oldEnd = Point.Empty;
                try 
                { 
                    oldStart = _part.GetInspectStartPoint(); 
                    oldEnd = _part.GetInspectEndPoint(); 
                } 
                catch { }

                try
                {
                    try { _part.SetInspectStartPoint(inspectStart); _part.SetInspectEndPoint(inspectEnd); } catch { }
                    return SearchCenterMark(externalImage, save, ct);
                }
                finally
                {
                    try { _part.SetInspectStartPoint(oldStart); _part.SetInspectEndPoint(oldEnd); } catch { }
                }
            }
        }

        // #region Public API 내부 적절한 위치에 추가
        public void SetProfileMode(RunnerProfileMode mode)
        {
            lock (_sync)
            {
                _profileMode = mode;
                _opt.ProfileMode = mode;
            }
        }

        public RunnerProfileMode GetProfileMode()
        {
            lock (_sync)
                return _profileMode;
        }

        public void SetInspectRoiSourceMode(RoiSourceMode mode)
        {
            lock (_sync)
            {
                _opt.InspectRoiSourceMode = mode;
            }
        }

        public RoiSourceMode GetInspectRoiSourceMode()
        {
            lock (_sync)
                return _opt.InspectRoiSourceMode;
        }

        public void SetProfileInspectRoi(RunnerProfileMode mode, Point inspectStart, Point inspectEnd, bool enabled = true)
        {
            lock (_sync)
            {
                var s = new Point(Math.Min(inspectStart.X, inspectEnd.X), Math.Min(inspectStart.Y, inspectEnd.Y));
                var e = new Point(Math.Max(inspectStart.X, inspectEnd.X), Math.Max(inspectStart.Y, inspectEnd.Y));

                _profileRoiSettings[mode] = new ProfileRoiSetting
                {
                    Enabled = enabled,
                    InspectStart = s,
                    InspectEnd = e
                };
            }
        }

        public void SetProfileInspectRoiEnabled(RunnerProfileMode mode, bool enabled)
        {
            lock (_sync)
            {
                ProfileRoiSetting cfg;
                if (_profileRoiSettings.TryGetValue(mode, out cfg) == false || cfg == null)
                {
                    cfg = new ProfileRoiSetting();
                    _profileRoiSettings[mode] = cfg;
                }
                cfg.Enabled = enabled;
            }
        }
    }
}
