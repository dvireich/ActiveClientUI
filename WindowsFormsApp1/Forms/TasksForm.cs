using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.ServiceModel;
using System.Windows.Forms;
using WindowsFormsApp1.Controlers;
using WindowsFormsApp1.DataModel.Enums;
using WindowsFormsApp1.Interfaces;
using WindowsFormsApp1.ServiceReference1;

namespace WindowsFormsApp1
{
    public partial class TasksForm : BaseForm, ITaskView
    {
        private bool _activated;
        //private TaskType _currentType;
        private int _oldSelectedIndex;
        private int _oldTopItemIndex;
        protected TaskFormControler _controler;
        protected List<string> ColumnsName { get; set; }

        private Dictionary<string, Action<ListView, ListViewItem>> _userClickToAction;

        public bool NoTasksVisible
        {
            get
            {
                return NoTasks.Visible;
            }
            set
            {
                Invoke((MethodInvoker)(() =>
                {
                    NoTasks.Visible = value;
                }));
            }
        }

        public bool ListViewVisible
        {
            get
            {
                return listView1.Visible;
            }
            set
            {
                Invoke((MethodInvoker)(() =>
                {
                    listView1.Visible = value;
                }));
            }
        }

        public void ShowData(List<IShowable> data)
        {
            listView1.Show(data);
            //listView1.FitToData();
            listView1.RestoreSelectedIndex(_oldSelectedIndex);
            listView1.RestoreTopIndex(_oldTopItemIndex);
        }

        protected TasksForm(string client, string id)
        {
            InitializeComponent();
            InitializeUserClickToAction();
        }

        private void InitializeUserClickToAction()
        {
            _userClickToAction = new Dictionary<string, Action<ListView, ListViewItem>>()
            {
                { "Remove" , RemoveEventHandler}
            };
        }

        private void UpdateTaskData()
        {
            try
            {
                _oldSelectedIndex = listView1.GetSelectedIndex();
                _oldTopItemIndex = listView1.GetdTopItemIndex();
                _controler.UpdateTaskData();
            }
            catch { }
        }

        private void CreateListView()
        {
            listView1.CreateListView(null, null, ColumnsName);
        }

        private void RemoveEventHandler(ListView selectedListViewProperty, ListViewItem selected)
        {
            _controler.Remove(selected.Index);
        }

        private ContextMenuOvveride CreatePopUpMenu()
        {
            ContextMenuOvveride PopupMenu = new ContextMenuOvveride();
            ListViewItem selected = listView1.SelectedItems[0];
            ListView selectedListViewProperty = selected.ListView;

            List<MenuItemData> menuItemDataList = new List<MenuItemData>();
            foreach (var kv in _userClickToAction)
            {
                var userClickActionName = kv.Key;
                var action = kv.Value;

                var menuItemData = new MenuItemData()
                {
                    Header = userClickActionName,
                    OnClick = action,
                    SelectedListViewItem = selected,
                    SelectedListViewProperty = selectedListViewProperty,
                };

                menuItemDataList.Add(menuItemData);
            }

            return menuItemDataList.CreatePopUpMenu(PopupMenuClosed);
        }

        private void PopupMenuClosed(object sender, EventArgs e)
        {
            if (!(sender is ContextMenuOvveride popUpMenu)) return;
            popUpMenu.ClickThenCollapse -= PopupMenuClosed;
            while (popUpMenu.MenuItems.Count > 0)
            {
                var mi = popUpMenu.MenuItems[0];
                mi.Dispose();
            }
            popUpMenu.Dispose();
        }

        private void Tasks_FormClosing(object sender, FormClosingEventArgs e)
        {
            CloseAllThreads();
        }

        private void listView1_MouseDown_1(object sender, MouseEventArgs e)
        {
            var listview = sender as ListView;
            if (e.Button == MouseButtons.Right && (listview == null || listview.SelectedItems.Count <= 0)) return;
            if (listview.ContextMenu != null)
            {
                PopupMenuClosed(listview.ContextMenu, null);
            }
            if (e.Button == MouseButtons.Right)
            {
                listview.ContextMenu = CreatePopUpMenu();
            }
        }

        private void listView1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            var listview = sender as ListView;
            PopupMenuClosed(listview.ContextMenu, null);
            if (listview == null || listview.SelectedItems.Count <= 0) return;
            var popUpMenu = CreatePopUpMenu();
            listview.ContextMenu = popUpMenu;
        }

        private void Tasks_Activated(object sender, EventArgs e)
        {
            if (_activated) return;
            _activated = true;
            CreateListView();
            listView1.SizeChanged += (obj, args) => listView1.FitToData();
            UpdateTaskData();
            PefromTaskEveryXTime(UpdateTaskData, 1);
        }

        public void DisplayMessage(MessageType type, string header, string message)
        {
            switch (type)
            {
                case MessageType.Error:
                    MessageBox.Show(message, header, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case MessageType.Info:
                    MessageBox.Show(message, header, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                case MessageType.Question:
                    MessageBox.Show(message, header, MessageBoxButtons.OK, MessageBoxIcon.Question);
                    break;
            }
        }
    }
}
