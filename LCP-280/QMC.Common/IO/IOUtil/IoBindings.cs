// IoBindings.cs
using System;
using QMC.Common;
using QMC.Common.IO;       // DIOUnit
using QMC.Common.IOUtil;  // DIO, Cylinder, Vacuum, IoAutoBindings

namespace QMC.Common.IOUtil
{
    /// <summary>
    /// 하이브리드 바인딩:
    /// 1) 자동 바인딩(IoAutoBindings.RegisterAll)으로 대부분을 구성
    /// 2) 그 위에 수동 바인딩(패치/추가/오버라이드)을 적용
    /// 
    /// 사용 예:
    ///   IoBindings.RegisterAll();   // 앱 시작 시 1회
    ///   DIO.Out("WAFER FEEDER.UPOut", true); // 자동 생성된 키
    ///   IoAutoBindings.Cylinders["WAFER FEEDER"].Extend(); // 자동 생성된 도메인
    /// </summary>
    public static class IoBindings
    {
        /// <summary>
        /// 자동 바인딩 → 수동 패치 순으로 실행.
        /// unit을 넘기면 그걸 사용하고, 아니면 EquipmentLocator.Instance.UnitIO 사용.
        /// </summary>
        public static void RegisterAll(DIOUnit unit = null)
        {
            // --- 안전 체크 ---
            //var inst = EquipmentLocator.Instance
            //    ?? throw new InvalidOperationException("EquipmentLocator.Initialize(...) 이전에 IoBindings.RegisterAll()이 호출되었습니다.");

            //unit ??= inst.UnitIO
            //    ?? throw new InvalidOperationException("Equipment.UnitIO가 초기화되지 않았습니다.");

            var inst = EquipmentLocator.Instance;
            if (inst == null)
                throw new InvalidOperationException("EquipmentLocator.Initialize(...) 이전에 IoBindings.RegisterAll()이 호출되었습니다.");

            if (unit == null)
            {
                unit = inst.UnitIO;
                if (unit == null)
                    throw new InvalidOperationException("Equipment.UnitIO가 초기화되지 않았습니다.");
            }

            // 1) 자동 바인딩 (JSON 스캔 → IO 키/Domains 생성)
            IoAutoBindings.RegisterAll();

            // 2) 수동 패치/추가/오버라이드 (원하는 만큼 아래에 작성)
            ManualPatch(unit);
        }

        /// <summary>
        /// 이 메서드 안에서만 수동 등록을 작성하세요.
        /// - 같은 키로 다시 Map하면 자동 바인딩을 덮어씁니다(override).
        /// - Cylinders/Vacuums 딕셔너리에 새로 넣거나 같은 이름으로 넣으면 교체됩니다.
        /// </summary>
        private static void ManualPatch(DIOUnit unit)
        {
            // ============ 예시 ============

            // 2-1) IO 키 수동 매핑 (자동 생성 키를 덮어쓰기 또는 새 키 추가)
            //  - 채널 이름(Name)으로 찾고 싶으면 MapByName 사용
            //  - 모듈/표시번호를 직접 지정하려면 Map 사용
            //
            // DIO.MapByName(unit, "Stack.Red",    isOutput: true,  channelName: "TL RED");
            // DIO.MapByName(unit, "Stack.Yellow", isOutput: true,  channelName: "TL YELLOW");
            // DIO.MapByName(unit, "Stack.Green",  isOutput: true,  channelName: "TL GREEN");
            //
            // DIO.Map("Some.Direct.Do",  "DIO Module1", "Y000", isOutput:true);
            // DIO.Map("Some.Direct.Di",  "DIO Module1", "X000", isOutput:false);

            // 2-2) 도메인(Cylinder/Vacuum) 오버라이드 또는 추가
            //  - 자동 생성된 “WAFER FEEDER” 실린더를 원하는 키로 재정의:
            // IoAutoBindings.Cylinders["WAFER FEEDER"] =
            //     new Cylinder("WAFER FEEDER", "Feeder.UpOut", "Feeder.DownOut", "Feeder.UpIn", "Feeder.DownIn");
            //
            //  - 새 실린더 추가(자동으로 못 만든 경우):
            // DIO.MapByName(unit, "Loader.ClampOut",   true,  "LOADER CLAMP");
            // DIO.MapByName(unit, "Loader.UnclampOut", true,  "LOADER UNCLAMP");
            // DIO.MapByName(unit, "Loader.ClampIn",    false, "LOADER CLAMP");
            // DIO.MapByName(unit, "Loader.UnclampIn",  false, "LOADER UNCLAMP");
            // IoAutoBindings.Cylinders["LOADER"] =
            //     new Cylinder("LOADER", "Loader.ClampOut", "Loader.UnclampOut", "Loader.ClampIn", "Loader.UnclampIn");
            //
            //  - 버큠 오버라이드/추가:
            // DIO.MapByName(unit, "Picker.VacOut", true,  "PICKER VACUUM");
            // DIO.MapByName(unit, "Picker.VacOk",  false, "PICKER VACUUM CHECK");
            // IoAutoBindings.Vacuums["PICKER"] =
            //     new Vacuum("PICKER", "Picker.VacOut", "Picker.VacOk");

            // ============ 여기에 프로젝트별 수동 바인딩 추가 ============
        }
    }
}


//사용 예시
// IO 직접
//DIO.Out("WAFER FEEDER.UPOut", true);

//// 도메인
//IoAutoBindings.Cylinders["WAFER FEEDER"].Extend();
//IoAutoBindings.Vacuums["EJECTOR"].OnWaitOk();

//if (QMC.Common.IOUtil.DIO.In("Feeder.UpIn", out var isUp) && isUp)
//{
//    // 센서 ON 로직
//}

//var unit = QMC.Common.EquipmentLocator.Instance.UnitIO;

//// DO/DI 키 등록(덮어쓰기 가능)
//QMC.Common.IOUtil.DIO.MapByName(unit, "Test.VacOut", true, "EJECTOR VACUUM");
//QMC.Common.IOUtil.DIO.MapByName(unit, "Test.VacOk", false, "EJECTOR VACUUM CHECK");

//// 사용
//QMC.Common.IOUtil.DIO.Out("Test.VacOut", true);
//QMC.Common.IOUtil.DIO.In("Test.VacOk", out var ok);

//QMC.Common.IOUtil.DIO.Map("Tmp.Lamp", "DIO Module1", "Y000", isOutput: true);
//QMC.Common.IOUtil.DIO.Out("Tmp.Lamp", true);

//// 실린더
//QMC.Common.IOUtil.IoAutoBindings.Cylinders["WAFER FEEDER"].Extend();

//// 버큠
//QMC.Common.IOUtil.IoAutoBindings.Vacuums["EJECTOR"].OnWaitOk();