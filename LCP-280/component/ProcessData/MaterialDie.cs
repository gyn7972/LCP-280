
using QMC.Common;
using QMC.Common.PKGTester;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace QMC.LCP_280.Process.Component
{
    public enum DieProcessState
    {
        None,       // 초기
        Mapped,     // Vision Mapping 완료
        Picked,     // Pick 완료
        Inspecting, // 검사 중
        Inspected,  // 검사 완료
        Rejected,   // 불량
        Placed,     // 언로더 배치 완료
        Skip,
        //Error,       // 오류
        //Error_load,
        //Error_MAlign,
        //Error_Probe,
        //Error_UnloadAlign,
        //Error_Unloader,
    }

    [Serializable]
    public class MaterialDie : Material
    {
        // ===== 식별 =====
        [DefaultValue(-1)] public int Index { get; set; } = -1;
        [DefaultValue(-1)] public int SocketIndex { get; set; } = -1;
        [DefaultValue(0)] public int MapX { get; set; } = 0;
        [DefaultValue(0)] public int MapY { get; set; } = 0;

        [DefaultValue(0)] public double BinX { get; set; } = 0;
        [DefaultValue(0)] public double BinY { get; set; } = 0;

        //Online 구분시 Rank Number
        [DefaultValue(0)] public int PreRank { get; set; } = 0;

        // ===== Vision 위치 =====
        [DefaultValue(0.0)] public double CenterX { get; set; } = 0.0;
        [DefaultValue(0.0)] public double CenterY { get; set; } = 0.0;
        [DefaultValue(0.0)] public double Angle { get; set; } = 0.0;

        [DefaultValue(0.0)] public double UnloadAlignOffsetX { get; set; } = 0.0;
        [DefaultValue(0.0)] public double UnloadAlignOffsetY { get; set; } = 0.0;
        [DefaultValue(0.0)] public double UnloadAlignOffsetT { get; set; } = 0.0;

        // ===== 상태 =====
        [DefaultValue(DieProcessState.None)] public DieProcessState State { get; set; } = DieProcessState.None;
        [DefaultValue(true)] public bool IsPass { get; set; } = true;
        [DefaultValue("")] public string RejectReason { get; set; } = "";
        [DefaultValue("")] public string SkipReason { get; set; } = "";

        [DefaultValue(0)] public int Rank { get; set; } = 0;
        [DefaultValue("-")] public string RankName { get; set; } = "-";

        // ===== 검사 데이터 =====
        public Dictionary<string, double> MeasureValues { get; set; } = new Dictionary<string, double>();
        public PKGTesterResult TesterResult = new PKGTesterResult();

        // ===== 트래킹 =====
        [DefaultValue("")] public string SourceBinFileName { get; set; } = "";

        [DefaultValue("")] public string SourceWaferId { get; set; } = "";
        [DefaultValue("")] public string TargetWaferId { get; set; } = "";
        

        [DefaultValue(-1)] public int TargetSlot { get; set; } = -1;
        [DefaultValue(-1)] public int TargetChipIndex { get; set; } = -1;

        // ===== 유틸 =====
        public void AddMeasure(string key, double value) => MeasureValues[key] = value;

        public double? GetMeasure(string key)
        {
            double value;
            if (MeasureValues.TryGetValue(key, out value))
                return value;
            return null;
        }

        public void SetReject(string reason)
        {
            IsPass = false;
            RejectReason = reason;
            State = DieProcessState.Rejected;
        }

        public void SetSkip(string reason)
        {
            IsPass = true;
            SkipReason = reason;
            State = DieProcessState.Skip;
        }
    }
}
