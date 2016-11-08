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
            this.outputText = new System.Windows.Forms.TextBox();
            this.CopyButton = new System.Windows.Forms.Button();
            this.outputPath = new System.Windows.Forms.TextBox();
            this.FD = new System.Windows.Forms.FolderBrowserDialog();
            this.sourcePath1 = new System.Windows.Forms.TextBox();
            this.sourcePath2 = new System.Windows.Forms.TextBox();
            this.AddButton2 = new System.Windows.Forms.Button();
            this.AddButton1 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // LoadButton
            // 
            this.LoadButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.LoadButton.Location = new System.Drawing.Point(559, 325);
            this.LoadButton.Margin = new System.Windows.Forms.Padding(2);
            this.LoadButton.Name = "LoadButton";
            this.LoadButton.Size = new System.Drawing.Size(74, 32);
            this.LoadButton.TabIndex = 0;
            this.LoadButton.Text = "Generate";
            this.LoadButton.UseVisualStyleBackColor = true;
            this.LoadButton.Click += new System.EventHandler(this.GenerateClick);
            // 
            // OD
            // 
            this.OD.FileName = "openFileDialog1";
            // 
            // outputText
            // 
            this.outputText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.outputText.Location = new System.Drawing.Point(6, 79);
            this.outputText.Margin = new System.Windows.Forms.Padding(2);
            this.outputText.Multiline = true;
            this.outputText.Name = "outputText";
            this.outputText.ReadOnly = true;
            this.outputText.Size = new System.Drawing.Size(711, 239);
            this.outputText.TabIndex = 2;
            // 
            // CopyButton
            // 
            this.CopyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CopyButton.Location = new System.Drawing.Point(637, 326);
            this.CopyButton.Margin = new System.Windows.Forms.Padding(2);
            this.CopyButton.Name = "CopyButton";
            this.CopyButton.Size = new System.Drawing.Size(80, 30);
            this.CopyButton.TabIndex = 3;
            this.CopyButton.Text = "Copy";
            this.CopyButton.UseVisualStyleBackColor = true;
            this.CopyButton.Click += new System.EventHandler(this.CopyFileClick);
            // 
            // outputPath
            // 
            this.outputPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.outputPath.Location = new System.Drawing.Point(124, 333);
            this.outputPath.Margin = new System.Windows.Forms.Padding(2);
            this.outputPath.Name = "outputPath";
            this.outputPath.Size = new System.Drawing.Size(426, 21);
            this.outputPath.TabIndex = 4;
            // 
            // sourcePath1
            // 
            this.sourcePath1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.sourcePath1.Location = new System.Drawing.Point(124, 14);
            this.sourcePath1.Margin = new System.Windows.Forms.Padding(2);
            this.sourcePath1.Name = "sourcePath1";
            this.sourcePath1.Size = new System.Drawing.Size(426, 21);
            this.sourcePath1.TabIndex = 5;
            // 
            // sourcePath2
            // 
            this.sourcePath2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.sourcePath2.Location = new System.Drawing.Point(124, 49);
            this.sourcePath2.Margin = new System.Windows.Forms.Padding(2);
            this.sourcePath2.Name = "sourcePath2";
            this.sourcePath2.Size = new System.Drawing.Size(426, 21);
            this.sourcePath2.TabIndex = 6;
            // 
            // AddButton2
            // 
            this.AddButton2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.AddButton2.Location = new System.Drawing.Point(559, 43);
            this.AddButton2.Margin = new System.Windows.Forms.Padding(2);
            this.AddButton2.Name = "AddButton2";
            this.AddButton2.Size = new System.Drawing.Size(158, 32);
            this.AddButton2.TabIndex = 7;
            this.AddButton2.Text = "Load ILScript Assembly";
            this.AddButton2.UseVisualStyleBackColor = true;
            this.AddButton2.Click += new System.EventHandler(this.LoadILScriptAssemblyClick);
            // 
            // AddButton1
            // 
            this.AddButton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.AddButton1.Location = new System.Drawing.Point(559, 7);
            this.AddButton1.Margin = new System.Windows.Forms.Padding(2);
            this.AddButton1.Name = "AddButton1";
            this.AddButton1.Size = new System.Drawing.Size(158, 32);
            this.AddButton1.TabIndex = 8;
            this.AddButton1.Text = "Load Main Assembly";
            this.AddButton1.UseVisualStyleBackColor = true;
            this.AddButton1.Click += new System.EventHandler(this.LoadMainProjectAssemblyClick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(107, 12);
            this.label1.TabIndex = 9;
            this.label1.Text = "MainProject Path:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(30, 52);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(89, 12);
            this.label2.TabIndex = 10;
            this.label2.Text = "ILScript Path:";
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(42, 336);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 12);
            this.label3.TabIndex = 11;
            this.label3.Text = "Output Path:";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(728, 367);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.AddButton1);
            this.Controls.Add(this.AddButton2);
            this.Controls.Add(this.sourcePath2);
            this.Controls.Add(this.sourcePath1);
            this.Controls.Add(this.outputPath);
            this.Controls.Add(this.CopyButton);
            this.Controls.Add(this.outputText);
            this.Controls.Add(this.LoadButton);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "MainForm";
            this.Text = "CodeGenerationTools";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button LoadButton;
        private System.Windows.Forms.OpenFileDialog OD;
        private System.Windows.Forms.TextBox outputText;
        private System.Windows.Forms.Button CopyButton;
        private System.Windows.Forms.TextBox outputPath;
        private System.Windows.Forms.FolderBrowserDialog FD;
        private System.Windows.Forms.TextBox sourcePath1;
        private System.Windows.Forms.TextBox sourcePath2;
        private System.Windows.Forms.Button AddButton2;
        private System.Windows.Forms.Button AddButton1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
    }
}

