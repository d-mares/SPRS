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

namespace SPRS.Custom_Controls
{
    public partial class Product_List_Item : UserControl
    {
        int prod_id;
        Product product;
        
        public Product_List_Item()
        {
            InitializeComponent();
        }

        public event EventHandler<string> ReplacePanelRequested;

        public Product_List_Item(int id)
        {
            InitializeComponent();
            prod_id = id;
            product = Queried_Products.GetProductById(prod_id);
            Load_Image(prod_id);
            Product_Details_Load();

        }
        private void Product_Details_Load()
        {
            button1.Text = $"{product.Title} \r\n {product.Author} ";
            button2.Text = $"$ {product.Price.ToString("n2")}";
            button3.Text = $"{product.Publisher}";
            button8.Text = $"{product.PrimaryCategory}, {product.SecondaryCategory}" ;
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

        private void Send_To_Product(object sender, EventArgs e)
        {
            Active_User.Product_To_Be_Shown = prod_id;

            ReplacePanelRequested?.Invoke(this, "DETAILS");
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

        private void Add_To_Bookmark(object sender, EventArgs e)
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
    }
}
