using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1.Helpers.Interface
{
    public interface IFileManager
    {
        bool Exists(string path);

        void Delete(string path);

        FileStream Create(string path);

        long GetFileLength(string path);

        FileStream OpenRead(string path);

        string ReadAllText(string path);

        StreamWriter CreateText(string path);
    }
}
