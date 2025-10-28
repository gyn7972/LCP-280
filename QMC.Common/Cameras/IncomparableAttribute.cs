using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false)]
    public class IncomparableAttribute : Attribute
    {
        #region Constructor
        public IncomparableAttribute()
        {

        }
        #endregion
    }

    public interface ISupportCompareChangedObject
    {
        bool CompareEnable { get; set; }
    }
}
