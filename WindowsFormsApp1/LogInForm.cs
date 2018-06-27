using System;
using System.IO;
using System.ServiceModel;
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
        private bool _logout;

        public LogInForm()
        {
            InitializeComponent();
            RememberMe(false);
        }

        private void CloseAllConnections()
        {
            if (authenticationService != null)
                ((ICommunicationObject)authenticationService).Close();

            if (loadUser != null)
                ((ICommunicationObject)loadUser).Close();
        }

        private static void initializeServiceReferences<T>(ref T shellService, string path)
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
            var shellAdress = string.Format("http://localhost:80/ShellTrasferServer/{0}", path);
            var shellUri = new Uri(shellAdress);
            var shellEndpointAddress = new EndpointAddress(shellUri);
            var shellChannel = new ChannelFactory<T>(shellBinding, shellEndpointAddress);
            shellService = shellChannel.CreateChannel();
        }

        private void SignInButton_Click(object sender, EventArgs e)
        {
            string error;
            Cursor.Current = Cursors.WaitCursor;
            //Sign In
            if (SignIn(out error, out string id) && loadUser.LoadUser(id))
            {
                MessageBox.Show("Successfully Signed In!", "Sign In", MessageBoxButtons.OK, MessageBoxIcon.Information);

                //save username and password for next login
                if (RememberMeCheckBox.Checked)
                {
                    RememberMe(true);
                }

                using (var mainForm = new mainForm(id,this))
                {
                    this.Visible = false;
                    Cursor.Current = Cursors.Default;
                    mainForm.ShowDialog();
                }

                if (!_logout)
                {
                    this.Close();
                    return;
                } 

                _logout = false;
                this.Visible = true;

            }
            else
            {
                MessageBox.Show(error, "Sign In", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Cursor.Current = Cursors.Default;
                return;
            }
        }

        private void RememberMe(bool save)
        {
            var directoryName = "Files";
            var fileName = "LoginState";
            var path = Path.Combine(directoryName, fileName);
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            if (save)
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                using (var file = File.CreateText(path))
                {
                    file.Write(string.Format("{0} {1}", UserNameTextBox.Text, PasswordTextBox.Text));
                }
            }
            else
            {
                if (!File.Exists(path)) return;

                var allText = File.ReadAllText(path);
                var vals = allText.Split(' ');

                UserNameTextBox.Text = vals[0];
                PasswordTextBox.Text = vals[1];
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

        private void ExitButton_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void EnterKeyDownEvent(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SignInButton_Click(null, null);
            }
        }

        private void SignUpButton_Click(object sender, EventArgs e)
        {
            //Sign Up
            string error;
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
                SignInButton_Click(null, null);
            }

        }

        private void ForgotPasswordButton_Click(object sender, EventArgs e)
        {
            try
            {
                this.Visible = false;
                var userName = UserNameTextBox.Text;

                var resp = authenticationService.GetSecurityQuestion(new GetSecurityQuestionRequest()
                {
                    userName = userName
                });

                if (!string.IsNullOrEmpty(resp.error) ||
                    !string.IsNullOrEmpty(resp.error))
                {
                    MessageBox.Show(string.Format("Cant get security question for user: {0} with the following error: {1}", userName, resp.error)
                        , "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                else if (string.IsNullOrEmpty(resp.GetSecurityQuestionResult))
                {
                    MessageBox.Show(string.Format("There is no security question for user: {0}:", userName)
                        , "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                using (var restorePasswordForm = new RestorePasswordForm(userName, resp.GetSecurityQuestionResult))
                {
                    restorePasswordForm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Restore Password", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Visible = true;
            }
        }

        public void Logout()
        {
            _logout = true;
        }

    }
}
