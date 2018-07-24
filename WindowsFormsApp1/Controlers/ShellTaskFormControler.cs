using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsFormsApp1.Interfaces;

namespace WindowsFormsApp1.Controlers
{
    public class ShellTaskFormControler : TaskFormControler
    {
        public ShellTaskFormControler(string selectedClient, string endpointId, ITaskView view) : base(selectedClient, endpointId, view)
        {
        }

        protected override List<IShowable> ConvertToIShowableList(List<string> shellTasks, List<string> downloadUploadTasks)
        {
            return ConvertToListOfShellTask(shellTasks).ToList<IShowable>();
        }

        public override void Remove(int index)
        {
            var resp = _proxyService.DeleteClientTask(_selectedClient, true , index + 1);
            if (resp) return;

            _view.DisplayMessage(MessageType.Error, "Delete task", "Error delete task");
        }
    }
}
