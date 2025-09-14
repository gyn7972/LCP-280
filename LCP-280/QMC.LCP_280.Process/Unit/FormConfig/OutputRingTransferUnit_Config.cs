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
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Unit
{
    public partial class OutputRingTransferUnit_Config : Form
    {
        private const string _UNIT_NAME = "OutputRingTransfer";
        private Equipment _Equipment => Equipment.Instance;
        private OutputRingTransfer _OutputRingTransfer;
        private OutputRingTransferConfig _cfg;
        private readonly Size _designerSize;
        private bool _sizeMismatchWarned;

        
        private readonly List<IoRef> _ioInputs = new List<IoRef>();
        private readonly List<IoRef> _ioOutputs = new List<IoRef>();

        public OutputRingTransferUnit_Config()
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
                    _OutputRingTransfer = unit as OutputRingTransfer;
                    _cfg = _OutputRingTransfer?.OutputRingTransferConfig;
                }

                if (_OutputRingTransfer == null)
                {
                    MessageBox.Show($"{_UNIT_NAME} Unit을 찾을 수 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unit 초기화 오류: " + ex.Message);
            }
        }

        private void InitializeUI()
        {
            try
            {
                SetAxisDefinitionsToAxisListBox();
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

        private void PopulateAllAxesInJogControl()
        {
            try
            {
                if (jogControl == null)
                {
                    return;
                }

                if (_OutputRingTransfer == null || _OutputRingTransfer.Axes == null || _OutputRingTransfer.Axes.Count == 0)
                {
                    jogControl.SetTeachingAxisList(null);
                    return;
                }

                var names = new List<string>();
                foreach (var pair in _OutputRingTransfer.Axes)
                {
                    var axis = pair.Value;
                    if (axis == null) continue;
                    string name = axis.Name;
                    if (string.IsNullOrWhiteSpace(name) && axis.Setup != null)
                    {
                        name = axis.Setup.Name;
                    }
                    if (string.IsNullOrWhiteSpace(name)) continue;
                    if (!names.Contains(name))
                    {
                        names.Add(name);
                    }
                }
                jogControl.SetTeachingAxisList(names.ToArray());
            }
            catch (Exception ex)
            {
                Debug.WriteLine("PopulateAllAxesInJogControl error: " + ex.Message);
            }
        }

        public void SetPanelSize(int width, int height)
        {
            if (!_sizeMismatchWarned && (width != _designerSize.Width || height != _designerSize.Height))
            {
                _sizeMismatchWarned = true; // 단순 플래그만 변경 (팝업 생략)
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

        private void btnCurrentPos_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_Equipment.Units.TryGetValue(_UNIT_NAME, out var unit))
                {
                    return;
                }

                int selIndex = GetSelectedIndex(positionItemView);
                if (selIndex < 0 || _cfg == null || _cfg.TeachingPositions == null || selIndex >= _cfg.TeachingPositions.Count)
                {
                    return;
                }

                var tp = _cfg.TeachingPositions[selIndex];
                var updated = new Dictionary<string, double>();

                foreach (var kv in tp.AxisPositions)
                {
                    string axisKey = kv.Key;
                    double fallback = kv.Value;
                    double pos = fallback;
                    MotionAxis axis = null;

                    if (tp.Axes != null)
                    {
                        tp.Axes.TryGetValue(axisKey, out axis);
                    }

                    if (axis == null && _OutputRingTransfer?.Axes != null)
                    {
                        _OutputRingTransfer.Axes.TryGetValue(axisKey, out axis);
                    }

                    if (axis == null && _OutputRingTransfer?.Axes != null)
                    {
                        foreach (var ap in _OutputRingTransfer.Axes)
                        {
                            if (ap.Value != null && string.Equals(ap.Value.Name, axisKey, StringComparison.OrdinalIgnoreCase))
                            {
                                axis = ap.Value;
                                break;
                            }
                        }
                    }

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
                pc.Add(new TitleOnlyProperty($"Teaching Position: {tp.Name} (mm, Abs. Pos)"));
                pc.Add(new StringProperty("Description", tp.Description ?? string.Empty));

                foreach (var kv in updated)
                {
                    pc.Add(new DoubleProperty(kv.Key + " Position (mm)", kv.Value));
                }

                foreach (var ex in tp.ExtraInfo)
                {
                    pc.Add(new StringProperty("Extra: " + ex.Key, ex.Value?.ToString() ?? string.Empty));
                }

                positionEditorView?.SetProperties(pc);
            }
            catch (Exception ex)
            {
                MessageBox.Show("현재 위치 읽기 오류: " + ex.Message);
            }
        }

        private void InitializeRadioButtonView()
        {
            try
            {
                if (rbTeachingMoveMode != null)
                {
                    rbTeachingMoveMode.SetOptions(true, "Fine", "Coarse");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("RadioButton 오류: " + ex.Message);
            }
        }

        private void SetAxisDefinitionsToAxisListBox()
        {
            try
            {
                var eq = Equipment.Instance;
                if (!eq.Units.TryGetValue(_UNIT_NAME, out var unit))
                {
                    positionItemView?.SetItems();
                    return;
                }

                var transfer = unit as OutputRingTransfer;
                if (transfer == null || transfer.TeachingPositions == null || transfer.TeachingPositions.Count == 0)
                {
                    positionItemView?.SetItems();
                    return;
                }

                var list = new List<string>();
                foreach (var tp in transfer.TeachingPositions)
                {
                    if (tp != null && !string.IsNullOrWhiteSpace(tp.Name))
                    {
                        list.Add(tp.Name);
                    }
                }
                positionItemView?.SetItems(list.ToArray());
            }
            catch (Exception ex)
            {
                Debug.WriteLine("SetAxisDefinitions 오류: " + ex.Message);
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
        }

        private void OnPositionItemSelected(object sender, int index)
        {
            try
            {
                ShowTeachingPositionInPropertyCollectionView(index);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("OnPositionItemSelected 오류: " + ex.Message);
            }
        }

        private void ShowTeachingPositionInPropertyCollectionView(int idx)
        {
            var eq = Equipment.Instance;
            if (!eq.Units.TryGetValue(_UNIT_NAME, out var unit))
            {
                return;
            }

            var transfer = unit as OutputRingTransfer;
            var config = transfer?.OutputRingTransferConfig;
            if (config == null || config.TeachingPositions == null) return;
            if (idx < 0 || idx >= config.TeachingPositions.Count) return;

            var tp = config.TeachingPositions[idx];
            var pc = new PropertyCollection();
            pc.Add(new TitleOnlyProperty($"Teaching Position: {tp.Name} (mm, Abs. Pos)"));
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

        private void InitializeDigitalIO()
        {
            try
            {
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

                var hardInputs = new List<dynamic>();
                var hardOutputs = new List<dynamic>();

                if (eq.Units.TryGetValue(_UNIT_NAME, out var unit) && unit is OutputRingTransfer transfer && transfer.OutputRingTransferConfig != null)
                {
                    var cfg = transfer.OutputRingTransferConfig;
                    var t = cfg.GetType();
                    var piIn = t.GetProperty("HardInputs");
                    if (piIn != null)
                    {
                        foreach (var item in (System.Collections.IEnumerable)piIn.GetValue(cfg))
                        {
                            hardInputs.Add(item);
                        }
                    }
                    var piOut = t.GetProperty("HardOutputs");
                    if (piOut != null)
                    {
                        foreach (var item in (System.Collections.IEnumerable)piOut.GetValue(cfg))
                        {
                            hardOutputs.Add(item);
                        }
                    }
                }

                BuildInputProperties(scan, unitIO, hardInputs);
                BuildOutputProperties(scan, unitIO, hardOutputs);

                scan.InputChanged -= OnDioInputChanged;
                scan.InputChanged += OnDioInputChanged;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("InitializeDigitalIO 오류: " + ex.Message);
            }
        }

        private void BuildInputProperties(DioScanService scan, DIOUnit unitIO, List<dynamic> hardInputs)
        {
            if (hardInputs == null || hardInputs.Count == 0)
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

            foreach (var item in hardInputs)
            {
                string disp = item.Disp;
                string name = item.Name;
                int no = item.No;

                string moduleName;
                string display;
                ResolveInputChannel(unitIO, disp, out moduleName, out display);

                bool state = false;
                if (moduleName != null)
                {
                    scan.TryGetInput(moduleName, display, out state);
                }

                var ps = new PropertyState(no.ToString(), disp + " " + name, state);
                pc.Add(ps);
                _ioInputs.Add(new IoRef { Module = moduleName, Disp = display, Prop = ps });
            }

            inputView.SetProperties(pc);
        }

        private void BuildOutputProperties(DioScanService scan, DIOUnit unitIO, List<dynamic> hardOutputs)
        {
            if (hardOutputs == null || hardOutputs.Count == 0)
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

            foreach (var item in hardOutputs)
            {
                string disp = item.Disp;
                string name = item.Name;
                int no = item.No;

                string moduleName;
                string display;
                ResolveOutputChannel(unitIO, disp, out moduleName, out display);

                bool state = false; // 필요 시 실제 출력 상태 조회
                var ps = new PropertyState(no.ToString(), disp + " " + name, state);
                pc.Add(ps);
                _ioOutputs.Add(new IoRef { Module = moduleName, Disp = display, Prop = ps });
            }

            outputView.SetProperties(pc);
        }

        private void ResolveInputChannel(DIOUnit unitIO, string disp, out string moduleName, out string display)
        {
            moduleName = null;
            display = disp;
            if (unitIO == null || unitIO.Modules == null)
            {
                return;
            }
            foreach (var m in unitIO.Modules)
            {
                if (m == null || m.Inputs == null) continue;
                foreach (var ch in m.Inputs)
                {
                    if (string.Equals(ch.DisplayNo, disp, StringComparison.OrdinalIgnoreCase))
                    {
                        moduleName = m.ModuleName;
                        display = ch.DisplayNo;
                        return;
                    }
                }
            }
        }

        private void ResolveOutputChannel(DIOUnit unitIO, string disp, out string moduleName, out string display)
        {
            moduleName = null;
            display = disp;
            if (unitIO == null || unitIO.Modules == null)
            {
                return;
            }
            foreach (var m in unitIO.Modules)
            {
                if (m == null || m.Outputs == null) continue;
                foreach (var ch in m.Outputs)
                {
                    if (string.Equals(ch.DisplayNo, disp, StringComparison.OrdinalIgnoreCase))
                    {
                        moduleName = m.ModuleName;
                        display = ch.DisplayNo;
                        return;
                    }
                }
            }
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
                        _ioInputs[i].Prop.State = value;
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

        private static string NormalizeXYKey(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return raw;
            string upper = raw.Trim().ToUpperInvariant();
            var m = Regex.Match(upper, @"^(X|Y)0*(\d+)$");
            if (m.Success)
            {
                string letter = m.Groups[1].Value;
                string digits = m.Groups[2].Value;
                if (string.IsNullOrEmpty(digits)) digits = "0";
                return letter + digits;
            }
            return upper;
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

                foreach (var r in _ioOutputs)
                {
                    if (string.Equals(r.Disp, key, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(NormalizeXYKey(r.Disp), cmpKey, StringComparison.OrdinalIgnoreCase))
                    {
                        module = r.Module;
                        originalDisp = r.Disp;
                        break;
                    }
                }

                if (string.IsNullOrEmpty(module) || string.IsNullOrEmpty(originalDisp)) return;

                bool before = false;
                scan.TryGetOutput(module, originalDisp, out before);

                var dr = MessageBox.Show($"[{module}:{originalDisp}] 현재={before}\n변경?", "Output Toggle", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr != DialogResult.Yes) return;

                int rc = scan.WriteOutput(module, originalDisp, !before);
                if (rc != 0)
                {
                    MessageBox.Show("WriteOutput 실패 rc=" + rc);
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
                    if (!string.Equals(norm, key, StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(norm, originalDisp, StringComparison.OrdinalIgnoreCase))
                    {
                        outputView.SetStateByKey(norm, after);
                    }
                }
                catch
                {
                    // ignore UI errors
                }

                MessageBox.Show(originalDisp + ": " + before + "->" + after);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Output 토글 오류: " + ex.Message);
            }
        }

        private void btnMovePosition_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_Equipment.Units.TryGetValue(_UNIT_NAME, out var unit)) return;
                var transfer = unit as OutputRingTransfer;
                if (transfer == null) return;

                int selIndex = GetSelectedIndex(positionItemView);
                if (selIndex < 0 || transfer.OutputRingTransferConfig == null || transfer.OutputRingTransferConfig.TeachingPositions == null || selIndex >= transfer.OutputRingTransferConfig.TeachingPositions.Count)
                {
                    return;
                }

                var tp = transfer.OutputRingTransferConfig.TeachingPositions[selIndex];

                bool isFine = true;
                if (rbTeachingMoveMode != null)
                {
                    try
                    {
                        var prop = rbTeachingMoveMode.GetType().GetProperty("SelectedIndex");
                        if (prop != null)
                        {
                            object v = prop.GetValue(rbTeachingMoveMode, null);
                            if (v is int)
                            {
                                isFine = ((int)v) == 0;
                            }
                        }
                    }
                    catch
                    {
                        isFine = true;
                    }
                }

                double defFine = 5;
                double defCoarse = 20;
                double defAcc = 10;
                double defDec = 10;
                double defJerk = 50;

                foreach (var kv in tp.AxisPositions)
                {
                    string axisKey = kv.Key;
                    double targetPos = kv.Value;
                    MotionAxis axis = null;

                    if (tp.Axes != null)
                    {
                        tp.Axes.TryGetValue(axisKey, out axis);
                    }
                    if (axis == null && transfer.Axes != null)
                    {
                        transfer.Axes.TryGetValue(axisKey, out axis);
                    }
                    if (axis == null && transfer.Axes != null)
                    {
                        foreach (var ap in transfer.Axes)
                        {
                            if (ap.Value != null && string.Equals(ap.Value.Name, axisKey, StringComparison.OrdinalIgnoreCase))
                            {
                                axis = ap.Value;
                                break;
                            }
                        }
                    }
                    if (axis == null) continue;

                    double vel = isFine
                        ? (axis.Config != null && axis.Config.JogFineVelocity > 0 ? axis.Config.JogFineVelocity : defFine)
                        : (axis.Config != null && axis.Config.JogCoarseVelocity > 0 ? axis.Config.JogCoarseVelocity : defCoarse);
                    double acc = axis.Config != null && axis.Config.JogAcc > 0 ? axis.Config.JogAcc : defAcc;
                    double dec = axis.Config != null && axis.Config.JogDec > 0 ? axis.Config.JogDec : defDec;
                    double jerk = axis.Config != null ? (axis.Config.AccJerkPercent + axis.Config.DecJerkPercent) / 2.0 : defJerk;

                    axis.MoveAbs(targetPos, vel, acc, dec, jerk);
                }

                foreach (var kv in tp.AxisPositions)
                {
                    MotionAxis axis = null;
                    if (tp.Axes != null)
                    {
                        tp.Axes.TryGetValue(kv.Key, out axis);
                    }
                    if (axis == null && transfer.Axes != null)
                    {
                        transfer.Axes.TryGetValue(kv.Key, out axis);
                    }
                    if (axis == null) continue;
                    axis.WaitMoveDone(-1);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Move 오류: " + ex.Message);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_Equipment.Units.TryGetValue(_UNIT_NAME, out var unit)) return;
                var transfer = unit as OutputRingTransfer;
                if (transfer == null) return;

                int selIndex = GetSelectedIndex(positionItemView);
                if (selIndex < 0 || transfer.TeachingPositions == null || selIndex >= transfer.TeachingPositions.Count)
                {
                    return;
                }

                positionEditorView?.Apply();
                var props = positionEditorView?.GetCurrentProperties();
                if (props == null) return;

                var target = transfer.TeachingPositions[selIndex];
                var newAxes = new Dictionary<string, double>();
                if (target.AxisPositions != null)
                {
                    foreach (var kv in target.AxisPositions)
                    {
                        newAxes[kv.Key] = kv.Value;
                    }
                }
                var newExtra = new Dictionary<string, object>();
                if (target.ExtraInfo != null)
                {
                    foreach (var kv in target.ExtraInfo)
                    {
                        newExtra[kv.Key] = kv.Value;
                    }
                }
                string newDesc = target.Description;

                foreach (var p in props)
                {
                    if (p is StringProperty && string.Equals(p.Title, "Description", StringComparison.OrdinalIgnoreCase))
                    {
                        var sp = (StringProperty)p;
                        newDesc = sp.Value ?? string.Empty;
                        continue;
                    }
                    if (p is DoubleProperty && p.Title.EndsWith(" Position (mm)", StringComparison.OrdinalIgnoreCase))
                    {
                        string axisKey = p.Title.Substring(0, p.Title.IndexOf(" Position (mm)"));
                        newAxes[axisKey] = ((DoubleProperty)p).Value;
                        continue;
                    }
                    if (p is StringProperty && p.Title.StartsWith("Extra:", StringComparison.OrdinalIgnoreCase))
                    {
                        string extraKey = p.Title.Substring("Extra:".Length).Trim();
                        newExtra[extraKey] = ((StringProperty)p).Value;
                        continue;
                    }
                }

                target.Description = newDesc;
                target.AxisPositions = newAxes;
                target.ExtraInfo = newExtra;

                transfer.OutputRingTransferConfig.SetTeachingPosition(
                    new TeachingPosition(target.Name, new Dictionary<string, double>(newAxes), newDesc)
                    {
                        ExtraInfo = new Dictionary<string, object>(newExtra)
                    });

                transfer.OutputRingTransferConfig.LoadAndBindAxes(Equipment.Instance.AxisManager);
                transfer.TeachingPositions.Clear();
                foreach (var tp in transfer.OutputRingTransferConfig.TeachingPositions)
                {
                    transfer.TeachingPositions.Add(tp);
                }

                SetAxisDefinitionsToAxisListBox();
                MessageBox.Show("저장 완료", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("저장 오류: " + ex.Message);
            }
        }

        private int GetSelectedIndex(object listControl)
        {
            if (listControl == null) return -1;
            try
            {
                var pi = listControl.GetType().GetProperty("SelectedIndex");
                if (pi != null)
                {
                    object v = pi.GetValue(listControl, null);
                    if (v is int) return (int)v;
                }
            }
            catch
            {
                return -1;
            }
            return -1;
        }

        private void OnAxisSelected(object sender, int index)
        {
            // 필요 시 구현
        }
    }
}