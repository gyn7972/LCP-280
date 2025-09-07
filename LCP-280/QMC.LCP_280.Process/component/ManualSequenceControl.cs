using System;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using QMC.Common.Sequence;
using QMC.LCP_280.Process.Unit;
using System.Collections.Generic;
using System.Linq;

namespace QMC.LCP_280.Process.Sequences
{
    /// <summary>
    /// Generic manual sequence control supporting multiple sequence registration.
    /// - Register multiple SequenceBase instances with a key/name.
    /// - ComboBox to select active sequence, then existing single-step/manual functions apply.
    /// </summary>
    public partial class ManualSequenceControl : UserControl
    {
        // Generic sequence instance (currently selected)
        private SequenceBase _sequence;

        // Registered sequences
        private readonly Dictionary<string, SequenceBase> _sequences = new Dictionary<string, SequenceBase>(StringComparer.OrdinalIgnoreCase);

        // Delegates for customization per sequence key (optional)
        private readonly Dictionary<string, Func<string[]>> _stepNameProviders = new Dictionary<string, Func<string[]>>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Func<string, bool>> _startSingleHandlers = new Dictionary<string, Func<string, bool>>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Func<int, string>> _indexNameResolvers = new Dictionary<string, Func<int, string>>(StringComparer.OrdinalIgnoreCase);

        // Active delegates (bound to current _sequence)
        private Func<string[]> _getStepNames;            // Step 이름 목록 공급자
        private Func<string, bool> _startSingle;         // 단일 Step 실행 (true=성공)
        private Func<int, string> _stepIndexToName;      // 실행 중 StepIndex → StepName 매핑

        private bool _initialized;
        private bool _runtimeInitTried;
        private bool _stepListInitialized;

        private const string StepPrefix = "[STEP]";

        #region Design Mode Helper
        private bool IsDesign
        {
            get
            {
                if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return true;
                if (DesignMode) return true;
                var svc = GetService(typeof(System.ComponentModel.Design.IDesignerHost));
                return svc != null;
            }
        }
        #endregion

        public ManualSequenceControl()
        {
            InitializeComponent();
            if (IsDesign) TryPopulateDesignTimeSample();
            WireUiEvents();
        }

        private void WireUiEvents()
        {
            try { _cboSequence.SelectedIndexChanged += (s, e) => OnSequenceSelectionChanged(); } catch { }
        }

        #region Multi Sequence API
        /// <summary>
        /// 모든 등록된 시퀀스 제거.
        /// </summary>
        public void ClearSequences()
        {
            DetachSequenceEvents();
            foreach (var kv in _sequences) { try { kv.Value.Dispose(); } catch { } }
            _sequences.Clear();
            _stepNameProviders.Clear();
            _startSingleHandlers.Clear();
            _indexNameResolvers.Clear();
            _sequence = null;
            _cboSequence?.Items.Clear();
            _lstStep?.Items.Clear();
            UpdateButtonStates();
        }

        /// <summary>
        /// 시퀀스 등록 (UI combo에 표시). key가 null/중복이면 무시.
        /// </summary>
        public void RegisterSequence(string key, SequenceBase sequence,
            Func<string[]> stepNameProvider = null,
            Func<string, bool> startSingleHandler = null,
            Func<int, string> stepIndexNameResolver = null,
            bool autoSelect = false)
        {
            if (string.IsNullOrWhiteSpace(key) || sequence == null) return;
            if (_sequences.ContainsKey(key)) return;
            _sequences[key] = sequence;
            if (stepNameProvider != null) _stepNameProviders[key] = stepNameProvider;
            if (startSingleHandler != null) _startSingleHandlers[key] = startSingleHandler;
            if (stepIndexNameResolver != null) _indexNameResolvers[key] = stepIndexNameResolver;
            _cboSequence.Items.Add(key);
            if (autoSelect || _cboSequence.SelectedIndex < 0)
                _cboSequence.SelectedItem = key;
        }

        /// <summary>
        /// 등록된 시퀀스 해제.
        /// </summary>
        public void UnregisterSequence(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return;
            if (_sequences.TryGetValue(key, out var seq))
            {
                if (ReferenceEquals(seq, _sequence))
                {
                    DetachSequenceEvents();
                    _sequence = null;
                }
                try { seq.Dispose(); } catch { }
            }
            _sequences.Remove(key);
            _stepNameProviders.Remove(key);
            _startSingleHandlers.Remove(key);
            _indexNameResolvers.Remove(key);
            _cboSequence.Items.Remove(key);
            if (_cboSequence.SelectedIndex < 0 && _cboSequence.Items.Count > 0)
                _cboSequence.SelectedIndex = 0;
            else
                RebindActiveFromCombo();
        }

        private void OnSequenceSelectionChanged()
        {
            RebindActiveFromCombo();
        }

        private void RebindActiveFromCombo()
        {
            string sel = _cboSequence.SelectedItem as string;
            if (string.IsNullOrEmpty(sel) || !_sequences.TryGetValue(sel, out var seq))
            {
                DetachSequenceEvents();
                _sequence = null;
                _getStepNames = null; _startSingle = null; _stepIndexToName = null;
                _lstStep.Items.Clear();
                UpdateButtonStates();
                return;
            }

            // 이전 시퀀스 이벤트 제거 후 새로 바인딩
            DetachSequenceEvents();
            _sequence = seq;
            _getStepNames = _stepNameProviders.TryGetValue(sel, out var p) ? p : null;
            _startSingle = _startSingleHandlers.TryGetValue(sel, out var s) ? s : null;
            _stepIndexToName = _indexNameResolvers.TryGetValue(sel, out var r) ? r : null;
            WireSequenceEvents();

            _stepListInitialized = false;
            PopulateStepList();
            UpdateButtonStates();
        }
        #endregion

        #region Legacy Single Sequence API (Backward compatibility)
        /// <summary>
        /// 기존 API: 하나만 세팅. 내부적으로 key="(default)" 등록.
        /// </summary>
        public void SetSequence(SequenceBase sequence,
            Func<string[]> stepNameProvider = null,
            Func<string, bool> startSingleHandler = null,
            Func<int, string> stepIndexNameResolver = null)
        {
            ClearSequences();
            RegisterSequence("(default)", sequence, stepNameProvider, startSingleHandler, stepIndexNameResolver, autoSelect: true);
        }
        #endregion

        #region Populate Step List
        private void PopulateStepList()
        {
            if (IsDesign) return; // 디자인 시 샘플 메서드 사용
            if (_stepListInitialized) return;
            try
            {
                _lstStep.Items.Clear();
                var names = GetStepNamesInternal();
                if (names != null)
                {
                    foreach (var s in names)
                        _lstStep.Items.Add($"{StepPrefix} {s}");
                }
                _lstStep.Items.Add("--------------------------------");
                _lstStep.Items.Add("단일 Step 선택 후 Manual Action → 해당 Step 1회 실행");
                _stepListInitialized = true;
            }
            catch { }
        }

        private string[] GetStepNamesInternal()
        {
            // 1) delegate 우선
            if (_getStepNames != null)
            {
                try { return _getStepNames(); } catch { }
            }
            // 2) 시퀀스 타입에 public static string[] GetStepNames() 있으면 호출
            if (_sequence != null)
            {
                var t = _sequence.GetType();
                var mi = t.GetMethod("GetStepNames", BindingFlags.Public | BindingFlags.Static);
                if (mi != null && mi.ReturnType == typeof(string[]))
                {
                    try { return (string[])mi.Invoke(null, null); } catch { }
                }
                // 3) 내부 nested enum Step 추출
                var stepEnum = t.GetNestedType("Step", BindingFlags.Public | BindingFlags.NonPublic);
                if (stepEnum != null && stepEnum.IsEnum)
                {
                    try { return Enum.GetNames(stepEnum); } catch { }
                }
            }
            return new string[0];
        }
        #endregion

        #region Single Step Helpers
        private bool TryGetSelectedSingleStep(out string stepName)
        {
            stepName = null;
            var sel = _lstStep.SelectedItem as string;
            if (string.IsNullOrWhiteSpace(sel)) return false;
            if (!sel.StartsWith(StepPrefix)) return false;
            stepName = sel.Substring(StepPrefix.Length).Trim();
            return !string.IsNullOrEmpty(stepName);
        }
        #endregion

        #region Lifecycle / Auto Init (Backward Compatibility)
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (IsDesign) return;
            if (_runtimeInitTried) return;
            _runtimeInitTried = true;
            // 이전 버전 호환: 아무것도 등록 안 되어 있으면 InputStage 자동 등록 시도
            if (_sequences.Count == 0)
                TryAutoInitInputStageSequence();
            PopulateStepList();
            UpdateButtonStates();
        }

        private void TryAutoInitInputStageSequence()
        {
            try
            {
                if (!Equipment.Instance.Units.TryGetValue("InputStage", out var baseUnit)) return;
                var unit = baseUnit as InputStage; if (unit == null) return;
                var asm = typeof(InputStage).Assembly;
                var seqType = asm.GetType("QMC.LCP_280.Process.Component.SeqInputStage")
                              ?? asm.GetType("QMC.LCP_280.Process.Component.Seq_InputStage");
                if (seqType == null) return;
                var create = seqType.GetMethod("CreateFromUnit", BindingFlags.Public | BindingFlags.Static);
                object seqObj = null;
                if (create != null)
                    seqObj = create.Invoke(null, new object[] { unit });
                else
                    seqObj = Activator.CreateInstance(seqType, nonPublic: true);
                if (!(seqObj is SequenceBase seqBase)) return;

                Func<string, bool> startSingle = null;
                var miStartSingle = seqType.GetMethod("StartSingle", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (miStartSingle != null && miStartSingle.ReturnType == typeof(bool) && miStartSingle.GetParameters().Length == 1)
                    startSingle = (name) => { try { return (bool)miStartSingle.Invoke(seqBase, new object[] { name }); } catch { return false; } };
                Func<int, string> indexToName = idx =>
                {
                    try
                    {
                        var stepEnum = seqType.GetNestedType("Step", BindingFlags.NonPublic | BindingFlags.Public);
                        if (stepEnum != null && stepEnum.IsEnum && Enum.IsDefined(stepEnum, idx))
                            return Enum.GetName(stepEnum, idx);
                    }
                    catch { }
                    return null;
                };
                Func<string[]> getNames = () => GetStepNamesViaReflection(seqType) ?? new string[0];
                RegisterSequence("InputStage", seqBase, getNames, startSingle, indexToName, autoSelect: true);
            }
            catch { }
        }

        private static string[] GetStepNamesViaReflection(Type seqType)
        {
            try
            {
                var mi = seqType.GetMethod("GetStepNames", BindingFlags.Public | BindingFlags.Static);
                if (mi != null && mi.ReturnType == typeof(string[]))
                    return (string[])mi.Invoke(null, null);
                var stepEnum = seqType.GetNestedType("Step", BindingFlags.Public | BindingFlags.NonPublic);
                if (stepEnum != null && stepEnum.IsEnum) return Enum.GetNames(stepEnum);
            }
            catch { }
            return null;
        }
        #endregion

        #region Sequence Events
        private void WireSequenceEvents()
        {
            if (_sequence == null) return;
            _sequence.StateChanged += OnSequenceStateChanged;
            _sequence.StepChanged += OnSequenceStepChanged;
            _sequence.ErrorOccurred += OnSequenceError;
            _sequence.Completed += OnSequenceCompleted;
        }
        private void DetachSequenceEvents()
        {
            if (_sequence == null) return;
            _sequence.StateChanged -= OnSequenceStateChanged;
            _sequence.StepChanged -= OnSequenceStepChanged;
            _sequence.ErrorOccurred -= OnSequenceError;
            _sequence.Completed -= OnSequenceCompleted;
        }

        private void OnSequenceStateChanged(SequenceBase seq, SequenceState oldS, SequenceState newS) => SafeUI(UpdateButtonStates);
        private void OnSequenceStepChanged(SequenceBase seq, int step) => SafeUI(() => HighlightRunningStep(step));
        private void OnSequenceError(SequenceBase seq, Exception ex) => SafeUI(UpdateButtonStates);
        private void OnSequenceCompleted(SequenceBase seq) => SafeUI(UpdateButtonStates);
        #endregion

        #region Button Actions
        private void OnManualClick(object sender, EventArgs e)
        {
            if (IsDesign) return;
            if (_sequence == null) return;

            // 실행 전 사용자 확인
            try
            {
                var selKey = _cboSequence != null ? _cboSequence.SelectedItem as string : null;
                var msg = "어떤 시퀀스 진행하시겠습니까?" + (string.IsNullOrWhiteSpace(selKey) ? "" : "\r\n현재 선택: " + selKey);
                var dr = MessageBox.Show(msg, "시퀀스 실행 확인", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr != DialogResult.Yes) return;
            }
            catch { }

            if (_sequence.IsRunning || _sequence.IsPaused)
            {
                _sequence.Stop();
                return;
            }

            if (_startSingle != null && TryGetSelectedSingleStep(out var singleStep))
            {
                if (_startSingle(singleStep)) UpdateButtonStates();
                return;
            }

            _sequence.Start();
            UpdateButtonStates();
        }

        private void OnStopClick(object sender, EventArgs e)
        { if (IsDesign) return; _sequence?.Stop(); }
        private void OnRecoverClick(object sender, EventArgs e)
        { if (IsDesign) return; _sequence?.Recover(); }
        #endregion

        #region UI Helpers
        private void UpdateButtonStates()
        {
            if (IsDesign)
            {
                _btnManual.Enabled = true; _btnStop.Enabled = true; _btnRecover.Enabled = true; return;
            }
            if (_sequence == null)
            {
                _btnManual.Enabled = false; _btnStop.Enabled = false; _btnRecover.Enabled = false; return;
            }
            _btnStop.Enabled = _sequence.IsRunning || _sequence.IsPaused || _sequence.IsError;
            _btnRecover.Enabled = _sequence.IsError;
            _btnManual.Enabled = !_sequence.IsRunning && !_sequence.IsPaused && !_sequence.IsError;
        }

        private void HighlightRunningStep(int stepIndex)
        {
            string name = null;
            if (_stepIndexToName != null)
            {
                try { name = _stepIndexToName(stepIndex); } catch { name = null; }
            }
            if (string.IsNullOrEmpty(name) && _sequence != null)
            {
                try
                {
                    var t = _sequence.GetType();
                    var stepEnum = t.GetNestedType("Step", BindingFlags.NonPublic | BindingFlags.Public);
                    if (stepEnum != null && stepEnum.IsEnum && Enum.IsDefined(stepEnum, stepIndex))
                        name = Enum.GetName(stepEnum, stepIndex);
                }
                catch { }
            }
            if (string.IsNullOrEmpty(name)) return;

            for (int i = 0; i < _lstStep.Items.Count; i++)
            {
                var item = _lstStep.Items[i] as string;
                if (item != null && item.StartsWith(StepPrefix))
                {
                    var nm = item.Substring(StepPrefix.Length).Trim();
                    if (string.Equals(nm, name, StringComparison.OrdinalIgnoreCase))
                    {
                        _lstStep.SelectedIndex = i;
                        break;
                    }
                }
            }
        }

        private void TryPopulateDesignTimeSample()
        {
            try
            {
                _lstStep.Items.Clear();
                _lstStep.Items.Add("[DESIGN] Step preview");
                foreach (var s in new[] { "Init", "Move", "Action" }) _lstStep.Items.Add($"{StepPrefix} {s}");
                _lstStep.Items.Add("--------------------------------");
                _lstStep.Items.Add("단일 Step 선택 후 Manual 실행");
                UpdateButtonStates();
            }
            catch { }
        }

        private void SafeUI(Action a)
        { if (IsDisposed) return; try { if (InvokeRequired) BeginInvoke(a); else a(); } catch { } }
        #endregion

        private void _panelButtons_Paint(object sender, PaintEventArgs e) { }
    }
}
