using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media.Media3D;
using QMC.Common; // For PointD

namespace QMC.LCP_280.Process.Component
{
    [Serializable]
    public sealed class MaterialWafer : QMC.Common.Material
    {
        // ===== Identification =====
        [DefaultValue("")] public string LotId { get; set; } = "";
        [DefaultValue("")] public string LotDate { get; set; } = DateTime.Now.ToString("yyyyMMdd");

        [DefaultValue("")] public string CarrierId { get; set; } = "";
        [DefaultValue(-1)] public int SlotIndex { get; set; } = -1;

        // ===== Recipe Keys (모든 Chip 공통 검사 Key) =====
        public List<string> RecipeKeys { get; set; } = new List<string>();

        // ===== Wafer Info =====
        public WaferSummary Summary { get; set; } = new WaferSummary();

        // ===== Chip Data =====
        public List<MaterialDie> Dies { get; set; } = new List<MaterialDie>();

        // ===== Reset =====
        public void Reset()
        {
            LotId = "";
            LotDate = DateTime.Now.ToString("yyyyMMdd");
            CarrierId = "";
            SlotIndex = -1;
            Summary = new WaferSummary();
            RecipeKeys.Clear();
            Dies.Clear();
        }

        // ===== Chip 관리 함수 =====
        public MaterialDie AddChip(int index, int mapX, int mapY)
        {
            var chip = new MaterialDie
            {
                Index = index,
                MapX = mapX,
                MapY = mapY,
                Exists = true,
                State = DieProcessState.Mapped,
                SourceWaferId = LotId
            };

            foreach (var key in RecipeKeys)
                chip.AddMeasure(key, double.NaN);

            Dies.Add(chip);
            return chip;
        }

        public MaterialDie GetChipByIndex(int index) =>
            Dies.FirstOrDefault(c => c.Index == index);

        public MaterialDie GetChipByMap(int x, int y) =>
            Dies.FirstOrDefault(c => c.MapX == x && c.MapY == y);

        // ===== Chip 정보 업데이트 (동적 Pitch 추정 / 중복 좌표 합치기) =====
        /// <summary>
        /// 설정 Pitch 와 달라도 실제 측정 좌표로부터 Pitch 를 추정하여 MapX/MapY 및 Index 를 계산한다.
        /// centers 는 Chip 중심 좌표(mm) 목록이며 동일 Chip 이 여러 번 스캔되어 좌표가 근접하면 마지막 값을 사용한다.
        /// </summary>
        /// <param name="centers">Chip 중심 좌표 목록 (mm)</param>
        /// <param name="chipPitchXmm">설정 Pitch X (mm)</param>
        /// <param name="chipPitchYmm">설정 Pitch Y (mm)</param>
        public void UpdateChipInfo(List<PointD> centers, double chipPitchXmm, double chipPitchYmm)
        {
            if (centers == null) return;
            var rawList = centers; // already a list
            Dies.Clear();
            if (rawList.Count == 0) return;
            if (chipPitchXmm <= 0 || chipPitchYmm <= 0) throw new ArgumentOutOfRangeException("Chip pitch must be > 0");

            // 1) 중복/중첩 Chip 병합 (마지막 좌표가 최종)
            double tolX = chipPitchXmm * 0.30; // 허용 오차 (조정 가능)
            double tolY = chipPitchYmm * 0.30;
            var merged = new List<PointD>();
            foreach (var p in rawList)
            {
                int found = -1;
                for (int i = 0; i < merged.Count; i++)
                {
                    if (Math.Abs(merged[i].X - p.X) <= tolX && Math.Abs(merged[i].Y - p.Y) <= tolY)
                    {
                        found = i; break;
                    }
                }
                if (found >= 0)
                {
                    // 동일 Chip -> 마지막 좌표로 갱신 (요청: 리스트의 마지막 우선)
                    merged[found] = p;
                }
                else
                {
                    merged.Add(p);
                }
            }
            if (merged.Count == 0) return;

            // 2) 좌상단 기준 Chip (X 최소, 그 다음 Y 최소)
            var topLeft = merged.OrderBy(v => v.X).ThenBy(v => v.Y).First();
            double baseX = topLeft.X;
            double baseY = topLeft.Y;

            // 3) PitchY 추정 (기준 Chip 아래쪽 후보)
            double estPitchY = chipPitchYmm;
            var downCandidates = merged
                .Where(v => v.Y > baseY && Math.Abs(v.X - baseX) <= tolX)
                .Select(v => v.Y - baseY)
                .Where(dy => dy > tolY * 0.2)
                .OrderBy(dy => dy)
                .ToList();
            if (downCandidates.Count > 0)
            {
                estPitchY = downCandidates.First();
                if (estPitchY < chipPitchYmm * 0.5 || estPitchY > chipPitchYmm * 1.5)
                    estPitchY = chipPitchYmm;
            }

            // 4) PitchX 추정 (기준 Chip 오른쪽 후보)
            double estPitchX = chipPitchXmm;
            var rightCandidates = merged
                .Where(v => v.X > baseX && Math.Abs(v.Y - baseY) <= tolY)
                .Select(v => v.X - baseX)
                .Where(dx => dx > tolX * 0.2)
                .OrderBy(dx => dx)
                .ToList();
            if (rightCandidates.Count > 0)
            {
                estPitchX = rightCandidates.First();
                if (estPitchX < chipPitchXmm * 0.5 || estPitchX > chipPitchXmm * 1.5)
                    estPitchX = chipPitchXmm;
            }

            if (estPitchX <= 0) estPitchX = chipPitchXmm;
            if (estPitchY <= 0) estPitchY = chipPitchYmm;

            // 5) Chip 객체 생성 및 Grid 계산
            var temp = new List<MaterialDie>();
            foreach (var p in merged)
            {
                int mapX = (int)Math.Round((p.X - baseX) / estPitchX, MidpointRounding.AwayFromZero);
                int mapY = (int)Math.Round((p.Y - baseY) / estPitchY, MidpointRounding.AwayFromZero);
                if (mapX < 0) mapX = 0;
                if (mapY < 0) mapY = 0;
                var chip = new MaterialDie
                {
                    MapX = mapX,
                    MapY = mapY,
                    CenterX = p.X,
                    CenterY = p.Y,
                    Angle = 0.0,
                    Exists = true,
                    State = DieProcessState.Mapped,
                    SourceWaferId = LotId
                };
                temp.Add(chip);
            }

            // 6) Index 부여 (행 우선)
            int idx = 0;
            foreach (var chip in temp.OrderBy(c => c.MapY).ThenBy(c => c.MapX))
                chip.Index = idx++;

            Dies.AddRange(temp.OrderBy(c => c.Index));

            // 측정값 초기화
            foreach (var chip in Dies)
            {
                foreach (var key in RecipeKeys)
                    if (!chip.MeasureValues.ContainsKey(key))
                        chip.AddMeasure(key, double.NaN);
            }
        }
    }
}
