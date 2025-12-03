using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QMC.Common.Account
{
    public partial class FormLogin : Form
    {
        private Account selectAccount = null;
        private bool initComplete = false;

        public FormLogin()
        {
            InitializeComponent();

            cbEditGrade.Items.Clear();
            cbEditGrade.Items.Add(UserGrade.Operator.ToString());
            cbEditGrade.Items.Add(UserGrade.Maintenance.ToString());
        }

        private void FormLogin_Load(object sender, EventArgs e)
        {
            // initialize
            if (!initComplete)
            {
                AccountManager.Load();
                initComplete = true;
            }

            // Login
            UpdateLoginPage();

            // Account
            UpdateAccountPage();
        }

        private void ClearAccountList()
        {
            dgvAccount.Rows.Clear();
        }

        private void UpdateAccountList()
        {
            dgvAccount.Rows.Clear();
            foreach (var account in AccountManager.Accounts)
            {
                dgvAccount.Rows.Add(account.UserID, account.Grade.ToString());
            }
        }

        private void UpdateLoginPage()
        {
            if (AccountManager.IsLoggedIn())
            {
                tbUserID.Text = AccountManager.CurrentAccount?.UserID;
                tbPassword.Text = AccountManager.CurrentAccount?.Password;
                tbUserID.Enabled = false;
                tbPassword.Enabled = false;
                btnLogin.Text = "Logout";
                lbError.Text = "";
            }
            else
            {
                tbUserID.Text = "";
                tbPassword.Text = "";
                tbUserID.Enabled = true;
                tbPassword.Enabled = true;
                btnLogin.Text = "Login";
            }
        }

        private void UpdateAccountPage()
        {
            if (AccountManager.IsLoggedIn() && AccountManager.CurrentAccount.Grade == UserGrade.Supervisor)
            {
                UpdateAccountList();
                dgvAccount.Enabled = true;

                tbEditID.Enabled = true;
                tbEditPassword.Enabled = true;
                cbEditGrade.Enabled = true;

                btnApply.Enabled = true;
                btnDelete.Enabled = true;
            }
            else
            {
                ClearAccountList();
                dgvAccount.Enabled = false;

                tbEditID.Enabled = false;
                tbEditPassword.Enabled = false;
                cbEditGrade.Enabled = false;

                btnApply.Enabled = false;
                btnDelete.Enabled = false;
            }
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (AccountManager.IsLoggedIn())
            {
                // Logout
                AccountManager.Logout();
                lbError.Text = "Please enter your User ID and Password.";
            }
            else
            {
                // Login
                string id = tbUserID.Text.Trim();
                string pw = tbPassword.Text;

                if (!AccountManager.Login(id, pw))
                {
                    lbError.Text = "Invalid User ID or Password.";
                } else
                {
                    this.Close();
                }
            }

            UpdateLoginPage();
            UpdateAccountPage();
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            string id = "";
            string pw = "";
            UserGrade grade = UserGrade.None;

            try
            {
                id = tbEditID.Text.Trim();
                pw = tbEditPassword.Text;
                grade = cbEditGrade.SelectedItem != null ? (UserGrade)Enum.Parse(typeof(UserGrade), cbEditGrade.SelectedItem.ToString()) : UserGrade.None;
            }
            catch (Exception)
            {
                var mb = new MessageBoxOk();
                mb.ShowDialog("Error!", "Invalid account data. Please check the input values.");

                return;
            }

            Account tempAccount = new Account(grade, id, pw);   
            if (tempAccount.Validate())
            {
                if (AccountManager.IsExistAccount(tempAccount.UserID))
                {
                    // Update existing account
                    var existingAccount = AccountManager.Accounts.First(a => a.UserID == tempAccount.UserID);
                    existingAccount.Grade = tempAccount.Grade;
                    existingAccount.Password = tempAccount.Password;

                    AccountManager.Save();
                    UpdateAccountList();

                    var mb = new MessageBoxOk();
                    mb.ShowDialog("Info!", $"Account '{tempAccount.UserID}' has been updated.");
                }
                else
                {
                    // Add new account
                    if (!AccountManager.AddAccount(new Account(tempAccount.Grade, tempAccount.UserID, tempAccount.Password)))
                    {
                        var mb2 = new MessageBoxOk();
                        mb2.ShowDialog("Error!", $"Failed to add account '{tempAccount.UserID}'. It may already exist.");

                        return;
                    }

                    AccountManager.Save();
                    UpdateAccountList();

                    var mb3 = new MessageBoxOk();
                    mb3.ShowDialog("Info!", $"Account '{tempAccount.UserID}' has been added.");
                }  
            }
            else
            {
                var mb = new MessageBoxOk();
                mb.ShowDialog("Error!", "Invalid account data. Please check the input values.");
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (selectAccount != null)
            {
                var ask = new MessageBoxYesNo();
                if (ask.ShowDialog("Confirm Delete", "Are you sure you want to delete the account '{selectAccount.UserID}'?") == DialogResult.Yes)
                {
                    AccountManager.RemoveAccount(selectAccount.UserID);
                    AccountManager.Save();
                    UpdateAccountList();
                    selectAccount = null;
                }
            }
        }

        private void dgvAccount_SelectionChanged(object sender, EventArgs e)
        {
            selectAccount = null;

            int selectIndex = dgvAccount.CurrentCell.RowIndex;
            if (selectIndex >= 0 && selectIndex < AccountManager.Accounts.Count)
            {
                var cellValue = dgvAccount.Rows[selectIndex].Cells[0].Value;
                if (cellValue != null)
                {
                    string userId = cellValue.ToString();
                    if (!string.IsNullOrEmpty(userId))
                    {
                        selectAccount = AccountManager.Accounts.FirstOrDefault(a => a.UserID == userId);
                    }
                }
            }

            if (selectAccount != null)
            {
                tbEditID.Text = selectAccount.UserID;
                tbEditPassword.Text = selectAccount.Password;
                cbEditGrade.SelectedItem = selectAccount.Grade.ToString();
            }
            else
            {
                tbEditID.Text = "";
                tbEditPassword.Text = "";
                cbEditGrade.SelectedItem = null;
            }
        }
    }
}
