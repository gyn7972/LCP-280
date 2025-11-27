using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using QMC.Common.PKGTester;

namespace QMC.LCP_280.Process.Unit
{
    public partial class ltfTestConditionSetPage : UserControl
    {
        private ItfTesterData itfData = new ItfTesterData();
        private ItfItem clipboardItem;
        private string filePath = "";
        private bool isModified = false;

        private Equipment equipment => Equipment.Instance;
        private Component.MeasurementRecipe currentRecipe
        {
            get
            {
                if (IsDesignMode()) return null;
                return Equipment.Instance?.EquipmentRecipe?.CurrentRecipe;
            }
        }

        public ltfTestConditionSetPage()
        {
            InitializeComponent();
            InitializeItemGrid();
        }

        private bool IsDesignMode()
        {
            return LicenseManager.UsageMode == LicenseUsageMode.Designtime
                   || (Site?.DesignMode ?? false)
                   || DesignMode;
        }

        private void TestConditionSetPage_Load(object sender, EventArgs e)
        {
            if (IsDesignMode()) return;
            TryLoadFromRecipe();
        }

        private void TestConditionSetPage_VisibleChanged(object sender, EventArgs e)
        {
            if (IsDesignMode()) return;
            if (Visible)
            {
                var recipe = currentRecipe;
                if (recipe != null)
                {
                    if (filePath != recipe.TestConditionSetFile || isModified)
                        TryLoadFromRecipe();
                }
            }
        }

        private void TryLoadFromRecipe()
        {
            var recipe = currentRecipe;
            if (recipe == null || string.IsNullOrWhiteSpace(recipe.TestConditionSetFile) || !File.Exists(recipe.TestConditionSetFile))
            {
                filePath = "";
                lbSetNameValue.Text = "";
                itfData = new ItfTesterData();
                RebuildGrid();
                return;
            }

            try
            {
                itfData = ItfTesterData.Load(recipe.TestConditionSetFile);
                filePath = recipe.TestConditionSetFile;
                lbSetNameValue.Text = Path.GetFileNameWithoutExtension(filePath);
                isModified = false;
                RebuildGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ITF 로드 실패: {ex.Message}", "Error");
                filePath = "";
                lbSetNameValue.Text = "";
                itfData = new ItfTesterData();
                RebuildGrid();
            }
        }

        #region Grid 초기화
        private void InitializeItemGrid()
        {
            dataGrid.Font = new Font("맑은 고딕", 8);
            dataGrid.AllowUserToAddRows = false;
            dataGrid.AllowUserToDeleteRows = false;
            dataGrid.MultiSelect = false;
            dataGrid.SelectionMode = DataGridViewSelectionMode.CellSelect;
            dataGrid.EditMode = DataGridViewEditMode.EditOnEnter;

            BuildColumns();
            dataGrid.SelectionChanged += dataGrid_SelectionChanged;
            dataGrid.CellValueChanged += dataGrid_CellValueChanged;
            dataGrid.CurrentCellDirtyStateChanged += dataGrid_CurrentCellDirtyStateChanged;
        }

        private void BuildColumns()
        {
            dataGrid.Columns.Clear();

            AddColumn("No", "No", 50, readOnly: true);
            AddColumn("Item", "Item", 120);
            AddColumn("AppValue", "App Value", 70);
            AddColumn("ApplyTime", "Apply Time", 70);
            AddColumn("WaitTime", "Wait Time", 70);
            AddColumn("OffTime", "Off Time", 70);
            AddColumn("NplcTime", "Nplc Time", 70);
            AddColumn("Low", "Low", 70);
            AddColumn("High", "High", 70);
            AddColumn("AppRange", "App Range", 120, readOnly: true);
            AddColumn("CH", "CH", 60);
        }

        private void AddColumn(string name, string header, int width, bool readOnly = false)
        {
            var col = new DataGridViewTextBoxColumn
            {
                Name = name,
                HeaderText = header,
                Width = width,
                ReadOnly = readOnly,
                SortMode = DataGridViewColumnSortMode.NotSortable
            };
            dataGrid.Columns.Add(col);
        }
        #endregion

        #region Grid 재구성
        private void RebuildGrid()
        {
            string selectedItem = null;
            int selectedCol = 0;
            int prevRow = -1;
            int firstDisplayed = -1;

            if (dataGrid.CurrentCell != null)
            {
                prevRow = dataGrid.CurrentCell.RowIndex;
                selectedCol = dataGrid.CurrentCell.ColumnIndex;
                if (prevRow >= 0 && prevRow < itfData.Items.Count)
                    selectedItem = itfData.Items[prevRow].ItemName;
                try { firstDisplayed = dataGrid.FirstDisplayedScrollingRowIndex; } catch { firstDisplayed = -1; }
            }

            dataGrid.Rows.Clear();
            for (int i = 0; i < itfData.Items.Count; i++)
            {
                var it = itfData.Items[i];
                int r = dataGrid.Rows.Add();
                var row = dataGrid.Rows[r];
                FillRow(row, i, it);
            }

            DataGridViewCell toSelect = null;
            if (!string.IsNullOrEmpty(selectedItem))
            {
                foreach (DataGridViewRow rr in dataGrid.Rows)
                {
                    if (rr.Cells["Item"].Value?.ToString() == selectedItem)
                    {
                        toSelect = rr.Cells[selectedCol < dataGrid.ColumnCount ? selectedCol : 0];
                        break;
                    }
                }
            }
            if (toSelect == null && dataGrid.Rows.Count > 0)
            {
                int fallback = prevRow;
                if (fallback >= dataGrid.Rows.Count) fallback = dataGrid.Rows.Count - 1;
                if (fallback < 0) fallback = 0;
                toSelect = dataGrid.Rows[fallback].Cells[0];
            }
            if (toSelect != null)
            {
                dataGrid.CurrentCell = toSelect;
                if (!dataGrid.Focused) dataGrid.Focus();
            }
            if (firstDisplayed >= 0 && firstDisplayed < dataGrid.Rows.Count)
            {
                try { dataGrid.FirstDisplayedScrollingRowIndex = firstDisplayed; } catch { }
            }
        }

        private void FillRow(DataGridViewRow row, int index, ItfItem it)
        {
            row.Cells["No"].Value = index + 1;
            row.Cells["Item"].Value = it.ItemName;
            row.Cells["AppValue"].Value = Format(it.SourceValue);
            row.Cells["ApplyTime"].Value = Format(it.SreDelay);
            row.Cells["WaitTime"].Value = Format(it.WaitTime);
            row.Cells["OffTime"].Value = Format(it.OffTime);
            row.Cells["NplcTime"].Value = Format(it.NplcTime);
            row.Cells["Low"].Value = Format(it.MeasureLow);
            row.Cells["High"].Value = Format(it.MeasureHigh);
            row.Cells["AppRange"].Value = BuildRangeText(it);
            row.Cells["CH"].Value = it.KeyChNo.HasValue ? $"CH {it.KeyChNo.Value}" : "";
        }

        private string BuildRangeText(ItfItem it)
        {
            // 우선순위: FullRange / 2nd Range / MeasureLow~High
            if (it.FullRangeMin.HasValue && it.FullRangeMax.HasValue)
                return $"{it.FullRangeMin.Value:G} - {it.FullRangeMax.Value:G}";
            if (it.SecondRangeMin.HasValue && it.SecondRangeMax.HasValue)
                return $"{it.SecondRangeMin.Value:G} - {it.SecondRangeMax.Value:G}";
            if (it.MeasureLow.HasValue && it.MeasureHigh.HasValue)
                return $"{it.MeasureLow.Value:G} - {it.MeasureHigh.Value:G}";
            return "";
        }

        private string Format(double? v) => v.HasValue ? v.Value.ToString("G", CultureInfo.InvariantCulture) : "";
        #endregion

        #region Cell 편집 반영
        private void dataGrid_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dataGrid.IsCurrentCellDirty)
                dataGrid.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private void dataGrid_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= itfData.Items.Count) return;
            var it = itfData.Items[e.RowIndex];
            var cell = dataGrid.Rows[e.RowIndex].Cells[e.ColumnIndex];
            string col = dataGrid.Columns[e.ColumnIndex].Name;
            string text = cell.Value?.ToString();

            try
            {
                switch (col)
                {
                    case "Item":
                        it.ItemName = text;
                        break;
                    case "AppValue":
                        it.SourceValue = ParseDouble(text);
                        break;
                    case "ApplyTime":
                        it.SreDelay = ParseDouble(text);
                        break;
                    case "WaitTime":
                        it.WaitTime = ParseDouble(text);
                        break;
                    case "OffTime":
                        it.OffTime = ParseDouble(text);
                        break;
                    case "NplcTime":
                        it.NplcTime = ParseDouble(text);
                        break;
                    case "Low":
                        it.MeasureLow = ParseDouble(text);
                        UpdateRangeIfNeeded(it);
                        break;
                    case "High":
                        it.MeasureHigh = ParseDouble(text);
                        UpdateRangeIfNeeded(it);
                        break;
                    case "CH":
                        // "CH 1" 또는 숫자만 허용
                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            text = text.Trim().ToUpperInvariant().Replace("CH", "").Trim();
                            int ch;
                            if (int.TryParse(text, out ch))
                                it.KeyChNo = ch;
                            else
                                it.KeyChNo = null;
                        }
                        else
                        {
                            it.KeyChNo = null;
                        }
                        break;
                }
                isModified = true;
                // 읽기전용 셀 갱신(AppRange, No 등)
                FillRow(dataGrid.Rows[e.RowIndex], e.RowIndex, it);
            }
            catch
            {
                // 무시, 사용자가 잘못 입력 시 이전 값 재표시
                FillRow(dataGrid.Rows[e.RowIndex], e.RowIndex, it);
            }
        }

        private void UpdateRangeIfNeeded(ItfItem it)
        {
            // 별도 로직 필요시 (현재는 MeasureLow/High 값만 AppRange 대체 용)
        }

        private double? ParseDouble(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            double v;
            return double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out v) ? (double?)v : null;
        }
        #endregion

        #region SelectionChanged
        private void dataGrid_SelectionChanged(object sender, EventArgs e)
        {
            // 필요 시 확장 (현재는 아무것도 안 함)
        }
        #endregion

        #region 버튼 동작 (Items)
        private void btnItemInsert_Click(object sender, EventArgs e)
        {
            var newItem = new ItfItem
            {
                ItemName = "NewItem" + (itfData.Items.Count + 1),
                SourceValue = 0,
                SreDelay = 0,
                WaitTime = 0,
                OffTime = 0,
                NplcTime = 0.1,
                MeasureLow = null,
                MeasureHigh = null
            };
            itfData.AddItem(newItem);
            isModified = true;
            RebuildGrid();
        }

        private void btnItemDelete_Click(object sender, EventArgs e)
        {
            var cell = dataGrid.CurrentCell;
            if (cell == null) return;
            int idx = cell.RowIndex;
            if (idx < 0 || idx >= itfData.Items.Count) return;
            itfData.RemoveItemAt(idx);
            isModified = true;
            RebuildGrid();
        }

        private void btnItemCopy_Click(object sender, EventArgs e)
        {
            var cell = dataGrid.CurrentCell;
            if (cell == null) return;
            int idx = cell.RowIndex;
            if (idx < 0 || idx >= itfData.Items.Count) return;
            clipboardItem = CloneItem(itfData.Items[idx]);
        }

        private void btnItemPaste_Click(object sender, EventArgs e)
        {
            if (clipboardItem == null) return;
            var cell = dataGrid.CurrentCell;
            int idx = cell != null ? cell.RowIndex : itfData.Items.Count;
            if (idx < 0 || idx > itfData.Items.Count) idx = itfData.Items.Count;
            var clone = CloneItem(clipboardItem);
            clone.ItemName = clone.ItemName + "_Copy";
            itfData.InsertItem(idx, clone);
            isModified = true;
            RebuildGrid();
        }

        private void btnItemUp_Click(object sender, EventArgs e)
        {
            var cell = dataGrid.CurrentCell;
            if (cell == null) return;
            int idx = cell.RowIndex;
            if (idx <= 0) return;
            itfData.MoveItemUp(idx);
            isModified = true;
            RebuildGrid();
            dataGrid.CurrentCell = dataGrid.Rows[idx - 1].Cells[1];
        }

        private void btnItemDown_Click(object sender, EventArgs e)
        {
            var cell = dataGrid.CurrentCell;
            if (cell == null) return;
            int idx = cell.RowIndex;
            if (idx < 0 || idx >= itfData.Items.Count - 1) return;
            itfData.MoveItemDown(idx);
            isModified = true;
            RebuildGrid();
            dataGrid.CurrentCell = dataGrid.Rows[idx + 1].Cells[1];
        }

        private void btnItemClear_Click(object sender, EventArgs e)
        {
            var cell = dataGrid.CurrentCell;
            if (cell == null) return;
            int idx = cell.RowIndex;
            if (idx < 0 || idx >= itfData.Items.Count) return;
            var it = itfData.Items[idx];
            it.SourceValue = null;
            it.SreDelay = null;
            it.WaitTime = null;
            it.OffTime = null;
            it.NplcTime = null;
            it.MeasureLow = null;
            it.MeasureHigh = null;
            it.KeyChNo = null;
            isModified = true;
            FillRow(dataGrid.Rows[idx], idx, it);
        }

        private void btnItemModify_Click(object sender, EventArgs e)
        {
            // Grid 직접 편집 반영 이미 적용. 강제 새로고침만 수행.
            isModified = true;
            RebuildGrid();
        }

        private ItfItem CloneItem(ItfItem src)
        {
            if (src == null) return null;
            return new ItfItem
            {
                Index = src.Index,
                RawItemCode = src.RawItemCode,
                ItemName = src.ItemName,
                ItemName2 = src.ItemName2,
                SourceValue = src.SourceValue,
                SreDelay = src.SreDelay,
                WaitTime = src.WaitTime,
                OffTime = src.OffTime,
                NplcTime = src.NplcTime,
                MeasureLow = src.MeasureLow,
                MeasureHigh = src.MeasureHigh,
                MeasureLimit = src.MeasureLimit,
                KeyChNo = src.KeyChNo,
                WaveCount = src.WaveCount,
                IntegrationTime = src.IntegrationTime,
                EsdMode = src.EsdMode,
                EsdCount = src.EsdCount,
                VfStep = src.VfStep,
                WaveItemWP = src.WaveItemWP == null ? null : new ItfWaveMetric
                {
                    Name = src.WaveItemWP.Name,
                    LowLevel = src.WaveItemWP.LowLevel,
                    HighLevel = src.WaveItemWP.HighLevel,
                    Use = src.WaveItemWP.Use
                },
                CalcItems = src.CalcItems != null ? src.CalcItems.ToList() : null
            };
        }
        #endregion

        #region 파일 동작
        private void btnNewSet_Click(object sender, EventArgs e)
        {
            filePath = "";
            lbSetNameValue.Text = "";
            itfData = new ItfTesterData();
            isModified = true;
            RebuildGrid();
        }

        private void btnOpenSet_Click(object sender, EventArgs e)
        {
            if (IsDesignMode()) return;
            using (var dialog = new OpenFileDialog())
            {
                dialog.InitialDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Measurement", "TestConditionSet");
                dialog.Filter = "ITF files (*.itf)|*.itf|All files (*.*)|*.*";
                dialog.Title = "Open ITF File";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        itfData = ItfTesterData.Load(dialog.FileName);
                        filePath = dialog.FileName;
                        lbSetNameValue.Text = Path.GetFileNameWithoutExtension(filePath);
                        if (currentRecipe != null)
                        {
                            currentRecipe.TestConditionSetFile = filePath;
                            currentRecipe.Save();
                        }
                        isModified = false;
                        RebuildGrid();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"로드 실패: {ex.Message}", "Error");
                    }
                }
            }
        }

        private void btnSaveSet_Click(object sender, EventArgs e)
        {
            if (IsDesignMode()) return;
            using (var dialog = new SaveFileDialog())
            {
                dialog.InitialDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Measurement", "TestConditionSet");
                dialog.Filter = "ITF files (*.itf)|*.itf|All files (*.*)|*.*";
                dialog.Title = "Save ITF File";
                dialog.FileName = string.IsNullOrEmpty(filePath) ? "NewFile.itf" : Path.GetFileName(filePath);

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    int rc = itfData.Save(dialog.FileName, preserveUnknownKeys: true);
                    if (rc == 0)
                    {
                        filePath = dialog.FileName;
                        lbSetNameValue.Text = Path.GetFileNameWithoutExtension(filePath);
                        if (currentRecipe != null)
                        {
                            currentRecipe.TestConditionSetFile = filePath;
                            currentRecipe.Save();
                        }
                        isModified = false;
                        MessageBox.Show("저장 완료", "Info");
                    }
                    else
                    {
                        MessageBox.Show("저장 실패", "Error");
                    }
                }
            }
        }
        #endregion
    }
}