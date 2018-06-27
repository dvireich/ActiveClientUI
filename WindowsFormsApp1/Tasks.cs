using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.ServiceModel;
using System.Windows.Forms;
using WindowsFormsApp1.ServiceReference1;

namespace WindowsFormsApp1
{
    public enum TaskType
    {
        Shell,
        UploadDownload
    }
    public partial class Tasks : Form
    {
        private bool _activated;
        private string _selectedClient;
        static IActiveShell shellService;
        private TaskType _currentType;
        private string _wcfServicesPathId;

        public System.Threading.Timer StatusTimer { get; private set; }
        public string SelectedClient { get => _selectedClient; set => _selectedClient = value; }

        private void CloseAllConnections()
        {
            if (shellService != null)
                ((ICommunicationObject)shellService).Close();
        }

        public Tasks(TaskType type, string client, string id)
        {
            _wcfServicesPathId = id;
            _currentType = type;
            SelectedClient = client;
            InitializeComponent();
            CreateListView();
            initializeServiceReferences(_wcfServicesPathId);
        }

        

        Object statusFromServerLock = new Object();
        private void GetStatusFromServer()
        {
            lock (statusFromServerLock)
            {
                var status = shellService.GetStatus();
                status = status.Replace("\r", "");
                var statusSplittedNewLine = status.Split('\n');
                statusSplittedNewLine = statusSplittedNewLine.Where(str => !string.IsNullOrEmpty(str) && !string.IsNullOrWhiteSpace(str)).ToArray();
                if (statusSplittedNewLine.Length == 2 && statusSplittedNewLine[1] == "There is no clients connected")
                {
                    this.Invoke((MethodInvoker)(() =>
                    {
                        NoTasks.Visible = true;
                        listView1.Visible = false;
                        NoTasks.Visible = false;
                    }));
                    return;
                }
                var clients = status.Split(new string[] { "Client" }, StringSplitOptions.RemoveEmptyEntries);
                clients = clients.ToArray().Skip(1).Take(clients.Count() - 2).ToArray();
                var index = 0;
                foreach (var client in clients)
                {
                    var clientId = client.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries).ElementAt(1).Split('\t').First().Trim();
                    if (clientId != SelectedClient) continue;
                    var fields = client.Replace('\t', '\n').Split('\n');
                    fields = fields.Where(str => !string.IsNullOrEmpty(str) && !string.IsNullOrWhiteSpace(str) && str != "The selected ").ToArray();

                    var id = fields[0].Split(':').Last();
                    var nickName = fields[1].Split(':').Last();
                    var isAlive = bool.Parse(fields[2].Split(':').Last()) ? Status.On : Status.Off;
                    var shellTasks = new List<string>();
                    var i = 4;
                    while (fields[i] != "Upload And Download Tasks:")
                    {
                        if (fields[i] == "There is no shell tasks")
                        {
                            i++;
                            break;
                        }
                        else if(fields[i].StartsWith("Task Number:"))
                        {
                            i++;
                            continue;
                        }

                        shellTasks.Add(fields[i]);
                        i++;
                    }
                    var downloadUploadTasks = new List<string>();
                    i++;
                    while (i < fields.Count())
                    {
                        if (fields[i] == "There is no Download or Upload tasks")
                        {
                            i++;
                            break;
                        }
                        else if (fields[i].StartsWith("Task Number:"))
                        {
                            i++;
                            continue;
                        }

                        downloadUploadTasks.Add(fields[i]);
                        i++;
                    }

                    this.Invoke((MethodInvoker)(() =>
                    {
                        NoTasks.Visible = false;
                        listView1.Visible = true;
                        var oldFocusedIndex = listView1 != null ?
                                                        listView1.SelectedItems != null ?
                                                                    listView1.SelectedItems.Count > 0 ?
                                                                                    listView1.SelectedItems[0].Index :
                                                                                    -1 :
                                                                                    -1 :
                                                                                    -1;
                        listView1.SuspendLayout();
                        listView1.BeginUpdate();
                        listView1.Items.Clear();
                        foreach (var task in _currentType == TaskType.Shell ? shellTasks : downloadUploadTasks)
                        {
                            if(_currentType == TaskType.Shell)
                            {
                                var splited = task.Split(' ');
                                var splitedList = splited.ToList();
                                if (splitedList.First() == "Download" || splitedList.First() == "Upload")
                                    continue;
                                if(splitedList.First() != "Run")
                                    splitedList.RemoveAt(0);
                                var command = splitedList.First();
                                splitedList.RemoveAt(0);
                                var args = string.Join(" ", splitedList);
                                if(!string.IsNullOrEmpty(command))
                                    AddToListViewShell(index, command, args);
                                index++;
                            }
                            else if (_currentType == TaskType.UploadDownload)
                            {
                                var splited = task.Split(' ');
                                var splitedList = splited.ToList();
                                if (splitedList.First() != "Download" && splitedList.First() != "Upload")
                                    continue;
                                var command = splitedList.First();
                                var filenName = splitedList.ElementAt(1);
                                var path = splitedList.ElementAt(2);
                                AddToListViewUploadDownload(index, command, filenName, path);
                                index++;
                            }
                        }
                        if (oldFocusedIndex > -1 && listView1 != null && listView1.Items != null && oldFocusedIndex < listView1.Items.Count)
                        {
                            listView1.Items[oldFocusedIndex].Selected = true;
                        }
                        else if (listView1 != null && listView1.Items != null && listView1.Items.Count > 0)
                        {
                            listView1.Items[0].Selected = true;
                        }
                        listView1.EndUpdate();
                        listView1.ResumeLayout();
                        listView1.Refresh();
                    }));
                }
            }
        }

        private void AddToListViewUploadDownload(int index, string command, string fileName, string path, bool check = false)
        {
            //listView1.BeginUpdate();
            // Create three items and three sets of subitems for each item.
            ListViewItem item1 = new ListViewItem(command, index);
            // Place a check mark next to the item.
            item1.Checked = check;
            item1.SubItems.Add(fileName);
            item1.SubItems.Add(path);
            listView1.Items.Add(item1);
            //listView1.EndUpdate();
            //listView1.Refresh();
        }

        private void AddToListViewShell(int index, string command, string args, bool check = false)
        {
            //listView1.BeginUpdate();
            // Create three items and three sets of subitems for each item.
            ListViewItem item1 = new ListViewItem(command, index);
            // Place a check mark next to the item.
            item1.Checked = check;
            item1.SubItems.Add(args);
            listView1.Items.Add(item1);
            //listView1.EndUpdate();
            //listView1.Refresh();
        }

        private static void initializeServiceReferences(string wcfServicesPathId)
        {
            //Confuguring the Shell service
            var shellBinding = new BasicHttpBinding();
            shellBinding.Security.Mode = BasicHttpSecurityMode.None;
            shellBinding.CloseTimeout = TimeSpan.MaxValue;
            shellBinding.ReceiveTimeout = TimeSpan.MaxValue;
            shellBinding.SendTimeout = new TimeSpan(0, 0, 10, 0, 0);
            shellBinding.OpenTimeout = TimeSpan.MaxValue;
            shellBinding.MaxReceivedMessageSize = int.MaxValue;
            shellBinding.MaxBufferPoolSize = int.MaxValue;
            shellBinding.MaxBufferSize = int.MaxValue;
            //Put Public ip of the server copmuter
            var shellAdress = string.Format("http://localhost:80/ShellTrasferServer/ActiveShell/{0}", wcfServicesPathId);
            var shellUri = new Uri(shellAdress);
            var shellEndpointAddress = new EndpointAddress(shellUri);
            var shellChannel = new ChannelFactory<IActiveShell>(shellBinding, shellEndpointAddress);
            shellService = shellChannel.CreateChannel();
        }

        private void PefromTaskEveryXTime(Action task, int seconds)
        {
            var startTimeSpan = TimeSpan.Zero;
            var periodTimeSpan = TimeSpan.FromSeconds(seconds);

            StatusTimer = new System.Threading.Timer((e) =>
            {
                task();
            }, null, startTimeSpan, periodTimeSpan);
        }

        private void CreateListView()
        {
            listView1.View = View.Details;
            // Allow the user to edit item text.
            listView1.LabelEdit = true;
            // Allow the user to rearrange columns.
            listView1.AllowColumnReorder = false;
            // Display check boxes.
            listView1.CheckBoxes = false;
            // Select the item and subitems when selection is made.
            listView1.FullRowSelect = true;
            // Display grid lines.
            listView1.GridLines = true;
            // Sort the items in the list in ascending order.
            listView1.Sorting = SortOrder.None;


            // Create columns for the items and subitems.
            // Width of -2 indicates auto-size.
            if(_currentType == TaskType.UploadDownload)
            {
                listView1.Columns.Add("Action Type", -2, HorizontalAlignment.Left);
                listView1.Columns.Add("File name", -2, HorizontalAlignment.Left);
                listView1.Columns.Add("Path", -2, HorizontalAlignment.Center);
            }
            else if (_currentType == TaskType.Shell)
            {
                listView1.Columns.Add("Command", -2, HorizontalAlignment.Left);
                listView1.Columns.Add("arguments", -2, HorizontalAlignment.Center);
            }

        }

        private ContextMenuOvveride CreatePopUpMenu(TaskType type)
        {
            ContextMenuOvveride PopupMenu = new ContextMenuOvveride();
            ListViewItem selected = listView1.SelectedItems[0];
            ListView selectedListViewProperty = selected.ListView;
            var src = type.ToString().ToLower();
            PopupMenu.AddMenuItem("Remove",
                                (obj, args) =>
                                {
                                    var succeeded = shellService.DeleteClientTask(_selectedClient, _currentType == TaskType.Shell ? true : false, selected.Index + 1);
                                    if (!succeeded)
                                        MessageBox.Show("Error delete task", "Delete task", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                },
                                string.Format("Delete this task"));

            PopupMenu.ClickThenCollapse += PopupMenuClosed;
            return PopupMenu;
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
            StatusTimer.Dispose();
            ((ICommunicationObject)shellService).Close();
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
            GetStatusFromServer();
            PefromTaskEveryXTime(GetStatusFromServer, 5);
        }

        private void Tasks_SizeChanged(object sender, EventArgs e)
        {
            for (var i = 0; i < listView1.Columns.Count; i++)
            {
                listView1.AutoResizeColumn(i, ColumnHeaderAutoResizeStyle.HeaderSize);
            }
        }
    }
}
