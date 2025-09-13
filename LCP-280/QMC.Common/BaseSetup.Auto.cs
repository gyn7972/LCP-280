using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common
{
    public abstract partial class BaseSetup
    {
        public virtual ConfigReflectionMapper CreateMapper()
        {
            return new ConfigReflectionMapper(this);
        }
    }
}
