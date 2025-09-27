using QMC.Common.Common;
using QMC.Common.Component;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QMC.Common.BarcodeReader
{
    /// <summary>
    /// 바코드 데이터 이벤트 인자
    /// </summary>
    public class BarcodeDataEventArgs : EventArgs
    {
        public string Data { get; set; }
        public DateTime Timestamp { get; set; }
        public string ScannerName { get; set; }
        public string CodeType { get; set; } = "Unknown";
    }

    public class OpticonBarcodeReader : BaseComponent
    {
        #region Field
        private SerialComm communicator;
        #endregion

        #region Property
        public new OpticonBarcodeReaderConfig Config { get; private set; }
        #endregion

        // 1. Field 추가 (기존 Field 영역에 추가)
        private bool isAutoTriggerEnabled = false;
        private Thread autoTriggerThread;
        private bool stopAutoTrigger = false;

        // 2. Event 추가 (기존 코드 앞에 추가)
        #region Event - NLV-5201 지원용 추가
        /// <summary>
        /// 바코드 데이터 수신 이벤트
        /// </summary>
        public event EventHandler<BarcodeDataEventArgs> BarcodeDataReceived;

        /// <summary>
        /// 오류 발생 이벤트  
        /// </summary>
        public event EventHandler<string> ErrorOccurred;

        /// <summary>
        /// 상태 변경 이벤트
        /// </summary>
        public event EventHandler<string> StatusChanged;

        /// <summary>
        /// 연결 상태 확인
        /// </summary>
        public bool IsConnected => communicator?.IsOpen ?? false;
        #endregion

        // 3. Property 추가 (기존 Property 영역에 추가)
        /// <summary>
        /// 자동 트리거 모드 상태
        /// </summary>
        public bool IsAutoTriggerEnabled => isAutoTriggerEnabled;

        #region Constructor
        public OpticonBarcodeReader(string name) : base(name)
        {
            Config = new OpticonBarcodeReaderConfig(name);
            communicator = new SerialComm();
            communicator.ETXString = "\r"; // NLV-5201용: CR만 사용 (기존 "\r\n"에서 변경)
            UpdateCommunicatorConfig();
        }
        #endregion

        #region Override Method
        public override int Initialize()
        {
            if (communicator.IsOpen)
            {
                communicator.Close();
            }

            UpdateCommunicatorConfig();
            if (!communicator.Open())
            {
                return -1;
            }

            return 0;
        }
        public override int Create()
        {
            // Do something if needed
            return 0;
        }
        public override void Close()
        {
            StopAutoTrigger(); // 추가
            communicator.Close();
        }
        #endregion

        #region Method
        #region NLV-5201 지원 메서드들

        /// <summary>
        /// 자동 트리거 모드 시작
        /// </summary>
        public int StartAutoTrigger()
        {
            try
            {
                if (!communicator.IsOpen)
                {
                    OnErrorOccurred("바코드 리더가 연결되지 않음");
                    return -1;
                }

                if (isAutoTriggerEnabled)
                {
                    OnStatusChanged("자동 트리거 모드가 이미 실행 중입니다");
                    return 0;
                }

                // 자동 트리거 활성화 명령 전송 (ESC + +I + CR)
                string enableAutoCmd = "\x1B+I\r";
                if (!communicator.Send(enableAutoCmd))
                {
                    OnErrorOccurred("자동 트리거 활성화 명령 전송 실패");
                    return -2;
                }

                StartAutoTriggerMonitoring();
                isAutoTriggerEnabled = true;
                OnStatusChanged("자동 트리거 모드 시작");
                return 0;
            }
            catch (Exception ex)
            {
                OnErrorOccurred($"자동 트리거 시작 오류: {ex.Message}");
                return -3;
            }
        }

        /// <summary>
        /// 자동 트리거 모드 중지
        /// </summary>
        public int StopAutoTrigger()
        {
            try
            {
                if (!isAutoTriggerEnabled) return 0;

                stopAutoTrigger = true;

                // 자동 트리거 비활성화 명령 전송 (ESC + +F + CR)
                if (communicator.IsOpen)
                {
                    string disableAutoCmd = "\x1B+F\r";
                    communicator.Send(disableAutoCmd);
                }

                // 모니터링 스레드 종료 대기
                if (autoTriggerThread != null && autoTriggerThread.IsAlive)
                {
                    autoTriggerThread.Join(1000);
                }

                isAutoTriggerEnabled = false;
                stopAutoTrigger = false;
                OnStatusChanged("자동 트리거 모드 중지");
                return 0;
            }
            catch (Exception ex)
            {
                OnErrorOccurred($"자동 트리거 중지 오류: {ex.Message}");
                return -1;
            }
        }

        /// <summary>
        /// 소프트웨어 버전 확인
        /// </summary>
        public bool GetVersion()
        {
            try
            {
                if (!communicator.IsOpen) return false;

                // ESC + Z1 + CR
                string command = "\x1BZ1\r";
                return communicator.Send(command);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 부저 설정
        /// </summary>
        public bool SetBuzzer(bool enable)
        {
            try
            {
                if (!communicator.IsOpen) return false;

                // ESC + W8 + CR (활성화) 또는 ESC + W0 + CR (비활성화)
                string command = enable ? "\x1BW8\r" : "\x1BW0\r";
                return communicator.Send(command);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 자동 트리거 모니터링 스레드
        /// </summary>
        private void StartAutoTriggerMonitoring()
        {
            stopAutoTrigger = false;
            autoTriggerThread = new Thread(() =>
            {
                while (!stopAutoTrigger && communicator.IsOpen)
                {
                    try
                    {
                        string data;
                        if (communicator.Recv(out data))
                        {
                            if (!string.IsNullOrWhiteSpace(data))
                            {
                                data = data.Trim();
                                if (IsBarcodeData(data))
                                {
                                    OnBarcodeDataReceived(new BarcodeDataEventArgs
                                    {
                                        Data = data,
                                        Timestamp = DateTime.Now,
                                        ScannerName = this.Name
                                    });
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (!stopAutoTrigger)
                        {
                            OnErrorOccurred($"자동 트리거 모니터링 오류: {ex.Message}");
                        }
                    }
                    Thread.Sleep(10);
                }
            })
            {
                IsBackground = true,
                Name = $"AutoTrigger-{this.Name}"
            };
            autoTriggerThread.Start();
        }

        /// <summary>
        /// 바코드 데이터 판단
        /// </summary>
        private bool IsBarcodeData(string data)
        {
            return !data.StartsWith("Version") &&
                   !data.StartsWith("ERROR") &&
                   !data.StartsWith("OK") &&
                   !data.Contains("Tuning") &&
                   data.Length > 0;
        }

        /// <summary>
        /// 이벤트 발생 메서드들
        /// </summary>
        protected virtual void OnBarcodeDataReceived(BarcodeDataEventArgs e)
        {
            BarcodeDataReceived?.Invoke(this, e);
        }

        protected virtual void OnErrorOccurred(string error)
        {
            ErrorOccurred?.Invoke(this, error);
        }

        protected virtual void OnStatusChanged(string status)
        {
            StatusChanged?.Invoke(this, status);
        }

        #endregion

        public int Read(out string data)
        {
            data = string.Empty;
            if (communicator == null || !communicator.IsOpen)
                return -1;

            string command = GetBarcodeReadStartCommandString();
            for (int i = 0; i < Config.RetryCount; i++)
            {
                if (communicator.Send(command) != true)
                    return -2;

                if (communicator.Recv(out data) == true)
                {
                    if (!string.IsNullOrWhiteSpace(data))
                    {
                        data = data.Trim();

                        // 이벤트 발생 추가
                        OnBarcodeDataReceived(new BarcodeDataEventArgs
                        {
                            Data = data,
                            Timestamp = DateTime.Now,
                            ScannerName = this.Name
                        });

                        break;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(data))
                return -2;

            return 0;
        }

        public int ReadAbort()
        {
            if (communicator == null || !communicator.IsOpen)
                return -1;

            string command = GetBarcodeReadStopCommandString();
            if (communicator.Send(command) != true)
                return -2;

            return 0;
        }
        private string GetBarcodeReadStartCommandString()
        {
            byte[] cmdBytes = { 0x1b, 0x5a, 0x0d }; // [ESC] [Z] [CR]
            return Encoding.ASCII.GetString(cmdBytes);
        }
        private string GetBarcodeReadStopCommandString()
        {
            byte[] cmdBytes = { 0x1b, 0x59, 0x0d }; // [ESC] [Y] [CR]
            return Encoding.ASCII.GetString(cmdBytes);
        }

        private void UpdateCommunicatorConfig()
        {
            if (communicator != null)
            {
                communicator.PortName = Config.PortName;
                communicator.BaudRate = Config.BaudRate;
                communicator.DataBits = Config.DataBits;
                communicator.Parity = Config.Parity;
                communicator.StopBits = Config.StopBits;
                communicator.Handshake = Config.Handshake;
                communicator.ConversationTimeout = Config.ConversationTimeout;
            }
        }

        public bool TryGetValue(string key, out object barcoder)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
