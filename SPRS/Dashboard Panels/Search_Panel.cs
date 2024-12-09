using Google.Protobuf.Reflection;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace SPRS.Dashboard_Panels
{
    public partial class Search_Panel : BasePanel
    {
        public Search_Panel()
        {
            InitializeComponent();
            Generate_Search_Result();
            label1.Text += $" '{Active_User.Searchbar_Text}'";
        }

        private void Generate_Search_Result()
        {
            SQLControl db = new SQLControl();

            string query =
                "SELECT PRODUCT_ID " +
                "FROM PRODUCT " +
                "WHERE TITLE LIKE @title;";

            db.AddParam("@title", $"%{Active_User.Searchbar_Text}%");
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
            else
            {
                MessageBox.Show("No books found for the selected author.");
            }
        }
        private void HandlePanelChangeRequest(object sender, string panelTag)
        {
            RequestPanelChange(panelTag);
        }
    }
}
