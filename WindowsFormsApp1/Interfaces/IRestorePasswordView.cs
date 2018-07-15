using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1.Interfaces
{
    public interface IRestorePasswordView
    {
        void CloseForm();

        void DisplayMessage(MessageType type, string header, string message);
    }
}
