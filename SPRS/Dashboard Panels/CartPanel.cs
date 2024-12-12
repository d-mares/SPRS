using MySql.Data.MySqlClient;
using Org.BouncyCastle.Utilities;
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
    public partial class CartPanel : BasePanel
    {
        public CartPanel()
        {
            InitializeComponent();
            Generate_Cart_Items();
            UpdateLabels();
        }
        private void Generate_Cart_Items()
        {
            SQLControl db = new SQLControl();

            string query =
                "SELECT PRODUCT_ID FROM USER_CART_PRODUCTS WHERE USER_ID = @user";

            db.AddParam("@user", Active_User.LoggedInUserId);
            db.ExecQuery(query);

            if (!string.IsNullOrEmpty(db.Exception))
            {
                MessageBox.Show($"Error: {db.Exception}");
                return;
            }
            List<int> prods = new List<int>();

            if (db.SQLDS != null && db.SQLDS.Tables.Count > 0)
            {
                foreach (DataRow row in db.SQLDS.Tables[0].Rows)
                {

                    if (int.TryParse(row["PRODUCT_ID"].ToString(), out int productId))
                    {
                        // Create a new instance of cart item with the product ID
                        Cart_Item cart_item = new Cart_Item(productId);
                        cart_item.ReplacePanelRequested += HandlePanelChangeRequest;
                        cart_item.RemoveItem += HandleRemoveItem;

                        // Add the Wishlist_Item to the flowLayoutPanel
                        flowLayoutPanel1.Controls.Add(cart_item);
                    }
                }
            }
            else
            {
                MessageBox.Show("No items found in the wishlist.");
            }
        }

        private void HandleRemoveItem(object sender, EventArgs e)
        {
            if (sender is Cart_Item removableControl)
            {
                // Unsubscribe from the event
                    removableControl.RemoveItem -= HandleRemoveItem;

                // Remove the control from its parent container
                removableControl.Parent?.Controls.Remove(removableControl);

                // Dispose of the control to release resources
                removableControl.Dispose();
                UpdateLabels();
            }
        }

        private void HandlePanelChangeRequest(object sender, string tag)
        {
            RequestPanelChange(tag);
        }

        private void Purchase_clicked(object sender, EventArgs e)
        {
            // Get the total cost and the cart items for the logged-in user
            var (totalCost, cartItems) = GetCartTotalAndItems(Active_User.LoggedInUserId);

            if (totalCost == 0 || cartItems == null || cartItems.Count == 0)
            {
                MessageBox.Show("Your cart is empty.");
                return;
            }

            // Insert the order into the ORDERS table
            int orderId = InsertOrder(Active_User.LoggedInUserId, totalCost);

            if (orderId == -1)
            {
                MessageBox.Show("Error placing the order.");
                return;
            }

            InsertOrderProductsAndClearCart(orderId, cartItems, Active_User.LoggedInUserId);

            MessageBox.Show("Purchase completed successfully.", "Order Placed", MessageBoxButtons.OK, MessageBoxIcon.Information);
            RequestPanelChange("ORDER_HISTORY"); 
        }

        private void UpdateLabels()
        {
            SQLControl db = new SQLControl();

            // Add parameter for user ID
            db.AddParam("@userId", Active_User.LoggedInUserId);

            // Execute the query
            string query = @"
                SELECT 
                    SUM(ucp.QUANTITY * p.PRICE) AS TotalPrice,
                    SUM(ucp.QUANTITY) AS TotalBooks
                FROM USER_CART_PRODUCTS ucp
                JOIN PRODUCT p ON ucp.PRODUCT_ID = p.PRODUCT_ID
                WHERE ucp.USER_ID = @userId;";

            db.ExecQuery(query);

            // Check for exceptions
            if (!string.IsNullOrEmpty(db.Exception))
            {
                throw new Exception($"Database error: {db.Exception}");
            }

            // Extract results
            if (db.SQLDS.Tables.Count > 0 && db.SQLDS.Tables[0].Rows.Count > 0)
            {
                DataRow row = db.SQLDS.Tables[0].Rows[0];
                decimal totalPrice = row["TotalPrice"] != DBNull.Value ? Convert.ToDecimal(row["TotalPrice"]) : 0;
                int totalBooks = row["TotalBooks"] != DBNull.Value ? Convert.ToInt32(row["TotalBooks"]) : 0;
                button2.Text = $"Total for {totalBooks} books:";
                button1.Text = $"$ {totalPrice.ToString("n2")}";
            }
        }

        private (decimal totalCost, List<(int productId, int quantity, decimal price)> cartItems) GetCartTotalAndItems(int userId)
        {
            SQLControl db = new SQLControl();
            db.AddParam("@user_id", userId);

            // Create a list to store cart items (productId, quantity, price)
            var cartItems = new List<(int productId, int quantity, decimal price)>();

            // Query to get the total cost and the cart items
            string query = @"
                SELECT p.PRODUCT_ID, ucp.QUANTITY, p.PRICE 
                FROM USER_CART_PRODUCTS ucp
                JOIN PRODUCT p ON ucp.PRODUCT_ID = p.PRODUCT_ID
                WHERE ucp.USER_ID = @user_id;";

            db.ExecQuery(query);

            if (!string.IsNullOrEmpty(db.Exception))
            {
                MessageBox.Show($"Error: {db.Exception}", "get cart total");
                return (0, null);
            }

            decimal totalCost = 0;

            foreach (DataRow row in db.SQLDS.Tables[0].Rows)
            {
                int productId = (int)row["PRODUCT_ID"];
                int quantity = (int)row["QUANTITY"];
                decimal price = (decimal)row["PRICE"];

                totalCost += price * quantity;

                // Add the item to the list
                cartItems.Add((productId, quantity, price));
            }
            return (totalCost, cartItems);
        }

        private int InsertOrder(int userId, decimal totalCost)
        {
            SQLControl db = new SQLControl();
            db.AddParam("@user_id", userId);
            db.AddParam("@total_cost", totalCost);

            // Query to insert into the ORDERS table
            string query = "INSERT INTO ORDERS (USER_ID, ORDER_TIME, TOTAL) VALUES (@user_id, NOW(), @total_cost);";

            db.ExecQuery(query);

            if (!string.IsNullOrEmpty(db.Exception))
            {
                MessageBox.Show($"Error: {db.Exception}", "insert order");
                return -1;  // Indicate error
            }

            // Return the last inserted order ID
            return db.GetLastInsertId();  // This is for MySQL, use appropriate method in C# to get the last inserted ID
        }

        private void InsertOrderProductsAndClearCart(int orderId, List<(int productId, int quantity, decimal price)> cartItems, int userId)
        {
            SQLControl db = new SQLControl();

            // Insert products into ORDER_PRODUCTS
            foreach (var item in cartItems)
            {
                db.AddParam("@order_id", orderId);
                db.AddParam("@product_id", item.productId);
                db.AddParam("@quantity", item.quantity);
                db.AddParam("@price", item.price);
                db.AddParam("@subtotal", item.price * item.quantity);

                string query = @"
                    INSERT INTO ORDER_PRODUCTS (ORDER_ID, PRODUCT_ID, QUANTITY, PRICE, SUBTOTAL)
                    VALUES (@order_id, @product_id, @quantity, @price, @subtotal);";

                db.ExecQuery(query);

                query = @"insert into user_activity(user_id, product_id, activity_type, activity_time) values (@user_id, @product_id, 'PURCHASE', NOW());";
                db.AddParam("@user_id", Active_User.LoggedInUserId);
                db.AddParam("@product_id", item.productId);
                db.ExecQuery(query);

                if (!string.IsNullOrEmpty(db.Exception))
                {
                    MessageBox.Show($"{orderId}, {item.productId},{item.quantity}, {item.price}, {item.price * item.quantity}");
                    MessageBox.Show($"Error: {db.Exception}", "insert orderproducts");
                    return;
                }
            }

            // Clear the user's cart
            db.AddParam("@user_id", userId);
            string deleteQuery = "DELETE FROM USER_CART_PRODUCTS WHERE USER_ID = @user_id;";
            db.ExecQuery(deleteQuery);

            if (!string.IsNullOrEmpty(db.Exception))
            {
                MessageBox.Show($"Error clearing cart: {db.Exception}");
            }
        }



    }
}
