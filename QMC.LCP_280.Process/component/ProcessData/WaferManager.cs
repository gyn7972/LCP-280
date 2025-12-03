using QMC.Common;
using QMC.LCP_280.Process.Component;
using System;
using System.Collections.Generic;
using System.IO;

//사용예시
//string cfgPath = Path.Combine(
//    AppDomain.CurrentDomain.BaseDirectory,
//    "Configs", "WaferData", "WaferDataConfig.json");

//// 1) 장비 기동 시 → 상태 복구
//WaferManager.Instance.Initialize(cfgPath);

//// 2) 기존 진행중 Wafer 확인
//var oldWafer = WaferManager.Instance.GetWafer("CARRIER01", 5);
//if (oldWafer != null && oldWafer.Summary.InProcess)
//    Console.WriteLine($"복구됨: Lot={oldWafer.LotId}, 진행중");

//// 3) 새 Lot 투입 시 → 기존 저장 후 초기화
//var newWafer = WaferManager.Instance.LoadNewLot("CARRIER01", 5, "LOT20250910");
//Console.WriteLine($"새 Lot 등록: Lot={newWafer.LotId}, Carrier={newWafer.CarrierId}, Slot={newWafer.SlotIndex}");

//// 4) 종료 전 저장
//WaferManager.Instance.Save(cfgPath);
/////////////////////////////////
///

// 구조 설명.
//WaferManager(싱글톤)
// └─ WaferDataConfig(BaseConfig)
//     └─ List < CarrierData >
//          └─ List < WaferData >
//               ├─ RecipeKeys(List<string>)
//               ├─ List < ChipData >
//               └─ WaferSummary


namespace QMC.LCP_280.Process.Component
{
    public sealed class WaferManager
    {
        private static readonly Lazy<WaferManager> _instance =
            new Lazy<WaferManager>(() => new WaferManager());

        public static WaferManager Instance => _instance.Value;

        private WaferDataConfig _config;
        private readonly object _gate = new object();

        private WaferManager() { }

        // ===== 초기화 / 복구 =====
        public void Initialize(string configPath)
        {
            _config = WaferDataConfig.LoadOrCreate(configPath);
        }

        // ===== 조회 =====
        public MaterialCassette GetCarrier(string carrierId)
        {
            lock (_gate)
            {
                return _config.Carriers.Find(c =>
                    c.CarrierId.Equals(carrierId, StringComparison.OrdinalIgnoreCase));
            }
        }

        public MaterialWafer GetWafer(string carrierId, int slot)
        {
            var carrier = GetCarrier(carrierId);
            return carrier?.GetWafer(slot);
        }

        // ===== Carrier 관리 =====
        public MaterialCassette AddCarrier(string carrierId, int slotCount = 25)
        {
            lock (_gate)
            {
                var existing = GetCarrier(carrierId);
                if (existing != null) return existing;

                var carrier = new MaterialCassette
                {
                    CarrierId = carrierId,
                    SlotCount = slotCount
                };
                _config.Carriers.Add(carrier);
                return carrier;
            }
        }

        public bool RemoveCarrier(string carrierId)
        {
            lock (_gate)
            {
                return _config.Carriers.RemoveAll(c =>
                    c.CarrierId.Equals(carrierId, StringComparison.OrdinalIgnoreCase)) > 0;
            }
        }

        // ===== Wafer 관리 =====
        public void AddWafer(string carrierId, int slot, MaterialWafer wafer)
        {
            var carrier = AddCarrier(carrierId);
            carrier.SetWafer(slot, wafer);
        }

        public void RemoveWafer(string carrierId, int slot)
        {
            var carrier = GetCarrier(carrierId);
            carrier?.RemoveWafer(slot);
        }

        // ===== 새 Lot 로딩 =====
        public MaterialWafer LoadNewLot(string carrierId, int slot, string lotId)
        {
            lock (_gate)
            {
                // 기존 Wafer 저장
                _config.Save();   // BaseConfig.Save() 사용 (GetFilePath() 내부에서 경로 지정)

                // 새 WaferData 생성
                var newWafer = new MaterialWafer
                {
                    WaferId = lotId,
                    CarrierId = carrierId,
                    SlotIndex = slot,
                    WaferDate = DateTime.Now.ToString("yyyyMMdd")
                };

                AddWafer(carrierId, slot, newWafer);
                return newWafer;
            }
        }

        // ===== 저장 =====
        public void Save()
        {
            lock (_gate)
            {
                _config.Save();  // BaseConfig.Save() 호출
            }
        }

        public void SetRecipeKeys(string carrierId, int slot, List<string> keys, bool force = false)
        {
            var wafer = GetWafer(carrierId, slot);
            if (wafer == null) return;

            // 진행 중인 Wafer에 덮어씌우지 않도록 보호
            if (wafer.Summary.InProcess && !force) return;

            wafer.RecipeKeys = keys;
            lock (wafer.Dies)
            {
                foreach (var c in wafer.Dies)
                {
                    c.MeasureValues.Clear();
                    foreach (var key in keys)
                        c.AddMeasure(key, double.NaN);
                }
            }
        }

    }
}
