using System;
using System.Reflection;
using System.Windows.Forms;

namespace QMC.Common
{
    /// <summary>
    /// Wraps a Form inside a UserControl so hosts can uniformly treat child views as Controls.
    /// Also implements IResizable and forwards size updates to the wrapped view.
    /// </summary>
    public class FormAdapterControl : UserControl, IResizable
    {
        public Control InnerControl { get; private set; }
        public Form InnerForm { get; private set; }

        public FormAdapterControl(Form form)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));
            InnerForm = form;

            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;

            InnerControl = form;
            Controls.Add(form);
            form.Show();
        }

        public void SetPanelSize(int width, int height)
        {
            // Prefer IResizable then reflection
            var resizable = InnerControl as IResizable;
            if (resizable != null)
            {
                resizable.SetPanelSize(width, height);
                return;
            }

            try
            {
                MethodInfo mi = InnerControl.GetType().GetMethod(
                    "SetPanelSize",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null,
                    new Type[] { typeof(int), typeof(int) },
                    null);
                if (mi != null)
                {
                    mi.Invoke(InnerControl, new object[] { width, height });
                }
            }
            catch { }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    if (InnerForm != null)
                    {
                        InnerForm.Dispose();
                        InnerForm = null;
                    }
                }
                catch { }
            }
            base.Dispose(disposing);
        }
    }
}
