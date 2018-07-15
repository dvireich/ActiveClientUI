using System.ServiceModel;

namespace WindowsFormsApp1
{
    partial class LogInForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.UserNameTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.PasswordTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.SecurityQuestionTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.SecurityAnswerTextBox = new System.Windows.Forms.TextBox();
            this.SignInButton = new System.Windows.Forms.Button();
            this.ExitButton = new System.Windows.Forms.Button();
            this.RememberMeCheckBox = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.SignUpButton = new System.Windows.Forms.Button();
            this.ForgotPasswordButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // UserNameTextBox
            // 
            this.UserNameTextBox.Location = new System.Drawing.Point(135, 57);
            this.UserNameTextBox.Name = "UserNameTextBox";
            this.UserNameTextBox.Size = new System.Drawing.Size(380, 22);
            this.UserNameTextBox.TabIndex = 0;
            this.UserNameTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.EnterKeyDownEvent);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(29, 57);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 17);
            this.label1.TabIndex = 1;
            this.label1.Text = "User Name:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(29, 99);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(73, 17);
            this.label2.TabIndex = 3;
            this.label2.Text = "Password:";
            // 
            // PasswordTextBox
            // 
            this.PasswordTextBox.Location = new System.Drawing.Point(135, 99);
            this.PasswordTextBox.Name = "PasswordTextBox";
            this.PasswordTextBox.Size = new System.Drawing.Size(380, 22);
            this.PasswordTextBox.TabIndex = 2;
            this.PasswordTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.EnterKeyDownEvent);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(29, 140);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(188, 17);
            this.label3.TabIndex = 5;
            this.label3.Text = "Security Question: (optional)";
            // 
            // SecurityQuestionTextBox
            // 
            this.SecurityQuestionTextBox.Location = new System.Drawing.Point(236, 140);
            this.SecurityQuestionTextBox.Name = "SecurityQuestionTextBox";
            this.SecurityQuestionTextBox.Size = new System.Drawing.Size(380, 22);
            this.SecurityQuestionTextBox.TabIndex = 4;
            this.SecurityQuestionTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.EnterKeyDownEvent);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(29, 181);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(238, 17);
            this.label4.TabIndex = 7;
            this.label4.Text = "Security Question Answer: (optional)";
            // 
            // SecurityAnswerTextBox
            // 
            this.SecurityAnswerTextBox.Location = new System.Drawing.Point(273, 181);
            this.SecurityAnswerTextBox.Name = "SecurityAnswerTextBox";
            this.SecurityAnswerTextBox.Size = new System.Drawing.Size(380, 22);
            this.SecurityAnswerTextBox.TabIndex = 6;
            this.SecurityAnswerTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.EnterKeyDownEvent);
            // 
            // SignInButton
            // 
            this.SignInButton.Location = new System.Drawing.Point(287, 230);
            this.SignInButton.Name = "SignInButton";
            this.SignInButton.Size = new System.Drawing.Size(75, 25);
            this.SignInButton.TabIndex = 8;
            this.SignInButton.Text = "Sign In";
            this.SignInButton.UseVisualStyleBackColor = true;
            this.SignInButton.Click += new System.EventHandler(this.SignInButton_Click);
            // 
            // ExitButton
            // 
            this.ExitButton.Location = new System.Drawing.Point(525, 232);
            this.ExitButton.Name = "ExitButton";
            this.ExitButton.Size = new System.Drawing.Size(75, 23);
            this.ExitButton.TabIndex = 9;
            this.ExitButton.Text = "Exit";
            this.ExitButton.UseVisualStyleBackColor = true;
            this.ExitButton.Click += new System.EventHandler(this.ExitButton_Click);
            // 
            // RememberMeCheckBox
            // 
            this.RememberMeCheckBox.AutoSize = true;
            this.RememberMeCheckBox.Checked = true;
            this.RememberMeCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.RememberMeCheckBox.Location = new System.Drawing.Point(32, 232);
            this.RememberMeCheckBox.Name = "RememberMeCheckBox";
            this.RememberMeCheckBox.Size = new System.Drawing.Size(122, 21);
            this.RememberMeCheckBox.TabIndex = 10;
            this.RememberMeCheckBox.Text = "Remember Me";
            this.RememberMeCheckBox.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(29, 18);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(354, 17);
            this.label5.TabIndex = 11;
            this.label5.Text = "Please enter user name and password in order to login";
            // 
            // SignUpButton
            // 
            this.SignUpButton.Location = new System.Drawing.Point(192, 230);
            this.SignUpButton.Name = "SignUpButton";
            this.SignUpButton.Size = new System.Drawing.Size(75, 25);
            this.SignUpButton.TabIndex = 13;
            this.SignUpButton.Text = "Sign Up";
            this.SignUpButton.UseVisualStyleBackColor = true;
            this.SignUpButton.Click += new System.EventHandler(this.SignUpButton_Click);
            // 
            // ForgotPasswordButton
            // 
            this.ForgotPasswordButton.Location = new System.Drawing.Point(381, 230);
            this.ForgotPasswordButton.Name = "ForgotPasswordButton";
            this.ForgotPasswordButton.Size = new System.Drawing.Size(128, 25);
            this.ForgotPasswordButton.TabIndex = 14;
            this.ForgotPasswordButton.Text = "Forgot Password";
            this.ForgotPasswordButton.UseVisualStyleBackColor = true;
            this.ForgotPasswordButton.Click += new System.EventHandler(this.ForgotPasswordButton_Click);
            // 
            // LogInForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(674, 319);
            this.ControlBox = false;
            this.Controls.Add(this.ForgotPasswordButton);
            this.Controls.Add(this.SignUpButton);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.RememberMeCheckBox);
            this.Controls.Add(this.ExitButton);
            this.Controls.Add(this.SignInButton);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.SecurityAnswerTextBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.SecurityQuestionTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.PasswordTextBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.UserNameTextBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Location = new System.Drawing.Point(0, 0);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LogInForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Log In";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox UserNameTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox PasswordTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox SecurityQuestionTextBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox SecurityAnswerTextBox;
        private System.Windows.Forms.Button SignInButton;
        private System.Windows.Forms.Button ExitButton;
        private System.Windows.Forms.CheckBox RememberMeCheckBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button SignUpButton;
        private System.Windows.Forms.Button ForgotPasswordButton;
    }
}