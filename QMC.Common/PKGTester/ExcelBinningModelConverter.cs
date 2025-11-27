using System;
using System.Linq;

namespace QMC.Common.PKGTester
{
    public static class ExcelBinningModelConverter
    {
        // ExcelBinningModel → BinningSpecSheet
        public static BinningSpecSheet ToSpecSheet(ExcelBinningModel excel, Func<ExcelBinItem, string> labelSelector = null)
        {
            var sheet = new BinningSpecSheet();
            if (excel == null) return sheet;

            foreach (var key in excel.ItemKeys)
                sheet.AddHeader(key);

            foreach (var row in excel.Bins)
            {
                if (row == null) continue;

                string label = labelSelector != null
                    ? labelSelector(row)
                    : (!string.IsNullOrWhiteSpace(row.Name) ? row.Name : $"BIN{row.Bin}");

                if (!sheet.AddNewBin(label))
                    continue;

                var spec = sheet.Specs.FirstOrDefault(s => s.BinLabel == label);
                if (spec == null) continue;

                foreach (var header in sheet.Headers)
                {
                    if (!spec.Items.ContainsKey(header)) continue;

                    if (row.Items.TryGetValue(header, out var r))
                        spec.Items[header].CopyFrom(r);
                    else
                        spec.Items[header].Ignore = true;
                }
            }

            return sheet;
        }

        // BinningSpecSheet → ExcelBinningModel (선택)
        public static ExcelBinningModel ToExcelModel(BinningSpecSheet sheet)
        {
            var model = new ExcelBinningModel();
            if (sheet == null) return model;

            foreach (var h in sheet.Headers)
            {
                model.ItemKeys.Add(h);
                model.ItemDisplayNames.Add(h);
                model.ItemUnits.Add("");
            }

            int n = 1;
            foreach (var spec in sheet.Specs)
            {
                var bin = new ExcelBinItem
                {
                    No = n,
                    Bin = n,
                    Sub = 0,
                    Name = spec.BinLabel,
                    Op = "",
                    Ng = ""
                };

                foreach (var key in model.ItemKeys)
                {
                    if (spec.Items.TryGetValue(key, out var r))
                    {
                        bin.Items[key] = new BinningRange(key)
                        {
                            Min = r.Min,
                            Max = r.Max,
                            Ignore = r.Ignore
                        };
                    }
                    else
                    {
                        bin.Items[key] = new BinningRange(key) { Ignore = true };
                    }
                }

                model.Bins.Add(bin);
                n++;
            }

            return model;
        }
    }
}