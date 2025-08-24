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
            // Count how many forms are registered for this category
            List<FormInfo> forms = FormManager.Instance.GetRegisteredForms(menuType);
            int count = forms != null ? forms.Count : 0;

            if (count >= 2)
            {
                // Use generic tabbed host
                return new TabbedViewHost(menuType);
            }

            if (count == 1)
            {
                var form = FormManager.Instance.CreateFormInstance(forms[0]);
                return new FormAdapterControl(form) { Dock = DockStyle.Fill };
            }

            // 0 registered -> create a default form using the category-specific manager
            Form defaultForm = CreateDefaultForm(menuType);
            return new FormAdapterControl(defaultForm) { Dock = DockStyle.Fill };
        }

        private Form CreateDefaultForm(MenuButtonType menuType)
        {
            switch (menuType)
            {
                case MenuButtonType.Main: return FormManagerMain.Instance.CreateMainForm();
                case MenuButtonType.Config: return FormManagerConfig.Instance.CreateConfigForm();
                case MenuButtonType.Working: return FormManagerWorking.Instance.CreateWorkingForm();
                case MenuButtonType.Recipe: return FormManagerRecipe.Instance.CreateRecipeForm();
                case MenuButtonType.Setup: return FormManagerSetup.Instance.CreateSetupForm();
                case MenuButtonType.Log: return FormManagerLog.Instance.CreateLogForm();
                default: return new Form();
            }
        }
    }
}
