using System;
using System.Collections.Generic;
using System.ComponentModel;
//using System.Data;
using System.Drawing;
using System.IO;
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
        private Equipment equipment => Equipment.Instance;
        private Component.MeasurementRecipe currentRecipe => equipment.EquipmentRecipe.CurrentRecipe;

        private TestConditionSet tempSet = new TestConditionSet(); // 임시 변수
        private TestConditionItem clipboardItem;
        private PropertyCollection pcItem;

        private string filePath = "";
        private bool isModified = false;

        public TestConditionSetPage()
        {
            InitializeComponent();
            InitiaizeConditionSetGrid();
        }

        private void TestConditionSetPage_Load(object sender, EventArgs e)
        {
            if (currentRecipe != null)
            {
                if (tempSet.LoadFromFile(currentRecipe.TestConditionSetPath) == 0)
                {                     
                    filePath = currentRecipe.TestConditionSetPath;
                    lbSetNameValue.Text = Path.GetFileNameWithoutExtension(filePath);
                    UpdateConditionSetGrid();
                }
                else
                {
                    filePath = "";
                    lbSetNameValue.Text = "";
                    ClearConditionSetGrid();
                }
            }
        }

        private void TestConditionSetPage_VisibleChanged(object sender, EventArgs e)
        {
            // 수정 후 저장하지 않고 다른 Page로 이동 시, 적용되지 않은 상태로 유지하기 위함
            if (Visible)
            {
                if (currentRecipe != null)
                {
                    if (filePath != currentRecipe.TestConditionSetPath || isModified)
                    {
                        if (tempSet.LoadFromFile(currentRecipe.TestConditionSetPath) == 0)
                        {
                            filePath = currentRecipe.TestConditionSetPath;
                            lbSetNameValue.Text = Path.GetFileNameWithoutExtension(filePath);
                            UpdateConditionSetGrid();
                        }
                        else
                        {
                            filePath = "";
                            lbSetNameValue.Text = "";
                            ClearConditionSetGrid();
                        }
                    }
                }
            }
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

        private void ClearConditionSetGrid()
        {
            dataGrid.Rows.Clear();
        }

        private void UpdateConditionSetGrid()
        {
            ClearConditionSetGrid();
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
                                    if (pd.Name.Contains("Expression"))
                                    {
                                        row.Cells[pd.Name].Value = "-";
                                    }
                                    else
                                    {
                                        object value = pd.GetValue(item);

                                        if (value is Array arr && !(value is string))
                                        {
                                            row.Cells[pd.Name].Value = "[values]";
                                        }
                                        else
                                        {
                                            row.Cells[pd.Name].Value = pd.GetValue(item)?.ToString();
                                        }
                                    }   
                                }
                                break;
                            case TestItemCategory.Optical:
                                {
                                    if (pd.Name.Contains("Source") || pd.Name.Contains("Measure") || pd.Name.Contains("Expression"))
                                    {
                                        row.Cells[pd.Name].Value = "-";
                                    }
                                    else
                                    {
                                        object value = pd.GetValue(item);

                                        if (value is Array arr && !(value is string))
                                        {
                                            row.Cells[pd.Name].Value = "[values]";
                                        }
                                        else
                                        {
                                            row.Cells[pd.Name].Value = pd.GetValue(item)?.ToString();
                                        }
                                    }
                                }
                                break;
                            case TestItemCategory.UserDefined:
                                {
                                    if (pd.Name == "Type" || pd.Name == "Expression")
                                    {
                                        row.Cells[pd.Name].Value = pd.GetValue(item)?.ToString();
                                    }
                                    else
                                    {
                                        row.Cells[pd.Name].Value = "-";
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
            isModified = true;

            UpdateConditionSetGrid();
        }

        private void btnItemDelete_Click(object sender, EventArgs e)
        {
            var currentCell = dataGrid.CurrentCell;
            if (currentCell == null || currentCell.RowIndex < 0 || currentCell.RowIndex >= tempSet.Items.Count)
            {
                return;
            }

            int selectIndex = currentCell.RowIndex;
            tempSet.RemoveItemAt(selectIndex);
            isModified = true;

            UpdateConditionSetGrid();
        }

        private void btnItemCopy_Click(object sender, EventArgs e)
        {
            var currentCell = dataGrid.CurrentCell;
            if (currentCell == null || currentCell.RowIndex < 0 || currentCell.RowIndex >= tempSet.Items.Count)
            {
                return;
            }

            int selectIndex = currentCell.RowIndex;
            clipboardItem = tempSet.Items[selectIndex].Clone() as TestConditionItem;
        }

        private void btnItemPaste_Click(object sender, EventArgs e)
        {
            var currentCell = dataGrid.CurrentCell;
            if (currentCell == null || currentCell.RowIndex < 0 || currentCell.RowIndex >= tempSet.Items.Count)
            {
                return;
            }
            if (clipboardItem == null)
            {
                return;
            }

            int selectIndex = currentCell.RowIndex;
            TestConditionItem newItem = clipboardItem.Clone() as TestConditionItem;
            newItem.Name += "_Copy";
            tempSet.InsertItem(selectIndex, newItem);
            isModified = true;

            UpdateConditionSetGrid();
        }

        private void btnItemUp_Click(object sender, EventArgs e)
        {
            var currentCell = dataGrid.CurrentCell;
            if (currentCell == null || currentCell.RowIndex < 0 || currentCell.RowIndex >= tempSet.Items.Count)
            {
                return;
            }

            int selectIndex = currentCell.RowIndex;
            if (selectIndex > 0)
            {
                tempSet.MoveItemUp(selectIndex);
                dataGrid.CurrentCell = dataGrid.Rows[selectIndex - 1].Cells[0];
                isModified = true;

                UpdateConditionSetGrid();
            }
        }

        private void btnItemDown_Click(object sender, EventArgs e)
        {
            var currentCell = dataGrid.CurrentCell;
            if (currentCell == null || currentCell.RowIndex < 0 || currentCell.RowIndex >= tempSet.Items.Count)
            {
                return;
            }

            int selectIndex = currentCell.RowIndex;
            if (selectIndex < tempSet.Items.Count - 1)
            {
                tempSet.MoveItemDown(selectIndex);
                dataGrid.CurrentCell = dataGrid.Rows[selectIndex + 1].Cells[0];
                isModified = true;

                UpdateConditionSetGrid();
            }
        }

        private void btnItemClear_Click(object sender, EventArgs e)
        {
            var currentCell = dataGrid.CurrentCell;
            if (currentCell == null || currentCell.RowIndex < 0 || currentCell.RowIndex >= tempSet.Items.Count)
            {
                return;
            }

            int selectIndex = currentCell.RowIndex;
            tempSet.Items[selectIndex].Reset();
            isModified = true;

            UpdateConditionSetGrid();
        }

        private void btnItemModify_Click(object sender, EventArgs e)
        {
            var currentCell = dataGrid.CurrentCell;
            if (currentCell == null || currentCell.RowIndex < 0 || currentCell.RowIndex >= tempSet.Items.Count)
            {
                return;
            }
            if (pcItem == null)
            {
                return;
            }

            try
            {
                pcvItem.Apply();

                int selectIndex = currentCell.RowIndex; 
                tempSet.Items[selectIndex].ApplyValueFromPropertyCollection(pcItem);
                isModified = true;

                UpdateConditionSetGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "Error");
                return;
            }
        }

        private void btnNewSet_Click(object sender, EventArgs e)
        {
            filePath = "";
            lbSetNameValue.Text = "";
            ClearConditionSetGrid();

            isModified = true;

            UpdateConditionSetGrid();
        }

        private void btnOpenSet_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.InitialDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Measurement", "TestConditionSet");
                dialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
                dialog.Title = "Open Test Condition Set";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var filePath = dialog.FileName;
                    if (tempSet.LoadFromFile(filePath) == 0)
                    {
                        // Apply
                        equipment.Tester.LoadTestConditionSet(filePath);
                        if (currentRecipe != null)
                        {
                            currentRecipe.TestConditionSetPath = filePath;
                            currentRecipe.Save();
                        }

                        this.filePath = filePath;
                        lbSetNameValue.Text = Path.GetFileNameWithoutExtension(filePath);
                        isModified = false;

                        UpdateConditionSetGrid();
                    }
                    else
                    {
                        MessageBox.Show("Failed to load file.", "Error");
                    }
                }
            }
        }

        private void btnSaveSet_Click(object sender, EventArgs e)
        {
            if (!tempSet.Validate())
            {
                MessageBox.Show("Test condition set is not valid.", "Error");
                return;
            }

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.InitialDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Measurement", "TestConditionSet");
                dialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
                dialog.Title = "Save Test Condition Set";
                dialog.FileName = "*.json";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var filePath = dialog.FileName;
                    if (tempSet.SaveToFile(filePath) == 0)
                    {
                        // Apply
                        equipment.Tester.LoadTestConditionSet(filePath);
                        if (currentRecipe != null)
                        {
                            currentRecipe.TestConditionSetPath = filePath;
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
    }
}