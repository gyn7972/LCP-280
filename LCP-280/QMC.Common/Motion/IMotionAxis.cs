using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common.Motion
{
    public interface IMotionAxis
    {
        string Name { get; set; }
        string Unit { get; set; }
    }
}
