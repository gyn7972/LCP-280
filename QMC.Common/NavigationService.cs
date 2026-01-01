using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace QMC.Common
{
    /// <summary>
    /// Centralizes mapping from MenuButtonType to a view host using existing FormManager* classes.
    /// Preserves existing behaviors: auto register, 2+ -> TabControl host, 1 -> single form, 0 -> default.
    /// Now returns Control so MainForm can host UserControls directly.
    /// </summary>
    public sealed class NavigationService
    {
        private static readonly Lazy<NavigationService> _instance = new Lazy<NavigationService>(() => new NavigationService());
        public static NavigationService Instance => _instance.Value;

        private NavigationService() { }

        // Backward compatible API: still returns Form when needed (wrapped control inside a minimal host form)
        public Form CreateCenterForm(MenuButtonType menuType)
        {
            var control = CreateCenterControl(menuType);
            // If the control is already a Form, it can be hosted directly; otherwise wrap into a minimal form
            var form = control as Form;
            if (form != null) return form;

            var host = new Form
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill,
                BackColor = System.Drawing.Color.White
            };
            host.Controls.Add(control);
            control.Dock = DockStyle.Fill;
            return host;
        }

        public Control CreateCenterControl(MenuButtonType menuType)
        {
            // 각 메뉴는 전용 탭 호스트(FormX)를 항상 사용하여 탭 헤더 높이 제외 로직을 일관 적용
            switch (menuType)
            {
                case MenuButtonType.Main:
                    return new FormAdapterControl(new FormMain()) { Dock = DockStyle.Fill };
                case MenuButtonType.Config:
                    return new FormAdapterControl(new FormConfig()) { Dock = DockStyle.Fill };
                case MenuButtonType.Menual:
                    return new FormAdapterControl(new FormMenual()) { Dock = DockStyle.Fill };
                case MenuButtonType.Recipe:
                    return new FormAdapterControl(new FormRecipe()) { Dock = DockStyle.Fill };
                case MenuButtonType.Setup:
                    return new FormAdapterControl(new FormSetup()) { Dock = DockStyle.Fill };
                case MenuButtonType.Log:
                    return new FormAdapterControl(new FormLog()) { Dock = DockStyle.Fill };
            }

            // Fallback: 기존 count 기반 로직
            List<FormInfo> forms = FormManager.Instance.GetRegisteredForms(menuType);
            int count = forms != null ? forms.Count : 0;

            if (count >= 2)
            {
                return new TabbedViewHost(menuType);
            }

            if (count == 1)
            {
                var form = FormManager.Instance.CreateFormInstance(forms[0]);
                return new FormAdapterControl(form) { Dock = DockStyle.Fill };
            }

            Form defaultForm = CreateDefaultForm(menuType);
            return new FormAdapterControl(defaultForm) { Dock = DockStyle.Fill };
        }

        private Form CreateDefaultForm(MenuButtonType menuType)
        {
            switch (menuType)
            {
                case MenuButtonType.Main: return FormManagerMain.Instance.CreateMainForm();
                case MenuButtonType.Config: return FormManagerConfig.Instance.CreateConfigForm();
                case MenuButtonType.Menual: return FormManagerMenual.Instance.CreateMenaulForm();
                case MenuButtonType.Recipe: return FormManagerRecipe.Instance.CreateRecipeForm();
                case MenuButtonType.Setup: return FormManagerSetup.Instance.CreateSetupForm();
                case MenuButtonType.Log: return FormManagerLog.Instance.CreateLogForm();
                default: return new Form();
            }
        }
    }
}
