using System;
using System.Linq;

namespace QMC.LCP_280.Process.Component
{
    public sealed class GemMessageRouter : IGemMessageHandler
    {
        // 외부에서 결과를 구독할 수 있도록 이벤트 정의 (필요에 따라 사용)
        public event Action<string> OnServerStatusUpdated;
        public event Action<string> OnTrayIdResult;
        public event Action<string> OnPPSelectedResult;
        public event Action<string> OnPPUploadResult;
        public event Action<string> OnLotStartResult;
        public event Action<string> OnLotCompleteResult;
        public event Action OnStopOrAbort;

        public void HandlePayload(string payload)
        {
            if (string.IsNullOrWhiteSpace(payload)) 
                return;

            var tokens = payload.Split(',');
            if (tokens.Length <= 0)
                return;

            string cmd = tokens[0];

            // 1. Server Status Check
            if (cmd == "ServerStatus")
            {
                // 예: ServerStatus,Run or ServerStatus,Idle
                string status = tokens.Length > 1 ? tokens[1] : "Unknown";
                OnServerStatusUpdated?.Invoke(status);
                return;
            }

            // 2. Tray ID Report Result (ACK/NACK)
            if (cmd == "TrayIdReport")
            {
                // 예: TrayIdReport,OK or TrayIdReport,NG
                string result = tokens.Length > 1 ? tokens[1] : "";
                OnTrayIdResult?.Invoke(result);
                return;
            }

            // 3. PP Selected Result
            if (cmd == "PPSelected")
            {
                string result = tokens.Length > 1 ? tokens[1] : "";
                OnPPSelectedResult?.Invoke(result);
                return;
            }

            // 4. PP Upload Completed Result
            if (cmd == "PPUploadCompleted")
            {
                string result = tokens.Length > 1 ? tokens[1] : "";
                OnPPUploadResult?.Invoke(result);
                return;
            }

            // 5. Lot Processing Started Result
            if (cmd == "LotProcessingStarted")
            {
                string result = tokens.Length > 1 ? tokens[1] : "";
                OnLotStartResult?.Invoke(result);
                return;
            }

            // 6. Lot Processing Completed Result
            if (cmd == "LotProcessingCompleted")
            {
                string result = tokens.Length > 1 ? tokens[1] : "";
                OnLotCompleteResult?.Invoke(result);
                return;
            }

            // 7. Abort / Stop Command
            if (cmd == "Aborted" || cmd == "Stop")
            {
                OnStopOrAbort?.Invoke();
                return;
            }
        }
    }
}