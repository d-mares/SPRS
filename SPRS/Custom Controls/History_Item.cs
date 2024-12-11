using SPRS.Active_Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SPRS.Custom_Controls
{
    public partial class History_Item : UserControl
    {

        int prod_id;
        public event EventHandler<string> ReplacePanelRequested;
        Product product;

        public History_Item(int id)
        {
            InitializeComponent();
            prod_id = id;
            product = Queried_Products.GetProductById(prod_id);
            Load_Image(prod_id);
            Product_Details_Load();
        }

        private void Product_Details_Load()
        {
            button1.Text = $"{product.Title} \r\n {product.Author} ";
            button2.Text = $"$ {product.Price.ToString("n2")}";
            button3.Text = $"{product.Publisher}";
            button8.Text = $"{product.PrimaryCategory}, {product.SecondaryCategory}";
        }

        private void Send_To_Product(object sender, EventArgs e)
        {
            Active_User.Product_To_Be_Shown = prod_id;

            ReplacePanelRequested?.Invoke(this, "DETAILS");
        }

        private void Load_Image(int id)
        {
            string solution_directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\");
            string imagePath = Path.Combine(solution_directory, "product_images", $"{id}.jpg");
            try
            {
                pictureBox1.Image = Image.FromFile(imagePath);
            }
            catch
            {
                pictureBox1.Image = Properties.Resources.Default_Book;
            }
        }

    }
}
