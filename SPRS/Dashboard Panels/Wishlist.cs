using SPRS.Active_Classes;
using SPRS.Custom_Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SPRS.Dashboard_Panels
{
    public partial class Wishlist : BasePanel
    {
        public Wishlist()
        {
            InitializeComponent();
            Generate_Wishlist_Items();
        }

        private void Generate_Wishlist_Items()
        {
            SQLControl db = new SQLControl();

            string query =
                "SELECT PRODUCT_ID FROM WISHLISTED_ITEMS WHERE USER_ID = @user";

            db.AddParam("@user", Active_User.LoggedInUserId);
            db.ExecQuery(query);

            if (!string.IsNullOrEmpty(db.Exception))
            {
                MessageBox.Show($"Error: {db.Exception}");
                return;
            }
            List<int> prods = new List<int>();

            if(db.SQLDS != null && db.SQLDS.Tables.Count > 0)
            {
                foreach (DataRow row in db.SQLDS.Tables[0].Rows)
                {
                    
                    if (int.TryParse(row["PRODUCT_ID"].ToString(), out int productId))
                    {
                        // Create a new instance of Wishlist_Item with the product ID
                        Wishlist_Item wishlistItem = new Wishlist_Item(productId);
                        wishlistItem.ReplacePanelRequested += HandlePanelChangeRequest;

                        // Add the Wishlist_Item to the flowLayoutPanel
                        flowLayoutPanel1.Controls.Add(wishlistItem);
                    }
                }
            }
            else
            {
                MessageBox.Show("No items found in the wishlist.");
            }
        }
        private void HandlePanelChangeRequest(object sender, string tag)
        {
            RequestPanelChange(tag);
        }
    }
}
