using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using QMC.Common;

namespace QMC.LCP_280.Process.Component
{
    public class GemClient
    {
        // 설정
        private const string ServerIp = "127.0.0.1";
        private const int ServerPort = 5000;

        // 프로토콜 정의 (Server와 동일해야 함)
        private const char STX = (char)0x02;
        private const char ETX = (char)0x03;

        private TcpClient _client;
        private NetworkStream _stream;
        private Thread _receiveThread;
        private bool _isRunning;

        // 수신 버퍼
        private StringBuilder _rxBuffer = new StringBuilder();
        private object _rxLock = new object();

        // 이벤트 (메시지 수신 시 외부로 알림)
        public event Action<bool> OnConnectionChanged;
        public event Action<string> OnMessageReceived;
        public event Action<string> OnLog;

        public bool IsConnected => _client != null && _client.Connected;

        public void Start()
        {
            if (_isRunning) return;
            _isRunning = true;

            _receiveThread = new Thread(ConnectionLoop) { IsBackground = true };
            _receiveThread.Start();
        }

        public void Stop()
        {
            _isRunning = false;
            Disconnect();
        }

        private void ConnectionLoop()
        {
            while (_isRunning)
            {
                try
                {
                    if (!IsConnected)
                    {
                        GemLog("Connecting to GEM...");
                        _client = new TcpClient();
                        _client.Connect(ServerIp, ServerPort);
                        _stream = _client.GetStream();

                        GemLog("Connected to GEM Server.");
                        OnConnectionChanged?.Invoke(true);

                        // 수신 루프
                        ReceiveLoop();
                    }
                }
                catch (Exception ex)
                {
                    // 접속 실패 시 잠시 대기 후 재시도
                    Thread.Sleep(1000);
                    Log.Write(ex);
                }
            }
        }

        private void ReceiveLoop()
        {
            byte[] buffer = new byte[4096];

            try
            {
                while (_isRunning && IsConnected)
                {
                    int bytesRead = _stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break; // 연결 끊김

                    string chunk = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    ProcessData(chunk);
                }
            }
            catch (Exception ex)
            {
                GemLog($"Receive Error: {ex.Message}");
                Log.Write(ex);
            }
            finally
            {
                Disconnect();
            }
        }

        private void Disconnect()
        {
            if (_client != null)
            {
                _client.Close();
                _client = null;
                _stream = null;
                OnConnectionChanged?.Invoke(false);
                GemLog("Disconnected from GEM.");
            }
        }

        // 데이터 조립 및 파싱 (SocketServer의 AppendAndExtractFrames와 대칭)
        private void ProcessData(string chunk)
        {
            lock (_rxLock)
            {
                _rxBuffer.Append(chunk);
                string content = _rxBuffer.ToString();

                while (true)
                {
                    int stxIndex = content.IndexOf(STX);
                    if (stxIndex < 0)
                    {
                        // STX가 없는데 버퍼가 너무 크면 비움 (에러 상황)
                        if (content.Length > 8192) _rxBuffer.Clear();
                        break;
                    }

                    // STX 앞의 쓰레기 데이터 제거
                    if (stxIndex > 0)
                    {
                        _rxBuffer.Remove(0, stxIndex);
                        content = _rxBuffer.ToString();
                        stxIndex = 0;
                    }

                    int etxIndex = content.IndexOf(ETX);
                    if (etxIndex < 0) break; // 아직 데이터가 다 안 옴

                    // 메시지 추출 (STX, ETX 제외)
                    string packet = content.Substring(1, etxIndex - 1); // STX(0) 다음부터 ETX 전까지

                    // 버퍼에서 제거 (ETX 포함)
                    _rxBuffer.Remove(0, etxIndex + 1);
                    content = _rxBuffer.ToString();

                    // 쉼표 제거 및 이벤트 발생
                    string message = packet.Trim(',');
                    OnMessageReceived?.Invoke(message);
                }
            }
        }

        public void Send(string message)
        {
            if (!IsConnected)
            {
                GemLog("Cannot send. Not connected.");
                return;
            }

            try
            {
                // SocketServer가 기대하는 포맷: STX + , + Message + , + ETX
                string frame = $"{STX},{message},{ETX}";
                byte[] data = Encoding.UTF8.GetBytes(frame);

                _stream.Write(data, 0, data.Length);
                GemLog($"[SEND] {message}");
            }
            catch (Exception ex)
            {
                GemLog($"Send Error: {ex.Message}");
                Log.Write(ex);
                Disconnect();
            }
        }

        private void GemLog(string msg)
        {
            OnLog?.Invoke($"[GEM Client] {msg}");
        }
    }
}

