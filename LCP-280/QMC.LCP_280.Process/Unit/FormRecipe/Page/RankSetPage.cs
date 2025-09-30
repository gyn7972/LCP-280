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
using System.IO;

namespace QMC.LCP_280.Process.Unit.FormRecipe.Page
{
    public partial class RankSetPage : UserControl
    {
        private Equipment equipment => Equipment.Instance;
        private Component.MeasurementRecipe currentRecipe => equipment.EquipmentRecipe.CurrentRecipe;

        private BinningSpecSheet tempSheet = new BinningSpecSheet();
        private PropertyCollection pc;

        private string filePath = "";
        private bool isModified = false;

        private int defaultColumnCount = 0;

        public RankSetPage()
        {
            InitializeComponent();
            InitRankSetGrid();
        }

        private void RankSetPage_Load(object sender, EventArgs e)
        {
            if (currentRecipe != null)
            {
                if (tempSheet.LoadFromFile(currentRecipe.BinningSpecSheetPath) == 0)
                {
                    filePath = currentRecipe.BinningSpecSheetPath;
                    lbSetNameValue.Text = Path.GetFileNameWithoutExtension(filePath);
                }
                else
                {
                    filePath = "";
                    lbSetNameValue.Text = "No file";
                }
            }

            UpdateRankSetGrid();
        }

        private void RankSetPage_VisibleChanged(object sender, EventArgs e)
        {
            // 수정 후 저장하지 않고 다른 Page로 이동 시, 적용되지 않은 상태로 유지하기 위함
            if (Visible)
            {
                if (currentRecipe != null)
                {
                    if (filePath != currentRecipe.BinningSpecSheetPath || isModified)
                    {
                        if (tempSheet.LoadFromFile(currentRecipe.BinningSpecSheetPath) == 0)
                        {
                            filePath = currentRecipe.BinningSpecSheetPath;
                            lbSetNameValue.Text = Path.GetFileNameWithoutExtension(filePath);
                            UpdateRankSetGrid();
                        }
                        else
                        {
                            filePath = "";
                            lbSetNameValue.Text = "";
                            ClearRankSetGrid();
                        }
                    }                
                }
            }
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
            isModified = true;
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
            isModified = true;

            UpdateRankSetGrid();
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            // 현재 tester에 있는 측정 아이템들을 기반으로 Header Columns을 생성한다.
            tempSheet.Clear();

            var testerHeaders = equipment.Tester.Result.Items.Keys;
            foreach (var header in testerHeaders)
            {
                tempSheet.AddHeader(header);
            }

            // max Rank 갯수만큼 빈을 추가한다.
            int maxRank = (int)nudMaxRank.Value;
            for (int i = 0; i < maxRank; i++)
            {
                tempSheet.AddNewBin("NewBinLabel");
            }
            isModified = true;

            UpdateRankSetGrid();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.InitialDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BinningSpecSheet");
                dialog.Filter = "Binning Spec Sheet (*.csv)|*.csv|All Files (*.*)|*.*";
                dialog.Title = "Open Binning Spec Sheet";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var filePath = dialog.FileName;
                    if (tempSheet.LoadFromFile(dialog.FileName) == 0)
                    {
                        // Apply
                        equipment.Tester.LoadBinningSpecSheet(filePath);
                        if (currentRecipe != null)
                        {
                            currentRecipe.BinningSpecSheetPath = filePath;
                            currentRecipe.Save();
                        }

                        this.filePath = filePath;
                        lbSetNameValue.Text = Path.GetFileNameWithoutExtension(filePath);
                        isModified = false;

                        UpdateRankSetGrid();
                    }
                    else
                    {
                        MessageBox.Show("Failed to load file.", "Error");
                    }
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.InitialDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BinningSpecSheet");
                dialog.Filter = "Binning Spec Sheet (*.csv)|*.csv|All Files (*.*)|*.*";
                dialog.Title = "Save Binning Spec Sheet";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var filePath = dialog.FileName;
                    if (tempSheet.SaveToFile(dialog.FileName) == 0)
                    {
                        // Apply
                        equipment.Tester.LoadBinningSpecSheet(filePath);
                        if (currentRecipe != null)
                        {
                            currentRecipe.BinningSpecSheetPath = filePath;
                            currentRecipe.Save();
                        }

                        this.filePath = filePath;
                        lbSetNameValue.Text = Path.GetFileNameWithoutExtension(filePath);
                        isModified = false;
                    }
                    else
                    {
                        MessageBox.Show("Failed to save file.", "Error");
                    }
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
                isModified = true;

                UpdateRankSetGrid();
            }
        }
    }
}
