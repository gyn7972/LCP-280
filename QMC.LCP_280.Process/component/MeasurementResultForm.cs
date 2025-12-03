using Newtonsoft.Json.Linq;
using QMC.Common;
using QMC.Common.PKGTester;
using QMC.LCP_280.Process.Unit;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Component
{
    public partial class MeasurementResultForm : Form
    {
        private PKGTester tester => Equipment.Instance.Tester;

        private CancellationTokenSource _ctsRepeat;
        private bool _columnsInitialized;

        // CSV 헤더 형태 동일하게 고정 컬럼 정의
        private readonly string[] FixedHeaders = new[]
        {
            "Timestamp",
            "SocketNumber",
            "BinNo",
            "BinType",
            "BinLabel"
        };

        // 중복 시작 방지
        private bool _autoMeasureRunning = false;



        public MeasurementResultForm()
        {
            InitializeComponent(); // 디자이너 메서드 호출
            if (tester != null)
            {
                tester.OnConditionSetChanged += Tester_OnConditionSetChanged;

                // 추가: 자동 측정 완료 이벤트도 구독
                tester.OnMeasureCompleted += Tester_OnMeasureCompleted;

                // 기존: 수동 측정 완료 이벤트
                tester.OnManualMeasureCompleted += Tester_OnMeasureCompleted;

                tester.OnMeasureAborted += Tester_OnMeasureAborted;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (dataGridResult.Columns.Count == 0)
                BuildGridColumns(); 
        }

        // 새로운 보조 메서드: 없는 컬럼만 추가
        private void AppendNewItemColumns()
        {
            if (tester?.ConditionSet?.Items == null) return;

            foreach (var item in tester.ConditionSet.Items)
            {
                if (!dataGridResult.Columns.Contains(item.Name))
                {
                    var col = new DataGridViewTextBoxColumn
                    {
                        Name = item.Name,
                        HeaderText = item.Name,
                        Width = 80,
                        ReadOnly = true,
                        SortMode = DataGridViewColumnSortMode.NotSortable
                    };
                    dataGridResult.Columns.Add(col);
                }
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // 사용자가 X 누를 때는 실제 종료하지 않고 숨김 처리 → 데이터/이벤트 유지
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
                return;
            }

            try
            {
                tester.OnConditionSetChanged -= Tester_OnConditionSetChanged;

                // 추가: 자동 측정 구독 해제
                tester.OnMeasureCompleted -= Tester_OnMeasureCompleted;

                tester.OnManualMeasureCompleted -= Tester_OnMeasureCompleted;
                tester.OnMeasureAborted -= Tester_OnMeasureAborted;
            }
            catch { }
            base.OnFormClosing(e);
        }

        // ===== 기존 로직 메서드들 (InitializeComponent 제거 후 그대로 유지) =====
        private void Tester_OnMeasureCompleted(object sender)
        {
            if (InvokeRequired) { Invoke(new Action(() => Tester_OnMeasureCompleted(sender))); return; }
            AddNewMeasureResult();
        }
        private void Tester_OnMeasureAborted(object sender)
        {
            if (InvokeRequired) { Invoke(new Action(() => Tester_OnMeasureAborted(sender))); return; }
            // 필요 시 실패 행 추가 가능
        }
        private void Tester_OnConditionSetChanged(object sender)
        {
            if (InvokeRequired) { Invoke(new Action(() => Tester_OnConditionSetChanged(sender))); return; }

            // 기존 데이터는 유지하고, 없는 컬럼만 추가
            AppendNewItemColumns();

            lbResultValue.Text = "";
            lbMeasureTime.Text = "Measure Time: -";
            lbCurrentIndexNo.Text = "Rotary Index No: -";
        }

        private void ClearResultGrid()
        {
            try { dataGridResult.Rows.Clear(); } catch { }
            _columnsInitialized = false;
        }
        private void ResetResultGrid()
        {
            try { dataGridResult.Rows.Clear(); } catch { }
        }
       
        private void BuildGridColumns()
        {
            dataGridResult.Columns.Clear();
            _columnsInitialized = false;

            // 고정 컬럼
            foreach (var h in FixedHeaders)
            {
                var col = new DataGridViewTextBoxColumn
                {
                    Name = h,
                    HeaderText = h,
                    ReadOnly = true,
                    Width = 110,
                    SortMode = DataGridViewColumnSortMode.NotSortable
                };
                dataGridResult.Columns.Add(col);
            }

            // 측정 아이템 컬럼
            if (tester?.ConditionSet?.Items != null)
            {
                foreach (var item in tester.ConditionSet.Items)
                {
                    var col = new DataGridViewTextBoxColumn
                    {
                        Name = item.Name,
                        HeaderText = item.Name,
                        ReadOnly = true,
                        Width = 80,
                        SortMode = DataGridViewColumnSortMode.NotSortable
                    };
                    dataGridResult.Columns.Add(col);
                }
            }

            _columnsInitialized = true;
        }

        private void RebuildDynamicItemColumns()
        {
            if (!_columnsInitialized)
            {
                BuildGridColumns();
                return;
            }

            // 기존 측정 아이템 컬럼 제거 (고정 헤더 제외)
            var fixedSet = FixedHeaders.ToHashSet(StringComparer.OrdinalIgnoreCase);
            var toRemove = dataGridResult.Columns.Cast<DataGridViewColumn>()
                .Where(c => !fixedSet.Contains(c.Name))
                .ToList();
            foreach (var c in toRemove)
                dataGridResult.Columns.Remove(c);

            // 새 항목 다시 추가
            if (tester?.ConditionSet?.Items != null)
            {
                foreach (var item in tester.ConditionSet.Items)
                {
                    if (!dataGridResult.Columns.Contains(item.Name))
                    {
                        var col = new DataGridViewTextBoxColumn
                        {
                            Name = item.Name,
                            HeaderText = item.Name,
                            ReadOnly = true,
                            Width = 80,
                            SortMode = DataGridViewColumnSortMode.NotSortable
                        };
                        dataGridResult.Columns.Add(col);
                    }
                }
            }
        }


        private void AddNewMeasureResult()
        {
            if (!_columnsInitialized) BuildGridColumns();

            int rowIndex = dataGridResult.Rows.Add();
            var row = dataGridResult.Rows[rowIndex];
            row.HeaderCell.Value = (dataGridResult.Rows.Count - 1).ToString();

            var r = tester.Result;
            var bin = r.BinningResult;
            int socketNo = GetCurrentProbeIndexNo() + 1;

            // Timestamp
            row.Cells["Timestamp"].Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");
            row.Cells["SocketNumber"].Value = socketNo;
            row.Cells["BinNo"].Value = bin?.BinNo ?? -1;
            row.Cells["BinType"].Value = bin?.BinType.ToString() ?? "";
            row.Cells["BinLabel"].Value = bin?.BinLabel ?? "";

            // Rank 정보 (현재 구현 없으면 공백 또는 Bin과 동일 처리)
            int topRankBinNo = bin?.BinNo ?? -1;
            string topRankBinType = bin?.BinType.ToString() ?? "";
            string topRankBinLabel = bin?.BinLabel ?? "";

            // Rank 컬럼은 현재 FixedHeaders에 없으므로, 컬럼 존재 시에만 기록
            if (dataGridResult.Columns.Contains("TopRankBinNo"))
                row.Cells["TopRankBinNo"].Value = bin?.BinNo ?? -1;
            if (dataGridResult.Columns.Contains("TopRankBinType"))
                row.Cells["TopRankBinType"].Value = bin?.BinType.ToString() ?? "";
            if (dataGridResult.Columns.Contains("TopRankBinLabel"))
                row.Cells["TopRankBinLabel"].Value = bin?.BinLabel ?? "";

            // 측정 아이템 값
            foreach (var key in r.Items.Keys)
            {
                if (dataGridResult.Columns.Contains(key))
                    row.Cells[key].Value = r.Items[key].ToString();
            }

            // 행 선택 / 스크롤
            dataGridResult.ClearSelection();
            row.Selected = true;
            try { dataGridResult.FirstDisplayedScrollingRowIndex = rowIndex; } catch { }

            // 상단 표시
            switch (bin.BinType)
            {
                case BinningType.GoodBin:
                    lbResultValue.Text = $"{bin.BinNo}. {bin.BinLabel}";
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
            lbMeasureTime.Text = $"Measure Time: {tester.MeasureTime.TotalMilliseconds:F1} ms";
            lbCurrentIndexNo.Text = $"Rotary Index No: {GetCurrentProbeIndexNo() + 1}";
        }



        private void btnLastClear_Click(object sender, EventArgs e)
        {
            if (dataGridResult.Rows.Count <= 0) return;
            int last = dataGridResult.Rows.Count - 1;
            if (dataGridResult.AllowUserToAddRows && dataGridResult.Rows[last].IsNewRow) last--;
            if (last >= 0) dataGridResult.Rows.RemoveAt(last);
        }
        private void btnResultClear_Click(object sender, EventArgs e) => ResetResultGrid();

        private void btnResultSave_Click(object sender, EventArgs e)
        {
            if (dataGridResult.Rows.Count == 0 || dataGridResult.Columns.Count == 0)
            {
                MessageBox.Show("저장할 데이터가 없습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            using (var dlg = new SaveFileDialog { Filter = "CSV (*.csv)|*.csv", FileName = "Result.csv" })
            {
                if (dlg.ShowDialog() != DialogResult.OK) return;
                try
                {
                    var sb = new StringBuilder();
                    // 헤더 동일 순서
                    foreach (DataGridViewColumn col in dataGridResult.Columns)
                    {
                        sb.Append(col.HeaderText);
                        sb.Append(",");
                    }
                    sb.Length--; sb.AppendLine();

                    foreach (DataGridViewRow row in dataGridResult.Rows)
                    {
                        if (row.IsNewRow) continue;
                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            var v = cell.Value?.ToString() ?? "";
                            if (v.Contains(",") || v.Contains("\"") || v.Contains("\n"))
                                v = "\"" + v.Replace("\"", "\"\"") + "\"";
                            sb.Append(v).Append(",");
                        }
                        sb.Length--; sb.AppendLine();
                    }
                    System.IO.File.WriteAllText(dlg.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("저장 완료", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("저장 중 오류: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private int GetCurrentProbeIndexNo()
        {
            var prober = Equipment.Instance.GetUnit(Equipment.UnitKeys.IndexChipProber) as IndexChipProber;
            return prober != null ? prober.GetProbeIndexNo() : 0;
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
                    int res = await tester.ManualMeasureAsync(rotaryIndex);
                    if (res < 0) break;
                    if (i < repeatCount - 1) await Task.Delay(intervalMs, token);
                }
            }
            catch (OperationCanceledException) { }
            finally
            {
                _ctsRepeat.Dispose();
                _ctsRepeat = null;
            }
        }
        private void btnTestStart_Click(object sender, EventArgs e)
        {
            if (_ctsRepeat != null) return;
            int repeatCount = chkRepeat.Checked ? (int)nudRepeatCount.Value : 1;
            int intervalMs = chkRepeat.Checked ? (int)nudIntervalDelay.Value : 500;
            Task.Run(() => RunManualMeasureAsync(repeatCount, intervalMs));
        }
        private void btnTestStop_Click(object sender, EventArgs e) => _ctsRepeat?.Cancel();



        // GUI에서 선택해서 가져와야함.
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

        // Rotary를 목표 ProbeIndex로 이동(회전 + 대기)
        private async Task<int> MoveRotaryToProbeSocketAsync(int targetProbeIndex, Rotary rotary, IndexChipProbeController controller, CancellationToken ct)
        {
            if (rotary == null || controller == null)
                return -1;

            int count = rotary.GetIndexCount();
            if (count <= 0)
                return -1;

            if (targetProbeIndex < 0 || targetProbeIndex >= count)
                return 0; // 현재 위치 사용

            // 현재 LoadIndex
            int currentLoadIndex = rotary.GetLoadIndexNo();

            // loadIndex = (probeIndex + IndexOfProbe) % count
            int indexOfProbeOffset = controller.Config.IndexOfProbe;
            int desiredLoadIndex = (targetProbeIndex + indexOfProbeOffset) % count;

            if (desiredLoadIndex == currentLoadIndex)
                return 0;

            int forwardSteps = (desiredLoadIndex - currentLoadIndex + count) % count;
            int backwardSteps = (currentLoadIndex - desiredLoadIndex + count) % count;

            bool useForward = forwardSteps <= backwardSteps;
            int steps = useForward ? forwardSteps : backwardSteps;

            for (int i = 0; i < steps; i++)
            {
                ct.ThrowIfCancellationRequested();
                string reason;
                bool ok = useForward
                    ? rotary.TryMoveIndexNext(out reason)
                    : rotary.TryMoveIndexPrev(out reason);

                if (!ok)
                {
                    Log.Write("MeasurementResultForm", $"Rotary 이동 실패: {reason}");
                    return -1;
                }

                int wrc = rotary.WaitIndexMoveDone();
                if (wrc != 0)
                {
                    Log.Write("MeasurementResultForm", "Rotary 이동 대기 타임아웃/오류");
                    return -1;
                }
            }
            return 0;
        }

        private async void buttonSeqMeasureStart_Click(object sender, EventArgs e)
        {
            if (_autoMeasureRunning)
                return;

            var rotary = Equipment.Instance.GetUnit(Equipment.UnitKeys.Rotary) as Rotary;
            var controller = Equipment.Instance.GetUnit(Equipment.UnitKeys.IndexChipProbeController) as IndexChipProbeController;

            if (tester == null || controller == null || rotary == null)
            {
                MessageBox.Show("필수 유닛 바인딩이 누락되었습니다.(Tester/ProbeController/Rotary)", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!tester.CanMeasure())
            {
                MessageBox.Show("Tester 준비가 되지 않았습니다. Test Condition/Binning/계측기 상태를 확인하세요.", "준비 안됨", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _autoMeasureRunning = true;
            var prevCursor = Cursor.Current;
            buttonSeqMeasureStart.Enabled = false;
            Cursor.Current = Cursors.WaitCursor;

            var cts = new CancellationTokenSource();
            var token = cts.Token;

            try
            {
                // 1) 선택된 Probe Index로 이동(선택 없으면 현재 유지)
                int selectedProbeIndex = GetSelectedProbeIndex();
                if (selectedProbeIndex >= 0)
                {
                    int rcMove = await Task.Run(() => MoveRotaryToProbeSocketAsync(selectedProbeIndex, rotary, controller, token));
                    if (rcMove != 0)
                    {
                        MessageBox.Show("Rotary 목표 소켓 이동 실패", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                // 2) 측정 준비 (인터락/안전)
                int rcReady = await Task.Run(() => controller.RunInspectionReady(), token);
                if (rcReady != 0)
                {
                    MessageBox.Show("측정 준비 실패 (RunInspectionReady)", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 3) Top/Bottom 모드 반영(일시 적용 후 복구)

                var recipe = Equipment.Instance.EquipmentRecipe.CurrentRecipe;
                bool originalMode = recipe.ContectTop;
                recipe.ContectTop = GetSelectedTopMode();
                try
                {
                    int rcMeasure = await Task.Run(() => controller.RunInspection(), token);
                    if (rcMeasure != 0)
                    {
                        MessageBox.Show("측정 실패 (RunInspection)", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                finally
                {
                    recipe.ContectTop = originalMode;
                }
                // 성공 시 그리드 추가는 OnMeasureCompleted 이벤트에서 처리됨
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("자동 측정 취소됨", "취소", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("자동 측정 실행 중 예외: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = prevCursor;
                buttonSeqMeasureStart.Enabled = true;
                _autoMeasureRunning = false;
                cts.Dispose();
            }
        }


        // ===== CSV 파싱 헬퍼 =====
        private static string[] ParseCsvLine(string line)
        {
            if (string.IsNullOrEmpty(line)) return new string[0];
            var list = new System.Collections.Generic.List<string>();
            var sb = new StringBuilder();
            bool inQuotes = false;
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (inQuotes)
                {
                    if (c == '\"')
                    {
                        if (i + 1 < line.Length && line[i + 1] == '\"')
                        {
                            sb.Append('\"'); // 이스케이프된 인용부호
                            i++;
                        }
                        else
                        {
                            inQuotes = false;
                        }
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
                else
                {
                    if (c == ',')
                    {
                        list.Add(sb.ToString());
                        sb.Clear();
                    }
                    else if (c == '\"')
                    {
                        inQuotes = true;
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
            }
            list.Add(sb.ToString());
            return list.ToArray();
        }

        // CSV 파일 로드 (clearExisting = true면 기존 행 제거, 컬럼은 유지/추가)
        private void LoadCsvFile(string filePath, bool clearExisting)
        {
            if (!File.Exists(filePath))
            {
                MessageBox.Show("파일이 존재하지 않습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                using (var reader = new StreamReader(filePath, Encoding.UTF8, true))
                {
                    string headerLine = reader.ReadLine();
                    if (headerLine == null)
                    {
                        MessageBox.Show("빈 파일입니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    var headers = ParseCsvLine(headerLine);
                    if (headers.Length == 0)
                    {
                        MessageBox.Show("헤더를 읽을 수 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // 컬럼 초기화 조건
                    if (!_columnsInitialized || dataGridResult.Columns.Count == 0)
                        BuildGridColumns();

                    // 필요한 새 컬럼 추가
                    foreach (var h in headers)
                    {
                        if (string.IsNullOrWhiteSpace(h)) continue;
                        if (!dataGridResult.Columns.Contains(h))
                        {
                            var col = new DataGridViewTextBoxColumn
                            {
                                Name = h,
                                HeaderText = h,
                                ReadOnly = true,
                                Width = 90,
                                SortMode = DataGridViewColumnSortMode.NotSortable
                            };
                            dataGridResult.Columns.Add(col);
                        }
                    }

                    if (clearExisting)
                        dataGridResult.Rows.Clear();

                    int added = 0;
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var cells = ParseCsvLine(line);
                        if (cells.Length == 0) continue;

                        int rowIndex = dataGridResult.Rows.Add();
                        var row = dataGridResult.Rows[rowIndex];
                        row.HeaderCell.Value = (dataGridResult.Rows.Count - 1).ToString();

                        // 매핑하여 채우기
                        for (int i = 0; i < headers.Length; i++)
                        {
                            string colName = headers[i];
                            if (string.IsNullOrEmpty(colName)) continue;
                            string value = i < cells.Length ? cells[i] : string.Empty;
                            if (dataGridResult.Columns.Contains(colName))
                                row.Cells[colName].Value = value;
                        }
                        added++;
                    }

                    if (added == 0)
                        MessageBox.Show("데이터 행이 없습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    else
                        MessageBox.Show($"{added} 행 로드 완료.", "로드", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("로드 중 오류: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnResultLoad_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog()
            {
                Filter = "CSV (*.csv)|*.csv|All Files (*.*)|*.*",
                Title = "결과 파일 불러오기"
            })
            {
                if (ofd.ShowDialog() != DialogResult.OK) return;

                // 기존 데이터 유지 여부 선택 (간단한 Yes/No)
                var keep = MessageBox.Show("기존 데이터 유지 후 뒤에 붙이겠습니까?\n[아니오] 선택 시 기존 행을 지웁니다.",
                    "로드 모드 선택", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                if (keep == DialogResult.Cancel) return;

                bool clearExisting = (keep == DialogResult.No);
                LoadCsvFile(ofd.FileName, clearExisting);
            }
        }





    }
}