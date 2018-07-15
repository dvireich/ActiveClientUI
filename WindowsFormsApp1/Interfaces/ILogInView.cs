using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1.Interfaces
{
    interface ILogInView
    {
        string UserNameTextBoxText { get; set; }

        string PasswordTextBoxText { get; set; }

        string SecurityQuestionText { get; set; }

        string SecurityAnswerText { get; set; }

        bool RememberMeCheckBoxChecked { get; set; }

        bool FormVisible { get; set; }

        void OpenMainForm(string id);

        void OpenRestorePasswordForm(string userName, string securityQuestion);

        DialogResult DisplayMessage(MessageType type, string header, string message);
    }
}
