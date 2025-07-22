using System;

namespace SP_GridTypeView
{
    public class PropertyState : PropertyBase
    {
        public bool State { get; set; }

        public PropertyState() : base()
        {
            State = false;
        }

        public PropertyState(string title, object value, bool state = false) : base(title, value)
        {
            State = state;
        }

        public override void SetValue(string text)
        {
            base.SetValue(text);
            if (bool.TryParse(text, out bool result))
                State = result;
        }
    }
}
