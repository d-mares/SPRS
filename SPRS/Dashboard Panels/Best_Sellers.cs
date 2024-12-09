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
    public partial class Best_Sellers : BasePanel
    {
        public Best_Sellers()
        {
            InitializeComponent();
            Generate_Top_100();
        }

        private void Generate_Top_100()
        {
            SQLControl db = new SQLControl();

            string query =
                "SELECT PRODUCT_ID " +
                "FROM PRODUCT " +
                "ORDER BY COPIES_SOLD DESC LIMIT 100";

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
