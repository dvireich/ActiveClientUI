using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFormsApp1.Authentication;
using WindowsFormsApp1.LoadUser;

namespace WindowsFormsApp1
{
    public partial class LogInForm : Form
    {
        IAuthentication authenticationService;
        ILoadUser loadUser;
        private bool _activated;

        public LogInForm()
        {
            InitializeComponent();
        }

        private void CloseAllConnections()
        {
            if (authenticationService != null)
                ((ICommunicationObject)authenticationService).Close();

            if (loadUser != null)
                ((ICommunicationObject)loadUser).Close();
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
            string error;
            //Sign Up
            if (SignUpCheckBox.Checked)
            {
                if (!SignUp(out error))
                {
                    MessageBox.Show(error, "Sign Up", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                else
                {
                    MessageBox.Show("Successfully Signed Up!", "Sign Up", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    //Ask user if he want to login
                    var resp = MessageBox.Show("Would you like to sign in?", "Sign In", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (resp == DialogResult.No) return;
                }
            }

            //Sign In
            if (SignIn(out error, out string id) && loadUser.LoadUser(id))
            {
                MessageBox.Show("Successfully Signed In!", "Sign In", MessageBoxButtons.OK, MessageBoxIcon.Information);
                using (var mainForm = new mainForm(id))
                {
                    this.Visible = false;
                    mainForm.ShowDialog();
                }
            }
            else
            {
                MessageBox.Show(error, "Sign In", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        private bool SignUp(out string error)
        {
            var resp = authenticationService.SignUp(new SignUpRequest()
            {
                userName = UserNameTextBox.Text,
                password = PasswordTextBox.Text
            });

            error = resp.error;
            return resp.SignUpResult;
        }

        private bool SignIn(out string error, out string id)
        {
            var resp = authenticationService.Authenticate(new AuthenticateRequest()
            {
                userName = UserNameTextBox.Text,
                password = PasswordTextBox.Text
            });

            error = resp.error;
            id = resp.AuthenticateResult;

            return !string.IsNullOrEmpty(id);
        }

        private void LogInForm_Activated(object sender, EventArgs e)
        {
            if (_activated) return;
            _activated = true;
            initializeServiceReferences(ref authenticationService, "Authentication");
            initializeServiceReferences(ref loadUser, "LoadUser");
        }

        
    }
}
