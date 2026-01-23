using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common.PKGTester
{
    public enum BinningType
    {
        None,
        GoodBin,
        NgBin,
    };

    public class BinningResult
    {
        #region Properties
        public int BinNo { get; set; }
        public BinningType BinType { get; set; }
        public string BinLabel { get; set; }
        #endregion

        #region Constructors
        public BinningResult()
        {
            Reset();
        }
        #endregion

        #region Methods
        public void Reset()
        {
            BinNo = -1;
            BinType = BinningType.None;
            BinLabel = "";
        }
        public bool CopyFrom(BinningResult result)
        {
            if (result == null)
                return false;

            BinNo = result.BinNo;
            BinType = result.BinType;
            BinLabel = result.BinLabel;
            return true;
        }
        #endregion
    }

    /// <summary>
    /// 사전 정의된 사양에 따라 데이터를 특정 빈에 할당하는 분류기를 나타냅니다.
    /// </summary>
    public class BinningClassifier
    {
        #region Fields
        private BinningSpecSheet specSheet = new BinningSpecSheet();

        // Simulation 지원
        public bool IsSimulation { get; set; }
        public bool IsDryRun { get; set; }
        private static readonly object _randLock = new object();
        private static readonly Random _rand = new Random();
        // Simulation 시 GoodBin 배정 확률 (0~1)
        public double SimulationGoodProbability { get; set; } = 0.75;
        #endregion

        #region Properties
        #endregion

        #region Public Accessors (추가)
        /// <summary>
        /// 내부 SpecSheet의 헤더/빈 스펙을 읽기 전용으로 반환합니다.
        /// </summary>
        public BinningSpecSheet GetSpecSheet()
        {
            return specSheet;
        }

        /// <summary>
        /// 지정한 BinLabel에 해당하는 항목별 Range 딕셔너리를 복사하여 반환합니다.
        /// 존재하지 않으면 null 반환.
        /// </summary>
        public IReadOnlyDictionary<string, BinningRange> GetRangesForBin(string binLabel)
        {
            if (string.IsNullOrWhiteSpace(binLabel))
                return null;
            var spec = specSheet.Specs.FirstOrDefault(s => string.Equals(s.BinLabel, binLabel, StringComparison.OrdinalIgnoreCase));
            if (spec == null)
                return null;
            // 복사본 생성(외부에서 값 변경 못하도록)
            var copy = new Dictionary<string, BinningRange>(spec.Items.Count);
            foreach (var kv in spec.Items)
            {
                var r = kv.Value;
                var nr = new BinningRange(kv.Key)
                {
                    Min = r.Min,
                    Max = r.Max,
                    Ignore = r.Ignore
                };
                copy.Add(kv.Key, nr);
            }
            return copy;
        }
        #endregion

        #region Constructors
        public BinningClassifier()
        {
        }
        #endregion

        #region Methods
        public void Clear()
        {
            specSheet.Clear();
        }
        public bool AssignSpecSheet(BinningSpecSheet sheet)
        {
            return specSheet.CopyFrom(sheet);
        }

        //Rank 분류를 여기서 매긴다.
        public BinningResult Classify(IReadOnlyDictionary<string, TestItemResult> data)
        {
            // Simulation / DryRun이면 랜덤 분류
            if (IsSimulation || IsDryRun)
            {
                return ClassifyRandom();
            }

            BinningResult result = new BinningResult();
            try
            {
                for (int binIndex = 0; binIndex < specSheet.Specs.Count; binIndex++)
                {
                    var spec = specSheet.Specs[binIndex];
                    bool allInRange = true;
                    foreach (var header in spec.Items.Keys)
                    {
                        if (data.ContainsKey(header))
                        {
                            var testItemResult = data[header];
                            var binningRange = spec.Items[header];
                            if (!binningRange.IsInRange(testItemResult.Value))
                            {
                                allInRange = false;
                                break;
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }

                    // 사양을 모두 만족하는 경우 해당 빈을 결과로 설정하고 반환
                    if (allInRange)
                    {
                        result.BinNo = binIndex + 1; // 1-based index
                        result.BinType = BinningType.GoodBin;
                        result.BinLabel = spec.BinLabel;
                        return result;
                    }
                }

                // 어떤 빈에도 속하지 않는 경우
                result.BinNo = -1;
                result.BinType = BinningType.NgBin;
                result.BinLabel = "NG";
                return result;
            }
            catch
            {
                result.Reset();
            }
            return result;
        }

        private BinningResult ClassifyRandom()
        {
            var result = new BinningResult();
            try
            {
                int specCount = specSheet.Specs.Count;
                if (specCount <= 0)
                {
                    // 사양 없으면 전부 NG
                    result.BinNo = -1;
                    result.BinType = BinningType.NgBin;
                    result.BinLabel = "NG";
                    return result;
                }

                double p;
                lock (_randLock) { p = _rand.NextDouble(); }

                if (p <= SimulationGoodProbability)
                {
                    int pickIndex;
                    lock (_randLock) { pickIndex = _rand.Next(specCount); }
                    var spec = specSheet.Specs[pickIndex];
                    result.BinNo = pickIndex + 1;
                    result.BinType = BinningType.GoodBin;
                    result.BinLabel = spec.BinLabel;
                }
                else
                {
                    result.BinNo = -1;
                    result.BinType = BinningType.NgBin;
                    result.BinLabel = "NG";
                }
            }
            catch
            {
                result.Reset();
            }
            return result;
        }
        #endregion
    }
}
