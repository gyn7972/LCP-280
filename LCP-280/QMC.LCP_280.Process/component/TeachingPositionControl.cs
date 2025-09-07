using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using QMC.Common;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Unit;

namespace QMC.LCP_280.Process.Component
{
    /// <summary>
    /// 공용 Teaching Position 뷰/편집/이동 컨트롤.
    /// - 런타임에 다수 Unit 등록/해제(RegisterUnit / UnregisterUnit / ClearUnits)
    /// - 각 Unit 별 TeachingPosition 제공자 + Move 실행 delegate 사용
    /// - 기존 SetUnits(InputStage, InputStageEjector, InputDieTransfer) 호환 유지 (내부 RegisterUnit 호출)
    /// - UnitName 지정 시 단일 필터링, null이면 전체
    /// </summary>
    public partial class TeachingPositionControl : UserControl
    {
        #region 필드 / 구조
        private Equipment Equipment => Equipment.Instance;

        // 구(旧) 고정 Unit 참조 (SetUnits 하위 호환용)
        private InputStage _inputStage;
        private InputStageEjector _ejector;
        private InputDieTransfer _dieTransfer;

        // 외부 주입 여부
        private bool _externalUnitsProvided;

        // 일반화: UnitKey -> Provider / Move / Save / UnitRef
        private readonly Dictionary<string, Func<IEnumerable<TeachingPosition>>> _tpProviders
            = new Dictionary<string, Func<IEnumerable<TeachingPosition>>>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Action<string, double>> _moveExecutors
            = new Dictionary<string, Action<string, double>>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Action<TeachingPosition>> _saveExecutors
            = new Dictionary<string, Action<TeachingPosition>>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, BaseUnit> _unitRefs
            = new Dictionary<string, BaseUnit>(StringComparer.OrdinalIgnoreCase);

        private class TPEntry
        {
            public string Display;
            public string UnitKey;
            public string PositionName;
            public TeachingPosition TP;
        }
        private readonly List<TPEntry> _entries = new List<TPEntry>();
        private readonly Dictionary<string, TPEntry> _displayToEntry =
            new Dictionary<string, TPEntry>(StringComparer.OrdinalIgnoreCase);

        private bool _initialized;
        private string _unitName = null; // null => multi (전체)
        #endregion

        #region 프로퍼티
        [Browsable(true), Category("Teaching"), Description("단일 UnitName 지정 시 해당 Unit만 표시, 비우면 전체")]
        public string UnitName
        {
            get => _unitName;
            set
            {
                _unitName = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
                ReloadTeachingPositions();
            }
        }
        #endregion

        #region 생성 / 초기화
        public TeachingPositionControl()
        {
            InitializeComponent();
            if (!DesignMode)
                InitializeRuntime();
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            if (!DesignMode) InitializeRuntime();
        }

        private void InitializeRuntime()
        {
            if (_initialized) return;
            _initialized = true;

            if (!_externalUnitsProvided)
                TryBindDefaultUnits(); // 초기 자동 바인딩(기본 3종)

            WireEvents();
            InitMoveMode();
            AdjustButtonRow(); // ensure layout reflects initial visibility
            ReloadTeachingPositions();
        }
        #endregion

        #region Public API (일반화)
        /// <summary>
        /// 모든 등록 Unit/Delegate 초기화. (UI 초기화 후 다시 RegisterUnit 호출)
        /// </summary>
        public void ClearUnits()
        {
            _tpProviders.Clear();
            _moveExecutors.Clear();
            _saveExecutors.Clear();
            _unitRefs.Clear();
            _externalUnitsProvided = true; // 자동 바인딩 비활성
            _unitName = null;
            _entries.Clear();
            _displayToEntry.Clear();
            positionItemView?.SetItems("(Teaching Position 없음)");
        }

        /// <summary>
        /// Unit 등록. (provider 또는 mover 가 null이면 스킵)
        /// saveAction 없으면 기본 reflection 저장 로직 시도.
        /// </summary>
        public void RegisterUnit(
            string unitKey,
            BaseUnit unit,
            Func<IEnumerable<TeachingPosition>> provider,
            Action<string, double> moveAction,
            Action<TeachingPosition> saveAction = null,
            bool autoReload = true)
        {
            if (string.IsNullOrWhiteSpace(unitKey)) return;
            if (provider == null || moveAction == null) return;
            unitKey = unitKey.Trim();

            _tpProviders[unitKey] = provider;
            _moveExecutors[unitKey] = moveAction;
            if (saveAction != null)
                _saveExecutors[unitKey] = saveAction;
            _unitRefs[unitKey] = unit;

            _externalUnitsProvided = true;
            if (autoReload && _initialized)
                ReloadTeachingPositions();
        }

        /// <summary>
        /// Unit 해제
        /// </summary>
        public void UnregisterUnit(string unitKey, bool autoReload = true)
        {
            if (string.IsNullOrWhiteSpace(unitKey)) return;
            _tpProviders.Remove(unitKey);
            _moveExecutors.Remove(unitKey);
            _saveExecutors.Remove(unitKey);
            _unitRefs.Remove(unitKey);
            if (autoReload)
                ReloadTeachingPositions();
        }

        /// <summary>
        /// 기존 Input 전용 SetUnits → 내부에서 RegisterUnit 호출 (하위 호환)
        /// </summary>
        public void SetUnits(InputStage stage, InputStageEjector ejector, InputDieTransfer dieTransfer, bool reload = true)
        {
            ClearUnits();

            _inputStage = stage;
            _ejector = ejector;
            _dieTransfer = dieTransfer;

            if (stage != null)
            {
                RegisterUnit("InputStage",
                    stage,
                    () => stage.InputStageConfig?.TeachingPositions,
                    (name, vel) => stage.MoveToTeachingPosition(name, vel: vel),
                    tp => stage.InputStageConfig?.SetTeachingPosition(tp),
                    false);
            }
            if (ejector != null)
            {
                RegisterUnit("InputStageEjector",
                    ejector,
                    () => ejector.InputStageEjectorConfig?.TeachingPositions,
                    (name, vel) => ejector.MoveToTeachingPosition(name, vel: vel),
                    tp => ejector.InputStageEjectorConfig?.SetTeachingPosition(tp),
                    false);
            }
            if (dieTransfer != null)
            {
                RegisterUnit("InputDieTransfer",
                    dieTransfer,
                    () => dieTransfer.InputDieTransferConfig?.TeachingPositions,
                    (name, vel) => dieTransfer.MoveToTeachingPosition(name, vel: vel),
                    tp => dieTransfer.InputDieTransferConfig?.SetTeachingPosition(tp),
                    false);
            }

            if (_initialized && reload)
                ReloadTeachingPositions();
        }
        #endregion

        #region Public API (Button Visibility)
        [Browsable(true), Category("Teaching"), Description("Save 버튼 표시 여부")]
        public bool ShowSaveButton { get => btnSave?.Visible ?? false; set { if (btnSave != null) { btnSave.Visible = value; AdjustButtonRow(); } } }
        [Browsable(true), Category("Teaching"), Description("Cancel 버튼 표시 여부")]
        public bool ShowCancelButton { get => btnCancel?.Visible ?? false; set { if (btnCancel != null) { btnCancel.Visible = value; AdjustButtonRow(); } } }
        /// <summary>
        /// Save / Cancel 버튼 가시성 일괄 설정
        /// </summary>
        public void SetSaveCancelVisible(bool showSave, bool showCancel)
        {
            if (btnSave != null) btnSave.Visible = showSave;
            if (btnCancel != null) btnCancel.Visible = showCancel;
            AdjustButtonRow();
        }

        private void AdjustButtonRow()
        {
            try
            {
                if (rightPanel == null || rightPanel.RowCount < 3) return; // designer table
                bool any = (btnSave?.Visible ?? false) || (btnCancel?.Visible ?? false);
                var style = rightPanel.RowStyles[1];
                if (!any)
                {
                    // collapse
                    style.Height = 0f;
                    style.SizeType = System.Windows.Forms.SizeType.Absolute;
                    flowButtonsPanel.Visible = false;
                }
                else
                {
                    style.Height = 50f; // original height
                    style.SizeType = System.Windows.Forms.SizeType.Absolute;
                    flowButtonsPanel.Visible = true;
                }
                rightPanel.PerformLayout();
                rightPanel.Invalidate();
            }
            catch { }
        }
        #endregion

        #region 내부 기본(초기) 자동 바인딩
        private void TryBindDefaultUnits()
        {
            try
            {
                if (Equipment?.Units == null) return;

                BaseUnit unit; // ConcurrentDictionary<string, BaseUnit> 에 맞는 형식
                if (Equipment.Units.TryGetValue("InputStage", out unit))
                    _inputStage = unit as InputStage;
                if (Equipment.Units.TryGetValue("InputStageEjector", out unit))
                    _ejector = unit as InputStageEjector;
                if (Equipment.Units.TryGetValue("InputDieTransfer", out unit))
                    _dieTransfer = unit as InputDieTransfer;

                // 자동 바인딩도 일반 Register 경로 사용
                if (_inputStage != null || _ejector != null || _dieTransfer != null)
                    SetUnits(_inputStage, _ejector, _dieTransfer, false);
            }
            catch { }
        }
        #endregion

        #region 이벤트 / UI
        private void WireEvents()
        {
            btnMovePosition.Click += (s, e) => MoveSelected();
            btnSave.Click += (s, e) => SaveCurrentEditingPosition();
            btnCancel.Click += (s, e) => CancelEdit();

            var ev = positionItemView.GetType().GetEvent("ItemSelected");
            if (ev != null)
            {
                EventHandler<int> handler = (s, idx) => OnPositionSelected(idx);
                ev.AddEventHandler(positionItemView, handler);
            }
        }

        private void InitMoveMode()
        {
            rdoFine.Checked = true;
            rdoCoarse.Checked = false;
        }

        private double GetJogVelocityFactor()
            => rdoFine.Checked ? 1.0 : 5.0;
        #endregion

        #region TeachingPosition 로드/Move
        private void ReloadTeachingPositions()
        {
            if (!_initialized || DesignMode) return;

            _entries.Clear();
            _displayToEntry.Clear();

            try
            {
                IEnumerable<string> keys;
                if (_unitName != null)
                    keys = _tpProviders.Keys.Where(k => k.Equals(_unitName, StringComparison.OrdinalIgnoreCase));
                else
                    keys = _tpProviders.Keys;

                foreach (var key in keys)
                {
                    if (!_tpProviders.TryGetValue(key, out var provider)) continue;
                    IEnumerable<TeachingPosition> list = null;
                    try { list = provider?.Invoke(); } catch { }
                    if (list == null) continue;

                    foreach (var tp in list)
                    {
                        if (tp == null || string.IsNullOrWhiteSpace(tp.Name)) continue;
                        var disp = key + "." + tp.Name;
                        var entry = new TPEntry
                        {
                            Display = disp,
                            UnitKey = key,
                            PositionName = tp.Name,
                            TP = tp
                        };
                        _entries.Add(entry);
                        _displayToEntry[disp] = entry;
                    }
                }

                if (_entries.Count == 0)
                {
                    positionItemView?.SetItems("(Teaching Position 없음)");
                }
                else
                {
                    var arr = _entries.Select(e => e.Display)
                                      .OrderBy(s => s)
                                      .ToArray();
                    positionItemView?.SetItems(arr);
                    SafeUI(() => positionItemView.SelectedIndex = 0);
                }
            }
            catch (Exception ex)
            {
                try { Log.Write("TeachingPositionControl", "Reload", ex.Message); } catch { }
            }
        }

        private void MoveSelected()
        {
            try
            {
                var display = positionItemView?.SelectedItemName;
                if (string.IsNullOrEmpty(display)) return;
                if (!_displayToEntry.TryGetValue(display, out var entry)) return;

                // 이동 확인 메시지
                var msg = $"{entry.Display} 위치로 이동하시겠습니까?";
                if (MessageBox.Show(msg, "Move Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;

                if (_moveExecutors.TryGetValue(entry.UnitKey, out var mover))
                    mover(entry.PositionName, GetJogVelocityFactor());
            }
            catch (Exception ex)
            {
                try { Log.Write("TeachingPositionControl", "Move", ex.Message); } catch { }
            }
        }
        #endregion

        #region 선택/에디터 로드/저장
        private void OnPositionSelected(int index)
        {
            try
            {
                var display = positionItemView?.SelectedItemName;
                if (string.IsNullOrEmpty(display)) return;
                if (_displayToEntry.TryGetValue(display, out var entry))
                    LoadTeachingPositionToEditor(entry.TP);
            }
            catch { }
        }

        private void LoadTeachingPositionToEditor(TeachingPosition tp)
        {
            if (tp == null || positionEditorView == null) return;
            try
            {
                var pcType = typeof(QMC.Common.PropertyCollection);
                var pc = Activator.CreateInstance(pcType);
                var addMi = pcType.GetMethod("Add", new[] { typeof(string), typeof(object) });
                if (addMi != null && tp.AxisPositions != null)
                {
                    foreach (var kv in tp.AxisPositions)
                        addMi.Invoke(pc, new object[] { kv.Key, kv.Value });
                }
                var setMi = positionEditorView.GetType().GetMethod("SetProperties",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                setMi?.Invoke(positionEditorView, new object[] { pc });
            }
            catch (Exception ex)
            {
                try { Log.Write("TeachingPositionControl", "EditorLoad", ex.Message); } catch { }
            }
        }

        private void SaveCurrentEditingPosition()
        {
            try
            {
                var display = positionItemView?.SelectedItemName;
                if (string.IsNullOrEmpty(display)) return;
                if (!_displayToEntry.TryGetValue(display, out var entry) || entry.TP == null) return;

                // 에디터 값 반영
                var getMi = positionEditorView.GetType().GetMethod("GetCurrentProperties",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                var pc = getMi?.Invoke(positionEditorView, null);
                if (pc != null)
                {
                    var pcType = pc.GetType();
                    var getValueMi = pcType.GetMethod("GetValue");
                    if (getValueMi != null && entry.TP.AxisPositions != null)
                    {
                        var keys = entry.TP.AxisPositions.Keys.ToList();
                        foreach (var axisKey in keys)
                        {
                            try
                            {
                                var generic = getValueMi.MakeGenericMethod(typeof(double));
                                var valObj = generic.Invoke(pc, new object[] { axisKey });
                                if (valObj is double d)
                                    entry.TP.AxisPositions[axisKey] = d;
                            }
                            catch { }
                        }
                    }
                }

                // 1) 사용자 정의 saveAction 우선
                if (_saveExecutors.TryGetValue(entry.UnitKey, out var saver))
                {
                    saver(entry.TP);
                }
                else
                {
                    // 2) reflection 저장 (UnitKey + "Config" 안에 SetTeachingPosition 함수 찾기)
                    if (_unitRefs.TryGetValue(entry.UnitKey, out var unit) && unit != null)
                    {
                        // <UnitKey>Config 프로퍼티 추정
                        var cfgProp = unit.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                            .FirstOrDefault(p =>
                                p.CanRead &&
                                p.Name.EndsWith("Config", StringComparison.OrdinalIgnoreCase) &&
                                p.PropertyType.GetProperties().Any(pp => pp.Name == "TeachingPositions"));

                        var cfgObj = cfgProp?.GetValue(unit);
                        if (cfgObj != null)
                        {
                            var setMi2 = cfgObj.GetType().GetMethod("SetTeachingPosition",
                                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                                null, new[] { typeof(TeachingPosition) }, null);
                            if (setMi2 != null)
                                setMi2.Invoke(cfgObj, new object[] { entry.TP });
                        }
                    }
                }

                Log.Write("TeachingPositionControl", "Save", $"{display} 저장 완료");
            }
            catch (Exception ex)
            {
                try { Log.Write("TeachingPositionControl", "SaveError", ex.Message); } catch { }
            }
        }

        private void CancelEdit()
        {
            try
            {
                var display = positionItemView?.SelectedItemName;
                if (string.IsNullOrEmpty(display)) return;
                if (_displayToEntry.TryGetValue(display, out var entry))
                    LoadTeachingPositionToEditor(entry.TP);
            }
            catch { }
        }
        #endregion

        #region Refresh / SafeUI
        public void RefreshData()
        {
            // 외부 등록 없다면 기존 자동 바인딩(초기 상태) 유지
            if (!_externalUnitsProvided)
                TryBindDefaultUnits();

            ReloadTeachingPositions();
        }

        private void SafeUI(Action a)
        {
            if (IsDisposed) return;
            try
            {
                if (InvokeRequired) BeginInvoke(a);
                else a();
            }
            catch { }
        }
        #endregion
    }
}
