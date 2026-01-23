using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using QMC.Common;
using QMC.Common.PKGTester;

public static class DataBinningBinLoader
{
    public static ExcelBinningModel LoadBIN(string binFile)
    {
        if (!File.Exists(binFile))
            return null;

        var model = new ExcelBinningModel();
        string[] lines;

        try
        {
            lines = File.ReadAllLines(binFile);
        }
        catch (Exception ex)
        {
            Log.Write(ex);
            return null;
        }

        if (lines.Length == 0)
            return model;

        // 1) Spec Header2("No,BIN") 찾기
        int header2Index = -1;
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var trimmed = line.TrimStart();
            if (trimmed.StartsWith("No,BIN", StringComparison.OrdinalIgnoreCase))
            {
                header2Index = i;
                break;
            }
        }

        int header1Index = header2Index - 1;
        if (header2Index < 0 || header1Index < 0)
            return model;

        int applyValueIndex = header2Index + 1;
        if (applyValueIndex >= lines.Length)
            return model;

        string[] header1 = SplitCsvLine(lines[header1Index]);
        string[] header2 = SplitCsvLine(lines[header2Index]);
        string[] applyRow = SplitCsvLine(lines[applyValueIndex]);

        // 2) Header2 기반 고정 컬럼 매핑
        var fixedIndex = BuildHeaderIndexMap(header2);

        int idxNo = GetIndexOrDefault(fixedIndex, "No", 0);
        int idxBin = GetIndexOrDefault(fixedIndex, "BIN", 1);
        int idxSub = GetIndexOrDefault(fixedIndex, "Sub", -1);
        int idxName = GetIndexOrDefault(fixedIndex, "Name", -1);
        int idxOp = GetIndexOrDefault(fixedIndex, "OP", -1);
        int idxNg = GetIndexOrDefault(fixedIndex, "NG", -1);

        model.HasSubColumn = (idxSub >= 0);

        // 3) 아이템 시작 위치(보통 CH1 시작)
        int itemStartIndex = FindFirstIndex(header2, "CH1");
        if (itemStartIndex < 0)
        {
            itemStartIndex = (idxNg >= 0 ? idxNg + 1 :
                              idxOp >= 0 ? idxOp + 1 :
                              idxName >= 0 ? idxName + 1 : 0);
        }

        // 4) 아이템 키/유닛 수집 (Header1 + ApplyValue는 itemStartIndex 이후 컬럼을 사용)
        model.ItemKeys.Clear();
        model.ItemDisplayNames.Clear();
        model.ItemUnits.Clear();

        var itemColumnIndices = new List<int>();

        for (int c = itemStartIndex; c < header1.Length; c++)
        {
            string key = (header1[c] ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(key))
                continue;

            model.ItemKeys.Add(key);
            model.ItemDisplayNames.Add(key);
            itemColumnIndices.Add(c);

            string unit = c < applyRow.Length ? (applyRow[c] ?? string.Empty).Trim() : string.Empty;
            model.ItemUnits.Add(unit);
        }

        // 5) 데이터 로우 파싱
        int dataRowIndex = applyValueIndex + 1;

        for (int i = dataRowIndex; i < lines.Length; i++)
        {
            string line = lines[i];
            if (string.IsNullOrWhiteSpace(line))
                continue;

            if (line.TrimStart().StartsWith("Macadams", StringComparison.OrdinalIgnoreCase))
                break;

            string[] cols = SplitCsvLine(line);
            if (cols.Length == 0)
                continue;

            string noStr = GetCol(cols, idxNo);
            string binStr = GetCol(cols, idxBin);
            string subStr = (idxSub >= 0) ? GetCol(cols, idxSub) : "";
            string nameStr = (idxName >= 0) ? GetCol(cols, idxName) : "";
            string opStr = (idxOp >= 0) ? GetCol(cols, idxOp) : "";
            string ngStr = (idxNg >= 0) ? GetCol(cols, idxNg) : "";

            if (string.IsNullOrEmpty(noStr) &&
                string.IsNullOrEmpty(binStr) &&
                string.IsNullOrEmpty(nameStr))
            {
                continue;
            }

            var binItem = new ExcelBinItem();

            if (int.TryParse(noStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out int nNo))
                binItem.No = nNo;

            if (int.TryParse(binStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out int nBin))
                binItem.Bin = nBin;

            if (idxSub >= 0 && int.TryParse(subStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out int nSub))
                binItem.Sub = nSub;
            else
                binItem.Sub = 0;

            binItem.Name = nameStr;
            binItem.Op = opStr;
            binItem.Ng = ngStr;

            for (int k = 0; k < model.ItemKeys.Count; k++)
            {
                string key = model.ItemKeys[k];
                int colIdx = itemColumnIndices[k];

                string raw = colIdx < cols.Length ? (cols[colIdx] ?? string.Empty).Trim() : string.Empty;

                if (string.IsNullOrEmpty(raw))
                {
                    if (!binItem.Items.ContainsKey(key))
                        binItem.Items[key] = new BinningRange(key) { Ignore = true };
                    continue;
                }

                if (TryParseRange(raw, out double min, out double max))
                {
                    binItem.Items[key] = new BinningRange(key)
                    {
                        Min = min,
                        Max = max,
                        Ignore = false
                    };
                }
                else
                {
                    binItem.Items[key] = new BinningRange(key) { Ignore = true };
                }
            }

            model.Bins.Add(binItem);
        }

        return model;
    }

    private static Dictionary<string, int> BuildHeaderIndexMap(string[] header2)
    {
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        if (header2 == null)
            return map;

        for (int i = 0; i < header2.Length; i++)
        {
            var name = (header2[i] ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(name))
                continue;

            if (!map.ContainsKey(name))
                map.Add(name, i);
        }

        return map;
    }

    private static int GetIndexOrDefault(Dictionary<string, int> map, string key, int defaultValue)
    {
        if (map == null)
            return defaultValue;

        int idx;
        return map.TryGetValue(key, out idx) ? idx : defaultValue;
    }

    private static int FindFirstIndex(string[] cols, string token)
    {
        if (cols == null)
            return -1;

        for (int i = 0; i < cols.Length; i++)
        {
            var s = (cols[i] ?? string.Empty).Trim();
            if (s.Equals(token, StringComparison.OrdinalIgnoreCase))
                return i;
        }

        return -1;
    }

    // ===========================================================
    // 헬퍼 함수들
    // ===========================================================
    private static string[] SplitCsvLine(string line)
    {
        return (line ?? string.Empty).Split(',');
    }

    private static string GetCol(string[] cols, int index)
    {
        if (cols == null || index < 0 || index >= cols.Length)
            return string.Empty;
        return cols[index]?.Trim() ?? string.Empty;
    }

    private static bool TryParseRange(string s, out double min, out double max)
    {
        min = max = 0;

        if (string.IsNullOrWhiteSpace(s))
            return false;

        var parts = s.Split('~');
        if (parts.Length != 2)
            return false;

        if (!double.TryParse(parts[0].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out min) &&
            !double.TryParse(parts[0].Trim(), out min))
            return false;

        if (!double.TryParse(parts[1].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out max) &&
            !double.TryParse(parts[1].Trim(), out max))
            return false;

        return true;
    }

    /// <summary>
    /// BIN 포맷으로 저장 (1차 버전: Spec 영역 중심, 헤더는 고정 포맷으로 생성)
    /// </summary>
    public static int SaveBIN(string filePath, ExcelBinningModel model)
    {
        try
        {
            using (var sw = new StreamWriter(filePath, false))
            {
                // ---- 상단 헤더 (단순 고정 포맷) ----
                sw.WriteLine("TSE_BIN_NEW_FORMAT_V1");
                sw.WriteLine($"Date,{DateTime.Now:yyyy-M-d}");
                sw.WriteLine($"Time,{DateTime.Now:HH:mm:ss}");
                sw.WriteLine("Create,QMC");
                sw.WriteLine("Reserve1,Reserve1");
                sw.WriteLine("Reserve2,Reserve2");
                sw.WriteLine("Reserve3,Reserve3");

                // 예제 BIN 처럼 "10,12,12," 구조 → (아이템수, BIN수, BIN수)로 구성
                int itemCount = model.ItemKeys.Count;
                int binCount = model.Bins.Count;
                sw.WriteLine($"{itemCount},{binCount},{binCount},");

                // 0초기화 측정 영역(예제와 동일하게 0~25까지)
                for (int i = 0; i <= 25; i++)
                {
                    sw.WriteLine($"{i},0,0,0,0,0,0,0,0,0,,");
                }

                // ---- Spec Header 1 (Item 이름) ----
                // ,,,,,,KELFS,KELDG,VR1,VF3,Watt,WD,VR,IR,VF1,DeltaVR,
                sw.Write(",,,,,,");
                foreach (var key in model.ItemKeys)
                {
                    sw.Write(key);
                    sw.Write(",");
                }
                sw.WriteLine();

                // ---- Spec Header 2 (No,BIN,Sub,Name,OP,NG,CH1...) ----
                sw.Write("No,BIN,Sub,Name,OP,NG,");
                for (int i = 0; i < itemCount; i++)
                {
                    sw.Write("CH1,");
                }
                sw.WriteLine();

                // ---- ApplyValue / Unit Row ----
                // ,,,ApplyValue,,,50.00 uA,50.00 uA,...
                sw.Write(",,,ApplyValue,,,");

                for (int i = 0; i < itemCount; i++)
                {
                    string unit = (i < model.ItemUnits.Count ? model.ItemUnits[i] : "");
                    sw.Write(unit);
                    sw.Write(",");
                }
                sw.WriteLine();

                // ---- Spec Data Rows ----
                // 1,1,,SV700-HA-A-01,,,,,0~99,2.9~3.1,255~270, ...
                foreach (var binItem in model.Bins)
                {
                    sw.Write(binItem.No.ToString(CultureInfo.InvariantCulture));
                    sw.Write(",");
                    sw.Write(binItem.Bin.ToString(CultureInfo.InvariantCulture));
                    sw.Write(",");
                    sw.Write(binItem.Sub.ToString(CultureInfo.InvariantCulture));
                    sw.Write(",");
                    sw.Write(binItem.Name ?? "");
                    sw.Write(",");
                    sw.Write(binItem.Op ?? "");
                    sw.Write(",");
                    sw.Write(binItem.Ng ?? "");
                    sw.Write(",");

                    for (int i = 0; i < itemCount; i++)
                    {
                        string key = model.ItemKeys[i];

                        if (binItem.Items.TryGetValue(key, out var range) && !range.Ignore)
                        {
                            sw.Write(string.Format(
                                CultureInfo.InvariantCulture, "{0}~{1}", range.Min, range.Max));
                        }

                        sw.Write(",");
                    }

                    sw.WriteLine();
                }

                // ---- Macadams 영역 (예제와 동일한 기본 틀) ----
                sw.WriteLine("Macadams");
                sw.WriteLine("No,Name,Use,X,Y,Angle,A,B,Step");

                // A~Z까지 26개 기본 생성
                string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                for (int i = 0; i < 26; i++)
                {
                    char name = (i < letters.Length) ? letters[i] : ('A');
                    sw.WriteLine($"{i},{name},0,0,0,0,0,0,0,0,,");
                }
            }

            return 0;
        }
        catch (Exception ex)
        {
            Log.Write(ex);
            return -1;
        }
    }

    
    
}
