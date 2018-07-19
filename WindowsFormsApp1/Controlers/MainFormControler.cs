using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFormsApp1.Authentication;
using WindowsFormsApp1.Interfaces;
using WindowsFormsApp1.ServiceReference1;

namespace WindowsFormsApp1
{
    public class MainFormControler : CommunicationControler
    {
        IMainView _view;

        private View _currentView;

        private string _cutPath;
        private string _copyPath;
        private string _currentPath;

        private bool _selectedRemoteClientConnected;
        private bool _cmdOpenedRemoteClientPc;
        private bool _blockAllOperations;

        private List<FileFolder> _currentFilesFoldersList;
        private List<FileFolder> CurrentFileFolderList
        {
            get
            {
                if (_currentFilesFoldersList == null)
                {
                    _currentFilesFoldersList = new List<FileFolder>();
                }
                return _currentFilesFoldersList;
            }
            set
            {
                _currentFilesFoldersList = value;
            }
        }

        public MainFormControler(string endpointId, IMainView view) : base(endpointId)
        {
            _view = view;
            _view.EnableViewModification = true;
            _view.ShouldChangeCurrentPathText = true;
            _currentView = _view.CurrentView;
        }

        private List<FileFolder> CalculateFileOrFolderData(List<string> folderList)
        {
            var FileFolderList = new List<FileFolder>();

            var dirData = _proxyService.ActiveNextCommand("dir").Replace("\r", "");
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
                    var folder = new FileFolder(FileFolderType.Folder, lineSplit[3], string.Empty, string.Format("{0} {1}", lineSplit[0], lineSplit[1]));
                    FileFolderList.Add(folder);
                }
                else
                {

                    var persedNumber = long.Parse(lineSplit[2].Replace(",", ""));
                    var fileSize = string.Format("{0} KB", ConvertBytesToKB(persedNumber));
                    var modificationDate = string.Format("{0} {1}", lineSplit[0], lineSplit[1]);
                    var file = new FileFolder(FileFolderType.File, lineSplit[3], fileSize, modificationDate);
                    FileFolderList.Add(file);
                }
            }

            return FileFolderList;
        }

        private List<string> GetFileListFromFolderList(List<string> folderList)
        {
            var allListStr = _proxyService.ActiveNextCommand("dir /b").Replace("\r", "");
            var allList = allListStr.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            allList.RemoveAt(allList.Count - 1);
            allList.RemoveAt(0);
            allList.RemoveAll(file => folderList.Contains(file));
            return allList;
        }

        private List<string> GetFolderListFromServerAndAssignTheFolderPath(string folderListStr)
        {
            var folderList = folderListStr.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            _currentPath = folderList.ElementAt(folderList.Count - 1);
            if (_view.ShouldChangeCurrentPathText)
            {
                _view.CurrentPathTextBoxText = _currentPath;
            }

            _view.FormWidth = Math.Max(TextRenderer.MeasureText(_view.CurrentPathTextBoxText, _view.CurrentPathTextBoxFont).Width + 100, _view.FormWidth);
            folderList.RemoveAt(folderList.Count - 1);
            folderList.RemoveAt(0);
            return folderList;
        }

        private List<FileFolder> GetFileFolderList()
        {
            OpenCmdOnRemoteClientPc();

            string folderListStr = string.Empty;
            try
            {
                folderListStr = _proxyService.ActiveNextCommand("dir /b /ad").Replace("\r", "");
            }
            catch
            {
                _view.DisplayMessage(MessageType.Error, "No Connection", "There is no connection to the server!. Please check server state");
                return null;
            }

            if (folderListStr == "Client CallBack is Not Found" ||
                    folderListStr.StartsWith("Error") ||
                    folderListStr == "The communication object, System.ServiceModel.Channels.ServiceChannel, cannot be used for communication because it has been Aborted.")
            {
                _cmdOpenedRemoteClientPc = false;
                return null;
            }

            var folderList = GetFolderListFromServerAndAssignTheFolderPath(folderListStr);

            var fileList = GetFileListFromFolderList(folderList);

            var fileFolderData =  CalculateFileOrFolderData(folderList);

            fileFolderData.Insert(0, new FileFolder(FileFolderType.Folder, "..", "0" , string.Empty));

            return fileFolderData;
        }

        public void UpdateFileFolderList()
        {
            List<IShowable> data;
            if (!_selectedRemoteClientConnected) return;

            var ffl = GetFileFolderList();
            if (ffl == null) return;
            if (!ffl.IsDiffrentFrom(CurrentFileFolderList) && _currentView == _view.CurrentView) return;

            CurrentFileFolderList = ffl;
            _currentView = _view.CurrentView;

            if (_view.CurrentView == View.Details)
            {
                data = ffl.Select(ff => new DetailsViewFileFolder(ff.GetType(), ff.GetName(), ff.getSize(), ff.GetLastModificationDate())).ToList<IShowable>();
            }
            else
            {
                data = ffl.ToList<IShowable>();
            }

            _view.ShowData(data);
        }

        public void OpenCmdOnRemoteClientPc()
        {
            if (_cmdOpenedRemoteClientPc) return;

            if (string.IsNullOrEmpty(_proxyService.ActiveClientRun())) return;
            _cmdOpenedRemoteClientPc = true;
        }

        public void UpdateUsersStatus()
        {
            var status = _proxyService.GetStatus();
            status = ParseStatus(status, out string[] statusSplittedNewLine, out bool isSelectedClientAlive, out string selectedClient);

            if (statusSplittedNewLine.Length == 2 && statusSplittedNewLine[1] == "There is no clients connected" ||
                selectedClient != null && !isSelectedClientAlive)
            {
                _selectedRemoteClientConnected = false;
                _view.NoSelectedClientLabelVisible = true;
                _view.CurrentPathTextBoxText = string.Empty;
                _view.ListViewVisible = false;
            }
            else
            {
                _selectedRemoteClientConnected = true;
                _view.NoSelectedClientLabelVisible = false;
                _view.ListViewVisible = true;
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

        private static long ConvertBytesToKB(long bytes)
        {
            var ans = (int)(bytes / 1024);
            return ans == 0 ? 1 : ans;
        }

        //Events

        public void Reanme(string oldName, string newName)
        {
            if(_blockAllOperations)
            {
                _view.DisplayMessage(MessageType.Error, "Operations blocked", "No operation can be done at this moment");
                return;
            }

            var result = _proxyService.ActiveNextCommand(string.Format("rename \"{0}\" \"{1}\"", oldName, newName));
            if (result.StartsWith("Error"))
            {
                _view.DisplayMessage(MessageType.Error, "Rename", $"Error in rename: {result}");
                return;
            }
            UpdateFileFolderList();
        }

        public void Delete(string name)
        {
            if (_blockAllOperations)
            {
                _view.DisplayMessage(MessageType.Error, "Operations blocked", "No operation can be done at this moment");
                return;
            }

            var result = _proxyService.ActiveNextCommand(string.Format("del \"{0}\"", name));
            if (result.StartsWith("Error"))
            {
                _view.DisplayMessage(MessageType.Error, "Delete", $"Error in Delete: {result}");
                return;
            }
            UpdateFileFolderList();
        }

        public void Copy(string name)
        {
            if (_blockAllOperations)
            {
                _view.DisplayMessage(MessageType.Error, "Operations blocked", "No operation can be done at this moment");
                return;
            }
            _cutPath = null;
            _copyPath = Path.Combine(_currentPath, name);
        }

        public void Cut(string name)
        {
            if (_blockAllOperations)
            {
                _view.DisplayMessage(MessageType.Error, "Operations blocked", "No operation can be done at this moment");
                return;
            }
            _copyPath = null;
            _cutPath = Path.Combine(_currentPath, name);
        }

        public void Paste(string fileFolderType, string selectedName)
        {
            if (_blockAllOperations)
            {
                _view.DisplayMessage(MessageType.Error, "Operations blocked", "No operation can be done at this moment");
                return;
            }

            if (!Enum.TryParse(fileFolderType, out FileFolderType type))
            {
                _view.DisplayMessage(MessageType.Error, "Unrecognized type ", $"The type: {fileFolderType} of: {selectedName} is unrecognized");
                return;
            }

            var pathToPaste = type == FileFolderType.Folder ? Path.Combine(_currentPath, selectedName) : _currentPath;
            TryToCutPaste(pathToPaste);
            TryToCopyPaste(pathToPaste);

            UpdateFileFolderList();
        }

        private void TryToCutPaste(string path)
        {
            if (string.IsNullOrEmpty(_cutPath)) return;

            var result = _proxyService.ActiveNextCommand(string.Format("copy \"{0}\" \"{1}\" /Y", _cutPath, path));
            if (result.StartsWith("Error"))
            {
                _view.DisplayMessage(MessageType.Error, "Pates", $"Error in paste: {result}");
                return;
            }


            result = _proxyService.ActiveNextCommand(string.Format("del \"{0}\"", _cutPath));
            if (result.StartsWith("Error"))
            {
                _view.DisplayMessage(MessageType.Error, "Pates", $"Error in paste: {result}");
                return;
            }
        }

        private void TryToCopyPaste(string path)
        {
            var result = _proxyService.ActiveNextCommand(string.Format("copy \"{0}\" \"{1}\" /-Y", _copyPath, path));
            if (result.StartsWith("Error"))
            {
                _view.DisplayMessage(MessageType.Error, "Pates", $"Error in paste: {result}");
                return;
            }
        }

        public void ChangeWorkingDirectoryPath(string relativeOrAbsolutePath)
        {
            if (relativeOrAbsolutePath == "..")
            {
                _proxyService.ActiveNextCommand("cd..");
            }
            else
            {
                _proxyService.ActiveNextCommand(string.Format("cd \"{0}\"", relativeOrAbsolutePath));
            }
        }

        public void CheckCurrentPathAndChangeWorkingDirectoryIfValid()
        {
            var newPath = _view.CurrentPathTextBoxText;

            var newPathCharArr = newPath.ToCharArray();
            var isInTheDrive = _currentPath.Contains(newPath);
            if (newPathCharArr.Last() == '\\' && !isInTheDrive)
                newPath = newPath.Remove(newPathCharArr.Length - 1);

            var isNewPathDrive = !isInTheDrive && newPath.Length == 2 && newPath.ElementAt(1) == ':';

            var resp = _proxyService.ActiveNextCommand(isNewPathDrive ? newPath : string.Format("cd \"{0}\"", newPath));

            var splitedPath = resp.Replace("\r", "").Split('\n');
            splitedPath = splitedPath.Where(str => !string.IsNullOrEmpty(str) && !string.IsNullOrWhiteSpace(str)).ToArray();
            var returnPath = isNewPathDrive ? newPath : splitedPath.Last();
            if (returnPath.ToLower() != newPath.ToLower() && returnPath.ToLower() != newPath.ToLower().Substring(0, newPath.Length - 1))
            {
                _view.DisplayMessage(MessageType.Error, "Invalid path", $"Error in changing path. The path: {newPath} is invalid");
            }
        }

        public bool Exit(string username)
        {
            if (_blockAllOperations)
            {
                _view.DisplayMessage(MessageType.Error, "Operations blocked", "No operation can be done at this moment");
                return false;
            }

            return Logout(username);
        }

        public bool Logout(string username)
        {
            if (_blockAllOperations) return false;

            var resp = _authenticationProxy.Logout(new LogoutRequest()
            {
                userName = username,
                userType = "ActiveClient"
            });
            if (resp.LogoutResult) return true;

            _view.DisplayMessage(MessageType.Error, "Logut", $"Error in logout for user: {username} with the error: {resp.error}");
            return false;
        }

        //Transfer Data

        public void DownLoad(DownloadUpLoadData downLoadFileData)
        {
            _blockAllOperations = true;
            _view.EnableViewModification = false;

            DownloadRequest requestData = new DownloadRequest();
            requestData.FileName = downLoadFileData.FileName;
            requestData.PathInServer = downLoadFileData.PathInServer;
            requestData.NewStart = false;
            requestData.id = string.Empty;
            requestData.taskId = string.Empty;
            requestData.PathToSaveInClient = string.Empty;
            using (var fileStrem = CreateNewFile(downLoadFileData.FileName, downLoadFileData.PathToSaveInClinet))
            {
                if (fileStrem == null)
                {
                    _view.DisplayMessage(MessageType.Error, "Create local file", $"Could not create: {downLoadFileData.FileName} on your PC");
                    _view.EnableViewModification = true;
                    _blockAllOperations = false;
                    return;
                }

                var fileInfo = WaitUntilDataFullyBufferedOnServerMemory(downLoadFileData, out bool error);
                if (error) return;

                _view.ProgressBarValue = 100;
                _view.ProgressLabelText = "Download process:  Transfering File from Server Memory to your PC...";
                _view.ProgressBarValue = 0;

                while (true)
                {
                    if (fileInfo.FileName.StartsWith("Error"))
                    {
                        _view.DisplayMessage(MessageType.Error, "Download", $"Error in download: {downLoadFileData.FileName}");
                        error = true;
                        break;
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

                    SendRequestAndTryAgainIfTimeOutOrEndpointNotFound(() => fileInfo = _proxyService.ActiveDownloadFile(requestData));
                    if (!fileInfo.FileEnded)
                    {
                        _view.ProgressBarValue = (int)((fileStrem.Position / double.Parse(fileInfo.FileSize)) * 100);
                    }
                    else
                    {
                        _view.ProgressBarValue = 100;
                    }
                }
                _blockAllOperations = false;
                _view.EnableViewModification = true;

                if (error) return;
                
                _view.ProgressBarValue = 100;
                _view.DisplayMessage(MessageType.Info, "Download", "Download completed successfully");
                _view.ProgressBarVisible = false;
                _view.ProgressLabelVisible = false;
                _view.ProgressBarValue = 0;
            }
        }

        private RemoteFileInfo WaitUntilDataFullyBufferedOnServerMemory(DownloadUpLoadData downloadData, out bool error)
        {
            error = false;
            _view.ProgressBarVisible = true;
            _view.ProgressLabelText = "Download process: Uploading the file from remote PC to the server memory...";
            _view.ProgressLabelVisible = true;

            var watch = new Stopwatch();
            double lastStop = 0;
            DownloadRequest requestData = new DownloadRequest();
            requestData.FileName = downloadData.FileName;
            requestData.PathInServer = downloadData.PathInServer;
            requestData.NewStart = true;
           
            
            watch.Start();
            var periodToCheckInTheServer = 1;
            while (true)
            {
                if (watch.Elapsed.TotalSeconds - lastStop >= periodToCheckInTheServer)
                {
                    lastStop = watch.Elapsed.TotalSeconds;
                    var response = SendRequestAndTryAgainIfTimeOutOrEndpointNotFound(() => _proxyService.ActiveDownloadFile(requestData));
                    if (response.FileName.StartsWith("Buffering") && response.FileByteStream.Length == 0)
                    {
                        var precentage = response.FileName.Split(new string[] { "Memory" }, StringSplitOptions.RemoveEmptyEntries).Last().Trim().Split(' ').First();
                        _view.ProgressBarValue = int.Parse(precentage);

                    }
                    else if (response.FileName.StartsWith("Error"))
                    {
                        _view.DisplayMessage(MessageType.Error, "Download", $"Error in Buffreing file in server memory:  {response.FileName}");
                        _blockAllOperations = false;
                        _view.EnableViewModification = true;
                        error = true;
                        break;
                    }
                    else
                    {
                        return response;
                    }
                }
                requestData.NewStart = false;
            }
            return null;
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

        public void UpLoadFile(DownloadUpLoadData upLoadFileData)
        {
            _blockAllOperations = true;
            _view.EnableViewModification = false;

            var path = Path.Combine(upLoadFileData.Directory, upLoadFileData.FileName);
            FileInfo fileInfo = new FileInfo(path);
            RemoteFileInfo uploadRequestInfo = new RemoteFileInfo();
            uploadRequestInfo.FileName = upLoadFileData.FileName;
            uploadRequestInfo.Length = fileInfo.Length;
            uploadRequestInfo.PathToSaveOnServer = upLoadFileData.PathToSaveOnServer;
            uploadRequestInfo.FreshStart = true;

            _view.ProgressBarVisible = true;
            _view.ProgressBarValue = 0;
            _view.ProgressLabelText = "Upload Process: Uploading from this PC to the server memory...";
            _view.ProgressLabelVisible = true;

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
                            _proxyService.StartTransferData();
                            _proxyService.ActiveUploadFile(uploadRequestInfo);
                        });

                        _view.ProgressBarValue = (int)((file.Position / (double)file.Length) * 100);
                        uploadRequestInfo.FreshStart = false;
                    }
                }
                uploadRequestInfo.FileByteStream = new byte[0];
                uploadRequestInfo.FileEnded = true;
                file.Close();

                _view.ProgressBarValue = 100;
                _view.ProgressBarValue = 0;
                _view.ProgressLabelText = "Upload Process: Transfering File from server Memory to remote client PC...";

                var response = SendRequestAndTryAgainIfTimeOutOrEndpointNotFound(() => _proxyService.ActiveUploadFile(uploadRequestInfo));

                if (response.FileName.StartsWith("Error"))
                {
                    _view.DisplayMessage(MessageType.Error, "Error in upload", $"Error in buffering file in remote client memory: {response.FileName}");
                    _blockAllOperations = false;
                    _view.EnableViewModification = true;
                    return;
                }
                if (response.FileName.StartsWith("Buffering") && response.FileByteStream.Length == 0)
                {
                    var precentage = response.FileName.Split(new string[] { "Memory" }, StringSplitOptions.RemoveEmptyEntries).Last().Trim().Split(' ').First();
                    _view.ProgressBarValue = int.Parse(precentage);
                }
                if (response.FileName == "Upload Ended")
                {
                    _blockAllOperations = false;
                    _view.EnableViewModification = true;
                    return;
                }
            }
            WaitUntilDataFullyBuffredInRemoteClientMemory();
            _blockAllOperations = false;
            _view.EnableViewModification = true;
        }

        private void WaitUntilDataFullyBuffredInRemoteClientMemory()
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

            watch.Start();
            var periodToCheckInTheServer = 1;
            while (true)
            {
                if (watch.Elapsed.TotalSeconds - lastStop >= periodToCheckInTheServer)
                {
                    lastStop = watch.Elapsed.TotalSeconds;
                     var response = SendRequestAndTryAgainIfTimeOutOrEndpointNotFound(() => _proxyService.ActiveUploadFile(requestData));
                    if (response.FileName.StartsWith("Buffering") && response.FileByteStream.Length == 0)
                    {
                        var precentage = response.FileName.Split(new string[] { "Memory" }, StringSplitOptions.RemoveEmptyEntries).Last().Trim().Split(' ').First();
                        _view.ProgressBarValue = int.Parse(precentage);
                    }
                    else if (response.FileName.StartsWith("Error"))
                    {
                        _view.DisplayMessage(MessageType.Error, "Error in upload", $"Error in buffering file in remote client memory: {response.FileName}");
                        return;
                    }
                    else
                    {
                        _view.ProgressBarValue = 100;
                        _view.DisplayMessage(MessageType.Info, "Upload", "Upload completed successfully");
                        _view.ProgressBarVisible = false;
                        _view.ProgressLabelVisible = false;
                        _view.ProgressBarValue = 0;
                        return;
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

    }
}
