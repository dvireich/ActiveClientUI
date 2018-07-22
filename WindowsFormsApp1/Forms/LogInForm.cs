using System;
using System.IO;
using System.ServiceModel;
using System.Windows.Forms;
using WindowsFormsApp1.Authentication;
using WindowsFormsApp1.Controlers;
using WindowsFormsApp1.Helpers;
using WindowsFormsApp1.Interfaces;
using WindowsFormsApp1.LoadUser;

namespace WindowsFormsApp1
{
    public partial class LogInForm : BaseForm, ILogInView
    {   
        LogInFormControler _controler;

        public string SecurityQuestionText
        {
            get
            {
                return SecurityQuestionTextBox.Text;
            }
            set
            {
                Invoke((MethodInvoker)(() =>
                {
                    SecurityQuestionTextBox.Text = value;
                }));
            }
        }

        public string SecurityAnswerText
        {
            get
            {
                return SecurityAnswerTextBox.Text;
            }
            set
            {
                Invoke((MethodInvoker)(() =>
                {
                    SecurityAnswerTextBox.Text = value;
                }));
            }
        }

        public string UserNameTextBoxText
        {
            get
            {
                return UserNameTextBox.Text;
            }
            set
            {
                Invoke((MethodInvoker)(() =>
                {
                    UserNameTextBox.Text = value;
                }));
            }
        }

        public string PasswordTextBoxText
        {
            get
            {
                return PasswordTextBox.Text;
            }
            set
            {
                Invoke((MethodInvoker)(() =>
                {
                    PasswordTextBox.Text = value;
                }));
            }
        }

        public bool RememberMeCheckBoxChecked
        {
            get
            {
                return RememberMeCheckBox.Checked;
            }
            set
            {
                Invoke((MethodInvoker)(() =>
                {
                    RememberMeCheckBox.Checked = value;
                }));
            }
        }

        public bool FormVisible
        {
            get
            {
                return Visible;
            }
            set
            {
                Invoke((MethodInvoker)(() =>
                {
                    Visible = value;
                }));
            }
        }

        public LogInForm()
        {
            InitializeComponent();
            _controler = new LogInFormControler(null, 
                                                this,
                                                new FileManager(),
                                                new DirectoryManager());
            _controler.RememberMeOnLoad();
        }

        public void OpenMainForm(string id)
        {
            using (var mainForm = new MainForm(id, UserNameTextBox.Text))
            {
                this.Visible = false;
                Cursor.Current = Cursors.Default;
                mainForm.ShowDialog();
            }
        }

        private void SignInButton_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            _controler.SignIn();

            Cursor.Current = Cursors.Default;
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
            _controler.SignUp();
        }

        private void ForgotPasswordButton_Click(object sender, EventArgs e)
        {
            _controler.ForgotPassword();
        }

        public void OpenRestorePasswordForm(string userName, string securityQuestion)
        {
            using (var restorePasswordForm = new RestorePasswordForm(userName, securityQuestion))
            {
                restorePasswordForm.ShowDialog();
            }
        }

        public DialogResult DisplayMessage(MessageType type, string header, string message)
        {
            switch (type)
            {
                case MessageType.Error:
                    return MessageBox.Show(message, header, MessageBoxButtons.OK, MessageBoxIcon.Error);
                case MessageType.Info:
                    return MessageBox.Show(message, header, MessageBoxButtons.OK, MessageBoxIcon.Information);
                case MessageType.Question:
                    return MessageBox.Show(message, header, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            }

            return DialogResult.Cancel;
        }
    }
}
