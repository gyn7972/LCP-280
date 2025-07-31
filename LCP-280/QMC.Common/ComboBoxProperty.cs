using System;
using System.Collections.Generic;

namespace QMC.Common
{
    public class ComboBoxProperty : PropertyBase
    {
        public List<string> Options { get; }

        public ComboBoxProperty(string title, string selectedValue, List<string> options) : base(title, selectedValue)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public override void SetValue(string text)
        {
            if (Options.Contains(text))
            {
                base.SetValue(text);
            }
        }
    }
}