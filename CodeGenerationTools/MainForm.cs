using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using ILRuntime.Other;
using Mono.Cecil;
using Mono.Cecil.Cil;


namespace CodeGenerationTools
{

    public partial class MainForm : Form
    {
        #region Fields
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

        private readonly HashSet<Type> _adaptorSet = new HashSet<Type>();
        private readonly HashSet<Type> _delegateSet = new HashSet<Type>();
        private readonly Dictionary<string, object> _delegateRegDic = new Dictionary<string, object>();


        //private string _adaptorAttrName = "ILRuntimeTest.TestFramework.NeedAdaptorAttribute";
        //private string _delegateAttrName = "ILRuntimeTest.TestFramework.DelegateExportAttribute";

        //private Type _adptorAttr ;
        //private Type _delegateAttr;

        #endregion

        #region WinForm Event

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
            //adaptorTxt.Text = Properties.Settings.Default["adaptor_export_attr"] as string;
            //if (adaptorTxt.Text == "") adaptorTxt.Text = _adaptorAttrName;
            //delegateTxt.Text = Properties.Settings.Default["delegate_export_attr"] as string;
            //if (delegateTxt.Text == "") delegateTxt.Text = _delegateAttrName;
            outputText.Text = "";

            _adaptorSet.Clear();
            _delegateSet.Clear();
            _delegateRegDic.Clear();

            LoadTemplates();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Properties.Settings.Default["out_path"] = outputPath.Text;
            Properties.Settings.Default["assembly_path"] = sourcePath1.Text;
            Properties.Settings.Default["assembly_path1"] = sourcePath2.Text;
            //Properties.Settings.Default["adaptor_export_attr"] = adaptorTxt.Text;
            //Properties.Settings.Default["delegate_export_attr"] = delegateTxt.Text;

            Properties.Settings.Default.Save();
        }

        private void LoadMainProjectAssemblyClick(object sender, EventArgs e)
        {
            var targetPath = sourcePath1.Text;

            if (targetPath == "")
            {
                if (OD.ShowDialog() != DialogResult.OK) return;

                Properties.Settings.Default["assembly_path"] = OD.FileName;
                Properties.Settings.Default.Save();
                sourcePath1.Text = targetPath = OD.FileName;
            }

            var assembly = Assembly.LoadFrom(targetPath);
            if (assembly == null) return;
            //types
            var types = assembly.GetTypes();

            foreach (var type in types)
            {
                //load ad
                var attr = type.GetCustomAttribute(typeof(NeedAdaptorAttribute), false);
                if (attr != null)
                {
                    Print("[adaptor]" + type.FullName);
                    LoadAdaptor(type);
                    continue;
                }

                //load delegate
                var attr1 = type.GetCustomAttribute(typeof(DelegateExportAttribute), false);
                if (attr1 != null)
                {
                    Print("[delegate convertor]" + type.FullName);
                    LoadDelegateConvertor(type);
                }
            }

        }

        private void LoadILScriptAssemblyClick(object sender, EventArgs e)
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

                            Print("[delegate register]" + type.DeclaringType.FullName);
                            LoadDelegateRegister(type.DeclaringType.FullName, type.DeclaringType);
                        }
                    }
                }
            }

        }

        private void GenerateClick(object sender, EventArgs e)
        {
            if (_adaptorSet.Count <= 0 && _delegateSet.Count <= 0 && _delegateRegDic.Count <= 0)
            {
                Print("[Warnning] There is nothing to Generate");
                return;
            }

            Print("===============================Clear Old Files================================");
            var files = Directory.GetFiles(_outputPath);
            foreach (var file in files)
            {
                File.Delete(file);
            }

            Print("[=============================Generate Begin==============================]");

            foreach (var type in _adaptorSet)
            {
                CreateAdaptor(type);
            }

            CreateILRuntimeHelper();

            Print("[=============================Generate End=================================]");
        }

        private void CopyFileClick(object sender, EventArgs e)
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

        private void Print(string s)
        {
            outputText.Text += s + "\r\n";
            Console.WriteLine(s);
        }

        #endregion

        #region CodeGenerate Methods

        private void LoadDelegateRegister(string key, object type)
        {
            if (!_delegateRegDic.ContainsKey(key))
                _delegateRegDic.Add(key, type);
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
            Print($"==================Begin create helper:=====================");

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
            foreach (var val in _delegateRegDic.Values)
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

            Print($"==============End create helper:===================");
        }

        private void CreateAdaptor(Type type)
        {
            if (type.IsInterface)
                return;

            Print($"================begin create adaptor:{type.Name}=======================");

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

            Print($"================end create adaptor:{type.Name}=======================");

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

        #endregion

    }
}