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
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Unit.FormRecipe.Page
{
    public partial class CellTesterPage : UserControl
    {
        private PKGTester tester => Equipment.Instance.Tester;

        public CellTesterPage()
        {
            InitializeComponent();

            dataGridResult.Font = new Font("맑은 고딕", 8);
            
            if (tester != null)
            {
                tester.OnConditionSetChanged += Tester_OnConditionSetChanged;
                tester.OnManualMeasureCompleted += Tester_OnMeasureCompleted;

                casSpectrumViewer.AttachSpectrometer(tester.Spectrometer);
            }
        }

        private void Tester_OnMeasureCompleted(object sender)
        {
            AddResultToResultGrid();
        }

        private void Tester_OnConditionSetChanged(object sender)
        {
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

        private void AddResultToResultGrid()
        {
            int rowIndex = dataGridResult.Rows.Add();
            var row = dataGridResult.Rows[rowIndex];

            foreach (var key in tester.Results.Keys)
            {
                row.Cells[key].Value = tester.Results[key].ToString();
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

        private async void btnTestStart_Click(object sender, EventArgs e)
        {
            int result = await tester.ManualMeasureAsync(1, 500);
        }

        private void btnTestStop_Click(object sender, EventArgs e)
        {

        }
    }
}
