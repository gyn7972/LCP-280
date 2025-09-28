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

        #region Image Viewer - 새로 추가
        private bool isCapturingImage = false;
        private Image currentCapturedImage = null;
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
                BindingList_Barcder();
                WriteEvents_Barcder();
                InitializeImageViewer(); // 이미지 뷰어 초기화 추가
            }
            catch (Exception ex)
            {
                Log.Write("LCP-280", $"InitializeUI error: {ex}");
            }
        }

        #region Image Viewer Implementation

        /// <summary>
        /// 이미지 뷰어 초기화
        /// </summary>
        private void InitializeImageViewer()
        {
            try
            {
                // 이미지 뷰어 컨트롤들이 존재하는지 확인
                if (btnCaptureImage != null)
                {
                    btnCaptureImage.Click += BtnCaptureImage_Click;
                }

                if (btnSaveImage != null)
                {
                    btnSaveImage.Click += BtnSaveImage_Click;
                    btnSaveImage.Enabled = false;
                }

                if (lblImageStatus != null)
                {
                    lblImageStatus.Text = "Ready";
                    lblImageStatus.ForeColor = Color.Black;
                }

                if (progressBarImage != null)
                {
                    progressBarImage.Visible = false;
                }

                // 기본 플레이스홀더 이미지 표시
                DisplayPlaceholderImage();

                Log.Write("ImageViewer", "이미지 뷰어 초기화 완료");
            }
            catch (Exception ex)
            {
                Log.Write("ImageViewer", $"이미지 뷰어 초기화 오류: {ex}");
            }
        }

        /// <summary>
        /// 플레이스홀더 이미지 표시
        /// </summary>
        private void DisplayPlaceholderImage()
        {
            try
            {
                // pictureBoxScanner가 존재하지 않으면 종료
                if (pictureBoxScanner == null) return;

                using (Bitmap placeholder = new Bitmap(640, 480))
                using (Graphics g = Graphics.FromImage(placeholder))
                {
                    g.Clear(Color.DarkGray);

                    string message = "No Image\n\nClick 'Capture' to get\nscanner image\n\n⚠️ Takes ~40 seconds";
                    using (Font font = new Font("Arial", 12, FontStyle.Bold))
                    using (Brush brush = new SolidBrush(Color.White))
                    {
                        StringFormat format = new StringFormat
                        {
                            Alignment = StringAlignment.Center,
                            LineAlignment = StringAlignment.Center
                        };

                        g.DrawString(message, font, brush,
                                   new RectangleF(0, 0, 640, 480), format);
                    }

                    if (pictureBoxScanner.Image != null)
                    {
                        pictureBoxScanner.Image.Dispose();
                    }
                    pictureBoxScanner.Image = new Bitmap(placeholder);
                }
            }
            catch (Exception ex)
            {
                Log.Write("ImageViewer", $"플레이스홀더 이미지 생성 오류: {ex}");
            }
        }

        /// <summary>
        /// 이미지 캡처 버튼 클릭 이벤트
        /// </summary>
        private async void BtnCaptureImage_Click(object sender, EventArgs e)
        {
            if (SelectedItem_Barcder == null || !SelectedItem_Barcder.IsConnected)
            {
                MessageBox.Show("바코드 리더를 먼저 연결하세요.", "알림",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (isCapturingImage)
            {
                MessageBox.Show("이미지 캡처가 진행 중입니다.", "알림",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult result = MessageBox.Show(
                "이미지 캡처는 약 40초가 소요됩니다.\n계속하시겠습니까?",
                "이미지 캡처 확인",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            await CaptureImageAsync();
        }

        /// <summary>
        /// 비동기 이미지 캡처
        /// </summary>
        private async Task CaptureImageAsync()
        {
            isCapturingImage = true;

            try
            {
                // UI 상태 변경
                if (btnCaptureImage != null) btnCaptureImage.Enabled = false;
                if (btnSaveImage != null) btnSaveImage.Enabled = false;
                if (lblImageStatus != null)
                {
                    lblImageStatus.Text = "Capturing... (40초 소요)";
                    lblImageStatus.ForeColor = Color.Orange;
                }
                if (progressBarImage != null)
                {
                    progressBarImage.Visible = true;
                    progressBarImage.Style = ProgressBarStyle.Marquee;
                }

                await Task.Run(() =>
                {
                    try
                    {
                        // 실제 스캐너 이미지 캡처 시도
                        Invoke(new Action(() =>
                        {
                            CaptureRealImage(); // CreateTestImage() 대신 실제 캡처 호출
                        }));
                    }
                    catch (Exception ex)
                    {
                        Invoke(new Action(() =>
                        {
                            Log.Write("ImageViewer", $"이미지 캡처 오류: {ex}");
                            if (lblImageStatus != null)
                            {
                                lblImageStatus.Text = $"캡처 실패: {ex.Message}";
                                lblImageStatus.ForeColor = Color.Red;
                            }
                            // 실패 시 테스트 이미지로 폴백
                            CreateTestImage();
                        }));
                    }
                });

                if (lblImageStatus != null)
                {
                    lblImageStatus.Text = "캡처 완료";
                    lblImageStatus.ForeColor = Color.Green;
                }
                if (btnSaveImage != null) btnSaveImage.Enabled = true;

                Log.Write("ImageViewer", "이미지 캡처 완료");
            }
            catch (Exception ex)
            {
                Log.Write("ImageViewer", $"이미지 캡처 프로세스 오류: {ex}");
                if (lblImageStatus != null)
                {
                    lblImageStatus.Text = $"오류: {ex.Message}";
                    lblImageStatus.ForeColor = Color.Red;
                }
            }
            finally
            {
                isCapturingImage = false;
                if (btnCaptureImage != null) btnCaptureImage.Enabled = true;
                if (progressBarImage != null) progressBarImage.Visible = false;
            }
        }

        /// <summary>
        /// 테스트용 더미 이미지 생성
        /// </summary>
        private void CreateTestImage()
        {
            try
            {
                if (pictureBoxScanner == null) return;

                using (Bitmap testImage = new Bitmap(640, 480))
                using (Graphics g = Graphics.FromImage(testImage))
                {
                    // 그라데이션 배경
                    //using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    //    new Rectangle(0, 0, 640, 480),
                    //    Color.LightBlue, Color.DarkBlue,
                    //    System.Drawing.Drawing2D.LinearGradientMode.Diagonal))
                    //{
                    //    g.FillRectangle(brush, 0, 0, 640, 480);
                    //}

                    string info = $"Test Scanner Image\n{DateTime.Now:yyyy-MM-dd HH:mm:ss}\n640 x 480\nNLV-5201 Scanner";
                    using (Font font = new Font("Arial", 14, FontStyle.Bold))
                    using (Brush textBrush = new SolidBrush(Color.White))
                    {
                        StringFormat format = new StringFormat
                        {
                            Alignment = StringAlignment.Center,
                            LineAlignment = StringAlignment.Center
                        };

                        g.DrawString(info, font, textBrush,
                                   new RectangleF(0, 0, 640, 480), format);
                    }

                    // 십자선 그리기
                    using (Pen crossPen = new Pen(Color.Red, 2))
                    {
                        g.DrawLine(crossPen, 320, 0, 320, 480);
                        g.DrawLine(crossPen, 0, 240, 640, 240);
                    }

                    if (currentCapturedImage != null)
                    {
                        currentCapturedImage.Dispose();
                    }

                    currentCapturedImage = new Bitmap(testImage);

                    if (pictureBoxScanner.Image != null)
                    {
                        pictureBoxScanner.Image.Dispose();
                    }
                    pictureBoxScanner.Image = new Bitmap(testImage);
                }
            }
            catch (Exception ex)
            {
                Log.Write("ImageViewer", $"테스트 이미지 생성 오류: {ex}");
            }
        }

        /// <summary>
        /// 실제 스캐너에서 이미지 캡처
        /// </summary>
        private void CaptureRealImage()
        {
            try
            {
                if (pictureBoxScanner == null) return;

                byte[] imageData;
                int result = SelectedItem_Barcder.CaptureImage(out imageData);

                if (result == 0 && imageData != null && imageData.Length > 0)
                {
                    // 바이트 배열을 이미지로 변환
                    using (var ms = new MemoryStream(imageData))
                    {
                        try
                        {
                            // 현재 이미지 해제
                            if (currentCapturedImage != null)
                            {
                                currentCapturedImage.Dispose();
                            }

                            if (pictureBoxScanner.Image != null)
                            {
                                pictureBoxScanner.Image.Dispose();
                            }

                            // 새 이미지 설정
                            currentCapturedImage = new Bitmap(Image.FromStream(ms));
                            pictureBoxScanner.Image = new Bitmap(currentCapturedImage);

                            Log.Write("ImageViewer", $"실제 이미지 캡처 성공: {imageData.Length} bytes");
                        }
                        catch (Exception ex)
                        {
                            Log.Write("ImageViewer", $"이미지 변환 오류: {ex.Message}");
                            // 실패 시 테스트 이미지 생성
                            CreateTestImage();
                        }
                    }
                }
                else
                {
                    Log.Write("ImageViewer", $"이미지 캡처 실패: result={result}");
                    // 실패 시 테스트 이미지 생성
                    CreateTestImage();
                }
            }
            catch (Exception ex)
            {
                Log.Write("ImageViewer", $"실제 이미지 캡처 오류: {ex}");
                // 실패 시 테스트 이미지 생성
                CreateTestImage();
            }
        }

        /// <summary>
        /// 이미지 저장 버튼 클릭 이벤트
        /// </summary>
        private void BtnSaveImage_Click(object sender, EventArgs e)
        {
            if (currentCapturedImage == null)
            {
                MessageBox.Show("저장할 이미지가 없습니다.", "알림",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SaveFileDialog saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "PNG Files|*.png|JPEG Files|*.jpg|Bitmap Files|*.bmp";
                    saveDialog.DefaultExt = "png";
                    saveDialog.FileName = $"Scanner_Image_{DateTime.Now:yyyyMMdd_HHmmss}";

                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        ImageFormat format = ImageFormat.Png;
                        string extension = Path.GetExtension(saveDialog.FileName).ToLower();

                        switch (extension)
                        {
                            case ".jpg":
                            case ".jpeg":
                                format = ImageFormat.Jpeg;
                                break;
                            case ".bmp":
                                format = ImageFormat.Bmp;
                                break;
                            default:
                                format = ImageFormat.Png;
                                break;
                        }

                        currentCapturedImage.Save(saveDialog.FileName, format);

                        if (lblImageStatus != null)
                        {
                            lblImageStatus.Text = "저장 완료";
                            lblImageStatus.ForeColor = Color.Green;
                        }

                        MessageBox.Show($"이미지가 저장되었습니다.\n{saveDialog.FileName}", "저장 완료",
                                      MessageBoxButtons.OK, MessageBoxIcon.Information);

                        Log.Write("ImageViewer", $"이미지 저장 완료: {saveDialog.FileName}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write("ImageViewer", $"이미지 저장 오류: {ex}");
                MessageBox.Show($"이미지 저장 중 오류가 발생했습니다:\n{ex.Message}", "저장 오류",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);

                if (lblImageStatus != null)
                {
                    lblImageStatus.Text = "저장 실패";
                    lblImageStatus.ForeColor = Color.Red;
                }
            }
        }

        /// <summary>
        /// 이미지 뷰어 정리
        /// </summary>
        private void CleanupImageViewer()
        {
            try
            {
                if (currentCapturedImage != null)
                {
                    currentCapturedImage.Dispose();
                    currentCapturedImage = null;
                }

                if (pictureBoxScanner?.Image != null)
                {
                    pictureBoxScanner.Image.Dispose();
                    pictureBoxScanner.Image = null;
                }
            }
            catch (Exception ex)
            {
                Log.Write("ImageViewer", $"이미지 뷰어 정리 오류: {ex}");
            }
        }

        #endregion

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
