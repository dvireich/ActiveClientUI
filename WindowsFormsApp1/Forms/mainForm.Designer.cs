using System.ServiceModel;

namespace WindowsFormsApp1
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.logOutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.actionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.detailsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.smallIconsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.largIconsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.enterDirectoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.remaneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.currentPathTextBox = new System.Windows.Forms.TextBox();
            this.downloadUploadLable = new System.Windows.Forms.Label();
            this.Type = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.listView1 = new WindowsFormsApp1.ListViewNF();
            this.downloadUploadProgressBar = new PrecentageProgressBar();
            this.NoSelectedClient = new System.Windows.Forms.Label();
            this.folderImage = new System.Windows.Forms.PictureBox();
            this.fileImage = new System.Windows.Forms.PictureBox();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.folderImage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fileImage)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.actionToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(474, 28);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem,
            this.logOutToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(44, 24);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Image = global::WindowsFormsApp1.Properties.Resources.exit;
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(216, 26);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.ExitToolStripMenuItem_Click);
            // 
            // logOutToolStripMenuItem
            // 
            this.logOutToolStripMenuItem.Image = global::WindowsFormsApp1.Properties.Resources.Logout_37127;
            this.logOutToolStripMenuItem.Name = "logOutToolStripMenuItem";
            this.logOutToolStripMenuItem.Size = new System.Drawing.Size(216, 26);
            this.logOutToolStripMenuItem.Text = "Log Out";
            this.logOutToolStripMenuItem.Click += new System.EventHandler(this.LogOutToolStripMenuItem_Click);
            // 
            // actionToolStripMenuItem
            // 
            this.actionToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.viewToolStripMenuItem,
            this.statusToolStripMenuItem,
            this.enterDirectoryToolStripMenuItem,
            this.remaneToolStripMenuItem,
            this.deleteToolStripMenuItem,
            this.copyToolStripMenuItem,
            this.cutToolStripMenuItem});
            this.actionToolStripMenuItem.Name = "actionToolStripMenuItem";
            this.actionToolStripMenuItem.Size = new System.Drawing.Size(64, 24);
            this.actionToolStripMenuItem.Text = "Action";
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.detailsToolStripMenuItem,
            this.smallIconsToolStripMenuItem,
            this.largIconsToolStripMenuItem});
            this.viewToolStripMenuItem.Image = global::WindowsFormsApp1.Properties.Resources.view;
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(183, 26);
            this.viewToolStripMenuItem.Text = "View";
            // 
            // detailsToolStripMenuItem
            // 
            this.detailsToolStripMenuItem.Image = global::WindowsFormsApp1.Properties.Resources.details;
            this.detailsToolStripMenuItem.Name = "detailsToolStripMenuItem";
            this.detailsToolStripMenuItem.Size = new System.Drawing.Size(159, 26);
            this.detailsToolStripMenuItem.Text = "Details";
            this.detailsToolStripMenuItem.Click += new System.EventHandler(this.DetailsToolStripMenuItem_Click);
            // 
            // smallIconsToolStripMenuItem
            // 
            this.smallIconsToolStripMenuItem.Image = global::WindowsFormsApp1.Properties.Resources.small_icons;
            this.smallIconsToolStripMenuItem.Name = "smallIconsToolStripMenuItem";
            this.smallIconsToolStripMenuItem.Size = new System.Drawing.Size(159, 26);
            this.smallIconsToolStripMenuItem.Text = "Small Icons";
            this.smallIconsToolStripMenuItem.Click += new System.EventHandler(this.SmallIconsToolStripMenuItem_Click);
            // 
            // largIconsToolStripMenuItem
            // 
            this.largIconsToolStripMenuItem.Image = global::WindowsFormsApp1.Properties.Resources.large_icons;
            this.largIconsToolStripMenuItem.Name = "largIconsToolStripMenuItem";
            this.largIconsToolStripMenuItem.Size = new System.Drawing.Size(159, 26);
            this.largIconsToolStripMenuItem.Text = "Larg Icons";
            this.largIconsToolStripMenuItem.Click += new System.EventHandler(this.LargIconsToolStripMenuItem_Click);
            // 
            // statusToolStripMenuItem
            // 
            this.statusToolStripMenuItem.Image = global::WindowsFormsApp1.Properties.Resources.user_status_512;
            this.statusToolStripMenuItem.Name = "statusToolStripMenuItem";
            this.statusToolStripMenuItem.Size = new System.Drawing.Size(183, 26);
            this.statusToolStripMenuItem.Text = "Status";
            this.statusToolStripMenuItem.Click += new System.EventHandler(this.StatusToolStripMenuItem_Click);
            // 
            // enterDirectoryToolStripMenuItem
            // 
            this.enterDirectoryToolStripMenuItem.Name = "enterDirectoryToolStripMenuItem";
            this.enterDirectoryToolStripMenuItem.Size = new System.Drawing.Size(183, 26);
            this.enterDirectoryToolStripMenuItem.Text = "Enter Directory";
            this.enterDirectoryToolStripMenuItem.Visible = false;
            // 
            // remaneToolStripMenuItem
            // 
            this.remaneToolStripMenuItem.Name = "remaneToolStripMenuItem";
            this.remaneToolStripMenuItem.Size = new System.Drawing.Size(183, 26);
            this.remaneToolStripMenuItem.Text = "Rename";
            this.remaneToolStripMenuItem.Visible = false;
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(183, 26);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Visible = false;
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(183, 26);
            this.copyToolStripMenuItem.Text = "Copy";
            this.copyToolStripMenuItem.Visible = false;
            // 
            // cutToolStripMenuItem
            // 
            this.cutToolStripMenuItem.Name = "cutToolStripMenuItem";
            this.cutToolStripMenuItem.Size = new System.Drawing.Size(183, 26);
            this.cutToolStripMenuItem.Text = "Cut";
            this.cutToolStripMenuItem.Visible = false;
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(53, 24);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Image = global::WindowsFormsApp1.Properties.Resources.help;
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(125, 26);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.AboutToolStripMenuItem_Click);
            // 
            // currentPathTextBox
            // 
            this.currentPathTextBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.currentPathTextBox.Location = new System.Drawing.Point(0, 28);
            this.currentPathTextBox.Name = "currentPathTextBox";
            this.currentPathTextBox.Size = new System.Drawing.Size(474, 22);
            this.currentPathTextBox.TabIndex = 10;
            this.currentPathTextBox.Enter += new System.EventHandler(this.CurrentPathTextBox_Enter);
            this.currentPathTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.CurrentPathTextBox_KeyDown);
            this.currentPathTextBox.Leave += new System.EventHandler(this.CurrentPathTextBox_Leave);
            // 
            // downloadUploadLable
            // 
            this.downloadUploadLable.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.downloadUploadLable.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.downloadUploadLable.Location = new System.Drawing.Point(0, 383);
            this.downloadUploadLable.Name = "downloadUploadLable";
            this.downloadUploadLable.Size = new System.Drawing.Size(474, 30);
            this.downloadUploadLable.TabIndex = 9;
            this.downloadUploadLable.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.downloadUploadLable.Visible = false;
            // 
            // listView1
            // 
            this.listView1.Alignment = System.Windows.Forms.ListViewAlignment.SnapToGrid;
            this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.Type});
            this.listView1.Location = new System.Drawing.Point(0, 48);
            this.listView1.MultiSelect = false;
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(474, 388);
            this.listView1.TabIndex = 6;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.SelectedIndexChanged += new System.EventHandler(this.ListView1_SelectedIndexChanged);
            this.listView1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ListView1_MouseClick);
            this.listView1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.ListView1_MouseDoubleClick);
            // 
            // downloadUploadProgressBar
            // 
            this.downloadUploadProgressBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.downloadUploadProgressBar.Location = new System.Drawing.Point(0, 413);
            this.downloadUploadProgressBar.Name = "downloadUploadProgressBar";
            this.downloadUploadProgressBar.Size = new System.Drawing.Size(474, 23);
            this.downloadUploadProgressBar.TabIndex = 8;
            this.downloadUploadProgressBar.Visible = false;
            // 
            // NoSelectedClient
            // 
            this.NoSelectedClient.Dock = System.Windows.Forms.DockStyle.Fill;
            this.NoSelectedClient.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.NoSelectedClient.Location = new System.Drawing.Point(0, 50);
            this.NoSelectedClient.Name = "NoSelectedClient";
            this.NoSelectedClient.Size = new System.Drawing.Size(474, 333);
            this.NoSelectedClient.TabIndex = 7;
            this.NoSelectedClient.Text = "No Selected Client\r\nGo to Action->Status and select Client\r\n";
            this.NoSelectedClient.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // folderImage
            // 
            this.folderImage.Image = global::WindowsFormsApp1.Properties.Resources.folder;
            this.folderImage.Location = new System.Drawing.Point(39, 287);
            this.folderImage.Name = "folderImage";
            this.folderImage.Size = new System.Drawing.Size(68, 64);
            this.folderImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.folderImage.TabIndex = 2;
            this.folderImage.TabStop = false;
            this.folderImage.Visible = false;
            // 
            // fileImage
            // 
            this.fileImage.Image = global::WindowsFormsApp1.Properties.Resources.File;
            this.fileImage.Location = new System.Drawing.Point(132, 287);
            this.fileImage.Name = "fileImage";
            this.fileImage.Size = new System.Drawing.Size(68, 64);
            this.fileImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.fileImage.TabIndex = 0;
            this.fileImage.TabStop = false;
            this.fileImage.Visible = false;
            // 
            // mainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(474, 436);
            this.Controls.Add(this.NoSelectedClient);
            this.Controls.Add(this.currentPathTextBox);
            this.Controls.Add(this.downloadUploadLable);
            this.Controls.Add(this.downloadUploadProgressBar);
            this.Controls.Add(this.folderImage);
            this.Controls.Add(this.fileImage);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.listView1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "mainForm";
            this.Text = "File Viewer-Transfer";
            this.Activated += new System.EventHandler(this.MainForm_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.folderImage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fileImage)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem actionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.PictureBox fileImage;
        private System.Windows.Forms.PictureBox folderImage;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem detailsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem smallIconsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem largIconsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem enterDirectoryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem remaneToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem statusToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.TextBox currentPathTextBox;
        private System.Windows.Forms.Label downloadUploadLable;
        private System.Windows.Forms.ColumnHeader Type;
        private ListViewNF listView1;
        private PrecentageProgressBar downloadUploadProgressBar;
        private System.Windows.Forms.Label NoSelectedClient;
        private System.Windows.Forms.ToolStripMenuItem logOutToolStripMenuItem;
    }
}

