using SPRS.Active_Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SPRS.Custom_Controls
{
    public partial class Single_Carousel : UserControl
    {
        List<int> products;
        List<ProductCard> cards = new List<ProductCard>();
        public event EventHandler<string> PanelChangeRequest;
        int count = 0;
        public Single_Carousel(List<int> prods)
        {
            InitializeComponent();
            products = prods;
            Populate_Carousel();
            
        }

        private void Populate_Carousel() 
        {
            pictureBox1.Visible = false;
            foreach (int id in products) 
            {
                ProductCard productCard = new ProductCard(id);
                productCard.Dock = DockStyle.Fill;
                productCard.ReplacePanelRequested += HandlePanelChangeRequest;

                cards.Add(productCard);
            }
            panel2.Controls.Add(cards[0]);

        }

        private void Change_Item_Left(object sender, EventArgs e)
        {
            count -= 1;
            panel2.Controls.Clear();
            panel2.Controls.Add(cards[count]);
            if (count == 0) pictureBox1.Visible = false;
            pictureBox4.Visible = true;
        }
        private void Change_Card_Right(object sender, EventArgs e)
        {
            count += 1;
            panel2.Controls.Clear();
            panel2.Controls.Add(cards[count]);
            if (count == cards.Count - 1) pictureBox4.Visible = false;
            pictureBox1.Visible = true;
        }

        private void HandlePanelChangeRequest(object sender, string tag)
        {
            PanelChangeRequest?.Invoke(sender, tag);
        }

        private void Add_Bookmark(object sender, EventArgs e)
        {
            SQLControl db = new SQLControl();

            // Check if the product is already wishlisted by the user
            string checkQuery = "SELECT COUNT(*) FROM WISHLISTED_ITEMS WHERE user_id = @user AND product_id = @prod_id";
            db.AddParam("@user", Active_User.LoggedInUserId);
            db.AddParam("@prod_id", cards[count].prod_id);
            db.ExecQuery(checkQuery);

            if (int.Parse(db.SQLDS.Tables[0].Rows[0][0].ToString()) > 0) // If the count is greater than 0, the item is already in the wishlist
            {
                MessageBox.Show("This item is already in your wishlist!");
                return;
            }

            // If not, insert the new wishlist entry
            string insertQuery = "INSERT INTO WISHLISTED_ITEMS(user_id, product_id) VALUES (@user, @prod_id)";
            db.AddParam("@user", Active_User.LoggedInUserId);
            db.AddParam("@prod_id", cards[count].prod_id);
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

        private void Add_Cart(object sender, EventArgs e)
        {
            SQLControl db = new SQLControl();

            // Check if the product is already wishlisted by the user
            string checkQuery = "SELECT COUNT(*) FROM user_cart_products WHERE user_id = @user AND product_id = @prod_id";
            db.AddParam("@user", Active_User.LoggedInUserId);
            db.AddParam("@prod_id", cards[count].prod_id);
            db.ExecQuery(checkQuery);

            if (int.Parse(db.SQLDS.Tables[0].Rows[0][0].ToString()) > 0) // If the count is greater than 0, the item is already in the wishlist
            {
                MessageBox.Show("This item is already in your cart!");
                return;
            }

            // If not, insert the new wishlist entry
            string insertQuery = "INSERT INTO user_cart_products(user_id, product_id, quantity) VALUES (@user, @prod_id, 1)";
            db.AddParam("@user", Active_User.LoggedInUserId);
            db.AddParam("@prod_id", cards[count].prod_id);
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
