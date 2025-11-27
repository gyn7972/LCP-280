
using QMC.Common;
using QMC.Common.Alarm;
using QMC.Common.Cameras.HIKVISION;
using QMC.Common.Component;
using QMC.Common.IOUtil;
using QMC.Common.Motion;
using QMC.Common.Motions;
using QMC.Common.PKGTester;
using QMC.Common.Unit;
using QMC.Common.VisionPart;
using QMC.LCP_280.Process.Component;
using System; // added for Obsolete attribute
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
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
            eDieTransferPlaceZNotSafety = 3001,
            eOutputFeederCylinderZNotSafety,
            eOutputFeederYNotSafe,
            eNoBinDetected,
            eClampFB,
            eClampLift,
            ePlate,
            eStageNotLoding,
            ePlateCyliderZNotDown,
            eNotReadyToMeasure, // 임시 알람 번호
        }

        #region InitAlarm
        protected override void InitAlarm()
        {
            base.InitAlarm();
            AlarmInfo alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eDieTransferPlaceZNotSafety;
            alarm.Title = "Die TrZAxis Not safety Pos.";
            alarm.Cause = "Die Transfer Z-Axis가 안전 위치가 아닙니다. 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eOutputFeederCylinderZNotSafety;
            alarm.Title = "Feeder Z-Cylinder Not safety Pos.";
            alarm.Cause = "Feeder Z-Cylinder가 안전 위치가 아닙니다. 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            //
            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eOutputFeederYNotSafe;
            alarm.Title = "Feeder Y-Axis Not safety Pos.";
            alarm.Cause = "Feeder Y-Axis가 안전 위치가 아닙니다. 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            //eNoBinDetected
            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eNoBinDetected;
            alarm.Title = "No Bin Detected";
            alarm.Cause = "Bin이 감지되지 않았습니다. Bin이 있는지 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            //,
            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eClampFB;
            alarm.Title = "Clamp F/B Not Pos.";
            alarm.Cause = "Clamp F/B가 지정 위치가 아닙니다. 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);
            //,
            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eClampLift;
            alarm.Title = "Clamp Lift Not Pos.";
            alarm.Cause = "Clamp Lift가 지정 위치가 아닙니다. 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);
            //,
            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.ePlate;
            alarm.Title = "Plate Not Pos.";
            alarm.Cause = "Plate가 지정 위치가 아닙니다. 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);
            //,
            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eStageNotLoding;
            alarm.Title = "Stage Not Loading Pos.";
            alarm.Cause = "Stage 가 준비 위치가 아닙니다.\n 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);
            //,
            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.ePlateCyliderZNotDown;
            alarm.Title = "Plate CylinerZ Not Down.";
            alarm.Cause = "Plate CylinerZ 가 하강 위치가 아닙니다.\n 상태 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eNotReadyToMeasure;
            alarm.Title = "측정 준비가 되지 않았습니다.";
            alarm.Cause = "1. 적용된 Test Condition Set가 있는지 확인하여 주십시오. 2. 계측기가 정상적으로 Initialize 되어 있는지 확인하여 주십시오.";
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
        Rotary RotaryUnit { get; set; }


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
            RotaryUnit = Equipment.Instance.GetUnit(UnitKeys.Rotary) as Rotary;

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
            BindCylinder(_cylPlate);

            if (!IoAutoBindings.Cylinders.TryGetValue("OutStageLift", out _cylClampLift))
            {
                Log.Write("OutputStage", "BindIoDomains", "Cylinder not found: OutStageLift");
            }
            BindCylinder(_cylClampLift);

            if (!IoAutoBindings.Cylinders.TryGetValue("OutStageClampFB", out _cylClampFB))
            {
                Log.Write("OutputStage", "BindIoDomains", "Cylinder not found: OutStageClampFB");
            }
            BindCylinder(_cylClampFB);
        }
        public override bool IsInterlockOK(BaseComponent baseComponent, BaseComponent.InterlockEventArgs e)
        {
            bool bRet = base.IsInterlockOK(baseComponent, e);
            if (baseComponent == AxisX || baseComponent == AxisY || baseComponent == AxisT)
            {
                if (this.OutputDieTransfer.IsPositionPlaceZSafety() == false)
                {
                    this.AxisX?.EmgStop();
                    this.AxisY?.EmgStop();
                    this.AxisT?.EmgStop();
                    PostAlarm((int)AlarmKeys.eDieTransferPlaceZNotSafety);
                    return false;
                }
                else if (this.IsPlateDown() == false)
                {
                    this.AxisX?.EmgStop();
                    this.AxisY?.EmgStop();
                    this.AxisT?.EmgStop();
                    PostAlarm((int)AlarmKeys.ePlateCyliderZNotDown);
                    return false;
                }
                else if (this.OutputFeeder.IsFeederUp() == false)
                {
                    this.AxisX?.EmgStop();
                    this.AxisY?.EmgStop();
                    this.AxisT?.EmgStop();
                    PostAlarm((int)AlarmKeys.eOutputFeederCylinderZNotSafety);
                    return false;
                }
            }
            else if (baseComponent == this._cylPlate)
            {
                if (e.IsExtend)
                {
                    if (this.IsPositionBinLoading() == false)
                    {
                        this.PlateDown();
                        PostAlarm((int)AlarmKeys.eStageNotLoding);
                        return false;
                    }
                    //else if (this.OutputFeeder.IsFeederUp() == false)
                    //{
                    //    this.PlateDown();
                    //    PostAlarm((int)AlarmKeys.eOutputFeederCylinderZNotSafety);
                    //    return false;
                    //}
                }
            }
            else if (baseComponent == this._cylClampLift)
            {
                if (e.IsExtend)
                {
                    if (this.OutputFeeder.IsFeederUp() == false)
                    {
                        this.PlateDown();
                        PostAlarm((int)AlarmKeys.eOutputFeederCylinderZNotSafety);
                        return false;
                    }
                }
            }
            return bRet;
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

            if (bCheckSignal == false)
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
        private int WaitPlateStateOrAlarm(bool expectUp, int timeoutMs = 3000, int pollMs = 2)
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
        private int WaitClampLiftStateOrAlarm(bool expectUp, int timeoutMs = 3000, int pollMs = 2)
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
        private int WaitClampFBStateOrAlarm(bool expectFwd, int timeoutMs = 3000, int pollMs = 2)
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
                    PostAlarm((int)AlarmKeys.eDieTransferPlaceZNotSafety);
                    return -1;
                }
                if (!OutputFeeder.IsFeederZSafetyPosition())
                {
                    AxisX?.EmgStop(); AxisY?.EmgStop(); AxisT?.EmgStop();
                    PostAlarm((int)AlarmKeys.eOutputFeederCylinderZNotSafety);
                    return -1;
                }
                if (!OutputFeeder.IsFeederYSafetyPosition())
                {
                    AxisX?.EmgStop(); AxisY?.EmgStop(); AxisT?.EmgStop();
                    PostAlarm((int)AlarmKeys.eOutputFeederYNotSafe);
                    return -1;
                }

                Thread.Sleep(1);
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
        public int MoveTeachingPositionOnce(OutputStageConfig.TeachingPositionName name, bool isFine)
        {
            return MoveTeachingPositionOnce((int)name, isFine);
        }

        private int IsInterlockStageOK()
        {
            int nRet = 0;

            if (OutputDieTransfer.IsPositionPickZSafety() == false)
            {
                return -1;
            }

            if (OutputFeeder.IsFeederZSafetyPosition() == false)
            {
                return -2;
            }
            return nRet;
        }
        public int MoveToStageReadyPosition(bool isFine = false)
        {
            int nRet = 0;
            Task<int> task = MoveToStageReadyPositionAsync();
            while (IsEndTask(task) == false)
            {
                // Check Interlock.!!! 구문 넣을것.!!!
                nRet = IsInterlockStageOK();
                if (nRet == -1)
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eDieTransferPlaceZNotSafety);
                    return -1;
                }
                if (nRet == -2)
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eOutputFeederCylinderZNotSafety);
                    return -1;
                }

                Thread.Sleep(1);
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
            int nRet = 0;
            nRet = IsInterlockStageOK();
            if (nRet == -1)
            {
                this.AxisX.EmgStop();
                this.AxisY.EmgStop();
                this.AxisT.EmgStop();
                PostAlarm((int)AlarmKeys.eDieTransferPlaceZNotSafety);
                return -1;
            }
            if (nRet == -2)
            {
                this.AxisX.EmgStop();
                this.AxisY.EmgStop();
                this.AxisT.EmgStop();
                PostAlarm((int)AlarmKeys.eOutputFeederCylinderZNotSafety);
                return -1;
            }
            return MoveTeachingPositionOnce((int)OutputStageConfig.TeachingPositionName.Ready, isFine);
        }

        public int MoveToStageCenterPosition(bool isFine = false)
        {
            Task<int> task = MoveToStageCenterPositionAsync(isFine);
            while (IsEndTask(task) == false)
            {
                if (OutputDieTransfer.IsPositionPickZSafety() == false)
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eDieTransferPlaceZNotSafety);
                    return -1;
                }

                if (OutputFeeder.IsFeederZSafetyPosition() == false)
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eOutputFeederCylinderZNotSafety);
                    return -1;
                }

                if (OutputFeeder.IsFeederYSafetyPosition() == false)
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eOutputFeederYNotSafe);
                    return -1;
                }

                if(IsPlateDown() == false)
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.ePlateCyliderZNotDown);
                    return -1;
                }

                Thread.Sleep(1);
            }
            return task.Result;
        }
        public Task<int> MoveToStageCenterPositionAsync(bool isFine = false)
        {
            return Task.Run(() =>
            {
                OnMoveToStageCenterPosition(isFine);
                return 0;
            });
        }
        private int OnMoveToStageCenterPosition(bool isFine = false)
        {
            int nRet = 0;
            nRet = IsInterlockStageOK();
            if (nRet == -1)
            {
                this.AxisX.EmgStop();
                this.AxisY.EmgStop();
                this.AxisT.EmgStop();
                PostAlarm((int)AlarmKeys.eDieTransferPlaceZNotSafety);
                return -1;
            }
            if (nRet == -2)
            {
                this.AxisX.EmgStop();
                this.AxisY.EmgStop();
                this.AxisT.EmgStop();
                PostAlarm((int)AlarmKeys.eOutputFeederCylinderZNotSafety);
                return -1;
            }

            if (IsPlateDown() == false)
            {
                this.AxisX.EmgStop();
                this.AxisY.EmgStop();
                this.AxisT.EmgStop();
                PostAlarm((int)AlarmKeys.ePlateCyliderZNotDown);
                return -1;
            }

            return MoveTeachingPositionOnce((int)OutputStageConfig.TeachingPositionName.CenterPoint, isFine);
        }

        public int MoveToStageLoadPosition(bool isFine = false)
        {
            if (IsPositionBinLoading())
                return 0;

            int nRet = 0;
            Task<int> task = MoveToStageLoadPositionAsync();
            while (IsEndTask(task) == false)
            {
                // Check Interlock.!!! 구문 넣을것.!!!
                nRet = IsInterlockStageOK();
                if (nRet == -1)
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eDieTransferPlaceZNotSafety);
                    return -1;
                }
                if (nRet == -2)
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eOutputFeederCylinderZNotSafety);
                    return -1;
                }

                Thread.Sleep(1);
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
            int nRet = 0;
            nRet = IsInterlockStageOK();
            if (nRet == -1)
            {
                this.AxisX.EmgStop();
                this.AxisY.EmgStop();
                this.AxisT.EmgStop();
                PostAlarm((int)AlarmKeys.eDieTransferPlaceZNotSafety);
                return -1;
            }
            if (nRet == -2)
            {
                this.AxisX.EmgStop();
                this.AxisY.EmgStop();
                this.AxisT.EmgStop();
                PostAlarm((int)AlarmKeys.eOutputFeederCylinderZNotSafety);
                return -1;
            }
            return MoveTeachingPositionOnce((int)OutputStageConfig.TeachingPositionName.Loading, isFine);
        }

        public int MoveToStageUnloadPosition(bool isFine = false)
        {
            int nRet = 0;
            Task<int> task = MoveToStageUnloadPositionAsync();
            while (IsEndTask(task) == false)
            {
                nRet = IsInterlockStageOK();
                if (nRet == -1)
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eDieTransferPlaceZNotSafety);
                    return -1;
                }
                if (nRet == -2)
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eOutputFeederCylinderZNotSafety);
                    return -1;
                }
                Thread.Sleep(1);
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
            int nRet = 0;
            nRet = IsInterlockStageOK();
            if (nRet == -1)
            {
                this.AxisX.EmgStop();
                this.AxisY.EmgStop();
                this.AxisT.EmgStop();
                PostAlarm((int)AlarmKeys.eDieTransferPlaceZNotSafety);
                return -1;
            }
            if (nRet == -2)
            {
                this.AxisX.EmgStop();
                this.AxisY.EmgStop();
                this.AxisT.EmgStop();
                PostAlarm((int)AlarmKeys.eOutputFeederCylinderZNotSafety);
                return -1;
            }

            return MoveTeachingPositionOnce((int)OutputStageConfig.TeachingPositionName.Unloading, isFine);
        }
        
        public int MoveToTeachingPosition(string positionName, double vel = 5, double acc = 10, double dec = 10, double jerk = 50)
        {
            var tp = Config.GetTeachingPosition(positionName);
            if (tp == null) 
                return -1;
            int result = 0;



            return result;
        }

        #region seq signals
        //public bool RequestBin { get; set; }
        //public bool BinLoadingReady { get; set; }
        //public bool BinLoadingDone { get; set; }
        //public bool BinUnloadingDone { get; set; }
        //public bool BinUnloadingReady { get; set; }
        //public bool BinCompleteWorking { get; internal set; }
        //public bool RequestInputDie { get; internal set; }

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
                var Bin = GetMaterialWafer();
                if (Config.IsSimulation == false
                    && Config.IsDryRun == false)
                {
                    if (IsRingPresent() == true)
                    {
                        if (Bin == null)
                        {
                            //알람 발생 해야함.
                            // 제품이 있는데 wafer 정보가 없으면 이상
                            //이건 다른곳에서 확인해야 하나? 이 함수에서는,,
                            Log.Write(UnitName, "IsWorkCompleted", "Wafer present but wafer info is null");
                            return false;
                        }
                    }
                    else
                    {
                        if (Bin == null)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    if (Bin == null)
                    {
                        return false;
                    }
                }

                if (Config.IsSimulation == false
                   && Config.IsDryRun == false)
                {
                    if (IsRingPresent() == false)
                    {
                        return false;
                    }
                    else //제품이 있고 wafer상태가 Completed 가 아니면 작업중으로 간주
                    {
                        if (Bin.Presence == Material.MaterialPresence.Exist)
                        {
                            if (Bin.ProcessSatate != Material.MaterialProcessSatate.Completed)
                            {
                                bRet = true;
                            }
                        }
                    }
                }
                else
                {
                    if (Bin.Presence == Material.MaterialPresence.Exist)
                    {
                        if (Bin.ProcessSatate != Material.MaterialProcessSatate.Completed)
                        {
                            // 작업 중임.
                            bRet = true;
                        }
                    }
                }

                //기존코드
                {
                    //if (wafer == null)
                    //{
                    //    if(Config.IsSimulation)
                    //    {
                    //        return false;
                    //    }
                    //    else
                    //    {
                    //        if (IsRingPresent() == false)
                    //        {
                    //            return false;
                    //        }
                    //        else
                    //        {
                    //            // 새 Wafer를 만들되, SlotIndex를 유추해 -1 발생을 줄임
                    //            int slotId = OutputCassetteLifter?.GetCurrectSlotID() ?? -1;
                    //            var placeholder = new MaterialWafer()
                    //            {
                    //                Presence = Material.MaterialPresence.Exist,
                    //                ProcessSatate = Material.MaterialProcessSatate.Processing,
                    //                SlotIndex = slotId
                    //            };
                    //            this.SetMaterial(placeholder);
                    //            this.UpdateUI();
                    //            //기존 코드
                    //            {
                    //                //OutputFeeder.MakePath();
                    //                //OutputFeeder.MoveMaterial(new MaterialWafer(), this);
                    //                //var waferOutputStage = this.GetMaterialWafer();
                    //                //if(waferOutputStage == null)
                    //                //{
                    //                //    return false;
                    //                //}
                    //                ////waferOutputStage.ProcessSatate = Material.MaterialProcessSatate.Ready;
                    //                //waferOutputStage.ProcessSatate = Material.MaterialProcessSatate.Processing;
                    //                //this.SetMaterial(waferOutputStage);
                    //                //this.UpdateUI();
                    //            }
                    //        }
                    //    }
                    //}

                    //wafer = GetMaterialWafer();
                    //if (wafer != null && wafer.Presence == Material.MaterialPresence.Exist)
                    //{
                    //    if (wafer.ProcessSatate != Material.MaterialProcessSatate.Completed)
                    //    {
                    //        bRet = true;
                    //    }
                    //}
                }

            }
            catch (Exception ex)
            {
                bRet = false;
                Log.Write(ex);
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
               this.RunUnitStatus == UnitStatus.CycleStop ||
               this.RunUnitStatus == UnitStatus.ManualRunning)
            {
                this.State = ProcessState.Stop;
                return 0;
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

            Log.Write(UnitName, "LoadingBinPrepare", "LoadingBinPrepare Start");

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
            nRtn = MoveToStageLoadPosition(isFine);
            if (nRtn != 0)
            {
                Log.Write(UnitName, "LoadingBinPrepare", "MoveToStageLoadPosition Fail");
                return -1;
            }
            if (IsStop) { return 0; }

            bool bSimulation = Config.IsSimulation;
            // Clamp Back → Lift Down
            ClampBackward();
            ClampLiftDown();
            //Plate UP → 
            PlateUp();

            Log.Write(UnitName, "LoadingBinPrepare", "LoadingBinPrepare End");
            return 0;
        }

        public int LoadingBinComplete(bool isFine = false)
        {
            int ret = 0;

            if (RunMode == UnitRunMode.Manual)
            {
                CurrentFunc = LoadingBinComplete;
            }

            // 아직 Wafer 안 올라옴 → 대기
            bool bRtn = Config.IsSimulation;
            // 준비 안 되었으면 호출 순서 오류
            if (!IsRingPresent() && bRtn == false && Config.IsDryRun == false)
            {
                Log.Write(UnitName, "LoadingComp", "Not prepared (call LoadingBinComplete first)");
                return -1;
            }

            
            if (IsRingPresent() || bRtn || Config.IsDryRun)
            {
                Log.Write(UnitName, "LoadingComp", "Bin detected -> Completing");
                {
                    ClampLiftUp();
                    ClampForward();
                    PlateDown();
                    SetVacuum(true);
                }
                // 센터 Teaching 이동
                ret = MoveToStageCenterPosition(isFine);
                if (ret != 0)
                {
                    Log.Write(this, "Fail: Move Load");
                    return ret;
                }
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
        }

        public int PrepareOutputStageUnloadingBin()
        {
            int nRtn = 0;
            Log.Write(UnitName, "PrepareOutputStageUnloadingBin", "Start");
            if (!IsRingPresent())
            {
                Log.Write(UnitName, "PrepareOutputStageUnloadingBin", "No Bin");
                return -1;
            }

            nRtn = MoveToStageUnloadPosition();
            if (nRtn != 0)
            {
                Log.Write(this, "Fail: Move Unload");
                return -1;
            }

            ClampBackward();
            ClampLiftDown();
            PlateUp();
            SetVacuum(false);

            Log.Write(UnitName, "UnloadingPrep", "StageUnloadingReady = TRUE (Wait wafer pick)");
            return 0;
        }

        public int UnloadingBinComplete()
        {
            int nRtn = 0;

            if (IsRingPresent())
            {
                Log.Write(UnitName, "UnloadingComp", "Not prepared");
                return -1;
            }

            Log.Write(UnitName, "UnloadingComp", "Done");
            return nRtn;
        }
        public bool IsRingPresent()
        {
            bool bRtn = true;
            if (Config.IsSimulation || Config.IsDryRun)
            {
                // 시뮬레이션: 실제 보유 머티리얼로 판단
                return this.GetMaterial() is MaterialWafer;
                //return true;
            }
            else if (!Ring0() || !Ring1())
            {
                var v = this.GetMaterial() as MaterialWafer;
                if(v != null)
                {
                    return v.Presence == Material.MaterialPresence.Exist;

                }
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

        public void MarkCurrentReservedMissing()
        {
            try
            {
                var wafer = GetMaterialWafer();
                if (wafer == null || wafer.Dies == null || wafer.Dies.Count == 0) 
                    return;

                lock (wafer)
                {
                    if (_currentDie == null) 
                        return;

                    int idx = _currentDie.Index;
                    var die = wafer.Dies.FirstOrDefault(d => d != null && d.Index == idx);
                    if (die == null) 
                        return;

                    // 이미 Placed면 변경하지 않음
                    if (die.State == DieProcessState.Placed) 
                        return;

                    die.State = DieProcessState.Rejected;
                    die.Presence = Material.MaterialPresence.Exist; // 변경: NotExist → Exist
                    
                    //Test 해보자.
                    //PlaceDie(die);
                }
                UpdateUI();
                Log.Write(UnitName, "MarkCurrentReservedMissing", "Marked current reserved slot as Rejected.");
            }
            catch (Exception ex)
            {
                Log.Write(UnitName, "MarkCurrentReservedMissing", ex.Message);
            }
        }

        public void PlaceDie(MaterialDie die)
        {
            var wafer = GetMaterialWafer();
            if (wafer == null || die == null) 
                return;

            bool allPlacedOrRejected = false;
            string waferIdSnapshot = null;

            lock (wafer)
            {
                if (_currentDie != null)
                {
                    die.BinX = _currentDie.BinX;
                    die.BinY = _currentDie.BinY;
                    int idx = wafer.Dies.IndexOf(_currentDie);
                    if (idx >= 0)
                    {
                        die.Presence = Material.MaterialPresence.Exist;
                        die.State = DieProcessState.Placed;
                        wafer.Dies[idx] = die;
                    }
                    _currentDie = die;
                }

                waferIdSnapshot = wafer.WaferId;

                // 전체 다이 상태 검사(두 조건: Placed만 / Placed+Rejected)
                if (wafer.Dies.All(d => d != null && d.State == DieProcessState.Placed))
                {
                    wafer.ProcessSatate = Material.MaterialProcessSatate.Completed;
                    Log.Write(UnitName, "PlaceDie", "All dies placed -> Completed");
                }
                else if (wafer.Dies.All(d => d != null &&
                         (d.State == DieProcessState.Placed || d.State == DieProcessState.Rejected)))
                {
                    wafer.ProcessSatate = Material.MaterialProcessSatate.Completed;
                    Log.Write(UnitName, "PlaceDie", "All dies Placed/Rejected -> Completed");
                }

                allPlacedOrRejected = (wafer.ProcessSatate == Material.MaterialProcessSatate.Completed);
            }

            // 2) 결과 저장 (개별 다이) - 실패 시 즉시 반환
            Equipment.Instance.ResultWriterManager.CurrentTestConditionSet = Equipment.Instance.Tester.ConditionSet;
            int rc = 0;

            // 3) 통계 누적 (다이 단위)
            Equipment.Instance.ResultWriterManager.AccumulateDie(die);

            rc = AssignDataToMaterialObject(die);
            if (rc != 0) 
            { 
                PostAlarm((int)AlarmKeys.eNotReadyToMeasure); 
                Log.Write(UnitName, "PlaceDie", "AssignDataToMaterialObject Fail"); 
                return; 
            }

            rc = Equipment.Instance.ResultWriterManager.AppendTxTDie(die);
            if (rc != 0) 
            { 
                PostAlarm((int)AlarmKeys.eNotReadyToMeasure); 
                Log.Write(UnitName, "PlaceDie", "AppendTxTDie Fail"); 
                return;
            }

            rc = Equipment.Instance.ResultWriterManager.AppendPrdDie(die);
            if (rc != 0) 
            { 
                PostAlarm((int)AlarmKeys.eNotReadyToMeasure); 
                Log.Write(UnitName, "PlaceDie", "AppendPrdDie Fail"); 
                return; 
            }

            rc = Equipment.Instance.ResultWriterManager.AppendWafDie(die);
            if (rc != 0) 
            { 
                PostAlarm((int)AlarmKeys.eNotReadyToMeasure); 
                Log.Write(UnitName, "PlaceDie", "AppendWafDie Fail"); 
                return; 
            }

            // 전부 저장해보자.
            // 5) 웨이퍼 종료 시점에만 요약(SUM) 파일 확정
            //if (allPlacedOrRejected && !string.IsNullOrWhiteSpace(die.SourceWaferId))
            {
                // 4) Bin 요약 파일은 매 다이 덮어쓰기가 필요하면 유지, 아니면 웨이퍼 종료시에만
                Equipment.Instance.ResultWriterManager.AppendBinDie(die);
                Equipment.Instance.ResultWriterManager.FinalizeSummary();
                Equipment.Instance.ResultWriterManager.WriteSumFile(die);
            }

            // 6) UI & 이벤트
            UpdateUI();
            OnDiePlaced(die);
        }
        #endregion

        /// <summary>
        /// NextDie(Processing 상태에서 Mapped + Presence == Exist)가 존재하는지 여부만 확인.
        /// 내부 상태 변경(Completed 전환 등) 없이 순수 조회만 수행.
        /// </summary>
        /// 
        private int _lastHasNextMask = -1;
        public bool HasNextDie()
        {
            var wafer = GetMaterialWafer();
            if (wafer == null) 
                return false;

            lock (wafer)
            {
                // 맵이 없으면 없음
                var dies = wafer.Dies;
                if (dies == null || dies.Count == 0)
                    return false;

                // TryReserveNextEmptyBin과 동일 기준: Presence 무시, State만 사용
                bool has = dies.Any(d =>
                    d != null &&
                    d.State != DieProcessState.Placed &&
                    d.State != DieProcessState.Rejected);

                // 상태 정합성 보정
                if (has == false)
                {
                    if (wafer.ProcessSatate != Material.MaterialProcessSatate.Completed)
                        wafer.ProcessSatate = Material.MaterialProcessSatate.Completed;
                }
                else
                {
                    if (wafer.ProcessSatate == Material.MaterialProcessSatate.Completed)
                        wafer.ProcessSatate = Material.MaterialProcessSatate.Processing;
                }
                return has;
            }

            // 진짜 안되면 변경해보자.
            //var wafer = GetMaterialWafer();
            //if (wafer == null) return false;
            //lock (wafer)
            //{
            //    var dies = wafer.Dies;
            //    if (dies == null || dies.Count == 0) return false;

            //    int total = dies.Count;
            //    int placed = 0;
            //    int rejected = 0;
            //    int unplaced = 0; // 실제 배치 대상
            //    foreach (var d in dies)
            //    {
            //        if (d == null) continue;
            //        if (d.State == DieProcessState.Placed) { placed++; continue; }
            //        if (d.State == DieProcessState.Rejected) { rejected++; continue; }
            //        // 나머지(NONE / Mapped / Picked / Inspected 등)는 아직 배치 가능
            //        unplaced++;
            //    }

            //    // 순수 조회: wafer.ProcessSatate 변경하지 않음
            //    // false가 나올 때 구분은 호출측에서 카운트로 판단 가능하도록 로그 조건 추가
            //    bool has = unplaced > 0;

            //    // 변화 시에만 로그 (과다 방지)
            //    int mask = (has ? 1 : 0) |
            //               ((placed == total) ? 2 : 0) |
            //               ((rejected == total) ? 4 : 0);
            //    if (_lastHasNextMask != mask)
            //    {
            //        Log.Write(UnitName, "HasNextDie",
            //            $"has={has}, total={total}, placed={placed}, rejected={rejected}, unplaced={unplaced}, waferState={wafer.ProcessSatate}");
            //        _lastHasNextMask = mask;
            //    }
            //    return has;
            //}
        }


        private IOrderedEnumerable<MaterialDie> OrderEmptyDiesForPlacement(IEnumerable<MaterialDie> dies)
        {
            // 좌하단부터: 현재 좌표계가 우상단 원점(0,0)처럼 동작하는 경우
            // BinY, BinX 모두 내림차순으로 정렬하면 물리적 좌하단부터 선택됨
            return dies
                .Where(d => d != null && d.Presence != Material.MaterialPresence.Exist)
                .OrderByDescending(d => d.BinY)
                .ThenByDescending(d => d.BinX);
        }


        // 다음 빈 Bin 예약: 정렬 제거, 리스트 순서(경로 순서) 사용
        public bool TryReserveNextEmptyBin(out double binX, out double binY, out MaterialDie slot)
        {
            binX = binY = -1;
            slot = null;

            var wafer = GetMaterialWafer();
            if (wafer == null || wafer.Dies == null || wafer.Dies.Count == 0)
                return false;

            Func<MaterialDie, bool> isUnplaced = d =>
                d != null &&
                d.State != DieProcessState.Placed &&
                d.State != DieProcessState.Rejected;

            var dieRotary = RotaryUnit?.GetUnloadSocketMaterial();
            var dieOutTr = OutputDieTransfer.GetMaterial() as MaterialDie;

            MaterialDie next = null;

            // 1) OutputDieTransfer가 들고 있는 다이 우선
            if (dieOutTr != null)
                next = wafer.Dies.FirstOrDefault(d => isUnplaced(d) && d.Index == dieOutTr.Index);

            // 2) Rotary 소켓 다이
            if (next == null && dieRotary != null)
                next = wafer.Dies.FirstOrDefault(d => isUnplaced(d) && d.Index == dieRotary.Index);

            // 3) 그 외 첫 미배치
            if (next == null)
                next = wafer.Dies.FirstOrDefault(isUnplaced);

            if (next == null)
                return false;

            _currentDie = next;
            binX = next.BinX;
            binY = next.BinY;
            slot = next;

            bool matchedOutTr = (dieOutTr != null && next.Index == dieOutTr.Index);
            bool matchedRotary = (dieRotary != null && next.Index == dieRotary.Index);

            Log.Write(UnitName, "TryReserveNextEmptyBin",
                $"Reserved Index={next.Index}, Bin=({binX},{binY}), State={next.State}, Presence={next.Presence}, MatchedByOutTr={matchedOutTr}, MatchedByRotary={matchedRotary}");

            return true;



            //binX = binY = -1;
            //slot = null;
            //var wafer = GetMaterialWafer();
            //if (wafer == null || wafer.Dies == null || wafer.Dies.Count == 0)
            //    return false;

            //// “미배치 대상” 정의: Placed/Rejected가 아닌 다이
            //Func<MaterialDie, bool> isUnplaced = d =>
            //    d != null &&
            //    d.State != DieProcessState.Placed &&
            //    d.State != DieProcessState.Rejected;

            //var dieRotary = RotaryUnit?.GetUnloadSocketMaterial();
            //var dieOutTr = OutputDieTransfer.GetMaterial() as MaterialDie;
            //MaterialDie next = null;

            //if (dieRotary != null)
            //    next = wafer.Dies.FirstOrDefault(d => isUnplaced(d) && d.Index == dieRotary.Index);

            //if (next == null && dieOutTr != null)
            //    next = wafer.Dies.FirstOrDefault(d => isUnplaced(d) && d.Index == dieOutTr.Index);

            //if (next == null)
            //    next = wafer.Dies.FirstOrDefault(d => isUnplaced(d));

            //if (next == null)
            //    return false;

            //_currentDie = next;
            //binX = next.BinX;
            //binY = next.BinY;
            //slot = next;

            //bool matched = (dieRotary != null && next.Index == dieRotary.Index);
            //Log.Write(UnitName, "TryReserveNextEmptyBin",
            //    $"Reserved Index={next.Index}, Bin=({binX},{binY}), State={next.State}, Presence={next.Presence}, MatchedByRotaryIndex={matched}");
            //return true;


        }
       
        public (double x, double y) GetBinWorldPosition(double binX, double binY)
        {
            var eq = Equipment.Instance;
            var recipe = eq.EquipmentRecipe.CurrentRecipe;

            // 1) 피치 결정: ChipWidth/Height 우선, 없으면 BinPitch로 폴백
            double pitchX = (recipe.ChipWidth > 0) ? recipe.ChipWidth :
                            (recipe.BinPitchXmm > 0) ? recipe.BinPitchXmm : 1.0;
            double pitchY = (recipe.ChipHeight > 0) ? recipe.ChipHeight :
                            (recipe.BinPitchYmm > 0) ? recipe.BinPitchYmm : 1.0;

            // 2) CenterPoint Teaching (월드 좌표 원점 역할)
            var (centerX, centerY, _) = Config.GetPositionWithOffset(OutputStageConfig.TeachingPositionName.CenterPoint.ToString());

            // 3) 중심 인덱스 계산
            double indexCenterX, indexCenterY;

            var wafer = GetMaterialWafer();
            if (wafer?.Dies != null && wafer.Dies.Count > 0)
            {
                // 현재 맵 데이터로부터 격자 범위를 구해 중심 인덱스 산출
                int minIdxX = (int)Math.Round(wafer.Dies.Min(d => d.BinX));
                int maxIdxX = (int)Math.Round(wafer.Dies.Max(d => d.BinX));
                int minIdxY = (int)Math.Round(wafer.Dies.Min(d => d.BinY));
                int maxIdxY = (int)Math.Round(wafer.Dies.Max(d => d.BinY));

                indexCenterX = (minIdxX + maxIdxX) / 2.0;
                indexCenterY = (minIdxY + maxIdxY) / 2.0;
            }
            else
            {
                // 맵 데이터가 없으면 웨이퍼 지름 + 피치로 격자 개수를 추정
                double diameterMm = (recipe.WaferDiameter > 0) ? recipe.WaferDiameter : 0.0;
                double marginMm = 0.0; // 필요 시 설정으로 분리 가능
                double radiusMm = Math.Max(0.0, diameterMm / 2.0 - marginMm);

                int halfCellsX = (pitchX > 0) ? (int)Math.Floor(radiusMm / pitchX) : 0;
                int halfCellsY = (pitchY > 0) ? (int)Math.Floor(radiusMm / pitchY) : 0;

                int cntX = Math.Max(1, halfCellsX * 2 + 1);
                int cntY = Math.Max(1, halfCellsY * 2 + 1);

                indexCenterX = (cntX - 1) / 2.0;
                indexCenterY = (cntY - 1) / 2.0;
            }

            // 4) 인덱스 오프셋 → 월드(mm) 오프셋
            double offsetX = (binX - indexCenterX) * pitchX;
            double offsetY = (binY - indexCenterY) * pitchY;

            // 5) 최종 월드 좌표
            double targetX = centerX + offsetX;
            double targetY = centerY + offsetY;
            return (targetX, targetY);
        }
        //public (double x, double y) GetBinWorldPosition(double binX, double binY)
        //{
        //    var eq = Equipment.Instance;
        //    var recipe = eq.EquipmentRecipe.CurrentRecipe;

        //    // Pitch 및 카운트
        //    double pitchX = recipe.BinPitchXmm > 0 ? recipe.BinPitchXmm : 1.0;
        //    double pitchY = recipe.BinPitchYmm > 0 ? recipe.BinPitchYmm : 1.0;
        //    //pitchX /= 1000;
        //    //pitchY /= 1000;
        //    int cntX = recipe.BinCountX > 0 ? recipe.BinCountX : 1;
        //    int cntY = recipe.BinCountY > 0 ? recipe.BinCountY : 1;

        //    // 기준 Teaching (CenterPoint) 기반
        //    var (centerX, centerY, _) = Config.GetPositionWithOffset(OutputStageConfig.TeachingPositionName.CenterPoint.ToString());

        //    // 센터 기준 좌표계: 중앙이 (0,0)
        //    double offsetX = (binX - (cntX - 1) / 2.0) * pitchX;
        //    double offsetY = (binY - (cntY - 1) / 2.0) * pitchY;

        //    double targetX = centerX + offsetX;
        //    double targetY = centerY + offsetY;
        //    return (targetX, targetY);
        //}

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
            bool bRet = true;
            bRet &= this.AxisX.IsMoveDone();
            bRet &= this.AxisY.IsMoveDone();
            bRet &= this.AxisT.IsMoveDone();
            bRet &= HasNextDie();

            return bRet;
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
                AxisX?.EmgStop(); 
                AxisY?.EmgStop(); 
                AxisT?.EmgStop();
                Log.Write(this, "ClampBackward Failed");
            }
            return r;
        }

        #endregion

        public void ResetForNewRun(bool moveToSafeReady = true, bool clearWafer = true, bool clearOffsets = true)
        {
            // 1) 런타임/시퀀스 플래그 초기화
            _currentDie = null;

            // 2) 비전 리소스 정리(선택)
            try
            {
                OutStageCamera?.LatestImage?.Dispose();
                if (OutStageCamera != null) 
                    OutStageCamera.LatestImage = null;
            }
            catch (Exception ex)
            {
                Log.Write(UnitName, $"[ResetForNewRun] Clear camera image failed: {ex.Message}");
            }

            // 3) 머티리얼 정리/초기화(선택)
            try
            {
                var wafer = GetMaterialWafer();
                if (clearWafer)
                {
                    SetMaterial(null);
                    UpdateUI();
                }
                else if (wafer != null)
                {
                    wafer.ProcessSatate = Material.MaterialProcessSatate.Unknown;
                }
            }
            catch (Exception ex)
            {
                Log.Write(UnitName, $"[ResetForNewRun] Material reset failed: {ex.Message}");
            }

            // 4) 오프셋 초기화(선택)
            if (clearOffsets)
            {
                try
                {
                    Config.SetOffset(OutputStageConfig.TeachingPositionName.CenterPoint.ToString(), 0, 0, 0);
                    Config.SetOffset(OutputStageConfig.TeachingPositionName.Loading.ToString(), 0, 0, 0);
                    Config.SetOffset(OutputStageConfig.TeachingPositionName.Unloading.ToString(), 0, 0, 0);
                    Config.SetOffset(OutputStageConfig.TeachingPositionName.Ready.ToString(), 0, 0, 0);
                }
                catch (Exception ex)
                {
                    Log.Write(UnitName, $"[ResetForNewRun] Clear offsets failed: {ex.Message}");
                }
            }

            // 5) IO 안전 상태 복귀
            try
            {
                if (!(Config.IsSimulation || Config.IsDryRun))
                {
                    // 순서: 클램프 후퇴 → 리프트 다운 → 플레이트 다운 → 진공 OFF
                    ClampBackward();
                    ClampLiftDown();
                    //PlateDown();
                    PlateUp();
                    SetVacuum(false);
                }
            }
            catch (Exception ex)
            {
                Log.Write(UnitName, $"[ResetForNewRun] IO safe-state failed: {ex.Message}");
            }
        }

        //OutputStage 클래스 내부(예: PlaceDie 위쪽 또는 ResetForNewRun 아래 편한 위치)
        public int CloneDieMapFromInputStage(InputStage inputStage,
                                             bool rotate180 = false,
                                             bool swapXY = false,
                                             bool mirrorX = false,
                                             bool mirrorY = false)
        {
            try
            {
                var src = inputStage?.GetMaterialWafer();
                if (src == null || src.Dies == null || src.Dies.Count == 0) return -1;

                var dst = GetMaterialWafer();
                if (dst == null)
                {
                    dst = new MaterialWafer();
                    SetMaterial(dst);
                }

                // 기본 메타 복사
                dst.WaferId = string.IsNullOrWhiteSpace(dst.WaferId) ? $"QMC_BIN_{src.WaferId}" : dst.WaferId;
                dst.CarrierId = src.CarrierId;
                dst.WaferDate = src.WaferDate;
                dst.Presence = Material.MaterialPresence.Exist;
                dst.ProcessSatate = Material.MaterialProcessSatate.Processing;

                var list = new List<MaterialDie>(src.Dies.Count);

                foreach (var s in src.Dies)
                {
                    if (s == null) continue;

                    // 좌표 변환(필요 시)
                    double mx = s.MapX, my = s.MapY;
                    if (rotate180)
                    {
                        mx = -mx; my = -my;
                    }
                    if (mirrorX) mx = -mx;
                    if (mirrorY) my = -my;

                    if (swapXY)
                    {
                        var tmp = mx; mx = my; my = tmp;
                    }

                    // 복제: Index/Name 보존, 상태는 Output 목적에 맞게 초기화
                    var d = new MaterialDie
                    {
                        Index = s.Index,                   // 보존
                        Name = s.Name,                     // 보존
                        MapX = (int)mx,
                        MapY = (int)my,
                        // Output Bin 좌표는 내부에서 변환 사용 시 따로 설정 가능(없으면 MapX/Y 기반 사용)
                        BinX = mx,
                        BinY = my,

                        // Output 시작 상태: 아직 놓지 않음
                        Presence = Material.MaterialPresence.NotExist,
                        State = DieProcessState.None,

                        SourceWaferId = dst.WaferId
                    };

                    list.Add(d);
                }

                // Index는 보존하되, 정렬(순회)은 별도 루틴에서 수행
                // 리스트는 Index 오름차순으로 정렬하여 보관(선택)
                dst.Dies = list.OrderBy(d => d.Index).ToList();

                UpdateUI();
                Log.Write(UnitName, "CloneDieMapFromInputStage",
                    $"Cloned {dst.Dies.Count} dies from '{inputStage.UnitName}' (preserved Index/Name)");
                return 0;
            }
            catch (Exception ex)
            {
                Log.Write(UnitName, "CloneDieMapFromInputStage", ex.Message);
                return -1;
            }

        }

        public int AssignDataToMaterialObject(MaterialDie materialDie)
        {
            if (materialDie == null)
            {
                Log.Write(UnitName, "AssignDataToMaterialObject", "materialDie Info. Fail.");
                return -1;
            }

            PKGTesterResult result = materialDie.TesterResult;
            // 임시 테스트 코드 -----
            string logDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            if (!System.IO.Directory.Exists(logDir))
                System.IO.Directory.CreateDirectory(logDir);

            //var wafer = Rotary.GetMaterial() as MaterialDie;
            //var die = Rotary.GetProbeSocketMaterial();
            string waferID = "";
            if (materialDie != null)
            {
                waferID = materialDie.SourceWaferId;
                Log.Write(UnitName, $"Index_{materialDie.Index}, WaferID_{materialDie.SourceWaferId}, " +
                    $"BinID_{materialDie.TargetWaferId}, State_{materialDie.State.ToString()}");
            }
            else
            {
                waferID = "None";
                Log.Write(UnitName, "AssignDataToMaterialObject", "die.SourceWaferId Fail");
            }
            int nSocketIndex = materialDie.SocketIndex;

            string logFile = System.IO.Path.Combine(logDir, $"{waferID}_{DateTime.Now:yyyyMMdd}.csv");
            bool fileExists = System.IO.File.Exists(logFile);
            // 신규 파일일 때만 StrainGage 컬럼을 헤더에 추가(기존 파일 헤더 불일치 방지)
            var sgKeys = new List<string>();
            ////if (!fileExists && materialDie != null && materialDie.MeasureValues != null)
            //{
                sgKeys = materialDie.MeasureValues.Keys
                          .Where(k => k.StartsWith("SG", StringComparison.OrdinalIgnoreCase))
                          .OrderBy(k => k, StringComparer.OrdinalIgnoreCase)
                          .ToList();
            //}

            using (var writer = new System.IO.StreamWriter(logFile, true, System.Text.Encoding.UTF8))
            {
                // 파일이 없으면 헤더 추가
                if (!fileExists)
                {
                    writer.Write("Time,");
                    writer.Write("SocketNo,");
                    writer.Write("DieNo,");
                    writer.Write("DiePosX,");
                    writer.Write("DiePosY,");

                    // Bin / Rank 컬럼
                    writer.Write("BinNo,");
                    writer.Write("BinType,");
                    writer.Write("BinLabel,");

                    foreach (var item in result.Items)
                    {
                        writer.Write($"{item.Key},");
                    }

                    // StrainGage 헤더(있을 때만)
                    //foreach (var key in sgKeys)
                    //{
                    //    writer.Write($"{key},");
                    //}

                    writer.WriteLine();
                }

                // 데이터 행 추가 +1하지말자.
                writer.Write($"{DateTime.Now:yyyy-MM-dd HH:mm:ss:fff},");
                writer.Write($"{nSocketIndex + 1},");
                writer.Write($"{materialDie.Index + 1},");
                writer.Write($"{materialDie.MapX * -1},");
                writer.Write($"{materialDie.MapY * -1},");

                // Bin / Rank 값
                var binResult = result.BinningResult;

                // BinNo / BinLabel
                writer.Write($"{binResult?.BinNo},");
                writer.Write($"{binResult?.BinType},");
                writer.Write($"{binResult?.BinLabel},");

                //foreach (var item in result.Items)
                //{
                //    writer.Write($"{item.Value},");
                //}

                // 신규 파일 헤더에 StrainGage 키를 넣은 경우에만 값도 함께 출력
                //if (sgKeys.Count > 0 && materialDie != null && materialDie.MeasureValues != null)
                {
                    foreach (var key in sgKeys)
                    {
                        double v;
                        materialDie.MeasureValues.TryGetValue(key, out v);
                        writer.Write($"{v},");
                    }
                }

                writer.WriteLine();
            }
            // ---------------------
            return 0;
        }

        private int SaveResultData(MaterialDie materialDie)
        {
            // Do Something...
            PKGTesterResult result = materialDie.TesterResult;
            //var wafer = Rotary.GetMaterial() as MaterialDie;   //InputStage.GetMaterialWafer();
            //var die = Rotary.GetProbeSocketMaterial();
            string waferID = "";
            if (materialDie != null)
            {
                waferID = materialDie.SourceWaferId;
                Log.Write(UnitName, $"Index_{materialDie.Index}, WaferID_{materialDie.SourceWaferId}, " +
                    $"BinID_{materialDie.TargetWaferId}, State_{materialDie.State.ToString()}");
            }
            else
            {
                waferID = "None";
                Log.Write(UnitName, "AssignDataToMaterialObject", "die.SourceWaferId Fail");
            }


            string logDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ResultData", waferID);
            if (!System.IO.Directory.Exists(logDir))
                System.IO.Directory.CreateDirectory(logDir);

            int nIndex = materialDie.SocketIndex;   //this.GetProbeIndexNo();

            string logFile = System.IO.Path.Combine(logDir, $"{waferID}_{DateTime.Now:yyyyMMdd}.txt");
            bool fileExists = System.IO.File.Exists(logFile);

            // 신규 파일일 때만 StrainGage 컬럼을 헤더에 추가(기존 파일 헤더 불일치 방지)
            var sgKeys = new List<string>();
            //if (!fileExists && materialDie != null && materialDie.MeasureValues != null)
            {
                sgKeys = materialDie.MeasureValues.Keys
                          .Where(k => k.StartsWith("SG", StringComparison.OrdinalIgnoreCase))
                          .OrderBy(k => k, StringComparer.OrdinalIgnoreCase)
                          .ToList();
            }

            using (var writer = new System.IO.StreamWriter(logFile, true, System.Text.Encoding.UTF8))
            {
                // 파일이 없으면 헤더 추가
                if (!fileExists)
                {
                    writer.Write("Bin_FileNeme.bin\n");
                    writer.Write($"{waferID}\n");
                    writer.Write("XADR,");
                    writer.Write("YADR,");
                    writer.Write("RANK,");

                    // StrainGage 헤더(있을 때만)
                    foreach (var key in sgKeys)
                    {
                        writer.Write($"{key},");
                    }

                    foreach (var item in result.Items)
                    {
                        writer.Write($"{item.Key},");
                    }

                    writer.WriteLine();
                }

                // 데이터 행 추가.
                //writer.Write($"{die.MapX},");
                //writer.Write($"{die.MapY},");
                //현장맞춤..
                writer.Write($"{materialDie.MapX * -1},");
                writer.Write($"{materialDie.MapY * -1},");
                // Bin / Rank 값
                var binResult = result.BinningResult;
                writer.Write($"{binResult?.BinNo},");

                // 신규 파일 헤더에 StrainGage 키를 넣은 경우에만 값도 함께 출력
                //if (sgKeys.Count > 0 && materialDie != null && materialDie.MeasureValues != null)
                {
                    foreach (var key in sgKeys)
                    {
                        double v;
                        materialDie.MeasureValues.TryGetValue(key, out v);
                        writer.Write($"{v},");
                    }
                }

                foreach (var item in result.Items)
                {
                    writer.Write($"{item.Value},");
                }
                writer.WriteLine();
            }
            // ---------------------
            return 0;
        }


    }
}