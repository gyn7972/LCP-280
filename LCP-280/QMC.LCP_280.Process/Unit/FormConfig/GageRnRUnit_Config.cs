using QMC.Common;
using QMC.Common.DIO;
using QMC.Common.IO;
using QMC.Common.IOUtil;
using QMC.Common.Motions;
using QMC.LCP_280.Process.Component;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Unit
{
    public partial class GageRnRUnit_Config : Form
    {
        private const string UNIT_NAME = "GageRnR";

        private Equipment EquipmentInstance => Equipment.Instance;

        private GageRnR _unit;
        private GageRnRConfig _config;

        private readonly Size _designerSize;
        private bool _sizeMismatchWarned;

        

        private readonly List<IoRef> _ioInputs = new List<IoRef>();
        private readonly List<IoRef> _ioOutputs = new List<IoRef>();

        public GageRnRUnit_Config()
        {
            InitializeComponent();

            InitializeUnit();

            _designerSize = this.Size;

            InitializeUI();

            if (outputView != null)
            {
                outputView.ItemClicked -= new EventHandler<string>(OnOutputItemClicked);
                outputView.ItemClicked += new EventHandler<string>(OnOutputItemClicked);
            }
        }

        #region Unit Init
        private void InitializeUnit()
        {
            try
            {
                if (EquipmentInstance.Units.TryGetValue(UNIT_NAME, out var raw))
                {
                    _unit = raw as GageRnR;
                    _config = _unit?.GageRnRConfig;
                }

                if (_unit == null)
                {
                    MessageBox.Show(
                        UNIT_NAME + " Unit을 찾을 수 없습니다.\nEquipment 등록 상태를 확인하세요.",
                        "오류",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Unit 초기화 중 오류: " + ex.Message,
                    "오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
        #endregion

        #region UI Init
        private void InitializeUI()
        {
            try
            {
                PopulateTeachingPositionList();
                HookTeachingPositionSelection();
                InitializeMoveModeOptions();
                InitializeDigitalIO();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("InitializeUI 오류: " + ex.Message);
            }
        }

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

            positionItemView.ItemSelected -= OnPositionSelected;
            positionItemView.ItemSelected += OnPositionSelected;
        }

        private void OnPositionSelected(object sender, int index)
        {
            try
            {
                ShowTeachingPositionInEditor(index);
                UpdateJogAxisFilter(index);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("OnPositionSelected 오류: " + ex.Message);
            }
        }

        private void ShowTeachingPositionInEditor(int index)
        {
            if (_config?.TeachingPositions == null)
            {
                return;
            }

            if (index < 0 || index >= _config.TeachingPositions.Count)
            {
                return;
            }

            var tp = _config.TeachingPositions[index];

            var pc = new PropertyCollection();
            pc.Add(new TitleOnlyProperty("Teaching Position: " + tp.Name + " (mm, Abs. Pos)"));
            pc.Add(new StringProperty("Description", tp.Description ?? string.Empty));

            foreach (var kv in tp.AxisPositions)
            {
                pc.Add(new DoubleProperty(kv.Key + " Position (mm)", kv.Value));
            }

            foreach (var extra in tp.ExtraInfo)
            {
                pc.Add(new StringProperty("Extra: " + extra.Key, extra.Value?.ToString() ?? string.Empty));
            }

            positionEditorView?.SetProperties(pc);
        }

        private void InitializeMoveModeOptions()
        {
            try
            {
                rbTeachingMoveMode?.SetOptions(true, "Fine", "Coarse");
                rbTeachingMoveMode.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("InitializeMoveModeOptions 오류: " + ex.Message);
            }
        }

        private void UpdateJogAxisFilter(int index)
        {
            try
            {
                if (_config?.TeachingPositions == null)
                {
                    return;
                }

                if (index < 0 || index >= _config.TeachingPositions.Count)
                {
                    return;
                }

                var tp = _config.TeachingPositions[index];
                if (jogControl != null && tp?.AxisPositions != null)
                {
                    jogControl.SetTeachingAxisList(tp.AxisPositions.Keys);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("UpdateJogAxisFilter 오류: " + ex.Message);
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

                IEnumerable<object> hardInputs = Enumerable.Empty<object>();
                IEnumerable<object> hardOutputs = Enumerable.Empty<object>();

                if (eq.Units.TryGetValue(UNIT_NAME, out var raw) && raw is GageRnR gage && gage.GageRnRConfig != null)
                {
                    var cfg = gage.GageRnRConfig;
                    var t = cfg.GetType();
                    var piIn = t.GetProperty("HardInputs");
                    var piOut = t.GetProperty("HardOutputs");

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

                Tuple<string, string> ResolveInput(string disp)
                {
                    if (dioUnit?.Modules == null)
                    {
                        return new Tuple<string, string>(null, disp);
                    }
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
                    if (dioUnit?.Modules == null)
                    {
                        return new Tuple<string, string>(null, disp);
                    }
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
                Debug.WriteLine("InitializeDigitalIO 오류: " + ex.Message);
            }
        }

        private void BuildInputProperties(
            DioScanService scan,
            IEnumerable<object> hardInputs,
            Func<string, Tuple<string, string>> resolver)
        {
            if (hardInputs == null || !hardInputs.Any())
            {
                inputView.SetProperties(new PropertyCollection());
                return;
            }

            var pc = new PropertyCollection
            {
                ShowNoColumn = true,
                IsInputParameter = false
            };
            pc.Add(new TitleOnlyProperty("No", "Name", "State"));

            foreach (var def in hardInputs)
            {
                int no = GetIntProp(def, "No");
                string name = GetStringProp(def, "Name");
                string disp = GetStringProp(def, "Disp");
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

        private void BuildOutputProperties(
            DioScanService scan,
            IEnumerable<object> hardOutputs,
            Func<string, Tuple<string, string>> resolver)
        {
            if (hardOutputs == null || !hardOutputs.Any())
            {
                outputView.SetProperties(new PropertyCollection());
                return;
            }

            var pc = new PropertyCollection
            {
                ShowNoColumn = true,
                IsInputParameter = false
            };
            pc.Add(new TitleOnlyProperty("No", "Name", "State"));

            foreach (var def in hardOutputs)
            {
                int no = GetIntProp(def, "No");
                string name = GetStringProp(def, "Name");
                string disp = GetStringProp(def, "Disp");
                var map = resolver(disp);
                string nameCell = disp + " " + name;
                var ps = new PropertyState(no.ToString(), nameCell, false);
                pc.Add(ps);
                _ioOutputs.Add(new IoRef { Module = map.Item1, Disp = map.Item2, Prop = ps });
            }

            outputView.SetProperties(pc);
        }

        private static int GetIntProp(object o, string p)
        {
            if (o == null) return 0;
            var pi = o.GetType().GetProperty(p);
            if (pi == null) return 0;
            try
            {
                var v = pi.GetValue(o, null);
                if (v == null) return 0;
                return Convert.ToInt32(v);
            }
            catch { return 0; }
        }

        private static string GetStringProp(object o, string p)
        {
            if (o == null) return string.Empty;
            var pi = o.GetType().GetProperty(p);
            if (pi == null) return string.Empty;
            try
            {
                var v = pi.GetValue(o, null);
                return v?.ToString() ?? string.Empty;
            }
            catch { return string.Empty; }
        }

        private void OnDioInputChanged(string module, string disp, bool value)
        {
            try
            {
                foreach (var item in _ioInputs)
                {
                    if (item.IsSameIO(module, disp) )
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
            raw = raw.Trim().ToUpperInvariant();
            var m = Regex.Match(raw, "^(X|Y)0*(\\d+)$");
            if (m.Success)
            {
                string letter = m.Groups[1].Value;
                string digits = m.Groups[2].Value;
                if (string.IsNullOrEmpty(digits)) digits = "0";
                return letter + digits;
            }
            return raw;
        }

        private void OnOutputItemClicked(object sender, string key)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key)) return;
                var scan = EquipmentInstance?.DioScan;
                if (scan == null) return;

                string cmpKey = NormalizeXYKey(key);
                string module = null;
                string originalDisp = null;

                foreach (var item in _ioOutputs)
                {
                    var stored = item.Disp;
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

                var dr = MessageBox.Show(
                    "[" + module + ":" + originalDisp + "] 현재=" + before + "\n변경하시겠습니까?",
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
        #endregion

        #region Move Teaching Position
        private void btnMovePosition_Click(object sender, EventArgs e)
        {
            try
            {
                if (_unit == null || _config == null)
                {
                    MessageBox.Show("Unit을 찾을 수 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                int selIndex = GetSelectedPositionIndex();
                if (selIndex < 0 || selIndex >= _config.TeachingPositions.Count)
                {
                    MessageBox.Show("선택된 Teaching Position이 없습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var tp = _config.TeachingPositions[selIndex];
                bool isFine = IsFineMode();

                double defaultFineVel = 5.0;
                double defaultCoarseVel = 20.0;
                double defaultAcc = 10.0;
                double defaultDec = 10.0;
                double defaultJerk = 50.0;

                var moveResults = new List<Tuple<string, int>>();

                foreach (var kv in tp.AxisPositions)
                {
                    string axisKey = kv.Key;
                    double target = kv.Value;
                    MotionAxis axis = ResolveAxis(tp, axisKey);
                    if (axis == null) continue;

                    double vel = isFine
                        ? (axis.Config != null && axis.Config.JogFineVelocity > 0 ? axis.Config.JogFineVelocity : defaultFineVel)
                        : (axis.Config != null && axis.Config.JogCoarseVelocity > 0 ? axis.Config.JogCoarseVelocity : defaultCoarseVel);

                    double acc = axis.Config != null && axis.Config.JogAcc > 0 ? axis.Config.JogAcc : defaultAcc;
                    double dec = axis.Config != null && axis.Config.JogDec > 0 ? axis.Config.JogDec : defaultDec;
                    double jerk = axis.Config != null ? (axis.Config.AccJerkPercent + axis.Config.DecJerkPercent) / 2.0 : defaultJerk;

                    int rc = axis.MoveAbs(target, vel, acc, dec, jerk);
                    moveResults.Add(new Tuple<string, int>(axisKey, rc));
                }

                int waitErrors = 0;
                foreach (var kv in tp.AxisPositions)
                {
                    MotionAxis axis = ResolveAxis(tp, kv.Key);
                    if (axis == null) continue;
                    int rc = axis.WaitMoveDone(-1);
                    if (rc != 0) waitErrors++;
                }

                bool anyFail = moveResults.Any(m => m.Item2 != 0) || waitErrors > 0;
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

        private MotionAxis ResolveAxis(TeachingPosition tp, string axisKey)
        {
            MotionAxis axis = null;

            if (tp.Axes != null)
            {
                tp.Axes.TryGetValue(axisKey, out axis);
            }

            if (axis == null && _unit.Axes.TryGetValue(axisKey, out var direct))
            {
                axis = direct;
            }

            if (axis == null)
            {
                foreach (var p in _unit.Axes)
                {
                    var a = p.Value;
                    if (a != null && string.Equals(a.Name, axisKey, StringComparison.OrdinalIgnoreCase))
                    {
                        axis = a;
                        break;
                    }
                }
            }

            return axis;
        }

        private bool IsFineMode()
        {
            try
            {
                if (rbTeachingMoveMode == null)
                {
                    return true;
                }
                var pi = rbTeachingMoveMode.GetType().GetProperty("SelectedIndex");
                if (pi != null)
                {
                    object v = pi.GetValue(rbTeachingMoveMode, null);
                    if (v is int idx)
                    {
                        return idx == 0; // 0 -> Fine
                    }
                }
            }
            catch { }
            return true;
        }

        private int GetSelectedPositionIndex()
        {
            int sel = -1;
            try
            {
                var pi = positionItemView?.GetType().GetProperty("SelectedIndex");
                if (pi != null)
                {
                    object v = pi.GetValue(positionItemView, null);
                    if (v is int i)
                    {
                        sel = i;
                    }
                }
            }
            catch { sel = -1; }
            return sel;
        }
        #endregion

        #region Save / Current Position
        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (_unit == null || _config == null)
                {
                    MessageBox.Show("Unit을 찾을 수 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                int selIndex = GetSelectedPositionIndex();
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
                var newAxisPositions = new Dictionary<string, double>(target.AxisPositions ?? new Dictionary<string, double>());
                string newDescription = target.Description;
                var newExtra = target.ExtraInfo != null
                    ? new Dictionary<string, object>(target.ExtraInfo)
                    : new Dictionary<string, object>();

                foreach (var p in props)
                {
                    if (p is StringProperty spDesc && string.Equals(p.Title, "Description", StringComparison.OrdinalIgnoreCase))
                    {
                        newDescription = spDesc.Value ?? string.Empty;
                        continue;
                    }
                    if (p is DoubleProperty dpPos && p.Title.EndsWith(" Position (mm)", StringComparison.OrdinalIgnoreCase))
                    {
                        string axisKey = p.Title.Substring(0, p.Title.IndexOf(" Position (mm)"))
                            .Trim();
                        newAxisPositions[axisKey] = dpPos.Value;
                        continue;
                    }
                    if (p is StringProperty spExtra && p.Title.StartsWith("Extra:", StringComparison.OrdinalIgnoreCase))
                    {
                        string extraKey = p.Title.Substring("Extra:".Length)
                            .Trim();
                        newExtra[extraKey] = spExtra.Value;
                        continue;
                    }
                }

                target.Description = newDescription;
                target.AxisPositions = newAxisPositions;
                target.ExtraInfo = newExtra;

                _config.SetTeachingPosition(
                    new TeachingPosition(
                        target.Name,
                        new Dictionary<string, double>(target.AxisPositions),
                        target.Description
                    )
                    {
                        ExtraInfo = new Dictionary<string, object>(target.ExtraInfo)
                    }
                );

                _config.LoadAndBindAxes(Equipment.Instance.AxisManager);

                _unit.TeachingPositions.Clear();
                foreach (var tp in _config.TeachingPositions)
                {
                    _unit.TeachingPositions.Add(tp);
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
                if (_unit == null || _config == null)
                {
                    MessageBox.Show("Unit을 찾을 수 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                int selIndex = GetSelectedPositionIndex();
                if (selIndex < 0 || selIndex >= _config.TeachingPositions.Count)
                {
                    MessageBox.Show("선택된 Teaching Position이 없습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var tp = _config.TeachingPositions[selIndex];
                var updated = new Dictionary<string, double>();

                foreach (var kv in tp.AxisPositions)
                {
                    string axisKey = kv.Key;
                    double fallback = kv.Value;
                    MotionAxis axis = ResolveAxis(tp, axisKey);
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
                    updated[axisKey] = pos;
                }

                var pc = new PropertyCollection();
                pc.Add(new TitleOnlyProperty("Teaching Position: " + tp.Name + " (mm, Abs. Pos)"));
                pc.Add(new StringProperty("Description", tp.Description ?? string.Empty));
                foreach (var ap in updated)
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
                MessageBox.Show(
                    "현재 위치 읽기 오류: " + ex.Message,
                    "오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
        #endregion

        #region Layout / Paint
        public void SetPanelSize(int width, int height)
        {
            if (!_sizeMismatchWarned && (width != _designerSize.Width || height != _designerSize.Height))
            {
                string formName = this.GetType().Name;
                string msg =
                    "폼: " + formName + "\n" +
                    "디자이너 크기: " + _designerSize.Width + " x " + _designerSize.Height + "\n" +
                    "전달 크기(SetPanelSize): " + width + " x " + height + "\n" +
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
            int centerX = this.ClientSize.Width / 2;
            using (Pen p = new Pen(Color.Black, 2))
            {
                e.Graphics.DrawLine(p, centerX, 0, centerX, this.ClientSize.Height);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }

        private void OnAxisSelected(object sender, int index)
        {
            // 필요 시 축 선택 로직 추가
        }
        #endregion
    }
}