using QMC.Common.DIO;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Reflection;

namespace QMC.Common
{
    public partial class TopContentsIOStatusControl : UserControl
    {
        // 내부상태
        private bool _lampRedOn;
        private bool _lampYellowOn;
        private bool _lampGreenOn;

        private bool _buzzerInput;              // 알람/문제 발생시 true (깜빡임 유지)
        private bool _buzzerSoundEnabled = true; // 소리 ON/OFF 상태

        private bool _blinkPhase;               // 깜빡임 위상

        private System.Windows.Forms.Timer _refreshTimer;            // I/O 상태 갱신 타이머
        private object _equipmentStatusUnit;    // EquipmentStatus 유닛 캐시

        // 외부 통지 이벤트(Operator_Main 등에서 실제 MUTE 출력 처리)
        public event EventHandler<bool> BuzzerSoundToggled;

        public TopContentsIOStatusControl()
        {
            InitializeComponent();

            // 더블버퍼링
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);

            // I/O 갱신 타이머 초기화 (100ms 주기)
            _refreshTimer = new System.Windows.Forms.Timer();
            _refreshTimer.Interval = 100;
            _refreshTimer.Tick += OnRefreshTimerTick;
        }

        // 컨트롤이 표시될 때 타이머 시작
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (Visible)
            {
                InitializeEquipmentStatus();
                _refreshTimer.Start();
            }
            else
            {
                _refreshTimer.Stop();
            }
        }

        // EquipmentStatus 유닛 초기화 및 캐싱
        private void InitializeEquipmentStatus()
        {
            try
            {
                var eq = EquipmentLocator.Instance;
                if (eq == null) return;

                // Reflection으로 Units 프로퍼티 접근
                var unitsProperty = eq.GetType().GetProperty("Units");
                if (unitsProperty != null)
                {
                    var units = unitsProperty.GetValue(eq) as System.Collections.IDictionary;
                    if (units != null && units.Contains("EquipmentStatus"))
                    {
                        _equipmentStatusUnit = units["EquipmentStatus"];
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"InitializeEquipmentStatus Error: {ex.Message}");
            }
        }

        // 주기적으로 EquipmentStatus에서 I/O 상태 읽기
        private void OnRefreshTimerTick(object sender, EventArgs e)
        {
            try
            {
                if (_equipmentStatusUnit == null)
                {
                    InitializeEquipmentStatus(); // 재시도
                    if (_equipmentStatusUnit == null) return;
                }

                // GetSnapshot 메서드 호출
                var getSnapshotMethod = _equipmentStatusUnit.GetType().GetMethod("GetSnapshot");
                if (getSnapshotMethod != null)
                {
                    var snapshot = getSnapshotMethod.Invoke(_equipmentStatusUnit, null);
                    if (snapshot != null)
                    {
                        // 타워램프 상태 업데이트
                        bool redOn = false;
                        bool yellowOn = false;
                        bool greenOn = false;

                        var outputsProperty = snapshot.GetType().GetProperty("Outputs");
                        if (outputsProperty != null)
                        {
                            var outputs = outputsProperty.GetValue(snapshot) as System.Collections.Generic.Dictionary<string, bool>;
                            if (outputs != null)
                            {
                                outputs.TryGetValue("TL_RED", out redOn);
                                outputs.TryGetValue("TL_YELLOW", out yellowOn);
                                outputs.TryGetValue("TL_GREEN", out greenOn);
                            }
                        }

                        UpdateLampInputs(redOn, yellowOn, greenOn);

                        // 부저 상태 업데이트
                        bool buzzerOn = false;
                        var outputs2 = outputsProperty?.GetValue(snapshot) as System.Collections.Generic.Dictionary<string, bool>;
                        outputs2?.TryGetValue("BUZZER", out buzzerOn);
                        UpdateBuzzerInput(buzzerOn);

                        // EMG 상태 확인
                        var anyEmgProperty = snapshot.GetType().GetProperty("AnyEmg");
                        if (anyEmgProperty != null)
                        {
                            bool anyEmg = (bool)anyEmgProperty.GetValue(snapshot);
                            if (anyEmg)
                            {
                                // EMG 발생 시 UI 표시
                                if (this.BackColor != Color.LightCoral)
                                {
                                    this.BackColor = Color.LightCoral; // 비상 상태 표시
                                }
                            }
                            else
                            {
                                if (this.BackColor != SystemColors.Control)
                                {
                                    this.BackColor = SystemColors.Control; // 정상 상태
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TopContentsIO Update Error: {ex.Message}");
            }
        }

        public void SetPanelSize(int width, int height)
        {
            this.SuspendLayout();
            tableLayoutContentsIOStatusPanel.SuspendLayout();
            try
            {
                // 비율 적용
                int panelWidth = (int)(width * 1.0);
                int panelHeight = (int)(height * 0.9);

                // tableLayoutMenuButtonPanel 크기 조정
                this.Size = new Size(panelWidth, panelHeight);
                tableLayoutContentsIOStatusPanel.Size = new Size(panelWidth, panelHeight);

                // 좌측 정렬, 위아래 중앙 정렬
                int x = 0; // 좌측
                int y = (this.Height - tableLayoutContentsIOStatusPanel.Height) / 2; // 위아래 중앙
                tableLayoutContentsIOStatusPanel.Location = new Point(x, y);
            }
            finally
            {
                tableLayoutContentsIOStatusPanel.ResumeLayout();
                this.ResumeLayout();
            }

            // 필요시 레이아웃 갱신
            tableLayoutContentsIOStatusPanel.Invalidate();
            this.Invalidate();
        }

        // ----------- 외부 갱신 API -----------
        /// <summary>타워램프 입력 표시(R/Y/G)</summary>
        public void UpdateLampInputs(bool redOn, bool yellowOn, bool greenOn)
        {
            _lampRedOn = redOn;
            _lampYellowOn = yellowOn;
            _lampGreenOn = greenOn;

            // UI 업데이트 (InvokeRequired 체크 추가)
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => UpdateLampUI()));
            }
            else
            {
                UpdateLampUI();
            }
        }

        private void UpdateLampUI()
        {
            // btnLampRed, btnLampYellow, btnLampGreen이 null이 아닌지 확인
            if (btnLampRed != null)
            {
                btnLampRed.Image = _lampRedOn ? Properties.Resources.ico_circle_red_14 : Properties.Resources.ico_circle_off_14;
                btnLampRed.Text = _lampRedOn ? "ON" : "OFF";
            }

            if (btnLampYellow != null)
            {
                btnLampYellow.Image = _lampYellowOn ? Properties.Resources.ico_circle_yellow_14 : Properties.Resources.ico_circle_off_14;
                btnLampYellow.Text = _lampYellowOn ? "ON" : "OFF";
            }

            if (btnLampGreen != null)
            {
                btnLampGreen.Image = _lampGreenOn ? Properties.Resources.ico_circle_green_14 : Properties.Resources.ico_circle_off_14;
                btnLampGreen.Text = _lampGreenOn ? "ON" : "OFF";
            }
        }

        /// <summary>부저 인풋(알람/문제 발생) 활성화 여부</summary>
        public void UpdateBuzzerInput(bool active)
        {
            _buzzerInput = active;

            if (btnBuzzerInput == null) return;

            if (_buzzerInput)
            {
                btnBuzzerInput.Text = "ACTIVE";
                btnBuzzerInput.Image = Properties.Resources.ico_speaker_green_14;
                if (timerBlink != null && !timerBlink.Enabled) timerBlink.Start();
            }
            else
            {
                if (timerBlink != null) timerBlink.Stop();
                _blinkPhase = false;
                btnBuzzerInput.Text = "IDLE";
                btnBuzzerInput.Image = Properties.Resources.ico_speaker_gray_14;
            }
        }

        /// <summary>외부에서 소리 상태를 동기화(초기값/장비쪽 변경 반영)</summary>
        public void SetBuzzerSoundState(bool soundOn)
        {
            _buzzerSoundEnabled = soundOn;
            if (btnBuzzerSound != null)
            {
                btnBuzzerSound.Image = _buzzerSoundEnabled ? Properties.Resources.ico_speaker_green_14
                                                           : Properties.Resources.ico_speaker_gray_14;
            }
        }

        // ----------- 이벤트 핸들러 -----------
        private void btnBuzzerSound_Click(object sender, EventArgs e)
        {
            // 소리만 제어 (신호/깜빡임은 그대로)
            _buzzerSoundEnabled = !_buzzerSoundEnabled;
            SetBuzzerSoundState(_buzzerSoundEnabled);

            // EquipmentStatus를 통해 실제 부저 제어
            try
            {
                if (_equipmentStatusUnit != null)
                {
                    var setOutputMethod = _equipmentStatusUnit.GetType().GetMethod("SetOutput");
                    if (setOutputMethod != null)
                    {
                        setOutputMethod.Invoke(_equipmentStatusUnit, new object[] { "BUZZER", _buzzerSoundEnabled });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Buzzer control error: {ex.Message}");
            }

            // 외부로 통지
            BuzzerSoundToggled?.Invoke(this, _buzzerSoundEnabled);
        }

        private void timerBlink_Tick(object sender, EventArgs e)
        {
            if (!_buzzerInput || btnBuzzerInput == null) return;

            _blinkPhase = !_blinkPhase;
            btnBuzzerInput.Image = _blinkPhase ? Properties.Resources.ico_speaker_green_14
                                               : Properties.Resources.ico_speaker_gray_14;
            btnBuzzerInput.Text = "ACTIVE";
        }

        // 타워램프 패턴 적용 헬퍼 메서드
        public void ApplyTowerPattern(string pattern)
        {
            try
            {
                if (_equipmentStatusUnit != null)
                {
                    var applyPatternMethod = _equipmentStatusUnit.GetType().GetMethod("ApplyTowerPattern");
                    if (applyPatternMethod != null)
                    {
                        // enum 값 매핑
                        object enumValue = null;
                        switch (pattern.ToLower())
                        {
                            case "idle":
                                enumValue = 0;
                                break;
                            case "running":
                                enumValue = 1;
                                break;
                            case "warning":
                                enumValue = 2;
                                break;
                            case "alarm":
                                enumValue = 3;
                                break;
                            case "alloff":
                                enumValue = 4;
                                break;
                        }

                        if (enumValue != null)
                        {
                            applyPatternMethod.Invoke(_equipmentStatusUnit, new object[] { enumValue });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Tower pattern apply error: {ex.Message}");
            }
        }
    }
}