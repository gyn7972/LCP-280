using QMC.Common;
using QMC.Common.Alarm;
using QMC.Common.Cameras.HIKVISION;
using QMC.Common.Component;
using QMC.Common.IOUtil;
using QMC.Common.Motion;
using QMC.Common.Motions;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Component;
using System; // added for Obsolete attribute
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static QMC.LCP_280.Process.Equipment;

namespace QMC.LCP_280.Process.Unit
{
    public class OutputStage : BaseUnit<OutputStageConfig>
    {
        public enum AlarmKeys
        {
            eDieTransferPlaceZNotSafe = 3001,
            eOutputFeederCylinderZNotSafe,
            eOutputFeederYNotSafe,
            eNoBinDetected,
            eClampFB,
            eClampLift,
            ePlate,
        }

        #region InitAlarm
        protected override void InitAlarm()
        {
            base.InitAlarm();
            AlarmInfo alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eDieTransferPlaceZNotSafe;
            alarm.Title = "Die TrZAxis Not Sfarety Pos.";
            alarm.Cause = "Die Transfer Z-Axis가 안전 위치가 아닙니다.\n 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Warning.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eOutputFeederCylinderZNotSafe;
            alarm.Title = "Feeder Z-Cylinder Not Sfarety Pos.";
            alarm.Cause = "Feeder Z-Cylinder가 안전 위치가 아닙니다.\n 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Warning.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            //
            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eOutputFeederYNotSafe;
            alarm.Title = "Feeder Y-Axis Not Sfarety Pos.";
            alarm.Cause = "Feeder Y-Axis가 안전 위치가 아닙니다.\n 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Warning.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            //eNoBinDetected
            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eNoBinDetected;
            alarm.Title = "No Bin Detected";
            alarm.Cause = "Bin이 감지되지 않았습니다.\n Bin이 있는지 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Warning.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            //,
            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eClampFB;
            alarm.Title = "Clamp F/B Not Pos.";
            alarm.Cause = "Clamp F/B가 지정 위치가 아닙니다.\n 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Warning.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);
            //,
            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eClampLift;
            alarm.Title = "Clamp Lift Not Pos.";
            alarm.Cause = "Clamp Lift가 지정 위치가 아닙니다.\n 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Warning.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);
            //,
            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.ePlate;
            alarm.Title = "Plate Not Pos.";
            alarm.Cause = "Plate가 지정 위치가 아닙니다.\n 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Warning.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);


        }
        #endregion

        // OutStage camera
        public HIKGigECamera OutStageCamera { get; private set; }
        public string OutStageCameraKey { get; set; } = "Out_Stage";

        OutputDieTransfer OutputDieTransfer { get; set; }
        OutputFeeder OutputFeeder { get; set; }
        OutputCassetteLifter OutputCassetteLifter { get; set; }


        public OutputStage(OutputStageConfig config = null)
            : base(new OutputStageConfig())
        {
            AddComponents();
        }

        public override void AddComponents()
        {
            Config.LoadAndBindAxes(Equipment.Instance.AxisManager);
            Config.InitializeDefaultTeachingPositions();
            
            BindAxes();
            BindIoDomains();
            BindCamera();
        }

        protected override void OnBindUnit()
        {
            base.OnBindUnit();
            OutputFeeder = Equipment.Instance.GetUnit(UnitKeys.OutputFeeder) as OutputFeeder;
            OutputDieTransfer = Equipment.Instance.GetUnit(UnitKeys.OutputDieTransfer) as OutputDieTransfer;
            OutputCassetteLifter = Equipment.Instance.GetUnit(UnitKeys.OutputCassetteLifter) as OutputCassetteLifter;
        }

        private void BindCamera()
        {
            var eq = Equipment.Instance;
            if (eq == null) return;
            if (eq.Cameras != null && eq.Cameras.TryGetValue(OutStageCameraKey, out var cam))
                OutStageCamera = cam as HIKGigECamera;
            else
                OutStageCamera = eq.OutStageCam; // fallback
        }

        #region Axis Helpers
        private MotionAxis _axX, _axY, _axT;
        public MotionAxis AxisX => _axX;
        public MotionAxis AxisY => _axY;
        public MotionAxis AxisT => _axT;

        private void BindAxes()
        {
            var mgr = Equipment.Instance?.AxisManager;
            if (mgr == null)
            {
                Log.Write("OutputStage", "[BindAxes] AxisManager null");
                return;
            }

            const string unitName = "Unit"; // Equipment에서 축 등록 시 사용한 유닛명과 동일해야 함
            BindAxis(mgr, unitName, AxisNames.BinStageX, ref _axX);
            BindAxis(mgr, unitName, AxisNames.BinStageY, ref _axY);
            BindAxis(mgr, unitName, AxisNames.BinStageT, ref _axT);
        }
        public double GetTP(string tpName, string axisName)
        {
            var tp = Config.GetTeachingPosition(tpName);
            if (tp != null && tp.AxisPositions != null && tp.AxisPositions.TryGetValue(axisName, out var v)) return v;
            return 0.0;
        }
        public void MoveAxisOnce(MotionAxis ax, double target)
        {
            if (ax == null) return;
            if (System.Math.Abs(ax.GetPosition() - target) > ax.Config.InposTolerance * 3)
                ax.MoveAbs(target, ax.Config.MaxVelocity, ax.Config.RunAcc, ax.Config.RunDec, ax.Config.AccJerkPercent);
        }
        public bool InPos(MotionAxis ax, double target) => ax == null || ax.InPosition(target);
        public bool InPosTeaching(TeachingPosition tp)
        {
            if (tp == null)
                return false;
            return InPosTeaching(tp.Name);
        }
        public bool InPosTeaching(string name)
        {
            var (t, pz, plz) = Config.GetPositionWithOffset(name);
            return InPos(_axX, t) && InPos(_axY, pz) && InPos(_axT, plz);
        }
        #endregion

        #region IO Low-Level
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

        #region IO Domain Mapping (Reorganized)
        private Cylinder _cylClampLift;
        private Cylinder _cylClampFB;
        private Cylinder _cylPlate;
        private Vacuum _vacuum;

        private void BindIoDomains()
        {
            var eq = Equipment.Instance; var unit = eq?.UnitIO; if (unit == null) return;

            // Vacuum 별칭으로 조회만
            if (!IoAutoBindings.Vacuums.TryGetValue("OutStageVac", out _vacuum))
            {
                Log.Write("OutputStage", "BindIoDomains", "Vacuums not found: OutStageVac");
            }

            // Cylinder는 중앙 별칭으로 조회만
            if (!IoAutoBindings.Cylinders.TryGetValue("OutStagePlate", out _cylPlate))
            {
                Log.Write("OutputStage", "BindIoDomains", "Cylinder not found: OutStagePlate");
            }

            if (!IoAutoBindings.Cylinders.TryGetValue("OutStageLift", out _cylClampLift))
            {
                Log.Write("OutputStage", "BindIoDomains", "Cylinder not found: OutStageLift");
            }

            if (!IoAutoBindings.Cylinders.TryGetValue("OutStageClampFB", out _cylClampFB))
            {
                Log.Write("OutputStage", "BindIoDomains", "Cylinder not found: OutStageClampFB");
            }
        }
        private bool IsAtTeaching(OutputStageConfig.TeachingPositionName name)
        {
            // Config에 저장된 TeachingPosition 조회
            var tp = Config.GetTeachingPosition(name.ToString());
            if (tp == null || tp.AxisPositions == null || tp.AxisPositions.Count == 0)
                return false;

            // TeachingPosition에 포함된 각 축이 모두 In-Position인지 검사
            foreach (var kv in tp.AxisPositions)
            {
                var axisKey = kv.Key;
                var target = kv.Value;

                MotionAxis ax;
                if (!Axes.TryGetValue(axisKey, out ax) || ax == null)
                    return false;

                if (!InPos(ax, target))
                    return false;
            }
            return true;
        }

        // === Domain Control (표준 구동) ===
        public bool SetVacuum(bool on)
        {
            if (_vacuum == null) return false;
            if (on) _vacuum.On();
            else _vacuum.Off();
            return true;
        }

        public bool SetClampPlate(bool bUpDn)
        {
            if (_cylPlate == null)
                return false;

            if (bUpDn)
            {
                if (!IsAtTeaching(OutputStageConfig.TeachingPositionName.Loading) &&
                    !IsAtTeaching(OutputStageConfig.TeachingPositionName.Unloading))
                {
                    MessageBox.Show("SetClampPlate Interlock",
                              "Plate UP blocked: not at Loading/Unloading teaching position.",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
                return _cylPlate.Extend();
            }
            else
            {
                return _cylPlate.Retract();
            }
        }

        public bool SetClampLift(bool bUpDn)
        {
            if (_cylClampLift == null)
                return false;

            if (bUpDn)
            {
                return _cylClampLift.Extend();
            }
            else
            {
                if (!IsClampBwd())
                    return false; // 기존 인터락 유지

                return _cylClampLift.Retract();
            }
        }

        public bool SetClampFB(bool bFwdBwd)
        {
            if (_cylClampFB == null)
                return false;

            if (bFwdBwd)
            {
                if (!IsClampLiftUp())
                    return false; // 기존 인터락 유지

                return _cylClampFB.Extend();
            }
            else
            {
                if (!IsClampLiftUp())
                    return false; // 기존 인터락 유지

                return _cylClampFB.Retract();
            }
        }

        // --- Existing High-Level APIs (인터락 포함) ---
        public bool IsVacuum() => (_vacuum?.IsOk() ?? false) || ReadInput(OutputStageConfig.IO.VACUUM_CHECK);
        public bool IsPlateUp() => ReadInput(OutputStageConfig.IO.PLATE_UP);
        public bool IsPlateDown() => ReadInput(OutputStageConfig.IO.PLATE_DOWN);
        public bool IsClampLiftUp() => !IsClampLiftDown();
        public bool IsClampLiftDown() => ReadInput(OutputStageConfig.IO.CLAMP_DOWN_CHECK);
        public bool IsClampFwd() => ReadInput(OutputStageConfig.IO.CLAMP_FWD_CHECK);
        public bool IsClampBwd() => !IsClampFwd();
        public bool Ring0() => ReadInput(OutputStageConfig.IO.RING_CHECK0);
        public bool Ring1() => ReadInput(OutputStageConfig.IO.RING_CHECK1);
        

        // === Direct Valve Control (입력 신호/인터락 무관 강제 구동용) ===
        public bool IsVacuumValveOn() => IsOutputOn(OutputStageConfig.IO.VACUUM);
        public bool IsClampLiftUpValveOn() => IsOutputOn(OutputStageConfig.IO.CLAMP_UP);
        #endregion

        public bool IsStatus_RequestBin { get; set; }
        public bool IsStatus_StageLoadingReady { get; private set; }
        public bool IsStatus_StageLoadingDone { get; private set; }
        public bool IsStatus_StageUnloadingDone { get; private set; }
        public bool IsStatus_StageUnloadingReady { get; private set; }
        public bool IsStatus_CompleteWorking { get; internal set; }

        public MaterialWafer GetWaferMaterial()
        {
            throw new NotImplementedException();
        }


        // ================== Generic Single Axis Move (Safety Interlock 동일 구조) ==================
        /// <summary>
        /// 단일 축 이동 (Safety 인터락 포함). 이동 완료까지 블록.
        /// </summary>
        public int MoveAxisPositionOne(MotionAxis axis, double target, bool isFine = false)
        {
            if (axis == null) return -1;

            if (CheckMoveSafety(axis) != 0)
            {
                return -1;
            }

            Task<int> task = MoveAxisPositionOneAsync(axis, target, isFine);
            while (IsEndTask(task) == false)
            {
                // 동일 Safety Interlock
                if (!OutputDieTransfer.IsPickZSafetyPos())
                {
                    AxisX?.EmgStop(); AxisY?.EmgStop(); AxisT?.EmgStop();
                    PostAlarm((int)AlarmKeys.eDieTransferPlaceZNotSafe);
                    return -1;
                }
                if (!OutputFeeder.IsFeederZSafetyPosition())
                {
                    AxisX?.EmgStop(); AxisY?.EmgStop(); AxisT?.EmgStop();
                    PostAlarm((int)AlarmKeys.eOutputFeederCylinderZNotSafe);
                    return -1;
                }
                if (!OutputFeeder.IsFeederYSafetyPosition())
                {
                    AxisX?.EmgStop(); AxisY?.EmgStop(); AxisT?.EmgStop();
                    PostAlarm((int)AlarmKeys.eOutputFeederYNotSafe);
                    return -1;
                }

                Thread.Sleep(0);
            }
            return task.Result;
        }
        public int MoveApplyOffset(string positionName, double dx, double dy, double dt)
        {
            int nRtn = 0;
            // Teaching Position 가져오기
            var tp = Config.GetTeachingPosition(positionName);
            if (tp == null) 
                return -1;

            // 오프셋 적용
            Config.SetOffset(positionName, dx, dy, dt);
            var (x, y, t) = Config.GetPositionWithOffset(positionName);   //Offset 포함 위치 - Align 수행 시 data 있음.

            int rc = 0;
            if (AxisX != null) rc |= MoveAxisPositionOne(AxisX, x, false);
            if (AxisY != null) rc |= MoveAxisPositionOne(AxisY, y, false);
            if (AxisT != null) rc |= MoveAxisPositionOne(AxisT, t, false);
            if (rc != 0) 
                return -1;

            return 0;
        }
        
        /// //////////////////////////////////////////////////////////////////////////////////////////////
        // UI, sequence 용 Move 함수
        public int MoveTeachingPositionOnce(OutputStageConfig.TeachingPositionName name, bool isFine)
        {
            return MoveTeachingPositionOnce((int)name, isFine);
        }
        
        public int MoveToStageReadyPosition(bool isFine = false)
        {
            Task<int> task = MoveToStageReadyPositionAsync();
            while (IsEndTask(task) == false)
            {
                // Check Interlock.!!! 구문 넣을것.!!!
                // DieTransfer PickZ Safety
                if (!OutputDieTransfer.IsPickZSafetyPos())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eDieTransferPlaceZNotSafe);
                    return -1;
                }

                if (!OutputFeeder.IsFeederZSafetyPosition())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eOutputFeederCylinderZNotSafe);
                    return -1;
                }

                if (!OutputFeeder.IsFeederYSafetyPosition())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eOutputFeederYNotSafe);
                    return -1;
                }

                Thread.Sleep(0);
            }
            return task.Result;
        }
        public Task<int> MoveToStageReadyPositionAsync()
        {
            return Task.Run(() =>
            {
                OnMoveToStageReadyPosition();
                return 0;
            });
        }
        private int OnMoveToStageReadyPosition(bool isFine = false)
        {
            return MoveTeachingPositionOnce((int)OutputStageConfig.TeachingPositionName.Ready, isFine);
        }

        public int MoveToStageLoadPosition(bool isFine = false)
        {
            Task<int> task = MoveToStageLoadPositionAsync();
            while (IsEndTask(task) == false)
            {
                // Check Interlock.!!! 구문 넣을것.!!!
                // DieTransfer PickZ Safety
                if (!OutputDieTransfer.IsPlaceZSafetyPos())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eDieTransferPlaceZNotSafe);
                    return -1;
                }

                bool bSimulation = Config.IsSimulation;
                if(!bSimulation)
                {
                    if (!OutputFeeder.IsFeederZSafetyPosition())
                    {
                        this.AxisX.EmgStop();
                        this.AxisY.EmgStop();
                        this.AxisT.EmgStop();
                        PostAlarm((int)AlarmKeys.eOutputFeederCylinderZNotSafe);
                        return -1;
                    }

                }

                if (!OutputFeeder.IsFeederYSafetyPosition())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eOutputFeederYNotSafe);
                    return -1;
                }

                Thread.Sleep(0);
            }
            return task.Result;
        }
        public Task<int> MoveToStageLoadPositionAsync()
        {
            return Task.Run(() =>
            {
                OnMoveToStageLoadPosition();
                return 0;
            });
        }
        private int OnMoveToStageLoadPosition(bool isFine = false)
        {
            return MoveTeachingPositionOnce((int)OutputStageConfig.TeachingPositionName.Loading, isFine);
        }

        public int MoveToStageCenterPosition(bool isFine = false)
        {
            Task<int> task = MoveToStageCenterPositionAsync();
            while (IsEndTask(task) == false)
            {
                // DieTransfer PickZ Safety
                if (!OutputDieTransfer.IsPickZSafetyPos())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eDieTransferPlaceZNotSafe);
                    return -1;
                }

                if (!OutputFeeder.IsFeederZSafetyPosition())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eOutputFeederCylinderZNotSafe);
                    return -1;
                }

                if (!OutputFeeder.IsFeederYSafetyPosition())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eOutputFeederYNotSafe);
                    return -1;
                }

                Thread.Sleep(0);
            }
            return task.Result;
        }
        public Task<int> MoveToStageCenterPositionAsync()
        {
            return Task.Run(() =>
            {
                OnMoveToStageCenterPosition();
                return 0;
            });
        }
        private int OnMoveToStageCenterPosition(bool isFine = false)
        {
            return MoveTeachingPositionOnce((int)OutputStageConfig.TeachingPositionName.CenterPoint, isFine);
        }

        public int MoveToStageUnloadPosition(bool isFine = false)
        {
            Task<int> task = MoveToStageUnloadPositionAsync();
            while (IsEndTask(task) == false)
            {
                // Check Interlock.!!! 구문 넣을것.!!!
                // DieTransfer PickZ Safety
                if (!OutputDieTransfer.IsPickZSafetyPos())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eDieTransferPlaceZNotSafe);
                    return -1;
                }

                if (!OutputFeeder.IsFeederZSafetyPosition())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eOutputFeederCylinderZNotSafe);
                    return -1;
                }

                if (!OutputFeeder.IsFeederYSafetyPosition())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eOutputFeederYNotSafe);
                    return -1;
                }
                Thread.Sleep(0);
            }
            return task.Result;
        }
        public Task<int> MoveToStageUnloadPositionAsync(bool isFine = false)
        {
            return Task.Run(() =>
            {
                OnMoveToStageUnloadPosition(isFine);
                return 0;
            });
        }
        private int OnMoveToStageUnloadPosition(bool isFine = false)
        {
            return MoveTeachingPositionOnce((int)OutputStageConfig.TeachingPositionName.Unloading, isFine);
        }



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


        public override int OnRun() { int ret = 0; return ret; }
        public override int OnStop() { int ret = 0; base.OnStop(); return ret; }
        protected override int OnRunReady() { return 0; }
        protected override int OnRunWork() { return 0; }
        protected override int OnRunComplete() { return 0; }


        #region Seq 단위 동작 함수
        public int LoadingBinPrepare()
        {
            int nRtn = 0;

            Log.Write(this, "Start LoadingBinPrepare");
            IsStatus_StageLoadingReady = true;
            IsStatus_StageLoadingDone = false;

            // 이미 웨이퍼 존재하면 준비 단계 불필요 (바로 완료 단계 가능)
            if (Config.IsSimulation)
            {

            }
            else if (IsRingPresent())
            {
                Log.Write(UnitName, "LoadingPrep", "Bin already present -> Skip prepare");
                return nRtn;
            }

            // 로딩 Teaching 이동
            nRtn = MoveToStageLoadPosition();
            if (nRtn != 0)
            {
                Log.Write(this, "Fail: Move Load");
                return -1;
            }

            bool bSimulation = Config.IsSimulation;

            // Clamp Back → Lift Down
            SetClampFB(false);
            if (!IsClampBwd())
            {
                if(!bSimulation)
                {
                    PostAlarm((int)AlarmKeys.eClampFB);
                    Log.Write(this, "Fail: ClampBack");
                    return -1;
                }
            }

            SetClampLift(false);
            if (!IsClampLiftDown())
            {
                if (!bSimulation)
                {
                    PostAlarm((int)AlarmKeys.eClampLift);
                    Log.Write(this, "Fail: ClampLiftDown");
                    return -1;
                }
            }

            //Plate Down → 
            SetClampPlate(false);
            if (!IsPlateDown())
            {
                if (!bSimulation)
                {
                    PostAlarm((int)AlarmKeys.ePlate);
                    Log.Write(this, "Fail: PlateUp");
                    return -1;
                }
            }

            IsStatus_StageLoadingReady = true;
            Log.Write(UnitName, "LoadingPrep", "StageLoadingReady = TRUE (Wait wafer)");

            Log.Write(this, "End LoadingBinPrepare");
            return 0;
        }
        public int LoadingBinComplete()
        {
            int ret = 0;

            // 이미 완료
            if (IsStatus_StageLoadingDone)
                return 0;

            // 준비 안 되었으면 호출 순서 오류
            if (!IsStatus_StageLoadingReady && !IsRingPresent())
            {
                Log.Write(UnitName, "LoadingComp", "Not prepared (call LoadingBinComplete first)");
                return -1;
            }

            // 아직 Wafer 안 올라옴 → 대기
            bool bRtn = Config.IsSimulation;
            if (IsRingPresent() || bRtn)
            {
                Log.Write(UnitName, "LoadingComp", "Bin detected -> Completing");

                if (Config.IsSimulation)
                {
                    Thread.Sleep(1000);
                }
                else if (!IsPlateUp())
                {
                    SetClampPlate(true);
                    if (!IsPlateUp())
                    {
                        PostAlarm((int)AlarmKeys.ePlate);
                        Log.Write(this, "Fail: PlateUp");
                        return -1;
                    }

                    SetClampLift(true);
                    if (!IsClampLiftUp())
                    {
                        PostAlarm((int)AlarmKeys.eClampLift);
                        Log.Write(this, "Fail: ClampLiftUp");
                        return -1;
                    }

                    SetClampFB(true);
                    if (!IsClampFwd())
                    {
                        PostAlarm((int)AlarmKeys.eClampFB);
                        Log.Write(this, "Fail: ClampForward");
                        return -1;
                    }
                }
                else
                {
                    Log.Write(UnitName, "LoadingComp", "Not IsPlateUp");
                    return -1;
                }

                // 센터 Teaching 이동
                ret = MoveToStageCenterPosition();
                if (ret != 0)
                {
                    Log.Write(this, "Fail: Move Load");
                    return ret;
                }

                IsStatus_StageLoadingDone = true;
                IsStatus_StageLoadingReady = false;
                Log.Write(UnitName, "LoadingComp", "Done");

                return 0;
            }
            else
            {
                // 우선 대기? // 신호 이상?
                PostAlarm((int)AlarmKeys.eNoBinDetected);
                Log.Write(UnitName, "LoadingComp", "No Bin detected");
                return -1;
            }

            return ret;
        }

        public int UnloadingBinPrepare()
        {
            int nRtn = 0;
            Log.Write(UnitName, "UnloadingPrep", "Start");
            IsStatus_StageUnloadingDone = false;
            IsStatus_StageUnloadingReady = false;

            if (!IsRingPresent())
            {
                Log.Write(UnitName, "UnloadingPrep", "No Bin -> Skip");
                IsStatus_StageUnloadingDone = true;
                return 0;
            }

            nRtn = MoveToStageUnloadPosition();
            if (nRtn != 0)
            {
                return -1;
            }

            SetClampFB(false);
            if (!IsClampBwd())
            {
                PostAlarm((int)AlarmKeys.eClampFB);
                Log.Write(this, "Fail: ClampBack");
                return -1;
            }
            SetClampLift(false);
            if (!IsClampLiftDown())
            {
                PostAlarm((int)AlarmKeys.eClampLift);
                Log.Write(this, "Fail: ClampLiftDown");
                return -1;
            }
            SetClampPlate(false);
            if (!IsPlateDown())
            {
                PostAlarm((int)AlarmKeys.ePlate);
                Log.Write(this, "Fail: PlateUp");
                return -1;
            }

            IsStatus_StageUnloadingReady = true;
            Log.Write(UnitName, "UnloadingPrep", "StageUnloadingReady = TRUE (Wait wafer pick)");
            return 0;
        }

        public int UnloadingBinComplete()
        {
            int nRtn = 0;

            if (!IsStatus_StageUnloadingReady && IsRingPresent())
            {
                Log.Write(UnitName, "UnloadingComp", "Not prepared");
                return -1;
            }

            IsStatus_StageUnloadingDone = true;
            IsStatus_StageUnloadingReady = false;
            Log.Write(UnitName, "UnloadingComp", "Done");
            return nRtn;
        }

        public bool IsRingPresent()
        {
            bool bRtn = true;
            if (Config.IsSimulation)
            {
                return true;
            }
            else if (!Ring0() || !Ring1())
            {
                Log.Write(UnitName, "IsRingPresent", $"Ring not present (R0={Ring0()}, R1={Ring1()})");
                return false;
            }

            return bRtn;
        }
        public bool IsBinLoadingPosition()
        {
            var tp = TeachingPositions[(int)OutputStageConfig.TeachingPositionName.Loading];
            if (tp == null) 
                return false;
            return InPosTeaching(tp);
        }
        public bool IsBinUnloadingPosition()
        {
            var tp = TeachingPositions[(int)OutputStageConfig.TeachingPositionName.Unloading];
            if (tp == null) return false;
            return InPosTeaching(tp);
        }
        public bool IsBinCenterPosition()
        {
            var tp = TeachingPositions[(int)OutputStageConfig.TeachingPositionName.CenterPoint];
            if (tp == null) return false;
            return InPosTeaching(tp);
        }






        public int BinLoading()
        {
            int nRet = -1;
            /* TODO */
            return nRet;
        }

        public int ChipPlaceDown()
        {
            int nRet = -1;
            /* TODO */
            return nRet;
        }

        public int BinUnloading()
        {
            int nRet = -1;
            /* TODO */
            return nRet;
        }

        
        #endregion
    }
}