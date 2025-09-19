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
using System.Linq;
using System.Threading;
using static QMC.LCP_280.Process.Equipment;

namespace QMC.LCP_280.Process.Unit
{
    /// <summary>
    /// IndexChipProbeController Unit
    ///  - Probe Z / Probe Card XYZ / Sphere Z 축 Teaching Positions
    ///  - Sphere Forward/Backward Cylinder + Probe Card Vacuum IO 바인딩
    ///  - OutputStage 구조 패턴 적용 (Regions / Helpers / High-Level API)
    /// </summary>
    public class IndexChipProbeController : BaseUnit<IndexChipProbeControllerConfig>
    {
        public enum AlarmKeys
        {
            eIndexChipProbeController = 4701,
            eRotaryNotSafe = 4702,
            eProbeTimeout = 4703,
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

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eProbeTimeout;
            alarm.Title = "Probe Timeout.";
            alarm.Cause = "Probe Timeout입니다.\n Probe 확인 및 재 측정 바랍니다.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Warning.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

        }
        #endregion

        #region Unit
        Rotary Rotary { get; set; }
        #endregion

        #region Config / Teaching

        public IndexChipProbeControllerConfig IndexChipProbeControllerConfig => Config;

        #endregion

        #region Axes
        private MotionAxis _probeZ, _probeCardX, _probeCardY, _probeCardZ, _sphereZ;
        public MotionAxis AxisProbeZ => _probeZ;
        public MotionAxis AxisProbeCardX => _probeCardX;
        public MotionAxis AxisProbeCardY => _probeCardY;
        public MotionAxis AxisProbeCardZ => _probeCardZ;
        public MotionAxis AxisSphereZ => _sphereZ;              //Top
        #endregion

        #region IO Domain Members
        private Cylinder _cylSphere;        // FWD / BWD Cylinder
        private Vacuum _vacProbeCard;       // Probe Card Vacuum
        #endregion

        #region Constants (Names)
        private const string NAME_SPHERE_FW = IndexChipProbeControllerConfig.IO.SPHERE_FW_VLV;
        private const string NAME_SPHERE_BW = IndexChipProbeControllerConfig.IO.SPHERE_BW_VLV;
        private const string NAME_PROBE_VAC = IndexChipProbeControllerConfig.IO.PROBE_VAC_VLV;
        private const string NAME_PROBE_VAC_OK = IndexChipProbeControllerConfig.IO.PROBE_VAC_OK;
        #endregion

        #region ctor / Initialization
        public IndexChipProbeController(IndexChipProbeControllerConfig config = null) : base(new IndexChipProbeControllerConfig())
        {
            
            AddComponents();
        }

        public override void AddComponents()
        {
            Config.LoadAndBindAxes(Equipment.Instance.AxisManager);
            Config.InitializeDefaultTeachingPositions();
            
            BindAxes();
            BindIoDomains();
        }
        #endregion

        #region Axis Binding / Helpers
        protected override void OnBindUnit()
        {
            base.OnBindUnit();
            Rotary = Equipment.Instance.GetUnit(UnitKeys.Rotary) as Rotary;
        }
        private void BindAxes()
        {
            var mgr = Equipment.Instance?.AxisManager;
            if (mgr == null)
            {
                Log.Write("IndexChipProbeController", "[BindAxes] AxisManager null");
                return;
            }

            const string unitName = "Unit"; // 축 등록 시 사용된 유닛명(Equipment.CreateAxes에서 동일)

            BindAxis(mgr, unitName, AxisNames.ProbeZ, ref _probeZ);
            BindAxis(mgr, unitName, AxisNames.ProbeCardX, ref _probeCardX);
            BindAxis(mgr, unitName, AxisNames.ProbeCardY, ref _probeCardY);
            BindAxis(mgr, unitName, AxisNames.ProbeCardZ, ref _probeCardZ);
            BindAxis(mgr, unitName, AxisNames.SphereZ, ref _sphereZ);
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
            foreach (var axisKey in tp.AxisPositions.Keys)
            {
                if (Axes.TryGetValue(axisKey, out var axis))
                {
                    double pos = tp.AxisPositions[axisKey];
                    int r = axis.MoveAbs(pos, vel, acc, dec, jerk);
                    if (r != 0) result = r;
                }
            }
            return result;
        }
        public bool InPosTeaching(string positionName)
        {
            var tp = Config.GetTeachingPosition(positionName);
            if (tp == null) return false;
            foreach (var kv in tp.AxisPositions)
                if (!Axes.TryGetValue(kv.Key, out var axis) || !InPos(axis, kv.Value)) return false;
            return true;
        }
        #endregion

        #region Low-Level IO Access
        public bool ReadInput(string name)
        {
            var hi = Config.HardInputs.FirstOrDefault(i => i.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
            if (hi == null) return false;
            var eq = Equipment.Instance; var dio = eq?.DioScan; if (dio == null) return false;
            foreach (var m in eq.UnitIO.Modules)
                if (dio.TryGetInput(m.ModuleName, hi.Disp, out var v)) return v;
            return false;
        }
        public bool WriteOutput(string name, bool on)
        {
            var ho = Config.HardOutputs.FirstOrDefault(o => o.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
            if (ho == null) return false;
            var eq = Equipment.Instance; var dio = eq?.DioScan; if (dio == null) return false;
            foreach (var m in eq.UnitIO.Modules)
                if (dio.WriteOutput(m.ModuleName, ho.Disp, on) == 0) return true;
            return false;
        }
        public bool IsOutputOn(string name)
        {
            var ho = Config.HardOutputs.FirstOrDefault(o => o.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
            if (ho == null) return false;
            var eq = Equipment.Instance; var dio = eq?.DioScan; if (dio == null) return false;
            foreach (var m in eq.UnitIO.Modules)
                if (dio.TryGetOutput(m.ModuleName, ho.Disp, out var v)) return v;
            return false;
        }
        #endregion

        #region IO Domain Mapping (Reorganized)
        private void BindIoDomains()
        {
            var eq = Equipment.Instance; var unit = eq?.UnitIO; if (unit == null) return;

            // Vacuum 별칭으로 조회만
            if (!IoAutoBindings.Vacuums.TryGetValue("ProbeCardVac", out _vacProbeCard))
            {
                Log.Write("IndexChipProbeController", "BindIoDomains", "Vacuums not found: ProbeCardVac");
            }

            if (!IoAutoBindings.Cylinders.TryGetValue("ProbeSphere", out _cylSphere))
            {
                Log.Write("IndexChipProbeController", "BindIoDomains", "Cylinder not found: ProbeSphere");
            }
        }

        // === Domain Control (표준 구동) ===
        public bool SetProbeVac(bool on)
        {
            if (_vacProbeCard == null) return false;
            if (on) _vacProbeCard.On();
            else   _vacProbeCard.Off();
            return true;
        }
        public bool SetSphereFB(bool bFwdBwd)
        {
            if (_cylSphere == null) return false;

            if (bFwdBwd)
            {
                return _cylSphere.Extend();
            }
            else
            {
                return _cylSphere.Retract();
            }
        }
        public bool ProbeVacOk() => _vacProbeCard?.IsOk() ?? false;
        public bool IsSphereForward() => ReadInput(NAME_SPHERE_FW);   // Forward sensor
        public bool IsSphereBackward() => ReadInput(NAME_SPHERE_BW);   // Backward sensor
        /////////////////////


        // === Direct Valve Control (강제 구동) ===
        public bool IsSphereFwdValveOn()       => IsOutputOn(NAME_SPHERE_FW);
        public bool IsProbeVacValveOn()        => IsOutputOn(NAME_PROBE_VAC);
        #endregion

        #region Lifecycle
        public override int OnRun()  { int ret = 0; return ret; }
        public override int OnStop() { int ret = 0; base.OnStop(); return ret; }
        protected override int OnRunReady() { return 0; }
        protected override int OnRunWork() { return 0; }
        protected override int OnRunComplete() { return 0; }
        #endregion

        public int IsRotaryIdle()
        {
            if (Rotary != null && Rotary.IsAnyAxisMoving())
            {
                AxisProbeCardX.EmgStop();
                AxisProbeCardY.EmgStop();
                AxisProbeCardZ.EmgStop();
                AxisProbeZ.EmgStop();
                AxisSphereZ.EmgStop();

                PostAlarm((int)AlarmKeys.eRotaryNotSafe);
                return -1;
            }
            return 0;
        }

        private void LogSequence(string log)
        {
            Log.Write(UnitName, this.CurrentFunc.Method.Name, $"[Sequence] {log}");
        }
        protected override void OnMakeSequence()
        {
            base.OnMakeSequence();
            this.SequencePlayers.Add(BottomContactOnce);
        }
        #region Seq 단위 동작 함수

        public int TopContact()
        {
            int nRet = -1;
            /* TODO */
            return nRet;
        }

        /// <summary>
        /// Bottom Contact 1개 소켓 검사 시컨스
        /// 순서:
        ///  1) ProbeCard Ready Z축 하강 및 확인
        ///  2) ProbeCard Ready X/Y 이동
        ///  3) ProbeCard Z Ready 이동
        ///  4) ProbeCard Up X/Y 이동
        ///  5) ProbeCard Z축 상승
        ///  6) ChipProber 검사 요구 신호 전달
        ///  7) 검사완료 신호 대기
        ///  8) 검사완료 처리
        ///  9) ProbeCard Ready Z축 하강
        ///  10) 완료
        /// </summary>
        public int BottomContactOnce(bool bFineSpeed = false)
        {
            int bRtn = 0;

            try
            {
                LogSequence("Start");
                this.CurrentFunc = BottomContactOnce;

                int nIndex = GetProbeIndexNo();

                bRtn = IsRotaryIdle();
                if (bRtn != 0)
                    return -1;

                // 1) Ready Z축 하강
                bRtn = MovePositionBottomReadyZDown(nIndex, bFineSpeed);
                if (bRtn != 0) return -1;

                // 2) Ready X/Y 이동
                bRtn = MovePositionBottomReadyXY(nIndex, bFineSpeed);
                if (bRtn != 0) return -1;

                // 3) ProbeCard Z Ready 위치
                bRtn = MovePositionBottomReadyZ(nIndex, bFineSpeed);
                if (bRtn != 0) return -1;

                // 4) Up X/Y 이동
                bRtn = MovePositionBottomUpXY(nIndex, bFineSpeed);
                if (bRtn != 0) return -1;

                // 5) Z축 상승
                bRtn = MovePositionBottomUpZ(nIndex, bFineSpeed);
                if (bRtn != 0) return -1;

                // 6) 검사 요구 신호
                SetChipProberRequest(true);

                // 7) 검사 완료 신호 대기
                bRtn = WaitChipProberDone(timeoutMs: 5000);
                if (bRtn != 0) return -1;

                // 8) 검사 완료 처리
                SetChipProberRequest(false);

                // 9) Ready Z축 하강
                bRtn = MovePositionBottomReadyZDown(nIndex, bFineSpeed);
                if (bRtn != 0) return -1;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return -1;
            }
            finally
            {
                LogSequence("End");
            }

            return bRtn;
        }

        private int GetProbeIndexNo()
        {
            int nIndex = 0;
            if (Rotary == null)
                return nIndex;

            nIndex = (Rotary.GetLoadIndexNo() + this.Config.IndexOfProbe) % Rotary.GetIndexCount();
            return nIndex;
        }

        private int MovePositionBottomReadyZDown(int indexNo, bool fine = false)
        {
            int nRtn = 0;
            return nRtn;
            //return MoveToTeaching(EnumName($"Bottom_Index{indexNo}_Ready"), AxisNames.ProbeCardZ, fine);
        }

        private int MovePositionBottomReadyXY(int indexNo, bool fine = false)
        {
            int nRtn = 0;
            return nRtn;
            //return MoveToTeaching(EnumName($"Bottom_Index{indexNo}_Ready"), new[] { AxisNames.ProbeCardX, AxisNames.ProbeCardY }, fine);
        }

        private int MovePositionBottomReadyZ(int indexNo, bool fine = false)
        {
            int nRtn = 0;
            return nRtn;
            //return MoveToTeaching(EnumName($"Bottom_Index{indexNo}_Ready"), AxisNames.ProbeCardZ, fine);
        }

        private int MovePositionBottomUpXY(int indexNo, bool fine = false)
        {
            int nRtn = 0;
            return nRtn;
            //return MoveToTeaching(EnumName($"Bottom_Index{indexNo}_Up"), new[] { AxisNames.ProbeCardX, AxisNames.ProbeCardY }, fine);
        }

        private int MovePositionBottomUpZ(int indexNo, bool fine = false)
        {
            int nRtn = 0;
            return nRtn;
            //return MoveToTeaching(EnumName($"Bottom_Index{indexNo}_Up"), AxisNames.ProbeCardZ, fine);
        }

        // ChipProber 신호 인터페이스 (예시)
        private void SetChipProberRequest(bool on)
        {
            // IO 혹은 인터페이스를 통해 검사 요구 신호 전달
            WriteOutput("PROBER_REQ", on);
        }

        private int WaitChipProberDone(int timeoutMs)
        {
            int tick = Environment.TickCount;
            while (Environment.TickCount - tick < timeoutMs)
            {
                if (ReadInput("PROBER_DONE"))
                    return 0;
                Thread.Sleep(10);
            }
            PostAlarm((int)AlarmKeys.eProbeTimeout);
            return -1;
        }

        #endregion

    }
}