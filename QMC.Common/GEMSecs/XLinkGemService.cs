using QMC.Common.Component;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using XLINKGEMLib;

namespace QMC.Common.GEMSecs
{
    public sealed class XLinkGemService : BaseComponent, IDisposable
    {
        // =====================================================================
        // DEFINE.H (C# Port) - 이 클래스에서 직접 사용
        // =====================================================================
        public static class Define
        {
            public static class Ini
            {
                public const string HSMS = "HSMS";
                public const string HSMS_PORT = "PORT";
                public const string HSMS_DEVICEID = "DEVICEID";
                public const string HSMS_LINKTEST = "LINKTEST";
                public const string HSMS_RETRY = "RETRY";
                public const string HSMS_T3 = "T3";
                public const string HSMS_T5 = "T5";
                public const string HSMS_T6 = "T6";
                public const string HSMS_T7 = "T7";
                public const string HSMS_T8 = "T8";

                public const string GEM_MDLN = "MDLN";
                public const string GEM_SOFTREV = "SOFTREV";
            }

            public enum CONTROL_STATE : short
            {
                CONTROL_UNKOWN = 0,
                CONTROL_EQUIPMENT_OFFLINE = 1,
                CONTROL_GOING_TO_ONLINE = 2,
                CONTROL_HOST_OFFLINE = 3,
                CONTROL_ONLINE_LOCAL = 4,
                CONTROL_ONLINE_REMOTE = 5,
            }

            public enum DEFAULT_CONTROL_STATE : short
            {
                DFFAULT_EQUIPMENT_OFFLINE = 0,
                DEFAULT_HOST_OFFLINE = 1,
                DEFAULT_ONLINE_LOCAL = 2,
                DEFAULT_ONLINE_REMOTE = 3
            }

            public enum SPOOL_STATE : short
            {
                SPOOL_INACTIVE = 0,
                SPOOL_ACTIVE = 1,
                SPOOL_FULL = 2,
                SPOOL_PURGE = 3,
                SPOOL_TRANSMIT = 4,
                SPOOL_NOSPOOL = 5,
                SPOOL_POWEROFF = 6,
                SPOOL_POWERON = 7,
            }

            public const string UNIT_ID = "101";
            public const string PORT1_PORTID = "1";

            public static class SVID
            {
                public const int SVID_CONTROL_STATE = 102;
                public const int SVID_EQ_STATE = 103;
                public const int SVID_ALARM_STATE = 104;
                public const int SVID_PPID = 106;
                public const int SVID_PORT_ID = 120;
                public const int SVID_LOT_ID = 123;
                public const int SVID_ALCD = 130;
                public const int SVID_ALID = 131;
                public const int SVID_ALTX = 132;
                public const int SVID_CURRENT_TIME = 200;
                public const int SVID_ITEM_FILE = 300;
                public const int SVID_BIN_FILE = 301;
                public const int SVID_PRDFILE_PATH = 302;
                public const int SVID_PRDFILE_UPLOAD_STATE = 303;
                public const int SVID_PROBE_TOTAL_CNT = 304;
                public const int SVID_PROBE_GOOD_CNT = 305;
                public const int SVID_PROBE_NG_CNT = 306;
                public const int SVID_PROBE_STATIC = 307;
                public const int SVID_PROBE_BINS = 308;
                public const int SVID_PROBE_TEST_OFFSET = 309;
                public const int SVID_ECID = 331;
                public const int SVID_ECVAL = 332;
                public const int SVID_START_TIME = 333;
                public const int SVID_END_TIME = 334;
            }

            public static class CEID
            {
                public const int CEID_CONTROL_STATUS = 1001;
                public const int CEID_MACHINE_STATUS = 1002;

                public const int CEID_PORT_LOADING_COMPLEATE = 1012;
                public const int CEID_BCR_READ_COMPLETE = 1021;
                public const int CEID_PPSELECED = 1011;
                public const int CEID_PROCESS_START = 1041;
                public const int CEID_WAFER_LOADING_COMPLEATE = 1022;
                public const int CEID_PRDFILE_LOAD_COMPLEATE = 1053;
                public const int CEID_DCOL_DATA = 1054;
                public const int CEID_WAFER_UNLOADING_COMPLEATE = 1023;
                public const int CEID_PROCESS_END = 1042;
                public const int CEID_PORT_UNLOADING_COMPLEATE = 1013;

                public const int CEID_CONTROLSTATE_OFFLINE = 1050;
                public const int CEID_CONTROLSTATE_LOCAL = 1051;
                public const int CEID_CONTROLSTATE_REMOTE = 1052;
                public const int CEID_ECID_CHANGE = 1055;
            }

            public static class ECID
            {
                public const int ECID_DEFAULT_COMM_STATE = 3000;
                public const int ECID_T3 = 3001;
                public const int ECID_T5 = 3002;
                public const int ECID_T6 = 3003;
                public const int ECID_T7 = 3004;
                public const int ECID_T8 = 3005;
                public const int ECID_RETRY = 3006;
                public const int ECID_CONVERSATION_TIMEOUT = 3007;
                public const int ECID_ESTABLISH_TIMEOUT = 3008;
                public const int ECID_LINKTEST = 3009;
                public const int ECID_DEFAULT_CONTROL_STATE = 3010;
                public const int ECID_SOFTREV = 3011;
                public const int ECID_TIME_FORMAT = 3012;
            }

            public static class PROCESS
            {
                public const string PROCESS_UNDEFINE = "0";
                public const string PROCESS_INIT = "1";
                public const string PROCESS_IDLE = "2";
                public const string PROCESS_SETUP = "3";
                public const string PROCESS_READY = "4";
                public const string PROCESS_EXECUTING = "5";
                public const string PROCESS_PAUSED = "6";
                public const string PROCESS_CANCEL = "7";
            }

            public static class PRC_STATE
            {
                public const int PRC_STATE_NONE = 0;
                public const int PRC_STATE_NOT_INPUT = 1;
                public const int PRC_STATE_INPUT = 2;
                public const int PRC_STATE_NORMAL_END = 3;
                public const int PRC_STATE_ABNORMAL_END = 4;
                public const int PRC_STATE_CANCEL = 5;
                public const int PRC_STATE_UNDEFINE = 6;
            }

            public static class OPERATION_MODE
            {
                public const int OPERATION_MODE_AUTO = 1;
                public const int OPERATION_MODE_CAL = 3;
                public const int OPERATION_MODE_PROBE_CARD_CANGE = 4;
                public const int OPERATION_MODE_RESORTING = 6;
            }
        }

        private readonly object _sync = new object();
        private XLinkGEM _gem;
        private bool _disposed;

        public XLinkGemServiceConfig Config { get; set; }

        public bool IsStarted { get; private set; }

        // ===== 런타임 상태 =====
        public string SecsDirectory { get; private set; }
        public int TimeFormatMode { get; private set; } = 1; // 0=12, 1=16, 2=14

        // CEID 텍스트 캐시 (GetCEIDTEXT)
        private readonly Dictionary<int, string> _ceidTextCache = new Dictionary<int, string>();

        // SECS 수신 폴링 스레드
        private Thread _secsPollThread;
        private volatile bool _secsPollRun;

        // ===== 외부로 노출할 이벤트 =====
        public event EventHandler<GemControlStateChangedEventArgs> ControlStateChanged;
        public event EventHandler<GemAlarmEventArgs> AlarmEventReceived;
        public event EventHandler<GemErrorEventArgs> ErrorEventReceived;
        public event EventHandler<GemRemoteCommandEventArgs> RemoteCommandReceived;
        public event EventHandler<GemCommunicationStateEventArgs> CommunicationStateChanged;
        public event EventHandler<GemTerminalMessageEventArgs> TerminalMessageReceived;
        public event EventHandler<GemReceiveSecsMsgEventArgs> ReceiveSecsMsg;

        public event EventHandler<GemEquipmentConstantsChangedEventArgs> EquipmentConstantsChanged;
        public event EventHandler<GemSecsMessageReceivedEventArgs> SecsMessageReceived;

        public void Create()
        {
            ThrowIfDisposed();

            lock (_sync)
            {
                if (_gem != null) 
                    return;

                _gem = new XLinkGEM();
                HookEvents();
            }
        }

        public void SetConfig(XLinkGemServiceConfig config)
        {
            if (config == null) 
                throw new ArgumentNullException(nameof(config));
            
            ThrowIfDisposed();

            lock (_sync)
            {
                Config = config;
            }
        }

        public void ApplyConfig()
        {
            EnsureCreated();

            XLinkGemServiceConfig cfg;
            lock (_sync) cfg = Config;

            if (cfg == null)
                throw new InvalidOperationException("Config가 설정되지 않았습니다. SetConfig() 먼저 호출하세요.");

            cfg.Reset();
            cfg.Validate();

            if (cfg.LogEnabled)
                Directory.CreateDirectory(cfg.LogPath);

            ConfigureBasic(
                cfg.Ip,
                cfg.Port,
                cfg.DevId,
                cfg.ModelName,
                cfg.SoftRev,
                passiveMode: cfg.Mode == XLinkGemServiceConfig.HsmsMode.Passive,
                cfg.T3, cfg.T5, cfg.T6, cfg.T7, cfg.T8,
                cfg.LinkTestInterval,
                cfg.LogEnabled ? cfg.LogPath : null,
                cfg.LogPrefix,
                cfg.LogKeepDays,
                cfg.EstablishTimeout,
                cfg.TimeFormatDigits);
        }

        public void StartWithConfig(XLinkGemServiceConfig config, bool setOnlineRemote = false)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            SetConfig(config);

            if (!config.Enable)
                return;

            Create();
            ApplyConfig();
            Start();

            if (setOnlineRemote)
                SetOnlineRemote();
        }

        public void Configure(Action<XLinkGEM> configure)
        {
            if (configure == null) throw new ArgumentNullException(nameof(configure));
            EnsureCreated();

            lock (_sync)
            {
                configure(_gem);
            }
        }

        public void ConfigureBasic(
            string ip,
            short port,
            short devId,
            string modelName,
            string softRev,
            bool passiveMode,
            short t3, short t5, short t6, short t7, short t8,
            short linkTestInterval,
            string logPath,
            string logPrefix,
            short logKeepDays,
            short establishTimeout,
            short timeFormatDigits)
        {
            EnsureCreated();

            lock (_sync)
            {
                _gem.MDLN = modelName ?? "";
                _gem.SOFTREV = softRev ?? "";

                _gem.SetIP(ip ?? "127.0.0.1");
                _gem.SetPort(port);
                _gem.SetDevID(devId);

                if (!string.IsNullOrWhiteSpace(logPath))
                    _gem.SetLog(logPath, logPrefix ?? "GEM");
                if (logKeepDays > 0)
                    _gem.SetLogKeepPeriod(logKeepDays);

                _gem.SetT3Timeout(t3);
                _gem.SetT5Timeout(t5);
                _gem.SetT6Timeout(t6);
                _gem.SetT7Timeout(t7);
                _gem.SetT8Timeout(t8);
                _gem.SetLinkTestInterval(linkTestInterval);

                if (passiveMode) _gem.SetPassive();
                else _gem.SetActive();

                if (establishTimeout > 0)
                    _gem.SetEstablishTimeout(establishTimeout);

                // [FIX] 중복/오동작 제거:
                // timeFormatDigits(0/1/2)를 그대로 SetTimeFormat에 넘기면 안 됨.
                ApplyTimeFormatUnsafe(timeFormatDigits);
            }
        }

        private void ApplyTimeFormatUnsafe(int mode)
        {
            // mode: 0=12, 1=16, 2=14 (MFC 호환)
            TimeFormatMode = mode;

            try
            {
                if (mode == 0)
                {
                    _gem.SetTimeFormat(12);
                    Diag("SetTimeFormat=12 applied (mode=0)");
                }
                else if (mode == 1)
                {
                    _gem.SetTimeFormat(16);
                    Diag("SetTimeFormat=16 applied (mode=1)");
                }
                else if (mode == 2)
                {
                    _gem.SetTimeFormat(14);
                    Diag("SetTimeFormat=14 applied (mode=2)");
                }
                else
                {
                    Diag($"SetTimeFormat skipped: invalid mode={mode}");
                }
            }
            catch (Exception ex)
            {
                Diag($"SetTimeFormat FAIL: mode={mode} - {ex.GetType().Name}: {ex.Message}");
            }
        }

        public void Start()
        {
            EnsureCreated();

            lock (_sync)
            {
                if (IsStarted) return;

                bool ok = _gem.Start();
                if (!ok)
                    throw new InvalidOperationException("XLinkGEM Start() 실패");

                // [ADD] Start 후 COMM Enable
                EnableCommunicationUnsafe();

                IsStarted = true;
                Diag("GEM Start OK");
            }
        }

        public void Stop()
        {
            if (_gem == null) return;

            // [ADD] 폴링 스레드 종료
            try { StopSecsPolling(); } catch { }

            lock (_sync)
            {
                try { _gem.Stop(); } catch { }
                IsStarted = false;
            }
        }

        public void SetOffline()
        {
            EnsureCreated();
            lock (_sync) _gem.SetOffline();
        }

        public void SetOnlineLocal()
        {
            EnsureCreated();
            lock (_sync) _gem.SetOnlineLocal();
        }

        public void SetOnlineRemote()
        {
            EnsureCreated();
            lock (_sync) _gem.SetOnlineRemote();
        }

        public bool EventReport(int ceid)
        {
            EnsureCreated();
            lock (_sync) return _gem.EventReport(ceid);
        }

        public bool SetVariableValue(int vid, string value)
        {
            EnsureCreated();
            lock (_sync) return _gem.SetVariableValue(vid, value ?? "");
        }

        public bool AlarmSet(int alid)
        {
            EnsureCreated();
            lock (_sync) return _gem.AlarmSet(alid);
        }

        public bool AlarmClear(int alid)
        {
            EnsureCreated();
            lock (_sync) return _gem.AlarmClear(alid);
        }

        public void AcceptRemoteCommand(int msgId, short acceptCode = 0x00)
        {
            EnsureCreated();
            lock (_sync) _gem.AcceptRCMD(msgId, acceptCode);
        }

        public void RejectRemoteCommand(int msgId, short rejectCode)
        {
            EnsureCreated();
            lock (_sync) _gem.RejectRCMD(msgId, rejectCode);
        }

        public IReadOnlyList<GemRcmdParam> ReadRemoteCommandParams(short maxParams = 32)
        {
            EnsureCreated();

            var list = new List<GemRcmdParam>();
            lock (_sync)
            {
                for (short i = 0; i < maxParams; i++)
                {
                    string name;
                    string value;

                    try
                    {
                        name = _gem.GetRCMDParamName(i);
                        value = _gem.GetRCMDParamValue(i);
                    }
                    catch
                    {
                        break;
                    }

                    if (string.IsNullOrWhiteSpace(name))
                        break;

                    list.Add(new GemRcmdParam(i, name, value));
                }
            }

            return list.AsReadOnly();
        }

        // =====================================================================
        // SecsGemStart() 대응: 정의파일 로드 + 포맷/커스텀메시지 세팅 + 폴링 시작
        // =====================================================================
        public void InitializeGemDefinitions(
            string secsDirectory,
            int timeFormatMode,
            bool setCustomSecsMessageAll = true,
            bool startSecsPolling = true)
        {
            if (string.IsNullOrWhiteSpace(secsDirectory))
                throw new ArgumentNullException(nameof(secsDirectory));

            EnsureCreated();

            SecsDirectory = secsDirectory;
            TimeFormatMode = timeFormatMode;

            lock (_sync)
            {
                Diag($"InitializeGemDefinitions: dir={secsDirectory}, timeFormatMode={timeFormatMode}");

                ApplyTimeFormatUnsafe(timeFormatMode);

                try
                {
                    _gem.SetFormatALID(54);
                    _gem.SetFormatCEID(52);
                    _gem.SetFormatECID(54);
                    _gem.SetFormatRPTID(52);
                    _gem.SetFormatVID(54);
                    _gem.SetFormatTRID(54);
                    _gem.SetFormatDATAID(52);
                    Diag("SetFormat* applied");
                }
                catch (Exception ex)
                {
                    Diag($"SetFormat* FAIL: {ex.GetType().Name}: {ex.Message}");
                }

                TryLoadDefineFile(() => _gem.LoadVID(Path.Combine(SecsDirectory, "INNOBIZ_PROBER_SVID.TXT")));
                TryLoadDefineFile(() => _gem.LoadCEID(Path.Combine(SecsDirectory, "INNOBIZ_PROBER_CEID.TXT")));
                TryLoadDefineFile(() => _gem.LoadALID(Path.Combine(SecsDirectory, "INNOBIZ_PROBER_ALARM.TXT")));
                TryLoadDefineFile(() => _gem.LoadECID(Path.Combine(SecsDirectory, "INNOBIZ_PROBER_ECID.TXT")));
                TryLoadDefineFile(() => _gem.LoadAlarm(Path.Combine(SecsDirectory, "INNOBIZ_PROBER_ALARM.TXT")));
                TryLoadDefineFile(() => _gem.LoadRCMD(Path.Combine(SecsDirectory, "INNOBIZ_PROBER_RCMD.TXT")));

                try 
                { 
                    _gem.LoadDefine(); 
                    Diag("LoadDefine OK"); 
                } 
                catch (Exception ex) 
                { Diag($"LoadDefine FAIL: {ex.Message}"); }

                if (setCustomSecsMessageAll)
                {
                    try 
                    { _gem.SetCustomSecsMessageAll(); 
                        Diag("SetCustomSecsMessageAll OK"); 
                    } 
                    catch (Exception ex) 
                    { Diag($"SetCustomSecsMessageAll FAIL: {ex.Message}"); }
                }

                try { _gem.AddRCMD(0, "PP-SELECT"); } catch { }
                try { _gem.AddRCMD(0, "UNLOAD"); } catch { }
                try { _gem.AddRCMD(0, "START"); } catch { }

                ApplyMfcDisableAutoReplyUnsafe();

                BuildCeidTextCacheUnsafe();
            }

            if (startSecsPolling)
                StartSecsPolling();
        }

        private void TryLoadDefineFile(Func<bool> loader)
        {
            try
            {
                bool ok = loader();
                Diag($"LoadDefineFile: {(ok ? "OK" : "FAIL(false)")}");
            }
            catch (Exception ex)
            {
                Diag($"LoadDefineFile exception: {ex.GetType().Name}: {ex.Message}");
            }
        }

        private void BuildCeidTextCacheUnsafe()
        {
            _ceidTextCache.Clear();

            short count;
            try { count = _gem.GetCEIDCount(); }
            catch { return; }

            for (short i = 0; i < count; i++)
            {
                int ceid = 0;
                string name = "";
                string text = "";
                try
                {
                    _gem.GetCEID(i, ref ceid, ref name, ref text);
                }
                catch
                {
                    continue;
                }

                if (ceid <= 0) continue;
                if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(text)) continue;

                _ceidTextCache[ceid] = string.IsNullOrWhiteSpace(text) ? (name ?? "") : text;
            }
        }

        public string GetCeidText(int ceid)
        {
            lock (_sync)
            {
                return _ceidTextCache.TryGetValue(ceid, out var t) ? t : "";
            }
        }

        public string GetCurrentTimeString()
        {
            var now = DateTime.Now;

            if (TimeFormatMode == 1) // 16자리: yyyyMMddHHmmss00
                return now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture) + "00";

            if (TimeFormatMode == 2) // 14자리: yyyyMMddHHmmss
                return now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);

            // 12자리: yyMMddHHmmss
            return now.ToString("yyMMddHHmmss", CultureInfo.InvariantCulture);
        }

        // =====================================================================
        // SECS 수신 폴링
        // =====================================================================
        public void StartSecsPolling(int intervalMs = 5)
        {
            EnsureCreated();

            if (_secsPollThread != null)
                return;

            _secsPollRun = true;
            _secsPollThread = new Thread(() => SecsPollLoop(intervalMs))
            {
                IsBackground = true,
                Name = "XLinkGEM.SeqsPoll"
            };
            _secsPollThread.Start();
        }

        public void StopSecsPolling()
        {
            _secsPollRun = false;

            try
            {
                if (_secsPollThread != null && !_secsPollThread.Join(1000))
                    _secsPollThread.Interrupt();
            }
            catch { }
            finally
            {
                _secsPollThread = null;
            }
        }

        private void SecsPollLoop(int intervalMs)
        {
            while (_secsPollRun)
            {
                try
                {
                    short devId = 0, stream = 0, function = 0, wbit = 0;
                    int sysByte = 0;

                    bool has;
                    lock (_sync)
                    {
                        if (_gem == null) return;
                        has = _gem.LoadSecsMsg(ref devId, ref stream, ref function, ref wbit, ref sysByte);
                    }

                    if (has)
                    {
                        SecsMessageReceived?.Invoke(this, new GemSecsMessageReceivedEventArgs(
                            devId, stream, function, wbit, sysByte));

                        lock (_sync)
                        {
                            try { _gem.CloseSecsMsg(); } catch { }
                        }
                    }
                }
                catch { }

                if (intervalMs > 0)
                    Thread.Sleep(intervalMs);
            }
        }

        // =====================================================================
        // ECID 변경 요청 처리 (Accept 후 런타임 세션 파라미터 재적용)
        // =====================================================================
        // =====================================================================
        // ECID 변경 요청 처리 (Accept 후 런타임 세션 파라미터 재적용 + 디버깅)
        // =====================================================================
        private void HandleNewEquipmentConst(int lMsgId, short count)
        {
            var list = new List<GemEquipmentConstantChange>();

            lock (_sync)
            {
                for (short i = 0; i < count; i++)
                {
                    int ecid;
                    string value;
                    try
                    {
                        ecid = _gem.GetTargetEquipmentConstantId(i);
                        value = _gem.GetNewEquipmentConstantValue(i);
                    }
                    catch (Exception ex)
                    {
                        Diag($"ECID read FAIL idx={i}: {ex.GetType().Name}: {ex.Message}");
                        continue;
                    }

                    if (ecid <= 0)
                        continue;

                    list.Add(new GemEquipmentConstantChange(ecid, value ?? ""));
                }

                if (list.Count == 0)
                {
                    Diag($"NewEquipmentConstant: msgId={lMsgId}, count={count} -> empty");
                    return;
                }

                Diag($"NewEquipmentConstant: msgId={lMsgId}, items={list.Count}");
                foreach (var ch in list)
                    Diag($"  ECID={ch.Ecid}, VALUE='{ch.Value}'");

                bool acceptOk = false;
                try
                {
                    acceptOk = _gem.AcceptNewEquipmentConst(lMsgId);
                    Diag($"AcceptNewEquipmentConst: {(acceptOk ? "OK" : "FAIL(false)")}, msgId={lMsgId}");
                }
                catch (Exception ex)
                {
                    Diag($"AcceptNewEquipmentConst FAIL: {ex.GetType().Name}: {ex.Message}");
                }

                if (acceptOk)
                {
                    foreach (var ch in list)
                    {
                        // 1) ECID 값 반영
                        try { _gem.SetEquipmentConstantValue(ch.Ecid, ch.Value); }
                        catch (Exception ex) { Diag($"SetEquipmentConstantValue FAIL ecid={ch.Ecid}: {ex.Message}"); }

                        // 2) MFC처럼 런타임 세션 파라미터 즉시 반영
                        if (ch.Ecid == Define.ECID.ECID_T3)
                        {
                            if (short.TryParse(ch.Value, out var v)) { try { _gem.SetT3Timeout(v); } catch (Exception ex) { Diag($"SetT3Timeout FAIL: {ex.Message}"); } }
                        }
                        else if (ch.Ecid == Define.ECID.ECID_T5)
                        {
                            if (short.TryParse(ch.Value, out var v)) { try { _gem.SetT5Timeout(v); } catch (Exception ex) { Diag($"SetT5Timeout FAIL: {ex.Message}"); } }
                        }
                        else if (ch.Ecid == Define.ECID.ECID_T6)
                        {
                            if (short.TryParse(ch.Value, out var v)) { try { _gem.SetT6Timeout(v); } catch (Exception ex) { Diag($"SetT6Timeout FAIL: {ex.Message}"); } }
                        }
                        else if (ch.Ecid == Define.ECID.ECID_T7)
                        {
                            if (short.TryParse(ch.Value, out var v)) { try { _gem.SetT7Timeout(v); } catch (Exception ex) { Diag($"SetT7Timeout FAIL: {ex.Message}"); } }
                        }
                        else if (ch.Ecid == Define.ECID.ECID_T8)
                        {
                            if (short.TryParse(ch.Value, out var v)) { try { _gem.SetT8Timeout(v); } catch (Exception ex) { Diag($"SetT8Timeout FAIL: {ex.Message}"); } }
                        }
                        else if (ch.Ecid == Define.ECID.ECID_LINKTEST)
                        {
                            if (short.TryParse(ch.Value, out var v)) { try { _gem.SetLinkTestInterval(v); } catch (Exception ex) { Diag($"SetLinkTestInterval FAIL: {ex.Message}"); } }
                        }
                        else if (ch.Ecid == Define.ECID.ECID_RETRY)
                        {
                            if (short.TryParse(ch.Value, out var v)) TryInvokeComMethodUnsafe("SetRetry", v);
                        }
                        else if (ch.Ecid == Define.ECID.ECID_ESTABLISH_TIMEOUT)
                        {
                            if (short.TryParse(ch.Value, out var v)) { try { _gem.SetEstablishTimeout(v); } catch (Exception ex) { Diag($"SetEstablishTimeout FAIL: {ex.Message}"); } }
                        }
                        else if (ch.Ecid == Define.ECID.ECID_CONVERSATION_TIMEOUT)
                        {
                            if (short.TryParse(ch.Value, out var v))
                            {
                                TryInvokeComMethodUnsafe("SetConversationTimeout", v);
                                TryInvokeComMethodUnsafe("SetCTTimeout", v);
                                TryInvokeComMethodUnsafe("SetEstablishCommRetryTimer", v);
                            }
                        }
                        else if (ch.Ecid == Define.ECID.ECID_TIME_FORMAT)
                        {
                            if (int.TryParse(ch.Value, out var mode))
                                ApplyTimeFormatUnsafe(mode);
                        }
                        else if (ch.Ecid == Define.ECID.ECID_SOFTREV)
                        {
                            try { _gem.SOFTREV = ch.Value ?? ""; } catch (Exception ex) { Diag($"Set SOFTREV FAIL: {ex.Message}"); }
                        }
                    }

                    // MFC: SVID_ECID / SVID_ECVAL 설정 후 CEID_ECID_CHANGE 보고
                    var last = list[list.Count - 1];
                    try
                    {
                        _gem.SetVariableValue(Define.SVID.SVID_ECID, last.Ecid.ToString(CultureInfo.InvariantCulture));
                        _gem.SetVariableValue(Define.SVID.SVID_ECVAL, last.Value ?? "");
                        var ok = _gem.EventReport(Define.CEID.CEID_ECID_CHANGE);
                        Diag($"EventReport(CEID_ECID_CHANGE={Define.CEID.CEID_ECID_CHANGE}) => {ok}");
                    }
                    catch (Exception ex)
                    {
                        Diag($"ECID_CHANGE report FAIL: {ex.GetType().Name}: {ex.Message}");
                    }
                }
                else
                {
                    try
                    {
                        _gem.RejectNewEquipmentConst(lMsgId, 1);
                        Diag($"RejectNewEquipmentConst sent: msgId={lMsgId}");
                    }
                    catch (Exception ex)
                    {
                        Diag($"RejectNewEquipmentConst FAIL: {ex.GetType().Name}: {ex.Message}");
                    }
                }
            }

            if (list.Count > 0)
                EquipmentConstantsChanged?.Invoke(this, new GemEquipmentConstantsChangedEventArgs(lMsgId, list.AsReadOnly()));
        }



        // ===== 이벤트 연결 =====
        private void HookEvents()
        {
            _gem.ControlStateChange += OnControlStateChange;
            _gem.AlarmEvent += OnAlarmEvent;
            _gem.ErrorEvent += OnErrorEvent;
            _gem.RemoteCommand += OnRemoteCommand;
            _gem.CommunicationState += OnCommunicationState;
            _gem.ReceiveSecsMsg += OnReceiveSecsMsg;
            _gem.TerminalDisplaySingle += OnTerminalDisplaySingle;
            _gem.TerminalDisplayMulti += OnTerminalDisplayMulti;

            _gem.NewEquipmentConstant += OnNewEquipConst;
        }

        private void UnhookEvents()
        {
            if (_gem == null) return;

            try { _gem.ControlStateChange -= OnControlStateChange; } catch { }
            try { _gem.AlarmEvent -= OnAlarmEvent; } catch { }
            try { _gem.ErrorEvent -= OnErrorEvent; } catch { }
            try { _gem.RemoteCommand -= OnRemoteCommand; } catch { }
            try { _gem.CommunicationState -= OnCommunicationState; } catch { }
            try { _gem.ReceiveSecsMsg -= OnReceiveSecsMsg; } catch { }
            try { _gem.TerminalDisplaySingle -= OnTerminalDisplaySingle; } catch { }
            try { _gem.TerminalDisplayMulti -= OnTerminalDisplayMulti; } catch { }

            try { _gem.NewEquipmentConstant -= OnNewEquipConst; } catch { }
        }

        private void OnControlStateChange(short sControlState)
            => ControlStateChanged?.Invoke(this, new GemControlStateChangedEventArgs(sControlState));

        private void OnAlarmEvent(short sStatus, int sALID, string szALTX)
            => AlarmEventReceived?.Invoke(this, new GemAlarmEventArgs(sALID, sStatus));

        private void OnErrorEvent(short sErrorCode, string szErrMsg)
            => ErrorEventReceived?.Invoke(this, new GemErrorEventArgs(sErrorCode, szErrMsg));

        private void OnRemoteCommand(int lMsgID, short sCommandId, string szRCMD, short sParamCount)
        {
            if (DiagnosticsEnabled)
            {
                Diag($"RemoteCommand IN: msgId={lMsgID}, cmdId={sCommandId}, cmd='{szRCMD}', paramCount={sParamCount}");

                try
                {
                    var ps = ReadRemoteCommandParams(maxParams: (short)Math.Max((short)32, sParamCount));
                    foreach (var p in ps)
                        Diag($"  RCMD Param[{p.Index}] {p.Name}='{p.Value}'");
                }
                catch (Exception ex)
                {
                    Diag($"ReadRemoteCommandParams FAIL: {ex.GetType().Name}: {ex.Message}");
                }
            }

            RemoteCommandReceived?.Invoke(this, new GemRemoteCommandEventArgs(lMsgID, sCommandId, szRCMD, sParamCount));
        }

        private void OnCommunicationState(short nCode, string szMsg)
            => CommunicationStateChanged?.Invoke(this, new GemCommunicationStateEventArgs(nCode, szMsg));

        private void OnReceiveSecsMsg()
            => ReceiveSecsMsg?.Invoke(this, new GemReceiveSecsMsgEventArgs());

        private void OnTerminalDisplaySingle(int lMsgID, string szTerminalMsg)
            => TerminalMessageReceived?.Invoke(this, new GemTerminalMessageEventArgs(lMsgID, new[] { szTerminalMsg ?? "" }));

        private void OnTerminalDisplayMulti(int lMsgID, short sCount)
        {
            var lines = new List<string>(Math.Max((short)0, sCount));

            lock (_sync)
            {
                for (short i = 0; i < sCount; i++)
                    lines.Add(_gem.GetTerminalDisplayMessageMulti(i) ?? "");
            }

            TerminalMessageReceived?.Invoke(this, new GemTerminalMessageEventArgs(lMsgID, lines.ToArray()));
        }

        private void OnNewEquipConst(int lMsgID, short nCount)
        {
            try
            {
                HandleNewEquipmentConst(lMsgID, nCount);
            }
            catch (Exception ex)
            {
                ErrorEventReceived?.Invoke(this, new GemErrorEventArgs(-1, "HandleNewEquipmentConst: " + ex.Message));
                try
                {
                    lock (_sync) { _gem.RejectNewEquipmentConst(lMsgID, 1); }
                }
                catch { }
            }
        }

        private void EnsureCreated()
        {
            ThrowIfDisposed();
            if (_gem == null)
                throw new InvalidOperationException("GEM이 생성되지 않았습니다. Create() 먼저 호출하세요.");
        }

        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(XLinkGemService));
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            try { Stop(); } catch { }
            try { UnhookEvents(); } catch { }

            if (_gem != null)
            {
                try { Marshal.FinalReleaseComObject(_gem); } catch { }
                _gem = null;
            }
        }

        // =====================================================================
        // Debug / Diagnostics
        // =====================================================================
        public bool DiagnosticsEnabled { get; set; } = false;

        private void Diag(string message)
        {
            if (!DiagnosticsEnabled) return;
            try { Trace.WriteLine("[GEM] " + message); } catch { }
        }

        // =====================================================================
        // 동작 호환: COM 메서드가 인터롭에 없을 수도 있어 late-binding으로 안전 호출
        // =====================================================================
        private void TryInvokeComMethodUnsafe(string methodName, params object[] args)
        {
            try
            {
                if (_gem == null) 
                    return;

                _gem.GetType().InvokeMember(
                    methodName,
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod,
                    binder: null,
                    target: _gem,
                    args: args);

                Diag($"COM Invoke OK: {methodName}({string.Join(", ", args ?? Array.Empty<object>())})");
            }
            catch (Exception ex)
            {
                // interop에 없거나 COM에서 지원 안 하면 무시 (best-effort)
                Diag($"COM Invoke FAIL: {methodName} - {ex.GetType().Name}: {ex.Message}");
            }
        }

        private void ApplyMfcDisableAutoReplyUnsafe()
        {
            // MFC: DisableAutoReply(1,1), (2,99), (7,17), (7,19), (1,15)
            // XLinkGEM interop에 노출되지 않을 수 있으므로 late-binding 사용
            TryInvokeComMethodUnsafe("DisableAutoReply", 1, 1);
            TryInvokeComMethodUnsafe("DisableAutoReply", 2, 99);
            TryInvokeComMethodUnsafe("DisableAutoReply", 7, 17);
            TryInvokeComMethodUnsafe("DisableAutoReply", 7, 19);
            TryInvokeComMethodUnsafe("DisableAutoReply", 1, 15);
        }

        private void EnableCommunicationUnsafe()
        {
            // Start() 후 EnableCommunication()
            TryInvokeComMethodUnsafe("EnableCommunication");
        }




    }




    public readonly struct GemRcmdParam
    {
        public short Index { get; }
        public string Name { get; }
        public string Value { get; }

        public GemRcmdParam(short index, string name, string value)
        {
            Index = index;
            Name = name ?? "";
            Value = value ?? "";
        }
    }

    public sealed class GemControlStateChangedEventArgs : EventArgs
    {
        public short ControlState { get; }
        public GemControlStateChangedEventArgs(short controlState) => ControlState = controlState;
    }

    public sealed class GemAlarmEventArgs : EventArgs
    {
        public int ALID { get; }
        public short ALCD { get; }
        public GemAlarmEventArgs(int alid, short alcd) { ALID = alid; ALCD = alcd; }
    }

    public sealed class GemErrorEventArgs : EventArgs
    {
        public int ErrorCode { get; }
        public string ErrorText { get; }
        public GemErrorEventArgs(int errorCode, string errorText)
        {
            ErrorCode = errorCode;
            ErrorText = errorText ?? "";
        }
    }

    public sealed class GemRemoteCommandEventArgs : EventArgs
    {
        public int MsgId { get; }
        public short CommandId { get; }
        public string Command { get; }
        public short ParamCount { get; }

        public GemRemoteCommandEventArgs(int msgId, short commandId, string command, short paramCount)
        {
            MsgId = msgId;
            CommandId = commandId;
            Command = command ?? "";
            ParamCount = paramCount;
        }
    }

    public sealed class GemCommunicationStateEventArgs : EventArgs
    {
        public short Code { get; }
        public string Message { get; }

        public GemCommunicationStateEventArgs(short code, string message)
        {
            Code = code;
            Message = message ?? "";
        }
    }

    public sealed class GemTerminalMessageEventArgs : EventArgs
    {
        public int MsgId { get; }
        public string[] Lines { get; }
        public GemTerminalMessageEventArgs(int msgId, string[] lines)
        {
            MsgId = msgId;
            Lines = lines ?? Array.Empty<string>();
        }
    }

    public sealed class GemReceiveSecsMsgEventArgs : EventArgs
    {
    }

    public readonly struct GemEquipmentConstantChange
    {
        public int Ecid { get; }
        public string Value { get; }

        public GemEquipmentConstantChange(int ecid, string value)
        {
            Ecid = ecid;
            Value = value ?? "";
        }
    }

    public sealed class GemEquipmentConstantsChangedEventArgs : EventArgs
    {
        public int MsgId { get; }
        public IReadOnlyList<GemEquipmentConstantChange> Changes { get; }

        public GemEquipmentConstantsChangedEventArgs(int msgId, IReadOnlyList<GemEquipmentConstantChange> changes)
        {
            MsgId = msgId;
            Changes = changes ?? Array.Empty<GemEquipmentConstantChange>();
        }
    }

    public sealed class GemSecsMessageReceivedEventArgs : EventArgs
    {
        public short DevId { get; }
        public short Stream { get; }
        public short Function { get; }
        public short WBit { get; }
        public int SysByte { get; }

        public GemSecsMessageReceivedEventArgs(short devId, short stream, short function, short wbit, int sysByte)
        {
            DevId = devId;
            Stream = stream;
            Function = function;
            WBit = wbit;
            SysByte = sysByte;
        }
    }
}