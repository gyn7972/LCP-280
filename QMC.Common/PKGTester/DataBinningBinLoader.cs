using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using QMC.Common;
using QMC.Common.PKGTester;

public static class DataBinningBinLoader
{
    /// <summary>
    /// TSE_BIN_NEW_FORMAT_V1 텍스트 포맷 BIN 파일을 로드하여
    /// ExcelBinningModel로 변환한다.
    /// 
    /// 구조:
    ///   - 여러 줄 헤더 (TSE_BIN_NEW_FORMAT_V1, Date, Time, ...)
    ///   - 중간에 0 초기화 영역 (0,0,0,0,...)
    ///   - Spec Header 1: ,,,,,,KELFS,KELDG,VR1,VF3,...
    ///   - Spec Header 2: No,BIN,Sub,Name,OP,NG,CH1,CH1,...
    ///   - ApplyValue Row: ,,,ApplyValue,,,50.00 uA,50.00 uA,10.00 uA,...
    ///   - Spec Data Rows: 1,1,,SV700-HA-A-01,,,,,0~99,2.9~3.1,...
    ///   - 이후 Macadams 영역은 무시.
    /// </summary>
    public static ExcelBinningModel LoadBIN(string binFile)
    {
        if (!File.Exists(binFile))
            return null;

        var model = new ExcelBinningModel();
        string[] lines;

        try
        {
            // 인코딩은 기본(보통 ANSI/UTF-8)으로 충분
            lines = File.ReadAllLines(binFile);
        }
        catch (Exception ex)
        {
            Log.Write(ex);
            return null;
        }

        if (lines.Length == 0)
            return model;

        // 1) Spec Header 1 (KELFS, KELDG, VR1...) 라인 찾기
        int specHeaderIndex = -1;
        for (int i = 0; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            // KELFS 키워드 포함되는 줄
            if (lines[i].IndexOf("KELFS", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                specHeaderIndex = i;
                break;
            }
        }

        if (specHeaderIndex < 0)
        {
            // Spec 영역 없음
            return model;
        }

        // -----------------------------
        // 헤더 1: Item Key 라인
        // -----------------------------
        string[] header1 = SplitCsvLine(lines[specHeaderIndex]);
        var itemColumnIndices = new List<int>();

        model.ItemKeys.Clear();
        model.ItemDisplayNames.Clear();
        model.ItemUnits.Clear();

        for (int c = 0; c < header1.Length; c++)
        {
            string name = header1[c].Trim();
            if (string.IsNullOrEmpty(name))
                continue;

            // 예: KELFS, KELDG, VR1...
            model.ItemKeys.Add(name);
            model.ItemDisplayNames.Add(name);
            itemColumnIndices.Add(c);
        }

        // -----------------------------
        // 헤더 2: No,BIN,Sub,Name,OP,NG,CH1,CH1,...
        // (실제 파싱에는 크게 필요 없으므로 스킵)
        // -----------------------------
        int header2Index = specHeaderIndex + 1;
        if (header2Index >= lines.Length)
            return model;

        //string[] header2 = SplitCsvLine(lines[header2Index]);

        // -----------------------------
        // ApplyValue / Unit 줄
        // ,,,ApplyValue,,,50.00 uA,50.00 uA,10.00 uA,...
        // -----------------------------
        int applyValueIndex = specHeaderIndex + 2;
        if (applyValueIndex >= lines.Length)
            return model;

        string[] applyRow = SplitCsvLine(lines[applyValueIndex]);

        // ItemUnits 채우기
        foreach (int colIdx in itemColumnIndices)
        {
            string unit = colIdx < applyRow.Length ? applyRow[colIdx].Trim() : string.Empty;
            model.ItemUnits.Add(unit);
        }

        // -----------------------------
        // Spec Data Rows
        // -----------------------------
        int dataRowIndex = specHeaderIndex + 3;

        for (int i = dataRowIndex; i < lines.Length; i++)
        {
            string line = lines[i];

            if (string.IsNullOrWhiteSpace(line))
                continue;

            // Macadams 이후는 Spec과 무관
            if (line.TrimStart().StartsWith("Macadams", StringComparison.OrdinalIgnoreCase))
                break;

            string[] cols = SplitCsvLine(line);
            if (cols.Length == 0)
                continue;

            // No,BIN,Sub,Name,OP,NG,...
            string noStr = GetCol(cols, 0);
            string binStr = GetCol(cols, 1);
            string subStr = GetCol(cols, 2);
            string nameStr = GetCol(cols, 3);
            string opStr = GetCol(cols, 4);
            string ngStr = GetCol(cols, 5);

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

            if (int.TryParse(subStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out int nSub))
                binItem.Sub = nSub;

            binItem.Name = nameStr;
            binItem.Op = opStr;
            binItem.Ng = ngStr;

            // 각 Item Range 파싱
            for (int k = 0; k < model.ItemKeys.Count; k++)
            {
                string key = model.ItemKeys[k];
                int colIdx = itemColumnIndices[k];

                string raw = colIdx < cols.Length ? cols[colIdx].Trim() : string.Empty;

                if (string.IsNullOrEmpty(raw))
                {
                    // 값 비어있으면 Ignore
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
                    // 파싱 실패 → Ignore
                    binItem.Items[key] = new BinningRange(key) { Ignore = true };
                }
            }

            model.Bins.Add(binItem);
        }

        return model;
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

    // ===========================================================
    // 헬퍼 함수들
    // ===========================================================
    private static string[] SplitCsvLine(string line)
    {
        // 단순 CSV (따옴표 없는 구조)라 , 기준 split만으로 충분.
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
}
