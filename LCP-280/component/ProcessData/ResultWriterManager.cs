// LCP-280\component\ProcessData\ResultWriterManager.cs
using QMC.Common;
using QMC.Common.Account;
using QMC.Common.PKGTester;
using QMC.LCP_280.Process.Work;
using Renci.SshNet;
using Renci.SshNet.Common;
using System;
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
    /// ResultWriterManager (B안 정리본)
    /// - 프로그램 동작 중에는 로컬 결과 파일을 계속 갱신(기존 방식 유지)
    /// - 네트워크 업로드는 "직접 copy" 대신 Outbox(로컬 스풀)로 스냅샷을 1초 단위로 생성
    /// - 백그라운드 업로더가 Outbox의 "최신 스냅샷 1개"만 네트워크 목적지에 덮어쓰기 업로드
    /// - 강제 종료되어도 Outbox가 남아 있고, 다음 실행 시 이어서 업로드
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

        public PRDContext PrdContext { get; private set; } = new PRDContext();
        public SUMContext SumContext { get; private set; } = new SUMContext();
        public WAFContext WafContext { get; private set; } = new WAFContext();

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
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ResultData", _runDate);
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

        private CancellationTokenSource _uploaderCts;
        private Task _uploaderTask;
        private readonly AutoResetEvent _uploaderWake = new AutoResetEvent(false);

        private string GetOutboxRoot()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ResultData", _runDate, "_outbox");
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

        private bool ShouldEnqueueNow(string destKey, TimeSpan interval)
        {
            lock (_outboxLock)
            {
                if (_lastEnqueueUtcByDest.TryGetValue(destKey, out var lastUtc))
                {
                    if ((DateTime.UtcNow - lastUtc) < interval)
                        return false;
                }
                _lastEnqueueUtcByDest[destKey] = DateTime.UtcNow;
                return true;
            }
        }

        /// <summary>
        /// 기존의 "스냅샷 복사" 대신, 업로드 작업 지시만 Outbox 큐에 넣는다(rename-only).
        /// </summary>
        private void MirrorLocalToNetworkIfNeeded(string waferId, string localFile, string networkFileForSmb)
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

            if (string.IsNullOrWhiteSpace(dest))
                return;

            // 파일 타입별 최소 enqueue 간격 적용 (실시간/부하 균형)
            var interval = GetMinIntervalForFile(localFile);
            if (!ShouldEnqueueNow(dest, interval))
                return;

            EnqueueUploadJob(localFile, dest);
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
                if (!File.Exists(localFile))
                    return;

                Directory.CreateDirectory(GetOutboxQueueDir());

                var job = new UploadJob
                {
                    LocalFile = localFile,
                    DestFile = destFile,
                    Mode = UploadMode,
                    Utc = DateTime.UtcNow
                };

                // job 파일 내용은 단순 텍스트로 (닷넷프레임워크에서 JSON 의존성 피함)
                // 1) Mode
                // 2) LocalFile
                // 3) DestFile
                // 4) UtcTicks
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
                File.Move(tmp, final); // 원자적으로 큐에 투입

                _uploaderWake.Set();
            }
            catch (Exception ex)
            {
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

        private void UploadLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                bool didWork = false;

                try
                {
                    string q = GetOutboxQueueDir();
                    if (!Directory.Exists(q))
                    {
                        _uploaderWake.WaitOne(500);
                        continue;
                    }

                    // 오래된 job부터 처리
                    var jobs = Directory.EnumerateFiles(q, "*.job")
                                        .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
                                        .ToList();

                    foreach (var jobPath in jobs)
                    {
                        if (token.IsCancellationRequested) break;

                        // 처리중 락: .job -> .job.working (rename-only)
                        string working = jobPath + ".working";
                        try
                        {
                            if (File.Exists(working))
                                continue;

                            File.Move(jobPath, working);
                        }
                        catch
                        {
                            continue; // 다른 스레드/프로세스가 잡았거나 잠김
                        }

                        didWork = true;

                        var job = TryParseJobFile(working);
                        if (job == null || string.IsNullOrWhiteSpace(job.LocalFile) || string.IsNullOrWhiteSpace(job.DestFile))
                        {
                            TryDeleteFileQuietly(working);
                            continue;
                        }

                        // 최신 상태만 올리고 싶으면 여기서 “중복/구버전 job” 제거 로직도 가능.
                        // (필요시 추가해드릴게요)

                        bool ok = false;

                        if (job.Mode == RemoteUploadMode.SmbShare)
                        {
                            ok = TryUploadLatestViaSmb_RenameOnly(job.LocalFile, job.DestFile);
                        }
                        else if (job.Mode == RemoteUploadMode.Sftp)
                        {
                            // SFTP도 tmpRemote 업로드 후 rename(이미 구현돼 있음)
                            ok = TryUploadSnapshotViaSftp(job.LocalFile, job.DestFile);
                        }

                        if (ok)
                        {
                            TryDeleteFileQuietly(working);
                        }
                        else
                        {
                            // 실패 시 재시도 위해 되돌림
                            try
                            {
                                if (File.Exists(working))
                                {
                                    // 너무 빠른 재시도를 막고 싶으면 .retryAt 같은 정책을 넣을 수 있음
                                    File.Move(working, working.Replace(".working", ".job"));
                                }
                            }
                            catch
                            {
                                // 최악의 경우 .working이 남아도 다음 실행 때 정리/재시도 처리 가능
                            }

                            // 잠깐 쉬었다가 재시도
                            Thread.Sleep(200);
                        }
                    }
                }
                catch (Exception ex)
                {
                    try { Log.Write("ResultWriterManager", nameof(UploadLoop), ex.Message); } catch { }
                }

                if (!didWork)
                    _uploaderWake.WaitOne(300);
            }
        }

        /// <summary>
        /// SMB 업로드: tmp로 COPY 후 마지막에 Rename(=Move)로 최종 반영.
        /// - 실제 “rename-only”는 최종 단계만 rename을 의미 (네트워크로의 데이터 전송은 copy가 필요)
        /// - 중요한 점: 웨이퍼 배출 스레드에서는 절대 이 함수가 호출되지 않음(백그라운드 전용)
        /// </summary>
        private bool TryUploadLatestViaSmb_RenameOnly(string localFile, string networkFile)
        {
            const int retry = 10;

            for (int i = 0; i < retry; i++)
            {
                try
                {
                    if (!File.Exists(localFile))
                        return false;

                    string ndir = Path.GetDirectoryName(networkFile);
                    if (!string.IsNullOrWhiteSpace(ndir))
                        Directory.CreateDirectory(ndir);

                    string tmp = networkFile + ".tmp";

                    // 네트워크 전송 자체는 Copy가 필요 (SMB는 rename만으로 로컬->원격 전송 불가)
                    // 대신 최종 반영은 rename으로 원자적 교체
                    File.Copy(localFile, tmp, true);

                    // 최종 파일을 원자적으로 교체
                    // .NET Framework에서도 File.Replace가 가능하지만, SMB에서 정책/권한에 따라 실패할 수 있어 Move 기반으로 구성
                    if (File.Exists(networkFile))
                    {
                        try { File.Delete(networkFile); } catch { /* ignore */ }
                    }

                    File.Move(tmp, networkFile);
                    return true;
                }
                catch
                {
                    Thread.Sleep(200 * (i + 1));
                }
            }

            return false;
        }

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

        // 3) 다이 누적시 BinLabel 유지
        private readonly Dictionary<int, string> _binLabelMap = new Dictionary<int, string>();

        // ===== WaferTotalSummary 중복 기록 방지 =====
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

                // B안: 프로그램 시작 시 Outbox 업로더 실행 (이전 Outbox 있으면 이어서 업로드)
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

            string localDir = GetLocalResultDir(waferId);
            Directory.CreateDirectory(localDir);
            string localFile = Path.Combine(localDir, waferId + ".txt");

            string networkDir = ShouldUploadToNetwork() ? GetNetworkResultDir(waferId, eqpConfig.TXTResultPath) : null;
            string networkFile = !string.IsNullOrWhiteSpace(networkDir) ? Path.Combine(networkDir, waferId + ".txt") : null;

            string strBinFileName = die.SourceBinFileName;

            lock (_ioLock)
            {
                bool exists = File.Exists(localFile);
                using (var w = new StreamWriter(localFile, true, Encoding.UTF8))
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

                if (!string.IsNullOrWhiteSpace(networkFile))
                    MirrorLocalToNetworkIfNeeded(waferId, localFile, networkFile);
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

            string localDir = GetLocalResultDir(waferId);
            Directory.CreateDirectory(localDir);
            string localFile = Path.Combine(localDir, waferId + ".bin");

            string networkDir = ShouldUploadToNetwork() ? GetNetworkResultDir(waferId, eqpConfig.BinResultPath) : null;
            string networkFile = !string.IsNullOrWhiteSpace(networkDir) ? Path.Combine(networkDir, waferId + ".bin") : null;

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
                    MirrorLocalToNetworkIfNeeded(waferId, localFile, networkFile);
            }

            return 0;
        }

        public int WriteSumFile(MaterialDie die)
        {
            FinalizeSummary();

            string waferId = SafeWaferId(die.SourceWaferId);
            SyncSumContextFromEquipmentSummaryIfPossible(waferId);

            var eqpConfig = Equipment.Instance.EquipmentConfig;

            string localDir = GetLocalResultDir(waferId);
            Directory.CreateDirectory(localDir);
            string localFile = Path.Combine(localDir, waferId + ".sum");

            string networkDir = ShouldUploadToNetwork() ? GetNetworkResultDir(waferId, eqpConfig.SUMResultPath) : null;
            string networkFile = !string.IsNullOrWhiteSpace(networkDir) ? Path.Combine(networkDir, waferId + ".sum") : null;

            lock (_ioLock)
            {
                using (var w = new StreamWriter(localFile, false, Encoding.UTF8))
                {
                    w.WriteLine("EQPName," + SumContext.EqpName);
                    w.WriteLine();
                    w.WriteLine("StartTime," + SumContext.StartTime);
                    w.WriteLine("EndTime," + SumContext.EndTime);
                    w.WriteLine();
                    w.WriteLine("WaferID," + SumContext.WaferID);
                    w.WriteLine();
                    w.WriteLine("TotalCount," + SumContext.TotalCount);
                    w.WriteLine("GoodCount," + SumContext.GoodCount);
                    w.WriteLine("NGCount," + SumContext.NGCount);
                    w.WriteLine();

                    w.Write("Item");
                    foreach (var it in SumContext.ItemNames) w.Write("," + it);
                    w.WriteLine();

                    WriteSummaryLine(w, "Min", SumContext.ItemNames, SumContext.Min);
                    WriteSummaryLine(w, "Max", SumContext.ItemNames, SumContext.Max);
                    WriteSummaryLine(w, "Avg", SumContext.ItemNames, SumContext.Avg);
                    WriteSummaryLine(w, "Std", SumContext.ItemNames, SumContext.Std);

                    w.WriteLine();
                    w.WriteLine("BinNo,BinCount");
                    foreach (var kv in SumContext.BinCounts)
                        w.WriteLine(kv.Key + "," + kv.Value);

                    w.WriteLine();

                    foreach (var line in SumContext.ParameterBlock)
                        w.WriteLine(line);

                    foreach (var line in SumContext.ZeroBlock)
                        w.WriteLine(line);
                }

                if (!string.IsNullOrWhiteSpace(networkFile))
                    MirrorLocalToNetworkIfNeeded(waferId, localFile, networkFile);
            }

            return 0;
        }

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

            string loginId = (AccountManager.CurrentAccount != null)
                ? (AccountManager.CurrentAccount.UserID?.ToString() ?? "OPERATOR")
                : "OPERATOR";

            PrdContext.HeaderLines.Clear();
            PrdContext.HeaderLines.Add("Filecreationtime," + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            PrdContext.HeaderLines.Add("-100");
            PrdContext.HeaderLines.Add("67");
            PrdContext.HeaderLines.Add("46");
            PrdContext.HeaderLines.Add("-75");
            PrdContext.HeaderLines.Add("1");
            PrdContext.HeaderLines.Add("1");
            PrdContext.HeaderLines.Add("13089"); // TotalCount 자리(파일 내부에서 계속 업데이트)
            PrdContext.HeaderLines.Add(binFile);
            PrdContext.HeaderLines.Add(binFile);
            PrdContext.HeaderLines.Add(waferId);
            PrdContext.HeaderLines.Add(waferId);
            PrdContext.HeaderLines.Add(loginId);
            PrdContext.HeaderLines.Add(SumContext.EqpName);

            BuildTestConditionParameterBlock();

            if (PrdContext.ZeroBlock1.Count == 0)
            {
                const string zeroLine = "0 0 0 0 0 0.00 0.00 0 0.00";
                for (int i = 0; i < 20; i++)
                {
                    PrdContext.ZeroBlock1.Add(zeroLine);
                    SumContext.ZeroBlock.Add(zeroLine);
                }
            }

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

            lock (_ioLock)
            {
                bool exists = File.Exists(localFile);
                using (var w = new StreamWriter(localFile, true, Encoding.UTF8))
                {
                    if (!exists)
                    {
                        BuildPrdHeader(die);

                        foreach (var line in PrdContext.HeaderLines) w.WriteLine(line);
                        foreach (var line in PrdContext.ParameterBlock) w.WriteLine(line);
                        foreach (var line in PrdContext.ZeroBlock1) w.WriteLine(line);
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
                    MirrorLocalToNetworkIfNeeded(waferId, localFile, networkFile);
            }

            return 0;
        }

        private void BuildWafHeader(MaterialDie die)
        {
            if (die == null || die.TesterResult == null)
                return;

            if (WafContext.HeaderInitialized)
                return;

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
            string loginId = (AccountManager.CurrentAccount?.UserID ?? "OPERATOR").ToString();
            string eqpName = (SumContext.EqpName ?? "LPC-280").Replace("EqpName,", "");

            WafContext.HeaderLines.Add(productName);
            WafContext.HeaderLines.Add(productName);
            WafContext.HeaderLines.Add(waferId);
            WafContext.HeaderLines.Add(waferId);
            WafContext.HeaderLines.Add(loginId);
            WafContext.HeaderLines.Add(eqpName);

            if (WafContext.ZeroBlock1.Count == 0)
            {
                const string zeroLine = "0 0 0 0 0 0.00 0.00 0 0.00";
                for (int i = 0; i < 20; i++)
                    WafContext.ZeroBlock1.Add(zeroLine);
            }

            if (WafContext.ZeroBlock2.Count == 0)
            {
                WafContext.ZeroBlock2.Add("1");
                for (int i = 0; i < 31; i++)
                    WafContext.ZeroBlock2.Add("0");
            }

            WafContext.HeaderInitialized = true;
        }

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

            lock (_ioLock)
            {
                bool exists = File.Exists(localFile);

                if (!WafContext.HeaderInitialized)
                    BuildWafHeader(die);

                using (var w = new StreamWriter(localFile, true, Encoding.UTF8))
                {
                    if (!exists)
                    {
                        foreach (var line in WafContext.HeaderLines) w.WriteLine(line);
                        foreach (var line in WafContext.ParameterBlock) w.WriteLine(line);
                        foreach (var line in WafContext.ZeroBlock1) w.WriteLine(line);
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
                    MirrorLocalToNetworkIfNeeded(waferId, localFile, networkFile);
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
            if (die == null || die.TesterResult == null) return;

            lock (_summaryLock)
            {
                var itemsDict = GetInternalItemDict(die.TesterResult);
                if (itemsDict == null) return;

                if (die.IsPass) SumContext.GoodCount++; else SumContext.NGCount++;

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

            if (CurrentTestConditionSet == null || CurrentTestConditionSet.Items == null || CurrentTestConditionSet.Items.Count == 0)
                return;

            int seq = 1;
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
            }
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

                if(cfg.NetworkMode == 0)
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
                using (var w = new StreamWriter(localFile, true, Encoding.UTF8))
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
                    MirrorLocalToNetworkIfNeeded(row.WaferId, localFile, networkFile);
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

    }
}