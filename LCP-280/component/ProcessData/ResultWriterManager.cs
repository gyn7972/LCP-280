using QMC.Common;
using QMC.Common.Account;
using QMC.Common.PKGTester;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace QMC.LCP_280.Process.Component.ProcessData
{
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
            // 동적 생성된 CSV 컬럼(순서 보존)
            public List<string> DataColumns { get; set; } = new List<string>();
            public bool HeaderInitialized { get; set; }
        }
        public class WAFContext
        {
            public List<string> HeaderLines { get; set; } = new List<string>();
            public List<string> ParameterBlock { get; set; } = new List<string>();
            public List<string> ZeroBlock1 { get; set; } = new List<string>();
            public List<string> ZeroBlock2 { get; set; } = new List<string>();
            // 동적 생성된 CSV 컬럼(순서 보존)
            public List<string> DataColumns { get; set; } = new List<string>();
            public bool HeaderInitialized { get; set; }
        }
        #endregion

        private Equipment Equipment = Equipment.Instance;
        
        public PRDContext PrdContext { get; private set; } = new PRDContext();
        public SUMContext SumContext { get; private set; } = new SUMContext();
        public WAFContext WafContext { get; private set; } = new WAFContext();
        // TestConditionSet 연계: 외부에서 주입 (레시피 로딩 직후 할당)
        public TestConditionSet CurrentTestConditionSet { get; set; }
        private string _lastPrdWaferId;
        private string _lastWafWaferId;
        private readonly object _headerLock = new object();

        private static readonly string _runDate = DateTime.Now.ToString("yyyyMMdd");
        private string _baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ResultData");
        private string _logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

        public string BaseDir { 
            get 
            {
                return _baseDir;
            }
            set
            {
                 _baseDir = value;
            }
        }

        private string LogDir {
            get
            {
                // 1) 외부에서 강제로 세팅한 값이 있으면 그 값을 최우선 사용
                if (!string.IsNullOrWhiteSpace(_logDir))
                    return _logDir;

                // 2) EquipmentConfig 기반 경로
                try
                {
                    var cfg = Equipment.Instance?.EquipmentConfig;
                    if (cfg != null)
                    {
                        // 네트워크 모드에서는 설정 경로 우선
                        if (cfg.NetworkMode > 1 && !string.IsNullOrWhiteSpace(cfg.LogPath))
                            return cfg.LogPath;
                    }
                }
                catch
                {
                    // ignore (fallback 사용)
                }

                // 3) 기본 fallback
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            }
            set
            {
                // 재귀 방지 + 정규화
                _logDir = string.IsNullOrWhiteSpace(value)
                    ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs")
                    : value.Trim();
            }
        }
        private readonly object _ioLock = new object();

        // ===== [ADD] Local spooling + network mirroring =====
        private readonly object _networkMirrorLock = new object();
        private readonly Dictionary<string, DateTime> _lastMirrorUtcByDest = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);

        // 네트워크 미러링 최소 간격(너무 자주 복사하면 네트워크가 다시 느려짐)
        private static readonly TimeSpan NetworkMirrorMinInterval = TimeSpan.FromMilliseconds(500);

        private string GetLocalResultRoot()
        {
            // 기존 패턴 유지: ResultData\yyyyMMdd
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
            // configuredPath 가 비었으면 네트워크 업로드를 하지 않음
            if (string.IsNullOrWhiteSpace(configuredPath))
                return null;

            // e.g. Y:\폴더명\{waferId}
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

        private void MirrorLocalToNetworkIfNeeded(string localFile, string networkFile)
        {
            if (string.IsNullOrWhiteSpace(localFile) || string.IsNullOrWhiteSpace(networkFile))
                return;

            if (!File.Exists(localFile))
                return;

            lock (_networkMirrorLock)
            {
                DateTime lastUtc;
                if (_lastMirrorUtcByDest.TryGetValue(networkFile, out lastUtc))
                {
                    if ((DateTime.UtcNow - lastUtc) < NetworkMirrorMinInterval)
                        return;
                }

                try
                {
                    var ndir = Path.GetDirectoryName(networkFile);
                    if (!string.IsNullOrWhiteSpace(ndir))
                        Directory.CreateDirectory(ndir);

                    // temp로 복사 후 move(부분 파일/깨진 파일 노출 방지)
                    string tmp = networkFile + ".tmp";

                    // overwrite=true
                    File.Copy(localFile, tmp, true);

                    // Move는 같은 볼륨에서 원자적이지만, 네트워크/다른 볼륨에서는 덮어쓰기가 안될 수 있음.
                    // 안전하게: 기존 삭제 후 move
                    if (File.Exists(networkFile))
                        File.Delete(networkFile);

                    File.Move(tmp, networkFile);

                    _lastMirrorUtcByDest[networkFile] = DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    // 네트워크 일시 오류는 흔함 → 로컬은 이미 저장됐으므로 여기서는 무시(필요시 Log)
                    try { Log.Write("ResultWriterManager", nameof(MirrorLocalToNetworkIfNeeded), ex.Message); } catch { }
                }
            }
        }
        // ===== [ADD END] =====



        // 1) 클래스 필드 영역(기존 필드들 아래)에 요약 계산용 내부 상태 추가
        private readonly object _summaryLock = new object();
        private Dictionary<string, double> _sum = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, double> _sumSq = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        private bool _summaryFinalized = false;

        public ResultWriterManager()
        {
            try
            {
                Directory.CreateDirectory(BaseDir);

                var logDir = LogDir;
                if (!string.IsNullOrWhiteSpace(logDir))
                    Directory.CreateDirectory(logDir);
            }
            catch { /* 필요시 Log.Write(e) */ }
        }

        #region Legacy Format Append API
        public int AppendTxTDie(MaterialDie die)
        {
            PKGTesterResult result = die.TesterResult;
            string waferId = SafeWaferId(die.SourceWaferId);

            var eqpConfig = Equipment.Instance.EquipmentConfig;

            // [CHG] 항상 로컬에 먼저 저장
            string localDir = GetLocalResultDir(waferId);
            Directory.CreateDirectory(localDir);
            string localFile = Path.Combine(localDir, waferId + ".txt");

            // [ADD] 업로드 목적지
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

                // [ADD] 네트워크 미러링(주기 제한)
                if (!string.IsNullOrWhiteSpace(networkFile))
                    MirrorLocalToNetworkIfNeeded(localFile, networkFile);
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

            // [CHG] 로컬 먼저 저장
            string localDir = GetLocalResultDir(waferId);
            Directory.CreateDirectory(localDir);
            string localFile = Path.Combine(localDir, waferId + ".bin");

            // [ADD] 업로드 목적지
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

                // [ADD]
                if (!string.IsNullOrWhiteSpace(networkFile))
                    MirrorLocalToNetworkIfNeeded(localFile, networkFile);
            }

            return 0;
        }

        public int WriteSumFile(MaterialDie die)
        {
            FinalizeSummary();

            string waferId = SafeWaferId(die.SourceWaferId);
            // [ADD] 최종 파일 쓰기 직전 SSOT 동기화 (가장 안전)
            SyncSumContextFromEquipmentSummaryIfPossible(waferId);

            var eqpConfig = Equipment.Instance.EquipmentConfig;

            // [CHG] 로컬 먼저 저장
            string localDir = GetLocalResultDir(waferId);
            Directory.CreateDirectory(localDir);
            string localFile = Path.Combine(localDir, waferId + ".sum");

            // [ADD] 업로드 목적지
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

                // [ADD]
                if (!string.IsNullOrWhiteSpace(networkFile))
                    MirrorLocalToNetworkIfNeeded(localFile, networkFile);
            }

            return 0;
        }

        private void BuildPrdHeader(MaterialDie die)
        {
            if (die == null || die.TesterResult == null)
                return;
            if (PrdContext.HeaderInitialized)
                return;

            // 1) 테스트 아이템 키 수집
            var dict = GetInternalItemDict(die.TesterResult);
            var itemKeys = new List<string>();
            if (dict != null)
            {
                foreach (var k in dict.Keys)            // Dictionary 열거 순서(추가 순서)를 그대로 사용
                {
                    if (string.IsNullOrWhiteSpace(k)) continue;
                    if (!itemKeys.Contains(k))          // Distinct 처리(첫 등장 유지)
                        itemKeys.Add(k);
                }
            }

            // 3) 최종 컬럼 구성 (기본 좌표 + Rank + TestItems + Index)
            PrdContext.DataColumns.Clear();
            PrdContext.DataColumns.AddRange(new[] { "XADR", "YADR", "RANK" });
            PrdContext.DataColumns.AddRange(itemKeys);
            PrdContext.DataColumns.Add("Index");

            var waferId = die.SourceWaferId ?? "UNKNOWN";
            var binFile = StripExtension(die.SourceBinFileName);
            string loginId = string.Empty;// AccountManager.CurrentAccount.UserID ?? "OPERATOR";
            if (AccountManager.CurrentAccount != null)
            {
                loginId = AccountManager.CurrentAccount.UserID.ToString() ?? "OPERATOR";
            }
            else
            {
                loginId = "OPERATOR";
            }

            // 4) HeaderLines 동적 구성 (필요한 메타 정보)
            PrdContext.HeaderLines.Clear();
            //PrdContext.HeaderLines.Add("FileCreationTime," + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            PrdContext.HeaderLines.Add("Filecreationtime," + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            PrdContext.HeaderLines.Add("-100"); //<- ?
            PrdContext.HeaderLines.Add("67"); //<- ?
            PrdContext.HeaderLines.Add("46"); //<- ?
            PrdContext.HeaderLines.Add("-75"); //<- ?
            PrdContext.HeaderLines.Add("1"); //<- ?
            PrdContext.HeaderLines.Add("1"); //<- ?
            PrdContext.HeaderLines.Add("13089"); //<-전체 검사 갯수.. //SumContext.TotalCount // 이 항목은 계속 업데이트 되어야함.
            PrdContext.HeaderLines.Add(binFile);
            PrdContext.HeaderLines.Add(binFile);
            PrdContext.HeaderLines.Add(waferId);
            PrdContext.HeaderLines.Add(waferId);
            PrdContext.HeaderLines.Add(loginId); // <-로그인 ID  AccountManager.CurrentAccount.UserID
            PrdContext.HeaderLines.Add(SumContext.EqpName);

            BuildTestConditionParameterBlock();

            // 형식 맞추기용
            // 5) ZeroBlock1 / ZeroBlock2 고정 데이터 주입 (파일 형식 맞춤)
            if (PrdContext.ZeroBlock1.Count == 0)
            {
                const string zeroLine = "0 0 0 0 0 0.00 0.00 0 0.00";
                for (int i = 0; i < 20; i++)
                {
                    PrdContext.ZeroBlock1.Add(zeroLine);
                    SumContext.ZeroBlock.Add(zeroLine);
                }
            }
            // 형식 맞추기용
            if (PrdContext.ZeroBlock2.Count == 0)
            {
                // 첫 줄 1, 이후 30줄 0 (총 31줄)
                PrdContext.ZeroBlock2.Add("1");
                for (int i = 0; i < 30; i++)
                    PrdContext.ZeroBlock2.Add("0");
            }

            PrdContext.HeaderInitialized = true;
        }
        // PRD 헤더 TotalCount 라인 인덱스 (고정 구조: 0~13 중 7이 TotalCount)
        private const int PRD_TOTALCOUNT_HEADER_INDEX = 7;
        private void UpdatePrdHeaderTotalCountInFile(string filePath)
        {
            try
            {
                // 헤더 구조: 파일 처음부터 CSV 헤더 직전까지 모두 라인 단위 저장되어 있으므로 전체 읽기 후 특정 인덱스 교체
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
            catch
            {
                // 필요 시 로그 처리
            }
        }
        public int AppendPrdDie(MaterialDie die)
        {
            if (die == null || die.TesterResult == null)
                return -1;

            string waferId = SafeWaferId(die.SourceWaferId);
            var eqpConfig = Equipment.Instance.EquipmentConfig;

            // [CHG] 로컬 먼저 저장
            string localDir = GetLocalResultDir(waferId);
            Directory.CreateDirectory(localDir);
            string localFile = Path.Combine(localDir, waferId + ".prd");

            // [ADD] 업로드 목적지
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

                        foreach (var line in PrdContext.HeaderLines)
                            w.WriteLine(line);
                        foreach (var line in PrdContext.ParameterBlock)
                            w.WriteLine(line);
                        foreach (var line in PrdContext.ZeroBlock1)
                            w.WriteLine(line);
                        foreach (var line in PrdContext.ZeroBlock2)
                            w.WriteLine(line);

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
                                TestItemResult ti;
                                if (itemDict.TryGetValue(col, out ti) && ti != null)
                                    sb.Append(ti.Value);
                                else
                                    sb.Append("0");
                                break;
                        }
                    }

                    w.WriteLine(sb.ToString());
                }

                if (exists)
                {
                    if (PrdContext.HeaderLines.Count > PRD_TOTALCOUNT_HEADER_INDEX)
                        PrdContext.HeaderLines[PRD_TOTALCOUNT_HEADER_INDEX] = SumContext.TotalCount.ToString();

                    // [CHG] 로컬 파일만 직접 수정
                    UpdatePrdHeaderTotalCountInFile(localFile);
                }

                // [ADD] 네트워크 미러링
                if (!string.IsNullOrWhiteSpace(networkFile))
                    MirrorLocalToNetworkIfNeeded(localFile, networkFile);
            }

            return 0;
        }

        /// <summary>
        /// WAF 파일에서 사용할 측정 아이템 컬럼 순서를 한 번만 구축
        /// (아이템 이름 라인은 쓰지 않고, 값만 이 순서대로 출력)
        /// </summary>
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

            // 샘플 헤더 라인 구성 (Timestamp 제외, 요구된 순서)
            WafContext.HeaderLines.Clear();
            WafContext.HeaderLines.Add("-100");
            WafContext.HeaderLines.Add("67");
            WafContext.HeaderLines.Add("46");
            WafContext.HeaderLines.Add("-75");
            WafContext.HeaderLines.Add("1");
            WafContext.HeaderLines.Add("1");
            // TotalCount 자리: 아직 모르면 샘플값(13089) 또는 SumContext.TotalCount
            WafContext.HeaderLines.Add(SumContext.TotalCount > 0 ? SumContext.TotalCount.ToString() : "13089");
            //VA1VPRO16
            string productName = StripExtension(die.SourceBinFileName);
            string waferId = SafeWaferId(die.SourceWaferId);
            string loginId = (AccountManager.CurrentAccount?.UserID ?? "OPERATOR").ToString();
            string eqpName = (SumContext.EqpName ?? "LPC-280").Replace("EqpName,", "");

            // 샘플에서 제품명 2회, 웨이퍼ID 2회
            WafContext.HeaderLines.Add(productName);
            WafContext.HeaderLines.Add(productName);
            WafContext.HeaderLines.Add(waferId);
            WafContext.HeaderLines.Add(waferId);
            WafContext.HeaderLines.Add(loginId);
            WafContext.HeaderLines.Add(eqpName);

            // ZeroBlock1: 20줄
            if (WafContext.ZeroBlock1.Count == 0)
            {
                const string zeroLine = "0 0 0 0 0 0.00 0.00 0 0.00";
                for (int i = 0; i < 20; i++)
                    WafContext.ZeroBlock1.Add(zeroLine);
            }
            // ZeroBlock2: 첫 줄 1, 이후 30줄 0 (총 31줄)
            if (WafContext.ZeroBlock2.Count == 0)
            {
                WafContext.ZeroBlock2.Add("1");
                for (int i = 0; i < 31; i++)
                    WafContext.ZeroBlock2.Add("0");
            }

            WafContext.HeaderInitialized = true;


            // 헤더(앞부분 숫자/문자 라인)는 WafContext.HeaderLines / ParameterBlock / ZeroBlock1 / ZeroBlock2 가
            // 이미 채워져 있다고 가정하고, 여기서는 건드리지 않음.
            WafContext.HeaderInitialized = true;
        }
        public int AppendWafDie(MaterialDie die)
        {
            if (die == null || die.TesterResult == null)
                return -1;

            string waferId = SafeWaferId(die.SourceWaferId);
            var eqpConfig = Equipment.Instance.EquipmentConfig;

            // [CHG] 로컬 먼저 저장
            string localDir = GetLocalResultDir(waferId);
            Directory.CreateDirectory(localDir);
            string localFile = Path.Combine(localDir, waferId + ".waf");

            // [ADD] 업로드 목적지
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
                        foreach (var line in WafContext.HeaderLines)
                            w.WriteLine(line);

                        foreach (var line in WafContext.ParameterBlock)
                            w.WriteLine(line);

                        foreach (var line in WafContext.ZeroBlock1)
                            w.WriteLine(line);

                        foreach (var line in WafContext.ZeroBlock2)
                            w.WriteLine(line);
                    }

                    var dict = GetInternalItemDict(r)
                               ?? new Dictionary<string, TestItemResult>(StringComparer.OrdinalIgnoreCase);

                    var sb = new StringBuilder(256);
                    sb.Append((die.MapX).ToString(CultureInfo.InvariantCulture));
                    sb.Append(',');
                    sb.Append((die.MapY).ToString(CultureInfo.InvariantCulture));

                    foreach (var key in WafContext.DataColumns)
                    {
                        sb.Append(',');
                        TestItemResult item;
                        if (dict.TryGetValue(key, out item) && item != null)
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

                // [ADD]
                if (!string.IsNullOrWhiteSpace(networkFile))
                    MirrorLocalToNetworkIfNeeded(localFile, networkFile);
            }

            return 0;
        }
        #endregion

        #region 완료 후 일괄 업로드

        private string GetLocalWaferDir(string waferId)
        {
            waferId = SafeWaferId(waferId);
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ResultData", _runDate, waferId);
        }

        private static void EnsureDir(string dir)
        {
            if (!string.IsNullOrWhiteSpace(dir))
                Directory.CreateDirectory(dir);
        }

        private static void CopyWithRetry(string src, string dst, int retries = 5, int delayMs = 200)
        {
            if (!File.Exists(src))
                return;

            EnsureDir(Path.GetDirectoryName(dst));

            Exception last = null;
            for (int i = 0; i < retries; i++)
            {
                try
                {
                    // temp -> replace (부분 파일 방지)
                    string tmp = dst + ".tmp";
                    File.Copy(src, tmp, true);

                    if (File.Exists(dst))
                        File.Delete(dst);

                    File.Move(tmp, dst);
                    return;
                }
                catch (Exception ex)
                {
                    last = ex;
                    Thread.Sleep(delayMs * (i + 1)); // simple backoff
                }
            }

            if (last != null)
                Log.Write("ResultWriterManager", nameof(CopyWithRetry), last.Message);
        }

        /// <summary>
        /// 웨이퍼 종료(언로딩 완료 시점 등)에서 호출: 로컬 결과를 네트워크로 일괄 업로드
        /// - NetworkMode==0 : 로컬만 사용 (업로드 안함)
        /// - NetworkMode>0 : EquipmentConfig.*ResultPath로 파일 복사
        /// </summary>
        public void FlushWaferResultToNetwork(string waferId)
        {
            waferId = SafeWaferId(waferId);

            var cfg = Equipment.Instance?.EquipmentConfig;
            if (cfg == null)
                return;

            if (cfg.NetworkMode <= 0)
                return;

            string localDir = GetLocalWaferDir(waferId);
            if (!Directory.Exists(localDir))
                return;

            // Finalize를 여기서 한 번 더 보장
            try { FinalizeSummary(); } catch { }

            // 파일별 업로드(경로 분리 요구사항 반영)
            try
            {
                // txt
                if (!string.IsNullOrWhiteSpace(cfg.TXTResultPath))
                    CopyWithRetry(Path.Combine(localDir, waferId + ".txt"),
                                  Path.Combine(cfg.TXTResultPath, waferId, waferId + ".txt"));

                // prd
                if (!string.IsNullOrWhiteSpace(cfg.PRDResultPath))
                    CopyWithRetry(Path.Combine(localDir, waferId + ".prd"),
                                  Path.Combine(cfg.PRDResultPath, waferId, waferId + ".prd"));

                // waf
                if (!string.IsNullOrWhiteSpace(cfg.WAFResultPath))
                    CopyWithRetry(Path.Combine(localDir, waferId + ".waf"),
                                  Path.Combine(cfg.WAFResultPath, waferId, waferId + ".waf"));

                // bin
                if (!string.IsNullOrWhiteSpace(cfg.BinResultPath))
                    CopyWithRetry(Path.Combine(localDir, waferId + ".bin"),
                                  Path.Combine(cfg.BinResultPath, waferId, waferId + ".bin"));

                // sum
                if (!string.IsNullOrWhiteSpace(cfg.SUMResultPath))
                    CopyWithRetry(Path.Combine(localDir, waferId + ".sum"),
                                  Path.Combine(cfg.SUMResultPath, waferId, waferId + ".sum"));
            }
            catch (Exception ex)
            {
                Log.Write("ResultWriterManager", "FlushWaferResultToNetwork", ex.Message);
            }
        }

        #endregion


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
                    //w.Write("," + (die.MapX * -1));
                    //w.Write("," + (die.MapY * -1));
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

        #region Shared Helpers
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

                // 다른 웨이퍼 snapshot이면 덮어쓰지 않음(안전)
                var safeReq = SafeWaferId(waferId);
                var safeRow = SafeWaferId(row.WaferId);
                if (!string.IsNullOrWhiteSpace(safeReq) &&
                    !string.Equals(safeReq, safeRow, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                // 1) SUM의 EqpName/WaferID는 Summary 기준으로 통일
                if (!string.IsNullOrWhiteSpace(row.MachineName))
                    SumContext.EqpName = row.MachineName;

                if (!string.IsNullOrWhiteSpace(row.WaferId))
                    SumContext.WaferID = row.WaferId;

                // 2) SUM의 StartTime/EndTime도 Summary 기준으로 통일
                // WaferTotalSummary CSV는 HH:mm:ss만 저장하지만,
                // Summary row는 DateTime을 가지고 있으므로 여기서는 날짜까지 포함해 기록.
                if (row.Start != DateTime.MinValue)
                    SumContext.StartTime = row.Start.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

                if (row.End != DateTime.MinValue)
                    SumContext.EndTime = row.End.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            }
            catch
            {
                // Summary 동기화 실패는 공정 흐름을 막지 않음
            }
        }

        private string SafeWaferId(string waferId)
        {
            if (string.IsNullOrEmpty(waferId)) return "UNKNOWN";
            return waferId.Trim();
        }

        private void MergeTesterItems(PKGTesterResult result, Dictionary<string, double> store)
        {
            var dict = GetInternalItemDict(result);
            if (dict == null) return;
            foreach (var kv in dict)
            {
                if (kv.Key == null || kv.Value == null) continue;
                double val = kv.Value.Value;
                store[kv.Key] = val;
            }
        }

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

        private double GetItemValue(PKGTesterResult r, string key)
        {
            var dict = GetInternalItemDict(r);
            if (dict == null) return 0.0;
            TestItemResult item;
            if (dict.TryGetValue(key, out item) && item != null)
                return item.Value;
            return 0.0;
        }

        private List<string> CollectStrainKeys(Dictionary<string, double> mv)
        {
            var list = new List<string>();
            if (mv == null) return list;
            foreach (var kv in mv)
            {
                if (kv.Key.StartsWith("SG", StringComparison.OrdinalIgnoreCase))
                    list.Add(kv.Key);
            }
            list.Sort(StringComparer.OrdinalIgnoreCase);
            return list;
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
                double v;
                if (!map.TryGetValue(it, out v)) v = 0.0;
                w.Write("," + v);
            }
            w.WriteLine();
        }
        #endregion

        // 2) 메서드 추가: 웨이퍼 시작 시 초기화
        public void ResultLogData_BeginWaferSummary(string waferId, string eqpName = "UNKNOWN")
        {
            lock (_summaryLock)
            {
                SumContext.WaferID = string.IsNullOrEmpty(waferId) ? "UNKNOWN" : waferId.Trim();
                SumContext.EqpName = eqpName ?? "UNKNOWN";

                // [CHG] 기본값은 fallback
                SumContext.StartTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                SumContext.EndTime = "";
                // [ADD] 가능하면 SummaryContext 기준으로 덮어씀(SSOT)
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
                    {
                        _waferTotalSummaryWritten.Remove(safeId);
                    }
                }
            }
        }

        // 3) 메서드 추가: 다이 1개 반영 (저장 직후 호출)
        private readonly Dictionary<int, string> _binLabelMap = new Dictionary<int, string>();

        public void AccumulateDie(MaterialDie die)
        {
            if (die == null || die.TesterResult == null) return;

            lock (_summaryLock)
            {
                var itemsDict = GetInternalItemDict(die.TesterResult);
                if (itemsDict == null) return;

                SumContext.TotalCount++;

                if (die.IsPass) SumContext.GoodCount++; else SumContext.NGCount++;

                // die.TesterResult.BinningResult 에서 BinNo / BinLabel 파싱
                int rawBinNo = -1;
                string rawLabel = null;
                var br = die.TesterResult.BinningResult;
                if (br != null)
                {
                    rawBinNo = br.BinNo;
                    rawLabel = br.BinLabel;
                }
                // NG(-1) → 0번 Bin(특수)로 치환
                int storeBinNo = rawBinNo < 0 ? 0 : rawBinNo;

                int cur;
                SumContext.BinCounts.TryGetValue(storeBinNo, out cur);
                SumContext.BinCounts[storeBinNo] = cur + 1;

                // 라벨 누적: 유효 라벨이면 저장 (빈 번호가 양수 또는 0(NG→Special))
                if (!string.IsNullOrWhiteSpace(rawLabel))
                {
                    // NG 라벨은 "NG" 일 가능성 → 0번에 저장
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

                    double min;
                    if (!SumContext.Min.TryGetValue(key, out min) || val < min)
                        SumContext.Min[key] = val;

                    double max;
                    if (!SumContext.Max.TryGetValue(key, out max) || val > max)
                        SumContext.Max[key] = val;

                    double s;
                    _sum.TryGetValue(key, out s);
                    _sum[key] = s + val;

                    double s2;
                    _sumSq.TryGetValue(key, out s2);
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
                    double sum = 0; _sum.TryGetValue(key, out sum);
                    double sumSq = 0; _sumSq.TryGetValue(key, out sumSq);

                    double avg = sum / n;
                    double variance = (sumSq / n) - (avg * avg);
                    if (variance < 0) variance = 0; // 수치적 안정성
                    double std = Math.Sqrt(variance);

                    SumContext.Avg[key] = avg;
                    SumContext.Std[key] = std;
                }

                // [CHG] 기본은 fallback, 있으면 SummaryContext 기준으로 덮어씀
                SumContext.EndTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                SyncSumContextFromEquipmentSummaryIfPossible(SumContext.WaferID);

                _summaryFinalized = true;
            }
        }

        // 파일명에서 확장자를 제거.
        // removeAll = false : 마지막 한 개 확장자만 제거 (a.b.c.bin -> a.b.c)
        // removeAll = true  : 끝에서부터 점(.)을 가진 모든 확장자 제거 (a.b.c.bin -> a)
        // 비어있거나 공백이면 "NONE" 반환
        private string StripExtension(string name, bool removeAll = false)
        {
            if (string.IsNullOrWhiteSpace(name)) return "NONE";

            string justName = Path.GetFileName(name); // 경로 제거

            if (!removeAll)
            {
                // 단일 확장자만 제거
                return Path.GetFileNameWithoutExtension(justName);
            }

            // 모든 확장자(마지막 점 뒤 문자열) 반복 제거
            // 예) "wafer.log.bin" -> "wafer"
            int dot;
            while ((dot = justName.LastIndexOf('.')) > 0) // 맨 앞이 점(.)인 숨김 파일(.gitignore)은 유지
            {
                justName = justName.Substring(0, dot);
            }
            return justName;
        }

        /// <summary>
        /// TestConditionSet.Items 내용을 PRD 헤더 ParameterBlock에 벤더 포맷(예시 숫자 라인)으로 변환하여 적재.
        /// 라인 포맷 (추정 매핑):
        /// SourceValue SourceTime,MeasureTime Type(Source enum int) MeasureLimit Low High Gain0 Offset0
        /// - Low / High 값이 현재 클래스에 존재하지 않으므로 Expression에서 "a b" 형태 또는 "a~b" 형태로 추출 시도, 없으면 0.
        /// - Gain0 / Offset0 는 첫 번째 배열 요소 사용.
        /// - 측정/컴퓨트가 아닌 아이템은 MeasureTime / Gain0 / Offset0 를 기본값으로 출력.
        /// </summary>
        private void BuildTestConditionParameterBlock()
        {
            PrdContext.ParameterBlock.Clear();

            if (CurrentTestConditionSet == null || CurrentTestConditionSet.Items == null || CurrentTestConditionSet.Items.Count == 0)
                return;

            int seq = 1; // 순번 (1부터)
            foreach (var it in CurrentTestConditionSet.Items)
            {
                // 기존 값들
                double srcVal = it.SourceValue;      // applyval
                double srcTime = it.SourceTime;      // applytime
                // double measTime = it.MeasureTime; // 순서 요구에서 제외
                // double srcLimit = it.MeasureLimit; // 요구 포맷에 없음

                (double low, double high) = TryExtractRange(it.Expression);
                double gain0 = (it.Gain != null && it.Gain.Length > 0) ? it.Gain[0] : 0.0;
                double offset0 = (it.Offset != null && it.Offset.Length > 0) ? it.Offset[0] : 0.0;
                int typeCode = (int)it.Type;

                // 요구 순서:
                // 순번 typeCode applyval applytime 0 Low high gain offset
                // 구분자는 기존 포맷과 동일하게 공백 사용
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

        // Expression 문자열에서 범위 (Low, High) 추출: "a b", "a  b", "a~b"
        private (double low, double high) TryExtractRange(string expr)
        {
            if (string.IsNullOrWhiteSpace(expr))
                return (0, 0);

            // "~" 형태
            var tildeParts = expr.Split('~');
            if (tildeParts.Length == 2 &&
                double.TryParse(tildeParts[0].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out double low1) &&
                double.TryParse(tildeParts[1].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out double high1))
            {
                return (low1, high1);
            }

            // 공백 구분 숫자 2개 연속
            var tokens = expr.Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries)
                             .Where(t => double.TryParse(t, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                             .Select(t => t)
                             .ToList();
            if (tokens.Count >= 2 &&
                double.TryParse(tokens[0], NumberStyles.Any, CultureInfo.InvariantCulture, out double low2) &&
                double.TryParse(tokens[1], NumberStyles.Any, CultureInfo.InvariantCulture, out double high2))
            {
                return (low2, high2);
            }

            return (0, 0);
        }
        private void ResetPrdHeaderIfWaferChanged(string waferId)
        {
            waferId = SafeWaferId(waferId);
            lock (_headerLock)
            {
                if (!string.Equals(_lastPrdWaferId, waferId, StringComparison.OrdinalIgnoreCase))
                {
                    // 이전 웨이퍼 요약 마무리 (선택)
                    if (!string.IsNullOrEmpty(_lastPrdWaferId))
                        FinalizeSummary();

                    PrdContext.HeaderInitialized = false;
                    PrdContext.HeaderLines.Clear();
                    PrdContext.ParameterBlock.Clear();
                    PrdContext.ZeroBlock1.Clear();
                    PrdContext.ZeroBlock2.Clear();
                    PrdContext.DataColumns.Clear();

                    _lastPrdWaferId = waferId;

                    // 새 웨이퍼 요약 시작(선택)
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
        private readonly object _waferTotalSummaryOnceLock = new object();
        private readonly HashSet<string> _waferTotalSummaryWritten = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

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
                Log.Write("ResultWriterManager", nameof(GetWaferTotalSummaryFilePath), ex.Message);
            }

            return Path.Combine(BaseDir, fileName);
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

            string file = GetWaferTotalSummaryFilePath();
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
                        w.WriteLine(BuildWaferTotalSummaryTopLine(row.Date));
                        w.WriteLine(string.Join(",", WaferTotalSummaryColumns));
                    }

                    var pickers = row.Pickers ?? new string[0];
                    string GetPicker(int idx) => (idx < pickers.Length && !string.IsNullOrWhiteSpace(pickers[idx])) ? pickers[idx] : "USE";

                    // Yield: 예시처럼 소수 2자리
                    string yieldStr = row.Yield.ToString("0.##", CultureInfo.InvariantCulture);

                    // CycleTime은 예시처럼 소수점 4자리까지 찍힘. (요구에 맞춰 조정 가능)
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
            }

            return 0;
        }
    }
}