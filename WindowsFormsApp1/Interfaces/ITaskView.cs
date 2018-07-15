using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1.Interfaces
{
    public interface ITaskView : IView
    {
        bool NoTasksVisible { get; set; }

        bool ListViewVisible { get; set; }
    }
}
