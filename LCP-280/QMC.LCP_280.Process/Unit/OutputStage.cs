using QMC.Common;
using QMC.Common.Alarm;
using QMC.Common.Cameras.HIKVISION;
using QMC.Common.Component;
using QMC.Common.IOUtil;
using QMC.Common.Motion;
using QMC.Common.Motions;
using QMC.Common.Unit;
using QMC.Common.VisionPart;
using QMC.LCP_280.Process.Component;
using System; // added for Obsolete attribute
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static QMC.LCP_280.Process.Equipment;

namespace QMC.LCP_280.Process.Unit
{
    public class OutputStage : BaseUnit<OutputStageConfig>
    {
        // 다이 배치 이벤트
        public sealed class DiePlacedEventArgs : EventArgs
        {
            public MaterialDie Die { get; }
            public double BinX { get; }
            public double BinY { get; }

            public DiePlacedEventArgs(MaterialDie die)
            {
                Die = die;
                if (die != null)
                {
                    BinX = die.BinX;
                    BinY = die.BinY;
                }
            }
        }
        public event EventHandler<DiePlacedEventArgs> DiePlaced;

        public delegate void UpdateUIWafer(MaterialWafer wafer);
        public event UpdateUIWafer EventUpdateUIWafer;

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
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eOutputFeederCylinderZNotSafe;
            alarm.Title = "Feeder Z-Cylinder Not Sfarety Pos.";
            alarm.Cause = "Feeder Z-Cylinder가 안전 위치가 아닙니다.\n 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            //
            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eOutputFeederYNotSafe;
            alarm.Title = "Feeder Y-Axis Not Sfarety Pos.";
            alarm.Cause = "Feeder Y-Axis가 안전 위치가 아닙니다.\n 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            //eNoBinDetected
            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eNoBinDetected;
            alarm.Title = "No Bin Detected";
            alarm.Cause = "Bin이 감지되지 않았습니다.\n Bin이 있는지 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            //,
            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eClampFB;
            alarm.Title = "Clamp F/B Not Pos.";
            alarm.Cause = "Clamp F/B가 지정 위치가 아닙니다.\n 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);
            //,
            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eClampLift;
            alarm.Title = "Clamp Lift Not Pos.";
            alarm.Cause = "Clamp Lift가 지정 위치가 아닙니다.\n 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);
            //,
            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.ePlate;
            alarm.Title = "Plate Not Pos.";
            alarm.Cause = "Plate가 지정 위치가 아닙니다.\n 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);


        }
        #endregion

        // OutStage camera
        public HIKGigECamera OutStageCamera { get; private set; }
        public string OutStageCameraKey { get; set; } = "Out_Stage";

        OutputDieTransfer OutputDieTransfer { get; set; }
        OutputFeeder OutputFeeder { get; set; }
        OutputCassetteLifter OutputCassetteLifter { get; set; }


        MaterialDie _currentDie = null;
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
        //public double GetTP(string tpName, string axisName)
        //{
        //    var tp = Config.GetTeachingPosition(tpName);
        //    if (tp != null && tp.AxisPositions != null && tp.AxisPositions.TryGetValue(axisName, out var v)) return v;
        //    return 0.0;
        //}

        //public bool InPos(MotionAxis ax, double target) => ax == null || ax.InPosition(target);
        //public bool InPosTeaching(TeachingPosition tp)
        //{
        //    if (tp == null)
        //        return false;
        //    return InPosTeaching(tp.Name);
        //}
        //public bool InPosTeaching(string name)
        //{
        //    var (t, pz, plz) = Config.GetPositionWithOffset(name);
        //    return InPos(_axX, t) && InPos(_axY, pz) && InPos(_axT, plz);
        //}
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
        public bool SetVacuum(bool on, bool bCheckSignal = false)
        {
            if (_vacuum == null)
                return false;

            if (!bCheckSignal)
            {
                if (on)
                    _vacuum.On();
                else
                    _vacuum.Off();
            }
            else
            {
                if (on)
                    _vacuum.OnWaitOk();
                else
                    _vacuum.OffWaitOk();
            }

            return true;
        }

        public bool SetClampPlate(bool bUpDn)
        {
            if (_cylPlate == null)
                return false;

            if (bUpDn)
            {
                //if (!IsAtTeaching(OutputStageConfig.TeachingPositionName.Loading) &&
                //    !IsAtTeaching(OutputStageConfig.TeachingPositionName.Unloading))
                //{
                //    MessageBox.Show("SetClampPlate Interlock",
                //              "Plate UP blocked: not at Loading/Unloading teaching position.",
                //              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //    return false;
                //}
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
                //if (!IsClampLiftUp())
                //    return false; // 기존 인터락 유지

                return _cylClampFB.Retract();
            }
        }

        // --- Existing High-Level APIs (인터락 포함) ---
        public bool IsVacuumOn()
        {
            if (Config.IsSimulation || Config.IsDryRun)
            {
                return true;
            }

            return this.ReadInput(OutputStageConfig.IO.VACUUM_CHECK);
        }
        public bool Ring0()
        {
            if (Config.IsSimulation || Config.IsDryRun)
            {
                return true;
            }

            return this.ReadInput(OutputStageConfig.IO.RING_CHECK0);
        }
        public bool Ring1()
        {
            if (Config.IsSimulation || Config.IsDryRun)
            {
                return true;
            }

            return this.ReadInput(OutputStageConfig.IO.RING_CHECK1);
        }
        public bool IsClampLiftUp()
        {
            if (Config.IsSimulation)
            {
                return true;
            }

            return !IsClampLiftDown(); 
        }
        public bool IsClampLiftDown()
        {
            if (Config.IsSimulation)
            {
                return true;
            }

            return this.ReadInput(OutputStageConfig.IO.CLAMP_DOWN_CHECK);
        }
        public bool IsClampFwd()
        {
            if (Config.IsSimulation)
            {
                return true;
            }

            return this.ReadInput(OutputStageConfig.IO.CLAMP_FWD_CHECK);
        }
        public bool IsClampBwd()
        {
            if (Config.IsSimulation)
            {
                return true;
            }

            return !IsClampFwd();
        }
        public bool IsPlateUp()
        {
            if (Config.IsSimulation)
            {
                return true;
            }

            return this.ReadInput(OutputStageConfig.IO.PLATE_UP);
        }
        public bool IsPlateDown()
        {
            if (Config.IsSimulation)
            {
                return true;
            }

            return this.ReadInput(OutputStageConfig.IO.PLATE_DOWN);
        }

        // === Cylinder 완료 대기 Helpers ===
        // Plate: expectUp=true(UP 기대), false(DOWN 기대)
        private int WaitPlateStateOrAlarm(bool expectUp, int timeoutMs = 1500, int pollMs = 2)
        {
            if (Config.IsSimulation || Config.IsDryRun)
                return 0;

            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds <= timeoutMs)
            {
                bool ok = expectUp ? IsPlateUp() : IsPlateDown();
                if (ok)
                    return 0;

                Thread.Sleep(pollMs);
            }

            PostAlarm((int)AlarmKeys.ePlate);
            Log.Write(UnitName, expectUp ? "[Plate] UP timeout" : "[Plate] DOWN timeout");
            return -1;
        }

        // ClampLift: expectUp=true(UP 기대), false(DOWN 기대)
        private int WaitClampLiftStateOrAlarm(bool expectUp, int timeoutMs = 1500, int pollMs = 2)
        {
            if (Config.IsSimulation || Config.IsDryRun)
                return 0;

            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds <= timeoutMs)
            {
                bool ok = expectUp ? IsClampLiftUp() : IsClampLiftDown();
                if (ok)
                    return 0;

                Thread.Sleep(pollMs);
            }

            PostAlarm((int)AlarmKeys.eClampLift);
            Log.Write(UnitName, expectUp ? "[ClampLift] UP timeout" : "[ClampLift] DOWN timeout");
            return -1;
        }

        // Clamp F/B: expectFwd=true(FWD 기대), false(BWD 기대)
        private int WaitClampFBStateOrAlarm(bool expectFwd, int timeoutMs = 1500, int pollMs = 2)
        {
            if (Config.IsSimulation || Config.IsDryRun)
                return 0;

            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds <= timeoutMs)
            {
                bool ok = expectFwd ? IsClampFwd() : IsClampBwd();
                if (ok)
                    return 0;

                Thread.Sleep(pollMs);
            }

            PostAlarm((int)AlarmKeys.eClampFB);
            Log.Write(UnitName, expectFwd ? "[ClampFB] FWD timeout" : "[ClampFB] BWD timeout");
            return -1;
        }

        // === Direct Valve Control (입력 신호/인터락 무관 강제 구동용) ===
        public bool IsVacuumValveOn()
        {
            if (Config.IsSimulation)
            {
                return true;
            }

            return this.IsOutputOn(OutputStageConfig.IO.VACUUM);
        }
        #endregion

        // ================== Generic Single Axis Move (Safety Interlock 동일 구조) ==================
        /// <summary>
        /// 단일 축 이동 (Safety 인터락 포함). 이동 완료까지 블록.
        /// </summary>
        public int MoveAxisPositionOne(MotionAxis axis, double target, bool isFine = false)
        {
            if (axis == null) return -1;

            Task<int> task = MoveAxisPositionOneAsync(axis, target, isFine);
            while (IsEndTask(task) == false)
            {
                // 동일 Safety Interlock
                if (!OutputDieTransfer.IsPositionPlaceZSafety())
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
                if (!OutputDieTransfer.IsPositionPickZSafety())
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
                if (!OutputDieTransfer.IsPositionPlaceZSafety())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eDieTransferPlaceZNotSafe);
                    return -1;
                }

                //if (!OutputFeeder.IsFeederZSafetyPosition())
                //{
                //    this.AxisX.EmgStop();
                //    this.AxisY.EmgStop();
                //    this.AxisT.EmgStop();
                //    PostAlarm((int)AlarmKeys.eOutputFeederCylinderZNotSafe);
                //    return -1;
                //}

                //if (!OutputFeeder.IsFeederYSafetyPosition())
                //{
                //    this.AxisX.EmgStop();
                //    this.AxisY.EmgStop();
                //    this.AxisT.EmgStop();
                //    PostAlarm((int)AlarmKeys.eOutputFeederYNotSafe);
                //    return -1;
                //}

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
                if (!OutputDieTransfer.IsPositionPickZSafety())
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
                if (!OutputDieTransfer.IsPositionPickZSafety())
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

                //if (!OutputFeeder.IsFeederYSafetyPosition())
                //{
                //    this.AxisX.EmgStop();
                //    this.AxisY.EmgStop();
                //    this.AxisT.EmgStop();
                //    PostAlarm((int)AlarmKeys.eOutputFeederYNotSafe);
                //    return -1;
                //}
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

            //Todo : 인터락 확인 후 이동 하도록 수정.
            //foreach (var axisKey in tp.AxisPositions.Keys)
            //{
            //    if (Axes.TryGetValue(axisKey, out var axis))
            //    {
            //        double pos = tp.AxisPositions[axisKey];
            //        int r = axis.MoveAbs(pos, vel, acc, dec, jerk);
            //        if (r != 0) result = r;
            //    }
            //}

            return result;
        }

        #region seq signals
        public bool RequestBin { get; set; }
        public bool BinLoadingReady { get; private set; }
        public bool BinLoadingDone { get; private set; }
        public bool BinUnloadingDone { get; private set; }
        public bool BinUnloadingReady { get; private set; }
        public bool BinCompleteWorking { get; internal set; }
        public bool RequestInputDie { get; internal set; }

        public MaterialWafer GetMaterialWafer()
        {
            var mat = GetMaterial();
            return mat as MaterialWafer;
        }

        public bool IsWorking()
        {
            bool bRet = false;
            try
            {
                var wafer = GetMaterialWafer();
                if (wafer == null)
                {
                    if(Config.IsSimulation)
                    {
                        return false;
                    }
                    else
                    {
                        if (IsRingPresent() == false)
                        {
                            return false;
                        }
                        else
                        {
                            OutputFeeder.MakePath();
                            OutputFeeder.MoveMaterial(new MaterialWafer(), this);
                            var waferOutputStage = this.GetMaterialWafer();
                            //waferOutputStage.ProcessSatate = Material.MaterialProcessSatate.Ready;
                            waferOutputStage.ProcessSatate = Material.MaterialProcessSatate.Processing;
                            this.SetMaterial(waferOutputStage);
                            this.UpdateUI();
                        }
                    }
                    
                }

                if (wafer.Presence == Material.MaterialPresence.Exist)
                {
                    if (wafer.ProcessSatate != Material.MaterialProcessSatate.Completed)
                    {
                        bRet = true;
                    }
                }
            }
            catch
            {
                bRet = false;
            }
            return bRet;
        }
        #endregion

        #region Lifecycle
        public override int OnRun()
        {
            int ret = 0;
            if (this.RunUnitStatus == UnitStatus.Stopped ||
                this.RunUnitStatus == UnitStatus.Stopping ||
                this.RunUnitStatus == UnitStatus.CycleStop)
            {
                this.State = ProcessState.Stop;
                ret = -1;
            }
            if (this.RunUnitStatus == UnitStatus.Running)
            {
                return 0;
            }
            if (ret != 0)
            {
                this.State = ProcessState.Stop;
                this.OnStop();
            }
            return ret;
        }
        public override int OnStop() 
        {
            int ret = 0;
            this.RunUnitStatus = UnitStatus.Stopped;
            this.State = ProcessState.Stop;
            base.OnStop();
            return ret;
        }
        protected override int OnRunReady() { return 0; }
        protected override int OnRunWork() { return 0; }
        protected override int OnRunComplete() { return 0; }
        #endregion

        protected override void OnMakeSequence()
        {
            base.OnMakeSequence();
            this.SequencePlayers.Add(LoadingBinPrepare);
            this.SequencePlayers.Add(LoadingBinComplete);
        }

        #region Seq 단위 동작 함수
        public int LoadingBinPrepare(bool isFine = false)
        {
            int nRtn = 0;

            if(RunMode == UnitRunMode.Manual)
            {
                CurrentFunc = LoadingBinPrepare;
            }

            Log.Write(this, "Start LoadingBinPrepare");
            BinLoadingReady = true;
            BinLoadingDone = false;

            // 이미 웨이퍼 존재하면 준비 단계 불필요 (바로 완료 단계 가능)
            if (!Config.IsSimulation && !Config.IsDryRun)
            {
                if (IsRingPresent())
                {
                    Log.Write(UnitName, "LoadingPrep", "Bin already present -> Skip prepare");
                    return nRtn;
                }
            }

            // 로딩 Teaching 이동
            nRtn = MoveToStageLoadPosition();
            if (nRtn != 0)
            {
                Log.Write(this, "Fail: Move Load");
                return -1;
            }
            if (IsStop) { return 0; }

            bool bSimulation = Config.IsSimulation;
            // Clamp Back → Lift Down
            ClampBackward();
            //SetClampFB(false);
            //if (!IsClampBwd())
            //{
            //    if(!bSimulation)
            //    {
            //        PostAlarm((int)AlarmKeys.eClampFB);
            //        Log.Write(this, "Fail: ClampBack");
            //        return -1;
            //    }
            //}
            if (IsStop) { return 0; }

            ClampLiftDown();
            //SetClampLift(false);
            //if (!IsClampLiftDown())
            //{
            //    if (!bSimulation)
            //    {
            //        PostAlarm((int)AlarmKeys.eClampLift);
            //        Log.Write(this, "Fail: ClampLiftDown");
            //        return -1;
            //    }
            //}
            if (IsStop) { return 0; }

            //Plate UP → 
            PlateUp();
            //SetClampPlate(true);
            //if (!IsPlateUp())
            //{
            //    if (!bSimulation)
            //    {
            //        PostAlarm((int)AlarmKeys.ePlate);
            //        Log.Write(this, "Fail: PlateUp");
            //        return -1;
            //    }
            //}
            if (IsStop) { return 0; }

            BinLoadingReady = true;
            Log.Write(UnitName, "LoadingPrep", "StageLoadingReady = TRUE (Wait wafer)");

            Log.Write(this, "End LoadingBinPrepare");
            return 0;
        }
        public int LoadingBinComplete(bool isFine = false)
        {
            int ret = 0;

            if (RunMode == UnitRunMode.Manual)
            {
                CurrentFunc = LoadingBinComplete;
            }

            // 이미 완료
            if (BinLoadingDone)
                return 0;

            // 준비 안 되었으면 호출 순서 오류
            if (!BinLoadingReady && !IsRingPresent())
            {
                Log.Write(UnitName, "LoadingComp", "Not prepared (call LoadingBinComplete first)");
                return -1;
            }

            // 아직 Wafer 안 올라옴 → 대기
            bool bRtn = Config.IsSimulation;
            if (IsRingPresent() || bRtn || Config.IsDryRun)
            {
                PlateDown();
                //SetClampPlate(false);
                //if (!IsPlateDown())
                //{
                //    if (!Config.IsSimulation)
                //    {
                //        PostAlarm((int)AlarmKeys.ePlate);
                //        Log.Write(this, "Fail: PlateUp");
                //        return -1;
                //    }
                //}
                if (IsStop) { return 0; }

                Log.Write(UnitName, "LoadingComp", "Bin detected -> Completing");
                if (!IsPlateUp()|| Config.IsSimulation || Config.IsDryRun)
                {
                    PlateDown();
                    //SetClampPlate(false);
                    //if (!IsPlateDown())
                    //{
                    //    if(!Config.IsSimulation)
                    //    {
                    //        PostAlarm((int)AlarmKeys.ePlate);
                    //        Log.Write(this, "Fail: PlateUp");
                    //        return -1;
                    //    }
                    //}
                    if (IsStop) { return 0; }

                    ClampLiftUp();
                    //SetClampLift(true);
                    //if (!IsClampLiftUp())
                    //{
                    //    if (!Config.IsSimulation)
                    //    {
                    //        PostAlarm((int)AlarmKeys.eClampLift);
                    //        Log.Write(this, "Fail: ClampLiftUp");
                    //        return -1;
                    //    }
                    //}
                    if (IsStop) { return 0; }

                    ClampForward();
                    //SetClampFB(true);
                    //if (!IsClampFwd())
                    //{
                    //    if (!Config.IsSimulation)
                    //    {
                    //        PostAlarm((int)AlarmKeys.eClampFB);
                    //        Log.Write(this, "Fail: ClampForward");
                    //        return -1;
                    //    }
                    //}
                    if (IsStop) { return 0; }
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

                //var wafer = GetMaterialWafer();
                //wafer.ProcessSatate = Material.MaterialProcessSatate.Processing;

                BinLoadingDone = true;
                BinLoadingReady = false;
                Log.Write(UnitName, "LoadingComp", "Done");

                return ret;
            }
            else
            {
                AxisX?.EmgStop(); AxisY?.EmgStop(); AxisT?.EmgStop();
                // 우선 대기? // 신호 이상?
                PostAlarm((int)AlarmKeys.eNoBinDetected);
                Log.Write(UnitName, "LoadingComp", "No Bin detected");
                return -1;
            }

            return ret;
        }
        public int PrepareOutputStageUnloadingBin()
        {
            int nRtn = 0;
            Log.Write(UnitName, "UnloadingPrep", "Start");
            
            if (!IsRingPresent())
            {
                Log.Write(UnitName, "UnloadingPrep", "No Bin -> Skip");
                return 0;
            }

            nRtn = MoveToStageUnloadPosition();
            if (nRtn != 0)
            {
                Log.Write(this, "Fail: Move Unload");
                return -1;
            }
            if (IsStop) { return 0; }

            ClampBackward();
            //SetClampFB(false);
            //if (!IsClampBwd())
            //{
            //    PostAlarm((int)AlarmKeys.eClampFB);
            //    Log.Write(this, "Fail: ClampBack");
            //    return -1;
            //}
            if (IsStop) { return 0; }

            ClampLiftDown();
            //SetClampLift(false);
            //if (!IsClampLiftDown())
            //{
            //    PostAlarm((int)AlarmKeys.eClampLift);
            //    Log.Write(this, "Fail: ClampLiftDown");
            //    return -1;
            //}
            if (IsStop) { return 0; }

            PlateUp();
            //SetClampPlate(true);
            //if (!IsPlateUp())
            //{
            //    PostAlarm((int)AlarmKeys.ePlate);
            //    Log.Write(this, "Fail: PlateUp");
            //    return -1;
            //}
            if (IsStop) { return 0; }

            Log.Write(UnitName, "UnloadingPrep", "StageUnloadingReady = TRUE (Wait wafer pick)");
            return 0;
        }
        public int UnloadingBinComplete()
        {
            int nRtn = 0;

            if (!BinUnloadingReady && IsRingPresent())
            {
                Log.Write(UnitName, "UnloadingComp", "Not prepared");
                return -1;
            }

            BinUnloadingDone = true;
            BinUnloadingReady = false;
            Log.Write(UnitName, "UnloadingComp", "Done");
            return nRtn;
        }
        public bool IsRingPresent()
        {
            bool bRtn = true;
            if (Config.IsSimulation || Config.IsDryRun)
            {
                return true;
            }
            else if (!Ring0() || !Ring1())
            {
                //Log.Write(UnitName, "IsRingPresent", $"Ring not present (R0={Ring0()}, R1={Ring1()})");
                return false;
            }

            return bRtn;
        }
        public bool IsPositionBinLoading()
        {
            var tp = TeachingPositions[(int)OutputStageConfig.TeachingPositionName.Loading];
            if (tp == null) 
                return false;
            return InPosTeaching(tp);
        }
        public bool IsPositionBinUnloading()
        {
            var tp = TeachingPositions[(int)OutputStageConfig.TeachingPositionName.Unloading];
            if (tp == null) return false;
            return InPosTeaching(tp);
        }
        public bool IsPositionBinCenter()
        {
            var tp = TeachingPositions[(int)OutputStageConfig.TeachingPositionName.CenterPoint];
            if (tp == null) return false;
            return InPosTeaching(tp);
        }
        public bool IsStageInterLockOK()
        {
            // 1) CenterPoint Teaching 확보
            var tp = Config.GetTeachingPosition(OutputStageConfig.TeachingPositionName.CenterPoint.ToString());
            if (tp == null || tp.AxisPositions == null)
            {
                Log.Write(UnitName, "MoveSafety", "CenterPoint teaching not found");
                return false;
            }

            // 2) Center 좌표 (OutputStage 축명은 BinStageX / BinStageY 사용)
            if (!tp.AxisPositions.TryGetValue(AxisNames.BinStageX, out var centerX) ||
                !tp.AxisPositions.TryGetValue(AxisNames.BinStageY, out var centerY))
            {
                Log.Write(UnitName, "MoveSafety", "CenterPoint BinStageX/BinStageY value missing");
                return false;
            }

            // 3) 사각형 Half Range
            double halfX = Config.SafeStageRectHalfWidthX;
            double halfY = Config.SafeStageRectHalfHeightY;
            if (halfX <= 0 || halfY <= 0)
            {
                Log.Write(UnitName, "MoveSafety",
                    $"Invalid rectangle half sizes. HalfX={halfX:F3}, HalfY={halfY:F3}");
                return false;
            }

            // 4) 현재 위치
            double curX = AxisX?.GetPosition() ?? centerX;
            double curY = AxisY?.GetPosition() ?? centerY;

            // 5) 사각형 내부 판정
            bool inRect =
                Math.Abs(curX - centerX) <= halfX &&
                Math.Abs(curY - centerY) <= halfY;

            if (inRect)
                return true;

            Log.Write(UnitName, "MoveSafety",
                $"Fail: Out of RECT safe window. Cur=({curX:F3},{curY:F3}) Center=({centerX:F3},{centerY:F3}) Half=({halfX:F3},{halfY:F3})");
            return false;

        }
        
        public void UpdateUI()
        {
            MaterialWafer materialWafer = GetMaterialWafer();
            EventUpdateUIWafer?.BeginInvoke(materialWafer, null, null);
        }
        public void PlaceDie(MaterialDie die)
        {
            lock(this)
            {
                if (_currentDie != null)
                {
                    die.BinX = _currentDie.BinX;
                    die.BinY = _currentDie.BinY;
                    MaterialWafer wafer = GetMaterialWafer();
                    if (wafer != null)
                    {
                        int index = wafer.Dies.IndexOf(_currentDie);
                        if (index >= 0)
                        {
                            wafer.Dies[index] = die;
                        }
                    }
                    _currentDie = null;
                }
            }
            

            // UI 갱신 이벤트 발행 + 개별 배치 이벤트
            UpdateUI();
            OnDiePlaced(die);
        }
        #endregion

        /// <summary>
        /// NextDie(Processing 상태에서 Mapped + Presence == Exist)가 존재하는지 여부만 확인.
        /// 내부 상태 변경(Completed 전환 등) 없이 순수 조회만 수행.
        /// </summary>
        public bool HasNextDie()
        {
            lock(this)
            {
                var wafer = GetMaterialWafer();
                if (wafer == null) return false;

                lock (wafer)
                {
                    if (wafer.Presence != Material.MaterialPresence.Exist)
                        return false;

                    if (wafer.ProcessSatate == Material.MaterialProcessSatate.Completed)
                        return false;

                    if (wafer.ProcessSatate != Material.MaterialProcessSatate.Processing)
                        return false;

                    var next = wafer.Dies
                    .Where(d => d != null && d.Presence != Material.MaterialPresence.Exist)
                    .OrderBy(d => d.BinY).ThenBy(d => d.BinX)
                    .FirstOrDefault();

                    if (next != null)
                    {
                        //Log.Write(UnitName, "HasNextDie", $"Next Die found: Index={next.Index}, Bin=({next.BinX},{next.BinY})");
                    }
                    else
                    {
                        wafer.ProcessSatate = Material.MaterialProcessSatate.Completed;
                        Log.Write(UnitName, "HasNextDie", "No next die found");
                    }

                    return next != null;
                }
            }
            
        }

        // 다음 빈 Bin을 예약(내부 _currentDie 설정)하고 Bin 좌표 반환
        public bool TryReserveNextEmptyBin(out double binX, out double binY, out MaterialDie slot)
        {
            binX = binY = -1;
            slot = null;

            var wafer = GetMaterialWafer();
            if (wafer == null || wafer.Dies == null || wafer.Dies.Count == 0)
                return false;

            // BinY → BinX 순서로 빈 칸 검색
            var next = wafer.Dies
                .Where(d => d != null && d.Presence != Material.MaterialPresence.Exist)
                .OrderBy(d => d.BinY).ThenBy(d => d.BinX)
                .FirstOrDefault();

            if (next == null)
                return false;

            _currentDie = next; // 예약
            binX = next.BinX;
            binY = next.BinY;
            slot = next;
            return true;
        }

        // 레시피와 센터 Teaching을 기준으로 Bin의 XY 세계좌표(mm) 계산
        public (double x, double y) GetBinWorldPosition(double binX, double binY)
        {
            var eq = Equipment.Instance;
            var recipe = eq.EquipmentRecipe.CurrentRecipe;

            // Pitch 및 카운트
            double pitchX = recipe.BinPitchXmm > 0 ? recipe.BinPitchXmm : 1.0;
            double pitchY = recipe.BinPitchYmm > 0 ? recipe.BinPitchYmm : 1.0;
            //pitchX /= 1000;
            //pitchY /= 1000;
            int cntX = recipe.BinCountX > 0 ? recipe.BinCountX : 1;
            int cntY = recipe.BinCountY > 0 ? recipe.BinCountY : 1;

            // 기준 Teaching (CenterPoint) 기반
            var (centerX, centerY, _) = Config.GetPositionWithOffset(OutputStageConfig.TeachingPositionName.CenterPoint.ToString());

            // 센터 기준 좌표계: 중앙이 (0,0)
            double offsetX = (binX - (cntX - 1) / 2.0) * pitchX;
            double offsetY = (binY - (cntY - 1) / 2.0) * pitchY;

            double targetX = centerX + offsetX;
            double targetY = centerY + offsetY;
            return (targetX, targetY);
        }
        
        public int MoveToBinPosition(double binX, double binY, bool isFine = false)
        {
            // 지정 Bin 위치로 XY 이동
            var (tx, ty) = GetBinWorldPosition(binX, binY);

            int rc = 0;
            if (AxisX != null) rc |= MoveAxisPositionOne(AxisX, tx, isFine);
            if (AxisY != null) rc |= MoveAxisPositionOne(AxisY, ty, isFine);
            
            if (rc != 0) 
                return -1;

            return 0;
        }
        #region Update UI
        public void OnDiePlaced(MaterialDie die)
        {
            try
            {
                DiePlaced?.Invoke(this, new DiePlacedEventArgs(die));
            }
            catch (Exception ex)
            {
                Log.Write(UnitName, "[OnDiePlaced] " + ex.Message);
            }
        }

        public bool CanPlaceDie()
        {
            return HasNextDie();
        }


        // === Cylinder 고레벨 제어(완료 대기 포함) ===
        public int PlateUp()
        {
            SetClampPlate(true);
            int r = WaitPlateStateOrAlarm(expectUp: true);
            if (r != 0)
            {
                AxisX?.EmgStop(); AxisY?.EmgStop(); AxisT?.EmgStop();
                Log.Write(this, "PlateUp Failed");
            }
            return r;
        }

        public int PlateDown()
        {
            SetClampPlate(false);
            int r = WaitPlateStateOrAlarm(expectUp: false);
            if (r != 0)
            {
                AxisX?.EmgStop(); AxisY?.EmgStop(); AxisT?.EmgStop();
                Log.Write(this, "PlateDown Failed");
            }
            return r;
        }

        public int ClampLiftUp()
        {
            SetClampLift(true);
            int r = WaitClampLiftStateOrAlarm(expectUp: true);
            if (r != 0)
            {
                AxisX?.EmgStop(); AxisY?.EmgStop(); AxisT?.EmgStop();
                Log.Write(this, "ClampLiftUp Failed");
            }
            return r;
        }

        public int ClampLiftDown()
        {
            // 인터락은 SetClampLift(false) 내부에서 IsClampBwd() 확인
            bool issued = SetClampLift(false);
            if (!issued && !(Config.IsSimulation || Config.IsDryRun))
            {
                PostAlarm((int)AlarmKeys.eClampLift);
                Log.Write(this, "ClampLiftDown Command Rejected (Interlock)");
                return -1;
            }

            int r = WaitClampLiftStateOrAlarm(expectUp: false);
            if (r != 0)
            {
                AxisX?.EmgStop(); AxisY?.EmgStop(); AxisT?.EmgStop();
                Log.Write(this, "ClampLiftDown Failed");
            }
            return r;
        }

        public int ClampForward()
        {
            // 인터락은 SetClampFB(true) 내부에서 IsClampLiftUp() 확인
            bool issued = SetClampFB(true);
            if (!issued && !(Config.IsSimulation || Config.IsDryRun))
            {
                PostAlarm((int)AlarmKeys.eClampFB);
                Log.Write(this, "ClampForward Command Rejected (Interlock)");
                return -1;
            }

            int r = WaitClampFBStateOrAlarm(expectFwd: true);
            if (r != 0)
            {
                AxisX?.EmgStop(); AxisY?.EmgStop(); AxisT?.EmgStop();
                Log.Write(this, "ClampForward Failed");
            }
            return r;
        }

        public int ClampBackward()
        {
            // 인터락은 SetClampFB(false) 내부에서 IsClampLiftUp() 확인
            bool issued = SetClampFB(false);
            if (!issued && !(Config.IsSimulation || Config.IsDryRun))
            {
                PostAlarm((int)AlarmKeys.eClampFB);
                Log.Write(this, "ClampBackward Command Rejected (Interlock)");
                return -1;
            }

            int r = WaitClampFBStateOrAlarm(expectFwd: false);
            if (r != 0)
            {
                AxisX?.EmgStop(); AxisY?.EmgStop(); AxisT?.EmgStop();
                Log.Write(this, "ClampBackward Failed");
            }
            return r;
        }

        #endregion
    }
}