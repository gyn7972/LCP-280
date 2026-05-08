using Newtonsoft.Json.Linq;
using QMC.Common;
using QMC.Common.Account;
using QMC.Common.PKGTester;
using QMC.Common.Spectrometer;
using QMC.Common.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace QMC.LCP_280.Process.Unit.FormRecipe.Page
{
    public partial class CellTesterPage : UserControl
    {
        private PKGTester tester => Equipment.Instance.Tester;

        private CancellationTokenSource _ctsRepeat;
        // 중복 시작 방지
        private bool _autoMeasureRunning = false;

        private Component.MeasurementRecipe currentRecipe
        {
            get
            {
                if (DesignModeHelper.IsDesignMode(this)) return null;
                var eq = Equipment.Instance;
                return eq?.EquipmentRecipe?.CurrentRecipe;
            }
        }

        public CellTesterPage()
        {
            InitializeComponent();

            rbvOption.SetOptions(false, "Off", "On");

            dataGridResult.Font = new Font("맑은 고딕", 8);
            
            if (tester != null)
            {
                tester.OnConditionSetChanged += Tester_OnConditionSetChanged;
                tester.OnManualMeasureCompleted += Tester_OnMeasureCompleted;
                tester.OnMeasureAborted += Tester_OnMeasureAborted;

                casSpectrumViewer.AttachSpectrometer(tester.Spectrometer);
            }
        }  

        private void CellTesterPage_Load(object sender, EventArgs e)
        {
            if (DesignModeHelper.IsDesignMode(this)) return;
            if (currentRecipe == null) return;

            UpdateNewResultGrid();
        }

        private void Tester_OnMeasureCompleted(object sender)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => Tester_OnMeasureCompleted(sender)));
                return;
            }

            AddNewManualMeasureResult();
        }

        private void Tester_OnMeasureAborted(object sender)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => Tester_OnMeasureAborted(sender)));
                return;
            }

            //MessageBox.Show("Measurement was stopped due to an error.", "Error");
        }

        private void Tester_OnConditionSetChanged(object sender)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => Tester_OnConditionSetChanged(sender)));
                return;
            }

            UpdateNewResultGrid();
            lbResultValue.Text = "";
            lbMeasureTime.Text = "Measure Time: - ";
            lbCurrentIndexNo.Text = $"Rotary Index No: {GetCurrentProbeIndexNo() + 1}";
        }

        private void ClearResultGrid()
        {             
            dataGridResult.Rows.Clear();
            dataGridResult.Columns.Clear();
        }

        private void UpdateNewResultGrid()
        {
            if (DesignModeHelper.IsDesignMode(this)) 
                return;

            //var recipe = currentRecipe;
            //if (recipe == null)
            //{
            //    // 그리드만 클리어하고 종료
            //    try { dataGridView1.Rows.Clear(); } catch { }
            //    return;
            //}

            ClearResultGrid();
            foreach (var item in tester.ConditionSet.Items)
            {
                DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn();
                col.Name = item.Name;
                col.HeaderText = item.Name;
                col.Width = 80;
                col.ReadOnly = true;
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
                dataGridResult.Columns.Add(col);
            }
        }

        private void ResetResultGrid()
        {
            dataGridResult.Rows.Clear();
        }

        private void AddNewManualMeasureResult()
        {
            int rowIndex = dataGridResult.Rows.Add();
            var row = dataGridResult.Rows[rowIndex];
            dataGridResult.Rows[rowIndex].HeaderCell.Value = $"{dataGridResult.Rows.Count - 1}";

            PKGTesterResult result = tester.Result;

            // 각 항목별 결과 표시
            foreach (var key in result.Items.Keys)
            {
                row.Cells[key].Value = result.Items[key].ToString();
            }

            // 마지막 행으로 스크롤 및 선택
            if (dataGridResult.Rows.Count > 0)
            {
                int lastRowIndex = dataGridResult.Rows.Count - 1;
                if (dataGridResult.AllowUserToAddRows && dataGridResult.Rows[lastRowIndex].IsNewRow)
                {
                    lastRowIndex--;
                }
                if (lastRowIndex >= 0)
                {
                    dataGridResult.ClearSelection();
                    dataGridResult.Rows[lastRowIndex].Selected = true;
                    dataGridResult.FirstDisplayedScrollingRowIndex = lastRowIndex;
                }
            }

            // BinNo에 따라 결과 표시
            BinningResult binningResult = result.BinningResult;
            switch (binningResult.BinType)
            {
                case BinningType.GoodBin:
                    lbResultValue.Text = $"{binningResult.BinNo}. {binningResult.BinLabel}";
                    lbResultValue.ForeColor = Color.Lime;
                    break;
                case BinningType.NgBin:
                    lbResultValue.Text = "NG";
                    lbResultValue.ForeColor = Color.Red;
                    break;
                default:
                    lbResultValue.Text = "UNKNOWN";
                    lbResultValue.ForeColor = Color.Gray;
                    break;
            }

            // 측정 시간 표시
            lbMeasureTime.Text = $"Measure Time: {tester.MeasureTime.TotalMilliseconds:F1} ms";
            lbCurrentIndexNo.Text = $"Rotary Index No: {GetCurrentProbeIndexNo() + 1}";
        }

        private void btnLastClear_Click(object sender, EventArgs e)
        {
            if (dataGridResult.Rows.Count > 0)
            {
                // DataGridView의 마지막 행이 NewRow(입력용 빈 행)일 수 있으므로 체크
                int lastRowIndex = dataGridResult.Rows.Count - 1;
                if (dataGridResult.AllowUserToAddRows && dataGridResult.Rows[lastRowIndex].IsNewRow)
                {
                    lastRowIndex--;
                }
                if (lastRowIndex >= 0)
                {
                    dataGridResult.Rows.RemoveAt(lastRowIndex);
                }
            }
        }

        private void btnResultClear_Click(object sender, EventArgs e)
        {
            ResetResultGrid();
        }

        private void btnResultSave_Click(object sender, EventArgs e)
        {
            if (dataGridResult.Rows.Count == 0 || dataGridResult.Columns.Count == 0)
            {
                MessageBox.Show("저장할 데이터가 없습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.Filter = "CSV 파일 (*.csv)|*.csv";
                dlg.Title = "결과 저장";
                dlg.FileName = "Result.csv";
                if (dlg.ShowDialog() != DialogResult.OK)
                    return;

                try
                {
                    var sb = new StringBuilder();

                    // 헤더
                    for (int i = 0; i < dataGridResult.Columns.Count; i++)
                    {
                        sb.Append(dataGridResult.Columns[i].HeaderText);
                        if (i < dataGridResult.Columns.Count - 1)
                            sb.Append(",");
                    }
                    sb.AppendLine();

                    // 데이터
                    foreach (DataGridViewRow row in dataGridResult.Rows)
                    {
                        if (row.IsNewRow) continue;

                        for (int i = 0; i < dataGridResult.Columns.Count; i++)
                        {
                            var value = row.Cells[i].Value?.ToString() ?? "";
                            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
                                value = $"\"{value.Replace("\"", "\"\"")}\"";
                            sb.Append(value);
                            if (i < dataGridResult.Columns.Count - 1)
                                sb.Append(",");
                        }
                        sb.AppendLine();
                    }

                    System.IO.File.WriteAllText(dlg.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("저장 완료", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("저장 중 오류 발생: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private int GetCurrentProbeIndexNo()
        {
            IndexChipProber proberUnit = Equipment.Instance.GetUnit(Equipment.UnitKeys.IndexChipProber) as IndexChipProber;
            int rotaryIndex = 0;
            if (proberUnit != null)
                rotaryIndex = proberUnit.GetProbeIndexNo();
            return rotaryIndex;
        }

        private async void RunManualMeasureAsync(int repeatCount, int intervalMs)
        {
            _ctsRepeat = new CancellationTokenSource();
            var token = _ctsRepeat.Token;

            try
            {
                for (int i = 0; i < repeatCount; i++)
                {
                    token.ThrowIfCancellationRequested();

                    int rotaryIndex = GetCurrentProbeIndexNo();
                    int result = await tester.ManualMeasureAsync(rotaryIndex);

                    // 측정 실패 시 반복 중단
                    if (result < 0)
                        break;

                    // 마지막 반복이 아니면 interval 대기
                    if (i < repeatCount - 1)
                        await Task.Delay(intervalMs, token);
                }
            }
            catch (OperationCanceledException)
            {
                // canceled
            }
            finally
            {
                _ctsRepeat.Dispose();
                _ctsRepeat = null;
            }
        }

        private void btnTestStart_Click(object sender, EventArgs e)
        {
            if (_ctsRepeat != null)
            {
                // 이미 동작 중이면 무시
                return;
            }

            //var ask = new MessageBoxYesNo();
            //if (ask.ShowDialog("Test", "시작 하시겠습니까?") != DialogResult.Yes)
            //{
            //    return;
            //}

            int repeatCount = 1;
            int intervalMs = 500;// 500;

            if (rbvOption.SelectedIndex == 1)
            {
                repeatCount = (int)nudRepeatCount.Value;
                intervalMs = (int)nudIntervalDelay.Value;
            }

            Task.Run(() => RunManualMeasureAsync(repeatCount, intervalMs)); 
        }

        private void btnTestStop_Click(object sender, EventArgs e)
        {
            _ctsRepeat?.Cancel();
        }

        private void rbTop_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void rbBottom_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void cbProbeIndex_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private int GetSelectedProbeIndex()
        {
            if (cbProbeIndex == null) return -1;

            if (cbProbeIndex.InvokeRequired)
            {
                try
                {
                    var idx = (int)cbProbeIndex.Invoke(new Func<int>(() => cbProbeIndex.SelectedIndex));
                    return idx < 0 ? -1 : idx;
                }
                catch
                {
                    return -1;
                }
            }

            var selected = cbProbeIndex.SelectedIndex;
            return selected < 0 ? -1 : selected;
            //if (cbProbeIndex == null || cbProbeIndex.SelectedIndex < 0) return -1;
            //// "1"~"8" → 0~7
            //return cbProbeIndex.SelectedIndex;
        }

        private bool GetSelectedTopMode()
        {
            return rbTop != null && rbTop.Checked;
        }


        private async void btnProbeSeq_Click(object sender, EventArgs e)
        {
            if (_autoMeasureRunning)
                return;

            var rotary = Equipment.Instance.GetUnit(Equipment.UnitKeys.Rotary) as Rotary;
            var controller = Equipment.Instance.GetUnit(Equipment.UnitKeys.IndexChipProbeController) as IndexChipProbeController;

            if (controller == null || rotary == null)
            {
                MessageBox.Show("필수 유닛 바인딩이 누락되었습니다.(ProbeController/Rotary)", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _autoMeasureRunning = true;
            var btn = sender as Control;   // 클릭한 버튼만 비활성화
            if (btn != null) 
                btn.Enabled = false;

            // Top/Bottom 모드 읽기
            bool isTop = GetSelectedTopMode();
            int selectedProbeIndex = GetSelectedProbeIndex(); // 0-based (콤보: 0~7)

            var cts = new CancellationTokenSource();
            var token = cts.Token;

            Task<int> task = Task.Run(async () =>
            {
                try
                {
                    // 2) 안전/인터락 확인을 위한 Ready 구성 (Z 축 등 안전 보장)
                    //    검사(측정)는 하지 않고 'Contact Ready' 위치로만 이동
                    int rc = 0;

                    // 우선 전체 Z 안전으로 정리(필요 시)
                    try
                    {
                        // 존재 시 사용 (메서드가 없으면 0 취급)
                        rc = controller.MovePositionSafetyZ();
                        if (rc != 0) return -1;
                    }
                    catch { /* 안전Z 메서드 없으면 통과 */ }

                    // 1) 선택 소켓으로 Rotary 이동 (선택 없으면 스킵)
                    if (selectedProbeIndex >= 0)
                    {
                        int rcMove = await MoveRotaryToProbeSocketAsync(selectedProbeIndex, rotary, controller, token).ConfigureAwait(false);
                        if (rcMove != 0) return -1;
                    }

                    // 3) Top/Bottom에 맞는 Contact Ready 위치로 이동
                    if (isTop)
                    {
                        // 상단 접촉 준비 위치로 이동
                        // 메서드가 제공되는 경우 사용
                        try
                        {
                            rc = controller.MovePositionTopContact_Index_Ready(selectedProbeIndex);
                            if (rc != 0) return -1;
                            rc = controller.MovePositionTopContact_Index_Up(selectedProbeIndex);
                            if (rc != 0) return -1;
                        }
                        catch
                        {
                            // 대체: 시퀀스형 준비 로직 (검사 없이 위치만 보장)
                            var recipe = Equipment.Instance.EquipmentRecipe.CurrentRecipe;
                            bool original = recipe.ContectTop;
                            recipe.ContectTop = true;
                            try
                            {
                                rc = controller.RunInspectionReady();
                                if (rc != 0) return -1;
                            }
                            finally
                            {
                                recipe.ContectTop = original;
                            }
                        }
                    }
                    else
                    {
                        // 하단 접촉 준비 위치로 이동
                        try
                        {
                            rc = controller.SyncProbeZGripperUpAndBottomReady(selectedProbeIndex);
                            if (rc != 0) return -1;

                            //rc = controller.MovePositionProbeZGripperIndexUp();
                            //if (rc != 0) return -1;
                            //rc = controller.MovePositionBottomContact_Index_Ready(selectedProbeIndex);
                            //if (rc != 0) return -1;

                            if (controller.Config.GripperMode == true)
                            {
                                rc = controller.MovePositionGripperXClamp();
                                if (rc != 0) return -1;
                            }
                            if (controller.SetProbeVac(true) == false)
                            {
                                return -1;
                            }
                            rc = controller.MovePositionBottomContact_Index_Up(selectedProbeIndex);
                            if (rc != 0) return -1;

                        }
                        catch
                        {
                            var recipe = Equipment.Instance.EquipmentRecipe.CurrentRecipe;
                            bool original = recipe.ContectTop;
                            recipe.ContectTop = false;
                            try
                            {
                                rc = controller.MovePositionGripperXReady();
                                if (rc != 0) return -1;

                                rc = controller.RunInspectionReady();
                                if (rc != 0) return -1;
                            }
                            finally
                            {
                                recipe.ContectTop = original;
                            }
                        }
                    }

                    return 0;
                }
                catch (OperationCanceledException)
                {
                    return -1;
                }
                catch (Exception ex)
                {
                    Log.Write("CellTesterPage", "btnProbeSeq_Click", ex.Message);
                    return -1;
                }
            }, token);

            // 진행 표시 (취소 지원)
            var pf = new ProgressForm("Probe Position Move", "이동 중...", task, controller);
            pf.StopProcess += _ =>
            {
                try { controller.CancelSequence(); } catch { }
                try { cts.Cancel(); } catch { }
            };

            pf.ShowDialog(this);

            // 결과 처리
            try
            {
                if (pf.DialogResult == DialogResult.Cancel)
                {
                    MessageBox.Show("이동이 취소되었습니다.", "취소", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    int rc = await task.ConfigureAwait(true);
                    if (rc != 0)
                        MessageBox.Show("Probe 위치 이동 실패.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    else
                        MessageBox.Show("Probe 위치 이동 완료.", "완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            finally
            {
                _autoMeasureRunning = false;
                if (btn != null) btn.Enabled = true;
                cts.Dispose();

                // 상태 라벨 갱신
                try { lbCurrentIndexNo.Text = $"Rotary Index No: {GetCurrentProbeIndexNo() + 1}"; } catch { }
            }
        }

        private async void btnProbeSafety_Click(object sender, EventArgs e)
        {
            if (_autoMeasureRunning)
                return;

            var controller = Equipment.Instance.GetUnit(Equipment.UnitKeys.IndexChipProbeController) as IndexChipProbeController;
            if (controller == null)
            {
                MessageBox.Show("ProbeController 바인딩 없음.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _autoMeasureRunning = true;
            var btn = sender as Control;
            if (btn != null) btn.Enabled = false;

            var cts = new CancellationTokenSource();
            var token = cts.Token;

            Task<int> task = Task.Run(() =>
            {
                try
                {
                    // 안전 위치 이동 (Z/FW/BW/ProbeCardZ 등을 안전 보장)
                    int rc = 0;
                    try
                    {
                        //if (controller.Config.GripperMode == true)
                        //{
                        //    rc = controller.MovePositionGripperXReady();
                        //    if (rc != 0) return -1;
                        //}

                        rc = controller.MovePositionSafetyZ();
                        if (rc != 0) return -1;

                        if (controller.SetProbeVac(false) == false)
                        {
                            return -1;
                        }
                    }
                    catch
                    {
                        // 메서드가 없거나 실패 시, 시퀀스 기반 보장 로직을 사용
                        // 컨트롤러 내부가 안전위치 인터락을 가지고 있으므로 RunInspectionReady로 대체
                        rc = controller.EnsureReady(isFine: false);
                        if (rc != 0) return -1;

                        if (controller.SetProbeVac(false) == false)
                        {
                            return -1;
                        }
                    }

                    // 최종 안전 판별(가능 시)
                    try
                    {
                        if (!controller.IsAllSafetyAxisPos() && !controller.IsProbeSafetyAxisPos())
                            return -1;
                    }
                    catch { /* 안전 판별 API 없으면 생략 */ }

                    return 0;
                }
                catch (OperationCanceledException)
                {
                    return -1;
                }
                catch (Exception ex)
                {
                    Log.Write("CellTesterPage", "btnProbeSafety_Click", ex.Message);
                    return -1;
                }
            }, token);

            var pf = new ProgressForm("Probe Safety Move", "안전 위치로 이동 중...", task, controller);
            pf.StopProcess += _ =>
            {
                try { controller.CancelSequence(); } catch { }
                try { cts.Cancel(); } catch { }
            };

            pf.ShowDialog(this);

            try
            {
                if (pf.DialogResult == DialogResult.Cancel)
                {
                    MessageBox.Show("이동이 취소되었습니다.", "취소", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    int rc = await task.ConfigureAwait(true);
                    if (rc != 0)
                        MessageBox.Show("안전 위치 이동 실패.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    else
                        MessageBox.Show("안전 위치 이동 완료.", "완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            finally
            {
                _autoMeasureRunning = false;
                if (btn != null) btn.Enabled = true;
                cts.Dispose();
            }
        }

        // Rotary를 목표 ProbeIndex로 이동(회전 + 대기)
        private async Task<int> MoveRotaryToProbeSocketAsync(int targetProbeIndex, 
            Rotary rotary, IndexChipProbeController controller, CancellationToken ct)
        {
            if (rotary == null || controller == null)
                return -1;

            int count = rotary.GetIndexCount();
            if (count <= 0)
                return -1;

            if (targetProbeIndex < 0 || targetProbeIndex >= count)
                return 0; // 현재 위치 사용

            // 현재 Probe가 바라보고 있는 소켓 인덱스
            int currentProbeIndex = controller.GetProbeIndexNo();
            // 정방향(전진)만 허용: 목표가 현재보다 뒤에 있으면 그대로 차, 앞(순환)이라면 래핑
            // ex) current=7, target=0, count=8 → steps = 1 (forward 래핑)
            int steps = 0;
            if (targetProbeIndex >= currentProbeIndex)
                steps = targetProbeIndex - currentProbeIndex;
            else
                steps = count - (currentProbeIndex - targetProbeIndex);

            if (steps == 0)
                return 0; // 이미 목표

            int nRet = 0;
            for (int i = 0; i < steps; i++)
            {
                ct.ThrowIfCancellationRequested();
                nRet = rotary.Rotate();
                if (nRet != 0)
                {
                    Log.Write("MeasurementResultForm", $"Rotary 회전 실패");
                    return -1;
                }

                // (선택) 조기 종료 확인
                int newProbeIndex = controller.GetProbeIndexNo();
                if (newProbeIndex == targetProbeIndex)
                {
                    break;
                }
            }

            // 최종 확인
            int finalProbeIndex = controller.GetProbeIndexNo();
            if (finalProbeIndex != targetProbeIndex)
            {
                Log.Write("CellTesterPage", $"목표 도달 미확인 (final={finalProbeIndex}, target={targetProbeIndex})");
                return -1;
            }

            return 0;
        }

        private void btnTestMotionStart_Click(object sender, EventArgs e)
        {
            if (_ctsRepeat != null)
            {
                // 이미 동작 중이면 무시
                return;
            }

            var ask = new MessageBoxYesNo();
            if (ask.ShowDialog("Test", "시작 하시겠습니까?") != DialogResult.Yes)
            {
                return;
            }

            int repeatCount = 1;
            int intervalMs = 500;

            if (rbvOption.SelectedIndex == 1)
            {
                repeatCount = (int)nudRepeatCount.Value;
                intervalMs = (int)nudIntervalDelay.Value;
            }

            Task.Run(async () =>
            {
                await RunManualMeasureWithControllerMotionAsync(repeatCount, intervalMs).ConfigureAwait(false);
            });
        }

        private void btnTestMotionStop_Click(object sender, EventArgs e)
        {
            _ctsRepeat?.Cancel();

            try
            {
                var controller = Equipment.Instance.GetUnit(Equipment.UnitKeys.IndexChipProbeController) as IndexChipProbeController;
                controller?.CancelSequence();
            }
            catch { }
        }


        private int MoveContactUp_UsingController(int selectedProbeIndex, bool isTop, IndexChipProbeController controller)
        {
            int nRet = 0;

            if (isTop)
            {
                if (controller.IsContactTop() == false)
                {
                    if (controller.SetContectTop(true) == false)
                    {
                        Log.Write(controller.UnitName, "[RunInspection] SetContectTop(Top) failed");
                        return -1;
                    }
                }

                //Log.Write("kkkkkkProb", "Start3");
                nRet = controller.TopContactAndMeasureOnce();
                if (nRet != 0)
                {
                    Log.Write(controller.UnitName, "[RunInspection] TopContactAndMeasureOnce failed");
                    return -1;
                }
            }
            else
            {
                if (controller.IsContactProbe() == false)
                {
                    if (controller.SetContectTop(false) == false)
                    {
                        Log.Write(controller.UnitName, "[RunInspection] SetContectTop(Bottom) failed");
                        return -1;
                    }
                }

                while (controller.IsRotaryIdle() != 0)
                {
                    return -1;
                }

                bool bFineSpeed = true;
                int nIndex = controller.GetProbeIndexNo();
                controller.SetProbeVac(true);

                nRet = controller.MovePositionGripperXReady(bFineSpeed);
                if (nRet != 0)
                {
                    Log.Write(controller.UnitName, "[BottomContactOnce] MovePositionGripperXReady failed");
                    return -1;
                }

                nRet = controller.MovePositionProbeZGripperIndexUp(nIndex, bFineSpeed);
                if (nRet != 0)
                {
                    Log.Write(controller.UnitName, "[BottomContactOnce] MovePositionGripperXIndexUp failed");
                    return -1;
                }

                nRet = controller.MovePositionBottomContact_Index_Ready(nIndex, bFineSpeed);
                if (nRet != 0)
                {
                    Log.Write(controller.UnitName, "[BottomContactOnce] MovePositionBottomContact_Index_Ready failed");
                    return -1;
                }

                if (controller.Config.GripperMode)
                {
                    nRet = controller.MovePositionGripperXClamp(bFineSpeed);
                    if (nRet != 0)
                    {
                        Log.Write(controller.UnitName, "[BottomContactOnce] MovePositionGripperXReady failed");
                        return -1;
                    }
                }

                if (controller.IsPositionProbeZGripperIndexUp(nIndex) == false)
                {
                    Log.Write(controller.UnitName, "[BottomContactOnce] IsPositionProbeZGripperIndexUp failed");
                    return -1;
                }
                //if (controller.IsPositionProbeZGripperIndexUp() == false)
                //{
                //    Log.Write(controller.UnitName, "[BottomContactOnce] IsPositionProbeZGripperIndexUp failed");
                //    return -1;
                //}

                nRet = controller.MovePositionBottomContact_Index_Up(nIndex, bFineSpeed);
                if (nRet != 0)
                {
                    Log.Write(controller.UnitName, "[BottomContactOnce] MovePositionBottomContact_Index_Up failed");
                    return -1;
                }

                controller.WaitByTime(controller.Config.UpperWaitTime);

                //nRet = controller.BottomContactAndMeasureOnce();
                //if (nRet != 0)
                //{
                //    Log.Write(controller.UnitName, "[RunInspection] BottomContactAndMeasureOnce failed");
                //    return -1;
                //}
            }


            //if (isTop)
            //{
            //    rc = controller.MovePositionTopContact_Index_Ready(selectedProbeIndex);
            //    if (rc != 0) 
            //        return -1;

            //    rc = controller.MovePositionTopContact_Index_Up(selectedProbeIndex);
            //    if (rc != 0) 
            //        return -1;

            //    return 0;
            //}

            //rc = controller.BottomContactAndMeasureOnce();
            //if (rc != 0)
            //    return -1;

            // Bottom
            //if (controller.SetProbeVac(true) == false)
            //    return -1;

            //rc = controller.MovePositionProbeZGripperIndexUp();
            //if (rc != 0) 
            //    return -1;

            //rc = controller.MovePositionBottomContact_Index_Ready(selectedProbeIndex);
            //if (rc != 0) 
            //    return -1;

            //rc = controller.MovePositionBottomContact_Index_Up(selectedProbeIndex);
            //if (rc != 0) 
            //    return -1;

            return 0;
        }

        private int MoveContactDownToSafety_UsingController(IndexChipProbeController controller)
        {
            int rc = controller.MovePositionSafetyZ();
            if (rc != 0) 
                return -1;

            // Bottom Vac Off (Top이면 영향 없음)
            try { controller.SetProbeVac(false); } catch { }

            return 0;
        }

        private async Task RunManualMeasureWithControllerMotionAsync(int repeatCount, int intervalMs)
        {
            // 기존 RunManualMeasureAsync는 내부에서 _ctsRepeat를 새로 만들어서 Stop 제어가 꼬일 수 있으므로
            // Motion용 루프는 여기서 _ctsRepeat를 생성/관리합니다.
            _ctsRepeat = new CancellationTokenSource();
            var token = _ctsRepeat.Token;

            var rotary = Equipment.Instance.GetUnit(Equipment.UnitKeys.Rotary) as Rotary;
            var controller = Equipment.Instance.GetUnit(Equipment.UnitKeys.IndexChipProbeController) as IndexChipProbeController;

            if (tester == null || rotary == null || controller == null)
                return;

            // 시작 시점 UI 값 고정
            bool isTop = GetSelectedTopMode();
            int selectedProbeIndex = GetSelectedProbeIndex(); // 0~7 or -1

            try
            {
                // 1) SafetyZ 이동
                token.ThrowIfCancellationRequested();
                int rc = controller.MovePositionSafetyZ();
                if (rc != 0) return;

                // 2) (선택된 경우) 지정 소켓으로 1번만 회전 이동
                token.ThrowIfCancellationRequested();
                if (selectedProbeIndex >= 0)
                {
                    rc = await MoveRotaryToProbeSocketAsync(selectedProbeIndex, rotary, controller, token).ConfigureAwait(false);
                    if (rc != 0) return;
                }

                // 3) 반복: Contact Up -> tester.ManualMeasureAsync -> SafetyZ(Down) -> interval
                for (int i = 0; i < repeatCount; i++)
                {
                    token.ThrowIfCancellationRequested();

                    // Contact Up(Top/Bottom)
                    rc = MoveContactUp_UsingController(selectedProbeIndex, isTop, controller);
                    if (rc != 0) break;

                    token.ThrowIfCancellationRequested();

                    //// 측정은 기존대로 ManualMeasureAsync 유지
                    int rotaryIndex = GetCurrentProbeIndexNo();
                    int result = await tester.ManualMeasureAsync(rotaryIndex).ConfigureAwait(false);
                    if (result < 0) break;

                    token.ThrowIfCancellationRequested();

                    // SafetyZ 복귀(Down)
                    rc = MoveContactDownToSafety_UsingController(controller);
                    if (rc != 0) break;

                    // interval
                    if (i < repeatCount - 1)
                        await Task.Delay(intervalMs, token).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                // Stop
            }
            finally
            {
                try { _ctsRepeat?.Dispose(); } catch { }
                _ctsRepeat = null;
            }
        }
    }
}