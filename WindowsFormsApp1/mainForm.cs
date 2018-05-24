using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public partial class mainForm : Form
    {
        private List<string> fileActions = new List<string> { "Rename", "Delete", "Copy", "Cut", "Paste", "Upload", "Download"};
        private List<string> folderActions = new List<string> { "Enter Directory" };
        private HashSet<string> allActions;
        private static IActiveShell shellService;
        private static IActiveShell getStatusShellService;
        private static IActiveShell getFolderListShellService;
        private bool _activated;
        private bool _needToActivteCMD = true;
        private string cutPath;
        private string copyPath;
        private string currentPath;
        private bool _currentClientConnected;

        public System.Threading.Timer StatusTimer { get; private set; }
        public System.Threading.Timer FolderListTimer { get; private set; }

        public mainForm()
        {
            InitializeComponent();
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            folderActions.AddRange(fileActions);
            var all = new List<string>();
            all.AddRange(folderActions);
            all.AddRange(fileActions);
            allActions = new HashSet<string>(all);
            initializeServiceReferences();
            CreateListView();
            listView1.SizeChanged += new EventHandler(ListView_SizeChanged);
        }

        private System.Threading.Timer PefromTaskEveryXTime(Action task, int seconds)
        {
            var startTimeSpan = TimeSpan.Zero;
            var periodTimeSpan = TimeSpan.FromSeconds(seconds);

            var timer = new System.Threading.Timer((e) =>
            {
                task();
            }, null, startTimeSpan, periodTimeSpan);
            return timer;
        }

        private void ListView_SizeChanged(object sender, EventArgs e)
        {
            OptimisticTryAndFail(() =>
            {
                if(listView1.View == View.Details)
                {
                    for (var i = 0; i < listView1.Columns.Count; i++)
                    {
                        listView1.AutoResizeColumn(i, ColumnHeaderAutoResizeStyle.ColumnContent);
                        listView1.AutoResizeColumn(i, ColumnHeaderAutoResizeStyle.HeaderSize);
                    }
                }
                
            }, 1000, 3, "Fail to ListView_SizeChanged");


        }

        private ContextMenuOvveride CreatePopUpMenu(CmdLineType type)
        {
            ContextMenuOvveride PopupMenu = new ContextMenuOvveride();
            ListViewItem selected = null;
            ListView selectedListViewProperty = null;
            OptimisticTryAndFail(() =>
            {
                selected = listView1.SelectedItems[0];
                selectedListViewProperty = selected.ListView;
            }, 1000, 3, "Fail to CreatePopUpMenu - selected = listView1.SelectedItems[0]");
            
            if (type == CmdLineType.Folder)
            {
                PopupMenu.AddMenuItem("Enter",
                                            (obj, args) =>
                                            {
                                                var name = selectedListViewProperty.GetFromListViewAndListViewItemByColumnName(selected, "Name");
                                                if (name == "..")
                                                {
                                                    shellService.ActiveNextCommand("cd..");
                                                }
                                                else
                                                {
                                                    shellService.ActiveNextCommand(string.Format("cd \"{0}\"", name));
                                                }
                                            },
                                            "Enter this Directory");
            }

            var src = type.ToString().ToLower();
            PopupMenu.AddMenuItem("Reanme",
                                    (obj, args) =>
                                    {
                                        var name = selectedListViewProperty.GetFromListViewAndListViewItemByColumnName(selected, "Name");
                                        string input = Microsoft.VisualBasic.Interaction.InputBox("Enter the new name", "Rename", name, -1, -1);
                                        var succeeded = shellService.ActiveNextCommand(string.Format("rename \"{0}\" \"{1}\"", name, input));
                                        if (succeeded.StartsWith("Error"))
                                            MessageBox.Show("Error in rename", "Rename", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    },
                                    string.Format("Rename this {0}", selectedListViewProperty.GetFromListViewAndListViewItemByColumnName(selected, "Type").ToString()));
            PopupMenu.AddMenuItem("Delete",
                                    (obj, args) =>
                                    {
                                        var name = selectedListViewProperty.GetFromListViewAndListViewItemByColumnName(selected, "Name");
                                        var succeeded = shellService.ActiveNextCommand(string.Format("del \"{0}\"", name));
                                        if (succeeded.StartsWith("Error"))
                                            MessageBox.Show("Error in deletion", "Delete", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    });
            PopupMenu.AddMenuItem("Copy",
                                    (obj, args) =>
                                    {
                                        cutPath = null;
                                        copyPath = Path.Combine(currentPath, selectedListViewProperty.GetFromListViewAndListViewItemByColumnName(selected, "Name").ToString());
                                    });
            PopupMenu.AddMenuItem("Cut",
                                    (obj, args) =>
                                    {
                                        copyPath = null;
                                        cutPath = Path.Combine(currentPath, selectedListViewProperty.GetFromListViewAndListViewItemByColumnName(selected, "Name"));
                                    });
            PopupMenu.AddMenuItem("Paste",
                                    (obj, args) =>
                                    {
                                        var selectedType = selectedListViewProperty.GetFromListViewAndListViewItemByColumnName(selected, "Type");
                                        var currentPathToPaste = selectedType == CmdLineType.Folder.ToString() ?
                                                                 Path.Combine(currentPath, selectedListViewProperty.GetFromListViewAndListViewItemByColumnName(selected, "Name")) :
                                                                 currentPath;
                                        if (!string.IsNullOrEmpty(copyPath))
                                        {
                                            var succeeded = shellService.ActiveNextCommand(string.Format("copy \"{0}\" \"{1}\" /-Y", copyPath, currentPathToPaste));
                                            if (succeeded.StartsWith("Error"))
                                                MessageBox.Show("Error in paste", "Paste", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        }
                                        else if (!string.IsNullOrEmpty(cutPath))
                                        {
                                            var succeeded = shellService.ActiveNextCommand(string.Format("copy \"{0}\" \"{1}\" /Y", cutPath, currentPathToPaste));
                                            if (succeeded.StartsWith("Error"))
                                                MessageBox.Show("Error in paste", "Paste", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                            succeeded = shellService.ActiveNextCommand(string.Format("del \"{0}\"", cutPath));
                                            if (succeeded.StartsWith("Error"))
                                                MessageBox.Show("Error in paste", "Paste", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        }
                                        cutPath = copyPath = null;



                                    },
                                    string.Format("Move this Directory to another place", src));
            //at this time block the download of folder
            if (type == CmdLineType.File)
                PopupMenu.AddMenuItem("Download",
                                    (obj, args) =>
                                    {
                                        using (var saveForm = new FolderBrowserDialog())
                                        {
                                            var fileName = selectedListViewProperty.GetFromListViewAndListViewItemByColumnName(selected, "Name");
                                            if (saveForm.ShowDialog() == DialogResult.OK)
                                            {
                                                try
                                                {
                                                    downLoadFile(fileName, currentPath, saveForm.SelectedPath);
                                                    downloadUploadProgressBar.Value = 100;
                                                    downloadUploadProgressBar.Refresh();
                                                    MessageBox.Show("Download completed successfully","Download",MessageBoxButtons.OK, MessageBoxIcon.Information);
                                                    downloadUploadProgressBar.Visible = false;
                                                    downloadUploadLable.Visible = false;
                                                }

                                                catch (Exception e)
                                                {
                                                    MessageBox.Show(e.Message, "Error in download", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                                }
                                               
                                            }
                                        }
                                    });
            PopupMenu.AddMenuItem("Upload",
                                    (obj, args) =>
                                    {
                                        using (var saveForm = new OpenFileDialog())
                                        {
                                            if (saveForm.ShowDialog() == DialogResult.OK)
                                            {
                                                try
                                                {
                                                    var fileName = saveForm.FileName.Split('\\').Last();
                                                    var path = Path.GetDirectoryName(saveForm.FileName);
                                                    upLoadFile(fileName, path, currentPath);
                                                    downloadUploadProgressBar.Value = 100;
                                                    downloadUploadProgressBar.Refresh();
                                                    MessageBox.Show("Upload completed successfully", "Upload", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                                    downloadUploadProgressBar.Visible = false;
                                                    downloadUploadLable.Visible = false;
                                                }

                                                catch (Exception e)
                                                {
                                                    MessageBox.Show(e.Message, "Error in upload", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                                }

                                            }
                                        }
                                    });


            PopupMenu.ClickThenCollapse += PopupMenuClosed;
            return PopupMenu;
        }

        private void PopupMenuClosed(object sender, EventArgs e)
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
            //Must start with View.Details. If want to switch state then use the relevat function
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
            listView1.Columns[0].Text = "Type";
            listView1.Columns.Add("Name", -2, HorizontalAlignment.Left);
            listView1.Columns.Add("Size", -2, HorizontalAlignment.Left);
            listView1.Columns.Add("Last file modification", -2, HorizontalAlignment.Center);


            // Create two ImageList objects.
            ImageList imageListSmall = new ImageList();
            ImageList imageListLarge = new ImageList();

            imageListLarge.ImageSize = new Size(64, 64);
            imageListSmall.ImageSize = new Size(16, 16);

            imageListSmall.Images.Add(folderImage.Image);
            imageListSmall.Images.Add(fileImage.Image);
            imageListLarge.Images.Add(folderImage.Image);
            imageListLarge.Images.Add(fileImage.Image);

            //Assign the ImageList objects to the ListView.
            listView1.LargeImageList = imageListLarge;
            listView1.SmallImageList = imageListSmall;
        }

        private void detailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.View == View.Details) return;
            OptimisticTryAndFail(() =>
            {
                listView1.View = View.Details;
                var typeColIndex = listView1.GetColumnNumber("Type");
                var typeCol = listView1.Columns[typeColIndex];
                var firstCol = listView1.Columns[0];
                listView1.Columns.RemoveAt(typeColIndex);
                listView1.Columns.RemoveAt(0);
                listView1.Columns.Insert(0, typeCol);
                listView1.Columns.Insert(typeColIndex, firstCol);
            }, 1000, 3, "Fail to detailsToolStripMenuItem_Click");
        }

        private void smallIconsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.View == View.SmallIcon) return;
            OptimisticTryAndFail(() =>
            {
                var needToSwitchCol = listView1.View == View.Details;
                listView1.View = View.SmallIcon;
                if (needToSwitchCol)
                    SwitchNameColWithFirstCol();
            }, 1000, 3, "Fail to smallIconsToolStripMenuItem_Click");  
        }

        private void largIconsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.View == View.LargeIcon) return;
            OptimisticTryAndFail(() =>
            {
                var needToSwitchCol = listView1.View == View.Details;
                listView1.View = View.LargeIcon;
                if (needToSwitchCol)
                    SwitchNameColWithFirstCol();
            }, 1000, 3, "Fail to largIconsToolStripMenuItem_Click"); 
        }

        private void SwitchNameColWithFirstCol()
        {
            OptimisticTryAndFail(() =>
            {
                var nameColIndex = listView1.GetColumnNumber("Name");
                var nameCol = listView1.Columns[nameColIndex];
                var firstCol = listView1.Columns[0];
                listView1.Columns.RemoveAt(nameColIndex);
                listView1.Columns.RemoveAt(0);
                listView1.Columns.Insert(0, nameCol);
                listView1.Columns.Insert(nameColIndex, firstCol);
            }, 1000, 3, "Fail to SwitchNameColWithFirstCol");
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            OptimisticTryAndFail(() =>
            {
                var listview = sender as ListView;
                if (listview == null || listview.SelectedItems.Count <= 0) return;
                var typeColumnNumber = listview.GetColumnNumber("Type");
                var selected = listview.SelectedItems[0];
                var type = (CmdLineType)Enum.Parse(typeof(CmdLineType), selected.SubItems[typeColumnNumber].Text);
                var popUpMenu = CreatePopUpMenu(type);
                listview.ContextMenu = popUpMenu;
                var menuIndex = this.menuStrip1.GetMenuStripItemIndex("Action");
                var actionMenuItem = (ToolStripMenuItem)this.menuStrip1.Items[menuIndex];
                if (type == CmdLineType.File)
                {
                    allActions.ToList().ForEach(actionName => actionMenuItem.MakeVisible(actionName, false));
                    //fileActions.ForEach(actionName => actionMenuItem.MakeVisible(actionName));
                }
                else
                {
                    allActions.ToList().ForEach(actionName => actionMenuItem.MakeVisible(actionName, false));
                    //folderActions.ForEach(actionName => actionMenuItem.MakeVisible(actionName));
                }
            }, 1000, 3, "Fail to listView1_SelectedIndexChanged");
        }

        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            OptimisticTryAndFail(() =>
            {
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
                    var type = (CmdLineType)Enum.Parse(typeof(CmdLineType), selected.SubItems[typeColumnNumber].Text);
                    listview.ContextMenu = CreatePopUpMenu(type);
                    if (type == CmdLineType.File)
                    {
                        allActions.ToList().ForEach(actionName => actionMenuItem.MakeVisible(actionName, false));
                        //fileActions.ForEach(actionName => actionMenuItem.MakeVisible(actionName));
                    }
                    else
                    {
                        allActions.ToList().ForEach(actionName => actionMenuItem.MakeVisible(actionName, false));
                        //folderActions.ForEach(actionName => actionMenuItem.MakeVisible(actionName));
                    }
                }
            }, 1000, 3, "Fail to listView1_MouseClick");
            
        }
        private static void initializeServiceReferences()
        {
            initializeServiceReferences(ref shellService);
            initializeServiceReferences(ref getStatusShellService);
            initializeServiceReferences(ref getFolderListShellService);
        }
        private static void initializeServiceReferences(ref IActiveShell shellService)
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
            var shellAdress = string.Format("http://localhost:80/ShellTrasferServer/ActiveShell");
            var shellUri = new Uri(shellAdress);
            var shellEndpointAddress = new EndpointAddress(shellUri);
            var shellChannel = new ChannelFactory<IActiveShell>(shellBinding, shellEndpointAddress);
            shellService = shellChannel.CreateChannel();
        }

        private static byte[] ReadFully(Stream input)
        {
            if (!input.CanRead) return new byte[0];
            byte[] bytes;
            using (var reader = new BinaryReader(input))
            {
                bytes = reader.ReadBytes((int)input.Length);
            }
            return bytes;
        }

        private void upLoadFile(string fileName, string directory, string pathToSaveOnServer)
        {
            var path = Path.Combine(directory, fileName);
            FileInfo fileInfo = new FileInfo(path);
            RemoteFileInfo uploadRequestInfo = new RemoteFileInfo();
            uploadRequestInfo.FileName = fileName;
            uploadRequestInfo.Length = fileInfo.Length;
            uploadRequestInfo.PathToSaveOnServer = pathToSaveOnServer;
            uploadRequestInfo.FreshStart = true;
            downloadUploadProgressBar.Visible = true;
            downloadUploadProgressBar.Value = 0;
            downloadUploadLable.Visible = true;
            downloadUploadLable.Text = string.Format("Start uploading from this computer to the file to the server memory");
            downloadUploadProgressBar.Refresh();
            downloadUploadLable.Refresh();
            using (var file = File.OpenRead(path))
            {
                int bytesRead;
                var chunk = 999999;
                var byteStream = new byte[chunk * 10];
                uploadRequestInfo.FileSize = file.Length.ToString();
                while ((bytesRead = file.Read(byteStream, 0, byteStream.Length)) > 0)
                {
                    for (var i = 0; i < bytesRead; i = i + chunk)
                    {
                        uploadRequestInfo.FileByteStream = byteStream.Skip(i).Take(chunk).ToArray();
                        uploadRequestInfo.FileEnded = false;
                        SendRequestAndTryAgainIfTimeOutOrEndpointNotFound(() =>
                        {
                            shellService.StartTransferData();
                            shellService.ActiveUploadFile(uploadRequestInfo);
                        });
                        downloadUploadProgressBar.Value =  (int)((file.Position / (double)file.Length) * 100);
                        downloadUploadProgressBar.Refresh();
                        uploadRequestInfo.FreshStart = false;
                    }
                }
                uploadRequestInfo.FileByteStream = new byte[0];
                uploadRequestInfo.FileEnded = true;
                file.Close();
                downloadUploadProgressBar.Value = 100;
                downloadUploadProgressBar.Refresh();
                downloadUploadLable.Text = string.Format("Start Buffering File from server Memory to remote client Memory");
                downloadUploadProgressBar.Value = 0;
                downloadUploadLable.Refresh();
                downloadUploadProgressBar.Refresh();
                var response = SendRequestAndTryAgainIfTimeOutOrEndpointNotFound(() => shellService.ActiveUploadFile(uploadRequestInfo));

                if (response.FileName.StartsWith("Error"))
                {
                    throw new Exception(response.FileName);
                }
                if (response.FileName.StartsWith("Buffering") && response.FileByteStream.Length == 0)
                {
                    var precentage = response.FileName.Split(new string[] { "Memory" }, StringSplitOptions.RemoveEmptyEntries).Last().Trim().Split(' ').First();
                    downloadUploadProgressBar.Value = int.Parse(precentage);
                    downloadUploadProgressBar.Refresh();
                }
                if (response.FileName == "Upload Ended")
                    return;
            }
            FinishUpload();
        }
        private void FinishUpload()
        {
            var watch = new Stopwatch();
            double lastStop = 0;
            RemoteFileInfo requestData = new RemoteFileInfo()
            {
                FileByteStream = new byte[0],
                FileName = string.Empty,
                Length = 0,
                PathToSaveOnServer = string.Empty,
                FileSize = string.Empty,
                FreshStart = false,
                id = string.Empty,
                taskId = string.Empty,
            };

            var response = SendRequestAndTryAgainIfTimeOutOrEndpointNotFound(() => shellService.ActiveUploadFile(requestData));
            if (response.FileName.StartsWith("Buffering") && response.FileByteStream.Length == 0)
            {
                var precentage = response.FileName.Split(new string[] { "Memory" }, StringSplitOptions.RemoveEmptyEntries).Last().Trim().Split(' ').First();
                downloadUploadProgressBar.Value = int.Parse(precentage);
                downloadUploadProgressBar.Refresh();
            }
            else if (response.FileName.StartsWith("Error"))
            {
                throw new Exception(response.FileName);
            }
            else
            {
                return;
            }
            watch.Start();
            var periodToCheckInTheServer = 1;
            while (true)
            {
                if (watch.Elapsed.TotalSeconds - lastStop >= periodToCheckInTheServer)
                {
                    lastStop = watch.Elapsed.TotalSeconds;
                    response = response = SendRequestAndTryAgainIfTimeOutOrEndpointNotFound(() => shellService.ActiveUploadFile(requestData));
                    if (response.FileName.StartsWith("Buffering") && response.FileByteStream.Length == 0)
                    {
                        var precentage = response.FileName.Split(new string[] { "Memory" }, StringSplitOptions.RemoveEmptyEntries).Last().Trim().Split(' ').First();
                        downloadUploadProgressBar.Value = int.Parse(precentage);
                        downloadUploadProgressBar.Refresh();
                    }
                    else if (response.FileName.StartsWith("Error"))
                    {
                        throw new Exception(response.FileName);
                    }
                    else
                    {
                        return;
                    }
                }
            }
        }
        private RemoteFileInfo ReadyToStartDownload(string fileName, string pathInServer, string pathToSaveInClinet)
        {
            downloadUploadProgressBar.Visible = true;
            downloadUploadLable.Visible = true;
            downloadUploadLable.Text = string.Format("Start uploading the file to the server memory");
            downloadUploadProgressBar.Refresh();
            downloadUploadLable.Refresh();
            var watch = new Stopwatch();
            double lastStop = 0;
            DownloadRequest requestData = new DownloadRequest();
            requestData.FileName = fileName;
            requestData.PathInServer = pathInServer;
            requestData.NewStart = true;
            var response = SendRequestAndTryAgainIfTimeOutOrEndpointNotFound(() => shellService.ActiveDownloadFile(requestData));
            if (response.FileName.StartsWith("Buffering") && response.FileByteStream.Length == 0)
            {
                var precentage = response.FileName.Split(new string[] { "Memory" }, StringSplitOptions.RemoveEmptyEntries).Last().Trim().Split(' ').First();
                downloadUploadProgressBar.Value = int.Parse(precentage);
                downloadUploadProgressBar.Refresh();
            }
            else if (response.FileName.StartsWith("Error"))
            {
                throw new Exception(response.FileName);
            }
            else
            {
                return response;
            }
            requestData.NewStart = false;
            watch.Start();
            var periodToCheckInTheServer = 1;
            while (true)
            {
                if (watch.Elapsed.TotalSeconds - lastStop >= periodToCheckInTheServer)
                {
                    lastStop = watch.Elapsed.TotalSeconds;
                    response = SendRequestAndTryAgainIfTimeOutOrEndpointNotFound(() => shellService.ActiveDownloadFile(requestData));
                    if (response.FileName.StartsWith("Buffering") && response.FileByteStream.Length == 0)
                    {
                        var precentage = response.FileName.Split(new string[] { "Memory" }, StringSplitOptions.RemoveEmptyEntries).Last().Trim().Split(' ').First();
                        downloadUploadProgressBar.Value = int.Parse(precentage);
                        downloadUploadProgressBar.Refresh();
                    }
                    else if (response.FileName.StartsWith("Error"))
                    {
                        throw new Exception(response.FileName);
                    }
                    else
                    {
                        return response;
                    }
                }
            }
        }
        private void downLoadFile(string fileName, string pathInServer, string pathToSaveInClinet)
        {
            DownloadRequest requestData = new DownloadRequest();
            requestData.FileName = fileName;
            requestData.PathInServer = pathInServer;
            requestData.NewStart = false;
            requestData.id = string.Empty;
            requestData.taskId = string.Empty;
            requestData.PathToSaveInClient = string.Empty;
            using (var fileStrem = CreateNewFile(fileName, pathToSaveInClinet))
            {
                if (fileStrem == null)
                {
                    Console.WriteLine("Fail to create File in your computer " + fileName);
                    return;
                }

                var fileInfo = ReadyToStartDownload(fileName, pathInServer, pathToSaveInClinet);
                downloadUploadProgressBar.Value = 100;
                downloadUploadLable.Text = string.Format("Start transfering File from Server Memory...");
                downloadUploadLable.Refresh();
                downloadUploadProgressBar.Value = 0;
                downloadUploadProgressBar.Refresh();
                while (true)
                {
                    if (fileInfo.FileName.StartsWith("Error"))
                    {
                        throw new Exception(fileInfo.FileName);
                    }

                    if (fileInfo.FileEnded)
                    {
                        break;
                    }
                    else
                    {
                        var lastByteToWrite = 0;
                        var lastChunk = fileStrem.Position + fileInfo.FileByteStream.Length >= long.Parse(fileInfo.FileSize);
                        if (lastChunk)
                            lastByteToWrite = Convert.ToInt32(long.Parse(fileInfo.FileSize) - fileStrem.Position);
                        fileStrem.Write(fileInfo.FileByteStream, 0, lastChunk ? lastByteToWrite : fileInfo.FileByteStream.Length);
                        fileStrem.Flush();
                    }

                    SendRequestAndTryAgainIfTimeOutOrEndpointNotFound(() => fileInfo = shellService.ActiveDownloadFile(requestData));
                    if (!fileInfo.FileEnded)
                    {
                        downloadUploadProgressBar.Value = (int)((fileStrem.Position / double.Parse(fileInfo.FileSize)) * 100);
                        downloadUploadProgressBar.Refresh();
                    }
                    else
                    {
                        downloadUploadProgressBar.Value = 100;
                        downloadUploadProgressBar.Refresh();
                    }
                }
            }
        }

        public static void SendRequestAndTryAgainIfTimeOutOrEndpointNotFound(Action op, Action inTimeOutException = null, Action inCommunicationException = null, Action inGeneralException = null)
        {
            while (true)
            {
                try
                {
                    op();
                    break;
                }
                catch (TimeoutException e)
                {
                    if (inTimeOutException != null)
                        inTimeOutException();
                    //try again
                }
                catch (CommunicationException e)
                {
                    if (inCommunicationException != null)
                        inCommunicationException();
                    //try again
                }
                catch (Exception e)
                {
                    if (inGeneralException != null)
                        inGeneralException();
                    else
                        throw e;
                }
            }
        }

        public static T SendRequestAndTryAgainIfTimeOutOrEndpointNotFound<T>(Func<T> op, Action inTimeOutException = null, Action inCommunicationException = null, Action inGeneralException = null)
        {
            while (true)
            {
                try
                {
                    return op();
                }
                catch (TimeoutException e)
                {
                    if (inTimeOutException != null)
                        inTimeOutException();
                    //try again
                }
                catch (CommunicationException e)
                {
                    if (inCommunicationException != null)
                        inCommunicationException();
                    //try again
                }
                catch (Exception e)
                {
                    if (inGeneralException != null)
                        inGeneralException();
                    else
                        throw e;
                }
            }
        }

        private static FileStream CreateNewFile(string fileName, string pathTosave)
        {
            var dirPath = pathTosave;
            var path = Path.Combine(dirPath, fileName);

            try
            {

                // Delete the file if it exists.
                if (File.Exists(path))
                {
                    // Note that no lock is put on the
                    // file and the possibility exists
                    // that another process could do
                    // something with it between
                    // the calls to Exists and Delete.
                    File.Delete(path);
                }

                // Create the file.
                if (!Directory.Exists(dirPath))
                    Directory.CreateDirectory(dirPath);
                FileStream fs = File.Create(path);
                return fs;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        

        private static void Help()
        {
            Console.WriteLine("The download command: Download file-name path-in-remote path-to-save-in-your-copmuter");
            Console.WriteLine("The upload command: Upload file-name path-in-your-copmuter path-to-save-in-remote");
            Console.WriteLine("The status command: Status");
            Console.WriteLine("The select client command: SelectClient client-id");
            Console.WriteLine("The clear all clients task queue command: ClearCommands");
            Console.WriteLine("The exit command: exit");
            Console.WriteLine("The close client command: CloseClient client-id");
            Console.WriteLine("The open client shell command: Run");
            Console.WriteLine("The clear all server data command: ClearAllData");
            Console.WriteLine("The delete client task command: DeleteClientTask client-id Donwload-Upload/Shell Task-Number");
            Console.WriteLine("The set nick name command: SetClientNick client-id nick-name");
            Console.WriteLine("The help command: Help");
        }

        private static void ConsolePrintColor(string text, Dictionary<Regex, ConsoleColor> wordsToColor)
        {
            var replace = text.Replace("\r", "");
            var withNoNewLine = replace.Split('\n');
            foreach (var line in withNoNewLine)
            {
                var words = line.Split(' ');
                foreach (var firstWord in words)
                {
                    if (wordsToColor.Count(key => key.Key.IsMatch(firstWord)) == 1)
                    {
                        Console.ForegroundColor = wordsToColor.First(key => key.Key.IsMatch(firstWord)).Value;
                        Console.Write(firstWord);
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.Write(firstWord);
                    }
                    Console.Write(" ");

                }
                Console.WriteLine();
            }
        }

        private static string tryToConnect(string response)
        {
            try
            {
                response = shellService.ActiveClientRun();
                Console.WriteLine("Connected to server, Remote computer connected to server");
            }
            catch
            {
                Console.WriteLine("Cannot connect to server, or Remote computer not connected to server");
                Console.WriteLine("try again?");
                var answer = Console.ReadLine();
                if (answer != "y")
                {
                    Console.WriteLine("Exiting....");
                    System.Threading.Thread.Sleep(3000);
                    ((ICommunicationObject)shellService).Close();
                    Environment.Exit(0);
                }

                Thread.Sleep(2000);
                tryToConnect(response);
            }

            return response;
        }

        private static void GetLastRow(out string actionResponse, string currentActionResponse, ref string response)
        {
            var responseReplace = response.Replace("\r", "");
            var splited = responseReplace.Split('\n');
            var errorCheck = splited.Last(x => x != "").StartsWith("Error");
            if (errorCheck == true)
            {
                actionResponse = currentActionResponse;
                response = splited.Last(x => x != "");
                return;
            }
            actionResponse = splited.Last(x => x != "");
            var actionResponseDelete = actionResponse;
            actionResponse = actionResponse + "> ";
            var strb = new StringBuilder();
            foreach (var str in splited)
            {
                if (str != actionResponseDelete)
                    if (str.Length > 0)
                    {
                        var newstr = "";
                        if (str.StartsWith("\n"))
                            newstr = str.Substring("\n".Length);
                        else
                            newstr = str;
                        if (newstr == "")
                            continue;
                        strb.AppendLine(newstr);
                    }
            }
            response = strb.ToString();
        }

        public static void ClearCurrentConsoleLine()
        {
            var row = Console.CursorTop - 1;
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, Console.CursorTop - 1);
        }

        private void statusToolStripMenuItem_Click(object sender, EventArgs e)
        {
            statusForm status = new statusForm();
            status.BringToFront();
            status.ShowDialog();
        }

        Object statusFromServerLock = new Object();
        private void GetStatusFromServer()
        {
            OptimisticTryAndFail(() =>
            {
                lock (statusFromServerLock)
                {
                    var status = getStatusShellService.GetStatus();
                    status = status.Replace("\r", "");
                    var statusSplittedNewLine = status.Split('\n');
                    statusSplittedNewLine = statusSplittedNewLine.Where(str => !string.IsNullOrEmpty(str) && !string.IsNullOrWhiteSpace(str)).ToArray();
                    var selectedClientId = statusSplittedNewLine.Last().Split(':').Last().Trim();
                    var clients = status.Split(new string[] { "Client" }, StringSplitOptions.RemoveEmptyEntries);
                    clients = clients.ToArray().Skip(1).Take(clients.Count() - 2).ToArray();
                    var isSelectedClientAlive = false;
                    var selectedClient = clients.FirstOrDefault(client =>
                    {
                        var fields = client.Replace('\t', '\n').Split('\n');
                        fields = fields.Where(str => !string.IsNullOrEmpty(str) && !string.IsNullOrWhiteSpace(str) && str != "The selected ").ToArray();
                        var id = fields[0].Split(':').Last().Trim();
                        isSelectedClientAlive = bool.Parse(fields[2].Split(':').Last());
                        return id == selectedClientId;
                    });
                    if (statusSplittedNewLine.Length == 2 && statusSplittedNewLine[1] == "There is no clients connected" ||
                    selectedClient != null && !isSelectedClientAlive)
                    {
                        _currentClientConnected = false;
                        this.Invoke((MethodInvoker)(() =>
                        {
                            listView1.BeginUpdate();
                            listView1.SuspendLayout();
                            NoSelectedClient.Visible = true;
                            currentPathTextBox.Text = string.Empty;
                            listView1.Visible = false;
                            listView1.ResumeLayout();
                            listView1.EndUpdate();
                        }));
                    }
                    else
                    {
                        _currentClientConnected = true;
                        this.Invoke((MethodInvoker)(() =>
                        {
                            listView1.BeginUpdate();
                            listView1.SuspendLayout();
                            NoSelectedClient.Visible = false;
                            listView1.Visible = true;
                            listView1.ResumeLayout();
                            listView1.EndUpdate();
                        }));
                    }
                }
            }, 1000, 3, "Fail to GetStatusFromServer");
        }

        static long ConvertBytesToKB(long bytes)
        {
            var ans = (int) (bytes / 1024);
            return ans == 0 ? 1 : ans;

        }

        Object folderListFromServerLock = new Object();
        int raiseException = 3;
        private void GetFolderListFromServer()
        {
            if (!_currentClientConnected) return;
            OptimisticTryAndFail(() =>
            {
                lock (folderListFromServerLock)
                {
                    try
                    {
                        if (_needToActivteCMD)
                        {
                            getFolderListShellService.ActiveClientRun();
                            _needToActivteCMD = false;
                        }
                        var folderListStr = getFolderListShellService.ActiveNextCommand("dir /b /ad").Replace("\r", "");
                        if (folderListStr == "Client CallBack is Not Found" ||
                            folderListStr.StartsWith("Error") ||
                            folderListStr == "The communication object, System.ServiceModel.Channels.ServiceChannel, cannot be used for communication because it has been Aborted.")
                        {
                            _needToActivteCMD = true;
                            Thread.Sleep(10000);
                            raiseException = 3;
                            return;
                        }
                        var folderList = folderListStr.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        currentPath = folderList.ElementAt(folderList.Count - 1);
                        currentPathTextBox.KeyDown -= currentPathTextBox_KeyDown;
                        if (!currentPathTextBoxSelected)
                            this.Invoke((MethodInvoker)(() => currentPathTextBox.Text = currentPath));
                        this.Invoke((MethodInvoker)(() =>
                                this.Width = Math.Max(TextRenderer.MeasureText(currentPathTextBox.Text, currentPathTextBox.Font).Width + 100, this.Width)));
                        currentPathTextBox.KeyDown += currentPathTextBox_KeyDown;
                        folderList.RemoveAt(folderList.Count - 1);
                        folderList.RemoveAt(0);

                        var allListStr = getFolderListShellService.ActiveNextCommand("dir /b").Replace("\r", "");
                        var allList = allListStr.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        allList.RemoveAt(allList.Count - 1);
                        allList.RemoveAt(0);
                        allList.RemoveAll(file => folderList.Contains(file));
                        var fileList = allList;

                        var dirData = getFolderListShellService.ActiveNextCommand("dir").Replace("\r", "");
                        var dirDataArr = dirData.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        for (var i = 0; i < 6; i++)
                        {
                            dirDataArr.RemoveAt(0);
                        }
                        for (var i = 0; i < 3; i++)
                        {
                            dirDataArr.RemoveAt(dirDataArr.Count - 1);
                        }
                        var FileFolderList = new List<CMDFileFolder>();
                        foreach (var line in dirDataArr)
                        {
                            
                            var lineSplit = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList();
                            if(lineSplit[2].ToUpper() == "PM" || lineSplit[2].ToUpper() == "AM")
                            {
                                lineSplit[1] = lineSplit[1] + " " + lineSplit[2];
                                for(var i = 2; i < lineSplit.Count - 1; i ++)
                                {
                                    lineSplit[i] = lineSplit[i + 1];
                                }
                                lineSplit = lineSplit.Take(lineSplit.Count - 1).ToList();
                            }
                            for (var i = 4; i < lineSplit.Count; i++)
                            {
                                lineSplit[3] += " " + lineSplit[i];
                            }
                            var isDirectory = folderList.Contains(lineSplit[3]);
                            if (isDirectory)
                            {
                                var folder = new CMDFileFolder(CmdLineType.Folder, lineSplit[3], string.Empty, string.Format("{0} {1}", lineSplit[0], lineSplit[1]));
                                FileFolderList.Add(folder);
                            }
                            else
                            {

                                var persedNumber = long.Parse(lineSplit[2].Replace(",", ""));
                                var fileSize = string.Format("{0} KB", ConvertBytesToKB(persedNumber));
                                var modificationDate = string.Format("{0} {1}", lineSplit[0], lineSplit[1]);
                                var file = new CMDFileFolder(CmdLineType.File, lineSplit[3], fileSize, modificationDate);
                                FileFolderList.Add(file);
                            }
                        }
                        this.Invoke((MethodInvoker)(() =>
                        {
                            var oldTopItemIndex = 0;
                            if (listView1.View == View.Details)
                                oldTopItemIndex = listView1.TopItem == null ? 0 : listView1.TopItem.Index;
                            int oldFocusedIndex = listView1 != null ?
                                                        listView1.SelectedItems != null ?
                                                                    listView1.SelectedItems.Count > 0 ?
                                                                                    listView1.SelectedItems[0].Index :
                                                                                    -1 :
                                                                                    -1 :
                                                                                    -1;
                            listView1.SuspendLayout();
                            listView1.BeginUpdate();
                            listView1.Items.Clear();
                            AddToListView(0, "..", CmdLineType.Folder);
                            var index = 1;
                            foreach (var item in FileFolderList)
                            {
                                AddToListView(index++, item);
                            }
                            if (oldFocusedIndex > -1 && listView1 != null && listView1.Items != null && oldFocusedIndex < listView1.Items.Count)
                            {
                                listView1.Items[oldFocusedIndex].Selected = true;
                            }

                            listView1.EndUpdate();
                            listView1.ResumeLayout();
                            if (listView1.View == View.Details)
                                listView1.TopItem = listView1.Items[oldTopItemIndex];
                        }));
                        raiseException = 3;
                    }
                    catch (Exception e)
                    {
                        if(raiseException == 0)
                        {
                            raiseException = 3;
                            MessageBox.Show("No connection to the Server", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        raiseException--;
                    }
                }
            }, 1000, 3, "Fail to GetFolderListFromServer");
        }

        private void AddToListView(int index, string name, CmdLineType type, bool check = false)
        {
            OptimisticTryAndFail(() =>
            {
                listView1.BeginUpdate();
                listView1.SuspendLayout();
                // Create three items and three sets of subitems for each item.
                ListViewItem item1 = new ListViewItem(listView1.View == View.Details ? type.ToString() : name, index);
                // Place a check mark next to the item.
                item1.Checked = check;
                item1.SubItems.Add(listView1.View == View.Details ? name : type.ToString());
                item1.ImageIndex = (int)type;
                listView1.Items.Add(item1);
                listView1.ResumeLayout();
                listView1.EndUpdate();
                listView1.Refresh();
                ListView_SizeChanged(null, null);
            }, 1000, 3, "Fail to AddToListView");
        }

        private void AddToListView(int index, CMDFileFolder ff, bool check = false)
        {
            OptimisticTryAndFail(() =>
            {
                listView1.BeginUpdate();
                listView1.SuspendLayout();
                // Create three items and three sets of subitems for each item.
                ListViewItem item1 = new ListViewItem(listView1.View == View.Details ? ff.GetType().ToString() : ff.GetName(), index);
                // Place a check mark next to the item.
                item1.Checked = check;
                item1.SubItems.Add(listView1.View == View.Details ? ff.GetName() : ff.GetType().ToString());
                item1.SubItems.Add(ff.GetType() == CmdLineType.Folder ? string.Empty : ff.getSize().ToString());
                item1.SubItems.Add(ff.GetLastModificationDate());
                item1.ImageIndex = (int)ff.GetType();
                listView1.Items.Add(item1);
                listView1.ResumeLayout();
                listView1.EndUpdate();
                listView1.Refresh();
                ListView_SizeChanged(null, null);
            }, 1000, 3, "Fail to AddToListView");
        }

        private void mainForm_Activated(object sender, EventArgs e)
        {
            if (_activated) return;
            _activated = true;
            GetStatusFromServer();
            StatusTimer = PefromTaskEveryXTime(GetStatusFromServer, 15);
            FolderListTimer = PefromTaskEveryXTime(GetFolderListFromServer, 3);
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            OptimisticTryAndFail(() =>
            {
                if (listView1.SelectedItems.Count == 0) return;
                var selected = listView1.SelectedItems[0];
                if (selected.GetFromListViewItemCollectionByColumnName("Name") == "..")
                {
                    shellService.ActiveNextCommand("cd..");
                }
                else
                {
                    shellService.ActiveNextCommand(string.Format("cd \"{0}\"", selected.GetFromListViewItemCollectionByColumnName("Name")));
                }
            }, 1000, 3, "Fail to listView1_MouseDoubleClick");
        }

        private void OptimisticTryAndFail(Action act, int timeBetweenTry, int numOfTry, string exception)
        {
            while(numOfTry >= 0)
            {
                try
                {
                    act();
                    return;
                }
                catch (Exception e)
                {
                    numOfTry--;
                }
            }
             throw new Exception(exception);
        }

        private void currentPathTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            var textBox = sender as TextBox;
            var newPath = textBox.Text;
            var newPathCharArr = newPath.ToCharArray();
            var isInTheDrive = currentPath.Contains(newPath);
            if (newPathCharArr.Last() == '\\' && !isInTheDrive)
                newPath = newPath.Remove(newPathCharArr.Length - 1);
            var isNewPathDrive = !isInTheDrive && newPath.Length == 2 && newPath.ElementAt(1) == ':';
            var resp = shellService.ActiveNextCommand(isNewPathDrive ? newPath : string.Format("cd \"{0}\"", newPath));
            var splitedPath = resp.Replace("\r","").Split('\n');
            splitedPath = splitedPath.Where(str => !string.IsNullOrEmpty(str) && !string.IsNullOrWhiteSpace(str)).ToArray();
            var returnPath = isNewPathDrive ? newPath : splitedPath.Last();
            if (returnPath.ToLower() != newPath.ToLower() && returnPath.ToLower() != newPath.ToLower().Substring(0, newPath.Length-1))
                MessageBox.Show("Invalid path", "Error in changing path", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private bool currentPathTextBoxSelected;
        private void currentPathTextBox_Leave(object sender, EventArgs e)
        {
            currentPathTextBoxSelected = false;
        }

        private void currentPathTextBox_Enter(object sender, EventArgs e)
        {
            if (NoSelectedClient.Visible) return;
            currentPathTextBoxSelected = true;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var about = new AboutBox())
            {
                about.ShowDialog();
            }
        }
    }
}
