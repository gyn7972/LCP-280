using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel; // DesignMode 판단
using QMC.Common;

namespace QMC.LCP_280.Process
{
    /// <summary>
    /// Equipment 제어 및 모니터링 폼 (로직 / 이벤트 구현 파일)
    /// </summary>
    public partial class Sequence_Main : Form
    {
        private Equipment equipment;
        private Timer statusUpdateTimer;
        private bool _unitColumnsAutosized = false; // 최초 1회 자동폭 조정 용도

        public Sequence_Main()
        {
            InitializeComponent();

            if (IsInDesignMode())
            {
                return;
            }

            InitializeEquipment();
            WireEvents();
            InitializeTimer();

            UpdateUnitStatus();
            UpdateUnitComboBox();
        }

        private bool IsInDesignMode()
        {
            return LicenseManager.UsageMode == LicenseUsageMode.Designtime
                   || (Site?.DesignMode ?? false);
        }

        private void InitializeEquipment()
        {
            try
            {
                equipment = Equipment.Instance;
                equipment.StateChanged     += Equipment_StateChanged;
                equipment.UnitStateChanged += Equipment_UnitStateChanged;
                equipment.ErrorOccurred    += Equipment_ErrorOccurred;
                Log.Write("LCP-280", "Equipment 초기화 완료");
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                MessageBox.Show(
                    $"Equipment 초기화 중 오류가 발생했습니다: {ex.Message}",
                    "오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void WireEvents()
        {
            if (btnStartAll       != null) btnStartAll.Click       += BtnStartAll_Click;
            if (btnStopAll        != null) btnStopAll.Click        += BtnStopAll_Click;
            if (btnSaveAllConfigs != null) btnSaveAllConfigs.Click += BtnSaveAllConfigs_Click;
            if (btnLoadAllConfigs != null) btnLoadAllConfigs.Click += BtnLoadAllConfigs_Click;
            if (btnSaveAllRecipes != null) btnSaveAllRecipes.Click += BtnSaveAllRecipes_Click;
            if (btnLoadAllRecipes != null) btnLoadAllRecipes.Click += BtnLoadAllRecipes_Click;
            if (btnStartUnit      != null) btnStartUnit.Click      += BtnStartUnit_Click;
            if (btnStopUnit       != null) btnStopUnit.Click       += BtnStopUnit_Click;
        }

        private void InitializeTimer()
        {
            statusUpdateTimer = new Timer();
            statusUpdateTimer.Interval = 1000;
            statusUpdateTimer.Tick += StatusUpdateTimer_Tick;
            statusUpdateTimer.Start();
        }

        #region Button Events
        private async void BtnStartAll_Click(object sender, EventArgs e)
        {
            try
            {
                btnStartAll.Enabled = false;
                LogMessage("설비 전체 시작 중...");
                var result = await equipment.StartAllUnitsAsync();
                LogMessage(result ? "설비 전체 시작 완료" : "설비 전체 시작 실패");
            }
            catch (Exception ex)
            {
                LogMessage($"설비 시작 오류: {ex.Message}");
            }
            finally
            {
                btnStartAll.Enabled = true;
                UpdateUnitStatus();
                UpdateUnitComboBox();
            }
        }

        private async void BtnStopAll_Click(object sender, EventArgs e)
        {
            try
            {
                btnStopAll.Enabled = false;
                LogMessage("설비 전체 정지 중...");
                var result = await equipment.StopAllUnitsAsync(false);
                LogMessage(result ? "설비 전체 정지 완료" : "설비 전체 정지 실패");
            }
            catch (Exception ex)
            {
                LogMessage($"설비 정지 오류: {ex.Message}");
            }
            finally
            {
                btnStopAll.Enabled = true;
                UpdateUnitStatus();
                UpdateUnitComboBox();
            }
        }

        private async void BtnStartUnit_Click(object sender, EventArgs e)
        {
            if (cmbUnits?.SelectedItem == null) return;
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
            finally
            {
                UpdateUnitStatus();
            }
        }

        private async void BtnStopUnit_Click(object sender, EventArgs e)
        {
            if (cmbUnits?.SelectedItem == null) return;
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
            finally
            {
                UpdateUnitStatus();
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
        #endregion

        #region Equipment Events
        private void Equipment_StateChanged(object sender, EquipmentStateChangedEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => Equipment_StateChanged(sender, e)));
                return;
            }
            if (lblEquipmentState != null)
            {
                lblEquipmentState.Text = $"State: {e.NewState}";
                switch (e.NewState)
                {
                    case EquipmentState.Running:  lblEquipmentState.ForeColor = Color.Green;  break;
                    case EquipmentState.Error:    lblEquipmentState.ForeColor = Color.Red;    break;
                    case EquipmentState.Starting:
                    case EquipmentState.Stopping: lblEquipmentState.ForeColor = Color.Orange; break;
                    default:                       lblEquipmentState.ForeColor = Color.Black;  break;
                }
            }
            LogMessage($"Equipment 상태 변경: {e.OldState} → {e.NewState}");
            UpdateUnitStatus();
        }

        private void Equipment_UnitStateChanged(object sender, UnitStateChangedEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => Equipment_UnitStateChanged(sender, e)));
                return;
            }
            LogMessage($"Unit '{e.UnitName}' 상태 변경: {e.State}");
            UpdateUnitStatus();
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

        #region Helper
        private void UpdateUnitStatus()
        {
            if (IsInDesignMode()) return;
            if (lstUnitStatus == null || equipment == null) return;

            if (InvokeRequired) { BeginInvoke(new Action(UpdateUnitStatus)); return; }

            try
            {
                var statuses = equipment.GetAllUnitStatus();
                // 디버그: 실제 Unit 개수 출력
                LogMessage($"[DBG] Units={statuses.Count}");

                lstUnitStatus.BeginUpdate();
                lstUnitStatus.Items.Clear();

                foreach (var kv in statuses)
                {
                    var s = kv.Value;
                    if (s == null) continue;

                    var item = new ListViewItem(s.UnitName ?? "-");
                    item.SubItems.Add(s.State.ToString());
                    item.SubItems.Add(s.ComponentCount.ToString());
                    item.SubItems.Add(s.RunningTime.ToString(@"hh\:mm\:ss"));

                    switch (s.State)
                    {
                        case UnitState.Running: item.BackColor = Color.LightGreen; break;
                        case UnitState.Error: item.BackColor = Color.LightCoral; break;
                        case UnitState.Starting:
                        case UnitState.Stopping: item.BackColor = Color.LightYellow; break;
                        default: item.BackColor = Color.White; break;
                    }
                    lstUnitStatus.Items.Add(item);
                }

                if (!_unitColumnsAutosized && lstUnitStatus.Items.Count > 0)
                {
                    for (int i = 0; i < lstUnitStatus.Columns.Count; i++)
                        lstUnitStatus.AutoResizeColumn(i, ColumnHeaderAutoResizeStyle.HeaderSize);
                    _unitColumnsAutosized = true;
                }
                lstUnitStatus.EndUpdate();

                LogMessage($"[DBG] ListView Items={lstUnitStatus.Items.Count}");
            }
            catch (Exception ex)
            {
                LogMessage($"Unit 상태 업데이트 오류: {ex.Message}");
            }
        }
        //private void UpdateUnitStatus()
        //{
        //    if (IsInDesignMode()) return;
        //    if (lstUnitStatus == null || equipment == null) return;

        //    if (InvokeRequired)
        //    {
        //        BeginInvoke(new Action(UpdateUnitStatus));
        //        return;
        //    }

        //    try
        //    {
        //        var statuses = equipment.GetAllUnitStatus();
        //        lstUnitStatus.BeginUpdate();
        //        lstUnitStatus.Items.Clear();
        //        foreach (var kv in statuses)
        //        {
        //            var s = kv.Value;
        //            var item = new ListViewItem(s.UnitName ?? "-");
        //            item.SubItems.Add(s.State.ToString());
        //            item.SubItems.Add(s.ComponentCount.ToString());
        //            item.SubItems.Add(s.RunningTime.ToString(@"hh\:mm\:ss"));
        //            switch (s.State)
        //            {
        //                case UnitState.Running:   item.BackColor = Color.LightGreen;  break;
        //                case UnitState.Error:     item.BackColor = Color.LightCoral;  break;
        //                case UnitState.Starting:
        //                case UnitState.Stopping:  item.BackColor = Color.LightYellow; break;
        //                default:                  item.BackColor = Color.White;       break;
        //            }
        //            lstUnitStatus.Items.Add(item);
        //        }
        //        if (!_unitColumnsAutosized && lstUnitStatus.Items.Count > 0)
        //        {
        //            for (int i = 0; i < lstUnitStatus.Columns.Count; i++)
        //            {
        //                lstUnitStatus.AutoResizeColumn(i, ColumnHeaderAutoResizeStyle.HeaderSize);
        //            }
        //            _unitColumnsAutosized = true;
        //        }
        //        lstUnitStatus.EndUpdate();
        //        //lstUnitStatus.Refresh();
        //    }
        //    catch (Exception ex)
        //    {
        //        LogMessage($"Unit 상태 업데이트 오류: {ex.Message}");
        //    }
        //}

        private void UpdateUnitComboBox()
        {
            if (IsInDesignMode()) return;
            if (cmbUnits == null || equipment == null) return;

            if (InvokeRequired)
            {
                BeginInvoke(new Action(UpdateUnitComboBox));
                return;
            }
            try
            {
                var selected = cmbUnits.SelectedItem?.ToString();
                var names = equipment.GetRegisteredUnitNames();
                cmbUnits.BeginUpdate();
                cmbUnits.Items.Clear();
                foreach (var n in names)
                {
                    cmbUnits.Items.Add(n);
                }
                if (!string.IsNullOrEmpty(selected) && cmbUnits.Items.Contains(selected))
                {
                    cmbUnits.SelectedItem = selected;
                }
                else if (cmbUnits.Items.Count > 0)
                {
                    cmbUnits.SelectedIndex = 0;
                }
                cmbUnits.EndUpdate();
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
            if (rtbLog == null) return;
            var ts = DateTime.Now.ToString("HH:mm:ss.fff");
            var line = $"[{ts}] {message}";
            rtbLog.SelectionStart  = rtbLog.TextLength;
            rtbLog.SelectionLength = 0;
            rtbLog.SelectionColor  = color ?? Color.LimeGreen;
            rtbLog.AppendText(line + Environment.NewLine);
            rtbLog.SelectionColor  = rtbLog.ForeColor;
            rtbLog.ScrollToCaret();
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
            if (IsInDesignMode()) { base.OnFormClosing(e); return; }
            try
            {
                statusUpdateTimer?.Stop();
                statusUpdateTimer?.Dispose();
                if (equipment != null && equipment.State == EquipmentState.Running)
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
                if (equipment != null)
                {
                    equipment.StateChanged     -= Equipment_StateChanged;
                    equipment.UnitStateChanged -= Equipment_UnitStateChanged;
                    equipment.ErrorOccurred    -= Equipment_ErrorOccurred;
                }
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