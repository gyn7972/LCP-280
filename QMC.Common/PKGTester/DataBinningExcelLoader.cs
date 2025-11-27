using System;
using System.Globalization;
using System.IO;
using System.Collections.Generic;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;

namespace QMC.Common.PKGTester
{
    public static class DataBinningExcelLoader
    {
        // ================================================================
        //  Excel → Model
        // ================================================================
        public static ExcelBinningModel Load(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return null;

            IWorkbook workbook = null;

            try
            {
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var ext = Path.GetExtension(filePath).ToLowerInvariant();
                    workbook = (ext == ".xls") ? (IWorkbook)new HSSFWorkbook(fs)
                                               : (IWorkbook)new XSSFWorkbook(fs);
                }

                var ws = workbook.GetSheetAt(0);
                if (ws == null) return null;

                var model = new ExcelBinningModel();

                IRow hRow0 = ws.GetRow(0);  // No BIN Name 1 "" 2 "" ...
                IRow hRow1 = ws.GetRow(1);  // "" "" ""   KELFS "" KELDG "" ...
                IRow hRow2 = ws.GetRow(2);  // "" "" ApplyValue 50uA "" 50uA ...

                if (hRow0 == null || hRow1 == null || hRow2 == null)
                    return null;

                // -----------------------------
                // ❶ 기본 구조 해석 (고정)
                // -----------------------------
                // A:No (0), B:BIN (1), C:Name (2)
                int NO_COL = 0;
                int BIN_COL = 1;
                int NAME_COL = 2;

                // -----------------------------
                // ❷ Item 컬럼 계산
                // -----------------------------
                // 3,5,7,9,11 …이 실제 값 컬럼
                List<int> itemValueCols = new List<int>();
                int lastCol = hRow0.LastCellNum;

                for (int c = 3; c < lastCol; c += 2) // 3,5,7,9...
                {
                    string headName = GetCell(hRow1, c);
                    if (!string.IsNullOrWhiteSpace(headName) && headName != "&")
                    {
                        model.ItemKeys.Add(headName);
                        model.ItemDisplayNames.Add(headName);
                        model.ItemUnits.Add(GetCell(hRow2, c));
                        itemValueCols.Add(c);
                    }
                }

                // -----------------------------
                // ❸ 데이터 Row 읽기
                // -----------------------------
                for (int r = 3; r <= ws.LastRowNum; r++)
                {
                    IRow row = ws.GetRow(r);
                    if (row == null) continue;

                    string name = GetCell(row, NAME_COL);
                    if (string.IsNullOrWhiteSpace(name)) continue;

                    var bin = new ExcelBinItem
                    {
                        No = ParseInt(GetCell(row, NO_COL)),
                        Bin = ParseInt(GetCell(row, BIN_COL)),
                        Sub = 0,
                        Name = name,
                        Op = "",
                        Ng = ""
                    };

                    // -----------------------------
                    // Item 값들 파싱
                    // -----------------------------
                    for (int i = 0; i < itemValueCols.Count; i++)
                    {
                        int col = itemValueCols[i];
                        string raw = GetCell(row, col);

                        if (string.IsNullOrWhiteSpace(raw))
                            continue;

                        if (TryRange(raw, out double min, out double max))
                        {
                            var br = new BinningRange(model.ItemKeys[i])
                            {
                                Min = min,
                                Max = max,
                                Ignore = false
                            };
                            bin.Items[model.ItemKeys[i]] = br;
                        }
                        else
                        {
                            bin.Items[model.ItemKeys[i]] =
                                new BinningRange(model.ItemKeys[i]) { Ignore = true };
                        }
                    }

                    model.Bins.Add(bin);
                }

                return model;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return null;
            }
            finally
            {
                workbook?.Close();
            }
        }

        // ================================================================
        // Model → Excel 저장
        // ================================================================
        public static int Save(string filePath, ExcelBinningModel model)
        {
            try
            {
                IWorkbook wb = new XSSFWorkbook();
                ISheet ws = wb.CreateSheet("Binning");

                // -----------------------------------------------------------
                // Row0: No | BIN | Name | 1 | "" | 2 | "" | 3 | "" ...
                // -----------------------------------------------------------
                IRow row0 = ws.CreateRow(0);
                row0.CreateCell(0).SetCellValue("No");
                row0.CreateCell(1).SetCellValue("BIN");
                row0.CreateCell(2).SetCellValue("Name");

                int itemCount = model.ItemKeys.Count;
                for (int i = 0; i < itemCount; i++)
                {
                    int col = 3 + (i * 2);
                    row0.CreateCell(col).SetCellValue((i + 1).ToString());   // "1", "2", "3" ...
                    row0.CreateCell(col + 1).SetCellValue("");               // & 위치(공백)
                }

                // -----------------------------------------------------------
                // Row1: "" | "" | "" | KELFS | "" | KELDG | "" | VR1 | ...
                // -----------------------------------------------------------
                IRow row1 = ws.CreateRow(1);
                for (int i = 0; i < itemCount; i++)
                {
                    int col = 3 + (i * 2);
                    row1.CreateCell(col).SetCellValue(model.ItemDisplayNames[i]);  // ex) KELFS
                    row1.CreateCell(col + 1).SetCellValue("");                     // & 위치
                }

                // -----------------------------------------------------------
                // Row2: "" | "" | ApplyValue | 50uA | "" | 50uA | ""
                // -----------------------------------------------------------
                IRow row2 = ws.CreateRow(2);
                row2.CreateCell(2).SetCellValue("ApplyValue");

                for (int i = 0; i < itemCount; i++)
                {
                    int col = 3 + (i * 2);
                    row2.CreateCell(col).SetCellValue(model.ItemUnits[i]);  // ex) 50uA
                    row2.CreateCell(col + 1).SetCellValue("");
                }

                // -----------------------------------------------------------
                // Row3+: 실제 데이터
                // -----------------------------------------------------------
                int rIdx = 3;

                foreach (var b in model.Bins)
                {
                    IRow row = ws.CreateRow(rIdx++);

                    row.CreateCell(0).SetCellValue(b.No);
                    row.CreateCell(1).SetCellValue(b.Bin);
                    row.CreateCell(2).SetCellValue(b.Name);

                    for (int i = 0; i < itemCount; i++)
                    {
                        int col = 3 + (i * 2);
                        string key = model.ItemKeys[i];

                        if (b.Items.TryGetValue(key, out var br) && !br.Ignore)
                            row.CreateCell(col).SetCellValue($"{br.Min}~{br.Max}");
                        else
                            row.CreateCell(col).SetCellValue("");

                        // col+1 은 항상 "&" 위치이므로 빈칸
                        row.CreateCell(col + 1).SetCellValue("");
                    }
                }

                // 저장
                using (var fs = new FileStream(filePath, FileMode.Create))
                    wb.Write(fs);

                return 0;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return -1;
            }
        }

        // ================================================================
        // 유틸 함수
        // ================================================================
        private static string GetCell(IRow row, int col)
        {
            return row?.GetCell(col)?.ToString()?.Trim() ?? "";
        }

        private static int ParseInt(string s)
        {
            return int.TryParse(s, out var v) ? v : 0;
        }

        public static bool TryRange(string s, out double min, out double max)
        {
            min = max = 0;
            if (string.IsNullOrWhiteSpace(s)) return false;

            var sp = s.Split('~');
            if (sp.Length != 2) 
                return false;

            return double.TryParse(sp[0], NumberStyles.Any, CultureInfo.InvariantCulture, out min)
                && double.TryParse(sp[1], NumberStyles.Any, CultureInfo.InvariantCulture, out max);
        }
    }
}
