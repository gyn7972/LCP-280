using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common.Account
{
    public class Account
    {
        #region Properties
        public string Name { get; set; }
        public UserGrade Grade { get; set; }
        public string UserID { get; set; }
        public string Password { get; set; }
        #endregion

        #region Constructor
        public Account(string name)
        {
            Reset();
        }
        public Account(string name, UserGrade grade, string id, string pw)
        {
            Name = name;
            Grade = grade;
            UserID = id;
            Password = pw;
        }
        #endregion

        #region Method
        public void Reset()
        {
            Name = "Unknown";
            Grade = UserGrade.None;
            UserID = "";
            Password = "";
        }
        public bool Validate()
        {
            if (string.IsNullOrWhiteSpace(Name) || Name == "Unknown" || Name == "QMC" || Name.Contains(","))
                return false;

            if (Grade != UserGrade.None)
            {
                if (string.IsNullOrWhiteSpace(UserID) || UserID.Contains(",")) 
                    return false;
                if (string.IsNullOrWhiteSpace(Password) || Password.Contains(","))
                    return false;
            }
            return true;
        }
        public new string ToString()
        {
            return $"{Name},{Grade.ToString()},{UserID},{Password}";
        }
        public bool Parse(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return false;

            var tokens = str.Split(',');
            if (tokens.Length != 4)
                return false;

            Name = tokens[0];
            if (!Enum.TryParse(tokens[1], out UserGrade grade))
                return false;
            Grade = grade;
            UserID = tokens[2];
            Password = tokens[3];
            return Validate();
        }
        #endregion
    }

    
}
