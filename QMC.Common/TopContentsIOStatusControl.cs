using QMC.Common.DIO;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace QMC.Common
{
    public partial class TopContentsIOStatusControl : UserControl
    {
        // 내부상태
        private bool _lampRedOn;
        private bool _lampYellowOn;
        private bool _lampGreenOn;

        private bool _buzzerInput;               // 알람/문제 발생시 true (깜빡임 유지)
        private bool _buzzerSoundEnabled = true; // 소리 ON/OFF 상태

        private bool _blinkPhase;                // 깜빡임 위상

        private System.Windows.Forms.Timer _refreshTimer; // I/O 상태 갱신 타이머

        // EquipmentStatus 유닛 캐시(반사 사용)
        private object _equipmentStatusUnit;

        // 리플렉션 MethodInfo 캐시 (오버헤드 절감)
        private MethodInfo _miGetSnapshot;
        private MethodInfo _miSetOutput;
        private MethodInfo _miApplyTowerPattern;

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

        // 외부에서 직접 EquipmentStatus 주입
        public void AttachEquipmentStatus(object equipmentStatusUnit)
        {
            _equipmentStatusUnit = equipmentStatusUnit;
            CacheEquipmentStatusMethods();
            if (Visible && _refreshTimer != null && !_refreshTimer.Enabled)
                _refreshTimer.Start();
        }

        // 연결 해제
        public void DetachEquipmentStatus()
        {
            _equipmentStatusUnit = null;
            _miGetSnapshot = null;
            _miSetOutput = null;
            _miApplyTowerPattern = null;
            if (_refreshTimer != null && _refreshTimer.Enabled)
                _refreshTimer.Stop();
        }

        private bool IsDesignModeSafe()
        {
            return LicenseManager.UsageMode == LicenseUsageMode.Designtime
                   || (this.Site?.DesignMode ?? false)
                   || this.DesignMode;
        }

        private void EnsureRefreshTimer()
        {
            if (_refreshTimer == null)
            {
                _refreshTimer = new System.Windows.Forms.Timer();
                _refreshTimer.Interval = 100;
                _refreshTimer.Tick += OnRefreshTimerTick;
            }
        }

        // 컨트롤이 표시될 때 타이머 시작
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            // 디자인 모드/Dispose 상태에서는 아무것도 하지 않음
            if (IsDisposed || Disposing || IsDesignModeSafe())
                return;

            try
            {
                if (Visible)
                {
                    // 장비 유닛 캐시 시도
                    if (_equipmentStatusUnit == null)
                        InitializeEquipmentStatus();

                    // 타이머 보장 후 시작
                    EnsureRefreshTimer();
                    if (!_refreshTimer.Enabled)
                        _refreshTimer.Start();
                }
                else
                {
                    if (_refreshTimer != null && _refreshTimer.Enabled)
                        _refreshTimer.Stop();
                }
            }
            catch
            {
                // 표시 전환 중 예외는 무시(디자이너/수명 주기 중간 단계 보호)
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_refreshTimer != null)
                {
                    try { _refreshTimer.Stop(); } catch { }
                    try { _refreshTimer.Tick -= OnRefreshTimerTick; } catch { }
                    try { _refreshTimer.Dispose(); } catch { }
                    _refreshTimer = null;
                }
            }
            base.Dispose(disposing);
        }

        // EquipmentStatus 유닛 초기화 및 캐싱 (fallback)
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
                        CacheEquipmentStatusMethods();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"InitializeEquipmentStatus Error: {ex.Message}");
            }
        }

        // 반사 MethodInfo 캐싱
        private void CacheEquipmentStatusMethods()
        {
            try
            {
                if (_equipmentStatusUnit == null) return;
                var t = _equipmentStatusUnit.GetType();
                _miGetSnapshot = _miGetSnapshot ?? t.GetMethod("GetSnapshot", BindingFlags.Instance | BindingFlags.Public);
                _miSetOutput = _miSetOutput ?? t.GetMethod("SetOutput", BindingFlags.Instance | BindingFlags.Public);
                _miApplyTowerPattern = _miApplyTowerPattern ?? t.GetMethod("ApplyTowerPattern", BindingFlags.Instance | BindingFlags.Public);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CacheEquipmentStatusMethods Error: {ex.Message}");
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

                // GetSnapshot 호출
                if (_miGetSnapshot == null) 
                    CacheEquipmentStatusMethods();

                var snapshot = _miGetSnapshot?.Invoke(_equipmentStatusUnit, null);
                if (snapshot == null) 
                    return;

                // 타워램프 출력 상태 읽기
                bool redOn = false, yellowOn = false, greenOn = false;
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
                
                if(EquipmentLocator.Instance.m_bBuzzerOff)
                {
                    buzzerOn = false;
                }
                UpdateBuzzerInput(buzzerOn);

                // EMG 상태 확인
                var anyEmgProperty = snapshot.GetType().GetProperty("AnyEmg");
                if (anyEmgProperty != null)
                {
                    bool anyEmg = (bool)anyEmgProperty.GetValue(snapshot);
                    this.BackColor = anyEmg ? Color.LightCoral : SystemColors.Control;
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

            if (InvokeRequired) BeginInvoke(new Action(UpdateLampUI));
            else UpdateLampUI();
        }

        private void UpdateLampUI()
        {
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
                    if (_miSetOutput == null) 
                        CacheEquipmentStatusMethods();
                    _miSetOutput?.Invoke(_equipmentStatusUnit, new object[] { "BUZZER", _buzzerSoundEnabled });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Buzzer control error: {ex.Message}");
            }

            // 외부로 통지
            BuzzerSoundToggled?.Invoke(this, _buzzerSoundEnabled);

            if(EquipmentLocator.Instance.m_bBuzzerOff == true)
            {
                EquipmentLocator.Instance.m_bBuzzerOff = false;
            }
            else
            {
                EquipmentLocator.Instance.m_bBuzzerOff = true;
            }
            //EquipmentLocator.Instance.m_bBuzzerOff = true;
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
                if (_equipmentStatusUnit == null) return;

                if (_miApplyTowerPattern == null) CacheEquipmentStatusMethods();
                if (_miApplyTowerPattern == null) return;

                // enum 동적 파싱(TowerLampPattern) - 이름 기반
                var param = _miApplyTowerPattern.GetParameters();
                if (param == null || param.Length != 1) return;

                var enumType = param[0].ParameterType;
                object enumValue = null;

                var name = (pattern ?? "").Trim();
                if (name.Length > 0)
                {
                    foreach (var en in Enum.GetNames(enumType))
                    {
                        if (string.Equals(en, name, StringComparison.OrdinalIgnoreCase))
                        {
                            enumValue = Enum.Parse(enumType, en, true);
                            break;
                        }
                    }
                    if (enumValue == null)
                    {
                        switch (name.ToLowerInvariant())
                        {
                            case "idle": enumValue = Enum.Parse(enumType, "Idle", true); break;
                            case "running": enumValue = Enum.Parse(enumType, "Running", true); break;
                            case "warning": enumValue = Enum.Parse(enumType, "Warning", true); break;
                            case "alarm": enumValue = Enum.Parse(enumType, "Alarm", true); break;
                            case "alloff": enumValue = Enum.Parse(enumType, "AllOff", true); break;
                        }
                    }
                }

                if (enumValue != null)
                {
                    _miApplyTowerPattern.Invoke(_equipmentStatusUnit, new object[] { enumValue });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Tower pattern apply error: {ex.Message}");
            }
        }
    }
}