using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common.VisionPart
{
    [Serializable]
    public class AlignerParameter
    {
        public XytCoordinate CenterCoordinate { set; get; }

        public AlignerParameter()
        {
            CenterCoordinate = new XytCoordinate{ };
        }
    }

    public class AlignerParameterCollection : Collection<AlignerParameter>
    {
        
    }
}
