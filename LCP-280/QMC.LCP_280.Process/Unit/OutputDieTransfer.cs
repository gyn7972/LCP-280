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
using System.Threading.Tasks;
using static QMC.LCP_280.Process.Equipment;
using static QMC.LCP_280.Process.Unit.OutputDieTransferConfig.IO; // IO 상수/배열

namespace QMC.LCP_280.Process.Unit
{
    public class OutputDieTransfer : BaseUnit<OutputDieTransferConfig>
    {
        public enum AlarmKeys
        {
            eDieTransferPickZNotSafe = 3001,
            eOutputStageAxesMoving = 3002,
            eRotaryAxesMoving = 3003,
        }

        #region InitAlarm
        protected override void InitAlarm()
        {
            base.InitAlarm();
            AlarmInfo alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eDieTransferPickZNotSafe;
            alarm.Title = "Die Tr Z-Axis Not Sfarety Pos.";
            alarm.Cause = "Die TrZAxis이 안전 위치가 아닙니다.\n 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Warning.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

        }
        #endregion

        #region Unit
        Rotary Rotary { get; set; }
        OutputStage OutputStage { get; set; }

        #endregion

        #region Axis Helpers
        private MotionAxis _toolT, _pickZ, _placeZ;
        public MotionAxis AxisToolT => _toolT;
        public MotionAxis AxisPickZ => _pickZ;
        public MotionAxis AxisPlaceZ => _placeZ;
        private void BindAxes()
        {
            var mgr = Equipment.Instance?.AxisManager;
            if (mgr == null)
            {
                Log.Write("OutputDieTransfer", "[BindAxes] AxisManager null");
                return;
            }

            const string unitName = "Unit"; // Equipment에서 축 등록 시 사용한 유닛명과 동일해야 함
            BindAxis(mgr, unitName, AxisNames.RightToolT, ref _toolT);
            BindAxis(mgr, unitName, AxisNames.RightPickZ, ref _pickZ);
            BindAxis(mgr, unitName, AxisNames.RightPlaceZ, ref _placeZ);
        }
        #endregion


        public OutputDieTransfer(OutputDieTransferConfig config = null)
            : base(new OutputDieTransferConfig())
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
        protected override void OnBindUnit()
        {
            base.OnBindUnit();
            Rotary = Equipment.Instance.GetUnit(UnitKeys.Rotary) as Rotary;
            OutputStage = Equipment.Instance.GetUnit(UnitKeys.OutputStage) as OutputStage;
        }



        public int MoveAxisPositionOne(MotionAxis axis, double target, bool isFine = false)
        {
            if (axis == null) return -1;

            Task<int> task = MoveAxisPositionOneAsync(axis, target, isFine);
            while (IsEndTask(task) == false)
            {
                if (axis == AxisPickZ)
                {
                    if (OutputStage.IsAnyAxisMoving())
                    {
                        AxisToolT.EmgStop();
                        AxisPickZ.EmgStop();
                        AxisPlaceZ.EmgStop();
                        PostAlarm((int)AlarmKeys.eOutputStageAxesMoving);
                        return -1;
                    }
                }

                if (axis == AxisPlaceZ)
                {
                    if (Rotary.IsAnyAxisMoving())
                    {
                        AxisToolT.EmgStop();
                        AxisPickZ.EmgStop();
                        AxisPlaceZ.EmgStop();
                        PostAlarm((int)AlarmKeys.eRotaryAxesMoving);
                    }
                }

                Thread.Sleep(1);
            }
            return task.Result;
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
            return MoveTeachingPositionOnce((int)OutputDieTransferConfig.TeachingPositionName.SafetyZone, isFine);
        }
        private int IsMoveInterLockSafetyZ()
        {
            int nRet = 0;
            // Check Interlock.!!! 구문 넣을것.!!!

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
                            AxisToolT?.EmgStop();
                            AxisPickZ?.EmgStop();
                            AxisPlaceZ?.EmgStop();
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

        public int MovePositionReady(bool isFine = false)
        {
            Task<int> task = MovePositionAsyncReady(isFine);
            while (IsEndTask(task) == false)
            {
                int nRtn = IsMoveInterLockReady();
                if (nRtn != 0)
                {
                    return -1;
                }

                Thread.Sleep(0);
            }
            return task.Result;
        }
        public Task<int> MovePositionAsyncReady(bool isFine = false)
        {
            return Task.Run(() =>
            {
                OnMovePositionReady(isFine);
                return 0;
            });
        }
        private int OnMovePositionReady(bool isFine = false)
        {
            int nRet = 0;
            if (!IsPlaceZSafetyPos() || !IsPickZSafetyPos())
            {
                nRet = MovePositionSafetyZ();
                if (nRet != 0)
                {
                    return -1;
                }
            }

            double dTPos = GetTP(InputDieTransferConfig.TeachingPositionName.Ready.ToString(),
                        AxisNames.LeftToolT);
            nRet = MoveAxisPositionOne(AxisToolT, dTPos);
            if (nRet != 0)
            {
                return -1;
            }

            double dZPos = GetTP(InputDieTransferConfig.TeachingPositionName.Ready.ToString(),
                        AxisNames.LeftPlaceZ);
            nRet = MoveAxisPositionOne(AxisPlaceZ, dZPos);
            if (nRet != 0)
            {
                return -1;
            }

            return nRet;
            //return MoveTeachingPositionOnce((int)InputDieTransferConfig.TeachingPositionName.Pickup, isFine);
        }
        private int IsMoveInterLockReady()
        {
            int nRet = 0;
            if (OutputStage != null && OutputStage.IsAnyAxisMoving())
            {
                AxisToolT?.EmgStop();
                AxisPickZ?.EmgStop();
                AxisPlaceZ?.EmgStop();
                PostAlarm((int)AlarmKeys.eOutputStageAxesMoving);
                return -1;
            }

            if (Rotary != null && Rotary.IsAnyAxisMoving())
            {
                AxisToolT?.EmgStop();
                AxisPickZ?.EmgStop();
                AxisPlaceZ?.EmgStop();
                PostAlarm((int)AlarmKeys.eRotaryAxesMoving);
                return -1;
            }

            return nRet;
        }
        public Task<int> MovePositionAsyncSafeReady(bool isFine = false, CancellationToken ct = default(CancellationToken))
        {
            return Task.Run(() =>
            {
                // Task로 돌리고 별도 인터락/취소 감시
                var coreTask = Task.Run(() => OnMovePositionReady(isFine), ct);

                while (!IsEndTask(coreTask))
                {
                    if (ct.IsCancellationRequested)
                    {
                        try
                        {
                            AxisToolT?.EmgStop();
                            AxisPickZ?.EmgStop();
                            AxisPlaceZ?.EmgStop();
                        }
                        catch { }
                        return -999; // 취소 코드
                    }

                    int nRtn = IsMoveInterLockReady();
                    if (nRtn != 0)
                    {
                        return -1;
                    }

                    Thread.Sleep(5); // 0→5ms로 약간 여유 (CPU 점유 감소)
                }

                return coreTask.Result;
            }, ct);
        }

        public int MovePositionPickUp_Index(int nIndex = 0, bool isFine = false)
        {
            Task<int> task = MovePositionAsyncPickUp_Index(isFine, nIndex);
            while (IsEndTask(task) == false)
            {
                int nRtn = IsMoveInterLockPickUp_Index(nIndex);
                if (nRtn != 0)
                {
                    return -1;
                }

                Thread.Sleep(0);
            }
            return task.Result;
        }
        public Task<int> MovePositionAsyncPickUp_Index(bool isFine = false, int nIndex = 0)
        {
            return Task.Run(() =>
            {
                OnMovePositionPickUp_Index(isFine, nIndex);
                return 0;
            });
        }
        private int OnMovePositionPickUp_Index(bool isFine = false, int nIndex = 0)
        {
            int nRet = 0;
            if (!IsPlaceZSafetyPos() || !IsPickZSafetyPos())
            {
                nRet = MovePositionSafetyZ();
                if (nRet != 0)
                {
                    return -1;
                }
            }

            // nIndex 처리 (0-based와 1-based 모두 지원)
            //  - 1~8 : 그대로 사용 (PickUp_Index1 ~ PickUp_Index8)
            //  - 0~7 : +1 보정하여 1~8 매핑
            int teachingIdx;
            if (nIndex >= 1 && nIndex <= 8)
                teachingIdx = nIndex;
            else if (nIndex >= 0 && nIndex < 8)
                teachingIdx = nIndex + 1; // 0-based 입력으로 판단
            else
            {
                Log.Write(UnitName, $"[OnMovePositionPickUp_Index] Invalid index {nIndex}. Range 0~7 or 1~8");
                return -1;
            }

            string tpName = $"PickUp_Index{teachingIdx}";
            var tpObj = Config.GetTeachingPosition(tpName);
            if (tpObj == null)
            {
                Log.Write(UnitName, $"[OnMovePositionPickUp_Index] Teaching not found: {tpName}");
                return -1;
            }

            double dTPos = GetTP(tpName, AxisNames.RightToolT);
            nRet = MoveAxisPositionOne(AxisToolT, dTPos);
            if (nRet != 0)
            {
                Log.Write(UnitName, $"[OnMovePositionPickUp_Index] ToolT move failed tp={tpName} pos={dTPos}");
                return -1;
            }

            double dZPos = GetTP(tpName, AxisNames.RightPickZ);
            nRet = MoveAxisPositionOne(AxisPickZ, dZPos);
            if (nRet != 0)
            {
                Log.Write(UnitName, $"[OnMovePositionPickUp_Index] PickUpZ move failed tp={tpName} pos={dZPos}");
                return -1;
            }

            return nRet;
            //return MoveTeachingPositionOnce((int)InputDieTransferConfig.TeachingPositionName.Pickup, isFine);
        }
        private int IsMoveInterLockPickUp_Index(int nIndex = 0)
        {
            int nRet = 0;

            if (Rotary != null && Rotary.IsAnyAxisMoving())
            {
                AxisToolT?.EmgStop();
                AxisPickZ?.EmgStop();
                AxisPlaceZ?.EmgStop();
                PostAlarm((int)AlarmKeys.eRotaryAxesMoving);
                return -1;
            }

            return nRet;
        }
        public Task<int> MovePositionAsyncSafePickUp_Index(bool isFine = false, int nIndex = 0, CancellationToken ct = default(CancellationToken))
        {
            return Task.Run(() =>
            {
                // Task로 돌리고 별도 인터락/취소 감시
                var coreTask = Task.Run(() => OnMovePositionPickUp_Index(isFine, nIndex), ct);

                while (!IsEndTask(coreTask))
                {
                    if (ct.IsCancellationRequested)
                    {
                        try
                        {
                            AxisToolT?.EmgStop();
                            AxisPickZ?.EmgStop();
                            AxisPlaceZ?.EmgStop();
                        }
                        catch { }
                        return -999; // 취소 코드
                    }

                    int nRtn = IsMoveInterLockPickUp_Index(nIndex);
                    if (nRtn != 0)
                    {
                        return -1;
                    }

                    Thread.Sleep(5); // 0→5ms로 약간 여유 (CPU 점유 감소)
                }

                return coreTask.Result;
            }, ct);
        }


        public int MovePositionPlace(bool isFine = false)
        {
            Task<int> task = MovePositionAsyncPlace(isFine);
            while (IsEndTask(task) == false)
            {
                int nRtn = IsMoveInterLockPlace();
                if (nRtn != 0)
                {
                    return -1;
                }

                Thread.Sleep(0);
            }
            return task.Result;
        }
        public Task<int> MovePositionAsyncPlace(bool isFine = false)
        {
            return Task.Run(() =>
            {
                OnMovePositionPlace(isFine);
                return 0;
            });
        }
        private int OnMovePositionPlace(bool isFine = false)
        {
            int nRet = 0;
            if (!IsPlaceZSafetyPos() || !IsPickZSafetyPos())
            {
                nRet = MovePositionSafetyZ();
                if (nRet != 0)
                {
                    return -1;
                }
            }

            double dTPos = GetTP(OutputDieTransferConfig.TeachingPositionName.Place.ToString(),
                        AxisNames.RightToolT);
            nRet = MoveAxisPositionOne(AxisToolT, dTPos);
            if (nRet != 0)
            {
                return -1;
            }

            double dZPos = GetTP(OutputDieTransferConfig.TeachingPositionName.Place.ToString(),
                        AxisNames.RightPlaceZ);
            nRet = MoveAxisPositionOne(AxisPlaceZ, dZPos);
            if (nRet != 0)
            {
                return -1;
            }

            return nRet;
            //return MoveTeachingPositionOnce((int)InputDieTransferConfig.TeachingPositionName.Place, isFine);
        }
        private int IsMoveInterLockPlace()
        {
            int nRet = 0;
            if (OutputStage != null && OutputStage.IsAnyAxisMoving())
            {
                AxisToolT?.EmgStop();
                AxisPickZ?.EmgStop();
                AxisPlaceZ?.EmgStop();
                PostAlarm((int)AlarmKeys.eOutputStageAxesMoving);
                return -1;
            }

            if (Rotary != null && Rotary.IsAnyAxisMoving())
            {
                AxisToolT?.EmgStop();
                AxisPickZ?.EmgStop();
                AxisPlaceZ?.EmgStop();
                PostAlarm((int)AlarmKeys.eRotaryAxesMoving);
                return -1;
            }

            return nRet;
        }
        public Task<int> MovePositionAsyncSafePlace(bool isFine = false, CancellationToken ct = default(CancellationToken))
        {
            return Task.Run(() =>
            {
                // OnMovePlacePosition을 Task로 돌리고 별도 인터락/취소 감시
                var coreTask = Task.Run(() => OnMovePositionPlace(isFine), ct);

                while (!IsEndTask(coreTask))
                {
                    if (ct.IsCancellationRequested)
                    {
                        try
                        {
                            AxisToolT?.EmgStop();
                            AxisPickZ?.EmgStop();
                            AxisPlaceZ?.EmgStop();
                        }
                        catch { }
                        return -999; // 취소 코드
                    }

                    int nRtn = IsMoveInterLockPlace();
                    if (nRtn != 0)
                    {
                        return -1;
                    }

                    Thread.Sleep(5); // 0→5ms로 약간 여유 (CPU 점유 감소)
                }

                return coreTask.Result;
            }, ct);
        }



        public bool IsPickZSafetyPos(double fallbackTolerance = 0.01,
                                                 bool useAxisInposTolerance = true,
                                                 bool treatMissingAsSafe = true)
        {
            if (AxisPickZ == null)
                return treatMissingAsSafe;

            var cfg = Config;
            if (cfg == null) return false;

            // 우선순위: SafetyPos → SafetyZone
            string[] candidateNames =
            {
                "SafetyPos",
                OutputDieTransferConfig.TeachingPositionName.SafetyZone.ToString()
            };

            string foundName = null;
            foreach (var name in candidateNames)
            {
                if (cfg.GetTeachingPosition(name) != null)
                {
                    foundName = name;
                    break;
                }
            }

            if (foundName == null)
                return treatMissingAsSafe ? true : false;

            var tp = cfg.GetTeachingPosition(foundName);
            if (tp == null) return false;

            // Offset 적용 PickZ 목표값
            var (_, pickZTarget, _) = cfg.GetPositionWithOffset(foundName);

            double cur = AxisPickZ.GetPosition();
            double tol = useAxisInposTolerance
                ? (AxisPickZ.Config?.InposTolerance ?? fallbackTolerance)
                : fallbackTolerance;

            // 동일위치(=InPos) 판정
            return System.Math.Abs(cur - pickZTarget) <= tol;
        }

        /// <summary>
        /// DieTransfer ToolT 축이 SafetyPos(or SafetyZone fallback) 위치인지 확인.
        /// SafetyZone Teaching에 ToolT 값이 없으면 다음 후보로 넘어감.
        /// </summary>
        public bool IsToolTSafetyPos(double fallbackTolerance = 0.01,
                                                 bool useAxisInposTolerance = true,
                                                 bool treatMissingAsSafe = true)
        {
            if (AxisToolT == null)
                return treatMissingAsSafe;

            var cfg = Config; 
            if (cfg == null) return false;

            string[] candidateNames =
            {
                "SafetyPos",
                OutputDieTransferConfig.TeachingPositionName.SafetyZone.ToString()
            };

            string foundName = null;
            foreach (var name in candidateNames)
            {
                var tpTest = cfg.GetTeachingPosition(name);
                if (tpTest == null) continue;
                // 해당 Teaching에 ToolT 좌표가 실제 존재하는지 확인 (없으면 스킵)
                if (tpTest.AxisPositions != null &&
                    tpTest.AxisPositions.Keys.Any(k => string.Equals(k, AxisNames.LeftToolT, StringComparison.OrdinalIgnoreCase)))
                {
                    foundName = name;
                    break;
                }
            }

            if (foundName == null)
                return treatMissingAsSafe;

            var (_, _, _) = cfg.GetPositionWithOffset(foundName);
            // Offset 적용 튜플에서 t 사용
            var (tTarget, _, _) = cfg.GetPositionWithOffset(foundName);

            double cur = AxisToolT.GetPosition();
            double tol = useAxisInposTolerance
                ? (AxisToolT.Config?.InposTolerance ?? fallbackTolerance)
                : fallbackTolerance;

            return System.Math.Abs(cur - tTarget) <= tol;
        }

        /// <summary>
        /// DieTransfer PlaceZ 축이 SafetyPos(or SafetyZone fallback) 위치인지 확인.
        /// </summary>
        public bool IsPlaceZSafetyPos(double fallbackTolerance = 0.01,
                                                  bool useAxisInposTolerance = true,
                                                  bool treatMissingAsSafe = true)
        {
            if (AxisPlaceZ == null)
                return treatMissingAsSafe;

            var cfg = Config;
            if (cfg == null) 
                return false;

            string[] candidateNames =
            {
                "SafetyPos",
                OutputDieTransferConfig.TeachingPositionName.SafetyZone.ToString()
            };

            string foundName = null;
            foreach (var name in candidateNames)
            {
                if (cfg.GetTeachingPosition(name) != null)
                {
                    foundName = name;
                    break;
                }
            }

            if (foundName == null)
                return treatMissingAsSafe;

            var (_, _, placeZTarget) = cfg.GetPositionWithOffset(foundName);

            double cur = AxisPlaceZ.GetPosition();
            double tol = useAxisInposTolerance
                ? (AxisPlaceZ.Config?.InposTolerance ?? fallbackTolerance)
                : fallbackTolerance;

            return System.Math.Abs(cur - placeZTarget) <= tol;
        }



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
            {
                if (!Axes.TryGetValue(kv.Key, out var axis) || !InPos(axis, kv.Value)) 
                    return false;
            }
            return true;
        }

        public double GetTP(string tpName, string axisName)
        {
            var tp = Config.GetTeachingPosition(tpName);
            if (tp != null && tp.AxisPositions != null && tp.AxisPositions.TryGetValue(axisName, out var v)) return v;
            return 0.0;
        }
        public void MoveAxisOnce(MotionAxis ax, double target)
        {
            if (ax == null) return;
            if (System.Math.Abs(ax.GetPosition() - target) > ax.Config.InposTolerance * 3)
                ax.MoveAbs(target, ax.Config.MaxVelocity, ax.Config.RunAcc, ax.Config.RunDec, ax.Config.AccJerkPercent);
        }
        public bool InPos(MotionAxis ax, double target) => ax == null || ax.InPosition(target);
        
        #region IO Helpers (Input / Output 상태)
        public bool ReadInput(string name)
        {
            var hi = Config.HardInputs.FirstOrDefault(i => i.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (hi == null) return false;
            var eq = Equipment.Instance; var dio = eq?.DioScan; if (dio == null) return false;
            foreach (var m in eq.UnitIO.Modules)
                if (dio.TryGetInput(m.ModuleName, hi.Disp, out var v)) return v;
            return false;
        }
        public bool WriteOutput(string name, bool on)
        {
            var ho = Config.HardOutputs.FirstOrDefault(o => o.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (ho == null) return false;
            var eq = Equipment.Instance; var dio = eq?.DioScan; if (dio == null) return false;
            foreach (var m in eq.UnitIO.Modules)
                if (dio.WriteOutput(m.ModuleName, ho.Disp, on) == 0) return true;
            return false;
        }

        // 출력 캐시 상태 조회 (입력과 무관하게 실제 On/Off 표시)
        public bool IsOutputOn(string name)
        {
            var ho = Config.HardOutputs.FirstOrDefault(o => o.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (ho == null) return false;
            var eq = Equipment.Instance; var dio = eq?.DioScan; if (dio == null) return false;
            foreach (var m in eq.UnitIO.Modules)
            {
                if (dio.TryGetOutput(m.ModuleName, ho.Disp, out var v)) return v;
            }
            return false;
        }
        #endregion

        private Vacuum[] _vacuum = new Vacuum[4];              // Vacuum + OK sensor
        public Vacuum[] _blow = new Vacuum[4];
        public Vacuum[] _vent = new Vacuum[4];

        private void BindIoDomains()
        {
            var eq = Equipment.Instance; var unit = eq?.UnitIO; if (unit == null) return;

            // Vacuum 별칭으로 조회만
            if (!IoAutoBindings.Vacuums.TryGetValue("OutputDieTransferVac1", out _vacuum[0]))
            {
                Log.Write("OutputDieTransfer", "BindIoDomains", "Vacuums not found: OutputDieTransferVac1");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("OutputDieTransferVac2", out _vacuum[1]))
            {
                Log.Write("OutputDieTransfer", "BindIoDomains", "Vacuums not found: OutputDieTransferVac2");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("OutputDieTransferVac3", out _vacuum[2]))
            {
                Log.Write("OutputDieTransfer", "BindIoDomains", "Vacuums not found: OutputDieTransferVac3");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("OutputDieTransferVac4", out _vacuum[3]))
            {
                Log.Write("OutputDieTransfer", "BindIoDomains", "Vacuums not found: OutputDieTransferVac4");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("OutputDieTransferBlow1", out _blow[0]))
            {
                Log.Write("OutputDieTransfer", "BindIoDomains", "Vacuums not found: OutputDieTransferBlow1");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("OutputDieTransferBlow2", out _blow[1]))
            {
                Log.Write("OutputDieTransfer", "BindIoDomains", "Vacuums not found: OutputDieTransferBlow2");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("OutputDieTransferBlow3", out _blow[2]))
            {
                Log.Write("OutputDieTransfer", "BindIoDomains", "Vacuums not found: OutputDieTransferBlow3");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("OutputDieTransferBlow4", out _blow[3]))
            {
                Log.Write("OutputDieTransfer", "BindIoDomains", "Vacuums not found: OutputDieTransferBlow4");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("OutputDieTransferVent1", out _vent[0]))
            {
                Log.Write("OutputDieTransfer", "BindIoDomains", "Vacuums not found: OutputDieTransferVent1");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("OutputDieTransferVent2", out _vent[1]))
            {
                Log.Write("OutputDieTransfer", "BindIoDomains", "Vacuums not found: OutputDieTransferVent2");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("OutputDieTransferVent3", out _vent[2]))
            {
                Log.Write("OutputDieTransfer", "BindIoDomains", "Vacuums not found: OutputDieTransferVent3");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("OutputDieTransferVent4", out _vent[3]))
            {
                Log.Write("OutputDieTransfer", "BindIoDomains", "Vacuums not found: OutputDieTransferVent4");
            }
        }

        // === Domain Control (표준 구동) ===
        public bool SetVacuum(int nNo, bool on)
        {
            if (_vacuum[nNo] == null) return false;
            if (on) _vacuum[nNo].On();
            else _vacuum[nNo].Off();
            return true;
        }

        public bool SetBlow(int nNo, bool on)
        {
            if (_blow[nNo] == null) return false;
            if (on) _blow[nNo].On();
            else _blow[nNo].Off();
            return true;
        }

        public bool SetVent(int nNo, bool on)
        {
            if (_vent[nNo] == null) return false;
            if (on) _vent[nNo].On();
            else _vent[nNo].Off();
            return true;
        }

        #region Arm Vacuum / Blow / Vent Control
        public bool ArmFlowOk(int armIndex) => armIndex >= 0 && armIndex < ARM_FLOW.Length && ReadInput(ARM_FLOW[armIndex]);
        public bool AirTankOk() => ReadInput(AIR_TANK_PRESS);
        public bool VacuumTankOk() => ReadInput(VAC_TANK_PRESS);
        #endregion
        /// //////////////////////////////////////////////////////////////////


        public override int OnRun()
        {
            return base.OnRun();
        }

        public override int OnStop()
        {
            return base.OnStop();
        }

        protected override int OnRunReady() { return 0; }
        protected override int OnRunWork() { return 0; }
        protected override int OnRunComplete() { return 0; }


        #region Seq 단위 동작 함수
        public int ChipPickUp()
        {
            int nRet = -1;
            /* TODO */
            return nRet;
        }
        public int RotateArm()
        {
            int nRet = -1;
            /* TODO */
            return nRet;
        }

        public int ChipPlaceDown()
        {
            int nRet = -1;
            /* TODO */
            return nRet;
        }
        #endregion

    }
}