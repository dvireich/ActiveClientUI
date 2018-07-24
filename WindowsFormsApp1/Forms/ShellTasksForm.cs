using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsFormsApp1.Controlers;

namespace WindowsFormsApp1.Forms
{
    public class ShellTasksForm : TasksForm
    {
        public ShellTasksForm(string client, string id) : base( client, id)
        {
            ColumnsName = new List<string>() { "Command", "Arguments" };
            _controler = new ShellTaskFormControler(client, id, this);
        }
    }
}
