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
    
    public partial class Signup : Form
    {
        SQLControl db;
        public Signup()
        {
            InitializeComponent();
            db = new SQLControl();
        }

        private void Back(object sender, EventArgs e)
        {
            Login login = new Login();
            this.Hide();
            login.Show();
        }

        private void Register(object sender, EventArgs e)
        {
            // Retrieve input from textboxes
            string username = textBox1.Text.ToLower();
            string password = textBox2.Text;
            string confirmPassword = textBox3.Text;

            // Check for empty inputs
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                MessageBox.Show("Please fill out all fields.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Check if passwords match
            if (password != confirmPassword)
            {
                MessageBox.Show("Passwords do not match. Please try again.", "Password Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Check if username is already taken
            string checkQuery = "SELECT COUNT(*) FROM users WHERE USERNAME = @user;";
            db.AddParam("@user", username);
            db.ExecQuery(checkQuery);

            if (!string.IsNullOrEmpty(db.Exception))
            {
                MessageBox.Show($"Error: {db.Exception}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int userCount = Convert.ToInt32(db.SQLDS.Tables[0].Rows[0][0]);
            if (userCount > 0)
            {
                MessageBox.Show("Username is already taken. Please choose a different one.", "Registration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Hash the password
            string hashedPassword = PasswordHelper.HashPassword(password);

            // Insert new user into the database
            string insertQuery = "INSERT INTO users (USERNAME, USER_PASSWORD) VALUES (@user, @password);";
            db.AddParam("@user", username);
            db.AddParam("@password", hashedPassword);
            db.ExecQuery(insertQuery);

            if (!string.IsNullOrEmpty(db.Exception))
            {
                MessageBox.Show($"Error: {db.Exception}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Notify the user of successful registration
            MessageBox.Show("Registration successful! You can now log in.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Optionally redirect to the login page
            Login loginForm = new Login();
            this.Hide();
            loginForm.Show();

        }
    }
}
