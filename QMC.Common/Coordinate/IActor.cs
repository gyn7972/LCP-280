using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common
{
    public interface IActor
    {
        int Initialize();
        int Execute(uint nID, SettingParameterCollection parameter);
        SettingParameterCollection GetParameters(uint nID);
        //Function GetFunction(uint nID);
        int GetFunctionCount();

        int StopExecute();
        string Name { set; get; }
    }
}
