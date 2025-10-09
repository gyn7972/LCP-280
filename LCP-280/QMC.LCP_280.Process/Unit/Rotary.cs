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
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static QMC.LCP_280.Process.Equipment;
using static QMC.LCP_280.Process.Unit.RotaryConfig.IO; // 

namespace QMC.LCP_280.Process.Unit
{
    public class Rotary : BaseUnit<RotaryConfig>
    {
        // === Load 인덱스 변경 이벤트 (UI 연동용) ===
        public delegate void LoadIndexChangedHandler(object sender, int loadIndex0Based);
        public event LoadIndexChangedHandler LoadIndexChanged;
        protected virtual void OnLoadIndexChanged(int loadIndex0Based)
        {
            LoadIndexChangedHandler handler = this.LoadIndexChanged;
            if (handler != null)
            {
                handler(this, loadIndex0Based);
            }
        }


        public enum AlarmKeys
        {
            eIndexRotary = 4800,
            eRotaryNotSafe,
            InputDieTraansferPlaceZError,
            IndexLoadAlignerZError,
            IndexChipProbeControllerZError,
            OutputDieTransferPlaceZError,
            InputDieTransferTimeout,
            RotaryIndexMoveError,
            eOutputDieTransferTimeout,
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

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.InputDieTransferTimeout;
            alarm.Title = "InputDieTransfer Timeout";
            alarm.Cause = "InputDieTransfer Place 동작이 Timeout 되었습니다.\n 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.RotaryIndexMoveError;
            alarm.Title = "Rotary Index Move Error";
            alarm.Cause = "Rotary Index Move 중 Error가 발생하였습니다.\n 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eOutputDieTransferTimeout;  
            alarm.Title = "OutputDieTransfer Timeout";
            alarm.Cause = "OutputDieTransfer Place 동작이 Timeout 되었습니다.\n 포지션 확인 후 다시 시작 하십시요.";
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
            public bool UseSocket;
            private MaterialDie _material;
            public void SetMaterialDie(MaterialDie die) => _material = die;
            public MaterialDie GetMaterialDie() => _material;
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

        // (클래스 상단 SocketInfo 정의 아래 혹은 같은 Region 내부 적절한 위치에 추가)
        private void RefreshSocketUsage()
        {
            if (_sockets == null) 
                return;
            // Config 값 ↔ 소켓 인덱스 매핑
            for (int i = 0; i < _sockets.Length; i++)
            {
                bool use = false;
                switch (i)
                {
                    case 0: use = Config.UseSocket1; break;
                    case 1: use = Config.UseSocket2; break;
                    case 2: use = Config.UseSocket3; break;
                    case 3: use = Config.UseSocket4; break;
                    case 4: use = Config.UseSocket5; break;
                    case 5: use = Config.UseSocket6; break;
                    case 6: use = Config.UseSocket7; break;
                    case 7: use = Config.UseSocket8; break;
                }
                _sockets[i].UseSocket = use;
                // 비활성 소켓이면 상태를 Empty 로 유지 (또는 Completed 로 표시해 파이프라인 진행 가속 가능)
                if (!use)
                {
                    // 파이프라인 로직이 Empty 를 회전 필요 조건으로 오해하지 않도록 Completed 로 두고 싶다면 아래 한 줄 교체:
                    //_sockets[i].SetState(RotarySocketState.Completed);
                    _sockets[i].SetState(RotarySocketState.Empty);
                    _sockets[i].SetMaterialDie(null);
                }
            }
        }

        private void InitSockets()
        {
            int cnt = GetIndexCount();
            double step = 360.0 / cnt;
            _sockets = new SocketInfo[cnt];
            for (int i = 0; i < cnt; i++)
            {
                _sockets[i] = new SocketInfo(i, i * step);
            }
            RefreshSocketUsage(); // ← 추가: Config 기반 소켓 사용여부 반영
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

        #region Socket Public Accessors
        public SocketInfo GetSocket(int no)
        {
            lock (_socketLock)
            {
                if (_sockets == null) return null;
                if (no < 0 || no >= _sockets.Length) return null;
                return _sockets[no]; // 참조 반환 (UI가 상태 변화를 즉시 반영 가능)
            }
        }

        public SocketInfo[] GetAllSockets()
        {
            lock (_socketLock)
            {
                if (_sockets == null) return new SocketInfo[0];
                // 원본 참조 배열 그대로 반환 (변경 감지 필요하면 ToArray()로 복사 가능)
                return _sockets;
            }
        }
        #endregion


        public override void SetMaterial(Material m)
        {
            var socket = GetLoadSocketInfo();
            socket.SetMaterialDie  (m as MaterialDie);
            //base.SetMaterial(m);
        }
        public MaterialDie GetLoadSocketMaterial()
        {
            var socket = GetLoadSocketInfo();
            return socket.GetMaterialDie();
        }
        public SocketInfo GetLoadSocketInfo()
        {
            int idx = GetLoadIndexNo();
            lock (_socketLock)
            {
                var die = _sockets[idx].GetMaterialDie();
                if (die == null)
                {
                    _sockets[idx].SetMaterialDie(new MaterialDie());
                }

                return _sockets[idx];
            }
        }
        public MaterialDie GetMAlignSocketMaterial()
        {
            SocketInfo socket = GetMAlignSocketInfo();
            return socket.GetMaterialDie();
        }
        private SocketInfo GetMAlignSocketInfo()
        {
            int idx = IndexLoadAligner.GetAlignIndexNo();
            lock (_socketLock)
            {
                return _sockets[idx];
            }
        }
        public MaterialDie GetProbeSocketMaterial()
        {
            SocketInfo socket = GetProbeSocketInfo();
            return socket.GetMaterialDie();
        }
        private SocketInfo GetProbeSocketInfo()
        {
            int idx = IndexChipProbeController.GetProbeIndexNo();
            lock (_socketLock)
            {
                return _sockets[idx];
            }   
        }
        public MaterialDie GetUnloaderAlignSocketMaterial()
        {
            SocketInfo socket = GetUnloaderAlignSocketInfo();
            return socket.GetMaterialDie();
        }
        private SocketInfo GetUnloaderAlignSocketInfo()
        {
            int idx = IndexUnloadAligner.GetUnloaderAlignIndexNo();
            lock (_socketLock)
            {
                return _sockets[idx];
            }
        }
        public MaterialDie GetUnloadSocketMaterial()
        {
            SocketInfo socket = GetUnloadSocketInfo();
            return socket.GetMaterialDie();
        }
        private SocketInfo GetUnloadSocketInfo()
        {
            int idx = OutputDieTransfer.GetUnloaderIndexNo();
            lock (_socketLock)
            {
                return _sockets[idx];
            }
        }


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

        //public bool InPosTeaching(string name)
        //{
        //    double t = Config.GetPositionWithOffset(name);
        //    return InPos(_axisT, t);
        //}

        public void ApplyOffset(string name, double deltaT) => Config.SetOffset(name, deltaT);
        #endregion

        #region Axis helpers
        //public bool InPos(MotionAxis ax, double target) => ax == null || ax.InPosition(target);
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
            bool bRtn = false;

            reason = null;
            var axis = _axisT;
            if (axis == null)
            {
                reason = "TAxis Null.";
                return false;
            }

            if(RunUnitStatus != UnitStatus.Running)
            {
                // 1) Safe-Zone check.
                if (!VerifyAllUnitsSafe(out reason))
                {
                    Log.Write("Rotary", $"Index Move Blocked: {reason}");
                    return false;
                }
            }

            // 3) Move Check.
            int rc = step < 0 ? axis.MovePrevIndex() : axis.MoveNextIndex();
            if (rc != 0)
            {
                reason = $"Index ??? ????(rc={rc})";
                return false;
            }

            _moveStartTime = DateTime.Now;

            // (변경) 이동 완료 후에 이벤트 발생하도록 비동기 처리
            Task.Run(() =>
            {
                int wrc = WaitIndexMoveDone();
                if (wrc == 0)
                {
                    try
                    {
                        OnLoadIndexChanged(GetLoadIndexNo());
                    }
                    catch (Exception ex)
                    {
                        Log.Write("Rotary", $"LoadIndexChanged dispatch fail: {ex.Message}");
                    }
                }
                else
                {
                    Log.Write("Rotary", $"Index move wait timeout/err (rc={wrc})");
                }
            });

            return true;
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
                // 이동 중이면 계속 대기
                if (!IsAxisMoving(AxisNames.IndexT))
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

        public int PollIntervalMs { get; set; } = 30;
        private int WaitUntil(Func<bool> cond, int timeoutMs)
        {
            int nRtn = 0;
            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                if (cond()) return nRtn;
                Thread.Sleep(PollIntervalMs);
            }

            nRtn = 0;
            return nRtn;
        }
        private bool VerifyAllUnitsSafe(out string reason)
        {
            reason = null;
            var eq = Equipment.Instance;
            if (eq == null || eq.Units == null) 
                return true;

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

        public bool IsVacuumOk(int slotIndex)
        {
            if (FLOW == null)
            {
                return false;
            }

            if (slotIndex < 0)
            {
                return false;
            }

            if (slotIndex >= FLOW.Length)
            {
                return false;
            }

            if(Config.IsSimulation || Config.IsDryRun)
            {
                return true;
            }

            return this.ReadInput(FLOW[slotIndex]);
        }

        #region Pressure
        public bool AirTankPressureOk()
        {
            return this.ReadInput(AIR_TANK_PRESSURE);
        }
        public bool VacTankPressureOk()
        {
           return this.ReadInput(VAC_TANK_PRESSURE) || this.ReadInput(VAC_TANK_PRESSURE_LEGACY);
        }
        #endregion

        #region Seq Signal
        public bool RequestInputDieTrDie { get; set; } = false;
        public bool DoneInputDieTrDie { get; set; } = false;
        #endregion


        // 모든 사용 소켓이 비어있는지 검사 (사용 설정된 소켓만 대상)
        private bool IsAllUsedSocketsEmpty()
        {
            if (_sockets == null) return true;
            lock (_socketLock)
            {
                foreach (var s in _sockets)
                {
                    if (!s.UseSocket) continue;
                    var die = s.GetMaterialDie();
                    if (die != null && die.Presence == Material.MaterialPresence.Exist)
                        return false; // 하나라도 존재
                }
                return true;
            }
        }

        public override int OnRun()
        {
            int nRtn = 0;

            if (this.RunUnitStatus == UnitStatus.Stopped ||
                this.RunUnitStatus == UnitStatus.Stopping ||
                this.RunUnitStatus == UnitStatus.CycleStop)
            {
                this.State = ProcessState.Stop;
                return -1;
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

        protected override int OnStart()
        {
            this.IndexLoadAligner.Start();
            this.IndexChipProbeController.Start();
            this.IndexUnloadAligner.Start();
            
            return base.OnStart();
        }

        public override int OnStop()
        {
            int ret = 0;

            this.RunUnitStatus = UnitStatus.Stopped;
            this.State = ProcessState.Stop;

            IndexLoadAligner?.OnStop();
            IndexChipProbeController?.OnStop();
            IndexUnloadAligner?.OnStop();

            base.OnStop();
            return ret;
        }
        protected override int OnRunReady() 
        {
            int nRet = 0;
            if (IsAxisMoving(AxisNames.IndexT))
            {
                return 0;
            }

            nRet = ExecuteUnitActionReady();
            if (nRet != 0)
            {
                Log.Write(UnitName, "[ExecuteUnitActionReady] Failed");
                return -1;
            }

            State = ProcessState.Work;
            return nRet;

        }
        protected override int OnRunWork() 
        {
            int nRet = 0;

            int nIndex = GetLoadIndexNo();
            bool useSocket = this.Config.GetUseSocket(nIndex);

            // === INPUT (Load 위치) 처리 영역 (DryRun 단순화) =========================
            RequestInputDieTrDie = false;

            if (Config.IsUnitDryRun)
            {
                // DryRun: InputDieTransfer 와의 인터페이스 없이 즉시 소켓에 Die 존재 상태를 시뮬레이션
                var socket = GetLoadSocketInfo();
                var die = socket.GetMaterialDie();
                if (die == null)
                {
                    die = new MaterialDie();
                    socket.SetMaterialDie(die);
                }
                if (useSocket)
                {
                    // 존재하지 않으면 바로 채워 넣음
                    if (die.Presence != Material.MaterialPresence.Exist)
                    {
                        die.Presence = Material.MaterialPresence.Exist;
                        die.ProcessSatate = Material.MaterialProcessSatate.Ready;
                        socket.SetState(RotarySocketState.Loaded);
                    }
                }
                // DryRun에서는 InputDieTransfer 상태/Complete 여부를 보지 않음
            }
            else
            {
                if (this.InputDieTransfer != null 
                 && this.InputDieTransfer.State == ProcessState.Complete)
                {
                    var die = GetLoadSocketMaterial();
                    if (die != null)
                    {
                        if (die.Presence != Material.MaterialPresence.Exist)
                        {
                            if (useSocket)
                            {
                                RequestInputDieTrDie = true;
                            }
                        }
                    }
                }
                else
                {
                    // 2) (기존 로직 교체) : "사용 중인 현재 Load 소켓이 비어있으면 절대 돌지 않는다"
                    // (주의) 다른 소켓이 비어있어도 '현재 Load 위치 소켓' 이 이미 로딩되어 있다면 공정/회전 진행.
                    // 요구사항: "소켓을 사용중인데(현재 위치) 로딩 안되어 있으면 돌면 안돼" 에 맞춘 최소 제한.
                    var loadSock = GetLoadSocketInfo();
                    var loadDie = loadSock.GetMaterialDie();
                    bool needLoad = useSocket &&
                                    (loadDie == null || loadDie.Presence != Material.MaterialPresence.Exist);
                    if (needLoad)
                    {
                        RequestInputDieTrDie = true;
                        return 0; // 아직 로딩 안됨 → 회전/후속 공정 금지
                    }
                    // 요구사항:
                    // 1) 사용(Enable)된 소켓 중 하나라도 제품(Exist)이 있으면 → 이후 공정(Align/Probe/Unload)을 순차 진행
                    // 2) 사용 소켓 모두 비어있으면 → 제품이 투입될 때까지 대기 (회전/공정 진행 X)
                    //if (IsAllUsedSocketsEmpty())
                    //{
                    //    // 제품이 전혀 없으므로 투입 대기.
                    //    // Load 위치 소켓이 사용 가능하면 투입 요청 플래그를 올려 InputDieTransfer 가 준비될 때 픽업하도록 함.
                    //    if (useSocket)
                    //        RequestInputDieTrDie = true;

                    //    // 진행을 중단하고 다음 OnRunWork 사이클에서 다시 검사
                    //    return 0;
                    //}
                }
            }
            
            nRet = ExecuteUnitAction();
            if (nRet != 0)
            {
                Log.Write(UnitName, "[ExecuteUnitAction] Failed");
                return -1;
            }

            // 여기 블록(Load 투입 대기 + Unloader 배출 확인)이 확실히 완료된 다음에만 Rotate
            bool needLoadWait = (RequestInputDieTrDie == true) && useSocket;
            nRet = WaitPostActionSettled(needLoadWait, 60000 * 1000);
            if (nRet != 0)
            {
                Log.Write(UnitName, "[WaitPostActionSettled] Failed");
                return -1;
            }
            // 투입 완료되었으면 요청 플래그 내림
            RequestInputDieTrDie = false;

            // 5) 회전 전 최종 안전 조건:
            //    - 현재 Load 소켓이 사용중 && 아직도 비어있다면 회전 금지 (이중 방어)
            var finalLoadSock = GetLoadSocketInfo();
            var finalDie = finalLoadSock.GetMaterialDie();
            if (useSocket && (finalDie == null || finalDie.Presence != Material.MaterialPresence.Exist))
            {
                // 예상치 못하게 아직 로딩 안됨 → 다시 로딩 시도
                RequestInputDieTrDie = true;
                return 0;
            }

            nRet = Rotate();
            if (nRet != 0)
            {
                PostAlarm((int)AlarmKeys.RotaryIndexMoveError);
                Log.Write(UnitName, "[Rotate] Failed");
                return nRet;
            }

            
            State = ProcessState.Complete;

            return nRet;
        }
        protected override int OnRunComplete() 
        {
            int nRtn = 0;

            if (IsAxisMoving(AxisNames.IndexT))
            {
                return 0;
            }

            State = ProcessState.None;
            return nRtn; 
        }

        protected override void OnMakeSequence()
        {
            base.OnMakeSequence();

            //this.SequencePlayers.Add(CanRotate);
            this.SequencePlayers.Add(ExecuteUnitActionReady);
            this.SequencePlayers.Add(Rotate);
            this.SequencePlayers.Add(ExecuteUnitAction);
        }

        #region Auto Seq 함수
        public int ExecuteUnitActionReady(bool isFine = false)
        {
            int nRtn = 0;
            if (RunMode == UnitRunMode.Manual)
            {
                this.CurrentFunc = ExecuteUnitActionReady;
            }

            Task<int> task = ExecuteUnitActionReadyAsync(isFine);
            while (IsEndTask(task) == false)
            {
                if (IsStop) { return 0; }

                ExecuteUnitActionInterlockLoadMAlign();
                ExecuteUnitActionInterlockProbe();
                //interlock
                Thread.Sleep(1);
            }
            return task.Result;
        }
        protected Task<int> ExecuteUnitActionReadyAsync(bool isFine = false)
        {
            return Task.Run(() =>
            {
                OnExecuteUnitActionReady(isFine);
                return 0;
            });
        }
        protected int OnExecuteUnitActionReady(bool isFine = false)
        {
            try
            {
                var t1 = (IndexLoadAligner != null)
                    ? Task.Run(() =>
                    {
                        var th = Thread.CurrentThread;
                        if (th.Name == null)
                        {
                            try { th.Name = "RunAlignSocketOnceReady(LoadAligner)"; } catch { }
                        }
                        return IndexLoadAligner.RunAlignSocketOnceReady();
                    })
                    : Task.FromResult(0);

                var t2 = (IndexChipProbeController != null)
                    ? Task.Run(() =>
                    {
                        var th = Thread.CurrentThread;
                        if (th.Name == null)
                        {
                            try { th.Name = "RunInspectionReady(ProbeController)"; } catch { }
                        }
                        return IndexChipProbeController.RunInspectionReady();
                    })
                    : Task.FromResult(0);

                var t3 = (IndexUnloadAligner != null)
                    ? Task.Run(() =>
                    {
                        var th = Thread.CurrentThread;
                        if (th.Name == null)
                        {
                            try { th.Name = "RunAlignSocketOnceReady(UnloadAligner)"; } catch { }
                        }
                        return IndexUnloadAligner.RunAlignSocketOnceReady();
                    })
                    : Task.FromResult(0);

                Task.WaitAll(t1, t2, t3);

                int r1 = t1.Result;
                int r2 = t2.Result;
                int r3 = t3.Result;

                if (r1 != 0 || r2 != 0 || r3 != 0)
                {
                    Log.Write(UnitName, $"OnExecuteUnitActionReady Fail (LoadAligner={r1}, Probe={r2}, UnloadAligner={r3})");
                    return -1;
                }
                return 0;
            }
            catch (Exception ex)
            {
                Log.Write(UnitName, $"OnExecuteUnitActionReady Exception: {ex.Message}");
                return -1;
            }
        }

        public int ExecuteUnitAction(bool isFine = false)
        {
            int nRtn = 0;
            if (RunMode == UnitRunMode.Manual)
            {
                this.CurrentFunc = ExecuteUnitAction;
            }

            Task<int> task = ExecuteUnitActionAsync(isFine);
            while (IsEndTask(task) == false)
            {
                if (IsStop) { return 0; }

                ExecuteUnitActionInterlockLoadMAlign();
                ExecuteUnitActionInterlockProbe();
                Thread.Sleep(1);
            }

            // 예외 전파 및 결과 반영
            if (task.IsFaulted)
            {
                Log.Write(UnitName, "[ExecuteUnitAction] Faulted: " + task.Exception?.GetBaseException().Message);
                return -1;
            }
            return task.Result;
        }
        protected Task<int> ExecuteUnitActionAsync(bool isFine = false)
        {
            return Task.Run(() =>
            {
                OnExecuteUnitAction(isFine);
                return 0;
            });
        }
        protected int OnExecuteUnitAction(bool isFine = false)
        {
            try
            {
                bool bRet = false;

                // DryRun에서도 IndexLoadAligner / IndexChipProbeController / IndexUnloadAligner 는 실제 실행
                // 단, InputDieTransfer / OutputDieTransfer 와의 인터페이스만 배제
                var t1 = (IndexLoadAligner != null)
                    ? Task.Run(() =>
                    {
                        var th = Thread.CurrentThread;
                        if (th.Name == null)
                        {
                            try { th.Name = "RunAlignSocketOnce(LoadAligner)"; } catch { }
                        }
                        return IndexLoadAligner.RunAlignSocketOnce();
                    })
                    : Task.FromResult(0);

                var t2 = (IndexChipProbeController != null)
                    ? Task.Run(() =>
                    {
                        var th = Thread.CurrentThread;
                        if (th.Name == null)
                        {
                            try { th.Name = "RunInspection(ProbeController)"; } catch { }
                        }
                        return IndexChipProbeController.RunInspection();
                    })
                    : Task.FromResult(0);

                var t3 = (IndexUnloadAligner != null)
                    ? Task.Run(() =>
                    {
                        var th = Thread.CurrentThread;
                        if (th.Name == null)
                        {
                            try { th.Name = "RunAlignSocketOnce(UnloadAligner)"; } catch { }
                        }
                        return IndexUnloadAligner.RunAlignSocketOnce();
                    })
                    : Task.FromResult(0);

                if (Config.IsUnitDryRun)
                {
                    // DryRun: OutputDieTransfer 절차 제외 (이벤트/대기 스킵)
                    // 소켓 상태 간단 전이 (Loaded -> Aligned -> Probed)
                    var loadSocket = GetLoadSocketInfo();
                    if (loadSocket != null && loadSocket.GetMaterialDie() != null)
                    {
                        if (loadSocket.State == RotarySocketState.Loaded)
                        {
                            loadSocket.SetState(RotarySocketState.Aligned);
                        }
                        else if (loadSocket.State == RotarySocketState.Aligned)
                        {
                            loadSocket.SetState(RotarySocketState.Probed);
                        }
                    }

                    // 3개 유닛 태스크 실제 실행
                    Task.WaitAll(t1, t2, t3);

                    int r1d = t1.Result;
                    int r2d = t2.Result;
                    int r3d = t3.Result;
                    if (r1d != 0 || r2d != 0 || r3d != 0)
                    {
                        Log.Write(UnitName, $"[DryRun] OnExecuteUnitAction Fail (LoadAligner={r1d}, Probe={r2d}, UnloadAligner={r3d})");
                        return -1;
                    }

                    // Unloader 위치(간단히 Load 반대편) 소켓 비우기 시뮬레이션
                    int unloadIdx = (GetLoadIndexNo() + (GetIndexCount() / 2)) % GetIndexCount();
                    lock (_socketLock)
                    {
                        if (unloadIdx >= 0 && unloadIdx < GetIndexCount())
                        {
                            var s = _sockets[unloadIdx];
                            if (s.GetMaterialDie() != null &&
                                s.GetMaterialDie().Presence == Material.MaterialPresence.Exist)
                            {
                                s.GetMaterialDie().Presence = Material.MaterialPresence.NotExist;
                                s.SetMaterialDie(null);
                                s.SetState(RotarySocketState.Empty);
                            }
                        }
                    }
                    return 0;
                }

                // ===== 실제 운전 (DryRun 아님): 기존 OutputDieTransfer 연동 유지 =====
                // 언로더 얼라인 준비가 끝난 후 픽업 시작 신호
                t3.Wait();
                Thread.Sleep(1);

                if (OutputDieTransfer != null)
                {
                    // 1) Unloader 위치 Die 존재 여부 선확인
                    MaterialDie unloadDie = null;
                    try
                    {
                        unloadDie = GetUnloadSocketMaterial();
                    }
                    catch
                    {
                        unloadDie = null;
                    }

                    bool hasDie =
                        unloadDie != null &&
                        unloadDie.Presence == Material.MaterialPresence.Exist;

                    if (hasDie == true)
                    {
                        PrepareOutputDieTransferHandshake();
                        
                        // OutputDieTransfer가 Running 상태이고 Work로 진입해 Start 대기를 할 준비가 되었는지 확인
                        //if (OutputDieTransfer.RunUnitStatus == UnitStatus.Running &&
                        //    OutputDieTransfer.State == ProcessState.Work)
                        //{
                        //    return 0;
                        //}

                        this.OutputDieTransfer.RisePickupStartEvent();
                        bRet = OutputDieTransfer.WaitPickupDoneEvent(Config.OutputDieTransferTimeoutMs > 0
                                                        ? Config.OutputDieTransferTimeoutMs
                                                        : 60000);
                        if (!bRet)
                        {
                            PostAlarm((int)AlarmKeys.eOutputDieTransferTimeout);
                            Log.Write(UnitName, "OnExecuteUnitAction Fail (OutputDieTransfer WaitPickupDoneEvent Timeout)");
                            return -1;
                        }

                        // 2) 픽 성공 여부 확인 (LastPickSucceeded 플래그 기반)
                        if (OutputDieTransfer.LastPickSucceeded)
                        {
                            // OutputDieTransfer 완료 시: OutputDieTransfer의 소켓 정보만 사용하여 비우기
                            try
                            {
                                int idx = this.OutputDieTransfer.GetUnloaderIndexNo();
                                if (idx >= 0 && idx < GetIndexCount())
                                {
                                    lock (_socketLock)
                                    {
                                        _sockets[idx].SetMaterialDie(null);
                                        _sockets[idx].SetState(RotarySocketState.Empty);
                                    }
                                    Log.Write(UnitName, $"[OutputDieTransfer] Socket {(idx + 1)} -> Empty");
                                }
                                else
                                {
                                    Log.Write(UnitName, $"[OutputDieTransfer] Invalid unloader socket index: {idx}");
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Write(UnitName, $"[OutputDieTransfer] 소켓 상태 초기화 실패: {ex.Message}");
                            }
                        }
                        else
                        {
                            // 픽업 동작은 끝났으나 성공 플래그 False → 소켓 유지
                            Log.Write(UnitName, "[OutputDieTransfer] Pick sequence ended but LastPickSucceeded = false. Socket keep.");
                        }

                    }
                    else
                    {
                        // OutputDieTransfer가 Work 상태에서 Start만 기다릴 가능성 → 직접 Done 보내 종료 유도
                        OutputDieTransfer.RisePickupDoneEvent();
                    }
                }

                //Task.WaitAll(t1, t2, t3);
                Task.WaitAll(t1, t2);

                int r1 = t1.Result;
                int r2 = t2.Result;
                int r3 = t3.Result;

                if (r1 != 0 || r2 != 0 || r3 != 0)
                {
                    Log.Write(UnitName, $"OnExecuteUnitAction Fail (LoadAligner={r1}, Probe={r2}, UnloadAligner={r3})");
                    return -1;
                }

                return 0;
            }
            catch (Exception ex)
            {
                Log.Write(UnitName, $"OnExecuteUnitAction Exception: {ex.Message}");
                return -1;
            }
        }

        // OutputDieTransfer 사용 직전 (Start 이벤트 Set 전에) 추가
        private void PrepareOutputDieTransferHandshake()
        {
            if (OutputDieTransfer == null) return;
            // 이전 Done 잔여 신호 제거 (있으면 소비)
            OutputDieTransfer.WaitPickupDoneEvent(0);
            // 이전 Start 잔여 신호 제거 (있으면 소비)
            OutputDieTransfer.WaitPickupStartEvent(0);
        }

        private int WaitPostActionSettled(bool needLoadWait, int timeoutMs)
        {
            var timeout = new TimeoutChecker(timeoutMs, autoStart: true);

            while (true)
            {
                if (IsStop) { return 0; }

                // 1) Load 소켓 투입 완료 대기
                bool loadOk = true;
                if (needLoadWait)
                {
                    var socket = GetLoadSocketInfo();
                    var die = socket.GetMaterialDie();

                    var loadDie = GetLoadSocketMaterial();
                    loadOk = (loadDie != null && loadDie.Presence == Material.MaterialPresence.Exist);

                    //loadDie.Presence = Material.MaterialPresence.Exist;
                    //loadDie.ProcessSatate = Material.MaterialProcessSatate.Ready;
                    socket.SetMaterialDie(loadDie);
                    socket.SetState(RotarySocketState.Loaded);

                }

                // 2) Unloader Aligner에 잔류품 없음을 확인
                bool unloadOk = true;
                if (IndexUnloadAligner != null)
                {
                    var unloaderDie = GetUnloaderAlignSocketMaterial();
                    unloadOk = (unloaderDie == null || unloaderDie.Presence != Material.MaterialPresence.Exist);
                }

                if (loadOk && unloadOk)
                    return 0;

                if (timeout.IsCompleted)
                {
                    if (!loadOk)
                    {
                        Log.Write(UnitName, "[WaitPostActionSettled] Load socket die not supplied (timeout)");
                        PostAlarm((int)AlarmKeys.InputDieTransferTimeout);
                    }
                    if (!unloadOk)
                    {
                        Log.Write(UnitName, "[WaitPostActionSettled] UnloadAligner still has die (timeout)");
                        PostAlarm((int)AlarmKeys.eOutputDieTransferTimeout);
                    }
                    return -1;
                }
                Thread.Sleep(1);
            }
        }

        public int Rotate(bool isFine = false)
        {
            int nRet = 0;
            if (RunMode == UnitRunMode.Manual)
            {
                this.CurrentFunc = Rotate;
            }

            nRet = MovePositionRotate();
            if (nRet != 0)
            {
                AxisT.EmgStop();
                PostAlarm((int)AlarmKeys.RotaryIndexMoveError);
                Log.Write(UnitName, "Rotate Fail");
                return -1;
            }

            // 3. 회전 후 소켓 상태 전이 (예: Load -> Loading 등)
            //PostRotateStateTransition();
            return nRet;
        }

        //단위 동작.
        public bool IsInterlockOKWidthAllUnit()
        {
            bool bRet = true;
            string reason = null;
            if (_axisT == null)
            {
                //PostAlarm((int)AlarmKeys.eIndexRotary);
                reason = "AxisT NULL";
                return false;
            }

            // ZUp이 아닌 경우 OK
            //bRet = (IndexChipProbeController == null)
            //    ? true
            //    : !IndexChipProbeController.IsTopContactIndexZUp(IndexChipProbeController.GetProbeIndexNo());
            // Z-Up이 아닌 경우 OK (고전 if/else 방식)
            if (IndexChipProbeController == null)
            {
                bRet = true;
            }
            else
            {
                bool isZUp1 = false, isZUp2 = false;
                try
                {
                    //
                    isZUp1 = IndexChipProbeController.IsTopContactIndexZUp(IndexChipProbeController.GetProbeIndexNo());
                    isZUp2 = IndexChipProbeController.IsBottomIndexZUp(IndexChipProbeController.GetProbeIndexNo());
                    //if (IndexChipProbeController.Config.ContectTopMode)
                    //{
                    //    isZUp1 = IndexChipProbeController.IsTopContactIndexZUp(IndexChipProbeController.GetProbeIndexNo());
                    //}
                    //else
                    //{
                    //    isZUp2 = IndexChipProbeController.IsBottomIndexZUp(IndexChipProbeController.GetProbeIndexNo());
                    //}
                }
                catch
                {
                    isZUp1 = false;
                    isZUp2 = false;
                }
                if (isZUp1 == false && isZUp2 == false)
                {
                    bRet = true;
                }
                else
                {
                    bRet = false;
                }
            }

            if (IndexLoadAligner == null)
            {
                bRet = true;
            }
            else
            {
                bool isZUp = false;
                try
                {
                    isZUp = IndexLoadAligner.IsAlignZIndexUp(IndexLoadAligner.GetAlignIndexNo());
                }
                catch
                {
                    isZUp = false;
                }
                bRet = !isZUp; // Z-Up이 아니면 OK
            }

            if (InputDieTransfer == null)
            {
                bRet = true;
            }
            else
            {
                bool isZUp = false;
                try
                {
                    isZUp = InputDieTransfer.IsPositionPlaceZSafety();
                }
                catch
                {
                    isZUp = false;
                }
                bRet = isZUp; // Z-Up이 아니면 OK
            }

            if(OutputDieTransfer == null)
            {
                bRet = true;
            }
            else
            {
                bool isZUp = false;
                try
                {
                    isZUp = OutputDieTransfer.IsPositionPickZSafety();
                }
                catch
                {
                    isZUp = false;
                }
                bRet = isZUp; // Z-Up이 아니면 OK
            }

            if (RunUnitStatus != UnitStatus.Running)
            {
                if (!VerifyAllUnitsSafe(out reason))
                {
                    //PostAlarm((int)AlarmKeys.eRotaryNotSafe);
                    reason = "Not Safe: " + reason;
                    return false;
                }
            }

            if (_sockets == null)
            {
                //PostAlarm((int)AlarmKeys.eIndexRotary);
                reason = "Socket array NULL";
                return false;
            }

            return bRet;
        }
        public int MovePositionRotate(bool isFine = false)
        {
            if (IsInterlockOKWidthAllUnit() == false)
            {
                Log.Write(UnitName, "MovePositionRotate Interlock Fail");
                return -1;
            }

            Task<int> task = MovePositionAsyncRotate(isFine);
            while (IsEndTask(task) == false)
            {
                if (IsInterlockOKWidthAllUnit() == false)
                {
                    Log.Write(UnitName, "MovePositionRotate Interlock Fail");
                    return -1;
                }

                Thread.Sleep(1);
            }
            return task.Result;
        }
        public Task<int> MovePositionAsyncRotate(bool isFine = false)
        {
            return Task.Run(() =>
            {
                OnMovePositionRotate(isFine);
                return 0;
            });
        }
        private int OnMovePositionRotate(bool isFine = false)
        {
            int nRet = 0;

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
                PostAlarm((int)AlarmKeys.eRotaryNotSafe);
                return -1;
            }

            // 이동 완료 후 현재 Load 소켓 번호 이벤트 통지
            //OnLoadIndexChanged(GetLoadIndexNo());

            return nRet;
        }

        /// //////////////////////////////////////////////////////////////////
        
        public int ExecuteUnitLoadDie(bool isFine = false)
        {
            int nRtn = 0;
            if (RunMode == UnitRunMode.Manual)
            {
                this.CurrentFunc = ExecuteUnitLoadDie;
            }
            RequestInputDieTrDie = true; // InputDieTransfer에 Chip 요청 상태로 변경.

            return nRtn;
        }

        public int ExecuteUnitUnLoadDie(bool isFine = false)
        {
            int nRtn = 0;
            if (RunMode == UnitRunMode.Manual)
            {
                this.CurrentFunc = ExecuteUnitUnLoadDie;

            }

            return nRtn;
        }

        public int ExecuteUnitActionInterlockLoadMAlign(bool isFine = false)
        {
            int nRet = 0;


            return nRet;
        }

        public int ExecuteUnitActionInterlockProbe(bool isFine = false)
        {
            int nRet = 0;


            return nRet;
        }
        
        #endregion
    }
}