using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common
{
    [Serializable]
    public abstract class ParamContainer
    {
        public abstract ListParam ToListParam();
        public abstract void SetParam(ListParam listParam);
    }
}
