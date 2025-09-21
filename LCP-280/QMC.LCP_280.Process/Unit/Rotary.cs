using QMC.Common;
using QMC.Common.Alarm;
using QMC.Common.Component;
using QMC.Common.IOUtil;
using QMC.Common.Motion;
using QMC.Common.Motions;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Component;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using static QMC.LCP_280.Process.Equipment;
using static QMC.LCP_280.Process.Unit.RotaryConfig.IO; // IO ???/?迭 ???? ???

namespace QMC.LCP_280.Process.Unit
{
    public class Rotary : BaseUnit<RotaryConfig>
    {
        public enum AlarmKeys
        {
            eIndexRotary = 4800,
            eRotaryNotSafe,

        }

        #region InitAlarm
        protected override void InitAlarm()
        {
            base.InitAlarm();
            AlarmInfo alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eRotaryNotSafe;
            alarm.Title = "Rorary Not Sfarety Pos.";
            alarm.Cause = "Rorary가 안전 위치가 아닙니다.\n 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Warning.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);
        }
        #endregion

        #region Unit
        InputDieTransfer InputDieTransfer { get; set; }
        IndexLoadAligner IndexLoadAligner { get; set; }
        IndexChipProbeController IndexChipProbeController { get; set; }
        IndexUnloadAligner IndexUnloadAligner { get; set; }
        OutputDieTransfer OutputDieTransfer { get; set; }
        #endregion

        private MotionAxis _axisT;
        public MotionAxis AxisT => _axisT;
        private DateTime _moveStartTime;

        public bool DryRun { get; private set; }
        public bool RequestChip { get; set; } = false;

        public void SetDryRun(bool on) => DryRun = on;
        private readonly Dictionary<string, bool> _simOutputs = new Dictionary<string, bool>(System.StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, bool> _simInputs = new Dictionary<string, bool>(System.StringComparer.OrdinalIgnoreCase);

        // ???? Safe ??? ???(??? ????)
        private static readonly string[] SafeNames = new[] { "SafetyZone", "Safe", "SasfeZone", "SAFE", "SAFEZONE", "SAFE_ZONE" };

        public Rotary(RotaryConfig config = null) : base(new RotaryConfig())
        {

            AddComponents();
        }

        public override void AddComponents()
        {
            Config.LoadAndBindAxes(Equipment.Instance.AxisManager);
            Config.InitializeDefaultTeachingPositions();

            BindAxes();
            BindIoDomains();

            var il = InterlockManager.Instance;
            il.AddAxisMustBeHomed("RotaryTHomed", _axisT, "T?? Home ??? ?? ???? ????????.");
            il.AddGlobalRule("EquipStateRunningBlock", () =>
            {
                return Equipment.Instance != null && Equipment.Instance.EqState == EquipmentState.Running
                    ? "??????? ????? ?ε??? ???? ????? ???????." : null;
            });
        }

        protected override void OnBindUnit()
        {
            base.OnBindUnit();
            InputDieTransfer = Equipment.Instance.GetUnit(UnitKeys.InputDieTransfer) as InputDieTransfer;
            IndexLoadAligner = Equipment.Instance.GetUnit(UnitKeys.IndexLoadAligner) as IndexLoadAligner;
            IndexChipProbeController = Equipment.Instance.GetUnit(UnitKeys.IndexChipProbeController) as IndexChipProbeController;
            IndexUnloadAligner = Equipment.Instance.GetUnit(UnitKeys.IndexUnloadAligner) as IndexUnloadAligner;
            OutputDieTransfer = Equipment.Instance.GetUnit(UnitKeys.OutputDieTransfer) as OutputDieTransfer;
        }

        private void BindAxes()
        {
            //AxisNames.IndexT
            var mgr = Equipment.Instance?.AxisManager;
            if (mgr == null)
            {
                Log.Write("Rotary", "[BindAxes] AxisManager null");
                return;
            }

            const string unitName = "Unit"; // Equipment???? ?? ??? ?? ????? ?????? ??????? ??
            BindAxis(mgr, unitName, AxisNames.IndexT, ref _axisT);

        }

        public int GetLoadIndexNo()
        {
            //Todo : Implement 구영남
            double dPos = AxisT.GetPosition();          // 현재 T축 위치
            double dStep = 360.0 / GetIndexCount();     // 한 소켓 간격 (45°)

            // 각도를 0~360 범위로 정규화
            dPos = (dPos % 360 + 360) % 360;

            // 소켓 번호 계산 (0~7)
            int nIndex = (int)((dPos / dStep) + 0.5) % GetIndexCount();
            return nIndex;
            //double dPos = AxisT.GetPosition();
            //double dStep = (360.0) / GetIndexCount();
            //int nIndex = (int)(((360 - dPos) / dStep) + 0.5);
            //while (nIndex < 0)
            //{
            //    nIndex += GetIndexCount();
            //}
            //return nIndex % this.GetIndexCount();
        }


        public override int OnRun()
        {
            int ret = 0;
            return ret;
        }
        public override int OnStop()
        {
            int ret = 0;
            base.OnStop();
            return ret;
        }
        protected override int OnRunReady() { return 0; }
        protected override int OnRunWork() { return 0; }
        protected override int OnRunComplete() { return 0; }

        #region Teaching
        public void TeachCurrentPosition(string name, string description = null)
        {
            var pos = new Dictionary<string, double>();
            foreach (var kv in Axes) pos[kv.Key] = kv.Value.GetPosition();
            Config.SetTeachingPosition(new TeachingPosition(name, pos, description));
        }

        public int MoveToTeachingPosition(string name, double vel = 0, double acc = 0, double dec = 0, double jerk = 0)
        {
            var tp = Config.GetTeachingPosition(name); if (tp == null) return -1;
            double t = Config.GetPositionWithOffset(name);
            if (_axisT == null) return -2;
            return _axisT.MoveAbs(t,
                vel > 0 ? vel : _axisT.Config.MaxVelocity,
                acc > 0 ? acc : _axisT.Config.RunAcc,
                dec > 0 ? dec : _axisT.Config.RunDec,
                jerk > 0 ? jerk : _axisT.Config.AccJerkPercent);
        }

        public bool InPosTeaching(string name)
        {
            double t = Config.GetPositionWithOffset(name);
            return InPos(_axisT, t);
        }

        public void ApplyOffset(string name, double deltaT) => Config.SetOffset(name, deltaT);
        #endregion

        #region Axis helpers
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
        #endregion

        #region Index Move (with Interlock)
        public bool TryMoveIndexPrev(out string reason)
        {
            return TryMoveIndexStep(-1, out reason);
        }

        public bool TryMoveIndexNext(out string reason)
        {
            return TryMoveIndexStep(+1, out reason);
        }

        private bool TryMoveIndexStep(int step, out string reason)
        {
            reason = null;
            var axis = _axisT;
            if (axis == null)
            {
                reason = "T???? ???ε????? ???????.";
                return false;
            }

            // 1) ???? Safe-Zone ?????: 4?? ?????? Safe TeachingPosition?? ???? ??
            if (!VerifyAllUnitsSafe(out reason))
                return false;

            // 2) InterlockManager ??? ???(???? + ?? ????)
            //var il = InterlockManager.Instance;
            //if (!il.ValidateAxisForHome(axis, out reason))
            //    return false;
            //if (!il.ValidateForHomeStep(new[] { axis }, out reason))
            //    return false;

            // 3) ???? ?ε??? ???
            int rc = step < 0 ? axis.MovePrevIndex() : axis.MoveNextIndex();
            if (rc != 0)
            {
                reason = $"Index ??? ????(rc={rc})";
                return false;
            }

            _moveStartTime = DateTime.Now;
            return true;
        }

        
        private bool IsIndexMoveDone()
        {
            if (AxisT == null) 
                return true;

            if (!IsAxisMoving(AxisNames.IndexT)) 
                return true;

            if ((DateTime.Now - _moveStartTime).TotalMilliseconds > AxisT.Setup.MoveTimeoutMs)
            {
                Log.Write("Rotary", "Index Move Timeout");
                return true;
            }
            return false;
        }

        // 인덱스 이동 완료 대기 (성공:0, 타임아웃:-1)
        public int WaitIndexMoveDone(int timeoutMs = -1, int pollMs = 5)
        {
            if (AxisT == null) return -1;

            if (timeoutMs <= 0)
            {
                // Setup 없으면 기본 20000
                timeoutMs = (AxisT.Setup != null && AxisT.Setup.MoveTimeoutMs > 0)
                    ? AxisT.Setup.MoveTimeoutMs
                    : 20000;
            }

            // DryRun 모드면 짧게 대기 후 OK
            if (DryRun)
            {
                Thread.Sleep(20);
                return 0;
            }

            var start = DateTime.Now;
            while (true)
            {
                if (!IsAxisMoving(AxisNames.IndexT))
                    return 0; // 완료

                if ((DateTime.Now - start).TotalMilliseconds > timeoutMs)
                {
                    Log.Write(UnitName, $"Index Move Timeout (>{timeoutMs} ms)");
                    return -1;
                }
                Thread.Sleep(pollMs);
            }
        }

        private bool VerifyAllUnitsSafe(out string reason)
        {
            reason = null;
            var eq = Equipment.Instance;
            if (eq == null || eq.Units == null) return true;

            // InputDieTransfer
            if (eq.Units.TryGetValue("InputDieTransfer", out var u3))
            {
                if (!IsUnitInSafeByConnectedAxes(u3))
                {
                    reason = "InputDieTransfer Not in Safety Zone";
                    return false;
                }
            }

            // IndexLoadAligner
            if (eq.Units.TryGetValue("IndexLoadAligner", out var u2))
            {
                if (!IsUnitInSafeByConnectedAxes(u2))
                {
                    reason = "IndexLoadAligner Not in Safety Zone";
                    return false;
                }
            }

            // IndexChipProbeController
            if (eq.Units.TryGetValue("IndexChipProbeController", out var u1))
            {
                if (!IsUnitInSafeByConnectedAxes(u1))
                {
                    reason = "IndexChipProbeController Not in Safety Zone";
                    return false;
                }
            }

            // OutputDieTransfer
            if (eq.Units.TryGetValue("OutputDieTransfer", out var u4))
            {
                if (!IsUnitInSafeByConnectedAxes(u4))
                {
                    reason = "OutputDieTransfer Not in Safety Zone";
                    return false;
                }
            }

            return true;
        }

        private bool IsUnitInSafeByConnectedAxes(object unit)
        {
            if (unit == null) 
                return true;

            // Config(BaseConfig) 획득
            //var t = unit.GetType();
            //var propConfig = t.GetProperty("Config");
            //var cfg = propConfig?.GetValue(unit) as BaseConfig;
            //if (cfg?.TeachingPositions == null) return true;
            // Config(BaseConfig) 획득
            var t = unit.GetType();
            var propConfig = t.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .FirstOrDefault(p => p.Name == "Config" && typeof(BaseConfig).IsAssignableFrom(p.PropertyType));
            var cfg = propConfig?.GetValue(unit) as BaseConfig;
            if (cfg?.TeachingPositions == null) return true;


            // 유닛 보유 축 사전(Dictionary<string, MotionAxis>) 획득
            var propAxes = t.GetProperty("Axes");
            var unitAxes = propAxes?.GetValue(unit) as System.Collections.Generic.IDictionary<string, MotionAxis>;

            foreach (var safeName in SafeNames)
            {
                var tp = cfg.TeachingPositions.FirstOrDefault(p => string.Equals(p.Name, safeName, StringComparison.OrdinalIgnoreCase));
                if (tp == null) continue;

                // TeachingPosition의 바인딩된 축 사전 (Dictionary<string, MotionAxis>) 리플렉션으로 접근
                System.Collections.Generic.IDictionary<string, MotionAxis> tpAxes = null;
                try
                {
                    var tpAxesProp = tp.GetType().GetProperty("Axes");
                    tpAxes = tpAxesProp?.GetValue(tp) as System.Collections.Generic.IDictionary<string, MotionAxis>;
                }
                catch { /* ignore */ }

                bool ok = true;
                int checkedAny = 0;

                foreach (var kv in tp.AxisPositions)
                {
                    string axisKey = kv.Key;
                    double target = kv.Value;

                    MotionAxis axis = null;

                    // 1) TeachingPosition에 바인딩된 축 우선
                    if (tpAxes != null)
                    {
                        tpAxes.TryGetValue(axisKey, out axis);
                    }

                    // 2) 유닛 보유 축에서 키/이름으로 검색
                    if (axis == null && unitAxes != null)
                    {
                        if (!unitAxes.TryGetValue(axisKey, out axis))
                        {
                            axis = unitAxes.Values.FirstOrDefault(a => a != null && string.Equals(a.Name, axisKey, StringComparison.OrdinalIgnoreCase));
                        }
                    }

                    // 연결되지 않은 축은 비교 대상에서 제외
                    if (axis == null) continue;

                    checkedAny++;
                    try
                    {
                        if (!axis.InPosition(target))
                        {
                            ok = false;
                            break;
                        }
                    }
                    catch
                    {
                        ok = false;
                        break;
                    }
                }

                // 바인딩된 축이 하나도 없으면 안전으로 간주(필요 시 false로 변경 가능)
                if (ok && (checkedAny == 0 || checkedAny > 0))
                    return true;
            }

            return false;
        }

        private bool IsUnitInSafe(System.Func<string, bool> inPosTeaching)
        {
            for (int i = 0; i < SafeNames.Length; i++)
                if (inPosTeaching(SafeNames[i])) return true;
            return false;
        }
        #endregion

        #region IO Helpers
        public bool ReadInput(string name)
        {
            if (DryRun) { bool v; return _simInputs.TryGetValue(name, out v) && v; }
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

        public bool IsOutputOn(string name)
        {
            if (DryRun) { bool v; return _simOutputs.TryGetValue(name, out v) && v; }
            var ho = Config.HardOutputs.FirstOrDefault(o => o.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
            if (ho == null) return false;
            var eq = Equipment.Instance; var dio = eq?.DioScan; if (dio == null) return false;
            foreach (var m in eq.UnitIO.Modules)
                if (dio.TryGetOutput(m.ModuleName, ho.Disp, out var v)) return v;
            return false;
        }
        #endregion

        private Vacuum[] _vacuum = new Vacuum[8];              // Vacuum + OK sensor
        public Vacuum[] _blow = new Vacuum[8];
        public Vacuum[] _vent = new Vacuum[8];

        private void BindIoDomains()
        {
            var eq = Equipment.Instance; var unit = eq?.UnitIO; if (unit == null) return;

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyVac1", out _vacuum[0]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyVac1");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyVac2", out _vacuum[1]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyVac2");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyVac3", out _vacuum[2]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyVac3");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyVac4", out _vacuum[3]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyVac4");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyVac5", out _vacuum[4]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyVac5");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyVac6", out _vacuum[5]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyVac6");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyVac7", out _vacuum[6]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyVac7");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyVac8", out _vacuum[7]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyVac8");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyBlow1", out _blow[0]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyBlow1");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyBlow2", out _blow[1]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyBlow2");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyBlow3", out _blow[2]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyBlow3");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyBlow4", out _blow[3]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyBlow4");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyBlow5", out _blow[4]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyBlow5");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyBlow6", out _blow[5]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyBlow6");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyBlow7", out _blow[6]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyBlow7");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyBlow8", out _blow[7]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyBlow8");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyVent1", out _vent[0]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyVent1");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyVent2", out _vent[1]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyVent2");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyVent3", out _vent[2]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyVent3");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyVent4", out _vent[3]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyVent4");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyVent5", out _vent[4]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyVent5");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyVent6", out _vent[5]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyVent6");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyVent7", out _vent[6]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyVent7");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyVent8", out _vent[7]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyVent8");
            }

        }

        // === Domain Control (??? ????) ===
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

        public bool SlotFlowOk(int slotIndex) => slotIndex >= 0 && slotIndex < FLOW.Length && ReadInput(FLOW[slotIndex]);

        #region Pressure
        public bool AirTankPressureOk() => ReadInput(AIR_TANK_PRESSURE);
        public bool VacTankPressureOk() => ReadInput(VAC_TANK_PRESSURE) || ReadInput(VAC_TANK_PRESSURE_LEGACY);
        #endregion


        protected override void OnMakeSequence()
        {
            base.OnMakeSequence();

            this.SequencePlayers.Add(Rotate);
            this.SequencePlayers.Add(ExecuteUnitLoadDie);
            this.SequencePlayers.Add(ExecuteUnitLoadMAlign);
            this.SequencePlayers.Add(ExecuteUnitProbe);
            this.SequencePlayers.Add(ExecuteUnitUnloadAlign);
            this.SequencePlayers.Add(ExecuteUnitUnLoadDie);

        }


        #region Seq 함수

        public int GetIndexCount()
        {
            return 8;
        }

        public int Rotate(bool isFine = false)
        {
            int nRet = 0;

            this.CurrentFunc = Rotate;

            string reason;
            if (!TryMoveIndexNext(out reason))
            {
                // 재시도 루프(로그만)
                Log.Write(UnitName, $"TryMoveIndexNext Fail: {reason}");
                Thread.Sleep(50);
                return -1;
            }

            nRet = WaitIndexMoveDone();
            if (nRet != 0)
            {
                // 필요 시 Alarm 발생 가능
                // RaiseAlarm((int)AlarmKeys.eIndexRotary, "Index Move Timeout");
                return -1;
            }


            return nRet;
        }
        //사전에 Unit 상태 및 안전 위치 확인 함수.
        public int IsExecuteUnitLoadDie()
        {
            int nRet = 0;

            //InputDieTr는 작업여부 상태신호 보자. //밖에서 확인하고 들어오게 하자.
            if (InputDieTransfer.IsWork())
            {
                return -1; // 대기 인디.
            }

            return nRet;
        }

        public int ExecuteUnitLoadDie(bool isFine = false)
        {
            int nRtn = 0;
            this.CurrentFunc = ExecuteUnitLoadDie;



            return nRtn;
        }

        public int ExecuteUnitLoadMAlign(bool isFine = false)
        {
            this.CurrentFunc = ExecuteUnitLoadMAlign;

            Task<int> task = ExecuteUnitAsyncLoadMAlign(isFine);
            while (IsEndTask(task) == false)
            {
                ExecuteUnitActionInterlockLoadMAlign(isFine);
                Thread.Sleep(1);
            }
            return task.Result;
        }
        public Task<int> ExecuteUnitAsyncLoadMAlign(bool isFine = false)
        {
            return Task.Run(() =>
            {
                OnExecuteUnitLoadMAlign(isFine);
                return 0;
            });
        }
        public int OnExecuteUnitLoadMAlign(bool isFine = false)
        {
            int nRet = 0;

            RequestChip = true; // InputDieTransfer에 Chip 요청 상태로 변경.

            nRet &= IndexLoadAligner.AlignSocketOnceReady();
            if (nRet != 0)
            {
                Log.Write(UnitName, "ExecuteUnitAction Ready Fail");
                return -1;
            }

            nRet &= IndexLoadAligner.AlignSocketOnce();
            if (nRet != 0)
            {
                Log.Write(UnitName, "ExecuteUnitAction Fail");
                return -1;
            }

            return nRet;
        }
        public int ExecuteUnitActionInterlockLoadMAlign(bool isFine = false)
        {
            int nRet = 0;


            return nRet;
        }

        public int ExecuteUnitProbe(bool isFine = false)
        {
            this.CurrentFunc = ExecuteUnitProbe;

            Task<int> task = ExecuteUnitAsyncProbe(isFine);
            while (IsEndTask(task) == false)
            {
                ExecuteUnitActionInterlockProbe(isFine);
                Thread.Sleep(1);
            }
            return task.Result;
        }
        public Task<int> ExecuteUnitAsyncProbe(bool isFine = false)
        {
            return Task.Run(() =>
            {
                OnExecuteUnitProbe(isFine);
                return 0;
            });
        }
        public int OnExecuteUnitProbe(bool isFine = false)
        {
            int nRet = 0;

            RequestChip = true; // InputDieTransfer에 Chip 요청 상태로 변경.

            nRet &= IndexChipProbeController.ContactReady();
            nRet &= IndexUnloadAligner.AlignSocketOnceReady();
            if (nRet != 0)
            {
                Log.Write(UnitName, "ExecuteUnitAction Ready Fail");
                return -1;
            }

            nRet &= IndexChipProbeController.ContactBottomOrTop();
            nRet &= IndexUnloadAligner.AlignSocketOnce();
            if (nRet != 0)
            {
                Log.Write(UnitName, "ExecuteUnitAction Fail");
                return -1;
            }

            return nRet;
        }
        public int ExecuteUnitActionInterlockProbe(bool isFine = false)
        {
            int nRet = 0;


            return nRet;
        }

        public int ExecuteUnitUnloadAlign(bool isFine = false)
        {
            this.CurrentFunc = ExecuteUnitUnloadAlign;

            Task<int> task = ExecuteUnitAsyncUnloadAlign(isFine);
            while (IsEndTask(task) == false)
            {
                ExecuteUnitInterlockUnloadAlign(isFine);
                Thread.Sleep(1);
            }
            return task.Result;
        }
        public Task<int> ExecuteUnitAsyncUnloadAlign(bool isFine = false)
        {
            return Task.Run(() =>
            {
                OnExecuteUnitUnloadAlign(isFine);
                return 0;
            });
        }
        public int OnExecuteUnitUnloadAlign(bool isFine = false)
        {
            int nRet = 0;

            RequestChip = true; // InputDieTransfer에 Chip 요청 상태로 변경.

            nRet &= IndexUnloadAligner.AlignSocketOnceReady();
            if (nRet != 0)
            {
                Log.Write(UnitName, "ExecuteUnitAction Ready Fail");
                return -1;
            }

            nRet &= IndexUnloadAligner.AlignSocketOnce();
            if (nRet != 0)
            {
                Log.Write(UnitName, "ExecuteUnitAction Fail");
                return -1;
            }

            return nRet;
        }
        public int ExecuteUnitInterlockUnloadAlign(bool isFine = false)
        {
            int nRet = 0;


            return nRet;
        }

        public int ExecuteUnitUnLoadDie(bool isFine = false)
        {
            int nRtn = 0;
            this.CurrentFunc = ExecuteUnitUnLoadDie;



            return nRtn;
        }

        

        
        #endregion
    }
}