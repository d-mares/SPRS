using Mysqlx.Crud;
using SPRS.Active_Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SPRS.Dashboard_Panels
{
    public partial class Popular_Products : BasePanel
    {
        public Popular_Products()
        {
            InitializeComponent();
            Generate_Hottest_100();
        }

        private void Generate_Hottest_100()
        {
            SQLControl db = new SQLControl();

            // Getting the most interacted books from the past week (weighted)
            string query =
            "SELECT PRODUCT_ID, " +

                // Summing the weighted activity scores for views, wishlist, add to cart, and purchase
                "SUM(CASE " +
                        "WHEN ACTIVITY_TYPE = 'PURCHASE' THEN 5 " +
                        "WHEN ACTIVITY_TYPE = 'ADD_TO_CART' THEN 3 " +
                        "WHEN ACTIVITY_TYPE = 'WISHLIST' THEN 2 " +
                        "WHEN ACTIVITY_TYPE = 'VIEW' THEN 1 " +
                        "ELSE 0 " +
                    "END) as activity_score " +

                "FROM USER_ACTIVITY " +
                "WHERE ACTIVITY_TIME >= NOW() - INTERVAL 7 DAY " +
                "GROUP BY PRODUCT_ID " +
                "ORDER BY activity_score DESC " + 
                "LIMIT 100;";

            db.ExecQuery(query);

            if (!string.IsNullOrEmpty(db.Exception))
            {
                MessageBox.Show($"Error: {db.Exception}");
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
