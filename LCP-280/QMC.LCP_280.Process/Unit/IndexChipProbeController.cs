using QMC.Common;
using QMC.Common.Alarm;
using QMC.Common.Cameras;
using QMC.Common.Component;
using QMC.Common.IOUtil;
using QMC.Common.Motion;
using QMC.Common.Motions;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Component;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
            eSphereNotForward = 4704,
            eSphereFBTimeout = 4705,
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

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eSphereNotForward;
            alarm.Title = "Sphere Cylinder Not Forward.";
            alarm.Cause = "Sphere Cylinder가 Forward 위치가 아닙니다.\n 포지션 확인 바랍니다.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Warning.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eSphereFBTimeout;
            alarm.Title = "Sphere Cylinder Forward/Backward Timeout.";
            alarm.Cause = "Sphere Cylinder Forward/Backward Timeout입니다.\n Cylinder 확인 바랍니다.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Warning.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

        }
        #endregion

        #region Unit
        Rotary Rotary { get; set; }
        IndexChipProber IndexChipProber { get; set; }
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
            return MoveTeachingPositionOnce((int)IndexChipProbeControllerConfig.TeachingPositionName.SafetyZone, isFine);
        }
        private int IsMoveInterLockSafetyZ()
        {
            int nRet = 0;
            // Check Interlock.!!! 구문 넣을것.!!!
            if (Rotary != null && !Rotary.IsAnyAxisMoving())
            {
                AxisProbeZ?.EmgStop();
                AxisProbeCardX?.EmgStop();
                AxisProbeCardY?.EmgStop();
                AxisProbeCardZ?.EmgStop();
                AxisSphereZ?.EmgStop();
                PostAlarm((int)AlarmKeys.eRotaryNotSafe);
                nRet = -1;
                return nRet;
            }
            return nRet;
        }
        public Task<int> MovePositionAsyncSafeSafetyZ(bool isFine = false, CancellationToken ct = default(CancellationToken))
        {
            return Task.Run(() =>
            {
                // OnMovePickUpPosition을 Task로 돌리고 별도 인터락/취소 감시
                var coreTask = Task.Run(() => OnMovePositionSafetyZ(isFine), ct);

                while (!IsEndTask(coreTask))
                {
                    if (ct.IsCancellationRequested)
                    {
                        try
                        {
                            AxisProbeZ?.EmgStop();
                            AxisProbeCardX?.EmgStop();
                            AxisProbeCardY?.EmgStop();
                            AxisProbeCardZ?.EmgStop();
                            AxisSphereZ?.EmgStop();
                        }
                        catch { }
                        return -999; // 취소 코드
                    }

                    int nRtn = IsMoveInterLockSafetyZ();
                    if (nRtn != 0)
                    {
                        return -1;
                    }

                    Thread.Sleep(5); // 0→5ms로 약간 여유 (CPU 점유 감소)
                }

                return coreTask.Result;
            },
            ct);
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
            return MoveTeachingPositionOnce((int)IndexChipProbeControllerConfig.TeachingPositionName.SphereZ_Up, isFine);
        }
        private int IsMoveInterLockSphereZDown()
        {
            int nRet = 0;
            // Check Interlock.!!! 구문 넣을것.!!!
            if (Rotary != null && !Rotary.IsAnyAxisMoving())
            {
                AxisProbeZ?.EmgStop();
                AxisProbeCardX?.EmgStop();
                AxisProbeCardY?.EmgStop();
                AxisProbeCardZ?.EmgStop();
                AxisSphereZ?.EmgStop();
                PostAlarm((int)AlarmKeys.eRotaryNotSafe);
                nRet = -1;
                return nRet;
            }
            return nRet;
        }
        public Task<int> MovePositionAsyncSafeSphereZDown(bool isFine = false, CancellationToken ct = default(CancellationToken))
        {
            return Task.Run(() =>
            {
                // OnMovePickUpPosition을 Task로 돌리고 별도 인터락/취소 감시
                var coreTask = Task.Run(() => OnMovePositionSphereZDown(isFine), ct);

                while (!IsEndTask(coreTask))
                {
                    if (ct.IsCancellationRequested)
                    {
                        try
                        {
                            AxisProbeZ?.EmgStop();
                            AxisProbeCardX?.EmgStop();
                            AxisProbeCardY?.EmgStop();
                            AxisProbeCardZ?.EmgStop();
                            AxisSphereZ?.EmgStop();
                        }
                        catch { }
                        return -999; // 취소 코드
                    }

                    int nRtn = IsMoveInterLockSphereZDown();
                    if (nRtn != 0)
                    {
                        return -1;
                    }

                    Thread.Sleep(5); // 0→5ms로 약간 여유 (CPU 점유 감소)
                }

                return coreTask.Result;
            },
            ct);
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
            return MoveTeachingPositionOnce((int)IndexChipProbeControllerConfig.TeachingPositionName.SphereZ_Ready, isFine);
        }
        private int IsMoveInterLockSphereZReady()
        {
            int nRet = 0;
            // Check Interlock.!!! 구문 넣을것.!!!
            if (Rotary != null && !Rotary.IsAnyAxisMoving())
            {
                AxisProbeZ?.EmgStop();
                AxisProbeCardX?.EmgStop();
                AxisProbeCardY?.EmgStop();
                AxisProbeCardZ?.EmgStop();
                AxisSphereZ?.EmgStop();
                PostAlarm((int)AlarmKeys.eRotaryNotSafe);
                nRet = -1;
                return nRet;
            }
            return nRet;
        }
        public Task<int> MovePositionAsyncSafeSphereZReady(bool isFine = false, CancellationToken ct = default(CancellationToken))
        {
            return Task.Run(() =>
            {
                // OnMovePickUpPosition을 Task로 돌리고 별도 인터락/취소 감시
                var coreTask = Task.Run(() => OnMovePositionSphereZReady(isFine), ct);

                while (!IsEndTask(coreTask))
                {
                    if (ct.IsCancellationRequested)
                    {
                        try
                        {
                            AxisProbeZ?.EmgStop();
                            AxisProbeCardX?.EmgStop();
                            AxisProbeCardY?.EmgStop();
                            AxisProbeCardZ?.EmgStop();
                            AxisSphereZ?.EmgStop();
                        }
                        catch { }
                        return -999; // 취소 코드
                    }

                    int nRtn = IsMoveInterLockSphereZReady();
                    if (nRtn != 0)
                    {
                        return -1;
                    }

                    Thread.Sleep(5); // 0→5ms로 약간 여유 (CPU 점유 감소)
                }

                return coreTask.Result;
            },
            ct);
        }


        public int MovePositionTopContact_Index_Up(int nIndex = 0, bool isFine = false)
        {
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
            // nIndex 처리 (0-based와 1-based 모두 지원)
            //  - 1~8 : 그대로 사용 (Place_Index1 ~ Place_Index8)
            //  - 0~7 : +1 보정하여 1~8 매핑
            int teachingIdx = 0;
            if (nIndex >= 1 && nIndex <= 8)
                teachingIdx = nIndex + 1;
            else if (nIndex >= 0 && nIndex < 8)
                teachingIdx = nIndex + 1; // 0-based 입력으로 판단
            else
            {
                Log.Write(UnitName, $"[OnMovePositionTopContact_Index_Up] Invalid index {nIndex}. Range 0~7 or 1~8");
                return -1;
            }

            nRet = OnMovePositionTopContact_Index_ReadyZ(nIndex, isFine);
            if (nRet != 0)
            {
                Log.Write(UnitName, "[OnMovePositionTopContact_Index_ReadyZ] ToolT move failed");
                return -1;
            }

            string tpName = $"TopContact_index{teachingIdx}_Up";
            var tpObj = IndexChipProbeControllerConfig.GetTeachingPosition(tpName);
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
            return nRet;
            //return MoveTeachingPositionOnce((int)IndexChipProbeControllerConfig.TeachingPositionName.SphereZ_Ready, isFine);
        }
        private int IsMoveInterLockTopContact_Index_Up()
        {
            int nRet = 0;
            // Check Interlock.!!! 구문 넣을것.!!!
            if (Rotary != null && !Rotary.IsAnyAxisMoving())
            {
                AxisProbeZ?.EmgStop();
                AxisProbeCardX?.EmgStop();
                AxisProbeCardY?.EmgStop();
                AxisProbeCardZ?.EmgStop();
                AxisSphereZ?.EmgStop();
                PostAlarm((int)AlarmKeys.eRotaryNotSafe);
                nRet = -1;
                return nRet;
            }
            return nRet;
        }
        public Task<int> MovePositionAsyncSafeTopContact_Index_Up(int nIndex, bool isFine = false, CancellationToken ct = default(CancellationToken))
        {
            return Task.Run(() =>
            {
                // OnMovePickUpPosition을 Task로 돌리고 별도 인터락/취소 감시
                var coreTask = Task.Run(() => OnMovePositionTopContact_Index_Up(nIndex, isFine), ct);

                while (!IsEndTask(coreTask))
                {
                    if (ct.IsCancellationRequested)
                    {
                        try
                        {
                            AxisProbeZ?.EmgStop();
                            AxisProbeCardX?.EmgStop();
                            AxisProbeCardY?.EmgStop();
                            AxisProbeCardZ?.EmgStop();
                            AxisSphereZ?.EmgStop();
                        }
                        catch { }
                        return -999; // 취소 코드
                    }

                    int nRtn = IsMoveInterLockSphereZReady();
                    if (nRtn != 0)
                    {
                        return -1;
                    }

                    Thread.Sleep(5); // 0→5ms로 약간 여유 (CPU 점유 감소)
                }

                return coreTask.Result;
            },
            ct);
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
            // nIndex 처리 (0-based와 1-based 모두 지원)
            //  - 1~8 : 그대로 사용 (Place_Index1 ~ Place_Index8)
            //  - 0~7 : +1 보정하여 1~8 매핑
            int teachingIdx = 0;
            if (nIndex >= 0 && nIndex < 8)
                teachingIdx = nIndex + 1; // 0-based 입력으로 판단
            else
            {
                Log.Write(UnitName, $"[OnMovePositionTopContact_Index_Ready] Invalid index {nIndex}. Range 0~7 or 1~8");
                return -1;
            }

            string tpName = $"TopContact_Index{teachingIdx}_Ready";
            var tpObj = IndexChipProbeControllerConfig.GetTeachingPosition(tpName);
            if (tpObj == null)
            {
                Log.Write(UnitName, $"[OnMovePositionTopContact_Index_Ready] Teaching not found: {tpName}");
                return -1;
            }

            double dTPos = GetTP(tpName, AxisNames.ProbeZ);
            nRet = OnMoveAxisPositionOne(AxisProbeZ, dTPos);
            if (nRet != 0)
            {
                Log.Write(UnitName, $"[OnMovePositionTopContact_Index_Ready] ToolT move failed tp={tpName} pos={dTPos}");
                return -1;
            }
            return nRet;
            //return MoveTeachingPositionOnce((int)IndexChipProbeControllerConfig.TeachingPositionName.SphereZ_Ready, isFine);
        }
        private int IsMoveInterLockTopContact_Index_Ready()
        {
            int nRet = 0;
            // Check Interlock.!!! 구문 넣을것.!!!
            if (Rotary != null && !Rotary.IsAnyAxisMoving())
            {
                AxisProbeZ?.EmgStop();
                AxisProbeCardX?.EmgStop();
                AxisProbeCardY?.EmgStop();
                AxisProbeCardZ?.EmgStop();
                AxisSphereZ?.EmgStop();
                PostAlarm((int)AlarmKeys.eRotaryNotSafe);
                nRet = -1;
                return nRet;
            }
            return nRet;
        }
        public Task<int> MovePositionAsyncSafeTopContact_Index_Ready(int nIndex, bool isFine = false, CancellationToken ct = default(CancellationToken))
        {
            return Task.Run(() =>
            {
                // OnMovePickUpPosition을 Task로 돌리고 별도 인터락/취소 감시
                var coreTask = Task.Run(() => OnMovePositionTopContact_Index_Ready(nIndex, isFine), ct);

                while (!IsEndTask(coreTask))
                {
                    if (ct.IsCancellationRequested)
                    {
                        try
                        {
                            AxisProbeZ?.EmgStop();
                            AxisProbeCardX?.EmgStop();
                            AxisProbeCardY?.EmgStop();
                            AxisProbeCardZ?.EmgStop();
                            AxisSphereZ?.EmgStop();
                        }
                        catch { }
                        return -999; // 취소 코드
                    }

                    int nRtn = IsMoveInterLockTopContact_Index_Ready();
                    if (nRtn != 0)
                    {
                        return -1;
                    }

                    Thread.Sleep(5); // 0→5ms로 약간 여유 (CPU 점유 감소)
                }

                return coreTask.Result;
            },
            ct);
        }
        private int OnMovePositionTopContact_Index_ReadyZ(int nIndex, bool bFineSpeed)
        {
            int nRet = 0;
            // nIndex 처리 (0-based와 1-based 모두 지원)
            //  - 1~8 : 그대로 사용 (Place_Index1 ~ Place_Index8)
            //  - 0~7 : +1 보정하여 1~8 매핑
            int teachingIdx = 0;
            if (nIndex >= 1 && nIndex <= 8)
                teachingIdx = nIndex + 1;
            else if (nIndex >= 0 && nIndex < 8)
                teachingIdx = nIndex + 1; // 0-based 입력으로 판단
            else
            {
                Log.Write(UnitName, $"[OnMovePositionTopContact_Index_ReadyZ] Invalid index {nIndex}. Range 0~7 or 1~8");
                return -1;
            }

            string tpName = $"TopContact_Index{teachingIdx}_Ready";
            var tpObj = IndexChipProbeControllerConfig.GetTeachingPosition(tpName);
            if (tpObj == null)
            {
                Log.Write(UnitName, $"[OnMovePositionTopContact_Index_ReadyZ] Teaching not found: {tpName}");
                return -1;
            }

            double dTPos = GetTP(tpName, AxisNames.ProbeZ);
            nRet = OnMoveAxisPositionOne(AxisProbeZ, dTPos);
            if (nRet != 0)
            {
                Log.Write(UnitName, $"[OnMovePositionTopContact_Index_ReadyZ] ToolT move failed tp={tpName} pos={dTPos}");
                return -1;
            }
            return nRet;
            //return MoveTeachingPositionOnce((int)IndexChipProbeControllerConfig.TeachingPositionName.SphereZ_Ready, isFine);

        }

        public int MovePositionBottomContact_Index_Up(int nIndex = 0, bool isFine = false)
        {
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
            // nIndex 처리 (0-based와 1-based 모두 지원)
            //  - 1~8 : 그대로 사용 (Place_Index1 ~ Place_Index8)
            //  - 0~7 : +1 보정하여 1~8 매핑
            int teachingIdx = 0;
            if (nIndex >= 1 && nIndex <= 8)
                teachingIdx = nIndex + 1;
            else if (nIndex >= 0 && nIndex < 8)
                teachingIdx = nIndex + 1; // 0-based 입력으로 판단
            else
            {
                Log.Write(UnitName, $"[OnMovePositionBottomContact_Index_Up] Invalid index {nIndex}. Range 0~7 or 1~8");
                return -1;
            }

            nRet = OnMovePositionBottomContact_Index_ReadyZ(nIndex, isFine);
            if(nRet != 0)
            {
                Log.Write(UnitName, $"[OnMovePositionBottomContact_Index_Up] MovePositionBottomContact_Index_ReadyZ failed index={nIndex}");
                return -1;
            }

            string tpName = $"Bottom_index{teachingIdx}_Up";
            var tpObj = IndexChipProbeControllerConfig.GetTeachingPosition(tpName);
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

            double dZPos = GetTP(tpName, AxisNames.ProbeCardZ);
            nRet &= OnMoveAxisPositionOne(AxisProbeCardZ, dZPos);
            if (nRet != 0)
            {
                Log.Write(UnitName, $"[OnMovePositionBottomContact_Index_Up] ToolT move failed tp={tpName} posZ={dZPos}");
                return -1;
            }


            return nRet;
            //return MoveTeachingPositionOnce((int)IndexChipProbeControllerConfig.TeachingPositionName.SphereZ_Ready, isFine);
        }
        private int IsMoveInterLockBottomContact_Index_Up()
        {
            int nRet = 0;
            // Check Interlock.!!! 구문 넣을것.!!!
            if (Rotary != null && !Rotary.IsAnyAxisMoving())
            {
                AxisProbeZ?.EmgStop();
                AxisProbeCardX?.EmgStop();
                AxisProbeCardY?.EmgStop();
                AxisProbeCardZ?.EmgStop();
                AxisSphereZ?.EmgStop();
                PostAlarm((int)AlarmKeys.eRotaryNotSafe);
                nRet = -1;
                return nRet;
            }
            return nRet;
        }
        public Task<int> MovePositionAsyncSafeBottomContact_Index_Up(int nIndex, bool isFine = false, CancellationToken ct = default(CancellationToken))
        {
            return Task.Run(() =>
            {
                // OnMovePickUpPosition을 Task로 돌리고 별도 인터락/취소 감시
                var coreTask = Task.Run(() => OnMovePositionBottomContact_Index_Up(nIndex, isFine), ct);

                while (!IsEndTask(coreTask))
                {
                    if (ct.IsCancellationRequested)
                    {
                        try
                        {
                            AxisProbeZ?.EmgStop();
                            AxisProbeCardX?.EmgStop();
                            AxisProbeCardY?.EmgStop();
                            AxisProbeCardZ?.EmgStop();
                            AxisSphereZ?.EmgStop();
                        }
                        catch { }
                        return -999; // 취소 코드
                    }

                    int nRtn = IsMoveInterLockBottomContact_Index_Up();
                    if (nRtn != 0)
                    {
                        return -1;
                    }

                    Thread.Sleep(5); // 0→5ms로 약간 여유 (CPU 점유 감소)
                }

                return coreTask.Result;
            },
            ct);
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
            int nRet = 0;
            // nIndex 처리 (0-based와 1-based 모두 지원)
            //  - 1~8 : 그대로 사용 (Place_Index1 ~ Place_Index8)
            //  - 0~7 : +1 보정하여 1~8 매핑
            int teachingIdx = 0;
            if (nIndex >= 1 && nIndex <= 8)
                teachingIdx = nIndex + 1;
            else if (nIndex >= 0 && nIndex < 8)
                teachingIdx = nIndex + 1; // 0-based 입력으로 판단
            else
            {
                Log.Write(UnitName, $"[OnMovePositionBottomContact_Index_Ready] Invalid index {nIndex}. Range 0~7 or 1~8");
                return -1;
            }

            string tpName = $"Bottom_Index{teachingIdx}_Ready";
            var tpObj = IndexChipProbeControllerConfig.GetTeachingPosition(tpName);
            if (tpObj == null)
            {
                Log.Write(UnitName, $"[OnMovePositionBottomContact_Index_Ready] Teaching not found: {tpName}");
                return -1;
            }

            double dZPos = GetTP(tpName, AxisNames.ProbeCardZ);
            nRet &= OnMoveAxisPositionOne(AxisProbeCardZ, dZPos);
            if (nRet != 0)
            {
                Log.Write(UnitName, $"[OnMovePositionBottomContact_Index_Ready] ToolT move failed tp={tpName} posZ={dZPos}");
                return -1;
            }

            double dXPos = GetTP(tpName, AxisNames.ProbeCardX);
            double dYPos = GetTP(tpName, AxisNames.ProbeCardY);

            nRet &= OnMoveAxisPositionOne(AxisProbeCardX, dXPos);
            nRet &= OnMoveAxisPositionOne(AxisProbeCardX, dYPos);
            if (nRet != 0)
            {
                Log.Write(UnitName, $"[OnMovePositionBottomContact_Index_Ready] ToolT move failed tp={tpName} posX={dXPos}");
                Log.Write(UnitName, $"[OnMovePositionBottomContact_Index_Ready] ToolT move failed tp={tpName} posY={dYPos}");
                return -1;
            }
            return nRet;
            //return MoveTeachingPositionOnce((int)IndexChipProbeControllerConfig.TeachingPositionName.SphereZ_Ready, isFine);
        }
        private int IsMoveInterLockBottomContact_Index_Ready()
        {
            int nRet = 0;
            // Check Interlock.!!! 구문 넣을것.!!!
            if (Rotary != null && !Rotary.IsAnyAxisMoving())
            {
                AxisProbeZ?.EmgStop();
                AxisProbeCardX?.EmgStop();
                AxisProbeCardY?.EmgStop();
                AxisProbeCardZ?.EmgStop();
                AxisSphereZ?.EmgStop();
                PostAlarm((int)AlarmKeys.eRotaryNotSafe);
                nRet = -1;
                return nRet;
            }
            return nRet;
        }
        public Task<int> MovePositionAsyncSafeBottomContact_Index_Ready(int nIndex, bool isFine = false, CancellationToken ct = default(CancellationToken))
        {
            return Task.Run(() =>
            {
                // OnMovePickUpPosition을 Task로 돌리고 별도 인터락/취소 감시
                var coreTask = Task.Run(() => OnMovePositionBottomContact_Index_Ready(nIndex, isFine), ct);

                while (!IsEndTask(coreTask))
                {
                    if (ct.IsCancellationRequested)
                    {
                        try
                        {
                            AxisProbeZ?.EmgStop();
                            AxisProbeCardX?.EmgStop();
                            AxisProbeCardY?.EmgStop();
                            AxisProbeCardZ?.EmgStop();
                            AxisSphereZ?.EmgStop();
                        }
                        catch { }
                        return -999; // 취소 코드
                    }

                    int nRtn = IsMoveInterLockBottomContact_Index_Ready();
                    if (nRtn != 0)
                    {
                        return -1;
                    }

                    Thread.Sleep(5); // 0→5ms로 약간 여유 (CPU 점유 감소)
                }

                return coreTask.Result;
            },
            ct);
        }

        private int OnMovePositionBottomContact_Index_ReadyZ(int nIndex = 0, bool isFine = false)
        {
            int nRet = 0;
            // nIndex 처리 (0-based와 1-based 모두 지원)
            //  - 1~8 : 그대로 사용 (Place_Index1 ~ Place_Index8)
            //  - 0~7 : +1 보정하여 1~8 매핑
            int teachingIdx = 0;
            if (nIndex >= 1 && nIndex <= 8)
                teachingIdx = nIndex + 1;
            else if (nIndex >= 0 && nIndex < 8)
                teachingIdx = nIndex + 1; // 0-based 입력으로 판단
            else
            {
                Log.Write(UnitName, $"[OnMovePositionBottomContact_Index_Up] Invalid index {nIndex}. Range 0~7 or 1~8");
                return -1;
            }

            string tpName = $"Bottom_Index{teachingIdx}_Ready";
            var tpObj = IndexChipProbeControllerConfig.GetTeachingPosition(tpName);
            if (tpObj == null)
            {
                Log.Write(UnitName, $"[OnMovePositionBottomContact_Index_Ready] Teaching not found: {tpName}");
                return -1;
            }

            double dZPos = GetTP(tpName, AxisNames.ProbeCardZ);
            nRet &= OnMoveAxisPositionOne(AxisProbeCardZ, dZPos);
            if (nRet != 0)
            {
                Log.Write(UnitName, $"[OnMovePositionBottomContact_Index_Ready] ToolT move failed tp={tpName} posZ={dZPos}");
                return -1;
            }

            return nRet;
        }

        #region Safety Position Helpers (Individual Axis)
        /// <summary>
        /// 내부 헬퍼: TeachingPosition에서 특정 축 목표값을 얻는다.
        /// </summary>
        private bool TryGetTeachingAxisPosition(string tpName, string axisName, out double target)
        {
            target = 0.0;
            var tp = Config.GetTeachingPosition(tpName);
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

        /// <summary>
        /// ProbeZ 축이 SafetyZone TeachingPosition 의 ProbeZ 위치에 InPosition 인가
        /// </summary>
        public bool IsAxisProbeZSafetyPos()
            => IsAxisInTeachingPosition(AxisProbeZ,
                IndexChipProbeControllerConfig.TeachingPositionName.SafetyZone.ToString(),
                AxisNames.ProbeZ);

        /// <summary>
        /// ProbeCardZ 축이 SafetyZone TeachingPosition 의 ProbeCardZ 위치에 InPosition 인가
        /// </summary>
        public bool IsAxisProbeCardZSafetyPos()
            => IsAxisInTeachingPosition(AxisProbeCardZ,
                IndexChipProbeControllerConfig.TeachingPositionName.SafetyZone.ToString(),
                AxisNames.ProbeCardZ);

        /// <summary>
        /// SphereZ 축이 SphereZ_Up (우선) 또는 SphereZ_Ready TeachingPosition 위치에 InPosition 인가
        /// </summary>
        public bool IsAxisSphereZSafetyPos()
        {
            // Up 우선
            if (IsAxisInTeachingPosition(AxisSphereZ,
                    IndexChipProbeControllerConfig.TeachingPositionName.SphereZ_Up.ToString(),
                    AxisNames.SphereZ))
                return true;

            // Up 이 없거나 InPosition 아니면 Ready 로 재확인
            return IsAxisInTeachingPosition(AxisSphereZ,
                    IndexChipProbeControllerConfig.TeachingPositionName.SphereZ_Ready.ToString(),
                    AxisNames.SphereZ);
        }

        /// <summary>
        /// 세 축(ProbeZ, ProbeCardZ, SphereZ)이 모두 Safety 위치인가
        /// </summary>
        public bool IsAllSafetyAxisPos()
            => IsAxisProbeZSafetyPos() &&
               IsAxisProbeCardZSafetyPos() &&
               IsAxisSphereZSafetyPos();
        



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
        public bool IsSphereForward()
        {
            if(Config.IsSimulation || Config.IsDryRun)
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
        
        /////////////////////


        // === Direct Valve Control (강제 구동) ===
        public bool IsSphereFwdValveOn()       => IsOutputOn(NAME_SPHERE_FW);
        public bool IsProbeVacValveOn()        => IsOutputOn(NAME_PROBE_VAC);
        #endregion


        #region seq Signals

        public bool CompleteProbe { get; internal set; } = false;

        #endregion

        #region Lifecycle
        public override int OnRun()
        {
            int ret = 0;

            if (this.RunUnitStatus == UnitStatus.Stopped ||
                this.RunUnitStatus == UnitStatus.Stopping ||
                this.RunUnitStatus == UnitStatus.CycleStop)
            {
                this.State = ProcessState.Stop;
                ret = 1;
            }
            else
            {
                switch (State)
                {
                    case ProcessState.Ready:
                        if (Rotary.RequestProbe)
                        {
                            CompleteProbe = false;
                            ret = OnRunReady();
                        }
                        break;
                    case ProcessState.Work:
                        ret = OnRunWork();
                        break;
                    case ProcessState.Complete:
                        ret = OnRunComplete();
                        if (ret == 0)
                        {
                            CompleteProbe = true;
                        }
                        break;
                    default:
                        this.State = ProcessState.Ready;
                        break;
                }
            }

            if (ret != 0)
            {
                this.State = ProcessState.Stop;
                this.OnStop();
            }

            return ret;
        }
        public override int OnStop() 
        {
            int ret = 0;
            this.RunUnitStatus = UnitStatus.Stopped;
            this.State = ProcessState.Stop;
            base.OnStop();
            return ret;
        }
        protected override int OnRunReady() { return 0; }
        protected override int OnRunWork() { return 0; }
        protected override int OnRunComplete() { return 0; }
        #endregion

        public int IsRotaryIdle()
        {
            if (Rotary != null && !Rotary.IsAnyAxisMoving())
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

        public bool IsTopRequired()
        {
            //Todo: Recipe Data로 사용해야함.
            return Config.ContectMode;
        }
        protected override void OnMakeSequence()
        {
            base.OnMakeSequence();
            this.SequencePlayers.Add(ContactReady);
            this.SequencePlayers.Add(ContactBottomOrTop);
            
            //this.SequencePlayers.Add(b => (IsBottomRequired() ? BottomContactOnce(b) : TopContact(b)));
            //this.SequencePlayers.Add(ContactBottomOrTop);
            //this.SequencePlayers.Add(BottomContactOnce);
            //this.SequencePlayers.Add(TopContact);

        }
        #region Seq 단위 동작 함수

        public int ContactReady(bool bFineSpeed = false)
        {
            int nRet = -1;

            // 1) Rotary Idle 확인인데.. 실제 공정 시 필요 없을듯.
            nRet = IsRotaryIdle();
            if (nRet != 0)
                return -1;

            if (SetSphereFB(true))
            {
                var sw = Stopwatch.StartNew();
                while (!IsSphereForward())
                {
                    if (sw.ElapsedMilliseconds > 2000)
                    {
                        PostAlarm((int)AlarmKeys.eSphereFBTimeout);
                        Log.Write(UnitName, "[BottomContactOnce] SphereFB-F Timeout");
                        return -1;
                    }
                    Thread.Sleep(1);
                }
            }

            nRet = MovePositionSphereZReady();
            if (nRet != 0)
            {
                return -1;
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
        public int ContactBottomOrTop(bool bFineSpeed = false)
        {
            int nRet = 0;
            this.CurrentFunc = ContactBottomOrTop;
            LogSequence("Start");

            if(IsTopRequired())
            {
                nRet = TopContactOnce(bFineSpeed);
                if (nRet != 0)
                {
                    return -1;
                }
            }
            else
            {
                nRet = BottomContactOnce(bFineSpeed);
                if (nRet != 0)
                {
                    return -1;
                }
            }

            LogSequence("End");
            return nRet; 
        }

        public int TopContactOnce(bool bFineSpeed = false)
        {
            int nRet = 0;
            try
            {
                LogSequence("Start");
                this.CurrentFunc = TopContactOnce;

                int nIndex = GetProbeIndexNo();

                nRet = IsRotaryIdle();
                if (nRet != 0)
                    return -1;

                if (!IsSphereForward())
                {
                    PostAlarm((int)AlarmKeys.eSphereNotForward);
                    Log.Write(UnitName, "[TopContactOnce] Sphere not forward");
                    return -1;
                }

                nRet = MovePositionSphereZDown(); //실제로는 다운임.
                if (nRet != 0)
                {
                    return -1;
                }

                nRet = MovePositionTopContact_Index_Ready(nIndex, bFineSpeed);
                if (nRet != 0)
                {
                    Log.Write(UnitName, "[TopContactOnce] MovePositionTopContact_Index_Ready failed");
                    return -1;
                }

                nRet = MovePositionTopContact_Index_Up(nIndex, bFineSpeed);
                if (nRet != 0)
                {
                    Log.Write(UnitName, "[TopContactOnce] OnMovePositionTopContact_Index_Up failed");
                    return -1;
                }

                // 6) 검사 요구 신호
                SetChipProberRequest(true);

                // 7) 검사 완료 신호 대기
                nRet = ContactInspectionWait();
                if (nRet != 0)
                {

                    return -1;
                }

                // 8) 검사 완료 처리
                SetChipProberRequest(false);

                // 9) Ready Z축 하강
                nRet = OnMovePositionTopContact_Index_ReadyZ(nIndex, bFineSpeed);
                if (nRet != 0)
                {

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
            int nRet = 0;

            try
            {
                LogSequence("Start");
                this.CurrentFunc = BottomContactOnce;

                int nIndex = GetProbeIndexNo();

                nRet = IsRotaryIdle();
                if (nRet != 0)
                    return -1;

                if (!IsSphereForward())
                {
                    PostAlarm((int)AlarmKeys.eSphereNotForward);
                    Log.Write(UnitName, "[BottomContactOnce] Sphere not forward");
                    return -1;
                }

                nRet = MovePositionSphereZDown(); //실제로는 다운임.
                if (nRet != 0)
                {
                    return -1;
                }

                nRet = MovePositionBottomContact_Index_Ready(nIndex, bFineSpeed);
                if (nRet != 0)
                {
                    Log.Write(UnitName, "[BottomContactOnce] MovePositionBottomContact_Index_Ready failed");
                    return -1;
                }

                nRet = OnMovePositionBottomContact_Index_Up(nIndex, bFineSpeed);
                if (nRet != 0)
                {
                    Log.Write(UnitName, "[BottomContactOnce] MovePositionBottomContact_Index_Up failed");
                    return -1;
                }

                // 6) 검사 요구 신호
                SetChipProberRequest(true);

                // 7) 검사 완료 신호 대기
                nRet = ContactInspectionWait();
                if (nRet != 0)
                {

                    return -1;
                }

                // 8) 검사 완료 처리
                SetChipProberRequest(false);

                // 9) Ready Z축 하강
                nRet = OnMovePositionBottomContact_Index_ReadyZ(nIndex, bFineSpeed);
                if (nRet != 0)
                {

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

            return nRet;
        }

        public int ContactInspectionWait(bool bFineSpeed = false)
        {
            int nRet = 0;
            if (IndexChipProber == null)
            {
                Log.Write(UnitName, "[BottomContactWait] IndexChipProber null");
                return -1;
            }
            if(Config.IsSimulation || Config.IsDryRun)
            {
                Thread.Sleep(1000);
                return 0;
            }

            // Config에 타임아웃 속성이 있다면 사용 (예: Config.ProbeInspectTimeoutMs), 없으면 기본값
            int timeoutMs = Config.ProbeInspectTimeOutms;
            try
            {
                var cfgType = Config.GetType();
                var pi = cfgType.GetProperty("ProbeInspectTimeOutms");
                if (pi != null && pi.PropertyType == typeof(int))
                {
                    int v = (int)pi.GetValue(Config, null);
                    if (v > 0) timeoutMs = v;
                }
            }
            catch { /* 무시 */ }

            var sw = Stopwatch.StartNew();
            while (!IndexChipProber.InspectDone)
            {
                if (sw.ElapsedMilliseconds > timeoutMs)
                {
                    PostAlarm((int)AlarmKeys.eProbeTimeout);
                    Log.Write(UnitName, $"[BottomContactWait] Inspect timeout (> {timeoutMs} ms)");
                    return -1;
                }

                // (선택) Rotary 움직임 감시하여 비정상 시 탈출 가능
                if (Rotary != null && !Rotary.IsAnyAxisMoving())
                {
                    Log.Write(UnitName, "[BottomContactWait] Rotary moving detected - abort wait");
                    return -1;
                }

                Thread.Sleep(5);
            }

            // 완료 시 후속 처리 필요하면 여기에 추가
            return nRet;
        }

        private void SetChipProberRequest(bool bRtn)
        {
            IndexChipProber.RequestChipInsp = bRtn;
        }

        private int GetProbeIndexNo()
        {
            if (Rotary == null)
                return 0;

            int loadIndex = Rotary.GetLoadIndexNo();

            // 반시계 방향으로 2칸 이동
            int probeIndex = (loadIndex - this.Config.IndexOfProbe + Rotary.GetIndexCount()) % Rotary.GetIndexCount();

            return probeIndex;

        }

        #endregion

    }
}