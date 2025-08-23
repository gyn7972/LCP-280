using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common.IO
{
    public interface IDIODriver
    {
        int ReadInput(int boardNo, int portNo, int channelNo, out bool value);
        int ReadOutput(int boardNo, int portNo, int channelNo, out bool value);
        int WriteOutput(int boardNo, int portNo, int channelNo, bool value);
        int PulseOutput(int boardNo, int portNo, int channelNo, int widthMs);
    }
}
