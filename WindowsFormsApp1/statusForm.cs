using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFormsApp1.ServiceReference1;

namespace WindowsFormsApp1
{
    public enum Status
    {
        Off,
        On
    }
    public partial class statusForm : Form
    {
        private bool _activated;
        private string _selectedClient;
        static IActiveShell shellService;
        private string _wcfServicesPathId;

        public System.Threading.Timer StatusTimer { get; private set; }
        public string SelectedClient { get => _selectedClient; set => _selectedClient = value; }

        private void CloseAllConnections()
        {
            if (shellService != null)
                ((ICommunicationObject)shellService).Close();
        }

        public statusForm(string id)
        {
            _wcfServicesPathId = id;
            InitializeComponent();
            CreateListView();
            initializeServiceReferences(_wcfServicesPathId);
            listView1.SizeChanged += new EventHandler(ListView_SizeChanged);
            this.FormClosing += StatusForm_FormClosing;  
        }

        private void StatusForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            StatusTimer.Dispose();
            ((ICommunicationObject)shellService).Close();
        }

        Object statusFromServerLock = new Object();
        private void GetStatusFromServer()
        {
            lock (statusFromServerLock)
            {
                //this.Invoke((MethodInvoker)(() =>  listView1.Items.Clear()));
                var status = shellService.GetStatus();
                status = status.Replace("\r", "");
                var statusSplittedNewLine = status.Split('\n');
                statusSplittedNewLine = statusSplittedNewLine.Where(str => !string.IsNullOrEmpty(str) && !string.IsNullOrWhiteSpace(str)).ToArray();
                if (statusSplittedNewLine.Length == 2 && statusSplittedNewLine[1] == "There is no clients connected")
                {
                    this.Invoke((MethodInvoker)(() =>
                    {
                        noClientConnected.Visible = true;
                        listView1.Visible = false;
                        selectedClientLabel.Visible = false;
                    }));
                    return;
                }
                int oldFocusedIndex = -1;
                this.Invoke((MethodInvoker)(() =>
                {
                    try
                    {
                        oldFocusedIndex = listView1 != null ?
                                                                                listView1.SelectedItems != null ?
                                                                                            listView1.SelectedItems.Count > 0 ?
                                                                                                            listView1.SelectedItems[0].Index :
                                                                                                            -1 :
                                                                                                            -1 :
                                                                                                            -1;
                        listView1.SuspendLayout();
                        listView1.BeginUpdate();
                        selectedClientLabel.Visible = true;
                        selectedClientLabel.Text = string.Format("Selected client: {0}", statusSplittedNewLine.Last().Split(':').Last());
                        listView1.Items.Clear();
                    }
                    catch(Exception)
                    {
                        return;
                    }
                    
                }));
                SelectedClient = statusSplittedNewLine.Last().Split(':').Last();
                var clients = status.Split(new string[] { "Client" }, StringSplitOptions.RemoveEmptyEntries);
                clients = clients.ToArray().Skip(1).Take(clients.Count() - 2).ToArray();
                var index = 0;
                foreach(var client in clients)
                {
                    var fields = client.Replace('\t', '\n').Split('\n');
                    fields = fields.Where(str => !string.IsNullOrEmpty(str) && !string.IsNullOrWhiteSpace(str) && str!= "The selected ").ToArray();

                    var id = fields[0].Split(':').Last();
                    var nickName = fields[1].Split(':').Last();
                    var isAlive = bool.Parse(fields[2].Split(':').Last()) ? Status.On : Status.Off;
                    var shellTasks = new List<string>();
                    var i = 4;
                    while(fields[i] != "Upload And Download Tasks:")
                    {
                        if (fields[i] == "There is no shell tasks")
                        {
                            i++;
                            break;
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
                           
                        shellTasks.Add(fields[i]);
                        i++;
                    }
                    this.Invoke((MethodInvoker)(() =>
                    {
                        noClientConnected.Visible = false;
                        listView1.Visible = true;
                        AddToListView(index, id.Trim(), isAlive ,nickName);
                    }));
                    index++;
                }
                this.Invoke((MethodInvoker)(() =>
                {
                    listView1.ItemSelectionChanged -= this.listView1_ItemSelectionChanged_1;
                    if (oldFocusedIndex > -1 && listView1 != null && listView1.Items != null && oldFocusedIndex < listView1.Items.Count)
                    {
                        listView1.Items[oldFocusedIndex].Selected = true;
                    }
                    else if (listView1 != null && listView1.Items != null && listView1.Items.Count > 0)
                    {
                        listView1.Items[0].Selected = true;
                    }
                    listView1.ItemSelectionChanged += this.listView1_ItemSelectionChanged_1;
                    listView1.EndUpdate();
                    listView1.ResumeLayout();
                    listView1.Refresh();
                }
                ));
            }
        }

        private void AddToListView(int index, string id, Status status, string nickName, bool check = false)
        {
            listView1.BeginUpdate();
            // Create three items and three sets of subitems for each item.
            ListViewItem item1 = new ListViewItem(status.ToString(), index);
            // Place a check mark next to the item.
            item1.Checked = check;
            item1.SubItems.Add(id);
            item1.SubItems.Add(nickName);
            item1.ImageIndex = (int)status;
            listView1.Items.Add(item1);
            listView1.EndUpdate();
            listView1.Refresh();
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
            listView1.Sorting = SortOrder.Ascending;


            // Create columns for the items and subitems.
            // Width of -2 indicates auto-size.
            listView1.Columns.Add("Status", -2, HorizontalAlignment.Left);
            listView1.Columns.Add("Id", -2, HorizontalAlignment.Left);
            listView1.Columns.Add("Nick Name", -2, HorizontalAlignment.Center);
            

            // Create two ImageList objects.
            ImageList imageListSmall = new ImageList();
            ImageList imageListLarge = new ImageList();

            imageListSmall.Images.Add(DisconnectedIcon.Image);
            imageListSmall.Images.Add(ConnectedIcon.Image);
            
            imageListLarge.Images.Add(DisconnectedIcon.Image);
            imageListLarge.Images.Add(ConnectedIcon.Image);

            //Assign the ImageList objects to the ListView.
            listView1.LargeImageList = imageListLarge;
            listView1.SmallImageList = imageListSmall;
        }

        private void ListView_SizeChanged(object sender, EventArgs e)
        {
            for (var i = 0; i < listView1.Columns.Count; i++)
            {
                listView1.AutoResizeColumn(i, ColumnHeaderAutoResizeStyle.HeaderSize);
            }
        }

        private void statusForm_Activated(object sender, EventArgs e)
        {
            if (_activated) return;
            _activated = true;
            GetStatusFromServer();
            PefromTaskEveryXTime(GetStatusFromServer, 1);
        }

        private ContextMenuOvveride CreatePopUpMenu(Status type)
        {
            ContextMenuOvveride PopupMenu = new ContextMenuOvveride();
            ListViewItem selected = listView1.SelectedItems[0];
            ListView selectedListViewProperty = selected.ListView;
            var src = type.ToString().ToLower();
            if (type == Status.Off)
            {
                PopupMenu.AddMenuItem("Remove",
                                    (obj, args) =>
                                    {
                                        var id = selectedListViewProperty.GetFromListViewAndListViewItemByColumnName(selected, "Id");
                                        var succeeded = shellService.ActiveCloseClient(id);
                                        if (!succeeded)
                                            MessageBox.Show("Error closing client", "Remove client", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    },
                                    string.Format("Copy this Directory to another place", src));
                PopupMenu.AddMenuItem("Show unfinished Shell tasks",
                                       (obj, args) =>
                                       {
                                           var id = selectedListViewProperty.GetFromListViewAndListViewItemByColumnName(selected, "Id");
                                           using (Tasks taskFrom = new Tasks(TaskType.Shell, id, _wcfServicesPathId))
                                           {
                                               taskFrom.ShowDialog();
                                           } 
                                       },
                                       string.Format("Delete this Directory", src));
                PopupMenu.AddMenuItem("Show unfinished Download\\Upload tasks",
                                        (obj, args) =>
                                        {
                                            var id = selectedListViewProperty.GetFromListViewAndListViewItemByColumnName(selected, "Id");
                                            using (Tasks taskFrom = new Tasks(TaskType.UploadDownload, id, _wcfServicesPathId))
                                            {
                                                taskFrom.ShowDialog();
                                            } 
                                        },
                                        string.Format("Delete this Directory", src));

            }
            else
            {
                PopupMenu.AddMenuItem("Remove",
                                    (obj, args) =>
                                    {
                                        var id = selectedListViewProperty.GetFromListViewAndListViewItemByColumnName(selected, "Id");
                                        var succeeded = shellService.ActiveCloseClient(id);
                                        if (!succeeded)
                                            MessageBox.Show("Error closing client", "Remove client", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    },
                                    string.Format("Copy this Directory to another place", src));

                PopupMenu.AddMenuItem("Set Nick Name",
                                        (obj, args) =>
                                        {
                                            string input = Microsoft.VisualBasic.Interaction.InputBox("Enter the new nick name", "Set Nick Name", "Default", -1, -1);
                                            var id = selectedListViewProperty.GetFromListViewAndListViewItemByColumnName(selected, "Id");
                                            var succeeded = shellService.ActiveSetNickName(id, input);
                                            if (!succeeded)
                                                MessageBox.Show("Error set nickName", "set nickName", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        },
                                        string.Format("Change the name of this {0}", src));
                PopupMenu.AddMenuItem("Select",
                                        (obj, args) =>
                                        {
                                            var id = selectedListViewProperty.GetFromListViewAndListViewItemByColumnName(selected, "Id");
                                            var succeeded = shellService.SelectClient(id);
                                            if (!succeeded)
                                                MessageBox.Show("Error selecting client", "Select client", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        },
                                        string.Format("Delete this Directory", src));
                PopupMenu.AddMenuItem("Show unfinished Shell tasks",
                                        (obj, args) =>
                                        {
                                            var id = selectedListViewProperty.GetFromListViewAndListViewItemByColumnName(selected, "Id");
                                            using (Tasks taskFrom = new Tasks(TaskType.Shell, id, _wcfServicesPathId))
                                            {
                                                taskFrom.ShowDialog();
                                            }
                                        },
                                        string.Format("Delete this Directory", src));
                PopupMenu.AddMenuItem("Show unfinished Download\\Upload tasks",
                                        (obj, args) =>
                                        {
                                            var id = selectedListViewProperty.GetFromListViewAndListViewItemByColumnName(selected, "Id");
                                            using (Tasks taskFrom = new Tasks(TaskType.UploadDownload, id, _wcfServicesPathId))
                                            {
                                                taskFrom.ShowDialog();
                                            }
                                        },
                                        string.Format("Delete this Directory", src));
            }

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
                var type = (Status)Enum.Parse(typeof(Status), selected.SubItems[typeColumnNumber].Text);
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
            var type = (Status)Enum.Parse(typeof(Status), selected.SubItems[typeColumnNumber].Text);
            var popUpMenu = CreatePopUpMenu(type);
            listview.ContextMenu = popUpMenu;
        }
    }
}
