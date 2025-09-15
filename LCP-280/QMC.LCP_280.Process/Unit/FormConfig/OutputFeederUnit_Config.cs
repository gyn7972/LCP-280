using QMC.Common;
using QMC.Common.Component;
using QMC.Common.CustomControl;
using QMC.Common.DIO;
using QMC.Common.IO;
using QMC.Common.IOUtil;
using QMC.Common.Motions;
using QMC.Common.UI;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Component; // Added for TeachingPosition
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Unit
{
    public partial class OutputFeederUnit_Config : Form
    {
        private const string _UNIT_NAME = "OutputFeeder";

        private Equipment _Equipment => Equipment.Instance;
        private OutputFeeder _unit;
        private OutputFeederConfig _cfg;

        private readonly Size _designerSize;
        private bool _sizeMismatchWarned;

        private readonly List<IoRef> _ioInputs = new List<IoRef>();
        private readonly List<IoRef> _ioOutputs = new List<IoRef>();

        private Timer _axisPosTimer; // reserved
        private Timer _ioTimer;      // reserved

        public OutputFeederUnit_Config()
        {
            InitializeComponent();

            InitializeUnit();

            SuspendLayout();
            _designerSize = Size;
            InitializeUI();
            ResumeLayout(true);

            outputView.ItemClicked -= new EventHandler<string>(OnOutputItemClicked);
            outputView.ItemClicked += new EventHandler<string>(OnOutputItemClicked);
        }

        private void InitializeUnit()
        {
            try
            {
                if (_Equipment.Units.TryGetValue(_UNIT_NAME, out var unit))
                {
                    _unit = unit as OutputFeeder;
                    _cfg = _unit?.Config;
                }

                if (_unit == null)
                {
                    MessageBox.Show(
                        _UNIT_NAME + " Unit을 찾을 수 없습니다.\nEquipment에 Unit이 등록되어 있는지 확인하세요.",
                        "오류",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Unit 초기화 중 오류 발생: " + ex.Message,
                    "오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        #region UI 초기화

        private void InitializeUI()
        {
            try
            {
                PopulateTeachingPositionList();
                HookTeachingPositionSelection();
                InitializeRadioButtonView();
                InitializeDigitalIO();
                PopulateAllAxesInJogControl();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("InitializeUI error: " + ex.Message);
            }
        }

        private void InitializeRadioButtonView()
        {
            try
            {
                rbTeachingMoveMode?.SetOptions(true, "Fine", "Coarse");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("RadioButtonView 오류: " + ex.Message);
            }
        }

        #endregion

        #region Teaching Position 목록 / 표시

        private void PopulateTeachingPositionList()
        {
            try
            {
                if (_unit?.TeachingPositions != null && _unit.TeachingPositions.Count > 0)
                {
                    string[] names = _unit.TeachingPositions
                        .Select(t => t.Name)
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
                Debug.WriteLine("PopulateTeachingPositionList 오류: " + ex.Message);
            }
        }

        private void HookTeachingPositionSelection()
        {
            if (positionItemView == null)
            {
                return;
            }

            positionItemView.ItemSelected -= OnPositionItemSelected;
            positionItemView.ItemSelected += OnPositionItemSelected;
        }

        private void OnPositionItemSelected(object sender, int selectedIndex)
        {
            try
            {
                ShowTeachingPositionInEditor(selectedIndex);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("OnPositionItemSelected error: " + ex.Message);
            }
        }

        private void ShowTeachingPositionInEditor(int selectedIndex)
        {
            if (_cfg?.TeachingPositions == null) return;
            if (selectedIndex < 0 || selectedIndex >= _cfg.TeachingPositions.Count) return;

            var tp = _cfg.TeachingPositions[selectedIndex];

            var pc = new PropertyCollection();
            pc.Add(new TitleOnlyProperty("Teaching Position: " + tp.Name + " (mm, Abs. Pos)"));
            pc.Add(new StringProperty("Description", tp.Description ?? string.Empty));

            foreach (var axis in tp.AxisPositions)
            {
                pc.Add(new DoubleProperty(axis.Key + " Position (mm)", axis.Value));
            }

            foreach (var kv in tp.ExtraInfo)
            {
                pc.Add(new StringProperty("Extra: " + kv.Key, kv.Value?.ToString() ?? string.Empty));
            }

            positionEditorView?.SetProperties(pc);
        }

        #endregion

        #region Move / Save / CurrentPos

        private void btnMovePosition_Click(object sender, EventArgs e)
        {
            try
            {
                if (_unit == null)
                {
                    MessageBox.Show("Unit을 찾을 수 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                int selIndex = GetSelectedTeachingIndex();
                if (selIndex < 0 || selIndex >= _unit.Config.TeachingPositions.Count)
                {
                    MessageBox.Show("선택된 Teaching Position이 없습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var tp = _unit.Config.TeachingPositions[selIndex];
                bool isFine = GetSelectedMoveModeIsFine();

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

                    MotionAxis axis = null;

                    if (tp.Axes != null && tp.Axes.TryGetValue(axisKey, out var boundAxis))
                    {
                        axis = boundAxis;
                    }

                    if (axis == null && _unit.Axes.TryGetValue(axisKey, out var directAxis))
                    {
                        axis = directAxis;
                    }

                    if (axis == null)
                    {
                        foreach (var pair in _unit.Axes)
                        {
                            if (pair.Value != null && string.Equals(pair.Value.Name, axisKey, StringComparison.OrdinalIgnoreCase))
                            {
                                axis = pair.Value;
                                break;
                            }
                        }
                    }

                    if (axis == null)
                    {
                        continue; // axis not found
                    }

                    double vel = isFine
                        ? (axis.Config != null && axis.Config.JogFineVelocity > 0 ? axis.Config.JogFineVelocity : defaultFineVel)
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
                    MotionAxis axis = null;
                    if (tp.Axes != null && tp.Axes.TryGetValue(kv.Key, out var boundAxis2))
                    {
                        axis = boundAxis2;
                    }
                    if (axis == null && _unit.Axes.TryGetValue(kv.Key, out var directAxis2))
                    {
                        axis = directAxis2;
                    }
                    if (axis == null)
                    {
                        continue;
                    }

                    int rc = axis.WaitMoveDone(-1);
                    if (rc != 0)
                    {
                        waitErrors++;
                    }
                }

                bool anyFail = moveResults.Exists(t => t.Item2 != 0) || waitErrors > 0;

                if (!anyFail)
                {
                    MessageBox.Show(
                        "Teaching Position 이동 완료",
                        "Move",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
                else
                {
                    MessageBox.Show(
                        "일부 축 이동 실패 또는 타임아웃",
                        "Move",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Move 처리 중 오류: " + ex.Message,
                    "오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (_unit == null)
                {
                    MessageBox.Show("Unit을 찾을 수 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                int selIndex = GetSelectedTeachingIndex();
                if (selIndex < 0 || selIndex >= _unit.TeachingPositions.Count)
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

                var target = _unit.TeachingPositions[selIndex];
                var newAxisPositions = new Dictionary<string, double>();
                if (target.AxisPositions != null)
                {
                    foreach (var kv in target.AxisPositions)
                    {
                        newAxisPositions[kv.Key] = kv.Value;
                    }
                }

                string newDescription = target.Description;
                var newExtra = new Dictionary<string, object>();
                if (target.ExtraInfo != null)
                {
                    foreach (var kv in target.ExtraInfo)
                    {
                        newExtra[kv.Key] = kv.Value;
                    }
                }

                foreach (var p in props)
                {
                    if (p is StringProperty && string.Equals(p.Title, "Description", StringComparison.OrdinalIgnoreCase))
                    {
                        var sp = (StringProperty)p;
                        newDescription = sp.Value ?? string.Empty;
                        continue;
                    }

                    if (p is DoubleProperty && p.Title.EndsWith(" Position (mm)", StringComparison.OrdinalIgnoreCase))
                    {
                        var dp = (DoubleProperty)p;
                        int pos = p.Title.IndexOf(" Position (mm)", StringComparison.OrdinalIgnoreCase);
                        string axisKey = p.Title.Substring(0, pos).Trim();
                        newAxisPositions[axisKey] = dp.Value;
                        continue;
                    }

                    if (p is StringProperty && p.Title.StartsWith("Extra:", StringComparison.OrdinalIgnoreCase))
                    {
                        var sp = (StringProperty)p;
                        string extraKey = p.Title.Substring("Extra:".Length).Trim();
                        newExtra[extraKey] = sp.Value;
                        continue;
                    }
                }

                target.Description = newDescription;
                target.AxisPositions = newAxisPositions;
                target.ExtraInfo = newExtra;

                _unit.Config.SetTeachingPosition(
                    new TeachingPosition(
                        target.Name,
                        new Dictionary<string, double>(target.AxisPositions),
                        target.Description
                    )
                    {
                        ExtraInfo = new Dictionary<string, object>(target.ExtraInfo)
                    }
                );

                _unit.Config.LoadAndBindAxes(Equipment.Instance.AxisManager);

                var snapshot = _unit.Config.TeachingPositions != null
                     ? new List<TeachingPosition>(_unit.Config.TeachingPositions)
                     : new List<TeachingPosition>();

                _unit.Config.TeachingPositions = snapshot.ToList();
                if (_unit.TeachingPositions != null)
                {
                    _unit.TeachingPositions.Clear();
                    foreach (var tp in snapshot)
                    {
                        _unit.TeachingPositions.Add(tp);
                    }
                }

                PopulateTeachingPositionList();

                MessageBox.Show(
                    "변경된 Teaching Position이 저장되었습니다.",
                    "저장 완료",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "저장 처리 중 오류: " + ex.Message,
                    "오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void btnCurrentPos_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_Equipment.Units.TryGetValue(_UNIT_NAME, out var unit))
                {
                    MessageBox.Show(
                        "Unit을 찾을 수 없습니다.",
                        "오류",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    return;
                }

                int selIndex = GetSelectedTeachingIndex();

                if (selIndex < 0 || _cfg == null || _cfg.TeachingPositions == null || selIndex >= _cfg.TeachingPositions.Count)
                {
                    MessageBox.Show(
                        "선택된 Teaching Position이 없습니다.",
                        "알림",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
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

                    if (axis == null && _unit?.Axes != null && _unit.Axes.TryGetValue(axisKey, out var direct))
                    {
                        axis = direct;
                    }

                    if (axis == null && _unit?.Axes != null)
                    {
                        foreach (var pair in _unit.Axes)
                        {
                            MotionAxis a = pair.Value;
                            if (a != null && string.Equals(a.Name, axisKey, StringComparison.OrdinalIgnoreCase))
                            {
                                axis = a;
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
                editorProperties.Add(new TitleOnlyProperty("Teaching Position: " + tp.Name + " (mm, Abs. Pos)"));
                editorProperties.Add(new StringProperty("Description", tp.Description ?? string.Empty));

                foreach (var ap in updatedPositions)
                {
                    editorProperties.Add(new DoubleProperty(ap.Key + " Position (mm)", ap.Value));
                }

                foreach (var extra in tp.ExtraInfo)
                {
                    editorProperties.Add(new StringProperty("Extra: " + extra.Key, extra.Value?.ToString() ?? string.Empty));
                }

                positionEditorView?.SetProperties(editorProperties);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "현재 위치 읽기 중 오류: " + ex.Message,
                    "오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private int GetSelectedTeachingIndex()
        {
            int selIndex = -1;
            try
            {
                var pi = positionItemView?.GetType().GetProperty("SelectedIndex");
                if (pi != null)
                {
                    object val = pi.GetValue(positionItemView, null);
                    if (val is int)
                    {
                        selIndex = (int)val;
                    }
                }
            }
            catch
            {
                selIndex = -1;
            }
            return selIndex;
        }

        private bool GetSelectedMoveModeIsFine()
        {
            bool isFine = true;
            try
            {
                if (rbTeachingMoveMode != null)
                {
                    var siProp = rbTeachingMoveMode.GetType().GetProperty("SelectedIndex");
                    if (siProp != null)
                    {
                        object v = siProp.GetValue(rbTeachingMoveMode, null);
                        if (v is int)
                        {
                            isFine = ((int)v) == 0;
                        }
                    }
                }
            }
            catch
            {
                isFine = true;
            }
            return isFine;
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

                HardInputDef[] hardInputs;
                HardOutputDef[] hardOutputs;

                if (eq?.Units != null && eq.Units.TryGetValue(_UNIT_NAME, out var unit) && unit is InputCassetteLifter lifter && lifter.Config != null)
                {
                    var cfg = lifter.Config;
                    var cfgType = cfg.GetType();
                    var piIn = cfgType.GetProperty("HardInputs");
                    var piOut = cfgType.GetProperty("HardOutputs");
                    hardInputs = piIn?.GetValue(cfg) as HardInputDef[] ?? Array.Empty<HardInputDef>();
                    hardOutputs = piOut?.GetValue(cfg) as HardOutputDef[] ?? Array.Empty<HardOutputDef>();
                }
                else
                {
                    hardInputs = Array.Empty<HardInputDef>();
                    hardOutputs = Array.Empty<HardOutputDef>();
                }

                Func<string, Tuple<string, string>> resolveIn = disp =>
                {
                    if (unitIO?.Modules == null) return new Tuple<string, string>(null, disp);
                    foreach (var m in unitIO.Modules)
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
                };

                Func<string, Tuple<string, string>> resolveOut = disp =>
                {
                    if (unitIO?.Modules == null) return new Tuple<string, string>(null, disp);
                    foreach (var m in unitIO.Modules)
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
                };

                if (hardInputs.Length > 0)
                {
                    var pcIn = new PropertyCollection { ShowNoColumn = true, IsInputParameter = false };
                    pcIn.Add(new TitleOnlyProperty("No", "Name", "State"));
                    foreach (var item in hardInputs)
                    {
                        var map = resolveIn(item.Disp);
                        bool cur = false;
                        if (map.Item1 != null) scan.TryGetInput(map.Item1, map.Item2, out cur);
                        string nameCell = item.Disp + " " + item.Name;
                        var ps = new PropertyState(item.No.ToString(), nameCell, cur);
                        pcIn.Add(ps);
                        _ioInputs.Add(new IoRef { Module = map.Item1, Disp = map.Item2, Prop = ps });
                    }
                    inputView.SetProperties(pcIn);
                }
                else
                {
                    inputView.SetProperties(new PropertyCollection());
                }

                if (hardOutputs.Length > 0)
                {
                    var pcOut = new PropertyCollection { ShowNoColumn = true, IsInputParameter = false };
                    pcOut.Add(new TitleOnlyProperty("No", "Name", "State"));
                    foreach (var item in hardOutputs)
                    {
                        var map = resolveOut(item.Disp);
                        bool cur = false;
                        string nameCell = item.Disp + " " + item.Name;
                        var ps = new PropertyState(item.No.ToString(), nameCell, cur);
                        pcOut.Add(ps);
                        _ioOutputs.Add(new IoRef { Module = map.Item1, Disp = map.Item2, Prop = ps });
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
                for (int i = 0; i < _ioInputs.Count; i++)
                {
                    var item = _ioInputs[i];
                    if (item.Module == module && item.IsSameIO(module, disp))
                    {
                        item.Prop.State = value;
                        inputView.SetStateByKey(disp, value);
                        break;
                    }
                }
            }
            catch
            {
                // ignore
            }
        }

        private void OnOutputItemClicked(object sender, string key)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key)) return;
                var eq = Equipment.Instance;
                var scan = eq?.DioScan;
                if (scan == null) return;

                string cmpKey = NormalizeXYKey(key);
                string module = null;
                string originalDisp = null;

                foreach (var entry in _ioOutputs)
                {
                    string storedDisp = entry.Disp;
                    if (string.Equals(storedDisp, key, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(NormalizeXYKey(storedDisp), cmpKey, StringComparison.OrdinalIgnoreCase))
                    {
                        module = entry.Module;
                        originalDisp = storedDisp;
                        break;
                    }
                }

                if (string.IsNullOrEmpty(module) || string.IsNullOrEmpty(originalDisp)) return;

                bool before = false;
                scan.TryGetOutput(module, originalDisp, out before);

                var dr = MessageBox.Show(
                    "[" + module + ":" + originalDisp + "] 현재 상태 = " + before + "\r\n변경하시겠습니까?",
                    "Output Toggle",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (dr != DialogResult.Yes) return;

                int rc = scan.WriteOutput(module, originalDisp, !before);
                if (rc != 0)
                {
                    MessageBox.Show(
                        "WriteOutput 실패 (rc=" + rc + ")",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    return;
                }

                scan.RefreshOnce();
                bool after = before;
                scan.TryGetOutput(module, originalDisp, out after);

                if (outputView != null)
                {
                    outputView.SetStateByKey(key, after);
                    if (!string.Equals(key, originalDisp, StringComparison.OrdinalIgnoreCase))
                    {
                        outputView.SetStateByKey(originalDisp, after);
                    }
                    string norm = NormalizeXYKey(key);
                    if (!string.Equals(norm, key, StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(norm, originalDisp, StringComparison.OrdinalIgnoreCase))
                    {
                        outputView.SetStateByKey(norm, after);
                    }
                }

                MessageBox.Show(
                    originalDisp + ": " + before + " -> " + after,
                    "Info",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Output 토글 처리 중 오류: " + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private static string NormalizeXYKey(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return raw;
            string trimmed = raw.Trim().ToUpperInvariant();
            var m = Regex.Match(trimmed, @"^(X|Y)0*(\d+)$");
            if (m.Success)
            {
                string letter = m.Groups[1].Value;
                string digits = m.Groups[2].Value;
                if (string.IsNullOrEmpty(digits)) digits = "0";
                return letter + digits;
            }
            return trimmed;
        }

        #endregion

        #region JogControl

        private void PopulateAllAxesInJogControl()
        {
            try
            {
                if (jogControl == null) return;

                // InitializeUnit()에서 채운 _unit을 그대로 사용
                if (_unit?.Axes == null || _unit.Axes.Count == 0)
                {
                    jogControl.SetTeachingAxisList(null);
                    return;
                }
                string[] axisNames = _unit.Axes.Values
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

        #endregion

        #region Panel Size

        public void SetPanelSize(int width, int height)
        {
            if (!_sizeMismatchWarned && (width != _designerSize.Width || height != _designerSize.Height))
            {
                _sizeMismatchWarned = true;
                Debug.WriteLine(
                    "[SizeMismatch] Form=" + GetType().Name +
                    " Designer=" + _designerSize.Width + "x" + _designerSize.Height +
                    " Requested=" + width + "x" + height
                );
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

        #endregion

        #region Axis 선택 / 위치 업데이트 (확장 포인트)

        private void OnAxisSelected(object sender, int index)
        {
            // 필요 시 구현
        }

        private void UpdateAxisActualPosition()
        {
            // Timer 활용 예정 지점
        }

        #endregion

        #region Paint / Resize

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            int centerX = ClientSize.Width / 2;
            using (var blackPen = new Pen(Color.Black, 2))
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