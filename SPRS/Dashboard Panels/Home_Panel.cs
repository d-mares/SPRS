using SPRS.Active_Classes;
using SPRS.Custom_Controls;
using SPRS.Dashboard_Panels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace SPRS
{
    public partial class Home_Panel : BasePanel
    {
        SQLControl db = new SQLControl();
        public Home_Panel()
        {
            InitializeComponent();
            bool UserHasActivity = Check_User_Activity();
            if (UserHasActivity)
            {
                Show_Recommended_Category();
                Show_ForYou_Choices();
            }
            else { ShowBestCategory(); FallbackRecommendations(); }
        }

        private void FallbackRecommendations()
        {
            SQLControl db = new SQLControl();

            string query =
                "SELECT P.PRODUCT_ID, " +
                       "P.TITLE, " +
                       "COUNT(UA.PRODUCT_ID) AS TotalInteractions, " +
                       "SUM(CASE WHEN UA.ACTIVITY_TYPE = 'PURCHASE' THEN 10 " +
                                "WHEN UA.ACTIVITY_TYPE = 'ADD_TO_CART' THEN 5 " +
                                "WHEN UA.ACTIVITY_TYPE = 'WISHLIST' THEN 3 " +
                                "WHEN UA.ACTIVITY_TYPE = 'VIEW' THEN 1 END) AS WeightedScore " +
                "FROM USER_ACTIVITY UA " +
                "JOIN PRODUCT P ON UA.PRODUCT_ID = P.PRODUCT_ID " +
                "WHERE UA.ACTIVITY_TIME > NOW() - INTERVAL 30 DAY " +
                "GROUP BY P.PRODUCT_ID, P.TITLE " +
                "ORDER BY WeightedScore DESC, TotalInteractions DESC " +
                "LIMIT 150;";

            db.AddParam("@user_id", Active_User.LoggedInUserId);
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
                    // Convert the product ID to an integer and add it to the list
                    prods.Add(Convert.ToInt32(row["PRODUCT_ID"]));
                }
            }
            else return;

            Single_Carousel carousel = new Single_Carousel(prods);
            carousel.Dock = DockStyle.Fill;
            carousel.PanelChangeRequest += HandlePanelChangeRequest;
            tableLayoutPanel4.Controls.Add(carousel);
        }

        private void Show_ForYou_Choices()
        {
            SQLControl db = new SQLControl();

            string query =
                "WITH SimilarUsers AS ( " +
                    "SELECT UA2.USER_ID, " +
                           "SUM(CASE WHEN UA1.ACTIVITY_TYPE = 'PURCHASE' AND UA2.ACTIVITY_TYPE = 'PURCHASE' THEN 10 " +
                                    "WHEN UA1.ACTIVITY_TYPE = 'ADD_TO_CART' AND UA2.ACTIVITY_TYPE = 'ADD_TO_CART' THEN 5 " +
                                    "WHEN UA1.ACTIVITY_TYPE = 'WISHLIST' AND UA2.ACTIVITY_TYPE = 'WISHLIST' THEN 3 " +
                                    "WHEN UA1.ACTIVITY_TYPE = 'VIEW' AND UA2.ACTIVITY_TYPE = 'VIEW' THEN 1 END) AS SimilarityScore " +
                    "FROM USER_ACTIVITY UA1 " +
                    "JOIN USER_ACTIVITY UA2 ON UA1.PRODUCT_ID = UA2.PRODUCT_ID " +
                    "WHERE UA1.USER_ID = @user_id AND UA1.USER_ID != UA2.USER_ID " +
                    "GROUP BY UA2.USER_ID " +
                    "HAVING SimilarityScore > 5 " + // the more data in these tables the higher this value can go, giving users a tighter recommendation system
                "), " +
                "BookInteractions AS ( " +
                    "SELECT UA.PRODUCT_ID, " +
                           "SUM(CASE WHEN UA.ACTIVITY_TYPE = 'PURCHASE' THEN 10 " +
                                    "WHEN UA.ACTIVITY_TYPE = 'ADD_TO_CART' THEN 5 " +
                                    "WHEN UA.ACTIVITY_TYPE = 'WISHLIST' THEN 3 " +
                                    "WHEN UA.ACTIVITY_TYPE = 'VIEW' THEN 1 END) AS InteractionScore " +
                    "FROM USER_ACTIVITY UA " +
                    "JOIN SimilarUsers SU ON UA.USER_ID = SU.USER_ID " +
                    "GROUP BY UA.PRODUCT_ID " +
                "), " +
                "UserPurchases AS ( " +
                    "SELECT OP.PRODUCT_ID " +
                    "FROM ORDERS O " +
                    "JOIN ORDER_PRODUCTS OP ON O.ORDER_ID = OP.ORDER_ID " +
                    "WHERE O.USER_ID = @user_id " +
                "), " +
                "FilteredBooks AS ( " +
                    "SELECT BI.PRODUCT_ID, BI.InteractionScore " +
                    "FROM BookInteractions BI " +
                    "LEFT JOIN UserPurchases UP ON BI.PRODUCT_ID = UP.PRODUCT_ID " +
                    "WHERE UP.PRODUCT_ID IS NULL " +
                ") " +
                "SELECT P.PRODUCT_ID, P.TITLE, P.COPIES_SOLD, FB.InteractionScore " +
                "FROM PRODUCT P " +
                "JOIN FilteredBooks FB ON P.PRODUCT_ID = FB.PRODUCT_ID " +
                "ORDER BY FB.InteractionScore DESC " +
                "LIMIT 5;";

            db.AddParam("@user_id", Active_User.LoggedInUserId);
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
                    // Convert the product ID to an integer and add it to the list
                    prods.Add(Convert.ToInt32(row["PRODUCT_ID"]));
                }
            }
            else return;

            if (prods.Count < 5) { FallbackRecommendations(); return; }
            Single_Carousel carousel = new Single_Carousel(prods);
            carousel.Dock = DockStyle.Fill;
            carousel.PanelChangeRequest += HandlePanelChangeRequest;
            tableLayoutPanel4.Controls.Add(carousel);
        }

        private bool Check_User_Activity()
        {
            SQLControl db = new SQLControl();

            string query =
                "SELECT COUNT(*) FROM USER_ACTIVITY WHERE USER_ID = @user;";

            db.AddParam("@user", Active_User.LoggedInUserId);
            db.ExecQuery(query);

            if (!string.IsNullOrEmpty(db.Exception))
            {
                MessageBox.Show($"Error: {db.Exception}");
            }

            if (db.SQLDS != null && db.SQLDS.Tables.Count > 0 && db.SQLDS.Tables[0].Rows.Count > 0)
            {
                // Check if the count is greater than 10 to give basic recommendations until a threshhold of 10 interactions occurs
                if (int.Parse(db.SQLDS.Tables[0].Rows[0][0].ToString()) > 10)
                {
                    return true; // User has activity
                }
                else
                {
                    return false; // User has no previous activity
                }
            }
            else
            {
                // If no results, handle the case where no data is returned
                MessageBox.Show("No data returned from query.");
                return false;
            }

        }

        private void Show_Recommended_Category()
        {
            SQLControl db = new SQLControl();

            string query =
            // Subquery to find the category with the most interaction weighted by type of activity
            "WITH MostInteractedCategory AS ( " +
                "SELECT P.PRIMARY_CATEG AS category " +
                "FROM USER_ACTIVITY A " +
                "JOIN PRODUCT P ON A.PRODUCT_ID = P.PRODUCT_ID " +
                "WHERE A.USER_ID = @user_id " +
                "GROUP BY P.PRIMARY_CATEG " +
                "ORDER BY SUM( " +
                    "CASE " +
                        "WHEN A.ACTIVITY_TYPE = 'PURCHASE' THEN 10 " +
                        "WHEN A.ACTIVITY_TYPE = 'ADD_TO_CART' THEN 5 " +
                        "WHEN A.ACTIVITY_TYPE = 'WISHLIST' THEN 3 " +
                        "WHEN A.ACTIVITY_TYPE = 'VIEW' THEN 1 " +
                    "END " +
                ") DESC " +
                "LIMIT 1 " +
            ") " +

            // Query to get the 5 books with the highest sales in that category, do
            "SELECT P.PRODUCT_ID, P.PRIMARY_CATEG " +
            "FROM PRODUCT P " +
            "LEFT JOIN ORDER_PRODUCTS OP ON P.PRODUCT_ID = OP.PRODUCT_ID " +
            "LEFT JOIN ORDERS O ON OP.ORDER_ID = O.ORDER_ID " +
            "WHERE P.PRIMARY_CATEG = (SELECT category FROM MostInteractedCategory) " +
            "AND P.PRODUCT_ID NOT IN ( " +
                "SELECT OP.PRODUCT_ID " +
                "FROM ORDERS O " +
                "JOIN ORDER_PRODUCTS OP ON O.ORDER_ID = OP.ORDER_ID " +
                "WHERE O.USER_ID = @user_id " +
            ") " +
            "ORDER BY P.COPIES_SOLD DESC " +
            "LIMIT 5;";

            db.AddParam("@user_id", Active_User.LoggedInUserId);
            db.ExecQuery(query);

            if (!string.IsNullOrEmpty(db.Exception))
            {
                MessageBox.Show($"Error: {db.Exception}");
                return;
            }

            List<int> prods = new List<int>();

            if (db.SQLDS != null && db.SQLDS.Tables.Count > 0)
            {
                Active_User.recommended_categ = db.SQLDS.Tables[0].Rows[0]["PRIMARY_CATEG"].ToString();
                foreach (DataRow row in db.SQLDS.Tables[0].Rows)
                {
                    // Convert the product ID to an integer and add it to the list
                    prods.Add(Convert.ToInt32(row["PRODUCT_ID"]));
                }
            }
            else return;

            Single_Carousel carousel = new Single_Carousel(prods);
            carousel.Dock = DockStyle.Fill;
            carousel.PanelChangeRequest += HandlePanelChangeRequest;
            tableLayoutPanel6.Controls.Add(carousel);
            button6.Text += $"{Active_User.recommended_categ}\"";
        }

        private void ShowBestCategory()
        {
            SQLControl db = new SQLControl();

            string query = 
                "WITH BestSellingCategory AS ( " +
                    "SELECT PRIMARY_CATEG AS category " +
                    "FROM PRODUCT " +
                    "GROUP BY PRIMARY_CATEG " +
                    "ORDER BY SUM(COPIES_SOLD) DESC " +
                    "LIMIT 1 " +
                ") " +
                "SELECT P.PRODUCT_ID, P.PRIMARY_CATEG " +
                "FROM PRODUCT P " +
                "WHERE P.PRIMARY_CATEG = (SELECT category FROM BestSellingCategory) " +
                "ORDER BY P.COPIES_SOLD DESC " +
                "LIMIT 5;";

            db.ExecQuery(query);

            if (!string.IsNullOrEmpty(db.Exception))
            {
                MessageBox.Show($"Error: {db.Exception}");
                return;
            }
            List<int> prods = new List<int>();
            

            if (db.SQLDS != null && db.SQLDS.Tables.Count > 0)
            {
                Active_User.recommended_categ = db.SQLDS.Tables[0].Rows[0]["PRIMARY_CATEG"].ToString();
                foreach (DataRow row in db.SQLDS.Tables[0].Rows)
                {
                    // Convert the product ID to an integer and add it to the list
                    prods.Add(Convert.ToInt32(row["PRODUCT_ID"]));
                }
            }
            else return;
            Single_Carousel carousel = new Single_Carousel(prods);
            carousel.Dock = DockStyle.Fill;
            carousel.PanelChangeRequest += HandlePanelChangeRequest;
            tableLayoutPanel6.Controls.Add(carousel);
            button6.Text += $"{Active_User.recommended_categ}\"";
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
            Active_User.Product_To_Be_Shown = random.Next(max);


            // Check if the sender has a Tag property and retrieve its value
            if (sender is Control control && control.Tag != null)
            {
                // Trigger the event with the retrieved tag
                RequestPanelChange(control.Tag.ToString());
            }
        }

        private void HandlePanelChangeRequest(object sender, string panelTag)
        {
            RequestPanelChange(panelTag);
        }

        private void Send_To_Category(object sender, EventArgs e)
        {

        }

        private void roundedPanel9_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
