using QMC.Common;
using QMC.Common.Component;
using QMC.Common.IOUtil;
using QMC.Common.Motion;
using QMC.Common.Motions;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Component; // TeachingPosition
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace QMC.LCP_280.Process.Unit
{
    /// <summary>
    /// InputRingTransfer (Wafer Feeder / Ring Transfer Unit)
    ///  - X ?? ??? + Lift + Clamp (Ring ???? ??? / Overload ???)
    ///  - Teaching Position ???? (InputRingTransferConfig)
    ///  - Cylinder ??? ???? API (FeederUp/Down, Clamp)
    ///  - OutputStage / InputStage ?? ?????? Region/???? ????
    /// </summary>
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
        }
        #region Config / Teaching
        
        
        #endregion

        #region Axes
        private MotionAxis _feederY;
        public MotionAxis FeederX => _feederY;

        public InputStage InputStage { get; private set; }
        public InputCassetteLifter InputCassetteLifter { get; private set; }
        #endregion

        #region IO Domain Members
        private Cylinder _feederLift; // Up/Down
        private Cylinder _cylClamp;   // Clamp/Unclamp
        #endregion

        #region Constructor / Initialization
        public InputFeeder(InputFeederConfig config = null) : base(new InputFeederConfig())
        {
            
            AddComponents();
        }

        protected override void InitAlarm()
        {
            base.InitAlarm();
            AlarmRegister((int)AlarmKeys.Alarm_WaferLoadingFailed, "Wafer Loading Failed", "웨이퍼 로딩에 실패 하였습니다.","Error");
            AlarmRegister((int)AlarmKeys.Alarm_BarcodeReadingFailed, "Barcode Reading Failed", "바코드 읽기에 실패 하였습니다.\n바코드 상태를 확인 하여 주십시요", "Error");
            AlarmRegister((int)AlarmKeys.Alarm_StageLoadingFailed, "Stage Loading Failed", "스테이지 로딩에 실패 하였습니다.","Error");
            AlarmRegister((int)AlarmKeys.Alarm_StageUnloadingFailed, "Stage Unloading Failed", "스테이지 언로딩에 실패 하였습니다.","Error");
            AlarmRegister((int)AlarmKeys.Alarm_WaferUnloadingFailed, "Wafer Unloading Failed", "웨이퍼 언로딩에 실패 하였습니다.","Error");
            AlarmRegister((int)AlarmKeys.Alarm_InputStageInterlockFailed, "Input Stage Interlock Failed", "웨이퍼 로딩을 위한 인터락이 맞지 않습니다.\n장비 상태를 확인 하여 주십시요.", "Error");
            AlarmRegister((int)AlarmKeys.Alarm_GripperClampFailed, "Gripper Clamp Failed", "그리퍼 클램프에 실패 하였습니다.\n장비 상태를 확인 하여 주십시요.", "Error");




        }

        

        public override void AddComponents()
        {
            Config.LoadAndBindAxes(Equipment.Instance.AxisManager);
            Config.InitializeDefaultTeachingPositions();
            TeachingPositions.Clear();
            foreach (var tp in Config.TeachingPositions)
                TeachingPositions.Add(tp);
            BindAxes();
            BindIoDomains();
        }
        #endregion

        #region Runtime Hooks
        
        public override int OnStop() { int ret = 0; base.OnStop(); return ret; }
        #endregion

        #region Axis Binding
        private void BindAxes()
        {
            var mgr = Equipment.Instance?.AxisManager;
            if (mgr == null)
            {
                Log.Write("InputRingTransfer", "[BindAxes] AxisManager null");
                return;
            }

            const string unitName = "Unit"; // Equipment???? ?? ??? ?? ????? ?????? ??????? ??
            BindAxis(mgr, unitName, AxisNames.WaferFeederY, ref _feederY);
        }
        public void MoveAxisOnce(MotionAxis ax, double target)
        {
            if (ax == null) return;
            if (System.Math.Abs(ax.GetPosition() - target) > ax.Config.InposTolerance * 3)
                ax.MoveAbs(target, ax.Config.MaxVelocity, ax.Config.RunAcc, ax.Config.RunDec, ax.Config.AccJerkPercent);
        }
        public bool InPos(MotionAxis ax, double target) => ax == null || ax.InPosition(target);
        public double GetTP(string tpName, string axisName)
        {
            var tp = Config.GetTeachingPosition(tpName);
            if (tp != null && tp.AxisPositions != null && tp.AxisPositions.TryGetValue(axisName, out var v)) return v;
            return 0.0;
        }
        #endregion

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

        public bool InPosTeaching(string positionName)
        {
            var tp = Config.GetTeachingPosition(positionName);
            if (tp == null) return false;
            foreach (var kv in tp.AxisPositions)
                if (!Axes.TryGetValue(kv.Key, out var axis) || !InPos(axis, kv.Value)) return false;
            return true;
        }

        #endregion

        #region Low-Level IO (Read/Write by Name)
        public bool ReadInput(string name)
        {
            var hi = Config.HardInputs.FirstOrDefault(i => i.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
            if (hi == null) return false;
            var eq = Equipment.Instance; var dio = eq?.DioScan; if (dio == null) return false;
            foreach (var m in eq.UnitIO.Modules)
                if (dio.TryGetInput(m.ModuleName, hi.Disp, out var v)) return v;
            return false;
        }
        public bool WriteOutput(string name, bool on)
        {
            var ho = Config.HardOutputs.FirstOrDefault(o => o.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
            if (ho == null) return false;
            var eq = Equipment.Instance; var dio = eq?.DioScan; if (dio == null) return false;
            foreach (var m in eq.UnitIO.Modules)
                if (dio.WriteOutput(m.ModuleName, ho.Disp, on) == 0) return true;
            return false;
        }
        public bool IsOutputOn(string name)
        {
            var ho = Config.HardOutputs.FirstOrDefault(o => o.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
            if (ho == null) return false;
            var eq = Equipment.Instance; var dio = eq?.DioScan; if (dio == null) return false;
            foreach (var m in eq.UnitIO.Modules)
                if (dio.TryGetOutput(m.ModuleName, ho.Disp, out var v)) return v;
            return false;
        }
        #endregion

        #region IO Domain Mapping
        private void BindIoDomains()
        {
            var eq = Equipment.Instance; var unit = eq?.UnitIO; if (unit == null) return;

            if (!IoAutoBindings.Cylinders.TryGetValue("InFeederLift", out _feederLift))
            {
                Log.Write("InputRingTransfer", "BindIoDomains", "Cylinder not found: InFeederLift");
            }

            if (!IoAutoBindings.Cylinders.TryGetValue("InFeederClamp", out _cylClamp))
            {
                Log.Write("InputRingTransfer", "BindIoDomains", "Cylinder not found: InFeederClamp");
            }
        }
        #endregion

        // === Domain Control (??? ????) ===
        public bool SetLift(bool bUpDn)
        {
            if (_feederLift == null) return false;
            if (bUpDn) return _feederLift.Extend();
            else return _feederLift.Retract();
        }
        public bool SetClamp(bool bUpDn)
        {
            if (_cylClamp == null) return false;
            if (bUpDn) return _cylClamp.Extend();
            else return _cylClamp.Retract();
        }
        #region Status Helpers
        public bool IsFeederUp() => ReadInput(InputFeederConfig.IO.FEEDER_UP);
        public bool IsFeederDown() => ReadInput(InputFeederConfig.IO.FEEDER_DOWN);
        public bool IsUnclamped() => ReadInput(InputFeederConfig.IO.FEEDER_UNCLAMP);
        public bool IsRingPresent() => ReadInput(InputFeederConfig.IO.FEEDER_RING_CHECK);
        public bool IsOverload() => ReadInput(InputFeederConfig.IO.FEEDER_OVERLOAD);
        #endregion

        protected override void OnBindUnit()
        {
            base.OnBindUnit();
            InputStage = Equipment.Instance.GetUnit("InputStage") as InputStage;
            InputCassetteLifter = Equipment.Instance.GetUnit("InputCassetteLifter") as InputCassetteLifter;
        }
        /// ////////////////////////////////////////////////////////////////
        public override int OnRun()
        {
            int ret = 0;

            if (this.Status == UnitRunStatus.Stop || this.Status == UnitRunStatus.CycleStop)
            {
                this.State = ProcessState.Stop;
                return 1;
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

            return ret;
        }
        protected override  int OnRunReady()
        {
            int ret = 0;
            if(this.InputStage.IsRequestWafer && this.InputCassetteLifter.IsWaferReadyForUnloding)
            {
                this.State = ProcessState.Work;
            }
            return ret;
        }
        protected override int OnRunWork()
        {
            int ret = 0;
            //1. Wafer Loading
            ret = WaferLoading();
            if (ret != 0) 
            { 
                AlarmPost((int)AlarmKeys.Alarm_WaferLoadingFailed);
                this.State = ProcessState.Error; return ret; 
            }
            
            //3. Stage Loading
            ret = StageLoading();
            
            if (ret != 0) 
            { 
                AlarmPost((int)AlarmKeys.Alarm_StageLoadingFailed);
                this.State = ProcessState.Error; return ret;
            }
            //4. Stage Unloading
            ret = StageUnloading();
            
            if (ret != 0) 
            { 
                AlarmPost((int)AlarmKeys.Alarm_StageUnloadingFailed);
                this.State = ProcessState.Error; return ret;
            }
            //5. Wafer Unloading
            ret = WaferUnloading();
            
            if (ret != 0) 
            { 
                AlarmPost((int)AlarmKeys.Alarm_WaferUnloadingFailed);
                this.State = ProcessState.Error; return ret;
            }
            this.State = ProcessState.Complete;
            return ret;
        }
        #region === Direct Valve Control (??? ???/????? ???? ???? ??????) ===
        public bool IsFeederUpValveOn() => IsOutputOn(InputFeederConfig.IO.FEEDER_UP_VALVE);
        public bool IsFeederDownValveOn() => IsOutputOn(InputFeederConfig.IO.FEEDER_DOWN_VALVE);
        public bool IsFeederClampValveOn() => IsOutputOn(InputFeederConfig.IO.FEEDER_CLAMP_VALVE);
        public bool IsFeederUnclampValveOn() => IsOutputOn(InputFeederConfig.IO.FEEDER_UNCLAMP_VALVE);
        #endregion

        #region Seq ???? ???? ???
        public int WaferLoading(bool isFine = true)
        {
            int nRet = -1;
            nRet = MoveToReay(isFine);
            if (nRet != 0)
            {
                return nRet;
            }
            nRet = UnClampGripper();
            if (nRet != 0)
            {
                return nRet;
            }

            if (nRet != 0)
            {
                AlarmPost((int)AlarmKeys.Alarm_WaferLoadingFailed);
                return nRet;
            }


            nRet = BarcodeReading();
            if (nRet != 0)
            {
                AlarmPost((int)AlarmKeys.Alarm_BarcodeReadingFailed);
                return nRet;
            }

            return nRet;
        }
        public int ClampGripper()
        {
            int nRet = 0;
            if (this.SetClamp(true))
            {
                Log.Write("InputFeeder", "WaferLoading", "Clamp Success");
            }
            else
            {
                Log.Write("InputFeeder", "WaferLoading", "Clamp Failed");
                AlarmPost((int)AlarmKeys.Alarm_GripperClampFailed);
                nRet = -1;
                return nRet;
            }
            return nRet;
        }
        
        public int UnClampGripper()
        {
            int nRet = 0;
            if (this.SetClamp(false))
            {
                Log.Write("InputFeeder", "WaferLoading", "Unclamp Success");
            }
            else
            {
                Log.Write("InputFeeder", "WaferLoading", "Unclamp Failed");
                AlarmPost((int)AlarmKeys.Alarm_GripperClampFailed);
                nRet = -1;
                return nRet;
            }

            return nRet;
        }
        public int UpFeeder()
        {
            int nRet = 0;
            if (this.SetLift(true))
            {
                Log.Write("InputFeeder", "WaferLoading", "Feeder Up Success");
            }
            else
            {
                Log.Write("InputFeeder", "WaferLoading", "Feeder Up Failed");
                AlarmPost((int)AlarmKeys.Alarm_WaferLoadingFailed);
                nRet = -1;
                return nRet;
            }
            return nRet;
        }
        public int DownFeeder()
        {
            int nRet = 0;
            if (this.SetLift(false))
            {
                Log.Write("InputFeeder", "WaferLoading", "Feeder Down Success");
            }
            else
            {
                Log.Write("InputFeeder", "WaferLoading", "Feeder Down Failed");
                AlarmPost((int)AlarmKeys.Alarm_WaferLoadingFailed);
                nRet = -1;
                return nRet;
            }
            return nRet;
        }

        public int MoveToReay(bool isFine)
        {
            int nRet = 0;
            if (IsInterlockOKWaferLoading() == false)
            {
                AlarmPost((int)AlarmKeys.Alarm_WaferLoadingFailed);
                nRet = -1;
                return nRet;
            }
            nRet = base.MoveTeachingPositionOnce((int)InputFeederConfig.TeachingPositionName.Ready, isFine);
            if (nRet != 0)
            {
                AlarmPost((int)AlarmKeys.Alarm_WaferLoadingFailed);
                nRet = -1;
                return nRet;
            }
            return nRet;
        }

        public Task<int> MoveToReayAsync(bool isFine)
        {
            return Task.Run(() => MoveToReay(isFine));
        }

        public int MoveToCassette(bool isFine)
        {
            Task<int> task = MoveToCassetteAsync(isFine);
            while(IsEndTask(task) == false)
            {
                if(IsInterlockOKMoveToCassette() == false)
                {
                    foreach(var ax in Axes.Values)
                    {
                        ax.EmgStop();
                    }
                    AlarmPost((int)AlarmKeys.Alarm_WaferLoadingFailed);
                    return -1;
                }
                System.Threading.Thread.Sleep(1);
            }
            return task.Result;
        }

        private bool IsInterlockOKMoveToCassette()
        {
            bool isOK = this.InputStage.IsWaferLoadingPosition();
            isOK &= this.InputCassetteLifter.IsWaferReadyForLoading();
            return true;
        }

        protected int OnMoveToCassette(bool isFine)
        {
            int nRet = 0;
            if (IsInterlockOKWaferLoading() == false)
            {
                AlarmPost((int)AlarmKeys.Alarm_WaferLoadingFailed);
                nRet = -1;
                return nRet;
            }
            nRet = base.MoveTeachingPositionOnce((int)InputFeederConfig.TeachingPositionName.Cassette, isFine);
            if (nRet != 0)
            {
                AlarmPost((int)AlarmKeys.Alarm_WaferLoadingFailed);
                nRet = -1;
                return nRet;
            }
            return nRet;
        }
        public Task<int> MoveToCassetteAsync(bool isFine)
        {
            return Task.Run(() => OnMoveToCassette(isFine));
        }

        private bool IsInterlockOKWaferLoading()
        {
            throw new NotImplementedException();
        }

        public int BarcodeReading()
        {
            int nRet = -1;
            /* TODO */
            return nRet;
        }

        public int StageLoading()
        {
            int nRet = -1;
            /* TODO */
            return nRet;
        }

        public int StageUnloading()
        {
            int nRet = -1;
            /* TODO */
            return nRet;
        }

        public int WaferUnloading()
        {
            int nRet = -1;
            /* TODO */
            return nRet;
        }
        #endregion

    }
}