using System;

namespace QMC.Common
{
    /// <summary>
    /// Provides a consistent way for hosted views/forms to receive panel size updates
    /// without relying on reflection.
    /// </summary>
    public interface IResizable
    {
        void SetPanelSize(int width, int height);
    }
}
