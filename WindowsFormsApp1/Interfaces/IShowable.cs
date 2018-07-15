using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1.Interfaces
{
    public interface IShowable
    {
        string MainItem { get; }

        List<string> SubItems { get;}

        int ImageIndex { get;}

    }
}
