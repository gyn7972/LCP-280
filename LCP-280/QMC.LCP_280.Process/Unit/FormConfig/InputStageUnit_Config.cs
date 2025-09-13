using QMC.Common;
using QMC.Common.Motions;
using QMC.LCP_280.Process.Component;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using static QMC.LCP_280.Process.Unit.IndexChipProbeController; // TeachingPosition 공유

namespace QMC.LCP_280.Process.Unit
{
    public partial class InputStageUnit_Config : Form
    {
        private const string _UNIT_NAME = "InputStage";
        private Equipment _Equipment => Equipment.Instance;
        private InputStage _InputStage;
        private InputStageConfig _cfg;
        private readonly Size _designerSize;
        private bool _sizeMismatchWarned;

        private struct _IoRef { public string Module; public string Disp; public PropertyState Prop; }
        private readonly List<_IoRef> _ioInputs = new List<_IoRef>();
        private readonly List<_IoRef> _ioOutputs = new List<_IoRef>();

        public InputStageUnit_Config()
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
                    _InputStage = unit as InputStage;
                    _cfg = _InputStage?.InputStageConfig;
                }
                if (_InputStage == null)
                    MessageBox.Show($"{_UNIT_NAME} Unit을 찾을 수 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unit 초기화 중 오류 발생: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            catch (Exception ex) { Console.WriteLine("InitializeUI error: " + ex.Message); }
        }

        private void PopulateAllAxesInJogControl()
        {
            try
            {
                if (jogControl == null) return;
                if (_InputStage?.Axes == null || _InputStage.Axes.Count == 0) { jogControl.SetTeachingAxisList(null); return; }
                var axisNames = _InputStage.Axes.Values.Where(a => a != null).Select(a => a.Name ?? a.Setup?.Name).Where(n => !string.IsNullOrWhiteSpace(n)).Distinct().ToArray();
                jogControl.SetTeachingAxisList(axisNames);
            }
            catch (Exception ex) { Console.WriteLine("PopulateAllAxesInJogControl error: " + ex.Message); }
        }

        public void SetPanelSize(int width, int height)
        {
            // 디자이너 값과 다른 경우 경고(1회)
            if (!_sizeMismatchWarned && (width != _designerSize.Width || height != _designerSize.Height))
            {
                string formName = this.GetType().Name;
                string msg =
                    $"폼: {formName}\n" +
                    $"디자이너 크기: {_designerSize.Width} x {_designerSize.Height}\n" +
                    $"전달 크기(SetPanelSize): {width} x {height}\n\n" +
                    "크기가 일치하지 않습니다.";
#if DEBUG
                Debug.WriteLine($"[SizeMismatch] {msg}");
#endif
                //try { MessageBox.Show(this, msg, "크기 불일치", MessageBoxButtons.OK, MessageBoxIcon.Warning); } catch { /* ignore */ }
                //_sizeMismatchWarned = true;
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

            Console.WriteLine($"📐 {nameof(InputCassetteLifterUnit_Config)}.SetPanelSize → {width}x{height}");
        }

        private void btnCurrentPos_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_Equipment.Units.TryGetValue(_UNIT_NAME, out var unit)) return;
                int selIndex = -1;
                try { var pi = positionItemView?.GetType().GetProperty("SelectedIndex"); if (pi != null) selIndex = (int)pi.GetValue(positionItemView, null); } catch { selIndex = -1; }
                if (selIndex < 0 || _cfg?.TeachingPositions == null || selIndex >= _cfg.TeachingPositions.Count) return;
                var tp = _cfg.TeachingPositions[selIndex];
                var updated = new Dictionary<string, double>();
                foreach (var kv in tp.AxisPositions)
                {
                    string axisKey = kv.Key; double fallback = kv.Value; double pos = fallback; MotionAxis axis = null;
                    if (tp.Axes != null) tp.Axes.TryGetValue(axisKey, out axis);
                    if (axis == null && _InputStage?.Axes != null && _InputStage.Axes.TryGetValue(axisKey, out var direct)) axis = direct;
                    if (axis == null && _InputStage?.Axes != null)
                        foreach (var p in _InputStage.Axes) if (p.Value != null && string.Equals(p.Value.Name, axisKey, StringComparison.OrdinalIgnoreCase)) { axis = p.Value; break; }
                    if (axis != null) { try { pos = axis.GetPosition(); } catch { pos = fallback; } }
                    updated[axisKey] = pos;
                }
                var pc = new PropertyCollection();
                pc.Add(new TitleOnlyProperty($"Teaching Position: {tp.Name} (mm, Abs. Pos)"));
                pc.Add(new StringProperty("Description", tp.Description ?? ""));
                foreach (var ap in updated) pc.Add(new DoubleProperty($"{ap.Key} Position (mm)", ap.Value));
                foreach (var exInfo in tp.ExtraInfo) pc.Add(new StringProperty($"Extra: {exInfo.Key}", exInfo.Value?.ToString() ?? ""));
                positionEditorView?.SetProperties(pc);
            }
            catch (Exception ex) { MessageBox.Show("현재 위치 읽기 오류: " + ex.Message); }
        }

        private void InitializeRadioButtonView() { try { rbTeachingMoveMode?.SetOptions(true, "Fine", "Coarse"); } catch (Exception ex) { Console.WriteLine("RadioButton 오류: " + ex.Message); } }

        private void SetAxisDefinitionsToAxisListBox()
        {
            try
            {
                var eq = Equipment.Instance;
                if (eq.Units.TryGetValue(_UNIT_NAME, out var unit))
                {
                    var stage = unit as InputStage;
                    if (stage?.TeachingPositions != null && stage.TeachingPositions.Count > 0)
                        positionItemView?.SetItems(stage.TeachingPositions.Select(t => t.Name).ToArray());
                    else positionItemView?.SetItems();
                }
            }
            catch (Exception ex) { Console.WriteLine("SetAxisDefinitions 오류: " + ex.Message); }
        }

        private void SetupPositionItemSelectionEvent()
        { if (positionItemView == null) return; positionItemView.ItemSelected -= OnPositionItemSelected; positionItemView.ItemSelected += OnPositionItemSelected; }

        private void OnPositionItemSelected(object sender, int index)
        { try { ShowTeachingPositionInPropertyCollectionView(index); } catch (Exception ex) { Console.WriteLine("OnPositionItemSelected 오류: " + ex.Message); } }

        private void ShowTeachingPositionInPropertyCollectionView(int idx)
        {
            var eq = Equipment.Instance; if (!eq.Units.TryGetValue(_UNIT_NAME, out var unit)) return;
            var stage = unit as InputStage; var config = stage?.InputStageConfig; if (config?.TeachingPositions == null) return;
            if (idx < 0 || idx >= config.TeachingPositions.Count) return; var tp = config.TeachingPositions[idx];
            var pc = new PropertyCollection(); pc.Add(new TitleOnlyProperty($"Teaching Position: {tp.Name} (mm, Abs. Pos)")); pc.Add(new StringProperty("Description", tp.Description ?? ""));
            foreach (var axis in tp.AxisPositions) pc.Add(new DoubleProperty($"{axis.Key} Position (mm)", axis.Value));
            foreach (var kv in tp.ExtraInfo) pc.Add(new StringProperty($"Extra: {kv.Key}", kv.Value?.ToString() ?? ""));
            positionEditorView?.SetProperties(pc);
        }

        private void InitializeDigitalIO()
        {
            try
            {
                var eq = Equipment.Instance; var scan = eq?.DioScan; var unitIO = eq?.UnitIO;
                if (scan == null || unitIO == null) { inputView.SetProperties(new PropertyCollection()); outputView.SetProperties(new PropertyCollection()); return; }
                _ioInputs.Clear(); _ioOutputs.Clear();
                var hardInputs = new List<dynamic>(); var hardOutputs = new List<dynamic>();
                if (eq.Units.TryGetValue(_UNIT_NAME, out var unit) && unit is InputStage stage && stage.InputStageConfig != null)
                {
                    var cfg = stage.InputStageConfig; var t = cfg.GetType();
                    var piIn = t.GetProperty("HardInputs"); if (piIn != null) hardInputs = ((System.Collections.IEnumerable)piIn.GetValue(cfg))?.Cast<dynamic>().ToList() ?? new List<dynamic>();
                    var piOut = t.GetProperty("HardOutputs"); if (piOut != null) hardOutputs = ((System.Collections.IEnumerable)piOut.GetValue(cfg))?.Cast<dynamic>().ToList() ?? new List<dynamic>();
                }
                Func<string, Tuple<string, string>> resolveIn = disp => { if (unitIO?.Modules == null) return Tuple.Create<string, string>(null, disp); foreach (var m in unitIO.Modules) { if (m?.Inputs == null) continue; foreach (var ch in m.Inputs) if (string.Equals(ch.DisplayNo, disp, StringComparison.OrdinalIgnoreCase)) return Tuple.Create(m.ModuleName, ch.DisplayNo); } return Tuple.Create<string, string>(null, disp); };
                Func<string, Tuple<string, string>> resolveOut = disp => { if (unitIO?.Modules == null) return Tuple.Create<string, string>(null, disp); foreach (var m in unitIO.Modules) { if (m?.Outputs == null) continue; foreach (var ch in m.Outputs) if (string.Equals(ch.DisplayNo, disp, StringComparison.OrdinalIgnoreCase)) return Tuple.Create(m.ModuleName, ch.DisplayNo); } return Tuple.Create<string, string>(null, disp); };
                if (hardInputs.Count > 0)
                {
                    var pcIn = new PropertyCollection { ShowNoColumn = true, IsInputParameter = false }; pcIn.Add(new TitleOnlyProperty("No", "Name", "State"));
                    foreach (var it in hardInputs) { string disp = it.Disp; string name = it.Name; int no = it.No; var map = resolveIn(disp); bool cur = false; if (map.Item1 != null) scan.TryGetInput(map.Item1, map.Item2, out cur); var ps = new PropertyState(no.ToString(), $"{disp} {name}", cur); pcIn.Add(ps); _ioInputs.Add(new _IoRef { Module = map.Item1, Disp = map.Item2, Prop = ps }); }
                    inputView.SetProperties(pcIn);
                }
                else inputView.SetProperties(new PropertyCollection());
                if (hardOutputs.Count > 0)
                {
                    var pcOut = new PropertyCollection { ShowNoColumn = true, IsInputParameter = false }; pcOut.Add(new TitleOnlyProperty("No", "Name", "State"));
                    foreach (var it in hardOutputs) { string disp = it.Disp; string name = it.Name; int no = it.No; var map = resolveOut(disp); bool cur = false; var ps = new PropertyState(no.ToString(), $"{disp} {name}", cur); pcOut.Add(ps); _ioOutputs.Add(new _IoRef { Module = map.Item1, Disp = map.Item2, Prop = ps }); }
                    outputView.SetProperties(pcOut);
                }
                else outputView.SetProperties(new PropertyCollection());
                scan.InputChanged -= OnDioInputChanged; scan.InputChanged += OnDioInputChanged;
            }
            catch (Exception ex) { Console.WriteLine("InitializeDigitalIO 오류: " + ex.Message); }
        }

        private void OnDioInputChanged(string module, string disp, bool value)
        {
            try { foreach (var r in _ioInputs) if (r.Module == module && string.Equals(r.Disp, disp, StringComparison.OrdinalIgnoreCase)) { r.Prop.State = value; inputView.SetStateByKey(disp, value); break; } } catch { }
        }

        private static string NormalizeXYKey(string raw)
        { if (string.IsNullOrWhiteSpace(raw)) return raw; raw = raw.Trim().ToUpperInvariant(); var m = Regex.Match(raw, @"^(X|Y)0*(\d+)$"); if (m.Success) { var letter = m.Groups[1].Value; var digits = m.Groups[2].Value; if (string.IsNullOrEmpty(digits)) digits = "0"; return letter + digits; } return raw; }

        private void OnOutputItemClicked(object sender, string key)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key)) return; var eq = Equipment.Instance; var scan = eq?.DioScan; if (scan == null) return; var cmpKey = NormalizeXYKey(key); string module = null; string originalDisp = null;
                foreach (var r in _ioOutputs)
                { if (string.Equals(r.Disp, key, StringComparison.OrdinalIgnoreCase) || string.Equals(NormalizeXYKey(r.Disp), cmpKey, StringComparison.OrdinalIgnoreCase)) { module = r.Module; originalDisp = r.Disp; break; } }
                if (string.IsNullOrEmpty(module) || string.IsNullOrEmpty(originalDisp)) return; bool before = false; scan.TryGetOutput(module, originalDisp, out before);
                var dr = MessageBox.Show($"[{module}:{originalDisp}] 현재={before}\n변경?", "Output Toggle", MessageBoxButtons.YesNo, MessageBoxIcon.Question); if (dr != DialogResult.Yes) return;
                int rc = scan.WriteOutput(module, originalDisp, !before); if (rc != 0) { MessageBox.Show($"WriteOutput 실패 rc={rc}"); return; }
                scan.RefreshOnce(); bool after = before; scan.TryGetOutput(module, originalDisp, out after);
                try { outputView.SetStateByKey(key, after); if (!string.Equals(key, originalDisp, StringComparison.OrdinalIgnoreCase)) outputView.SetStateByKey(originalDisp, after); var norm = NormalizeXYKey(key); if (!string.Equals(norm, key, StringComparison.OrdinalIgnoreCase) && !string.Equals(norm, originalDisp, StringComparison.OrdinalIgnoreCase)) outputView.SetStateByKey(norm, after); } catch { }
                MessageBox.Show($"{originalDisp}: {before}->{after}");
            }
            catch (Exception ex) { MessageBox.Show("Output 토글 오류: " + ex.Message); }
        }

        private void btnMovePosition_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_Equipment.Units.TryGetValue(_UNIT_NAME, out var unit)) return; var stage = unit as InputStage; if (stage == null) return;
                int selIndex = -1; try { var pi = positionItemView?.GetType().GetProperty("SelectedIndex"); if (pi != null) selIndex = (int)pi.GetValue(positionItemView, null); } catch { selIndex = -1; }
                if (selIndex < 0 || stage.InputStageConfig.TeachingPositions == null || selIndex >= stage.InputStageConfig.TeachingPositions.Count) return;
                var tp = stage.InputStageConfig.TeachingPositions[selIndex]; bool isFine = true; try { var si = rbTeachingMoveMode?.GetType().GetProperty("SelectedIndex"); if (si != null) isFine = ((int)si.GetValue(rbTeachingMoveMode, null)) == 0; } catch { }
                double defFine = 5, defCoarse = 20, defAcc = 10, defDec = 10, defJerk = 50;
                foreach (var kv in tp.AxisPositions)
                {
                    MotionAxis axis = null; if (tp.Axes != null) tp.Axes.TryGetValue(kv.Key, out axis); if (axis == null && stage.Axes.TryGetValue(kv.Key, out var direct)) axis = direct; if (axis == null) foreach (var ap in stage.Axes) if (ap.Value != null && string.Equals(ap.Value.Name, kv.Key, StringComparison.OrdinalIgnoreCase)) { axis = ap.Value; break; }
                    if (axis == null) continue; double vel = isFine ? (axis.Config?.JogFineVelocity > 0 ? axis.Config.JogFineVelocity : defFine) : (axis.Config?.JogCoarseVelocity > 0 ? axis.Config.JogCoarseVelocity : defCoarse); double acc = axis.Config?.JogAcc > 0 ? axis.Config.JogAcc : defAcc; double dec = axis.Config?.JogDec > 0 ? axis.Config.JogDec : defDec; double jerk = axis.Config != null ? (axis.Config.AccJerkPercent + axis.Config.DecJerkPercent) / 2.0 : defJerk; axis.MoveAbs(kv.Value, vel, acc, dec, jerk);
                }
                foreach (var kv in tp.AxisPositions)
                { MotionAxis axis = null; if (tp.Axes != null) tp.Axes.TryGetValue(kv.Key, out axis); if (axis == null && stage.Axes.TryGetValue(kv.Key, out var direct)) axis = direct; if (axis == null) continue; axis.WaitMoveDone(-1); }
            }
            catch (Exception ex) { MessageBox.Show("Move 오류: " + ex.Message); }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_Equipment.Units.TryGetValue(_UNIT_NAME, out var unit)) return; var stage = unit as InputStage; if (stage == null) return;
                int selIndex = -1; try { var pi = positionItemView?.GetType().GetProperty("SelectedIndex"); if (pi != null) selIndex = (int)pi.GetValue(positionItemView, null); } catch { selIndex = -1; }
                if (selIndex < 0 || stage.TeachingPositions == null || selIndex >= stage.TeachingPositions.Count) return;
                positionEditorView?.Apply(); var props = positionEditorView?.GetCurrentProperties(); if (props == null) return;
                var target = stage.TeachingPositions[selIndex]; var newAxes = new Dictionary<string, double>(target.AxisPositions ?? new Dictionary<string, double>()); var newExtra = target.ExtraInfo != null ? new Dictionary<string, object>(target.ExtraInfo) : new Dictionary<string, object>(); string newDesc = target.Description;
                foreach (var p in props)
                {
                    if (p is StringProperty && string.Equals(p.Title, "Description", StringComparison.OrdinalIgnoreCase)) { newDesc = ((StringProperty)p).Value ?? ""; continue; }
                    if (p is DoubleProperty && p.Title.EndsWith(" Position (mm)", StringComparison.OrdinalIgnoreCase)) { var axisKey = p.Title.Substring(0, p.Title.IndexOf(" Position (mm)")); newAxes[axisKey] = ((DoubleProperty)p).Value; continue; }
                    if (p is StringProperty && p.Title.StartsWith("Extra:", StringComparison.OrdinalIgnoreCase)) { var key = p.Title.Substring("Extra:".Length).Trim(); newExtra[key] = ((StringProperty)p).Value; continue; }
                }
                target.Description = newDesc; target.AxisPositions = newAxes; target.ExtraInfo = newExtra;
                stage.InputStageConfig.SetTeachingPosition(new TeachingPosition(target.Name, new Dictionary<string, double>(newAxes), newDesc) { ExtraInfo = new Dictionary<string, object>(newExtra) });
                stage.InputStageConfig.LoadAndBindAxes(Equipment.Instance.AxisManager);
                stage.TeachingPositions.Clear(); foreach (var tp in stage.InputStageConfig.TeachingPositions) stage.TeachingPositions.Add(tp);
                SetAxisDefinitionsToAxisListBox(); MessageBox.Show("저장 완료", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { MessageBox.Show("저장 오류: " + ex.Message); }
        }

        private void OnAxisSelected(object sender, int index)
        {
            // Axis 선택 처리 필요 시 구현
        }
    }
}