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
        private Timer _refreshTimer;
        public int RefreshIntervalMs { get; set; } = 400;

        // Added for PropertyCollectionView support
        private readonly Dictionary<string, string> _dispToInputKey = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> _dispToOutputKey = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private PropertyCollection _lastInputPc;
        private PropertyCollection _lastOutputPc;
        private bool UsingPropertyViews => (inputView != null && outputView != null && inputView.GetType().Name.Contains("IOPropertyCollectionView"));

        private ListBox _fallbackInput;
        private ListBox _fallbackOutput;

        private readonly Dictionary<string, Func<bool>> _customInputStates = new Dictionary<string, Func<bool>>();
        private readonly Dictionary<string, (Action on, Action off, Func<bool> state)> _customOutputActions = new Dictionary<string, (Action on, Action off, Func<bool> state)>();
        private int _customInSeq = 0;
        private int _customOutSeq = 0;
        // NEW: external display key mapping for custom IO
        private readonly Dictionary<string, string> _customInputDisplayKeys = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> _customOutputDisplayKeys = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

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
            if (units != null)
                foreach (var u in units.Where(x => x != null)) _units.Add(u);
            ScanUnits();
            PopulateListViews(); // fallback (ListBox) 용
            BuildPropertyCollections(); // IOPropertyCollectionView 용
            SafeRefreshStates();
        }

        public void BindEjectorVacuum(QMC.LCP_280.Process.Unit.InputStage stage)
        {
            if (!IsHandleCreated)
            {
                HandleCreated += (s, e) => BindEjectorVacuum(stage);
                return;
            }
            _units.Clear();
            _inputKeys.Clear();
            _outputKeys.Clear();
            _displayMap.Clear();
            _customInputStates.Clear();
            _customOutputActions.Clear();
            if (stage != null && stage.EjectorVacuum != null)
            {
                try { AddVacuum(stage.EjectorVacuum, stage.UnitName ?? "InputStage"); } catch { }
            }
            PopulateListViews();
            BuildPropertyCollections();
            SafeRefreshStates();
        }

        // === Simple delegate binding (custom, non DIO key) ===
        public void BindDIOInput(Func<bool> stateFunc, string label = "Input", string displayKey = null)
        {
            if (stateFunc == null) return;
            var key = "__custom_in_" + (++_customInSeq);
            _customInputStates[key] = stateFunc;
            _displayMap[key] = label;
            if (!string.IsNullOrWhiteSpace(displayKey)) _customInputDisplayKeys[key] = displayKey.Trim();
            PopulateListViews();
            BuildPropertyCollections();
        }

        public void BindDIOOutput(Action onAction, Action offAction, string label = "Output", Func<bool> stateFunc = null, string displayKey = null)
        {
            if (onAction == null && offAction == null) return;
            var key = "__custom_out_" + (++_customOutSeq);
            _customOutputActions[key] = (onAction ?? (()=>{ }), offAction ?? (()=>{ }), stateFunc);
            _displayMap[key] = label;
            if (!string.IsNullOrWhiteSpace(displayKey)) _customOutputDisplayKeys[key] = displayKey.Trim();
            PopulateListViews();
            BuildPropertyCollections();
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
            if (_inputKeys.Add(key)) _displayMap[key] = label;
        }
        private void RegisterOutput(string key, string label)
        {
            if (string.IsNullOrWhiteSpace(key)) return;
            if (_outputKeys.Add(key)) _displayMap[key] = label;
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

                foreach (var k in _inputKeys.OrderBy(x => x))
                    inBox?.Items.Add(new IoItem { Key = k, Label = _displayMap.TryGetValue(k, out var l) ? l : k });
                foreach (var ck in _customInputStates.Keys.OrderBy(x => x))
                    inBox?.Items.Add(new IoItem { Key = ck, Label = _displayMap.TryGetValue(ck, out var l) ? l : ck });

                foreach (var k in _outputKeys.OrderBy(x => x))
                    outBox?.Items.Add(new IoItem { Key = k, Label = _displayMap.TryGetValue(k, out var l) ? l : k });
                foreach (var ck in _customOutputActions.Keys.OrderBy(x => x))
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
        private void OutputList_DoubleClick(object sender, EventArgs e)
        {
            if (!(sender is ListBox lb)) return;
            var sel = lb.SelectedItem as IoItem; if (sel == null) return;
            try
            {
                if (sel.Key.StartsWith("__custom_out_", StringComparison.OrdinalIgnoreCase))
                {
                    if (_customOutputActions.TryGetValue(sel.Key, out var act))
                    {
                        bool cur = false;
                        if (act.state != null)
                        {
                            try { cur = act.state(); } catch { }
                        }
                        // toggle: if current on -> offAction else onAction
                        if (cur) act.off?.Invoke(); else act.on?.Invoke();
                        return;
                    }
                }
                bool curStd = false;
                bool hasState = TryReadOutput(sel.Key, ref curStd);
                if (hasState) DIO.Out(sel.Key, !curStd); else DIO.Out(sel.Key, true);
            }
            catch (Exception ex) { SafeLog("Toggle", ex.Message); }
        }

        private bool TryReadOutput(string key, ref bool value)
        {
            try
            {
                // 새로운 공식 API 사용 (리플렉션 제거)
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
                    foreach (var kv in _dispToOutputKey)
                    {
                        bool on = false;
                        var logical = kv.Value;
                        if (logical.StartsWith("__custom_out_", StringComparison.OrdinalIgnoreCase))
                        {
                            if (_customOutputActions.TryGetValue(logical, out var act) && act.state != null)
                            {
                                try { on = act.state(); } catch { on = false; }
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
                    inBox.DrawMode = DrawMode.OwnerDrawFixed;
                    inBox.DrawItem -= ListBox_DrawItem;
                    inBox.DrawItem += ListBox_DrawItem;
                    inBox.Refresh();
                }
                var outBox = FindInnerListBox(outputView);
                if (outBox != null)
                {
                    foreach (var obj in outBox.Items.OfType<IoItem>())
                    {
                        bool cur = false;
                        if (obj.Key.StartsWith("__custom_out_", StringComparison.OrdinalIgnoreCase))
                        {
                            if (_customOutputActions.TryGetValue(obj.Key, out var act) && act.state != null)
                            {
                                try { cur = act.state(); } catch { cur = false; }
                                obj.State = cur; continue;
                            }
                        }
                        if (TryReadOutput(obj.Key, ref cur)) obj.State = cur;
                    }
                    outBox.DrawMode = DrawMode.OwnerDrawFixed;
                    outBox.DrawItem -= ListBox_DrawItem;
                    outBox.DrawItem += ListBox_DrawItem;
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

            _dispToInputKey.Clear();
            _dispToOutputKey.Clear();
            var inPc = new PropertyCollection { ShowNoColumn = true };
            var outPc = new PropertyCollection { ShowNoColumn = true };
            inPc.Add(new TitleOnlyProperty("No", "Name", "State"));
            outPc.Add(new TitleOnlyProperty("No", "Name", "State"));

            int no = 1;
            foreach (var k in _inputKeys.OrderBy(x => x))
            {
                if (!DIO.TryGetPointInfo(k, out var isOut, out var module, out var disp) || isOut) continue;
                var label = _displayMap.TryGetValue(k, out var l) ? l : k;
                var text = string.IsNullOrWhiteSpace(disp) ? label : $"{disp} {label}";
                bool state = false; try { DIO.In(k, out state); } catch { state = false; }
                inPc.Add(new PropertyState(no.ToString(), text, state) { ShowNoColumn = true });
                if (!string.IsNullOrEmpty(disp) && !_dispToInputKey.ContainsKey(disp)) _dispToInputKey[disp] = k;
                no++;
            }
            // Custom inputs
            foreach (var kv in _customInputStates.OrderBy(x => x.Key))
            {
                bool state = false; try { state = kv.Value(); } catch { }
                string desiredDisp;
                if (!_customInputDisplayKeys.TryGetValue(kv.Key, out desiredDisp) || string.IsNullOrWhiteSpace(desiredDisp))
                    desiredDisp = $"CI{no:000}"; // fallback
                // ensure unique within mapping
                var uniqueDisp = desiredDisp;
                int dup = 1;
                while (_dispToInputKey.ContainsKey(uniqueDisp))
                {
                    uniqueDisp = desiredDisp + "_" + (++dup).ToString();
                }
                inPc.Add(new PropertyState(no.ToString(), $"{uniqueDisp} {_displayMap[kv.Key]}", state) { ShowNoColumn = true });
                if (!_dispToInputKey.ContainsKey(uniqueDisp)) _dispToInputKey[uniqueDisp] = kv.Key;
                no++;
            }

            no = 1;
            foreach (var k in _outputKeys.OrderBy(x => x))
            {
                if (!DIO.TryGetPointInfo(k, out var isOut, out var module, out var disp) || !isOut) continue;
                var label = _displayMap.TryGetValue(k, out var l) ? l : k;
                var text = string.IsNullOrWhiteSpace(disp) ? label : $"{disp} {label}";
                bool state = false; DIO.TryGetOutputState(k, out state);
                outPc.Add(new PropertyState(no.ToString(), $"{disp} {text}".Trim(), state) { ShowNoColumn = true });
                if (!string.IsNullOrEmpty(disp) && !_dispToOutputKey.ContainsKey(disp)) _dispToOutputKey[disp] = k;
                no++;
            }
            foreach (var kv in _customOutputActions.OrderBy(x => x.Key))
            {
                bool state = false; try { if (kv.Value.state != null) state = kv.Value.state(); } catch { }
                string desiredDisp;
                if (!_customOutputDisplayKeys.TryGetValue(kv.Key, out desiredDisp) || string.IsNullOrWhiteSpace(desiredDisp))
                    desiredDisp = $"CO{no:000}"; // fallback
                var uniqueDisp = desiredDisp;
                int dup = 1;
                while (_dispToOutputKey.ContainsKey(uniqueDisp))
                {
                    uniqueDisp = desiredDisp + "_" + (++dup).ToString();
                }
                outPc.Add(new PropertyState(no.ToString(), $"{uniqueDisp} {_displayMap[kv.Key]}", state) { ShowNoColumn = true });
                if (!_dispToOutputKey.ContainsKey(uniqueDisp)) _dispToOutputKey[uniqueDisp] = kv.Key;
                no++;
            }

            _lastInputPc = inPc;
            _lastOutputPc = outPc;
            try { inputView?.SetProperties(inPc); } catch { }
            try {
                if (outputView != null)
                {
                    outputView.ItemClicked -= OutputView_ItemClicked; // 중복 방지
                    outputView.ItemClicked += OutputView_ItemClicked;
                    outputView.SetProperties(outPc);
                }
            } catch { }
        }

        private void OutputView_ItemClicked(object sender, string dispKey)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dispKey)) return;
                // dispKey 는 Y000 / CO001 등 DisplayNo 또는 가상키
                if (!_dispToOutputKey.TryGetValue(dispKey, out var logical)) return;

                bool cur = false;
                bool isCustom = logical.StartsWith("__custom_out_", StringComparison.OrdinalIgnoreCase);
                if (isCustom)
                {
                    if (_customOutputActions.TryGetValue(logical, out var act) && act.state != null)
                    {
                        try { cur = act.state(); } catch { cur = false; }
                    }
                }
                else
                {
                    DIO.TryGetOutputState(logical, out cur);
                }

                var label = _displayMap.TryGetValue(logical, out var l) ? l : logical;
                var targetOn = !cur;
                var ask = targetOn ? $"'{label}' 출력을 ON 시키겠습니까?" : $"'{label}' 출력을 OFF 시키겠습니까?";
                var dr = MessageBox.Show(ask, "DIO Output", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr != DialogResult.Yes) return;

                bool newState = cur;
                if (isCustom)
                {
                    if (_customOutputActions.TryGetValue(logical, out var act))
                    {
                        if (targetOn) act.on?.Invoke(); else act.off?.Invoke();
                        if (act.state != null) { try { newState = act.state(); } catch { newState = targetOn; } }
                        else newState = targetOn; // 상태 delegate 없으면 가정
                    }
                }
                else
                {
                    try { DIO.Out(logical, targetOn); newState = targetOn; } catch { }
                }
                // 즉시 UI 반영
                outputView?.SetStateByKey(dispKey, newState);
            }
            catch (Exception ex)
            {
                SafeLog("OutputToggle", ex.Message);
            }
        }
    }
}
