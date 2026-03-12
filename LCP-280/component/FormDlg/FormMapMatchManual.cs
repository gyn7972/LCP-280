using QMC.Common;
using QMC.Common.Cameras.HIKVISION;
using QMC.Common.UI;
using QMC.Common.Vision;
using QMC.LCP_280.Process;
using QMC.LCP_280.Process.Unit;
using QMC.LCP_280.Process.Work;
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
        public event EventHandler<QMC.Common.Controls.DisplayView_DieScanMap.DisplayItemEventArgs> MotorMoveRequested;

        private PointF? _pickedScan;
        private PointF? _pickedDownload;
        private readonly List<MapPair> _pairs = new List<MapPair>();
        private PickMode _pickMode = PickMode.None;

        // ===== Camera (Operator_Main 방식) =====
        private VisionImageViewer _cameraViewer;
        private HIKGigECamera _camera; // Equipment.Instance.InStageCam 사용

        // ===== Downloaded Map File =====
        private string _downloadedMapFilePath;
        private List<PointD> _downloadedMapPoints;          // waf에서 읽은 원본 좌표 보관
        private string _downloadedMapLoadedFromPath;        // 어떤 파일에서 읽었는지(캐시 무효화 판단용)

        public IReadOnlyList<PointD> DownloadedMapPoints => _downloadedMapPoints;

        // ===== Scan Map File (like Download) =====
        private string _scanMapFilePath;
        private string _scanMapLoadedFromPath; // 캐시 무효화 판단용

        // ===== Scan Map Data Cache (like Download) =====
        private List<PointD> _scanMapPoints;
        private string _scanMapInfoText;

        public IReadOnlyList<PointD> ScanMapPoints => _scanMapPoints;

        // [ADD] 외부에서 로그를 남기기 위해 입력된 ID에 접근할 수 있는 프로퍼티
        public string UserId { get; private set; }

        // [ADD] 장비에서 실제 사용 중인 웨이퍼(Scan 기준 데이터)
        private MaterialWafer _targetWafer;

        // [ADD] Download 파일에서 읽은 원본 die(PreRank 포함)
        private List<MaterialDie> _downloadedDies;

        // [ADD] 자동 Dx/Dy 갱신 중에 ValueChanged 재진입 방지
        private bool _suppressAutoCalc;

        // [ADD] Undo 스냅샷
        private bool _undoCaptured;
        private Dictionary<int, DieSnapshot> _undoByDieIndex;

        private double _lastMatchRatePercent = 0.0;

        private struct DieSnapshot
        {
            public double MapX;
            public double MapY;
            public int PreRank;
        }

        // [ADD] Apply 누적 방지용 "원본" 스냅샷 (ScanMap 로드 시점 기준)
        private bool _baseCaptured;
        private Dictionary<int, DieSnapshot> _baseByDieIndex;

        // ===== [ADD] Monitoring_Main 방식 모터 이동 =====
        private InputStage _inputStage;            // 실제 MoveStage 호출용 (옵션)
        private MaterialWafer _lastInputWafer;     // CenterX/CenterY 매핑용 (옵션)

        public FormMapMatchManual()
        {
            InitializeComponent();

            try
            {
                // 이벤트 중복 연결 방지 (-= 후 +=) 및 통합 예외 처리
                this.viewScan.MotorMoveRequested -= ViewScan_MotorMoveRequested;
                this.viewScan.MotorMoveRequested += ViewScan_MotorMoveRequested;

                this.viewDownload.MotorMoveRequested -= ViewDownload_MotorMoveRequested;
                this.viewDownload.MotorMoveRequested += ViewDownload_MotorMoveRequested;

                // [ADD] 클릭만으로 Pick (DieScanMapControl에서 새로 노출한 이벤트)
                this.viewScan.ItemClicked -= ViewScan_ItemClickedPick;
                this.viewScan.ItemClicked += ViewScan_ItemClickedPick;

                this.viewDownload.ItemClicked -= ViewDownload_ItemClickedPick;
                this.viewDownload.ItemClicked += ViewDownload_ItemClickedPick;

                Shown -= FormMapMatchManual_Shown;
                Shown += FormMapMatchManual_Shown;

                FormClosing -= FormMapMatchManual_FormClosing;
                FormClosing += FormMapMatchManual_FormClosing;

                VisibleChanged -= FormMapMatchManual_VisibleChanged;
                VisibleChanged += FormMapMatchManual_VisibleChanged;

                // [ADD] 회전/미러 변경 시 자동 재계산 익명 핸들러 연결
                // (익명 함수는 -= 로 해제가 안되지만, 생성자에서 최초 1회만 실행되므로 문제 없습니다)
                if (cbRotate != null)
                    cbRotate.SelectedIndexChanged += (s, e) => RecomputeDxDyFromPairsAndUpdateUi();

                if (chkMirrorX != null)
                    chkMirrorX.CheckedChanged += (s, e) => RecomputeDxDyFromPairsAndUpdateUi();

                if (chkMirrorY != null)
                    chkMirrorY.CheckedChanged += (s, e) => RecomputeDxDyFromPairsAndUpdateUi();

                // [ADD] Monitoring_Main처럼 기본 유닛 확보 (Form이 단독으로 열린 경우에도 Move 가능)
                _inputStage = TryGetUnit<InputStage>(Equipment.UnitKeys.InputStage);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

        // [ADD] Monitoring_Main과 동일한 유닛 getter
        private static T TryGetUnit<T>(string unitName) where T : class
        {
            try
            {
                var eq = Equipment.Instance;
                if (eq?.Units != null && eq.Units.TryGetValue(unitName, out var u))
                    return u as T;
            }
            catch { }
            return null;
        }

        // [ADD] Monitoring_Main EnsureAxisReadyOrShowMessage 축 가드 (동일 호출)
        private bool EnsureAxisReadyOrShowMessage(string actionName)
        {
            try
            {
                var eq = Equipment.Instance;
                if (eq == null)
                {
                    MessageBox.Show("Equipment 인스턴스를 찾을 수 없습니다.", "오류",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                return eq.EnsureAxisReadyForAutoOrMove(actionName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "초기화 필요",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
        }

        // ===== [ADD] 1) FormMapMatchManual에서 직접 모터 이동 구현 (Monitoring_Main 참고) =====
        private void MovePickMotorTo(double stageX, double stageY)
        {
            try
            {
                if (!EnsureAxisReadyOrShowMessage("MapMatchManual.PickMove"))
                    return;

                if (_inputStage == null)
                    _inputStage = TryGetUnit<InputStage>(Equipment.UnitKeys.InputStage);

                if (_inputStage == null)
                    return;

                int rc = _inputStage.MoveStage(stageX, stageY);
                if (rc != 0)
                {
                    MessageBox.Show("모터 이동 실패(인터락 또는 축 오류)", "오류",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"모터 이동 실패: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ===== [ADD] 2) 더블클릭 이벤트 -> die 매핑 -> MoveStage 호출 (Monitoring_Main OnDieInput_MotorMoveRequested 참고) =====
        private void HandleMotorMoveRequested(QMC.Common.Controls.DisplayView_DieScanMap.DisplayItemEventArgs e)
        {
            if (e?.Item == null)
                return;

            try { MotorMoveRequested?.Invoke(this, e); } catch { }

            if (_pickMode != PickMode.None)
                return;

            var item = e.Item;

            // Download에서 클릭된 경우 -> Scan die로 치환
            if (item.GroupId == 1) // Download
            {
                // Scan wafer 기준으로 Map 좌표로 die를 다시 찾는다
                var scanDie = FindScanDieByMap(item.DieMap.X, item.DieMap.Y);
                if (scanDie != null)
                {
                    MovePickMotorTo(scanDie.CenterX, scanDie.CenterY);
                    return;
                }

                // 못 찾으면 이동하지 않거나(권장), fallback 정책을 명확히
                return;
            }

            // Scan 아이템인 경우 기존대로
            var die = FindInputDieByDisplayItem(item);
            if (die != null)
            {
                double stageX = die.CenterX;
                double stageY = die.CenterY;

                if (Math.Abs(stageX) < 0.0001 && Math.Abs(stageY) < 0.0001)
                {
                    stageX = die.MapX;
                    stageY = die.MapY;
                }

                MovePickMotorTo(stageX, stageY);
                return;
            }

            MovePickMotorTo(item.DieMap.X, item.DieMap.Y);
        }

        private MaterialDie FindScanDieByMap(int mapX, int mapY)
        {
            try
            {
                var dies = (_targetWafer ?? _lastInputWafer)?.Dies;
                if (dies == null) return null;

                return dies.FirstOrDefault(d => d != null && d.MapX == mapX && d.MapY == mapY);
            }
            catch { return null; }
        }


        //기존 코드
        //private void HandleMotorMoveRequested(QMC.Common.Controls.DisplayView_DieScanMap.DisplayItemEventArgs e)
        //{
        //    if (e == null || e.Item == null)
        //        return;

        //    // 외부(상위)에서 이미 MotorMoveRequested를 핸들링하고 싶으면 기존 이벤트도 같이 쏘되,
        //    // 여기서는 "FormMapMatchManual 자체에서" 바로 이동도 수행한다.
        //    try { MotorMoveRequested?.Invoke(this, e); } catch { }

        //    // Pick 모드이면 이동하지 않음 (Pick 우선)
        //    if (_pickMode != PickMode.None)
        //        return;

        //    // 1) 우선 Scan(=InputStage wafer)에서 die 찾아 CenterX/CenterY로 이동
        //    var die = FindInputDieByDisplayItem(e.Item);
        //    if (die != null)
        //    {
        //        double stageX = die.CenterX;
        //        double stageY = die.CenterY;

        //        // 방어: CenterX/CenterY가 비어있으면 MapX/MapY로 fallback
        //        if (Math.Abs(stageX) < 0.0001 && Math.Abs(stageY) < 0.0001)
        //        {
        //            stageX = die.MapX;
        //            stageY = die.MapY;
        //        }

        //        MovePickMotorTo(stageX, stageY);
        //        return;
        //    }

        //    // 2) 매핑 실패 시 Map 좌표로 이동(기존 Monitoring_Main fallback 동일 흐름)
        //    MovePickMotorTo(e.Item.DieMap.X, e.Item.DieMap.Y);
        //}

        // [ADD] Monitoring_Main FindInputDieByDisplayItem 방식 차용 (FormMapMatchManual은 DisplayView_DieScanMap 기반)
        private MaterialDie FindInputDieByDisplayItem(QMC.Common.Controls.DisplayView_DieScanMap.DisplayItem displayItem)
        {
            try
            {
                // 우선순위: _targetWafer(스크린/장비 Scan) -> _lastInputWafer (예비)
                var wafer = _targetWafer ?? _lastInputWafer;
                var dies = wafer?.Dies;
                if (dies == null)
                    return null;

                if (displayItem.DieId >= 0)
                {
                    var byIndex = dies.FirstOrDefault(d => d != null && d.Index == displayItem.DieId);
                    if (byIndex != null)
                        return byIndex;
                }

                // DieMap 우선으로 매칭
                double mx = displayItem.DieMap.X;
                double my = displayItem.DieMap.Y;
                var byMap = dies.FirstOrDefault(d => d != null && (int)d.MapX == mx && (int)d.MapY == my);
                if (byMap != null)
                    return byMap;

                // Position 기반 fallback (컨트롤 구현에 따라 DieMap/Position이 동일할 수 있음)
                mx = displayItem.Position.X;
                my = displayItem.Position.Y;
                byMap = dies.FirstOrDefault(d => d != null && (int)d.MapX == mx && (int)d.MapY == my);
                if (byMap != null)
                    return byMap;

                return null;
            }
            catch
            {
                return null;
            }
        }

        public void BindTargetWafer(MaterialWafer wafer)
        {
            _targetWafer = wafer;
            _lastInputWafer = wafer; // [ADD] Monitoring_Main과 동일 캐시 개념

            // Scan은 항상 장비 웨이퍼를 화면에 표시 (Auto/Manual 공용)
            try
            {
                var dies = _targetWafer?.Dies;
                if (dies != null && dies.Count > 0)
                {
                    // DieScanMapControl은 MaterialDie를 바로 받을 수 있음
                    viewScan?.SetDieList(dies);

                    // preview/overlay용 캐시도 갱신
                    _scanMapPoints = dies.Select(d => new PointD(d.MapX, d.MapY)).ToList();
                    _scanMapInfoText = _targetWafer?.WaferId ?? "SCAN";

                    // [ADD] 다이얼로그 표시 시점의 wafer 상태를 base로 캡처(누적 적용 방지)
                    _undoCaptured = false;
                    _undoByDieIndex = null;
                    CaptureBaseSnapshot();
                }
            }
            catch { }
        }

        private void ViewScan_ItemClickedPick(object sender, QMC.Common.Controls.DisplayView_DieScanMap.DisplayItemEventArgs e)
        {
            if (e?.Item == null) return;

            // Pick 모드일 때만 Pick 처리
            if (_pickMode == PickMode.Scan)
            {
                _pickedScan = new PointF(e.Item.DieMap.X, e.Item.DieMap.Y);

                _pickMode = PickMode.None;
                btnPickScan.Enabled = true;
                btnPickDownload.Enabled = true;
                UpdateHint();
                return;
            }
        }

        private void ViewDownload_ItemClickedPick(object sender, QMC.Common.Controls.DisplayView_DieScanMap.DisplayItemEventArgs e)
        {
            if (e?.Item == null) return;

            // Pick 모드일 때만 Pick 처리
            if (_pickMode == PickMode.Download)
            {
                _pickedDownload = new PointF(e.Item.DieMap.X, e.Item.DieMap.Y);

                _pickMode = PickMode.None;
                btnPickScan.Enabled = true;
                btnPickDownload.Enabled = true;
                UpdateHint();
                return;
            }
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
                // [ADD] 창이 열릴 때 ID 초기화 (이전 값 제거)
                UserId = null;

                var eq = Equipment.Instance;
                eq.UserId = null;
                
                if (textBoxUserID != null)
                    textBoxUserID.Text = string.Empty;

                EnsureCameraViewerCreated();

                if (_camera == null)
                    _camera = Equipment.Instance?.InStageCam;

                BindCamera();
                ResumeCameras();

                // [FIX] dlg Show 시 다운로드 맵 파일을 먼저 로드해서 _downloadedMapPoints 채움
                try 
                { 
                    EnsureDownloadedMapLoaded(promptFileIfMissing: false);

                    LoadDownloadedMapToView();
                    LoadScanMapToView();
                    // [ADD] 초기에도 한번 계산 시도
                    RecomputeDxDyFromPairsAndUpdateUi();
                    UpdateMatchRateUi(); // [ADD] 초기 표시 (다운로드맵이 이미 세팅된 케이스)


                    // [ADD] 창이 열릴 때 TopMost = false
                    this.TopMost = false;
                } 
                catch (Exception ex)
                {
                    Log.Write(ex);
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
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

            try
            {
                EnsureCameraViewerCreated();
                if (_camera == null) _camera = Equipment.Instance?.InStageCam;
                if (_cameraViewer?.Camera == null) BindCamera();

                ResumeCameras();
            }
            catch { }
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

                        // [FIX] PauseCameras()에서 StopLive() 했으니 재개 시 StartLive() 필요
                        try { cam.StartLive(); } catch { }
                    }

                    try { viewer.ResumeDisplay(); } catch { }

                    try
                    {
                        var startMethod = viewer.GetType().GetMethod("StartUpdateTask");
                        startMethod?.Invoke(viewer, null);
                    }
                    catch { }
                }

                // [ADD] viewer/camera가 아직 바인딩 안 된 경우 대비
                if ((_cameraViewer == null || _cameraViewer.IsDisposed) || _cameraViewer.Camera == null)
                {
                    EnsureCameraViewerCreated();

                    if (_camera == null)
                        _camera = Equipment.Instance?.InStageCam;

                    BindCamera();
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

                // [ADD] WaferId 먼저 반영
                try
                {
                    var waferId = _targetWafer?.WaferId ?? _scanMapInfoText ?? "N/A";
                    viewScan?.SetWaferId(waferId);
                }
                catch { }

                // 리스트 표시
                viewScan?.SetDieList(dies);

                // [ADD] die count는 SetDieList()가 내부에서 덮어쓸 수 있으니 마지막에 overlay 포맷으로 다시 설정
                try
                {
                    int scanCount = dies.Count(d => d != null && d.Presence == MaterialPresence.Exist);

                    // 원본(ORG)은 Download 쪽 데이터(가능하면 _downloadedDies 우선, 아니면 _downloadedMapPoints 기반)
                    int orgCount = 0;
                    if (_downloadedDies != null && _downloadedDies.Count > 0)
                        orgCount = _downloadedDies.Count(d => d != null && d.Presence == MaterialPresence.Exist);
                    else if (_downloadedMapPoints != null && _downloadedMapPoints.Count > 0)
                        orgCount = _downloadedMapPoints.Count;

                    viewScan?.SetDieCountOverlay(orgCount, scanCount);
                }
                catch { }
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

                // [ADD] viewDownload 상단 라벨에 waferId / 카운트 표시
                try
                {
                    var waferId = _targetWafer?.WaferId ?? _scanMapInfoText ?? "N/A";
                    viewDownload?.SetWaferId(waferId);

                    int orgCount = dies.Count(d => d != null && d.Presence == MaterialPresence.Exist);
                    int scanCount = scanDies.Count(d => d != null && d.Presence == MaterialPresence.Exist);
                    viewDownload?.SetDieCountOverlay(orgCount, scanCount);
                }
                catch { }

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
            //Auto / Manual 에 따라 구분 필요.
            _pickMode = PickMode.Scan;
            btnPickScan.Enabled = false;
            btnPickDownload.Enabled = true;
            UpdateHint();
            return;
            //_scanMapFilePath = string.Empty;
            //EnsureScanMapLoaded(promptFileIfMissing: true);
            //LoadScanMapToView();
            //_pickMode = PickMode.Scan;
            //btnPickScan.Enabled = false;
            //btnPickDownload.Enabled = true;
        }

        private void btnPickDownload_Click(object sender, EventArgs e)
        {
            //Auto / Manual 에 따라 구분 필요.
            _pickMode = PickMode.Download;
            btnPickDownload.Enabled = false;
            btnPickScan.Enabled = true;
            return;
            //_downloadedMapFilePath = string.Empty;
            //EnsureDownloadedMapLoaded(promptFileIfMissing: true);
            //LoadDownloadedMapToView();
            //_pickMode = PickMode.Download;
            //btnPickDownload.Enabled = false;
            //btnPickScan.Enabled = true;
        }

        private void ViewScan_MotorMoveRequested(object sender, QMC.Common.Controls.DisplayView_DieScanMap.DisplayItemEventArgs e)
        {
            if (e?.Item == null) return;

            // 1) Pick 모드면 Pick 처리
            if (_pickMode == PickMode.Scan)
            {
                _pickedScan = new PointF(e.Item.DieMap.X, e.Item.DieMap.Y);

                _pickMode = PickMode.None;
                btnPickScan.Enabled = true;
                btnPickDownload.Enabled = true;
                UpdateHint();
                return;
            }

            // [FIX] Monitoring_Main 로직 기반으로 실제 이동 수행
            JogScanMapOffset(0, 0);
            HandleMotorMoveRequested(e);
        }

        private void ViewDownload_MotorMoveRequested(object sender, QMC.Common.Controls.DisplayView_DieScanMap.DisplayItemEventArgs e)
        {
            if (e?.Item == null) return;

            // 1) Pick 모드면 Pick 처리
            if (_pickMode == PickMode.Download)
            {
                _pickedDownload = new PointF(e.Item.DieMap.X, e.Item.DieMap.Y);

                _pickMode = PickMode.None;
                btnPickScan.Enabled = true;
                btnPickDownload.Enabled = true;
                UpdateHint();
                return;
            }

            JogScanMapOffset(0, 0);
            HandleMotorMoveRequested(e);
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
            if (_suppressAutoCalc)
                return;

            try
            {
                if (_pairs == null || _pairs.Count <= 0)
                    return;

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

                if (n <= 0)
                    return;

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

        //기존코드
        //private bool EnsureDownloadedMapLoaded(bool promptFileIfMissing)
        //{
        //    // 1) 경로 없으면 파일 선택
        //    if (string.IsNullOrWhiteSpace(_downloadedMapFilePath) || !File.Exists(_downloadedMapFilePath))
        //    {
        //        if (!promptFileIfMissing)
        //            return false;

        //        using (var ofd = new OpenFileDialog())
        //        {
        //            ofd.Title = "Map File 선택";
        //            ofd.Filter = "Wafer Map (*.waf)|*.waf|All Files (*.*)|*.*";
        //            ofd.CheckFileExists = true;
        //            ofd.CheckPathExists = true;
        //            ofd.Multiselect = false;

        //            // 마지막 경로 힌트
        //            try
        //            {
        //                var hintDir = Path.GetDirectoryName(_downloadedMapFilePath);
        //                if (!string.IsNullOrWhiteSpace(hintDir) && Directory.Exists(hintDir))
        //                    ofd.InitialDirectory = hintDir;
        //            }
        //            catch { }

        //            if (ofd.ShowDialog(this) != DialogResult.OK)
        //                return false;

        //            _downloadedMapFilePath = ofd.FileName;
        //        }
        //    }

        //    // 2) 이미 같은 파일을 로드했고 캐시가 있으면 재사용
        //    if (_downloadedMapPoints != null
        //        && _downloadedMapPoints.Count > 0
        //        && string.Equals(_downloadedMapLoadedFromPath, _downloadedMapFilePath, StringComparison.OrdinalIgnoreCase))
        //    {
        //        return true;
        //    }

        //    // 3) PerformChipMapping 처럼 ReadFile로 파싱 후 보관
        //    try
        //    {
        //        var wafer = new MaterialWafer();
        //        var diesOrg = wafer.ReadFileOnline(_downloadedMapFilePath, MaterialWafer.MapTyp.waf);

        //        if (diesOrg == null || diesOrg.Count == 0)
        //        {
        //            MessageBox.Show(
        //                "Map 파일을 읽었지만 데이터가 비어있습니다.\r\n" + _downloadedMapFilePath,
        //                "Map File",
        //                MessageBoxButtons.OK,
        //                MessageBoxIcon.Information);

        //            _downloadedDies = null;
        //            _downloadedMapPoints = null;
        //            _downloadedMapLoadedFromPath = null;
        //            return false;
        //        }

        //        _downloadedDies = diesOrg;
        //        _downloadedMapPoints = diesOrg.Select(d => new Point(d.MapX, d.MapY)).ToList();

        //        _downloadedMapLoadedFromPath = _downloadedMapFilePath;
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(
        //            "다운로드 Map 파일 로드 실패: " + ex.Message,
        //            "Error",
        //            MessageBoxButtons.OK,
        //            MessageBoxIcon.Error);

        //        _downloadedMapFilePath = string.Empty;
        //        _downloadedDies = null;
        //        _downloadedMapPoints = null;
        //        _downloadedMapLoadedFromPath = null;
        //        return false;
        //    }
        //}

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
                _scanMapPoints = dies.Select(p => new PointD(p.MapX, p.MapY)).ToList();

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

        

        private void btnUp_Click(object sender, EventArgs e)
        {
            JogScanMapOffset(0, +1);
        }

        private void btnDown_Click(object sender, EventArgs e)
        {
            JogScanMapOffset(0, -1);
        }

        private void btnLeft_Click(object sender, EventArgs e)
        {
            JogScanMapOffset(+1, 0);
        }

        private void btnRight_Click(object sender, EventArgs e)
        {
            JogScanMapOffset(-1, 0);
        }

        /// <summary>
        /// 조그: Download 뷰어에서 Overlay되는 소스맵(=변환된 Scan/TargetWafer)을 1칩 단위로 이동시키기 위해
        /// Dx/Dy를 +/-1 업데이트 후, Preview Apply 수행.
        /// </summary>
        /// <summary>
        /// 조그: Download 뷰어에서 Overlay되는 소스맵(=변환된 Scan/TargetWafer)을 1칩 단위로 이동시키기 위해
        /// Dx/Dy를 +/-1 업데이트 후, Preview Apply 수행.
        /// </summary>
        private void JogScanMapOffset(int dxStep, int dyStep)
        {
            try
            {
                // nudDx/nudDy를 직접 이동(=Transform의 translate 값 변경)
                nudDx.Value = ClampToNumericUpDown(nudDx, (double)nudDx.Value + dxStep);
                nudDy.Value = ClampToNumericUpDown(nudDy, (double)nudDy.Value + dyStep);

                // [수정] 조그 이동 시에는 화면만 갱신하고(calcScore: false), 매칭율 계산은 생략하여 반응속도 향상
                ApplyManualMatch(previewOnly: true, calcScore: false);
            }
            catch { }
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            // Pair 기반 자동계산 반영
            RecomputeDxDyFromPairsAndUpdateUi();

            // Apply(=확정) 동작
            ApplyManualMatch(previewOnly: false, calcScore: true);
        }


        /// <summary>
        /// Apply 메인 진입점(확정/미리보기 공용)
        /// [수정] calcScore 파라미터 추가 (기본값 true)
        /// </summary>
        private void ApplyManualMatch(bool previewOnly, bool calcScore = true)
        {
            if (!ValidateTargetWafer())
                return;

            // 누적 적용 방지: 항상 base로 복원 후 재적용
            RestoreBaseSnapshot();

            // Undo 스냅샷은 "확정 Apply" 때만 1회 저장 (미리보기에서 Undo 잡으면 UX 꼬임)
            if (!previewOnly)
                CaptureUndoSnapshotIfNeeded();

            // PreviewOnly일 때는 파일 없으면 그냥 리턴, 확정일 때는 경고창
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
            // 좌표 변환 적용
            ApplyTransformToTargetWaferAndCopyPreRank(settings, _downloadedDies);

            // 화면(Overlay) 갱신은 항상 수행 (그래야 이동한 게 보임)
            UpdateDownloadOverlayView();

            // [수정] 요청에 따라 calcScore가 true일 때만 매칭율 계산
            if (calcScore)
            {
                UpdateMatchRateUi();
            }
            else
            {
                // 계산 안 할 때는 라벨에 상태 표시 (선택 사항)
                if (lblMatchRate != null && !lblMatchRate.IsDisposed)
                {
                    lblMatchRate.Text = "Match: ... (Press Apply)";
                    lblMatchRate.ForeColor = Color.Black;
                }
            }

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

                    var tp = Transform(new PointD(die.MapX, die.MapY), settings);

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

                // [ADD] viewScan 상단 라벨도 동기 갱신(선택)
                try
                {
                    var waferId = _targetWafer?.WaferId ?? _scanMapInfoText ?? "N/A";
                    viewScan?.SetWaferId(waferId);

                    int orgCount = _downloadedDies?.Count(d => d != null && d.Presence == MaterialPresence.Exist) ?? 0;
                    int scanCount = _targetWafer?.Dies?.Count(d => d != null && d.Presence == MaterialPresence.Exist) ?? 0;
                    viewScan?.SetDieCountOverlay(orgCount, scanCount);
                }
                catch { }

                // [ADD] viewDownload 상단 라벨 갱신(Apply/Preview 때 계속 업데이트)
                try
                {
                    var waferId = _targetWafer?.WaferId ?? _scanMapInfoText ?? "N/A";
                    viewDownload?.SetWaferId(waferId);

                    int orgCount = _downloadedDies.Count(d => d != null && d.Presence == MaterialPresence.Exist);
                    int scanCount = _targetWafer?.Dies?.Count(d => d != null && d.Presence == MaterialPresence.Exist) ?? 0;
                    viewDownload?.SetDieCountOverlay(orgCount, scanCount);
                }
                catch { }

                // Download + 변환된 Scan(=target wafer)
                viewDownload?.SetDieListOverlay(_downloadedDies, _targetWafer.Dies);
            }
            catch { }
        }

        public void SetDownloadedMapFromWaferDies(MaterialWafer wafer, string infoText = "WAFER(DIES)")
        {
            try
            {
                if (wafer?.Dies == null || wafer.Dies.Count <= 0)
                {
                    _downloadedDies = null;
                    _downloadedMapPoints = null;
                    _downloadedMapFilePath = string.Empty;
                    _downloadedMapLoadedFromPath = null;
                    return;
                }

                // 다운로드 맵 파일이 없는 상황이므로, wafer.Dies를 "원본맵"처럼 복제해서 사용
                // (ApplyTransformToTargetWaferAndCopyPreRank()는 downloadedDies의 (MapX,MapY)->PreRank 맵을 쓰므로)
                // PreRank는 없으면 0으로 유지해도 됨.
                var src = wafer.Dies.Where(d => d != null && d.Presence == MaterialPresence.Exist).ToList();

                var copy = new List<MaterialDie>(src.Count);
                for (int i = 0; i < src.Count; i++)
                {
                    var d = src[i];
                    copy.Add(new MaterialDie
                    {
                        Index = d.Index,
                        Name = infoText,
                        MapX = d.MapX,
                        MapY = d.MapY,
                        PreRank = d.PreRank,
                        Presence = MaterialPresence.Exist,
                        State = DieProcessState.Mapped
                    });
                }

                _downloadedDies = copy;
                _downloadedMapPoints = copy.Select(d => new PointD(d.MapX, d.MapY)).ToList();

                // "캐시 키" 비슷하게 문자열을 채워둠(중복 로드 방지)
                _downloadedMapFilePath = string.Empty;
                _downloadedMapLoadedFromPath = infoText;

                // 화면 즉시 갱신
                try { LoadDownloadedMapToView(); } catch { }
            }
            catch
            {
                _downloadedDies = null;
                _downloadedMapPoints = null;
                _downloadedMapFilePath = string.Empty;
                _downloadedMapLoadedFromPath = null;
            }
        }

        private bool EnsureDownloadedMapLoaded(bool promptFileIfMissing)
        {
            // [ADD] mapFile이 없는 경우(혹은 SetDownloadedMapFromWaferDies로 이미 주입된 경우)
            // _downloadedDies/_downloadedMapPoints가 있으면 파일 로드 없이 성공 처리
            if (_downloadedMapPoints != null && _downloadedMapPoints.Count > 0 &&
                _downloadedDies != null && _downloadedDies.Count > 0)
            {
                return true;
            }

            // ===== 기존 코드 그대로 =====
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
                var diesOrg = wafer.ReadFileOnline(_downloadedMapFilePath, MaterialWafer.MapTyp.txt);
                //var diesOrg = wafer.ReadFileOnline(_downloadedMapFilePath, MaterialWafer.MapTyp.waf);

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
                _downloadedMapPoints = diesOrg.Select(d => new PointD(d.MapX, d.MapY)).ToList();

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

        // [ADD] 현재 overlay 기준 매칭율 계산 (0~100)
        private double ComputeMatchRatePercent()
        {
            try
            {
                if (_downloadedDies == null || _downloadedDies.Count == 0) return 0.0;
                if (_targetWafer?.Dies == null || _targetWafer.Dies.Count == 0) return 0.0;

                // 다운로드 좌표 set
                var downSet = new HashSet<long>();
                for (int i = 0; i < _downloadedDies.Count; i++)
                {
                    var d = _downloadedDies[i];
                    if (d == null) continue;
                    long key = ((long)d.MapX << 32) ^ (uint)d.MapY;
                    downSet.Add(key);
                }

                int hit = 0;
                int total = 0;

                lock (_targetWafer.Dies)
                {
                    for (int i = 0; i < _targetWafer.Dies.Count; i++)
                    {
                        var s = _targetWafer.Dies[i];
                        if (s == null) continue;

                        long key = ((long)s.MapX << 32) ^ (uint)s.MapY;
                        if (downSet.Contains(key)) hit++;
                        total++;
                    }
                }

                if (total <= 0) return 0.0;

                //Scan기준
                //return (hit / (double)total) * 100.0;
                
                //Download Map기준
                return (hit / (double)downSet.Count) * 100.0;
            }
            catch
            {
                return 0.0;
            }
        }

        // [ADD] 매칭율 표시 + Limit 표기 + 색상
        private void UpdateMatchRateUi()
        {
            try
            {
                double limit = GetRecipeMatchLimitPercent();
                double score;

                // 1) 우선: "진짜 Map 파일 기준 채점" (파일이 있을 때)
                if (!TryComputeRealMapMatchPercent(out score))
                {
                    // 2) fallback: 파일이 없으면 화면에 로드된 리스트끼리 비교 (메모리 계산)
                    score = ComputeMatchRatePercent();
                }

                _lastMatchRatePercent = score;

                if (lblMatchRate == null || lblMatchRate.IsDisposed)
                    return;

                lblMatchRate.Text = $"Match: {_lastMatchRatePercent:0.00}% (Limit: {limit:0.00}%)";

                // 색상: limit 이상 Green, 미만 Red
                // limit=0이면(레시피 미설정) 기본색 유지
                if (limit > 0.0)
                {
                    lblMatchRate.ForeColor = (_lastMatchRatePercent >= limit)
                        ? Color.ForestGreen
                        : Color.Red;
                }
            }
            catch { }
        }

        // [ADD] OK 버튼: 확정 후 닫기 (Apply는 화면 유지)
        private void btnOk_Click(object sender, EventArgs e)
        {
            try
            {
                if(UserId == null)
                {
                    string userId = textBoxUserID.Text.Trim();
                    if (string.IsNullOrWhiteSpace(userId))
                    {
                        MessageBox.Show("User ID를 입력해주세요.", "입력 확인", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        textBoxUserID.Focus();
                        return; // 닫지 않고 중단
                    }
                    UserId = userId.Trim();

                    var eq = Equipment.Instance;
                    eq.UserId = UserId;

                    Log.Write("StartWafer", "OK", userId);
                }

                // 마지막 상태 반영
                RecomputeDxDyFromPairsAndUpdateUi();

                // OK 버튼은 당연히 최종 계산 포함
                ApplyManualMatch(previewOnly: false, calcScore: true);

                // OK로 닫음(취소/Undo 방지)
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch
            {
                // 실패 시엔 닫지 않음
            }
        }

        // =========================
        // [ADD] 레시피 MatchLimit 가져오기
        // =========================
        private double GetRecipeMatchLimitPercent()
        {
            try
            {
                var recipe = Equipment.Instance?.EquipmentRecipe?.CurrentRecipe;
                if (recipe == null) return 0.0;
                return recipe.WaferMatchLimitPercent;
            }
            catch
            {
                return 0.0;
            }
        }

        // =========================
        // [ADD] 실제 Mapmatch 점수 계산 (mapFile이 있을 때만)
        // =========================
        private bool TryComputeRealMapMatchPercent(out double percent)
        {
            percent = 0.0;
            try
            {
                // 원본 파일 경로가 없으면 리얼 계산 불가 -> 메모리 계산(ComputeMatchRatePercent)으로 fallback
                if (string.IsNullOrWhiteSpace(_downloadedMapLoadedFromPath) || !File.Exists(_downloadedMapLoadedFromPath))
                    return false;

                if (_targetWafer == null)
                    return false;

                // [FIX] Mapmatch()를 호출하면 자동으로 최적 위치를 찾아버리므로(Auto Fitting),
                //       수동으로 조작한(Jog) 현재 상태의 점수를 알 수 없습니다.
                //       따라서 "이동 없이 채점만 하는" 함수를 호출해야 합니다.

                // percent = _targetWafer.Mapmatch(_downloadedMapLoadedFromPath, MaterialWafer.MapTyp.waf); // (X) 자동 보정됨

                percent = _targetWafer.CalculateCurrentMatchScore(_downloadedMapLoadedFromPath, MaterialWafer.MapTyp.txt); // (O) 현재 점수
                //percent = _targetWafer.CalculateCurrentMatchScore(_downloadedMapLoadedFromPath, MaterialWafer.MapTyp.waf); // (O) 현재 점수

                return true;
            }
            catch
            {
                return false;
            }

            //percent = 0.0;
            //try
            //{
            //    if (_targetWafer == null) 
            //        return false;

            //    var mapFile = _downloadedMapFilePath;
            //    if (string.IsNullOrWhiteSpace(mapFile) || !File.Exists(mapFile))
            //        return false;

            //    // NOTE: Mapmatch는 내부에서 BinX/BinY 업데이트 등의 부작용 가능성이 있음.
            //    //       현 요구는 "진짜 점수" 표기이므로 호출 허용.
            //    double s = _targetWafer.Mapmatch(mapFile, MaterialWafer.MapTyp.waf);
            //    //double s = _targetWafer.MapmatchFast(mapFile, MaterialWafer.MapTyp.waf);
            //    percent = s * 100.0;

            //    _lastRealMapMatchPercent = percent;
            //    _lastRealMapMatchFile = mapFile;
            //    _lastRealMapMatchTime = DateTime.Now;

            //    return true;
            //}
            //catch
            //{
            //    return false;
            //}
        }

        private void btnSelectLogin_Click(object sender, EventArgs e)
        {
            try
            {
                // AccountManager가 로드되지 않았을 경우를 대비해 로드 시도
                if (QMC.Common.Account.AccountManager.Accounts.Count == 0)
                {
                    QMC.Common.Account.AccountManager.Load();
                }

                var accounts = QMC.Common.Account.AccountManager.Accounts;

                if (accounts == null || accounts.Count == 0)
                {
                    MessageBox.Show("등록된 계정이 없습니다.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // ContextMenu 생성
                ContextMenuStrip menu = new ContextMenuStrip();

                foreach (var acc in accounts)
                {
                    // 각 계정에 대한 메뉴 아이템 생성
                    ToolStripMenuItem item = new ToolStripMenuItem(acc.UserID);
                    item.Tag = acc.UserID; // Tag에 ID 저장
                    item.Click += (s, args) =>
                    {
                        // 메뉴 아이템 클릭 시 텍스트박스에 ID 반영
                        if (textBoxUserID != null)
                        {
                            textBoxUserID.Text = acc.UserID;
                        }
                    };
                    menu.Items.Add(item);
                }

                // 버튼 바로 아래에 메뉴 표시
                menu.Show(btnSelectLogin, new Point(0, btnSelectLogin.Height));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"계정 목록을 불러오는 중 오류가 발생했습니다: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
