using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    static class ListExtentions
    {
        public static ContextMenuOvveride CreatePopUpMenu(this List<MenuItemData> menuItemDataList, EventHandler popupMenuClosed)
        {
            ContextMenuOvveride popupMenu = new ContextMenuOvveride();
            menuItemDataList.ForEach(menuItemData => AddMenuItem(popupMenu, menuItemData));

            popupMenu.ClickThenCollapse += popupMenuClosed;
            return popupMenu;
        }

        private static void AddMenuItem(ContextMenuOvveride menu, MenuItemData menuItemData)
        {
            EventHandler onClick = (obj, args) => menuItemData.OnClick(menuItemData.SelectedListViewProperty, menuItemData.SelectedListViewItem);
            menu.AddMenuItem(menuItemData.Header, onClick, menuItemData.ToolTip);
        }

        public static bool IsDiffrentFrom<T>(this List<T> current, List<T> other)
        {
            if (current == null || other == null || current.Count != other.Count) return true;

            for (int i = 0; i < current.Count; i++)
            {
                if (!current[i].Equals(other[i]))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
