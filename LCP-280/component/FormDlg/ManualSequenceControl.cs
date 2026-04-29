using QMC.Common.Alarm;
using QMC.Common.Unit;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace QMC.LCP_280.Process.Component.FormDlg
{
    public partial class ManualSequenceControl : UserControl
    {
        public sealed class ManualStep
        {
            public string DisplayName { get; }
            public string UnitKey { get; }
            public string MethodName { get; }
            public Func<int> IndexProvider { get; }
            public Func<object, int, Task<int>> Executor { get; }
            public Func<object, int, CancellationToken, Task<int>> CancellableExecutor { get; }

            public ManualStep(string displayName, string unitKey, string methodName, Func<int> indexProvider = null)
            {
                DisplayName = displayName;
                UnitKey = unitKey;
                MethodName = methodName;
                IndexProvider = indexProvider;
            }

            // 기존 델리게이트 방식
            public ManualStep(string displayName, string unitKey, Func<object, int, Task<int>> executor, Func<int> indexProvider = null)
            {
                DisplayName = displayName;
                UnitKey = unitKey;
                MethodName = "Delegate";
                Executor = executor;
                CancellableExecutor = (unit, index, token) => executor(unit, index);
                IndexProvider = indexProvider;
            }

            // 취소 토큰 지원 델리게이트 방식
            public ManualStep(string displayName, string unitKey, Func<object, int, CancellationToken, Task<int>> executor, Func<int> indexProvider = null)
            {
                DisplayName = displayName;
                UnitKey = unitKey;
                MethodName = "Delegate";
                CancellableExecutor = executor;
                Executor = (unit, index) => executor(unit, index, CancellationToken.None);
                IndexProvider = indexProvider;
            }

            // sync / no index
            public static ManualStep Create<TUnit>(string displayName, string unitKey, Func<TUnit, int> executor, Func<int> indexProvider = null)
            {
                return new ManualStep(
                    displayName,
                    unitKey,
                    (unit, index, token) => Task.Run(() => executor((TUnit)unit), token),
                    indexProvider);
            }

            // sync / with index
            public static ManualStep Create<TUnit>(string displayName, string unitKey, Func<TUnit, int, int> executor, Func<int> indexProvider)
            {
                return new ManualStep(
                    displayName,
                    unitKey,
                    (unit, index, token) => Task.Run(() => executor((TUnit)unit, index), token),
                    indexProvider);
            }

            // async / no index
            public static ManualStep Create<TUnit>(string displayName, string unitKey, Func<TUnit, Task<int>> executor, Func<int> indexProvider = null)
            {
                return new ManualStep(
                    displayName,
                    unitKey,
                    (unit, index, token) => executor((TUnit)unit),
                    indexProvider);
            }

            // async / with index
            public static ManualStep Create<TUnit>(string displayName, string unitKey, Func<TUnit, int, Task<int>> executor, Func<int> indexProvider)
            {
                return new ManualStep(
                    displayName,
                    unitKey,
                    (unit, index, token) => executor((TUnit)unit, index),
                    indexProvider);
            }

            // async / no index / cancellation
            public static ManualStep Create<TUnit>(string displayName, string unitKey, Func<TUnit, CancellationToken, Task<int>> executor, Func<int> indexProvider = null)
            {
                return new ManualStep(
                    displayName,
                    unitKey,
                    (unit, index, token) => executor((TUnit)unit, token),
                    indexProvider);
            }

            // async / with index / cancellation
            public static ManualStep Create<TUnit>(string displayName, string unitKey, Func<TUnit, int, CancellationToken, Task<int>> executor, Func<int> indexProvider)
            {
                return new ManualStep(
                    displayName,
                    unitKey,
                    (unit, index, token) => executor((TUnit)unit, index, token),
                    indexProvider);
            }
        }

        private CancellationTokenSource _executionCts;
        private BaseUnit _executingUnit;
        private readonly List<BaseUnit> _statusSyncUnits = new List<BaseUnit>();
        public BaseUnit StatusSyncMasterUnit { get; set; }

        private readonly List<string> _steps = new List<string>(); // legacy
        private readonly List<ManualStep> _manualSteps = new List<ManualStep>(); // multi-unit
        private readonly Dictionary<string, object> _units = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        private int _selectedIndex = -1;
        private int _runningIndex = -1;

        public object ParentUnit { get; set; } // legacy
        public Func<int> IndexProvider { get; set; } = () => 0; // legacy

        private readonly Timer _playTimer = new Timer();
        private bool _isPlaying;
        private bool _isExecuting;

        private Form _runningDialog;
        private Label _runningLabel;
        private ProgressBar _runningProgress;

        public ManualSequenceControl()
        {
            InitializeComponent();
            InitializeListBoxStyle();
            InitializeControlStyle();

            // 필요 시 UI에서 바로 보이도록
            _btnNext.Visible = true;
            btnPlay.Visible = true;

            // 알람 발생 시 수동 시퀀스 강제 정지
            AlarmManager.Instance.AlarmAdded += OnAlarmAdded;

            _playTimer.Interval = 500;
            _playTimer.Tick += async (s, e) =>
            {
                if (_isExecuting)
                {
                    return;
                }

                // 알람 활성 상태면 자동 진행 중지
                if (AlarmManager.Instance.IsAlarm)
                {
                    RequestStop("알람 활성 상태로 자동 실행이 중지되었습니다.");
                    return;
                }

                int count = GetStepCount();
                if (count == 0)
                {
                    return;
                }

                _selectedIndex = (_selectedIndex + 1) % count;
                _lstSteps.SelectedIndex = _selectedIndex;
                await ExecuteCurrentStepAsync();
            };
        }
        private void OnAlarmAdded(AlarmInfo alarm)
        {
            if (alarm == null || IsDisposed)
            {
                return;
            }

            if (InvokeRequired)
            {
                try
                {
                    BeginInvoke(new Action<AlarmInfo>(OnAlarmAdded), alarm);
                }
                catch
                {
                }

                return;
            }

            RequestStop($"알람 발생으로 정지 요청됨... [{alarm.Source}] {alarm.Title}");
        }

        private void RequestStop(string message)
        {
            _isPlaying = false;
            _playTimer.Stop();
            _runningIndex = -1;
            _lstSteps.Invalidate();

            if (!_isExecuting)
            {
                CloseRunningDialog();
                return;
            }

            try
            {
                _executionCts?.Cancel();
            }
            catch
            {
            }

            if (_executingUnit != null)
            {
                _executingUnit.RunUnitStatus = BaseUnit.UnitStatus.Stopping;
                TryStopAxes(_executingUnit);

                if (ReferenceEquals(_executingUnit, StatusSyncMasterUnit))
                {
                    foreach (var syncUnit in _statusSyncUnits)
                    {
                        if (syncUnit == null || ReferenceEquals(syncUnit, _executingUnit))
                        {
                            continue;
                        }

                        syncUnit.RunUnitStatus = BaseUnit.UnitStatus.Stopping;
                        TryStopAxes(syncUnit);
                    }
                }
            }

            if (_runningLabel != null && !string.IsNullOrWhiteSpace(message))
            {
                _runningLabel.Text = message;
            }
        }
        public void SetStatusSyncUnits(BaseUnit masterUnit, params BaseUnit[] syncUnits)
        {
            StatusSyncMasterUnit = masterUnit;
            _statusSyncUnits.Clear();

            if (syncUnits == null)
            {
                return;
            }

            foreach (var unit in syncUnits)
            {
                if (unit == null)
                {
                    continue;
                }

                if (!_statusSyncUnits.Contains(unit))
                {
                    _statusSyncUnits.Add(unit);
                }
            }
        }

        private void InitializeControlStyle()
        {
            BackColor = Color.FromArgb(246, 246, 248);
            tableLayoutPanel1.BackColor = BackColor;
            tableLayoutPanel2.BackColor = BackColor;
            tableLayoutPanel1.Padding = new Padding(10, 8, 10, 10);

            _lstSteps.Font = new Font("Segoe UI", 11.5F, FontStyle.Bold);
            _lstSteps.ForeColor = Color.FromArgb(28, 28, 30);
            _lstSteps.BackColor = Color.FromArgb(250, 250, 252);

            StyleActionButton(btnRun, Color.FromArgb(0, 122, 255), Color.White);
            StyleActionButton(_btnNext, Color.FromArgb(10, 132, 255), Color.White);
            StyleActionButton(btnPlay, Color.FromArgb(52, 199, 89), Color.White);
            StyleActionButton(btnStop, Color.FromArgb(255, 69, 58), Color.White);

            var dbProp = typeof(Control).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            if (dbProp != null)
            {
                dbProp.SetValue(_lstSteps, true, null);
            }
        }
        private static void StyleActionButton(QMC.Common.IndividualMenuButton button, Color backColor, Color foreColor)
        {
            button.BackColor = backColor;
            button.CustomBackColor = backColor;
            button.ForeColor = foreColor;
            button.CustomForeColor = foreColor;

            var font = new Font("Segoe UI", 11F, FontStyle.Bold);
            button.Font = font;
            button.CustomFont = font;

            button.Margin = new Padding(6);
        }


        // ---------- New API ----------
        public void RegisterUnit(string unitKey, object unit)
        {
            if (string.IsNullOrWhiteSpace(unitKey) || unit == null)
            {
                return;
            }

            _units[unitKey] = unit;
        }

        public void SetSteps(IEnumerable<ManualStep> steps)
        {
            _manualSteps.Clear();
            _steps.Clear();
            _lstSteps.Items.Clear();

            if (steps != null)
            {
                foreach (var step in steps)
                {
                    if (step == null || string.IsNullOrWhiteSpace(step.DisplayName))
                    {
                        continue;
                    }

                    _manualSteps.Add(step);
                    _lstSteps.Items.Add(step.DisplayName);
                }
            }

            _selectedIndex = _manualSteps.Count > 0 ? 0 : -1;
            _lstSteps.SelectedIndex = _selectedIndex;
            _lstSteps.Invalidate();
        }

        // ---------- Legacy API ----------
        public void BindUnit(object parentUnit, IEnumerable<string> stepMethodNames)
        {
            ParentUnit = parentUnit;
            SetSteps(stepMethodNames);
        }

        public void SetSteps(IEnumerable<string> steps)
        {
            _manualSteps.Clear();
            _steps.Clear();
            _lstSteps.Items.Clear();

            if (steps != null)
            {
                foreach (var step in steps)
                {
                    if (!string.IsNullOrWhiteSpace(step))
                    {
                        _steps.Add(step);
                        _lstSteps.Items.Add(step);
                    }
                }
            }

            _selectedIndex = _steps.Count > 0 ? 0 : -1;
            _lstSteps.SelectedIndex = _selectedIndex;
            _lstSteps.Invalidate();
        }

        private int GetStepCount()
        {
            return _manualSteps.Count > 0 ? _manualSteps.Count : _steps.Count;
        }

        private async Task<int> ExecuteCurrentStepAsync()
        {
            if (_isExecuting)
            {
                return -1;
            }

            // 시작 전 알람 활성 상태 차단
            if (AlarmManager.Instance.IsAlarm)
            {
                RequestStop("알람 활성 상태에서는 수동 시퀀스를 실행할 수 없습니다.");
                return -1;
            }

            if (_selectedIndex < 0 || _selectedIndex >= GetStepCount())
            {
                return -1;
            }

            object unit;
            string methodName;
            string unitKey;
            Func<int> idxProvider;
            Func<object, int, CancellationToken, Task<int>> cancellableExecutor = null;

            if (_manualSteps.Count > 0)
            {
                var ms = _manualSteps[_selectedIndex];
                if (!_units.TryGetValue(ms.UnitKey, out unit) || unit == null)
                {
                    return -1;
                }

                methodName = string.IsNullOrWhiteSpace(ms.MethodName) ? ms.DisplayName : ms.MethodName;
                unitKey = ms.UnitKey;
                idxProvider = ms.IndexProvider ?? IndexProvider;
                cancellableExecutor = ms.CancellableExecutor;
            }
            else
            {
                if (ParentUnit == null)
                {
                    return -1;
                }

                unit = ParentUnit;
                methodName = _steps[_selectedIndex];
                unitKey = ParentUnit.GetType().Name;
                idxProvider = IndexProvider;
            }

            _executionCts?.Dispose();
            _executionCts = new CancellationTokenSource();
            CancellationToken token = _executionCts.Token;

            _isExecuting = true;
            SetButtonsEnabled(false);
            ShowRunningDialog(unitKey, methodName);

            var baseUnit = unit as BaseUnit;
            _executingUnit = baseUnit;
            bool manualStatusApplied = false;
            var syncedUnitsApplied = new List<BaseUnit>();
            try
            {
                if (baseUnit != null)
                {
                    baseUnit.CalcelToken = _executionCts;
                    baseUnit.RunUnitStatus = BaseUnit.UnitStatus.ManualRunning;
                    manualStatusApplied = true;
                    if (ReferenceEquals(baseUnit, StatusSyncMasterUnit))
                    {
                        foreach (var syncUnit in _statusSyncUnits)
                        {
                            if (syncUnit == null || ReferenceEquals(syncUnit, baseUnit))
                            {
                                continue;
                            }

                            syncUnit.CalcelToken = _executionCts;
                            syncUnit.RunUnitStatus = BaseUnit.UnitStatus.ManualRunning;
                            syncedUnitsApplied.Add(syncUnit);
                        }
                    }

                }

                int rc;
                int index = idxProvider != null ? idxProvider() : 0;
                if (cancellableExecutor != null)
                {
                    rc = await cancellableExecutor(unit, index, token);
                }
                else
                {
                    var method = unit.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public);
                    if (method == null)
                    {
                        return -1;
                    }

                    if (!TryBuildMethodArgs(method.GetParameters(), index, token, out var args))
                    {
                        return -1;
                    }

                    rc = await InvokeMethodAsync(unit, method, args, token);
                }

                if (rc == 0)
                {
                    _runningIndex = _selectedIndex;
                    _lstSteps.Invalidate();
                }

                return rc;
            }
            catch (OperationCanceledException)
            {
                return -2;
            }
            catch
            {
                return -1;
            }
            finally
            {
                if (manualStatusApplied && baseUnit != null)
                {
                    baseUnit.RunUnitStatus = BaseUnit.UnitStatus.Stopped;
                    baseUnit.CalcelToken = null;
                }

                foreach (var syncUnit in syncedUnitsApplied)
                {
                    syncUnit.RunUnitStatus = BaseUnit.UnitStatus.Stopped;
                    syncUnit.CalcelToken = null;
                }

                _executingUnit = null;
                _isExecuting = false;
                SetButtonsEnabled(true);

                if (!_isPlaying)
                {
                    CloseRunningDialog();
                }
            }
        }
        private static bool TryBuildMethodArgs(ParameterInfo[] ps, int index, CancellationToken token, out object[] args)
        {
            args = null;

            if (ps.Length == 0)
            {
                args = new object[0];
                return true;
            }

            if (ps.Length == 1)
            {
                if (ps[0].ParameterType == typeof(bool))
                {
                    args = new object[] { false };
                    return true;
                }

                if (ps[0].ParameterType == typeof(int))
                {
                    args = new object[] { index };
                    return true;
                }

                if (ps[0].ParameterType == typeof(CancellationToken))
                {
                    args = new object[] { token };
                    return true;
                }
            }

            if (ps.Length == 2)
            {
                if (ps[0].ParameterType == typeof(int) && ps[1].ParameterType == typeof(bool))
                {
                    args = new object[] { index, false };
                    return true;
                }

                if (ps[0].ParameterType == typeof(int) && ps[1].ParameterType == typeof(CancellationToken))
                {
                    args = new object[] { index, token };
                    return true;
                }

                if (ps[0].ParameterType == typeof(bool) && ps[1].ParameterType == typeof(CancellationToken))
                {
                    args = new object[] { false, token };
                    return true;
                }
            }

            return false;
        }

        private static async Task<int> InvokeMethodAsync(object unit, MethodInfo method, object[] args, CancellationToken token)
        {
            if (typeof(Task).IsAssignableFrom(method.ReturnType))
            {
                var result = method.Invoke(unit, args);

                if (result is Task<int> taskInt)
                {
                    return await taskInt;
                }

                if (result is Task task)
                {
                    await task;
                    return 0;
                }

                return 0;
            }

            return await Task.Run(() =>
            {
                token.ThrowIfCancellationRequested();

                var result = method.Invoke(unit, args);

                if (result is int n)
                {
                    return n;
                }

                return 0;
            }, token);
        }
        private static void TryStopAxes(BaseUnit unit)
        {
            if (unit == null)
            {
                return;
            }

            foreach (var axis in unit.Axes.Values)
            {
                try
                {
                    axis?.Stop();
                }
                catch
                {
                }
            }
        }

        private void ShowRunningDialog(string unitKey, string methodName)
        {
            if (_runningDialog == null || _runningDialog.IsDisposed)
            {
                _runningDialog = new Form
                {
                    Text = "Manual Sequence Running",
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false,
                    MinimizeBox = false,
                    ShowInTaskbar = false,
                    StartPosition = FormStartPosition.CenterParent,
                    ClientSize = new Size(420, 112),
                    BackColor = Color.FromArgb(246, 246, 248)
                };

                _runningLabel = new Label
                {
                    Dock = DockStyle.Top,
                    Height = 58,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(28, 28, 30)
                };

                _runningProgress = new ProgressBar
                {
                    Dock = DockStyle.Bottom,
                    Height = 20,
                    Style = ProgressBarStyle.Marquee,
                    MarqueeAnimationSpeed = 30
                };

                _runningDialog.Controls.Add(_runningLabel);
                _runningDialog.Controls.Add(_runningProgress);
            }

            _runningLabel.Text = $"실행 중: [{unitKey}] {methodName}";

            if (!_runningDialog.Visible)
            {
                var owner = FindForm();
                if (owner != null)
                {
                    _runningDialog.Show(owner);
                }
                else
                {
                    _runningDialog.Show();
                }
            }
        }

        private void CloseRunningDialog()
        {
            if (_runningDialog != null && !_runningDialog.IsDisposed && _runningDialog.Visible)
            {
                _runningDialog.Hide();
            }
        }

        private void SetButtonsEnabled(bool enabled)
        {
            btnRun.Enabled = enabled;
            _btnNext.Enabled = enabled;
            btnPlay.Enabled = enabled;
            btnStop.Enabled = true;
        }

        private async void _btnRun_Click(object sender, EventArgs e)
        {
            if (GetStepCount() == 0)
            {
                return;
            }

            if (_lstSteps.SelectedIndex < 0)
            {
                _lstSteps.SelectedIndex = 0;
            }

            _selectedIndex = _lstSteps.SelectedIndex;
            await ExecuteCurrentStepAsync();
        }

        private async void _btnNext_Click(object sender, EventArgs e)
        {
            int count = GetStepCount();
            if (count == 0)
            {
                return;
            }

            _selectedIndex = (_selectedIndex + 1) % count;
            _lstSteps.SelectedIndex = _selectedIndex;
            await ExecuteCurrentStepAsync();
        }

        private void _btnPlay_Click(object sender, EventArgs e)
        {
            _isPlaying = !_isPlaying;
            if (_isPlaying)
            {
                _playTimer.Start();
            }
            else
            {
                _playTimer.Stop();
                if (!_isExecuting)
                {
                    CloseRunningDialog();
                }
            }
        }

        private void _btnStop_Click(object sender, EventArgs e)
        {
            RequestStop("정지 요청됨... 취소/축 정지 진행 중");
        }

        private void InitializeListBoxStyle()
        {
            // 행 높이도 같이 키워 가독성 확보
            _lstSteps.ItemHeight = 35;
            _lstSteps.DrawMode = DrawMode.OwnerDrawFixed;
            _lstSteps.BorderStyle = BorderStyle.None;
            _lstSteps.BackColor = Color.FromArgb(250, 250, 252);

            _lstSteps.DrawItem -= _lstSteps_DrawItem;
            _lstSteps.DrawItem += _lstSteps_DrawItem;

            _lstSteps.SelectedIndexChanged -= _lstSteps_SelectedIndexChanged;
            _lstSteps.SelectedIndexChanged += _lstSteps_SelectedIndexChanged;
        }

        private void _lstSteps_SelectedIndexChanged(object sender, EventArgs e)
        {
            _selectedIndex = _lstSteps.SelectedIndex;
            _lstSteps.Invalidate();
        }

        private void _lstSteps_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= _lstSteps.Items.Count)
            {
                return;
            }

            bool isSelected = e.Index == _selectedIndex;
            bool isRunning = e.Index == _runningIndex;

            var itemBounds = new Rectangle(e.Bounds.X + 6, e.Bounds.Y + 4, e.Bounds.Width - 12, e.Bounds.Height - 8);

            // 선택색 최우선
            Color fillColor;
            Color borderColor;
            Color accentColor;

            if (isSelected)
            {
                fillColor = Color.FromArgb(0, 122, 255);      // 선택 소켓 강조색(최우선)
                borderColor = Color.FromArgb(0, 95, 204);
                accentColor = Color.FromArgb(255, 255, 255);
            }
            else if (isRunning)
            {
                fillColor = Color.FromArgb(230, 242, 255);
                borderColor = Color.FromArgb(0, 122, 255);
                accentColor = Color.FromArgb(0, 122, 255);
            }
            else
            {
                fillColor = Color.White;
                borderColor = Color.FromArgb(226, 226, 230);
                accentColor = Color.FromArgb(210, 210, 214);
            }

            SmoothingMode oldMode = e.Graphics.SmoothingMode;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (var path = CreateRoundedRectanglePath(itemBounds, 12))
            using (var fillBrush = new SolidBrush(fillColor))
            using (var borderPen = new Pen(borderColor, isSelected ? 2.2f : 1.2f))
            {
                e.Graphics.FillPath(fillBrush, path);
                e.Graphics.DrawPath(borderPen, path);
            }

            // 좌측 선택 강조바 (선택 인지성 강화)
            var accentRect = new Rectangle(itemBounds.X + 2, itemBounds.Y + 6, 6, itemBounds.Height - 12);
            using (var accentBrush = new SolidBrush(accentColor))
            {
                e.Graphics.FillRectangle(accentBrush, accentRect);
            }

            e.Graphics.SmoothingMode = oldMode;

            string text = _lstSteps.Items[e.Index].ToString();

            // 선택 표시를 명확하게
            if (isSelected)
            {
                text = "✓ " + text;
            }
            else if (isRunning)
            {
                text = "● " + text;
            }

            var textRect = new Rectangle(itemBounds.X + 18, itemBounds.Y, itemBounds.Width - 24, itemBounds.Height);
            TextRenderer.DrawText(
                e.Graphics,
                text,
                _lstSteps.Font,
                textRect,
                isSelected ? Color.White : Color.FromArgb(28, 28, 30),
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }
        private static GraphicsPath CreateRoundedRectanglePath(Rectangle bounds, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;

            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();

            return path;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _playTimer.Stop();
                AlarmManager.Instance.AlarmAdded -= OnAlarmAdded;

                if (_runningDialog != null && !_runningDialog.IsDisposed)
                {
                    _runningDialog.Close();
                    _runningDialog.Dispose();
                }
            }

            base.Dispose(disposing);
        }
    }
}