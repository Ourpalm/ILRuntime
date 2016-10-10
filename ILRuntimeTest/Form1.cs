using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

using ILRuntime.Runtime.Enviorment;
using ILRuntimeTest.Test;
using ILRuntimeTest.TestBase;

namespace ILRuntimeTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            /*ILRuntime.Runtime.Debugger.DebugService.Instance.OnBreakPoint += (str) =>
            {
                MessageBox.Show(str);
            };*/

            ColumnHeader[] row1 =
            {
                new ColumnHeader() {Name = "TestName", Text = "TestName", Width = 300},
                new ColumnHeader() {Name = "Result", Text = "TestResult", Width = 200},
            };

            listView1.Columns.AddRange(row1);
            listView1.View = View.Details;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (OD.ShowDialog() == DialogResult.OK)
            {
                using (System.IO.FileStream fs = new System.IO.FileStream(OD.FileName, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    var app = new ILRuntime.Runtime.Enviorment.AppDomain();
                    var path = System.IO.Path.GetDirectoryName(OD.FileName);
                    var name = System.IO.Path.GetFileNameWithoutExtension(OD.FileName);
                    using (System.IO.FileStream fs2 = new System.IO.FileStream(string.Format("{0}\\{1}.pdb", path, name), System.IO.FileMode.Open))
                        app.LoadAssembly(fs, fs2, new Mono.Cecil.Pdb.PdbReaderProvider());
                    app.DelegateManager.RegisterDelegateConvertor<TestFramework.IntDelegate>((action) =>
                    {
                        return new TestFramework.IntDelegate((a) =>
                        {
                            ((Action<int>)action)(a);
                        });
                    });
                    app.DelegateManager.RegisterDelegateConvertor<TestFramework.IntDelegate2>((action) =>
                    {
                        return new TestFramework.IntDelegate2((a) =>
                        {
                            return ((Func<int, int>)action)(a);
                        });
                    });
                    /*app.RegisterCLRMethodRedirection(typeof(UnitTest.Logger).GetMethod("Log"), (ctx, instance, param, ga) =>
                    {
                        Console.WriteLine(param[0]);
                        return null;
                    });
                    app.DelegateManager.RegisterDelegateConvertor<TestDele.Action2<int, string>>((action) =>
                    {
                        return new TestDele.Action2<int, string>((a, b) =>
                        {
                            ((Action<int, string>)action)(a, b);
                        });
                    });
                    
                    app.DelegateManager.RegisterDelegateConvertor<UnitTest.Perform.Action>((action) =>
                    {
                        return new UnitTest.Perform.Action(() =>
                        {
                            ((Action)action)();
                        });
                    });
                    app.DelegateManager.RegisterDelegateConvertor<TestDele.myup>((action) =>
                    {
                        return new TestDele.myup(() =>
                        {
                            ((Action)action)();
                        });
                    });
                   
                    app.DelegateManager.RegisterDelegateConvertor<Comparison<int>>((action) =>
                  {
                      return new Comparison<int>((a, b) =>
                      {
                          return ((Func<int, int, int>)action)(a, b);
                      });
                  });
                  */
                    app.DelegateManager.RegisterMethodDelegate<int>();
                    //app.DelegateManager.RegisterMethodDelegate<int, string>();
                    app.DelegateManager.RegisterFunctionDelegate<int, int>();
                    //app.DelegateManager.RegisterFunctionDelegate<int, int, int>();
                    //app.DelegateManager.RegisterMethodDelegate<MyClass2>();
                    List<TestResultInfo> resList = new List<TestResultInfo>();
                    Assembly assembly = Assembly.LoadFrom(OD.FileName);
                    if (assembly != null)
                    {
                        //types
                        var types = assembly.GetTypes();
                        foreach (var type in types)
                        {
                            if (type.IsGenericTypeDefinition)
                                continue;
                            //methods
                            var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                            foreach (var methodInfo in methods)
                            {
                                string fullName = string.Format("{0}.{1}", type.Namespace, type.Name);
                                //Console.WriteLine("call the method:{0},return type {1},params count{2}", fullName + "." + methodInfo.Name, methodInfo.ReturnType, methodInfo.GetParameters().Length);
                                //目前只支持无参数，无返回值测试
                                if (methodInfo.GetParameters().Length == 0)
                                {
                                    var testUnit = new StaticTestUnit();
                                    testUnit.Init(app, fullName, methodInfo.Name);
                                    testUnit.Run();
                                    resList.Add(testUnit.CheckResult());
                                }
                            }
                        }
                    }

                    listView1.Items.Clear();
                    StringBuilder sb = new StringBuilder();
                    foreach (var resInfo in resList)
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
            }

        }
    }
}
