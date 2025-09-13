using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.LCP_280.Process.Component.ProcessData
{

    public class Material
    {
        public Material()
        {

        }
        public string Name { get; set; } = string.Empty;

        public DateTime ArrivedTime { get; set; } = DateTime.MinValue;
        public object Tag { get; set; } = null;




    }
}
