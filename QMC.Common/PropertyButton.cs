using System;

namespace QMC.Common
{
    [Serializable]
    public class PropertyButton : PropertyBase
    {
        public string Label { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public Action OnClick { get; set; }

        public PropertyButton(string no, string label, Action onClick)
        {
            this.No = no;          // PropertyBase에 No, Name 같은 공용 필드가 보통 있음
            this.Name = label;
            this.Label = label;
            this.OnClick = onClick;
        }
    }
}
