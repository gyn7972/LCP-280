using System;
using System.Collections.Generic;
using System.IO;
using QMC.Common;
using QMC.Common.PKGTester;


public static class ITFParser
{
    //public static TestConditionSet LoadFromITF(string path)
    //{
    //    var tc = new TestConditionSet();
    //    var lines = File.ReadAllLines(path);

    //    var items = new List<TestConditionItem>();
    //    TestConditionItem current = null;

    //    foreach (var raw in lines)
    //    {
    //        var line = raw.Trim();
    //        if (line.StartsWith("[TESTER_DATA]"))
    //            continue;

    //        if (line.StartsWith("Item["))
    //        {
    //            // 새로운 항목 시작
    //            current = new TestConditionItem("");
    //            items.Add(current);

    //            int idxStart = line.IndexOf('[') + 1;
    //            int idxEnd = line.IndexOf(']');
    //            current.Index = int.Parse(line.Substring(idxStart, idxEnd - idxStart));

    //            string[] parts = line.Split('=');
    //            current.ItemCode = int.Parse(parts[1]);
    //        }
    //        else if (current != null)
    //        {
    //            // Key=Value 형태
    //            if (!line.Contains("=")) continue;

    //            var kv = line.Split('=');
    //            string key = kv[0].Trim();
    //            string val = kv.Length > 1 ? kv[1].Trim() : "";

    //            switch (key)
    //            {
    //                case string s when s.StartsWith("ItemName"):
    //                    current.ItemName = val;
    //                    break;

    //                case string s when s.StartsWith("SourceValue"):
    //                    current.AppValue = ParseDouble(val);
    //                    break;

    //                case string s when s.StartsWith("SreDelay"):
    //                    current.ApplyTime = ParseDouble(val);
    //                    break;

    //                case string s when s.StartsWith("WaitTime"):
    //                    current.WaitTime = ParseDouble(val);
    //                    break;

    //                case string s when s.StartsWith("OffTime"):
    //                    current.OffTime = ParseDouble(val);
    //                    break;

    //                case string s when s.StartsWith("NplcTime"):
    //                    current.NplcTime = ParseDouble(val);
    //                    break;

    //                case string s when s.StartsWith("MeasureLow"):
    //                    current.Low = ParseDouble(val);
    //                    break;

    //                case string s when s.StartsWith("MeasureHigh"):
    //                    current.High = ParseDouble(val);
    //                    break;

    //                case string s when s.StartsWith("KeyChNo"):
    //                    current.Channel = "CH" + val;
    //                    break;

    //                case string s when s.StartsWith("SourceRange"):
    //                    current.SourceRange = int.Parse(val);
    //                    break;

    //                case string s when s.StartsWith("ItemUnit"):
    //                    current.ItemUnit = int.Parse(val);
    //                    break;
    //            }
    //        }
    //    }

    //    // Index 기준 정렬 (원본 ITF 순서 보장)
    //    items.Sort((a, b) => a.Index.CompareTo(b.Index));

    //    // TestConditionSet.Items 는 읽기 전용이므로 직접 할당 불가.
    //    // AddItem/Copy 메서드를 통해 내부 리스트에 주입.
    //    tc.ClearItems();
    //    foreach (var it in items)
    //        tc.AddItem(it);

    //    return tc;
    //    //tc.Items = items;
    //    //return tc;
    //}

    //public static void SaveToITF(string path, TestConditionSet cond)
    //{
    //    using (var sw = new StreamWriter(path))
    //    {
    //        sw.WriteLine("[TESTER_DATA]");
    //        sw.WriteLine($"ItemCount={cond.Items.Count}");

    //        for (int i = 0; i < cond.Items.Count; i++)
    //        {
    //            var it = cond.Items[i];

    //            sw.WriteLine($"Item[{i}]={it.ItemCode}");
    //            sw.WriteLine($"ItemName[{i}]={it.ItemName}");
    //            sw.WriteLine($"SourceValue[{i}]={it.AppValue}");
    //            sw.WriteLine($"SreDelay[{i}]={it.ApplyTime}");
    //            sw.WriteLine($"WaitTime[{i}]={it.WaitTime}");
    //            sw.WriteLine($"OffTime[{i}]={it.OffTime}");
    //            sw.WriteLine($"NplcTime[{i}]={it.NplcTime}");
    //            sw.WriteLine($"MeasureLow[{i}]={it.Low}");
    //            sw.WriteLine($"MeasureHigh[{i}]={it.High}");
    //            sw.WriteLine($"KeyChNo[{i}]={it.Channel.Replace("CH", "")}");
    //            sw.WriteLine($"SourceRange[{i}]={it.SourceRange}");
    //            sw.WriteLine($"ItemUnit[{i}]={it.ItemUnit}");
    //            sw.WriteLine();
    //        }
    //    }
    //}

    private static double ParseDouble(string s)
    {
        double d;
        double.TryParse(s, out d);
        return d;
    }
}
