using QMC.Common;
using QMC.Common.Component;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Component
{
    public partial class UnitConfig : UserControl
    {
        private object _config;
        private ConfigReflectionMapper _mapper;

        // 새 옵션
        public bool AutoReloadOnActivate { get; set; } = true;

        public UnitConfig()
        {
            InitializeComponent();
            this.VisibleChanged += UnitConfig_VisibleChanged;
        }

        public void BindConfig(object config)
        {
            _config = config;
            RebuildMapperAndView();
        }

        private void UnitConfig_VisibleChanged(object sender, EventArgs e)
        {
            if (!AutoReloadOnActivate) 
                return;
            if (!Visible) 
                return;
            if (_config == null) 
                return;

            // 다시 보일 때 자동 재로드
            if (IsDirty())
            {
                //var r = MessageBox.Show(
                //    "저장되지 않은 변경이 있습니다.\r\n무시하고 파일에서 다시 읽을까요?",
                //    "자동 재로드",
                //    MessageBoxButtons.YesNo,
                //    MessageBoxIcon.Question);
                //if (r != DialogResult.Yes) return;
            }

            InvokeLoad(_config);
            RebuildMapperAndView();
        }

        private bool IsDirty()
        {
            if (_config == null || _mapper == null) return false;
            var pc = configurationPropertyView.GetCurrentProperties();
            if (pc == null) return false;

            var t = _config.GetType();
            foreach (var p in pc)
            {
                var pi = t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                          .FirstOrDefault(x =>
                          {
                              var dn = x.GetCustomAttributes(true)
                                  .OfType<System.ComponentModel.DisplayNameAttribute>()
                                  .FirstOrDefault()?.DisplayName;
                              return string.Equals(dn ?? x.Name, p.Title, StringComparison.OrdinalIgnoreCase);
                          });
                if (pi == null) continue;

                object cur = null;
                try { cur = pi.GetValue(_config); } catch { }
                object ui = p.Value;

                if (cur == null && ui == null) continue;
                if (cur == null || ui == null) return true;

                if (cur is IConvertible && ui is IConvertible)
                {
                    try
                    {
                        double d1 = Convert.ToDouble(cur);
                        double d2 = Convert.ToDouble(ui);
                        if (Math.Abs(d1 - d2) > 1e-9) return true;
                        continue;
                    }
                    catch { }
                }
                if (!Equals(cur, ui)) return true;
            }
            return false;
        }



        private void btnApplyConfig_Click(object sender, EventArgs e)
        {
             if (_mapper == null || _config == null) 
                return;

            try
            {
                configurationPropertyView.Apply();
                var pc = configurationPropertyView.GetCurrentProperties();
                if (pc != null) 
                    _mapper.ApplyToObject(pc);

                InvokeSave(_config);
                MessageBox.Show("저장 완료", "Config", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("저장 실패: " + ex.Message, "Config", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnReloadConfig_Click(object sender, EventArgs e)
        {
            if (_config == null) 
                return;

            if (IsDirty())
            {
                if (MessageBox.Show("저장되지 않은 변경 사항이 있습니다. 폐기 후 다시 로드할까요?",
                                    "Reload", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;
            }
            InvokeLoad(_config);
            RebuildMapperAndView();
        }

        private void InvokeSave(object cfg)
        {
            var t = cfg.GetType();
            // Saveconfig → Save
            var mi = t.GetMethod("Saveconfig", BindingFlags.Public | BindingFlags.Instance)
                  ?? t.GetMethod("Save", BindingFlags.Public | BindingFlags.Instance);
            try
            {
                if (mi != null && mi.GetParameters().Length == 0)
                    mi.Invoke(cfg, null);
            }
            catch (Exception ex) 
            {
                Log.Write(ex);
            }
        }

        private void InvokeLoad(object cfg)
        {
            var t = cfg.GetType();
            // 우선순위: LoadAndBindAxes(axisManager) → Loadconfig() → Load()
            // axisManager 없는 경우 단순 Load 사용
            MethodInfo mi = null;
            object[] args = null;

            // LoadAndBindAxes(MotionAxisManager) 시그니처 탐색
            mi = t.GetMethod("LoadAndBindAxes", BindingFlags.Public | BindingFlags.Instance);
            if (mi != null)
            {
                var ps = mi.GetParameters();
                if (ps.Length == 1 && ps[0].ParameterType.Name.Contains("MotionAxisManager"))
                {
                    // AxisManager 구할 수 있으면 전달, 없으면 무시하고 Load로 fallback
                    var eqType = Type.GetType("QMC.LCP_280.Process.Equipment");
                    object axisMgr = null;
                    try
                    {
                        var instProp = eqType?.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                        var eqInstance = instProp?.GetValue(null);
                        axisMgr = eqInstance?.GetType().GetProperty("AxisManager")?.GetValue(eqInstance);
                    }
                    catch { }
                    if (axisMgr != null) { args = new[] { axisMgr }; }
                    else { mi = null; } // fallback
                }
                else mi = null;
            }

            if (mi == null)
                mi = t.GetMethod("Loadconfig", BindingFlags.Public | BindingFlags.Instance); // (있다면)
            if (mi == null)
                mi = t.GetMethod("Load", BindingFlags.Public | BindingFlags.Instance);

            try
            {
                if (mi != null && mi.GetParameters().Length == (args?.Length ?? 0))
                    mi.Invoke(cfg, args);
            }
            catch { }
        }

        private void RebuildMapperAndView()
        {
            if (_config == null)
            {
                configurationPropertyView.SetProperties(new PropertyCollection());
                _mapper = null;
                return;
            }

            _mapper = new ConfigReflectionMapper(_config);
            var pc = _mapper.PropertyCollection;
            var type = _config.GetType();

            // 0) Provider (선택)
            var provider = _config as IPropertyOrderProvider;
            var providerOrder = provider?.GetPropertyOrder()
                                 ?.Select((n,i)=> new { NameOrDisplay = n?.Trim(), Ord = i })
                                 ?.Where(x => !string.IsNullOrWhiteSpace(x.NameOrDisplay))
                                 ?.ToDictionary(x => x.NameOrDisplay, x => x.Ord,
                                     StringComparer.OrdinalIgnoreCase)
                                 ?? new Dictionary<string,int>(StringComparer.OrdinalIgnoreCase);

    var categoryPriorityMap = provider?.GetCategoryOrder()
                             ?? new Dictionary<string,int>(StringComparer.OrdinalIgnoreCase);

    // 1) 속성 메타 (DisplayName / DisplayOrder / CategoryOrder)
    var propMeta = type
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(p => p.CanRead && p.CanWrite)
        .Select(p =>
        {
            var dn = p.GetCustomAttributes(true)
                      .OfType<DisplayNameAttribute>()
                      .FirstOrDefault()?.DisplayName;
            var display = string.IsNullOrWhiteSpace(dn) ? p.Name : dn;

            var attrOrder = p.GetCustomAttributes(true)
                             .OfType<DisplayOrderAttribute>()
                             .FirstOrDefault()?.Order;

            var catOrderAttr = p.GetCustomAttributes(true)
                                .OfType<CategoryOrderAttribute>()
                                .FirstOrDefault()?.Order;

            // Provider 순서 (없으면 null)
            providerOrder.TryGetValue(display, out var provOrd1);
            if (!providerOrder.TryGetValue(p.Name, out var provOrd2)) provOrd2 = provOrd1;

            int provOrd = provOrd1 != 0 || providerOrder.ContainsKey(display) ? provOrd1 :
                          (providerOrder.ContainsKey(p.Name) ? provOrd2 : int.MaxValue);

            return new
            {
                Prop = p,
                PropName = p.Name,
                Display = display,
                AttrOrder = attrOrder,      // Attribute 우선
                ProviderOrder = provOrd,    // 그 다음 Provider
                CategoryOrderAttr = catOrderAttr
            };
        })
        .ToList();

    // 2) 내부 리스트
    var listField = typeof(PropertyCollection)
                    .GetField("_properties", BindingFlags.NonPublic | BindingFlags.Instance);
    var list = listField?.GetValue(pc) as IList;
    if (list != null)
    {
        var items = list.Cast<object>().ToList();
        list.Clear();

        int seq = 0;
        var sortable = items
            .Select(o =>
            {
                var tItem = o.GetType();
                var titleProp = tItem.GetProperty("Title") ?? tItem.GetProperty("Name");
                string title = titleProp?.GetValue(o)?.ToString() ?? "";

                bool isHeader = tItem.Name.Contains("TitleOnlyProperty");

                // Category 추출
                string category = "";
                var catProp = tItem.GetProperty("Category");
                if (catProp != null)
                    category = catProp.GetValue(o)?.ToString() ?? "";

                // 헤더면 Title 을 Category 로 사용 (Provider 매핑 가능)
                if (isHeader && string.IsNullOrEmpty(category))
                    category = title;

                // 매칭
                var meta = propMeta.FirstOrDefault(m =>
                    string.Equals(m.Display, title, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(m.PropName, title, StringComparison.OrdinalIgnoreCase));

                // 속성 row 이고 Category 비어있으면 Reflection 으로 CategoryAttribute 추출 또는 General 기본
                if (!isHeader && string.IsNullOrEmpty(category))
                {
                    var catAttr = meta?.Prop?.GetCustomAttributes(true)
                                       .OfType<CategoryAttribute>()
                                       .FirstOrDefault();
                    if (catAttr != null)
                        category = catAttr.Category;
                    if (string.IsNullOrEmpty(category))
                        category = "General"; // fallback
                }

                // Property 순서 계산 (Attribute 우선 → Provider)
                int orderByAttr = meta?.AttrOrder ?? int.MaxValue;
                int orderByProvider = meta?.ProviderOrder ?? int.MaxValue;
                int effectiveOrder = (orderByAttr != int.MaxValue) ? orderByAttr : orderByProvider;

                // Category 우선순위
                int catOrdAttr = meta?.CategoryOrderAttr ?? 1000;
                int catOrdProv = categoryPriorityMap.TryGetValue(category, out var co) ? co : 1000;
                int catEffective = (meta?.CategoryOrderAttr != null) ? catOrdAttr : catOrdProv;

                // 최종 정렬 키
                long baseKey = (long)catEffective * 1_000_000L;
                long withinKey;
                if (isHeader)
                    withinKey = 0;
                else
                {
                    int propOrderInside = (effectiveOrder == int.MaxValue) ? 500_000 : (effectiveOrder + 1);
                    withinKey = propOrderInside;
                }
                long sortKey = baseKey + withinKey;

                return new
                {
                    Obj = o,
                    SortKey = sortKey,
                    Seq = seq++
                };
            })
            .OrderBy(x => x.SortKey)
            .ThenBy(x => x.Seq)
            .ToList();

        foreach (var x in sortable)
            list.Add(x.Obj);
    }

    configurationPropertyView.GroupName = type.Name;
    configurationPropertyView.SetProperties(pc);
}
    }
}