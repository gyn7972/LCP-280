namespace SP_GridTypeView
{
    public class DoubleProperty : PropertyBase
    {
        public new double Value
        {
            get
            {
                if (base.Value is double d)
                    return d;
                return 0.0;
            }
            set
            {
                base.Value = value;
            }
        }

        public DoubleProperty() : base()
        {
            base.Value = 0.0;
        }

        public DoubleProperty(string title, double value) : base(title, value)
        {
        }
    }
}