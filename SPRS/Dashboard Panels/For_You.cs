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

namespace SPRS.Dashboard_Panels
{
    public partial class For_You : BasePanel
    {
        public For_You()
        {
            InitializeComponent();
            bool UserHasActivity = Check_User_Activity();
            if (UserHasActivity) Generate_For_You();
            else FallbackRecommendation();
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
                MessageBox.Show($"Error: {db.Exception}", "check activity erro");
            }

            if (db.SQLDS != null && db.SQLDS.Tables.Count > 0 && db.SQLDS.Tables[0].Rows.Count > 0)
            {
                // Check if the count is greater than 10, meaning the user has activity plenty of interaction
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

        private void Generate_For_You()
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
                "HAVING SimilarityScore > 0 " + // the more data in these tables the higher this value can go, giving users a tighter recommendation system
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
            "LIMIT 150;";

            db.AddParam("@user_id", Active_User.LoggedInUserId);
            db.ExecQuery(query);

            if (!string.IsNullOrEmpty(db.Exception))
            {
                MessageBox.Show($"Error: {db.Exception}", "generate for you error");
                return;
            }

            List<int> productIds = new List<int>();

            if (db.SQLDS.Tables.Count > 0 && db.SQLDS.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow row in db.SQLDS.Tables[0].Rows)
                {
                    // Convert the product ID to an integer and add it to the list
                    productIds.Add(Convert.ToInt32(row["PRODUCT_ID"]));
                }

                
                Search_Result_Panel search_Result_Panel = new Search_Result_Panel(productIds);
                search_Result_Panel.Dock = DockStyle.Fill;
                panel1.Controls.Clear();
                search_Result_Panel.PanelChangeRequest += HandlePanelChangeRequest;
                panel1.Controls.Add(search_Result_Panel);
            }
            else return;

            
        }

        private void FallbackRecommendation()
        {
            // as a fallback when users have no previous activity, show the best books from the past month 

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

            db.ExecQuery(query);

            if (!string.IsNullOrEmpty(db.Exception))
            {
                MessageBox.Show($"Error: {db.Exception}", "fallback recommendations error");
                return;
            }

            List<int> productIds = new List<int>();

            if (db.SQLDS.Tables.Count > 0 && db.SQLDS.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow row in db.SQLDS.Tables[0].Rows)
                {
                    if (int.TryParse(row["PRODUCT_ID"].ToString(), out int productId))
                    {
                        productIds.Add(productId);
                    }
                }

                Search_Result_Panel search_Result_Panel = new Search_Result_Panel(productIds);
                search_Result_Panel.Dock = DockStyle.Fill;
                panel1.Controls.Clear();
                search_Result_Panel.PanelChangeRequest += HandlePanelChangeRequest;
                panel1.Controls.Add(search_Result_Panel);
            }
        }
        private void HandlePanelChangeRequest(object sender, string panelTag)
        {
            RequestPanelChange(panelTag);
        }
    }
}
