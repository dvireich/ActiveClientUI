using System.ServiceModel;

namespace WindowsFormsApp1
{
    partial class RestorePasswordForm
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
            CloseAllConnections();

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
            this.label3 = new System.Windows.Forms.Label();
            this.SecurityQuestionTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.SecurityAnswerTextBox = new System.Windows.Forms.TextBox();
            this.OkButton = new System.Windows.Forms.Button();
            this.CancelButton = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(11, 50);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(124, 17);
            this.label3.TabIndex = 5;
            this.label3.Text = "Security Question:";
            // 
            // SecurityQuestionTextBox
            // 
            this.SecurityQuestionTextBox.Enabled = false;
            this.SecurityQuestionTextBox.Location = new System.Drawing.Point(141, 50);
            this.SecurityQuestionTextBox.Name = "SecurityQuestionTextBox";
            this.SecurityQuestionTextBox.Size = new System.Drawing.Size(457, 22);
            this.SecurityQuestionTextBox.TabIndex = 4;
            this.SecurityQuestionTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.EnterKeyDownEvent);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(11, 91);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(238, 17);
            this.label4.TabIndex = 7;
            this.label4.Text = "Security Question Answer: (optional)";
            // 
            // SecurityAnswerTextBox
            // 
            this.SecurityAnswerTextBox.Location = new System.Drawing.Point(255, 91);
            this.SecurityAnswerTextBox.Name = "SecurityAnswerTextBox";
            this.SecurityAnswerTextBox.Size = new System.Drawing.Size(380, 22);
            this.SecurityAnswerTextBox.TabIndex = 6;
            this.SecurityAnswerTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.EnterKeyDownEvent);
            // 
            // OkButton
            // 
            this.OkButton.Location = new System.Drawing.Point(205, 138);
            this.OkButton.Name = "OkButton";
            this.OkButton.Size = new System.Drawing.Size(75, 23);
            this.OkButton.TabIndex = 8;
            this.OkButton.Text = "OK";
            this.OkButton.UseVisualStyleBackColor = true;
            this.OkButton.Click += new System.EventHandler(this.OkButton_Click);
            // 
            // CancelButton
            // 
            this.CancelButton.Location = new System.Drawing.Point(382, 138);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(75, 23);
            this.CancelButton.TabIndex = 9;
            this.CancelButton.Text = "Cancel";
            this.CancelButton.UseVisualStyleBackColor = true;
            this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
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
            // RestorePasswordForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(655, 212);
            this.ControlBox = false;
            this.Controls.Add(this.label5);
            this.Controls.Add(this.CancelButton);
            this.Controls.Add(this.OkButton);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.SecurityAnswerTextBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.SecurityQuestionTextBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RestorePasswordForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Restore Password";
            this.Activated += new System.EventHandler(this.RestorePasswordForm_Activated);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox SecurityQuestionTextBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox SecurityAnswerTextBox;
        private System.Windows.Forms.Button OkButton;
        private new System.Windows.Forms.Button CancelButton;
        private System.Windows.Forms.Label label5;
    }
}