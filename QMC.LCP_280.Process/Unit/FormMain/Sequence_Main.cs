using QMC.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel; // DesignMode 판단
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using static QMC.Common.Unit.BaseUnit;

namespace QMC.LCP_280.Process
{
    [FormOrder(3)]
    public partial class Sequence_Main : Form
    {
        private Equipment equipment;
        private System.Windows.Forms.Timer statusUpdateTimer;
        private bool _unitColumnsAutosized = false;
        private bool _listViewInitChecked = false;
        private int _listViewLastItemCount = 0;

        // 추가: 동시 재진입 방지 및 UI 잠금
        private readonly object _unitStatusUiLock = new object();
        private int _unitStatusRebuildInProgress;

        public Sequence_Main()
        {
            InitializeComponent();
            if (IsInDesignMode()) return;
            InitializeEquipment();
            WireEvents();
            InitializeTimer();
            UpdateUnitStatus();
            UpdateUnitComboBox();
        }

        private bool IsInDesignMode() => LicenseManager.UsageMode == LicenseUsageMode.Designtime || (Site?.DesignMode ?? false);

        private void InitializeEquipment()
        {
            try
            {
                equipment = Equipment.Instance;
                equipment.StateChanged += Equipment_StateChanged;
                equipment.UnitStateChanged += Equipment_UnitStateChanged;
                equipment.ErrorOccurred += Equipment_ErrorOccurred;
                Log.Write("LCP-280", "Equipment 초기화 완료");
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                MessageBox.Show($"Equipment 초기화 중 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void WireEvents()
        {
            if (btnStartAll != null) btnStartAll.Click += BtnStartAll_Click;
            if (btnStopAll != null) btnStopAll.Click += BtnStopAll_Click;
            if (btnSaveAllConfigs != null) btnSaveAllConfigs.Click += BtnSaveAllConfigs_Click;
            if (btnLoadAllConfigs != null) btnLoadAllConfigs.Click += BtnLoadAllConfigs_Click;
            if (btnSaveAllRecipes != null) btnSaveAllRecipes.Click += BtnSaveAllRecipes_Click;
            if (btnLoadAllRecipes != null) btnLoadAllRecipes.Click += BtnLoadAllRecipes_Click;

            if (btnStartUnit != null) btnStartUnit.Click += BtnStartUnit_Click;
            if (btnStopUnit != null) btnStopUnit.Click += BtnStopUnit_Click;
        }

        private void InitializeTimer()
        {
            statusUpdateTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            statusUpdateTimer.Tick += StatusUpdateTimer_Tick;
            statusUpdateTimer.Start();
        }

        #region ListView Setup
        private void EnsureListViewConfigured()
        {
            if (lstUnitStatus == null) 
                return;

            if (_listViewInitChecked) 
                return;

            lstUnitStatus.Visible = true;
            lstUnitStatus.Enabled = true;
            lstUnitStatus.View = View.Details;
            lstUnitStatus.FullRowSelect = true;
            lstUnitStatus.GridLines = true;
            lstUnitStatus.HideSelection = false;
            lstUnitStatus.MultiSelect = false;
            lstUnitStatus.VirtualMode = false;
            if (lstUnitStatus.Columns.Count == 0)
            {
                lstUnitStatus.Columns.Add("Unit", 120);
                lstUnitStatus.Columns.Add("State", 80);
                lstUnitStatus.Columns.Add("Comp", 60);
                lstUnitStatus.Columns.Add("RunTime", 90);
            }
            _unitColumnsAutosized = false;
            _listViewInitChecked = true;
        }
        #endregion

        #region Update Logic
        private void UpdateUnitStatus()
        {
            if (IsInDesignMode()) 
                return;

            if (lstUnitStatus == null || equipment == null) 
                return;

            if (InvokeRequired) 
            { 
                BeginInvoke(new Action(UpdateUnitStatus)); 
                return; 
            }

            EnsureListViewConfigured();

            try
            {
                var statuses = equipment.GetAllUnitStatus();
                bool rebuild = lstUnitStatus.Items.Count != statuses.Count;
                if (rebuild)
                {
                    // 재진입(타이머/이벤트 동시) 방지
                    if (Interlocked.Exchange(ref _unitStatusRebuildInProgress, 1) == 1)
                        return;

                    try
                    {
                        if (lstUnitStatus.IsDisposed) return;
                        if (!lstUnitStatus.IsHandleCreated) return;

                        // 스냅샷 → 버퍼 생성
                        var buffer = new List<ListViewItem>(statuses.Count);
                        foreach (var kv in statuses)
                        {
                            var s = kv.Value;
                            if (s == null) continue;
                            try
                            {
                                buffer.Add(CreateListViewItemFromStatus(s));
                            }
                            catch (Exception exItem)
                            {
                                LogMessage($"UnitItem 생성 오류({s?.UnitName}): {exItem.Message}");
                            }
                        }

                        // UI 크리티컬 섹션
                        lock (_unitStatusUiLock)
                        {
                            if (lstUnitStatus.IsDisposed) return;
                            lstUnitStatus.BeginUpdate();
                            try
                            {
                                lstUnitStatus.Items.Clear();
                                if (buffer.Count > 0)
                                    lstUnitStatus.Items.AddRange(buffer.ToArray());
                            }
                            finally
                            {
                                lstUnitStatus.EndUpdate();
                            }
                        }
                    }
                    finally
                    {
                        Interlocked.Exchange(ref _unitStatusRebuildInProgress, 0);
                    }
                }
                else
                {
                    // 기존 행 업데이트(재구성 아님)
                    lock (_unitStatusUiLock)
                    {
                        foreach (ListViewItem item in lstUnitStatus.Items)
                        {
                            var unitName = item.SubItems[0].Text;
                            if (!statuses.TryGetValue(unitName, out var st) || st == null) continue;
                            try
                            {
                                UpdateListViewItem(item, st);
                            }
                            catch (Exception exUpd)
                            {
                                LogMessage($"UnitItem 업데이트 오류({unitName}): {exUpd.Message}");
                            }
                        }
                    }
                }
                _listViewLastItemCount = lstUnitStatus.Items.Count;
                if (!_unitColumnsAutosized && lstUnitStatus.Items.Count > 0)
                {
                    for (int i = 0; i < lstUnitStatus.Columns.Count; i++)
                        lstUnitStatus.AutoResizeColumn(i, ColumnHeaderAutoResizeStyle.HeaderSize);
                    _unitColumnsAutosized = true;
                }
                lstUnitStatus.Invalidate();
            }
            catch (Exception ex) { LogMessage($"Unit 상태 업데이트 오류: {ex.Message}"); }
        }

        private ListViewItem CreateListViewItemFromStatus(UnitStatusInfo s)
        {
            var item = new ListViewItem(s.UnitName ?? "-");
            item.SubItems.Add(s.RunUnitStatus.ToString());
            item.SubItems.Add(s.ComponentCount.ToString());
            item.SubItems.Add(FormatRuntime(s));
            ApplyStateColor(item, s.RunUnitStatus);
            return item;
        }

        private void UpdateListViewItem(ListViewItem item, UnitStatusInfo s)
        {
            var st = s.RunUnitStatus.ToString(); 
            if (item.SubItems[1].Text != st) 
                item.SubItems[1].Text = st;

            var comp = s.ComponentCount.ToString(); 
            if (item.SubItems[2].Text != comp) 
                item.SubItems[2].Text = comp;

            var rt = FormatRuntime(s); 

            if (item.SubItems[3].Text != rt) 
                item.SubItems[3].Text = rt;

            ApplyStateColor(item, s.RunUnitStatus);
        }

        private string FormatRuntime(UnitStatusInfo s) { try { return s.RunningTime.ToString(@"hh\:mm\:ss"); } catch { return "00:00:00"; } }

        private void ApplyStateColor(ListViewItem item, UnitStatus state)
        {
            switch (state)
            {
                case UnitStatus.AutoRunning: item.BackColor = Color.LightGreen; break;
                case UnitStatus.Error: item.BackColor = Color.LightCoral; break;
                case UnitStatus.Starting:
                case UnitStatus.Stopping: item.BackColor = Color.LightYellow; break;
                default: item.BackColor = Color.White; break;
            }
        }
        #endregion

        #region Equipment Event Handlers

        private int _eqStateUpdatePending;              // 0 or 1 (동시 업데이트 합치기)
        private EquipmentStateChangedEventArgs _lastEqStateArgs; // 마지막 전달된 인자 (Coalesce 용)

        private void Equipment_StateChanged(object sender, EquipmentStateChangedEventArgs e)
        {
            if (IsDisposed || Disposing) 
                return;

            // 가장 최근 이벤트 저장 (참조만)
            _lastEqStateArgs = e;

            // 이미 UI 업데이트 예약되어 있으면 또 예약하지 않음
            if (Interlocked.Exchange(ref _eqStateUpdatePending, 1) == 1)
                return;

            //// UI 스레드 마샬링 (비동기)
            //if (InvokeRequired) 
            //{ 
            //    Invoke(new Action(() => Equipment_StateChanged(sender, e))); 
            //    return; 
            //}
            // UI 스레드 마샬링 (비동기)
            if (InvokeRequired)
            {
                try
                {
                    BeginInvoke(new Action(ApplyEquipmentStateCoalesced));
                }
                catch { Interlocked.Exchange(ref _eqStateUpdatePending, 0); }
            }
            else
            {
                ApplyEquipmentStateCoalesced();
            }
            //}

            //if (lblEquipmentState != null) 
            //{ 
            //    lblEquipmentState.Text = $"State: {e.NewState}"; 
            //    switch (e.NewState) 
            //    { 
            //        case EquipmentState.Running: lblEquipmentState.ForeColor = Color.Green; break; 
            //        case EquipmentState.Error: lblEquipmentState.ForeColor = Color.Red; break; 
            //        case EquipmentState.Starting: 
            //        case EquipmentState.Stopping: lblEquipmentState.ForeColor = Color.Orange; break; 
            //        default: lblEquipmentState.ForeColor = Color.Black; break; 
            //    }
            //} 

            //LogMessage($"Equipment 상태 변경: {e.OldState} → {e.NewState}"); 
            //UpdateUnitStatus(); 
        }

            // 실제 UI 갱신 (합쳐진 마지막 상태 1회 반영)
        private void ApplyEquipmentStateCoalesced()
        {
            // 플래그 리셋
            Interlocked.Exchange(ref _eqStateUpdatePending, 0);

            if (IsDisposed || Disposing) return;

            var args = _lastEqStateArgs;
            if (args == null) return;

            try
            {
                if (lblEquipmentState != null)
                {
                    lblEquipmentState.Text = $"State: {args.NewState}";
                    switch (args.NewState)
                    {
                        case EquipmentState.AutoRunning:
                            lblEquipmentState.ForeColor = Color.Green; break;
                        case EquipmentState.Error:
                            lblEquipmentState.ForeColor = Color.Red; break;
                        case EquipmentState.Starting:
                        case EquipmentState.Stopping:
                            lblEquipmentState.ForeColor = Color.Orange; break;
                        default:
                            lblEquipmentState.ForeColor = Color.Black; break;
                    }
                }

                LogMessage($"Equipment 상태 변경: {args.OldState} → {args.NewState}");

                // 전체 갱신 비용이 크다면 필요 시 지연/Throttle 가능
                UpdateUnitStatus();
            }
            catch (Exception ex)
            {
                LogMessage($"Equipment 상태 UI 반영 오류: {ex.Message}", Color.Red);
            }
        }

        private void Equipment_UnitStateChanged(object sender, UnitStateChangedEventArgs e) 
        { 
            if (InvokeRequired) 
            { 
                Invoke(new Action(() => Equipment_UnitStateChanged(sender, e))); 
                return; 
            } 
            LogMessage($"Unit '{e.UnitName}' 상태 변경: {e.RunUnitStatus}");

            // 빠른 단일 업데이트
            UpdateSingleUnitRow(e.UnitName, e.RunUnitStatus);

            //UpdateUnitStatus();
        }
        private void UpdateSingleUnitRow(string unitName, UnitStatus newState)
        {
            if (lstUnitStatus == null) return;
            foreach (ListViewItem item in lstUnitStatus.Items)
            {
                if (item.SubItems[0].Text.Equals(unitName, StringComparison.OrdinalIgnoreCase))
                {
                    item.SubItems[1].Text = newState.ToString();
                    ApplyStateColor(item, newState);
                    return;
                }
            }
        }

        private void Equipment_ErrorOccurred(object sender, EquipmentErrorEventArgs e) { if (InvokeRequired) { Invoke(new Action(() => Equipment_ErrorOccurred(sender, e))); return; } LogMessage($"Equipment 오류: {e.ErrorMessage}", Color.Red); }
        private void StatusUpdateTimer_Tick(object sender, EventArgs e) { UpdateUnitStatus(); UpdateUnitComboBox(); }
        #endregion

        #region Buttons
        private async void BtnStartAll_Click(object sender, EventArgs e)
        {
            if (equipment == null) 
                return;

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

        private async void BtnStartUnit_Click(object sender, EventArgs e)
        {
            if (cmbUnits?.SelectedItem == null || equipment == null) return;
            var unitName = cmbUnits.SelectedItem.ToString();
            try
            {
                LogMessage($"Unit '{unitName}' 시작 중...");
                var result = await equipment.StartUnitAsync(unitName);
                LogMessage(result ? $"Unit '{unitName}' 시작 완료" : $"Unit '{unitName}' 시작 실패");
            }
            catch (Exception ex) 
            {
                Log.Write(ex);
                LogMessage($"Unit '{unitName}' 시작 오류: {ex.Message}"); 
            }
            finally 
            { UpdateUnitStatus(); }
        }

        private async void BtnStopAll_Click(object sender, EventArgs e)
        { 
            try 
            { 
                btnStopAll.Enabled = false; 
                LogMessage("설비 전체 정지 중..."); 
                var result = await equipment.StopAllUnitsAsync(); 
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
        private void BtnSaveAllConfigs_Click(object sender, EventArgs e) { try { var result = equipment.SaveAllConfigs(); LogMessage(result ? "모든 Config 저장 완료" : "Config 저장 실패"); } catch (Exception ex) { LogMessage($"Config 저장 오류: {ex.Message}"); } }
        private void BtnLoadAllConfigs_Click(object sender, EventArgs e) { try { var result = equipment.LoadAllConfigs(); LogMessage(result ? "모든 Config 로드 완료" : "Config 로드 실패"); } catch (Exception ex) { LogMessage($"Config 로드 오류: {ex.Message}"); } }
        private void BtnSaveAllRecipes_Click(object sender, EventArgs e) 
        { 
            try 
            { 
                //var result = equipment.SaveAllRecipes(); 
                //LogMessage(result ? "모든 Recipe 저장 완료" : "Recipe 저장 실패"); 
            } 
            catch (Exception ex) 
            { LogMessage($"Recipe 저장 오류: {ex.Message}"); } 
        }

        private void BtnLoadAllRecipes_Click(object sender, EventArgs e) 
        { 
            try 
            { 
                //var result = equipment.LoadAllRecipes(); 
                //LogMessage(result ? "모든 Recipe 로드 완료" : "Recipe 로드 실패"); 
            } 
            catch (Exception ex) 
            { LogMessage($"Recipe 로드 오류: {ex.Message}"); } }

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
        #endregion

        #region Combo & Logging
        private void UpdateUnitComboBox()
        {
            if (IsInDesignMode()) return;
            if (cmbUnits == null || equipment == null) return;
            if (InvokeRequired) { BeginInvoke(new Action(UpdateUnitComboBox)); return; }
            try
            {
                var selected = cmbUnits.SelectedItem?.ToString();
                var names = equipment.GetRegisteredUnitNames();
                cmbUnits.BeginUpdate();
                cmbUnits.Items.Clear();
                foreach (var n in names) cmbUnits.Items.Add(n);
                if (!string.IsNullOrEmpty(selected) && cmbUnits.Items.Contains(selected)) cmbUnits.SelectedItem = selected; else if (cmbUnits.Items.Count > 0) cmbUnits.SelectedIndex = 0;
                cmbUnits.EndUpdate();
            }
            catch (Exception ex) { LogMessage($"Unit 목록 업데이트 오류: {ex.Message}"); }
        }

        private void LogMessage(string message, Color? color = null)
        {
            if (InvokeRequired) { Invoke(new Action(() => LogMessage(message, color))); return; }
            if (rtbLog == null) return;
            var ts = DateTime.Now.ToString("HH:mm:ss.fff");
            var line = $"[{ts}] {message}";
            rtbLog.SelectionStart = rtbLog.TextLength;
            rtbLog.SelectionLength = 0;
            rtbLog.SelectionColor = color ?? Color.LimeGreen;
            rtbLog.AppendText(line + Environment.NewLine);
            rtbLog.SelectionColor = rtbLog.ForeColor;
            rtbLog.ScrollToCaret();
            if (rtbLog.Lines.Length > 1000)
            { var lines = rtbLog.Lines; var newLines = new string[800]; Array.Copy(lines, 200, newLines, 0, 800); rtbLog.Lines = newLines; }
        } 
        #endregion

        #region Form Closing
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (IsInDesignMode()) { base.OnFormClosing(e); return; }
            try
            {
                statusUpdateTimer?.Stop();
                statusUpdateTimer?.Dispose();
                if (equipment != null && equipment.EqState == EquipmentState.AutoRunning)
                {
                    var result = MessageBox.Show("설비가 실행 중입니다. 정지하고 종료하시겠습니까?", "확인", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes) equipment.StopAllUnitsAsync().GetAwaiter().GetResult(); else { e.Cancel = true; return; }
                }
                if (equipment != null)
                {
                    equipment.StateChanged -= Equipment_StateChanged;
                    equipment.UnitStateChanged -= Equipment_UnitStateChanged;
                    equipment.ErrorOccurred -= Equipment_ErrorOccurred;
                }
            }
            catch (Exception ex) { LogMessage($"폼 종료 중 오류: {ex.Message}"); }
            base.OnFormClosing(e);
        }


        #endregion

        private void Sequence_Main_Load(object sender, EventArgs e)
        {

        }
    }
}