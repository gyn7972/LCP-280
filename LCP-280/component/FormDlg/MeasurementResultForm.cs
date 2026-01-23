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
        private bool _columnsInitialized;
        // CSV 헤더 형태 동일하게 고정 컬럼 정의
        private readonly string[] FixedHeaders = new[]
        {
            "Time",
            "IndexNo",
            "BinNo",
            "BinType",
            "BinLabel"
        };

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

        private bool TrySetCellValue(DataGridViewRow row, string columnName, object value)
        {
            if (row == null || string.IsNullOrWhiteSpace(columnName)) return false;
            if (!dataGridResult.Columns.Contains(columnName)) return false;

            try
            {
                row.Cells[columnName].Value = value;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void AddNewMeasureResult()
        {
            try
            {
                if (!_columnsInitialized) BuildGridColumns();

                // ConditionSet 변경 등으로 컬럼이 동적으로 추가된 경우 보강
                AppendNewItemColumns();

                var r = tester?.Result;
                if (r == null) return;

                var bin = r.BinningResult;
                int socketNo = GetCurrentProbeIndexNo() + 1;

                int rowIndex = dataGridResult.Rows.Add();
                var row = dataGridResult.Rows[rowIndex];
                row.HeaderCell.Value = (dataGridResult.Rows.Count - 1).ToString();

                // Time / 기본 정보 (컬럼 누락 시에도 예외 없이 진행)
                TrySetCellValue(row, "Time", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff"));
                TrySetCellValue(row, "IndexNo", socketNo);
                TrySetCellValue(row, "BinNo", bin?.BinNo ?? -1);
                TrySetCellValue(row, "BinType", bin?.BinType.ToString() ?? string.Empty);
                TrySetCellValue(row, "BinLabel", bin?.BinLabel ?? string.Empty);

                // Rank 컬럼은 존재 시에만 기록
                TrySetCellValue(row, "TopRankBinNo", bin?.BinNo ?? -1);
                TrySetCellValue(row, "TopRankBinType", bin?.BinType.ToString() ?? string.Empty);
                TrySetCellValue(row, "TopRankBinLabel", bin?.BinLabel ?? string.Empty);

                // 측정 아이템 값
                if (r.Items != null)
                {
                    foreach (var kv in r.Items)
                    {
                        if (dataGridResult.Columns.Contains(kv.Key))
                            TrySetCellValue(row, kv.Key, kv.Value != null ? kv.Value.ToString() : string.Empty);
                    }
                }

                // 행 선택 / 스크롤
                dataGridResult.ClearSelection();
                row.Selected = true;
                try { dataGridResult.FirstDisplayedScrollingRowIndex = rowIndex; } catch { }

                // 상단 표시 (bin null 안전 처리)
                switch (bin?.BinType ?? default(BinningType))
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

                var mt = tester != null ? tester.MeasureTime : TimeSpan.Zero;
                lbMeasureTime.Text = $"Measure Time: {mt.TotalMilliseconds:F1} ms";
                lbCurrentIndexNo.Text = $"Rotary Index No: {socketNo}";
            }
            catch (Exception ex)
            {
                // UI 동작 유지가 우선이므로 메시지 최소화 (필요 시 Log.Write로 교체 가능)
                try
                {
                    MessageBox.Show("결과 표시 중 오류: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch { }
            }
        }
        private void btnLastClear_Click(object sender, EventArgs e)
        {
            if (dataGridResult.Rows.Count <= 0) return;
            int last = dataGridResult.Rows.Count - 1;
            if (dataGridResult.AllowUserToAddRows && dataGridResult.Rows[last].IsNewRow) last--;
            if (last >= 0) dataGridResult.Rows.RemoveAt(last);
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