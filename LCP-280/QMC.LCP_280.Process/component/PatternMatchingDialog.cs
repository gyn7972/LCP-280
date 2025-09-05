using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
using QMC.Common.VisionPart;
using QMC.Common.Vision;
using QMC.Common.Vision.Tools;
using QMC.Common.Cameras;
using QMC.Common;

namespace QMC.LCP_280.Process
{
    public partial class PatternMatchingDialog : Form
    {
        private string _recipeDirectory;
        private string _currentRecipeName = "Default";

        private InternalMultiPatternMatchingVisionPart _visionPart;
        private MultiPatternMatchingParameters _parameters;

        private readonly List<Camera> _cameras = new List<Camera>();
        private readonly List<string> _cameraNames = new List<string>();

        private bool _suspendAutoLoad = false;
        private Point _lastResultPoint = Point.Empty; // 검색 결과 표시용
        private double _lastResultAngle = 0;          // 각도 저장 (미사용시 확장 대비)

        public PatternMatchingDialog()
        {
            InitializeComponent();

            _recipeDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "PatternMatching");

            if (_visionPart == null)
            {
                _visionPart = new InternalMultiPatternMatchingVisionPart("PM_Dialog") { Simulated = true };
                _visionPart.Create();
            }
            _parameters = _visionPart.GetPatternMatchingParameters();

            maintROIControl.SetOwner(_visionPart);
            maintROIControl.SetImageviwer(_viewer);
            AttachEvents();

            if (_btnLoadImage != null) _btnLoadImage.Click += BtnLoadImage_Click;
            if (_btnClose != null) _btnClose.Click += (s, e) => Close();
            if (_btnSaveParam != null) _btnSaveParam.Click += BtnSaveParam_Click; // confirm save
            if (_btnLoadParam != null) _btnLoadParam.Click += (s, e) => LoadRecipeForCurrentCamera();

            if (patternMatchingParamControl != null)
            {
                patternMatchingParamControl.Load += patternMatchingParamControl_Load;
                patternMatchingParamControl.UpdateParameters(_parameters);
            }

            // 뷰어 결과 크로스 표시용 Paint 이벤트
            if (_viewer != null)
                _viewer.Paint += Viewer_PaintCross;

            LoadRecipe(_currentRecipeName);
            TryBindEquipmentCameras();
            InitializeCameraList();
            UpdateStatus("Ready");
        }

        private void BtnSaveParam_Click(object sender, EventArgs e)
        {
            try
            {
                maintROIControl?.CommitCurrentRoi();
                var dr = MessageBox.Show(this, "현재 설정을 저장하시겠습니까?", "저장 확인", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr == DialogResult.Yes)
                {
                    SaveRecipeForCurrentCamera();
                }
                else
                {
                    UpdateStatus("Save canceled");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "저장 중 오류: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Viewer_PaintCross(object sender, PaintEventArgs e)
        {
            if (_lastResultPoint == Point.Empty) return;
            if (_viewer?.Image == null) return;
            try
            {
                // PictureBox(Zoom) 좌표 변환
                var imgW = _viewer.Image.Width;
                var imgH = _viewer.Image.Height;
                var boxW = _viewer.ClientSize.Width;
                var boxH = _viewer.ClientSize.Height;
                double scale = Math.Min((double)boxW / imgW, (double)boxH / imgH);
                int drawW = (int)(imgW * scale);
                int drawH = (int)(imgH * scale);
                int offX = (boxW - drawW) / 2;
                int offY = (boxH - drawH) / 2;

                int cx = offX + (int)(_lastResultPoint.X * scale);
                int cy = offY + (int)(_lastResultPoint.Y * scale);
                int len = 15; // 반길이
                using (Pen p = new Pen(Color.Red, 2))
                {
                    e.Graphics.DrawLine(p, cx - len, cy, cx + len, cy);
                    e.Graphics.DrawLine(p, cx, cy - len, cx, cy + len);
                }
            }
            catch { }
        }

        #region Core Helpers
        private void UpdateStatus(string text)
        {
            if (_lblStatus != null) _lblStatus.Text = text;
        }

        private void SyncParametersFromUI()
        {
            if (patternMatchingParamControl?.Parameters != null)
            {
                _parameters = patternMatchingParamControl.Parameters; // 동일 객체 참조
                _visionPart.SetPatternMatchingParameters(_parameters);
            }
        }

        private string GetCurrentCameraName() => _viewer?.Camera?.Name ?? "NoCamera";
        #endregion

        #region Camera Binding
        private void TryBindEquipmentCameras()
        {
            try
            {
                var eq = Equipment.Instance;
                if (eq == null) return;
                if (eq.Cameras.Count == 0 && eq.State != EquipmentState.Initializing)
                {
                    try { eq.InitializeEquipment(); } catch { }
                }
                if (eq.Cameras.Count > 0) SetCameras(eq.Cameras.Values);
            }
            catch (Exception ex)
            {
                Log.Write("PatternMatchingDialog", "TryBindEquipmentCameras error: " + ex.Message);
            }
        }

        public void SetCameras(IEnumerable<Camera> cameras)
        {
            _cameras.Clear();
            _cameraNames.Clear();
            if (cameras == null) return;
            foreach (var cam in cameras)
            {
                if (cam == null) continue;
                _cameras.Add(cam);
                _cameraNames.Add(cam.Name ?? $"Cam{_cameraNames.Count}");
            }
        }

        private void InitializeCameraList()
        {
            try
            {
                if (cameraListBoxItemsView == null || _viewer == null) return;
                cameraListBoxItemsView.ItemSelected -= OnCameraItemSelected;
                cameraListBoxItemsView.ItemSelected += OnCameraItemSelected;

                if (_cameras.Count == 0)
                {
                    cameraListBoxItemsView.SetItems();
                    return;
                }
                cameraListBoxItemsView.SetItems(_cameraNames.ToArray());

                _viewer.VisibleCrossLine = true;
                _viewer.FrameRate = 30;

                cameraListBoxItemsView.SelectedIndex = 0;
                ApplyCameraSelection(0);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Camera list init failed: " + ex.Message);
                cameraListBoxItemsView?.SetItems();
            }
        }

        private void OnCameraItemSelected(object sender, int selectedIndex) => ApplyCameraSelection(selectedIndex);

        private void ApplyCameraSelection(int index)
        {
            try
            {
                if (index < 0 || index >= _cameras.Count) return;

                // 1) 현재 Viewer 내용 완전 초기화
                ClearViewer();

                // 2) (기존) 이전 카메라 라이브 정지
                try { _viewer.Camera?.StopLive(); } catch { }

                // 3) 새 카메라 할당
                var cam = _cameras[index];
                _viewer.Camera = cam;

                // 4) 새 카메라 기준 Viewer 재설정/초기 이미지 세팅
                ResetViewerForCameraChange(cam);

                // 5) 라이브 시작/업데이트
                try { cam.StartLive(); } catch { }
                _viewer.Display();
                _viewer.StartUpdateTask();

                // 6) ROI/파라미터 영역 동기화
                SyncImageInfoToControls(cam);

                // 7) 레시피 로드 (자동 저장은 제거)
                _suspendAutoLoad = true;
                try { LoadRecipeForCurrentCamera(); } finally { _suspendAutoLoad = false; }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ApplyCameraSelection error: " + ex.Message);
            }
        }

        private void SyncImageInfoToControls(Camera cam)
        {
            if (cam == null) return;
            try
            {
                maintROIControl.SetOwner(_visionPart);
                maintROIControl.SetImageviwer(_viewer);
                maintROIControl.UpdateImageInfo(cam.Resolution);
                maintROIControl.EnsureDefaultRoiTools();
            }
            catch (Exception ex)
            {
                Console.WriteLine("SyncImageInfoToControls error: " + ex.Message);
            }
        }

        private void ResetViewerForCameraChange(Camera cam)
        {
            try
            {
                if (cam == null) return;
                try
                {
                    if (_viewer.Scale != null && cam.Resolution.Width > 0)
                    {
                        _viewer.Scale.Wheel = 1.0;
                        _viewer.Scale.SetMousePoint(new Point(cam.Resolution.Width / 2, cam.Resolution.Height / 2));
                        _viewer.Scale.MoveCenter(new Size(cam.Resolution.Width, cam.Resolution.Height));
                    }
                }
                catch { }

                _viewer.InitCrossLine();
                _viewer.ShowCrossLine(_viewer.VisibleCrossLine);

                cam.GrabSync(out var snap);
                if (snap != null)
                {
                    _viewer.SetImageNDisplay(snap);
                    maintROIControl.UpdateImageInfo(cam.Resolution);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ResetViewerForCameraChange error: " + ex.Message);
            }
        }

        private void ClearViewer()
        {
            try
            {
                if (_viewer == null) return;
                try { _viewer.Camera?.StopLive(); } catch { }
                try { _viewer.ResultOverlays?.Clear(); } catch { }
                try { _viewer.NormalOverlays?.Clear(); } catch { }
                try { _viewer.Image = null; } catch { }
                try { if (_viewer.InputImage != null) _viewer.InputImage = null; } catch { }
                try { _lastResultPoint = Point.Empty; _lastResultAngle = 0; } catch { }
                try { _viewer.InitCrossLine(); } catch { }
                _viewer.Invalidate();
            }
            catch { }
        }
        #endregion

        #region Recipe Save / Load
        private string GetRecipePath(string cameraName, string recipeName)
        {
            string dir = Path.Combine(_recipeDirectory, cameraName ?? "NoCamera");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, recipeName + ".pmrecipe.json");
        }
        private string GetLegacyRecipePath(string recipeName)
        {
            Directory.CreateDirectory(_recipeDirectory);
            return Path.Combine(_recipeDirectory, recipeName + ".pmrecipe.json");
        }
        private void SaveRecipeForCurrentCamera() => SaveRecipe(_currentRecipeName, GetCurrentCameraName());
        private void LoadRecipeForCurrentCamera() => LoadRecipe(_currentRecipeName, GetCurrentCameraName());

        private void SaveRecipe(string recipeName, string cameraName = null)
        {
            try
            {
                // 현재 UI ROI 정보를 VisionPart로 먼저 반영
                maintROIControl?.CommitCurrentRoi();
                SyncParametersFromUI();
                if (string.IsNullOrEmpty(cameraName)) cameraName = GetCurrentCameraName();
                var roi = new PatternMatchingRoiJson();
                try
                {
                    roi.TrainStart = _visionPart.GetTrainStartPoint();
                    roi.TrainEnd = _visionPart.GetTrainEndPoint();
                    roi.InspectStart = _visionPart.GetInspectStartPoint();
                    roi.InspectEnd = _visionPart.GetInspectEndPoint();
                }
                catch { }
                if (_parameters == null) _parameters = _visionPart.GetPatternMatchingParameters();
                var container = new PatternMatchingRecipeJson
                {
                    Parameters = _parameters?.Clone(),
                    Roi = roi,
                    LastCameraName = cameraName
                };
                var path = GetRecipePath(cameraName, recipeName);
                PatternMatchingRecipeStore.Save(path, container);
                UpdateStatus($"Recipe saved: {path}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Recipe 저장 실패: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadRecipe(string recipeName, string cameraName = null)
        {
            try
            {
                if (string.IsNullOrEmpty(cameraName)) cameraName = GetCurrentCameraName();
                var path = GetRecipePath(cameraName, recipeName);
                var container = PatternMatchingRecipeStore.Load(path);
                if (container == null && cameraName != "NoCamera")
                {
                    var legacy = PatternMatchingRecipeStore.Load(GetLegacyRecipePath(recipeName));
                    if (legacy != null)
                    {
                        container = legacy;
                        PatternMatchingRecipeStore.Save(path, container); // migrate
                    }
                }
                if (container == null)
                {
                    UpdateStatus("Recipe 파일 없음. 새로 생성 예정.");
                    return;
                }
                if (container.Parameters != null)
                {
                    _parameters = container.Parameters.Clone();
                    _visionPart.SetPatternMatchingParameters(_parameters);
                    patternMatchingParamControl?.UpdateParameters(_parameters);
                }
                if (container.Roi != null)
                {
                    try
                    {
                        _visionPart.SetTrainStartPoint(container.Roi.TrainStart);
                        _visionPart.SetTrainEndPoint(container.Roi.TrainEnd);
                        _visionPart.SetInspectStartPoint(container.Roi.InspectStart);
                        _visionPart.SetInspectEndPoint(container.Roi.InspectEnd);
                        // ROI를 UI에 다시 반영
                        maintROIControl?.ReloadRoiFromPart();
                    }
                    catch { }
                }
                maintROIControl.SetOwner(_visionPart);
                maintROIControl.EnsureDefaultRoiTools();
                AttachEvents();
                UpdateStatus($"Recipe loaded: {path}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Recipe 로드 실패: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Train / Search
        private void BtnLoadImage_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files|*.bmp;*.png;*.jpg;*.jpeg;*.tif;*.tiff|All Files|*.*";
                if (ofd.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    using (var img = Image.FromFile(ofd.FileName))
                    {
                        var vimg = VisionImage.CreateInstance(img);
                        vimg.Tag = Path.GetFileName(ofd.FileName);
                        _visionPart.TestImage = vimg;
                        _viewer.Image = vimg.GetImage();
                        _viewer.Refresh();
                        maintROIControl.UpdateImageInfo(_viewer.Image.Size);
                        maintROIControl.EnsureDefaultRoiTools();
                        UpdateStatus($"Loaded: {Path.GetFileName(ofd.FileName)}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "이미지 로드 실패: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnTrain_Click(object sender, EventArgs e)
        {
            try
            {
                SyncParametersFromUI();
                Point start = _visionPart.GetTrainStartPoint();
                Point end = _visionPart.GetTrainEndPoint();
                if (start == Point.Empty && end == Point.Empty)
                {
                    MessageBox.Show(this, "Train ROI가 설정되지 않았습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                VisionImage sourceImage = null;
                if (_viewer.Camera != null && _viewer.Camera.Opened)
                    _viewer.Camera.GrabSync(out sourceImage);
                if (sourceImage == null && _viewer.Camera?.LatestImage?.RawData != null)
                    sourceImage = _viewer.Camera.LatestImage;
                if (sourceImage == null && _visionPart.TestImage?.GetImage() != null)
                    sourceImage = _visionPart.TestImage;
                if (sourceImage == null && _viewer.InputImage?.RawData != null)
                    sourceImage = _viewer.InputImage;
                if (sourceImage == null || sourceImage.GetImage() == null)
                {
                    MessageBox.Show(this, "Train에 사용할 이미지가 없습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                var cut = sourceImage.CutVisionImage(start, end);
                if (cut == null || cut.GetImage() == null)
                {
                    MessageBox.Show(this, "ROI 잘라내기에 실패했습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                AddTrainImage(cut);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Train 실패: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private VisionImage AcquireCurrentSearchImage()
        {
            VisionImage src = null;
            try
            {
                var cam = _viewer?.Camera;
                if (cam != null && cam.Opened && cam.LatestImage?.RawData != null)
                    src = cam.LatestImage;
                if (src == null && cam != null && cam.Opened)
                {
                    try { cam.GrabSync(out src); } catch { }
                }
                if (src == null && _viewer?.InputImage?.RawData != null)
                    src = _viewer.InputImage;
                if (src == null && _visionPart.TestImage?.RawData != null)
                    src = _visionPart.TestImage;
            }
            catch { }
            return src;
        }

        private void EnsureTemplatesTrained()
        {
            if (_parameters?.TrainImages == null) return;
            for (int i = 0; i < _parameters.TrainImages.Count; i++)
            {
                var t = _parameters.TrainImages[i];
                if (t == null || t.GetImage() == null) continue;
                try { _visionPart.OnTrain(new Point(0, 0), new Point(t.Header.Width - 1, t.Header.Height - 1), _parameters, null, i); } catch { }
            }
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            SyncParametersFromUI();
            if (_parameters == null)
                _parameters = _visionPart.GetPatternMatchingParameters();

            if (_parameters.TrainImages.Count == 0 || _parameters.TrainImages.All(v => v == null || v.GetImage() == null))
            {
                MessageBox.Show(this, "최소 1개 이상의 Train Image가 필요합니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var testImage = AcquireCurrentSearchImage();
            if (testImage == null || testImage.GetImage() == null)
            {
                MessageBox.Show(this, "검색할 이미지(카메라 또는 로드된 이미지)가 없습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            _visionPart.TestImage = testImage; // 내부 참조 업데이트

            EnsureTemplatesTrained();

            Point searchStart = new Point(0, 0);
            Point searchEnd = new Point(testImage.Header.Width - 1, testImage.Header.Height - 1);
            try
            {
                if (_visionPart.UseInspectRoi)
                {
                    var ispStart = _visionPart.GetInspectStartPoint();
                    var ispEnd = _visionPart.GetInspectEndPoint();
                    if (ispEnd.X > ispStart.X && ispEnd.Y > ispStart.Y)
                    {
                        searchStart = ispStart;
                        searchEnd = ispEnd;
                    }
                }
            }
            catch { }

            try
            {
                if (_viewer != null) _viewer.ResultOverlays.Clear();
                // 검색 전에 ROI 최신화
                maintROIControl?.CommitCurrentRoi();
                int ret = _visionPart.OnSearch(searchStart, searchEnd, _parameters, null, testImage);
                if (ret != 0)
                {
                    UpdateStatus("Search failed (ret=" + ret + ")");
                    return;
                }
                var result = _visionPart.GetResult();
                if (result != null && result.Values.Count > 0)
                {
                    var v = result.Values[0];
                    _lastResultPoint = new Point((int)v.X, (int)v.Y);
                    _lastResultAngle = v.R;
                    // UI 표시 (디자이너에 추가된 텍스트박스 존재 시)
                    if (txtResultX != null) txtResultX.Text = v.X.ToString("0.000");
                    if (txtResultY != null) txtResultY.Text = v.Y.ToString("0.000");
                    if (txtResultT != null) txtResultT.Text = v.R.ToString("0.000");
                    _viewer?.Invalidate();
                }
                // 결과 오버레이 (패턴툴에서 제공되면 표시)
                if (result != null && _viewer != null)
                {
                    foreach (var ov in result.ResultOverlays)
                    {
                        ov.Visible = true;
                        _viewer.ResultOverlays.Add(ov);
                    }
                    _viewer.Display();
                }

                if (result != null && result.Values.Count > 0)
                {
                    UpdateStatus("Search Success.");
                }
                else
                {
                    UpdateStatus("Search Fail.");
                }

                    
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Search 예외: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus("Search exception");
            }
        }

        private void AddTrainImage(VisionImage cut)
        {
            if (cut == null || cut.RawData == null || cut.Header == null || cut.Header.Width <= 0 || cut.Header.Height <= 0) return;
            // ROI Commit 후 Tag 순서 보정
            maintROIControl?.CommitCurrentRoi();
            _parameters.TrainImages.RemoveAll(v => v == null || v.RawData == null);
            cut.Tag = $"Train_{_parameters.TrainImages.Count}";
            _parameters.TrainImages.Add(cut);
            // 파라미터 컨트롤 즉시 갱신
            patternMatchingParamControl?.UpdateParameters(_parameters);
            UpdateStatus($"Train image added (Count={_parameters.TrainImages.Count})");
        }
        #endregion

        #region Events / ROI
        private void patternMatchingParamControl_Load(object sender, EventArgs e)
        {
            patternMatchingParamControl?.UpdateParameters(_parameters);
        }

        private void AttachEvents()
        {
            if (maintROIControl != null)
            {
                maintROIControl.TrainImageCaptured -= MaintROIControl_TrainImageCaptured;
                maintROIControl.TrainImageCaptured += MaintROIControl_TrainImageCaptured;
            }
        }

        private void MaintROIControl_TrainImageCaptured(VisionImage image)
        {
            try
            {
                if (image == null || image.GetImage() == null) return;
                SyncParametersFromUI();
                AddTrainImage(image);
            }
            catch (Exception ex)
            {
                Log.Write("PatternMatchingDialog", "TrainImageCaptured handler exception: " + ex.Message);
            }
        }
        #endregion

        #region Internal VisionPart Wrapper
        private class InternalMultiPatternMatchingVisionPart : MultiPatternMatchingVisionPart
        {
            private MultiPatternMatchingParameters _parameters = new MultiPatternMatchingParameters();
            public InternalMultiPatternMatchingVisionPart(string name) : base(name) { Simulated = true; }
            public override MultiPatternMatchingParameters GetPatternMatchingParameters() => _parameters;
            public override void SetPatternMatchingParameters(MultiPatternMatchingParameters parameters)
            {
                _parameters = parameters ?? new MultiPatternMatchingParameters();
            }
            public override IlluminationDataSet GetIlluminationDataSet() => null;
        }
        #endregion

        #region Static API
        public static void ShowDialogModal(IWin32Window owner = null)
        {
            using (var dlg = new PatternMatchingDialog())
            {
                if (owner == null) dlg.ShowDialog();
                else dlg.ShowDialog(owner);
            }
        }
        #endregion

    }
}