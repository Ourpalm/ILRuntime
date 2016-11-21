namespace ILRuntimeTest
{
    partial class TestMainForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.OD = new System.Windows.Forms.OpenFileDialog();
            this.btnRun = new System.Windows.Forms.Button();
            this.listView1 = new System.Windows.Forms.ListView();
            this.tbLog = new System.Windows.Forms.TextBox();
            this.btnLoad = new System.Windows.Forms.Button();
            this.txtPath = new System.Windows.Forms.TextBox();
            this.btnRunSelect = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // OD
            // 
            this.OD.Filter = "*.dll|*.dll";
            // 
            // btnRun
            // 
            this.btnRun.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRun.Location = new System.Drawing.Point(581, 380);
            this.btnRun.Margin = new System.Windows.Forms.Padding(2);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(95, 29);
            this.btnRun.TabIndex = 0;
            this.btnRun.Text = "RunAll";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.OnBtnRun);
            // 
            // listView1
            // 
            this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView1.Location = new System.Drawing.Point(12, 12);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(664, 143);
            this.listView1.TabIndex = 1;
            this.listView1.UseCompatibleStateImageBehavior = false;
            // 
            // tbLog
            // 
            this.tbLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbLog.Location = new System.Drawing.Point(12, 161);
            this.tbLog.Multiline = true;
            this.tbLog.Name = "tbLog";
            this.tbLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbLog.Size = new System.Drawing.Size(664, 206);
            this.tbLog.TabIndex = 2;
            // 
            // btnLoad
            // 
            this.btnLoad.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLoad.Location = new System.Drawing.Point(378, 380);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(100, 29);
            this.btnLoad.TabIndex = 3;
            this.btnLoad.Text = "Load Assembly";
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.OnBtnLoad);
            // 
            // txtPath
            // 
            this.txtPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPath.Location = new System.Drawing.Point(12, 385);
            this.txtPath.Name = "txtPath";
            this.txtPath.Size = new System.Drawing.Size(349, 21);
            this.txtPath.TabIndex = 4;
            // 
            // BtnRunSelect
            // 
            this.btnRunSelect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRunSelect.Location = new System.Drawing.Point(484, 380);
            this.btnRunSelect.Name = "btnRunSelect";
            this.btnRunSelect.Size = new System.Drawing.Size(92, 29);
            this.btnRunSelect.TabIndex = 5;
            this.btnRunSelect.Text = "RunSelect";
            this.btnRunSelect.UseVisualStyleBackColor = true;
            this.btnRunSelect.Click += new System.EventHandler(this.OnBtnRunSelect);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(688, 420);
            this.Controls.Add(this.btnRunSelect);
            this.Controls.Add(this.txtPath);
            this.Controls.Add(this.btnLoad);
            this.Controls.Add(this.tbLog);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.btnRun);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "MainForm";
            this.Text = "TestAll";
            this.Load += new System.EventHandler(this.OnFormLoaded);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog OD;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.TextBox tbLog;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.TextBox txtPath;
        private System.Windows.Forms.Button btnRunSelect;
    }
}

