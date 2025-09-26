using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common.PKGTester
{
    public enum TestItemCategory
    {
        Electrical,
        Optical,
        UserDefined,
        Undefined
    };

    public enum TestItemType
    {
        [Category("Undefined")] None,

        [Category("Electrical")] VF,
        [Category("Electrical")] VR,
        [Category("Electrical")] IF,
        [Category("Electrical")] IR,

        [Category("Optical")] RadInt,
        [Category("Optical")] PhotInt,
        [Category("Optical")] WP,
        [Category("Optical")] FWHM,
        [Category("Optical")] CIEX,
        [Category("Optical")] CIEY,
        [Category("Optical")] CIEZ,
        [Category("Optical")] CIEU,
        [Category("Optical")] CIEV1976,
        [Category("Optical")] CIEV1960,
        [Category("Optical")] LambdaDom,
        [Category("Optical")] Purity,
        [Category("Optical")] CCT,
        [Category("Optical")] CRI,
        [Category("Optical")] Centroid,
        [Category("Optical")] StimulusX,
        [Category("Optical")] StimulusY,
        [Category("Optical")] StimulusZ,
        [Category("Optical")] PickValue,
        [Category("Optical")] ADC,

        [Category("UserDefined")] UserDefine,

        //[Category("Undefined")] KELFS,
        //[Category("Undefined")] KELDG,
        //[Category("Undefined")] TOV,
    };

    public static class TestItemHelper
    {
        public static TestItemCategory GetCategory(this TestItemType type)
        {
            var typeInfo = type.GetType();
            var memInfo = typeInfo.GetMember(type.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(CategoryAttribute), false);
            if (attributes.Length > 0)
            {
                var category = ((CategoryAttribute)attributes[0]).Category;
                if (Enum.TryParse(category, out TestItemCategory result))
                {
                    return result;
                }
            }
            return TestItemCategory.Undefined;
        }
    }
}