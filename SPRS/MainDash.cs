using SPRS.Active_Classes;
using SPRS.Custom_Controls;
using SPRS.Dashboard_Panels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SPRS
{
    public partial class MainDash : Form
    {
        public MainDash()
        {
            InitializeComponent();

            LoadPanelByTag("HOME");
        }

        private void nav_hover_in(object sender, EventArgs e)
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

        // 
        //  GUI Panel Generation, this function receives the tag from the button and rebuilds the gui accordingly
        //
        private void Generate_Main_Panel(object sender, EventArgs e)
        {
            Control control = (Control)sender;
            LoadPanelByTag(control.Tag.ToString());
        }

        private void LoadPanelByTag(string tag) {

            UserControl ActivePanel = null;
            switch (tag)
            {
                case "HOME":
                    ActivePanel = new Home_Panel();
                    break;
                case "POPULAR":
                    ActivePanel = new Popular_Products();
                    break;
                case "BEST_SELLERS":
                    ActivePanel = new Best_Sellers();
                    break;
                case "FOR_YOU":
                    ActivePanel = new For_You();
                    break;
                case "CATEG":
                    ActivePanel = new Categories();
                    break;
                case "AUTHORS":
                    ActivePanel = new Authors();
                    break;
                case "PUBLISH":
                    ActivePanel = new Publishers();
                    break;
                case "ORDER_HISTORY":
                    ActivePanel = new OrderHistory();
                    break;
                case "WISHLIST":
                    ActivePanel = new Wishlist();
                    break;
                case "CART":
                    ActivePanel = new CartPanel();
                    break;
                case "SEARCH":
                    Active_User.Searchbar_Text = textBox1.Text;
                    ActivePanel = new Search_Panel();
                    break;
                case "DETAILS":
                    ActivePanel = new Product_Details();
                    break;

            }

            if (ActivePanel is BasePanel basePanel)
            {
                basePanel.PanelChangeRequested += HandlePanelChangeRequested;
            }

            if (ActivePanel != null)
            {
                // The new panel will fill the whole existing panel when added
                ActivePanel.Dock = DockStyle.Fill;

                // Add panel to stack
                Active_User.panels.Push(ActivePanel);

                // Clear the current controls in the panel
                Main_Fillable_Panel.Controls.Clear();

                // Add the User Control to Main Fillable Panel
                Main_Fillable_Panel.Controls.Add(ActivePanel);
            }
        }
        private void HandlePanelChangeRequested(object sender, string panelTag)
        {
            // Handle panel change requests from UserControls
            LoadPanelByTag(panelTag);
        }

        private void roundButton12_Click(object sender, EventArgs e)
        {
            Active_User.LoggedInUserId = -1;
            Active_User.LoggedInUsername = "";

            Login login = new Login();
            this.Hide();
            login.Show();
        }

        private void Go_Back(object sender, EventArgs e)
        {
            if (Active_User.panels.Count > 0)
            {
                Control panel = Active_User.panels.Pop();

                if (Main_Fillable_Panel.Controls[0] == panel && Active_User.panels.Count > 0)
                {
                    panel = Active_User.panels.Pop();
                }

                Main_Fillable_Panel.Controls.Clear();

                Main_Fillable_Panel.Controls.Add(panel);
            }
        }
    }
}