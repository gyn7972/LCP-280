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

        // 1. 상태 업데이트를 위한 타이머 추가
        private System.Windows.Forms.Timer _ioUpdateTimer;

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

                // 2. 타이머 초기화 및 시작
                StartIoUpdateTimer();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("InitializeUI error: " + ex.Message);
            }
        }

        // 3. 타이머 관련 메서드 추가
        private void StartIoUpdateTimer()
        {
            if (_ioUpdateTimer == null)
            {
                _ioUpdateTimer = new System.Windows.Forms.Timer();
                _ioUpdateTimer.Interval = 300; // 300ms 마다 갱신 (장비 상황에 맞게 조절)
                _ioUpdateTimer.Tick += OnIoUpdateTimerTick;
            }
            _ioUpdateTimer.Start();
        }

        private void OnIoUpdateTimerTick(object sender, EventArgs e)
        {
            try
            {
                // 컨트롤이 보이지 않거나 해제된 상태면 불필요한 통신 방지
                if (this.IsDisposed || !this.Visible) 
                    return;

                var eq = Equipment.Instance;
                var scan = eq?.DioScan;
                if (scan == null) return;

                bool isViewChanged = false; // UI를 새로고침해야 하는지 확인하는 플래그

                // Input 상태 갱신
                foreach (var item in _ioInputs)
                {
                    if (scan.TryGetInput(item.Module, item.Disp, out bool val))
                    {
                        if (item.Prop.State != val)
                        {
                            item.Prop.State = val; // 모델(Data)의 상태 변경
                            inputView?.SetStateByKey(item.Disp, val); // 기존 로직 유지 (혹시 맞을 경우를 대비)
                            isViewChanged = true;
                        }
                    }
                }

                // Output 상태 갱신
                foreach (var item in _ioOutputs)
                {
                    if (scan.TryGetOutput(item.Module, item.Disp, out bool val))
                    {
                        if (item.Prop.State != val)
                        {
                            item.Prop.State = val; // 모델(Data)의 상태 변경
                            outputView?.SetStateByKey(item.Disp, val); // UI 색상 변경 시도
                            isViewChanged = true;
                        }
                    }
                }

                // ★ 핵심: 상태값이 단 하나라도 바뀌었다면 강제로 전체 View를 다시 그리도록 지시합니다.
                if (isViewChanged)
                {
                    inputView?.Refresh();
                    outputView?.Refresh();
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

        // 4. 컨트롤이 해제될 때 타이머 정지 (메모리 누수 방지)
        protected override void OnHandleDestroyed(EventArgs e)
        {
            if (_ioUpdateTimer != null)
            {
                _ioUpdateTimer.Stop();
                _ioUpdateTimer.Dispose();
                _ioUpdateTimer = null;
            }
            base.OnHandleDestroyed(e);
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
