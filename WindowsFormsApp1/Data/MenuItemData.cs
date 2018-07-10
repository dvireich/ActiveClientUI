using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public class MenuItemData
    {
        public string Header;
        public Action<ListView,ListViewItem> OnClick;
        public string ToolTip = null;

        public ListViewItem SelectedListViewItem;
        public ListView SelectedListViewProperty;
    }
}
