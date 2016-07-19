using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ILRuntimeTest.TestBase;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;

namespace ILRuntimeTest.Test
{
    abstract class BaseTestUnit : ITestable
    {
        protected ILRuntime.Runtime.Enviorment.AppDomain _app;
        protected string _assemblyName;
        protected string _typeName;
        protected string _methodName;
        protected bool _pass;
        protected StringBuilder message = new StringBuilder();

        #region 接口方法

        public bool Init(string fileName)
        {
            _assemblyName = fileName;
            if (!File.Exists(_assemblyName))
                return false;
            using (var fs = new System.IO.FileStream(_assemblyName, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                _app = new ILRuntime.Runtime.Enviorment.AppDomain();
                var path = System.IO.Path.GetDirectoryName(_assemblyName);
                var name = System.IO.Path.GetFileNameWithoutExtension(_assemblyName);
                using (var fs2 = new System.IO.FileStream(string.Format("{0}\\{1}.pdb", path, name), System.IO.FileMode.Open))
                    _app.LoadAssembly(fs, fs2, new Mono.Cecil.Pdb.PdbReaderProvider());
            }

            return true;
        }

        public bool Init(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            if (app == null)
                return false;

            _app = app;
            return true;
        }

        public bool Init(ILRuntime.Runtime.Enviorment.AppDomain app, string type, string method)
        {
            if (app == null)
                return false;

            _typeName = type;
            _methodName = method;

            _app = app;
            return true;
        }

        //需要子类去实现
        public abstract void Run();

        public abstract bool Check();

        public abstract TestResultInfo CheckResult();

        #endregion

        #region 常用工具方法

        public Object getInstance(string type)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// call Method with no params and no return value;
        /// </summary>
        /// <param name="Instance">Instacne, if it is null means static method,else means instance method</param>
        /// <param name="type">TypeName ,eg "Namespace.ClassType"</param>
        /// <param name="method">MethodName</param>
        public void Invoke(Object Instance, string type, string method)
        {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            _app.Invoke(type, method); //InstanceTest
            sw.Stop();
            message.AppendLine("Elappsed Time:" + sw.ElapsedMilliseconds + "ms\n");
        }

        public void Invoke(string type, string method)
        {
            try
            {
                var sw = new System.Diagnostics.Stopwatch();
                sw.Start();
                var res = _app.Invoke(type, method); //InstanceTest
                sw.Stop();
                if (res != null)
                    message.AppendLine("Return:" + res);
                message.AppendLine("Elappsed Time:" + sw.ElapsedMilliseconds + "ms\n");
                _pass = true;
            }
            catch (ILRuntime.Runtime.Intepreter.ILRuntimeException e)
            {
                message.AppendLine(e.Message);
                if (!string.IsNullOrEmpty(e.ThisInfo))
                {
                    message.AppendLine("this:");
                    message.AppendLine(e.ThisInfo);
                }
                message.AppendLine("Local Variables:");
                message.AppendLine(e.LocalInfo);
                message.AppendLine(e.StackTrace);
                _pass = false;
            }
        }

        ////无返回值
        //void Invoke<T>(Object Instance, string type, string method, T param)
        //{
        //    throw new NotImplementedException();
        //}

        //void Invoke<T1, T2>(Object Instance, string type, string method, T1 param1, T2 param2)
        //{
        //    throw new NotImplementedException();
        //}

        //void Invoke<T1, T2, T3>(Object Instance, string type, string method, T1 param1, T2 param2, T3 param3)
        //{
        //    throw new NotImplementedException();
        //}

        //void Invoke<T1, T2, T3, T4>(Object Instance, string type, string method, T1 param1, T2 param2, T3 param3, T4 param4)
        //{
        //    throw new NotImplementedException();
        //}

        //void Invoke<T1, T2, T3, T4, T5>(Object Instance, string type, string method, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5)
        //{
        //    throw new NotImplementedException();
        //}

        ////有返回值
        //T Invoke<T>(Object Instance, string type, string method)
        //{
        //    throw new NotImplementedException();
        //}

        //T Invoke<T, T1, T2>(Object Instance, string type, string method, T1 param1, T2 param2)
        //{
        //    throw new NotImplementedException();
        //}

        //T Invoke<T, T1, T2, T3>(Object Instance, string type, string method, T1 param1, T2 param2, T3 param3)
        //{
        //    throw new NotImplementedException();
        //}

        //T Invoke<T, T1, T2, T3, T4>(Object Instance, string type, string method, T1 param1, T2 param2, T3 param3, T4 param4)
        //{
        //    throw new NotImplementedException();
        //}

        //T Invoke<T, T1, T2, T3, T4, T5>(Object Instance, string type, string method, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5)
        //{
        //    throw new NotImplementedException();
        //}

        //protected bool AssertToBe<T>(T assertValue, T result)
        //{
        //    throw new NotImplementedException();
        //}

        //protected bool AssertNotToBe<T>(T errorValue, T result)
        //{
        //    throw new NotImplementedException();
        //}

        //protected void Assert(bool expression, string errorLog)
        //{
        //    throw new NotImplementedException();
        //}

        //protected void Assert(bool expression, Action<bool> resAction)
        //{
        //    throw new NotImplementedException();
        //}

        #endregion

    }
}
