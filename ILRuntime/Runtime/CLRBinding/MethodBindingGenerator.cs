using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.CLR.Utils;
using System.Runtime.CompilerServices;

namespace ILRuntime.Runtime.CLRBinding
{
    static class MethodBindingGenerator
    {
        static string GenerateParameterTypes(MethodInfo i, bool needBracket)
        {
            string clsName, realClsName;
            bool isByRef;
            var param = i.GetParameters();
            StringBuilder sb2 = new StringBuilder();
            if (needBracket)
                sb2.Append("{");
            bool first = true;
            foreach (var j in param)
            {
                if (first)
                    first = false;
                else
                    sb2.Append(", ");
                sb2.Append("typeof(");
                j.ParameterType.GetClassName(out clsName, out realClsName, out isByRef);
                sb2.Append(realClsName);
                sb2.Append(")");
                if (isByRef)
                    sb2.Append(".MakeByRefType()");
            }
            if (needBracket)
                sb2.Append("}");
            return sb2.ToString();
        }
        internal static string GenerateMethodRegisterCode(this Type type, MethodInfo[] methods, HashSet<MethodBase> excludes, out bool needMethods)
        {
            needMethods = false;
            MethodInfo[] allMethods = type.GetMethods();

            StringBuilder sb = new StringBuilder();
            int idx = 0;
            bool isMethodsGot = false;
            foreach (var i in methods)
            {
                if (excludes != null && excludes.Contains(i))
                    continue;
                if (type.ShouldSkipMethod(i))
                    continue;

                if (i.IsGenericMethod)
                {
                    if (!isMethodsGot)
                    {
                        sb.AppendLine(@"            Dictionary<string, List<MethodInfo>> genericMethods = new Dictionary<string, List<MethodInfo>>();
            List<MethodInfo> lst = null;                    
            foreach(var m in type.GetMethods())
            {
                if(m.IsGenericMethodDefinition)
                {
                    if (!genericMethods.TryGetValue(m.Name, out lst))
                    {
                        lst = new List<MethodInfo>();
                        genericMethods[m.Name] = lst;
                    }
                    lst.Add(m);
                }
            }");
                        isMethodsGot = true;
                    }

                    var param = i.GetGenericArguments();
                    StringBuilder sb2 = new StringBuilder();
                    sb2.Append("{");
                    bool first = true;
                    string clsName, realClsName;
                    bool isByRef;
                    foreach (var j in param)
                    {
                        if (first)
                            first = false;
                        else
                            sb2.Append(", ");
                        sb2.Append("typeof(");
                        j.GetClassName(out clsName, out realClsName, out isByRef);
                        sb2.Append(realClsName);
                        sb2.Append(")");
                        if (isByRef)
                            sb2.Append(".MakeByRefType()");
                    }
                    sb2.Append("}");
                    sb.AppendLine(string.Format("            args = new Type[]{0};", sb2));
                    sb.AppendLine(string.Format("            if (genericMethods.TryGetValue(\"{0}\", out lst))", i.Name));
                    sb.Append(@"            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, ");
                    if (i.ReturnType != typeof(void))
                    {
                        sb.Append("typeof(");
                        i.ReturnType.GetClassName(out clsName, out realClsName, out isByRef);
                        sb.Append(realClsName);
                        sb.Append(")");
                    }
                    else
                        sb.Append("typeof(void)");
                    if (i.GetParameters().Length > 0)
                    {
                        sb.Append(", ");
                        sb.Append(GenerateParameterTypes(i, false));
                    }
                    sb.Append(@"))
                    {
                        method = m.MakeGenericMethod(args);
");
                    sb.AppendLine("#if ENABLE_NEO_MODE");
                    sb.Append("                        app.RegisterCLRMethodRedirectionNeo(method, ");
                    sb.AppendLine(string.Format("{0}_{1}_Neo);", i.Name, idx));
                    sb.AppendLine("#else");
                    sb.Append("                        app.RegisterCLRMethodRedirection(method, ");
                    sb.AppendLine(string.Format("{0}_{1});", i.Name, idx));
                    sb.AppendLine("#endif");
                    sb.AppendLine(@"
                        break;
                    }
                }
            }");
                }
                else
                {
                    string clsName, realClsName;
                    bool isByRef;
                    var param = i.GetParameters();
                    string sb2 = GenerateParameterTypes(i, true);
                    sb.AppendLine(string.Format("            args = new Type[]{0};", sb2));

                    i.ReturnType.GetClassName(out clsName, out realClsName, out isByRef);
                    if ((i.Name.Equals("op_Implicit") || i.Name.Equals("op_Explicit")) && allMethods.Count(m => m.Name.Equals(i.Name)) > 1)
                    {
                        // Type conversions can have different return types
                        needMethods = true;
                        sb.AppendLine(string.Format("            method = methods.Where(t => t.Name.Equals(\"{0}\") && t.ReturnType == typeof({1}) && t.CheckMethodParams(args)).Single();", i.Name, realClsName));
                    }
                    else if (allMethods.Any(m => m.IsGenericMethod && m.Name.Equals(i.Name) && m.CheckMethodParams(param)))
                    {
                        // Check for a generic method with the same name and params
                        needMethods = true;
                        sb.AppendLine(string.Format("            method = methods.Where(t => t.Name.Equals(\"{0}\") && t.CheckMethodParams(args)).Single();", i.Name));
                    }
                    else
                    {
                        if (i.DeclaringType != type)
                        {
                            sb.AppendLine(string.Format("            method = typeof({1}).GetMethod(\"{0}\", flag, null, args, null);", i.Name, i.DeclaringType.FullName));
                        }
                        else
                            sb.AppendLine(string.Format("            method = type.GetMethod(\"{0}\", flag, null, args, null);", i.Name));
                    }

                    sb.AppendLine("#if ENABLE_NEO_MODE");
                    sb.AppendLine(string.Format("            app.RegisterCLRMethodRedirectionNeo(method, {0}_{1}_Neo);", i.Name, idx));
                    sb.AppendLine("#else");
                    sb.AppendLine(string.Format("            app.RegisterCLRMethodRedirection(method, {0}_{1});", i.Name, idx));
                    sb.AppendLine("#endif");
                }

                idx++;
            }
            return sb.ToString();
        }

        static void GenerateOperatorCode(StringBuilder sb, string propName, ParameterInfo[] param, Type returnType, string typeClsName, bool isLegacy)
        {
            string p0 = isLegacy ? param[0].Name : "@" + param[0].Name;
            string p1 = param.Length > 1 ? (isLegacy ? param[1].Name : "@" + param[1].Name) : "";

            switch (propName)
            {
                case "Equality":
                    sb.AppendLine(string.Format("{0} == {1};", p0, p1));
                    break;
                case "Inequality":
                    sb.AppendLine(string.Format("{0} != {1};", p0, p1));
                    break;
                case "Addition":
                    sb.AppendLine(string.Format("{0} + {1};", p0, p1));
                    break;
                case "Subtraction":
                    sb.AppendLine(string.Format("{0} - {1};", p0, p1));
                    break;
                case "Multiply":
                    sb.AppendLine(string.Format("{0} * {1};", p0, p1));
                    break;
                case "Division":
                    sb.AppendLine(string.Format("{0} / {1};", p0, p1));
                    break;
                case "GreaterThan":
                    sb.AppendLine(string.Format("{0} > {1};", p0, p1));
                    break;
                case "GreaterThanOrEqual":
                    sb.AppendLine(string.Format("{0} >= {1};", p0, p1));
                    break;
                case "LessThan":
                    sb.AppendLine(string.Format("{0} < {1};", p0, p1));
                    break;
                case "LessThanOrEqual":
                    sb.AppendLine(string.Format("{0} <= {1};", p0, p1));
                    break;
                case "UnaryNegation":
                    sb.AppendLine(string.Format("-{0};", p0));
                    break;
                case "LogicalNot":
                    sb.AppendLine(string.Format("!{0};", p0));
                    break;
                case "Modulus":
                    sb.AppendLine(string.Format("{0} % {1};", p0, p1));
                    break;
                case "LeftShift":
                    sb.AppendLine(string.Format("{0} << {1};", p0, p1));
                    break;
                case "RightShift":
                    sb.AppendLine(string.Format("{0} >> {1};", p0, p1));
                    break;
                case "Implicit":
                case "Explicit":
                    {
                        string clsName, realClsName;
                        bool isByRef;
                        returnType.GetClassName(out clsName, out realClsName, out isByRef);
                        sb.AppendLine(string.Format("({1}){0};", p0, realClsName));
                    }
                    break;
                case "Increment":
                    sb.AppendLine(string.Format("++{0};", p0));
                    break;
                case "Decrement":
                    sb.AppendLine(string.Format("--{0};", p0));
                    break;
                default:
                    sb.AppendLine(string.Format("// TODO: NotImplementedException {0}", propName));
                    break;
            }
        }

        static void GenerateMethodWraperCode_Neo(StringBuilder sb, Type type, MethodInfo i, int idx, string typeClsName, List<Type> valueTypeBinders, bool isMultiArr, bool isProperty, ParameterInfo[] param, string genericArguments)
        {
            sb.AppendLine(string.Format("        static void {0}_{1}_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)", i.Name, idx));
            sb.AppendLine("        {");
            sb.AppendLine("            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;");
            sb.AppendLine("            int __curPrim = 0;");
            
            if (!i.IsStatic)
            {
                if (type.IsValueType)
                {
                    sb.AppendLine(string.Format("            {0} instance_of_this_method = default({0});", typeClsName));
                    sb.AppendLine("            // TODO: ValueType instance in Neo");
                }
                else
                {
                    sb.AppendLine(string.Format("            {0} instance_of_this_method = ({0})ILIntepreter.ReadNeoReference(__frameBase, ref __curPrim, __mStack);", typeClsName));
                }
            }

            for (int j = 0; j < param.Length; j++)
            {
                var p = param[j];
                p.ParameterType.AppendArgumentCodeNeo(sb, j + 1, p.Name, valueTypeBinders, isMultiArr);
            }

            if (isMultiArr)
            {
                if (i.Name == "Get")
                {
                    sb.Append(string.Format("            var result_of_this_method = instance_of_this_method["));
                    for (int j = 0; j < param.Length; j++) 
                    { 
                        if (j > 0) sb.Append(", ");
                        sb.Append("a" + (j + 1)); 
                    }
                    sb.AppendLine("];");
                }
                else if (i.Name == "Set")
                {
                    sb.Append(string.Format("            instance_of_this_method["));
                    for (int j = 0; j < param.Length - 1; j++) 
                    { 
                        if (j > 0) sb.Append(", ");
                        sb.Append("a" + (j + 1)); 
                    }
                    sb.AppendLine(string.Format("] = a{0};", param.Length));
                }
                else if (i.Name == "Address")
                {
                    sb.Append(string.Format("            var result_of_this_method = instance_of_this_method.Address("));
                    for (int j = 0; j < param.Length; j++) 
                    { 
                        if (j > 0) sb.Append(", ");
                        sb.Append("a" + (j + 1)); 
                    }
                    sb.AppendLine(");");
                }
                else
                {
                    sb.Append(string.Format("            instance_of_this_method.{0}(", i.Name));
                    for (int j = 0; j < param.Length; j++) 
                    { 
                        if (j > 0) sb.Append(", ");
                        sb.Append("a" + (j + 1)); 
                    }
                    sb.AppendLine(");");
                }
            }
            else
            {
                if (i.ReturnType != typeof(void))
                {
                    sb.Append("            var result_of_this_method = ");
                }
                else
                {
                    sb.Append("            ");
                }

                if (i.IsStatic)
                {
                    if (isProperty)
                    {
                        string propType = i.Name.Substring(0, i.Name.IndexOf("_"));
                        string propName = i.Name.Substring(i.Name.IndexOf("_") + 1);
                        if (propType == "get") sb.AppendLine(string.Format("{0}.{1};", typeClsName, propName));
                        else if (propType == "set") sb.AppendLine(string.Format("{0}.{1} = {2};", typeClsName, propName, param[0].Name));
                        else if (propType == "add") sb.AppendLine(string.Format("{0}.{1} += {2};", typeClsName, propName, param[0].Name));
                        else if (propType == "remove") sb.AppendLine(string.Format("{0}.{1} -= {2};", typeClsName, propName, param[0].Name));
                        else if (propType == "op") GenerateOperatorCode(sb, propName, param, i.ReturnType, typeClsName, false);
                        else sb.AppendLine(string.Format("// TODO: NotImplementedException {0}", i.Name));
                    }
                    else
                    {
                        sb.Append(string.Format("{0}.{1}{2}(", typeClsName, i.Name, genericArguments));
                        for (int j = 0; j < param.Length; j++) 
                        { 
                            if (j > 0) sb.Append(", ");
                            if (param[j].IsOut && param[j].ParameterType.IsByRef) sb.Append("out ");
                            else if (param[j].IsIn && param[j].ParameterType.IsByRef) sb.Append("in ");
                            else if (param[j].ParameterType.IsByRef) sb.Append("ref ");
                            sb.Append("@" + param[j].Name); 
                        }
                        sb.AppendLine(");");
                    }
                }
                else
                {
                    if (isProperty)
                    {
                        string propType = i.Name.Substring(0, i.Name.IndexOf("_"));
                        string propName = i.Name.Substring(i.Name.IndexOf("_") + 1);
                        if (propType == "get") 
                        {
                            if (param.Length > 0) sb.AppendLine(string.Format("instance_of_this_method[{0}];", param[0].Name));
                            else sb.AppendLine(string.Format("instance_of_this_method.{0};", propName));
                        }
                        else if (propType == "set")
                        {
                            if (param.Length > 1) sb.AppendLine(string.Format("instance_of_this_method[{0}] = {1};", param[0].Name, param[1].Name));
                            else sb.AppendLine(string.Format("instance_of_this_method.{0} = {1};", propName, param[0].Name));
                        }
                        else if (propType == "add") sb.AppendLine(string.Format("instance_of_this_method.{0} += {1};", propName, param[0].Name));
                        else if (propType == "remove") sb.AppendLine(string.Format("instance_of_this_method.{0} -= {1};", propName, param[0].Name));
                        else sb.AppendLine("// TODO: NotImplementedException");
                    }
                    else
                    {
                        sb.Append(string.Format("instance_of_this_method.{0}{1}(", i.Name, genericArguments));
                        for (int j = 0; j < param.Length; j++) 
                        { 
                            if (j > 0) sb.Append(", ");
                            if (param[j].IsOut && param[j].ParameterType.IsByRef) sb.Append("out ");
                            else if (param[j].IsIn && param[j].ParameterType.IsByRef) sb.Append("in ");
                            else if (param[j].ParameterType.IsByRef) sb.Append("ref ");
                            sb.Append("@" + param[j].Name); 
                        }
                        sb.AppendLine(");");
                    }
                }
            }

            if (i.ReturnType != typeof(void))
            {
                i.ReturnType.GetReturnValueCodeNeo(sb);
            }

            sb.AppendLine("        }");
        }

        static void GenerateMethodWraperCode_Legacy(StringBuilder sb, Type type, MethodInfo i, int idx, string typeClsName, List<Type> valueTypeBinders, bool isMultiArr, bool isProperty, ParameterInfo[] param, int paramCnt, Enviorment.AppDomain domain, string forceInstanceTypeName, string genericArguments)
        {
            sb.AppendLine(string.Format("        static StackObject* {0}_{1}(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)", i.Name, idx));
            sb.AppendLine("        {");
            sb.AppendLine("            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;");
            if (param.Length != 0 || !i.IsStatic)
                sb.AppendLine("            StackObject* ptr_of_this_method;");
            sb.AppendLine(string.Format("            StackObject* __ret = __esp - {0};", paramCnt));
            sb.AppendLine();
            bool hasByRef = param.HasByRefParam();
            string shouldFreeParam = hasByRef ? "false" : "true";
            for (int j = param.Length; j > 0; j--)
            {
                var p = param[j - 1];
                sb.AppendLine(string.Format("            ptr_of_this_method = __esp - {0};", param.Length - j + 1));
                p.ParameterType.AppendArgumentCode(sb, j, p.Name, valueTypeBinders, isMultiArr, hasByRef, true);
                sb.AppendLine();
            }
            bool noUnbox = false;
            if(i.Name == "Start" && i.IsGenericMethod)
            {
                var ga = i.GetGenericArguments();
                if (typeof(IAsyncStateMachine).IsAssignableFrom(ga[0]))
                {
                    noUnbox = true;
                }
            }
            if (!i.IsStatic)
            {
                sb.AppendLine(string.Format("            ptr_of_this_method = __esp - {0};", paramCnt));
                if (type.IsPrimitive)
                    sb.AppendLine(string.Format("            {0} instance_of_this_method = GetInstance(__domain, ptr_of_this_method, __mStack);", typeClsName));
                else if (type.IsValueType && !type.IsPrimitive && valueTypeBinders != null && valueTypeBinders.Contains(type))
                {
                    string clsName, realClsName;
                    bool isByRef;
                    type.GetClassName(out clsName, out realClsName, out isByRef);

                    sb.AppendLine(string.Format("            {0} instance_of_this_method = new {0}();", realClsName));

                    sb.AppendLine(string.Format("            if (ILRuntime.Runtime.Generated.CLRBindings.s_{0}_Binder != null) {{", clsName));

                    sb.AppendLine(string.Format("                ILRuntime.Runtime.Generated.CLRBindings.s_{0}_Binder.ParseValue(ref instance_of_this_method, __intp, ptr_of_this_method, __mStack, false);", clsName));

                    sb.AppendLine("            } else {");

                    if (type.IsValueType)
                        sb.AppendLine("                ptr_of_this_method = ILIntepreter.GetObjectAndResolveReference(ptr_of_this_method);");
                    sb.AppendLine(string.Format("                instance_of_this_method = {0};", type.GetRetrieveValueCode(typeClsName)));
                    if (!type.IsValueType && !hasByRef)
                        sb.AppendLine("                __intp.Free(ptr_of_this_method);");

                    sb.AppendLine("            }");
                }
                else
                {
                    if (type.IsValueType && !type.IsPrimitive)
                    {
                        sb.AppendLine("            ptr_of_this_method = ILIntepreter.GetObjectAndResolveReference(ptr_of_this_method);");                            
                    }
                    if (noUnbox)
                        sb.AppendLine(string.Format("            object instance_of_this_method = {0};", type.GetRetrieveValueCode(typeClsName)));
                    else
                    {
                        string useTypeName = forceInstanceTypeName != null ? forceInstanceTypeName : typeClsName;
                        sb.AppendLine(string.Format("            {0} instance_of_this_method = {1};", useTypeName, type.GetRetrieveValueCode(useTypeName)));
                    }
                    if (!type.IsValueType && !hasByRef)
                        sb.AppendLine("            __intp.Free(ptr_of_this_method);");
                }
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
                    string[] t = new string[2];
                    int firstUnderlineIndex = i.Name.IndexOf("_");
                    t[0] = i.Name.Substring(0, firstUnderlineIndex);
                    t[1] = i.Name.Substring(firstUnderlineIndex + 1);
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
                    else if (propType == "op") GenerateOperatorCode(sb, t[1], param, i.ReturnType, typeClsName, true);
                    else if(propType == "add")
                    {
                        string clsName, realClsName;
                        bool isByRef;
                        i.DeclaringType.GetClassName(out clsName, out realClsName, out isByRef);
                        sb.AppendLine(string.Format("{0}.{1} += {2};", realClsName, i.Name.Substring(4), param[0].Name));
                    }
                    else if (propType == "remove")
                    {
                        string clsName, realClsName;
                        bool isByRef;
                        i.DeclaringType.GetClassName(out clsName, out realClsName, out isByRef);
                        sb.AppendLine(string.Format("{0}.{1} -= {2};", realClsName, i.Name.Substring(7), param[0].Name));
                    }
                    else
                        throw new NotImplementedException();
                }
                else
                {
                    sb.Append(string.Format("{0}.{1}{2}(", typeClsName, i.Name, genericArguments));
                    param.AppendParameters(sb);
                    sb.AppendLine(");");
                }
            }
            else
            {
                if (isProperty)
                {
                    string[] t = new string[2];
                    int firstUnderlineIndex = i.Name.IndexOf("_");
                    t[0] = i.Name.Substring(0, firstUnderlineIndex);
                    t[1] = i.Name.Substring(firstUnderlineIndex + 1);
                    string propType = t[0];

                    if(noUnbox)
                    {
                        if (propType == "get")
                        {
                            bool isIndexer = param.Length > 0;
                            if (isIndexer)
                            {
                                sb.AppendLine(string.Format("(({1})instance_of_this_method)[{0}];", param[0].Name, typeClsName));
                            }
                            else
                                sb.AppendLine(string.Format("(({1})instance_of_this_method).{0};", t[1], typeClsName));
                        }
                        else if (propType == "set")
                        {
                            bool isIndexer = param.Length > 1;
                            if (isIndexer)
                            {
                                sb.AppendLine(string.Format("(({2})instance_of_this_method)[{0}] = {1};", param[0].Name, param[1].Name, typeClsName));
                            }
                            else
                                sb.AppendLine(string.Format("(({2})instance_of_this_method).{0} = {1};", t[1], param[0].Name, typeClsName, typeClsName));
                        }
                        else if (propType == "add")
                        {
                            sb.AppendLine(string.Format("(({2})instance_of_this_method).{0} += {1};", i.Name.Substring(4), param[0].Name, typeClsName));
                        }
                        else if (propType == "remove")
                        {
                            sb.AppendLine(string.Format("(({2})instance_of_this_method).{0} -= {1};", i.Name.Substring(7), param[0].Name, typeClsName));
                        }
                        else
                            throw new NotImplementedException();
                    }
                    else
                    {
                        if (propType == "get")
                        {
                            bool isIndexer = param.Length > 0;
                            if (isIndexer)
                            {
                                sb.AppendLine(string.Format("instance_of_this_method[{0}];", param[0].Name));
                            }
                            else
                                sb.AppendLine(string.Format("instance_of_this_method.{0};", t[1]));
                        }
                        else if (propType == "set")
                        {
                            bool isIndexer = param.Length > 1;
                            if (isIndexer)
                            {
                                sb.AppendLine(string.Format("instance_of_this_method[{0}] = {1};", param[0].Name, param[1].Name));
                            }
                            else
                                sb.AppendLine(string.Format("instance_of_this_method.{0} = {1};", t[1], param[0].Name));
                        }
                        else if (propType == "add")
                        {
                            sb.AppendLine(string.Format("instance_of_this_method.{0} += {1};", i.Name.Substring(4), param[0].Name));
                        }
                        else if (propType == "remove")
                        {
                            sb.AppendLine(string.Format("instance_of_this_method.{0} -= {1};", i.Name.Substring(7), param[0].Name));
                        }
                        else if (propType == "op") GenerateOperatorCode(sb, t[1], param, i.ReturnType, typeClsName, true);
                        else
                            throw new NotImplementedException();
                    }
                }
                else if (isMultiArr)
                {
                    if (i.Name == "Get")
                    {
                        sb.Append("instance_of_this_method[");
                        param.AppendParameters(sb, true);
                        sb.AppendLine("];");
                    }
                    else
                    {
                        sb.Append("instance_of_this_method[");
                        param.AppendParameters(sb, true, 1);
                        sb.Append("]");
                        sb.Append(" = a");
                        sb.Append(param.Length);
                        sb.AppendLine(";");
                    }
                }
                else
                {
                    if(noUnbox)
                        sb.Append(string.Format("(({2})instance_of_this_method).{0}{1}(", i.Name, genericArguments, typeClsName));
                    else
                        sb.Append(string.Format("instance_of_this_method.{0}{1}(", i.Name, genericArguments));
                    param.AppendParameters(sb);
                    sb.AppendLine(");");
                }
            }
            sb.AppendLine();

            //Ref/Out
            for (int j = param.Length; j > 0; j--)
            {
                var p = param[j - 1];
                if (!p.ParameterType.IsByRef && !hasByRef)
                {
                    continue;
                }
                string clsName, realClsName;
                bool isByRef;
                var pt = p.ParameterType.IsByRef ? p.ParameterType.GetElementType() : p.ParameterType;
                pt.GetClassName(out clsName, out realClsName, out isByRef);
                sb.AppendLine(string.Format("            ptr_of_this_method = __esp - {0};", param.Length - j + 1));
                if (p.ParameterType.IsByRef)
                {
                    sb.AppendLine(@"            switch(ptr_of_this_method->ObjectType)
            {
                case ObjectTypes.StackObjectReference:
                    {
                        var ___dst = ILIntepreter.ResolveReference(ptr_of_this_method);");

                    if (pt.IsValueType && !pt.IsPrimitive && valueTypeBinders != null && valueTypeBinders.Contains(pt))
                    {
                        sb.AppendLine(string.Format("                if (ILRuntime.Runtime.Generated.CLRBindings.s_{0}_Binder != null) {{", clsName));

                        sb.AppendLine(string.Format("                        ILRuntime.Runtime.Generated.CLRBindings.s_{0}_Binder.WriteBackValue(__domain, ptr_of_this_method, __mStack, ref {1});", clsName, p.Name));

                        sb.AppendLine("                } else {");

                        pt.GetRefWriteBackValueCode(sb, p.Name);

                        sb.AppendLine("                }");
                    }
                    else
                    {
                        pt.GetRefWriteBackValueCode(sb, p.Name);
                    }

                    sb.Append(@"                    }
                    break;
                case ObjectTypes.FieldReference:
                    {
                        var ___obj = __mStack[ptr_of_this_method->Value];
                        if(___obj is ILTypeInstance)
                        {
                            ((ILTypeInstance)___obj)[ptr_of_this_method->ValueLow] = @");
                    sb.Append(p.Name);
                    sb.Append(@";
                        }
                        else
                        {
                            var ___type = __domain.GetType(___obj.GetType()) as CLRType;
                            ___type.SetFieldValue(ptr_of_this_method->ValueLow, ref ___obj, @");
                    sb.Append(p.Name);
                    sb.Append(@");
                        }
                    }
                    break;
                case ObjectTypes.StaticFieldReference:
                    {
                        var ___type = __domain.GetType(ptr_of_this_method->Value);
                        if(___type is ILType)
                        {
                            ((ILType)___type).StaticInstance[ptr_of_this_method->ValueLow] = @");
                    sb.Append(p.Name);
                    sb.Append(@";
                        }
                        else
                        {
                            ((CLRType)___type).SetStaticFieldValue(ptr_of_this_method->ValueLow, @");
                    sb.Append(p.Name);
                    sb.Append(@");
                        }
                    }
                    break;
                 case ObjectTypes.ArrayReference:
                    {
                        var instance_of_arrayReference = __mStack[ptr_of_this_method->Value] as ");
                    sb.Append(realClsName);
                    sb.Append(@"[];
                        instance_of_arrayReference[ptr_of_this_method->ValueLow] = @");
                    sb.Append(p.Name);
                    sb.AppendLine(@";
                    }
                    break;
            }");
                    sb.AppendLine();
                }
                else if(pt.IsValueType && !pt.IsPrimitive)
                {
                    sb.AppendLine("            __intp.FreeStackValueType(ptr_of_this_method);");
                }

                sb.AppendLine("            __intp.Free(ptr_of_this_method);");
            }

            if (!i.IsStatic && ((type.IsValueType && !type.IsPrimitive) || hasByRef))//need to write back value type instance
            {
                sb.AppendLine(string.Format("            ptr_of_this_method = __esp - {0};", paramCnt));
                bool noWriteback = noUnbox; 
                if (type.IsValueType && !type.IsPrimitive && !noWriteback)
                {
                    if (valueTypeBinders != null && valueTypeBinders.Contains(type))
                    {
                        string clsName, realClsName;
                        bool isByRef;
                        type.GetClassName(out clsName, out realClsName, out isByRef);

                        sb.AppendLine(string.Format("            if (ILRuntime.Runtime.Generated.CLRBindings.s_{0}_Binder != null) {{", clsName));

                        sb.AppendLine(string.Format("                ILRuntime.Runtime.Generated.CLRBindings.s_{0}_Binder.WriteBackValue(__domain, ptr_of_this_method, __mStack, ref instance_of_this_method);", clsName));

                        sb.AppendLine("            } else {");

                        sb.AppendLine("                WriteBackInstance(__domain, ptr_of_this_method, __mStack, ref instance_of_this_method);");

                        sb.AppendLine("            }");
                    }
                    else
                    {
                        sb.AppendLine("            WriteBackInstance(__domain, ptr_of_this_method, __mStack, ref instance_of_this_method);");
                    }
                    sb.AppendLine();
                }
                sb.AppendLine("            __intp.Free(ptr_of_this_method);");
            }
            
            if (i.ReturnType != typeof(void))
            {
                if (i.ReturnType.IsValueType && !i.ReturnType.IsPrimitive && valueTypeBinders != null && valueTypeBinders.Contains(i.ReturnType))
                {
                    string clsName, realClsName;
                    bool isByRef;
                    i.ReturnType.GetClassName(out clsName, out realClsName, out isByRef);

                    sb.AppendLine(string.Format("            if (ILRuntime.Runtime.Generated.CLRBindings.s_{0}_Binder != null) {{", clsName));

                    sb.AppendLine(string.Format("                ILRuntime.Runtime.Generated.CLRBindings.s_{0}_Binder.PushValue(ref result_of_this_method, __intp, __ret, __mStack);", clsName));
                    sb.AppendLine("                return __ret + 1;");

                    sb.AppendLine("            } else {");

                    i.ReturnType.GetReturnValueCode(sb, domain);

                    sb.AppendLine("            }");
                }
                else
                {
                    i.ReturnType.GetReturnValueCode(sb, domain);
                }
            }
            else
                sb.AppendLine("            return __ret;");
            sb.AppendLine("        }");
        }

        internal static string GenerateMethodWraperCode(this Type type, MethodInfo[] methods, string typeClsName, HashSet<MethodBase> excludes, List<Type> valueTypeBinders, Enviorment.AppDomain domain, string forceInstanceTypeName = null)
        {
            StringBuilder sb = new StringBuilder();
            bool isMultiArr = type.IsArray && type.GetArrayRank() > 1;
            int idx = 0;
            foreach (var i in methods)
            {
                if (excludes != null && excludes.Contains(i))
                    continue;
                if (type.ShouldSkipMethod(i))
                    continue;
                bool isProperty = i.IsSpecialName;
                var param = i.GetParameters();
                int paramCnt = param.Length;
                if (!i.IsStatic)
                    paramCnt++;

                string genericArguments = "";
                if (i.IsGenericMethod)
                {
                    var p = i.GetGenericArguments();
                    StringBuilder sb2 = new StringBuilder();
                    bool first = true;
                    sb2.Append('<');
                    foreach (var j in p)
                    {
                        if (first)
                            first = false;
                        else
                            sb2.Append(", ");
                        string clsName, realClsName;
                        bool isByRef;
                        j.GetClassName(out clsName, out realClsName, out isByRef);
                        sb2.Append(realClsName);
                    }
                    sb2.Append('>');
                    genericArguments = sb2.ToString();
                }

                sb.AppendLine("#if ENABLE_NEO_MODE");
                GenerateMethodWraperCode_Neo(sb, type, i, idx, typeClsName, valueTypeBinders, isMultiArr, isProperty, param, genericArguments);
                sb.AppendLine("#else");
                GenerateMethodWraperCode_Legacy(sb, type, i, idx, typeClsName, valueTypeBinders, isMultiArr, isProperty, param, paramCnt, domain, forceInstanceTypeName, genericArguments);
                sb.AppendLine("#endif");
                
                sb.AppendLine();
                idx++;
            }

            return sb.ToString();
        }
    }
}