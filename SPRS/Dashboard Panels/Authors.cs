using System;
using System.Collections;
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
    public partial class Authors : BasePanel
    {
        public Authors()
        {
            InitializeComponent();
        }

        private void Populate_Options(object sender, EventArgs e)
        {
            SQLControl db = new SQLControl();

            Button btn = sender as Button;
            string range_condition = $"WHERE LEFT(LAST_NAME, 1) = '{btn.Tag.ToString()}'";

            // no authors populated if they have no associated books
            string query =
                "SELECT DISTINCT AUTHOR.LAST_NAME " +
                "FROM AUTHOR " +
                "INNER JOIN PRODUCT ON AUTHOR.AUTHOR_ID = PRODUCT.AUTHOR_ID " +
                $"{range_condition} " +
                "ORDER BY AUTHOR.LAST_NAME;";

            db.ExecQuery(query);

            if (!string.IsNullOrEmpty(db.Exception))
            {
                MessageBox.Show($"Error: {db.Exception}");
                return;
            }

            comboBox1.DataSource = db.SQLDS.Tables[0];  // Bind the data table
            comboBox1.DisplayMember = "LAST_NAME";              // Column to display
            comboBox1.ValueMember = "LAST_NAME";
            UpdatePanel(range_condition);
            
        }

        private void Update_Panel(object sender, EventArgs e)
        {
            UpdatePanel("");
        }

        private void UpdatePanel(string range_condition)
        {
            string authorLastName = comboBox1.SelectedValue?.ToString();

            if (string.IsNullOrEmpty(authorLastName))
            {
                MessageBox.Show("No authors in this section");
                return;
            }
            SQLControl db = new SQLControl();

            string query;

            if (range_condition == "")
            {
                query =
                    "SELECT PRODUCT.PRODUCT_ID " +
                    "FROM PRODUCT " +
                    "INNER JOIN AUTHOR ON PRODUCT.AUTHOR_ID = AUTHOR.AUTHOR_ID " +
                    "WHERE AUTHOR.LAST_NAME = @lastName " +
                    "ORDER BY PRODUCT.COPIES_SOLD DESC;";
                db.AddParam("@lastName", authorLastName);
            }
            else
            {
                query =
                   "SELECT PRODUCT.PRODUCT_ID " +
                   "FROM PRODUCT " +
                   "INNER JOIN AUTHOR ON PRODUCT.AUTHOR_ID = AUTHOR.AUTHOR_ID " +
                   $"{range_condition} " +
                   "ORDER BY AUTHOR.LAST_NAME;";
            }
            

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

        private void Nav_hover_in(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            btn.BackColor = Color.FromArgb(21, 96, 130);
            btn.ForeColor = Color.White;
        }

        private void Nav_Hover_out(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            btn.BackColor = Color.White;
            btn.ForeColor = Color.FromArgb(21, 96, 130);
        }
    }
}
