using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Component
{
    public partial class ProcessDataManual : Form
    {
        private string _configPath;
        private bool _initialized;

        public ProcessDataManual()
        {
            InitializeComponent();
        }

        private void ProcessDataManual_Load(object sender, EventArgs e)
        {
            try
            {
                _configPath = GetDefaultConfigPath();
                txtConfigPath.Text = _configPath;

                // Defaults for quick test
                if (string.IsNullOrWhiteSpace(txtCarrierId.Text)) txtCarrierId.Text = "CARRIER_MANUAL";
                if (nudSlot.Value < 0) nudSlot.Value = 0;
                if (string.IsNullOrWhiteSpace(txtLotId.Text)) txtLotId.Text = $"LOT{DateTime.Now:yyyyMMdd}";
                if (string.IsNullOrWhiteSpace(txtRecipeKeys.Text)) txtRecipeKeys.Text = "Brightness,Resistance,Defect";

                // Lazy initialize on first action; or do it now for convenience
                EnsureInitialized();
                RefreshChipList();
            }
            catch (Exception ex)
            {
                SetStatus($"Load error: {ex.Message}");
            }
        }

        private string GetDefaultConfigPath()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            return System.IO.Path.Combine(baseDir, "Configs", "WaferData", "WaferDataConfig.json");
        }

        private void EnsureInitialized()
        {
            if (_initialized) return;
            try
            {
                WaferManager.Instance.Initialize(_configPath);
                _initialized = true;
                SetStatus("WaferManager initialized.");
            }
            catch (Exception ex)
            {
                SetStatus($"Init failed: {ex.Message}");
            }
        }

        private void btnInit_Click(object sender, EventArgs e)
        {
            _configPath = string.IsNullOrWhiteSpace(txtConfigPath.Text) ? GetDefaultConfigPath() : txtConfigPath.Text;
            EnsureInitialized();
        }

        private void btnLoadNewLot_Click(object sender, EventArgs e)
        {
            try
            {
                EnsureInitialized();
                var carrierId = txtCarrierId.Text?.Trim();
                var slot = (int)nudSlot.Value;
                var lotId = txtLotId.Text?.Trim();
                if (string.IsNullOrEmpty(carrierId) || string.IsNullOrEmpty(lotId))
                {
                    SetStatus("CarrierId/LotId required.");
                    return;
                }
                var wafer = WaferManager.Instance.LoadNewLot(carrierId, slot, lotId);
                SetStatus($"Loaded new lot: {wafer.WaferId} at {wafer.CarrierId}[{wafer.SlotIndex}]");
                RefreshChipList();
            }
            catch (Exception ex)
            {
                SetStatus($"Load lot error: {ex.Message}");
            }
        }

        private void btnSetRecipeKeys_Click(object sender, EventArgs e)
        {
            try
            {
                EnsureInitialized();
                var carrierId = txtCarrierId.Text?.Trim();
                var slot = (int)nudSlot.Value;
                var keys = ParseRecipeKeys();
                if (keys.Count == 0)
                {
                    SetStatus("No recipe keys.");
                    return;
                }
                WaferManager.Instance.SetRecipeKeys(carrierId, slot, keys, force: true);
                SetStatus($"Recipe keys set: {string.Join(",", keys)}");
                RefreshChipList();
            }
            catch (Exception ex)
            {
                SetStatus($"Set keys error: {ex.Message}");
            }
        }

        private void btnAddSampleChips_Click(object sender, EventArgs e)
        {
            try
            {
                EnsureInitialized();
                var wafer = CurrentWafer();
                if (wafer == null)
                {
                    SetStatus("No wafer. Load lot first.");
                    return;
                }
                lock (wafer.Dies)
                {
                    // Center-based coordinates: (0,0) is wafer center.
                    // Create 5 chips around center: (0,0), (-1,0), (1,0), (0,-1), (0,1)
                    var coords = new (int x, int y)[] { (0, 0), (-1, 0), (1, 0), (0, -1), (0, 1) };
                    int startIdx = wafer.Dies.Count > 0 ? wafer.Dies.Max(c => c.Index) + 1 : 0;
                    int added = 0;
                    foreach (var (x, y) in coords)
                    {
                        var idx = startIdx + added;
                        wafer.AddChip(idx, x, y);
                        added++;
                    }
                    SetStatus($"Added {added} center-based chips. Total={wafer.Dies.Count}");
                    RefreshChipList();
                }
            }
            catch (Exception ex)
            {
                SetStatus($"Add chips error: {ex.Message}");
            }
        }

        private void btnSimulateInspect_Click(object sender, EventArgs e)
        {
            try
            {
                EnsureInitialized();
                var wafer = CurrentWafer();
                if (wafer == null)
                {
                    SetStatus("No wafer. Load lot first.");
                    return;
                }
                lock (wafer.Dies)
                {
                    if (lvChips.SelectedItems.Count == 0 && wafer.Dies.Count == 0)
                    {
                        SetStatus("No chips to inspect.");
                        return;
                    }
                    int index = 0;
                    if (lvChips.SelectedItems.Count > 0)
                    {
                        int.TryParse(lvChips.SelectedItems[0].SubItems[0].Text, out index);
                    }
                    var chip = wafer.GetChipByIndex(index) ?? wafer.Dies.FirstOrDefault();
                    if (chip == null)
                    {
                        SetStatus("Chip not found.");
                        return;
                    }

                    // Simulate inspect
                    chip.State = DieProcessState.Inspecting;

                    var keys = wafer.RecipeKeys ?? new List<string>();
                    if (keys.Count == 0)
                    {
                        keys = new List<string> { "Brightness", "Resistance", "Defect" };
                    }

                    var rnd = new Random(index + Environment.TickCount);
                    foreach (var k in keys)
                    {
                        double v;
                        if (string.Equals(k, "Brightness", StringComparison.OrdinalIgnoreCase))
                            v = 100 + rnd.NextDouble() * 50; // 100~150
                        else if (string.Equals(k, "Resistance", StringComparison.OrdinalIgnoreCase))
                            v = (index % 2 == 0) ? 0.04 + rnd.NextDouble() * 0.01 : 0.06 + rnd.NextDouble() * 0.02; // pass/fail mix
                        else if (string.Equals(k, "Defect", StringComparison.OrdinalIgnoreCase))
                            v = (index % 3 == 0) ? 1.0 : 0.0;
                        else
                            v = rnd.NextDouble() * 100;
                        chip.AddMeasure(k, v);
                    }

                    // Simple rule: pass if Resistance <= 0.05
                    var r = chip.GetMeasure("Resistance");
                    bool pass = !r.HasValue || r.Value <= 0.05;
                    chip.IsPass = pass;
                    chip.State = pass ? DieProcessState.Inspected : DieProcessState.Rejected;

                    // Assume placed to unloader (simulation)
                    chip.TargetWaferId = "UNLOAD01";
                    chip.TargetSlot = 0;
                    chip.TargetChipIndex = chip.Index;

                    SetStatus($"Chip {chip.Index} => {(chip.IsPass ? "PASS" : "FAIL")} ({chip.State})");
                    RefreshChipList();
                }
            }
            catch (Exception ex)
            {
                SetStatus($"Simulate error: {ex.Message}");
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                EnsureInitialized();
                WaferManager.Instance.Save();
                SetStatus("Saved.");
            }
            catch (Exception ex)
            {
                SetStatus($"Save error: {ex.Message}");
            }
        }

        private void lvChips_SelectedIndexChanged(object sender, EventArgs e)
        {
            // reserved for future detail view
        }

        private MaterialWafer CurrentWafer()
        {
            var carrierId = txtCarrierId.Text?.Trim();
            var slot = (int)nudSlot.Value;
            if (string.IsNullOrEmpty(carrierId)) return null;
            return WaferManager.Instance.GetWafer(carrierId, slot);
        }

        private List<string> ParseRecipeKeys()
        {
            var src = txtRecipeKeys.Text ?? string.Empty;
            var parts = src.Split(new[] { ',', ';', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            var list = new List<string>();
            foreach (var p in parts)
            {
                var s = p.Trim();
                if (s.Length > 0 && !list.Contains(s, StringComparer.OrdinalIgnoreCase)) list.Add(s);
            }
            return list;
        }

        private void RefreshChipList()
        {
            try
            {
                lvChips.BeginUpdate();
                lvChips.Items.Clear();
                var wafer = CurrentWafer();
                if (wafer == null)
                {
                    lvChips.EndUpdate();
                    return;
                }
                lock (wafer.Dies)
                {
                    foreach (var chip in wafer.Dies.OrderBy(c => c.Index))
                    {
                        var measures = FormatMeasures(chip);
                        var item = new ListViewItem(new[]
                        {
                        chip.Index.ToString(),
                        chip.MapX.ToString(),
                        chip.MapY.ToString(),
                        chip.State.ToString(),
                        chip.IsPass ? "True" : "False",
                        measures
                    });
                        lvChips.Items.Add(item);
                    }
                    lvChips.EndUpdate();
                }
            }
            catch
            {
                try { lvChips.EndUpdate(); } catch { }
            }
        }

        private string FormatMeasures(MaterialDie chip)
        {
            if (chip == null || chip.MeasureValues == null || chip.MeasureValues.Count == 0)
                return string.Empty;
            var sb = new StringBuilder();
            bool first = true;
            foreach (var kv in chip.MeasureValues)
            {
                if (!first) sb.Append("; ");
                first = false;
                sb.Append(kv.Key).Append('=');
                sb.Append(double.IsNaN(kv.Value) ? "NaN" : kv.Value.ToString("0.###"));
            }
            return sb.ToString();
        }

        private void SetStatus(string msg)
        {
            if (lblStatus == null) return;
            lblStatus.Text = $"[{DateTime.Now:HH:mm:ss}] {msg}";
        }
    }
}
