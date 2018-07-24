namespace WindowsFormsApp1
{
    partial class TasksForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TasksForm));
            this.NoTasks = new System.Windows.Forms.Label();
            this.listView1 = new WindowsFormsApp1.ListViewNF();
            this.SuspendLayout();
            // 
            // NoTasks
            // 
            this.NoTasks.Dock = System.Windows.Forms.DockStyle.Fill;
            this.NoTasks.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.NoTasks.Location = new System.Drawing.Point(0, 0);
            this.NoTasks.Name = "NoTasks";
            this.NoTasks.Size = new System.Drawing.Size(407, 377);
            this.NoTasks.TabIndex = 1;
            this.NoTasks.Text = "No task for this client";
            this.NoTasks.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // listView1
            // 
            this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView1.Location = new System.Drawing.Point(0, 0);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(407, 377);
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.listView1_ItemSelectionChanged);
            this.listView1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.listView1_MouseDown_1);
            // 
            // Tasks
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(407, 377);
            this.Controls.Add(this.NoTasks);
            this.Controls.Add(this.listView1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Tasks";
            this.Text = "Tasks";
            this.Activated += new System.EventHandler(this.Tasks_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Tasks_FormClosing);
            this.ResumeLayout(false);

        }

        #endregion

        private ListViewNF listView1;
        private System.Windows.Forms.Label NoTasks;
    }
}