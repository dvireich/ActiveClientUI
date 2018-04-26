namespace WindowsFormsApp1
{
    partial class statusForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(statusForm));
            this.listView1 = new System.Windows.Forms.ListView();
            this.ConnectedIcon = new System.Windows.Forms.PictureBox();
            this.DisconnectedIcon = new System.Windows.Forms.PictureBox();
            this.noClientConnected = new System.Windows.Forms.Label();
            this.selectedClientLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.ConnectedIcon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.DisconnectedIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // listView1
            // 
            this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView1.Location = new System.Drawing.Point(0, 0);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(536, 389);
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.listView1_ItemSelectionChanged_1);
            this.listView1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.listView1_MouseDown);
            // 
            // ConnectedIcon
            // 
            this.ConnectedIcon.AccessibleName = "ConnectedIcon";
            this.ConnectedIcon.Image = global::WindowsFormsApp1.Properties.Resources.ConnectionOn;
            this.ConnectedIcon.Location = new System.Drawing.Point(82, 353);
            this.ConnectedIcon.Name = "ConnectedIcon";
            this.ConnectedIcon.Size = new System.Drawing.Size(53, 36);
            this.ConnectedIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.ConnectedIcon.TabIndex = 1;
            this.ConnectedIcon.TabStop = false;
            this.ConnectedIcon.Visible = false;
            // 
            // DisconnectedIcon
            // 
            this.DisconnectedIcon.AccessibleName = "DisconnectedIcon";
            this.DisconnectedIcon.Image = global::WindowsFormsApp1.Properties.Resources.ConnectionOff;
            this.DisconnectedIcon.Location = new System.Drawing.Point(82, 353);
            this.DisconnectedIcon.Name = "DisconnectedIcon";
            this.DisconnectedIcon.Size = new System.Drawing.Size(53, 36);
            this.DisconnectedIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.DisconnectedIcon.TabIndex = 1;
            this.DisconnectedIcon.TabStop = false;
            this.DisconnectedIcon.Visible = false;
            // 
            // noClientConnected
            // 
            this.noClientConnected.Dock = System.Windows.Forms.DockStyle.Fill;
            this.noClientConnected.Font = new System.Drawing.Font("Microsoft Sans Serif", 25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.noClientConnected.Location = new System.Drawing.Point(0, 0);
            this.noClientConnected.Name = "noClientConnected";
            this.noClientConnected.Size = new System.Drawing.Size(536, 389);
            this.noClientConnected.TabIndex = 2;
            this.noClientConnected.Text = "No Client Connected";
            this.noClientConnected.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.noClientConnected.Visible = false;
            // 
            // selectedClientLabel
            // 
            this.selectedClientLabel.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.selectedClientLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.selectedClientLabel.Location = new System.Drawing.Point(0, 366);
            this.selectedClientLabel.Name = "selectedClientLabel";
            this.selectedClientLabel.Size = new System.Drawing.Size(536, 23);
            this.selectedClientLabel.TabIndex = 4;
            this.selectedClientLabel.Text = "label1";
            this.selectedClientLabel.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.selectedClientLabel.Visible = false;
            // 
            // statusForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(536, 389);
            this.Controls.Add(this.selectedClientLabel);
            this.Controls.Add(this.DisconnectedIcon);
            this.Controls.Add(this.ConnectedIcon);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.noClientConnected);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "statusForm";
            this.Text = "Status";
            this.Activated += new System.EventHandler(this.statusForm_Activated);
            ((System.ComponentModel.ISupportInitialize)(this.ConnectedIcon)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.DisconnectedIcon)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion

        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.PictureBox DisconnectedIcon;
        private System.Windows.Forms.PictureBox ConnectedIcon;
        private System.Windows.Forms.Label noClientConnected;
        private System.Windows.Forms.Label selectedClientLabel;
    }
}