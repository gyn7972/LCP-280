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
        }
        #endregion

        #region Unit
        public OutputCassetteLifter OutputCassetteLifter { get; set; }
        public OutputStage OutputStage { get; set; }
        public InputStage InputStage { get; set; }
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
                    bool stageAtSafe = (this.OutputStage != null) &&
                               (this.OutputStage.IsPositionBinLoading() || this.OutputStage.IsPositionBinUnloading());

                    if (stageAtSafe == false)
                    {
                        this.AxisOutputFeederY?.EmgStop();
                        PostAlarm((int)AlarmKeys.Alarm_OutputStageInterlockFailed);
                        bRet = false;
                    }
                    else
                    {
                        if (this.IsPositionCassette())
                        {
                            bRet = IsInterlockOKWithCassette(e);
                            if (bRet == false)
                            {
                                this.AxisOutputFeederY?.EmgStop();
                                PostAlarm((int)AlarmKeys.Alarm_OutputStageInterlockFailed);
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
                    PostAlarm((int)AlarmKeys.Alarm_OutputStageInterlockFailed);
                    bRet = false;
                }
            }
            return bRet;
        }
        private bool IsInterlockOKWithCassette(BaseComponent.InterlockEventArgs e)
        {
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
            if (!OutputCassetteLifter.IsBinReadyForLoading())
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_IsBinReadyForLoading);
                Log.Write(this, "OutputCassetteLifter Not Ready for Loading");
                bRtn = false;
                return bRtn;
            }

            if (!OutputStage.IsPositionBinLoading())
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_BinLoadingPosition);
                Log.Write(this, "OutputStage Not Ready for Loading");
                bRtn = false;
                return bRtn;
            }
            return bRtn;
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

            if (!feederSensor && feederObj == null)
            {
                Log.Write(UnitName, "[Unload] Wafer missing on feeder after transfer");
                PostAlarm((int)AlarmKeys.Alarm_BinUnloadingFailed);
                return -1;
            }
            if (feederSensor && feederObj == null)
            {
                Log.Write(UnitName, "[Unload] Feeder ring detected but object null");
                PostAlarm((int)AlarmKeys.Alarm_BinUnloadingFailed);
                return -1;
            }
            if (feederObj != null && !feederSensor)
            {
                Log.Write(UnitName, "[Unload] Wafer object exists but feeder sensor off");
                PostAlarm((int)AlarmKeys.Alarm_BinUnloadingFailed);
                return -1;
            }
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

            if (!feederSensor && feederObj == null)
            {
                Log.Write(UnitName, $"[Unload] Completed feeder empty OK (Slot:{slotIndex})");
                return 0;
            }
            if (!feederSensor && feederObj != null)
            {
                Log.Write(UnitName, "[Unload] Object remained although sensor off -> force clear");
                SetMaterial(null);
                return 0;
            }
            if (feederSensor && feederObj == null)
            {
                Log.Write(UnitName, "[Unload] Sensor ON but no wafer object -> lost?");
                PostAlarm((int)AlarmKeys.Alarm_BinUnloadingFailed);
                return -1;
            }
            if (feederSensor && feederObj != null)
            {
                Log.Write(UnitName, "[Unload] Feeder still holds wafer after unload-to-cassette step");
                PostAlarm((int)AlarmKeys.Alarm_BinUnloadingFailed);
                return -1;
            }
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

                // 장비 전부 정지하고 싶어.
                // 장비 전체 정지 함수 불어와줘.
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
                if(RunMode == UnitRunMode.Auto)
                {
                    if (IsInterlockOKBinLoading() == false)
                    {
                        AxisOutputFeederY.EmgStop();
                        PostAlarm((int)AlarmKeys.Alarm_BinLoadingFailed);
                        return -1;
                    }
                }

                IsMoveInterLockReady();

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
                if (IsInterlockOKMoveToCassette() == false)
                {
                    AxisOutputFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_BinLoadingFailed);
                    return -1;
                }

                IsMoveInterLockCassette();
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
            return MoveTeachingPositionOnce((int)OutputFeederConfig.TeachingPositionName.Cassette, isFine);
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
        public bool IsFeederUp()
        {
            if(Config.IsSimulation)
            {
                return true;
            }
            return this.ReadInput(OutputFeederConfig.IO.FEEDER_UP);
        }
        public bool IsFeederDown()
        {
            if (Config.IsSimulation)
            {
                return true;
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

            // OutputFeeder엔 Unclamp 전용 알람 키가 없어 Clamp 실패 알람을 공용 사용
            PostAlarm((int)AlarmKeys.Alarm_GripperClampFailed);
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

            // Up 실패는 FeederClampUp, Down 실패는 BinLoadingFailed로 처리(기존 로직과 동일한 의미)
            int alarm = expectUp
                ? (int)AlarmKeys.Alarm_FeederClampUpDown
                : (int)AlarmKeys.Alarm_BinLoadingFailed;

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
            //this.State = ProcessState.Stop;
            _exchangeStandbyForNextLoad = false;

            base.OnStop();
            return ret;
        }
        protected override int OnRunReady()
        {
            int nRet = 0;
            MaterialWafer BinStage = this.OutputStage.GetMaterialWafer();
            try
            {
                // Stage Wafer 작업 완료 시 true임.
                if (this.OutputStage.IsWorking())
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
                            else
                            {
                                NeedUnloadFirst = false;
                            }
                        }
                        else
                        {
                            //여기에서 BinStage Data가 없는 경우에 InputStage에서 
                            //맵데이터를 기다리고 있는 중인 경우.
                            // 여기 오기전에 최소한 BinStage에 제품이 있으니깐..
                            // 제품 가지고 Ready 신호는 가지고 있어야 하지 않나.
                            // Feeder에서 Wafer로 Data를 넘겨야 하는데... 
                            // Feeder가 아직 Data를 가지고 있다.. 음..
                            // BinWafer Data만 1차로 넘기고
                            // 2차로 InputStage Data를 받고 진행하는 걸로 하자.


                            NeedUnloadFirst = false;
                        }
                    }
                    else
                    {
                        // 시뮬/드라이런: 데이터 기반 판단
                        NeedUnloadFirst = (BinStage != null && BinStage.SlotIndex != -1);
                    }
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

            MaterialWafer wafer = this.OutputStage.GetMaterialWafer();
            // 0) Stage에 제품이 있으면 "언로딩 먼저"
            if (NeedUnloadFirst)
            {
                nRet = BinUnloading(true);
                if (nRet != 0)
                {
                    Log.Write(UnitName, "OnRunWork", "BinUnloading Failed");
                    return nRet;
                }
                NeedUnloadFirst = false;
                if(IsStop)
                {
                    Log.Write(UnitName, "OnRunWork", "IsStop-BinUnloading");
                    return 0;
                }

                NeedUnloadFirst = false;
                this.State = ProcessState.Complete;
                return 0;
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
                        PostAlarm((int)AlarmKeys.Alarm_BinLoadingFailed);
                        return nRet;
                    }
                    if (IsStop)
                    {
                        Log.Write(UnitName, "OnRunWork", "IsScanCompleted");
                        return 0;
                    }
                }

                if (this.OutputCassetteLifter.IsHaveMoreProcessWafer() == true)
                {
                    nRet = BinLoading(true);
                    if (nRet != 0)
                    {
                        AxisOutputFeederY.EmgStop();
                        PostAlarm((int)AlarmKeys.Alarm_BinLoadingFailed);
                        this.State = ProcessState.Error;
                        return nRet;
                    }
                    if (IsStop)
                    {
                        Log.Write(UnitName, "OnRunWork", "IsScanCompleted");
                        return 0;
                    }

                    this.State = ProcessState.Complete;
                    Log.Write(UnitName, "OnRunWork", "LoadFlowStep.StageLoadingAfter completed.");
                }
                else
                {
                    if (!IsPositionReady())
                    {
                        nRet = MoveToReady();
                        if (nRet != 0)
                        {
                            AxisOutputFeederY.EmgStop();
                            PostAlarm((int)AlarmKeys.Alarm_BinLoadingFailed);
                            this.State = ProcessState.Error;
                            return nRet;
                        }
                    }

                    // [ADD] Input/Output Cassette 모두 소진 되었는지 확인 후 언로딩 + 장비 정지
                    TryShutdownIfAllCassettesEmpty();

                    //카세트 교체 알람 발생.
                    // ← 추가: 전 슬롯 완료되었는지 검사하여 1회 알람
                    //try
                    //{
                    //    nRet = this.OutputCassetteLifter.CheckCassetteCompletedAndAlarmOnce();
                    //    if (nRet != 0)
                    //    {
                    //        this.Stop();
                    //        OutputCassetteLifter.Stop();
                    //        OutputStage.Stop();
                    //        return 0;
                    //    }
                    //}
                    //catch (Exception ex)
                    //{
                    //    Log.Write(ex);
                    //}
                }
            }
            
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
                        if (srcWafer == null || srcWafer.Dies == null || srcWafer.Dies.Count == 0)
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
            var Bin = GetMaterial() as MaterialWafer;
            // 픽업 직후 재선택 방지: Processing 전환 + SlotIndex 보정 + 경로 준비
            if (Bin != null)
            {
                Bin.Presence = Material.MaterialPresence.Exist;
                Bin.ProcessSatate = Material.MaterialProcessSatate.Ready;
                lock (Bin.Dies)
                {
                    if (Bin.Dies == null || Bin.Dies.Count == 0)
                    {
                        // 이 안이 핵심. InputStage Wafer Data도 여기서 가져옴.
                        MakePath();
                    }
                }
            }
            OutputStage?.UpdateUI();


            var waferOnFeeder2 = this.GetMaterial() as MaterialWafer;
            if (waferOnFeeder2 == null)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_BinLoadingFailed);
                Log.Write(this, "No wafer on Feeder to move to OutputStage");
                return -1;
            }
            this.MoveMaterial(waferOnFeeder2, OutputStage);
            waferOnFeeder2.ProcessSatate = Material.MaterialProcessSatate.Ready;
            OutputStage.SetMaterial(waferOnFeeder2);
            this.SetMaterial(null);

            Bin = OutputStage?.GetMaterialWafer();
            Bin.ProcessSatate = Material.MaterialProcessSatate.Processing;
            OutputStage?.SetMaterial(Bin);

            // 웨이퍼 로딩 확정 시 요약 시작
            var waferOnStage = InputStage?.GetMaterialWafer();
            //VA1VPRO16
            Equipment.Instance.ResultWriterManager.BeginWaferSummary(waferOnStage?.WaferId, "VA1VPRO16");

            Log.Write(UnitName, "OnRunWork", "LoadFlowStep.BinStageMapping completed.");
            return nRet;
        }

        protected override int OnRunComplete()
        {
            int ret = 0;
            this.State = ProcessState.Ready;
            Log.Write(UnitName, "OnRunComplete", "OnRunComplete Ok");
            return ret;
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

            //string strBarcode = string.Empty;
            nRet = GetBarcode(out strBarcode);
            {
                var c = this.OutputCassetteLifter.GetMaterialCassette();
                int nIndex = this.OutputCassetteLifter.GetCurrectSlotID();
                MaterialWafer Bin = c.GetWafer(nIndex);

                // 캐리어 정보만 보전하고, 상태는 Ready 유지 (Processing으로 올리지 않음)
                Bin.CarrierId = c.CarrierId;
                if (Config.IsSimulation
                    || Config.IsDryRun)
                {
                    strBarcode = string.Format("{0}_{0}", strBarcode, Bin.CarrierId);
                }
                else
                {
                    Bin.WaferId = strBarcode;
                }
                this.SetMaterial(Bin);
                Log.Write(UnitName, "WaferLoading", strBarcode);
            }
            Log.Write(UnitName, "BinLoading Complete");
            return nRet;
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
                PostAlarm((int)AlarmKeys.Alarm_BinLoadingFailed);
                nRet = -1;
                return nRet;
            }

            nRet = UpFeeder();
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_BinLoadingFailed);
                nRet = -1;
                return nRet;
            }
            Log.Write(this, "MoveToReay End");


            return nRet;
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
            nRet = MovePositionCassette(isFine);
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                Log.Write(UnitName, "UnloadOnlyFeederToCassette", "UnloadOnlyFeederToCassette Fail - MovePositionCassette");
                return -1;
            }

            nRet = UnClampGripper();
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                Log.Write(UnitName, "UnloadOnlyFeederToCassette", "UnloadOnlyFeederToCassette Fail - UnClampGripper");
                return -1;
            }

            // Feeder -> Cassette: 데이터 돌려놓기
            var waferOnFeeder = this.GetMaterial() as MaterialWafer;
            if (waferOnFeeder != null && waferOnFeeder.SlotIndex >= 0)
            {
                var cassette = this.OutputCassetteLifter.GetMaterialCassette();
                waferOnFeeder.ProcessSatate = Material.MaterialProcessSatate.Completed;
                waferOnFeeder.Presence = Material.MaterialPresence.Exist;
                cassette.SetWafer(waferOnFeeder.SlotIndex, waferOnFeeder);
                SetMaterial(null);
            }
            else
            {
                Log.Write(UnitName, "UnloadOnlyFeederToCassette", "UnloadOnly: Feeder has no wafer or invalid SlotIndex");
            }

            // 회피 = 바코드 위치 대기
            nRet = MovePositionBarcode(isFine);
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                Log.Write(UnitName, "UnloadOnlyFeederToCassette", "UnloadOnlyFeederToCassette Fail - MovePositionBarcode");
                return -1;
            }

            // 다음 로딩 가능 여부에 따라 대기 위치 결정
            bool hasNext = false;
            try { hasNext = OutputCassetteLifter != null && OutputCassetteLifter.IsHaveMoreProcessWafer(); }
            catch { hasNext = false; }

            if (hasNext)
            {
                // 교환(연속 로딩) 최적화: 바코드에서 대기
                nRet = MovePositionBarcode(isFine);
                if (nRet != 0)
                {
                    AxisOutputFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                    Log.Write(UnitName, "UnloadOnlyFeederToCassette", "UnloadOnlyFeederToCassette Fail - MovePositionBarcode");
                    return -1;
                }
                _exchangeStandbyForNextLoad = true;  // 다음 로딩을 Barcode에서 시작
            }
            else
            {
                // 더 가져올 Wafer 없음: Ready에서 대기
                nRet = MoveToReady(isFine);
                if (nRet != 0)
                {
                    AxisOutputFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_BinLoadingFailed);
                    Log.Write(UnitName, "UnloadOnlyFeederToCassette", "Fail - MoveToReady");
                    return -1;
                }
                _exchangeStandbyForNextLoad = false;
            }

            // Feeder material 정리
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
                    PostAlarm((int)AlarmKeys.Alarm_BinLoadingFailed);
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
                PostAlarm((int)AlarmKeys.Alarm_BinLoadingFailed);
                Log.Write(this, "MovePositionCassette Failed");
                nRet = -1;
                return nRet;
            }

            nRet = ClampGripper();
            if (nRet != 0)
            {
                PostAlarm((int)AlarmKeys.Alarm_BinLoadingFailed);
                Log.Write(this, "ClampGripper Failed");
                nRet = -1;
                return nRet;
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
            if (IsInterlockOKWaferLoading() == false)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_BinLoadingFailed);
                Log.Write(this, "Not IsInterlockOKWaferLoading");
                nRet = -1;
                return nRet;
            }
            nRet = base.MoveTeachingPositionOnce((int)OutputFeederConfig.TeachingPositionName.Cassette, isFine);
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_BinLoadingFailed);
                Log.Write(this, "MoveTeachingPositionOnce Failed");
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
                PostAlarm((int)AlarmKeys.Alarm_BarcodeReadingFailed);
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
                && (Config.IsSimulation || Config.IsDryRun))
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
                IsPositionReady() == false)
            {
                PostAlarm((int)AlarmKeys.Alarm_OutputFeederNoPosition);
                Log.Write(UnitName, "OnEnsureReady Fail - No Position");
                return -1;
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

            // Already at Ready → ensure Up/Unclamp then OK
            if (IsPositionReady())
            {
                if (IsUnClamped() == false)
                {
                    nRet = UnClampGripper();
                    if (nRet != 0)
                    {
                        Log.Write(UnitName, "CheckReady Fail - UnClampGripper");
                        return nRet;
                    }
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

            // At other TP → safety checks then move Ready
            if (!IsInterlockOKWithCassete())
            {
                PostAlarm((int)AlarmKeys.Alarm_OutputFeederInterlockFailed);
                Log.Write(UnitName, "CheckReady Fail - IsInterlockOKWithCassete");
                return -1;
            }

            bool stageAtSafe = (OutputStage == null) ||
                               OutputStage.IsPositionBinLoading() ||
                               OutputStage.IsPositionBinUnloading();
            if (!stageAtSafe)
            {
                PostAlarm((int)AlarmKeys.Alarm_OutputStageInterlockFailed);
                Log.Write(UnitName, "CheckReady Fail - OutputStage not at BinLoading/Unloading");
                return -1;
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
            _isSafetyMoving = false;
            CurrentFunc = null;
            NeedUnloadFirst = false;
            _exchangeStandbyForNextLoad = false;
            UnitDryRunTest = false;

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
        //public BinMapOrigin OutputBinOrigin { get; set; } = BinMapOrigin.BottomLeft; // InputStage와 보통 동일
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
        // [변경] MakePath 본문 교체
        public int MakePath_Cal()
        {
            int nRet = 0;
            var wafer = this.GetMaterial() as MaterialWafer;
            if (wafer == null) 
                return nRet;

            bool needPath = (wafer.Dies == null || wafer.Dies.Count == 0);
            if (!(wafer.ProcessSatate == Material.MaterialProcessSatate.Ready
                  || wafer.ProcessSatate == Material.MaterialProcessSatate.Processing))
            {
                return nRet;
            }
            if (!needPath) 
                return nRet;
            lock (wafer.Dies)
            {
                if (wafer.Dies != null)
                    wafer.Dies.Clear();

                wafer.Dies = new List<MaterialDie>();
                try
                {
                    var recipe = Equipment.Instance.EquipmentRecipe.CurrentRecipe;

                    int cntX = 5;// recipe.BinCountX > 0 ? recipe.BinCountX : 1;
                    int cntY = 5;// recipe.BinCountY > 0 ? recipe.BinCountY : 1;

                    double centerX = (cntX - 1) / 2.0;
                    double centerY = (cntY - 1) / 2.0;

                    int index = 0;

                    // 기준 그리드(gx, gy)를 원점/반전 설정에 따라 (bx, by)로 투영
                    for (int gy = 0; gy < cntY; gy++)
                    {
                        for (int gx = 0; gx < cntX; gx++)
                        {
                            int bx, by;
                            ToBinCoord(gx, gy, cntX, cntY, out bx, out by);

                            double mapX = bx - centerX;
                            double mapY = by - centerY;

                            var die = new MaterialDie
                            {
                                Index = index++,
                                Presence = Material.MaterialPresence.NotExist,
                                ProcessSatate = Material.MaterialProcessSatate.Unknown,

                                BinX = bx,
                                BinY = by,

                                MapX = (int)mapX,
                                MapY = (int)mapY
                            };
                            wafer.Dies.Add(die);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Write(UnitName, "MakePath", "Exception: " + ex.Message);
                }
            }

            return nRet;
        }
        // InputStage 좌표 그대로 복제 + 180도 회전 (레시피 BinCount 미사용)
        // rotate180=true 고정 요구 조건에 맞춰 구현. mirror/스왑 옵션 확장 가능.
        // InputStage 좌표 그대로 복제 + 중심 기준 180도 회전
        // rotate180=true: (x,y) → (2*centerX - x, 2*centerY - y)
        private int CopyInputMapRotate180(MaterialWafer srcWafer, MaterialWafer dstWafer, bool rotate180 = true)
        {
            try
            {

               
                if (srcWafer == null || srcWafer.Dies == null || srcWafer.Dies.Count == 0)
                    return -1;
                if (dstWafer == null)
                    return -2;
                lock (srcWafer.Dies)
                {
                    lock (dstWafer.Dies)
                    {
                        List<MaterialDie> MyDies = srcWafer.Dies.ToList();


                        const double tol = 1e-6;
                        // 원본 좌표 집합(유일 값)
                        List<double> xs = MyDies
                            .Where(d => d != null)
                            .Select(d => d.MapX)
                            .OrderBy(v => v)
                            .Aggregate(new List<double>(), (acc, v) =>
                            {
                                if (acc.Count == 0 || Math.Abs(acc[acc.Count - 1] - v) > tol)
                                    acc.Add(v);
                                return acc;
                            });

                        List<double> ys = MyDies
                            .Where(d => d != null)
                            .Select(d => d.MapY)
                            .OrderBy(v => v)
                            .Aggregate(new List<double>(), (acc, v) =>
                            {
                                if (acc.Count == 0 || Math.Abs(acc[acc.Count - 1] - v) > tol)
                                    acc.Add(v);
                                return acc;
                            });

                        if (xs.Count == 0 || ys.Count == 0)
                            return -3;

                        int nx = xs.Count;
                        int ny = ys.Count;

                        double centerX = (xs[0] + xs[nx - 1]) / 2.0;
                        double centerY = (ys[0] + ys[ny - 1]) / 2.0;

                        int FindIndex(List<double> list, double value)
                        {
                            // tolerance 검색
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

                        if (dstWafer.Dies != null)
                            dstWafer.Dies.Clear();

                        dstWafer.Dies = new List<MaterialDie>(MyDies.Count);

                        int newIndex = 0;
                        foreach (var s in MyDies)
                        {
                            if (s == null)
                                continue;

                            int ix = FindIndex(xs, s.MapX);
                            int iy = FindIndex(ys, s.MapY);
                            if (ix < 0 || iy < 0)
                                continue;

                            // Bin 인덱스 회전(행/열 반전)
                            int rx = rotate180 ? (nx - 1 - ix) : ix;
                            int ry = rotate180 ? (ny - 1 - iy) : iy;

                            // 중심 기준 180° 회전된 실제 좌표
                            double newMapX, newMapY;
                            //if (rotate180)
                            //{
                            //    double relX = s.MapX - centerX;
                            //    double relY = s.MapY - centerY;
                            //    newMapX = centerX - relX; // = 2*centerX - s.MapX
                            //    newMapY = centerY - relY; // = 2*centerY - s.MapY
                            //}
                            //else
                            //{
                            //    newMapX = s.MapX;
                            //    newMapY = s.MapY;
                            //}

                            //기존 반전에서 X만 반전으로 수정
                            rx = ix;                 // X 인덱스 유지
                            ry = ny - 1 - iy;        // Y 반전
                            newMapX = s.MapX;        // X 좌표 유지
                            newMapY = 2 * centerY - s.MapY; // Y 좌표 반전
                            dstWafer.Dies.Add(new MaterialDie
                            {
                                Index = newIndex++,
                                Presence = Material.MaterialPresence.NotExist,
                                ProcessSatate = Material.MaterialProcessSatate.Unknown,
                                BinX = rx,
                                BinY = ry,
                                MapX = (int)newMapX,
                                MapY = (int)newMapY
                            });
                        }
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                Log.Write(UnitName, "CopyInputMapRotate180", ex.Message);
                return -9;
            }
        }

        public enum PathStartCorner
        {
            BottomLeft,
            BottomRight,
            TopLeft,
            TopRight
        }
        public enum PathPrimaryAxis
        {
            XFirst,  // 행 우선(가로 먼저 진행)
            YFirst   // 열 우선(세로 먼저 진행)
        }
        public enum PathTraversalMode
        {
            Raster,      // 래스터(항상 같은 방향으로 진행, 다음 행/열로 넘어갈 때 되돌아감)
            Serpentine   // 지그재그(행/열마다 진행 방향 반전)
        }
        // 기본값 예시: 좌하단 시작, X 먼저, 지그재그
        //public PathStartCorner StartCorner { get; set; } = PathStartCorner.BottomLeft;
        public PathStartCorner StartCorner { get; set; } = PathStartCorner.BottomRight;
        public PathPrimaryAxis PrimaryAxis { get; set; } = PathPrimaryAxis.XFirst;
        public PathTraversalMode Traversal { get; set; } = PathTraversalMode.Serpentine;

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
        public bool CloneRotate180ForBin { get; set; } = true;         // 복제 시 180° 회전 적용 여부

        // 경로/맵 관련 메서드 근처에 추가
        /// <summary>
        /// 현재 설정(StartCorner/PrimaryAxis/Traversal)에 따라 Dies 순서를 재정렬합니다.
        /// - 격자 인덱스 키는 BinX/BinY의 정수 반올림값을 사용
        /// - 누락 셀은 건너뜁니다.
        /// - 정렬 후 Index는 호출부에서 재부여하세요.
        /// </summary>
        private void OrderDiesByMode(MaterialWafer wafer)
        {
            if (wafer?.Dies == null || wafer.Dies.Count == 0) return;

            // 격자 키를 한 번만 계산(정수 셀). 같은 셀 내 중복 다이도 보존.
            lock (wafer.Dies)
            {
                var items = wafer.Dies
                .Select(d => new { Die = d, BX = (int)Math.Round(d.BinX), BY = (int)Math.Round(d.BinY) })
                .ToList();

                var xs = items.Select(i => i.BX).Distinct().OrderBy(v => v).ToList();
                var ys = items.Select(i => i.BY).Distinct().OrderBy(v => v).ToList();
                if (xs.Count == 0 || ys.Count == 0) return;

                // (bx,by) → 동일 셀의 다이 목록(원래 순서/Index 유지)
                var buckets = new Dictionary<(int bx, int by), List<MaterialDie>>();
                foreach (var it in items)
                {
                    var key = (it.BX, it.BY);
                    if (!buckets.TryGetValue(key, out var list))
                    {
                        list = new List<MaterialDie>();
                        buckets[key] = list;
                    }
                    list.Add(it.Die);
                }

                // StartCorner에 따른 기본 진행 방향 (외부 설정 사용: 강제 덮어쓰기 제거)
                List<int> xBase, yBase;
                switch (StartCorner)
                {
                    default:
                    case PathStartCorner.BottomLeft:
                        xBase = xs;                             // L → R
                        yBase = ys;                             // Bottom → Top
                        break;
                    case PathStartCorner.BottomRight:
                        xBase = xs.AsEnumerable().Reverse().ToList();  // R → L
                        yBase = ys;                                    // Bottom → Top
                        break;
                    case PathStartCorner.TopLeft:
                        xBase = xs;                                    // L → R
                        yBase = ys.AsEnumerable().Reverse().ToList();  // Top → Bottom
                        break;
                    case PathStartCorner.TopRight:
                        xBase = xs.AsEnumerable().Reverse().ToList();  // R → L
                        yBase = ys.AsEnumerable().Reverse().ToList();  // Top → Bottom
                        break;
                }

                var newList = new List<MaterialDie>(wafer.Dies.Count);

                if (PrimaryAxis == PathPrimaryAxis.XFirst)
                {
                    // 행 우선: Y 바깥 루프, X 안쪽 루프
                    for (int row = 0; row < yBase.Count; row++)
                    {
                        int by = yBase[row];
                        IEnumerable<int> xSeq = xBase;
                        if (Traversal == PathTraversalMode.Serpentine && (row % 2 == 1))
                            xSeq = xBase.AsEnumerable().Reverse();

                        foreach (int bx in xSeq)
                        {
                            if (buckets.TryGetValue((bx, by), out var list))
                            {
                                // 같은 셀 내에서는 기존 Index 오름차순(원래 순서) 유지
                                newList.AddRange(list.OrderBy(d => d.Index));
                            }
                        }
                    }
                }
                else // YFirst (열 우선)
                {
                    for (int col = 0; col < xBase.Count; col++)
                    {
                        int bx = xBase[col];
                        IEnumerable<int> ySeq = yBase;
                        if (Traversal == PathTraversalMode.Serpentine && (col % 2 == 1))
                            ySeq = yBase.AsEnumerable().Reverse();

                        foreach (int by in ySeq)
                        {
                            if (buckets.TryGetValue((bx, by), out var list))
                            {
                                newList.AddRange(list.OrderBy(d => d.Index));
                            }
                        }
                    }
                }

                // 다이 정보는 변경하지 않고, 리스트 순서만 교체
                wafer.Dies = newList;
            }
        }

        private bool TryCloneMapFromInputStage(MaterialWafer dstWafer)
        {
            try
            {
                var inputStage = Equipment.Instance.GetUnit("InputStage") as InputStage;
                var srcWafer = inputStage?.GetMaterialWafer();

                if (srcWafer == null || srcWafer.Dies == null || srcWafer.Dies.Count == 0)
                    return false;

                lock (srcWafer.Dies)
                {
                    lock (dstWafer.Dies)
                    {
                        // 기존에 구현된 복제 유틸 사용
                        // - CopyInputMapRotate180: MapX/Y 그리드 보존 + 필요 시 180도 회전
                        //   BinX/BinY는 회전 반영한 격자 인덱스로 재계산됨
                        if (dstWafer.Dies != null)
                            dstWafer.Dies.Clear();

                        dstWafer.Dies = new List<MaterialDie>(srcWafer.Dies.Count);

                        CloneRotate180ForBin = true;
                        int rc = CopyInputMapRotate180(srcWafer, dstWafer, rotate180: CloneRotate180ForBin);
                        if (rc != 0)
                        {
                            Log.Write(UnitName, "MakePath", $"Clone from InputStage failed rc={rc}");
                            return false;
                        }
                        // 경로 모드 적용: StartCorner/PrimaryAxis/Traversal 설정에 따라 정렬
                        OrderDiesByMode(dstWafer);

                        // Index 재연속화(안전)
                        for (int i = 0; i < dstWafer.Dies.Count; i++)
                            dstWafer.Dies[i].Index = i;

                        Log.Write(UnitName, "MakePath",
                            $"Cloned map from InputStage. Count={dstWafer.Dies.Count} Rotate180={CloneRotate180ForBin}");

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
                    switch (StartCorner)
                    {
                        default:
                        case PathStartCorner.BottomLeft: xStart = 0; yStart = 0; xDir = +1; yDir = +1; break;
                        case PathStartCorner.BottomRight: xStart = cntX - 1; yStart = 0; xDir = -1; yDir = +1; break;
                        case PathStartCorner.TopLeft: xStart = 0; yStart = cntY - 1; xDir = +1; yDir = -1; break;
                        case PathStartCorner.TopRight: xStart = cntX - 1; yStart = cntY - 1; xDir = -1; yDir = -1; break;
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

                        if (!inside) return;

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

                    if (PrimaryAxis == PathPrimaryAxis.XFirst)
                    {
                        for (int row = 0; row < cntY; row++)
                        {
                            int rawY = yLineForward[row];
                            var xSeq = (Traversal == PathTraversalMode.Serpentine && (row % 2 == 1))
                                ? xLineReverse
                                : xLineForward;

                            foreach (int rawX in xSeq)
                                tryAdd(rawX, rawY);
                        }
                    }
                    else
                    {
                        for (int col = 0; col < cntX; col++)
                        {
                            int rawX = xLineForward[col];
                            var ySeq = (Traversal == PathTraversalMode.Serpentine && (col % 2 == 1))
                                ? yLineReverse
                                : yLineForward;

                            foreach (int rawY in ySeq)
                                tryAdd(rawX, rawY);
                        }
                    }

                    for (int i = 0; i < list.Count; i++)
                        list[i].Index = i;

                    Bin.Dies.AddRange(list);

                    Log.Write(UnitName, "MakePath",
                        $"Circular(Fallback)={UseCircularBinMap} Dies={Bin.Dies.Count} Grid=({cntX}x{cntY}) Pitch=({pitchX:F3},{pitchY:F3})mm Radius={radiusMm:F3}mm");
                }
                catch (Exception ex)
                {
                    Log.Write(UnitName, "MakePath", "Exception: " + ex.Message);
                }

            }
            return nRet;
        }
    }
}
