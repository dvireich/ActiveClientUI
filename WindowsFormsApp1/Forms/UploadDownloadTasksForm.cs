using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsFormsApp1.Controlers;

namespace WindowsFormsApp1.Forms
{
    public class UploadDownloadTasksForm : TasksForm
    {
        public UploadDownloadTasksForm(string client, string id) : base(client, id)
        {
            ColumnsName = new List<string>() { "Action Type", "File name", "Path" };
            _controler = new UploadDownloadTaskFormControler(client, id, this);
        }
    }
}
