using LCP_280;
using QMC.Common;
using QMC.Common.Alarm;
using QMC.Common.Component;
using QMC.Common.IOUtil;
using QMC.Common.Motion;
using QMC.Common.Motions;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Component; // TeachingPosition
using QMC.LCP_280.Process.Unit.FormMain;
using QMC.LCP_280.Process.Unit.FormWork.Repro;
using QMC.LCP_280.Process.Work;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Navigation;
using static QMC.LCP_280.Process.Equipment;
using static QMC.LCP_280.Process.Unit.FormSetup.BarcoderControl;

namespace QMC.LCP_280.Process.Unit
{
    public class InputFeeder : BaseUnit<InputFeederConfig>
    {
        public new enum AlarmKeys
        {
            // ===== 기존 알람(의미 유지 / 문구 개선) =====
            Alarm_WaferLoadingFailed = 10101,
            Alarm_BarcodeReadingFailed = 10102,
            Alarm_StageLoadingFailed = 10103,
            Alarm_StageUnloadingFailed = 10104,
            Alarm_WaferUnloadingFailed = 10105,

            Alarm_InputStageInterlockFailed = 10106,

            Alarm_GripperClampFailed = 10107,
            Alarm_FeederClampUp = 10108,
            Alarm_IsWaferReadyForLoading = 10109,
            Alarm_WaferLoadingPosition = 10110,
            Alarm_InputCassetteLifterInterlockFailed = 10111,
            Alarm_InputFeederNoPosition = 10112,
            Alarm_InputFeederInterlockFailed = 10113,
            Alarm_GripperUnClampFailed = 10114,
            Alarm_WaferDataFaild = 10115,

            // ===== 추가/명확화(현 코드에서 실제로 구분 필요) =====
            Alarm_FeederLiftUpTimeout = 10116,
            Alarm_FeederLiftDownTimeout = 10117,
            Alarm_FeederClampTimeout = 10118,
            Alarm_FeederUnclampTimeout = 10119,

            Alarm_WaferMissingAfterStageToFeeder = 10120,
            Alarm_WaferMissingAfterFeederToCassette = 10121,
            Alarm_WaferSensorDataMismatch = 10122,

            // ===== 기존에 값이 명시되지 않아 위험하던 항목 =====
            Alarm_VerifyWaferMovedStageToFeeder = 10123,
            Alarm_AlignT = 10124,
            Alarm_UnloadTargetSlotInvalid = 10125,
            Alarm_CassetteSlotNotEmptyForUnload = 10126,
            Alarm_CassetteMoveToSlotFailedForUnload = 10127,

            Alarm_UnloadFeederToCassette_MoveFeederToCassettePosFailed = 10128,
            Alarm_UnloadFeederToCassette_UnclampFailed = 10129,
            Alarm_UnloadFeederToCassette_WaferDataInvalid = 10130,
            Alarm_UnloadFeederToCassette_MoveStandbyBarcodeFailed = 10131,
            Alarm_UnloadFeederToCassette_MoveStandbyReadyFailed = 10132,

            //IsRingPresent
            Alarm_RingPresentFailed = 10133,
        }
        #region InitAlarm
        protected override void InitAlarm()
        {
            string source = "Wafer_Feeder";
            base.InitAlarm();
            LogDuplicateAlarmCodes();

            // 1. 공용 파일 로더에서 알람 목록 가져오기
            var loadedAlarms = GlobalAlarmTable.Instance.GetAlarmsForSource(source);
            if (loadedAlarms == null || loadedAlarms.Count == 0)
            {
                Log.Write("AlarmInit", $"Cannot find alarms for source '{source}' in the alarm file. Only default alarms will be registered.");



                // 2000~2004: Flow 실패(상위 레벨)
                AlarmRegister((int)AlarmKeys.Alarm_WaferLoadingFailed,
                    "InputFeeder Wafer Loading Failed",
                    "InputFeeder loading sequence failed. (Please check Cassette/Barcode/Feeder/Stage status and interlocks.)",
                     source, "Error");

                AlarmRegister((int)AlarmKeys.Alarm_BarcodeReadingFailed,
                    "InputFeeder Barcode Read Failed",
                    "Barcode reading failed. Please check barcode print status/reader status/barcode position (Teaching) and trigger settings.",
                     source, "Error");

                AlarmRegister((int)AlarmKeys.Alarm_StageLoadingFailed,
                    "InputStage Loading Failed",
                    "Stage loading failed. Please check stage position/clamp/plate status and interlocks.",
                     source, "Error");

                AlarmRegister((int)AlarmKeys.Alarm_StageUnloadingFailed,
                    "InputStage Unloading Failed",
                    "Stage unloading failed. Please check stage preparation (unloading position/clamp/plate) and interlocks.",
                     source, "Error");

                AlarmRegister((int)AlarmKeys.Alarm_WaferUnloadingFailed,
                    "InputFeeder Wafer Unloading Failed",
                    "InputFeeder unloading sequence failed. (Please check Feeder/Stage/Cassette status and wafer presence.)",
                     source, "Error");

                // 2010~: 인터락
                AlarmRegister((int)AlarmKeys.Alarm_InputStageInterlockFailed,
                    "Interlock Failed - InputStage",
                    "Operation stopped due to interlock mismatch. InputStage may not be in the loading/unloading position, or it may be in a dangerous state such as axis moving/plate UP/clamp lift UP.",
                     source, "Error");

                AlarmRegister((int)AlarmKeys.Alarm_InputFeederInterlockFailed,
                    "Interlock Failed - InputFeeder",
                    "Operation stopped due to interlock mismatch. Please check Feeder position/clamp status/safety conditions.",
                     source, "Error");

                AlarmRegister((int)AlarmKeys.Alarm_InputFeederNoPosition,
                    "InputFeeder Unknown Position",
                    "Current Feeder Y does not correspond to any Teaching Position (Ready/Barcode/Stage/Cassette). Move to the safe position (Ready) manually and check Teaching/Origin status.",
                     source, "Error");

                AlarmRegister((int)AlarmKeys.Alarm_InputCassetteLifterInterlockFailed,
                    "Interlock Failed - InputCassetteLifter",
                    "Cassette lifter interlock mismatch. Please check cassette presence/lifter axis movement/Ready for Loading signal.",
                     source, "Error");

                AlarmRegister((int)AlarmKeys.Alarm_IsWaferReadyForLoading,
                    "Cassette Not Ready For Loading",
                    "Cassette Ready For Loading signal is OFF. Please check cassette installation status/lifter position/sensor status.",
                     source, "Error");

                AlarmRegister((int)AlarmKeys.Alarm_WaferLoadingPosition,
                    "InputStage Not In Loading Position",
                    "InputStage is not in the Wafer Loading Position. Please move to the stage loading position and try again.",
                     source, "Error");

                // 2020~: 실린더/그리퍼
                AlarmRegister((int)AlarmKeys.Alarm_GripperClampFailed,
                    "Feeder Clamp Failed",
                    "Clamp operation failed (clamp completion signal not confirmed). Please check air/valve/cylinder/sensor status and interference.",
                     source, "Error");

                AlarmRegister((int)AlarmKeys.Alarm_GripperUnClampFailed,
                    "Feeder Unclamp Failed",
                    "Unclamp operation failed (unclamp completion signal not confirmed). Please check air/valve/cylinder/sensor status and interference.",
                     source, "Error");

                AlarmRegister((int)AlarmKeys.Alarm_FeederClampUp,
                    "Feeder Lift Up Required",
                    "Feeder is not in the UP state. Please check the Feeder Lift (UP) sensor/air/valve status.",
                     source, "Error");

                // 2030~: 타임아웃(정확 원인 분리)
                AlarmRegister((int)AlarmKeys.Alarm_FeederLiftUpTimeout,
                    "Feeder Lift Up Timeout",
                    "Feeder Lift UP timeout. Please check UP sensor input/air pressure/valve/cylinder/mechanism interference.",
                     source, "Error");

                AlarmRegister((int)AlarmKeys.Alarm_FeederLiftDownTimeout,
                    "Feeder Lift Down Timeout",
                    "Feeder Lift DOWN timeout. Please check DOWN sensor input/air pressure/valve/cylinder/mechanism interference.",
                     source, "Error");

                AlarmRegister((int)AlarmKeys.Alarm_FeederClampTimeout,
                    "Feeder Clamp Timeout",
                    "Feeder Clamp timeout. Please check Clamp sensor input/air pressure/valve/cylinder/mechanism interference.",
                     source, "Error");

                AlarmRegister((int)AlarmKeys.Alarm_FeederUnclampTimeout,
                    "Feeder Unclamp Timeout",
                    "Feeder Unclamp timeout. Please check Unclamp sensor input/air pressure/valve/cylinder/mechanism interference.",
                     source, "Error");

                // 2040~: 유실/정합성
                AlarmRegister((int)AlarmKeys.Alarm_WaferDataFaild,
                    "Wafer Data Mismatch",
                    "Wafer sensor status and data object (Material) status do not match. (e.g., sensor is ON but object is null, object exists but sensor is OFF) You may need to check inside the equipment and reset the data.",
                     source, "Error");

                AlarmRegister((int)AlarmKeys.Alarm_WaferSensorDataMismatch,
                    "Wafer Sensor/Data Inconsistency",
                    "Wafer sensor/data consistency error. Please check sensor input and internal wafer object status.",
                     source, "Error");

                AlarmRegister((int)AlarmKeys.Alarm_WaferMissingAfterStageToFeeder,
                    "Wafer Missing After Stage -> Feeder",
                    "Wafer is not detected in Feeder after transferring from Stage to Feeder. There is a possibility of wafer loss/drop/gripper unclamp.",
                     source, "Error");

                AlarmRegister((int)AlarmKeys.Alarm_WaferMissingAfterFeederToCassette,
                    "Wafer Missing After Feeder -> Cassette",
                    "Feeder is not emptied (sensor ON/object remaining) or wafer status is abnormal after discharging from Feeder to Cassette. Please check discharge operation/sensor status.",
                     source, "Error");

                // 2050~: 기존 명칭 정리(필요시 유지)
                AlarmRegister((int)AlarmKeys.Alarm_VerifyWaferMovedStageToFeeder,
                    "Verify Transfer Stage -> Feeder Failed",
                    "Verification of transfer from Stage -> Feeder failed. Please check sensor/data consistency and SlotIndex match.",
                     source, "Error");

                AlarmRegister((int)AlarmKeys.Alarm_AlignT,
                    "InputStage Align(T) Failed",
                    "InputStage Align(T) failed. Please check alignment conditions/vision/axis status.",
                     source, "Error");

                AlarmRegister((int)AlarmKeys.Alarm_UnloadTargetSlotInvalid,
                    "Unload Target Slot Invalid",
                    "Cannot determine the target slot (SlotIndex) for unloading. (Please check Feeder/Stage/Lifter SlotIndex)",
                     source, "Error");

                AlarmRegister((int)AlarmKeys.Alarm_CassetteSlotNotEmptyForUnload,
                    "Cassette Slot Not Empty",
                    "Target Cassette Slot for unloading is not empty. (Please check Slot Empty status/mapping data)",
                     source, "Error");

                AlarmRegister((int)AlarmKeys.Alarm_CassetteMoveToSlotFailedForUnload,
                    "Cassette MoveToSlot Failed",
                    "Failed to move Cassette to the target Slot for unloading. Please check axis status/interlock/limit/servo status.",
                     source, "Error");

                AlarmRegister((int)AlarmKeys.Alarm_UnloadFeederToCassette_MoveFeederToCassettePosFailed,
                    "Unload Feeder->Cassette Failed - Move Position Cassette",
                    "Failed to move to Cassette Teaching Position during Feeder->Cassette unloading. (Check Y axis status/interlock/Teaching/servo alarm)",
                     source, "Error");

                AlarmRegister((int)AlarmKeys.Alarm_UnloadFeederToCassette_UnclampFailed,
                    "Unload Feeder->Cassette Failed - Unclamp",
                    "Unclamp failure/timeout during Feeder->Cassette unloading. (Check air pressure/valve/cylinder/sensor/interference)",
                     source, "Error");

                AlarmRegister((int)AlarmKeys.Alarm_UnloadFeederToCassette_WaferDataInvalid,
                    "Unload Feeder->Cassette Failed - Wafer Data Invalid",
                    "No wafer data in Feeder or SlotIndex is invalid, so it cannot be applied to Cassette. (Check sensor/Material object consistency)",
                     source, "Error");

                AlarmRegister((int)AlarmKeys.Alarm_UnloadFeederToCassette_MoveStandbyBarcodeFailed,
                    "Unload Feeder->Cassette Failed - Move Standby Barcode",
                    "Failed to move to the next loading standby (Barcode) position after unloading. (Check Y axis status/Teaching/interlock)",
                     source, "Error");

                AlarmRegister((int)AlarmKeys.Alarm_UnloadFeederToCassette_MoveStandbyReadyFailed,
                    "Unload Feeder->Cassette Failed - Move Standby Ready",
                    "Failed to move to the safe standby (Ready) position after unloading. (Check Y axis status/Teaching/interlock)",
                     source, "Error");

                //Alarm_RingPresentFailed
                AlarmRegister((int)AlarmKeys.Alarm_RingPresentFailed,
                    "Feeder Ring Present Check Failed",
                    "Failed to check Ring Present status of Feeder. Sensor/data consistency check is required.",
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
        //enum 코드 중복 진단 로그 추가
        private void LogDuplicateAlarmCodes()
        {
            try
            {
                var dup = Enum.GetValues(typeof(AlarmKeys))
                    .Cast<int>()
                    .GroupBy(v => v)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                if (dup.Count > 0)
                    Log.Write(UnitName, $"[Alarm] Duplicate alarm codes detected: {string.Join(",", dup)}");
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }
        #endregion

        #region Unit
        public InputCassetteLifter InputCassetteLifterUnit { get; set; }
        public InputStage InputStageUnit { get; set; }

        private InputDieTransfer InputDieTransferUnit { get; set; }
        private Rotary RotaryUnit { get; set; }
        private OutputDieTransfer OutputDieTransferUnit { get; set; }
        private OutputStage OutputStageUnit { get; set; }
        #endregion

        #region Axes
        private MotionAxis _feederY;
        public MotionAxis AxisInputFeederY => _feederY;
        #endregion

        // Safety 동작 중 여부
        private bool _isSafetyMoving = false;
        private const int PollSleepMs = 5;  

        #region IO Domain Members
        private Cylinder _feederLift; // Up/Down
        private Cylinder _cylClamp;   // Clamp/Unclamp
        #endregion

        #region Constructor / Initialization
        public InputFeeder(InputFeederConfig config = null)
            : base(config ?? new InputFeederConfig())
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
                AxisInputFeederY.Config.IsSimulation = true;
                Log.Write(UnitName, "Simulation Mode");
            }
        }
        protected override void OnBindUnit()
        {
            base.OnBindUnit();
            InputCassetteLifterUnit = Equipment.Instance.GetUnit("InputCassetteLifter") as InputCassetteLifter;
            InputStageUnit = Equipment.Instance.GetUnit("InputStage") as InputStage;

            InputDieTransferUnit = Equipment.Instance.GetUnit(UnitKeys.InputDieTransfer) as InputDieTransfer;
            RotaryUnit = Equipment.Instance.GetUnit(UnitKeys.Rotary) as Rotary;
            OutputDieTransferUnit = Equipment.Instance.GetUnit(UnitKeys.OutputDieTransfer) as OutputDieTransfer;
            OutputStageUnit = Equipment.Instance.GetUnit(UnitKeys.OutputStage) as OutputStage;
        }
        #endregion

        #region Axis Binding
        private void BindAxes()
        {
            var mgr = Equipment.Instance?.AxisManager;
            if (mgr == null)
            {
                Log.Write(UnitName, "[BindAxes] AxisManager null");
                return;
            }

            const string unitName = "Unit"; // Equipment
            BindAxis(mgr, unitName, AxisNames.WaferFeederY, ref _feederY);
        }
        #endregion

        public override bool IsInterlockOK(BaseComponent baseComponent, BaseComponent.InterlockEventArgs e)
        {
            bool bRet = base.IsInterlockOK(baseComponent, e);
            if (baseComponent == this.AxisInputFeederY)
            {
                if (_isSafetyMoving)
                    return true;

                if (this.IsFeederDown())
                {
                    if (this.InputStageUnit.IsPositionWaferLoading() == false
                        && this.InputStageUnit.IsPositionWaferUnloading() == false)
                    {
                        this.AxisInputFeederY?.EmgStop();
                        PostAlarm((int)AlarmKeys.Alarm_InputStageInterlockFailed);
                        bRet = false;
                    }
                    else
                    {
                        if (this.IsPositionCassette())
                        {
                            bRet = IsInterlockOKWithCassette(e);
                            if (bRet == false)
                            {
                                this.AxisInputFeederY?.EmgStop();
                                PostAlarm((int)AlarmKeys.Alarm_InputStageInterlockFailed);
                                return bRet;
                            }
                        }
                    }
                }
            }
            else if (baseComponent == this._feederLift)
            {
                if (this.IsPositionCassette())
                {
                    PostAlarm((int)AlarmKeys.Alarm_InputStageInterlockFailed);
                    bRet = false;
                }
            }
            return bRet;
        }

        private bool IsInterlockOKWithCassette(BaseComponent.InterlockEventArgs e)
        {
            if (this.InputStageUnit.IsPlateUp() || this.InputStageUnit.IsClampLiftUp())
            {
                double dCurrentY = this.AxisInputFeederY.GetPosition();
                double dStageY = this.GetTP(InputFeederConfig.TeachingPositionName.Cassette.ToString(), this.AxisInputFeederY.Name);
                if (dCurrentY > dStageY + this.AxisInputFeederY.Config.InposTolerance)
                {
                    return false;
                }
            }
            return true;
        }

        private bool IsInterlockOKBinLoading()
        {
            bool bRtn = true;
            if (InputCassetteLifterUnit.IsWaferReadyForLoading() == false)
            {
                Log.Write(this, "InputCassetteLifter Not Ready for Loading");
                bRtn = false;
                return bRtn;
            }

            if (InputStageUnit.IsPositionWaferLoading() == false)
            {
                if (InputStageUnit.IsStageInterLockOK() == false)
                {
                    Log.Write(this, "InputStage Not Ready for Loading");
                    bRtn = false;
                    return bRtn;
                }
            }

            return bRtn;
        }
        private bool IsInterlockOKMoveToCassette()
        {
            bool isOK = true; //this.InputStageUnit.IsPositionWaferLoading();
            
            if(this.InputStageUnit.IsPositionWaferLoading() == false
                && this.InputStageUnit.IsPositionWaferUnloading() == false)
            {
                isOK = false;
            }

            //이건 확인이 필요한데. 언로딩할수도있는건데 로딩만 확인한다...
            isOK &= this.InputCassetteLifterUnit.IsWaferReadyForLoading();
            
            return isOK;
        }

        #region New Wafer Missing Handling Helpers
        // [ADD] 언로딩 시 웨이퍼 존재/데이터 정합성 검사 및 처리 헬퍼
        private int CheckStageWaferBeforeUnload(MaterialWafer waferStage)
        {
            // Stage 센서가 존재한다고 알려주는데 객체가 null -> 데이터 불일치
            if (InputStageUnit.IsRingPresent())
            {
                if (waferStage == null)
                {
                    Log.Write(UnitName, "[Unload] Stage ring detected but wafer object null");
                    PostAlarm((int)AlarmKeys.Alarm_WaferDataFaild);
                    return -1;
                }
                return 0;
            }

            // 객체는 있는데 센서는 없음 -> 데이터 불일치
            if (waferStage != null && !InputStageUnit.IsRingPresent())
            {
                Log.Write(UnitName, "[Unload] Wafer object exists but stage sensor off");
                PostAlarm((int)AlarmKeys.Alarm_WaferDataFaild);
                return -1;
            }

            // 센서도 없고 객체도 없음 -> 언로드할 웨이퍼 없음 (정상적으로 이미 제거된 상황일 수 있음)
            if (waferStage == null)
            {
                Log.Write(UnitName, "[Unload] No wafer on stage, skip stage unloading");
                NeedUnloadFirst = false;
                return -2; // 상위에서 Skip 용도로 사용
            }



            return 0;
        }
        // [ADD] 피더로 옮긴 후 피더 상태 검증
        private int VerifyWaferMovedStageToFeeder(MaterialWafer waferMoved)
        {
            bool feederSensor = IsRingPresent();
            var feederObj = GetMaterial() as MaterialWafer;

            // 기대: Stage->Feeder 후에는 Feeder에 wafer가 있어야 함
            if (feederSensor == false && feederObj == null)
            {
                Log.Write(UnitName, "[Unload] Wafer missing on feeder after stage -> feeder transfer");
                PostAlarm((int)AlarmKeys.Alarm_WaferMissingAfterStageToFeeder);
                return -1;
            }

            if (feederSensor && feederObj == null)
            {
                Log.Write(UnitName, "[Unload] Feeder ring detected but object null");
                PostAlarm((int)AlarmKeys.Alarm_WaferDataFaild);
                return -1;
            }

            var equipment = Equipment.Instance;
            bool IsDryRunEqp = equipment.EquipmentConfig.IsDryRun;
            if (feederObj != null && !feederSensor && !(Config.IsSimulation || (Config.IsDryRun || IsDryRunEqp)))
            {
                Log.Write(UnitName, "[Unload] Feeder object exists but feeder sensor off");
                PostAlarm((int)AlarmKeys.Alarm_WaferSensorDataMismatch);
                return -1;
            }

            if (feederObj != null && waferMoved != null)
            {
                if (feederObj.SlotIndex != waferMoved.SlotIndex && waferMoved.SlotIndex >= 0)
                {
                    Log.Write(UnitName, "[Unload] SlotIndex mismatch (Stage:" + waferMoved.SlotIndex + ", Feeder:" + feederObj.SlotIndex + ")");
                    PostAlarm((int)AlarmKeys.Alarm_WaferDataFaild);
                    return -1;
                }
            }
            return 0;
        }
        // [ADD] Cassette 언로딩 이후 최종 상태 점검
        private int VerifyAfterUnloadToCassette(int slotIndex)
        {
            bool feederSensor = IsRingPresent();
            var feederObj = GetMaterial() as MaterialWafer;

            // 정상: 피더 센서 OFF + 객체 null
            if (!feederSensor && feederObj == null)
            {
                Log.Write(UnitName, "[Unload] Completed -> Feeder empty OK (Slot:" + slotIndex + ")");
                return 0;
            }

            if (!feederSensor && feederObj != null)
            {
                Log.Write(UnitName, "[Unload] Feeder object remained although sensor off -> force clear");
                SetMaterial(null);
                return 0;
            }

            // Feeder->Cassette 수행이 끝났는데 센서가 ON이면 "배출 후에도 남아있다/유실" 계열
            if (feederSensor && feederObj == null)
            {
                Log.Write(UnitName, "[Unload] Sensor ON but no wafer object -> lost wafer?");
                PostAlarm((int)AlarmKeys.Alarm_WaferMissingAfterFeederToCassette);
                return -1;
            }

            if (feederSensor && feederObj != null)
            {
                Log.Write(UnitName, "[Unload] Feeder still holds wafer after unload-to-cassette step");
                PostAlarm((int)AlarmKeys.Alarm_WaferMissingAfterFeederToCassette);
                return -1;
            }

            return 0;
        }
        #endregion

        // Move with Interlock Check
        public int MovePositionReady(bool isFine = false)
        {
            bool bRet = false;
            bRet = InPosTeaching(TeachingPositions[(int)InputFeederConfig.TeachingPositionName.Ready]);
            if (bRet)
            {
                return 0;
            }

            Task<int> task = MovePositionAsyncReady(isFine);
            while (IsEndTask(task) == false)
            {
                if (RunMode == UnitRunMode.Auto)
                {
                    //Ready 가는데 이거는 애매한디.
                    //if (IsInterlockOKWaferLoading() == false)
                    //{
                    //    AxisInputFeederY.EmgStop();
                    //    PostAlarm((int)AlarmKeys.Alarm_WaferLoadingFailed);
                    //    return -1;
                    //}
                    IsMoveInterLockReady();
                }
                else if (RunMode == UnitRunMode.Manual)
                {
                    IsMoveInterLockReady();
                }

                Thread.Sleep(1);
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
                return MoveTeachingPositionOnce((int)InputFeederConfig.TeachingPositionName.Ready, isFine);
            }
            finally
            {
                _isSafetyMoving = false;
            }
        }
        private int IsMoveInterLockReady()
        {
            int nRet = 0;

            // Ready 이동 중에는 Unclamp 상태가 필수
            if (IsUnClamped() == false)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_InputFeederInterlockFailed);
                Log.Write(this, "CheckMoveInterLockReady Fail - IsRingPresent()");
                nRet = -1;
                return nRet;
            }

            if (InputStageUnit.IsAnyAxisMoving())
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_InputStageInterlockFailed);
                Log.Write(UnitName, "IsMoveInterLockReady", "InputStage 축 이동중.");
                nRet = -1;
                return nRet;
            }

            // Cassette or InputStage 위치 및 Signal 확인 후 진행. 
            if (!InputCassetteLifterUnit.IsWaferReadyForLoading() || !InputStageUnit.IsPositionWaferLoading())
            {
                if (!IsFeederUp())
                {
                    AxisInputFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_FeederClampUp);
                    Log.Write(UnitName, "IsMoveInterLockReady", "Feeder Up Fail.");
                    nRet = -1;
                    return nRet;
                }
            }

            return nRet;
        }

        public int MovePositionStage(bool isFine = false)
        {
            if(IsPositionStage() == true)
            {
                return 0;
            }
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
            return MoveTeachingPositionOnce((int)InputFeederConfig.TeachingPositionName.Stage, isFine);
        }
        private int IsMoveInterLockStage()
        {
            int nRet = 0;
            // Check Interlock.!!! 구문 넣을것.!!!
            //if (IsFeederUp())
            //{
            //    FeederY.EmgStop();
            //    PostAlarm((int)AlarmKeys.Alarm_FeederClampUp);
            //    nRet = -1;
            //    return nRet;
            //}

            if (InputStageUnit.IsAnyAxisMoving())
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_InputStageInterlockFailed);
                nRet = -1;
                return nRet;
            }

            return nRet;
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
            return MoveTeachingPositionOnce((int)InputFeederConfig.TeachingPositionName.Barcode, isFine);
        }
        private int IsMoveInterLockBarcode()
        {
            int nRet = 0;
            // Check Interlock.!!! 구문 넣을것.!!!
            //if (IsFeederUp())
            //{
            //    FeederY.EmgStop();
            //    PostAlarm((int)AlarmKeys.Alarm_FeederClampUp);
            //    nRet = -1;
            //    return nRet;
            //}

            if (InputStageUnit.IsAnyAxisMoving())
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_InputStageInterlockFailed);
                nRet = -1;
                return nRet;
            }

            if (InputCassetteLifterUnit.IsAnyAxisMoving())
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_InputStageInterlockFailed);
                nRet = -1;
                return nRet;
            }

            return nRet;
        }

        public int MovePositionCassette(bool isFine = false)
        {
            if (IsPositionCassette())
                return 0;

            Task<int> task = MovePositionAsyncCassette(isFine);
            while (IsEndTask(task) == false)
            {
                if (IsInterlockOKMoveToCassette() == false)
                {
                    AxisInputFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_WaferLoadingFailed);
                    return -1;
                }

                IsMoveInterLockCassetteOk();
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
            return MoveTeachingPositionOnce((int)InputFeederConfig.TeachingPositionName.Cassette, isFine);
        }
        private bool IsMoveInterLockCassetteOk()
        {
            bool bRet = false;

            if (InputStageUnit.IsAnyAxisMoving())
            {
                bRet = false;
                return bRet;
            }

            if (InputCassetteLifterUnit.IsAnyAxisMoving())
            {
                bRet = false;
                return bRet;
            }

            if (InputStageUnit.IsPositionWaferLoading() == false
                &&InputStageUnit.IsPositionWaferUnloading() == false)
            {
                bRet = false;
                return bRet;
            }

            var equipment = Equipment.Instance;
            bool IsDryRunEqp = equipment.EquipmentConfig.IsDryRun;
            if (Config.IsSimulation == false && (Config.IsDryRun == false && IsDryRunEqp == false))
            {
                if (InputStageUnit.IsPlateUp() == true)
                {
                    bRet = false;
                    return bRet;
                }

                if (InputStageUnit.IsClampLiftUp() == true)
                {
                    bRet = false;
                    return bRet;
                }
            }

            bRet = true;
            return bRet;
        }


        public bool IsPositionFeederZSafety()
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
        public bool IsPositionFeederYSafety()
        {
            bool bRtn = false;
            if (AxisInputFeederY == null)
                return bRtn;

            var cfg = Config;
            if (cfg == null)
                return bRtn;

            bRtn = IsPositionReady();
            return bRtn;
        }

        public bool IsPositionSetPos()
        {
            var tp = TeachingPositions[(int)InputFeederConfig.TeachingPositionName.SetPosition];
            if (tp == null)
                return false;
            return InPosTeaching(tp);
        }

        public bool IsPositionReady()
        {
            var tp = TeachingPositions[(int)InputFeederConfig.TeachingPositionName.Ready];
            if (tp == null)
                return false;
            return InPosTeaching(tp);
        }
        public bool IsPositionBarcode()
        {
            var tp = TeachingPositions[(int)InputFeederConfig.TeachingPositionName.Barcode];
            if (tp == null)
                return false;
            return InPosTeaching(tp);
        }
        public bool IsPositionStage()
        {
            var tp = TeachingPositions[(int)InputFeederConfig.TeachingPositionName.Stage];
            if (tp == null)
                return false;
            return InPosTeaching(tp);
        }
        public bool IsPositionCassette()
        {
            var tp = TeachingPositions[(int)InputFeederConfig.TeachingPositionName.Cassette];
            if (tp == null)
                return false;
            return InPosTeaching(tp);
        }


        #region Teaching Helpers
        public int MoveToTeachingPosition(string positionName, bool isFine)
        {
            if (string.IsNullOrWhiteSpace(positionName))
            {
                Log.Write(UnitName, nameof(MoveToTeachingPosition),
                        $"[TeachingMove] TeachingPositions에서 '{positionName}' 을 찾지 못했습니다.");
                return -1;
            }

            int result = 0;

            InputFeederConfig.TeachingPositionName en;
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

        #region Low-Level IO (Read/Write by Name)
        #endregion

        #region IO Domain Mapping
        private void BindIoDomains()
        {
            var eq = Equipment.Instance; var unit = eq?.UnitIO; if (unit == null) return;

            if (!IoAutoBindings.Cylinders.TryGetValue("InFeederLift", out _feederLift))
            {
                Log.Write("InputFeeder", "BindIoDomains", "Cylinder not found: InFeederLift");
            }
            BindCylinder(_feederLift);

            if (!IoAutoBindings.Cylinders.TryGetValue("InFeederClamp", out _cylClamp))
            {
                Log.Write("InputFeeder", "BindIoDomains", "Cylinder not found: InFeederClamp");
            }
            BindCylinder(_cylClamp);
        }
        #endregion

        #region Status Helpers
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

            bool bRet = false;
            if (bUpDn)
                bRet = _cylClamp.Extend();
            else
                bRet = _cylClamp.Retract();

            return bRet;
        }
        public bool IsFeederUp()
        {
            if (Config.IsSimulation)
            {
                return true;
            }
            return this.ReadInput(InputFeederConfig.IO.FEEDER_UP);
        }
        public bool IsFeederDown()
        {
            bool bRet = false;
            if (Config.IsSimulation)
            {
                bRet = true;
                return bRet;
            }
            bRet = this.ReadInput(InputFeederConfig.IO.FEEDER_DOWN);

            return bRet;
        }
        public bool IsClamped()
        {
            bool bRet = false;
            if (Config.IsSimulation)
            {
                bRet = true;
                return bRet;
            }
            bRet = !this.ReadInput(InputFeederConfig.IO.FEEDER_UNCLAMP);
            return bRet;
        }
        public bool IsUnClamped()
        {
            bool bRet = false;
            if (Config.IsSimulation)
            {
                return true;
            }
            bRet = this.ReadInput(InputFeederConfig.IO.FEEDER_UNCLAMP);
            return bRet;
        }
        public bool IsRingPresent()
        {
            bool bRet = false;

            var equipment = Equipment.Instance;
            bool IsDryRunEqp = equipment.EquipmentConfig.IsDryRun;
            if (Config.IsSimulation || (Config.IsDryRun || IsDryRunEqp))
            {
                bRet = this.GetMaterial() is MaterialWafer;
                return bRet;
            }
            bRet =  this.ReadInput(InputFeederConfig.IO.FEEDER_RING_CHECK);
            return bRet;
        }
        public bool IsOverload()
        {
            var equipment = Equipment.Instance;
            bool IsDryRunEqp = equipment.EquipmentConfig.IsDryRun;
            if (Config.IsSimulation || (Config.IsDryRun || IsDryRunEqp))
            {
                return true;
            }
            return this.ReadInput(InputFeederConfig.IO.FEEDER_OVERLOAD);
        }

        // === Cylinder 완료 대기 Helper ===
        // Clamp: expectClamp=true(Clamp 완료 기대), false(Unclamp 완료 기대)
        private int WaitClampStateOrAlarm(bool expectClamp, int timeoutMs = 1500, int pollMs = 2)
        {
            var equipment = Equipment.Instance;
            bool IsDryRunEqp = equipment.EquipmentConfig.IsDryRun;
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

            // [FIX] 기존 실패 알람도 있으나, timeout 전용 알람 정의가 있으니 그것을 사용
            int alarm = expectClamp
                ? (int)AlarmKeys.Alarm_FeederClampTimeout
                : (int)AlarmKeys.Alarm_FeederUnclampTimeout;

            PostAlarm(alarm);
            Log.Write(UnitName, expectClamp ? "[Clamp] Gripper CLAMP timeout" : "[Clamp] Gripper UNCLAMP timeout");
            return -1;
        }
        // Lift: expectUp=true(UP 완료 기대), false(DOWN 완료 기대)
        private int WaitLiftStateOrAlarm(bool expectUp, int timeoutMs = 1500, int pollMs = 2)
        {
            var equipment = Equipment.Instance;
            bool IsDryRunEqp = equipment.EquipmentConfig.IsDryRun;
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

            // [FIX] Down 타임아웃을 로딩 실패로 뭉치지 않음
            int alarm = expectUp
                ? (int)AlarmKeys.Alarm_FeederLiftUpTimeout
                : (int)AlarmKeys.Alarm_FeederLiftDownTimeout;

            PostAlarm(alarm);
            Log.Write(UnitName, expectUp ? "[Lift] Feeder UP timeout" : "[Lift] Feeder DOWN timeout");
            return -1;
        }
        #endregion

        #region === Direct Valve Control ===
        public bool IsFeederUpValveOn() => this.IsOutputOn(InputFeederConfig.IO.FEEDER_UP_VALVE);
        public bool IsFeederDownValveOn() => this.IsOutputOn(InputFeederConfig.IO.FEEDER_DOWN_VALVE);
        public bool IsFeederClampValveOn() => this.IsOutputOn(InputFeederConfig.IO.FEEDER_CLAMP_VALVE);
        public bool IsFeederUnclampValveOn() => this.IsOutputOn(InputFeederConfig.IO.FEEDER_UNCLAMP_VALVE);
        #endregion


        #region Signals
        public bool IsWaferLoadDone { get; set; }
        bool NeedUnloadFirst { get; set; } = false;
        // 클래스 필드 영역
        private volatile bool _exchangeStandbyForNextLoad = false; // 언로드 후 다음 로딩을 바코드에서 시작
        public bool ExchangeStandbyForNextLoad
        {
            get { return _exchangeStandbyForNextLoad; }
        }

        private volatile bool _nextDoScanAndLoad = false;     // Ready에서 결정: Scan+Load를 수행할지
        private volatile bool _nextStandbyAtBarcode = false;  // Ready에서 결정: Work 끝나고 바코드 대기 여부

        private enum LoadFlowStep
        {
            None = 0,
            Step01 = 1,   // MoveToNextSlot + PrepareLoadingStage
            Step02 = 2,   // UnloadWaferFromCassette (Barcode 포함)
            Step03 = 3,   // StageLoading + Material move + MoveToReady
            Step04 = 4,   // InputStage.LoadingWaferComplete + PreparetoInputStage(Align/Mapping)
            Completed = 5
        }

        private enum UnloadFlowStep
        {
            None = 0,
            Step01 = 1,   // CheckStageWaferBeforeUnload
            Step02 = 2,   // Stage -> Feeder (PrepareInputStageUnloadingWafer + Move + Clamp)
            Step03 = 3,   // Feeder -> Cassette (MoveToSlot + UnloadWaferFeederToCassette)
            Completed = 4
        }

        private LoadFlowStep _loadStep = LoadFlowStep.None;
        private UnloadFlowStep _unloadStep = UnloadFlowStep.None;
        private int _unloadTargetSlot = -1;
        #endregion

        private LoadFlowStep DetermineNextLoadStep()
        {
            // 이미 진행 중이면 유지 (재시작/재진입 시 이어가기)
            if (_loadStep == LoadFlowStep.Step01 ||
                _loadStep == LoadFlowStep.Step02 ||
                _loadStep == LoadFlowStep.Step03 ||
                _loadStep == LoadFlowStep.Step04)
                return _loadStep;

            // Feeder에 wafer가 남아 있으면 Stage로 올리는 Step03부터 재개
            bool feederHasWafer = HasValidWaferOnFeederForTransfer(); 
            //bool feederHasWafer = GetMaterial() is MaterialWafer;
            if (feederHasWafer)
                return LoadFlowStep.Step03;

            // [FIX] Scan+Load 계획이면, (Scan 전이라 hasMore가 false여도) Step01로 진입
            if (_nextDoScanAndLoad)
                return LoadFlowStep.Step01;

            // 카세트에 진행할 wafer가 있으면 정상 Step01부터 (Scan 완료된 상태)
            bool hasMore = InputCassetteLifterUnit?.IsHaveMoreProcessWafer() == true;
            if (hasMore)
                return LoadFlowStep.Step01;

            return LoadFlowStep.Completed;
        }

        private void AdvanceLoadStepOnSuccess(LoadFlowStep done)
        {
            switch (done)
            {
                case LoadFlowStep.Step01: _loadStep = LoadFlowStep.Step02; break;
                case LoadFlowStep.Step02: _loadStep = LoadFlowStep.Step03; break;
                case LoadFlowStep.Step03: _loadStep = LoadFlowStep.Step04; break;
                case LoadFlowStep.Step04: _loadStep = LoadFlowStep.Completed; break;
            }
        }

        private void MarkLoadStepOnFailure(LoadFlowStep failed)
        {
            _loadStep = failed;
        }

        private int ComputeUnloadTargetSlot()
        {
            // 우선순위: Feeder wafer slot -> Stage wafer slot -> Lifter current slot
            var feederObj = GetMaterial() as MaterialWafer;
            if (feederObj != null && feederObj.SlotIndex >= 0)
                return feederObj.SlotIndex;

            var stageObj = InputStageUnit?.GetMaterialWafer();
            if (stageObj != null && stageObj.SlotIndex >= 0)
                return stageObj.SlotIndex;

            int lifterSlot = InputCassetteLifterUnit?.GetCurrectSlotID() ?? -1;
            if (lifterSlot >= 0)
                return lifterSlot;

            return -1;
        }

        private UnloadFlowStep DetermineNextUnloadStep()
        {
            if (_unloadStep == UnloadFlowStep.Step01 ||
                _unloadStep == UnloadFlowStep.Step02 ||
                _unloadStep == UnloadFlowStep.Step03)
                return _unloadStep;

            // Stage에 wafer가 있고 Completed면 Step01부터
            var stageWafer = InputStageUnit?.GetMaterialWafer();
            bool stageHasWafer = InputStageUnit?.IsRingPresent() == true;

            if (NeedUnloadFirst && stageHasWafer)
                return UnloadFlowStep.Step01;

            // Stage가 비어도 Feeder에 wafer가 남아있으면 Step03(Feeder->Cassette)부터 재개
            bool feederHasWafer = (GetMaterial() is MaterialWafer) || IsRingPresent();
            if (feederHasWafer)
            {
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
            _unloadStep = failed;
        }


        public int RunUnloadWaferFlowStep(bool isFine)
        {
            int nRet = 0;

            switch (_unloadStep)
            {
                case UnloadFlowStep.Step01:

                    try
                    {
                        var ctx = Equipment.Instance.SummaryContext;
                        ctx.GetCurrentSummaryOrNull()?.StartUnload();
                    }
                    catch (Exception ex)
                    { Log.Write(ex); }

                    nRet = WaferUnloading_CheckStage(true);
                    if (nRet != 0) { MarkUnloadStepOnFailure(UnloadFlowStep.Step01); return nRet; }
                    if (IsStop) { MarkUnloadStepOnFailure(UnloadFlowStep.Step01); return 0; }
                    AdvanceUnloadStepOnSuccess(UnloadFlowStep.Step01);
                    goto case UnloadFlowStep.Step02;

                case UnloadFlowStep.Step02:
                    nRet = WaferUnloading_StageToFeeder(true);
                    if (nRet != 0) { MarkUnloadStepOnFailure(UnloadFlowStep.Step02); return nRet; }
                    if (IsStop) { MarkUnloadStepOnFailure(UnloadFlowStep.Step02); return 0; }

                    if (_unloadTargetSlot < 0)
                        _unloadTargetSlot = ComputeUnloadTargetSlot();

                    if (_unloadTargetSlot < 0)
                    {
                        AxisInputFeederY.EmgStop();
                        PostAlarm((int)AlarmKeys.Alarm_UnloadTargetSlotInvalid);
                        this.State = ProcessState.Error;
                        Log.Write(UnitName, "OnRunWork", "Unload target slot invalid");
                        return -1;
                    }

                    AdvanceUnloadStepOnSuccess(UnloadFlowStep.Step02);
                    goto case UnloadFlowStep.Step03;

                case UnloadFlowStep.Step03:
                    // 카세트 슬롯으로 이동 후 Feeder -> Cassette 배출
                    nRet = InputCassetteLifterUnit.MoveToSlot(_unloadTargetSlot, true);
                    if (nRet != 0)
                    {
                        AxisInputFeederY.EmgStop();
                        PostAlarm((int)AlarmKeys.Alarm_CassetteMoveToSlotFailedForUnload);
                        this.State = ProcessState.Error;
                        MarkUnloadStepOnFailure(UnloadFlowStep.Step03);
                        return nRet;
                    }

                    nRet = WaferUnloading_FeederToCassette(true);
                    if (nRet != 0) { MarkUnloadStepOnFailure(UnloadFlowStep.Step03); return nRet; }
                    if (IsStop) { MarkUnloadStepOnFailure(UnloadFlowStep.Step03); return 0; }
                    AdvanceUnloadStepOnSuccess(UnloadFlowStep.Step03);

                    try
                    {
                        var ctx = Equipment.Instance.SummaryContext;
                        ctx.GetCurrentSummaryOrNull()?.StopUnload();
                    }
                    catch (Exception ex)
                    { Log.Write(ex); }
                    break;

                case UnloadFlowStep.Completed:
                default:
                    break;
            }

            return nRet;
        }

        public int RunLoadWaferFlowStep(out bool didLoad, bool isFine)
        {
            int nRet = 0;
            didLoad = false;

            switch (_loadStep)
            {
                case LoadFlowStep.Step01:

                    try
                    {
                        // machineName은 일단 고정 (추후 EquipmentConfig에서 가져오도록 개선 가능)
                        var ctx = Equipment.Instance.SummaryContext;
                        ctx.Begin("--", "--", machineName: "VA1VPRO16");

                        ctx.GetCurrentSummaryOrNull()?.StartLoad();
                    }
                    catch (Exception ex)
                    { Log.Write(ex); }

                    didLoad = true;
                    // Scan은 여기서만 1회 수행 (중복 제거)
                    if (_nextDoScanAndLoad)
                    {
                        nRet = InputCassetteLifterUnit.ScanWafer();
                        if (nRet != 0)
                        {
                            MarkLoadStepOnFailure(LoadFlowStep.Step01);
                            return nRet;
                        }
                        if (IsStop) { MarkLoadStepOnFailure(LoadFlowStep.Step01); return 0; }
                    }

                    // 스캔 후에도 진행 wafer 없으면 Load 종료
                    if (InputCassetteLifterUnit?.IsHaveMoreProcessWafer() != true)
                    {
                        _loadStep = LoadFlowStep.Completed;
                        break;
                    }

                    nRet = WaferLoading_Prepare(true);
                    if (nRet != 0) { MarkLoadStepOnFailure(LoadFlowStep.Step01); return nRet; }
                    if (IsStop) { MarkLoadStepOnFailure(LoadFlowStep.Step01); return 0; }
                    AdvanceLoadStepOnSuccess(LoadFlowStep.Step01);
                    goto case LoadFlowStep.Step02;

                case LoadFlowStep.Step02:
                    didLoad = true;
                    nRet = WaferLoading_CassetteToFeeder(true);
                    if (nRet != 0) { MarkLoadStepOnFailure(LoadFlowStep.Step02); return nRet; }
                    if (IsStop) { MarkLoadStepOnFailure(LoadFlowStep.Step02); return 0; }
                    AdvanceLoadStepOnSuccess(LoadFlowStep.Step02);
                    goto case LoadFlowStep.Step03;

                case LoadFlowStep.Step03:
                    didLoad = true;
                    nRet = WaferLoading_FeederToStage(true);
                    if (nRet != 0) { MarkLoadStepOnFailure(LoadFlowStep.Step03); return nRet; }
                    if (IsStop) { MarkLoadStepOnFailure(LoadFlowStep.Step03); return 0; }
                    AdvanceLoadStepOnSuccess(LoadFlowStep.Step03);

                    goto case LoadFlowStep.Step04;

                case LoadFlowStep.Step04:
                    didLoad = true;
                    nRet = WaferLoading_StageAlignMap(true);
                    if (nRet != 0) { MarkLoadStepOnFailure(LoadFlowStep.Step04); return nRet; }
                    if (IsStop) { MarkLoadStepOnFailure(LoadFlowStep.Step04); return 0; }
                    AdvanceLoadStepOnSuccess(LoadFlowStep.Step04);
                    break;

                case LoadFlowStep.Completed:
                default:
                    break;
            }

            _loadStep = LoadFlowStep.Completed;
            return nRet;
        }


        #region Lifecycle
        public override int OnRun()
        {
            int ret = 0;
            if (this.RunUnitStatus == UnitStatus.Stopped ||
                this.RunUnitStatus == UnitStatus.Stopping ||
                this.RunUnitStatus == UnitStatus.Error ||
                this.RunUnitStatus == UnitStatus.ManualRunning)
            {
                this.State = ProcessState.Stop;
                return 0; // 에러로 보내지 않음
            }

            if (this.RunUnitStatus == UnitStatus.CycleStop)
            {
                this.State = ProcessState.Ready; // 안전 대기
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
            
            // 1) 안전/일관성 체크: 현재 Stage/Feeder 물리센서와 데이터 객체 일치 확인(필요 시 알람)
            try
            {
                MaterialWafer waferStage = this.InputStageUnit?.GetMaterialWafer();
                // Stage Wafer 작업 중이면 true임.
                if (this.InputStageUnit.IsWorking())
                {
                    _nextDoScanAndLoad = false;
                    _nextStandbyAtBarcode = false;
                    NeedUnloadFirst = false;

                    if (waferStage != null
                        && waferStage.ProcessSatate == Material.MaterialProcessSatate.Ready)
                    {
                        // 센터 포지션 미완료 → Stage Load Complete + Align/Mapping 준비
                        if (InputStageUnit.IsWaferCenterPosition() == false)
                        {
                            nRet = InputStageUnit.LoadingWaferComplete();
                            if (nRet != 0)
                            {
                                AxisInputFeederY.EmgStop();
                                PostAlarm((int)AlarmKeys.Alarm_StageLoadingFailed);
                                Log.Write(this, "OnRunWork Fail - LoadingWaferComplete");
                                return nRet;
                            }

                            nRet = PreparetoInputStage();
                            if (nRet != 0)
                            {
                                AxisInputFeederY.EmgStop();
                                PostAlarm((int)AlarmKeys.Alarm_StageLoadingFailed);
                                Log.Write(this, "OnRunWork Fail - LoadingWaferComplete");
                                return nRet;
                            }
                            // 이미 Stage가 작업 중이면 이후 Work에서는 특별히 할 게 없으므로 Complete로 넘길 수도 있으나,
                            // 기존 흐름 유지: Ready→Work로 전환
                        }
                        else if (InputStageUnit.IsWaferCenterPosition()) // Align/Mapping 준비
                        {
                            nRet = PreparetoInputStage();
                            if (nRet != 0)
                            {
                                AxisInputFeederY.EmgStop();
                                PostAlarm((int)AlarmKeys.Alarm_StageLoadingFailed);
                                Log.Write(this, "OnRunWork Fail - LoadingWaferComplete");
                                return nRet;
                            }
                        }
                    }
                    else if (waferStage != null
                        && waferStage.ProcessSatate == Material.MaterialProcessSatate.Processing)
                    {
                        bool chipMappingDone = false;
                        chipMappingDone = this.InputStageUnit.ChipMappingDone;
                        if(chipMappingDone == false)
                        {
                            nRet = PreparetoInputStage();
                            if (nRet != 0)
                            {
                                AxisInputFeederY.EmgStop();
                                PostAlarm((int)AlarmKeys.Alarm_StageLoadingFailed);
                                Log.Write(this, "OnRunWork Fail - LoadingWaferComplete");
                                return nRet;
                            }
                        }
                    }

                    /// Scan/Load 계획 없이 Work로 전환
                    _nextDoScanAndLoad = false;
                    _nextStandbyAtBarcode = false;
                    // Stage Working 분기는 Ready에서 할 거 다 했으므로 Complete로 종료
                    this.State = ProcessState.Complete;
                    return 0;
                }

                // =========================
                // 2) Stage가 Working이 아닌 경우: Plan만 세우고 Work로 넘김
                // =========================
                var equipment = Equipment.Instance;
                bool IsDryRunEqp = equipment.EquipmentConfig.IsDryRun;
                bool sim = (Config.IsSimulation || (Config.IsDryRun || IsDryRunEqp));

                // 완료된 웨이퍼가 Stage에 있으면 언로딩 우선
                NeedUnloadFirst =
                    waferStage != null
                    && waferStage.SlotIndex != -1
                    && waferStage.ProcessSatate == Material.MaterialProcessSatate.Completed
                    && (sim || InputStageUnit.IsRingPresent());

                if (NeedUnloadFirst) // 언로드 우선
                {
                    _nextDoScanAndLoad = false;
                    _nextStandbyAtBarcode = true;

                    _unloadStep = DetermineNextUnloadStep();
                    Log.Write(UnitName, "OnRunReady", "Plan: UnloadFirst=TRUE");
                }
                else
                {
                    bool cassettePresent = false;
                    bool scanned = false;
                    bool hasMore = false;

                    try { cassettePresent = InputCassetteLifterUnit.IsAnyCassettePresent(); } catch { cassettePresent = false; }
                    try { scanned = InputCassetteLifterUnit.IsScanCompleted(); } catch { scanned = false; }
                    try { hasMore = InputCassetteLifterUnit.IsHaveMoreProcessWafer(); } catch { hasMore = false; }

                    bool needScan = !scanned;
                    _nextDoScanAndLoad = cassettePresent && (needScan || hasMore);
                    _nextStandbyAtBarcode = false;

                    _loadStep = DetermineNextLoadStep();
                    Log.Write(UnitName, "OnRunReady", $"Plan: UnloadFirst=FALSE, DoScanAndLoad={_nextDoScanAndLoad}");
                }

                this.State = ProcessState.Work;
                return 0;

            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return nRet;
            }
        }
        protected override int OnRunWork()
        {
            int nRet = 0;

            // 1) Unload 우선 플로우
            if (NeedUnloadFirst)
            {
                if (_unloadStep == UnloadFlowStep.None || _unloadStep == UnloadFlowStep.Completed)
                {
                    _unloadStep = DetermineNextUnloadStep();
                }

                if (!IsSafeToStartInputWaferUnloading())
                {
                    // 아직 Output 작업이 끝나지 않았으므로 "대기" 성격으로 1 반환(알람 아님)
                    // 호출부 Step 로직이 0/!=0만 본다면, 1을 리턴하고 다음 사이클에 재시도되게 구성하는 방식 권장
                    //Log.Write(UnitName, "CheckStageWaferBeforeUnload",
                    //    "Blocked: OutputStage not completed or buffers not empty.");
                    return 0;
                }

                nRet = RunUnloadWaferFlowStep(true);

                if (_unloadStep == UnloadFlowStep.Completed)
                {
                    _unloadTargetSlot = -1;
                    NeedUnloadFirst = false;
                    this.State = ProcessState.Complete;
                }

                return nRet;
            }

            // ===== Load 플로우 (항상 FSM로 진입) =====
            bool didLoad = false;
            if (_loadStep == LoadFlowStep.None || _loadStep == LoadFlowStep.Completed)
            {
                _loadStep = DetermineNextLoadStep();
            }

            nRet = RunLoadWaferFlowStep(out didLoad, true);

            if ((nRet != 0)
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
            if (IsPositionReady() == false)
            {
                if(InputStageUnit.IsPositionWaferLoading() == false
                   && InputStageUnit.IsPositionWaferUnloading() == false)
                {
                    AxisInputFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_StageLoadingFailed);
                    Log.Write(UnitName, "OnRunWork", "Fail - IsPositionWaferLoading() == false");
                    return nRet;
                }

                // wafer 가지고 있는지 유/무 판단 필요.
                bool hasWafer = HasWaferOnFeeder();
                // Ready로 복귀할 때, 현재 로직은 "Unclamp 상태"를 요구함.
                // 그런데 웨이퍼를 잡고 있는 상태(클램프 가능성)라면 Ready 복귀 자체가 위험/불가할 수 있음.
                if (hasWafer == true)
                {
                    // 1) 클램프가 닫혀 있으면(=웨이퍼를 잡고 있을 가능성) Ready 이동 금지
                    //    -> 여기서 무조건 Unclamp 해버리면 웨이퍼 낙하 가능하니, 정책을 확실히 해야 함.
                    //    안전 우선: 알람/에러로 보내고 사용자가 "언로드" 버튼을 누르게 유도.
                    if (IsSafeToMoveReady() == false)
                    {
                        AxisInputFeederY.EmgStop();
                        PostAlarm((int)AlarmKeys.Alarm_InputFeederInterlockFailed);
                        Log.Write(UnitName, "OnRunWork",
                            "Blocked MoveToReady: Feeder has wafer and is not unclamped. Run unload flow first.");
                        return -1;
                    }

                    // 2) (선택) 웨이퍼는 있는데 Unclamp 상태로 판단되는 경우(데이터/센서 불일치 가능)
                    //    -> 이 경우도 위험하므로 막는 방향 권장
                    var equipment = Equipment.Instance;
                    bool IsDryRunEqp = equipment.EquipmentConfig.IsDryRun;
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
                    AxisInputFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_WaferLoadingFailed);
                    this.State = ProcessState.Error;
                    Log.Write(UnitName, "OnRunWork", "Fail - MovePositionReady");
                    return nRet;
                }
                nRet = UpFeeder();
                if (nRet != 0)
                {
                    AxisInputFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_WaferLoadingFailed);
                    this.State = ProcessState.Error;
                    Log.Write(UnitName, "OnRunWork", "Fail - UpFeeder");
                    return nRet;
                }
            }

            this.State = ProcessState.Complete;
            Log.Write(UnitName, "OnRunWork", "Standby Ready.");
            return 0;
        }

        protected override int OnRunComplete()
        {
            int ret = 0;
            _loadStep = LoadFlowStep.None;
            _unloadStep = UnloadFlowStep.None;
            _unloadTargetSlot = -1;

            this.State = ProcessState.Ready;
            return ret;
        }

        // Feeder가 wafer를 "잡고 있다/보유 중"인지 판단
        // - 실기: RingPresent 센서 + (클램프 상태/객체) 조합
        // - 시뮬/드라이런: Material 객체 기준이 더 신뢰됨
        private bool HasWaferOnFeeder()
        {
            try
            {
                // 시뮬/드라이런이면 센서보다 Material이 기준
                var equipment = Equipment.Instance;
                bool IsDryRunEqp = equipment.EquipmentConfig.IsDryRun;
                if (Config.IsSimulation || (Config.IsDryRun || IsDryRunEqp))
                {
                    return (GetMaterial() is MaterialWafer);
                }

                bool sensor = false;
                try { sensor = IsRingPresent(); } catch { sensor = false; }

                bool obj = (GetMaterial() is MaterialWafer);

                // 센서가 ON이면 우선 wafer 있다고 판단
                if (sensor) 
                    return true;

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

        //Step 재개에는 strict 판정 사용.
        private bool HasValidWaferOnFeederForTransfer()
        {
            try
            {
                var equipment = Equipment.Instance;
                bool isDryRunEqp = equipment.EquipmentConfig.IsDryRun;
                bool sim = (Config.IsSimulation || Config.IsDryRun || isDryRunEqp);

                var obj = GetMaterial() as MaterialWafer;
                bool sensor = IsRingPresent();

                if (sim)
                    return obj != null;

                // 실기에서는 센서+객체 모두 만족해야 "전달 가능한 정상 wafer"로 판단
                return (obj != null) && sensor;
            }
            catch
            {
                return false;
            }
        }

        // Ready 이동이 가능한 "안전 상태"인지 판단
        // 현재 코드 기준: Ready로 갈 때 UnClamp 상태를 요구하므로 그 조건을 명확히 둠
        private bool IsSafeToMoveReady()
        {
            try
            {
                var equipment = Equipment.Instance;
                bool IsDryRunEqp = equipment.EquipmentConfig.IsDryRun;
                if (Config.IsSimulation || (Config.IsDryRun || IsDryRunEqp))
                {
                    return true;
                }

                // MovePositionReady() 내부가 IsUnClamped()를 강제하므로 여기서도 동일 조건 사용
                return IsUnClamped();
            }
            catch
            {
                return false;
            }
        }



        public int WaferLoading_Prepare(bool isFine = false)
        {
            int nRet = 0;

            nRet = this.InputCassetteLifterUnit.MoveToNextSlot();
            if (nRet != 0)
            {
                AxisInputFeederY.EmgStop();
                Log.Write(this, "OnRunWork Fail - MoveToNextSlot");
                return nRet;
            }
            Log.Write(UnitName, "OnRunWork", "MoveToNextSlot completed.");

            nRet = PrepareLoadingStage();
            if (nRet != 0)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_InputStageInterlockFailed);
                Log.Write(this, "OnRunWork Fail - PrepareLoadingWafer");
                return nRet;
            }
            Log.Write(UnitName, "OnRunWork", "PrepareLoadingStage completed.");

            return nRet;
        }

        public int WaferLoading_CassetteToFeeder(bool isFine = false)
        {
            int nRet = 0;

            nRet = UnloadWaferFromCassette(); // 여기서 Barcode Reading 포함
            if (nRet != 0)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_WaferLoadingFailed);
                Log.Write(this, "OnRunWork Fail - WaferLoading");
                return nRet;
            }
            Log.Write(UnitName, "OnRunWork", "UnloadWaferFromCassette completed.");

            return nRet;
        }

        public int WaferLoading_FeederToStage(bool isFine = false)
        {
            int nRet = 0;

            nRet = StageLoading();
            if (nRet != 0)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageLoadingFailed);
                this.State = ProcessState.Error;
                Log.Write(this, "OnRunWork Fail - StageLoading");
                return nRet;
            }

            var waferOnFeeder = this.GetMaterial() as MaterialWafer;
            if (waferOnFeeder != null)
            {
                // 기존 인스턴스를 Stage로 이동
                this.MoveMaterial(waferOnFeeder, InputStageUnit);
                // 가공 상태 유지/설정
                //waferOnFeeder.ProcessSatate = Material.MaterialProcessSatate.Processing;
                waferOnFeeder.ProcessSatate = Material.MaterialProcessSatate.Ready;
                InputStageUnit.SetMaterial(waferOnFeeder);

                // Feeder의 material 비우기
                this.SetMaterial(null);
            }
            else
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_WaferDataFaild);
                Log.Write(this, "No wafer on Feederto move to InputStage ");
                return nRet;
            }
            Log.Write(UnitName, "OnRunWork", "waferOnFeeder completed.");

            nRet = MoveToReady();
            if (nRet != 0)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageLoadingFailed);
                Log.Write(this, "OnRunWork Fail - MoveToReady");
                return nRet;
            }
            Log.Write(UnitName, "OnRunWork", "MoveToReady completed.");

            if (IsStop) //여기에서 시컨스 돌다가 정지해야함. 조건문으로 처리 하든지.
            {
                Log.Write(UnitName, "OnRunWork", "MoveToReady - IsStop.");
                return 0;
            }

            return nRet;
        }

        public int WaferLoading_StageAlignMap(bool isFine = false)
        {
            int nRet = 0;

            nRet = InputStageUnit.LoadingWaferComplete();
            if (nRet != 0)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageLoadingFailed);
                Log.Write(this, "OnRunWork Fail - LoadingWaferComplete");
                return nRet;
            }
            Log.Write(UnitName, "OnRunWork", "LoadingWaferComplete completed.");

            var WaferOnStage = InputStageUnit.GetMaterialWafer();
            if (WaferOnStage != null)
            {
                try
                {
                    var waferId = WaferOnStage.WaferId;
                    var binId = WaferOnStage.WaferId; // 정책: BINID = WAFERID

                    var ctx = Equipment.Instance.SummaryContext;
                    ctx.GetCurrentSummaryOrNull()?.SetWaferId(waferId);
                    ctx.GetCurrentSummaryOrNull()?.SetBinId(binId);
                    RotaryUnit.ApplyPickersToWaferSummaryIfActive();

                    ctx.GetCurrentSummaryOrNull()?.StopLoad();
                }
                catch (Exception ex)
                { Log.Write(ex); }
            }

            nRet = PreparetoInputStage();
            if (nRet != 0)
            {
                //20251222 - 여기서 알람 발생. 
                // PreparetoInputStage 내부에서 알람 구분해서 
                // 알람 발생 필요.
                // 여기는.. 알람빼고.
                // 근데.. 내부에서 알람 발생하면 장비 전체가 안멈추나?

                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageLoadingFailed);
                Log.Write(this, "OnRunWork Fail - PreparetoInputStage");
                return nRet;
            }
            Log.Write(UnitName, "OnRunWork", "PreparetoInputStage completed.");

            return nRet;
        }

        public int WaferLoading(bool isFine = false)
        {
            int nRet = 0;
            nRet = this.InputCassetteLifterUnit.MoveToNextSlot();
            if (nRet != 0)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_WaferLoadingFailed);
                Log.Write(this, "OnRunWork Fail - MoveToNextSlot");
                return nRet;
            }
            Log.Write(UnitName, "OnRunWork", "MoveToNextSlot completed.");

            nRet = PrepareLoadingStage();
            if (nRet != 0)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_InputStageInterlockFailed);
                Log.Write(this, "OnRunWork Fail - PrepareLoadingWafer");
                return nRet;
            }
            Log.Write(UnitName, "OnRunWork", "PrepareLoadingStage completed.");

            nRet = UnloadWaferFromCassette(); // 여기서 Barcode Reading 포함
            if (nRet != 0)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_WaferLoadingFailed);
                Log.Write(this, "OnRunWork Fail - WaferLoading");
                return nRet;
            }
            Log.Write(UnitName, "OnRunWork", "UnloadWaferFromCassette completed.");

            nRet = StageLoading();
            if (nRet != 0)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageLoadingFailed);
                this.State = ProcessState.Error;
                Log.Write(this, "OnRunWork Fail - StageLoading");
                return nRet;
            }

            var waferOnFeeder = this.GetMaterial() as MaterialWafer;
            if (waferOnFeeder != null)
            {
                // 기존 인스턴스를 Stage로 이동
                this.MoveMaterial(waferOnFeeder, InputStageUnit);
                // 가공 상태 유지/설정
                //waferOnFeeder.ProcessSatate = Material.MaterialProcessSatate.Processing;
                waferOnFeeder.ProcessSatate = Material.MaterialProcessSatate.Ready;
                InputStageUnit.SetMaterial(waferOnFeeder);

                // Feeder의 material 비우기
                this.SetMaterial(null);
            }
            else
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_WaferDataFaild);
                Log.Write(this, "No wafer on Feederto move to InputStage ");
                return nRet;
            }
            Log.Write(UnitName, "OnRunWork", "waferOnFeeder completed.");

            nRet = MoveToReady();
            if (nRet != 0)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageLoadingFailed);
                Log.Write(this, "OnRunWork Fail - MoveToReady");
                return nRet;
            }
            Log.Write(UnitName, "OnRunWork", "MoveToReady completed.");

            if(IsStop) //여기에서 시컨스 돌다가 정지해야함. 조건문으로 처리 하든지.
            {
                Log.Write(UnitName, "OnRunWork", "MoveToReady - IsStop.");
                return 0;
            }

            nRet = InputStageUnit.LoadingWaferComplete();
            if (nRet != 0)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageLoadingFailed);
                Log.Write(this, "OnRunWork Fail - LoadingWaferComplete");
                return nRet;
            }
            Log.Write(UnitName, "OnRunWork", "LoadingWaferComplete completed.");

            nRet = PreparetoInputStage();
            if (nRet != 0)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageLoadingFailed);
                Log.Write(this, "OnRunWork Fail - PreparetoInputStage");
                return nRet;
            }
            Log.Write(UnitName, "OnRunWork", "PreparetoInputStage completed.");
            return nRet;
        }
        #endregion


        #region Manual Sequence (for ManualSequenceControl)

        // ===== Manual Load (Auto 동일 Step) =====
        // [ADD] 통합 로딩 시퀀스 (한 번에 실행)
        public int Manual_Load_Batch(bool isFine = true)
        {
            int nRet = 0;
            bool didLoad = false;

            // [Smart Resume]
            // 만약 Feeder에 이미 웨이퍼가 있다면(이전 로딩 중단 등), 
            // 카세트에서 꺼내는 동작(Step01/02)을 건너뛰고 Feeder->Stage(Step03)부터 시작
            if (HasValidWaferOnFeederForTransfer()) //if (HasWaferOnFeeder())
            {
                Log.Write(UnitName, "Manual_Load_Batch", "Wafer detected on Feeder. Resuming from Step03.");
                _loadStep = LoadFlowStep.Step03;
            }
            else
            {
                // 일반적인 경우 처음부터 시작
                _loadStep = LoadFlowStep.Step01;
                // Manual Batch는 보통 지정된 슬롯 작업을 가정하므로 Scan은 건너뜀 (필요시 true 변경)
                _nextDoScanAndLoad = false;
            }

            // 로직 수행 (RunLoadWaferFlowStep 내부에서 goto case로 연속 실행됨)
            nRet = RunLoadWaferFlowStep(out didLoad, isFine);

            // 완료 상태 정리
            if (_loadStep == LoadFlowStep.Completed)
            {
                // 로딩 성공 후, Auto 모드처럼 Ready 위치 복귀 및 Feeder Up 처리
                if (didLoad)
                {
                    int finishRet = Manual_StandbyReady(isFine);
                    if (finishRet != 0) 
                        return finishRet;
                }
            }
            // 실패했더라도 nRet을 반환하여 상위에서 알람 확인 가능
            return nRet;
        }
        
        // ===== Manual Unload (Auto 동일 Step) =====
        // [ADD] 통합 언로딩 시퀀스 (Die가 있어도 강제 진행)
        public int Manual_Unload_Batch(bool isFine = true)
        {
            // [Smart Resume Logic]
            // 에러 후 재시작 시, Feeder에 웨이퍼가 이미 있다면 Stage에서 가져오는 동작(Step1,2)을 스킵하고
            // 바로 Cassette에 넣는 동작(Step3)으로 점프해야 함.
            if (HasWaferOnFeeder())
            {
                Log.Write(UnitName, "Manual_Unload_Batch", "Wafer detected on Feeder. Resuming from Step03 (To Cassette).");
                _unloadStep = UnloadFlowStep.Step03;
            }
            else
            {
                // Feeder가 비어있으면 정상적으로 Stage 확인부터 시작
                _unloadStep = UnloadFlowStep.Step01;
            }

            // 2. 안전 체크 없이 바로 로직 수행 (RunUnloadWaferFlowStep 내부에서 goto case로 연속 실행)
            int nRet = RunUnloadWaferFlowStep(isFine);

            // 3. 완료 상태 정리
            if (_unloadStep == UnloadFlowStep.Completed)
            {
                _unloadTargetSlot = -1;
                NeedUnloadFirst = false;

                // 언로드 완료 후 안전 위치(Ready)로 복귀까지 수행하고 싶다면 주석 해제
                // Manual_StandbyReady(isFine); 
            }

            return nRet;
        }

        // Auto OnRunWork() 끝에서 Standby Ready 처리만 따로 필요할 때(수동에서 개별 호출 가능)
        public int Manual_StandbyReady(bool isFine = true)
        {
            // Auto OnRunWork()의 마지막 부분을 외부에서 직접 재현하기 위한 호출
            // (IsPositionReady 아니면 MovePositionReady + UpFeeder)
            int nRet = 0;

            if (IsPositionReady() == false)
            {
                if (InputStageUnit != null
                    && InputStageUnit.IsPositionWaferLoading() == false
                    && InputStageUnit.IsPositionWaferUnloading() == false)
                {
                    AxisInputFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_StageLoadingFailed);
                    Log.Write(UnitName, "Manual_StandbyReady", "Fail - InputStage not in Loading/Unloading position");
                    return -1;
                }

                // Auto와 동일: wafer 있는 상태에서 Ready 이동은 Unclamp 요구
                bool hasWafer = HasWaferOnFeeder();
                if (hasWafer && IsSafeToMoveReady() == false)
                {
                    AxisInputFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_InputFeederInterlockFailed);
                    Log.Write(UnitName, "Manual_StandbyReady", "Blocked: Feeder has wafer and not unclamped");
                    return -1;
                }

                nRet = MovePositionReady(isFine);
                if (nRet != 0)
                {
                    AxisInputFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_WaferLoadingFailed);
                    Log.Write(UnitName, "Manual_StandbyReady", "Fail - MovePositionReady");
                    return nRet;
                }

                nRet = UpFeeder();
                if (nRet != 0)
                {
                    AxisInputFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_WaferLoadingFailed);
                    Log.Write(UnitName, "Manual_StandbyReady", "Fail - UpFeeder");
                    return nRet;
                }
            }

            return 0;
        }


        #endregion

        protected override void OnMakeSequence()
        {
            // [CHANGE] ManualSequenceControl에 노출할 Step 기반으로 재구성
            // (기존: PrepareLoadingStage/UnloadWaferFromCassette/StageLoading/MoveToReady/WaferUnloading)
            this.SequencePlayers.Clear();

            this.SequencePlayers.Add(Manual_StandbyReady);                    // 마지막 Ready 복귀만 따로

            // ===== Unload =====
            this.SequencePlayers.Add(Manual_Unload_Batch);

            // ===== Load =====
            this.SequencePlayers.Add(Manual_Load_Batch);
        }


        #region Sequence Auto
        private int PreparetoInputStage()
        {
            int nRet = 0;

            // 6) 정렬/매핑
            nRet = InputStageUnit.AlignT();
            if (nRet != 0)
            {
                // 내부에서 알람 발생.
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_AlignT);
                Log.Write(this, "PreparetoInputStage Fail - AlignT");
                return nRet;
            }

            nRet = InputStageUnit.AlignXY();
            if (nRet != 0)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageLoadingFailed);
                this.State = ProcessState.Error;
                Log.Write(this, "PreparetoInputStage Fail - AlignXY");
                return nRet;
            }

            nRet = InputStageUnit.PerformChipMapping();
            if (nRet != 0)
            {
                AxisInputFeederY.EmgStop();
                // 내부에서 알람 발생하므로 중복 방지
                PostAlarm((int)AlarmKeys.Alarm_StageLoadingFailed);
                Log.Write(this, "PreparetoInputStage Fail - PerformChipMapping");
                return nRet;
            }

            return nRet;
        }

        public int PrepareLoadingStage(bool isFine = false)
        {
            int nRet = 0;

            if (RunMode == UnitRunMode.Manual)
            {
                CurrentFunc = PrepareLoadingStage;
            }

            nRet = InputStageUnit.PrepareLoadingStage();
            if (nRet != 0)
            {
                Log.Write(UnitName, "PrepareLoadingStage Fail - InputStage.PrepareLoadingStage()");
                return -1;
            }
            return nRet;
        }

        public int UnloadWaferFromCassette(bool isFine = false)
        {
            int nRet = 0;

            if (RunMode == UnitRunMode.Manual)
            {
                CurrentFunc = UnloadWaferFromCassette;
            }

            Log.Write(this, "WaferLoading Start");
            if (IsMoveInterLockCassetteOk() == false)
            {
                Log.Write(UnitName, "WaferLoading", "WaferLoading Fail - IsMoveInterLockCassette");
                return -1;
            }

            // 교차(Exchange) 대기 처리: 언로드 직후이면 바코드에서 바로 시작
            bool preferBarcode = _exchangeStandbyForNextLoad || IsPositionBarcode();
            if (preferBarcode)
            {
                if (IsPositionBarcode() == false)
                {
                    nRet = MovePositionBarcode(isFine);
                    if (nRet != 0)
                    {
                        Log.Write(UnitName, "WaferLoading", "WaferLoading Fail - MovePositionBarcode");
                        return nRet;
                    }
                }
                Log.Write(UnitName, "WaferLoading", "[Exchange] Standby at Barcode → skip MoveToReady");
                _exchangeStandbyForNextLoad = false; // 1회 사용
            }
            else
            {
                // 이미 Ready면 스킵
                if (IsPositionReady() == false)
                {
                    nRet = MoveToReady(isFine);
                    if (nRet != 0)
                    {
                        Log.Write(UnitName, "WaferLoading", "WaferLoading Fail - MoveToReay");
                        return nRet;
                    }
                }
                else
                {
                    Log.Write(UnitName, "WaferLoading", "[Skip] Already at Ready");
                }
            }

            nRet = UnClampGripper();
            if (nRet != 0)
            {
                Log.Write(UnitName, "WaferLoading", "WaferLoading Fail - UnClampGripper");
                return nRet;
            }

            nRet = DownFeeder();
            if (nRet != 0)
            {
                Log.Write(UnitName, "WaferLoading", "WaferLoading Fail - DownFeeder");
                return nRet;
            }

            nRet = MoveToCassette(isFine);
            if (nRet != 0)
            {
                Log.Write(UnitName, "WaferLoading", "WaferLoading Fail - MoveToCassette");
                return nRet;
            }

            // =======================
            // 바코드 읽기 (트리거/비트리거 분기)
            // =======================
            string strBarcode = string.Empty;
            try
            {
                nRet = BarcodeReading(isFine);
                if (nRet != 0)
                {
                    Log.Write(UnitName, "WaferLoading", "WaferLoading Fail - BarcodeReading");
                    return nRet;
                }

                bool useTrigger = false;
                var equipment = Equipment.Instance;
                bool IsDryRunEqp = equipment.EquipmentConfig.IsDryRun;
                if ((Config.IsSimulation == false && (Config.IsDryRun == false && IsDryRunEqp == false))
                   && (InputCassetteLifterUnit?.IsTriggerModeConfigured() == true))
                {
                    useTrigger = true;
                }

                if(InputCassetteLifterUnit.Config.UseBarcode)
                {
                    if (useTrigger)
                    {
                        // 트리거 모드: 자동 트리거 켜고 이벤트 큐에서 수신 대기
                        int tOn = InputCassetteLifterUnit.EnsureTriggerOn();
                        if (tOn != 0)
                        {
                            Log.Write(UnitName, "WaferLoading", "Auto-Trigger On Failed → fallback to polling");
                            useTrigger = false; // 폴링으로 폴백
                        }
                        else
                        {
                            InputCassetteLifterUnit.ClearBarcodeBuffer();
                            // 기준 위치에서 1차 대기
                            if (InputCassetteLifterUnit.WaitBarcode(out strBarcode, timeoutMs: 500) == 0 && !string.IsNullOrEmpty(strBarcode))
                            {
                                // OK
                            }
                            else
                            {
                                // 스캔 파라미터
                                const double scanStep = 1.0;  // mm
                                const int scanPairs = 5;      // ±1~±6 step
                                const int settleMs = 50;
                                double basePosY = 0;

                                try
                                {
                                    basePosY = this.GetTP(InputFeederConfig.TeachingPositionName.Barcode.ToString(), this.AxisInputFeederY.Name);
                                }
                                catch
                                {
                                    basePosY = AxisInputFeederY.GetPosition();
                                }

                                for (int i = 1; i <= scanPairs; i++)
                                {
                                    // +오프셋
                                    double targetPlus = basePosY + (scanStep * i);
                                    if (IsMoveInterLockBarcode() != 0)
                                    {
                                        AxisInputFeederY.EmgStop();
                                        PostAlarm((int)AlarmKeys.Alarm_InputStageInterlockFailed);
                                        Log.Write(UnitName, "WaferLoading", "Barcode scan interlock fail (+offset)");
                                        return -1;
                                    }
                                    nRet = MoveAxisPositionOne(AxisInputFeederY, targetPlus, isFine);
                                    if (nRet != 0)
                                    {
                                        AxisInputFeederY.EmgStop();
                                        Log.Write(UnitName, "WaferLoading", "Move Y +offset fail during barcode scan (trigger)");
                                        return -1;
                                    }
                                    Thread.Sleep(settleMs);

                                    if (InputCassetteLifterUnit.WaitBarcode(out strBarcode, timeoutMs: 500) == 0 && !string.IsNullOrEmpty(strBarcode))
                                        break;

                                    // -오프셋
                                    double targetMinus = basePosY - (scanStep * i);
                                    if (IsMoveInterLockBarcode() != 0)
                                    {
                                        AxisInputFeederY.EmgStop();
                                        PostAlarm((int)AlarmKeys.Alarm_InputStageInterlockFailed);
                                        Log.Write(UnitName, "WaferLoading", "Barcode scan interlock fail (-offset)");
                                        return -1;
                                    }
                                    nRet = MoveAxisPositionOne(AxisInputFeederY, targetMinus, isFine);
                                    if (nRet != 0)
                                    {
                                        AxisInputFeederY.EmgStop();
                                        Log.Write(UnitName, "WaferLoading", "Move Y -offset fail during barcode scan (trigger)");
                                        return -1;
                                    }
                                    Thread.Sleep(settleMs);

                                    if (InputCassetteLifterUnit.WaitBarcode(out strBarcode, timeoutMs: 500) == 0 && !string.IsNullOrEmpty(strBarcode))
                                        break;
                                }

                                // 스캔 종료 후 기준 위치 복귀.
                                try
                                {
                                    nRet = BarcodeReading(isFine);
                                    if (nRet != 0)
                                    {
                                        Log.Write(UnitName, "WaferLoading", "WaferLoading Fail - BarcodeReading (return to base)");
                                        return nRet;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Log.Write(ex);
                                }
                            }

                            if (string.IsNullOrEmpty(strBarcode))
                            {
                                //Barcode 실패 시 Ready.
                                //if (IsPositionBarcode())
                                {
                                    if (InputCassetteLifterUnit.IsAnyAxisMoving())
                                    {
                                        InputCassetteLifterUnit.WaferLifterZ.EmgStop();
                                    }
                                    if (IsFeederDown())
                                    {
                                        if (IsRingPresent())
                                        {
                                            // 잡고 있던 못잡고 있던 일단 cassette 위치로 이동해서 내려놓고 시작하자.
                                            if (IsUnClamped() == false || IsUnClamped() == true)
                                            {
                                                nRet = MovePositionCassette();
                                                if (nRet != 0)
                                                {
                                                    AxisInputFeederY.EmgStop();
                                                    PostAlarm((int)AlarmKeys.Alarm_InputStageInterlockFailed);
                                                    Log.Write(UnitName, "CheckReady Fail - InputStage.IsStageInterLockOK");
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
                                                    AxisInputFeederY.EmgStop();
                                                    PostAlarm((int)AlarmKeys.Alarm_InputStageInterlockFailed);
                                                    Log.Write(UnitName, "CheckReady Fail - InputStage.IsStageInterLockOK");
                                                    return -1;
                                                }
                                                nRet = UpFeeder();
                                                if (nRet != 0)
                                                {
                                                    AxisInputFeederY.EmgStop();
                                                    PostAlarm((int)AlarmKeys.Alarm_InputStageInterlockFailed);
                                                    Log.Write(UnitName, "CheckReady Fail - InputStage.IsStageInterLockOK");
                                                    return -1;
                                                }
                                            }
                                            else
                                            {
                                                AxisInputFeederY.EmgStop();
                                                PostAlarm((int)AlarmKeys.Alarm_InputStageInterlockFailed);
                                                Log.Write(UnitName, "CheckReady Fail - InputStage.IsStageInterLockOK");
                                                return -1;
                                            }
                                        }
                                        else
                                        {

                                            AxisInputFeederY.EmgStop();
                                            PostAlarm((int)AlarmKeys.Alarm_InputStageInterlockFailed);
                                            Log.Write(UnitName, "CheckReady Fail - InputStage.IsStageInterLockOK");
                                            return -1;
                                        }
                                    }
                                    else
                                    {
                                        AxisInputFeederY.EmgStop();
                                        PostAlarm((int)AlarmKeys.Alarm_InputStageInterlockFailed);
                                        Log.Write(UnitName, "CheckReady Fail - InputStage.IsStageInterLockOK");
                                        return -1;
                                    }
                                }

                                AxisInputFeederY.EmgStop();
                                PostAlarm((int)AlarmKeys.Alarm_BarcodeReadingFailed);
                                Log.Write(UnitName, "WaferLoading", "Barcode Reading Failed after trigger scan");
                                return -1;
                            }
                        }
                    }
                    else if (useTrigger == false)
                    {
                        nRet = GetBarcode(out strBarcode);
                        // 실패 시: 바코드 위치 기준 Y축 소폭 스캔(앞/뒤 왕복) 재시도
                        if (nRet != 0 || string.IsNullOrEmpty(strBarcode))
                        {
                            // 스캔 파라미터
                            const double scanStep = 1;      // mm 단위 스텝
                            const int scanPairs = 6;       // ±1~±6 step 
                            const int settleMs = 50;       // 이동 후 안정화 대기

                            // 바코드 기준 위치
                            double basePosY = 0;
                            try
                            {
                                basePosY = this.GetTP(InputFeederConfig.TeachingPositionName.Barcode.ToString(), this.AxisInputFeederY.Name);
                            }
                            catch (Exception ex)
                            {
                                basePosY = AxisInputFeederY.GetPosition();
                                Log.Write(ex);
                            }

                            // 먼저 기준 위치에서 한 번 더 시도(리더 타이밍 보정 목적)
                            if (nRet != 0 || string.IsNullOrEmpty(strBarcode))
                            {
                                Thread.Sleep(settleMs);
                                nRet = GetBarcode(out strBarcode);
                            }

                            // 왕복 스캔 루프
                            if (nRet != 0 || string.IsNullOrEmpty(strBarcode))
                            {
                                for (int i = 1; i <= scanPairs; i++)
                                {
                                    //아래는 필요시 멈추자.
                                    //if (IsStop)
                                    //{
                                    //    Log.Write(UnitName, "WaferLoading", "Barcode scan stopped by IsStop");
                                    //    return 0;
                                    //}

                                    // +오프셋
                                    double targetPlus = basePosY + (scanStep * i);
                                    // 이동 전 간단 인터락 체크
                                    if (IsMoveInterLockBarcode() != 0)
                                    {
                                        AxisInputFeederY.EmgStop();
                                        PostAlarm((int)AlarmKeys.Alarm_InputStageInterlockFailed);
                                        Log.Write(UnitName, "WaferLoading", "Barcode scan interlock fail (+offset)");
                                        return -1;
                                    }
                                    nRet = MoveAxisPositionOne(AxisInputFeederY, targetPlus, isFine);
                                    if (nRet != 0)
                                    {
                                        //MoveAxisPositionOne 내부에서 알람 발생.
                                        AxisInputFeederY.EmgStop();
                                        Log.Write(UnitName, "WaferLoading", "Move Y +offset fail during barcode scan");
                                        return -1;
                                    }
                                    Thread.Sleep(settleMs);

                                    nRet = GetBarcode(out strBarcode);
                                    if (nRet == 0 && !string.IsNullOrEmpty(strBarcode))
                                        break;

                                    // -오프셋
                                    double targetMinus = basePosY - (scanStep * i);
                                    if (IsMoveInterLockBarcode() != 0)
                                    {
                                        AxisInputFeederY.EmgStop();
                                        PostAlarm((int)AlarmKeys.Alarm_InputStageInterlockFailed);
                                        Log.Write(UnitName, "WaferLoading", "Barcode scan interlock fail (-offset)");
                                        return -1;
                                    }
                                    nRet = MoveAxisPositionOne(AxisInputFeederY, targetMinus, isFine);
                                    if (nRet != 0)
                                    {
                                        AxisInputFeederY.EmgStop();
                                        Log.Write(UnitName, "WaferLoading", "Move Y -offset fail during barcode scan");
                                        return -1;
                                    }
                                    Thread.Sleep(settleMs);

                                    nRet = GetBarcode(out strBarcode);
                                    if (nRet == 0 && !string.IsNullOrEmpty(strBarcode))
                                    {
                                        break;
                                    }
                                }

                                try
                                {
                                    // 스캔 종료 후 바코드 기준 위치로 복귀(일관성 유지)
                                    nRet = BarcodeReading(isFine);
                                    if (nRet != 0)
                                    {
                                        Log.Write(UnitName, "WaferLoading", "WaferLoading Fail - BarcodeReading");
                                        return nRet;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Log.Write(ex);
                                }
                            }

                            // 최종 실패 처리: 알람 발생
                            if (nRet != 0 || string.IsNullOrEmpty(strBarcode))
                            {
                                //Barcode 실패 시 Ready.
                                //if (IsPositionBarcode())
                                {
                                    if (InputCassetteLifterUnit.IsAnyAxisMoving())
                                    {
                                        InputCassetteLifterUnit.WaferLifterZ.EmgStop();
                                    }
                                    if (IsFeederDown())
                                    {
                                        if (IsRingPresent())
                                        {
                                            // 잡고 있던 못잡고 있던 일단 cassette 위치로 이동해서 내려놓고 시작하자.
                                            if (IsUnClamped() == false || IsUnClamped() == true)
                                            {
                                                nRet = MovePositionCassette();
                                                if (nRet != 0)
                                                {
                                                    AxisInputFeederY.EmgStop();
                                                    PostAlarm((int)AlarmKeys.Alarm_InputStageInterlockFailed);
                                                    Log.Write(UnitName, "CheckReady Fail - InputStage.IsStageInterLockOK");
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
                                                    AxisInputFeederY.EmgStop();
                                                    PostAlarm((int)AlarmKeys.Alarm_InputStageInterlockFailed);
                                                    Log.Write(UnitName, "CheckReady Fail - InputStage.IsStageInterLockOK");
                                                    return -1;
                                                }
                                            }
                                            else
                                            {
                                                AxisInputFeederY.EmgStop();
                                                PostAlarm((int)AlarmKeys.Alarm_InputStageInterlockFailed);
                                                Log.Write(UnitName, "CheckReady Fail - InputStage.IsStageInterLockOK");
                                                return -1;
                                            }
                                        }
                                        else
                                        {

                                            AxisInputFeederY.EmgStop();
                                            PostAlarm((int)AlarmKeys.Alarm_InputStageInterlockFailed);
                                            Log.Write(UnitName, "CheckReady Fail - InputStage.IsStageInterLockOK");
                                            return -1;
                                        }
                                    }
                                    else
                                    {
                                        AxisInputFeederY.EmgStop();
                                        PostAlarm((int)AlarmKeys.Alarm_InputStageInterlockFailed);
                                        Log.Write(UnitName, "CheckReady Fail - InputStage.IsStageInterLockOK");
                                        return -1;
                                    }
                                }

                                AxisInputFeederY.EmgStop();
                                PostAlarm((int)AlarmKeys.Alarm_BarcodeReadingFailed);
                                Log.Write(UnitName, "WaferLoading", "Barcode Reading Failed after scanning");
                                return -1;
                            }
                        }
                    }
                }
                else
                {
                    nRet = GetBarcode(out strBarcode);
                    if (nRet != 0 || string.IsNullOrEmpty(strBarcode))
                    {
                        AxisInputFeederY.EmgStop();
                        PostAlarm((int)AlarmKeys.Alarm_BarcodeReadingFailed);
                        Log.Write(UnitName, "WaferLoading", "Barcode Reading Failed after scanning");
                        return -1;
                    }

                    if (Config.IsSimulation == false && (Config.IsDryRun == false && IsDryRunEqp == false))
                    {
                        // [CHANGE] 바코드 미사용 시: 수동 Wafer ID 입력
                        // Todo: 입력창 띄워져 있는 상태에서 장비 정비 시 자동으로 입력창 닫고 장비 정지하자.
                        string waferId;
                        bool ok = FormInputWaferID.TryGetWaferId(owner: null, initialValue: string.Empty, out waferId);
                        if (!ok || string.IsNullOrWhiteSpace(waferId))
                        {
                            strBarcode = strBarcode.Trim();
                            //AxisInputFeederY.EmgStop();
                            //PostAlarm((int)AlarmKeys.Alarm_BarcodeReadingFailed);
                            //Log.Write(UnitName, "WaferLoading", "Wafer ID input canceled/empty");
                            //return -1;
                        }
                        else
                        {
                            strBarcode = waferId.Trim();
                        }
                    }
                }

                var c = this.InputCassetteLifterUnit.GetMaterialCassette();
                int nIndex = this.InputCassetteLifterUnit.GetCurrectSlotID();
                MaterialWafer wafer = c.GetWafer(nIndex);
                // 캐리어 정보만 보전하고, 상태는 Ready 유지 (Processing으로 올리지 않음)
                wafer.CarrierId = c.CarrierId;
                wafer.WaferId = strBarcode;
                this.SetMaterial(wafer);

                RaiseWaferIdChanged(wafer.WaferId);
                Log.Write(UnitName, "WaferLoading", strBarcode);
                Log.Write(UnitName, "WaferLoading", "WaferLoading Complete");
                return nRet;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            finally
            {
                InputCassetteLifterUnit.EnsureTriggerOff();
            }

            return nRet;
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

        public int StageLoading(bool isFine = false)
        {
            int nRet = 0;

            if (RunMode == UnitRunMode.Manual)
            {
                CurrentFunc = StageLoading;
            }

            Log.Write(this, "StageLoading Start");
            if (IsMoveInterLockCassetteOk() == false)
            {
                Log.Write(this, "StageLoading Fail - IsMoveInterLockCassette");
                return -1;
            }

            nRet = MovePositionStage(isFine);
            if (nRet != 0)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageLoadingFailed);
                Log.Write(this, "StageLoading Fail - MovePositionStage");
                nRet = -1;
                return nRet;
            }

            nRet = UnClampGripper();
            if (nRet != 0)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageLoadingFailed);
                Log.Write(this, "StageLoading Fail - UnClampGripper");
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

            Log.Write(this, "MoveToReady Start");
            if (IsMoveInterLockCassetteOk() == false)
            {
                return -1;
            }

            nRet = MovePositionReady(isFine);
            if (nRet != 0)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_WaferLoadingFailed);
                nRet = -1;
                return nRet;
            }
            //if (IsStop) { return 0; }

            nRet = UpFeeder();
            if (nRet != 0)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_WaferLoadingFailed);
                nRet = -1;
                return nRet;
            }
            Log.Write(this, "MoveToReay End");
            return nRet;
        }

        public int WaferUnloading_CheckStage(bool isFine = false)
        {
            int nRet = 0;

            if (IsStop) 
                return 0;

            Log.Write(UnitName, "WaferUnloading", "Start");
            MaterialWafer WaferData = this.InputStageUnit.GetMaterialWafer();

            int chk = CheckStageWaferBeforeUnload(WaferData);
            if (chk == -2)
            {
                NeedUnloadFirst = false;
                this.State = ProcessState.Complete;
                Log.Write(UnitName, "CheckStageWaferBeforeUnload", "Complete");
                return 0;
            }
            if (chk != 0)
            {
                AxisInputFeederY.EmgStop();
                Log.Write(UnitName, "CheckStageWaferBeforeUnload", "Failed");
                return -1;
            }
            return nRet;
        }

        public int WaferUnloading_StageToFeeder(bool isFine = false)
        {
            int nRet = 0;
            MaterialWafer WaferData = this.InputStageUnit.GetMaterialWafer();
             
            nRet = WaferUnloadingStage(WaferData);
            if (nRet != 0 && nRet != -2)
            {
                AxisInputFeederY.EmgStop();
                // WaferUnloadingStage 내부에서 상세 알람을 올리므로 여기서 2004로 덮지 않음
                return nRet;
            }
            if (nRet == -2)
            {
                Log.Write(UnitName, "WaferUnloading", "IsStop-WaferUnloadingStage -2");
                return 0;
            }

            if (VerifyWaferMovedStageToFeeder(WaferData) != 0)
            {
                AxisInputFeederY.EmgStop();
                // Verify 내부에서 상세 알람(2040/2028/2042/2050)을 올리므로 2004로 덮지 않음
                Log.Write(UnitName, "VerifyWaferMovedStageToFeeder", "Failed");
                return -1;
            }

            return nRet;
        }

        public int WaferUnloading_FeederToCassette(bool isFine = false)
        {
            int nRet = 0;
            MaterialWafer WaferData = this.InputStageUnit.GetMaterialWafer();

            nRet = WaferUnloadingFeeder(WaferData);
            if (nRet != 0)
            {
                if (nRet == -2)
                    return 0;

                AxisInputFeederY.EmgStop();
                // WaferUnloadingFeeder 내부에서 상세 알람을 올리므로 2004로 덮지 않음
                Log.Write(UnitName, "WaferUnloadingFeeder", "Failed");
            }

            _exchangeStandbyForNextLoad = _nextStandbyAtBarcode;
            return nRet;
        }


        public int WaferUnloading(bool isFine = false)
        {
            int nRet = 0;
            if (RunMode == UnitRunMode.Manual)
            {
                CurrentFunc = WaferUnloading;
            }

            Log.Write(UnitName, "WaferUnloading", "Start");
            MaterialWafer WaferData = this.InputStageUnit.GetMaterialWafer();
            bool bWaferInStage = this.InputStageUnit.IsRingPresent();
            bool bWaferinFeeder = IsRingPresent();
            if (WaferData == null)
            {
                Log.Write(UnitName, "OnRunWork: WaferUnloading - wafer is null, forced create wafer.");
            }

            if (bWaferInStage)
            {
                // [ADD] Stage 존재 여부와 데이터 검증
                int chk = CheckStageWaferBeforeUnload(WaferData);
                if (chk == -2)
                {
                    // Skip stage unload
                    NeedUnloadFirst = false;
                    this.State = ProcessState.Complete;
                    Log.Write(UnitName, "CheckStageWaferBeforeUnload", "Complete");
                    return 0;
                }
                if (chk != 0)
                {
                    AxisInputFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_WaferUnloadingFailed);
                    this.State = ProcessState.Error;
                    Log.Write(UnitName, "CheckStageWaferBeforeUnload", "Failed");
                    return -1;
                }
                if (IsStop)
                {
                    Log.Write(UnitName, "WaferUnloading", "IsStop-CheckStageWaferBeforeUnload");
                    return 0;
                }

                nRet = WaferUnloadingStage(WaferData);
                if (nRet != 0 && nRet != -2)
                {
                    AxisInputFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_WaferUnloadingFailed);
                    return 0;
                }
                if (nRet == -2)
                {
                    Log.Write(UnitName, "WaferUnloading", "IsStop-WaferUnloadingStage -2");
                    return 0;
                }
                if (IsStop)
                {
                    Log.Write(UnitName, "WaferUnloading", "IsStop-WaferUnloadingStage");
                    return 0;
                }

                // Stage -> Feeder 이동 후 검증
                if (VerifyWaferMovedStageToFeeder(WaferData) != 0)
                {
                    AxisInputFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_WaferUnloadingFailed);
                    Log.Write(UnitName, "VerifyWaferMovedStageToFeeder", "Failed");
                    return -1;
                }
                if (IsStop)
                {
                    Log.Write(UnitName, "WaferUnloading", "IsStop-VerifyWaferMovedStageToFeeder");
                    return 0;
                }

                nRet = WaferUnloadingFeeder(WaferData);
                if (nRet != 0)
                {
                    if (nRet != -2)
                    {
                        AxisInputFeederY.EmgStop();
                    }
                    else
                    {
                        if (nRet == -2)
                        {
                            return 0;
                        }
                        AxisInputFeederY.EmgStop();
                        PostAlarm((int)AlarmKeys.Alarm_WaferUnloadingFailed);
                        Log.Write(UnitName, "WaferUnloadingFeeder", "Failed");
                    }
                }
                if (IsStop)
                {
                    Log.Write(UnitName, "WaferUnloading", "IsStop-WaferUnloadingFeeder");
                    return 0;
                }
            }
            return nRet;
        }

        private int WaferUnloadingStage(MaterialWafer wafer)
        {
            int nRet = 0;

            nRet = this.InputStageUnit.PrepareInputStageUnloadingWafer();
            if (nRet != 0)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                this.State = ProcessState.Error;
                Log.Write(this, "WaferUnloading Fail - PrepareInputStageUnloadingWafer");
                return nRet;
            }
            if (IsStop) { return 0; }

            nRet = UnloadWaferStagetToFeeder();
            if (nRet != 0)
            {
                AxisInputFeederY.EmgStop();
                // Stage -> Feeder 이동 실패는 StageUnloadingFailed가 더 정확
                PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                this.State = ProcessState.Error;
                Log.Write(this, "WaferUnloading Fail - UnloadWaferStagetToFeeder");
                return nRet;
            }
            if (IsStop) { return 0; }

            nRet = ClampGripper();
            if (nRet != 0)
            {
                AxisInputFeederY.EmgStop();
                // ClampGripper 내부 timeout은 2032로 떨어짐(상세)
                // 여기서는 보조로 2020을 올리지 않고 그대로 리턴
                Log.Write(this, "UnloadWaferFeederToCassette Fail - ClampGripper");
                return -1;
            }


            //여기서 피더에 wafer 정보가 없으면 넘긴다. 
            //있으면 Pass

            var waferFromStage = wafer;
            this.InputStageUnit.MoveMaterial(waferFromStage, this);
            this.InputStageUnit.SetMaterial(null);

            if (VerifyWaferMovedStageToFeeder(waferFromStage) != 0)
            {
                return -1;
            }

            //여기서부터 보자!! 26-02-07
            // SlotIndex / EmptySlot 검증은 "Feeder->Cassette 단계"에서 최종 수행이 더 자연스럽지만,
            // 기존 로직 유지: 여기서도 1차로 Empty 확인하되 알람은 명확히 분리
            var waferFromFeeder = this.GetMaterial() as MaterialWafer;
            int slotFromStage = (waferFromFeeder != null) ? waferFromFeeder.SlotIndex : -1;
            int lifterSlot = this.InputCassetteLifterUnit.GetCurrectSlotID();
            int nSlot = slotFromStage >= 0 ? slotFromStage : lifterSlot;
            if (nSlot < 0)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_UnloadTargetSlotInvalid);
                this.State = ProcessState.Error;
                Log.Write(this, "WaferUnloadingFeeder - Invalid slot index (no Stage/Lifter slot)");
                return -1;
            }

            if (this.InputCassetteLifterUnit.IsSlotEmpty(nSlot) == false)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_CassetteSlotNotEmptyForUnload);
                this.State = ProcessState.Error;
                Log.Write(this, "WaferUnloadingFeeder Fail - IsSlotEmpty");
                return -1;
            }

            return 0;
        }
        private int WaferUnloadingFeeder(MaterialWafer wafer)
        {
            int nRet = 0;

            if (!IsRingPresent() && GetMaterial() == null)
            {
                Log.Write(UnitName, "[Unload] Feeder has no wafer -> skip feeder unload");
                NeedUnloadFirst = false;
                return -2;
            }

            int slotFromStage = (wafer != null) ? wafer.SlotIndex : -1;
            int lifterSlot = this.InputCassetteLifterUnit.GetCurrectSlotID();
            int nSlot = slotFromStage >= 0 ? slotFromStage : lifterSlot;
            if (nSlot < 0)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_UnloadTargetSlotInvalid);
                this.State = ProcessState.Error;
                Log.Write(this, "WaferUnloadingFeeder - Invalid slot index (no Stage/Lifter slot)");
                return -1;
            }

            if (this.InputCassetteLifterUnit.IsSlotEmpty(nSlot) == false)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_CassetteSlotNotEmptyForUnload);
                this.State = ProcessState.Error;
                Log.Write(this, "WaferUnloadingFeeder Fail - IsSlotEmpty");
                return -1;
            }

            nRet = this.InputCassetteLifterUnit.MoveToSlot(nSlot);
            if (nRet != 0)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_CassetteMoveToSlotFailedForUnload);
                this.State = ProcessState.Error;
                Log.Write(this, "WaferUnloadingFeeder Fail - MoveToSlot");
                return nRet;
            }

            // [FIX] 여기서부터 2004(Alarm_WaferUnloadingFailed) 제거.
            // UnloadWaferFeederToCassette() 내부에서 원인별 알람(2070~2074)을 이미 올리므로 그대로 전파만 한다.
            nRet = UnloadWaferFeederToCassette(true);
            if (nRet != 0)
            {
                if (nRet == -2)
                    return -2;

                AxisInputFeederY.EmgStop();
                this.State = ProcessState.Error;

                Log.Write(UnitName, "WaferUnloadingFeeder",
                    "UnloadWaferFeederToCassette failed -> propagate (no Alarm_WaferUnloadingFailed).");
                return nRet;
            }

            // 최종 상태 점검(여기서 2041로 분리됨)
            int v = VerifyAfterUnloadToCassette(nSlot);
            if (v != 0)
            {
                AxisInputFeederY.EmgStop();
                this.State = ProcessState.Error;
                return v;
            }

            return 0;
        }

        public int UnloadWaferFeederToCassette(bool isFine = false)
        {
            int nRet = 0;

            // 1) Cassette 위치로 이동 실패
            nRet = MovePositionCassette(isFine);
            if (nRet != 0)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_UnloadFeederToCassette_MoveFeederToCassettePosFailed);
                Log.Write(this, "UnloadWaferFeederToCassette Fail - MovePositionCassette");
                return -1;
            }

            // 2) Unclamp 실패(내부 timeout은 2033이 먼저 올라가지만, 이 단계 의미를 더 명확히 보여주기 위해 2071도 추가)
            nRet = UnClampGripper();
            if (nRet != 0)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_UnloadFeederToCassette_UnclampFailed);
                Log.Write(this, "UnloadWaferFeederToCassette Fail - UnClampGripper");
                return -1;
            }

            // 3) Feeder -> Cassette 데이터 반영 (여기서 기존엔 로그만 찍고 진행해서 추적이 매우 어려움)
            var waferOnFeeder = this.GetMaterial() as MaterialWafer;
            if (waferOnFeeder == null || waferOnFeeder.SlotIndex < 0)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_UnloadFeederToCassette_WaferDataInvalid);
                Log.Write(this, "UnloadWaferFeederToCassette Fail - waferOnFeeder null or SlotIndex invalid");
                return -1;
            }
            else
            {
                var cassette = this.InputCassetteLifterUnit.GetMaterialCassette();
                waferOnFeeder.ProcessSatate = Material.MaterialProcessSatate.Completed;
                waferOnFeeder.Presence = Material.MaterialPresence.Exist;
                cassette.SetWafer(waferOnFeeder.SlotIndex, waferOnFeeder);
            }

            // 4) 다음 로딩 가능 여부에 따라 대기 위치 결정 (이동 실패를 각각 알람으로 분리)
            bool hasNext = false;
            try 
            { 
                hasNext = InputCassetteLifterUnit != null && InputCassetteLifterUnit.IsHaveMoreProcessWafer(); 
            }
            catch (Exception ex) 
            {
                hasNext = false;
                Log.Write(ex);
            }

            if (hasNext)
            {
                nRet = MovePositionBarcode(isFine);
                if (nRet != 0)
                {
                    AxisInputFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_UnloadFeederToCassette_MoveStandbyBarcodeFailed);
                    Log.Write(UnitName, "UnloadWaferFeederToCassette", "MovePositionBarcode Failed");
                    return -1;
                }
                _exchangeStandbyForNextLoad = true;
            }
            else
            {
                nRet = MoveToReady(isFine);
                if (nRet != 0)
                {
                    AxisInputFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_UnloadFeederToCassette_MoveStandbyReadyFailed);
                    Log.Write(UnitName, "UnloadWaferFeederToCassette", "Fail - MoveToReady");
                    return -1;
                }
                _exchangeStandbyForNextLoad = false;
            }

            // 5) Feeder material 정리 (배출 완료 후 비움)
            this.SetMaterial(null);
            return 0;

        }
        #endregion

        #region Seq 단위 동작
        public int ClampGripper()
        {
            int nRet = 0;
            this.SetClamp(true);
            nRet = WaitClampStateOrAlarm(expectClamp: true, timeoutMs: 1500, pollMs: 2);
            if (nRet != 0)
            {
                AxisInputFeederY?.EmgStop();
                Log.Write(this, "Clamp Failed");
                return -1;
            }
            return 0;

            //if (!IsClamped())
            //{
            //    Log.Write(this, "Clamp Failed");
            //    PostAlarm((int)AlarmKeys.Alarm_GripperClampFailed);
            //    nRet = -1;
            //    return nRet;
            //}
            //return nRet;
        }
        public int UnClampGripper()
        {
            int nRet = 0;
            this.SetClamp(false);
            nRet = WaitClampStateOrAlarm(expectClamp: false, timeoutMs: 1500, pollMs: 2);
            if (nRet != 0)
            {
                AxisInputFeederY?.EmgStop();
                Log.Write(this, "Unclamp Failed");
                return -1;
            }
            return 0;
            //if (!IsUnClamped())
            //{
            //    Log.Write(this, "Unclamp Failed");
            //    PostAlarm((int)AlarmKeys.Alarm_GripperClampFailed);
            //    nRet = -1;
            //    return nRet;
            //}
            //return nRet;
        }
        public int UpFeeder()
        {
            int nRet = 0;
            this.SetLift(true);
            nRet = WaitLiftStateOrAlarm(expectUp: true, timeoutMs: 1500, pollMs: 2);
            if (nRet != 0)
            {
                AxisInputFeederY?.EmgStop();
                //DownFeeder(); // 실패 시 다운 시도는 오히려 위험할 수 있어서 제거. (실패 원인에 따라선 다운도 안될 수 있고, 중간에 멈추면 더 위험)
                Log.Write(this, "Feeder Up Failed");
                return -1;
            }
            return 0;
        }
        public int DownFeeder()
        {
            int nRet = 0;
            this.SetLift(false);
            nRet = WaitLiftStateOrAlarm(expectUp: false, timeoutMs: 1500, pollMs: 2);
            if (nRet != 0)
            {
                AxisInputFeederY?.EmgStop();
                //UpFeeder(); // 실패 시 업 시도는 오히려 위험할 수 있어서 제거. (실패 원인에 따라선 업도 안될 수 있고, 중간에 멈추면 더 위험)
                Log.Write("InputFeeder", "WaferLoading", "Feeder Down Failed");
                return -1;
            }
            return 0;
        }

        public int MoveToCassette(bool isFine = false)
        {
            int nRet = 0;

            CurrentFunc = MoveToCassette;

            Log.Write(this, "MoveToCassette Start");
            nRet = MovePositionCassette(isFine);
            if (nRet != 0)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_WaferLoadingFailed);
                nRet = -1;
                return nRet;
            }
            //if (IsStop) { return 0; }

            nRet = ClampGripper();
            if (nRet != 0)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_WaferLoadingFailed);
                nRet = -1;
                return nRet;
            }

            Log.Write(this, "MoveToCassette End");
            return nRet;
        }
        public Task<int> MoveToCassetteAsync(bool isFine)
        {
            return Task.Run(() => OnMoveToCassette(isFine));
        }
        protected int OnMoveToCassette(bool isFine)
        {
            int nRet = 0;
            if (IsInterlockOKWaferLoading() == false)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_WaferLoadingFailed);
                Log.Write(this, "OnMoveToCassette Fail - IsInterlockOKWaferLoading");
                nRet = -1;
                return nRet;
            }
            nRet = base.MoveTeachingPositionOnce((int)InputFeederConfig.TeachingPositionName.Cassette, isFine);
            if (nRet != 0)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_WaferLoadingFailed);
                Log.Write(this, "OnMoveToCassette Fail - MoveTeachingPositionOnce");
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
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_BarcodeReadingFailed);
                Log.Write(this, "BarcodeReading Fail - MovePositionBarcode");
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

            var equipment = Equipment.Instance;
            bool IsDryRunEqp = equipment.EquipmentConfig.IsDryRun;
            if (Config.IsSimulation || (Config.IsDryRun || IsDryRunEqp))
            {
                var now = DateTime.Now;
                strBarcode = "TestWafer" + now.ToString("yyyyMMddHHmm"); // yyyyMMddHHmm 도 가능
            }
            else
            {
                strBarcode = InputCassetteLifterUnit.ReadBarcoder();
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
                // 앞/뒤로 움직이면서 Barcode 재시도 후에 알람 발생.
                //PostAlarm((int)AlarmKeys.Alarm_BarcodeReadingFailed);
                Log.Write(this, "Barcode Reading Failed");
                nRet = -1;
                return nRet;
            }

            return nRet;
        }


        public int UnloadWaferStagetToFeeder(bool isFine = false)
        {
            int nRet = 0;

            nRet = UnClampGripper();
            if (nRet != 0)
            {
                PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                Log.Write(this, "UnloadWaferStagetToFeeder Fail - UnClampGripper");
                nRet = -1;
                return nRet;
            }

            nRet = DownFeeder();
            if (nRet != 0)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                Log.Write(this, "UnloadWaferStagetToFeeder Fail - DownFeeder");
                nRet = -1;
                return nRet;
            }

            nRet = MovePositionStage(isFine);
            if (nRet != 0)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                Log.Write(this, "UnloadWaferStagetToFeeder Fail - MovePositionStage");
                nRet = -1;
                return nRet;
            }

            return nRet;
        }
        private bool IsInterlockOKWaferLoading()
        {
            bool bRtn = true;
            // Cassette or InputStage 위치 및 Signal 확인 후 진행. 
            if (!InputCassetteLifterUnit.IsWaferReadyForLoading())
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_IsWaferReadyForLoading);
                Log.Write(this, "InputCassetteLifter Not Ready for Loading");
                bRtn = false;
                return bRtn;
            }

            // 이거 애매한디...
            if (!InputStageUnit.IsPositionWaferLoading())
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_WaferLoadingPosition);
                Log.Write(this, "InputStage Not Ready for Loading");
                bRtn = false;
                return bRtn;
            }
            return bRtn;
        }
        public bool IsInterlockOKWithCassete()
        {
            bool bRtn = true;

            double dYSafePosOffset = Config.FeederToCassetteOverapLength;
            if (Config.IsSimulation == false)
            {
                if (IsClamped())
                {
                    dYSafePosOffset += Config.WaferRingframeSize;
                }
            }

            var tp = TeachingPositions[(int)InputFeederConfig.TeachingPositionName.Cassette];
            double dInterlockPos = tp.GetAxisPosition(this.AxisInputFeederY.Name);

            dInterlockPos += dYSafePosOffset;
            if (AxisInputFeederY.GetPosition() < dInterlockPos)
            {
                Log.Write(this.UnitName, "IsInterlockOKWithCassete",
                $"FeederY Position Low. Current:" +
                $"{AxisInputFeederY.GetPosition()}, InterlockPos:{dInterlockPos}");

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
            var equipment = Equipment.Instance;
            bool IsDryRunEqp = equipment.EquipmentConfig.IsDryRun;
            if (Config != null 
                && (Config.IsSimulation || (Config.IsDryRun || IsDryRunEqp)))
            {
                if (AxisInputFeederY != null)
                {
                    double pos = 0;
                    try { pos = AxisInputFeederY.GetPosition(); } 
                    catch (Exception ex) { Log.Write(ex); }

                    if (Math.Abs(pos) < 0.01) // 필요 시 공차 Config 로 분리 가능
                    {
                        nRet = MovePositionReady();
                        if (nRet != 0)
                        {
                            Log.Write(UnitName, "CheckReady Fail - MovePositionReady");
                            return nRet;
                        }

                        Log.Write(UnitName, "Simulation - FeederY Position 0 → Ready 통과 (NoPosition 체크 생략)");
                        return nRet; // 바로 OK
                    }
                }
            }

            // 알려진 포지션이 전혀 아니면 오류
            if (IsPositionCassette() == false
                && IsPositionBarcode() == false
                && IsPositionStage() == false
                && IsPositionReady() == false
                && IsPositionSetPos() == false)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_InputFeederNoPosition);
                Log.Write(UnitName, "CheckReady Fail - No Position");
                return -1;
            }

            if(IsPositionSetPos())
            {
                if (IsRingPresent() == true
                    || IsClamped() == true)
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

                if (IsFeederUp() == false)
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

            if (InputStageUnit == null || InputStageUnit.IsStageInterLockOK() == false)
            {
                if (IsPositionReady())
                {
                    return 0;
                }
                else
                {
                    AxisInputFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_InputStageInterlockFailed);
                    Log.Write(UnitName, "CheckReady Fail - InputStage.IsStageInterLockOK");
                    return -1;
                }
            }

            //Barcode 실패 시 Ready.
            if(IsPositionBarcode())
            {
                if(InputCassetteLifterUnit.IsAnyAxisMoving())
                {
                    InputCassetteLifterUnit.WaferLifterZ.EmgStop();
                }
                if(IsFeederDown())
                {
                    if (IsRingPresent())
                    {
                        // 잡고 있던 못잡고 있던 일단 cassette 위치로 이동해서 내려놓고 시작하자.
                        if (IsUnClamped() == false || IsUnClamped() == true)
                        {
                            nRet = MovePositionCassette();
                            if (nRet != 0)
                            {
                                AxisInputFeederY.EmgStop();
                                PostAlarm((int)AlarmKeys.Alarm_InputStageInterlockFailed);
                                Log.Write(UnitName, "CheckReady Fail - InputStage.IsStageInterLockOK");
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
                                AxisInputFeederY.EmgStop();
                                PostAlarm((int)AlarmKeys.Alarm_InputStageInterlockFailed);
                                Log.Write(UnitName, "CheckReady Fail - InputStage.IsStageInterLockOK");
                                return -1;
                            }
                        }
                        else
                        {
                            AxisInputFeederY.EmgStop();
                            PostAlarm((int)AlarmKeys.Alarm_InputStageInterlockFailed);
                            Log.Write(UnitName, "CheckReady Fail - InputStage.IsStageInterLockOK");
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
                            AxisInputFeederY.EmgStop();
                            PostAlarm((int)AlarmKeys.Alarm_InputStageInterlockFailed);
                            Log.Write(UnitName, "CheckReady Fail - InputStage.IsStageInterLockOK");
                            return -1;
                        }

                        //AxisInputFeederY.EmgStop();
                        //PostAlarm((int)AlarmKeys.Alarm_InputStageInterlockFailed);
                        //Log.Write(UnitName, "CheckReady Fail - InputStage.IsStageInterLockOK");
                        //return -1;
                    }
                }
                else
                {
                    AxisInputFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_InputStageInterlockFailed);
                    Log.Write(UnitName, "CheckReady Fail - InputStage.IsStageInterLockOK");
                    return -1;
                }
            }

            if (IsPositionCassette()
                || IsPositionBarcode()
                || IsPositionStage())
            {
                if (IsInterlockOKWithCassete() == false)
                {
                    AxisInputFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_InputFeederInterlockFailed);
                    Log.Write(UnitName, "CheckReady Fail - IsInterlockOKWithCassete");
                    return -1;
                }

                if (InputStageUnit.IsPositionWaferLoading() == false
                && InputStageUnit.IsPositionWaferUnloading() == false)
                {
                    AxisInputFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_InputStageInterlockFailed);
                    Log.Write(UnitName, "CheckReady Fail - InputStage.IsStageInterLockOK");
                    return -1;
                }

                if (IsRingPresent() == true 
                    || IsClamped() == true)
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

                if (IsFeederUp() == false)
                {
                    nRet = UpFeeder();
                    if (nRet != 0)
                    {
                        Log.Write(UnitName, "CheckReady Fail - UpFeeder");
                        return nRet;
                    }
                }
            }
            else
            {
                if (IsFeederUp() == false)
                {
                    nRet = UpFeeder();
                    if (nRet != 0)
                    {
                        Log.Write(UnitName, "CheckReady Fail - UpFeeder");
                        return nRet;
                    }
                }
            }

                return nRet;
        }

        // 클래스 내부에 추가
        public void ResetForNewRun(bool moveToSafeReady = true)
        {
            // 1) 플래그/스텝 초기화
            _isSafetyMoving = false;
            _exchangeStandbyForNextLoad = false;
            NeedUnloadFirst = false;
            IsWaferLoadDone = false;

            _loadStep = LoadFlowStep.None;
            _unloadStep = UnloadFlowStep.None;
            _unloadTargetSlot = -1;

            // 2) Feeder 보유 머티리얼 제거
            try
            {
                this.SetMaterial(null);
            }
            catch (Exception ex)
            {
                Log.Write(this, $"ResetForNewRun SetMaterial(null) failed: {ex.Message}");
            }

            // 3) 안전 IO 상태 확보 (실기에서만 대기)
            try
            {
                if (!(Config?.IsSimulation == true 
                    || Config?.IsDryRun == true))
                {
                    if (!IsUnClamped())
                        UnClampGripper();   // 타임아웃 시 내부 알람 처리
                }
            }
            catch (Exception ex)
            {
                Log.Write(this, $"ResetForNewRun IO safe-state failed: {ex.Message}");
            }

            // 4) Ready 위치 복귀(선택)
            if (moveToSafeReady)
            {
                try
                {
                    // 내부에서 위치/인터락을 점검하며 필요 시 Ready로 이동
                    EnsureReady();
                }
                catch (Exception ex)
                {
                    Log.Write(this, $"ResetForNewRun EnsureReady failed: {ex.Message}");
                }
            }

            // 3) 안전 IO 상태 확보 (실기에서만 대기)
            try
            {
                if (!(Config?.IsSimulation == true
                    || Config?.IsDryRun == true))
                {
                    if (!IsFeederUp())
                        UpFeeder();
                }
            }
            catch (Exception ex)
            {
                Log.Write(this, $"ResetForNewRun IO safe-state failed: {ex.Message}");
            }

            // 5) 현재 수동 함수 포인터 정리(수동 재개 시 혼동 방지)
            this.CurrentFunc = null;
        }
        #endregion

        // [ADD] 바코드(=WaferId) 확정 시 UI로 알리기 위한 이벤트
        public event Action<string> WaferIdChanged;

        // [ADD] 이벤트 호출 헬퍼
        private void RaiseWaferIdChanged(string waferId)
        {
            try { WaferIdChanged?.Invoke(waferId); }
            catch (Exception ex) { Log.Write(ex); }
        }

        public int MoveToTeachingPositionBySelectionIndex(int teachingSelIndex, bool isFine = false)
        {
            if (Config == null)
                return -1;

            string tpName;
            if (!Config.GetTeachingPositionName(teachingSelIndex, out tpName) || string.IsNullOrWhiteSpace(tpName))
                return -1;

            InputFeederConfig.TeachingPositionName en;
            if (!Enum.TryParse(tpName, out en))
                return -1;

            switch (en)
            {
                case InputFeederConfig.TeachingPositionName.Ready:
                    return MovePositionReady(isFine);

                case InputFeederConfig.TeachingPositionName.Stage:
                    return MovePositionStage(isFine);
                    
                case InputFeederConfig.TeachingPositionName.Barcode:
                    return MovePositionBarcode(isFine);
                    
                case InputFeederConfig.TeachingPositionName.Cassette:
                    return MovePositionCassette(isFine);
                    
                case InputFeederConfig.TeachingPositionName.SetPosition:
                    break;

                default:
                    break;
            }

            return 0;
        }


        private bool IsSafeToStartInputWaferUnloading()
        {
            // OutputStage가 없으면 기존 동작 유지(보수적으로 true로 두거나 false로 막을지 정책 선택)
            if (OutputStageUnit == null)
                return true;

            // (1) OutputStage wafer가 없으면(혹은 링 감지 안됨) -> output 쪽 작업 없음
            var outWafer = OutputStageUnit.GetMaterialWafer();
            if (outWafer == null)
                return true;

            // (2) OutputStage가 Completed면 OK
            if (outWafer.ProcessSatate == Material.MaterialProcessSatate.Completed)
            {
                Log.Write(UnitName,
                    "IsSafeToStartInputWaferUnloading: OutputStage wafer is Completed -> OK to unload input wafer.");
                return true;
            }

            var inArmEmpty = InputDieTransferUnit?.GetMaterial() as MaterialDie;
            bool rotaryEmpty = RotaryUnit.IsHaveDie();
            var outArmEmpty = OutputDieTransferUnit?.GetMaterial() as MaterialDie;
            if (inArmEmpty == null && rotaryEmpty == false && outArmEmpty == null)
            {
                return true;
            }

            // rc == 1 이면 아직 버퍼가 남아있다는 뜻
            return false;
        }
    }
}