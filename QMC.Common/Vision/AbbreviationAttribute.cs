using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common.Vision
{
    [Serializable]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Class, AllowMultiple = false)]
    public class AbbreviationAttribute : Attribute
    {
        #region Field
        private string m_Abbreviation;
        #endregion

        #region Constructor
        public AbbreviationAttribute(string abbreviation)
        {
            this.Abbreviation = abbreviation;
        }
        public AbbreviationAttribute() : this("") { }
        #endregion

        #region Property
        public string Abbreviation
        {
            get { return this.m_Abbreviation; }
            set { this.m_Abbreviation = value; }
        }
        #endregion

        #region Method
        public static string GetAbbreviation(Enum value)
        {
            string abbreviation = "";
            AbbreviationAttribute attribute = null;
            MemberInfo[] members = null;

            //value가 null일 경우 abrreviation을 빈값으로 반환
            if (value == null) return abbreviation;

            members = value.GetType().GetMember(value.ToString());

            if (0 < members.Length)
            {
                attribute = members[0].GetCustomAttribute<AbbreviationAttribute>();
                if (attribute != null)
                    abbreviation = attribute.Abbreviation;
                else
                    abbreviation = value.ToString();
            }
            else
                abbreviation = value.ToString();

            return abbreviation;
        }

        public static string GetAbbreviation(Type value)
        {
            string abbreviation = "";
            AbbreviationAttribute attribute = null;

            attribute = value.GetCustomAttribute<AbbreviationAttribute>();

            if (attribute != null)
                abbreviation = attribute.Abbreviation;
            else
                abbreviation = value.ToString();

            return abbreviation;
        }
        #endregion
    }
}
