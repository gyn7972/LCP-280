using QMC.Common.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common.History
{
    public class BaseHistory
    {
        #region Field
        private DateTime time = DateTime.Now;
        private string userID = AccountManager.CurrentAccount?.UserID;
        #endregion

        #region Property
        public DateTime Time => time;
        public string UserID => userID;
        #endregion

        #region Constructor
        public BaseHistory()
        {
        }
        #endregion
    }
}
