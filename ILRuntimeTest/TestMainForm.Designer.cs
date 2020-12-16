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
            this.btnGenerateBinding = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.btnTestCrossBind = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // OD
            // 
            this.OD.Filter = "*.dll|*.dll";
            // 
            // btnRun
            // 
            this.btnRun.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRun.Location = new System.Drawing.Point(574, 380);
            this.btnRun.Margin = new System.Windows.Forms.Padding(2);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(62, 29);
            this.btnRun.TabIndex = 0;
            this.btnRun.Text = "RunAll";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.OnBtnRun);
            // 
            // listView1
            // 
            this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(0, 0);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(809, 157);
            this.listView1.TabIndex = 1;
            this.listView1.UseCompatibleStateImageBehavior = false;
            // 
            // tbLog
            // 
            this.tbLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbLog.Location = new System.Drawing.Point(0, 2);
            this.tbLog.Multiline = true;
            this.tbLog.Name = "tbLog";
            this.tbLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbLog.Size = new System.Drawing.Size(809, 192);
            this.tbLog.TabIndex = 2;
            // 
            // btnLoad
            // 
            this.btnLoad.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLoad.Location = new System.Drawing.Point(392, 380);
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
            this.txtPath.Size = new System.Drawing.Size(374, 21);
            this.txtPath.TabIndex = 4;
            // 
            // btnRunSelect
            // 
            this.btnRunSelect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRunSelect.Location = new System.Drawing.Point(498, 379);
            this.btnRunSelect.Name = "btnRunSelect";
            this.btnRunSelect.Size = new System.Drawing.Size(71, 29);
            this.btnRunSelect.TabIndex = 5;
            this.btnRunSelect.Text = "RunSelect";
            this.btnRunSelect.UseVisualStyleBackColor = true;
            this.btnRunSelect.Click += new System.EventHandler(this.OnBtnRunSelect);
            // 
            // btnGenerateBinding
            // 
            this.btnGenerateBinding.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGenerateBinding.Location = new System.Drawing.Point(641, 380);
            this.btnGenerateBinding.Name = "btnGenerateBinding";
            this.btnGenerateBinding.Size = new System.Drawing.Size(84, 29);
            this.btnGenerateBinding.TabIndex = 6;
            this.btnGenerateBinding.Text = "Binding";
            this.btnGenerateBinding.UseVisualStyleBackColor = true;
            this.btnGenerateBinding.Click += new System.EventHandler(this.btnGenerateBinding_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(12, 12);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.listView1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tbLog);
            this.splitContainer1.Size = new System.Drawing.Size(809, 355);
            this.splitContainer1.SplitterDistance = 157;
            this.splitContainer1.TabIndex = 7;
            // 
            // btnTestCrossBind
            // 
            this.btnTestCrossBind.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnTestCrossBind.Location = new System.Drawing.Point(731, 380);
            this.btnTestCrossBind.Name = "btnTestCrossBind";
            this.btnTestCrossBind.Size = new System.Drawing.Size(98, 29);
            this.btnTestCrossBind.TabIndex = 8;
            this.btnTestCrossBind.Text = "CrossBind Gen";
            this.btnTestCrossBind.UseVisualStyleBackColor = true;
            this.btnTestCrossBind.Click += new System.EventHandler(this.button1_Click);
            // 
            // TestMainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(833, 420);
            this.Controls.Add(this.btnTestCrossBind);
            this.Controls.Add(this.btnGenerateBinding);
            this.Controls.Add(this.btnRunSelect);
            this.Controls.Add(this.txtPath);
            this.Controls.Add(this.btnLoad);
            this.Controls.Add(this.btnRun);
            this.Controls.Add(this.splitContainer1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "TestMainForm";
            this.Text = "TestAll";
            this.Load += new System.EventHandler(this.OnFormLoaded);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
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
        private System.Windows.Forms.Button btnGenerateBinding;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button btnTestCrossBind;
    }
}