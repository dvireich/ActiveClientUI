using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFormsApp1.ServiceReference1;

namespace WindowsFormsApp1
{
    public class DownloadUploadForm : CommunicationForm
    {
        public void UpLoadFile(DownloadUpLoadData upLoadFileData)
        {
            var path = Path.Combine(upLoadFileData.Directory, upLoadFileData.FileName);
            FileInfo fileInfo = new FileInfo(path);
            RemoteFileInfo uploadRequestInfo = new RemoteFileInfo();
            uploadRequestInfo.FileName = upLoadFileData.FileName;
            uploadRequestInfo.Length = fileInfo.Length;
            uploadRequestInfo.PathToSaveOnServer = upLoadFileData.PathToSaveOnServer;
            uploadRequestInfo.FreshStart = true;

            Invoke((MethodInvoker)(() =>
            {
                if(upLoadFileData.ProgressBar != null)
                {
                    upLoadFileData.ProgressBar.Visible = true;
                    upLoadFileData.ProgressBar.Value = 0;
                }
                if(upLoadFileData.Label != null)
                {
                    upLoadFileData.Label.Visible = true;
                    upLoadFileData.Label.Text = string.Format("Upload Process: Uploading from this PC to the server memory...");
                    upLoadFileData.Label.Refresh();
                }             
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
                            upLoadFileData.Shell.StartTransferData();
                            upLoadFileData.Shell.ActiveUploadFile(uploadRequestInfo);
                        });

                        Invoke((MethodInvoker)(() =>
                        {
                            if(upLoadFileData.ProgressBar != null)
                            {
                                upLoadFileData.ProgressBar.Value = (int)((file.Position / (double)file.Length) * 100);
                            }
                        }));

                        uploadRequestInfo.FreshStart = false;

                    }
                }
                uploadRequestInfo.FileByteStream = new byte[0];
                uploadRequestInfo.FileEnded = true;
                file.Close();

                Invoke((MethodInvoker)(() =>
                {
                    if (upLoadFileData.ProgressBar != null)
                    {
                        upLoadFileData.ProgressBar.Value = 100;
                    }
                    if(upLoadFileData.Label != null)
                    {
                        upLoadFileData.Label.Text = string.Format("Upload Process: Transfering File from server Memory to remote client PC...");
                    }
                    if (upLoadFileData.ProgressBar != null)
                    {
                        upLoadFileData.ProgressBar.Value = 0;
                    }
                }));

                var response = SendRequestAndTryAgainIfTimeOutOrEndpointNotFound(() => upLoadFileData.Shell.ActiveUploadFile(uploadRequestInfo));

                if (response.FileName.StartsWith("Error"))
                {
                    throw new Exception(response.FileName);
                }
                if (response.FileName.StartsWith("Buffering") && response.FileByteStream.Length == 0)
                {
                    var precentage = response.FileName.Split(new string[] { "Memory" }, StringSplitOptions.RemoveEmptyEntries).Last().Trim().Split(' ').First();

                    Invoke((MethodInvoker)(() =>
                    {
                        if (upLoadFileData.ProgressBar != null)
                        {
                            upLoadFileData.ProgressBar.Value = int.Parse(precentage);
                        }
                    }));

                }
                if (response.FileName == "Upload Ended")
                    return;
            }
            FinishUpload(upLoadFileData);
        }

        private void FinishUpload(DownloadUpLoadData finishUploadData)
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

            var response = SendRequestAndTryAgainIfTimeOutOrEndpointNotFound(() => finishUploadData.Shell.ActiveUploadFile(requestData));
            if (response.FileName.StartsWith("Buffering") && response.FileByteStream.Length == 0)
            {
                var precentage = response.FileName.Split(new string[] { "Memory" }, StringSplitOptions.RemoveEmptyEntries).Last().Trim().Split(' ').First();
                Invoke((MethodInvoker)(() =>
                {
                    if (finishUploadData.ProgressBar != null)
                    {
                        finishUploadData.ProgressBar.Value = int.Parse(precentage);
                    }
                }));
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
                    response = response = SendRequestAndTryAgainIfTimeOutOrEndpointNotFound(() => finishUploadData.Shell.ActiveUploadFile(requestData));
                    if (response.FileName.StartsWith("Buffering") && response.FileByteStream.Length == 0)
                    {
                        var precentage = response.FileName.Split(new string[] { "Memory" }, StringSplitOptions.RemoveEmptyEntries).Last().Trim().Split(' ').First();
                        Invoke((MethodInvoker)(() =>
                        {
                            if (finishUploadData.ProgressBar != null)
                            {
                                finishUploadData.ProgressBar.Value = int.Parse(precentage);
                            }
                        }));
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

        private RemoteFileInfo ReadyToStartDownload(DownloadUpLoadData readyToStartDownloadData)
        {
            Invoke((MethodInvoker)(() =>
            {
                if(readyToStartDownloadData.ProgressBar != null)
                {
                    readyToStartDownloadData.ProgressBar.Visible = true;
                }
                if(readyToStartDownloadData.Label != null)
                {
                    readyToStartDownloadData.Label.Visible = true;
                    readyToStartDownloadData.Label.Text = string.Format("Download process: Uploading the file from remote PC to the server memory...");
                    readyToStartDownloadData.Label.Refresh();
                }
            }));

            var watch = new Stopwatch();
            double lastStop = 0;
            DownloadRequest requestData = new DownloadRequest();
            requestData.FileName = readyToStartDownloadData.FileName;
            requestData.PathInServer = readyToStartDownloadData.PathInServer;
            requestData.NewStart = true;
            var response = SendRequestAndTryAgainIfTimeOutOrEndpointNotFound(() => readyToStartDownloadData.Shell.ActiveDownloadFile(requestData));
            if (response.FileName.StartsWith("Buffering") && response.FileByteStream.Length == 0)
            {
                var precentage = response.FileName.Split(new string[] { "Memory" }, StringSplitOptions.RemoveEmptyEntries).Last().Trim().Split(' ').First();

                Invoke((MethodInvoker)(() =>
                {
                    if (readyToStartDownloadData.ProgressBar != null)
                    {
                        readyToStartDownloadData.ProgressBar.Value = int.Parse(precentage);
                    }
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
                    response = SendRequestAndTryAgainIfTimeOutOrEndpointNotFound(() => readyToStartDownloadData.Shell.ActiveDownloadFile(requestData));
                    if (response.FileName.StartsWith("Buffering") && response.FileByteStream.Length == 0)
                    {
                        var precentage = response.FileName.Split(new string[] { "Memory" }, StringSplitOptions.RemoveEmptyEntries).Last().Trim().Split(' ').First();

                        Invoke((MethodInvoker)(() =>
                        {
                            if (readyToStartDownloadData.ProgressBar != null)
                            {
                                readyToStartDownloadData.ProgressBar.Value = int.Parse(precentage);
                            }

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

        public void DownLoadFile(DownloadUpLoadData downLoadFileData)
        {
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
                    Console.WriteLine("Fail to create File in your computer " + downLoadFileData.FileName);
                    return;
                }

                var fileInfo = ReadyToStartDownload(downLoadFileData);

                Invoke((MethodInvoker)(() =>
                {
                    if (downLoadFileData.ProgressBar != null)
                    {
                        downLoadFileData.ProgressBar.Value = 100;
                    }
                    if (downLoadFileData.Label != null)
                    {
                        downLoadFileData.Label.Text = string.Format("Download process:  Transfering File from Server Memory to your PC...");
                    }
                    if (downLoadFileData.ProgressBar != null)
                    {
                        downLoadFileData.ProgressBar.Value = 0;
                    }
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

                    SendRequestAndTryAgainIfTimeOutOrEndpointNotFound(() => fileInfo = downLoadFileData.Shell.ActiveDownloadFile(requestData));
                    if (!fileInfo.FileEnded)
                    {
                        Invoke((MethodInvoker)(() =>
                        {
                            if (downLoadFileData.ProgressBar != null)
                            {
                                downLoadFileData.ProgressBar.Value = (int)((fileStrem.Position / double.Parse(fileInfo.FileSize)) * 100);
                            }
                        }));
                    }
                    else
                    {
                        Invoke((MethodInvoker)(() =>
                        {
                            if (downLoadFileData.ProgressBar != null)
                            {
                                downLoadFileData.ProgressBar.Value = 100;
                            }
                        }));
                    }
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
    }
}
