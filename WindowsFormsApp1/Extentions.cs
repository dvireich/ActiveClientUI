using System;
using System.Drawing;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    static class Extentions
    {
        public static void AddMenuItem(this ContextMenuOvveride cm, string header, EventHandler onClick, string tooltip = default(string))
        {
            OptimisticTryAndFail(() =>
            {
                var mi = new MenuItemOvveride(header, cm);

                //mi.tip = tooltip;
                mi.Click += onClick;
                Action beforeDisposing = null;
                beforeDisposing = () =>
                {
                    mi.BeforeDispose -= beforeDisposing;
                    mi.Click -= onClick;
                };
                mi.BeforeDispose += beforeDisposing;
                cm.MenuItems.Add(mi);
            }, 1000, 3, "Fail to AddMenuItem");
        }

        public static int GetColumnNumber(this ListView listview, string columnHeader)
        {
            int ret = -1;
            OptimisticTryAndFail(() =>
            {
                foreach (ColumnHeader column in listview.Columns)
                {
                    if (column.Text == columnHeader)
                    {
                        ret = column.Index;
                        return;
                    }   
                }
                ret = -1;
                return;
            }, 1000, 3, "Fail to GetColumnNumber");
            return ret;
        }

        public static int GetMenuStripItemIndex (this MenuStrip menu, string name)
        {
            int ret = -1;
            OptimisticTryAndFail(() =>
            {
                var index = 0;
                foreach (ToolStripItem item in menu.Items)
                {
                    if (item.Text == name)
                    {
                        ret = index;
                        return;
                    }   
                    index++;
                }
                ret = -1;
                return;
            }, 1000, 3, "Fail to GetMenuStripItemIndex");
            return ret;
        }

        public static void MakeVisible(this ToolStripMenuItem item, string name, bool visible = true)
        {
                foreach (ToolStripMenuItem tsi in item.DropDown.Items)
                {
                    if (tsi.Text == name)
                    {
                        tsi.Visible = visible;
                        return;
                    }
                }
        }

        public static void RemoveFromListViewItemCollectionByColumnName(this ListView collection, string column, string value)
        {
                var columnNum = collection.GetColumnNumber(column);
                foreach (ListViewItem item in collection.Items)
                {
                    if (item.SubItems[columnNum].Text == value)
                        collection.Items.Remove(item);
                }
        }

        public static string GetFromListViewItemCollectionByColumnName(this ListViewItem item, string column)
        {
            string ret = string.Empty;

                var columnNum = item.ListView.GetColumnNumber(column);
                ret = item.SubItems[columnNum].Text;
            return ret;
        }

        public static string GetFromListViewAndListViewItemByColumnName(this ListView lv, ListViewItem item,  string column)
        {
            string ret = string.Empty;
                var columnNum = lv.GetColumnNumber(column);
                ret = item.SubItems[columnNum].Text;
            return ret;
        }

        private static void OptimisticTryAndFail(Action act, int timeBetweenTry, int numOfTry, string exception)
        {
            while (numOfTry >= 0)
            {
                try
                {
                    act();
                    return;
                }
                catch (Exception)
                {
                    numOfTry--;
                }
            }
            throw new Exception(exception);
        }
    }

    
}
