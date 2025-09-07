using System;
using System.Linq;
using QMC.Common;
using QMC.Common.Sequence;
using QMC.Common.Alarm;
using QMC.LCP_280.Process.Unit;

namespace QMC.LCP_280.Process.Component
{
    /// <summary>
    /// Input Die Transfer Pick (Chip Up) Sequence.
    /// - Simplified port of legacy Head Pick logic (Vent -> Vacuum -> Down -> Pick -> Up).
    /// - Axis used: Pick Z (InputDieTransfer.PickZ)
    /// - IO: Arm Vent / Vacuum (indexed)
    /// - Provides timing parameters & basic alarms.
    /// </summary>
    internal class SeqInputDieTransferChipUp : SequenceBase
    {
        #region Steps
        public enum Step
        {
            Idle = 0,
            Init,
            Vent_Close, Vent_Close_Delay,
            Vacuum_On, Vacuum_On_Delay,
            Move_Down, Move_Down_Wait,
            Stabilize_Down,
            Move_Up, Move_Up_Wait,
            Finish,
            Error
        }
        #endregion

        #region Alarm
        private enum AlarmKey
        {
            First = 47000,
            AxisMoveTimeout,
            VacuumTimeout,
            PositionInvalid,
            GenericError
        }
        private readonly System.Collections.Generic.Dictionary<int, AlarmInfo> _alarms = new System.Collections.Generic.Dictionary<int, AlarmInfo>();
        private bool _alarmsInitialized;
        private void InitAlarms()
        {
            if (_alarmsInitialized) return; _alarmsInitialized = true;
            AddAlarm(AlarmKey.AxisMoveTimeout, "PickZ Move Timeout", "Pick Z 축 이동 타임아웃", "Error");
            AddAlarm(AlarmKey.VacuumTimeout, "Vacuum Timeout", "진공 형성 타임아웃", "Error");
            AddAlarm(AlarmKey.PositionInvalid, "Position Invalid", "티칭 위치 혹은 목표 위치 오류", "Error");
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
                if (AlarmManager.Instance.Alarms.Any(a => a.Code == info.Code)) return; // prevent duplicates
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
        private bool _vacOnIssued;
        private bool _ventClosedIssued;
        private int _armIndex;

        // Parameters (ms / mm)
        public int VentCloseDelayMs { get; set; } = 80;          // Legacy VentOffDelay (short)
        public int VacuumDelayMs { get; set; } = 120;            // Time to allow vacuum stabilization after On
        public int DownStabilizeDelayMs { get; set; } = 50;      // Hold time at down before up
        public int MoveTimeoutMs { get; set; } = 6000;
        public int VacuumTimeoutMs { get; set; } = 1500;         // Optional wait if sensor later (placeholder)

        // Teaching position names (optional). If not set, numeric positions can be provided
        public string UpTeachingName { get; set; } = "Ready";    // Default safe/ready pos
        public string DownTeachingName { get; set; } = null;     // If null use DownOffset logic

        // Direct positions override (nullable)
        public double? UpPositionOverride { get; set; }
        public double? DownPositionOverride { get; set; }

        // Result flags
        public bool PickCompleted { get; private set; }

        private bool IsDryRun => _unit?.DryRun ?? false;
        public Step CurrentStep => _step;
        #endregion

        public SeqInputDieTransferChipUp(InputDieTransfer unit, int armIndex = 0) : base("SeqInputDieTransferChipUp")
        {
            _unit = unit ?? throw new ArgumentNullException(nameof(unit));
            _armIndex = armIndex;
            InitAlarms();
        }

        public void SetArmIndex(int idx) { if (idx >= 0) _armIndex = idx; }

        public bool Start(Step first = Step.Init)
        {
            _step = first;
            _prevLoggedStep = Step.Idle;
            _tick = DateTime.UtcNow;
            PickCompleted = false;
            _vacOnIssued = false; _ventClosedIssued = false;
            return base.Start(0);
        }

        private bool Timeout(int ms) => (DateTime.UtcNow - _tick).TotalMilliseconds >= ms;
        private string StepCode(Step s) => ((int)s) + ":" + s;

        private void GoError(string msg, AlarmKey key)
        {
            try { Log.Write(Name, "Error", Name, msg); } catch { }
            PostAlarm(key, $"Step={_step} Msg={msg}");
            _step = Step.Error;
        }

        private bool ResolvePositions()
        {
            try
            {
                var pickZ = _unit.PickZ;
                if (pickZ == null) { GoError("PickZ axis null", AlarmKey.GenericError); return false; }

                // Up
                if (UpPositionOverride.HasValue) _targetUpPos = UpPositionOverride.Value;
                else if (!string.IsNullOrEmpty(UpTeachingName)) _targetUpPos = _unit.GetTP(UpTeachingName, pickZ.Setup.Name);
                else { GoError("Up position undefined", AlarmKey.PositionInvalid); return false; }

                // Down
                if (DownPositionOverride.HasValue) _targetDownPos = DownPositionOverride.Value;
                else if (!string.IsNullOrEmpty(DownTeachingName)) _targetDownPos = _unit.GetTP(DownTeachingName, pickZ.Setup.Name);
                else
                {
                    // Fallback: relative offset (simple example) -> go 2mm down from up
                    _targetDownPos = _targetUpPos - 2.0; // negative direction assumed down
                }
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
                    // Ensure vent closed state first
                    _step = Step.Vent_Close; _tick = DateTime.UtcNow; break;

                case Step.Vent_Close:
                    if (!_ventClosedIssued)
                    {
                        _unit.SetArmVent(_armIndex, false); // Vent Close
                        _ventClosedIssued = true;
                        _tick = DateTime.UtcNow;
                    }
                    _step = Step.Vent_Close_Delay; break;

                case Step.Vent_Close_Delay:
                    if (IsDryRun || Timeout(VentCloseDelayMs)) { _step = Step.Vacuum_On; _tick = DateTime.UtcNow; }
                    break;

                case Step.Vacuum_On:
                    if (!_vacOnIssued)
                    {
                        _unit.SetArmVac(_armIndex, true);
                        _vacOnIssued = true;
                        _tick = DateTime.UtcNow;
                    }
                    _step = Step.Vacuum_On_Delay; break;

                case Step.Vacuum_On_Delay:
                    if (IsDryRun || Timeout(VacuumDelayMs)) { _step = Step.Move_Down; _tick = DateTime.UtcNow; }
                    else if (Timeout(VacuumTimeoutMs)) GoError("Vacuum timeout", AlarmKey.VacuumTimeout);
                    break;

                case Step.Move_Down:
                    _unit.PickZ?.MoveAbs(_targetDownPos, _unit.PickZ.Config.MaxVelocity, _unit.PickZ.Config.RunAcc, _unit.PickZ.Config.RunDec, _unit.PickZ.Config.AccJerkPercent);
                    _step = Step.Move_Down_Wait; _tick = DateTime.UtcNow; break;

                case Step.Move_Down_Wait:
                    if (IsDryRun || _unit.PickZ?.InPosition(_targetDownPos) == true)
                    { _step = Step.Stabilize_Down; _tick = DateTime.UtcNow; }
                    else if (Timeout(MoveTimeoutMs)) GoError("Down move timeout", AlarmKey.AxisMoveTimeout);
                    break;

                case Step.Stabilize_Down:
                    if (IsDryRun || Timeout(DownStabilizeDelayMs)) { _step = Step.Move_Up; _tick = DateTime.UtcNow; }
                    break;

                case Step.Move_Up:
                    _unit.PickZ?.MoveAbs(_targetUpPos, _unit.PickZ.Config.MaxVelocity, _unit.PickZ.Config.RunAcc, _unit.PickZ.Config.RunDec, _unit.PickZ.Config.AccJerkPercent);
                    _step = Step.Move_Up_Wait; _tick = DateTime.UtcNow; break;

                case Step.Move_Up_Wait:
                    if (IsDryRun || _unit.PickZ?.InPosition(_targetUpPos) == true)
                    { PickCompleted = true; _step = Step.Finish; _tick = DateTime.UtcNow; }
                    else if (Timeout(MoveTimeoutMs)) GoError("Up move timeout", AlarmKey.AxisMoveTimeout);
                    break;

                case Step.Finish:
                    return -1;
                case Step.Error:
                    return -1;
            }

            if (before != _step && _prevLoggedStep != _step)
            {
                try { Log.Write(Name, "Step", Name, $"StepChange {StepCode(before)} -> {StepCode(_step)} Arm={_armIndex}"); } catch { }
                _prevLoggedStep = _step;
            }
            return current + 1;
        }
    }
}
