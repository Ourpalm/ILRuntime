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
                    string wraperCode = GenerateWraperCode(i, methods, realClsName);
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
                //EventHandler is currently not supported
                if (i.IsSpecialName && (i.Name.StartsWith("add") || i.Name.StartsWith("remove")))
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

        static string GenerateWraperCode(Type type, MethodInfo[] methods, string typeClsName)
        {
            StringBuilder sb = new StringBuilder();

            int idx = 0;
            foreach (var i in methods)
            {
                if (i.IsPrivate)
                    continue;
                if (i.IsGenericMethod)
                    continue;
                //EventHandler is currently not supported
                if (i.IsSpecialName && (i.Name.StartsWith("add") || i.Name.StartsWith("remove")))
                    continue;
                bool isProperty = i.IsSpecialName;
                var param = i.GetParameters();
                int paramCnt = param.Length;
                if (!i.IsStatic)
                    paramCnt++;
                sb.AppendLine(string.Format("        static StackObject* {0}_{1}(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)", i.Name, idx));
                sb.AppendLine("        {");
                sb.AppendLine("            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;");
                sb.AppendLine("            StackObject* p;");
                sb.AppendLine(string.Format("            StackObject* ret = ILIntepreter.Minus(esp, {0});", paramCnt));
                for(int j = 1; j <= param.Length; j++)
                {
                    var p = param[j - 1];
                    sb.AppendLine(string.Format("            p = ILIntepreter.Minus(esp, {0});", j));
                    string tmp, clsName;
                    bool isByRef;
                    GetClassName(p.ParameterType, out tmp, out clsName, out isByRef);
                    if (isByRef)
                        sb.AppendLine("            p = ILIntepreter.GetObjectAndResolveReference(p);");
                    sb.AppendLine(string.Format("            {0} {1} = {2};", clsName, p.Name, GetRetrieveValueCode(p.ParameterType, clsName)));
                    if (!isByRef && !p.ParameterType.IsPrimitive)
                        sb.AppendLine("            intp.Free(p);");
                }
                if (!i.IsStatic)
                {
                    sb.AppendLine(string.Format("            p = ILIntepreter.Minus(esp, {0});", paramCnt));
                    string tmp, clsName;
                    bool isByRef;
                    GetClassName(type, out tmp, out clsName, out isByRef);
                    if (isByRef)
                        sb.AppendLine("            p = ILIntepreter.GetObjectAndResolveReference(p);");
                    sb.AppendLine(string.Format("            {0} instance = {1};", clsName, GetRetrieveValueCode(type, clsName)));
                    if (!isByRef && !type.IsPrimitive)
                        sb.AppendLine("            intp.Free(p);");
                }
                sb.AppendLine();
                if (i.ReturnType != typeof(void))
                {
                    sb.Append("            var result_of_this_method = ");
                }
                else
                    sb.Append("            ");
                if (i.IsStatic)
                {
                    if (isProperty)
                    {
                        string[] t = i.Name.Split('_');
                        string propType = t[0];

                        if (propType == "get")
                        {
                            bool isIndexer = param.Length > 0;
                            if (isIndexer)
                            {
                                sb.AppendLine(string.Format("{1}[{0}];", param[0].Name, typeClsName));
                            }
                            else
                                sb.AppendLine(string.Format("{1}.{0};", t[1], typeClsName));
                        }
                        else if (propType == "set")
                        {
                            bool isIndexer = param.Length > 1;
                            if (isIndexer)
                            {
                                sb.AppendLine(string.Format("{2}[{0}] = {1};", param[0].Name, param[1].Name, typeClsName));
                            }
                            else
                                sb.AppendLine(string.Format("{2}.{0} = {1};", t[1], param[0].Name, typeClsName));
                        }
                        else
                            throw new NotImplementedException();
                    }
                    else
                    {
                        sb.Append(string.Format("{0}.{1}(", typeClsName, i.Name));
                        AppendParameters(param, sb);
                        sb.AppendLine(");");
                    }
                }
                else
                {
                    if (isProperty)
                    {
                        string[] t = i.Name.Split('_');
                        string propType = t[0];
                        
                        if (propType == "get")
                        {
                            bool isIndexer = param.Length > 0;
                            if (isIndexer)
                            {
                                sb.AppendLine(string.Format("instance[{0}];", param[0].Name));
                            }
                            else
                                sb.AppendLine(string.Format("instance.{0};", t[1]));
                        }
                        else if (propType == "set")
                        {
                            bool isIndexer = param.Length > 1;
                            if (isIndexer)
                            {
                                sb.AppendLine(string.Format("instance[{0}] = {1};", param[0].Name, param[1].Name));
                            }
                            else
                                sb.AppendLine(string.Format("instance.{0} = {1};", t[1], param[0].Name));
                        }
                        else
                            throw new NotImplementedException();
                    }
                    else
                    {
                        sb.Append(string.Format("instance.{0}(", i.Name));
                        AppendParameters(param, sb);
                        sb.AppendLine(");");
                    }
                }
                if (i.ReturnType != typeof(void))
                {
                    GetReturnValueCode(i.ReturnType, sb);
                }
                else
                    sb.AppendLine("            return ret;");
                sb.AppendLine("        }");
                sb.AppendLine();
                idx++;
            }

            return sb.ToString();
        }

        static void AppendParameters(ParameterInfo[] param, StringBuilder sb)
        {
            bool first = true;
            foreach (var j in param)
            {
                if (first)
                    first = false;
                else
                    sb.Append(", ");
                if (j.IsOut)
                    sb.Append("out ");
                else if (j.ParameterType.IsByRef)
                    sb.Append("ref ");
                sb.Append(j.Name);
            }
        }

        static void GetReturnValueCode(Type type, StringBuilder sb)
        {
            if (type.IsPrimitive)
            {
                if (type == typeof(int))
                {
                    sb.AppendLine("            ret->ObjectType = ObjectTypes.Integer;");
                    sb.AppendLine("            ret->Value = result_of_this_method;");
                }
                else if (type == typeof(long))
                {
                    sb.AppendLine("            ret->ObjectType = ObjectTypes.Long;");
                    sb.AppendLine("            *(long*)&ret->Value = result_of_this_method;");
                }
                else if (type == typeof(short))
                {
                    sb.AppendLine("            ret->ObjectType = ObjectTypes.Integer;");
                    sb.AppendLine("            ret->Value = result_of_this_method;");
                }
                else if (type == typeof(bool))
                {
                    sb.AppendLine("            ret->ObjectType = ObjectTypes.Integer;");
                    sb.AppendLine("            ret->Value = result_of_this_method ? 1 : 0;");
                }
                else if (type == typeof(ushort))
                {
                    sb.AppendLine("            ret->ObjectType = ObjectTypes.Integer;");
                    sb.AppendLine("            ret->Value = result_of_this_method;");
                }
                else if (type == typeof(float))
                {
                    sb.AppendLine("            ret->ObjectType = ObjectTypes.Float;");
                    sb.AppendLine("            *(float*)&ret->Value = result_of_this_method;");
                }
                else if (type == typeof(double))
                {
                    sb.AppendLine("            ret->ObjectType = ObjectTypes.Double;");
                    sb.AppendLine("            *(double*)&ret->Value = result_of_this_method;");
                }
                else if (type == typeof(byte))
                {
                    sb.AppendLine("            ret->ObjectType = ObjectTypes.Integer;");
                    sb.AppendLine("            (byte)ret->Value = result_of_this_method;");
                }
                else if (type == typeof(sbyte))
                {
                    sb.AppendLine("            ret->ObjectType = ObjectTypes.Integer;");
                    sb.AppendLine("            (sbyte)ret->Value = result_of_this_method;");
                }
                else if (type == typeof(uint))
                {
                    sb.AppendLine("            ret->ObjectType = ObjectTypes.Integer;");
                    sb.AppendLine("            ret->Value = (int)result_of_this_method;");
                }
                else if (type == typeof(char))
                {
                    sb.AppendLine("            ret->ObjectType = ObjectTypes.Integer;");
                    sb.AppendLine("            ret->Value = (int)result_of_this_method;");
                }
                else if (type == typeof(ulong))
                {
                    sb.AppendLine("            ret->ObjectType = ObjectTypes.Long;");
                    sb.AppendLine("            *(ulong*)&ret->Value = result_of_this_method;");
                }
                else
                    throw new NotImplementedException();
                sb.AppendLine("            return ret + 1;");

            }
            else
            {
                sb.AppendLine("            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);");
            }
        }

        static string GetRetrieveValueCode(Type type, string realClsName)
        {
            if (type.IsByRef)
                type = type.GetElementType();
            if (type.IsPrimitive)
            {
                if (type == typeof(int))
                {
                    return "p->Value";
                }
                else if (type == typeof(long))
                {
                    return "*(long*)&p->Value";
                }
                else if (type == typeof(short))
                {
                    return "(short)p->Value";
                }
                else if (type == typeof(bool))
                {
                    return "p->Value == 1";
                }
                else if (type == typeof(ushort))
                {
                    return "(ushort)p->Value";
                }
                else if (type == typeof(float))
                {
                    return "*(float*)&p->Value";
                }
                else if (type == typeof(double))
                {
                    return "*(double*)&p->Value";
                }
                else if (type == typeof(byte))
                {
                    return "(byte)p->Value";
                }
                else if (type == typeof(sbyte))
                {
                    return "(sbyte)p->Value";
                }
                else if (type == typeof(uint))
                {
                    return "(uint)p->Value";
                }
                else if (type == typeof(char))
                {
                    return "(char)p->Value";
                }
                else if (type == typeof(ulong))
                {
                    return "*(ulong*)&p->Value";
                }
                else
                    throw new NotImplementedException();
            }
            else
            {
                return string.Format("({0})StackObject.ToObject(p, domain, mStack)", realClsName);
            }
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
