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
    public partial class Cart_Item : UserControl
    {
        int prod_id;
        public event EventHandler<string> ReplacePanelRequested;
        public event EventHandler RemoveItem;
        Product product;
        public Cart_Item(int id)
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
            button8.Text = $"{product.PrimaryCategory}, {product.SecondaryCategory}";
        }

        private void Send_To_Product(object sender, EventArgs e)
        {
            Active_User.Product_To_Be_Shown = prod_id;

            ReplacePanelRequested?.Invoke(this, "DETAILS");
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

        private void Remove_Cart(object sender, EventArgs e)
        {
            SQLControl db = new SQLControl();

            // If not, insert the new wishlist entry
            string query = "DELETE FROM USER_CART_PRODUCTS WHERE USER_ID = @user and PRODUCT_ID = @prod_id";
            db.AddParam("@user", Active_User.LoggedInUserId);
            db.AddParam("@prod_id", prod_id);
            db.ExecQuery(query);

            if (!string.IsNullOrEmpty(db.Exception))
            {
                MessageBox.Show($"Error: {db.Exception}");
                return;
            }
            else
            {
                MessageBox.Show("Item removed from cart!");
                RemoveItem?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
