namespace ILRuntimeDebuggerLauncher
{
    partial class LauncherForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LauncherForm));
            this.listView1 = new System.Windows.Forms.ListView();
            this.ProcessName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.MachineName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ProcessId = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.EndPoint = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btn_Refresh = new System.Windows.Forms.Button();
            this.btn_Cancle = new System.Windows.Forms.Button();
            this.btn_OK = new System.Windows.Forms.Button();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.btn_EnterIpAndPort = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // listView1
            // 
            resources.ApplyResources(this.listView1, "listView1");
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ProcessName,
            this.MachineName,
            this.ProcessId,
            this.EndPoint});
            this.listView1.FullRowSelect = true;
            this.listView1.HideSelection = false;
            this.listView1.MultiSelect = false;
            this.listView1.Name = "listView1";
            this.listView1.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listView1_MouseDoubleClick);
            // 
            // ProcessName
            // 
            resources.ApplyResources(this.ProcessName, "ProcessName");
            // 
            // MachineName
            // 
            resources.ApplyResources(this.MachineName, "MachineName");
            // 
            // ProcessId
            // 
            resources.ApplyResources(this.ProcessId, "ProcessId");
            // 
            // EndPoint
            // 
            resources.ApplyResources(this.EndPoint, "EndPoint");
            // 
            // btn_Refresh
            // 
            resources.ApplyResources(this.btn_Refresh, "btn_Refresh");
            this.btn_Refresh.Name = "btn_Refresh";
            this.btn_Refresh.UseVisualStyleBackColor = true;
            this.btn_Refresh.Click += new System.EventHandler(this.btn_Refresh_Click);
            // 
            // btn_Cancle
            // 
            resources.ApplyResources(this.btn_Cancle, "btn_Cancle");
            this.btn_Cancle.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_Cancle.Name = "btn_Cancle";
            this.btn_Cancle.UseVisualStyleBackColor = true;
            // 
            // btn_OK
            // 
            resources.ApplyResources(this.btn_OK, "btn_OK");
            this.btn_OK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btn_OK.Name = "btn_OK";
            this.btn_OK.UseVisualStyleBackColor = true;
            // 
            // checkBox1
            // 
            resources.ApplyResources(this.checkBox1, "checkBox1");
            this.checkBox1.Checked = true;
            this.checkBox1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // btn_EnterIpAndPort
            // 
            resources.ApplyResources(this.btn_EnterIpAndPort, "btn_EnterIpAndPort");
            this.btn_EnterIpAndPort.Name = "btn_EnterIpAndPort";
            this.btn_EnterIpAndPort.UseVisualStyleBackColor = true;
            this.btn_EnterIpAndPort.Click += new System.EventHandler(this.btn_EnterIpAndPort_Click);
            // 
            // LauncherForm
            // 
            this.AcceptButton = this.btn_OK;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btn_Cancle;
            this.Controls.Add(this.btn_EnterIpAndPort);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.btn_OK);
            this.Controls.Add(this.btn_Cancle);
            this.Controls.Add(this.btn_Refresh);
            this.Controls.Add(this.listView1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "LauncherForm";
            this.ShowInTaskbar = false;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader ProcessName;
        private System.Windows.Forms.ColumnHeader MachineName;
        private System.Windows.Forms.Button btn_Refresh;
        private System.Windows.Forms.Button btn_Cancle;
        private System.Windows.Forms.Button btn_OK;
        private System.Windows.Forms.ColumnHeader ProcessId;
        private System.Windows.Forms.ColumnHeader EndPoint;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Button btn_EnterIpAndPort;
    }
}