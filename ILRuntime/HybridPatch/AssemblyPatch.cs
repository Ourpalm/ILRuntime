using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Mono.Cecil;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Intepreter.OpCodes;
using ILRuntime.Runtime.Stack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
namespace ILRuntime.Hybrid
{
    public class AssemblyPatch
    {
        AssemblyPatchInfo patchInfo;
        const string PatchedTypeName = "ILRuntime.Hybrid.Patched";
        List<KeyValuePair<ILType, int>> patchedTypes = new List<KeyValuePair<ILType, int>>();
        List<KeyValuePair<ILMethod, int>> patchedMethods = new List<KeyValuePair<ILMethod, int>>();
        Dictionary<string, TypeDefinition> internalTypes;
        Dictionary<string, TypeReference> genericParameters = new Dictionary<string, TypeReference>();
        ModuleDefinition module;
        public string Name { get { return patchInfo.Name; } }

        public bool IsPatched { get;private set; }

        public void ApplyPatch(ILType[] types, ILMethod[] methods)
        {
            foreach(var i in patchedTypes)
            {
                types[i.Value] = i.Key;
            }
            foreach(var i in patchedMethods)
            {
                methods[i.Value] = i.Key;
            }
        }

        TypeReference FindGenericArgument(string name, TypeReference ctxType)
        {
            if (ctxType != null && ctxType is GenericInstanceType git)
            {
                var gd = git.ElementType;
                for (int i = 0; i < gd.GenericParameters.Count; i++)
                {
                    if (gd.GenericParameters[i].Name == name)
                        return git.GenericArguments[i];
                }
            }
            else if (ctxType != null && ctxType.GenericParameters.Count > 0)
            {
                for (int i = 0; i < ctxType.GenericParameters.Count; i++)
                {
                    if (ctxType.GenericParameters[i].Name == name)
                        return ctxType.GenericParameters[i];
                }
            }
            return null;
        }

        TypeReference ResolveType(TypeReferencePatchInfo info, Runtime.Enviorment.AppDomain domain, TypeReference ctxType, bool preferInternal = false)
        {
            if (info.IsByReference)
            {
                var et = ResolveType(info.ElementType, domain, ctxType, preferInternal);
                return new ByReferenceType(et);
            }
            else if (info.IsArray)
            {
                var et = ResolveType(info.ElementType, domain, ctxType, preferInternal);
                return new ArrayType(et);
            }
            else if (info.IsGenericInstance)
            {
                var et = ResolveType(info.ElementType, domain, null, preferInternal);
                TypeReference[] args = new TypeReference[info.GenericArguments.Length];
                for(int i = 0; i < args.Length; i++)
                {
                    args[i] = ResolveType(info.GenericArguments[i].Value, domain, ctxType, preferInternal);
                }
                return et.MakeGenericInstanceType(args);
            }
            else if (info.IsGenericParameter)
            {
                var ga = FindGenericArgument(info.Name, ctxType);
                if (ga != null)
                    return ga;
                if(!genericParameters.TryGetValue(info.Name, out var gp))
                {
                    gp = new GenericParameter(info.Name, new TypeDefinition("dummy", "dummy", Mono.Cecil.TypeAttributes.Public));
                    genericParameters[info.Name] = gp;
                }
                return gp;
            }
            else
            {
                var it = domain.GetType(info.Name);
                TypeDefinition type = null;
                TypeReference res = null;
                preferInternal = preferInternal || it == null;
                if (preferInternal && internalTypes.TryGetValue(info.Name, out type))
                {
                    res = type;
                }
                if (res == null)
                {
                    if (it is ILType iltype)
                    {
                        res = iltype.TypeDefinition;
                    }
                    else
                    {
                        res = module.ImportReference(((CLRType)it).TypeForCLR);
                    }
                }
                if (res.HasGenericParameters && ctxType != null)
                {
                    TypeReference[] args = new TypeReference[res.GenericParameters.Count];
                    for (int i = 0; i < args.Length; i++)
                    {
                        var p = res.GenericParameters[i];
                        args[i] = FindGenericArgument(p.Name, ctxType);
                    }
                    res = res.MakeGenericInstanceType(args);
                }
                return res;
            }
        }

        void InitializeTypes(Runtime.Enviorment.AppDomain domain)
        {
            internalTypes = new Dictionary<string, TypeDefinition>();
            List<TypeDefinition> defs = new List<TypeDefinition>();
            List<ILType> types = new List<ILType>();
            foreach (var i in patchInfo.Types)
            {
                string name = i.IsNew ? i.Name : $"{i.Name}___Extra";
                TypeDefinition td = new TypeDefinition(i.Namespace, name, Mono.Cecil.TypeAttributes.Public, module.TypeSystem.Object);
                internalTypes[$"{i.Namespace}.{i.Name}"] = td;
                if (i.IsGenericDefinition)
                {
                    foreach (var j in i.GenericParameters)
                    {
                        td.GenericParameters.Add(new GenericParameter(j, td));
                    }
                }
                ILType type = new ILType(td, domain);
                domain.AddType(type);
                types.Add(type);
                defs.Add(td);
            }
            for (int i = 0; i < defs.Count; i++)
            {
                var td = defs[i];
                var type = types[i];
                var info = patchInfo.Types[i];
                if(info.BaseType != null)
                {
                    td.BaseType = ResolveType(info.BaseType, domain, td, true);
                }
                foreach(var f in info.Fields)
                {
                    var ft = ResolveType(f.FieldType, domain, td);
                    Mono.Cecil.FieldAttributes attr = Mono.Cecil.FieldAttributes.Public;
                    if (f.IsStatic)
                        attr |= Mono.Cecil.FieldAttributes.Static;
                    FieldDefinition fd = new FieldDefinition(f.Name, attr, ft);
                    fd.InitialValue = f.InitialValues;
                    td.Fields.Add(fd);
                }
                if (!info.IsNew)
                    patchedTypes.Add(new KeyValuePair<ILType, int>(type, info.Index));
                foreach (var m in info.Methods)
                {
                    Mono.Cecil.MethodAttributes attr = Mono.Cecil.MethodAttributes.Public;
                    if (!info.IsNew || m.IsStatic)
                        attr |= Mono.Cecil.MethodAttributes.Static;
                    MethodDefinition mdef = new MethodDefinition(m.Name, attr, ResolveType(m.ReturnType, domain, td));

                    foreach(var p in m.Parameters)
                    {
                        mdef.Parameters.Add(new ParameterDefinition(ResolveType(p, domain, td)));
                    }
                    if (m.GenericParameters.Length > 0)
                    {
                        foreach (var ga in m.GenericParameters)
                        {
                            mdef.GenericParameters.Add(new GenericParameter(ResolveType(ga, domain, td).Name, mdef));
                        }
                    }
                    mdef.Body = new Mono.Cecil.Cil.MethodBody(mdef);
                    foreach(var v in m.LocalVariables)
                    {
                        mdef.Body.Variables.Add(new Mono.Cecil.Cil.VariableDefinition(ResolveType(v, domain, td)));
                    }
                    td.Methods.Add(mdef);
                }
            }

            for(int i = 0; i < defs.Count; i++)
            {
                var td = defs[i];
                var type = types[i];
                var info = patchInfo.Types[i];
                var methods = type.GetMethods();
                for (int j = 0; j < methods.Count; j++)
                {
                    var m = methods[j];
                    var mi = info.Methods[j];
                    if (!mi.IsNew)
                        patchedMethods.Add(new KeyValuePair<ILMethod, int>((ILMethod)m, mi.Index));

                    if (td.HasGenericParameters)
                    {
                        ((ILMethod)m).SetMethodPatchContext(new MethodPatchContext(this, mi));
                    }
                    else
                        InitializeMethodBody(td, mi, (ILMethod)m, domain);
                }
            }
        }

        int GetFieldIndex(IType type, string name)
        {
            if(type is ILType ilType)
            {
                ilType.GetField(name, out int idx);
                return idx;
            }
            else
            {
                CLRType clrType = type as CLRType;
                return clrType.GetFieldIndex(name);
            }
        }

        MethodReference ResolveMethod(MethodReferencePatchInfo method, Runtime.Enviorment.AppDomain domain, TypeReference ctxType, bool preferInternal=false)
        {
            if (method.IsGenericInstance)
            {
                var mDef = ResolveMethod(method.GenericDefinition, domain, ctxType);
                GenericInstanceMethod gim = new GenericInstanceMethod(mDef);
                for (int i = 0; i < mDef.GenericParameters.Count; i++)
                {
                    var args = method.GenericArguments[i];
                    if (mDef.GenericParameters[i].Name == args.Key)
                    {
                        gim.GenericArguments.Add(ResolveType(args.Value, domain, ctxType));
                    }
                    else
                        throw new Exception();
                }
                return gim;
            }
            else
            {
                var dtr = ResolveType(method.DeclaringType, domain, ctxType, preferInternal);
                TypeDefinition dt;
                if (dtr is GenericInstanceType && dtr.Module == null)
                    dt = dtr.GetElementType().Resolve();
                else
                    dt = dtr.Resolve();
                foreach (var i in dt.Methods)
                {
                    bool isExtendedMethod = (i.IsStatic != method.IsStatic && method.Parameters.Length == i.Parameters.Count - 1);
                    if (i.Name == method.Name && (i.Parameters.Count == method.Parameters.Length || isExtendedMethod))
                    {
                        bool isMatch = true;
                        for (int j = 0; j < i.Parameters.Count; j++)
                        {
                            if (j == 0 && isExtendedMethod)
                            {
                                var pt = ResolveType(method.DeclaringType, domain, dtr);
                                var pt2 = ResolveType(TypeReferencePatchInfo.Create(i.Parameters[0].ParameterType, null), domain, dtr);
                                if (!pt.CheckTypeReferenceEqual(pt2))
                                {
                                    isMatch = false;
                                    break;
                                }
                            }
                            else
                            {
                                int offset = isExtendedMethod ? -1 : 0;
                                var pt = ResolveType(method.Parameters[j + offset], domain, dtr);
                                var pt2 = ResolveType(TypeReferencePatchInfo.Create(i.Parameters[j].ParameterType, null), domain, dtr);
                                if (!pt.CheckTypeReferenceEqual(pt2))
                                {
                                    isMatch = false;
                                    break;
                                }
                            }
                        }
                        if (isMatch)
                        {
                            if(dtr is GenericInstanceType)
                            {
                                MethodReference res = new MethodReference(i.Name, i.ReturnType, dtr);
                                foreach (var p in i.Parameters)
                                {
                                    var pt = ResolveType(TypeReferencePatchInfo.Create(p.ParameterType, null), domain, dtr);
                                    res.Parameters.Add(new ParameterDefinition(pt));
                                }
                                return res;
                            }
                            return i;
                        }
                    }
                }
                if (!preferInternal)
                    return ResolveMethod(method, domain, ctxType, true);
            }
            return null;
        }

        
        internal void InitializeMethodBody(TypeReference declaringType, MethodPatchInfo info, ILMethod method, Runtime.Enviorment.AppDomain domain)
        {
            var body = info.CodeBody;
            OpCode[] res = new OpCode[body.Length];
            Dictionary<int, int[]> jumptables = null;
            for(int i = 0; i < body.Length; i++)
            {
                var opcode = body[i];
                switch (opcode.Code)
                {
                    case OpCodeEnum.Call:
                    case OpCodeEnum.Newobj:
                    case OpCodeEnum.Ldftn:
                    case OpCodeEnum.Ldvirtftn:
                    case OpCodeEnum.Callvirt:
                        {
                            if (opcode.TokenInteger > 0)
                            {
                                var mr = ResolveMethod(patchInfo.MethodReferences[opcode.TokenInteger - 1], domain, declaringType);
                                var im = domain.GetMethod(mr, (ILType)method.DeclearingType, method, out var invalidToken);
                                domain.CacheMethod(im);
                                opcode.TokenInteger = im.GetHashCode();
                            }
                        }
                        break;
                    case OpCodeEnum.Box:
                    case OpCodeEnum.Unbox_Any:
                    case OpCodeEnum.Unbox:
                    case OpCodeEnum.Initobj:
                    case OpCodeEnum.Isinst:
                    case OpCodeEnum.Newarr:
                    case OpCodeEnum.Stobj:
                    case OpCodeEnum.Ldobj:
                    case OpCodeEnum.Castclass:
                        {
                            var it = domain.GetType(ResolveType(patchInfo.TypeReferences[opcode.TokenInteger], domain, null), method.DeclearingType, method);
                            opcode.TokenInteger = it.GetHashCode();
                        }
                        break;
                    case OpCodeEnum.Constrained:
                        {
                            var it = domain.GetType(ResolveType(patchInfo.TypeReferences[opcode.TokenInteger], domain, null), method.DeclearingType, method);
                            opcode.TokenInteger = it.GetHashCode();

                            var mr = ResolveMethod(patchInfo.MethodReferences[opcode.TokenLong - 1], domain, declaringType);
                            var im = domain.GetMethod(mr, (ILType)method.DeclearingType, method, out var invalidToken);
                            domain.CacheMethod(im);
                            opcode.TokenLong = im.GetHashCode();
                        }
                        break;
                    case OpCodeEnum.Stfld:
                    case OpCodeEnum.Ldfld:
                    case OpCodeEnum.Ldflda:
                        {
                            var fr = patchInfo.FieldReferences[(int)opcode.TokenLong];
                            var ft = domain.GetType(ResolveType(fr.DeclaringType, domain, null, fr.IsInternal), method.DeclearingType, method);
                            int fieldIndex = fr.FieldIndex;
                            if (fieldIndex == 0)
                            {
                                fieldIndex = GetFieldIndex(ft, fr.Name);
                            }
                            else
                                fieldIndex |= 0x10000000;
                            opcode.TokenLong = ((long)ft.GetHashCode() << 32) | (uint)fieldIndex;
                        }
                        break;
                    case OpCodeEnum.Stsfld:
                    case OpCodeEnum.Ldsfld:
                    case OpCodeEnum.Ldsflda:
                        {
                            var fr = patchInfo.FieldReferences[(int)opcode.TokenLong];
                            var ft = domain.GetType(ResolveType(fr.DeclaringType, domain, null, fr.IsInternal), method.DeclearingType, method);
                            int fieldIndex = fr.FieldIndex;
                            if (fieldIndex == 0)
                            {
                                fieldIndex = GetFieldIndex(ft, fr.Name);
                            }
                            else
                                fieldIndex |= 0x10000000;
                            opcode.TokenLong = ((long)ft.GetHashCode() << 32) | (uint)fieldIndex;
                        }
                        break;
                    case OpCodeEnum.Ldstr:
                        {
                            var str = patchInfo.StringReferences[opcode.TokenLong];
                            opcode.TokenLong = domain.CacheString(str);
                        }
                        break;
                    case OpCodeEnum.Ldtoken:
                        {
                            switch (opcode.TokenInteger)
                            {
                                case 0:
                                    var fr = patchInfo.FieldReferences[(int)opcode.TokenLong];
                                    var ft = domain.GetType(ResolveType(fr.DeclaringType, domain, null, fr.IsInternal), method.DeclearingType, method);
                                    int fieldIndex = fr.FieldIndex;
                                    if (fieldIndex == 0)
                                    {
                                        fieldIndex = GetFieldIndex(ft, fr.Name);
                                    }
                                    else
                                        fieldIndex |= 0x10000000;
                                    opcode.TokenLong = ((long)ft.GetHashCode() << 32) | (uint)fieldIndex;
                                    break;
                                case 1:
                                    var it = domain.GetType(ResolveType(patchInfo.TypeReferences[(int)opcode.TokenLong], domain, null), method.DeclearingType, method);
                                    opcode.TokenLong = it.GetHashCode();
                                    break;
                                default:
                                    throw new Exception();
                            }
                        }
                        break;
                    case OpCodeEnum.Switch:
                        if (jumptables == null)
                            jumptables = new Dictionary<int, int[]>();
                        jumptables[opcode.TokenInteger] = patchInfo.JumpTables[opcode.TokenInteger];
                        break;
                    default:
                        break;
                }
                res[i] = opcode;
            }
            method.SetBodyAndJumptables(res, jumptables);
        }

        public bool CanApplyPatch(Assembly assembly)
        {
            if (IsPatched)
                return false;
            var type = assembly.GetType(PatchedTypeName);
            if (type == null)
                return false;
            var attr = type.GetCustomAttribute<PatchableAssemblyAttribute>(false);
            if (attr == null)
                return false;
            if (attr.Name != patchInfo.Name || attr.AssemblyHash != patchInfo.BaseHash)
                return false;
            return true;
        }

        public void PatchAssembly(Assembly assembly, Runtime.Enviorment.AppDomain domain)
        {
            if (IsPatched)
                throw new NotSupportedException("This patch is already applied");
            var type = assembly.GetType(PatchedTypeName);
            if (type == null)
                throw new NotSupportedException("The specified assembly is not patchable");
            var attr = type.GetCustomAttribute<PatchableAssemblyAttribute>(false);
            if (attr == null)
                throw new NotSupportedException("The specified assembly is not patchable");
            if (attr.Name != patchInfo.Name || attr.AssemblyHash != patchInfo.BaseHash)
                throw new NotSupportedException($"Patch not compatible, expected:{attr.Name}({attr.AssemblyHash}) got:{patchInfo.Name}({patchInfo.BaseHash})");
            domain.AddReferenceAssembly(assembly);
            MethodInfo getTypesMI = type.GetMethod(AssemblyInjector.PatchTypeGetPatchableTypesMethodName);
            Type[] types = getTypesMI.Invoke(null, null) as Type[];
            foreach (var i in types)
            {
                domain.GetType(i);
            }
            ((DefaultAssemblyResolver)module.AssemblyResolver).AddSearchDirectory(Path.GetDirectoryName(assembly.Location));
            InitializeTypes(domain);

            MethodInfo applyMI = type.GetMethod(AssemblyInjector.PatchApplyPatchMethodName);
            applyMI.Invoke(null, new[] { (object)this , domain});
        }

        public static AssemblyPatch LoadFromStream(Stream stream)
        {
            AssemblyPatch patch = new AssemblyPatch();
            patch.patchInfo = AssemblyPatchInfo.FromStream(new BinaryReader(stream));
            patch.module = ModuleDefinition.CreateModule("HotfixModule", ModuleKind.Dll);
            return patch;
        }
    }

    public class PatchableAssemblyAttribute : Attribute
    {
        public string Name { get; set; }
        public string AssemblyHash { get; set; }
    }

    public unsafe delegate StackObject* PatchGetFieldDelegate(int fieldIdx, ILIntepreter intp, StackObject* esp, IList<object> mStack);
    public unsafe delegate void PatchSetFieldDelegate(int fieldIdx, ILIntepreter intp, StackObject* esp, IList<object> mStack);

    public interface IPatchableType
    {
        ILTypeInstance ILRuntimeExtraFieldObject { get; }
        unsafe StackObject* ILRuntimeGetFieldValue(int fieldIdx, ILIntepreter intp, StackObject* esp, IList<object> mStack);
        unsafe void ILRuntimeSetFieldValue(int fieldIdx, ILIntepreter intp, StackObject* esp, IList<object> mStack);
    }

    public interface IPatchSettings
    {
        bool ShouldTypeIncludeInPatch(TypeDefinition type);
    }
}
