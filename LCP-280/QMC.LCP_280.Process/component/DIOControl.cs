using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using QMC.Common.IOUtil; // DIO, Cylinder, Vacuum
using QMC.Common.Unit;
using QMC.Common; // Log
using QMC.LCP_280.Process.Unit; // added for InputStage

namespace QMC.LCP_280.Process.Component
{
    /// <summary>
    /// 공용 DIO/Domain 제어 패널.
    /// - 각 Unit 인스턴스에서 Cylinder / Vacuum 필드를 리플렉션으로 검색하여 단동 동작 버튼 제공
    /// - Vacuum: ON/OFF 토글 버튼, IsOk() 상태 표시
    /// - Cylinder: Extend / Retract 버튼 + 센서(Up/Fwd / Down/Bwd) 상태 표시 (가능한 경우)
    /// - 주기적으로(기본 300ms) 상태 갱신
    /// 사용 절차:
    ///   dioControl.BindUnits(inputStage, outputStage, ringTransfer ...);
    ///   (ChipLoading_Working 등 폼에 올려 공용 사용)
    /// </summary>
    public partial class DIOControl : UserControl
    {
        private readonly List<BaseUnit> _units = new List<BaseUnit>();
        private readonly HashSet<string> _inputKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _outputKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> _displayMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase); // key -> label

        // 클래스 필드에 추가
        private readonly Dictionary<string, bool> _customOutputSoftStates = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);


        private Timer _refreshTimer;
        public int RefreshIntervalMs { get; set; } = 400;

        // Added for PropertyCollectionView support
        private readonly Dictionary<string, string> _dispToInputKey = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> _dispToOutputKey = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private PropertyCollection _lastInputPc;
        private PropertyCollection _lastOutputPc;
        private bool UsingPropertyViews => (inputView != null && outputView != null && inputView.GetType().Name.Contains("IOPropertyCollectionView"));

        // ⚡ 구조 동일 여부 빠른 판단용: 마지막 표시 키 순서 캐시
        private readonly List<string> _lastInputDispOrder = new List<string>();
        private readonly List<string> _lastOutputDispOrder = new List<string>();

        // 초기 표시 성능 모드: true면 Build 시 상태값 조회 생략(다음 타이머 틱에서 채움)
        [DefaultValue(true)]
        public bool FastInitialState { get; set; } = true;

        private ListBox _fallbackInput;
        private ListBox _fallbackOutput;

        private readonly Dictionary<string, Func<bool>> _customInputStates = new Dictionary<string, Func<bool>>();
        private readonly Dictionary<string, (Action on, Action off, Func<bool> state)> _customOutputActions = new Dictionary<string, (Action on, Action off, Func<bool> state)>();
        private int _customInSeq = 0;
        private int _customOutSeq = 0;
        // NEW: external display key mapping for custom IO
        private readonly Dictionary<string, string> _customInputDisplayKeys = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> _customOutputDisplayKeys = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private readonly List<string> _inputSequence = new List<string>();
        private readonly List<string> _outputSequence = new List<string>();

        // Maintain insertion order for custom IO (Dictionary in .NET Framework 4.8 does not guarantee insertion order)
        private readonly List<string> _customInputSequence = new List<string>();
        private readonly List<string> _customOutputSequence = new List<string>();

        public enum SortingMode
        {
            Insertion,
            AlphabeticalKey,
            AlphabeticalLabel
        }

        public SortingMode IoSortMode { get; set; } = SortingMode.AlphabeticalKey;

        public DIOControl()
        {
            InitializeComponent();
            InitTimer();
        }

        private void InitTimer()
        {
            _refreshTimer = new Timer();
            _refreshTimer.Interval = RefreshIntervalMs;
            _refreshTimer.Tick += (s, e) => SafeRefreshStates();
            _refreshTimer.Start();
        }

        #region Public API
        public void BindUnits(params BaseUnit[] units)
        {
            if (!IsHandleCreated)
            {
                HandleCreated += (s, e) => BindUnits(units);
                return;
            }
            _units.Clear();
            _inputKeys.Clear();
            _outputKeys.Clear();
            _displayMap.Clear();
            _customInputStates.Clear();
            _customOutputActions.Clear();
            _inputSequence.Clear();
            _outputSequence.Clear();
            _customInputSequence.Clear();
            _customOutputSequence.Clear();
            if (units != null)
                foreach (var u in units.Where(x => x != null)) _units.Add(u);
            ScanUnits();
            PopulateListViews(); // fallback (ListBox) 용
            BuildPropertyCollections(); // IOPropertyCollectionView 용
            SafeRefreshStates();
        }

        // === Simple delegate binding (custom, non DIO key) ===
        public void BindDIOInput(Func<bool> stateFunc, string label = "Input", string displayKey = null)
        {
            if (stateFunc == null) return;
            var key = "__custom_in_" + (++_customInSeq);
            _customInputStates[key] = stateFunc;
            _displayMap[key] = label;
            _customInputSequence.Add(key);
            if (!string.IsNullOrWhiteSpace(displayKey)) _customInputDisplayKeys[key] = displayKey.Trim();
            if (_batchMode) { _batchDirty = true; return; }
            PopulateListViews();
            BuildPropertyCollections();
        }

        public void BindDIOOutput(Action onAction, Action offAction, string label = "Output", Func<bool> stateFunc = null, string displayKey = null)
        {
            if (onAction == null && offAction == null) return;
            var key = "__custom_out_" + (++_customOutSeq);
            _customOutputActions[key] = (onAction ?? (()=>{ }), offAction ?? (()=>{ }), stateFunc);
            _displayMap[key] = label;
            _customOutputSequence.Add(key);
            if (!string.IsNullOrWhiteSpace(displayKey)) _customOutputDisplayKeys[key] = displayKey.Trim();
            if (_batchMode) { _batchDirty = true; return; }
            PopulateListViews();
            BuildPropertyCollections();
        }

        // === High-level helpers (Cylinder / Vacuum) ===
        // Cylinder: extend/retract 도메인 함수 + (선택) 센서 표시
        public void BindCylinder(string label,
                         Action extend,
                         Action retract,
                         Func<bool> isExtended,
                         Func<bool> isRetracted = null,
                         string displayKey = null,
                         bool showSensors = true,
                         string extendedName = "UP",
                         string retractedName = "DOWN")
        {
            var keyBase = string.IsNullOrWhiteSpace(displayKey) ? label : displayKey;

            // 센서 표시 (선택)
            if (showSensors)
            {
                if (isExtended != null)
                    BindDIOInput(isExtended, $"{label} {extendedName} Sns", $"{keyBase}_{extendedName}Sns");
                if (isRetracted != null)
                    BindDIOInput(isRetracted, $"{label} {retractedName} Sns", $"{keyBase}_{retractedName}Sns");
            }

            // 토글 상태 판단: Up(또는 Fwd/Clamp) 센서가 없으면 Down(또는 Bwd/Unclamp) 센서를 반전해 사용
            Func<bool> stateFunc = isExtended ?? (isRetracted != null ? (Func<bool>)(() => !isRetracted()) : null);

            // 출력 토글 (상태 함수 제공 필수: 없으면 한쪽만 동작하게 됨)
            BindDIOOutput(extend, retract, label, stateFunc, keyBase);
        }

        // Vacuum: on/off 도메인 함수 + (선택) OK 센서 표시
        public void BindVacuum(string label,
                               Action on,
                               Action off,
                               Func<bool> isOk = null,
                               Func<bool> isOnState = null,
                               string displayKey = null,
                               bool showOkSensor = true)
        {
            var keyBase = string.IsNullOrWhiteSpace(displayKey) ? label : displayKey;

            if (showOkSensor && isOk != null)
                BindDIOInput(isOk, $"{label} OK(Sns)", $"{keyBase}_Ok");

            BindDIOOutput(on, off, label, isOnState, keyBase);
        }


        public void RebuildLists()
        {
            PopulateListViews();
            BuildPropertyCollections();
            SafeRefreshStates();
        }
        #endregion

        #region Scan Cylinders / Vacuums
        private void ScanUnits()
        {
            foreach (var u in _units)
            {
                try
                {
                    var fields = u.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    foreach (var f in fields)
                    {
                        var v = f.GetValue(u);
                        if (v == null) continue;
                        if (v is Cylinder cyl) AddCylinder(cyl, u.UnitName);
                        else if (v is Vacuum vac) AddVacuum(vac, u.UnitName);
                    }
                }
                catch (Exception ex) { SafeLog($"ScanUnits:{u.UnitName}", ex.Message); }
            }
        }

        private void AddCylinder(Cylinder c, string owner)
        {
            // Reflect private keys: _fwdOutKey,_bwdOutKey,_fwdInKey,_bwdInKey
            var t = c.GetType();
            string fwdOut = GetPrivateString(t, c, "_fwdOutKey");
            string bwdOut = GetPrivateString(t, c, "_bwdOutKey");
            string fwdIn = GetPrivateString(t, c, "_fwdInKey");
            string bwdIn = GetPrivateString(t, c, "_bwdInKey");
            RegisterOutput(fwdOut, $"{c.Name}.FWD ({owner})");
            RegisterOutput(bwdOut, $"{c.Name}.BWD ({owner})");
            RegisterInput(fwdIn, $"{c.Name}.FWD Sns ({owner})");
            RegisterInput(bwdIn, $"{c.Name}.BWD Sns ({owner})");
        }

        private void AddVacuum(Vacuum v, string owner)
        {
            var t = v.GetType();
            string outKey = GetPrivateString(t, v, "_outKey");
            string okKey = GetPrivateString(t, v, "_okInKey");
            RegisterOutput(outKey, $"{v.Name}.Vac ({owner})");
            if (!string.IsNullOrWhiteSpace(okKey) && !okKey.Contains("/*NO_SENSOR*/"))
                RegisterInput(okKey, $"{v.Name}.Ok ({owner})");
        }

        private string GetPrivateString(Type t, object inst, string field)
        {
            try
            {
                var fi = t.GetField(field, BindingFlags.Instance | BindingFlags.NonPublic);
                if (fi != null && fi.FieldType == typeof(string))
                    return (string)fi.GetValue(inst);
            }
            catch { }
            return null;
        }
        #endregion

        #region Register keys
        private void RegisterInput(string key, string label)
        {
            if (string.IsNullOrWhiteSpace(key)) return;
            if (_inputKeys.Add(key))
            {
                _displayMap[key] = label;
                _inputSequence.Add(key);
            }
        }
        private void RegisterOutput(string key, string label)
        {
            if (string.IsNullOrWhiteSpace(key)) return;
            if (_outputKeys.Add(key))
            {
                _displayMap[key] = label;
                _outputSequence.Add(key);
            }
        }
        #endregion

        #region Populate ListViews
        private void PopulateListViews()
        {
            try
            {
                var inBox = FindInnerListBox(inputView);
                var outBox = FindInnerListBox(outputView);
                if (inBox == null) inBox = EnsureFallback(inputView, true);
                if (outBox == null) outBox = EnsureFallback(outputView, false);
                inBox?.BeginUpdate(); outBox?.BeginUpdate();
                inBox?.Items.Clear(); outBox?.Items.Clear();

                if (_inputKeys.Count == 0 && _outputKeys.Count == 0 && (_customInputStates.Count == 0 && _customOutputActions.Count == 0))
                {
                    try
                    {
                        foreach (var k in DIO.GetAllInputKeys()) RegisterInput(k, k);
                        foreach (var k in DIO.GetAllOutputKeys()) RegisterOutput(k, k);
                    }
                    catch { }
                }

                IEnumerable<string> orderedInputs;
                IEnumerable<string> orderedOutputs;

                switch (IoSortMode)
                {
                    case SortingMode.Insertion:
                        orderedInputs = _inputSequence;
                        orderedOutputs = _outputSequence;
                        break;
                    case SortingMode.AlphabeticalLabel:
                        orderedInputs = _inputKeys
                            .OrderBy(k => _displayMap.TryGetValue(k, out var l) ? l : k, StringComparer.OrdinalIgnoreCase);
                        orderedOutputs = _outputKeys
                            .OrderBy(k => _displayMap.TryGetValue(k, out var l) ? l : k, StringComparer.OrdinalIgnoreCase);
                        break;
                    case SortingMode.AlphabeticalKey:
                    default:
                        orderedInputs = _inputKeys.OrderBy(x => x, StringComparer.OrdinalIgnoreCase);
                        orderedOutputs = _outputKeys.OrderBy(x => x, StringComparer.OrdinalIgnoreCase);
                        break;
                }

                foreach (var k in orderedInputs)
                    inBox?.Items.Add(new IoItem { Key = k, Label = _displayMap.TryGetValue(k, out var l) ? l : k, IsSeparator = IsSeparatorLabel(_displayMap.TryGetValue(k, out var lbl) ? lbl : k) });
                // custom inputs keep insertion order
                foreach (var ck in _customInputSequence)
                    inBox?.Items.Add(new IoItem { Key = ck, Label = _displayMap.TryGetValue(ck, out var l) ? l : ck, IsSeparator = IsSeparatorLabel(_displayMap.TryGetValue(ck, out var lbl2) ? lbl2 : ck) });

                foreach (var k in orderedOutputs)
                    outBox?.Items.Add(new IoItem { Key = k, Label = _displayMap.TryGetValue(k, out var l) ? l : k });
                foreach (var ck in _customOutputSequence)
                    outBox?.Items.Add(new IoItem { Key = ck, Label = _displayMap.TryGetValue(ck, out var l) ? l : ck });

                if (inBox != null) inBox.DisplayMember = nameof(IoItem.Label);
                if (outBox != null) outBox.DisplayMember = nameof(IoItem.Label);
                inBox?.EndUpdate(); outBox?.EndUpdate();
                if (outBox != null)
                {
                    outBox.DoubleClick -= OutputList_DoubleClick;
                    outBox.DoubleClick += OutputList_DoubleClick;
                }
            }
            catch (Exception ex) { SafeLog("Populate", ex.Message); }
        }

        private static bool IsSeparatorLabel(string label)
            => !string.IsNullOrEmpty(label) && label.StartsWith("---- ", StringComparison.OrdinalIgnoreCase);

        private ListBox FindInnerListBox(Control parent)
        {
            if (parent == null) return null;
            if (parent is ListBox lb) return lb;
            foreach (Control c in parent.Controls)
            {
                var found = FindInnerListBox(c);
                if (found != null) return found;
            }
            return null;
        }

        private ListBox EnsureFallback(Control host, bool isInput)
        {
            if (host == null) return null;
            var target = isInput ? _fallbackInput : _fallbackOutput;
            if (target != null && !target.IsDisposed) return target;
            target = new ListBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black,
                ForeColor = Color.Lime,
                BorderStyle = BorderStyle.FixedSingle,
                IntegralHeight = false
            };
            target.DrawMode = DrawMode.OwnerDrawFixed;
            target.DrawItem += ListBox_DrawItem;
            host.Controls.Add(target);
            if (isInput) _fallbackInput = target; else _fallbackOutput = target;
            return target;
        }
        #endregion

        #region Output Toggle
        // OutputList_DoubleClick 교체
        private void OutputList_DoubleClick(object sender, EventArgs e)
        {
            if (!(sender is ListBox lb)) return;
            var sel = lb.SelectedItem as IoItem; if (sel == null) return;
            if (sel.IsSeparator) return;
            try
            {
                if (sel.Key.StartsWith("__custom_out_", StringComparison.OrdinalIgnoreCase))
                {
                    if (_customOutputActions.TryGetValue(sel.Key, out var act))
                    {
                        bool cur;
                        if (act.state != null)
                        {
                            try { cur = act.state(); }
                            catch
                            {
                                cur = _customOutputSoftStates.TryGetValue(sel.Key, out var s) ? s : false;
                            }
                        }
                        else
                        {
                            cur = _customOutputSoftStates.TryGetValue(sel.Key, out var s) ? s : false;
                        }

                        bool targetOn = !cur;
                        if (targetOn) act.on?.Invoke(); else act.off?.Invoke();
                        _customOutputSoftStates[sel.Key] = targetOn;
                        return;
                    }
                }

                bool curStd = false;
                bool hasState = TryReadOutput(sel.Key, ref curStd);
                if (!hasState) return;
                DIO.Out(sel.Key, !curStd);
            }
            catch (Exception ex) { SafeLog("Toggle", ex.Message); }
        }

        private bool TryReadOutput(string key, ref bool value)
        {
            try
            {
                if (DIO.TryGetOutputState(key, out var v)) { value = v; return true; }
            }
            catch (Exception ex)
            {
                SafeLog("TryReadOutput", ex.Message);
            }
            return false;
        }

        // IoPoint 구조 변화(속성→필드, 비공개, 이름 변경 가능성)에 대비한 유연한 추출
        private bool TryExtractIoPointInfo(object point, out string module, out string disp, out bool isOutput)
        {
            module = null;
            disp = null;
            isOutput = false;
            try
            {
                var t = point.GetType();

                object GetMember(string primary, params string[] fallback)
                {
                    // 1) 속성 검색 (대소문자 구분 X)
                    var prop = t.GetProperty(primary, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase);
                    if (prop == null && fallback != null)
                    {
                        foreach (var name in fallback)
                        {
                            prop = t.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase);
                            if (prop != null) break;
                        }
                    }
                    if (prop != null)
                        return prop.GetValue(point, null);

                    // 2) 필드 검색
                    var field = t.GetField(primary, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase);
                    if (field == null && fallback != null)
                    {
                        foreach (var name in fallback)
                        {
                            field = t.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase);
                            if (field != null) break;
                        }
                    }
                    if (field != null)
                        return field.GetValue(point);

                    return null;
                }

                module = GetMember("Module", "Mod", "ModuleName") as string;
                disp = GetMember("Disp", "Display", "Name", "Key") as string;

                var ioFlagObj = GetMember("IsOutput", "Output", "IsOut");
                if (ioFlagObj is bool b) isOutput = b;
                else if (ioFlagObj is int ib) isOutput = ib != 0;

                // 최소 조건
                return !string.IsNullOrEmpty(disp);
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region Refresh
        private void SafeRefreshStates()
        {
            try
            {
                // PropertyCollectionView 모드일 경우 PictureBox 색상만 업데이트
                if (UsingPropertyViews && _dispToInputKey.Count + _dispToOutputKey.Count > 0)
                {
                    foreach (var kv in _dispToInputKey)
                    {
                        bool on = false;
                        var logical = kv.Value;
                        if (logical.StartsWith("__custom_in_", StringComparison.OrdinalIgnoreCase))
                        {
                            if (_customInputStates.TryGetValue(logical, out var f)) { try { on = f(); } catch { on = false; } }
                        }
                        else
                        {
                            try { on = DIO.In(logical, out var v) && v; } catch { on = false; }
                        }
                        inputView?.SetStateByKey(kv.Key, on);
                    }
                    // SafeRefreshStates 내 PropertyCollectionView 경로에서 출력 상태 갱신 부분 교체
                    foreach (var kv in _dispToOutputKey)
                    {
                        bool on = false;
                        var logical = kv.Value;
                        if (logical.StartsWith("__custom_out_", StringComparison.OrdinalIgnoreCase))
                        {
                            if (_customOutputActions.TryGetValue(logical, out var act))
                            {
                                if (act.state != null)
                                {
                                    try { on = act.state(); }
                                    catch
                                    {
                                        on = _customOutputSoftStates.TryGetValue(logical, out var s) ? s : false;
                                    }
                                }
                                else
                                {
                                    on = _customOutputSoftStates.TryGetValue(logical, out var s) ? s : false;
                                }
                            }
                        }
                        else
                        {
                            DIO.TryGetOutputState(logical, out on);
                        }
                        outputView?.SetStateByKey(kv.Key, on);
                    }
                    return; // ListBox 경로 종료
                }

                // ===== ListBox Fallback =====
                var inBox = FindInnerListBox(inputView);
                if (inBox != null)
                {
                    foreach (var obj in inBox.Items.OfType<IoItem>())
                    {
                        if (obj.IsSeparator) { obj.State = false; continue; }
                        bool v = false;
                        if (obj.Key.StartsWith("__custom_in_", StringComparison.OrdinalIgnoreCase))
                        {
                            if (_customInputStates.TryGetValue(obj.Key, out var f))
                            {
                                try { v = f(); } catch { v = false; }
                                obj.State = v; continue;
                            }
                        }
                        obj.State = ReadInputKey(obj.Key, out v) && v;
                    }
                    inBox.Refresh();
                }
                // SafeRefreshStates 내 출력 상태 갱신 부분(리스트박스 경로) 교체
                var outBox = FindInnerListBox(outputView);
                if (outBox != null)
                {
                    foreach (var obj in outBox.Items.OfType<IoItem>())
                    {
                        bool cur = false;
                        if (obj.Key.StartsWith("__custom_out_", StringComparison.OrdinalIgnoreCase))
                        {
                            if (_customOutputActions.TryGetValue(obj.Key, out var act))
                            {
                                if (act.state != null)
                                {
                                    try { cur = act.state(); }
                                    catch
                                    {
                                        cur = _customOutputSoftStates.TryGetValue(obj.Key, out var s) ? s : false;
                                    }
                                }
                                else
                                {
                                    cur = _customOutputSoftStates.TryGetValue(obj.Key, out var s) ? s : false;
                                }
                                obj.State = cur;
                                continue;
                            }
                        }
                        if (TryReadOutput(obj.Key, ref cur)) obj.State = cur;
                    }
                    outBox.Refresh();
                }
            }
            catch (Exception ex) { SafeLog("Refresh", ex.Message); }
        }

        private void ListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            if (e.Index < 0) return;
            var lb = sender as ListBox;
            var item = lb.Items[e.Index] as IoItem;
            var text = item?.Label ?? lb.Items[e.Index].ToString();
            if (item != null && item.IsSeparator)
            {
                using (var b = new SolidBrush(Color.FromArgb(40, 40, 40)))
                    e.Graphics.FillRectangle(b, e.Bounds);
                using (var f = new SolidBrush(Color.Gold))
                {
                    var font = new Font(e.Font, FontStyle.Bold);
                    e.Graphics.DrawString(text, font, f, e.Bounds.Location);
                }
                return;
            }
            var on = item != null && item.State;
            var bg = on ? (e.BackColor == SystemColors.Highlight ? Color.LimeGreen : Color.FromArgb(0, 200, 0)) : e.BackColor;
            using (var b = new SolidBrush(on ? bg : e.BackColor))
                e.Graphics.FillRectangle(b, e.Bounds);
            using (var f = new SolidBrush(on ? Color.Black : e.ForeColor))
                e.Graphics.DrawString(text, e.Font, f, e.Bounds.Location);
            e.DrawFocusRectangle();
        }

        private bool ReadInputKey(string key, out bool value)
        {
            value = false;
            try { return DIO.In(key, out value); } catch { return false; }
        }
        #endregion

        #region Helper
        private void SafeLog(string cat, string msg)
        {
            try { Log.Write("DIOControl", cat, "DIOControl", msg); } catch { }
        }

        private sealed class IoItem
        {
            public string Key { get; set; }
            public string Label { get; set; }
            public bool State { get; set; }
            public bool IsSeparator { get; set; }
            public override string ToString() => Label;
        }
        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _refreshTimer?.Stop();
                _refreshTimer?.Dispose();
            }
            base.Dispose(disposing);
        }

        private void BuildPropertyCollections()
        {
            if (!UsingPropertyViews) return; // ListBox fallback 모드 유지

            // 1) 새 표시 키 시퀀스 + 매핑 미리 계산 (중복 제거/유니크 처리 포함)
            var newDispIn = new List<string>();
            var newMapIn = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var newDispOut = new List<string>();
            var newMapOut = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            // 입력 표준 키들
            foreach (var k in _inputKeys.OrderBy(x => x))
            {
                if (!DIO.TryGetPointInfo(k, out var isOut, out var module, out var disp) || isOut) continue;
                var label = _displayMap.TryGetValue(k, out var l) ? l : k;
                var text = string.IsNullOrWhiteSpace(disp) ? label : disp;
                if (string.IsNullOrWhiteSpace(text)) text = label;
                if (!newMapIn.ContainsKey(text))
                {
                    newMapIn[text] = k;
                    newDispIn.Add(text);
                }
            }
            // 입력 커스텀 키들 (고유키 생성)
            int no = 1;
            foreach (var key in _customInputSequence)
            {
                string desiredDisp;
                if (!_customInputDisplayKeys.TryGetValue(key, out desiredDisp) || string.IsNullOrWhiteSpace(desiredDisp))
                    desiredDisp = $"CI{no:000}";
                var uniqueDisp = desiredDisp; int dup = 1;
                while (newMapIn.ContainsKey(uniqueDisp)) uniqueDisp = desiredDisp + "_" + (++dup).ToString();
                newMapIn[uniqueDisp] = key;
                newDispIn.Add(uniqueDisp);
                no++;
            }

            // 출력 표준 키들
            foreach (var k in _outputKeys.OrderBy(x => x))
            {
                if (!DIO.TryGetPointInfo(k, out var isOut, out var module, out var disp) || !isOut) continue;
                var label = _displayMap.TryGetValue(k, out var l) ? l : k;
                var text = string.IsNullOrWhiteSpace(disp) ? label : disp;
                if (string.IsNullOrWhiteSpace(text)) text = label;
                if (!newMapOut.ContainsKey(text))
                {
                    newMapOut[text] = k;
                    newDispOut.Add(text);
                }
            }
            // 출력 커스텀 키들
            no = 1;
            foreach (var key in _customOutputSequence)
            {
                string desiredDisp;
                if (!_customOutputDisplayKeys.TryGetValue(key, out desiredDisp) || string.IsNullOrWhiteSpace(desiredDisp))
                    desiredDisp = $"CO{no:000}";
                var uniqueDisp = desiredDisp; int dup = 1;
                while (newMapOut.ContainsKey(uniqueDisp)) uniqueDisp = desiredDisp + "_" + (++dup).ToString();
                newMapOut[uniqueDisp] = key;
                newDispOut.Add(uniqueDisp);
                no++;
            }

            // 2) 구조 동일하면 컨트롤 재생성 없이 매핑만 교체 후 상태만 갱신
            bool sameStructure =
                _lastInputDispOrder.Count == newDispIn.Count &&
                _lastOutputDispOrder.Count == newDispOut.Count &&
                !_lastInputDispOrder.Where((t, i) => !string.Equals(t, newDispIn[i], StringComparison.OrdinalIgnoreCase)).Any() &&
                !_lastOutputDispOrder.Where((t, i) => !string.Equals(t, newDispOut[i], StringComparison.OrdinalIgnoreCase)).Any();

            if (sameStructure)
            {
                _dispToInputKey.Clear();
                foreach (var kv in newMapIn) _dispToInputKey[kv.Key] = kv.Value;
                _dispToOutputKey.Clear();
                foreach (var kv in newMapOut) _dispToOutputKey[kv.Key] = kv.Value;

                _lastInputDispOrder.Clear(); _lastInputDispOrder.AddRange(newDispIn);
                _lastOutputDispOrder.Clear(); _lastOutputDispOrder.AddRange(newDispOut);

                SafeRefreshStates();
                return;
            }

            _dispToInputKey.Clear();
            _dispToOutputKey.Clear();

            var inPc = new PropertyCollection { ShowNoColumn = true };
            var outPc = new PropertyCollection { ShowNoColumn = true };
            inPc.Add(new TitleOnlyProperty("No", "Name", "State"));
            outPc.Add(new TitleOnlyProperty("No", "Name", "State"));

            // 3) 실제 PC 구성 (빠른 초기 로드: 상태 조회 생략 옵션)
            int noIn = 1;
            foreach (var disp in newDispIn)
            {
                bool state = false;
                if (!FastInitialState)
                {
                    var k = newMapIn[disp];
                    if (k.StartsWith("__custom_in_", StringComparison.OrdinalIgnoreCase))
                    { if (_customInputStates.TryGetValue(k, out var f)) { try { state = f(); } catch { state = false; } } }
                    else { try { state = DIO.In(k, out var v) && v; } catch { state = false; } }
                }
                inPc.Add(new PropertyState(noIn.ToString(), disp, state) { ShowNoColumn = true });
                _dispToInputKey[disp] = newMapIn[disp];
                noIn++;
            }

            int noOut = 1;
            foreach (var disp in newDispOut)
            {
                bool state = false;
                if (!FastInitialState)
                {
                    var k = newMapOut[disp];
                    if (k.StartsWith("__custom_out_", StringComparison.OrdinalIgnoreCase))
                    {
                        if (_customOutputActions.TryGetValue(k, out var act) && act.state != null) { try { state = act.state(); } catch { state = false; } }
                    }
                    else { DIO.TryGetOutputState(k, out state); }
                }
                outPc.Add(new PropertyState(noOut.ToString(), disp, state) { ShowNoColumn = true });
                _dispToOutputKey[disp] = newMapOut[disp];
                noOut++;
            }

            _lastInputPc = inPc;
            _lastOutputPc = outPc;

            try { inputView?.SetProperties(inPc); } catch { }
            try
            {
                if (outputView != null)
                {
                    outputView.ItemClicked -= OutputView_ItemClicked;
                    outputView.ItemClicked += OutputView_ItemClicked;
                    outputView.SetProperties(outPc);
                }
            }
            catch { }

            // 캐시 갱신
            _lastInputDispOrder.Clear(); _lastInputDispOrder.AddRange(newDispIn);
            _lastOutputDispOrder.Clear(); _lastOutputDispOrder.AddRange(newDispOut);
        }

        // OutputView_ItemClicked 교체 (PropertyCollectionView에서의 토글)
        private void OutputView_ItemClicked(object sender, string dispKey)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dispKey)) return;
                if (!_dispToOutputKey.TryGetValue(dispKey, out var logical)) return;

                if (logical.StartsWith("__custom_out_", StringComparison.OrdinalIgnoreCase))
                {
                    if (_customOutputActions.TryGetValue(logical, out var act))
                    {
                        bool cur;
                        if (act.state != null)
                        {
                            try { cur = act.state(); }
                            catch
                            {
                                cur = _customOutputSoftStates.TryGetValue(logical, out var s) ? s : false;
                            }
                        }
                        else
                        {
                            cur = _customOutputSoftStates.TryGetValue(logical, out var s) ? s : false;
                        }

                        var targetOn = !cur;
                        if (targetOn) act.on?.Invoke(); else act.off?.Invoke();
                        _customOutputSoftStates[logical] = targetOn;

                        outputView?.SetStateByKey(dispKey, targetOn);
                    }
                    return;
                }

                bool curStd = false; DIO.TryGetOutputState(logical, out curStd);
                var target = !curStd;
                DIO.Out(logical, target);
                outputView?.SetStateByKey(dispKey, target);
            }
            catch (Exception ex)
            {
                SafeLog("OutputToggle", ex.Message);
            }
        }

        private bool _batchMode;               // batch 등록 모드 여부
        private bool _batchDirty;              // batch 동안 변경 발생 여부

        // Begin/End batch API
        public void BeginBatch()
        {
            _batchMode = true;
            _batchDirty = false;
        }
        public void EndBatch(bool rebuild = true)
        {
            _batchMode = false;
            if (rebuild && _batchDirty)
            {
                PopulateListViews();
                BuildPropertyCollections();
                SafeRefreshStates();
            }
        }
    }
}
