using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace QMC.Common
{
    /// <summary>
    /// 언어 파일 생성 및 관리를 위한 유틸리티 Form
    /// </summary>
    public partial class LanguageSetupForm : Form
    {
        private LanguageManager _langManager;
        private TextBox txtLog;
        private Button btnScanEquipment;
        private Button btnScanAlarms;
        private Button btnScanAllForms;
        private Button btnSaveKorean;
        private Button btnSaveEnglish;
        private Button btnLoadKorean;
        private Button btnLoadEnglish;
        private ComboBox cboLanguage;
        private Label lblCurrentLanguage;
        private GroupBox grpScan;
        private GroupBox grpSave;
        private GroupBox grpLoad;
        private GroupBox grpCurrent;
        private TreeView tvLang;
        private Button btnRefreshTree;
        private Button btnApplyCurrent;
        private CheckBox chkTopMost;

        private Form main;
        public LanguageSetupForm(Form main)
        {
            InitializeComponent();
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

        private void InitializeComponent()
        {
            this.Text = "Language Setup Utility";
            this.Size = new System.Drawing.Size(1100, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Scan Group
            grpScan = new GroupBox { Text = "1. Scan", Location = new System.Drawing.Point(10, 10), Size = new System.Drawing.Size(560, 120) };
            btnScanEquipment = new Button { Text = "Scan Equipment", Location = new System.Drawing.Point(10, 25), Size = new System.Drawing.Size(150, 35) }; btnScanEquipment.Click += BtnScanEquipment_Click;
            btnScanAlarms = new Button { Text = "Scan Alarms", Location = new System.Drawing.Point(170, 25), Size = new System.Drawing.Size(150, 35) }; btnScanAlarms.Click += BtnScanAlarms_Click;
            btnScanAllForms = new Button { Text = "Scan All Forms", Location = new System.Drawing.Point(330, 25), Size = new System.Drawing.Size(150, 35) }; btnScanAllForms.Click += BtnScanAllForms_Click;
            grpScan.Controls.Add(btnScanEquipment); grpScan.Controls.Add(btnScanAlarms); grpScan.Controls.Add(btnScanAllForms);

            // Save Group
            grpSave = new GroupBox { Text = "2. Save Language Files", Location = new System.Drawing.Point(10, 140), Size = new System.Drawing.Size(560, 80) };
            btnSaveKorean = new Button { Text = "Save Korean", Location = new System.Drawing.Point(10, 25), Size = new System.Drawing.Size(150, 40) }; btnSaveKorean.Click += BtnSaveKorean_Click;
            btnSaveEnglish = new Button { Text = "Save English", Location = new System.Drawing.Point(170, 25), Size = new System.Drawing.Size(150, 40) }; btnSaveEnglish.Click += BtnSaveEnglish_Click;
            grpSave.Controls.Add(btnSaveKorean); grpSave.Controls.Add(btnSaveEnglish);

            // Load Group
            grpLoad = new GroupBox { Text = "3. Load & Test", Location = new System.Drawing.Point(10, 230), Size = new System.Drawing.Size(560, 80) };
            btnLoadKorean = new Button { Text = "Load Korean", Location = new System.Drawing.Point(10, 25), Size = new System.Drawing.Size(150, 40) }; btnLoadKorean.Click += BtnLoadKorean_Click;
            btnLoadEnglish = new Button { Text = "Load English", Location = new System.Drawing.Point(170, 25), Size = new System.Drawing.Size(150, 40) }; btnLoadEnglish.Click += BtnLoadEnglish_Click;
            grpLoad.Controls.Add(btnLoadKorean); grpLoad.Controls.Add(btnLoadEnglish);

            // Current Language Group
            grpCurrent = new GroupBox { Text = "Current Language", Location = new System.Drawing.Point(10, 320), Size = new System.Drawing.Size(560, 90) };
            lblCurrentLanguage = new Label { Text = "Available Languages:", Location = new System.Drawing.Point(10, 25), AutoSize = true };
            cboLanguage = new ComboBox { Location = new System.Drawing.Point(150, 22), Size = new System.Drawing.Size(200, 25), DropDownStyle = ComboBoxStyle.DropDownList }; cboLanguage.SelectedIndexChanged += CboLanguage_SelectedIndexChanged;
            chkTopMost = new CheckBox { Text = "TopMost", Location = new System.Drawing.Point(370, 24), AutoSize = true }; chkTopMost.CheckedChanged += (s, e) => this.TopMost = chkTopMost.Checked;
            btnApplyCurrent = new Button { Text = "Apply to Open Forms", Location = new System.Drawing.Point(10, 55), Size = new System.Drawing.Size(200, 28) }; btnApplyCurrent.Click += (s, e) => { ApplyCurrentLanguage(); };
            grpCurrent.Controls.Add(lblCurrentLanguage); grpCurrent.Controls.Add(cboLanguage); grpCurrent.Controls.Add(chkTopMost); grpCurrent.Controls.Add(btnApplyCurrent);

            // Log TextBox
            txtLog = new TextBox { Multiline = true, ScrollBars = ScrollBars.Vertical, Location = new System.Drawing.Point(10, 420), Size = new System.Drawing.Size(560, 230), ReadOnly = true };

            // TreeView for language keys
            tvLang = new TreeView { Location = new System.Drawing.Point(580, 10), Size = new System.Drawing.Size(500, 610), LabelEdit = false, ShowNodeToolTips = true }; tvLang.DoubleClick += TvLang_DoubleClick;

            btnRefreshTree = new Button { Text = "Refresh Tree", Location = new System.Drawing.Point(580, 630), Size = new System.Drawing.Size(120, 30) }; btnRefreshTree.Click += BtnRefreshTree_Click;

            this.Controls.Add(grpScan); this.Controls.Add(grpSave); this.Controls.Add(grpLoad); this.Controls.Add(grpCurrent); this.Controls.Add(txtLog); this.Controls.Add(tvLang); this.Controls.Add(btnRefreshTree);

            this.Load += LanguageSetupForm_Load;
        }

        private void LanguageSetupForm_Load(object sender, EventArgs e)
        {
            RefreshLanguageList();
            AddLog("Language Setup Utility loaded.");
            AddLog($"Language folder: {System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Languages")}");
            BuildTree();
        }

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
                    ?.GetValue(_langManager) as System.Collections.IDictionary;
                if (formKeys != null)
                {
                    foreach (System.Collections.DictionaryEntry de in formKeys)
                        AddPathNode(rootForm, de.Key.ToString(), new TreeTag("Form", de.Key.ToString(), de.Value?.ToString()));
                }
            }
            catch { }

            // Equipment DisplayName/Category
            try
            {
                var display = _langManager.GetType().GetField("_displayNames", BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.GetValue(_langManager) as System.Collections.IDictionary;
                var category = _langManager.GetType().GetField("_categories", BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.GetValue(_langManager) as System.Collections.IDictionary;
                var dispNode = new TreeNode("DisplayName");
                var catNode = new TreeNode("Category");
                if (display != null)
                {
                    foreach (System.Collections.DictionaryEntry de in display)
                        AddPathNode(dispNode, de.Key.ToString(), new TreeTag("DisplayName", de.Key.ToString(), de.Value?.ToString()));
                }
                if (category != null)
                {
                    foreach (System.Collections.DictionaryEntry de in category)
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
                var propOrders = propOrdersField?.GetValue(_langManager) as System.Collections.IDictionary;
                if (propOrders != null)
                {
                    var orderRoot = new TreeNode("PropertyOrder");
                    foreach (System.Collections.DictionaryEntry groupEntry in propOrders)
                    {
                        var groupName = groupEntry.Key?.ToString() ?? string.Empty;
                        var groupNode = new TreeNode(groupName);
                        var innerObj = groupEntry.Value;

                        var entries = new System.Collections.Generic.List<System.Tuple<string, int>>();
                        if (innerObj is System.Collections.IDictionary nonGen)
                        {
                            foreach (System.Collections.DictionaryEntry de in nonGen)
                                entries.Add(System.Tuple.Create(de.Key?.ToString() ?? string.Empty, SafeToInt(de.Value)));
                        }
                        else if (innerObj is System.Collections.IEnumerable en)
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
                                entries.Add(System.Tuple.Create(k, v));
                            }
                        }

                        foreach (var kv in entries.OrderBy(x => x.Item2).ThenBy(x => x.Item1))
                        {
                            // node 텍스트에 순서를 표시하고, 편집은 프로퍼티명(키)을 변경
                            var flatKey = groupName + "." + kv.Item1;
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
                    ?.GetValue(_langManager) as System.Collections.IDictionary;
                var causes = _langManager.GetType().GetField("_alarmCauses", BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.GetValue(_langManager) as System.Collections.IDictionary;
                var tNode = new TreeNode("Title");
                var cNode = new TreeNode("Cause");
                if (titles != null)
                {
                    foreach (System.Collections.DictionaryEntry de in titles)
                        AddPathNode(tNode, de.Key.ToString(), new TreeTag("AlarmTitle", de.Key.ToString(), de.Value?.ToString()));
                }
                if (causes != null)
                {
                    foreach (System.Collections.DictionaryEntry de in causes)
                        AddPathNode(cNode, de.Key.ToString(), new TreeTag("AlarmCause", de.Key.ToString(), de.Value?.ToString()));
                }
                rootAlarm.Nodes.Add(tNode);
                rootAlarm.Nodes.Add(cNode);
            }
            catch { }

            tvLang.Nodes.Add(rootForm);
            tvLang.Nodes.Add(rootEquip);
            tvLang.Nodes.Add(rootAlarm);
            // Collapse all for concise view; expand top-level
            tvLang.CollapseAll();
            foreach (TreeNode n in tvLang.Nodes) n.Expand();
            tvLang.EndUpdate();
        }

        private static int SafeToInt(object o)
        {
            if (o == null) return int.MaxValue;
            try { return Convert.ToInt32(o); } catch { return int.MaxValue; }
        }

        private void TvLang_DoubleClick(object sender, EventArgs e)
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

        private void RenamePropertyOrderKey(string group, string oldProp, string newProp)
        {
            try
            {
                var f = _langManager.GetType().GetField("_propertyOrders", BindingFlags.NonPublic | BindingFlags.Instance);
                var dict = f?.GetValue(_langManager) as System.Collections.IDictionary; if (dict == null) return;
                var inner = dict[group];
                if (inner == null) return;

                // generic and non-generic both through IDictionary where possible
                if (inner is System.Collections.IDictionary nonGen)
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

        private void BtnRefreshTree_Click(object sender, EventArgs e)
        {
            // 최신 Equipment 상태에서 PropertyOrder 재수집 후 트리 갱신
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

        private void SetPropertyOrderValue(string flatKey, int order)
        {
            try
            {
                int dot = flatKey.IndexOf('.');
                if (dot <= 0) return;
                string group = flatKey.Substring(0, dot);
                string prop = flatKey.Substring(dot + 1);
                var f = _langManager.GetType().GetField("_propertyOrders", BindingFlags.NonPublic | BindingFlags.Instance);
                var dict = f?.GetValue(_langManager) as System.Collections.IDictionary; if (dict == null) return;

                // get or create inner dictionary
                object inner = dict[group];
                if (inner == null)
                {
                    var innerType = typeof(System.Collections.Generic.Dictionary<string, int>);
                    inner = Activator.CreateInstance(innerType, new object[] { System.StringComparer.OrdinalIgnoreCase });
                    dict[group] = inner;
                }
                var innerDic = inner as System.Collections.IDictionary;
                if (innerDic != null)
                {
                    innerDic[prop] = order;
                }
            }
            catch { }
        }

        private void SetDictValue(string fieldName, string key, string value)
        {
            var f = _langManager.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            var dic = f?.GetValue(_langManager) as System.Collections.IDictionary; if (dic == null) return;
            dic[key] = value;
        }

        private void BtnScanEquipment_Click(object sender, EventArgs e)
        {
            try
            {
                AddLog("Scanning Equipment...");

                var equipmentType = Type.GetType("QMC.LCP_280.Process.Equipment");
                if (equipmentType == null)
                {
                    AddLog("ERROR: Equipment type not found. Make sure QMC.LCP_280.Process is loaded.");
                    MessageBox.Show("Equipment 타입을 찾을 수 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var instanceProp = equipmentType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                if (instanceProp == null)
                {
                    AddLog("ERROR: Equipment.Instance property not found.");
                    MessageBox.Show("Equipment.Instance를 찾을 수 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var equipment = instanceProp.GetValue(null);
                if (equipment == null)
                {
                    AddLog("ERROR: Equipment.Instance is null.");
                    MessageBox.Show("Equipment.Instance가 null입니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                Log.Write("LanguageSetup", $"Equipment 스캔 실패: {ex.Message}");
                MessageBox.Show($"Equipment 스캔 실패:\n{ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnScanAlarms_Click(object sender, EventArgs e)
        {
            try
            {
                AddLog("Scanning Alarms...");

                var equipmentType = Type.GetType("QMC.LCP_280.Process.Equipment");
                if (equipmentType == null)
                {
                    AddLog("ERROR: Equipment type not found. Make sure QMC.LCP_280.Process is loaded.");
                    MessageBox.Show("Equipment 타입을 찾을 수 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var instanceProp = equipmentType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                if (instanceProp == null)
                {
                    AddLog("ERROR: Equipment.Instance property not found.");
                    MessageBox.Show("Equipment.Instance를 찾을 수 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var equipment = instanceProp.GetValue(null);
                if (equipment == null)
                {
                    AddLog("ERROR: Equipment.Instance is null.");
                    MessageBox.Show("Equipment.Instance가 null입니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                Log.Write("LanguageSetup", $"Alarm 스캔 실패: {ex.Message}");
                MessageBox.Show($"Alarm 스캔 실패:\n{ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnScanAllForms_Click(object sender, EventArgs e)
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
            catch (Exception)
            {
            }
        }

        private void BtnSaveKorean_Click(object sender, EventArgs e)
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
                Log.Write("LanguageSetup", $"Korean 저장 실패: {ex.Message}");
                MessageBox.Show($"저장 실패:\n{ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSaveEnglish_Click(object sender, EventArgs e)
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
                Log.Write("LanguageSetup", $"English 저장 실패: {ex.Message}");
                MessageBox.Show($"저장 실패:\n{ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnLoadKorean_Click(object sender, EventArgs e)
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
                Log.Write("LanguageSetup", $"Korean 로드 실패: {ex.Message}");
                MessageBox.Show($"로드 실패:\n{ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnLoadEnglish_Click(object sender, EventArgs e)
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
                Log.Write("LanguageSetup", $"English 로드 실패: {ex.Message}");
                MessageBox.Show($"로드 실패:\n{ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CboLanguage_SelectedIndexChanged(object sender, EventArgs e)
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
        private Form GetMainFormSafe()
        {
            try { return this.main; } catch { return null; }
        }

        private void ApplyCurrentLanguage()
        {
            // Equipment & Alarm 적용
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

            // Form 적용 (열려있는 모든 폼)
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
                var currentLang = cboLanguage.Items.Cast<string>().FirstOrDefault(l => l.Equals(_langManager.CurrentLanguage, System.StringComparison.OrdinalIgnoreCase));
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

        // '.' 구분으로 하위 트리 구성 + 태그 부여
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

        private class TreeTag
        {
            public string Section;
            public string Key;
            public string Value;
            public TreeTag(string s, string k, string v) { Section = s; Key = k; Value = v; }
        }

    }

    // 간단 입력 박스
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
