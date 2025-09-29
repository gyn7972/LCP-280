using QMC.Common;
using QMC.Common.BarcodeReader;
using QMC.Common.Vision;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using static QMC.LCP_280.Process.Unit.FormSetup.BarcoderControl;

namespace QMC.LCP_280.Process.Unit.FormSetup
{
    [FormOrder(6)]
    public partial class Barcoder_Setup : Form
    {
        private readonly Equipment Equipment = Equipment.Instance;

        public Barcoder_Setup()
        {
            InitializeComponent();

            // BarcoderControl 이벤트 구독
            RegisterBarcoderControlEvents();
        }

        /// <summary>
        /// BarcoderControl 이벤트 등록
        /// </summary>
        private void RegisterBarcoderControlEvents()
        {
            if (barcoderControl != null)
            {
                barcoderControl.BarcoderSelected += BarcoderControl_BarcoderSelected;
                barcoderControl.SaveRequested += BarcoderControl_SaveRequested;
                barcoderControl.ScanRequested += BarcoderControl_ScanRequested;
                barcoderControl.BarcodeDataReceived += BarcoderControl_BarcodeDataReceived;
                barcoderControl.ErrorOccurred += BarcoderControl_ErrorOccurred;
            }
        }

        #region 이벤트 핸들러

        /// <summary>
        /// 바코드 리더 선택 시 호출
        /// </summary>
        private void BarcoderControl_BarcoderSelected(object sender, BarcoderSelectedEventArgs e)
        {
            try
            {
                Log.Write("Barcoder_Setup", $"바코드 리더 선택됨: {e.DeviceName}");

                // 여기서 선택된 바코드 리더에 대한 데이터 처리
                // 예: 로그 기록, 상태 업데이트 등

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
        /// 바코드 데이터 수신 시 호출
        /// </summary>
        private void BarcoderControl_BarcodeDataReceived(object sender, BarcodeDataEventArgs e)
        {
            try
            {
                Log.Write("Barcoder_Setup", $"바코드 데이터 수신: {e.Data} at {e.Timestamp}");

                // 여기서 바코드 데이터 처리
                // 예: 데이터베이스 저장, 검증, 다른 시스템으로 전송 등
                ProcessBarcodeData(e.Data, e.Timestamp);
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

                // 여기서 에러 처리
                // 예: 알림, 로그, 재연결 시도 등
                HandleBarcoderError(error);
            }
            catch (Exception ex)
            {
                Log.Write("Barcoder_Setup", $"Error 처리 오류: {ex}");
            }
        }

        #endregion

        #region 데이터 처리 메서드

        /// <summary>
        /// 수동 스캔 수행
        /// </summary>
        private void PerformManualScan(OpticonBarcodeReader barcoder)
        {
            try
            {
                barcoderControl.UpdateScanStatus("스캔 중...", Color.Orange);

                string data;
                int result = barcoder.Read(out data);

                if (result == 0 && !string.IsNullOrWhiteSpace(data))
                {
                    barcoderControl.UpdateScanStatus($"[{DateTime.Now:HH:mm:ss}] {data}", Color.Blue);
                    Log.Write("Barcoder_Setup", $"수동 스캔 성공: {data}");
                }
                else
                {
                    barcoderControl.UpdateScanStatus("바코드를 읽지 못했습니다.", Color.Red);
                    Log.Write("Barcoder_Setup", "수동 스캔 실패");
                }
            }
            catch (Exception ex)
            {
                barcoderControl.UpdateScanStatus($"스캔 오류: {ex.Message}", Color.Red);
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
                    barcoderControl.UpdateScanButtonText("Start Auto");
                    barcoderControl.UpdateScanStatus("자동 트리거 모드 중지됨", Color.Gray);
                    Log.Write("Barcoder_Setup", "자동 트리거 모드 중지");
                }
                else
                {
                    if (barcoder.StartAutoTrigger() == 0)
                    {
                        barcoderControl.UpdateScanButtonText("Stop Auto");
                        barcoderControl.UpdateScanStatus("자동 트리거 모드 시작됨 - 바코드를 스캐너에 가져다 대세요", Color.Green);
                        Log.Write("Barcoder_Setup", "자동 트리거 모드 시작");
                    }
                    else
                    {
                        barcoderControl.UpdateScanStatus("자동 트리거 모드 시작 실패", Color.Red);
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
            // 여기에 실제 데이터 처리 로직 구현
            // 예:
            // - 데이터 검증
            // - 데이터베이스 저장
            // - 다른 시스템으로 전송
            // - 화면 업데이트

            Log.Write("Barcoder_Setup", $"데이터 처리: {data}");
        }

        /// <summary>
        /// 바코드 리더 에러 처리
        /// </summary>
        private void HandleBarcoderError(string error)
        {
            // 여기에 실제 에러 처리 로직 구현
            // 예:
            // - 재연결 시도
            // - 사용자 알림
            // - 에러 로그 저장

            Log.Write("Barcoder_Setup", $"에러 처리: {error}");
        }

        #endregion

        /// <summary>
        /// 폼 종료 시 이벤트 해제
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (barcoderControl != null)
            {
                barcoderControl.BarcoderSelected -= BarcoderControl_BarcoderSelected;
                barcoderControl.SaveRequested -= BarcoderControl_SaveRequested;
                barcoderControl.ScanRequested -= BarcoderControl_ScanRequested;
                barcoderControl.BarcodeDataReceived -= BarcoderControl_BarcodeDataReceived;
                barcoderControl.ErrorOccurred -= BarcoderControl_ErrorOccurred;
            }

            base.OnFormClosing(e);
        }
    }
}