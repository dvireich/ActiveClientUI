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
    public partial class mainForm : ListViewForm
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
        private List<CMDFileFolder> currentFilesAndFolders;
        private bool userActionDisabled;
        private bool ShouldChangePathTextBox;
        private Object statusFromServerLock = new Object();
        private Object folderListFromServerLock = new Object();

        private Dictionary<string, Action<ListView, ListViewItem>> _userClickToAction;
        private List<string> _columnNames;

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

        private void InitializeColNames()
        {
            _columnNames = new List<string>()
            {
                "Type",
                "Name",
                "Size",
                "Last file modification"
            };
        }

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
            InitializeServiceReferences(_wcfServicesPathId);
            InitializeUserClickToAction();
            InitializeColNames();
            CreateListView();
        }

        private void EnterClickEventHandler(ListView selectedListViewProperty, ListViewItem selected)
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
        }

        private void ReanmeClickEventHandler(ListView selectedListViewProperty, ListViewItem selected)
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
        }

        private void DeleteClickEventHandler(ListView selectedListViewProperty, ListViewItem selected)
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
        }

        private void CopyClickEventHandler(ListView selectedListViewProperty, ListViewItem selected)
        {
            if (userActionDisabled)
            {
                DisplayMessageDoOperationWhileDonwloadUpload();
                return;
            }

            cutPath = null;
            copyPath = Path.Combine(currentPath, selectedListViewProperty.GetFromListViewAndListViewItemByColumnName(selected, "Name").ToString());
        }

        private void CutClickEventHandler(ListView selectedListViewProperty, ListViewItem selected)
        {
            if (userActionDisabled)
            {
                DisplayMessageDoOperationWhileDonwloadUpload();
                return;
            }

            copyPath = null;
            cutPath = Path.Combine(currentPath, selectedListViewProperty.GetFromListViewAndListViewItemByColumnName(selected, "Name"));
        }

        private void PasteClickEventHandler(ListView selectedListViewProperty, ListViewItem selected)
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
        }

        private void DownloadClickEventHandler(ListView selectedListViewProperty, ListViewItem selected)
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
                            var downloadData = new DownloadUpLoadData()
                            {
                                Shell = shellService,
                                Label = downloadUploadLable,
                                ProgressBar = downloadUploadProgressBar,
                                FileName = fileName,
                                PathInServer = currentPath,
                                PathToSaveInClinet = saveForm.SelectedPath

                            };

                            DownLoadFile(downloadData);
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
        }

        private void UploadClickEventHandler(ListView selectedListViewProperty, ListViewItem selected)
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

                        var uploadData = new DownloadUpLoadData()
                        {
                            Shell = shellService,
                            Label = downloadUploadLable,
                            ProgressBar = downloadUploadProgressBar,
                            FileName = fileName,
                            Directory = path,
                            PathToSaveOnServer = currentPath
                        };

                        UpLoadFile(uploadData);

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
        }

        private ContextMenuOvveride CreatePopUpMenu(CmdLineType type)
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
                if (type == CmdLineType.File && userClickActionName == "Enter" ||
                    type == CmdLineType.Folder && userClickActionName == "Download") continue;

                    var menuItemData = new MenuItemData()
                {
                    Header = userClickActionName,
                    OnClick = action,
                    SelectedListViewItem = selected,
                    SelectedListViewProperty = selectedListViewProperty,
                };

                menuItemDataList.Add(menuItemData);
            }

            return CreatePopUpMenu(menuItemDataList);
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

            CreateListView(listView1, smallImageData, largeImageData, _columnNames);

        }

        private void DetailsToolStripMenuItem_Click(object sender, EventArgs e)
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

        private void SmallIconsToolStripMenuItem_Click(object sender, EventArgs e)
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

        private void LargIconsToolStripMenuItem_Click(object sender, EventArgs e)
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

        private void ListView1_SelectedIndexChanged(object sender, EventArgs e)
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

        private void ListView1_MouseClick(object sender, MouseEventArgs e)
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

        private void InitializeServiceReferences(string wcfServicesPathId)
        {
            shellService = initializeServiceReferences<IActiveShell>(wcfServicesPathId);
            getStatusShellService = initializeServiceReferences<IActiveShell>(wcfServicesPathId);
            getFolderListShellService = initializeServiceReferences<IActiveShell>(wcfServicesPathId);
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

                            AddToListView(0, new CMDFileFolder(CmdLineType.Folder, "..", null, null));

                            var index = 1;
                            foreach (var item in FileFolderList)
                            {
                                AddToListView(index++, item);
                            }
                            if (oldFocusedIndex > -1 && listView1 != null && listView1.Items != null && oldFocusedIndex < listView1.Items.Count)
                            {
                                listView1.Items[oldFocusedIndex].Selected = true;
                            }
                            ListView_SizeChanged(listView1, null);
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
                }
                catch (Exception)
                {
                    MessageBox.Show("No connection to the Server", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            currentPathTextBox.KeyDown -= CurrentPathTextBox_KeyDown;
            if (!ShouldChangePathTextBox)
                this.Invoke((MethodInvoker)(() => currentPathTextBox.Text = currentPath));
            this.Invoke((MethodInvoker)(() =>
                    this.Width = Math.Max(TextRenderer.MeasureText(currentPathTextBox.Text, currentPathTextBox.Font).Width + 100, this.Width)));
            currentPathTextBox.KeyDown += CurrentPathTextBox_KeyDown;
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

        private void AddToListView(int index, CMDFileFolder ff, bool check = false)
        {
            var listViewItemData = new ListViewItemData()
            {
                Index = index,
                check = check,
                imageIndex = (int)ff.GetType(),
                MainItem = listView1.View != View.Details ? ff.GetName() : ff.GetType().ToString(),
                SubItems = new List<string>(){
                                                listView1.View == View.Details ? ff.GetName() : ff.GetType().ToString(),
                                                ff.GetType() == CmdLineType.Folder ? string.Empty : ff.getSize().ToString(),
                                                ff.GetLastModificationDate()}
            };

            AddToListView(listView1, listViewItemData);
        }

        private void MainForm_Activated(object sender, EventArgs e)
        {
            if (_activated) return;
            _activated = true;

            listView1.SizeChanged += new EventHandler(ListView_SizeChanged);
            currentPathTextBox.GotFocus += CurrentPathTextBox_Enter;
            currentPathTextBox.LostFocus += CurrentPathTextBox_Leave;

            GetStatusFromServer();
            PefromTaskEveryXTime(GetStatusFromServer, 1);
            PefromTaskEveryXTime(GetFolderListFromServer, 1);
        }

        private void ListView1_MouseDoubleClick(object sender, MouseEventArgs e)
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

        private void CurrentPathTextBox_KeyDown(object sender, KeyEventArgs e)
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

        private void CurrentPathTextBox_Leave(object sender, EventArgs e)
        {
            ShouldChangePathTextBox = false;
        }

        private void CurrentPathTextBox_Enter(object sender, EventArgs e)
        {
            if (NoSelectedClient.Visible) return;
            ShouldChangePathTextBox = true;
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (userActionDisabled)
            {
                DisplayMessageDoOperationWhileDonwloadUpload();
                return;
            }

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
            if (userActionDisabled)
            {
                DisplayMessageDoOperationWhileDonwloadUpload();
                return;
            }

            _loginFrom.Logout();
            this.Close();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
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
    }
}
