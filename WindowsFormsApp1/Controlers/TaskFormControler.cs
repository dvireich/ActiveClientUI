using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsFormsApp1.DataModel;
using WindowsFormsApp1.DataModel.Enums;
using WindowsFormsApp1.Interfaces;

namespace WindowsFormsApp1.Controlers
{
    class TaskFormControler : CommunicationControler
    {
        ITaskView _view;

        private List<string> lastShellTasks;
        private List<string> lastDownloadUploadTasks;
        private string _selectedClient;
        private TaskType _currentType;

        public TaskFormControler(TaskType currentType, string selectedClient, string endpointId, ITaskView view) : base(endpointId)
        {
            _currentType = currentType;
            _selectedClient = selectedClient;
            _view = view;
        }

        public void UpdateTaskData()
        {
            var status = _proxyService.GetStatus();

            status = status.Replace("\r", "");
            var statusSplittedNewLine = status.Split('\n');
            statusSplittedNewLine = statusSplittedNewLine.Where(str => !string.IsNullOrEmpty(str) && !string.IsNullOrWhiteSpace(str)).ToArray();

            if (!CheckIfAnyClientConnectedAndShowLablelIfNot(statusSplittedNewLine)) return;

            var clients = status.Split(new string[] { "Client" }, StringSplitOptions.RemoveEmptyEntries);
            clients = clients.ToArray().Skip(1).Take(clients.Count() - 2).ToArray();

            foreach (var client in clients)
            {
                if (!ParseClientTasks(client, out List<string> shellTasks, out List<string> downloadUploadTasks)) continue;

                if (!CheckIfIShellTasksOrDownloadUploadTasksChanged(shellTasks, downloadUploadTasks)) return;

                _view.NoTasksVisible = false;
                _view.ListViewVisible = true;
                var listToShow = _currentType == TaskType.Shell ? ConvertToListOfShellTask(shellTasks).ToList<IShowable>() :
                                                                  ConvertToListOfDownloadUploadTask(downloadUploadTasks).ToList<IShowable>();
                _view.ShowData(listToShow);
            }
        }

        private bool CheckIfAnyClientConnectedAndShowLablelIfNot(string[] status)
        {
            status = status.Where(str => !string.IsNullOrEmpty(str) && !string.IsNullOrWhiteSpace(str)).ToArray();
            if (status.Length == 2 && status[1] == "There is no clients connected")
            {
                _view.NoTasksVisible = true;
                _view.ListViewVisible = false;
                return false;
            }

            return true;
        }

        private bool CheckIfIShellTasksOrDownloadUploadTasksChanged(List<string> shellTasks, List<string> downloadUploadTasks)
        {
            if (lastShellTasks == null ||
               lastDownloadUploadTasks == null ||
               lastShellTasks.Count != shellTasks.Count ||
               lastDownloadUploadTasks.Count != downloadUploadTasks.Count)
            {
                lastShellTasks = shellTasks;
                lastDownloadUploadTasks = downloadUploadTasks;
                return true;
            }
            else
            {
                return shellTasks.IsDiffrentFrom(lastShellTasks) || downloadUploadTasks.IsDiffrentFrom(lastDownloadUploadTasks);
            }
        }

        private bool ParseClientTasks(string client, out List<string> shellTasks, out List<string> downloadUploadTasks)
        {
            shellTasks = new List<string>();
            downloadUploadTasks = new List<string>();

            var clientId = client.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries).ElementAt(1).Split('\t').First().Trim();
            if (clientId != _selectedClient) return false;
            var fields = client.Replace('\t', '\n').Split('\n');
            fields = fields.Where(str => !string.IsNullOrEmpty(str) && !string.IsNullOrWhiteSpace(str) && str != "The selected ").ToArray();

            var id = fields[0].Split(':').Last();
            var nickName = fields[1].Split(':').Last();
            var isAlive = bool.Parse(fields[2].Split(':').Last()) ? StatusType.On : StatusType.Off;

            var i = 4;
            while (fields[i] != "Upload And Download Tasks:")
            {
                if (fields[i] == "There is no shell tasks")
                {
                    i++;
                    break;
                }
                else if (fields[i].StartsWith("Task Number:"))
                {
                    i++;
                    continue;
                }

                shellTasks.Add(fields[i]);
                i++;
            }

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
            return true;
        }

        private List<ShellTaskData> ConvertToListOfShellTask(List<string> tasks)
        {
            var result = new List<ShellTaskData>();
            foreach (var task in tasks)
            {
                var splited = task.Split(' ');
                var splitedList = splited.ToList();
                if (splitedList.First() == "Download" || splitedList.First() == "Upload")
                    continue;
                if (splitedList.First() != "Run")
                    splitedList.RemoveAt(0);
                var command = splitedList.First();
                splitedList.RemoveAt(0);
                var args = string.Join(" ", splitedList);
                if (string.IsNullOrEmpty(command)) continue;
                result.Add(new ShellTaskData(command, args));
            }

            return result;
        }

        private List<DownloadUploadTaskData> ConvertToListOfDownloadUploadTask(List<string> tasks)
        {
            var result = new List<DownloadUploadTaskData>();
            foreach (var task in tasks)
            {
                var splited = task.Split(' ');
                var splitedList = splited.ToList();
                if (splitedList.First() != "Download" && splitedList.First() != "Upload")
                    continue;
                var command = splitedList.First();
                var filenName = splitedList.ElementAt(1);
                var path = splitedList.ElementAt(2);
                result.Add(new DownloadUploadTaskData(command, filenName, path));
            }

            return result;
        }

        public void Remove(int index)
        {
            var resp = _proxyService.DeleteClientTask(_selectedClient, _currentType == TaskType.Shell ? true : false, index + 1);
            if (resp) return;

            _view.DisplayMessage(MessageType.Error, "Delete task", "Error delete task");
        }
    }
}
