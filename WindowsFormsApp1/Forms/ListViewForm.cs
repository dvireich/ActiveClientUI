using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public class ListViewForm : DownloadUploadForm
    {
        public void AddMenuItem(ContextMenuOvveride menu, MenuItemData menuItemData)
        {
            EventHandler onClick = (obj, args) => menuItemData.OnClick(menuItemData.SelectedListViewProperty, menuItemData.SelectedListViewItem);
            menu.AddMenuItem(menuItemData.Header, onClick, menuItemData.ToolTip);
        }

        public ContextMenuOvveride CreatePopUpMenu(List<MenuItemData> menuItemDataList)
        {
            ContextMenuOvveride PopupMenu = new ContextMenuOvveride();
            menuItemDataList.ForEach(menuItemData => AddMenuItem(PopupMenu, menuItemData));

            PopupMenu.ClickThenCollapse += PopupMenuClosed;
            return PopupMenu;
        }

        public void CreateListView(ListView listView, ImageData smallImagData, ImageData largeImageData , List<string> colNames)
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
                for (int i = 1; i < colNames.Count -1; i++)
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
        }

        public void AddToListView(ListView listView, ListViewItemData listViewItemData)
        {
            listView.BeginUpdate();
            listView.SuspendLayout();
            // Create three items and three sets of subitems for each item.
            ListViewItem item1 = new ListViewItem(listViewItemData.MainItem, listViewItemData.Index);
            // Place a check mark next to the item.
            item1.Checked = listViewItemData.check;

            foreach(var subItem in listViewItemData.SubItems)
            {
                item1.SubItems.Add(subItem);
            }
            
            item1.ImageIndex = listViewItemData.imageIndex;
            listView.Items.Add(item1);
            listView.ResumeLayout();
            listView.EndUpdate();
            listView.Refresh();
        }

        public void PopupMenuClosed(object sender, EventArgs e)
        {
            var popUpMenu = sender as ContextMenuOvveride;
            popUpMenu.Collapse -= PopupMenuClosed;
            while (popUpMenu.MenuItems.Count > 0)
            {
                var mi = popUpMenu.MenuItems[0];
                mi.Dispose();
            }
            popUpMenu.Dispose();
        }
    }
}
