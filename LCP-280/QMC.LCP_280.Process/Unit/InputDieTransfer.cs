using QMC.Common;
using QMC.Common.Component;
using QMC.Common.IOUtil;
using QMC.Common.Motion;
using QMC.Common.Motions;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Component;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;

namespace QMC.LCP_280.Process.Unit
{
    /// <summary>
    /// InputDieTransfer Unit
    ///  - Tool T / Pick Z / Place Z 축 관리 + Teaching Position 및 Offset
    ///  - 4 Arm Vacuum / Blow / Vent 제어
    ///  - Air/Vac Tank Pressure / Arm Flow 센서 입력
    ///  - DryRun 시뮬레이션 지원
    ///  - OutputStage 스타일의 Region/메서드 구조로 재구성
    /// </summary>
    public class InputDieTransfer : BaseUnit
    {
        #region Config / Teaching
        public InputDieTransferConfig InputDieTransferConfig { get; private set; }
        public List<TeachingPosition> TeachingPositions { get; private set; } = new List<TeachingPosition>();
        #endregion

        #region DryRun Simulation
        public bool DryRun { get; private set; }
        public void SetDryRun(bool on) => DryRun = on;
        private readonly Dictionary<string, bool> _simOutputs = new Dictionary<string, bool>(System.StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, bool> _simInputs  = new Dictionary<string, bool>(System.StringComparer.OrdinalIgnoreCase);
        #endregion

        #region Axes
        private MotionAxis _toolT, _pickZ, _placeZ;
        public MotionAxis ToolT => _toolT;
        public MotionAxis PickZ => _pickZ;
        public MotionAxis PlaceZ => _placeZ;
        #endregion

        #region ctor / Initialization
        public InputDieTransfer(InputDieTransferConfig config = null) : base("InputDieTransferConfig")
        {
            InputDieTransferConfig = config ?? new InputDieTransferConfig();
            AddComponents();
        }

        public override void AddComponents()
        {
            InputDieTransferConfig.LoadAndBindAxes(Equipment.Instance.AxisManager);
            InputDieTransferConfig.InitializeDefaultTeachingPositions();
            TeachingPositions.Clear();
            foreach (var tp in InputDieTransferConfig.TeachingPositions)
                TeachingPositions.Add(tp);
            BindAxes();
            // (Arm IO 는 단순 DO/DI 이름 매핑이므로 별도 Cylinder/Vacuum Domain 구성 생략)
            BindIoDomains();
        }
        #endregion

        #region Axis Binding / Helpers
        private void BindAxes()
        {
            var mgr = Equipment.Instance?.AxisManager;
            if (mgr == null)
            {
                Log.Write("InputDieTransfer", "[BindAxes] AxisManager null");
                return;
            }

            const string unitName = "Unit"; // Equipment에서 축 등록 시 사용한 유닛명과 동일해야 함
            BindAxis(mgr, unitName, AxisNames.LeftToolT, ref _toolT);
            BindAxis(mgr, unitName, AxisNames.LeftPickZ, ref _pickZ);
            BindAxis(mgr, unitName, AxisNames.LeftPlaceZ, ref _placeZ);
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
            var tp = InputDieTransferConfig.GetTeachingPosition(tpName);
            if (tp != null && tp.AxisPositions != null && tp.AxisPositions.TryGetValue(axisName, out var v)) return v;
            return 0.0;
        }
        #endregion

        #region Teaching Helpers
        public void TeachCurrentPosition(string name, string description = null)
        {
            var pos = new Dictionary<string, double>();
            foreach (var kv in Axes) pos[kv.Key] = kv.Value.GetPosition();
            InputDieTransferConfig.SetTeachingPosition(new TeachingPosition(name, pos, description));
        }
        public int MoveToTeachingPosition(string name, double vel = 0, double acc = 0, double dec = 0, double jerk = 0)
        {
            var tp = InputDieTransferConfig.GetTeachingPosition(name);
            if (tp == null) return -1;
            var (t, pz, plz) = InputDieTransferConfig.GetPositionWithOffset(name);
            int rc = 0;
            if (_toolT != null)  rc |= _toolT.MoveAbs(t,   vel > 0 ? vel : _toolT.Config.MaxVelocity,  acc > 0 ? acc : _toolT.Config.RunAcc,  dec > 0 ? dec : _toolT.Config.RunDec,  jerk > 0 ? jerk : _toolT.Config.AccJerkPercent);
            if (_pickZ != null)  rc |= _pickZ.MoveAbs(pz,  vel > 0 ? vel : _pickZ.Config.MaxVelocity,  acc > 0 ? acc : _pickZ.Config.RunAcc,  dec > 0 ? dec : _pickZ.Config.RunDec,  jerk > 0 ? jerk : _pickZ.Config.AccJerkPercent);
            if (_placeZ != null) rc |= _placeZ.MoveAbs(plz, vel > 0 ? vel : _placeZ.Config.MaxVelocity, acc > 0 ? acc : _placeZ.Config.RunAcc, dec > 0 ? dec : _placeZ.Config.RunDec, jerk > 0 ? jerk : _placeZ.Config.AccJerkPercent);
            return rc;
        }
        public bool InPosTeaching(string name)
        {
            var (t, pz, plz) = InputDieTransferConfig.GetPositionWithOffset(name);
            return InPos(_toolT, t) && InPos(_pickZ, pz) && InPos(_placeZ, plz);
        }
        public void ApplyOffset(string name, double t, double pickZ, double placeZ)
            => InputDieTransferConfig.SetOffset(name, t, pickZ, placeZ);
        #endregion

        #region Low-Level IO (Name Based + DryRun)
        public bool ReadInput(string name)
        {
            if (DryRun) { return _simInputs.TryGetValue(name, out var v) && v; }
            var hi = InputDieTransferConfig.HardInputs.FirstOrDefault(i => i.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
            if (hi == null) return false;
            var eq = Equipment.Instance; var dio = eq?.DioScan; if (dio == null) return false;
            foreach (var m in eq.UnitIO.Modules)
                if (dio.TryGetInput(m.ModuleName, hi.Disp, out var v)) return v;
            return false;
        }
        public bool WriteOutput(string name, bool on)
        {
            if (DryRun) { _simOutputs[name] = on; return true; }
            var ho = InputDieTransferConfig.HardOutputs.FirstOrDefault(o => o.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
            if (ho == null) return false;
            var eq = Equipment.Instance; var dio = eq?.DioScan; if (dio == null) return false;
            foreach (var m in eq.UnitIO.Modules)
                if (dio.WriteOutput(m.ModuleName, ho.Disp, on) == 0) return true;
            return false;
        }
        #endregion

        #region Arm Vacuum / Blow / Vent Control
        private static readonly string[] VAC_NAMES  = { InputDieTransferConfig.IO.ARM1_VAC,  InputDieTransferConfig.IO.ARM2_VAC,  InputDieTransferConfig.IO.ARM3_VAC,  InputDieTransferConfig.IO.ARM4_VAC };
        private static readonly string[] BLOW_NAMES = { InputDieTransferConfig.IO.ARM1_BLOW, InputDieTransferConfig.IO.ARM2_BLOW, InputDieTransferConfig.IO.ARM3_BLOW, InputDieTransferConfig.IO.ARM4_BLOW };
        private static readonly string[] VENT_NAMES = { InputDieTransferConfig.IO.ARM1_VENT, InputDieTransferConfig.IO.ARM2_VENT, InputDieTransferConfig.IO.ARM3_VENT, InputDieTransferConfig.IO.ARM4_VENT };

        private Vacuum[] _vacuum = new Vacuum[4];              // Vacuum + OK sensor
        public Vacuum[] _blow = new Vacuum[4];
        public Vacuum[] _vent = new Vacuum[4];

        private void BindIoDomains()
        {
            var eq = Equipment.Instance; var unit = eq?.UnitIO; if (unit == null) return;

            // Vacuum 별칭으로 조회만
            if (!IoAutoBindings.Vacuums.TryGetValue("InputDieTransferVac1", out _vacuum[0]))
            {
                Log.Write("InputDieTransfer", "BindIoDomains", "Vacuums not found: InputDieTransferVac1");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("InputDieTransferVac2", out _vacuum[1]))
            {
                Log.Write("InputDieTransfer", "BindIoDomains", "Vacuums not found: InputDieTransferVac2");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("InputDieTransferVac3", out _vacuum[2]))
            {
                Log.Write("InputDieTransfer", "BindIoDomains", "Vacuums not found: InputDieTransferVac3");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("InputDieTransferVac4", out _vacuum[3]))
            {
                Log.Write("InputDieTransfer", "BindIoDomains", "Vacuums not found: InputDieTransferVac4");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("InputDieTransferBlow1", out _blow[0]))
            {
                Log.Write("InputDieTransfer", "BindIoDomains", "Vacuums not found: InputDieTransferBlow1");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("InputDieTransferBlow2", out _blow[1]))
            {
                Log.Write("InputDieTransfer", "BindIoDomains", "Vacuums not found: InputDieTransferBlow2");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("InputDieTransferBlow3", out _blow[2]))
            {
                Log.Write("InputDieTransfer", "BindIoDomains", "Vacuums not found: InputDieTransferBlow3");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("InputDieTransferBlow4", out _blow[3]))
            {
                Log.Write("InputDieTransfer", "BindIoDomains", "Vacuums not found: InputDieTransferBlow4");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("InputDieTransferVent1", out _vent[0]))
            {
                Log.Write("InputDieTransfer", "BindIoDomains", "Vacuums not found: InputDieTransferVent1");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("InputDieTransferVent2", out _vent[1]))
            {
                Log.Write("InputDieTransfer", "BindIoDomains", "Vacuums not found: InputDieTransferVent2");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("InputDieTransferVent3", out _vent[2]))
            {
                Log.Write("InputDieTransfer", "BindIoDomains", "Vacuums not found: InputDieTransferVent3");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("InputDieTransferVent4", out _vent[3]))
            {
                Log.Write("InputDieTransfer", "BindIoDomains", "Vacuums not found: InputDieTransferVent4");
            }
        }

        // === Domain Control (표준 구동) ===
        public bool SetVacuum(int nNo, bool on)
        {
            if (_vacuum[nNo] == null) return false;
            if (on) _vacuum[nNo].On();
            else _vacuum[nNo].Off();
            return true;
        }
        public bool SetBlow(int nNo, bool on)
        {
            if (_blow[nNo] == null) return false;
            if (on) _blow[nNo].On();
            else _blow[nNo].Off();
            return true;
        }
        public bool SetVent(int nNo, bool on)
        {
            if (_vent[nNo] == null) return false;
            if (on) _vent[nNo].On();
            else _vent[nNo].Off();
            return true;
        }
        public bool AirTankPressureOk() => ReadInput(InputDieTransferConfig.IO.AIR_TANK_PRESSURE);
        public bool VacTankPressureOk() => ReadInput(InputDieTransferConfig.IO.VAC_TANK_PRESSURE);
        public bool ArmFlowOk(int armIndex)
        {
            switch (armIndex)
            {
                case 0: return ReadInput(InputDieTransferConfig.IO.ARM1_FLOW);
                case 1: return ReadInput(InputDieTransferConfig.IO.ARM2_FLOW);
                case 2: return ReadInput(InputDieTransferConfig.IO.ARM3_FLOW);
                case 3: return ReadInput(InputDieTransferConfig.IO.ARM4_FLOW);
            }
            return false;
        }
        /// //////////////////////////////////////////////////////////////////
        #endregion

        #region Lifecycle
        public override void OnRun() => base.OnRun();
        public override void OnStop() => base.OnStop();
        #endregion

        public Task<int> MovePickUpPosition()
        {
            

            return MoveTeachingPositionOnceASync((int)InputDieTransferConfig.TeachingPositionName.Pickup,false);
        }
        public Task<int> MoveTeachingPositionOnceASync(int selIndex, bool isFine)
        {
            return Task.Run(() => MoveTeachingPositionOnce(selIndex, isFine));
        }
        public bool IsInterlockOK(int selIndex)
        {
            switch((InputDieTransferConfig.TeachingPositionName)selIndex)
            {
                case InputDieTransferConfig.TeachingPositionName.Pickup:
                    return IsInterlockOKPickup();
                default:
                    return true;
                
            }
            
        }

        private bool IsInterlockOKPickup()
        {
            return true;
        }

        public int MoveTeachingPositionOnce(int selIndex, bool isFine)
        {
            if(IsInterlockOK(selIndex))
            {
                var tp = this.InputDieTransferConfig.TeachingPositions[selIndex];



                var moveResults = new List<Tuple<string, int>>();

                foreach (var kv in tp.AxisPositions)
                {


                    string axisKey = kv.Key;
                    double targetPos = kv.Value;

                    // 축 찾기: TeachingPosition.Axes 사전 우선 → 없으면 Unit.Axes에서 키 또는 Name 으로 재검색
                    MotionAxis axis = null;
                    if (tp.Axes != null && tp.Axes.TryGetValue(axisKey, out axis)) { }
                    if (axis == null && this.Axes.TryGetValue(axisKey, out var directAxis)) axis = directAxis;
                    if (axis == null)
                    {
                        // Name 매칭 시도
                        foreach (var aPair in this.Axes)
                        {
                            if (aPair.Value != null && string.Equals(aPair.Value.Name, axisKey, StringComparison.OrdinalIgnoreCase))
                            {
                                axis = aPair.Value; break;
                            }
                        }
                    }
                    if (axis == null) continue; // 해당 축 없음 → 스킵

                    // 이동 명령 전송 (비동기 실행; 완료는 WaitMoveDone 사용)
                    int rc = axis.MoveAbs(targetPos, isFine);
                    moveResults.Add(new Tuple<string, int>(axisKey, rc));
                }


                // 이동 완료 대기 (모든 축 대상으로 최대 공통 Timeout 사용: 각 axis.Setup.MoveTimeoutMs)
                int waitErrors = 0;
                foreach (var kv in tp.AxisPositions)
                {
                    MotionAxis axis = null;
                    if (tp.Axes != null && tp.Axes.TryGetValue(kv.Key, out axis)) { }
                    if (axis == null && this.Axes.TryGetValue(kv.Key, out var directAxis)) axis = directAxis;
                    if (axis == null) continue;

                    int rc = axis.WaitMoveDone(-1); // axis.Setup.MoveTimeoutMs 사용
                    if (rc != 0) waitErrors++;
                }
                if (waitErrors == 0)
                {
                    return 0;
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                return -1;
            }
            
                
        }

        public void StopTeachingPositionOnce(int selIndex)
        {
            var tp = this.InputDieTransferConfig.TeachingPositions[selIndex];



            var moveResults = new List<Tuple<string, int>>();

            foreach (var kv in tp.AxisPositions)
            {


                string axisKey = kv.Key;
                double targetPos = kv.Value;

                // 축 찾기: TeachingPosition.Axes 사전 우선 → 없으면 Unit.Axes에서 키 또는 Name 으로 재검색
                MotionAxis axis = null;
                if (tp.Axes != null && tp.Axes.TryGetValue(axisKey, out axis)) { }
                if (axis == null && this.Axes.TryGetValue(axisKey, out var directAxis)) axis = directAxis;
                if (axis == null)
                {
                    // Name 매칭 시도
                    foreach (var aPair in this.Axes)
                    {
                        if (aPair.Value != null && string.Equals(aPair.Value.Name, axisKey, StringComparison.OrdinalIgnoreCase))
                        {
                            axis = aPair.Value; break;
                        }
                    }
                }
                if (axis == null) continue; // 해당 축 없음 → 스킵

                // 이동 명령 전송 (비동기 실행; 완료는 WaitMoveDone 사용)
                int rc = axis.Stop();
                
            }
            
        }

        #region Seq 단위 동작 함수
        public int ChipPickUp()
        {
            int nRet = -1;
            /* TODO */
            return nRet;
        }

        public int RotateArm()
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
        #endregion
    }
}