using QMC.Common;
using QMC.Common.Motion.Ajin;
using QMC.Common.Motions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using QMC.LCP_280.Process; // AxisNames
using QMC.LCP_280.Process.Component; // InterlockManager, MachineHomeCoordinator

namespace QMC.LCP_280.Process.Unit
{
    [FormOrder(1)]
    /// <summary>
    /// Motion Setup: UI event + data/logic (Designer partial holds only GUI declarations)
    /// Config / Setup 자동 매핑 (ConfigReflectionMapper 사용)
    /// </summary>
    public partial class Motion_Setup : Form
    {
        private const string UNIT_NAME = "unit"; // axis manager key unit
        private Equipment Equipment => Equipment.Instance;

        // runtime references
        private MotionAxisManager _axisManager;
        private MotionAxis _axis; // currently selected axis

        // position viewer collection
        private PropertyCollection _editorProrertiesPosition;

        // reflection mappers (자동 UI ↔ 객체 동기화)
        private ConfigReflectionMapper _setupMapper;
        private ConfigReflectionMapper _configMapper;

        // timers / tasks
        private System.Windows.Forms.Timer _axisPosTimer; // disambiguated
        private CancellationTokenSource _homeCts;

        public Motion_Setup()
        {
            InitializeComponent();
            SuspendLayout();
            _axisManager = Equipment.AxisManager;
            InitializeLogic();
            ResumeLayout(true);
        }

        private void Motion_Setup_Load(object sender, EventArgs e) { }

        #region Init Logic / Wiring
        private void InitializeLogic()
        {
            try
            {
                WireAxisSelectionEvent();
                BindAxisList();
                InitializeStatusTimer();
            }
            catch (Exception ex)
            {
                Log.Write("MotionSetup", "Init", ex.ToString());
            }
        }

        private void WireAxisSelectionEvent()
        {
            if (selectAxisListBoxItemsView != null)
            {
                selectAxisListBoxItemsView.ItemSelected -= OnAxisSelected; // ensure single subscription
                selectAxisListBoxItemsView.ItemSelected += OnAxisSelected;
            }
            if (motorIoPropertyCollectionView != null)
            {
                motorIoPropertyCollectionView.ItemClicked -= OnMotorIOItemClicked;
                motorIoPropertyCollectionView.ItemClicked += OnMotorIOItemClicked;
            }
        }

        private void BindAxisList()
        {
            try
            {
                var axes = _axisManager?.GetAll();
                if (axes == null || axes.Length == 0)
                {
                    selectAxisListBoxItemsView?.SetItems("(No Axes)");
                    return;
                }
                var names = axes.Where(a => a != null)
                                 .Select(a => a.Name)
                                 .Distinct(StringComparer.OrdinalIgnoreCase)
                                 .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                                 .ToArray();
                selectAxisListBoxItemsView.GroupName = "Select Axis";
                selectAxisListBoxItemsView.SetItems(names);
                if (names.Length > 0) selectAxisListBoxItemsView.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Log.Write("MotionSetup", "BindAxisList", ex.ToString());
                selectAxisListBoxItemsView?.SetItems("(Error)");
            }
        }
        #endregion

        #region Axis Selection → Build PropertyCollections & Load Files
        private void OnAxisSelected(object sender, int selectedIndex)
        {
            try
            {
                if (selectedIndex < 0)
                {
                    _axis = null;
                    positionVelocityPropertyCollectionView?.SetProperties(null);
                    configurationListBoxItemsView?.SetProperties(null);
                    speedListBoxItemsView?.SetProperties(null);
                    _setupMapper = null;
                    _configMapper = null;
                    return;
                }

                var axisName = selectAxisListBoxItemsView?.SelectedItemName;
                if (string.IsNullOrWhiteSpace(axisName)) return;

                _axis = _axisManager?.GetAll()?.FirstOrDefault(a => a.Name.Equals(axisName, StringComparison.OrdinalIgnoreCase));
                if (_axis == null)
                {
                    configurationListBoxItemsView?.SetProperties(null);
                    speedListBoxItemsView?.SetProperties(null);
                    _setupMapper = null; _configMapper = null; return;
                }

                // load persisted files
                LoadAxisFiles(_axis);

                // reflection mappers (자동 속성 스캔)
                _setupMapper = new ConfigReflectionMapper(_axis.Setup);
                _configMapper = new ConfigReflectionMapper(_axis.Config);

                _editorProrertiesPosition = BuildPositionProperties(_axis);
                positionVelocityPropertyCollectionView?.SetProperties(_editorProrertiesPosition);
                configurationListBoxItemsView?.SetProperties(_setupMapper.PropertyCollection);
                speedListBoxItemsView?.SetProperties(_configMapper.PropertyCollection);

                BuildAndSetIOCollections(_axis);
                btnHome.Enabled = true; // enable per-axis home
            }
            catch (Exception ex)
            {
                Log.Write("MotionSetup", "OnAxisSelected", ex.ToString());
            }
        }

        private void LoadAxisFiles(MotionAxis axis)
        {
            if (axis == null) return;
            try
            {
                string axisRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "Axes", UNIT_NAME);
                Directory.CreateDirectory(axisRoot);
                string setupPath = Path.Combine(axisRoot, axis.Name + ".setup.json");
                string configPath = Path.Combine(axisRoot, axis.Name + ".config.json");

                if (File.Exists(setupPath))
                {
                    try { var loadedSetup = MotionAxisSetup.LoadOrCreate(setupPath, true, true); if (loadedSetup != null) axis.Setup = loadedSetup; } catch (Exception ex) { Log.Write("MotionSetup", $"Setup load failed {axis.Name}: {ex.Message}"); }
                }
                else { try { axis.Setup.Save(setupPath); } catch { } }

                if (File.Exists(configPath))
                {
                    try { var loadedCfg = MotionAxisConfig.LoadOrCreate(configPath, true, true); if (loadedCfg != null) axis.Config = loadedCfg; } catch (Exception ex) { Log.Write("MotionSetup", $"Config load failed {axis.Name}: {ex.Message}"); }
                }
                else { try { axis.Config.Save(configPath); } catch { } }
            }
            catch (Exception ex)
            {
                Log.Write("MotionSetup", "LoadAxisFiles", ex.ToString());
            }
        }
        #endregion

        #region Build PropertyCollections (Position Only)
        private PropertyCollection BuildPositionProperties(MotionAxis axis)
        {
            var pc = new PropertyCollection();
            pc.Add(new DoubleProperty("Command Position", axis.Status.PV.CommandPosition));
            pc.Add(new DoubleProperty("Actual Position", axis.Status.PV.ActualPosition));
            pc.Add(new DoubleProperty("Error Position", axis.Status.PV.ErrorPosition));
            pc.Add(new DoubleProperty("Command Velocity", axis.Status.PV.CommandVelocity));
            pc.Add(new DoubleProperty("Actual Velocity", axis.Status.PV.ActualVelocity));
            pc.IsInputParameter = false; // output only
            return pc;
        }
        #endregion

        #region IO collections
        private void BuildAndSetIOCollections(MotionAxis axis)
        {
            if (axis == null) return;
            var ioProperties = new PropertyCollection { ShowNoColumn = false };
            ioProperties.Add(new PropertyState("01", "Servo On", axis.Status.IO.ServoOn));
            ioProperties.Add(new PropertyState("02", "Alarm", axis.Status.IO.Alarm));
            ioProperties.Add(new PropertyState("03", "Negative Limit Sensor", axis.Status.IO.NegativeLimitSensor));
            ioProperties.Add(new PropertyState("04", "Positive Limit Sensor", axis.Status.IO.PositiveLimitSensor));
            ioProperties.Add(new PropertyState("05", "Home Sensor", axis.Status.IO.HomeSensor));
            motorIoPropertyCollectionView?.SetProperties(ioProperties);

            var state = new PropertyCollection { ShowNoColumn = false };
            state.Add(new PropertyState("01", "Done", axis.Status.State.Done));
            state.Add(new PropertyState("02", "Inposition", axis.Status.State.Inposition));
            state.Add(new PropertyState("03", "Inposition Done", axis.Status.State.InpositionDone));
            state.Add(new PropertyState("04", "Inposition Timeout", axis.Status.State.InpositionTimeout));
            state.Add(new PropertyState("05", "Home End", axis.Status.State.HomeEnd));
            state.Add(new PropertyState("06", "Home Timeout", axis.Status.State.HomeTimeout));
            state.IsInputParameter = false;
            motorStateIoPropertyCollectionView?.SetProperties(state);
        }
        #endregion

        #region Timer update
        private void InitializeStatusTimer()
        {
            _axisPosTimer?.Stop();
            if (_axisPosTimer != null) _axisPosTimer.Tick -= AxisPosTimer_Tick;
            _axisPosTimer = new System.Windows.Forms.Timer { Interval = 300 }; // disambiguated
            _axisPosTimer.Tick += AxisPosTimer_Tick;
            _axisPosTimer.Start();
        }

        private void AxisPosTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (_axis == null) return;
                // refresh runtime values
                _editorProrertiesPosition = BuildPositionProperties(_axis);
                positionVelocityPropertyCollectionView?.SetProperties(_editorProrertiesPosition);
                BuildAndSetIOCollections(_axis);
            }
            catch { }
        }
        #endregion

        #region Save Helpers
        private void ApplyAllPropertyViews()
        {
            try
            {
                configurationListBoxItemsView?.Apply();
                speedListBoxItemsView?.Apply();
            }
            catch { }
        }
        #endregion

        #region Button Events (Save / Servo / Home / HomeAll)
        private void btn_Save_Setup_Motion_Configuration_Click(object sender, EventArgs e)
        {
            try
            {
                if (_axis == null) { MessageBox.Show("축을 선택하세요."); return; }
                if (_setupMapper == null) { MessageBox.Show("Setup 매퍼가 없습니다."); return; }

                ApplyAllPropertyViews();
                var pc = configurationListBoxItemsView?.GetCurrentProperties();
                if (pc != null) _setupMapper.ApplyToObject(pc);

                string axisRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "Axes", UNIT_NAME);
                Directory.CreateDirectory(axisRoot);
                string setupPath = Path.Combine(axisRoot, _axis.Name + ".setup.json");
                _axis.Setup.Save(setupPath);

                // Reload to reflect normalization/backfill
                LoadAxisFiles(_axis);
                _setupMapper = new ConfigReflectionMapper(_axis.Setup);
                configurationListBoxItemsView?.SetProperties(_setupMapper.PropertyCollection);
                MessageBox.Show($"'{_axis.Name}' 설정 저장 완료.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("저장 오류: " + ex.Message);
            }
        }

        private void btn_Save_Setup_Motion_Speed_Click(object sender, EventArgs e)
        {
            try
            {
                if (_axis == null) { MessageBox.Show("축을 선택하세요."); return; }
                if (_configMapper == null) { MessageBox.Show("Config 매퍼가 없습니다."); return; }

                ApplyAllPropertyViews();
                var pc = speedListBoxItemsView?.GetCurrentProperties();
                if (pc != null) _configMapper.ApplyToObject(pc);

                string axisRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "Axes", UNIT_NAME);
                Directory.CreateDirectory(axisRoot);
                string cfgPath = Path.Combine(axisRoot, _axis.Name + ".config.json");
                _axis.Config.Save(cfgPath);

                LoadAxisFiles(_axis);
                _configMapper = new ConfigReflectionMapper(_axis.Config);
                speedListBoxItemsView?.SetProperties(_configMapper.PropertyCollection);
                MessageBox.Show($"'{_axis.Name}' 속도 설정 저장 완료.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("속도 저장 오류: " + ex.Message);
            }
        }

        private void btnServoOn_Click(object sender, EventArgs e)
        {
            try
            {
                if (_axis == null) { MessageBox.Show("축 선택 필요"); return; }
                int rc = _axis.Servo(true);
                if (rc != 0) MessageBox.Show($"Servo ON 실패(rc={rc})");
            }
            catch (Exception ex) { MessageBox.Show("Servo On 오류: " + ex.Message); }
        }

        private void btnServoOff_Click(object sender, EventArgs e)
        {
            try
            {
                if (_axis == null) { MessageBox.Show("축 선택 필요"); return; }
                int rc = _axis.Servo(false);
                if (rc != 0) MessageBox.Show($"Servo OFF 실패(rc={rc})");
            }
            catch (Exception ex) { MessageBox.Show("Servo Off 오류: " + ex.Message); }
        }

        private async void btnHomeAll_Click(object sender, EventArgs e)
        {
            HomeProgressForm dlg = null;
            try
            {
                // 홈 진행 전 사용자 확인
                var ask = new MessageBoxYesNo();
                if (ask.ShowDialog("확인", "축 홈을 진행하시겠습니까?") != DialogResult.Yes)
                    return;

                _homeCts?.Cancel();
                _homeCts?.Dispose();
                _homeCts = new CancellationTokenSource();
                var token = _homeCts.Token;
                var axes = _axisManager?.GetAll();
                if (axes == null || axes.Length == 0)
                {
                    MessageBox.Show("등록된 축이 없습니다.");
                    return;
                }
                foreach (var ax in axes)
                {
                    try { ax.ClearAlarm(); } catch { }
                    try { ax.Servo(true); } catch { }
                }
                var seq = MachineHomeCoordinator.BuildDefaultHomeSequence(Equipment);
                dlg = new HomeProgressForm();
                dlg.InitializeProgress("Machine Home", seq.TotalSteps);

                seq.OnProgress(p =>
                {
                    dlg.SafeUpdate(p);
                });

                dlg.CancelRequested += () =>
                {
                    try { _homeCts.Cancel(); } catch { }
                    try { Equipment.AxisManager?.EmgStopAll(); } catch { }
                };

                dlg.ForceStopRequested += () =>
                {
                    try { Equipment.AxisManager?.EmgStopAll(); } catch { }
                };

                var runTask = seq.RunAsync(token);

                dlg.Show(this);

                dlg.BringToFront();

                var completed = await Task.WhenAny(runTask, Task.Delay(Timeout.Infinite, token)).ConfigureAwait(true);
                if (completed != runTask)
                {
                    await Task.WhenAny(runTask, Task.Delay(2000)).ConfigureAwait(true);
                }
                var results = await runTask.ConfigureAwait(true);
                dlg.SafeUpdate(new OperationProgress { OperationId = "HOME", Title = "Home", StepIndex = seq.TotalSteps - 1, TotalSteps = seq.TotalSteps, IsCompleted = true, IsCanceled = token.IsCancellationRequested, IsAborted = seq.Aborted, Message = seq.AbortReason });
                int success = results.Count(r => r.Success);
                int notStarted = results.Count(r => !r.Started);
                int fail = results.Count - success - notStarted;
                string msg = $"Home 완료\r\n성공: {success}, 실패: {fail}, 미시작: {notStarted}";
                if (fail > 0 || notStarted > 0)
                {
                    var detailList = new List<string>();
                    foreach (var r in results)
                    {
                        string status;
                        if (r.Success)
                        {
                            status = "OK";
                        }
                        else if (r.Started)
                        {
                            if (r.Error != null && r.Error.Message != null)
                            {
                                status = r.Error.Message;
                            }
                            else
                            {
                                status = "rc=" + r.ReturnCode;
                            }
                        }
                        else
                        {
                            status = "NOT STARTED (" + r.FailReason + ")";
                        }

                        detailList.Add("- " + r.AxisName + ": " + status);
                    }
                    var detail = string.Join("\r\n", detailList);

                    msg += "\r\n\r\n" + detail;
                }
                MessageBox.Show(msg, "Home");
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("Home 취소됨");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Home 오류: " + ex.Message);
            }
            finally
            {
                try { dlg?.Close(); dlg?.Dispose(); } catch { }
            }
        }

        private async void btnHome_Click(object sender, EventArgs e)
        {
            HomeProgressForm dlg = null;
            try
            {
                if (_axis == null) 
                { 
                    MessageBox.Show("축 선택 필요"); 
                    return; 
                }

                // 홈 진행 전 사용자 확인 (축 이름 포함)
                var ask = new MessageBoxYesNo();
                if (ask.ShowDialog("확인", $"[{_axis.Name}] 축을 홈 진행할까요?") != DialogResult.Yes)
                    return;


                EnsureAxisHomeInterlocks(_axis);
                string reason;
                //if (!InterlockManager.Instance.ValidateAxisForHome(_axis, out reason))
                //{
                //    MessageBox.Show("홈 인터락 차단:\r\n" + reason);
                //    return;
                //}
                var seq = MachineHomeCoordinator.BuildDefaultHomeSequence(Equipment);
                var eval = await seq.PreAxisInterlockAsync(-1, _axis, CancellationToken.None).ConfigureAwait(true);
                if (!eval.Ok)
                {
                    MessageBox.Show("홈 인터락(코디네이터) 차단:\r\n" + eval.Reason);
                    return;
                }
                dlg = new HomeProgressForm();
                dlg.InitializeProgress($"Axis Home - {_axis.Name}", 1);
                dlg.SafeUpdate(new OperationProgress { OperationId = "HOME", Title = $"Home {_axis.Name}", StepIndex = 0, TotalSteps = 1, IsCompleted = false, Message = "Homing..." });
                bool userCanceled = false;
                dlg.CancelRequested += () => { userCanceled = true; try { Equipment.AxisManager?.EmgStopAll(); } catch { } };
                dlg.ForceStopRequested += () => { try { Equipment.AxisManager?.EmgStopAll(); } catch { } };
                var runTask = Task.Run(() => { try { _axis.ClearAlarm(); _axis.Servo(true); _axis.HomeSync(); } catch (Exception ex) { throw ex; } });
                dlg.Show(this); 
                dlg.BringToFront();
                await runTask.ConfigureAwait(true);
                dlg.SafeUpdate(new OperationProgress { OperationId = "HOME", Title = $"Home {_axis.Name}", StepIndex = 0, TotalSteps = 1, IsCompleted = true, IsCanceled = userCanceled, Message = userCanceled ? "Canceled" : "Completed" });
                MessageBox.Show(userCanceled ? $"축 {_axis.Name} 홈 취소" : $"축 {_axis.Name} 홈 완료");
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                MessageBox.Show("원점 구동 오류: " + ex.Message);
            }
            finally
            {
                try 
                { 
                    dlg?.Close(); 
                    dlg?.Dispose(); 
                } 
                catch { }
            }
        }
        #endregion

        #region Interlock helper
        private void EnsureAxisHomeInterlocks(MotionAxis axis)
        {
            if (axis == null) return;
            try
            {
                var il = InterlockManager.Instance; il.Start();
                if (axis.Name.Equals(AxisNames.WaferStageX, StringComparison.OrdinalIgnoreCase))
                {
                    var rules = il.GetRules();
                    bool Has(string n) => rules.Any(r => r.Name.Equals(n, StringComparison.OrdinalIgnoreCase));
                    if (!Has("WaferStageX_Home_VacuumOff")) il.AddAxisIoRequire("WaferStageX_Home_VacuumOff", axis, "WaferStageIO", "X201", false);
                    if (!Has("WaferStageX_Home_ClampOpen")) il.AddAxisIoRequire("WaferStageX_Home_ClampOpen", axis, "WaferStageIO", "X202", true);
                }
            }
            catch { }
        }
        #endregion

        #region IO click
        private void OnMotorIOItemClicked(object sender, string key)
        {
            var k = key?.Trim().ToUpperInvariant();
            if (string.IsNullOrEmpty(k) || _axis == null) return;
            try
            {
                if (k == "SERVO")
                {
                    bool wantOn = !_axis.Status.IO.ServoOn;
                    int rc = _axis.Servo(wantOn);
                    if (rc != 0) MessageBox.Show($"Servo {(wantOn ? "On" : "Off")} 실패(rc={rc})");
                }
                else if (k == "ALARM")
                {
                    int rc = _axis.ClearAlarm();
                    if (rc != 0) MessageBox.Show($"Alarm Reset 실패(rc={rc})");
                }
            }
            catch (Exception ex) { Log.Write(ex); }
        }
        #endregion

        #region Paint/Resize
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            int centerX = this.ClientSize.Width / 2;
            using (Pen blackPen = new Pen(Color.Black, 2))
            {
                e.Graphics.DrawLine(blackPen, centerX, 0, centerX, this.ClientSize.Height);
            }
        }
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }
        #endregion

        #region Panel Resize API
        public void SetPanelSize(int width, int height)
        {
            try
            {
                SuspendLayout();
                Size = new Size(width, height);
                ClientSize = new Size(width, height);
                Invalidate();
                Update();
            }
            finally { ResumeLayout(true); }
            Console.WriteLine($"Motion_Setup.SetPanelSize → {width}x{height}");
        }
        #endregion

        private void gbAxisPositions_Enter(object sender, EventArgs e)
        {

        }

        private void gbAxisProperty_Enter(object sender, EventArgs e)
        {

        }
    }
}
