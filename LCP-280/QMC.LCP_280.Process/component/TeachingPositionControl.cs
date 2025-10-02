using QMC.Common;
using QMC.Common.Component;
using QMC.Common.Unit;
using QMC.LCP_280.Process;
using QMC.LCP_280.Process.Unit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Component
{
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
        // Always show flag (default false so caller can decide per form)
        private bool _alwaysShowSaveCancel = false; // 기본 false (폼별 제어)
        private bool _postCreateReapplyPending = false;

        // === New layout customization fields ===
        public enum ButtonAlignMode { Left, Center, Right, Stretch }
        private bool _autoLayoutButtons = true;
        private ButtonAlignMode _buttonAlignment = ButtonAlignMode.Center;
        private int _buttonSpacing = 12;
        private Size _buttonSize = new Size(90, 32);

        [Browsable(true), Category("Teaching Buttons"), Description("자동 중앙/정렬 등 내부 레이아웃 사용 여부. False면 디자이너에서 위치 수동 조정 가능")] 
        [DefaultValue(true)]
        public bool AutoLayoutButtons
        {
            get => _autoLayoutButtons;
            set { if (_autoLayoutButtons == value) return; _autoLayoutButtons = value; UpdateButtonPanelLayout(); }
        }

        [Browsable(true), Category("Teaching Buttons"), Description("Save/Cancel 수평 정렬 방식")] 
        [DefaultValue(ButtonAlignMode.Center)]
        public ButtonAlignMode ButtonAlignment
        {
            get => _buttonAlignment;
            set { if (_buttonAlignment == value) return; _buttonAlignment = value; UpdateButtonPanelLayout(); }
        }

        [Browsable(true), Category("Teaching Buttons"), Description("Save 와 Cancel 사이 간격(px)")] 
        [DefaultValue(12)]
        public int ButtonSpacing
        {
            get => _buttonSpacing;
            set { _buttonSpacing = Math.Max(0, value); ApplyButtonSizeAndSpacing(); UpdateButtonPanelLayout(); }
        }

        [Browsable(true), Category("Teaching Buttons"), Description("Save/Cancel 버튼 크기")] 
        public Size ButtonSize
        {
            get => _buttonSize;
            set { if (value.Width > 10 && value.Height > 10) { _buttonSize = value; ApplyButtonSizeAndSpacing(); UpdateButtonPanelLayout(); } }
        }

        private void ApplyButtonSizeAndSpacing()
        {
            try
            {
                if (btnSave != null) { btnSave.Size = _buttonSize; btnSave.Margin = new Padding(0, 0, _buttonSpacing, 0); }
                if (btnCancel != null) { btnCancel.Size = _buttonSize; btnCancel.Margin = new Padding(0); }
            }
            catch { }
        }

        // ===== Adjust helpers early definition so calls compile =====
        private void AdjustButtonRow(bool force = false)
        {
            try
            {
                if (rightPanel == null) return;
                if (rightPanel.RowStyles == null || rightPanel.RowStyles.Count < 3)
                {
                    if (!force) return;
                    while (rightPanel.RowStyles.Count < 3)
                        rightPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 33));
                }
                if (_alwaysShowSaveCancel)
                {
                    if (btnSave != null) btnSave.Visible = true;
                    if (btnCancel != null) btnCancel.Visible = true;
                    if (flowButtonsPanel != null) flowButtonsPanel.Visible = true;
                }
                bool any = (btnSave?.Visible ?? false) || (btnCancel?.Visible ?? false);
                var style = rightPanel.RowStyles[1];
                if (!any)
                {
                    style.Height = 0f; style.SizeType = SizeType.Absolute; if (flowButtonsPanel != null) flowButtonsPanel.Visible = false;
                }
                else
                {
                    style.Height = 50f; style.SizeType = SizeType.Absolute; if (flowButtonsPanel != null) { flowButtonsPanel.Visible = true; UpdateButtonPanelLayout(); }
                }
                rightPanel.PerformLayout();
                flowButtonsPanel?.PerformLayout();
                Invalidate();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TPC:{Name}] AdjustButtonRow error: {ex.Message}");
            }
            finally
            {
                DumpDeep("AdjustButtonRow");
            }
        }
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

        [Browsable(true)]
        [Category("Teaching")]
        [Description("Save/Cancel 버튼을 항상 강제 표시 (true이면 SetSaveCancelVisible 값 무시)")]
        [DefaultValue(false)] // 디자이너가 기본값(false)이면 코드 생성 안하도록
        public bool AlwaysShowSaveCancel
        {
            get => _alwaysShowSaveCancel;
            set
            {
                if (_alwaysShowSaveCancel == value) return;
                _alwaysShowSaveCancel = value;
                Console.WriteLine($"[TPC:{Name}] AlwaysShowSaveCancel set -> {_alwaysShowSaveCancel} (Created={IsHandleCreated}, btnSaveNull={btnSave==null})");
                if (IsHandleCreated)
                    ForceApplyAlwaysFlag();
                else
                    _postCreateReapplyPending = true;
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
            SetupButtonPanelStyle();
            InitMoveMode();
            AdjustButtonRow(); // ensure layout reflects initial visibility
            ReloadTeachingPositions();
        }

        private void SetupButtonPanelStyle()
        {
            try
            {
                if (flowButtonsPanel != null)
                {
                    flowButtonsPanel.Padding = new Padding(0);
                    flowButtonsPanel.Margin = new Padding(0);
                    flowButtonsPanel.AutoSize = false;
                    flowButtonsPanel.WrapContents = false;
                    flowButtonsPanel.FlowDirection = FlowDirection.LeftToRight;
                }
                ApplyButtonSizeAndSpacing();
            }
            catch { }
        }

        private void TryBindDefaultUnits()
        {
            try
            {
                if (Equipment?.Units == null) return;
                IUnit u;
                if (Equipment.Units.TryGetValue("InputStage", out u)) _inputStage = u as InputStage;
                if (Equipment.Units.TryGetValue("InputStageEjector", out u)) _ejector = u as InputStageEjector;
                if (Equipment.Units.TryGetValue("InputDieTransfer", out u)) _dieTransfer = u as InputDieTransfer;
                if (_inputStage != null || _ejector != null || _dieTransfer != null)
                    SetUnits(_inputStage, _ejector, _dieTransfer, false);
            }
            catch { }
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
            // signature kept the same; BaseUnit is still the common base class
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
        /// Unit 해제 - 내부 Equipment 인스턴스에 위임.
        /// (컨트롤에서 Equipment 내부 상태 컬렉션 직접 접근하지 않음)
        /// </summary>
        public bool UnregisterUnit(string unitName)
        {
            if (string.IsNullOrWhiteSpace(unitName)) return false;
            try
            {
                return Equipment.UnregisterUnit(unitName);
            }
            catch (Exception ex)
            {
                try { Log.Write("TeachingPositionControl", "UnregisterUnit", ex.Message); } catch { }
                return false;
            }
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
                    () => stage.Config.TeachingPositions,
                    (name, vel) => stage.MoveToTeachingPosition(name, vel: vel),
                    tp => stage.Config?.SetTeachingPosition(tp),
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
            if (_alwaysShowSaveCancel) { showSave = true; showCancel = true; }
            if (btnSave != null) btnSave.Visible = showSave;
            if (btnCancel != null) btnCancel.Visible = showCancel;
            if ((showSave || showCancel) && flowButtonsPanel != null) flowButtonsPanel.Visible = true;
            Console.WriteLine($"[TPC:{Name}] SetSaveCancelVisible -> save:{showSave}, cancel:{showCancel}, always:{_alwaysShowSaveCancel}");
            AdjustButtonRow();
            DumpDeep("SetSaveCancelVisible");
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            Console.WriteLine($"[TPC:{Name}] OnHandleCreated (postReapply={_postCreateReapplyPending}, always={_alwaysShowSaveCancel})");
            try { AdjustButtonRow(force: true); } catch { }
            if (_alwaysShowSaveCancel || _postCreateReapplyPending)
                BeginInvoke(new Action(ForceApplyAlwaysFlag));
        }

        private void ForceApplyAlwaysFlag()
        {
            try
            {
                _postCreateReapplyPending = false;
                if (btnSave != null) btnSave.Visible = true;
                if (btnCancel != null) btnCancel.Visible = true;
                if (flowButtonsPanel != null) flowButtonsPanel.Visible = true;
                AdjustButtonRow(force: true);
                DumpDeep("ForceApplyAlwaysFlag");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TPC:{Name}] ForceApplyAlwaysFlag error: {ex.Message}");
            }
        }

        // 강한 진단용 추가 상세 덤프
        private void DumpDeep(string reason)
        {
            try
            {
                Console.WriteLine($"[TPC:{Name}] ==== DumpDeep ({reason}) ====");
                Console.WriteLine($"  AlwaysShow={_alwaysShowSaveCancel}, Initialized={_initialized}, HandleCreated={IsHandleCreated}");
                Console.WriteLine($"  btnSave null={btnSave==null}, Visible={btnSave?.Visible}, Parent={btnSave?.Parent?.Name}");
                Console.WriteLine($"  btnCancel null={btnCancel==null}, Visible={btnCancel?.Visible}, Parent={btnCancel?.Parent?.Name}");
                Console.WriteLine($"  flowButtonsPanel null={flowButtonsPanel==null}, Visible={flowButtonsPanel?.Visible}, Parent={flowButtonsPanel?.Parent?.Name}");
                Console.WriteLine($"  rightPanel null={rightPanel==null}, RowStylesCount={rightPanel?.RowStyles?.Count}");
                if (rightPanel?.RowStyles != null)
                {
                    for (int i = 0; i < rightPanel.RowStyles.Count; i++)
                        Console.WriteLine($"   Row[{i}] SizeType={rightPanel.RowStyles[i].SizeType} Height={rightPanel.RowStyles[i].Height}");
                }
                Console.WriteLine("  Controls in flowButtonsPanel:");
                if (flowButtonsPanel != null)
                {
                    foreach (Control c in flowButtonsPanel.Controls)
                        Console.WriteLine($"    - {c.Name} Visible={c.Visible} Type={c.GetType().Name}");
                }
                Console.WriteLine("==============================");
            }
            catch { }
        }

        // Centering helper: 재계산하여 Save/Cancel 버튼을 가운데 정렬 (또는 선택한 Alignment)
        private void UpdateButtonPanelLayout()
        {
            try
            {
                if (flowButtonsPanel == null || rightPanel == null) return;
                if (!flowButtonsPanel.Visible) return;
                if (!_autoLayoutButtons) return; // 사용자 수동 배치 허용

                if (flowButtonsPanel.Dock != DockStyle.None)
                {
                    flowButtonsPanel.Dock = DockStyle.None;
                    flowButtonsPanel.Anchor = AnchorStyles.Top;
                }

                // 총 폭 계산
                int totalWidth = 0; int visibleCount = 0;
                foreach (Control c in flowButtonsPanel.Controls)
                {
                    if (!c.Visible) continue;
                    visibleCount++;
                    totalWidth += c.Width + c.Margin.Left + c.Margin.Right;
                }
                if (visibleCount == 0) return;
                if (totalWidth < 10) totalWidth = 10;
                int cellWidth = rightPanel.Width - 8;
                if (totalWidth > cellWidth) totalWidth = cellWidth - 4;
                flowButtonsPanel.Width = ( _buttonAlignment == ButtonAlignMode.Stretch ? cellWidth - 8 : totalWidth );

                int targetRowHeight = (rightPanel.RowStyles.Count > 1) ? (int)rightPanel.RowStyles[1].Height : flowButtonsPanel.Height;
                int desiredHeight = _buttonSize.Height + 2;
                flowButtonsPanel.Height = desiredHeight;

                // 수평 위치
                switch (_buttonAlignment)
                {
                    case ButtonAlignMode.Left:
                        flowButtonsPanel.Left = 4; break;
                    case ButtonAlignMode.Center:
                        flowButtonsPanel.Left = (cellWidth - totalWidth) / 2 + 4; break;
                    case ButtonAlignMode.Right:
                        flowButtonsPanel.Left = cellWidth - totalWidth + 4; break;
                    case ButtonAlignMode.Stretch:
                        flowButtonsPanel.Left = 4; break;
                }

                // Stretch 일 때 내부 버튼 가운데 정렬 (FlowLayoutPanel 자체 폭은 크게 유지)
                if (_buttonAlignment == ButtonAlignMode.Stretch)
                {
                    int free = flowButtonsPanel.Width - totalWidth;
                    int leftPad = free > 0 ? free / 2 : 0;
                    flowButtonsPanel.Padding = new Padding(leftPad, 0, 0, 0);
                }
                else
                {
                    flowButtonsPanel.Padding = new Padding(0);
                }

                flowButtonsPanel.Top = targetRowHeight > desiredHeight ? (targetRowHeight - desiredHeight) / 2 : 0;
            }
            catch { }
        }

        private void WireEvents()
        {
            btnMovePosition.Click += (s, e) => MoveSelected();
            btnSave.Click += (s, e) => SaveCurrentEditingPosition();
            btnCancel.Click += (s, e) => CancelEdit();

            // 사이즈 변경 시 버튼 재정렬
            if (rightPanel != null)
                rightPanel.SizeChanged += (s, e) => UpdateButtonPanelLayout();

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
                    if (!_tpProviders.TryGetValue(key, out var provider))
                        continue;
                    
                    IEnumerable<TeachingPosition> list = null;
                    
                    try 
                    { 
                        list = provider?.Invoke(); 
                    } 
                    catch { }
                    
                    if (list == null) 
                        continue;

                    foreach (var tp in list)
                    {
                        if (tp == null || string.IsNullOrWhiteSpace(tp.Name)) 
                            continue;

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
                try { Log.Write("TeachingPositionControl", "Reload", ex.Message); } 
                catch { }
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
                var ask = new MessageBoxYesNo();
                if (ask.ShowDialog("Move Confirm", $"{entry.Display} 위치로 이동하시겠습니까?") != DialogResult.Yes)
                {
                    return;
                }

                if (_moveExecutors.TryGetValue(entry.UnitKey, out var mover))
                {
                    mover(entry.PositionName, GetJogVelocityFactor());
                }
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

                // 올바른 시그니처: Add(string title, string valueUnit, object obj)
                var addMi = pcType.GetMethod("Add", new[] { typeof(string), typeof(string), typeof(object) });

                if (addMi != null && tp.AxisPositions != null)
                {
                    foreach (var kv in tp.AxisPositions)
                    {
                        // 중간에 valueUnit 파라미터 추가 (빈 문자열 또는 단위)
                        addMi.Invoke(pc, new object[] { kv.Key, "", kv.Value });
                    }
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
                if (string.IsNullOrEmpty(display))
                {
                    MessageBox.Show("선택된 Teaching Position이 없습니다.", "정보", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (!_displayToEntry.TryGetValue(display, out var entry) || entry.TP == null)
                {
                    MessageBox.Show("Teaching Position 데이터를 찾을 수 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 에디터가 없으면 저장 불가
                if (positionEditorView == null)
                {
                    MessageBox.Show("편집기를 찾을 수 없습니다. 편집 가능한 상태에서 다시 시도해 주세요.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    // 에디터 값 반영(가능할 때만)
                    object pc = null;
                    var getMi = positionEditorView.GetType().GetMethod("GetCurrentProperties",
                        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                    if (getMi != null)
                    {
                        pc = getMi.Invoke(positionEditorView, null);
                    }

                    if (pc != null && entry.TP.AxisPositions != null)
                    {
                        var pcType = pc.GetType();
                        var getValueMi = pcType.GetMethod("GetValue");
                        if (getValueMi != null)
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
                                catch
                                {
                                    // 개별 키 갱신 실패는 무시하고 계속
                                }
                            }
                        }
                    }
                }

                // 1) 사용자 지정 저장 로직이 있으면 우선
                if (_saveExecutors.TryGetValue(entry.UnitKey, out var saver))
                {
                    saver(entry.TP);
                }
                else
                {
                    // 2) 리플렉션 기반 기본 저장
                    if (_unitRefs.TryGetValue(entry.UnitKey, out var unit) && unit != null)
                    {
                        var cfgProp = unit.GetType()
                            .GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)
                            .FirstOrDefault(p =>
                                p.CanRead &&
                                p.Name.EndsWith("Config", StringComparison.OrdinalIgnoreCase) &&
                                p.PropertyType.GetProperties().Any(pp => pp.Name == "TeachingPositions"));

                        var cfgObj = cfgProp?.GetValue(unit);
                        if (cfgObj != null)
                        {
                            var setMi2 = cfgObj.GetType().GetMethod("SetTeachingPosition",
                                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic,
                                null, new[] { typeof(TeachingPosition) }, null);
                            setMi2?.Invoke(cfgObj, new object[] { entry.TP });
                        }
                    }
                }

                Log.Write("TeachingPositionControl", "Save", $"{display} 저장 완료");
            }
            catch (Exception ex)
            {
                try { Log.Write("TeachingPositionControl", "SaveError", ex.ToString()); } catch { }
                MessageBox.Show($"저장 중 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        // 상태 덤프용 헬퍼
        public void DumpState(string reason = null)
        {
            try
            {
                string r = string.IsNullOrEmpty(reason) ? "" : ("(" + reason + ") ");
                Console.WriteLine($"[TPC:{Name}] {r}AlwaysShow={_alwaysShowSaveCancel}, btnSaveVis={btnSave?.Visible}, btnCancelVis={btnCancel?.Visible}, Row1Height={(rightPanel?.RowStyles?.Count>1? rightPanel.RowStyles[1].Height: -1)}");
            }
            catch { }
        }
    }
}
