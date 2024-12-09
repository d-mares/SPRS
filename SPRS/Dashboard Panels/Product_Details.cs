using SPRS.Active_Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SPRS.Dashboard_Panels
{
    public partial class Product_Details : UserControl
    {
        int prod_id;
        Product Product;
        
        public Product_Details()
        {
            InitializeComponent();
            prod_id = Active_User.Product_To_Be_Shown;
            Product = Queried_Products.GetProductById(prod_id);
            Load_Image(prod_id);
            Update_Viewed(); // we want to update the user actions table to add an entry with the product as viewed
            Product_Details_Load();
        }

        private void Product_Details_Load()
        {
            button1.Text = Product.Title;
            button2.Text = Product.Author;
            button5.Text += Product.Description;
            button6.Text = $"$ {Product.Price.ToString("n2")}";
            year.Text += Product.Year;
            button8.Text += Product.PrimaryCategory;
            button9.Text += Product.SecondaryCategory;
            button10.Text += Product.Publisher;
            button11.Text += Product.CopiesSold;
            label1.Text = prod_id.ToString();
        }

        private void Update_Viewed()
        {
            SQLControl db = new SQLControl();
            string query =
                "INSERT INTO USER_ACTIVITY(USER_ID, PRODUCT_ID, ACTIVITY_TYPE, ACTIVITY_TIME)VALUES (@user, @prod_id, 'VIEW', @time)";
            db.AddParam("@user", Active_User.LoggedInUserId);
            db.AddParam("@prod_id", prod_id);
            db.AddParam("@time", DateTime.Now);
            db.ExecQuery(query);


            if (!string.IsNullOrEmpty(db.Exception))
            {
                MessageBox.Show($"Error: {db.Exception}");
                return;
            }
        }
        private void Load_Image(int id)
        {
            string solution_directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\");
            string imagePath = Path.Combine(solution_directory, "product_images", $"{id}.jpg");
            try
            {
                pictureBox1.Image = Image.FromFile(imagePath);
            }
            catch
            {
                pictureBox1.Image = Properties.Resources.Default_Book;
            }
        }

        private void Add_To_Wishlist(object sender, EventArgs e)
        {
            SQLControl db = new SQLControl();

            // Check if the product is already wishlisted by the user
            string checkQuery = "SELECT COUNT(*) FROM WISHLISTED_ITEMS WHERE user_id = @user AND product_id = @prod_id";
            db.AddParam("@user", Active_User.LoggedInUserId);
            db.AddParam("@prod_id", prod_id);
            db.ExecQuery(checkQuery);

            if (int.Parse(db.SQLDS.Tables[0].Rows[0][0].ToString()) > 0) // If the count is greater than 0, the item is already in the wishlist
            {
                MessageBox.Show("This item is already in your wishlist!");
                return;
            }

            // If not, insert the new wishlist entry
            string insertQuery = "INSERT INTO WISHLISTED_ITEMS(user_id, product_id) VALUES (@user, @prod_id)";
            db.AddParam("@user", Active_User.LoggedInUserId);
            db.AddParam("@prod_id", prod_id);
            db.ExecQuery(insertQuery);

            if (!string.IsNullOrEmpty(db.Exception))
            {
                MessageBox.Show($"Error: {db.Exception}");
                return;
            }
            else
            {
                MessageBox.Show("Item successfully added to your wishlist!");
            }
        }
        private void Add_To_Cart(object sender, EventArgs e)
        {
            SQLControl db = new SQLControl();

            // Check if the product is already wishlisted by the user
            string checkQuery = "SELECT COUNT(*) FROM user_cart_products WHERE user_id = @user AND product_id = @prod_id";
            db.AddParam("@user", Active_User.LoggedInUserId);
            db.AddParam("@prod_id", prod_id);
            db.ExecQuery(checkQuery);

            if (int.Parse(db.SQLDS.Tables[0].Rows[0][0].ToString()) > 0) // If the count is greater than 0, the item is already in the wishlist
            {
                MessageBox.Show("This item is already in your cart!");
                return;
            }

            // If not, insert the new wishlist entry
            string insertQuery = "INSERT INTO user_cart_products(user_id, product_id, quantity) VALUES (@user, @prod_id, 1)";
            db.AddParam("@user", Active_User.LoggedInUserId);
            db.AddParam("@prod_id", prod_id);
            db.ExecQuery(insertQuery);

            if (!string.IsNullOrEmpty(db.Exception))
            {
                MessageBox.Show($"Error: {db.Exception}");
                return;
            }
            else
            {
                MessageBox.Show("Item successfully added to your cart!");
            }
        }
    }
}
