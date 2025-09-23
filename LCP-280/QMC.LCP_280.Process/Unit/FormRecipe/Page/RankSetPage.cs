using QMC.Common;
using QMC.Common.PKGTester;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Unit.FormRecipe.Page
{
    public partial class RankSetPage : UserControl
    {
        private Equipment equipment => Equipment.Instance;
        private PKGTester tester => equipment.Tester;

        private BinningSpecSheet tempSheet = new BinningSpecSheet();
        private PropertyCollection pc;

        private int defaultColumnCount = 0;

        public RankSetPage()
        {
            InitializeComponent();
            InitRankSetGrid();
        }

        private void InitRankSetGrid()
        {
            dataGridRank.Font = new Font("맑은 고딕", 8);

            dataGridRank.Columns.Clear();

            // Bin No
            DataGridViewColumn colBinNo = new DataGridViewTextBoxColumn();
            colBinNo.Name = "Bin No";
            colBinNo.HeaderText = "Bin No";
            colBinNo.ReadOnly = true;
            colBinNo.SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridRank.Columns.Add(colBinNo);
            defaultColumnCount++;

            // Bin Label
            DataGridViewColumn colBinLabel = new DataGridViewTextBoxColumn();
            colBinLabel.Name = "Bin Label";
            colBinLabel.HeaderText = "Bin Label";
            colBinLabel.ReadOnly = false;
            colBinLabel.SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridRank.Columns.Add(colBinLabel);
            defaultColumnCount++;
        }

        private void ClearRankSetGrid()
        {
            dataGridRank.SuspendLayout();
            dataGridRank.Rows.Clear();
            for (int i = dataGridRank.Columns.Count - 1; i >= defaultColumnCount; i--)
            {
                dataGridRank.Columns.RemoveAt(i);
            }
            dataGridRank.ResumeLayout();
        }

        private void UpdateRankSetGrid()
        {
            dataGridRank.SuspendLayout();

            // 1. Clear existing rows
            ClearRankSetGrid();

            // 2. Add header Columns
            foreach (var header in tempSheet.Headers)
            {
                DataGridViewColumn col = new DataGridViewTextBoxColumn();
                col.Name = header;
                col.HeaderText = header;
                col.ReadOnly = true;
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
                dataGridRank.Columns.Add(col);
            }

            // 3. Add rows
            for (int binIndex = 0; binIndex < tempSheet.Specs.Count; binIndex++)
            {
                var spec = tempSheet.Specs[binIndex];

                var row = new DataGridViewRow();
                row.CreateCells(dataGridRank);
                row.Cells[0].Value = (binIndex + 1).ToString(); // BIN No.
                row.Cells[1].Value = spec.BinLabel; // Label

                // Items
                for (int headerIndex = 0; headerIndex < tempSheet.Headers.Count; headerIndex++)
                {
                    var header = tempSheet.Headers[headerIndex];
                    var item = spec.Items[header];

                    row.Cells[defaultColumnCount + headerIndex].Value = item.ToString();
                }
                dataGridRank.Rows.Add(row);
            }

            dataGridRank.ResumeLayout();
        }

        private void dataGridRank_SelectionChanged(object sender, EventArgs e)
        {
            int rowIndex = dataGridRank.CurrentCell?.RowIndex ?? -1;
            int colIndex = dataGridRank.CurrentCell?.ColumnIndex ?? -1;

            if (rowIndex < 0 || colIndex < 0)
                return;

            int binIndex = rowIndex;
            string header = dataGridRank.Columns[colIndex].Name;

            if ((0 <= binIndex && binIndex < tempSheet.Specs.Count) && tempSheet.Headers.Contains(header))
            {
                BinningRange item = tempSheet.Specs[binIndex].Items[header];
                pc = item.GetPropertyCollection();
                pcvEdit.SetProperties(pc);
            }
            else
            {
                pc = null;
                pcvEdit.SetProperties(null);
            }
        }

        private void dataGridRank_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            int rowIndex = e.RowIndex;
            int colIndex = e.ColumnIndex;

            if (rowIndex < 0 || colIndex < 0)
                return;

            if (dataGridRank.Columns[colIndex].Name == "Bin Label")
            {
                int binIndex = rowIndex;
                string newLabel = dataGridRank.Rows[rowIndex].Cells[colIndex].Value?.ToString() ?? "";
                if ((0 <= binIndex && binIndex < tempSheet.Specs.Count) && !string.IsNullOrEmpty(newLabel))
                {
                    tempSheet.Specs[binIndex].BinLabel = newLabel;
                }
                else
                {
                    // Invalid index or empty label, revert to old value
                    dataGridRank.Rows[rowIndex].Cells[colIndex].Value = tempSheet.Specs[binIndex].BinLabel;
                }
            }
        }

        private void btnMaxRankUpdate_Click(object sender, EventArgs e)
        {
            int maxRank = (int)nudMaxRank.Value;
            if (tempSheet.Specs.Count > maxRank)
            {
                // max rank가 적은 경우 제거한다.
                int deleteCount = tempSheet.Specs.Count - maxRank;
                tempSheet.RemoveBinsFromLastBin(deleteCount);
            }
            else
            {
                // max rank가 많은 경우 추가한다.
                int addCount = maxRank - tempSheet.Specs.Count;
                for (int i = 0; i < addCount; i++)
                {
                    tempSheet.AddNewBin("NewBinLabel");
                }
            }

            UpdateRankSetGrid();
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            // 현재 tester에 있는 측정 아이템들을 기반으로 Header Columns을 생성한다.
            tempSheet.Clear();
            foreach (var header in tester.Result.Items.Keys)
            {
                tempSheet.AddHeader(header);
            }

            // max Rank 갯수만큼 빈을 추가한다.
            int maxRank = (int)nudMaxRank.Value;
            for (int i = 0; i < maxRank; i++)
            {
                tempSheet.AddNewBin("NewBinLabel");
            }

            UpdateRankSetGrid();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = "Binning Spec Sheet (*.csv)|*.csv|All Files (*.*)|*.*";
                dlg.Title = "비닝 사양 파일 열기";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    bool loaded = tempSheet.LoadFromFile(dlg.FileName);
                    if (loaded)
                    {
                        UpdateRankSetGrid();
                        MessageBox.Show("파일을 성공적으로 불러왔습니다.", "정보", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("파일을 불러오지 못했습니다.\n파일 형식을 확인하세요.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.Filter = "Binning Spec Sheet (*.csv)|*.csv|All Files (*.*)|*.*";
                dlg.Title = "비닝 사양 파일 저장";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    bool saved = tempSheet.SaveToFile(dlg.FileName);
                    if (saved)
                    {
                        MessageBox.Show("파일을 성공적으로 저장했습니다.", "정보", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("파일 저장에 실패했습니다.\n파일 경로 및 권한을 확인하세요.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            // Interlock
            if (!tempSheet.Validate())
            {
                MessageBox.Show("Invalid test sheet.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var result = MessageBox.Show("Do you want to save it in the Apply and Recipe data?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                try
                {
                    if (Equipment.Instance.Tester.LoadBinningSpecSheet(tempSheet) == 0)
                    {
                        // 레시피 처리 수정 필요...
                    }
                    else
                    {
                        MessageBox.Show("Failed to apply the test sheet.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
        }

        private void btnModify_Click(object sender, EventArgs e)
        {
            int rowIndex = dataGridRank.CurrentCell?.RowIndex ?? -1;
            int colIndex = dataGridRank.CurrentCell?.ColumnIndex ?? -1;

            if (rowIndex < 0 || colIndex < 0)
                return;

            int binIndex = rowIndex;
            string header = dataGridRank.Columns[colIndex].Name;

            if ((0 <= binIndex && binIndex < tempSheet.Specs.Count) && tempSheet.Headers.Contains(header))
            {
                pcvEdit.Apply();

                BinningRange tempItem = new BinningRange("");
                tempItem.ApplyValueFromPropertyCollection(pc);
                if (!tempItem.Validate())
                {
                    MessageBox.Show("Invalid value.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                BinningRange item = tempSheet.Specs[binIndex].Items[header];
                item.CopyFrom(tempItem);
                UpdateRankSetGrid();
            }
        }

        
    }
}
