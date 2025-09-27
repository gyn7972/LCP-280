using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common.Account
{
    public static class AccountManager
    {
        #region Fields
        private static List<Account> accounts = new List<Account>();
        #endregion

        #region Supervisor Account
        public static Account SupervisorAccount = new Account("QMC", UserGrade.Supervisor, "QMC", "QMC0710!");
        #endregion

        #region Account Management
        public static void Clear()
        {
            accounts.Clear();
        }
        public static bool AddAccount(Account account)
        {
            if (account == null || !account.Validate())
                return false;
            if (accounts.Any(a => a.Name == account.Name))
                return false;
            accounts.Add(account);
            return true;
        }
        public static bool RemoveAccount(string name)
        {
            var account = accounts.FirstOrDefault(a => a.Name == name);
            if (account != null)
            {
                accounts.Remove(account);
                return true;
            }
            return false;
        }
        #endregion

        #region Login
        public static Account FindAccount(string name, string id, string pw)
        {
            // 입력한 정보가 Supervisor 계정과 일치하면 SupervisorAccount 반환
            if (name == SupervisorAccount.Name && id == SupervisorAccount.UserID && pw == SupervisorAccount.Password)
            {
                return SupervisorAccount;
            }

            // 그렇지 않으면 일반 계정 목록에서 검색
            return accounts.FirstOrDefault(a => a.Name == name && a.UserID == id && a.Password == pw);
        }
        #endregion

        #region File
        public static int LoadFromFile(string filePath)
        {
            Clear();
            if (!System.IO.File.Exists(filePath))
                return -1;

            try
            {
                int count = 0;
                foreach (var line in System.IO.File.ReadLines(filePath))
                {
                    var account = new Account("");
                    if (account.Parse(line))
                    {
                        if (AddAccount(account))
                            count++;
                    }
                }
                return 0;
            }
            catch (Exception)
            {
                return -1;
            }
        }
        public static int SaveToFile(string filePath)
        {
            try
            {
                var sb = new StringBuilder();
                foreach (var account in accounts)
                {
                    sb.AppendLine(account.ToString());
                }
                System.IO.File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
                return 0;
            }
            catch (Exception)
            {
                return -1;
            }
        }
        #endregion
    }
}
