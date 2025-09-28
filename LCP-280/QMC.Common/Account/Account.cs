using QMC.Common.Keithley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static QMC.Common.Keithley.KeithelySourcemeterConfig;

namespace QMC.Common.Account
{
    public class Account
    {
        #region Properties
        public UserGrade Grade { get; set; }
        public string UserID { get; set; }
        public string Password { get; set; }
        #endregion

        #region Constructor
        public Account()
        {
            Reset();
        }
        public Account(UserGrade grade, string id, string pw)
        {
            Grade = grade;
            UserID = id;
            Password = pw;
        }
        #endregion

        #region Method
        public void Reset()
        {
            Grade = UserGrade.None;
            UserID = "";
            Password = "";
        }
        public bool Validate()
        {
            if (string.IsNullOrWhiteSpace(UserID))
                return false;
            if (UserID == AccountManager.GuestAccount.UserID || UserID == AccountManager.SupervisorAccount.UserID || UserID.Contains(","))
                return false;

            if (Grade != UserGrade.None)
            {       
                if (string.IsNullOrWhiteSpace(Password) || Password.Contains(","))
                    return false;
            }
            return true;
        }
        public string ToEncrytString()
        {
            string plainText = $"{Grade.ToString()},{UserID},{Password}";
            string encryptedText = Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText));

            return encryptedText;
        }
        public bool DecryptAndParse(string str)
        {
            // Check argument
            if (string.IsNullOrWhiteSpace(str))
                return false;

            // Decrypt
            string decryptedText = Encoding.UTF8.GetString(Convert.FromBase64String(str));

            // Parse
            var tokens = decryptedText.Split(',');
            if (tokens.Length != 3)
                return false;

            if (!Enum.TryParse(tokens[0], out UserGrade grade))
                return false;
            Grade = grade;
            UserID = tokens[1];
            Password = tokens[2];
            return Validate();
        }
        #endregion
    }
}
