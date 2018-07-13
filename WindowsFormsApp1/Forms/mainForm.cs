using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFormsApp1.ServiceReference1;

namespace WindowsFormsApp1
{

    public partial class MainForm : BaseForm, IMainView
    {
        MainFormControler _controler;
        private string _wcfServicesPathId;
        private List<string> fileActions = new List<string> { "Rename", "Delete", "Copy", "Cut", "Paste", "Upload", "Download" };
        private List<string> folderActions = new List<string> { "Enter Directory" };
        private List<string>  _columnNames = new List<string>() { "Type", "Name","Size", "Last file modification" };

    private bool _activated;
        private string _username;
        private bool _shouldChangeCurrentPathText;
        private bool _enableViewModification;

        private Object statusFromServerLock = new Object();
        private Object folderListFromServerLock = new Object();

        private Dictionary<string, Action<ListView, ListViewItem>> _userClickToAction;
        private int _oldTopItemIndex;
        private int _oldSelectedIndex;

        //IMainView Implementation

        //Properties

        string IMainView.CurrentPathTextBoxText
        {
            get
            {
                return currentPathTextBox.Text;
            }
            set
            {
                currentPathTextBox.KeyDown -= CurrentPathTextBox_KeyDown;
                this.Invoke((MethodInvoker)(() => currentPathTextBox.Text = value));
                currentPathTextBox.KeyDown += CurrentPathTextBox_KeyDown;
            }
        }

        int IMainView.FormWidth
        {
            get
            {
                return this.Width;
            }
            set
            {
                Invoke((MethodInvoker)(() =>
                {
                    this.Width = value;
                }));
            }
        }

        Font IMainView.CurrentPathTextBoxFont
        {
            get
            {
                return currentPathTextBox.Font;
            }
            set
            {
                Invoke((MethodInvoker)(() =>
                {
                    currentPathTextBox.Font = value;
                })); 
            }
        }

        bool IMainView.NoSelectedClientLabelVisible
        {
            get
            {
                return NoSelectedClient.Visible;
            }
            set
            {
                Invoke((MethodInvoker)(() =>
                {
                    NoSelectedClient.Visible = value;
                })); 
            }
        }

        bool IMainView.ListViewVisible
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

        int IMainView.ProgressBarValue
        {
            get
            {
                return downloadUploadProgressBar.Value;
            }
            set
            {
                Invoke((MethodInvoker)(() =>
                {
                    downloadUploadProgressBar.Value = value;
                })); 
            }
        }

        string IMainView.ProgressLabelText
        {
            get
            {
                return downloadUploadLable.Text;
            }

            set
            {
                Invoke((MethodInvoker)(() =>
                {
                    downloadUploadLable.Text = value;
                }));
            }
        }

        bool IMainView.ProgressBarVisible
        {
            get
            {
                return downloadUploadProgressBar.Visible;
            }
            set
            {
                Invoke((MethodInvoker)(() =>
                {
                    downloadUploadProgressBar.Visible = value;
                }));
            }
        }

        bool IMainView.ProgressLabelVisible
        {
            get
            {
                return downloadUploadLable.Visible;
            }
            set
            {
                Invoke((MethodInvoker)(() =>
                {
                    downloadUploadLable.Visible = value;
                }));
            }
        }

        bool IMainView.ShouldChangeCurrentPathText
        {
            get
            {
                return _shouldChangeCurrentPathText;
            }
            set
            {
                _shouldChangeCurrentPathText = value;
            }
        }

        bool IMainView.EnableViewModification
        {
            get
            {
                return
                _enableViewModification &&
                detailsToolStripMenuItem.Enabled &&
                smallIconsToolStripMenuItem.Enabled &&
                largIconsToolStripMenuItem.Enabled &&
                enterDirectoryToolStripMenuItem.Enabled &&
                remaneToolStripMenuItem.Enabled &&
                deleteToolStripMenuItem.Enabled &&
                copyToolStripMenuItem.Enabled &&
                cutToolStripMenuItem.Enabled &&
                helpToolStripMenuItem.Enabled;
            }
            set
            {
                Invoke((MethodInvoker)(() =>
                {
                    _enableViewModification = detailsToolStripMenuItem.Enabled = smallIconsToolStripMenuItem.Enabled = largIconsToolStripMenuItem.Enabled = enterDirectoryToolStripMenuItem.Enabled =
                 remaneToolStripMenuItem.Enabled = deleteToolStripMenuItem.Enabled = copyToolStripMenuItem.Enabled = cutToolStripMenuItem.Enabled =
                 helpToolStripMenuItem.Enabled = value;
                }));
            }
        }

        void IMainView.SetController(MainFormControler controller)
        {
            throw new NotImplementedException();
        }

        //Methods

        void IMainView.DisplayMessage(MessageType type, string header, string message)
        {
            DisplayMessage(type, header, message);
        }

        void IMainView.ShowData(List<FileFolder> data)
        {
            listView1.Show(data);
            listView1.FitToData();
            listView1.RestoreSelectedIndex(_oldSelectedIndex);
            listView1.RestoreTopIndex(_oldTopItemIndex);
        }

        //End IMainView Implementation

        private void DisplayMessage(MessageType type, string header, string message)
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

        private void InitializDataStructures()
        {
            InitializeUserClickToAction();
        }

        private void InitializeUserClickToAction()
        {
            _userClickToAction = new Dictionary<string, Action<ListView, ListViewItem>>()
            {
                { "Rename" , ReanmeClickEventHandler},
                { "Delete" , DeleteClickEventHandler},
                { "Copy" , CopyClickEventHandler},
                { "Cut" , CutClickEventHandler},
                { "Paste" , PasteClickEventHandler},
                { "Upload" , UploadClickEventHandler},
                { "Download" , DownloadClickEventHandler},
                { "Enter" , EnterClickEventHandler}
            };
        }

        public MainForm(string id, string username)
        {
            _wcfServicesPathId = id;
            _username = username;

            _controler = new MainFormControler(_wcfServicesPathId, this);
            InitializeComponent();
            InitializDataStructures();
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            
            CreateListView();
        }

        private ContextMenuOvveride CreatePopUpMenu(FileFolderType type)
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
                if (type == FileFolderType.File && userClickActionName == "Enter" ||
                    type == FileFolderType.Folder && userClickActionName == "Download") continue;

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

        private void CreateListView()
        {

            var largeImageData = new ImageData()
            {
                ImageList = new List<Image>()
                {
                    folderImage.Image,
                    fileImage.Image
                },
                imageSize = new Size(64, 64)
            };

            var smallImageData = new ImageData()
            {
                ImageList = new List<Image>()
                {
                    folderImage.Image,
                    fileImage.Image
                },
                imageSize = new Size(16, 16)
            };

            listView1.CreateListView(smallImageData, largeImageData, _columnNames);

        }

        private void SwitchNameColWithFirstCol()
        {

            var nameColIndex = listView1.GetColumnNumber("Name");
            var nameCol = listView1.Columns[nameColIndex];
            var firstCol = listView1.Columns[0];
            listView1.Columns.RemoveAt(nameColIndex);
            listView1.Columns.RemoveAt(0);
            listView1.Columns.Insert(0, nameCol);
            listView1.Columns.Insert(nameColIndex, firstCol);
        }

        private void StatusToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (statusForm status = new statusForm(_wcfServicesPathId))
            {
                status.BringToFront();
                status.ShowDialog();
            }
        }
      
        private void GetStatusFromServer()
        {
            _controler.UpdateUsersStatus();
        }

        private void UpdateViewWithFileFolderList()
        {
            _oldTopItemIndex = listView1.View == View.Details ? listView1.GetdTopItemIndex() : 0;
            _oldSelectedIndex = listView1.GetFocusedIndex();
            _controler.UpdateFileFolderList();
        }

        //Events
        private void MainForm_Activated(object sender, EventArgs e)
        {
            if (_activated) return;
            _activated = true;

            listView1.SizeChanged += (obj, args) => listView1.FitToData();
            currentPathTextBox.GotFocus += CurrentPathTextBox_Enter;
            currentPathTextBox.LostFocus += CurrentPathTextBox_Leave;

            GetStatusFromServer();
            PefromTaskEveryXTime(GetStatusFromServer, 1);
            PefromTaskEveryXTime(UpdateViewWithFileFolderList, 1);
        }

        private void ListView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var selected = listView1.GetSelectedItem();
            if (selected == null) return;

            _controler.ChangeWorkingDirectoryPath(selected.GetFromListViewItemCollectionByColumnName("Name"));
        }

        private void CurrentPathTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;

            _controler.CheckCurrentPathAndChangeWorkingDirectoryIfValid();
        }

        private void CurrentPathTextBox_Leave(object sender, EventArgs e)
        {
            _shouldChangeCurrentPathText = true;
        }

        private void CurrentPathTextBox_Enter(object sender, EventArgs e)
        {
            if (NoSelectedClient.Visible) return;
            _shouldChangeCurrentPathText = false;
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!_controler.Exit(_username)) return;
            Environment.Exit(0);
        }

        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var about = new AboutBox())
            {
                about.ShowDialog();
            }
        }

        private void LogOutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!_controler.Logout(_username)) return;
            this.Close();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            CloseAllThreads();
            _controler.Logout(_username);
        }

        private void DetailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!_enableViewModification) return;

            listView1.View = View.Details;
            var typeColIndex = listView1.GetColumnNumber("Type");
            var typeCol = listView1.Columns[typeColIndex];
            var firstCol = listView1.Columns[0];
            listView1.Columns.RemoveAt(typeColIndex);
            listView1.Columns.RemoveAt(0);
            listView1.Columns.Insert(0, typeCol);
            listView1.Columns.Insert(typeColIndex, firstCol);
        }

        private void SmallIconsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!_enableViewModification) return;

            if (listView1.View == View.SmallIcon) return;
            var needToSwitchCol = listView1.View == View.Details;
            listView1.View = View.SmallIcon;
            if (needToSwitchCol)
                SwitchNameColWithFirstCol();
        }

        private void LargIconsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!_enableViewModification) return;

            if (listView1.View == View.LargeIcon) return;
            var needToSwitchCol = listView1.View == View.Details;
            listView1.View = View.LargeIcon;
            if (needToSwitchCol)
                SwitchNameColWithFirstCol();
        }

        private void ListView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Create hashset of operations
            folderActions.AddRange(fileActions);
            var all = new List<string>();
            all.AddRange(folderActions);
            all.AddRange(fileActions);
            var allActions = new HashSet<string>(all);

            var listview = sender as ListView;
            if (listview == null || listview.SelectedItems.Count <= 0) return;
            var typeColumnNumber = listview.GetColumnNumber("Type");
            var selected = listview.SelectedItems[0];
            var type = (FileFolderType)Enum.Parse(typeof(FileFolderType), selected.SubItems[typeColumnNumber].Text);
            var popUpMenu = CreatePopUpMenu(type);
            listview.ContextMenu = popUpMenu;
            var menuIndex = this.menuStrip1.GetMenuStripItemIndex("Action");
            var actionMenuItem = (ToolStripMenuItem)this.menuStrip1.Items[menuIndex];
            if (type == FileFolderType.File)
            {
                allActions.ToList().ForEach(actionName => actionMenuItem.MakeVisible(actionName, false));
            }
            else
            {
                allActions.ToList().ForEach(actionName => actionMenuItem.MakeVisible(actionName, false));
            }
        }

        private void ListView1_MouseClick(object sender, MouseEventArgs e)
        {
            //Create hashset of operations
            folderActions.AddRange(fileActions);
            var all = new List<string>();
            all.AddRange(folderActions);
            all.AddRange(fileActions);
            var allActions = new HashSet<string>(all);

            var listview = sender as ListView;
            if (e.Button == MouseButtons.Right && listview == null) return;
            var menuActionIndex = this.menuStrip1.GetMenuStripItemIndex("Action");
            var actionMenuItem = (ToolStripMenuItem)this.menuStrip1.Items[menuActionIndex];
            if (listview.ContextMenu != null)
            {
                PopupMenuClosed(listview.ContextMenu, null);
                allActions.ToList().ForEach(actionName => actionMenuItem.MakeVisible(actionName, false));
            }
            if (listview.SelectedItems.Count <= 0) return;
            if (e.Button == MouseButtons.Right)
            {
                var selected = listview.SelectedItems[0];
                var typeColumnNumber = listview.GetColumnNumber("Type");
                var type = (FileFolderType)Enum.Parse(typeof(FileFolderType), selected.SubItems[typeColumnNumber].Text);
                listview.ContextMenu = CreatePopUpMenu(type);
                if (type == FileFolderType.File)
                {
                    allActions.ToList().ForEach(actionName => actionMenuItem.MakeVisible(actionName, false));
                }
                else
                {
                    allActions.ToList().ForEach(actionName => actionMenuItem.MakeVisible(actionName, false));
                }
            }
        }

        //Event handlers

        private void EnterClickEventHandler(ListView selectedListViewProperty, ListViewItem selected)
        {
            var name = selectedListViewProperty.GetFromListViewAndListViewItemByColumnName(selected, "Name");
            _controler.ChangeWorkingDirectoryPath(name);
        }

        private void ReanmeClickEventHandler(ListView selectedListViewProperty, ListViewItem selected)
        {
            var name = selectedListViewProperty.GetFromListViewAndListViewItemByColumnName(selected, "Name");
            string input = Microsoft.VisualBasic.Interaction.InputBox("Enter the new name", "Rename", name, -1, -1);
            _controler.Reanme(name, input);
        }

        private void DeleteClickEventHandler(ListView selectedListViewProperty, ListViewItem selected)
        {
            _controler.Delete(selectedListViewProperty.GetFromListViewAndListViewItemByColumnName(selected, "Name"));
        }

        private void CopyClickEventHandler(ListView selectedListViewProperty, ListViewItem selected)
        {
            _controler.Copy(selectedListViewProperty.GetFromListViewAndListViewItemByColumnName(selected, "Name").ToString());
        }

        private void CutClickEventHandler(ListView selectedListViewProperty, ListViewItem selected)
        {
            _controler.Cut(selectedListViewProperty.GetFromListViewAndListViewItemByColumnName(selected, "Name"));
        }

        private void PasteClickEventHandler(ListView selectedListViewProperty, ListViewItem selected)
        {
            var selectedType = selectedListViewProperty.GetFromListViewAndListViewItemByColumnName(selected, "Type");
            var selectedName = selectedListViewProperty.GetFromListViewAndListViewItemByColumnName(selected, "Name");
            _controler.Paste(selectedType, selectedName);
        }

        private void DownloadClickEventHandler(ListView selectedListViewProperty, ListViewItem selected)
        {
            using (var saveForm = new FolderBrowserDialog())
            {
                var fileName = selectedListViewProperty.GetFromListViewAndListViewItemByColumnName(selected, "Name");
                if (saveForm.ShowDialog() != DialogResult.OK) return;
                RunInAnotherThread(() =>
                {
                    try
                    {
                        var downloadData = new DownloadUpLoadData()
                        {
                            FileName = fileName,
                            PathInServer = currentPathTextBox.Text,
                            PathToSaveInClinet = saveForm.SelectedPath
                        };

                        _controler.DownLoad(downloadData);
                    }

                    catch (Exception e)
                    {
                        DisplayMessage(MessageType.Info, "Download", $"Error in download: {e.Message}");
                    }
                });
            }
        }

        private void UploadClickEventHandler(ListView selectedListViewProperty, ListViewItem selected)
        {
            using (var saveForm = new OpenFileDialog())
            {
                if (saveForm.ShowDialog() != DialogResult.OK) return;
                try
                {
                    var fileName = saveForm.FileName.Split('\\').Last();
                    var path = Path.GetDirectoryName(saveForm.FileName);

                    var uploadData = new DownloadUpLoadData()
                    {
                        FileName = fileName,
                        Directory = path,
                        PathToSaveOnServer = currentPathTextBox.Text
                    };

                    _controler.UpLoadFile(uploadData);
                }

                catch (Exception e)
                {
                    DisplayMessage(MessageType.Error, "Upload", $"Error in upload: {e.Message}");
                }
            }
        }
    }
}
