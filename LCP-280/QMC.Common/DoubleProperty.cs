

namespace QMC.Common
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

    public class BoolProperty : PropertyBase
    {
        public new bool Value
        {
            get
            {
                if (base.Value is bool b)
                    return b;
                return false;
            }
            set
            {
                base.Value = value;
            }
        }

        public BoolProperty() : base()
        {
            base.Value = false;
        }

        public BoolProperty(string title, bool value) : base(title, value)
        {
        }
    }

    public class IntProperty : PropertyBase
    {
        public new int Value
        {
            get
            {
                if (base.Value is int i)
                    return i;
                return 0;
            }
            set
            {
                base.Value = value;
            }
        }

        public IntProperty() : base()
        {
            base.Value = 0;
        }

        public IntProperty(string title, int value) : base(title, value)
        {
        }
    }

    public class StringProperty : PropertyBase
    {
        public new string Value
        {
            get
            {
                if (base.Value is string s)
                    return s;
                return string.Empty;
            }
            set
            {
                base.Value = value;
            }
        }

        public StringProperty() : base()
        {
            base.Value = string.Empty;
        }

        public StringProperty(string title, string value) : base(title, value)
        {
        }
    }

    public class FloatProperty : PropertyBase
    {
        public new float Value
        {
            get
            {
                if (base.Value is float f)
                    return f;
                return 0f;
            }
            set
            {
                base.Value = value;
            }
        }

        public FloatProperty() : base()
        {
            base.Value = 0f;
        }

        public FloatProperty(string title, float value) : base(title, value)
        {
        }
    }

    public class LongProperty : PropertyBase
    {
        public new long Value
        {
            get
            {
                if (base.Value is long l)
                    return l;
                return 0L;
            }
            set
            {
                base.Value = value;
            }
        }

        public LongProperty() : base()
        {
            base.Value = 0L;
        }

        public LongProperty(string title, long value) : base(title, value)
        {
        }
    }
}