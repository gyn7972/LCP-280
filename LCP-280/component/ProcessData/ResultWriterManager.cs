// LCP-280\component\ProcessData\ResultWriterManager.cs
using QMC.Common;
using QMC.Common.Account;
using QMC.Common.PKGTester;
using QMC.LCP_280.Process.Work;
using Renci.SshNet;
using Renci.SshNet.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QMC.LCP_280.Process.Component.ProcessData
{
    /// <summary>
    /// ResultWriterManager (B안 Improved)
    /// - 1. Local Writing: Always performed immediately (synchronous).
    /// - 2. Network Upload: Always performed in Background (asynchronous/outbox).
    /// - Even if Wafer ends, the background thread continues to process the queue.
    /// </summary>
    public class ResultWriterManager
    {
        #region Context Classes
        public class SUMContext
        {
            public string EqpName { get; set; } = "LPC-280";
            public string StartTime { get; set; }
            public string EndTime { get; set; }
            public string WaferID { get; set; }
            public int TotalCount { get; set; }
            public int GoodCount { get; set; }
            public int NGCount { get; set; }
            public List<string> ItemNames { get; set; } = new List<string>();
            public Dictionary<string, double> Min { get; set; } = new Dictionary<string, double>();
            public Dictionary<string, double> Max { get; set; } = new Dictionary<string, double>();
            public Dictionary<string, double> Avg { get; set; } = new Dictionary<string, double>();
            public Dictionary<string, double> Std { get; set; } = new Dictionary<string, double>();
            public Dictionary<int, int> BinCounts { get; set; } = new Dictionary<int, int>();
            public List<string> ParameterBlock { get; set; } = new List<string>();
            public List<string> ZeroBlock { get; set; } = new List<string>();
        }

        public class PRDContext
        {
            public List<string> HeaderLines { get; set; } = new List<string>();
            public List<string> ParameterBlock { get; set; } = new List<string>();
            public List<string> ZeroBlock1 { get; set; } = new List<string>();
            public List<string> ZeroBlock2 { get; set; } = new List<string>();
            public List<string> DataColumns { get; set; } = new List<string>();
            public bool HeaderInitialized { get; set; }
        }

        public class WAFContext
        {
            public List<string> HeaderLines { get; set; } = new List<string>();
            public List<string> ParameterBlock { get; set; } = new List<string>();
            public List<string> ZeroBlock1 { get; set; } = new List<string>();
            public List<string> ZeroBlock2 { get; set; } = new List<string>();
            public List<string> DataColumns { get; set; } = new List<string>();
            public bool HeaderInitialized { get; set; }
        }
        #endregion

        private Equipment Equipment = Equipment.Instance;

        public PRDContext PrdContext { get; set; } = new PRDContext();
        public SUMContext SumContext { get; set; } = new SUMContext();
        public WAFContext WafContext { get; set; } = new WAFContext();

        public TestConditionSet CurrentTestConditionSet { get; set; }

        private string _lastPrdWaferId;
        private string _lastWafWaferId;
        private readonly object _headerLock = new object();

        private static readonly string _runDate = DateTime.Now.ToString("yyyyMMdd");
        private string _baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ResultData");
        private string _logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

        public string BaseDir
        {
            get => _baseDir;
            set => _baseDir = value;
        }

        private string LogDir
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_logDir))
                    return _logDir;

                try
                {
                    var cfg = Equipment.Instance?.EquipmentConfig;
                    if (cfg != null)
                    {
                        if (cfg.NetworkMode > 1 && !string.IsNullOrWhiteSpace(cfg.LogPath))
                            return cfg.LogPath;
                    }
                }
                catch { }

                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            }
            set
            {
                _logDir = string.IsNullOrWhiteSpace(value)
                    ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs")
                    : value.Trim();
            }
        }

        private readonly object _ioLock = new object();

        #region Paths / Helpers (Local Result + Network)
        private string GetLocalResultRoot()
        {
            // [수정] 프로그램 시작 시 고정된 _runDate 대신, 현재 시점의 날짜를 사용하여 자정이 지나면 폴더가 변경되도록 수정
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ResultData", DateTime.Now.ToString("yyyyMMdd"));
            //return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ResultData", _runDate);
        }

        private string GetLocalResultDir(string waferId)
        {
            var root = GetLocalResultRoot();
            Directory.CreateDirectory(root);
            return Path.Combine(root, waferId);
        }

        private string GetNetworkResultDir(string waferId, string configuredPath)
        {
            if (string.IsNullOrWhiteSpace(configuredPath))
                return null;

            return Path.Combine(configuredPath, waferId);
        }

        private bool ShouldUploadToNetwork()
        {
            try
            {
                var cfg = Equipment.Instance?.EquipmentConfig;
                return (cfg != null && cfg.NetworkMode > 0);
            }
            catch
            {
                return false;
            }
        }

        private string SafeWaferId(string waferId)
        {
            if (string.IsNullOrEmpty(waferId)) return "UNKNOWN";
            return waferId.Trim();
        }

        private string StripExtension(string name, bool removeAll = false)
        {
            if (string.IsNullOrWhiteSpace(name)) return "NONE";

            string justName = Path.GetFileName(name);

            if (!removeAll)
                return Path.GetFileNameWithoutExtension(justName);

            int dot;
            while ((dot = justName.LastIndexOf('.')) > 0)
                justName = justName.Substring(0, dot);

            return justName;
        }
        #endregion

        // =========================================================
        // ======================= (B) OUTBOX =======================
        // =========================================================
        #region B: Rename-only Upload Queue + Background Uploader (SMB/SFTP)

        private readonly object _outboxLock = new object();

        // “실시간처럼”: 기본 1초. (PRD/WAF는 더 크게 잡는 것을 권장)
        private static readonly TimeSpan DefaultMinInterval = TimeSpan.FromSeconds(1);
        private static readonly TimeSpan LargeFileMinInterval = TimeSpan.FromSeconds(5); // 권장값: PRD/WAF

        // 목적지(networkFile)별 업로드 요청 속도 제한
        private readonly Dictionary<string, DateTime> _lastEnqueueUtcByDest =
            new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);

        // [수정 1] 제한 시간에 걸려 대기 중인 파일들을 추적하기 위한 Dictionary 추가
        // Key: DestFile (목적지 경로), Value: LocalFile (원본 경로)
        private readonly Dictionary<string, string> _pendingUploads =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private CancellationTokenSource _uploaderCts;
        private Task _uploaderTask;
        private readonly AutoResetEvent _uploaderWake = new AutoResetEvent(false);

        private string GetOutboxRoot()
        {
            //return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ResultData", _runDate, "_outbox");
            // 기존: Path.Combine(..., "ResultData", _runDate, "_outbox");  <-- 날짜 종속적 (위험)
            // 변경: Path.Combine(..., "ResultData", "_outbox");            <-- 날짜 독립적 (안전)
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ResultData", "_outbox");
        }

        private string GetOutboxQueueDir()
        {
            return Path.Combine(GetOutboxRoot(), "queue");
        }

        private bool IsLargeFile(string localFile)
        {
            var ext = (Path.GetExtension(localFile) ?? "").ToLowerInvariant();
            return ext == ".prd" || ext == ".waf";
        }

        private TimeSpan GetMinIntervalForFile(string localFile)
        {
            return IsLargeFile(localFile) ? LargeFileMinInterval : DefaultMinInterval;
        }

        // [수정 2] ShouldEnqueueNow 로직 변경 (여기서는 단순히 시간 체크만 수행)
        private bool IsRateLimited(string destKey, TimeSpan interval)
        {
            try
            {
                lock (_outboxLock) // Dictionary 접근 시 lock 필요
                {
                    if (_lastEnqueueUtcByDest.TryGetValue(destKey, out var lastUtc))
                    {
                        if ((DateTime.UtcNow - lastUtc) < interval)
                            return true; // 제한 걸림
                    }
                    return false; // 제한 안 걸림
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }
        }

        private void MarkEnqueued(string destKey)
        {
            try
            {
                lock (_outboxLock)
                {
                    _lastEnqueueUtcByDest[destKey] = DateTime.UtcNow;
                    // 큐에 들어갔으므로 대기 목록에서는 제거
                    if (_pendingUploads.ContainsKey(destKey))
                        _pendingUploads.Remove(destKey);
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

        private bool ShouldEnqueueNow(string destKey, TimeSpan interval)
        {
            try
            {
                if (_lastEnqueueUtcByDest.TryGetValue(destKey, out var lastUtc))
                {
                    if ((DateTime.UtcNow - lastUtc) < interval)
                        return false;
                }
                _lastEnqueueUtcByDest[destKey] = DateTime.UtcNow;
                return true;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }
        }

        /// <summary>
        /// This method simply queues the upload job. 
        /// It does NOT copy files immediately. It delegates everything to the background thread.
        /// </summary>
        // [수정 3] QueueNetworkUpload 로직 전면 수정
        private void QueueNetworkUpload(string waferId, string localFile, string networkFileForSmb)
        {
            if (string.IsNullOrWhiteSpace(localFile))
                return;

            if (!ShouldUploadToNetwork())
                return;

            string dest;
            switch (UploadMode)
            {
                case RemoteUploadMode.Sftp:
                    dest = BuildSftpRemotePath(waferId, localFile);
                    break;
                case RemoteUploadMode.SmbShare:
                    dest = networkFileForSmb;
                    break;
                default:
                    return;
            }

            if (string.IsNullOrWhiteSpace(dest)) return;

            var interval = GetMinIntervalForFile(localFile);
            bool executeNow = false;

            lock (_outboxLock)
            {
                // 1. 속도 제한에 걸리는지 확인
                if (IsRateLimited(dest, interval))
                {
                    // 2-A. 제한에 걸리면 "Pending(보류)" 상태로 등록만 하고 리턴.
                    //      나중에 백그라운드 스레드가 시간이 되면 큐에 넣음.
                    _pendingUploads[dest] = localFile;
                    // 백그라운드 스레드가 Pending 목록을 체크하도록 깨움
                    _uploaderWake.Set();
                    return;
                }

                // 2-B. 제한이 없으면 즉시 큐에 넣음
                MarkEnqueued(dest);
                executeNow = true;
            }

            // Lock 밖에서 파일 생성 (I/O)
            if (executeNow)
            {
                EnqueueUploadJob(localFile, dest);
            }
        }

        private sealed class UploadJob
        {
            public string LocalFile;
            public string DestFile;
            public RemoteUploadMode Mode;
            public DateTime Utc;
        }

        /// <summary>
        /// Outbox 큐에 job 파일 생성. (데이터 복사 없음)
        /// </summary>
        private void EnqueueUploadJob(string localFile, string destFile)
        {
            try
            {
                //생성자에서 1회 호출.
                //Directory.CreateDirectory(GetOutboxQueueDir());

                var job = new UploadJob
                {
                    LocalFile = localFile,
                    DestFile = destFile,
                    Mode = UploadMode,
                    Utc = DateTime.UtcNow
                };

                // Job format: Mode \n LocalFile \n DestFile \n Ticks
                string body = string.Join("\n",
                    ((int)job.Mode).ToString(CultureInfo.InvariantCulture),
                    job.LocalFile ?? "",
                    job.DestFile ?? "",
                    job.Utc.Ticks.ToString(CultureInfo.InvariantCulture)
                );

                string name = Guid.NewGuid().ToString("N") + ".job";
                string tmp = Path.Combine(GetOutboxQueueDir(), name + ".tmp");
                string final = Path.Combine(GetOutboxQueueDir(), name);

                File.WriteAllText(tmp, body, Encoding.UTF8);
                File.Move(tmp, final); // Atomic move

                // Signal the background thread
                _uploaderWake.Set();
            }
            catch (Exception ex)
            {
                // [안전장치] 만약 디렉터리가 없어서 에러난 경우라면 여기서 생성 시도 (선택 사항)
                if (ex is DirectoryNotFoundException)
                {
                    try { Directory.CreateDirectory(GetOutboxQueueDir()); } catch { }
                }

                try { Log.Write("ResultWriterManager", nameof(EnqueueUploadJob), ex.Message); } catch { }
            }
        }

        private void StartOutboxUploader()
        {
            if (_uploaderTask != null) return;

            _uploaderCts = new CancellationTokenSource();
            _uploaderTask = Task.Run(() => UploadLoop(_uploaderCts.Token));
        }

        public void StopOutboxUploader(bool drain = false, int drainTimeoutMs = 5000)
        {
            try
            {
                if (drain)
                    DrainJobs(drainTimeoutMs);

                _uploaderCts?.Cancel();
                _uploaderWake.Set();
                _uploaderTask?.Wait(1000);
            }
            catch { }
        }

        private void DrainJobs(int timeoutMs)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                if (!HasAnyJob())
                    return;
                Thread.Sleep(100);
            }
        }

        private bool HasAnyJob()
        {
            try
            {
                string q = GetOutboxQueueDir();
                if (!Directory.Exists(q)) return false;
                return Directory.EnumerateFiles(q, "*.job").Any();
            }
            catch
            {
                return true;
            }
        }

        private UploadJob TryParseJobFile(string jobPath)
        {
            try
            {
                var lines = File.ReadAllLines(jobPath, Encoding.UTF8);
                if (lines.Length < 3) return null;

                int modeInt = 0;
                int.TryParse(lines[0].Trim(), out modeInt);

                return new UploadJob
                {
                    Mode = (RemoteUploadMode)modeInt,
                    LocalFile = lines[1].Trim(),
                    DestFile = lines[2].Trim(),
                    Utc = (lines.Length >= 4 && long.TryParse(lines[3].Trim(), out var ticks))
                        ? new DateTime(ticks, DateTimeKind.Utc)
                        : DateTime.UtcNow
                };
            }
            catch
            {
                return null;
            }
        }

        // [수정] UploadLoop에서 MarkEnqueued 호출 시 Lock 확인
        private void UploadLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                bool didWork = false;

                try
                {
                    // === [추가된 부분] Pending(보류)된 파일들 체크 ===
                    // 제한 시간이 풀린 항목이 있으면 큐에 집어넣음
                    List<KeyValuePair<string, string>> toEnqueue = null;
                    lock (_outboxLock)
                    {
                        if (_pendingUploads.Count > 0)
                        {
                            var now = DateTime.UtcNow;
                            foreach (var kvp in _pendingUploads)
                            {
                                string dest = kvp.Key;
                                string src = kvp.Value;
                                var interval = GetMinIntervalForFile(src);

                                bool ready = false;
                                if (_lastEnqueueUtcByDest.TryGetValue(dest, out var lastUtc))
                                {
                                    if ((now - lastUtc) >= interval) ready = true;
                                }
                                else
                                {
                                    ready = true;
                                }

                                if (ready)
                                {
                                    if (toEnqueue == null) toEnqueue = new List<KeyValuePair<string, string>>();
                                    toEnqueue.Add(kvp);
                                }
                            }
                        }

                        // [중요] 상태 변경도 Lock 안에서 수행해야 안전함
                        if (toEnqueue != null)
                        {
                            foreach (var item in toEnqueue)
                            {
                                // 이제 MarkEnqueued 내부에는 Lock이 없으므로 안전하게 호출됨
                                MarkEnqueued(item.Key);
                            }
                        }
                    }

                    // Lock 밖에서 실제 파일 I/O 수행
                    if (toEnqueue != null)
                    {
                        foreach (var item in toEnqueue)
                        {
                            EnqueueUploadJob(item.Value, item.Key);
                            didWork = true;
                        }
                    }
                    // ===============================================

                    string q = GetOutboxQueueDir();
                    if (Directory.Exists(q))
                    {
                        var jobs = Directory.EnumerateFiles(q, "*.job")
                                            .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
                                            .ToList();

                        foreach (var jobPath in jobs)
                        {
                            if (token.IsCancellationRequested) break;

                            string working = jobPath + ".working";
                            try
                            {
                                if (File.Exists(working)) continue;
                                File.Move(jobPath, working);
                            }
                            catch { continue; }

                            didWork = true;

                            var job = TryParseJobFile(working);
                            if (job == null || string.IsNullOrWhiteSpace(job.LocalFile) || string.IsNullOrWhiteSpace(job.DestFile))
                            {
                                TryDeleteFileQuietly(working);
                                continue;
                            }

                            bool ok = false;
                            if (job.Mode == RemoteUploadMode.SmbShare)
                                ok = TryUploadLatestViaSmb_RenameOnly(job.LocalFile, job.DestFile);
                            else if (job.Mode == RemoteUploadMode.Sftp)
                                ok = TryUploadSnapshotViaSftp(job.LocalFile, job.DestFile);

                            if (ok)
                            {
                                TryDeleteFileQuietly(working);
                            }
                            else
                            {
                                try
                                {
                                    if (File.Exists(working))
                                        File.Move(working, working.Replace(".working", ".job"));
                                }
                                catch { }
                                Thread.Sleep(500);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    try { Log.Write("ResultWriterManager", nameof(UploadLoop), ex.Message); } catch { }
                }

                // 할 일이 없으면 대기 (Pending 체크를 위해 타임아웃을 짧게 가져감)
                if (!didWork)
                {
                    // [중요] Pending 항목이 있다면 100ms마다 깨어나서 시간 체크
                    // Pending 항목이 없다면 500ms~1000ms 대기
                    int waitTime = 1000;
                    lock (_outboxLock)
                    {
                        if (_pendingUploads.Count > 0) waitTime = 100;
                    }
                    _uploaderWake.WaitOne(waitTime);
                }
            }
        }

        private bool TryUploadLatestViaSmb_RenameOnly(string localFile, string networkFile)
        {
            const int retry = 3;

            for (int i = 0; i < retry; i++)
            {
                try
                {
                    if (!File.Exists(localFile))
                        return true; // 원본 파일이 없으면 성공으로 간주

                    string ndir = Path.GetDirectoryName(networkFile);
                    if (!string.IsNullOrWhiteSpace(ndir))
                        Directory.CreateDirectory(ndir);

                    string tmp = networkFile + ".tmp";

                    // [수정] File.Copy 대신 FileStream을 사용하여 FileShare.ReadWrite로 읽기 시도
                    // 메인 스레드가 쓰고 있는 중이어도 읽을 수 있도록 허용
                    using (var sourceStream = new FileStream(localFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var destStream = new FileStream(tmp, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        sourceStream.CopyTo(destStream);
                    }

                    // Atomic Swap (기존 파일 삭제 후 이름 변경)
                    if (File.Exists(networkFile))
                    {
                        try { File.Delete(networkFile); } catch { }
                    }
                    File.Move(tmp, networkFile);
                    return true;
                }
                catch (IOException ioEx)
                {
                    // 파일이 잠겨있어서 읽지 못하는 경우 (잠시 대기 후 재시도)
                    // 로그에 남기되 너무 자주 남기지 않도록 주의
                    Log.Write("ResultWriterManager", "SmbUpload_Retry", ioEx.Message); 
                    Thread.Sleep(500);
                }
                catch (Exception ex)
                {
                    // 기타 에러
                    try { Log.Write(ex); } catch { }
                    return false; // 재시도 하지 않고 실패 처리 (혹은 상황에 따라 continue)
                }
            }

            return false;
        }
        //private bool TryUploadLatestViaSmb_RenameOnly(string localFile, string networkFile)
        //{
        //    const int retry = 3;

        //    for (int i = 0; i < retry; i++)
        //    {
        //        try
        //        {
        //            if (!File.Exists(localFile))
        //                return true; // If local file is gone, we consider it "done" or invalid.

        //            string ndir = Path.GetDirectoryName(networkFile);
        //            if (!string.IsNullOrWhiteSpace(ndir))
        //                Directory.CreateDirectory(ndir);

        //            string tmp = networkFile + ".tmp";

        //            // Copy to temp
        //            File.Copy(localFile, tmp, true);

        //            // Atomic Swap
        //            if (File.Exists(networkFile))
        //            {
        //                try { File.Delete(networkFile); } catch { }
        //            }
        //            File.Move(tmp, networkFile);
        //            return true;
        //        }
        //        catch
        //        {
        //            Thread.Sleep(500);
        //        }
        //    }

        //    return false;
        //}

        private static void TryDeleteFileQuietly(string path)
        {
            try { if (File.Exists(path)) File.Delete(path); } catch { }
        }

        #endregion

        // =========================================================
        // ================== Summary Calculation State =============
        // =========================================================
        private readonly object _summaryLock = new object();
        private Dictionary<string, double> _sum = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, double> _sumSq = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        private bool _summaryFinalized = false;

        private readonly Dictionary<int, string> _binLabelMap = new Dictionary<int, string>();
        private readonly object _waferTotalSummaryOnceLock = new object();
        private readonly HashSet<string> _waferTotalSummaryWritten = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public ResultWriterManager()
        {
            try
            {
                Directory.CreateDirectory(BaseDir);

                var logDir = LogDir;
                if (!string.IsNullOrWhiteSpace(logDir))
                    Directory.CreateDirectory(logDir);

                // [수정] 생성 시 Outbox Queue 폴더 미리 생성 (I/O 부하 감소)
                Directory.CreateDirectory(GetOutboxQueueDir());

                // [ADD] Start the Local File Save Worker
                InitializeSaveWorker();

                // Start the background uploader immediately on startup.
                StartOutboxUploader();
            }
            catch { }
        }

        // =========================================================
        // ===================== Result Writers =====================
        // =========================================================
        #region Legacy Format Append API (write local + outbox spool)

        public int AppendTxTDie(MaterialDie die)
        {
            PKGTesterResult result = die.TesterResult;
            string waferId = SafeWaferId(die.SourceWaferId);

            var eqpConfig = Equipment.Instance.EquipmentConfig;

            // 1. Local Write
            string localDir = GetLocalResultDir(waferId);
            Directory.CreateDirectory(localDir);
            string localFile = Path.Combine(localDir, waferId + ".txt");

            string networkDir = ShouldUploadToNetwork() ? GetNetworkResultDir(waferId, eqpConfig.TXTResultPath) : null;
            string networkFile = !string.IsNullOrWhiteSpace(networkDir) ? Path.Combine(networkDir, waferId + ".txt") : null;

            string strBinFileName = die.SourceBinFileName;

            // [수정] 네트워크 업로드 큐 등록을 lock 밖으로 빼기 위해 플래그 사용
            bool needUpload = false;

            lock (_ioLock)
            {
                bool exists = File.Exists(localFile);
                using (var fs = new FileStream(localFile, FileMode.Append, FileAccess.Write, FileShare.Read))
                using (var w = new StreamWriter(fs, Encoding.UTF8))
                {
                    if (!exists)
                    {
                        w.WriteLine(strBinFileName);
                        w.WriteLine(waferId);
                        w.Write("XADR,YADR,RANK");
                        foreach (var key in EnumerateItemKeys(result)) w.Write("," + key);
                        w.WriteLine();
                    }

                    var bin = result.BinningResult;
                    w.Write((die.MapX).ToString());
                    w.Write("," + (die.MapY).ToString());
                    w.Write("," + (bin != null ? bin.BinNo.ToString() : "0"));
                    foreach (var v in EnumerateItemValues(result)) w.Write("," + v);
                    w.WriteLine();
                }

                // 파일 쓰기가 성공했으면 업로드 플래그 설정
                if (!string.IsNullOrWhiteSpace(networkFile))
                {
                    needUpload = true;
                }
            }

            // 2. Queue for Background Upload (Lock 밖에서 실행하여 멈춤 방지)
            if (needUpload)
            {
                QueueNetworkUpload(waferId, localFile, networkFile);
            }

            return 0;
        }

        public int AppendBinDie(MaterialDie die)
        {
            if (die == null || die.TesterResult == null) return -1;

            var br = die.TesterResult.BinningResult;
            if (br != null && !string.IsNullOrWhiteSpace(br.BinLabel))
            {
                int bn = br.BinNo < 0 ? 0 : br.BinNo;
                if (!_binLabelMap.ContainsKey(bn) || string.IsNullOrWhiteSpace(_binLabelMap[bn]))
                    _binLabelMap[bn] = br.BinLabel;
            }

            Dictionary<int, int> binCountsSnapshot;
            int total, ok, ng;
            Dictionary<int, string> labelSnapshot;

            lock (_summaryLock)
            {
                binCountsSnapshot = new Dictionary<int, int>(SumContext.BinCounts);
                total = SumContext.TotalCount;
                ok = SumContext.GoodCount;
                ng = SumContext.NGCount;
                labelSnapshot = new Dictionary<int, string>(_binLabelMap);
            }

            string waferId = SafeWaferId(die.SourceWaferId);
            var eqpConfig = Equipment.Instance.EquipmentConfig;

            // 1. Local Write
            string localDir = GetLocalResultDir(waferId);
            Directory.CreateDirectory(localDir);
            string localFile = Path.Combine(localDir, waferId + ".bin");

            string networkDir = ShouldUploadToNetwork() ? GetNetworkResultDir(waferId, eqpConfig.BinResultPath) : null;
            string networkFile = !string.IsNullOrWhiteSpace(networkDir) ? Path.Combine(networkDir, waferId + ".bin") : null;

            bool needUpload = false;

            lock (_ioLock)
            {
                using (var w = new StreamWriter(localFile, false, Encoding.UTF8))
                {
                    w.WriteLine("No,Name,Count");
                    w.WriteLine();

                    var ordered = binCountsSnapshot.Keys
                        .OrderBy(k => k == 0 ? int.MaxValue : k)
                        .ToList();

                    foreach (var no in ordered)
                    {
                        string name;
                        if (!labelSnapshot.TryGetValue(no, out name) || string.IsNullOrWhiteSpace(name))
                            name = (no == 0) ? "Special" : $"SV700-HA-A-{no:00}";

                        int count = binCountsSnapshot[no];
                        w.WriteLine($"{no},{name},{count}");
                    }

                    w.WriteLine();
                    w.WriteLine("[Test Count Info]");
                    w.WriteLine($"Total,{total}");
                    w.WriteLine($"OK,{ok}");
                    w.WriteLine($"NG,{ng}");
                    double goodRate = total > 0 ? (ok * 100.0 / total) : 0.0;
                    w.WriteLine($"Good Rate,{goodRate:0.00}");
                }

                if (!string.IsNullOrWhiteSpace(networkFile))
                {
                    needUpload = true;
                }
            }

            // 2. Queue for Background Upload (Lock 밖에서 실행)
            if (needUpload)
            {
                QueueNetworkUpload(waferId, localFile, networkFile);
            }

            return 0;
        }

        // [수정 1] SumContext의 Deep Copy(복사본)를 생성하는 메서드 추가
        public SUMContext GetSumContextSnapshot()
        {
            lock (_summaryLock)
            {
                var snapshot = new SUMContext
                {
                    EqpName = this.SumContext.EqpName,
                    StartTime = this.SumContext.StartTime,
                    EndTime = this.SumContext.EndTime,
                    WaferID = this.SumContext.WaferID,
                    TotalCount = this.SumContext.TotalCount,
                    GoodCount = this.SumContext.GoodCount,
                    NGCount = this.SumContext.NGCount,
                    ItemNames = new List<string>(this.SumContext.ItemNames),
                    ParameterBlock = new List<string>(this.SumContext.ParameterBlock),
                    ZeroBlock = new List<string>(this.SumContext.ZeroBlock),
                    // 딕셔너리 복사
                    Min = new Dictionary<string, double>(this.SumContext.Min),
                    Max = new Dictionary<string, double>(this.SumContext.Max),
                    Avg = new Dictionary<string, double>(this.SumContext.Avg),
                    Std = new Dictionary<string, double>(this.SumContext.Std),
                    BinCounts = new Dictionary<int, int>(this.SumContext.BinCounts)
                };
                return snapshot;
            }
        }

        // [수정 2] WriteSumFile이 '특정 시점의 스냅샷'을 받아서 쓰도록 오버로딩 혹은 수정
        // 기존 메서드는 유지하되 내부에서 스냅샷을 사용하거나, 외부에서 스냅샷을 넘겨받아야 함.
        // 여기서는 외부(OutputStage)에서 스냅샷을 넘겨주는 방식(WriteSumFileFromSnapshot)을 권장합니다.

        public int WriteSumFileFromSnapshot(MaterialDie die, SUMContext snapshotContext)
        {
            if (snapshotContext == null) return -1;

            string waferId = SafeWaferId(die.SourceWaferId);
            var eqpConfig = Equipment.Instance.EquipmentConfig;

            // 1. Local Write
            string localDir = GetLocalResultDir(waferId);
            try { Directory.CreateDirectory(localDir); } catch { return -1; }

            string localFile = Path.Combine(localDir, waferId + ".sum");

            // Network Path
            string networkDir = ShouldUploadToNetwork() ? GetNetworkResultDir(waferId, eqpConfig.SUMResultPath) : null;
            string networkFile = !string.IsNullOrWhiteSpace(networkDir) ? Path.Combine(networkDir, waferId + ".sum") : null;

            bool needUpload = false;

            lock (_ioLock)
            {
                try
                {
                    // [핵심 변경] FileMode.Append -> FileMode.Create (덮어쓰기)
                    // Summary 파일은 누적 데이터의 최종본이므로 계속 뒤에 붙이는게 아니라 갱신해야 합니다.
                    using (var fs = new FileStream(localFile, FileMode.Create, FileAccess.Write, FileShare.Read))
                    using (var w = new StreamWriter(fs, Encoding.UTF8))
                    {
                        w.WriteLine("EQPName," + snapshotContext.EqpName);
                        w.WriteLine();
                        w.WriteLine("StartTime," + snapshotContext.StartTime);
                        w.WriteLine("EndTime," + snapshotContext.EndTime);
                        w.WriteLine();
                        w.WriteLine("WaferID," + snapshotContext.WaferID);
                        w.WriteLine();
                        w.WriteLine("TotalCount," + snapshotContext.TotalCount);
                        w.WriteLine("GoodCount," + snapshotContext.GoodCount);
                        w.WriteLine("NGCount," + snapshotContext.NGCount);
                        w.WriteLine();

                        w.Write("Item");
                        foreach (var it in snapshotContext.ItemNames) w.Write("," + it);
                        w.WriteLine();

                        WriteSummaryLine(w, "Min", snapshotContext.ItemNames, snapshotContext.Min);
                        WriteSummaryLine(w, "Max", snapshotContext.ItemNames, snapshotContext.Max);
                        WriteSummaryLine(w, "Avg", snapshotContext.ItemNames, snapshotContext.Avg);
                        WriteSummaryLine(w, "Std", snapshotContext.ItemNames, snapshotContext.Std);

                        w.WriteLine();
                        w.WriteLine("BinNo,BinCount");
                        foreach (var kv in snapshotContext.BinCounts)
                            w.WriteLine(kv.Key + "," + kv.Value);

                        w.WriteLine();

                        foreach (var line in snapshotContext.ParameterBlock)
                            w.WriteLine(line);

                        foreach (var line in snapshotContext.ZeroBlock)
                            w.WriteLine(line);
                    }

                    if (!string.IsNullOrWhiteSpace(networkFile))
                    {
                        needUpload = true;
                    }
                }
                catch (Exception ex)
                {
                    Log.Write("ResultWriterManager", "WriteSumFile", ex.Message);
                    return -1;
                }
            }

            // 2. Queue for Background Upload
            if (needUpload)
            {
                QueueNetworkUpload(waferId, localFile, networkFile);
            }

            return 0;
        }

        // [수정] WriteSumFile: Lock 범위 축소 및 안전장치 강화
        public int WriteSumFile(MaterialDie die)
        {

            // 현재 시점의 SumContext 스냅샷 생성
            SUMContext currentSnapshot = GetSumContextSnapshot();
            return WriteSumFileFromSnapshot(die, currentSnapshot);


            //// Lock이 필요 없는 준비 작업
            //FinalizeSummary();

            //string waferId = SafeWaferId(die.SourceWaferId);
            //SyncSumContextFromEquipmentSummaryIfPossible(waferId);

            //var eqpConfig = Equipment.Instance.EquipmentConfig;

            //// 1. Local Write
            //string localDir = GetLocalResultDir(waferId);
            //try { Directory.CreateDirectory(localDir); } catch { return -1; } // 안전장치

            //string localFile = Path.Combine(localDir, waferId + ".sum");

            //string networkDir = ShouldUploadToNetwork() ? GetNetworkResultDir(waferId, eqpConfig.SUMResultPath) : null;
            //string networkFile = !string.IsNullOrWhiteSpace(networkDir) ? Path.Combine(networkDir, waferId + ".sum") : null;

            //bool needUpload = false;

            //lock (_ioLock)
            //{
            //    try
            //    {
            //        using (var fs = new FileStream(localFile, FileMode.Append, FileAccess.Write, FileShare.Read))
            //        using (var w = new StreamWriter(fs, Encoding.UTF8))
            //        //using (var w = new StreamWriter(localFile, false, Encoding.UTF8))
            //        {
            //            w.WriteLine("EQPName," + SumContext.EqpName);
            //            w.WriteLine();
            //            w.WriteLine("StartTime," + SumContext.StartTime);
            //            w.WriteLine("EndTime," + SumContext.EndTime);
            //            w.WriteLine();
            //            w.WriteLine("WaferID," + SumContext.WaferID);
            //            w.WriteLine();
            //            w.WriteLine("TotalCount," + SumContext.TotalCount);
            //            w.WriteLine("GoodCount," + SumContext.GoodCount);
            //            w.WriteLine("NGCount," + SumContext.NGCount);
            //            w.WriteLine();

            //            w.Write("Item");
            //            foreach (var it in SumContext.ItemNames) w.Write("," + it);
            //            w.WriteLine();

            //            WriteSummaryLine(w, "Min", SumContext.ItemNames, SumContext.Min);
            //            WriteSummaryLine(w, "Max", SumContext.ItemNames, SumContext.Max);
            //            WriteSummaryLine(w, "Avg", SumContext.ItemNames, SumContext.Avg);
            //            WriteSummaryLine(w, "Std", SumContext.ItemNames, SumContext.Std);

            //            w.WriteLine();
            //            w.WriteLine("BinNo,BinCount");
            //            foreach (var kv in SumContext.BinCounts)
            //                w.WriteLine(kv.Key + "," + kv.Value);

            //            w.WriteLine();

            //            foreach (var line in SumContext.ParameterBlock)
            //                w.WriteLine(line);

            //            foreach (var line in SumContext.ZeroBlock)
            //                w.WriteLine(line);
            //        }

            //        if (!string.IsNullOrWhiteSpace(networkFile))
            //        {
            //            needUpload = true;
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        Log.Write("ResultWriterManager", "WriteSumFile", ex.Message);
            //        return -1; // 파일 쓰기 실패 시 중단
            //    }
            //}

            //// 2. Queue for Background Upload (Lock 밖에서 실행)
            //if (needUpload)
            //{
            //    QueueNetworkUpload(waferId, localFile, networkFile);
            //}

            //return 0;
        }

        // [수정] BuildPrdHeader: ZeroBlock1 제거 및 ParameterBlock 30줄 적용
        private void BuildPrdHeader(MaterialDie die)
        {
            if (die == null || die.TesterResult == null)
                return;
            if (PrdContext.HeaderInitialized)
                return;

            var dict = GetInternalItemDict(die.TesterResult);
            var itemKeys = new List<string>();
            if (dict != null)
            {
                foreach (var k in dict.Keys)
                {
                    if (string.IsNullOrWhiteSpace(k)) continue;
                    if (!itemKeys.Contains(k))
                        itemKeys.Add(k);
                }
            }

            PrdContext.DataColumns.Clear();
            PrdContext.DataColumns.AddRange(new[] { "XADR", "YADR", "RANK" });
            PrdContext.DataColumns.AddRange(itemKeys);
            PrdContext.DataColumns.Add("Index");

            var waferId = die.SourceWaferId ?? "UNKNOWN";
            var binFile = StripExtension(die.SourceBinFileName);

            string loginId = !string.IsNullOrWhiteSpace(Equipment.Instance.UserId)
                ? Equipment.Instance.UserId
                : ((AccountManager.CurrentAccount != null) ? (AccountManager.CurrentAccount.UserID?.ToString() ?? "OPERATOR") : "OPERATOR");

            PrdContext.HeaderLines.Clear();
            PrdContext.HeaderLines.Add("Filecreationtime," + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            PrdContext.HeaderLines.Add("-100");
            PrdContext.HeaderLines.Add("67");
            PrdContext.HeaderLines.Add("46");
            PrdContext.HeaderLines.Add("-75");
            PrdContext.HeaderLines.Add("1");
            PrdContext.HeaderLines.Add("1");
            PrdContext.HeaderLines.Add("13089");
            PrdContext.HeaderLines.Add(binFile);
            PrdContext.HeaderLines.Add(binFile);
            PrdContext.HeaderLines.Add(waferId);
            PrdContext.HeaderLines.Add(waferId);
            PrdContext.HeaderLines.Add(loginId);
            PrdContext.HeaderLines.Add(SumContext.EqpName);

            // 여기서 ParameterBlock을 30줄로 생성
            BuildTestConditionParameterBlock();

            // [삭제] ZeroBlock1 생성 로직 제거 (ParameterBlock에 통합됨)
            // if (PrdContext.ZeroBlock1.Count == 0) { ... } -> 삭제

            if (PrdContext.ZeroBlock2.Count == 0)
            {
                PrdContext.ZeroBlock2.Add("1");
                for (int i = 0; i < 30; i++)
                    PrdContext.ZeroBlock2.Add("0");
            }

            PrdContext.HeaderInitialized = true;
        }

        private const int PRD_TOTALCOUNT_HEADER_INDEX = 7;

        private void UpdatePrdHeaderTotalCountInFile(string filePath)
        {
            try
            {
                var lines = File.ReadAllLines(filePath, Encoding.UTF8);
                if (lines.Length > PRD_TOTALCOUNT_HEADER_INDEX)
                {
                    string newVal = SumContext.TotalCount.ToString();
                    if (!string.Equals(lines[PRD_TOTALCOUNT_HEADER_INDEX], newVal, StringComparison.Ordinal))
                    {
                        lines[PRD_TOTALCOUNT_HEADER_INDEX] = newVal;
                        File.WriteAllLines(filePath, lines, Encoding.UTF8);
                    }
                }
            }
            catch { }
        }

        // [추가] 웨이퍼 전체 칩 개수를 설정하는 메서드
        public void SetWaferTotalCount(int totalCount)
        {
            lock (_summaryLock)
            {
                // 0보다 클 때만 설정 (혹시 모를 초기화 방지)
                if (totalCount > 0)
                {
                    SumContext.TotalCount = totalCount;
                }
            }
        }


        // [수정] AppendPrdDie: Lock 범위 축소
        public int AppendPrdDie(MaterialDie die)
        {
            if (die == null || die.TesterResult == null)
                return -1;

            string waferId = SafeWaferId(die.SourceWaferId);
            var eqpConfig = Equipment.Instance.EquipmentConfig;

            string localDir = GetLocalResultDir(waferId);
            Directory.CreateDirectory(localDir);
            string localFile = Path.Combine(localDir, waferId + ".prd");

            string networkDir = ShouldUploadToNetwork() ? GetNetworkResultDir(waferId, eqpConfig.PRDResultPath) : null;
            string networkFile = !string.IsNullOrWhiteSpace(networkDir) ? Path.Combine(networkDir, waferId + ".prd") : null;

            var r = die.TesterResult;
            bool needUpload = false;

            lock (_ioLock)
            {
                bool exists = File.Exists(localFile);
                using (var fs = new FileStream(localFile, FileMode.Append, FileAccess.Write, FileShare.Read))
                using (var w = new StreamWriter(fs, Encoding.UTF8))
                {
                    if (!exists)
                    {
                        BuildPrdHeader(die);

                        foreach (var line in PrdContext.HeaderLines) w.WriteLine(line);
                        foreach (var line in PrdContext.ParameterBlock) w.WriteLine(line);
                        foreach (var line in PrdContext.ZeroBlock2) w.WriteLine(line);

                        w.WriteLine(string.Join(",", PrdContext.DataColumns));
                    }
                    else
                    {
                        if (!PrdContext.HeaderInitialized)
                            BuildPrdHeader(die);
                    }

                    int rank = r.BinningResult != null ? r.BinningResult.BinNo : die.Rank;
                    var itemDict = GetInternalItemDict(r) ?? new Dictionary<string, TestItemResult>();
                    var sb = new StringBuilder(256);

                    foreach (var col in PrdContext.DataColumns)
                    {
                        if (sb.Length > 0) sb.Append(',');
                        switch (col)
                        {
                            case "XADR": sb.Append(die.MapX); break;
                            case "YADR": sb.Append(die.MapY); break;
                            case "RANK": sb.Append(rank); break;
                            case "Index": sb.Append(die.SocketIndex + 1); break;
                            default:
                                if (itemDict.TryGetValue(col, out var ti) && ti != null) sb.Append(ti.Value);
                                else sb.Append("0");
                                break;
                        }
                    }

                    w.WriteLine(sb.ToString());
                }

                if (exists)
                {
                    if (PrdContext.HeaderLines.Count > PRD_TOTALCOUNT_HEADER_INDEX)
                        PrdContext.HeaderLines[PRD_TOTALCOUNT_HEADER_INDEX] = SumContext.TotalCount.ToString();

                    UpdatePrdHeaderTotalCountInFile(localFile);
                }

                if (!string.IsNullOrWhiteSpace(networkFile))
                {
                    needUpload = true;
                }
            }

            // 2. Queue for Background Upload (Lock 밖에서 실행)
            if (needUpload)
            {
                QueueNetworkUpload(waferId, localFile, networkFile);
            }

            return 0;
        }

        private void BuildWafHeader(MaterialDie die)
        {
            if (die == null || die.TesterResult == null)
                return;

            if (WafContext.HeaderInitialized)
                return;

            // [추가] 안전장치: PRD보다 먼저 불릴 경우 ParameterBlock 생성
            if (WafContext.ParameterBlock.Count == 0)
            {
                BuildTestConditionParameterBlock();
            }

            var dict = GetInternalItemDict(die.TesterResult);
            if (dict != null)
            {
                foreach (var k in dict.Keys)
                {
                    if (string.IsNullOrWhiteSpace(k)) continue;
                    if (!WafContext.DataColumns.Contains(k))
                        WafContext.DataColumns.Add(k);
                }
            }

            WafContext.HeaderLines.Clear();
            WafContext.HeaderLines.Add("-100");
            WafContext.HeaderLines.Add("67");
            WafContext.HeaderLines.Add("46");
            WafContext.HeaderLines.Add("-75");
            WafContext.HeaderLines.Add("1");
            WafContext.HeaderLines.Add("1");
            WafContext.HeaderLines.Add(SumContext.TotalCount > 0 ? SumContext.TotalCount.ToString() : "13089");

            string productName = StripExtension(die.SourceBinFileName);
            string waferId = SafeWaferId(die.SourceWaferId);

            string loginId = !string.IsNullOrWhiteSpace(Equipment.Instance.UserId)
                ? Equipment.Instance.UserId
                : ((AccountManager.CurrentAccount?.UserID ?? "OPERATOR").ToString());

            string eqpName = (SumContext.EqpName ?? "LPC-280").Replace("EqpName,", "");

            WafContext.HeaderLines.Add(productName);
            WafContext.HeaderLines.Add(productName);
            WafContext.HeaderLines.Add(waferId);
            WafContext.HeaderLines.Add(waferId);
            WafContext.HeaderLines.Add(loginId);
            WafContext.HeaderLines.Add(eqpName);

            // [삭제] ZeroBlock1 생성 로직 제거
            // if (WafContext.ZeroBlock1.Count == 0) { ... } -> 삭제

            if (WafContext.ZeroBlock2.Count == 0)
            {
                WafContext.ZeroBlock2.Add("1");
                for (int i = 0; i < 30; i++)
                    WafContext.ZeroBlock2.Add("0");
            }

            WafContext.HeaderInitialized = true;
        }

        // [수정] AppendWafDie: Lock 범위 축소
        public int AppendWafDie(MaterialDie die)
        {
            if (die == null || die.TesterResult == null)
                return -1;

            string waferId = SafeWaferId(die.SourceWaferId);
            var eqpConfig = Equipment.Instance.EquipmentConfig;

            string localDir = GetLocalResultDir(waferId);
            Directory.CreateDirectory(localDir);
            string localFile = Path.Combine(localDir, waferId + ".waf");

            string networkDir = ShouldUploadToNetwork() ? GetNetworkResultDir(waferId, eqpConfig.WAFResultPath) : null;
            string networkFile = !string.IsNullOrWhiteSpace(networkDir) ? Path.Combine(networkDir, waferId + ".waf") : null;

            PKGTesterResult r = die.TesterResult;
            double height = die.GetMeasure("Height").HasValue ? die.GetMeasure("Height").Value : 0.0;

            bool needUpload = false;

            lock (_ioLock)
            {
                bool exists = File.Exists(localFile);

                if (!WafContext.HeaderInitialized)
                    BuildWafHeader(die);

                using (var fs = new FileStream(localFile, FileMode.Append, FileAccess.Write, FileShare.Read))
                using (var w = new StreamWriter(fs, Encoding.UTF8))
                {
                    if (!exists)
                    {
                        foreach (var line in WafContext.HeaderLines) w.WriteLine(line);
                        foreach (var line in WafContext.ParameterBlock) w.WriteLine(line);
                        foreach (var line in WafContext.ZeroBlock2) w.WriteLine(line);
                    }

                    var dict = GetInternalItemDict(r) ?? new Dictionary<string, TestItemResult>(StringComparer.OrdinalIgnoreCase);

                    var sb = new StringBuilder(256);
                    sb.Append(die.MapX.ToString(CultureInfo.InvariantCulture));
                    sb.Append(',');
                    sb.Append(die.MapY.ToString(CultureInfo.InvariantCulture));

                    foreach (var key in WafContext.DataColumns)
                    {
                        sb.Append(',');
                        if (dict.TryGetValue(key, out var item) && item != null)
                            sb.Append(item.Value.ToString("0.#####", CultureInfo.InvariantCulture));
                        else
                            sb.Append("0");
                    }

                    sb.Append(',');
                    sb.Append((die.SocketIndex + 1).ToString(CultureInfo.InvariantCulture));
                    sb.Append(',');
                    sb.Append(height.ToString("0.000", CultureInfo.InvariantCulture));

                    w.WriteLine(sb.ToString());
                }

                if (!string.IsNullOrWhiteSpace(networkFile))
                {
                    needUpload = true;
                }
            }

            // 2. Queue for Background Upload (Lock 밖에서 실행)
            if (needUpload)
            {
                QueueNetworkUpload(waferId, localFile, networkFile);
            }

            return 0;
        }


        #endregion

        // =========================================================
        // ===================== Logging (CSV) ======================
        // =========================================================
        #region Internal (Logging Unification)
        public int WriteCsvLog(MaterialDie die, PKGTesterResult result, List<string> sgKeys)
        {
            string waferId = SafeWaferId(die.SourceWaferId);
            string file = Path.Combine(LogDir, waferId + "_" + DateTime.Now.ToString("yyyyMMdd") + ".csv");

            lock (_ioLock)
            {
                bool exists = File.Exists(file);
                using (var w = new StreamWriter(file, true, Encoding.UTF8))
                {
                    if (!exists)
                    {
                        w.Write("Time,SocketNo,DieNo,DiePosX,DiePosY,BinNo,BinType,BinLabel");
                        foreach (var key in EnumerateItemKeys(result)) w.Write("," + key);
                        foreach (var sg in sgKeys) w.Write("," + sg);
                        w.WriteLine();
                    }

                    var bin = result.BinningResult;
                    w.Write(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                    w.Write("," + (die.SocketIndex + 1));
                    w.Write("," + (die.Index + 1));
                    w.Write("," + (die.MapX));
                    w.Write("," + (die.MapY));
                    w.Write("," + (bin != null ? bin.BinNo.ToString() : ""));
                    w.Write("," + (bin != null ? ((int)bin.BinType).ToString() : ""));
                    w.Write("," + (bin != null ? (bin.BinLabel ?? "") : ""));
                    foreach (var v in EnumerateItemValues(result)) w.Write("," + v);

                    foreach (var sg in sgKeys)
                    {
                        double v = 0;
                        die.MeasureValues.TryGetValue(sg, out v);
                        w.Write("," + v);
                    }
                    w.WriteLine();
                }
            }

            return 0;
        }
        #endregion

        // =========================================================
        // ===================== Shared Helpers =====================
        // =========================================================
        #region Shared Helpers
        private IDictionary<string, TestItemResult> GetInternalItemDict(PKGTesterResult result)
        {
            if (result == null) return null;

            var prop = result.GetType().GetProperty("Items", BindingFlags.Public | BindingFlags.Instance);
            if (prop != null)
            {
                var dict = prop.GetValue(result, null) as IDictionary<string, TestItemResult>;
                if (dict != null) return dict;
            }

            var field = result.GetType().GetField("items", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                var dict = field.GetValue(result) as IDictionary<string, TestItemResult>;
                if (dict != null) return dict;
            }

            return null;
        }

        private IEnumerable<string> EnumerateItemKeys(PKGTesterResult result)
        {
            var dict = GetInternalItemDict(result);
            if (dict != null)
            {
                foreach (var k in dict.Keys)
                {
                    if (!string.IsNullOrEmpty(k))
                        yield return k;
                }
            }
        }

        private IEnumerable<double> EnumerateItemValues(PKGTesterResult result)
        {
            var dict = GetInternalItemDict(result);
            if (dict != null)
            {
                foreach (var kv in dict)
                {
                    if (kv.Value != null)
                        yield return kv.Value.Value;
                }
            }
        }

        private void WriteSummaryLine(StreamWriter w, string label, List<string> items, Dictionary<string, double> map)
        {
            w.Write(label);
            foreach (var it in items)
            {
                if (!map.TryGetValue(it, out double v)) v = 0.0;
                w.Write("," + v);
            }
            w.WriteLine();
        }

        private void SyncSumContextFromEquipmentSummaryIfPossible(string waferId)
        {
            try
            {
                var eq = Equipment.Instance;
                var sc = eq?.SummaryContext;
                if (sc == null)
                    return;

                var row = sc.GetSnapshotOrNull();
                if (row == null)
                    return;

                var safeReq = SafeWaferId(waferId);
                var safeRow = SafeWaferId(row.WaferId);
                if (!string.IsNullOrWhiteSpace(safeReq) &&
                    !string.Equals(safeReq, safeRow, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                if (!string.IsNullOrWhiteSpace(row.MachineName))
                    SumContext.EqpName = row.MachineName;

                if (!string.IsNullOrWhiteSpace(row.WaferId))
                    SumContext.WaferID = row.WaferId;

                if (row.Start != DateTime.MinValue)
                    SumContext.StartTime = row.Start.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

                if (row.End != DateTime.MinValue)
                    SumContext.EndTime = row.End.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            }
            catch { }
        }
        #endregion

        // =========================================================
        // ================= Summary / Accumulation =================
        // =========================================================
        public void ResultLogData_BeginWaferSummary(string waferId, string eqpName = "UNKNOWN")
        {
            lock (_summaryLock)
            {
                SumContext.WaferID = string.IsNullOrEmpty(waferId) ? "UNKNOWN" : waferId.Trim();
                SumContext.EqpName = eqpName ?? "UNKNOWN";

                SumContext.StartTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                SumContext.EndTime = "";
                SyncSumContextFromEquipmentSummaryIfPossible(waferId);

                SumContext.TotalCount = 0;
                SumContext.GoodCount = 0;
                SumContext.NGCount = 0;
                SumContext.ItemNames.Clear();
                SumContext.Min.Clear();
                SumContext.Max.Clear();
                SumContext.Avg.Clear();
                SumContext.Std.Clear();
                SumContext.BinCounts.Clear();
                SumContext.ParameterBlock.Clear();
                SumContext.ZeroBlock.Clear();
                _sum.Clear();
                _sumSq.Clear();
                _summaryFinalized = false;

                ResetPrdHeaderIfWaferChanged(waferId);
                ResetWafHeaderIfWaferChanged(waferId);

                lock (_waferTotalSummaryOnceLock)
                {
                    var safeId = SafeWaferId(waferId);
                    if (!string.IsNullOrWhiteSpace(safeId))
                        _waferTotalSummaryWritten.Remove(safeId);
                }
            }
        }

        public void AccumulateDie(MaterialDie die)
        {
            if (die == null || die.TesterResult == null) 
                return;

            lock (_summaryLock)
            {
                var itemsDict = GetInternalItemDict(die.TesterResult);
                if (itemsDict == null) 
                    return;

                if (die.IsPass)
                {
                    SumContext.GoodCount++; 
                }
                else
                {
                    SumContext.NGCount++;
                }

                int rawBinNo = -1;
                string rawLabel = null;
                var br = die.TesterResult.BinningResult;
                if (br != null)
                {
                    rawBinNo = br.BinNo;
                    rawLabel = br.BinLabel;
                }

                int storeBinNo = rawBinNo < 0 ? 0 : rawBinNo;

                SumContext.BinCounts.TryGetValue(storeBinNo, out int cur);
                SumContext.BinCounts[storeBinNo] = cur + 1;

                if (!string.IsNullOrWhiteSpace(rawLabel))
                {
                    if (!_binLabelMap.ContainsKey(storeBinNo))
                        _binLabelMap[storeBinNo] = rawLabel;
                    else if (string.IsNullOrWhiteSpace(_binLabelMap[storeBinNo]))
                        _binLabelMap[storeBinNo] = rawLabel;
                }

                foreach (var kv in itemsDict)
                {
                    string key = kv.Key;
                    if (key == null || kv.Value == null) continue;
                    double val = kv.Value.Value;

                    if (!SumContext.ItemNames.Contains(key))
                        SumContext.ItemNames.Add(key);

                    if (!SumContext.Min.TryGetValue(key, out double min) || val < min)
                        SumContext.Min[key] = val;

                    if (!SumContext.Max.TryGetValue(key, out double max) || val > max)
                        SumContext.Max[key] = val;

                    _sum.TryGetValue(key, out double s);
                    _sum[key] = s + val;

                    _sumSq.TryGetValue(key, out double s2);
                    _sumSq[key] = s2 + (val * val);
                }
            }
        }

        public void FinalizeSummary()
        {
            lock (_summaryLock)
            {
                if (_summaryFinalized)
                    return;

                int n = SumContext.TotalCount;
                if (n <= 0)
                {
                    _summaryFinalized = true;
                    return;
                }

                foreach (var key in SumContext.ItemNames)
                {
                    _sum.TryGetValue(key, out double sum);
                    _sumSq.TryGetValue(key, out double sumSq);

                    double avg = sum / n;
                    double variance = (sumSq / n) - (avg * avg);
                    if (variance < 0) variance = 0;
                    double std = Math.Sqrt(variance);

                    SumContext.Avg[key] = avg;
                    SumContext.Std[key] = std;
                }

                SumContext.EndTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                SyncSumContextFromEquipmentSummaryIfPossible(SumContext.WaferID);

                _summaryFinalized = true;
            }
        }

        // =========================================================
        // ========== TestConditionSet -> ParameterBlock ============
        // =========================================================
        private void BuildTestConditionParameterBlock()
        {
            PrdContext.ParameterBlock.Clear();
            WafContext.ParameterBlock.Clear();
            SumContext.ParameterBlock.Clear();

            int seq = 1;
            int addedCount = 0;

            // 1. 실제 아이템 추가
            if (CurrentTestConditionSet != null && CurrentTestConditionSet.Items != null)
            {
                foreach (var it in CurrentTestConditionSet.Items)
                {
                    double srcVal = it.SourceValue;
                    double srcTime = it.SourceTime;

                    (double low, double high) = TryExtractRange(it.Expression);
                    double gain0 = (it.Gain != null && it.Gain.Length > 0) ? it.Gain[0] : 0.0;
                    double offset0 = (it.Offset != null && it.Offset.Length > 0) ? it.Offset[0] : 0.0;
                    int typeCode = (int)it.Type;

                    string line =
                        seq.ToString(CultureInfo.InvariantCulture) + " " +
                        typeCode.ToString(CultureInfo.InvariantCulture) + " " +
                        srcVal.ToString("0.#####", CultureInfo.InvariantCulture) + " " +
                        srcTime.ToString("0.#####", CultureInfo.InvariantCulture) + " " +
                        "0" + " " +
                        low.ToString("0.#####", CultureInfo.InvariantCulture) + " " +
                        high.ToString("0.#####", CultureInfo.InvariantCulture) + " " +
                        gain0.ToString("0.#####", CultureInfo.InvariantCulture) + " " +
                        offset0.ToString("0.#####", CultureInfo.InvariantCulture);

                    PrdContext.ParameterBlock.Add(line);
                    WafContext.ParameterBlock.Add(line);
                    SumContext.ParameterBlock.Add(line);

                    seq++;
                    addedCount++;
                }
            }

            // 2. [변경] 총 30줄이 될 때까지 더미 라인 채우기 (기존 ZeroBlock1 영역 통합)
            const int TARGET_PARAM_LINES = 30;
            // 빈 줄 포맷 (기존 ZeroBlock1과 동일한 포맷 사용)
            const string DUMMY_LINE = "0 0 0 0 0 0.00 0.00 0 0.00";

            while (addedCount < TARGET_PARAM_LINES)
            {
                PrdContext.ParameterBlock.Add(DUMMY_LINE);
                WafContext.ParameterBlock.Add(DUMMY_LINE);
                SumContext.ParameterBlock.Add(DUMMY_LINE);
                addedCount++;
            }




            //PrdContext.ParameterBlock.Clear();

            //if (CurrentTestConditionSet == null || CurrentTestConditionSet.Items == null || CurrentTestConditionSet.Items.Count == 0)
            //    return;

            //int seq = 1;
            //foreach (var it in CurrentTestConditionSet.Items)
            //{
            //    double srcVal = it.SourceValue;
            //    double srcTime = it.SourceTime;

            //    (double low, double high) = TryExtractRange(it.Expression);
            //    double gain0 = (it.Gain != null && it.Gain.Length > 0) ? it.Gain[0] : 0.0;
            //    double offset0 = (it.Offset != null && it.Offset.Length > 0) ? it.Offset[0] : 0.0;
            //    int typeCode = (int)it.Type;

            //    string line =
            //        seq.ToString(CultureInfo.InvariantCulture) + " " +
            //        typeCode.ToString(CultureInfo.InvariantCulture) + " " +
            //        srcVal.ToString("0.#####", CultureInfo.InvariantCulture) + " " +
            //        srcTime.ToString("0.#####", CultureInfo.InvariantCulture) + " " +
            //        "0" + " " +
            //        low.ToString("0.#####", CultureInfo.InvariantCulture) + " " +
            //        high.ToString("0.#####", CultureInfo.InvariantCulture) + " " +
            //        gain0.ToString("0.#####", CultureInfo.InvariantCulture) + " " +
            //        offset0.ToString("0.#####", CultureInfo.InvariantCulture);

            //    PrdContext.ParameterBlock.Add(line);
            //    WafContext.ParameterBlock.Add(line);
            //    SumContext.ParameterBlock.Add(line);

            //    seq++;
            //}
        }

        private (double low, double high) TryExtractRange(string expr)
        {
            if (string.IsNullOrWhiteSpace(expr))
                return (0, 0);

            var tildeParts = expr.Split('~');
            if (tildeParts.Length == 2 &&
                double.TryParse(tildeParts[0].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out double low1) &&
                double.TryParse(tildeParts[1].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out double high1))
            {
                return (low1, high1);
            }

            var tokens = expr.Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries)
                             .Where(t => double.TryParse(t, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                             .ToList();
            if (tokens.Count >= 2 &&
                double.TryParse(tokens[0], NumberStyles.Any, CultureInfo.InvariantCulture, out double low2) &&
                double.TryParse(tokens[1], NumberStyles.Any, CultureInfo.InvariantCulture, out double high2))
            {
                return (low2, high2);
            }

            return (0, 0);
        }

        // =========================================================
        // =================== Header Reset Helpers =================
        // =========================================================
        private void ResetPrdHeaderIfWaferChanged(string waferId)
        {
            waferId = SafeWaferId(waferId);
            lock (_headerLock)
            {
                if (!string.Equals(_lastPrdWaferId, waferId, StringComparison.OrdinalIgnoreCase))
                {
                    if (!string.IsNullOrEmpty(_lastPrdWaferId))
                        FinalizeSummary();

                    PrdContext.HeaderInitialized = false;
                    PrdContext.HeaderLines.Clear();
                    PrdContext.ParameterBlock.Clear();
                    PrdContext.ZeroBlock1.Clear();
                    PrdContext.ZeroBlock2.Clear();
                    PrdContext.DataColumns.Clear();

                    _lastPrdWaferId = waferId;

                    ResultLogData_BeginWaferSummary(waferId, SumContext.EqpName);
                }
            }
        }

        private void ResetWafHeaderIfWaferChanged(string waferId)
        {
            waferId = SafeWaferId(waferId);
            lock (_headerLock)
            {
                if (!string.Equals(_lastWafWaferId, waferId, StringComparison.OrdinalIgnoreCase))
                {
                    WafContext.HeaderInitialized = false;
                    WafContext.HeaderLines.Clear();
                    WafContext.ParameterBlock.Clear();
                    WafContext.ZeroBlock1.Clear();
                    WafContext.ZeroBlock2.Clear();
                    WafContext.DataColumns.Clear();
                    _lastWafWaferId = waferId;
                }
            }
        }

        // =========================================================
        // ======= (Optional) WaferTotalSummary / ProdSummary ========
        // =========================================================
        // 사용자가 "여기 전부 지워도 됨" 이라고 했던 블록은 그대로 유지 가능하지만,
        // "B안만 남긴 정리본" 요구에 맞춰 본 파일에서는 생략했습니다.
        //
        // 만약 해당 ProductionInfo / WaferTotalSummary 기능도 그대로 필요하면,
        // 기존 코드 블록을 이 클래스 하단에 그대로 붙여 넣으면 됩니다.


        //여기 전부 지워도 됨.
        // ======== Production Summary CSV (YYYYMMWaferTotalOUT_DB.csv) ========
        // 생산요약 한 줄 데이터 모델
        public class ProductionSummaryRow
        {
            public DateTime Date { get; set; }                 // yyyy-MM-dd
            public string EquipmentName { get; set; }          // 설비명 (예: VA1VPRO03)
            public string Model { get; set; }                  // 모델명
            public TimeSpan DayProductionTime { get; set; }    // 주간생산시간 (HH:mm:ss)
            public int DayCount { get; set; }                  // 주간 수량
            public TimeSpan NightProductionTime { get; set; }  // 야간생산시간 (HH:mm:ss)
            public int NightCount { get; set; }                // 야간 수량
            public double YieldPercent { get; set; }           // 수율(예: 99.84)
            public TimeSpan TotalProductionTime { get; set; }  // Total 생산시간 (HH:mm:ss)
            public int TotalCount { get; set; }                // Total 수량
        }

        // 파일명 규칙: YYYYMM + "WaferTotalOUT_DB.csv"
        // 경로: EquipmentConfig.ProductionInfoPath(있으면 우선, 파일 또는 디렉터리 둘 다 허용) → 없으면 BaseDir
        private string GetProductionInfoFilePath()
        {
            string ym = DateTime.Now.ToString("yyyyMM", CultureInfo.InvariantCulture);
            string fileName = ym + "WaferTotalOUT_DB.csv";
            string FilePath = "";
            try
            {
                var cfg = Equipment.Instance?.EquipmentConfig;
                string path = cfg?.ProductionInfoPath;

                if (cfg.NetworkMode == 0)
                {
                    //BaseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ProductionInfo", _runDate);
                    FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ProductionInfo", fileName);
                    return FilePath;
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(path))
                    {
                        // 디렉터리로 지정된 경우
                        if (Directory.Exists(path))
                        {
                            return Path.Combine(path, fileName);
                        }

                        // 파일 경로로 지정된 경우 → 동일 폴더에 규칙 파일명으로 저장
                        string dir = Path.GetDirectoryName(path);
                        if (!string.IsNullOrWhiteSpace(dir))
                        {
                            // 디렉터리가 없으면 생성
                            if (!Directory.Exists(dir))
                                Directory.CreateDirectory(dir);

                            return Path.Combine(dir, fileName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write("ResultWriterManager", "GetProductionInfoFilePath", ex.Message);
                // fall through to BaseDir
            }

            // 기본 경로: 날짜 폴더(BaseDir)
            return Path.Combine(BaseDir, fileName);
        }

        // HH:mm:ss 고정 포맷 (누적 24시간 초과 시에도 시간 누적 표시)
        private static string FormatTimeSpan(TimeSpan ts)
        {
            long totalSeconds = (long)Math.Max(0, ts.TotalSeconds);
            int h = (int)(totalSeconds / 3600);
            int m = (int)((totalSeconds % 3600) / 60);
            int s = (int)(totalSeconds % 60);
            return $"{h:00}:{m:00}:{s:00}";
        }

        /// <summary>
        /// 생산요약 CSV에 행 추가. 파일이 없으면 헤더를 먼저 씁니다.
        /// 컬럼: 날짜,설비명,Model,주간생산시간,주간,야간생산시간,야간,수율,Total,TotalCount
        /// 파일명: YYYYMMWaferTotalOUT_DB.csv
        /// </summary>
        public int AppendProductionSummaryRow(ProductionSummaryRow row)
        {
            if (row == null) return -1;

            string file = GetProductionInfoFilePath();
            string dir = Path.GetDirectoryName(file);
            if (string.IsNullOrWhiteSpace(dir))
                dir = BaseDir;
            Directory.CreateDirectory(dir);

            lock (_ioLock)
            {
                bool exists = File.Exists(file);
                using (var w = new StreamWriter(file, true, Encoding.UTF8))
                {
                    if (!exists)
                    {
                        // 헤더 1회
                        w.WriteLine("날짜,설비명,Model,주간생산시간,주간,야간생산시간,야간,수율,Total,TotalCount");
                    }

                    // 값 라인
                    string date = row.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                    string eqp = row.EquipmentName ?? string.Empty;
                    string model = row.Model ?? string.Empty;
                    string dayTime = FormatTimeSpan(row.DayProductionTime);
                    string nightTime = FormatTimeSpan(row.NightProductionTime);
                    string totalTime = FormatTimeSpan(row.TotalProductionTime);
                    string yield = row.YieldPercent.ToString("0.##", CultureInfo.InvariantCulture);

                    w.WriteLine(string.Join(",",
                        date,
                        eqp,
                        model,
                        dayTime,
                        row.DayCount.ToString(CultureInfo.InvariantCulture),
                        nightTime,
                        row.NightCount.ToString(CultureInfo.InvariantCulture),
                        yield,
                        totalTime,
                        row.TotalCount.ToString(CultureInfo.InvariantCulture)
                    ));
                }
            }
            return 0;
        }

        // ======== End of Production Summary CSV ========
        // ============================================


        // ======== Wafer Total Summary CSV (YYYYMMWaferTotalSummaryData.csv) ========

        // ===== WaferTotalSummary 중복 기록 방지 =====
        //private readonly object _waferTotalSummaryOnceLock = new object();
        //private readonly HashSet<string> _waferTotalSummaryWritten = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // 파일명 규칙: YYYYMM + "WaferTotalSummaryData.csv"
        private static string BuildWaferTotalSummaryTopLine(DateTime date)
        {
            // 예시: ",11/ 1,,,,,,,,기준,11/ 1 00:00~24:00,"
            // 월/일 표현에서 일은 공백 패딩이 들어가므로 (11/ 1) 형태로 맞춤.
            string md = date.ToString("M", CultureInfo.InvariantCulture); // "11/1"
            md = md.Replace("/", "/ "); // "11/ 1" 같이 보이도록 유사하게
            return "," + md + ",,,,,,,,,기준," + md + " 00:00~24:00,";
        }

        private static readonly string[] WaferTotalSummaryColumns = new[]
        {
            "DATE","MachineName","WAFERID","BINID","START","END",
            "Total Time","Run Time","Down Time","Scan Time","Ld Time","ULd Time","SortTime",
            "AlarmCnt",
            "Total Count","Scan Count","Out Count","Miss Count","Scan NG","OutSide",
            "WaferVision","AlignVision","IndexVision","Contact",
            "Ld Pick","Ld Place","ULd Pick","ULd Place",
            "C/T",
            "Total NG","Contact Retry",
            "Yield","UPH","UPD",
            "Picker1","Picker2","Picker3","Picker4","Picker5","Picker6","Picker7","Picker8"
        };

        private string GetWaferTotalSummaryFilePath()
        {
            string ym = DateTime.Now.ToString("yyyyMM", CultureInfo.InvariantCulture);
            string fileName = ym + "WaferTotalSummaryData.csv";

            try
            {
                var cfg = Equipment.Instance?.EquipmentConfig;
                string path = cfg?.ProductionInfoPath;

                // 네트워크 모드 0이면 로컬 ProductionInfo로 (기존 패턴 유지)
                if (cfg != null && cfg.NetworkMode == 0)
                {
                    return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ProductionInfo", fileName);
                }

                if (!string.IsNullOrWhiteSpace(path))
                {
                    if (Directory.Exists(path))
                        return Path.Combine(path, fileName);

                    string dir = Path.GetDirectoryName(path);
                    if (!string.IsNullOrWhiteSpace(dir))
                    {
                        if (!Directory.Exists(dir))
                            Directory.CreateDirectory(dir);

                        return Path.Combine(dir, fileName);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }

            return Path.Combine(BaseDir, fileName);
        }
        private string GetWaferTotalSummaryLocalFilePath()
        {
            string ym = DateTime.Now.ToString("yyyyMM", CultureInfo.InvariantCulture);
            string fileName = ym + "WaferTotalSummaryData.csv";

            // 네트워크 모드와 무관하게 항상 로컬 고정 저장
            string localDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ProductionInfo");
            Directory.CreateDirectory(localDir);
            return Path.Combine(localDir, fileName);
        }

        // (선택) 네트워크 경로가 필요할 때만 사용
        private string GetWaferTotalSummaryNetworkFilePath()
        {
            try
            {
                var cfg = Equipment.Instance?.EquipmentConfig;
                if (cfg == null) return null;
                if (cfg.NetworkMode <= 0) return null;

                // 기존 설정(ProductionInfoPath)을 네트워크 목적지로 사용 (디렉터리/파일 둘 다 허용)
                string path = cfg.ProductionInfoPath;
                if (string.IsNullOrWhiteSpace(path)) return null;

                string ym = DateTime.Now.ToString("yyyyMM", CultureInfo.InvariantCulture);
                string fileName = ym + "WaferTotalSummaryData.csv";

                if (Directory.Exists(path))
                    return Path.Combine(path, fileName);

                string dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrWhiteSpace(dir))
                    return Path.Combine(dir, fileName);

                return null;
            }
            catch
            {
                return null;
            }
        }


        // 예시 시간 포맷: 00:40:38 (24시간 초과 가능)
        private static string FormatTimeSpanHms(TimeSpan ts)
        {
            long totalSeconds = (long)Math.Max(0, ts.TotalSeconds);
            int h = (int)(totalSeconds / 3600);
            int m = (int)((totalSeconds % 3600) / 60);
            int s = (int)(totalSeconds % 60);
            return $"{h:00}:{m:00}:{s:00}";
        }

        private static string FormatDateMmDd(DateTime dt) => dt.ToString("MM-dd", CultureInfo.InvariantCulture);
        private static string FormatTimeHms(DateTime dt) => dt.ToString("HH:mm:ss", CultureInfo.InvariantCulture);

        private static string Csv(string v)
        {
            if (v == null) return "";
            if (v.IndexOfAny(new[] { ',', '"', '\n', '\r' }) >= 0)
                return "\"" + v.Replace("\"", "\"\"") + "\"";
            return v;
        }

        public int AppendWaferTotalSummaryRow(QMC.LCP_280.Process.Component.WaferSummary.WaferTotalSummaryRow row)
        {
            if (row == null) return -1;

            // 1) 항상 로컬 파일로 저장
            string localFile = GetWaferTotalSummaryLocalFilePath();

            lock (_ioLock)
            {
                bool exists = File.Exists(localFile);
                using (var fs = new FileStream(localFile, FileMode.Append, FileAccess.Write, FileShare.Read))
                using (var w = new StreamWriter(fs, Encoding.UTF8))
                {
                    if (!exists)
                    {
                        w.WriteLine(BuildWaferTotalSummaryTopLine(row.Date));
                        w.WriteLine(string.Join(",", WaferTotalSummaryColumns));
                    }

                    var pickers = row.Pickers ?? new string[0];
                    string GetPicker(int idx) => (idx < pickers.Length && !string.IsNullOrWhiteSpace(pickers[idx])) ? pickers[idx] : "USE";

                    string yieldStr = row.Yield.ToString("0.##", CultureInfo.InvariantCulture);
                    string ctStr = row.CycleTime.ToString("0.####", CultureInfo.InvariantCulture);

                    w.WriteLine(string.Join(",",
                        FormatDateMmDd(row.Date),
                        Csv(row.MachineName ?? ""),
                        Csv(row.WaferId ?? ""),
                        Csv(row.BinId ?? ""),
                        FormatTimeHms(row.Start),
                        FormatTimeHms(row.End),

                        FormatTimeSpanHms(row.TotalTime),
                        FormatTimeSpanHms(row.RunTime),
                        FormatTimeSpanHms(row.DownTime),
                        FormatTimeSpanHms(row.ScanTime),
                        FormatTimeSpanHms(row.LoadTime),
                        FormatTimeSpanHms(row.UnloadTime),
                        FormatTimeSpanHms(row.SortTime),

                        row.AlarmCount.ToString(CultureInfo.InvariantCulture),

                        row.TotalCount.ToString(CultureInfo.InvariantCulture),
                        row.ScanCount.ToString(CultureInfo.InvariantCulture),
                        row.OutCount.ToString(CultureInfo.InvariantCulture),
                        row.MissCount.ToString(CultureInfo.InvariantCulture),
                        row.ScanNg.ToString(CultureInfo.InvariantCulture),
                        row.OutSide.ToString(CultureInfo.InvariantCulture),

                        row.WaferVision.ToString(CultureInfo.InvariantCulture),
                        row.AlignVision.ToString(CultureInfo.InvariantCulture),
                        row.IndexVision.ToString(CultureInfo.InvariantCulture),
                        row.Contact.ToString(CultureInfo.InvariantCulture),

                        row.LdPick.ToString(CultureInfo.InvariantCulture),
                        row.LdPlace.ToString(CultureInfo.InvariantCulture),
                        row.ULdPick.ToString(CultureInfo.InvariantCulture),
                        row.ULdPlace.ToString(CultureInfo.InvariantCulture),

                        ctStr,

                        row.TotalNg.ToString(CultureInfo.InvariantCulture),
                        row.ContactRetry.ToString(CultureInfo.InvariantCulture),

                        yieldStr,
                        row.Uph.ToString(CultureInfo.InvariantCulture),
                        row.Upd.ToString(CultureInfo.InvariantCulture),

                        Csv(GetPicker(0)), Csv(GetPicker(1)), Csv(GetPicker(2)), Csv(GetPicker(3)),
                        Csv(GetPicker(4)), Csv(GetPicker(5)), Csv(GetPicker(6)), Csv(GetPicker(7))
                    ));
                }

                // 2) 네트워크 모드일 때만 Outbox로 추가 업로드(덮어쓰기)
                string networkFile = GetWaferTotalSummaryNetworkFilePath();
                if (!string.IsNullOrWhiteSpace(networkFile))
                {
                    QueueNetworkUpload(row.WaferId, localFile, networkFile);
                }
            }

            return 0;
        }

        //SFTP 설정/모드

        public enum RemoteUploadMode
        {
            Disabled = 0,
            SmbShare = 1,   // 기존 network path copy
            Sftp = 2        // 신규 SFTP 업로드
        }

        public enum SftpAuthMode
        {
            Password = 0,
            PrivateKey = 1
        }

        public sealed class SftpUploadSettings
        {
            public string Host { get; set; }
            public int Port { get; set; } = 22;
            public string Username { get; set; }

            // auth 선택
            public SftpAuthMode AuthMode { get; set; } = SftpAuthMode.Password;

            // Password 방식
            public string Password { get; set; }

            // PrivateKey 방식
            public string PrivateKeyFile { get; set; }       // 예: C:\keys\id_rsa
            public string PrivateKeyPassphrase { get; set; } // optional

            // 우선 고정
            public string RemoteBaseDir { get; set; } = "/upload/LCP-280";

            // 튜닝
            public int ConnectTimeoutMs { get; set; } = 5000;
        }

        public RemoteUploadMode UploadMode { get; set; } = RemoteUploadMode.SmbShare; // 기본: 기존 방식 유지
        public SftpUploadSettings SftpSettings { get; set; } = new SftpUploadSettings();

        private string BuildSftpRemotePath(string waferId, string localFilePath)
        {
            waferId = SafeWaferId(waferId);

            string ext = (Path.GetExtension(localFilePath) ?? "").TrimStart('.').ToLowerInvariant();
            string type;
            switch (ext)
            {
                case "txt": type = "TXT"; break;
                case "prd": type = "PRD"; break;
                case "waf": type = "WAF"; break;
                case "sum": type = "SUM"; break;
                case "bin": type = "BIN"; break;
                default: type = "ETC"; break;
            }

            // [CHG] PRD만 우선 고정: /upload/LCP-280/PRD/<waferId>/<waferId>.prd
            if (type == "PRD")
                return $"/upload/LCP-280/PRD/{waferId}/{waferId}.{ext}";

            // [CHG] 나머지는 통상 경로 사용: /upload/LCP-280/<TYPE>/<waferId>/<waferId>.<ext>
            return $"{SftpSettings.RemoteBaseDir}/{type}/{waferId}/{waferId}.{ext}";
        }

        private SftpClient CreateSftpClient()
        {
            if (SftpSettings == null) throw new InvalidOperationException("SftpSettings is null.");
            if (string.IsNullOrWhiteSpace(SftpSettings.Host)) throw new InvalidOperationException("SFTP Host is empty.");
            if (string.IsNullOrWhiteSpace(SftpSettings.Username)) throw new InvalidOperationException("SFTP Username is empty.");

            ConnectionInfo conn;

            if (SftpSettings.AuthMode == SftpAuthMode.PrivateKey)
            {
                if (string.IsNullOrWhiteSpace(SftpSettings.PrivateKeyFile))
                    throw new InvalidOperationException("PrivateKeyFile is empty.");

                var keyFile = string.IsNullOrWhiteSpace(SftpSettings.PrivateKeyPassphrase)
                    ? new PrivateKeyFile(SftpSettings.PrivateKeyFile)
                    : new PrivateKeyFile(SftpSettings.PrivateKeyFile, SftpSettings.PrivateKeyPassphrase);

                conn = new ConnectionInfo(
                    SftpSettings.Host,
                    SftpSettings.Port,
                    SftpSettings.Username,
                    new PrivateKeyAuthenticationMethod(SftpSettings.Username, keyFile)
                );
            }
            else
            {
                if (string.IsNullOrWhiteSpace(SftpSettings.Password))
                    throw new InvalidOperationException("SFTP Password is empty.");

                conn = new ConnectionInfo(
                    SftpSettings.Host,
                    SftpSettings.Port,
                    SftpSettings.Username,
                    new PasswordAuthenticationMethod(SftpSettings.Username, SftpSettings.Password)
                );
            }

            conn.Timeout = TimeSpan.FromMilliseconds(Math.Max(1000, SftpSettings.ConnectTimeoutMs));
            return new SftpClient(conn);
        }

        private void EnsureSftpDirectory(SftpClient sftp, string remoteDir)
        {
            if (string.IsNullOrWhiteSpace(remoteDir)) return;

            string[] parts = remoteDir.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            string cur = "/";
            foreach (var p in parts)
            {
                cur = (cur == "/") ? ("/" + p) : (cur + "/" + p);
                if (!sftp.Exists(cur))
                    sftp.CreateDirectory(cur);
            }
        }

        private bool TryUploadSnapshotViaSftp(string snapshotFile, string remoteFile)
        {
            const int retry = 10;

            for (int i = 0; i < retry; i++)
            {
                try
                {
                    using (var sftp = CreateSftpClient())
                    {
                        sftp.Connect();

                        string remoteDir = remoteFile.Substring(0, remoteFile.LastIndexOf('/'));
                        EnsureSftpDirectory(sftp, remoteDir);

                        string tmpRemote = remoteFile + ".tmp";

                        using (var fs = new FileStream(snapshotFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            sftp.UploadFile(fs, tmpRemote, true);
                        }

                        if (sftp.Exists(remoteFile))
                            sftp.DeleteFile(remoteFile);

                        sftp.RenameFile(tmpRemote, remoteFile);
                        sftp.Disconnect();
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    try { Log.Write("ResultWriterManager", nameof(TryUploadSnapshotViaSftp), ex.Message); } catch { }
                    Thread.Sleep(200 * (i + 1));
                }
            }

            return false;
        }

        // =========================================================
        // ================== (C) Async Save Worker ================
        // =========================================================
        #region Async Save Worker (High Performance Logging)

        // =========================================================
        // [ADD] Async Save Worker Implementation
        // =========================================================
        private BlockingCollection<MaterialDie> _saveQueue;
        private Task _saveWorkerTask;
        private CancellationTokenSource _saveCts;

        private void InitializeSaveWorker()
        {
            _saveQueue = new BlockingCollection<MaterialDie>(new ConcurrentQueue<MaterialDie>());
            _saveCts = new CancellationTokenSource();

            // LongRunning 옵션을 주어 전용 스레드를 할당받도록 함
            _saveWorkerTask = Task.Factory.StartNew(SaveWorkerLoop,
                                                    _saveCts.Token,
                                                    TaskCreationOptions.LongRunning,
                                                    TaskScheduler.Default);
        }

        public void StopSaveWorker()
        {
            if (_saveQueue != null)
            {
                _saveQueue.CompleteAdding();
                try { _saveWorkerTask?.Wait(5000); } catch { }
                _saveCts?.Cancel();
                _saveQueue.Dispose();
            }
        }

        /// <summary>
        /// 외부(OutputStage)에서 호출하는 진입점.
        /// 데이터를 복제(Clone)하여 큐에 넣고 즉시 리턴함.
        /// </summary>
        public void EnqueueDieSave(MaterialDie die, TestConditionSet currentCondition)
        {
            if (die == null) return;

            // 1. 데이터 오염 방지를 위한 Deep Copy (스냅샷 생성)
            MaterialDie snapshot = CloneMaterialDieForSave(die);

            // 2. TestConditionSet도 메인 스레드 시점의 것을 함께 넘겨주는 것이 안전함
            // (구조상 복잡하면 Worker 내부에서 참조하되, 변경 빈도가 낮으면 괜찮음)
            // 여기서는 튜플이나 별도 클래스로 묶어 보내는 대신,
            // WorkerLoop 안에서 전역 설정을 갱신하도록 처리 (기존 로직 호환성 유지)

            if (_saveQueue != null && !_saveQueue.IsAddingCompleted)
            {
                _saveQueue.Add(snapshot);
            }
        }

        private void SaveWorkerLoop()
        {
            foreach (var die in _saveQueue.GetConsumingEnumerable())
            {
                try
                {
                    // [중요] I/O 작업 수행

                    // 1. Condition Set 갱신 (메인 스레드 객체 참조 주의: 읽기 전용이면 무방)
                    // 필요하다면 Enqueue 시점에 ConditionSet도 복사해서 넘겨야 함.
                    this.CurrentTestConditionSet = Equipment.Instance.Tester.ConditionSet;

                    // 2. 통계 누적
                    AccumulateDie(die);

                    // 3. 파일 저장 (기존 메서드 재사용)
                    // 주의: 아래 메서드들 내부의 lock(_ioLock) 덕분에 스레드 안전함
                    AppendTxTDie(die);
                    AppendPrdDie(die);
                    AppendWafDie(die);
                    AppendBinDie(die);

                    FinalizeSummary();
                    WriteSumFile(die);

                    Log.Write("ResultWriterManager", "SaveWorker", $"[Saved] Index: {die.Index}, Map:({die.MapX},{die.MapY})");
                }
                catch (Exception ex)
                {
                    Log.Write("ResultWriterManager", "SaveWorker_Error", $"Index: {die?.Index} - {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 저장 시점의 데이터 스냅샷을 만들기 위한 복제 함수
        /// </summary>
        private MaterialDie CloneMaterialDieForSave(MaterialDie org)
        {
            MaterialDie newDie = new MaterialDie();

            // 기본 값 타입 복사
            newDie.Index = org.Index;
            newDie.MapX = org.MapX;
            newDie.MapY = org.MapY;
            newDie.BinX = org.BinX;
            newDie.BinY = org.BinY;
            newDie.State = org.State;
            //newDie.Result = org.Result; // Enum or struct
            newDie.Rank = org.Rank;
            newDie.RankName = org.RankName;
            //newDie.IsCell = org.IsCell;
            newDie.SourceWaferId = org.SourceWaferId;
            newDie.SourceBinFileName = org.SourceBinFileName;
            newDie.SocketIndex = org.SocketIndex;

            // 참조 타입: TesterResult (가장 중요)
            if (org.TesterResult != null)
            {
                // PKGTesterResult가 DeepClone을 지원하면 좋으나, 없다면 새 객체 생성 후 값 복사
                // 여기서는 참조만 넘기면 위험하므로, 최소한의 얕은 복사라도 수행해야 함.
                // *TesterResult 내부 데이터가 메인 로직에서 변경되지 않는다면 참조도 가능하지만*,
                // 안전을 위해 TesterResult도 복제하는 로직을 PKGTesterResult 클래스에 추가하는 것을 권장.
                // 여기서는 일단 참조를 넘기되, 메인 로직에서 이 객체를 재사용하지 않는다는 전제가 필요함.
                newDie.TesterResult = org.TesterResult;
            }

            // 참조 타입: MeasureValues (Dictionary)
            if (org.MeasureValues != null)
            {
                newDie.MeasureValues = new Dictionary<string, double>(org.MeasureValues);
            }

            return newDie;
        }

        #endregion
    }
}