using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common.VisionPart
{
    public class AlignerResult
    {
        private XytCoordinateCollection m_SearchPositions;

        public XytCoordinateCollection SearchPositions
        {
            get { return m_SearchPositions; }
            set { m_SearchPositions = value; }
        }
        public AlignerResult()
        {
            m_SearchPositions = new XytCoordinateCollection();
        }
    }
}
