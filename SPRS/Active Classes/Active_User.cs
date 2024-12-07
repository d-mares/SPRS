using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPRS.Active_Classes
{
    public static class Active_User
    {
        public static int LoggedInUserId = -1; // Default value (invalid ID)
        public static string LoggedInUsername = "";

        // This is to hold the value of product id for when the product details page is about to generate
        public static int Product_To_Be_Shown = -1;
    }
}
