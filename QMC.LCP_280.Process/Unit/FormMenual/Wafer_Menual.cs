using LCP_280;
using QMC.Common;
using QMC.Common.Account;
using QMC.Common.UI;
using QMC.LCP_280.Process.Component;
using QMC.LCP_280.Process.Unit.FormSetup;
using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static QMC.Common.Unit.BaseUnit;

namespace QMC.LCP_280.Process.Unit.FormWork
{
    [FormOrder(2)]
    /// <summary>
    /// WaferRing Working Form
    /// - TeachingPositionControl: InputFeeder, InputCassetteLifter
    /// - DIO 제어:
    ///    InputFeeder : Feeder Up/Down/Clamp 관련 센서 + 밸브 강제 제어
    ///    InputCassetteLifter : Cassette / RingJut / Mapping 센서 표시
    /// </summary>
    public partial class Wafer_Menual : Form
    {
        private const string WORK_NAME = "InputWafer";
        private Equipment Equipment => Equipment.Instance;

        
        private InputCassetteLifter InputCassetteLifter { get; set; }
        private InputFeeder InputFeeder { get; set; }
        private InputStage InputStage { get; set; }

        private MaterialCassette MaterialCassette { get; set; }

        private bool _initialized;
        private bool _preloadRequested;
        private bool _deferredInitDone; // 지연 초기화 완료 여부

        private DateTime _lastMarkDrawUtc = DateTime.MinValue;
        private TimeSpan _markDrawMinInterval = TimeSpan.FromMilliseconds(120); // 과도한 갱신 억제


        public Wafer_Menual() : this(
            TryGetUnit<InputCassetteLifter>("InputCassetteLifter"),
            TryGetUnit<InputFeeder>("InputFeeder"),
            TryGetUnit<InputStage>("InputStage")
            )
        {
        }

        public Wafer_Menual(InputCassetteLifter cassetteLifter, InputFeeder ringTransfer, InputStage inputStage)
        {
            InitializeComponent();
            InputCassetteLifter = cassetteLifter;
            InputFeeder = ringTransfer;
            InputStage = inputStage;

            Load += InputWafer_Working_Load;
            FormClosing += InputWafer_Working_FormClosing;
            _InputWaferCameraviewer.LightControlRequested += LightControlRequested;

            waferMapView_InputWafer.SetMaterialCassette(InputCassetteLifter.GetMaterialCassette());

            comboBoxSlot.SelectedIndex = 0;
        }

        /// <summary>
        /// 외부에서 Show 전에 UI 구성을 미리 수행 (Handle 확보)
        /// </summary>
        public void PreloadUI()
        {
            if (IsDisposed || Disposing) return;
            if (_preloadRequested) return;
            _preloadRequested = true;
            EnsureInitialized();
            var handle = Handle; // 강제 핸들 생성
        }


        private void InputWafer_Working_Load(object sender, EventArgs e)
        {
            EnsureInitialized();
        }

        private static T TryGetUnit<T>(string unitName) where T : class
        {
            try
            {
                var eq = Equipment.Instance;
                if (eq?.Units != null && eq.Units.TryGetValue(unitName, out var u))
                    return u as T;
            }
            catch { }
            return null;
        }

        private void EnsureInitialized()
        {
            if (_initialized) return;
            _initialized = true;
            try
            {
                Text = $"{WORK_NAME} Working";
                BeginInvoke(new Action(StartDeferredInit)); // 무거운 바인딩 지연
            }
            catch (Exception ex)
            {
                try { Controls.Add(new Label { Dock = DockStyle.Fill, Text = $"Init 실패: {ex.Message}", ForeColor = System.Drawing.Color.Red, TextAlign = System.Drawing.ContentAlignment.MiddleCenter }); } catch { }
            }
        }

        private async void StartDeferredInit()
        {
            if (_deferredInitDone) return;
            _deferredInitDone = true;
            await Task.Delay(30); // 첫 Paint 이후 수행 유도
            if (IsDisposed || Disposing) return;
            try
            {
                BindTeachingPositions();
                BindDioControls();
                BindCamera();
                InitSequences();
                SubscribeInputStageMarkEvents();   // ★ 추가: 마크 이벤트 구독
            }
            catch { }
        }

        #region Teaching
        private void BindTeachingPositions()
        {
            try
            {
                if (teachingPositionControl == null) return;
                teachingPositionControl.ClearUnits();

                if (InputFeeder != null)
                {
                    teachingPositionControl.RegisterUnit(
                        "InputFeeder",
                        InputFeeder,
                        () => InputFeeder.Config?.TeachingPositions,
                        (name, vel) => InputFeeder.MoveToTeachingPosition(name, vel: vel),
                        //(name, vel) => InputFeeder.MoveTeachingPositionOnce(name, vel),
                        tp => InputFeeder.Config?.SetTeachingPosition(tp),
                        autoReload: false);
                }

                if (InputCassetteLifter != null)
                {
                    teachingPositionControl.RegisterUnit(
                        "InputCassetteLifter",
                        InputCassetteLifter,
                        () => InputCassetteLifter.Config?.TeachingPositions,
                        (name, vel) => InputCassetteLifter.MoveToTeachingPosition(name, vel: vel),
                        tp => InputCassetteLifter.Config?.SetTeachingPosition(tp),
                        autoReload: false);
                }

                teachingPositionControl.SetSaveCancelVisible(false, false);
                teachingPositionControl.RefreshData();
            }
            catch { }
        }
        #endregion

        #region DIO
        private void BindDioControls()
        {
            try
            {
                if (dioControl == null) 
                    return;
                dioControl.IoSortMode = DIOControl.SortingMode.Insertion;

                if (InputFeeder != null)
                {
                    StrongBindInputFeeder();
                }

                if (InputCassetteLifter != null)
                {
                    dioControl.BindDIOInput(() => InputCassetteLifter.IsCassettePresent0(), "Cassette Present 0", "ICL_Cass0");
                    dioControl.BindDIOInput(() => InputCassetteLifter.IsCassettePresent1(), "Cassette Present 1", "ICL_Cass1");
                    dioControl.BindDIOInput(() => InputCassetteLifter.IsAnyCassettePresent(), "Cassette Any", "ICL_CassAny");
                    dioControl.BindDIOInput(() => InputCassetteLifter.IsWaferProtrusionDetectionSensor(), "Ring Jut", "ICL_RingJut");
                    dioControl.BindDIOInput(() => InputCassetteLifter.MappingSensor(), "Mapping Sensor", "ICL_Mapping");
                }

                dioControl.RebuildLists();
            }
            catch { }
        }

        private void StrongBindInputFeeder()
        {
            if (InputFeeder == null || dioControl == null) return;

            // 간단 디바운스: required회 연속으로 true가 나와야 true로 인정
            bool ConsecutiveTrue(Func<bool> read, int required = 3, int intervalMs = 2)
            {
                int count = 0;
                for (int i = 0; i < required; i++)
                {
                    bool v = false;
                    try { v = read(); } catch { v = false; }
                    if (v) count++; else count = 0;
                    if (i < required - 1) 
                        System.Threading.Thread.Sleep(intervalMs);
                }
                return count >= required;
            }

            // Lift(Up/Down): 두 센서 일관성 체크 + 디바운스
            bool IsLiftUpStable() =>
                ConsecutiveTrue(() => InputFeeder.IsFeederUp() && !InputFeeder.IsFeederDown(), 2, 10);
            bool IsLiftDownStable() =>
                ConsecutiveTrue(() => InputFeeder.IsFeederDown() && !InputFeeder.IsFeederUp(), 2, 10);

            // Clamp/Unclamp: Unclamp 센서만 있는 경우
            bool IsClampedStable() =>
                ConsecutiveTrue(() => !InputFeeder.IsUnClamped(), 2, 10);
            bool IsUnclampedStable() =>
                ConsecutiveTrue(() => InputFeeder.IsUnClamped(), 2, 10);

            try
            {
                // Sensors (표시용도 역시 흔들리면 동일 래퍼로 감싸는 것을 권장)
                dioControl.BindDIOInput(() => ConsecutiveTrue(() => InputFeeder.IsFeederUp()), "Feeder UP Sns", "IRT_FeederUp");
                dioControl.BindDIOInput(() => ConsecutiveTrue(() => InputFeeder.IsFeederDown()), "Feeder DOWN Sns", "IRT_FeederDown");
                dioControl.BindDIOInput(() => ConsecutiveTrue(() => InputFeeder.IsUnClamped()), "Feeder UNCLAMP Sns", "IRT_Unclamp");
                dioControl.BindDIOInput(() => ConsecutiveTrue(() => InputFeeder.IsRingPresent()), "Feeder RING Sns", "IRT_Ring");
                dioControl.BindDIOInput(() => ConsecutiveTrue(() => InputFeeder.IsOverload()), "Feeder OVERLOAD Sns", "IRT_Overload");

                // Feeder Up/Down (서로 배타 제어)
                dioControl.BindCylinder(
                    label: "Up/Down",
                    extend: () => InputFeeder.SetLift(true),   // 반대 코일 OFF → 목표 코일 ON은 SetLift 내부에서 보장되도록
                    retract: () => InputFeeder.SetLift(false),
                    isExtended: () => IsLiftUpStable(),        // 밸브 코일이 아니라 '위치 센서' 기준
                    isRetracted: () => IsLiftDownStable(),
                    displayKey: "FeederUpDn",
                    showSensors: false,
                    extendedName: "UP",
                    retractedName: "DOWN"
                );

                // Feeder Clamp/Unclamp (서로 배타 제어)
                dioControl.BindCylinder(
                    label: "Clamp/Unclamp",
                    extend: () => InputFeeder.SetClamp(true),
                    retract: () => InputFeeder.SetClamp(false),
                    isExtended: () => IsClampedStable(),       // Unclamp 센서의 부정으로 클램프 상태 판단
                    isRetracted: () => IsUnclampedStable(),
                    displayKey: "FeederClamp",
                    showSensors: false,
                    extendedName: "CLAMP",
                    retractedName: "UNCLAMP"
                );
            }
            catch { }
        }
        #endregion

        #region Camera
        private void BindCamera()
        {
            try
            {
                if (_InputWaferCameraviewer != null && InputStage?.StageCamera != null)
                {
                    if (_InputWaferCameraviewer.Camera != InputStage.StageCamera)
                        _InputWaferCameraviewer.Camera = InputStage.StageCamera;
                    try { InputStage.StageCamera.StartLive(); } catch { }
                    try { _InputWaferCameraviewer.StartUpdateTask(); } catch { }
                }
            }
            catch { }
        }
        #endregion


        #region Sequences
        private void InitSequences()
        {
            try
            {
                // 최신 Equipment 등록본으로 다시 참조 갱신 (폼 생성 후 재초기화 상황 대비)
                InputCassetteLifter = TryGetUnit<InputCassetteLifter>("InputCassetteLifter");
                InputFeeder = TryGetUnit<InputFeeder>("InputFeeder");
                InputStage = TryGetUnit<InputStage>("InputStage");
                
                if (InputCassetteLifter != null)
                {
                    manualSequenceControlInputCassette.ParentUnit = InputCassetteLifter;
                }

                if (InputFeeder != null)
                {
                    //manualSequenceControlInputWafer.ParentUnit = InputFeeder; // 시퀀스 등록 대상 유닛 지정
                    manualSequenceControlInputFeeder.ParentUnit = InputFeeder;
                }

                if (InputStage != null)
                {
                    manualSequenceControlInputWaferStage.ParentUnit = InputStage; // 시퀀스 등록 대상 유닛 지정
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }
        #endregion

        private void InputWafer_Working_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                UnsubscribeInputStageMarkEvents();
            }
            catch { }
        }

        // 마크 이벤트 구독 / 해제 메서드 추가
        private void SubscribeInputStageMarkEvents()
        {
            try
            {
                if (InputStage != null)
                {
                    // 중복 방지 위해 먼저 제거
                    InputStage.MarksFound -= InputStage_MarksFound;
                    InputStage.MarksFound += InputStage_MarksFound;
                }
            }
            catch { }
        }

        private void UnsubscribeInputStageMarkEvents()
        {
            try
            {
                if (InputStage != null)
                    InputStage.MarksFound -= InputStage_MarksFound;
            }
            catch { }
        }

        // 이벤트 핸들러 구현
        private void InputStage_MarksFound(object sender, PatternMarksFoundEventArgs e)
        {
            // 스로틀
            var now = DateTime.UtcNow;
            if (now - _lastMarkDrawUtc < _markDrawMinInterval)
                return;
            _lastMarkDrawUtc = now;

            // UI 스레드 처리
            if (_InputWaferCameraviewer == null || _InputWaferCameraviewer.IsDisposed)
                return;


            //_InputWaferCameraviewer.ResumeDisplay();
            if (_InputWaferCameraviewer.InvokeRequired)
            {
                _InputWaferCameraviewer.Invoke(new Action(() => ApplyInputStageMarkOverlays(e)));
            }
            else
            {
                ApplyInputStageMarkOverlays(e);
            }
        }

        // Overlay 적용 메서드
        private void ApplyInputStageMarkOverlays(PatternMarksFoundEventArgs e)
        {
            try
            {
                if (_InputWaferCameraviewer == null) return;

                // 라이브 그랩 중이라면 이미지 교체는 선택 – 필요 시만 적용
                // if (e.Image != null && !_InputWaferCameraviewer.Simulated)
                //     _InputWaferCameraviewer.InputImage = e.Image;  // 프로젝트의 실제 이미지 교체 API에 맞게 조정

                // ResultOverlays 컬렉션 반영 (VisionImageViewer 내부 구현이 해당 컬렉션을 그리는 구조라고 가정)
                var overlaysProp = _InputWaferCameraviewer.GetType().GetProperty("ResultOverlays");
                var list = overlaysProp?.GetValue(_InputWaferCameraviewer) as System.Collections.IList;

                if (list != null)
                {
                    lock (list)
                    {
                        list?.Clear();

                        if (e.Marks == null || e.Marks.Count == 0)
                        {
                            _InputWaferCameraviewer.Refresh();
                            return;
                        }

                        for (int i = 0; i < e.Marks.Count; i++)
                        {
                            var m = e.Marks[i];
                            var ov = new QMC.Common.Vision.PatternMatchResultOverlay
                            {
                                Center = new System.Drawing.PointF((float)m.X, (float)m.Y),
                                PatternWidth = m.TrainW > 0 ? m.TrainW : 40,
                                PatternHeight = m.TrainH > 0 ? m.TrainH : 40,
                                AngleDeg = (float)m.AngleDeg,
                                CrossHalfLenPx = (i == e.RepresentativeIndex) ? 24 : 16,
                                Highlight = (i == e.RepresentativeIndex),
                                Index = i,
                                Color = (i == e.RepresentativeIndex) ? System.Drawing.Color.Yellow : System.Drawing.Color.Lime,
                                Thickness = (i == e.RepresentativeIndex) ? 2f : 1f,
                                Visible = true
                            };
                            list?.Add(ov);
                        }
                    }
                }
                _InputWaferCameraviewer.ResumeDisplay();
            }
            catch (Exception ex)
            {
                Log.Write("InputWafer_Working", "ApplyInputStageMarkOverlays", ex.Message);
            }
        }

        private void btnMapping_Click(object sender, EventArgs e)
        {
            try
            {
                if (InputCassetteLifter == null)
                {
                    MessageBox.Show("InputCassetteLifter 초기화되지 않았습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Wafer 감지 수행.
                var v = InputCassetteLifter.ScanWaferAsync();
                ProgressForm progressForm = new ProgressForm("Cassette Mapping","Scanning......" ,v);
                progressForm.ShowDialog(this);

                int nRet = v.Result;
                if (nRet != 0)
                {
                    //MessageBox.Show($"Wafer 감지 실패. 오류 코드: {nRet}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Log.Write("InputWafer_Working", "", $"Wafer 감지 실패. 오류 코드: {nRet}", "오류");
                    return;
                }
                // scan 후 재설정.
                var materialCassette = InputCassetteLifter.GetMaterialCassette();
                if (materialCassette == null)
                {
                    MessageBox.Show("Wafer 감지 실패.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                waferMapView_InputWafer.SetMaterialCassette(materialCassette);
                waferMapView_InputWafer.Refresh();

                MessageBox.Show("Wafer 감지가 완료되었습니다.", "정보", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Wafer 감지 중 오류 발생: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void checkBoxTest_CheckedChanged(object sender, EventArgs e)
        {
            //todo : 사용금지 - config 불러오기/저장 다시 만들어야함.
            var ask = new MessageBoxYesNo();
            if (ask.ShowDialog("확인", "DryRun 모드를 변경합니다. 진행하시겠습니까?") != DialogResult.Yes)
                return;

            if (checkBoxTest.Checked)
            {
                //Equipment?.ConfigManager?.ApplyGlobalDryRunAndSave(true, save: true);
            }
            else if(!checkBoxTest.Checked)
            {
                //Equipment?.ConfigManager?.ApplyGlobalDryRunAndSave(false, save: true);
            }
            else
            {
                //Equipment?.ConfigManager?.ApplyGlobalDryRunAndSave(false, save: false);
            }
        }

        private void checkBoxSimulation_CheckedChanged(object sender, EventArgs e)
        {
            //todo : 사용금지 - config 불러오기/저장 다시 만들어야함.
            var ask = new MessageBoxYesNo();
            if (ask.ShowDialog("확인", "Simulation 모드를 변경합니다. 진행하시겠습니까?") != DialogResult.Yes)
                return;

            if (checkBoxSimulation.Checked)
            {
                //Equipment?.ConfigManager?.ApplyGlobalSimulationAndSave(true, save: true);
            }
            else if (!checkBoxSimulation.Checked)
            {
                //Equipment?.ConfigManager?.ApplyGlobalSimulationAndSave(false, save: true);
            }
            else
            {
                //Equipment?.ConfigManager?.ApplyGlobalSimulationAndSave(false, save: false);
            }
        }

        private void buttonRequstInput_Click(object sender, EventArgs e)
        {
            var ask = new MessageBoxYesNo();
            if (ask.ShowDialog("확인", "RequestLoadWafe. 진행하시겠습니까?") != DialogResult.Yes)
                return;

        }

        private void LightControlRequested(object sender, EventArgs e)
        {
            Form _lightControlPopup = null;

            if (_lightControlPopup != null && !_lightControlPopup.IsDisposed)
            {
                _lightControlPopup.Close();
            }

            Form popupForm = new Form();
            popupForm.Text = "Light Control";
            popupForm.Size = new Size(467, 286);
            popupForm.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            popupForm.MaximizeBox = false;
            popupForm.MinimizeBox = false;
            popupForm.ShowInTaskbar = false;

            popupForm.StartPosition = FormStartPosition.Manual;
            Point cursorPos = Cursor.Position;
            popupForm.Location = cursorPos;

            popupForm.Owner = null;

            SimpleLightControl lightControl = new SimpleLightControl();
            lightControl.Dock = DockStyle.Fill;
            popupForm.Controls.Add(lightControl);

            _lightControlPopup = popupForm;
            popupForm.FormClosed += (s, ev) => { _lightControlPopup = null; };
            popupForm.Show();
            this.Activate();
        }

        private async void buttonMoveToSlot_Click(object sender, EventArgs e)
        {
            var ask = new MessageBoxYesNo();
            if (ask.ShowDialog("Question", "Are you sure you want Move?") != DialogResult.Yes)
                return;

            int nRet = 0;
            var cts = new CancellationTokenSource();
            var token = cts.Token;
            SetUnitsManualRunning(true);

            var t = Task.Run(async () =>
            {
                token.ThrowIfCancellationRequested();

                nRet = await Task.Run(() => MoveToSlotAsync(token), token).ConfigureAwait(false);
                if (nRet != 0)
                {
                    Log.Write("LCP-280", "buttonMoveToSlot_Click", "MoveToSlotAsync failed");
                    return nRet;
                }
                return nRet;

            }, token);

            var form = new ProgressForm("Manual Running", "Move To Slot", t, null);
            try
            {
                var mb = new MessageBoxOk();
                form.ShowDialog();
                if (t.IsFaulted)
                {
                    //mb.ShowDialog("MoveToSlot Fail!", t.Exception?.GetBaseException().Message);
                    SetUnitsManualRunning(false);
                    return;
                }

                if (t.IsCanceled)
                {
                    //mb.ShowDialog("MoveToSlot Cancel!", t.Exception?.GetBaseException().Message);
                    SetUnitsManualRunning(false);
                    return;
                }

                var rc = await t.ConfigureAwait(true);
                if (rc != 0)
                {
                    //MessageBox.Show("MoveToSlot Fail",
                    //    "MoveToSlot Fail", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                SetUnitsManualRunning(false);
                return;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return;
            }
            finally
            {
                SetUnitsManualRunning(false);
            }
        }

        private int _nSelectSlot = 0;
        private void SetUnitsManualRunning(bool running)
        {
            try
            {
                if (running)
                {
                    InputCassetteLifter.StartManual();
                    InputFeeder.StartManual();
                    InputStage.StartManual();
                }
                else
                {
                    InputCassetteLifter.Stop();
                    InputFeeder.Stop();
                    InputStage.Stop();
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

        private async Task<int> MoveToSlotAsync(CancellationToken ct)
        {
            return await Task.Run(() =>
            {
                int nRet = 0;

                if (_nSelectSlot == -1)
                {
                    var mb = new MessageBoxOk();
                    mb.ShowDialog("Warring", "No Select Slot.");
                }

                nRet = this.InputCassetteLifter.MoveToSlot(_nSelectSlot); // 언로딩 해야하는 Slot으로 이동 요청.
                if (nRet != 0)
                {
                    Log.Write("LCP-280", "buttonMoveToSlot_Click", "MoveToSlot Fail.");
                    return nRet;
                }

                return nRet;
            }, ct).ConfigureAwait(false);
        }

        private void comboBoxSlot_SelectedIndexChanged(object sender, EventArgs e)
        {
            _nSelectSlot = comboBoxSlot.SelectedIndex;
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            InputStage.TryGetMarkCenterGlobalDirectional(true, null, out var gx, out var gy, out var s);
        }
    }
}
