using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common.Account
{
    /// <summary>
    /// 계정을 관리하고 로그인/로그아웃 기능을 제공하는 클래스입니다.
    /// </summary>
    public static class AccountManager
    {
        #region Fields
        private static List<Account> accounts = new List<Account>();
        private static Account currentAcount = GuestAccount;
        #endregion

        #region Defined Account
        public static Account GuestAccount = new Account(UserGrade.None, "Guest", ""); // Logout 계정
        public static Account SupervisorAccount = new Account(UserGrade.Supervisor, "QMC", "QMC0710!"); // Supervisor 계정 (QMC Master)
        #endregion

        #region Event
        public static EventHandler OnLoginStateChanged;
        #endregion

        #region Properties
        public static Account CurrentAccount { get => currentAcount; private set => currentAcount = value; }
        public static IReadOnlyList<Account> Accounts { get => accounts.AsReadOnly(); }
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
            if (accounts.Any(a => a.UserID == account.UserID))
                return false;

            accounts.Add(account);
            return true;
        }
        public static bool RemoveAccount(string id)
        {
            var account = accounts.FirstOrDefault(a => a.UserID == id);
            if (account != null)
            {
                accounts.Remove(account);
                return true;
            }
            return false;
        }
        public static bool IsExistAccount(string id)
        {
            return accounts.Any(a => a.UserID == id);
        }
        public static Account GetAccount(string id)
        {
            return accounts.FirstOrDefault(a => a.UserID == id);
        }
        #endregion

        #region Login / Logout
        public static bool Login(string id, string pw)
        {
            // 입력한 정보가 Supervisor 계정과 일치하면 SupervisorAccount 반환
            if (id == SupervisorAccount.UserID && pw == SupervisorAccount.Password)
            {
                currentAcount = SupervisorAccount;
                OnLoginStateChanged?.Invoke(null, EventArgs.Empty);
                return true;
            }

            // 그렇지 않으면 일반 계정 목록에서 검색
            Account loginAccount = accounts.FirstOrDefault(a => a.UserID == id && a.Password == pw);
            if (loginAccount != null)
            {
                currentAcount = loginAccount;
                OnLoginStateChanged?.Invoke(null, EventArgs.Empty);
                return true;
            }

            // 일치하는 계정이 없음
            return false;
        }
        public static void Logout()
        {
            currentAcount = GuestAccount;
            OnLoginStateChanged?.Invoke(null, EventArgs.Empty);
        }
        public static bool IsLoggedIn()
        {
            return currentAcount != null && currentAcount.Grade != UserGrade.None;
        }
        #endregion

        #region File
        public static int Load()
        {
            string filePath = GetFilePath();
            return LoadFromFile(filePath);
        }
        public static int Save()
        {
            string filePath = GetFilePath();
            return SaveToFile(filePath);
        }
        private static string GetFilePath()
        {
            var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "Account");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            return Path.Combine(dir, "Account.dat");
        }
        private static int LoadFromFile(string filePath)
        {
            Clear();
            if (!System.IO.File.Exists(filePath))
                return -1;

            try
            {
                int count = 0;
                foreach (var line in System.IO.File.ReadLines(filePath))
                {
                    var account = new Account();
                    if (account.DecryptAndParse(line))
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
        private static int SaveToFile(string filePath)
        {
            try
            {
                var sb = new StringBuilder();
                foreach (var account in accounts)
                {
                    sb.AppendLine(account.ToEncrytString());
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