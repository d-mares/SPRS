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

namespace SPRS.Dashboard_Panels
{
    public partial class Search_Result_Panel : UserControl
    {
        public Search_Result_Panel()
        {
            InitializeComponent();
        }
        public Search_Result_Panel(List<int> results)
        {
            InitializeComponent();
            result_list = results;
            current_index = 0;
            Load_Items(15);


        }
        private List<int> result_list;
        int current_index;

        public event EventHandler<string> PanelChangeRequest;

        private void Load_Items(int count)
        {
            //get products to add to the list
            int end_index = Math.Min(current_index + count, result_list.Count);
            var productsToLoad = result_list.GetRange(current_index, end_index - current_index);

            // add the products to the panel
            foreach (int productId in productsToLoad)
            {
                Product_List_Item productItem = new Product_List_Item(productId);
                productItem.ReplacePanelRequested += HandlePanelChangeRequest;

                flowLayoutPanel1.Controls.Add(productItem);
            }

            // update the index
            current_index = end_index;
            
            if (current_index >= result_list.Count)
            {
                roundedPanel1.Hide();
            }

        }

        private void HandlePanelChangeRequest(object sender, string tag )
        {
            PanelChangeRequest?.Invoke(sender, tag);
        }

        private void See_More(object sender, EventArgs e)
        {
            Load_Items(15);
        }
    }
}
