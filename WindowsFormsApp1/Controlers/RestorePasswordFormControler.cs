using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsFormsApp1.Authentication;
using WindowsFormsApp1.Interfaces;

namespace WindowsFormsApp1.Controlers
{
    public class RestorePasswordFormControler : CommunicationControler
    {
        IRestorePasswordView _view;
        private string _userName;
        private string _securityQuestion;

        public RestorePasswordFormControler(string endpointId, IRestorePasswordView view, string userName, string securityQuestion) : base(null)
        {
            _view = view;
            _userName = userName;
            _securityQuestion = securityQuestion;
        }

        public void Restore(string asnwer)
        {
            var resp = _authenticationProxy.RestorePasswordFromUserNameAndSecurityQuestion(new RestorePasswordFromUserNameAndSecurityQuestionRequest()
            {
                userName = _userName,
                answer = asnwer
            });

            if (!string.IsNullOrEmpty(resp.error))
            {
                _view.DisplayMessage(MessageType.Error, "Error", $"The answer {asnwer} does not math to the question {_securityQuestion} for user {_userName}");
                return;
            }
            _view.DisplayMessage(MessageType.Info, "Password Restore", $"Successfully Restored Password!{Environment.NewLine}Your password: {resp.RestorePasswordFromUserNameAndSecurityQuestionResult}");
            _view.CloseForm();
        }
    }
}
