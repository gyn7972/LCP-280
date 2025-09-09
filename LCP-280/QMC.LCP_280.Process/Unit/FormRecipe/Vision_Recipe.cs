using System;
using System.Windows.Forms;
using System.ComponentModel; // 디자인 타임 가드
using QMC.Common;

namespace QMC.LCP_280.Process.Unit.FormRecipe
{
    public partial class Vision_Recipe : Form
    {
        public Vision_Recipe()
        {
            InitializeComponent();
            this.Load += Vision_Recipe_Load;
        }

        private void Vision_Recipe_Load(object sender, EventArgs e)
        {
            // 디자이너일 때는 아무 것도 하지 않음
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;

            // 만약 남아있는 Fill이 있으면 제거(안전보정)
            if (patternMatchingControl1.Dock == DockStyle.Fill)
            {
                patternMatchingControl1.Dock = DockStyle.None;
                patternMatchingControl1.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                // 최소 크기 보장 (디자이너에서 지정한 값보다 작아지지 않도록)
                patternMatchingControl1.MinimumSize = new System.Drawing.Size(900, 600);
            }

            try
            {
                var eq = Equipment.Instance;
                if (eq != null && eq.Cameras.Count > 0)
                    patternMatchingControl1?.SetCameras(eq.Cameras.Values);
            }
            catch { /* swallow at runtime */ }
        }
    }
}
