using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using QMC.Common;
using QMC.Common.IOUtil;

namespace QMC.LCP_280.Process.Unit
{
    public partial class Cylinder_Setup : Form
    {
        private Timer _axisPosTimer;
        private Cylinder _selectedCylinder;
        private PropertyCollection _ioStateProperties;
        private bool _configDirty = false;

        public Cylinder_Setup()
        {
            InitializeComponent();
            SuspendLayout();
            InitializeUI();
            ResumeLayout(true);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _axisPosTimer?.Stop();
                if (_axisPosTimer != null)
                {
                    _axisPosTimer.Tick -= AxisPosTimer_Tick;
                    _axisPosTimer.Dispose();
                    _axisPosTimer = null;
                }
            }
            base.Dispose(disposing);
        }

        #region 초기화
        private void InitializeUI()
        {
            try
            {
                WireSelectionEvent();
                BindCylinderList();
                InitializeStatusTimer();
            }
            catch (Exception ex)
            {
                Log.Write("CylinderSetup", "Init", ex.ToString());
            }
        }

        private void WireSelectionEvent()
        {
            if (selectItemListBoxItemsView == null) return;
            selectItemListBoxItemsView.ItemSelected -= OnCylinderSelected;
            selectItemListBoxItemsView.ItemSelected += OnCylinderSelected;
        }

        private void BindCylinderList()
        {
            try
            {
                if (IoAutoBindings.Cylinders == null || IoAutoBindings.Cylinders.Count == 0)
                {
                    selectItemListBoxItemsView.SetItems("(No Cylinders)");
                    return;
                }

                var names = IoAutoBindings.Cylinders.Keys
                    .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                selectItemListBoxItemsView.GroupName = "Cylinders";
                selectItemListBoxItemsView.SetItems(names);

                if (names.Length > 0)
                    selectItemListBoxItemsView.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Log.Write("CylinderSetup", "BindList", ex.ToString());
                selectItemListBoxItemsView.SetItems("(Error)");
            }
        }
        #endregion

        #region 선택 처리
        private void OnCylinderSelected(object sender, int selectedIndex)
        {
            try
            {
                _configDirty = false;

                var items = selectItemListBoxItemsView.GetItems();
                if (selectedIndex < 0 || selectedIndex >= items.Length)
                {
                    _selectedCylinder = null;
                    cylinderPropertyCollectionView.SetProperties(null);
                    inputStatepropertyCollectionView.SetProperties(null);
                    lbStatusValue.Text = "No Selection";
                    return;
                }

                var name = items[selectedIndex];
                if (!IoAutoBindings.Cylinders.TryGetValue(name, out var cyl))
                {
                    _selectedCylinder = null;
                    cylinderPropertyCollectionView.SetProperties(null);
                    inputStatepropertyCollectionView.SetProperties(null);
                    lbStatusValue.Text = "Not Found";
                    return;
                }

                _selectedCylinder = cyl;

                // --- 핵심 수정: Config 동기화 (ManualPatch는 cyl.Config만 세팅함) ---
                SyncConfigReference(_selectedCylinder, forceReload: false);

                var cfgProps = BuildCylinderConfigProperties(_selectedCylinder);
                cylinderPropertyCollectionView.SetProperties(cfgProps);

                _ioStateProperties = BuildIoStateProperties(_selectedCylinder);
                inputStatepropertyCollectionView.SetProperties(_ioStateProperties);

                UpdateStatusLabel();
            }
            catch (Exception ex)
            {
                Log.Write("CylinderSetup", "OnSelect", ex.ToString());
            }
        }
        #endregion

        #region Config 동기화 & 재로드
        private void SyncConfigReference(Cylinder cyl, bool forceReload)
        {
            if (cyl == null) return;

            // 1. 파일 경로 계산
            var probe = new CylinderConfig { Name = cyl.Name };
            var path = probe.GetFilePath();

            CylinderConfig loaded = null;

            bool shouldLoad =
                forceReload ||
                File.Exists(path); // 파일 있으면 항상 로드 (기존: _config / Config 존재하면 스킵했던 것이 문제)

            if (shouldLoad)
            {
                try
                {
                    loaded = CylinderConfig.LoadOrCreate(path, true, true);
                }
                catch (Exception ex)
                {
                    Log.Write("CylinderSetup", $"Config load failed: {cyl.Name} - {ex.Message}");
                }
            }

            // 2. 로드 성공 시 그걸로 동기화, 실패 시 기존 것을 유지 / 없으면 새로 생성
            var effective = loaded
                            ?? cyl._config
                            ?? (cyl.Config as CylinderConfig)
                            ?? new CylinderConfig { Name = cyl.Name };

            // 3. 최종 적용(두 레퍼런스 통일)
            cyl._config = effective;
            cyl.Config = effective;
        }
        #endregion

        #region Property Builders
        private CylinderConfig GetEffectiveConfig(Cylinder cyl)
            => cyl?._config ?? (cyl?.Config as CylinderConfig);

        private PropertyCollection BuildCylinderConfigProperties(Cylinder cyl)
        {
            var pc = new PropertyCollection();

            pc.Add(new TitleOnlyProperty("Cylinder"));
            pc.Add(new StringProperty("Name", cyl.Name));
            pc.Add(new StringProperty("Forward Out Key", cyl.FwdOutKey));
            pc.Add(new StringProperty("Backward Out Key", cyl.BwdOutKey));
            pc.Add(new StringProperty("Forward In Key", cyl.FwdInKey));
            pc.Add(new StringProperty("Backward In Key", cyl.BwdInKey));

            var cfg = GetEffectiveConfig(cyl);
            if (cfg != null)
            {
                pc.Add(new TitleOnlyProperty("Config (Editable)"));
                pc.Add(new BoolProperty("Simulation", cfg.IsSimulation));
                pc.Add(new IntProperty("Extend Timeout (ms)", cfg.ExtendTimeout));
                pc.Add(new IntProperty("Retract Timeout (ms)", cfg.RetractTimeout));
                pc.Add(new IntProperty("Settle Delay (ms)", cfg.SettleDelay));
                pc.Add(new IntProperty("Sensor Retry Count", cfg.SensorRetryCount));
            }

            pc.IsInputParameter = true;
            HookDirtyTracking(pc);
            return pc;
        }

        private void HookDirtyTracking(PropertyCollection pc)
        {
            _configDirty = false;
            // 필요 시 TextBox 변경 시 이벤트로 플래그 세팅 (PropertyCollectionView 개선 후 가능)
        }

        private PropertyCollection BuildIoStateProperties(Cylinder cyl)
        {
            var pc = new PropertyCollection();
            pc.Add(new TitleOnlyProperty("IO State"));
            pc.Add(CreateStateProperty("Forward Out", false));
            pc.Add(CreateStateProperty("Backward Out", false));
            pc.Add(CreateStateProperty("Forward Sensor", false));
            pc.Add(CreateStateProperty("Backward Sensor", false));
            pc.IsInputParameter = false;
            return pc;
        }

        private PropertyBase CreateStateProperty(string title, bool value)
        {
            var stateType = AppDomain.CurrentDomain
                .GetAssemblies()
                .Select(a => a.GetType("QMC.Common.PropertyState", false))
                .FirstOrDefault(t => t != null);

            if (stateType != null)
            {
                try
                {
                    var ctor = stateType.GetConstructors()
                        .FirstOrDefault(c =>
                        {
                            var p = c.GetParameters();
                            return p.Length == 3 &&
                                   p[0].ParameterType == typeof(string) &&
                                   p[1].ParameterType == typeof(string) &&
                                   p[2].ParameterType == typeof(bool);
                        });
                    if (ctor != null)
                        return (PropertyBase)ctor.Invoke(new object[] { title, title, value });
                }
                catch { }
            }
            return new BoolProperty(title, value);
        }
        #endregion

        #region 상태 타이머 & 갱신
        private void InitializeStatusTimer()
        {
            _axisPosTimer?.Stop();
            if (_axisPosTimer != null)
                _axisPosTimer.Tick -= AxisPosTimer_Tick;

            _axisPosTimer = new Timer { Interval = 500 };
            _axisPosTimer.Tick += AxisPosTimer_Tick;
            _axisPosTimer.Start();
        }

        private void AxisPosTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                UpdateIoStates();
                UpdateStatusLabel();
            }
            catch (Exception ex)
            {
                Log.Write("CylinderSetup", "Timer", ex.ToString());
            }
        }

        private void UpdateIoStates()
        {
            if (_selectedCylinder == null || _ioStateProperties == null) return;

            bool? fwdOut = TryReadDIO(_selectedCylinder.FwdOutKey);
            bool? bwdOut = TryReadDIO(_selectedCylinder.BwdOutKey);

            bool? fwdIn = (!_selectedCylinder.FwdInKey?.Contains("/*NO_SENSOR*/") ?? false)
                ? TryReadDIO(_selectedCylinder.FwdInKey) : null;
            bool? bwdIn = (!_selectedCylinder.BwdInKey?.Contains("/*NO_SENSOR*/") ?? false)
                ? TryReadDIO(_selectedCylinder.BwdInKey) : null;

            SetStateValue(_ioStateProperties, "Forward Out", fwdOut);
            SetStateValue(_ioStateProperties, "Backward Out", bwdOut);
            SetStateValue(_ioStateProperties, "Forward Sensor", fwdIn, isSensor: true);
            SetStateValue(_ioStateProperties, "Backward Sensor", bwdIn, isSensor: true);

            inputStatepropertyCollectionView.SetProperties(_ioStateProperties);
        }

        private bool? TryReadDIO(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return null;
            try { if (DIO.In(key, out var on)) return on; } catch { }
            return null;
        }

        private void SetStateValue(PropertyCollection pc, string title, bool? value, bool isSensor = false)
        {
            if (pc == null) return;
            var listField = typeof(PropertyCollection).GetField("_properties", BindingFlags.NonPublic | BindingFlags.Instance);
            if (listField == null) return;
            var list = listField.GetValue(pc) as System.Collections.IList;
            if (list == null) return;

            foreach (var obj in list)
            {
                if (obj is PropertyBase pb)
                {
                    var tProp = pb.GetType().GetProperty("Title") ?? pb.GetType().GetProperty("Name");
                    var vProp = pb.GetType().GetProperty("Value");
                    if (tProp == null || vProp == null) continue;

                    var tVal = tProp.GetValue(pb)?.ToString();
                    if (string.Equals(tVal, title, StringComparison.OrdinalIgnoreCase))
                    {
                        vProp.SetValue(pb, value ?? false);
                        break;
                    }
                }
            }
        }

        private void UpdateStatusLabel()
        {
            if (_selectedCylinder == null)
            {
                lbStatusValue.Text = "No Selection";
                lbStatusValue.ForeColor = Color.Gray;
                return;
            }

            bool fwdOn = false;
            bool bwdOn = false;
            bool hasFwd = !_selectedCylinder.FwdInKey?.Contains("/*NO_SENSOR*/") ?? false;
            bool hasBwd = !_selectedCylinder.BwdInKey?.Contains("/*NO_SENSOR*/") ?? false;

            if (hasFwd) DIO.In(_selectedCylinder.FwdInKey, out fwdOn);
            if (hasBwd) DIO.In(_selectedCylinder.BwdInKey, out bwdOn);

            string txt;
            Color color;

            if (hasFwd || hasBwd)
            {
                if (fwdOn && !bwdOn) { txt = "Forward/Up"; color = Color.Lime; }
                else if (!fwdOn && bwdOn) { txt = "Backward/Down"; color = Color.DeepSkyBlue; }
                else if (fwdOn && bwdOn) { txt = "Sensor Conflict"; color = Color.OrangeRed; }
                else { txt = "Intermediate"; color = Color.Yellow; }
            }
            else
            {
                txt = "No Sensors";
                color = Color.LightGray;
            }

            lbStatusValue.Text = $"{_selectedCylinder.Name} : {txt}";
            lbStatusValue.ForeColor = color;
        }
        #endregion

        #region 버튼 이벤트
        private void btn_Forward_Move_Click(object sender, EventArgs e)
        {
            if (_selectedCylinder == null) return;
            try
            {
                var cfg = GetEffectiveConfig(_selectedCylinder);
                _selectedCylinder.Extend(cfg?.ExtendTimeout ?? 1000, cfg?.SettleDelay ?? 50);
                UpdateStatusLabel();
            }
            catch (Exception ex)
            {
                Log.Write("CylinderSetup", "Forward", ex.ToString());
            }
        }

        private void btn_Backward_Move_Click(object sender, EventArgs e)
        {
            if (_selectedCylinder == null) return;
            try
            {
                var cfg = GetEffectiveConfig(_selectedCylinder);
                _selectedCylinder.Retract(cfg?.RetractTimeout ?? 1000, cfg?.SettleDelay ?? 50);
                UpdateStatusLabel();
            }
            catch (Exception ex)
            {
                Log.Write("CylinderSetup", "Backward", ex.ToString());
            }
        }

        private void btn_Save_Setup_Cylinder_Click(object sender, EventArgs e)
        {
            if (_selectedCylinder == null)
            {
                MessageBox.Show("선택된 실린더가 없습니다.", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            try
            {
                cylinderPropertyCollectionView.Apply();
                var pc = cylinderPropertyCollectionView.GetCurrentProperties();
                if (pc == null)
                {
                    MessageBox.Show("저장할 속성이 없습니다.", "Info",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var cfg = GetEffectiveConfig(_selectedCylinder) ?? new CylinderConfig { Name = _selectedCylinder.Name };

                cfg.IsSimulation = SafeGetBool(pc, "Simulation", cfg.IsSimulation);
                cfg.ExtendTimeout = SafeGetInt(pc, "Extend Timeout (ms)", cfg.ExtendTimeout);
                cfg.RetractTimeout = SafeGetInt(pc, "Retract Timeout (ms)", cfg.RetractTimeout);
                cfg.SettleDelay = SafeGetInt(pc, "Settle Delay (ms)", cfg.SettleDelay);
                cfg.SensorRetryCount = SafeGetInt(pc, "Sensor Retry Count", cfg.SensorRetryCount);

                cfg.Validate();
                var path = cfg.GetFilePath();
                cfg.Save(path, true);

                // 저장 후 두 참조 모두 갱신
                _selectedCylinder._config = cfg;
                _selectedCylinder.Config = cfg;

                _configDirty = false;

                MessageBox.Show($"저장 완료\n{path}", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                cylinderPropertyCollectionView.SetProperties(BuildCylinderConfigProperties(_selectedCylinder));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"저장 중 오류: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region 값 읽기 Helper
        private bool SafeGetBool(PropertyCollection pc, string title, bool fallback)
        {
            try { return pc.GetValue<bool>(title); } catch { return fallback; }
        }

        private int SafeGetInt(PropertyCollection pc, string title, int fallback)
        {
            try { return pc.GetValue<int>(title); }
            catch
            {
                try
                {
                    var str = pc.GetValue<string>(title);
                    if (int.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out var i))
                        return i;
                }
                catch { }
                return fallback;
            }
        }
        #endregion

        #region Paint / Resize
        protected override void OnPaint(PaintEventArgs e) => base.OnPaint(e);
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }
        #endregion
    }
}