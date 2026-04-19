using QMC.Common;
using QMC.Common.Alarm;
using QMC.Common.Component;
using QMC.Common.IOUtil;
using QMC.Common.Motions;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Component;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using static QMC.Common.Material;
using static QMC.LCP_280.Process.Equipment;
using static QMC.LCP_280.Process.Unit.RotaryConfig.IO; // 

namespace QMC.LCP_280.Process.Unit
{
    public class Rotary : BaseUnit<RotaryConfig>
    {
        private bool IsDryRunEqp
        {
            get
            {
                var eq = Equipment.Instance;
                bool r = eq.EquipmentConfig.IsDryRun;
                return r;
            }
        }

        #region Alarm
        public new enum AlarmKeys
        {
            eIndexRotary = 10501,
            eRotaryNotSafe,
            InputDieTransferPlaceZError,
            IndexLoadAlignerZError,
            IndexChipProbeControllerZError,
            OutputDieTransferPickZError,
            InputDieTransferTimeout,
            RotaryIndexMoveError,
            eOutputDieTransferTimeout,
            eRotaryVaccum,
            ExecuteUnitActionError,
            IndexLoadAlignerError,
        }
        protected override void InitAlarm()
        {
            string source = "Index";
            base.InitAlarm();
            // 1. 공용 파일 로더에서 알람 목록 가져오기
            var loadedAlarms = GlobalAlarmTable.Instance.GetAlarmsForSource(source);
            if (loadedAlarms == null || loadedAlarms.Count == 0)
            {
                Log.Write("AlarmInit", $"Cannot find alarms for source '{source}' in the alarm file. Only default alarms will be registered.");

                AlarmInfo alarm = new AlarmInfo();

                alarm.Code = (int)AlarmKeys.eIndexRotary;
                alarm.Title = "Index Rotary Error";
                alarm.Cause = "Error occurred during Index Rotary operation. Please check the position and restart.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eRotaryNotSafe;
                alarm.Title = "Rorary Not safety Pos.";
                alarm.Cause = "Rotary is not in a safe position. Please check the position and restart.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.InputDieTransferPlaceZError;
                alarm.Title = "InputDieTraansferPlaceZ Not safety Pos.";
                alarm.Cause = "InputDieTransferPlaceZ is not in a safe position. Please check the position and restart.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.IndexLoadAlignerZError;
                alarm.Title = "IndexLoadAlignerZ Not safety Pos.";
                alarm.Cause = "IndexLoadAlignerZ is not in a safe position. Please check the position and restart.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.IndexChipProbeControllerZError;
                alarm.Title = "IndexChipProbeControllerZ Not safety Pos.";
                alarm.Cause = "IndexChipProbeControllerZ is not in a safe position. Please check the position and restart.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.OutputDieTransferPickZError;
                alarm.Title = "OutputDieTransferPlaceZ Not safety Pos.";
                alarm.Cause = "OutputDieTransferPlaceZ is not in a safe position. Please check the position and restart.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.InputDieTransferTimeout;
                alarm.Title = "InputDieTransfer Timeout";
                alarm.Cause = "InputDieTransfer Place operation timed out. Please check the position and restart.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.RotaryIndexMoveError;
                alarm.Title = "Rotary Index Move Error";
                alarm.Cause = "Error occurred during Rotary Index Move. Please check the position and restart.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eOutputDieTransferTimeout;
                alarm.Title = "OutputDieTransfer Timeout";
                alarm.Cause = "OutputDieTransfer Place operation timed out. Please check the position and restart.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eRotaryVaccum;
                alarm.Title = "Rotary Vaccum Error";
                alarm.Cause = "Rotary Vacuum Error. Please check the position and restart.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.ExecuteUnitActionError;
                alarm.Title = "Execute Unit Action Error";
                alarm.Cause = "Error occurred while executing Unit Action. Please check the position and restart.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                //IndexLoadAlignerError
                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.IndexLoadAlignerError;
                alarm.Title = "IndexLoadAligner Error";
                alarm.Cause = "Error occurred in IndexLoadAligner. Please check the position and restart.";
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

        #region Unit
        InputDieTransfer InputDieTransfer { get; set; }
        IndexLoadAligner IndexLoadAligner { get; set; }
        IndexChipProbeController IndexChipProbeController { get; set; }
        IndexUnloadAligner IndexUnloadAligner { get; set; }
        OutputDieTransfer OutputDieTransfer { get; set; }
        InputStage InputStage { get; set; } // 추가: InputStage 참조
        OutputStage OutputStage { get; set; } // 추가: OutputStage 참조
        #endregion

        #region Axis
        private MotionAxis _axisT;
        private MotionAxis _axisPlaceZ;
        public MotionAxis AxisIndexT => _axisT;
        public MotionAxis AxisPlaceZ => _axisPlaceZ;
        private DateTime _moveStartTime;
        #endregion

        #region Event UI 연동
        // === Load 인덱스 변경 이벤트 (UI 연동용) ===
        public delegate void LoadIndexChangedHandler(object sender, int loadIndex0Based);
        public event LoadIndexChangedHandler LoadIndexChanged;
        #endregion

        // Safe
        private static readonly string[] SafeNames = new[] { "SafetyZone", "Safe", "SasfeZone", "SAFE", "SAFEZONE", "SAFE_ZONE" };
        #region Socket State 관리 (간단/가독성 중심)
        // 소켓 상태 정의
        public enum RotarySocketState
        {
            None = -1,
            Empty = 0,
            Loading,
            Loaded,
            MAligning,
            MAligned,
            Probing,
            Probed,
            VAligning,
            VAligned,
            Unloading,    // UnloadAlign 동작(언로더 얼라인 공정)
            Unloaded,     // OutputDieTransfer 픽/배출 공정 (새로 추가)
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

        public void ClearSockets()
        {
            int cnt = GetIndexCount();
            for (int i = 0; i < cnt; i++)
            {
                _sockets[i].SetState(RotarySocketState.Empty);
                _sockets[i].SetMaterialDie(null);
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
            if (AxisIndexT == null)
            {
                return 0;
            }

            // 2. 원시 위치 읽기 (논리 단위: 시뮬레이션은 그대로, 실기는 *1000 스케일 사용 중)
            double rawLogicalPosition = AxisIndexT.GetPosition();
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

        public int GetTrashCanIndexNo()
        {
            int loadIndex = this.GetLoadIndexNo();
            int probeIndex = (loadIndex - 5 + this.GetIndexCount()) % this.GetIndexCount();
            return probeIndex;
        }
        
        private MaterialDie EnsureSocketDie(SocketInfo s)
        {
            if (s == null)
            {
                Log.Write(UnitName, "EnsureSocketDie", "SocketInfo Null");
                return null;
            }

            var d = s.GetMaterialDie();
            if (d == null)
            {
                d = new MaterialDie
                {
                    Presence = Material.MaterialPresence.Unknown,
                    ProcessSatate = Material.MaterialProcessSatate.Unknown
                };
                s.SetMaterialDie(d);
            }
            return d;
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
        public void SetSocket(int no, SocketInfo socketInfo)
        {
            _sockets[no] = socketInfo;
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

        private string[] BuildPickersSnapshot()
        {
            // 규칙: UseSocket=true -> "USE", false -> "UNUSE"
            // (프로젝트 표준이 "NOT_USE"라면 여기만 바꾸면 됨)
            return new[]
            {
                Config.UseSocket1 ? "USE" : "UNUSE",
                Config.UseSocket2 ? "USE" : "UNUSE",
                Config.UseSocket3 ? "USE" : "UNUSE",
                Config.UseSocket4 ? "USE" : "UNUSE",
                Config.UseSocket5 ? "USE" : "UNUSE",
                Config.UseSocket6 ? "USE" : "UNUSE",
                Config.UseSocket7 ? "USE" : "UNUSE",
                Config.UseSocket8 ? "USE" : "UNUSE",
            };
        }

        public void ApplyPickersToWaferSummaryIfActive()
        {
            try
            {
                var eq = Equipment.Instance;
                var sum = eq?.SummaryContext?.Current;
                if (sum == null) 
                    return;

                // SummaryContext가 활성일 때만 의미 있음(비활성인데 넣어봤자 다음 Begin에서 Reset될 수 있음)
                if (eq.SummaryContext.IsActive == false) 
                    return;

                sum.SetPickersUseFlags(BuildPickersSnapshot());
            }
            catch (Exception ex)
            {
                Log.Write(UnitName, nameof(ApplyPickersToWaferSummaryIfActive), ex.Message);
            }
        }


        public override void SetMaterial(Material m)
        {
            var socket = GetLoadSocketInfo();
            socket.SetMaterialDie  (m as MaterialDie);
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
                var s = _sockets[idx];
                EnsureSocketDie(s); // ← 플레이스홀더 보장
                return s;
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

        public void MoveMaterialToOutputDieTransfer()
        {
            var socket = GetUnloadSocketInfo();
            var die = socket.GetMaterialDie();
            OutputDieTransfer.SetMaterial(die);

            socket.SetMaterialDie(null);
        }
        public void ReMoveMaterialToOutputDieTransfer()
        {
            var socket = GetUnloadSocketInfo();
            var die = socket.GetMaterialDie();
            socket.SetMaterialDie(null);
        }

        private SocketInfo GetUnloadSocketInfo()
        {
            int idx = OutputDieTransfer.GetUnloaderIndexNo();
            lock (_socketLock)
            {
                return _sockets[idx];
            }
        }
        public MaterialDie GetTrashCanSocketMaterial()
        {
            var socket = GetTrashCanSocketInfo();
            return socket.GetMaterialDie();
        }
        public SocketInfo GetTrashCanSocketInfo()
        {
            int idx = GetTrashCanIndexNo();
            lock (_socketLock)
            {
                var s = _sockets[idx];
                EnsureSocketDie(s); // ← 플레이스홀더 보장
                return s;
            }
        }
        // 빈 소켓 판단(중앙화)
        public bool IsLoadSocketEmpty()
        {
            var s = GetLoadSocketInfo();
            var d = s?.GetMaterialDie();
            return d == null
                || d.Presence == Material.MaterialPresence.NotExist
                || d.Presence == Material.MaterialPresence.Unknown;
        }

        protected virtual void OnLoadIndexChanged(int loadIndex0Based)
        {
            LoadIndexChangedHandler handler = this.LoadIndexChanged;
            if (handler != null)
            {
                handler(this, loadIndex0Based);
            }
        }

        /// <summary>
        /// 소켓 데이터(머티리얼/태그/상태)를 클리어합니다.
        /// socketNo = -1 이면 전체 소켓을 대상, 0~7이면 해당 소켓만 대상입니다.
        /// offIo = true 시 실기에서 해당 소켓 IO(Vac/Blow/Vent)를 모두 OFF 합니다.
        /// resetState = true 시 소켓 상태를 Empty 로 변경합니다.
        /// 반환: 0=성공, -1=파라미터 오류
        /// </summary>
        public int ClearSocketData(int socketNo = -1, bool offIo = true, bool resetState = true)
        {
            if (_sockets == null || _sockets.Length == 0)
                return 0;

            bool clearAll = (socketNo < 0);
            if (!clearAll && (socketNo >= _sockets.Length))
                return -1;

            try
            {
                bool sim = (Config?.IsSimulation == true) || (Config?.IsDryRun == true) || (Config?.IsUnitDryRun == true);

                lock (_socketLock)
                {
                    int start = clearAll ? 0 : socketNo;
                    int end = clearAll ? _sockets.Length - 1 : socketNo;

                    for (int i = start; i <= end; i++)
                    {
                        var s = _sockets[i];
                        if (s == null) continue;

                        // 데이터 클리어
                        s.SetMaterialDie(null);
                        s.Tag = null;

                        if (resetState)
                            s.SetState(RotarySocketState.Empty);

                        // 해당 소켓 IO 안전 OFF
                        if (offIo && !sim)
                        {
                            try { SetVacuum(i, false); } catch { }
                            try { SetBlow(i, false); } catch { }
                            try { SetVent(i, false); } catch { }
                        }
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                Log.Write(UnitName, $"[ClearSocketData] Failed: {ex.Message}");
                return -1;
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
            il.AddAxisMustBeHomed("RotaryTHomed", _axisT, "T axis Home movement has not been completed.");
            il.AddGlobalRule("EquipStateRunningBlock", () =>
            {
                return Equipment.Instance != null && Equipment.Instance.EqState == EquipmentState.AutoRunning
                    ? "Manual operation is not allowed during AutoRunning." : null;
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
            InputStage = Equipment.Instance.GetUnit(UnitKeys.InputStage) as InputStage; // 추가: 바인딩
            OutputStage = Equipment.Instance.GetUnit(UnitKeys.OutputStage) as OutputStage; // 추가: 바인딩
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

            const string unitName = "Unit"; 
            BindAxis(mgr, unitName, AxisNames.IndexT, ref _axisT);
            BindAxis(mgr, unitName, AxisNames.IndexPlaceZ, ref _axisPlaceZ);
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

        private readonly object _moveTargetLock = new object();
        private bool _hasPendingTarget = false;
        private double _pendingTargetDeg = 0.0;   // 0~360 기준
        private int _pendingStepDir = 0;          // -1 / +1
        private double GetAxisDeg()
        {
            if (AxisIndexT == null) return 0.0;
            double p = AxisIndexT.GetPosition();
            return Config.IsSimulation ? p : (p * 1000.0);
        }

        /// <summary>
        /// 현재 각도에서 stepDir(-1/+1) 한 칸 이동한 목표각(0~360)을 계산
        /// </summary>
        private const double FixedIndexStepDeg = 45.0;
        private double CalcNextTargetDegFromCurrent(int stepDir)
        {
            double cur = NormalizeAngle(GetAxisDeg());
            double tgt = cur + (stepDir < 0 ? +FixedIndexStepDeg : -FixedIndexStepDeg);
            return NormalizeAngle(tgt);
        }

        private void SetPendingMoveTarget(double targetDeg, int stepDir)
        {
            lock (_moveTargetLock)
            {
                _pendingTargetDeg = NormalizeAngle(targetDeg);
                _pendingStepDir = stepDir < 0 ? -1 : +1;
                _hasPendingTarget = true;
            }
        }

        private bool TryGetPendingMoveTarget(out double targetDeg)
        {
            lock (_moveTargetLock)
            {
                targetDeg = _pendingTargetDeg;
                return _hasPendingTarget;
            }
        }

        private void ClearPendingMoveTarget()
        {
            lock (_moveTargetLock)
            {
                _hasPendingTarget = false;
                _pendingTargetDeg = 0.0;
                _pendingStepDir = 0;
            }
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

            //// 1) Safe-Zone check.
            if (VerifyAllUnitsSafe(out reason) == false)
            {
                Log.Write("Rotary", $"Index Move Blocked: {reason}");
                return false;
            }

            int stepDir = (step < 0) ? -1 : +1;
            double targetDeg = CalcNextTargetDegFromCurrent(stepDir);

            // 3) Move Check.
            int rc = step < 0 ? axis.MovePrevIndex() : axis.MoveNextIndex();
            if (rc != 0)
            {
                reason = $"Index move start failed (rc={rc})";
                return false;
            }

            SetPendingMoveTarget(targetDeg, stepDir);
            _moveStartTime = DateTime.Now;

            //Thread.Sleep(100); //100->50
            //var swStart = Stopwatch.StartNew();
            //while (this.AxisIndexT.IsMoveDone() && swStart.ElapsedMilliseconds < 50)
            //{
            //    Thread.Sleep(1);
            //}
            //Thread.Sleep(50); //100->50
            //_moveStartTime = DateTime.Now;
            // (변경) 이동 완료 후에 이벤트 발생하도록 비동기 처리
            //Task.Run(() =>
            //{
            //    //Thread.Sleep(70); //100->50->70
            //    // Done + InPosition 동시 확인(가능하면 목표각 기반, 실패 시 기존 방식 fallback)
            //    int wrc = WaitIndexMoveDone();
            //    if (wrc == 0)
            //    {
            //        try
            //        {
            //            OnLoadIndexChanged(GetLoadIndexNo());
            //        }
            //        catch (Exception ex)
            //        {
            //            Log.Write("Rotary", $"LoadIndexChanged dispatch fail: {ex.Message}");
            //        }
            //    }
            //    else
            //    {
            //        Log.Write("Rotary", $"Index move wait timeout/err (rc={wrc})");
            //    }
            //});
            return true;
        }

        // [수정 1] WaitIndexMoveDone: 무한 대기 방지 및 타임아웃 적용
        public int WaitIndexMoveDone(int timeoutMs = 10000, int pollMs = 2) // 기본 타임아웃 10초 설정
        {
            int nRet = 0;
            if (AxisIndexT == null)
            {
                nRet = -1;
                return -1;
            }

            try
            {
                if (pollMs < 1) pollMs = 1;
                if (timeoutMs <= 0) timeoutMs = 10000;

                var sw = Stopwatch.StartNew();
                double tolDeg = AxisIndexT.Config?.InposTolerance ?? 0.002;

                while (sw.ElapsedMilliseconds <= timeoutMs)
                {
                    // 정지 요청 우선 처리
                    var eqState = Equipment.Instance?.EqState ?? EquipmentState.Unknown;
                    if (eqState == EquipmentState.Starting ||
                        eqState == EquipmentState.AutoRunning ||
                        eqState == EquipmentState.ManualRunning)
                    {
                        if (IsStop)
                        {
                            ClearPendingMoveTarget();
                            nRet = -1;
                            return -1;
                        }
                    }

                    // 핵심: -1(사실상 장시간 블로킹) 금지
                    // MotionAxis.WaitMoveDone 내부에서 Done + 위치 보조 판단 수행
                    int rc = AxisIndexT.WaitMoveDone(pollMs);
                    if (rc == 0)
                    {
                        // 포지션 확인 안하면 빠른디.. 아오..
                        //if (TryGetPendingMoveTarget(out double targetDeg))
                        //{
                        //    double cur = NormalizeAngle(GetAxisDeg());
                        //    double err = Math.Abs(NormalizeAngle(cur - targetDeg));
                        //    if (err > 180.0)
                        //        err = 360.0 - err;

                        //    if (err <= tolDeg)
                        //    {
                        //        //ClearPendingMoveTarget();
                        //        OnLoadIndexChanged(GetLoadIndexNo());
                        //        nRet = 0;
                        //        return 0;
                        //    }
                        //}
                        //else
                        {
                            //ClearPendingMoveTarget();
                            OnLoadIndexChanged(GetLoadIndexNo());
                            nRet = 0;
                            return 0; // 기존 호환
                        }
                    }

                    Thread.Sleep(pollMs);
                }

                Log.Write(UnitName, nameof(WaitIndexMoveDone),
                    $"Timeout ({timeoutMs}ms) axis={AxisIndexT?.Name}");
                ClearPendingMoveTarget();

                nRet = -1;
                return -1;
            }
            catch(Exception ex)
            {
                Log.Write(ex);
                return -1;
            }
        }

        private readonly object _lockIndexMoving = new object();
        // 공통 위치 오차 계산 (0~180)
        private double GetAngularErrorDeg(double currentDeg, double targetDeg)
        {
            double err = Math.Abs(NormalizeAngle(currentDeg - targetDeg));
            if (err > 180.0) err = 360.0 - err;
            return err;
        }

        public bool  IsIndexMoving()
        {
            var ax = AxisIndexT;
            if (ax == null) return false;

            //lock이 필요한가?
            lock (_lockIndexMoving)
            {
                // 공통 tolerance
                double tolDeg = ax.Config?.InposTolerance ?? 0.002;
                // 1) 목표각이 있으면 목표각 기준이 최우선 (기존 WaitIndexMoveDone과 동일 철학)
                if (TryGetPendingMoveTarget(out double targetDeg))
                {
                    double cur = NormalizeAngle(GetAxisDeg());
                    double err = GetAngularErrorDeg(cur, targetDeg);
                    bool inTarget = (err <= tolDeg);
                    bool driverMoving = IsAxisMoving(AxisNames.IndexT);

                    // 목표 미도달 또는 드라이버 moving이면 moving
                    if (!inTarget || driverMoving)
                    {
                        return true;
                    }

                    return false;
                }
                else // 아래 구문 지우면 안됨... 흠.. targetDeg fail 나는 경우 있음.. 
                {
                    // 1) 1차: 드라이버 상태
                    bool driverMoving = IsAxisMoving(AxisNames.IndexT);
                    if (driverMoving == true)
                    {
                        return true;
                    }
                    else
                    {
                        // 2) 드라이버가 moving이라도, "인덱스 위치에 충분히 근접"하면 정지로 간주(상태 지연 보정)
                        double stepDeg = 360.0 / GetIndexCount(); // 45
                        // 현재 각도(deg). (단위는 질문에서 맞다고 했으니 그대로 사용)
                        double curDeg = 0;
                        if (Config.IsSimulation)
                        {
                            curDeg = ax.GetPosition();// * 1000.0;
                        }
                        else
                        {
                            //확인 필요.  아래가 맞겠는데...
                            curDeg = ax.GetPosition() * 1000.0;
                        }

                        // 0~step 구간 잔여
                        double remain = curDeg % stepDeg;
                        if (remain < 0)
                            remain += stepDeg;

                        // 가장 가까운 인덱스까지의 오차 (0 근처 OR step 근처 모두 허용)
                        double err = Math.Min(remain, stepDeg - remain);

                        // err이 tolerance 이하면 "실질적으로 멈춤"으로 판단
                        if (err >= tolDeg)
                        {
                            return true;
                        }
                    }

                    return false;
                }
            }
        }
        #endregion

        #region I/O Binding
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
        // === Domain Control ===
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
        public bool SetTrashEjector(bool on)
        {
            bool bRet = false;
            if (on)
                bRet = this.WriteOutput(TRASH_CNA_EJECTOR, true);
            else
                bRet = this.WriteOutput(TRASH_CNA_EJECTOR, false);
            
            if(Config.IsSimulation)
            {
                bRet = true;
            }

            return bRet;
        }
        public bool SetTrashVacuum(bool on)
        {
            bool bRet = false;
            if (on)
                bRet = this.WriteOutput(TRASH_CNA_VACUUM, true);
            else
                bRet = this.WriteOutput(TRASH_CNA_VACUUM, false);

            if (Config.IsSimulation)
            {
                bRet = true;
            }

            return bRet;
        }
        public bool IsVacuumOK(int slotIndex)
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

            if(Config.IsSimulation || (Config.IsDryRun || IsDryRunEqp))
            {
                return true;
            }

            return this.ReadInput(FLOW[slotIndex]);
        }

        public bool IsOutVacummOn(int slotIndex)
        {
            bool bRet = false;

            switch (slotIndex)
            {
                //case 0: bRet = this.IsOutputOn("RotatyVac1"); break;
                case 0: bRet = this.IsOutputOn(VAC1); break;
                case 1: bRet = this.IsOutputOn(VAC2); break;
                case 2: bRet = this.IsOutputOn(VAC3); break;
                case 3: bRet = this.IsOutputOn(VAC4); break;
                case 4: bRet = this.IsOutputOn(VAC5); break;
                case 5: bRet = this.IsOutputOn(VAC6); break;
                case 6: bRet = this.IsOutputOn(VAC7); break;
                case 7: bRet = this.IsOutputOn(VAC8); break;
            }
            

            return bRet;
        }

        // === Rotary Vacuum 상태 대기 공용 유틸 ===
        // expectOn: true=ON 될 때까지, false=OFF 될 때까지 대기
        // timeoutMs/pollMs: 타임아웃/폴링 간격
        public int WaitVacuumStateOrAlarm(int armIndex, bool expectOn, int timeoutMs = 1000, int pollMs = 1)
        {
            if (Config.IsSimulation || (Config.IsDryRun || IsDryRunEqp))
                return 0;

            //Todo: 2025-10-10 GYN: Vacuum 해결 되면 return 지우기.
            return 0;

            //var sw = Stopwatch.StartNew();
            //while (sw.ElapsedMilliseconds <= timeoutMs)
            //{
            //    bool ok = IsVacuumOK(armIndex);
            //    if (expectOn ? ok : !ok)
            //        return 0;

            //    Thread.Sleep(pollMs);
            //}

            //// 타임아웃 처리
            //PostAlarm((int)AlarmKeys.eRotaryVaccum);
            //Log.Write(UnitName, expectOn ? "[Vacuum] Arm vacuum ON timeout" : "[Vacuum] Arm vacuum OFF timeout");
            //return -1;
        }

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
        
        public bool RequestOutputDieTrDie { get; set; } = false;
        public bool IsHaveDie()
        {
            bool bRet = false;
            foreach (var v in this._sockets)
            {
                var die = v.GetMaterialDie();
                if (die != null)
                {
                    if (die.Presence == Material.MaterialPresence.Exist)
                    {
                        return true;
                    }
                }

            }
            return bRet;
        }

        // [ADD] OutputStage 강제 Completed 중복 호출 방지용 래치
        //  - Input wafer 종료 조건이 유지되는 동안 ForceComplete가 틱마다 반복 호출되는 것을 방지
        //  - 조건이 다시 깨지면(false) 자동으로 래치 해제되어 다음 웨이퍼에 재동작 가능
        private bool _outStageForceCompletedLatched = false;

        #endregion


        #region Lifecycle
        public override int OnRun()
        {
            int nRtn = 0;

            try
            {
                if (this.RunUnitStatus == UnitStatus.Stopped ||
                    this.RunUnitStatus == UnitStatus.Stopping ||
                    this.RunUnitStatus == UnitStatus.Error ||
                    this.RunUnitStatus == UnitStatus.CycleStop ||
                    this.RunUnitStatus == UnitStatus.ManualRunning)
                {
                    this.State = ProcessState.Stop;
                    return 0;
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
            catch (Exception ex)
            {
                Log.Write(ex);
                this.OnStop();
                return -1;
            }
            finally
            {
              //  TaktEnd("OnRun");
            }
                
        }
        protected override int OnStart()
        {
            if (_resetNgOnNewInput)
                ResetExecuteActionNgCount("Start");

            return base.OnStart();
        }
        public override int OnStop()
        {
            int ret = 0;
            this.RunUnitStatus = UnitStatus.Stopped;
            base.OnStop();
            return ret;
        }
        protected override int OnRunReady() 
        {
            try
            {
                int nRet = 0;
                if (IsIndexMoving())
                {
                    return 0;
                }

                nRet = ExecuteUnitActionReady();
                if (nRet != 0)
                {
                    //AxisIndexT.EmgStop();
                    PostAlarm((int)AlarmKeys.ExecuteUnitActionError);
                    Log.Write(UnitName, "[ExecuteUnitActionReady] Failed");
                    return -1;
                }

                State = ProcessState.Work;
                return nRet;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                this.OnStop();
                return -1;
            }
        }

        private int _onRunWorkEntered = 0; // 0=idle, 1=running
        private Task<int> _loadWaitTask = null;
        private readonly object _loadWaitLock = new object();

        protected override int OnRunWork() 
        {
            int nRet = 0;
            try
            {
                if (Interlocked.CompareExchange(ref _onRunWorkEntered, 1, 0) != 0)
                {
                    // 이미 실행 중이면 중복 진입 차단
                    return 0;
                }

                // 인덱스 이동 중이면 대기
                if (IsIndexMoving())
                {
                    return 0;
                }

                TaktStart("One Cycle");
                int nIndex = GetLoadIndexNo();
                bool useSocket = this.Config.GetUseSocket(nIndex);
                // 변경: InputStage에 공급 가능한 Die가 있는 경우에만 요청 신호 설정
                bool hasNextDie = true;
                if (this.InputDieTransfer != null)
                {
                    var die = GetLoadSocketMaterial();
                    if (die != null)
                    {
                        if (die.Presence != Material.MaterialPresence.Exist)
                        {
                            if (useSocket)
                            {
                                try
                                {
                                    hasNextDie = this.InputStage?.HasNextDie() ?? true; // InputStage 미바인딩 시 기존 동작 유지
                                    if(hasNextDie == false)
                                    {
                                        var hasDie = InputDieTransfer.GetMaterial() as MaterialDie;
                                        //if (hasDie != null)
                                        if (hasDie != null && (hasDie.State == DieProcessState.Mapped 
                                            || hasDie.State == DieProcessState.Picked))
                                        {
                                            hasNextDie = true;
                                        }
                                    }
                                }
                                catch
                                {
                                    hasNextDie = true;
                                }

                                if (hasNextDie)
                                {
                                    this.SetVent(nIndex, false);
                                    this.SetBlow(nIndex, false);
                                    Thread.Sleep(1);
                                    this.SetVacuum(nIndex, true);

                                    TaktStart("Place Die");
                                    RequestInputDieTrDie = true;
                                }
                                else
                                {
                                    RequestInputDieTrDie = false; // 명시적으로 요청 안 함
                                }
                            }
                            else
                            {
                                try
                                {
                                    hasNextDie = this.InputStage?.HasNextDie() ?? true; // InputStage 미바인딩 시 기존 동작 유지
                                    if (hasNextDie == false)
                                    {
                                        var hasDie = InputDieTransfer.GetMaterial() as MaterialDie;
                                        //if (hasDie != null)
                                        if (hasDie != null && (hasDie.State == DieProcessState.Mapped
                                            || hasDie.State == DieProcessState.Picked))
                                        {
                                            hasNextDie = true;
                                        }
                                    }
                                }
                                catch
                                {
                                    hasNextDie = true;
                                }
                            }
                        }
                    }
                }
                //Log.Write("kkkkkkRotary", "InputDieTransfer Request");
                
                var recipe = Equipment.Instance.EquipmentRecipe.CurrentRecipe;
                if (recipe.UseSameAsWafer == true)
                {
                    // [ADD] 조기 return 전에 ForceComplete 조건을 먼저 평가
                    // - "모든 die가 소진되어 더 이상 ExecuteUnitAction/WaitPostActionSettled로 진입하지 않는" 케이스에서
                    // OutputStage가 Processing으로 남아 언로딩이 막히는 문제를 방지
                    try
                    {
                        bool inputWaferCompleted = false;
                        var Wafer = InputStage.GetMaterialWafer();
                        if (Wafer != null)
                        {
                            if (Wafer.ProcessSatate == Material.MaterialProcessSatate.Completed)
                            {
                                inputWaferCompleted = true;
                            }
                        }
                        else
                        {
                            inputWaferCompleted = true;
                        }

                        bool inputDone = (hasNextDie == false); //inputStage
                        bool inputEmpty = (InputDieTransfer == null) || (InputDieTransfer.GetMaterial() == null);
                        bool rotaryEmpty = (IsHaveDie() == false);
                        bool odtEmpty = (OutputDieTransfer == null) || (OutputDieTransfer.GetMaterial() == null);
                        
                        var currentOutWafer = OutputStage?.GetMaterialWafer();
                        bool outStageHasBin = (OutputStage != null)
                                              && OutputStage.IsRingPresent()
                                              && (currentOutWafer != null)
                                              && (currentOutWafer?.Presence == Material.MaterialPresence.Exist);
                        
                        // [초기 구간 보호] OutStage 쪽에 "언로딩 대상"이 실제로 있을 때만 고려
                        // - HasNextDie() 호출은 내부적으로 ProcessSatate를 건드리므로 피하고,
                        //   최소한 Dies 존재 여부 정도만 확인 (초기 오작동 방지)
                        bool outStageHasMap = false;
                        try
                        {
                            var w = OutputStage?.GetMaterialWafer();
                            outStageHasMap = (w != null && w.Dies != null && w.Dies.Count > 0);
                        }
                        catch { outStageHasMap = false; }

                        bool shouldForce = inputWaferCompleted && inputDone && inputEmpty && rotaryEmpty && odtEmpty && outStageHasBin && outStageHasMap;
                        if (shouldForce)
                        {
                            if (_outStageForceCompletedLatched == false)
                            {
                                _outStageForceCompletedLatched = true;
                                OutputStage.ForceCompleteAndAllowUnloadWhenBuffersEmpty(
                                    "Input done + no rotary/ODT die (early-return path, latched)");
                            }
                        }
                        else
                        {
                            // 조건이 깨지면 래치 해제
                            _outStageForceCompletedLatched = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                    }
                }

                if (useSocket)
                {
                    if (IsHaveDie() == false && RequestInputDieTrDie == false)
                    {
                        return 0;
                    }
                }
                else
                {
                    if (IsHaveDie() == false && hasNextDie == false)
                    {
                        return 0;
                    }
                }

                // [수정] Load Die 대기와 TaktTime 기록을 별도의 Task로 비동기 실행
                // 이렇게 하면 ExecuteUnitAction() 처리 시간과 관계없이 다이가 실제 공급되는 즉시 
                // 해당 모니터링 Task가 TaktEnd("Load Die")를 호출해 정확한 시간을 측정합니다.
                bool needLoadWait = (RequestInputDieTrDie == true) && useSocket;
                Task<int> loadWaitTask = null;
                if (needLoadWait)
                {
                    lock (_loadWaitLock)
                    {
                        // 이전 task가 남아있고 아직 안끝났으면 재사용(중복 생성 방지)
                        if (_loadWaitTask != null && !_loadWaitTask.IsCompleted)
                        {
                            loadWaitTask = _loadWaitTask;
                        }
                        else
                        {
                            _loadWaitTask = Task.Run(() =>
                            {
                                int res = -1;
                                try
                                {
                                    res = WaitPostActionSettled(needLoadWait, 60000 * 5);
                                    if (res == 0)
                                    {
                                        //Log.Write(UnitName, "OnRunWork", "Load Die Completed");
                                    }
                                }
                                finally
                                {
                                    TaktEnd("Place Die");
                                }
                                return res;
                            });
                            loadWaitTask = _loadWaitTask;
                        }
                    }
                }
                
                try
                {
                    if (IsIndexMoving())
                    {
                        return 0;
                    }
                    Log.Write(UnitName, "OnRunWork", "ExecuteUnitAction");
                    nRet = ExecuteUnitAction();
                }
                catch(Exception ex)
                {
                    Log.Write(ex);
                }

                //Log.Write("kkkkkkRotary", "ExecuteUnitAction End");
                if (nRet != 0)
                {
                    // ODT Start 신호가 남지 않도록 방어적 리셋(실패 시에도)
                    try
                    {
                        OutputDieTransfer?.ReSetPickupStartEvent();
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                    }

                    //AxisIndexT.EmgStop();
                    PostAlarm((int)AlarmKeys.ExecuteUnitActionError);
                    Log.Write(UnitName, "[ExecuteUnitAction] Failed");
                    return -1;
                }

                // 여기 블록(Load 투입 완료 대기)
                // 백그라운드에서 진행중인 Load 투입 완료 최종 확인
                if(loadWaitTask != null)
                {
                    nRet = loadWaitTask.Result;
                    if (nRet != 0)
                    {
                        //AxisIndexT.EmgStop();
                        PostAlarm((int)AlarmKeys.eIndexRotary);
                        Log.Write(UnitName, "OnRun", "[WaitPostActionSettled] Failed");
                        return -1;
                    }
                    // RequestInputDieTrDie는 WaitPostActionSettled(loadOk)에서 해제됨
                    // 투입 완료되었으면 요청 플래그 내림
                    // RequestInputDieTrDie = false;
                }
                
                const int safeWaitTimeoutMs = 10000;
                var swSafe = Stopwatch.StartNew();

                bool bInputTr = false;
                bool bOutTr = false;
                bool bIndexAlignZ = false;
                bool bIndexProbeZ = false;
                bool bIndexProbeCardZ = false;
                while (swSafe.ElapsedMilliseconds < safeWaitTimeoutMs)
                {
                    if (IsStop)
                        return 0;

                    bInputTr = InputDieTransfer?.IsPositionPlaceZSafety() ?? false;
                    bOutTr = OutputDieTransfer?.IsPositionPickZSafety() ?? false;
                    bIndexAlignZ = IndexLoadAligner?.IsPositionAlignZSafety() ?? false;
                    bIndexProbeZ = IndexChipProbeController?.IsPositionProbeZSafety() ?? false;
                    bIndexProbeCardZ = IndexChipProbeController?.IsPositionProbeCardZSafety() ?? false;

                    if (bInputTr && bOutTr && bIndexAlignZ && bIndexProbeZ && bIndexProbeCardZ)
                        break;

                    Thread.Sleep(2);
                }
                // timeout
                if (swSafe.ElapsedMilliseconds >= safeWaitTimeoutMs)
                {
                    PostAlarm((int)AlarmKeys.eRotaryNotSafe);
                    Log.Write(UnitName, "OnRun", "WaitInterlockSafe - Timeout waiting safety positions.");
                    return -1;
                }

                //bool bInputTr = false;
                //bool bOutTr = false;
                //bool bIndexAlignZ = false;
                //bool bIndexProbeZ = false;
                //bool bIndexProbeCardZ = false;
                //var swSafe = Stopwatch.StartNew();
                //try
                //{
                //    while (true)
                //    {
                //        if (IsStop)
                //            return 0;

                //        bInputTr = InputDieTransfer.IsPositionPlaceZSafety();
                //        bOutTr = OutputDieTransfer.IsPositionPickZSafety();
                //        bIndexAlignZ = IndexLoadAligner.IsPositionAlignZSafety();
                //        bIndexProbeZ = IndexChipProbeController.IsPositionProbeZSafety();
                //        bIndexProbeCardZ = IndexChipProbeController.IsPositionProbeCardZSafety();
                //        if (bInputTr && bOutTr && bIndexAlignZ
                //            && bIndexProbeZ && bIndexProbeCardZ)
                //        {
                //            break;
                //        }
                //        Thread.Sleep(1);

                //        // 너무 오래 걸리면 로그 한번 찍어주는 정도의 안전장치
                //        if (swSafe.ElapsedMilliseconds > 5000) 
                //        {
                //            Log.Write(UnitName, "WaitInterlockSafe", 
                //                $"WaitInterlockSafe: Waiting for safe conditions... InputTrSafe={bInputTr} OutTrSafe={bOutTr} AlignZSafe={bIndexAlignZ} ProbeZSafe={bIndexProbeZ} ProbeCardZSafe={bIndexProbeCardZ}");
                //        }
                //    }
                //}
                //catch(Exception ex)
                //{
                //    Log.Write(ex);
                //}
                
                if (IsStop)
                    return 0;

                //Log.Write("kkkkkkRotary", "Rotate");
                TaktStart("Rotate");
                try
                {
                    nRet = Rotate();
                    Log.Write(UnitName, "[Rotate] Rotate Comp");
                }
                finally
                {
                    TaktEnd("Rotate");
                }

                //Log.Write("kkkkkkRotary", "Rotate End");
                // 회전 직후 Start 신호 재설정(기존 동작 유지)
                OutputDieTransfer.ReSetPickupStartEvent();
                if (nRet != 0)
                {
                    //AxisIndexT.EmgStop();
                    PostAlarm((int)AlarmKeys.RotaryIndexMoveError);
                    Log.Write(UnitName, "[Rotate] Failed");
                    return nRet;
                }

                //UI 상태 업데이트를 위해 여기서 한 번해주자. 
                //여기 느림.
                //OnLoadIndexChanged(GetLoadIndexNo());
                Log.Write(UnitName, "OnRun", "[Rotate] Complete");
                State = ProcessState.Complete;
                return nRet;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                this.OnStop();
                return -1;
            }
            finally
            {
                if (State != ProcessState.Complete)
                {
                    // 정상 완료가 아니면 요청 플래그 잔류 방지
                    RequestInputDieTrDie = false;
                }

                Interlocked.Exchange(ref _onRunWorkEntered, 0);
                TaktEnd("One Cycle");
            }
        }

        protected override int OnRunComplete() 
        {
            try
            {
                State = ProcessState.Work;
                return 0;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return 0;
            }
        }
        #endregion

        #region Interlock
        public override bool IsInterlockOK(BaseComponent baseComponent, BaseComponent.InterlockEventArgs e)
        {
            bool bRet = base.IsInterlockOK(baseComponent, e);
            if (baseComponent == this.AxisIndexT)
            {
                if (VerifyAllUnitsSafe(out string reason) == false)
                {
                    AxisIndexT?.EmgStop();
                    PostAlarm((int)AlarmKeys.eRotaryNotSafe);
                    Log.Write(this, $"Rotary AxisIndexT is not in Safety Position: {reason}");
                    return false;
                }
            }
            return bRet;
        }
        //Index 회전 시 무조건 확인 함수.
        private bool VerifyAllUnitsSafe(out string reason)
        {
            reason = null;

            // InputDieTransfer
            if (InputDieTransfer.IsPositionPlaceZSafety() == false)
            {
                AxisIndexT.EmgStop();
                PostAlarm((int)AlarmKeys.InputDieTransferPlaceZError);
                reason = "InputDieTransfer Not in Safety Zone";
                return false;
            }

            // OutputDieTransfer
            if (OutputDieTransfer.IsPositionPickZSafety() == false)
            {
                AxisIndexT.EmgStop();
                PostAlarm((int)AlarmKeys.OutputDieTransferPickZError);
                reason = "OutputDieTransfer Not in Safety Zone";
                return false;
            }

            // IndexLoadAligner
            if (IndexLoadAligner.IsPositionAlignZSafety() == false)
            {
                AxisIndexT.EmgStop();
                PostAlarm((int)AlarmKeys.IndexLoadAlignerZError);
                reason = "IndexLoadAligner Not in Safety Zone";
                return false;
            }

            // IndexChipProbeController
            if (IndexChipProbeController.IsPositionProbeZSafety() == false
                || IndexChipProbeController.IsPositionProbeCardZSafety() == false)
            {
                AxisIndexT.EmgStop();
                PostAlarm((int)AlarmKeys.IndexChipProbeControllerZError);
                reason = "IndexChipProbeController Not in Safety Zone";
                return false;
            }

            // IndexPlaceZ 확인 필요. - 선언시.

            return true;
        }
        #endregion

        #region Auto Seq 함수
        protected override void OnMakeSequence()
        {
            base.OnMakeSequence();

            //this.SequencePlayers.Add(CanRotate);
            this.SequencePlayers.Add(ExecuteUnitActionReady);
            this.SequencePlayers.Add(Rotate);
            this.SequencePlayers.Add(ExecuteUnitAction);
        }
        public int ExecuteUnitActionReady(bool isFine = false)
        {
            if (RunMode == UnitRunMode.Manual)
            {
                this.CurrentFunc = ExecuteUnitActionReady;
            }

            Task<int> task = ExecuteUnitActionReadyAsync(isFine);
            while (IsEndTask(task) == false)
            {
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
            if (RunMode == UnitRunMode.Manual)
            {
                this.CurrentFunc = ExecuteUnitAction;
            }

            Task<int> task = ExecuteUnitActionAsync(isFine);
            while (IsEndTask(task) == false)
            {
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
        //병렬로 동시에 시작되게 바꿔야 겠다. 
        //순차적으로 돌리면서 느리다. 
        protected int OnExecuteUnitAction(bool isFine = false)
        {
            bool pickupStartSet = false;
            try
            {
                Log.Write(UnitName, "OnExecuteUnitAction", "Start");
                // ====== 병렬로 동시에 시작 ======
                var tLoadAlign = (IndexLoadAligner != null)
                    ? Task.Run(() =>
                    {
                        try
                        {
                            //Log.Write("kkkkkkRotary", "IndexLoadAligner");
                            TaktStart("M-Align");
                            var th = Thread.CurrentThread;
                            if (th.Name == null) th.Name = "RunAlignSocketOnce(LoadAligner)";
                            try { return IndexLoadAligner.RunAlignSocketOnce(); }
                            catch (Exception ex) { Log.Write(ex); return -1; }
                        }
                        finally
                        {
                            TaktEnd("M-Align");
                            //Log.Write("kkkkkkRotary", "IndexLoadAligner End");
                        }
                    })
                    : Task.FromResult(0);

                var tProbe = (IndexChipProbeController != null)
                    ? Task.Run(() =>
                    {
                        try
                        {
                            //Log.Write("kkkkkkRotary", "IndexChipProbeController");
                            TaktStart("Plobe Inspection");
                            var th = Thread.CurrentThread;
                            if (th.Name == null) th.Name = "RunInspection(ProbeController)";
                            try { return IndexChipProbeController.RunInspection(); }
                            catch (Exception ex) { Log.Write(ex); return -1; }
                        }
                        finally 
                        {
                            TaktEnd("Plobe Inspection");
                            //Log.Write("kkkkkkRotary", "IndexChipProbeController end");
                        }
                    })
                    : Task.FromResult(0);

                var tUnloadAlign = (IndexUnloadAligner != null)
                    ? Task.Run(() =>
                    {
                    try
                    {
                        //Log.Write("kkkkkkRotary", "IndexUnloadAligner");
                        TaktStart("UnloadAlign");
                        var th = Thread.CurrentThread;
                        if (th.Name == null) th.Name = "RunAlignSocketOnce(UnloadAligner)";
                        try { return IndexUnloadAligner.RunAlignSocketOnce(); }
                        catch (Exception ex) { Log.Write(ex); return -1; }
                        }
                        finally
                        {
                            TaktEnd("UnloadAlign");
                            //Log.Write("kkkkkkRotary", "IndexUnloadAligner end");
                        }
                    })
                    : Task.FromResult(0);

                var tTrash = Task.Run(() =>
                {
                try
                {
                    //Log.Write("kkkkkkRotary", "RunTrashCanSocketOnce");
                    TaktStart("TrashCan");
                    var th = Thread.CurrentThread;
                    if (th.Name == null) th.Name = "RunTrashCanSocketOnce(Rotary)";
                    try { return RunTrashCanSocketOnce(); }
                    catch (Exception ex) { Log.Write(ex); return -1; }
                    }
                    finally
                    {
                        TaktEnd("TrashCan");
                        //Log.Write("kkkkkkRotary", "RunTrashCanSocketOnce end");
                    }
                });

                // ====== OutputDieTransfer (언로더 완료 후 처리) ======
                var odtTask = Task.Run(() =>
                {
                    try
                    {
                        //Log.Write("kkkkkkUnloader", "Start");
                        TaktStart("Pick Die");

                        // 언로더가 완료된 후 진행
                        if (tUnloadAlign.IsCompleted == false)
                        {
                            if (tUnloadAlign.Wait(30000) == false) // 30초 등 안전 타임아웃 설정 권장
                            {
                                Log.Write(UnitName, "OnExecuteUnitAction", "IndexUnloadAligner Wait Timeout");
                                return -1;
                            }
                        }
                            
                        int rUnload1 = tUnloadAlign.Result;
                        if (rUnload1 != 0)
                        {
                            Log.Write(UnitName, "OnExecuteUnitAction", "IndexUnloadAligner Failed before ODT pick");
                            return -1;  
                        }

                        if (OutputDieTransfer == null)
                            return 0;

                        // Unloader 위치에 Die 존재하는지 확인
                        MaterialDie unloadDie = null;
                        try 
                        { 
                            unloadDie = GetUnloadSocketMaterial(); 
                        }
                        catch (Exception ex) 
                        { Log.Write(ex); }

                        bool hasDie = unloadDie != null
                                      && unloadDie.Presence == Material.MaterialPresence.Exist;
                        if (!hasDie)
                        {
                            return 0;
                        }

                        //binArm에 신호를 보내서 binArm에서 die 정보를 받고 수행하도록 하자.
                        //if(unloadDie.ProcessSatate == MaterialProcessSatate.Skipped
                        //  || unloadDie.State == DieProcessState.Rejected
                        //  || unloadDie.State == DieProcessState.Skip )
                        //{
                        //    return 0;
                        //}

                        // [ADD] Bin1이 아니면 언로더 스킵 (Trash로 회전되어 배출됨)
                        // TEST
                        //if (!ShouldUnloadToOutput(unloadDie))
                        //{
                        //    Log.Write(UnitName, "[OutputDieTransfer] Skip unload: not Bin1");
                        //    return 0;
                        //}

                        //Log.Write("kkkkkkUnloader", "SetPickupStartEvent");
                        // 3. [수정] 핸드쉐이크 시작 전, Done 이벤트를 미리 리셋하여 이전 신호 제거
                        OutputDieTransfer.ResetPickupDoneEvent();

                        // 4. 시작 이벤트 발생
                        OutputDieTransfer.SetPickupStartEvent();
                        pickupStartSet = true;

                        var sw = System.Diagnostics.Stopwatch.StartNew();
                        double timeoutMs = 60000 * 2;
                        bool done = false;
                        while (sw.ElapsedMilliseconds < timeoutMs)
                        {
                            if (IsStop)
                            {
                                OutputDieTransfer.ReSetPickupStartEvent();
                                pickupStartSet = false;
                                return 0;
                            }

                            // 50ms 동안 대기. 신호 오면 true 반환하고 즉시 탈출.
                            // 신호 안 오면 false 반환하고 루프 다시 돔 (IsStop 체크)
                            //if (OutputDieTransfer.WaitPickupDoneEvent(10))
                            if (OutputDieTransfer.WaitPickupDoneEvent(100)) //디버깅때는 10ms, 실제로는 50ms 정도가 적당할 듯 (너무 짧으면 CPU 점유율 상승, 너무 길면 응답성 저하)
                            {
                                done = true;
                                break;
                            }
                        }

                        //Log.Write("kkkkkkUnloader", "SetPickupStartEvent done");
                        if (!done)
                        {
                            //AxisIndexT.EmgStop();
                            PostAlarm((int)AlarmKeys.eOutputDieTransferTimeout);
                            Log.Write(UnitName, "OutputDieTransfer Timeout");
                            return -1;
                        }

                        // 픽 성공 여부 확인
                        if (OutputDieTransfer.LastPickSucceeded)
                        {
                            try
                            {
                                int idx = OutputDieTransfer.GetUnloaderIndexNo();
                                if (idx >= 0 && idx < GetIndexCount())
                                {
                                    _sockets[idx].SetMaterialDie(null);
                                    _sockets[idx].SetState(RotarySocketState.Empty);
                                    Log.Write(UnitName, string.Format("[OutputDieTransfer] Socket {0} -> Empty", idx + 1));
                                }
                                else
                                {
                                    Log.Write(UnitName, string.Format("[OutputDieTransfer] Invalid Unloader Index: {0}", idx));
                                }
                            }
                            catch (Exception ex) { Log.Write(ex); }
                        }
                        else
                        {
                            Log.Write(UnitName, "[OutputDieTransfer] Pick sequence ended but failed. Socket kept.");
                        }

                        TaktEnd("Pick Die");
                        //Log.Write("kkkkkkUnloader", "End");
                        return 0;
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                        return -1;
                    }
                    finally
                    {
                        TaktEnd("Pick Die");
                    }
                });

                TaktStart("WaitAll");
                // ====== 모든 태스크 병렬 실행 후 대기 ======
                // [수정] WaitAll 대신 WhenAll 사용 고려 혹은 안전하게 대기
                // 기존 Task.WaitAll은 예외 발생 시 바로 throw하므로 try-catch로 감싸야 함
                //Task.WaitAll(new Task[] { tLoadAlign, tProbe, tUnloadAlign, tTrash, odtTask });
                var all = Task.WhenAll(tLoadAlign, tProbe, tUnloadAlign, tTrash, odtTask);
                bool completed = all.Wait(TimeSpan.FromSeconds(10)); // 장비 택타임 기준으로 조정
                if (!completed)
                {
                    // 완료 후 Start 신호 해제 (Output 쪽에서 인식 후 꺼주길 기대하지만, 안전장치로)
                    OutputDieTransfer.ReSetPickupStartEvent();
                    pickupStartSet = false;

                    PostAlarm((int)AlarmKeys.ExecuteUnitActionError);
                    Log.Write(UnitName, "OnExecuteUnitAction", "Task.WhenAll timeout");
                    return -1;
                }

                // 완료 후 Start 신호 해제 (Output 쪽에서 인식 후 꺼주길 기대하지만, 안전장치로)
                OutputDieTransfer.ReSetPickupStartEvent();
                pickupStartSet = false;

                int rLoad = tLoadAlign.Result;
                int rProbe = tProbe.Result;
                int rUnload = tUnloadAlign.Result;
                int rTrash = tTrash.Result;
                int rOdt = odtTask.Result;
                if (rLoad != 0 || rProbe != 0 || rUnload != 0 || rTrash != 0 || rOdt != 0)
                {
                    //이거.. 그냥 정지 시키는거 위험한데.
                    //AxisIndexT.EmgStop();

                    _executeActionNgLimit = IndexLoadAligner.Config.AlarmCount;
                    int ngNow = 0;// IncreaseExecuteActionNgCountAndGet();

                    //Log.Write(UnitName,
                    //    string.Format("OnExecuteUnitAction Fail (LoadAligner={0}, Probe={1}, UnloadAligner={2}, TrashCan={3}, ODT={4})",
                    //        rLoad, rProbe, rUnload, rTrash, rOdt));
                    

                    if (rLoad != 0)
                    {
                        ngNow = IncreaseExecuteActionNgCountAndGet();
                        //PostAlarm((int)AlarmKeys.IndexLoadAlignerError);
                    }
                    else
                        ngNow = _executeActionNgCount; // 참고용 로그

                    Log.Write(UnitName,
                        $"OnExecuteUnitAction Fail (LoadAligner={rLoad}, " +
                        $"Probe={rProbe}, UnloadAligner={rUnload}, TrashCan={rTrash}, " +
                        $"ODT={rOdt}) / NGCount={ngNow}/{_executeActionNgLimit}");

                    // 누적 횟수 도달 시 알람
                    if (ngNow >= _executeActionNgLimit)
                    {
                        PostAlarm((int)AlarmKeys.IndexLoadAlignerError); // 또는 전용 AlarmKeys 추가 권장
                        Log.Write(UnitName, $"[NGCOUNT] ExecuteUnitAction NG limit reached: {ngNow}/{_executeActionNgLimit}");

                        ResetExecuteActionNgCount("IndexLoadAlignerErrorRaised");
                    }
                    
                    TaktEnd("WaitAll");
                    return -1;
                }

                ResetExecuteActionNgCount("OnExecuteUnitActionSuccess");

                TaktEnd("WaitAll");
                //Log.Write(UnitName, "OnExecuteUnitAction", "End");
                return 0;
            }
            catch (AggregateException ae) // Task.WaitAll 예외 처리
            {
                //AxisIndexT.EmgStop();
                foreach (var e in ae.InnerExceptions)
                {
                    Log.Write(UnitName, $"OnExecuteUnitAction Exception: {e.Message}");
                }
                return -1;
            }
            catch (Exception ex)
            {
                //AxisIndexT.EmgStop();
                Log.Write(ex);
                return -1;
            }
            finally
            {
                if (pickupStartSet)
                {
                    try 
                    { 
                        OutputDieTransfer.ReSetPickupStartEvent();
                        Log.Write(UnitName, "OnExecuteUnitAction", "OutputDieTransfer.ReSetPickupStartEvent()");
                    }
                    catch (Exception ex) 
                    { Log.Write(ex); }
                }
            }
        }

        // ===== NG Count Policy =====
        private readonly object _ngCountLock = new object();
        private int _executeActionNgCount = 0;

        // 임계값(레시피/설정으로 빼도 됨)
        private int _executeActionNgLimit = 3;

        // 옵션 정책
        private bool _resetNgOnAlarm = true;      // 알람 발생 시 카운터 리셋
        private bool _resetNgOnNewLot = true;     // 새 시작/리셋 시 리셋
        private bool _resetNgOnNewInput = true;   // 신규 투입 시 리셋(원할 때)
        private void ResetExecuteActionNgCount(string reason = "")
        {
            lock (_ngCountLock)
            {
                _executeActionNgCount = 0;
            }
            //Log.Write(UnitName, $"[NGCOUNT] Reset ExecuteUnitAction NG Count. reason={reason}");
        }

        private int IncreaseExecuteActionNgCountAndGet()
        {
            lock (_ngCountLock)
            {
                _executeActionNgCount++;
                return _executeActionNgCount;
            }
        }

        // Bin 필터: 검사 결과가 Bin1일 때만 언로더 수행
        private bool ShouldUnloadToOutput(MaterialDie die)
        {
            ////Test
            return true;

            //if (die == null) return false;
            //// 검사 결과(Binning)에서 Bin1 판정
            //var bin = die.TesterResult?.BinningResult;
            //if (bin == null) return false;
            //// 우선순위: BinNo == 1
            //if (bin.BinNo == 1)
            //    return true;
            //// 보조: 라벨이 "Bin1" 또는 "Bin 1" (대소문자 무시)
            //var label = bin.BinLabel ?? string.Empty;
            //if (label.Equals("bin1", StringComparison.OrdinalIgnoreCase)
            //    || label.Equals("bin 1", StringComparison.OrdinalIgnoreCase))
            //    return true;
            //return false;
        }

        // [ADD] Trash로 버릴 때 상태/존재 플래그를 일관되게 정리
        private void MarkDieDiscarded(MaterialDie die)
        {
            if (die == null) 
                return;

            try
            {
                //die.State = DieProcessState.Rejected; // 폐기 표식(프로젝트 규약 그대로 사용)
                //die.Presence = Material.MaterialPresence.NotExist; // 물리적으로 없음
                die.ProcessSatate = Material.MaterialProcessSatate.Completed;
            }
            catch { /* 방어적 */ }
        }


        private int RunTrashCanSocketOnce()
        {
            int nRet = 0;
            int nIndexTrash = GetTrashCanIndexNo();

            this.SetVacuum(nIndexTrash, false);
            Thread.Sleep(1);
            this.SetBlow(nIndexTrash, true);
            Thread.Sleep(1);
            if (SetTrashEjector(true) == false)
            {
                Log.Write(UnitName, "[RunTrashCanSocketOnce] ", "TrashEjector ON fail");
                return -1;
            }

            if (SetTrashVacuum(true) == false)
            {
                Log.Write(UnitName, "[RunTrashCanSocketOnce] ", "TrashVacuum ON fail");
                SetTrashEjector(false);
                return -1;
            }
            //일정 시간 대기
            WaitByTime(GetClearTimeMs()); // 기본: 100ms
            
            // [ADD] Trash 위치 소켓에 남은 Die가 있으면 폐기 상태로 마킹 후 제거
            var trashSock = GetTrashCanSocketInfo();
            var trashDie = trashSock?.GetMaterialDie();
            if (trashDie != null && trashDie.Presence == Material.MaterialPresence.Exist)
            {
                MarkDieDiscarded(trashDie);
            }
            trashSock?.SetMaterialDie(null);

            if (SetTrashVacuum(false) == false)
            {
                Log.Write(UnitName, "[RunTrashCanSocketOnce] ", "TrashVacuum OFF fail");
                SetTrashEjector(false);
                return -1;
            }
            if (SetTrashEjector(false) == false)
            {
                Log.Write(UnitName, "[RunTrashCanSocketOnce] ", "TrashEjector OFF fail");
                return -1;
            }

            var Socket = GetTrashCanSocketInfo();
            // [ADD] 실제 폐기되는 Die의 상태/존재 플래그 정리 후 제거
            var removedDie = Socket.GetMaterialDie();
            if (removedDie != null)
                MarkDieDiscarded(removedDie);

            Socket.SetMaterialDie(null);

            this.SetVent(nIndexTrash, false);
            this.SetBlow(nIndexTrash, false);
            Thread.Sleep(1);

            bool useSocket = this.Config.GetUseSocket(nIndexTrash);
            if (useSocket)
            {
                this.SetVacuum(nIndexTrash, true);
            }
            else
            {
                this.SetVacuum(nIndexTrash, false);
            }

            Log.Write(UnitName, "[RunTrashCanSocketOnce] ", "Clear Comp.");
            return nRet;
        }

        private int WaitPostActionSettled(bool needLoadWait, int timeoutMs)
        {
            var timeout = new TimeoutChecker(timeoutMs, autoStart: true); 
            while (true)
            {
                if (IsStop) 
                { 
                    return 0; 
                }

                // 1) Load 소켓 투입 완료 대기
                bool loadOk = true;
                if (needLoadWait)
                {
                    var socket = GetLoadSocketInfo();
                    var loadDie = socket?.GetMaterialDie();
                    loadOk = (loadDie != null && loadDie.Presence == Material.MaterialPresence.Exist);
                    if (loadOk)
                    {
                        socket.SetState(RotarySocketState.Loaded);
                    }
                }

                if (loadOk)
                {
                    // load 완료 시 즉시 요청 해제(중복 place 트리거 방지)
                    RequestInputDieTrDie = false;
                    break;
                }
                
                if (timeout.IsCompleted)
                {
                    if (!loadOk)
                    {
                        Log.Write(UnitName, "[WaitPostActionSettled] Load socket die not supplied (timeout)");
                        PostAlarm((int)AlarmKeys.InputDieTransferTimeout);
                    }
                    return -1;
                }
                Thread.Sleep(1);
            }

            // 인덱스 변경 이벤트는 회전 완료 루트(WaitIndexMoveDone)에서만 발생시킴
            // 이게 여기 들어가는거 맞음?
            // 이거 때문에 디버깅때 겁나 느려진듯.
            //OnLoadIndexChanged(GetLoadIndexNo()); // <- 여기는 들어가면 안되겠다.
            return 0;
        }

        public int Rotate(bool isFine = false)
        {
            int nRet = 0;
            if (RunMode == UnitRunMode.Manual)
            {
                this.CurrentFunc = Rotate;
            }

            TaktStart("MoveRotate");
            nRet = MovePositionRotate();
            if (nRet != 0)
            {
                //AxisIndexT.EmgStop();
                PostAlarm((int)AlarmKeys.RotaryIndexMoveError);
                Log.Write(UnitName, "Rotate Fail");
                return -1;
            }
            TaktEnd("MoveRotate");

            TaktStart("WaitDoneRotate");
            nRet = WaitIndexMoveDone();
            if (nRet != 0)
            {
                //AxisIndexT.EmgStop();
                PostAlarm((int)AlarmKeys.RotaryIndexMoveError);
                Log.Write(UnitName, "Rotate Fail");
                return -1;
            }
            TaktEnd("WaitDoneRotate");

            return nRet;
        }

        //단위 동작.
        public int MovePositionRotate(bool isFine = false)
        {
            string strmsg;
            if(VerifyAllUnitsSafe(out strmsg) == false)
            {
                Log.Write(UnitName, "MovePositionRotate Interlock Fail");
                return -1;
            }
           
            Task<int> task = MovePositionAsyncRotate(isFine);
            while (IsEndTask(task) == false)
            {
                //VerifyAllUnitsSafe() 중복 검사
                if (VerifyAllUnitsSafe(out strmsg) == false)
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
            if (TryMoveIndexNext(out reason) == false)
            {
                // 재시도 루프(로그만)
                Log.Write(UnitName, $"TryMoveIndexNext Fail: {reason}");
                Thread.Sleep(10);
                return -1;
            }
            return nRet;
        }
        #endregion

        #region Clear Sockets
        public int InitializeAfterHome(bool isFine = false)
        {
            Task<int> task = InitializeAfterHomeAsync(isFine);
            while (IsEndTask(task) == false)
            {
                Thread.Sleep(1);
            }
            return task.Result;
        }
        public Task<int> InitializeAfterHomeAsync(bool isFine = false)
        {
            return Task.Run(() =>
            {
                OnInitializeAfterHome(isFine);
                return 0;
            });
        }
        private int OnInitializeAfterHome(bool isFine = false)
        {
            int nRet = 0;
            try
            {
                int socketCount = GetIndexCount();
                if (SetTrashEjector(true) == false)
                {
                    Log.Write(UnitName, "[InitializeAfterHome] TrashEjector ON fail");
                    return -1;
                }

                if (SetTrashVacuum(true) == false)
                {
                    Log.Write(UnitName, "[InitializeAfterHome] TrashVacuum ON fail");
                    SetTrashEjector(false);
                    return -1;
                }

                for (int i = 0; i < socketCount; i++)
                {
                    string reason = string.Empty;
                    // 1) Safe-Zone check.
                    if (VerifyAllUnitsSafe(out reason) == false)
                    {
                        Log.Write("Rotary", $"Index Move Blocked: {reason}");
                        return -1;
                    }

                    // 취소 요청 감지: 예외 대신 정상 종료 코드 반환
                    //this.CalcelToken?.Token.ThrowIfCancellationRequested();
                    //if (this.CalcelToken?.Token.IsCancellationRequested == true || this.IsStop)
                    //if(IsStop)
                    //{
                    //    SetTrashVacuum(false);
                    //    SetTrashEjector(false);
                    //    SetBlow(CrashCanIdx, false);
                    //    Log.Write(UnitName, "[InitializeAfterHome] Canceled");
                    //    return 0;
                    //}
                    //이거하면 Manual에 아에 동작을 안하는데...
                    //이거 진짜 고민 필요하다..
                    //if (IsStop) { /* IO 복구/정리 */ return 0; }


                    int CrashCanIdx = GetTrashCanIndexNo();

                    SetVacuum(CrashCanIdx, false);
                    Thread.Sleep(1);
                    SetBlow(CrashCanIdx, true);
                    //일정 시간 대기
                    WaitByTime(GetClearTimeMs()); // 기본: 500ms

                    // 2) 다음 인덱스로 한 칸 이동 (전체 소켓 수 만큼 반복 → 원위치 복귀)
                    nRet = MovePositionRotate();
                    if (nRet != 0)
                    {
                        Log.Write(UnitName, $"[InitializeAfterHome] Index move start fail: {reason}");
                        PostAlarm((int)AlarmKeys.RotaryIndexMoveError);
                        return -1;
                    }

                    nRet = WaitIndexMoveDone();
                    if (nRet != 0)
                    {
                        Log.Write(UnitName, "[InitializeAfterHome] Index move wait timeout");
                        PostAlarm((int)AlarmKeys.RotaryIndexMoveError);
                        return -1;
                    }

                    SetBlow(CrashCanIdx, false);
                    Log.Write(UnitName, $"[InitializeAfterHome] Clear Comp. {i}");
                }

                if (SetTrashVacuum(false) == false)
                {
                    Log.Write(UnitName, "[InitializeAfterHome] TrashVacuum OFF fail");
                    SetTrashEjector(false);
                    return -1;
                }

                if (SetTrashEjector(false) == false)
                {
                    Log.Write(UnitName, "[InitializeAfterHome] TrashEjector OFF fail");
                    return -1;
                }

                this.ClearSockets();

                return nRet;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return -1;
            }
            finally
            {
                SetTrashVacuum(false);
                SetTrashEjector(false);
                for(int i=0; i < GetIndexCount(); i++)
                {
                    SetVacuum(i, false);
                    SetBlow(i, false);
                    SetVent(i, false);
                }
            }
        }
        private int GetClearTimeMs()
        {
            // 0 또는 음수면 기본 500ms로 사용, 그 외 값은 그대로 사용
            int v = (Config != null) ? Config.ClearTimeMs : 0;
            return (v <= 0) ? 100 : v;
        }
        #endregion

        public void ResetForNewRun(bool clearSockets = true, bool moveIndexToSafe = true)
        {
            // 1) 런타임/시퀀스 플래그 초기화
            RequestInputDieTrDie = false;
            RequestOutputDieTrDie = false;
            _moveStartTime = DateTime.MinValue;

            if (_resetNgOnNewLot)
                ResetExecuteActionNgCount("ResetForNewRun");

            // 2) OutputDieTransfer 핸드셰이크 잔여 신호 정리
            try
            {
                OutputDieTransfer?.ReSetPickupStartEvent();
                OutputDieTransfer?.ResetPickupDoneEvent();
            }
            catch (Exception ex)
            {
                Log.Write(UnitName, $"[ResetForNewRun] ODT handshake reset failed: {ex.Message}");
            }

            // 3) IO 안전 상태(실기에서만)로 복귀
            try
            {
                bool sim = (Config?.IsSimulation == true) || (Config?.IsDryRun == true) || (Config?.IsUnitDryRun == true);
                if (!sim)
                {
                    int cnt = GetIndexCount();
                    for (int i = 0; i < cnt; i++)
                    {
                        try { SetBlow(i, false); } catch { }
                        try { SetVent(i, false); } catch { }
                        try { SetVacuum(i, false); } catch { }
                    }
                    try { SetTrashEjector(false); } catch { }
                    try { SetTrashVacuum(false); } catch { }
                }
            }
            catch (Exception ex)
            {
                Log.Write(UnitName, $"[ResetForNewRun] IO safe reset failed: {ex.Message}");
            }

            // 4) 소켓 사용 설정/내용 초기화
            lock (_socketLock)
            {
                try
                {
                    RefreshSocketUsage(); // Config.UseSocket1~8 반영
                    if (clearSockets && _sockets != null)
                    {
                        foreach (var s in _sockets)
                        {
                            s?.SetMaterialDie(null);
                            s?.SetState(RotarySocketState.Empty);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Write(UnitName, $"[ResetForNewRun] Socket reset failed: {ex.Message}");
                }
                ClearSocketData();
            }

            // 6) UI에 현재 Load 인덱스 알림(선택)
            try { OnLoadIndexChanged(GetLoadIndexNo()); } catch { }
        }

        public int MoveToTeachingPositionBySelectionIndex(int teachingSelIndex, bool isFine = false)
        {
            if (Config == null)
                return -1;

            string tpName;
            if (!Config.GetTeachingPositionName(teachingSelIndex, out tpName) || string.IsNullOrWhiteSpace(tpName))
                return -1;

            RotaryConfig.TeachingPositionName en;
            if (!Enum.TryParse(tpName, out en))
                return -1;

            switch (en)
            {
                // ===== AlignZ Index Up/Ready (Index1~8 -> 0~7) =====
                //case RotaryConfig.TeachingPositionName.AlignZ_Index1_Up: 
                //    nIndex = 0; 
                //    return MovePositionAlignZUp(nIndex, isFine);

                default:
                    return -1;
            }

            //return 0;
        }


    }
}