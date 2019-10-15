using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CodeGenerationTools.Generator;
using ILRuntime.Other;
using ILRuntime.Mono.Cecil;
using ILRuntime.Mono.Cecil.Cil;


namespace CodeGenerationTools
{

    public partial class MainForm : Form
    {
        #region Fields
        private string _outputPath;

        //private readonly HashSet<Type> _adaptorSet = new HashSet<Type>();
        //private readonly HashSet<Type> _delegateSet = new HashSet<Type>();
        //private readonly Dictionary<string, object> _delegateRegDic = new Dictionary<string, object>();

        //private Type _adaptorAttr;
        //private Type _delegateAttr;

        private readonly Dictionary<string, TypeDefinition> _adaptorDic = new Dictionary<string, TypeDefinition>();
        private readonly Dictionary<string, TypeDefinition> _delegateCovDic = new Dictionary<string, TypeDefinition>();
        private readonly Dictionary<string, TypeReference> _delegateRegDic = new Dictionary<string, TypeReference>();

        private AdaptorGenerator _adGenerator;
        private HelperGenerator _helpGenerator;


        private string _adaptorAttrName = "ILRuntime.Other.NeedAdaptorAttribute";
        private string _delegateAttrName = "ILRuntime.Other.DelegateExportAttribute";

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

            outputText.Text = "";

            _adaptorDic.Clear();
            _delegateCovDic.Clear();
            _delegateRegDic.Clear();

            LoadTemplates();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Properties.Settings.Default["out_path"] = outputPath.Text;
            Properties.Settings.Default["assembly_path"] = sourcePath1.Text;
            Properties.Settings.Default["assembly_path1"] = sourcePath2.Text;
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

            try
            {
                var module = ModuleDefinition.ReadModule(targetPath);
                if (module == null) return;

                _adaptorDic.Clear();
                _delegateCovDic.Clear();

                var typeList = module.GetTypes();
                foreach (var t in typeList)
                {
                    foreach (var customAttribute in t.CustomAttributes)
                    {
                        if (customAttribute.AttributeType.FullName == _adaptorAttrName)
                        {
                            Print("[Need Adaptor]" + t.FullName);
                            LoadAdaptor(t);
                            continue;
                        }

                        if (customAttribute.AttributeType.FullName == _delegateAttrName)
                        {
                            //unity dll egg hurt name has '/'
                            var typeName = t.FullName.Replace("/", ".");
                            Print("[Delegate Export]" + typeName);
                            LoadDelegateConvertor(t);
                            continue;
                        }
                    }
                }


                //var assembly = Assembly.LoadFrom(targetPath);
                //if (assembly == null) return;

                ////如果自定义属性用自定义
                //_adaptorAttr = assembly.GetType("ILRuntime.Other.NeedAdaptorAttribute");
                //if (_adaptorAttr == null) _adaptorAttr = typeof(NeedAdaptorAttribute);

                //_delegateAttr = assembly.GetType("ILRuntime.Other.DelegateExportAttribute");
                //if (_delegateAttr == null) _delegateAttr = typeof(DelegateExportAttribute);

                ////types
                //Type[] types;
                //try
                //{
                //    types = assembly.GetTypes();
                //}
                //catch (ReflectionTypeLoadException ex)
                //{
                //    types = ex.Types;
                //}
                //catch (IOException ioex)
                //{
                //    Print(ioex.Message);
                //    types = new Type[0];
                //}

                //foreach (var type in types)
                //{
                //    if (type == null) continue;
                //    //load adaptor
                //    if (type == _adaptorAttr)
                //        continue;

                //    //var attr = type.GetCustomAttribute(typeof(NeedAdaptorAttribute), false);
                //    //if (attr.Length > 0)
                //    var attr = type.GetCustomAttribute(_adaptorAttr, false);
                //    if (attr != null)
                //    {
                //        Print("[adaptor]" + type.FullName);
                //        LoadAdaptor(type);
                //        continue;
                //    }
                //    if (type == _delegateAttr)
                //        continue;
                //    //load delegate
                //    //var attr1 = type.GetCustomAttributes(typeof(DelegateExportAttribute), false);
                //    //if (attr1.Length > 0)
                //    var attr1 = type.GetCustomAttribute(_delegateAttr, false);
                //    if (attr1 != null)
                //    {
                //        Print("[delegate convertor]" + type.FullName);
                //        LoadDelegateConvertor(type);
                //    }
                //}
                Print("----------main assembly loaded");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
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

            try
            {
                using (var fs = File.Open(targetPath, FileMode.Open))
                {
                    _delegateRegDic.Clear();
                    var module = ModuleDefinition.ReadModule(fs);
                    foreach (var typeDefinition in module.Types)
                    {
                        foreach (var methodDefinition in typeDefinition.Methods)
                        {
                            if (methodDefinition?.Body?.Instructions == null)
                                continue;
                            foreach (var instruction in methodDefinition.Body.Instructions)
                            {
                                if (instruction.OpCode != OpCodes.Newobj || instruction.Previous == null ||
                                    instruction.Previous.OpCode != OpCodes.Ldftn) continue;

                                var type = instruction.Operand as MethodReference;
                                if (type == null ||
                                    (!type.DeclaringType.Name.Contains("Action") &&
                                     !type.DeclaringType.Name.Contains("Func"))) continue;

                                var typeName = type.DeclaringType.FullName;//.Replace("/", ".");
                                Print("[delegate register]" + typeName);
                                LoadDelegateRegister(typeName, type.DeclaringType);
                            }
                        }
                    }
                }

                Print("----------ilscript assembly loaded");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }


        }

        private void GenerateClick(object sender, EventArgs e)
        {
            if (_adaptorDic.Count <= 0 && _delegateCovDic.Count <= 0 && _delegateRegDic.Count <= 0)
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

            foreach (var type in _adaptorDic.Values)
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

        //private void LoadDelegateRegister(string key, object type)
        //{
        //    if (!_delegateRegDic.ContainsKey(key))
        //        _delegateRegDic.Add(key, type);
        //}

        //private void LoadDelegateConvertor(Type type)
        //{
        //    if (!_delegateSet.Contains(type))
        //        _delegateSet.Add(type);
        //}

        //private void LoadAdaptor(Type type)
        //{
        //    if (!_adaptorSet.Contains(type))
        //        _adaptorSet.Add(type);
        //}


        //private void CreateILRuntimeHelper()
        //{
        //    Print($"==================Begin create helper:=====================");

        //    _helpGenerator.LoadData(new Tuple<HashSet<Type>, HashSet<Type>, Dictionary<string, object>>(_adaptorSet, _delegateSet, _delegateRegDic));
        //    var helperStr = _helpGenerator.Generate();

        //    using (var fs2 = File.Create(_outputPath + "helper.cs"))
        //    {
        //        var sw = new StreamWriter(fs2);
        //        sw.Write(helperStr);
        //        sw.Flush();
        //    }

        //    Print($"==============End create helper:===================");
        //}

        //private void CreateAdaptor(Type type)
        //{
        //    if (type.IsInterface)
        //        return;

        //    Print($"================begin create adaptor:{type.Name}=======================");

        //    var adaptorName = type.Name + "Adaptor";

        //    using (var fs = File.Create(_outputPath + adaptorName + ".cs"))
        //    {

        //        _adGenerator.LoadData(type);
        //        var classbody = _adGenerator.Generate();

        //        var sw = new StreamWriter(fs);
        //        sw.Write(classbody);
        //        sw.Flush();
        //    }

        //    Print($"================end create adaptor:{type.Name}=======================");

        //}

        private void LoadTemplates()
        {
            var tmpdPath = Application.StartupPath + "/Template/";

            _adGenerator = new AdaptorGenerator();
            _adGenerator.LoadTemplateFromFile(tmpdPath + "adaptor.tmpd");

            _helpGenerator = new HelperGenerator();
            _helpGenerator.LoadTemplateFromFile(tmpdPath + "helper.tmpd");

        }


        private void CreateILRuntimeHelper()
        {
            Print($"==================Begin create helper:=====================");

            _helpGenerator.LoadData(new Tuple<Dictionary<string, TypeDefinition>, Dictionary<string, TypeDefinition>, Dictionary<string, TypeReference>>(_adaptorDic, _delegateCovDic, _delegateRegDic));
            var helperStr = _helpGenerator.Generate();

            using (var fs2 = File.Create(_outputPath + "helper.cs"))
            {
                var sw = new StreamWriter(fs2);
                sw.Write(helperStr);
                sw.Flush();
            }

            Print($"==============End create helper:===================");
        }

        private void CreateAdaptor(TypeDefinition type)
        {
            if (type.IsInterface)
                return;


            Print($"================begin create adaptor:{type.Name}=======================");

            var adaptorName = type.Name + "Adaptor";

            using (var fs = File.Create(_outputPath + adaptorName + ".cs"))
            {

                _adGenerator.LoadData(type);
                var classbody = _adGenerator.Generate();

                var sw = new StreamWriter(fs);
                sw.Write(classbody);
                sw.Flush();
            }

            Print($"================end create adaptor:{type.Name}=======================");

        }


        private void LoadDelegateRegister(string key, TypeReference type)
        {
            if (!_delegateRegDic.ContainsKey(key))
                _delegateRegDic.Add(key, type);
            else
                _delegateRegDic[key] = type;
        }

        private void LoadDelegateConvertor(TypeDefinition type)
        {
            var key = type.FullName.Replace("/", ".");
            if (!_delegateCovDic.ContainsKey(key))
                _delegateCovDic.Add(key, type);
            else
                _delegateCovDic[type.FullName] = type;
        }

        private void LoadAdaptor(TypeDefinition type)
        {
            //var key = type.FullName.Replace("/", ".");
            if (!_adaptorDic.ContainsKey(type.FullName))
                _adaptorDic.Add(type.FullName, type);
            else
                _adaptorDic[type.FullName] = type;
        }

        #endregion

    }
}