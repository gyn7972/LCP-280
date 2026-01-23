using System.ComponentModel;
using System.Windows.Forms;

namespace QMC.Common
{
    public static class DesignModeHelper
    {
        public static bool IsDesignMode(Control c)
        {
            return LicenseManager.UsageMode == LicenseUsageMode.Designtime
                   || (c?.Site?.DesignMode ?? false);
                 //|| (c?.DesignMode ?? false); // 보호됨 멤버에 접근 불가
        }
    }
}