using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using QMC.Common.Unit; // BaseUnit 사용

namespace QMC.LCP_280.Process.Component
{
    public partial class ManualSequenceControl : UserControl
    {
        #region 내부 Step 표현
        private class SequenceStep
        {
            public BaseUnit Unit { get; private set; }
            public string Name { get; private set; }
            public Func<int> FuncInt { get; private set; }
            public Func<bool> FuncBool { get; private set; }

            public SequenceStep(BaseUnit unit, string name, Func<int> f) { Unit = unit; Name = name; FuncInt = f; }
            public SequenceStep(BaseUnit unit, string name, Func<bool> f) { Unit = unit; Name = name; FuncBool = f; }

            public int Invoke()
            {
                if (FuncInt != null) return FuncInt();
                if (FuncBool != null) return FuncBool() ? 1 : 0;
                return -1;
            }
            public override string ToString() { return Name; }
        }
        #endregion

        #region 필드
        private readonly List<BaseUnit> _units = new List<BaseUnit>();
        private readonly List<SequenceStep> _allSteps = new List<SequenceStep>();
        private readonly List<SequenceStep> _filteredSteps = new List<SequenceStep>();
        private readonly object _sync = new object();

        private int _currentIndex = 0; // 현재 실행 인덱스(Filtered 기준)
        private bool _isRunning = false;
        private CancellationTokenSource _cts;
        #endregion

        #region 이벤트
        public event EventHandler<EventArgs> StepStarted;
        public event EventHandler<SequenceCompletedEventArgs> StepCompleted;
        public event EventHandler<EventArgs> AllCompleted;
        #endregion

        #region 공개 속성
        [Browsable(false)]
        public bool IsRunning { get { lock (_sync) return _isRunning; } }
        [Browsable(false)]
        public int CurrentIndex { get { lock (_sync) return _currentIndex; } }
        [Browsable(false)]
        public int StepCount { get { lock (_sync) return _filteredSteps.Count; } }
        [Browsable(false)]
        public string CurrentStepName
        {
            get
            {
                lock (_sync)
                {
                    if (_currentIndex >= 0 && _currentIndex < _filteredSteps.Count)
                        return _filteredSteps[_currentIndex].Name;
                    return string.Empty;
                }
            }
        }
        #endregion

        public ManualSequenceControl()
        {
            InitializeComponent();
            WireUiEvents();
        }

        #region 초기 UI 이벤트 연결
        private void WireUiEvents()
        {
            if (_btnManual != null) _btnManual.Click += _btnManual_Click;
            if (_btnStop != null) _btnStop.Click += _btnStop_Click;
            if (_btnRecover != null) _btnRecover.Click += _btnRecover_Click;
            if (_btnBack != null) _btnBack.Click += _btnBack_Click;
            if (_cboSequence != null) _cboSequence.SelectedIndexChanged += _cboSequence_SelectedIndexChanged;
            if (_lstStep != null) _lstStep.DoubleClick += _lstStep_DoubleClick;
            UpdateButtons();
        }
        #endregion

        #region 외부 API (등록)
        public void ClearSequences()
        {
            EnsureNotRunning();
            lock (_sync)
            {
                _units.Clear();
                _allSteps.Clear();
                _filteredSteps.Clear();
                _currentIndex = 0;
            }
            RefreshUnitCombo();
            RefreshStepList();
        }

        public void AddSequence(Func<int> func, string name, BaseUnit unit = null)
        {
            if (func == null) throw new ArgumentNullException("func");
            if (string.IsNullOrWhiteSpace(name)) name = func.Method.Name;
            lock (_sync)
            {
                _allSteps.Add(new SequenceStep(unit, name, func));
                if (unit != null && !_units.Contains(unit)) _units.Add(unit);
            }
            RefreshUnitCombo();
            ApplyUnitFilter();
        }

        public void AddSequence(Func<bool> func, string name, BaseUnit unit = null)
        {
            if (func == null) throw new ArgumentNullException("func");
            if (string.IsNullOrWhiteSpace(name)) name = func.Method.Name;
            lock (_sync)
            {
                _allSteps.Add(new SequenceStep(unit, name, func));
                if (unit != null && !_units.Contains(unit)) _units.Add(unit);
            }
            RefreshUnitCombo();
            ApplyUnitFilter();
        }

        // Unit 내 메소드 등록. methodNames 없으면 규칙(Seq/Step 포함 & 파라미터 없음 & int/bool 반환)
        public void AddUnitMethods(BaseUnit unit, params string[] methodNames)
        {
            if (unit == null) throw new ArgumentNullException("unit");

            IEnumerable<MethodInfo> methods;
            var t = unit.GetType();
            if (methodNames != null && methodNames.Length > 0)
            {
                methods = methodNames
                    .Select(n => t.GetMethod(n, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                    .Where(m => m != null);
            }
            else
            {
                methods = t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(m => m.GetParameters().Length == 0 &&
                        (m.ReturnType == typeof(int) || m.ReturnType == typeof(bool)) &&
                        (m.Name.IndexOf("Seq", StringComparison.OrdinalIgnoreCase) >= 0 ||
                         m.Name.IndexOf("Step", StringComparison.OrdinalIgnoreCase) >= 0));
            }

            foreach (var mi in methods)
            {
                try
                {
                    if (mi.ReturnType == typeof(int))
                    {
                        var f = (Func<int>)Delegate.CreateDelegate(typeof(Func<int>), unit, mi);
                        AddSequence(f, unit.UnitName + "." + mi.Name, unit);
                    }
                    else if (mi.ReturnType == typeof(bool))
                    {
                        var f = (Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>), unit, mi);
                        AddSequence(f, unit.UnitName + "." + mi.Name, unit);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("[ManualSequence] AddUnitMethods Error: " + ex.Message);
                }
            }
        }
        #endregion

        #region 실행 제어
        public async Task<int> ExecuteCurrentAsync()
        {
            SequenceStep step;
            lock (_sync)
            {
                if (_isRunning) throw new InvalidOperationException("실행중");
                if (_currentIndex < 0 || _currentIndex >= _filteredSteps.Count) throw new InvalidOperationException("유효하지 않은 인덱스");
                step = _filteredSteps[_currentIndex];
                _isRunning = true;
                _cts = new CancellationTokenSource();
            }
            OnStepStarted();
            UpdateStatus("Run: " + step.Name);
            UpdateButtons();

            int result = -1;
            try
            {
                result = await Task.Run(() => step.Invoke(), _cts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                result = -999; // 취소 코드
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[ManualSequence] Execute Error: " + ex.Message);
                result = -1;
            }
            finally
            {
                bool allDone = false;
                lock (_sync)
                {
                    _isRunning = false;
                    if (result != -999) // 취소가 아니면 인덱스 진행
                        _currentIndex++;
                    if (_currentIndex >= _filteredSteps.Count)
                        allDone = true;
                }

                PostUI(() =>
                {
                    OnStepCompleted(step.Name, result);
                    if (allDone) OnAllCompleted();
                    RefreshStepList();
                    UpdateButtons();
                });
            }
            return result;
        }

        public async Task RunAllAsync()
        {
            while (true)
            {
                int idx;
                lock (_sync) idx = _currentIndex;
                if (idx >= StepCount) break;
                await ExecuteCurrentAsync().ConfigureAwait(false);
                lock (_sync)
                {
                    if (_isRunning) break; // 안전
                }
            }
        }

        public void Cancel()
        {
            lock (_sync)
            {
                try { _cts?.Cancel(); } catch { }
            }
        }

        public void ResetIndex()
        {
            EnsureNotRunning();
            lock (_sync) _currentIndex = 0;
            RefreshStepList();
        }

        public void MovePrevIndex()
        {
            EnsureNotRunning();
            lock (_sync)
            {
                if (_currentIndex > 0) _currentIndex--;
            }
            RefreshStepList();
        }
        #endregion

        #region UI 이벤트 구현
        private async void _btnManual_Click(object sender, EventArgs e)
        {
            if (IsRunning) return;
            try { await ExecuteCurrentAsync().ConfigureAwait(false); } catch (Exception ex) { ShowError(ex.Message); }
        }
        private void _btnStop_Click(object sender, EventArgs e)
        {
            if (!IsRunning) return;
            Cancel();
        }
        private void _btnRecover_Click(object sender, EventArgs e)
        {
            if (IsRunning) return;
            ResetIndex();
        }
        private void _btnBack_Click(object sender, EventArgs e)
        {
            if (IsRunning) return;
            MovePrevIndex();
        }
        private void _cboSequence_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyUnitFilter();
        }
        private void _lstStep_DoubleClick(object sender, EventArgs e)
        {
            if (IsRunning) return;
            int sel = _lstStep.SelectedIndex;
            if (sel >= 0 && sel < _lstStep.Items.Count)
            {
                lock (_sync) _currentIndex = sel;
                RefreshStepList();
            }
        }
        #endregion

        #region UI Helper
        private void RefreshUnitCombo()
        {
            PostUI(delegate
            {
                var selected = _cboSequence.SelectedItem as BaseUnit;
                _cboSequence.BeginUpdate();
                _cboSequence.Items.Clear();
                _cboSequence.Items.Add("<ALL>");
                foreach (var u in _units) _cboSequence.Items.Add(u);
                _cboSequence.DisplayMember = "UnitName"; // BaseUnit.UnitName
                _cboSequence.EndUpdate();
                if (selected != null && _units.Contains(selected))
                    _cboSequence.SelectedItem = selected;
                else
                    _cboSequence.SelectedIndex = 0;
            });
        }

        private void ApplyUnitFilter()
        {
            object sel = null;
            PostUI(() => sel = _cboSequence.SelectedItem);
            lock (_sync)
            {
                _filteredSteps.Clear();
                if (sel == null || (sel is string && (string)sel == "<ALL>"))
                {
                    _filteredSteps.AddRange(_allSteps);
                }
                else if (sel is BaseUnit)
                {
                    var unit = (BaseUnit)sel;
                    _filteredSteps.AddRange(_allSteps.Where(s => s.Unit == unit));
                }
                _currentIndex = 0; // 필터 변경 시 리셋
            }
            RefreshStepList();
        }

        private void RefreshStepList()
        {
            PostUI(delegate
            {
                _lstStep.BeginUpdate();
                _lstStep.Items.Clear();
                List<string> items;
                int cur;
                lock (_sync)
                {
                    cur = _currentIndex;
                    items = _filteredSteps.Select((s, i) => string.Format("{0}{1:00}. {2}", i == cur ? ">> " : "   ", i, s.Name)).ToList();
                }
                foreach (var it in items) _lstStep.Items.Add(it);
                if (cur >= 0 && cur < _lstStep.Items.Count) _lstStep.SelectedIndex = cur; else _lstStep.SelectedIndex = -1;
                _lstStep.EndUpdate();
                UpdateButtons();
            });
        }

        private void UpdateButtons()
        {
            PostUI(delegate
            {
                bool running = IsRunning;
                _btnManual.Enabled = !running && StepCount > 0 && CurrentIndex < StepCount;
                _btnStop.Enabled = running;
                _btnRecover.Enabled = !running;
                _btnBack.Enabled = !running && CurrentIndex > 0;
            });
        }

        private void UpdateStatus(string text)
        {
            Debug.WriteLine("[ManualSequence] " + text);
        }

        private void ShowError(string msg)
        {
            try { MessageBox.Show(this, msg, "ManualSequence", MessageBoxButtons.OK, MessageBoxIcon.Error); } catch { }
        }

        private void PostUI(Action a)
        {
            if (a == null) return;
            if (IsHandleCreated && InvokeRequired)
            {
                try { BeginInvoke(a); } catch { }
            }
            else a();
        }
        #endregion

        #region 내부 Helper & 이벤트 발생
        private void EnsureNotRunning()
        {
            lock (_sync)
            {
                if (_isRunning) throw new InvalidOperationException("실행 중에는 변경 불가");
            }
        }
        private void OnStepStarted() { try { StepStarted?.Invoke(this, EventArgs.Empty); } catch { } }
        private void OnStepCompleted(string name, int result) { try { StepCompleted?.Invoke(this, new SequenceCompletedEventArgs(name, result, CurrentIndex)); } catch { } }
        private void OnAllCompleted() { try { AllCompleted?.Invoke(this, EventArgs.Empty); } catch { } }
        #endregion
    }

    public class SequenceCompletedEventArgs : EventArgs
    {
        public string StepName { get; private set; }
        public int Result { get; private set; }
        public int NextIndex { get; private set; }
        public SequenceCompletedEventArgs(string stepName, int result, int nextIndex)
        {
            StepName = stepName; Result = result; NextIndex = nextIndex;
        }
    }
}
