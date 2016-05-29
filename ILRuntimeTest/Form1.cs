using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using ILRuntime.Runtime.Enviorment;
namespace ILRuntimeTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            ILRuntime.Runtime.Debugger.DebugService.Instance.OnBreakPoint += (str) =>
            {
                MessageBox.Show(str);
            };
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (OD.ShowDialog() == DialogResult.OK)
            {
                using (System.IO.FileStream fs = new System.IO.FileStream(OD.FileName, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    ILRuntime.Runtime.Enviorment.AppDomain app = new ILRuntime.Runtime.Enviorment.AppDomain();
                    string path = System.IO.Path.GetDirectoryName(OD.FileName);
                    string name = System.IO.Path.GetFileNameWithoutExtension(OD.FileName);
                    using (System.IO.FileStream fs2 = new System.IO.FileStream(string.Format("{0}\\{1}.pdb", path, name), System.IO.FileMode.Open))
                        app.LoadAssembly(fs, fs2, new Mono.Cecil.Pdb.PdbReaderProvider());
                    System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                    sw.Start();
                    app.Invoke("TestCases.SimpleTest", "InstanceTest"); 
                    sw.Stop();
                    System.Diagnostics.Debugger.Log(2, "info", "Elappsed Time:" + sw.ElapsedMilliseconds + "ms\n");
                    
                }
            }
        }
    }
}
