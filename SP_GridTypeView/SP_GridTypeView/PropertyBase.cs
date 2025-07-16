namespace SP_GridTypeView
{
    public class PropertyBase
    {
        public string Title { get; set; }
        public object Value
        {
            get { return _value; }
            set { _value = value; }
        }

        private object _value;

        public PropertyBase()
        {
            Title = string.Empty;
            _value = null;
        }

        public PropertyBase(string title, object value)
        {
            Title = title;
            _value = value;
        }

        /// <summary>
        /// 텍스트 입력값을 Value에 할당합니다.
        /// </summary>
        /// <param name="text">입력 문자열</param>
        public virtual void SetValue(string text)
        {
            Value = text;
        }
    }
}