using QMC.Common;
using QMC.Common.BarcodeReader;
using QMC.Common.Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Unit.FormSetup
{
    [FormOrder(6)]
    public partial class Barcoder_Setup : Form
    {
        private readonly Equipment Equipment = Equipment.Instance;

        #region Barcder
        private ConfigReflectionMapper ConfigMapper_Barcder;

        private OpticonBarcodeReader SelectedItem_Barcder;
        private List<string> DeviceNames_Barcder;
        #endregion

        private bool isAutoTriggerMode = false;

        public Barcoder_Setup()
        {
            InitializeComponent();
            SuspendLayout();

            InitializeUI();
        }

        // ===== Initialize =====
        private void InitializeUI()
        {
            try
            {
                BindingList_Barcder(); // 추가
                WriteEvents_Barcder(); // 추가
            }
            catch (Exception ex)
            {
                Log.Write("LCP-280", $"InitializeUI error: {ex}");
            }
        }

        #region Barcoder
        private void BindingList_Barcder()
        {
            try
            {
                DeviceNames_Barcder = new List<string>();

                // Equipment에서 정보들을 가져옴
                foreach (var Key in Equipment.Instance.Barcoders.Keys)
                {
                    DeviceNames_Barcder.Add(Key);
                }

                if (DeviceNames_Barcder.Count == 0)
                {
                    // 없으면 빈 상태로
                    listBoxItemsView?.SetItems();
                    return;
                }

                listBoxItemsView?.SetItems(DeviceNames_Barcder.ToArray());

                // 첫 번째 자동 선택
                if (DeviceNames_Barcder.Count > 0)
                {
                    listBoxItemsView.SelectedIndex = 0;

                    OnSelected_Barcder(this, 0);
                }
            }
            catch (Exception ex)
            {
                Log.Write("Barcoder", $"Binding Barcoder List error: {ex}");
            }
        }

        #endregion

        #region Barcoder Event
        private void WriteEvents_Barcder()
        {
            // 바코드 리스트 선택 이벤트
            if (listBoxItemsView != null)
            {
                listBoxItemsView.ItemSelected -= OnSelected_Barcder;
                listBoxItemsView.ItemSelected += OnSelected_Barcder;
            }

            if (btn_Save_Barcoder_Setup != null)
            {
                btn_Save_Barcoder_Setup.Click -= btn_Save_Barcoder_Setup_Click;
                btn_Save_Barcoder_Setup.Click += btn_Save_Barcoder_Setup_Click;
            }

            // 스캔 버튼 이벤트 추가
            if (btnBarcoderScan != null)
            {
                btnBarcoderScan.Click -= btnBarcoderScan_Click;
                btnBarcoderScan.Click += btnBarcoderScan_Click;
            }
        }

        private void OnSelected_Barcder(object sender, int selectedIndex)
        {
            // 기존 바코드 리더 이벤트 해제
            if (SelectedItem_Barcder != null)
            {
                UnsubscribeBarcoderEvents(SelectedItem_Barcder);
            }

            if (selectedIndex < 0 || selectedIndex >= DeviceNames_Barcder.Count) return;

            var name = DeviceNames_Barcder[selectedIndex];
            SelectedItem_Barcder = Equipment.Instance.Barcoders[name];

            // 새로운 바코드 리더 이벤트 연결
            if (SelectedItem_Barcder != null)
            {
                SubscribeBarcoderEvents(SelectedItem_Barcder);
            }

            // Config 객체를 직접 전달
            LoadProperties_Barcoder(SelectedItem_Barcder?.Config, SelectedItem_Barcder?.Name);

            // 스캔 버튼 상태 업데이트
            UpdateScanButtonState();
        }

        // ===== Properties Loading =====
        private void LoadProperties_Barcoder(object config, string deviceName)
        {
            try
            {
                // 기존 매핑 해제
                propertyCollectionView?.SetProperties(null);
                ConfigMapper_Barcder = null;

                if (config != null)
                {
                    Application.DoEvents();

                    ConfigMapper_Barcder = new ConfigReflectionMapper(config);
                    propertyCollectionView?.SetProperties(ConfigMapper_Barcder.PropertyCollection);
                    propertyCollectionView?.Refresh();

                    Log.Write("Barcoder", $"Device '{deviceName}' properties loaded successfully");
                }
            }
            catch (Exception ex)
            {
                Log.Write("Barcoder", $"LoadDeviceProperties error: {ex}");
                propertyCollectionView?.SetProperties(null);
                ConfigMapper_Barcder = null;
            }
        }

        private void btn_Save_Barcoder_Setup_Click(object sender, EventArgs e)
        {
            try
            {
                // 기존 저장 로직...
                var pc = propertyCollectionView?.GetCurrentProperties();
                if (pc != null)
                {
                    ConfigMapper_Barcder.ApplyToObject(pc);
                }

                var saveResult = SelectedItem_Barcder.Config.Save();
                string result = saveResult == 0 ? "완료" : "에러";
                MessageBox.Show($"저장 결과: {result}");

                // 설정 변경 후 재연결 추가
                if (saveResult == 0 && SelectedItem_Barcder.IsConnected)
                {
                    SelectedItem_Barcder.Close();
                    System.Threading.Thread.Sleep(100);
                    SelectedItem_Barcder.Initialize();
                }
            }
            catch (Exception ex)
            {
                Log.Write("Setup", $"Save error: {ex}");
            }
        }

        private string LogConfigProperties(string phase, object config)
        {
            var props = config.GetType().GetProperties().Take(5);
            var values = string.Join(", ", props.Select(p => $"{p.Name}={p.GetValue(config)}"));
            Log.Write("Setup", $"=== {phase} === {values}");
            return values;
        }

        private string LogPropertyCollection(string phase, object pc)
        {
            if (pc == null) return "null";

            var pcType = pc.GetType();
            var countProp = pcType.GetProperty("Count");
            var count = countProp?.GetValue(pc) ?? 0;

            Log.Write("Setup", $"=== {phase} === Count: {count}");
            return count.ToString();
        }
        #endregion

        #region NLV-5201 지원 메서드들

        /// <summary>
        /// 바코드 리더 이벤트 구독
        /// </summary>
        private void SubscribeBarcoderEvents(OpticonBarcodeReader barcoder)
        {
            if (barcoder != null)
            {
                barcoder.BarcodeDataReceived += Barcoder_BarcodeDataReceived;
                barcoder.ErrorOccurred += Barcoder_ErrorOccurred;
                barcoder.StatusChanged += Barcoder_StatusChanged;
            }
        }

        /// <summary>
        /// 바코드 리더 이벤트 해제
        /// </summary>
        private void UnsubscribeBarcoderEvents(OpticonBarcodeReader barcoder)
        {
            if (barcoder != null)
            {
                barcoder.BarcodeDataReceived -= Barcoder_BarcodeDataReceived;
                barcoder.ErrorOccurred -= Barcoder_ErrorOccurred;
                barcoder.StatusChanged -= Barcoder_StatusChanged;
            }
        }

        /// <summary>
        /// 바코드 데이터 수신 이벤트
        /// </summary>
        private void Barcoder_BarcodeDataReceived(object sender, BarcodeDataEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => Barcoder_BarcodeDataReceived(sender, e)));
                return;
            }

            try
            {
                labelBarcoderData.Text = $"[{e.Timestamp:HH:mm:ss}] {e.Data}";
                labelBarcoderData.ForeColor = System.Drawing.Color.Blue;
                Log.Write("Barcoder", $"바코드 수신: {e.Data}");
            }
            catch (Exception ex)
            {
                Log.Write("Barcoder", $"BarcodeDataReceived 처리 오류: {ex}");
            }
        }

        /// <summary>
        /// 오류 발생 이벤트
        /// </summary>
        private void Barcoder_ErrorOccurred(object sender, string error)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => Barcoder_ErrorOccurred(sender, error)));
                return;
            }

            labelBarcoderData.Text = $"오류: {error}";
            labelBarcoderData.ForeColor = System.Drawing.Color.Red;
            Log.Write("Barcoder", $"오류: {error}");
        }

        /// <summary>
        /// 상태 변경 이벤트
        /// </summary>
        private void Barcoder_StatusChanged(object sender, string status)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => Barcoder_StatusChanged(sender, status)));
                return;
            }

            Log.Write("Barcoder", $"상태: {status}");
        }

        /// <summary>
        /// 스캔 버튼 클릭 이벤트
        /// </summary>
        private void btnBarcoderScan_Click(object sender, EventArgs e)
        {
            try
            {
                if (SelectedItem_Barcder == null)
                {
                    MessageBox.Show("바코드 리더를 선택하세요.");
                    return;
                }

                if (!SelectedItem_Barcder.IsConnected)
                {
                    if (SelectedItem_Barcder.Initialize() != 0)
                    {
                        MessageBox.Show("바코드 리더 연결에 실패했습니다.");
                        return;
                    }
                }

                if (!isAutoTriggerMode)
                {
                    PerformManualScan();
                }
                else
                {
                    ToggleAutoTrigger();
                }
            }
            catch (Exception ex)
            {
                Log.Write("Barcoder", $"btnBarcoderScan_Click error: {ex}");
                MessageBox.Show($"스캔 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// 수동 스캔 수행
        /// </summary>
        private void PerformManualScan()
        {
            try
            {
                labelBarcoderData.Text = "스캔 중...";
                labelBarcoderData.ForeColor = System.Drawing.Color.Orange;

                string data;
                int result = SelectedItem_Barcder.Read(out data);

                if (result == 0 && !string.IsNullOrWhiteSpace(data))
                {
                    labelBarcoderData.Text = $"[{DateTime.Now:HH:mm:ss}] {data}";
                    labelBarcoderData.ForeColor = System.Drawing.Color.Blue;
                }
                else
                {
                    labelBarcoderData.Text = "바코드를 읽지 못했습니다.";
                    labelBarcoderData.ForeColor = System.Drawing.Color.Red;
                }
            }
            catch (Exception ex)
            {
                labelBarcoderData.Text = $"스캔 오류: {ex.Message}";
                labelBarcoderData.ForeColor = System.Drawing.Color.Red;
            }
        }

        /// <summary>
        /// 자동 트리거 모드 토글
        /// </summary>
        private void ToggleAutoTrigger()
        {
            try
            {
                if (SelectedItem_Barcder.IsAutoTriggerEnabled)
                {
                    SelectedItem_Barcder.StopAutoTrigger();
                    btnBarcoderScan.Text = "Start Auto";
                    labelBarcoderData.Text = "자동 트리거 모드 중지됨";
                    labelBarcoderData.ForeColor = System.Drawing.Color.Gray;
                }
                else
                {
                    if (SelectedItem_Barcder.StartAutoTrigger() == 0)
                    {
                        btnBarcoderScan.Text = "Stop Auto";
                        labelBarcoderData.Text = "자동 트리거 모드 시작됨 - 바코드를 스캐너에 가져다 대세요";
                        labelBarcoderData.ForeColor = System.Drawing.Color.Green;
                    }
                    else
                    {
                        labelBarcoderData.Text = "자동 트리거 모드 시작 실패";
                        labelBarcoderData.ForeColor = System.Drawing.Color.Red;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write("Barcoder", $"ToggleAutoTrigger error: {ex}");
            }
        }

        /// <summary>
        /// 스캔 버튼 상태 업데이트
        /// </summary>
        private void UpdateScanButtonState()
        {
            if (SelectedItem_Barcder == null)
            {
                btnBarcoderScan.Text = "Manual Scan";
                btnBarcoderScan.Enabled = false;
                labelBarcoderData.Text = "바코드 리더를 선택하세요.";
                labelBarcoderData.ForeColor = System.Drawing.Color.Gray;
                return;
            }

            btnBarcoderScan.Enabled = true;

            if (isAutoTriggerMode)
            {
                btnBarcoderScan.Text = SelectedItem_Barcder.IsAutoTriggerEnabled ? "Stop Auto" : "Start Auto";
            }
            else
            {
                btnBarcoderScan.Text = "Manual Scan";
            }

            labelBarcoderData.Text = "바코드 데이터가 여기에 표시됩니다.";
            labelBarcoderData.ForeColor = System.Drawing.Color.Gray;
        }

        #endregion

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            try
            {
                if (SelectedItem_Barcder != null)
                {
                    SelectedItem_Barcder.StopAutoTrigger();
                    UnsubscribeBarcoderEvents(SelectedItem_Barcder);
                }
            }
            catch (Exception ex)
            {
                Log.Write("Barcoder", $"Form close error: {ex}");
            }

            base.OnFormClosed(e);
        }
    }
}
