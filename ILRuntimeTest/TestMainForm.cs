using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Runtime.Enviorment;
using ILRuntimeTest.Test;
using ILRuntimeTest.TestBase;
using ILRuntimeTest.TestFramework;

namespace ILRuntimeTest
{
    public partial class TestMainForm : Form
    {
        public static ILRuntime.Runtime.Enviorment.AppDomain _app;
        private Assembly _assembly;
        FileStream fs, fs2;
        private List<TestResultInfo> _resList = new List<TestResultInfo>();
        private List<BaseTestUnit> _testUnitList = new List<BaseTestUnit>();

        private ListViewItemSelectionChangedEventArgs _selectItemArgs = null;
        private bool _isLoadAssembly;

        public TestMainForm()
        {
            InitializeComponent();
            ColumnHeader[] row1 =
            {
                new ColumnHeader() {Name = "TestName", Text = "TestName", Width = 300},
                new ColumnHeader() {Name = "Result", Text = "TestResult", Width = 200},
                //new ColumnHeader() {Name = "Run", Text = "Click", Width = 200},
            };

            listView1.Columns.AddRange(row1);
            listView1.View = View.Details;
            _app = new ILRuntime.Runtime.Enviorment.AppDomain();
            _app.DebugService.StartDebugService(56000);
        }

        private void OnBtnRun(object sender, EventArgs e)
        {
            //if (_assembly != null)
            //{
            //    //types
            //    var types = _assembly.GetTypes();
            //    foreach (var type in types)
            //    {
            //        if (type.IsGenericTypeDefinition)
            //            continue;
            //        //methods
            //        var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            //        foreach (var methodInfo in methods)
            //        {
            //            string fullName = string.Format("{0}.{1}", type.Namespace, type.Name);
            //            //Console.WriteLine("call the method:{0},return type {1},params count{2}", fullName + "." + methodInfo.Name, methodInfo.ReturnType, methodInfo.GetParameters().Length);
            //            //目前只支持无参数，无返回值测试
            //            if (methodInfo.GetParameters().Length == 0)
            //            {
            //                var testUnit = new StaticTestUnit();
            //                testUnit.Init(_app, fullName, methodInfo.Name);
            //                testUnit.Run();
            //                _resList.Add(testUnit.CheckResult());
            //            }
            //        }
            //    }
            //}

            if (_app == null)
                return;

            if (_testUnitList.Count <= 0)
                return;
            _resList.Clear();
            foreach (var unit in _testUnitList)
            {
                unit.Run();
                _resList.Add(unit.CheckResult());
            }

            listView1.Items.Clear();
            StringBuilder sb = new StringBuilder();
            foreach (var resInfo in _resList)
            {
                sb.Append("Test:");
                sb.AppendLine(resInfo.TestName);
                sb.Append("TestResult:");
                sb.AppendLine(resInfo.Result.ToString());
                sb.AppendLine("Log:");
                sb.AppendLine(resInfo.Message);
                sb.AppendLine("=======================");
                var item = new ListViewItem(resInfo.TestName);
                item.SubItems.Add(resInfo.Result.ToString());
                item.BackColor = resInfo.Result ? Color.Green : Color.Red;
                listView1.Items.Add(item);
            }
            tbLog.Text = sb.ToString();
        }

        private void OnBtnLoad(object sender, EventArgs e)
        {
            if (fs != null)
                fs.Close();
            if (fs2 != null)
                fs2.Close();
            if (txtPath.Text == "")
            {
                if (OD.ShowDialog() == DialogResult.OK)
                {
                    Properties.Settings.Default["assembly_path"] = txtPath.Text = OD.FileName;
                    Properties.Settings.Default.Save();
                }
                else
                {
                    return;
                }
            }

            try
            {
                fs = new FileStream(txtPath.Text, FileMode.Open, FileAccess.Read);
                {
                    var path = Path.GetDirectoryName(txtPath.Text);
                    var name = Path.GetFileNameWithoutExtension(txtPath.Text);
                    var pdbPath = Path.Combine(path, name) + ".pdb";
                    if (!File.Exists(pdbPath)) {
                        name = Path.GetFileName(txtPath.Text);
                        pdbPath = Path.Combine(path, name) + ".mdb";
                    }

                    fs2 = new System.IO.FileStream(pdbPath, FileMode.Open);
                    {
                        ILRuntime.Mono.Cecil.Cil.ISymbolReaderProvider symbolReaderProvider = null;
                        if (pdbPath.EndsWith (".pdb")) {
                            symbolReaderProvider = new ILRuntime.Mono.Cecil.Pdb.PdbReaderProvider ();
                        }/* else if (pdbPath.EndsWith (".mdb")) {
                            symbolReaderProvider = new Mono.Cecil.Mdb.MdbReaderProvider ();
                        }*/

                        _app.LoadAssembly(fs, fs2, symbolReaderProvider);
                        _isLoadAssembly = true;
                    }

                    ILRuntimeHelper.Init(_app);
                    ILRuntime.Runtime.Generated.CLRBindings.Initialize(_app);

                    LoadTest();
                    UpdateBtnState();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("[Error:]" + ex);
            }

        }

        private void OnBtnRunSelect(object sender, EventArgs e)
        {
            if (_selectItemArgs == null)
                return;

            var testUnit = _testUnitList[_selectItemArgs.ItemIndex];
            testUnit.Run();
            var res = testUnit.CheckResult();
            _selectItemArgs.Item.SubItems[1].Text = res.Result.ToString();
            _selectItemArgs.Item.BackColor = res.Result ? Color.Green : Color.Red;

            StringBuilder sb = new StringBuilder();
            sb.Append("Test:");
            sb.AppendLine(res.TestName);
            sb.Append("TestResult:");
            sb.AppendLine(res.Result.ToString());
            sb.AppendLine("Log:");
            sb.AppendLine(res.Message);
            sb.AppendLine("=======================");
            tbLog.Text = sb.ToString();
        }

        private void OnFormLoaded(object sender, EventArgs e)
        {
            txtPath.Text = Properties.Settings.Default["assembly_path"] as string;
            listView1.ItemSelectionChanged += OnItemSelectChanged;
            btnRunSelect.Enabled = false;
            btnRun.Enabled = false;
        }

        private void OnItemSelectChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (e.ItemIndex > _testUnitList.Count - 1)
            {
                Console.WriteLine("select index out of range");
                return;
            }

            _selectItemArgs = e;
            UpdateBtnState();
        }


        private void LoadTest()
        {
            _testUnitList.Clear();
            _resList.Clear();

            var types = _app.LoadedTypes.Values.ToList();
            foreach (var type in types)
            {
                var ilType = type as ILType;
                if (ilType == null)
                    continue;
                var methods = ilType.GetMethods();
                foreach (var methodInfo in methods)
                {
                    string fullName = ilType.FullName;
                    //Console.WriteLine("call the method:{0},return type {1},params count{2}", fullName + "." + methodInfo.Name, methodInfo.ReturnType, methodInfo.GetParameters().Length);
                    //目前只支持无参数，无返回值测试
                    if (methodInfo.ParameterCount == 0 && methodInfo.IsStatic && ((ILRuntime.CLR.Method.ILMethod)methodInfo).Definition.IsPublic)
                    {
                        var testUnit = new StaticTestUnit();
                        testUnit.Init(_app, fullName, methodInfo.Name);
                        _testUnitList.Add(testUnit);
                    }
                }
            }

            listView1.Items.Clear();
            foreach (var testUnit in _testUnitList)
            {
                var item = new ListViewItem(testUnit.TestName);
                item.SubItems.Add("--");
                //item.BackColor = testUnit.Result ? Color.Green : Color.Red;
                listView1.Items.Add(item);
            }
        }

        private void UpdateBtnState()
        {
            btnRun.Enabled = _isLoadAssembly;
            btnRunSelect.Enabled = _selectItemArgs != null;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var msg = ILRuntime.Runtime.Enviorment.CrossBindingCodeGenerator.GenerateCrossBindingAdapterCode(typeof(TestClass2), "ILRuntimeTest");
            MessageBox.Show(msg);
            msg = ILRuntime.Runtime.Enviorment.CrossBindingCodeGenerator.GenerateCrossBindingAdapterCode(typeof(IDisposable), "ILRuntimeTest");
            MessageBox.Show(msg);
        }

        private void btnGenerateBinding_Click(object sender, EventArgs e)
        {
            /*List<Type> types = new List<Type>();
            types.Add(typeof(int));
            types.Add(typeof(float));
            types.Add(typeof(long));
            types.Add(typeof(object));
            types.Add(typeof(string));
            types.Add(typeof(ValueType));
            types.Add(typeof(Console));
            types.Add(typeof(Array));
            types.Add(typeof(Dictionary<string, int>));
            types.Add(typeof(Dictionary<ILRuntime.Runtime.Intepreter.ILTypeInstance, int>));
            types.Add(typeof(TestFramework.TestStruct));
            ILRuntime.Runtime.CLRBinding.BindingCodeGenerator.GenerateBindingCode(types, "..\\..\\AutoGenerate");*/
            ILRuntime.Runtime.Enviorment.AppDomain domain = new ILRuntime.Runtime.Enviorment.AppDomain();
            using (FileStream fs = new FileStream(txtPath.Text, FileMode.Open, FileAccess.Read))
            {
                domain.LoadAssembly(fs);

                //Crossbind Adapter is needed to generate the correct binding code
                ILRuntimeHelper.Init(domain);
                string outputPath = ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + "AutoGenerate"; // "..\\..\\AutoGenerate"
                ILRuntime.Runtime.CLRBinding.BindingCodeGenerator.GenerateBindingCode(domain, outputPath);
            }
        }
    }
}
