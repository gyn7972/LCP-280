using QMC.Common.PKGTester;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common.Account
{
    public enum UserGrade
    {
        None = -1,
        Operator,
        Maintenance,
        Supervisor,
    }

    //public static class UserGradeHelper
    //{
    //    public static string ToString(this UserGrade grade)
    //    {
    //        switch (grade)
    //        {
    //            case UserGrade.None:
    //                return "None";
    //            case UserGrade.Operator:
    //                return "Operator";
    //            case UserGrade.Maintenance:
    //                return "Maintenance";
    //            case UserGrade.Supervisor:
    //                return "Supervisor";
    //            default:
    //                return "Unknown";
    //        }
    //    }
    //}

    //public static class TestItemHelper
    //{
    //    public static TestItemCategory GetCategory(this TestItemType type)
    //    {
    //        var typeInfo = type.GetType();
    //        var memInfo = typeInfo.GetMember(type.ToString());
    //        var attributes = memInfo[0].GetCustomAttributes(typeof(CategoryAttribute), false);
    //        if (attributes.Length > 0)
    //        {
    //            var category = ((CategoryAttribute)attributes[0]).Category;
    //            if (Enum.TryParse(category, out TestItemCategory result))
    //            {
    //                return result;
    //            }
    //        }
    //        return TestItemCategory.Undefined;
    //    }
    //}
}
