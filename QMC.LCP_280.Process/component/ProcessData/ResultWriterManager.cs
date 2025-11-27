using QMC.Common;
using QMC.Common.Account;
using QMC.Common.IOUtil;
using QMC.Common.PKGTester;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using static QMC.LCP_280.Process.Component.ProcessData.ResultWriterManager;

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

        public PRDContext PrdContext { get; private set; } = new PRDContext();
        public SUMContext SumContext { get; private set; } = new SUMContext();
        public WAFContext WafContext { get; private set; } = new WAFContext();
        // TestConditionSet 연계: 외부에서 주입 (레시피 로딩 직후 할당)
        public TestConditionSet CurrentTestConditionSet { get; set; }
        private string _lastPrdWaferId;
        private string _lastWafWaferId;
        private readonly object _headerLock = new object();

        private static readonly string _runDate = DateTime.Now.ToString("yyyyMMdd");
        private string BaseDir { 
            get 
            { 
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ResultData", _runDate); 
            } 
        }
        private string LogDir { 
            get 
            { 
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", _runDate); 
            } 
        }

        private readonly object _ioLock = new object();


        // 1) 클래스 필드 영역(기존 필드들 아래)에 요약 계산용 내부 상태 추가
        private readonly object _summaryLock = new object();
        private Dictionary<string, double> _sum = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, double> _sumSq = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        private bool _summaryFinalized = false;

        public ResultWriterManager()
        {
            // 날짜 폴더 미리 생성
            try
            {
                Directory.CreateDirectory(BaseDir);
                Directory.CreateDirectory(LogDir);
            }
            catch { /* 필요시 Log.Write(e) */ }
        }


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
                    w.Write("," + (die.MapX * -1));
                    w.Write("," + (die.MapY * -1));
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

        #region Legacy Format Append API
        public int AppendTxTDie(MaterialDie die)
        {
            PKGTesterResult result = die.TesterResult;

            string waferId = SafeWaferId(die.SourceWaferId);
            string dir = Path.Combine(BaseDir, waferId);
            Directory.CreateDirectory(dir);
            //string file = Path.Combine(dir, waferId + "_" + DateTime.Now.ToString("yyyyMMdd") + ".txt");
            string file = Path.Combine(dir, waferId + ".txt");

            string strBinFileName = die.SourceBinFileName;//  "none.bin"; //불러와야함.
            lock (_ioLock)
            {
                bool exists = File.Exists(file);
                using (var w = new StreamWriter(file, true, Encoding.UTF8))
                {
                    if (!exists)
                    {
                        w.WriteLine(strBinFileName);
                        w.WriteLine(waferId);
                        w.Write("XADR,YADR,RANK");
                        //foreach (var sg in sgKeys) w.Write("," + sg);
                        foreach (var key in EnumerateItemKeys(result)) w.Write("," + key);
                        w.WriteLine();
                    }

                    // 데이터 행 추가.
                    var bin = result.BinningResult;
                    w.Write((die.MapX * -1).ToString());                        //XADR
                    w.Write("," + (die.MapY * -1).ToString());                  //YADR
                    w.Write("," + (bin != null ? bin.BinNo.ToString() : "0"));  //RANK
                    //foreach (var sg in sgKeys)
                    //{
                    //    double v = 0;
                    //    die.MeasureValues.TryGetValue(sg, out v);
                    //    w.Write("," + v);
                    //}
                    foreach (var v in EnumerateItemValues(result)) w.Write("," + v);
                    w.WriteLine();
                }
            }
            return 0;
        }
        public int AppendBinDie(MaterialDie die)
        {
            if (die == null || die.TesterResult == null) return -1;

            // 현재 다이의 BinResult를 먼저 파싱하여 라벨 맵 갱신 (단독 호출 상황 대비)
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
                // 라벨 맵 스냅샷
                labelSnapshot = new Dictionary<int, string>(_binLabelMap);
            }

            string waferId = SafeWaferId(die.SourceWaferId);
            string dir = Path.Combine(BaseDir, waferId);
            Directory.CreateDirectory(dir);
            string file = Path.Combine(dir, waferId + ".bin");

            lock (_ioLock)
            {
                using (var w = new StreamWriter(file, false, Encoding.UTF8))
                {
                    w.WriteLine("No,Name,Count");
                    w.WriteLine();

                    // BinNo 정렬(0(Special/NG)은 맨 뒤)
                    var ordered = binCountsSnapshot.Keys
                        .OrderBy(k => k == 0 ? int.MaxValue : k)
                        .ToList();

                    foreach (var no in ordered)
                    {
                        string name;
                        if (!labelSnapshot.TryGetValue(no, out name) || string.IsNullOrWhiteSpace(name))
                        {
                            // 라벨 없으면 규칙 적용
                            name = (no == 0) ? "Special" : $"SV700-HA-A-{no:00}";
                        }
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
            }

            return 0;
        }
        public int WriteSumFile(MaterialDie die)
        {
            FinalizeSummary(); // ← 추가

            //waferId = SafeWaferId(waferId);
            //if (string.IsNullOrEmpty(waferId)) return -1;
            //string dir = Path.Combine(BaseDir, waferId);
            //Directory.CreateDirectory(dir);
            //string file = Path.Combine(dir, waferId + ".sum");

            string waferId = SafeWaferId(die.SourceWaferId);
            string dir = Path.Combine(BaseDir, waferId);
            Directory.CreateDirectory(dir);
            string file = Path.Combine(dir, waferId + ".sum");
            lock (_ioLock)
            {
                using (var w = new StreamWriter(file, false, Encoding.UTF8))
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
            if(AccountManager.CurrentAccount != null)
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
            PrdContext.HeaderLines.Add("Filecreationtime,"+DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            PrdContext.HeaderLines.Add("-100"); //<- ?
            PrdContext.HeaderLines.Add("67"); //<- ?
            PrdContext.HeaderLines.Add("46"); //<- ?
            PrdContext.HeaderLines.Add("-75"); //<- ?
            PrdContext.HeaderLines.Add("1"); //<- ?
            PrdContext.HeaderLines.Add("1"); //<- ?
            PrdContext.HeaderLines.Add("13089"); //<-전체 검사 갯수.. //SumContext.TotalCount // 이 항목은 계속 업데이트 되어야함.
            PrdContext.HeaderLines.Add(waferId);
            PrdContext.HeaderLines.Add(waferId);
            PrdContext.HeaderLines.Add(binFile);
            PrdContext.HeaderLines.Add(binFile);
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
            string dir = Path.Combine(BaseDir, waferId);
            Directory.CreateDirectory(dir);
            string file = Path.Combine(dir, waferId + ".prd");
            var r = die.TesterResult;

            lock (_ioLock)
            {
                bool exists = File.Exists(file);
                using (var w = new StreamWriter(file, true, Encoding.UTF8))
                {
                    if (!exists)
                    {
                        //file없으면 여기서 header한번만들고 쓰자.
                        BuildPrdHeader(die);

                        // 상단 메타 헤더 출력
                        foreach (var line in PrdContext.HeaderLines)
                            w.WriteLine(line);
                        // (Item Parameter)
                        foreach (var line in PrdContext.ParameterBlock)
                            w.WriteLine(line);
                        // (Zero 블록 유지)
                        foreach (var line in PrdContext.ZeroBlock1)
                            w.WriteLine(line);
                        foreach (var line in PrdContext.ZeroBlock2)
                            w.WriteLine(line);

                        // CSV 헤더 출력
                        w.WriteLine(string.Join(",", PrdContext.DataColumns));
                    }
                    else
                    {
                        // 재시작 등으로 DataColumns 비어 있으면 복구 시도
                        if (!PrdContext.HeaderInitialized)
                            BuildPrdHeader(die);
                    }

                    // Rank (TesterResult 우선, 없으면 Die.Rank)
                    int rank = r.BinningResult != null ? r.BinningResult.BinNo : die.Rank;
                    // 값 매핑 사전 준비 (아이템 값)
                    var itemDict = GetInternalItemDict(r) ?? new Dictionary<string, TestItemResult>();
                    var sb = new StringBuilder(256);
                    foreach (var col in PrdContext.DataColumns)
                    {
                        if (sb.Length > 0) sb.Append(',');
                        switch (col)
                        {
                            case "XADR": sb.Append(die.MapX * -1); break;
                            case "YADR": sb.Append(die.MapY * -1); break;
                            case "RANK": sb.Append(rank); break;
                            case "Index": sb.Append(die.SocketIndex + 1); break;
                            default:
                                {
                                    TestItemResult ti;
                                    if (itemDict.TryGetValue(col, out ti) && ti != null)
                                        sb.Append(ti.Value);
                                    else
                                        sb.Append("0");
                                }
                                break;
                        }
                    }
                    w.WriteLine(sb.ToString());
                }

                // 헤더 라인 내 TotalCount 값을 최신으로 교체 (파일 존재 시)
                if (exists)
                {
                    // 메모리 헤더도 갱신 (재생성 시 일관성 유지)
                    if (PrdContext.HeaderLines.Count > PRD_TOTALCOUNT_HEADER_INDEX)
                        PrdContext.HeaderLines[PRD_TOTALCOUNT_HEADER_INDEX] = SumContext.TotalCount.ToString();

                    UpdatePrdHeaderTotalCountInFile(file);
                }
            }

            return 0;
        }

        //public int AppendPrdDie(MaterialDie die)
        //{
        //    if (die == null || die.TesterResult == null) 
        //        return -1;

        //    string waferId = SafeWaferId(die.SourceWaferId);
        //    string dir = Path.Combine(BaseDir, waferId);
        //    Directory.CreateDirectory(dir);
        //    string file = Path.Combine(dir, waferId + ".prd");
        //    var r = die.TesterResult;

        //    lock (_ioLock)
        //    {
        //        bool exists = File.Exists(file);
        //        using (var w = new StreamWriter(file, true, Encoding.UTF8))
        //        {
        //            if (!exists)
        //            {
        //                //file없으면 여기서 header한번만들고 쓰자.
        //                BuildPrdHeader(die);

        //                // 상단 메타 헤더 출력
        //                foreach (var line in PrdContext.HeaderLines)
        //                    w.WriteLine(line);
        //                // (Item Parameter)
        //                foreach (var line in PrdContext.ParameterBlock)
        //                    w.WriteLine(line);
        //                // (Zero 블록 유지)
        //                foreach (var line in PrdContext.ZeroBlock1)
        //                    w.WriteLine(line);
        //                foreach (var line in PrdContext.ZeroBlock2)
        //                    w.WriteLine(line);

        //                // CSV 헤더 출력
        //                w.WriteLine(string.Join(",", PrdContext.DataColumns));
        //            }
        //            else
        //            {
        //                // 재시작 등으로 DataColumns 비어 있으면 복구 시도
        //                if (!PrdContext.HeaderInitialized)
        //                    BuildPrdHeader(die);
        //            }

        //            // Rank (TesterResult 우선, 없으면 Die.Rank)
        //            int rank = r.BinningResult != null ? r.BinningResult.BinNo : die.Rank;
        //            // 값 매핑 사전 준비 (아이템 값)
        //            var itemDict = GetInternalItemDict(r) ?? new Dictionary<string, TestItemResult>();
        //            var sb = new StringBuilder(256);
        //            foreach (var col in PrdContext.DataColumns)
        //            {
        //                if (sb.Length > 0) sb.Append(',');
        //                switch (col)
        //                {
        //                    case "XADR": sb.Append(die.MapX * -1); break;
        //                    case "YADR": sb.Append(die.MapY * -1); break;
        //                    case "RANK": sb.Append(rank); break;
        //                    case "Index": sb.Append(die.SocketIndex + 1); break;
        //                    default:
        //                        {
        //                            TestItemResult ti;
        //                            if (itemDict.TryGetValue(col, out ti) && ti != null)
        //                                sb.Append(ti.Value);
        //                            else
        //                                sb.Append("0");
        //                        }
        //                        break;
        //                }
        //            }
        //            w.WriteLine(sb.ToString());
        //        }
        //    }
        //    return 0;
        //}

        // ResultWriterManager 클래스 내부

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
            string dir = Path.Combine(BaseDir, waferId);
            Directory.CreateDirectory(dir);
            string file = Path.Combine(dir, waferId + ".waf");
            PKGTesterResult r = die.TesterResult;

            // 높이 측정값 (없으면 0.0)
            double height = die.GetMeasure("Height").HasValue ? die.GetMeasure("Height").Value : 0.0;

            lock (_ioLock)
            {
                bool exists = File.Exists(file);

                // 아직 DataColumns 안 만들어졌으면 한 번만 구성
                if (!WafContext.HeaderInitialized)
                {
                    BuildWafHeader(die);
                }

                using (var w = new StreamWriter(file, true, Encoding.UTF8))
                {
                    // 파일이 처음 만들어질 때만 앞부분 헤더/파라미터/0 블록 출력
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

                        // ★ 아이템 이름 라인(CSV 헤더)은 WAF에 쓰지 않는다 ★
                        // (예제 포맷처럼 바로 데이터만 이어짐)
                    }

                    // 실제 측정 아이템 값들 가져오기
                    var dict = GetInternalItemDict(r)
                               ?? new Dictionary<string, TestItemResult>(StringComparer.OrdinalIgnoreCase);

                    var sb = new StringBuilder(256);

                    // 1) 좌표 (예제: -50,63, …)
                    sb.Append((die.MapX * -1).ToString(CultureInfo.InvariantCulture));
                    sb.Append(',');
                    sb.Append((die.MapY * -1).ToString(CultureInfo.InvariantCulture));
                    // 필요하면 여기서 Y 부호만 바꿔서 튜닝 가능 (예제와 비교해서 조정)

                    // 측정 아이템 값
                    foreach (var key in WafContext.DataColumns)
                    {
                        sb.Append(',');
                        TestItemResult item;
                        if (dict.TryGetValue(key, out item) && item != null)
                            sb.Append(item.Value.ToString("0.#####", CultureInfo.InvariantCulture));
                        else
                            sb.Append("0");
                    }

                    // Index (SocketIndex + 1) 및 Height
                    sb.Append(',');
                    sb.Append((die.SocketIndex + 1).ToString(CultureInfo.InvariantCulture));
                    sb.Append(',');
                    sb.Append(height.ToString("0.000", CultureInfo.InvariantCulture));

                    w.WriteLine(sb.ToString());
                }
            }

            return 0;
        }

        #endregion

        #region Shared Helpers
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
        public void BeginWaferSummary(string waferId, string eqpName = "UNKNOWN")
        {
            lock (_summaryLock)
            {
                SumContext.WaferID = string.IsNullOrEmpty(waferId) ? "UNKNOWN" : waferId.Trim();
                SumContext.EqpName = eqpName ?? "UNKNOWN";
                SumContext.StartTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                SumContext.EndTime = "";
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

                SumContext.EndTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
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
            //foreach (var it in CurrentTestConditionSet.Items)
            //{
            //    double srcVal = it.SourceValue;
            //    double srcTime = it.SourceTime;
            //    double measTime = it.MeasureTime;
            //    double srcLimit = it.MeasureLimit;

            //    // Expression 에서 Low/High 추출 시도 (예: "255 290", "2.9 3.25", "452.5 460" 또는 "255~290")
            //    (double low, double high) = TryExtractRange(it.Expression);

            //    // Gain / Offset 첫 번째 값
            //    double gain0 = (it.Gain != null && it.Gain.Length > 0) ? it.Gain[0] : 0.0;
            //    double offset0 = (it.Offset != null && it.Offset.Length > 0) ? it.Offset[0] : 0.0;

            //    // 타입 enum → 정수 출력
            //    int typeCode = (int)it.Type;

            //    // 포맷: "SourceValue SourceTime,MeasureTime Type MeasureLimit Low High Gain0 Offset0"
            //    // 소수 표현 규칙: 예제에 맞춰 기본은 필요한 자리만, Gain/Offset은 0.##### / High/Low는 입력 그대로
            //    string line =
            //        srcVal.ToString("0.#####", CultureInfo.InvariantCulture) + " " +
            //        srcTime.ToString("0.#####", CultureInfo.InvariantCulture) + "," +
            //        measTime.ToString("0.#####", CultureInfo.InvariantCulture) + " " +
            //        typeCode + " " +
            //        srcLimit.ToString("0.#####", CultureInfo.InvariantCulture) + " " +
            //        low.ToString("0.#####", CultureInfo.InvariantCulture) + " " +
            //        high.ToString("0.#####", CultureInfo.InvariantCulture) + " " +
            //        gain0.ToString("0.#####", CultureInfo.InvariantCulture) + " " +
            //        offset0.ToString("0.#####", CultureInfo.InvariantCulture);

            //    PrdContext.ParameterBlock.Add(line);
            //    WafContext.ParameterBlock.Add(line);
            //    SumContext.ParameterBlock.Add(line);
            //}

            // 아래 ZeroBlock1 / ZeroBlock2 는 기존 유지(초기화되지 않았다면 호출 측에서 채워짐)
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
                    BeginWaferSummary(waferId, SumContext.EqpName);
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
    }
}