using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1.DataModel
{
    public class ShellTaskData : Showable
    {
        private string _command;
        private string _args;

        public ShellTaskData(string command, string args) : base(command, new List<string>() { args }, -1)
        {
            _command = command;
            _args = args;
        }

        public string GetCommand()
        {
            return _command;
        }

        public string GetArgs()
        {
            return _args;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ShellTaskData other)) return false;
            return _command == other.GetCommand() && _args == other.GetArgs();
        }
    }
}
