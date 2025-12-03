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
        // 디자인 모드 보호 + 널 안전 접근
        private Component.MeasurementRecipe currentRecipe
        {
            get
            {
                if (IsDesignMode()) return null;
                var eq = Equipment.Instance;
                return eq?.EquipmentRecipe?.CurrentRecipe;
            }
        }

        private TestConditionSet tempSet = new TestConditionSet(); // 임시 변수
        private TestConditionItem clipboardItem;
        private PropertyCollection pcItem;

        private string filePath = "";
        private bool isModified = false;

        private Dictionary<string, string> headerConv = new Dictionary<string, string>()
        {
            // { ItemProperty, ItemHeader }
            { "Name", "Name" },
            { "Type", "Type" },
            { "SourceValue", "App Value" },
            { "SourceTime", "Apply Time" },
            { "WaitTime", "Wait Time" },
            { "OffTime", "Off Time" },
            { "MeasureTime", "Measure Time" },
            { "MeasureLimit", "Measure Limit" },
            { "MeasureLow", "Low" },
            { "MeasureHigh", "High" },
            { "Expression", "Expression" },
            { "UseGain", "Use Gain" },
            { "UseOffset", "Use Offset" },
            { "Gain", "Gain" },
            { "Offset", "Offset" },
        };

        public TestConditionSetPage()
        {
            InitializeComponent();
            InitiaizeConditionSetGrid();
        }

        private bool IsDesignMode()
        {
            return LicenseManager.UsageMode == LicenseUsageMode.Designtime
                   || (this.Site?.DesignMode ?? false)
                   || this.DesignMode;
        }

        private void TestConditionSetPage_Load(object sender, EventArgs e)
        {
            if (IsDesignMode()) return;

            var recipe = currentRecipe;
            if (recipe != null)
            {
                if (tempSet.LoadFromFile(recipe.TestConditionSetFile) == 0)
                {
                    filePath = recipe.TestConditionSetFile;
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
            if (IsDesignMode()) return;

            // 수정 후 저장하지 않고 다른 Page로 이동 시, 적용되지 않은 상태로 유지하기 위함
            if (Visible)
            {
                var recipe = currentRecipe;
                if (recipe != null)
                {
                    if (filePath != recipe.TestConditionSetFile || isModified)
                    {
                        if (tempSet.LoadFromFile(recipe.TestConditionSetFile) == 0)
                        {
                            filePath = recipe.TestConditionSetFile;
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
                //col.HeaderText = pdName.DisplayName;
                col.HeaderText = headerConv[pdName.Name];
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
                //col.HeaderText = pd.DisplayName;
                col.HeaderText = headerConv[pd.Name];
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
            // 1) 기존 선택 정보 보존
            string selectedName = null;
            int selectedColIndex = 0;
            int prevRowIndex = -1;
            int firstDisplayedRow = -1;
            if (dataGrid.CurrentCell != null)
            {
                prevRowIndex = dataGrid.CurrentCell.RowIndex;
                selectedColIndex = dataGrid.CurrentCell.ColumnIndex;
                if (prevRowIndex >= 0 && prevRowIndex < tempSet.Items.Count)
                {
                    // 아이템의 고유 식별자로 Name 사용
                    selectedName = tempSet.Items[prevRowIndex].Name;
                }
            }
            try
            {
                firstDisplayedRow = dataGrid.FirstDisplayedScrollingRowIndex;
            }
            catch
            {
                firstDisplayedRow = -1;
            }

            // 2) 그리드 재구성
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

                    if (!dataGrid.Columns.Contains(pd.Name))
                        continue;

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
                                    if (value is Array && !(value is string))
                                        row.Cells[pd.Name].Value = "[values]";
                                    else
                                        row.Cells[pd.Name].Value = value?.ToString();
                                }
                            }
                            break;
                        case TestItemCategory.Optical:
                            {
                                if (pd.Name.Contains("Source") || (pd.Name.Contains("Measure") && !(pd.Name == "MeasureLow" || pd.Name == "MeasureHigh")) || pd.Name.Contains("Expression"))
                                {
                                    row.Cells[pd.Name].Value = "-";
                                }
                                else
                                {
                                    object value = pd.GetValue(item);
                                    if (value is Array && !(value is string))
                                        row.Cells[pd.Name].Value = "[values]";
                                    else
                                        row.Cells[pd.Name].Value = value?.ToString();
                                }
                            }
                            break;
                        case TestItemCategory.UserDefined:
                            {
                                if (pd.Name == "Type" || pd.Name == "Expression" || pd.Name == "MeasureLow" || pd.Name == "MeasureHigh")
                                    row.Cells[pd.Name].Value = pd.GetValue(item)?.ToString();
                                else
                                    row.Cells[pd.Name].Value = "-";
                            }
                            break;
                        default:
                            {
                                if (pd.Name == "Type")
                                    row.Cells[pd.Name].Value = pd.GetValue(item)?.ToString();
                                else
                                    row.Cells[pd.Name].Value = "-";
                            }
                            break;
                    }
                }
            }

            // 3) 선택 복구 로직
            DataGridViewCell cellToSelect = null;

            if (!string.IsNullOrEmpty(selectedName) && dataGrid.Rows.Count > 0)
            {
                foreach (DataGridViewRow r in dataGrid.Rows)
                {
                    // Name 열은 초기화 시 항상 존재하도록 구성됨
                    var nameCellValue = r.Cells["Name"].Value?.ToString();
                    if (nameCellValue == selectedName)
                    {
                        // 기존 열 인덱스가 범위를 벗어나면 첫 열로 폴백
                        if (selectedColIndex >= 0 && selectedColIndex < dataGrid.ColumnCount)
                            cellToSelect = r.Cells[selectedColIndex];
                        else
                            cellToSelect = r.Cells[0];
                        break;
                    }
                }
            }

            // 삭제되었거나 못 찾은 경우 이전 행 인덱스 기반 폴백
            if (cellToSelect == null && dataGrid.Rows.Count > 0)
            {
                int fallbackRow = prevRowIndex;
                if (fallbackRow >= dataGrid.Rows.Count) fallbackRow = dataGrid.Rows.Count - 1;
                if (fallbackRow < 0) fallbackRow = 0;
                cellToSelect = dataGrid.Rows[fallbackRow].Cells[0];
            }

            if (cellToSelect != null)
            {
                dataGrid.CurrentCell = cellToSelect;
                // 포커스 유지 (필요 시)
                if (!dataGrid.Focused)
                    dataGrid.Focus();
            }

            // 4) 스크롤 위치 복구 (가능한 경우)
            if (firstDisplayedRow >= 0 && firstDisplayedRow < dataGrid.Rows.Count)
            {
                try
                {
                    dataGrid.FirstDisplayedScrollingRowIndex = firstDisplayedRow;
                }
                catch
                {
                    // 일부 상황(행이 적어짐 등)에서는 예외 가능 → 무시
                }
            }
        }

        //private void UpdateConditionSetGrid()
        //{
        //    ClearConditionSetGrid();
        //    foreach (var item in tempSet.Items)
        //    {
        //        int index = dataGrid.Rows.Add();
        //        DataGridViewRow row = dataGrid.Rows[index];

        //        var pdcol = TypeDescriptor.GetProperties(item);

        //        // Name first
        //        PropertyDescriptor pdName = pdcol.Find("Name", false);
        //        if (pdName != null)
        //        {
        //            row.Cells[pdName.Name].Value = pdName.GetValue(item)?.ToString();
        //        }

        //        foreach (PropertyDescriptor pd in pdcol)
        //        {
        //            if (pd.Name == "Name")
        //                continue;

        //            if (dataGrid.Columns.Contains(pd.Name))
        //            {
        //                switch (item.GetTestItemCategory())
        //                {
        //                    case TestItemCategory.Electrical:
        //                        {
        //                            if (pd.Name.Contains("Expression"))
        //                            {
        //                                row.Cells[pd.Name].Value = "-";
        //                            }
        //                            else
        //                            {
        //                                object value = pd.GetValue(item);

        //                                if (value is Array arr && !(value is string))
        //                                {
        //                                    row.Cells[pd.Name].Value = "[values]";
        //                                }
        //                                else
        //                                {
        //                                    row.Cells[pd.Name].Value = pd.GetValue(item)?.ToString();
        //                                }
        //                            }   
        //                        }
        //                        break;
        //                    case TestItemCategory.Optical:
        //                        {
        //                            if (pd.Name.Contains("Source") || pd.Name.Contains("Measure") || pd.Name.Contains("Expression"))
        //                            {
        //                                row.Cells[pd.Name].Value = "-";
        //                            }
        //                            else
        //                            {
        //                                object value = pd.GetValue(item);

        //                                if (value is Array arr && !(value is string))
        //                                {
        //                                    row.Cells[pd.Name].Value = "[values]";
        //                                }
        //                                else
        //                                {
        //                                    row.Cells[pd.Name].Value = pd.GetValue(item)?.ToString();
        //                                }
        //                            }
        //                        }
        //                        break;
        //                    case TestItemCategory.UserDefined:
        //                        {
        //                            if (pd.Name == "Type" || pd.Name == "Expression")
        //                            {
        //                                row.Cells[pd.Name].Value = pd.GetValue(item)?.ToString();
        //                            }
        //                            else
        //                            {
        //                                row.Cells[pd.Name].Value = "-";
        //                            }
        //                        }
        //                        break;
        //                    default:
        //                        {
        //                            if (pd.Name == "Type")
        //                            {
        //                                row.Cells[pd.Name].Value = pd.GetValue(item)?.ToString();
        //                            }
        //                            else
        //                            {
        //                                row.Cells[pd.Name].Value = "-";
        //                            }
        //                        }
        //                        break;
        //                }
        //            }
        //        }
        //    }
        //}

        private void dataGrid_SelectionChanged(object sender, EventArgs e)
        {
            var currentCell = dataGrid.CurrentCell;
            pcItem = null;

            if (currentCell != null)
            {
                int selectIndex = currentCell.RowIndex;
                if (selectIndex >= 0 && selectIndex < tempSet.Items.Count)
                {
                    pcItem = tempSet.Items[selectIndex].GetPropertyCollection();
                }
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
                if (IsDesignMode()) return;

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
                            currentRecipe.TestConditionSetFile = filePath;
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
            if (IsDesignMode()) 
                return;

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
                            currentRecipe.TestConditionSetFile = filePath;
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