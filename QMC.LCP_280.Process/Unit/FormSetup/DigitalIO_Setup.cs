using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using QMC.Common;
using QMC.Common.IO;
using QMC.Common.DIO; // DioScanService 사용

namespace QMC.LCP_280.Process.Unit
{
    [FormOrder(2)]
    /// <summary>
    /// DigitalIO Setup 폼 - UI 로직 및 데이터 처리 (Designer에는 순수 UI만 존재)
    /// </summary>
    public partial class DigitalIO_Setup : Form
    {
        private sealed class ModuleListItem
        {
            public string Display { get; set; }
            public DIOModuleSetup Module { get; set; }
            public bool? IsDI { get; set; } // true=DI, false=DO, null=헤더/구분선
            public override string ToString() => Display;
        }

        // ===== Data / Services =====
        private readonly Equipment equipment = Equipment.Instance;
        private DIOUnit _unit;                 // Equipment에서 만든 _unitIO 참조
        private QMC.Common.DIO.DioScanService _scan;          // Equipment에서 만든 _dioScan 참조 (전체 한정명)
        private DIOModuleSetup _selected;      // 현재 선택된 모듈 (Mixed 사용 시)
        private string _setupPath;             // Unit.dio.setup.json 경로
        private List<ModuleListItem> _moduleListItems = new List<ModuleListItem>();
        private DIOModuleSetup _lastDiModule;
        private DIOModuleSetup _lastDoModule;

        // 저장 인덱스(향후 필요 시)
        private Dictionary<(string section, string title), PropertyBase> _configIndex;
        private Dictionary<(string section, string title), PropertyBase> _speedIndex;

        public DigitalIO_Setup()
        {
            InitializeComponent();
            SuspendLayout();
            InitializeUI();
            ResumeLayout(true);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            if (_scan != null)
            {
                _scan.InputChanged -= OnInputChanged;
                _scan.OutputChanged -= OnOutputChanged;
            }
        }

        // ===== Initialize =====
        private void InitializeUI()
        {
            try
            {
                _unit = equipment.UnitIO;   // LoadOrCreateDefault 된 맵 사용
                _scan = equipment.DioScan;  // 주기 스캐너
                _setupPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "Unit.dio.setup.json");

                if (_scan != null)
                {
                    _scan.InputChanged += OnInputChanged;
                    _scan.OutputChanged += OnOutputChanged;
                }

                WireIOSelectionEvent();
                BindModuleList();
            }
            catch (Exception ex)
            {
                Log.Write("LCP-280", $"InitializeUI error: {ex}");
            }
        }

        // ===== Event Wiring =====
        private void WireIOSelectionEvent()
        {
            if (dioModuleListBoxItemsView == null) return;
            dioModuleListBoxItemsView.ItemSelected -= OnIOItemSelected;
            dioModuleListBoxItemsView.ItemSelected += OnIOItemSelected;
            outputIOPropertyCollectionView.ItemClicked -= OnOutputItemClicked;
            outputIOPropertyCollectionView.ItemClicked += OnOutputItemClicked;
        }

        // ===== Module List Binding =====
        private void BindModuleList()
        {
            _moduleListItems.Clear();
            var items = new List<object>();

            _moduleListItems.Add(new ModuleListItem { Display = "─ DI Modules ─" });
            items.Add(_moduleListItems[_moduleListItems.Count - 1]);

            if (_unit?.Modules != null)
            {
                foreach (var m in _unit.Modules)
                {
                    if (m?.Inputs != null && m.Inputs.Count > 0)
                    {
                        _moduleListItems.Add(new ModuleListItem { Display = m.ModuleName, Module = m, IsDI = true });
                        items.Add(_moduleListItems[_moduleListItems.Count - 1]);
                    }
                }
            }

            _moduleListItems.Add(new ModuleListItem { Display = string.Empty });
            items.Add(_moduleListItems[_moduleListItems.Count - 1]);

            _moduleListItems.Add(new ModuleListItem { Display = "─ DO Modules ─" });
            items.Add(_moduleListItems[_moduleListItems.Count - 1]);

            if (_unit?.Modules != null)
            {
                foreach (var m in _unit.Modules)
                {
                    if (m?.Outputs != null && m.Outputs.Count > 0)
                    {
                        _moduleListItems.Add(new ModuleListItem { Display = m.ModuleName, Module = m, IsDI = false });
                        items.Add(_moduleListItems[_moduleListItems.Count - 1]);
                    }
                }
            }

            dioModuleListBoxItemsView.SetItems(items.ToArray());
            int firstSelectable = _moduleListItems.FindIndex(x => x.IsDI != null);
            if (firstSelectable >= 0) dioModuleListBoxItemsView.SelectedIndex = firstSelectable;
        }

        // ===== Selection Changed =====
        private void OnIOItemSelected(object sender, int selectedIndex)
        {
            try
            {
                if (selectedIndex < 0 || selectedIndex >= _moduleListItems.Count) return;
                var item = _moduleListItems[selectedIndex];
                if (item?.IsDI == null || item.Module == null) return;
                if (item.IsDI == true) BindChannels_DI(item.Module); else BindChannels_DO(item.Module);
            }
            catch (Exception ex)
            {
                Log.Write("LCP-280", $"OnIOItemSelected error: {ex}");
            }
        }

        // ===== Input/Output Changed (Scan Event) =====
        private void OnInputChanged(string module, string disp, bool value)
        {
            if (_lastDiModule != null && string.Equals(module, _lastDiModule.ModuleName, StringComparison.OrdinalIgnoreCase) && inputIOPropertyCollectionView != null)
            {
                void Update() => inputIOPropertyCollectionView.SetStateByKey(disp, value);
                if (inputIOPropertyCollectionView.IsHandleCreated && inputIOPropertyCollectionView.InvokeRequired)
                    inputIOPropertyCollectionView.BeginInvoke((Action)Update);
                else
                    Update();
            }
        }

        private void OnOutputChanged(string module, string disp, bool value)
        {
            if (_lastDoModule != null && string.Equals(module, _lastDoModule.ModuleName, StringComparison.OrdinalIgnoreCase) && outputIOPropertyCollectionView != null)
            {
                void Update() => outputIOPropertyCollectionView.SetStateByKey(disp, value);
                if (outputIOPropertyCollectionView.IsHandleCreated && outputIOPropertyCollectionView.InvokeRequired)
                    outputIOPropertyCollectionView.BeginInvoke((Action)Update);
                else
                    Update();
            }
        }

        // ===== Output Toggle =====
        private void OnOutputItemClicked(object sender, string key)
        {
            var m = _lastDoModule;
            if (_scan == null || m == null || string.IsNullOrEmpty(key)) return;
            var ask = new MessageBoxYesNo();
            if (ask.ShowDialog("Info", "Signal 변경하시겠습니까?") == DialogResult.No) return;
            bool before = false;
            _scan.TryGetOutput(m.ModuleName, key, out before);
            var rc = _scan.WriteOutput(m.ModuleName, key, !before);
            if (rc != 0)
            {
                new MessageBoxOk().ShowDialog("Error", $"WriteOutput 실패 (rc={rc})");
                return;
            }
            _scan.RefreshOnce();
            bool after = before;
            _scan.TryGetOutput(m.ModuleName, key, out after);
            //new MessageBoxOk().ShowDialog("Info!", $"{key}: {before} -> {after}");
        }

        // ===== Channel Binding =====
        private void BindChannels_DI(DIOModuleSetup module)
        {
            _lastDiModule = module;
            var diProps = new PropertyCollection { ShowNoColumn = false };
            int no = 1;
            if (module?.Inputs != null)
            {
                foreach (var ch in module.Inputs)
                {
                    bool v = false; _scan?.TryGetInput(module.ModuleName, ch.DisplayNo, out v);
                    diProps.Add(new PropertyState(no.ToString("00"), $"{ch.DisplayNo} {ch.Name}", v));
                    no++;
                }
            }
            inputIOPropertyCollectionView?.SetProperties(diProps);
            RefreshStatesOnce();
        }

        private void BindChannels_DO(DIOModuleSetup module)
        {
            _lastDoModule = module;
            var doProps = new PropertyCollection { ShowNoColumn = false };
            int no = 1;
            if (module?.Outputs != null)
            {
                foreach (var ch in module.Outputs)
                {
                    bool v = false; _scan?.TryGetOutput(module.ModuleName, ch.DisplayNo, out v);
                    doProps.Add(new PropertyState(no.ToString("00"), $"{ch.DisplayNo} {ch.Name}", v));
                    no++;
                }
            }
            outputIOPropertyCollectionView?.SetProperties(doProps);
            RefreshStatesOnce();
        }

        private void RefreshStatesOnce()
        {
            if (_scan == null) return;
            if (_lastDiModule?.Inputs != null && inputIOPropertyCollectionView != null)
            {
                foreach (var ch in _lastDiModule.Inputs)
                {
                    if (_scan.TryGetInput(_lastDiModule.ModuleName, ch.DisplayNo, out var v))
                    {
                        void Update() => inputIOPropertyCollectionView.SetStateByKey(ch.DisplayNo, v);
                        if (inputIOPropertyCollectionView.IsHandleCreated && inputIOPropertyCollectionView.InvokeRequired)
                            inputIOPropertyCollectionView.BeginInvoke((Action)Update);
                        else
                            Update();
                    }
                }
            }
            if (_lastDoModule?.Outputs != null && outputIOPropertyCollectionView != null)
            {
                foreach (var ch in _lastDoModule.Outputs)
                {
                    if (_scan.TryGetOutput(_lastDoModule.ModuleName, ch.DisplayNo, out var v))
                    {
                        void Update() => outputIOPropertyCollectionView.SetStateByKey(ch.DisplayNo, v);
                        if (outputIOPropertyCollectionView.IsHandleCreated && outputIOPropertyCollectionView.InvokeRequired)
                            outputIOPropertyCollectionView.BeginInvoke((Action)Update);
                        else
                            Update();
                    }
                }
            }
        }

        // ===== Save Buttons (현재 비어 있음) =====
        private void btn_Save_Setup_Output_Property_Click(object sender, EventArgs e)
        {
            try { /* TODO: Output Setup Save 구현 */ }
            catch (Exception ex)
            {
                MessageBox.Show($"저장 중 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_Save_Setup_Input_Property_Click(object sender, EventArgs e)
        {
            try { /* TODO: Input Setup Save 구현 */ }
            catch (Exception ex)
            {
                MessageBox.Show($"저장 중 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ===== (Optional) Index Utilities =====
        private static Dictionary<(string section, string title), PropertyBase> BuildIndex(PropertyCollection pc)
        {
            var map = new Dictionary<(string section, string title), PropertyBase>(StringTupleComparer.OrdinalIgnoreCase);
            if (pc == null || pc.Count == 0) return map;
            string currentSection = string.Empty;
            foreach (var p in pc)
            {
                if (p == null) continue;
                if (p is TitleOnlyProperty)
                {
                    currentSection = GetName(p) ?? string.Empty;
                    continue;
                }
                var title = GetName(p);
                if (string.IsNullOrEmpty(title)) continue;
                var key = (currentSection, title);
                if (!map.ContainsKey(key)) map[key] = p;
            }
            return map;
        }

        private PropertyBase Find(string section, string title)
        {
            if (_configIndex != null && _configIndex.TryGetValue((section ?? string.Empty, title), out var p1)) return p1;
            return null;
        }
        private PropertyBase FindS(string section, string title)
        {
            if (_speedIndex != null && _speedIndex.TryGetValue((section ?? string.Empty, title), out var p1)) return p1;
            return null;
        }

        private double GetDouble(string section, string title, double fallback) => ReadDouble(Find(section, title), fallback);
        private double GetDoubleS(string section, string title, double fallback) => ReadDouble(FindS(section, title), fallback);
        private bool GetBool(string section, string title, bool fallback) => ReadBool(Find(section, title), fallback);
        private int GetInt(string section, string title, int fallback) => ReadInt(Find(section, title), fallback);
        private int GetIntS(string section, string title, int fallback) => ReadInt(FindS(section, title), fallback);

        private static double ReadDouble(PropertyBase p, double fallback)
        {
            if (p == null) return fallback;
            if (p is DoubleProperty dp) return dp.Value;
            if (p is BoolProperty bp) return bp.Value ? 1.0 : 0.0;
            var vProp = p.GetType().GetProperty("Value");
            if (vProp != null)
            {
                var v = vProp.GetValue(p);
                if (v is IConvertible)
                {
                    try { return Convert.ToDouble(v, CultureInfo.InvariantCulture); } catch { }
                }
            }
            return fallback;
        }
        private static bool ReadBool(PropertyBase p, bool fallback)
        {
            if (p == null) return fallback;
            if (p is BoolProperty bp) return bp.Value;
            var vProp = p.GetType().GetProperty("Value");
            if (vProp != null)
            {
                var v = vProp.GetValue(p);
                if (v is bool b) return b;
                if (v is IConvertible)
                {
                    try { return Convert.ToDouble(v, CultureInfo.InvariantCulture) != 0.0; } catch { }
                }
            }
            return fallback;
        }
        private static int ReadInt(PropertyBase p, int fallback)
        {
            if (p == null) return fallback;
            switch (p)
            {
                case IntProperty ip: return ip.Value;
                case LongProperty lp: try { checked { return (int)lp.Value; } } catch { return fallback; }
                case FloatProperty fp: return (int)Math.Round(fp.Value);
                case DoubleProperty dp: return (int)Math.Round(dp.Value);
                case BoolProperty bp: return bp.Value ? 1 : 0;
                case StringProperty sp:
                    if (int.TryParse(sp.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var i)) return i;
                    if (double.TryParse(sp.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)) return (int)Math.Round(d);
                    return fallback;
            }
            var vProp = p.GetType().GetProperty("Value");
            if (vProp != null)
            {
                var v = vProp.GetValue(p);
                if (v is int i) return i;
                if (v is long l) { try { checked { return (int)l; } } catch { return fallback; } }
                if (v is float f) return (int)Math.Round(f);
                if (v is double d) return (int)Math.Round(d);
                if (v is bool b) return b ? 1 : 0;
                if (v is string s)
                {
                    if (int.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var i2)) return i2;
                    if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var d2)) return (int)Math.Round(d2);
                }
                if (v is IConvertible)
                {
                    try { return Convert.ToInt32(v, CultureInfo.InvariantCulture); } catch { }
                }
            }
            return fallback;
        }
        private static string GetName(PropertyBase p)
        {
            if (p == null) return null;
            var nameProp = p.GetType().GetProperty("Name");
            var titleProp = p.GetType().GetProperty("Title");
            return nameProp?.GetValue(p)?.ToString() ?? titleProp?.GetValue(p)?.ToString();
        }
        private sealed class StringTupleComparer : IEqualityComparer<(string section, string title)>
        {
            public static readonly StringTupleComparer OrdinalIgnoreCase = new StringTupleComparer();
            private readonly StringComparer _cmp = StringComparer.OrdinalIgnoreCase;
            public bool Equals((string section, string title) x, (string section, string title) y) => _cmp.Equals(x.section, y.section) && _cmp.Equals(x.title, y.title);
            public int GetHashCode((string section, string title) obj) => HashCode.Combine(_cmp.GetHashCode(obj.section ?? string.Empty), _cmp.GetHashCode(obj.title ?? string.Empty));
        }

        private void DigitalIO_Setup_Load(object sender, EventArgs e)
        {

        }

        private void dioModuleListBoxItemsView_Load(object sender, EventArgs e)
        {

        }
    }
}
