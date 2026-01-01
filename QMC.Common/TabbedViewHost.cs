using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace QMC.Common
{
    /// <summary>
    /// Common Tab host that loads forms registered for a MenuButtonType using FormManager.
    /// Replaces duplicated code in FormMain/FormConfig/FormManual/FormRecipe/FormSetup/FormLog.
    /// Keeps lazy-loading and SetPanelSize behavior.
    /// </summary>
    public class TabbedViewHost : UserControl, IResizable
    {
        private readonly MenuButtonType _category;
        private readonly TabControl _tab;
        private readonly Dictionary<TabPage, Control> _tabInstances = new Dictionary<TabPage, Control>();

        private int _tabHeight = 28;
        private Font _tabFont = new Font("¸¼Àº °íµñ", 9, FontStyle.Regular);

        public TabbedViewHost(MenuButtonType category)
        {
            _category = category;
            BackColor = Color.White;

            _tab = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = _tabFont,
                DrawMode = TabDrawMode.OwnerDrawFixed,
                ItemSize = new Size(120, _tabHeight),
                SizeMode = TabSizeMode.Fixed,
                BackColor = Color.White
            };
            _tab.DrawItem += Tab_DrawItem;
            _tab.SelectedIndexChanged += Tab_SelectedIndexChanged;

            Controls.Add(_tab);

            LoadTabs();
        }

        private List<FormInfo> GetForms()
        {
            switch (_category)
            {
                case MenuButtonType.Main: return FormManager.Instance.GetRegisteredForms(MenuButtonType.Main);
                case MenuButtonType.Config: return FormManager.Instance.GetRegisteredForms(MenuButtonType.Config);
                case MenuButtonType.Menual: return FormManager.Instance.GetRegisteredForms(MenuButtonType.Menual);
                case MenuButtonType.Recipe: return FormManager.Instance.GetRegisteredForms(MenuButtonType.Recipe);
                case MenuButtonType.Setup: return FormManager.Instance.GetRegisteredForms(MenuButtonType.Setup);
                case MenuButtonType.Log: return FormManager.Instance.GetRegisteredForms(MenuButtonType.Log);
                default: return new List<FormInfo>();
            }
        }

        private void LoadTabs()
        {
            var forms = GetForms();
            if (forms.Count == 0)
            {
                CreateSampleTab();
                return;
            }

            foreach (var info in forms)
            {
                var tab = new TabPage(info.DisplayName) { BackColor = Color.White, Tag = info };
                _tab.TabPages.Add(tab);
            }
        }

        private void CreateSampleTab()
        {
            var tab = new TabPage("Sample") { BackColor = Color.White };
            var label = new Label
            {
                Text = $"No {_category} Forms Registered\n\nUse FormManager.Instance.RegisterForm()",
                Font = new Font("¸¼Àº °íµñ", 12, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };
            tab.Controls.Add(label);
            _tab.TabPages.Add(tab);
        }

        // Public warm-up to preload first tab content (called from MainForm after startup)
        public void WarmUp()
        {
            var page = _tab.SelectedTab;
            if (page != null)
            {
                LoadPageContents(page);
            }
        }

        private void Tab_SelectedIndexChanged(object sender, EventArgs e)
        {
            var page = _tab.SelectedTab;
            if (page == null) return;
            LoadPageContents(page);
        }

        private void LoadPageContents(TabPage page)
        {
            if (!_tabInstances.ContainsKey(page))
            {
                if (!(page.Tag is FormInfo info)) return;
                try
                {
                    var form = FormManager.Instance.CreateFormInstance(info);
                    var adapter = new FormAdapterControl(form) { Dock = DockStyle.Fill };
                    page.Controls.Clear();
                    page.Controls.Add(adapter);
                    _tabInstances[page] = adapter;

                    // apply size without tab header height
                    var available = GetAvailableClientSize();
                    adapter.SetPanelSize(available.Width, available.Height);
                }
                catch (Exception ex)
                {
                    var error = new Label
                    {
                        Text = $"Æû ·Îµå ½ÇÆÐ: {info.DisplayName}\r\n{ex.Message}",
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleCenter,
                        Font = new Font("¸¼Àº °íµñ", 12, FontStyle.Bold),
                        ForeColor = Color.Red
                    };
                    page.Controls.Clear();
                    page.Controls.Add(error);
                }
            }
            else
            {
                var control = _tabInstances[page] as IResizable;
                if (control != null)
                {
                    var available = GetAvailableClientSize();
                    control.SetPanelSize(available.Width, available.Height);
                }
            }
        }

        private void Tab_DrawItem(object sender, DrawItemEventArgs e)
        {
            TabPage page = _tab.TabPages[e.Index];
            Rectangle tabRect = _tab.GetTabRect(e.Index);
            Color backColor = (e.Index == _tab.SelectedIndex) ? Color.White : Color.Gainsboro;
            using (Brush backBrush = new SolidBrush(backColor))
            {
                e.Graphics.FillRectangle(backBrush, tabRect);
            }
            TextRenderer.DrawText(e.Graphics, page.Text, _tab.Font, tabRect, Color.Black,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        private Size GetAvailableClientSize()
        {
            // approximate header height using ItemSize.Height
            int header = _tab.ItemSize.Height + 2;
            return new Size(_tab.ClientSize.Width, Math.Max(0, _tab.ClientSize.Height - header));
        }

        public void SetPanelSize(int width, int height)
        {
            this.Size = new Size(width, height);
            this.ClientSize = new Size(width, height);

            if (_tab != null)
            {
                _tab.Size = new Size(width, height);
                var page = _tab.SelectedTab;
                if (page != null && _tabInstances.ContainsKey(page))
                {
                    var resizable = _tabInstances[page] as IResizable;
                    if (resizable != null)
                    {
                        var available = GetAvailableClientSize();
                        resizable.SetPanelSize(available.Width, available.Height);
                    }
                }
                _tab.Invalidate();
                _tab.Update();
            }

            Invalidate();
            Update();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var c in _tabInstances.Values)
                {
                    c.Dispose();
                }
                _tabInstances.Clear();
            }
            base.Dispose(disposing);
        }
    }
}
