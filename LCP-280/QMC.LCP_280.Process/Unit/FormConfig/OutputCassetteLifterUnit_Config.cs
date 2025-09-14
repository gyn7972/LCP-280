using QMC.Common;
using QMC.Common.CustomControl;
using QMC.Common.DIO;
using QMC.Common.IO;
using QMC.Common.IOUtil;
using QMC.Common.Motions;
using QMC.Common.Spectrometer;
using QMC.LCP_280.Process.Component; // TeachingPosition
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Unit
{
    /// <summary>
    /// OutputCassetteLifter Unit Config Form
    /// UI 와 동작 로직 분리 (디자이너는 UI만, 이 파일은 로직)
    /// </summary>
    public partial class OutputCassetteLifterUnit_Config : Form
    {
        #region Fields
        private const string _UNIT_NAME = "OutputCassetteLifter";

        private Equipment _Equipment => Equipment.Instance;
        private OutputCassetteLifter _OutputCassetteLifter { get; set; }
        private OutputCassetteLifterConfig _cfg;

        private readonly Size _designerSize;
        private bool _sizeMismatchWarned;

        // Digital IO 목록 표시용 내부 구조
     
        private readonly List<IoRef> _ioInputs = new List<IoRef>();
        private readonly List<IoRef> _ioOutputs = new List<IoRef>();
        #endregion

        #region Ctor
        public OutputCassetteLifterUnit_Config()
        {
            InitializeComponent();

            InitializeUnit();

            SuspendLayout();
            _designerSize = Size;
            InitializeUI();
            ResumeLayout(true);

            // 출력 항목 클릭 이벤트 연결 (토글)
            outputView.ItemClicked -= new EventHandler<string>(OnOutputItemClicked);
            outputView.ItemClicked += new EventHandler<string>(OnOutputItemClicked);
        }
        #endregion

        #region Initialize
        private void InitializeUnit()
        {
            try
            {
                if (_Equipment.Units.TryGetValue(_UNIT_NAME, out var unit))
                {
                    _OutputCassetteLifter = unit as OutputCassetteLifter;
                    _cfg = _OutputCassetteLifter?.OutputCassetteLifterConfig;
                }

                if (_OutputCassetteLifter == null)
                {
                    MessageBox.Show(
                        $"{_UNIT_NAME} Unit을 찾을 수 없습니다.\nEquipment에 Unit이 등록되어 있는지 확인하세요.",
                        "오류",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unit 초기화 중 오류 발생: {ex.Message}",
                    "오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void InitializeUI()
        {
            try
            {
                SetTeachingPositionItems();
                SetupPositionItemSelectionEvent();
                InitializeRadioButtonView();
                InitializeDigitalIO();
                PopulateAllAxesInJogControl();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("InitializeUI error: " + ex.Message);
            }
        }
        #endregion

        #region Axis (Jog / Teaching Position)
        private void PopulateAllAxesInJogControl()
        {
            try
            {
                if (jogControl == null)
                    return;

                if (_OutputCassetteLifter?.Axes == null || _OutputCassetteLifter.Axes.Count == 0)
                {
                    jogControl.SetTeachingAxisList(null);
                    return;
                }

                var axisNames = _OutputCassetteLifter
                    .Axes
                    .Values
                    .Where(a => a != null)
                    .Select(a => a.Name ?? a.Setup?.Name)
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .Distinct()
                    .ToArray();

                jogControl.SetTeachingAxisList(axisNames);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("PopulateAllAxesInJogControl error: " + ex.Message);
            }
        }

        private void SetTeachingPositionItems()
        {
            try
            {
                if (_OutputCassetteLifter?.TeachingPositions != null &&
                    _OutputCassetteLifter.TeachingPositions.Count > 0)
                {
                    var names = _OutputCassetteLifter
                        .TeachingPositions
                        .Select(tp => tp.Name)
                        .ToArray();

                    positionItemView?.SetItems(names);
                }
                else
                {
                    positionItemView?.SetItems();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("SetTeachingPositionItems error: " + ex.Message);
            }
        }

        private void SetupPositionItemSelectionEvent()
        {
            if (positionItemView == null)
                return;

            positionItemView.ItemSelected -= OnPositionItemSelected;
            positionItemView.ItemSelected += OnPositionItemSelected;
        }

        private void OnPositionItemSelected(object sender, int selectedIndex)
        {
            try
            {
                ShowTeachingPosition(selectedIndex);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("OnPositionItemSelected: " + ex.Message);
            }
        }

        private void ShowTeachingPosition(int selectedIndex)
        {
            if (_OutputCassetteLifter?.OutputCassetteLifterConfig?.TeachingPositions == null)
                return;

            var list = _OutputCassetteLifter.OutputCassetteLifterConfig.TeachingPositions;
            if (selectedIndex < 0 || selectedIndex >= list.Count)
                return;

            var tp = list[selectedIndex];
            var pc = new PropertyCollection();

            pc.Add(new TitleOnlyProperty($"Teaching Position: {tp.Name} (mm, Abs. Pos)"));
            pc.Add(new StringProperty("Description", tp.Description ?? string.Empty));

            foreach (var axis in tp.AxisPositions)
            {
                pc.Add(new DoubleProperty($"{axis.Key} Position (mm)", axis.Value));
            }

            foreach (var kv in tp.ExtraInfo)
            {
                pc.Add(new StringProperty($"Extra: {kv.Key}", kv.Value?.ToString() ?? string.Empty));
            }

            positionEditorView?.SetProperties(pc);
        }
        #endregion

        #region Digital IO
        private static T GetPropValue<T>(object obj, string prop, T def = default(T))
        {
            if (obj == null)
                return def;

            try
            {
                var pi = obj.GetType().GetProperty(prop);
                if (pi == null)
                    return def;

                var v = pi.GetValue(obj, null);
                if (v == null)
                    return def;

                if (v is T variable)
                    return variable;

                return (T)Convert.ChangeType(v, typeof(T));
            }
            catch
            {
                return def;
            }
        }

        private void InitializeDigitalIO()
        {
            try
            {
                if (inputView == null)
                    return;

                var eq = Equipment.Instance;
                var scan = eq?.DioScan;
                var unitIO = eq?.UnitIO;

                if (scan == null || unitIO == null)
                {
                    inputView.SetProperties(new PropertyCollection());
                    outputView.SetProperties(new PropertyCollection());
                    return;
                }

                _ioInputs.Clear();
                _ioOutputs.Clear();

                // Reflection 으로 배열 가져오기 (타입 의존성 제거)
                Array hardInputsArr = Array.CreateInstance(typeof(object), 0);
                Array hardOutputsArr = Array.CreateInstance(typeof(object), 0);

                if (_OutputCassetteLifter?.OutputCassetteLifterConfig != null)
                {
                    var cfg = _OutputCassetteLifter.OutputCassetteLifterConfig;
                    var cfgType = cfg.GetType();

                    var piIn = cfgType.GetProperty("HardInputs");
                    var piOut = cfgType.GetProperty("HardOutputs");

                    var hi = piIn?.GetValue(cfg) as Array;
                    if (hi != null)
                        hardInputsArr = hi;

                    var ho = piOut?.GetValue(cfg) as Array;
                    if (ho != null)
                        hardOutputsArr = ho;
                }

                Func<string, Tuple<string, string>> resolveIn = disp =>
                {
                    if (unitIO?.Modules == null)
                        return new Tuple<string, string>(null, disp);

                    foreach (var m in unitIO.Modules)
                    {
                        if (m?.Inputs == null)
                            continue;

                        foreach (var ch in m.Inputs)
                        {
                            if (string.Equals(ch.DisplayNo, disp, StringComparison.OrdinalIgnoreCase))
                            {
                                return new Tuple<string, string>(m.ModuleName, ch.DisplayNo);
                            }
                        }
                    }

                    return new Tuple<string, string>(null, disp);
                };

                Func<string, Tuple<string, string>> resolveOut = disp =>
                {
                    if (unitIO?.Modules == null)
                        return new Tuple<string, string>(null, disp);

                    foreach (var m in unitIO.Modules)
                    {
                        if (m?.Outputs == null)
                            continue;

                        foreach (var ch in m.Outputs)
                        {
                            if (string.Equals(ch.DisplayNo, disp, StringComparison.OrdinalIgnoreCase))
                            {
                                return new Tuple<string, string>(m.ModuleName, ch.DisplayNo);
                            }
                        }
                    }

                    return new Tuple<string, string>(null, disp);
                };

                // Inputs
                if (hardInputsArr.Length > 0)
                {
                    var pcIn = new PropertyCollection
                    {
                        ShowNoColumn = true,
                        IsInputParameter = false
                    };

                    pcIn.Add(new TitleOnlyProperty("No", "Name", "State"));

                    foreach (var item in hardInputsArr)
                    {
                        string disp = GetPropValue<string>(item, "Disp", string.Empty);
                        string name = GetPropValue<string>(item, "Name", string.Empty);
                        int no = GetPropValue<int>(item, "No", 0);

                        var map = resolveIn(disp);
                        bool cur = false;
                        if (map.Item1 != null)
                        {
                            scan.TryGetInput(map.Item1, map.Item2, out cur);
                        }

                        string nameCell = $"{disp} {name}";
                        var ps = new PropertyState(no.ToString(), nameCell, cur);

                        pcIn.Add(ps);
                        _ioInputs.Add(new IoRef
                        {
                            Module = map.Item1,
                            Disp = map.Item2,
                            Prop = ps
                        });
                    }

                    inputView.SetProperties(pcIn);
                }
                else
                {
                    inputView.SetProperties(new PropertyCollection());
                }

                // Outputs
                if (hardOutputsArr.Length > 0)
                {
                    var pcOut = new PropertyCollection
                    {
                        ShowNoColumn = true,
                        IsInputParameter = false
                    };

                    pcOut.Add(new TitleOnlyProperty("No", "Name", "State"));

                    foreach (var item in hardOutputsArr)
                    {
                        string disp = GetPropValue<string>(item, "Disp", string.Empty);
                        string name = GetPropValue<string>(item, "Name", string.Empty);
                        int no = GetPropValue<int>(item, "No", 0);

                        var map = resolveOut(disp);
                        bool cur = false; // 필요 시 현재 출력 상태 조회

                        string nameCell = $"{disp} {name}";
                        var ps = new PropertyState(no.ToString(), nameCell, cur);

                        pcOut.Add(ps);
                        _ioOutputs.Add(new IoRef
                        {
                            Module = map.Item1,
                            Disp = map.Item2,
                            Prop = ps
                        });
                    }

                    outputView.SetProperties(pcOut);
                }
                else
                {
                    outputView.SetProperties(new PropertyCollection());
                }

                scan.InputChanged -= OnDioInputChanged;
                scan.InputChanged += OnDioInputChanged;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("InitializeDigitalIO error: " + ex.Message);
            }
        }

        private void OnDioInputChanged(string module, string disp, bool value)
        {
            try
            {
                foreach (var item in _ioInputs)
                {
                    if (item.IsSameIO(module, disp))
                    {
                        item.Prop.State = value;
                        inputView.SetStateByKey(disp, value);
                        break;
                    }
                }
            }
            catch
            {
                // 무시
            }
        }
        #endregion

        #region Move / Teaching Execution
        private void InitializeRadioButtonView()
        {
            try
            {
                rbTeachingMoveMode?.SetOptions(true, "Fine", "Coarse");
            }
            catch
            {
                // ignore
            }
        }

        private void btnMovePosition_Click(object sender, EventArgs e)
        {
            try
            {
                if (_OutputCassetteLifter == null)
                    return;

                int selIndex = GetSelectedPositionIndex();
                if (selIndex < 0 ||
                    selIndex >= _OutputCassetteLifter.OutputCassetteLifterConfig.TeachingPositions.Count)
                {
                    MessageBox.Show(
                        "선택된 Teaching Position이 없습니다.",
                        "알림",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                var tp = _OutputCassetteLifter.OutputCassetteLifterConfig.TeachingPositions[selIndex];
                bool isFine = IsFineSelected();

                // 기본 파라미터 (축 Config 에 값 없을 때 fallback)
                double defaultFineVel = 5.0;
                double defaultCoarseVel = 20.0;
                double defaultAcc = 10.0;
                double defaultDec = 10.0;
                double defaultJerk = 50.0;

                var moveResults = new List<int>();

                // 이동 명령
                foreach (var kv in tp.AxisPositions)
                {
                    MotionAxis axis = ResolveAxis(tp, kv.Key);
                    if (axis == null)
                        continue;

                    double vel = isFine
                        ? (axis.Config != null && axis.Config.JogFineVelocity > 0
                            ? axis.Config.JogFineVelocity
                            : defaultFineVel)
                        : (axis.Config != null && axis.Config.JogCoarseVelocity > 0
                            ? axis.Config.JogCoarseVelocity
                            : defaultCoarseVel);

                    double acc = axis.Config != null && axis.Config.JogAcc > 0
                        ? axis.Config.JogAcc
                        : defaultAcc;

                    double dec = axis.Config != null && axis.Config.JogDec > 0
                        ? axis.Config.JogDec
                        : defaultDec;

                    double jerk = axis.Config != null
                        ? (axis.Config.AccJerkPercent + axis.Config.DecJerkPercent) / 2.0
                        : defaultJerk;

                    int rc = axis.MoveAbs(kv.Value, vel, acc, dec, jerk);
                    moveResults.Add(rc);
                }

                // 완료 대기
                int waitErrors = 0;
                foreach (var kv in tp.AxisPositions)
                {
                    MotionAxis axis = ResolveAxis(tp, kv.Key);
                    if (axis == null)
                        continue;

                    int rc = axis.WaitMoveDone(-1);
                    if (rc != 0)
                    {
                        waitErrors++;
                    }
                }

                bool anyMoveFail = moveResults.Any(r => r != 0) || waitErrors > 0;
                if (!anyMoveFail)
                {
                    MessageBox.Show(
                        "Teaching Position 이동 완료",
                        "Move",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(
                        "일부 축 이동 실패 또는 타임아웃",
                        "Move",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Move 처리 중 오류: " + ex.Message,
                    "오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private MotionAxis ResolveAxis(TeachingPosition tp, string axisKey)
        {
            MotionAxis axis = null;

            if (tp.Axes != null)
            {
                tp.Axes.TryGetValue(axisKey, out axis);
            }

            if (axis == null &&
                _OutputCassetteLifter.Axes.TryGetValue(axisKey, out var directAxis))
            {
                axis = directAxis;
            }

            if (axis == null)
            {
                foreach (var pair in _OutputCassetteLifter.Axes)
                {
                    if (pair.Value != null &&
                        string.Equals(pair.Value.Name, axisKey, StringComparison.OrdinalIgnoreCase))
                    {
                        axis = pair.Value;
                        break;
                    }
                }
            }

            return axis;
        }

        private bool IsFineSelected()
        {
            if (rbTeachingMoveMode == null)
                return true;

            try
            {
                var pi = rbTeachingMoveMode.GetType().GetProperty("SelectedIndex");
                if (pi != null)
                {
                    object v = pi.GetValue(rbTeachingMoveMode, null);
                    if (v is int idx)
                    {
                        return idx == 0; // 0 = Fine
                    }
                }
            }
            catch
            {
                // ignore → Fine 기본
            }

            return true;
        }

        private int GetSelectedPositionIndex()
        {
            int selIndex = -1;
            try
            {
                var pi = positionItemView.GetType().GetProperty("SelectedIndex");
                if (pi != null)
                {
                    object val = pi.GetValue(positionItemView, null);
                    if (val is int v)
                    {
                        selIndex = v;
                    }
                }
            }
            catch
            {
                selIndex = -1;
            }

            return selIndex;
        }
        #endregion

        #region Save / Current Position Update
        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (_OutputCassetteLifter == null)
                    return;

                int selIndex = GetSelectedPositionIndex();
                if (selIndex < 0 || selIndex >= _OutputCassetteLifter.TeachingPositions.Count)
                {
                    MessageBox.Show(
                        "선택된 Teaching Position이 없습니다.",
                        "알림",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                // 에디터 반영
                positionEditorView?.Apply();
                var props = positionEditorView?.GetCurrentProperties();
                if (props == null || props.Count == 0)
                {
                    MessageBox.Show(
                        "편집할 데이터가 없습니다.",
                        "알림",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                var target = _OutputCassetteLifter.TeachingPositions[selIndex];

                var newAxisPositions = new Dictionary<string, double>(
                    target.AxisPositions ?? new Dictionary<string, double>());

                string newDescription = target.Description;
                var newExtra = target.ExtraInfo != null
                    ? new Dictionary<string, object>(target.ExtraInfo)
                    : new Dictionary<string, object>();

                foreach (var p in props)
                {
                    // Description
                    if (p is StringProperty spDesc &&
                        string.Equals(p.Title, "Description", StringComparison.OrdinalIgnoreCase))
                    {
                        newDescription = spDesc.Value ?? string.Empty;
                        continue;
                    }

                    // Axis Position
                    if (p is DoubleProperty dp &&
                        p.Title.EndsWith(" Position (mm)", StringComparison.OrdinalIgnoreCase))
                    {
                        var axisKey = p.Title
                            .Substring(0, p.Title.IndexOf(" Position (mm)"))
                            .Trim();
                        newAxisPositions[axisKey] = dp.Value;
                        continue;
                    }

                    // Extra
                    if (p is StringProperty spExtra &&
                        p.Title.StartsWith("Extra:", StringComparison.OrdinalIgnoreCase))
                    {
                        var extraKey = p.Title.Substring("Extra:".Length).Trim();
                        newExtra[extraKey] = spExtra.Value;
                        continue;
                    }
                }

                target.Description = newDescription;
                target.AxisPositions = newAxisPositions;
                target.ExtraInfo = newExtra;

                _OutputCassetteLifter
                    .OutputCassetteLifterConfig
                    .SetTeachingPosition(
                        new TeachingPosition(
                            target.Name,
                            new Dictionary<string, double>(target.AxisPositions),
                            target.Description)
                        {
                            ExtraInfo = new Dictionary<string, object>(target.ExtraInfo)
                        });

                _OutputCassetteLifter
                    .OutputCassetteLifterConfig
                    .LoadAndBindAxes(Equipment.Instance.AxisManager);

                _OutputCassetteLifter.TeachingPositions.Clear();
                foreach (var tp in _OutputCassetteLifter.OutputCassetteLifterConfig.TeachingPositions)
                {
                    _OutputCassetteLifter.TeachingPositions.Add(tp);
                }

                SetTeachingPositionItems();

                MessageBox.Show(
                    "변경된 Teaching Position이 저장되었습니다.",
                    "저장 완료",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "저장 처리 중 오류: " + ex.Message,
                    "오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void btnCurrentPos_Click(object sender, EventArgs e)
        {
            try
            {
                if (_OutputCassetteLifter == null)
                    return;

                int selIndex = GetSelectedPositionIndex();
                if (selIndex < 0 ||
                    _cfg == null ||
                    _cfg.TeachingPositions == null ||
                    selIndex >= _cfg.TeachingPositions.Count)
                {
                    MessageBox.Show(
                        "선택된 Teaching Position이 없습니다.",
                        "알림",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                var tp = _cfg.TeachingPositions[selIndex];
                var updatedPositions = new Dictionary<string, double>();

                foreach (var kv in tp.AxisPositions)
                {
                    string axisKey = kv.Key;
                    double fallback = kv.Value;

                    MotionAxis axis = null;

                    if (tp.Axes != null)
                    {
                        tp.Axes.TryGetValue(axisKey, out axis);
                    }

                    if (axis == null &&
                        _OutputCassetteLifter.Axes != null &&
                        _OutputCassetteLifter.Axes.TryGetValue(axisKey, out var directAxis))
                    {
                        axis = directAxis;
                    }

                    if (axis == null && _OutputCassetteLifter.Axes != null)
                    {
                        foreach (var pair in _OutputCassetteLifter.Axes)
                        {
                            if (pair.Value != null &&
                                string.Equals(pair.Value.Name, axisKey, StringComparison.OrdinalIgnoreCase))
                            {
                                axis = pair.Value;
                                break;
                            }
                        }
                    }

                    double pos = fallback;
                    if (axis != null)
                    {
                        try
                        {
                            pos = axis.GetPosition();
                        }
                        catch
                        {
                            pos = fallback;
                        }
                    }

                    updatedPositions[axisKey] = pos;
                }

                var editorProperties = new PropertyCollection();
                editorProperties.Add(new TitleOnlyProperty($"Teaching Position: {tp.Name} (mm, Abs. Pos)"));
                editorProperties.Add(new StringProperty("Description", tp.Description ?? string.Empty));

                foreach (var ap in updatedPositions)
                {
                    editorProperties.Add(new DoubleProperty($"{ap.Key} Position (mm)", ap.Value));
                }

                foreach (var extra in tp.ExtraInfo)
                {
                    editorProperties.Add(new StringProperty($"Extra: {extra.Key}", extra.Value?.ToString() ?? string.Empty));
                }

                positionEditorView?.SetProperties(editorProperties);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "현재 위치 읽기 중 오류: " + ex.Message,
                    "오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Output Toggle
        private static string NormalizeXYKey(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return raw;

            raw = raw.Trim().ToUpperInvariant();
            var m = Regex.Match(raw, @"^(X|Y)0*(\d+)$");
            if (m.Success)
            {
                var letter = m.Groups[1].Value;
                var digits = m.Groups[2].Value;
                if (string.IsNullOrEmpty(digits))
                {
                    digits = "0";
                }
                return letter + digits;
            }
            return raw;
        }

        private void OnOutputItemClicked(object sender, string key)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key))
                    return;

                var eq = Equipment.Instance;
                var scan = eq?.DioScan;
                if (scan == null)
                    return;

                var cmpKey = NormalizeXYKey(key);

                string module = null;
                string originalDisp = null;

                foreach (var o in _ioOutputs)
                {
                    var storedDisp = o.Disp;
                    if (string.Equals(storedDisp, key, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(NormalizeXYKey(storedDisp), cmpKey, StringComparison.OrdinalIgnoreCase))
                    {
                        module = o.Module;
                        originalDisp = storedDisp;
                        break;
                    }
                }

                if (string.IsNullOrEmpty(module) || string.IsNullOrEmpty(originalDisp))
                    return;

                bool before = false;
                scan.TryGetOutput(module, originalDisp, out before);

                var dr = MessageBox.Show(
                    $"[{module}:{originalDisp}] 현재 상태 = {before}\r\n변경하시겠습니까?",
                    "Output Toggle",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (dr != DialogResult.Yes)
                    return;

                int rc = scan.WriteOutput(module, originalDisp, !before);
                if (rc != 0)
                {
                    MessageBox.Show(
                        $"WriteOutput 실패 (rc={rc})",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                // 캐시 갱신
                scan.RefreshOnce();

                bool after = before;
                scan.TryGetOutput(module, originalDisp, out after);

                try
                {
                    outputView?.SetStateByKey(key, after);

                    if (!string.Equals(key, originalDisp, StringComparison.OrdinalIgnoreCase))
                    {
                        outputView?.SetStateByKey(originalDisp, after);
                    }

                    var norm = NormalizeXYKey(key);
                    if (!string.Equals(norm, key, StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(norm, originalDisp, StringComparison.OrdinalIgnoreCase))
                    {
                        outputView?.SetStateByKey(norm, after);
                    }
                }
                catch
                {
                    // ignore
                }

                MessageBox.Show(
                    $"{originalDisp}: {before} -> {after}",
                    "Info",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Output 토글 처리 중 오류: " + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Misc
        private void button_Test_Click(object sender, EventArgs e)
        {
            var testGyn = new TestGyn();
            testGyn.ShowDialog();
        }

        public void SetPanelSize(int width, int height)
        {
            if (!_sizeMismatchWarned &&
                (width != _designerSize.Width || height != _designerSize.Height))
            {
                Debug.WriteLine(
                    $"[SizeMismatch] Form:{GetType().Name} Designer:{_designerSize.Width}x{_designerSize.Height} Passed:{width}x{height}");
            }

            try
            {
                SuspendLayout();
                Size = new Size(width, height);
                Invalidate();
                Update();
            }
            finally
            {
                ResumeLayout(true);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            int centerX = ClientSize.Width / 2;
            using (Pen blackPen = new Pen(Color.Black, 2))
            {
                e.Graphics.DrawLine(blackPen, centerX, 0, centerX, ClientSize.Height);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }
        #endregion
    }
}