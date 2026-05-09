using QMC.Common;
using QMC.Common.Alarm;
using QMC.Common.Cameras.HIKVISION;
using QMC.Common.Component;
using QMC.Common.Motion;
using QMC.Common.Motions;
using QMC.Common.Unit;
using QMC.Common.Vision;
using QMC.LCP_280.Process.Component;
using QMC.LCP_280.Process.Unit.FormWork.Repro;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using static QMC.LCP_280.Process.Equipment;
 
namespace QMC.LCP_280.Process.Unit
{
    /// <summary>
    /// IndexLoadAligner Unit
    ///  - Align T / Index Z 축 Teaching Positions 관리
    ///  - OutputStage 스타일 Region/메서드 구조 적용
    ///  - 현재 별도 IO 없음 (추후 필요 시 IO Mapping 추가)
    /// </summary>
    public class IndexLoadAligner : BaseUnit<IndexLoadAlignerConfig>
    {
        public event EventHandler<PatternMarksFoundEventArgs> MarksFound;

        public new enum AlarmKeys
        {
            eAlignTAxesNotReady = 10601,
            eAlignTAxesMoving,
            eRotaryAxesMoving,
            eIndexLoadAlignerRotateMoveToSocket,
        }

        #region InitAlarm
        protected override void InitAlarm()
        {
            string source = "Index_LoadAlign";
            base.InitAlarm();
            // 1. 공용 파일 로더에서 알람 목록 가져오기
            var loadedAlarms = GlobalAlarmTable.Instance.GetAlarmsForSource(source);
            if (loadedAlarms == null || loadedAlarms.Count == 0)
            {
                Log.Write("AlarmInit", $"Cannot find alarms for source '{source}' in the alarm file. Only default alarms will be registered.");

                AlarmInfo alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eAlignTAxesNotReady;
                alarm.Title = "IndexLoadAligner T-Axis Not ReadyPos.";
                alarm.Cause = "IndexLoadAligner T-Axis is not at the ready position.\n Please check the position and restart.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eAlignTAxesMoving;
                alarm.Title = "IndexLoadAligner T-Axis Axis Moving";
                alarm.Cause = "IndexLoadAligner T-Axis is moving. Please stop and try again.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eRotaryAxesMoving;
                alarm.Title = "Rotary Axis Moving";
                alarm.Cause = "Rotary axis is moving. Please stop and try again.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eIndexLoadAlignerRotateMoveToSocket;
                alarm.Title = "IndexLoadAligner Rotate Move To Socket Failed";
                alarm.Cause = "Failed to move IndexLoadAligner rotate to socket position. Please check the mechanism and try again.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
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

        #region Config / Teaching
        public IndexLoadAlignerConfig IndexLoadAlignerConfig => Config;
        public IndexLoadAlignerRecipe Recipe
        {
            get
            {
                return Config.TeachingRecipe;
            }
        }
        #endregion

        #region Unit
        Rotary Rotary { get; set; }
        #endregion

        #region Axes
        private MotionAxis _alignT, _indexZ;
        public MotionAxis AxisAlignT => _alignT;
        public MotionAxis AxisIndexZ => _indexZ;
        #endregion

        #region Camera Binding
        public HIKGigECamera IndexAlignerCam { get; private set; }
        public string IndexAlignerCamKey => "Index_Aligner";
        private void BindCamera()
        {
            var eq = Equipment.Instance;
            if (eq == null)
                return;

            if (eq.Cameras != null && eq.Cameras.TryGetValue(IndexAlignerCamKey, out var cam))
                IndexAlignerCam = cam as HIKGigECamera;
            else
                IndexAlignerCam = eq.Index_AlignerCam;
        }
        public PatternMatchingRunner _pmRunner;
        // Pattern Matching Runner (간소화: Recipe 자동 관리)
        public PatternMatchingRunner PmRunner
        {
            get
            {
                if (_pmRunner == null)
                {
                    _pmRunner = VisionRunnerHub.GetOrCreate(IndexAlignerCamKey);
                }
                return _pmRunner;
            }
        }
        #endregion

        // Safety 동작 중 여부
        private bool _isSafetyMoving = false;

        #region ctor / Initialization
        public IndexLoadAligner(IndexLoadAlignerConfig config = null) : base(new IndexLoadAlignerConfig())
        {
            
            AddComponents();
        }

        public override void AddComponents()
        {
            Config.LoadAndBindAxes(Equipment.Instance.AxisManager);
            Config.InitializeDefaultTeachingPositions();
            
            BindAxes();
            BindCamera();
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
                Log.Write("IndexLoadAligner", "[BindAxes] AxisManager null");
                return;
            }

            const string unitName = "Unit"; // Equipment.CreateAxes 에서 사용한 유닛명과 동일해야 함
            BindAxis(mgr, unitName, AxisNames.AlignT, ref _alignT);
            BindAxis(mgr, unitName, AxisNames.IndexZ, ref _indexZ);
        }
        public override bool IsInterlockOK(BaseComponent baseComponent, BaseComponent.InterlockEventArgs e)
        {
            bool bRet = base.IsInterlockOK(baseComponent, e);
            if (baseComponent == this.AxisIndexZ)
            {
                if (_isSafetyMoving)
                    return true;
                if (!IsRotaryReadyStable(3, 5)) //if (Rotary.IsIndexReadyForAction(out string reason) == false)
                {
                    AxisIndexZ?.EmgStop();
                    PostAlarm((int)AlarmKeys.eRotaryAxesMoving);
                    Log.Write(UnitName, nameof(IsInterlockOK), $"Rotary moving interlock. {Rotary.GetIndexMovingDebugText()}");
                    return false;
                }

                if (this.IsAlignTReady() == false)
                {
                    AxisIndexZ?.EmgStop();
                    PostAlarm((int)AlarmKeys.eAlignTAxesNotReady);
                    return false;
                }
            }
            else if (baseComponent == this.AxisAlignT)
            {
                // AlignT 축 이동시 별도 인터락 없음
            }
            return bRet;
        }
        public int MovePositionSafetyZ(bool isFine = false)
        {
            Task<int> task = MovePositionAsyncSafetyZ(isFine);
            while (IsEndTask(task) == false)
            {
                int nRtn = IsMoveInterLockSafetyZ();
                if (nRtn != 0)
                {
                    return -1;
                }

                Thread.Sleep(1);
            }

            // ??? 이건 막아야 겠는데?
            while (this.IsPositionAlignZSafety() == false)
            {
                if (IsStop)
                {
                    return 0;
                }
                Thread.Sleep(1);
            }
            return task.Result;
        }
        private Task<int> MovePositionAsyncSafetyZ(bool isFine = false)
        {
            return Task.Run(() =>
            {
                OnMovePositionSafetyZ(isFine);
                return 0;
            });
        }
        private int OnMovePositionSafetyZ(bool isFine = false)
        {
            _isSafetyMoving = true;
            try
            {
                return MoveTeachingPositionOnce((int)IndexLoadAlignerRecipe.TeachingPositionName.SafetyZone, isFine);
            }
            finally
            {
                _isSafetyMoving = false;
            }
        }
        private int IsMoveInterLockSafetyZ()
        {
            int nRet = 0;

            if (Rotary != null && !IsRotaryReadyStable(3, 5)) //if (Rotary != null && Rotary.IsIndexReadyForAction(out string reason) == false)
            {
                AxisIndexZ.EmgStop();
                AxisAlignT.EmgStop();
                PostAlarm((int)AlarmKeys.eRotaryAxesMoving);
                Log.Write(UnitName, nameof(IsMoveInterLockSafetyZ), $"Rotary moving interlock. {Rotary.GetIndexMovingDebugText()}");
                return -1;
            }

            //if(IsAxisMoving(AxisNames.AlignT))
            //{
            //    AxisIndexZ.EmgStop();
            //    AxisAlignT.EmgStop();
            //    PostAlarm((int)AlarmKeys.eAlignTAxesMoving);
            //    return -1;
            //}

            return nRet;
        }
        public Task<int> MovePositionAsyncSafeSafetyZ(bool isFine = false, CancellationToken ct = default(CancellationToken))
        {
            return Task.Run(() =>
            {
                // Task로 돌리고 별도 인터락/취소 감시
                var coreTask = Task.Run(() => OnMovePositionSafetyZ(isFine), ct);

                while (!IsEndTask(coreTask))
                {
                    if (ct.IsCancellationRequested)
                    {
                        try
                        {
                            AxisIndexZ?.EmgStop();
                            AxisAlignT?.EmgStop();
                        }
                        catch { }
                        return -999; // 취소 코드
                    }

                    int nRtn = IsMoveInterLockSafetyZ();
                    if (nRtn != 0)
                    {
                        return -1;
                    }

                    Thread.Sleep(1); // 0→5ms로 약간 여유 (CPU 점유 감소)
                }

                return coreTask.Result;
            }, ct);
        }

        public int MovePositionAlignZUp(int nIndex = 0, bool isFine = false)
        {
            Task<int> task = MovePositionAsyncAlignUp(nIndex, isFine);
            while (IsEndTask(task) == false)
            {
                int nRtn = IsMoveInterLockAlignUp(nIndex);
                if (nRtn != 0)
                {
                    return -1;
                }

                Thread.Sleep(1);
            }
            return task.Result;
        }
        private Task<int> MovePositionAsyncAlignUp(int nIndex = 0, bool isFine = false)
        {
            return Task.Run(() =>
            {
                OnMovePositionAlignUp(nIndex, isFine);
                return 0;
            });
        }
        private int OnMovePositionAlignUp(int nIndex = 0, bool isFine = false)
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
                Log.Write(UnitName, $"[OnMovePositionAlignUp_Index] Invalid index {nIndex}. Range 0~7 or 1~8");
                return -1;
            }

            string tpName = $"AlignZ_Index{teachingIdx}_Contact";
            var tpObj = GetTeachingPosition(tpName);
            if (tpObj == null)
            {
                Log.Write(UnitName, $"[OnMovePositionAlignUp_Index] Teaching not found: {tpName}");
                return -1;
            }

            double dZPos = GetTP(tpName, AxisNames.IndexZ);
            nRet = OnMoveAxisPositionOne(AxisIndexZ, dZPos);
            if (nRet != 0)
            {
                Log.Write(UnitName, $"[OnMovePositionPlace_Index] PlaceZ move failed tp={tpName} pos={dZPos}");
                return -1;
            }

            while (IsAlignZIndexUp() == false)
            {
                if (IsStop)
                {
                    return 0;
                }
                Thread.Sleep(1);
            }

            return nRet;
        }

        private int IsMoveInterLockAlignUp(int nIndex = 0)
        {
            int nRet = 0;

            if (Rotary != null)
            {
                if (!IsRotaryReadyStable(3, 5)) //if (Rotary.IsIndexReadyForAction(out string reason) == false)
                {
                    // 정말로 계속 움직이고 있다면, 위치가 틀어졌을 가능성이 높음 -> 정지
                    AxisIndexZ.EmgStop();
                    AxisAlignT.EmgStop();
                    PostAlarm((int)AlarmKeys.eRotaryAxesMoving);
                    Log.Write(UnitName, nameof(IsMoveInterLockAlignUp), $"Rotary moving interlock. {Rotary.GetIndexMovingDebugText()}");
                    return -1;
                }
            }

            return nRet;
        }

        private bool IsRotaryReadyStable(int checkCount = 3, int pollMs = 5)
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

        public Task<int> MovePositionAsyncSafeAlignUp(int nIndex = 0, bool isFine = false, CancellationToken ct = default(CancellationToken))
        {
            return Task.Run(() =>
            {
                // Task로 돌리고 별도 인터락/취소 감시
                var coreTask = Task.Run(() => OnMovePositionAlignUp(nIndex, isFine), ct);

                while (!IsEndTask(coreTask))
                {
                    if (ct.IsCancellationRequested)
                    {
                        try
                        {
                            AxisIndexZ?.EmgStop();
                            AxisAlignT?.EmgStop();
                        }
                        catch { }
                        return -999; // 취소 코드
                    }

                    int nRtn = IsMoveInterLockSafetyZ();
                    if (nRtn != 0)
                    {
                        return -1;
                    }

                    Thread.Sleep(1); // 0→5ms로 약간 여유 (CPU 점유 감소)
                }

                return coreTask.Result;
            }, ct);
        }


        public int MovePositionAlignZReady(int nIndex = 0, bool isFine = false)
        {
            Task<int> task = MovePositionAsyncAlignZReady(nIndex, isFine);
            while (IsEndTask(task) == false)
            {
                int nRtn = IsMoveInterLockAlignZReady(nIndex);
                if (nRtn != 0)
                {
                    return -1;
                }

                Thread.Sleep(1);
            }
            return task.Result;
        }
        private Task<int> MovePositionAsyncAlignZReady(int nIndex = 0, bool isFine = false)
        {
            return Task.Run(() =>
            {
                OnMovePositionAlignZReady(nIndex, isFine);
                return 0;
            });
        }
        private int OnMovePositionAlignZReady(int nIndex = 0, bool isFine = false)
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
                Log.Write(UnitName, $"[OnMovePositionAlignUp_Index] Invalid index {nIndex}. Range 0~7 or 1~8");
                return -1;
            }

            string tpName = $"AlignZ_Index{teachingIdx}_Ready";
            var tpObj = GetTeachingPosition(tpName);
            if (tpObj == null)
            {
                Log.Write(UnitName, $"[OnMovePositionAlignUp_Index] Teaching not found: {tpName}");
                return -1;
            }

            double dZPos = GetTP(tpName, AxisNames.IndexZ);
            nRet = OnMoveAxisPositionOne(AxisIndexZ, dZPos);
            if (nRet != 0)
            {
                Log.Write(UnitName, $"[OnMovePositionPlace_Index] PlaceZ move failed tp={tpName} pos={dZPos}");
                return -1;
            }
            
            return nRet;
        }
        private int IsMoveInterLockAlignZReady(int nIndex = 0)
        {
            int nRet = 0;

            if (Rotary != null && !IsRotaryReadyStable(3, 5)) //if (Rotary != null && Rotary.IsIndexReadyForAction(out string reason) == false)
            {
                AxisIndexZ.EmgStop();
                AxisAlignT.EmgStop();
                PostAlarm((int)AlarmKeys.eRotaryAxesMoving);
                Log.Write(UnitName, nameof(IsMoveInterLockAlignZReady), $"Rotary moving interlock. {Rotary.GetIndexMovingDebugText()}");
                return -1;
            }
            return nRet;
        }
        public Task<int> MovePositionAsyncSafeAlignZReady(int nIndex = 0, bool isFine = false, CancellationToken ct = default(CancellationToken))
        {
            return Task.Run(() =>
            {
                // Task로 돌리고 별도 인터락/취소 감시
                var coreTask = Task.Run(() => OnMovePositionAlignZReady(nIndex, isFine), ct);

                while (!IsEndTask(coreTask))
                {
                    if (ct.IsCancellationRequested)
                    {
                        try
                        {
                            AxisIndexZ?.EmgStop();
                            AxisAlignT?.EmgStop();
                        }
                        catch { }
                        return -999; // 취소 코드
                    }

                    int nRtn = IsMoveInterLockSafetyZ();
                    if (nRtn != 0)
                    {
                        return -1;
                    }

                    Thread.Sleep(1); // 0→5ms로 약간 여유 (CPU 점유 감소)
                }

                return coreTask.Result;
            }, ct);
        }

        // === AlignT_Foward ===
        public int MovePositionAlignTForward(bool isFine = false)
        {
            Task<int> task = MovePositionAsyncAlignTForward(isFine);
            while (IsEndTask(task) == false)
            {
                int nRtn = IsMoveInterLockAlignTForward();
                if (nRtn != 0)
                {
                    return -1;
                }
                Thread.Sleep(1);
            }
            return task.Result;
        }
        private Task<int> MovePositionAsyncAlignTForward(bool isFine = false)
        {
            return Task.Run(() =>
            {
                OnMovePositionAlignTForward(isFine);
                return 0;
            });
        }
        private int OnMovePositionAlignTForward(bool isFine = false)
        {
            int nRet = 0;
            nRet = MoveTeachingPositionOnce((int)IndexLoadAlignerRecipe.TeachingPositionName.AlignT_Foward, isFine);
            if(nRet != 0)
            {
                return -1;
            }
            while (IsAlignTForward() == false)
            {
                if(IsStop)
                {
                    return 0;
                }
                Thread.Sleep(1);
            }
            return nRet;
        }
        private int IsMoveInterLockAlignTForward()
        {
            int nRet = 0;
            if (Rotary != null && !IsRotaryReadyStable(3, 5)) //if (Rotary != null && Rotary.IsIndexReadyForAction(out string reason) == false)
            {
                AxisIndexZ?.EmgStop();
                AxisAlignT?.EmgStop();
                PostAlarm((int)AlarmKeys.eRotaryAxesMoving);
                Log.Write(UnitName, nameof(IsMoveInterLockAlignTForward), $"Rotary moving interlock. {Rotary.GetIndexMovingDebugText()}");
                return -1;
            }
            return nRet;
        }
        public Task<int> MovePositionAsyncSafeAlignTForward(bool isFine = false, CancellationToken ct = default(CancellationToken))
        {
            return Task.Run(() =>
            {
                var coreTask = Task.Run(() => OnMovePositionAlignTForward(isFine), ct);
                while (!IsEndTask(coreTask))
                {
                    if (ct.IsCancellationRequested)
                    {
                        try { AxisIndexZ?.EmgStop(); AxisAlignT?.EmgStop(); } catch { }
                        return -999; // 취소 코드
                    }
                    int nRtn = IsMoveInterLockAlignTForward();
                    if (nRtn != 0)
                    {
                        return -1;
                    }
                    Thread.Sleep(1);
                }
                return coreTask.Result;
            }, ct);
        }

        // === AlignT_Backward ===
        public int MovePositionAlignTBackward(bool isFine = false)
        {
            Task<int> task = MovePositionAsyncAlignTBackward(isFine);
            while (IsEndTask(task) == false)
            {
                int nRtn = IsMoveInterLockAlignTBackward();
                if (nRtn != 0)
                {
                    return -1;
                }
                Thread.Sleep(1);
            }
            return task.Result;
        }
        private Task<int> MovePositionAsyncAlignTBackward(bool isFine = false)
        {
            return Task.Run(() =>
            {
                return OnMovePositionAlignTBackward(isFine);
            });
        }
        private int OnMovePositionAlignTBackward(bool isFine = false)
        {
            int nRet = 0;
            nRet = MoveTeachingPositionOnce((int)IndexLoadAlignerRecipe.TeachingPositionName.AlignT_Backward, isFine);
            if (nRet != 0)
            {
                return -1;
            }
            while (IsAlignTBackward() == false)
            {
                if (IsStop)
                    return 0;

                Thread.Sleep(1);
            }
            return nRet;
            //return MoveTeachingPositionOnce((int)IndexLoadAlignerConfig.TeachingPositionName.AlignT_Backward, isFine);
        }
        private int IsMoveInterLockAlignTBackward()
        {
            int nRet = 0;
            if (Rotary != null && !IsRotaryReadyStable(3, 5)) //if (Rotary != null && Rotary.IsIndexReadyForAction(out string reason) == false)
            {
                Log.Write(UnitName, nameof(IsMoveInterLockAlignTBackward),
                    $"Rotary moving interlock. {Rotary.GetIndexMovingDebugText()}");

                AxisIndexZ?.EmgStop();
                AxisAlignT?.EmgStop();
                PostAlarm((int)AlarmKeys.eRotaryAxesMoving);

                return -1;
            }
            return nRet;
        }
        public Task<int> MovePositionAsyncSafeAlignTBackward(bool isFine = false, CancellationToken ct = default(CancellationToken))
        {
            return Task.Run(() =>
            {
                var coreTask = Task.Run(() => OnMovePositionAlignTBackward(isFine), ct);
                while (!IsEndTask(coreTask))
                {
                    if (ct.IsCancellationRequested)
                    {
                        try { AxisIndexZ?.EmgStop(); AxisAlignT?.EmgStop(); } catch { }
                        return -999; // 취소 코드
                    }
                    int nRtn = IsMoveInterLockAlignTBackward();
                    if (nRtn != 0)
                    {
                        return -1;
                    }
                    Thread.Sleep(1);
                }
                return coreTask.Result;
            }, ct);
        }


        // === AlignT_Ready ===
        public int MovePositionAlignTReady(bool isFine = false)
        {
            string readyName = IndexLoadAlignerRecipe.TeachingPositionName.AlignT_Ready.ToString();
            if (InPosTeaching(readyName))
            {
                return 0;
            }
                
            Task<int> task = MovePositionAsyncAlignTReady(isFine);
            while (IsEndTask(task) == false)
            {
                int nRtn = IsMoveInterLockAlignTReady();
                if (nRtn != 0)
                {
                    return -1;
                }
                Thread.Sleep(1);
            }
            return task.Result;
        }
        private Task<int> MovePositionAsyncAlignTReady(bool isFine = false)
        {
            return Task.Run(() =>
            {
                OnMovePositionAlignTReady(isFine);
                return 0;
            });
        }
        private int OnMovePositionAlignTReady(bool isFine = false)
        {
            return MoveTeachingPositionOnce((int)IndexLoadAlignerRecipe.TeachingPositionName.AlignT_Ready, isFine);
        }
        private int IsMoveInterLockAlignTReady()
        {
            int nRet = 0;
            if (Rotary != null && !IsRotaryReadyStable(3, 5)) //if (Rotary != null && Rotary.IsIndexReadyForAction(out string reason) == false)
            {
                Log.Write(UnitName, nameof(IsMoveInterLockAlignTReady), $"Rotary moving interlock. {Rotary.GetIndexMovingDebugText()}");
                AxisIndexZ?.EmgStop();
                AxisAlignT?.EmgStop();
                PostAlarm((int)AlarmKeys.eRotaryAxesMoving);
                return -1;
            }
            return nRet;
        }
        public Task<int> MovePositionAsyncSafeAlignTReady(bool isFine = false, CancellationToken ct = default(CancellationToken))
        {
            return Task.Run(() =>
            {
                var coreTask = Task.Run(() => OnMovePositionAlignTReady(isFine), ct);
                while (!IsEndTask(coreTask))
                {
                    if (ct.IsCancellationRequested)
                    {
                        try { AxisIndexZ?.EmgStop(); AxisAlignT?.EmgStop(); } catch { }
                        return -999; // 취소 코드
                    }
                    int nRtn = IsMoveInterLockAlignTReady();
                    if (nRtn != 0)
                    {
                        return -1;
                    }
                    Thread.Sleep(1);
                }
                return coreTask.Result;
            }, ct);
        }


        // ===== 위치 확인 (TeachingPosition 기준) =====
        // AlignT 위치 확인
        public bool IsAlignTReady()
        {
            var name = IndexLoadAlignerRecipe.TeachingPositionName.AlignT_Ready.ToString();
            return InPosTeaching(name);
        }

        public bool IsAlignTForward()
        {
            var name = IndexLoadAlignerRecipe.TeachingPositionName.AlignT_Foward.ToString();
            return InPosTeaching(name);
        }

        public bool IsAlignTBackward()
        {
            var name = IndexLoadAlignerRecipe.TeachingPositionName.AlignT_Backward.ToString();
            return InPosTeaching(name);
        }

        // AlignZ(IndexZ) 위치 확인: 현재 인덱스 버전
        public bool IsAlignZIndexUp()
        {
            int nIndex = GetAlignIndexNo();
            return IsAlignZIndexUp(nIndex);
        }

        public bool IsAlignZIndexReady()
        {
            int nIndex = GetAlignIndexNo();
            return IsAlignZIndexReady(nIndex);
        }

        // AlignZ(IndexZ) 위치 확인: 특정 인덱스(0~7 또는 1~8 허용)
        public bool IsAlignZIndexUp(int nIndex)
        {
            // 기존 이동 로직과 동일한 인덱스 보정 규칙 유지
            // nIndex 처리
            int teachingIdx = 0;
            if (nIndex >= 0 && nIndex < 8)
            {
                teachingIdx = nIndex + 1;
            }
            else
                return false;

            string tpName = $"AlignZ_Index{teachingIdx}_Contact";

            // Z축만 판정 (축별 확인)
            if (AxisIndexZ == null) return true;
            var tp = GetTeachingPosition(tpName);
            if (tp == null) return false;

            double target = GetTP(tpName, AxisNames.IndexZ);
            try { return AxisIndexZ.InPosition(target); } catch { return false; }
        }

        public bool IsAlignZIndexReady(int nIndex)
        {
            // nIndex 처리
            int teachingIdx = 0;
            if (nIndex >= 0 && nIndex < 8)
            {
                teachingIdx = nIndex + 1;
            }
            else
                return false;

            string tpName = $"AlignZ_Index{teachingIdx}_Ready";
            // Z축만 판정 (축별 확인)
            if (AxisIndexZ == null) 
                return true;

            var tp = GetTeachingPosition(tpName);
            if (tp == null) 
                return false;

            double target = GetTP(tpName, AxisNames.IndexZ);
            try { return AxisIndexZ.InPosition(target); } catch { return false; }
        }

        public bool IsPositionAlignZSafety()
        {
            const string tpName = nameof(IndexLoadAlignerRecipe.TeachingPositionName.SafetyZone);
            if (AxisIndexZ == null)
                return true;

            // 현재 실제 위치 읽기
            double currentPos;
            try
            {
                currentPos = AxisIndexZ.GetPosition();
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }
            // 요구사항: 실제 위치가 0(또는 매우 근접) 이면 Safety 로 간주
            // 허용 오차는 장비 정밀도에 따라 조정(예: 0.005 이하)
            const double zeroTolerance = 0.001;
            if (Math.Abs(currentPos) <= zeroTolerance)
            {
                return true;
            }

            double target = GetTP(tpName, AxisNames.IndexZ);
            try
            {
                return AxisIndexZ.InPosition(target);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }
        }
        #endregion

        #region Teaching
        public void TeachCurrentPosition(string positionName, string description = null)
        {
            var axisPositions = new Dictionary<string, double>();
            foreach (var axisPair in Axes)
                axisPositions[axisPair.Key] = axisPair.Value.GetPosition();
            var tp = new TeachingPosition(positionName, axisPositions, description);
            Config.SetTeachingPosition(tp);
        }
        public int MoveToTeachingPosition(string positionName, bool isFine)
        {
            if (string.IsNullOrWhiteSpace(positionName))
            {
                Log.Write(UnitName, nameof(MoveToTeachingPosition),
                       $"[TeachingMove] '{positionName}' not found in TeachingPositions."); 
                return -1;
            }

            int result = 0;

            IndexLoadAlignerRecipe.TeachingPositionName en;
            if (Enum.TryParse(positionName, out en))
            {
                int selIndex = FindTeachingSelectionIndex(positionName);
                if (selIndex >= 0)
                {
                    result = MoveToTeachingPositionBySelectionIndex(selIndex, isFine);
                }
                else
                {
                    Log.Write(UnitName, nameof(MoveToTeachingPosition),
                        $"[TeachingMove] Index for '{positionName}' not found in TeachingPositions.");
                    return -1;
                }
            }

            return result;
        }
        #endregion

        #region IO Placeholders
        public bool ReadInput(string name) => false; // No IO defined yet
        public bool WriteOutput(string name, bool on) => false; // No IO defined yet
        #endregion


        #region seq signal
        public bool CompleteLoadAligner { get; set; } = false;
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
            // 1) Recipe 보장 (실패 체크 권장)
            bool loaded = false;
            try
            {
                loaded = PmRunner.LoadRecipe();
            }
            catch (Exception ex)
            {
                Log.Write(UnitName, "AlignXY", "LoadRecipe exception: " + ex.Message);
            }
            if (!loaded)
            {
                Log.Write(UnitName, "AlignXY", "Fail: LoadRecipe returned false");
                // 필요 시 알람:
                // PostAlarm((int)AlarmKeys.eVisionSearch);
                return -1;
            }

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

        protected override void OnMakeSequence()
        {
            base.OnMakeSequence();
            this.SequencePlayers.Add(RunAlignSocketOnceReady);
            this.SequencePlayers.Add(RunAlignSocketOnce);
        }

        #region Seq 단위 동작 함수
        public int RunAlignSocketOnceReady(bool bFineSpeed = false)
        {
            int bRtn = 0;
            try
            {
                this.CurrentFunc = RunAlignSocketOnceReady;
                LogSequence("Start");
                                                                  
                while (!IsRotaryReadyStable(3, 5))  //while (this.Rotary.IsIndexReadyForAction(out string reason) == false)
                {
                    if (IsStop)
                    {
                        return 0;
                    }
                    Thread.Sleep(1);
                }

                int nIndex = GetAlignIndexNo();
                bRtn = MovePositionAlignTReady(bFineSpeed);
                if (bRtn != 0)
                {
                    Log.Write(UnitName, "RunAlignSocketOnceReady", "Fail: MovePositionAlignTReady");
                    return -1;
                }

                bRtn = MovePositionSafetyZ(bFineSpeed);
                if (bRtn != 0)
                {
                    Log.Write(UnitName, "RunAlignSocketOnceReady", "Fail: MovePositionAlignZReady");
                    return -1;
                }
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

        /// <summary>
        /// 소켓 1개에 대한 정렬 묶음
        /// 순서: Z Up(해당 소켓) -> T Forward -> T Backward -> T Ready
        /// 모든 축 이동은 안전 Async 버전 사용(내부 폴링).
        /// </summary>
        private int _mAlignRunFlag = 0;
        private long _mAlignRunSeq = 0;
        public int RunAlignSocketOnce(bool bFineSpeed = false)
        {
            int nRet = 0;
            this.CurrentFunc = RunAlignSocketOnce;
            
            LogSequence("Start");
            int nIndex = GetAlignIndexNo();
            try
            {
                long seq = Interlocked.Increment(ref _mAlignRunSeq);
                if (Interlocked.Exchange(ref _mAlignRunFlag, 1) == 1)
                {
                    Log.Write(UnitName, "RunAlignSocketOnce",
                        $"[MAlign#{seq}] BLOCKED: duplicated call. Thread={Thread.CurrentThread.ManagedThreadId}, Name={Thread.CurrentThread.Name}");
                    return 0;
                }

                bool bUseSocket = this.Rotary.Config.GetUseSocket(nIndex);
                var socket = this.Rotary.GetSocket(nIndex);
                MaterialDie die = this.Rotary.GetMAlignSocketMaterial();

                Log.Write(UnitName, "MAlign",
                    $"[MAlign#{seq}] ENTER. " +
                    $"Thread={Thread.CurrentThread.ManagedThreadId}, Name={Thread.CurrentThread.Name}, " +
                    $"LoadIndex={Rotary.GetLoadIndexNo()}, AlignIndex={nIndex}, " +
                    $"UseSocket={bUseSocket}, " +
                    $"SocketState={socket?.State}, " +
                    $"DieNull={(die == null)}, " +
                    $"Presence={die?.Presence}, DieState={die?.State}, ProcState={die?.ProcessSatate}, " +
                    $"RotaryMoveDone={Rotary.IsIndexReadyForAction(out string reason)}");

                if (bUseSocket == false)
                {
                    Log.Write(UnitName, "MAlign", $"[MAlign#{seq}] Skip: No socket. AlignIndex={nIndex}");
                    return 0;
                }
                if (die == null)
                {
                    Log.Write(UnitName, "MAlign", $"[MAlign#{seq}] Skip: No die. AlignIndex={nIndex}");
                    return 0;
                }
                if (die.Presence != Material.MaterialPresence.Exist)
                {
                    Log.Write(UnitName, "MAlign",
                        $"[MAlign#{seq}] Skip: No die presence. AlignIndex={nIndex}, Presence={die.Presence}");
                    return 0;
                }

                if (this.RunUnitStatus != UnitStatus.ManualRunning)
                {
                    // [추가] 상태 기반 Skip (이미 NG/결과완료이면 재측정 안 함)
                    if (die.State == DieProcessState.Rejected ||
                        die.State == DieProcessState.Skip ||
                        die.ProcessSatate == Material.MaterialProcessSatate.Skipped)
                    {
                        Log.Write(UnitName, "MAlign",
                            $"Skip re-align: DieState={die.State}, ProcessState={die.ProcessSatate}");
                        return 0;
                    }
                }

                while (!IsRotaryReadyStable(3, 5)) //while (this.Rotary.IsIndexReadyForAction(out reason) == false)
                {
                    if (IsStop)
                    {
                        Log.Write(UnitName, "MAlign", "IsStop True");
                        return 0;
                    }
                    Thread.Sleep(1);
                }

                TaktStart("One Cycle");
                socket.SetState(Rotary.RotarySocketState.MAligning);
                // === 동작 수행 (Z Up -> T Fwd -> T Bwd -> T Ready -> Z Safe) ===
                // 2) T Ready // tact Time 모자라면 비동기 처리 할것.
                TaktStart("AlignTReady");
                if (IsAlignTReady() == false)
                {
                    nRet = MovePositionAlignTReady(bFineSpeed);
                    if (nRet != 0)
                    {
                        Log.Write(UnitName, "MAlign", "Fail: MovePositionAlignTReady/MovePositionAlignZReady");
                        return -1;
                    }
                }
                TaktEnd("AlignTReady");

                // 3) Z Up
                TaktStart("AlignZUp");
                nRet = MovePositionAlignZUp(nIndex, bFineSpeed);
                if (nRet != 0)
                {
                    Log.Write(UnitName, "MAlign", "Fail: MovePositionAlignUp");
                    return -1;
                }
                TaktEnd("AlignZUp");

                //Place가 제대로 안되서 여기에서 검사하고 얼라인했지만
                //칩이 틸트가 져서 밝기 차이로 검출 실패가 많음.
                //알고리즘 개선 안하면 사용 못함
                if(false)
                {
                    // Vision Align 검사 추가.
                    TaktStart("AlignXY_Vision");
                    nRet = AlignXY(bFineSpeed);
                    TaktEnd("AlignXY_Vision");
                    if (nRet != 0) //NG면
                    {
                        TaktStart("SafetyZ");
                        Log.Write(UnitName, "MAlign", "Fail: AlignXY");
                        try
                        {
                            var ctx = Equipment.Instance.SummaryContext;
                            ctx.GetCurrentSummaryOrNull()?.AddAlignVisionAsMiss();
                        }
                        catch (Exception ex)
                        { Log.Write(ex); }

                        if (false)
                        {
                            nRet = MoveSafeAndReadyAndWait(bFineSpeed);
                            if (nRet != 0)
                            {
                                return -1;
                            }
                        }
                        else
                        {
                            nRet = MovePositionAlignTReady(bFineSpeed);
                            if (nRet != 0)
                            {
                                Log.Write(UnitName, "MAlign", "Fail: MovePositionAlignTReady");
                                nRet = -1;
                            }
                            nRet = MovePositionSafetyZ(bFineSpeed);
                            if (nRet != 0)
                            {
                                Log.Write(UnitName, "MAlign", "Fail: MovePositionSafetyZ");
                                return -1;
                            }
                        }

                        CompleteLoadAligner = true;
                        die.Presence = Material.MaterialPresence.Exist;
                        die.ProcessSatate = Material.MaterialProcessSatate.Skipped;
                        die.State = DieProcessState.Skip;
                        socket.SetState(Rotary.RotarySocketState.Error);
                        LogSequence("End");
                        TaktEnd("SafetyZ");
                        TaktEnd("One Cycle");

                        return 0;
                    }
                }

                // 4) T Forward
                TaktStart("AlignTForward");
                nRet = MovePositionAlignTForward(bFineSpeed);
                if (nRet != 0)
                {
                    Log.Write(UnitName, "MAlign", "Fail: MovePositionAlignTForward1");
                    return -1;
                }
                TaktEnd("AlignTForward");

                TaktStart("WaitTime1Step");
                WaitByTime(Config.WaitTime1Step);
                TaktEnd("WaitTime1Step");

                // 5) T Backward
                TaktStart("AlignTBackward");
                nRet = MovePositionAlignTBackward(bFineSpeed);
                if (nRet != 0)
                {
                    Log.Write(UnitName, "MAlign", "Fail: MovePositionAlignTBackward");
                    return -1;
                }
                TaktEnd("AlignTBackward");

                TaktStart("WaitTime2Step");
                WaitByTime(Config.WaitTime2Step);
                TaktEnd("WaitTime2Step");

                //nRet = MovePositionAlignTForward(bFineSpeed);
                //if (nRet != 0)
                //{
                //    Log.Write(UnitName, "MAlign", "Fail: MovePositionAlignTForward2");
                //    return -1;
                //}
                //WaitByTime(Config.WaitTime3Step);

                //Ready Skip
                TaktStart("AlignTReady2");
                nRet = MovePositionAlignTReady(bFineSpeed);
                if (nRet != 0)
                {
                    Log.Write(UnitName, "MAlign", "Fail: MovePositionAlignTReady");
                    return -1;
                }
                TaktEnd("AlignTReady2");

                //20260507 GYN - 정상적일때 주석 해제.
                //// Vision Align 검사 추가.
                //if (true)
                //{
                //    TaktStart("AlignXY_Vision");
                //    nRet = AlignXY(bFineSpeed);
                //    TaktEnd("AlignXY_Vision");
                //}

                //TaktStart("SafetyZ");
                //if (nRet != 0)
                //{
                //    try
                //    {
                //        var ctx = Equipment.Instance.SummaryContext;
                //        ctx.GetCurrentSummaryOrNull()?.AddAlignVisionAsMiss();
                //    }
                //    catch (Exception ex)
                //    { Log.Write(ex); }

                //    if(false)
                //    {
                //        nRet = MoveSafeAndReadyAndWait(bFineSpeed);
                //        if (nRet != 0)
                //        {
                //            return -1;
                //        }
                //    }
                //    else
                //    {
                //        nRet = MovePositionAlignTReady(bFineSpeed);
                //        if (nRet != 0)
                //        {
                //            Log.Write(UnitName, "MAlign", "Fail: MovePositionAlignTReady");
                //            nRet = -1;
                //        }
                //        nRet = MovePositionSafetyZ(bFineSpeed);
                //        if (nRet != 0)
                //        {
                //            Log.Write(UnitName, "MAlign", "Fail: MovePositionSafetyZ");
                //            return -1;
                //        }
                //    }

                //    CompleteLoadAligner = true;
                //    die.Presence = Material.MaterialPresence.Exist;
                //    die.ProcessSatate = Material.MaterialProcessSatate.Skipped;
                //    die.State = DieProcessState.Skip;
                //    socket.SetState(Rotary.RotarySocketState.Error);
                //    LogSequence("End");
                //}
                //else
                {
                    if (false)
                    {
                        nRet = MoveSafeAndReadyAndWait(bFineSpeed);
                        if (nRet != 0)
                        {
                            return -1;
                        }
                    }
                    else
                    {
                        nRet = MovePositionAlignTReady(bFineSpeed);
                        if (nRet != 0)
                        {
                            Log.Write(UnitName, "MAlign", "Fail: MovePositionAlignTReady");
                            nRet = -1;
                        }
                        nRet = MovePositionSafetyZ(bFineSpeed);
                        if (nRet != 0)
                        {
                            Log.Write(UnitName, "MAlign", "Fail: MovePositionSafetyZ");
                            return -1;
                        }
                    }


                    die.State = DieProcessState.Inspecting;
                    CompleteLoadAligner = true;
                    socket.SetState(Rotary.RotarySocketState.MAligned);
                    LogSequence("End");
                }
                TaktEnd("SafetyZ");
                TaktEnd("One Cycle");
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return -1;
            }
            finally
            {
                if (IsAlignTReady() == false)
                {
                    nRet = MovePositionAlignTReady(bFineSpeed);
                }
                if (IsPositionAlignZSafety() == false)
                {
                    nRet += MovePositionSafetyZ(bFineSpeed);
                }
                if (nRet != 0)
                {
                    Log.Write(UnitName, "MAlign", "Fail: MovePositionSafetyZ");
                    nRet = -1;
                }
                CompleteLoadAligner = true;

                // 중요: 재진입 플래그 해제
                Interlocked.Exchange(ref _mAlignRunFlag, 0);
            }

            return nRet;
        }

        private int MoveSafeAndReadyAndWait(bool bFineSpeed)
        {
            Task<int> tz = MovePositionAsyncSafeSafetyZ(bFineSpeed);
            Task<int> tt = MovePositionAsyncSafeAlignTReady(bFineSpeed);

            Task.WaitAll(tz, tt);

            if (tz.Result != 0 || tt.Result != 0)
            {
                Log.Write(UnitName, "MAlign", $"Fail: Move async result tz={tz.Result}, tt={tt.Result}");
                return -1;
            }

            const int timeoutMs = 5000;
            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                if (IsStop)
                {
                    return 0;
                }

                if (IsPositionAlignZSafety() && IsAlignTReady())
                {
                    return 0;
                }
                Thread.Sleep(1);
            }

            Log.Write(UnitName, "MAlign", "Fail: timeout waiting SafetyZ/AlignTReady in-position");
            return -1;
        }


        private void LogSequence(string log)
        {
                if (this.CurrentFunc == null)
                    return;

                Log.Write(UnitName, this.CurrentFunc.Method.Name, $"[Sequence] {log}");
        }

        public int GetAlignIndexNo()
        {
            if (Rotary == null)
                return 0;

            int loadIndex = Rotary.GetLoadIndexNo();

            // 반시계 방향으로 1칸 이동
            int probeIndex = (loadIndex - this.Config.IndexOfMAlign + Rotary.GetIndexCount()) % Rotary.GetIndexCount();

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

            if (IsPositionAlignZSafety() == false)
            {
                nRet = MovePositionSafetyZ();
                if (nRet != 0)
                {
                    Log.Write(this, "CheckReady Fail - MovePositionSafetyZ");
                    return nRet;
                }
            }

            if(IsAlignTReady() == false)
            {
                nRet = MovePositionAlignTReady(isFine);
                if (nRet != 0)
                {
                    Log.Write(this, "CheckReady Fail - MovePositionAlignTReady");
                    return nRet;
                }
            }

            return nRet;
        }
        #endregion

        // 클래스 내부에 추가
        public void ResetForNewRun(bool moveToSafeReady = true)
        {
            // 1) 런타임/시퀀스 플래그 초기화
            _isSafetyMoving = false;
            CompleteLoadAligner = false;
            this.CurrentFunc = null;

            Interlocked.Exchange(ref _mAlignRunFlag, 0);

            // 2) 안전 위치 복귀(선택)
            if (moveToSafeReady)
            {
                try
                {
                    while (!IsRotaryReadyStable(3, 5)) //while (this.Rotary.IsIndexReadyForAction(out string reason) == false)
                    {
                        if (IsStop)
                        {
                            return;
                        }
                        Thread.Sleep(1);
                    }
                    EnsureReady(); // IndexZ: SafetyZone, AlignT: Ready
                }
                catch (Exception ex)
                {
                    Log.Write(UnitName, $"[ResetForNewRun] EnsureReady failed: {ex.Message}");
                }
            }
        }


        public bool IsStatus_AlignDoneXY { get; set; }
        public bool IsAlignResult { get; set; }
        public double dLastFoundX { get; set; }
        public double dLastFoundY { get; set; }
        public double dLastFoundAngle { get; private set; }

        public int AlignXY(bool bFineSpeed = false)
        {
            IsStatus_AlignDoneXY = false;
            IsAlignResult = false;
            dLastFoundX = 0.0;
            dLastFoundY = 0.0;
            dLastFoundAngle = 0.0;

            MaterialDie die = this.Rotary.GetMAlignSocketMaterial();
            if (die == null || die.Presence != Material.MaterialPresence.Exist)
            {
                Log.Write(UnitName, "Align", "Skip: No die on unload socket");
                IsStatus_AlignDoneXY = true;
                return 0;
            }

            var equipment = Equipment.Instance;
            bool IsDryRunEqp = equipment.EquipmentConfig.IsDryRun;
            if (Config.IsSimulation || (this.Config.IsDryRun || IsDryRunEqp))
            {
                // ========== [수정 시작] Random Failure Simulation ==========
                // 0 ~ 99 사이의 난수 생성
                // 예: 80보다 작으면 성공(80%), 크면 실패(20%)로 설정
                bool isSimulatedSuccess = new Random().Next(0, 99) < 99;
                if (isSimulatedSuccess)
                {
                    IsAlignResult = true;       // Align 성공
                    IsStatus_AlignDoneXY = true;
                    Log.Write(UnitName, "Simulation", "Align Simulated: SUCCESS");
                    return 0;
                }
                else
                {
                    IsAlignResult = false;      // Align 실패 (Reject 처리 유도)
                    IsStatus_AlignDoneXY = true; // 동작 자체는 완료됨

                    // 실패 시, 현재 제품 상태를 Reject로 마킹해야 시퀀스가 정상 흐름을 탑니다.
                    // (실제 비전 로직 실패 시에도 아래와 유사하게 처리됩니다)
                    var loaddie = Rotary.GetLoadSocketMaterial();
                    if (loaddie != null)
                    {
                        loaddie.SetReject("Align Fail (Simulated)");
                        // die.State = DieProcessState.Rejected; // SetReject 내부에서 처리됨
                    }

                    Log.Write(UnitName, "Simulation", "Align Simulated: FAIL (Random)");
                    return -1; // 시퀀스 동작 자체는 에러(return -1)가 아니라 0(정상종료) 후 결과값(Result)으로 판별
                }
                // ========== [수정 끝] ==========
            }

            if (IndexAlignerCam == null)
            {
                Log.Write(UnitName, "AlignXY", "Fail: IndexAlignerCam null");
                IsStatus_AlignDoneXY = true;
                return -1;
            }

            try
            {
                if (IndexAlignerCam.IsLiveOn)
                {
                    IndexAlignerCam.StopLive();
                    Thread.Sleep(50);
                }
            }
            catch { }

            try
            {
                VisionImage img = null;
                IndexAlignerCam.SuspendedImageDisplay = true;
                IndexAlignerCam.GrabSync(out img);
                var result = PmRunner.Search(img);

                // 4) 결과 표시 (성공/실패 여부와 관계없이 호출하여 화면 갱신)
                var matches = (result != null && result.Matches != null)
                                ? result.Matches.ToArray()
                                : null;

                int repIdx = 0;
                if (result != null && result.Matches != null &&
                    result.ReferenceIndex >= 0 && result.ReferenceIndex < result.Matches.Count)
                {
                    repIdx = result.ReferenceIndex;
                }

                // 실패 시 matches가 null이므로 이미지만 갱신됨
                RaiseMarks(img, matches, repIdx);
                IndexAlignerCam.SuspendedImageDisplay = false;

                //if (result != null && result.Success && result.Matches != null && result.Matches.Count > 0)
                //{
                //    int repIdx = 2; // (result.ReferenceIndex >= 0 && result.ReferenceIndex < result.Matches.Count) ? result.ReferenceIndex : 0;
                //    RaiseMarks(img, result.Matches.ToArray(), repIdx);
                //    IndexAlignerCam.SuspendedImageDisplay = false;
                //}

                // 5) Offset 저장 (mm)
                if (result != null && result.Success)
                {
                    IsAlignResult = true;

                    var pt = GetPixelToMmScale(result.X, result.Y);
                    dLastFoundX = pt.X;
                    dLastFoundY = pt.Y;
                    dLastFoundAngle = result.R;

                    
                    // ==========================================================
                    // [추가됨] 실패 시 이미지 저장 (날짜시간_밀리초.bmp)
                    // ==========================================================
                    if(true)  //파라미터로 OK IMAGE 저장 유/무 설정하자.
                    {
                        try
                        {
                            // 1. 저장 경로 설정 (D:\Log\Image\{UnitName}\Fail)
                            string saveFolder = $@"D:\LCP-280\Log\Image\{UnitName}\OK";
                            // 2. 폴더 없으면 생성
                            if (!System.IO.Directory.Exists(saveFolder))
                            {
                                System.IO.Directory.CreateDirectory(saveFolder);
                            }

                            // 3. 파일명 생성 (년월일_시분초_밀리초)
                            string fileName = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + ".bmp";
                            string fullPath = System.IO.Path.Combine(saveFolder, fileName);

                            // 4. 저장 실행
                            if (img != null)
                            {
                                img.Save(fullPath, VisionImage.FileFilter.bmp);
                                Log.Write(UnitName, "AlignXY", $"Saved Fail Image: {fileName}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Write(UnitName, "AlignXY", $"Image Save Error: {ex.Message}");
                        }
                    }
                    // ==========================================================

                    Log.Write(UnitName, "AlignXY",
                        $"VisionX={dLastFoundX:F4}, VisionY={dLastFoundY:F4}, VisionAngle={dLastFoundAngle:F4}");
                }
                else
                {
                    IsAlignResult = false;
                    Log.Write(UnitName, "AlignXY",
                        $"Vision Search Fail. reason={(result != null ? result.FailReason : "result null")}");

                    // ==========================================================
                    // [추가됨] 실패 시 이미지 저장 (날짜시간_밀리초.bmp)
                    // ==========================================================
                    try
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
                        if (img != null)
                        {
                            img.Save(fullPath, VisionImage.FileFilter.bmp);
                            Log.Write(UnitName, "AlignXY", $"Saved Fail Image: {fileName}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Write(UnitName, "AlignXY", $"Image Save Error: {ex.Message}");
                    }
                    // ==========================================================

                    // 필요 시 알람:
                    // PostAlarm((int)AlarmKeys.eVisionSearch);
                    return -1; // ← 실패를 상위에서 감지하고 싶으면 -1 유지, "그냥 진행"이면 0으로 바꿔도 됨
                }

                return 0;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return -1;
            }
            finally
            {
                IsStatus_AlignDoneXY = true;
                IndexAlignerCam.SuspendedImageDisplay = false;
            }
        }

        PointD GetPixelToMmScale(double dX, double dY)
        {
            double mmPerPixelX = (dX - IndexAlignerCam.CameraConfig.Resolution.Width / 2) * IndexAlignerCam.CameraConfig.Scale.X;
            double mmPerPixelY = (dY - IndexAlignerCam.CameraConfig.Resolution.Height / 2) * IndexAlignerCam.CameraConfig.Scale.Y;
            return new PointD(mmPerPixelX, mmPerPixelY);
        }

        private void RaiseMarks(VisionImage img,
                            QMC.Common.Vision.Tools.PatternMatchingResult.PatternMatchingResultValue[] matches,
                            int representativeIndex)
        {
            int trainW = 0, trainH = 0;
            try
            {
                var ti = PmRunner?.Parameters?.TrainImages?
                         .FirstOrDefault(t => t?.Header != null && t.Header.Width > 0 && t.Header.Height > 0);
                if (ti != null) { trainW = ti.Header.Width; trainH = ti.Header.Height; }
            }
            catch { }

            var e = new PatternMarksFoundEventArgs
            {
                Image = img,
                RepresentativeIndex = representativeIndex
            };

            if(matches != null)
            {
                foreach (var m in matches)
                {
                    e.Marks.Add(new PatternMatchInfo
                    {
                        X = m.X,
                        Y = m.Y,
                        AngleDeg = m.R,
                        Score = m.Score,
                        TrainW = trainW,
                        TrainH = trainH
                    });
                }
            }
            else
            {
                e.Marks.Add(new PatternMatchInfo
                {
                    X =0,
                    Y = 0,
                    AngleDeg = 0,
                    Score = 0,
                    TrainW = trainW,
                    TrainH = trainH
                });
            }

            try { MarksFound?.Invoke(this, e); } catch { }
        }

        public int MoveToTeachingPositionBySelectionIndex(int teachingSelIndex, bool isFine = false)
        {
            if (Config == null)
                return -1;

            string tpName;
            if (!Config.GetTeachingPositionName(teachingSelIndex, out tpName) || string.IsNullOrWhiteSpace(tpName))
                return -1;

            IndexLoadAlignerRecipe.TeachingPositionName en;
            if (!Enum.TryParse(tpName, out en))
                return -1;

            int nIndex = -1;
            switch (en)
            {
                // ===== AlignZ Index Up/Ready (Index1~8 -> 0~7) =====
                case IndexLoadAlignerRecipe.TeachingPositionName.AlignZ_Index1_Contact: nIndex = 0; return MovePositionAlignZUp(nIndex, isFine);
                case IndexLoadAlignerRecipe.TeachingPositionName.AlignZ_Index1_Ready: nIndex = 0; return MovePositionAlignZReady(nIndex, isFine);

                case IndexLoadAlignerRecipe.TeachingPositionName.AlignZ_Index2_Contact: nIndex = 1; return MovePositionAlignZUp(nIndex, isFine);
                case IndexLoadAlignerRecipe.TeachingPositionName.AlignZ_Index2_Ready: nIndex = 1; return MovePositionAlignZReady(nIndex, isFine);

                case IndexLoadAlignerRecipe.TeachingPositionName.AlignZ_Index3_Contact: nIndex = 2; return MovePositionAlignZUp(nIndex, isFine);
                case IndexLoadAlignerRecipe.TeachingPositionName.AlignZ_Index3_Ready: nIndex = 2; return MovePositionAlignZReady(nIndex, isFine);

                case IndexLoadAlignerRecipe.TeachingPositionName.AlignZ_Index4_Contact: nIndex = 3; return MovePositionAlignZUp(nIndex, isFine);
                case IndexLoadAlignerRecipe.TeachingPositionName.AlignZ_Index4_Ready: nIndex = 3; return MovePositionAlignZReady(nIndex, isFine);

                case IndexLoadAlignerRecipe.TeachingPositionName.AlignZ_Index5_Contact: nIndex = 4; return MovePositionAlignZUp(nIndex, isFine);
                case IndexLoadAlignerRecipe.TeachingPositionName.AlignZ_Index5_Ready: nIndex = 4; return MovePositionAlignZReady(nIndex, isFine);

                case IndexLoadAlignerRecipe.TeachingPositionName.AlignZ_Index6_Contact: nIndex = 5; return MovePositionAlignZUp(nIndex, isFine);
                case IndexLoadAlignerRecipe.TeachingPositionName.AlignZ_Index6_Ready: nIndex = 5; return MovePositionAlignZReady(nIndex, isFine);

                case IndexLoadAlignerRecipe.TeachingPositionName.AlignZ_Index7_Contact: nIndex = 6; return MovePositionAlignZUp(nIndex, isFine);
                case IndexLoadAlignerRecipe.TeachingPositionName.AlignZ_Index7_Ready: nIndex = 6; return MovePositionAlignZReady(nIndex, isFine);

                case IndexLoadAlignerRecipe.TeachingPositionName.AlignZ_Index8_Contact: nIndex = 7; return MovePositionAlignZUp(nIndex, isFine);
                case IndexLoadAlignerRecipe.TeachingPositionName.AlignZ_Index8_Ready: nIndex = 7; return MovePositionAlignZReady(nIndex, isFine);

                // ===== AlignT =====
                case IndexLoadAlignerRecipe.TeachingPositionName.AlignT_Foward:
                    return MovePositionAlignTForward(isFine);

                case IndexLoadAlignerRecipe.TeachingPositionName.AlignT_Backward:
                    return MovePositionAlignTBackward(isFine);

                case IndexLoadAlignerRecipe.TeachingPositionName.AlignT_Ready:
                    return MovePositionAlignTReady(isFine);

                // ===== Safety =====
                case IndexLoadAlignerRecipe.TeachingPositionName.SafetyZone:
                    return MovePositionSafetyZ(isFine);

                default:
                    return -1;
            }
        }

        public override double GetTP(string tpName, string axisName)
        {
            try
            {
                var recipe = Config?.TeachingRecipe;
                var tp = recipe?.Get(tpName);
                if (tp != null && tp.AxisPositions != null && tp.AxisPositions.TryGetValue(axisName, out var v))
                    return v;

                // fallback: 기존 Config/BaseUnit
                return base.GetTP(tpName, axisName);
            }
            catch
            {
                return base.GetTP(tpName, axisName);
            }
        }

        private int FindTeachingSelectionIndex(string positionName)
        {
            try
            {
                var list = GetTeachingList();
                if (list == null)
                    return -1;

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

        private IList<TeachingPosition> GetTeachingList()
        {
            var r = Config?.TeachingRecipe;
            if (r?.TeachingPositions != null)
                return r.TeachingPositions;

            return Config?.TeachingPositions ?? new List<TeachingPosition>();
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

    }
}