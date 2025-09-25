using Newtonsoft.Json.Linq;
using QMC.Common.PKGTester;
using QMC.Common.Spectrometer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Unit.FormRecipe.Page
{
    public partial class CellTesterPage : UserControl
    {
        private PKGTester tester => Equipment.Instance.Tester;

        // Repeat
        private CancellationTokenSource _ctsRepeat;

        public CellTesterPage()
        {
            InitializeComponent();

            rbvOption.SetOptions(false, "Off", "On");

            dataGridResult.Font = new Font("맑은 고딕", 8);
            
            if (tester != null)
            {
                tester.OnConditionSetChanged += Tester_OnConditionSetChanged;
                tester.OnManualMeasureCompleted += Tester_OnMeasureCompleted;

                casSpectrumViewer.AttachSpectrometer(tester.Spectrometer);

                tester.Sourcemeter.OnMeasureFailed += Sourcemeter_OnMeasureFailed;
                tester.Spectrometer.OnMeasureFailed += Spectrometer_OnMeasureFailed;
            }
        }

        private void Spectrometer_OnMeasureFailed(object sender, string e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => MessageBox.Show(e)));
                return;
            }
            MessageBox.Show(e);
        }

        private void Sourcemeter_OnMeasureFailed(object sender, string e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => MessageBox.Show(e)));
                return;
            }
            MessageBox.Show(e);
        }

        private void Tester_OnMeasureCompleted(object sender)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => Tester_OnMeasureCompleted(sender)));
                return;
            }

            AddNewManualMeasureResult();
        }

        private void Tester_OnConditionSetChanged(object sender)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => Tester_OnConditionSetChanged(sender)));
                return;
            }

            UpdateNewResultGrid();
        }

        private void ClearResultGrid()
        {             
            dataGridResult.Rows.Clear();
            dataGridResult.Columns.Clear();
        }

        private void UpdateNewResultGrid()
        {
            ClearResultGrid();
            foreach (var item in tester.ConditionSet.Items)
            {
                if (!item.IsMeasureItem())
                    continue;

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

        private async void RunManualMeasureAsync(int repeatCount, int intervalMs)
        {
            _ctsRepeat = new CancellationTokenSource();
            var token = _ctsRepeat.Token;

            try
            {
                for (int i = 0; i < repeatCount; i++)
                {
                    token.ThrowIfCancellationRequested();

                    int result = await tester.ManualMeasureAsync();

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

            int repeatCount = 1;
            int intervalMs = 500;

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
    }
}