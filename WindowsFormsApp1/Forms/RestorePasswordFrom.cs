using System;
using System.ServiceModel;
using System.Windows.Forms;
using WindowsFormsApp1.Authentication;

namespace WindowsFormsApp1
{
    public partial class RestorePasswordForm : Form
    {
        IAuthentication authenticationService;
        private bool _activated;
        private string _userName;
        private string _securityQuestion;

        public RestorePasswordForm(string userName, string securityQuestion)
        {
            InitializeComponent();
            _userName = userName;
            _securityQuestion = securityQuestion;
        }

        private void CloseAllConnections()
        {
            if (authenticationService != null)
                ((ICommunicationObject)authenticationService).Close();
        }

        private static void initializeServiceReferences<T>(ref T shellService,string path)
        {
            //Confuguring the Shell service
            var shellBinding = new BasicHttpBinding();
            shellBinding.Security.Mode = BasicHttpSecurityMode.None;
            shellBinding.CloseTimeout = TimeSpan.MaxValue;
            shellBinding.ReceiveTimeout = TimeSpan.MaxValue;
            shellBinding.SendTimeout = new TimeSpan(0, 0, 10, 0, 0);
            shellBinding.OpenTimeout = TimeSpan.MaxValue;
            shellBinding.MaxReceivedMessageSize = int.MaxValue;
            shellBinding.MaxBufferPoolSize = int.MaxValue;
            shellBinding.MaxBufferSize = int.MaxValue;
            //Put Public ip of the server copmuter
            var shellAdress = string.Format("http://localhost:80/ShellTrasferServer/{0}",path);
            var shellUri = new Uri(shellAdress);
            var shellEndpointAddress = new EndpointAddress(shellUri);
            var shellChannel = new ChannelFactory<T>(shellBinding, shellEndpointAddress);
            shellService = shellChannel.CreateChannel();
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            var userAnswer = SecurityAnswerTextBox.Text;
            var userQuestion = SecurityQuestionTextBox.Text;
            var resp = authenticationService.RestorePasswordFromUserNameAndSecurityQuestion(new RestorePasswordFromUserNameAndSecurityQuestionRequest()
            {
                userName = _userName,
                answer = userAnswer
            });

            if(string.IsNullOrEmpty(resp.error))
            {
                throw new Exception(string.Format("The answer {0} does not math to the question {1} for user {2}", userAnswer, userQuestion, _userName));
            }

            MessageBox.Show(string.Format("Successfully Restored Password!{0}Your password: {1}",Environment.NewLine,resp.RestorePasswordFromUserNameAndSecurityQuestionResult), "Password Restore", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }

        private void RestorePasswordForm_Activated(object sender, EventArgs e)
        {
            if (_activated) return;
            _activated = true;
            initializeServiceReferences(ref authenticationService, "Authentication");

            SecurityQuestionTextBox.Text = _securityQuestion;
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void EnterKeyDownEvent(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                OkButton_Click(null, null);
            }
        }
    }
}
