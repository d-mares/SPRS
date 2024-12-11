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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolBar;

namespace SPRS.Dashboard_Panels
{
    public partial class OrderHistory : BasePanel
    {
        public OrderHistory()
        {
            InitializeComponent();
            PopulateHistory();
        }

        private void PopulateHistory()
        {
            SQLControl db = new SQLControl();
            string query = "SELECT ORDER_ID, ORDER_TIME, TOTAL FROM ORDERS WHERE USER_ID = @user ORDER BY ORDER_TIME DESC;";
            db.AddParam("@user", Active_User.LoggedInUserId);

            db.ExecQuery(query);

            if (db.RecordCount == 0)
            {
                // If no orders are found, set Button1 text to "No History"
                button1.Text = "No orders placed yet";
                comboBox1.Hide();
                pictureBox4.Hide();
                return;
            }

            if (!string.IsNullOrEmpty(db.Exception))
            {
                MessageBox.Show($"Error: {db.Exception}");
                return;
            }

            foreach (DataRow row in db.SQLDS.Tables[0].Rows)
            {
                // Extract the ORDER_ID, ORDER_TIME, and TOTAL
                int orderId = Convert.ToInt32(row["ORDER_ID"]);
                DateTime orderTime = Convert.ToDateTime(row["ORDER_TIME"]);
                decimal total = Convert.ToDecimal(row["TOTAL"]);

                // Format the display text as "yyyy-MM-dd HH:mm:ss - $price"
                string displayText = orderTime.ToString("yyyy-MM-dd HH:mm:ss") + " - $" + total.ToString("F2");

                // Add the formatted text and value (order ID) to the ComboBox
                comboBox1.Items.Add(new KeyValuePair<int, string>(orderId, displayText));
            }

            // Set the display member and value member for ComboBox
            comboBox1.DisplayMember = "Value";
            comboBox1.ValueMember = "Key";
        }

        private void Show_Order(object sender, EventArgs e)
        {
            SQLControl db = new SQLControl();

            if (comboBox1.SelectedItem == null)
            {
                MessageBox.Show("Please select an order.");
                return;
            }

            KeyValuePair<int, string> selectedOrder = (KeyValuePair<int, string>)comboBox1.SelectedItem;
            int orderId = selectedOrder.Key;

            string query = @"
                SELECT PRODUCT_ID
                FROM ORDER_PRODUCTS
                WHERE ORDER_ID = @order";

            db.AddParam("@order", orderId);

            db.ExecQuery(query);

            if (db.RecordCount == 0)
            {
                MessageBox.Show("No products found for this order.");
                return;
            }

            flowLayoutPanel1.Controls.Clear();

            foreach (DataRow row in db.SQLDS.Tables[0].Rows)
            {
                int productId = Convert.ToInt32(row["PRODUCT_ID"]);

                // Create a new instance of History_Item
                History_Item historyItem = new History_Item(productId);

                // Add the control to the FlowLayoutPanel
                flowLayoutPanel1.Controls.Add(historyItem);
            }
            button1?.Dispose();

            string totalQuery = @"
                SELECT SUM(SUBTOTAL) AS Total
                FROM ORDER_PRODUCTS
                WHERE ORDER_ID = @order";

            db.AddParam("@order", orderId);
            db.ExecQuery(totalQuery);

            if (db.SQLDS.Tables[0].Rows.Count > 0)
            {
                decimal total = Convert.ToDecimal(db.SQLDS.Tables[0].Rows[0]["Total"]);
                button3.Text = $"Order Total: ${total:F2}";
            }

            
        }
    }

}
