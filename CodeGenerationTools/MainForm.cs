using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeGenerationTools
{
    public partial class MainForm : Form
    {
        private string _outputPath;
        private string _helperTmpd;
        private string _adaptorTmpd;
        private string _vmVoidTmpd;
        private string _vmReturnTmpd;
        private string _abmVoidTmpd;
        private string _abmReturnTmpd;

        private string _delegateVoidTmpd;
        private string _delegateReturnTmpd;


        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            _outputPath = Application.StartupPath + "/Output/";
            if (!Directory.Exists(_outputPath))
                Directory.CreateDirectory(_outputPath);
            textBox2.Text = Properties.Settings.Default["out_path"] as string;
            textBox3.Text = Properties.Settings.Default["assembly_path"] as string;

            LoadTemplates();
        }

        private void LoadButton_Click(object sender, EventArgs e)
        {
            var targetPath = textBox3.Text;

            if (targetPath == "")
            {
                if (OD.ShowDialog() != DialogResult.OK) return;

                Properties.Settings.Default["assembly_path"] = OD.FileName;
                Properties.Settings.Default.Save();
                textBox3.Text = targetPath = OD.FileName;
            }

            //if (OD.ShowDialog() != DialogResult.OK) return;

            textBox1.Text = "";

            using (var fs = new System.IO.FileStream(targetPath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                var assembly = Assembly.LoadFrom(targetPath);
                if (assembly == null) return;
                //types
                var types = assembly.GetTypes();

                //export adaptor
                int count = types.Length;
                int index = 0;

                var adptorAttr = assembly.GetType("ILRuntimeTest.TestFramework.NeedAdaptorAttribute");
                var delegateAttr = assembly.GetType("ILRuntimeTest.TestFramework.DelegateExportAttribute");

                foreach (var type in types)
                {
                    var attr = type.GetCustomAttribute(adptorAttr);
                    if (attr == null)
                        continue;
                    OnProgress($"-----generate type:{type.FullName}-----------", index++ / count);
                    CreateAdaptor(type);
                }

                //export helper
                var helperStr = _helperTmpd;
                Print("-------------------generate helper------------------------");

                var adptorStr = "";
                foreach (var type in types)
                {
                    var attr = type.GetCustomAttribute(adptorAttr);
                    if (attr == null)
                        continue;
                    adptorStr += CreateAdaptorInit(type);
                }
                helperStr = helperStr.Replace("{$AdptorInit}", adptorStr);

                var delegateStr = "";
                foreach (var type in types)
                {
                    var attr = type.GetCustomAttribute(delegateAttr);
                    if (attr == null)
                        continue;
                    delegateStr += CreateDelegateInit(type);
                }
                helperStr = helperStr.Replace("{$DelegateInit}", delegateStr);

                using (var fs2 = File.Create(_outputPath + "helper.cs"))
                {
                    var sw = new StreamWriter(fs2);
                    sw.Write(helperStr);
                    sw.Flush();
                }

                Print("-------------------generate end------------------------");

            }
        }

        private void CopyButton_Click(object sender, EventArgs e)
        {
            var targetPath = textBox2.Text;

            if (targetPath == "")
            {
                if (FD.ShowDialog() != DialogResult.OK) return;

                Properties.Settings.Default["out_path"] = FD.SelectedPath;
                Properties.Settings.Default.Save();
                textBox2.Text = targetPath = FD.SelectedPath;
            }

            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }

            var files = Directory.GetFiles(_outputPath);
            if (files.Length == 0)
            {
                MessageBox.Show("no file to copy,please generate code first");
                return;
            }

            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);
                File.Copy(file, targetPath + "/" + fileName, true);
            }

            MessageBox.Show("file copied");
        }

        private void OnProgress(string s, int i)
        {
            textBox1.Text += s + "\r\n";
            progressBar.Value = i;
        }

        private void Print(string s)
        {
            textBox1.Text += s + "\r\n";
        }

        private void LoadTemplates()
        {
            var tmpdPath = Application.StartupPath + "/Template/";
            //load helper.tmpd
            _helperTmpd = File.ReadAllText(tmpdPath + "helper.tmpd");
            //load adaptor.tmpd
            _adaptorTmpd = File.ReadAllText(tmpdPath + "adaptor.tmpd");
            //load vmethod.tmpd
            _vmVoidTmpd = File.ReadAllText(tmpdPath + "method_virtual_void.tmpd");
            _vmReturnTmpd = File.ReadAllText(tmpdPath + "method_virtual_return.tmpd");
            //load abmethod.tmpd
            _abmVoidTmpd = File.ReadAllText(tmpdPath + "method_abstract_void.tmpd");
            _abmReturnTmpd = File.ReadAllText(tmpdPath + "method_abstract_return.tmpd");
            //load the delegate.tmpd
            _delegateVoidTmpd = File.ReadAllText(tmpdPath + "delegate_void.tmpd");
            _delegateReturnTmpd = File.ReadAllText(tmpdPath + "delegate_return.tmpd");
        }

        private string CreateAdaptorInit(Type type)
        {
            Print($"------adaptor Init:{type.Name}-----------");

            return $"app.RegisterCrossBindingAdaptor(new {type.FullName + "Adaptor"}());\r\n";
        }

        private string CreateDelegateInit(Type type)
        {
            Print($"------delegate Init:{type.Name}-----------");

            var method = type.GetMethod("Invoke");
            var tmpd = method.ReturnType == typeof(void) ? _delegateVoidTmpd : _delegateReturnTmpd;
            var argsType = "";
            var args = "";
            var returnType = method.ReturnType == typeof(void) ? "" : method.ReturnType.Name;
            foreach (var param in method.GetParameters())
            {
                argsType += param.ParameterType.Name + ",";
                args += param.Name + ",";
            }
            argsType = argsType.Trim(',');
            args = args.Trim(',');
            tmpd = tmpd.Replace("{$DelegateName}", type.FullName);
            tmpd = tmpd.Replace("{$argsType}", argsType);
            tmpd = tmpd.Replace("{$args}", args);
            if (method.ReturnType != typeof(void))
                tmpd = tmpd.Replace("{$returnType}", returnType);

            return tmpd;
        }

        private void CreateAdaptor(Type type)
        {
            var adaptorName = type.Name + "Adaptor";
            var classbody = _adaptorTmpd;
            var methodsbody = "";

            using (var fs = File.Create(_outputPath + adaptorName + ".cs"))
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);//| BindingFlags.DeclaredOnly
                foreach (var methodInfo in methods.Where(methodInfo => methodInfo.DeclaringType != typeof(object)))
                {
                    if (methodInfo.IsAbstract)
                    {
                        methodsbody += CreateAbstractMethod(methodInfo);
                    }
                    else if (methodInfo.IsVirtual)
                    {
                        methodsbody += CreateVirtualMethod(methodInfo);
                    }
                }

                classbody = classbody.Replace("{$ClassName}", type.Name);
                classbody = classbody.Replace("{$MethodArea}", methodsbody);
                var sw = new StreamWriter(fs);
                sw.Write(classbody);
                sw.Flush();
            }


        }

        private string CreateVirtualMethod(MethodInfo methodInfo)
        {
            Print($"------method:{methodInfo.Name}-----{methodInfo.DeclaringType}------");
            var methodStr = methodInfo.ReturnType == typeof(void) ? _vmVoidTmpd : _vmReturnTmpd;
            var argStr = "";
            var argNoTypeStr = "";
            methodStr = methodStr.Replace("{$VMethodName}", methodInfo.Name);
            foreach (var pInfo in methodInfo.GetParameters())
            {
                argStr += pInfo.ParameterType.Name + " " + pInfo.Name + ",";
                argNoTypeStr += pInfo.Name + ",";
            }
            argStr = argStr.Trim(',');
            argNoTypeStr = argNoTypeStr.Trim(',');
            methodStr = methodStr.Replace("{$args}", argStr);
            methodStr = methodStr.Replace("{$args_no_type}", argNoTypeStr);

            methodStr = methodStr.Replace("{$comma}", argStr == "" ? "" : ",");
            methodStr = methodStr.Replace("{$modifier}", methodInfo.Accessmodifier().ToString().ToLower());

            if (methodInfo.ReturnType != typeof(void))
                methodStr = methodStr.Replace("{$returnType}", methodInfo.ReturnType.Name);

            return methodStr;
        }

        private string CreateAbstractMethod(MethodInfo methodInfo)
        {
            Print($"------method:{methodInfo.Name}-----{methodInfo.DeclaringType}------");
            var methodStr = methodInfo.ReturnType == typeof(void) ? _abmVoidTmpd : _abmReturnTmpd;
            string argStr = "";
            string argNoTypeStr = "";
            methodStr = methodStr.Replace("{$AMethodName}", methodInfo.Name);
            foreach (var pInfo in methodInfo.GetParameters())
            {
                argStr += pInfo.ParameterType.Name + " " + pInfo.Name + ",";
                argNoTypeStr += pInfo.Name + ",";
            }
            argStr = argStr.Trim(',');
            argNoTypeStr = argNoTypeStr.Trim(',');
            methodStr = methodStr.Replace("{$args}", argStr);
            methodStr = methodStr.Replace("{$args_no_type}", argNoTypeStr);

            methodStr = methodStr.Replace("{$comma}", argStr == "" ? "" : ",");
            methodStr = methodStr.Replace("{$modifier}", methodInfo.Accessmodifier().ToString().ToLower());

            if (methodInfo.ReturnType == typeof(void)) return methodStr;

            methodStr = methodStr.Replace("{$returnType}", methodInfo.ReturnType.Name);
            var returnStr = methodInfo.ReturnType.IsValueType ? "return 0;" : "return null;";
            methodStr = methodStr.Replace("{$returnDefault}", returnStr);

            return methodStr;
        }


    }
}