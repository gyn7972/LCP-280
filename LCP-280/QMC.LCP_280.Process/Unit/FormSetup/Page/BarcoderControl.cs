using QMC.Common;
using QMC.Common.BarcodeReader;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Unit.FormSetup
{
    public partial class BarcoderControl : UserControl
    {
        // 이벤트 인자 클래스들
        public class BarcoderSelectedEventArgs : EventArgs
        {
            public OpticonBarcodeReader SelectedBarcoder { get; set; }
            public string DeviceName { get; set; }
        }

        public class BarcoderSaveEventArgs : EventArgs
        {
            public OpticonBarcodeReader Barcoder { get; set; }
            public QMC.Common.PropertyCollection Properties { get; set; }
        }

        public class BarcoderScanEventArgs : EventArgs
        {
            public OpticonBarcodeReader Barcoder { get; set; }
            public bool IsAutoTriggerMode { get; set; }
        }

        #region Events
        // 바코드 리더 선택 이벤트
        public event EventHandler<BarcoderSelectedEventArgs> BarcoderSelected;

        // 저장 버튼 클릭 이벤트
        public event EventHandler<BarcoderSaveEventArgs> SaveRequested;

        // 스캔 버튼 클릭 이벤트
        public event EventHandler<BarcoderScanEventArgs> ScanRequested;

        // 바코드 데이터 수신 이벤트 (UI에서 부모로 전달)
        public event EventHandler<BarcodeDataEventArgs> BarcodeDataReceived;

        // 에러 발생 이벤트
        public event EventHandler<string> ErrorOccurred;
        #endregion

        #region Barcder
        private ConfigReflectionMapper ConfigMapper_Barcder;
        private OpticonBarcodeReader SelectedItem_Barcder;
        private List<string> DeviceNames_Barcder;
        #endregion

        private bool isAutoTriggerMode = false;

        public BarcoderControl()
        {
            InitializeComponent();
            InitializeUI();
        }

        // ===== Initialize =====
        private void InitializeUI()
        {
            try
            {
                BindingList_Barcder();
                WriteEvents_Barcder();
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

            // 부모 폼으로 이벤트 발생
            BarcoderSelected?.Invoke(this, new BarcoderSelectedEventArgs
            {
                SelectedBarcoder = SelectedItem_Barcder,
                DeviceName = name
            });
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
                var pc = propertyCollectionView?.GetCurrentProperties();
                if (pc != null && SelectedItem_Barcder != null)
                {
                    // 부모 폼으로 이벤트 발생
                    SaveRequested?.Invoke(this, new BarcoderSaveEventArgs
                    {
                        Barcoder = SelectedItem_Barcder,
                        Properties = pc
                    });
                }
            }
            catch (Exception ex)
            {
                Log.Write("Setup", $"Save button click error: {ex}");
                ErrorOccurred?.Invoke(this, $"저장 버튼 오류: {ex.Message}");
            }
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

                // 부모 폼으로 이벤트 전달
                BarcodeDataReceived?.Invoke(this, e);
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

            // 부모 폼으로 이벤트 전달
            ErrorOccurred?.Invoke(this, error);
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

                // 부모 폼으로 이벤트 발생
                ScanRequested?.Invoke(this, new BarcoderScanEventArgs
                {
                    Barcoder = SelectedItem_Barcder,
                    IsAutoTriggerMode = isAutoTriggerMode
                });
            }
            catch (Exception ex)
            {
                Log.Write("Barcoder", $"btnBarcoderScan_Click error: {ex}");
                MessageBox.Show($"스캔 오류: {ex.Message}");
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

        // UI 업데이트 메서드 (부모 폼에서 호출 가능)
        public void UpdateScanStatus(string message, Color color)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateScanStatus(message, color)));
                return;
            }

            labelBarcoderData.Text = message;
            labelBarcoderData.ForeColor = color;
        }

        public void UpdateScanButtonText(string text)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateScanButtonText(text)));
                return;
            }

            btnBarcoderScan.Text = text;
        }

        #endregion
    }
}