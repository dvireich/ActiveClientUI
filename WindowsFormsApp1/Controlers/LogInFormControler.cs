using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFormsApp1.Authentication;
using WindowsFormsApp1.Helpers.Interface;
using WindowsFormsApp1.Interfaces;
using WindowsFormsApp1.LoadUser;

namespace WindowsFormsApp1.Controlers
{
    class LogInFormControler : CommunicationControler
    {
        ILogInView _view;
        IFileManager _fileManager;
        IDirectoryManager _directoryManager;

        public LogInFormControler(string endpointId, 
                                  ILogInView view,
                                  IFileManager fileManager,
                                  IDirectoryManager directoryManager) : base(null)
        {
            _view = view;
            _fileManager = fileManager;
            _directoryManager = directoryManager;
        }

        public void RememberMeOnLoad()
        {
            var directoryName = "Files";
            var fileName = "LoginState";
            var path = Path.Combine(directoryName, fileName);
            if (!_directoryManager.Exists(directoryName))
            {
                _directoryManager.CreateDirectory(directoryName);
            }

            if (!_fileManager.Exists(path)) return;

            var allText = _fileManager.ReadAllText(path);
            var vals = allText.Split(' ');

            _view.UserNameTextBoxText = vals[0];
            _view.PasswordTextBoxText = vals[1];
        }

        public void RememberMeOnSave(string userName, string password)
        {
            var directoryName = "Files";
            var fileName = "LoginState";
            var path = Path.Combine(directoryName, fileName);
            if (!_directoryManager.Exists(directoryName))
            {
                _directoryManager.CreateDirectory(directoryName);
            }

            if (_fileManager.Exists(path))
            {
                _fileManager.Delete(path);
            }

            using (var file = _fileManager.CreateText(path))
            {
                file.Write(string.Format("{0} {1}", userName, password));
            }
        }

        private bool SignIn(string userName, string password, out string error, out string id)
        {
            var resp = _authenticationProxy.AuthenticateActiveClientAndSignIn(new AuthenticateActiveClientAndSignInRequest()
            {
                userName = userName,
                password = password,
            });

            error = resp.error;
            id = resp.AuthenticateActiveClientAndSignInResult;

            return !string.IsNullOrEmpty(id);
        }

        public void SignIn()
        {
            if (SignIn(_view.UserNameTextBoxText, _view.PasswordTextBoxText, out string error, out string id) && _loadUserProxy.LoadUser(id))
            {
                _view.DisplayMessage(MessageType.Info, "Sign In", "Successfully Signed In!");

                //save username and password for next login
                if (_view.RememberMeCheckBoxChecked)
                {
                    RememberMeOnSave(_view.UserNameTextBoxText, _view.PasswordTextBoxText);
                }

                _view.OpenMainForm(id);

                _view.FormVisible = true;

            }
            else
            {
                _view.DisplayMessage(MessageType.Error, "Sign In", error);
            }
        }

        private bool SignUp(string userName, string password, out string error)
        {
            var resp = _authenticationProxy.SignUp(new SignUpRequest()
            {
                userName = userName,
                password = password,
                
            });

            error = resp.error;
            return resp.SignUpResult;
        }

        private void SetSecurityQuestionAndAnswer()
        {
            if (string.IsNullOrEmpty(_view.SecurityQuestionText) || string.IsNullOrEmpty(_view.SecurityAnswerText))
            {
                _view.DisplayMessage(MessageType.Info, "Security Question And Answer", "The security question field or security answer was emprt. Not setting security question");
                return;
            }
           var resp = _authenticationProxy.SetSecurityQuestionAndAnswer(new SetSecurityQuestionAndAnswerRequest()
            {
                 userName = _view.UserNameTextBoxText,
                 password = _view.PasswordTextBoxText,
                 question = _view.SecurityQuestionText,
                 answer = _view.SecurityAnswerText
            });

            if(resp.SetSecurityQuestionAndAnswerResult)
            {
                _view.DisplayMessage(MessageType.Info, "Security Question And Answer", "Security question and answer set successfully!");
                return;
            }

            _view.DisplayMessage(MessageType.Error, "Security Question And Answer", $"Error setting security question and answer: {resp.error}");
        }

        public void SignUp()
        {
            //Sign Up
            if (!SignUp(_view.UserNameTextBoxText, _view.PasswordTextBoxText, out string error))
            {
                _view.DisplayMessage(MessageType.Error, "Sign Up", error);
            }
            else
            {
                SetSecurityQuestionAndAnswer();
                if (_view.DisplayMessage(MessageType.Question, "Sign In", "Would you like to sign in?") == DialogResult.No) return;
                SignIn();
            }
        }

        public void ForgotPassword()
        {
            try
            {
                _view.FormVisible = false;

                var resp = _authenticationProxy.GetSecurityQuestion(new GetSecurityQuestionRequest()
                {
                    userName = _view.UserNameTextBoxText
                });

                if (!string.IsNullOrEmpty(resp.error) ||
                    !string.IsNullOrEmpty(resp.error))
                {
                    _view.DisplayMessage(MessageType.Error, "Error", $"Cant get security question for user: {_view.UserNameTextBoxText} with the following error: {resp.error}");
                    return;
                }
                else if (string.IsNullOrEmpty(resp.GetSecurityQuestionResult))
                {
                    _view.DisplayMessage(MessageType.Error, "Error", $"There is no security question for user: {_view.UserNameTextBoxText}");
                    return;
                }

                _view.OpenRestorePasswordForm(_view.UserNameTextBoxText, resp.GetSecurityQuestionResult);
            }
            catch (Exception ex)
            {
                _view.DisplayMessage(MessageType.Error, "Restore Password", ex.Message);
            }
            finally
            {
                _view.FormVisible = true;
            }
        }
    }
}
