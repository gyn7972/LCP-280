using System;
using System.Threading;
using QMC.Common.Sequence;
using QMC.Common.Motions;

namespace QMC.LCP_280.Process.Unit
{
    public sealed class InputStageSequence : SequenceBase
    {
        public enum Step { Stop, Loading, Clamp, FileReading, Align, Scan, MapMerge, Unloading, PickUp, WorkingPosition, ChipAlign, Execute, LoadingPosition, CenterPosition, AlignPosition, RecipeChange }
        public enum SubStep { Init, Step1, Step2, Step3, Step4, Step5, Step6, Step7, Step8, Step9, Finish }

        private readonly InputStage _stage;
        private readonly InputStageConfig _cfg;
        private Step _step = Step.Stop;
        private SubStep _sub = SubStep.Init;
        private DateTime _tick;
        private int _scanIndex;
        private int _chipIndex;
        private readonly object _gate = new object();
        private const int SCAN_MAX = 12;

        public Step CurrentMode => _step;
        public SubStep CurrentSub => _sub;

        public InputStageSequence(InputStage stage, string name = null) : base(name ?? "InputStageSeq") { _stage = stage ?? throw new ArgumentNullException(nameof(stage)); _cfg = _stage.InputStageConfig; }

        public bool Start(Step step) { lock (_gate) { _step = step; _sub = SubStep.Init; _scanIndex = 0; _chipIndex = 0; _tick = DateTime.UtcNow; } return base.Start(0); }

        private void Next() { if (_sub != SubStep.Finish) _sub = _sub + 1; }
        private void Goto(SubStep s) { _sub = s; }
        private int CompleteStep() { _step = Step.Stop; return -1; }
        private bool TimeOverMs(int ms) => (DateTime.UtcNow - _tick).TotalMilliseconds >= ms;
        private void ResetTick() => _tick = DateTime.UtcNow;

        protected override int ExecuteStep(int currentStep, System.Threading.CancellationToken ct)
        {
            switch (_step)
            {
                case Step.Stop: return -1;
                case Step.Loading: return DoLoading(currentStep);
                case Step.Clamp: return DoClamp(currentStep);
                case Step.FileReading: return DoFileReading(currentStep);
                case Step.Align: return DoAlign(currentStep);
                case Step.Scan: return DoScan(currentStep);
                case Step.MapMerge: return DoMapMerge(currentStep);
                case Step.Unloading: return DoUnloading(currentStep);
                case Step.PickUp: return DoPickUp(currentStep);
                case Step.WorkingPosition: return DoWorkingPosition(currentStep);
                case Step.ChipAlign: return DoChipAlign(currentStep);
                case Step.Execute: return DoExecute(currentStep);
                case Step.LoadingPosition: return DoLoadingPosition(currentStep);
                case Step.CenterPosition: return DoCenterPosition(currentStep);
                case Step.AlignPosition: return DoAlignPosition(currentStep);
                case Step.RecipeChange: return DoRecipeChange(currentStep);
            }
            return CompleteStep();
        }

        private int DoLoading(int cur)
        {
            double tx = _stage.GetTP("Loading", "Wafer Stage X Axis");
            double ty = _stage.GetTP("Loading", "Wafer Stage Y Axis");
            double tt = _stage.GetTP("Loading", "Wafer Stage T Axis");
            switch (_sub)
            {
                case SubStep.Init: _stage.SetClamp(false); ResetTick(); Next(); break;
                case SubStep.Step1:
                    if (_stage.IsRingPresent() || TimeOverMs(300)) { _stage.MoveAxisOnce(_stage.AxisY, ty); _stage.MoveAxisOnce(_stage.AxisX, tx); _stage.MoveAxisOnce(_stage.AxisT, tt); Next(); }
                    break;
                case SubStep.Step2:
                    if (!(_stage.InPos(_stage.AxisX, tx) && _stage.InPos(_stage.AxisY, ty) && _stage.InPos(_stage.AxisT, tt))) return cur + 1;
                    ResetTick(); Next(); break;
                case SubStep.Step3: if (!TimeOverMs(80)) return cur + 1; Next(); break;
                case SubStep.Step4: _sub = SubStep.Finish; break;
                case SubStep.Finish: return CompleteStep();
            }
            return cur + 1;
        }

        private int DoClamp(int cur)
        {
            switch (_sub)
            {
                case SubStep.Init: _stage.ClampLiftUp(); ResetTick(); Next(); break;
                case SubStep.Step1: if (!TimeOverMs(150)) return cur + 1; _stage.SetClamp(true); ResetTick(); Next(); break;
                case SubStep.Step2: if (_stage.IsClamp() || _stage.IsClampDown() || TimeOverMs(500)) Next(); break;
                case SubStep.Step3: _sub = SubStep.Finish; break;
                case SubStep.Finish: return CompleteStep();
            }
            return cur + 1;
        }

        private int DoFileReading(int cur)
        { switch (_sub) { case SubStep.Init: ResetTick(); Next(); break; case SubStep.Step1: if (!TimeOverMs(150)) return cur + 1; Next(); break; case SubStep.Step2: Next(); break; case SubStep.Finish: return CompleteStep(); } return cur + 1; }

        private int DoAlign(int cur)
        { switch (_sub) { case SubStep.Init: ResetTick(); Next(); break; case SubStep.Step1: if (!TimeOverMs(120)) return cur + 1; Next(); break; case SubStep.Step2: Next(); break; case SubStep.Step3: ResetTick(); Next(); break; case SubStep.Step4: if (!TimeOverMs(50)) return cur + 1; _sub = SubStep.Finish; break; case SubStep.Finish: return CompleteStep(); } return cur + 1; }

        private int DoScan(int cur)
        {
            switch (_sub)
            {
                case SubStep.Init: _scanIndex = 0; Next(); break;
                case SubStep.Step1:
                    double baseX = _stage.GetTP("Ready", "Wafer Stage X Axis");
                    double baseY = _stage.GetTP("Ready", "Wafer Stage Y Axis");
                    double baseT = _stage.GetTP("Ready", "Wafer Stage T Axis");
                    double targetX = baseX + (_scanIndex * 5.0);
                    _stage.MoveAxisOnce(_stage.AxisX, targetX); _stage.MoveAxisOnce(_stage.AxisY, baseY); _stage.MoveAxisOnce(_stage.AxisT, baseT); Next(); break;
                case SubStep.Step2:
                    double tx = _stage.GetTP("Ready", "Wafer Stage X Axis") + (_scanIndex * 5.0);
                    if (!(_stage.InPos(_stage.AxisX, tx) && _stage.InPos(_stage.AxisY, _stage.GetTP("Ready", "Wafer Stage Y Axis")))) return cur + 1; ResetTick(); Next(); break;
                case SubStep.Step3:
                    if (!TimeOverMs(40)) return cur + 1; _scanIndex++; if (_scanIndex < SCAN_MAX) Goto(SubStep.Step1); else Next(); break;
                case SubStep.Step4: ResetTick(); Next(); break;
                case SubStep.Step5: if (!TimeOverMs(50)) return cur + 1; _sub = SubStep.Finish; break;
                case SubStep.Finish: return CompleteStep();
            }
            return cur + 1;
        }

        private int DoMapMerge(int cur)
        { switch (_sub) { case SubStep.Init: ResetTick(); Next(); break; case SubStep.Step1: if (!TimeOverMs(80)) return cur + 1; Next(); break; case SubStep.Step2: Next(); break; case SubStep.Finish: return CompleteStep(); } return cur + 1; }

        private int DoUnloading(int cur)
        {
            double tx = _stage.GetTP("Unloading", "Wafer Stage X Axis"); double ty = _stage.GetTP("Unloading", "Wafer Stage Y Axis"); double tt = _stage.GetTP("Unloading", "Wafer Stage T Axis");
            switch (_sub)
            {
                case SubStep.Init: _stage.MoveAxisOnce(_stage.AxisY, ty); _stage.MoveAxisOnce(_stage.AxisX, tx); _stage.MoveAxisOnce(_stage.AxisT, tt); Next(); break;
                case SubStep.Step1:
                    if (!(_stage.InPos(_stage.AxisX, tx) && _stage.InPos(_stage.AxisY, ty) && _stage.InPos(_stage.AxisT, tt))) return cur + 1;
                    _stage.SetClamp(false); _stage.VacuumOff(); ResetTick(); Next(); break;
                case SubStep.Step2: if (!TimeOverMs(300)) return cur + 1; Next(); break;
                case SubStep.Step3: _sub = SubStep.Finish; break;
                case SubStep.Finish: return CompleteStep();
            }
            return cur + 1;
        }

        private int DoPickUp(int cur)
        { switch (_sub) { case SubStep.Init: ResetTick(); Next(); break; case SubStep.Step1: _stage.VacuumOn(); ResetTick(); Next(); break; case SubStep.Step2: if (_stage.VacuumCheck() || TimeOverMs(100)) { _chipIndex++; Next(); } break; case SubStep.Step3: _sub = SubStep.Finish; break; case SubStep.Finish: return CompleteStep(); } return cur + 1; }

        private int DoWorkingPosition(int cur)
        {
            double baseX = _stage.GetTP("Ready", "Wafer Stage X Axis"); double baseY = _stage.GetTP("Ready", "Wafer Stage Y Axis"); double baseT = _stage.GetTP("Ready", "Wafer Stage T Axis"); double tgtX = baseX + _chipIndex * 1.0;
            switch (_sub)
            {
                case SubStep.Init: _stage.MoveAxisOnce(_stage.AxisX, tgtX); _stage.MoveAxisOnce(_stage.AxisY, baseY); _stage.MoveAxisOnce(_stage.AxisT, baseT); Next(); break;
                case SubStep.Step1: if (!(_stage.InPos(_stage.AxisX, tgtX) && _stage.InPos(_stage.AxisY, baseY) && _stage.InPos(_stage.AxisT, baseT))) return cur + 1; Next(); break;
                case SubStep.Step2: _sub = SubStep.Finish; break;
                case SubStep.Finish: return CompleteStep();
            }
            return cur + 1;
        }

        private int DoChipAlign(int cur)
        { switch (_sub) { case SubStep.Init: ResetTick(); Next(); break; case SubStep.Step1: if (!TimeOverMs(60)) return cur + 1; Next(); break; case SubStep.Step2: Next(); break; case SubStep.Step3: _sub = SubStep.Finish; break; case SubStep.Finish: return CompleteStep(); } return cur + 1; }

        private int DoExecute(int cur)
        { switch (_sub) { case SubStep.Init: ResetTick(); Next(); break; case SubStep.Step1: _stage.VacuumOff(); ResetTick(); Next(); break; case SubStep.Step2: if (!TimeOverMs(80)) return cur + 1; _sub = SubStep.Finish; break; case SubStep.Finish: return CompleteStep(); } return cur + 1; }

        private int DoLoadingPosition(int cur)
        {
            double tx = _stage.GetTP("Loading", "Wafer Stage X Axis"); double ty = _stage.GetTP("Loading", "Wafer Stage Y Axis"); double tt = _stage.GetTP("Loading", "Wafer Stage T Axis");
            switch (_sub)
            {
                case SubStep.Init: _stage.MoveAxisOnce(_stage.AxisX, tx); Next(); break;
                case SubStep.Step1: if (!_stage.InPos(_stage.AxisX, tx)) return cur + 1; _stage.MoveAxisOnce(_stage.AxisY, ty); Next(); break;
                case SubStep.Step2: if (!_stage.InPos(_stage.AxisY, ty)) return cur + 1; _stage.MoveAxisOnce(_stage.AxisT, tt); Next(); break;
                case SubStep.Step3: if (!_stage.InPos(_stage.AxisT, tt)) return cur + 1; _sub = SubStep.Finish; break;
                case SubStep.Finish: return CompleteStep();
            }
            return cur + 1;
        }

        private int DoCenterPosition(int cur)
        {
            double tx = _stage.GetTP("Home", "Wafer Stage X Axis"); double ty = _stage.GetTP("Home", "Wafer Stage Y Axis"); double tt = _stage.GetTP("Home", "Wafer Stage T Axis");
            switch (_sub)
            {
                case SubStep.Init: _stage.MoveAxisOnce(_stage.AxisY, ty); Next(); break;
                case SubStep.Step1: if (!_stage.InPos(_stage.AxisY, ty)) return cur + 1; _stage.MoveAxisOnce(_stage.AxisX, tx); Next(); break;
                case SubStep.Step2: if (!_stage.InPos(_stage.AxisX, tx)) return cur + 1; _stage.MoveAxisOnce(_stage.AxisT, tt); Next(); break;
                case SubStep.Step3: if (!_stage.InPos(_stage.AxisT, tt)) return cur + 1; _sub = SubStep.Finish; break;
                case SubStep.Finish: return CompleteStep();
            }
            return cur + 1;
        }

        private int DoAlignPosition(int cur)
        {
            double tx = _stage.GetTP("Ready", "Wafer Stage X Axis"); double ty = _stage.GetTP("Ready", "Wafer Stage Y Axis"); double tt = _stage.GetTP("Ready", "Wafer Stage T Axis");
            switch (_sub)
            {
                case SubStep.Init: _stage.MoveAxisOnce(_stage.AxisY, ty); Next(); break;
                case SubStep.Step1: if (!_stage.InPos(_stage.AxisY, ty)) return cur + 1; _stage.MoveAxisOnce(_stage.AxisX, tx); Next(); break;
                case SubStep.Step2: if (!_stage.InPos(_stage.AxisX, tx)) return cur + 1; _stage.MoveAxisOnce(_stage.AxisT, tt); Next(); break;
                case SubStep.Step3: if (!_stage.InPos(_stage.AxisT, tt)) return cur + 1; _sub = SubStep.Finish; break;
                case SubStep.Finish: return CompleteStep();
            }
            return cur + 1;
        }

        private int DoRecipeChange(int cur)
        { switch (_sub) { case SubStep.Init: ResetTick(); Next(); break; case SubStep.Step1: if (!TimeOverMs(50)) return cur + 1; ResetTick(); Next(); break; case SubStep.Step2: if (!TimeOverMs(50)) return cur + 1; ResetTick(); Next(); break; case SubStep.Step3: if (!TimeOverMs(50)) return cur + 1; _sub = SubStep.Finish; break; case SubStep.Finish: return CompleteStep(); } return cur + 1; }
    }
}
