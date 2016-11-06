namespace CodeGenerationTools
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
            this.LoadButton = new System.Windows.Forms.Button();
            this.OD = new System.Windows.Forms.OpenFileDialog();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.CopyButton = new System.Windows.Forms.Button();
            this.outputPath = new System.Windows.Forms.TextBox();
            this.FD = new System.Windows.Forms.FolderBrowserDialog();
            this.sourcePath1 = new System.Windows.Forms.TextBox();
            this.sourcePath2 = new System.Windows.Forms.TextBox();
            this.AddButton2 = new System.Windows.Forms.Button();
            this.AddButton1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // LoadButton
            // 
            this.LoadButton.Location = new System.Drawing.Point(417, 407);
            this.LoadButton.Margin = new System.Windows.Forms.Padding(2);
            this.LoadButton.Name = "LoadButton";
            this.LoadButton.Size = new System.Drawing.Size(74, 32);
            this.LoadButton.TabIndex = 0;
            this.LoadButton.Text = "Load";
            this.LoadButton.UseVisualStyleBackColor = true;
            this.LoadButton.Click += new System.EventHandler(this.LoadButton_Click);
            // 
            // OD
            // 
            this.OD.FileName = "openFileDialog1";
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(6, 92);
            this.progressBar.Margin = new System.Windows.Forms.Padding(2);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(569, 24);
            this.progressBar.TabIndex = 1;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(6, 120);
            this.textBox1.Margin = new System.Windows.Forms.Padding(2);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(569, 280);
            this.textBox1.TabIndex = 2;
            // 
            // CopyButton
            // 
            this.CopyButton.Location = new System.Drawing.Point(495, 408);
            this.CopyButton.Margin = new System.Windows.Forms.Padding(2);
            this.CopyButton.Name = "CopyButton";
            this.CopyButton.Size = new System.Drawing.Size(80, 30);
            this.CopyButton.TabIndex = 3;
            this.CopyButton.Text = "Copy";
            this.CopyButton.UseVisualStyleBackColor = true;
            this.CopyButton.Click += new System.EventHandler(this.CopyButton_Click);
            // 
            // outputPath
            // 
            this.outputPath.Location = new System.Drawing.Point(6, 415);
            this.outputPath.Margin = new System.Windows.Forms.Padding(2);
            this.outputPath.Name = "outputPath";
            this.outputPath.Size = new System.Drawing.Size(402, 21);
            this.outputPath.TabIndex = 4;
            // 
            // sourcePath1
            // 
            this.sourcePath1.Location = new System.Drawing.Point(6, 14);
            this.sourcePath1.Margin = new System.Windows.Forms.Padding(2);
            this.sourcePath1.Name = "sourcePath1";
            this.sourcePath1.Size = new System.Drawing.Size(485, 21);
            this.sourcePath1.TabIndex = 5;
            // 
            // sourcePath2
            // 
            this.sourcePath2.Location = new System.Drawing.Point(6, 49);
            this.sourcePath2.Margin = new System.Windows.Forms.Padding(2);
            this.sourcePath2.Name = "sourcePath2";
            this.sourcePath2.Size = new System.Drawing.Size(485, 21);
            this.sourcePath2.TabIndex = 6;
            // 
            // AddButton2
            // 
            this.AddButton2.Location = new System.Drawing.Point(501, 43);
            this.AddButton2.Margin = new System.Windows.Forms.Padding(2);
            this.AddButton2.Name = "AddButton2";
            this.AddButton2.Size = new System.Drawing.Size(74, 32);
            this.AddButton2.TabIndex = 7;
            this.AddButton2.Text = "Add";
            this.AddButton2.UseVisualStyleBackColor = true;
            this.AddButton2.Click += new System.EventHandler(this.LoadUnityAssemblyClick);
            // 
            // AddButton1
            // 
            this.AddButton1.Location = new System.Drawing.Point(501, 7);
            this.AddButton1.Margin = new System.Windows.Forms.Padding(2);
            this.AddButton1.Name = "AddButton1";
            this.AddButton1.Size = new System.Drawing.Size(74, 32);
            this.AddButton1.TabIndex = 8;
            this.AddButton1.Text = "Add";
            this.AddButton1.UseVisualStyleBackColor = true;
            this.AddButton1.Click += new System.EventHandler(this.LoadILAssemblyClick);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(586, 449);
            this.Controls.Add(this.AddButton1);
            this.Controls.Add(this.AddButton2);
            this.Controls.Add(this.sourcePath2);
            this.Controls.Add(this.sourcePath1);
            this.Controls.Add(this.outputPath);
            this.Controls.Add(this.CopyButton);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.LoadButton);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "MainForm";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button LoadButton;
        private System.Windows.Forms.OpenFileDialog OD;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button CopyButton;
        private System.Windows.Forms.TextBox outputPath;
        private System.Windows.Forms.FolderBrowserDialog FD;
        private System.Windows.Forms.TextBox sourcePath1;
        private System.Windows.Forms.TextBox sourcePath2;
        private System.Windows.Forms.Button AddButton2;
        private System.Windows.Forms.Button AddButton1;
    }
}

