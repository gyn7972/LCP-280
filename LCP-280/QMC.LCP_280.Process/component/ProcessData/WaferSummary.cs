using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.LCP_280.Process.Component
{
    [Serializable]
    public sealed class WaferSummary
    {
        // ===== 장비 상태 =====
        [DefaultValue(false)] public bool InProcess { get; set; } = false;
        [DefaultValue(false)] public bool Resorting { get; set; } = false;
        [DefaultValue(false)] public bool Abort { get; set; } = false;
        [DefaultValue(false)] public bool UserLoad { get; set; } = false;
        [DefaultValue(false)] public bool ManualUnload { get; set; } = false;

        // ===== Lot 정보 =====
        [DefaultValue("")] public string LotNo { get; set; } = "";
        [DefaultValue("")] public string OutputRingNo { get; set; } = "";
        [DefaultValue("")] public string InputRingNo { get; set; } = "";
        [DefaultValue("")] public string MachineRecipe { get; set; } = "";
        [DefaultValue("")] public string Operator { get; set; } = "";

        // ===== 시간/카운트/사이클 =====
        public WaferTime Time { get; set; } = new WaferTime();
        public WaferCount Count { get; set; } = new WaferCount();
        public WaferTactTime TactTime { get; set; } = new WaferTactTime();
    }

    // ================== 시간 ==================
    [Serializable]
    public sealed class WaferTime
    {
        public WaferTimeStamp Stamp { get; set; } = new WaferTimeStamp();

        [DefaultValue(0.0)] public double TotalTime { get; set; } = 0.0;
        [DefaultValue(0.0)] public double RunTime { get; set; } = 0.0;
        [DefaultValue(0.0)] public double DownTime { get; set; } = 0.0;
        [DefaultValue(0.0)] public double LoadingTime { get; set; } = 0.0;
        [DefaultValue(0.0)] public double ScanTime { get; set; } = 0.0;
        [DefaultValue(0.0)] public double SortTime { get; set; } = 0.0;
        [DefaultValue(0.0)] public double UnloadingTime { get; set; } = 0.0;
        [DefaultValue(0.0)] public double BinChangeTime { get; set; } = 0.0;
    }

    [Serializable]
    public sealed class WaferTimeStamp
    {
        [DefaultValue(0.0)] public double ProcessStart { get; set; } = 0;
        [DefaultValue(0.0)] public double ProcessEnd { get; set; } = 0;
        [DefaultValue(0.0)] public double LoadingStart { get; set; } = 0;
        [DefaultValue(0.0)] public double LoadingEnd { get; set; } = 0;
        [DefaultValue(0.0)] public double ScanStart { get; set; } = 0;
        [DefaultValue(0.0)] public double ScanEnd { get; set; } = 0;
        [DefaultValue(0.0)] public double SortStart { get; set; } = 0;
        [DefaultValue(0.0)] public double SortEnd { get; set; } = 0;
        [DefaultValue(0.0)] public double UnloadingStart { get; set; } = 0;
        [DefaultValue(0.0)] public double UnloadingEnd { get; set; } = 0;
    }

    // ================== 카운트 ==================
    [Serializable]
    public sealed class WaferCount
    {
        [DefaultValue(0)] public int BinChangeCount { get; set; } = 0;
        [DefaultValue(0)] public int AlarmCount { get; set; } = 0;

        public WaferChipCount ChipCount { get; set; } = new WaferChipCount();
    }

    [Serializable]
    public sealed class WaferChipCount
    {
        [DefaultValue(0)] public int TotalCount { get; set; } = 0;
        [DefaultValue(0)] public int MapCount { get; set; } = 0;
        [DefaultValue(0)] public int ScanCount { get; set; } = 0;
        [DefaultValue(0)] public int MatchCount { get; set; } = 0;
        [DefaultValue(0)] public int RankChipCount { get; set; } = 0;
        [DefaultValue(0)] public int PlaceCount { get; set; } = 0;
        [DefaultValue(0)] public int TotalMissCount { get; set; } = 0;
        [DefaultValue(0)] public int PickMissCount { get; set; } = 0;
        [DefaultValue(0)] public int RevisionMissCount { get; set; } = 0;
        [DefaultValue(0)] public int PlaceMissCount { get; set; } = 0;
        [DefaultValue(0)] public int InspectionNgCount { get; set; } = 0;
        [DefaultValue(0)] public int OverAngleCount { get; set; } = 0;

        // Rank별 Place 수량 (예: MAX_BIN = 50)
        public int[] PlaceRankCount { get; set; } = new int[50];
    }

    // ================== 사이클 타임 ==================
    [Serializable]
    public sealed class WaferTactTime
    {
        [DefaultValue(0L)] public long CycleTimeSum { get; set; } = 0;
        [DefaultValue(0)] public int CycleTimeAddCount { get; set; } = 0;

        public double Average => CycleTimeAddCount > 0 ? (double)CycleTimeSum / CycleTimeAddCount : 0.0;
    }
}
