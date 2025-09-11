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
    /// 
    internal static class IO
    {
        // Inputs (Sensor)
        public const string RING_CHECK0 = "WAFER STAGE RING CHECK 0";  // X025
        public const string RING_CHECK1 = "WAFER STAGE RING CHECK 1";  // X026
        public const string CLAMP_DOWN_SNS = "WAFER STAGE CLAMP DOWN";    // X027 (클램프 Down 위치 감지)
        public const string CLAMP_FWD_SNS = "WAFER STAGE CLAMP";         // X028 (클램프 Up/Clamp 상태)
        public const string EXPANDER_UP_SNS = "WAFER STAGE EXPANDER UP";   // X029
        public const string EXPANDER_DOWN_SNS = "WAFER STAGE EXPANDER DOWN"; // X030
        public const string VAC_OK_SNS = "EJECTOR VACUUM CHECK";      // X031

        // Outputs (Valve)
        public const string CLAMP_UP_OUT = "WAFER STAGE CLAMP UP";      // Y020 (Lift Up)
        public const string CLAMP_DOWN_OUT = "WAFER STAGE CLAMP DOWN";    // Y021 (Lift Down)
        public const string CLAMP_FWD_OUT = "WAFER STAGE CLAMP";         // Y022 (Clamp Forward / Close)
        public const string CLAMP_BWD_OUT = "WAFER STAGE UNCLAMP";       // Y023 (Clamp Back / Open)
        public const string EXPANDER_UP_OUT = "WAFER STAGE EXPANDER UP";   // Y024
        public const string EXPANDER_DOWN_OUT = "WAFER STAGE EXPANDER DOWN"; // Y025
        public const string VAC_OUT = "EJECTOR VACUUM";            // Y038

        // Inputs
        public const string SPHERE_FW_SNS = "SPHERE FW";                // X038 (Forward sensor)
        public const string SPHERE_BW_SNS = "SPHERE BW";                // X039 (Backward sensor)
        public const string PROBE_VAC_OK = "PROBE CARD VACUUM CHECK";  // X050
                                                                       // Outputs
        public const string SPHERE_FW_VLV = "SPHERE FW";                // Y026 (Forward valve)
        public const string SPHERE_BW_VLV = "SPHERE BW";                // Y027 (Backward valve)
        public const string PROBE_VAC_VLV = "PROBE CARD VACUUM";  // Y075 (Vac valve or combined channel)

        // Inputs (X032~X037)
        public const string LEFT_AIR_TANK_PRESSURE = "LEFT TOOL AIR TANK PRESSURE CHECK";      // X032
        public const string LEFT_VAC_TANK_PRESSURE = "LEFT TOOL VACUUM TANK PRESSURE CHECK";   // X033
        public const string LEFT_ARM1_FLOW = "LEFT TOOL ARM 1 FLOW CHECK";                     // X034
        public const string LEFT_ARM2_FLOW = "LEFT TOOL ARM 2 FLOW CHECK";                     // X035
        public const string LEFT_ARM3_FLOW = "LEFT TOOL ARM 3 FLOW CHECK";                     // X036
        public const string LEFT_ARM4_FLOW = "LEFT TOOL ARM 4 FLOW CHECK";                     // X037

        // Outputs (Y039~Y050)
        public const string LEFT_ARM1_VAC = "LEFT ARM 1 VACUUM"; // Y039
        public const string LEFT_ARM2_VAC = "LEFT ARM 2 VACUUM"; // Y040
        public const string LEFT_ARM3_VAC = "LEFT ARM 3 VACUUM"; // Y041
        public const string LEFT_ARM4_VAC = "LEFT ARM 4 VACUUM"; // Y042
        public const string LEFT_ARM1_BLOW = "LEFT ARM 1 BLOW";   // Y043
        public const string LEFT_ARM2_BLOW = "LEFT ARM 2 BLOW";   // Y044
        public const string LEFT_ARM3_BLOW = "LEFT ARM 3 BLOW";   // Y045
        public const string LEFT_ARM4_BLOW = "LEFT ARM 4 BLOW";   // Y046
        public const string LEFT_ARM1_VENT = "LEFT ARM 1 VENT";   // Y047
        public const string LEFT_ARM2_VENT = "LEFT ARM 2 VENT";   // Y048
        public const string LEFT_ARM3_VENT = "LEFT ARM 3 VENT";   // Y049
        public const string LEFT_ARM4_VENT = "LEFT ARM 4 VENT";   // Y050

        // Sensors (Inputs)
        public const string WAFER_FEEDER_UP = "WAFER FEEDER UP";              // X020
        public const string WAFER_FEEDER_DOWN = "WAFER FEEDER DOWN";            // X021
        public const string WAFER_FEEDER_UNCLAMP = "WAFER FEEDER UNCLAMP";         // X022 (Open 상태 확인)
        public const string WAFER_FEEDER_RING_CHECK = "WAFER FEEDER RING CHECK";      // X023
        public const string WAFER_FEEDER_OVERLOAD = "WAFER FEEDER OVERLOAD CHECK";  // X024

        // Valves (Outputs)
        public const string WAFER_FEEDER_UP_VALVE = "WAFER FEEDER UP";              // Y016 Up 솔
        public const string WAFER_FEEDER_DOWN_VALVE = "WAFER FEEDER DOWN";            // Y017 Down 솔 (기존 DOWNE 오타 정규화)
        public const string WAFER_FEEDER_CLAMP_VALVE = "WAFER FEEDER CLAMP";           // Y018 Clamp (Close)
        public const string WAFER_FEEDER_UNCLAMP_VALVE = "WAFER FEEDER UNCLAMP";         // Y019 Unclamp (Open)

        // Inputs
        public const string RIGHT_AIR_TANK_PRESS = "RIGHT TOOL AIR TANK PRESSURE CHECK";
        public const string RIGHT_VAC_TANK_PRESS = "RIGHT TOOL VACUUM TANK PRESSURE CHECK";
        public const string RIGHT_ARM1_FLOW = "RIGHT TOOL ARM 1 FLOW CHECK";
        public const string RIGHT_ARM2_FLOW = "RIGHT TOOL ARM 2 FLOW CHECK";
        public const string RIGHT_ARM3_FLOW = "RIGHT TOOL ARM 3 FLOW CHECK";
        public const string RIGHT_ARM4_FLOW = "RIGHT TOOL ARM 4 FLOW CHECK";

        // Outputs (Vac / Blow / Vent)
        public const string RIGHT_ARM1_VAC = "RIGHT ARM 1 VACUUM";
        public const string RIGHT_ARM2_VAC = "RIGHT ARM 2 VACUUM";
        public const string RIGHT_ARM3_VAC = "RIGHT ARM 3 VACUUM";
        public const string RIGHT_ARM4_VAC = "RIGHT ARM 4 VACUUM";
        public const string RIGHT_ARM1_BLOW = "RIGHT ARM 1 BLOW";
        public const string RIGHT_ARM2_BLOW = "RIGHT ARM 2 BLOW";
        public const string RIGHT_ARM3_BLOW = "RIGHT ARM 3 BLOW";
        public const string RIGHT_ARM4_BLOW = "RIGHT ARM 4 BLOW";
        public const string RIGHT_ARM1_VENT = "RIGHT ARM 1 VENT";
        public const string RIGHT_ARM2_VENT = "RIGHT ARM 2 VENT";
        public const string RIGHT_ARM3_VENT = "RIGHT ARM 3 VENT";
        public const string RIGHT_ARM4_VENT = "RIGHT ARM 4 VENT";

        // Inputs
        public const string BIN_FEEDER_UP = "BIN FEEDER UP";              // X064
        public const string BIN_FEEDER_DOWN = "BIN FEEDER DOWN";            // X065
        public const string BIN_FEEDER_UNCLAMP = "BIN FEEDER UNCLAMP";         // X066 (Open 상태 확인)
        public const string BIN_FEEDER_RING_CHECK = "BIN FEEDER RING CHECK";      // X067
        public const string BIN_FEEDER_OVERLOAD = "BIN FEEDER OVERLOAD CHECK";  // X068

        // Outputs (원본 Config 의 DOWNE / UNCALMP 오타를 정규화하여 사용)
        public const string BIN_FEEDER_UP_VALVE = "BIN FEEDER UP";        // Y034 Up 솔
        public const string BIN_FEEDER_DOWN_VALVE = "BIN FEEDER DOWN";      // Y035 Down 솔 (원본: DOWNE)
        public const string BIN_FEEDER_CLAMP_VALVE = "BIN FEEDER CLAMP";     // Y036 Clamp
        public const string BIN_FEEDER_UNCLAMP_VALVE = "BIN FEEDER UNCLAMP";   // Y037 Unclamp (원본: UNCALMP)

        // Inputs
        public const string BIN_RING_CHECK0 = "BIN STAGE RING CHECK 0";          // X057
        public const string BIN_RING_CHECK1 = "BIN STAGE RING CHECK 1";          // X058
        public const string BIN_CLAMP_FWD_CHECK = "BIN STAGE CLAMP FWD CHECK";   // X059 (Clamp closed)
        public const string BIN_CLAMP_DOWN_CHECK = "BIN STAGE CLAMP DOWN CHECK"; // X060 (Lift down)
        public const string BIN_PLATE_UP = "BIN STAGE PLATE UP";                 // X061
        public const string BIN_PLATE_DOWN = "BIN STAGE PLATE DOWN";             // X062
        public const string BIN_VACUUM_CHECK = "BIN STAGE VACUUM CHECK";         // X063

        // Outputs
        public const string BIN_CLAMP_UP = "BIN STAGE CLAMP UP";     // Y028 (Lift Up valve)
        public const string BIN_CLAMP_DOWN = "BIN STAGE CLAMP DOWN"; // Y029 (Lift Down valve)
        public const string BIN_CLAMP_FWD = "BIN STAGE CLAMP FWD";   // Y030 (Clamp Close)
        public const string BIN_CLAMP_BWD = "BIN STAGE CLAMP BWD";   // Y031 (Clamp Open)
        public const string BIN_PLATE_UP_OUT = "BIN STAGE PLATE UP";   // Y032 (Plate Up valve)
        public const string BIN_PLATE_DOWN_OUT = "BIN STAGE PLATE DOWN"; // Y033 (Plate Down valve)
        public const string BIN_VACUUM = "BIN STAGE VACUUM";         // Y088 Vacuum valve

        //INDEX
        // Inputs (X040 ~ X049)
        public const string AIR_TANK_PRESSURE = "INDEX AIR TANK PRESSURE CHECK";      // X040
        public const string VAC_TANK_PRESSURE = "INDEX VACCUM TANK PRESSURE CHECK";    // X041 (table spelling)
                                                                                       // (legacy mis-typed string kept for backward compat)
        public const string VAC_TANK_PRESSURE_LEGACY = "INDEX VACCUM TANK PRRESSURE CHECK"; // old code spelling
        public const string INDEX_FLOW1 = "INDEX 1 FLOW CHECK"; // X042
        public const string INDEX_FLOW2 = "INDEX 2 FLOW CHECK"; // X043
        public const string INDEX_FLOW3 = "INDEX 3 FLOW CHECK"; // X044
        public const string INDEX_FLOW4 = "INDEX 4 FLOW CHECK"; // X045
        public const string INDEX_FLOW5 = "INDEX 5 FLOW CHECK"; // X046
        public const string INDEX_FLOW6 = "INDEX 6 FLOW CHECK"; // X047
        public const string INDEX_FLOW7 = "INDEX 7 FLOW CHECK"; // X048
        public const string INDEX_FLOW8 = "INDEX 8 FLOW CHECK"; // X049

        // Outputs (Y051 ~ Y058 Vacuum, Y059 ~ Y066 Blow, Y067 ~ Y074 Vent)
        public const string INDEX_VAC1 = "INDEX 1 VACUUM"; 
        public const string INDEX_VAC2 = "INDEX 2 VACUUM"; 
        public const string INDEX_VAC3 = "INDEX 3 VACUUM"; 
        public const string INDEX_VAC4 = "INDEX 4 VACUUM";
        public const string INDEX_VAC5 = "INDEX 5 VACUUM";
        public const string INDEX_VAC6 = "INDEX 6 VACUUM"; 
        public const string INDEX_VAC7 = "INDEX 7 VACUUM"; 
        public const string INDEX_VAC8 = "INDEX 8 VACUUM";
        public const string INDEX_BLOW1 = "INDEX 1 BLOW";
        public const string INDEX_BLOW2 = "INDEX 2 BLOW";
        public const string INDEX_BLOW3 = "INDEX 3 BLOW"; 
        public const string INDEX_BLOW4 = "INDEX 4 BLOW";
        public const string INDEX_BLOW5 = "INDEX 5 BLOW"; 
        public const string INDEX_BLOW6 = "INDEX 6 BLOW";
        public const string INDEX_BLOW7 = "INDEX 7 BLOW"; 
        public const string INDEX_BLOW8 = "INDEX 8 BLOW";
        public const string INDEX_VENT1 = "INDEX 1 VENT";
        public const string INDEX_VENT2 = "INDEX 2 VENT"; 
        public const string INDEX_VENT3 = "INDEX 3 VENT"; 
        public const string INDEX_VENT4 = "INDEX 4 VENT";
        public const string INDEX_VENT5 = "INDEX 5 VENT"; 
        public const string INDEX_VENT6 = "INDEX 6 VENT"; 
        public const string INDEX_VENT7 = "INDEX 7 VENT"; 
        public const string INDEX_VENT8 = "INDEX 8 VENT";

    }

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

            // ============ 여기에 프로젝트별 수동 바인딩 추가 ============.
            Cylinder cylinder = null;
            Vacuum vacuum = null;
            //inputStage
            // Vacuum
            DIO.MapByName(unit, "InStage.VacOut", true, IO.VAC_OUT);
            DIO.MapByName(unit, "InStage.VacOk", false, IO.VAC_OK_SNS);
            vacuum = new Vacuum("InStageVac", "InStage.VacOut", "InStage.VacOk");
            IoAutoBindings.Vacuums["InStageVac"] = vacuum;

            // Plate (Up/Down)
            DIO.MapByName(unit, "InStage.ExpUpOut", true, IO.EXPANDER_UP_OUT);
            DIO.MapByName(unit, "InStage.ExpDownOut", true, IO.EXPANDER_DOWN_OUT);
            DIO.MapByName(unit, "InStage.ExpUpIn", false, IO.EXPANDER_UP_SNS);
            DIO.MapByName(unit, "InStage.ExpDownIn", false, IO.EXPANDER_DOWN_SNS);
            cylinder = new Cylinder(
                "InStageExpander", 
                "InStage.ExpUpOut", 
                "InStage.ExpDownOut", 
                "InStage.ExpUpIn", 
                "InStage.ExpDownIn");
            IoAutoBindings.Cylinders["InStageExpander"] = cylinder;

            // Clamp Lift (Up/Down) -> sensors: Up sensor 없음 (Clamp Up 센서 공용 사용), Down 센서 존재
            DIO.MapByName(unit, "InStage.ClampUpOut", true, IO.CLAMP_UP_OUT);
            DIO.MapByName(unit, "InStage.ClampDownOut", true, IO.CLAMP_DOWN_OUT);
            DIO.MapByName(unit, "InStage.ClampDownIn", false, IO.CLAMP_DOWN_SNS);
            cylinder = new Cylinder(
                "InStageClampLift",
                "InStage.ClampUpOut",
                "InStage.ClampDownOut",
                "InStage.ClampUpIn/*NO_SENSOR*/",
                "InStage.ClampDownIn");
            IoAutoBindings.Cylinders["InStageClampLift"] = cylinder;

            // Clamp FWD/BWD (direct)
            DIO.MapByName(unit, "InStage.ClampFwdOut", true, IO.CLAMP_FWD_OUT);
            DIO.MapByName(unit, "InStage.ClampBwdOut", true, IO.CLAMP_BWD_OUT);
            DIO.MapByName(unit, "InStage.ClampFwdIn", false, IO.CLAMP_FWD_SNS);
            cylinder = new Cylinder(
                "InStageClampFB",
                "InStage.ClampFwdOut",
                "InStage.ClampBwdOut",
                "InStage.ClampFwdIn",
                "InStage.ClampBwdIn/*NO_SENSOR*/");
            IoAutoBindings.Cylinders["InStageClampFB"] = cylinder;

            //IndexChipProbeController
            DIO.MapByName(unit, "ProbeCtrl.VacOut", true, IO.PROBE_VAC_VLV);
            DIO.MapByName(unit, "ProbeCtrl.VacOk", false, IO.PROBE_VAC_OK);
            vacuum = new Vacuum("ProbeCardVac", "ProbeCtrl.VacOut", "ProbeCtrl.VacOk");
            IoAutoBindings.Vacuums["ProbeCardVac"] = vacuum;

            // Sphere Cylinder (Forward / Backward)
            DIO.MapByName(unit, "ProbeCtrl.SphereFwOut", true, IO.SPHERE_FW_VLV);
            DIO.MapByName(unit, "ProbeCtrl.SphereBwOut", true, IO.SPHERE_BW_VLV);
            DIO.MapByName(unit, "ProbeCtrl.SphereFwIn", false, IO.SPHERE_FW_SNS);
            DIO.MapByName(unit, "ProbeCtrl.SphereBwIn", false, IO.SPHERE_BW_SNS);
            cylinder = new Cylinder("ProbeSphere", "ProbeCtrl.SphereFwOut", "ProbeCtrl.SphereBwOut", "ProbeCtrl.SphereFwIn", "ProbeCtrl.SphereBwIn");
            IoAutoBindings.Cylinders["ProbeSphere"] = cylinder;

            //IndexChipProber - IO X
            //IndexLoadAligner - IO X
            //IndexUnloadAligner - IO X
            //InputCassetteLifter - vacuum, cylinder X

            //InputDieTransfer
            DIO.MapByName(unit, "InputDieTransfer.VacOut1", true, IO.LEFT_ARM1_VAC);
            DIO.MapByName(unit, "InputDieTransfer.VacOk1", false, IO.LEFT_ARM1_FLOW);
            vacuum = new Vacuum("InputDieTransferVac1", "InputDieTransfer.VacOut1", "InputDieTransfer.VacOk1");
            IoAutoBindings.Vacuums["InputDieTransferVac1"] = vacuum;

            DIO.MapByName(unit, "InputDieTransfer.VacOut2", true, IO.LEFT_ARM2_VAC);
            DIO.MapByName(unit, "InputDieTransfer.VacOk2", false, IO.LEFT_ARM2_FLOW);
            vacuum = new Vacuum("InputDieTransferVac2", "InputDieTransfer.VacOut2", "InputDieTransfer.VacOk2");
            IoAutoBindings.Vacuums["InputDieTransferVac2"] = vacuum;

            DIO.MapByName(unit, "InputDieTransfer.VacOut3", true, IO.LEFT_ARM3_VAC);
            DIO.MapByName(unit, "InputDieTransfer.VacOk3", false, IO.LEFT_ARM3_FLOW);
            vacuum = new Vacuum("InputDieTransferVac3", "InputDieTransfer.VacOut3", "InputDieTransfer.VacOk3");
            IoAutoBindings.Vacuums["InputDieTransferVac3"] = vacuum;

            DIO.MapByName(unit, "InputDieTransfer.VacOut4", true, IO.LEFT_ARM4_VAC);
            DIO.MapByName(unit, "InputDieTransfer.VacOk4", false, IO.LEFT_ARM4_FLOW);
            vacuum = new Vacuum("InputDieTransferVac4", "InputDieTransfer.VacOut4", "InputDieTransfer.VacOk4");
            IoAutoBindings.Vacuums["InputDieTransferVac4"] = vacuum;

            // 센서 없는 Blow/Vent 같은 채널(출력만 존재)
            DIO.MapByName(unit, "InputDieTransfer.BlowOut1", true, IO.LEFT_ARM1_BLOW);
            var blow = new Vacuum("InputDieTransferBlow1", "InputDieTransfer.BlowOut1", null);
            IoAutoBindings.Vacuums["InputDieTransferBlow1"] = blow;

            DIO.MapByName(unit, "InputDieTransfer.BlowOut2", true, IO.LEFT_ARM2_BLOW);
            blow = new Vacuum("InputDieTransferBlow2", "InputDieTransfer.BlowOut2", null);
            IoAutoBindings.Vacuums["InputDieTransferBlow2"] = blow;

            DIO.MapByName(unit, "InputDieTransfer.BlowOut3", true, IO.LEFT_ARM3_BLOW);
            blow = new Vacuum("InputDieTransferBlow3", "InputDieTransfer.BlowOut3", null);
            IoAutoBindings.Vacuums["InputDieTransferBlow3"] = blow;

            DIO.MapByName(unit, "InputDieTransfer.BlowOut4", true, IO.LEFT_ARM4_BLOW);
            blow = new Vacuum("InputDieTransferBlow4", "InputDieTransfer.BlowOut4", null);
            IoAutoBindings.Vacuums["InputDieTransferBlow4"] = blow;

            DIO.MapByName(unit, "InputDieTransfer.VentOut1", true, IO.LEFT_ARM1_VENT);
            var vent = new Vacuum("InputDieTransferVent1", "InputDieTransfer.VentOut1", null);
            IoAutoBindings.Vacuums["InputDieTransferVent1"] = vent;

            DIO.MapByName(unit, "InputDieTransfer.VentOut2", true, IO.LEFT_ARM2_VENT);
            vent = new Vacuum("InputDieTransferVent2", "InputDieTransfer.VentOut2", null);
            IoAutoBindings.Vacuums["InputDieTransferVent2"] = vent;

            DIO.MapByName(unit, "InputDieTransfer.VentOut3", true, IO.LEFT_ARM3_VENT);
            vent = new Vacuum("InputDieTransferVent3", "InputDieTransfer.VentOut3", null);
            IoAutoBindings.Vacuums["InputDieTransferVent3"] = vent;

            DIO.MapByName(unit, "InputDieTransfer.VentOut4", true, IO.LEFT_ARM4_VENT);
            vent = new Vacuum("InputDieTransferVent4", "InputDieTransfer.VentOut4", null);
            IoAutoBindings.Vacuums["InputDieTransferVent4"] = vent;

            //InputRingTransfer
            DIO.MapByName(unit, "InFeeder.UpOut", true, IO.WAFER_FEEDER_UP_VALVE);
            DIO.MapByName(unit, "InFeeder.DownOut", true, IO.WAFER_FEEDER_DOWN_VALVE);
            DIO.MapByName(unit, "InFeeder.UpIn", false, IO.WAFER_FEEDER_UP);
            DIO.MapByName(unit, "InFeeder.DownIn", false, IO.WAFER_FEEDER_DOWN);
            cylinder = new Cylinder(
               "InFeederLift",
               "InFeeder.UpOut",
               "InFeeder.DownOut",
               "InFeeder.UpIn",
               "InFeeder.DownIn"); 
            IoAutoBindings.Cylinders["InFeederLift"] = cylinder;

            // Clamp (Close/Open) - Only UNCLAMP sensor 존재
            DIO.MapByName(unit, "InFeeder.ClampOut", true, IO.WAFER_FEEDER_CLAMP_VALVE);
            DIO.MapByName(unit, "InFeeder.UnclampOut", true, IO.WAFER_FEEDER_UNCLAMP_VALVE);
            DIO.MapByName(unit, "InFeeder.UnclampIn", false, IO.WAFER_FEEDER_UNCLAMP);
            cylinder = new Cylinder(
                "InFeederClamp",
                "InFeeder.ClampOut",
                "InFeeder.UnclampOut",
                null,
                "InFeeder.UnclampIn");
            IoAutoBindings.Cylinders["InFeederClamp"] = cylinder;

            // InputStageEjector - IO X
            // OutputCassetteLifter - vacuum, cylinder X

            // OutputDieTransfer
            DIO.MapByName(unit, "OutputDieTransfer.VacOut1", true, IO.RIGHT_ARM1_VAC);
            DIO.MapByName(unit, "OutputDieTransfer.VacOk1", false, IO.RIGHT_ARM1_FLOW);
            vacuum = new Vacuum("OutputDieTransferVac1", "OutputDieTransfer.VacOut1", "OutputDieTransfer.VacOk1");
            IoAutoBindings.Vacuums["OutputDieTransferVac1"] = vacuum;

            DIO.MapByName(unit, "OutputDieTransfer.VacOut2", true, IO.RIGHT_ARM2_VAC);
            DIO.MapByName(unit, "OutputDieTransfer.VacOk2", false, IO.RIGHT_ARM2_FLOW);
            vacuum = new Vacuum("OutputDieTransferVac2", "OutputDieTransfer.VacOut2", "OutputDieTransfer.VacOk2");
            IoAutoBindings.Vacuums["OutputDieTransferVac2"] = vacuum;

            DIO.MapByName(unit, "OutputDieTransfer.VacOut3", true, IO.RIGHT_ARM3_VAC);
            DIO.MapByName(unit, "OutputDieTransfer.VacOk3", false, IO.RIGHT_ARM3_FLOW);
            vacuum = new Vacuum("OutputDieTransferVac3", "OutputDieTransfer.VacOut3", "OutputDieTransfer.VacOk3");
            IoAutoBindings.Vacuums["OutputDieTransferVac3"] = vacuum;

            DIO.MapByName(unit, "OutputDieTransfer.VacOut4", true, IO.RIGHT_ARM4_VAC);
            DIO.MapByName(unit, "OutputDieTransfer.VacOk4", false, IO.RIGHT_ARM4_FLOW);
            vacuum = new Vacuum("OutputDieTransferVac4", "OutputDieTransfer.VacOut4", "OutputDieTransfer.VacOk4");
            IoAutoBindings.Vacuums["OutputDieTransferVac4"] = vacuum;

            // 센서 없는 Blow/Vent 같은 채널(출력만 존재)
            DIO.MapByName(unit, "OutputDieTransfer.BlowOut1", true, IO.RIGHT_ARM1_BLOW);
            blow = new Vacuum("OutputDieTransferBlow1", "OutputDieTransfer.BlowOut1", null);
            IoAutoBindings.Vacuums["OutputDieTransferBlow1"] = blow;

            DIO.MapByName(unit, "OutputDieTransfer.BlowOut2", true, IO.RIGHT_ARM2_BLOW);
            blow = new Vacuum("OutputDieTransferBlow2", "OutputDieTransfer.BlowOut2", null);
            IoAutoBindings.Vacuums["OutputDieTransferBlow2"] = blow;

            DIO.MapByName(unit, "OutputDieTransfer.BlowOut3", true, IO.RIGHT_ARM3_BLOW);
            blow = new Vacuum("OutputDieTransferBlow3", "OutputDieTransfer.BlowOut3", null);
            IoAutoBindings.Vacuums["OutputDieTransferBlow3"] = blow;

            DIO.MapByName(unit, "OutputDieTransfer.BlowOut4", true, IO.RIGHT_ARM4_BLOW);
            blow = new Vacuum("OutputDieTransferBlow4", "OutputDieTransfer.BlowOut4", null);
            IoAutoBindings.Vacuums["OutputDieTransferBlow4"] = blow;

            DIO.MapByName(unit, "OutputDieTransfer.VentOut1", true, IO.RIGHT_ARM1_VENT);
            vent = new Vacuum("OutputDieTransferVent1", "OutputDieTransfer.VentOut1", null);
            IoAutoBindings.Vacuums["OutputDieTransferVent1"] = vent;

            DIO.MapByName(unit, "OutputDieTransfer.VentOut2", true, IO.RIGHT_ARM2_VENT);
            vent = new Vacuum("OutputDieTransferVent2", "OutputDieTransfer.VentOut2", null);
            IoAutoBindings.Vacuums["OutputDieTransferVent2"] = vent;

            DIO.MapByName(unit, "OutputDieTransfer.VentOut3", true, IO.RIGHT_ARM3_VENT);
            vent = new Vacuum("OutputDieTransferVent3", "OutputDieTransfer.VentOut3", null);
            IoAutoBindings.Vacuums["OutputDieTransferVent3"] = vent;

            DIO.MapByName(unit, "OutputDieTransfer.VentOut4", true, IO.RIGHT_ARM4_VENT);
            vent = new Vacuum("OutputDieTransferVent4", "OutputDieTransfer.VentOut4", null);
            IoAutoBindings.Vacuums["OutputDieTransferVent4"] = vent;

            // OutputRingTransfer
            DIO.MapByName(unit, "OutFeeder.UpOut", true, IO.BIN_FEEDER_UP_VALVE);
            DIO.MapByName(unit, "OutFeeder.DownOut", true, IO.BIN_FEEDER_DOWN_VALVE);
            DIO.MapByName(unit, "OutFeeder.UpIn", false, IO.BIN_FEEDER_UP);
            DIO.MapByName(unit, "OutFeeder.DownIn", false, IO.BIN_FEEDER_DOWN);
            cylinder = new Cylinder(
                "OutFeederLift",
                "OutFeeder.UpOut",
                "OutFeeder.DownOut",
                "OutFeeder.UpIn",
                "OutFeeder.DownIn");
            IoAutoBindings.Cylinders["OutFeederLift"] = cylinder;

            // Clamp (Close/Open) - Only UNCLAMP sensor 존재
            DIO.MapByName(unit, "OutFeeder.ClampOut", true, IO.BIN_FEEDER_CLAMP_VALVE);
            DIO.MapByName(unit, "OutFeeder.UnclampOut", true, IO.BIN_FEEDER_UNCLAMP_VALVE);
            DIO.MapByName(unit, "OutFeeder.UnclampIn", false, IO.BIN_FEEDER_UNCLAMP);
            cylinder = new Cylinder(
                "OutFeederClamp",
                "OutFeeder.ClampOut",
                "OutFeeder.UnclampOut",
                null,
                "OutFeeder.UnclampIn");
            IoAutoBindings.Cylinders["OutFeederClamp"] = cylinder;

            //OutputStage
            // Vacuum
            DIO.MapByName(unit, "OutStage.VacOut", true, IO.BIN_VACUUM);
            DIO.MapByName(unit, "OutStage.VacOk", false, IO.BIN_VACUUM_CHECK);
            vacuum = new Vacuum("OutStageVac", "OutStage.VacOut", "OutStage.VacOk");
            IoAutoBindings.Vacuums["OutStageVac"] = vacuum;

            // Plate
            DIO.MapByName(unit, "OutStage.PlateUpOut", true, IO.BIN_PLATE_UP_OUT);
            DIO.MapByName(unit, "OutStage.PlateDownOut", true, IO.BIN_PLATE_DOWN_OUT);
            DIO.MapByName(unit, "OutStage.PlateUpIn", false, IO.BIN_PLATE_UP);
            DIO.MapByName(unit, "OutStage.PlateDownIn", false, IO.BIN_PLATE_DOWN);
            cylinder = new Cylinder(
                "OutStagePlate", 
                "OutStage.PlateUpOut", 
                "OutStage.PlateDownOut", 
                "OutStage.PlateUpIn", 
                "OutStage.PlateDownIn");
            IoAutoBindings.Cylinders["OutStagePlate"] = cylinder;

            // Lift
            DIO.MapByName(unit, "OutStage.LiftUpOut", true, IO.BIN_CLAMP_UP);
            DIO.MapByName(unit, "OutStage.LiftDownOut", true, IO.BIN_CLAMP_DOWN);
            DIO.MapByName(unit, "OutStage.LiftDownIn", false, IO.BIN_CLAMP_DOWN_CHECK);
            cylinder = new Cylinder(
                "OutStageLift",
                "OutStage.LiftUpOut",
                "OutStage.LiftDownOut",
                null,
                "OutStage.LiftDownIn");
            IoAutoBindings.Cylinders["OutStageLift"] = cylinder;

            // Clamp FWD/BWD
            DIO.MapByName(unit, "OutStage.ClampFwdOut", true, IO.BIN_CLAMP_FWD);
            DIO.MapByName(unit, "OutStage.ClampBwdOut", true, IO.BIN_CLAMP_BWD);
            DIO.MapByName(unit, "OutStage.ClampFwdIn", false, IO.BIN_CLAMP_FWD_CHECK);
            cylinder = new Cylinder(
                "OutStageClampFB",
                "OutStage.ClampFwdOut",
                "OutStage.ClampBwdOut",
                "OutStage.ClampFwdIn",
                null);
            IoAutoBindings.Cylinders["OutStageClampFB"] = cylinder;


            //Rotaty
            DIO.MapByName(unit, "Rotaty.VacOut1", true, IO.INDEX_VAC1);
            DIO.MapByName(unit, "Rotaty.VacOk1", false, IO.INDEX_FLOW1);
            vacuum = new Vacuum("RotatyVac1", "Rotaty.VacOut1", "Rotaty.VacOk1");
            IoAutoBindings.Vacuums["RotatyVac1"] = vacuum;

            DIO.MapByName(unit, "Rotaty.VacOut2", true, IO.INDEX_VAC2);
            DIO.MapByName(unit, "Rotaty.VacOk2", false, IO.INDEX_FLOW2);
            vacuum = new Vacuum("RotatyVac2", "Rotaty.VacOut2", "Rotaty.VacOk2");
            IoAutoBindings.Vacuums["RotatyVac2"] = vacuum;

            DIO.MapByName(unit, "Rotaty.VacOut3", true, IO.INDEX_VAC3);
            DIO.MapByName(unit, "Rotaty.VacOk3", false, IO.INDEX_FLOW3);
            vacuum = new Vacuum("RotatyVac3", "Rotaty.VacOut3", "Rotaty.VacOk3");
            IoAutoBindings.Vacuums["RotatyVac3"] = vacuum;

            DIO.MapByName(unit, "Rotaty.VacOut4", true, IO.INDEX_VAC4);
            DIO.MapByName(unit, "Rotaty.VacOk4", false, IO.INDEX_FLOW4);
            vacuum = new Vacuum("RotatyVac4", "Rotaty.VacOut4", "Rotaty.VacOk4");
            IoAutoBindings.Vacuums["RotatyVac4"] = vacuum;

            DIO.MapByName(unit, "Rotaty.VacOut5", true, IO.INDEX_VAC5);
            DIO.MapByName(unit, "Rotaty.VacOk5", false, IO.INDEX_FLOW5);
            vacuum = new Vacuum("RotatyVac5", "Rotaty.VacOut5", "Rotaty.VacOk5");
            IoAutoBindings.Vacuums["RotatyVac5"] = vacuum;

            DIO.MapByName(unit, "Rotaty.VacOut6", true, IO.INDEX_VAC6);
            DIO.MapByName(unit, "Rotaty.VacOk6", false, IO.INDEX_FLOW6);
            vacuum = new Vacuum("RotatyVac6", "Rotaty.VacOut6", "Rotaty.VacOk6");
            IoAutoBindings.Vacuums["RotatyVac6"] = vacuum;

            DIO.MapByName(unit, "Rotaty.VacOut7", true, IO.INDEX_VAC7);
            DIO.MapByName(unit, "Rotaty.VacOk7", false, IO.INDEX_FLOW7);
            vacuum = new Vacuum("RotatyVac7", "Rotaty.VacOut7", "Rotaty.VacOk7");
            IoAutoBindings.Vacuums["RotatyVac7"] = vacuum;

            DIO.MapByName(unit, "Rotaty.VacOut8", true, IO.INDEX_VAC8);
            DIO.MapByName(unit, "Rotaty.VacOk8", false, IO.INDEX_FLOW8);
            vacuum = new Vacuum("RotatyVac8", "Rotaty.VacOut8", "Rotaty.VacOk8");
            IoAutoBindings.Vacuums["RotatyVac8"] = vacuum;

            // 센서 없는 Blow/Vent 같은 채널(출력만 존재)
            DIO.MapByName(unit, "Rotaty.BlowOut1", true, IO.INDEX_BLOW1);
            blow = new Vacuum("RotatyBlow1", "Rotaty.BlowOut1", null);
            IoAutoBindings.Vacuums["RotatyBlow1"] = blow;

            DIO.MapByName(unit, "Rotaty.BlowOut2", true, IO.INDEX_BLOW2);
            blow = new Vacuum("RotatyBlow2", "Rotaty.BlowOut2", null);
            IoAutoBindings.Vacuums["RotatyBlow2"] = blow;

            DIO.MapByName(unit, "Rotaty.BlowOut3", true, IO.INDEX_BLOW3);
            blow = new Vacuum("RotatyBlow3", "Rotaty.BlowOut3", null);
            IoAutoBindings.Vacuums["RotatyBlow3"] = blow;

            DIO.MapByName(unit, "Rotaty.BlowOut4", true, IO.INDEX_BLOW4);
            blow = new Vacuum("RotatyBlow4", "Rotaty.BlowOut4", null);
            IoAutoBindings.Vacuums["RotatyBlow4"] = blow;

            DIO.MapByName(unit, "Rotaty.BlowOut5", true, IO.INDEX_BLOW5);
            blow = new Vacuum("RotatyBlow5", "Rotaty.BlowOut5", null);
            IoAutoBindings.Vacuums["RotatyBlow5"] = blow;

            DIO.MapByName(unit, "Rotaty.BlowOut6", true, IO.INDEX_BLOW6);
            blow = new Vacuum("RotatyBlow6", "Rotaty.BlowOut6", null);
            IoAutoBindings.Vacuums["RotatyBlow6"] = blow;

            DIO.MapByName(unit, "Rotaty.BlowOut7", true, IO.INDEX_BLOW7);
            blow = new Vacuum("RotatyBlow7", "Rotaty.BlowOut7", null);
            IoAutoBindings.Vacuums["RotatyBlow7"] = blow;

            DIO.MapByName(unit, "Rotaty.BlowOut8", true, IO.INDEX_BLOW8);
            blow = new Vacuum("RotatyBlow8", "Rotaty.BlowOut8", null);
            IoAutoBindings.Vacuums["RotatyBlow8"] = blow;


            DIO.MapByName(unit, "Rotaty.VentOut1", true, IO.INDEX_VENT1);
            vent = new Vacuum("RotatyVent1", "Rotaty.VentOut1", null);
            IoAutoBindings.Vacuums["RotatyVent1"] = vent;

            DIO.MapByName(unit, "Rotaty.VentOut2", true, IO.INDEX_VENT2);
            vent = new Vacuum("RotatyVent2", "Rotaty.VentOut2", null);
            IoAutoBindings.Vacuums["RotatyVent2"] = vent;

            DIO.MapByName(unit, "Rotaty.VentOut3", true, IO.INDEX_VENT3);
            vent = new Vacuum("RotatyVent3", "Rotaty.VentOut3", null);
            IoAutoBindings.Vacuums["RotatyVent3"] = vent;

            DIO.MapByName(unit, "Rotaty.VentOut4", true, IO.INDEX_VENT4);
            vent = new Vacuum("RotatyVent4", "Rotaty.VentOut4", null);
            IoAutoBindings.Vacuums["RotatyVent4"] = vent;

            DIO.MapByName(unit, "Rotaty.VentOut5", true, IO.INDEX_VENT5);
            vent = new Vacuum("RotatyVent5", "Rotaty.VentOut5", null);
            IoAutoBindings.Vacuums["RotatyVent5"] = vent;

            DIO.MapByName(unit, "Rotaty.VentOut6", true, IO.INDEX_VENT6);
            vent = new Vacuum("RotatyVent6", "Rotaty.VentOut6", null);
            IoAutoBindings.Vacuums["RotatyVent6"] = vent;

            DIO.MapByName(unit, "Rotaty.VentOut7", true, IO.INDEX_VENT7);
            vent = new Vacuum("RotatyVent7", "Rotaty.VentOut7", null);
            IoAutoBindings.Vacuums["RotatyVent7"] = vent;

            DIO.MapByName(unit, "Rotaty.VentOut8", true, IO.INDEX_VENT8);
            vent = new Vacuum("RotatyVent8", "Rotaty.VentOut8", null);
            IoAutoBindings.Vacuums["RotatyVent8"] = vent;

            // ============ 프로젝트 공통: 중앙 CylinderConfig 로드 & 주입 ============

            try
            {
                // 자동/수동으로 바인딩된 모든 실린더에 대해 설정 로드/주입
                foreach (var kv in IoAutoBindings.Cylinders)
                {
                    var cylName = kv.Key;
                    var cyl = kv.Value;
                    try
                    {
                        var cfg = new CylinderConfig { Name = cylName };
                        // 파일 없으면 생성, 있으면 로드 후 누락 필드 보강 저장
                        cfg = CylinderConfig.LoadOrCreate(cfg.GetFilePath(), indented: true, backfill: true);
                        cyl.Config = cfg;
                    }
                    catch (Exception ex)
                    {
                        Log.Write("IoBindings", "ManualPatch", $"CylinderConfig load failed: {cylName} - {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write("IoBindings", "ManualPatch", "Central cylinder config injection error: " + ex.Message);
            }


            // ============ 필요 시: 키 별칭(별도 표준 키 제공) ============
            // 현장 채널명 스캔으로 생성된 키가 제각각일 수 있어,
            // 유닛 코드에서 쉽게 찾도록 표준 키를 추가 등록(별칭)합니다.
            // 예) InputStage:
            //  - InStageExpander / InStageClampLift / InStageClampFB
            //try
            //{
            //    foreach (var kv in IoAutoBindings.Cylinders)
            //    {
            //        var k = kv.Key.ToUpperInvariant();

            //        // Expander
            //        if (k.Contains("INSTAGE") && (k.Contains("EXPAND") || k.Contains("PLATE")))
            //            IoAutoBindings.Cylinders["InStageExpander"] = kv.Value;

            //        // Clamp Lift (Up/Down)
            //        if (k.Contains("INSTAGE") && k.Contains("CLAMP") && (k.Contains("LIFT") || k.Contains("UP") || k.Contains("DOWN")))
            //            IoAutoBindings.Cylinders["InStageClampLift"] = kv.Value;

            //        // Clamp FWD/BWD
            //        if (k.Contains("INSTAGE") && k.Contains("CLAMP") && (k.Contains("FWD") || k.Contains("FORWARD") || k.Contains("BWD") || k.Contains("BACK")))
            //            IoAutoBindings.Cylinders["InStageClampFB"] = kv.Value;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Log.Write("IoBindings", "ManualPatch", "Alias mapping failed: " + ex.Message);
            //}
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