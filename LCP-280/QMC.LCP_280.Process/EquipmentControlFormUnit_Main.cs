using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Linq;
using QMC.Common;

namespace QMC.LCP_280.Process
{
    /// <summary>
    /// Equipment 제어 및 모니터링 폼
    /// </summary>
    public partial class EquipmentControlFormUnit_Main : Form
    {
        private Equipment equipment;
        private Timer statusUpdateTimer;
        
        // UI Controls
        private Button btnStartAll;
        private Button btnStopAll;
        private Button btnSaveAllConfigs;
        private Button btnLoadAllConfigs;
        private Button btnSaveAllRecipes;
        private Button btnLoadAllRecipes;
        private Label lblEquipmentState;
        private ListView lstUnitStatus;
        private RichTextBox rtbLog;
        private ComboBox cmbUnits;
        private Button btnStartUnit;
        private Button btnStopUnit;

        public EquipmentControlFormUnit_Main()
        {
            InitializeEquipment();
            InitializeComponent();
            InitializeUI();
            InitializeTimer();
        }

        private void InitializeComponent()
        {
            this.Text = "Equipment Control Panel - LCP-280";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(800, 600);
        }

        private void InitializeEquipment()
        {
            try
            {
                equipment = Equipment.Instance;
                
                // 이벤트 구독
                equipment.StateChanged += Equipment_StateChanged;
                equipment.UnitStateChanged += Equipment_UnitStateChanged;
                equipment.ErrorOccurred += Equipment_ErrorOccurred;

                Log.Write("LCP-280", "Equipment 초기화 완료");
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                MessageBox.Show($"Equipment 초기화 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeUI()
        {
            this.SuspendLayout();

            // 메인 패널
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 4,
                Margin = new Padding(10)
            };

            // 열 스타일
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            // 행 스타일
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 80F));  // 설비 제어
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));  // Unit 제어
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 60F));   // 상태 표시
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));   // 로그

            // 설비 제어 패널
            var equipmentControlPanel = CreateEquipmentControlPanel();
            mainPanel.Controls.Add(equipmentControlPanel, 0, 0);
            mainPanel.SetColumnSpan(equipmentControlPanel, 2);

            // Unit 제어 패널
            var unitControlPanel = CreateUnitControlPanel();
            mainPanel.Controls.Add(unitControlPanel, 0, 1);
            mainPanel.SetColumnSpan(unitControlPanel, 2);

            // 상태 패널
            var statusPanel = CreateStatusPanel();
            mainPanel.Controls.Add(statusPanel, 0, 2);

            // Unit 목록 패널
            var unitListPanel = CreateUnitListPanel();
            mainPanel.Controls.Add(unitListPanel, 1, 2);

            // 로그 패널
            var logPanel = CreateLogPanel();
            mainPanel.Controls.Add(logPanel, 0, 3);
            mainPanel.SetColumnSpan(logPanel, 2);

            this.Controls.Add(mainPanel);
            this.ResumeLayout(true);
        }

        private GroupBox CreateEquipmentControlPanel()
        {
            var panel = new GroupBox
            {
                Text = "Equipment Control",
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            var tableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 6,
                RowCount = 1
            };

            // 버튼들 생성
            btnStartAll = new Button
            {
                Text = "Start All",
                BackColor = Color.LightGreen,
                Dock = DockStyle.Fill,
                Font = new Font("맑은 고딕", 9F, FontStyle.Bold)
            };
            btnStartAll.Click += BtnStartAll_Click;

            btnStopAll = new Button
            {
                Text = "Stop All",
                BackColor = Color.LightCoral,
                Dock = DockStyle.Fill,
                Font = new Font("맑은 고딕", 9F, FontStyle.Bold)
            };
            btnStopAll.Click += BtnStopAll_Click;

            btnSaveAllConfigs = new Button
            {
                Text = "Save Configs",
                BackColor = Color.LightBlue,
                Dock = DockStyle.Fill
            };
            btnSaveAllConfigs.Click += BtnSaveAllConfigs_Click;

            btnLoadAllConfigs = new Button
            {
                Text = "Load Configs",
                BackColor = Color.LightYellow,
                Dock = DockStyle.Fill
            };
            btnLoadAllConfigs.Click += BtnLoadAllConfigs_Click;

            btnSaveAllRecipes = new Button
            {
                Text = "Save Recipes",
                BackColor = Color.LightCyan,
                Dock = DockStyle.Fill
            };
            btnSaveAllRecipes.Click += BtnSaveAllRecipes_Click;

            btnLoadAllRecipes = new Button
            {
                Text = "Load Recipes",
                BackColor = Color.LightPink,
                Dock = DockStyle.Fill
            };
            btnLoadAllRecipes.Click += BtnLoadAllRecipes_Click;

            // 열 스타일 설정
            for (int i = 0; i < 6; i++)
            {
                tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.67F));
            }

            tableLayout.Controls.Add(btnStartAll, 0, 0);
            tableLayout.Controls.Add(btnStopAll, 1, 0);
            tableLayout.Controls.Add(btnSaveAllConfigs, 2, 0);
            tableLayout.Controls.Add(btnLoadAllConfigs, 3, 0);
            tableLayout.Controls.Add(btnSaveAllRecipes, 4, 0);
            tableLayout.Controls.Add(btnLoadAllRecipes, 5, 0);

            panel.Controls.Add(tableLayout);
            return panel;
        }

        private GroupBox CreateUnitControlPanel()
        {
            var panel = new GroupBox
            {
                Text = "Individual Unit Control",
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            var tableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1
            };

            // Unit 선택 콤보박스
            cmbUnits = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            btnStartUnit = new Button
            {
                Text = "Start Unit",
                BackColor = Color.PaleGreen,
                Dock = DockStyle.Fill
            };
            btnStartUnit.Click += BtnStartUnit_Click;

            btnStopUnit = new Button
            {
                Text = "Stop Unit",
                BackColor = Color.PaleVioletRed,
                Dock = DockStyle.Fill
            };
            btnStopUnit.Click += BtnStopUnit_Click;

            var lblUnit = new Label
            {
                Text = "Select Unit:",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            // 열 스타일 설정
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));

            tableLayout.Controls.Add(lblUnit, 0, 0);
            tableLayout.Controls.Add(cmbUnits, 1, 0);
            tableLayout.Controls.Add(btnStartUnit, 2, 0);
            tableLayout.Controls.Add(btnStopUnit, 3, 0);

            panel.Controls.Add(tableLayout);
            return panel;
        }

        private GroupBox CreateStatusPanel()
        {
            var panel = new GroupBox
            {
                Text = "Equipment Status",
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            lblEquipmentState = new Label
            {
                Text = "State: Ready",
                Dock = DockStyle.Top,
                Font = new Font("맑은 고딕", 12F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Height = 30
            };

            var equipmentInfo = new Label
            {
                Text = "Equipment: LCP-280\nManufacturer: QMC\nRegistered Units: 0",
                Dock = DockStyle.Fill,
                Font = new Font("맑은 고딕", 9F),
                TextAlign = ContentAlignment.TopLeft
            };

            panel.Controls.Add(equipmentInfo);
            panel.Controls.Add(lblEquipmentState);
            return panel;
        }

        private GroupBox CreateUnitListPanel()
        {
            var panel = new GroupBox
            {
                Text = "Unit Status",
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            lstUnitStatus = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Consolas", 9F)
            };

            // 컬럼 추가
            lstUnitStatus.Columns.Add("Unit Name", 120);
            lstUnitStatus.Columns.Add("State", 80);
            lstUnitStatus.Columns.Add("Components", 80);
            lstUnitStatus.Columns.Add("Running Time", 100);

            panel.Controls.Add(lstUnitStatus);
            return panel;
        }

        private GroupBox CreateLogPanel()
        {
            var panel = new GroupBox
            {
                Text = "Operation Log",
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            rtbLog = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 9F),
                BackColor = Color.Black,
                ForeColor = Color.LimeGreen,
                ReadOnly = true
            };

            panel.Controls.Add(rtbLog);
            return panel;
        }

        private void InitializeTimer()
        {
            statusUpdateTimer = new Timer
            {
                Interval = 1000 // 1초마다 업데이트
            };
            statusUpdateTimer.Tick += StatusUpdateTimer_Tick;
            statusUpdateTimer.Start();
        }

        #region Event Handlers

        private async void BtnStartAll_Click(object sender, EventArgs e)
        {
            try
            {
                btnStartAll.Enabled = false;
                LogMessage("설비 전체 시작 중...");
                
                var result = await equipment.StartAllUnitsAsync();
                
                if (result)
                {
                    LogMessage("설비 전체 시작 완료");
                }
                else
                {
                    LogMessage("설비 전체 시작 실패");
                }
            }
            catch (Exception ex)
            {
                LogMessage($"설비 시작 오류: {ex.Message}");
            }
            finally
            {
                btnStartAll.Enabled = true;
            }
        }

        private async void BtnStopAll_Click(object sender, EventArgs e)
        {
            try
            {
                btnStopAll.Enabled = false;
                LogMessage("설비 전체 정지 중...");
                
                var result = await equipment.StopAllUnitsAsync();
                
                if (result)
                {
                    LogMessage("설비 전체 정지 완료");
                }
                else
                {
                    LogMessage("설비 전체 정지 실패");
                }
            }
            catch (Exception ex)
            {
                LogMessage($"설비 정지 오류: {ex.Message}");
            }
            finally
            {
                btnStopAll.Enabled = true;
            }
        }

        private async void BtnStartUnit_Click(object sender, EventArgs e)
        {
            if (cmbUnits.SelectedItem != null)
            {
                var unitName = cmbUnits.SelectedItem.ToString();
                try
                {
                    LogMessage($"Unit '{unitName}' 시작 중...");
                    var result = await equipment.StartUnitAsync(unitName);
                    LogMessage(result ? $"Unit '{unitName}' 시작 완료" : $"Unit '{unitName}' 시작 실패");
                }
                catch (Exception ex)
                {
                    LogMessage($"Unit '{unitName}' 시작 오류: {ex.Message}");
                }
            }
        }

        private async void BtnStopUnit_Click(object sender, EventArgs e)
        {
            if (cmbUnits.SelectedItem != null)
            {
                var unitName = cmbUnits.SelectedItem.ToString();
                try
                {
                    LogMessage($"Unit '{unitName}' 정지 중...");
                    var result = await equipment.StopUnitAsync(unitName);
                    LogMessage(result ? $"Unit '{unitName}' 정지 완료" : $"Unit '{unitName}' 정지 실패");
                }
                catch (Exception ex)
                {
                    LogMessage($"Unit '{unitName}' 정지 오류: {ex.Message}");
                }
            }
        }

        private void BtnSaveAllConfigs_Click(object sender, EventArgs e)
        {
            try
            {
                var result = equipment.SaveAllConfigs();
                LogMessage(result ? "모든 Config 저장 완료" : "Config 저장 실패");
            }
            catch (Exception ex)
            {
                LogMessage($"Config 저장 오류: {ex.Message}");
            }
        }

        private void BtnLoadAllConfigs_Click(object sender, EventArgs e)
        {
            try
            {
                var result = equipment.LoadAllConfigs();
                LogMessage(result ? "모든 Config 로드 완료" : "Config 로드 실패");
            }
            catch (Exception ex)
            {
                LogMessage($"Config 로드 오류: {ex.Message}");
            }
        }

        private void BtnSaveAllRecipes_Click(object sender, EventArgs e)
        {
            try
            {
                var result = equipment.SaveAllRecipes();
                LogMessage(result ? "모든 Recipe 저장 완료" : "Recipe 저장 실패");
            }
            catch (Exception ex)
            {
                LogMessage($"Recipe 저장 오류: {ex.Message}");
            }
        }

        private void BtnLoadAllRecipes_Click(object sender, EventArgs e)
        {
            try
            {
                var result = equipment.LoadAllRecipes();
                LogMessage(result ? "모든 Recipe 로드 완료" : "Recipe 로드 실패");
            }
            catch (Exception ex)
            {
                LogMessage($"Recipe 로드 오류: {ex.Message}");
            }
        }

        private void Equipment_StateChanged(object sender, EquipmentStateChangedEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => Equipment_StateChanged(sender, e)));
                return;
            }

            lblEquipmentState.Text = $"State: {e.NewState}";
            
            // 상태에 따른 색상 변경
            switch (e.NewState)
            {
                case EquipmentState.Running:
                    lblEquipmentState.ForeColor = Color.Green;
                    break;
                case EquipmentState.Error:
                    lblEquipmentState.ForeColor = Color.Red;
                    break;
                case EquipmentState.Starting:
                case EquipmentState.Stopping:
                    lblEquipmentState.ForeColor = Color.Orange;
                    break;
                default:
                    lblEquipmentState.ForeColor = Color.Black;
                    break;
            }

            LogMessage($"Equipment 상태 변경: {e.OldState} → {e.NewState}");
        }

        private void Equipment_UnitStateChanged(object sender, UnitStateChangedEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => Equipment_UnitStateChanged(sender, e)));
                return;
            }

            LogMessage($"Unit '{e.UnitName}' 상태 변경: {e.State}");
        }

        private void Equipment_ErrorOccurred(object sender, EquipmentErrorEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => Equipment_ErrorOccurred(sender, e)));
                return;
            }

            LogMessage($"Equipment 오류: {e.ErrorMessage}", Color.Red);
        }

        private void StatusUpdateTimer_Tick(object sender, EventArgs e)
        {
            UpdateUnitStatus();
            UpdateUnitComboBox();
        }

        #endregion

        #region Helper Methods

        private void UpdateUnitStatus()
        {
            try
            {
                var unitStatuses = equipment.GetAllUnitStatus();
                
                lstUnitStatus.Items.Clear();
                foreach (var kvp in unitStatuses)
                {
                    var status = kvp.Value;
                    var item = new ListViewItem(status.UnitName);
                    item.SubItems.Add(status.State.ToString());
                    item.SubItems.Add(status.ComponentCount.ToString());
                    item.SubItems.Add(status.RunningTime.ToString(@"hh\:mm\:ss"));
                    
                    // 상태에 따른 색상 설정
                    switch (status.State)
                    {
                        case UnitState.Running:
                            item.BackColor = Color.LightGreen;
                            break;
                        case UnitState.Error:
                            item.BackColor = Color.LightCoral;
                            break;
                        case UnitState.Starting:
                        case UnitState.Stopping:
                            item.BackColor = Color.LightYellow;
                            break;
                        default:
                            item.BackColor = Color.White;
                            break;
                    }
                    
                    lstUnitStatus.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Unit 상태 업데이트 오류: {ex.Message}");
            }
        }

        private void UpdateUnitComboBox()
        {
            try
            {
                var currentSelection = cmbUnits.SelectedItem?.ToString();
                var unitNames = equipment.GetRegisteredUnitNames();
                
                cmbUnits.Items.Clear();
                foreach (var unitName in unitNames)
                {
                    cmbUnits.Items.Add(unitName);
                }
                
                // 이전 선택 복원
                if (!string.IsNullOrEmpty(currentSelection) && cmbUnits.Items.Contains(currentSelection))
                {
                    cmbUnits.SelectedItem = currentSelection;
                }
                else if (cmbUnits.Items.Count > 0)
                {
                    cmbUnits.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Unit 목록 업데이트 오류: {ex.Message}");
            }
        }

        private void LogMessage(string message, Color? color = null)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => LogMessage(message, color)));
                return;
            }

            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            var logEntry = $"[{timestamp}] {message}";
            
            rtbLog.SelectionStart = rtbLog.TextLength;
            rtbLog.SelectionLength = 0;
            rtbLog.SelectionColor = color ?? Color.LimeGreen;
            rtbLog.AppendText(logEntry + Environment.NewLine);
            rtbLog.SelectionColor = rtbLog.ForeColor;
            rtbLog.ScrollToCaret();
            
            // 로그 라인 수 제한 (성능을 위해)
            if (rtbLog.Lines.Length > 1000)
            {
                var lines = rtbLog.Lines;
                var newLines = new string[800];
                Array.Copy(lines, 200, newLines, 0, 800);
                rtbLog.Lines = newLines;
            }
        }

        #endregion

        #region Form Events

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                statusUpdateTimer?.Stop();
                statusUpdateTimer?.Dispose();
                
                // Equipment가 실행 중이면 정지 확인
                if (equipment.State == EquipmentState.Running)
                {
                    var result = MessageBox.Show(
                        "설비가 실행 중입니다. 정지하고 종료하시겠습니까?",
                        "확인",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);
                    
                    if (result == DialogResult.Yes)
                    {
                        equipment.StopAllUnitsAsync().GetAwaiter().GetResult();
                    }
                    else
                    {
                        e.Cancel = true;
                        return;
                    }
                }
                
                // 이벤트 구독 해제
                equipment.StateChanged -= Equipment_StateChanged;
                equipment.UnitStateChanged -= Equipment_UnitStateChanged;
                equipment.ErrorOccurred -= Equipment_ErrorOccurred;
            }
            catch (Exception ex)
            {
                LogMessage($"폼 종료 중 오류: {ex.Message}");
            }
            
            base.OnFormClosing(e);
        }

        #endregion
    }
}