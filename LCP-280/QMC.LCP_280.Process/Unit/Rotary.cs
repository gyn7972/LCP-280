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
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static QMC.LCP_280.Process.Equipment;
using static QMC.LCP_280.Process.Unit.RotaryConfig.IO; // IO ???/?迭 ???? ???

namespace QMC.LCP_280.Process.Unit
{
    public class Rotary : BaseUnit<RotaryConfig>
    {
        public enum AlarmKeys
        {
            eIndexRotary = 4800,
            eRotaryNotSafe,
            InputDieTraansferPlaceZError,
            IndexLoadAlignerZError,
            IndexChipProbeControllerZError,
            OutputDieTransferPlaceZError,
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
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.InputDieTraansferPlaceZError;
            alarm.Title = "InputDieTraansferPlaceZ Not Sfarety Pos.";
            alarm.Cause = "InputDieTraansferPlaceZ가 안전 위치가 아닙니다.\n 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.IndexLoadAlignerZError;
            alarm.Title = "IndexLoadAlignerZ Not Sfarety Pos.";
            alarm.Cause = "IndexLoadAlignerZ가 안전 위치가 아닙니다.\n 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.IndexChipProbeControllerZError;
            alarm.Title = "IndexChipProbeControllerZ Not Sfarety Pos.";
            alarm.Cause = "IndexChipProbeControllerZ가 안전 위치가 아닙니다.\n 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.OutputDieTransferPlaceZError;
            alarm.Title = "OutputDieTransferPlaceZ Not Sfarety Pos.";
            alarm.Cause = "OutputDieTransferPlaceZ가 안전 위치가 아닙니다.\n 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);
        }
        #endregion

        #region Unit
        InputDieTransfer InputDieTransfer { get; set; }
        IndexLoadAligner IndexLoadAligner { get; set; }
        IndexChipProbeController IndexChipProbeController { get; set; }
        IndexUnloadAligner IndexUnloadAligner { get; set; }
        OutputDieTransfer OutputDieTransfer { get; set; }
        #endregion

        private MotionAxis _axisT;
        public MotionAxis AxisT => _axisT;
        private DateTime _moveStartTime;

        // Safe
        private static readonly string[] SafeNames = new[] { "SafetyZone", "Safe", "SasfeZone", "SAFE", "SAFEZONE", "SAFE_ZONE" };


        #region Socket State 관리 (간단/가독성 중심)
        // 소켓 상태 정의
        public enum RotarySocketState
        {
            Empty,
            Loading,
            Loaded,
            Aligning,
            Aligned,
            Probing,
            Probed,
            Unloading,      // UnloadAlign 동작(언로더 얼라인 공정)
            Outputting,     // OutputDieTransfer 픽/배출 공정 (새로 추가)
            Completed,
            Error
        }

        // 소켓 정보 구조
        public class SocketInfo
        {
            public int No { get; private set; }                // 0~7
            public double CenterAngleDeg { get; private set; } // 기준 중심 각도(기본 0,45,90,...)
            public RotarySocketState State;                    // 현재 상태
            public DateTime LastUpdated;
            public object Tag;                                 // 필요 시 임시 데이터(Chip ID 등)

            public SocketInfo(int no, double angleDeg)
            {
                No = no;
                CenterAngleDeg = angleDeg;
                State = RotarySocketState.Empty;
                LastUpdated = DateTime.Now;
            }

            public void SetState(RotarySocketState st)
            {
                State = st;
                LastUpdated = DateTime.Now;
            }
        }

        private SocketInfo[] _sockets;
        private readonly object _socketLock = new object();

        // 각 소켓 중심각 (기본: 360 / IndexCount * i). Teaching 오프셋 보정용
        private double _angleOffsetDeg = 0.0; // 필요 시 Teaching Position으로 셋업 가능

        // 허용 오차(현재 각도가 어느 소켓인지 판단할 때 사용)
        private const double SOCKET_MATCH_TOLERANCE_DEG = 0.1; // 기구 정밀/인덱스 정확도에 맞게 조정

        private void InitSockets()
        {
            int cnt = GetIndexCount();
            double step = 360.0 / cnt;
            _sockets = new SocketInfo[cnt];
            for (int i = 0; i < cnt; i++)
            {
                _sockets[i] = new SocketInfo(i, i * step);
            }
        }

        // 외부에서(디버그/보정) 기준 회전 오프셋 적용
        public void SetAngleOffsetDeg(double offsetDeg)
        {
            _angleOffsetDeg = NormalizeAngle(offsetDeg);
        }

        // 현재 축 위치 → 가장 가까운 소켓 번호(0~7)
        private int GetNearestSocketIndexByPosition()
        {
            if (AxisT == null)
            {
                return 0;
            }

            // 기존 코드가 AxisT.GetPosition() * 1000 사용 → 유지
            double rawDeg = AxisT.GetPosition() * 1000.0;
            double cur = NormalizeAngle(rawDeg - _angleOffsetDeg);

            double bestDiff = double.MaxValue;
            int bestIdx = 0;

            lock (_socketLock)
            {
                for (int i = 0; i < _sockets.Length; i++)
                {
                    double center = _sockets[i].CenterAngleDeg;
                    double diff = MinAngleDistance(cur, center);
                    if (diff < bestDiff)
                    {
                        bestDiff = diff;
                        bestIdx = i;
                    }
                }
            }

            // 오차 범위 밖이라면(기계 보정 필요) 그냥 근사치 반환 (로그만)
            if (bestDiff > SOCKET_MATCH_TOLERANCE_DEG)
            {
                Log.Write(UnitName, $"[GetNearestSocketIndexByPosition] Angle mismatch diff={bestDiff:0.###}deg (tol={SOCKET_MATCH_TOLERANCE_DEG})");
            }

            return bestIdx;
        }

        /// 외부 사용: 현재 로드 스테이션(Load 위치)에 존재하는 "소켓 번호(1~8)" (물리 소켓 ID +1)
        public int CurrentLoadSocketNo
        {
            get
            {
                int idx = GetLoadIndexNo();
                return idx + 1;
            }
        }

        // 소켓 상태 Get/Set
        public RotarySocketState GetSocketState(int socketNo1Based)
        {
            int idx = socketNo1Based - 1;
            if (idx < 0 || idx >= GetIndexCount())
            {
                return RotarySocketState.Error;
            }
            lock (_socketLock)
            {
                return _sockets[idx].State;
            }
        }

        public bool SetSocketState(int socketNo1Based, RotarySocketState state)
        {
            int idx = socketNo1Based - 1;
            if (idx < 0 || idx >= GetIndexCount())
            {
                return false;
            }
            lock (_socketLock)
            {
                _sockets[idx].SetState(state);
            }
            return true;
        }

        // 상태 일괄 초기화
        public void ClearAllSocketStates(RotarySocketState init = RotarySocketState.Empty)
        {
            lock (_socketLock)
            {
                foreach (var s in _sockets)
                {
                    s.SetState(init);
                }
            }
        }

        // 회전 후 스테이션별 상태 이동이 필요하다면 여기서 로직 추가 가능 (현재는 물리 소켓 고정 관리 방식)
        private void OnAfterIndexRotated(int stepLogical)
        {
            // 현재 설계: 소켓 배열은 "물리 소켓"을 표현. 회전 시 상태 재배열 불필요.
            // 만약 "스테이션 관점"으로 상태를 이동/Shift 하고 싶다면 아래 주석 해제 후 사용.
            /*
            if (stepLogical == 0) return;
            lock (_socketLock)
            {
                var list = _sockets.ToList();
                int cnt = list.Count;
                SocketInfo[] rotated = new SocketInfo[cnt];
                for (int i = 0; i < cnt; i++)
                {
                    // stepLogical > 0 : 시계 방향 한 칸 이동 시 소켓이 다음 스테이션으로 가므로 반대로 인덱스 매핑
                    int src = (i - stepLogical) % cnt;
                    if (src < 0) src += cnt;
                    rotated[i] = list[src];
                }
                _sockets = rotated;
            }
            */
        }

        private static double NormalizeAngle(double deg)
        {
            deg = deg % 360.0;
            if (deg < 0)
            {
                deg += 360.0;
            }
            return deg;
        }

        private static double MinAngleDistance(double a, double b)
        {
            double d = Math.Abs(a - b);
            return d > 180.0 ? 360.0 - d : d;
        }


        /*
         * GetLoadIndexNo()
         *  - 매우 중요: "현재 Load 위치(기계 고정 스테이션)에 물리적으로 서 있는 소켓 번호(0~7)" 를 반환
         *  - 다른 Unit 들(예: InputDieTransfer, Align, Probe 등)이 이 값을 기반으로
         *    '현재 나에게 온 소켓이 몇 번 소켓인가?' 를 판단하는 구조
         *  - 따라서 이 함수는 '스테이션 위치 Index' 가 아니라 '물리 소켓 ID' 를 반환해야 한다.
         *  - Load 위치 자체는 기구적으로 고정되어 있고 회전은 소켓이 돌아오므로
         *    서보 각도 -> 소켓ID 매핑을 통해 계산한다.
         *  - (주의) 아래 계산식의 방향(360 - dPos)은 실제 회전 방향(시계/반시계)에 따라 조정 가능
         */
        public int GetLoadIndexNo()
        {
            // 1. 축 객체 확인
            if (AxisT == null)
            {
                return 0;
            }

            // 2. 원시 위치 읽기 (논리 단위: 시뮬레이션은 그대로, 실기는 *1000 스케일 사용 중)
            double rawLogicalPosition = AxisT.GetPosition();
            double dPos = 0.0;

            if (Config.IsSimulation)
            {
                // 시뮬레이션 모드: 이미 degree 단위라고 가정
                dPos = rawLogicalPosition;
            }
            else
            {
                // 실기: 기존 코드 관례 유지 (축 값 * 1000 → degree 로 사용)
                dPos = rawLogicalPosition * 1000.0;
            }

            // 3. (선택) 방향 반전 필요 시 설정
            //    - 현재 장비에서 CCW(반시계) 증가가 0→1→2 로 진행된다면 true 유지
            //    - 만약 증가 방향이 반대라면 false 로 바꾸거나 Config 플래그로 치환
            bool invertDirection = true;
            if (invertDirection)
            {
                dPos = 360.0 - dPos;
            }

            // 4. 기계적 0점 보정 (Teaching 등으로 세팅된 _angleOffsetDeg 적용)
            dPos = NormalizeAngle(dPos - _angleOffsetDeg);

            // 5. 인덱스 계산 준비
            int count = GetIndexCount();          // 예: 8
            double step = 360.0 / count;          // 예: 45도

            // 6. 중앙 기준 라운딩: 경계 근처(예 44.9 / 45.1) 안정화 위해 half-step 이동 후 Floor
            double shifted = dPos + (step / 2.0);

            // 7. 임시 인덱스 산출
            int index = (int)Math.Floor(shifted / step);

            // 8. 범위 정규화 (wrap)
            if (index >= count)
            {
                index -= count;
            }
            if (index < 0)
            {
                index += count;
            }

            // 9. 결과(물리 소켓 ID: 0 ~ count-1)
            return index;
        }

        public int GetIndexCount()
        {
            return 8;
        }
        #endregion

        #region Socket Helper (추가 Refactoring)
        /*
         * 추가된 Helper 함수들
         *  - 다른 로직에서 스테이션 Offset 기반 접근을 반복 작성하지 않도록 캡슐화
         *  - "Load 기준 Offset" 을 넣으면 그 위치에 '현재 물리적으로 서 있는 소켓의 물리 ID(0~7)' 를 반환
         *  - 단, 소켓 상태 배열은 물리 ID 순서이므로 상태 접근 시 (index = 물리ID)
         */
        private int GetPhysicalSocketIndexAtStationOffset(int stationOffset)
        {
            int loadPhysical = GetLoadIndexNo(); // 현재 Load에 있는 물리 소켓 ID
            int count = GetIndexCount();

            int idx = loadPhysical + stationOffset;
            idx = idx % count;
            if (idx < 0)
            {
                idx += count;
            }
            return idx;
        }

        private SocketInfo GetSocketInfoAtStationOffset(int stationOffset)
        {
            int physicalIdx = GetPhysicalSocketIndexAtStationOffset(stationOffset);
            lock (_socketLock)
            {
                return _sockets[physicalIdx];
            }
        }

        // 1-based 소켓 번호 (UI 용)
        public int GetLoadSocketNo1Based()
        {
            int val = GetLoadIndexNo();
            val = val + 1;
            return val;
        }

        // 디버깅용: 스테이션 offset 별 현재 소켓/상태 문자열
        public string GetStationsSnapshot(params int[] stationOffsets)
        {
            if (stationOffsets == null || stationOffsets.Length == 0)
            {
                return string.Empty;
            }

            List<string> list = new List<string>();
            foreach (var off in stationOffsets)
            {
                int pIdx = GetPhysicalSocketIndexAtStationOffset(off);
                RotarySocketState st;
                lock (_socketLock)
                {
                    st = _sockets[pIdx].State;
                }
                list.Add($"Off{off}:Sock{pIdx + 1}[{st}]");
            }
            string joined = string.Join(", ", list);
            return joined;
        }
        #endregion



        public Rotary(RotaryConfig config = null) : base(new RotaryConfig())
        {

            AddComponents();
        }

        public override void AddComponents()
        {
            Config.LoadAndBindAxes(Equipment.Instance.AxisManager);
            Config.InitializeDefaultTeachingPositions();

            BindAxes();
            BindIoDomains();
            InitSockets();

            var il = InterlockManager.Instance;
            il.AddAxisMustBeHomed("RotaryTHomed", _axisT, "T?? Home ??? ?? ???? ????????.");
            il.AddGlobalRule("EquipStateRunningBlock", () =>
            {
                return Equipment.Instance != null && Equipment.Instance.EqState == EquipmentState.Running
                    ? "??????? ????? ?ε??? ???? ????? ???????." : null;
            });
        }

        protected override void OnBindUnit()
        {
            base.OnBindUnit();
            InputDieTransfer = Equipment.Instance.GetUnit(UnitKeys.InputDieTransfer) as InputDieTransfer;
            IndexLoadAligner = Equipment.Instance.GetUnit(UnitKeys.IndexLoadAligner) as IndexLoadAligner;
            IndexChipProbeController = Equipment.Instance.GetUnit(UnitKeys.IndexChipProbeController) as IndexChipProbeController;
            IndexUnloadAligner = Equipment.Instance.GetUnit(UnitKeys.IndexUnloadAligner) as IndexUnloadAligner;
            OutputDieTransfer = Equipment.Instance.GetUnit(UnitKeys.OutputDieTransfer) as OutputDieTransfer;
        }

        private void BindAxes()
        {
            //AxisNames.IndexT
            var mgr = Equipment.Instance?.AxisManager;
            if (mgr == null)
            {
                Log.Write("Rotary", "[BindAxes] AxisManager null");
                return;
            }

            const string unitName = "Unit"; // Equipment???? ?? ??? ?? ????? ?????? ??????? ??
            BindAxis(mgr, unitName, AxisNames.IndexT, ref _axisT);

        }

        
        #region Teaching
        public void TeachCurrentPosition(string name, string description = null)
        {
            var pos = new Dictionary<string, double>();
            foreach (var kv in Axes) pos[kv.Key] = kv.Value.GetPosition();
            Config.SetTeachingPosition(new TeachingPosition(name, pos, description));
        }

        public int MoveToTeachingPosition(string name, double vel = 0, double acc = 0, double dec = 0, double jerk = 0)
        {
            var tp = Config.GetTeachingPosition(name); 
            if (tp == null) 
                return -1;

            double t = Config.GetPositionWithOffset(name);
            if (_axisT == null) 
                return -2;

            int nRtn = 0;

            //Todo : 인터락 확인 후 이동 하도록 수정.
            //nRtn =  _axisT.MoveAbs(t,
            //    vel > 0 ? vel : _axisT.Config.MaxVelocity,
            //    acc > 0 ? acc : _axisT.Config.RunAcc,
            //    dec > 0 ? dec : _axisT.Config.RunDec,
            //    jerk > 0 ? jerk : _axisT.Config.AccJerkPercent);

            return nRtn; 
        }

        public bool InPosTeaching(string name)
        {
            double t = Config.GetPositionWithOffset(name);
            return InPos(_axisT, t);
        }

        public void ApplyOffset(string name, double deltaT) => Config.SetOffset(name, deltaT);
        #endregion

        #region Axis helpers
        public bool InPos(MotionAxis ax, double target) => ax == null || ax.InPosition(target);
        #endregion

        #region Index Move (with Interlock)
        public bool TryMoveIndexPrev(out string reason)
        {
            return TryMoveIndexStep(-1, out reason);
        }

        public bool TryMoveIndexNext(out string reason)
        {
            return TryMoveIndexStep(+1, out reason);
        }

        private bool TryMoveIndexStep(int step, out string reason)
        {
            reason = null;
            var axis = _axisT;
            if (axis == null)
            {
                reason = "TAxis Null.";
                return false;
            }

            // 1) Safe-Zone check.
            if (!VerifyAllUnitsSafe(out reason))
                return false;

            // 3) Move Check.
            int rc = step < 0 ? axis.MovePrevIndex() : axis.MoveNextIndex();
            if (rc != 0)
            {
                reason = $"Index ??? ????(rc={rc})";
                return false;
            }

            _moveStartTime = DateTime.Now;
            return true;
        }

        private bool IsIndexMoveDone()
        {
            if (AxisT == null) 
                return true;

            if (IsAxisMoving(AxisNames.IndexT)) 
                return true;

            if ((DateTime.Now - _moveStartTime).TotalMilliseconds > AxisT.Setup.MoveTimeoutMs)
            {
                Log.Write("Rotary", "Index Move Timeout");
                return true;
            }
            return false;
        }

        // 인덱스 이동 완료 대기 (성공:0, 타임아웃:-1)
        public int WaitIndexMoveDone(int timeoutMs = -1, int pollMs = 5)
        {
            if (AxisT == null) 
                return -1;

            if (timeoutMs <= 0)
            {
                // Setup 없으면 기본 20000
                timeoutMs = (AxisT.Setup != null && AxisT.Setup.MoveTimeoutMs > 0)
                    ? AxisT.Setup.MoveTimeoutMs
                    : 20000;
            }
            Thread.Sleep(100);
            var start = DateTime.Now;
            while (true)
            {
                if (IsAxisMoving(AxisNames.IndexT))
                {
                    return 0; // 완료
                }

                if ((DateTime.Now - start).TotalMilliseconds > timeoutMs)
                {
                    Log.Write(UnitName, $"Index Move Timeout (>{timeoutMs} ms)");
                    return -1;
                }
                Thread.Sleep(pollMs);
            }
        }

        private bool VerifyAllUnitsSafe(out string reason)
        {
            reason = null;
            var eq = Equipment.Instance;
            if (eq == null || eq.Units == null) return true;

            // InputDieTransfer
            if (eq.Units.TryGetValue("InputDieTransfer", out var u3))
            {
                if (!IsUnitAxisInSafetyZone(u3, AxisNames.LeftPlaceZ, out var r))
                {
                    AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.InputDieTraansferPlaceZError);
                    reason = "InputDieTransfer Not in Safety Zone";
                    return false;
                }
            }

            // IndexLoadAligner
            if (eq.Units.TryGetValue("IndexLoadAligner", out var u2))
            {
                if (!IsUnitInSafeByConnectedAxes(u2))
                {
                    AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.IndexLoadAlignerZError);
                    reason = "IndexLoadAligner Not in Safety Zone";
                    return false;
                }
            }

            // IndexChipProbeController
            if (eq.Units.TryGetValue("IndexChipProbeController", out var u1))
            {
                if (!IsUnitInSafeByConnectedAxes(u1))
                {
                    AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.IndexChipProbeControllerZError);
                    reason = "IndexChipProbeController Not in Safety Zone";
                    return false;
                }
            }

            // OutputDieTransfer
            if (eq.Units.TryGetValue("OutputDieTransfer", out var u4))
            {
                if (!IsUnitAxisInSafetyZone(u4, AxisNames.RightPickZ, out var r4))
                {
                    AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.OutputDieTransferPlaceZError);
                    reason = "OutputDieTransfer Not in Safety Zone";
                    return false;
                }
            }

            return true;
        }

        private bool IsUnitInSafeByConnectedAxes(object unit)
        {
            if (unit == null) 
                return true;

            // Config(BaseConfig) 획득
            var t = unit.GetType();
            var propConfig = t.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .FirstOrDefault(p => p.Name == "Config" && typeof(BaseConfig).IsAssignableFrom(p.PropertyType));
            var cfg = propConfig?.GetValue(unit) as BaseConfig;
            if (cfg?.TeachingPositions == null) return true;


            // 유닛 보유 축 사전(Dictionary<string, MotionAxis>) 획득
            var propAxes = t.GetProperty("Axes");
            var unitAxes = propAxes?.GetValue(unit) as System.Collections.Generic.IDictionary<string, MotionAxis>;

            foreach (var safeName in SafeNames)
            {
                var tp = cfg.TeachingPositions.FirstOrDefault(p => string.Equals(p.Name, safeName, StringComparison.OrdinalIgnoreCase));
                if (tp == null) continue;

                // TeachingPosition의 바인딩된 축 사전 (Dictionary<string, MotionAxis>) 리플렉션으로 접근
                System.Collections.Generic.IDictionary<string, MotionAxis> tpAxes = null;
                try
                {
                    var tpAxesProp = tp.GetType().GetProperty("Axes");
                    tpAxes = tpAxesProp?.GetValue(tp) as System.Collections.Generic.IDictionary<string, MotionAxis>;
                }
                catch { /* ignore */ }

                bool ok = true;
                int checkedAny = 0;

                foreach (var kv in tp.AxisPositions)
                {
                    string axisKey = kv.Key;
                    double target = kv.Value;

                    MotionAxis axis = null;

                    // 1) TeachingPosition에 바인딩된 축 우선
                    if (tpAxes != null)
                    {
                        tpAxes.TryGetValue(axisKey, out axis);
                    }

                    // 2) 유닛 보유 축에서 키/이름으로 검색
                    if (axis == null && unitAxes != null)
                    {
                        if (!unitAxes.TryGetValue(axisKey, out axis))
                        {
                            axis = unitAxes.Values.FirstOrDefault(a => a != null && string.Equals(a.Name, axisKey, StringComparison.OrdinalIgnoreCase));
                        }
                    }

                    // 연결되지 않은 축은 비교 대상에서 제외
                    if (axis == null) continue;

                    checkedAny++;
                    try
                    {
                        if (!axis.InPosition(target))
                        {
                            ok = false;
                            break;
                        }
                    }
                    catch
                    {
                        ok = false;
                        break;
                    }
                }

                // 바인딩된 축이 하나도 없으면 안전으로 간주(필요 시 false로 변경 가능)
                if (ok && (checkedAny == 0 || checkedAny > 0))
                    return true;
            }

            return false;
        }

        // 지정 축만 SafetyZone TeachingPosition으로 확인
        private bool IsUnitAxisInSafetyZone(object unit, string axisName, out string reason)
        {
            reason = null;
            if (unit == null) { reason = "Unit null"; return false; }

            // Config(BaseConfig)
            var t = unit.GetType();
            var propConfig = t.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .FirstOrDefault(p => p.Name == "Config" && typeof(BaseConfig).IsAssignableFrom(p.PropertyType));
            var cfg = propConfig?.GetValue(unit) as BaseConfig;
            if (cfg?.TeachingPositions == null)
            {
                reason = "TeachingPositions not found";
                return false;
            }

            // 유닛 보유 축 사전
            var propAxes = t.GetProperty("Axes");
            var unitAxes = propAxes?.GetValue(unit) as System.Collections.Generic.IDictionary<string, MotionAxis>;

            foreach (var safeName in SafeNames)
            {
                var tp = cfg.TeachingPositions.FirstOrDefault(p => string.Equals(p.Name, safeName, StringComparison.OrdinalIgnoreCase));
                if (tp == null) continue;

                // 목표 위치 찾기 (축 키 케이스 무시)
                double target;
                bool hasTarget = false;
                if (tp.AxisPositions.TryGetValue(axisName, out target))
                {
                    hasTarget = true;
                }
                else
                {
                    var kv = tp.AxisPositions.FirstOrDefault(k => string.Equals(k.Key, axisName, StringComparison.OrdinalIgnoreCase));
                    if (kv.Key != null)
                    {
                        target = kv.Value;
                        hasTarget = true;
                    }
                }

                if (!hasTarget)
                {
                    reason = $"SafetyZone target not found for '{axisName}'";
                    return false;
                }

                // TeachingPosition에 바인딩된 축 사전
                System.Collections.Generic.IDictionary<string, MotionAxis> tpAxes = null;
                try
                {
                    var tpAxesProp = tp.GetType().GetProperty("Axes");
                    tpAxes = tpAxesProp?.GetValue(tp) as System.Collections.Generic.IDictionary<string, MotionAxis>;
                }
                catch { /* ignore */ }

                MotionAxis axis = null;

                // 1) TeachingPosition 바인딩에서 우선 검색
                if (tpAxes != null)
                {
                    if (!tpAxes.TryGetValue(axisName, out axis))
                    {
                        axis = tpAxes.Values.FirstOrDefault(a => a != null && string.Equals(a.Name, axisName, StringComparison.OrdinalIgnoreCase));
                    }
                }

                // 2) 유닛 보유 축에서 검색
                if (axis == null && unitAxes != null)
                {
                    if (!unitAxes.TryGetValue(axisName, out axis))
                    {
                        axis = unitAxes.Values.FirstOrDefault(a => a != null && string.Equals(a.Name, axisName, StringComparison.OrdinalIgnoreCase));
                    }
                }

                if (axis == null)
                {
                    reason = $"Axis not bound: '{axisName}'";
                    return false;
                }

                try
                {
                    if (!axis.InPosition(target))
                    {
                        reason = $"'{axisName}' not in SafetyZone";
                        return false;
                    }
                }
                catch
                {
                    reason = $"'{axisName}' safety check failed";
                    return false;
                }

                // 지정 축만 확인 성공
                return true;
            }

            reason = "SafetyZone TeachingPosition not found";
            return false;
        }

        private bool IsUnitInSafe(System.Func<string, bool> inPosTeaching)
        {
            for (int i = 0; i < SafeNames.Length; i++)
                if (inPosTeaching(SafeNames[i])) return true;
            return false;
        }
        #endregion

        #region IO Helpers
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

        private Vacuum[] _vacuum = new Vacuum[8];              // Vacuum + OK sensor
        public Vacuum[] _blow = new Vacuum[8];
        public Vacuum[] _vent = new Vacuum[8];

        private void BindIoDomains()
        {
            var eq = Equipment.Instance; var unit = eq?.UnitIO; if (unit == null) return;

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyVac1", out _vacuum[0]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyVac1");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyVac2", out _vacuum[1]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyVac2");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyVac3", out _vacuum[2]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyVac3");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyVac4", out _vacuum[3]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyVac4");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyVac5", out _vacuum[4]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyVac5");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyVac6", out _vacuum[5]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyVac6");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyVac7", out _vacuum[6]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyVac7");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyVac8", out _vacuum[7]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyVac8");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyBlow1", out _blow[0]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyBlow1");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyBlow2", out _blow[1]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyBlow2");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyBlow3", out _blow[2]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyBlow3");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyBlow4", out _blow[3]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyBlow4");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyBlow5", out _blow[4]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyBlow5");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyBlow6", out _blow[5]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyBlow6");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyBlow7", out _blow[6]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyBlow7");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyBlow8", out _blow[7]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyBlow8");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyVent1", out _vent[0]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyVent1");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyVent2", out _vent[1]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyVent2");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyVent3", out _vent[2]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyVent3");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyVent4", out _vent[3]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyVent4");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyVent5", out _vent[4]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyVent5");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyVent6", out _vent[5]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyVent6");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyVent7", out _vent[6]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyVent7");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyVent8", out _vent[7]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyVent8");
            }

        }

        // === Domain Control (??? ????) ===
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

        public bool SlotFlowOk(int slotIndex) => slotIndex >= 0 && slotIndex < FLOW.Length && ReadInput(FLOW[slotIndex]);

        #region Pressure
        public bool AirTankPressureOk() => ReadInput(AIR_TANK_PRESSURE);
        public bool VacTankPressureOk() => ReadInput(VAC_TANK_PRESSURE) || ReadInput(VAC_TANK_PRESSURE_LEGACY);
        #endregion


        #region Seq Signal
        public bool RequestInputDieTrDie { get; set; } = false;
        // Thread-safe 플래그 (기존 이름 유지해 외부 영향 최소화)
        private int _reqLoadAligner;
        public bool RequestLoadAligner
        {
            get; set;
            //get { return System.Threading.Thread.VolatileRead(ref _reqLoadAligner) != 0; }
            //set { System.Threading.Interlocked.Exchange(ref _reqLoadAligner, value ? 1 : 0); }
        }

        private int _reqProbe;
        public bool RequestProbe
        {
            get; set;
            //get { return System.Threading.Thread.VolatileRead(ref _reqProbe) != 0; }
            //set { System.Threading.Interlocked.Exchange(ref _reqProbe, value ? 1 : 0); }
        }

        private int _reqUnloadAligner;
        public bool RequestUnloaderAligner
        {
            get; set;
            //get { return System.Threading.Thread.VolatileRead(ref _reqUnloadAligner) != 0; }
            //set { System.Threading.Interlocked.Exchange(ref _reqUnloadAligner, value ? 1 : 0); }
        }
        public bool RequestOutputDieTrDie { get; set; } = false;
        #endregion

        // 추가: 필요한 Unit 실행 보조
        private void TryStartUnitIfNeeded(BaseUnit unit)
        {
            if (unit == null) return;
            if (unit.RunUnitStatus == BaseUnit.UnitStatus.Running ||
                unit.RunUnitStatus == BaseUnit.UnitStatus.Starting)
                return;

            Equipment.Instance.StartUnitSync(unit.UnitName);
        }

        private void TryStopUnitIfNeeded(BaseUnit unit)
        {
            if (unit == null) return;
            if (unit.RunUnitStatus == BaseUnit.UnitStatus.Stopped ||
                unit.RunUnitStatus == BaseUnit.UnitStatus.Stopping ||
                unit.RunUnitStatus == BaseUnit.UnitStatus.CycleStop)
                return;

            Equipment.Instance.StopUnitSync(unit.UnitName);
        }

        public override int OnRun()
        {
            int nRtn = 0;

            if (this.RunUnitStatus == UnitStatus.Stopped ||
                this.RunUnitStatus == UnitStatus.Stopping ||
                this.RunUnitStatus == UnitStatus.CycleStop)
            {
                this.State = ProcessState.Stop;
                return 1;
            }

            switch (State)
            {
                case ProcessState.Ready:
                    nRtn = OnRunReady();
                    break;
                case ProcessState.Work:
                    nRtn = OnRunWork();
                    break;
                case ProcessState.Complete:
                    nRtn = OnRunComplete();
                    break;
                default:
                    RequestInputDieTrDie = false;
                    //RequestLoadAligner = false;
                    //RequestProbe = false;
                    //RequestUnloaderAligner = false;
                    RequestOutputDieTrDie = false;
                    this.State = ProcessState.Ready;
                    break;
            }
            if (nRtn != 0)
            {
                this.State = ProcessState.Stop;
                this.OnStop();
            }

            return nRtn;
        }
        public override int OnStop()
        {
            int ret = 0;

            TryStopUnitIfNeeded(IndexLoadAligner);
            TryStopUnitIfNeeded(IndexChipProbeController);
            TryStopUnitIfNeeded(IndexUnloadAligner);

            //TryStopUnitIfNeeded(InputDieTransfer);
            //TryStopUnitIfNeeded(OutputDieTransfer);

            this.RunUnitStatus = UnitStatus.Stopped;
            this.State = ProcessState.Stop;
            base.OnStop();
            return ret;
        }
        protected override int OnRunReady() 
        {
            int nRtn = 0;

            //TryStartUnitIfNeeded(IndexLoadAligner);
            //TryStartUnitIfNeeded(IndexChipProbeController);
            //TryStartUnitIfNeeded(IndexUnloadAligner);
            //TryStartUnitIfNeeded(InputDieTransfer);
            //TryStartUnitIfNeeded(OutputDieTransfer);

            // (추가) 공정 상태 갱신
            UpdateProcessStates();

            // 1. 회전 가능 여부 판단
            string rotateReason = string.Empty;
            int chk = CanRotate();
            switch (chk)
            {
                case ROT_CHK_OK:
                    State = ProcessState.Work;
                    return nRtn;

                case ROT_CHK_SKIP_NO_DEMAND:
                    RequestInputDieTrDie = true;
                    RequestLoadAligner = true;
                    RequestProbe = true;
                    RequestUnloaderAligner = true;
                    State = ProcessState.Complete;
                    return nRtn;

                case ROT_CHK_WAIT_STATION_BUSY:
                    // 아직 공정 진행 중 → 대기 (로그 과다 방지 위해 필요 시 주석 해제)
                    // Log.Write(UnitName, "[RotateWait] Station busy");
                    State = ProcessState.Ready;
                    return nRtn;

                case ROT_CHK_ERR_AXIS_NULL:
                case ROT_CHK_ERR_AXIS_BUSY:
                case ROT_CHK_ERR_NOT_SAFE:
                case ROT_CHK_ERR_SOCKET_ARRAY:
                default:
                    Log.Write(UnitName, $"[RotateError] {GetRotateCheckMessage(chk)}");
                    return -1;
            }
        }
        protected override int OnRunWork() 
        {
            int nRtn = 0;

            if (Rotate() != 0)
            {
                Log.Write(UnitName, "[Rotate] Failed");
                return -1;
            }

            nRtn = ExecuteUnitLoadMAlign();
            if (nRtn != 0)
            {
                return nRtn;
            }



            IndexLoadAligner.CompleteLoadAligner = true;
            IndexChipProbeController.CompleteProbe = true;
            IndexUnloadAligner.CompleteUnloadAligner = true;

            // 회전 수행 후 다음 단계 요청 신호 셋업
            RequestInputDieTrDie = true;
            //RequestLoadAligner = true;
            //RequestProbe = true;
            //RequestUnloaderAligner = true;
            State = ProcessState.Complete;

            return nRtn;
        }

        protected override int OnRunComplete() 
        {
            int nRtn = 0;

            if (IsAxisMoving(AxisNames.IndexT))
            {
                return 0;
            }
            
            if (!Config.IsSimulation)
            {
                if (IndexUnloadAligner.CompleteUnloadAligner)
                {
                    RequestOutputDieTrDie = true;
                }
            }
            else
            {
                RequestOutputDieTrDie = true;
            }
            if (!Config.IsSimulation)
            {
                if (InputDieTransfer.CompleteInputDie &&
                //IndexLoadAligner.CompleteLoadAligner &&
                //IndexChipProbeController.CompleteProbe &&
                //IndexUnloadAligner.CompleteUnloadAligner &&
                OutputDieTransfer.CompleteOutputDie)
                {
                    // 3. 회전 후 소켓 상태 전이 (예: Load -> Loading 등)
                    PostRotateStateTransition();

                    // (추가) 공정 상태 갱신
                    UpdateProcessStates();

                    Thread.Sleep(2000); // 시뮬레이션용 대기
                    State = ProcessState.None;
                }
            }
            else
            {
                InputDieTransfer.CompleteInputDie = true;
                //IndexLoadAligner.CompleteLoadAligner = true;
                //IndexChipProbeController.CompleteProbe = true;
                //IndexUnloadAligner.CompleteUnloadAligner = true;
                OutputDieTransfer.CompleteOutputDie = true;

                if (InputDieTransfer.CompleteInputDie &&
                //IndexLoadAligner.CompleteLoadAligner &&
                //IndexChipProbeController.CompleteProbe &&
                //IndexUnloadAligner.CompleteUnloadAligner &&
                OutputDieTransfer.CompleteOutputDie)
                {
                    // 3. 회전 후 소켓 상태 전이 (예: Load -> Loading 등)
                    PostRotateStateTransition();

                    // (추가) 공정 상태 갱신
                    UpdateProcessStates();

                    //Thread.Sleep(2000); // 시뮬레이션용 대기
                    State = ProcessState.None;
                }
            }

            return nRtn; 
        }

        protected override void OnMakeSequence()
        {
            base.OnMakeSequence();

            this.SequencePlayers.Add(CanRotate);
            this.SequencePlayers.Add(Rotate);
            this.SequencePlayers.Add(ExecuteUnitLoadDie);
            this.SequencePlayers.Add(ExecuteUnitLoadMAlign);
            this.SequencePlayers.Add(ExecuteUnitProbe);
            this.SequencePlayers.Add(ExecuteUnitUnloadAlign);
            this.SequencePlayers.Add(ExecuteUnitUnLoadDie);
        }

        #region Seq 함수
        // ====== Station / Socket 연계 정의 (제품 유무 + Unit Complete 조합으로 회전 인터락 판단) ======
        private class StationRule
        {
            public string Name;
            public int Offset; // Load 기준 상대 인덱스
            public Func<bool> IsUnitComplete;
            public RotarySocketState[] BlockingStates;
            public bool ProductRequired;
        }
        private StationRule[] _stationRules;
        private void InitStationRules()
        {
            const int LOAD_OFFSET = 0;
            const int ALIGN_OFFSET = 1;
            const int PROBE_OFFSET = 2;
            const int UNLOAD_OUTPUT_OFFSET = 4;

            _stationRules = new[]
            {
                new StationRule {
                    Name = "Load",
                    Offset = LOAD_OFFSET,
                    IsUnitComplete = () => InputDieTransfer == null || InputDieTransfer.CompleteInputDie,
                    BlockingStates = new[]{ RotarySocketState.Loading, RotarySocketState.Loaded },
                    ProductRequired = true
                },
                new StationRule {
                    Name = "Align",
                    Offset = ALIGN_OFFSET,
                    IsUnitComplete = () => IndexLoadAligner == null || IndexLoadAligner.CompleteLoadAligner,
                    BlockingStates = new[]{ RotarySocketState.Aligning },
                    ProductRequired = true
                },
                new StationRule {
                    Name = "Probe",
                    Offset = PROBE_OFFSET,
                    IsUnitComplete = () => IndexChipProbeController == null || IndexChipProbeController.CompleteProbe,
                    BlockingStates = new[]{ RotarySocketState.Probing },
                    ProductRequired = true
                },
                new StationRule {
                    Name = "UnloadOutput",
                    Offset = UNLOAD_OUTPUT_OFFSET,
                    IsUnitComplete = () =>
                    {
                        int loadIdx = GetLoadIndexNo();
                        int idx = (loadIdx + UNLOAD_OUTPUT_OFFSET) % GetIndexCount();
                        RotarySocketState curState;
                        lock (_socketLock) curState = _sockets[idx].State;

                        if (curState == RotarySocketState.Unloading)
                        {
                            return IndexUnloadAligner == null || IndexUnloadAligner.CompleteUnloadAligner;
                        }
                        if (curState == RotarySocketState.Outputting)
                        {
                            return OutputDieTransfer == null || OutputDieTransfer.CompleteOutputDie;
                        }
                        return true;
                    },
                    BlockingStates = new[]{ RotarySocketState.Unloading, RotarySocketState.Outputting },
                    ProductRequired = true
                }
            };
        }
        private bool HasProduct(RotarySocketState st)
        {
            return st != RotarySocketState.Empty;
        }

        // - Unit Complete 신호를 무조건 보지 않고:
        // "소켓에 제품이 있고 그 소켓이 해당 스테이션에서 아직 처리 상태
        // (BlockingStates)에 속하며 Unit Complete == false" 인 경우에만 BLOCK
        // ==== Rotate Check Codes ====
        private const int ROT_CHK_OK = 0;
        private const int ROT_CHK_SKIP_NO_DEMAND = 1;
        private const int ROT_CHK_WAIT_STATION_BUSY = 2;
        private const int ROT_CHK_ERR_AXIS_NULL = -1;
        private const int ROT_CHK_ERR_AXIS_BUSY = -2;
        private const int ROT_CHK_ERR_NOT_SAFE = -3;
        private const int ROT_CHK_ERR_SOCKET_ARRAY = -4;
        private string GetRotateCheckMessage(int code)
        {
            switch (code)
            {
                case ROT_CHK_OK: return "OK";
                case ROT_CHK_SKIP_NO_DEMAND: return "No rotation demand";
                case ROT_CHK_WAIT_STATION_BUSY: return "Station processing";
                case ROT_CHK_ERR_AXIS_NULL: return "AxisT NULL";
                case ROT_CHK_ERR_AXIS_BUSY: return "AxisT Moving/Busy";
                case ROT_CHK_ERR_NOT_SAFE: return "Not Safe";
                case ROT_CHK_ERR_SOCKET_ARRAY: return "Socket array NULL";
                default: return $"Unknown({code})";
            }
        }


        // ====== 추가: 스테이션 오프셋 상수 (기존 InitStationRules 와 동일하게 유지) ======
        private const int STATION_OFFSET_LOAD = 0;
        private const int STATION_OFFSET_ALIGN = 1;
        private const int STATION_OFFSET_PROBE = 2;
        private const int STATION_OFFSET_UNLOAD_OUTPUT = 4;

        // (신규) 공정 상태 자동 갱신
        private void UpdateProcessStates()
        {
            if (_sockets == null) return;

            int loadIdx = GetLoadIndexNo();
            int alignIdx = IndexLoadAligner.GetAlignIndexNo();          //GetPhysicalSocketIndexAtStationOffset(STATION_OFFSET_ALIGN);
            int probeIdx = IndexChipProbeController.GetProbeIndexNo();  //GetPhysicalSocketIndexAtStationOffset(STATION_OFFSET_PROBE);

            RotarySocketState loadState, alignState, probeState;

            lock (_socketLock)
            {
                loadState = _sockets[loadIdx].State;
                alignState = _sockets[alignIdx].State;
                probeState = _sockets[probeIdx].State;
            }

            // 1) Load 스테이션: (현재 PostRotateStateTransition 에서 Empty→Loaded 처리 중)
            //    필요 시 Loading 단계 분리하려면 InputDieTransfer 동작 중일 때 Loading 세팅 로직 추가 가능.

            // 2) Align 스테이션 상태 전이
            if (alignState == RotarySocketState.Loaded)
            {
                // 아직 Align 동작 시작 안했고 Align Unit 이 처리 가능 상태라면 시작
                //if (IndexLoadAligner != null && !IndexLoadAligner.CompleteLoadAligner)
                if (IndexLoadAligner != null)// && IndexLoadAligner.CompleteLoadAligner)
                {
                    lock (_socketLock)
                    {
                        if (_sockets[alignIdx].State == RotarySocketState.Loaded)
                        {
                            _sockets[alignIdx].SetState(RotarySocketState.Aligned);
                            //RequestLoadAligner = true;
                        }
                    }
                }
            }
            //else if (alignState == RotarySocketState.Aligning)
            //{
            //    if (IndexLoadAligner == null || IndexLoadAligner.CompleteLoadAligner)
            //    {
            //        lock (_socketLock)
            //        {
            //            if (_sockets[alignIdx].State == RotarySocketState.Aligning)
            //            {
            //                _sockets[alignIdx].SetState(RotarySocketState.Aligned);
            //                RequestLoadAligner = false;
            //            }
            //        }
            //    }
            //}

            // 3) Probe 스테이션 상태 전이
            if (probeState == RotarySocketState.Aligned)
            {
                if (IndexChipProbeController != null)// && IndexChipProbeController.CompleteProbe)
                {
                    lock (_socketLock)
                    {
                        if (_sockets[probeIdx].State == RotarySocketState.Aligned)
                        {
                            _sockets[probeIdx].SetState(RotarySocketState.Probed);
                            //RequestProbe = true;
                        }
                    }
                }
            }
            //else if (probeState == RotarySocketState.Probing)
            //{
            //    if (IndexChipProbeController == null || IndexChipProbeController.CompleteProbe)
            //    {
            //        lock (_socketLock)
            //        {
            //            if (_sockets[probeIdx].State == RotarySocketState.Probing)
            //            {
            //                _sockets[probeIdx].SetState(RotarySocketState.Probed);
            //                RequestProbe = false;
            //            }
            //        }
            //    }
            //}
            
            // 4) Unload/Output 스테이션은 기존 UpdateUnloadOutputComposite() 호출로 상태 전이 관리(Probed 이후)
            UpdateUnloadOutputComposite();
        }

        private int CanRotate(out string reason)
        {
            reason = null;

            // (추가) 회전 판단 전 상태 전이 갱신
            UpdateProcessStates();

            if (_axisT == null)
            {
                PostAlarm((int)AlarmKeys.eIndexRotary);
                reason = "AxisT NULL";
                return ROT_CHK_ERR_AXIS_NULL;
            }
            if (IsAxisMoving(AxisNames.IndexT))
            {
                PostAlarm((int)AlarmKeys.eRotaryNotSafe);
                reason = "AxisT Busy";
                return ROT_CHK_ERR_AXIS_BUSY;
            }

            if (!VerifyAllUnitsSafe(out reason))
            {
                PostAlarm((int)AlarmKeys.eRotaryNotSafe);
                reason = "Not Safe: " + reason;
                return ROT_CHK_ERR_NOT_SAFE;
            }

            if (_sockets == null)
            {
                PostAlarm((int)AlarmKeys.eIndexRotary);
                reason = "Socket array NULL";
                return ROT_CHK_ERR_SOCKET_ARRAY;
            }
            if (_stationRules == null)
            {
                InitStationRules();
            }

            int loadIdx = GetLoadIndexNo();
            int cnt = GetIndexCount();

            // 스테이션별 인터락
            foreach (var rule in _stationRules)
            {
                int idx = (loadIdx + rule.Offset) % cnt;
                if (idx < 0)
                {
                    idx += cnt;
                }

                RotarySocketState st;
                lock (_socketLock)
                {
                    st = _sockets[idx].State;
                }

                bool productExists = HasProduct(st);

                if (rule.ProductRequired && !productExists)
                {
                    // 제품 없음 → 이 스테이션은 회전 BLOCK 조건에서 제외
                    continue;
                }

                // 소켓 상태가 아직 처리 중(BlockingStates)에 포함되고, Unit Complete 가 false 면 BLOCK
                if (rule.BlockingStates.Length > 0 &&
                    rule.BlockingStates.Contains(st) &&
                    !rule.IsUnitComplete())
                {
                    reason = $"{rule.Name} Processing (State={st})";
                    return ROT_CHK_WAIT_STATION_BUSY;
                }
            }

            // 회전 필요 판단
            if (!NeedRotate(out var needReason))
            {
                reason = needReason;
                return ROT_CHK_SKIP_NO_DEMAND;
            }

            return ROT_CHK_OK;
        }

        // 회전 필요 판단 (통합 스테이션 반영)
        // 회전 필요 판단 (Align 단계를 포함한 정밀 조건 반영)
        private bool NeedRotate(out string reason)
        {
            reason = null;

            if (_sockets == null)
            {
                reason = "Sockets NULL";
                return false;
            }
            if (_stationRules == null)
                InitStationRules();

            // 실제 각 스테이션의 '현재 물리 소켓 인덱스' (유닛 제공 인덱스가 우선)
            int loadIdx = GetLoadIndexNo();

            int alignIdx = -1;
            if (IndexLoadAligner != null)
            {
                try { alignIdx = IndexLoadAligner.GetAlignIndexNo(); } catch { alignIdx = -1; }
            }
            if (alignIdx < 0) // fallback
                alignIdx = (loadIdx + 1) % GetIndexCount();

            int probeIdx = -1;
            if (IndexChipProbeController != null)
            {
                try { probeIdx = IndexChipProbeController.GetProbeIndexNo(); } catch { probeIdx = -1; }
            }
            if (probeIdx < 0)
                probeIdx = (loadIdx + 2) % GetIndexCount();

            int unloadIdx = -1;
            if (IndexUnloadAligner != null)
            {
                try { unloadIdx = IndexUnloadAligner.GetUnloadIndexNo(); } catch { unloadIdx = -1; }
            }
            if (unloadIdx < 0)
                unloadIdx = (loadIdx + 4) % GetIndexCount();

            RotarySocketState loadState, alignState, probeState, unloadState;
            lock (_socketLock)
            {
                loadState = _sockets[loadIdx].State;
                alignState = _sockets[alignIdx].State;
                probeState = _sockets[probeIdx].State;
                unloadState = _sockets[unloadIdx].State;
            }

            var loadRule = _stationRules.First(r => r.Name == "Load");
            var alignRule = _stationRules.First(r => r.Name == "Align");
            var probeRule = _stationRules.First(r => r.Name == "Probe");
            var unloadRule = _stationRules.First(r => r.Name == "UnloadOutput");

            // 우선순위 정의 (상위 스테이션이 먼저 비워져야 파이프라인 흐름 최대화)
            // 1) Unload/Output 비워 전진 필요
            // 2) Probe -> Unload/Output
            // 3) Align -> Probe
            // 4) Load -> Align
            // 필요 시 정책 조정 가능
            // 조건 충족하는 첫 항목 즉시 회전 true 반환 (여러 개 동시에 충족되더라도 회전은 1회)

            // Unload/Output Completed → 다음 사이클
            if (unloadState == RotarySocketState.Completed)
            {
                reason = "Unload/Output 완료 → 사이클 진행";
                return true;
            }

            // Probe → Unload (Unloading/Outputting/Completed/Empty 만 수용)
            if (probeState == RotarySocketState.Probed &&
                (unloadState == RotarySocketState.Empty ||
                 unloadState == RotarySocketState.Completed))
            {
                reason = "Probe -> Unload/Output 이송";
                return true;
            }

            // Align → Probe
            if (alignState == RotarySocketState.Aligned &&
                alignRule.IsUnitComplete() &&
                (probeState == RotarySocketState.Empty ||
                 probeState == RotarySocketState.Completed))
            {
                reason = "Align -> Probe 이송";
                return true;
            }

            // Load → Align
            if (loadState == RotarySocketState.Loaded &&
                loadRule.IsUnitComplete() &&
                (alignState == RotarySocketState.Empty ||
                 alignState == RotarySocketState.Completed))
            {
                reason = "Load -> Align 이송";
                return true;
            }

            // 회전 수요 없음
            reason = "No rotation demand";
            return false;
        }

        //private bool NeedRotate(out string reason)
        //{
        //    reason = null;

        //    if (_sockets == null)
        //    {
        //        reason = "Sockets NULL";
        //        return false;
        //    }
        //    if (_stationRules == null)
        //    {
        //        InitStationRules();
        //    }

        //    int loadIdx = GetLoadIndexNo();
        //    int alignIdx = IndexLoadAligner.GetAlignIndexNo();              //  (loadIdx + 1) % GetIndexCount(); // STATION_OFFSET_ALIGN
        //    int probeIdx = IndexChipProbeController.GetProbeIndexNo();      //(loadIdx + 2) % GetIndexCount(); // STATION_OFFSET_PROBE
        //    int unloadOutputIdx = IndexUnloadAligner.GetUnloadIndexNo();    //(loadIdx + 4) % GetIndexCount(); // STATION_OFFSET_UNLOAD_OUTPUT

        //    var loadState = _sockets[loadIdx].State;
        //    var alignState = _sockets[alignIdx].State;
        //    var probeState = _sockets[probeIdx].State;
        //    var unloadOutState = _sockets[unloadOutputIdx].State;

        //    var loadRule = _stationRules.First(r => r.Name == "Load");
        //    var alignRule = _stationRules.First(r => r.Name == "Align");
        //    var probeRule = _stationRules.First(r => r.Name == "Probe");
        //    var unloadOutputRule = _stationRules.First(r => r.Name == "UnloadOutput");

        //    // 0) 잘못된 기존 로직 정리:
        //    //  - (기존) Load 위치 Empty → 바로 회전 (X)
        //    //    => Empty 이면 그냥 그 자리에서 투입 진행해야 함. 회전하면 투입 기회를 잃음.
        //    //    => 따라서 'Empty' 자체는 회전 트리거가 아님.

        //    // 1) Load 단계 완료 → Align 위치로 이송 필요
        //    //    조건:
        //    //      - Load 소켓 상태가 Loaded (Loading 은 아직 진행중이므로 불가)
        //    //      - Load Unit 완료 신호 (LoadRule.IsUnitComplete())
        //    //      - Align 위치가 비어 있거나(Empty) / 이전 제품 처리가 끝난 상태(Completed)
        //    //      - Align 위치가 아직 Aligning/Aligned/Probing 등으로 점유 중이면 대기
        //    if (loadState == RotarySocketState.Loaded &&
        //        loadRule.IsUnitComplete() &&
        //        (alignState == RotarySocketState.Empty || alignState == RotarySocketState.Completed))
        //    {
        //        reason = "Load -> Align 이송";
        //        return true;
        //    }

        //    // 2) Align 단계 완료 → Probe 위치로 이송 필요
        //    //    조건:
        //    //      - Align 소켓이 Aligned 상태
        //    //      - Align Unit 완료 (alignRule.IsUnitComplete())
        //    //      - Probe 위치가 비어있거나(Empty) / 이전 결과가 정리된 상태(Completed)
        //    //      - Probe 위치가 Probing/Probed(대기 중 UnloadOutput이 안비었음) 이면 대기
        //    if (alignState == RotarySocketState.Aligned &&
        //        alignRule.IsUnitComplete() &&
        //        (probeState == RotarySocketState.Empty || probeState == RotarySocketState.Completed))
        //    {
        //        reason = "Align -> Probe 이송";
        //        return true;
        //    }

        //    // 3) Probe 단계 완료 → Unload/Output 통합 위치로 이송 필요
        //    //    조건:
        //    //      - Probe 소켓이 Probed
        //    //      - 통합 스테이션(4) 이 Empty 또는 Completed (Completed 는 다음 제품 받아도 됨)
        //    if (probeState == RotarySocketState.Probed &&
        //        (unloadOutState == RotarySocketState.Empty || unloadOutState == RotarySocketState.Completed))
        //    {
        //        reason = "Probe -> Unload/Output 이송";
        //        return true;
        //    }

        //    // 4) Unload/Output 통합 스테이션 완료 → 제품 배출 반영 후 다음 공정 사이클 진행
        //    //    조건:
        //    //      - 통합 스테이션 소켓 상태 Completed
        //    //      - (선택) Completed 후 일정 시간 경과 or 배출 보고 여부 등을 추가 가능
        //    if (unloadOutState == RotarySocketState.Completed)
        //    {
        //        reason = "Unload/Output 완료 → 다음 사이클";
        //        return true;
        //    }

        //    // 5) 예외: 초기 모든 소켓 Empty 이고 첫 제품을 투입해야 하는데 Load 위치가 이미 Empty → 회전 불필요
        //    reason = "No rotation demand";
        //    return false;
        //}

        // 통합 스테이션 상태 전이 처리 (주기적으로 호출)
        // - 위치: Load 기준 +4 (InitStationRules 의 UNLOAD_OUTPUT_OFFSET 과 동일해야 함)
        // - 기대 흐름:
        //      Probe 스테이션에서 Probed -> 회전 -> (통합 위치 도착) 여전히 Probed 상태
        //      1) Probed  상태에서 Unloading 시작 조건 충족 시  Unloading 진입 (RequestUnloaderAligner = true)
        //      2) Unloading 완료(IndexUnloadAligner.CompleteUnloadAligner==true) -> Outputting (RequestOutputDieTrDie = true)
        //      3) Outputting 완료(OutputDieTransfer.CompleteOutputDie==true)    -> Completed
        //      4) Completed 는 이후 회전 시 Empty 로 재사용 (정책에 따라 즉시 Empty 로 바꿀 수도 있음)
        private void UpdateUnloadOutputComposite()
        {
            if (_sockets == null)
            {
                return;
            }

            if (_stationRules == null)
            {
                InitStationRules();
            }


            // InitStationRules 내부 상수와 동일하게 유지
            const int UNLOAD_OUTPUT_OFFSET = 4;

            int loadIdx = GetLoadIndexNo();
            int idx = IndexUnloadAligner.GetUnloadIndexNo();  //(loadIdx + UNLOAD_OUTPUT_OFFSET) % GetIndexCount();

            RotarySocketState state;
            lock (_socketLock)
            {
                state = _sockets[idx].State;
            }

            if (state == RotarySocketState.Probed)
            {
                lock (_socketLock)
                {
                    _sockets[idx].SetState(RotarySocketState.Unloading);
                    //RequestUnloaderAligner = true;
                }
                return;
            }

            if (state == RotarySocketState.Unloading)
            {
                if (IndexUnloadAligner == null)// || IndexUnloadAligner.CompleteUnloadAligner)
                {
                    lock (_socketLock)
                    {
                        _sockets[idx].SetState(RotarySocketState.Outputting);
                        //RequestUnloaderAligner = false;
                        //RequestOutputDieTrDie = true;
                    }
                }
                return;
            }

            if (state == RotarySocketState.Outputting)
            {
                if (OutputDieTransfer == null || OutputDieTransfer.CompleteOutputDie)
                {
                    lock (_socketLock)
                    {
                        _sockets[idx].SetState(RotarySocketState.Completed);
                        //RequestOutputDieTrDie = false;
                    }
                }
                return;
            }

            if (state == RotarySocketState.Completed)
            {
                lock (_socketLock)
                {
                    var s = _sockets[idx];
                    //if ((DateTime.Now - s.LastUpdated).TotalSeconds > 5)
                    {
                        s.SetState(RotarySocketState.Empty);
                    }
                }
            }
        }

        private void PostRotateStateTransition()
        {
            if (_sockets == null)
            {
                return;
            }

            int loadIdx = GetLoadIndexNo();
            lock (_socketLock)
            {
                var s = _sockets[loadIdx];
                //if (s.State == RotarySocketState.Empty && RequestInputDieTrDie)
                if (s.State == RotarySocketState.Empty && 
                    InputDieTransfer.CompleteInputDie)
                {
                    //s.SetState(RotarySocketState.Loading);
                    s.SetState(RotarySocketState.Loaded);
                }
            }
        }


        public int CanRotate(bool isFine = false)
        {
            int nRet = 0;

            this.CurrentFunc = CanRotate;

            UpdateUnloadOutputComposite();

            string reason;
            int chk = CanRotate(out reason);
            if (chk == ROT_CHK_OK)
            {
                nRet = 0;
            }
            else if (chk == ROT_CHK_SKIP_NO_DEMAND)
            {
                nRet = 1;
            }
            else if (chk == ROT_CHK_WAIT_STATION_BUSY)
            {
                nRet = 2;
            }
            else
            {
                Log.Write(UnitName, $"CanRotate Error: {GetRotateCheckMessage(chk)}");
                nRet = -1;
                return nRet;
            }

            return nRet;
        }

        public int Rotate(bool isFine = false)
        {
            int nRet = 0;

            this.CurrentFunc = Rotate;

            string reason;
            if (!TryMoveIndexNext(out reason))
            {
                // 재시도 루프(로그만)
                Log.Write(UnitName, $"TryMoveIndexNext Fail: {reason}");
                Thread.Sleep(50);
                return -1;
            }

            nRet = WaitIndexMoveDone();
            if (nRet != 0)
            {
                // 필요 시 Alarm 발생 가능
                // RaiseAlarm((int)AlarmKeys.eIndexRotary, "Index Move Timeout");
                PostAlarm((int)AlarmKeys.eRotaryNotSafe);
                return -1;
            }

            // 3. 회전 후 소켓 상태 전이 (예: Load -> Loading 등)
            PostRotateStateTransition();
            return nRet;
        }
        
        
        
        //사전에 Unit 상태 및 안전 위치 확인 함수.
        public int IsExecuteUnitLoadDie()
        {
            int nRet = 0;

            //InputDieTr는 작업여부 상태신호 보자. //밖에서 확인하고 들어오게 하자.
            if (InputDieTransfer.IsWork())
            {
                return -1; // 대기 인디.
            }

            return nRet;
        }
        public int ExecuteUnitLoadDie(bool isFine = false)
        {
            int nRtn = 0;
            this.CurrentFunc = ExecuteUnitLoadDie;

            RequestInputDieTrDie = true; // InputDieTransfer에 Chip 요청 상태로 변경.

            return nRtn;
        }
        public int ExecuteUnitLoadMAlign(bool isFine = false)
        {
            this.CurrentFunc = ExecuteUnitLoadMAlign;

            Task<int> task = ExecuteUnitAsyncLoadMAlign(isFine);
            while (IsEndTask(task) == false)
            {
                ExecuteUnitActionInterlockLoadMAlign(isFine);
                Thread.Sleep(1);
            }
            return task.Result;
        }
        public Task<int> ExecuteUnitAsyncLoadMAlign(bool isFine = false)
        {
            return Task.Run(() =>
            {
                OnExecuteUnitLoadMAlign(isFine);
                return 0;
            });
        }
        public int OnExecuteUnitLoadMAlign(bool isFine = false)
        {
            int nRet = 0;

            nRet &= IndexLoadAligner.AlignSocketOnceReady();
            if (nRet != 0)
            {
                Log.Write(UnitName, "ExecuteUnitAction Ready Fail");
                return -1;
            }

            nRet &= IndexLoadAligner.AlignSocketOnce();
            if (nRet != 0)
            {
                Log.Write(UnitName, "ExecuteUnitAction Fail");
                return -1;
            }

            return nRet;
        }
        public int ExecuteUnitActionInterlockLoadMAlign(bool isFine = false)
        {
            int nRet = 0;


            return nRet;
        }
        public int ExecuteUnitProbe(bool isFine = false)
        {
            this.CurrentFunc = ExecuteUnitProbe;

            Task<int> task = ExecuteUnitAsyncProbe(isFine);
            while (IsEndTask(task) == false)
            {
                ExecuteUnitActionInterlockProbe(isFine);
                Thread.Sleep(1);
            }
            return task.Result;
        }
        public Task<int> ExecuteUnitAsyncProbe(bool isFine = false)
        {
            return Task.Run(() =>
            {
                OnExecuteUnitProbe(isFine);
                return 0;
            });
        }
        public int OnExecuteUnitProbe(bool isFine = false)
        {
            int nRet = 0;

            RequestInputDieTrDie = true; // InputDieTransfer에 Chip 요청 상태로 변경.

            nRet &= IndexChipProbeController.ContactReady();
            nRet &= IndexUnloadAligner.AlignSocketOnceReady();
            if (nRet != 0)
            {
                Log.Write(UnitName, "ExecuteUnitAction Ready Fail");
                return -1;
            }

            nRet &= IndexChipProbeController.ContactBottomOrTop();
            nRet &= IndexUnloadAligner.AlignSocketOnce();
            if (nRet != 0)
            {
                Log.Write(UnitName, "ExecuteUnitAction Fail");
                return -1;
            }

            return nRet;
        }
        public int ExecuteUnitActionInterlockProbe(bool isFine = false)
        {
            int nRet = 0;


            return nRet;
        }
        public int ExecuteUnitUnloadAlign(bool isFine = false)
        {
            this.CurrentFunc = ExecuteUnitUnloadAlign;

            Task<int> task = ExecuteUnitAsyncUnloadAlign(isFine);
            while (IsEndTask(task) == false)
            {
                ExecuteUnitInterlockUnloadAlign(isFine);
                Thread.Sleep(1);
            }
            return task.Result;
        }
        public Task<int> ExecuteUnitAsyncUnloadAlign(bool isFine = false)
        {
            return Task.Run(() =>
            {
                OnExecuteUnitUnloadAlign(isFine);
                return 0;
            });
        }
        public int OnExecuteUnitUnloadAlign(bool isFine = false)
        {
            int nRet = 0;

            RequestInputDieTrDie = true; // InputDieTransfer에 Chip 요청 상태로 변경.

            nRet &= IndexUnloadAligner.AlignSocketOnceReady();
            if (nRet != 0)
            {
                Log.Write(UnitName, "ExecuteUnitAction Ready Fail");
                return -1;
            }

            nRet &= IndexUnloadAligner.AlignSocketOnce();
            if (nRet != 0)
            {
                Log.Write(UnitName, "ExecuteUnitAction Fail");
                return -1;
            }

            return nRet;
        }
        public int ExecuteUnitInterlockUnloadAlign(bool isFine = false)
        {
            int nRet = 0;


            return nRet;
        }
        public int ExecuteUnitUnLoadDie(bool isFine = false)
        {
            int nRtn = 0;
            this.CurrentFunc = ExecuteUnitUnLoadDie;

            RequestOutputDieTrDie = true;

            return nRtn;
        }

        #endregion
    }
}