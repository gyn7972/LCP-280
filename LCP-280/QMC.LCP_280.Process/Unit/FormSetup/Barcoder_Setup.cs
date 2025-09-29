using QMC.Common;
using QMC.Common.BarcodeReader;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using static QMC.LCP_280.Process.Unit.FormSetup.BarcoderControl;

namespace QMC.LCP_280.Process.Unit.FormSetup
{
    [FormOrder(6)]
    public partial class Barcoder_Setup : Form
    {
        #region Barcoder State Events (Form → Control)

        /// <summary>
        /// 스캔 상태 변경 이벤트 (Form → Control)
        /// </summary>
        public class BarcoderScanStateChangedEventArgs : EventArgs
        {
            public string Message { get; set; }
            public System.Drawing.Color Color { get; set; }
        }

        /// <summary>
        /// 스캔 버튼 텍스트 변경 이벤트 (Form → Control)
        /// </summary>
        public class BarcoderButtonTextChangedEventArgs : EventArgs
        {
            public string ButtonText { get; set; }
        }

        /// <summary>
        /// 바코드 데이터 리스트 추가 이벤트 (Form → Control)
        /// </summary>
        public class BarcoderDataAddedEventArgs : EventArgs
        {
            public string Data { get; set; }
            public DateTime Timestamp { get; set; }
        }

        #endregion

        private readonly Equipment Equipment = Equipment.Instance;

        // 바코드 데이터 저장
        private List<BarcodeDataItem> _barcodeDataList;

        public Barcoder_Setup()
        {
            InitializeComponent();

            // 데이터 리스트 초기화
            _barcodeDataList = new List<BarcodeDataItem>();

            // BarcoderControl 이벤트 구독 (Control → Form)
            RegisterBarcoderControlEvents();
        }

        #region Event Registration

        private void RegisterBarcoderControlEvents()
        {
            if (barcoderControl != null)
            {
                barcoderControl.BarcoderSelected += BarcoderControl_BarcoderSelected;
                barcoderControl.SaveRequested += BarcoderControl_SaveRequested;
                barcoderControl.ScanRequested += BarcoderControl_ScanRequested;
                barcoderControl.ListClearRequested += BarcoderControl_ListClearRequested;
                barcoderControl.BarcodeDataReceived += BarcoderControl_BarcodeDataReceived;
                barcoderControl.ErrorOccurred += BarcoderControl_ErrorOccurred;
            }
        }

        #endregion

        #region Event Handlers (Control → Form)

        /// <summary>
        /// 바코드 리더 선택 시 호출
        /// </summary>
        private void BarcoderControl_BarcoderSelected(object sender, BarcoderSelectedEventArgs e)
        {
            try
            {
                Log.Write("Barcoder_Setup", $"바코드 리더 선택됨: {e.DeviceName}");

                // 선택된 바코드 리더에 대한 데이터 처리
                if (e.SelectedBarcoder != null)
                {
                    // 추가 데이터 처리 로직
                }
            }
            catch (Exception ex)
            {
                Log.Write("Barcoder_Setup", $"BarcoderSelected 처리 오류: {ex}");
            }
        }

        /// <summary>
        /// 저장 요청 시 호출
        /// </summary>
        private void BarcoderControl_SaveRequested(object sender, BarcoderSaveEventArgs e)
        {
            try
            {
                if (e.Barcoder == null || e.Properties == null)
                {
                    MessageBox.Show("저장할 데이터가 없습니다.");
                    return;
                }

                Log.Write("Barcoder_Setup", "설정 저장 시작");

                // ConfigMapper를 사용하여 속성 적용
                var configMapper = new ConfigReflectionMapper(e.Barcoder.Config);
                configMapper.ApplyToObject(e.Properties);

                // 설정 저장
                var saveResult = e.Barcoder.Config.Save();
                string result = saveResult == 0 ? "완료" : "에러";
                MessageBox.Show($"저장 결과: {result}");

                Log.Write("Barcoder_Setup", $"설정 저장 완료: {result}");

                // 설정 변경 후 재연결
                if (saveResult == 0 && e.Barcoder.IsConnected)
                {
                    e.Barcoder.Close();
                    System.Threading.Thread.Sleep(100);
                    e.Barcoder.Initialize();
                    Log.Write("Barcoder_Setup", "바코드 리더 재연결 완료");
                }
            }
            catch (Exception ex)
            {
                Log.Write("Barcoder_Setup", $"Save 처리 오류: {ex}");
                MessageBox.Show($"저장 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// 스캔 요청 시 호출
        /// </summary>
        private void BarcoderControl_ScanRequested(object sender, BarcoderScanEventArgs e)
        {
            try
            {
                if (e.Barcoder == null)
                {
                    MessageBox.Show("바코드 리더를 선택하세요.");
                    return;
                }

                // 연결 확인
                if (!e.Barcoder.IsConnected)
                {
                    if (e.Barcoder.Initialize() != 0)
                    {
                        MessageBox.Show("바코드 리더 연결에 실패했습니다.");
                        return;
                    }
                }

                // 자동/수동 모드에 따라 처리
                if (!e.IsAutoTriggerMode)
                {
                    PerformManualScan(e.Barcoder);
                }
                else
                {
                    ToggleAutoTrigger(e.Barcoder);
                }
            }
            catch (Exception ex)
            {
                Log.Write("Barcoder_Setup", $"Scan 처리 오류: {ex}");
                MessageBox.Show($"스캔 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// 리스트 초기화 요청 시 호출
        /// </summary>
        private void BarcoderControl_ListClearRequested(object sender, BarcoderListClearEventArgs e)
        {
            try
            {
                // 데이터 초기화
                _barcodeDataList.Clear();

                // Control에 UI 초기화 알림 (Form → Control)
                barcoderControl.ClearBarcodeDataList();

                Log.Write("Barcoder_Setup", "바코드 데이터 리스트 초기화");
            }
            catch (Exception ex)
            {
                Log.Write("Barcoder_Setup", $"ListClear 처리 오류: {ex}");
            }
        }

        /// <summary>
        /// 바코드 데이터 수신 시 호출
        /// </summary>
        private void BarcoderControl_BarcodeDataReceived(object sender, BarcodeDataEventArgs e)
        {
            try
            {
                Log.Write("Barcoder_Setup", $"바코드 데이터 수신: {e.Data} at {e.Timestamp}");

                // 데이터 처리
                ProcessBarcodeData(e.Data, e.Timestamp);

                // 리스트에 추가
                AddBarcodeDataToList(e.Data, e.Timestamp);
            }
            catch (Exception ex)
            {
                Log.Write("Barcoder_Setup", $"BarcodeData 처리 오류: {ex}");
            }
        }

        /// <summary>
        /// 에러 발생 시 호출
        /// </summary>
        private void BarcoderControl_ErrorOccurred(object sender, string error)
        {
            try
            {
                Log.Write("Barcoder_Setup", $"바코드 리더 오류: {error}");
                HandleBarcoderError(error);
            }
            catch (Exception ex)
            {
                Log.Write("Barcoder_Setup", $"Error 처리 오류: {ex}");
            }
        }

        #endregion

        #region Business Logic (데이터 처리)

        /// <summary>
        /// 수동 스캔 수행
        /// </summary>
        private void PerformManualScan(OpticonBarcodeReader barcoder)
        {
            try
            {
                // UI 상태 업데이트 (Form → Control)
                NotifyScanStateChanged("스캔 중...", Color.Orange);

                string data;
                int result = barcoder.Read(out data);

                if (result == 0 && !string.IsNullOrWhiteSpace(data))
                {
                    NotifyScanStateChanged($"[{DateTime.Now:HH:mm:ss}] {data}", Color.Blue);
                    Log.Write("Barcoder_Setup", $"수동 스캔 성공: {data}");

                    // 리스트에 추가
                    AddBarcodeDataToList(data, DateTime.Now);
                }
                else
                {
                    NotifyScanStateChanged("바코드를 읽지 못했습니다.", Color.Red);
                    Log.Write("Barcoder_Setup", "수동 스캔 실패");
                }
            }
            catch (Exception ex)
            {
                NotifyScanStateChanged($"스캔 오류: {ex.Message}", Color.Red);
                Log.Write("Barcoder_Setup", $"PerformManualScan 오류: {ex}");
            }
        }

        /// <summary>
        /// 자동 트리거 모드 토글
        /// </summary>
        private void ToggleAutoTrigger(OpticonBarcodeReader barcoder)
        {
            try
            {
                if (barcoder.IsAutoTriggerEnabled)
                {
                    barcoder.StopAutoTrigger();

                    // UI 업데이트 (Form → Control)
                    NotifyButtonTextChanged("Start Auto");
                    NotifyScanStateChanged("자동 트리거 모드 중지됨", Color.Gray);

                    Log.Write("Barcoder_Setup", "자동 트리거 모드 중지");
                }
                else
                {
                    if (barcoder.StartAutoTrigger() == 0)
                    {
                        // UI 업데이트 (Form → Control)
                        NotifyButtonTextChanged("Stop Auto");
                        NotifyScanStateChanged("자동 트리거 모드 시작됨 - 바코드를 스캐너에 가져다 대세요", Color.Green);

                        Log.Write("Barcoder_Setup", "자동 트리거 모드 시작");
                    }
                    else
                    {
                        NotifyScanStateChanged("자동 트리거 모드 시작 실패", Color.Red);
                        Log.Write("Barcoder_Setup", "자동 트리거 모드 시작 실패");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write("Barcoder_Setup", $"ToggleAutoTrigger 오류: {ex}");
            }
        }

        /// <summary>
        /// 바코드 데이터 처리
        /// </summary>
        private void ProcessBarcodeData(string data, DateTime timestamp)
        {
            // 실제 데이터 처리 로직 구현
            // 예:
            // - 데이터 검증
            // - 데이터베이스 저장
            // - 다른 시스템으로 전송
            // - 통계 업데이트

            Log.Write("Barcoder_Setup", $"데이터 처리: {data}");
        }

        /// <summary>
        /// 바코드 데이터를 리스트에 추가
        /// </summary>
        private void AddBarcodeDataToList(string data, DateTime timestamp)
        {
            // 데이터 저장
            _barcodeDataList.Add(new BarcodeDataItem
            {
                Data = data,
                Timestamp = timestamp
            });

            // Control에 UI 업데이트 알림 (Form → Control)
            barcoderControl.OnBarcoderDataAdded(new BarcoderDataAddedEventArgs
            {
                Data = data,
                Timestamp = timestamp
            });

            Log.Write("Barcoder_Setup", $"리스트에 추가: {data} (총 {_barcodeDataList.Count}개)");
        }

        /// <summary>
        /// 바코드 리더 에러 처리
        /// </summary>
        private void HandleBarcoderError(string error)
        {
            // 실제 에러 처리 로직 구현
            // 예:
            // - 재연결 시도
            // - 사용자 알림
            // - 에러 로그 저장

            Log.Write("Barcoder_Setup", $"에러 처리: {error}");
        }

        #endregion

        #region Notify Methods (Form → Control)

        /// <summary>
        /// 스캔 상태 변경 알림
        /// </summary>
        private void NotifyScanStateChanged(string message, Color color)
        {
            barcoderControl.OnScanStateChanged(new BarcoderScanStateChangedEventArgs
            {
                Message = message,
                Color = color
            });
        }

        /// <summary>
        /// 버튼 텍스트 변경 알림
        /// </summary>
        private void NotifyButtonTextChanged(string buttonText)
        {
            barcoderControl.OnButtonTextChanged(new BarcoderButtonTextChangedEventArgs
            {
                ButtonText = buttonText
            });
        }

        #endregion

        #region Helper Class

        /// <summary>
        /// 바코드 데이터 아이템
        /// </summary>
        private class BarcodeDataItem
        {
            public string Data { get; set; }
            public DateTime Timestamp { get; set; }
        }

        #endregion

        #region Form Cleanup

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (barcoderControl != null)
            {
                barcoderControl.BarcoderSelected -= BarcoderControl_BarcoderSelected;
                barcoderControl.SaveRequested -= BarcoderControl_SaveRequested;
                barcoderControl.ScanRequested -= BarcoderControl_ScanRequested;
                barcoderControl.ListClearRequested -= BarcoderControl_ListClearRequested;
                barcoderControl.BarcodeDataReceived -= BarcoderControl_BarcodeDataReceived;
                barcoderControl.ErrorOccurred -= BarcoderControl_ErrorOccurred;
            }

            base.OnFormClosing(e);
        }

        #endregion
    }
}