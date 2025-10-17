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
using static QMC.LCP_280.Process.Unit.FormMain.SequenceAutoControl;
using static QMC.LCP_280.Process.Unit.FormMain.SequenceManualControl;

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

        private Form _lightControlPopup = null;

        public Operator_Main() : this(
            TryGetUnit<InputFeeder>("InputFeeder"),
            TryGetUnit<InputDieTransfer>("InputDieTransfer"),
            TryGetUnit<Rotary>("Rotary"),
            TryGetUnit<OutputDieTransfer>("OutputDieTransfer"),
            TryGetUnit<OutputFeeder>("OutputFeeder"),
            TryGetUnit<InputStage>("InputStage"),
            TryGetUnit<IndexUnloadAligner>("IndexUnloadAligner"),
            TryGetUnit<OutputStage>("OutputStage"),
            TryGetUnit<InputCassetteLifter>("InputCassetteLifter"),
            TryGetUnit<IndexLoadAligner>("IndexLoadAligner"),
            TryGetUnit<IndexChipProbeController>("IndexChipProbeController"),
            TryGetUnit<OutputCassetteLifter>("OutputCassetteLifter"),
            TryGetUnit<InputStageEjector>("InputStageEjector"))
        {
        }

        public Operator_Main(InputFeeder inputFeeder, InputDieTransfer inputDieTransfer, Rotary rotary,
                            OutputDieTransfer outputDieTransfer, OutputFeeder outputFeeder,
                            InputStage inputStage, IndexUnloadAligner indexUnloadAligner, OutputStage outputStage,
                            InputCassetteLifter inputCassetteLifter, IndexLoadAligner indexLoadAligner,
                            IndexChipProbeController indexChipProbeController, OutputCassetteLifter outputCassetteLifter,
                            InputStageEjector inputStageEjector)
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

            _readySequences = new HashSet<string>();
            _startSequences = new HashSet<string>();

            Load += Vision_Manual_Load;

            sequenceAutoControl.SequenceButtonRequested += OnAutoSequenceButtonRequested;
            sequenceManualControl.SequenceButtonRequested += OnManualSequenceButtonRequested;

            InputWaferCamera.LightControlRequested += LightControlRequested;
            IndexOutputCamera.LightControlRequested += LightControlRequested;
            OutputWaferCamera.LightControlRequested += LightControlRequested;
        }

        #region Form Cleanup
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (sequenceAutoControl != null)
                sequenceAutoControl.SequenceButtonRequested -= OnAutoSequenceButtonRequested;
            if (sequenceManualControl != null)
                sequenceManualControl.SequenceButtonRequested -= OnManualSequenceButtonRequested;

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
            bool canStart = state == EquipmentState.Running || state == EquipmentState.Ready;
            bool canStop = state == EquipmentState.Running || state == EquipmentState.Starting;

            //sequenceAutoControl.SetButtonEnabled("Ready", canReady);
            //sequenceAutoControl.SetButtonEnabled("Start", canStart);
            //sequenceAutoControl.SetButtonEnabled("Stop", canStop);
            //sequenceAutoControl.SetButtonEnabled("CycleStop", canStop);
            //sequenceAutoControl.SetButtonEnabled("Reset", state != EquipmentState.Starting); // 예시

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

        #region Auto Sequence 처리
        private void OnAutoSequenceButtonRequested(object sender, AutoSequenceEventArgs e)
        {
            Log.Write("Operator_Main", $"Auto Sequence {e.Command} 요청");
            switch (e.Command)
            {
                case "Ready":
                    HandleAutoReady();
                    break;

                case "Start":
                    HandleAutoStart(); // 설비 전체 Start 위임
                    break;

                case "Stop":
                    HandleAutoStop();  // 설비 전체 Stop 위임
                    break;

                case "CycleStop":
                    HandleAutoCycleStop();
                    break;

                case "Reset":
                    HandleAutoReset();
                    break;
            }
        }

        private void HandleAutoReady()
        {
            _autoReady = !_autoReady;
            NotifyAutoSequenceStateChanged("Ready", _autoReady);

            if (_autoReady)
            {
                _autoReadyCts?.Cancel();
                _autoReadyCts = new CancellationTokenSource();
                var ct = _autoReadyCts.Token;
                var prev = Cursor.Current;
                Cursor.Current = Cursors.WaitCursor;
                ExecuteAutoReadyAsync(ct).ContinueWith(t =>
                {
                    try
                    {
                        if (IsDisposed || Disposing) return;
                        BeginInvoke(new Action(() =>
                        {
                            Cursor.Current = prev;
                            if (t.IsFaulted)
                                Log.Write("Operator_Main", $"Auto Ready 예외: {t.Exception?.GetBaseException().Message}");
                            if (t.IsCanceled)
                                Log.Write("Operator_Main", "Auto Ready 취소됨");
                        }));
                    }
                    catch { }
                });
                Log.Write("Operator_Main", "Auto Ready ON");
            }
            else
            {
                _autoReadyCts?.Cancel();
                Log.Write("Operator_Main", "Auto Ready OFF");
            }
        }

        private async void HandleAutoStart()
        {
            var eq = Equipment.Instance;
            if (eq == null) return;

            try
            {
                // UI 토글 알림(즉시 반영), 최종 상태는 Eq.StateChanged에서 수렴
                NotifyAutoSequenceStateChanged("Start", true);

                // 설비 전체 시작
                var ok = await eq.StartAllUnitsAsync().ConfigureAwait(true);
                if (!ok)
                {
                    NotifyAutoSequenceStateChanged("Start", false);
                    MessageBox.Show("설비 시작 실패", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    _autoStarting = false;
                    return;
                }

                // 성공 시 내부 플래그 정리
                _autoReady = false;
                _autoStarting = true;
                Log.Write("Operator_Main", "Auto Start 완료 (Equipment.StartAllUnitsAsync)");
            }
            catch (Exception ex)
            {
                NotifyAutoSequenceStateChanged("Start", false);
                _autoStarting = false;
                Log.Write(ex);
                MessageBox.Show($"설비 시작 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            //if (!_autoStarting)
            //{
            //    if (!_autoReady)
            //    {
            //        MessageBox.Show("Auto Ready를 먼저 실행해주세요.");
            //        return;
            //    }
            //    _autoReady = false;
            //    _autoStarting = true;
            //    NotifyAutoSequenceStateChanged("Ready", false);
            //    NotifyAutoSequenceStateChanged("Start", true);
            //    ExecuteAutoStart();
            //    Log.Write("Operator_Main", "Auto Start 실행 (Ready OFF)");
            //}
            //else
            //{
            //    _autoStarting = false;
            //    NotifyAutoSequenceStateChanged("Start", false);
            //    Log.Write("Operator_Main", "Auto Start OFF");
            //}
        }

        private async void HandleAutoStop()
        {
            var eq = Equipment.Instance;
            if (eq == null) return;

            try
            {
                NotifyAutoSequenceStateChanged("Stop", true);

                // 로컬 시퀀스 토글/상태 정리(UI만)
                _autoReady = false;
                _autoStarting = false;
                _readySequences.Clear();
                _startSequences.Clear();
                try { sequenceAutoControl.ResetAllButtons(); } catch { }
                try { sequenceManualControl.ResetAllButtons(); } catch { }

                // 설비 전체 정지
                var ok = await eq.StopAllUnitsAsync().ConfigureAwait(true);
                if (!ok)
                {
                    MessageBox.Show("설비 정지 실패(일부 유닛 타임아웃 가능)", "오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                Log.Write("Operator_Main", "Auto Stop 완료 (Equipment.StopAllUnitsAsync)");
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                MessageBox.Show($"설비 정지 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // 최종 UI 토글 해제, 실제 상태는 Eq.StateChanged에서 최종 수렴
                NotifyAutoSequenceStateChanged("Stop", false);
            }

            //_autoReady = false;
            //_autoStarting = false;
            //_readySequences.Clear();
            //_startSequences.Clear();
            //sequenceAutoControl.ResetAllButtons();
            //sequenceManualControl.ResetAllButtons();
            //NotifyAutoSequenceStateChanged("Stop", true);
            //Task.Delay(500).ContinueWith(_ =>
            //{
            //    this.Invoke(new Action(() => { NotifyAutoSequenceStateChanged("Stop", false); }));
            //});
            //ExecuteAutoStop();
            //Log.Write("Operator_Main", "Auto Stop 실행 - 모든 Sequence 초기화");
        }

        private void HandleAutoCycleStop()
        {
            NotifyAutoSequenceStateChanged("CycleStop", true);
            Task.Delay(500).ContinueWith(_ =>
            {
                this.Invoke(new Action(() => { NotifyAutoSequenceStateChanged("CycleStop", false); }));
            });
            ExecuteAutoCycleStop();
            Log.Write("Operator_Main", "Auto CycleStop 실행");
        }

        private void HandleAutoReset()
        {
            NotifyAutoSequenceStateChanged("Reset", true);
            Task.Delay(500).ContinueWith(_ =>
            {
                this.Invoke(new Action(() => { NotifyAutoSequenceStateChanged("Reset", false); }));
            });
            ExecuteAutoReset();
            Log.Write("Operator_Main", "Auto Reset 실행");
        }

        private void NotifyAutoSequenceStateChanged(string command, bool isActive)
        {
            sequenceAutoControl.OnAutoSequenceStateChanged(new AutoSequenceStateChangedEventArgs
            {
                Command = command,
                IsActive = isActive
            });
        }
        #endregion

        #region Manual Sequence 처리
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


        // 시퀀스명 → Unit 매핑 (새 함수 1개만 추가)
        private BaseUnit GetUnitForSequence(string sequenceName)
        {
            switch (sequenceName)
            {
                case "InputWafer": return InputFeeder;
                case "ChipLoading": return InputDieTransfer;
                case "Process": return Rotary;
                case "ChipUnloading": return OutputDieTransfer;
                case "OutputWafer": return OutputFeeder;
            }
            return null;
        }
        private async void HandleStartAction(string sequenceName)
        {
            // Start ON → OFF (Stop)
            if (_startSequences.Contains(sequenceName))
            {
                _startSequences.Remove(sequenceName);
                NotifySequenceStateChanged(sequenceName, "Start", false, true);
                Log.Write("Operator_Main", $"{sequenceName} Start OFF (Stop 요청)");
                var unit = GetUnitForSequence(sequenceName);
                if (unit != null)
                {
                    try { await Equipment.Instance.StopUnitAsync(unit.UnitName); } catch { }
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

            // 공통 StartSequenceAsync 사용
            bool ok = await StartSequenceAsync(sequenceName, CancellationToken.None);
            if (!ok)
            {
                _startSequences.Remove(sequenceName);
                NotifySequenceStateChanged(sequenceName, "Start", false, true);
                Log.Write("Operator_Main", $"{sequenceName} Start 실패");
            }
        }
        //private void HandleStartAction(string sequenceName)
        //{
        //    bool isStarting = _startSequences.Contains(sequenceName);
        //    if (isStarting)
        //    {
        //        _startSequences.Remove(sequenceName);
        //        NotifySequenceStateChanged(sequenceName, "Start", false, true);
        //        Log.Write("Operator_Main", $"{sequenceName} Start OFF");
        //    }
        //    else
        //    {
        //        if (!_readySequences.Contains(sequenceName))
        //        {
        //            MessageBox.Show($"{sequenceName}를 먼저 Ready 상태로 만들어주세요.");
        //            return;
        //        }
        //        _readySequences.Remove(sequenceName);
        //        _startSequences.Add(sequenceName);
        //        NotifySequenceStateChanged(sequenceName, "Ready", false, false);
        //        NotifySequenceStateChanged(sequenceName, "Start", true, true);
        //        ExecuteSequence(sequenceName, "Start");
        //        Log.Write("Operator_Main", $"{sequenceName} Start ON (Ready OFF)");
        //    }
        //}

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
        #endregion

        #region 비즈니스 로직
        private CancellationTokenSource _autoReadyCts;
        private async Task ExecuteAutoReadyAsync(CancellationToken ct)
        {
            Log.Write("Operator_Main", "Auto Ready 시작 (공통 로직 사용)");
            bool ok = await ReadyAllSequencesAsync(ct);
            if (ok) Log.Write("Operator_Main", "Auto Ready 완료 (모든 시퀀스 Ready ON)");
            else Log.Write("Operator_Main", "Auto Ready 실패");
        }

        private async void ExecuteAutoStart()
        {
            // Auto Start 도 Manual Start 와 동일한 흐름 (Ready 자동 보정 + Start 전환)
            var cts = new CancellationTokenSource();
            try
            {
                Log.Write("Operator_Main", "Auto Start 시작 (공통 로직 사용)");
                bool ok = await StartAllSequencesAsync(cts.Token);
                if (ok) Log.Write("Operator_Main", "Auto Start 완료 - 모든 Sequence Start ON");
                else Log.Write("Operator_Main", "Auto Start 실패");
            }
            catch (OperationCanceledException)
            {
                Log.Write("Operator_Main", "Auto Start 취소됨");
            }
        }

        private async void ExecuteAutoStop()
        {
            await StopAllSequencesAsync(); // UI & HashSet
            var equipment = Equipment.Instance;
            try { await equipment.StopAllUnitsAsync(); } catch { }
            Log.Write("Operator_Main", "Auto Stop 완료 (공통 로직)");
        }

        private void ExecuteAutoCycleStop()
        {
        }

        private void ExecuteAutoReset()
        {
            InputCassetteLifter.SetMaterial(new Material());
            InputFeeder.SetMaterial(new Material());
            InputStage.SetMaterial(new Material());
            InputDieTransfer.SetMaterial(new Material());
            Rotary.SetMaterial(new Material());
            IndexLoadAligner.SetMaterial(new Material());
            IndexChipProbeController.SetMaterial(new Material());
            IndexUnloadAligner.SetMaterial(new Material());
            OutputDieTransfer.SetMaterial(new Material());
            OutputStage.SetMaterial(new Material());
            OutputFeeder.SetMaterial(new Material());
            OutputCassetteLifter.SetMaterial(new Material());
        }

        private void ExecuteSequence(string sequenceName, string action)
        {
            var sequenceHandlers = new Dictionary<string, Action<string>>
            {
                { "InputWafer", HandleInputWafer },
                { "ChipLoading", HandleChipLoading },
                { "Process", HandleProcess },
                { "ChipUnloading", HandleChipUnloading },
                { "OutputWafer", HandleOutputWafer }
            };
            if (sequenceHandlers.TryGetValue(sequenceName, out var handler))
                handler(action);
        }

        private CancellationTokenSource _readyCts;
        private bool _readyBusy;

        private async Task<bool> TryReadySequenceAsync(string sequenceName, CancellationToken ct)
        {
            int rc;
            switch (sequenceName)
            {
                case "InputWafer": rc = await HandleInputWaferReadyAsync(ct); break;
                case "ChipLoading": rc = await HandleChipLoadingReadyAsync(ct); break;
                case "Process": rc = await HandleProcessReadyAsync(ct); break;
                case "ChipUnloading": rc = await HandleChipUnloadingReadyAsync(ct); break;
                case "OutputWafer": rc = await HandleOutputWaferReadyAsync(ct); break;
                default:
                    Log.Write("Operator_Main", $"알 수 없는 Sequence '{sequenceName}' Ready 요청");
                    return false;
            }
            if (rc != 0)
            {
                Log.Write("Operator_Main", $"{sequenceName} Ready 실패(rc={rc})");
                return false;
            }
            return true;
        }

        #region Handle Manual Async Ready
        private Task<int> HandleInputWaferReadyAsync(CancellationToken ct)
        {
            return Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();
                int nRet = InputFeeder?.EnsureReady() ?? -1;
                if (nRet != 0) return nRet;
                ct.ThrowIfCancellationRequested();
                nRet = InputStageEjector?.CheckReady() ?? -1;
                return nRet;
            }, ct);
        }
        private Task<int> HandleChipLoadingReadyAsync(CancellationToken ct)
        {
            return Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();
                return InputDieTransfer?.EnsureReady() ?? -1;
            }, ct);
        }
        private Task<int> HandleProcessReadyAsync(CancellationToken ct)
        {
            return Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();
                int nRet = IndexLoadAligner?.EnsureReady() ?? -1;
                if (nRet != 0) return nRet;
                ct.ThrowIfCancellationRequested();
                nRet = IndexChipProbeController?.EnsureReady() ?? -1;
                return nRet;
            }, ct);
        }
        private Task<int> HandleChipUnloadingReadyAsync(CancellationToken ct)
        {
            return Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();
                return OutputDieTransfer?.EnsureReady() ?? -1;
            }, ct);
        }
        private Task<int> HandleOutputWaferReadyAsync(CancellationToken ct)
        {
            return Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();
                return OutputFeeder?.EnsureReady() ?? -1;
            }, ct);
        }
        #endregion

        // 공통 Start
        private async Task<bool> TryStartUnitAsync(BaseUnit unit)
        {
            if (unit == null) return false;
            try
            {
                var eq = Equipment.Instance;
                if (eq == null)
                {
                    MessageBox.Show("Equipment 인스턴스가 초기화되지 않았습니다.", "오류",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                var unitName = unit.UnitName;
                if (string.IsNullOrEmpty(unitName))
                {
                    MessageBox.Show("UnitName 이 비어있습니다.", "오류",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                if (unit.RunUnitStatus == BaseUnit.UnitStatus.Running)
                {
                    MessageBox.Show($"Unit '{unitName}' 는 이미 실행 중입니다.", "정보",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return true;
                }

                var prev = Cursor.Current;
                Cursor.Current = Cursors.WaitCursor;
                try
                {
                    bool ok = await eq.StartUnitAsync(unitName).ConfigureAwait(true);
                    if (!ok)
                    {
                        MessageBox.Show($"Unit '{unitName}' 시작 실패.", "오류",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            return true;
        }

        private Task<bool> HandleInputStart() => TryStartUnitAsync(InputFeeder);
        private Task<bool> HandleChipLoadingStart() => TryStartUnitAsync(InputDieTransfer);
        private Task<bool> HandleProcessStart() => TryStartUnitAsync(Rotary);
        private Task<bool> HandleChipUnloadingStart() => TryStartUnitAsync(OutputDieTransfer);
        private Task<bool> HandleOutputWaferStart() => TryStartUnitAsync(OutputFeeder);
        #endregion

        #region Sequence Action Handlers (Ready 분기 비동기 방식으로 교체)
        private async void HandleInputWafer(string action)
        {
            if (action == "Ready")
            {
                Log.Write("Operator_Main", "InputWafer Ready 위치로 이동");
                int rc = await HandleInputWaferReadyAsync(CancellationToken.None);
                if (rc != 0) return;
            }
            else if (action == "Start")
            {
                Log.Write("Operator_Main", "InputWafer 시퀀스 실행");
                if (!await HandleInputStart()) return;
            }
        }

        private async void HandleChipLoading(string action)
        {
            if (action == "Ready")
            {
                Log.Write("Operator_Main", "ChipLoading Ready");
                int rc = await HandleChipLoadingReadyAsync(CancellationToken.None);
                if (rc != 0) return;
            }
            else if (action == "Start")
            {
                Log.Write("Operator_Main", "ChipLoading Start");
                if (!await HandleChipLoadingStart()) return;
            }
        }

        private async void HandleProcess(string action)
        {
            if (action == "Ready")
            {
                Log.Write("Operator_Main", "Process Ready");
                int rc = await HandleProcessReadyAsync(CancellationToken.None);
                if (rc != 0) return;
            }
            else if (action == "Start")
            {
                Log.Write("Operator_Main", "Process Start");
                if (!await HandleProcessStart()) return;
            }
        }

        private async void HandleChipUnloading(string action)
        {
            if (action == "Ready")
            {
                Log.Write("Operator_Main", "ChipUnloading Ready");
                int rc = await HandleChipUnloadingReadyAsync(CancellationToken.None);
                if (rc != 0) return;
            }
            else if (action == "Start")
            {
                Log.Write("Operator_Main", "ChipUnloading Start");
                if (!await HandleChipUnloadingStart()) return;
            }
        }

        private async void HandleOutputWafer(string action)
        {
            if (action == "Ready")
            {
                Log.Write("Operator_Main", "OutputWafer Ready 위치로 이동");
                int rc = await HandleOutputWaferReadyAsync(CancellationToken.None);
                if (rc != 0) return;
            }
            else if (action == "Start")
            {
                Log.Write("Operator_Main", "OutputWafer 시퀀스 실행");
                if (!await HandleOutputWaferStart()) return;
            }
        }
        #endregion

        // ====== (추가/수정 시작) : Manual / Auto 시퀀스 공통화를 위한 최소 공통 유틸 ======

        #region Unified Sequence Helpers

        // 실행 순서(필요하면 Config 로 대체 가능)
        private static readonly string[] _sequenceOrder =
        {
            "InputWafer","ChipLoading","Process","ChipUnloading","OutputWafer"
        };

        // (이미 존재) 시퀀스 → Unit 매핑 재사용
        // private BaseUnit GetUnitForSequence(string sequenceName) { ... }

        // Start 공통 (Manual/Auto 공용) : Start 토글 전용 내부 호출용
        private async Task<bool> StartSequenceAsync(string sequenceName, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var unit = GetUnitForSequence(sequenceName);
            if (unit == null)
            {
                Log.Write("Operator_Main", $"StartSequenceAsync 실패 - Unit 매핑 없음: {sequenceName}");
                return false;
            }
            // 이미 Running 이면 성공 간주
            if (unit.RunUnitStatus == BaseUnit.UnitStatus.Running)
                return true;

            // 1) Start 시도
            var started = await TryStartUnitAsync(unit).ConfigureAwait(true);
            if (!started)
            {
                Log.Write("Operator_Main", $"StartSequenceAsync 실패(시작 요청 실패): {sequenceName}");
                return false;
            }

            // 2) Running 전이 대기 (타임아웃 내 폴링)
            const int timeoutMs = 5000; // 필요 시 설정값으로 분리
            var ok = await WaitForUnitRunningAsync(unit, timeoutMs, ct).ConfigureAwait(true);
            if (!ok)
            {
                Log.Write("Operator_Main", $"StartSequenceAsync 실패(Running 전이 타임아웃): {sequenceName}");
                return false;
            }

            return true;
        }
        // Unit Running 대기 유틸
        private async Task<bool> WaitForUnitRunningAsync(BaseUnit unit, int timeoutMs, CancellationToken ct)
        {
            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                ct.ThrowIfCancellationRequested();
                if (unit.RunUnitStatus == BaseUnit.UnitStatus.Running || unit.IsRunning)
                    return true;

                await Task.Delay(100, ct).ConfigureAwait(true);
            }
            return unit.RunUnitStatus == BaseUnit.UnitStatus.Running || unit.IsRunning;
        }

        // Ready 공통 (Manual/Auto 공용) : 실패 시 false
        private async Task<bool> ReadySequenceAsync(string sequenceName, CancellationToken ct)
        {
            if (_readySequences.Contains(sequenceName))
                return true; // 이미 Ready

            bool ok = await TryReadySequenceAsync(sequenceName, ct);
            if (ok)
            {
                _readySequences.Add(sequenceName);
                NotifySequenceStateChanged(sequenceName, "Ready", true, false);
            }
            return ok;
        }

        // 모든 시퀀스 Ready (Auto 전용이지만 Manual 과 동일 로직 재사용)
        private async Task<bool> ReadyAllSequencesAsync(CancellationToken ct)
        {
            foreach (var seq in _sequenceOrder)
            {
                if (!await ReadySequenceAsync(seq, ct))
                {
                    Log.Write("Operator_Main", $"Auto Ready 중단 - {seq} 실패");
                    return false;
                }
            }
            return true;
        }

        // 모든 시퀀스 Start (Auto 전용이지만 Manual 과 동일 로직 재사용)
        private async Task<bool> StartAllSequencesAsync(CancellationToken ct)
        {
            foreach (var seq in _sequenceOrder)
            {
                ct.ThrowIfCancellationRequested();

                // Auto 에서는 Ready 안 되어 있으면 자동 Ready 수행
                if (!_readySequences.Contains(seq))
                {
                    if (!await ReadySequenceAsync(seq, ct))
                    {
                        Log.Write("Operator_Main", $"Auto Start 중 Ready 실패 - {seq}");
                        return false;
                    }
                }

                // Manual StartAction 과 동일한 상태 전환 (Ready OFF → Start ON)
                _readySequences.Remove(seq);
                NotifySequenceStateChanged(seq, "Ready", false, false);

                if (_startSequences.Contains(seq))
                    continue; // 이미 Start ON

                _startSequences.Add(seq);
                NotifySequenceStateChanged(seq, "Start", true, true);

                if (!await StartSequenceAsync(seq, ct))
                {
                    // 실패 시 UI 복구
                    _startSequences.Remove(seq);
                    NotifySequenceStateChanged(seq, "Start", false, true);
                    Log.Write("Operator_Main", $"Auto Start 실패 - {seq}");
                    return false;
                }
                Log.Write("Operator_Main", $"Auto Start OK - {seq}");
            }
            return true;
        }

        // 모든 시퀀스 Stop (Manual 토글과 동일 논리)
        private async Task StopAllSequencesAsync()
        {
            var eq = Equipment.Instance;
            foreach (var seq in _sequenceOrder)
            {
                if (_startSequences.Contains(seq))
                {
                    _startSequences.Remove(seq);
                    NotifySequenceStateChanged(seq, "Start", false, true);
                    var u = GetUnitForSequence(seq);
                    try { if (u != null) await eq.StopUnitAsync(u.UnitName); } catch { }
                }
                if (_readySequences.Contains(seq))
                {
                    _readySequences.Remove(seq);
                    NotifySequenceStateChanged(seq, "Ready", false, false);
                }
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

        private async void btnHomeAll_Click(object sender, EventArgs e)
        {
            HomeProgressForm dlg = null;
            try
            {
                // 홈 진행 전 사용자 확인
                var ask = new MessageBoxYesNo();
                if (ask.ShowDialog("확인", "축 홈을 진행하시겠습니까?") != DialogResult.Yes)
                    return;

                _homeCts?.Cancel();
                _homeCts?.Dispose();
                _homeCts = new CancellationTokenSource();
                var token = _homeCts.Token;
                //var axes = _axisManager?.GetAll();
                var axes = _Equipment.AxisManager?.GetAll();
                if (axes == null || axes.Length == 0)
                {
                    MessageBox.Show("등록된 축이 없습니다.");
                    return;
                }
                foreach (var ax in axes)
                {
                    try { ax.ClearAlarm(); } catch { }
                    try { ax.Servo(true); } catch { }
                }
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
                        MessageBox.Show("Home 취소됨");
                        return;
                    }
                }

                var results = await runTask.ConfigureAwait(true);
                dlg.SafeUpdate(new OperationProgress { OperationId = "HOME", Title = "Home", StepIndex = seq.TotalSteps - 1, TotalSteps = seq.TotalSteps, IsCompleted = true, IsCanceled = token.IsCancellationRequested, IsAborted = seq.Aborted, Message = seq.AbortReason });
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
                            if (r.Error != null && r.Error.Message != null)
                            {
                                status = r.Error.Message;
                            }
                            else
                            {
                                status = "rc=" + r.ReturnCode;
                            }
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

                Task<int> t = Rotary.RunManualFunction(Rotary.InitializeAfterHome);
                ProgressForm form = new ProgressForm("Manual Running", nameof(Rotary.InitializeAfterHome), t, this.Rotary);
                if (t != null)
                {
                    try
                    {
                        form.ShowDialog();
                        if (form.DialogResult == DialogResult.Cancel)
                        {
                            this.Rotary.CancelSequence();
                            MessageBox.Show("Rotary InitializeAfterHome 실패", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            // 취소는 정상 흐름으로 처리: 실패 메시지 표시하지 않음
                            return;
                        }
                        else if (t.IsFaulted)
                        {
                            // 예외 메시지 표시
                            var mb = new MessageBoxOk();
                            mb.ShowDialog("Manual Run Error!", t.Exception?.GetBaseException().Message);

                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                    }
                }
                // 여기서 정상적으로 종료 후에... 초기화 완료 후 추가 시컨스 적용해야 한다...
                //int nRet = Rotary.InitializeAfterHome();   // Index Clear 위치로 한 스텝씩 이동하면서 기존에 있는 제품 제거.
                //if(nRet != 0)
                //{
                //    MessageBox.Show("Rotary InitializeAfterHome 실패", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //}

                MessageBox.Show(msg, "Home");
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("Home 취소됨");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Home 오류: " + ex.Message);
            }
            finally
            {
                try 
                { 
                    dlg?.Close(); 
                    dlg?.Dispose(); 
                } 
                catch { }
            }
        }
    }
}
