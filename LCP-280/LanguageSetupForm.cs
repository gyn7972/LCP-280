using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Collections;
using System.Collections.Generic;

namespace QMC.Common
{
    /// <summary>
    /// 언어 파일 생성 및 관리를 위한 유틸리티 Form
    /// </summary>
    public partial class LanguageSetupForm : Form
    {
        private LanguageManager _langManager;
        private Form main;

        public LanguageSetupForm(Form main)
        {
            InitializeComponent(); // Designer.cs에 정의된 메서드 호출
            _langManager = LanguageManager.Instance;
            this.main = main;
        }

        // 모달리스로 띄우는 정적 헬퍼
        public static void ShowModeless(Form main)
        {
            var f = new LanguageSetupForm(main);
            f.TopMost = false;
            f.Show();
        }

        public static void OpenOrActivate(Form main)
        {
            foreach (Form f in Application.OpenForms)
            {
                if (f is LanguageSetupForm lsf)
                {
                    lsf.Activate();
                    return;
                }
            }
            ShowModeless(main);
        }

        private void LanguageSetupForm_Load(object sender, EventArgs e)
        {
            RefreshLanguageList();
            AddLog("Language Setup Utility loaded.");
            AddLog($"Language folder: {System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Languages")}");
            BuildTree();
        }

        #region Logic & Helpers

        private void BuildTree()
        {
            tvLang.BeginUpdate();
            tvLang.Nodes.Clear();
            var rootForm = new TreeNode("Form");
            var rootEquip = new TreeNode("Equipment");
            var rootAlarm = new TreeNode("Alarm");

            // Form keys
            try
            {
                var formKeys = _langManager.GetType().GetField("_formControls", BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.GetValue(_langManager) as IDictionary;
                if (formKeys != null)
                {
                    foreach (DictionaryEntry de in formKeys)
                        AddPathNode(rootForm, de.Key.ToString(), new TreeTag("Form", de.Key.ToString(), de.Value?.ToString()));
                }
            }
            catch { }

            // Equipment DisplayName/Category
            try
            {
                var display = _langManager.GetType().GetField("_displayNames", BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.GetValue(_langManager) as IDictionary;
                var category = _langManager.GetType().GetField("_categories", BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.GetValue(_langManager) as IDictionary;
                var dispNode = new TreeNode("DisplayName");
                var catNode = new TreeNode("Category");
                if (display != null)
                {
                    foreach (DictionaryEntry de in display)
                        AddPathNode(dispNode, de.Key.ToString(), new TreeTag("DisplayName", de.Key.ToString(), de.Value?.ToString()));
                }
                if (category != null)
                {
                    foreach (DictionaryEntry de in category)
                        AddPathNode(catNode, de.Key.ToString(), new TreeTag("Category", de.Key.ToString(), de.Value?.ToString()));
                }
                rootEquip.Nodes.Add(dispNode);
                rootEquip.Nodes.Add(catNode);
            }
            catch { }

            // Equipment PropertyOrder (Type group -> property = index)
            try
            {
                var propOrdersField = _langManager.GetType().GetField("_propertyOrders", BindingFlags.NonPublic | BindingFlags.Instance);
                var propOrders = propOrdersField?.GetValue(_langManager) as IDictionary;
                if (propOrders != null)
                {
                    var orderRoot = new TreeNode("PropertyOrder");
                    foreach (DictionaryEntry groupEntry in propOrders)
                    {
                        var groupName = groupEntry.Key?.ToString() ?? string.Empty;
                        var groupNode = new TreeNode(groupName);
                        var innerObj = groupEntry.Value;

                        var entries = new List<Tuple<string, int>>();
                        if (innerObj is IDictionary nonGen)
                        {
                            foreach (DictionaryEntry de in nonGen)
                                entries.Add(Tuple.Create(de.Key?.ToString() ?? string.Empty, SafeToInt(de.Value)));
                        }
                        else if (innerObj is IEnumerable en)
                        {
                            foreach (var item in en)
                            {
                                if (item == null) continue;
                                var t = item.GetType();
                                var kp = t.GetProperty("Key");
                                var vp = t.GetProperty("Value");
                                if (kp == null || vp == null) continue;
                                string k = kp.GetValue(item)?.ToString() ?? string.Empty;
                                int v = SafeToInt(vp.GetValue(item));
                                entries.Add(Tuple.Create(k, v));
                            }
                        }

                        foreach (var kv in entries.OrderBy(x => x.Item2).ThenBy(x => x.Item1))
                        {
                            // node 텍스트에 순서를 표시하고, 편집은 프로퍼티명(키)을 변경
                            var child = new TreeNode(kv.Item2.ToString())
                            {
                                // Section: PropertyOrder, Key: groupName, Value: propertyName
                                Tag = new TreeTag("PropertyOrder", groupName, kv.Item1),
                                ToolTipText = kv.Item1 // tooltip에 프로퍼티명 표시
                            };
                            groupNode.Nodes.Add(child);
                        }

                        orderRoot.Nodes.Add(groupNode);
                    }
                    rootEquip.Nodes.Add(orderRoot);
                }
            }
            catch { }

            // Alarm Title/Cause
            try
            {
                var titles = _langManager.GetType().GetField("_alarmTitles", BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.GetValue(_langManager) as IDictionary;
                var causes = _langManager.GetType().GetField("_alarmCauses", BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.GetValue(_langManager) as IDictionary;
                var tNode = new TreeNode("Title");
                var cNode = new TreeNode("Cause");
                if (titles != null)
                {
                    foreach (DictionaryEntry de in titles)
                        AddPathNode(tNode, de.Key.ToString(), new TreeTag("AlarmTitle", de.Key.ToString(), de.Value?.ToString()));
                }
                if (causes != null)
                {
                    foreach (DictionaryEntry de in causes)
                        AddPathNode(cNode, de.Key.ToString(), new TreeTag("AlarmCause", de.Key.ToString(), de.Value?.ToString()));
                }
                rootAlarm.Nodes.Add(tNode);
                rootAlarm.Nodes.Add(cNode);
            }
            catch { }

            tvLang.Nodes.Add(rootForm);
            tvLang.Nodes.Add(rootEquip);
            tvLang.Nodes.Add(rootAlarm);

            tvLang.CollapseAll();
            foreach (TreeNode n in tvLang.Nodes) n.Expand();
            tvLang.EndUpdate();
        }

        private static int SafeToInt(object o)
        {
            if (o == null) return int.MaxValue;
            try { return Convert.ToInt32(o); } catch { return int.MaxValue; }
        }

        private void RenamePropertyOrderKey(string group, string oldProp, string newProp)
        {
            try
            {
                var f = _langManager.GetType().GetField("_propertyOrders", BindingFlags.NonPublic | BindingFlags.Instance);
                var dict = f?.GetValue(_langManager) as IDictionary; if (dict == null) return;
                var inner = dict[group];
                if (inner == null) return;

                if (inner is IDictionary nonGen)
                {
                    if (!nonGen.Contains(oldProp)) return;
                    var val = nonGen[oldProp];
                    nonGen.Remove(oldProp);
                    nonGen[newProp] = val;
                    return;
                }

                // fallback: reflection for Dictionary<string,int>
                var innerType = inner.GetType();
                var containsKey = innerType.GetMethod("ContainsKey");
                var remove = innerType.GetMethod("Remove", new[] { typeof(string) });
                var setItem = innerType.GetProperty("Item");
                var getItem = innerType.GetProperty("Item");
                if (containsKey != null && (bool)containsKey.Invoke(inner, new object[] { oldProp }))
                {
                    var oldVal = getItem?.GetValue(inner, new object[] { oldProp });
                    remove?.Invoke(inner, new object[] { oldProp });
                    setItem?.SetValue(inner, oldVal, new object[] { newProp });
                }
            }
            catch { }
        }

        private void SetDictValue(string fieldName, string key, string value)
        {
            var f = _langManager.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            var dic = f?.GetValue(_langManager) as IDictionary; if (dic == null) return;
            dic[key] = value;
        }

        private void ApplyCurrentLanguage()
        {
            object equipment = null;
            try
            {
                var equipmentType = Type.GetType("QMC.LCP_280.Process.Equipment");
                var instanceProp = equipmentType?.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                equipment = instanceProp?.GetValue(null);
                if (equipment != null)
                {
                    _langManager.ApplyEquipmentLanguage(equipment);
                    _langManager.ApplyAlarmLanguage(equipment);
                }
            }
            catch { }

            foreach (Form form in Application.OpenForms)
            {
                try { _langManager.ApplyFormLanguage(form); } catch { }
            }
        }

        private void RefreshLanguageList()
        {
            var languages = _langManager.GetAvailableLanguages();
            cboLanguage.Items.Clear();
            foreach (var lang in languages) cboLanguage.Items.Add(lang);
            if (cboLanguage.Items.Count > 0)
            {
                var currentLang = cboLanguage.Items.Cast<string>().FirstOrDefault(l => l.Equals(_langManager.CurrentLanguage, StringComparison.OrdinalIgnoreCase));
                if (currentLang != null) cboLanguage.SelectedItem = currentLang; else cboLanguage.SelectedIndex = 0;
            }
        }

        private void AddLog(string message)
        {
            if (txtLog.InvokeRequired) { txtLog.Invoke(new Action<string>(AddLog), message); return; }
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\r\n");
            txtLog.SelectionStart = txtLog.Text.Length;
            txtLog.ScrollToCaret();
        }

        private void AddPathNode(TreeNode root, string path, TreeTag tag)
        {
            var parts = (path ?? string.Empty).Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            TreeNode cur = root;
            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i];
                var existing = cur.Nodes.Cast<TreeNode>().FirstOrDefault(n => n.Text == part);
                if (existing == null)
                {
                    existing = new TreeNode(part);
                    cur.Nodes.Add(existing);
                }
                cur = existing;
            }
            cur.Tag = tag;
            cur.ToolTipText = tag?.Value ?? string.Empty;
        }

        #endregion

        #region Event Handlers

        private void tvLang_DoubleClick(object sender, EventArgs e)
        {
            var node = tvLang.SelectedNode; if (node == null || node.Tag == null) return;
            var tag = (TreeTag)node.Tag;
            var input = new InputBox();

            // PropertyOrder는 키(프로퍼티명) 변경 모드
            if (string.Equals(tag.Section, "PropertyOrder", StringComparison.OrdinalIgnoreCase))
            {
                var oldName = tag.Value; // property name
                var newName = input.ShowDialog($"{tag.Key} - Rename Property", oldName);
                if (newName == null) return;
                newName = newName.Trim();
                if (newName.Length == 0 || string.Equals(newName, oldName, StringComparison.Ordinal)) return;
                RenamePropertyOrderKey(tag.Key, oldName, newName);
                tag.Value = newName;
                AddLog($"PropertyOrder: {tag.Key}.{oldName} -> {newName}");
                BuildTree();
                return;
            }

            var newValue = input.ShowDialog(tag.Key, tag.Value);
            if (newValue == null) return;
            try
            {
                switch (tag.Section)
                {
                    case "Form":
                        SetDictValue("_formControls", tag.Key, newValue);
                        break;
                    case "DisplayName":
                        SetDictValue("_displayNames", tag.Key, newValue);
                        break;
                    case "Category":
                        SetDictValue("_categories", tag.Key, newValue);
                        break;
                    case "AlarmTitle":
                        SetDictValue("_alarmTitles", tag.Key, newValue);
                        break;
                    case "AlarmCause":
                        SetDictValue("_alarmCauses", tag.Key, newValue);
                        break;
                }
                tag.Value = newValue;
                AddLog($"Updated {tag.Section}:{tag.Key}");
                BuildTree();
            }
            catch (Exception ex)
            {
                AddLog("ERROR: " + ex.Message);
            }
        }

        private void btnRefreshTree_Click(object sender, EventArgs e)
        {
            try
            {
                var equipmentType = Type.GetType("QMC.LCP_280.Process.Equipment");
                var instanceProp = equipmentType?.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                var equipment = instanceProp?.GetValue(null);
                if (equipment != null)
                {
                    _langManager.ScanEquipmentProperties(equipment);
                }
            }
            catch { }
            BuildTree();
        }

        private void btnScanEquipment_Click(object sender, EventArgs e)
        {
            try
            {
                AddLog("Scanning Equipment...");

                var equipmentType = Type.GetType("QMC.LCP_280.Process.Equipment");
                if (equipmentType == null)
                {
                    AddLog("ERROR: Equipment type not found.");
                    MessageBox.Show("Equipment 타입을 찾을 수 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var instanceProp = equipmentType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                var equipment = instanceProp?.GetValue(null);

                if (equipment == null)
                {
                    AddLog("ERROR: Equipment.Instance is null.");
                    return;
                }

                _langManager.ScanEquipmentProperties(equipment);
                AddLog("Equipment scan completed successfully.");
                MessageBox.Show("Equipment 스캔 완료", "성공", MessageBoxButtons.OK, MessageBoxIcon.Information);
                BuildTree();
            }
            catch (Exception ex)
            {
                AddLog($"ERROR: {ex.Message}");
                MessageBox.Show($"Equipment 스캔 실패:\n{ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnScanAlarms_Click(object sender, EventArgs e)
        {
            try
            {
                AddLog("Scanning Alarms...");
                var equipmentType = Type.GetType("QMC.LCP_280.Process.Equipment");
                var instanceProp = equipmentType?.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                var equipment = instanceProp?.GetValue(null);

                if (equipment == null)
                {
                    AddLog("ERROR: Equipment.Instance is null.");
                    return;
                }

                _langManager.ScanEquipmentAlarms(equipment);
                AddLog("Alarm scan completed successfully.");
                MessageBox.Show("Alarm 스캔 완료", "성공", MessageBoxButtons.OK, MessageBoxIcon.Information);
                BuildTree();
            }
            catch (Exception ex)
            {
                AddLog($"ERROR: {ex.Message}");
                MessageBox.Show($"Alarm 스캔 실패:\n{ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnScanAllForms_Click(object sender, EventArgs e)
        {
            try
            {
                AddLog("Scanning all Forms...");
                Application.OpenForms
                    .OfType<Form>()
                    .ToList()
                    .ForEach(f =>
                    {
                        AddLog($"Scanning Form: {f.Name}");
                        _langManager.ScanFormControls(f);
                    });
                BuildTree();
            }
            catch (Exception) { }
        }

        private void btnSaveKorean_Click(object sender, EventArgs e)
        {
            try
            {
                AddLog("Saving Korean language files...");
                _langManager.SaveLanguage("Korean");
                AddLog("Korean language files saved successfully.");
                MessageBox.Show("Korean 언어 파일 저장 완료", "성공", MessageBoxButtons.OK, MessageBoxIcon.Information);
                RefreshLanguageList();
            }
            catch (Exception ex)
            {
                AddLog($"ERROR: {ex.Message}");
                MessageBox.Show($"저장 실패:\n{ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSaveEnglish_Click(object sender, EventArgs e)
        {
            try
            {
                AddLog("Saving English language files...");
                _langManager.SaveLanguage("English");
                AddLog("English language files saved successfully.");
                AddLog("NOTE: Manual translation required for English files.");
                MessageBox.Show("English 언어 파일 저장 완료\n\n수동 번역이 필요합니다.", "성공", MessageBoxButtons.OK, MessageBoxIcon.Information);
                RefreshLanguageList();
            }
            catch (Exception ex)
            {
                AddLog($"ERROR: {ex.Message}");
                MessageBox.Show($"저장 실패:\n{ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnLoadKorean_Click(object sender, EventArgs e)
        {
            try
            {
                AddLog("Loading Korean language...");
                _langManager.LoadLanguage("Korean");
                _langManager.CurrentLanguage = "Korean";
                AddLog("Korean language loaded successfully.");
                MessageBox.Show("Korean 언어 로드 완료", "성공", MessageBoxButtons.OK, MessageBoxIcon.Information);
                RefreshLanguageList();
                BuildTree();
            }
            catch (Exception ex)
            {
                AddLog($"ERROR: {ex.Message}");
                MessageBox.Show($"로드 실패:\n{ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnLoadEnglish_Click(object sender, EventArgs e)
        {
            try
            {
                AddLog("Loading English language...");
                _langManager.LoadLanguage("English");
                _langManager.CurrentLanguage = "English";
                AddLog("English language loaded successfully.");
                MessageBox.Show("English 언어 로드 완료", "성공", MessageBoxButtons.OK, MessageBoxIcon.Information);
                RefreshLanguageList();
                BuildTree();
            }
            catch (Exception ex)
            {
                AddLog($"ERROR: {ex.Message}");
                MessageBox.Show($"로드 실패:\n{ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cboLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboLanguage.SelectedItem != null)
            {
                string selectedLang = cboLanguage.SelectedItem.ToString();
                _langManager.CurrentLanguage = selectedLang;
                AddLog($"Language changed to: {selectedLang}");
                ApplyCurrentLanguage();
                BuildTree();
            }
        }

        private void btnApplyCurrent_Click(object sender, EventArgs e)
        {
            ApplyCurrentLanguage();
        }

        private void chkTopMost_CheckedChanged(object sender, EventArgs e)
        {
            this.TopMost = chkTopMost.Checked;
        }

        #endregion

        private class TreeTag
        {
            public string Section;
            public string Key;
            public string Value;
            public TreeTag(string s, string k, string v) { Section = s; Key = k; Value = v; }
        }
    }

    // 간단 입력 박스 (내부 클래스로 유지하거나 별도 파일로 분리 가능)
    internal class InputBox : Form
    {
        private TextBox txt;
        private Button ok, cancel;
        public InputBox()
        {
            this.Text = "Edit";
            this.Size = new System.Drawing.Size(400, 180);
            txt = new TextBox { Multiline = true, ScrollBars = ScrollBars.Vertical, Location = new System.Drawing.Point(10, 10), Size = new System.Drawing.Size(360, 90) };
            ok = new Button { Text = "OK", Location = new System.Drawing.Point(210, 110), Size = new System.Drawing.Size(75, 25) };
            cancel = new Button { Text = "Cancel", Location = new System.Drawing.Point(295, 110), Size = new System.Drawing.Size(75, 25) };
            ok.Click += (s, e) => this.DialogResult = DialogResult.OK;
            cancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            this.Controls.Add(txt); this.Controls.Add(ok); this.Controls.Add(cancel);
            this.StartPosition = FormStartPosition.CenterParent;
        }
        public string ShowDialog(string title, string value)
        {
            this.Text = title;
            txt.Text = value ?? string.Empty;
            return base.ShowDialog() == DialogResult.OK ? txt.Text : null;
        }
    }
}