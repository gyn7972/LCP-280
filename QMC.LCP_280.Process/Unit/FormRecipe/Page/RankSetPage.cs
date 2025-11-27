using QMC.Common;
using QMC.Common.PKGTester;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Unit.FormRecipe.Page
{
    public partial class RankSetPage : UserControl
    {
        private Equipment equipment => Equipment.Instance;
        private Component.MeasurementRecipe currentRecipe
        {
            get
            {
                if (DesignModeHelper.IsDesignMode(this)) return null;
                var eq = Equipment.Instance;
                return eq?.EquipmentRecipe?.CurrentRecipe;
            }
        }

        // --- Excel 기반 모델 ---
        private ExcelBinningModel excelModel = new ExcelBinningModel();
        private string excelFilePath = "";
        private bool isModified = false;

        private PropertyCollection pc;

        public RankSetPage()
        {
            InitializeComponent();
            InitRankSetGrid();
        }

        private void RankSetPage_Load(object sender, EventArgs e)
        {
            if (DesignModeHelper.IsDesignMode(this)) return;

            var recipe = currentRecipe;
            if (recipe == null) return;

            excelModel.Clear();
            excelFilePath = recipe.BinningSpecSheetFile; // 경로는 그대로 사용(확장자만 xlsx로 변경 가능)

            if (!string.IsNullOrEmpty(excelFilePath) && File.Exists(excelFilePath))
            {
                //excelModel = DataBinningExcelLoader.Load(excelFilePath) ?? new ExcelBinningModel();
                //lbSetNameValue.Text = Path.GetFileNameWithoutExtension(excelFilePath);
                if (LoadSpecFile(excelFilePath))
                    lbSetNameValue.Text = Path.GetFileNameWithoutExtension(excelFilePath);
                else
                    lbSetNameValue.Text = "No file";
            }
            else
            {
                lbSetNameValue.Text = "No file";
            }

            UpdateRankSetGridExcel();
        }

        private void RankSetPage_VisibleChanged(object sender, EventArgs e)
        {
            if (!Visible) return;
            if (DesignModeHelper.IsDesignMode(this)) return;

            var recipe = currentRecipe;
            if (recipe == null) return;

            if (excelFilePath != recipe.BinningSpecSheetFile || isModified)
            {
                excelFilePath = recipe.BinningSpecSheetFile;
                if (!string.IsNullOrEmpty(excelFilePath) && File.Exists(excelFilePath))
                {
                    //excelModel = DataBinningExcelLoader.Load(excelFilePath) ?? new ExcelBinningModel();
                    //lbSetNameValue.Text = Path.GetFileNameWithoutExtension(excelFilePath);
                    if (LoadSpecFile(excelFilePath))
                        lbSetNameValue.Text = Path.GetFileNameWithoutExtension(excelFilePath);
                    else
                        lbSetNameValue.Text = "No file";
                    UpdateRankSetGridExcel();
                }
                else
                {
                    excelModel.Clear();
                    lbSetNameValue.Text = "No file";
                    dataGridRank.Rows.Clear();
                    dataGridRank.Columns.Clear();
                }
                isModified = false;
            }
        }

        private void InitRankSetGrid()
        {
            dataGridRank.Font = new Font("맑은 고딕", 8);
            dataGridRank.EnableHeadersVisualStyles = false;
            dataGridRank.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
            dataGridRank.ColumnHeadersHeight = 60;
            dataGridRank.AllowUserToAddRows = false;
            dataGridRank.AllowUserToDeleteRows = false;
            dataGridRank.RowHeadersVisible = false;

            dataGridRank.CellPainting += dataGridRank_CellPainting;
        }

        #region Grid Build / Update

        private void UpdateRankSetGridExcel()
        {
            dataGridRank.SuspendLayout();
            dataGridRank.Columns.Clear();
            dataGridRank.Rows.Clear();

            // 왼쪽 고정 컬럼들
            dataGridRank.Columns.Add("No", "No");
            dataGridRank.Columns.Add("BIN", "BIN");
            dataGridRank.Columns.Add("Sub", "Sub");
            dataGridRank.Columns.Add("Name", "Name");
            dataGridRank.Columns.Add("OP", "OP");
            dataGridRank.Columns.Add("NG", "NG");

            foreach (DataGridViewColumn c in dataGridRank.Columns)
            {
                c.SortMode = DataGridViewColumnSortMode.NotSortable;
                c.ReadOnly = false;
            }

            // Item 컬럼들
            foreach (var key in excelModel.ItemKeys)
            {
                var col = new DataGridViewTextBoxColumn();
                col.Name = key;
                col.HeaderText = key; // 실제 헤더 텍스트는 CellPainting에서 그림
                col.ReadOnly = false;
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
                dataGridRank.Columns.Add(col);
            }

            // 행 채우기
            foreach (var bin in excelModel.Bins)
            {
                int rowIdx = dataGridRank.Rows.Add();
                var row = dataGridRank.Rows[rowIdx];

                row.Cells["No"].Value = bin.No;
                row.Cells["BIN"].Value = bin.Bin;
                row.Cells["Sub"].Value = bin.Sub;
                row.Cells["Name"].Value = bin.Name;
                row.Cells["OP"].Value = bin.Op;
                row.Cells["NG"].Value = bin.Ng;

                foreach (var key in excelModel.ItemKeys)
                {
                    if (bin.Items.TryGetValue(key, out var range) && !range.Ignore)
                        row.Cells[key].Value = $"{range.Min}~{range.Max}";
                    else
                        row.Cells[key].Value = "";
                }
            }

            dataGridRank.ResumeLayout();
        }

        #endregion

        #region Grid Events

        // 헤더 3줄(ITEM / CH1 / UNIT) 커스텀 그리기
        private void dataGridRank_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex != -1 || excelModel == null)
                return;

            string colName = dataGridRank.Columns[e.ColumnIndex].Name;

            // 기본 컬럼들은 1줄 헤더
            if (colName == "No" || colName == "BIN" || colName == "Sub" ||
                colName == "Name" || colName == "OP" || colName == "NG")
            {
                e.PaintBackground(e.CellBounds, false);

                TextRenderer.DrawText(
                    e.Graphics,
                    colName,
                    e.CellStyle.Font,
                    e.CellBounds,
                    e.CellStyle.ForeColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                e.Handled = true;
                return;
            }

            int idx = excelModel.ItemKeys.IndexOf(colName);
            if (idx < 0) return;

            e.PaintBackground(e.CellBounds, false);

            string item = excelModel.ItemDisplayNames[idx];
            string ch = "CH1";
            string unit = excelModel.ItemUnits.Count > idx ? excelModel.ItemUnits[idx] : "";

            Rectangle r1 = new Rectangle(e.CellBounds.Left, e.CellBounds.Top, e.CellBounds.Width, e.CellBounds.Height / 3);
            Rectangle r2 = new Rectangle(e.CellBounds.Left, e.CellBounds.Top + e.CellBounds.Height / 3, e.CellBounds.Width, e.CellBounds.Height / 3);
            Rectangle r3 = new Rectangle(e.CellBounds.Left, e.CellBounds.Top + (e.CellBounds.Height * 2 / 3), e.CellBounds.Width, e.CellBounds.Height / 3);

            TextRenderer.DrawText(e.Graphics, item, e.CellStyle.Font, r1, e.CellStyle.ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            TextRenderer.DrawText(e.Graphics, ch, e.CellStyle.Font, r2, Color.Black,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            TextRenderer.DrawText(e.Graphics, unit, e.CellStyle.Font, r3, Color.Black,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

            e.Handled = true;
        }

        private void dataGridRank_SelectionChanged(object sender, EventArgs e)
        {
            if (excelModel == null || dataGridRank.CurrentCell == null)
                return;

            int rowIndex = dataGridRank.CurrentCell.RowIndex;
            int colIndex = dataGridRank.CurrentCell.ColumnIndex;

            if (rowIndex < 0 || colIndex < 0)
                return;

            if (rowIndex >= excelModel.Bins.Count)
            {
                pcvEdit.SetProperties(null);
                return;
            }

            string header = dataGridRank.Columns[colIndex].Name;

            if (!excelModel.ItemKeys.Contains(header))
            {
                pcvEdit.SetProperties(null);
                return;
            }

            var binItem = excelModel.Bins[rowIndex];
            if (!binItem.Items.TryGetValue(header, out var range))
            {
                range = new BinningRange(header) { Ignore = true };
                binItem.Items[header] = range;
            }

            pc = range.GetPropertyCollection();
            pcvEdit.SetProperties(pc);
        }

        private void dataGridRank_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (excelModel == null) return;

            int rowIndex = e.RowIndex;
            int colIndex = e.ColumnIndex;
            if (rowIndex < 0 || colIndex < 0) return;
            if (rowIndex >= excelModel.Bins.Count) return;

            var bin = excelModel.Bins[rowIndex];
            string colName = dataGridRank.Columns[colIndex].Name;
            var cellVal = dataGridRank.Rows[rowIndex].Cells[colIndex].Value?.ToString() ?? "";

            switch (colName)
            {
                case "No":
                    bin.No = SafeInt(cellVal);
                    break;
                case "BIN":
                    bin.Bin = SafeInt(cellVal);
                    break;
                case "Sub":
                    bin.Sub = SafeInt(cellVal);
                    break;
                case "Name":
                    bin.Name = cellVal;
                    break;
                case "OP":
                    bin.Op = cellVal;
                    break;
                case "NG":
                    bin.Ng = cellVal;
                    break;
                default:
                    // Item Range 직접 입력 허용 (ex: "0~1")
                    if (!excelModel.ItemKeys.Contains(colName)) break;

                    if (string.IsNullOrWhiteSpace(cellVal))
                    {
                        if (bin.Items.ContainsKey(colName))
                            bin.Items[colName].Ignore = true;
                    }
                    else if (DataBinningExcelLoader.TryRange(cellVal, out double min, out double max))
                    {
                        if (!bin.Items.TryGetValue(colName, out var range))
                        {
                            range = new BinningRange(colName);
                            bin.Items[colName] = range;
                        }
                        range.Min = min;
                        range.Max = max;
                        range.Ignore = false;
                    }
                    else
                    {
                        MessageBox.Show("잘못된 Range 형식입니다. 예) 0~1", "Error");
                        dataGridRank.Rows[rowIndex].Cells[colIndex].Value = "";
                    }
                    break;
            }

            isModified = true;
        }

        #endregion

        #region Buttons

        private void btnNew_Click(object sender, EventArgs e)
        {
            excelModel.Clear();

            // Tester 결과의 헤더를 기준으로 ItemKeys 생성
            var testerHeaders = equipment?.Tester?.Result?.Items?.Keys;
            if (testerHeaders != null)
            {
                foreach (var h in testerHeaders)
                {
                    excelModel.ItemKeys.Add(h);
                    excelModel.ItemDisplayNames.Add(h);
                    excelModel.ItemUnits.Add(""); // Unit은 나중에 입력
                }
            }

            int maxRank = (int)nudMaxRank.Value;
            for (int i = 0; i < maxRank; i++)
            {
                var bin = new ExcelBinItem
                {
                    No = i + 1,
                    Bin = i + 1,
                    Sub = 0,
                    Name = $"SV700-HA-A-{(i + 1):D2}"
                };
                excelModel.Bins.Add(bin);
            }

            isModified = true;
            UpdateRankSetGridExcel();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.InitialDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Measurement", "BinningSpecSheet");
                dialog.Filter =
                        "Spec Files (*.xlsx;*.xls;*.bin)|*.xlsx;*.xls;*.bin|Excel (*.xlsx;*.xls)|*.xlsx;*.xls|BIN (*.bin)|*.bin";

                dialog.Title = "Open Excel Spec Sheet";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var path = dialog.FileName;
                    //var model = DataBinningExcelLoader.Load(path);
                    //if (model == null)
                    //{
                    //    MessageBox.Show("Failed to load excel file.", "Error");
                    //    return;
                    //}
                    if (!LoadSpecFile(path))
                    {
                        MessageBox.Show("Failed to load file.", "Error");
                        return;
                    }
                    //excelModel = model;
                    excelFilePath = path;
                    lbSetNameValue.Text = Path.GetFileNameWithoutExtension(excelFilePath);
                    isModified = false;

                    if (currentRecipe != null)
                    {
                        currentRecipe.BinningSpecSheetFile = excelFilePath;
                        currentRecipe.Save();
                    }

                    UpdateRankSetGridExcel();
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.InitialDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Measurement", "BinningSpecSheet");
                //dialog.Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*";
                dialog.Filter =
                        "Spec Files (*.xlsx;*.xls;*.bin)|*.xlsx;*.xls;*.bin|Excel (*.xlsx)|*.xlsx|BIN (*.bin)|*.bin";

                dialog.Title = "Save Excel Spec Sheet";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var path = dialog.FileName;
                    if (SaveSpecFile(path))
                    {
                        excelFilePath = path;
                        lbSetNameValue.Text = Path.GetFileNameWithoutExtension(excelFilePath);
                        isModified = false;

                        if (currentRecipe != null)
                        {
                            currentRecipe.BinningSpecSheetFile = excelFilePath;
                            currentRecipe.Save();
                        }

                        MessageBox.Show("Saved.");
                    }
                    else
                    {
                        MessageBox.Show("Failed to save spec file.", "Error");
                    }
                    //if (DataBinningExcelLoader.Save(path, excelModel) == 0)
                    //{
                    //    excelFilePath = path;
                    //    lbSetNameValue.Text = Path.GetFileNameWithoutExtension(excelFilePath);
                    //    isModified = false;

                    //    if (currentRecipe != null)
                    //    {
                    //        currentRecipe.BinningSpecSheetFile = excelFilePath;
                    //        currentRecipe.Save();
                    //    }

                    //    MessageBox.Show("Saved.");
                    //}
                    //else
                    //{
                    //    MessageBox.Show("Failed to save excel file.", "Error");
                    //}
                }
            }
        }

        private void btnMaxRankUpdate_Click(object sender, EventArgs e)
        {
            int maxRank = (int)nudMaxRank.Value;
            if (excelModel == null) return;

            if (excelModel.Bins.Count > maxRank)
            {
                excelModel.Bins.RemoveRange(maxRank, excelModel.Bins.Count - maxRank);
            }
            else
            {
                for (int i = excelModel.Bins.Count; i < maxRank; i++)
                {
                    var bin = new ExcelBinItem
                    {
                        No = i + 1,
                        Bin = i + 1,
                        Sub = 0,
                        Name = $"SV700-HA-A-{(i + 1):D2}"
                    };
                    excelModel.Bins.Add(bin);
                }
            }

            isModified = true;
            UpdateRankSetGridExcel();
        }

        private void btnModify_Click(object sender, EventArgs e)
        {
            if (excelModel == null || dataGridRank.CurrentCell == null || pc == null)
                return;

            int rowIndex = dataGridRank.CurrentCell.RowIndex;
            int colIndex = dataGridRank.CurrentCell.ColumnIndex;

            if (rowIndex < 0 || colIndex < 0 || rowIndex >= excelModel.Bins.Count)
                return;

            string key = dataGridRank.Columns[colIndex].Name;
            if (!excelModel.ItemKeys.Contains(key))
                return;

            pcvEdit.Apply();

            var tmp = new BinningRange(key);
            tmp.ApplyValueFromPropertyCollection(pc);
            if (!tmp.Validate())
            {
                MessageBox.Show("Invalid value.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var bin = excelModel.Bins[rowIndex];
            if (!bin.Items.TryGetValue(key, out var range))
            {
                range = new BinningRange(key);
                bin.Items[key] = range;
            }
            range.CopyFrom(tmp);

            isModified = true;
            UpdateRankSetGridExcel();
        }

        #endregion

        #region Util

        private int SafeInt(string s)
        {
            if (int.TryParse(s, out var v)) return v;
            return 0;
        }


        // ------------------------------------------------------------------
        // 파일 타입 자동 구분 로더
        // ------------------------------------------------------------------
        private bool LoadSpecFile(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return false;

            string ext = Path.GetExtension(path).ToLower();

            if (ext == ".xlsx" || ext == ".xls")
            {
                excelModel = DataBinningExcelLoader.Load(path) ?? new ExcelBinningModel();
                return true;
            }
            else if (ext == ".bin")
            {
                excelModel = DataBinningBinLoader.LoadBIN(path) ?? new ExcelBinningModel();
                return true;
            }

            return false;
        }

        // ------------------------------------------------------------------
        // 파일 타입 자동 구분 저장
        // ------------------------------------------------------------------
        private bool SaveSpecFile(string path)
        {
            string ext = Path.GetExtension(path).ToLower();

            if (ext == ".xlsx" || ext == ".xls")
            {
                return DataBinningExcelLoader.Save(path, excelModel) == 0;
            }
            else if (ext == ".bin")
            {
                return DataBinningBinLoader.SaveBIN(path, excelModel) == 0;
            }

            return false;
        }



        #endregion
    }
}
