using QMC.Common;
using QMC.Common.Controls;
using QMC.Common.Motions;
using QMC.Common.UI;
using QMC.Common.Unit;
using QMC.Common.Vision;
using QMC.LCP_280.Process.Component;
using QMC.LCP_280.Process.Unit.FormSetup;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static QMC.LCP_280.Process.Unit.FormMain.SequenceManualControl;
using static QMC.LCP_280.Process.Unit.InputStage;

namespace QMC.LCP_280.Process.Unit.FormMain
{
    [FormOrder(2)]
    public partial class Operator_Main : Form
    {
        private Equipment _Equipment;

        // Units
        private InputCassetteLifter InputCassetteLifter { get; set; }
        private InputFeeder InputFeeder { get; set; }
        private InputStage InputStage { get; set; }
        private InputDieTransfer InputDieTransfer { get; set; }
        private Rotary Rotary { get; set; }
        private IndexLoadAligner IndexLoadAligner { get; set; }
        private IndexChipProbeController IndexChipProbeController { get; set; }
        private IndexChipProber IndexChipProber { get; set; }
        private IndexUnloadAligner IndexUnloadAligner { get; set; }
        private OutputDieTransfer OutputDieTransfer { get; set; }
        private OutputStage OutputStage { get; set; }
        private OutputFeeder OutputFeeder{ get; set; }
        private OutputCassetteLifter OutputCassetteLifter { get; set; }
        private InputStageEjector InputStageEjector { get; set; }
        // State
        private bool _initialized;
        private bool _preloadRequested;
        private bool _deferredInitDone;
        private bool _isLayoutEditMode;

        // Auto Sequence 상태
        private bool _autoReady = false;
        private bool _autoStarting = false;

        // Manual Sequence 상태
        private HashSet<string> _readySequences;
        private HashSet<string> _startSequences;

        // Jog Popup
        private Form_AxisJogPopup _jogPopup = null;
        private AxisPostionPopup _axisPosPopup = null;

        public Operator_Main() : this(
            TryGetUnit<InputFeeder>(Equipment.UnitKeys.InputFeeder),
            TryGetUnit<InputDieTransfer>(Equipment.UnitKeys.InputDieTransfer),
            TryGetUnit<Rotary>(Equipment.UnitKeys.Rotary),
            TryGetUnit<OutputDieTransfer>(Equipment.UnitKeys.OutputDieTransfer),
            TryGetUnit<OutputFeeder>(Equipment.UnitKeys.OutputFeeder),
            TryGetUnit<InputStage>(Equipment.UnitKeys.InputStage),
            TryGetUnit<IndexUnloadAligner>(Equipment.UnitKeys.IndexUnloadAligner),
            TryGetUnit<OutputStage>(Equipment.UnitKeys.OutputStage),
            TryGetUnit<InputCassetteLifter>(Equipment.UnitKeys.InputCassetteLifter),
            TryGetUnit<IndexLoadAligner>(Equipment.UnitKeys.IndexLoadAligner),
            TryGetUnit<IndexChipProbeController>(Equipment.UnitKeys.IndexChipProbeController),
            TryGetUnit<OutputCassetteLifter>(Equipment.UnitKeys.OutputCassetteLifter),
            TryGetUnit<InputStageEjector>(Equipment.UnitKeys.InputStageEjector),
            TryGetUnit<IndexChipProber>(Equipment.UnitKeys.IndexChipProber))
        {
        }

        public Operator_Main(InputFeeder inputFeeder, InputDieTransfer inputDieTransfer, Rotary rotary,
                            OutputDieTransfer outputDieTransfer, OutputFeeder outputFeeder,
                            InputStage inputStage, IndexUnloadAligner indexUnloadAligner, OutputStage outputStage,
                            InputCassetteLifter inputCassetteLifter, IndexLoadAligner indexLoadAligner,
                            IndexChipProbeController indexChipProbeController, OutputCassetteLifter outputCassetteLifter,
                            InputStageEjector inputStageEjector, IndexChipProber indexChipProber)
        {
            InitializeComponent();

            _Equipment = Equipment.Instance;
            if (_Equipment != null)
            {
                _Equipment.StateChanged += Eq_StateChanged_ForOperator;
                _Equipment.UnitStateChanged += Eq_UnitStateChanged_ForOperator;
            }

            InputFeeder = inputFeeder;
            InputDieTransfer = inputDieTransfer;
            Rotary = rotary;
            OutputDieTransfer = outputDieTransfer;
            OutputFeeder = outputFeeder;
            InputStage = inputStage;
            IndexUnloadAligner = indexUnloadAligner;
            OutputStage = outputStage;

            InputCassetteLifter = inputCassetteLifter;
            IndexLoadAligner = indexLoadAligner;
            IndexChipProbeController = indexChipProbeController;
            OutputCassetteLifter = outputCassetteLifter;

            InputStageEjector = inputStageEjector;
            IndexChipProber = indexChipProber;

            _readySequences = new HashSet<string>();
            _startSequences = new HashSet<string>();

            Load += Vision_Manual_Load;

            sequenceManualControl.SequenceButtonRequested += OnManualSequenceButtonRequested;

            InputWaferCamera.LightControlRequested += LightControlRequested;
            IndexOutputCamera.LightControlRequested += LightControlRequested;
            OutputWaferCamera.LightControlRequested += LightControlRequested;

            if (InputStage != null)
            {
                InputStage.MarksFound -= OnMarksFound;
                InputStage.MarksFound += OnMarksFound;
            }

            if(IndexUnloadAligner != null)
            {
                IndexUnloadAligner.MarksFound -= OnMarksFound;
                IndexUnloadAligner.MarksFound += OnMarksFound;
            }

                
        }

        #region Form Cleanup
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (sequenceManualControl != null)
                sequenceManualControl.SequenceButtonRequested -= OnManualSequenceButtonRequested;

            if (InputStage != null)
                InputStage.MarksFound -= OnMarksFound;

            if (IndexUnloadAligner != null)
                IndexUnloadAligner.MarksFound -= OnMarksFound;

            try
            {
                if (_Equipment != null)
                {
                    _Equipment.StateChanged -= Eq_StateChanged_ForOperator;
                    _Equipment.UnitStateChanged -= Eq_UnitStateChanged_ForOperator;
                }
            }
            catch { }
            base.OnFormClosing(e);
        }
        #endregion

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

        #region UI 초기화
        public void PreloadUI()
        {
            if (IsDisposed || Disposing) return;
            if (_preloadRequested) return;
            _preloadRequested = true;
            EnsureInitialized();
            var handle = Handle;
        }
        private void Vision_Manual_Load(object sender, EventArgs e) => EnsureInitialized();
        private void EnsureInitialized()
        {
            if (_initialized) return;
            _initialized = true;

            try
            {
                BeginInvoke(new Action(StartDeferredInit));
            }
            catch (Exception ex)
            {
                try
                {
                    Controls.Add(new Label
                    {
                        Dock = DockStyle.Fill,
                        Text = $"Init 실패: {ex.Message}",
                        ForeColor = Color.Red,
                        TextAlign = ContentAlignment.MiddleCenter
                    });
                }
                catch { }
            }
        }

        private void OnMarksFound(object sender, PatternMarksFoundEventArgs e)
        {
            VisionImageViewer viewer = null;
            if (sender == (object)InputStage) 
                viewer = InputWaferCamera;
            else if (sender == (object)IndexUnloadAligner) 
                viewer = IndexOutputCamera;

            if (viewer == null || viewer.IsDisposed) 
                return;

            
            Action apply = () => ApplyMarkOverlays(viewer, e);
            if (viewer.InvokeRequired) viewer.BeginInvoke(apply);
            else apply();
        }

        private void ApplyMarkOverlays(VisionImageViewer viewer, PatternMarksFoundEventArgs e)
        {
            try
            {
                
                // 최신 이미지 갱신 가능 시
                if (e.Image != null)
                {
                    // 프로젝트의 이미지 설정 메서드에 맞게 적용하세요.
                    // 예: viewer.InputImage = e.Image.GetImage();
                }

                // ResultOverlays 컬렉션 존재 시 사용
                var overlaysProp = viewer.GetType().GetProperty("ResultOverlays");
                var list = overlaysProp?.GetValue(viewer) as System.Collections.IList;
                if (list != null)
                {
                    lock (list)
                    {

                        list?.Clear();

                        for (int i = 0; i < e.Marks.Count; i++)
                        {
                            var m = e.Marks[i];

                            var ov = new QMC.Common.Vision.PatternMatchResultOverlay
                            {
                                Center = new PointF((float)m.X, (float)m.Y),
                                PatternWidth = m.TrainW > 0 ? m.TrainW : 40,
                                PatternHeight = m.TrainH > 0 ? m.TrainH : 40,
                                AngleDeg = (float)m.AngleDeg,
                                CrossHalfLenPx = (i == e.RepresentativeIndex) ? 24 : 16,
                                Highlight = (i == e.RepresentativeIndex),
                                Index = i,
                                Color = (i == e.RepresentativeIndex) ? Color.Yellow : Color.Lime,
                                Thickness = (i == e.RepresentativeIndex) ? 2f : 1f,
                                Visible = true
                            };
                            list?.Add(ov);
                        }
                    }
                }
                viewer.ResumeDisplay();
            }
            catch { }
        }

        private async void StartDeferredInit()
        {
            if (_deferredInitDone) return;
            _deferredInitDone = true;
            await Task.Delay(30);
            if (IsDisposed || Disposing) return;
            try { BindCamera(); } catch { }
        }
        #endregion

        #region Camera
        private void BindCamera()
        {
            try
            {
                if (InputWaferCamera != null && InputStage?.StageCamera != null)
                {
                    if (InputWaferCamera.Camera != InputStage.StageCamera)
                        InputWaferCamera.Camera = InputStage.StageCamera;
                    try { InputStage.StageCamera.StartLive(); } catch { }
                    try { InputWaferCamera.StartUpdateTask(); } catch { }
                }
                if (IndexOutputCamera != null && IndexUnloadAligner?.IndexOutCamera != null)
                {
                    if (IndexOutputCamera.Camera != IndexUnloadAligner.IndexOutCamera)
                        IndexOutputCamera.Camera = IndexUnloadAligner.IndexOutCamera;
                    try { IndexUnloadAligner.IndexOutCamera.StartLive(); } catch { }
                    try { IndexOutputCamera.StartUpdateTask(); } catch { }
                }
                if (OutputWaferCamera != null && OutputStage?.OutStageCamera != null)
                {
                    if (OutputWaferCamera.Camera != OutputStage.OutStageCamera)
                        OutputWaferCamera.Camera = OutputStage.OutStageCamera;
                    try { OutputStage.OutStageCamera.StartLive(); } catch { }
                    try { OutputWaferCamera.StartUpdateTask(); } catch { }
                }
            }
            catch { }
        }
        #endregion


        // 장비 상태 반영
        private void Eq_StateChanged_ForOperator(object sender, EquipmentStateChangedEventArgs e)
        {
            if (IsDisposed || Disposing) 
                return;
            if (InvokeRequired) 
            { 
                BeginInvoke(new Action<object, EquipmentStateChangedEventArgs>(Eq_StateChanged_ForOperator), sender, e); 
                return; 
            }

            ApplyOperatorEquipmentState(e.NewState);
        }

        private void ApplyOperatorEquipmentState(EquipmentState state)
        {
            // Auto 버튼들 (sequenceAutoControl 내부 API 가정)
            bool canReady = state == EquipmentState.Stopped || state == EquipmentState.Ready;
            bool canStart = state == EquipmentState.AutoRunning || state == EquipmentState.Ready;
            bool canStop = state == EquipmentState.AutoRunning || state == EquipmentState.Starting;

            if (state == EquipmentState.Error)
            {
                //sequenceAutoControl.SetAllEnabled(false);
                //sequenceManualControl.SetAllEnabled(false);
            }
            else
            {
                // Manual 컨트롤은 Running 중에도 Ready/Start 토글 제한 정책에 맞게
                //sequenceAutoControl.SetAllEnabled(true);
                //sequenceManualControl.SetAllEnabled(true);
            }
        }

        // Unit 상태 반영 (필요 시 단일 유닛 수동 제어 UI 갱신 – 확장 여지)
        private void Eq_UnitStateChanged_ForOperator(object sender, UnitStateChangedEventArgs e)
        {
            if (IsDisposed || Disposing) return;
            if (InvokeRequired) { BeginInvoke(new Action<object, UnitStateChangedEventArgs>(Eq_UnitStateChanged_ForOperator), sender, e); return; }

            // 예: Manual 버튼 깜빡임/비활성 처리 필요시 여기에 추가
            // sequenceManualControl.OnSequenceStateChanged(new SequenceStateChangedEventArgs { ... });
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

        #region Manual Sequence 처리
        // Start 공통 (Manual/Auto 공용) : Start 토글 전용 내부 호출용
        private async Task<bool> StartSequenceAsync(string sequenceName, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return await StartUnitsForSequenceAsync(sequenceName, ct, parallel: true).ConfigureAwait(true);
        }
        // Unit Running 대기 유틸
        private async Task<bool> WaitForUnitRunningAsync(BaseUnit unit, int timeoutMs, CancellationToken ct)
        {
            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                ct.ThrowIfCancellationRequested();
                if (unit.RunUnitStatus == BaseUnit.UnitStatus.AutoRunning || unit.IsRunning)
                    return true;

                await Task.Delay(100, ct).ConfigureAwait(true);
            }
            return unit.RunUnitStatus == BaseUnit.UnitStatus.AutoRunning || unit.IsRunning;
        }
        // 시퀀스명 → Units 매핑 (복수 유닛 지원)
        private IEnumerable<BaseUnit> GetUnitsForSequence(string sequenceName)
        {
            switch (sequenceName)
            {
                case "InputWafer":
                    return new BaseUnit[]
                    {
                        InputFeeder,
                        InputCassetteLifter,
                        InputStage
                    }.Where(u => u != null);

                case "ChipLoading":
                    return new BaseUnit[]
                    {
                        InputDieTransfer,
                        InputStageEjector
                    }.Where(u => u != null);

                case "Process":
                    return new BaseUnit[]
                    {
                        Rotary,
                        IndexLoadAligner,
                        IndexChipProbeController,
                        IndexChipProber,
                        IndexUnloadAligner
                    }.Where(u => u != null);

                case "ChipUnloading":
                    return new BaseUnit[]
                    {
                        OutputDieTransfer
                    }.Where(u => u != null);

                case "OutputWafer":
                    return new BaseUnit[]
                    {
                        OutputFeeder,
                        OutputCassetteLifter,
                        OutputStage
                    }.Where(u => u != null);
            }
            return Enumerable.Empty<BaseUnit>();
        }
        // 시퀀스 단위 다중 시작 + Running 전이 대기
        private async Task<bool> StartUnitsForSequenceAsync(string sequenceName, CancellationToken ct, bool parallel = true)
        {
            var units = GetUnitsForSequence(sequenceName).ToList();
            if (units.Count == 0)
            {
                Log.Write("Operator_Main", $"StartUnitsForSequenceAsync - '{sequenceName}' 매핑된 Unit 없음");
                return false;
            }

            // UnitName 기준 중복 제거
            var distinctUnits = new List<BaseUnit>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var u in units)
            {
                var key = string.IsNullOrEmpty(u.UnitName) ? u.GetHashCode().ToString() : u.UnitName;
                if (seen.Add(key))
                    distinctUnits.Add(u);
            }

            if (parallel)
            {
                var startTasks = distinctUnits.Select(TryStartUnitAsync).ToArray();
                var started = await Task.WhenAll(startTasks).ConfigureAwait(true);
                if (!started.All(r => r)) return false;

                var waitTasks = distinctUnits.Select(u => WaitForUnitRunningAsync(u, 5000, ct)).ToArray();
                var waited = await Task.WhenAll(waitTasks).ConfigureAwait(true);
                return waited.All(r => r);
            }
            else
            {
                foreach (var u in distinctUnits)
                {
                    ct.ThrowIfCancellationRequested();
                    var ok = await TryStartUnitAsync(u).ConfigureAwait(true);
                    if (!ok) return false;

                    var running = await WaitForUnitRunningAsync(u, 5000, ct).ConfigureAwait(true);
                    if (!running) return false;
                }
                return true;
            }
        }
        // 시퀀스 단위 다중 정지
        private async Task StopUnitsForSequenceAsync(string sequenceName)
        {
            var eq = Equipment.Instance;
            var units = GetUnitsForSequence(sequenceName).ToList();
            foreach (var u in units)
            {
                try
                {
                    if (u != null && !string.IsNullOrEmpty(u.UnitName))
                        await eq.StopUnitAsync(u.UnitName).ConfigureAwait(true);
                }
                catch (Exception ex) { Log.Write(ex); }
            }
        }

        private void OnManualSequenceButtonRequested(object sender, SequenceEventArgs e)
        {
            Log.Write("Operator_Main", $"Manual Sequence {e.SequenceName} {e.Action} 요청");
            if (e.Action == "Ready") 
                HandleReadyAction(e.SequenceName);
            else if (e.Action == "Start") 
                HandleStartAction(e.SequenceName);
        }
        private async void HandleReadyAction(string sequenceName)
        {
            if (_readyBusy)
            {
                Log.Write("Operator_Main", "Ready 작업 진행 중 - 중복 요청 무시");
                return;
            }

            bool isCurrentlyReady = _readySequences.Contains(sequenceName);
            if (isCurrentlyReady)
            {
                _readySequences.Remove(sequenceName);
                NotifySequenceStateChanged(sequenceName, "Ready", false, false);
                Log.Write("Operator_Main", $"{sequenceName} Ready OFF");
                return;
            }

            _readyBusy = true;
            _readyCts?.Cancel();
            _readyCts = new CancellationTokenSource();
            var ct = _readyCts.Token;

            var prevCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            SetReadyUiBusy(true);

            try
            {
                bool ok = await TryReadySequenceAsync(sequenceName, ct);
                if (ok && !ct.IsCancellationRequested)
                {
                    _readySequences.Add(sequenceName);
                    NotifySequenceStateChanged(sequenceName, "Ready", true, false);
                    Log.Write("Operator_Main", $"{sequenceName} Ready ON");
                }
                else
                {
                    NotifySequenceStateChanged(sequenceName, "Ready", false, false);
                }
            }
            catch (OperationCanceledException)
            {
                Log.Write("Operator_Main", $"{sequenceName} Ready 취소");
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                NotifySequenceStateChanged(sequenceName, "Ready", false, false);
                MessageBox.Show($"{sequenceName} Ready 예외: {ex.Message}", "Ready Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = prevCursor;
                SetReadyUiBusy(false);
                _readyBusy = false;
            }
        }
        private void SetReadyUiBusy(bool busy)
        {
            try { sequenceManualControl.Enabled = !busy; } catch { }
        }
        private async void HandleStartAction(string sequenceName)
        {
            // Start ON → OFF (Stop)
            if (_startSequences.Contains(sequenceName))
            {
                _startSequences.Remove(sequenceName);
                NotifySequenceStateChanged(sequenceName, "Start", false, true);
                Log.Write("Operator_Main", $"{sequenceName} Start OFF (Stop 요청)");

                try
                {
                    await StopUnitsForSequenceAsync(sequenceName);
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }
                return;
            }

            // Start OFF → ON
            if (!_readySequences.Contains(sequenceName))
            {
                MessageBox.Show($"{sequenceName}를 먼저 Ready 상태로 만들어주세요.");
                return;
            }

            // Ready OFF 처리
            _readySequences.Remove(sequenceName);
            NotifySequenceStateChanged(sequenceName, "Ready", false, false);

            _startSequences.Add(sequenceName);
            NotifySequenceStateChanged(sequenceName, "Start", true, true);
            Log.Write("Operator_Main", $"{sequenceName} Start ON (Ready OFF)");

            // 복수 Unit 시작
            bool ok = await StartSequenceAsync(sequenceName, CancellationToken.None);
            if (!ok)
            {
                _startSequences.Remove(sequenceName);
                NotifySequenceStateChanged(sequenceName, "Start", false, true);
                Log.Write("Operator_Main", $"{sequenceName} Start 실패");
            }
        }
        private void NotifySequenceStateChanged(string sequenceName, string action, bool isActive, bool updateText)
        {
            sequenceManualControl.OnSequenceStateChanged(new SequenceStateChangedEventArgs
            {
                SequenceName = sequenceName,
                Action = action,
                IsActive = isActive,
                UpdateText = updateText
            });
        }


        // ===== Ready: 시퀀스별 다중 Ready 작업 빌드/실행 =====

        // 시퀀스별 Ready 작업(Task<int>) 목록 생성
        // rc == 0 이면 성공, 0 이외는 실패
        private IEnumerable<Func<CancellationToken, Task<int>>> BuildReadyTasks(string sequenceName)
        {
            switch (sequenceName)
            {
                case "InputWafer":
                    return new Func<CancellationToken, Task<int>>[]
                    {
                        // InputStageEjector.CheckReady()
                        ct => Task.Run(() =>
                        {
                            ct.ThrowIfCancellationRequested();
                            return InputStageEjector?.CheckReady() ?? -1;
                        }, ct),

                        // InputFeeder.EnsureReady()
                        ct => Task.Run(() =>
                        {
                            ct.ThrowIfCancellationRequested();
                            return InputFeeder?.EnsureReady() ?? -1;
                        }, ct),
                        
                    };

                case "ChipLoading":
                    return new Func<CancellationToken, Task<int>>[]
                    {
                        ct => Task.Run(() =>
                        {
                            ct.ThrowIfCancellationRequested();
                            return InputDieTransfer?.EnsureReady() ?? -1;
                        }, ct)
                    };

                case "Process":
                    return new Func<CancellationToken, Task<int>>[]
                    {
                        ct => Task.Run(() =>
                        {
                            ct.ThrowIfCancellationRequested();
                            return IndexLoadAligner?.EnsureReady() ?? -1;
                        }, ct),
                        ct => Task.Run(() =>
                        {
                            ct.ThrowIfCancellationRequested();
                            return IndexChipProbeController?.EnsureReady() ?? -1;
                        }, ct),
                    };

                case "ChipUnloading":
                    return new Func<CancellationToken, Task<int>>[]
                    {
                        ct => Task.Run(() =>
                        {
                            ct.ThrowIfCancellationRequested();
                            return OutputDieTransfer?.EnsureReady() ?? -1;
                        }, ct)
                    };

                case "OutputWafer":
                    return new Func<CancellationToken, Task<int>>[]
                    {
                        ct => Task.Run(() =>
                        {
                            ct.ThrowIfCancellationRequested();
                            return OutputFeeder?.EnsureReady() ?? -1;
                        }, ct)
                    };
            }

            // 알 수 없는 시퀀스
            return Enumerable.Empty<Func<CancellationToken, Task<int>>>();
        }
        // 시퀀스 단위로 Ready 작업 실행(병렬/순차)
        private async Task<bool> ReadyUnitsForSequenceAsync(string sequenceName, CancellationToken ct, bool parallel = true)
        {
            var tasksFactory = BuildReadyTasks(sequenceName).ToList();
            if (tasksFactory.Count == 0)
            {
                Log.Write("Operator_Main", $"ReadyUnitsForSequenceAsync - '{sequenceName}' Ready 작업 없음");
                return false;
            }

            try
            {
                if (parallel)
                {
                    var tasks = tasksFactory.Select(f => f(ct)).ToArray();
                    var rcs = await Task.WhenAll(tasks).ConfigureAwait(true);
                    if (rcs.Any(rc => rc != 0))
                    {
                        Log.Write("Operator_Main", $"{sequenceName} Ready 실패(rc들: {string.Join(",", rcs)})");
                        return false;
                    }
                }
                else
                {
                    foreach (var f in tasksFactory)
                    {
                        ct.ThrowIfCancellationRequested();
                        var rc = await f(ct).ConfigureAwait(true);
                        if (rc != 0)
                        {
                            Log.Write("Operator_Main", $"{sequenceName} Ready 실패(rc={rc})");
                            return false;
                        }
                    }
                }
                return true;
            }
            catch (OperationCanceledException)
            {
                Log.Write("Operator_Main", $"{sequenceName} Ready 취소됨");
                throw;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }
        }
        // (기존) TryReadySequenceAsync → 공통 Ready 실행으로 대체
        private async Task<bool> TryReadySequenceAsync(string sequenceName, CancellationToken ct)
        {
            return await ReadyUnitsForSequenceAsync(sequenceName, ct, parallel: true).ConfigureAwait(true);
        }
        #endregion


        #region 비즈니스 로직
        private CancellationTokenSource _readyCts;
        private bool _readyBusy;

        // 공통 Start
        private async Task<bool> TryStartUnitAsync(BaseUnit unit)
        {
            if (unit == null) 
                return false;

            try
            {
                var eq = Equipment.Instance;
                if (eq == null)
                {
                    Log.Write("Operator_Main", "TryStartUnitAsync 실패 - Equipment 인스턴스 없음");
                    return false;
                }
                var unitName = unit.UnitName;
                if (string.IsNullOrEmpty(unitName))
                {
                    Log.Write("Operator_Main", "TryStartUnitAsync 실패 - UnitName 비어있음");
                    return false;
                }
                if (unit.RunUnitStatus == BaseUnit.UnitStatus.AutoRunning)
                {
                    Log.Write("Operator_Main", $"TryStartUnitAsync - Unit '{unitName}' 이미 실행 중");
                    return true;
                }

                var prev = Cursor.Current;
                Cursor.Current = Cursors.WaitCursor;
                try
                {
                    bool ok = await eq.StartUnitAsync(unitName).ConfigureAwait(true);
                    if (!ok)
                    {
                        Log.Write("Operator_Main", $"TryStartUnitAsync 실패 - Unit '{unitName}' 시작 실패");
                        return false; // 중요: 실제 실패 반환
                    }
                    return true;
                }
                finally 
                { 
                    Cursor.Current = prev; 
                }
            }
            catch (Exception ex) 
            { 
                Log.Write(ex);
                return false; // 예외 시 실패 반환 (중요)
            }
        }
        #endregion


        // Jog Popup
        private void btn_Jog_Click(object sender, EventArgs e)
        {
            ShowOrRestoreJogPopup(this);
            ShowOrRestoreAxisPosPopup(this);
        }
        private void ShowOrRestoreJogPopup(IWin32Window owner)
        {
            if (_jogPopup == null || _jogPopup.IsDisposed)
            {
                _jogPopup = new Form_AxisJogPopup
                {
                    StartPosition = FormStartPosition.CenterParent,
                    ShowInTaskbar = true
                };
                _jogPopup.StartPosition = FormStartPosition.CenterScreen;
                _jogPopup.Owner = null;
                _jogPopup.Load += (s, e) =>
                {
                    TaskbarHelper.SetAppId(_jogPopup.Handle, "MyApp.JogPanel");
                };
                _jogPopup.FormClosed += (s, _) => { _jogPopup = null; };
                _jogPopup.FormClosing += (s, ev) =>
                {
                    if (ev.CloseReason == CloseReason.UserClosing)
                    {
                        ev.Cancel = true;
                        _jogPopup.Hide();
                    }
                };
            }
            if (!_jogPopup.Visible)
                _jogPopup.Show();
            if (_jogPopup.WindowState == FormWindowState.Minimized)
                _jogPopup.WindowState = FormWindowState.Normal;
            _jogPopup.BringToFront();
            _jogPopup.TopMost = true;
            _jogPopup.Activate();
        }
        private void ShowOrRestoreAxisPosPopup(IWin32Window owner)
        {
            if (_axisPosPopup == null || _axisPosPopup.IsDisposed)
            {
                _axisPosPopup = new AxisPostionPopup
                {
                    StartPosition = FormStartPosition.CenterParent,
                    ShowInTaskbar = true
                };
                _axisPosPopup.StartPosition = FormStartPosition.CenterScreen;
                _axisPosPopup.Owner = null;
                _axisPosPopup.Load += (s, e) =>
                {
                    TaskbarHelper.SetAppId(_axisPosPopup.Handle, "MyApp.AxisPosition");
                };
                _axisPosPopup.FormClosed += (s, _) => { _axisPosPopup = null; };
                _axisPosPopup.FormClosing += (s, ev) =>
                {
                    if (ev.CloseReason == CloseReason.UserClosing)
                    {
                        ev.Cancel = true;
                        _axisPosPopup.Hide();
                    }
                };
            }
            if (!_axisPosPopup.Visible)
                _axisPosPopup.Show();
            if (_axisPosPopup.WindowState == FormWindowState.Minimized)
                _axisPosPopup.WindowState = FormWindowState.Normal;
            _axisPosPopup.BringToFront();
            _axisPosPopup.TopMost = true;
            _axisPosPopup.Activate();
        }
        private CancellationTokenSource _homeCts;

        // === HomeAll: Helpers ===
        private bool ConfirmHome()
        {
            var ask = new MessageBoxYesNo();
            return ask.ShowDialog("확인", "축 홈을 진행하시겠습니까?") == DialogResult.Yes;
        }

        private CancellationToken PrepareNewHomeToken()
        {
            _homeCts?.Cancel();
            _homeCts?.Dispose();
            _homeCts = new CancellationTokenSource();
            return _homeCts.Token;
        }

        private bool TryServoOnAllAxes()
        {
            var axes = _Equipment.AxisManager?.GetAll();
            if (axes == null || axes.Length == 0)
            {
                MessageBox.Show("등록된 축이 없습니다.");
                return false;
            }

            foreach (var ax in axes)
            {
                try { ax.ClearAlarm(); } catch { }
                try { ax.Servo(true); } catch { }
            }
            return true;
        }

        private async Task<int> RunHomeSequenceWithDialogAsync(CancellationToken token)
        {
            HomeProgressForm dlg = null;
            try
            {
                var seq = MachineHomeCoordinator.BuildDefaultHomeSequence(_Equipment);

                dlg = new HomeProgressForm();
                dlg.InitializeProgress("Machine Home", seq.TotalSteps);

                seq.OnProgress(p =>
                {
                    dlg.SafeUpdate(p);
                });

                dlg.CancelRequested += () =>
                {
                    try { _homeCts.Cancel(); } catch { }
                    try { _Equipment.AxisManager?.EmgStopAll(); } catch { }
                };

                dlg.ForceStopRequested += () =>
                {
                    try { _Equipment.AxisManager?.EmgStopAll(); } catch { }
                };

                var runTask = seq.RunAsync(token);

                dlg.Show(this);
                dlg.BringToFront();

                // 취소/완료 중 먼저 끝난 것 대기
                var completed = await Task.WhenAny(runTask, Task.Delay(Timeout.Infinite, token)).ConfigureAwait(true);

                if (completed != runTask)
                {
                    // 취소됨: 2초 유예 후 여전히 미완료면 더 기다리지 않고 종료
                    var grace = await Task.WhenAny(runTask, Task.Delay(2000)).ConfigureAwait(true);
                    if (grace != runTask)
                    {
                        dlg.SafeUpdate(new OperationProgress
                        {
                            OperationId = "HOME",
                            Title = "Home",
                            StepIndex = seq.TotalSteps - 1,
                            TotalSteps = seq.TotalSteps,
                            IsCompleted = true,
                            IsCanceled = true,
                            IsAborted = true,
                            Message = "Canceled"
                        });
                        throw new OperationCanceledException();
                    }
                }

                var results = await runTask.ConfigureAwait(true);

                dlg.SafeUpdate(new OperationProgress
                {
                    OperationId = "HOME",
                    Title = "Home",
                    StepIndex = seq.TotalSteps - 1,
                    TotalSteps = seq.TotalSteps,
                    IsCompleted = true,
                    IsCanceled = token.IsCancellationRequested,
                    IsAborted = seq.Aborted,
                    Message = seq.AbortReason
                });

                int success = results.Count(r => r.Success);
                int notStarted = results.Count(r => !r.Started);
                int fail = results.Count - success - notStarted;

                string msg = $"Home 완료\r\n성공: {success}, 실패: {fail}, 미시작: {notStarted}";

                if (fail > 0 || notStarted > 0)
                {
                    var detailList = new List<string>();
                    foreach (var r in results)
                    {
                        string status;
                        if (r.Success)
                        {
                            status = "OK";
                        }
                        else if (r.Started)
                        {
                            status = (r.Error != null && r.Error.Message != null)
                                ? r.Error.Message
                                : "rc=" + r.ReturnCode;
                        }
                        else
                        {
                            status = "NOT STARTED (" + r.FailReason + ")";
                        }
                        detailList.Add("- " + r.AxisName + ": " + status);
                    }
                    var detail = string.Join("\r\n", detailList);
                    msg += "\r\n\r\n" + detail;
                }

                // 결과 표시(요약)
                //MessageBox.Show(msg, "Home");

                // 반환: 모두 성공(미시작/실패 없음) 시 0, 아니면 -1
                return (fail == 0 && notStarted == 0 && !seq.Aborted) ? 0 : -1;
            }
            finally
            {
                try { dlg?.Close(); dlg?.Dispose(); } catch { }
            }
        }

        private async Task<int> RunRotaryInitializeAfterHomeAsync(CancellationToken token)
        {
            Task<int> t = Rotary.RunManualFunction(Rotary.InitializeAfterHome);
            if (t == null) 
                return 0; // 실행할 작업 없음 → OK 취급

            var form = new ProgressForm("Manual Running", nameof(Rotary.InitializeAfterHome), t, this.Rotary);
            try
            {
                form.ShowDialog();

                if (form.DialogResult == DialogResult.Cancel)
                {
                    this.Rotary.CancelSequence();
                    MessageBox.Show("Rotary InitializeAfterHome 실패", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return -1;
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

        private async Task<int> MoveUnitsToReadyAsync(CancellationToken token)
        {
            string failureSummary = null;

            var t = Task.Run(async () =>
            {
                try
                {
                    // 1) InputDieTransfer + OutputDieTransfer (동시)
                    {
                        token.ThrowIfCancellationRequested();

                        var tasks = new List<Task<int>>();
                        var names = new List<string>();

                        if (InputDieTransfer != null)
                        {
                            tasks.Add(Task.Run(() => InputDieTransfer.EnsureReady(), token));
                            names.Add("InputDieTransfer");
                        }
                        if (OutputDieTransfer != null)
                        {
                            tasks.Add(Task.Run(() => OutputDieTransfer.EnsureReady(), token));
                            names.Add("OutputDieTransfer");
                        }

                        if (tasks.Count > 0)
                        {
                            var results = await Task.WhenAll(tasks).ConfigureAwait(false);
                            for (int i = 0; i < results.Length; i++)
                            {
                                if (results[i] != 0)
                                {
                                    failureSummary = names[i];
                                    return -1; // 첫 실패 시 즉시 NG
                                }
                            }
                        }
                    }

                    // 2) Rotary (단독)
                    if (Rotary != null)
                    {
                        token.ThrowIfCancellationRequested();

                        var rc = await Task.Run(() => Rotary.ExecuteUnitActionReady(), token).ConfigureAwait(false);
                        if (rc != 0)
                        {
                            failureSummary = "Rotary(ExecuteUnitActionReady)";
                            return -1;
                        }
                    }

                    // 3) InputStageEjector (단독, CheckReady)
                    if (InputStageEjector != null)
                    {
                        token.ThrowIfCancellationRequested();

                        var rc = await Task.Run(() => InputStageEjector.CheckReady(), token).ConfigureAwait(false);
                        if (rc != 0)
                        {
                            failureSummary = "InputStageEjector(CheckReady)";
                            return -1;
                        }
                    }

                    // 4) InputFeeder + OutputFeeder (동시)
                    {
                        token.ThrowIfCancellationRequested();

                        var tasks = new List<Task<int>>();
                        var names = new List<string>();

                        if (InputFeeder != null)
                        {
                            tasks.Add(Task.Run(() => InputFeeder.EnsureReady(), token));
                            names.Add("InputFeeder");
                        }
                        if (OutputFeeder != null)
                        {
                            tasks.Add(Task.Run(() => OutputFeeder.EnsureReady(), token));
                            names.Add("OutputFeeder");
                        }

                        if (tasks.Count > 0)
                        {
                            var results = await Task.WhenAll(tasks).ConfigureAwait(false);
                            for (int i = 0; i < results.Length; i++)
                            {
                                if (results[i] != 0)
                                {
                                    failureSummary = names[i];
                                    return -1;
                                }
                            }
                        }
                    }

                    // 5) InputStage + OutputStage (동시)
                    {
                        token.ThrowIfCancellationRequested();

                        var tasks = new List<Task<int>>();
                        var names = new List<string>();

                        if (InputStage != null)
                        {
                            tasks.Add(Task.Run(() => InputStage.MoveToStageReadyPosition(), token));
                            names.Add("InputStage");
                        }
                        if (OutputStage != null)
                        {
                            tasks.Add(Task.Run(() => OutputStage.MoveToStageReadyPosition(), token));
                            names.Add("OutputStage");
                        }

                        if (tasks.Count > 0)
                        {
                            var results = await Task.WhenAll(tasks).ConfigureAwait(false);
                            for (int i = 0; i < results.Length; i++)
                            {
                                if (results[i] != 0)
                                {
                                    failureSummary = names[i];
                                    return -1;
                                }
                            }
                        }
                    }

                    return 0; // 모두 OK
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                    failureSummary = "Ready 이동 중 예외";
                    return -1;
                }
            }, token);

            var form = new ProgressForm("Manual Running", "MoveUnitsToReady", t, null);
            try
            {
                form.ShowDialog();

                if (form.DialogResult == DialogResult.Cancel)
                {
                    // 사용자가 Stop을 눌렀을 가능성 → 상위 토큰 취소 시도(있으면)
                    try { _homeCts?.Cancel(); } catch { }
                    return -1;
                }

                if (t.IsFaulted)
                {
                    var mb = new MessageBoxOk();
                    mb.ShowDialog("Ready 이동 중 예외!", t.Exception?.GetBaseException().Message);
                    return -1;
                }

                if (t.IsCanceled)
                {
                    throw new OperationCanceledException();
                }

                var rc = await t.ConfigureAwait(true);
                if (rc != 0)
                {
                    MessageBox.Show("Ready 이동 실패: " + (failureSummary ?? string.Empty),
                        "Ready 실패", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                return rc;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                MessageBox.Show("Ready 이동 처리 중 예외: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
        }

        // === HomeAll: Entrypoint ===
        private async void btnHomeAll_Click(object sender, EventArgs e)
        {
            try
            {
                // 1) 사용자 확인
                if (!ConfirmHome()) 
                    return;

                // 2) 취소 토큰 준비
                var token = PrepareNewHomeToken();

                // 3) 축 알람 초기화 및 서보 ON
                if (!TryServoOnAllAxes()) 
                    return;

                // 4) 홈 시퀀스 실행(다이얼로그 포함)
                var rcHome = await RunHomeSequenceWithDialogAsync(token).ConfigureAwait(true);
                if (rcHome != 0)
                {
                    // 필요시 조기 종료하려면 return; 하세요.
                    MessageBox.Show("Home 실패 또는 미완료 항목이 있습니다.", "Home 실패", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 6) 전 유닛 Ready 이동 → int 반환
                var rcReady = await MoveUnitsToReadyAsync(token).ConfigureAwait(true);
                if (rcReady != 0)
                {
                    //MessageBox.Show("일부 유닛 Ready 이동 실패", "Ready 실패", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 5) Rotary InitializeAfterHome → int 반환
                var rcRot = await RunRotaryInitializeAfterHomeAsync(token).ConfigureAwait(true);
                if (rcRot != 0)
                {
                    MessageBox.Show("실패", "실패", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return; // 실패 시 종료
                }

                // 7) 결과 표시
                MessageBox.Show("초기화 완료", "Home");
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("Home 취소됨");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Home 오류: " + ex.Message);
            }
        }


        private MeasurementResultForm _measurementResultForm;
        private void BtnMeasurementResult_Click(object sender, EventArgs e)
        {
            if (_measurementResultForm == null || _measurementResultForm.IsDisposed)
                _measurementResultForm = new MeasurementResultForm();

            if (!_measurementResultForm.Visible)
                _measurementResultForm.Show();
            else
                _measurementResultForm.BringToFront();

            _measurementResultForm.Focus();
            _measurementResultForm.TopMost = true;
            _measurementResultForm.TopMost = false; // 잠깐 TopMost로 올렸다가 해제 (앞으로 Bring)
        }
    }
}
