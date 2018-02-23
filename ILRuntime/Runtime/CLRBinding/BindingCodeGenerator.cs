using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Other;

namespace ILRuntime.Runtime.CLRBinding
{
    public class BindingCodeGenerator
    {
        
        public static void GenerateBindingCode(List<Type> types, string outputPath, 
                                               HashSet<MethodBase> excludeMethods = null, HashSet<FieldInfo> excludeFields = null, 
                                               List<Type> bindedValueTypes = null, List<Type> delegateTypes = null)
        {
            if (!System.IO.Directory.Exists(outputPath))
                System.IO.Directory.CreateDirectory(outputPath);
            string[] oldFiles = System.IO.Directory.GetFiles(outputPath, "*.cs");
            foreach (var i in oldFiles)
            {
                System.IO.File.Delete(i);
            }
            List<string> clsNames = new List<string>();
            foreach (var i in types)
            {
                string clsName, realClsName;
                bool isByRef;
                if (i.GetCustomAttributes(typeof(ObsoleteAttribute), true).Length > 0)
                    continue;
                i.GetClassName(out clsName, out realClsName, out isByRef);
                clsNames.Add(clsName);
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(outputPath + "/" + clsName + ".cs", false, new UTF8Encoding(false)))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(@"using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
using ILRuntime.Reflection;
using ILRuntime.CLR.Utils;

namespace ILRuntime.Runtime.Generated
{
    unsafe class ");
                    sb.AppendLine(clsName);
                    sb.Append(@"    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
");
                    string flagDef = "            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;";
                    string methodDef = "            MethodBase method;";
                    string fieldDef = "            FieldInfo field;";
                    string argsDef = "            Type[] args;";
                    string typeDef = string.Format("            Type type = typeof({0});", realClsName);

                    MethodInfo[] methods = i.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
                    FieldInfo[] fields = i.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
                    string registerMethodCode = i.GenerateMethodRegisterCode(methods, excludeMethods);
                    string registerFieldCode = i.GenerateFieldRegisterCode(fields, excludeFields);
                    string registerValueTypeCode = i.GenerateValueTypeRegisterCode(realClsName);
                    string registerMiscCode = i.GenerateMiscRegisterCode(realClsName, true, true);
                    string commonCode = i.GenerateCommonCode(realClsName);
                    ConstructorInfo[] ctors = i.GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
                    string ctorRegisterCode = i.GenerateConstructorRegisterCode(ctors, excludeMethods);
                    string methodWraperCode = i.GenerateMethodWraperCode(methods, realClsName, excludeMethods, bindedValueTypes);
                    string fieldWraperCode = i.GenerateFieldWraperCode(fields, realClsName, excludeFields);
                    string cloneWraperCode = i.GenerateCloneWraperCode(fields, realClsName);
                    string ctorWraperCode = i.GenerateConstructorWraperCode(ctors, realClsName, excludeMethods, bindedValueTypes);

                    bool hasMethodCode = !string.IsNullOrEmpty(registerMethodCode);
                    bool hasFieldCode = !string.IsNullOrEmpty(registerFieldCode);
                    bool hasValueTypeCode = !string.IsNullOrEmpty(registerValueTypeCode);
                    bool hasMiscCode = !string.IsNullOrEmpty(registerMiscCode);
                    bool hasCtorCode = !string.IsNullOrEmpty(ctorRegisterCode);
                    bool hasNormalMethod = methods.Where(x => !x.IsGenericMethod).Count() != 0;

                    if ((hasMethodCode && hasNormalMethod) || hasFieldCode || hasCtorCode)
                        sb.AppendLine(flagDef);
                    if (hasMethodCode || hasCtorCode)
                        sb.AppendLine(methodDef);
                    if (hasFieldCode)
                        sb.AppendLine(fieldDef);
                    if (hasMethodCode || hasFieldCode || hasCtorCode)
                        sb.AppendLine(argsDef);
                    if (hasMethodCode || hasFieldCode || hasValueTypeCode || hasMiscCode || hasCtorCode)
                        sb.AppendLine(typeDef);


                    sb.AppendLine(registerMethodCode);
                    sb.AppendLine(registerFieldCode);
                    sb.AppendLine(registerValueTypeCode);
                    sb.AppendLine(registerMiscCode);
                    sb.AppendLine(ctorRegisterCode);
                    sb.AppendLine("        }");
                    sb.AppendLine();
                    sb.AppendLine(commonCode);
                    sb.AppendLine(methodWraperCode);
                    sb.AppendLine(fieldWraperCode);
                    sb.AppendLine(cloneWraperCode);
                    sb.AppendLine(ctorWraperCode);
                    sb.AppendLine("    }");
                    sb.AppendLine("}");

                    sw.Write(Regex.Replace(sb.ToString(), "(?<!\r)\n", "\r\n"));
                    sw.Flush();
                }
            }

            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(outputPath + "/CLRBindings.cs", false, new UTF8Encoding(false)))
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(@"using System;
using System.Collections.Generic;
using System.Reflection;

namespace ILRuntime.Runtime.Generated
{
    class CLRBindings
    {
        /// <summary>
        /// Initialize the CLR binding, please invoke this AFTER CLR Redirection registration
        /// </summary>
        public static void Initialize(ILRuntime.Runtime.Enviorment.AppDomain app)
        {");
                foreach (var i in clsNames)
                {
                    sb.Append("            ");
                    sb.Append(i);
                    sb.AppendLine(".Register(app);");
                }

                sb.AppendLine(@"        }
    }
}");
                sw.Write(Regex.Replace(sb.ToString(), "(?<!\r)\n", "\r\n"));
            }

            GenerateValueTypeBinderMapping(bindedValueTypes, outputPath);
            GenerateDelegateBinding(delegateTypes, outputPath);
        }

        internal class CLRBindingGenerateInfo
        {
            public Type Type { get; set; }
            public HashSet<MethodInfo> Methods { get; set; }
            public HashSet<FieldInfo> Fields { get; set; }
            public HashSet<ConstructorInfo> Constructors { get; set; }
            public bool ArrayNeeded { get; set; }
            public bool DefaultInstanceNeeded { get; set; }
            public bool ValueTypeNeeded { get; set; }

            public bool NeedGenerate
            {
                get
                {
                    if (Methods.Count == 0 && Constructors.Count == 0 && Fields.Count == 0 && !ArrayNeeded && !DefaultInstanceNeeded && !ValueTypeNeeded)
                        return false;
                    else
                    {
                        //Making CLRBinding for such types makes no sense
                        if (Type == typeof(Delegate) || Type == typeof(System.Runtime.CompilerServices.RuntimeHelpers))
                            return false;
                        return true;
                    }
                }
            }
        }

        public static void GenerateBindingCode(ILRuntime.Runtime.Enviorment.AppDomain domain, string outputPath)
        {
            if (domain == null)
                return;
            if (!System.IO.Directory.Exists(outputPath))
                System.IO.Directory.CreateDirectory(outputPath);
            Dictionary<Type, CLRBindingGenerateInfo> infos = new Dictionary<Type, CLRBindingGenerateInfo>(new ByReferenceKeyComparer<Type>());
            CrawlAppdomain(domain, infos);
            string[] oldFiles = System.IO.Directory.GetFiles(outputPath, "*.cs");
            foreach (var i in oldFiles)
            {
                System.IO.File.Delete(i);
            }

            HashSet<MethodBase> excludeMethods = null;
            HashSet<FieldInfo> excludeFields = null;
            HashSet<string> files = new HashSet<string>();
            List<string> clsNames = new List<string>();

            var bindedValueTypes = new List<Type>(domain.ValueTypeBinders.Keys);
            var delegateTypes = new List<Type>();

            foreach (var info in infos)
            {
                if (!info.Value.NeedGenerate)
                    continue;
                Type i = info.Value.Type;
                if (i.BaseType == typeof(MulticastDelegate))
                {
                    delegateTypes.Add(i);
                    continue;
                }
                string clsName, realClsName;
                bool isByRef;
                if (i.GetCustomAttributes(typeof(ObsoleteAttribute), true).Length > 0)
                    continue;
                i.GetClassName(out clsName, out realClsName, out isByRef);
                if (clsNames.Contains(clsName))
                    clsName = clsName + "_t";
                clsNames.Add(clsName);
                string oFileName = outputPath + "/" + clsName;
                int len = Math.Min(oFileName.Length, 100);
                if (len < oFileName.Length)
                    oFileName = oFileName.Substring(0, len) + "_t";
                while (files.Contains(oFileName))
                    oFileName = oFileName + "_t";
                files.Add(oFileName);
                oFileName = oFileName + ".cs";
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(oFileName, false, new UTF8Encoding(false)))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(@"using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
using ILRuntime.Reflection;
using ILRuntime.CLR.Utils;

namespace ILRuntime.Runtime.Generated
{
    unsafe class ");
                    sb.AppendLine(clsName);
                    sb.Append(@"    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
");
                    string flagDef =    "            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;";
                    string methodDef =  "            MethodBase method;";
                    string fieldDef =   "            FieldInfo field;";
                    string argsDef =    "            Type[] args;";
                    string typeDef = string.Format("            Type type = typeof({0});", realClsName);

                    MethodInfo[] methods = info.Value.Methods.ToArray();
                    FieldInfo[] fields = info.Value.Fields.ToArray();
                    string registerMethodCode = i.GenerateMethodRegisterCode(methods, excludeMethods);
                    string registerFieldCode = fields.Length > 0 ? i.GenerateFieldRegisterCode(fields, excludeFields) : null;
                    string registerValueTypeCode = info.Value.ValueTypeNeeded ? i.GenerateValueTypeRegisterCode(realClsName) : null;
                    string registerMiscCode = i.GenerateMiscRegisterCode(realClsName, info.Value.DefaultInstanceNeeded, info.Value.ArrayNeeded);
                    string commonCode = i.GenerateCommonCode(realClsName);
                    ConstructorInfo[] ctors = info.Value.Constructors.ToArray();
                    string ctorRegisterCode = i.GenerateConstructorRegisterCode(ctors, excludeMethods);
                    string methodWraperCode = i.GenerateMethodWraperCode(methods, realClsName, excludeMethods, bindedValueTypes);
                    string fieldWraperCode = fields.Length > 0 ? i.GenerateFieldWraperCode(fields, realClsName, excludeFields) : null;
                    string cloneWraperCode = null;
                    if (info.Value.ValueTypeNeeded)
                    {
                        //Memberwise clone should copy all fields
                        var fs = i.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
                        cloneWraperCode = i.GenerateCloneWraperCode(fs, realClsName);
                    }

                    bool hasMethodCode = !string.IsNullOrEmpty(registerMethodCode);
                    bool hasFieldCode = !string.IsNullOrEmpty(registerFieldCode);
                    bool hasValueTypeCode = !string.IsNullOrEmpty(registerValueTypeCode);
                    bool hasMiscCode = !string.IsNullOrEmpty(registerMiscCode);
                    bool hasCtorCode = !string.IsNullOrEmpty(ctorRegisterCode);
                    bool hasNormalMethod = methods.Where(x => !x.IsGenericMethod).Count() != 0;

                    if ((hasMethodCode && hasNormalMethod) || hasFieldCode || hasCtorCode)
                        sb.AppendLine(flagDef);
                    if (hasMethodCode || hasCtorCode)
                        sb.AppendLine(methodDef);
                    if (hasFieldCode)
                        sb.AppendLine(fieldDef);
                    if (hasMethodCode || hasFieldCode || hasCtorCode)
                        sb.AppendLine(argsDef);
                    if (hasMethodCode || hasFieldCode || hasValueTypeCode || hasMiscCode || hasCtorCode)
                        sb.AppendLine(typeDef);

                    sb.AppendLine(registerMethodCode);
                    if (fields.Length > 0)
                        sb.AppendLine(registerFieldCode);
                    if (info.Value.ValueTypeNeeded)
                        sb.AppendLine(registerValueTypeCode);
                    if (!string.IsNullOrEmpty(registerMiscCode))
                        sb.AppendLine(registerMiscCode);
                    sb.AppendLine(ctorRegisterCode);
                    sb.AppendLine("        }");
                    sb.AppendLine();
                    sb.AppendLine(commonCode);
                    sb.AppendLine(methodWraperCode);
                    if (fields.Length > 0)
                        sb.AppendLine(fieldWraperCode);
                    if (info.Value.ValueTypeNeeded)
                        sb.AppendLine(cloneWraperCode);
                    string ctorWraperCode = i.GenerateConstructorWraperCode(ctors, realClsName, excludeMethods, bindedValueTypes);
                    sb.AppendLine(ctorWraperCode);
                    sb.AppendLine("    }");
                    sb.AppendLine("}");

                    sw.Write(Regex.Replace(sb.ToString(), "(?<!\r)\n", "\r\n"));
                    sw.Flush();
                }
            }

            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(outputPath + "/CLRBindings.cs", false, new UTF8Encoding(false)))
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(@"using System;
using System.Collections.Generic;
using System.Reflection;

namespace ILRuntime.Runtime.Generated
{
    class CLRBindings
    {
        /// <summary>
        /// Initialize the CLR binding, please invoke this AFTER CLR Redirection registration
        /// </summary>
        public static void Initialize(ILRuntime.Runtime.Enviorment.AppDomain app)
        {");
                foreach (var i in clsNames)
                {
                    sb.Append("            ");
                    sb.Append(i);
                    sb.AppendLine(".Register(app);");
                }

                sb.AppendLine(@"        }
    }
}");
                sw.Write(Regex.Replace(sb.ToString(), "(?<!\r)\n", "\r\n"));
            }

            GenerateValueTypeBinderMapping(bindedValueTypes, outputPath);
            GenerateDelegateBinding(delegateTypes, outputPath);
        }

        internal static void CrawlAppdomain(ILRuntime.Runtime.Enviorment.AppDomain domain, Dictionary<Type, CLRBindingGenerateInfo> infos)
        {
            var arr = domain.LoadedTypes.Values.ToArray();
            //Prewarm
            foreach (var type in arr)
            {
                if (type is CLR.TypeSystem.ILType)
                {
                    if (type.HasGenericParameter)
                        continue;
                    var methods = type.GetMethods().ToList();
                    foreach (var i in ((CLR.TypeSystem.ILType)type).GetConstructors())
                        methods.Add(i);
                    if (((CLR.TypeSystem.ILType)type).GetStaticConstroctor() != null)
                        methods.Add(((CLR.TypeSystem.ILType)type).GetStaticConstroctor());
                    foreach (var j in methods)
                    {
                        CLR.Method.ILMethod method = j as CLR.Method.ILMethod;
                        if (method != null)
                        {
                            if (method.GenericParameterCount > 0 && !method.IsGenericInstance)
                                continue;
                            var body = method.Body;
                        }
                    }
                }
            }
            arr = domain.LoadedTypes.Values.ToArray();
            foreach (var type in arr)
            {
                if (type is CLR.TypeSystem.ILType)
                {
                    if (type.TypeForCLR.IsByRef || type.HasGenericParameter)
                        continue;
                    var methods = type.GetMethods().ToList();
                    foreach (var i in ((CLR.TypeSystem.ILType)type).GetConstructors())
                        methods.Add(i);

                    foreach (var j in methods)
                    {
                        CLR.Method.ILMethod method = j as CLR.Method.ILMethod;
                        if (method != null)
                        {
                            if (method.GenericParameterCount > 0 && !method.IsGenericInstance)
                                continue;
                            var body = method.Body;
                            foreach (var ins in body)
                            {
                                switch (ins.Code)
                                {
                                    case Intepreter.OpCodes.OpCodeEnum.Newobj:
                                        {
                                            CLR.Method.CLRMethod m = domain.GetMethod(ins.TokenInteger) as CLR.Method.CLRMethod;
                                            if (m != null)
                                            {
                                                if (m.DeclearingType.IsDelegate)
                                                    continue;
                                                Type t = m.DeclearingType.TypeForCLR;
                                                CLRBindingGenerateInfo info;
                                                if (!infos.TryGetValue(t, out info))
                                                {
                                                    info = CreateNewBindingInfo(t);
                                                    infos[t] = info;
                                                }
                                                if (m.IsConstructor)
                                                    info.Constructors.Add(m.ConstructorInfo);
                                                else
                                                    info.Methods.Add(m.MethodInfo);
                                            }
                                        }
                                        break;
                                    case Intepreter.OpCodes.OpCodeEnum.Ldfld:
                                    case Intepreter.OpCodes.OpCodeEnum.Stfld:
                                    case Intepreter.OpCodes.OpCodeEnum.Ldflda:
                                    case Intepreter.OpCodes.OpCodeEnum.Ldsfld:
                                    case Intepreter.OpCodes.OpCodeEnum.Ldsflda:
                                    case Intepreter.OpCodes.OpCodeEnum.Stsfld:
                                        {
                                            var t = domain.GetType((int)(ins.TokenLong >> 32)) as CLR.TypeSystem.CLRType;
                                            if(t != null)
                                            {
                                                var fi = t.GetField((int)ins.TokenLong);
                                                if (fi != null && fi.IsPublic)
                                                {
                                                    CLRBindingGenerateInfo info;
                                                    if (!infos.TryGetValue(t.TypeForCLR, out info))
                                                    {
                                                        info = CreateNewBindingInfo(t.TypeForCLR);
                                                        infos[t.TypeForCLR] = info;
                                                    }
                                                    if(ins.Code == Intepreter.OpCodes.OpCodeEnum.Stfld || ins.Code == Intepreter.OpCodes.OpCodeEnum.Stsfld)
                                                    {
                                                        if (t.IsValueType)
                                                        {
                                                            info.ValueTypeNeeded = true;
                                                            info.DefaultInstanceNeeded = true;
                                                        }
                                                    }
                                                    if (t.TypeForCLR.CheckCanPinn() || !t.IsValueType)
                                                        info.Fields.Add(fi);
                                                }
                                            }
                                        }
                                        break;
                                    case Intepreter.OpCodes.OpCodeEnum.Ldtoken:
                                        {
                                            if (ins.TokenInteger == 0)
                                            {
                                                var t = domain.GetType((int)(ins.TokenLong >> 32)) as CLR.TypeSystem.CLRType;
                                                if (t != null)
                                                {
                                                    var fi = t.GetField((int)ins.TokenLong);
                                                    if (fi != null)
                                                    {
                                                        CLRBindingGenerateInfo info;
                                                        if (!infos.TryGetValue(t.TypeForCLR, out info))
                                                        {
                                                            info = CreateNewBindingInfo(t.TypeForCLR);
                                                            infos[t.TypeForCLR] = info;
                                                        }
                                                        info.Fields.Add(fi);
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                    case Intepreter.OpCodes.OpCodeEnum.Newarr:
                                        {
                                            var t = domain.GetType(ins.TokenInteger) as CLR.TypeSystem.CLRType;
                                            if(t != null)
                                            {
                                                CLRBindingGenerateInfo info;
                                                if (!infos.TryGetValue(t.TypeForCLR, out info))
                                                {
                                                    info = CreateNewBindingInfo(t.TypeForCLR);
                                                    infos[t.TypeForCLR] = info;
                                                }
                                                info.ArrayNeeded = true;
                                            }
                                        }
                                        break;
                                    case Intepreter.OpCodes.OpCodeEnum.Call:
                                    case Intepreter.OpCodes.OpCodeEnum.Callvirt:
                                        {
                                            CLR.Method.CLRMethod m = domain.GetMethod(ins.TokenInteger) as CLR.Method.CLRMethod;
                                            if (m != null)
                                            {
                                                //Cannot explicit call base class's constructor directly
                                                if (m.IsConstructor)
                                                    continue;
                                                if (!m.MethodInfo.IsPublic)
                                                    continue;
                                                Type t = m.DeclearingType.TypeForCLR;
                                                CLRBindingGenerateInfo info;
                                                if (!infos.TryGetValue(t, out info))
                                                {
                                                    info = CreateNewBindingInfo(t);
                                                    infos[t] = info;
                                                }

                                                info.Methods.Add(m.MethodInfo);
                                            }
                                        }
                                        break;
                                }
                            }
                        }
                    }
                }
            }
        }

        internal static CLRBindingGenerateInfo CreateNewBindingInfo(Type t)
        {
            CLRBindingGenerateInfo info = new CLRBindingGenerateInfo();
            info.Type = t;
            info.Methods = new HashSet<MethodInfo>();
            info.Fields = new HashSet<FieldInfo>();
            info.Constructors = new HashSet<ConstructorInfo>();
            if (t.IsValueType)
                info.DefaultInstanceNeeded = true;
            return info;
        }

        internal static void GenerateValueTypeBinderParseMethod(StringBuilder sb, Type type, string clsName, string typeClsName)
        {
            sb.AppendLine(string.Format("        public static {0} Parse_{1}(ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)", typeClsName, clsName));
            sb.AppendLine("        {");
            sb.AppendLine(string.Format("            var clrType = (CLRType)__intp.AppDomain.GetType(typeof({0}));", typeClsName));
            sb.AppendLine(string.Format("            var binder = (ValueTypeBinder<{0}>)clrType.ValueTypeBinder;", typeClsName));
            sb.AppendLine();
            sb.AppendLine(string.Format("            {0} value = new {0}();", typeClsName));
            sb.AppendLine();
            sb.AppendLine("            var a = ILIntepreter.GetObjectAndResolveReference(ptr_of_this_method);");
            sb.AppendLine("            if (a->ObjectType == ObjectTypes.ValueTypeObjectReference)");
            sb.AppendLine("            {");
            sb.AppendLine("                var ptr = *(StackObject**)&a->Value;");
            sb.AppendLine("                binder.AssignFromStack(ref value, ptr, __mStack);");
            sb.AppendLine("                __intp.FreeStackValueType(ptr_of_this_method);");
            sb.AppendLine("            }");
            sb.AppendLine("            else");
            sb.AppendLine("            {");
            sb.AppendLine(string.Format("                value = ({0})StackObject.ToObject(a, __intp.AppDomain, __mStack);", typeClsName));
            sb.AppendLine("                __intp.Free(ptr_of_this_method);");
            sb.AppendLine("            }");
            sb.AppendLine();
            sb.AppendLine("            return value;");
            sb.AppendLine("        }");
        }

        internal static void GenerateValueTypeBinderWriteBackMethod(StringBuilder sb, Type type, string clsName, string typeClsName)
        {
            sb.AppendLine(string.Format("        public static void WriteBack_{0}(ILRuntime.Runtime.Enviorment.AppDomain __domain, StackObject* ptr_of_this_method, IList<object> __mStack, ref {1} instance_of_this_method)", clsName, typeClsName));
            sb.AppendLine("        {");
            sb.AppendLine(@"            ptr_of_this_method = ILIntepreter.GetObjectAndResolveReference(ptr_of_this_method);
            switch(ptr_of_this_method->ObjectType)
            {
                case ObjectTypes.Object:
                    {
                        __mStack[ptr_of_this_method->Value] = instance_of_this_method;");
            sb.Append(@"                    }
                    break;
                case ObjectTypes.FieldReference:
                    {
                        var ___obj = __mStack[ptr_of_this_method->Value];
                        if(___obj is ILTypeInstance)
                        {
                            ((ILTypeInstance)___obj)[ptr_of_this_method->ValueLow] = instance_of_this_method");
            sb.Append(@";
                        }
                        else
                        {
                            var t = __domain.GetType(___obj.GetType()) as CLRType;
                            t.SetFieldValue(ptr_of_this_method->ValueLow, ref ___obj, instance_of_this_method");
            sb.Append(@");
                        }
                    }
                    break;
                case ObjectTypes.StaticFieldReference:
                    {
                        var t = __domain.GetType(ptr_of_this_method->Value);
                        if(t is ILType)
                        {
                            ((ILType)t).StaticInstance[ptr_of_this_method->ValueLow] = instance_of_this_method");
            sb.Append(@";
                        }
                        else
                        {
                            ((CLRType)t).SetStaticFieldValue(ptr_of_this_method->ValueLow, instance_of_this_method");
            sb.Append(@");
                        }
                    }
                    break;
                case ObjectTypes.ArrayReference:
                    {
                        var instance_of_arrayReference = __mStack[ptr_of_this_method->Value] as ");
            sb.Append(typeClsName);
            sb.AppendLine(@"[];
                        instance_of_arrayReference[ptr_of_this_method->ValueLow] = instance_of_this_method;
                    }
                    break;");

            // for binded value type
            sb.AppendLine("                case ObjectTypes.ValueTypeObjectReference:");
            sb.AppendLine("                    {");
            sb.AppendLine(string.Format("                        var clrType = (CLRType)__domain.GetType(typeof({0}));", typeClsName));
            sb.AppendLine(string.Format("                        var binder = (ValueTypeBinder<{0}>)clrType.ValueTypeBinder;", typeClsName));
            sb.AppendLine();
            sb.AppendLine("                        var dst = *((StackObject**)&ptr_of_this_method->Value);");
            sb.AppendLine("                        binder.CopyValueTypeToStack (ref instance_of_this_method, dst, __mStack);");
            sb.AppendLine("                    }");
            sb.AppendLine("                    break;");

            sb.AppendLine("            }");
            sb.AppendLine(@"        }");
        }

        internal static void GenerateValueTypeBinderPushMethod(StringBuilder sb, Type type, string clsName, string typeClsName)
        {
            sb.AppendLine(string.Format("        public static void Push_{0}(ref {1} value, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)", clsName, typeClsName));
            sb.AppendLine("        {");
            sb.AppendLine(string.Format("            var clrType = (CLRType)__intp.AppDomain.GetType(typeof({0}));", typeClsName));
            sb.AppendLine(string.Format("            var binder = (ValueTypeBinder<{0}>)clrType.ValueTypeBinder;", typeClsName));
            sb.AppendLine();
            sb.AppendLine("            __intp.AllocValueType(ptr_of_this_method, binder.CLRType);");
            sb.AppendLine("            var dst = *((StackObject**)&ptr_of_this_method->Value);");
            sb.AppendLine("            binder.CopyValueTypeToStack(ref value, dst, __mStack);");
            sb.AppendLine("        }");
        }

        internal static void GenerateValueTypeBinderMapping(List<Type> bindedValueTypes, string outputPath)
        {
            if (bindedValueTypes == null)
                bindedValueTypes = new List<Type>(0);

            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(outputPath + "/ValueTypeBinderMapping.cs", false, new UTF8Encoding(false)))
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(@"using System.Collections.Generic;
using ILRuntime.Other;
using System;
using System.Reflection;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Stack;

public static unsafe class ValueTypeBinderMapping
{");
                sb.AppendLine();

                foreach (var i in bindedValueTypes)
                {
                    string clsName, typeClsName;
                    bool isByRef;
                    i.GetClassName(out clsName, out typeClsName, out isByRef);

                    GenerateValueTypeBinderParseMethod(sb, i, clsName, typeClsName);
                    sb.AppendLine();

                    GenerateValueTypeBinderWriteBackMethod(sb, i, clsName, typeClsName);
                    sb.AppendLine();

                    GenerateValueTypeBinderPushMethod(sb, i, clsName, typeClsName);
                    sb.AppendLine();
                    sb.AppendLine();
                }

                sb.AppendLine();
                sb.AppendLine(@"}");

                sw.Write(Regex.Replace(sb.ToString(), "(?<!\r)\n", "\r\n"));
            }
        }

        internal static void GenerateDelegateBinding(List<Type> types, string outputPath)
        {
            if (types == null)
                types = new List<Type>(0);

            List<string> clsNames = new List<string>();

            foreach (var i in types)
            {
                var mi = i.GetMethod("Invoke");
                var miParameters = mi.GetParameters();
                if (mi.ReturnType == typeof(void) && miParameters.Length == 0)
                    continue;

                string clsName, realClsName, paramClsName, paramRealClsName;
                bool isByRef, paramIsByRef;
                i.GetClassName(out clsName, out realClsName, out isByRef);
                clsNames.Add(clsName);
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(outputPath + "/" + clsName + ".cs", false, new UTF8Encoding(false)))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(@"using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
using ILRuntime.Reflection;
using ILRuntime.CLR.Utils;

namespace ILRuntime.Runtime.Generated
{
    unsafe class ");
                    sb.AppendLine(clsName);
                    sb.AppendLine(@"    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {");
                    bool first = true;

                    if (mi.ReturnType != typeof(void))
                    {
                        sb.Append("            app.DelegateManager.RegisterFunctionDelegate<");
                        first = true;
                        foreach (var j in miParameters)
                        {
                            if (first)
                            {
                                first = false;
                            }
                            else
                                sb.Append(", ");
                            j.ParameterType.GetClassName(out paramClsName, out paramRealClsName, out paramIsByRef);
                            sb.Append(paramRealClsName);
                        }
                        if (!first)
                            sb.Append(", ");
                        mi.ReturnType.GetClassName(out paramClsName, out paramRealClsName, out paramIsByRef);
                        sb.Append(paramRealClsName);
                        sb.AppendLine("> ();");
                    }
                    else
                    {
                        sb.Append("            app.DelegateManager.RegisterMethodDelegate<");
                        first = true;
                        foreach (var j in miParameters)
                        {
                            if (first)
                            {
                                first = false;
                            }
                            else
                                sb.Append(", ");
                            j.ParameterType.GetClassName(out paramClsName, out paramRealClsName, out paramIsByRef);
                            sb.Append(paramRealClsName);
                        }
                        sb.AppendLine("> ();");
                    }
                    sb.AppendLine();

                    sb.Append("            app.DelegateManager.RegisterDelegateConvertor<");
                    sb.Append(realClsName);
                    sb.AppendLine(">((act) =>");
                    sb.AppendLine("            {");
                    sb.Append("                return new ");
                    sb.Append(realClsName);
                    sb.Append("((");
                    first = true;
                    foreach (var j in miParameters)
                    {
                        if (first)
                        {
                            first = false;
                        }
                        else
                            sb.Append(", ");
                        sb.Append(j.Name);
                    }
                    sb.AppendLine(") =>");
                    sb.AppendLine("                {");
                    if (mi.ReturnType != typeof(void))
                    {
                        sb.Append("                    return ((Func<");
                        first = true;
                        foreach (var j in miParameters)
                        {
                            if (first)
                            {
                                first = false;
                            }
                            else
                                sb.Append(", ");
                            j.ParameterType.GetClassName(out paramClsName, out paramRealClsName, out paramIsByRef);
                            sb.Append(paramRealClsName);
                        }
                        if (!first)
                            sb.Append(", ");
                        mi.ReturnType.GetClassName(out paramClsName, out paramRealClsName, out paramIsByRef);
                        sb.Append(paramRealClsName);
                    }
                    else
                    {
                        sb.Append("                    ((Action<");
                        first = true;
                        foreach (var j in miParameters)
                        {
                            if (first)
                            {
                                first = false;
                            }
                            else
                                sb.Append(", ");
                            j.ParameterType.GetClassName(out paramClsName, out paramRealClsName, out paramIsByRef);
                            sb.Append(paramRealClsName);
                        }
                    }
                    sb.Append(">)act)(");
                    first = true;
                    foreach (var j in miParameters)
                    {
                        if (first)
                        {
                            first = false;
                        }
                        else
                            sb.Append(", ");
                        sb.Append(j.Name);
                    }
                    sb.AppendLine(");");
                    sb.AppendLine("                });");
                    sb.AppendLine("            });");

                    sb.AppendLine("        }");
                    sb.AppendLine("    }");
                    sb.AppendLine("}");

                    sw.Write(Regex.Replace(sb.ToString(), "(?<!\r)\n", "\r\n"));
                    sw.Flush();
                }
            }

            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(outputPath + "/CLRBindings_Delegate.cs", false, new UTF8Encoding(false)))
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(@"using System;
using System.Collections.Generic;
using System.Reflection;

namespace ILRuntime.Runtime.Generated
{
    class CLRDelegateBindings
    {
        /// <summary>
        /// Initialize the CLR Delegate binding, please invoke this AFTER CLR Redirection registration
        /// </summary>
        public static void Initialize(ILRuntime.Runtime.Enviorment.AppDomain app)
        {");
                foreach (var i in clsNames)
                {
                    sb.Append("            ");
                    sb.Append(i);
                    sb.AppendLine(".Register(app);");
                }

                sb.AppendLine(@"        }
    }
}");
                sw.Write(Regex.Replace(sb.ToString(), "(?<!\r)\n", "\r\n"));
            }
        }

    }
}
