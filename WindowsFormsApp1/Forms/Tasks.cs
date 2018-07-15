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
    public partial class Tasks : BaseForm, ITaskView
    {
        private bool _activated;
        private TaskType _currentType;
        private int _oldSelectedIndex;
        private int _oldTopItemIndex;
        private TaskFormControler _controler;

        private List<string> _uploadDownloadColumns = new List<string>() { "Action Type", "File name", "Path" };
        private List<string> _shellColumns = new List<string>() { "Command", "Arguments"};
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

        public Tasks(TaskType type, string client, string id)
        {
            _currentType = type;
            InitializeComponent();
            InitializeUserClickToAction();
            _controler = new TaskFormControler(type, client, id, this);
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

        private void CreateListView(TaskType type)
        {
            var colNames = type == TaskType.Shell ? _shellColumns : _uploadDownloadColumns;
            listView1.CreateListView(null, null, colNames);
        }

        private void RemoveEventHandler(ListView selectedListViewProperty, ListViewItem selected)
        {
            _controler.Remove(selected.Index);
        }

        private ContextMenuOvveride CreatePopUpMenu(TaskType type)
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
            var popUpMenu = sender as ContextMenuOvveride;
            if (popUpMenu == null) return;
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
                listview.ContextMenu = CreatePopUpMenu(_currentType);
            }
        }

        private void listView1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            var listview = sender as ListView;
            PopupMenuClosed(listview.ContextMenu, null);
            if (listview == null || listview.SelectedItems.Count <= 0) return;
            var popUpMenu = CreatePopUpMenu(_currentType);
            listview.ContextMenu = popUpMenu;
        }

        private void Tasks_Activated(object sender, EventArgs e)
        {
            if (_activated) return;
            _activated = true;
            CreateListView(_currentType);
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
