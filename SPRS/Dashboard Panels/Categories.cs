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

namespace SPRS.Dashboard_Panels
{
    public partial class Categories : BasePanel
    {

        public Categories()
        {
            InitializeComponent();
            Populate_ComboBox_Options();
        }
        private void Populate_ComboBox_Options()
        {
            SQLControl db = new SQLControl();

            string query =
                "SELECT PRIMARY_CATEG AS CATEGORY FROM PRODUCT " +
                "UNION " +
                "SELECT SECONDARY_CATEG AS CATEGORY FROM PRODUCT " +
                "ORDER BY CATEGORY;";
            db.ExecQuery(query);

            if (!string.IsNullOrEmpty(db.Exception))
            {
                MessageBox.Show($"Error: {db.Exception}");
                return;
            }

            comboBox1.DataSource = db.SQLDS.Tables[0];  // Bind the data table
            comboBox1.DisplayMember = "CATEGORY";              // Column to display
            comboBox1.ValueMember = "CATEGORY";
        }

        private void Update_Panel(object sender, EventArgs e)
        {
            string selectedCategory = comboBox1.SelectedValue?.ToString();

            if (string.IsNullOrEmpty(selectedCategory))
            {
                MessageBox.Show("Please select a valid category.");
                return;
            }
            SQLControl db = new SQLControl();

            string query =
                "SELECT PRODUCT_ID " +
                "FROM PRODUCT " +
                "WHERE PRIMARY_CATEG = @categ OR SECONDARY_CATEG = @categ " +
                "ORDER BY COPIES_SOLD DESC;";

            db.AddParam("@categ", selectedCategory);

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
                MessageBox.Show("No products found for the selected category.");
            }
        }
        private void HandlePanelChangeRequest(object sender, string panelTag)
        {
            RequestPanelChange(panelTag);
        }
    }
}
