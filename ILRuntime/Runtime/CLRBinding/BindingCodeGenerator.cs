using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;

namespace ILRuntime.Runtime.CLRBinding
{
    public class BindingCodeGenerator
    {
        public static void GenerateBindingCode(List<Type> types, string outputPath)
        {
            if (!System.IO.Directory.Exists(outputPath))
                System.IO.Directory.CreateDirectory(outputPath);
            foreach (var i in types)
            {
                string clsName, realClsName;
                bool isByRef;
                GetClassName(i, out clsName, out realClsName, out isByRef);
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(outputPath + "/" + clsName + ".cs", false, Encoding.UTF8))
                {
                    sw.Write(@"using System;
using System.Collections.Generic;
using System.Reflection;

using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
using ILRuntime.Reflection;

namespace ILRuntime.Runtime.Generated
{
    unsafe class ");
                    sw.WriteLine(clsName);
                    sw.Write(@"    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodInfo method;
            Type[] args;
            Type type = typeof(");
                    sw.Write(realClsName);
                    sw.WriteLine(");");
                    MethodInfo[] methods = i.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
                    string registerCode = GenerateRegisterCode(i, methods);
                    string wraperCode = GenerateWraperCode(i, methods);
                    sw.WriteLine(registerCode);
                    sw.WriteLine("        }");
                    sw.WriteLine();
                    sw.WriteLine(wraperCode);
                    sw.WriteLine("    }");
                    sw.WriteLine("}");
                    sw.Flush();
                }
            }
        }

        static string GenerateRegisterCode(Type type, MethodInfo[] methods)
        {
            StringBuilder sb = new StringBuilder();
            int idx = 0;
            foreach (var i in methods)
            {
                if (i.IsPrivate)
                    continue;
                if (i.IsGenericMethod)
                    continue;

                bool isProperty = i.IsSpecialName;
                var param = i.GetParameters();
                StringBuilder sb2 = new StringBuilder();
                sb2.Append("{");
                bool first = true;
                foreach(var j in param)
                {
                    if (first)
                        first = false;
                    else
                        sb2.Append(", ");
                    sb2.Append("typeof(");
                    string tmp, clsName;
                    bool isByRef;
                    GetClassName(j.ParameterType, out tmp, out clsName, out isByRef);
                    sb2.Append(clsName);                    
                    sb2.Append(")");
                    if (isByRef)
                        sb2.Append(".MakeByRefType()");
                }
                sb2.Append("}");
                sb.AppendLine(string.Format("            args = new Type[]{0};", sb2));
                sb.AppendLine(string.Format("            method = type.GetMethod(\"{0}\", flag, null, args, null);", i.Name));
                sb.AppendLine(string.Format("            app.RegisterCLRMethodRedirection(method, {0}_{1});", i.Name, idx));

                idx++;
            }
            return sb.ToString();
        }

        static string GenerateWraperCode(Type type, MethodInfo[] methods)
        {
            StringBuilder sb = new StringBuilder();

            int idx = 0;
            foreach (var i in methods)
            {
                if (i.IsPrivate)
                    continue;
                if (i.IsGenericMethod)
                    continue;

                bool isProperty = i.IsSpecialName;

                sb.AppendLine(string.Format("        static StackObject* {0}_{1}(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)", i.Name, idx));
                sb.AppendLine("        {");
                sb.AppendLine("        }");
                sb.AppendLine();
                idx++;
            }

            return sb.ToString();
        }

        static void GetClassName(Type type, out string clsName, out string realClsName, out bool isByRef)
        {
            isByRef = type.IsByRef;
            if (isByRef)
                type = type.GetElementType();
            clsName = type.Namespace.Replace(".", "_") + "_" + type.Name.Replace(".", "_").Replace("`", "_").Replace("<", "_").Replace(">", "_");
            bool isGeneric = false;
            string ga = null;
            if (type.IsGenericType)
            {
                isGeneric = true;
                clsName += "_";
                ga = "<";
                var args = type.GetGenericArguments();
                bool first = true;
                foreach (var j in args)
                {
                    if (first)
                        first = false;
                    else
                    {
                        clsName += "_";
                        ga += ", ";
                    }
                    clsName += j.Name;
                    string a, b;
                    bool tmp;
                    GetClassName(j, out a, out b, out tmp);
                    ga += b;
                }
                ga += ">";
            }

            realClsName = type.Namespace + ".";
            if (isGeneric)
            {
                int idx = type.Name.IndexOf("`");
                realClsName += type.Name.Substring(0, idx);
                realClsName += ga;
            }
            else
                realClsName += type.Name;
        }

    }
}
