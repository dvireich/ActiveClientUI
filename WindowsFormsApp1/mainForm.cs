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
    public partial class mainForm : Form
    {
        private List<string> fileActions = new List<string> { "Rename", "Delete", "Copy", "Cut", "Paste", "Upload", "Download" };
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
        private string _wcfServicesPathId;
        private LogInForm _loginFrom;
        private static readonly log4net.ILog log
      = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private List<CMDFileFolder> currentFilesAndFolders;
        private bool userActionDisabled;

        public System.Threading.Timer StatusTimer { get; private set; }
        public System.Threading.Timer FolderListTimer { get; private set; }

        private void CloseAllConnections()
        {
            try
            {
                if (shellService != null)
                    ((ICommunicationObject)shellService).Close();

                if (getStatusShellService != null)
                    ((ICommunicationObject)getStatusShellService).Close();


                if (getFolderListShellService != null)
                    ((ICommunicationObject)getFolderListShellService).Close();

                shellService = null;
                getStatusShellService = null;
                getFolderListShellService = null;
            }
            catch { }

        }

        public mainForm(string id, LogInForm loginForm)
        {
            _wcfServicesPathId = id;
            _loginFrom = loginForm;
            InitializeComponent();
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            folderActions.AddRange(fileActions);
            var all = new List<string>();
            all.AddRange(folderActions);
            all.AddRange(fileActions);
            allActions = new HashSet<string>(all);
            initializeServiceReferences(_wcfServicesPathId);
            CreateListView();
            listView1.SizeChanged += new EventHandler(ListView_SizeChanged);

        }

        private System.Threading.Timer PefromTaskEveryXTime(Action task, int seconds)
        {
            var startTimeSpan = TimeSpan.Zero;
            var periodTimeSpan = TimeSpan.FromSeconds(seconds);

            var timer = new System.Threading.Timer((e) =>
            {
                try
                {
                    task();
                }
                catch (Exception ex)
                {
                    log.Debug($"Error in executing task: {task.GetType().FullName} with the following exception: {ex.Message}");
                }
            }, null, startTimeSpan, periodTimeSpan);
            return timer;
        }

        private void ListView_SizeChanged(object sender, EventArgs e)
        {
            if (listView1.View == View.Details)
            {
                for (var i = 0; i < listView1.Columns.Count; i++)
                {
                    listView1.AutoResizeColumn(i, ColumnHeaderAutoResizeStyle.ColumnContent);
                    listView1.AutoResizeColumn(i, ColumnHeaderAutoResizeStyle.HeaderSize);
                }
            }


        }

        private ContextMenuOvveride CreatePopUpMenu(CmdLineType type)
        {
            ContextMenuOvveride PopupMenu = new ContextMenuOvveride();
            ListViewItem selected = null;
            ListView selectedListViewProperty = null;
            selected = listView1.SelectedItems[0];
            selectedListViewProperty = selected.ListView;

            if (type == CmdLineType.Folder)
            {
                PopupMenu.AddMenuItem("Enter",
                                            (obj, args) =>
                                            {
                                                if (userActionDisabled)
                                                {
                                                    DisplayMessageDoOperationWhileDonwloadUpload();
                                                    return;
                                                }

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
                                        if (userActionDisabled)
                                        {
                                            DisplayMessageDoOperationWhileDonwloadUpload();
                                            return;
                                        }

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
                                        if (userActionDisabled)
                                        {
                                            DisplayMessageDoOperationWhileDonwloadUpload();
                                            return;
                                        }

                                        var name = selectedListViewProperty.GetFromListViewAndListViewItemByColumnName(selected, "Name");
                                        var succeeded = shellService.ActiveNextCommand(string.Format("del \"{0}\"", name));
                                        if (succeeded.StartsWith("Error"))
                                            MessageBox.Show("Error in deletion", "Delete", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    });
            PopupMenu.AddMenuItem("Copy",
                                    (obj, args) =>
                                    {
                                        if (userActionDisabled)
                                        {
                                            DisplayMessageDoOperationWhileDonwloadUpload();
                                            return;
                                        }

                                        cutPath = null;
                                        copyPath = Path.Combine(currentPath, selectedListViewProperty.GetFromListViewAndListViewItemByColumnName(selected, "Name").ToString());
                                    });
            PopupMenu.AddMenuItem("Cut",
                                    (obj, args) =>
                                    {
                                        if (userActionDisabled)
                                        {
                                            DisplayMessageDoOperationWhileDonwloadUpload();
                                            return;
                                        }

                                        copyPath = null;
                                        cutPath = Path.Combine(currentPath, selectedListViewProperty.GetFromListViewAndListViewItemByColumnName(selected, "Name"));
                                    });
            PopupMenu.AddMenuItem("Paste",
                                    (obj, args) =>
                                    {
                                        if (userActionDisabled)
                                        {
                                            DisplayMessageDoOperationWhileDonwloadUpload();
                                            return;
                                        }

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
                                        if (userActionDisabled)
                                        {
                                            DisplayMessageDoOperationWhileDonwloadUpload();
                                            return;
                                        }

                                        using (var saveForm = new FolderBrowserDialog())
                                        {
                                            var fileName = selectedListViewProperty.GetFromListViewAndListViewItemByColumnName(selected, "Name");
                                            if (saveForm.ShowDialog() == DialogResult.OK)
                                            {
                                                RunInAnotherThread(() =>
                                                {
                                                    userActionDisabled = true;
                                                    try
                                                    {
                                                        downLoadFile(fileName, currentPath, saveForm.SelectedPath);
                                                        Invoke((MethodInvoker)(() =>
                                                        {
                                                            downloadUploadProgressBar.Value = 100;

                                                            MessageBox.Show("Download completed successfully", "Download", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                                            downloadUploadProgressBar.Visible = false;
                                                            downloadUploadLable.Visible = false;
                                                        }));
                                                    }

                                                    catch (Exception e)
                                                    {
                                                        MessageBox.Show(e.Message, "Error in download", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                                    }
                                                }, () =>
                                                {
                                                    userActionDisabled = false;
                                                });




                                            }
                                        }
                                    });
            PopupMenu.AddMenuItem("Upload",
                                    (obj, args) =>
                                    {
                                        if (userActionDisabled)
                                        {
                                            DisplayMessageDoOperationWhileDonwloadUpload();
                                            return;
                                        }

                                        using (var saveForm = new OpenFileDialog())
                                        {
                                            if (saveForm.ShowDialog() == DialogResult.OK)
                                            {
                                                try
                                                {
                                                    var fileName = saveForm.FileName.Split('\\').Last();
                                                    var path = Path.GetDirectoryName(saveForm.FileName);
                                                    upLoadFile(fileName, path, currentPath);

                                                    Invoke((MethodInvoker)(() =>
                                                    {
                                                        downloadUploadProgressBar.Value = 100;

                                                        MessageBox.Show("Upload completed successfully", "Upload", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                                        downloadUploadProgressBar.Visible = false;
                                                        downloadUploadLable.Visible = false;
                                                    }));
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
            if (userActionDisabled)
            {
                DisplayMessageDoOperationWhileDonwloadUpload();
                return;
            }

            if (listView1.View == View.Details) return;
            listView1.View = View.Details;
            var typeColIndex = listView1.GetColumnNumber("Type");
            var typeCol = listView1.Columns[typeColIndex];
            var firstCol = listView1.Columns[0];
            listView1.Columns.RemoveAt(typeColIndex);
            listView1.Columns.RemoveAt(0);
            listView1.Columns.Insert(0, typeCol);
            listView1.Columns.Insert(typeColIndex, firstCol);
        }

        private void smallIconsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (userActionDisabled)
            {
                DisplayMessageDoOperationWhileDonwloadUpload();
                return;
            }

            if (listView1.View == View.SmallIcon) return;
            var needToSwitchCol = listView1.View == View.Details;
            listView1.View = View.SmallIcon;
            if (needToSwitchCol)
                SwitchNameColWithFirstCol();
        }

        private void largIconsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (userActionDisabled)
            {
                DisplayMessageDoOperationWhileDonwloadUpload();
                return;
            }

            if (listView1.View == View.LargeIcon) return;
            var needToSwitchCol = listView1.View == View.Details;
            listView1.View = View.LargeIcon;
            if (needToSwitchCol)
                SwitchNameColWithFirstCol();
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

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
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
        }

        private void listView1_MouseClick(object sender, MouseEventArgs e)
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
        }
        private static void initializeServiceReferences(string wcfServicesPathId)
        {
            initializeServiceReferences(ref shellService, wcfServicesPathId);
            initializeServiceReferences(ref getStatusShellService, wcfServicesPathId);
            initializeServiceReferences(ref getFolderListShellService, wcfServicesPathId);
        }
        private static void initializeServiceReferences(ref IActiveShell shellService, string wcfEndpointId)
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
            var shellAdress = string.Format("http://localhost:80/ShellTrasferServer/ActiveShell/{0}", wcfEndpointId);
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

            Invoke((MethodInvoker)(() =>
            {
                downloadUploadProgressBar.Visible = true;
                downloadUploadProgressBar.Value = 0;
                downloadUploadLable.Visible = true;
                downloadUploadLable.Text = string.Format("Upload Process: Uploading from this PC to the server memory...");

                downloadUploadLable.Refresh();
            }));

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

                        Invoke((MethodInvoker)(() =>
                        {
                            downloadUploadProgressBar.Value = (int)((file.Position / (double)file.Length) * 100);
                        }));

                        uploadRequestInfo.FreshStart = false;

                    }
                }
                uploadRequestInfo.FileByteStream = new byte[0];
                uploadRequestInfo.FileEnded = true;
                file.Close();

                Invoke((MethodInvoker)(() =>
                {
                    downloadUploadProgressBar.Value = 100;
                    downloadUploadLable.Text = string.Format("Upload Process: Transfering File from server Memory to remote client PC...");
                    downloadUploadProgressBar.Value = 0;
                }));

                var response = SendRequestAndTryAgainIfTimeOutOrEndpointNotFound(() => shellService.ActiveUploadFile(uploadRequestInfo));

                if (response.FileName.StartsWith("Error"))
                {
                    throw new Exception(response.FileName);
                }
                if (response.FileName.StartsWith("Buffering") && response.FileByteStream.Length == 0)
                {
                    var precentage = response.FileName.Split(new string[] { "Memory" }, StringSplitOptions.RemoveEmptyEntries).Last().Trim().Split(' ').First();

                    Invoke((MethodInvoker)(() =>
                    {
                        downloadUploadProgressBar.Value = int.Parse(precentage);
                    }));

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
            Invoke((MethodInvoker)(() =>
            {
                downloadUploadProgressBar.Visible = true;
                downloadUploadLable.Visible = true;
                downloadUploadLable.Text = string.Format("Download process: Uploading the file from remote PC to the server memory...");

                downloadUploadLable.Refresh();
            }));

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

                Invoke((MethodInvoker)(() =>
                {
                    downloadUploadProgressBar.Value = int.Parse(precentage);
                }));

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

                        Invoke((MethodInvoker)(() =>
                        {
                            downloadUploadProgressBar.Value = int.Parse(precentage);

                        }));

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

                Invoke((MethodInvoker)(() =>
                {
                    downloadUploadProgressBar.Value = 100;
                    downloadUploadLable.Text = string.Format("Download process:  Transfering File from Server Memory to your PC...");
                    downloadUploadProgressBar.Value = 0;
                }));

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
                        Invoke((MethodInvoker)(() =>
                        {
                            downloadUploadProgressBar.Value = (int)((fileStrem.Position / double.Parse(fileInfo.FileSize)) * 100);
                        }));
                    }
                    else
                    {
                        Invoke((MethodInvoker)(() =>
                        {
                            downloadUploadProgressBar.Value = 100;
                        }));
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
                catch (TimeoutException)
                {
                    if (inTimeOutException != null)
                        inTimeOutException();
                    //try again
                }
                catch (CommunicationException)
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
                catch (TimeoutException)
                {
                    if (inTimeOutException != null)
                        inTimeOutException();
                    //try again
                }
                catch (CommunicationException)
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
            catch (Exception)
            {
                return null;
            }
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
            using (statusForm status = new statusForm(_wcfServicesPathId))
            {
                status.BringToFront();
                status.ShowDialog();
            }

        }

        Object statusFromServerLock = new Object();
        private void GetStatusFromServer()
        {
            lock (statusFromServerLock)
            {
                string[] statusSplittedNewLine;
                bool isSelectedClientAlive;
                string selectedClient;

                var status = getStatusShellService.GetStatus();
                status = ParseStatus(status, out statusSplittedNewLine, out isSelectedClientAlive, out selectedClient);

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
        }

        private static string ParseStatus(string status, out string[] statusSplittedNewLine, out bool isSelectedClientAlive, out string selectedClient)
        {
            status = status.Replace("\r", "");
            statusSplittedNewLine = status.Split('\n');
            statusSplittedNewLine = statusSplittedNewLine.Where(str => !string.IsNullOrEmpty(str) && !string.IsNullOrWhiteSpace(str)).ToArray();
            var selectedClientId = statusSplittedNewLine.Last().Split(':').Last().Trim();
            var clients = status.Split(new string[] { "Client" }, StringSplitOptions.RemoveEmptyEntries);
            clients = clients.ToArray().Skip(1).Take(clients.Count() - 2).ToArray();
            var SelectedClientAlive = false;
            selectedClient = clients.FirstOrDefault(client =>
            {
                var fields = client.Replace('\t', '\n').Split('\n');
                fields = fields.Where(str => !string.IsNullOrEmpty(str) && !string.IsNullOrWhiteSpace(str) && str != "The selected ").ToArray();
                var id = fields[0].Split(':').Last().Trim();
                SelectedClientAlive = bool.Parse(fields[2].Split(':').Last());
                return id == selectedClientId;
            });

            isSelectedClientAlive = SelectedClientAlive;

            return status;
        }

        static long ConvertBytesToKB(long bytes)
        {
            var ans = (int)(bytes / 1024);
            return ans == 0 ? 1 : ans;

        }

        Object folderListFromServerLock = new Object();
        int raiseException = 3;
        private void GetFolderListFromServer()
        {
            if (!_currentClientConnected) return;

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

                    var folderList = GetFolderListFromServerAndAssignTheFolderPath(folderListStr);

                    var fileList = CalculateFileListFromFolderList(folderList);

                    var FileFolderList = CalculateFileAndFolderAdditionalData(folderList);

                    var shouldChangeUi = CheckIfFileFolderListChanged(FileFolderList);

                    if (shouldChangeUi)
                    {
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
                            ListView_SizeChanged(null, null);
                            listView1.EndUpdate();
                            listView1.ResumeLayout();
                            if (listView1.View == View.Details)
                            {
                                try
                                {
                                    listView1.TopItem = listView1.Items[oldTopItemIndex];
                                }
                                catch
                                { }

                            }
                        }));
                    }

                    raiseException = 3;
                }
                catch (Exception)
                {
                    if (raiseException == 0)
                    {
                        raiseException = 3;
                        MessageBox.Show("No connection to the Server", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    raiseException--;
                }
            }
        }

        private static List<CMDFileFolder> CalculateFileAndFolderAdditionalData(List<string> folderList)
        {
            var FileFolderList = new List<CMDFileFolder>();

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

            foreach (var line in dirDataArr)
            {

                var lineSplit = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList();
                if (lineSplit[2].ToUpper() == "PM" || lineSplit[2].ToUpper() == "AM")
                {
                    lineSplit[1] = lineSplit[1] + " " + lineSplit[2];
                    for (var i = 2; i < lineSplit.Count - 1; i++)
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

            return FileFolderList;
        }

        private static List<string> CalculateFileListFromFolderList(List<string> folderList)
        {
            var allListStr = getFolderListShellService.ActiveNextCommand("dir /b").Replace("\r", "");
            var allList = allListStr.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            allList.RemoveAt(allList.Count - 1);
            allList.RemoveAt(0);
            allList.RemoveAll(file => folderList.Contains(file));
            return allList;
        }

        private List<string> GetFolderListFromServerAndAssignTheFolderPath(string folderListStr)
        {
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
            return folderList;
        }

        private bool CheckIfFileFolderListChanged(List<CMDFileFolder> FileFolderList)
        {
            var listChanged = false;
            if (currentFilesAndFolders == null || currentFilesAndFolders.Count != FileFolderList.Count)
            {
                currentFilesAndFolders = FileFolderList;
                listChanged = true;
            }
            else
            {
                for (int i = 0; i < currentFilesAndFolders.Count; i++)
                {
                    if (!currentFilesAndFolders[i].Equals(FileFolderList[i]))
                    {
                        currentFilesAndFolders = FileFolderList;
                        listChanged = true;
                        break;
                    }
                }
            }

            return listChanged;
        }

        private void AddToListView(int index, string name, CmdLineType type, bool check = false)
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
            //ListView_SizeChanged(null, null);
        }

        private void AddToListView(int index, CMDFileFolder ff, bool check = false)
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
            //ListView_SizeChanged(null, null);
        }

        private void mainForm_Activated(object sender, EventArgs e)
        {
            if (_activated) return;
            _activated = true;

            CreateHandles();

            GetStatusFromServer();
            StatusTimer = PefromTaskEveryXTime(GetStatusFromServer, 1);
            FolderListTimer = PefromTaskEveryXTime(GetFolderListFromServer, 1);
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (userActionDisabled)
            {
                DisplayMessageDoOperationWhileDonwloadUpload();
                return;
            }

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
        }

        private void OptimisticTryAndFail(Action act, int timeBetweenTry, int numOfTry, string exception)
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

        private void currentPathTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (userActionDisabled)
            {
                DisplayMessageDoOperationWhileDonwloadUpload();
                return;
            }

            if (e.KeyCode != Keys.Enter) return;
            var textBox = sender as TextBox;
            var newPath = textBox.Text;
            var newPathCharArr = newPath.ToCharArray();
            var isInTheDrive = currentPath.Contains(newPath);
            if (newPathCharArr.Last() == '\\' && !isInTheDrive)
                newPath = newPath.Remove(newPathCharArr.Length - 1);
            var isNewPathDrive = !isInTheDrive && newPath.Length == 2 && newPath.ElementAt(1) == ':';
            var resp = shellService.ActiveNextCommand(isNewPathDrive ? newPath : string.Format("cd \"{0}\"", newPath));
            var splitedPath = resp.Replace("\r", "").Split('\n');
            splitedPath = splitedPath.Where(str => !string.IsNullOrEmpty(str) && !string.IsNullOrWhiteSpace(str)).ToArray();
            var returnPath = isNewPathDrive ? newPath : splitedPath.Last();
            if (returnPath.ToLower() != newPath.ToLower() && returnPath.ToLower() != newPath.ToLower().Substring(0, newPath.Length - 1))
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
            if (userActionDisabled)
            {
                DisplayMessageDoOperationWhileDonwloadUpload();
                return;
            }

            Environment.Exit(0);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var about = new AboutBox())
            {
                about.ShowDialog();
            }
        }

        private void logOutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (userActionDisabled)
            {
                DisplayMessageDoOperationWhileDonwloadUpload();
                return;
            }

            _loginFrom.Logout();
            this.Close();
        }

        private void CloseAllThreads()
        {
            StatusTimer.Dispose();
            FolderListTimer.Dispose();
        }

        private void mainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            CloseAllThreads();
            CloseAllConnections();
        }

        private void DisplayMessageDoOperationWhileDonwloadUpload()
        {
            if (userActionDisabled)
            {
                MessageBox.Show("Can not do any operation while downloading or uploading.", "Invalid Operation", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RunInAnotherThread(Action task, Action callback)
        {
            Task.Factory.StartNew(() =>
            {
                task();
            }).ContinueWith(t => callback?.Invoke());
        }

        private void CreateHandles()
        {
            if (!IsHandleCreated)
            {
                CreateControl();
            }
            if (!downloadUploadProgressBar.IsHandleCreated)
            {
                downloadUploadProgressBar.CreateControl();
            }
        }
    }
}
