using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ILRuntimeDebuggerLauncher
{
    public partial class FrmLauncher : Form
    {
        public string Host { get; set; }
        public FrmLauncher()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Host = tbAddress.Text;
            Close();
        }
    }
}
