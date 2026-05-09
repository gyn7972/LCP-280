using QMC.Common;
using QMC.Common.Alarm;
using QMC.Common.Cameras.HIKVISION;
using QMC.Common.Component;
using QMC.Common.IOUtil;
using QMC.Common.Motions;
using QMC.Common.PKGTester;
using QMC.Common.Unit;
using QMC.Common.Vision;
using QMC.LCP_280.Process.Component;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using static QMC.LCP_280.Process.Equipment;

namespace QMC.LCP_280.Process.Unit
{
    public class IndexChipProbeController : BaseUnit<IndexChipProbeControllerConfig>
    {
        public new enum AlarmKeys
        {
            eRotaryAxesMoving = 10701,
            eRotaryNotSafety,
            eProbeTimeout,
            eSphereNotForward,
            eSphereFBTimeout,
            eProbeCardZNotSafety,
            eProbeZNotIndexUp,
            eSphereMoveDownTimeout,
            eSphereMoveUpTimeout,
            eAxisDistanceInterlock, // [ADD] ProbeCardZ(상부) - GripperX(하부) 거리 인터락
            eIndexProbeControllerRotateMoveToSocket,
        }

        #region InitAlarm
        protected override void InitAlarm()
        {
            string source = "Index_ProbeCont";
            base.InitAlarm();

            // 1. 공용 파일 로더에서 알람 목록 가져오기
            var loadedAlarms = GlobalAlarmTable.Instance.GetAlarmsForSource(source);
            if (loadedAlarms == null || loadedAlarms.Count == 0)
            {
                Log.Write("AlarmInit", $"Could not find alarms for source '{source}' in the alarm file. Only default alarms will be registered.");

                AlarmInfo alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eRotaryAxesMoving;
                alarm.Title = "Rotary Axis Moving";
                alarm.Cause = "Rotary axis is moving. Please stop and try again.";
                alarm.Source = source; //this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eRotaryNotSafety;
                alarm.Title = "Rotary Not Safety Pos.";
                alarm.Cause = "Rotary is not in safety position. Please check the position and restart.";
                alarm.Source = source; //this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eProbeTimeout;
                alarm.Title = "Probe Timeout.";
                alarm.Cause = "Probe timeout. Please check the probe and remeasure.";
                alarm.Source = source; //this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eSphereNotForward;
                alarm.Title = "Sphere Cylinder Not Forward.";
                alarm.Cause = "Sphere cylinder is not in forward position. Please check the position.";
                alarm.Source = source; //this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eSphereFBTimeout;
                alarm.Title = "Sphere Cylinder Forward/Backward Timeout.";
                alarm.Cause = "Sphere cylinder forward/backward timeout. Please check the cylinder.";
                alarm.Source = source; //this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eProbeCardZNotSafety;
                alarm.Title = "Probe-Card Z Not Safety.";
                alarm.Cause = "Probe-Card Z axis is not in safety position. Please check the position.";
                alarm.Source = source; //this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eProbeZNotIndexUp;
                alarm.Title = "Probe Z Not Index Up.";
                alarm.Cause = "Probe Z axis is not in Index Up position. Please check the position.";
                alarm.Source = source; //this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eSphereMoveDownTimeout;
                alarm.Title = "Sphere Move Down Timeout";
                alarm.Cause = "Sphere Z axis failed to move to Down position. Please check the position.";
                alarm.Source = source; //this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eSphereMoveUpTimeout;
                alarm.Title = "Sphere Move Up Timeout";
                alarm.Cause = "Sphere Z axis failed to move to Up position. Please check the position.";
                alarm.Source = source; //this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eAxisDistanceInterlock;
                alarm.Title = "Axis Distance Interlock";
                alarm.Cause = "Movement blocked due to violation of top/bottom axis distance limit.";
                alarm.Source = source; //this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eIndexProbeControllerRotateMoveToSocket;
                alarm.Title = "Rotate Move To Socket";
                alarm.Cause = "Rotate is moving to socket. Please wait until it reaches the position.";
                alarm.Source = source; //this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Inform.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);


            }
            else
            {
                // 2. m_dicAlarms에 일괄 등록
                foreach (var alarmInfo in loadedAlarms)
                {
                    if (!m_dicAlarms.ContainsKey(alarmInfo.Code))
                    {
                        m_dicAlarms.Add(alarmInfo.Code, alarmInfo);
                    }
                }
            }
        }
        #endregion

        #region Unit
        Rotary Rotary { get; set; }
        IndexChipProber IndexChipProber { get; set; }
        #endregion

        #region Config / Teaching /Recipe

        public IndexChipProbeControllerConfig IndexChipProbeControllerConfig => Config;
        private IndexChipProbeControllerRecipe Recipe => Config?.TeachingRecipe;

        //Conter저장
        private PersistentContactCounter _contactCounter;
        public long ContactTotalCount => _contactCounter?.TotalCount ?? 0;

        #endregion

        #region Axes
        private MotionAxis _probeZ, _probeCardX, _probeCardY, _probeCardZ, _sphereZ, _GripperX;
        public MotionAxis AxisProbeZ => _probeZ;
        public MotionAxis AxisProbeCardX => _probeCardX;
        public MotionAxis AxisProbeCardY => _probeCardY;
        public MotionAxis AxisProbeCardZ => _probeCardZ;
        public MotionAxis AxisSphereZ => _sphereZ;              //Top
        public MotionAxis AxisGripperX => _GripperX;            //Bottom 전용.
        #endregion

        // Safety 동작 중 여부
        private bool _isSafetyMoving = false;

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
        public IndexChipProbeController(IndexChipProbeControllerConfig config = null)
            : base(config ?? new IndexChipProbeControllerConfig())
        {
            AddComponents();
        }

        public override void AddComponents()
        {
            // [CHG] Config.LoadAndBindAxes()는 내부에서 Recipe 로드까지 위임하지만,
            // Teaching은 명시적으로 Recipe를 기준으로 로딩/기본값 세팅까지 수행하는 쪽이 명확함
            Config.LoadAndBindAxes(Equipment.Instance.AxisManager);

            // [CHG] Teaching 기본값은 Recipe에 생성
            Config.TeachingRecipe?.InitializeDefaultTeachingPositions(save: true);

            _contactCounter = new PersistentContactCounter();

            BindAxes();
            BindIoDomains();
            BindCamera(); // [ADD] Camera Binding
        }
        #endregion

        #region Axis Binding / Helpers
        protected override void OnBindUnit()
        {
            base.OnBindUnit();
            Rotary = Equipment.Instance.GetUnit(UnitKeys.Rotary) as Rotary;
            IndexChipProber = Equipment.Instance.GetUnit(UnitKeys.IndexChipProber) as IndexChipProber;



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
            BindAxis(mgr, unitName, AxisNames.GripperX, ref _GripperX);
        }

        private IList<TeachingPosition> GetTeachingList()
        {
            var r = Config?.TeachingRecipe;
            if (r?.TeachingPositions != null)
            {
                return r.TeachingPositions;
            }

            return Config?.TeachingPositions ?? new List<TeachingPosition>();
        }
        public override double GetTP(string tpName, string axisName)
        {
            try
            {
                var recipe = Config?.TeachingRecipe;
                var tp = recipe?.Get(tpName);
                if (tp != null && tp.AxisPositions != null && tp.AxisPositions.TryGetValue(axisName, out var v))
                    return v;

                // fallback: 기존 방식 (Config)
                return base.GetTP(tpName, axisName);
            }
            catch
            {
                return base.GetTP(tpName, axisName);
            }
        }
        private TeachingPosition GetTeachingPosition(string tpName)
        {
            var r = Recipe;
            if (r != null)
                return r.Get(tpName);

            // 혹시라도 TeachingRecipe가 null인 비정상 상태 대비(호환/안전)
            return Config.GetTeachingPosition(tpName);
        }

        public override bool InPosTeaching(string positionName)
        {
            var recipe = Config?.TeachingRecipe;
            var tp = recipe?.Get(positionName);
            if (tp == null)
                return base.InPosTeaching(positionName);

            foreach (var kv in tp.AxisPositions)
            {
                if (!Axes.TryGetValue(kv.Key, out var axis))
                    return false;

                // BaseUnit의 Teaching 완화 판정 재사용
                // (InPosTeachingAxis가 protected라면 여기에서 사용 가능)
                if (!InPos(axis, kv.Value) && !axis.InPosition(kv.Value))
                    return false;
            }
            return true;
        }

        public override bool IsInterlockOK(BaseComponent baseComponent, BaseComponent.InterlockEventArgs e)
        {
            bool bRet = base.IsInterlockOK(baseComponent, e);
            if (baseComponent == this.AxisProbeCardZ || baseComponent == this.AxisProbeZ)
            {
                if (_isSafetyMoving)
                    return true;

                if (IsRotaryReadyStable(3, 5) == false)
                {
                    AxisProbeCardZ?.EmgStop();
                    AxisProbeZ?.EmgStop();
                    PostAlarm((int)AlarmKeys.eRotaryAxesMoving);
                    Log.Write(UnitName, nameof(IsInterlockOK), $"Rotary moving interlock. {Rotary.GetIndexMovingDebugText()}");
                    return false;
                }

                if (baseComponent == this.AxisProbeZ)
                {
                    //Ready 위치 이동시에 알람 걸림.
                    if (IsPositionProbeCardZSafety() == false
                        && IsBottomIndexZReady(GetProbeIndexNo()) == false)
                    {
                        AxisProbeCardZ?.EmgStop();
                        AxisProbeZ?.EmgStop();
                        PostAlarm((int)AlarmKeys.eProbeCardZNotSafety);
                        return false;
                    }
                }

                if (baseComponent == this.AxisProbeCardZ)
                {
                    //Ready 위치 이동시에 알람 걸림.
                    //조건 필요.
                    //if (IsPositionGripperXIndexUp() == false 
                    //    && IsBottomIndexZReady(GetProbeIndexNo()) == false)
                    //{
                    //    AxisProbeCardZ?.EmgStop();
                    //    AxisProbeZ?.EmgStop();
                    //    PostAlarm((int)AlarmKeys.eProbeZNotIndexUp);
                    //    return false;
                    //}
                }
            }
            else if (baseComponent == this.AxisProbeCardX ||
                     baseComponent == this.AxisProbeCardY)
            {
                //if (this.IsPositionProbeCardZSafety() == false)  // Todo: ProbeCardZ Safety -> Ready Pos 이동하는 시퀀스 추가 필요 
                if (this.IsBottomContactIndexZUp(GetProbeIndexNo()))
                {
                    AxisProbeCardX?.EmgStop();
                    AxisProbeCardY?.EmgStop();
                    PostAlarm((int)AlarmKeys.eProbeCardZNotSafety);
                    return false;
                }
            }
            else if (baseComponent == this.AxisSphereZ)
            {
                // AxisSphereZ 축 이동시 별도 인터락 없음
            }
            return bRet;
        }
        public bool IsRotaryReadyStable(int checkCount = 3, int pollMs = 5)
        {
            if (Rotary == null) return true;

            for (int i = 0; i < checkCount; i++)
            {
                if (!Rotary.IsIndexReadyForAction(out _))
                    return false;

                if (i < checkCount - 1)
                    Thread.Sleep(pollMs);
            }
            return true;
        }


        public int MovePositionSafetyZ(bool isFine = false)
        {
            Task<int> task = MovePositionAsyncSafetyZ(isFine);
            while (IsEndTask(task) == false)
            {
                IsMoveInterLockSafetyZ();
                Thread.Sleep(1);
            }
            return task.Result;
        }
        public Task<int> MovePositionAsyncSafetyZ(bool isFine = false)
        {
            return Task.Run(() =>
            {
                return OnMovePositionSafetyZ(isFine);
            });
        }
        private int OnMovePositionSafetyZ(bool isFine = false)
        {
            int nRet = 0;
            _isSafetyMoving = true;

            if (true) // 순차 방식
            {
                try
                {
                    string tpName = $"SafetyZone";
                    var tpObj = GetTeachingPosition(tpName);
                    if (tpObj == null)
                    {
                        Log.Write(UnitName, $"[OnMovePosition_SafetyZone] Teaching not found: {tpName}");
                        return -1;
                    }

                    double dZPos = 0.0;
                    //if (IsPositionProbeCardZSafety() == false) //무조건 움직이자.
                    //TaktStart("ProbeCardZSafety");
                    {
                        dZPos = GetTP(tpName, AxisNames.ProbeCardZ);
                        nRet = OnMoveAxisPositionOne(AxisProbeCardZ, dZPos);
                        if (nRet != 0)
                        {
                            Log.Write(UnitName, $"[OnMovePositionSafetyZ] ProbeCardZ move failed tp={tpName} posZ={dZPos}");
                            return -1;
                        }
                    }
                    //TaktEnd("ProbeCardZSafety");

                    //Gripper Ready로 하고 내려야함.
                    //TaktStart("GripperXReady");
                    //if (IsPositionGripperXClamp()) //걍 무조건 Ready 보냄.
                    {
                        nRet = MovePositionGripperXReady();
                        if (nRet != 0)
                        {
                            Log.Write(UnitName, $"[OnMovePositionSafetyZ] move failed: MovePositionGripperXReady");
                            return -1;
                        }
                    }
                    //TaktEnd("GripperXReady");

                    //TaktStart("ProbeZSafety");
                    if (IsPositionProbeCardZSafety() == false)
                    {
                        Log.Write(UnitName, $"[OnMovePositionSafetyZ] ProbeCardZ move failed - IsPositionProbeCardZSafety");
                        return -1;
                    }
                    //if (IsPositionProbeZSafety() == false) //무조건 움직이자.
                    {
                        dZPos = GetTP(tpName, AxisNames.ProbeZ);
                        nRet = OnMoveAxisPositionOne(AxisProbeZ, dZPos);
                        if (nRet != 0)
                        {
                            Log.Write(UnitName, $"[OnMovePositionSafetyZ] ProbeZ move failed tp={tpName} posZ={dZPos}");
                            return -1;
                        }
                    }
                    //TaktEnd("ProbeZSafety");

                    return nRet;
                }
                finally
                {
                    _isSafetyMoving = false;
                }
            }
        }

        private int IsMoveInterLockSafetyZ()
        {
            int nRet = 0;
            // Check Interlock.!!! 구문 넣을것.!!!
            // Safety 로 동작 중에 아래 인터락이 필요한가?
            // 
            if (IsRotaryReadyStable(3, 5) == false)
            {
                AxisProbeZ?.EmgStop();
                AxisProbeCardX?.EmgStop();
                AxisProbeCardY?.EmgStop();
                AxisProbeCardZ?.EmgStop();
                AxisSphereZ?.EmgStop();
                PostAlarm((int)AlarmKeys.eRotaryNotSafety);
                Log.Write(UnitName, nameof(IsMoveInterLockSafetyZ), $"Rotary moving interlock. {Rotary.GetIndexMovingDebugText()}");
                nRet = -1;
                return nRet;
            }

            return nRet;
        }

        public int MovePositionSafetyProbeZ(bool isFine = false)
        {
            Task<int> task = MovePositionAsyncSafetyProbeZ(isFine);
            while (IsEndTask(task) == false)
            {
                IsMoveInterLockSafetyProbeZ();
                Thread.Sleep(1);
            }
            return task.Result;
        }

        public Task<int> MovePositionAsyncSafetyProbeZ(bool isFine = false)
        {
            return Task.Run(() =>
            {
                return OnMovePositionSafetyProbeZ(isFine);
            });
        }
        private int OnMovePositionSafetyProbeZ(bool isFine = false)
        {
            int nRet = 0;
            _isSafetyMoving = true;
            try
            {
                string tpName = $"SafetyZone";
                var tpObj = GetTeachingPosition(tpName);
                if (tpObj == null)
                {
                    Log.Write(UnitName, $"[OnMovePosition_SafetyZone] Teaching not found: {tpName}");
                    return -1;
                }

                double dZPos = 0.0;
                if (IsPositionProbeCardZSafety() == false) //무조건 움직이자.
                {
                    dZPos = GetTP(tpName, AxisNames.ProbeCardZ);
                    nRet = OnMoveAxisPositionOne(AxisProbeCardZ, dZPos);
                    if (nRet != 0)
                    {
                        Log.Write(UnitName, $"[OnMovePositionSafetyZ] ProbeCardZ move failed tp={tpName} posZ={dZPos}");
                        return -1;
                    }
                }

                //Gripper Ready로 하고 내려야함.
                if (IsPositionGripperXClamp())
                {
                    nRet = MovePositionGripperXReady();
                    if (nRet != 0)
                    {
                        Log.Write(UnitName, $"[OnMovePositionSafetyZ] move failed: MovePositionGripperXReady");
                        return -1;
                    }
                }

                //if (IsPositionProbeZSafety() == false) //무조건 움직이자.
                {
                    dZPos = GetTP(tpName, AxisNames.ProbeZ);
                    nRet = OnMoveAxisPositionOne(AxisProbeZ, dZPos);
                    if (nRet != 0)
                    {
                        Log.Write(UnitName, $"[OnMovePositionSafetyZ] ProbeZ move failed tp={tpName} posZ={dZPos}");
                        return -1;
                    }
                }

                return nRet;
            }
            finally
            {
                _isSafetyMoving = false;
            }
        }
        private int IsMoveInterLockSafetyProbeZ()
        {
            int nRet = 0;
            // Check Interlock.!!! 구문 넣을것.!!!
            if (IsRotaryReadyStable(3, 5) == false)
            {
                AxisProbeZ?.EmgStop();
                AxisProbeCardX?.EmgStop();
                AxisProbeCardY?.EmgStop();
                AxisProbeCardZ?.EmgStop();
                AxisSphereZ?.EmgStop();
                PostAlarm((int)AlarmKeys.eRotaryNotSafety);
                Log.Write(UnitName, nameof(IsMoveInterLockTopContact_Index_Up), $"Rotary moving interlock. {Rotary.GetIndexMovingDebugText()}");
                nRet = -1;
                return nRet;
            }

            return nRet;
        }

        public int MovePositionSafetyProbeCardZ(bool isFine = false)
        {
            Task<int> task = MovePositionAsyncSafetyProbeCardZ(isFine);
            while (IsEndTask(task) == false)
            {
                IsMoveInterLockSafetyProbeCardZ();
                Thread.Sleep(1);
            }
            return task.Result;
        }
        public Task<int> MovePositionAsyncSafetyProbeCardZ(bool isFine = false)
        {
            return Task.Run(() =>
            {
                return OnMovePositionSafetyProbeCardZ(isFine);
            });
        }
        private int OnMovePositionSafetyProbeCardZ(bool isFine = false)
        {
            int nRet = 0;
            _isSafetyMoving = true;
            try
            {
                string tpName = $"SafetyZone";
                var tpObj = GetTeachingPosition(tpName);
                if (tpObj == null)
                {
                    Log.Write(UnitName, $"[OnMovePosition_SafetyZone] Teaching not found: {tpName}");
                    return -1;
                }

                double dZPos = 0.0;
                //if (IsPositionProbeCardZSafety() == false) //무조건 움직이자.
                {
                    dZPos = GetTP(tpName, AxisNames.ProbeCardZ);
                    nRet = OnMoveAxisPositionOne(AxisProbeCardZ, dZPos);
                    if (nRet != 0)
                    {
                        Log.Write(UnitName, $"[OnMovePositionSafetyZ] ProbeCardZ move failed tp={tpName} posZ={dZPos}");
                        return -1;
                    }
                }

                return nRet;
            }
            finally
            {
                _isSafetyMoving = false;
            }
        }
        private int IsMoveInterLockSafetyProbeCardZ()
        {
            int nRet = 0;
            // Check Interlock.!!! 구문 넣을것.!!!
            if (IsRotaryReadyStable(3, 5) == false)
            {
                AxisProbeZ?.EmgStop();
                AxisProbeCardX?.EmgStop();
                AxisProbeCardY?.EmgStop();
                AxisProbeCardZ?.EmgStop();
                AxisSphereZ?.EmgStop();
                PostAlarm((int)AlarmKeys.eRotaryNotSafety);
                Log.Write(UnitName, nameof(IsMoveInterLockSafetyProbeCardZ), $"Rotary moving interlock. {Rotary.GetIndexMovingDebugText()}");
                nRet = -1;
                return nRet;
            }

            return nRet;
        }

        public int MovePositionSphereZDown(bool isFine = false)
        {
            Task<int> task = MovePositionAsyncSphereZDown(isFine);
            while (IsEndTask(task) == false)
            {
                IsMoveInterLockSphereZDown();
                Thread.Sleep(1);
            }
            return task.Result;
        }
        public Task<int> MovePositionAsyncSphereZDown(bool isFine = false)
        {
            return Task.Run(() =>
            {
                return OnMovePositionSphereZDown(isFine);
            });
        }
        private int OnMovePositionSphereZDown(bool isFine = false)
        {
            return MoveTeachingPositionOnce((int)IndexChipProbeControllerRecipe.TeachingPositionName.SphereZ_Measure, isFine);
        }
        private int IsMoveInterLockSphereZDown()
        {
            int nRet = 0;
            // Check Interlock.!!! 구문 넣을것.!!!
            if (IsRotaryReadyStable(3, 5) == false)
            {
                AxisProbeZ?.EmgStop();
                AxisProbeCardX?.EmgStop();
                AxisProbeCardY?.EmgStop();
                AxisProbeCardZ?.EmgStop();
                AxisSphereZ?.EmgStop();
                PostAlarm((int)AlarmKeys.eRotaryNotSafety);
                Log.Write(UnitName, nameof(IsMoveInterLockTopContact_Index_Up), $"Rotary moving interlock. {Rotary.GetIndexMovingDebugText()}");
                nRet = -1;
                return nRet;
            }

            return nRet;
        }

        public int MovePositionSphereZReady(bool isFine = false)
        {
            Task<int> task = MovePositionAsyncSphereZReady(isFine);
            while (IsEndTask(task) == false)
            {
                IsMoveInterLockSphereZReady();
                Thread.Sleep(1);
            }
            return task.Result;
        }
        public Task<int> MovePositionAsyncSphereZReady(bool isFine = false)
        {
            return Task.Run(() =>
            {
                return OnMovePositionSphereZReady(isFine);
            });
        }
        private int OnMovePositionSphereZReady(bool isFine = false)
        {
            return MoveTeachingPositionOnce((int)IndexChipProbeControllerRecipe.TeachingPositionName.SphereZ_Ready, isFine);
        }
        private int IsMoveInterLockSphereZReady()
        {
            int nRet = 0;
            // Check Interlock.!!! 구문 넣을것.!!!
            if (IsRotaryReadyStable(3, 5) == false)
            {
                AxisProbeZ?.EmgStop();
                AxisProbeCardX?.EmgStop();
                AxisProbeCardY?.EmgStop();
                AxisProbeCardZ?.EmgStop();
                AxisSphereZ?.EmgStop();
                PostAlarm((int)AlarmKeys.eRotaryNotSafety);
                Log.Write(UnitName, nameof(IsMoveInterLockTopContact_Index_Up), $"Rotary moving interlock. {Rotary.GetIndexMovingDebugText()}");
                nRet = -1;
                return nRet;
            }

            return nRet;
        }


        public int MovePositionTopContact_Index_Up(int nIndex = 0, bool isFine = false)
        {
            if (IsTopContactIndexZUp(nIndex) == true)
            {
                return 0;
            }

            if (IsTopRequired() == false)
            {
                Log.Write(UnitName, "MovePositionTopContact_Index_Contact", "Not in Top Mode.");
                return -1;
            }

            if (nIndex != GetProbeIndexNo())
            {
                Log.Write(UnitName, "MovePositionTopContact_Index_Contact", "Index Number does not match.");
                return -1;
            }


            Task<int> task = MovePositionAsyncTopContact_Index_Up(nIndex, isFine);
            while (IsEndTask(task) == false)
            {
                IsMoveInterLockTopContact_Index_Up();
                Thread.Sleep(1);
            }
            return task.Result;
        }
        public Task<int> MovePositionAsyncTopContact_Index_Up(int nIndex = 0, bool isFine = false)
        {
            return Task.Run(() =>
            {
                return OnMovePositionTopContact_Index_Up(nIndex, isFine);
            });
        }
        private int OnMovePositionTopContact_Index_Up(int nIndex = 0, bool isFine = false)
        {
            int nRet = 0;
            // nIndex 처리
            int teachingIdx = 0;
            if (nIndex >= 0 && nIndex < 8)
            {
                teachingIdx = nIndex + 1;
            }
            else
            {
                Log.Write(UnitName, $"[OnMovePositionTopContact_Index_Up] Invalid index {nIndex}. Range 0~7");
                return -1;
            }

            string tpName = Recipe.GetTopContactName(nIndex);
            var tpObj = GetTeachingPosition(tpName);
            if (tpObj == null)
            {
                Log.Write(UnitName, $"[OnMovePositionTopContact_Index_Up] Teaching not found: {tpName}");
                return -1;
            }

            double dTPos = GetTP(tpName, AxisNames.ProbeZ);
            nRet = OnMoveAxisPositionOne(AxisProbeZ, dTPos);
            if (nRet != 0)
            {
                Log.Write(UnitName, $"[OnMovePositionTopContact_Index_Up] ToolT move failed tp={tpName} pos={dTPos}");
                return -1;
            }

            while (true)
            {
                if (IsStop)
                {
                    return 0;
                }

                if (IsTopContactIndexZUp(nIndex))
                {
                    break;
                }
                Thread.Sleep(1);
            }

            return nRet;
        }
        private int IsMoveInterLockTopContact_Index_Up()
        {
            int nRet = 0;
            // Check Interlock.!!! 구문 넣을것.!!!
            if (IsRotaryReadyStable(3, 5) == false)
            {
                AxisProbeZ?.EmgStop();
                AxisProbeCardX?.EmgStop();
                AxisProbeCardY?.EmgStop();
                AxisProbeCardZ?.EmgStop();
                AxisSphereZ?.EmgStop();
                PostAlarm((int)AlarmKeys.eRotaryNotSafety);
                Log.Write(UnitName, nameof(IsMoveInterLockTopContact_Index_Up),
                    $"Rotary moving interlock. {Rotary.GetIndexMovingDebugText()}");
                nRet = -1;
                return nRet;
            }
            return nRet;
        }

        public int MovePositionTopContact_Index_Ready(int nIndex = 0, bool isFine = false)
        {
            Task<int> task = MovePositionAsyncTopContact_Index_Ready(nIndex, isFine);
            while (IsEndTask(task) == false)
            {
                IsMoveInterLockTopContact_Index_Ready();
                Thread.Sleep(1);
            }
            return task.Result;
        }
        public Task<int> MovePositionAsyncTopContact_Index_Ready(int nIndex = 0, bool isFine = false)
        {
            return Task.Run(() =>
            {
                return OnMovePositionTopContact_Index_Ready(nIndex, isFine);
            });
        }
        private int OnMovePositionTopContact_Index_Ready(int nIndex = 0, bool isFine = false)
        {
            int nRet = 0;
            // nIndex 처리
            int teachingIdx = 0;
            if (nIndex >= 0 && nIndex < 8)
                teachingIdx = nIndex + 1; // 0-based 입력으로 판단
            else
            {
                Log.Write(UnitName, $"[OnMovePositionTopContact_Index_Ready] Invalid index {nIndex}. Range 0~7 or 1~8");
                return -1;
            }
            //string tpName = $"Top_Index{teachingIdx}_Ready";
            string tpName = Recipe.GetTopReadyName(nIndex);
            var tpObj = GetTeachingPosition(tpName);
            if (tpObj == null)
            {
                Log.Write(UnitName, $"[OnMovePositionTopContact_Index_Ready] Teaching not found: {tpName}");
                return -1;
            }

            double dPosZ = GetTP(tpName, AxisNames.ProbeZ);
            nRet = OnMoveAxisPositionOne(AxisProbeZ, dPosZ);
            if (nRet != 0)
            {
                Log.Write(UnitName, $"[OnMovePositionTopContact_Index_Ready] ToolT move failed tp={tpName} pos={dPosZ}");
                return -1;
            }
            return nRet;
        }
        private int IsMoveInterLockTopContact_Index_Ready()
        {
            int nRet = 0;
            // Check Interlock.!!! 구문 넣을것.!!!
            if (IsRotaryReadyStable(3, 5) == false)
            {
                AxisProbeZ?.EmgStop();
                AxisProbeCardX?.EmgStop();
                AxisProbeCardY?.EmgStop();
                AxisProbeCardZ?.EmgStop();
                AxisSphereZ?.EmgStop();
                PostAlarm((int)AlarmKeys.eRotaryNotSafety);
                Log.Write(UnitName, nameof(IsMoveInterLockTopContact_Index_Ready),
                     $"Rotary moving interlock. {Rotary.GetIndexMovingDebugText()}");
                nRet = -1;
                return nRet;
            }
            return nRet;
        }

        public int MovePositionBottomContact_Index_Up(int nIndex = 0, bool isFine = false)
        {
            if (IsBottomContactIndexZUp(nIndex) == true)
            {
                return 0;
            }

            if (IsTopRequired() == true)
            {
                Log.Write(UnitName, "MovePositionTopContact_Index_Up", "Not in Bottom Mode.");
                return -1;
            }

            if (nIndex != GetProbeIndexNo())
            {
                Log.Write(UnitName, "MovePositionTopContact_Index_Up", "Index Number does not match.");
                return -1;
            }

            Task<int> task = MovePositionAsyncBottomContact_Index_Up(nIndex, isFine);
            while (IsEndTask(task) == false)
            {
                IsMoveInterLockBottomContact_Index_Up(nIndex);
                Thread.Sleep(1);
            }
            return task.Result;
        }
        public Task<int> MovePositionAsyncBottomContact_Index_Up(int nIndex = 0, bool isFine = false)
        {
            return Task.Run(() =>
            {
                return OnMovePositionBottomContact_Index_Up(nIndex, isFine);
            });
        }
        private int OnMovePositionBottomContact_Index_Up(int nIndex = 0, bool isFine = false)
        {
            int nRet = 0;
            // nIndex 처리 
            int teachingIdx = 0;
            if (nIndex >= 0 && nIndex < 8)
            {
                teachingIdx = nIndex + 1;
            }
            else
            {
                Log.Write(UnitName, $"[OnMovePositionBottomContact_Index_Up] Invalid index {nIndex}.");
                return -1;
            }

            string tpName = $"SafetyZone";
            var tpObj = GetTeachingPosition(tpName);
            if (tpObj == null)
            {
                Log.Write(UnitName, $"[OnMovePosition_SafetyZone] Teaching not found: {tpName}");
                return -1;
            }

            double dZPos = 0.0;
            if (IsBottomContactIndexZUp(nIndex))
            {
                dZPos = GetTP(tpName, AxisNames.ProbeCardZ);
                nRet = OnMoveAxisPositionOne(AxisProbeCardZ, dZPos);
                if (nRet != 0)
                {
                    Log.Write(UnitName, $"[OnMovePositionBottomContact_Index_Ready] ToolT move failed tp={tpName} posZ={dZPos}");
                    return -1;
                }
            }

            tpName = Recipe.GetBottomContactName(nIndex);
            tpObj = GetTeachingPosition(tpName);
            if (tpObj == null)
            {
                Log.Write(UnitName, $"[OnMovePositionBottomContact_Index_Up] Teaching not found: {tpName}");
                return -1;
            }

            double dXPos = GetTP(tpName, AxisNames.ProbeCardX);
            double dYPos = GetTP(tpName, AxisNames.ProbeCardY);
            nRet = OnMoveAxisPositionOne(AxisProbeCardX, dXPos);
            if (nRet != 0)
            {
                Log.Write(UnitName, $"[OnMovePositionBottomContact_Index_Up] ToolT move failed tp={tpName} posX={dXPos}");
                return -1;
            }
            nRet = OnMoveAxisPositionOne(AxisProbeCardY, dYPos);
            if (nRet != 0)
            {
                Log.Write(UnitName, $"[OnMovePositionBottomContact_Index_Up] ToolT move failed tp={tpName} posY={dYPos}");
                return -1;
            }

            dZPos = GetTP(tpName, AxisNames.ProbeCardZ);
            if (Config.overdriveUse == true) //Overdrive 사용
            {
                double overdriveDist = Config.overdriveDist;
                if (overdriveDist > 0.5)
                {
                    Log.Write(UnitName, $"[OnMovePositionBottomContact_Index_Up] Overdrive distance {overdriveDist} is too large, limiting to 0.5");
                    overdriveDist = 0.5;
                }
                else if (overdriveDist < 0)
                {
                    Log.Write(UnitName, $"[OnMovePositionBottomContact_Index_Up] Overdrive distance {overdriveDist} is negative, disabling overdrive");
                    overdriveDist = 0;
                }

                double dDistToTarget = Math.Abs(dZPos + overdriveDist);
                nRet = OnMoveAxisPositionOne(AxisProbeCardZ, dDistToTarget);
            }
            else
            {
                nRet = OnMoveAxisPositionOne(AxisProbeCardZ, dZPos);
            }
            if (nRet != 0)
            {
                Log.Write(UnitName, $"[OnMovePositionBottomContact_Index_Up] ToolT move failed tp={tpName} posZ={dZPos}");
                return -1;
            }

            return nRet;
        }
        private int IsMoveInterLockBottomContact_Index_Up(int nIndex)
        {
            int nRet = 0;
            // Check Interlock.!!! 구문 넣을것.!!!
            if (IsRotaryReadyStable(3, 5) == false)
            {
                AxisProbeZ?.EmgStop();
                AxisProbeCardX?.EmgStop();
                AxisProbeCardY?.EmgStop();
                AxisProbeCardZ?.EmgStop();
                AxisSphereZ?.EmgStop();
                PostAlarm((int)AlarmKeys.eRotaryNotSafety);
                Log.Write(UnitName, nameof(IsMoveInterLockBottomContact_Index_Up), $"Rotary moving interlock. {Rotary.GetIndexMovingDebugText()}");
                nRet = -1;
                return nRet;
            }

            // [ADD] ProbeCardZ <-> ProbeZ 거리 인터락 (이동 중 지속 감시)
            if (!CheckDistanceInterlockAndStopIfViolation(nameof(IsMoveInterLockBottomContact_Index_Up)))
            {
                // 함수 내부에 알람 있음.
                Log.Write(UnitName, $"[IsMoveInterLockBottomContact_Index_Up] Distance interlock violated");
                return -1;
            }

            if (IsPositionProbeZGripperIndexUp(nIndex) == false)
            {
                AxisProbeCardZ?.EmgStop();
                PostAlarm((int)AlarmKeys.eProbeZNotIndexUp);
                Log.Write(UnitName, $"[IsMoveInterLockBottomContact_Index_Up] GripperX not Index Up");
                nRet = -1;
                return nRet;
            }

            return nRet;
        }

        public int MovePositionBottomContact_Index_Ready(int nIndex = 0, bool isFine = false)
        {
            Task<int> task = MovePositionAsyncBottomContact_Index_Ready(nIndex, isFine);
            while (IsEndTask(task) == false)
            {
                IsMoveInterLockBottomContact_Index_Ready();
                Thread.Sleep(1);
            }
            return task.Result;
        }
        public Task<int> MovePositionAsyncBottomContact_Index_Ready(int nIndex = 0, bool isFine = false)
        {
            return Task.Run(() =>
            {
                return OnMovePositionBottomContact_Index_Ready(nIndex, isFine);
            });
        }
        private int OnMovePositionBottomContact_Index_Ready(int nIndex = 0, bool isFine = false)
        {
            // 여기 확인하자. 2026-01-19
            int nRet = 0;
            // nIndex 처리
            int teachingIdx = 0;
            if (nIndex >= 0 && nIndex < 8)
            {
                teachingIdx = nIndex + 1;
            }
            else
            {
                Log.Write(UnitName, $"[OnMovePositionBottomContact_Index_Ready] Invalid index {nIndex}. Range 0~7 or 1~8");
                return -1;
            }

            string tpName = $"SafetyZone";
            var tpObj = GetTeachingPosition(tpName);
            if (tpObj == null)
            {
                Log.Write(UnitName, $"[OnMovePosition_SafetyZone] Teaching not found: {tpName}");
                return -1;
            }

            double dZPos = 0.0;
            if (IsBottomContactIndexZUp(nIndex))
            {
                dZPos = GetTP(tpName, AxisNames.ProbeCardZ);
                nRet = OnMoveAxisPositionOne(AxisProbeCardZ, dZPos);
                if (nRet != 0)
                {
                    Log.Write(UnitName, $"[OnMovePositionBottomContact_Index_Ready] ToolT move failed tp={tpName} posZ={dZPos}");
                    return -1;
                }
            }

            //tpName = $"Bottom_Index{teachingIdx}_Ready";
            tpName = Recipe.GetBottomReadyName(nIndex);
            tpObj = GetTeachingPosition(tpName);
            if (tpObj == null)
            {
                Log.Write(UnitName, $"[OnMovePositionBottomContact_Index_Ready] Teaching not found: {tpName}");
                return -1;
            }
            double dXPos = GetTP(tpName, AxisNames.ProbeCardX);
            double dYPos = GetTP(tpName, AxisNames.ProbeCardY);
            nRet = OnMoveAxisPositionOne(AxisProbeCardX, dXPos);
            if (nRet != 0)
            {
                Log.Write(UnitName, $"[OnMovePositionBottomContact_Index_Ready] ToolT move failed tp={tpName} posX={dXPos}");
                return -1;
            }
            nRet = OnMoveAxisPositionOne(AxisProbeCardY, dYPos);
            if (nRet != 0)
            {
                Log.Write(UnitName, $"[OnMovePositionBottomContact_Index_Ready] ToolT move failed tp={tpName} posY={dYPos}");
                return -1;
            }

            dZPos = GetTP(tpName, AxisNames.ProbeCardZ);
            nRet = OnMoveAxisPositionOne(AxisProbeCardZ, dZPos);
            if (nRet != 0)
            {
                Log.Write(UnitName, $"[OnMovePositionBottomContact_Index_Ready] ToolT move failed tp={tpName} posZ={dZPos}");
                return -1;
            }

            return nRet;
        }
        private int IsMoveInterLockBottomContact_Index_Ready()
        {
            int nRet = 0;
            // Check Interlock.!!! 구문 넣을것.!!!
            if (IsRotaryReadyStable(3, 5) == false)
            {
                AxisProbeZ?.EmgStop();
                AxisProbeCardX?.EmgStop();
                AxisProbeCardY?.EmgStop();
                AxisProbeCardZ?.EmgStop();
                AxisSphereZ?.EmgStop();
                PostAlarm((int)AlarmKeys.eRotaryNotSafety);
                Log.Write(UnitName, nameof(IsMoveInterLockBottomContact_Index_Ready), $"Rotary moving interlock. {Rotary.GetIndexMovingDebugText()}");
                nRet = -1;
                return nRet;
            }
            return nRet;
        }

        public int OnMovePositionBottomContact_Index_Ready_XYOnly(int nIndex = 0, bool isFine = false)
        {
            if (nIndex < 0 || nIndex >= 8)
            {
                Log.Write(UnitName, nameof(OnMovePositionBottomContact_Index_Ready_XYOnly),
                    $"Invalid index {nIndex}. Range 0~7");
                return -1;
            }

            string tpName = Recipe.GetBottomReadyName(nIndex);
            var tpObj = GetTeachingPosition(tpName);
            if (tpObj == null)
            {
                Log.Write(UnitName, nameof(OnMovePositionBottomContact_Index_Ready_XYOnly),
                    $"Teaching not found: {tpName}");
                return -1;
            }

            int nRet = 0;
            double dXPos = GetTP(tpName, AxisNames.ProbeCardX);
            double dYPos = GetTP(tpName, AxisNames.ProbeCardY);

            nRet |= OnMoveAxisPositionOne(AxisProbeCardX, dXPos);
            nRet |= OnMoveAxisPositionOne(AxisProbeCardY, dYPos);

            if (nRet != 0)
            {
                Log.Write(UnitName, nameof(OnMovePositionBottomContact_Index_Ready_XYOnly),
                    $"Move failed tp={tpName} X={dXPos}, Y={dYPos}");
                return -1;
            }

            return 0;
        }

        public Task<int> MovePositionAsyncBottomContact_Index_Ready_XYOnly(int nIndex = 0, bool isFine = false)
        {
            return Task.Run(() => OnMovePositionBottomContact_Index_Ready_XYOnly(nIndex, isFine));
        }

        public int MovePositionGripperXReady(bool isFine = false)
        {
            Task<int> task = MovePositionAsyncGripperXReady(isFine);
            while (IsEndTask(task) == false)
            {
                IsMoveInterLockGripperXReady();
                Thread.Sleep(1);
            }
            return task.Result;
        }
        public Task<int> MovePositionAsyncGripperXReady(bool isFine = false)
        {
            return Task.Run(() =>
            {
                return OnMovePositionGripperXReady(isFine);
            });
        }
        private int OnMovePositionGripperXReady(bool isFine = false)
        {
            return MoveTeachingPositionOnce((int)IndexChipProbeControllerRecipe.TeachingPositionName.GripperX_Ready, isFine);
        }
        private int IsMoveInterLockGripperXReady()
        {
            int nRet = 0;
            // Check Interlock.!!! 구문 넣을것.!!!
            if (IsRotaryReadyStable(3, 5) == false)
            {
                AxisProbeZ?.EmgStop();
                AxisProbeCardX?.EmgStop();
                AxisProbeCardY?.EmgStop();
                AxisProbeCardZ?.EmgStop();
                AxisSphereZ?.EmgStop();
                PostAlarm((int)AlarmKeys.eRotaryNotSafety);
                Log.Write(UnitName, nameof(IsMoveInterLockGripperXReady), $"Rotary moving interlock. {Rotary.GetIndexMovingDebugText()}");
                nRet = -1;
                return nRet;
            }

            return nRet;
        }
        public bool IsPositionGripperXReady()
        {
            bool bRet = false;
            bRet = IsAxisInTeachingPosition(AxisGripperX,
                IndexChipProbeControllerRecipe.TeachingPositionName.GripperX_Ready.ToString(),
                AxisNames.GripperX);
            return bRet;
        }

        public int MovePositionGripperXClamp(bool isFine = false)
        {
            Task<int> task = MovePositionAsyncGripperXClamp(isFine);
            while (IsEndTask(task) == false)
            {
                IsMoveInterLockGripperXClamp();
                Thread.Sleep(1);
            }
            return task.Result;
        }
        public Task<int> MovePositionAsyncGripperXClamp(bool isFine = false)
        {
            return Task.Run(() =>
            {
                return OnMovePositionGripperXClamp(isFine);
            });
        }
        private int OnMovePositionGripperXClamp(bool isFine = false)
        {
            return MoveTeachingPositionOnce((int)IndexChipProbeControllerRecipe.TeachingPositionName.GripperX_Clamp, isFine);
        }
        private int IsMoveInterLockGripperXClamp()
        {
            int nRet = 0;
            // Check Interlock.!!! 구문 넣을것.!!!
            if (IsRotaryReadyStable(3, 5) == false)
            {
                AxisProbeZ?.EmgStop();
                AxisProbeCardX?.EmgStop();
                AxisProbeCardY?.EmgStop();
                AxisProbeCardZ?.EmgStop();
                AxisSphereZ?.EmgStop();
                PostAlarm((int)AlarmKeys.eRotaryNotSafety);
                Log.Write(UnitName, nameof(IsMoveInterLockGripperXClamp), $"Rotary moving interlock. {Rotary.GetIndexMovingDebugText()}");
                nRet = -1;
                return nRet;
            }

            return nRet;
        }
        public bool IsPositionGripperXClamp()
        {
            bool bRet = false;
            bRet = IsAxisInTeachingPosition(AxisGripperX,
                IndexChipProbeControllerRecipe.TeachingPositionName.GripperX_Clamp.ToString(),
                AxisNames.GripperX);
            return bRet;
        }

        #region Z-Axis Sync Move (ProbeZ & ProbeCardZ)

        /// <summary>
        /// BottomContact_Index_Ready 와 ProbeZGripperIndexUp 을 
        /// 하나의 함수 안에서 동시에 처리합니다. (XY축 선 이동 -> Z축 동시 이동)
        /// </summary>
        public int SyncProbeZGripperUpAndBottomReady(int nIndex, bool bFineSpeed = false)
        {
            int nRet = 0;
            if (RunMode == UnitRunMode.Manual)
            {
                //this.CurrentFunc = SyncProbeZGripperUpAndBottomReady;
            }

            // 1. ProbeCard X, Y 이동 (Z축이 움직이기 전에 평면 위치를 먼저 맞춤)
            string tpNameReady = Recipe.GetBottomReadyName(nIndex);
            var tpReady = GetTeachingPosition(tpNameReady);
            if (tpReady == null)
            {
                Log.Write(UnitName, $"[SyncProbeZGripperUpAndBottomReady] Teaching not found tpNameReady: {tpNameReady}");
                return -1;
            }

            double dXPos = GetTP(tpNameReady, AxisNames.ProbeCardX);
            double dYPos = GetTP(tpNameReady, AxisNames.ProbeCardY);

            nRet |= OnMoveAxisPositionOne(AxisProbeCardX, dXPos);
            nRet |= OnMoveAxisPositionOne(AxisProbeCardY, dYPos);
            if (nRet != 0)
            {
                Log.Write(UnitName, "[SyncProbeZGripperUpAndBottomReady] ProbeCard X/Y move failed");
                return -1;
            }

            // 2. 동기화할 Z축 목표 위치 획득
            // ProbeZ의 Contact 위치
            //string tpNameProbeZ = IndexChipProbeControllerRecipe.TeachingPositionName.GripperX_Index_Contact.ToString();
            string tpNameProbeZ = Recipe.GetGripperContactName(nIndex);
            var tpProbeZ = GetTeachingPosition(tpNameProbeZ);
            if (tpProbeZ == null)
            {
                Log.Write(UnitName, $"[SyncProbeZGripperUpAndBottomReady] Teaching not found tpNameProbeZ: {tpNameProbeZ}");
                return -1;
            }

            double targetProbeZ = GetTP(tpNameProbeZ, AxisNames.ProbeZ);

            // 각 Z축의 SafetyZone 위치 획득
            string tpNameSafety = IndexChipProbeControllerRecipe.TeachingPositionName.SafetyZone.ToString();
            double probeZSafetyPos = GetTP(tpNameSafety, AxisNames.ProbeZ);
            double probeCardZSafetyPos = GetTP(tpNameSafety, AxisNames.ProbeCardZ);

            // ProbeZ의 실제 이동량 계산 (Contact 위치 - Safety 위치)
            double probeZMoveAmount = targetProbeZ - probeZSafetyPos;

            // [조건 적용] ProbeCardZ 타겟 = (ProbeCardZ Safety 위치) + (ProbeZ 이동량) + (오프셋)
            double offset = Config.SyncProbeCardZReady; //1.0;    // 필요 시 Config.dProbeZSyncOffset 등으로 설정
            if (offset > 2.0)
            {
                Log.Write(UnitName, $"[SyncProbeZGripperUpAndBottomReady] Offset {offset} is too large, limiting to 2.0");
                offset = 2.0;
            }
            else if (offset < -2.0)
            {
                Log.Write(UnitName, $"[SyncProbeZGripperUpAndBottomReady] Offset {offset} is too small, limiting to -2.0");
                offset = -2.0;
            }
            double targetProbeCardZ = probeCardZSafetyPos + probeZMoveAmount + offset;

            // 3. 속도/가감속 파라미터 가져오기 (사고 방지를 위해 ProbeZ의 파라미터로 통일)
            double velSync = AxisProbeZ.Config.MaxVelocity; // 사용 중인 QMC API에 맞게 속도 프로퍼티 접근
            double accSync = AxisProbeZ.Config.RunAcc;
            double decSync = AxisProbeZ.Config.RunDec;

            // 4. 동시 이동 함수 호출
            nRet = MoveProbeZAndProbeCardZSync(targetProbeZ, targetProbeCardZ, velSync, accSync, decSync, 20000, bFineSpeed);
            if (nRet != 0)
            {
                Log.Write(UnitName, "[SyncProbeZGripperUpAndBottomReady] Z Axis Sync Move failed");
                return -1;
            }

            return 0;
        }

        /// <summary>
        /// ProbeZ 와 ProbeCardZ 를 동일한 속도/파라미터로 동시에 절대(Abs) 이동.
        /// - Interlock 위반 시 두 축 Emergency Stop 후 -1 반환
        /// </summary>
        public int MoveProbeZAndProbeCardZSync(double targetProbeZ,
                                               double targetProbeCardZ,
                                               double vel, double acc, double dec,
                                               int timeoutMs = 20000,
                                               bool isFine = false)
        {
            var probeZ = AxisProbeZ;
            var probeCardZ = AxisProbeCardZ;

            if (probeZ == null || probeCardZ == null)
            {
                Log.Write(UnitName, "[MoveProbeZAndProbeCardZSync] Axis null");
                return -1;
            }

            // 사전 Interlock 감시
            if (Rotary != null && IsRotaryReadyStable(3, 5) == false)
            {
                probeZ.EmgStop();
                probeCardZ.EmgStop();
                PostAlarm((int)AlarmKeys.eRotaryNotSafety);
                Log.Write(UnitName, nameof(MoveProbeZAndProbeCardZSync), $"Rotary moving interlock. {Rotary.GetIndexMovingDebugText()}");
                return -1;
            }

            // 상부/하부 거리 인터락 사전 검사
            if (!CheckDistanceInterlockAndStopIfViolation(nameof(MoveProbeZAndProbeCardZSync)))
            {
                return -1;
            }

            // 동시에 시작
            int rc = 0;

            // QMC Motion API의 MoveAbs 오버로딩 중 속도/가감속을 명시할 수 있는 메서드를 호출합니다.
            // (만약 오버로딩이 없다면 rc |= probeZ.MoveAbs(targetProbeZ, RunMode == UnitRunMode.Auto, isFine); 로 변경)

            //파라미터로..빼놓자.. 사용/미사용 기능 적용하고...
            rc |= probeZ.MoveAbs(targetProbeZ, vel, acc, dec, probeZ.Config.AccJerkPercent);
            double dvel = probeCardZ.Config.MaxVelocity * 1.5;
            double dAcc = probeCardZ.Config.RunAcc * 1.5;
            double dDec = probeCardZ.Config.RunDec * 1.5;
            //probeCardZ가 probeZ 속도를 따라가지 못한다. Auto시에.
            rc |= probeCardZ.MoveAbs(targetProbeCardZ,
                    dvel, dAcc, dDec, probeCardZ.Config.AccJerkPercent);
            //rc |= probeCardZ.MoveAbs(targetProbeCardZ, vel, acc, dec, probeCardZ.Config.AccJerkPercent);
            if (rc != 0)
            {
                Log.Write(UnitName, "[MoveProbeZAndProbeCardZSync] MoveAbs start failed rc=" + rc);
                return -1;
            }

            var sw = timeoutMs > 0 ? Stopwatch.StartNew() : null;
            while (true)
            {
                bool probeZMoving = probeZ.IsMoveDone();
                bool probeCardZMoving = probeCardZ.IsMoveDone();

                // 완료
                if (probeZMoving && probeCardZMoving)
                {
                    break;
                }

                // 진행 중 Interlock 감시
                if (Rotary != null && IsRotaryReadyStable(3, 5) == false)
                {
                    probeZ.EmgStop();
                    probeCardZ.EmgStop();
                    PostAlarm((int)AlarmKeys.eRotaryNotSafety);
                    Log.Write(UnitName, nameof(MoveProbeZAndProbeCardZSync), $"Rotary moving interlock. {Rotary.GetIndexMovingDebugText()}");
                    return -1;
                }

                // 상/하부 축 거리 인터락 (충돌 방지 감시)
                if (!CheckDistanceInterlockAndStopIfViolation(nameof(MoveProbeZAndProbeCardZSync)))
                {
                    return -1;
                }

                // 타임아웃
                if (sw != null && sw.ElapsedMilliseconds > timeoutMs)
                {
                    probeZ.EmgStop();
                    probeCardZ.EmgStop();
                    Log.Write(UnitName, "[MoveProbeZAndProbeCardZSync] Timeout");
                    return -2;
                }

                Thread.Sleep(1);
            }

            if (probeZ.WaitMoveDone(-1) != 0)
            {
                Log.Write(UnitName, "MoveProbeZAndProbeCardZSync",
                    $"WaitMoveDone Timeout axis={probeZ.Name}");
                return -1;
            }

            if (probeCardZ.WaitMoveDone(-1) != 0)
            {
                Log.Write(UnitName, "MoveProbeZAndProbeCardZSync",
                    $"WaitMoveDone Timeout axis={probeCardZ.Name}");
                return -1;
            }

            return 0;
        }
        #endregion

        public int MovePositionProbeZGripperIndexUp(int nIndex, bool isFine = false)
        {
            Task<int> task = MovePositionAsyncProbeZGripperIndexUp(nIndex, isFine);
            while (IsEndTask(task) == false)
            {
                IsMoveInterLockProbeZGripperIndexUp();
                Thread.Sleep(1);
            }
            return task.Result;
        }
        public Task<int> MovePositionAsyncProbeZGripperIndexUp(int nIndex, bool isFine = false)
        {
            return Task.Run(() =>
            {
                return OnMovePositionProbeZGripperIndexUp(nIndex, isFine);
            });
        }
        private int OnMovePositionProbeZGripperIndexUp(int nIndex, bool isFine = false)
        {
            // 여기 확인하자. 2026-01-19
            int nRet = 0;
            // nIndex 처리
            int teachingIdx = 0;
            if (nIndex >= 0 && nIndex < 8)
            {
                teachingIdx = nIndex + 1;
            }
            else
            {
                Log.Write(UnitName, $"[OnMovePositionProbeZGripperIndexUp] Invalid index {nIndex}. Range 0~7 or 1~8");
                return -1;
            }

            string tpName = $"SafetyZone";
            var tpObj = GetTeachingPosition(tpName);
            if (tpObj == null)
            {
                Log.Write(UnitName, $"[OnMovePositionProbeZGripperIndexUp] Teaching not found: {tpName}");
                return -1;
            }

            double dZPos = 0.0;
            if (IsPositionProbeZGripperIndexUp(nIndex))
            {
                dZPos = GetTP(tpName, AxisNames.ProbeZ);
                nRet = OnMoveAxisPositionOne(AxisProbeZ, dZPos);
                if (nRet != 0)
                {
                    Log.Write(UnitName, $"[OnMovePositionProbeZGripperIndexUp] ToolT move failed tp={tpName} posZ={dZPos}");
                    return -1;
                }
            }

            tpName = Recipe.GetGripperContactName(nIndex);
            tpObj = GetTeachingPosition(tpName);
            if (tpObj == null)
            {
                Log.Write(UnitName, $"[OnMovePositionProbeZGripperIndexUp] Teaching not found: {tpName}");
                return -1;
            }

            dZPos = GetTP(tpName, AxisNames.ProbeZ);
            nRet = OnMoveAxisPositionOne(AxisProbeZ, dZPos);
            if (nRet != 0)
            {
                Log.Write(UnitName, $"[OnMovePositionProbeZGripperIndexUp] ToolT move failed tp={tpName} posZ={dZPos}");
                return -1;
            }

            return nRet;
            //return MoveTeachingPositionOnce((int)IndexChipProbeControllerRecipe.TeachingPositionName.GripperX_Index_Contact, isFine);
        }
        private int IsMoveInterLockProbeZGripperIndexUp()
        {
            int nRet = 0;
            // Check Interlock.!!! 구문 넣을것.!!!
            if (IsRotaryReadyStable(3, 5) == false)
            {
                AxisProbeZ?.EmgStop();
                AxisProbeCardX?.EmgStop();
                AxisProbeCardY?.EmgStop();
                AxisProbeCardZ?.EmgStop();
                AxisSphereZ?.EmgStop();
                PostAlarm((int)AlarmKeys.eRotaryNotSafety);
                Log.Write(UnitName, nameof(IsMoveInterLockProbeZGripperIndexUp), $"Rotary moving interlock. {Rotary.GetIndexMovingDebugText()}");
                nRet = -1;
                return nRet;
            }

            // [ADD] ProbeCardZ <-> ProbeZ 거리 인터락 (ProbeZ 이동 중 지속 감시)
            if (!CheckDistanceInterlockAndStopIfViolation(nameof(IsMoveInterLockProbeZGripperIndexUp)))
            {
                //함수 내부에 알람 있음.
                Log.Write(UnitName, $"[IsMoveInterLockGripperXIndexUp] Distance interlock violated");
                return -1;
            }

            return nRet;
        }
        public bool IsPositionProbeZGripperIndexUp(int nIndex)
        {
            int idx;
            if (nIndex >= 1 && nIndex <= 8)
                idx = nIndex + 1;
            else if (nIndex >= 0 && nIndex < 8)
                idx = nIndex + 1;
            else
                return false;

            string tpName = Recipe.GetGripperContactName(nIndex);
            return IsAxisInTeachingPosition(AxisProbeZ, tpName, AxisNames.ProbeZ);

            //bool bRet = false;
            //bRet = IsAxisInTeachingPosition(AxisProbeZ,
            //    IndexChipProbeControllerRecipe.TeachingPositionName.GripperX_Index_Contact.ToString(),
            //    AxisNames.ProbeZ);
            //return bRet;
        }


        #region Z-Axis In-Position Checkers (Teaching별 Z축만 검사)
        // Sphere Z 개별 확인
        public bool IsSphereZAtReady()
            => IsAxisInTeachingPosition(AxisSphereZ,
                IndexChipProbeControllerRecipe.TeachingPositionName.SphereZ_Ready.ToString(),
                AxisNames.SphereZ);

        public bool IsSphereZAtDown()
            => IsAxisInTeachingPosition(AxisSphereZ,
                IndexChipProbeControllerRecipe.TeachingPositionName.SphereZ_Measure.ToString(),
                AxisNames.SphereZ);

        // SafetyZone에서 각 Z축만 개별 확인 (기존 함수 활용)
        public bool IsProbeZAtSafetyZone()
        {
            return IsPositionProbeZSafety();       // SafetyZone의 ProbeZ만 검사
        }
        public bool IsPositionProbeCardZSafety()
        {
            return IsAxisProbeCardZSafetyPos();   // SafetyZone의 ProbeCardZ만 검사
        }

        // TopContact: ProbeZ만 검사 (Index 0~7 또는 1~8 허용)
        public bool IsTopContactIndexZUp(int nIndex)
        {
            int idx;
            if (nIndex >= 1 && nIndex <= 8)
                idx = nIndex + 1;
            else if (nIndex >= 0 && nIndex < 8)
                idx = nIndex + 1;
            else
                return false;

            //string tpName = $"Top_Index{idx}_Up";
            string tpName = Recipe.GetTopContactName(nIndex);
            return IsAxisInTeachingPosition(AxisProbeZ, tpName, AxisNames.ProbeZ);
        }

        public bool IsTopContactIndexZReady(int nIndex)
        {
            int idx;
            if (nIndex >= 1 && nIndex <= 8)
                idx = nIndex + 1;
            else if (nIndex >= 0 && nIndex < 8)
                idx = nIndex + 1;
            else return false;

            //string tpName = $"Top_Index{idx}_Ready";
            string tpName = Recipe.GetTopReadyName(nIndex);
            return IsAxisInTeachingPosition(AxisProbeZ, tpName, AxisNames.ProbeZ);
        }

        // BottomContact: ProbeCardZ만 검사 (Index 0~7 또는 1~8 허용)
        public bool IsBottomContactIndexZUp(int nIndex)
        {
            int idx;
            if (nIndex >= 1 && nIndex <= 8)
                idx = nIndex + 1;
            else if (nIndex >= 0 && nIndex < 8)
                idx = nIndex + 1;
            else
                return false;

            //string tpName = $"Bottom_Index{idx}_Up";
            string tpName = Recipe.GetBottomContactName(nIndex);
            return IsAxisInTeachingPosition(AxisProbeCardZ, tpName, AxisNames.ProbeCardZ);
        }

        public bool IsBottomIndexZReady(int nIndex)
        {
            int idx;
            if (nIndex >= 1 && nIndex <= 8)
                idx = nIndex + 1;
            else if (nIndex >= 0 && nIndex < 8)
                idx = nIndex + 1;
            else
                return false;

            //string tpName = $"Bottom_Index{idx}_Ready";
            string tpName = Recipe.GetBottomReadyName(nIndex);
            return IsAxisInTeachingPosition(AxisProbeCardZ, tpName, AxisNames.ProbeCardZ);
        }
        #endregion



        #region Safety Position Helpers (Individual Axis)
        /// <summary>
        /// 내부 헬퍼: TeachingPosition에서 특정 축 목표값을 얻는다.
        /// </summary>
        private bool TryGetTeachingAxisPosition(string tpName, string axisName, out double target)
        {
            target = 0.0;
            var tp = GetTeachingPosition(tpName);
            if (tp == null || tp.AxisPositions == null)
                return false;
            return tp.AxisPositions.TryGetValue(axisName, out target);
        }

        /// <summary>
        /// 공통 판정: 축이 null 이면 OK 로 간주. TeachingPosition 없으면 false.
        /// </summary>
        private bool IsAxisInTeachingPosition(MotionAxis axis, string tpName, string axisName)
        {
            if (axis == null)
                return true; // 미바인딩은 안전하다고 간주 (필요시 false 로 변경)
            if (!TryGetTeachingAxisPosition(tpName, axisName, out var target))
                return false;
            return axis.InPosition(target);
        }
        #endregion

        public bool IsPositionProbeZSafety()
        {
            // 현재 실제 위치 읽기
            double currentPos;
            try
            {
                currentPos = AxisProbeZ.GetPosition();
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }
            // 요구사항: 실제 위치가 0(또는 매우 근접) 이면 Safety 로 간주
            // 허용 오차는 장비 정밀도에 따라 조정(예: 0.005 이하)
            double zeroTolerance = AxisProbeZ.Config.InposTolerance; // 또는 0.005;
            if (Math.Abs(currentPos) <= zeroTolerance)
            {
                return true;
            }

            return IsAxisInTeachingPosition(AxisProbeZ,
                IndexChipProbeControllerRecipe.TeachingPositionName.SafetyZone.ToString(),
                AxisNames.ProbeZ);
        }

        public bool IsAxisProbeCardZSafetyPos()
        {
            // 현재 실제 위치 읽기
            double currentPos;
            try
            {
                currentPos = AxisProbeCardZ.GetPosition();
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }
            // 요구사항: 실제 위치가 0(또는 매우 근접) 이면 Safety 로 간주
            // 허용 오차는 장비 정밀도에 따라 조정(예: 0.005 이하)
            double zeroTolerance = AxisProbeCardZ.Config.InposTolerance; // 또는 0.005;
            if (Math.Abs(currentPos) <= zeroTolerance)
            {
                return true;
            }

            return IsAxisInTeachingPosition(AxisProbeCardZ,
                IndexChipProbeControllerRecipe.TeachingPositionName.SafetyZone.ToString(),
                AxisNames.ProbeCardZ);
        }

        public bool IsAxisSphereZSafetyPos()
        {
            // Up 우선
            if (IsAxisInTeachingPosition(AxisSphereZ,
                    IndexChipProbeControllerRecipe.TeachingPositionName.SphereZ_Ready.ToString(),
                    AxisNames.SphereZ))
            {
                return true;
            }

            // Up 이 없거나 InPosition 아니면 Ready 로 재확인
            return IsAxisInTeachingPosition(AxisSphereZ,
                    IndexChipProbeControllerRecipe.TeachingPositionName.SphereZ_Ready.ToString(),
                    AxisNames.SphereZ);
        }

        /// <summary>
        /// 세 축(ProbeZ, ProbeCardZ, SphereZ)이 모두 Safety 위치인가
        /// </summary>
        public bool IsAllSafetyAxisPos()
            => IsPositionProbeZSafety() &&
               IsAxisProbeCardZSafetyPos() &&
               IsAxisSphereZSafetyPos();

        public bool IsProbeSafetyAxisPos()
        {
            bool bRet1 = false, bRet2 = false;
            bRet1 = IsPositionProbeZSafety();
            bRet2 = IsAxisProbeCardZSafetyPos();

            return bRet1 && bRet2;
        }

        #endregion


        #region Camera Binding
        public HIKGigECamera ProbeCamera { get; private set; }
        public string ProbeCameraKey => "Index_Prober"; // 표준 Key 이름 사용
        private void BindCamera()
        {
            var eq = Equipment.Instance;
            if (eq == null)
                return;

            // Equipment에 정의된 카메라 Dictionary에서 Key로 조회하여 바인딩
            if (eq.Cameras != null && eq.Cameras.TryGetValue(ProbeCameraKey, out var cam))
                ProbeCamera = cam as HIKGigECamera;
            else
                ProbeCamera = eq.IndexProberCam;
        }

        // [ADD] 검사 결과 이미지 저장
        private void SaveResultImage(int index, string resultSuffix)
        {
            if (ProbeCamera == null || !ProbeCamera.Opened)
                return;

            try
            {
                // Grab 동기 호출
                if (ProbeCamera.GrabSync(out var image) == 0 && image != null)
                {
                    // 1. 저장 경로 설정 (D:\Log\Image\{UnitName}\Fail)
                    string saveFolder = $@"D:\LCP-280\Log\Image\{UnitName}\Fail";

                    // 2. 폴더 없으면 생성
                    if (!System.IO.Directory.Exists(saveFolder))
                    {
                        System.IO.Directory.CreateDirectory(saveFolder);
                    }

                    // 3. 파일명 생성 (년월일_시분초_밀리초)
                    string fileName = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + ".bmp";
                    string fullPath = System.IO.Path.Combine(saveFolder, fileName);

                    // 4. 저장 실행
                    if (image != null)
                    {
                        image.Save(fullPath, VisionImage.FileFilter.bmp);
                        Log.Write(UnitName, "AlignXY", $"Saved Fail Image: {fileName}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(UnitName, "SaveResultImage", $"Image save failed: {ex.Message}");
            }
        }
        #endregion

        #region Teaching Helpers
        public int MoveToTeachingPosition(string positionName, bool isFine = false)
        {
            if (string.IsNullOrWhiteSpace(positionName))
            {
                Log.Write(UnitName, nameof(MoveToTeachingPosition),
                        $"[TeachingMove] TeachingPositions에서 '{positionName}' 을 찾지 못했습니다.");
                return -1;
            }

            int result = 0;
            IndexChipProbeControllerRecipe.TeachingPositionName en;
            if (Enum.TryParse(positionName, out en))
            {
                int selIndex = FindTeachingSelectionIndex(positionName);
                if (selIndex >= 0)
                {
                    result = MoveToTeachingPositionBySelectionIndex(selIndex, isFine);
                }
                else
                {
                    // enum인데 index를 못 찾으면 실패 처리(원인 로그)
                    Log.Write(UnitName, nameof(MoveToTeachingPosition),
                        $"[TeachingMove] TeachingPositions에서 '{positionName}' index를 찾지 못했습니다.");
                    return -1;
                }
            }

            return result;
        }

        private int FindTeachingSelectionIndex(string positionName)
        {
            try
            {
                var list = GetTeachingList();
                if (list == null) return -1;

                for (int i = 0; i < list.Count; i++)
                {
                    var tp = list[i];
                    if (tp != null && string.Equals(tp.Name, positionName, StringComparison.OrdinalIgnoreCase))
                        return i;
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            return -1;
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
        public bool SetContectTop(bool on)
        {
            bool bRet = true;

            if (on)
            {
                // Top Contact
                WriteOutput(IndexChipProbeControllerConfig.IO.PROBECARD_CONTACT_VLV, false);
                Thread.Sleep(1); // 약간의 딜레이
                WriteOutput(IndexChipProbeControllerConfig.IO.BLADE_CONTACT_VLV, true);
            }
            else
            {
                // Probe Contact
                WriteOutput(IndexChipProbeControllerConfig.IO.BLADE_CONTACT_VLV, false);
                Thread.Sleep(1); // 약간의 딜레이
                WriteOutput(IndexChipProbeControllerConfig.IO.PROBECARD_CONTACT_VLV, true);
            }

            return bRet;
        }
        public bool IsContactTop()
        {
            return IsOutputOn(IndexChipProbeControllerConfig.IO.BLADE_CONTACT_VLV);
        }
        public bool IsContactProbe()
        {
            return IsOutputOn(IndexChipProbeControllerConfig.IO.PROBECARD_CONTACT_VLV);
        }
        public bool SetProbeVac(bool on)
        {
            if (_vacProbeCard == null) return false;
            if (on) _vacProbeCard.On();
            else _vacProbeCard.Off();
            return true;
        }
        public bool SetSphereFB(bool bFwdBwd)
        {
            if (_cylSphere == null)
                return false;

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
        public bool IsSphereForward()
        {
            var equipment = Equipment.Instance;
            bool IsDryRunEqp = equipment.EquipmentConfig.IsDryRun;
            if (Config.IsSimulation)// || Config.IsDryRun || IsDryRunEqp)
            {
                return true;
            }

            return ReadInput(NAME_SPHERE_FW);   // Forward sensor
        }
        public bool IsSphereBackward()
        {
            var equipment = Equipment.Instance;
            bool IsDryRunEqp = equipment.EquipmentConfig.IsDryRun;
            if (Config.IsSimulation)// || Config.IsDryRun || IsDryRunEqp)
            {
                return true;
            }

            return ReadInput(NAME_SPHERE_BW);   // Backward sensor
        }

        // === Direct Valve Control (강제 구동) ===
        public bool IsSphereFwdValveOn() => IsOutputOn(NAME_SPHERE_FW);
        public bool IsProbeVacValveOn() => IsOutputOn(NAME_PROBE_VAC);
        #endregion


        #region seq Signals

        public bool CompleteProbe { get; set; } = false;

        #endregion

        #region Lifecycle
        public override int OnRun()
        {
            int ret = 0;
            if (this.RunUnitStatus == UnitStatus.Stopped ||
               this.RunUnitStatus == UnitStatus.Stopping ||
               this.RunUnitStatus == UnitStatus.Error ||
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

            _contactCounter?.Save();

            this.RunUnitStatus = UnitStatus.Stopped;
            base.OnStop();
            return ret;
        }
        protected override int OnRunReady() { return 0; }
        protected override int OnRunWork() { return 0; }
        protected override int OnRunComplete() { return 0; }
        #endregion

        // [ADD] 마지막 측정 결과의 BinType을 die에서 읽는다.
        // 주: PopulateDieWithTesterResult(die)에서 die.TesterResult / MeasureValues / Rank 등을 채운다고 되어있으므로
        //     여기서는 die.TesterResult.BinningResult를 우선으로 본다.
        private bool TryGetDieBinType(MaterialDie die, out BinningType binType)
        {
            binType = BinningType.None;
            if (die == null)
                return false;

            try
            {
                // TesterResult는 public field로 존재함 (MaterialDie.cs)
                var tr = die.TesterResult;
                var bin = tr != null ? tr.BinningResult : null;
                if (bin == null)
                    return false;

                binType = bin.BinType;
                return true;
            }
            catch (Exception ex)
            {
                Log.Write(UnitName, nameof(TryGetDieBinType), ex.Message);
                return false;
            }
        }

        // [ADD] Simulation/DryRun에서 Good으로 나올 확률 (0~1)
        // 나머지는 NgBin으로 만들어 Retry 로직 테스트
        public double SimulationGoodProbability { get; set; } = 0.75;
        private static readonly object _simRandLock = new object();
        private static readonly Random _simRand = new Random();
        // [ADD] 측정 1회 + 결과 BinType 보고 NG면 재시도
        private int MeasureChipWithNgRetry(MaterialDie die, int maxRetry, int retryDelayMs = 10)
        {
            if (maxRetry < 0)
                maxRetry = 0;

            for (int attempt = 0; attempt <= maxRetry; attempt++)
            {
                int rc = IndexChipProber.MeasureChip();
                if (rc != 0)
                {
                    Log.Write(UnitName, nameof(MeasureChipWithNgRetry),
                        $"MeasureChip failed. attempt={attempt + 1}/{maxRetry + 1} rc={rc}");
                    return -1;
                }

                var equipment = Equipment.Instance;
                bool isDryRunEqp = equipment.EquipmentConfig.IsDryRun;

                if ((Config.IsSimulation || Config.IsDryRun || isDryRunEqp) && die != null)
                {
                    try
                    {
                        double p = SimulationGoodProbability;
                        if (p < 0.0) p = 0.0;
                        if (p > 1.0) p = 1.0;

                        bool isGood;
                        lock (_simRandLock)
                        {
                            isGood = _simRand.NextDouble() < p;
                        }

                        if (die.TesterResult != null && die.TesterResult.BinningResult != null)
                        {
                            die.TesterResult.BinningResult.BinType = isGood ? BinningType.GoodBin : BinningType.NgBin;
                            Log.Write(UnitName, nameof(MeasureChipWithNgRetry),
                                $"[SIM] Forced BinType={die.TesterResult.BinningResult.BinType} pGood={p:0.###} attempt={attempt + 1}/{maxRetry + 1}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Write(UnitName, nameof(MeasureChipWithNgRetry), $"[SIM] Failed to force BinType: {ex.Message}");
                    }
                }

                if (!TryGetDieBinType(die, out var binType))
                {
                    Log.Write(UnitName, nameof(MeasureChipWithNgRetry),
                        "Measure OK but BinType not available -> treat as success.");
                    if (die != null)
                        die.State = DieProcessState.Inspected;
                    return 0;
                }

                if (binType != BinningType.NgBin)
                {
                    if (die != null)
                        die.State = DieProcessState.Inspected;

                    Log.Write(UnitName, nameof(MeasureChipWithNgRetry),
                        $"Measure OK. BinType={binType} attempt={attempt + 1}/{maxRetry + 1}");
                    return 0;
                }

                // 여기부터 진짜 NG
                if (attempt < maxRetry)
                {
                    Log.Write(UnitName, nameof(MeasureChipWithNgRetry),
                        $"Measure NG -> retry. attempt={attempt + 1}/{maxRetry + 1}");

                    try
                    {
                        var ctx = Equipment.Instance.SummaryContext;
                        ctx.GetCurrentSummaryOrNull()?.AddContactRetry();
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                    }

                    Thread.Sleep(Math.Max(0, retryDelayMs));
                    continue;
                }

                Log.Write(UnitName, nameof(MeasureChipWithNgRetry),
                    $"Measure NG(final). attempt={attempt + 1}/{maxRetry + 1}");

                if (die != null)
                    die.SetReject("Error_Measure");

                return -1;
            }

            return -1;
        }

        //private int MeasureChipWithNgRetry(MaterialDie die, int maxRetry, int retryDelayMs = 10)
        //{
        //    int nRet = -1; //여기는 OK안되면 -1 로 나가야함.
        //    // maxRetry=2 => 총 3회 시도
        //    if (maxRetry < 0)
        //        maxRetry = 0;

        //    for (int attempt = 0; attempt <= maxRetry; attempt++)
        //    {
        //        int rc = IndexChipProber.MeasureChip();
        //        if (rc != 0)
        //        {
        //            Log.Write(UnitName, nameof(MeasureChipWithNgRetry),
        //                $"MeasureChip failed. attempt={attempt + 1}/{maxRetry + 1} rc={rc}");
        //            nRet = -1;  //return - 1;
        //        }

        //        // [ADD] Simulation/DryRun: BinType을 랜덤으로 강제 세팅해서 NG/Retry 테스트
        //        var equipment = Equipment.Instance;
        //        bool IsDryRunEqp = equipment.EquipmentConfig.IsDryRun;
        //        if ((Config.IsSimulation || Config.IsDryRun || IsDryRunEqp) && die != null)
        //        {
        //            if (true)
        //            {
        //                //시뮬 측정 택타임
        //                if (Config.IsSimulation)
        //                {
        //                    Thread.Sleep(150);
        //                }

        //                try
        //                {
        //                    // 확률 클램프
        //                    double p = SimulationGoodProbability;
        //                    if (p < 0.0) p = 0.0;
        //                    if (p > 1.0) p = 1.0;
        //                    bool isGood;
        //                    lock (_simRandLock)
        //                    {
        //                        isGood = _simRand.NextDouble() < p;
        //                    }

        //                    die.TesterResult.BinningResult.BinType = isGood ? BinningType.GoodBin : BinningType.NgBin;

        //                    Log.Write(UnitName, nameof(MeasureChipWithNgRetry),
        //                        $"[SIM] Forced BinType={die.TesterResult.BinningResult.BinType} pGood={p:0.###} attempt={attempt + 1}/{maxRetry + 1}");
        //                }
        //                catch (Exception ex)
        //                {
        //                    Log.Write(UnitName, nameof(MeasureChipWithNgRetry), $"[SIM] Failed to force BinType: {ex.Message}");
        //                }
        //            }
        //        }

        //        // BinType 확인
        //        if (TryGetDieBinType(die, out var binType))
        //        {
        //            if (binType != QMC.Common.PKGTester.BinningType.NgBin)
        //            {
        //                die.State = DieProcessState.Inspected;
        //                Log.Write(UnitName, nameof(MeasureChipWithNgRetry),
        //                    $"Measure OK. BinType={binType} attempt={attempt + 1}/{maxRetry + 1}");
        //                nRet = 0;  //return 0;
        //            }

        //            //여기에서 결과값을 확인 후에 검사 결과 및 Retry 처리
        //            if (true)
        //            {
        //                Log.Write(UnitName, "[MeasureChip] AddContactRetry");
        //                try
        //                {
        //                    var ctx = Equipment.Instance.SummaryContext;
        //                    ctx.GetCurrentSummaryOrNull()?.AddContactRetry();
        //                }
        //                catch (Exception ex)
        //                { Log.Write(ex); }
        //            }

        //            // NG면 재시도
        //            Log.Write(UnitName, nameof(MeasureChipWithNgRetry),
        //                $"Measure result NG -> retry. attempt={attempt + 1}/{maxRetry + 1}");

        //            if (attempt < maxRetry)
        //            {
        //                Thread.Sleep(Math.Max(0, retryDelayMs));
        //            }
        //            else
        //            {
        //                return nRet;
        //            }
        //        }
        //        else
        //        {
        //            // Bin 정보를 못 읽으면 기존 정책대로 "성공"으로 처리(현장 정책에 따라 NG 처리로 바꿀 수 있음)
        //            Log.Write(UnitName, nameof(MeasureChipWithNgRetry),
        //                $"Measure OK but BinType not available -> treat as success.");
        //            nRet = 0;
        //            return 0;
        //        }
        //    }

        //    // 여기까지 오면 "계속 NG"
        //    die.SetReject("Error_Measure");
        //    return nRet;
        //}


        private void LogSequence(string log)
        {
            if (this.CurrentFunc == null)
                return;

            //Log.Write(UnitName, this.CurrentFunc.Method.Name, $"[Sequence] {log}");
        }

        public bool IsTopRequired()
        {
            //Todo: Recipe Data로 사용해야함.
            var recipe = Equipment.Instance.EquipmentRecipe.CurrentRecipe;
            return recipe.ContectTop;
            //return Config.ContectTopMode;
        }
        protected override void OnMakeSequence()
        {
            base.OnMakeSequence();
            this.SequencePlayers.Add(RunInspectionReady);
            this.SequencePlayers.Add(RunInspection);
        }
        #region Seq 단위 동작 함수

        public int RunInspectionReady(bool bFineSpeed = false)
        {
            int nRet = -1;

            while (IsRotaryReadyStable(3, 5) == false)
            {
                if (IsStop)
                {
                    return 0;
                }
                Thread.Sleep(1);
            }

            // 하부 Z-Axis.
            nRet = MovePositionSafetyZ();
            if (nRet != 0)
            {
                Log.Write(UnitName, "RunInspectionReady", "[RunInspectionReady] MovePositionSafetyZ failed");
                return -1;
            }

            if (Config.ViewMode == false)
            {
                if (IsSphereForward() == false)
                {
                    {
                        SetSphereFB(true);

                        Thread.Sleep(500);
                        var sw = Stopwatch.StartNew();
                        while (true)
                        {
                            if (IsSphereForward())
                            {
                                break;
                            }

                            if (sw.ElapsedMilliseconds > 5000)
                            {
                                PostAlarm((int)AlarmKeys.eSphereFBTimeout);
                                Log.Write(UnitName, "RunInspectionReady", "[RunInspectionReady] SphereFB-F Timeout");
                                return -1;
                            }
                            Thread.Sleep(5);
                        }
                    }
                }
                // 적분구 공정 위치.
                if (IsSphereZAtDown() == false)
                {
                    nRet = MovePositionSphereZDown();
                    if (nRet != 0)
                    {
                        Log.Write(UnitName, "RunInspectionReady", "[RunInspectionReady] MovePositionSphereZDown failed");
                        return -1;
                    }

                    var sw = Stopwatch.StartNew();
                    while (IsSphereZAtDown() == false)
                    {
                        if (IsStop)
                            return 0;

                        if (sw.ElapsedMilliseconds > 5000)
                        {
                            PostAlarm((int)AlarmKeys.eSphereFBTimeout);
                            Log.Write(UnitName, "RunInspectionReady", "[RunInspectionReady] SphereZ Down Timeout");
                            return -1;
                        }

                        Thread.Sleep(5);
                    }
                }
            }
            else
            {
                // 적분구 공정 위치.
                if (IsSphereZAtReady() == false)
                {
                    nRet = MovePositionSphereZReady();
                    if (nRet != 0)
                    {
                        Log.Write(UnitName, "RunInspectionReady", "[RunInspectionReady] MovePositionSphereZReady failed");
                        return -1;
                    }
                }
                if (IsSphereBackward() == false)
                {
                    //if (SetSphereFB(false))
                    {
                        SetSphereFB(false);

                        Thread.Sleep(500);
                        var sw = Stopwatch.StartNew();
                        while (true)
                        {
                            if (IsSphereBackward())
                            {
                                break;
                            }

                            if (sw.ElapsedMilliseconds > 5000)
                            {
                                PostAlarm((int)AlarmKeys.eSphereFBTimeout);
                                Log.Write(UnitName, "RunInspectionReady", "[RunInspectionReady] SphereFB-F Timeout");
                                return -1;
                            }
                            Thread.Sleep(5);
                        }
                    }
                }
            }


            return nRet;
        }

        /// <summary>
        /// BottomContactOnce OR TopContact
        ///  - 정책:
        ///    1) 먼저 BottomContactOnce 실행
        ///    2) 성공(0) 이면 TopContact 생략
        ///    3) 실패(음수) 시 TopContact 시도
        ///    4) 둘 다 실패하면 마지막 실패 코드 반환
        ///  - 필요 시 모드(Top/Bottom/Auto) 확장 가능
        /// </summary>
        public int RunInspection(bool bFineSpeed = false)
        {
            int nRet = 0;
            this.CurrentFunc = RunInspection;

            try
            {
                int nIndex = this.GetProbeIndexNo();
                MaterialDie die = this.Rotary.GetProbeSocketMaterial();
                bool bUseSocket = this.Rotary.Config.GetUseSocket(nIndex);
                if (bUseSocket == false)
                {
                    Log.Write(UnitName, "[RunInspection] Socket not used. Skip inspection.");
                    return 0;
                }
                // [ADD] 이미 Probed 상태면 재검사 생략
                var probeSocket = this.Rotary.GetSocket(nIndex);
                //if (probeSocket != null) && probeSocket.State == Rotary.RotarySocketState.Probed)
                //{
                //    Log.Write(UnitName, $"[RunInspection] Socket#{nIndex + 1} already Probed. Skip inspection.");
                //    return 0;
                //}

                if (die == null)
                {
                    Log.Write(UnitName, "RunInspection", "No die in probe socket");
                    return 0;
                }
                if (die.Presence != Material.MaterialPresence.Exist)
                {
                    Log.Write(UnitName, "RunInspection", "Die presence is not Exist");
                    return 0;
                }
                if (die.State == DieProcessState.Rejected)
                {
                    Log.Write(UnitName, "RunInspection", "DieProcessState.Rejected");
                    return 0;
                }
                if (die.State == DieProcessState.Skip)
                {
                    Log.Write(UnitName, "RunInspection", "DieProcessState.Skip");
                    return 0;
                }
                if (die.ProcessSatate == Material.MaterialProcessSatate.Skipped)
                {
                    Log.Write(UnitName, "RunInspection", "MaterialProcessSatate.Skipped");
                    return 0;
                }

                // 검사 시작 전에 Rotary가 완전히 멈췄는지 확인 (안 멈췄으면 대기)
                while (IsRotaryReadyStable(3, 5) == false)
                {
                    if (IsStop)
                    {
                        return 0;
                    }
                    Log.Write(UnitName, "RunInspection", "[RunInspection] IsRotaryIdle");
                    Thread.Sleep(1);
                }

                LogSequence("Start");
                TaktStart("One Cycle");
                //카메라로 컨택 확인 하고 싶을때 모드
                if (Config.ViewMode == false)
                {
                    if (IsSphereForward() == false)
                    {
                        //if (SetSphereFB(true))
                        {
                            SetSphereFB(true);
                            Thread.Sleep(500);
                            var sw = Stopwatch.StartNew();
                            while (IsSphereForward() == false)
                            {
                                if (sw.ElapsedMilliseconds > 5000)
                                {
                                    PostAlarm((int)AlarmKeys.eSphereFBTimeout);
                                    Log.Write(UnitName, "RunInspection", "SphereFB-F Timeout");
                                    return -1;
                                }
                                Thread.Sleep(1);
                            }
                        }
                    }

                    if (IsSphereZAtDown() == false)
                    {
                        nRet = MovePositionSphereZDown();
                        if (nRet != 0)
                        {
                            Log.Write(UnitName, "RunInspection", "MovePositionSphereZDown failed");
                            return -1;
                        }

                        var sw = Stopwatch.StartNew();
                        while (IsSphereZAtDown() == false)
                        {
                            if (sw.ElapsedMilliseconds > 5000)
                            {
                                PostAlarm((int)AlarmKeys.eSphereMoveDownTimeout);
                                Log.Write(UnitName, "RunInspection", "MovePositionSphereZDown Timeout");
                                return -1;
                            }
                            Thread.Sleep(1);
                        }
                    }
                }
                else
                {
                    if (IsSphereZAtReady() == false)
                    {
                        nRet = MovePositionSphereZReady();
                        if (nRet != 0)
                        {
                            Log.Write(UnitName, "RunInspection", "MovePositionSphereZDown failed");
                            return -1;
                        }

                        var sw = Stopwatch.StartNew();
                        while (IsSphereZAtReady() == false)
                        {
                            if (sw.ElapsedMilliseconds > 5000)
                            {
                                PostAlarm((int)AlarmKeys.eSphereMoveUpTimeout);
                                Log.Write(UnitName, "RunInspection", "MovePositionSphereZReady Timeout");
                                return -1;
                            }
                            Thread.Sleep(1);
                        }
                    }

                    if (IsSphereBackward() == false)
                    {
                        //if (SetSphereFB(false))
                        {
                            SetSphereFB(false);
                            Thread.Sleep(500);
                            var sw = Stopwatch.StartNew();
                            while (IsSphereBackward() == false)
                            {
                                if (sw.ElapsedMilliseconds > 5000)
                                {
                                    PostAlarm((int)AlarmKeys.eSphereFBTimeout);
                                    Log.Write(UnitName, "TopContactAndMeasureOnce", "[TopContactOnce] SphereFB-F Timeout");
                                    return -1;
                                }
                                Thread.Sleep(1);
                            }
                        }
                    }
                }

                probeSocket.SetState(Rotary.RotarySocketState.Probing);
                if (IsTopRequired())
                {
                    if (IsContactTop() == false)
                    {
                        if (SetContectTop(true) == false)
                        {
                            Log.Write(UnitName, "[RunInspection] SetContectTop(Top) failed");
                            return -1;
                        }
                    }

                    nRet = TopContactAndMeasureOnce(bFineSpeed);
                    if (nRet != 0)
                    {
                        Log.Write(UnitName, "[RunInspection] TopContactAndMeasureOnce failed");
                        nRet = -1;  //return -1;
                    }
                }
                else
                {
                    if (IsContactProbe() == false)
                    {
                        if (SetContectTop(false) == false)
                        {
                            Log.Write(UnitName, "[RunInspection] SetContectTop(Bottom) failed");
                            return -1;
                        }
                    }
                    nRet = BottomContactAndMeasureOnce(bFineSpeed);
                    if (nRet != 0)
                    {
                        Log.Write(UnitName, "[RunInspection] BottomContactAndMeasureOnce failed");
                        nRet = -1;  //return -1;
                    }
                }

                if (nRet == 0)
                {
                    die.ProcessSatate = Material.MaterialProcessSatate.Processing;
                    die.Presence = Material.MaterialPresence.Exist;
                }
                else
                {
                    Log.Write(UnitName, "[RunInspection] AddContactAsMiss");
                    try
                    {
                        var ctx = Equipment.Instance.SummaryContext;
                        ctx.GetCurrentSummaryOrNull()?.AddContactAsMiss();
                    }
                    catch (Exception ex)
                    { Log.Write(ex); }

                    die.SetReject("Error_Probe");
                    die.ProcessSatate = Material.MaterialProcessSatate.Processing; //Skip? Unloader는 해야지?
                    die.Presence = Material.MaterialPresence.Exist;
                }

                nRet = MovePositionSafetyZ();
                if (nRet != 0)
                {
                    Log.Write(UnitName, "[TopContactOnce] MovePositionSafetyZ failed");
                    nRet = -1;
                }
                while (this.IsProbeSafetyAxisPos() == false)
                {
                    if (IsStop)
                    {
                        return 0;
                    }
                    Thread.Sleep(1);
                }

                if (IsTopRequired() == false)
                    SetProbeVac(false);

                probeSocket.SetState(Rotary.RotarySocketState.Probed);
                LogSequence("End");
                return nRet;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            finally
            {
                if (IsTopRequired() == false)
                    SetProbeVac(false);

                if (this.IsProbeSafetyAxisPos() == false)
                {
                    nRet = MovePositionSafetyZ();
                    if (nRet != 0)
                    {
                        Log.Write(UnitName, "RunInspection", "MovePositionSafetyZ failed");
                        nRet = -1;
                    }
                }

                TaktEnd("One Cycle");
            }
            return nRet;
        }

        public int RunInspection(int nSocketIndex, bool bFineSpeed = false)
        {
            int nRet = 0;
            this.CurrentFunc = RunInspection;

            try
            {
                int nIndex = nSocketIndex;
                MaterialDie die = this.Rotary.GetProbeSocketMaterial();
                bool bUseSocket = this.Rotary.Config.GetUseSocket(nIndex);
                if (bUseSocket == false)
                {
                    Log.Write(UnitName, "[RunInspection] Socket not used. Skip inspection.");
                    return 0;
                }
                // [ADD] 이미 Probed 상태면 재검사 생략
                var probeSocket = this.Rotary.GetSocket(nIndex);
                if (probeSocket != null && probeSocket.State == Rotary.RotarySocketState.Probed)
                {
                    Log.Write(UnitName, $"[RunInspection] Socket#{nIndex + 1} already Probed. Skip inspection.");
                    return 0;
                }

                if (die == null)
                {
                    Log.Write(UnitName, "RunInspection", "No die in probe socket");
                    return 0;
                }
                if (die.Presence != Material.MaterialPresence.Exist)
                {
                    Log.Write(UnitName, "RunInspection", "Die presence is not Exist");
                    return 0;
                }
                if (die.State == DieProcessState.Rejected)
                {
                    Log.Write(UnitName, "RunInspection", "DieProcessState.Rejected");
                    return 0;
                }
                if (die.State == DieProcessState.Skip)
                {
                    Log.Write(UnitName, "RunInspection", "DieProcessState.Skip");
                    return 0;
                }
                if (die.ProcessSatate == Material.MaterialProcessSatate.Skipped)
                {
                    Log.Write(UnitName, "RunInspection", "MaterialProcessSatate.Skipped");
                    return 0;
                }

                // 검사 시작 전에 Rotary가 완전히 멈췄는지 확인 (안 멈췄으면 대기)
                while (IsRotaryReadyStable(3, 5) == false)
                {
                    if (IsStop)
                    {
                        return 0;
                    }
                    Log.Write(UnitName, "RunInspection", "[RunInspection] IsRotaryIdle");
                    Thread.Sleep(1);
                }

                if (this.RunUnitStatus == UnitStatus.ManualRunning)
                {
                    nRet = Rotary.MoveToSocket(
                            nIndex,
                            Rotary.IndexReference.Probe,
                            CancellationToken.None,
                            maxStep: 16,
                            settleMs: 50);

                    if (nRet != 0)
                    {
                        PostAlarm((int)AlarmKeys.eIndexProbeControllerRotateMoveToSocket);
                        Log.Write(UnitName, nameof(RunInspection),
                            $"Rotary.MoveToSocket failed. targetSocket={nSocketIndex}");
                        return -1;
                    }

                    //if(!IsRotaryReadyStable(3,5))
                    //if (Rotary.IsLoadSocketEmpty() == false || Rotary.IsIndexReadyForAction(out string reason))
                    if (Rotary.IsLoadSocketEmpty() == false || IsRotaryReadyStable(3, 5))
                    {
                        Log.Write(UnitName, nameof(RunInspection),
                            $"Rotary.MoveToSocket failed. targetSocket={nSocketIndex}");
                        return 0;
                    }
                }

                LogSequence("Start");
                TaktStart("One Cycle");
                //카메라로 컨택 확인 하고 싶을때 모드
                if (Config.ViewMode == false)
                {
                    if (IsSphereForward() == false)
                    {
                        //if (SetSphereFB(true))
                        {
                            SetSphereFB(true);
                            Thread.Sleep(500);
                            var sw = Stopwatch.StartNew();
                            while (IsSphereForward() == false)
                            {
                                if (sw.ElapsedMilliseconds > 5000)
                                {
                                    PostAlarm((int)AlarmKeys.eSphereFBTimeout);
                                    Log.Write(UnitName, "RunInspection", "SphereFB-F Timeout");
                                    return -1;
                                }
                                Thread.Sleep(1);
                            }
                        }
                    }

                    if (IsSphereZAtDown() == false)
                    {
                        nRet = MovePositionSphereZDown();
                        if (nRet != 0)
                        {
                            Log.Write(UnitName, "RunInspection", "MovePositionSphereZDown failed");
                            return -1;
                        }

                        var sw = Stopwatch.StartNew();
                        while (IsSphereZAtDown() == false)
                        {
                            if (sw.ElapsedMilliseconds > 5000)
                            {
                                PostAlarm((int)AlarmKeys.eSphereMoveDownTimeout);
                                Log.Write(UnitName, "RunInspection", "MovePositionSphereZDown Timeout");
                                return -1;
                            }
                            Thread.Sleep(1);
                        }
                    }
                }
                else
                {
                    if (IsSphereZAtReady() == false)
                    {
                        nRet = MovePositionSphereZReady();
                        if (nRet != 0)
                        {
                            Log.Write(UnitName, "RunInspection", "MovePositionSphereZDown failed");
                            return -1;
                        }

                        var sw = Stopwatch.StartNew();
                        while (IsSphereZAtReady() == false)
                        {
                            if (sw.ElapsedMilliseconds > 5000)
                            {
                                PostAlarm((int)AlarmKeys.eSphereMoveUpTimeout);
                                Log.Write(UnitName, "RunInspection", "MovePositionSphereZReady Timeout");
                                return -1;
                            }
                            Thread.Sleep(1);
                        }
                    }

                    if (IsSphereBackward() == false)
                    {
                        //if (SetSphereFB(false))
                        {
                            SetSphereFB(false);
                            Thread.Sleep(500);
                            var sw = Stopwatch.StartNew();
                            while (IsSphereBackward() == false)
                            {
                                if (sw.ElapsedMilliseconds > 5000)
                                {
                                    PostAlarm((int)AlarmKeys.eSphereFBTimeout);
                                    Log.Write(UnitName, "TopContactAndMeasureOnce", "[TopContactOnce] SphereFB-F Timeout");
                                    return -1;
                                }
                                Thread.Sleep(1);
                            }
                        }
                    }
                }

                probeSocket.SetState(Rotary.RotarySocketState.Probing);
                if (IsTopRequired())
                {
                    if (IsContactTop() == false)
                    {
                        if (SetContectTop(true) == false)
                        {
                            Log.Write(UnitName, "[RunInspection] SetContectTop(Top) failed");
                            return -1;
                        }
                    }

                    nRet = TopContactAndMeasureOnce(bFineSpeed);
                    if (nRet != 0)
                    {
                        Log.Write(UnitName, "[RunInspection] TopContactAndMeasureOnce failed");
                        nRet = -1;  //return -1;
                    }
                }
                else
                {
                    if (IsContactProbe() == false)
                    {
                        if (SetContectTop(false) == false)
                        {
                            Log.Write(UnitName, "[RunInspection] SetContectTop(Bottom) failed");
                            return -1;
                        }
                    }
                    nRet = BottomContactAndMeasureOnce(bFineSpeed);
                    if (nRet != 0)
                    {
                        Log.Write(UnitName, "[RunInspection] BottomContactAndMeasureOnce failed");
                        nRet = -1;  //return -1;
                    }
                }

                if (nRet == 0)
                {
                    die.ProcessSatate = Material.MaterialProcessSatate.Processing;
                    die.Presence = Material.MaterialPresence.Exist;
                }
                else
                {
                    Log.Write(UnitName, "[RunInspection] AddContactAsMiss");
                    try
                    {
                        var ctx = Equipment.Instance.SummaryContext;
                        ctx.GetCurrentSummaryOrNull()?.AddContactAsMiss();
                    }
                    catch (Exception ex)
                    { Log.Write(ex); }

                    die.SetReject("Error_Probe");
                    die.ProcessSatate = Material.MaterialProcessSatate.Processing; //Skip? Unloader는 해야지?
                    die.Presence = Material.MaterialPresence.Exist;
                }

                nRet = MovePositionSafetyZ();
                if (nRet != 0)
                {
                    Log.Write(UnitName, "[TopContactOnce] MovePositionSafetyZ failed");
                    nRet = -1;
                }
                while (this.IsProbeSafetyAxisPos() == false)
                {
                    if (IsStop)
                    {
                        return 0;
                    }
                    Thread.Sleep(1);
                }

                if (IsTopRequired() == false)
                    SetProbeVac(false);

                probeSocket.SetState(Rotary.RotarySocketState.Probed);
                LogSequence("End");
                return nRet;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            finally
            {
                if (IsTopRequired() == false)
                    SetProbeVac(false);

                if (this.IsProbeSafetyAxisPos() == false)
                {
                    nRet = MovePositionSafetyZ();
                    if (nRet != 0)
                    {
                        Log.Write(UnitName, "RunInspection", "MovePositionSafetyZ failed");
                        nRet = -1;
                    }
                }

                TaktEnd("One Cycle");
            }
            return nRet;
        }

        public int TopContactAndMeasureOnce(bool bFineSpeed = false)
        {
            int nRet = 0;
            try
            {
                this.CurrentFunc = TopContactAndMeasureOnce;
                while (IsRotaryReadyStable(3, 5) == false)
                {
                    if (IsStop)
                    {
                        return 0;
                    }
                    Thread.Sleep(2);
                }

                LogSequence("Start");
                int nIndex = GetProbeIndexNo();
                int nIndexNow = GetProbeIndexNo();
                if (nIndexNow != nIndex)
                {
                    Log.Write(UnitName, nameof(TopContactAndMeasureOnce),
                        $"ProbeIndex changed before move. old={nIndex}, now={nIndexNow}");
                    nIndex = nIndexNow;
                }

                nRet = MovePositionTopContact_Index_Up(nIndex, bFineSpeed);
                if (nRet != 0)
                {
                    Log.Write(UnitName, nameof(TopContactAndMeasureOnce),
                        $"[TopContactOnce] OnMovePositionTopContact_Index_Up failed. idx={nIndex}, dbg={Rotary?.GetIndexMovingDebugText()}");
                    nRet = -1;
                }

                WaitByTime(Config.UpperWaitTime);

                var die = Rotary != null ? Rotary.GetProbeSocketMaterial() : null;
                nRet = MeasureChipWithNgRetry(die, maxRetry: 0, retryDelayMs: 50);
                if (nRet != 0)
                {
                    Log.Write(UnitName, "[TopContactOnce] MeasureChipWithNgRetry failed (or NG after retries)");
                    nRet = -1;
                }

                if (nRet == 0)
                {
                    long c = _contactCounter.Increase();
                    Log.Write(UnitName, nameof(TopContactAndMeasureOnce), $"ContactCount++ => {c}");
                }

                if (Config.ViewMode)
                {
                    SaveResultImage(nIndex, nRet == 0 ? "Good" : "NG");
                }

                return nRet;
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
        public int BottomContactAndMeasureOnce(bool bFineSpeed = false)
        {
            this.CurrentFunc = BottomContactAndMeasureOnce;
            int nRet = 0;
            int nIndex = GetProbeIndexNo();
            try
            {
                while (IsRotaryReadyStable(3, 5) == false)
                {
                    if (IsStop)
                        return 0;

                    Thread.Sleep(1);
                }

                LogSequence("Start");
                SetProbeVac(true);
                nRet = MovePositionGripperXReady(bFineSpeed);
                if (nRet != 0)
                {
                    Log.Write(UnitName, "[BottomContactOnce] MovePositionGripperXReady failed");
                    return -1;
                }

                if (true)
                {
                    // 동기 방식
                    TaktStart("SyncProbeZUpAndBottomProbeZReady");
                    nRet = SyncProbeZGripperUpAndBottomReady(nIndex, bFineSpeed);
                    if (nRet != 0)
                    {
                        Log.Write(UnitName, "[BottomContactOnce] SyncProbeZGripperUpAndBottomReady failed");
                        return -1;
                    }
                    TaktEnd("SyncProbeZUpAndBottomProbeZReady");
                }
                
                // 병렬 방식
                {
                    Task<int> taskClamp = null;
                    if (Config.GripperMode)
                    {
                        TaktStart("GripperXClamp");

                        // 목표 위치와 현재 시작 위치 계산
                        string tpClampName = IndexChipProbeControllerRecipe.TeachingPositionName.GripperX_Clamp.ToString();
                        double targetX = GetTP(tpClampName, AxisNames.GripperX);
                        double startX = AxisGripperX.GetPosition();

                        // 안전 기준: 전체 이동 거리의 50% 이상 이동했을 때 (비율 대신 상수로 넣으려면 5.0 등 mm 단위 사용)
                        double ClampXMoveDist = Config.ClampXMoveDist;
                        ClampXMoveDist = ClampXMoveDist / 100.0; // Config가 % 단위로 들어온다고 가정
                        if (ClampXMoveDist <= 0.0)
                        {
                            ClampXMoveDist = 1.0;
                        }
                        double safeMoveDist = Math.Abs(targetX - startX) * ClampXMoveDist;
                        //double safeMoveDist = Math.Abs(targetX - startX) * 0.3;

                        // 1. Clamp 비동기 이동 시작
                        taskClamp = MovePositionAsyncGripperXClamp(bFineSpeed);

                        // 2. 이동 거리를 실시간으로 감시
                        while (IsEndTask(taskClamp) == false)
                        {
                            double currentX = AxisGripperX.GetPosition();
                            double movedDist = Math.Abs(currentX - startX);

                            // 지정된 안전 거리 이상 이동했거나, 이미 인포지션(목표 도달)인 경우 루프 탈출
                            if (movedDist >= safeMoveDist || AxisGripperX.InPosition(targetX))
                            {
                                break;
                            }

                            // 이동 중 인터락 감시
                            if (IsMoveInterLockGripperXClamp() != 0)
                            {
                                break;
                            }
                            Thread.Sleep(5);
                        }
                    }

                    if (IsPositionProbeZGripperIndexUp(nIndex) == false)
                    {
                        Log.Write(UnitName, "[BottomContactOnce] IsPositionProbeZGripperIndexUp failed");
                        return -1;
                    }

                    TaktStart("BottomProbeZUp");
                    // 3. ProbeCardZ축 상승 시작 (GripperX가 충분히 이동한 뒤에 병렬 실행)
                    nRet = MovePositionBottomContact_Index_Up(nIndex, bFineSpeed);
                    if (nRet != 0)
                    {
                        Log.Write(UnitName, "[BottomContactOnce] MovePositionBottomContact_Index_Up failed");
                        return -1;
                    }
                    TaktEnd("BottomProbeZUp");

                    // 4. Clamp 작업 완료 대기
                    if (taskClamp != null)
                    {
                        while (IsEndTask(taskClamp) == false)
                        {
                            IsMoveInterLockGripperXClamp();
                            Thread.Sleep(3);
                        }
                        if (taskClamp.Result != 0)
                        {
                            Log.Write(UnitName, "[BottomContactOnce] MovePositionGripperXClamp failed during async wait");
                            return -1;
                        }
                        TaktEnd("GripperXClamp");
                    }
                }

                TaktStart("UpperWaitTime");
                WaitByTime(Config.UpperWaitTime);
                TaktEnd("UpperWaitTime");
                // 6) 검사 요구 동기 처리.

                TaktStart("Measure");
                // [CHG] NG면 Retry
                var die = Rotary != null ? Rotary.GetProbeSocketMaterial() : null;
                //nRet = MeasureChipWithNgRetry(die, maxRetry: 1, retryDelayMs: 50);
                nRet = MeasureChipWithNgRetry(die, maxRetry: 0, retryDelayMs: 50);
                if (nRet != 0)
                {
                    if (Config.ViewMode)
                    {
                        // [ADD] 검사 완료 후 결과 이미지 저장
                        SaveResultImage(nIndex, nRet == 0 ? "Good" : "NG");
                    }
                    Log.Write(UnitName, "[BottomContactOnce] MeasureChipWithNgRetry failed (or NG after retries)");
                    TaktEnd("Measure");
                }
                else
                {
                    TaktEnd("Measure");
                }

                long c = _contactCounter.Increase();
                Log.Write(UnitName, nameof(BottomContactAndMeasureOnce), $"ContactCount++ => {c}");
                if (Config.ViewMode)
                {
                    // [ADD] 검사 완료 후 결과 이미지 저장
                    SaveResultImage(nIndex, nRet == 0 ? "Good" : "NG");
                }

                return MoveToSafetyZSync(ref nRet);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return -1;
            }
            finally
            {
                SetProbeVac(false);
                if (this.IsProbeSafetyAxisPos() == false)
                {
                    //병렬 순차 제어를 여기에다가 넣을까.. 그리고 택타임 측정?
                    nRet = MovePositionSafetyZ();
                    if (nRet != 0)
                    {
                        Log.Write(UnitName, "BottomContactOnce", "MovePositionSafetyZ failed");
                        nRet = -1;
                    }
                }
                LogSequence("End");
            }
        }

        public void ResetContactCount()
        {
            _contactCounter?.Reset(0);
            Log.Write(UnitName, nameof(ResetContactCount), "Contact count reset to 0");
        }

        private int MoveToSafetyZSync(ref int nRet)
        {
            if (true) // 병렬 방식
            {
                // 여기에 병렬 Safety 위치로 보내자.
                try
                {
                    string tpName = $"SafetyZone";
                    var tpObj = GetTeachingPosition(tpName);
                    if (tpObj == null)
                    {
                        Log.Write(UnitName, $"[OnMovePosition_SafetyZone] Teaching not found: {tpName}");
                        return -1;
                    }

                    double dZPos = 0.0;
                    Task<int> taskReady = null;
                    bool isGripperXStarted = false;

                    TaktStart("ProbeCardZSafety");
                    // 1. ProbeCardZ축 안전위치(하강) 목표 위치 및 시작 위치 획득
                    dZPos = GetTP(tpName, AxisNames.ProbeCardZ);
                    double startZ = AxisProbeCardZ.GetPosition();

                    // 안전 기준: 전체 Z축 이동 거리의 50% (필요시 0.5 값을 변경하여 시작 시점을 조절하세요)
                    double safeMoveDist = Math.Abs(dZPos - startZ) * 0.5;

                    // 2. ProbeCardZ축 하강 비동기 시작
                    Task<int> taskProbeCardZ = Task.Run(() =>
                    {
                        return OnMoveAxisPositionOne(AxisProbeCardZ, dZPos);
                    });

                    // 3. ProbeCardZ축 이동 거리를 실시간으로 감시하며 GripperX 이동 시작 시점 판단
                    while (IsEndTask(taskProbeCardZ) == false)
                    {
                        if (!isGripperXStarted)
                        {
                            double currentZ = AxisProbeCardZ.GetPosition();
                            double movedDist = Math.Abs(currentZ - startZ);

                            // 지정된 안전 깊이 이상 하강했거나, 이미 인포지션인 경우 Gripper 동작 시작
                            if (movedDist >= safeMoveDist || AxisProbeCardZ.InPosition(dZPos))
                            {
                                TaktStart("GripperXReady");
                                taskReady = MovePositionAsyncGripperXReady();
                                isGripperXStarted = true;
                            }
                        }

                        // 이동 중 인터락 지속 감시 (위반 시 탈출)
                        if (IsMoveInterLockSafetyZ() != 0)
                        {
                            break;
                        }
                        Thread.Sleep(3);
                    }

                    // 4. ProbeCardZ 이동 완전 종료 대기 및 결과 체크
                    if (taskProbeCardZ.Result != 0)
                    {
                        Log.Write(UnitName, $"[OnMovePositionSafetyZ] ProbeCardZ move failed tp={tpName} posZ={dZPos}");
                        return -1;
                    }
                    TaktEnd("ProbeCardZSafety");

                    // (방어 코드) 만약 Z축 거리가 너무 짧아 루프가 빨리 끝났다면 직접 GripperX 시작
                    if (!isGripperXStarted)
                    {
                        TaktStart("GripperXReady");
                        taskReady = MovePositionAsyncGripperXReady();
                    }

                    // 5. GripperX Ready 동작이 완전히 끝날 때까지 대기
                    if (taskReady != null)
                    {
                        while (IsEndTask(taskReady) == false)
                        {
                            if (IsMoveInterLockGripperXReady() != 0) break;
                            Thread.Sleep(1);
                        }
                        if (taskReady.Result != 0)
                        {
                            Log.Write(UnitName, $"[OnMovePositionSafetyZ] move failed: MovePositionGripperXReady async");
                            return -1;
                        }
                    }
                    TaktEnd("GripperXReady");

                    TaktStart("ProbeZSafety");
                    // 6. 모든 위치가 안전해지면 ProbeZ 안전위치로 이동
                    if (IsPositionProbeCardZSafety() == false)
                    {
                        Log.Write(UnitName, $"[OnMovePositionSafetyZ] ProbeCardZ move failed - IsPositionProbeCardZSafety");
                        return -1;
                    }

                    dZPos = GetTP(tpName, AxisNames.ProbeZ);
                    nRet = OnMoveAxisPositionOne(AxisProbeZ, dZPos);
                    if (nRet != 0)
                    {
                        Log.Write(UnitName, $"[OnMovePositionSafetyZ] ProbeZ move failed tp={tpName} posZ={dZPos}");
                        return -1;
                    }
                    TaktEnd("ProbeZSafety");

                    return nRet;
                }
                finally
                {
                    _isSafetyMoving = false;
                }
            }
        }

        public int GetProbeIndexNo()
        {
            if (Rotary == null)
                return 0;

            int loadIndex = Rotary.GetLoadIndexNo();

            // 반시계 방향으로 2칸 이동
            int probeIndex = (loadIndex - this.Config.IndexOfProbe + Rotary.GetIndexCount()) % Rotary.GetIndexCount();

            return probeIndex;

        }

        #endregion


        #region Ready
        public int EnsureReady(bool isFine = false)
        {
            Task<int> task = EnsureReadyAsync(isFine);
            while (IsEndTask(task) == false)
            {
                Thread.Sleep(1);
            }
            return task.Result;
        }
        private Task<int> EnsureReadyAsync(bool isFine = false)
        {
            return Task.Run(() =>
            {
                OnEnsureReady(isFine);
                return 0;
            });
        }
        private int OnEnsureReady(bool isFine)
        {
            int nRet = 0;

            if (IsPositionProbeZSafety() == false
               || IsPositionProbeCardZSafety() == false)
            {
                nRet = MovePositionSafetyZ();
                if (nRet != 0)
                {
                    Log.Write(UnitName, "OnEnsureReady", "CheckReady Fail - MovePositionSafetyZ");
                    return nRet;
                }
            }

            return nRet;
        }
        #endregion


        public override int MoveTeachingPositionOnce(int selIndex, bool isFine)
        {
            int waitErrors = 0;
            string teachName = string.Empty;
            bool bSuccssed = Config.GetTeachingPositionName(selIndex, out teachName);
            if (!bSuccssed)
            {
                Log.Write(UnitName, nameof(MoveTeachingPositionOnce),
                    $"[TEACH 이동 오류] 인덱스 '{selIndex}' 티칭포지션 이름을 찾을 수 없습니다.");
                return -1;
            }

            var list = GetTeachingList();
            var tp = list.FirstOrDefault(t => t != null && string.Equals(t.Name, teachName, StringComparison.OrdinalIgnoreCase));
            if (tp == null)
            {
                Log.Write(UnitName, nameof(MoveTeachingPositionOnce),
                    $"[TEACH 이동 오류] TeachingPosition을 찾을 수 없습니다. name='{teachName}'");
                return -1;
            }

            var axisPos = GetAxisPositions(tp);
            if (axisPos == null)
                return -1;

            var axisObj = GetAxisObjects(tp);
            foreach (var kv in axisPos)
            {
                string key = kv.Key;
                double target = kv.Value;

                MotionAxis axis = null;
                if (axisObj != null && axisObj.TryGetValue(key, out axis)) { }
                if (axis == null && Axes.TryGetValue(key, out var direct))
                    axis = direct;

                if (axis == null)
                {
                    foreach (var ap in Axes)
                    {
                        if (ap.Value != null &&
                            (ap.Key.Equals(key, StringComparison.OrdinalIgnoreCase) ||
                             ap.Value.Name.Equals(key, StringComparison.OrdinalIgnoreCase)))
                        {
                            axis = ap.Value;
                            break;
                        }
                    }
                }

                if (axis == null)
                    continue;

                bool IsAuto = false;
                if (RunMode == UnitRunMode.Auto ||
                    RunUnitStatus == UnitStatus.AutoRunning ||
                    RunUnitStatus == UnitStatus.ManualRunning)
                {
                    IsAuto = true;
                }
                else
                {
                    IsAuto = false;
                }
                waitErrors = axis.MoveAbs(target, IsAuto, isFine);
            }

            if (waitErrors != 0)
                return -1;

            foreach (var kv in axisPos)
            {
                MotionAxis axis = null;
                double target = kv.Value;

                if (axisObj != null && axisObj.TryGetValue(kv.Key, out axis)) { }
                if (axis == null && Axes.TryGetValue(kv.Key, out var directAxis))
                    axis = directAxis;

                if (axis == null)
                    continue;

                double timeoutMs = 2000;
                if (timeoutMs < 0) timeoutMs = axis.Setup.MoveTimeoutMs;

                var sw = Stopwatch.StartNew();
                while (sw.ElapsedMilliseconds < timeoutMs)
                {
                    if (axis.InPosition(target))
                        break;
                    Thread.Sleep(1);
                }

                if (axis.WaitMoveDone(-1) != 0 && axis.InPosition(target) == false)
                    waitErrors++;
            }

            return waitErrors == 0 ? 0 : -1;
        }

        public override void StopTeachingPositionOnce(int selIndex)
        {
            string teachName = string.Empty;
            bool bSucceeded = Config.GetTeachingPositionName(selIndex, out teachName);
            if (!bSucceeded)
                return;

            var list = GetTeachingList();
            var tp = list.FirstOrDefault(t => t != null && string.Equals(t.Name, teachName, StringComparison.OrdinalIgnoreCase));
            if (tp == null)
                return;

            var axisPos = GetAxisPositions(tp);
            if (axisPos == null)
                return;

            var axisObj = GetAxisObjects(tp);

            foreach (var kv in axisPos)
            {
                MotionAxis axis = null;
                if (axisObj != null && axisObj.TryGetValue(kv.Key, out axis)) { }
                if (axis == null && Axes.TryGetValue(kv.Key, out var direct))
                    axis = direct;
                if (axis == null) continue;

                try { axis.Stop(); } catch { }
            }
        }


        /// <summary>
        /// UI(Teaching Page 등)에서 "티칭 인덱스" 기반으로 해당 티칭 위치로 이동시키는 공용 API.
        /// - 다른 Page에서도 동일하게 사용 가능
        /// </summary>
        public int MoveToTeachingPositionBySelectionIndex(int teachingSelIndex, bool isFine = false)
        {
            if (Config == null)
                return -1;

            string tpName;
            if (!Config.GetTeachingPositionName(teachingSelIndex, out tpName) || string.IsNullOrWhiteSpace(tpName))
                return -1;

            IndexChipProbeControllerRecipe.TeachingPositionName en;
            if (!Enum.TryParse(tpName, out en))
            {
                return -1;
            }

            int nIndex = -1;
            switch (en)
            {
                // ===== Top =====
                case IndexChipProbeControllerRecipe.TeachingPositionName.Top_Index1_Contact:
                    nIndex = 0;
                    return MovePositionTopContact_Index_Up(nIndex, isFine);
                case IndexChipProbeControllerRecipe.TeachingPositionName.Top_Index1_Ready:
                    nIndex = 0;
                    return MovePositionTopContact_Index_Ready(nIndex, isFine);
                case IndexChipProbeControllerRecipe.TeachingPositionName.Top_Index2_Contact:
                    nIndex = 1;
                    return MovePositionTopContact_Index_Up(nIndex, isFine);
                case IndexChipProbeControllerRecipe.TeachingPositionName.Top_Index2_Ready:
                    nIndex = 1;
                    return MovePositionTopContact_Index_Ready(nIndex, isFine);

                case IndexChipProbeControllerRecipe.TeachingPositionName.Top_Index3_Contact:
                    nIndex = 2;
                    return MovePositionTopContact_Index_Up(nIndex, isFine);
                case IndexChipProbeControllerRecipe.TeachingPositionName.Top_Index3_Ready:
                    nIndex = 2;
                    return MovePositionTopContact_Index_Ready(nIndex, isFine);
                case IndexChipProbeControllerRecipe.TeachingPositionName.Top_Index4_Contact:
                    nIndex = 3;
                    return MovePositionTopContact_Index_Up(nIndex, isFine);
                case IndexChipProbeControllerRecipe.TeachingPositionName.Top_Index4_Ready:
                    nIndex = 3;
                    return MovePositionTopContact_Index_Ready(nIndex, isFine);

                case IndexChipProbeControllerRecipe.TeachingPositionName.Top_Index5_Contact:
                    nIndex = 4;
                    return MovePositionTopContact_Index_Up(nIndex, isFine);
                case IndexChipProbeControllerRecipe.TeachingPositionName.Top_Index5_Ready:
                    nIndex = 4;
                    return MovePositionTopContact_Index_Ready(nIndex, isFine);

                case IndexChipProbeControllerRecipe.TeachingPositionName.Top_Index6_Contact:
                    nIndex = 5;
                    return MovePositionTopContact_Index_Up(nIndex, isFine);
                case IndexChipProbeControllerRecipe.TeachingPositionName.Top_Index6_Ready:
                    nIndex = 5;
                    return MovePositionTopContact_Index_Ready(nIndex, isFine);

                case IndexChipProbeControllerRecipe.TeachingPositionName.Top_Index7_Contact:
                    nIndex = 6;
                    return MovePositionTopContact_Index_Up(nIndex, isFine);
                case IndexChipProbeControllerRecipe.TeachingPositionName.Top_Index7_Ready:
                    nIndex = 6;
                    return MovePositionTopContact_Index_Ready(nIndex, isFine);

                case IndexChipProbeControllerRecipe.TeachingPositionName.Top_Index8_Contact:
                    nIndex = 7;
                    return MovePositionTopContact_Index_Up(nIndex, isFine);
                case IndexChipProbeControllerRecipe.TeachingPositionName.Top_Index8_Ready:
                    nIndex = 7;
                    return MovePositionTopContact_Index_Ready(nIndex, isFine);

                // ===== Bottom =====
                case IndexChipProbeControllerRecipe.TeachingPositionName.Bottom_Index1_Contact:
                    nIndex = 0;
                    return MovePositionBottomContact_Index_Up(nIndex, isFine);
                case IndexChipProbeControllerRecipe.TeachingPositionName.Bottom_Index1_Ready:
                    nIndex = 0;
                    return MovePositionBottomContact_Index_Ready(nIndex, isFine);

                case IndexChipProbeControllerRecipe.TeachingPositionName.Bottom_Index2_Contact:
                    nIndex = 1;
                    return MovePositionBottomContact_Index_Up(nIndex, isFine);
                case IndexChipProbeControllerRecipe.TeachingPositionName.Bottom_Index2_Ready:
                    nIndex = 1;
                    return MovePositionBottomContact_Index_Ready(nIndex, isFine);

                case IndexChipProbeControllerRecipe.TeachingPositionName.Bottom_Index3_Contact:
                    nIndex = 2;
                    return MovePositionBottomContact_Index_Up(nIndex, isFine);
                case IndexChipProbeControllerRecipe.TeachingPositionName.Bottom_Index3_Ready:
                    nIndex = 2;
                    return MovePositionBottomContact_Index_Ready(nIndex, isFine);

                case IndexChipProbeControllerRecipe.TeachingPositionName.Bottom_Index4_Contact:
                    nIndex = 3;
                    return MovePositionBottomContact_Index_Up(nIndex, isFine);
                case IndexChipProbeControllerRecipe.TeachingPositionName.Bottom_Index4_Ready:
                    nIndex = 3;
                    return MovePositionBottomContact_Index_Ready(nIndex, isFine);

                case IndexChipProbeControllerRecipe.TeachingPositionName.Bottom_Index5_Contact:
                    nIndex = 4;
                    return MovePositionBottomContact_Index_Up(nIndex, isFine);
                case IndexChipProbeControllerRecipe.TeachingPositionName.Bottom_Index5_Ready:
                    nIndex = 4;
                    return MovePositionBottomContact_Index_Ready(nIndex, isFine);

                case IndexChipProbeControllerRecipe.TeachingPositionName.Bottom_Index6_Contact:
                    nIndex = 5;
                    return MovePositionBottomContact_Index_Up(nIndex, isFine);
                case IndexChipProbeControllerRecipe.TeachingPositionName.Bottom_Index6_Ready:
                    nIndex = 5;
                    return MovePositionBottomContact_Index_Ready(nIndex, isFine);

                case IndexChipProbeControllerRecipe.TeachingPositionName.Bottom_Index7_Contact:
                    nIndex = 6;
                    return MovePositionBottomContact_Index_Up(nIndex, isFine);
                case IndexChipProbeControllerRecipe.TeachingPositionName.Bottom_Index7_Ready:
                    nIndex = 6;
                    return MovePositionBottomContact_Index_Ready(nIndex, isFine);

                case IndexChipProbeControllerRecipe.TeachingPositionName.Bottom_Index8_Contact:
                    nIndex = 7;
                    return MovePositionBottomContact_Index_Up(nIndex, isFine);
                case IndexChipProbeControllerRecipe.TeachingPositionName.Bottom_Index8_Ready:
                    nIndex = 7;
                    return MovePositionBottomContact_Index_Ready(nIndex, isFine);

                // ===== Etc =====
                case IndexChipProbeControllerRecipe.TeachingPositionName.SafetyZone:
                    return MovePositionSafetyZ(isFine);

                case IndexChipProbeControllerRecipe.TeachingPositionName.SphereZ_Ready:
                    return MovePositionSphereZReady(isFine);

                case IndexChipProbeControllerRecipe.TeachingPositionName.SphereZ_Measure:
                    return MovePositionSphereZDown(isFine);

                case IndexChipProbeControllerRecipe.TeachingPositionName.GripperX_Ready:
                    return MovePositionGripperXReady(isFine);

                case IndexChipProbeControllerRecipe.TeachingPositionName.GripperX_Clamp:
                    return MovePositionGripperXClamp(isFine);

                //case IndexChipProbeControllerRecipe.TeachingPositionName.GripperX_Index_Contact:
                //    return MovePositionProbeZGripperIndexUp(isFine);

                case IndexChipProbeControllerRecipe.TeachingPositionName.GripperX_Index1_Contact:
                    nIndex = 0;
                    return MovePositionProbeZGripperIndexUp(nIndex, isFine);

                case IndexChipProbeControllerRecipe.TeachingPositionName.GripperX_Index2_Contact:
                    nIndex = 1;
                    return MovePositionProbeZGripperIndexUp(nIndex, isFine);

                case IndexChipProbeControllerRecipe.TeachingPositionName.GripperX_Index3_Contact:
                    nIndex = 2;
                    return MovePositionProbeZGripperIndexUp(nIndex, isFine);

                case IndexChipProbeControllerRecipe.TeachingPositionName.GripperX_Index4_Contact:
                    nIndex = 3;
                    return MovePositionProbeZGripperIndexUp(nIndex, isFine);

                case IndexChipProbeControllerRecipe.TeachingPositionName.GripperX_Index5_Contact:
                    nIndex = 4;
                    return MovePositionProbeZGripperIndexUp(nIndex, isFine);

                case IndexChipProbeControllerRecipe.TeachingPositionName.GripperX_Index6_Contact:
                    nIndex = 5;
                    return MovePositionProbeZGripperIndexUp(nIndex, isFine);

                case IndexChipProbeControllerRecipe.TeachingPositionName.GripperX_Index7_Contact:
                    nIndex = 6;
                    return MovePositionProbeZGripperIndexUp(nIndex, isFine);

                case IndexChipProbeControllerRecipe.TeachingPositionName.GripperX_Index8_Contact:
                    nIndex = 7;
                    return MovePositionProbeZGripperIndexUp(nIndex, isFine);

                default:
                    return -1;
            }
        }

        // ===== Distance Interlock (ProbeZ -> ProbeCardZ Max) =====

        private bool TryGetAxisPositionsForDistanceCheck(out double probeZ, out double probeCardZ)
        {
            probeZ = 0;
            probeCardZ = 0;

            try
            {
                if (AxisProbeZ == null || AxisProbeCardZ == null)
                    return false;

                probeZ = AxisProbeZ.GetPosition();
                probeCardZ = AxisProbeCardZ.GetPosition();
                return true;
            }
            catch (Exception ex)
            {
                Log.Write(UnitName, nameof(TryGetAxisPositionsForDistanceCheck), ex.Message);
                return false;
            }
        }

        /// <summary>
        /// ProbeZ 위치에 따른 ProbeCardZ 상승 상한값.
        /// (기본 안전값 예시)
        ///  - ProbeZ=5  -> ProbeCardZ Max=15.5
        ///  - ProbeZ=7  -> ProbeCardZ Max=17.5
        /// => Limit(ProbeZ) = ProbeZ + 10.5
        /// </summary>
        private static double CalcProbeCardZMaxByProbeZ(double probeZ)
        {
            return probeZ + 10.5;
        }

        /// <summary>
        /// true면 "위험(이동 금지)" 상태.
        /// 조건: ProbeCardZ가 상한을 초과(>=)하면 금지.
        /// </summary>
        private bool IsDistanceInterlockViolation(double probeZ, double probeCardZ)
        {
            // 튜닝용 안전 마진(mm)
            // +면 더 일찍 막음(더 안전), -면 덜 막음(덜 안전)
            const double marginMm = 0.0;

            double maxProbeCardZ = CalcProbeCardZMaxByProbeZ(probeZ) + marginMm;

            return probeCardZ >= maxProbeCardZ;
        }

        private bool CheckDistanceInterlockAndStopIfViolation(string caller)
        {
            // 축이 없거나 읽기 실패면 "통과" 정책 (원하면 false로 바꿀 수 있음)
            if (!TryGetAxisPositionsForDistanceCheck(out var probeZ, out var probeCardZ))
                return true;

            if (!IsDistanceInterlockViolation(probeZ, probeCardZ))
                return true;

            try
            {
                AxisProbeCardZ?.EmgStop();
                AxisProbeZ?.EmgStop();
            }
            catch { }

            var limit = CalcProbeCardZMaxByProbeZ(probeZ);
            Log.Write(UnitName, caller,
                $"[DistanceInterlock] BLOCKED. ProbeZ={probeZ:F3}, ProbeCardZ={probeCardZ:F3}, ProbeCardZMax={limit:F3}");

            PostAlarm((int)AlarmKeys.eAxisDistanceInterlock);
            return false;
        }

        public void ResetForNewRun(bool moveToSafeReady = true)
        {
            // 2) 안전 위치 복귀(선택)
            if (moveToSafeReady)
            {
                try
                {
                    while (IsRotaryReadyStable(3, 5) == false)
                    {
                        if (IsStop)
                        {
                            return;
                        }
                        Thread.Sleep(1);
                    }
                    EnsureReady();
                }
                catch (Exception ex)
                {
                    Log.Write(UnitName, $"[ResetForNewRun] ResetForNewRun failed: {ex.Message}");
                }
            }
        }

    }
}