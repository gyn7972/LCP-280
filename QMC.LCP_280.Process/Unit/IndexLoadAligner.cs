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

        public enum AlarmKeys
        {
            eAlignTAxesNotReady = 4701,
            eAlignTAxesMoving = 4702,
            eRotaryAxesMoving = 4703,
        }

        #region InitAlarm
        protected override void InitAlarm()
        {
            base.InitAlarm();
            AlarmInfo alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eAlignTAxesNotReady;
            alarm.Title = "IndexLoadAligner T-Axis Not ReadyPos.";
            alarm.Cause = "IndexLoadAligner T-Axis 가 준비 위치가 아닙니다.\n 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eAlignTAxesMoving;
            alarm.Title = "IndexLoadAligner T-Axis Axis Moving";
            alarm.Cause = "IndexLoadAligner T 축이 이동 중입니다. 정지 후 다시 시도하십시오.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eRotaryAxesMoving;
            alarm.Title = "Rotary Axis Moving";
            alarm.Cause = "Rotary 축이 이동 중입니다. 정지 후 다시 시도하십시오.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);
        }
        #endregion

        #region Config / Teaching
        public IndexLoadAlignerConfig IndexLoadAlignerConfig => Config;
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
                if (this.Rotary.IsIndexMoving())
                {
                    AxisIndexZ?.EmgStop();
                    PostAlarm((int)AlarmKeys.eRotaryAxesMoving);
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

            if (Rotary != null && this.Rotary.IsIndexMoving())
            {
                AxisIndexZ.EmgStop();
                AxisAlignT.EmgStop();
                PostAlarm((int)AlarmKeys.eRotaryAxesMoving);
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

                    Thread.Sleep(2); // 0→5ms로 약간 여유 (CPU 점유 감소)
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

            string tpName = $"AlignZ_Index{teachingIdx}_Up";
            var tpObj = IndexLoadAlignerConfig.GetTeachingPosition(tpName);
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

            //nRet = MoveTeachingPositionOnce((int)IndexLoadAlignerConfig.TeachingPositionName.AlignZ_Index1_Up, isFine);
            //if(nRet != 0)
            //{
            //    Log.Write(UnitName, $"[OnMovePositionAlignUp_Index] MoveTeachingPositionOnce failed: {tpName}");
            //    return -1;
            //}
            return nRet;
        }
        private int IsMoveInterLockAlignUp(int nIndex = 0)
        {
            int nRet = 0;

            if (Rotary != null && this.Rotary.IsIndexMoving())
            {
                AxisIndexZ.EmgStop();
                AxisAlignT.EmgStop();
                PostAlarm((int)AlarmKeys.eRotaryAxesMoving);
                return -1;
            }
            return nRet;
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

                    Thread.Sleep(2); // 0→5ms로 약간 여유 (CPU 점유 감소)
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
            var tpObj = IndexLoadAlignerConfig.GetTeachingPosition(tpName);
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

            if (Rotary != null && this.Rotary.IsIndexMoving())
            {
                AxisIndexZ.EmgStop();
                AxisAlignT.EmgStop();
                PostAlarm((int)AlarmKeys.eRotaryAxesMoving);
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

                    Thread.Sleep(2); // 0→5ms로 약간 여유 (CPU 점유 감소)
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
            if (Rotary != null && this.Rotary.IsIndexMoving())
            {
                AxisIndexZ?.EmgStop();
                AxisAlignT?.EmgStop();
                PostAlarm((int)AlarmKeys.eRotaryAxesMoving);
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
                    Thread.Sleep(2);
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
                OnMovePositionAlignTBackward(isFine);
                return 0;
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
                {
                    return 0;
                }
                Thread.Sleep(1);
            }
            return nRet;
            //return MoveTeachingPositionOnce((int)IndexLoadAlignerConfig.TeachingPositionName.AlignT_Backward, isFine);
        }
        private int IsMoveInterLockAlignTBackward()
        {
            int nRet = 0;
            if (Rotary != null && this.Rotary.IsIndexMoving())
            {
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
                    Thread.Sleep(2);
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
            if (Rotary != null && this.Rotary.IsIndexMoving())
            {
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
                    Thread.Sleep(2);
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

            string tpName = $"AlignZ_Index{teachingIdx}_Up";

            // Z축만 판정 (축별 확인)
            if (AxisIndexZ == null) return true;
            var tp = Config.GetTeachingPosition(tpName);
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

            var tp = Config.GetTeachingPosition(tpName);
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
            const double zeroTolerance = 0.007;
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
        public int MoveToTeachingPosition(string positionName, double vel = 5, double acc = 10, double dec = 10, double jerk = 50)
        {
            var tp = Config.GetTeachingPosition(positionName);
            if (tp == null) return -1;
            int result = 0;

            //Todo : Z축 확인 후 이동 하도록 수정.
            //foreach (var axisKey in tp.AxisPositions.Keys)
            //{
            //    if (Axes.TryGetValue(axisKey, out var axis))
            //    {
            //        double pos = tp.AxisPositions[axisKey];
            //        int r = axis.MoveAbs(pos, vel, acc, dec, jerk);
            //        if (r != 0) result = r;
            //    }
            //}

            return result;
        }
        //public bool InPosTeaching(string positionName)
        //{
        //    var tp = Config.GetTeachingPosition(positionName);
        //    if (tp == null) return false;
        //    foreach (var kv in tp.AxisPositions)
        //        if (!Axes.TryGetValue(kv.Key, out var axis) || !InPos(axis, kv.Value)) return false;
        //    return true;
        //}
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

        protected override void OnMakeSequence()
        {
            base.OnMakeSequence();
            this.SequencePlayers.Add(RunAlignSocketOnceReady);
            this.SequencePlayers.Add(RunAlignSocketOnce);
        }

        #region Seq 단위 동작 함수
        
        /// <summary>
        /// Rotary(인덱스) 정지까지 대기. 
        /// - 성공: 0, 타임아웃/오류: -1, Auto 중 Stop 신호: 0
        /// </summary>
        private int WaitForRotaryIdle(int timeoutMs = -1, int pollMs = 2)
        {
            var sw = Stopwatch.StartNew();
            while (true)
            {
                // Auto 모드에서 Stop 신호 시 즉시 반환
                if (RunMode == UnitRunMode.Auto && IsStop)
                    return 0;

                // 즉시 확인 API 사용(알람 미발행)
                if(this.Rotary.IsIndexMoving() == false)
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

        /// <summary>
        /// Rotary(인덱스)가 정지 상태인지 즉시 확인.
        /// - 정지면 0, 이동 중이면 -1 반환(알람 포스트).
        /// - 대기는 수행하지 않는다(메인이 반복 호출/대기).
        /// </summary>
        public int IsRotaryIdle()
        {
            if (Rotary != null && this.Rotary.IsIndexMoving())
            {
                //AxisIndexZ.EmgStop();
                //AxisAlignT.EmgStop();

                //확인용이니깐 알람은 울리지 말자.
                //PostAlarm((int)AlarmKeys.eRotaryNotSafe);
                return -1;
            }
            return 0;
        }

        public int RunAlignSocketOnceReady(bool bFineSpeed = false)
        {
            int bRtn = 0;
            try
            {
                this.CurrentFunc = RunAlignSocketOnceReady;
                LogSequence("Start");
               
                while (IsRotaryIdle() != 0)
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
        public int RunAlignSocketOnce(bool bFineSpeed = false)
        {
            int bRtn = 0;
            this.CurrentFunc = RunAlignSocketOnce;
            LogSequence("Start");
            
            int nIndex = GetAlignIndexNo();
            try
            {
                Log.Write("kkkkkkIndexLoadAligner", "Start");
                bool bUseSocket = this.Rotary.Config.GetUseSocket(nIndex);
                if(bUseSocket == false)
                {
                    Log.Write(UnitName, "MAlign", "Skip: No socket at unload align position");
                    return 0;
                }
                MaterialDie die = this.Rotary.GetMAlignSocketMaterial();
                if(die == null)
                {
                    return 0;
                }
                
                //if(die.Presence != Material.MaterialPresence.Exist
                //    && die.State != DieProcessState.Error_load )
                if(die.Presence != Material.MaterialPresence.Exist)
                {
                    return 0;
                }

                while(IsRotaryIdle() != 0)
                {
                    if(IsStop)
                    { 
                        return 0; 
                    }
                    Thread.Sleep(1);
                }
                Log.Write("die_State", "RunAlignSocketOnce", die.State.ToString());

                var socket = this.Rotary.GetSocket(nIndex);
                socket.SetState(Rotary.RotarySocketState.MAligning);

                // 2) T Ready // tact Time 모자라면 비동기 처리 할것.
                Log.Write("kkkkkkIndexLoadAligner", "Start1");
                bRtn &= MovePositionAlignTReady(bFineSpeed);
                Log.Write("kkkkkkIndexLoadAligner", "Start2");
                //bRtn &= MovePositionAlignZReady(nIndex, bFineSpeed);
                Log.Write("kkkkkkIndexLoadAligner", "Start3");
                if (bRtn != 0)
                {
                    Log.Write(UnitName, "MAlign", "Fail: MovePositionAlignTReady/MovePositionAlignZReady");
                    return -1;
                }

                Log.Write("kkkkkkIndexLoadAligner", "MovePositionAlignZUp");
                // 3) Z Up
                bRtn = MovePositionAlignZUp(nIndex, bFineSpeed);
                if (bRtn != 0)
                {
                    Log.Write(UnitName, "MAlign", "Fail: MovePositionAlignUp");
                    return -1;
                }
                Log.Write("kkkkkkIndexLoadAligner", "MovePositionAlignTForward");
                // 4) T Forward
                bRtn = MovePositionAlignTForward(bFineSpeed);
                if (bRtn != 0)
                {
                    Log.Write(UnitName, "MAlign", "Fail: MovePositionAlignTForward1");
                    return -1;
                }

                WaitByTime(Config.WaitTime1Step);
                Log.Write("kkkkkkIndexLoadAligner", "MovePositionAlignTBackward");
                // 5) T Backward
                bRtn = MovePositionAlignTBackward(bFineSpeed);
                if (bRtn != 0)
                {
                    Log.Write(UnitName, "MAlign", "Fail: MovePositionAlignTBackward");
                    return -1;
                }

                WaitByTime(Config.WaitTime2Step);
                //bRtn = MovePositionAlignTForward(bFineSpeed);
                //if (bRtn != 0)
                //{
                //    Log.Write(UnitName, "MAlign", "Fail: MovePositionAlignTForward2");
                //    return -1;
                //}
                //WaitByTime(Config.WaitTime3Step);


                // Vision Align 검사 추가.
                bRtn = AlignXY(bFineSpeed);
                if (bRtn != 0)
                {
                    Log.Write(UnitName, "MAlign", "Fail: AlignXY");

                    //die.Presence = Material.MaterialPresence.Exist;
                    //die.ProcessSatate = Material.MaterialProcessSatate.Skipped;
                    //die.State = DieProcessState.Error_MAlign;
                }
                else
                {
                	//Test시
                    //die.Presence = Material.MaterialPresence.Exist;
                    //die.ProcessSatate = Material.MaterialProcessSatate.Processing;
                    //die.State = DieProcessState.Inspecting;
                }
                die.State = DieProcessState.Inspecting;

                List<Task<int>> tasks = new List<Task<int>>();
                Task<int> t = null;
                Log.Write("kkkkkkIndexLoadAligner", "MovePositionSafetyZ");
                t = MovePositionAsyncSafeSafetyZ(bFineSpeed);
                Task<int> tz = t;
                tasks.Add(t);
                if (bRtn != 0)
                {
                    Log.Write(UnitName, "MAlign", "Fail: MovePositionSafetyZ");
                    return -1;
                }
                Log.Write("kkkkkkIndexLoadAligner", "MovePositionAlignTReady");
                t = MovePositionAsyncAlignTReady(bFineSpeed);
                if (bRtn != 0)
                {
                    Log.Write(UnitName, "MAlign", "Fail: MovePositionAlignTReady");
                    return -1;
                }
                tasks.Add(t);
                
                tz.Wait();
                Log.Write("kkkkkkIndexLoadAligner", "End");
                CompleteLoadAligner = true;
                socket.SetState(Rotary.RotarySocketState.MAligned);
                LogSequence("End");
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return -1;
            }
            finally
            {
                if(IsAlignTReady() == false)
                {
                    bRtn = MovePositionAlignTReady(bFineSpeed);
                }
                if(IsPositionAlignZSafety() == false)
                {
                    bRtn += MovePositionSafetyZ(bFineSpeed);
                }
                if (bRtn != 0)
                {
                    Log.Write(UnitName, "MAlign", "Fail: MovePositionSafetyZ");
                }
                CompleteLoadAligner = true;
            }

            return bRtn;
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

            // 2) 안전 위치 복귀(선택)
            if (moveToSafeReady)
            {
                try
                {
                    while (IsRotaryIdle() != 0)
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
            int nRet = 0;
            IsStatus_AlignDoneXY = false;
            IsAlignResult = false;
            dLastFoundX = 0.0;
            dLastFoundY = 0.0;
            dLastFoundAngle = 0.0;

            MaterialDie die = this.Rotary.GetMAlignSocketMaterial();
            if (die == null || die.Presence != Material.MaterialPresence.Exist)
            {
                Log.Write(UnitName, "AlignXY", "Skip: No die on unload socket");
                return 0;
            }

            if (Config.IsSimulation || this.Config.IsDryRun)
            {
                IsAlignResult = true;
                IsStatus_AlignDoneXY = true;
                return 0;
            }
            try
            {
                VisionImage img = null;
                double dX = 0;
                double dY = 0;
                double dAngle = 0;
                IndexAlignerCam.SuspendedImageDisplay = true;
                IndexAlignerCam.GrabSync(out img);
                var result = PmRunner.Search(img);
                if (result != null && result.Success && result.Matches != null && result.Matches.Count > 0)
                {
                    int repIdx = 2; // (result.ReferenceIndex >= 0 && result.ReferenceIndex < result.Matches.Count) ? result.ReferenceIndex : 0;
                    RaiseMarks(img, result.Matches.ToArray(), repIdx);
                    IndexAlignerCam.SuspendedImageDisplay = false;
                }

                if (result.Success)
                {
                    IsAlignResult = true;
                    dX = result.X;
                    dY = result.Y;
                    dAngle = result.R;
                }
                else
                {
                    IsAlignResult = false;
                    dX = 0;
                    dY = 0;
                    dAngle = 0;
                }

                PointD pt = GetPixelToMmScale(dX, dY);
                dLastFoundX = pt.X;
                dLastFoundY = pt.Y;
                dLastFoundAngle = dAngle;
                Log.Write(UnitName, "AlignXY",
                    $"VisionX={dLastFoundX:F4}, " +
                    $"VisionY={dLastFoundY:F4}, " +
                    $"VisionAngle={dLastFoundAngle:F4}");
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
            return nRet;
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
            try { MarksFound?.Invoke(this, e); } catch { }
        }
    }
}