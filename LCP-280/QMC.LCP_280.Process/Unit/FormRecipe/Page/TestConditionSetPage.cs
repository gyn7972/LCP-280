using System;
using System.Collections.Generic;
using System.ComponentModel;
//using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using QMC.Common;
using QMC.Common.PKGTester;

namespace QMC.LCP_280.Process.Unit
{
    public partial class TestConditionSetPage : UserControl
    {
        private TestConditionSet tempSet = new TestConditionSet("");
        private TestConditionItem clipboardItem;
        private PropertyCollection pcItem;

        public TestConditionSetPage()
        {
            InitializeComponent();
            InitiaizeConditionSetGrid();

            tempSet.ItemsChanged += TempSet_ItemsChanged;
        }

        private void TempSet_ItemsChanged(object sender)
        {
            lbSetNameValue.Text = tempSet.Name;
            UpdateConditionSetGrid();
        }

        private void InitiaizeConditionSetGrid()
        {
            dataGrid.Font = new Font("맑은 고딕", 8);

            dataGrid.Columns.Clear();
            var pdcol = TypeDescriptor.GetProperties(new TestConditionItem(""));
            
            // Name first
            PropertyDescriptor pdName = pdcol.Find("Name", false);
            if (pdName != null)
            {
                DataGridViewColumn col = new DataGridViewTextBoxColumn();
                col.Name = pdName.Name;
                col.HeaderText = pdName.DisplayName;
                col.ReadOnly = true;
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
                dataGrid.Columns.Add(col);
            }

            foreach (PropertyDescriptor pd in pdcol)
            {
                if (pd.Name == "Name")
                    continue;

                DataGridViewColumn col = new DataGridViewTextBoxColumn();
                col.Name = pd.Name;
                col.HeaderText = pd.DisplayName;
                col.ReadOnly = true;
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
                dataGrid.Columns.Add(col);
            }
        }

        private void ClearConditionSet()
        {
            dataGrid.Rows.Clear();
        }

        private void UpdateConditionSetGrid()
        {
            ClearConditionSet();
            foreach (var item in tempSet.Items)
            {
                int index = dataGrid.Rows.Add();
                DataGridViewRow row = dataGrid.Rows[index];

                var pdcol = TypeDescriptor.GetProperties(item);

                // Name first
                PropertyDescriptor pdName = pdcol.Find("Name", false);
                if (pdName != null)
                {
                    row.Cells[pdName.Name].Value = pdName.GetValue(item)?.ToString();
                }

                foreach (PropertyDescriptor pd in pdcol)
                {
                    if (pd.Name == "Name")
                        continue;

                    if (dataGrid.Columns.Contains(pd.Name))
                    {
                        switch (item.GetTestItemCategory())
                        {
                            case TestItemCategory.Electrical:
                                {
                                    if (pd.Name.Contains("Pulse"))
                                    {
                                        row.Cells[pd.Name].Value = "-";
                                    }
                                    else
                                    {
                                        row.Cells[pd.Name].Value = pd.GetValue(item)?.ToString();
                                    }       
                                }
                                break;
                            case TestItemCategory.ElectricalSource:
                                {
                                    if (pd.Name.Contains("Source") || pd.Name.Contains("Measure"))
                                    {
                                        if (pd.Name == "SourceValue")
                                        {
                                            row.Cells[pd.Name].Value = pd.GetValue(item)?.ToString();
                                        }
                                        else
                                        {
                                            row.Cells[pd.Name].Value = "-";
                                        }
                                    }
                                    else
                                    {
                                        row.Cells[pd.Name].Value = pd.GetValue(item)?.ToString();
                                    }
                                }
                                break;
                            case TestItemCategory.Optical:
                                {
                                    if (pd.Name.Contains("Source") || pd.Name.Contains("Measure") || pd.Name.Contains("Pulse"))
                                    {
                                        row.Cells[pd.Name].Value = "-";
                                    }
                                    else
                                    {
                                        row.Cells[pd.Name].Value = pd.GetValue(item)?.ToString();
                                    }
                                }
                                break;
                            default:
                                {
                                    if (pd.Name == "Type")
                                    {
                                        row.Cells[pd.Name].Value = pd.GetValue(item)?.ToString();
                                    }
                                    else
                                    {
                                        row.Cells[pd.Name].Value = "-";
                                    }
                                }
                                break;
                        }
                    }
                }
            }
        }

        private void dataGrid_SelectionChanged(object sender, EventArgs e)
        {
            int selectIndex = dataGrid.CurrentCell.RowIndex;

            pcItem = null;
            if (selectIndex >= 0 && selectIndex < tempSet.Items.Count)
            {
                pcItem = tempSet.Items[selectIndex].GetPropertyCollection();
            }

            pcvItem.SetProperties(pcItem);
        }

        private void btnItemInsert_Click(object sender, EventArgs e)
        {
            TestConditionItem newItem = new TestConditionItem("NewItem");
            tempSet.AddItem(newItem);
        }

        private void btnItemDelete_Click(object sender, EventArgs e)
        {
            if (dataGrid.CurrentCell == null)
            {
                MessageBox.Show("No item selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (dataGrid.CurrentCell.RowIndex < 0 || dataGrid.CurrentCell.RowIndex >= tempSet.Items.Count)
            {
                MessageBox.Show("Invalid item selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int selectIndex = dataGrid.CurrentCell.RowIndex;
            tempSet.RemoveItemAt(selectIndex);
        }

        private void btnItemCopy_Click(object sender, EventArgs e)
        {
            if (dataGrid.CurrentCell == null)
            {
                MessageBox.Show("No item selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (dataGrid.CurrentCell.RowIndex < 0 || dataGrid.CurrentCell.RowIndex >= tempSet.Items.Count)
            {
                MessageBox.Show("Invalid item selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int selectIndex = dataGrid.CurrentCell.RowIndex;
            clipboardItem = tempSet.Items[selectIndex].Clone() as TestConditionItem;
        }

        private void btnItemPaste_Click(object sender, EventArgs e)
        {
            if (dataGrid.CurrentCell == null)
            {
                MessageBox.Show("No item selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (dataGrid.CurrentCell.RowIndex < 0 || dataGrid.CurrentCell.RowIndex >= tempSet.Items.Count)
            {
                MessageBox.Show("Invalid item selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int selectIndex = dataGrid.CurrentCell.RowIndex;
            if (clipboardItem != null)
            {
                TestConditionItem newItem = clipboardItem.Clone() as TestConditionItem;
                newItem.Name += "_Copy";
                tempSet.InsertItem(selectIndex, newItem);
            }
            else
            {
                MessageBox.Show("Clipboard is empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnItemUp_Click(object sender, EventArgs e)
        {
            if (dataGrid.CurrentCell == null)
            {
                MessageBox.Show("No item selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (dataGrid.CurrentCell.RowIndex < 0 || dataGrid.CurrentCell.RowIndex >= tempSet.Items.Count)
            {
                MessageBox.Show("Invalid item selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int selectIndex = dataGrid.CurrentCell.RowIndex;
            if (selectIndex > 0)
            {
                tempSet.MoveItemUp(selectIndex);
                dataGrid.CurrentCell = dataGrid.Rows[selectIndex - 1].Cells[0];
            }
        }

        private void btnItemDown_Click(object sender, EventArgs e)
        {
            if (dataGrid.CurrentCell == null)
            {
                MessageBox.Show("No item selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (dataGrid.CurrentCell.RowIndex < 0 || dataGrid.CurrentCell.RowIndex >= tempSet.Items.Count)
            {
                MessageBox.Show("Invalid item selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int selectIndex = dataGrid.CurrentCell.RowIndex;
            if (selectIndex < tempSet.Items.Count - 1)
            {
                tempSet.MoveItemDown(selectIndex);
                dataGrid.CurrentCell = dataGrid.Rows[selectIndex + 1].Cells[0];
            }
        }

        private void btnItemClear_Click(object sender, EventArgs e)
        {
            if (dataGrid.CurrentCell == null)
            {
                MessageBox.Show("No item selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (dataGrid.CurrentCell.RowIndex < 0 || dataGrid.CurrentCell.RowIndex >= tempSet.Items.Count)
            {
                MessageBox.Show("Invalid item selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int selectIndex = dataGrid.CurrentCell.RowIndex;
            tempSet.Items[selectIndex].Reset();
            UpdateConditionSetGrid();
        }

        private void btnItemModify_Click(object sender, EventArgs e)
        {
            if (dataGrid.CurrentCell == null)
            {
                MessageBox.Show("No item selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (dataGrid.CurrentCell.RowIndex < 0 || dataGrid.CurrentCell.RowIndex >= tempSet.Items.Count)
            {
                MessageBox.Show("Invalid item selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (pcItem == null)
            {
                MessageBox.Show("No item selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                int selectIndex = dataGrid.CurrentCell.RowIndex; 
                tempSet.Items[selectIndex].ApplyValueFromPropertyCollection(pcItem);
                UpdateConditionSetGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        private void btnNewSet_Click(object sender, EventArgs e)
        {
            tempSet.ClearItems();
        }

        private void btnOpenSet_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
                openFileDialog.Title = "Open Test Condition Set";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    var filePath = openFileDialog.FileName;
                    if (tempSet.LoadFromFile(filePath) != 0)
                    {
                        MessageBox.Show("Failed to load the test condition set.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnSaveSet_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
                saveFileDialog.Title = "Save Test Condition Set";
                saveFileDialog.FileName = tempSet.Name + ".json";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    var filePath = saveFileDialog.FileName;
                    if (tempSet.SaveToFile(filePath) != 0)
                    {
                        MessageBox.Show("Failed to save the test condition set.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    Equipment.Instance.Tester.LoadTestConditionSet(tempSet);
                    lbSetNameValue.Text = tempSet.Name;
                }
            }
        }
    }
}
