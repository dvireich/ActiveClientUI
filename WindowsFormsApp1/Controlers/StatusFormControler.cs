using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsFormsApp1.DataModel.Enums;
using WindowsFormsApp1.Interfaces;

namespace WindowsFormsApp1.Controlers
{
    public class StatusFormControler : CommunicationControler
    {
        IStatusView _view;

        private List<PassiveClientStatusData> _lastClientsStatusList;
        private List<PassiveClientStatusData> LastClientsStatusList
        {
            get
            {
                if(_lastClientsStatusList == null)
                {
                    _lastClientsStatusList = new List<PassiveClientStatusData>();
                }

                return _lastClientsStatusList;
            }
            set
            {
                _lastClientsStatusList = value;
            }
        }

        internal StatusFormControler(string endpointId, IStatusView view) : base(endpointId)
        {
            _view = view;
        }

        public void UpdateClientStatuses()
        {
            var status = _proxyService.GetStatus().Replace("\r", "");
            var statusSplittedNewLine = status.Split('\n');
            statusSplittedNewLine = statusSplittedNewLine.Where(str => !string.IsNullOrEmpty(str) && !string.IsNullOrWhiteSpace(str)).ToArray();

            if (!CheckConnectedCleintAndUpdateView(statusSplittedNewLine)) return;

            ExtractSelectedAndUpdateView(statusSplittedNewLine);

            var clientStatusList = ParseClientStatus(status);

            if (!LastClientsStatusList.IsDiffrentFrom(clientStatusList)) return;

            LastClientsStatusList = clientStatusList;
            _view.ShowData(clientStatusList.ToList<IShowable>());
            _view.NoSelectedClientLabelVisible = false;
            _view.ListViewVisible = true;
        }

        private void ExtractSelectedAndUpdateView(string[] statusSplittedNewLine)
        {
            _view.SelectedClient = string.Format("Selected client: {0}", statusSplittedNewLine.Last().Split(':').Last());
            _view.SelectedClientVisible = true;
        }

        private bool CheckConnectedCleintAndUpdateView(string[] status)
        {
            status = status.Where(str => !string.IsNullOrEmpty(str) && !string.IsNullOrWhiteSpace(str)).ToArray();
            if (status.Length == 2 && status[1] == "There is no clients connected")
            {
                _view.NoSelectedClientLabelVisible = true;
                _view.ListViewVisible = false;
                _view.SelectedClientVisible = false;
                return false;
            }

            return true;
        }

        public void Remove(string id)
        {
            var resp = _proxyService.ActiveCloseClient(id);
            if (resp) return;
            _view.DisplayMessage(MessageType.Error, "Remove client", $"Error in closing remote client: {id}");
        }

        public void SetNickName(string id, string name)
        {
            var resp = _proxyService.ActiveSetNickName(id, name);
            if (resp) return;
            _view.DisplayMessage(MessageType.Error, "Set nickName", $"Error in set the nick name: {name} for remote client: {id}");
        }

        public void SelectRemoteClient(string id)
        {
            var resp = _proxyService.SelectClient(id);
            if (resp) return;
            _view.DisplayMessage(MessageType.Error, "Select Remote Client", $"Error in selecting the remote client: {id}");
        }

        private static List<PassiveClientStatusData> ParseClientStatus(string status)
        {
            List<PassiveClientStatusData> clientStatusList = new List<PassiveClientStatusData>();
            var clients = status.Split(new string[] { "Client" }, StringSplitOptions.RemoveEmptyEntries);
            clients = clients.ToArray().Skip(1).Take(clients.Count() - 2).ToArray();

            foreach (var client in clients)
            {
                var fields = client.Replace('\t', '\n').Split('\n');
                fields = fields.Where(str => !string.IsNullOrEmpty(str) && !string.IsNullOrWhiteSpace(str) && str != "The selected ").ToArray();

                var id = fields[0].Split(':').Last();
                var nickName = fields[1].Split(':').Last();
                var isAlive = bool.Parse(fields[2].Split(':').Last()) ? StatusType.On : StatusType.Off;
                var shellTasks = new List<string>();
                var i = 4;
                while (fields[i] != "Upload And Download Tasks:")
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
                clientStatusList.Add(new PassiveClientStatusData(id.Trim(), isAlive, nickName));
            }

            return clientStatusList;
        }




    }
}
