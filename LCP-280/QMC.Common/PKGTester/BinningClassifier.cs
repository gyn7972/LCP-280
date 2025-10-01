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
        #endregion

        #region Properties
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
        public BinningResult Classify(IReadOnlyDictionary<string, TestItemResult> data)
        {
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
        #endregion
    }
}
