using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using QMC.Common;
using QMC.Common.IOUtil;

namespace QMC.LCP_280.Process.Unit
{
    [FormOrder(5)]
    public partial class Cylinder_Setup : Form
    {
        private Timer _axisPosTimer;
        private Cylinder _selectedCylinder;
        private PropertyCollection _ioStateProperties;
        private bool _configDirty = false;

        // === 추가: 맵퍼 & 설정 객체 ===
        private ConfigReflectionMapper _mapper;
        private CylinderConfig _config;

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
                    _mapper = null;
                    _config = null;
                    cylinderPropertyCollectionView.SetProperties(null);
                    inputStatepropertyCollectionView.SetProperties(null);
                    lbStatusValue.Text = "No Selection";
                    UpdateControlButtonCaptions(null);
                    return;
                }

                var name = items[selectedIndex];
                if (!IoAutoBindings.Cylinders.TryGetValue(name, out var cyl))
                {
                    _selectedCylinder = null;
                    _mapper = null;
                    _config = null;
                    cylinderPropertyCollectionView.SetProperties(null);
                    inputStatepropertyCollectionView.SetProperties(null);
                    lbStatusValue.Text = "Not Found";
                    UpdateControlButtonCaptions(null);
                    return;
                }

                _selectedCylinder = cyl;

                // 설정 동기화 후 UI 로드
                SyncConfigReference(_selectedCylinder, forceReload: false);
                LoadConfigToUI(_selectedCylinder);

                // IO 상태 PropertyCollection 구성
                _ioStateProperties = BuildIoStateProperties(_selectedCylinder);
                inputStatepropertyCollectionView.SetProperties(_ioStateProperties);

                UpdateStatusLabel();
                UpdateControlButtonCaptions(_selectedCylinder);
            }
            catch (Exception ex)
            {
                Log.Write("CylinderSetup", "OnSelect", ex.ToString());
            }
        }
        #endregion

        #region 버튼 캡션 동적 변경
        private static readonly (string token, string fwd, string bwd, bool anyDirection)[] _captionPatterns = new[]
        {
            ("UP", "Up", "Down", true),
            ("DOWN", "Up", "Down", true), // DOWN 단독 포함 시에도 Up/Down 페어 유지
            ("CLAMP", "Clamp", "Unclamp", true),
            ("UNCLAMP", "Clamp", "Unclamp", true),
            ("OPEN", "Open", "Close", true),
            ("CLOSE", "Close", "Open", true),
            ("LOCK", "Lock", "Unlock", true),
            ("UNLOCK", "Lock", "Unlock", true),
            ("EXTEND", "Extend", "Retract", true),
            ("RETRACT", "Extend", "Retract", true),
            ("PUSH", "Push", "Return", true),
            ("IN", "In", "Out", true),
            ("OUT", "Out", "In", true)
        };

        private void UpdateControlButtonCaptions(Cylinder cyl)
        {
            if (btn_Forward_Move == null || btn_Backward_Move == null)
                return;

            if (cyl == null)
            {
                btn_Forward_Move.Text = "Forward";
                btn_Backward_Move.Text = "Backward";
                return;
            }

            string source = string.Join(" ", new[]
            {
                cyl.Name,
                SafeUpper(cyl.FwdOutKey), SafeUpper(cyl.BwdOutKey),
                SafeUpper(cyl.FwdInKey), SafeUpper(cyl.BwdInKey)
            }.Where(s => !string.IsNullOrWhiteSpace(s)));

            string forward = null, backward = null;

            // 1) 패턴 매칭 (우선순위: 배열 순서)
            foreach (var p in _captionPatterns)
            {
                if (source.IndexOf(p.token, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    forward = p.fwd;
                    backward = p.bwd;
                    break;
                }
            }

            // 2) 특수 케이스: 동시에 여러 패턴(예: UP & CLAMP) 있으면 이름 기반 우선
            if (forward != null && backward != null)
            {
                // Cylinder 이름에 명시적으로 Up/Down, Clamp 등 있으면 그것을 우선 (이미 forward/backward 설정됨)
            }
            else
            {
                // 3) 명시 패턴을 찾지 못한 경우: 기본 Forward/Backward
                forward = "Forward";
                backward = "Backward";
            }

            btn_Forward_Move.Text = forward;
            btn_Backward_Move.Text = backward;
        }

        private string SafeUpper(string s) => string.IsNullOrWhiteSpace(s) ? string.Empty : s.ToUpperInvariant();
        #endregion

        #region Config 동기화 & 재로드
        private void SyncConfigReference(Cylinder cyl, bool forceReload)
        {
            if (cyl == null) return;

            var probe = new CylinderConfig { Name = cyl.Name };
            var path = probe.GetFilePath();

            CylinderConfig loaded = null;
            bool shouldLoad = forceReload || File.Exists(path);

            if (shouldLoad)
            {
                try
                {
                    loaded = CylinderConfig.LoadOrCreate(path, true, false);
                }
                catch (Exception ex)
                {
                    Log.Write("CylinderSetup", $"Config load failed: {cyl.Name} - {ex.Message}");
                }
            }

            var effective = loaded
                            ?? cyl._config
                            ?? (cyl.Config as CylinderConfig)
                            ?? new CylinderConfig { Name = cyl.Name };

            //cyl._config = effective;
            cyl.Config = effective;
        }
        #endregion

        #region Config UI 로드 / 저장 (Mapper 사용)
        private CylinderConfig GetEffectiveConfig(Cylinder cyl)
            => cyl?._config ?? (cyl?.Config as CylinderConfig);

        private void LoadConfigToUI(Cylinder cyl)
        {
            _mapper = null;
            _config = null;
            cylinderPropertyCollectionView.SetProperties(null);

            if (cyl == null) return;

            var cfg = GetEffectiveConfig(cyl);
            if (cfg == null) return;

            // 파일 최신 로드 (선택)
            try { cfg.Load(); } catch { }

            _config = cfg;
            _mapper = cfg.CreateMapper();

            // 병합 PropertyCollection 생성 (Cylinder 기본 정보 + Config 속성 그룹)
            var merged = new PropertyCollection { IsInputParameter = true };
            merged.Add(new TitleOnlyProperty("Cylinder Info"));
            merged.Add(new StringProperty("Name", cyl.Name));
            merged.Add(new StringProperty("Forward Out Key", cyl.FwdOutKey));
            merged.Add(new StringProperty("Backward Out Key", cyl.BwdOutKey));
            merged.Add(new StringProperty("Forward In Key", cyl.FwdInKey));
            merged.Add(new StringProperty("Backward In Key", cyl.BwdInKey));
            merged.Add(new TitleOnlyProperty("Config"));

            foreach (var p in _mapper.PropertyCollection)
                merged.Add(p);

            cylinderPropertyCollectionView.GroupName = $"{cyl.Name} Config";
            cylinderPropertyCollectionView.SetProperties(merged);
        }

        private void SaveConfigFromUI()
        {
            if (_mapper == null || _config == null) return;

            try
            {
                cylinderPropertyCollectionView.Apply();
                var pc = cylinderPropertyCollectionView.GetCurrentProperties();
                if (pc == null) return;

                _mapper.ApplyToObject(pc);
                _config.Save();
                _configDirty = false;

                MessageBox.Show("저장 완료", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                // 저장 후 다시 로드(정규화 반영)
                LoadConfigToUI(_selectedCylinder);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"저장 실패: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region IO 상태 Property
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
            if (_selectedCylinder == null || _ioStateProperties == null) 
                return;

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
            if (string.IsNullOrWhiteSpace(key)) 
                return null;
            try { 
                if (DIO.In(key, out var on)) 
                    return on; 
            } 
            catch 
            { }
            //return null;
            return false;
        }

        private void SetStateValue(PropertyCollection pc, string title, bool? value, bool isSensor = false)
        {
            if (pc == null) 
                return;
            var listField = typeof(PropertyCollection).GetField("_properties", BindingFlags.NonPublic | BindingFlags.Instance);
            if (listField == null) 
                return;
            var list = listField.GetValue(pc) as System.Collections.IList;
            if (list == null) 
                return;

            bool targetVal = value ?? false;

            foreach (var obj in list)
            {
                if (obj is PropertyBase pb)
                {
                    // Title 또는 Name 얻기
                    var titleProp = pb.GetType().GetProperty("Title") ?? pb.GetType().GetProperty("Name");
                    if (titleProp == null)
                        continue;

                    var currentTitle = titleProp.GetValue(pb)?.ToString();
                    if (!string.Equals(currentTitle, title, StringComparison.OrdinalIgnoreCase))
                        continue;

                    try
                    {
                        // 1) 파생 클래스에 bool 전용 Value 프로퍼티가 있다면 그것을 우선
                        var valueProp = pb.GetType()
                            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                            .Where(p => p.Name == "Value" && p.CanWrite)
                            .OrderByDescending(p => p.PropertyType == typeof(bool)) // bool 우선
                            .ThenBy(p => p.DeclaringType == typeof(PropertyBase) ? 1 : 0) // 파생 타입 우선
                            .FirstOrDefault();

                        if (valueProp != null)
                        {
                            object boxed = targetVal;
                            // 필요 시 타입 변환 (예: int, string 등)
                            if (valueProp.PropertyType == typeof(string))
                                boxed = targetVal ? "True" : "False";
                            else if (valueProp.PropertyType == typeof(int))
                                boxed = targetVal ? 1 : 0;
                            else if (valueProp.PropertyType == typeof(object))
                                boxed = targetVal;

                            // bool 이 아닌데 직접 변환 안 되면 Convert 시도
                            if (boxed != null && !valueProp.PropertyType.IsAssignableFrom(boxed.GetType()))
                            {
                                try { boxed = Convert.ChangeType(boxed, valueProp.PropertyType, CultureInfo.InvariantCulture); }
                                catch { /* 무시하고 기본 값 유지 */ }
                            }

                            valueProp.SetValue(pb, boxed);
                        }
                        else
                        {
                            // 2) 최후: 기반 PropertyBase.Value 직접 사용 (리플렉션 없이)
                            pb.Value = targetVal;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                        //Log.Write("CylinderSetup", "SetStateValue", ex.ToString());
                    }
                    break;
                }
            }
            //try
            //{
            //    foreach (var obj in list)
            //    {
            //        if (obj is PropertyBase pb)
            //        {
            //            var tProp = pb.GetType().GetProperty("Title") ?? pb.GetType().GetProperty("Name");
            //            var vProp = pb.GetType().GetProperty("Value");
            //            if (tProp == null || vProp == null)
            //                continue;

            //            var tVal = tProp.GetValue(pb)?.ToString();
            //            if (string.Equals(tVal, title, StringComparison.OrdinalIgnoreCase))
            //            {
            //                vProp.SetValue(pb, value ?? false);
            //                break;
            //            }
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Log.Write(ex);
            //}
            
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
            SaveConfigFromUI();
        }
        #endregion

        #region (이전) 값 읽기 Helper (현재 자동 매퍼 사용으로 미사용 – 필요시 제거 가능)
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

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void tableLayoutPanel5_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}