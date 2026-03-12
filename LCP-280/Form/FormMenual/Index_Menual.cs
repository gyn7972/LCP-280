using QMC.Common;
using QMC.Common.Component;
using QMC.Common.PKGTester;
using QMC.Common.UI;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Component; // DIO / teaching controls
using QMC.LCP_280.Process.Unit.FormSetup;
using QMC.LCP_280.Process.Unit.FormWork.Repro;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation.Peers;
using System.Windows.Forms;
using static QMC.Common.FormMenual;
using static QMC.Common.Material;
using static QMC.LCP_280.Process.Unit.RotaryConfig.IO;
using Timer = System.Windows.Forms.Timer;

namespace QMC.LCP_280.Process.Unit.FormWork
{
    [FormOrder(3)]
    public partial class Index_Menual : Form, ITabActivationAware
    {
        private Equipment Equipment => Equipment.Instance;

        private IndexChipProbeController IndexChipProbeController;
        private IndexChipProber IndexChipProber;
        private IndexLoadAligner IndexLoadAligner;
        private IndexUnloadAligner IndexUnloadAligner;
        private Rotary Rotary;

        // 추가 유닛 (재현성 시퀀스용)
        private InputDieTransfer _inputDieTransfer;
        private InputStage _inputStage;
        private PKGTester _tester;

        private bool _initialized;
        private bool _deferredInitDone; // 지연 바인딩 여부
        private bool _preloadRequested;

        private ManualSeqReproTestRunner _manualReproTestRunner;


        #region 재현성 테스트 필드
        private readonly object _reproLock = new object();
        #endregion

        private volatile bool _pendingTeachingReload;



        #region Constructors
        public Index_Menual() : this(
            TryGetUnit<IndexChipProbeController>("IndexChipProbeController"),
            TryGetUnit<IndexChipProber>("IndexChipProber"),
            TryGetUnit<IndexLoadAligner>("IndexLoadAligner"),
            TryGetUnit<IndexUnloadAligner>("IndexUnloadAligner"),
            TryGetUnit<Rotary>("Rotary"))
        { }

        public Index_Menual(IndexChipProbeController probeController,
                               IndexChipProber prober,
                               IndexLoadAligner loadAligner,
                               IndexUnloadAligner unloadAligner,
                               Rotary rotary)
        {
            InitializeComponent();
            IndexChipProbeController = probeController;
            IndexChipProber = prober;
            IndexLoadAligner = loadAligner;
            IndexUnloadAligner = unloadAligner;
            Rotary = rotary;

            // 재현성 시퀀스 관련 유닛 바인딩
            _inputDieTransfer = TryGetUnit<InputDieTransfer>("InputDieTransfer");
            _inputStage = TryGetUnit<InputStage>("InputStage");
            _tester = Equipment?.Tester;

            Load += Process_Working_Load;

            _ProcessCameraviewer.LightControlRequested += LightControlRequested;

            // 재현성 테스트 러너 초기화
            _manualReproTestRunner = new ManualSeqReproTestRunner(Rotary, 
                _inputDieTransfer, 
                _inputStage, 
                IndexChipProbeController, 
                IndexLoadAligner, 
                Equipment?.Tester);

            _manualReproTestRunner.RunningChanged += on => 
            BeginInvoke(new Action(() => 
            {
                ButtonManualTest.Text = on ? "IndexCal Stop" : "IndexCal Start";
            }));
            _manualReproTestRunner.Message += msg => Log.Write("ReproTest", msg);


            SubscribeRecipeChanged();
            // ★ [ADD] UI가 다시 보이거나 활성화될 때 pending teaching reload 처리
            this.VisibleChanged += (s, e) =>
            {
                if (!Visible)
                    return;

                if (_pendingTeachingReload)
                {
                    _pendingTeachingReload = false;
                    try { BindTeachingPositions(); } catch { }
                }
            };
            this.Activated += (s, e) =>
            {
                if (_pendingTeachingReload)
                {
                    _pendingTeachingReload = false;
                    try { BindTeachingPositions(); } catch { }
                }
            };

        }
        #endregion


        // [추가] 탭 활성화 인터페이스 구현
        public void OnActivatedInTab()
        {
            ResumeCamera();
        }

        public void OnDeactivatedInTab()
        {
            PauseCamera();
        }

        // [추가] 폼 이벤트 오버라이드
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            try
            {
                if (IsDisposed || Disposing) return;
                if (this.Visible) ResumeCamera();
                else PauseCamera();
            }
            catch { }
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            try { ResumeCamera(); } catch { }
        }

        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);
            try { PauseCamera(); } catch { }
        }

        private void Index_Menual_FormClosing(object sender, FormClosingEventArgs e)
        {
            try { PauseCamera(); } catch { }
            UnsubscribeRecipeChanged();
        }

        // [추가] 카메라 제어 헬퍼 (_ProcessCameraviewer 사용)
        private void PauseCamera()
        {
            if (_ProcessCameraviewer == null || _ProcessCameraviewer.IsDisposed) return;

            try { _ProcessCameraviewer.SuspendDisplay(); } catch { }

            var cam = _ProcessCameraviewer.Camera;
            if (cam != null)
            {
                try
                {
                    cam.SuspendedImageDisplay = true;
                    cam.StopLive();
                }
                catch { }
            }

            try
            {
                var method = _ProcessCameraviewer.GetType().GetMethod("StopUpdateTask");
                if (method != null) method.Invoke(_ProcessCameraviewer, null);
            }
            catch { }
        }

        private void ResumeCamera()
        {
            if (_ProcessCameraviewer == null || _ProcessCameraviewer.IsDisposed) return;

            var cam = _ProcessCameraviewer.Camera;
            if (cam != null)
            {
                try { cam.SuspendedImageDisplay = false; } catch { }
                try { cam.StartLive(); } catch { }
            }

            try { _ProcessCameraviewer.ResumeDisplay(); } catch { }

            try
            {
                var method = _ProcessCameraviewer.GetType().GetMethod("StartUpdateTask");
                if (method != null) method.Invoke(_ProcessCameraviewer, null);
                else _ProcessCameraviewer.StartUpdateTask();
            }
            catch { }
        }

        public void PreloadUI()
        {
            if (IsDisposed || Disposing) return;
            if (_preloadRequested) return;
            _preloadRequested = true;
            EnsureInitialized();
            var h = Handle;
        }

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

        private void Process_Working_Load(object sender, EventArgs e)
        {
            EnsureInitialized();
        }

        #region Camera
        private void BindCamera()
        {
            try
            {
                if (_ProcessCameraviewer != null && IndexUnloadAligner?.IndexOutCamera!= null)
                {
                    if (_ProcessCameraviewer.Camera != IndexUnloadAligner.IndexOutCamera)
                        _ProcessCameraviewer.Camera = IndexUnloadAligner.IndexOutCamera;
                    try { IndexUnloadAligner.IndexOutCamera.StartLive(); } catch { }
                    try { _ProcessCameraviewer.StartUpdateTask(); } catch { }
                }
            }
            catch { }
        }
        #endregion

        #region Sequences
        private void InitSequences()
        {
            try
            {
                // 최신 Equipment 등록본으로 다시 참조 갱신 (폼 생성 후 재초기화 상황 대비)
                Rotary = TryGetUnit<Rotary>("Rotary");
                IndexLoadAligner = TryGetUnit<IndexLoadAligner>("IndexLoadAligner");
                IndexChipProbeController = TryGetUnit<IndexChipProbeController>("IndexChipProbeController");
                IndexChipProber = TryGetUnit<IndexChipProber>("IndexChipProber");
                IndexUnloadAligner = TryGetUnit<IndexUnloadAligner>("IndexUnloadAligner");

                if (Rotary != null)
                {
                    manualSequenceControlProcessSeq.ParentUnit = Rotary; // 시퀀스 등록 대상 유닛 지정
                }

                if (IndexLoadAligner != null)
                {
                    manualSequenceControl.ParentUnit = IndexLoadAligner; // 시퀀스 등록 대상 유닛 지정
                }
                if(IndexChipProbeController != null)
                {
                    manualSequenceControlProbe.ParentUnit = IndexChipProbeController;
                }
                if(IndexUnloadAligner != null)
                {
                    manualSequenceControlOutAlign.ParentUnit = IndexUnloadAligner;
                }

            }
            catch { }
        }
        #endregion

        private void EnsureInitialized()
        {
            if (_initialized) return;
            _initialized = true;
            try
            {
                Text = "Index Menual";
                BeginInvoke(new Action(StartDeferredInit));
            }
            catch (Exception ex)
            {
                try { Controls.Add(new Label { Dock = DockStyle.Fill, Text = $"Init 실패: {ex.Message}", ForeColor = System.Drawing.Color.Red, TextAlign = System.Drawing.ContentAlignment.MiddleCenter }); } catch { }
            }
        }

        private async void StartDeferredInit()
        {
            if (_deferredInitDone) 
                return;

            _deferredInitDone = true;
            await Task.Delay(30);

            if (IsDisposed || Disposing) 
                return;

            try
            {
                InitSequences();
                InitSocketIndexCombo();

                // ★ [ADD] 최초 진입 시 TeachingRecipe 강제 로드/기본값 보강
                try
                {
                    if (IndexChipProbeController != null)
                    {
                        // Config + TeachingRecipe 로드 및 축 바인딩
                        IndexChipProbeController.Config.LoadAndBindAxes(Equipment.Instance.AxisManager);

                        // TeachingPositions 비어있으면 기본 생성(파일 저장까지)
                        IndexChipProbeController.Config.InitializeDefaultTeachingPositions();

                        // 캐시 갱신(혹시 이전 캐시가 꼬였을 경우)
                        IndexChipProbeController.Config.InvalidateTeachingRecipeCache();
                    }

                    if (IndexLoadAligner != null)
                    {
                        // Config + TeachingRecipe 로드 및 축 바인딩
                        IndexLoadAligner.Config.LoadAndBindAxes(Equipment.Instance.AxisManager);

                        // TeachingPositions 비어있으면 기본 생성(파일 저장까지)
                        IndexLoadAligner.Config.InitializeDefaultTeachingPositions();

                        // 캐시 갱신(혹시 이전 캐시가 꼬였을 경우)
                        IndexLoadAligner.Config.InvalidateTeachingRecipeCache();
                    }
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }

                BindTeachingPositions();
                BindDioControls();
                BindCamera();
                SetupStatusTimer();
            }
            catch { }
        }

        private Timer _statusTimer;
        private int _statusTimerBusy; // 재진입 방지 플래그
        private int _lastProbeIndexNo = -1;
        private int _lastLoadIndexNo = -1;
        private int _lastAlignIndexNo = -1;
        private void SetupStatusTimer()
        {
            try
            {
                if (_statusTimer != null) return;
                _statusTimer = new Timer();
                _statusTimer.Interval = 500; // ms 단위 (필요 시 조정)
                _statusTimer.Tick += StatusTimer_Tick;
                _statusTimer.Start();
            }
            catch { }
        }

        // ====== 추가: Tick 이벤트 핸들러 ======
        private void StatusTimer_Tick(object sender, EventArgs e)
        {
            if (IsDisposed || Disposing) 
                return;

            if (Interlocked.Exchange(ref _statusTimerBusy, 1) == 1) 
                return; // 재진입 방지

            try
            {
                // ===== 0-based index 추출 (변경 감지용) =====
                int loadIndex0 = -1;
                int alignIndex0 = -1;
                int probeIndex0 = -1;
                int unloadIndex0 = -1;

                try { if (Rotary != null) loadIndex0 = Rotary.GetLoadIndexNo(); } catch { }
                try { if (IndexLoadAligner != null) alignIndex0 = IndexLoadAligner.GetAlignIndexNo(); } catch { }
                try { if (IndexChipProbeController != null) probeIndex0 = IndexChipProbeController.GetProbeIndexNo(); } catch { }
                try { if (IndexUnloadAligner != null) unloadIndex0 = IndexUnloadAligner.GetUnloaderAlignIndexNo(); } catch { }

                try
                {
                    int nIndexNo;
                    string socketNoText;

                    if (labelsocketNumberInput != null)
                    {
                        nIndexNo = loadIndex0 + 1;
                        socketNoText = (nIndexNo > 0) ? nIndexNo.ToString() : "---";
                        labelsocketNumberInput.Text = socketNoText;
                    }

                    if (labelsocketNumberLAlign != null)
                    {
                        nIndexNo = alignIndex0 + 1;
                        socketNoText = (nIndexNo > 0) ? nIndexNo.ToString() : "---";
                        labelsocketNumberLAlign.Text = socketNoText;
                    }

                    if (labelsocketNumberProbe != null)
                    {
                        nIndexNo = probeIndex0 + 1;
                        socketNoText = (nIndexNo > 0) ? nIndexNo.ToString() : "---";
                        labelsocketNumberProbe.Text = socketNoText;
                    }

                    if (labelsocketNumberUnload != null)
                    {
                        nIndexNo = unloadIndex0 + 1;
                        socketNoText = (nIndexNo > 0) ? nIndexNo.ToString() : "---";
                        labelsocketNumberUnload.Text = socketNoText;
                    }
                }
                catch { }

                // ===== 인덱스 변경 감지 → TeachingPosition alias 갱신 =====
                // alias 표시의 핵심은 probeIndex0 이므로 최소 그 변화는 감지 필요.
                bool needTeachingRefresh =
                    (probeIndex0 >= 0 && probeIndex0 != _lastProbeIndexNo) ||
                    (loadIndex0 >= 0 && loadIndex0 != _lastLoadIndexNo) ||
                    (alignIndex0 >= 0 && alignIndex0 != _lastAlignIndexNo); // [ADD]


                if (needTeachingRefresh)
                {
                    _lastProbeIndexNo = probeIndex0;
                    _lastLoadIndexNo = loadIndex0;
                    _lastAlignIndexNo = alignIndex0;

                    if (Visible && IsHandleCreated)
                    {
                        // provider가 다시 evaluate되어 alias(Top_Index_Contact 등) 값이 현재 index 기준으로 갱신됨
                        try { BindTeachingPositions(); } catch { }
                    }
                    else
                    {
                        _pendingTeachingReload = true;
                    }
                }
            }
            finally
            {
                Interlocked.Exchange(ref _statusTimerBusy, 0);
            }
        }


        private void SubscribeRecipeChanged()
        {
            EquipmentRecipe.CurrentRecipeChanged -= EquipmentRecipe_CurrentRecipeChanged;
            EquipmentRecipe.CurrentRecipeChanged += EquipmentRecipe_CurrentRecipeChanged;
        }

        private void UnsubscribeRecipeChanged()
        {
            EquipmentRecipe.CurrentRecipeChanged -= EquipmentRecipe_CurrentRecipeChanged;
        }

        private void EquipmentRecipe_CurrentRecipeChanged(object sender, EquipmentRecipe.MeasurementRecipeChangedEventArgs e)
        {
            try
            {
                if (!IsHandleCreated)
                {
                    _pendingTeachingReload = true;
                    return;
                }

                if (InvokeRequired)
                {
                    BeginInvoke(new Action(() => EquipmentRecipe_CurrentRecipeChanged(sender, e)));
                    return;
                }

                if (!Visible)
                {
                    _pendingTeachingReload = true;
                    return;
                }

                // 레시피 변경 → 유닛 티칭(Recipe cache 등) 갱신 + TeachingPositionControl 재바인딩/리로드
                try { IndexChipProbeController?.Config?.InvalidateTeachingRecipeCache(); } catch { }
                try { IndexLoadAligner?.Config?.InvalidateTeachingRecipeCache(); } catch { } // [ADD]

                BindTeachingPositions();
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

        #region Teaching Positions
        private void BindTeachingPositions()
        {
            try
            {
                if (teachingPositionControl == null) 
                    return;

                teachingPositionControl.ClearUnits();

                if (IndexLoadAligner != null)
                {
                    const string TP_ALIGNZ_UP_ALIAS = "AlignZ_Index_Contact";
                    const string TP_ALIGNZ_READY_ALIAS = "AlignZ_Index_Ready";

                    teachingPositionControl.RegisterUnit(
                        "IndexLoadAligner",
                        IndexLoadAligner,

                        // 1) Provider: Index1~8 Up/Ready 숨기고 alias 2개만 추가
                        () =>
                        {
                            var recipe = IndexLoadAligner.Config?.TeachingRecipe;
                            var list = recipe?.TeachingPositions;
                            if (list == null)
                                return null;

                            var hidden = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                            for (int i = 1; i <= 8; i++)
                            {
                                hidden.Add($"AlignZ_Index{i}_Contact");
                                hidden.Add($"AlignZ_Index{i}_Ready");
                            }

                            var result = new List<TeachingPosition>();

                            foreach (var tp in list)
                            {
                                if (tp == null || string.IsNullOrWhiteSpace(tp.Name))
                                    continue;

                                if (hidden.Contains(tp.Name))
                                    continue;

                                result.Add(tp);
                            }

                            TeachingPosition CloneForDisplay(string aliasName, TeachingPosition src, IReadOnlyList<string> axisNames, string desc)
                            {
                                var axisPositions = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
                                foreach (var a in axisNames)
                                {
                                    double v = 0.0;
                                    if (src != null && src.AxisPositions != null)
                                        src.AxisPositions.TryGetValue(a, out v);
                                    axisPositions[a] = v;
                                }
                                return new TeachingPosition(aliasName, axisPositions, desc);
                            }

                            TeachingPosition BuildAlias(string aliasName, IReadOnlyList<string> axisNames, string desc)
                            {
                                TeachingPosition src = null;

                                try
                                {
                                    int idx0 = IndexLoadAligner.GetAlignIndexNo(); // 0..7
                                    int idx1 = idx0 + 1;

                                    string realName = null;
                                    if (string.Equals(aliasName, TP_ALIGNZ_UP_ALIAS, StringComparison.OrdinalIgnoreCase))
                                        realName = $"AlignZ_Index{idx1}_Contact";
                                    else if (string.Equals(aliasName, TP_ALIGNZ_READY_ALIAS, StringComparison.OrdinalIgnoreCase))
                                        realName = $"AlignZ_Index{idx1}_Ready";

                                    if (!string.IsNullOrWhiteSpace(realName))
                                    {
                                        src = list.FirstOrDefault(t => t != null &&
                                            string.Equals(t.Name, realName, StringComparison.OrdinalIgnoreCase));
                                    }
                                }
                                catch { /* ignore */ }

                                return CloneForDisplay(aliasName, src, axisNames, desc);
                            }

                            // alias 2개 추가 (현재 index의 실제 값 표시)
                            result.Add(BuildAlias(
                                TP_ALIGNZ_UP_ALIAS,
                                new[] { AxisNames.IndexZ },
                                "Alias: AlignZ Up (current index)"));

                            result.Add(BuildAlias(
                                TP_ALIGNZ_READY_ALIAS,
                                new[] { AxisNames.IndexZ },
                                "Alias: AlignZ Ready (current index)"));

                            return result;
                        },

                        // 2) MoveAction: alias면 현재 index에 맞는 실제 티칭명으로 변환 후 이동
                        (name, vel) =>
                        {
                            if (string.IsNullOrWhiteSpace(name))
                                return;

                            string realName = name;

                            try
                            {
                                int idx0 = IndexLoadAligner.GetAlignIndexNo(); // 0..7
                                int idx1 = idx0 + 1;

                                if (string.Equals(name, TP_ALIGNZ_UP_ALIAS, StringComparison.OrdinalIgnoreCase))
                                    realName = $"AlignZ_Index{idx1}_Contact";
                                else if (string.Equals(name, TP_ALIGNZ_READY_ALIAS, StringComparison.OrdinalIgnoreCase))
                                    realName = $"AlignZ_Index{idx1}_Ready";
                            }
                            catch
                            {
                                realName = name; // fallback
                            }

                            IndexLoadAligner.MoveToTeachingPosition(realName, false);
                        },

                        // 3) SaveAction: Recipe로 저장
                        tp =>
                        {
                            try
                            {
                                var recipe = IndexLoadAligner.Config?.TeachingRecipe;
                                recipe?.UpsertFiltered(tp, save: true);
                            }
                            catch (Exception ex)
                            {
                                Log.Write(ex);
                            }
                        },

                        autoReload: false);
                }

                if (IndexUnloadAligner != null)
                {
                    //teachingPositionControl.RegisterUnit(
                    //    "IndexUnloadAligner",
                    //    IndexUnloadAligner,
                    //    () => IndexUnloadAligner.Config?.TeachingPositions,
                    //    (name, vel) => IndexUnloadAligner.MoveToTeachingPosition(name, vel: vel),
                    //    tp => IndexUnloadAligner.Config?.SetTeachingPosition(tp),
                    //    autoReload: false);
                }

                if (IndexChipProber != null)
                {
                    //teachingPositionControl.RegisterUnit(
                    //    "IndexChipProber",
                    //    IndexChipProber,
                    //    () => IndexChipProber.Config?.TeachingPositions,
                    //    (name, vel) => IndexChipProber.MoveToTeachingPosition(name, vel: vel),
                    //    tp => IndexChipProber.Config?.SetTeachingPosition(tp),
                    //    autoReload: false);
                }

                if (IndexChipProbeController != null)
                {
                    const string TP_TOP_CONTACT_ALIAS = "Top_Index_Contact";
                    const string TP_TOP_READY_ALIAS = "Top_Index_Ready";
                    const string TP_BOTTOM_CONTACT_ALIAS = "Bottom_Index_Contact";
                    const string TP_BOTTOM_READY_ALIAS = "Bottom_Index_Ready";

                    teachingPositionControl.RegisterUnit(
                        "IndexChipProbeController",
                        IndexChipProbeController,

                        // 1) Provider: index1~8 개별 항목은 숨기고, alias(그룹) 4개만 추가해서 노출
                        () =>
                        {
                            var list = IndexChipProbeController.TeachingPositions;
                            if (list == null)
                                return null;

                            // 숨길 항목(Top/Bottom + Index1~8 + Contact/Ready)
                            var hidden = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                            for (int i = 1; i <= 8; i++)
                            {
                                hidden.Add($"Top_Index{i}_Contact");
                                hidden.Add($"Top_Index{i}_Ready");
                                hidden.Add($"Bottom_Index{i}_Contact");
                                hidden.Add($"Bottom_Index{i}_Ready");
                            }

                            var result = new List<TeachingPosition>();

                            // 기존 TeachingPosition 중 숨김 대상 제외하고 그대로 추가
                            foreach (var tp in list)
                            {
                                if (tp == null || string.IsNullOrWhiteSpace(tp.Name))
                                    continue;

                                if (hidden.Contains(tp.Name))
                                    continue;

                                result.Add(tp);
                            }

                            TeachingPosition CloneForDisplay(string aliasName, TeachingPosition src, IReadOnlyList<string> axisNames, string desc)
                            {
                                var axisPositions = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

                                // src가 있으면 src값을 복사, 없으면 0.0
                                foreach (var a in axisNames)
                                {
                                    double v = 0.0;
                                    if (src != null && src.AxisPositions != null)
                                        src.AxisPositions.TryGetValue(a, out v);

                                    axisPositions[a] = v;
                                }

                                return new TeachingPosition(aliasName, axisPositions, desc);
                            }

                            bool TryResolveRealTpName(string aliasName, out string realName)
                            {
                                realName = null;

                                try
                                {
                                    var recipe = IndexChipProbeController.Config?.TeachingRecipe;
                                    if (recipe == null) return false;

                                    int idx0 = IndexChipProbeController.GetProbeIndexNo(); // 0..7

                                    if (string.Equals(aliasName, TP_TOP_CONTACT_ALIAS, StringComparison.OrdinalIgnoreCase))
                                        realName = recipe.GetTopContactName(idx0);
                                    else if (string.Equals(aliasName, TP_TOP_READY_ALIAS, StringComparison.OrdinalIgnoreCase))
                                        realName = recipe.GetTopReadyName(idx0);
                                    else if (string.Equals(aliasName, TP_BOTTOM_CONTACT_ALIAS, StringComparison.OrdinalIgnoreCase))
                                        realName = recipe.GetBottomContactName(idx0);
                                    else if (string.Equals(aliasName, TP_BOTTOM_READY_ALIAS, StringComparison.OrdinalIgnoreCase))
                                        realName = recipe.GetBottomReadyName(idx0);

                                    return !string.IsNullOrWhiteSpace(realName);
                                }
                                catch (Exception ex)
                                {
                                    Log.Write(ex);
                                    return false;
                                }
                            }

                            // 현재 list(실제 TeachingPositions)에서 real tp를 찾아 alias tp를 만들어 추가
                            TeachingPosition BuildAlias(string aliasName, IReadOnlyList<string> axisNames, string desc)
                            {
                                string realName;
                                TeachingPosition src = null;

                                if (TryResolveRealTpName(aliasName, out realName))
                                {
                                    src = list.FirstOrDefault(t => t != null &&
                                        string.Equals(t.Name, realName, StringComparison.OrdinalIgnoreCase));
                                }

                                return CloneForDisplay(aliasName, src, axisNames, desc);
                            }

                            // alias 항목 추가 (표시/선택 전용) - ★ 현재 index의 실제 티칭값이 표시되게 함
                            result.Add(BuildAlias(
                                TP_TOP_CONTACT_ALIAS,
                                new[] { AxisNames.ProbeZ },
                                "Alias: Top Index Contact (current index)"));

                            result.Add(BuildAlias(
                                TP_TOP_READY_ALIAS,
                                new[] { AxisNames.ProbeZ },
                                "Alias: Top Index Ready (current index)"));

                            result.Add(BuildAlias(
                                TP_BOTTOM_CONTACT_ALIAS,
                                new[] { AxisNames.ProbeCardX, AxisNames.ProbeCardY, AxisNames.ProbeCardZ },
                                "Alias: Bottom Index Contact (current index)"));

                            result.Add(BuildAlias(
                                TP_BOTTOM_READY_ALIAS,
                                new[] { AxisNames.ProbeCardX, AxisNames.ProbeCardY, AxisNames.ProbeCardZ },
                                "Alias: Bottom Index Ready (current index)"));

                            return result;
                        },

                        // 2) MoveAction: alias면 현재 index에 맞는 실제 티칭명으로 변환 후 이동
                        (name, vel) =>
                        {
                            if (string.IsNullOrWhiteSpace(name))
                                return;

                            string realName = name;

                            try
                            {
                                var recipe = IndexChipProbeController.Config?.TeachingRecipe;
                                if (recipe != null)
                                {
                                    int idx0 = IndexChipProbeController.GetProbeIndexNo(); // 0..7

                                    if (string.Equals(name, TP_TOP_CONTACT_ALIAS, StringComparison.OrdinalIgnoreCase))
                                        realName = recipe.GetTopContactName(idx0);
                                    else if (string.Equals(name, TP_TOP_READY_ALIAS, StringComparison.OrdinalIgnoreCase))
                                        realName = recipe.GetTopReadyName(idx0);
                                    else if (string.Equals(name, TP_BOTTOM_CONTACT_ALIAS, StringComparison.OrdinalIgnoreCase))
                                        realName = recipe.GetBottomContactName(idx0);
                                    else if (string.Equals(name, TP_BOTTOM_READY_ALIAS, StringComparison.OrdinalIgnoreCase))
                                        realName = recipe.GetBottomReadyName(idx0);
                                }
                            }
                            catch (Exception ex)
                            {
                                // recipe.GetXXXName은 잘못된 index면 예외를 던질 수 있으므로 방어
                                Log.Write(ex);
                                realName = name; // fallback
                            }

                            IndexChipProbeController.MoveToTeachingPosition(realName, false);
                        },

                        // saveAction은 여기선 의미 없음(저장 UI를 꺼둠)
                        tp => IndexChipProbeController.Config?.SetTeachingPosition(tp),

                        autoReload: false);
                }


                if (Rotary != null)
                {
                    teachingPositionControl.RegisterUnit(
                        "Rotary",
                        Rotary,
                        () => Rotary.Config?.TeachingPositions,
                        (name, vel) => Rotary.MoveToTeachingPosition(name, vel: vel),
                        tp => Rotary.Config?.SetTeachingPosition(tp),
                        autoReload: false);
                }

                teachingPositionControl.SetUnitAbbreviation("IndexLoadAligner", "MAlign", reload: false);
                teachingPositionControl.SetUnitAbbreviation("IndexChipProber", "Probe", reload: false);
                teachingPositionControl.SetUnitAbbreviation("IndexChipProbeController", "Probe", reload: false);
                teachingPositionControl.SetUnitAbbreviation("Rotary", "Index", reload: false);


                teachingPositionControl.SetSaveCancelVisible(false, false);
                teachingPositionControl.RefreshData();
            }
            catch { }
        }
        #endregion

        #region DIO Binding / Reflection Helpers
        private void BindDioControls()
        {
            if (dioControl == null) return;

            dioControl.IoSortMode = QMC.LCP_280.Process.Component.DIOControl.SortingMode.Insertion;

            try
            {
                BindRotaryActuators(Rotary);

                if (IndexChipProbeController != null)
                {
                    dioControl.BindDIOInput(() => false, "---- ProbeController ----", "Probe");
                    dioControl.BindDIOInput(() => IndexChipProbeController.ProbeVacOk(), "ProbeVac OK", "ProbeVacOk");
                    dioControl.BindDIOInput(() => IndexChipProbeController.IsSphereForward(), "Sphere FW Sns", "ProbeSphereFwSns");
                    dioControl.BindDIOInput(() => IndexChipProbeController.IsSphereBackward(), "Sphere BW Sns", "ProbeSphereBwSns");

                    dioControl.BindDIOOutput(
                        () => IndexChipProbeController.SetContectTop(true),
                        () => IndexChipProbeController.SetContectTop(false),
                        "Contect",
                        () => IndexChipProbeController.IsContactTop(),
                        "ContectTop");

                    dioControl.BindVacuum(
                        label: "Vacuum",
                        on: () => IndexChipProbeController.SetProbeVac(true),
                        off: () => IndexChipProbeController.SetProbeVac(false),
                        isOk: () => IndexChipProbeController.IsProbeVacValveOn(),
                        isOnState: () => IndexChipProbeController.IsProbeVacValveOn(),
                        displayKey: "ProbeVac",
                        showOkSensor: false // 위에서 OK 센서를 이미 표시했으므로 중복 방지
                    );

                    dioControl.BindCylinder(
                        label: "SphereFB",
                        extend: () => IndexChipProbeController.SetSphereFB(true),
                        retract: () => IndexChipProbeController.SetSphereFB(false),
                        // FWD 센서만 있어도 동작. BWD는 없으면 null 가능(토글은 FWD 센서로 판단)
                        isExtended: () => IndexChipProbeController.IsSphereFwdValveOn(),
                        isRetracted: null,
                        displayKey: "SphereFB",
                        showSensors: false,
                        extendedName: "FWD",
                        retractedName: "BWD"
                    );
                }

                dioControl.RebuildLists();
            }
            catch { }
        }

        private void BindRotaryActuators(Rotary rotary)
        {
            // 그룹 구분선: Rotary (유지)
            if (Rotary != null)
            {
                dioControl.BindDIOInput(() => false, "---- Index ----", "Index");
                dioControl.BindDIOInput(() => Rotary.AirTankPressureOk(), "Rot AirTank OK", "Index_AirTk");
                dioControl.BindDIOInput(() => Rotary.VacTankPressureOk(), "Rot VacTank OK", "Index_VacTk");

                int slotCount = SLOT_VAC.Length; // 8
                for (int slot = 0; slot < slotCount; slot++)
                {
                    int idx = slot;
                    string labelBase = $"Index{idx + 1}";

                    dioControl.BindDIOInput(
                        () => Rotary.IsVacuumOK(idx),
                        $"IndexSlot{idx + 1} FLOW",
                        $"IndexSlot{idx + 1}_Flow");

                    // VAC: 소프트 래치 토글 사용 (isOnState: null)
                    dioControl.BindVacuum(
                        label: $"{labelBase} VAC",
                        on: () => Rotary.SetVacuum(idx, true),
                        off: () => Rotary.SetVacuum(idx, false),
                        isOk: null,
                        isOnState: null,
                        displayKey: $"IndexSlot{idx + 1}_Vac",
                        showOkSensor: false
                    );

                    // BLOW
                    dioControl.BindVacuum(
                        label: $"{labelBase} Blow",
                        on: () => Rotary.SetBlow(idx, true),
                        off: () => Rotary.SetBlow(idx, false),
                        isOk: null,
                        isOnState: null,
                        displayKey: $"IndexSlot{idx + 1}_Blow",
                        showOkSensor: false
                    );

                    // VENT
                    dioControl.BindVacuum(
                        label: $"{labelBase} Vent",
                        on: () => Rotary.SetVent(idx, true),
                        off: () => Rotary.SetVent(idx, false),
                        isOk: null,
                        isOnState: null,
                        displayKey: $"IndexSlot{idx + 1}_Vent",
                        showOkSensor: false
                    );
                }
            }
        }
        #endregion

        

        // 추가: 소켓 번호 콤보 초기화 (1~8)
        private void InitSocketIndexCombo()
        {
            try
            {
                if (comboBoxIndexSocketNo == null) 
                    return;
                
                if (comboBoxIndexSocketNo.Items.Count == 0)
                {
                    for (int i = 1; i <= 8; i++)
                    {
                        comboBoxIndexSocketNo.Items.Add(i);
                    }

                    comboBoxIndexSocketNo.SelectedIndex = 0;
                }
            }
            catch { }
        }

        private async void btnInputMAlign_ClickAsync(object sender, EventArgs e)
        {
            var ask = new MessageBoxYesNo();
            if (ask.ShowDialog("확인", "구동 하시겠습니까?") != DialogResult.Yes)
                return;

            if (Equipment == null)
                return;
        }

        private void comboBoxIndexSocketNo_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Rotary.
        }

        private void btnRotary_Click(object sender, EventArgs e)
        {
            var mb = new MessageBoxOk();
            if (Equipment.Instance.EqState == EquipmentState.Starting ||
                Equipment.Instance.EqState == EquipmentState.AutoRunning ||
                Equipment.Instance.EqState == EquipmentState.ManualRunning)
            {
                mb.ShowDialog("Warring", "장비가 운전 중입니다. 정지 후 시도하세요.");
                return;
            }

            var ask = new MessageBoxYesNo();
            if (ask.ShowDialog("확인", "다음 소켓으로 구동 하시겠습니까?") != DialogResult.Yes)
                return;

            int nRet = 0;

            Log.Write(Rotary.UnitName, "btnRotary_Click", "Rotary Rotate Start");
            nRet = Rotary.MovePositionRotate();
            if(nRet != 0)
            {
                Log.Write(Rotary.UnitName, "Rotary Rotate 실패");
                return;
            }
            Log.Write(Rotary.UnitName, "btnRotary_Click", "Rotary Rotate -------------");
            nRet = Rotary.WaitIndexMoveDone();
            if (nRet != 0)
            {
                Log.Write(Rotary.UnitName, "Rotary Rotate 실패");
                return;
            }

            try { BindTeachingPositions(); } catch { }

            Log.Write(Rotary.UnitName, "btnRotary_Click", "Rotary Rotate End");
        }

        private void LightControlRequested(object sender, EventArgs e)
        {
            Form _lightControlPopup = null;

            if (_lightControlPopup != null && !_lightControlPopup.IsDisposed)
            {
                _lightControlPopup.Close();
            }

            Form popupForm = new Form();
            popupForm.Text = "Light Control";
            popupForm.Size = new Size(467, 286);
            popupForm.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            popupForm.MaximizeBox = false;
            popupForm.MinimizeBox = false;
            popupForm.ShowInTaskbar = false;

            popupForm.StartPosition = FormStartPosition.Manual;
            Point cursorPos = Cursor.Position;
            popupForm.Location = cursorPos;

            popupForm.Owner = null;

            SimpleLightControl lightControl = new SimpleLightControl();
            lightControl.Dock = DockStyle.Fill;
            popupForm.Controls.Add(lightControl);

            _lightControlPopup = popupForm;
            popupForm.FormClosed += (s, ev) => { _lightControlPopup = null; };
            popupForm.Show();
            this.Activate();
        }

        // === 재현성 테스트 ProgressForm 래핑 헬퍼 ===
        private Task<int> CreateReproTestTask(bool startMode, int timeoutMs = 0)
        {
            var tcs = new TaskCompletionSource<int>();
            CancellationTokenSource localCts = null;
            if (timeoutMs > 0)
            {
                localCts = new CancellationTokenSource();
                localCts.CancelAfter(timeoutMs);
                localCts.Token.Register(() =>
                {
                    try
                    {
                        _manualReproTestRunner.Stop();
                    }
                    catch { }
                });
            }

            void Handler(bool running)
            {
                // running == false → 종료
                if (!running)
                {
                    _manualReproTestRunner.RunningChanged -= Handler;
                    tcs.TrySetResult(0);
                    localCts?.Dispose();
                }
            }

            _manualReproTestRunner.RunningChanged += Handler;

            try
            {
                if (startMode)
                {
                    // Start 요청
                    _manualReproTestRunner.Start();
                }
                else
                {
                    // Stop 요청
                    _manualReproTestRunner.Stop();
                }
            }
            catch (Exception ex)
            {
                _manualReproTestRunner.RunningChanged -= Handler;
                tcs.TrySetException(ex);
            }

            return tcs.Task;
        }

        private void ButtonManualTest_Click(object sender, EventArgs e)
        {
            try
            {
                var mb = new MessageBoxOk();
                if (Equipment.Instance.EqState == EquipmentState.Starting ||
                    Equipment.Instance.EqState == EquipmentState.AutoRunning ||
                    Equipment.Instance.EqState == EquipmentState.ManualRunning)
                {
                    mb.ShowDialog("Warring", "장비가 운전 중입니다. 정지 후 시도하세요.");
                    return;
                }

                // Index Calibration 다이얼로그를 모델리스로 표시
                var dlg = new IndexCalibrationDialog(_manualReproTestRunner)
                {
                    StartPosition = FormStartPosition.CenterParent
                };

                // 필요 시 폼 닫힘 후 버튼 상태 복구
                dlg.FormClosed += (s, ev) =>
                {
                    try
                    {
                        // 다이얼로그가 닫힌 후 러너가 계속 돌고 있으면 버튼 텍스트 유지
                        ButtonManualTest.Text = _manualReproTestRunner.IsRunning ? "IndexCal Stop" : "IndexCal Start";
                    }
                    catch { }
                }; 

                // 모델리스로 띄우기 (부모는 현재 폼)
                dlg.Show(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Index Calibration Dialog 표시 실패: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            //var ask = new MessageBoxYesNo();
            //if (ask.ShowDialog("확인", "Index Cal을 실행하시겠습니까?") != DialogResult.Yes)
            //    return;

            //var btn = (Button)sender;
            //btn.Enabled = false;

            //bool isRunning = _manualReproTestRunner.IsRunning;
            //string title = "Manual Repro Test";
            //string msg = isRunning ? "정지 중..." : "실행 중...";
            //var task = CreateReproTestTask(startMode: !isRunning);

            //var pf = new ProgressForm(title, msg, task, _manualReproTestRunner);
            //pf.StopProcess += _ =>
            //{
            //    try { _manualReproTestRunner.Stop(); } catch { }
            //};

            //// 모달 표시 (원하면 Show(this)로 모델리스 가능)
            //pf.ShowDialog(this);

            //// 결과 처리
            //if (task.IsFaulted)
            //{
            //    MessageBox.Show("재현성 테스트 오류: " + task.Exception?.GetBaseException().Message,
            //        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}
            //else if (pf.DialogResult == DialogResult.Cancel)
            //{
            //    // 사용자가 중간 취소
            //    MessageBox.Show("재현성 테스트 취소됨", "취소", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //}
            //else
            //{
            //    // 정상 종료
            //    // 필요 시 완료 메시지 생략 가능
            //    // MessageBox.Show("재현성 테스트 완료", "완료");
            //}

            //btn.Enabled = true;
        }

        private async void ButtonClear_Click(object sender, EventArgs e)
        {
            var mb = new MessageBoxOk();
            if (Equipment.Instance.EqState == EquipmentState.Starting ||
                Equipment.Instance.EqState == EquipmentState.AutoRunning ||
                Equipment.Instance.EqState == EquipmentState.ManualRunning)
            {
                mb.ShowDialog("Warring", "장비가 운전 중입니다. 정지 후 시도하세요.");
                return;
            }

            var ask = new MessageBoxYesNo();
            if (ask.ShowDialog("확인", "Rotary 초기화(InitializeAfterHome)를 실행하시겠습니까?") != DialogResult.Yes)
                return;

            Rotary.RunUnitStatus = BaseUnit.UnitStatus.ManualRunning;

            var btn = (Button)sender;
            btn.Enabled = false;
            try
            {
                var rc = await RunRotaryInitializeAfterHomeAsync(CancellationToken.None).ConfigureAwait(true);
                if (rc == 0)
                {
                    MessageBox.Show("Rotary 초기화 완료.", "완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // 실패 메시지는 내부에서 이미 출력함
                }
            }
            finally
            {
                btn.Enabled = true;
                Rotary.RunUnitStatus = BaseUnit.UnitStatus.Stopped;
            }
        }

        private async Task<int> RunRotaryInitializeAfterHomeAsync(CancellationToken token)
        {
            if (Rotary == null)
            {
                MessageBox.Show("Rotary Unit 없음.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return -1;
            }

            Task<int> t = Rotary.RunManualFunction(Rotary.InitializeAfterHome);
            if (t == null) return 0; // 실행할 작업 없음 → OK 취급

            var form = new ProgressForm("Manual Running", nameof(Rotary.InitializeAfterHome), t, this.Rotary);

            // [추가] ProgressForm 취소 버튼 → 즉시 취소 요청
            form.StopProcess += _ =>
            {
                try { this.Rotary.CancelSequence(); } catch { }
            };

            try
            {
                form.ShowDialog(this);

                if (form.DialogResult == DialogResult.Cancel)
                {
                    // [개선] 폼이 닫힌 뒤에도 취소 전파 유예 대기
                    try
                    {
                        using (var grace = new CancellationTokenSource(2000))
                        {
                            await Task.WhenAny(t, Task.Delay(Timeout.Infinite, grace.Token)).ConfigureAwait(true);
                        }
                    }
                    catch { /* ignore */ }

                    if (!t.IsCompleted)
                    {
                        MessageBox.Show("취소 진행 중.", "취소 진행 중",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return -1;
                    }

                    // 작업이 완료되었으면 rc 확인
                    if (t.IsFaulted)
                    {
                        var mb = new MessageBoxOk();
                        mb.ShowDialog("Manual Run Error!", t.Exception?.GetBaseException().Message);
                        return -1;
                    }

                    var rcCanceled = await t.ConfigureAwait(true);
                    return rcCanceled == 0 ? 0 : -1;
                }

                if (t.IsFaulted)
                {
                    var mb = new MessageBoxOk();
                    mb.ShowDialog("Manual Run Error!", t.Exception?.GetBaseException().Message);
                    return -1;
                }

                // 정상 완료 → rc 확인
                var rc = await t.ConfigureAwait(true);
                if (rc != 0)
                {
                    MessageBox.Show($"Rotary InitializeAfterHome 실패(rc={rc})", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return -1;
                }

                return 0;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                MessageBox.Show("Rotary InitializeAfterHome 예외 발생: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
        }

        private void checkBoxIndexCal_CheckedChanged(object sender, EventArgs e)
        {
            if(checkBoxIndexCal.Checked)
            {
                Equipment.Instance.bIndexCal = true;
            }
            else
            {
                Equipment.Instance.bIndexCal = false;
            }
        }

        bool bVisionPos = false;
        private async void btnManualVision_Click(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            btn.Enabled = false;
            try
            {
                var mb = new MessageBoxOk();
                if (Equipment.Instance.EqState == EquipmentState.Starting ||
                    Equipment.Instance.EqState == EquipmentState.AutoRunning ||
                    Equipment.Instance.EqState == EquipmentState.ManualRunning)
                {
                    mb.ShowDialog("Warring", "장비가 운전 중입니다. 정지 후 시도하세요.");
                    return;
                }

                var ask = new MessageBoxYesNo();
                if (ask.ShowDialog("확인", "비전 위치를 변경하시겠습니까?") != DialogResult.Yes)
                    return;

                if (IndexChipProbeController == null)
                {
                    MessageBox.Show("IndexChipProbeController 없음.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 진행 폼 생성
                using (var cts = new CancellationTokenSource())
                {
                    var task = RunManualVisionAsync(cts.Token);

                    var pf = new ProgressForm("Manual Vision Move", "비전 위치 변경 중...", task, IndexChipProbeController);
                    pf.StopProcess += _ =>
                    {
                        try
                        {
                            // 장비 취소 전파 (있으면 사용)
                            IndexChipProbeController.CancelSequence();
                        }
                        catch { /* ignore */ }
                        try
                        {
                            cts.Cancel();
                        }
                        catch { /* ignore */ }
                    };

                    pf.ShowDialog(this);

                    // 결과 처리
                    if (pf.DialogResult == DialogResult.Cancel)
                    {
                        MessageBox.Show("작업이 취소되었습니다.", "취소", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    // 예외 확인
                    if (task.IsFaulted)
                    {
                        var ex = task.Exception?.GetBaseException();
                        mb.ShowDialog("Manual Vision Error!", ex?.Message ?? "Unknown error");
                        return;
                    }

                    // 정상 완료 코드 확인
                    var rc = await task.ConfigureAwait(true);
                    if (rc != 0)
                    {
                        MessageBox.Show($"비전 위치 변경 실패(rc={rc})", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // 완료 안내 (필요 시)
                    // MessageBox.Show("비전 위치가 변경되었습니다.", "완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            finally
            {
                btn.Enabled = true;
            }
        }

        // 비젼 위치 변경 작업을 비동기로 래핑
        private Task<int> RunManualVisionAsync(CancellationToken ct)
        {
            return Task.Run(() =>
            {
                int nRet = 0;

                // 취소 요청이 들어오면 즉시 종료
                void ThrowIfCanceled()
                {
                    if (ct.IsCancellationRequested)
                        throw new OperationCanceledException(ct);
                }

                ThrowIfCanceled();

                if (bVisionPos == true)
                {
                    if (IndexChipProbeController.IsSphereForward() == false)
                    {
                        if (IndexChipProbeController.SetSphereFB(true))
                        {
                            Thread.Sleep(500);
                            var sw = Stopwatch.StartNew();
                            while (true)
                            {
                                ThrowIfCanceled();

                                if(IndexChipProbeController.IsSphereForward())
                                {
                                    break;
                                }

                                if (sw.ElapsedMilliseconds > 5000)
                                {
                                    IndexChipProbeController.PostAlarm((int)IndexChipProbeController.AlarmKeys.eSphereFBTimeout);
                                    Log.Write(IndexChipProbeController.UnitName, "btnManualVision_Click", "[btnManualVision_Click] SphereFB-F Timeout");
                                    return -1;
                                }
                                Thread.Sleep(1);
                            }
                        }
                    }
                    // 적분구 공정 위치.
                    if (IndexChipProbeController.IsSphereZAtDown() == false)
                    {
                        nRet = IndexChipProbeController.MovePositionSphereZDown();
                        if (nRet != 0)
                        {
                            Log.Write(IndexChipProbeController.UnitName, "btnManualVision_Click", "[btnManualVision_Click] MovePositionSphereZDown failed");
                            return -1;
                        }
                        var sw = Stopwatch.StartNew();
                        while (IndexChipProbeController.IsSphereZAtDown() == false)
                        {
                            ThrowIfCanceled();


                            if (sw.ElapsedMilliseconds > 5000)
                            {
                                IndexChipProbeController.PostAlarm((int)IndexChipProbeController.AlarmKeys.eSphereFBTimeout);
                                Log.Write(IndexChipProbeController.UnitName, "btnManualVision_Click", "[btnManualVision_Click] SphereZ Down Timeout");
                                return -1;
                            }
                            Thread.Sleep(1);
                        }
                    }

                    bVisionPos = false;
                }
                else if(bVisionPos == false)
                {
                    // 적분구 공정 위치.
                    if (IndexChipProbeController.IsSphereZAtReady() == false)
                    {
                        nRet = IndexChipProbeController.MovePositionSphereZReady();
                        if (nRet != 0)
                        {
                            Log.Write(IndexChipProbeController.UnitName, "btnManualVision_Click", "[btnManualVision_Click] MovePositionSphereZReady failed");
                            return -1;
                        }
                    }
                    if (IndexChipProbeController.IsSphereBackward() == false)
                    {
                        if (IndexChipProbeController.SetSphereFB(false))
                        {
                            Thread.Sleep(500);
                            var sw = Stopwatch.StartNew();
                            while (true)
                            {

                                if (IndexChipProbeController.IsSphereBackward())
                                {
                                    break;
                                }

                                ThrowIfCanceled();
                                if (sw.ElapsedMilliseconds > 5000)
                                {
                                    IndexChipProbeController.PostAlarm((int)IndexChipProbeController.AlarmKeys.eSphereFBTimeout);
                                    Log.Write(IndexChipProbeController.UnitName, "btnManualVision_Click", "[btnManualVision_Click] SphereFB-F Timeout");
                                    return -1;
                                }
                                Thread.Sleep(1);
                            }
                        }
                    }

                    bVisionPos = true;
                }

                return 0;
            }, ct);
        }


    }
}
