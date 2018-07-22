using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsFormsApp1.Helpers.Interface;

namespace WindowsFormsApp1.Helpers
{
    public class FileManager : IFileManager
    {
        public FileStream Create(string path)
        {
            return File.Create(path);
        }

        public StreamWriter CreateText(string path)
        {
            return File.CreateText(path);
        }

        public void Delete(string path)
        {
            File.Delete(path);
        }

        public bool Exists(string path)
        {
            return File.Exists(path);
        }

        public long GetFileLength(string path)
        {
            return new FileInfo(path).Length;
        }

        public FileStream OpenRead(string path)
        {
            return File.OpenRead(path);
        }

        public string ReadAllText(string path)
        {
            return File.ReadAllText(path);
        }
    }
}
