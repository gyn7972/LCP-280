using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace QMC.Common.PKGTester
{
    /// <summary>
    /// *.ITF 파일 전체 구조 루트 모델.
    ///  - Scalars: 단일 키
    ///  - Arrays: 배열형 키 (key[index] = value)
    ///  - 편의 파싱 필드: Header / Items / Contacts / AfterItemNums
    ///  - 변경 후 저장 시 Raw를 재구성 (RebuildRaw)
    /// </summary>
    public sealed class ItfTesterData
    {
        // ===== Sentinel 정의 =====
        public const string SentinelInt = "-12851";
        public const string SentinelInt2 = "-13108";
        public const string SentinelDoubleFormatted = "-1.285100e+004"; // SentinelInt의 지수표현 포맷
        private static readonly string[] SentinelValues = { SentinelInt, SentinelInt2, SentinelDoubleFormatted };

        // ===== Raw =====
        public Dictionary<string, string> Scalars { get; private set; } =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, Dictionary<int, string>> Arrays { get; private set; } =
            new Dictionary<string, Dictionary<int, string>>(StringComparer.OrdinalIgnoreCase);

        // ===== Header-ish =====
        public int? ItemCount { get; private set; }
        public int? TotalCount { get; private set; }
        public int? OpticStart { get; private set; }
        public bool? Inspection { get; private set; }
        public bool? DCMap { get; private set; }
        public int? CieRankShape { get; private set; }

        // ===== Structured =====
        public List<ItfItem> Items { get; private set; } = new List<ItfItem>();
        public List<int> AfterItemNums { get; private set; } = new List<int>();
        public List<ItfContactSetting> Contacts { get; private set; } = new List<ItfContactSetting>();

        // 원본 라인(필요시 참조)
        public List<string> OriginalLines { get; private set; } = new List<string>();

        // 변경 플래그
        public bool IsDirty { get; private set; }

        // 관리되는(재생성 대상) 배열 키 집합
        private static readonly HashSet<string> KnownArrayKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Item","ItemName","ItemName2","ItemUnit","KeyChNo","OpenCheckFlag",
            "SourceRange","SourceValue","MeasureRange","MeasureUnit","MeasureLow","MeasureHigh","MeasureLimit",
            "SreDelay","WaitTime","OffTime","NplcTime","OpticDCUsed","NGSkip","Optical","StrSourceUnit",
            "WaveCount","ExposeCount","IvRange","Polarity",
            "Full RangeMin","Full RangeMax","2nd RangeMin","2nd RangeMax",
            "OpticItemRawLevel","OpticItemRawHighLevel","OpticItemRawName","OpticItemRawUse",
            "OpticItemPdLowLevel","OpticItemPdHighLevel","OpticItemPdName","OpticItemPdUse",
            "OpticItemR3Level","OpticItemR3HighLevel","OpticItemR3Name","OpticItemR3Use",
            "WaveItemWPLowLevel","WaveItemWPHighLevel","WaveItemWPName","WaveItemWPUse",
            "WaveItemWHLowLevel","WaveItemWHHighLevel","WaveItemWHName","WaveItemWHUse",
            "WaveItemWXLowLevel","WaveItemWXHighLevel","WaveItemWXName","WaveItemWXUse",
            "WaveItemWYLowLevel","WaveItemWYHighLevel","WaveItemWYName","WaveItemWYUse",
            "WaveItemWDLowLevel","WaveItemWDHighLevel","WaveItemWDName","WaveItemWDUse",
            "WaveItemWPULowLevel","WaveItemWPUHighLevel","WaveItemWPUName","WaveItemWPUUse",
            "WaveItemCCTLowLevel","WaveItemCCTHighLevel","WaveItemCCTName","WaveItemCCTUse",
            "WaveItemCIELowLevel","WaveItemCIEHighLevel","WaveItemCIEName","WaveItemCIEUse",
            "WaveItemCRILowLevel","WaveItemCRIHighLevel","WaveItemCRIName","WaveItemCRIUse",
            "WaveItemCRI9LowLevel","WaveItemCRI9HighLevel","WaveItemCRI9Name","WaveItemCRI9Use",
            "WaveItem2ndPLowLevel","WaveItem2ndPHighLevel","WaveItem2ndPName","WaveItem2ndPUse",
            "WaveItem3ndPLowLevel","WaveItem3ndPHighLevel","WaveItem3ndPName","WaveItem3ndPUse",
            "IntegrationTime","EsdMode","EsdCount","VfStep",
            "CalcItem",
            "AfterItemNum",
            "ContactUse","FailOnSkip","ContactLow","ContactHigh","ContactOp1","ContactOp2","ContactOK"
        };

        public ItfTesterData() { }

        // ======== Load ========
        public static ItfTesterData Load(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                throw new FileNotFoundException("ITF file not found", path);

            var lines = File.ReadAllLines(path);
            var data = new ItfTesterData();
            data.OriginalLines.AddRange(lines);

            data.ParseLines(lines);
            data.ParseHeader();
            data.BuildItems();
            data.BuildAfterItemNums();
            data.BuildContacts();

            data.IsDirty = false;
            return data;
        }

        // ======== Public 수정 API ========

        public void SetHeader(int? itemCount = null, int? totalCount = null, int? opticStart = null,
                              bool? inspection = null, bool? dcMap = null, int? cieRankShape = null)
        {
            ItemCount = itemCount;
            TotalCount = totalCount;
            OpticStart = opticStart;
            Inspection = inspection;
            DCMap = dcMap;
            CieRankShape = cieRankShape;
            MarkDirty();
        }

        // ---- Items 조작 ----
        public void AddItem(ItfItem item)
        {
            if (item == null) return;
            item.Index = Items.Count;
            Items.Add(item);
            MarkDirty();
        }

        public void InsertItem(int index, ItfItem item)
        {
            if (item == null) return;
            if (index < 0 || index > Items.Count) index = Items.Count;
            Items.Insert(index, item);
            ReindexItems();
            MarkDirty();
        }

        public void RemoveItemAt(int index)
        {
            if (index < 0 || index >= Items.Count) return;
            Items.RemoveAt(index);
            ReindexItems();
            MarkDirty();
        }

        public void MoveItemUp(int index)
        {
            if (index <= 0 || index >= Items.Count) return;
            var tmp = Items[index - 1];
            Items[index - 1] = Items[index];
            Items[index] = tmp;
            ReindexItems();
            MarkDirty();
        }

        public void MoveItemDown(int index)
        {
            if (index < 0 || index >= Items.Count - 1) return;
            var tmp = Items[index + 1];
            Items[index + 1] = Items[index];
            Items[index] = tmp;
            ReindexItems();
            MarkDirty();
        }

        public void UpdateItem(int index, Action<ItfItem> mutator)
        {
            if (mutator == null) return;
            if (index < 0 || index >= Items.Count) return;
            mutator(Items[index]);
            MarkDirty();
        }

        private void ReindexItems()
        {
            for (int i = 0; i < Items.Count; i++)
                Items[i].Index = i;
        }

        // ---- Contacts 조작 ----
        public void AddContact(ItfContactSetting c)
        {
            if (c == null) return;
            if (c.Index < 0) c.Index = Contacts.Count > 0 ? Contacts.Max(x => x.Index) + 1 : 0;
            Contacts.Add(c);
            MarkDirty();
        }

        public void UpsertContact(ItfContactSetting c)
        {
            if (c == null) return;
            var existing = Contacts.FirstOrDefault(x => x.Index == c.Index);
            if (existing != null)
            {
                existing.Use = c.Use;
                existing.FailOnSkip = c.FailOnSkip;
                existing.Low = c.Low;
                existing.High = c.High;
                existing.Op1 = c.Op1;
                existing.Op2 = c.Op2;
                existing.OK = c.OK;
            }
            else
            {
                Contacts.Add(c);
            }
            MarkDirty();
        }

        public void RemoveContact(int index)
        {
            var target = Contacts.FirstOrDefault(x => x.Index == index);
            if (target != null)
            {
                Contacts.Remove(target);
                MarkDirty();
            }
        }

        public void ClearContacts()
        {
            Contacts.Clear();
            MarkDirty();
        }

        // ---- AfterItemNums 조작 ----
        public void ReplaceAfterItemNums(IEnumerable<int> nums)
        {
            AfterItemNums = nums != null ? nums.ToList() : new List<int>();
            MarkDirty();
        }

        // ---- Raw 직접 수정 (Scalars / Arrays) ----
        public void SetScalar(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key)) return;
            if (value == null)
            {
                Scalars.Remove(key);
            }
            else
            {
                Scalars[key] = value;
            }
            MarkDirty();
        }

        public void RemoveScalar(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return;
            if (Scalars.Remove(key))
                MarkDirty();
        }

        public void SetArrayValue(string baseKey, int index, string value)
        {
            if (string.IsNullOrWhiteSpace(baseKey) || index < 0) return;
            if (value == null)
            {
                if (Arrays.TryGetValue(baseKey, out var dict))
                {
                    if (dict.Remove(index))
                        MarkDirty();
                    if (dict.Count == 0)
                        Arrays.Remove(baseKey);
                }
                return;
            }
            if (!Arrays.TryGetValue(baseKey, out var bucket))
            {
                bucket = new Dictionary<int, string>();
                Arrays[baseKey] = bucket;
            }
            bucket[index] = value;
            MarkDirty();
        }

        public void RemoveArrayValue(string baseKey, int index)
        {
            if (string.IsNullOrWhiteSpace(baseKey) || index < 0) return;
            if (Arrays.TryGetValue(baseKey, out var dict))
            {
                if (dict.Remove(index))
                    MarkDirty();
                if (dict.Count == 0)
                    Arrays.Remove(baseKey);
            }
        }

        // ======== Save ========
        public int Save(string path, bool preserveUnknownKeys = true)
        {
            if (string.IsNullOrWhiteSpace(path))
                return -1;

            try
            {
                // 구조를 Raw로 재구성
                RebuildRaw(preserveUnknownKeys);

                var sb = new StringBuilder(4096);
                sb.AppendLine("[TESTER_DATA]");

                // ItemCount 가장 먼저
                if (Scalars.TryGetValue("ItemCount", out var ic))
                {
                    sb.AppendLine($"ItemCount={ic}");
                }

                foreach (var kv in Scalars
                         .Where(k => !k.Key.Equals("ItemCount", StringComparison.OrdinalIgnoreCase))
                         .OrderBy(k => k.Key, StringComparer.OrdinalIgnoreCase))
                {
                    sb.AppendLine($"{kv.Key}={kv.Value}");
                }

                foreach (var baseKey in Arrays.Keys.OrderBy(k => k, StringComparer.OrdinalIgnoreCase))
                {
                    var idxMap = Arrays[baseKey];
                    foreach (var ekv in idxMap.OrderBy(k => k.Key))
                    {
                        sb.AppendLine($"{baseKey}[{ekv.Key}]={ekv.Value}");
                    }
                }

                File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
                IsDirty = false;
                return 0;
            }
            catch
            {
                return -1;
            }
        }

        // ======== 내부 파싱 ========
        private void ParseLines(string[] lines)
        {
            Scalars.Clear();
            Arrays.Clear();

            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim();
                if (string.IsNullOrEmpty(line)) continue;
                if (line.StartsWith("#")) continue;
                int eq = line.IndexOf('=');
                if (eq <= 0) continue;

                string keyPart = line.Substring(0, eq).Trim();
                string valPart = line.Substring(eq + 1).Trim();

                int lb = keyPart.IndexOf('[');
                int rb = keyPart.IndexOf(']');
                if (lb > 0 && rb > lb)
                {
                    string baseKey = keyPart.Substring(0, lb);
                    string idxStr = keyPart.Substring(lb + 1, rb - lb - 1);
                    int idx;
                    if (!int.TryParse(idxStr, NumberStyles.Any, CultureInfo.InvariantCulture, out idx))
                        continue;
                    if (!Arrays.TryGetValue(baseKey, out var bucket))
                    {
                        bucket = new Dictionary<int, string>();
                        Arrays[baseKey] = bucket;
                    }
                    bucket[idx] = valPart;
                }
                else
                {
                    Scalars[keyPart] = valPart;
                }
            }
        }

        private void ParseHeader()
        {
            ItemCount = TryParseIntScalar("ItemCount");
            TotalCount = TryParseIntScalar("TotalCount");
            OpticStart = TryParseIntScalar("OpticStart");
            CieRankShape = TryParseIntScalar("CieRankShape");
            Inspection = TryParseBoolScalar("Inspection");
            DCMap = TryParseBoolScalar("DCMap");
        }

        private int? TryParseIntScalar(string key)
        {
            string v;
            if (!Scalars.TryGetValue(key, out v)) return null;
            int i;
            if (int.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out i))
                return i;
            return null;
        }

        private bool? TryParseBoolScalar(string key)
        {
            string v;
            if (!Scalars.TryGetValue(key, out v)) return null;
            bool b;
            if (bool.TryParse(v, out b)) return b;
            int i;
            if (int.TryParse(v, out i)) return i != 0;
            return null;
        }

        private static bool IsSentinel(string v)
        {
            if (string.IsNullOrEmpty(v)) return false;
            return SentinelValues.Contains(v);
        }

        private int? TryParseIntArray(string baseKey, int index)
        {
            if (!Arrays.TryGetValue(baseKey, out var dict)) return null;
            if (!dict.TryGetValue(index, out var v)) return null;
            if (IsSentinel(v)) return null;
            int i;
            if (int.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out i))
                return i;
            return null;
        }

        private double? TryParseDoubleArray(string baseKey, int index)
        {
            if (!Arrays.TryGetValue(baseKey, out var dict)) return null;
            if (!dict.TryGetValue(index, out var v)) return null;
            if (IsSentinel(v)) return null;
            double d;
            if (double.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out d))
                return d;
            return null;
        }

        private string TryParseStringArray(string baseKey, int index)
        {
            if (!Arrays.TryGetValue(baseKey, out var dict)) return null;
            if (!dict.TryGetValue(index, out var v)) return null;
            if (IsSentinel(v)) return null;
            return v;
        }

        // ===== Items =====
        private void BuildItems()
        {
            Items.Clear();

            int maxIndex = MaxIndex("Item");
            maxIndex = Math.Max(maxIndex, MaxIndex("ItemName"));
            maxIndex = Math.Max(maxIndex, MaxIndex("ItemName2"));
            if (ItemCount.HasValue)
                maxIndex = Math.Max(maxIndex, ItemCount.Value - 1);

            for (int i = 0; i <= maxIndex; i++)
            {
                var it = new ItfItem { Index = i };

                it.RawItemCode = TryParseIntArray("Item", i);
                it.ItemName = TryParseStringArray("ItemName", i);
                it.ItemName2 = TryParseStringArray("ItemName2", i);
                it.ItemUnit = TryParseIntArray("ItemUnit", i);
                it.KeyChNo = TryParseIntArray("KeyChNo", i);
                it.OpenCheckFlag = TryParseIntArray("OpenCheckFlag", i);

                it.SourceRange = TryParseIntArray("SourceRange", i);
                it.SourceValue = TryParseDoubleArray("SourceValue", i);
                it.MeasureRange = TryParseIntArray("MeasureRange", i);
                it.MeasureUnit = TryParseIntArray("MeasureUnit", i);
                it.MeasureLow = TryParseDoubleArray("MeasureLow", i);
                it.MeasureHigh = TryParseDoubleArray("MeasureHigh", i);
                it.MeasureLimit = TryParseDoubleArray("MeasureLimit", i);

                it.SreDelay = TryParseDoubleArray("SreDelay", i);
                it.WaitTime = TryParseDoubleArray("WaitTime", i);
                it.OffTime = TryParseDoubleArray("OffTime", i);
                it.NplcTime = TryParseDoubleArray("NplcTime", i);

                it.OpticDCUsed = TryParseIntArray("OpticDCUsed", i);
                it.NGSkip = TryParseIntArray("NGSkip", i);
                it.Optical = TryParseIntArray("Optical", i);
                it.StrSourceUnit = TryParseStringArray("StrSourceUnit", i);

                it.WaveCount = TryParseIntArray("WaveCount", i);
                it.ExposeCount = TryParseIntArray("ExposeCount", i);
                it.IvRange = TryParseIntArray("IvRange", i);
                it.Polarity = TryParseIntArray("Polarity", i);

                it.FullRangeMin = TryParseDoubleArray("Full RangeMin", i);
                it.FullRangeMax = TryParseDoubleArray("Full RangeMax", i);
                it.SecondRangeMin = TryParseDoubleArray("2nd RangeMin", i);
                it.SecondRangeMax = TryParseDoubleArray("2nd RangeMax", i);

                it.OpticItemRawLevel = TryParseDoubleArray("OpticItemRawLevel", i);
                it.OpticItemRawHighLevel = TryParseDoubleArray("OpticItemRawHighLevel", i);
                it.OpticItemRawName = TryParseStringArray("OpticItemRawName", i);
                it.OpticItemRawUse = TryParseIntArray("OpticItemRawUse", i);

                it.OpticItemPdLowLevel = TryParseDoubleArray("OpticItemPdLowLevel", i);
                it.OpticItemPdHighLevel = TryParseDoubleArray("OpticItemPdHighLevel", i);
                it.OpticItemPdName = TryParseStringArray("OpticItemPdName", i);
                it.OpticItemPdUse = TryParseIntArray("OpticItemPdUse", i);

                it.OpticItemR3Level = TryParseDoubleArray("OpticItemR3Level", i);
                it.OpticItemR3HighLevel = TryParseDoubleArray("OpticItemR3HighLevel", i);
                it.OpticItemR3Name = TryParseStringArray("OpticItemR3Name", i);
                it.OpticItemR3Use = TryParseIntArray("OpticItemR3Use", i);

                it.WaveItemWP = BuildWaveMetric("WaveItemWP", i);
                it.WaveItemWH = BuildWaveMetric("WaveItemWH", i);
                it.WaveItemWX = BuildWaveMetric("WaveItemWX", i);
                it.WaveItemWY = BuildWaveMetric("WaveItemWY", i);
                it.WaveItemWD = BuildWaveMetric("WaveItemWD", i);
                it.WaveItemWPU = BuildWaveMetric("WaveItemWPU", i);
                it.WaveItemCCT = BuildWaveMetric("WaveItemCCT", i);
                it.WaveItemCIE = BuildWaveMetric("WaveItemCIE", i);
                it.WaveItemCRI = BuildWaveMetric("WaveItemCRI", i);
                it.WaveItemCRI9 = BuildWaveMetric("WaveItemCRI9", i);
                it.WaveItem2ndP = BuildWaveMetric("WaveItem2ndP", i);
                it.WaveItem3ndP = BuildWaveMetric("WaveItem3ndP", i);

                it.IntegrationTime = TryParseIntArray("IntegrationTime", i);
                it.EsdMode = TryParseIntArray("EsdMode", i);
                it.EsdCount = TryParseIntArray("EsdCount", i);
                it.VfStep = TryParseIntArray("VfStep", i);

                it.CalcItems = ExtractCalcItems(i);

                Items.Add(it);
            }
        }

        private ItfWaveMetric BuildWaveMetric(string prefix, int index)
        {
            var wm = new ItfWaveMetric
            {
                LowLevel = TryParseDoubleArray(prefix + "LowLevel", index),
                HighLevel = TryParseDoubleArray(prefix + "HighLevel", index),
                Name = TryParseStringArray(prefix + "Name", index),
                Use = TryParseIntArray(prefix + "Use", index)
            };
            return wm.IsEmpty() ? null : wm;
        }

        private List<int?> ExtractCalcItems(int itemIndex)
        {
            if (!Arrays.TryGetValue("CalcItem", out var dict))
                return new List<int?>();

            if (!ItemCount.HasValue || ItemCount.Value <= 0)
                return new List<int?>();

            int total = dict.Count;
            int groupSize = (total % ItemCount.Value == 0)
                ? total / ItemCount.Value
                : 10; // fallback

            var list = new List<int?>(groupSize);
            int start = itemIndex * groupSize;
            int end = start + groupSize - 1;

            bool anyValue = false;
            for (int idx = start; idx <= end; idx++)
            {
                string v;
                if (dict.TryGetValue(idx, out v))
                {
                    if (IsSentinel(v))
                    {
                        list.Add(null);
                    }
                    else
                    {
                        int parsed;
                        if (int.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out parsed))
                        {
                            list.Add(parsed);
                            anyValue = true;
                        }
                        else
                            list.Add(null);
                    }
                }
                else
                {
                    list.Add(null);
                }
            }

            if (!anyValue) return new List<int?>();
            return list;
        }

        private int MaxIndex(string baseKey)
        {
            if (!Arrays.TryGetValue(baseKey, out var dict)) return -1;
            int max = -1;
            foreach (var k in dict.Keys)
                if (k > max) max = k;
            return max;
        }

        // ===== AfterItemNums =====
        private void BuildAfterItemNums()
        {
            AfterItemNums.Clear();
            if (!Arrays.TryGetValue("AfterItemNum", out var dict)) return;

            int max = MaxIndex("AfterItemNum");
            for (int i = 0; i <= max; i++)
            {
                string v;
                if (dict.TryGetValue(i, out v))
                {
                    int n;
                    if (int.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out n))
                        AfterItemNums.Add(n);
                }
            }
        }

        // ===== Contacts =====
        private void BuildContacts()
        {
            Contacts.Clear();

            int max = MaxIndex("ContactUse");
            max = Math.Max(max, MaxIndex("FailOnSkip"));
            max = Math.Max(max, MaxIndex("ContactLow"));
            max = Math.Max(max, MaxIndex("ContactHigh"));
            max = Math.Max(max, MaxIndex("ContactOp1"));
            max = Math.Max(max, MaxIndex("ContactOp2"));
            max = Math.Max(max, MaxIndex("ContactOK"));

            for (int i = 0; i <= max; i++)
            {
                var c = new ItfContactSetting { Index = i };
                c.Use = ParseBoolArray("ContactUse", i);
                c.FailOnSkip = ParseBoolArray("FailOnSkip", i);
                c.Low = TryParseDoubleArray("ContactLow", i);
                c.High = TryParseDoubleArray("ContactHigh", i);
                c.Op1 = TryParseIntArray("ContactOp1", i);
                c.Op2 = TryParseIntArray("ContactOp2", i);
                c.OK = ParseBoolArray("ContactOK", i);

                if (!c.IsAllNull())
                    Contacts.Add(c);
            }
        }

        private bool? ParseBoolArray(string baseKey, int idx)
        {
            var i = TryParseIntArray(baseKey, idx);
            if (!i.HasValue) return null;
            return i.Value != 0;
        }

        // ===== Raw 재구성 =====
        public void RebuildRaw(bool preserveUnknownKeys)
        {
            // 재구성 대상 사전
            var scalars = preserveUnknownKeys
                ? new Dictionary<string, string>(Scalars, StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var arrays = preserveUnknownKeys
                ? Arrays.ToDictionary(k => k.Key,
                                      v => new Dictionary<int, string>(v.Value),
                                      StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, Dictionary<int, string>>(StringComparer.OrdinalIgnoreCase);

            // Known 키 삭제 후 재생성
            foreach (var k in KnownArrayKeys)
                arrays.Remove(k);

            // Header Scalars
            scalars["ItemCount"] = Items.Count.ToString(CultureInfo.InvariantCulture);
            if (TotalCount.HasValue) scalars["TotalCount"] = TotalCount.Value.ToString(CultureInfo.InvariantCulture);
            if (OpticStart.HasValue) scalars["OpticStart"] = OpticStart.Value.ToString(CultureInfo.InvariantCulture);
            if (Inspection.HasValue) scalars["Inspection"] = Inspection.Value ? "true" : "false";
            if (DCMap.HasValue) scalars["DCMap"] = DCMap.Value ? "true" : "false";
            if (CieRankShape.HasValue) scalars["CieRankShape"] = CieRankShape.Value.ToString(CultureInfo.InvariantCulture);

            // CalcItem 그룹 크기
            int calcGroupSize = 0;
            foreach (var it in Items)
                if (it.CalcItems != null && it.CalcItems.Count > calcGroupSize)
                    calcGroupSize = it.CalcItems.Count;

            for (int i = 0; i < Items.Count; i++)
            {
                var it = Items[i];

                PutInt(arrays, "Item", i, it.RawItemCode);
                PutStr(arrays, "ItemName", i, it.ItemName);
                PutStr(arrays, "ItemName2", i, it.ItemName2);
                PutInt(arrays, "ItemUnit", i, it.ItemUnit);
                PutInt(arrays, "KeyChNo", i, it.KeyChNo);
                PutInt(arrays, "OpenCheckFlag", i, it.OpenCheckFlag);

                PutInt(arrays, "SourceRange", i, it.SourceRange);
                PutDbl(arrays, "SourceValue", i, it.SourceValue);
                PutInt(arrays, "MeasureRange", i, it.MeasureRange);
                PutInt(arrays, "MeasureUnit", i, it.MeasureUnit);
                PutDbl(arrays, "MeasureLow", i, it.MeasureLow);
                PutDbl(arrays, "MeasureHigh", i, it.MeasureHigh);
                PutDbl(arrays, "MeasureLimit", i, it.MeasureLimit);

                PutDbl(arrays, "SreDelay", i, it.SreDelay);
                PutDbl(arrays, "WaitTime", i, it.WaitTime);
                PutDbl(arrays, "OffTime", i, it.OffTime);
                PutDbl(arrays, "NplcTime", i, it.NplcTime);

                PutInt(arrays, "OpticDCUsed", i, it.OpticDCUsed);
                PutInt(arrays, "NGSkip", i, it.NGSkip);
                PutInt(arrays, "Optical", i, it.Optical);
                PutStr(arrays, "StrSourceUnit", i, it.StrSourceUnit);

                PutInt(arrays, "WaveCount", i, it.WaveCount);
                PutInt(arrays, "ExposeCount", i, it.ExposeCount);
                PutInt(arrays, "IvRange", i, it.IvRange);
                PutInt(arrays, "Polarity", i, it.Polarity);

                PutDbl(arrays, "Full RangeMin", i, it.FullRangeMin);
                PutDbl(arrays, "Full RangeMax", i, it.FullRangeMax);
                PutDbl(arrays, "2nd RangeMin", i, it.SecondRangeMin);
                PutDbl(arrays, "2nd RangeMax", i, it.SecondRangeMax);

                PutDbl(arrays, "OpticItemRawLevel", i, it.OpticItemRawLevel);
                PutDbl(arrays, "OpticItemRawHighLevel", i, it.OpticItemRawHighLevel);
                PutStr(arrays, "OpticItemRawName", i, it.OpticItemRawName);
                PutInt(arrays, "OpticItemRawUse", i, it.OpticItemRawUse);

                PutDbl(arrays, "OpticItemPdLowLevel", i, it.OpticItemPdLowLevel);
                PutDbl(arrays, "OpticItemPdHighLevel", i, it.OpticItemPdHighLevel);
                PutStr(arrays, "OpticItemPdName", i, it.OpticItemPdName);
                PutInt(arrays, "OpticItemPdUse", i, it.OpticItemPdUse);

                PutDbl(arrays, "OpticItemR3Level", i, it.OpticItemR3Level);
                PutDbl(arrays, "OpticItemR3HighLevel", i, it.OpticItemR3HighLevel);
                PutStr(arrays, "OpticItemR3Name", i, it.OpticItemR3Name);
                PutInt(arrays, "OpticItemR3Use", i, it.OpticItemR3Use);

                PutWave(arrays, "WaveItemWP", i, it.WaveItemWP);
                PutWave(arrays, "WaveItemWH", i, it.WaveItemWH);
                PutWave(arrays, "WaveItemWX", i, it.WaveItemWX);
                PutWave(arrays, "WaveItemWY", i, it.WaveItemWY);
                PutWave(arrays, "WaveItemWD", i, it.WaveItemWD);
                PutWave(arrays, "WaveItemWPU", i, it.WaveItemWPU);
                PutWave(arrays, "WaveItemCCT", i, it.WaveItemCCT);
                PutWave(arrays, "WaveItemCIE", i, it.WaveItemCIE);
                PutWave(arrays, "WaveItemCRI", i, it.WaveItemCRI);
                PutWave(arrays, "WaveItemCRI9", i, it.WaveItemCRI9);
                PutWave(arrays, "WaveItem2ndP", i, it.WaveItem2ndP);
                PutWave(arrays, "WaveItem3ndP", i, it.WaveItem3ndP);

                PutInt(arrays, "IntegrationTime", i, it.IntegrationTime);
                PutInt(arrays, "EsdMode", i, it.EsdMode);
                PutInt(arrays, "EsdCount", i, it.EsdCount);
                PutInt(arrays, "VfStep", i, it.VfStep);

                if (calcGroupSize > 0)
                {
                    for (int j = 0; j < calcGroupSize; j++)
                    {
                        int flatIndex = i * calcGroupSize + j;
                        int? val = (it.CalcItems != null && j < it.CalcItems.Count) ? it.CalcItems[j] : (int?)null;
                        PutInt(arrays, "CalcItem", flatIndex, val, allowMissing: false);
                    }
                }
            }

            // AfterItemNum
            for (int i = 0; i < AfterItemNums.Count; i++)
                PutInt(arrays, "AfterItemNum", i, AfterItemNums[i]);

            // Contacts
            if (Contacts.Count > 0)
            {
                int maxIdx = Contacts.Max(c => c.Index);
                var map = Contacts.ToDictionary(c => c.Index, c => c);
                for (int i = 0; i <= maxIdx; i++)
                {
                    ItfContactSetting c;
                    if (map.TryGetValue(i, out c))
                    {
                        PutBoolAs01(arrays, "ContactUse", i, c.Use);
                        PutBoolAs01(arrays, "FailOnSkip", i, c.FailOnSkip);
                        PutDbl(arrays, "ContactLow", i, c.Low);
                        PutDbl(arrays, "ContactHigh", i, c.High);
                        PutInt(arrays, "ContactOp1", i, c.Op1);
                        PutInt(arrays, "ContactOp2", i, c.Op2);
                        PutBoolAs01(arrays, "ContactOK", i, c.OK);
                    }
                }
            }

            Scalars = scalars;
            Arrays = arrays;
        }

        // ===== 저장 헬퍼 =====
        private static string FormatDouble(double v)
        {
            return v.ToString("0.000000e+000", CultureInfo.InvariantCulture);
        }

        private static void SetArray(Dictionary<string, Dictionary<int, string>> arrays, string key, int index, string value)
        {
            Dictionary<int, string> bucket;
            if (!arrays.TryGetValue(key, out bucket))
            {
                bucket = new Dictionary<int, string>();
                arrays[key] = bucket;
            }
            bucket[index] = value;
        }

        private static void PutStr(Dictionary<string, Dictionary<int, string>> arrays, string key, int index, string value)
        {
            if (value == null) return;
            SetArray(arrays, key, index, value);
        }

        private static void PutInt(Dictionary<string, Dictionary<int, string>> arrays, string key, int index, int? value, bool allowMissing = true)
        {
            if (value.HasValue)
            {
                SetArray(arrays, key, index, value.Value.ToString(CultureInfo.InvariantCulture));
            }
            else if (!allowMissing)
            {
                SetArray(arrays, key, index, SentinelInt);
            }
        }

        private static void PutDbl(Dictionary<string, Dictionary<int, string>> arrays, string key, int index, double? value, bool allowMissing = true)
        {
            if (value.HasValue)
            {
                SetArray(arrays, key, index, FormatDouble(value.Value));
            }
            else if (!allowMissing)
            {
                SetArray(arrays, key, index, SentinelDoubleFormatted);
            }
        }

        private static void PutBoolAs01(Dictionary<string, Dictionary<int, string>> arrays, string key, int index, bool? value, bool allowMissing = true)
        {
            if (value.HasValue)
            {
                SetArray(arrays, key, index, value.Value ? "1" : "0");
            }
            else if (!allowMissing)
            {
                SetArray(arrays, key, index, "0");
            }
        }

        private static void PutWave(Dictionary<string, Dictionary<int, string>> arrays, string prefix, int index, ItfWaveMetric m)
        {
            if (m == null) return;
            PutDbl(arrays, prefix + "LowLevel", index, m.LowLevel);
            PutDbl(arrays, prefix + "HighLevel", index, m.HighLevel);
            PutStr(arrays, prefix + "Name", index, m.Name);
            PutInt(arrays, prefix + "Use", index, m.Use);
        }

        private void MarkDirty() => IsDirty = true;
    }

    // ===== Item 구조 =====
    public sealed class ItfItem
    {
        public int Index { get; set; }

        public int? RawItemCode { get; set; }
        public string ItemName { get; set; }
        public string ItemName2 { get; set; }
        public int? ItemUnit { get; set; }
        public int? KeyChNo { get; set; }
        public int? OpenCheckFlag { get; set; }

        public int? SourceRange { get; set; }
        public double? SourceValue { get; set; }
        public int? MeasureRange { get; set; }
        public int? MeasureUnit { get; set; }
        public double? MeasureLow { get; set; }
        public double? MeasureHigh { get; set; }
        public double? MeasureLimit { get; set; }

        public double? SreDelay { get; set; }
        public double? WaitTime { get; set; }
        public double? OffTime { get; set; }
        public double? NplcTime { get; set; }

        public int? OpticDCUsed { get; set; }
        public int? NGSkip { get; set; }
        public int? Optical { get; set; }
        public string StrSourceUnit { get; set; }

        public int? WaveCount { get; set; }
        public int? ExposeCount { get; set; }
        public int? IvRange { get; set; }
        public int? Polarity { get; set; }

        public double? FullRangeMin { get; set; }
        public double? FullRangeMax { get; set; }
        public double? SecondRangeMin { get; set; }
        public double? SecondRangeMax { get; set; }

        public double? OpticItemRawLevel { get; set; }
        public double? OpticItemRawHighLevel { get; set; }
        public string OpticItemRawName { get; set; }
        public int? OpticItemRawUse { get; set; }

        public double? OpticItemPdLowLevel { get; set; }
        public double? OpticItemPdHighLevel { get; set; }
        public string OpticItemPdName { get; set; }
        public int? OpticItemPdUse { get; set; }

        public double? OpticItemR3Level { get; set; }
        public double? OpticItemR3HighLevel { get; set; }
        public string OpticItemR3Name { get; set; }
        public int? OpticItemR3Use { get; set; }

        public ItfWaveMetric WaveItemWP { get; set; }
        public ItfWaveMetric WaveItemWH { get; set; }
        public ItfWaveMetric WaveItemWX { get; set; }
        public ItfWaveMetric WaveItemWY { get; set; }
        public ItfWaveMetric WaveItemWD { get; set; }
        public ItfWaveMetric WaveItemWPU { get; set; }
        public ItfWaveMetric WaveItemCCT { get; set; }
        public ItfWaveMetric WaveItemCIE { get; set; }
        public ItfWaveMetric WaveItemCRI { get; set; }
        public ItfWaveMetric WaveItemCRI9 { get; set; }
        public ItfWaveMetric WaveItem2ndP { get; set; }
        public ItfWaveMetric WaveItem3ndP { get; set; }

        public int? IntegrationTime { get; set; }
        public int? EsdMode { get; set; }
        public int? EsdCount { get; set; }
        public int? VfStep { get; set; }

        public List<int?> CalcItems { get; set; } = new List<int?>();
    }

    public sealed class ItfWaveMetric
    {
        public double? LowLevel { get; set; }
        public double? HighLevel { get; set; }
        public string Name { get; set; }
        public int? Use { get; set; }

        internal bool IsEmpty()
        {
            return !LowLevel.HasValue && !HighLevel.HasValue && string.IsNullOrEmpty(Name) && !Use.HasValue;
        }
    }

    public sealed class ItfContactSetting
    {
        public int Index { get; set; }
        public bool? Use { get; set; }
        public bool? FailOnSkip { get; set; }
        public double? Low { get; set; }
        public double? High { get; set; }
        public int? Op1 { get; set; }
        public int? Op2 { get; set; }
        public bool? OK { get; set; }

        internal bool IsAllNull()
        {
            return !Use.HasValue && !FailOnSkip.HasValue
                   && !Low.HasValue && !High.HasValue
                   && !Op1.HasValue && !Op2.HasValue && !OK.HasValue;
        }
    }
}