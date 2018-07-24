using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.ServiceModel;
using System.Windows.Forms;
using WindowsFormsApp1.Controlers;
using WindowsFormsApp1.DataModel.Enums;
using WindowsFormsApp1.Forms;
using WindowsFormsApp1.Interfaces;
using WindowsFormsApp1.ServiceReference1;

namespace WindowsFormsApp1
{
    
    public partial class StatusForm : BaseForm, IStatusView
    {
        private bool _activated;
        private string _wcfServicesPathId;
        private int _oldTopItemIndex;
        private int _oldSelectedIndex;
        private List<string> _columnNames = new List<string>() { "Status", "Id", "Nick name"};
        Dictionary<string, Action<ListView, ListViewItem>> _userClickToAction;
        private StatusFormControler _controler;

        string IStatusView.SelectedClient
        {
            get
            {
                return selectedClientLabel.Text;
            }
            set
            {
                Invoke((MethodInvoker)(() =>
                {
                    selectedClientLabel.Text = value;
                }));
            }
        }

        bool IStatusView.NoSelectedClientLabelVisible
        {
            get
            {
                return noClientConnected.Visible;
            }
            set
            {
                Invoke((MethodInvoker)(() =>
                {
                    noClientConnected.Visible = value;
                }));
            }
        }

        bool IStatusView.ListViewVisible
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

        bool IStatusView.SelectedClientVisible
        {
            get
            {
                return selectedClientLabel.Visible;
            }
            set
            {
                Invoke((MethodInvoker)(() =>
                {
                    selectedClientLabel.Visible = value;
                }));
            }
        }

        public void ShowData(List<IShowable> data)
        {
            listView1.Show(data);
            listView1.FitToData();
            listView1.RestoreSelectedIndex(_oldSelectedIndex);
            listView1.RestoreTopIndex(_oldTopItemIndex);
        }

        void IStatusView.SetController(StatusFormControler controller)
        {
            _controler = controller;
        }

        private void InitializeUserClickToAction()
        {
            _userClickToAction = new Dictionary<string, Action<ListView, ListViewItem>>()
            {
                { "Remove" , RemoveEventHendler},
                { "Show unfinished Shell tasks" , ShowUnfinishedTShellasksEventHendler},
                { "Show unfinished Download or Upload tasks" , ShowUnfinishedDownloadUploadTaskEventHendler},
                { "Set Nick Name" , SetNickNameEventHendler},
                { "Select" , SelectEventHendler},
            };
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

        public StatusForm(string id)
        {
            _wcfServicesPathId = id;
            InitializeComponent();
            InitializeUserClickToAction();
            _controler = new StatusFormControler(id, this);
        }

        private void StatusForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            CloseAllThreads();
        }

        private void UpdateClientStatuses()
        {
            try
            {
                _oldTopItemIndex = listView1.GetdTopItemIndex();
                _oldSelectedIndex = listView1.GetSelectedIndex();
                _controler.UpdateClientStatuses();
            }
            catch { }
        }

        private void CreateListView()
        {

            var largeImageData = new ImageData()
            {
                ImageList = new List<Image>()
                {
                    DisconnectedIcon.Image,
                    ConnectedIcon.Image
                },
                imageSize = new Size(64, 64)
            };

            var smallImageData = new ImageData()
            {
                ImageList = new List<Image>()
                {
                     DisconnectedIcon.Image,
                    ConnectedIcon.Image
                },
                imageSize = new Size(16, 16)
            };

            listView1.CreateListView(smallImageData, largeImageData, _columnNames);

        }

        private void statusForm_Activated(object sender, EventArgs e)
        {
            if (_activated) return;
            _activated = true;
            CreateListView();
            listView1.SizeChanged += (obj, args) => listView1.FitToData();

            UpdateClientStatuses();
            PefromTaskEveryXTime(UpdateClientStatuses, 1);
        }

        private void RemoveEventHendler(ListView selectedListViewProperty, ListViewItem selected)
        {
            var id = selectedListViewProperty.GetFromListViewAndListViewItemByColumnName(selected, "Id");
            _controler.Remove(id);
        }

        private void ShowUnfinishedTShellasksEventHendler(ListView selectedListViewProperty, ListViewItem selected)
        {
            var id = selectedListViewProperty.GetFromListViewAndListViewItemByColumnName(selected, "Id");
            using (TasksForm taskFrom = new ShellTasksForm(id, _wcfServicesPathId))
            {
                taskFrom.ShowDialog();
            }
        }

        private void ShowUnfinishedDownloadUploadTaskEventHendler(ListView selectedListViewProperty, ListViewItem selected)
        {
            var id = selectedListViewProperty.GetFromListViewAndListViewItemByColumnName(selected, "Id");
            using (TasksForm taskFrom = new UploadDownloadTasksForm(id, _wcfServicesPathId))
            {
                taskFrom.ShowDialog();
            }
        }

        private void SetNickNameEventHendler(ListView selectedListViewProperty, ListViewItem selected)
        {
            string nickName = Microsoft.VisualBasic.Interaction.InputBox("Enter the new nick name", "Set Nick Name", "Default", -1, -1);
            var id = selectedListViewProperty.GetFromListViewAndListViewItemByColumnName(selected, "Id");
            _controler.SetNickName(id, nickName);
        }

        private void SelectEventHendler(ListView selectedListViewProperty, ListViewItem selected)
        {
            var id = selectedListViewProperty.GetFromListViewAndListViewItemByColumnName(selected, "Id");
            _controler.SelectRemoteClient(id);
        }

        private ContextMenuOvveride CreatePopUpMenu(StatusImageType type)
        {
            ContextMenuOvveride PopupMenu = new ContextMenuOvveride();
            ListViewItem selected = listView1.SelectedItems[0];
            ListView selectedListViewProperty = selected.ListView;

            List<MenuItemData> menuItemDataList = new List<MenuItemData>();
            foreach (var kv in _userClickToAction)
            {
                var userClickActionName = kv.Key;
                var action = kv.Value;

                //at this time block the download of folder
                if (type == StatusImageType.Off && userClickActionName == "Set Nick Name" ||
                    type == StatusImageType.Off && userClickActionName == "Select") continue;

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

        private void listView1_MouseDown(object sender, MouseEventArgs e)
        {
            var listview = sender as ListView;
            if (e.Button == MouseButtons.Right && (listview == null || listview.SelectedItems.Count <= 0)) return;
            if (listview.ContextMenu != null)
            {
                PopupMenuClosed(listview.ContextMenu, null);
            }
            if (e.Button == MouseButtons.Right)
            {
                var selected = listview.SelectedItems[0];
                var typeColumnNumber = listview.GetColumnNumber("Status");
                var type = (StatusImageType)Enum.Parse(typeof(StatusImageType), selected.SubItems[typeColumnNumber].Text);
                listview.ContextMenu = CreatePopUpMenu(type);
            }
        }

        private void listView1_ItemSelectionChanged_1(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            var listview = sender as ListView;
            PopupMenuClosed(listview.ContextMenu, null);
            if (listview == null || listview.SelectedItems.Count <= 0) return;
            var typeColumnNumber = listview.GetColumnNumber("Status");
            var selected = listview.SelectedItems[0];
            var type = (StatusImageType)Enum.Parse(typeof(StatusImageType), selected.SubItems[typeColumnNumber].Text);
            var popUpMenu = CreatePopUpMenu(type);
            listview.ContextMenu = popUpMenu;
        }
    }
}
