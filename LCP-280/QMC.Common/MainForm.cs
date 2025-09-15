using QMC.Common.Alarm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace QMC.Common
{
    public partial class MainForm : Form
    {
        #region Field
        private bool _isShuttingDown;

        private Size MainSize;
        private TableLayoutPanel tableLayoutPanelFormMain;
        private Panel centerPanel; // 중앙 컨텐츠 전용 컨테이너
        private FormTop formTop;
        private FormBottom formBottom;
        private Control currentCenterView;

        private Form_Alarm formAlarm; // 알람 폼 인스턴스

        // 메뉴별 중앙 뷰 캐시
        private readonly Dictionary<MenuButtonType, Control> _centerViewCache = new Dictionary<MenuButtonType, Control>();

        // Prewarm
        private Queue<MenuButtonType> _prewarmQueue;
        private Timer _prewarmTimer;

        #endregion

        public MainForm()
        {
            InitializeComponent();

            // 🔧 MainForm 배경색을 흰색으로 설정
            this.BackColor = Color.White;
            this.DoubleBuffered = true;
            this.StartPosition = FormStartPosition.WindowsDefaultLocation;
            this.WindowState = FormWindowState.Normal;           // 일반 상태로 시작
            this.Load += MainForm_Load;
            // ⬇ 종료 이벤트 연결
            this.FormClosing += MainForm_FormClosing;
            this.FormClosed += MainForm_FormClosed;

            // (선택) 프로세스 종료 훅 – 폼이 강제 종료될 때도 안전장치
            AppDomain.CurrentDomain.ProcessExit += (_, __) =>
            {
                //try { EquipmentLocator.Instance?.Dispose(); } catch { }
                try
                {
                    try { EquipmentLocator.ShutdownAndDisposeSafely(); } catch { }
                }
                catch { }
            };
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            MainSize = new Size(1280, 1024);
            // 권장: 클라이언트 영역 기준으로 설정
            this.ClientSize = MainSize;

            // 🔧 MainForm 배경색을 다시 한 번 확실히 흰색으로 설정
            this.BackColor = Color.White;

            // FormManager 시스템 초기화
            InitializeFormManagers();

            // TableLayoutPanel 생성 및 설정
            tableLayoutPanelFormMain = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1,
                // 🔧 TableLayoutPanel 배경색도 흰색으로 설정
                BackColor = Color.White,
                Margin = new Padding(0)
            };

            // 각 행을 동일한 비율로 분할
            tableLayoutPanelFormMain.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
            tableLayoutPanelFormMain.RowStyles.Add(new RowStyle(SizeType.Percent, 84F)); //80
            tableLayoutPanelFormMain.RowStyles.Add(new RowStyle(SizeType.Percent, 6F));  //10
            tableLayoutPanelFormMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            this.Controls.Add(tableLayoutPanelFormMain);

            // FormTop을 첫번째 행(인덱스 0)에 추가
            formTop = new FormTop()
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill,
                Margin = new Padding(0)
            };
            tableLayoutPanelFormMain.Controls.Add(formTop, 0, 0);
            formTop.Show();

            // Center 전용 Panel 생성 및 추가
            centerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Margin = new Padding(0)
            };
            EnableDoubleBuffer(centerPanel);
            tableLayoutPanelFormMain.Controls.Add(centerPanel, 0, 1);

            // FormBottom을 세 번째 행(인덱스 2에 추가
            formBottom = new FormBottom()
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill,
                Margin = new Padding(0)
            };
            tableLayoutPanelFormMain.Controls.Add(formBottom, 0, 2);
            formBottom.Show();

            // FormBottom의 메뉴 버튼 클릭 이벤트 구독
            formBottom.MenuButtonClicked += FormBottom_MenuButtonClicked;

            // 폼이 보여진 후 실제 Width, Height 전달 및 리사이즈 연동
            this.Shown += (s, args) =>
            {
                ApplySizes();
                StartPrewarm();
            };
            this.SizeChanged += (s, args) => ApplySizes();

            // 🚀 폼이 처음 로드될 때 기본으로 Main 폼을 중앙에 표시
            SwitchCenterView(MenuButtonType.Main);

            formAlarm = new Form_Alarm();
            AlarmManager.Instance.PostAlarm += AlarmManager_PostAlarm;
        }

        // [MOD] 종료 이벤트 비동기화
        private async void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_isShuttingDown) return;
            _isShuttingDown = true;
            this.Enabled = false;
            this.Cursor = Cursors.WaitCursor;

            try
            {
                try
                {
                    if (formAlarm != null && !formAlarm.IsDisposed)
                    {
                        formAlarm.Hide();
                        formAlarm.Close();
                        formAlarm.Dispose();
                        formAlarm = null;
                    }
                }
                catch { }

                try
                {
                    _prewarmTimer?.Stop();
                    _prewarmTimer?.Dispose();
                    _prewarmTimer = null;
                    _prewarmQueue = null;
                }
                catch { }

                try
                {
                    if (EquipmentLocator.TryGet(out var eq))
                    {
                        var stopTask = eq.StopAllUnitsAsync();
                        var finished = await Task.WhenAny(stopTask, Task.Delay(8000));
                        if (finished != stopTask)
                            Console.WriteLine("StopAllUnitsAsync timeout - force disposing");
                        eq.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Shutdown error: " + ex.Message);
                }
            }
            finally
            {
                this.Cursor = Cursors.Default;

                //Process.GetCurrentProcess().Kill();
            }
        }
        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            // 추가로 해제할 핸들/매니저가 있으면 여기에서
            // 예) 글로벌 싱글톤 로그 Flush 등
            try { EquipmentLocator.ShutdownAndDisposeSafely(); } catch { }
        }

        private void AlarmManager_PostAlarm(AlarmInfo alarm)
        {
            BeginInvoke(new Action(() =>
            {
                // 1. 알람 리스트 갱신
                this.formAlarm.Alarms = AlarmManager.Instance.Alarms;

                // 2. 내용 강제 갱신
                this.formAlarm.RefreshAlarmView();

                // 3. 알람 다이얼로그는 열지 않고 알람만 발생 시킨다.
                this.ShowAlarmForm(formAlarm);
            }));
        }

        public void HideShowAlarm()
        {
            //  2025. 02. 04.  SCH : 새로운 Alarm Form 추가
            if (this.formAlarm.Visible)
            {
                this.formAlarm.Hide();
                this.formAlarm.Visible = false;
            }
            else
            {
                this.ShowAlarmForm(formAlarm);
            }
        }

        public void ShowAlarmForm(Form form)
        {
            Thread.Sleep(100);                      //  창이 너무 빨리 떠서 알람 코드가 안보이나?
            form.ShowDialog();
        }

        private static void EnableDoubleBuffer(Panel panel)
        {
            try
            {
                var pi = panel.GetType().GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
                pi?.SetValue(panel, true, null);
            }
            catch { }
            //try
            //{
            //    var doubleBufferPropertyInfo = panel.GetType().GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            //    if (doubleBufferPropertyInfo != null)
            //    {
            //        doubleBufferPropertyInfo.SetValue(panel, true, null);
            //    }
            //}
            //catch { }
        }

        private void StartPrewarm()
        {
            // 미리 로드할 대상 큐 구성 (현재 표시 중인 카테고리 제외)
            _prewarmQueue = new Queue<MenuButtonType>();
            foreach (MenuButtonType type in Enum.GetValues(typeof(MenuButtonType)))
            {
                // 이미 캐시되어 있으면 스킵
                if (_centerViewCache.ContainsKey(type)) continue;
                _prewarmQueue.Enqueue(type);
            }

            if (_prewarmQueue.Count == 0) return;

            _prewarmTimer = new Timer { Interval = 80 }; // 짧은 간격으로 한 개씩 준비
            _prewarmTimer.Tick += (s, e) =>
            {
                if (_prewarmQueue.Count == 0)
                {
                    _prewarmTimer.Stop();
                    _prewarmTimer.Dispose();
                    _prewarmTimer = null;
                    return;
                }

                var next = _prewarmQueue.Dequeue();
                try
                {
                    // 생성 및 캐시 저장
                    var view = GetOrCreateView(next, () => NavigationService.Instance.CreateCenterControl(next), next + "View");

                    // 탭 호스트면 첫 탭 미리 로드
                    var tabHost = view as TabbedViewHost;
                    if (tabHost != null)
                    {
                        tabHost.WarmUp();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Prewarm failed for {next}: {ex.Message}");
                }
            };
            _prewarmTimer.Start();
        }

        private void FormBottom_MenuButtonClicked(MenuButtonType menuType)
        {
            SwitchCenterView(menuType);
        }

        private void SwitchCenterView(MenuButtonType menuType)
        {
            // 현재 표시된 뷰 숨기기 및 컨테이너에서 제거
            if (currentCenterView != null)
            {
                centerPanel.SuspendLayout();
                try
                {
                    currentCenterView.Hide();
                    if (centerPanel != null && centerPanel.Controls.Contains(currentCenterView))
                    {
                        centerPanel.Controls.Remove(currentCenterView);
                    }
                }
                finally
                {
                    centerPanel.ResumeLayout();
                }
            }

            // 메뉴 타입에 따라 적절한 뷰 표시
            try
            {
                // 중앙 뷰 생성 또는 캐시에서 가져오기
                Control centerView = GetOrCreateView(menuType, () => NavigationService.Instance.CreateCenterControl(menuType), menuType + "View");
                SetupCenterView(centerView);
            }
            catch (Exception ex)
            {
                ShowNotImplementedMessage(menuType.ToString(), ex.Message);
            }

            // 선택된 뷰 표시
            if (currentCenterView != null)
            {
                centerPanel.SuspendLayout();
                try
                {
                    if (!centerPanel.Controls.Contains(currentCenterView))
                    {
                        centerPanel.Controls.Add(currentCenterView);
                    }
                    currentCenterView.Visible = true;
                    currentCenterView.BringToFront();
                }
                finally
                {
                    centerPanel.ResumeLayout();
                }

                // 사이즈 적용
                ApplySizes();

                // 🔧 뷰가 표시된 후 추가로 크기 확인
                Console.WriteLine($"📏 최종 표시된 뷰: {currentCenterView.GetType().Name}, Size={currentCenterView.Size}, Visible={currentCenterView.Visible}");
            }
        }

        private Control GetOrCreateView(MenuButtonType type, Func<Control> factory, string name)
        {
            Control view;
            if (!_centerViewCache.TryGetValue(type, out view) || view == null || view.IsDisposed)
            {
                view = factory();
                view.Dock = DockStyle.Fill;
                view.Name = name;
                _centerViewCache[type] = view;
            }
            return view;
        }

        private void SetupCenterView(Control view)
        {
            // 컨테이너 비우고 뷰 추가
            centerPanel.Controls.Clear();
            centerPanel.Controls.Add(view);
            currentCenterView = view;
            if (!view.Visible) view.Show();
        }

        private void ApplySizes()
        {
            if (tableLayoutPanelFormMain == null) return;

            int[] rowHeights = tableLayoutPanelFormMain.GetRowHeights();
            int[] colWidths = tableLayoutPanelFormMain.GetColumnWidths();
            if (rowHeights.Length < 3 || colWidths.Length < 1) return;

            int width = colWidths[0];
            try
            {
                // Top/Bottom 고정 반영
                formTop?.SetPanelSize(width, rowHeights[0]);
                formBottom?.SetPanelSize(width, rowHeights[2]);

                // Center 컨텐츠 반영 (IResizable 우선, 없으면 리플렉션)
                if (currentCenterView != null)
                {
                    // 가능하면 실제 centerPanel의 클라이언트 크기를 사용
                    int cWidth = (centerPanel != null) ? centerPanel.ClientSize.Width : width;
                    int cHeight = (centerPanel != null) ? centerPanel.ClientSize.Height : rowHeights[1];
                    if (cWidth <= 0) cWidth = width;
                    if (cHeight <= 0) cHeight = rowHeights[1];

                    var resizable = currentCenterView as IResizable;
                    if (resizable != null)
                    {
                        resizable.SetPanelSize(cWidth, cHeight);
                    }
                    else
                    {
                        TrySetPanelSize(currentCenterView, cWidth, cHeight);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Size apply failed: {ex.Message}");
            }
        }

        private static void TrySetPanelSize(Control control, int width, int height)
        {
            try
            {
                var mi = control.GetType().GetMethod(
                    "SetPanelSize",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null,
                    new Type[] { typeof(int), typeof(int) },
                    null);

                if (mi != null)
                {
                    mi.Invoke(control, new object[] { width, height });
                }
            }
            catch
            {
                // 무시
            }
        }

        private void ShowNotImplementedMessage(string menuName, string additionalInfo = null)
        {
            string message = $"'{menuName}' 메뉴는 아직 구현되지 않았습니다.";
            if (!string.IsNullOrEmpty(additionalInfo))
            {
                message += $"\n\n{additionalInfo}";
            }
            message += $"\n\nFormManager{menuName}.Instance.Register{menuName}Form()을 사용하여 폼을 등록하세요.";

            MessageBox.Show(message, "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // 기본 동작: 중앙 컨텐츠 비우기
            centerPanel.Controls.Clear();
            currentCenterView = null;
        }

        private void InitializeFormManagers()
        {
            try
            {
                Console.WriteLine("🚀 FormManager 시스템 초기화 시작");

                // 모든 FormManager 타입의 자동 등록 실행
                FormManagerConfig.Instance.AutoRegisterUnitConfigForms();
                FormManagerMain.Instance.AutoRegisterUnitMainForms();
                FormManagerWorking.Instance.AutoRegisterUnitWorkingForms();
                FormManagerRecipe.Instance.AutoRegisterUnitRecipeForms();
                FormManagerSetup.Instance.AutoRegisterUnitSetupForms();
                FormManagerLog.Instance.AutoRegisterUnitLogForms();

                Console.WriteLine("✅ FormManager 시스템 초기화 완료");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ FormManager 초기화 오류: {ex.Message}");
                MessageBox.Show($"FormManager 초기화 중 오류 발생: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
