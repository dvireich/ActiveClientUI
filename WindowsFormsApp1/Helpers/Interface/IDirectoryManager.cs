using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1.Helpers.Interface
{
    public interface IDirectoryManager
    {
        bool Exists(string path);

        void CreateDirectory(string path);
    }
}
