using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    class MenuItemOvveride : MenuItem
    {
        public event Action BeforeDispose;
        private ContextMenuOvveride contextMenu;
        public MenuItemOvveride(string header , ContextMenuOvveride contextMenu) : base(header)
        {
            this.contextMenu = contextMenu;
        }
        protected override void Dispose(bool disposing)
        {
            BeforeDispose?.Invoke();
            base.Dispose(disposing);
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            this.contextMenu.OnClickThenDispose(null);
        }
    }

    class ContextMenuOvveride : ContextMenu
    {
        public event EventHandler ClickThenCollapse;
        public ContextMenuOvveride() : base()
        {

        }

        public void OnClickThenDispose(EventArgs e)
        {
            ClickThenCollapse?.Invoke(this, e);
        }
    }
}
