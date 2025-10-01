using QMC.Common;
using QMC.Common.IOUtil;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Component;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using PropertyCollection = QMC.Common.PropertyCollection;

namespace QMC.LCP_280.Process.Unit.FormConfig
{
    public partial class DigitalIOControl : UserControl
    {
        private BaseUnit _unit;
        private BaseConfig _config;

        private HardInputDef[] _hardInputs;
        private HardOutputDef[] _hardOutputs;

        private readonly List<IoRef> _ioInputs = new List<IoRef>();
        private readonly List<IoRef> _ioOutputs = new List<IoRef>();

        public DigitalIOControl()
        {
            InitializeComponent();
        }

        public void SetUnitData(BaseUnit unit, BaseConfig config, HardInputDef[] hardInputs, HardOutputDef[] hardOutputs)
        {
            _unit = unit;
            _config = config;

            _hardInputs = hardInputs;
            _hardOutputs = hardOutputs;

            InitializeUI();
        }

        private void InitializeUI()
        {
            try
            {
                BindingList_DigitalIO();
                WriteEvents_DigitalIO();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("InitializeUI error: " + ex.Message);
            }
        }


        private void WriteEvents_DigitalIO()
        {
            if (outputView != null)
            {
                outputView.ItemClicked -= new EventHandler<string>(OnOutputItemClicked);
                outputView.ItemClicked += new EventHandler<string>(OnOutputItemClicked);
            }
        } 

        #region Digital IO

        private void BindingList_DigitalIO()
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

                HardInputDef[] hardInputs = _hardInputs ?? Array.Empty<HardInputDef>();
                HardOutputDef[] hardOutputs = _hardOutputs ?? Array.Empty<HardOutputDef>();

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

                var ask = new MessageBoxYesNo();
                if (ask.ShowDialog("Output Toggle", "[" + module + ":" + originalDisp + "] 현재 상태 = " + before + "\r\n변경하시겠습니까?") != DialogResult.Yes)
                {
                    return;
                }

                int rc = scan.WriteOutput(module, originalDisp, !before);
                if (rc != 0)
                {
                    var mb = new MessageBoxOk();
                    mb.ShowDialog("Error!", "WriteOutput 실패 (rc=" + rc + ")");

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

                var mb1 = new MessageBoxOk();
                mb1.ShowDialog("Info!", originalDisp + ": " + before + " -> " + after);
            }
            catch (Exception ex)
            {
                var mb = new MessageBoxOk();
                mb.ShowDialog("Error!", "Output 토글 처리 중 오류: " + ex.Message);
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

    }
}
