using QMC.Common;
using QMC.Common.BarcodeReader;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using static QMC.LCP_280.Process.Unit.FormSetup.Barcoder_Setup;

namespace QMC.LCP_280.Process.Unit.FormSetup
{
    public partial class BarcoderControl : UserControl
    {
        #region Barcoder Events (Control → Form)

        /// <summary>
        /// 바코드 리더 선택 이벤트
        /// </summary>
        public class BarcoderSelectedEventArgs : EventArgs
        {
            public OpticonBarcodeReader SelectedBarcoder { get; set; }
            public string DeviceName { get; set; }
        }

        /// <summary>
        /// 저장 요청 이벤트
        /// </summary>
        public class BarcoderSaveEventArgs : EventArgs
        {
            public OpticonBarcodeReader Barcoder { get; set; }
            public PropertyCollection Properties { get; set; }
        }

        /// <summary>
        /// 스캔 요청 이벤트
        /// </summary>
        public class BarcoderScanEventArgs : EventArgs
        {
            public OpticonBarcodeReader Barcoder { get; set; }
            public bool IsAutoTriggerMode { get; set; }
        }

        /// <summary>
        /// 리스트 초기화 요청 이벤트
        /// </summary>
        public class BarcoderListClearEventArgs : EventArgs
        {
            // 필요시 추가 정보
        }

        #endregion

        #region Events (Control → Form)

        public event EventHandler<BarcoderSelectedEventArgs> BarcoderSelected;
        public event EventHandler<BarcoderSaveEventArgs> SaveRequested;
        public event EventHandler<BarcoderScanEventArgs> ScanRequested;
        public event EventHandler<BarcoderListClearEventArgs> ListClearRequested;
        public event EventHandler<BarcodeDataEventArgs> BarcodeDataReceived;
        public event EventHandler<string> ErrorOccurred;

        #endregion

        #region Fields

        private ConfigReflectionMapper ConfigMapper_Barcder;
        private OpticonBarcodeReader SelectedItem_Barcder;
        private List<string> DeviceNames_Barcder;
        private bool isAutoTriggerMode = false;

        #endregion

        public BarcoderControl()
        {
            InitializeComponent();
            InitializeUI();
        }

        private void InitializeUI()
        {
            try
            {
                BindingList_Barcder();
                WriteEvents_Barcder();
            }
            catch (Exception ex)
            {
                Log.Write("BarcoderControl", $"InitializeUI error: {ex}");
            }
        }

        #region Barcoder Binding

        private void BindingList_Barcder()
        {
            try
            {
                DeviceNames_Barcder = new List<string>();

                foreach (var Key in Equipment.Instance.Barcoders.Keys)
                {
                    DeviceNames_Barcder.Add(Key);
                }

                if (DeviceNames_Barcder.Count == 0)
                {
                    listBoxItemsView?.SetItems();
                    return;
                }

                listBoxItemsView?.SetItems(DeviceNames_Barcder.ToArray());

                if (DeviceNames_Barcder.Count > 0)
                {
                    listBoxItemsView.SelectedIndex = 0;
                    OnSelected_Barcder(this, 0);
                }
            }
            catch (Exception ex)
            {
                Log.Write("BarcoderControl", $"Binding Barcoder List error: {ex}");
            }
        }

        #endregion

        #region Event Registration

        private void WriteEvents_Barcder()
        {
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

            if (btnBarcoderScan != null)
            {
                btnBarcoderScan.Click -= btnBarcoderScan_Click;
                btnBarcoderScan.Click += btnBarcoderScan_Click;
            }

            if (btn_ClearList != null)
            {
                btn_ClearList.Click -= btnClearList_Click;
                btn_ClearList.Click += btnClearList_Click;
            }
        }

        #endregion

        #region Button Click Handlers

        private void OnSelected_Barcder(object sender, int selectedIndex)
        {
            if (SelectedItem_Barcder != null)
            {
                UnsubscribeBarcoderEvents(SelectedItem_Barcder);
            }

            if (selectedIndex < 0 || selectedIndex >= DeviceNames_Barcder.Count) return;

            var name = DeviceNames_Barcder[selectedIndex];
            SelectedItem_Barcder = Equipment.Instance.Barcoders[name];

            if (SelectedItem_Barcder != null)
            {
                SubscribeBarcoderEvents(SelectedItem_Barcder);
            }

            LoadProperties_Barcoder(SelectedItem_Barcder?.Config, SelectedItem_Barcder?.Name);
            UpdateScanButtonState();

            // 부모 Form으로 이벤트 전달
            BarcoderSelected?.Invoke(this, new BarcoderSelectedEventArgs
            {
                SelectedBarcoder = SelectedItem_Barcder,
                DeviceName = name
            });
        }

        private void btn_Save_Barcoder_Setup_Click(object sender, EventArgs e)
        {
            try
            {
                var pc = propertyCollectionView?.GetCurrentProperties();
                if (pc != null && SelectedItem_Barcder != null)
                {
                    // 부모 Form으로 이벤트 전달
                    SaveRequested?.Invoke(this, new BarcoderSaveEventArgs
                    {
                        Barcoder = SelectedItem_Barcder,
                        Properties = pc
                    });
                }
            }
            catch (Exception ex)
            {
                Log.Write("BarcoderControl", $"Save button click error: {ex}");
                ErrorOccurred?.Invoke(this, $"저장 버튼 오류: {ex.Message}");
            }
        }

        private void btnBarcoderScan_Click(object sender, EventArgs e)
        {
            try
            {
                if (SelectedItem_Barcder == null)
                {
                    MessageBox.Show("바코드 리더를 선택하세요.");
                    return;
                }

                isAutoTriggerMode = true;
                //isAutoTriggerMode = false;
                // 부모 Form으로 이벤트 전달
                ScanRequested?.Invoke(this, new BarcoderScanEventArgs
                {
                    Barcoder = SelectedItem_Barcder,
                    IsAutoTriggerMode = isAutoTriggerMode
                });
            }
            catch (Exception ex)
            {
                Log.Write("BarcoderControl", $"btnBarcoderScan_Click error: {ex}");
                MessageBox.Show($"스캔 오류: {ex.Message}");
            }
        }

        private void btnClearList_Click(object sender, EventArgs e)
        {
            // 부모 Form으로 이벤트 전달
            ListClearRequested?.Invoke(this, new BarcoderListClearEventArgs());
        }

        #endregion

        #region Properties Loading

        private void LoadProperties_Barcoder(object config, string deviceName)
        {
            try
            {
                propertyCollectionView?.SetProperties(null);
                ConfigMapper_Barcder = null;

                if (config != null)
                {
                    Application.DoEvents();

                    ConfigMapper_Barcder = new ConfigReflectionMapper(config);
                    propertyCollectionView?.SetProperties(ConfigMapper_Barcder.PropertyCollection);
                    propertyCollectionView?.Refresh();

                    Log.Write("BarcoderControl", $"Device '{deviceName}' properties loaded successfully");
                }
            }
            catch (Exception ex)
            {
                Log.Write("BarcoderControl", $"LoadDeviceProperties error: {ex}");
                propertyCollectionView?.SetProperties(null);
                ConfigMapper_Barcder = null;
            }
        }

        #endregion

        #region Barcoder Events (Device → Control → Form)

        private void SubscribeBarcoderEvents(OpticonBarcodeReader barcoder)
        {
            if (barcoder != null)
            {
                barcoder.BarcodeDataReceived += Barcoder_BarcodeDataReceived;
                barcoder.ErrorOccurred += Barcoder_ErrorOccurred;
                barcoder.StatusChanged += Barcoder_StatusChanged;
            }
        }

        private void UnsubscribeBarcoderEvents(OpticonBarcodeReader barcoder)
        {
            if (barcoder != null)
            {
                barcoder.BarcodeDataReceived -= Barcoder_BarcodeDataReceived;
                barcoder.ErrorOccurred -= Barcoder_ErrorOccurred;
                barcoder.StatusChanged -= Barcoder_StatusChanged;
            }
        }

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
                labelBarcoderData.ForeColor = Color.Blue;
                Log.Write("Barcoder_Data", $"바코드 수신: {e.Data}");

                // 부모 Form으로 이벤트 전달
                BarcodeDataReceived?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                Log.Write("Barcoder_Data", $"BarcodeDataReceived 처리 오류: {ex}");
            }
        }

        private void Barcoder_ErrorOccurred(object sender, string error)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => Barcoder_ErrorOccurred(sender, error)));
                return;
            }

            labelBarcoderData.Text = $"오류: {error}";
            labelBarcoderData.ForeColor = Color.Red;
            Log.Write("Barcoder_Data", $"오류: {error}");

            // 부모 Form으로 이벤트 전달
            ErrorOccurred?.Invoke(this, error);
        }

        private void Barcoder_StatusChanged(object sender, string status)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => Barcoder_StatusChanged(sender, status)));
                return;
            }

            Log.Write("Barcoder", $"상태: {status}");
        }

        #endregion

        #region UI Update Methods (Form → Control)

        /// <summary>
        /// 스캔 상태 업데이트 (Form → Control)
        /// </summary>
        public void OnScanStateChanged(BarcoderScanStateChangedEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnScanStateChanged(e)));
                return;
            }

            labelBarcoderData.Text = e.Message;
            labelBarcoderData.ForeColor = e.Color;
        }

        /// <summary>
        /// 스캔 버튼 텍스트 업데이트 (Form → Control)
        /// </summary>
        public void OnButtonTextChanged(BarcoderButtonTextChangedEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnButtonTextChanged(e)));
                return;
            }

            btnBarcoderScan.Text = e.ButtonText;
        }

        /// <summary>
        /// 바코드 데이터 리스트에 추가 (Form → Control)
        /// </summary>
        public void OnBarcoderDataAdded(BarcoderDataAddedEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnBarcoderDataAdded(e)));
                return;
            }

            listBox_BarcodeData.Items.Add($"[{e.Timestamp:HH:mm:ss}] {e.Data}");

            // 자동 스크롤 (최신 항목으로)
            if (listBox_BarcodeData.Items.Count > 0)
            {
                listBox_BarcodeData.TopIndex = listBox_BarcodeData.Items.Count - 1;
            }
        }

        /// <summary>
        /// 바코드 데이터 리스트 초기화 (Form → Control)
        /// </summary>
        public void ClearBarcodeDataList()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(ClearBarcodeDataList));
                return;
            }

            listBox_BarcodeData.Items.Clear();
        }

        #endregion

        #region Helper Methods

        private void UpdateScanButtonState()
        {
            if (SelectedItem_Barcder == null)
            {
                btnBarcoderScan.Text = "Manual Scan";
                btnBarcoderScan.Enabled = false;
                labelBarcoderData.Text = "바코드 리더를 선택하세요.";
                labelBarcoderData.ForeColor = Color.Gray;
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
            labelBarcoderData.ForeColor = Color.Gray;
        }

        #endregion
    }
}