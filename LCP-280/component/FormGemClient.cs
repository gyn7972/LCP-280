using System;
using System.Drawing;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Component
{
    public partial class FormGemClient : Form
    {
        private GemClient _gemClient;
        private GemMessageRouter _router;

        public FormGemClient()
        {
            InitializeComponent();
            InitializeGemClient();
        }

        private void InitializeGemClient()
        {
            // 1. 초기화 및 이벤트 등록
            _gemClient = new GemClient();
            _router = new GemMessageRouter();

            // 연결 상태 변경 이벤트
            _gemClient.OnConnectionChanged += (isConnected) =>
            {
                if (this.IsDisposed) return;
                this.Invoke((Action)(() => {
                    if (lblStatus != null)
                    {
                        lblStatus.Text = isConnected ? "GEM Connected" : "GEM Disconnected";
                        lblStatus.BackColor = isConnected ? Color.Lime : Color.Red;
                    }
                    AppendLog(isConnected ? "Connected to Server" : "Disconnected from Server");
                }));
            };

            // 라우터 이벤트 구독 (UI 갱신이나 로직 처리)
            _router.OnTrayIdResult += (result) => Console.WriteLine($"Tray ID Check: {result}");
            _router.OnStopOrAbort += () => Console.WriteLine("STOP Command Received!");

            // [수정 포인트] GemClient가 메시지를 받으면 -> 로그 출력 후 -> Router에게 전달
            _gemClient.OnMessageReceived += (msg) =>
            {
                // 1. 수신 로그 화면 표시
                AppendLog($"[RECV] {msg}");

                // 2. 라우터에 전달하여 분석 (분석 결과는 위에서 등록한 이벤트로 옴)
                _router.HandlePayload(msg);
            };
            // 내부 로그 이벤트
            _gemClient.OnLog += (msg) => AppendLog($"[LOG] {msg}");

            // 2. 통신 시작
            _gemClient.Start();
        }

        private void AppendLog(string msg)
        {
            if (this.IsDisposed) return;
            if (this.InvokeRequired)
            {
                this.Invoke((Action)(() => AppendLog(msg)));
                return;
            }

            // txtLog에 텍스트 추가 (최신 로그가 아래로)
            if (txtLog != null)
            {
                string logLine = $"[{DateTime.Now:HH:mm:ss}] {msg}\r\n";
                txtLog.AppendText(logLine);
                txtLog.ScrollToCaret(); // 자동 스크롤
            }
            else
            {
                Console.WriteLine(msg);
            }
        }

        // =============================================================
        // [연결 제어]
        // =============================================================
        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (_gemClient.IsConnected)
                _gemClient.Stop();
            else
                _gemClient.Start();
        }

        // =============================================================
        // [기능 1] Control / EQ State 변경
        // =============================================================
        private void btnControlState_Click(object sender, EventArgs e)
        {
            // 값: 1=Offline, 4=Local, 5=Remote (기본값 Remote)
            string stateStr = "5";

            // Socket Protocol: MAIN,ControlStateChange,StateCode
            _gemClient.Send($"MAIN,ControlStateChange,{stateStr}");
            AppendLog($"[CMD] Control State Change -> {stateStr}");
        }

        private void btnEqState_Click(object sender, EventArgs e)
        {
            // 값: 1=Run, 2=Idle, 3=Down (기본값 Run)
            string stateStr = "1";

            // Socket Protocol: MAIN,EqStateChange,StateCode
            _gemClient.Send($"MAIN,EqStateChange,{stateStr}");
            AppendLog($"[CMD] EQ State Change -> {stateStr}");
        }

        // =============================================================
        // [기능 2] PP Select (레시피 선택)
        // =============================================================
        private void btnPPSelect_Click(object sender, EventArgs e)
        {
            string recipeId = txtRecipe.Text; // UI 입력값 사용

            _gemClient.Send($"MAIN,PPSelect,{recipeId}");
            AppendLog($"[CMD] PP Select -> {recipeId}");
        }

        // =============================================================
        // [기능 3] Port / Wafer Load & Unload
        // =============================================================
        private void btnPortLoad_Click(object sender, EventArgs e)
        {
            string portId = txtPortId.Text;
            string lotId = txtLotId.Text;

            _gemClient.Send($"MAIN,PortLoad,{portId},{lotId}");
            AppendLog($"[CMD] Port Load -> {portId}, {lotId}");
        }

        private void btnPortUnload_Click(object sender, EventArgs e)
        {
            string portId = txtPortId.Text;
            string lotId = txtLotId.Text;

            _gemClient.Send($"MAIN,PortUnload,{portId},{lotId}");
            AppendLog($"[CMD] Port Unload -> {portId}");
        }

        private void btnWaferLoad_Click(object sender, EventArgs e)
        {
            // 예시로 Port Load와 동일한 포트 ID 사용
            string portId = txtPortId.Text;
            _gemClient.Send($"MAIN,WaferLoad,{portId}");
            AppendLog($"[CMD] Wafer Load -> {portId}");
        }

        // =============================================================
        // [기능 4] Process Start / End (Lot Processing)
        // =============================================================
        private void btnProcStart_Click(object sender, EventArgs e)
        {
            string lotId = txtLotId.Text;

            _gemClient.Send($"MAIN,LotProcessingStarted,{lotId}");
            AppendLog($"[CMD] Process Start -> {lotId}");
        }

        private void btnProcEnd_Click(object sender, EventArgs e)
        {
            string lotId = txtLotId.Text;
            string trayId = "TRAY_A123"; // 임시값

            // 수율 정보 (임시값)
            int total = 100;
            int good = 90;
            int ng = 10;

            string msg = $"MAIN,LotProcessingCompleted,lotId={lotId},trayId={trayId},total={total},good={good},ng={ng}";

            _gemClient.Send(msg);
            AppendLog($"[CMD] Process End -> {lotId} (T:{total}/G:{good}/N:{ng})");
        }

        // =============================================================
        // [기능 5] Alarm Set / Clear
        // =============================================================
        private void btnAlarmSet_Click(object sender, EventArgs e)
        {
            string alid = txtAlarmId.Text;
            string alText = txtAlarmText.Text;

            _gemClient.Send($"MAIN,AlarmSet,{alid},{alText}");
            AppendLog($"[CMD] Alarm Set -> {alid} ({alText})");
        }

        private void btnAlarmClear_Click(object sender, EventArgs e)
        {
            string alid = txtAlarmId.Text;
            string alText = txtAlarmText.Text;

            _gemClient.Send($"MAIN,AlarmClear,{alid},{alText}");
            AppendLog($"[CMD] Alarm Clear -> {alid}");
        }

        // =============================================================
        // [기능 6] 기타 보고 (Tray Report 등)
        // =============================================================
        private void btnReportTray_Click(object sender, EventArgs e)
        {
            string recipeId = txtRecipe.Text;
            string trayId = "TRAY_A123";

            string msg = $"MAIN,TrayIdReport,{trayId},{recipeId}";
            _gemClient.Send(msg);
            AppendLog($"[CMD] Tray Report -> {trayId}");
        }

        // =============================================================
        // [수신] GEM 프로그램 -> 메인 장비 응답 처리
        // =============================================================
        private void HandleGemMessage(string message)
        {
            if (this.IsDisposed)
            {
                return;
            }

            // UI 스레드 처리
            this.Invoke((Action)(() => {
                // [로그 출력] 수신된 메시지를 화면에 표시
                AppendLog($"[RECV] {message}");

                string[] args = message.Split(',');
                if (args.Length == 0) return;

                string command = args[0];

                switch (command)
                {
                    case "TrayIdReport":
                        // 예: TrayIdReport,Ng,Code,Text...
                        string result = args.Length > 1 ? args[1] : "";
                        AppendLog($" -> Tray Report Result: {result}");
                        break;

                    case "PPSelected":
                        // 레시피 선택 결과 처리
                        AppendLog(" -> Recipe Selection Acknowledged");
                        break;

                    case "RemoteCommand":
                        // 원격 명령 수신 (예: START, STOP)
                        string rcmd = args.Length > 1 ? args[1] : "";
                        AppendLog($" -> Remote Command: {rcmd}");
                        MessageBox.Show($"Remote Command Received: {rcmd}");
                        break;

                    case "ServerStatus":
                        AppendLog(" -> Server Status Update Received");
                        break;
                }
            }));
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _gemClient.Stop();
            base.OnFormClosing(e);
        }
    }
}