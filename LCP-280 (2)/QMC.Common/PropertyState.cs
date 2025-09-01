using System;

namespace QMC.Common
{
    public class PropertyState : PropertyBase
    {
        public bool State { get; set; }
        public bool ShowNoColumn { get; set; }

        public PropertyState(string no, string name, bool state, bool showNoColumn = true)
            : base(no, name)
        {
            State = state;
            ShowNoColumn = showNoColumn;
        }
    }
}
