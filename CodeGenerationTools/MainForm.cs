using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace CodeGenerationTools
{
    public partial class MainForm : Form
    {
        private string _outputPath;
        private string _helperTmpd;
        private string _adaptorTmpd;
        private string _adaptorInterfaceTmpd;
        private string _vmVoidTmpd;
        private string _vmReturnTmpd;
        private string _abmVoidTmpd;
        private string _abmReturnTmpd;

        private string _delegateVoidTmpd;
        private string _delegateReturnTmpd;

        private string _adaptorRegisterTmpd;
        private string _actionRegisterTmpd;
        private string _functionRegisterTmpd;

        private HashSet<Type> _adaptorSet = new HashSet<Type>();
        private Dictionary<string, object> _delegateRegisterSet = new Dictionary<string, object>();
        private HashSet<Type> _delegateSet = new HashSet<Type>();


        private static readonly string AdaptorAttrName = "ILRuntimeTest.TestFramework.NeedAdaptorAttribute";
        private static readonly string DelegateAttrName = "ILRuntimeTest.TestFramework.DelegateExportAttribute";
        private Type _adptorAttr;//= assembly.GetType("");
        private Type _delegateAttr;//= assembly.GetType("");

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            _outputPath = Application.StartupPath + "/Output/";
            if (!Directory.Exists(_outputPath))
                Directory.CreateDirectory(_outputPath);
            outputPath.Text = Properties.Settings.Default["out_path"] as string;
            sourcePath1.Text = Properties.Settings.Default["assembly_path"] as string;
            sourcePath2.Text = Properties.Settings.Default["assembly_path1"] as string;

            LoadTemplates();
        }

        private void LoadButton_Click(object sender, EventArgs e)
        {
            //var targetPath = sourcePath1.Text;

            //if (targetPath == "")
            //{
            //    if (OD.ShowDialog() != DialogResult.OK) return;

            //    Properties.Settings.Default["assembly_path"] = OD.FileName;
            //    Properties.Settings.Default.Save();
            //    sourcePath1.Text = targetPath = OD.FileName;
            //}

            //textBox1.Text = "";

            //var assembly = Assembly.LoadFrom(targetPath);
            //if (assembly == null) return;
            ////types
            //var types = assembly.GetTypes();

            ////export adaptor
            //int count = types.Length;
            //int index = 0;

            //var _adptorAttr = assembly.GetType("ILRuntimeTest.TestFramework.NeedAdaptorAttribute");
            //var _delegateAttr = assembly.GetType("ILRuntimeTest.TestFramework.DelegateExportAttribute");

            //foreach (var type in types)
            //{
            //    var attr = type.GetCustomAttribute(_adptorAttr, false);
            //    if (attr == null)
            //        continue;
            //    OnProgress($"-----generate type:{type.FullName}-----------", index++ / count);
            //    CreateAdaptor(type);
            //}

            ////export helper
            //var helperStr = _helperTmpd;
            //Print("-------------------generate helper------------------------");

            //var adptorStr = "";
            //foreach (var type in types)
            //{
            //    var attr = type.GetCustomAttribute(_adptorAttr, false);
            //    if (attr == null)
            //        continue;
            //    adptorStr += CreateAdaptorInit(type);
            //}
            //helperStr = helperStr.Replace("{$AdptorInit}", adptorStr);

            //var delegateStr = "";
            //foreach (var type in types)
            //{
            //    var attr = type.GetCustomAttribute(_delegateAttr, false);
            //    if (attr == null)
            //        continue;
            //    delegateStr += CreateDelegateInit(type);
            //}
            //helperStr = helperStr.Replace("{$DelegateInit}", delegateStr);

            //using (var fs2 = File.Create(_outputPath + "helper.cs"))
            //{
            //    var sw = new StreamWriter(fs2);
            //    sw.Write(helperStr);
            //    sw.Flush();
            //}

            foreach (var type in _adaptorSet)
            {
                CreateAdaptor(type);
            }

            CreateILRuntimeHelper();

            Print("-------------------generate end------------------------");
        }

        private void LoadDelegateRegister(string key, object type)
        {
            if (!_delegateRegisterSet.ContainsKey(key))
                _delegateRegisterSet.Add(key, type);
        }

        private void LoadDelegateConvertor(Type type)
        {
            if (!_delegateSet.Contains(type))
                _delegateSet.Add(type);
        }

        private void LoadAdaptor(Type type)
        {

            if (!_adaptorSet.Contains(type))
                _adaptorSet.Add(type);
        }

        private void CopyButton_Click(object sender, EventArgs e)
        {
            var targetPath = outputPath.Text;

            if (targetPath == "")
            {
                if (FD.ShowDialog() != DialogResult.OK) return;

                Properties.Settings.Default["out_path"] = FD.SelectedPath;
                Properties.Settings.Default.Save();
                outputPath.Text = targetPath = FD.SelectedPath;
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
            _adaptorInterfaceTmpd = File.ReadAllText(tmpdPath + "adaptor_interface.tmpd");
            //load vmethod.tmpd
            _vmVoidTmpd = File.ReadAllText(tmpdPath + "method_virtual_void.tmpd");
            _vmReturnTmpd = File.ReadAllText(tmpdPath + "method_virtual_return.tmpd");
            //load abmethod.tmpd
            _abmVoidTmpd = File.ReadAllText(tmpdPath + "method_abstract_void.tmpd");
            _abmReturnTmpd = File.ReadAllText(tmpdPath + "method_abstract_return.tmpd");
            //load the delegate.tmpd
            _delegateVoidTmpd = File.ReadAllText(tmpdPath + "delegate_void.tmpd");
            _delegateReturnTmpd = File.ReadAllText(tmpdPath + "delegate_return.tmpd");
            //load delegateRegister template
            _actionRegisterTmpd = File.ReadAllText(tmpdPath + "action_register.tmpd");
            _functionRegisterTmpd = File.ReadAllText(tmpdPath + "function_register.tmpd");
            _adaptorRegisterTmpd = File.ReadAllText(tmpdPath + "adaptor_register.tmpd");

        }

        private string CreateAdaptorInit(Type type)
        {
            Print($"------adaptor Init:{type.Name}-----------");
            var tmpd = _adaptorRegisterTmpd;
            return tmpd.Replace("{$TypeName}", type.Name);
            //return $"app.RegisterCrossBindingAdaptor(new {type.FullName + "Adaptor"}());\r\n";
        }

        private string CreateDelegateConvertorInit(Type type)
        {
            Print($"------delegate convertor Init:{type.Name}-----------");

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

        private string CreateDelegateRegisterInit(Type type)
        {
            Print($"------delegate reg Init:{type.Name}-----------");

            var method = type.GetMethod("Invoke");
            var tmpd = method.ReturnType == typeof(void) ? _actionRegisterTmpd : _functionRegisterTmpd;
            var argsType = "";
            var returnType = method.ReturnType == typeof(void) ? "" : method.ReturnType.Name;
            foreach (var param in method.GetParameters())
            {
                argsType += param.ParameterType.Name + ",";
            }
            argsType = argsType.Trim(',');
            tmpd = tmpd.Replace("{$argsType}", argsType);
            if (method.ReturnType != typeof(void))
                tmpd = tmpd.Replace("{$returnType}", returnType);

            return tmpd;
        }

        private string CreateDelegateRegisterInit(TypeReference type)
        {
            Print($"------delegate reg Init:{type.FullName}-----------");

            var tmpd = type.FullName.Contains("Action") ? _actionRegisterTmpd : _functionRegisterTmpd;
            var argsType = "";
            var gtype = (GenericInstanceType)type;
            foreach (var param in gtype.GenericArguments)
            {
                if (param != null)
                    argsType += param.FullName + ",";
            }
            argsType = argsType.Trim(',');
            tmpd = tmpd.Replace("{$argsType}", argsType);
            return tmpd;
        }

        private void CreateILRuntimeHelper()
        {
            //export helper
            var helperStr = _helperTmpd;

            var adptorStr = "";
            foreach (var type in _adaptorSet)
            {
                adptorStr += CreateAdaptorInit(type);
            }
            helperStr = helperStr.Replace("{$AdaptorInit}", adptorStr);

            var delegateStr = "";
            foreach (var type in _delegateSet)
            {
                delegateStr += CreateDelegateConvertorInit(type);
            }
            helperStr = helperStr.Replace("{$DelegateInit}", delegateStr);

            var delegateRegStr = "";
            foreach (var val in _delegateRegisterSet.Values)
            {
                if (val is Type)
                {
                    delegateRegStr += CreateDelegateRegisterInit(val as Type);
                }
                else if (val is TypeReference)
                {
                    delegateRegStr += CreateDelegateRegisterInit(val as TypeReference);
                }
            }
            helperStr = helperStr.Replace("{$DelegateRegInit}", delegateRegStr);


            using (var fs2 = File.Create(_outputPath + "helper.cs"))
            {
                var sw = new StreamWriter(fs2);
                sw.Write(helperStr);
                sw.Flush();
            }
        }

        private void CreateAdaptor(Type type)
        {
            if (type.IsInterface)
                return;

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
                    else if (methodInfo.IsVirtual && !methodInfo.IsFinal)
                    {
                        methodsbody += CreateVirtualMethod(methodInfo);
                    }
                }

                classbody = classbody.Replace("{$ClassName}", type.Name);
                classbody = classbody.Replace("{$MethodArea}", methodsbody);

                var interfaceStr = "";
                foreach (var iface in type.GetInterfaces())
                {
                    interfaceStr += CreateInterfaceAdaptor(iface, type);
                }

                classbody = classbody.Replace("{$Interface}", interfaceStr);

                var sw = new StreamWriter(fs);
                sw.Write(classbody);
                sw.Flush();
            }
        }

        private string CreateInterfaceAdaptor(Type type, Type childType)
        {
            var classbody = _adaptorInterfaceTmpd;
            var adaptorName = childType.Name + "Adaptor.Adaptor";

            classbody = classbody.Replace("{$ClassName}", type.Name);
            classbody = classbody.Replace("{$AdaptorName}", adaptorName);

            return classbody;
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

        private void LoadILAssemblyClick(object sender, EventArgs e)
        {
            var targetPath = sourcePath1.Text;

            if (targetPath == "")
            {
                if (OD.ShowDialog() != DialogResult.OK) return;

                Properties.Settings.Default["assembly_path"] = OD.FileName;
                Properties.Settings.Default.Save();
                sourcePath1.Text = targetPath = OD.FileName;
            }

            textBox1.Text = "";

            var assembly = Assembly.LoadFrom(targetPath);
            if (assembly == null) return;
            //types
            var types = assembly.GetTypes();
            _adptorAttr = assembly.GetType(AdaptorAttrName);
            _delegateAttr = assembly.GetType(DelegateAttrName);

            foreach (var type in types)
            {
                //load ad
                var attr = type.GetCustomAttribute(_adptorAttr, false);
                if (attr != null)
                {
                    Console.WriteLine("[adaptor]" + type.FullName);
                    LoadAdaptor(type);
                    continue;
                }

                //load delegate
                var attr1 = type.GetCustomAttribute(_delegateAttr, false);
                if (attr1 != null)
                {
                    Console.WriteLine("[delegate convertor]" + type.FullName);
                    LoadDelegateConvertor(type);
                    //Console.WriteLine("[delegate register]" + type.FullName);
                    //LoadDelegateRegister(type.FullName, type);
                }
            }

        }

        private void LoadUnityAssemblyClick(object sender, EventArgs e)
        {
            var targetPath = sourcePath2.Text;

            if (targetPath == "")
            {
                if (OD.ShowDialog() != DialogResult.OK) return;

                Properties.Settings.Default["assembly_path1"] = OD.FileName;
                Properties.Settings.Default.Save();
                sourcePath2.Text = targetPath = OD.FileName;
            }

            using (var testFs = File.Open(targetPath, FileMode.Open))
            {
                var module = ModuleDefinition.ReadModule(testFs);
                foreach (var typeDefinition in module.Types)
                {
                    foreach (var methodDefinition in typeDefinition.Methods)
                    {
                        foreach (var instruction in methodDefinition.Body.Instructions)
                        {
                            if (instruction.OpCode != OpCodes.Newobj || instruction.Previous == null ||
                                instruction.Previous.OpCode != OpCodes.Ldftn) continue;

                            var type = instruction.Operand as MethodReference;
                            if (type == null ||
                                (!type.DeclaringType.Name.Contains("Action") &&
                                 !type.DeclaringType.Name.Contains("Func"))) continue;

                            Console.WriteLine("[delegate register]" + type.DeclaringType.FullName);
                            LoadDelegateRegister(type.DeclaringType.FullName, type.DeclaringType);
                        }
                    }
                }
            }

        }


    }
}