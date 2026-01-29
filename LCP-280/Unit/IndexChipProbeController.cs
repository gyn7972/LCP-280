using QMC.Common;
using QMC.Common.Alarm;
using QMC.Common.Cameras;
using QMC.Common.Component;
using QMC.Common.IOUtil;
using QMC.Common.Motion;
using QMC.Common.Motions;
using QMC.Common.PKGTester;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Component;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using static QMC.LCP_280.Process.Equipment;

namespace QMC.LCP_280.Process.Unit
{
    public class IndexChipProbeController : BaseUnit<IndexChipProbeControllerConfig>
    {
        public enum AlarmKeys
        {
            eRotaryAxesMoving = 4701,
            eRotaryNotSafety = 4702,
            eProbeTimeout = 4703,
            eSphereNotForward = 4704,
            eSphereFBTimeout = 4705,
            eProbeCardZNotSafety = 4706,
            eProbeZNotIndexUp = 4707,
            eSphereMoveDownTimeout = 4708,
            eSphereMoveUpTimeout = 4709,
            eAxisDistanceInterlock = 4710, // [ADD] ProbeCardZ(상부) - GripperX(하부) 거리 인터락
        }

        #region InitAlarm
        protected override void InitAlarm()
        {
            base.InitAlarm();
            AlarmInfo alarm = new AlarmInfo();

            alarm.Code = (int)AlarmKeys.eRotaryAxesMoving;
            alarm.Title = "Rotary Axis Moving";
            alarm.Cause = "Rotary 축이 이동 중입니다. 정지 후 다시 시도하십시오.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm.Code = (int)AlarmKeys.eRotaryNotSafety;
            alarm.Title = "Rorary Not Safety Pos.";
            alarm.Cause = "Rorary가 안전 위치가 아닙니다. 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eProbeTimeout;
            alarm.Title = "Probe Timeout.";
            alarm.Cause = "Probe Timeout입니다. Probe 확인 및 재 측정 바랍니다.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eSphereNotForward;
            alarm.Title = "Sphere Cylinder Not Forward.";
            alarm.Cause = "Sphere Cylinder가 Forward 위치가 아닙니다. 포지션 확인 바랍니다.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eSphereFBTimeout;
            alarm.Title = "Sphere Cylinder Forward/Backward Timeout.";
            alarm.Cause = "Sphere Cylinder Forward/Backward Timeout입니다. Cylinder 확인 바랍니다.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eProbeCardZNotSafety;
            alarm.Title = "Probe-Card Z Not Safety.";
            alarm.Cause = "Probe-Card Z 축이 안전 위치가 아닙니다. 포지션 확인 바랍니다.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eProbeZNotIndexUp;
            alarm.Title = "Probe Z Not Index Up.";
            alarm.Cause = "Probe Z 축이 Index Up 위치가 아닙니다. 포지션 확인 바랍니다.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eSphereMoveDownTimeout;
            alarm.Title = "Sphere Move Down Timeout";
            alarm.Cause = "Sphere Z 축이 Down 위치 이동 중 실패하였습니다. 포지션 확인 바랍니다.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eSphereMoveUpTimeout;
            alarm.Title = "Sphere Move Up Timeout";
            alarm.Cause = "Sphere Z 축이 Up 위치 이동 중 실패하였습니다. 포지션 확인 바랍니다.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eAxisDistanceInterlock;
            alarm.Title = "Axis Distance Interlock";
            alarm.Cause = "상/하 축 간 거리 제한 조건 위반으로 이동이 차단되었습니다.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

        }
        #endregion

        #region Unit
        Rotary Rotary { get; set; }
        IndexChipProber IndexChipProber { get; set; }
        #endregion

        #region Config / Teaching /Recipe

        public IndexChipProbeControllerConfig IndexChipProbeControllerConfig => Config;
        private IndexChipProbeControllerRecipe Recipe => Config?.TeachingRecipe;

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
        public IndexChipProbeController(IndexChipProbeControllerConfig config = null) : base(new IndexChipProbeControllerConfig())
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

            BindAxes();
            BindIoDomains();
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
                return r.TeachingPositions;

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
        private void SaveTeachingPosition(TeachingPosition tp)
        {
            var r = Recipe;
            if (r != null)
            {
                r.UpsertFiltered(tp, save: true);
                return;
            }

            Config.SetTeachingPosition(tp);
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

                if(Rotary.IsIndexMoving())
                {
                    AxisProbeCardZ?.EmgStop();
                    AxisProbeZ?.EmgStop();
                    PostAlarm((int)AlarmKeys.eRotaryAxesMoving);
                    return false;
                }

                if(baseComponent == this.AxisProbeZ)
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
                //if (this.Rotary.IsAxisMoving(AxisNames.IndexT))
                //{
                //    AxisProbeCardZ?.EmgStop();
                //    AxisProbeZ?.EmgStop();
                //    PostAlarm((int)AlarmKeys.eRotaryAxesMoving);
                //    return false;
                //}
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

        private int WaitForRotaryIdle(int timeoutMs = -1, int pollMs = 2)
        {
            var sw = Stopwatch.StartNew();
            while (true)
            {
                // Auto 모드에서 Stop 신호 시 즉시 반환
                if (RunMode == UnitRunMode.Auto && IsStop)
                    return 0;

                // 즉시 확인 API 사용(알람 미발행)
                if (this.Rotary.IsIndexMoving() == false)
                {
                    return 0;
                }

                if (timeoutMs >= 0 && sw.ElapsedMilliseconds >= timeoutMs)
                {
                    Log.Write(UnitName, nameof(WaitForRotaryIdle), $"Timeout waiting Rotary idle ({timeoutMs} ms)");
                    return -1;
                }
                Thread.Sleep(pollMs);
            }
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
                OnMovePositionSafetyZ(isFine);
                return 0;
            });
        }
        private int OnMovePositionSafetyZ(bool isFine = false)
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
                    nRet &= OnMoveAxisPositionOne(AxisProbeCardZ, dZPos);
                    if (nRet != 0)
                    {
                        Log.Write(UnitName, $"[OnMovePositionSafetyZ] ProbeCardZ move failed tp={tpName} posZ={dZPos}");
                        return -1;
                    }
                }

                //Gripper Ready로 하고 내려야함.
                if(IsPositionGripperXClamp())
                {
                    nRet &= MovePositionGripperXReady();
                    if (nRet != 0)
                    {
                        Log.Write(UnitName, $"[OnMovePositionSafetyZ] move failed: MovePositionGripperXReady");
                        return -1;
                    }
                }

                //if (IsPositionProbeZSafety() == false) //무조건 움직이자.
                {
                    dZPos = GetTP(tpName, AxisNames.ProbeZ);
                    nRet &= OnMoveAxisPositionOne(AxisProbeZ, dZPos);
                    if (nRet != 0)
                    {
                        Log.Write(UnitName, $"[OnMovePositionSafetyZ] ProbeZ move failed tp={tpName} posZ={dZPos}");
                        return -1;
                    }
                }

                return nRet;
                //return MoveTeachingPositionOnce((int)IndexChipProbeControllerConfig.TeachingPositionName.SafetyZone, isFine);
            }
            finally
            {
                _isSafetyMoving = false;
            }
        }
        //private int OnMovePositionSafetyZ(bool isFine = false)
        //{
        //    _isSafetyMoving = true;
        //    try
        //    {
        //        return MoveTeachingPositionOnce((int)IndexChipProbeControllerConfig.TeachingPositionName.SafetyZone, isFine);
        //    }
        //    finally
        //    {
        //        _isSafetyMoving = false;
        //    }
        //}
        private int IsMoveInterLockSafetyZ()
        {
            int nRet = 0;
            // Check Interlock.!!! 구문 넣을것.!!!
            if (Rotary.IsIndexMoving())
            {
                AxisProbeZ?.EmgStop();
                AxisProbeCardX?.EmgStop();
                AxisProbeCardY?.EmgStop();
                AxisProbeCardZ?.EmgStop();
                AxisSphereZ?.EmgStop();
                PostAlarm((int)AlarmKeys.eRotaryNotSafety);
                Log.Write(UnitName, "IsMoveInterLockSafetyZ", "Fail: Rotary.IsIndexMoving()");
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
                OnMovePositionSafetyProbeZ(isFine);
                return 0;
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
                    nRet &= OnMoveAxisPositionOne(AxisProbeCardZ, dZPos);
                    if (nRet != 0)
                    {
                        Log.Write(UnitName, $"[OnMovePositionSafetyZ] ProbeCardZ move failed tp={tpName} posZ={dZPos}");
                        return -1;
                    }
                }

                //Gripper Ready로 하고 내려야함.
                if (IsPositionGripperXClamp())
                {
                    nRet &= MovePositionGripperXReady();
                    if (nRet != 0)
                    {
                        Log.Write(UnitName, $"[OnMovePositionSafetyZ] move failed: MovePositionGripperXReady");
                        return -1;
                    }
                }

                //if (IsPositionProbeZSafety() == false) //무조건 움직이자.
                {
                    dZPos = GetTP(tpName, AxisNames.ProbeZ);
                    nRet &= OnMoveAxisPositionOne(AxisProbeZ, dZPos);
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
            if (Rotary.IsIndexMoving())
            {
                AxisProbeZ?.EmgStop();
                AxisProbeCardX?.EmgStop();
                AxisProbeCardY?.EmgStop();
                AxisProbeCardZ?.EmgStop();
                AxisSphereZ?.EmgStop();
                PostAlarm((int)AlarmKeys.eRotaryNotSafety);
                Log.Write(UnitName, "IsMoveInterLockSafetyZ", "Fail: Rotary.IsIndexMoving()");
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
                OnMovePositionSafetyProbeCardZ(isFine);
                return 0;
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
                    nRet &= OnMoveAxisPositionOne(AxisProbeCardZ, dZPos);
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
            if (Rotary.IsIndexMoving())
            {
                AxisProbeZ?.EmgStop();
                AxisProbeCardX?.EmgStop();
                AxisProbeCardY?.EmgStop();
                AxisProbeCardZ?.EmgStop();
                AxisSphereZ?.EmgStop();
                PostAlarm((int)AlarmKeys.eRotaryNotSafety);
                Log.Write(UnitName, "IsMoveInterLockSafetyZ", "Fail: Rotary.IsIndexMoving()");
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
                OnMovePositionSphereZDown(isFine);
                return 0;
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
            if (Rotary.IsIndexMoving())
            {
                AxisProbeZ?.EmgStop();
                AxisProbeCardX?.EmgStop();
                AxisProbeCardY?.EmgStop();
                AxisProbeCardZ?.EmgStop();
                AxisSphereZ?.EmgStop();
                PostAlarm((int)AlarmKeys.eRotaryNotSafety);
                Log.Write(UnitName, "IsMoveInterLockSphereZDown", "Fail: Rotary.IsIndexMoving()");
                nRet = -1;
                return nRet;
            }

            //if (Rotary != null && Rotary.IsAxisMoving(AxisNames.IndexT))
            //{
            //    AxisProbeZ?.EmgStop();
            //    AxisProbeCardX?.EmgStop();
            //    AxisProbeCardY?.EmgStop();
            //    AxisProbeCardZ?.EmgStop();
            //    AxisSphereZ?.EmgStop();
            //    PostAlarm((int)AlarmKeys.eRotaryNotSafety);
            //    nRet = -1;
            //    return nRet;
            //}
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
                OnMovePositionSphereZReady(isFine);
                return 0;
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
            if (Rotary.IsIndexMoving())
            {
                AxisProbeZ?.EmgStop();
                AxisProbeCardX?.EmgStop();
                AxisProbeCardY?.EmgStop();
                AxisProbeCardZ?.EmgStop();
                AxisSphereZ?.EmgStop();
                PostAlarm((int)AlarmKeys.eRotaryNotSafety);
                Log.Write(UnitName, "IsMoveInterLockSphereZReady", "Fail: Rotary.IsIndexMoving()");
                nRet = -1;
                return nRet;
            }

            //if (Rotary != null && Rotary.IsAxisMoving(AxisNames.IndexT))
            //{
            //    AxisProbeZ?.EmgStop();
            //    AxisProbeCardX?.EmgStop();
            //    AxisProbeCardY?.EmgStop();
            //    AxisProbeCardZ?.EmgStop();
            //    AxisSphereZ?.EmgStop();
            //    PostAlarm((int)AlarmKeys.eRotaryNotSafety);
            //    nRet = -1;
            //    return nRet;
            //}
            return nRet;
        }
        

        public int MovePositionTopContact_Index_Up(int nIndex = 0, bool isFine = false)
        {
            if (IsTopContactIndexZUp(nIndex) == true)
            {
                return 0;
            }

            if(IsTopRequired() == false)
            {
                Log.Write(UnitName, "MovePositionTopContact_Index_Contact", "Top Mode가 아닙니다.");
                return -1;
            }

            if(nIndex != GetProbeIndexNo())
            {
                Log.Write(UnitName, "MovePositionTopContact_Index_Contact", "Index Number가 맞지 않습니다.");
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
                OnMovePositionTopContact_Index_Up(nIndex, isFine);
                return 0;
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

            //if (IsTopContactIndexZUp(nIndex) == true)
            //{
            //    nRet = OnMovePositionTopContact_Index_ReadyZ(nIndex, isFine);
            //    if (nRet != 0)
            //    {
            //        Log.Write(UnitName, "[OnMovePositionTopContact_Index_ReadyZ] ToolT move failed");
            //        return -1;
            //    }
            //}

            //string tpName = $"Top_Index{teachingIdx}_Up";
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

            while(true)
            {
                if(IsStop)
                {
                    return 0;
                }

                if(IsTopContactIndexZUp(nIndex))
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
            if (Rotary.IsIndexMoving())
            {
                AxisProbeZ?.EmgStop();
                AxisProbeCardX?.EmgStop();
                AxisProbeCardY?.EmgStop();
                AxisProbeCardZ?.EmgStop();
                AxisSphereZ?.EmgStop();
                PostAlarm((int)AlarmKeys.eRotaryNotSafety);
                Log.Write(UnitName, "IsMoveInterLockTopContact_Index_Up", "Fail: Rotary.IsIndexMoving()");
                nRet = -1;
                return nRet;
            }

            //if (Rotary != null && Rotary.IsAxisMoving(AxisNames.IndexT))
            //{
            //    AxisProbeZ?.EmgStop();
            //    AxisProbeCardX?.EmgStop();
            //    AxisProbeCardY?.EmgStop();
            //    AxisProbeCardZ?.EmgStop();
            //    AxisSphereZ?.EmgStop();
            //    PostAlarm((int)AlarmKeys.eRotaryNotSafety);
            //    nRet = -1;
            //    return nRet;
            //}
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
                OnMovePositionTopContact_Index_Ready(nIndex, isFine);
                return 0;
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
            if (Rotary.IsIndexMoving())
            {
                AxisProbeZ?.EmgStop();
                AxisProbeCardX?.EmgStop();
                AxisProbeCardY?.EmgStop();
                AxisProbeCardZ?.EmgStop();
                AxisSphereZ?.EmgStop();
                PostAlarm((int)AlarmKeys.eRotaryNotSafety);
                Log.Write(UnitName, "IsMoveInterLockTopContact_Index_Ready", "Fail: Rotary.IsIndexMoving()");
                nRet = -1;
                return nRet;
            }

            //if (Rotary != null && Rotary.IsAxisMoving(AxisNames.IndexT))
            //{
            //    AxisProbeZ?.EmgStop();
            //    AxisProbeCardX?.EmgStop();
            //    AxisProbeCardY?.EmgStop();
            //    AxisProbeCardZ?.EmgStop();
            //    AxisSphereZ?.EmgStop();
            //    PostAlarm((int)AlarmKeys.eRotaryNotSafety);
            //    nRet = -1;
            //    return nRet;
            //}
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
                Log.Write(UnitName, "MovePositionTopContact_Index_Up", "Bottom Mode가 아닙니다.");
                return -1;
            }

            if (nIndex != GetProbeIndexNo())
            {
                Log.Write(UnitName, "MovePositionTopContact_Index_Up", "Index Number가 맞지 않습니다.");
                return -1;
            }

            Task<int> task = MovePositionAsyncBottomContact_Index_Up(nIndex, isFine);
            while (IsEndTask(task) == false)
            {
                IsMoveInterLockBottomContact_Index_Up();
                Thread.Sleep(1);
            }
            return task.Result;
        }
        public Task<int> MovePositionAsyncBottomContact_Index_Up(int nIndex = 0, bool isFine = false)
        {
            return Task.Run(() =>
            {
                OnMovePositionBottomContact_Index_Up(nIndex, isFine);
                return 0;
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

            //nRet = MovePositionBottomContact_Index_Ready(nIndex, isFine);
            //if (nRet != 0)
            //{
            //    Log.Write(UnitName, $"[OnMovePositionBottomContact_Index_Up] MovePositionBottomContact_Index_ReadyZ failed index={nIndex}");
            //    return -1;
            //}
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
                nRet &= OnMoveAxisPositionOne(AxisProbeCardZ, dZPos);
                if (nRet != 0)
                {
                    Log.Write(UnitName, $"[OnMovePositionBottomContact_Index_Ready] ToolT move failed tp={tpName} posZ={dZPos}");
                    return -1;
                }
            }

            //tpName = $"Bottom_Index{teachingIdx}_Up";
            tpName = Recipe.GetBottomContactName(nIndex);
            tpObj = GetTeachingPosition(tpName);
            if (tpObj == null)
            {
                Log.Write(UnitName, $"[OnMovePositionBottomContact_Index_Up] Teaching not found: {tpName}");
                return -1;
            }

            double dXPos = GetTP(tpName, AxisNames.ProbeCardX);
            double dYPos = GetTP(tpName, AxisNames.ProbeCardY);
            nRet &= OnMoveAxisPositionOne(AxisProbeCardX, dXPos);
            nRet &= OnMoveAxisPositionOne(AxisProbeCardY, dYPos);
            if (nRet != 0)
            {
                Log.Write(UnitName, $"[OnMovePositionBottomContact_Index_Up] ToolT move failed tp={tpName} posX={dXPos}");
                Log.Write(UnitName, $"[OnMovePositionBottomContact_Index_Up] ToolT move failed tp={tpName} posY={dYPos}");
                return -1;
            }

            dZPos = GetTP(tpName, AxisNames.ProbeCardZ);
            nRet &= OnMoveAxisPositionOne(AxisProbeCardZ, dZPos);
            if (nRet != 0)
            {
                Log.Write(UnitName, $"[OnMovePositionBottomContact_Index_Up] ToolT move failed tp={tpName} posZ={dZPos}");
                return -1;
            }

            return nRet;
        }
        private int IsMoveInterLockBottomContact_Index_Up()
        {
            int nRet = 0;
            // Check Interlock.!!! 구문 넣을것.!!!
            if (Rotary.IsIndexMoving())
            {
                AxisProbeZ?.EmgStop();
                AxisProbeCardX?.EmgStop();
                AxisProbeCardY?.EmgStop();
                AxisProbeCardZ?.EmgStop();
                AxisSphereZ?.EmgStop();
                PostAlarm((int)AlarmKeys.eRotaryNotSafety);
                Log.Write(UnitName, "IsMoveInterLockBottomContact_Index_Up", "Fail: Rotary.IsIndexMoving()");
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

            if (IsPositionProbeZGripperIndexUp() == false)
            {
                AxisProbeCardZ?.EmgStop();
                PostAlarm((int)AlarmKeys.eProbeZNotIndexUp);
                Log.Write(UnitName, $"[IsMoveInterLockBottomContact_Index_Up] GripperX not Index Up");
                nRet = -1;
                return nRet;
            }

            //if (Rotary != null && Rotary.IsAxisMoving(AxisNames.IndexT))
            //{
            //    AxisProbeZ?.EmgStop();
            //    AxisProbeCardX?.EmgStop();
            //    AxisProbeCardY?.EmgStop();
            //    AxisProbeCardZ?.EmgStop();
            //    AxisSphereZ?.EmgStop();
            //    PostAlarm((int)AlarmKeys.eRotaryNotSafety);
            //    nRet = -1;
            //    return nRet;
            //}
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
                OnMovePositionBottomContact_Index_Ready(nIndex, isFine);
                return 0;
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
                nRet &= OnMoveAxisPositionOne(AxisProbeCardZ, dZPos);
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
            nRet &= OnMoveAxisPositionOne(AxisProbeCardX, dXPos);
            nRet &= OnMoveAxisPositionOne(AxisProbeCardY, dYPos);
            if (nRet != 0)
            {
                Log.Write(UnitName, $"[OnMovePositionBottomContact_Index_Ready] ToolT move failed tp={tpName} posX={dXPos}");
                Log.Write(UnitName, $"[OnMovePositionBottomContact_Index_Ready] ToolT move failed tp={tpName} posY={dYPos}");
                return -1;
            }

            dZPos = GetTP(tpName, AxisNames.ProbeCardZ);
            nRet &= OnMoveAxisPositionOne(AxisProbeCardZ, dZPos);
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
            if (Rotary.IsIndexMoving())
            {
                AxisProbeZ?.EmgStop();
                AxisProbeCardX?.EmgStop();
                AxisProbeCardY?.EmgStop();
                AxisProbeCardZ?.EmgStop();
                AxisSphereZ?.EmgStop();
                PostAlarm((int)AlarmKeys.eRotaryNotSafety);
                Log.Write(UnitName, "IsMoveInterLockBottomContact_Index_Ready", "Fail: Rotary.IsIndexMoving()");
                nRet = -1;
                return nRet;
            }
            return nRet;
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
                OnMovePositionGripperXReady(isFine);
                return 0;
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
            if (Rotary.IsIndexMoving())
            {
                AxisProbeZ?.EmgStop();
                AxisProbeCardX?.EmgStop();
                AxisProbeCardY?.EmgStop();
                AxisProbeCardZ?.EmgStop();
                AxisSphereZ?.EmgStop();
                PostAlarm((int)AlarmKeys.eRotaryNotSafety);
                Log.Write(UnitName, "IsMoveInterLockGripperXReady", "Fail: Rotary.IsIndexMoving()");
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
                OnMovePositionGripperXClamp(isFine);
                return 0;
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
            if (Rotary.IsIndexMoving())
            {
                AxisProbeZ?.EmgStop();
                AxisProbeCardX?.EmgStop();
                AxisProbeCardY?.EmgStop();
                AxisProbeCardZ?.EmgStop();
                AxisSphereZ?.EmgStop();
                PostAlarm((int)AlarmKeys.eRotaryNotSafety);
                Log.Write(UnitName, "IsMoveInterLockGripperXClamp", "Fail: Rotary.IsIndexMoving()");
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

        public int MovePositionProbeZGripperIndexUp(bool isFine = false)
        {
            Task<int> task = MovePositionAsyncProbeZGripperIndexUp(isFine);
            while (IsEndTask(task) == false)
            {
                IsMoveInterLockProbeZGripperIndexUp();
                Thread.Sleep(1);
            }
            return task.Result;
        }
        public Task<int> MovePositionAsyncProbeZGripperIndexUp(bool isFine = false)
        {
            return Task.Run(() =>
            {
                OnMovePositionProbeZGripperIndexUp(isFine);
                return 0;
            });
        }
        private int OnMovePositionProbeZGripperIndexUp(bool isFine = false)
        {
            return MoveTeachingPositionOnce((int)IndexChipProbeControllerRecipe.TeachingPositionName.GripperX_Index_Contact, isFine);
        }
        private int IsMoveInterLockProbeZGripperIndexUp()
        {
            int nRet = 0;
            // Check Interlock.!!! 구문 넣을것.!!!
            if (Rotary.IsIndexMoving())
            {
                AxisProbeZ?.EmgStop();
                AxisProbeCardX?.EmgStop();
                AxisProbeCardY?.EmgStop();
                AxisProbeCardZ?.EmgStop();
                AxisSphereZ?.EmgStop();
                PostAlarm((int)AlarmKeys.eRotaryNotSafety);
                Log.Write(UnitName, "IsMoveInterLockProbeZGripperIndexUp", "Fail: Rotary.IsIndexMoving()");
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
        public bool IsPositionProbeZGripperIndexUp()
        {
            bool bRet = false;
            bRet = IsAxisInTeachingPosition(AxisProbeZ,
                IndexChipProbeControllerRecipe.TeachingPositionName.GripperX_Index_Contact.ToString(),
                AxisNames.ProbeZ);
            return bRet;
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
            return  IsAxisProbeCardZSafetyPos();   // SafetyZone의 ProbeCardZ만 검사
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
            const double zeroTolerance = 0.007;
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
            const double zeroTolerance = 0.007;
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
                return true;

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
                Thread.Sleep(5); // 약간의 딜레이
                WriteOutput(IndexChipProbeControllerConfig.IO.BLADE_CONTACT_VLV, true);
            }
            else
            {
                // Probe Contact
                WriteOutput(IndexChipProbeControllerConfig.IO.BLADE_CONTACT_VLV, false);
                Thread.Sleep(5); // 약간의 딜레이
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
        public bool IsSphereForward()
        {
            if (Config.IsSimulation || Config.IsDryRun)
            {
                return true;
            }

            return ReadInput(NAME_SPHERE_FW);   // Forward sensor
        }
        public bool IsSphereBackward()
        {
            if (Config.IsSimulation || Config.IsDryRun)
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
            this.RunUnitStatus = UnitStatus.Stopped;

            base.OnStop();
            return ret;
        }
        protected override int OnRunReady() { return 0; }
        protected override int OnRunWork() { return 0; }
        protected override int OnRunComplete() { return 0; }
        #endregion

        public int IsRotaryIdle()
        {
            if (Rotary.IsIndexMoving())
            {
                return -1;
            }
            //if (Rotary != null && Rotary.IsAxisMoving(AxisNames.IndexT))
            //{
            //    //AxisProbeCardX.EmgStop();
            //    //AxisProbeCardY.EmgStop();
            //    //AxisProbeCardZ.EmgStop();
            //    //AxisProbeZ.EmgStop();
            //    //AxisSphereZ.EmgStop();

            //    //PostAlarm((int)AlarmKeys.eRotaryNotSafe);
            //    return -1;
            //}
            return 0;
        }

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
        private int MeasureChipWithNgRetry(MaterialDie die, int maxRetry, int retryDelayMs = 50)
        {
            // maxRetry=2 => 총 3회 시도
            if (maxRetry < 0) 
                maxRetry = 0;

            for (int attempt = 0; attempt <= maxRetry; attempt++)
            {
                if (IsStop)
                    return 0;

                int rc = IndexChipProber.MeasureChip();
                if (rc != 0)
                {
                    Log.Write(UnitName, nameof(MeasureChipWithNgRetry),
                        $"MeasureChip failed. attempt={attempt + 1}/{maxRetry + 1} rc={rc}");
                    return -1;
                }

                // [ADD] Simulation/DryRun: BinType을 랜덤으로 강제 세팅해서 NG/Retry 테스트
                if ((Config.IsSimulation || Config.IsDryRun) && die != null)
                {
                    if(false)
                    {
                        try
                        {
                            // 확률 클램프
                            double p = SimulationGoodProbability;
                            if (p < 0.0) p = 0.0;
                            if (p > 1.0) p = 1.0;

                            bool isGood;
                            lock (_simRandLock)
                            {
                                isGood = _simRand.NextDouble() < p;
                            }

                            die.TesterResult.BinningResult.BinType = isGood ? BinningType.GoodBin : BinningType.NgBin;

                            Log.Write(UnitName, nameof(MeasureChipWithNgRetry),
                                $"[SIM] Forced BinType={die.TesterResult.BinningResult.BinType} pGood={p:0.###} attempt={attempt + 1}/{maxRetry + 1}");
                        }
                        catch (Exception ex)
                        {
                            Log.Write(UnitName, nameof(MeasureChipWithNgRetry), $"[SIM] Failed to force BinType: {ex.Message}");
                        }
                    }
                }

                // BinType 확인
                if (TryGetDieBinType(die, out var binType))
                {
                    if (binType != QMC.Common.PKGTester.BinningType.NgBin)
                    {
                        die.State = DieProcessState.Inspected;
                        Log.Write(UnitName, nameof(MeasureChipWithNgRetry),
                            $"Measure OK. BinType={binType} attempt={attempt + 1}/{maxRetry + 1}");
                        return 0;
                    }

                    //여기에서 결과값을 확인 후에 검사 결과 및 Retry 처리
                    if (true)
                    {
                        Log.Write(UnitName, "[MeasureChip] AddContactRetry");
                        try
                        {
                            var ctx = Equipment.Instance.SummaryContext;
                            ctx.GetCurrentSummaryOrNull()?.AddContactRetry();
                        }
                        catch (Exception ex)
                        { Log.Write(ex); }
                    }

                    // NG면 재시도
                    Log.Write(UnitName, nameof(MeasureChipWithNgRetry),
                        $"Measure result NG -> retry. attempt={attempt + 1}/{maxRetry + 1}");

                    if (attempt < maxRetry)
                        Thread.Sleep(Math.Max(0, retryDelayMs));
                }
                else
                {
                    // Bin 정보를 못 읽으면 기존 정책대로 "성공"으로 처리(현장 정책에 따라 NG 처리로 바꿀 수 있음)
                    Log.Write(UnitName, nameof(MeasureChipWithNgRetry),
                        $"Measure OK but BinType not available -> treat as success.");
                    return 0;
                }
            }

            // 여기까지 오면 "계속 NG"
            die.SetReject("Error_Measure");
            return -1;
        }


        private void LogSequence(string log)
        {
                if (this.CurrentFunc == null)
                    return;

                Log.Write(UnitName, this.CurrentFunc.Method.Name, $"[Sequence] {log}");
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

            //this.SequencePlayers.Add(b => (IsBottomRequired() ? BottomContactOnce(b) : TopContact(b)));
            //this.SequencePlayers.Add(ContactBottomOrTop);
            //this.SequencePlayers.Add(BottomContactOnce);
            //this.SequencePlayers.Add(TopContact);

        }
        #region Seq 단위 동작 함수

        public int RunInspectionReady(bool bFineSpeed = false)
        {
            int nRet = -1;

            //이거 계속 있어야 돼나.. 확인하고 뺴자.
            while (IsRotaryIdle() != 0)
            {
                if (IsStop)
                {
                    return 0;
                }
                Thread.Sleep(2);
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
                    //if (SetSphereFB(true))
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
                            Thread.Sleep(1);
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

                        Thread.Sleep(1);
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
                            Thread.Sleep(1);
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
            LogSequence("Start");

            Log.Write("kkkkkkProb", "Start");
            while (IsRotaryIdle() != 0)
            {
                if (IsStop)
                {
                    return 0;
                }
                Thread.Sleep(1);
            }

            try
            {
                Log.Write("kkkkkkProb", "Start2");
                MaterialDie die = this.Rotary.GetProbeSocketMaterial();
                int nIndex = this.GetProbeIndexNo();
                bool bUseSocket = this.Rotary.Config.GetUseSocket(nIndex);
                if (bUseSocket == false)
                {
                    Log.Write(UnitName, "[RunInspection] Socket not used. Skip inspection.");
                    return 0;
                }
                if (die == null)
                {
                    return 0;
                }
                if (die.Presence != Material.MaterialPresence.Exist)
                {
                    return 0;
                }
                if (die.State == DieProcessState.Rejected)
                {
                    return 0;
                }

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

                var socket = this.Rotary.GetSocket(nIndex);
                socket.SetState(Rotary.RotarySocketState.Probing);
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

                    Log.Write("kkkkkkProb", "Start3");
                    nRet = TopContactAndMeasureOnce(bFineSpeed);
                    if (nRet != 0)
                    {
                        Log.Write(UnitName, "[RunInspection] TopContactAndMeasureOnce failed");
                        return -1;
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
                        return -1;
                    }
                }

                if(nRet == 0)
                {
                    //die.State = DieProcessState.Inspected;
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


                    //die.State = DieProcessState.Error_Probe;
                    die.SetReject("Error_Probe");
                    die.ProcessSatate = Material.MaterialProcessSatate.Processing; //Skip? Unloader는 해야지?
                    die.Presence = Material.MaterialPresence.Exist;
                }

                //MovePositionSafetyZ(); 내부에서 진행.
                //nRet = MovePositionGripperXReady(bFineSpeed);
                //if (nRet != 0)
                //{
                //    Log.Write(UnitName, "[BottomContactOnce] MovePositionGripperXReady failed");
                //    return -1;
                //}

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

                SetProbeVac(false);
                socket.SetState(Rotary.RotarySocketState.Probed);
                LogSequence("End");
                return nRet;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            finally
            {
                SetProbeVac(false);
                if (this.IsProbeSafetyAxisPos() == false)
                {
                    nRet = MovePositionSafetyZ();
                    if (nRet != 0)
                    {
                        Log.Write(UnitName, "RunInspection", "MovePositionSafetyZ failed");
                        nRet = -1;
                    }
                    //Log.Write(UnitName, "RunInspection", "finally->여기가 들어오면...");
                }
            }
            return nRet;
        }

        public int TopContactAndMeasureOnce(bool bFineSpeed = false)
        {
            int nRet = 0;
            try
            {
                this.CurrentFunc = TopContactAndMeasureOnce;
                LogSequence("Start");

                while (IsRotaryIdle() != 0)
                {
                    if (IsStop)
                    {
                        return 0;
                    }
                    Thread.Sleep(1);
                }

                Log.Write("kkkkkkProb", "Start4");
                int nIndex = GetProbeIndexNo();
                
                Log.Write("kkkkkkProb", "Start5");
                //nRet = MovePositionTopContact_Index_Ready(nIndex, bFineSpeed);
                //if (nRet != 0)
                //{
                //    Log.Write(UnitName, "TopContactAndMeasureOnce", "[TopContactOnce] MovePositionBottomContact_Index_Ready failed");
                //    return -1;
                //}

                Log.Write("kkkkkkProb", "Start6");
                nRet = MovePositionTopContact_Index_Up(nIndex, bFineSpeed);
                if (nRet != 0)
                {
                    Log.Write(UnitName, "TopContactAndMeasureOnce", "[TopContactOnce] OnMovePositionTopContact_Index_Up failed");
                    return -1;
                }

                Log.Write("kkkkkkProb", "Start7");
                WaitByTime(Config.UpperWaitTime);

                //카메라로 컨택 확인 하고 싶을때 모드
                //if (Config.ViewMode == false)
                {
                    // 6) 검사 요구 동기 처리
                    // [CHG] NG면 Retry
                    var die = Rotary != null ? Rotary.GetProbeSocketMaterial() : null;
                    nRet = MeasureChipWithNgRetry(die, maxRetry: 1, retryDelayMs: 50);
                    if (nRet != 0)
                    {
                        Log.Write(UnitName, "[TopContactOnce] MeasureChipWithNgRetry failed (or NG after retries)");
                        return -1;
                    }
                    // Orignal:
                    //nRet = IndexChipProber.MeasureChip();
                    //if (nRet != 0)
                    //{
                    //    Log.Write(UnitName, "[TopContactOnce] MeasureChip failed");
                    //    nRet = - 1;
                    //}
                }
                Log.Write("kkkkkkProb", "Start9");
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
        public int BottomContactAndMeasureOnce(bool bFineSpeed = false)
        {
            int nRet = 0;
            int nIndex = GetProbeIndexNo();
            try
            {
                this.CurrentFunc = BottomContactAndMeasureOnce;
                LogSequence("Start");

                while (IsRotaryIdle() != 0)
                {
                    if (IsStop)
                    {
                        return 0;
                    }
                    Thread.Sleep(1);
                }

                SetProbeVac(true);

                nRet = MovePositionGripperXReady(bFineSpeed);
                if (nRet != 0)
                {
                    Log.Write(UnitName, "[BottomContactOnce] MovePositionGripperXReady failed");
                    return -1;
                }

                nRet = MovePositionProbeZGripperIndexUp(bFineSpeed);
                if (nRet != 0)
                {
                    Log.Write(UnitName, "[BottomContactOnce] MovePositionGripperXIndexUp failed");
                    return -1;
                }

                nRet = MovePositionBottomContact_Index_Ready(nIndex, bFineSpeed);
                if (nRet != 0)
                {
                    Log.Write(UnitName, "[BottomContactOnce] MovePositionBottomContact_Index_Ready failed");
                    return -1;
                }

                if (Config.GripperMode)
                {
                    nRet = MovePositionGripperXClamp(bFineSpeed);
                    if (nRet != 0)
                    {
                        Log.Write(UnitName, "[BottomContactOnce] MovePositionGripperXReady failed");
                        return -1;
                    }
                }

                nRet = MovePositionBottomContact_Index_Up(nIndex, bFineSpeed);
                if (nRet != 0)
                {
                    Log.Write(UnitName, "[BottomContactOnce] MovePositionBottomContact_Index_Up failed");
                    return -1;
                }

                WaitByTime(Config.UpperWaitTime);

                // 6) 검사 요구 동기 처리
                // [CHG] NG면 Retry
                var die = Rotary != null ? Rotary.GetProbeSocketMaterial() : null;
                //nRet = MeasureChipWithNgRetry(die, maxRetry: 1, retryDelayMs: 50);
                nRet = MeasureChipWithNgRetry(die, maxRetry: 0, retryDelayMs: 50);
                if (nRet != 0)
                {
                    Log.Write(UnitName, "[BottomContactOnce] MeasureChipWithNgRetry failed (or NG after retries)");
                    return -1;
                }
                // Orignal:
                //nRet = IndexChipProber.MeasureChip();
                //if (nRet != 0)
                //{
                //    Log.Write(UnitName, "[BottomContactOnce] MeasureChip failed");
                //    return -1;
                //}
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
            return nRet;
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
             || IsPositionProbeZSafety() == false)
            {
                nRet = MovePositionSafetyZ();
                if (nRet != 0)
                {
                    Log.Write(this, "CheckReady Fail - MovePositionSafetyZ");
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

                bool isAuto = RunMode == UnitRunMode.Auto;
                waitErrors = axis.MoveAbs(target, isAuto, isFine);
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

                case IndexChipProbeControllerRecipe.TeachingPositionName.GripperX_Index_Contact:
                    return MovePositionProbeZGripperIndexUp(isFine);

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

    }
}