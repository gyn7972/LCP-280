using QMC.Common;
using QMC.Common.CustomControl;
using QMC.Common.DIO;
using QMC.Common.IO;
using QMC.Common.IOUtil;
using QMC.Common.Motions;
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
    /// IndexUnloadAligner Unit Config Form (Logic Part)
    /// UI 구성요소 선언 및 InitializeComponent 는 Designer partial 에 존재.
    /// 여기서는 데이터 처리 / 이벤트 로직만 유지.
    /// </summary>
    public partial class IndexUnloadAlignerUnit_Config : Form
    {
        private const string UNIT_NAME = "IndexUnloadAligner";

        private Equipment EquipmentInstance
        {
            get => Equipment.Instance;
        }

        private IndexUnloadAligner _unit;
        private IndexUnloadAlignerConfig _config;

        private readonly Size _designerSize;
        private bool _sizeMismatchWarned;

        // ==== Digital IO 표시용 내부 구조 ====
        

        private readonly List<IoRef> _ioInputs = new List<IoRef>();
        private readonly List<IoRef> _ioOutputs = new List<IoRef>();

        public IndexUnloadAlignerUnit_Config()
        {
            InitializeComponent();

            InitializeUnit();

            this.SuspendLayout();
            _designerSize = this.Size;
            InitializeUI();
            this.ResumeLayout(true);

            // OutputView 항목 클릭 - Toggle 처리
            if (this.outputView != null)
            {
                this.outputView.ItemClicked -= new EventHandler<string>(OnOutputItemClicked);
                this.outputView.ItemClicked += new EventHandler<string>(OnOutputItemClicked);
            }

            Console.WriteLine("IndexUnloadAlignerUnit_Config 생성 완료");
        }

        private void InitializeUnit()
        {
            try
            {
                if (EquipmentInstance.Units.TryGetValue(UNIT_NAME, out var rawUnit))
                {
                    _unit = rawUnit as IndexUnloadAligner;
                    if (_unit != null)
                    {
                        _config = _unit.IndexUnloadAlignerConfig;
                    }
                }

                if (_unit == null)
                {
                    MessageBox.Show(
                        $"{UNIT_NAME} Unit을 찾을 수 없습니다.\nEquipment 등록 상태를 확인하세요.",
                        "오류",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
                else
                {
                    Console.WriteLine($"{UNIT_NAME} Unit 연결 성공");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unit 초기화 중 오류: {ex.Message}",
                    "오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        #region UI Initialize

        private void InitializeUI()
        {
            try
            {
                SetTeachingPositionsToList();
                SetupPositionItemSelectionEvent();
                InitializeMoveModeRadioButtons();
                InitializeDigitalIO();
            }
            catch (Exception ex)
            {
                Console.WriteLine("InitializeUI 오류: " + ex.Message);
            }
        }

        private void InitializeMoveModeRadioButtons()
        {
            try
            {
                // RadioButtonView.SetOptions(bool isVertical, params object[] options)
                // true => Vertical 배치
                rbTeachingMoveMode?.SetOptions(true, "Fine", "Coarse");
                rbTeachingMoveMode.SelectedIndex = 0; // 기본 Fine
            }
            catch (Exception ex)
            {
                Console.WriteLine("MoveMode 라디오 초기화 오류: " + ex.Message);
            }
        }

        #endregion

        #region Teaching Position List

        private void SetTeachingPositionsToList()
        {
            try
            {
                if (_unit == null)
                {
                    return;
                }

                if (_unit.TeachingPositions != null && _unit.TeachingPositions.Count > 0)
                {
                    var names = _unit.TeachingPositions
                        .Select(tp => tp.Name)
                        .ToArray();

                    positionItemView?.SetItems(names);

                    Console.WriteLine(
                        $"TeachingPositions 목록 설정: {names.Length} 개"
                    );
                }
                else
                {
                    positionItemView?.SetItems();
                    Console.WriteLine("TeachingPositions 비어 있음");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("TeachingPositions 설정 오류: " + ex.Message);
            }
        }

        private void SetupPositionItemSelectionEvent()
        {
            if (positionItemView == null)
            {
                return;
            }

            positionItemView.ItemSelected -= OnPositionItemSelected;
            positionItemView.ItemSelected += OnPositionItemSelected;

            Console.WriteLine("Position Item 선택 이벤트 설정 완료");
        }

        private void OnPositionItemSelected(
            object sender,
            int selectedIndex)
        {
            try
            {
                ShowTeachingPositionInEditor(selectedIndex);
                UpdateJogAxisFilter(selectedIndex);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "Position Item 선택 처리 오류: " + ex.Message
                );
            }
        }

        private void ShowTeachingPositionInEditor(int selectedIndex)
        {
            if (_unit == null || _config == null)
            {
                return;
            }

            if (_config.TeachingPositions == null)
            {
                return;
            }

            if (selectedIndex < 0 || selectedIndex >= _config.TeachingPositions.Count)
            {
                return;
            }

            var tp = _config.TeachingPositions[selectedIndex];

            var pc = new PropertyCollection();

            pc.Add(
                new TitleOnlyProperty(
                    $"Teaching Position: {tp.Name} (mm, Abs. Pos)"
                )
            );

            pc.Add(
                new StringProperty(
                    title: "Description",
                    value: tp.Description ?? string.Empty
                )
            );

            foreach (var axis in tp.AxisPositions)
            {
                string title = axis.Key + " Position (mm)";
                double value = axis.Value;

                pc.Add(
                    new DoubleProperty(
                        title: title,
                        value: value
                    )
                );
            }

            foreach (var kv in tp.ExtraInfo)
            {
                pc.Add(
                    new StringProperty(
                        title: "Extra: " + kv.Key,
                        value: kv.Value?.ToString() ?? string.Empty
                    )
                );
            }

            positionEditorView?.SetProperties(pc);
        }

        private void UpdateJogAxisFilter(int selectedIndex)
        {
            try
            {
                if (_unit == null || _config == null)
                {
                    return;
                }

                if (selectedIndex < 0 || selectedIndex >= _config.TeachingPositions.Count)
                {
                    return;
                }

                var tp = _config.TeachingPositions[selectedIndex];

                if (tp.AxisPositions != null && jogControl != null)
                {
                    jogControl.SetTeachingAxisList(tp.AxisPositions.Keys);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Jog 축 필터링 오류: " + ex.Message);
            }
        }

        #endregion

        #region Digital IO

        private void InitializeDigitalIO()
        {
            try
            {
                if (inputView == null || outputView == null)
                {
                    return;
                }

                var eq = EquipmentInstance;
                var scan = eq?.DioScan;
                var dioUnit = eq?.UnitIO;

                if (scan == null || dioUnit == null)
                {
                    inputView.SetProperties(new PropertyCollection());
                    outputView.SetProperties(new PropertyCollection());
                    return;
                }

                _ioInputs.Clear();
                _ioOutputs.Clear();

                // HardInputs / HardOutputs 를 reflection 으로 읽기 (타입 정적 의존 제거)
                IEnumerable<object> hardInputs = Enumerable.Empty<object>();
                IEnumerable<object> hardOutputs = Enumerable.Empty<object>();
                if (eq.Units != null && eq.Units.TryGetValue(UNIT_NAME, out var unitObj))
                {
                    var aligner = unitObj as IndexUnloadAligner;
                    if (aligner != null && aligner.IndexUnloadAlignerConfig != null)
                    {
                        var cfg = aligner.IndexUnloadAlignerConfig;
                        var cfgType = cfg.GetType();
                        var piIn = cfgType.GetProperty("HardInputs");
                        var piOut = cfgType.GetProperty("HardOutputs");
                        if (piIn != null)
                        {
                            var val = piIn.GetValue(cfg) as System.Collections.IEnumerable;
                            if (val != null) hardInputs = val.Cast<object>();
                        }
                        if (piOut != null)
                        {
                            var val = piOut.GetValue(cfg) as System.Collections.IEnumerable;
                            if (val != null) hardOutputs = val.Cast<object>();
                        }
                    }
                }

                Tuple<string, string> ResolveInput(string disp)
                {
                    if (dioUnit?.Modules == null) return new Tuple<string, string>(null, disp);
                    foreach (var m in dioUnit.Modules)
                    {
                        if (m?.Inputs == null) continue;
                        foreach (var ch in m.Inputs)
                        {
                            if (string.Equals(ch.DisplayNo, disp, StringComparison.OrdinalIgnoreCase))
                            {
                                return new Tuple<string, string>(m.ModuleName, ch.DisplayNo);
                            }
                        }
                    }
                    return new Tuple<string, string>(null, disp);
                }
                Tuple<string, string> ResolveOutput(string disp)
                {
                    if (dioUnit?.Modules == null) return new Tuple<string, string>(null, disp);
                    foreach (var m in dioUnit.Modules)
                    {
                        if (m?.Outputs == null) continue;
                        foreach (var ch in m.Outputs)
                        {
                            if (string.Equals(ch.DisplayNo, disp, StringComparison.OrdinalIgnoreCase))
                            {
                                return new Tuple<string, string>(m.ModuleName, ch.DisplayNo);
                            }
                        }
                    }
                    return new Tuple<string, string>(null, disp);
                }

                BuildInputProperties(scan, hardInputs, ResolveInput);
                BuildOutputProperties(scan, hardOutputs, ResolveOutput);
                scan.InputChanged -= OnDioInputChanged;
                scan.InputChanged += OnDioInputChanged;
            }
            catch (Exception ex)
            {
                Console.WriteLine("InitializeDigitalIO 오류: " + ex.Message);
            }
        }

        private void BuildInputProperties(DioScanService scan, IEnumerable<object> hardInputs, Func<string, Tuple<string, string>> resolver)
        {
            if (hardInputs == null || !hardInputs.Any())
            {
                inputView.SetProperties(new PropertyCollection());
                return;
            }
            var pc = new PropertyCollection { ShowNoColumn = true, IsInputParameter = false };
            pc.Add(new TitleOnlyProperty("No", "Name", "State"));
            foreach (var def in hardInputs)
            {
                int no = GetIntProperty(def, "No");
                string name = GetStringProperty(def, "Name");
                string disp = GetStringProperty(def, "Disp");
                var map = resolver(disp);
                bool cur = false;
                if (map.Item1 != null)
                {
                    scan.TryGetInput(map.Item1, map.Item2, out cur);
                }
                string nameCell = disp + " " + name;
                var ps = new PropertyState(no.ToString(), nameCell, cur);
                pc.Add(ps);
                _ioInputs.Add(new IoRef { Module = map.Item1, Disp = map.Item2, Prop = ps });
            }
            inputView.SetProperties(pc);
        }

        private void BuildOutputProperties(DioScanService scan, IEnumerable<object> hardOutputs, Func<string, Tuple<string, string>> resolver)
        {
            if (hardOutputs == null || !hardOutputs.Any())
            {
                outputView.SetProperties(new PropertyCollection());
                return;
            }
            var pc = new PropertyCollection { ShowNoColumn = true, IsInputParameter = false };
            pc.Add(new TitleOnlyProperty("No", "Name", "State"));
            foreach (var def in hardOutputs)
            {
                int no = GetIntProperty(def, "No");
                string name = GetStringProperty(def, "Name");
                string disp = GetStringProperty(def, "Disp");
                var map = resolver(disp);
                bool cur = false;
                string nameCell = disp + " " + name;
                var ps = new PropertyState(no.ToString(), nameCell, cur);
                pc.Add(ps);
                _ioOutputs.Add(new IoRef { Module = map.Item1, Disp = map.Item2, Prop = ps });
            }
            outputView.SetProperties(pc);
        }

        private static int GetIntProperty(object obj, string prop)
        {
            if (obj == null) return 0;
            var pi = obj.GetType().GetProperty(prop);
            if (pi == null) return 0;
            try
            {
                var val = pi.GetValue(obj, null);
                if (val == null) return 0;
                return Convert.ToInt32(val);
            }
            catch { return 0; }
        }

        private static string GetStringProperty(object obj, string prop)
        {
            if (obj == null) return string.Empty;
            var pi = obj.GetType().GetProperty(prop);
            if (pi == null) return string.Empty;
            try
            {
                var val = pi.GetValue(obj, null);
                return val?.ToString() ?? string.Empty;
            }
            catch { return string.Empty; }
        }

        private void OnDioInputChanged(string module, string disp, bool value)
        {
            try
            {
                for (int i = 0; i < _ioInputs.Count; i++)
                {
                    var item = _ioInputs[i];
                    if (item.IsSameIO(module, disp))
                    {
                        item.Prop.State = value;
                        inputView.SetStateByKey(disp, value);
                        break;
                    }
                }
            }
            catch { }
        }

        #endregion

        #region Output Toggle

        private static string NormalizeXYKey(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return raw;
            string trimmed = raw.Trim().ToUpperInvariant();
            var m = Regex.Match(trimmed, "^(X|Y)0*(\\d+)$");
            if (m.Success)
            {
                string letter = m.Groups[1].Value;
                string digits = m.Groups[2].Value;
                if (string.IsNullOrEmpty(digits)) digits = "0";
                return letter + digits;
            }
            return trimmed;
        }

        private void OnOutputItemClicked(object sender, string key)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key)) return;
                var eq = EquipmentInstance;
                var scan = eq?.DioScan;
                if (scan == null) return;
                string cmpKey = NormalizeXYKey(key);
                string module = null;
                string originalDisp = null;
                foreach (var item in _ioOutputs)
                {
                    string stored = item.Disp;
                    bool direct = string.Equals(stored, key, StringComparison.OrdinalIgnoreCase);
                    bool normalized = string.Equals(NormalizeXYKey(stored), cmpKey, StringComparison.OrdinalIgnoreCase);
                    if (direct || normalized)
                    {
                        module = item.Module;
                        originalDisp = stored;
                        break;
                    }
                }
                if (string.IsNullOrEmpty(module) || string.IsNullOrEmpty(originalDisp)) return;
                bool before = false;
                scan.TryGetOutput(module, originalDisp, out before);
                var dr = MessageBox.Show("[" + module + ":" + originalDisp + "] 현재=" + before + "\n변경하시겠습니까?", "Output Toggle", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr != DialogResult.Yes) return;
                int rc = scan.WriteOutput(module, originalDisp, !before);
                if (rc != 0)
                {
                    MessageBox.Show("WriteOutput 실패 (rc=" + rc + ")", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                scan.RefreshOnce();
                bool after = before;
                scan.TryGetOutput(module, originalDisp, out after);
                try
                {
                    outputView.SetStateByKey(key, after);
                    if (!string.Equals(key, originalDisp, StringComparison.OrdinalIgnoreCase))
                    {
                        outputView.SetStateByKey(originalDisp, after);
                    }
                    string norm = NormalizeXYKey(key);
                    if (!string.Equals(norm, key, StringComparison.OrdinalIgnoreCase) && !string.Equals(norm, originalDisp, StringComparison.OrdinalIgnoreCase))
                    {
                        outputView.SetStateByKey(norm, after);
                    }
                }
                catch { }
                MessageBox.Show(originalDisp + ": " + before + " -> " + after, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Output 토글 오류: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Move Teaching Position

        private void btnMovePosition_Click(object sender, EventArgs e)
        {
            try
            {
                if (!EquipmentInstance.Units.TryGetValue(UNIT_NAME, out var unitObj))
                {
                    MessageBox.Show("Unit을 찾을 수 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                var aligner = unitObj as IndexUnloadAligner;
                if (aligner == null)
                {
                    MessageBox.Show("Unit 형식 오류", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                int selIndex = GetSelectedPositionIndex();
                if (selIndex < 0 || selIndex >= aligner.IndexUnloadAlignerConfig.TeachingPositions.Count)
                {
                    MessageBox.Show("선택된 Teaching Position이 없습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                var tp = aligner.IndexUnloadAlignerConfig.TeachingPositions[selIndex];
                bool isFine = GetMoveModeIsFine();
                double defaultFineVel = 5.0;
                double defaultCoarseVel = 20.0;
                double defaultAcc = 10.0;
                double defaultDec = 10.0;
                double defaultJerk = 50.0;
                var moveResults = new List<Tuple<string, int>>();
                foreach (var kv in tp.AxisPositions)
                {
                    string axisKey = kv.Key;
                    double targetPos = kv.Value;
                    MotionAxis axis = ResolveAxis(tp, aligner, axisKey);
                    if (axis == null) continue;
                    double vel = isFine ? (axis.Config != null && axis.Config.JogFineVelocity > 0 ? axis.Config.JogFineVelocity : defaultFineVel)
                                        : (axis.Config != null && axis.Config.JogCoarseVelocity > 0 ? axis.Config.JogCoarseVelocity : defaultCoarseVel);
                    double acc = axis.Config != null && axis.Config.JogAcc > 0 ? axis.Config.JogAcc : defaultAcc;
                    double dec = axis.Config != null && axis.Config.JogDec > 0 ? axis.Config.JogDec : defaultDec;
                    double jerk = axis.Config != null ? (axis.Config.AccJerkPercent + axis.Config.DecJerkPercent) / 2.0 : defaultJerk;
                    int rc = axis.MoveAbs(targetPos, vel, acc, dec, jerk);
                    moveResults.Add(new Tuple<string, int>(axisKey, rc));
                }
                int waitErrors = 0;
                foreach (var kv in tp.AxisPositions)
                {
                    MotionAxis axis = ResolveAxis(tp, aligner, kv.Key);
                    if (axis == null) continue;
                    int rc = axis.WaitMoveDone(-1);
                    if (rc != 0) waitErrors++;
                }
                bool anyFail = moveResults.Exists(t => t.Item2 != 0) || waitErrors > 0;
                if (!anyFail)
                {
                    MessageBox.Show("Teaching Position 이동 완료", "Move", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("일부 축 이동 실패 또는 타임아웃", "Move", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Move 처리 중 오류: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private MotionAxis ResolveAxis(TeachingPosition tp, IndexUnloadAligner aligner, string axisKey)
        {
            MotionAxis axis = null;
            if (tp.Axes != null) tp.Axes.TryGetValue(axisKey, out axis);
            if (axis == null && aligner.Axes.TryGetValue(axisKey, out var directAxis)) axis = directAxis;
            if (axis == null)
            {
                foreach (var aPair in aligner.Axes)
                {
                    var candidate = aPair.Value;
                    if (candidate != null && string.Equals(candidate.Name, axisKey, StringComparison.OrdinalIgnoreCase))
                    {
                        axis = candidate;
                        break;
                    }
                }
            }
            return axis;
        }

        private bool GetMoveModeIsFine()
        {
            bool isFine = true;
            if (rbTeachingMoveMode != null)
            {
                try
                {
                    var pi = rbTeachingMoveMode.GetType().GetProperty("SelectedIndex");
                    if (pi != null)
                    {
                        object val = pi.GetValue(rbTeachingMoveMode, null);
                        if (val is int) isFine = ((int)val) == 0;
                    }
                }
                catch { isFine = true; }
            }
            return isFine;
        }

        private int GetSelectedPositionIndex()
        {
            int selIndex = -1;
            if (positionItemView == null) return -1;
            try
            {
                var pi = positionItemView.GetType().GetProperty("SelectedIndex");
                if (pi != null)
                {
                    object val = pi.GetValue(positionItemView, null);
                    if (val is int) selIndex = (int)val;
                }
            }
            catch { selIndex = -1; }
            return selIndex;
        }

        #endregion

        #region Save / Current Position

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (!EquipmentInstance.Units.TryGetValue(UNIT_NAME, out var unitObj))
                {
                    MessageBox.Show("Unit을 찾을 수 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                var aligner = unitObj as IndexUnloadAligner;
                if (aligner == null)
                {
                    MessageBox.Show("Unit 형식이 올바르지 않습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                int selIndex = GetSelectedPositionIndex();
                if (selIndex < 0 || selIndex >= aligner.TeachingPositions.Count)
                {
                    MessageBox.Show("선택된 Teaching Position이 없습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                positionEditorView?.Apply();
                var props = positionEditorView?.GetCurrentProperties();
                if (props == null || props.Count == 0)
                {
                    MessageBox.Show("편집할 데이터가 없습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                var target = aligner.TeachingPositions[selIndex];
                var newAxisPositions = new Dictionary<string, double>(target.AxisPositions ?? new Dictionary<string, double>());
                string newDescription = target.Description;
                var newExtra = target.ExtraInfo != null ? new Dictionary<string, object>(target.ExtraInfo) : new Dictionary<string, object>();
                foreach (var p in props)
                {
                    if (p is StringProperty spDesc && string.Equals(p.Title, "Description", StringComparison.OrdinalIgnoreCase))
                    {
                        newDescription = spDesc.Value ?? string.Empty;
                        continue;
                    }
                    if (p is DoubleProperty dpPos && p.Title.EndsWith(" Position (mm)", StringComparison.OrdinalIgnoreCase))
                    {
                        string axisKey = p.Title.Substring(0, p.Title.IndexOf(" Position (mm)")).Trim();
                        newAxisPositions[axisKey] = dpPos.Value;
                        continue;
                    }
                    if (p is StringProperty spExtra && p.Title.StartsWith("Extra:", StringComparison.OrdinalIgnoreCase))
                    {
                        string extraKey = p.Title.Substring("Extra:".Length).Trim();
                        newExtra[extraKey] = spExtra.Value;
                        continue;
                    }
                }
                target.Description = newDescription;
                target.AxisPositions = newAxisPositions;
                target.ExtraInfo = newExtra;
                aligner.IndexUnloadAlignerConfig.SetTeachingPosition(new TeachingPosition(target.Name, new Dictionary<string, double>(target.AxisPositions), target.Description)
                {
                    ExtraInfo = new Dictionary<string, object>(target.ExtraInfo)
                });
                aligner.IndexUnloadAlignerConfig.LoadAndBindAxes(Equipment.Instance.AxisManager);
                aligner.TeachingPositions.Clear();
                foreach (var tp in aligner.IndexUnloadAlignerConfig.TeachingPositions)
                {
                    aligner.TeachingPositions.Add(tp);
                }
                SetTeachingPositionsToList();
                MessageBox.Show("변경된 Teaching Position이 저장되었습니다.", "저장 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("저장 처리 중 오류: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCurrentPos_Click(object sender, EventArgs e)
        {
            try
            {
                if (!EquipmentInstance.Units.TryGetValue(UNIT_NAME, out var unitObj))
                {
                    MessageBox.Show("Unit을 찾을 수 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                var aligner = unitObj as IndexUnloadAligner;
                if (aligner == null)
                {
                    MessageBox.Show("Unit 형식 오류", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                int selIndex = GetSelectedPositionIndex();
                if (selIndex < 0 || _config == null || _config.TeachingPositions == null || selIndex >= _config.TeachingPositions.Count)
                {
                    MessageBox.Show("선택된 Teaching Position이 없습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                var tp = _config.TeachingPositions[selIndex];
                var updatedPositions = new Dictionary<string, double>();
                foreach (var kv in tp.AxisPositions)
                {
                    string axisKey = kv.Key;
                    double fallback = kv.Value;
                    MotionAxis axis = ResolveAxis(tp, aligner, axisKey);
                    double pos = fallback;
                    if (axis != null)
                    {
                        try { pos = axis.GetPosition(); }
                        catch { pos = fallback; }
                    }
                    updatedPositions[axisKey] = pos;
                }
                var pc = new PropertyCollection();
                pc.Add(new TitleOnlyProperty("Teaching Position: " + tp.Name + " (mm, Abs. Pos)"));
                pc.Add(new StringProperty("Description", tp.Description ?? string.Empty));
                foreach (var ap in updatedPositions)
                {
                    pc.Add(new DoubleProperty(ap.Key + " Position (mm)", ap.Value));
                }
                foreach (var extra in tp.ExtraInfo)
                {
                    pc.Add(new StringProperty("Extra: " + extra.Key, extra.Value?.ToString() ?? string.Empty));
                }
                positionEditorView?.SetProperties(pc);
            }
            catch (Exception ex)
            {
                MessageBox.Show("현재 위치 읽기 오류: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Utility / Layout

        public void SetPanelSize(
            int width,
            int height)
        {
            if (!_sizeMismatchWarned &&
                (width != _designerSize.Width || height != _designerSize.Height))
            {
                string formName = GetType().Name;
                string msg =
                    "폼: " + formName + "\n" +
                    "디자이너 크기: " + _designerSize.Width + " x " + _designerSize.Height + "\n" +
                    "전달 크기: " + width + " x " + height + "\n" +
                    "크기가 일치하지 않습니다.";

#if DEBUG
                Debug.WriteLine("[SizeMismatch] " + msg);
#endif
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

            using (var pen = new Pen(Color.Black, 2))
            {
                e.Graphics.DrawLine(
                    pen,
                    centerX,
                    0,
                    centerX,
                    ClientSize.Height
                );
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }

        private void OnAxisSelected(
            object sender,
            int index)
        {
            // Axis 선택시 필요 기능 추가 예정
        }

        #endregion
    }
}