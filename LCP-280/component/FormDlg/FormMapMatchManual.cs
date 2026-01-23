using QMC.Common;
using QMC.Common.Cameras.HIKVISION;
using QMC.Common.UI;
using QMC.Common.Vision;
using QMC.LCP_280.Process;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using static QMC.Common.Material;

namespace QMC.LCP_280.Process.Component
{
    public partial class FormMapMatchManual : Form
    {
        public event EventHandler<ManualMapMatchAppliedEventArgs> ManualMatchApplied;

        private PointF? _pickedScan;
        private PointF? _pickedDownload;
        private readonly List<MapPair> _pairs = new List<MapPair>();
        private PickMode _pickMode = PickMode.None;

        // ===== Camera (Operator_Main 방식) =====
        private VisionImageViewer _cameraViewer;
        private HIKGigECamera _camera; // Equipment.Instance.InStageCam 사용

        // ===== Downloaded Map File =====
        private string _downloadedMapFilePath;
        private List<Point> _downloadedMapPoints;          // waf에서 읽은 원본 좌표 보관
        private string _downloadedMapLoadedFromPath;        // 어떤 파일에서 읽었는지(캐시 무효화 판단용)

        public IReadOnlyList<Point> DownloadedMapPoints => _downloadedMapPoints;

        // ===== Scan Map File (like Download) =====
        private string _scanMapFilePath;
        private string _scanMapLoadedFromPath; // 캐시 무효화 판단용

        // ===== Scan Map Data Cache (like Download) =====
        private List<Point> _scanMapPoints;
        private string _scanMapInfoText;

        public IReadOnlyList<Point> ScanMapPoints => _scanMapPoints;

        // [ADD] 장비에서 실제 사용 중인 웨이퍼(Scan 기준 데이터)
        private MaterialWafer _targetWafer;

        // [ADD] Download 파일에서 읽은 원본 die(PreRank 포함)
        private List<MaterialDie> _downloadedDies;

        // [ADD] 자동 Dx/Dy 갱신 중에 ValueChanged 재진입 방지
        private bool _suppressAutoCalc;

        // [ADD] Undo 스냅샷
        private bool _undoCaptured;
        private Dictionary<int, DieSnapshot> _undoByDieIndex;

        private struct DieSnapshot
        {
            public int MapX;
            public int MapY;
            public int PreRank;
        }

        // [ADD] Apply 누적 방지용 "원본" 스냅샷 (ScanMap 로드 시점 기준)
        private bool _baseCaptured;
        private Dictionary<int, DieSnapshot> _baseByDieIndex;

        public FormMapMatchManual()
        {
            InitializeComponent();

            this.viewScan.MotorMoveRequested += ViewScan_MotorMoveRequested;
            this.viewDownload.MotorMoveRequested += ViewDownload_MotorMoveRequested;

            // [ADD] 클릭만으로 Pick (DieScanMapControl에서 새로 노출한 이벤트)
            try { this.viewScan.ItemClicked += ViewScan_ItemClickedPick; } catch { }
            try { this.viewDownload.ItemClicked += ViewDownload_ItemClickedPick; } catch { }

            Shown += FormMapMatchManual_Shown;
            FormClosing += FormMapMatchManual_FormClosing;
            VisibleChanged += FormMapMatchManual_VisibleChanged;

            // [ADD] 회전/미러 변경 시 자동 재계산
            try { cbRotate.SelectedIndexChanged += (s, e) => RecomputeDxDyFromPairsAndUpdateUi(); } catch { }
            try { chkMirrorX.CheckedChanged += (s, e) => RecomputeDxDyFromPairsAndUpdateUi(); } catch { }
            try { chkMirrorY.CheckedChanged += (s, e) => RecomputeDxDyFromPairsAndUpdateUi(); } catch { }
        }

        public void BindTargetWafer(MaterialWafer wafer)
        {
            _targetWafer = wafer;

            // Scan은 항상 장비 웨이퍼를 화면에 표시 (Auto/Manual 공용)
            try
            {
                var dies = _targetWafer?.Dies;
                if (dies != null && dies.Count > 0)
                {
                    // DieScanMapControl은 MaterialDie를 바로 받을 수 있음
                    viewScan?.SetDieList(dies);

                    // preview/overlay용 캐시도 갱신
                    _scanMapPoints = dies.Select(d => new Point(d.MapX, d.MapY)).ToList();
                    _scanMapInfoText = _targetWafer?.WaferId ?? "SCAN";
                }
            }
            catch { }
        }

        private void ViewScan_ItemClickedPick(object sender, QMC.Common.Controls.DisplayView_DieScanMap.DisplayItemEventArgs e)
        {
            //if (_pickMode != PickMode.Scan) return;
            if (e?.Item == null) return;

            _pickedScan = new PointF(e.Item.DieMap.X, e.Item.DieMap.Y);

            _pickMode = PickMode.None;
            btnPickScan.Enabled = true;
            btnPickDownload.Enabled = true;
            UpdateHint();
        }

        private void ViewDownload_ItemClickedPick(object sender, QMC.Common.Controls.DisplayView_DieScanMap.DisplayItemEventArgs e)
        {
            //if (_pickMode != PickMode.Download) return;
            if (e?.Item == null) return;

            _pickedDownload = new PointF(e.Item.DieMap.X, e.Item.DieMap.Y);

            _pickMode = PickMode.None;
            btnPickScan.Enabled = true;
            btnPickDownload.Enabled = true;
            UpdateHint();
        }

        public void SetDownloadedMapFile(string downloadedMapFilePath)
        {
            _downloadedMapFilePath = downloadedMapFilePath;
        }

        public void SetScanMapFile(string scanMapFilePath)
        {
            _scanMapFilePath = scanMapFilePath;
        }

        public void SetScanItems(IList<QMC.Common.Controls.DisplayView_DieScanMap.DisplayItem> items)
        {
            if (InvokeRequired) { BeginInvoke(new Action(() => SetScanItems(items))); return; }

            var list = items?.ToList() ?? new List<QMC.Common.Controls.DisplayView_DieScanMap.DisplayItem>();
            var dies = new List<MaterialDie>(list.Count);

            for (int i = 0; i < list.Count; i++)
            {
                var it = list[i];
                if (it == null) continue;

                dies.Add(new MaterialDie
                {
                    Index = it.DieId >= 0 ? it.DieId : i,
                    Name = it.Info ?? "",
                    MapX = it.DieMap.X,
                    MapY = it.DieMap.Y,
                    Presence = MaterialPresence.Exist,
                    State = DieProcessState.Mapped
                });
            }

            viewScan?.SetDieList(dies);
        }

        public void SetDownloadItems(IList<QMC.Common.Controls.DisplayView_DieScanMap.DisplayItem> items)
        {
            if (InvokeRequired) { BeginInvoke(new Action(() => SetDownloadItems(items))); return; }

            var list = items?.ToList() ?? new List<QMC.Common.Controls.DisplayView_DieScanMap.DisplayItem>();
            var dies = new List<MaterialDie>(list.Count);

            for (int i = 0; i < list.Count; i++)
            {
                var it = list[i];
                if (it == null) continue;

                dies.Add(new MaterialDie
                {
                    Index = it.DieId >= 0 ? it.DieId : i,
                    Name = it.Info ?? "",
                    MapX = it.DieMap.X,
                    MapY = it.DieMap.Y,
                    Presence = MaterialPresence.Exist,
                    State = DieProcessState.Mapped
                });
            }

            viewDownload?.SetDieList(dies);
        }

        public void BindEquipmentCamera(HIKGigECamera camera)
        {
            _camera = camera;
            BindCamera();
        }

        public void BindEquipmentInStageCamera()
        {
            BindEquipmentCamera(Equipment.Instance?.InStageCam);
        }

        private void FormMapMatchManual_Shown(object sender, EventArgs e)
        {
            try
            {
                EnsureCameraViewerCreated();

                if (_camera == null)
                    _camera = Equipment.Instance?.InStageCam;

                BindCamera();
                ResumeCameras();

                LoadScanMapToView();
                // [ADD] 초기에도 한번 계산 시도
                RecomputeDxDyFromPairsAndUpdateUi();
            }
            catch { }
        }

        private void FormMapMatchManual_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                PauseCameras();

                if (_cameraViewer != null)
                {
                    try { _cameraViewer.SuspendDisplay(); } catch { }
                    try { _cameraViewer.Camera = null; } catch { }
                }
            }
            catch { }

            // [ADD] OK로 닫는 게 아니면 Undo (Apply Preview 후 취소 대응)
            try
            {
                if (this.DialogResult != DialogResult.OK)
                    RestoreUndoSnapshot();
            }
            catch { }
        }

        private void FormMapMatchManual_VisibleChanged(object sender, EventArgs e)
        {
            try
            {
                if (Visible) ResumeCameras();
                else PauseCameras();
            }
            catch { }
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            try { ResumeCameras(); } catch { }
        }

        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);
            try { PauseCameras(); } catch { }
        }

        private void EnsureCameraViewerCreated()
        {
            if (_cameraViewer != null && !_cameraViewer.IsDisposed)
                return;

            Control host = null;

            try
            {
                if (pbCamera != null && !pbCamera.IsDisposed)
                {
                    // 중요: host를 숨기면 자식도 안 보임
                    pbCamera.Visible = true;

                    // 기존 PictureBox 내용/자식 정리
                    try { pbCamera.Image = null; } catch { }
                    try { pbCamera.Controls.Clear(); } catch { }

                    host = pbCamera;
                }
            }
            catch { } 

            if (host == null)
                host = pnlCamera;
            if (host == null)
                return;

            _cameraViewer = new VisionImageViewer
            {
                Dock = DockStyle.Fill
            };

            try { _cameraViewer.SizeMode = PictureBoxSizeMode.Zoom; } catch { }
            try { _cameraViewer.VisibleCrossLine = true; } catch { }
            try { _cameraViewer.FrameRate = 30; } catch { }

            host.Controls.Add(_cameraViewer);
            _cameraViewer.BringToFront();

            //if (_cameraViewer != null && !_cameraViewer.IsDisposed)
            //    return;

            //// 기존 pbCamera는 사용하지 않으므로 숨김 처리(디자이너 변경 없이)
            //try { if (pbCamera != null) pbCamera.Visible = false; } catch { }

            //_cameraViewer = new VisionImageViewer
            //{
            //    Dock = DockStyle.Fill
            //};

            //// Operator_Main 스타일 옵션들(필요 시)
            //try { _cameraViewer.VisibleCrossLine = true; } catch { }
            //try { _cameraViewer.FrameRate = 30; } catch { }

            //// pnlCamera 안에 넣기 (Designer에 pnlCamera가 존재)
            //if (pnlCamera != null)
            //{
            //    pnlCamera.Controls.Add(_cameraViewer);
            //    _cameraViewer.BringToFront();
            //}
        }

        private void BindCamera()
        {
            try
            {
                if (_cameraViewer == null || _cameraViewer.IsDisposed)
                    return;

                if (_camera == null)
                    return;

                if (_cameraViewer.Camera != _camera)
                    _cameraViewer.Camera = _camera;

                try { _camera.StartLive(); } catch { }
                try { _cameraViewer.StartUpdateTask(); } catch { }
                try { _cameraViewer.ResumeDisplay(); } catch { }
            }
            catch { }
        }

        private void PauseCameras()
        {
            try
            {
                void PauseViewer(VisionImageViewer viewer)
                {
                    if (viewer == null || viewer.IsDisposed) return;

                    try { viewer.SuspendDisplay(); } catch { }

                    var cam = viewer.Camera;
                    if (cam != null)
                    {
                        try { cam.SuspendedImageDisplay = true; } catch { }
                        try { cam.StopLive(); } catch { }
                    }

                    try
                    {
                        var stopMethod = viewer.GetType().GetMethod("StopUpdateTask");
                        stopMethod?.Invoke(viewer, null);
                    }
                    catch { }
                }

                PauseViewer(_cameraViewer);
            }
            catch { }
        }

        private void ResumeCameras()
        {
            try
            {
                void ResumeViewer(VisionImageViewer viewer)
                {
                    if (viewer == null || viewer.IsDisposed) return;

                    var cam = viewer.Camera;
                    if (cam != null)
                    {
                        try { cam.SuspendedImageDisplay = false; } catch { }
                    }

                    try { viewer.ResumeDisplay(); } catch { }
                    try
                    {
                        var startMethod = viewer.GetType().GetMethod("StartUpdateTask");
                        startMethod?.Invoke(viewer, null);
                    }
                    catch { }
                }

                ResumeViewer(_cameraViewer);
            }
            catch { }
        }

        private void LoadScanMapToView()
        {
            try
            {
                if (_scanMapPoints == null || _scanMapPoints.Count == 0)
                {
                    viewScan?.SetDieList(new List<MaterialDie>());
                    return;
                }

                string info = _scanMapInfoText ?? "SCAN";

                var dies = new List<MaterialDie>(_scanMapPoints.Count);
                for (int i = 0; i < _scanMapPoints.Count; i++)
                {
                    var p = _scanMapPoints[i];
                    dies.Add(new MaterialDie
                    {
                        Index = i,
                        Name = info,
                        MapX = (int)p.X,
                        MapY = (int)p.Y,
                        Presence = MaterialPresence.Exist,
                        State = DieProcessState.Mapped
                    });
                }

                viewScan?.SetDieList(dies);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Scan Map 파일 표시 실패: " + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void LoadDownloadedMapToView()
        {
            try
            {
                if (_downloadedMapPoints == null || _downloadedMapPoints.Count == 0)
                {
                    viewDownload?.SetDieList(new List<MaterialDie>());
                    return;
                }

                string info = "";
                try { info = Path.GetFileName(_downloadedMapLoadedFromPath ?? _downloadedMapFilePath) ?? ""; }
                catch { info = ""; }

                var dies = new List<MaterialDie>(_downloadedMapPoints.Count);
                for (int i = 0; i < _downloadedMapPoints.Count; i++)
                {
                    var p = _downloadedMapPoints[i];
                    dies.Add(new MaterialDie
                    {
                        Index = i,
                        Name = info,
                        MapX = (int)p.X,
                        MapY = (int)p.Y,
                        Presence = MaterialPresence.Exist,
                        State = DieProcessState.Mapped
                    });
                }

                var scanDies = new List<MaterialDie>();
                if (_scanMapPoints != null && _scanMapPoints.Count > 0)
                {
                    for (int i = 0; i < _scanMapPoints.Count; i++)
                    {
                        var sp = _scanMapPoints[i];
                        scanDies.Add(new MaterialDie
                        {
                            Index = i,
                            Name = _scanMapInfoText ?? "SCAN",
                            MapX = (int)sp.X,
                            MapY = (int)sp.Y,
                            Presence = MaterialPresence.Exist,
                            State = DieProcessState.Mapped
                        });
                    }
                }

                viewDownload?.SetDieListOverlay(dies, scanDies);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "다운로드 Map 파일 표시 실패: " + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void btnPickScan_Click(object sender, EventArgs e)
        {
            _scanMapFilePath = string.Empty;
            EnsureScanMapLoaded(promptFileIfMissing: true);

            LoadScanMapToView();

            _pickMode = PickMode.Scan;
            btnPickScan.Enabled = false;
            btnPickDownload.Enabled = true;
        }

        private void btnPickDownload_Click(object sender, EventArgs e)
        {
            _downloadedMapFilePath = string.Empty;
            EnsureDownloadedMapLoaded(promptFileIfMissing: true);

            LoadDownloadedMapToView();

            _pickMode = PickMode.Download;
            btnPickDownload.Enabled = false;
            btnPickScan.Enabled = true;
        }

        private void ViewScan_MotorMoveRequested(object sender, QMC.Common.Controls.DisplayView_DieScanMap.DisplayItemEventArgs e)
        {
            if (_pickMode != PickMode.Scan) return;
            if (e?.Item == null) return;

            // [FIX] Map 좌표 기준으로 Pick 해야 함 (Position 쓰면 좌표계 꼬임)
            _pickedScan = new PointF(e.Item.DieMap.X, e.Item.DieMap.Y);

            _pickMode = PickMode.None;
            btnPickScan.Enabled = true;
            btnPickDownload.Enabled = true;

            UpdateHint();
        }

        private void ViewDownload_MotorMoveRequested(object sender, QMC.Common.Controls.DisplayView_DieScanMap.DisplayItemEventArgs e)
        {
            if (_pickMode != PickMode.Download) return;
            if (e?.Item == null) return;

            // [FIX] Map 좌표 기준
            _pickedDownload = new PointF(e.Item.DieMap.X, e.Item.DieMap.Y);

            _pickMode = PickMode.None;
            btnPickScan.Enabled = true;
            btnPickDownload.Enabled = true;

            UpdateHint();
        }

        private void btnAddPair_Click(object sender, EventArgs e)
        {
            if (!_pickedScan.HasValue || !_pickedDownload.HasValue)
            {
                MessageBox.Show("Scan과 Download 포인트를 각각 먼저 선택하세요.", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var pair = new MapPair(_pickedScan.Value, _pickedDownload.Value);
            _pairs.Add(pair);

            _pickedScan = null;
            _pickedDownload = null;

            RefreshPairsList();
            UpdateHint();

            // [ADD] Pair 추가 즉시 자동 Dx/Dy 추정
            RecomputeDxDyFromPairsAndUpdateUi();
        }

        private void btnRemovePair_Click(object sender, EventArgs e)
        {
            int idx = lbPairs.SelectedIndex;
            if (idx < 0 || idx >= _pairs.Count) return;
            _pairs.RemoveAt(idx);

            RefreshPairsList();
            RecomputeDxDyFromPairsAndUpdateUi();
        }

        private void btnClearPairs_Click(object sender, EventArgs e)
        {
            _pairs.Clear();
            _pickedScan = null;
            _pickedDownload = null;

            RefreshPairsList();
            UpdateHint();

            RecomputeDxDyFromPairsAndUpdateUi();
        }

        private void RefreshPairsList()
        {
            lbPairs.BeginUpdate();
            lbPairs.Items.Clear();
            for (int i = 0; i < _pairs.Count; i++)
            {
                var p = _pairs[i];
                lbPairs.Items.Add(
                    string.Format(CultureInfo.InvariantCulture,
                        "#{0}  Scan({1:0.###},{2:0.###})  ->  Down({3:0.###},{4:0.###})",
                        i + 1, p.Scan.X, p.Scan.Y, p.Download.X, p.Download.Y));
            }
            lbPairs.EndUpdate();
        }

        private void UpdateHint()
        {
            string s = "";
            if (_pickedScan.HasValue) s += $"Scan=({_pickedScan.Value.X:0.###},{_pickedScan.Value.Y:0.###})  ";
            if (_pickedDownload.HasValue) s += $"Down=({_pickedDownload.Value.X:0.###},{_pickedDownload.Value.Y:0.###})";
            if (string.IsNullOrWhiteSpace(s)) s = "Pick Scan / Pick Download로 포인트를 선택하세요.";
        }

        private static int ParseRotate(ComboBox cb)
        {
            if (cb?.SelectedItem == null) return 0;
            int r;
            if (int.TryParse(cb.SelectedItem.ToString(), out r)) return r;
            return 0;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private enum PickMode { None, Scan, Download }

        public sealed class MapPair
        {
            public PointF Scan { get; private set; }
            public PointF Download { get; private set; }

            public MapPair(PointF scan, PointF download)
            {
                Scan = scan;
                Download = download;
            }
        }

        public sealed class ManualTransformSettings
        {
            public double Dx { get; set; }
            public double Dy { get; set; }
            public int RotateDeg { get; set; }
            public bool MirrorX { get; set; }
            public bool MirrorY { get; set; }
            public List<MapPair> Pairs { get; set; }
        }

        public sealed class ManualMapMatchAppliedEventArgs : EventArgs
        {
            public ManualTransformSettings Settings { get; private set; }
            public ManualMapMatchAppliedEventArgs(ManualTransformSettings s) { Settings = s; }
        }

        public static PointF Transform(PointF scan, ManualTransformSettings s)
        {
            double x = scan.X;
            double y = scan.Y;

            if (s.MirrorX) x = -x;
            if (s.MirrorY) y = -y;

            switch (NormalizeRotate(s.RotateDeg))
            {
                case 90: { var tx = x; x = y; y = -tx; } break;
                case 180: { x = -x; y = -y; } break;
                case 270: { var tx = x; x = -y; y = tx; } break;
            }

            x += s.Dx;
            y += s.Dy;

            return new PointF((float)x, (float)y);
        }

        // [ADD] Dx/Dy 계산용: Mirror/Rotate만 적용 (Dx/Dy 제외)
        private static PointF TransformNoTranslate(PointF scan, bool mirrorX, bool mirrorY, int rotateDeg)
        {
            double x = scan.X;
            double y = scan.Y;

            if (mirrorX) x = -x;
            if (mirrorY) y = -y;

            switch (NormalizeRotate(rotateDeg))
            {
                case 90: { var tx = x; x = y; y = -tx; } break;
                case 180: { x = -x; y = -y; } break;
                case 270: { var tx = x; x = -y; y = tx; } break;
            }

            return new PointF((float)x, (float)y);
        }

        // [ADD] Pairs -> Dx/Dy 자동 추정 및 nud 반영
        private void RecomputeDxDyFromPairsAndUpdateUi()
        {
            if (_suppressAutoCalc) return;

            try
            {
                if (_pairs == null || _pairs.Count <= 0) return;

                int r = ParseRotate(cbRotate);
                bool mx = chkMirrorX.Checked;
                bool my = chkMirrorY.Checked;

                double sumDx = 0.0;
                double sumDy = 0.0;
                int n = 0;

                // Least squares: Dx/Dy는 잔차 제곱합 최소 => 각 (download - transformNoTranslate(scan)) 평균
                for (int i = 0; i < _pairs.Count; i++)
                {
                    var p = _pairs[i];
                    var scanRT = TransformNoTranslate(p.Scan, mx, my, r);

                    sumDx += (p.Download.X - scanRT.X);
                    sumDy += (p.Download.Y - scanRT.Y);
                    n++;
                }

                if (n <= 0) return;

                double dx = sumDx / n;
                double dy = sumDy / n;

                // NumericUpDown 적용(범위 내로 클램프)
                _suppressAutoCalc = true;
                try
                {
                    nudDx.Value = ClampToNumericUpDown(nudDx, dx);
                    nudDy.Value = ClampToNumericUpDown(nudDy, dy);
                }
                finally
                {
                    _suppressAutoCalc = false;
                }
            }
            catch
            {
                _suppressAutoCalc = false;
            }
        }

        // [ADD] 범위/소수자릿수 고려해 안전하게 Value로 넣기
        private static decimal ClampToNumericUpDown(NumericUpDown nud, double v)
        {
            if (nud == null) return 0m;

            decimal dv;
            try { dv = (decimal)v; }
            catch { dv = 0m; }

            if (dv < nud.Minimum) dv = nud.Minimum;
            if (dv > nud.Maximum) dv = nud.Maximum;

            // DecimalPlaces 반영(반올림)
            int dp = nud.DecimalPlaces;
            if (dp >= 0 && dp <= 10)
            {
                dv = Math.Round(dv, dp, MidpointRounding.AwayFromZero);
            }

            return dv;
        }

        private static int NormalizeRotate(int r)
        {
            r %= 360;
            if (r < 0) r += 360;
            if (r == 90 || r == 180 || r == 270) return r;
            return 0;
        }

        // 이하 EnsureDownloadedMapLoaded / EnsureScanMapLoaded 는 기존 그대로…
        // (파일 로드 파트는 이번 수정의 본질과 무관)
        private bool EnsureDownloadedMapLoaded(bool promptFileIfMissing)
        {
            // 1) 경로 없으면 파일 선택
            if (string.IsNullOrWhiteSpace(_downloadedMapFilePath) || !File.Exists(_downloadedMapFilePath))
            {
                if (!promptFileIfMissing)
                    return false;

                using (var ofd = new OpenFileDialog())
                {
                    ofd.Title = "Map File 선택";
                    ofd.Filter = "Wafer Map (*.waf)|*.waf|All Files (*.*)|*.*";
                    ofd.CheckFileExists = true;
                    ofd.CheckPathExists = true;
                    ofd.Multiselect = false;

                    // 마지막 경로 힌트
                    try
                    {
                        var hintDir = Path.GetDirectoryName(_downloadedMapFilePath);
                        if (!string.IsNullOrWhiteSpace(hintDir) && Directory.Exists(hintDir))
                            ofd.InitialDirectory = hintDir;
                    }
                    catch { }

                    if (ofd.ShowDialog(this) != DialogResult.OK)
                        return false;

                    _downloadedMapFilePath = ofd.FileName;
                }
            }

            // 2) 이미 같은 파일을 로드했고 캐시가 있으면 재사용
            if (_downloadedMapPoints != null
                && _downloadedMapPoints.Count > 0
                && string.Equals(_downloadedMapLoadedFromPath, _downloadedMapFilePath, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // 3) PerformChipMapping 처럼 ReadFile로 파싱 후 보관
            try
            {
                var wafer = new MaterialWafer();
                var diesOrg = wafer.ReadFileOnline(_downloadedMapFilePath, MaterialWafer.MapTyp.waf);

                if (diesOrg == null || diesOrg.Count == 0)
                {
                    MessageBox.Show(
                        "Map 파일을 읽었지만 데이터가 비어있습니다.\r\n" + _downloadedMapFilePath,
                        "Map File",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    _downloadedDies = null;
                    _downloadedMapPoints = null;
                    _downloadedMapLoadedFromPath = null;
                    return false;
                }

                _downloadedDies = diesOrg;
                _downloadedMapPoints = diesOrg.Select(d => new Point(d.MapX, d.MapY)).ToList();

                _downloadedMapLoadedFromPath = _downloadedMapFilePath;
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "다운로드 Map 파일 로드 실패: " + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                _downloadedMapFilePath = string.Empty;
                _downloadedDies = null;
                _downloadedMapPoints = null;
                _downloadedMapLoadedFromPath = null;
                return false;
            }
        }

        private bool EnsureScanMapLoaded(bool promptFileIfMissing)
        {
            // 1) 경로 없으면 파일 선택
            if (string.IsNullOrWhiteSpace(_scanMapFilePath) || !File.Exists(_scanMapFilePath))
            {
                if (!promptFileIfMissing)
                    return false;

                using (var ofd = new OpenFileDialog())
                {
                    ofd.Title = "Scan Map File 선택";
                    ofd.Filter = "Wafer Map (*.waf)|*.waf|All Files (*.*)|*.*";
                    ofd.CheckFileExists = true;
                    ofd.CheckPathExists = true;
                    ofd.Multiselect = false;

                    // 마지막 경로 힌트
                    try
                    {
                        var hintDir = Path.GetDirectoryName(_scanMapFilePath);
                        if (!string.IsNullOrWhiteSpace(hintDir) && Directory.Exists(hintDir))
                            ofd.InitialDirectory = hintDir;
                    }
                    catch { }

                    if (ofd.ShowDialog(this) != DialogResult.OK)
                        return false;

                    _scanMapFilePath = ofd.FileName;
                }
            }

            // 2) 동일 파일 캐시 재사용
            if (_scanMapPoints != null
                && _scanMapPoints.Count > 0
                && string.Equals(_scanMapLoadedFromPath, _scanMapFilePath, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            try
            {
                var wafer = new MaterialWafer();
                var dies = wafer.ReadFileScan(_scanMapFilePath, MaterialWafer.MapTyp.waf);

                if (dies == null || dies.Count == 0)
                {
                    MessageBox.Show(
                        "Scan Map 파일을 읽었지만 데이터가 비어있습니다.\r\n" + _scanMapFilePath,
                        "Scan Map File",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    _scanMapPoints = null;
                    _scanMapLoadedFromPath = null;
                    _scanMapInfoText = null;
                    return false;
                }

                _scanMapLoadedFromPath = _scanMapFilePath;
                try { _scanMapInfoText = Path.GetFileName(_scanMapFilePath) ?? "SCAN"; }
                catch { _scanMapInfoText = "SCAN"; }

                // [FIX] ScanMap 로드 = 장비 웨이퍼(_targetWafer.Dies) 생성/교체
                if (_targetWafer != null)
                {
                    lock (_targetWafer.Dies)
                    {
                        _targetWafer.Dies.Clear();

                        // SourceWaferId 같은 건 MaterialDie에 있을 수도 있으나,
                        // 현재 컨텍스트에선 MapX/MapY/Index/PreRank만 중요.
                        for (int i = 0; i < dies.Count; i++)
                        {
                            var d = dies[i];
                            if (d == null) continue;

                            // 필요 정보만 보존 + Name 넣기
                            d.Name = _targetWafer.WaferId ?? _scanMapInfoText ?? "SCAN";
                            _targetWafer.Dies.Add(d);
                        }
                    }

                    // view 갱신 (Auto/Manual 동일 데이터)
                    viewScan?.SetDieList(_targetWafer.Dies);

                    // [ADD] ScanMap 로드 시점 상태를 Apply 기준 원본으로 저장(누적 방지)
                    _undoCaptured = false;
                    _undoByDieIndex = null;
                    CaptureBaseSnapshot();
                }

                // 기존 캐시도 유지(미리보기/오버레이용)
                _scanMapPoints = dies.Select(p => new Point(p.MapX, p.MapY)).ToList();

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Scan Map 파일 로드 실패: " + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                _scanMapFilePath = string.Empty;
                _scanMapPoints = null;
                _scanMapLoadedFromPath = null;
                _scanMapInfoText = null;
                return false;
            }
        }

        private void CaptureUndoSnapshotIfNeeded()
        {
            if (_undoCaptured) return;
            if (_targetWafer?.Dies == null) return;

            _undoByDieIndex = new Dictionary<int, DieSnapshot>();
            lock (_targetWafer.Dies)
            {
                for (int i = 0; i < _targetWafer.Dies.Count; i++)
                {
                    var d = _targetWafer.Dies[i];
                    if (d == null) continue;

                    // Index를 키로 사용하는 이유: Apply에서 MapX/MapY가 바뀌어도 식별 가능
                    _undoByDieIndex[d.Index] = new DieSnapshot
                    {
                        MapX = d.MapX,
                        MapY = d.MapY,
                        PreRank = d.PreRank
                    };
                }
            }

            _undoCaptured = true;
        }

        private void RestoreUndoSnapshot()
        {
            if (!_undoCaptured) return;
            if (_undoByDieIndex == null) return;
            if (_targetWafer?.Dies == null) return;

            lock (_targetWafer.Dies)
            {
                for (int i = 0; i < _targetWafer.Dies.Count; i++)
                {
                    var d = _targetWafer.Dies[i];
                    if (d == null) continue;

                    if (_undoByDieIndex.TryGetValue(d.Index, out var snap))
                    {
                        d.MapX = snap.MapX;
                        d.MapY = snap.MapY;
                        d.PreRank = snap.PreRank;
                    }
                }
            }
        }

        private void CaptureBaseSnapshot()
        {
            _baseCaptured = false;
            _baseByDieIndex = null;

            if (_targetWafer?.Dies == null) return;

            var dict = new Dictionary<int, DieSnapshot>();
            lock (_targetWafer.Dies)
            {
                for (int i = 0; i < _targetWafer.Dies.Count; i++)
                {
                    var d = _targetWafer.Dies[i];
                    if (d == null) continue;

                    dict[d.Index] = new DieSnapshot
                    {
                        MapX = d.MapX,
                        MapY = d.MapY,
                        PreRank = d.PreRank
                    };
                }
            }

            _baseByDieIndex = dict;
            _baseCaptured = true;
        }

        private void RestoreBaseSnapshot()
        {
            if (!_baseCaptured) return;
            if (_baseByDieIndex == null) return;
            if (_targetWafer?.Dies == null) return;

            lock (_targetWafer.Dies)
            {
                for (int i = 0; i < _targetWafer.Dies.Count; i++)
                {
                    var d = _targetWafer.Dies[i];
                    if (d == null) continue;

                    if (_baseByDieIndex.TryGetValue(d.Index, out var snap))
                    {
                        d.MapX = snap.MapX;
                        d.MapY = snap.MapY;
                        d.PreRank = snap.PreRank;
                    }
                }
            }
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            // Pair 기반 자동계산 반영
            RecomputeDxDyFromPairsAndUpdateUi();

            // Apply(=확정) 동작
            ApplyManualMatch(previewOnly: false);

            //RecomputeDxDyFromPairsAndUpdateUi();

            //if (_targetWafer == null || _targetWafer.Dies == null || _targetWafer.Dies.Count == 0)
            //{
            //    MessageBox.Show("장비 웨이퍼(Scan) 데이터가 없습니다.\r\nInputStage 웨이퍼를 먼저 주입하세요.", "Info",
            //        MessageBoxButtons.OK, MessageBoxIcon.Information);
            //    return;
            //}
            //// [ADD] 누적 적용 방지: Apply는 항상 ScanMap 로드 직후 상태로 초기화 후 재적용
            //RestoreBaseSnapshot();

            //// [ADD] Undo 스냅샷 (Apply로 실제 Dies를 바꾸기 전에 1회 저장)
            //CaptureUndoSnapshotIfNeeded();

            //if (!EnsureDownloadedMapLoaded(promptFileIfMissing: true))
            //    return;

            //if (_downloadedDies == null || _downloadedDies.Count == 0)
            //{
            //    MessageBox.Show("Download Map 데이터가 없습니다.", "Info",
            //        MessageBoxButtons.OK, MessageBoxIcon.Information);
            //    return;
            //}

            //var settings = new ManualTransformSettings
            //{
            //    Dx = (double)nudDx.Value,
            //    Dy = (double)nudDy.Value,
            //    RotateDeg = ParseRotate(cbRotate),
            //    MirrorX = chkMirrorX.Checked,
            //    MirrorY = chkMirrorY.Checked,
            //    Pairs = _pairs.ToList()
            //};

            //// (MapX,MapY) -> BinCode(PreRank)
            //var binByPos = new Dictionary<long, int>(_downloadedDies.Count);
            //for (int i = 0; i < _downloadedDies.Count; i++)
            //{
            //    var d = _downloadedDies[i];
            //    long key = ((long)d.MapX << 32) ^ (uint)d.MapY;
            //    if (!binByPos.ContainsKey(key))
            //        binByPos.Add(key, d.PreRank);
            //}

            //// [핵심] 장비가 들고있는 실제 Dies를 직접 갱신
            //lock (_targetWafer.Dies)
            //{
            //    for (int i = 0; i < _targetWafer.Dies.Count; i++)
            //    {
            //        var die = _targetWafer.Dies[i];
            //        if (die == null) continue;

            //        var tp = Transform(new PointF(die.MapX, die.MapY), settings);

            //        int nx = (int)Math.Round(tp.X);
            //        int ny = (int)Math.Round(tp.Y);

            //        die.MapX = nx;
            //        die.MapY = ny;

            //        long tkey = ((long)nx << 32) ^ (uint)ny;
            //        if (binByPos.TryGetValue(tkey, out int preRank))
            //            die.PreRank = preRank;    // ★ Download BinCode를 1:1로 이식
            //        else
            //            die.PreRank = 0;          // 미매칭 정책(필요 시 -1 등으로 변경)
            //    }
            //}

            //// downview overlay: Download + 변환된(=실제) Scan
            //var downInfo = Path.GetFileName(_downloadedMapLoadedFromPath ?? _downloadedMapFilePath) ?? "DOWNLOAD";
            //for (int i = 0; i < _downloadedDies.Count; i++)
            //    _downloadedDies[i].Name = downInfo;

            //viewDownload?.SetDieListOverlay(_downloadedDies, _targetWafer.Dies);

            //// 외부에 알림(필요 시 저장/로그/추가 처리)
            //ManualMatchApplied?.Invoke(this, new ManualMapMatchAppliedEventArgs(settings));

            //// 정책: 지금은 확인을 위해 닫지 않음
            //// DialogResult = DialogResult.OK;
            //// Close();
        }


        private void btnUp_Click(object sender, EventArgs e)
        {
            JogSourceMap(0, -1);
        }

        private void btnDown_Click(object sender, EventArgs e)
        {
            JogSourceMap(0, +1);
        }

        private void btnLeft_Click(object sender, EventArgs e)
        {
            JogSourceMap(-1, 0);
        }

        private void btnRight_Click(object sender, EventArgs e)
        {
            JogSourceMap(+1, 0);
        }

        /// <summary>
        /// 조그: Download 뷰어에서 Overlay되는 소스맵(=변환된 Scan/TargetWafer)을 1칩 단위로 이동시키기 위해
        /// Dx/Dy를 +/-1 업데이트 후, Preview Apply 수행.
        /// </summary>
        private void JogSourceMap(int dxStep, int dyStep)
        {
            try
            {
                // nudDx/nudDy를 직접 이동(=Transform의 translate 값 변경)
                nudDx.Value = ClampToNumericUpDown(nudDx, (double)nudDx.Value + dxStep);
                nudDy.Value = ClampToNumericUpDown(nudDy, (double)nudDy.Value + dyStep);

                // 조그는 보통 "미리보기" 성격: 이벤트/외부 반영은 안하고 화면만 맞춰보게
                ApplyManualMatch(previewOnly: true);
            }
            catch { }
        }

        /// <summary>
        /// Apply 메인 진입점(확정/미리보기 공용)
        /// </summary>
        private void ApplyManualMatch(bool previewOnly)
        {
            if (!ValidateTargetWafer())
                return;

            // 누적 적용 방지: 항상 base로 복원 후 재적용
            RestoreBaseSnapshot();

            // Undo 스냅샷은 "확정 Apply" 때만 1회 저장 (미리보기에서 Undo 잡으면 UX 꼬임)
            if (!previewOnly)
                CaptureUndoSnapshotIfNeeded();

            if (!EnsureDownloadedMapLoaded(promptFileIfMissing: !previewOnly))
                return;

            if (_downloadedDies == null || _downloadedDies.Count == 0)
            {
                if (!previewOnly)
                {
                    MessageBox.Show("Download Map 데이터가 없습니다.", "Info",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                return;
            }

            var settings = BuildCurrentSettings();
            ApplyTransformToTargetWaferAndCopyPreRank(settings, _downloadedDies);
            UpdateDownloadOverlayView();

            if (!previewOnly)
            {
                ManualMatchApplied?.Invoke(this, new ManualMapMatchAppliedEventArgs(settings));
            }
        }

        private bool ValidateTargetWafer()
        {
            if (_targetWafer == null || _targetWafer.Dies == null || _targetWafer.Dies.Count == 0)
            {
                MessageBox.Show("장비 웨이퍼(Scan) 데이터가 없습니다.\r\nInputStage 웨이퍼를 먼저 주입하세요.", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            return true;
        }

        private ManualTransformSettings BuildCurrentSettings()
        {
            return new ManualTransformSettings
            {
                Dx = (double)nudDx.Value,
                Dy = (double)nudDy.Value,
                RotateDeg = ParseRotate(cbRotate),
                MirrorX = chkMirrorX.Checked,
                MirrorY = chkMirrorY.Checked,
                Pairs = _pairs.ToList()
            };
        }

        private void ApplyTransformToTargetWaferAndCopyPreRank(ManualTransformSettings settings, List<MaterialDie> downloadedDies)
        {
            // (MapX,MapY) -> BinCode(PreRank)
            var binByPos = new Dictionary<long, int>(downloadedDies.Count);
            for (int i = 0; i < downloadedDies.Count; i++)
            {
                var d = downloadedDies[i];
                long key = ((long)d.MapX << 32) ^ (uint)d.MapY;
                if (!binByPos.ContainsKey(key))
                    binByPos.Add(key, d.PreRank);
            }

            lock (_targetWafer.Dies)
            {
                for (int i = 0; i < _targetWafer.Dies.Count; i++)
                {
                    var die = _targetWafer.Dies[i];
                    if (die == null) continue;

                    var tp = Transform(new PointF(die.MapX, die.MapY), settings);

                    int nx = (int)Math.Round(tp.X);
                    int ny = (int)Math.Round(tp.Y);

                    die.MapX = nx;
                    die.MapY = ny;

                    long tkey = ((long)nx << 32) ^ (uint)ny;
                    if (binByPos.TryGetValue(tkey, out int preRank))
                        die.PreRank = preRank;
                    else
                        die.PreRank = 0;
                }
            }
        }

        private void UpdateDownloadOverlayView()
        {
            try
            {
                var downInfo = Path.GetFileName(_downloadedMapLoadedFromPath ?? _downloadedMapFilePath) ?? "DOWNLOAD";
                for (int i = 0; i < _downloadedDies.Count; i++)
                    _downloadedDies[i].Name = downInfo;

                // Download + 변환된 Scan(=target wafer)
                viewDownload?.SetDieListOverlay(_downloadedDies, _targetWafer.Dies);
            }
            catch { }
        }

    }
}
