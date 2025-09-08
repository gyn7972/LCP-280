using Newtonsoft.Json;
using QMC.Common;
using QMC.Common.Component;
using QMC.Common.IOUtil;
using QMC.Common.Motion;
using QMC.Common.Motions;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Component;
using QMC.Common.Cameras; // added
using QMC.Common.Cameras.HIKVISION; // added
using System; // added for Math
using System.Collections.Generic;
using System.Linq;
using System.IO;
using QMC.Common.VisionPart; // (remain for compatibility but no longer directly used here)
using QMC.Common.Vision; // added
using QMC.Common.Vision.Tools; // added
using QMC.Common.Vision.Cognex; // (legacy types)
using QMC.LCP_280.Process; // PatternMatchingRunner
using QMC.Common.IOUtil; // for UnitIoHelper 추가

namespace QMC.LCP_280.Process.Unit
{
    public class InputStage : BaseUnit
    {
        // Wrapper collection to allow enum index access
        public class TeachingPositionCollection : List<TeachingPosition>
        {
            public TeachingPosition this[InputStageConfig.TeachingPositionName name]
            {
                get
                {
                    string key = name.ToString();
                    return this.FirstOrDefault(p => p != null && p.Name.Equals(key, System.StringComparison.OrdinalIgnoreCase));
                }
            }
        }

        public InputStageConfig InputStageConfig { get; private set; }
        public TeachingPositionCollection TeachingPositions { get; private set; } = new TeachingPositionCollection();

        // Vision / Sequence hooks
        public Func<bool> CamReadyFunc { get; set; }
        public Action SetLightingMultiAction { get; set; }
        public Action SetLightingCenterAction { get; set; }
        public Func<bool> GrabImageFunc { get; set; }
        public Func<(bool ok, List<double> thetaList)> FindMultiMarksFunc { get; set; }
        public Func<(bool ok, double x, double y)> FindCenterMarkFunc { get; set; }

        // Stage camera
        public HIKGigECamera StageCamera { get; private set; }
        public string StageCameraKey { get; set; } = "In_Stage";

        // ================= Simplified Pattern Matching via Runner =================
        // (Old: _pmPart / recipe manual load removed)
        private PatternMatchingRunner _pmRunner;
        private bool _runnerInitTried;

        // Pixel -> mm scale
        public double PixelSizeXmm { get; set; } = 0.01; // mm per pixel
        public double PixelSizeYmm { get; set; } = 0.01;

        // Image origin configuration
        public bool UseImageCenterAsOrigin { get; set; } = true;
        public double ImageOriginX { get; set; } = double.NaN;
        public double ImageOriginY { get; set; } = double.NaN;

        // Recipe settings (re-used by PatternMatchingRunner)
        public string PatternRecipeRootDir { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "PatternMatching");
        public string PatternRecipeName { get; set; } = "Default";

        // Auto reload (handled internally by runner recipe load each search when not loaded)
        public bool AutoReloadPatternRecipe { get; set; } = true; // kept for compatibility (not directly used now)
        public TimeSpan RecipeReloadInterval { get; set; } = TimeSpan.FromSeconds(30); // (unused after refactor)

        // Dry Run Mode
        public bool DryRun { get; private set; }
        public void SetDryRun(bool on) => DryRun = on;
        private bool _simClamp;
        private bool _simClampDown;
        private bool _simVac;
        private bool _simRingPresent;
        private bool _simExpUp;

        public InputStage(InputStageConfig config = null)
            : base("InputStageConfig")
        {
            InputStageConfig = config ?? new InputStageConfig();
            AddComponents();
        }

        public override void AddComponents()
        {
            InputStageConfig.LoadAndBindAxes(Equipment.Instance.AxisManager);
            InputStageConfig.InitializeDefaultTeachingPositions();

            TeachingPositions.Clear();
            foreach (var tp in InputStageConfig.TeachingPositions)
                TeachingPositions.Add(tp);
            BindAxes();
            BindIoDomains();
            BindCamera();
            EnsureDefaultVisionHooks();
        }

        private void BindCamera()
        {
            var eq = Equipment.Instance;
            if (eq == null) return;
            if (eq.Cameras != null && eq.Cameras.TryGetValue(StageCameraKey, out var cam))
                StageCamera = cam as HIKGigECamera;
            else
                StageCamera = eq.InStageCam; // fallback
        }

        #region PatternMatchingRunner Integration
        private void EnsureRunner()
        {
            if (_pmRunner != null || _runnerInitTried) return;
            _runnerInitTried = true;
            try
            {
                if (StageCamera == null) return;
                var opt = new PatternMatchingRunner.RunnerOptions
                {
                    AutoLoadRecipe = true,
                    RecipeRootDirectory = PatternRecipeRootDir,
                    RecipeName = PatternRecipeName,
                    UseInspectRoi = true,
                    Mode = PatternMatchingRunner.SearchMode.All, // default (multi search)
                    DrawCrossOnViewer = false,
                    EnableSaveImage = false,
                };
                _pmRunner = new PatternMatchingRunner(StageCamera, null, opt);
            }
            catch (Exception ex)
            {
                try { Log.Write("InputStage", "Runner init failed: " + ex.Message); } catch { }
                _pmRunner = null;
            }
        }

        private VisionImage EnsureLatestImage()
        {
            if (StageCamera == null) return null;
            var img = StageCamera.LatestImage;
            if (img == null || img.RawData == null)
            {
                try { StageCamera.GrabSync(out img); } catch { }
            }
            return img;
        }

        private (bool ok, List<double> thetaList) MultiSearchViaRunner()
        {
            if (DryRun)
                return (true, new List<double> { 0.0, 0.01, -0.005, 0.004, -0.003 });
            EnsureRunner();
            if (_pmRunner == null)
                return (false, null);
            try
            {
                _pmRunner.SetSearchMode(PatternMatchingRunner.SearchMode.All);
                var res = _pmRunner.Search(false);
                if (!res.Success || res.Matches == null || res.Matches.Count < 1)
                    return (false, null);
                // Collect raw R list (deg) ? trimming/averaging is done later in Seq logic
                var list = res.Matches.Select(m => m.R).ToList();
                return (list.Count >= 1, list);
            }
            catch (Exception ex)
            {
                try { Log.Write("InputStage", "MultiSearchViaRunner exception: " + ex.Message); } catch { }
                return (false, null);
            }
        }

        private (bool ok, double x, double y) CenterSearchViaRunner()
        {
            if (DryRun) return (true, 0.0, 0.0);
            EnsureRunner();
            if (_pmRunner == null)
                return (false, 0, 0);
            try
            {
                var img = EnsureLatestImage();
                if (img == null || img.Header == null || img.Header.Width <= 0 || img.Header.Height <= 0)
                    return (false, 0, 0);
                double imgCx, imgCy;
                if (UseImageCenterAsOrigin || double.IsNaN(ImageOriginX) || double.IsNaN(ImageOriginY))
                {
                    imgCx = (img.Header.Width) / 2.0;
                    imgCy = (img.Header.Height) / 2.0;
                }
                else
                {
                    imgCx = ImageOriginX;
                    imgCy = ImageOriginY;
                }
                _pmRunner.SetSearchMode(PatternMatchingRunner.SearchMode.First); // choose representative near center
                var res = _pmRunner.Search(false);
                if (!res.Success || res.Matches == null || res.Matches.Count == 0)
                    return (false, 0, 0);
                // Representative
                var rep = res.Matches[(res.ReferenceIndex >= 0 && res.ReferenceIndex < res.Matches.Count) ? res.ReferenceIndex : 0];
                double dxPixels = rep.X - imgCx;
                double dyPixels = rep.Y - imgCy;
                double mmX = dxPixels * PixelSizeXmm;
                double mmY = dyPixels * PixelSizeYmm;
                return (true, mmX, mmY);
            }
            catch (Exception ex)
            {
                try { Log.Write("InputStage", "CenterSearchViaRunner exception: " + ex.Message); } catch { }
                return (false, 0, 0);
            }
        }
        #endregion

        private void EnsureDefaultVisionHooks()
        {
            if (CamReadyFunc == null)
                CamReadyFunc = () => StageCamera != null && StageCamera.Opened;

            if (GrabImageFunc == null)
            {
                GrabImageFunc = () =>
                {
                    if (StageCamera == null) return false;
                    var rc = StageCamera.GrabSync(out var img);
                    if (rc != 0 || img == null) return false;
                    StageCamera.LatestImage = img;
                    return true;
                };
            }

            if (FindMultiMarksFunc == null)
                FindMultiMarksFunc = () => MultiSearchViaRunner();

            if (FindCenterMarkFunc == null)
                FindCenterMarkFunc = () => CenterSearchViaRunner();
        }

        public override void OnRun() => base.OnRun();
        public override void OnStop() => base.OnStop();

        #region Teaching & Motion Helpers
        public void TeachCurrentPosition(string positionName, string description = null)
        {
            var axisPositions = new Dictionary<string, double>();
            foreach (var axisPair in Axes)
                axisPositions[axisPair.Key] = axisPair.Value.GetPosition();
            var tp = new TeachingPosition(positionName, axisPositions, description);
            InputStageConfig.SetTeachingPosition(tp);
        }

        public int MoveToTeachingPosition(string positionName, double vel = 0, double acc = 0, double dec = 0, double jerk = 0)
        {
            var tp = InputStageConfig.GetTeachingPosition(positionName);
            if (tp == null) return -1;
            var (x, y, t) = InputStageConfig.GetPositionWithOffset(positionName);
            int rc = 0;
            if (AxisX != null) rc |= AxisX.MoveAbs(x, vel > 0 ? vel : AxisX.Config.MaxVelocity, acc > 0 ? acc : AxisX.Config.RunAcc, dec > 0 ? dec : AxisX.Config.RunDec, jerk > 0 ? jerk : AxisX.Config.AccJerkPercent);
            if (AxisY != null) rc |= AxisY.MoveAbs(y, vel > 0 ? vel : AxisY.Config.MaxVelocity, acc > 0 ? acc : AxisY.Config.RunAcc, dec > 0 ? dec : AxisY.Config.RunDec, jerk > 0 ? jerk : AxisY.Config.AccJerkPercent);
            if (AxisT != null) rc |= AxisT.MoveAbs(t, vel > 0 ? vel : AxisT.Config.MaxVelocity, acc > 0 ? acc : AxisT.Config.RunAcc, dec > 0 ? dec : AxisT.Config.RunDec, jerk > 0 ? jerk : AxisT.Config.AccJerkPercent);
            return rc;
        }

        public int MoveToTeachingPosition(TeachingPosition tp, double vel = 0, double acc = 0, double dec = 0, double jerk = 0)
        {
            if (tp == null) return -1;
            return MoveToTeachingPosition(tp.Name, vel, acc, dec, jerk);
        }
        public int MoveToTeachingPosition(InputStageConfig.TeachingPositionName name, double vel = 0, double acc = 0, double dec = 0, double jerk = 0)
            => MoveToTeachingPosition(name.ToString(), vel, acc, dec, jerk);

        public bool InPosTeaching(string positionName)
        {
            var (x, y, t) = InputStageConfig.GetPositionWithOffset(positionName);
            return InPos(AxisX, x) && InPos(AxisY, y) && InPos(AxisT, t);
        }
        public bool InPosTeaching(TeachingPosition tp) => tp != null && InPosTeaching(tp.Name);
        public bool InPosTeaching(InputStageConfig.TeachingPositionName name) => InPosTeaching(name.ToString());

        public void ApplyOffset(string positionName, double dx, double dy, double dt)
            => InputStageConfig.SetOffset(positionName, dx, dy, dt);
        #endregion

        #region Axis / IO Helper
        private MotionAxis _axX, _axY, _axT;
        public MotionAxis AxisX => _axX;
        public MotionAxis AxisY => _axY;
        public MotionAxis AxisT => _axT;

        private void BindAxes()
        {
            Axes.TryGetValue("Wafer Stage X Axis", out _axX);
            Axes.TryGetValue("Wafer Stage Y Axis", out _axY);
            Axes.TryGetValue("Wafer Stage T Axis", out _axT);
            bool useInPos = !InputStageConfig.EnablePredictiveControl;
            foreach (var ax in new[] { _axX, _axY, _axT })
            {
                if (ax == null) continue;
                try
                {
                    var mi = ax.GetType().GetMethod("SetInPositionEnable");
                    var mr = ax.GetType().GetMethod("SetInPositionRange");
                    if (mi != null) mi.Invoke(ax, new object[] { useInPos });
                    if (mr != null) mr.Invoke(ax, new object[] { InputStageConfig.MoveDoneRemainDistance });
                }
                catch { }
            }
        }

        public double GetTP(string tpName, string axisName)
        {
            var tp = InputStageConfig.GetTeachingPosition(tpName);
            if (tp != null && tp.AxisPositions != null && tp.AxisPositions.TryGetValue(axisName, out var v)) return v;
            return 0.0;
        }
        // Overload: TeachingPosition + axis name
        public double GetTP(TeachingPosition tp, string axisName)
        {
            if (tp == null || string.IsNullOrEmpty(axisName)) return 0.0;
            if (tp.AxisPositions != null && tp.AxisPositions.TryGetValue(axisName, out var v)) return v;
            return 0.0;
        }
        // Overload: TeachingPosition + MotionAxis (usage: _stage.GetTP(_stage.TeachingPositions[Align], _stage.AxisT))
        public double GetTP(TeachingPosition tp, MotionAxis axis)
        {
            if (axis == null) return 0.0;
            return GetTP(tp, axis.Name);
        }

        public void MoveAxisOnce(MotionAxis ax, double target)
        {
            if (ax == null) return;
            if (Math.Abs(ax.GetPosition() - target) > ax.Config.InposTolerance * 3)
                ax.MoveAbs(target, ax.Config.MaxVelocity, ax.Config.RunAcc, ax.Config.RunDec, ax.Config.AccJerkPercent);
        }

        public bool InPos(MotionAxis ax, double target)
        {
            if (ax == null) return true;
            return ax.InPosition(target);
        }

        public bool ReadInput(string name)
        {
            if (DryRun)
            {
                switch (name)
                {
                    case "WAFER STAGE CLAMP": return _simClamp;
                    case "WAFER STAGE CLAMP DOWN": return _simClampDown;
                    case "EJECTOR VACUUM CHECK": return _simVac;
                    case "WAFER STAGE RING CHECK 0":
                    case "WAFER STAGE RING CHECK 1": return _simRingPresent;
                    case "WAFER STAGE EXPANDER UP": return _simExpUp;
                    case "WAFER STAGE EXPANDER DOWN": return !_simExpUp;
                }
                return false;
            }
            bool v;
            return UnitIoHelper.TryReadInput(name, out v) && v; // 통합 Helper 이용
        }

        public bool WriteOutput(string name, bool on)
        {
            if (DryRun)
            {
                switch (name)
                {
                    case "WAFER STAGE CLAMP": _simClamp = on; if (on) _simClampDown = false; break;
                    case "WAFER STAGE UNCLAMP": if (on) { _simClamp = false; _simClampDown = true; } break;
                    case "EJECTOR VACUUM": _simVac = on; break;
                    case "WAFER STAGE EXPANDER UP": if (on) _simExpUp = true; break;
                    case "WAFER STAGE EXPANDER DOWN": if (on) _simExpUp = false; break;
                }
                return true;
            }
            return UnitIoHelper.TryWriteOutput(name, on); // 통합 Helper 이용
        }
        #endregion

        #region IO Domain (Cylinder / Vacuum)
        private Cylinder _clampLiftCylinder;
        private Cylinder _expanderCylinder;
        private Vacuum _ejectorVacuum;
        public Vacuum EjectorVacuum => _ejectorVacuum;

        private void BindIoDomains()
        {
            var eq = Equipment.Instance; var unit = eq?.UnitIO; if (unit == null) return;
            DIO.MapByName(unit, "Stage.ClampUpOut", true, InputStageConfig.IO.CLAMP_UP_OUT);
            DIO.MapByName(unit, "Stage.ClampDownOut", true, InputStageConfig.IO.CLAMP_DOWN_OUT);
            DIO.MapByName(unit, "Stage.ClampUpIn", false, InputStageConfig.IO.CLAMP_SNS);
            DIO.MapByName(unit, "Stage.ClampDownIn", false, InputStageConfig.IO.CLAMP_DOWN_SNS);
            _clampLiftCylinder = new Cylinder("StageClampLift", "Stage.ClampUpOut", "Stage.ClampDownOut", "Stage.ClampUpIn", "Stage.ClampDownIn");

            DIO.MapByName(unit, "Stage.ExpUpOut", true, InputStageConfig.IO.EXPANDER_UP_OUT);
            DIO.MapByName(unit, "Stage.ExpDownOut", true, InputStageConfig.IO.EXPANDER_DOWN_OUT);
            DIO.MapByName(unit, "Stage.ExpUpIn", false, InputStageConfig.IO.EXPANDER_UP_SNS);
            DIO.MapByName(unit, "Stage.ExpDownIn", false, InputStageConfig.IO.EXPANDER_DOWN_SNS);
            _expanderCylinder = new Cylinder("StageExpander", "Stage.ExpUpOut", "Stage.ExpDownOut", "Stage.ExpUpIn", "Stage.ExpDownIn");

            DIO.MapByName(unit, "Stage.VacOut", true, InputStageConfig.IO.VAC_OUT);
            DIO.MapByName(unit, "Stage.VacOk", false, InputStageConfig.IO.VAC_OK_SNS);
            _ejectorVacuum = new Vacuum("Stage", "Stage.VacOut", "Stage.VacOk");

            DIO.MapByName(unit, "Stage.ClampOut", true, InputStageConfig.IO.CLAMP_OUT);
            DIO.MapByName(unit, "Stage.UnclampOut", true, InputStageConfig.IO.UNCLAMP_OUT);
        }

        public void SetClamp(bool clamp)
        {
            if (DryRun)
            {
                _simClamp = clamp;
                _simClampDown = !clamp;
            }
            WriteOutput(InputStageConfig.IO.CLAMP_OUT, clamp);
            WriteOutput(InputStageConfig.IO.UNCLAMP_OUT, !clamp);
        }

        public bool VacuumOnWait(int timeoutMs = 1500)
        {
            if (DryRun) { _simVac = true; return true; }
            return _ejectorVacuum?.OnWaitOk(timeoutMs) ?? false;
        }
        public void VacuumOn() { if (DryRun) { _simVac = true; return; } _ejectorVacuum?.On(); }
        public void VacuumOff() { if (DryRun) { _simVac = false; return; } _ejectorVacuum?.Off(); }
        public bool IsVacuum() => DryRun ? _simVac : (_ejectorVacuum?.IsOk() ?? false);


        public bool IsClamp() => ReadInput(InputStageConfig.IO.CLAMP_SNS);
        public bool IsClampDown() => ReadInput(InputStageConfig.IO.CLAMP_DOWN_SNS);
        public bool Ring0() => ReadInput(InputStageConfig.IO.RING_CHECK0);
        public bool Ring1() => ReadInput(InputStageConfig.IO.RING_CHECK1);
        public bool IsRingPresent() => Ring0() || Ring1();
        public bool VacuumCheck() => ReadInput(InputStageConfig.IO.VAC_OK_SNS) || IsVacuum();
        public bool IsExpanderUp() => ReadInput(InputStageConfig.IO.EXPANDER_UP_SNS);
        public bool IsExpanderDown() => ReadInput(InputStageConfig.IO.EXPANDER_DOWN_SNS);
        #endregion

        public bool ClampLiftUp(int timeoutMs = 3000)
        {
            if (DryRun) return true;
            return _clampLiftCylinder?.Extend(timeoutMs) ?? false;
        }
        public bool ClampLiftDown(int timeoutMs = 3000)
        {
            if (DryRun) return true;
            return _clampLiftCylinder?.Retract(timeoutMs) ?? false;
        }
        public bool ExpanderUp(int timeoutMs = 3000)
        {
            if (DryRun) { _simExpUp = true; return true; }
            return _expanderCylinder?.Extend(timeoutMs) ?? false;
        }
        public bool ExpanderDown(int timeoutMs = 3000)
        {
            if (DryRun) { _simExpUp = false; return true; }
            return _expanderCylinder?.Retract(timeoutMs) ?? false;
        }
    }
}