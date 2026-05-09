using QMC.Common;
using QMC.Common.Alarm;
using QMC.Common.Component;
using QMC.Common.IOUtil;
using QMC.Common.Motion;
using QMC.Common.Motions;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Component;
using QMC.LCP_280.Process.Component.ProcessData;
using QMC.LCP_280.Process.Work;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
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
        bool _isDryRunEqp = false;
        private bool IsDryRunEqp
        {
            get
            {
                var eq = Equipment.Instance;
                bool r = eq.EquipmentConfig.IsDryRun;
                return r;
            }
        }

        public new enum AlarmKeys
        {
            Alarm_BinLoadingFailed = 11201,
            Alarm_BarcodeReadingFailed = 11202,
            Alarm_StageLoadingFailed = 11203,
            Alarm_StageUnloadingFailed = 11204,
            Alarm_BinUnloadingFailed = 11205,

            Alarm_OutputStageInterlockFailed = 11206,

            Alarm_GripperClampFailed = 11207,
            Alarm_FeederClampUpDown = 11208,
            Alarm_IsBinReadyForLoading = 11209,
            Alarm_BinLoadingPosition = 11210,
            Alarm_OutputFeederNoPosition = 11211,
            Alarm_OutputFeederInterlockFailed = 11212,
            Alarm_OutputFeederBinData = 11213,
            Alarm_PrepareOutputStageUnloadingBin = 11214,
            Alarm_OutputCassetteLifter_Fail = 11215,

            // timeout
            Alarm_FeederLiftUpTimeout = 11216,
            Alarm_FeederLiftDownTimeout = 11217,
            Alarm_FeederClampTimeout = 11218,
            Alarm_FeederUnclampTimeout = 11219,

            // consistency
            Alarm_BinMissingAfterStageToFeeder = 11220,
            Alarm_BinMissingAfterFeederToCassette = 11221,
            Alarm_BinSensorDataMismatch = 11222,

            // unload slot
            Alarm_UnloadTargetSlotInvalid = 11223,
            Alarm_CassetteSlotNotEmptyForUnload = 11224,
            Alarm_CassetteMoveToSlotFailedForUnload = 11225,

            // feeder->cassette step alarms
            Alarm_UnloadFeederToCassette_MoveFeederToCassettePosFailed = 11226,
            Alarm_UnloadFeederToCassette_UnclampFailed = 11227,
            Alarm_UnloadFeederToCassette_BinDataInvalid = 11228,
            Alarm_UnloadFeederToCassette_MoveStandbyBarcodeFailed = 11229,
            Alarm_UnloadFeederToCassette_MoveStandbyReadyFailed = 11230,

            Alarm_ScanBinFailed = 11231,
            Alarm_MoveToReadyFailed = 11232,
            Alarm_MoveToCassetteTeachFailed = 11233,
            Alarm_BinCassetteLoadingFailed = 11234

        }

        #region InitAlarm
        protected override void InitAlarm()
        {
            string source = "Bin_Feeder";
            base.InitAlarm();

            // 1. 공용 파일 로더에서 알람 목록 가져오기
            var loadedAlarms = GlobalAlarmTable.Instance.GetAlarmsForSource(source);
            if (loadedAlarms == null || loadedAlarms.Count == 0)
            {
                Log.Write("AlarmInit", $"Cannot find alarms for source '{source}' in the alarm file. Only default alarms will be registered.");

                AlarmRegister((int)AlarmKeys.Alarm_BinLoadingFailed,
                   "Bin Loading Failed",
                   "Bin loading failed.",
                source, "Error");
               AlarmRegister((int)AlarmKeys.Alarm_BarcodeReadingFailed,
                   "Barcode Reading Failed",
                   "Barcode reading failed. Please check the barcode status.",
                   source, "Error");
               AlarmRegister((int)AlarmKeys.Alarm_StageLoadingFailed,
                   "Stage Loading Failed",
                   "Stage loading failed.",
                   source, "Error");
               AlarmRegister((int)AlarmKeys.Alarm_StageUnloadingFailed,
                   "Stage Unloading Failed",
                   "Stage unloading failed.",
                   source, "Error");
               AlarmRegister((int)AlarmKeys.Alarm_BinUnloadingFailed,
                   "Bin Unloading Failed",
                   "Bin unloading failed.",
                   source, "Error");
               AlarmRegister((int)AlarmKeys.Alarm_OutputStageInterlockFailed,
                   "Output Stage Interlock Failed",
                   "Interlock for Bin loading does not match. Please check the equipment status.",
                   source, "Error");
               AlarmRegister((int)AlarmKeys.Alarm_GripperClampFailed,
                   "Gripper Clamp Failed",
                   "Gripper clamp failed. Please check the equipment status.",
                   source, "Error");
               AlarmRegister((int)AlarmKeys.Alarm_FeederClampUpDown,
                   "Feeder Clamp Up Failed",
                   "Feeder clamp is not in UP status. Please check the equipment status.",
                   source, "Error");

               // = 2022,
               AlarmRegister((int)AlarmKeys.Alarm_IsBinReadyForLoading,
                   "Bin ReadyForLoading Failed",
                   "Not at Ready for Loading position. Please check the equipment status.",
                   source, "Error");
               // = 2023,
               AlarmRegister((int)AlarmKeys.Alarm_BinLoadingPosition,
                   "Bin Loading Position Failed",
                   "Not at Loading position. Please check the equipment status.",
                   source, "Error");
               // = 2024,
               AlarmRegister((int)AlarmKeys.Alarm_OutputFeederNoPosition,
                   "Output Feeder No Position",
                   "Not at Output Feeder position. Please check the equipment status.",
                   source, "Error");
               // = 2025,
               AlarmRegister((int)AlarmKeys.Alarm_OutputFeederInterlockFailed,
                   "Output Feeder Interlock Failed",
                   "Output Feeder interlock does not match. Please check the equipment status.",
                   source, "Error");
               // = 2026,
               AlarmRegister((int)AlarmKeys.Alarm_OutputFeederBinData,
                   "Output Feeder Bin Data Error",
                   "Output Feeder Bin Data error. Please check the equipment status.",
                   source, "Error");
               AlarmRegister((int)AlarmKeys.Alarm_PrepareOutputStageUnloadingBin,
                   "Output Feeder PrepareOutputStageUnloadingBin Error",
                   "Output Feeder PrepareOutputStageUnloadingBin error. Please check the equipment status.",
                   source, "Error");

               //Alarm_OutputCassetteLifter_Fail
               AlarmRegister((int)AlarmKeys.Alarm_OutputCassetteLifter_Fail,
                   "Output Feeder OutputCassetteLifter Slot Error",
                   "Output Feeder OutputCassetteLifter Slot error. Please check the equipment status.",
                   source, "Error");

               // ===== 2030~ 타임아웃 =====
               AlarmRegister((int)AlarmKeys.Alarm_FeederLiftUpTimeout,
                   "Feeder Lift Up Timeout",
                   "Feeder Lift UP timeout. (Check air pressure/valve/cylinder/sensor/interference)",
                   source, "Error");

               AlarmRegister((int)AlarmKeys.Alarm_FeederLiftDownTimeout,
                   "Feeder Lift Down Timeout",
                   "Feeder Lift DOWN timeout. (Check air pressure/valve/cylinder/sensor/interference)",
                   source, "Error");

               AlarmRegister((int)AlarmKeys.Alarm_FeederClampTimeout,
                   "Feeder Clamp Timeout",
                   "Gripper CLAMP timeout. (Check air pressure/valve/cylinder/sensor/interference)",
                   source, "Error");

               AlarmRegister((int)AlarmKeys.Alarm_FeederUnclampTimeout,
                   "Feeder Unclamp Timeout",
                   "Gripper UNCLAMP timeout. (Check air pressure/valve/cylinder/sensor/interference)",
                   source, "Error");

               // ===== 2040~ 데이터/센서 =====
               AlarmRegister((int)AlarmKeys.Alarm_BinMissingAfterStageToFeeder,
                   "Bin Missing After Stage -> Feeder",
                   "Bin is not detected in Feeder after Stage->Feeder transfer. (Check sensor/clamp/transfer sequence)",
                   source, "Error");

               AlarmRegister((int)AlarmKeys.Alarm_BinMissingAfterFeederToCassette,
                   "Bin Missing After Feeder -> Cassette",
                   "Bin remains in Feeder or mismatch status after Feeder->Cassette unload. (Check sensor/data/interference)",
                   source, "Error");

               AlarmRegister((int)AlarmKeys.Alarm_BinSensorDataMismatch,
                   "Bin Sensor/Data Inconsistency",
                   "Bin sensor status and Material data are inconsistent. (Sensor is ON but object is null, or vice versa)",
                   source, "Error");

               // ===== 2060~ 슬롯/카세트 =====
               AlarmRegister((int)AlarmKeys.Alarm_UnloadTargetSlotInvalid,
                   "Unload Target Slot Invalid",
                   "Unload Target SlotIndex is invalid. (Check Feeder/Stage/Lifter SlotIndex)",
                   source, "Error");

               AlarmRegister((int)AlarmKeys.Alarm_CassetteSlotNotEmptyForUnload,
                   "Cassette Slot Not Empty",
                   "Unload Target Cassette Slot is not empty. (Check Slot status)",
                   source, "Error");

               AlarmRegister((int)AlarmKeys.Alarm_CassetteMoveToSlotFailedForUnload,
                   "Cassette MoveToSlot Failed",
                   "Failed to move Cassette to Unload Target Slot. (Check axis alarm/interlock/Teaching)",
                   source, "Error");

               // ===== 2070~ Feeder->Cassette 상세 =====
               AlarmRegister((int)AlarmKeys.Alarm_UnloadFeederToCassette_MoveFeederToCassettePosFailed,
                   "Unload Feeder->Cassette Failed - Move Position Cassette",
                   "Failed to move to Cassette Teaching Position during Feeder->Cassette unload.",
                   source, "Error");

               AlarmRegister((int)AlarmKeys.Alarm_UnloadFeederToCassette_UnclampFailed,
                   "Unload Feeder->Cassette Failed - Unclamp",
                   "Unclamp failed/timeout during Feeder->Cassette unload.",
                   source, "Error");

               AlarmRegister((int)AlarmKeys.Alarm_UnloadFeederToCassette_BinDataInvalid,
                   "Unload Feeder->Cassette Failed - Bin Data Invalid",
                   "Cannot apply to Cassette because there is no Bin data in Feeder or SlotIndex is invalid.",
                   source, "Error");

               AlarmRegister((int)AlarmKeys.Alarm_UnloadFeederToCassette_MoveStandbyBarcodeFailed,
                   "Unload Feeder->Cassette Failed - Move Standby Barcode",
                   "Failed to move to Barcode standby position after unload.",
                   source, "Error");

               AlarmRegister((int)AlarmKeys.Alarm_UnloadFeederToCassette_MoveStandbyReadyFailed,
                   "Unload Feeder->Cassette Failed - Move Standby Ready",
                   "Failed to move to Ready standby position after unload.",
                   source, "Error");

               AlarmRegister((int)AlarmKeys.Alarm_ScanBinFailed,
                   "Scan Bin Failed",
                   "OutputCassetteLifter ScanBin failed. (Check Cassette existence/sensor/axis alarm/slot status)",
                   source, "Error");

               AlarmRegister((int)AlarmKeys.Alarm_MoveToReadyFailed,
                   "Move To Ready Failed",
                   "Move/Lift UP to Ready standby position failed. (Check Teaching/axis alarm/interlock/cylinder status)",
                   source, "Error");

               AlarmRegister((int)AlarmKeys.Alarm_MoveToCassetteTeachFailed,
                   "Move To Cassette Teaching Failed",
                   "Move to Cassette Teaching Position failed. (Check Teaching/axis alarm/interlock)",
                   source, "Error");

               AlarmRegister((int)AlarmKeys.Alarm_BinCassetteLoadingFailed,
                   "Bin Cassette Loading Failed",
                   "Bin pickup/barcode/move sequence from Cassette failed. (Check detailed log)",
                    source, "Error");

            }
            else
            {
                // 2. m_dicAlarms에 일괄 등록
                foreach (var alarmInfo in loadedAlarms)
                {
                    if (!m_dicAlarms.ContainsKey(alarmInfo.Code))
                    {
                        m_dicAlarms.Add(alarmInfo.Code, alarmInfo);
                    }
                }
            }
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
           : base(config ?? new OutputFeederConfig())
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
            isOK = this.OutputCassetteLifter.IsBinReadyForLoading();
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
            if (feederObj != null && !feederSensor && !(Config.IsSimulation || (Config.IsDryRun|| IsDryRunEqp)))
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
                //eq.StopAllUnitsAsync();
                eq.SequenceStopAllAsync(CancellationToken.None);

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
                return OnMovePositionReady(isFine);
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
        public int MoveToTeachingPosition(string positionName, bool isFine)
        {
            if (string.IsNullOrWhiteSpace(positionName))
            {
                Log.Write(UnitName, nameof(MoveToTeachingPosition),
                        $"[TeachingMove] TeachingPositions에서 '{positionName}' 을 찾지 못했습니다.");
                return -1;
            }

            int result = 0;

            OutputFeederConfig.TeachingPositionName en;
            if (Enum.TryParse(positionName, out en))
            {
                int selIndex = FindTeachingSelectionIndex(positionName);
                if (selIndex >= 0)
                {
                    result = MoveToTeachingPositionBySelectionIndex(selIndex, isFine);
                }
                else
                {
                    Log.Write(UnitName, nameof(MoveToTeachingPosition),
                        $"[TeachingMove] TeachingPositions에서 '{positionName}' index를 찾지 못했습니다.");
                    return -1;
                }
            }

            return result;
        }

        private int FindTeachingSelectionIndex(string positionName)
        {
            try
            {
                var list = GetTeachingList();
                if (list == null)
                    return -1;

                for (int i = 0; i < list.Count; i++)
                {
                    var tp = list[i];
                    if (tp != null && string.Equals(tp.Name, positionName, StringComparison.OrdinalIgnoreCase))
                        return i;
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            return -1;
        }

        private IList<TeachingPosition> GetTeachingList()
        {
            // 1) Recipe 기반 TeachingRecipe가 있으면 그쪽 우선
            //    (Config 타입마다 TeachingRecipe 프로퍼티 존재 여부가 다르므로 reflection 사용)
            try
            {
                var cfg = Config;
                if (cfg != null)
                {
                    var prop = cfg.GetType().GetProperty("TeachingRecipe",
                        System.Reflection.BindingFlags.Instance |
                        System.Reflection.BindingFlags.Public |
                        System.Reflection.BindingFlags.NonPublic);

                    if (prop != null)
                    {
                        var teachingRecipe = prop.GetValue(cfg, null);
                        if (teachingRecipe != null)
                        {
                            // TeachingRecipe가 IHasTeachingPositions 구현한 경우가 많음
                            var has = teachingRecipe as QMC.LCP_280.Process.Unit.FormConfig.IHasTeachingPositions;
                            if (has != null && has.TeachingPositions != null)
                                return has.TeachingPositions;

                            // 혹시 인터페이스가 다르면 TeachingPositions 프로퍼티를 reflection으로 한번 더 시도
                            var tpProp = teachingRecipe.GetType().GetProperty("TeachingPositions",
                                System.Reflection.BindingFlags.Instance |
                                System.Reflection.BindingFlags.Public |
                                System.Reflection.BindingFlags.NonPublic);

                            var list = tpProp != null ? tpProp.GetValue(teachingRecipe, null) as IList<TeachingPosition> : null;
                            if (list != null)
                                return list;
                        }
                    }
                }
            }
            catch { /* ignore */ }

            // 2) 기본: Config.TeachingPositions
            return Config?.TeachingPositions ?? new List<TeachingPosition>();
        }
        #endregion

        #region IO Domain Mapping
        private void BindIoDomains()
        {
            var eq = Equipment.Instance; 
            var unit = eq?.UnitIO; 
            if (unit == null) 
                return;

            if (!IoAutoBindings.Cylinders.TryGetValue("OutFeederLift", out _feederLift))
            {
                Log.Write("OutputFeeder", "BindIoDomains", "Cylinder not found: OutFeederLift");
            }
            BindCylinder(_feederLift); //ADD

            if (!IoAutoBindings.Cylinders.TryGetValue("OutFeederClamp", out _cylClamp))
            {
                Log.Write("OutputFeeder", "BindIoDomains", "Cylinder not found: OutFeederClamp");
            }
            BindCylinder(_cylClamp); //ADD
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
            bool bRet = false;
            if (Config.IsSimulation || (Config.IsDryRun || IsDryRunEqp))
            {
                // 시뮬레이션: 실제 보유 머티리얼로 판단
                bRet = this.GetMaterial() is MaterialWafer;
                return bRet;
            }
            bRet = this.ReadInput(OutputFeederConfig.IO.FEEDER_RING_CHECK);
            return bRet;
        }
        public bool IsOverload()
        {
            if (Config.IsSimulation || (Config.IsDryRun || IsDryRunEqp))
            {
                return true;
            }
            return this.ReadInput(OutputFeederConfig.IO.FEEDER_OVERLOAD);
        }

        // === Cylinder 완료 대기 Helpers ===
        private int WaitClampStateOrAlarm(bool expectClamp, int timeoutMs = 1500, int pollMs = 2)
        {
            if (Config.IsSimulation || (Config.IsDryRun || IsDryRunEqp))
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
            if (Config.IsSimulation || (Config.IsDryRun || IsDryRunEqp))
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
               this.RunUnitStatus == UnitStatus.Error ||
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
                    bool sim = (Config.IsSimulation || (Config.IsDryRun || IsDryRunEqp));
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

            try
            {
                // 0) Stage에 제품이 있으면 "언로딩 먼저"
                if (NeedUnloadFirst)
                {
                    // 재개 시작 단계 보정
                    if (_unloadStep == UnloadFlowStep.None || _unloadStep == UnloadFlowStep.Completed)
                        _unloadStep = DetermineNextUnloadStep();

                    nRet = RunUnloadWaferFlowStep(true);

                    if (_unloadStep == UnloadFlowStep.Completed)
                    {
                        _unloadStep = UnloadFlowStep.Completed;
                        _unloadTargetSlot = -1;
                        NeedUnloadFirst = false;
                        this.State = ProcessState.Complete;
                    }
                    return nRet;
                }
                else
                {

                    // 1) Feeder -> Cassette: Scan 확인
                    if (this.OutputCassetteLifter.IsScanCompleted() == false)
                    {
                        nRet = this.OutputCassetteLifter.ScanBin(true);
                        if (nRet != 0)
                        {
                            AxisOutputFeederY.EmgStop();
                            PostAlarm((int)AlarmKeys.Alarm_ScanBinFailed); // 2080
                            return nRet;
                        }
                        if (IsStop)
                        {
                            Log.Write(UnitName, "RunLoadWaferFlowStep", "IsScanCompleted Check -> Stop");
                            return 0;
                        }
                    }

                    bool hasMore = this.OutputCassetteLifter.IsHaveMoreProcessWafer();
                    bool feederHasWafer = GetMaterial() is MaterialWafer;

                    // 2) 더 진행할 것이 전혀 없으면 Ready 복귀 후 종료 판단
                    if (!hasMore && !feederHasWafer)
                    {
                        if (!IsPositionReady())
                        {
                            nRet = MoveToReady();
                            if (nRet != 0)
                            {
                                AxisOutputFeederY.EmgStop();
                                PostAlarm((int)AlarmKeys.Alarm_MoveToReadyFailed); // 2081
                                this.State = ProcessState.Error;
                                return nRet;
                            }
                        }

                        TryShutdownIfAllCassettesEmpty();
                        this.State = ProcessState.Ready;
                        return 0;
                    }

                    // ===== Load 플로우 (항상 FSM로 진입) =====
                    bool didLoad = false;
                    if (_loadStep == LoadFlowStep.None || _loadStep == LoadFlowStep.Completed)
                    {
                        _loadStep = DetermineNextLoadStep();
                    }

                    nRet = RunLoadWaferFlowStep(out didLoad, true);
                    if (nRet != 0
                        || (_loadStep != LoadFlowStep.Completed))
                    {
                        Log.Write(UnitName, "OnRunWork", "Load flow not completed, continue work.");
                        return nRet;
                    }

                    // [FIX] 로드 수행 없으면 위치 복귀(Ready 이동) 자체를 하지 않음
                    if (didLoad == false)
                    {
                        this.State = ProcessState.Complete;
                        Log.Write(UnitName, "OnRunWork", "No wafer to load.");
                        return 0;
                    }

                    // 3) 아무 것도 할 웨이퍼가 없으면 대기 안전 위치로.
                    // 대기 위치인지 한번 더 확인하는거지.
                    if (IsPositionReady() == false)
                    {
                        if (OutputStage.IsPositionBinLoading() == false
                           && OutputStage.IsPositionBinUnloading() == false)
                        {
                            AxisOutputFeederY.EmgStop();
                            PostAlarm((int)AlarmKeys.Alarm_StageLoadingFailed);
                            Log.Write(UnitName, "OnRunWork", "Fail - IsPositionWaferLoading() == false");
                            return nRet;
                        }

                        // wafer 가지고 있는지 유/무 판단 필요.
                        bool hasWafer = HasBinOnFeeder();
                        // Ready로 복귀할 때, 현재 로직은 "Unclamp 상태"를 요구함.
                        // 그런데 웨이퍼를 잡고 있는 상태(클램프 가능성)라면 Ready 복귀 자체가 위험/불가할 수 있음.
                        if (hasWafer == true)
                        {
                            // 1) 클램프가 닫혀 있으면(=웨이퍼를 잡고 있을 가능성) Ready 이동 금지
                            //    -> 여기서 무조건 Unclamp 해버리면 웨이퍼 낙하 가능하니, 정책을 확실히 해야 함.
                            //    안전 우선: 알람/에러로 보내고 사용자가 "언로드" 버튼을 누르게 유도.
                            if (IsSafeToMoveReady() == false)
                            {
                                AxisOutputFeederY.EmgStop();
                                PostAlarm((int)AlarmKeys.Alarm_OutputFeederInterlockFailed);
                                Log.Write(UnitName, "OnRunWork",
                                    "Blocked MoveToReady: Feeder has wafer and is not unclamped. Run unload flow first.");
                                return -1;
                            }

                            // 2) (선택) 웨이퍼는 있는데 Unclamp 상태로 판단되는 경우(데이터/센서 불일치 가능)
                            //    -> 이 경우도 위험하므로 막는 방향 권장
                            if (!(Config.IsSimulation || (Config.IsDryRun || IsDryRunEqp)))
                            {
                                // 센서 OFF + 객체만 남은 케이스 등을 포함해 강하게 막고 싶으면 아래로 처리
                                // AxisInputFeederY.EmgStop();
                                // PostAlarm((int)AlarmKeys.Alarm_WaferSensorDataMismatch);
                                // Log.Write(UnitName, "OnRunWork", "Blocked MoveToReady: wafer data/sensor mismatch");
                                // return -1;
                            }
                        }

                        nRet = MovePositionReady();
                        if (nRet != 0)
                        {
                            AxisOutputFeederY.EmgStop();
                            PostAlarm((int)AlarmKeys.Alarm_BinLoadingFailed);
                            this.State = ProcessState.Error;
                            Log.Write(UnitName, "OnRunWork", "Fail - MovePositionReady");
                            return nRet;
                        }
                        nRet = UpFeeder();
                        if (nRet != 0)
                        {
                            AxisOutputFeederY.EmgStop();
                            PostAlarm((int)AlarmKeys.Alarm_BinLoadingFailed);
                            this.State = ProcessState.Error;
                            Log.Write(UnitName, "OnRunWork", "Fail - UpFeeder");
                            return nRet;
                        }
                    }

                    _loadStep = LoadFlowStep.Completed;
                    this.State = ProcessState.Complete;
                    Log.Write(UnitName, "OnRunWork", "LoadFlowStep.StageLoadingAfter completed.");

                    return 0;
                }
            }
            catch(Exception ex)
            {
                Log.Write(ex);
                return -1;
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

            if (RunUnitStatus != UnitStatus.ManualRunning)
            {
                var recipe = Equipment.Instance.EquipmentRecipe.CurrentRecipe;
                if (recipe.UseSameAsWafer == true)
                {
                    // [PATCH] 무한 대기 방지: timeout 추가 (Stop만으로는 현장 멈춤처럼 보임)
                    const int waitTimeoutMs = 60000 * 15; //15분안에는 로드쪽이 마무리 되어야지. Auto진행시. //1시간?
                    var swWait = System.Diagnostics.Stopwatch.StartNew();
                    var srcWafer = inputStage.GetMaterialWafer();
                    while (true)
                    {
                        if (IsStop)
                        {
                            Log.Write(UnitName, "BinStageMapping", "IsStop detected during waiting for InputStage wafer.");
                            return 0;
                        }

                        if (swWait.ElapsedMilliseconds > waitTimeoutMs)
                        {
                            AxisOutputFeederY.EmgStop();
                            PostAlarm((int)AlarmKeys.Alarm_StageLoadingFailed);
                            Log.Write(UnitName, "BinStageMapping", $"Timeout waiting for InputStage wafer (>{waitTimeoutMs}ms).");
                            return -1;
                        }

                        srcWafer = inputStage.GetMaterialWafer();
                        if (srcWafer != null && inputStage.ChipMappingDone == true)
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
            //추 후 전체 장비 통합 검토.
            Equipment.Instance.ResultWriterManager.ResultLogData_BeginWaferSummary(waferOnStage?.WaferId, "VA1VPRO16");
            // 웨이퍼의 전체 칩 개수 계산
            int totalDieCount = waferOnStage.Dies.Count;
            // 또는 실제 작업해야 할 칩 개수 (Skip 제외 등 조건에 따라)
            // int totalDieCount = wafer.Dies.Count(d => d.State != DieProcessState.Skip);
            // [핵심] ResultWriterManager에 전체 개수 설정
            Equipment.Instance.ResultWriterManager.SetWaferTotalCount(totalDieCount);
            Log.Write(UnitName, "OnRunWork", "LoadFlowStep.BinStageMapping completed.");
            return nRet;
        }

        #endregion

        protected override void OnMakeSequence()
        {
            base.OnMakeSequence();
            //this.SequencePlayers.Add(BinCassetteLoading);
            //this.SequencePlayers.Add(StageLoading);
            //this.SequencePlayers.Add(MoveToReady);
            //this.SequencePlayers.Add(BinUnloading);

            // Manual Sequences(New)
            this.SequencePlayers.Add(Manual_StandbyReady);
            this.SequencePlayers.Add(Manual_Unload_Batch);
            this.SequencePlayers.Add(Manual_Load_Batch);
            
        }


        #region Seq 단위 동작 함수
        /// <summary>
        /// Manually triggers the full Loading Sequence (Cassette -> Feeder -> Stage -> Ready)
        /// </summary>
        public int Manual_Load_Batch(bool isFine = false)
        {
            // Reset Step state for fresh start
            _loadStep = LoadFlowStep.Step01;

            // Just loop until completion or error, simulating the FSM behavior in OnRunWork
            while (_loadStep != LoadFlowStep.Completed)
            {
                if (IsStop) return 0;

                bool didLoad = false;
                int nRet = RunLoadWaferFlowStep(out didLoad, isFine);

                if (nRet != 0)
                {
                    // Error occurred
                    return nRet;
                }

                Thread.Sleep(10); // Yield slightly
            }

            return 0;
        }

        /// <summary>
        /// Manually triggers the full Unloading Sequence (Stage -> Feeder -> Cassette)
        /// </summary>
        public int Manual_Unload_Batch(bool isFine = false)
        {
            // Determine start step based on sensor/material status
            _unloadStep = DetermineNextUnloadStep();

            // If nothing to unload, exit
            if (_unloadStep == UnloadFlowStep.Completed || _unloadStep == UnloadFlowStep.None)
            {
                Log.Write(UnitName, "Manual_Unload_Batch", "Nothing to unload (Sensor/Data check).");
                return 0;
            }

            // Loop until completion
            while (_unloadStep != UnloadFlowStep.Completed)
            {
                if (IsStop) return 0;

                int nRet = RunUnloadWaferFlowStep(isFine);

                if (nRet != 0)
                {
                    // Error occurred
                    return nRet;
                }

                Thread.Sleep(10);
            }

            return 0;
        }

        /// <summary>
        /// Manually moves to Safe/Ready Position
        /// </summary>
        public int Manual_StandbyReady(bool isFine = false)
        {
            if (IsStop) return 0;

            // 1. Check Interlocks (Safety)
            if (IsPositionReady())
            {
                // Already there, just ensure Up
                return UpFeeder();
            }

            // 2. Logic similar to OnRunWork standby logic
            // Ensure we don't crash into Stage
            if (OutputStage.IsPositionBinLoading() == false &&
                OutputStage.IsPositionBinUnloading() == false)
            {
                // If stage is unsafe, we cannot move Y
                Log.Write(UnitName, "Manual_StandbyReady", "OutputStage not in safe position.");
                PostAlarm((int)AlarmKeys.Alarm_OutputStageInterlockFailed);
                return -1;
            }

            int nRet = 0;
            nRet = UnClampGripper();
            if (nRet != 0)
            {
                Log.Write(UnitName, "Manual_StandbyReady", "UnClampGripper failed.");
                return nRet;
            }

            // 3. Move
            nRet = MovePositionReady(isFine);
            if (nRet != 0)
            {
                Log.Write(UnitName, "Manual_StandbyReady", "MovePositionReady failed.");
                return nRet;
            }

            // 4. Lift Up
            nRet = UpFeeder();
            if (nRet != 0)
            {
                Log.Write(UnitName, "Manual_StandbyReady", "UpFeeder failed.");
                return nRet;
            }

            Log.Write(UnitName, "Manual_StandbyReady", "Completed.");
            return 0;
        }

        // [ADD] WaferExchangeDecision 로그 쓰로틀/변화 감지용(간단 버전)
        private int _lastWEDStateMask = -1;
        private int _lastWEDTick = 0;
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

            string barcode = string.Empty;
            if(OutputCassetteLifter.Config.UseBarcode)
            {
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
            }
            else
            {
                nRet = GetBarcode(out barcode);
                if (nRet != 0 || string.IsNullOrEmpty(barcode))
                {
                    AxisOutputFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_BarcodeReadingFailed);
                    Log.Write(UnitName, "WaferLoading", "Barcode Reading Failed after scanning");
                    return -1;
                }
            }

            strBarcode = barcode.Trim();
            {
                var c = this.OutputCassetteLifter.GetMaterialCassette();
                int nIndex = this.OutputCassetteLifter.GetCurrectSlotID();
                MaterialWafer Bin = c.GetWafer(nIndex);
                if (Bin == null)
                {
                    Log.Write(UnitName, "BinLoading Complete", "Bin == null");
                    return -1;
                }

                Bin.CarrierId = c.CarrierId;

                if (Config.IsSimulation || (Config.IsDryRun || IsDryRunEqp))
                {
                    strBarcode = string.Format("{0}_{1}", strBarcode, Bin.CarrierId);
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
                var wafer = this.OutputStage.GetMaterialWafer();
                if(wafer.ProcessSatate == Material.MaterialProcessSatate.Completed)
                {
                    return true;
                }

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
                
                //Todo: 2026-01-05 :: 확인 필요
                // [PATCH] Rotary 쪽 언로드 소켓 접근이 프로젝트별로 다를 수 있어 reflection으로 방어적 체크
                //try
                //{
                //    if (Rotary != null)
                //    {
                //        var mi = Rotary.GetType().GetMethod("GetUnloadSocketMaterial",
                //            System.Reflection.BindingFlags.Instance |
                //            System.Reflection.BindingFlags.Public |
                //            System.Reflection.BindingFlags.NonPublic);

                //        if (mi != null)
                //        {
                //            var dieObj = mi.Invoke(Rotary, null);
                //            if (dieObj is MaterialDie)
                //                return false;
                //        }
                //    }
                //}
                //catch (Exception ex)
                //{
                //    // 판단 불가면 보수적으로 막음
                //    Log.Write(UnitName, "IsSafeToStartStageUnloading", "Rotary unload-socket check exception: " + ex.Message);
                //    return false;
                //}


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

            if(RunUnitStatus == UnitStatus.AutoRunning)
            {
                // [ADD] ODT/Rotary 버퍼가 비기 전에는 Stage 언로딩 시작 금지
                if (!IsSafeToStartStageUnloading())
                {
                    // 여기서는 알람을 올리지 말고 "대기"가 안전합니다.
                    // Ready에서 다시 돌면서 자연스럽게 비면 언로딩 진행.
                    Log.Write(UnitName, "BinUnloading_Step01", "Blocked: OutputDieTransfer/Rotary still has die.");
                    return 1; // [IMPORTANT] 0이 아닌 '대기' 코드
                }
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

            // ===== [MOD] 공정 종료 확정(Equipment Summary 종료 + 파일 저장) =====
            try
            {
                var ctx = Equipment.Instance.SummaryContext;
                ctx.End();

                var row = ctx.GetSnapshotOrNull();
                if (row != null)
                {
                    Equipment.Instance.ResultWriterManager.AppendWaferTotalSummaryRow(row);
                }

                ctx.CommitCurrentToHistoryAndDeactivate();
            }
            catch (Exception ex)
            {
                Log.Write(UnitName, "UnloadOnlyFeederToCassette", "SummaryContext.End/AppendWaferTotalSummaryRow exception: " + ex.Message);
            }


            // [PATCH] MovePositionBarcode() 중복 호출 제거: 아래 hasNext 분기에서만 대기 위치 결정
            //Todo : 확인 하고 주석 처리.
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
            return Task.Run(() =>
            {
                return OnMoveToCassette(isFine);
            });
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

            if (Config.IsSimulation || (Config.IsDryRun || IsDryRunEqp))
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
                return OnEnsureReady(isFine);
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

                        //AxisOutputFeederY.EmgStop();
                        //PostAlarm((int)AlarmKeys.Alarm_OutputStageInterlockFailed);
                        //Log.Write(UnitName, "CheckReady Fail - OutputStage.IsStageInterLockOK");
                        //return -1;
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
                if (Config.IsSimulation == false && (Config.IsDryRun == false && IsDryRunEqp == false))
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

        // LCP-280\Unit\OutputFeeder.cs 내부
        // [수정] InputStage의 Map 정보를 복사하되, 
        // 1) 기본 장비 축 매핑(X:정방향, Y:역방향)을 먼저 적용한 뒤
        // 2) Recipe의 Rotate/Mirror 설정을 추가 반영함.
        private int CopyInputMapRotate180(MaterialWafer srcWafer, MaterialWafer dstWafer)
        {
            try
            {
                if (srcWafer?.Dies == null || srcWafer.Dies.Count == 0)
                    return -1;
                if (dstWafer == null)
                    return -2;

                lock (srcWafer.Dies)
                {
                    var sourceDies = srcWafer.Dies.OrderBy(d => d.Index).ToList();
                    if (sourceDies.Count == 0)
                        return -3;

                    // src lock 안에서 min/max 계산
                    double minX = sourceDies.Min(d => d.MapX);
                    double maxX = sourceDies.Max(d => d.MapX);
                    double minY = sourceDies.Min(d => d.MapY);
                    double maxY = sourceDies.Max(d => d.MapY);

                    // 전체 그리드 크기(중간 빈 줄 포함)
                    double nx = maxX - minX + 1;
                    double ny = maxY - minY + 1;
                    if (nx <= 0 || ny <= 0)
                        return -4;

                    lock (dstWafer.Dies)
                    {
                        dstWafer.Dies?.Clear();
                        dstWafer.Dies = new List<MaterialDie>(sourceDies.Count);

                        foreach (var s in sourceDies)
                        {
                            // 1) 절대좌표 -> 0-base 정규화
                            double x0 = s.MapX - minX;   // [0..nx-1]
                            double y0 = s.MapY - minY;   // [0..ny-1]

                            // 2) 장비 기본 보정 (Y 역방향)
                            double baseX = x0;
                            double baseY = ny - 1 - y0;

                            // 3) 180도 회전
                            double rotX = nx - 1 - baseX;
                            double rotY = ny - 1 - baseY;

                            // 4) 필요 시 다시 절대좌표로 복원 (dst는 0-base Bin 쓰는 구조면 그대로 사용)
                            double binX = rotX; // = maxX - s.MapX
                            double binY = rotY; // = s.MapY - minY

                            dstWafer.Dies.Add(new MaterialDie
                            {
                                Index = s.Index,
                                Presence = Material.MaterialPresence.Exist,
                                ProcessSatate = Material.MaterialProcessSatate.Ready,
                                BinX = binX,
                                BinY = binY,
                                MapX = binX,
                                MapY = binY
                            });
                        }
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                Log.Write(UnitName, nameof(CopyInputMapRotate180), ex.Message);
                return -9;
            }
        }


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

        // [UPDATED] TryCloneMapFromInputStage: 복제 → 순서 재정렬(베이스 코너/주축) → Index 재설정
        private bool TryCloneMapFromInputStage(MaterialWafer dstBin)
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
                    lock (dstBin.Dies)
                    {
                        if (dstBin.Dies != null) dstBin.Dies.Clear();
                        dstBin.Dies = new List<MaterialDie>(srcWafer.Dies.Count);
                        int rc = CopyInputMapRotate180(srcWafer, dstBin);
                        if (rc != 0)
                        {
                            Log.Write(UnitName, "MakePath", $"Clone failed rc={rc}");
                            return false;
                        }

                        Log.Write(UnitName, "MakePath", $"Cloned from InputStage. Count={dstBin.Dies.Count}");
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
                return (r.BinDiameter > 0) ? r.BinDiameter : 0.0;
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
                    var recipe = Equipment.Instance.EquipmentRecipe.CurrentRecipe;
                    if (recipe.UseSameAsWafer == true)
                    {
                        // 0) InputStage 맵을 우선 그대로 복제(개수/격자/좌표 일치 보장)
                        if (PreferCloneMapFromInputStage && TryCloneMapFromInputStage(Bin))
                        {
                            // 복제 성공 시 여기서 종료 → InputStage에서 도출된 칩 개수와 완전 동일
                            Log.Write(UnitName, "MakePath", "bIndexCal = true");
                            return 0;
                        }
                    }
                    // 1) (Fallback) ChipPitch + 웨이퍼 지름 기반 원형 맵 생성
                    // [수정] UseSameAsWafer == false 일 때 레시피 기준 생성 로직 (물리적 순서 방식)
                    else
                    {
                        double pitchX = ChipPitchXmm;
                        double pitchY = ChipPitchYmm;
                        if (pitchX <= 0) pitchX = 0.55;
                        if (pitchY <= 0) pitchY = 0.55;

                        double diameterMm = BinDiameterMm;

                        // 직경 설정이 없으면 기본 5x5 혹은 Pitch 기준 계산
                        if (diameterMm <= 0)
                        {
                            // 20개 정도 들어가는 크기로 임의 설정 (기존 로직 유지)
                            diameterMm = Math.Min(20 * pitchX, 20 * pitchY);
                        }

                        double radiusMm = Math.Max(0.0, diameterMm / 2.0 - Math.Max(0.0, BinCircleMarginMm));

                        // 반지름 기준 Grid Cell 개수 계산 (중심 0,0 기준)
                        int halfCellsX = (int)Math.Floor(radiusMm / pitchX);
                        int halfCellsY = (int)Math.Floor(radiusMm / pitchY);

                        // 전체 Grid 크기 (항상 홀수로 만들어 중심을 0,0으로 맞춤)
                        int nCols = Math.Max(1, halfCellsX * 2 + 1); // Width count (X축 개수)
                        int nRows = Math.Max(1, halfCellsY * 2 + 1); // Height count (Y축 개수)

                        // 중심 인덱스 (Floating point center)
                        double centerX = (nCols - 1) / 2.0;
                        double centerY = (nRows - 1) / 2.0;

                        // 레시피 설정 가져오기
                        var Recipe = Equipment.Instance.EquipmentRecipe.CurrentRecipe as MeasurementRecipe;
                        var StartCorner = Recipe != null ? Recipe.BinPathStartCorner : MapPathStartCorner.BottomLeft;
                        var PrimaryAxis = Recipe != null ? Recipe.BinPathPrimaryAxis : MapPathPrimaryAxis.XFirst;
                        var Traversal = Recipe != null ? Recipe.BinPathTraversalMode : MapPathTraversalMode.Serpentine;

                        var list = new List<MaterialDie>();

                        // --- [로직 변경] Start Corner, Axis, Serpentine 적용하여 순서대로 생성 ---
                        // 좌표계 기준: (0,0) = Bottom-Left
                        // X 증가: 오른쪽으로 이동
                        // Y 증가: 위쪽으로 이동

                        // 1. 루프 범위 및 방향 결정 (StartCorner 기준)
                        int xStart, xEnd, xStep;
                        int yStart, yEnd, yStep;

                        // X축 설정
                        // Left 시작(BottomLeft, TopLeft): 0 -> nCols (증가)
                        // Right 시작(BottomRight, TopRight): nCols-1 -> -1 (감소)
                        bool isLeftStart = (StartCorner == MapPathStartCorner.BottomLeft || StartCorner == MapPathStartCorner.TopLeft);
                        //if (isLeftStart) { xStart = 0; xEnd = nCols; xStep = 1; }
                        //else { xStart = nCols - 1; xEnd = -1; xStep = -1; }
                        // X축: LeftStart일 때 큰 값(nCols-1)부터 시작해야 함
                        if (isLeftStart) { xStart = nCols - 1; xEnd = -1; xStep = -1; }
                        else { xStart = 0; xEnd = nCols; xStep = 1; }

                        // Y축 설정
                        // Bottom 시작(BottomLeft, BottomRight): 0 -> nRows (증가)
                        // Top 시작(TopLeft, TopRight): nRows-1 -> -1 (감소)
                        bool isBottomStart = (StartCorner == MapPathStartCorner.BottomLeft || StartCorner == MapPathStartCorner.BottomRight);
                        //if (isBottomStart) { yStart = 0; yEnd = nRows; yStep = 1; }
                        //else { yStart = nRows - 1; yEnd = -1; yStep = -1; }
                        // Y축: BottomStart일 때 큰 값(nRows-1)부터 시작해야 함
                        if (isBottomStart) { yStart = nRows - 1; yEnd = -1; yStep = -1; }
                        else { yStart = 0; yEnd = nRows; yStep = 1; }
                        // 다이 추가 내부 함수
                        void CheckAndAddDie(int cx, int cy)
                        {
                            // 중심 기준 상대 좌표 계산 (물리 거리 계산용)
                            double relX = cx - centerX;
                            double relY = cy - centerY;

                            double dxMm = relX * pitchX;
                            double dyMm = relY * pitchY;
                            double dist2 = dxMm * dxMm + dyMm * dyMm;

                            // 원형 맵 사용 시 반경 체크
                            bool inside = !UseCircularBinMap ? true : (dist2 <= radiusMm * radiusMm);
                            if (!inside)
                            {
                                return;
                            }

                            list.Add(new MaterialDie
                            {
                                Index = list.Count, // 생성 순서대로 Index 부여 (0, 1, 2...)
                                Presence = Material.MaterialPresence.NotExist, // 초기 상태
                                ProcessSatate = Material.MaterialProcessSatate.Unknown,
                                BinX = cx,          // Grid 상의 절대 좌표 (0 ~ nCols-1)
                                BinY = cy,          // Grid 상의 절대 좌표 (0 ~ nRows-1)
                                MapX = (int)relX,   // 중심 기준 상대 좌표 (물리적 위치 계산용)
                                MapY = (int)relY
                            });
                        }

                        // 2. Primary Axis에 따른 이중 루프 실행
                        if (PrimaryAxis == MapPathPrimaryAxis.XFirst)
                        {
                            // [X-First]: Y(Row) 고정 후 X(Col) 이동 (가로 방향 우선)
                            // 바깥 루프: Y (StartCorner 기준 방향)
                            for (int y = yStart; y != yEnd; y += yStep)
                            {
                                // 안쪽 루프: X
                                // 몇 번째 라인인지 계산 (0, 1, 2...)
                                int rowCount = Math.Abs(y - yStart);

                                // 지그재그(Serpentine) 체크: 홀수 번째 라인(1, 3...)은 반대 방향
                                bool reverse = (Traversal == MapPathTraversalMode.Serpentine) && (rowCount % 2 != 0);

                                if (!reverse)
                                {
                                    // 정방향 (StartCorner 기준 X방향)
                                    for (int x = xStart; x != xEnd; x += xStep)
                                        CheckAndAddDie(x, y);
                                }
                                else
                                {
                                    // 역방향 (StartCorner 반대 X방향)
                                    // xStart가 0(증가)이면 역방향은 Max->0(감소)
                                    // xStart가 Max(감소)이면 역방향은 0->Max(증가)
                                    int revStart = (xStep > 0) ? (nCols - 1) : 0;
                                    int revEnd = (xStep > 0) ? -1 : nCols;
                                    int revStep = -xStep;

                                    for (int x = revStart; x != revEnd; x += revStep)
                                        CheckAndAddDie(x, y);
                                }
                            }
                        }
                        else // YFirst
                        {
                            // [Y-First]: X(Col) 고정 후 Y(Row) 이동 (세로 방향 우선)
                            // 바깥 루프: X (StartCorner 기준 방향)
                            for (int x = xStart; x != xEnd; x += xStep)
                            {
                                // 안쪽 루프: Y
                                int colCount = Math.Abs(x - xStart); // 0, 1, 2...

                                // 지그재그(Serpentine) 체크: 홀수 번째 라인(1, 3...)은 반대 방향
                                bool reverse = (Traversal == MapPathTraversalMode.Serpentine) && (colCount % 2 != 0);

                                if (!reverse)
                                {
                                    // 정방향 (StartCorner 기준 Y방향)
                                    for (int y = yStart; y != yEnd; y += yStep)
                                        CheckAndAddDie(x, y);
                                }
                                else
                                {
                                    // 역방향
                                    int revStart = (yStep > 0) ? (nRows - 1) : 0;
                                    int revEnd = (yStep > 0) ? -1 : nRows;
                                    int revStep = -yStep;

                                    for (int y = revStart; y != revEnd; y += revStep)
                                        CheckAndAddDie(x, y);
                                }
                            }
                        }

                        // 리스트 반영
                        Bin.Dies.Clear();
                        Bin.Dies.AddRange(list);

                        Log.Write(UnitName, "MakePath",
                            $"Circular(Fallback)={UseCircularBinMap} " +
                            $"StartCorner={StartCorner} Primary={PrimaryAxis} Zigzag={Traversal} " +
                            $"Dies={Bin.Dies.Count} " +
                            $"Grid=({nCols}x{nRows}) " +
                            $"Pitch=({pitchX:F3},{pitchY:F3})mm " +
                            $"Radius={radiusMm:F3}mm");
                    }
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
                if (Config.IsSimulation || (Config.IsDryRun || IsDryRunEqp))
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

        public int MoveToTeachingPositionBySelectionIndex(int teachingSelIndex, bool isFine = false)
        {
            if (Config == null)
                return -1;

            string tpName;
            if (!Config.GetTeachingPositionName(teachingSelIndex, out tpName) || string.IsNullOrWhiteSpace(tpName))
                return -1;

            OutputFeederConfig.TeachingPositionName en;
            if (!Enum.TryParse(tpName, out en))
                return -1;

            switch (en)
            {
                case OutputFeederConfig.TeachingPositionName.Ready:
                    return MovePositionReady(isFine);

                case OutputFeederConfig.TeachingPositionName.Stage:
                    return MovePositionStage(isFine);

                case OutputFeederConfig.TeachingPositionName.Barcode:
                    return MovePositionBarcode(isFine);

                case OutputFeederConfig.TeachingPositionName.Cassette:
                    return MovePositionCassette(isFine);

                case OutputFeederConfig.TeachingPositionName.SetPosition:
                    break;

                default:
                    break;
            }

            return 0;
        }

        /// <summary>
        /// [ADD] 통합 언로딩 시퀀스 함수
        /// Auto / Manual 모드를 모두 지원하며 상태머신(Step)으로 동작
        /// </summary>
        private int RunUnloadWaferFlowStep(bool isFine)
        {
            int nRet = 0;

            // [ADD] Manual 모드일 경우 재시작/정지(Resume/Stop)를 위해 CurrentFunc 등록
            if (this.RunMode == UnitRunMode.Manual)
            {
                CurrentFunc = RunUnloadWaferFlowStep;
            }

            // 1. 단계 초기화 및 결정
            if (_unloadStep == UnloadFlowStep.None || _unloadStep == UnloadFlowStep.Completed)
            {
                _unloadStep = DetermineNextUnloadStep();
            }

            // 2. 단계별 실행
            switch (_unloadStep)
            {
                case UnloadFlowStep.Step01:
                    // Stage Unloading Prepare & Check
                    nRet = BinUnloading_Step01(isFine);

                    // [대기] Blocked 상태면 0 리턴하여 대기 (다음 사이클에 재진입)
                    if (nRet == 1)
                        return 0;

                    if (nRet != 0) { MarkUnloadStepOnFailure(UnloadFlowStep.Step01); return nRet; }
                    if (IsStop) { MarkUnloadStepOnFailure(UnloadFlowStep.Step01); return 0; }

                    AdvanceUnloadStepOnSuccess(UnloadFlowStep.Step01);

                    // Auto 모드이거나, Manual이어도 연속 동작을 원하면 goto 사용
                    // (여기서는 흐름 끊기지 않게 바로 다음 스텝 진행)
                    goto case UnloadFlowStep.Step02;

                case UnloadFlowStep.Step02:
                    // Stage -> Feeder
                    nRet = BinUnloading_Step02(isFine);
                    if (nRet != 0) { MarkUnloadStepOnFailure(UnloadFlowStep.Step02); return nRet; }
                    if (IsStop) { MarkUnloadStepOnFailure(UnloadFlowStep.Step02); return 0; }

                    // 다음 타겟 슬롯 계산
                    if (_unloadTargetSlot < 0)
                        _unloadTargetSlot = ComputeUnloadTargetSlot();

                    if (_unloadTargetSlot < 0)
                    {
                        AxisOutputFeederY.EmgStop();
                        PostAlarm((int)AlarmKeys.Alarm_UnloadTargetSlotInvalid); // 2060
                        this.State = ProcessState.Error;
                        Log.Write(UnitName, "RunUnloadWaferFlowStep", "Unload target slot invalid");
                        return -1;
                    }

                    AdvanceUnloadStepOnSuccess(UnloadFlowStep.Step02);
                    goto case UnloadFlowStep.Step03;

                case UnloadFlowStep.Step03:
                    // Feeder -> Cassette
                    nRet = this.OutputCassetteLifter.MoveToSlot(_unloadTargetSlot);
                    if (nRet != 0)
                    {
                        AxisOutputFeederY.EmgStop();
                        PostAlarm((int)AlarmKeys.Alarm_CassetteMoveToSlotFailedForUnload); // 2062
                        this.State = ProcessState.Error;
                        Log.Write(UnitName, "RunUnloadWaferFlowStep", "OutputCassetteLifter.MoveToSlot Failed");
                        MarkUnloadStepOnFailure(UnloadFlowStep.Step03);
                        return nRet;
                    }

                    nRet = UnloadOnlyFeederToCassette(true);
                    if (nRet != 0)
                    {
                        AxisOutputFeederY.EmgStop();
                        // UnloadOnlyFeederToCassette 내부에서 이미 알람 발생함
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

            // 3. 완료 처리
            _unloadStep = UnloadFlowStep.Completed;
            _unloadTargetSlot = -1;
            NeedUnloadFirst = false;

            // Auto 모드일 때만 상태 변경 (Manual에서는 호출자가 제어할 수 있음)
            if (this.RunMode == UnitRunMode.Auto)
            {
                this.State = ProcessState.Complete;
            }
            else
            {
                // Manual 모드 완료 로그
                Log.Write(UnitName, "RunUnloadWaferFlowStep", "Manual Unloading Sequence Completed.");
            }

            return 0;
        }

        private int RunLoadWaferFlowStep(out bool didLoad, bool isFine)
        {
            int nRet = 0;
            didLoad = false;

            // 4) 스텝 머신 실행
            switch (_loadStep)
            {
                case LoadFlowStep.Step01:
                    didLoad = true;
                    // [Step 1] Cassette Next Slot 이동 + Stage 로딩 준비
                    nRet = BinLoading_Step01(true);

                    // [ADD] 대기(Blocked) 상태 등 0이 아닌 특수 리턴 처리 필요시 여기에 추가
                    if (nRet != 0)
                    {
                        AxisOutputFeederY.EmgStop();
                        // Step01 내부에서 이미 알람 발생함
                        this.State = ProcessState.Error;
                        MarkStepOnFailure(LoadFlowStep.Step01);
                        return nRet;
                    }
                    if (IsStop) { MarkStepOnFailure(LoadFlowStep.Step01); return 0; }

                    AdvanceLoadStepOnSuccess(LoadFlowStep.Step01);
                    goto case LoadFlowStep.Step02;

                case LoadFlowStep.Step02:
                    didLoad = true;
                    // [Step 2] Feeder가 Cassette에서 Bin 픽업 (바코드 포함)
                    nRet = BinLoading_Step02(true);
                    if (nRet != 0)
                    {
                        AxisOutputFeederY.EmgStop();
                        this.State = ProcessState.Error;
                        MarkStepOnFailure(LoadFlowStep.Step02);
                        return nRet;
                    }
                    if (IsStop) { MarkStepOnFailure(LoadFlowStep.Step02); return 0; }

                    AdvanceLoadStepOnSuccess(LoadFlowStep.Step02);
                    goto case LoadFlowStep.Step03;

                case LoadFlowStep.Step03:
                    didLoad = true;
                    // [Step 3] Feeder -> Stage 로딩 + Ready 복귀
                    nRet = BinLoading_Step03(true);
                    if (nRet != 0)
                    {
                        AxisOutputFeederY.EmgStop();
                        this.State = ProcessState.Error;
                        MarkStepOnFailure(LoadFlowStep.Step03);
                        return nRet;
                    }
                    if (IsStop) { MarkStepOnFailure(LoadFlowStep.Step03); return 0; }

                    AdvanceLoadStepOnSuccess(LoadFlowStep.Step03);
                    goto case LoadFlowStep.Step04;

                case LoadFlowStep.Step04:
                    didLoad = true;
                    // [Step 4] 데이터 매핑 (SetMappingData)
                    nRet = BinLoading_Step04(true);
                    if (nRet != 0)
                    {
                        AxisOutputFeederY.EmgStop();
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

            // 모든 스텝 완료 시
            _loadStep = LoadFlowStep.Completed;
            return 0;
        }

        // Feeder가 wafer를 "잡고 있다/보유 중"인지 판단
        // - 실기: RingPresent 센서 + (클램프 상태/객체) 조합
        // - 시뮬/드라이런: Material 객체 기준이 더 신뢰됨
        private bool HasBinOnFeeder()
        {
            try
            {
                // 시뮬/드라이런이면 센서보다 Material이 기준
                if (Config.IsSimulation || (Config.IsDryRun || IsDryRunEqp))
                    return (GetMaterial() is MaterialWafer);

                bool sensor = false;
                try { sensor = IsRingPresent(); } catch { sensor = false; }

                bool obj = (GetMaterial() is MaterialWafer);

                // 센서가 ON이면 우선 wafer 있다고 판단
                if (sensor) return true;

                // 센서 OFF인데 객체가 있으면 데이터만 남은 상태일 수 있으니 "보유"로 간주(안전 우선)
                if (obj) return true;

                return false;
            }
            catch
            {
                // 예외 시 안전측: 있다고 간주하면 이동을 막는 방향이 안전함
                return true;
            }
        }

        // Ready 이동이 가능한 "안전 상태"인지 판단
        // 현재 코드 기준: Ready로 갈 때 UnClamp 상태를 요구하므로 그 조건을 명확히 둠
        private bool IsSafeToMoveReady()
        {
            try
            {
                // 시뮬/드라이런이면 UnClamp 체크가 의미 약함
                if (Config.IsSimulation || (Config.IsDryRun || IsDryRunEqp))
                    return true;

                // MovePositionReady() 내부가 IsUnClamped()를 강제하므로 여기서도 동일 조건 사용
                return IsUnClamped();
            }
            catch
            {
                return false;
            }
        }
    }
}
