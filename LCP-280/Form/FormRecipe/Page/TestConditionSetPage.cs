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

        private bool _isUpdatingGrid = false; // 그리드 갱신 중 이벤트 가드

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
            { "UseTotalGain", "Use Total Gain" },
            { "UseTotalOffset", "Use Total Offset" },
            { "TotalGain", "Total Gain" },
            { "TotalOffset", "Total Offset" },
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

            // [추가] 다중 선택 및 키보드 입력 허용
            dataGrid.MultiSelect = true;
            dataGrid.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;

            // 키보드 이벤트 연결 (기존에 연결되어 있지 않다면 추가)
            dataGrid.KeyDown -= DataGrid_KeyDown; // 중복 방지
            dataGrid.KeyDown += DataGrid_KeyDown;


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
                if (pd.Name == "Unit")
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

        private void DataGrid_KeyDown(object sender, KeyEventArgs e)
        {
            // Ctrl + A : 전체 선택
            if (e.Control && e.KeyCode == Keys.A)
            {
                dataGrid.SelectAll();
                e.Handled = true;
            }
            // Ctrl + C : 복사
            else if (e.Control && e.KeyCode == Keys.C)
            {
                CopyToClipboard();
                e.Handled = true;
            }
            // Ctrl + V : 붙여넣기
            else if (e.Control && e.KeyCode == Keys.V)
            {
                PasteFromClipboard();
                e.Handled = true;
            }
        }

        private void CopyToClipboard()
        {
            DataObject dataObj = dataGrid.GetClipboardContent();
            if (dataObj != null)
                Clipboard.SetDataObject(dataObj);
        }

        private void PasteFromClipboard()
        {
            try
            {
                string s = Clipboard.GetText();
                if (string.IsNullOrEmpty(s)) return;

                // 엑셀 데이터는 탭(\t)과 줄바꿈(\r\n)으로 구분됨
                string[] lines = s.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length == 0) return;

                int startRow = 0;
                int startCol = 0;

                // 현재 선택된 셀이 있다면 거기서부터 시작, 아니면 (0,0)
                if (dataGrid.CurrentCell != null)
                {
                    startRow = dataGrid.CurrentCell.RowIndex;
                    startCol = dataGrid.CurrentCell.ColumnIndex;
                }

                // 데이터 변경 여부 플래그
                bool localModified = false;

                // 붙여넣을 데이터 루프
                for (int i = 0; i < lines.Length; i++)
                {
                    int targetRowIndex = startRow + i;

                    // [수정됨] 행이 모자라면 추가하지 않고 중단하거나 건너뜀
                    if (targetRowIndex >= tempSet.Items.Count)
                        break; // 더 이상 붙여넣을 행이 없으므로 루프 종료

                    string[] cells = lines[i].Split('\t');
                    var item = tempSet.Items[targetRowIndex];
                    var props = TypeDescriptor.GetProperties(item);

                    for (int j = 0; j < cells.Length; j++)
                    {
                        int targetColIndex = startCol + j;

                        // 컬럼 인덱스가 그리드 범위를 벗어나면 해당 행의 나머지 데이터는 무시
                        if (targetColIndex >= dataGrid.Columns.Count) break;

                        // 그리드 컬럼 이름으로 프로퍼티 찾기
                        string colName = dataGrid.Columns[targetColIndex].Name;
                        string valueStr = cells[j].Trim();

                        // Name 컬럼은 보통 Key이므로 변경을 막고 싶다면 아래 주석 해제
                        // if (colName == "Name") continue;

                        PropertyDescriptor pd = props.Find(colName, false);

                        // ReadOnly가 아니면 값 설정 시도
                        if (pd != null && !pd.IsReadOnly)
                        {
                            try
                            {
                                // "-" 나 빈 값 등은 무시
                                if (valueStr == "-" || string.IsNullOrEmpty(valueStr))
                                    continue;

                                // 타입 변환 (String -> Target Type)
                                var converter = TypeDescriptor.GetConverter(pd.PropertyType);
                                if (converter != null && converter.CanConvertFrom(typeof(string)))
                                {
                                    object newValue = converter.ConvertFrom(valueStr);
                                    pd.SetValue(item, newValue);
                                    localModified = true;
                                }
                            }
                            catch (Exception)
                            {
                                // 변환 실패 시 무시 (ex: 숫자가 와야 하는데 문자열 옴)
                            }
                        }
                    }
                }

                if (localModified)
                {
                    isModified = true;
                    UpdateConditionSetGrid(); // 그리드 UI 갱신
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Paste Error: {ex.Message}");
            }
        }

        private void ClearConditionSetGrid()
        {
            dataGrid.Rows.Clear();
        }

        private void UpdateConditionSetGrid()
        {
            _isUpdatingGrid = true;
            try
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
                        selectedName = tempSet.Items[prevRowIndex].Name;
                    }
                }
                try 
                { 
                    firstDisplayedRow = dataGrid.FirstDisplayedScrollingRowIndex; 
                } 
                catch (Exception ex)
                {
                    Log.Write(ex);
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
                        row.Cells[pdName.Name].Value = pdName.GetValue(item)?.ToString();

                    foreach (PropertyDescriptor pd in pdcol)
                    {
                        if (pd.Name == "Name") 
                            continue;

                        if (!dataGrid.Columns.Contains(pd.Name)) 
                            continue;

                        switch (item.GetTestItemCategory())
                        {
                            case TestItemCategory.Electrical:
                                if (pd.Name.Contains("Expression"))
                                    row.Cells[pd.Name].Value = "-";
                                else
                                {
                                    object value = pd.GetValue(item);
                                    row.Cells[pd.Name].Value = (value is Array && !(value is string)) ? "[values]" : value?.ToString();
                                }
                                break;

                            case TestItemCategory.Optical:
                                if (pd.Name.Contains("Source") || (pd.Name.Contains("Measure") && !(pd.Name == "MeasureLow" || pd.Name == "MeasureHigh")) || pd.Name.Contains("Expression"))
                                    row.Cells[pd.Name].Value = "-";
                                else
                                {
                                    object value = pd.GetValue(item);
                                    row.Cells[pd.Name].Value = (value is Array && !(value is string)) ? "[values]" : value?.ToString();
                                }
                                break;

                            case TestItemCategory.UserDefined:
                                if (pd.Name == "Type" || pd.Name == "Expression" || pd.Name == "MeasureLow" || pd.Name == "MeasureHigh")
                                    row.Cells[pd.Name].Value = pd.GetValue(item)?.ToString();
                                else
                                    row.Cells[pd.Name].Value = "-";
                                break;

                            default:
                                if (pd.Name == "Type")
                                    row.Cells[pd.Name].Value = pd.GetValue(item)?.ToString();
                                else
                                    row.Cells[pd.Name].Value = "-";
                                break;
                        }
                    }
                }

                // 3) 선택 복구
                DataGridViewCell cellToSelect = null;
                if (!string.IsNullOrEmpty(selectedName) && dataGrid.Rows.Count > 0)
                {
                    foreach (DataGridViewRow r in dataGrid.Rows)
                    {
                        var nameCellValue = r.Cells["Name"].Value?.ToString();
                        if (nameCellValue == selectedName)
                        {
                            cellToSelect = (selectedColIndex >= 0 && selectedColIndex < dataGrid.ColumnCount)
                                ? r.Cells[selectedColIndex]
                                : r.Cells[0];
                            break;
                        }
                    }
                }
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
                    if (!dataGrid.Focused) dataGrid.Focus();
                }

                // 4) 스크롤 복구
                if (firstDisplayedRow >= 0 && firstDisplayedRow < dataGrid.Rows.Count)
                {
                    try { dataGrid.FirstDisplayedScrollingRowIndex = firstDisplayedRow; } catch { }
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            finally
            {
                _isUpdatingGrid = false;
            }

            // 5) 최종 선택 기준으로 pcvItem 갱신(그리드 갱신 중 이벤트 무시했으므로 수동 동기화)
            TrySyncPropertyViewFromSelection();

        }

        private void dataGrid_SelectionChanged(object sender, EventArgs e)
        {
            if (_isUpdatingGrid) 
                return; // 그리드 갱신 중 발생한 이벤트 무시

            TrySyncPropertyViewFromSelection();
        }

        // 선택된 행을 기준으로 pcvItem에 속성 바인딩
        private void TrySyncPropertyViewFromSelection()
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

        public void ReloadFromRecipe(bool showErrorMessage = false)
        {
            if (IsDesignMode()) return;

            var recipe = currentRecipe;
            if (recipe == null) return;

            var path = recipe.TestConditionSetFile;

            try
            {
                if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                {
                    // 레시피 경로가 비었거나 파일이 없으면 UI Clear
                    filePath = "";
                    lbSetNameValue.Text = "";
                    isModified = false;
                    ClearConditionSetGrid();
                    return;
                }

                if (tempSet.LoadFromFile(path) == 0)
                {
                    // Apply(런타임에도 즉시 반영)
                    try { equipment.Tester.LoadTestConditionSet(path); } catch { }

                    filePath = path;
                    lbSetNameValue.Text = Path.GetFileNameWithoutExtension(filePath);
                    isModified = false;

                    UpdateConditionSetGrid();
                }
                else
                {
                    if (showErrorMessage)
                        MessageBox.Show("Failed to load file.", "Error");

                    filePath = "";
                    lbSetNameValue.Text = "";
                    isModified = false;
                    ClearConditionSetGrid();
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                if (showErrorMessage)
                    MessageBox.Show("Failed to load file.\n" + ex.Message, "Error");
            }
        }



    }
}