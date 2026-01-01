using QMC.Common;
using QMC.Common.Component;
using QMC.Common.IOUtil;
using QMC.Common.Motion;
using QMC.Common.Motions;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Component;
using QMC.LCP_280.Process.Work;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using static QMC.LCP_280.Process.Component.MeasurementRecipe;
using static System.Windows.Forms.AxHost;

namespace QMC.LCP_280.Process.Unit
{
    /// <summary>
    /// OutputFeeder (Bin Feeder / Ring Transfer - Output side)
    ///  - Y 축 이송 + Lift + Clamp
    ///  - Ring 존재 / Overload 센서
    ///  - Config/Unit 구조를 다른 Unit들과 통일
    /// </summary>
    public class OutputFeeder : BaseUnit<OutputFeederConfig>
    {
        enum AlarmKeys
        {
            Alarm_BinLoadingFailed = 2000,
            Alarm_BarcodeReadingFailed = 2001,
            Alarm_StageLoadingFailed = 2002,
            Alarm_StageUnloadingFailed = 2003,
            Alarm_BinUnloadingFailed = 2004,

            Alarm_OutputStageInterlockFailed = 2010,

            Alarm_GripperClampFailed = 2020,
            Alarm_FeederClampUpDown = 2021,
            Alarm_IsBinReadyForLoading = 2022,
            Alarm_BinLoadingPosition = 2023,
            Alarm_OutputFeederNoPosition = 2024,
            Alarm_OutputFeederInterlockFailed = 2025,
            Alarm_OutputFeederBinData = 2026,
            Alarm_PrepareOutputStageUnloadingBin = 2027,
            Alarm_OutputCassetteLifter_Fail = 2028,

            // ===== [ADD] 타임아웃/상태 분리 (InputFeeder 2030~2033와 동일 의미) =====
            Alarm_FeederLiftUpTimeout = 2030,
            Alarm_FeederLiftDownTimeout = 2031,
            Alarm_FeederClampTimeout = 2032,
            Alarm_FeederUnclampTimeout = 2033,

            // ===== [ADD] 센서/데이터 불일치 분리 (InputFeeder 2040~2042와 유사) =====
            Alarm_BinMissingAfterStageToFeeder = 2040,
            Alarm_BinMissingAfterFeederToCassette = 2041,
            Alarm_BinSensorDataMismatch = 2042,

            // ===== [ADD] 언로딩 슬롯/카세트 단계 분리 (InputFeeder 2060~와 유사) =====
            Alarm_UnloadTargetSlotInvalid = 2060,
            Alarm_CassetteSlotNotEmptyForUnload = 2061,
            Alarm_CassetteMoveToSlotFailedForUnload = 2062,

            // ===== [ADD] Feeder->Cassette 상세 알람 (InputFeeder 2070~2074와 동일 스타일) =====
            Alarm_UnloadFeederToCassette_MoveFeederToCassettePosFailed = 2070,
            Alarm_UnloadFeederToCassette_UnclampFailed = 2071,
            Alarm_UnloadFeederToCassette_BinDataInvalid = 2072,
            Alarm_UnloadFeederToCassette_MoveStandbyBarcodeFailed = 2073,
            Alarm_UnloadFeederToCassette_MoveStandbyReadyFailed = 2074,

            Alarm_ScanBinFailed = 2080,
            Alarm_MoveToReadyFailed = 2081,
            Alarm_MoveToCassetteTeachFailed = 2082,
            Alarm_BinCassetteLoadingFailed = 2083,
        }

        #region InitAlarm
        protected override void InitAlarm()
        {
            base.InitAlarm();
            AlarmRegister((int)AlarmKeys.Alarm_BinLoadingFailed,
                "Bin Loading Failed",
                "Bin 로딩에 실패 하였습니다.",
                "Error");
            AlarmRegister((int)AlarmKeys.Alarm_BarcodeReadingFailed,
                "Barcode Reading Failed",
                "바코드 읽기에 실패 하였습니다. 바코드 상태를 확인 하여 주십시요",
                "Error");
            AlarmRegister((int)AlarmKeys.Alarm_StageLoadingFailed,
                "Stage Loading Failed",
                "스테이지 로딩에 실패 하였습니다.",
                "Error");
            AlarmRegister((int)AlarmKeys.Alarm_StageUnloadingFailed,
                "Stage Unloading Failed",
                "스테이지 언로딩에 실패 하였습니다.",
                "Error");
            AlarmRegister((int)AlarmKeys.Alarm_BinUnloadingFailed,
                "Bin Unloading Failed",
                "Bin 언로딩에 실패 하였습니다.",
                "Error");
            AlarmRegister((int)AlarmKeys.Alarm_OutputStageInterlockFailed,
                "Output Stage Interlock Failed",
                "Bin 로딩을 위한 인터락이 맞지 않습니다. 장비 상태를 확인 하여 주십시요.",
                "Error");
            AlarmRegister((int)AlarmKeys.Alarm_GripperClampFailed,
                "Gripper Clamp Failed",
                "그리퍼 클램프에 실패 하였습니다. 장비 상태를 확인 하여 주십시요.",
                "Error");
            AlarmRegister((int)AlarmKeys.Alarm_FeederClampUpDown,
                "Feeder Clamp Up Failed",
                "피더 클램프 업 상태가 아닙니다. 장비 상태를 확인 하여 주십시요.",
                "Error");

            // = 2022,
            AlarmRegister((int)AlarmKeys.Alarm_IsBinReadyForLoading,
                "Bin ReadyForLoading Failed",
                "Ready for Loading 위치가 아닙니다. 장비 상태를 확인 하여 주십시요.",
                "Error");
            // = 2023,
            AlarmRegister((int)AlarmKeys.Alarm_BinLoadingPosition,
                "Bin Loading Position Failed",
                "Loading 위치가 아닙니다. 장비 상태를 확인 하여 주십시요.",
                "Error");
            // = 2024,
            AlarmRegister((int)AlarmKeys.Alarm_OutputFeederNoPosition,
                "Output Feeder No Position",
                "Output Feeder 위치가 아닙니다. 장비 상태를 확인 하여 주십시요.",
                "Error");
            // = 2025,
            AlarmRegister((int)AlarmKeys.Alarm_OutputFeederInterlockFailed,
                "Output Feeder Interlock Failed",
                "Output Feeder 인터락이 맞지 않습니다. 장비 상태를 확인 하여 주십시요.",
                "Error");
            // = 2026,
            AlarmRegister((int)AlarmKeys.Alarm_OutputFeederBinData,
                "Output Feeder Bin Data Error",
                "Output Feeder Bin Data 오류입니다. 장비 상태를 확인 하여 주십시요.",
                "Error");
            AlarmRegister((int)AlarmKeys.Alarm_PrepareOutputStageUnloadingBin,
                "Output Feeder PrepareOutputStageUnloadingBin Error",
                "Output Feeder PrepareOutputStageUnloadingBin 오류입니다. 장비 상태를 확인 하여 주십시요.",
                "Error");

            //Alarm_OutputCassetteLifter_Fail
            AlarmRegister((int)AlarmKeys.Alarm_OutputCassetteLifter_Fail,
                "Output Feeder OutputCassetteLifter Slot Error",
                "Output Feeder OutputCassetteLifter Slot 오류입니다. 장비 상태를 확인 하여 주십시요.",
                "Error");

            // ===== 2030~ 타임아웃 =====
            AlarmRegister((int)AlarmKeys.Alarm_FeederLiftUpTimeout,
                "Feeder Lift Up Timeout",
                "Feeder Lift UP 타임아웃. (에어압/밸브/실린더/센서/간섭 확인)",
                "Error");

            AlarmRegister((int)AlarmKeys.Alarm_FeederLiftDownTimeout,
                "Feeder Lift Down Timeout",
                "Feeder Lift DOWN 타임아웃. (에어압/밸브/실린더/센서/간섭 확인)",
                "Error");

            AlarmRegister((int)AlarmKeys.Alarm_FeederClampTimeout,
                "Feeder Clamp Timeout",
                "Gripper CLAMP 타임아웃. (에어압/밸브/실린더/센서/간섭 확인)",
                "Error");

            AlarmRegister((int)AlarmKeys.Alarm_FeederUnclampTimeout,
                "Feeder Unclamp Timeout",
                "Gripper UNCLAMP 타임아웃. (에어압/밸브/실린더/센서/간섭 확인)",
                "Error");

            // ===== 2040~ 데이터/센서 =====
            AlarmRegister((int)AlarmKeys.Alarm_BinMissingAfterStageToFeeder,
                "Bin Missing After Stage -> Feeder",
                "Stage->Feeder 이송 후 Feeder에서 Bin 존재가 확인되지 않습니다. (센서/클램프/이송 시퀀스 확인)",
                "Error");

            AlarmRegister((int)AlarmKeys.Alarm_BinMissingAfterFeederToCassette,
                "Bin Missing After Feeder -> Cassette",
                "Feeder->Cassette 배출 후 Feeder에 Bin이 남아있거나 불일치 상태입니다. (센서/데이터/간섭 확인)",
                "Error");

            AlarmRegister((int)AlarmKeys.Alarm_BinSensorDataMismatch,
                "Bin Sensor/Data Inconsistency",
                "Bin 센서 상태와 Material 데이터가 불일치합니다. (센서 ON인데 객체 null, 또는 반대)",
                "Error");

            // ===== 2060~ 슬롯/카세트 =====
            AlarmRegister((int)AlarmKeys.Alarm_UnloadTargetSlotInvalid,
                "Unload Target Slot Invalid",
                "언로딩 대상 SlotIndex가 유효하지 않습니다. (Feeder/Stage/Lifter SlotIndex 확인)",
                "Error");

            AlarmRegister((int)AlarmKeys.Alarm_CassetteSlotNotEmptyForUnload,
                "Cassette Slot Not Empty",
                "언로딩 대상 Cassette Slot이 비어있지 않습니다. (Slot 상태 확인 필요)",
                "Error");

            AlarmRegister((int)AlarmKeys.Alarm_CassetteMoveToSlotFailedForUnload,
                "Cassette MoveToSlot Failed",
                "언로딩 대상 Slot으로 Cassette 이동 실패. (축 알람/인터락/Teaching 확인)",
                "Error");

            // ===== 2070~ Feeder->Cassette 상세 =====
            AlarmRegister((int)AlarmKeys.Alarm_UnloadFeederToCassette_MoveFeederToCassettePosFailed,
                "Unload Feeder->Cassette Failed - Move Position Cassette",
                "Feeder->Cassette 배출 중 Cassette Teaching Position 이동 실패.",
                "Error");

            AlarmRegister((int)AlarmKeys.Alarm_UnloadFeederToCassette_UnclampFailed,
                "Unload Feeder->Cassette Failed - Unclamp",
                "Feeder->Cassette 배출 중 Unclamp 실패/타임아웃.",
                "Error");

            AlarmRegister((int)AlarmKeys.Alarm_UnloadFeederToCassette_BinDataInvalid,
                "Unload Feeder->Cassette Failed - Bin Data Invalid",
                "Feeder에 Bin 데이터가 없거나 SlotIndex가 유효하지 않아 Cassette 반영 불가.",
                "Error");

            AlarmRegister((int)AlarmKeys.Alarm_UnloadFeederToCassette_MoveStandbyBarcodeFailed,
                "Unload Feeder->Cassette Failed - Move Standby Barcode",
                "배출 후 Barcode 대기 위치 이동 실패.",
                "Error");

            AlarmRegister((int)AlarmKeys.Alarm_UnloadFeederToCassette_MoveStandbyReadyFailed,
                "Unload Feeder->Cassette Failed - Move Standby Ready",
                "배출 후 Ready 대기 위치 이동 실패.",
                "Error");

            AlarmRegister((int)AlarmKeys.Alarm_ScanBinFailed,
                "Scan Bin Failed",
                "OutputCassetteLifter ScanBin 실패. (Cassette 존재/센서/축 알람/슬롯 상태 확인)",
                "Error");

            AlarmRegister((int)AlarmKeys.Alarm_MoveToReadyFailed,
                "Move To Ready Failed",
                "Ready 대기 위치 이동/상승 동작 실패. (Teaching/축 알람/인터락/실린더 상태 확인)",
                "Error");

            AlarmRegister((int)AlarmKeys.Alarm_MoveToCassetteTeachFailed,
                "Move To Cassette Teaching Failed",
                "Cassette Teaching Position 이동 실패. (Teaching/축 알람/인터락 확인)",
                "Error");

            AlarmRegister((int)AlarmKeys.Alarm_BinCassetteLoadingFailed,
                "Bin Cassette Loading Failed",
                "Cassette에서 Bin 픽업/바코드/이동 시퀀스에 실패했습니다. (상세 로그 확인)",
                "Error");


        }
        #endregion

        #region Unit
        public OutputCassetteLifter OutputCassetteLifter { get; set; }
        public OutputStage OutputStage { get; set; }
        public InputStage InputStage { get; set; }
        public OutputDieTransfer OutputDieTransfer { get; set; }
        public Rotary Rotary { get; set; }
        #endregion

        #region Axis
        private MotionAxis _feederY;
        public MotionAxis AxisOutputFeederY => _feederY;
        #endregion
        // Safety 동작 중 여부
        private bool _isSafetyMoving = false;

        #region IO Domain Members
        private Cylinder _feederLift; // Up/Down
        private Cylinder _cylClamp;   // Clamp / Unclamp


        string strBarcode = string.Empty;

        #endregion

        #region ctor / Initialization
        public OutputFeeder(OutputFeederConfig config = null)
            : base(new OutputFeederConfig())
        {
            
            AddComponents();
        }

        public override void AddComponents()
        {
            Config.LoadAndBindAxes(Equipment.Instance.AxisManager);
            Config.InitializeDefaultTeachingPositions();
            
            BindAxes();
            BindIoDomains();

            Config.IsSimulation = Config.IsSimulation;
            if (Config.IsSimulation)
            {
                AxisOutputFeederY.Config.IsSimulation = true;
                Log.Write(UnitName, "Simulation Mode");
            }
        }

        protected override void OnBindUnit()
        {
            base.OnBindUnit();
            OutputCassetteLifter = Equipment.Instance.GetUnit("OutputCassetteLifter") as OutputCassetteLifter;
            OutputStage = Equipment.Instance.GetUnit("OutputStage") as OutputStage;
            InputStage = Equipment.Instance.GetUnit("InputStage") as InputStage;
            OutputDieTransfer = Equipment.Instance.GetUnit("OutputDieTransfer") as OutputDieTransfer;
            Rotary = Equipment.Instance.GetUnit("Rotary") as Rotary;
        }

        private void BindAxes()
        {
            var mgr = Equipment.Instance?.AxisManager;
            if (mgr == null)
            {
                Log.Write(UnitName, "[BindAxes] AxisManager null");
                return;
            }

            const string unitName = "Unit"; // Equipment에서 축 등록 시 사용한 유닛명과 동일해야 함
            BindAxis(mgr, unitName, AxisNames.BinFeederY, ref _feederY);
        }
        #endregion
        public override bool IsInterlockOK(BaseComponent baseComponent, BaseComponent.InterlockEventArgs e)
        {
            bool bRet = base.IsInterlockOK(baseComponent, e);
            if (baseComponent == this.AxisOutputFeederY)
            {
                if (_isSafetyMoving)
                    return true;

                if (this.IsFeederDown())
                {
                    if (this.OutputStage.IsPositionBinLoading() == false
                       && this.OutputStage.IsPositionBinUnloading() == false)
                    {
                        this.AxisOutputFeederY?.EmgStop();
                        PostAlarm((int)AlarmKeys.Alarm_OutputStageInterlockFailed);
                        bRet = false;
                    }
                    else
                    {
                        //20251211 - 여기 간헐적으로 인터락 걸림. 
                        // 조건 확인 필요
                        if (this.IsPositionCassette())
                        {
                            //bRet = IsInterlockOKWithCassette(e);
                            //if (bRet == false)
                            //{
                            //    this.AxisOutputFeederY?.EmgStop();
                            //    PostAlarm((int)AlarmKeys.Alarm_OutputStageInterlockFailed);
                            //    return bRet;
                            //}
                        }
                    }
                }
            }
            else if (baseComponent == this._feederLift)
            {
                if (this.IsPositionCassette())
                {
                    PostAlarm((int)AlarmKeys.Alarm_OutputStageInterlockFailed);
                    bRet = false;
                }
            }
            return bRet;
        }
        private bool IsInterlockOKWithCassette(BaseComponent.InterlockEventArgs e)
        {
            // 이거 맞는 인터락이야? 이거 왜 하는거지??
            // 우선 막자. 이해가 안됨.
            // 아래.. 조건은 카세트가 움직이기전에 피더 상태를 확인하는 인터락 같음.
            if (this.OutputStage.IsPlateUp() || this.OutputStage.IsClampLiftUp())
            {
                double dCurrentY = this.AxisOutputFeederY.GetPosition();
                double dStageY = this.GetTP(OutputFeederConfig.TeachingPositionName.Cassette.ToString(), this.AxisOutputFeederY.Name);
                if (dCurrentY > dStageY + this.AxisOutputFeederY.Config.InposTolerance)
                {
                    return false;
                }
            }
            return true;
        }
        private bool IsInterlockOKBinLoading()
        {
            bool bRtn = true;
            if (OutputCassetteLifter.IsBinReadyForLoading() == false)
            {
                Log.Write(this, "OutputCassetteLifter Not Ready for Loading");
                bRtn = false;
                return bRtn;
            }

            if(OutputStage.IsPositionBinLoading() == false)
            {
                if (!OutputStage.IsStageInterLockOK())
                {
                    Log.Write(this, "OutputStage Not Ready for Loading");
                    bRtn = false;
                    return bRtn;
                }
            }

            return bRtn;
        }
        private bool IsInterlockOKMoveToCassette()
        {
            bool isOK = this.OutputStage.IsPositionBinLoading();
            isOK &= this.OutputCassetteLifter.IsBinReadyForLoading();
            return isOK;
        }
        private bool IsInterlockOKWaferLoading()
        {
            bool bRtn = true;
            if (OutputCassetteLifter.IsBinReadyForLoading() == false)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_IsBinReadyForLoading);
                Log.Write(this, "OutputCassetteLifter Not Ready for Loading");
                bRtn = false;
                return bRtn;
            }

            if (OutputStage.IsPositionBinLoading() == false)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_BinLoadingPosition);
                Log.Write(this, "OutputStage Not Ready for Loading");
                bRtn = false;
                return bRtn;
            }
            return bRtn;
        }
        private bool IsInterlockOKWaferUnloading()
        {
            // Stage가 BinUnloading 안전 위치면 Cassette로 이동 허용
            bool stageSafe = OutputStage != null && OutputStage.IsPositionBinUnloading();
            bool cassetteReady = OutputCassetteLifter != null && OutputCassetteLifter.IsCassettePresentAll(); // 필요 시 Ready 신호 사용
            return stageSafe && cassetteReady;
        }

        #region Wafer Missing / Consistency Helpers
        // Stage 언로드 시작 전 Stage 센서 vs 객체 검증
        private int CheckStageWaferBeforeUnload(MaterialWafer BinOnStage)
        {
            // Stage 센서 ON인데 객체 null -> 데이터 유실
            if (OutputStage.IsRingPresent())
            {
                if (BinOnStage == null)
                {
                    Log.Write(UnitName, "[Unload] Stage ring detected but wafer object null");
                    PostAlarm((int)AlarmKeys.Alarm_OutputFeederBinData);
                    return -1;
                }
                return 0;
            }
            // 객체 존재 + 센서 OFF -> 불일치
            if (BinOnStage != null && OutputStage.IsRingPresent() == false)
            {
                Log.Write(UnitName, "[Unload] Wafer object exists but stage sensor off");
                PostAlarm((int)AlarmKeys.Alarm_OutputFeederBinData);
                return -1;
            }
            return 0;
        }

        // Stage → Feeder 이동 후 피더 상태 검증
        private int VerifyWaferMovedStageToFeeder(MaterialWafer waferMoved)
        {
            bool feederSensor = IsRingPresent();
            var feederObj = GetMaterial() as MaterialWafer;

            // 기대: Stage->Feeder 후에는 Feeder에 존재해야 함
            if (!feederSensor && feederObj == null)
            {
                Log.Write(UnitName, "[Unload] Bin missing on feeder after transfer");
                PostAlarm((int)AlarmKeys.Alarm_BinMissingAfterStageToFeeder);   // 2040
                return -1;
            }

            if (feederSensor && feederObj == null)
            {
                Log.Write(UnitName, "[Unload] Feeder ring detected but object null");
                PostAlarm((int)AlarmKeys.Alarm_BinSensorDataMismatch);          // 2042
                return -1;
            }

            // 실기에서만 센서 불일치 엄격 적용(시뮬/드라이런은 GetMaterial 기반)
            if (feederObj != null && !feederSensor && !(Config.IsSimulation || Config.IsDryRun))
            {
                Log.Write(UnitName, "[Unload] Feeder object exists but feeder sensor off");
                PostAlarm((int)AlarmKeys.Alarm_BinSensorDataMismatch);          // 2042
                return -1;
            }

            // SlotIndex mismatch는 데이터 오류(2026)로 보고 싶으면 올릴 수 있으나,
            // 현장 영향(동작 중복 알람) 우려가 있어 로그만 유지
            if (feederObj != null && waferMoved != null &&
                feederObj.SlotIndex != waferMoved.SlotIndex && waferMoved.SlotIndex >= 0)
            {
                Log.Write(UnitName, $"[Unload] SlotIndex mismatch Stage:{waferMoved.SlotIndex} Feeder:{feederObj.SlotIndex}");
            }

            return 0;
        }

        // Cassette로 최종 언로드 후 Feeder 상태 점검
        private int VerifyAfterUnloadToCassette(int slotIndex)
        {
            bool feederSensor = IsRingPresent();
            var feederObj = GetMaterial() as MaterialWafer;

            // 정상: 센서 OFF + 객체 null
            if (!feederSensor && feederObj == null)
            {
                Log.Write(UnitName, $"[Unload] Completed feeder empty OK (Slot:{slotIndex})");
                return 0;
            }

            // 센서 OFF인데 객체만 남음 -> 데이터만 정리하고 정상처리
            if (!feederSensor && feederObj != null)
            {
                Log.Write(UnitName, "[Unload] Object remained although sensor off -> force clear");
                SetMaterial(null);
                return 0;
            }

            // 배출 완료인데 센서 ON + 객체 남음 = 실제로 남아있음
            if (feederSensor && feederObj != null)
            {
                Log.Write(UnitName, "[Unload] Feeder still holds bin after unload-to-cassette step");
                PostAlarm((int)AlarmKeys.Alarm_BinMissingAfterFeederToCassette); // 2041
                return -1;
            }

            // 센서 ON인데 객체 null은 순간 구간일 수 있어 기존 코드처럼 알람 금지 유지
            return 0;
        }


        // === 모든 Cassette 투입 소진 시 언로딩/정지 처리 ===
        private void TryShutdownIfAllCassettesEmpty()
        {
            try
            {
                var inLifter = Equipment.Instance.GetUnit("InputCassetteLifter") as InputCassetteLifter;
                bool noInput = (inLifter == null) || !inLifter.IsHaveMoreProcessWafer();
                bool noOutput = (OutputCassetteLifter == null) || !OutputCassetteLifter.IsHaveMoreProcessWafer();

                // 둘 다 더 이상 투입할 것이 없을 때만 동작
                if (!noInput || !noOutput)
                    return;

                Log.Write(UnitName, "TryShutdownIfAllCassettesEmpty", "Input/Output Cassette 모두 더 이상 로딩할 Wafer 없음 → 언로딩 및 장비 정지 진행.");

                // Ready 복귀
                int readyRc = EnsureReady();
                if (readyRc != 0)
                    Log.Write(UnitName, "TryShutdownIfAllCassettesEmpty", $"EnsureReady 실패 rc={readyRc}");

                // Cassette 교체 알람 (양쪽 모두)
                try
                {
                    OutputCassetteLifter?.PostAlarm((int)OutputCassetteLifter.AlarmKeys.eCassetteChangeRequired);
                }
                catch { }
                try
                {
                    inLifter?.PostAlarm((int)InputCassetteLifter.AlarmKeys.eCassetteChangeRequired);
                }
                catch { }

                // Unit 정지 (필요한 Unit만)
                try { OutputStage?.Stop(); } catch { }
                try { OutputCassetteLifter?.Stop(); } catch { }
                try { inLifter?.Stop(); } catch { }
                try { this.Stop(); } catch { }

                var eq = Equipment.Instance;
                var state = eq?.EqState ?? EquipmentState.Unknown;
                eq.StopAllUnitsAsync();

                Log.Write(UnitName, "TryShutdownIfAllCassettesEmpty", "모든 관련 Unit 정지 완료.");
            }
            catch (Exception ex)
            {
                Log.Write(UnitName, "TryShutdownIfAllCassettesEmpty", "예외: " + ex.Message);
            }
        }
        #endregion

        public int MovePositionReady(bool isFine = false)
        {
            Task<int> task = MovePositionAsyncReady(isFine);
            while (IsEndTask(task) == false)
            {
                // [변경] Ready 이동에는 BinLoading 인터락을 강제하지 않음
                if (IsMoveInterLockReady() != 0)
                {
                    AxisOutputFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_OutputStageInterlockFailed);
                    return -1;
                }
                Thread.Sleep(1);

                //if(RunMode == UnitRunMode.Auto)
                //{
                //    if (IsInterlockOKBinLoading() == false)
                //    {
                //        AxisOutputFeederY.EmgStop();
                //        PostAlarm((int)AlarmKeys.Alarm_BinLoadingFailed);
                //        return -1;
                //    }
                //}
                //IsMoveInterLockReady();
                //Thread.Sleep(1);
            }
            return task.Result;
        }
        public Task<int> MovePositionAsyncReady(bool isFine = false)
        {
            return Task.Run(() =>
            {
                OnMovePositionReady(isFine);
                return 0;
            });
        }
        private int OnMovePositionReady(bool isFine = false)
        {
            _isSafetyMoving = true;
            try
            {
                return MoveTeachingPositionOnce((int)OutputFeederConfig.TeachingPositionName.Ready, isFine);
            }
            finally
            {
                _isSafetyMoving = false;
            }
        }
        private int IsMoveInterLockReady()
        {
            int nRet = 0;
            
            if (OutputStage?.IsAnyAxisMoving()== true)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_OutputStageInterlockFailed);
                nRet = -1;
                return nRet;
            }

            return nRet;
        }

        private int IsMoveInterLockStage()
        {
            int nRet = 0;
            if (OutputStage?.IsAnyAxisMoving() == true)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_OutputStageInterlockFailed);
                nRet = -1;
                return nRet;
            }

            return nRet;
        }

        private int IsMoveInterLockBarcode()
        {
            int nRet = 0;
            if (OutputStage?.IsAnyAxisMoving() == true)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_OutputStageInterlockFailed);
                nRet = -1;
                return nRet;
            }

            if (OutputCassetteLifter?.IsAnyAxisMoving() == true)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_OutputStageInterlockFailed);
                nRet = -1;
                return nRet;
            }

            return nRet;
        }

        private bool IsMoveInterLockCassette()
        {
            bool bRet = true;
            if (OutputStage?.IsAnyAxisMoving() == true)
            {
                Log.Write(UnitName, "IsMoveInterLockCassette", "OutputStage is moving");
                bRet = false;
                return bRet;
            }

            if (OutputCassetteLifter?.IsAnyAxisMoving() == true)
            {
                Log.Write(UnitName, "IsMoveInterLockCassette", "OutputCassetteLifter is moving");
                bRet = false;
                return bRet;
            }

            bool stageAtSafe = (OutputStage != null) &&
                       (OutputStage.IsPositionBinLoading() || OutputStage.IsPositionBinUnloading());
            if (stageAtSafe == false)
            {
                Log.Write(UnitName, "IsMoveInterLockCassette", "OutputStage not at safe position for moving to Cassette");
                return false;
            }

            return bRet;
        }


        public int MovePositionStage(bool isFine = false)
        {
            Task<int> task = MovePositionAsyncStage(isFine);
            while (IsEndTask(task) == false)
            {
                IsMoveInterLockStage();
                Thread.Sleep(1);
            }
            return task.Result;
        }
        public Task<int> MovePositionAsyncStage(bool isFine = false)
        {
            return Task.Run(() =>
            {
                OnMovePositionStage(isFine);
                return 0;
            });
        }
        private int OnMovePositionStage(bool isFine = false)
        {
            return MoveTeachingPositionOnce((int)OutputFeederConfig.TeachingPositionName.Stage, isFine);
        }
        
        public int MovePositionBarcode(bool isFine = false)
        {
            Task<int> task = MovePositionAsyncBarcode(isFine);
            while (IsEndTask(task) == false)
            {
                IsMoveInterLockBarcode();
                Thread.Sleep(1);
            }
            return task.Result;
        }
        public Task<int> MovePositionAsyncBarcode(bool isFine = false)
        {
            return Task.Run(() =>
            {
                OnMovePositionBarcode(isFine);
                return 0;
            });
        }
        private int OnMovePositionBarcode(bool isFine = false)
        {
            return MoveTeachingPositionOnce((int)OutputFeederConfig.TeachingPositionName.Barcode, isFine);
        }
        
        public int MovePositionCassette(bool isFine = false)
        {
            Task<int> task = MovePositionAsyncCassette(isFine);
            while (IsEndTask(task) == false)
            {
                // [변경] 로딩/언로딩 경로별 인터락 분기
                bool ok = NeedUnloadFirst ? IsInterlockOKWaferUnloading() : IsInterlockOKWaferLoading();
                if (ok == false)
                {
                    AxisOutputFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_OutputFeederInterlockFailed);
                    return -1;
                }

                if (IsMoveInterLockCassette() == false)
                {
                    AxisOutputFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_OutputFeederInterlockFailed);
                    return -1;
                }

                Thread.Sleep(1);
            }
            return task.Result;
        }
        public Task<int> MovePositionAsyncCassette(bool isFine = false)
        {
            return Task.Run(() =>
            {
                OnMovePositionCassette(isFine);
                return 0;
            });
        }
        private int OnMovePositionCassette(bool isFine = false)
        {
            _isSafetyMoving = true;
            try
            {
                bool ok = NeedUnloadFirst ? IsInterlockOKWaferUnloading() : IsInterlockOKWaferLoading();
                if (!ok)
                {
                    AxisOutputFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_OutputFeederInterlockFailed);
                    Log.Write(UnitName, "OnMovePositionCassette", "Interlock failed");
                    return -1;
                }

                return base.MoveTeachingPositionOnce((int)OutputFeederConfig.TeachingPositionName.Cassette, isFine);
            }
            finally
            {
                _isSafetyMoving = false;
            }
        }
        
        public bool IsFeederZSafetyPosition()
        {
            bool bRtn = false;

            if (_feederLift == null)
                return bRtn;

            if (this.Config.IsSimulation)
            {
                return true;
            }
            if (IsFeederUp())
                return true;

            if (IsFeederDown())
                return false;

            // 전이 상태(Up/Down 모두 OFF) → 안전 아님으로 판단
            return bRtn;
        }
        public bool IsFeederYSafetyPosition()
        {
            bool bRtn = false;
            if (AxisOutputFeederY == null)
                return bRtn;

            var cfg = Config;
            if (cfg == null)
                return bRtn;

            bRtn = IsPositionReady();
            return bRtn;
        }

        public bool IsPositionSetPos()
        {
            var tp = TeachingPositions[(int)OutputFeederConfig.TeachingPositionName.SetPosition];
            if (tp == null)
                return false;
            return InPosTeaching(tp);
        }

        public bool IsPositionReady()
        {
            var tp = TeachingPositions[(int)OutputFeederConfig.TeachingPositionName.Ready];
            if (tp == null)
                return false;
            return InPosTeaching(tp);
        }
        public bool IsPositionStage()
        {
            var tp = TeachingPositions[(int)OutputFeederConfig.TeachingPositionName.Stage];
            if (tp == null)
                return false;
            return InPosTeaching(tp);
        }
        public bool IsPositionBarcode()
        {
            var tp = TeachingPositions[(int)OutputFeederConfig.TeachingPositionName.Barcode];
            if (tp == null)
                return false;
            return InPosTeaching(tp);
        }
        public bool IsPositionCassette()
        {
            var tp = TeachingPositions[(int)OutputFeederConfig.TeachingPositionName.Cassette];
            if (tp == null)
                return false;
            return InPosTeaching(tp);
        }

        #region Teaching Helpers
        public void TeachCurrentPosition(string positionName, string description = null)
        {
            var axisPositions = new Dictionary<string, double>();
            foreach (var axisPair in Axes)
                axisPositions[axisPair.Key] = axisPair.Value.GetPosition();
            var tp = new TeachingPosition(positionName, axisPositions, description);
            Config.SetTeachingPosition(tp);
        }
        public int MoveToTeachingPosition(string positionName, double vel = 5, double acc = 10, double dec = 10, double jerk = 50)
        {
            var tp = Config.GetTeachingPosition(positionName);
            if (tp == null) return -1;
            int result = 0;
            foreach (var axisKey in tp.AxisPositions.Keys)
            {
                if (Axes.TryGetValue(axisKey, out var axis))
                {
                    double pos = tp.AxisPositions[axisKey];
                    int r = axis.MoveAbs(pos, vel, acc, dec, jerk);
                    if (r != 0) result = r;
                }
            }
            return result;
        }
        #endregion

        #region IO Domain Mapping
        private void BindIoDomains()
        {
            var eq = Equipment.Instance; var unit = eq?.UnitIO; if (unit == null) return;
            if (!IoAutoBindings.Cylinders.TryGetValue("OutFeederLift", out _feederLift))
            {
                Log.Write("OutputFeeder", "BindIoDomains", "Cylinder not found: OutFeederLift");
            }

            if (!IoAutoBindings.Cylinders.TryGetValue("OutFeederClamp", out _cylClamp))
            {
                Log.Write("OutputFeeder", "BindIoDomains", "Cylinder not found: OutFeederClamp");
            }
        }
        #endregion

        // === Domain Control (표준 구동) ===
        public bool SetLift(bool bUpDn)
        {
            if (_feederLift == null) 
                return false;
            if (bUpDn) 
                return _feederLift.Extend();
            else 
                return _feederLift.Retract();
        }
        public bool SetClamp(bool bUpDn)
        {
            if (_cylClamp == null) 
                return false;
            if (bUpDn) 
                return _cylClamp.Extend();
            else 
                return _cylClamp.Retract();
        }
        #region Status Helpers

        private bool _simFeederUp = true; // 시뮬 초기 Up로 가정
        public bool IsFeederUp()
        {
            if(Config.IsSimulation)
            {
                return _simFeederUp;
            }
            return this.ReadInput(OutputFeederConfig.IO.FEEDER_UP);
        }
        public bool IsFeederDown()
        {
            if (Config.IsSimulation)
            {
                return !_simFeederUp;
            }
            return this.ReadInput(OutputFeederConfig.IO.FEEDER_DOWN);
        }
        public bool IsClamped()
        {
            bool bRtn = false;
            if (Config.IsSimulation)
            {
                bRtn = true;
                return true;
            }
            bRtn = !this.ReadInput(OutputFeederConfig.IO.FEEDER_UNCLAMP);
            return bRtn;
        }
        public bool IsUnClamped()
        {
            if (Config.IsSimulation)
            {
                return true;
            }
            return this.ReadInput(OutputFeederConfig.IO.FEEDER_UNCLAMP);
        }
        public bool IsRingPresent()
        {
            if (Config.IsSimulation || Config.IsDryRun)
            {
                // 시뮬레이션: 실제 보유 머티리얼로 판단
                return this.GetMaterial() is MaterialWafer;
                //return true;
            }
            return this.ReadInput(OutputFeederConfig.IO.FEEDER_RING_CHECK);
        }
        public bool IsOverload()
        {
            if (Config.IsSimulation || Config.IsDryRun)
            {
                return true;
            }
            return this.ReadInput(OutputFeederConfig.IO.FEEDER_OVERLOAD);
        }

        // === Cylinder 완료 대기 Helpers ===
        private int WaitClampStateOrAlarm(bool expectClamp, int timeoutMs = 1500, int pollMs = 2)
        {
            if (Config.IsSimulation || Config.IsDryRun)
                return 0;

            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds <= timeoutMs)
            {
                bool ok = expectClamp ? IsClamped() : IsUnClamped();
                if (ok)
                    return 0;

                Thread.Sleep(pollMs);
            }

            int alarm = expectClamp
                        ? (int)AlarmKeys.Alarm_FeederClampTimeout
                        : (int)AlarmKeys.Alarm_FeederUnclampTimeout;

            PostAlarm(alarm);
            Log.Write(UnitName, expectClamp ? "[Clamp] Gripper CLAMP timeout" : "[Clamp] Gripper UNCLAMP timeout");
            return -1;
        }

        // Lift: expectUp=true(UP 기대), false(DOWN 기대)
        private int WaitLiftStateOrAlarm(bool expectUp, int timeoutMs = 1500, int pollMs = 2)
        {
            if (Config.IsSimulation || Config.IsDryRun)
                return 0;

            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds <= timeoutMs)
            {
                bool ok = expectUp ? IsFeederUp() : IsFeederDown();
                if (ok)
                    return 0;

                Thread.Sleep(pollMs);
            }

            int alarm = expectUp
                        ? (int)AlarmKeys.Alarm_FeederLiftUpTimeout
                        : (int)AlarmKeys.Alarm_FeederLiftDownTimeout;

            PostAlarm(alarm);
            Log.Write(UnitName, expectUp ? "[Lift] Feeder UP timeout" : "[Lift] Feeder DOWN timeout");
            return -1;
        }

        #endregion

        /// ////////////////////////////////////////////////////////////////////////////////////////
        #region === Direct Valve Control (입력 신호/인터락 무관 강제 구동용) ===
        public bool IsFeederUpValveOn() => this.IsOutputOn(OutputFeederConfig.IO.FEEDER_UP_VALVE);
        public bool IsFeederDownValveOn() => this.IsOutputOn(OutputFeederConfig.IO.FEEDER_DOWN_VALVE);
        public bool IsFeederClampValveOn() => this.IsOutputOn(OutputFeederConfig.IO.FEEDER_CLAMP_VALVE);
        public bool IsFeederUnclampValveOn() => this.IsOutputOn(OutputFeederConfig.IO.FEEDER_UNCLAMP_VALVE);
        #endregion

        #region DryRunTest 변수
        bool UnitDryRunTest { get; set; } = false;
        // DryRun 반복 제어용 최소 상태(토글)
        private bool _dryLoadedToStage = false;   // 마지막 사이클에서 Stage에 로딩했는지 여부
        private int _dryLastSlotIndex = -1;       // 마지막으로 픽업한 Slot (언로딩 대상)
        #endregion

        #region Signals
        bool NeedUnloadFirst { get; set; } = false;
        // 언로드 직후 다음 로딩을 바코드에서 시작하도록 하는 1회성 플래그
        private volatile bool _exchangeStandbyForNextLoad = false;
        #endregion

        // 클래스 필드 영역 아무 곳(예: Signals 바로 위/아래)에 추가
        private enum LoadFlowStep 
        { 
            None = 0, 
            Step01 = 1, 
            Step02 = 2, 
            Step03 = 3, 
            Step04 = 4, 
            Completed = 5 
        }
        private LoadFlowStep _loadStep = LoadFlowStep.None;

        private LoadFlowStep DetermineNextLoadStep()
        {
            // 이미 진행 중인 단계가 있으면 그대로 재개
            if (_loadStep == LoadFlowStep.Step01 ||
                _loadStep == LoadFlowStep.Step02 ||
                _loadStep == LoadFlowStep.Step03 ||
                _loadStep == LoadFlowStep.Step04)
                return _loadStep;

            bool feederHasWafer = GetMaterial() is MaterialWafer;
            bool hasMore = OutputCassetteLifter?.IsHaveMoreProcessWafer() == true;

            if (feederHasWafer) 
                return LoadFlowStep.Step03;   // Feeder 보유 → Stage 로딩부터

            if (hasMore) 
                return LoadFlowStep.Step01;   // 카세트 잔여 → 1단계부터

            return LoadFlowStep.Completed;
        }

        private void AdvanceLoadStepOnSuccess(LoadFlowStep done)
        {
            switch (done)
            {
                case LoadFlowStep.Step01: 
                    _loadStep = LoadFlowStep.Step02; 
                    break;
                case LoadFlowStep.Step02: 
                    _loadStep = LoadFlowStep.Step03; 
                    break;
                case LoadFlowStep.Step03: 
                    _loadStep = LoadFlowStep.Step04;
                    break;
                case LoadFlowStep.Step04: 
                    _loadStep = LoadFlowStep.Completed;
                    break;
            }
        }

        private void MarkStepOnFailure(LoadFlowStep failed)
        {
            _loadStep = failed; // 실패 단계부터 재시도
        }

        // [추가] 로딩 단계 상태와 동일하게 언로딩 단계 상태 관리
        private enum UnloadFlowStep 
        { 
            None = 0, Step01 = 1, 
            Step02 = 2, 
            Step03 = 3, 
            Completed = 4 
        }
        private UnloadFlowStep _unloadStep = UnloadFlowStep.None;
        private int _unloadTargetSlot = -1;

        // 언로딩 대상 슬롯 산출 헬퍼
        private int ComputeUnloadTargetSlot()
        {
            var waferFromFeeder = this.GetMaterial() as MaterialWafer;
            int slotFromFeeder = (waferFromFeeder != null) ? waferFromFeeder.SlotIndex : -1;
            int lifterSlot = this.OutputCassetteLifter?.GetCurrectSlotID() ?? -1;
            if (slotFromFeeder >= 0) 
                return slotFromFeeder;

            if (lifterSlot >= 0) 
                return lifterSlot;

            if (_dryLastSlotIndex >= 0) 
                return _dryLastSlotIndex;

            return -1;
        }

        // 현재 설비 상태로 다음 언로딩 단계 결정
        private UnloadFlowStep DetermineNextUnloadStep()
        {
            // 스테이지에 링 존재 → 언로딩 준비부터
            if (NeedUnloadFirst || (OutputStage?.IsRingPresent() == true))
            {
                if (_unloadStep == UnloadFlowStep.Step01 ||
                    _unloadStep == UnloadFlowStep.Step02 ||
                    _unloadStep == UnloadFlowStep.Step03)
                    return _unloadStep;

                return UnloadFlowStep.Step01;
            }

            // 스테이지는 비었고 피더에 웨이퍼가 남아있다면 Cassette로 최종 언로드 단계
            if (GetMaterial() is MaterialWafer)
            {
                if (_unloadStep == UnloadFlowStep.Step01 ||
                    _unloadStep == UnloadFlowStep.Step02 ||
                    _unloadStep == UnloadFlowStep.Step03)
                    return _unloadStep;

                // 슬롯 다시 산정
                _unloadTargetSlot = ComputeUnloadTargetSlot();
                return (_unloadTargetSlot >= 0) ? UnloadFlowStep.Step03 : UnloadFlowStep.Completed;
            }

            return UnloadFlowStep.Completed;
        }

        private void AdvanceUnloadStepOnSuccess(UnloadFlowStep done)
        {
            switch (done)
            {
                case UnloadFlowStep.Step01: _unloadStep = UnloadFlowStep.Step02; break;
                case UnloadFlowStep.Step02: _unloadStep = UnloadFlowStep.Step03; break;
                case UnloadFlowStep.Step03: _unloadStep = UnloadFlowStep.Completed; break;
            }
        }

        private void MarkUnloadStepOnFailure(UnloadFlowStep failed)
        {
            _unloadStep = failed; // 실패한 단계부터 재시도
        }

        #region Lifecycle
        public override int OnRun()
        {
            int ret = 0;
            if (this.RunUnitStatus == UnitStatus.Stopped ||
               this.RunUnitStatus == UnitStatus.Stopping ||
               this.RunUnitStatus == UnitStatus.CycleStop ||
               this.RunUnitStatus == UnitStatus.ManualRunning)
            {
                this.State = ProcessState.Stop;
                return 0;
            }

            switch (State)
            {
                case ProcessState.Ready:
                    ret = OnRunReady();
                    break;
                case ProcessState.Work:
                    ret = OnRunWork();
                    break;
                case ProcessState.Complete:
                    ret = OnRunComplete();
                    break;
                default:
                    this.State = ProcessState.Ready;
                    break;
            }

            if (ret != 0)
            {
                this.State = ProcessState.Stop;
                this.OnStop();
            }
            return ret;
        }
        protected override int OnStart()
        {
            return base.OnStart();
        }
        public override int OnStop()
        {
            int ret = 0;
            this.RunUnitStatus = UnitStatus.Stopped;
            base.OnStop();
            return ret;
        }
        protected override int OnRunReady()
        {
            int nRet = 0;
            MaterialWafer BinStage = this.OutputStage.GetMaterialWafer();
            try
            {
                // Stage Wafer 작업 중일때 true임.
                if (this.OutputStage.IsWorking() == true)
                {
                    if (BinStage != null)
                    {
                        if (OutputStage.IsPositionBinLoading() == false &&
                            OutputStage.IsPositionBinUnloading() == false &&
                            OutputStage.IsPositionBinCenter() == true &&
                            BinStage.ProcessSatate == Material.MaterialProcessSatate.Ready)
                        {
                            nRet = SetMappingData();
                        }
                    }
                    return nRet;
                }
                else if (this.OutputStage.IsWorking() == false)
                {
                    bool sim = (Config.IsSimulation || Config.IsDryRun);
                    if (sim == false)
                    {
                        if (BinStage != null && BinStage.SlotIndex != -1)
                        {
                            // 실기: 센서 기반 존재 판단
                            if (OutputStage.IsRingPresent() &&
                               BinStage.ProcessSatate == Material.MaterialProcessSatate.Completed &&
                               OutputStage.IsPositionBinLoading() == false &&
                               OutputStage.IsPositionBinUnloading() == false)
                            {
                                NeedUnloadFirst = true;
                            }
                            else if(OutputStage.IsRingPresent() == false)
                            {
                                NeedUnloadFirst = false;
                            }
                            else
                            {
                                // 그 외의 경우는 대기 // 무언정지라도 확인하고 처리하자.
                                return nRet;
                            }
                        }
                        else
                        {
                            if (OutputStage.IsRingPresent() == false)
                                NeedUnloadFirst = false;
                            else
                                return nRet;
                        }
                    }
                    else
                    {
                        // 시뮬/드라이런: 데이터 기반 판단
                        NeedUnloadFirst = (BinStage != null && BinStage.SlotIndex != -1);
                    }

                    // 언로딩 우선이면 언로딩 단계부터, 아니면 로딩 단계부터
                    // 스테이지에 제품이 있을때 로딩 일 수도 있잖아.
                    if (NeedUnloadFirst == true && OutputStage.IsRingPresent())
                    {
                        _unloadStep = DetermineNextUnloadStep();
                    }
                    else
                    {
                        _loadStep = DetermineNextLoadStep();
                    }

                    Log.Write(UnitName, "OnRunReady", "ProcessState.Work Start");
                    this.State = ProcessState.Work;
                }
                else
                {
                    // 그 외의 경우는 대기
                    return nRet;
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return nRet;
            }

            return nRet;
        }

        protected override int OnRunWork()
        {
            int nRet = 0;

            MaterialWafer wafer = this.OutputStage.GetMaterialWafer();

            // 0) Stage에 제품이 있으면 "언로딩 먼저"
            if (NeedUnloadFirst)
            {
                // 재개 시작 단계 보정
                if (_unloadStep == UnloadFlowStep.None || _unloadStep == UnloadFlowStep.Completed)
                    _unloadStep = DetermineNextUnloadStep();

                switch (_unloadStep)
                {
                    case UnloadFlowStep.Step01:
                        nRet = BinUnloading_Step01(true);

                        // [ADD] 대기(Blocked)면 Step01 성공처리/Step02 진행 금지. 다음 싸이클에 다시 Step01.
                        if (nRet == 1)
                            return 0;

                        if (nRet != 0) { MarkUnloadStepOnFailure(UnloadFlowStep.Step01); return nRet; }
                        if (IsStop) { MarkUnloadStepOnFailure(UnloadFlowStep.Step01); return 0; }
                        AdvanceUnloadStepOnSuccess(UnloadFlowStep.Step01);
                        goto case UnloadFlowStep.Step02;

                    case UnloadFlowStep.Step02:
                        nRet = BinUnloading_Step02(true);
                        if (nRet != 0) { MarkUnloadStepOnFailure(UnloadFlowStep.Step02); return nRet; }
                        if (IsStop) { MarkUnloadStepOnFailure(UnloadFlowStep.Step02); return 0; }

                        if (_unloadTargetSlot < 0)
                            _unloadTargetSlot = ComputeUnloadTargetSlot();

                        if (_unloadTargetSlot < 0)
                        {
                            AxisOutputFeederY.EmgStop();
                            PostAlarm((int)AlarmKeys.Alarm_UnloadTargetSlotInvalid); // 2060 (기존 2004 수정)
                            this.State = ProcessState.Error;
                            Log.Write(UnitName, "OnRunWork", "Unload target slot invalid");
                            return -1;
                        }

                        AdvanceUnloadStepOnSuccess(UnloadFlowStep.Step02);
                        goto case UnloadFlowStep.Step03;

                    case UnloadFlowStep.Step03:
                        nRet = this.OutputCassetteLifter.MoveToSlot(_unloadTargetSlot);
                        if (nRet != 0)
                        {
                            AxisOutputFeederY.EmgStop();
                            PostAlarm((int)AlarmKeys.Alarm_CassetteMoveToSlotFailedForUnload); // 2062 (기존 2004 수정)
                            this.State = ProcessState.Error;
                            Log.Write(UnitName, "OnRunWork", "OutputCassetteLifter.MoveToSlot Failed");
                            MarkUnloadStepOnFailure(UnloadFlowStep.Step03);
                            return nRet;
                        }

                        nRet = UnloadOnlyFeederToCassette(true);
                        if (nRet != 0)
                        {
                            AxisOutputFeederY.EmgStop();
                            // 여기서 2004로 덮지 않음:
                            // UnloadOnlyFeederToCassette 내부가 2070~2074/2041을 이미 올림
                            this.State = ProcessState.Error;
                            MarkUnloadStepOnFailure(UnloadFlowStep.Step03);
                            return nRet;
                        }
                        if (IsStop) { MarkUnloadStepOnFailure(UnloadFlowStep.Step03); return 0; }
                        AdvanceUnloadStepOnSuccess(UnloadFlowStep.Step03);
                        break;

                    case UnloadFlowStep.Completed:
                    default:
                        break;
                }

                _unloadStep = UnloadFlowStep.Completed;
                _unloadTargetSlot = -1;
                NeedUnloadFirst = false;
                this.State = ProcessState.Complete;
                return 0;

                //nRet = BinUnloading(true);
                //if (nRet != 0)
                //{
                //    Log.Write(UnitName, "OnRunWork", "BinUnloading Failed");
                //    return nRet;
                //}
                //NeedUnloadFirst = false;
                //if(IsStop)
                //{
                //    Log.Write(UnitName, "OnRunWork", "IsStop-BinUnloading");
                //    return 0;
                //}
                //NeedUnloadFirst = false;
                //this.State = ProcessState.Complete;
                //return 0;
            }
            else
            {
                // 1) Feeder -> Cassette: Scan
                if (this.OutputCassetteLifter.IsScanCompleted() == false)
                {
                    nRet = this.OutputCassetteLifter.ScanBin(true);
                    if (nRet != 0)
                    {
                        AxisOutputFeederY.EmgStop();
                        PostAlarm((int)AlarmKeys.Alarm_ScanBinFailed); // [FIX] 2000 -> 2080
                        return nRet;
                    }
                    if (IsStop)
                    {
                        Log.Write(UnitName, "OnRunWork", "IsScanCompleted");
                        return 0;
                    }
                }

                bool hasMore = this.OutputCassetteLifter.IsHaveMoreProcessWafer();
                bool feederHasWafer = GetMaterial() is MaterialWafer;

                // 2) 더 진행할 것이 전혀 없으면 Ready 복귀
                if (!hasMore && !feederHasWafer)
                {
                    if (!IsPositionReady())
                    {
                        nRet = MoveToReady();
                        if (nRet != 0)
                        {
                            AxisOutputFeederY.EmgStop();
                            PostAlarm((int)AlarmKeys.Alarm_MoveToReadyFailed); // [FIX] 2000 -> 2081
                            this.State = ProcessState.Error;
                            return nRet;
                        }
                    }

                    TryShutdownIfAllCassettesEmpty();
                    this.State = ProcessState.Ready;
                    return 0;
                }

                // 3) 재개 시작 단계 보정
                if (_loadStep == LoadFlowStep.None || _loadStep == LoadFlowStep.Completed)
                {
                    _loadStep = DetermineNextLoadStep();
                }

                switch (_loadStep)
                {
                    case LoadFlowStep.Step01:
                        nRet = BinLoading_Step01(true);
                        if (nRet != 0)
                        {
                            AxisOutputFeederY.EmgStop();
                            // Step01 내부에서 이미 OutputStageInterlockFailed(2010) 등을 올림.
                            // 여기서는 2000으로 덮지 않도록 제거.
                            this.State = ProcessState.Error;
                            MarkStepOnFailure(LoadFlowStep.Step01);
                            return nRet;
                        }
                        if (IsStop) { MarkStepOnFailure(LoadFlowStep.Step01); return 0; }
                        AdvanceLoadStepOnSuccess(LoadFlowStep.Step01);
                        goto case LoadFlowStep.Step02;

                    case LoadFlowStep.Step02:
                        nRet = BinLoading_Step02(true);
                        if (nRet != 0)
                        {
                            AxisOutputFeederY.EmgStop();
                            // Step02 내부에서 바코드/이동/실린더 관련 알람이 이미 발생 가능하므로 2000으로 덮지 않음
                            this.State = ProcessState.Error;
                            MarkStepOnFailure(LoadFlowStep.Step02);
                            return nRet;
                        }
                        if (IsStop) { MarkStepOnFailure(LoadFlowStep.Step02); return 0; }
                        AdvanceLoadStepOnSuccess(LoadFlowStep.Step02);
                        goto case LoadFlowStep.Step03;

                    case LoadFlowStep.Step03:
                        nRet = BinLoading_Step03(true);
                        if (nRet != 0)
                        {
                            AxisOutputFeederY.EmgStop();
                            // Step03 내부에서 StageLoadingFailed(2002)/2000 등 세부 알람이 이미 발생 가능
                            // 여기서 2000으로 덮지 않음
                            this.State = ProcessState.Error;
                            MarkStepOnFailure(LoadFlowStep.Step03);
                            return nRet;
                        }
                        if (IsStop) { MarkStepOnFailure(LoadFlowStep.Step03); return 0; }
                        AdvanceLoadStepOnSuccess(LoadFlowStep.Step03);
                        goto case LoadFlowStep.Step04;

                    case LoadFlowStep.Step04:
                        nRet = BinLoading_Step04(true);
                        if (nRet != 0)
                        {
                            AxisOutputFeederY.EmgStop();
                            // Step04는 SetMappingData 실패 -> 내부에서 StageLoadingFailed(2002)가 맞음(이미 올리고 있음)
                            this.State = ProcessState.Error;
                            MarkStepOnFailure(LoadFlowStep.Step04);
                            return nRet;
                        }
                        if (IsStop) { MarkStepOnFailure(LoadFlowStep.Step04); return 0; }
                        AdvanceLoadStepOnSuccess(LoadFlowStep.Step04);
                        break;

                    case LoadFlowStep.Completed:
                    default:
                        break;
                }

                _loadStep = LoadFlowStep.Completed;
                this.State = ProcessState.Complete;
                Log.Write(UnitName, "OnRunWork", "LoadFlowStep.StageLoadingAfter completed.");
                return 0;

                // 기존 코드
                //if (this.OutputCassetteLifter.IsHaveMoreProcessWafer() == true)
                //{
                //    nRet = BinLoading_Step01(true);
                //    if (nRet != 0) {
                //        AxisOutputFeederY.EmgStop();
                //        PostAlarm((int)AlarmKeys.Alarm_BinLoadingFailed);
                //        this.State = ProcessState.Error;
                //        return nRet;
                //    }
                //    if (IsStop)
                //    {
                //        Log.Write(UnitName, "OnRunWork", "IsScanCompleted");
                //        return 0;
                //    }

                //    nRet = BinLoading_Step02(true);
                //    if (nRet != 0)
                //    {
                //        AxisOutputFeederY.EmgStop();
                //        PostAlarm((int)AlarmKeys.Alarm_BinLoadingFailed);
                //        this.State = ProcessState.Error;
                //        return nRet;
                //    }
                //    if (IsStop)
                //    {
                //        Log.Write(UnitName, "OnRunWork", "IsScanCompleted");
                //        return 0;
                //    }

                //    nRet = BinLoading_Step03(true);
                //    if (nRet != 0)
                //    {
                //        AxisOutputFeederY.EmgStop();
                //        PostAlarm((int)AlarmKeys.Alarm_BinLoadingFailed);
                //        this.State = ProcessState.Error;
                //        return nRet;
                //    }
                //    if (IsStop)
                //    {
                //        Log.Write(UnitName, "OnRunWork", "IsScanCompleted");
                //        return 0;
                //    }

                //    nRet = BinLoading_Step04(true);
                //    if (nRet != 0)
                //    {
                //        AxisOutputFeederY.EmgStop();
                //        PostAlarm((int)AlarmKeys.Alarm_BinLoadingFailed);
                //        this.State = ProcessState.Error;
                //        return nRet;
                //    }
                //    if (IsStop)
                //    {
                //        Log.Write(UnitName, "OnRunWork", "IsScanCompleted");
                //        return 0;
                //    }

                //    this.State = ProcessState.Complete;
                //    Log.Write(UnitName, "OnRunWork", "LoadFlowStep.StageLoadingAfter completed.");

                //    //nRet = BinLoading(true);
                //    //if (nRet != 0)
                //    //{
                //    //    AxisOutputFeederY.EmgStop();
                //    //    PostAlarm((int)AlarmKeys.Alarm_BinLoadingFailed);
                //    //    this.State = ProcessState.Error;
                //    //    return nRet;
                //    //}
                //    //if (IsStop)
                //    //{
                //    //    Log.Write(UnitName, "OnRunWork", "IsScanCompleted");
                //    //    return 0;
                //    //}
                //    //this.State = ProcessState.Complete;
                //    //Log.Write(UnitName, "OnRunWork", "LoadFlowStep.StageLoadingAfter completed.");
                //}
                //else
                //{
                //    if (!IsPositionReady())
                //    {
                //        nRet = MoveToReady();
                //        if (nRet != 0)
                //        {
                //            AxisOutputFeederY.EmgStop();
                //            PostAlarm((int)AlarmKeys.Alarm_BinLoadingFailed);
                //            this.State = ProcessState.Error;
                //            return nRet;
                //        }
                //    }

                //    // [ADD] Input/Output Cassette 모두 소진 되었는지 확인 후 언로딩 + 장비 정지
                //    TryShutdownIfAllCassettesEmpty();

                //}
            }
        }
        protected override int OnRunComplete()
        {
            int ret = 0;
            _loadStep = LoadFlowStep.None;
            _unloadStep = UnloadFlowStep.None;
            _unloadTargetSlot = -1;
            this.State = ProcessState.Ready;
            Log.Write(UnitName, "OnRunComplete", "OnRunComplete Ok");
            return ret;
        }

        private int BinLoading_Step01(bool isFine = false)
        {
            int nRet = 0;

            nRet = this.OutputCassetteLifter.MoveToNextSlot(isFine);
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_OutputCassetteLifter_Fail); // [FIX] 2000 -> 2028
                return -1;
            }
            Log.Write(UnitName, "OnRunWork", "MoveToNextSlot completed.");

            nRet = OutputStage.LoadingBinPrepare(isFine);
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_OutputStageInterlockFailed);
                return -1;
            }
            Log.Write(UnitName, "OnRunWork", "LoadingBinPrepare completed.");

            return nRet;
        }

        private int BinLoading_Step02(bool isFine = false)
        {
            int nRet = 0;

            nRet = BinCassetteLoading(isFine); // Barcode 포함
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_BinCassetteLoadingFailed); // [FIX] 2000 -> 2083
                return -1;
            }
            Log.Write(UnitName, "OnRunWork", "BinCassetteLoading completed.");

            return nRet;
        }

        private int BinLoading_Step03(bool isFine = false)
        {
            int nRet = 0;

            nRet = StageLoading(isFine);
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageLoadingFailed);
                return -1;
            }
            Log.Write(UnitName, "OnRunWork", "StageLoading completed.");

            nRet = MoveToReady(isFine);
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_MoveToReadyFailed); // [FIX] 2000 -> 2081
                return -1;
            }
            Log.Write(UnitName, "OnRunWork", "MoveToReady completed.");

            nRet = OutputStage.LoadingBinComplete(isFine);
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageLoadingFailed);
                return -1;
            }
            Log.Write(UnitName, "OnRunWork", "LoadingBinComplete completed.");

            // 여기서 1차 Data 넘기자. 
            // Ready? Processing? 상태로 BinWafer 정보를 넘기자. Stage위에 제품은 있으니깐. 
            // 하지만 아직 작업은 하지 않는다.
            var waferOnFeeder2 = this.GetMaterial() as MaterialWafer;
            if (waferOnFeeder2 == null)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_OutputFeederBinData); // [FIX] 2000 -> 2026
                Log.Write(this, "No wafer on Feeder to move to OutputStage");
                return -1;
            }
            waferOnFeeder2.Presence = Material.MaterialPresence.Exist;
            waferOnFeeder2.ProcessSatate = Material.MaterialProcessSatate.Ready;
            OutputStage.SetMaterial(waferOnFeeder2);

            return nRet;
        }

        private int BinLoading_Step04(bool isFine = false)
        {
            int nRet = 0;

            nRet = SetMappingData();
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageLoadingFailed);
                return -1;
            }
            Log.Write(UnitName, "OnRunWork", "SetMappingData completed.");

            return nRet;
        }

        private int BinLoading(bool isFine = false)
        {
            int nRet = 0;
            nRet = this.OutputCassetteLifter.MoveToNextSlot(isFine);
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_BinLoadingFailed);
                return -1;
            }
            Log.Write(UnitName, "OnRunWork", "MoveToNextSlot completed.");

            nRet = OutputStage.LoadingBinPrepare(isFine);
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_OutputStageInterlockFailed);
                return -1;
            }
            Log.Write(UnitName, "OnRunWork", "LoadingBinPrepare completed.");

            nRet = BinCassetteLoading(isFine); // Barcode 포함
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_BinLoadingFailed);
                return -1;
            }
            Log.Write(UnitName, "OnRunWork", "BinCassetteLoading completed.");

            nRet = StageLoading(isFine);
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageLoadingFailed);
                return -1;
            }
            Log.Write(UnitName, "OnRunWork", "StageLoading completed.");

            nRet = MoveToReady(isFine);
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_BinLoadingFailed);
                return -1;
            }
            Log.Write(UnitName, "OnRunWork", "MoveToReady completed.");

            nRet = OutputStage.LoadingBinComplete(isFine);
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageLoadingFailed);
                return -1;
            }
            Log.Write(UnitName, "OnRunWork", "LoadingBinComplete completed.");

            // 여기서 1차 Data 넘기자. 
            // Ready? Processing? 상태로 BinWafer 정보를 넘기자. Stage위에 제품은 있으니깐. 
            // 하지만 아직 작업은 하지 않는다.
            var waferOnFeeder2 = this.GetMaterial() as MaterialWafer;
            if (waferOnFeeder2 == null)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_BinLoadingFailed);
                Log.Write(this, "No wafer on Feeder to move to OutputStage");
                return -1;
            }
            waferOnFeeder2.Presence = Material.MaterialPresence.Exist;
            waferOnFeeder2.ProcessSatate = Material.MaterialProcessSatate.Ready;
            OutputStage.SetMaterial(waferOnFeeder2);

            nRet = SetMappingData();
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageLoadingFailed);
                return -1;
            }
            Log.Write(UnitName, "OnRunWork", "SetMappingData completed.");

            return nRet;
        }

        private int SetMappingData()
        {
            int nRet = 0;
            // 2) Bin Stage Mapping -> InputStage의 Die 정보 복사
            var inputStage = Equipment.Instance.GetUnit("InputStage") as InputStage;
            if (inputStage == null)
            {
                Log.Write(UnitName, "BinStageMapping", "InputStage not found → inputStage = null.");
                return -1;
            }

            var srcWafer = inputStage.GetMaterialWafer();
            while (true)
            {
                if(IsStop)
                {
                    Log.Write(UnitName, "BinStageMapping", "IsStop detected during waiting for InputStage wafer.");
                    return 0;
                }

                srcWafer = inputStage.GetMaterialWafer();
                if (srcWafer != null)
                {
                    lock (srcWafer.Dies)
                    {
                        if (srcWafer.Dies == null 
                            || srcWafer.Dies.Count == 0
                            || srcWafer.ProcessSatate != Material.MaterialProcessSatate.Processing)
                        {
                            //Log?
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            var BinWafer = GetMaterial() as MaterialWafer;
            // 픽업 직후 재선택 방지: Processing 전환 + SlotIndex 보정 + 경로 준비
            if (BinWafer != null)
            {
                BinWafer.Presence = Material.MaterialPresence.Exist;
                BinWafer.ProcessSatate = Material.MaterialProcessSatate.Ready;
                lock (BinWafer.Dies)
                {
                    if (BinWafer.Dies == null || BinWafer.Dies.Count == 0)
                    {
                        // 이 안이 핵심. InputStage Wafer Data도 여기서 가져옴.
                        MakePath();
                    }
                }
            }

            OutputStage?.UpdateUI();
            OutputStage?.OnDiePlaced(null);

            var waferOnFeeder2 = this.GetMaterial() as MaterialWafer;
            if (waferOnFeeder2 == null)
            {
                AxisOutputFeederY.EmgStop();
                // [FIX] 여기는 로딩 실패(2000)보다 "Feeder Bin Data 오류"(2026)가 정확함.
                PostAlarm((int)AlarmKeys.Alarm_OutputFeederBinData);
                Log.Write(UnitName, "SetMappingData", "No wafer on Feeder to move to OutputStage");
                return -1;
            }

            this.MoveMaterial(waferOnFeeder2, OutputStage);
            waferOnFeeder2.ProcessSatate = Material.MaterialProcessSatate.Ready;
            OutputStage.SetMaterial(waferOnFeeder2);
            this.SetMaterial(null);

            BinWafer = OutputStage?.GetMaterialWafer();
            BinWafer.ProcessSatate = Material.MaterialProcessSatate.Processing;
            OutputStage?.SetMaterial(BinWafer);

            // 웨이퍼 로딩 확정 시 요약 시작
            var waferOnStage = InputStage?.GetMaterialWafer();
            //VA1VPRO16
            Equipment.Instance.ResultWriterManager.BeginWaferSummary(waferOnStage?.WaferId, "VA1VPRO16");

            Log.Write(UnitName, "OnRunWork", "LoadFlowStep.BinStageMapping completed.");
            return nRet;
        }

        #endregion

        protected override void OnMakeSequence()
        {
            base.OnMakeSequence();
            this.SequencePlayers.Add(BinCassetteLoading);
            this.SequencePlayers.Add(StageLoading);
            this.SequencePlayers.Add(MoveToReady);
            this.SequencePlayers.Add(BinUnloading);
        }


        #region Seq 단위 동작 함수
        // [ADD] WaferExchangeDecision 로그 쓰로틀/변화 감지용(간단 버전)
        private int _lastWEDStateMask = -1;
        private int _lastWEDTick = 0;
        private bool ShouldEnterWorkForWaferExchange(out bool unloadFirst)
        {
            unloadFirst = false;
            var waferBin = OutputStage?.GetMaterialWafer();
            bool stageHasBin = OutputStage?.IsRingPresent() == true;
            bool feederHasWafer = GetMaterial() is MaterialWafer;
            var feederBin = GetMaterial() as MaterialWafer;
            if (stageHasBin)
            {
                bool diesMissing = (waferBin == null) || waferBin.Dies == null || waferBin.Dies.Count == 0;
                bool noNextDie = false;
                try 
                { 
                    noNextDie = !OutputStage.HasNextDie(); 
                } 
                catch (Exception ex)
                { 
                    //noNextDie = true;
                    Log.Write(ex);
                }

                bool noNextDieByStateOnly = true;
                bool binFull = false;
                try
                {
                    lock (waferBin.Dies)
                    {
                        if (waferBin?.Dies != null && waferBin.Dies.Count > 0)
                        {
                            noNextDieByStateOnly = !waferBin.Dies.Any
                                (d => d != null &&
                                d.State != DieProcessState.Placed &&
                                d.State != DieProcessState.Rejected);
                        }
                        // 임시 우회: State-only가 남아있으면 '없다'로 보지 않음
                        noNextDie = noNextDie && noNextDieByStateOnly;
                        binFull = waferBin != null &&
                                       waferBin.Dies != null &&
                                       waferBin.Dies.Count > 0 &&
                                       waferBin.Dies.All(d =>
                                       d != null &&
                                       (d.State == DieProcessState.Placed || d.State == DieProcessState.Rejected));
                    }

                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }

                // 전체 완료(Placed+Rejected) 시 Completed 승격 (안전 보정)
                if (binFull &&
                    waferBin.ProcessSatate != Material.MaterialProcessSatate.Completed)
                {
                    waferBin.ProcessSatate = Material.MaterialProcessSatate.Completed;
                }

                // 진단 로그(쓰로틀/변화 시에만)
                try
                {
                    int total = waferBin?.Dies?.Count ?? 0;
                    int placed = waferBin?.Dies?.Count(d => d != null && d.Presence == Material.MaterialPresence.Exist) ?? 0;
                    var proc = waferBin?.ProcessSatate;

                    // 상태 마스크(간단 요약): 변화 감지/이슈 판별에만 사용
                    int mask = 0;
                    if (diesMissing) mask |= 1;
                    if (noNextDie) mask |= 2;
                    if (binFull) mask |= 4;
                    if (proc == Material.MaterialProcessSatate.Completed) mask |= 8;

                    int now = Environment.TickCount;
                    bool changed = (mask != _lastWEDStateMask);
                    bool issue = (mask != 0); // 하나라도 true면 이슈로 간주
                    int intervalMs = issue ? 1000 : 5000; // 이슈: 5초, 정상: 15초 간격

                    if (changed || (now - _lastWEDTick) >= intervalMs)
                    {
                        Log.Write(UnitName, "WaferExchangeDecision",
                            $"mask={mask}, stageHasBin={stageHasBin}, diesMissing={diesMissing}, noNextDie={noNextDie}, binFull={binFull}, " +
                            $"proc={proc}, totalDies={total}, placed={placed}");

                        _lastWEDStateMask = mask;
                        _lastWEDTick = now;
                    }
                }
                catch (Exception ex) 
                { Log.Write(ex); }

                if (waferBin == null && feederBin != null)
                {
                    unloadFirst = false;
                    return true; // 언로드 시퀀스 진입
                    //LoadFlowStep.BinStageMapping;
                }

                if (diesMissing || noNextDie || binFull ||
                    waferBin?.ProcessSatate == Material.MaterialProcessSatate.Completed)
                {
                    unloadFirst = true;
                    return true; // 언로드 시퀀스 진입
                }
                // 아직 더 놓을 다이 존재 → 유지 (Work 미진입, Ready 대기)
                return false;
            }

            // Stage 비어있고 Feeder에만 웨이퍼 존재 → Stage 로딩 진행
            if (!stageHasBin && feederHasWafer)
                return true;

            bool cassettePresent = OutputCassetteLifter?.IsCassettePresentAll() == true;
            bool scanDone = OutputCassetteLifter?.IsScanCompleted() == true;

            // Cassette 장착 + 스캔 미완료 → Scan 수행 위해 Work
            if (cassettePresent && !scanDone)
                return true;

            // Cassette에 더 로딩 가능한 웨이퍼 존재
            if (OutputCassetteLifter?.IsHaveMoreProcessWafer() == true)
            {
                return true;
            }
            else
            {
                TryShutdownIfAllCassettesEmpty();
                return false;
            }

            return false;
        }

        private int PreparetoOutputStage()
        {
            int nRet = 0;

            // T 보정 필요시. 
            //nRet = OutputStage.ScanBin();

            return nRet;
        }
        public int BinCassetteLoading(bool isFine = false)
        {
            int nRet = 0;

            if(RunMode == UnitRunMode.Manual)
            {
                CurrentFunc = BinCassetteLoading;
            }

            Log.Write(UnitName, "BinLoading Start");
            if (IsMoveInterLockCassette() == false)
            {
                Log.Write(UnitName, "Not IsMoveInterLockCassette");
                return -1;
            }

            // === Exchange 대기 전략 ===
            // - 언로드 직후 또는 이미 바코드에 있으면 Ready 이동 스킵
            bool preferBarcode = _exchangeStandbyForNextLoad || IsPositionBarcode();
            if (preferBarcode)
            {
                if (!IsPositionBarcode())
                {
                    nRet = MovePositionBarcode(isFine);
                    if (nRet != 0)
                    {
                        Log.Write(UnitName, "BinLoading Fail - MovePositionBarcode");
                        return nRet;
                    }
                }
                Log.Write(UnitName, "BinLoading", "[Exchange] Standby at Barcode → skip MoveToReady");
                _exchangeStandbyForNextLoad = false; // 1회 사용
            }
            else
            {
                // 이미 Ready면 스킵
                if (!IsPositionReady())
                {
                    nRet = MoveToReady(isFine);
                    if (nRet != 0)
                    {
                        Log.Write(UnitName, "BinLoading Failed - MoveToReady");
                        return nRet;
                    }
                }
                else
                {
                    Log.Write(UnitName, "BinLoading", "[Skip] Already at Ready");
                }
            }

            nRet = UnClampGripper();
            if (nRet != 0)
            {
                Log.Write(UnitName, "UnClampGripper Failed");
                return nRet;
            }

            nRet = DownFeeder();
            if (nRet != 0)
            {
                Log.Write(UnitName, "DownFeeder Failed");
                return nRet;
            }

            nRet = MoveToCassette(isFine);
            if (nRet != 0)
            {
                Log.Write(UnitName, "MoveToCassette Failed");
                return nRet;
            }

            nRet = BarcodeReading(isFine);
            if (nRet != 0)
            {
                Log.Write(UnitName, "BarcodeReading Failed");
                return nRet;
            }

            string barcode;
            nRet = ReadBarcodeWithRetry(out barcode, isFine);
            if (nRet != 0)
            {
                //if (IsPositionBarcode())
                {
                    if (OutputCassetteLifter.IsAnyAxisMoving())
                    {
                        OutputCassetteLifter.BinLifterZ.EmgStop();
                    }

                    if (IsFeederDown())
                    {
                        if (IsRingPresent())
                        {
                            if (IsUnClamped() == false || IsUnClamped() == true)
                            {
                                nRet = MovePositionCassette();
                                if (nRet != 0)
                                {
                                    AxisOutputFeederY.EmgStop();
                                    PostAlarm((int)AlarmKeys.Alarm_OutputStageInterlockFailed);
                                    Log.Write(UnitName, "CheckReady Fail - OutputStage.IsStageInterLockOK");
                                    return -1;
                                }
                                nRet = UnClampGripper();
                                if (nRet != 0)
                                {
                                    Log.Write(UnitName, "CheckReady Fail - UnClampGripper");
                                    return nRet;
                                }
                                nRet = MovePositionReady();
                                if (nRet != 0)
                                {
                                    AxisOutputFeederY.EmgStop();
                                    PostAlarm((int)AlarmKeys.Alarm_OutputStageInterlockFailed);
                                    Log.Write(UnitName, "CheckReady Fail - OutputStage.IsStageInterLockOK");
                                    return -1;
                                }
                            }
                            else
                            {
                                AxisOutputFeederY.EmgStop();
                                PostAlarm((int)AlarmKeys.Alarm_OutputStageInterlockFailed);
                                Log.Write(UnitName, "CheckReady Fail - OutputStage.IsStageInterLockOK");
                                return -1;
                            }
                        }
                        else
                        {

                            AxisOutputFeederY.EmgStop();
                            PostAlarm((int)AlarmKeys.Alarm_OutputStageInterlockFailed);
                            Log.Write(UnitName, "CheckReady Fail - OutputStage.IsStageInterLockOK");
                            return -1;
                        }
                    }
                    else
                    {
                        AxisOutputFeederY.EmgStop();
                        PostAlarm((int)AlarmKeys.Alarm_OutputStageInterlockFailed);
                        Log.Write(UnitName, "CheckReady Fail - OutputStage.IsStageInterLockOK");
                        return -1;
                    }
                }

                Log.Write(UnitName, "BinLoading", "ReadBarcodeWithRetry Failed");
                return -1;
            }
            
            strBarcode = barcode;
            {
                var c = this.OutputCassetteLifter.GetMaterialCassette();
                int nIndex = this.OutputCassetteLifter.GetCurrectSlotID();
                MaterialWafer Bin = c.GetWafer(nIndex);

                Bin.CarrierId = c.CarrierId;

                if (Config.IsSimulation || Config.IsDryRun)
                {
                    strBarcode = string.Format("{0}_{0}", strBarcode, Bin.CarrierId);
                }
                else
                {
                    Bin.WaferId = strBarcode;
                }
                this.SetMaterial(Bin);

                RaiseWaferIdChanged(strBarcode);
                Log.Write(UnitName, "WaferLoading", strBarcode);
            }

            Log.Write(UnitName, "BinLoading Complete");
            return 0;

        }

        public int StageLoading(bool isFine = false)
        {
            int nRet = 0;
            if (RunMode == UnitRunMode.Manual)
            {
                CurrentFunc = StageLoading;
            }
            Log.Write(this, "StageLoading Start");
            if (IsMoveInterLockCassette() == false)
            {
                Log.Write(this, "Not IsMoveInterLockCassette");
                return -1;
            }

            nRet = MovePositionStage(isFine);
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageLoadingFailed);
                Log.Write(this, "MovePositionStage Failed");
                nRet = -1;
                return nRet;
            }
            //if (IsStop) { return 0; }

            nRet = UnClampGripper();
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageLoadingFailed);
                Log.Write(this, "UnClampGripper Failed");
                nRet = -1;
                return nRet;
            }

            Log.Write(this, "StageLoading End");
            return nRet;
        }
        public int MoveToReady(bool isFine = false)
        {
            int nRet = 0;
            if (RunMode == UnitRunMode.Manual)
            {
                CurrentFunc = MoveToReady;
            }
            Log.Write(this, "MoveToReay Start");
            if (IsMoveInterLockCassette() == false)
            {
                return -1;
            }

            nRet = MovePositionReady(isFine);
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                // [FIX] MovePositionReady() 경로에서 이미 인터락(2010) 등 원인 알람이 발생 가능.
                //       여기서는 BinLoadingFailed(2000)로 덮지 않음.
                return -1;
            }

            nRet = UpFeeder();
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                // [FIX] UpFeeder() 내부 WaitLiftStateOrAlarm()가 2030/2031을 발생시킴.
                //       여기서는 BinLoadingFailed(2000)로 덮지 않음.
                return -1;
            }
            Log.Write(this, "MoveToReay End");


            return nRet;
        }

        private bool IsSafeToStartStageUnloading()
        {
            try
            {
                // Stage 쪽 die place가 아직 남아있고/진행 중이면 언로드 금지.
                // (ODT가 die를 들고 있으면 "곧 Place할 가능성"이 있으니 막아야 함)
                var odtDie = OutputDieTransfer?.GetMaterial() as MaterialDie;
                if (odtDie != null)
                    return false;

                //20251220 - 이 조건 성립이 안됨. 
                //null 이 아니라 다른 조건 봐야함.
                // Rotary 언로드 소켓에 die가 있으면, ODT가 곧 픽/플레이스 할 수 있으니 막음
                // 이거 있어야 되는디
                //var unloadSocketDie = Rotary?.GetUnloadSocketMaterial();
                //if (unloadSocketDie != null)
                //    return false;

                return true;
            }
            catch
            {
                // 판단 불가면 보수적으로 막음
                return false;
            }
        }

        public int BinUnloading_Step01(bool isFine = false)
        {
            int nRet = 0;

            // [ADD] ODT/Rotary 버퍼가 비기 전에는 Stage 언로딩 시작 금지
            if (!IsSafeToStartStageUnloading())
            {
                // 여기서는 알람을 올리지 말고 "대기"가 안전합니다.
                // Ready에서 다시 돌면서 자연스럽게 비면 언로딩 진행.
                Log.Write(UnitName, "BinUnloading_Step01", "Blocked: OutputDieTransfer/Rotary still has die.");
                return 1; // [IMPORTANT] 0이 아닌 '대기' 코드
            }

            bool bBinInStage = this.OutputStage.IsRingPresent();
            bool bBinInFeeder = IsRingPresent();
            var BinStage = this.OutputStage.GetMaterialWafer();

            if (BinStage == null)
            {
                Log.Write(UnitName, "OnRunWork", "OnRunWork: BinUnloading - wafer is null on OutputStage.");
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_OutputFeederBinData);
                return -1;
            }

            nRet = CheckStageWaferBeforeUnload(BinStage);
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_OutputFeederBinData);
                State = ProcessState.Error;
                Log.Write(UnitName, "OnRunWork", "CheckStageWaferBeforeUnload Failed");
                return -1;
            }

            // Stage 언로딩 준비
            nRet = this.OutputStage.PrepareOutputStageUnloadingBin();
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_PrepareOutputStageUnloadingBin);
                this.State = ProcessState.Error;
                Log.Write(UnitName, "OnRunWork", "OutputStage.PrepareOutputStageUnloadingBin Failed");
                return nRet;
            }

            return nRet;
        }

        public int BinUnloading_Step02(bool isFine = false)
        {
            int nRet = 0;

            bool bBinInStage = this.OutputStage.IsRingPresent();
            bool bBinInFeeder = IsRingPresent();
            var BinStage = this.OutputStage.GetMaterialWafer();

            // Stage → Feeder
            nRet = UnloadBinStageToFeeder();
            if (nRet != 0)
            {
                //함수 내부에서 알람 발생.
                AxisOutputFeederY.EmgStop();
                Log.Write(UnitName, "OnRunWork", "UnloadBinStageToFeeder Failed");
                return nRet;
            }

            nRet = ClampGripper();
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_GripperClampFailed);
                Log.Write(UnitName, "OnRunWork", "ClampGripper Failed");
                return nRet;
            }

            // 머티리얼 이동 (Stage → Feeder)
            var waferFromStage = BinStage;
            this.OutputStage.MoveMaterial(waferFromStage, this);
            this.OutputStage.SetMaterial(null);
            if (VerifyWaferMovedStageToFeeder(waferFromStage) != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_OutputFeederBinData);
                Log.Write(UnitName, "OnRunWork", "VerifyWaferMovedStageToFeeder Failed");
                return -1;
            }

            // 언로딩 대상 슬롯 계산
            var waferFromFeeder = this.GetMaterial() as MaterialWafer;
            int slotFromStage = (waferFromFeeder != null) ? waferFromFeeder.SlotIndex : -1;
            int lifterSlot = this.OutputCassetteLifter.GetCurrectSlotID();
            int nSlot = slotFromStage >= 0 ? slotFromStage : (lifterSlot >= 0 ? lifterSlot : _dryLastSlotIndex);
            if (nSlot < 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_BinUnloadingFailed);
                this.State = ProcessState.Error;
                Log.Write(UnitName, "OnRunWork", "BinUnloading - Invalid slot index (stage only case)");
                return -1;
            }
            Log.Write(UnitName, "BinUnloading", $"BinUnloading - MoveToSlot : {nSlot}");

            // [추가] 다음 단계에서 사용할 대상 슬롯 저장
            _unloadTargetSlot = nSlot;

            return nRet;
        }

        public int BinUnloading_Step03(bool isFine = false)
        {
            int nRet = 0;

            if (_unloadTargetSlot >= 0)
            {
                int rc = this.OutputCassetteLifter.MoveToSlot(_unloadTargetSlot);
                if (rc != 0)
                {
                    AxisOutputFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_CassetteMoveToSlotFailedForUnload); // 2062
                    this.State = ProcessState.Error;
                    Log.Write(UnitName, "BinUnloading_Step03", "MoveToSlot Failed");
                    return rc;
                }
            }

            nRet = UnloadOnlyFeederToCassette(true);
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                // 상세 알람은 내부에서 발생하므로 2004로 덮지 않음
                this.State = ProcessState.Error;
                return nRet;
            }

            Log.Write(UnitName, "BinUnloading", "End");
            return 0;
        }

        public int BinUnloading(bool isFine = false)
        {
            int nRet = 0;
            if (RunMode == UnitRunMode.Manual)
            {
                CurrentFunc = BinUnloading;
            }

            bool bBinInStage = this.OutputStage.IsRingPresent();
            bool bBinInFeeder = IsRingPresent();
            var BinStage = this.OutputStage.GetMaterialWafer();

            if (BinStage == null)
            {
                Log.Write(UnitName, "OnRunWork", "OnRunWork: BinUnloading - wafer is null on OutputStage.");
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_OutputFeederBinData);
                return -1;
            }

            nRet = CheckStageWaferBeforeUnload(BinStage);
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_OutputFeederBinData);
                State = ProcessState.Error;
                Log.Write(UnitName, "OnRunWork", "CheckStageWaferBeforeUnload Failed");
                return -1;
            }

            // Stage 언로딩 준비
            nRet = this.OutputStage.PrepareOutputStageUnloadingBin();
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_PrepareOutputStageUnloadingBin);
                this.State = ProcessState.Error;
                Log.Write(UnitName, "OnRunWork", "OutputStage.PrepareOutputStageUnloadingBin Failed");
                return nRet;
            }

            // Stage → Feeder
            nRet = UnloadBinStageToFeeder();
            if (nRet != 0)
            {
                //함수 내부에서 알람 발생.
                AxisOutputFeederY.EmgStop();
                Log.Write(UnitName, "OnRunWork", "UnloadBinStageToFeeder Failed");
                return nRet;
            }

            nRet = ClampGripper();
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_GripperClampFailed);
                Log.Write(UnitName, "OnRunWork", "ClampGripper Failed");
                return nRet;
            }

            // 머티리얼 이동 (Stage → Feeder)
            var waferFromStage = BinStage;
            this.OutputStage.MoveMaterial(waferFromStage, this);
            this.OutputStage.SetMaterial(null);
            if (VerifyWaferMovedStageToFeeder(waferFromStage) != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_OutputFeederBinData);
                Log.Write(UnitName, "OnRunWork", "VerifyWaferMovedStageToFeeder Failed");
                return -1;
            }

            // 언로딩 대상 슬롯 계산
            var waferFromFeeder = this.GetMaterial() as MaterialWafer;
            int slotFromStage = (waferFromFeeder != null) ? waferFromFeeder.SlotIndex : -1;
            int lifterSlot = this.OutputCassetteLifter.GetCurrectSlotID();
            int nSlot = slotFromStage >= 0 ? slotFromStage : (lifterSlot >= 0 ? lifterSlot : _dryLastSlotIndex);
            if (nSlot < 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_BinUnloadingFailed);
                this.State = ProcessState.Error;
                Log.Write(UnitName, "OnRunWork", "BinUnloading - Invalid slot index (stage only case)");
                return -1;
            }
            Log.Write(UnitName, "BinUnloading", $"BinUnloading - MoveToSlot : {nSlot}");

            nRet = this.OutputCassetteLifter.MoveToSlot(nSlot);
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_BinUnloadingFailed);
                this.State = ProcessState.Error;
                Log.Write(UnitName, "OnRunWork", "OutputCassetteLifter.MoveToSlot Failed");
                return nRet;
            }

            // Feeder → Cassette만 수행
            nRet = UnloadOnlyFeederToCassette(true);
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_BinUnloadingFailed);
                this.State = ProcessState.Error;
                return nRet;
            }
            Log.Write(UnitName, "BinUnloading", "End");

            return nRet;
        }

        public int BinUnloading(MaterialWafer wafer, bool isFine = false)
        {
            int nRet = 0;
            if (RunMode == UnitRunMode.Manual)
            {
                CurrentFunc = MoveToReady;
            }
            Log.Write(this, "BinUnloading Start");

            nRet = this.OutputStage.PrepareOutputStageUnloadingBin();
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_BinUnloadingFailed);
                Log.Write(this, "OutputStage.PrepareOutputStageUnloadingBin Failed");
                nRet = -1;
                return nRet;
            }

            nRet = UnloadBinStageToFeeder(isFine);
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                Log.Write(this, "UnloadBinStageToFeeder Failed");
                nRet = -1;
                return nRet;
            }

            //int nSlot = wafer.SlotIndex;
            // 안전한 언로딩 슬롯 산출: Stage wafer.SlotIndex → 없으면 Lifter 현재 슬롯 → DryRun 마지막 슬롯
            int slotFromStage = (wafer != null) ? wafer.SlotIndex : -1;
            int lifterSlot = this.OutputCassetteLifter.GetCurrectSlotID();
            int nSlot = slotFromStage >= 0 ? slotFromStage
                        : (lifterSlot >= 0 ? lifterSlot : _dryLastSlotIndex);
            if (nSlot < 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_BinUnloadingFailed);
                Log.Write(this, "BinUnloading - Invalid slot index (all sources invalid)");
                return -1;
            }

            Log.Write(UnitName, "BinUnloading", $"BinUnloading - MoveToSlot : {nSlot}");
            nRet = this.OutputCassetteLifter.MoveToSlot(nSlot); // 언로딩 해야하는 Slot으로 이동 요청.
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_BinUnloadingFailed);
                this.State = ProcessState.Error;
                Log.Write(this, "OutputCassetteLifter.MoveToSlot Failed");
                return nRet;
            }

            nRet = UnloadBinFeederToCassette(true);
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_BinUnloadingFailed);
                this.State = ProcessState.Error;
                Log.Write(this, "UnloadBinFeederToCassette Failed");
                return nRet;
            }

            Log.Write(this, "BinUnloading Complete");
            return nRet;
        }


        private int UnloadOnlyFeederToCassette(bool isFine = false)
        {
            int nRet = 0;
            // 1) Cassette 위치 이동 실패
            nRet = MovePositionCassette(isFine);
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_UnloadFeederToCassette_MoveFeederToCassettePosFailed);
                Log.Write(UnitName, "UnloadOnlyFeederToCassette", "Fail - MovePositionCassette");
                return -1;
            }

            // 2) Unclamp 실패
            nRet = UnClampGripper();
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_UnloadFeederToCassette_UnclampFailed);
                Log.Write(UnitName, "UnloadOnlyFeederToCassette", "Fail - UnClampGripper");
                return -1;
            }

            // 3) Feeder -> Cassette 데이터 반영 실패(데이터 invalid)
            var waferOnFeeder = this.GetMaterial() as MaterialWafer;
            if (waferOnFeeder == null || waferOnFeeder.SlotIndex < 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_UnloadFeederToCassette_BinDataInvalid);
                Log.Write(UnitName, "UnloadOnlyFeederToCassette", "Fail - waferOnFeeder null or SlotIndex invalid");
                return -1;
            }
            else
            {
                var cassette = this.OutputCassetteLifter.GetMaterialCassette();
                waferOnFeeder.ProcessSatate = Material.MaterialProcessSatate.Completed;
                waferOnFeeder.Presence = Material.MaterialPresence.Exist;
                cassette.SetWafer(waferOnFeeder.SlotIndex, waferOnFeeder);
                SetMaterial(null);
            }

            // 4) 배출 검증 실패는 BinUnloadingFailed(2004) 대신 “배출 후 잔류/불일치” 전용(2041)로 분리
            int verify = VerifyAfterUnloadToCassette(waferOnFeeder.SlotIndex);
            if (verify != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_BinMissingAfterFeederToCassette);
                Log.Write(UnitName, "UnloadOnlyFeederToCassette", "Fail - VerifyAfterUnloadToCassette");
                return verify;
            }

            // 회피 = 바코드 위치 대기
            nRet = MovePositionBarcode(isFine);
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_UnloadFeederToCassette_MoveStandbyBarcodeFailed); // [FIX] 2003 -> 2073
                Log.Write(UnitName, "UnloadOnlyFeederToCassette", "UnloadOnlyFeederToCassette Fail - MovePositionBarcode");
                return -1;
            }

            // 5) 다음 로딩 가능 여부에 따라 대기 위치 결정 + 이동 실패 알람 분리
            bool hasNext = false;
            try { hasNext = OutputCassetteLifter != null && OutputCassetteLifter.IsHaveMoreProcessWafer(); }
            catch { hasNext = false; }

            if (hasNext)
            {
                nRet = MovePositionBarcode(isFine);
                if (nRet != 0)
                {
                    AxisOutputFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_UnloadFeederToCassette_MoveStandbyBarcodeFailed);
                    Log.Write(UnitName, "UnloadOnlyFeederToCassette", "Fail - MovePositionBarcode");
                    return -1;
                }
                _exchangeStandbyForNextLoad = true;
            }
            else
            {
                nRet = MoveToReady(isFine);
                if (nRet != 0)
                {
                    AxisOutputFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_UnloadFeederToCassette_MoveStandbyReadyFailed);
                    Log.Write(UnitName, "UnloadOnlyFeederToCassette", "Fail - MoveToReady");
                    return -1;
                }
                _exchangeStandbyForNextLoad = false;
            }

            this.SetMaterial(null);
            return 0;
        }

        public int UnloadBinFeederToCassette(bool isFine = false)
        {
            int nRet = 0;

            if (!IsRingPresent() && GetMaterial() == null)
            {
                Log.Write(UnitName, "UnloadBinFeederToCassette", "[Unload] Feeder empty -> skip full unload sequence");
                _exchangeStandbyForNextLoad = true;
                return -2;
            }

            nRet = UnloadBinStagetToFeeder(isFine);
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                Log.Write(UnitName, "UnloadBinFeederToCassette", "UnloadBinStagetToFeeder Failed");
                nRet = -1;
                return nRet;
            }

            nRet = ClampGripper();
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                Log.Write(UnitName, "UnloadBinFeederToCassette", "ClampGripper Failed");
                nRet = -1;
                return nRet;
            }

            // Stage의 실제 웨이퍼를 가져와 그대로 Feeder로 이동
            var waferFromStage = this.OutputStage.GetMaterialWafer();
            if (waferFromStage == null)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                Log.Write(UnitName, "UnloadBinFeederToCassette", "No wafer on OutputStage to move to Feeder");
                return -1;
            }
            this.OutputStage.MoveMaterial(waferFromStage, this);
            this.OutputStage.SetMaterial(null);

            nRet = MovePositionCassette(isFine);
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                Log.Write(this, "MovePositionCassette Failed");
                nRet = -1;
                return nRet;
            }

            nRet = UnClampGripper();
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                Log.Write(this, "UnClampGripper Failed");
                nRet = -1;
                return nRet;
            }

            // 피더 -> 카세트: 웨이퍼 정보 되돌려 넣기
            var waferOnFeeder = this.GetMaterial() as MaterialWafer;
            if (waferOnFeeder != null && waferOnFeeder.SlotIndex >= 0)
            {
                var cassette = this.OutputCassetteLifter.GetMaterialCassette();
                waferOnFeeder.ProcessSatate = Material.MaterialProcessSatate.Completed;
                waferOnFeeder.Presence = Material.MaterialPresence.Exist;
                cassette.SetWafer(waferOnFeeder.SlotIndex, waferOnFeeder);
            }
            else
            {
                Log.Write(this, "Unload: Feeder has no wafer or invalid SlotIndex");
            }

            // 다음 로딩 가능 여부에 따라 대기 위치 결정
            bool hasNext = false;
            try { hasNext = OutputCassetteLifter != null && OutputCassetteLifter.IsHaveMoreProcessWafer(); }
            catch { hasNext = false; }

            if (hasNext)
            {
                nRet = MovePositionBarcode(isFine);
                if (nRet != 0)
                {
                    AxisOutputFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                    Log.Write(UnitName, "UnloadBinFeederToCassette", "MovePositionBarcode Failed");
                    return -1;
                }
                _exchangeStandbyForNextLoad = true;
            }
            else
            {
                nRet = MoveToReady(isFine);
                if (nRet != 0)
                {
                    AxisOutputFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_UnloadFeederToCassette_MoveStandbyReadyFailed); // [FIX] 2000 -> 2074
                    Log.Write(UnitName, "UnloadBinFeederToCassette", "Fail - MoveToReady");
                    return -1;
                }
                _exchangeStandbyForNextLoad = false;
            }

            // Feeder의 material 정리 (배출 완료 후 비움)
            this.SetMaterial(null);
            return nRet;

        }


        public int ClampGripper()
        {
            int nRet = 0;
            this.SetClamp(true);
            nRet = WaitClampStateOrAlarm(expectClamp: true, timeoutMs: 1500, pollMs: 2);
            if (nRet != 0)
            {
                AxisOutputFeederY?.EmgStop();
                Log.Write(this, "Clamp Failed");
                return -1;
            }
            return 0;
        }
        public int UnClampGripper()
        {
            int nRet = 0;
            this.SetClamp(false);
            nRet = WaitClampStateOrAlarm(expectClamp: false, timeoutMs: 1500, pollMs: 2);
            if (nRet != 0)
            {
                AxisOutputFeederY?.EmgStop();
                Log.Write(this, "Unclamp Failed");
                return -1;
            }
            return 0;
        }
        public int UpFeeder()
        {
            int nRet = 0;
            this.SetLift(true);
            if (Config.IsSimulation) 
            { 
                _simFeederUp = true; 
                return 0; 
            }

            nRet = WaitLiftStateOrAlarm(expectUp: true, timeoutMs: 1500, pollMs: 2);
            if (nRet != 0)
            {
                AxisOutputFeederY?.EmgStop();
                Log.Write(this, "Feeder Up Failed");
                return -1;
            }
            return 0;
        }
        public int DownFeeder()
        {
            int nRet = 0;
            this.SetLift(false);
            if (Config.IsSimulation)
            {
                _simFeederUp = false;
                return 0;
            }

            nRet = WaitLiftStateOrAlarm(expectUp: false, timeoutMs: 1500, pollMs: 2);
            if (nRet != 0)
            {
                AxisOutputFeederY?.EmgStop();
                Log.Write(this, "Feeder Down Failed");
                return -1;
            }
            return 0;
        }

        public int MoveToCassette(bool isFine = false)
        {
            int nRet = 0;
            nRet = MovePositionCassette(isFine);
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                // [FIX] MovePositionCassette() 내부에서 2025(Interlock) 등 원인 알람이 발생 가능 → 2000 제거
                Log.Write(this, "MovePositionCassette Failed");
                return -1;
            }

            nRet = ClampGripper();
            if (nRet != 0)
            {
                // [FIX] ClampGripper() 내부 WaitClampStateOrAlarm()가 2032(ClampTimeout) 등 발생 → 2000 제거
                Log.Write(this, "ClampGripper Failed");
                return -1;
            }

            return nRet;
        }
        public Task<int> MoveToCassetteAsync(bool isFine)
        {
            return Task.Run(() => OnMoveToCassette(isFine));
        }
        protected int OnMoveToCassette(bool isFine)
        {
            int nRet = 0;
            bool ok = NeedUnloadFirst ? IsInterlockOKWaferUnloading() : IsInterlockOKWaferLoading();
            if (!ok)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_OutputFeederInterlockFailed);
                Log.Write(UnitName, "OnMoveToCassette", "Interlock failed");
                nRet = -1;
                return nRet;
            }

            nRet = base.MoveTeachingPositionOnce((int)OutputFeederConfig.TeachingPositionName.Cassette, isFine);
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_MoveToCassetteTeachFailed); // [FIX] 2000 -> 2082
                Log.Write(UnitName, "OnMoveToCassette", "MoveTeachingPositionOnce Failed");
                nRet = -1;
                return nRet;
            }
            return nRet;
        }

        public int BarcodeReading(bool isFine = false)
        {
            int nRet = 0;

            nRet = MovePositionBarcode(isFine);
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_BarcodeReadingFailed);
                Log.Write(this, "MovePositionBarcode Failed");
                nRet = -1;
                return nRet;
            }

            return nRet;
        }
        public int GetBarcode(out string strBarcode)
        {
            int nRet = 0;
            strBarcode = string.Empty;
            // Barcode Reading Logic
            bool isRead = true; // TODO: Barcode Reading Logic

            if (Config.IsSimulation
                || Config.IsDryRun)
            {
                strBarcode = "TestBin";
            }
            else
            {
                strBarcode = OutputCassetteLifter.ReadBarcoder();
            }
            if (strBarcode != string.Empty)
            {
                isRead = true;
            }
            else
            {
                isRead = false;
            }
            if (isRead == false)
            {
                //PostAlarm((int)AlarmKeys.Alarm_BarcodeReadingFailed);
                Log.Write(UnitName, "GetBarcode", "Barcode Reading Failed");
                nRet = -1;
                return nRet;
            }

            return nRet;
        }

        public int UnloadBinStageToFeeder(bool isFine = false)
        {
            int nRet = 0;
            nRet = UnClampGripper();
            if (nRet != 0)
            {
                PostAlarm((int)AlarmKeys.Alarm_GripperClampFailed);
                Log.Write(this, "UnClampGripper Failed");
                nRet = -1;
                return nRet;
            }

            nRet = DownFeeder();
            if (nRet != 0)
            {
                PostAlarm((int)AlarmKeys.Alarm_FeederClampUpDown);
                Log.Write(this, "DownFeeder Failed");
                nRet = -1;
                return nRet;
            }

            nRet = MovePositionStage(isFine);
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_BinUnloadingFailed);
                Log.Write(this, "MovePositionStage Failed");
                nRet = -1;
                return nRet;
            }
            return nRet;
        }
        public int UnloadBinStagetToFeeder(bool isFine = false)
        {
            int nRet = 0;

            nRet = UnClampGripper();
            if (nRet != 0)
            {
                PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                Log.Write(this, "UnClampGripper Failed");
                nRet = -1;
                return nRet;
            }

            nRet = DownFeeder();
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                Log.Write(this, "DownFeeder Failed");
                nRet = -1;
                return nRet;
            }

            nRet = MovePositionStage(isFine);
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                Log.Write(this, "MovePositionStage Failed");
                nRet = -1;
                return nRet;
            }

            return nRet;
        }
        public bool IsInterlockOKWithCassete()
        {
            bool bRtn = true;
            double dYSafePosOffset = Config.dFeederToCassetteOverapLength;
            if (Config.IsSimulation == false)
            {
                if (IsClamped())
                {
                    dYSafePosOffset += Config.dWaferRingframeSize;
                }
            }
               
            var tp = TeachingPositions[(int)OutputFeederConfig.TeachingPositionName.Cassette];
            double dInterlockPos = tp.GetAxisPosition(this.AxisOutputFeederY.Name);

            dInterlockPos += dYSafePosOffset;
            if (AxisOutputFeederY.GetPosition() < dInterlockPos)
            {
                Log.Write(this.UnitName, "IsInterlockOKWithCassete", 
                $"FeederY Position Low. Current:" +
                $"{AxisOutputFeederY.GetPosition()}, InterlockPos:{dInterlockPos}");
                
                bRtn = false;
                return bRtn;
            }

            return bRtn;

        }
        #endregion

        #region Ready
        public int EnsureReady(bool isFine = false)
        {
            Task<int> task = EnsureReadyAsync(isFine);
            while (IsEndTask(task) == false)
            {
                Thread.Sleep(1);
            }
            return task.Result;
        }
        private Task<int> EnsureReadyAsync(bool isFine = false)
        {
            return Task.Run(() =>
            {
                OnEnsureReady(isFine);
                return 0;
            });
        }
        private int OnEnsureReady(bool isFine)
        {
            int nRet = 0;
            // Fast path: 이미 Ready + Up + Unclamp면 바로 OK
            try
            {
                if (IsPositionReady() && IsFeederUp() && IsUnClamped())
                {
                    return 0;
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }

            // --- Simulation 모드: 축 위치가 0(초기 상태) 이면 teaching 여부와 무관하게 OK 처리 ---
            if (Config != null
                && Config.IsSimulation)
            {
                if (AxisOutputFeederY != null)
                {
                    double pos = 0;
                    try { pos = AxisOutputFeederY.GetPosition(); } catch { }
                    if (Math.Abs(pos) < AxisOutputFeederY.Config.InposTolerance) // 필요 시 공차 Config 로 분리 가능
                    {
                        nRet = MovePositionReady();
                        if (nRet != 0)
                        {
                            Log.Write(UnitName, "CheckReady Fail - MovePositionReady");
                            return nRet;
                        }

                        Log.Write(UnitName, "Simulation - AxisFeederY Position 0 → Ready 통과 (NoPosition 체크 생략)");
                        return nRet; // 바로 OK
                    }
                }
            }

            if (IsPositionBarcode() == false &&
                IsPositionCassette() == false &&
                IsPositionStage() == false &&
                IsPositionReady() == false &&
                IsPositionSetPos() == false)
            {
                PostAlarm((int)AlarmKeys.Alarm_OutputFeederNoPosition);
                Log.Write(UnitName, "OnEnsureReady Fail - No Position");
                return -1;
            }

            if(IsPositionSetPos())
            {
                if (IsRingPresent() || IsClamped())
                {
                    nRet = UnClampGripper();
                    if (nRet != 0)
                    {
                        Log.Write(UnitName, "CheckReady Fail - UnClampGripper");
                        return nRet;
                    }
                }

                nRet = MovePositionReady();
                if (nRet != 0)
                {
                    Log.Write(UnitName, "CheckReady Fail - MovePositionReady");
                    return nRet;
                }

                if (!IsFeederUp())
                {
                    nRet = UpFeeder();
                    if (nRet != 0)
                    {
                        Log.Write(UnitName, "CheckReady Fail - UpFeeder");
                        return nRet;
                    }
                }

                return 0;
            }

            // Stage interlock must be OK (if Stage is present)
            if(OutputStage.IsPositionBinLoading() == false
               && OutputStage.IsPositionBinUnloading() == false)
            {
                AxisOutputFeederY?.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_OutputStageInterlockFailed);
                Log.Write(UnitName, "CheckReady Fail - OutputStage.IsStageInterLockOK false");
                return -1;
            }

            // At other TP → safety checks then move Ready
            // Feeder에서는 막았는데.. 카세트가 움직일때 
            // 제품 위치를 보기 위해서 아래 인터락 사용이지.
            //if (!IsInterlockOKWithCassete())
            //{
            //    PostAlarm((int)AlarmKeys.Alarm_OutputFeederInterlockFailed);
            //    Log.Write(UnitName, "CheckReady Fail - IsInterlockOKWithCassete");
            //    return -1;
            //}

            bool stageAtSafe = (OutputStage == null) ||
                               OutputStage.IsPositionBinLoading() ||
                               OutputStage.IsPositionBinUnloading();
            if (!stageAtSafe)
            {
                PostAlarm((int)AlarmKeys.Alarm_OutputStageInterlockFailed);
                Log.Write(UnitName, "CheckReady Fail - OutputStage not at BinLoading/Unloading");
                return -1;
            }

            //Barcode 위치에서 멈춘 경우.
            if (IsPositionBarcode())
            {
                if (OutputCassetteLifter.IsAnyAxisMoving())
                {
                    OutputCassetteLifter.BinLifterZ.EmgStop();
                }
                if (IsFeederDown())
                {
                    if (IsRingPresent())
                    {
                        if (IsUnClamped() == false || IsUnClamped() == true)
                        {
                            nRet = MovePositionCassette();
                            if (nRet != 0)
                            {
                                AxisOutputFeederY.EmgStop();
                                PostAlarm((int)AlarmKeys.Alarm_OutputStageInterlockFailed);
                                Log.Write(UnitName, "CheckReady Fail - OutputStage.IsStageInterLockOK");
                                return -1;
                            }
                            nRet = UnClampGripper();
                            if (nRet != 0)
                            {
                                Log.Write(UnitName, "CheckReady Fail - UnClampGripper");
                                return nRet;
                            }
                            nRet = MovePositionReady();
                            if (nRet != 0)
                            {
                                AxisOutputFeederY.EmgStop();
                                PostAlarm((int)AlarmKeys.Alarm_OutputStageInterlockFailed);
                                Log.Write(UnitName, "CheckReady Fail - OutputStage.IsStageInterLockOK");
                                return -1;
                            }
                        }
                        else
                        {
                            AxisOutputFeederY.EmgStop();
                            PostAlarm((int)AlarmKeys.Alarm_OutputStageInterlockFailed);
                            Log.Write(UnitName, "CheckReady Fail - OutputStage.IsStageInterLockOK");
                            return -1;
                        }
                    }
                    else
                    {

                        AxisOutputFeederY.EmgStop();
                        PostAlarm((int)AlarmKeys.Alarm_OutputStageInterlockFailed);
                        Log.Write(UnitName, "CheckReady Fail - OutputStage.IsStageInterLockOK");
                        return -1;
                    }
                }
                else
                {
                    AxisOutputFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_OutputStageInterlockFailed);
                    Log.Write(UnitName, "CheckReady Fail - OutputStage.IsStageInterLockOK");
                    return -1;
                }
            }

            if (IsRingPresent() || IsClamped())
            {
                nRet = UnClampGripper();
                if (nRet != 0)
                {
                    Log.Write(UnitName, "CheckReady Fail - UnClampGripper");
                    return nRet;
                }
            }

            nRet = MovePositionReady();
            if (nRet != 0)
            {
                Log.Write(UnitName, "CheckReady Fail - MovePositionReady");
                return nRet;
            }

            if (!IsFeederUp())
            {
                nRet = UpFeeder();
                if (nRet != 0)
                {
                    Log.Write(UnitName, "CheckReady Fail - UpFeeder");
                    return nRet;
                }
            }

            return 0;
        }
        #endregion

        // 클래스 내부에 추가
        public void ResetForNewRun(bool moveToSafeReady = true, bool clearMaterial = true, bool resetDryRunFlags = true)
        {
            // 재시작 시 잔류 센서와 객체 불일치 강제 정리
            try
            {
                if (!Config.IsSimulation && !Config.IsDryRun)
                {
                    if (!IsRingPresent() && GetMaterial() is MaterialWafer)
                    {
                        Log.Write(UnitName, "[ResetForNewRun] Sensor OFF but material object existed -> cleared");
                        SetMaterial(null);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(UnitName, "[ResetForNewRun] Consistency clear failed: " + ex.Message);
            }

            // 1) 런타임/시퀀스 플래그 초기화
            //_isSafetyMoving = false;
            //CurrentFunc = null;
            //NeedUnloadFirst = false;
            _exchangeStandbyForNextLoad = false;
            UnitDryRunTest = false;
            _loadStep = LoadFlowStep.None; // 추가: 단계 초기화
            _unloadStep = UnloadFlowStep.None;
            _unloadTargetSlot = -1;

            if (resetDryRunFlags)
            {
                _dryLoadedToStage = false;
                _dryLastSlotIndex = -1;
            }

            // 2) 보유 머티리얼 정리(선택)
            if (clearMaterial)
            {
                try { this.SetMaterial(null); }
                catch (Exception ex) { Log.Write(UnitName, $"[ResetForNewRun] Clear material failed: {ex.Message}"); }
            }

            // 3) 인접 유닛 정지 대기(선택)
            if (moveToSafeReady)
            {
                try
                {
                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    const int timeoutMs = 10000;
                    while ((OutputStage?.IsAnyAxisMoving() ?? false) || (OutputCassetteLifter?.IsAnyAxisMoving() ?? false))
                    {
                        if (IsStop) return;
                        if (sw.ElapsedMilliseconds > timeoutMs) break;
                        Thread.Sleep(1);
                    }
                }
                catch (Exception ex)
                {
                    Log.Write(UnitName, $"[ResetForNewRun] Wait neighbor units idle failed: {ex.Message}");
                }
            }

            // 4) 안전/Ready 복귀(선택)
            if (moveToSafeReady)
            {
                try
                {
                    // EnsureReady는 필요 시:
                    // - 위치 무정(Barcode/Cassette/Stage/Ready 아님) → 알람
                    // - Cassette/Stage/Barcode에 있을 때 인터락 검증 후 Ready 이동
                    // - Unclamp/Feeder Up 수행
                    int rc = EnsureReady();
                    if (rc != 0)
                    {
                        Log.Write(UnitName, "[ResetForNewRun] EnsureReady failed");
                    }
                }
                catch (Exception ex)
                {
                    Log.Write(UnitName, $"[ResetForNewRun] EnsureReady exception: {ex.Message}");
                }
            }
        }


        // [추가] 클래스 내부(필드/속성 영역)에 배치
        public enum BinMapOrigin { BottomLeft, BottomRight, TopLeft, TopRight }
        
        // 180도일때 사용
        //public BinMapOrigin OutputBinOrigin { get; set; } = BinMapOrigin.BottomLeft;
        public BinMapOrigin OutputBinOrigin { get; set; } = BinMapOrigin.BottomRight; // InputStage와 보통 동일
        public bool OutputBinMirrorX { get; set; } = false;
        public bool OutputBinMirrorY { get; set; } = false;
        public void ToBinCoord(int gx, int gy, int cntX, int cntY, out int bx, out int by)
        {
            switch (OutputBinOrigin)
            {
                case BinMapOrigin.BottomLeft: bx = gx; by = gy; break;
                case BinMapOrigin.BottomRight: bx = (cntX - 1 - gx); by = gy; break;
                case BinMapOrigin.TopLeft: bx = gx; by = (cntY - 1 - gy); break;
                case BinMapOrigin.TopRight: bx = (cntX - 1 - gx); by = (cntY - 1 - gy); break;
                default: bx = gx; by = gy; break;
            }
            if (OutputBinMirrorX) bx = (cntX - 1 - bx);
            if (OutputBinMirrorY) by = (cntY - 1 - by);
        }



        // [NEW] 경로 베이스 고정: 최초 1회만 레시피에서 캡처
        private bool _binPathBaseLocked;
        private MapPathStartCorner _binPathBaseCorner = MapPathStartCorner.BottomLeft;
        private MapPathPrimaryAxis _binPathBaseAxis = MapPathPrimaryAxis.XFirst;

        private void LockBinPathBaseFromRecipeOnce()
        {
            if (_binPathBaseLocked) return;
            var recipe = Equipment.Instance?.EquipmentRecipe?.CurrentRecipe as MeasurementRecipe;
            if (recipe == null) return;

            _binPathBaseCorner = recipe.BinPathStartCorner; //MapPathStartCorner.BottomLeft;
            _binPathBaseAxis = recipe.BinPathPrimaryAxis;
            _binPathBaseLocked = true;
        }

        // [UPDATED] InputStage 맵 복제: 회전/미러만 적용, 이후 베이스 코너/주축으로 순서 고정
        private int CopyInputMapRotate180(MaterialWafer srcWafer,
                                          MaterialWafer dstWafer,
                                          MeasurementRecipe.MapRotateOption rotate,
                                          MeasurementRecipe.MapMirrorOption mirror)
        {
            try
            {
                if (srcWafer == null || srcWafer.Dies == null || srcWafer.Dies.Count == 0)
                    return -1;
                if (dstWafer == null)
                    return -2;

                // 최초 1회만 베이스 경로 고정
                LockBinPathBaseFromRecipeOnce();

                lock (srcWafer.Dies)
                {
                    lock (dstWafer.Dies)
                    {
                        var sourceDies = srcWafer.Dies.Where(d => d != null).ToList();
                        if (sourceDies.Count == 0)
                            return -3;

                        const double tol = 1e-6;
                        var xs = sourceDies.Select(d => (double)d.MapX).OrderBy(v => v).Aggregate(new List<double>(), (acc, v) =>
                        {
                            if (acc.Count == 0 || Math.Abs(acc[acc.Count - 1] - v) > tol) acc.Add(v);
                            return acc;
                        });
                        var ys = sourceDies.Select(d => (double)d.MapY).OrderBy(v => v).Aggregate(new List<double>(), (acc, v) =>
                        {
                            if (acc.Count == 0 || Math.Abs(acc[acc.Count - 1] - v) > tol) acc.Add(v);
                            return acc;
                        });
                        if (xs.Count == 0 || ys.Count == 0)
                            return -3;

                        int nx = xs.Count, ny = ys.Count;
                        int FindIndex(List<double> list, double value)
                        {
                            int lo = 0, hi = list.Count - 1;
                            while (lo <= hi)
                            {
                                int mid = (lo + hi) / 2;
                                double diff = list[mid] - value;
                                if (Math.Abs(diff) <= tol) return mid;
                                if (diff < 0) lo = mid + 1; else hi = mid - 1;
                            }
                            for (int i = 0; i < list.Count; i++)
                                if (Math.Abs(list[i] - value) <= tol)
                                    return i;
                            return -1;
                        }

                        if (dstWafer.Dies != null) dstWafer.Dies.Clear();
                        dstWafer.Dies = new List<MaterialDie>(sourceDies.Count);

                        // 좌표는 회전/미러 반영, BinX/Y는 인덱스 변환만 반영
                        int newIndex = 0;
                        foreach (var s in sourceDies)
                        {
                            int ix = FindIndex(xs, s.MapX);
                            int iy = FindIndex(ys, s.MapY);
                            if (ix < 0 || iy < 0) continue;

                            var rxy = ApplyRotateToIndex(ix, iy, nx, ny, rotate);
                            var mxy = ApplyMirrorToIndex(rxy.tx, rxy.ty, nx, ny, mirror);

                            // 실제 좌표 회전/미러는 간단히 인덱스 변환 결과를 MapX/MapY에 반영(그리드 기반)
                            double newMapX = xs[mxy.tx];
                            double newMapY = ys[mxy.ty];

                            dstWafer.Dies.Add(new MaterialDie
                            {
                                Index = newIndex++,
                                Presence = Material.MaterialPresence.NotExist,
                                ProcessSatate = Material.MaterialProcessSatate.Unknown,
                                BinX = mxy.tx,
                                BinY = mxy.ty,
                                MapX = (int)Math.Round(newMapX),
                                MapY = (int)Math.Round(newMapY)
                            });
                        }
                        return 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(UnitName, "CopyInputMapRotate180", ex.Message);
                return -9;
            }
        }

        // [NEW] Rotate/Mirror만 반영해 격자 방향 변환 (인덱스 키 조회용)
        private static (int tx, int ty) ApplyRotateToIndex(int ix, int iy, int nx, int ny, MapRotateOption r)
        {
            switch (r)
            {
                case MapRotateOption.CW90: return (ny - 1 - iy, ix);
                case MapRotateOption.CW180: return (nx - 1 - ix, ny - 1 - iy);
                case MapRotateOption.CW270: return (iy, nx - 1 - ix);
                default: return (ix, iy);
            }
        }
        private static (int tx, int ty) ApplyMirrorToIndex(int ix, int iy, int nx, int ny, MapMirrorOption m)
        {
            switch (m)
            {
                case MapMirrorOption.X: return (nx - 1 - ix, iy);
                case MapMirrorOption.Y: return (ix, ny - 1 - iy);
                case MapMirrorOption.XY: return (nx - 1 - ix, ny - 1 - iy);
                default: return (ix, iy);
            }
        }


        // [UPDATED] 순서 재정렬: 회전/미러는 버킷 키 변환에만 반영, 순회 방향은 베이스 코너/주축으로만 결정
        private void OrderDiesByMode(MaterialWafer wafer)
        {
            if (wafer?.Dies == null || wafer.Dies.Count == 0) return;

            LockBinPathBaseFromRecipeOnce();

            lock (wafer.Dies)
            {
                var items = wafer.Dies.Select(d => new { Die = d, BX = (int)Math.Round(d.BinX), BY = (int)Math.Round(d.BinY) }).ToList();
                var xs = items.Select(i => i.BX).Distinct().OrderBy(v => v).ToList();
                var ys = items.Select(i => i.BY).Distinct().OrderBy(v => v).ToList();
                if (xs.Count == 0 || ys.Count == 0) { Log.Write(UnitName, "OrderDiesByMode", "No valid BinX/BinY values."); return; }

                // 회전/미러 읽기
                var recipe = Equipment.Instance.EquipmentRecipe.CurrentRecipe as MeasurementRecipe;
                var rotate = recipe?.BinRotate ?? MapRotateOption.None;
                var mirror = recipe?.BinMirror ?? MapMirrorOption.None;

                // 버킷: 회전/미러를 적용한 키로 구성
                var buckets = new Dictionary<(int bx, int by), List<MaterialDie>>();
                foreach (var it in items)
                {
                    var rxy = ApplyRotateToIndex(xs.IndexOf(it.BX), ys.IndexOf(it.BY), xs.Count, ys.Count, rotate);
                    var mxy = ApplyMirrorToIndex(rxy.tx, rxy.ty, xs.Count, ys.Count, mirror);
                    var key = (xs[mxy.tx], ys[mxy.ty]);
                    if (!buckets.TryGetValue(key, out var list))
                    {
                        list = new List<MaterialDie>();
                        buckets[key] = list;
                    }
                    list.Add(it.Die);
                }

                // 순회 방향은 “베이스 코너/주축”으로만 결정
                List<int> xBase, yBase;
                switch (_binPathBaseCorner)
                {
                    default:
                    case MapPathStartCorner.BottomLeft: xBase = xs; yBase = ys; break;
                    case MapPathStartCorner.BottomRight: xBase = xs.AsEnumerable().Reverse().ToList(); yBase = ys; break;
                    case MapPathStartCorner.TopLeft: xBase = xs; yBase = ys.AsEnumerable().Reverse().ToList(); break;
                    case MapPathStartCorner.TopRight: xBase = xs.AsEnumerable().Reverse().ToList(); yBase = ys.AsEnumerable().Reverse().ToList(); break;
                }

                var newList = new List<MaterialDie>(wafer.Dies.Count);
                var traversal = recipe?.BinPathTraversalMode ?? MapPathTraversalMode.Serpentine;

                if (_binPathBaseAxis == MapPathPrimaryAxis.XFirst)
                {
                    for (int row = 0; row < yBase.Count; row++)
                    {
                        int by = yBase[row];
                        IEnumerable<int> xSeq = xBase;
                        if (traversal == MapPathTraversalMode.Serpentine && (row % 2 == 1))
                            xSeq = xBase.AsEnumerable().Reverse();

                        foreach (int bx in xSeq)
                        {
                            if (buckets.TryGetValue((bx, by), out var list))
                                newList.AddRange(list.OrderBy(d => d.Index));
                        }
                    }
                }
                else
                {
                    for (int col = 0; col < xBase.Count; col++)
                    {
                        int bx = xBase[col];
                        IEnumerable<int> ySeq = yBase;
                        if (traversal == MapPathTraversalMode.Serpentine && (col % 2 == 1))
                            ySeq = yBase.AsEnumerable().Reverse();

                        foreach (int by in ySeq)
                        {
                            if (buckets.TryGetValue((bx, by), out var list))
                                newList.AddRange(list.OrderBy(d => d.Index));
                        }
                    }
                }

                wafer.Dies = newList;
            }
        }

        // [UPDATED] TryCloneMapFromInputStage: 복제 → 순서 재정렬(베이스 코너/주축) → Index 재설정
        private bool TryCloneMapFromInputStage(MaterialWafer dstWafer)
        {
            try
            {
                var inputStage = Equipment.Instance.GetUnit("InputStage") as InputStage;
                var srcWafer = inputStage?.GetMaterialWafer();
                if (srcWafer == null || srcWafer.Dies == null || srcWafer.Dies.Count == 0)
                {
                    Log.Write(UnitName, "TryCloneMapFromInputStage", "srcWafer empty");
                    return false;
                }

                lock (srcWafer.Dies)
                {
                    lock (dstWafer.Dies)
                    {
                        if (dstWafer.Dies != null) dstWafer.Dies.Clear();
                        dstWafer.Dies = new List<MaterialDie>(srcWafer.Dies.Count);

                        var recipe = Equipment.Instance.EquipmentRecipe.CurrentRecipe as MeasurementRecipe;
                        var rotate = recipe?.BinRotate ?? MapRotateOption.None;
                        var mirror = recipe?.BinMirror ?? MapMirrorOption.None;

                        int rc = CopyInputMapRotate180(srcWafer, dstWafer, rotate, mirror);
                        if (rc != 0)
                        {
                            Log.Write(UnitName, "MakePath", $"Clone failed rc={rc}");
                            return false;
                        }

                        // 순서 고정(베이스) 적용 후 Index 재설정
                        OrderDiesByMode(dstWafer);
                        for (int i = 0; i < dstWafer.Dies.Count; i++)
                            dstWafer.Dies[i].Index = i;

                        Log.Write(UnitName, "MakePath", $"Cloned from InputStage. Count={dstWafer.Dies.Count} Rotate={rotate} Mirror={mirror}");
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Write(UnitName, "TryCloneMapFromInputStage", ex.Message);
                return false;
            }
        }

        // OutputFeeder 클래스 내부: 경로/맵 관련 속성 근처에 추가
        // === Bin 맵 생성 파라미터 (InputStage와 동일 개념) ===

        public bool UseCircularBinMap { get; set; } = true;          // 원형(웨이퍼) 형태로 배치
        public bool UseChipPitchForBinCount { get; set; } = true;    // ChipPitch로 격자 개수 산정
        public double BinCircleMarginMm { get; set; } = 0.0;         // 경계 포함 여유(mm)

        // Recipe의 Chip 크기를 그대로 사용 (InputStage와 동일 방식)
        public double ChipPitchXmm
        {
            get
            {
                var eq = Equipment.Instance;
                var r = eq.EquipmentRecipe.CurrentRecipe;
                return (r.WChipPitchX > 0) ? r.WChipPitchX : 0.5; // fallback
            }
        }
        public double ChipPitchYmm
        {
            get
            {
                var eq = Equipment.Instance;
                var r = eq.EquipmentRecipe.CurrentRecipe;
                return (r.WChipPitchY > 0) ? r.WChipPitchY : 0.5; // fallback
            }
        }
        // Output Bin의 유효 지름(mm). 별도 항목이 없으면 웨이퍼 지름을 사용
        public double BinDiameterMm
        {
            get
            {
                var eq = Equipment.Instance;
                var r = eq.EquipmentRecipe.CurrentRecipe;
                return (r.WaferDiameter > 0) ? r.WaferDiameter : 0.0;
            }
        }

        // === Bin 맵 생성 파라미터 (InputStage와 동일 개념) ===
        // ... 기존 필드들 바로 근처에 추가 ...
        public bool PreferCloneMapFromInputStage { get; set; } = true;  // InputStage 맵이 있으면 우선 복제

        // 기존 MakePath 교체(상단부 로직만 변경, 나머지 생성 로직은 동일 유지)
        public int MakePath()
        {
            int nRet = 0;
            var Bin = this.GetMaterial() as MaterialWafer;
            if (Bin == null)
                return nRet;

            // 경로가 없을 때만 생성
            bool needPath = (Bin.Dies == null || Bin.Dies.Count == 0);
            if (!(Bin.ProcessSatate == Material.MaterialProcessSatate.Ready
                  || Bin.ProcessSatate == Material.MaterialProcessSatate.Processing))
            {
                return nRet;
            }
            lock (Bin.Dies)
            {
                if (!needPath)
                    return nRet;

                if (Bin.Dies != null)
                    Bin.Dies.Clear();

                Bin.Dies = new List<MaterialDie>();
                try
                {
                    Equipment.Instance.bIndexCal = true;
                    if (Equipment.Instance.bIndexCal == true)
                    {
                        // 0) InputStage 맵을 우선 그대로 복제(개수/격자/좌표 일치 보장)
                        if (PreferCloneMapFromInputStage && TryCloneMapFromInputStage(Bin))
                        {
                            // 복제 성공 시 여기서 종료 → InputStage에서 도출된 칩 개수와 완전 동일
                            return 0;
                        }
                    }

                    // 1) (Fallback) ChipPitch + 웨이퍼 지름 기반 원형 맵 생성
                    var recipe = Equipment.Instance.EquipmentRecipe.CurrentRecipe;
                    double pitchX = ChipPitchXmm;
                    double pitchY = ChipPitchYmm;
                    if (pitchX <= 0) pitchX = 0.5;
                    if (pitchY <= 0) pitchY = 0.5;

                    double diameterMm = BinDiameterMm;
                    int nCoutX = 5;
                    int nCoutY = 5;
                    //if (diameterMm <= 0 && (recipe.BinCountX > 0 || recipe.BinCountY > 0))
                    //{
                    //    double spanX = Math.Max(1, recipe.BinCountX) * pitchX;
                    //    double spanY = Math.Max(1, recipe.BinCountY) * pitchY;
                    //    diameterMm = Math.Min(spanX, spanY);
                    //}
                    if (diameterMm <= 0 && (nCoutX > 0 || nCoutY > 0))
                    {
                        double spanX = Math.Max(1, nCoutX) * pitchX;
                        double spanY = Math.Max(1, nCoutY) * pitchY;
                        diameterMm = Math.Min(spanX, spanY);
                    }
                    if (diameterMm <= 0)
                    {
                        diameterMm = Math.Min(20 * pitchX, 20 * pitchY);
                    }

                    double radiusMm = Math.Max(0.0, diameterMm / 2.0 - Math.Max(0.0, BinCircleMarginMm));

                    int halfCellsX = (int)Math.Floor(radiusMm / pitchX);
                    int halfCellsY = (int)Math.Floor(radiusMm / pitchY);
                    int cntX = Math.Max(1, halfCellsX * 2 + 1);
                    int cntY = Math.Max(1, halfCellsY * 2 + 1);

                    double centerX = (cntX - 1) / 2.0;
                    double centerY = (cntY - 1) / 2.0;
                    int xStart, yStart, xDir, yDir;

                    var Recipe = Equipment.Instance.EquipmentRecipe.CurrentRecipe as MeasurementRecipe;
                    var StartCorner = Recipe != null ? Recipe.BinPathStartCorner : MapPathStartCorner.BottomLeft;
                    var PrimaryAxis = Recipe != null ? Recipe.BinPathPrimaryAxis : MapPathPrimaryAxis.XFirst;
                    var Traversal = Recipe != null ? Recipe.BinPathTraversalMode : MapPathTraversalMode.Serpentine;
                    switch (StartCorner)
                    {
                        default:
                        case MapPathStartCorner.BottomLeft: xStart = 0; yStart = 0; xDir = +1; yDir = +1; break;
                        case MapPathStartCorner.BottomRight: xStart = cntX - 1; yStart = 0; xDir = -1; yDir = +1; break;
                        case MapPathStartCorner.TopLeft: xStart = 0; yStart = cntY - 1; xDir = +1; yDir = -1; break;
                        case MapPathStartCorner.TopRight: xStart = cntX - 1; yStart = cntY - 1; xDir = -1; yDir = -1; break;
                    }

                    IEnumerable<int> RangeDir(int start, int count, int dir)
                    {
                        if (dir > 0) { for (int i = 0; i < count; i++) yield return start + i; }
                        else { for (int i = 0; i < count; i++) yield return start - i; }
                    }

                    var xLineForward = RangeDir(xStart, cntX, xDir).ToList();
                    var xLineReverse = xLineForward.AsEnumerable().Reverse().ToList();
                    var yLineForward = RangeDir(yStart, cntY, yDir).ToList();
                    var yLineReverse = yLineForward.AsEnumerable().Reverse().ToList();

                    var list = new List<MaterialDie>();
                    Action<int, int> tryAdd = (rawX, rawY) =>
                    {
                        int bx, by;
                        ToBinCoord(rawX, rawY, cntX, cntY, out bx, out by);

                        double relCellX = bx - centerX;
                        double relCellY = by - centerY;

                        double dxMm = relCellX * pitchX;
                        double dyMm = relCellY * pitchY;
                        double dist2 = dxMm * dxMm + dyMm * dyMm;
                        bool inside = !UseCircularBinMap ? true : (dist2 <= radiusMm * radiusMm);

                        if (!inside)
                        {
                            Log.Write(UnitName, "MakePath",
                                $"Skip outside cell: Bin=({bx},{by}) " +
                                $"Map=({(int)relCellX},{(int)relCellY}) " +
                                $"Dist2={dist2:F3}mm² " +
                                $"Radius²={radiusMm * radiusMm:F3}mm²");
                            return;
                        }

                        list.Add(new MaterialDie
                        {
                            Index = -1,
                            Presence = Material.MaterialPresence.NotExist,
                            ProcessSatate = Material.MaterialProcessSatate.Unknown,
                            BinX = bx,
                            BinY = by,
                            MapX = (int)relCellX,
                            MapY = (int)relCellY
                        });
                    };

                    if (PrimaryAxis == MapPathPrimaryAxis.XFirst)
                    {
                        for (int row = 0; row < cntY; row++)
                        {
                            int rawY = yLineForward[row];
                            var xSeq = (Traversal == MapPathTraversalMode.Serpentine && (row % 2 == 1))
                                ? xLineReverse
                                : xLineForward;

                            foreach (int rawX in xSeq)
                                tryAdd(rawX, rawY);
                        }
                    }
                    else if (PrimaryAxis == MapPathPrimaryAxis.YFirst)
                    {
                        for (int col = 0; col < cntX; col++)
                        {
                            int rawX = xLineForward[col];
                            var ySeq = (Traversal == MapPathTraversalMode.Serpentine && (col % 2 == 1))
                                ? yLineReverse
                                : yLineForward;

                            foreach (int rawY in ySeq)
                            {
                                tryAdd(rawX, rawY);
                            }
                        }
                    }

                    for (int i = 0; i < list.Count; i++)
                    {
                        list[i].Index = i;
                    }
                    Bin.Dies.AddRange(list);

                    Log.Write(UnitName, "MakePath",
                        $"Circular(Fallback)={UseCircularBinMap} " +
                        $"Dies={Bin.Dies.Count} " +
                        $"Grid=({cntX}x{cntY}) " +
                        $"Pitch=({pitchX:F3},{pitchY:F3})mm " +
                        $"Radius={radiusMm:F3}mm");
                }
                catch (Exception ex)
                {
                    Log.Write(UnitName, "MakePath", "Exception: " + ex.Message);
                }
            }
            return nRet;
        }


        private int ReadBarcodeWithRetry(out string barcode, bool isFine)
        {
            barcode = string.Empty;
            int nRet = 0;

            try
            {
                // 1) 바코드 위치로 이동
                nRet = BarcodeReading(isFine);
                if (nRet != 0)
                {
                    Log.Write(UnitName, "BinLoading", "BarcodeReading Failed");
                    return nRet;
                }

                // 시뮬/드라이런은 기존 GetBarcode() 로직 그대로 사용
                if (Config.IsSimulation || Config.IsDryRun)
                {
                    return GetBarcode(out barcode);
                }

                bool useTrigger = false;
                try
                {
                    if (OutputCassetteLifter != null && OutputCassetteLifter.IsTriggerModeConfigured() == true)
                        useTrigger = true;
                }
                catch { useTrigger = false; }

                // 2) Trigger 모드
                if (useTrigger)
                {
                    int tOn = OutputCassetteLifter.EnsureTriggerOn();
                    if (tOn != 0)
                    {
                        Log.Write(UnitName, "BinLoading", "Auto-Trigger On Failed → fallback to polling");
                        useTrigger = false;
                    }
                    else
                    {
                        OutputCassetteLifter.ClearBarcodeBuffer();

                        // 기준 위치에서 1차 대기
                        if (OutputCassetteLifter.WaitBarcode(out barcode, timeoutMs: 500) == 0 && !string.IsNullOrEmpty(barcode))
                        {
                            return 0;
                        }

                        // Y축 ±스캔
                        const double scanStep = 1.0;
                        const int scanPairs = 5;
                        const int settleMs = 50;

                        double basePosY = 0;
                        try
                        {
                            basePosY = this.GetTP(OutputFeederConfig.TeachingPositionName.Barcode.ToString(), this.AxisOutputFeederY.Name);
                        }
                        catch
                        {
                            basePosY = AxisOutputFeederY.GetPosition();
                        }

                        for (int i = 1; i <= scanPairs; i++)
                        {
                            // +offset
                            double targetPlus = basePosY + (scanStep * i);
                            if (IsMoveInterLockBarcode() != 0)
                            {
                                AxisOutputFeederY.EmgStop();
                                PostAlarm((int)AlarmKeys.Alarm_OutputStageInterlockFailed);
                                Log.Write(UnitName, "BinLoading", "Barcode scan interlock fail (+offset)");
                                return -1;
                            }

                            nRet = MoveAxisPositionOne(AxisOutputFeederY, targetPlus, isFine);
                            if (nRet != 0)
                            {
                                AxisOutputFeederY.EmgStop();
                                Log.Write(UnitName, "BinLoading", "Move Y +offset fail during barcode scan (trigger)");
                                return -1;
                            }
                            Thread.Sleep(settleMs);

                            if (OutputCassetteLifter.WaitBarcode(out barcode, timeoutMs: 500) == 0 && !string.IsNullOrEmpty(barcode))
                                break;

                            // -offset
                            double targetMinus = basePosY - (scanStep * i);
                            if (IsMoveInterLockBarcode() != 0)
                            {
                                AxisOutputFeederY.EmgStop();
                                PostAlarm((int)AlarmKeys.Alarm_OutputStageInterlockFailed);
                                Log.Write(UnitName, "BinLoading", "Barcode scan interlock fail (-offset)");
                                return -1;
                            }

                            nRet = MoveAxisPositionOne(AxisOutputFeederY, targetMinus, isFine);
                            if (nRet != 0)
                            {
                                AxisOutputFeederY.EmgStop();
                                Log.Write(UnitName, "BinLoading", "Move Y -offset fail during barcode scan (trigger)");
                                return -1;
                            }
                            Thread.Sleep(settleMs);

                            if (OutputCassetteLifter.WaitBarcode(out barcode, timeoutMs: 500) == 0 && !string.IsNullOrEmpty(barcode))
                                break;
                        }

                        // 스캔 종료 후 기준 위치 복귀
                        try
                        {
                            nRet = BarcodeReading(isFine);
                            if (nRet != 0)
                            {
                                Log.Write(UnitName, "BinLoading", "BarcodeReading Failed (return to base)");
                                return nRet;
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Write(ex);
                        }

                        if (string.IsNullOrEmpty(barcode))
                        {
                            AxisOutputFeederY.EmgStop();
                            PostAlarm((int)AlarmKeys.Alarm_BarcodeReadingFailed);
                            Log.Write(UnitName, "BinLoading", "Barcode Reading Failed after trigger scan");
                            return -1;
                        }

                        return 0;
                    }
                }

                // 3) Polling(GetBarcode) + Y축 ±스캔 재시도 (Trigger 폴백 포함)
                nRet = GetBarcode(out barcode);
                if (nRet != 0 || string.IsNullOrEmpty(barcode))
                {
                    const double scanStep = 1.0;
                    const int scanPairs = 6;
                    const int settleMs = 50;

                    double basePosY = 0;
                    try
                    {
                        basePosY = this.GetTP(OutputFeederConfig.TeachingPositionName.Barcode.ToString(), this.AxisOutputFeederY.Name);
                    }
                    catch (Exception ex)
                    {
                        basePosY = AxisOutputFeederY.GetPosition();
                        Log.Write(ex);
                    }

                    // 기준 위치에서 1회 더 시도
                    Thread.Sleep(settleMs);
                    nRet = GetBarcode(out barcode);

                    // 왕복 스캔
                    if (nRet != 0 || string.IsNullOrEmpty(barcode))
                    {
                        for (int i = 1; i <= scanPairs; i++)
                        {
                            // +offset
                            double targetPlus = basePosY + (scanStep * i);
                            if (IsMoveInterLockBarcode() != 0)
                            {
                                AxisOutputFeederY.EmgStop();
                                PostAlarm((int)AlarmKeys.Alarm_OutputStageInterlockFailed);
                                Log.Write(UnitName, "BinLoading", "Barcode scan interlock fail (+offset)");
                                return -1;
                            }

                            nRet = MoveAxisPositionOne(AxisOutputFeederY, targetPlus, isFine);
                            if (nRet != 0)
                            {
                                AxisOutputFeederY.EmgStop();
                                Log.Write(UnitName, "BinLoading", "Move Y +offset fail during barcode scan");
                                return -1;
                            }
                            Thread.Sleep(settleMs);

                            nRet = GetBarcode(out barcode);
                            if (nRet == 0 && !string.IsNullOrEmpty(barcode))
                                break;

                            // -offset
                            double targetMinus = basePosY - (scanStep * i);
                            if (IsMoveInterLockBarcode() != 0)
                            {
                                AxisOutputFeederY.EmgStop();
                                PostAlarm((int)AlarmKeys.Alarm_OutputStageInterlockFailed);
                                Log.Write(UnitName, "BinLoading", "Barcode scan interlock fail (-offset)");
                                return -1;
                            }

                            nRet = MoveAxisPositionOne(AxisOutputFeederY, targetMinus, isFine);
                            if (nRet != 0)
                            {
                                AxisOutputFeederY.EmgStop();
                                Log.Write(UnitName, "BinLoading", "Move Y -offset fail during barcode scan");
                                return -1;
                            }
                            Thread.Sleep(settleMs);

                            nRet = GetBarcode(out barcode);
                            if (nRet == 0 && !string.IsNullOrEmpty(barcode))
                                break;
                        }

                        // 스캔 종료 후 기준 위치 복귀
                        try
                        {
                            nRet = BarcodeReading(isFine);
                            if (nRet != 0)
                            {
                                Log.Write(UnitName, "BinLoading", "BarcodeReading Failed (return to base)");
                                return nRet;
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Write(ex);
                        }
                    }

                    if (nRet != 0 || string.IsNullOrEmpty(barcode))
                    {
                        AxisOutputFeederY.EmgStop();
                        PostAlarm((int)AlarmKeys.Alarm_BarcodeReadingFailed);
                        Log.Write(UnitName, "BinLoading", "Barcode Reading Failed after scanning");
                        return -1;
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return -1;
            }
            finally
            {
                try { OutputCassetteLifter?.EnsureTriggerOff(); } catch { }
            }
        }


        private int MoveAxisPositionOne(MotionAxis axis, double target, bool isFine)
        {
            if (axis == null)
                return -1;

            Task<int> task = MoveAxisPositionOneAsync(axis, target, isFine);
            while (IsEndTask(task) == false)
            {
                IsMoveInterLockBarcode();
                Thread.Sleep(1);
            }
            return task.Result;
        }

        // [ADD] 바코드(=WaferId) 확정 시 UI로 알리기 위한 이벤트
        public event Action<string> BinIdChanged;

        // [ADD] 이벤트 호출 헬퍼
        private void RaiseWaferIdChanged(string waferId)
        {
            try { BinIdChanged?.Invoke(waferId); }
            catch (Exception ex) { Log.Write(ex); }
        }
    }
}
