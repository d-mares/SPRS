using System;
using System.Collections.Generic;
using System.Data;
using System.IO.Pipelines;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SPRS.Active_Classes
{
    
    public class Product
    {
        public int ProductId { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string PrimaryCategory { get; set; }
        public string SecondaryCategory { get; set; }
        public string Publisher { get; set; }
        public string Description { get; set; }
        public string Year { get; set; }
        public double Price { get; set; }
        public int CopiesSold { get; set; }

        SQLControl db = new SQLControl();
        public Product(int id) 
        {
            ProductId = id;
        }

        public void LoadProductData()
        {
            string query =
               "SELECT p.PRODUCT_ID, p.TITLE, a.FIRST_NAME, a.LAST_NAME, p.PRIMARY_CATEG, p.SECONDARY_CATEG, p.PUBLISHER, d.DESCRIPTION_CONTENT, p.YEAR_PUBLISHED, p.PRICE, p.COPIES_SOLD " +
                "FROM PRODUCT p " +
                "LEFT JOIN AUTHOR a ON p.AUTHOR_ID = a.AUTHOR_ID " +
                "LEFT JOIN PRODUCT_DESCRIPTION d ON p.DESCRIPTION_ID = d.DESCRIPTION_ID " +
                "WHERE p.PRODUCT_ID = @product_id";

            db.AddParam("@product_id", ProductId);
            db.ExecQuery(query);

            if (db.RecordCount == 1)
            {
                DataTable table = db.SQLDS.Tables[0];

                // Populate properties using the first row of data
                DataRow row = table.Rows[0];

                Title = row["TITLE"].ToString();
                Author = row["FIRST_NAME"].ToString() + " " + row["LAST_NAME"].ToString();
                PrimaryCategory = row["PRIMARY_CATEG"].ToString();
                SecondaryCategory = row["SECONDARY_CATEG"].ToString();
                Publisher = row["PUBLISHER"].ToString();
                Description = row["DESCRIPTION_CONTENT"].ToString();
                Year = row["YEAR_PUBLISHED"].ToString();
                Price = Convert.ToDouble(row["PRICE"]);
                CopiesSold = Convert.ToInt32(row["COPIES_SOLD"]);
            }
        }
        
    }
}
