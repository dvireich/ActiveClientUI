using System;
using System.ServiceModel;
using System.Windows.Forms;
using WindowsFormsApp1.Authentication;
using WindowsFormsApp1.Controlers;
using WindowsFormsApp1.Interfaces;

namespace WindowsFormsApp1
{
    public partial class RestorePasswordForm : BaseForm, IRestorePasswordView
    {
        private string _userName;
        private RestorePasswordFormControler _controler;

        public RestorePasswordForm(string userName, string securityQuestion)
        {
            _userName = userName;
            InitializeComponent();
            SecurityQuestionTextBox.Text = securityQuestion;
            _controler = new RestorePasswordFormControler(null, this, userName, securityQuestion);
        }

        public void DisplayMessage(MessageType type, string header, string message)
        {
            switch (type)
            {
                case MessageType.Error:
                    MessageBox.Show(message, header, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case MessageType.Info:
                    MessageBox.Show(message, header, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                case MessageType.Question:
                    MessageBox.Show(message, header, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    break;
            }
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            _controler.Restore(SecurityAnswerTextBox.Text);
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            CloseForm();
        }

        private void EnterKeyDownEvent(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                OkButton_Click(null, null);
            }
        }

        public void CloseForm()
        {
            this.Close();
        }
    }
}
