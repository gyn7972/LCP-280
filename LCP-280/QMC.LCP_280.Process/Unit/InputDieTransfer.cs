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
    ///  - Tool T / Pick Z / Place Z 축 제어 + Teaching Position 및 Offset
    ///  - 4 Arm Vacuum / Blow / Vent 제어
    ///  - Air/Vac Tank Pressure / Arm Flow 등의 입력
    ///  - DryRun 시뮬레이션 지원
    ///  - OutputStage 스타일과 Region/메서드 레이아웃 통일
    /// </summary>
    public class InputDieTransfer : BaseUnit<InputDieTransferConfig>
    {
        #region Config / Teaching
        public InputDieTransferConfig InputDieTransferConfig => Config;
        
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
        public InputDieTransfer(InputDieTransferConfig config = null) : base(config ?? new InputDieTransferConfig())
        {
            AddComponents();
        }

        public override void AddComponents()
        {
            Config.LoadAndBindAxes(Equipment.Instance.AxisManager);
            Config.InitializeDefaultTeachingPositions();
            
            BindAxes();
            // (Arm IO 는 단순 DO/DI 이름 관리이므로, 별도 Cylinder/Vacuum Domain 매핑은 선택)
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
            var tp = Config.GetTeachingPosition(tpName);
            if (tp != null && tp.AxisPositions != null && tp.AxisPositions.TryGetValue(axisName, out var v)) return v;
            return 0.0;
        }
        #endregion

        #region Teaching Helpers
        public void TeachCurrentPosition(string name, string description = null)
        {
            var pos = new Dictionary<string, double>();
            foreach (var kv in Axes) pos[kv.Key] = kv.Value.GetPosition();
            Config.SetTeachingPosition(new TeachingPosition(name, pos, description));
        }
        public int MoveToTeachingPosition(string name, double vel = 0, double acc = 0, double dec = 0, double jerk = 0)
        {
            var tp = Config.GetTeachingPosition(name);
            if (tp == null) return -1;
            var (t, pz, plz) = Config.GetPositionWithOffset(name);
            int rc = 0;
            if (_toolT != null)  rc |= _toolT.MoveAbs(t,   vel > 0 ? vel : _toolT.Config.MaxVelocity,  acc > 0 ? acc : _toolT.Config.RunAcc,  dec > 0 ? dec : _toolT.Config.RunDec,  jerk > 0 ? jerk : _toolT.Config.AccJerkPercent);
            if (_pickZ != null)  rc |= _pickZ.MoveAbs(pz,  vel > 0 ? vel : _pickZ.Config.MaxVelocity,  acc > 0 ? acc : _pickZ.Config.RunAcc,  dec > 0 ? dec : _pickZ.Config.RunDec,  jerk > 0 ? jerk : _pickZ.Config.AccJerkPercent);
            if (_placeZ != null) rc |= _placeZ.MoveAbs(plz, vel > 0 ? vel : _placeZ.Config.MaxVelocity, acc > 0 ? acc : _placeZ.Config.RunAcc, dec > 0 ? dec : _placeZ.Config.RunDec, jerk > 0 ? jerk : _placeZ.Config.AccJerkPercent);
            return rc;
        }
        public bool InPosTeaching(string name)
        {
            var (t, pz, plz) = Config.GetPositionWithOffset(name);
            return InPos(_toolT, t) && InPos(_pickZ, pz) && InPos(_placeZ, plz);
        }

        /// <summary>
        /// 지정한 Teaching Position에서 특정 축만 InPosition 여부를 확인.
        /// - T / PickZ / PlaceZ 는 Offset 적용 값을 사용
        /// - 그 외 축 이름이 오면 TeachingPosition.AxisPositions 값 그대로 비교
        /// </summary>
        /// <param name="tpName">Teaching Position 이름</param>
        /// <param name="axisName">
        /// 확인할 축 키(or 이름). 예:
        ///   AxisNames.LeftToolT / AxisNames.LeftPickZ / AxisNames.LeftPlaceZ
        /// </param>
        /// <returns>true = 지정 축이 목표 위치(InPositionTolerance 내)에 있음</returns>
        public bool InPosTeachingAxis(string tpName, string axisName)
        {
            if (string.IsNullOrEmpty(tpName) || string.IsNullOrEmpty(axisName)) return false;

            var tp = Config.GetTeachingPosition(tpName);
            if (tp == null) return false;

            // 표준 3축(T / PickZ / PlaceZ) 은 Offset 반영된 위치 사용
            var (t, pz, plz) = Config.GetPositionWithOffset(tpName);
            if (string.Equals(axisName, AxisNames.LeftToolT, StringComparison.OrdinalIgnoreCase))
                return InPos(_toolT, t);
            if (string.Equals(axisName, AxisNames.LeftPickZ, StringComparison.OrdinalIgnoreCase))
                return InPos(_pickZ, pz);
            if (string.Equals(axisName, AxisNames.LeftPlaceZ, StringComparison.OrdinalIgnoreCase))
                return InPos(_placeZ, plz);

            // 기타 축 처리: TeachingPosition에 저장된 원본 값 사용 (Offset 미적용)
            MotionAxis axis = null;
            if (tp.Axes != null && tp.Axes.TryGetValue(axisName, out var direct)) axis = direct;
            if (axis == null && Axes.TryGetValue(axisName, out var unitAxis)) axis = unitAxis;
            if (axis == null)
            {
                // Name 기준 추가 검색
                foreach (var kv in Axes)
                {
                    if (kv.Value != null &&
                        string.Equals(kv.Value.Name, axisName, StringComparison.OrdinalIgnoreCase))
                    {
                        axis = kv.Value; break;
                    }
                }
            }
            if (axis == null) return false;

            double target = tp.GetAxisPosition(axisName, 0.0);
            return InPos(axis, target);
        }

        /// <summary>
        /// DieTransfer PickZ 축이 SafetyPos Teaching (Offset 적용) 위치(또는 허용오차 범위)인지 확인.
        /// Teaching 이름이 SafetyPos 없으면 SafetyZone 순으로 fallback (둘 다 없으면 false).
        /// 장치/축이 없으면 true(안전)로 간주. 필요 시 treatMissingAsSafe=false 로 변경 가능.
        /// </summary>
        /// <param name="fallbackTolerance">축 설정값을 못 가져올 때 사용할 기본 허용오차</param>
        /// <param name="useAxisInposTolerance">축 Config.InposTolerance 사용 여부</param>
        /// <param name="treatMissingAsSafe">장치/Teaching 미존재 시 true 반환할지 여부</param>
        public bool IsDieTransferPickZSafetyPos(double fallbackTolerance = 0.01,
                                                 bool useAxisInposTolerance = true,
                                                 bool treatMissingAsSafe = true)
        {
            if (PickZ == null)
                return treatMissingAsSafe;

            var cfg = InputDieTransferConfig;
            if (cfg == null) return false;

            // 우선순위: SafetyPos → SafetyZone
            string[] candidateNames =
            {
                "SafetyPos",
                InputDieTransferConfig.TeachingPositionName.SafetyZone.ToString()
            };

            string foundName = null;
            foreach (var name in candidateNames)
            {
                if (cfg.GetTeachingPosition(name) != null)
                {
                    foundName = name;
                    break;
                }
            }

            if (foundName == null)
                return treatMissingAsSafe ? true : false;

            var tp = cfg.GetTeachingPosition(foundName);
            if (tp == null) return false;

            // Offset 적용 PickZ 목표값
            var (_, pickZTarget, _) = cfg.GetPositionWithOffset(foundName);

            double cur = PickZ.GetPosition();
            double tol = useAxisInposTolerance
                ? (PickZ.Config?.InposTolerance ?? fallbackTolerance)
                : fallbackTolerance;

            // 동일위치(=InPos) 판정
            return System.Math.Abs(cur - pickZTarget) <= tol;
        }

        /// <summary>
        /// DieTransfer ToolT 축이 SafetyPos(or SafetyZone fallback) 위치인지 확인.
        /// SafetyZone Teaching에 ToolT 값이 없으면 다음 후보로 넘어감.
        /// </summary>
        public bool IsDieTransferToolTSafetyPos(double fallbackTolerance = 0.01,
                                                 bool useAxisInposTolerance = true,
                                                 bool treatMissingAsSafe = true)
        {
            if (ToolT == null)
                return treatMissingAsSafe;

            var cfg = InputDieTransferConfig;
            if (cfg == null) return false;

            string[] candidateNames =
            {
                "SafetyPos",
                InputDieTransferConfig.TeachingPositionName.SafetyZone.ToString()
            };

            string foundName = null;
            foreach (var name in candidateNames)
            {
                var tpTest = cfg.GetTeachingPosition(name);
                if (tpTest == null) continue;
                // 해당 Teaching에 ToolT 좌표가 실제 존재하는지 확인 (없으면 스킵)
                if (tpTest.AxisPositions != null &&
                    tpTest.AxisPositions.Keys.Any(k => string.Equals(k, AxisNames.LeftToolT, StringComparison.OrdinalIgnoreCase)))
                {
                    foundName = name;
                    break;
                }
            }

            if (foundName == null)
                return treatMissingAsSafe;

            var (_, _, _) = cfg.GetPositionWithOffset(foundName);
            // Offset 적용 튜플에서 t 사용
            var (tTarget, _, _) = cfg.GetPositionWithOffset(foundName);

            double cur = ToolT.GetPosition();
            double tol = useAxisInposTolerance
                ? (ToolT.Config?.InposTolerance ?? fallbackTolerance)
                : fallbackTolerance;

            return System.Math.Abs(cur - tTarget) <= tol;
        }

        /// <summary>
        /// DieTransfer PlaceZ 축이 SafetyPos(or SafetyZone fallback) 위치인지 확인.
        /// </summary>
        public bool IsDieTransferPlaceZSafetyPos(double fallbackTolerance = 0.01,
                                                  bool useAxisInposTolerance = true,
                                                  bool treatMissingAsSafe = true)
        {
            if (PlaceZ == null)
                return treatMissingAsSafe;

            var cfg = InputDieTransferConfig;
            if (cfg == null) return false;

            string[] candidateNames =
            {
                "SafetyPos",
                InputDieTransferConfig.TeachingPositionName.SafetyZone.ToString()
            };

            string foundName = null;
            foreach (var name in candidateNames)
            {
                if (cfg.GetTeachingPosition(name) != null)
                {
                    foundName = name;
                    break;
                }
            }

            if (foundName == null)
                return treatMissingAsSafe;

            var (_, _, placeZTarget) = cfg.GetPositionWithOffset(foundName);

            double cur = PlaceZ.GetPosition();
            double tol = useAxisInposTolerance
                ? (PlaceZ.Config?.InposTolerance ?? fallbackTolerance)
                : fallbackTolerance;

            return System.Math.Abs(cur - placeZTarget) <= tol;
        }



        public void ApplyOffset(string name, double t, double pickZ, double placeZ)
            => Config.SetOffset(name, t, pickZ, placeZ);
        #endregion

        #region Low-Level IO (Name Based + DryRun)
        public bool ReadInput(string name)
        {
            if (DryRun) { return _simInputs.TryGetValue(name, out var v) && v; }
            var hi = Config.HardInputs.FirstOrDefault(i => i.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
            if (hi == null) return false;
            var eq = Equipment.Instance; var dio = eq?.DioScan; if (dio == null) return false;
            foreach (var m in eq.UnitIO.Modules)
                if (dio.TryGetInput(m.ModuleName, hi.Disp, out var v)) return v;
            return false;
        }
        public bool WriteOutput(string name, bool on)
        {
            if (DryRun) { _simOutputs[name] = on; return true; }
            var ho = Config.HardOutputs.FirstOrDefault(o => o.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
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
        public override int OnRun() 
        {
            int ret = 0;

            if (this.Status == UnitRunStatus.Stop || 
                this.Status == UnitRunStatus.CycleStop)
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
            base.OnStop(); 
            return ret; 
        }
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
                var tp = this.Config.TeachingPositions[selIndex];

                var moveResults = new List<Tuple<string, int>>();

                foreach (var kv in tp.AxisPositions)
                {


                    string axisKey = kv.Key;
                    double targetPos = kv.Value;

                    // 축 찾기: TeachingPosition.Axes 우선 후 실패 시 Unit.Axes에서 키 또는 Name 기준 검색
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
                    if (axis == null) continue; // 해당 축 없으면 스킵

                    // 이동 명령 전송 (버퍼링 없음; 완료는 WaitMoveDone 단계)
                    int rc = axis.MoveAbs(targetPos, isFine);
                    moveResults.Add(new Tuple<string, int>(axisKey, rc));
                }


                // 이동 완료 대기 (각 축 내부 Timeout 사용: 예 axis.Setup.MoveTimeoutMs)
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
            var tp = this.Config.TeachingPositions[selIndex];

            var moveResults = new List<Tuple<string, int>>();

            foreach (var kv in tp.AxisPositions)
            {
                string axisKey = kv.Key;
                double targetPos = kv.Value;

                // 축 찾기: TeachingPosition.Axes 우선 후 실패 시 Unit.Axes에서 키 또는 Name 기준 검색
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
                if (axis == null) continue; // 해당 축 없으면 스킵

                // 정지 명령
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