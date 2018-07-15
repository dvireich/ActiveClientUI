using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1.DataModel
{
    public class DownloadUploadTaskData : Showable
    {
        private string _command;
        private string _fileName;
        private string _path;

        public DownloadUploadTaskData(string command, string filename, string path) : base(command, new List<string>() { filename, path }, -1)
        {
            _command = command;
            _fileName = filename;
            _path = path;
        }

        public string GetCommand()
        {
            return _command;
        }

        public string GetFileName()
        {
            return _fileName;
        }

        public string GetPath()
        {
            return _path;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is DownloadUploadTaskData other)) return false;

            return _command == other.GetCommand() && _fileName == other.GetFileName() && _path == other.GetPath();

        }
    }
}
