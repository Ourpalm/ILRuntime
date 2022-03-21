using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ILRuntime.CLR.TypeSystem;
using ILRuntimeTest.Test;
using ILRuntimeTest.TestFramework;

namespace ILRuntimeTest.TestBase
{
    public class TestSession 
    {
        ILRuntime.Runtime.Enviorment.AppDomain _app;
        private List<BaseTestUnit> _testUnitList = new List<BaseTestUnit>();
        FileStream fs, fs2;

        public List<BaseTestUnit> TestList => _testUnitList;

        static TestSession lastSession;

        public static TestSession LastSession => lastSession;

        public ILRuntime.Runtime.Enviorment.AppDomain Appdomain => _app;

        public void Load(string assemblyPath, bool useRegister)
        {
            fs = new FileStream(assemblyPath, FileMode.Open, FileAccess.Read);
            {
                var path = Path.GetDirectoryName(assemblyPath);
                var name = Path.GetFileNameWithoutExtension(assemblyPath);
                var pdbPath = Path.Combine(path, name) + ".pdb";
                if (!File.Exists(pdbPath))
                {
                    name = Path.GetFileName(assemblyPath);
                    pdbPath = Path.Combine(path, name) + ".mdb";
                }

                _app = new ILRuntime.Runtime.Enviorment.AppDomain(useRegister ? ILRuntime.Runtime.ILRuntimeJITFlags.JITImmediately : ILRuntime.Runtime.ILRuntimeJITFlags.None);
                try
                {
                    _app.DebugService.StartDebugService(56000);
                }
                catch { }
                fs2 = new System.IO.FileStream(pdbPath, FileMode.Open);
                {
                    ILRuntime.Mono.Cecil.Cil.ISymbolReaderProvider symbolReaderProvider = null;
                    if (pdbPath.EndsWith(".pdb"))
                    {
                        symbolReaderProvider = new ILRuntime.Mono.Cecil.Pdb.PdbReaderProvider();
                    }/* else if (pdbPath.EndsWith (".mdb")) {
                            symbolReaderProvider = new Mono.Cecil.Mdb.MdbReaderProvider ();
                        }*/

                    _app.LoadAssembly(fs, fs2, symbolReaderProvider);
                }

                ILRuntimeHelper.Init(_app);
                ILRuntime.Runtime.Generated.CLRBindings.Initialize(_app);
                _app.InitializeBindings(true);
                LoadTest();
            }
            lastSession = this;
        }

        void LoadTest()
        {
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
        }

        public void Dispose()
        {
            fs?.Close();
            fs2?.Close();
            _app.Dispose();
            _app = null;
            _testUnitList.Clear();
        }
    }
}
