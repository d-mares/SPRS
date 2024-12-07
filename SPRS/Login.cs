using SPRS.Active_Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SPRS
{
    public partial class Login : Form
    {
        private SQLControl db;
        public Login()
        {
            InitializeComponent();
            db = new SQLControl();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Signup form = new Signup();
            form.Show();
            this.Hide();
        }

        private void roundButton1_Click(object sender, EventArgs e)
        {
            // Retrieve input from textboxes
            string username = textBox1.Text.ToLower();
            string password = textBox2.Text;

            // Check for empty inputs
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both username and password.");
                return;
            }

            if (Password_Verified(username, password))
            {
                // Query to check username and password
                string query = "SELECT USER_ID FROM users WHERE USERNAME = @user";

                // Add Parameters
                db.AddParam("@user", username);
                db.ExecQuery(query);

                if (!string.IsNullOrEmpty(db.Exception))
                {
                    MessageBox.Show($"Error: {db.Exception}");
                    return;
                }

                if (int.TryParse(db.SQLDS.Tables[0].Rows[0][0].ToString(), out int id))
                {
                    Active_User.LoggedInUserId = id;
                    Active_User.LoggedInUsername = username;
                    MessageBox.Show($"Log in successful. Welcome {username}!", "Welcome", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    MainDash main = new MainDash();
                    this.Hide();
                    main.Show();
                }
            }
            else
            {
                MessageBox.Show("Invalid username or password.");
            }
        }

        private bool Password_Verified(string username, string password)
        {
            string query = "SELECT USER_PASSWORD FROM users WHERE USERNAME = @user;";

            db.AddParam("@user", username);
            db.ExecQuery(query);

            if (!string.IsNullOrEmpty(db.Exception))
            {
                MessageBox.Show($"Error: {db.Exception}");
                return false;
            }

            if (db.SQLDS.Tables[0].Rows.Count == 1)
            {
                string stored_pass = db.SQLDS.Tables[0].Rows[0][0].ToString();
                if (PasswordHelper.VerifyPassword(password, stored_pass)) return true;
                else return false;
            }
            return false;
        }
    }
}
