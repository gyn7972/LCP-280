using QMC.Common;
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
    /// <summary>
    /// CassetteLoadingElevator Unit의 Config 폼
    /// Equipment와 연동하여 Config 및 Recipe 관리
    /// </summary>
    public partial class RotaryUnit_Config : Form
    {

        private const string _UNIT_NAME = "Rotary";
        private Equipment _Equipment => Equipment.Instance;
        private Rotary _Rotary { get; set; }
        private RotaryConfig _cfg;
        private readonly Size _designerSize;
        private bool _sizeMismatchWarned;

        
        private readonly List<IoRef> _ioInputs = new List<IoRef>();
        private readonly List<IoRef> _ioOutputs = new List<IoRef>();

        public RotaryUnit_Config()
        {
            InitializeComponent();
            InitializeUnit();
            this.SuspendLayout();
            _designerSize = this.Size;
            InitializeUI();
            this.ResumeLayout(true);

            // ★ 출력 항목 클릭 이벤트 연결 (토글)
            this.outputView.ItemClicked -= new System.EventHandler<string>(this.OnOutputItemClicked);
            this.outputView.ItemClicked += new System.EventHandler<string>(this.OnOutputItemClicked);

            Console.WriteLine($"✅ RotaryUnit_Config 생성자 완료");
        }

        private void InitializeUnit()
        {
            try
            {
                if (_Equipment.Units.TryGetValue(_UNIT_NAME, out var unit))
                {
                    _Rotary = unit as Rotary;
                    _cfg = _Rotary.RotaryConfig;
                }

                if (_Rotary == null)
                {
                    MessageBox.Show($"{_UNIT_NAME} Unit을 찾을 수 없습니다.\nEquipment에 Unit이 등록되어 있는지 확인하세요.",
                        "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                Console.WriteLine($"{_UNIT_NAME} Unit 연결 완료");
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
            }
            catch (Exception ex) { Console.WriteLine("InitializeUI error: " + ex.Message); }
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
                this.SuspendLayout();
                this.Size = new Size(width, height);
                this.Invalidate();
                this.Update();
            }
            finally
            {
                this.ResumeLayout(true);
            }

            Console.WriteLine($"📐 {nameof(InputCassetteLifterUnit_Config)}.SetPanelSize → {width}x{height}");
        }

        private void btnCurrentPos_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_Equipment.Units.TryGetValue(_UNIT_NAME, out var unit))
                {
                    MessageBox.Show("Unit을 찾을 수 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 현재 선택된 Teaching Position 인덱스 가져오기
                int selIndex = -1;
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

                if (selIndex < 0 || _cfg == null || _cfg.TeachingPositions == null || selIndex >= _cfg.TeachingPositions.Count)
                {
                    MessageBox.Show("선택된 Teaching Position이 없습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var tp = _cfg.TeachingPositions[selIndex];

                // 현재 위치 읽어서 AxisPositions 맵 갱신(표시용)
                var updatedPositions = new Dictionary<string, double>();
                foreach (var kv in tp.AxisPositions)
                {
                    string axisKey = kv.Key;
                    double fallback = kv.Value;

                    QMC.Common.Motions.MotionAxis axis = null;

                    // 1) TP 내부 바인딩 축
                    if (tp.Axes != null) 
                        tp.Axes.TryGetValue(axisKey, out axis);

                    // 2) Unit의 축 사전 키로 찾기
                    if (axis == null && _Rotary.Axes != null)
                    {
                        QMC.Common.Motions.MotionAxis directAxis;
                        if (_Rotary.Axes.TryGetValue(axisKey, out directAxis))
                            axis = directAxis;
                    }

                    // 3) 축 Name으로 매칭
                    if (axis == null && _Rotary.Axes != null)
                    {
                        foreach (var pair in _Rotary.Axes)
                        {
                            var a = pair.Value;
                            if (a != null && string.Equals(a.Name, axisKey, StringComparison.OrdinalIgnoreCase))
                            {
                                axis = a;
                                break;
                            }
                        }
                    }

                    // 위치 읽기
                    double pos = fallback;
                    if (axis != null)
                    {
                        try { pos = axis.GetPosition(); } catch { pos = fallback; }
                    }
                    updatedPositions[axisKey] = pos;
                }

                // 에디터에 표시 갱신
                var editorProperties = new PropertyCollection();
                editorProperties.Add(new TitleOnlyProperty($"Teaching Position: {tp.Name} (mm, Abs. Pos)"));
                editorProperties.Add(new StringProperty("Description", tp.Description ?? ""));

                foreach (var ap in updatedPositions)
                    editorProperties.Add(new DoubleProperty($"{ap.Key} Position (mm)", ap.Value));

                foreach (var extra in tp.ExtraInfo)
                    editorProperties.Add(new StringProperty($"Extra: {extra.Key}", extra.Value?.ToString() ?? ""));

                positionEditorView?.SetProperties(editorProperties);
            }
            catch (Exception ex)
            {
                MessageBox.Show("현재 위치 읽기 중 오류: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button_Test_Click(object sender, EventArgs e)
        {
            TestGyn testGyn = new TestGyn();
            testGyn.ShowDialog();
        }

        private void InitializeRadioButtonView() { try { rbTeachingMoveMode?.SetOptions(true, "Fine", "Coarse"); } catch (Exception ex) { Console.WriteLine("RadioButton 오류: " + ex.Message); } }

        private void SetAxisDefinitionsToAxisListBox()
        {
            try
            {
                var eq = Equipment.Instance;
                if (eq.Units.TryGetValue(_UNIT_NAME, out var unit))
                {
                    var rotary = unit as Rotary;
                    if (rotary?.TeachingPositions != null && rotary.TeachingPositions.Count > 0)
                        positionItemView?.SetItems(rotary.TeachingPositions.Select(t => t.Name).ToArray());
                    else positionItemView?.SetItems();
                }
            }
            catch (Exception ex) { Console.WriteLine("SetAxisDefinitions 오류: " + ex.Message); }
        }

        private void SetupPositionItemSelectionEvent()
        { if (positionItemView == null) return; positionItemView.ItemSelected -= OnPositionItemSelected; positionItemView.ItemSelected += OnPositionItemSelected; }

        private void OnPositionItemSelected(object sender, int index)
        {
            try
            {
                ShowTeachingPositionInPropertyCollectionView(index);
                if (!_Equipment.Units.TryGetValue(_UNIT_NAME, out var unit)) return; var rotary = unit as Rotary; if (rotary == null) return;
                if (index >= 0 && index < rotary.RotaryConfig.TeachingPositions.Count)
                {
                    var tp = rotary.RotaryConfig.TeachingPositions[index];
                    jogControl?.SetTeachingAxisList(tp.AxisPositions.Keys);
                }
            }
            catch (Exception ex) { Console.WriteLine("OnPositionItemSelected 오류: " + ex.Message); }
        }

        private void ShowTeachingPositionInPropertyCollectionView(int idx)
        {
            var eq = Equipment.Instance; if (!eq.Units.TryGetValue(_UNIT_NAME, out var unit)) return; var rotary = unit as Rotary; var config = rotary?.RotaryConfig; if (config?.TeachingPositions == null) return;
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
                if (eq.Units.TryGetValue(_UNIT_NAME, out var unit) && unit is Rotary rotary && rotary.RotaryConfig != null)
                {
                    var cfg = rotary.RotaryConfig; var t = cfg.GetType();
                    var piIn = t.GetProperty("HardInputs"); if (piIn != null) hardInputs = ((System.Collections.IEnumerable)piIn.GetValue(cfg))?.Cast<dynamic>().ToList() ?? new List<dynamic>();
                    var piOut = t.GetProperty("HardOutputs"); if (piOut != null) hardOutputs = ((System.Collections.IEnumerable)piOut.GetValue(cfg))?.Cast<dynamic>().ToList() ?? new List<dynamic>();
                }
                Func<string, Tuple<string, string>> resolveIn = disp => { if (unitIO?.Modules == null) return Tuple.Create<string, string>(null, disp); foreach (var m in unitIO.Modules) { if (m?.Inputs == null) continue; foreach (var ch in m.Inputs) if (string.Equals(ch.DisplayNo, disp, StringComparison.OrdinalIgnoreCase)) return Tuple.Create(m.ModuleName, ch.DisplayNo); } return Tuple.Create<string, string>(null, disp); };
                Func<string, Tuple<string, string>> resolveOut = disp => { if (unitIO?.Modules == null) return Tuple.Create<string, string>(null, disp); foreach (var m in unitIO.Modules) { if (m?.Outputs == null) continue; foreach (var ch in m.Outputs) if (string.Equals(ch.DisplayNo, disp, StringComparison.OrdinalIgnoreCase)) return Tuple.Create(m.ModuleName, ch.DisplayNo); } return Tuple.Create<string, string>(null, disp); };
                if (hardInputs.Count > 0)
                {
                    var pcIn = new PropertyCollection { ShowNoColumn = true, IsInputParameter = false }; pcIn.Add(new TitleOnlyProperty("No", "Name", "State"));
                    foreach (var it in hardInputs) { string disp = it.Disp; string name = it.Name; int no = it.No; var map = resolveIn(disp); bool cur = false; if (map.Item1 != null) scan.TryGetInput(map.Item1, map.Item2, out cur); var ps = new PropertyState(no.ToString(), $"{disp} {name}", cur); pcIn.Add(ps); _ioInputs.Add(new IoRef { Module = map.Item1, Disp = map.Item2, Prop = ps }); }
                    inputView.SetProperties(pcIn);
                }
                else inputView.SetProperties(new PropertyCollection());
                if (hardOutputs.Count > 0)
                {
                    var pcOut = new PropertyCollection { ShowNoColumn = true, IsInputParameter = false }; pcOut.Add(new TitleOnlyProperty("No", "Name", "State"));
                    foreach (var it in hardOutputs) { string disp = it.Disp; string name = it.Name; int no = it.No; var map = resolveOut(disp); bool cur = false; var ps = new PropertyState(no.ToString(), $"{disp} {name}", cur); pcOut.Add(ps); _ioOutputs.Add(new IoRef { Module = map.Item1, Disp = map.Item2, Prop = ps }); }
                    outputView.SetProperties(pcOut);
                }
                else outputView.SetProperties(new PropertyCollection());
                scan.InputChanged -= OnDioInputChanged; scan.InputChanged += OnDioInputChanged;
            }
            catch (Exception ex) { Console.WriteLine("InitializeDigitalIO 오류: " + ex.Message); }
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
            }
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
                try { outputView.SetStateByKey(key, after); if (!string.Equals(key, originalDisp, StringComparison.OrdinalIgnoreCase)) outputView.SetStateByKey(originalDisp, after); var norm = NormalizeXYKey(key); if (!string.Equals(norm, key, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(norm) && !string.Equals(norm, originalDisp, StringComparison.OrdinalIgnoreCase)) outputView.SetStateByKey(norm, after); } catch { }
                MessageBox.Show($"{originalDisp}: {before}->{after}");
            }
            catch (Exception ex) { MessageBox.Show("Output 토글 오류: " + ex.Message); }
        }

        private void btnMovePosition_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_Equipment.Units.TryGetValue(_UNIT_NAME, out var unit)) return; var rotary = unit as Rotary; if (rotary == null) return;
                int selIndex = -1; try { var pi = positionItemView?.GetType().GetProperty("SelectedIndex"); if (pi != null) selIndex = (int)pi.GetValue(positionItemView, null); } catch { selIndex = -1; }
                if (selIndex < 0 || rotary.RotaryConfig.TeachingPositions == null || selIndex >= rotary.RotaryConfig.TeachingPositions.Count) return;
                var tp = rotary.RotaryConfig.TeachingPositions[selIndex]; bool isFine = true; try { var si = rbTeachingMoveMode?.GetType().GetProperty("SelectedIndex"); if (si != null) isFine = ((int)si.GetValue(rbTeachingMoveMode, null)) == 0; } catch { }
                double defFine = 5, defCoarse = 20, defAcc = 10, defDec = 10, defJerk = 50;
                foreach (var kv in tp.AxisPositions)
                { MotionAxis axis = null; if (tp.Axes != null) tp.Axes.TryGetValue(kv.Key, out axis); if (axis == null && rotary.Axes.TryGetValue(kv.Key, out var direct)) axis = direct; if (axis == null) foreach (var ap in rotary.Axes) if (ap.Value != null && string.Equals(ap.Value.Name, kv.Key, StringComparison.OrdinalIgnoreCase)) { axis = ap.Value; break; } if (axis == null) continue; double vel = isFine ? (axis.Config?.JogFineVelocity > 0 ? axis.Config.JogFineVelocity : defFine) : (axis.Config?.JogCoarseVelocity > 0 ? axis.Config.JogCoarseVelocity : defCoarse); double acc = axis.Config?.JogAcc > 0 ? axis.Config.JogAcc : defAcc; double dec = axis.Config?.JogDec > 0 ? axis.Config.JogDec : defDec; double jerk = axis.Config != null ? (axis.Config.AccJerkPercent + axis.Config.DecJerkPercent) / 2.0 : defJerk; axis.MoveAbs(kv.Value, vel, acc, dec, jerk); }
                foreach (var kv in tp.AxisPositions)
                { MotionAxis axis = null; if (tp.Axes != null) tp.Axes.TryGetValue(kv.Key, out axis); if (axis == null && rotary.Axes.TryGetValue(kv.Key, out var direct)) axis = direct; if (axis == null) continue; axis.WaitMoveDone(-1); }
            }
            catch (Exception ex) { MessageBox.Show("Move 오류: " + ex.Message); }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_Equipment.Units.TryGetValue(_UNIT_NAME, out var unit)) return; var rotary = unit as Rotary; if (rotary == null) return;
                int selIndex = -1; try { var pi = positionItemView?.GetType().GetProperty("SelectedIndex"); if (pi != null) selIndex = (int)pi.GetValue(positionItemView, null); } catch { selIndex = -1; }
                if (selIndex < 0 || rotary.TeachingPositions == null || selIndex >= rotary.TeachingPositions.Count) return;
                positionEditorView?.Apply(); var props = positionEditorView?.GetCurrentProperties(); if (props == null) return;
                var target = rotary.TeachingPositions[selIndex]; var newAxes = new Dictionary<string, double>(target.AxisPositions ?? new Dictionary<string, double>()); var newExtra = target.ExtraInfo != null ? new Dictionary<string, object>(target.ExtraInfo) : new Dictionary<string, object>(); string newDesc = target.Description;
                foreach (var p in props)
                {
                    if (p is StringProperty && string.Equals(p.Title, "Description", StringComparison.OrdinalIgnoreCase)) { newDesc = ((StringProperty)p).Value ?? ""; continue; }
                    if (p is DoubleProperty && p.Title.EndsWith(" Position (mm)", StringComparison.OrdinalIgnoreCase)) { var axisKey = p.Title.Substring(0, p.Title.IndexOf(" Position (mm)")); newAxes[axisKey] = ((DoubleProperty)p).Value; continue; }
                    if (p is StringProperty && p.Title.StartsWith("Extra:", StringComparison.OrdinalIgnoreCase)) { var key = p.Title.Substring("Extra:".Length).Trim(); newExtra[key] = ((StringProperty)p).Value; continue; }
                }
                target.Description = newDesc; target.AxisPositions = newAxes; target.ExtraInfo = newExtra;
                rotary.RotaryConfig.SetTeachingPosition(new TeachingPosition(target.Name, new Dictionary<string, double>(newAxes), newDesc) { ExtraInfo = new Dictionary<string, object>(newExtra) });
                rotary.RotaryConfig.LoadAndBindAxes(Equipment.Instance.AxisManager);
                rotary.TeachingPositions.Clear(); foreach (var tp in rotary.RotaryConfig.TeachingPositions) rotary.TeachingPositions.Add(tp);
                SetAxisDefinitionsToAxisListBox(); MessageBox.Show("저장 완료", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { MessageBox.Show("저장 오류: " + ex.Message); }
        }

        private void OnAxisSelected(object sender, int index) { }
    }
}