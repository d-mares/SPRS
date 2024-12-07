using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SPRS
{
    public class BasePanel : UserControl
    {
        public event EventHandler<string> PanelChangeRequested;

        protected void RequestPanelChange(string panelTag)
        {
            PanelChangeRequested?.Invoke(this, panelTag);
        }
    }
}
