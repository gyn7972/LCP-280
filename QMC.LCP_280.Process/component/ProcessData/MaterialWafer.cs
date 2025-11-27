using QMC.Common; // For PointD
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Media.Media3D;

namespace QMC.LCP_280.Process.Component
{
    [Serializable]
    public sealed class MaterialWafer : QMC.Common.Material
    {
        // ===== Identification =====
        [DefaultValue("")] public string WaferId { get; set; } = "";
        [DefaultValue("")] public string WaferDate { get; set; } = DateTime.Now.ToString("yyyyMMdd");

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
            WaferId = "";
            WaferDate = DateTime.Now.ToString("yyyyMMdd");
            CarrierId = "";
            SlotIndex = -1;
            Summary = new WaferSummary();
            RecipeKeys.Clear();
            Dies.Clear();
        }

        // ===== Chip 관리 함수 =====
        public MaterialDie AddChip(int index, int mapX, int mapY)
        {
            string existingName = WaferId + "_" + index;
            var chip = new MaterialDie
            {
                Index = index,
                Name = existingName,
                MapX = mapX,
                MapY = mapY,
                Presence = MaterialPresence.Exist,
                State = DieProcessState.Mapped,
                SourceWaferId = WaferId
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
        /// 

        public void MakeWaferInfo(List<PointD> centers, double chipPitchXmm, double chipPitchYmm)
        {
            lock(this)
            {
                if (centers == null) return;
                var rawList = centers; // already a list
                Dies.Clear();
                if (rawList.Count == 0) 
                    return;
                if (chipPitchXmm <= 0 || chipPitchYmm <= 0) 
                    throw new ArgumentOutOfRangeException("Chip pitch must be > 0");

                // 1) 중복/중첩 Chip 병합 (마지막 좌표가 최종)
                double tolX = chipPitchXmm * 0.30; // 허용 오차 (조정 가능)
                double tolY = chipPitchYmm * 0.30;
                var merged = new List<PointD>();
                int nIndex = 0;
                string strFileName = "MakeWaferInfo_rawList" + DateTime.Now.Ticks.ToString();
                Log.Write(strFileName, "rawList", " WaferId,rawList.Count,rawList.Index,rawList.posX ,rawList.posY");
                foreach (var p in rawList)
                {
                    int found = -1;
                    nIndex++;
                    Log.Write(strFileName, "rawList", $"{WaferId},{rawList.Count},{nIndex},{p.X},{p.Y}");

                    for (int i = 0; i < merged.Count; i++)
                    {
                        if (Math.Abs(merged[i].X - p.X) <= tolX && Math.Abs(merged[i].Y - p.Y) <= tolY)
                        {
                            found = i; 
                            break;
                        }
                    }
                    if (found >= 0)
                    {
                        merged.RemoveAt(found);
                        merged.Add(p); // 마지막 관측을 순서에 반영
                    }
                    else
                    {
                        merged.Add(p);
                    }
                    //if (found >= 0)
                    //{
                    //    // 동일 Chip -> 마지막 좌표로 갱신 (요청: 리스트의 마지막 우선)
                    //    merged[found] = p;
                    //}
                    //else
                    //{
                    //    merged.Add(p);
                    //}
                }
                if (merged.Count == 0) 
                    return;

                string BinFileName = "";
                var recip = Equipment.Instance.EquipmentRecipe.CurrentRecipe;
                if (recip != null)
                {
                    var raw = recip.BinningSpecSheetFile;
                    if (!string.IsNullOrWhiteSpace(raw))
                    {
                        // 경로 끝이 \ 또는 / 로 끝나면 제거 후 파일명만 추출
                        raw = raw.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                        BinFileName = Path.GetFileName(raw);
                    }
                }

                // 5) Chip 객체 생성 및 Grid 계산
                var temp = new List<MaterialDie>();
                
                double estPitchX = chipPitchXmm;
                double estPitchY = chipPitchYmm;


                var refchip = new MaterialDie();
                refchip.CenterX = merged[0].X;
                refchip.CenterY = merged[0].Y;
                refchip.MapX = 0;
                refchip.MapY = 0;



                foreach (var p in merged)
                {
                    int mapX = (int)Math.Round((p.X - refchip.CenterX) / estPitchX) + refchip.MapX;
                    int mapY = (int)Math.Round((p.Y - refchip.CenterY) / estPitchY) + refchip.MapY;

                    var chip = new MaterialDie
                    {
                        ArrivedTime = DateTime.Now,
                        MapX = mapX,
                        MapY = mapY,
                        CenterX = p.X,
                        CenterY = p.Y,
                        Angle = 0.0,
                        Presence = MaterialPresence.Exist,
                        State = DieProcessState.Mapped,
                        SourceWaferId = WaferId,
                        SourceBinFileName = BinFileName,
                    };
                    temp.Add(chip);
                    refchip = chip;
                }

                // 6) Index 부여 (행 우선)
                int idx = 0;
                foreach (var chip in temp
                    .OrderBy(c => c.MapY)
                    .ThenBy(c => c.MapX)
                    .ThenBy(c => c.CenterY) // 타이브레이크 보강
                    .ThenBy(c => c.CenterX))
                {
                    chip.Index = idx++;
                }
                Dies.AddRange(temp.OrderBy(c => c.Index));
                // 6) Index 부여 (행 우선)
                //int idx = 0;
                //foreach (var chip in temp.OrderBy(c => c.MapY).ThenBy(c => c.MapX))
                //{
                //    chip.Index = idx++;
                //}
                //Dies.AddRange(temp.OrderBy(c => c.Index));
                strFileName = "MakeWaferInfo_Dies" + DateTime.Now.Ticks.ToString();
                Log.Write(strFileName, "rawList", $" WaferId,Dies.Count,Dies.Index,Dies.MapX ,Dies.MapY ,Dies.CenterX,Dies.CenterY ");
                for (int j=0; j < Dies.Count; j++)
                {
                    Log.Write(strFileName, "rawList", $"{WaferId}, {Dies.Count},{Dies[j].Index},{Dies[j].MapX},{Dies[j].MapY},{ Dies[j].CenterX},{ Dies[j].CenterY}");
                }

                // 측정값 초기화
                foreach (var chip in Dies)
                {
                    foreach (var key in RecipeKeys)
                    {
                        if (!chip.MeasureValues.ContainsKey(key))
                        {
                            chip.AddMeasure(key, double.NaN);
                        }
                    }
                }



            }
            
        }


        public void UpdateChipInfo(List<PointD> centers, double chipPitchXmm, double chipPitchYmm)
        {
            foreach (var p in centers)
            {
                double dMinTolx = chipPitchXmm * 0.3; // degree
                double dMinToly = chipPitchYmm * 0.3;
                var chip = Dies.FirstOrDefault(c => Math.Abs(c.CenterX - p.X) <= dMinTolx && Math.Abs(c.CenterY - p.Y) <= dMinToly);
                if (chip != null)
                {
                    chip.CenterX = p.X;
                    chip.CenterY = p.Y;
                }
            }   
        }

    }
}
