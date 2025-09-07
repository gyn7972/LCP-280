using System;
using System.Linq;
using QMC.Common;
using QMC.Common.Sequence;
using QMC.Common.Alarm;
using QMC.LCP_280.Process.Unit;

namespace QMC.LCP_280.Process.Component
{
    /// <summary>
    /// Input Die Transfer Chip Down (Place to socket) Sequence.
    /// - Port from legacy Head Place logic (Vac Off -> Vent -> Blow -> Up / variations).
    /// - Axis: Pick Z (InputDieTransfer.PickZ)
    /// - IO: Arm Vacuum / Vent / Blow (indexed) via helper methods in InputDieTransfer.
    /// - Supports 3 simplified modes:
    ///   FullCycle : Down -> VacuumOff -> VentPulse -> Blow -> Up -> VacuumRestore
    ///   DownAndVacuum : Down -> VacuumOff only (no vent/blow) -> Up
    ///   DownInposVacuum : Down -> (stay) -> VacuumOff + Blow simultaneously -> Up -> VacuumRestore
    /// </summary>
    internal class SeqInputDieTransferChipDown : SequenceBase
    {
        #region Mode / Steps
        public enum PlaceMode { FullCycle, DownAndVacuum, DownInposVacuum }
        public enum Step
        {
            Idle = 0,
            Init,
            Move_Down, Move_Down_Wait,
            // Common after in-position
            Vacuum_Off, Vacuum_Off_Delay,
            Vent_On, Vent_On_Delay,
            Vent_Off, Vent_Off_Delay,
            Blow_On, Blow_On_Delay,
            Move_Up, Move_Up_Wait,
            Blow_Off, Blow_Off_Delay,
            Vacuum_Restore,
            Finish,
            Error
        }
        #endregion

        #region Alarm
        private enum AlarmKey
        {
            First = 47100,
            AxisMoveTimeout,
            PositionInvalid,
            GenericError
        }
        private readonly System.Collections.Generic.Dictionary<int, AlarmInfo> _alarms = new System.Collections.Generic.Dictionary<int, AlarmInfo>();
        private bool _alarmsInitialized;
        private void InitAlarms()
        {
            if (_alarmsInitialized) return; _alarmsInitialized = true;
            AddAlarm(AlarmKey.AxisMoveTimeout, "PickZ Move Timeout", "Pick Z 축 이동 타임아웃", "Error");
            AddAlarm(AlarmKey.PositionInvalid, "Position Invalid", "티칭 위치 오류", "Error");
            AddAlarm(AlarmKey.GenericError, "Seq Generic Error", "일반 오류", "Error");
        }
        private void AddAlarm(AlarmKey key, string title, string cause, string grade)
        {
            _alarms[(int)key] = new AlarmInfo
            {
                Code = (int)key,
                Title = title,
                Cause = cause,
                Source = Name,
                Grade = grade,
                GeneratedTime = DateTime.Now
            };
        }
        private void PostAlarm(AlarmKey key, string detail = null)
        {
            try
            {
                if (!_alarmsInitialized) InitAlarms();
                if (!_alarms.TryGetValue((int)key, out var info)) return;
                if (AlarmManager.Instance.Alarms.Any(a => a.Code == info.Code)) return;
                var clone = new AlarmInfo
                {
                    Code = info.Code,
                    Title = info.Title,
                    Cause = string.IsNullOrEmpty(detail) ? info.Cause : info.Cause + " | " + detail,
                    Source = info.Source,
                    Grade = info.Grade,
                    GeneratedTime = DateTime.Now
                };
                AlarmManager.Instance.ShowAlarm(clone);
                Log.Write(Name, "Alarm", Name, $"AlarmPost Code={clone.Code} Title={clone.Title} Cause={clone.Cause}");
            }
            catch (Exception ex) { Log.Write(Name, "AlarmError", Name, ex.Message); }
        }
        #endregion

        #region Fields
        private readonly InputDieTransfer _unit;
        private Step _step = Step.Idle;
        private Step _prevLoggedStep = Step.Idle;
        private DateTime _tick;
        private double _targetDownPos;
        private double _targetUpPos;
        private int _armIndex;
        private PlaceMode _mode;

        // Flags
        public bool PlaceCompleted { get; private set; }

        // Position source
        public string UpTeachingName { get; set; } = "Ready";  // safe up
        public string DownTeachingName { get; set; } = null;    // if null derive from Up - DownOffset
        public double? UpPositionOverride { get; set; }
        public double? DownPositionOverride { get; set; }
        public double DownRelativeOffset { get; set; } = 2.0;   // mm downward from Up if no down teaching

        // Timing (ms)
        public int MoveTimeoutMs { get; set; } = 6000;
        public int VacuumOffDelayMs { get; set; } = 80;         // wait after vacuum off before vent
        public int VentOnDelayMs { get; set; } = 60;            // time vent open
        public int VentOffDelayMs { get; set; } = 50;           // settle after vent close
        public int BlowDelayMs { get; set; } = 120;             // blow on duration OR hold time before up (mode dependent)
        public int BlowOffDelayMs { get; set; } = 50;           // delay after blow off
        public int UpExtraDelayMs { get; set; } = 0;            // optional wait after reaching up (FullCycle)

        // Mode specific toggles
        public PlaceMode ModeSetting
        {
            get => _mode;
            set => _mode = value;
        }

        private bool IsDryRun => _unit?.DryRun ?? false;
        public Step CurrentStep => _step;
        #endregion

        public SeqInputDieTransferChipDown(InputDieTransfer unit, int armIndex = 0, PlaceMode mode = PlaceMode.FullCycle) : base("SeqInputDieTransferChipDown")
        {
            _unit = unit ?? throw new ArgumentNullException(nameof(unit));
            _armIndex = armIndex;
            _mode = mode;
            InitAlarms();
        }

        public void SetArmIndex(int idx) { if (idx >= 0) _armIndex = idx; }
        public void SetMode(PlaceMode m) => _mode = m;

        public bool Start(Step first = Step.Init)
        {
            _step = first;
            _prevLoggedStep = Step.Idle;
            _tick = DateTime.UtcNow;
            PlaceCompleted = false;
            return base.Start(0);
        }

        private bool Timeout(int ms) => (DateTime.UtcNow - _tick).TotalMilliseconds >= ms;
        private string StepCode(Step s) => ((int)s) + ":" + s;

        private void GoError(string msg, AlarmKey key)
        {
            try { Log.Write(Name, "Error", Name, msg); } catch { }
            PostAlarm(key, $"Step={_step} Msg={msg} Mode={_mode}");
            _step = Step.Error;
        }

        private bool ResolvePositions()
        {
            try
            {
                var pickZ = _unit.PickZ;
                if (pickZ == null) { GoError("PickZ axis null", AlarmKey.GenericError); return false; }

                if (UpPositionOverride.HasValue) _targetUpPos = UpPositionOverride.Value;
                else if (!string.IsNullOrEmpty(UpTeachingName)) _targetUpPos = _unit.GetTP(UpTeachingName, pickZ.Setup.Name);
                else { GoError("Up position undefined", AlarmKey.PositionInvalid); return false; }

                if (DownPositionOverride.HasValue) _targetDownPos = DownPositionOverride.Value;
                else if (!string.IsNullOrEmpty(DownTeachingName)) _targetDownPos = _unit.GetTP(DownTeachingName, pickZ.Setup.Name);
                else _targetDownPos = _targetUpPos - DownRelativeOffset; // assume negative direction is down
                return true;
            }
            catch (Exception ex)
            {
                GoError("ResolvePositions ex=" + ex.Message, AlarmKey.PositionInvalid);
                return false;
            }
        }

        protected override int ExecuteStep(int current, System.Threading.CancellationToken ct)
        {
            var before = _step;
            switch (_step)
            {
                case Step.Idle: return -1;

                case Step.Init:
                    if (!ResolvePositions()) break;
                    _unit.SetArmVent(_armIndex, false); // ensure closed
                    _unit.SetArmBlow(_armIndex, false);
                    _unit.SetArmVac(_armIndex, true);   // still holding before release
                    _unit.PlaceZ?.MoveAbs(_targetDownPos, _unit.PlaceZ.Config.MaxVelocity, _unit.PlaceZ.Config.RunAcc, _unit.PlaceZ.Config.RunDec, _unit.PlaceZ.Config.AccJerkPercent);
                    _step = Step.Move_Down_Wait; _tick = DateTime.UtcNow; break;

                case Step.Move_Down_Wait:
                    if (IsDryRun || _unit.PlaceZ?.InPosition(_targetDownPos) == true)
                    {
                        // Branch by mode
                        if (_mode == PlaceMode.DownAndVacuum)
                        {
                            _unit.SetArmVac(_armIndex, false); // release
                            _step = Step.Move_Up; // directly up
                            _unit.PlaceZ?.MoveAbs(_targetUpPos, _unit.PlaceZ.Config.MaxVelocity, _unit.PlaceZ.Config.RunAcc, _unit.PlaceZ.Config.RunDec, _unit.PlaceZ.Config.AccJerkPercent);
                            _tick = DateTime.UtcNow;
                        }
                        else if (_mode == PlaceMode.DownInposVacuum)
                        {
                            _unit.SetArmVac(_armIndex, false); // vacuum off
                            _unit.SetArmBlow(_armIndex, true);  // blow on at same time
                            _step = Step.Blow_On_Delay; _tick = DateTime.UtcNow;
                        }
                        else // FullCycle
                        {
                            _unit.SetArmVac(_armIndex, false);
                            _step = Step.Vacuum_Off_Delay; _tick = DateTime.UtcNow;
                        }
                    }
                    else if (Timeout(MoveTimeoutMs)) GoError("Down move timeout", AlarmKey.AxisMoveTimeout);
                    break;

                // ========== FullCycle path ==========
                case Step.Vacuum_Off_Delay:
                    if (IsDryRun || Timeout(VacuumOffDelayMs)) { _unit.SetArmVent(_armIndex, true); _step = Step.Vent_On_Delay; _tick = DateTime.UtcNow; }
                    break;

                case Step.Vent_On_Delay:
                    if (IsDryRun || Timeout(VentOnDelayMs)) { _unit.SetArmVent(_armIndex, false); _step = Step.Vent_Off_Delay; _tick = DateTime.UtcNow; }
                    break;

                case Step.Vent_Off_Delay:
                    if (IsDryRun || Timeout(VentOffDelayMs)) { _unit.SetArmBlow(_armIndex, true); _step = Step.Blow_On_Delay; _tick = DateTime.UtcNow; }
                    break;

                case Step.Blow_On_Delay:
                    if (IsDryRun || Timeout(BlowDelayMs))
                    {
                        _unit.PlaceZ?.MoveAbs(_targetUpPos, _unit.PlaceZ.Config.MaxVelocity, _unit.PlaceZ.Config.RunAcc, _unit.PlaceZ.Config.RunDec, _unit.PlaceZ.Config.AccJerkPercent);
                        _step = Step.Move_Up_Wait; _tick = DateTime.UtcNow;
                        if (_mode == PlaceMode.DownInposVacuum)
                        {
                            // In this mode blow already ON; keep until near up then off
                        }
                    }
                    break;

                // ====================================
                case Step.Move_Up_Wait:
                    if (IsDryRun || _unit.PlaceZ?.InPosition(_targetUpPos) == true)
                    {
                        if (_mode == PlaceMode.DownInposVacuum)
                        {
                            _unit.SetArmBlow(_armIndex, false); // blow off immediately
                            _step = Step.Vacuum_Restore; _tick = DateTime.UtcNow; break;
                        }
                        if (_mode == PlaceMode.DownAndVacuum)
                        {
                            _step = Step.Vacuum_Restore; _tick = DateTime.UtcNow; break;
                        }
                        // Full cycle path -> blow off first
                        _step = Step.Blow_Off; _tick = DateTime.UtcNow;
                    }
                    else if (Timeout(MoveTimeoutMs)) GoError("Up move timeout", AlarmKey.AxisMoveTimeout);
                    break;

                case Step.Blow_Off:
                    _unit.SetArmBlow(_armIndex, false);
                    _step = Step.Blow_Off_Delay; _tick = DateTime.UtcNow; break;

                case Step.Blow_Off_Delay:
                    if (IsDryRun || Timeout(BlowOffDelayMs)) { _step = Step.Vacuum_Restore; _tick = DateTime.UtcNow; }
                    break;

                case Step.Vacuum_Restore:
                    // Restore vacuum ON (idle state expectation)
                    _unit.SetArmVac(_armIndex, true);
                    if (IsDryRun || Timeout(UpExtraDelayMs)) { PlaceCompleted = true; _step = Step.Finish; }
                    break;

                case Step.Finish: return -1;
                case Step.Error: return -1;
            }

            if (before != _step && _prevLoggedStep != _step)
            {
                try { Log.Write(Name, "Step", Name, $"StepChange {StepCode(before)} -> {StepCode(_step)} Arm={_armIndex} Mode={_mode}"); } catch { }
                _prevLoggedStep = _step;
            }
            return current + 1;
        }
    }
}
