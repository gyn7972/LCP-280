using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;
using QMC.Common.Component; // added for IPropertyOrderProvider

namespace QMC.Common
{
    /// <summary>
    /// Equipment/Alarm/Form 다국어 관리 매니저 (정리된 버전)
    /// </summary>
    public partial class LanguageManager
    {
        private static LanguageManager _instance;
        public static LanguageManager Instance => _instance ?? (_instance = new LanguageManager());

        private readonly string _languageFolder;
        private string _currentLanguage = "Korean";

        private readonly Dictionary<string, string> _displayNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> _categories = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> _formControls = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> _alarmTitles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> _alarmCauses = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> _alarmGrades = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private LanguageManager()
        {
            _languageFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Languages");
            Directory.CreateDirectory(_languageFolder);
        }

        public string CurrentLanguage
        {
            get => _currentLanguage;
            set
            {
                if (_currentLanguage != value)
                {
                    _currentLanguage = value;
                    LoadLanguage(value);
                }
            }
        }

        #region Type / Reflection Helpers
        private static bool IsSimpleType(Type t) => t == null || t.IsPrimitive || t == typeof(string) || t == typeof(DateTime) || t.IsEnum || t == typeof(decimal);
        private bool IsUnitOrComponent(Type t)
        {
            if (t == null) return false;
            var bt = t;
            while (bt != null)
            {
                if (bt.Name.Contains("BaseUnit") || bt.Name.Contains("BaseComponent")) return true;
                bt = bt.BaseType;
            }
            return t.Name.Contains("Unit") || t.Name.Contains("Component") || t.Name.Contains("Config");
        }
        private static bool IsDictionaryLike(Type t)
        {
            if (t == null) return false;
            if (t.IsGenericType)
            {
                var g = t.GetGenericTypeDefinition();
                if (g == typeof(Dictionary<,>) || g == typeof(System.Collections.Concurrent.ConcurrentDictionary<,>)) return true;
            }
            return t.GetProperty("Keys") != null && t.GetProperty("Item") != null;
        }
        private static bool LooksLikeAlarmInfo(Type t)
        {
            if (t == null) return false;
            return t.GetProperty("Code") != null && (t.GetProperty("Title") != null || t.GetProperty("Cause") != null);
        }
        private FieldInfo GetFieldRecursive(Type type, string name)
        {
            while (type != null)
            {
                var fi = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (fi != null) return fi;
                type = type.BaseType;
            }
            return null;
        }

        private static string EncodeMultiline(string value)
        {
            if (value == null) return null;
            return value.Replace("\\", "\\\\")
                        .Replace("\r\n", "\n").Replace("\r", "\n")
                        .Replace("\n", "\\n")
                        .Replace("\t", "\\t");
        }
        private static string DecodeMultiline(string value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            var sb = new StringBuilder(value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if (c == '\\' && i + 1 < value.Length)
                {
                    char n = value[i + 1];
                    if (n == 'n') { sb.Append(Environment.NewLine); i++; continue; }
                    if (n == 't') { sb.Append('\t'); i++; continue; }
                    if (n == '\\') { sb.Append('\\'); i++; continue; }
                }
                sb.Append(c);
            }
            return sb.ToString();
        }
        #endregion

        #region Equipment Scan
        public void ScanEquipmentProperties(object equipment)
        {
            if (equipment == null) return;
            _displayNames.Clear();
            _categories.Clear();
            ScanObjectProperties(equipment, equipment.GetType().Name, new HashSet<int>());
        }
        private void ScanObjectProperties(object obj, string path, HashSet<int> visited)
        {
            if (obj == null) return;
            int id = RuntimeHelpers.GetHashCode(obj);
            if (!visited.Add(id)) return;
            var type = obj.GetType();
            if (IsSimpleType(type)) return;

            try
            {
                var asProvider = obj as IPropertyOrderProvider;
                if (asProvider != null)
                {
                    var order = asProvider.GetPropertyOrder();
                    if (order != null)
                        SetPropertyOrder(type.Name, order);
                }
            }
            catch { }

            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                try
                {
                    if (prop.GetCustomAttribute<BrowsableAttribute>() is BrowsableAttribute b && !b.Browsable) continue;
                    if (prop.GetCustomAttributes(true).Any(a => a.GetType().Name == "JsonIgnoreAttribute")) continue;
                    string propPath = path + "." + prop.Name;
                    var cat = prop.GetCustomAttribute<CategoryAttribute>();
                    if (cat != null && !string.IsNullOrWhiteSpace(cat.Category)) _categories[propPath] = cat.Category;
                    var disp = prop.GetCustomAttribute<DisplayNameAttribute>();
                    if (disp != null && !string.IsNullOrWhiteSpace(disp.DisplayName)) _displayNames[propPath] = disp.DisplayName;
                    var val = prop.GetValue(obj);
                    if (val == null) continue;
                    TraverseChild(val, prop.PropertyType, propPath, visited, ScanObjectProperties);
                }
                catch (Exception ex) { Log.Write(ex); }
            }
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                try
                {
                    var fType = field.FieldType; if (IsSimpleType(fType)) continue; var fVal = field.GetValue(obj); if (fVal == null) continue; string fPath = path + "." + field.Name;
                    TraverseChild(fVal, fType, fPath, visited, ScanObjectProperties);
                }
                catch (Exception ex) { Log.Write(ex); }
            }
        }
        private void TraverseChild(object value, Type valueType, string basePath, HashSet<int> visited, Action<object, string, HashSet<int>> recurse)
        {
            if (value == null) return;
            if (IsUnitOrComponent(valueType)) { recurse(value, basePath, visited); return; }
            if (IsDictionaryLike(valueType))
            {
                try
                {
                    var dt = value.GetType();
                    var valuesProp = dt.GetProperty("Values");
                    if (valuesProp != null)
                    {
                        var vals = valuesProp.GetValue(value) as IEnumerable;
                        if (vals != null)
                        {
                            int idx = 0;
                            foreach (var item in vals)
                            {
                                if (item == null) { idx++; continue; }
                                var it = item.GetType();
                                if (IsSimpleType(it)) { idx++; continue; }
                                string ip = basePath + "[" + idx + "]";
                                if (IsUnitOrComponent(it)) recurse(item, ip, visited);
                                idx++;
                            }
                            return;
                        }
                    }
                    var keysProp = dt.GetProperty("Keys");
                    var itemProp = dt.GetProperty("Item");
                    if (keysProp != null && itemProp != null)
                    {
                        var keys = keysProp.GetValue(value) as IEnumerable;
                        if (keys != null)
                        {
                            int idx = 0;
                            foreach (var key in keys)
                            {
                                object item = null;
                                try { item = itemProp.GetValue(value, new object[] { key }); } catch { }
                                if (item == null) { idx++; continue; }
                                var it = item.GetType();
                                if (IsSimpleType(it)) { idx++; continue; }
                                string ip = basePath + "[" + idx + "]";
                                if (IsUnitOrComponent(it)) recurse(item, ip, visited);
                                idx++;
                            }
                            return;
                        }
                    }
                }
                catch (Exception ex) { Log.Write(ex); }
            }
            if (value is IEnumerable en && !(value is string))
            {
                int idx = 0;
                foreach (var item in en)
                {
                    if (item == null) { idx++; continue; }
                    var it = item.GetType(); if (IsSimpleType(it)) { idx++; continue; }
                    string ip = basePath + "[" + idx + "]";
                    if (IsUnitOrComponent(it)) recurse(item, ip, visited);
                    idx++;
                }
            }
        }
        public void SaveEquipmentLanguage(string language)
        {
            try
            {
                string path = Path.Combine(_languageFolder, $"Equipment_{language}.ini");
                var existingDisplay = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                var existingCategory = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                var existingOrderFlat = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                if (File.Exists(path))
                {
                    string section = "";
                    foreach (var line in File.ReadAllLines(path, Encoding.UTF8))
                    {
                        var t = line.Trim(); if (t.Length == 0 || t.StartsWith(";") || t.StartsWith("#")) continue;
                        if (t.StartsWith("[") && t.EndsWith("]")) { section = t.Substring(1, t.Length - 2); continue; }
                        int idx = t.IndexOf('='); if (idx <= 0) continue; string key = t.Substring(0, idx).Trim(); string val = t.Substring(idx + 1).Trim();
                        if (section.Equals("DisplayName", StringComparison.OrdinalIgnoreCase)) existingDisplay[key] = val;
                        else if (section.Equals("Category", StringComparison.OrdinalIgnoreCase)) existingCategory[key] = val;
                        else if (section.Equals("PropertyOrder", StringComparison.OrdinalIgnoreCase))
                        {
                            if (int.TryParse(val, out var ord)) existingOrderFlat[key] = ord;
                        }
                    }
                }
                var mergedDisplay = new Dictionary<string, string>(existingDisplay, StringComparer.OrdinalIgnoreCase);
                foreach (var kv in _displayNames) mergedDisplay[kv.Key] = kv.Value;
                var mergedCategory = new Dictionary<string, string>(existingCategory, StringComparer.OrdinalIgnoreCase);
                foreach (var kv in _categories) mergedCategory[kv.Key] = kv.Value;

                var mergedOrder = new Dictionary<string, int>(existingOrderFlat, StringComparer.OrdinalIgnoreCase);
                foreach (var g in GetAllPropertyOrdersFlat())
                {
                    mergedOrder[g.Key] = g.Value;
                }

                var sb = new StringBuilder();
                sb.AppendLine("[DisplayName]"); foreach (var kv in mergedDisplay.OrderBy(k => k.Key)) sb.AppendLine(kv.Key + " = " + kv.Value);
                sb.AppendLine(); sb.AppendLine("[Category]"); foreach (var kv in mergedCategory.OrderBy(k => k.Key)) sb.AppendLine(kv.Key + " = " + kv.Value);
                if (mergedOrder.Count > 0)
                {
                    sb.AppendLine(); sb.AppendLine("[PropertyOrder]");
                    foreach (var kv in mergedOrder.OrderBy(k => k.Key)) sb.AppendLine(kv.Key + " = " + kv.Value);
                }
                File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
            }
            catch (Exception ex) { Log.Write(ex); }
        }
        public void LoadEquipmentLanguage(string language)
        {
            try
            {
                string fp = Path.Combine(_languageFolder, $"Equipment_{language}.ini"); if (!File.Exists(fp)) return;
                _displayNames.Clear(); _categories.Clear();
                _propertyOrders.Clear();
                string section = "";
                foreach (var line in File.ReadAllLines(fp, Encoding.UTF8))
                {
                    var t = line.Trim(); if (t.Length == 0 || t.StartsWith(";") || t.StartsWith("#")) continue;
                    if (t.StartsWith("[") && t.EndsWith("]")) { section = t.Substring(1, t.Length - 2); continue; }
                    int idx = t.IndexOf('='); if (idx <= 0) continue; string key = t.Substring(0, idx).Trim(); string val = t.Substring(idx + 1).Trim();
                    if (section.Equals("DisplayName", StringComparison.OrdinalIgnoreCase)) _displayNames[key] = val; else if (section.Equals("Category", StringComparison.OrdinalIgnoreCase)) _categories[key] = val;
                    else if (section.Equals("PropertyOrder", StringComparison.OrdinalIgnoreCase))
                    {
                        int dot = key.IndexOf('.'); if (dot > 0 && int.TryParse(val, out int ord))
                        {
                            string group = key.Substring(0, dot).Trim(); string prop = key.Substring(dot + 1).Trim();
                            if (!_propertyOrders.TryGetValue(group, out var map)) { map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase); _propertyOrders[group] = map; }
                            map[prop] = ord;
                        }
                    }
                }
            }
            catch (Exception ex) { Log.Write(ex); }
        }
        public void ApplyEquipmentLanguage(object equipment)
        { if (equipment == null) return; ApplyLanguageToObject(equipment, equipment.GetType().Name, new HashSet<int>()); }
        private void ApplyLanguageToObject(object obj, string path, HashSet<int> visited)
        {
            if (obj == null) return; int id = RuntimeHelpers.GetHashCode(obj); if (!visited.Add(id)) return; var type = obj.GetType(); if (IsSimpleType(type)) return;
            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                try
                {
                    if (prop.GetCustomAttribute<BrowsableAttribute>() is BrowsableAttribute b && !b.Browsable) continue;
                    if (prop.GetCustomAttributes(true).Any(a => a.GetType().Name == "JsonIgnoreAttribute")) continue;
                    var val = prop.GetValue(obj); if (val == null) continue; 
                    string p = path + "." + prop.Name; 
                    TraverseChild(val, prop.PropertyType, p, visited, ApplyLanguageToObject);
                }
                catch (Exception ex) { Log.Write(ex); }
            }
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            { try { var ft = field.FieldType; if (IsSimpleType(ft)) continue; var fv = field.GetValue(obj); if (fv == null) continue; string fp = path + "." + field.Name; TraverseChild(fv, ft, fp, visited, ApplyLanguageToObject); } catch (Exception ex) { Log.Write(ex); } }
        }
        public string GetDisplayName(string path, string def = null) => _displayNames.TryGetValue(path, out var v) ? v : (def ?? path);
        public string GetCategory(string path, string def = null) => _categories.TryGetValue(path, out var v) ? v : (def ?? "General");

        private IEnumerable<KeyValuePair<string, int>> GetAllPropertyOrdersFlat()
        {
            foreach (var grp in _propertyOrders)
            {
                var groupName = grp.Key;
                foreach (var kv in grp.Value)
                    yield return new KeyValuePair<string, int>(groupName + "." + kv.Key, kv.Value);
            }
        }
        private IEnumerable<string> GetAllPropertyOrderGroups()
        {
            return _propertyOrders.Keys.ToList();
        }
        #endregion

        #region Alarm Scan
        public void ScanEquipmentAlarms(object equipment)
        { if (equipment == null) return; _alarmTitles.Clear(); _alarmCauses.Clear(); _alarmGrades.Clear(); ScanObjectAlarms(equipment, equipment.GetType().Name, new HashSet<int>()); }
        private void ScanObjectAlarms(object obj, string path, HashSet<int> visited)
        {
            if (obj == null) return; int id = RuntimeHelpers.GetHashCode(obj); if (!visited.Add(id)) return; var type = obj.GetType(); if (IsSimpleType(type)) return;
            var dicField = GetFieldRecursive(type, "m_dicAlarms"); if (dicField != null) TryScanAlarmDictionary(dicField.GetValue(obj), path); else foreach (var f in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(x => x.Name.IndexOf("alarm", StringComparison.OrdinalIgnoreCase) >= 0)) TryScanAlarmDictionary(f.GetValue(obj), path);
            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            { try { if (prop.GetCustomAttribute<BrowsableAttribute>() is BrowsableAttribute b && !b.Browsable) continue; if (prop.GetCustomAttributes(true).Any(a => a.GetType().Name == "JsonIgnoreAttribute")) continue; var val = prop.GetValue(obj); if (val == null) continue; HandleAlarmChild(val, prop.PropertyType, path + "." + prop.Name, visited); } catch (Exception ex) { Log.Write(ex); } }
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            { try { var ft = field.FieldType; if (IsSimpleType(ft)) continue; var fv = field.GetValue(obj); if (fv == null) continue; HandleAlarmChild(fv, ft, path + "." + field.Name, visited); } catch (Exception ex) { Log.Write(ex); } }
        }
        private void HandleAlarmChild(object val, Type t, string basePath, HashSet<int> visited)
        {
            if (IsDictionaryLike(t)) { TryScanAlarmDictionary(val, basePath); return; }
            if (LooksLikeAlarmInfo(t)) { ProcessAlarmCandidate(val, basePath); return; }
            if (IsUnitOrComponent(t)) { ScanObjectAlarms(val, basePath, visited); return; }
            if (val is IEnumerable en && !(val is string)) { int idx = 0; foreach (var item in en) { if (item == null) { idx++; continue; } HandleAlarmChild(item, item.GetType(), basePath + "[" + idx + "]", visited); idx++; } }
        }
        private void TryScanAlarmDictionary(object dict, string path)
        {
            if (dict == null) return; try
            {
                var dt = dict.GetType(); var keysProp = dt.GetProperty("Keys"); var itemProp = dt.GetProperty("Item"); var valuesProp = dt.GetProperty("Values");
                if (keysProp != null && itemProp != null) { var keys = keysProp.GetValue(dict) as IEnumerable; if (keys != null) foreach (var k in keys) { object info = null; try { info = itemProp.GetValue(dict, new object[] { k }); } catch (Exception ex) { Log.Write(ex); } ProcessAlarmCandidate(info, path); } }
                if (valuesProp != null) { var vals = valuesProp.GetValue(dict) as IEnumerable; if (vals != null) foreach (var v in vals) ProcessAlarmCandidate(v, path); }
            }
            catch (Exception ex) { Log.Write(ex); }
        }
        private void ProcessAlarmCandidate(object candidate, string path)
        {
            if (candidate == null) return; var t = candidate.GetType(); if (LooksLikeAlarmInfo(t)) { try { var codeP = t.GetProperty("Code"); var titleP = t.GetProperty("Title"); var causeP = t.GetProperty("Cause"); var sourceP = t.GetProperty("Source"); int code = codeP != null ? (int)codeP.GetValue(candidate) : 0; string source = sourceP?.GetValue(candidate)?.ToString(); string unitName = !string.IsNullOrWhiteSpace(source) ? source : path; string alarmKey = unitName + "." + code; string title = titleP?.GetValue(candidate)?.ToString(); string cause = causeP?.GetValue(candidate)?.ToString(); if (!string.IsNullOrWhiteSpace(title)) _alarmTitles[alarmKey] = title; if (!string.IsNullOrWhiteSpace(cause)) _alarmCauses[alarmKey] = cause; } catch (Exception ex) { Log.Write(ex); } return; }
            if (IsDictionaryLike(t)) { TryScanAlarmDictionary(candidate, path); return; }
            if (IsUnitOrComponent(t)) { ScanObjectAlarms(candidate, path + "." + t.Name, new HashSet<int>()); return; }
            if (candidate is IEnumerable en && !(candidate is string)) { int idx = 0; foreach (var item in en) { ProcessAlarmCandidate(item, path + "[" + idx + "]"); idx++; } }
        }
        public void SaveAlarmLanguage(string language)
        {
            try
            {
                string path = Path.Combine(_languageFolder, $"Alarm_{language}.ini");
                var existingTitle = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                var existingCause = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                if (File.Exists(path))
                {
                    string section = "";
                    foreach (var line in File.ReadAllLines(path, Encoding.UTF8))
                    {
                        var t = line.Trim(); if (t.Length == 0 || t.StartsWith(";") || t.StartsWith("#")) continue;
                        if (t.StartsWith("[") && t.EndsWith("]")) { section = t.Substring(1, t.Length - 2); continue; }
                        int idx = t.IndexOf('='); if (idx <= 0) continue; string key = t.Substring(0, idx).Trim(); string val = t.Substring(idx + 1).Trim();
                        val = DecodeMultiline(val);
                        if (section.Equals("AlarmTitle", StringComparison.OrdinalIgnoreCase)) existingTitle[key] = val;
                        else if (section.Equals("AlarmCause", StringComparison.OrdinalIgnoreCase)) existingCause[key] = val;
                    }
                }
                var mergedTitle = new Dictionary<string, string>(existingTitle, StringComparer.OrdinalIgnoreCase);
                foreach (var kv in _alarmTitles) if (!string.IsNullOrEmpty(kv.Key)) mergedTitle[kv.Key] = kv.Value;
                var mergedCause = new Dictionary<string, string>(existingCause, StringComparer.OrdinalIgnoreCase);
                foreach (var kv in _alarmCauses) if (!string.IsNullOrEmpty(kv.Key)) mergedCause[kv.Key] = kv.Value;

                var sb = new StringBuilder();
                sb.AppendLine("[AlarmTitle]"); foreach (var kv in mergedTitle.OrderBy(k => k.Key)) sb.AppendLine(kv.Key + " = " + EncodeMultiline(kv.Value));
                sb.AppendLine(); sb.AppendLine("[AlarmCause]"); foreach (var kv in mergedCause.OrderBy(k => k.Key)) sb.AppendLine(kv.Key + " = " + EncodeMultiline(kv.Value));
                File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
            }
            catch (Exception ex) { Log.Write(ex); }
        }
        public void LoadAlarmLanguage(string language)
        { try { string fp = Path.Combine(_languageFolder, $"Alarm_{language}.ini"); if (!File.Exists(fp)) return; _alarmTitles.Clear(); _alarmCauses.Clear(); _alarmGrades.Clear(); string section = ""; foreach (var line in File.ReadAllLines(fp, Encoding.UTF8)) { var t = line.Trim(); if (t.Length == 0 || t.StartsWith(";") || t.StartsWith("#")) continue; if (t.StartsWith("[") && t.EndsWith("]")) { section = t.Substring(1, t.Length - 2); continue; } int idx = t.IndexOf('='); if (idx <= 0) continue; string key = t.Substring(0, idx).Trim(); string val = t.Substring(idx + 1).Trim(); val = DecodeMultiline(val); if (section.Equals("AlarmTitle", StringComparison.OrdinalIgnoreCase)) _alarmTitles[key] = val; else if (section.Equals("AlarmCause", StringComparison.OrdinalIgnoreCase)) _alarmCauses[key] = val; } } catch (Exception ex) { Log.Write(ex); } }
        public void ApplyAlarmLanguage(object equipment) { if (equipment == null) return; ApplyAlarmToObject(equipment, equipment.GetType().Name, new HashSet<int>()); }
        private void ApplyAlarmToObject(object obj, string path, HashSet<int> visited)
        {
            if (obj == null) return; int id = RuntimeHelpers.GetHashCode(obj); if (!visited.Add(id)) return; var type = obj.GetType(); if (IsSimpleType(type)) return;
            var dicField = GetFieldRecursive(type, "m_dicAlarms"); if (dicField != null) TryApplyAlarmDictionary(dicField.GetValue(obj), path); else foreach (var f in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(x => x.Name.IndexOf("alarm", StringComparison.OrdinalIgnoreCase) >= 0)) TryApplyAlarmDictionary(f.GetValue(obj), path);
            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            { try { if (prop.GetCustomAttribute<BrowsableAttribute>() is BrowsableAttribute b && !b.Browsable) continue; if (prop.GetCustomAttributes(true).Any(a => a.GetType().Name == "JsonIgnoreAttribute")) continue; var val = prop.GetValue(obj); if (val == null) continue; ApplyAlarmChild(val, prop.PropertyType, path + "." + prop.Name, visited); } catch (Exception ex) { Log.Write(ex); } }
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            { try { var ft = field.FieldType; if (IsSimpleType(ft)) continue; var fv = field.GetValue(obj); if (fv == null) continue; ApplyAlarmChild(fv, ft, path + "." + field.Name, visited); } catch (Exception ex) { Log.Write(ex); } }
        }
        private void ApplyAlarmChild(object val, Type t, string basePath, HashSet<int> visited)
        {
            if (IsDictionaryLike(t)) { TryApplyAlarmDictionary(val, basePath); return; }
            if (LooksLikeAlarmInfo(t)) { ApplyAlarmCandidate(val, basePath); return; }
            if (IsUnitOrComponent(t)) { ApplyAlarmToObject(val, basePath, visited); return; }
            if (val is IEnumerable en && !(val is string)) { int idx = 0; foreach (var item in en) { if (item == null) { idx++; continue; } ApplyAlarmChild(item, item.GetType(), basePath + "[" + idx + "]", visited); idx++; } }
        }
        private void TryApplyAlarmDictionary(object dict, string path)
        {
            if (dict == null) return; try
            {
                var dt = dict.GetType(); var keysProp = dt.GetProperty("Keys"); var itemProp = dt.GetProperty("Item"); var valuesProp = dt.GetProperty("Values");
                if (keysProp != null && itemProp != null) { var keys = keysProp.GetValue(dict) as IEnumerable; if (keys != null) foreach (var k in keys) { object info = null; try { info = itemProp.GetValue(dict, new object[] { k }); } catch (Exception ex) { Log.Write(ex); } ApplyAlarmCandidate(info, path); } }
                if (valuesProp != null) { var vals = valuesProp.GetValue(dict) as IEnumerable; if (vals != null) foreach (var v in vals) ApplyAlarmCandidate(v, path); }
            }
            catch (Exception ex) { Log.Write(ex); }
        }
        private void ApplyAlarmCandidate(object candidate, string path)
        {
            if (candidate == null) return; var t = candidate.GetType(); if (LooksLikeAlarmInfo(t)) { try { var codeP = t.GetProperty("Code"); var titleP = t.GetProperty("Title"); var causeP = t.GetProperty("Cause"); var sourceP = t.GetProperty("Source"); int code = codeP != null ? (int)codeP.GetValue(candidate) : 0; string source = sourceP?.GetValue(candidate)?.ToString(); string unitName = !string.IsNullOrWhiteSpace(source) ? source : path; string alarmKey = unitName + "." + code; if (titleP != null && _alarmTitles.TryGetValue(alarmKey, out var title)) titleP.SetValue(candidate, title); if (causeP != null && _alarmCauses.TryGetValue(alarmKey, out var cause)) causeP.SetValue(candidate, cause); } catch (Exception ex) { Log.Write(ex); } return; }
            if (IsDictionaryLike(t)) { TryApplyAlarmDictionary(candidate, path); return; }
            if (IsUnitOrComponent(t)) { ApplyAlarmToObject(candidate, path + "." + t.Name, new HashSet<int>()); return; }
            if (candidate is IEnumerable en && !(candidate is string)) { int idx = 0; foreach (var item in en) { ApplyAlarmCandidate(item, path + "[" + idx + "]"); idx++; } }
        }
        #endregion

        #region Form Scan
        public void ScanFormControls(Form form) { if (form == null) return; ScanControls(form, form.GetType().Name); }
        private string BuildPath(string parent, string name) => string.IsNullOrWhiteSpace(name) ? parent : parent + "." + name;
        private string NormalizePath(string k) { if (string.IsNullOrWhiteSpace(k)) return k; while (k.Contains("..")) k = k.Replace("..", "."); if (k.EndsWith(".")) k = k.TrimEnd('.'); return k; }
        private void ScanControls(Control parent, string path)
        { if (parent == null) return; foreach (Control c in parent.Controls) { try { string cur = BuildPath(path, c.Name); bool collect = (c is Button || c is GroupBox || c is Label) && !string.IsNullOrWhiteSpace(c.Name) && !string.IsNullOrWhiteSpace(c.Text); if (collect) { cur = NormalizePath(cur); _formControls[cur] = c.Text; } if (c.HasChildren) ScanControls(c, cur); } catch (Exception ex) { Log.Write(ex); } } }
        public void SaveFormLanguage(string language)
        {
            try
            {
                string path = Path.Combine(_languageFolder, $"Form_{language}.ini");
                var existing = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                if (File.Exists(path))
                {
                    string section = "";
                    foreach (var line in File.ReadAllLines(path, Encoding.UTF8))
                    {
                        var t = line.Trim(); if (t.Length == 0 || t.StartsWith(";") || t.StartsWith("#")) continue;
                        if (t.StartsWith("[") && t.EndsWith("]")) { section = t.Substring(1, t.Length - 2); continue; }
                        int idx = t.IndexOf('='); if (idx <= 0) continue; string key = t.Substring(0, idx).Trim(); string val = t.Substring(idx + 1).Trim();
                        val = DecodeMultiline(val);
                        if (section.Equals("Form", StringComparison.OrdinalIgnoreCase) || section == "") existing[key] = val;
                    }
                }
                var merged = new Dictionary<string, string>(existing, StringComparer.OrdinalIgnoreCase);
                foreach (var kv in _formControls) merged[kv.Key] = kv.Value;

                var sb = new StringBuilder();
                sb.AppendLine("[Form]"); foreach (var kv in merged.OrderBy(k => k.Key)) sb.AppendLine(kv.Key + " = " + EncodeMultiline(kv.Value));
                File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
            }
            catch (Exception ex) { Log.Write(ex); }
        }
        public void LoadFormLanguage(string language) { try { string fp = Path.Combine(_languageFolder, $"Form_{language}.ini"); if (!File.Exists(fp)) return; _formControls.Clear(); foreach (var line in File.ReadAllLines(fp, Encoding.UTF8)) { var t = line.Trim(); if (t.Length == 0 || t.StartsWith(";") || t.StartsWith("#")) continue; if (t.StartsWith("[") && t.EndsWith("]")) continue; int idx = t.IndexOf('='); if (idx <= 0) continue; string key = t.Substring(0, idx).Trim(); string val = t.Substring(idx + 1).Trim(); key = NormalizePath(key); val = DecodeMultiline(val); if (!_formControls.ContainsKey(key)) _formControls[key] = val; } } catch (Exception ex) { Log.Write(ex); } }
        private void ApplyControlLanguage(Control parent, string path) { if (parent == null) return; foreach (Control c in parent.Controls) { try { string cur = NormalizePath(BuildPath(path, c.Name)); bool apply = (c is Button || c is GroupBox || c is Label) && !string.IsNullOrWhiteSpace(c.Name); if (apply && _formControls.TryGetValue(cur, out var txt)) c.Text = txt; if (c.HasChildren) ApplyControlLanguage(c, cur); } catch (Exception ex) { Log.Write(ex); } } }
        public void ApplyFormLanguage(Form form) { if (form == null) return; ApplyControlLanguage(form, form.GetType().Name); }
        public List<string> GetMissingFormKeys(Form form) { var missing = new List<string>(); if (form == null) return missing; void Walk(Control p, string pt) { foreach (Control c in p.Controls) { string cur = NormalizePath(BuildPath(pt, c.Name)); if (!string.IsNullOrWhiteSpace(c.Name)) { bool target = c is Button || c is GroupBox || c is Label; if (target && !_formControls.ContainsKey(cur)) missing.Add(cur); } if (c.HasChildren) Walk(c, cur); } } Walk(form, form.GetType().Name); return missing; }
        public string GetControlText(string path, string def = null) => _formControls.TryGetValue(path, out var v) ? v : (def ?? path);
        #endregion

        #region All Save/Load/Apply
        public void SaveLanguage(string language) { SaveEquipmentLanguage(language); SaveAlarmLanguage(language); SaveFormLanguage(language); }
        public void LoadLanguage(string language) { LoadEquipmentLanguage(language); LoadAlarmLanguage(language); LoadFormLanguage(language); }
        public void ApplyLanguage(object equipment, Form form) { if (equipment != null) { ApplyEquipmentLanguage(equipment); ApplyAlarmLanguage(equipment); } if (form != null) ApplyFormLanguage(form); }
        #endregion

        #region Misc
        public List<string> GetAvailableLanguages() { var langs = new HashSet<string>(StringComparer.OrdinalIgnoreCase); try { if (Directory.Exists(_languageFolder)) { foreach (var file in Directory.GetFiles(_languageFolder, "*.ini")) { var name = Path.GetFileNameWithoutExtension(file); var parts = name.Split('_'); if (parts.Length == 2) langs.Add(parts[1]); } } } catch (Exception ex) { Log.Write(ex); } return langs.OrderBy(x => x).ToList(); }
        public void CreateDefaultLanguageFiles(object equipment) { ScanEquipmentProperties(equipment); SaveEquipmentLanguage("Korean"); SaveEquipmentLanguage("English"); }
        #endregion
    }
}
