using LCP_280;
using QMC.Common;
using QMC.Common.Component;
using QMC.Common.IOUtil;
using QMC.Common.Motion;
using QMC.Common.Motions;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Component; // TeachingPosition
using QMC.LCP_280.Process.Unit.FormWork.Repro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Navigation;
using static QMC.LCP_280.Process.Unit.FormSetup.BarcoderControl;

namespace QMC.LCP_280.Process.Unit
{
    public class InputFeeder : BaseUnit<InputFeederConfig>
    {
        enum AlarmKeys
        {
            Alarm_WaferLoadingFailed = 2000,
            Alarm_BarcodeReadingFailed = 2001,
            Alarm_StageLoadingFailed = 2002,
            Alarm_StageUnloadingFailed = 2003,
            Alarm_WaferUnloadingFailed = 2004,
            Alarm_InputStageInterlockFailed = 2010,
            Alarm_GripperClampFailed = 2020,
            Alarm_FeederClampUp = 2021,
            Alarm_IsWaferReadyForLoading = 2022,
            Alarm_WaferLoadingPosition = 2023,
            Alarm_InputCassetteLifteInterlockFailed = 2024,
            Alarm_InputFeederNoPosition = 2025,
            Alarm_InputFeederInterlockFailed = 2026,
            Alarm_GripperUnClampFailed = 2027,
            Alarm_WaferDataFaild = 2028,
            Alarm_VerifyWaferMovedStageToFeeder,
            Alarm_AlignT,
        }
        #region InitAlarm
        protected override void InitAlarm()
        {
            base.InitAlarm();
            AlarmRegister((int)AlarmKeys.Alarm_WaferLoadingFailed,
                "Wafer Loading Failed",
                "웨이퍼 로딩에 실패 하였습니다.",
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
            AlarmRegister((int)AlarmKeys.Alarm_WaferUnloadingFailed,
                "Wafer Unloading Failed",
                "웨이퍼 언로딩에 실패 하였습니다.",
                "Error");
            AlarmRegister((int)AlarmKeys.Alarm_InputStageInterlockFailed,
                "Input Stage Interlock Failed",
                "웨이퍼 로딩을 위한 인터락이 맞지 않습니다. 장비 상태를 확인 하여 주십시요.",
                "Error");
            AlarmRegister((int)AlarmKeys.Alarm_GripperClampFailed,
                "Gripper Clamp Failed",
                "그리퍼 클램프에 실패 하였습니다. 장비 상태를 확인 하여 주십시요.",
                "Error");
            AlarmRegister((int)AlarmKeys.Alarm_FeederClampUp,
                "Feeder Clamp Up Failed",
                "피더 클램프 업 상태가 아닙니다. 장비 상태를 확인 하여 주십시요.",
                "Error");
            AlarmRegister((int)AlarmKeys.Alarm_IsWaferReadyForLoading,
                "IsWaferReadyForLoading Fail",
                "Cassette Ready For Loading Signal Fail. 장비 상태를 확인 하여 주십시요.",
                "Error");
            AlarmRegister((int)AlarmKeys.Alarm_WaferLoadingPosition,
                "WaferLoadingPosition",
                "Wafer LoadingPosition Fail. 장비 상태를 확인 하여 주십시요.",
                "Error");
            AlarmRegister((int)AlarmKeys.Alarm_InputCassetteLifteInterlockFailed,
                "Input Cassette Lifter Interlock Failed",
                "Input Cassette Lifter Interlock Failed. 장비 상태를 확인 하여 주십시요.",
                "Error");
            AlarmRegister((int)AlarmKeys.Alarm_InputFeederNoPosition,
                "Input Feeder No Position",
                "Input Feeder No Position. 장비 상태를 확인 하여 주십시요. 가까운 포지션으로 이동 바랍니다.",
                "Error");
            AlarmRegister((int)AlarmKeys.Alarm_InputFeederInterlockFailed,
                "Input Feeder Interlock Failed",
                "Input Feeder Interlock Failed. 장비 상태를 확인 하여 주십시요.",
                "Error");
            AlarmRegister((int)AlarmKeys.Alarm_GripperUnClampFailed,
                "Gripper UnClamp Failed",
                "Gripper UnClamp Failed. 장비 상태를 확인 하여 주십시요.",
                "Error");
            AlarmRegister((int)AlarmKeys.Alarm_WaferDataFaild,
                "Wafer Data Faild",
                "Wafer Data Faild. 장비 상태를 확인 하여 주십시요.",
                "Error");
            AlarmRegister((int)AlarmKeys.Alarm_VerifyWaferMovedStageToFeeder,
                "VerifyWaferMovedStageToFeeder Faild,",
                "VerifyWaferMovedStageToFeeder, Faild. 장비 상태를 확인 하여 주십시요.",
                "Error");
            AlarmRegister((int)AlarmKeys.Alarm_AlignT,
               "Alarm_AlignT Faild,",
               "Alarm_AlignT, Faild. 장비 상태를 확인 하여 주십시요.",
               "Error");
            AlarmRegister((int)AlarmKeys.Alarm_WaferLoadingFailed,
               "Alarm_WaferLoadingFailed Faild,",
               "Alarm_WaferLoadingFailed, Faild. 장비 상태를 확인 하여 주십시요.",
               "Error");

        }
        #endregion

        #region Unit
        public InputCassetteLifter InputCassetteLifterUnit { get; set; }
        public InputStage InputStageUnit { get; set; }
        #endregion

        #region Axes
        private MotionAxis _feederY;
        public MotionAxis AxisInputFeederY => _feederY;
        #endregion

        // Safety 동작 중 여부
        private bool _isSafetyMoving = false;

        #region IO Domain Members
        private Cylinder _feederLift; // Up/Down
        private Cylinder _cylClamp;   // Clamp/Unclamp
        #endregion

        #region Constructor / Initialization
        public InputFeeder(InputFeederConfig config = null)
            : base(new InputFeederConfig())
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

            if (feederSensor == false && feederObj == null)
            {
                // 실제로 옮겼어야 하는 타이밍인데 없다면 유실 가능성
                Log.Write(UnitName, "[Unload] Wafer missing on feeder after stage -> feeder transfer");
                PostAlarm((int)AlarmKeys.Alarm_WaferUnloadingFailed);
                return -1;
            }

            if (feederSensor && feederObj == null)
            {
                Log.Write(UnitName, "[Unload] Feeder ring detected but object null");
                PostAlarm((int)AlarmKeys.Alarm_WaferDataFaild);
                return -1;
            }

            if (feederObj != null && waferMoved != null)
            {
                // SlotIndex 일치 여부(불일치면 경고)
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

            // 센서 OFF인데 객체 남아있음 -> 정리 누락
            if (!feederSensor && feederObj != null)
            {
                Log.Write(UnitName, "[Unload] Feeder object remained although sensor off -> force clear");
                SetMaterial(null);
                return 0;
            }

            // 센서 ON인데 객체 없음 -> 유실
            if (feederSensor && feederObj == null)
            {
                Log.Write(UnitName, "[Unload] Sensor ON but no wafer object -> lost wafer?");
                PostAlarm((int)AlarmKeys.Alarm_WaferUnloadingFailed);
                return -1;
            }

            // 센서 ON + 객체 존재 -> 아직 클램프 해제 안되었거나 Slot 업데이트 실패 가능성
            if (feederSensor && feederObj != null)
            {
                Log.Write(UnitName, "[Unload] Feeder still holds wafer after unload-to-cassette step");
                PostAlarm((int)AlarmKeys.Alarm_WaferUnloadingFailed);
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

            if (Config.IsSimulation == false && Config.IsDryRun == false)
            {
                if(IsUnClamped() == false)
                {
                    PostAlarm((int)AlarmKeys.Alarm_InputFeederInterlockFailed);
                    Log.Write(this, "CheckMoveInterLockReady Fail - IsRingPresent()");
                    return -1;
                }
                //if (IsRingPresent() == true)
                //{
                    
                //}
            }


            if (!IsUnClamped())
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_GripperClampFailed);
                Log.Write(UnitName, "IsMoveInterLockReady", "Feeder Clamp 닫혀 있음. (Wafer 잡고 있는지 확인 필요)");
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

            if(Config.IsSimulation == false 
                && Config.IsDryRun == false)
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
            if (Config.IsSimulation || Config.IsDryRun)
            {
                return this.GetMaterial() is MaterialWafer;
                //return true;
            }
            return this.ReadInput(InputFeederConfig.IO.FEEDER_RING_CHECK);
        }
        public bool IsOverload()
        {
            if (Config.IsSimulation || Config.IsDryRun)
            {
                return true;
            }
            return this.ReadInput(InputFeederConfig.IO.FEEDER_OVERLOAD);
        }

        // === Cylinder 완료 대기 Helper ===
        // Clamp: expectClamp=true(Clamp 완료 기대), false(Unclamp 완료 기대)
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
                ? (int)AlarmKeys.Alarm_GripperClampFailed
                : (int)AlarmKeys.Alarm_GripperUnClampFailed;

            PostAlarm(alarm);
            Log.Write(UnitName, expectClamp ? "[Clamp] Gripper CLAMP timeout" : "[Clamp] Gripper UNCLAMP timeout");
            return -1;
        }
        // Lift: expectUp=true(UP 완료 기대), false(DOWN 완료 기대)
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

            // 별도 Down 실패 알람 키가 없어 기존 키 사용
            int alarm = expectUp
                ? (int)AlarmKeys.Alarm_FeederClampUp
                : (int)AlarmKeys.Alarm_WaferLoadingFailed;

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
        #endregion

        #region Lifecycle
        public override int OnRun()
        {
            int ret = 0;
            if (this.RunUnitStatus == UnitStatus.Stopped ||
                this.RunUnitStatus == UnitStatus.Stopping ||
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
            //this.State = ProcessState.Stop;

            _exchangeStandbyForNextLoad = false; // 초기화

            base.OnStop();
            return ret;
        }
        protected override int OnRunReady()
        {
            int nRet = 0;
            MaterialWafer waferStage = this.InputStageUnit.GetMaterialWafer();
            try
            {
                // Stage Wafer 작업 완료 시 true임.
                if (this.InputStageUnit.IsWorking())
                {
                    if (waferStage != null)
                    {
                        if(waferStage.ProcessSatate == Material.MaterialProcessSatate.Ready)
                        {
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
                            }
                            else
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
                        
                    }
                    return nRet;
                }
                else if (this.InputStageUnit.IsWorking() == false)
                {
                    bool sim = (Config.IsSimulation || Config.IsDryRun);
                    if (sim == false)
                    {
                        if (waferStage != null && waferStage.SlotIndex != -1)
                        {
                            // 실기: 센서 기반 존재 판단
                            if(InputStageUnit.IsRingPresent() &&
                               waferStage.ProcessSatate == Material.MaterialProcessSatate.Completed &&
                               InputStageUnit.IsPositionWaferLoading() == false &&
                               InputStageUnit.IsPositionWaferUnloading() == false)
                            {
                                NeedUnloadFirst = true;
                            }
                            else
                            {
                                NeedUnloadFirst = false;
                            }
                        }
                        else
                        {
                            NeedUnloadFirst = false;
                        }
                    }
                    else
                    {
                        // 시뮬/드라이런: 데이터 기반 판단
                        NeedUnloadFirst = (waferStage != null && waferStage.SlotIndex != -1);
                    }

                    //여기서 카세트에 웨이퍼의 작업이 전부 완료 되었을때 처리.
                    // 놉! Output이 완료되면 같이 멈추게 한다.
                }

                Log.Write(UnitName, "OnRunRady", "ProcessState.Work Start");
                this.State = ProcessState.Work;
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

            if (Config.IsSimulation == false
                && Config.IsDryRun == false)
            {
                //if (this.InputStageUnit.StageCamera.IsLiveOn)
                {
                    this.InputStageUnit.StageCamera.StopLive();
                }
            }

            MaterialWafer waferStage = this.InputStageUnit.GetMaterialWafer();
            // 0) Stage에 제품이 있고 작업 완료 상태이면 "언로딩 먼저"
            if (NeedUnloadFirst)
            {
                nRet = WaferUnloading();
                if (nRet != 0)
                {
                    AxisInputFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_WaferUnloadingFailed);
                    this.State = ProcessState.Error;
                    Log.Write(UnitName, "CheckStageWaferBeforeUnload", "Failed");
                    return -1;
                }

                if (IsStop)
                {
                    Log.Write(UnitName, "OnRunWork", "WaferUnloading");
                    return 0;
                }
                this.State = ProcessState.Complete;
            }
            else
            {
                // 1) Feeder -> Cassette: Scan
                nRet = this.InputCassetteLifterUnit.ScanWafer();
                if (nRet != 0)
                {
                    AxisInputFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_WaferLoadingFailed);
                    return nRet;
                }
                if (IsStop) 
                {
                    Log.Write(UnitName, "OnRunWork", "InputCassetteLifterUnit.ScanWafer");
                    return 0; 
                }

                // 공정진행해야할 Wafer 있는지 확인 후 진행.
                if (this.InputCassetteLifterUnit.IsHaveMoreProcessWafer())
                {
                    nRet = WaferLoading();
                    if (nRet != 0)
                    {
                        AxisInputFeederY.EmgStop();
                        PostAlarm((int)AlarmKeys.Alarm_WaferLoadingFailed);
                        this.State = ProcessState.Error;
                        Log.Write(UnitName, "WaferLoading", "Failed");
                        return -1;
                    }
                    if (IsStop)
                    {
                        Log.Write(UnitName, "OnRunWork", "WaferLoading Stop");
                        return 0;
                    }
                }
                else
                {
                    if (IsPositionReady() == false)
                    {
                        Log.Write(UnitName, "OnWork", "IsHaveMoreProcessWafer() None.");
                        nRet = MovePositionStage();
                        if (nRet != 0)
                        {
                            AxisInputFeederY.EmgStop();
                            PostAlarm((int)AlarmKeys.Alarm_WaferLoadingFailed);
                            this.State = ProcessState.Error;
                            Log.Write(this, "OnRunWork Fail - MovePositionStage");
                            return nRet;
                        }

                        nRet = MovePositionReady();
                        if (nRet != 0)
                        {
                            AxisInputFeederY.EmgStop();
                            PostAlarm((int)AlarmKeys.Alarm_WaferLoadingFailed);
                            this.State = ProcessState.Error;
                            Log.Write(this, "OnRunWork Fail - MovePositionReady");
                            return nRet;
                        }
                        nRet = UpFeeder();
                        if (nRet != 0)
                        {
                            AxisInputFeederY.EmgStop();
                            PostAlarm((int)AlarmKeys.Alarm_WaferLoadingFailed);
                            this.State = ProcessState.Error;
                            Log.Write(this, "OnRunWork Fail - MovePositionReady");
                            return nRet;
                        }
                    }
                }

                this.State = ProcessState.Complete;
                Log.Write(UnitName, "OnRunWork", "MovePositionReady.");
            }
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

        protected override int OnRunComplete()
        {
            int ret = 0;
            this.State = ProcessState.Ready;
            return ret;
        }
        #endregion

        protected override void OnMakeSequence()
        {
            base.OnMakeSequence();
            this.SequencePlayers.Add(PrepareLoadingStage);
            this.SequencePlayers.Add(UnloadWaferFromCassette);
            this.SequencePlayers.Add(StageLoading);
            this.SequencePlayers.Add(MoveToReady);
            this.SequencePlayers.Add(WaferUnloading);
        }


        #region Sequence Auto
        private int PreparetoInputStage()
        {
            int nRet = 0;
            // 6) 정렬/매핑
            nRet = InputStageUnit.AlignT();
            if (nRet != 0)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_AlignT);
                this.State = ProcessState.Error;
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
                //PostAlarm((int)AlarmKeys.Alarm_StageLoadingFailed);
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
                Log.Write(this, "PrepareLoadingStage Fail - InputStage.PrepareLoadingStage()");
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
                if ((!Config.IsSimulation && !Config.IsDryRun)
                   && (InputCassetteLifterUnit?.IsTriggerModeConfigured() == true))
                {
                    useTrigger = true;
                }

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
                            const double scanStep = 2.0;  // mm
                            const int scanPairs = 6;      // ±1~±6 step
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

                            // 스캔 종료 후 기준 위치 복귀
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
                            AxisInputFeederY.EmgStop();
                            PostAlarm((int)AlarmKeys.Alarm_BarcodeReadingFailed);
                            Log.Write(UnitName, "WaferLoading", "Barcode Reading Failed after trigger scan");
                            return -1;
                        }
                    }
                }
                if (!useTrigger)
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

                                //아래는 필요시 멈추자.
                                //if (IsStop)
                                //{
                                //    Log.Write(UnitName, "WaferLoading", "Barcode scan stopped by IsStop");
                                //    return 0;
                                //}

                                // -오프셋
                                double targetMinus = basePosY - (scanStep * i);
                                nRet = MoveAxisPositionOne(AxisInputFeederY, targetPlus, isFine);
                                if (nRet != 0)
                                {
                                    AxisInputFeederY.EmgStop();
                                    Log.Write(UnitName, "WaferLoading", "Move Y -offset fail during barcode scan");
                                    return -1;
                                }
                                Thread.Sleep(settleMs);

                                nRet = GetBarcode(out strBarcode);
                                if (nRet == 0 && !string.IsNullOrEmpty(strBarcode))
                                    break;
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
                            AxisInputFeederY.EmgStop();
                            PostAlarm((int)AlarmKeys.Alarm_BarcodeReadingFailed);
                            Log.Write(UnitName, "WaferLoading", "Barcode Reading Failed after scanning");
                            return -1;
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
        
        // Seq =
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
            //else if(bWaferinFeeder)
            //{
            //    nRet = WaferUnloadingFeeder(WaferData);
            //    if (nRet != 0)
            //    {
            //        if (nRet != -2)
            //        {
            //            AxisInputFeederY.EmgStop();
            //        }
            //        else
            //        {
            //            if (nRet == -2)
            //            {
            //                return 0;
            //            }
            //            AxisInputFeederY.EmgStop();
            //            PostAlarm((int)AlarmKeys.Alarm_WaferUnloadingFailed);
            //            Log.Write(UnitName, "WaferUnloadingFeeder", "Failed");
            //        }
            //    }
            //    if (IsStop)
            //    {
            //        Log.Write(UnitName, "WaferUnloading", "bWaferinFeeder::true, IsStop-WaferUnloadingFeeder");
            //        return 0;
            //    }
            //}
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

            // 9) Feeder 내부 언로딩
            nRet = UnloadWaferStagetToFeeder();
            if (nRet != 0)
            {
                AxisInputFeederY.EmgStop();
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
                PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                Log.Write(this, "UnloadWaferFeederToCassette Fail - ClampGripper");
                nRet = -1;
                return nRet;
            }

            // Stage의 실제 웨이퍼를 가져와 그대로 Feeder로 이동
            var waferFromStage = wafer;// this.InputStage.GetMaterialWafer();

            this.InputStageUnit.MoveMaterial(waferFromStage, this);
            this.InputStageUnit.SetMaterial(null);

            // 이동 검증
            if (VerifyWaferMovedStageToFeeder(waferFromStage) != 0)
            {
                Log.Write(UnitName, "VerifyWaferMovedStageToFeeder", "Failed");
                return -1;
            }

            // 안전한 언로딩 슬롯 산출: Stage wafer.SlotIndex → 없으면 Lifter 현재 슬롯
            var waferFromFeeder = this.GetMaterial() as MaterialWafer;
            int slotFromStage = (waferFromFeeder != null) ? waferFromFeeder.SlotIndex : -1;
            int lifterSlot = this.InputCassetteLifterUnit.GetCurrectSlotID();
            int nSlot = slotFromStage >= 0 ? slotFromStage : lifterSlot;
            if (nSlot < 0)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_WaferUnloadingFailed);
                this.State = ProcessState.Error;
                Log.Write(this, "WaferUnloadingFeeder - Invalid slot index (no Stage/Lifter slot)");
                return -1;
            }
            Log.Write(UnitName, "WaferUnloadingFeeder", $"WaferUnloading - MoveToSlot : {nSlot}");

            // 카세트 슬롯 Empty 확인
            if (this.InputCassetteLifterUnit.IsSlotEmpty(nSlot) == false)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_WaferUnloadingFailed);
                this.State = ProcessState.Error;
                Log.Write(this, "WaferUnloadingFeeder Fail - IsSlotEmpty");
                return nRet;
            }

            Log.Write(this, "WaferUnloading Complete");
            return nRet;
        }
        private int WaferUnloadingFeeder(MaterialWafer wafer)
        {
            int nRet = 0;

            // 피더에 실제로 웨이퍼 있는지 먼저 확인 (없으면 Skip)
            if (!IsRingPresent() && GetMaterial() == null)
            {
                Log.Write(UnitName, "[Unload] Feeder has no wafer -> skip feeder unload");
                NeedUnloadFirst = false;
                return -2;
            }

            var waferFromFeeder = this.GetMaterial() as MaterialWafer;
            // 안전한 언로딩 슬롯 산출: Stage wafer.SlotIndex → 없으면 Lifter 현재 슬롯
            int slotFromStage = (wafer != null) ? wafer.SlotIndex : -1;
            int lifterSlot = this.InputCassetteLifterUnit.GetCurrectSlotID();
            int nSlot = slotFromStage >= 0 ? slotFromStage : lifterSlot;
            if (nSlot < 0)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_WaferUnloadingFailed);
                this.State = ProcessState.Error;
                Log.Write(this, "WaferUnloadingFeeder - Invalid slot index (no Stage/Lifter slot)");
                return -1;
            }
            Log.Write(UnitName, "WaferUnloadingFeeder", $"WaferUnloading - MoveToSlot : {nSlot}");

            // 카세트 슬롯 Empty 확인
            if (this.InputCassetteLifterUnit.IsSlotEmpty(nSlot) == false)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_WaferUnloadingFailed);
                this.State = ProcessState.Error;
                Log.Write(this, "WaferUnloadingFeeder Fail - IsSlotEmpty");
                return nRet;
            }
            if (IsStop) 
            {
                Log.Write(UnitName, "WaferUnloadingFeeder", "InputCassetteLifterUnit.IsSlotEmpty");
                return 0; 
            }


            nRet = this.InputCassetteLifterUnit.MoveToSlot(nSlot); // 언로딩 해야하는 Slot으로 이동 요청.
            if (nRet != 0)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_WaferUnloadingFailed);
                this.State = ProcessState.Error;
                Log.Write(this, "WaferUnloadingFeeder Fail - MoveToSlot");
                return nRet;
            }
            if (IsStop)
            {
                Log.Write(UnitName, "WaferUnloadingFeeder", "InputCassetteLifterUnit.MoveToSlot");
                return 0;
            }

            nRet = UnloadWaferFeederToCassette(true);
            if (nRet != 0)
            {
                if(nRet == -2)
                {
                    Log.Write(this, "WaferUnloadingFeeder End - UnloadWaferFeederToCassette");
                    return nRet;
                }
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_WaferUnloadingFailed);
                this.State = ProcessState.Error;
                Log.Write(this, "WaferUnloadingFeeder Fail - UnloadWaferFeederToCassette");
                return nRet;
            }
            // 최종 상태 점검
            VerifyAfterUnloadToCassette(nSlot);

            if (IsStop)
            {
                Log.Write(UnitName, "WaferUnloadingFeeder", "UnloadWaferFeederToCassette");
                return 0;
            }

            Log.Write(this, "WaferUnloadingFeeder Complete");
            return nRet;
        }
        public int UnloadWaferFeederToCassette(bool isFine = false)
        {
            int nRet = 0;

            nRet = MovePositionCassette(isFine);
            if (nRet != 0)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                Log.Write(this, "UnloadWaferFeederToCassette Fail - MovePositionCassette");
                nRet = -1;
                return nRet;
            }
            //if (IsStop) { return 0; }

            nRet = UnClampGripper();
            if (nRet != 0)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                Log.Write(this, "UnloadWaferFeederToCassette Fail - UnClampGripper");
                nRet = -1;
                return nRet;
            }
            // 피더 -> 카세트: 웨이퍼 정보 되돌려 넣기
            var waferOnFeeder = this.GetMaterial() as MaterialWafer;
            if (waferOnFeeder != null && waferOnFeeder.SlotIndex >= 0)
            {
                var cassette = this.InputCassetteLifterUnit.GetMaterialCassette();
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
            try { hasNext = InputCassetteLifterUnit != null && InputCassetteLifterUnit.IsHaveMoreProcessWafer(); }
            catch { hasNext = false; }

            if (hasNext)
            {
                nRet = MovePositionBarcode(isFine);
                if (nRet != 0)
                {
                    AxisInputFeederY.EmgStop();
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
                    AxisInputFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_WaferLoadingFailed);
                    Log.Write(UnitName, "UnloadWaferFeederToCassette", "Fail - MoveToReady");
                    return -1;
                }
                _exchangeStandbyForNextLoad = false;
            }

            // Feeder의 material 정리 (배출 완료 후 비움)
            this.SetMaterial(null);
            return nRet;

            //회피 Position으로 사용.
            //nRet = MovePositionBarcode(isFine);
            //if (nRet != 0)
            //{
            //    AxisInputFeederY.EmgStop();
            //    PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
            //    Log.Write(this, "UnloadWaferFeederToCassette Fail - MovePositionBarcode");
            //    nRet = -1;
            //    return nRet;
            //}
            //// 다음 로딩은 바코드에서 시작하도록 표시
            //_exchangeStandbyForNextLoad = true;
            //// Feeder의 material 정리 (배출 완료 후 비움)
            //this.SetMaterial(null);
            ////wafer = new MaterialWafer();
            ////MoveMaterial(wafer, null);

            //// ← 추가: 전 슬롯 완료되었는지 검사하여 1회 알람
            //try 
            //{ 
            //    nRet = this.InputCassetteLifter.CheckCassetteCompletedAndAlarmOnce();
            //    if(nRet != 0)
            //    {
            //        this.Stop();
            //        InputCassetteLifter.Stop();
            //        InputStage.Stop();
            //        return -2;
            //    }
            //} 
            //catch (Exception ex)
            //{
            //    Log.Write(ex);
            //}
            //return nRet;
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
                DownFeeder();
                Log.Write(this, "Feeder Up Failed");
                return -1;
            }
            return 0;
            //if (!IsFeederUp())
            //{
            //    Log.Write(this, "Feeder Up Failed");
            //    PostAlarm((int)AlarmKeys.Alarm_GripperClampFailed);
            //    nRet = -1;
            //    return nRet;
            //}
            //return nRet;
        }
        public int DownFeeder()
        {
            int nRet = 0;
            this.SetLift(false);
            nRet = WaitLiftStateOrAlarm(expectUp: false, timeoutMs: 1500, pollMs: 2);
            if (nRet != 0)
            {
                AxisInputFeederY?.EmgStop();
                UpFeeder();
                Log.Write("InputFeeder", "WaferLoading", "Feeder Down Failed");
                return -1;
            }
            return 0;
            //if (!IsFeederDown())
            //{
            //    AxisInputFeederY.EmgStop();
            //    Log.Write("InputFeeder", "WaferLoading", "Feeder Down Failed");
            //    PostAlarm((int)AlarmKeys.Alarm_WaferLoadingFailed);
            //    nRet = -1;
            //    return nRet;
            //}
            //return nRet;
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

            if(Config.IsSimulation
                || Config.IsDryRun)
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
            if (Config != null 
                && (Config.IsSimulation || Config.IsDryRun))
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
                && IsPositionReady() == false)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_InputFeederNoPosition);
                Log.Write(UnitName, "CheckReady Fail - No Position");
                return -1;
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
    }
}