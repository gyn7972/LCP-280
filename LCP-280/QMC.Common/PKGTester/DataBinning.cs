using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

namespace QMC.Common.PKGTester
{
    public enum BinningType
    {
        None,
        GoodBin,
        NgBin,
    };

    /// <summary>
    /// 데이터 비닝을 위한 범위 설정 클래스
    /// </summary>
    public class BinningRange : BaseConfig
    {
        #region Properties
        public double Min { get; set; }
        public double Max { get; set; }
        public bool IgnoreOutOfRange { get; set; }
        #endregion

        #region Constructors
        public BinningRange(string name) : base(name)
        {
            Reset();
        }
        #endregion

        #region Methods
        public override void Reset()
        {
            Min = 0;
            Max = 0;
            IgnoreOutOfRange = false;
        }
        public override bool Validate()
        {
            if (!IgnoreOutOfRange)
            {
                if (Min >= Max)
                    return false;
            }
            return true;
        }
        public bool IsInRange(double value)
        {
            if (IgnoreOutOfRange)
                return true;

            return (value >= Min && value <= Max);
        }
        public override PropertyCollection GetPropertyCollection()
        {
            var pc = new PropertyCollection();
            pc.Add("Binning Range");
            pc.Add("Name", Name); 
            pc.Add("Min", Min);
            pc.Add("Max", Max);
            pc.Add("IgnoreOutOfRange", IgnoreOutOfRange);
            return pc;
        }
        public override int ApplyValueFromPropertyCollection(PropertyCollection pc)
        {
            if (pc == null)
                return -1;
            try
            {
                Name = pc.GetValue<string>("Name");
                Min = pc.GetValue<double>("Min");
                Max = pc.GetValue<double>("Max");
                IgnoreOutOfRange = pc.GetValue<bool>("IgnoreOutOfRange");
            }
            catch (Exception ex)
            {
                // 필요시 로그 처리
                Log.Write(ex);
                return -1;
            }
            return 0;
        }
        public override string ToString()
        {
            if (IgnoreOutOfRange)
                return $"(Ignore)";
            else
                return $"{Name}: [{Min} ~ {Max}]";
        }
        #endregion
    }

    /// <summary>
    /// 레이블과 비닝 범위 컬렉션을 포함한 비닝에 대한 사양을 나타냅니다.
    /// </summary>
    public class BinningSpec
    {
        #region Fields
        private Dictionary<string, BinningRange> items = new Dictionary<string, BinningRange>();
        #endregion

        #region Properties
        public string BinLabel { get; set; }
        public Dictionary<string, BinningRange> Items => items;
        #endregion

        #region Constructors
        public BinningSpec(string binLabel)
        {
            BinLabel = binLabel;
        }
        #endregion

        #region Methods
        public void Clear()
        {
            items.Clear();
        }
        public void ResetItems()
        {
            foreach (var item in items.Values)
            {
                item.Reset();
            }
        }
        #endregion
    }

    /// <summary>
    /// 헤더와 빈 사양(BinningSpec)을 포함한 빈 구성 관리를 위한 사양 시트를 나타냅니다.
    /// </summary>
    public class BinningSpecSheet
    {
        #region Fields
        private List<string> headers = new List<string>();
        private List<BinningSpec> specs = new List<BinningSpec>();
        #endregion

        #region Properties
        public IReadOnlyList<BinningSpec> Specs => specs;
        #endregion

        #region Constructors
        public BinningSpecSheet()
        {
        }
        #endregion

        #region Methods
        public void Clear()
        {
            headers.Clear();
            foreach (var spec in specs)
            {
                spec.Clear();
            }
            specs.Clear();
        }
        public void Reset()
        {
            foreach (var spec in specs)
            {
                spec.ResetItems();
            }
        }
        public void AddHeader(string header)
        {
            if (!headers.Contains(header))
            {
                foreach (var spec in specs)
                {
                    spec.Items.Add(header, new BinningRange(""));
                }
                headers.Add(header);
            }
        }
        public bool AddNewBin(string binLabel)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(binLabel))
                    return false;
                if (binLabel == "NG")
                    return false;
                if (specs.Exists(s => s.BinLabel == binLabel))
                    return false;

                var newSpec = new BinningSpec(binLabel);
                foreach (var header in headers)
                {
                    newSpec.Items.Add(header, new BinningRange(""));
                }
                specs.Add(newSpec);
            }
            catch
            {
                return false;
            }
            return true;
        }
        public bool Validate()
        {
            if (specs.Count == 0)
                return false;
            if (headers.Count == 0)
                return false;

            return true;
        }
        public bool CopyFrom(BinningSpecSheet sheet)
        {
            try
            {
                if (sheet == null)
                    return false;
                if (!sheet.Validate())
                    return false;

                Clear();
                foreach (var header in sheet.headers)
                {
                    AddHeader(header);
                }
                foreach (var spec in sheet.specs)
                {
                    if (!AddNewBin(spec.BinLabel))
                        return false;
                    var newSpec = specs.Find(s => s.BinLabel == spec.BinLabel);
                    foreach (var header in headers)
                    {
                        if (spec.Items.ContainsKey(header) && newSpec.Items.ContainsKey(header))
                        {
                            var srcRange = spec.Items[header];
                            var dstRange = newSpec.Items[header];
                            dstRange.Min = srcRange.Min;
                            dstRange.Max = srcRange.Max;
                            dstRange.IgnoreOutOfRange = srcRange.IgnoreOutOfRange;
                        }
                    }
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
        public bool LoadFromFile(string filePath)
        {
            try
            {
                Clear();
                using (var reader = new System.IO.StreamReader(filePath, System.Text.Encoding.UTF8))
                {
                    // 헤더 읽기
                    var headerLine = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(headerLine))
                        return false;

                    var headerParts = headerLine.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (headerParts.Length < 2 || headerParts[0] != "BinLabel")
                        return false;
                    
                    for (int i = 1; i < headerParts.Length; i++)
                    {
                        AddHeader(headerParts[i].Trim());
                    }
                    
                    // 데이터 읽기
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var parts = line.Split(new char[] { ',' }, StringSplitOptions.None);
                        if (parts.Length != headers.Count + 1)
                            return false;
                        var binLabel = parts[0].Trim();
                        if (!AddNewBin(binLabel))
                            return false;

                        var spec = specs.Find(s => s.BinLabel == binLabel);
                        for (int i = 1; i < parts.Length; i++)
                        {
                            var header = headers[i - 1];
                            var rangeStr = parts[i].Trim();
                            if (rangeStr == "(Ignore)")
                            {
                                spec.Items[header].IgnoreOutOfRange = true;
                            }
                            else if (rangeStr.Contains("~"))
                            {
                                var rangeParts = rangeStr.Split('~');
                                if (rangeParts.Length != 2)
                                    return false;

                                if (double.TryParse(rangeParts[0], out double min) && double.TryParse(rangeParts[1], out double max))
                                {
                                    spec.Items[header].Min = min;
                                    spec.Items[header].Max = max;
                                    spec.Items[header].IgnoreOutOfRange = false;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            else if (string.IsNullOrWhiteSpace(rangeStr))
                            {
                                // 빈 값은 무시
                                continue;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
        public bool SaveToFile(string filePath)
        {
            try
            {
                var dir = System.IO.Path.GetDirectoryName(filePath);
                if (!System.IO.Directory.Exists(dir))
                {
                    System.IO.Directory.CreateDirectory(dir);
                }

                using (var writer = new System.IO.StreamWriter(filePath, false, System.Text.Encoding.UTF8))
                {
                    // 헤더 작성
                    writer.Write("BinLabel");
                    foreach (var header in headers)
                    {
                        writer.Write($",{header}");
                    }
                    writer.WriteLine();

                    // 데이터 작성
                    foreach (var spec in specs)
                    {
                        writer.Write(spec.BinLabel);
                        foreach (var header in headers)
                        {
                            if (spec.Items.TryGetValue(header, out var range))
                            {
                                if (range.IgnoreOutOfRange)
                                    writer.Write(",(Ignore)");
                                else
                                    writer.Write($",{range.Min}~{range.Max}");
                            }
                            else
                            {
                                writer.Write(",");
                            }
                        }
                        writer.WriteLine();
                    }
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
        #endregion
    }

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
