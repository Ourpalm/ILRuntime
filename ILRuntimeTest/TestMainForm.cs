using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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
        TestSession session;
        private Assembly _assembly;
        private List<TestResultInfo> _resList = new List<TestResultInfo>();

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

            if (session == null)
                return;

            if (session.TestList.Count <= 0)
                return;
            _resList.Clear();
            foreach (var unit in session.TestList)
            {
                unit.Run(true);
                _resList.Add(unit.CheckResult());
            }

            listView1.Items.Clear();
            StringBuilder sb = new StringBuilder();
            foreach (var resInfo in _resList)
            {
                sb.Append("Test:");
                sb.AppendLine(resInfo.TestName);
                sb.Append("TestResult:");
                sb.AppendLine(resInfo.Result == TestResults.Failed && resInfo.HasTodo ? $"{resInfo.Result}(Has TODO)" : resInfo.Result.ToString());
                sb.AppendLine("Log:");
                sb.AppendLine(resInfo.Message);
                sb.AppendLine("=======================");
                var item = new ListViewItem(resInfo.TestName);
                item.SubItems.Add(resInfo.Result.ToString());
                switch (resInfo.Result)
                {
                    case TestResults.Pass:
                    case TestResults.Ignored:
                        item.BackColor = Color.Green;
                        break;
                    case TestResults.Failed:
                        if(resInfo.HasTodo)
                            item.BackColor = Color.Yellow;
                        else
                            item.BackColor = Color.Red;
                        break;
                }
                listView1.Items.Add(item);
            }
            tbLog.Text = sb.ToString();
        }

        private void OnBtnLoad(object sender, EventArgs e)
        {
            session?.Dispose();
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
                Properties.Settings.Default["assembly_path"] = txtPath.Text;
                Properties.Settings.Default.Save();
                session = new TestSession();
                session.Load(txtPath.Text, cbEnableRegVM.Checked);
                _isLoadAssembly = true;
                LoadTest();
                UpdateBtnState();
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

            var testUnit = session.TestList[_selectItemArgs.ItemIndex];

            testUnit.Run();
            var res = testUnit.CheckResult();

            _selectItemArgs.Item.SubItems[1].Text = res.Result == TestResults.Failed && res.HasTodo ? $"{res.Result}(Has TODO)" : res.Result.ToString();
            switch (res.Result)
            {
                case TestResults.Pass:
                case TestResults.Ignored:
                    _selectItemArgs.Item.BackColor = Color.Green;
                    break;
                case TestResults.Failed:
                    if (res.HasTodo)
                        _selectItemArgs.Item.BackColor = Color.Yellow;
                    else
                        _selectItemArgs.Item.BackColor = Color.Red;
                    break;
            }

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
            if (e.ItemIndex > session.TestList.Count - 1)
            {
                Console.WriteLine("select index out of range");
                return;
            }

            _selectItemArgs = e;
            UpdateBtnState();
        }


        private void LoadTest()
        {
            _resList.Clear();
            listView1.Items.Clear();
            foreach (var testUnit in session.TestList)
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
            msg = ILRuntime.Runtime.Enviorment.CrossBindingCodeGenerator.GenerateCrossBindingAdapterCode(typeof(IAsyncStateMachine), "ILRuntimeTest");
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
                string outputPath = ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + "ILRuntimeTestBase/AutoGenerate"; // "..\\..\\AutoGenerate"
                ILRuntime.Runtime.CLRBinding.BindingCodeGenerator.GenerateBindingCode(domain, outputPath);
            }
        }
    }
}
