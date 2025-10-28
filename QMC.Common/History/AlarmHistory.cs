using QMC.Common.Alarm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common.History
{
    public class AlarmHistory : BaseHistory
    {
        #region Field
        private AlarmInfo info;
        #endregion

        #region Property
        public AlarmInfo Info => info;
        #endregion

        #region Constructor
        public AlarmHistory(AlarmInfo info)
        {
            this.info = info;
        }
        #endregion
    }
}