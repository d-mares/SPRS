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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace SPRS
{
    public partial class Home_Panel : BasePanel
    {
        SQLControl db;
        public Home_Panel()
        {
            InitializeComponent();
            db = new SQLControl();
        }

        private void Panel_Load(object sender, EventArgs e)
        {
            // updating the blank panel on load

            Wishlist_Count.Text = Get_Count('w');
            if (!string.IsNullOrEmpty(Get_Count('c')))
            {
                Cart_Count.Text = Get_Count('c');
            }
            else Cart_Count.Text = "0";
        }

        private string Get_Count(char c)
        {
            string query;
            switch (c)
            {
                case 'w':
                    query = "select count(*) from wishlisted_items where user_id = @user";
                    break;
                case 'R':
                    query = "select count(*) from product";
                    break;
                case 'c':
                default:
                    query = "select sum(quantity) from USER_CART_PRODUCTS where user_id = @user";
                    break;
            }

            // Add Parameters
            db.AddParam("@user", Active_User.LoggedInUserId);
            db.ExecQuery(query);

            if (!string.IsNullOrEmpty(db.Exception))
            {
                MessageBox.Show($"Error: {db.Exception}");
                return "";
            }

            else return db.SQLDS.Tables[0].Rows[0][0].ToString();
        }



        private void Section_Clicked(object sender, EventArgs e)
        {
            // Check if the sender has a Tag property and retrieve its value
            if (sender is Control control && control.Tag != null)
            {
                // Trigger the event with the retrieved tag
                RequestPanelChange(control.Tag.ToString());
            }
        }

        private void Mystery_Choice(object sender, EventArgs e)
        {
            int max = int.Parse(Get_Count('R'));

            Random random = new Random();
            Active_User.Product_To_Be_Shown = random.Next(max+1);


            // Check if the sender has a Tag property and retrieve its value
            if (sender is Control control && control.Tag != null)
            {
                // Trigger the event with the retrieved tag
                RequestPanelChange(control.Tag.ToString());
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {

        }
    }
}
