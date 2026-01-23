using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Forms;

namespace QMC.Common
{
    /// <summary>
    /// LanguageManager 사용을 돕는 확장 메서드들
    /// </summary>
    public static class LanguageManagerExtensions
    {
        /// <summary>
        /// PropertyCollection에서 속성 경로를 생성하여 언어 적용
        /// </summary>
        public static void ApplyLanguageToPropertyCollection(this LanguageManager manager, PropertyCollection pc, string basePath)
        {
            if (pc == null || manager == null)
                return;

            foreach (var prop in pc)
            {
                try
                {
                    var propType = prop.GetType();
                    var titleProp = propType.GetProperty("Title");
                    if (titleProp != null)
                    {
                        string title = titleProp.GetValue(prop)?.ToString();
                        if (!string.IsNullOrWhiteSpace(title))
                        {
                            string propPath = $"{basePath}.{title}";
                            string localizedTitle = manager.GetDisplayName(propPath, title);
                            titleProp.SetValue(prop, localizedTitle);
                        }
                    }

                    var categoryProp = propType.GetProperty("Category");
                    if (categoryProp != null)
                    {
                        string category = categoryProp.GetValue(prop)?.ToString();
                        if (!string.IsNullOrWhiteSpace(category))
                        {
                            string propPath = $"{basePath}.{category}";
                            string localizedCategory = manager.GetCategory(propPath, category);
                            categoryProp.SetValue(prop, localizedCategory);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }
            }
        }

        /// <summary>
        /// ConfigReflectionMapper에 언어 적용
        /// </summary>
        public static void ApplyLanguageToMapper(this LanguageManager manager, ConfigReflectionMapper mapper, string configTypeName)
        {
            if (mapper == null || manager == null || mapper.PropertyCollection == null)
                return;

            manager.ApplyLanguageToPropertyCollection(mapper.PropertyCollection, configTypeName);
        }

        /// <summary>
        /// Form의 모든 하위 Form도 재귀적으로 언어 적용
        /// </summary>
        public static void ApplyLanguageRecursive(this LanguageManager manager, Form form)
        {
            if (form == null || manager == null)
                return;

            manager.ApplyFormLanguage(form);

            // MDI 자식 폼들도 적용
            if (form.IsMdiContainer)
            {
                foreach (Form child in form.MdiChildren)
                {
                    manager.ApplyLanguageRecursive(child);
                }
            }

            // Owned Forms도 적용
            foreach (Form owned in form.OwnedForms)
            {
                manager.ApplyLanguageRecursive(owned);
            }
        }

        /// <summary>
        /// 특정 Type의 Category/DisplayName Attribute를 언어 매니저에 수집
        /// </summary>
        public static void CollectTypeAttributes(this LanguageManager manager, Type type, string basePath = null)
        {
            if (type == null || manager == null)
                return;

            if (string.IsNullOrWhiteSpace(basePath))
                basePath = type.Name;

            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in props)
            {
                var browsable = prop.GetCustomAttribute<BrowsableAttribute>();
                if (browsable != null && !browsable.Browsable)
                    continue;

                string propPath = $"{basePath}.{prop.Name}";

                var categoryAttr = prop.GetCustomAttribute<CategoryAttribute>();
                var displayAttr = prop.GetCustomAttribute<DisplayNameAttribute>();

                // 이 메서드는 내부적으로 딕셔너리에 저장하도록 LanguageManager 수정 필요
                // 현재는 ScanEquipmentProperties를 사용하는 것이 권장됨
            }
        }

        /// <summary>
        /// Equipment의 특정 Unit이나 Component만 스캔
        /// </summary>
        public static void ScanSpecificUnit(this LanguageManager manager, object unit, string unitName)
        {
            if (unit == null || manager == null)
                return;

            // 일시적으로 기존 딕셔너리 백업
            var originalDisplayNames = new System.Collections.Generic.Dictionary<string, string>();
            var originalCategories = new System.Collections.Generic.Dictionary<string, string>();

            // 스캔 (내부적으로 딕셔너리 업데이트)
            var visited = new System.Collections.Generic.HashSet<object>();
            // manager의 private 메서드에 접근할 수 없으므로, 전체 스캔 후 필터링 필요
            // 또는 LanguageManager에 public 메서드 추가 필요
        }
    }

    /// <summary>
    /// Form에 언어 변경 기능을 쉽게 추가하기 위한 Helper
    /// </summary>
    public class FormLanguageHelper
    {
        private readonly Form _form;
        private readonly LanguageManager _manager;

        public FormLanguageHelper(Form form)
        {
            _form = form;
            _manager = LanguageManager.Instance;
        }

        /// <summary>
        /// Form 로드 시 호출 - 현재 언어 적용
        /// </summary>
        public void OnFormLoad()
        {
            _manager.ApplyFormLanguage(_form);
        }

        /// <summary>
        /// 언어 변경 콤보박스 설정
        /// </summary>
        public void SetupLanguageComboBox(ComboBox comboBox)
        {
            var languages = _manager.GetAvailableLanguages();
            comboBox.Items.Clear();
            foreach (var lang in languages)
            {
                comboBox.Items.Add(lang);
            }

            comboBox.SelectedItem = _manager.CurrentLanguage;

            comboBox.SelectedIndexChanged += (s, e) =>
               {
                   if (comboBox.SelectedItem != null)
                   {
                       string selectedLang = comboBox.SelectedItem.ToString();
                       _manager.CurrentLanguage = selectedLang;
                       _manager.ApplyFormLanguage(_form);
                   }
               };
        }

        /// <summary>
        /// 언어 변경 메뉴 아이템 설정
        /// </summary>
        public void SetupLanguageMenu(ToolStripMenuItem parentMenu)
        {
            parentMenu.DropDownItems.Clear();

            var languages = _manager.GetAvailableLanguages();
            foreach (var lang in languages)
            {
                var item = new ToolStripMenuItem(lang);
                item.Checked = lang.Equals(_manager.CurrentLanguage, StringComparison.OrdinalIgnoreCase);
                item.Click += (s, e) =>
{
    _manager.CurrentLanguage = lang;
    _manager.ApplyFormLanguage(_form);

    // 체크 표시 업데이트
    foreach (ToolStripMenuItem menuItem in parentMenu.DropDownItems)
    {
        menuItem.Checked = menuItem.Text.Equals(lang, StringComparison.OrdinalIgnoreCase);
    }
};
                parentMenu.DropDownItems.Add(item);
            }
        }
    }
}
