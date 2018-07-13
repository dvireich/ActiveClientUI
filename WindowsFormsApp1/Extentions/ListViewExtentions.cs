using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    static class ListViewExtentions
    {
        public static void AddMenuItem(this ContextMenuOvveride cm, string header, EventHandler onClick, string tooltip = default(string))
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
        }

        public static int GetColumnNumber(this ListView listview, string columnHeader)
        {
            return listview.RunInInvoke<int>(() =>
            {
                foreach (ColumnHeader column in listview.Columns)
                {
                    if (column.Text == columnHeader)
                    {
                        return column.Index;
                    }
                }
                return -1;
            });
            
        }

        public static int GetMenuStripItemIndex(this MenuStrip menu, string name)
        {
            var index = 0;
            foreach (ToolStripItem item in menu.Items)
            {
                if (item.Text == name)
                {
                    return index;
                }
                index++;
            }
            return -1;
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
            collection.RunInInvoke(() =>
            {
                var columnNum = collection.GetColumnNumber(column);
                foreach (ListViewItem item in collection.Items)
                {
                    if (item.SubItems[columnNum].Text == value)
                        collection.Items.Remove(item);
                }
            });
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
            return lv.RunInInvoke<string>(() =>
            {
                string ret = string.Empty;
                var columnNum = lv.GetColumnNumber(column);
                ret = item.SubItems[columnNum].Text;
                return ret;
            });
            
        }

        public static void CreateListView(this ListView listView, ImageData smallImagData, ImageData largeImageData, List<string> colNames)
        {
            listView.RunInInvoke(() =>
            {
                //Must start with View.Details. If want to switch state then use the relevat function
                listView.View = View.Details;
                // Allow the user to edit item text.
                listView.LabelEdit = true;
                // Allow the user to rearrange columns.
                listView.AllowColumnReorder = false;
                // Display check boxes.
                listView.CheckBoxes = false;
                // Select the item and subitems when selection is made.
                listView.FullRowSelect = true;
                // Display grid lines.
                listView.GridLines = true;
                // Sort the items in the list in ascending order.
                listView.Sorting = SortOrder.Ascending;

                // Create columns for the items and subitems.
                if (colNames != null && colNames.Count > 0)
                {
                    listView.Columns[0].Text = colNames[0];

                    // Width of -2 indicates auto-size.
                    for (int i = 1; i < colNames.Count - 1; i++)
                    {
                        listView.Columns.Add(colNames[i], -2, HorizontalAlignment.Left);
                    }

                    listView.Columns.Add(colNames.Last(), -2, HorizontalAlignment.Left);
                }

                ImageList imageListSmall = null;
                if (smallImagData != null)
                {
                    imageListSmall = new ImageList();
                    imageListSmall.ImageSize = smallImagData.imageSize;
                    foreach (var image in smallImagData.ImageList)
                    {
                        imageListSmall.Images.Add(image);
                    }
                }

                ImageList imageListLarge = null;
                if (largeImageData != null)
                {
                    imageListLarge = new ImageList();
                    imageListLarge.ImageSize = largeImageData.imageSize;
                    foreach (var image in largeImageData.ImageList)
                    {
                        imageListLarge.Images.Add(image);
                    }
                }

                //Assign the ImageList objects to the ListView.
                listView.LargeImageList = imageListLarge;
                listView.SmallImageList = imageListSmall;
            });
        }

        public static void AddToListView(this ListView listView, ListViewItemData listViewItemData)
        {
            listView.RunInInvoke(() =>
            {
                listView.BeginUpdate();
                listView.SuspendLayout();
                // Create three items and three sets of subitems for each item.
                ListViewItem item1 = new ListViewItem(listViewItemData.MainItem, listViewItemData.Index);
                // Place a check mark next to the item.
                item1.Checked = listViewItemData.check;

                foreach (var subItem in listViewItemData.SubItems)
                {
                    item1.SubItems.Add(subItem);
                }

                item1.ImageIndex = listViewItemData.imageIndex;
                listView.Items.Add(item1);
                listView.ResumeLayout();
                listView.EndUpdate();
                listView.Refresh();
            });
        }

        public static int GetFocusedIndex(this ListView listview)
        {
            return listview.RunInInvoke<int>(() =>
            {
                return listview != null ?
                 listview.SelectedItems != null ?
                          listview.SelectedItems.Count > 0 ?
                                   listview.SelectedItems[0].Index :
                                             -1 :
                                             -1 :
                                             -1;
            });
        }

        public static int GetdTopItemIndex(this ListView listview)
        {
            return listview.RunInInvoke(() =>
            {
                return listview.TopItem == null ? 0 : listview.TopItem.Index;
            }); 
        }

        public static void Show(this ListView listview, List<FileFolder> ffl)
        {
            listview.RunInInvoke(() =>
            {
                listview.SuspendLayout();
                listview.BeginUpdate();
                listview.Items.Clear();

                for (int i = 0; i < ffl.Count; i++)
                {
                    var ff = ffl[i];
                    var listViewItemData = new ListViewItemData()
                    {
                        Index = i,
                        imageIndex = (int)ff.GetType(),
                        MainItem = listview.View != View.Details ? ff.GetName() : ff.GetType().ToString(),
                        SubItems = new List<string>(){
                                                listview.View == View.Details ? ff.GetName() : ff.GetType().ToString(),
                                                ff.GetType() == FileFolderType.Folder ? string.Empty : ff.getSize().ToString(),
                                                ff.GetLastModificationDate()}
                    };

                    listview.AddToListView(listViewItemData);
                }

                listview.EndUpdate();
                listview.ResumeLayout();
            });
        }

        public static void FitToData(this ListView listview)
        {
            listview.RunInInvoke(() =>
            {
                if (listview.View != View.Details) return;
                for (var i = 0; i < listview.Columns.Count; i++)
                {
                    listview.AutoResizeColumn(i, ColumnHeaderAutoResizeStyle.ColumnContent);
                    listview.AutoResizeColumn(i, ColumnHeaderAutoResizeStyle.HeaderSize);
                }
            }); 
        }

        public static void RestoreSelectedIndex(this ListView listview, int index)
        {
            listview.RunInInvoke(() =>
            {
                try
                {
                    listview.Items[index].Selected = true;
                }
                catch { }
            });
        }

        public static void RestoreTopIndex(this ListView listview, int index)
        {
            if (listview.View != View.Details) return;
            listview.RunInInvoke(() =>
            {
                try
                {
                    listview.TopItem = listview.Items[index];
                }
                catch { }
            });
        }

        private static void RunInInvoke(this ListView listview, Action act)
        {
            listview.Invoke(((MethodInvoker)(() =>
                {
                    act();
                })));
        }

        private static T RunInInvoke<T>(this ListView listview, Func<T> act)
        {
            T res = default(T);
            listview.Invoke(((MethodInvoker)(() =>
            {
                res = act();
            })));
            return res;
        }

        public static ListViewItem GetSelectedItem(this ListView listview)
        {
            if (listview.SelectedItems.Count == 0) return null;
            return listview.SelectedItems[0];
        }
    }

    
}
