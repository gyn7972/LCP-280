using Newtonsoft.Json;
using QMC.Common;
using QMC.Common.Component;
using QMC.Common.IOUtil;
using QMC.Common.Motion;
using QMC.Common.Motions;
using QMC.Common.Sequence;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Component;
using System.Collections.Generic;
using System.Linq;

namespace QMC.LCP_280.Process.Unit
{
    public class InputStage : BaseUnit
    {
        public InputStageConfig InputStageConfig { get; private set; }
        public List<TeachingPosition> TeachingPositions { get; private set; } = new List<TeachingPosition>();

        // 메인 모드 enum (C++ TWaferStageMode 대응)
        public enum StageMode
        {
            Stop,
            Loading,
            Clamp,
            FileReading,
            Align,
            Scan,
            MapMerge,
            Unloading,
            PickUp,
            WorkingPosition,
            ChipAlign,
            Execute,
            LoadingPosition,
            CenterPosition,
            AlignPosition,
            RecipeChange
        }

        private InputStageSequence _sequence; // 단일 시퀀스 인스턴스
        public StageMode CurrentMode => _sequence == null ? StageMode.Stop : (StageMode)_sequence.CurrentMode;

        public InputStage(InputStageConfig config = null)
            : base("InputStageConfig")
        {
            InputStageConfig = config ?? new InputStageConfig();
            AddComponents();
        }

        public override void AddComponents()
        {
            // 축 바인딩까지 포함해서 불러오기
            InputStageConfig.LoadAndBindAxes(Equipment.Instance.AxisManager);
            InputStageConfig.InitializeDefaultTeachingPositions();

            // TeachingPosition에 Axis 바인딩
            TeachingPositions.Clear();
            foreach (var tp in InputStageConfig.TeachingPositions)
                TeachingPositions.Add(tp);
            BindAxes();
            BindIoDomains();
        }

        public override void OnRun() => base.OnRun();

        public override void OnStop()
        {
            StopSequence();
            base.OnStop();
        }

        // 시퀀스 시작
        public bool StartSequence(StageMode mode)
        {
            StopSequence();
            _sequence = new InputStageSequence(this);
            _sequence.StateChanged += (s, o, n) => { /* 로깅/이벤트 */ };
            _sequence.ErrorOccurred += (s, ex) => { /* 에러 처리 */ };
            return _sequence.Start((InputStageSequence.Step)mode);
        }

        public void StopSequence()
        {
            if (_sequence != null)
            {
                _sequence.Stop();
                _sequence.Dispose();
                _sequence = null;
            }
        }

        public void RecoverSequence()
        {
            _sequence?.Recover();
        }

        public void PauseSequence() => _sequence?.Pause();
        public void ResumeSequence() => _sequence?.Resume();

        public bool IsSequenceRunning => _sequence != null && _sequence.IsRunning;

        public void TeachCurrentPosition(string positionName, string description = null)
        {
            var axisPositions = new Dictionary<string, double>();
            foreach (var axisPair in Axes)
            {
                axisPositions[axisPair.Key] = axisPair.Value.GetPosition();
            }
            var tp = new TeachingPosition(positionName, axisPositions, description);
            InputStageConfig.SetTeachingPosition(tp);
        }

        public int MoveToTeachingPosition(string positionName, double vel = 5, double acc = 10, double dec = 10, double jerk = 50)
        {
            var tp = InputStageConfig.GetTeachingPosition(positionName);
            if (tp == null) return -1;

            int result = 0;
            foreach (var axisKey in tp.AxisPositions.Keys)
            {
                if (Axes.TryGetValue(axisKey, out var axis))
                {
                    double pos = tp.AxisPositions[axisKey];
                    int r = axis.MoveAbs(pos, vel, acc, dec, jerk);
                    if (r != 0) result = r; // 마지막 에러 반환
                }
            }
            return result;
        }

        #region Axis / IO Helper (extracted for sequence reuse)
        private MotionAxis _axX, _axY, _axT;
        public MotionAxis AxisX => _axX; // expose for sequence
        public MotionAxis AxisY => _axY;
        public MotionAxis AxisT => _axT;

        private void BindAxes()
        {
            Axes.TryGetValue("Wafer Stage X Axis", out _axX);
            Axes.TryGetValue("Wafer Stage Y Axis", out _axY);
            Axes.TryGetValue("Wafer Stage T Axis", out _axT);
        }

        public double GetTP(string tpName, string axisName)
        {
            var tp = InputStageConfig.GetTeachingPosition(tpName);
            if (tp != null && tp.AxisPositions != null && tp.AxisPositions.TryGetValue(axisName, out var v)) return v;
            return 0.0;
        }

        public void MoveAxisOnce(MotionAxis ax, double target)
        {
            if (ax == null) return;
            // 목표와 충분히 차이날 때만 이동 명령 (InposTolerance 3배 이상 차이)
            if (System.Math.Abs(ax.GetPosition() - target) > ax.Config.InposTolerance * 3)
                ax.MoveAbs(target, ax.Config.MaxVelocity, ax.Config.RunAcc, ax.Config.RunDec, ax.Config.AccJerkPercent);
        }

        public bool InPos(MotionAxis ax, double target)
        {
            if (ax == null) return true;
            return ax.InPosition(target);
        }

        public bool ReadInput(string name)
        {
            var hi = InputStageConfig.HardInputs.FirstOrDefault(i => i.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
            if (hi == null) return false;
            var eq = Equipment.Instance; var dio = eq?.DioScan; if (dio == null) return false;
            foreach (var m in eq.UnitIO.Modules)
                if (dio.TryGetInput(m.ModuleName, hi.Disp, out var v)) return v;
            return false;
        }

        public bool WriteOutput(string name, bool on)
        {
            var ho = InputStageConfig.HardOutputs.FirstOrDefault(o => o.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
            if (ho == null) return false;
            var eq = Equipment.Instance; var dio = eq?.DioScan; if (dio == null) return false;
            foreach (var m in eq.UnitIO.Modules)
                if (dio.WriteOutput(m.ModuleName, ho.Disp, on) == 0) return true;
            return false;
        }
        #endregion

        #region IO Domain (Cylinder / Vacuum)
        private Cylinder _clampLiftCylinder;            // UP/DOWN
        private Cylinder _expanderCylinder;             // EXPANDER UP/DOWN
        private Vacuum _ejectorVacuum;                  // VACUUM

        // Hard IO 이름 상수 (Config와 동일)
        private const string NAME_CLAMP_UP = "WAFER STAGE CLAMP UP";
        private const string NAME_CLAMP_DOWN = "WAFER STAGE CLAMP DOWN";
        private const string NAME_CLAMP = "WAFER STAGE CLAMP";
        private const string NAME_CLAMP_UN = "WAFER STAGE UNCLAMP";
        private const string NAME_EXP_UP = "WAFER STAGE EXPANDER UP";
        private const string NAME_EXP_DOWN = "WAFER STAGE EXPANDER DOWN";
        private const string NAME_VAC_OUT = "EJECTOR VACUUM";
        private const string NAME_VAC_OK = "EJECTOR VACUUM CHECK";
        private const string NAME_RING0 = "WAFER STAGE RING CHECK 0";
        private const string NAME_RING1 = "WAFER STAGE RING CHECK 1";
        //private static readonly HardInputDef[] _hardInputs = new[]
        //{
        //    new HardInputDef { No = 1, Name = "WAFER STAGE RING CHECK 0",  Disp = "X025" },
        //    new HardInputDef { No = 2, Name = "WAFER STAGE RING CHECK 1",  Disp = "X026" },
        //    new HardInputDef { No = 3, Name = "WAFER STAGE CLAMP DOWN",    Disp = "X027" },
        //    new HardInputDef { No = 4, Name = "WAFER STAGE CLAMP",         Disp = "X028" },
        //    new HardInputDef { No = 5, Name = "WAFER STAGE EXPANDER UP",   Disp = "X029" },
        //    new HardInputDef { No = 6, Name = "WAFER STAGE EXPANDER DOWN", Disp = "X030" },
        //    new HardInputDef { No = 7, Name = "EJECTOR VACUUM CHECK",      Disp = "X031" },
        //};
        //private static readonly HardOutputDef[] _hardOutputs = new[]
        //{
        //    new HardOutputDef { No = 1, Name = "WAFER STAGE CLAMP UP",      Disp = "Y020" },
        //    new HardOutputDef { No = 2, Name = "WAFER STAGE CLAMP DOWN",    Disp = "Y021" },
        //    new HardOutputDef { No = 3, Name = "WAFER STAGE CLAMP",         Disp = "Y022" },
        //    new HardOutputDef { No = 4, Name = "WAFER STAGE UNCLAMP",       Disp = "Y023" },
        //    new HardOutputDef { No = 5, Name = "WAFER STAGE EXPANDER UP",   Disp = "Y024" },
        //    new HardOutputDef { No = 6, Name = "WAFER STAGE EXPANDER DOWN", Disp = "Y025" },
        //    new HardOutputDef { No = 7, Name = "EJECTOR VACUUM",            Disp = "Y038" },
        //};

        private void BindIoDomains()
        {
            var eq = Equipment.Instance; var unit = eq?.UnitIO; if (unit == null) return;

            // MapByName: 채널 이름 기반 키 매핑 (중복 호출 안전)
            // Clamp Lift Cylinder (UP/DOWN outputs + Clamp/Clamp Down inputs 활용 가정)
            DIO.MapByName(unit, "Stage.ClampUpOut", true, NAME_CLAMP_UP);
            DIO.MapByName(unit, "Stage.ClampDownOut", true, NAME_CLAMP_DOWN);
            DIO.MapByName(unit, "Stage.ClampUpIn", false, NAME_CLAMP);          // 센서 추정 (Clamp 상태)
            DIO.MapByName(unit, "Stage.ClampDownIn", false, NAME_CLAMP_DOWN);   // 센서 추정 (Clamp Down)
            _clampLiftCylinder = new Cylinder("StageClampLift", "Stage.ClampUpOut", "Stage.ClampDownOut", "Stage.ClampUpIn", "Stage.ClampDownIn");

            // Expander Cylinder
            DIO.MapByName(unit, "Stage.ExpUpOut", true, NAME_EXP_UP);
            DIO.MapByName(unit, "Stage.ExpDownOut", true, NAME_EXP_DOWN);
            DIO.MapByName(unit, "Stage.ExpUpIn", false, NAME_EXP_UP);      // 센서 이름 동일 가정
            DIO.MapByName(unit, "Stage.ExpDownIn", false, NAME_EXP_DOWN);
            _expanderCylinder = new Cylinder("StageExpander", "Stage.ExpUpOut", "Stage.ExpDownOut", "Stage.ExpUpIn", "Stage.ExpDownIn");

            // Vacuum (Output + Check)
            DIO.MapByName(unit, "Stage.VacOut", true, NAME_VAC_OUT);
            DIO.MapByName(unit, "Stage.VacOk", false, NAME_VAC_OK);
            _ejectorVacuum = new Vacuum("Stage", "Stage.VacOut", "Stage.VacOk");

            // Clamp (Clamp / Unclamp 출력만 단순 제어) → 별도 래퍼 사용
            DIO.MapByName(unit, "Stage.ClampOut", true, NAME_CLAMP);
            DIO.MapByName(unit, "Stage.UnclampOut", true, NAME_CLAMP_UN);
        }

        public bool ClampLiftUp(int timeoutMs = 3000) => _clampLiftCylinder?.Extend(timeoutMs) ?? false;
        public bool ClampLiftDown(int timeoutMs = 3000) => _clampLiftCylinder?.Retract(timeoutMs) ?? false;
        public void ClampAllOff() => _clampLiftCylinder?.AllOff();

        public bool ExpanderUp(int timeoutMs = 3000) => _expanderCylinder?.Extend(timeoutMs) ?? false;
        public bool ExpanderDown(int timeoutMs = 3000) => _expanderCylinder?.Retract(timeoutMs) ?? false;

        public bool VacuumOnWait(int timeoutMs = 1500) => _ejectorVacuum?.OnWaitOk(timeoutMs) ?? false;
        public void VacuumOn() => _ejectorVacuum?.On();
        public void VacuumOff() => _ejectorVacuum?.Off();
        public bool VacuumOk() => _ejectorVacuum?.IsOk() ?? false;

        public void SetClamp(bool clamp)
        {
            // 단순 Clamp/Unclamp 출력 제어
            WriteOutput(NAME_CLAMP, clamp);
            WriteOutput(NAME_CLAMP_UN, !clamp);
        }

        // Sensor helpers
        public bool IsClamp() => ReadInput(NAME_CLAMP);
        public bool IsClampDown() => ReadInput(NAME_CLAMP_DOWN);
        public bool Ring0() => ReadInput(NAME_RING0);
        public bool Ring1() => ReadInput(NAME_RING1);
        public bool IsRingPresent() => Ring0() || Ring1();
        public bool VacuumCheck() => ReadInput(NAME_VAC_OK) || VacuumOk();
        #endregion
    }
}