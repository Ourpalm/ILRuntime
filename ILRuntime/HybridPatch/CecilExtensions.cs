using ILRuntime.Mono.Cecil;
using ILRuntime.Mono.Cecil.Cil;
using ILRuntime.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace ILRuntime.Hybrid
{
    internal static class CecilExtensions
    {
        public static bool CheckTypeReferenceEqual(this TypeReference a, TypeReference b)
        {
            if(a == b) return true;
            if (a is TypeSpecification specA)
            {
                if (b is TypeSpecification specB)
                {
                    if (a is ByReferenceType)
                    {
                        return b is ByReferenceType && CheckTypeReferenceEqual(a.GetElementType(), b.GetElementType());
                    }
                    else if (a is ArrayType)
                    {
                        return b is ArrayType && CheckTypeReferenceEqual(a.GetElementType(), b.GetElementType());
                    }
                    else if (a is GenericInstanceType gitA)
                    {
                        if (b is GenericInstanceType gitB)
                        {
                            if (!CheckTypeReferenceEqual(gitA.ElementType, gitB.ElementType))
                                return false;
                            if ((gitA.GenericArguments.Count != gitB.GenericArguments.Count))
                                return false;
                            bool isMatch = true;
                            for (int i = 0; i < gitA.GenericArguments.Count; i++)
                            {
                                if (!CheckTypeReferenceEqual(gitA.GenericArguments[i], gitB.GenericArguments[i]))
                                {
                                    isMatch = false;
                                    break;
                                }
                            }
                            return isMatch;
                        }
                        else
                            return false;
                    }
                    else
                        throw new NotImplementedException();
                }
                else
                    return false;
            }
            else
            {
                if (b is TypeSpecification)
                    return false;
                return a.FullName == b.FullName;
            }
        }

        public static bool IsDelegate(this TypeReference typeRef)
        {
            if (typeRef == null)
            {
                return false;
            }

            // 1. 尝试解析 TypeReference 获取 TypeDefinition
            // TypeDefinition 包含了类型的完整信息，包括基类等。
            TypeDefinition typeDef = typeRef.Resolve();

            if (typeDef != null)
            {
                if (!typeDef.IsClass) // 委托首先是类
                {
                    return false;
                }
                TypeReference baseType = typeDef.BaseType;
                while (baseType != null)
                {
                    if (baseType.FullName == "System.MulticastDelegate")
                    {
                        return true;
                    }
                    if (baseType.FullName == "System.Delegate")
                    {
                        return true;
                    }
                    TypeDefinition resolvedBase = baseType.Resolve();
                    if (resolvedBase == null) break; // 无法继续解析基类链
                    baseType = resolvedBase.BaseType;
                }
                return false;
            }
            else
            {
                if (typeRef.FullName == "System.Delegate" || typeRef.FullName == "System.MulticastDelegate")
                {
                    return true;
                }
                return false;
            }
        }
        public static void FixClosureNameConsistency(this ModuleDefinition module, IPatchSettings settings)
        {
            MD5 md5 = MD5.Create();
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            if (module.HasTypes)
            {
                foreach (var i in module.Types)
                {
                    FixClosureNameConsistency(i, false, settings, bw, md5);
                }
            }
        }

        static void FixClosureNameConsistency(TypeDefinition type, bool forceInclude, IPatchSettings settings, BinaryWriter bw, MD5 md5)
        {
            bool shouldIncludeBySetting = settings != null ? settings.ShouldTypeIncludeInPatch(type) : false;
            bool isIgnore = false;
            bool shouldInclude = type.ShouldIncludeInPatch(out isIgnore) || forceInclude || shouldIncludeBySetting;
            if (shouldInclude && !type.IsDelegate() && !type.IsInterface && !isIgnore)
            {
                if (type.CheckHasClosureType())
                {
                    var nameMapping = type.BuildClosureNameMapping(bw, md5);
                    foreach (var i in type.NestedTypes)
                    {
                        if (i.IsClosureType())
                        {
                            if (nameMapping.TryGetValue(i, out var hash))
                            {
                                if (i.Name.Contains("<>c__DisplayClass"))
                                {
                                    i.Name = GetClosureAliasName(i.Name, "<>c__DisplayClass", hash);
                                }
                                else if(i.Name == "<>c")
                                {
                                    FixClosureAliasNameNoCapture(i, nameMapping);
                                }
                                else
                                {
                                    i.Name = GetClosureAliasName(i.Name, "__", hash);
                                }
                            }
                            else if (i.Name == "<>c")
                            {
                                FixClosureAliasNameNoCapture(i, nameMapping);
                            }
                            else
                                throw new NotImplementedException();
                        }
                    }
                }
            }
            if (type.HasNestedTypes)
            {
                foreach (var i in type.NestedTypes)
                {
                    if (!i.IsClosureType())
                        FixClosureNameConsistency(i, shouldInclude, settings, bw, md5);
                }
            }
        }

        static void FixClosureAliasNameNoCapture(TypeDefinition type, Dictionary<MemberReference, string> nameMapping)
        {
            string hash;
            foreach (var f in type.Fields)
            {
                if (nameMapping.TryGetValue(f, out hash) && f.Name.Contains("__"))
                {
                    f.Name = GetClosureAliasName(f.Name, "__", hash);
                }
            }
            foreach (var m in type.Methods)
            {
                if (nameMapping.TryGetValue(m, out hash) && m.Name.Contains("__"))
                {
                    m.Name = GetClosureAliasName(m.Name, "__", hash);
                }
            }
        }

        static bool CheckHasClosureType(this TypeDefinition type)
        {
            if (!type.IsClosureType() && type.HasNestedTypes)
            {
                foreach (var i in type.NestedTypes)
                {
                    if (i.IsClosureType())
                        return true;
                    else if (i.CheckHasClosureType())
                        return true;
                }
                return false;
            }
            else
                return false;
        }

        static Dictionary<MemberReference, string> BuildClosureNameMapping(this TypeDefinition type, BinaryWriter bw, MD5 md5, string hash = null, Dictionary<MemberReference, string> res = null)
        {
            if (res == null)
                res = new Dictionary<MemberReference, string>();
            foreach (var i in type.Methods)
            {
                BuildClosureNameMapping(res, i, bw, md5, hash);
            }
            foreach(var i in type.NestedTypes)
            {
                if (i.IsClosureType())
                {
                    foreach (var j in i.Methods)
                    {
                        if(res.TryGetValue(i, out hash))
                        {
                            BuildClosureNameMapping(res, j, bw, md5, hash);
                        }
                    }
                }
            }
            return res;
        }

        static void BuildClosureNameMapping(Dictionary<MemberReference, string> res, MethodDefinition method, BinaryWriter bw, MD5 md5, string hash = null)
        {
            if (method.HasBody)
            {
                if (string.IsNullOrEmpty(hash))
                    hash = method.ComputeHash(bw, md5, false).Substring(0, 6);

                CustomAttribute attr;
                if (method.IsAsyncMethod(out attr) || method.IsIteratorMethod(out attr))
                {
                    TypeReference tr = (TypeReference)attr.ConstructorArguments[0].Value;
                    var td = tr.Resolve();
                    if (td.IsClosureType())
                    {
                        res[td] = hash;
                        BuildClosureNameMapping(td, bw, md5, hash, res);
                    }
                }
                foreach (var ins in method.Body.Instructions)
                {
                    if ((ins.OpCode.Code == Code.Newobj || ins.OpCode.Code == Code.Ldftn) && ins.Operand is MethodReference mr)
                    {
                        var dt = mr.DeclaringType.Resolve();
                        if (dt.IsClosureType())
                        {
                            res[dt] = hash;
                            res[mr.Resolve()] = hash;
                        }
                    }
                    else if (ins.OpCode.Code == Code.Ldsfld && ins.Operand is FieldReference fr)
                    {
                        var dt = fr.DeclaringType.Resolve();
                        if (dt.IsClosureType())
                        {
                            res[dt] = hash;
                            res[fr.Resolve()] = hash;
                        }
                    }
                }
            }
            
        }

        static string attributeName = typeof(ILRuntimePatchAttribute).FullName;
        static string ignoreAttributeName = typeof(ILRuntimePatchIgnore).FullName;
        static string asyncAttributeName = typeof(AsyncStateMachineAttribute).Name;
        static string enumerableAttributeName = typeof(IteratorStateMachineAttribute).Name;

        public static bool ShouldIncludeInPatch(this TypeDefinition type, out bool isIgnore)
        {
            bool shouldInclude = false;
            isIgnore = false;
            if(type.HasCustomAttributes)
            {
                foreach (var attr in type.CustomAttributes)
                {
                    if (attr.AttributeType.FullName == ignoreAttributeName)
                        isIgnore = true;
                    if (attr.AttributeType.FullName == attributeName)
                    {
                        shouldInclude = true;
                    }
                }
            }
            return shouldInclude;
        }

        public static bool ShouldIncludeInPatch(this MethodDefinition method)
        {
            if (method.HasCustomAttributes)
            {
                foreach (var attr in method.CustomAttributes)
                {
                    if (attr.AttributeType.FullName == ignoreAttributeName)
                        return false;
                }
            }
            return true;
        }

        public static string GetClosureAliasName(string name, string identifier, string hash)
        {
            int startIdx = name.IndexOf(identifier);
            string heading = name.Substring(0, startIdx);
            name = name.Substring(startIdx);
            string trailing = name.Replace(identifier, "");
            string[] token = trailing.Split('_');
            if (token.Length > 1)
            {
                int mIdx = int.Parse(token[0]);
                //mi.ComputeHash(bw, md5, false).Substring(0, 6)
                return $"{heading}{identifier}{hash}_{token[1]}";
            }
            else
            {
                if (token[0].Contains("`"))
                    return $"{heading}{identifier}{hash}{token[0].Substring(token[0].IndexOf("`"))}";
                else
                    return $"{heading}{identifier}{hash}";
            }
        }
        public static bool IsClosureType(this TypeDefinition type)
        {
            if (type.IsNested)
            {
                if (type.Name.StartsWith("<"))
                {
                    if (type.HasCustomAttributes)
                    {
                        foreach(var i in type.CustomAttributes)
                        {
                            if (i.AttributeType.Name == "CompilerGeneratedAttribute")
                                return true;
                        }
                    }
                    return false;
                }
                else
                    return false;
            }
            else
                return false;
        }

        public static bool IsAsyncMethod(this MethodDefinition method, out CustomAttribute attr)
        {
            if (method.HasCustomAttributes)
            {
                foreach (var i in method.CustomAttributes)
                {
                    if (i.AttributeType.Name == asyncAttributeName)
                    {
                        attr = i;
                        return true;
                    }
                }
            }
            attr = null;
            return false;
        }
        public static bool IsIteratorMethod(this MethodDefinition method, out CustomAttribute attr)
        {
            if (method.HasCustomAttributes)
            {
                foreach (var i in method.CustomAttributes)
                {
                    if (i.AttributeType.Name == enumerableAttributeName)
                    {
                        attr = i;
                        return true;
                    }
                }
            }
            attr = null;
            return false;
        }
        public static void GetTypeNames(this TypeDefinition type, out string namesp, out string name)
        {
            if (type.IsNested)
            {
                var dt = type.DeclaringType;
                GetTypeNames(dt, out namesp, out var tn);
                name = tn + "+" + type.Name;
            }
            else
            {
                namesp = type.Namespace;
                name = type.Name;
            }
        }
        public static string GetSafeFullNames(this TypeReference type)
        {
            type.Resolve().GetTypeNames(out var namesp, out var name);
            if (!string.IsNullOrEmpty(namesp))
                return $"{namesp}.{name}";
            else
                return name;
        }
        public static MethodReference GetMethod(this TypeReference type, string name)
        {
            foreach (var i in type.Resolve().Methods)
            {
                if (i.Name == name)
                    return i;
            }
            return null;
        }
        public static GenericInstanceMethod MakeGenericInstanceMethod(this MethodReference self, params TypeReference[] arguments)
        {
            if (self == null)
                throw new ArgumentNullException("self");
            if (arguments == null)
                throw new ArgumentNullException("arguments");
            if (arguments.Length == 0)
                throw new ArgumentException();
            if (self.GenericParameters.Count != arguments.Length)
                throw new ArgumentException();

            var instance = new GenericInstanceMethod(self);

            foreach (var argument in arguments)
                instance.GenericArguments.Add(argument);

            return instance;
        }

        public static GenericInstanceType MakeGenericInstanceType(this TypeReference self, params TypeReference[] arguments)
        {
            if (self == null)
                throw new ArgumentNullException("self");
            if (arguments == null)
                throw new ArgumentNullException("arguments");
            if (arguments.Length == 0)
                throw new ArgumentException();
            if (self.GenericParameters.Count != arguments.Length)
                throw new ArgumentException();

            var instance = new GenericInstanceType(self);

            foreach (var argument in arguments)
                instance.GenericArguments.Add(argument);

            return instance;
        }

        public static int FindGenericArgument(this GenericInstanceType self, string name)
        {
            var def = self.Resolve();
            for(int i = 0; i < def.GenericParameters.Count; i++)
            {
                if (def.GenericParameters[i].Name == name)
                    return i;
            }
            return -1;
        }

        public static MethodReference GetMethod(this GenericInstanceType self, MethodReference method)
        {
            if (self.Resolve() != method.DeclaringType)
                throw new NotSupportedException();
            var result = new MethodReference(method.Name, method.ReturnType)
            {
                DeclaringType = self,
                HasThis = method.HasThis,
                ExplicitThis = method.ExplicitThis,
                CallingConvention = method.CallingConvention,
            };
            for (int i = 0; i < method.Parameters.Count; i++)
            {
                var p = method.Parameters[i];
                TypeReference tr = p.ParameterType;
                result.Parameters.Add(new ParameterDefinition(p.Name, p.Attributes, tr));
            }
            return result;
        }

        public static void AppendLoadArgument(this ILProcessor processor, int paramIdx, Instruction first)
        {
            switch (paramIdx)
            {
                case 0:
                    AppendInstruction(processor, first, processor.Create(OpCodes.Ldarg_0));
                    break;
                case 1:
                    AppendInstruction(processor, first, processor.Create(OpCodes.Ldarg_1));
                    break;
                case 2:
                    AppendInstruction(processor, first, processor.Create(OpCodes.Ldarg_2));
                    break;
                case 3:
                    AppendInstruction(processor, first, processor.Create(OpCodes.Ldarg_3));
                    break;
                default:
                    AppendInstruction(processor, first, processor.Create(OpCodes.Ldarg, paramIdx));
                    break;
            }
        }

        public static void AppendStoreArgument(this ILProcessor processor, ModuleDefinition module, ReflectionReferences reflection, VariableDefinition invokeCtx, ParameterDefinition param,int refIdx, int paramIdx, Instruction first)
        {
            var pt = ((TypeSpecification)param.ParameterType).ElementType;
            if (pt == module.TypeSystem.Int32 || pt == module.TypeSystem.Char || pt == module.TypeSystem.Boolean)
            {
                processor.AppendLoadArgument(paramIdx, first);
                AppendInstruction(processor, first, processor.Create(OpCodes.Ldloca, invokeCtx));
                processor.AppendLdc(first, refIdx);
                AppendInstruction(processor, first, processor.Create(OpCodes.Call, reflection.ReadInt32ByIndexMethod));
                if (pt == module.TypeSystem.Boolean)
                {
                    AppendInstruction(processor, first, processor.Create(OpCodes.Ldc_I4_1));
                    AppendInstruction(processor, first, processor.Create(OpCodes.Ceq));
                    AppendInstruction(processor, first, processor.Create(OpCodes.Stind_I1));
                }
                else
                    AppendInstruction(processor, first, processor.Create(OpCodes.Stind_I4));
            }
            else if (pt == module.TypeSystem.UInt32)
            {
                processor.AppendLoadArgument(paramIdx, first);
                AppendInstruction(processor, first, processor.Create(OpCodes.Ldloca, invokeCtx));
                processor.AppendLdc(first, refIdx);
                AppendInstruction(processor, first, processor.Create(OpCodes.Call, reflection.ReadInt32ByIndexMethod));
                AppendInstruction(processor, first, processor.Create(OpCodes.Stind_I4));
            }
            else if (pt == module.TypeSystem.Int16)
            {
                processor.AppendLoadArgument(paramIdx, first);
                AppendInstruction(processor, first, processor.Create(OpCodes.Ldloca, invokeCtx));
                processor.AppendLdc(first, refIdx);
                AppendInstruction(processor, first, processor.Create(OpCodes.Call, reflection.ReadInt32ByIndexMethod));
                AppendInstruction(processor, first, processor.Create(OpCodes.Stind_I2));
            }
            else if (pt == module.TypeSystem.UInt16)
            {
                processor.AppendLoadArgument(paramIdx, first);
                AppendInstruction(processor, first, processor.Create(OpCodes.Ldloca, invokeCtx));
                processor.AppendLdc(first, refIdx);
                AppendInstruction(processor, first, processor.Create(OpCodes.Call, reflection.ReadInt32ByIndexMethod));
                AppendInstruction(processor, first, processor.Create(OpCodes.Stind_I2));
            }
            else if (pt == module.TypeSystem.Byte)
            {
                processor.AppendLoadArgument(paramIdx, first);
                AppendInstruction(processor, first, processor.Create(OpCodes.Ldloca, invokeCtx));
                processor.AppendLdc(first, refIdx);
                AppendInstruction(processor, first, processor.Create(OpCodes.Call, reflection.ReadInt32ByIndexMethod));
                AppendInstruction(processor, first, processor.Create(OpCodes.Stind_I1));
            }
            else if (pt == module.TypeSystem.SByte)
            {
                processor.AppendLoadArgument(paramIdx, first);
                AppendInstruction(processor, first, processor.Create(OpCodes.Ldloca, invokeCtx));
                processor.AppendLdc(first, refIdx);
                AppendInstruction(processor, first, processor.Create(OpCodes.Call, reflection.ReadInt32ByIndexMethod));
                AppendInstruction(processor, first, processor.Create(OpCodes.Stind_I1));
            }
            else if (pt == module.TypeSystem.Int64)
            {
                processor.AppendLoadArgument(paramIdx, first);
                AppendInstruction(processor, first, processor.Create(OpCodes.Ldloca, invokeCtx));
                processor.AppendLdc(first, refIdx);
                AppendInstruction(processor, first, processor.Create(OpCodes.Call, reflection.ReadInt64ByIndexMethod));
                AppendInstruction(processor, first, processor.Create(OpCodes.Stind_I8));
            }
            else if (pt == module.TypeSystem.UInt64)
            {
                processor.AppendLoadArgument(paramIdx, first);
                AppendInstruction(processor, first, processor.Create(OpCodes.Ldloca, invokeCtx));
                processor.AppendLdc(first, refIdx);
                AppendInstruction(processor, first, processor.Create(OpCodes.Call, reflection.ReadInt64ByIndexMethod));
                AppendInstruction(processor, first, processor.Create(OpCodes.Stind_I8));
            }
            else if (pt == module.TypeSystem.IntPtr)
            {
                processor.AppendLoadArgument(paramIdx, first);
                AppendInstruction(processor, first, processor.Create(OpCodes.Ldloca, invokeCtx));
                processor.AppendLdc(first, refIdx);
                AppendInstruction(processor, first, processor.Create(OpCodes.Call, reflection.ReadInt64ByIndexMethod));
                AppendInstruction(processor, first, processor.Create(OpCodes.Stind_I));
            }
            else if (pt == module.TypeSystem.Single)
            {
                processor.AppendLoadArgument(paramIdx, first);
                AppendInstruction(processor, first, processor.Create(OpCodes.Ldloca, invokeCtx));
                processor.AppendLdc(first, refIdx);
                AppendInstruction(processor, first, processor.Create(OpCodes.Call, reflection.ReadFloatByIndexMethod));
                AppendInstruction(processor, first, processor.Create(OpCodes.Stind_R4));
            }
            else if (pt == module.TypeSystem.Double)
            {
                processor.AppendLoadArgument(paramIdx, first);
                AppendInstruction(processor, first, processor.Create(OpCodes.Ldloca, invokeCtx));
                processor.AppendLdc(first, refIdx);
                AppendInstruction(processor, first, processor.Create(OpCodes.Call, reflection.ReadDoubleByIndexMethod));
                AppendInstruction(processor, first, processor.Create(OpCodes.Stind_R8));
            }
            else if (!pt.ContainsGenericParameter && pt.Resolve().IsEnum)
            {
                processor.AppendLoadArgument(paramIdx, first);
                AppendInstruction(processor, first, processor.Create(OpCodes.Ldloca, invokeCtx));
                processor.AppendLdc(first, refIdx);
                AppendInstruction(processor, first, processor.Create(OpCodes.Call, reflection.ReadInt32ByIndexMethod));
                AppendInstruction(processor, first, processor.Create(OpCodes.Stind_I4));
            }
            else
            {
                processor.AppendLoadArgument(paramIdx, first);
                AppendInstruction(processor, first, processor.Create(OpCodes.Ldloca, invokeCtx));
                processor.AppendLdc(first, refIdx);
                if (pt.ContainsGenericParameter || pt.IsValueType)
                    AppendInstruction(processor, first, processor.Create(OpCodes.Call, reflection.GetReadResultByIndexMethod(pt)));
                else
                    AppendInstruction(processor, first, processor.Create(OpCodes.Call, reflection.GetReadObjectByIndexMethod(pt)));
                if (pt.IsValueType)
                    AppendInstruction(processor, first, processor.Create(OpCodes.Stobj, pt));
                else
                    AppendInstruction(processor, first, processor.Create(OpCodes.Stind_Ref));
            }
        }

        public static void AppendReadReturnValue(this ILProcessor processor, ModuleDefinition module, TypeReference returnType, ReflectionReferences reflection, VariableDefinition invokeCtx, Instruction first)
        {
            if (returnType != module.TypeSystem.Void)
            {
                AppendInstruction(processor, first, processor.Create(OpCodes.Ldloca, invokeCtx));
                if (returnType == module.TypeSystem.Int32 || returnType == module.TypeSystem.Char || returnType == module.TypeSystem.Boolean)
                {
                    AppendInstruction(processor, first, processor.Create(OpCodes.Call, reflection.ReadInt32Method));
                    if (returnType == module.TypeSystem.Boolean)
                    {
                        AppendInstruction(processor, first, processor.Create(OpCodes.Ldc_I4_1));
                        AppendInstruction(processor, first, processor.Create(OpCodes.Ceq));
                    }
                }
                else if (returnType == module.TypeSystem.UInt32)
                {
                    AppendInstruction(processor, first, processor.Create(OpCodes.Call, reflection.ReadInt32Method));
                    AppendInstruction(processor, first, processor.Create(OpCodes.Conv_U4));
                }
                else if (returnType == module.TypeSystem.Int16)
                {
                    AppendInstruction(processor, first, processor.Create(OpCodes.Call, reflection.ReadInt32Method));
                    AppendInstruction(processor, first, processor.Create(OpCodes.Conv_I2));
                }
                else if (returnType == module.TypeSystem.UInt16)
                {
                    AppendInstruction(processor, first, processor.Create(OpCodes.Call, reflection.ReadInt32Method));
                    AppendInstruction(processor, first, processor.Create(OpCodes.Conv_U2));
                }
                else if (returnType == module.TypeSystem.Byte)
                {
                    AppendInstruction(processor, first, processor.Create(OpCodes.Call, reflection.ReadInt32Method));
                    AppendInstruction(processor, first, processor.Create(OpCodes.Conv_U1));
                }
                else if (returnType == module.TypeSystem.SByte)
                {
                    AppendInstruction(processor, first, processor.Create(OpCodes.Call, reflection.ReadInt32Method));
                    AppendInstruction(processor, first, processor.Create(OpCodes.Conv_I1));
                }
                else if (returnType == module.TypeSystem.Int64)
                {
                    AppendInstruction(processor, first, processor.Create(OpCodes.Call, reflection.ReadInt64Method));
                }
                else if (returnType == module.TypeSystem.UInt64)
                {
                    AppendInstruction(processor, first, processor.Create(OpCodes.Call, reflection.ReadInt64Method));
                    AppendInstruction(processor, first, processor.Create(OpCodes.Conv_U8));
                }
                else if (returnType == module.TypeSystem.IntPtr)
                {
                    AppendInstruction(processor, first, processor.Create(OpCodes.Call, reflection.ReadInt64Method));
                    AppendInstruction(processor, first, processor.Create(OpCodes.Conv_I));
                }
                else if (returnType == module.TypeSystem.Single)
                {
                    AppendInstruction(processor, first, processor.Create(OpCodes.Call, reflection.ReadFloatMethod));
                }
                else if (returnType == module.TypeSystem.Double)
                {
                    AppendInstruction(processor, first, processor.Create(OpCodes.Call, reflection.ReadDoubleMethod));
                }
                else if (!returnType.ContainsGenericParameter && returnType.Resolve().IsEnum)
                {
                    AppendInstruction(processor, first, processor.Create(OpCodes.Call, reflection.ReadInt32Method));
                }
                else
                {
                    if(returnType.ContainsGenericParameter || returnType.IsValueType)
                        AppendInstruction(processor, first, processor.Create(OpCodes.Call, reflection.GetReadResultMethod(returnType)));
                    else
                        AppendInstruction(processor, first, processor.Create(OpCodes.Call, reflection.GetReadObjectMethod(returnType)));
                }
            }
        }

        public static void AppendPushArgument(this ILProcessor processor, ModuleDefinition module, ReflectionReferences reflection, VariableDefinition invokeCtx, ParameterDefinition param, int paramIdx, Instruction first)
        {
            AppendInstruction(processor, first, processor.Create(OpCodes.Ldloca, invokeCtx));
            bool isByref = param.ParameterType.IsByReference;
            var pt = isByref ? ((TypeSpecification)param.ParameterType).ElementType : param.ParameterType;
            if (pt == module.TypeSystem.Int32 || pt == module.TypeSystem.Char)
            {
                AppendLoadArgument(processor, paramIdx, first);
                if (isByref)
                    AppendInstruction(processor, first, processor.Create(OpCodes.Ldind_I4));
                AppendInstruction(processor, first, processor.Create(OpCodes.Call, reflection.PushInt32Method));
            }
            else if (pt == module.TypeSystem.UInt32)
            {
                AppendLoadArgument(processor, paramIdx, first);
                if (isByref)
                    AppendInstruction(processor, first, processor.Create(OpCodes.Ldind_U4));
                AppendInstruction(processor, first, processor.Create(OpCodes.Conv_I4));
                AppendInstruction(processor, first, processor.Create(OpCodes.Call, reflection.PushInt32Method));
            }
            else if (pt == module.TypeSystem.Boolean)
            {
                AppendLoadArgument(processor, paramIdx, first);
                if (isByref)
                    AppendInstruction(processor, first, processor.Create(OpCodes.Ldind_U1));
                AppendInstruction(processor, first, processor.Create(OpCodes.Call, reflection.PushBoolMethod));
            }
            else if (pt == module.TypeSystem.Int16)
            {
                AppendLoadArgument(processor, paramIdx, first);
                if (isByref)
                    AppendInstruction(processor, first, processor.Create(OpCodes.Ldind_I2));
                AppendInstruction(processor, first, processor.Create(OpCodes.Conv_I4));
                AppendInstruction(processor, first, processor.Create(OpCodes.Call, reflection.PushInt32Method));
            }
            else if (pt == module.TypeSystem.UInt16)
            {
                AppendLoadArgument(processor, paramIdx, first);
                if (isByref)
                    AppendInstruction(processor, first, processor.Create(OpCodes.Ldind_U2));
                AppendInstruction(processor, first, processor.Create(OpCodes.Conv_I4));
                AppendInstruction(processor, first, processor.Create(OpCodes.Call, reflection.PushInt32Method));
            }
            else if (pt == module.TypeSystem.Byte)
            {
                AppendLoadArgument(processor, paramIdx, first);
                if (isByref)
                    AppendInstruction(processor, first, processor.Create(OpCodes.Ldind_U1));
                AppendInstruction(processor, first, processor.Create(OpCodes.Conv_I4));
                AppendInstruction(processor, first, processor.Create(OpCodes.Call, reflection.PushInt32Method));
            }
            else if (pt == module.TypeSystem.SByte)
            {
                AppendLoadArgument(processor, paramIdx, first);
                if (isByref)
                    AppendInstruction(processor, first, processor.Create(OpCodes.Ldind_I1));
                AppendInstruction(processor, first, processor.Create(OpCodes.Conv_I4));
                AppendInstruction(processor, first, processor.Create(OpCodes.Call, reflection.PushInt32Method));
            }
            else if (pt == module.TypeSystem.Int64)
            {
                AppendLoadArgument(processor, paramIdx, first);
                if (isByref)
                    AppendInstruction(processor, first, processor.Create(OpCodes.Ldind_I8));
                AppendInstruction(processor, first, processor.Create(OpCodes.Call, reflection.PushInt64Method));
            }
            else if (pt == module.TypeSystem.UInt64)
            {
                AppendLoadArgument(processor, paramIdx, first);
                if (isByref)
                {
                    AppendInstruction(processor, first, processor.Create(OpCodes.Ldind_I8));
                }
                else
                    AppendInstruction(processor, first, processor.Create(OpCodes.Conv_I8));
                AppendInstruction(processor, first, processor.Create(OpCodes.Call, reflection.PushInt64Method));
            }
            else if (pt == module.TypeSystem.IntPtr)
            {
                AppendLoadArgument(processor, paramIdx, first);
                if (isByref)
                {
                    AppendInstruction(processor, first, processor.Create(OpCodes.Ldind_I));
                }
                AppendInstruction(processor, first, processor.Create(OpCodes.Conv_I8));
                AppendInstruction(processor, first, processor.Create(OpCodes.Call, reflection.PushInt64Method));
            }
            else if (pt == module.TypeSystem.Single)
            {
                AppendLoadArgument(processor, paramIdx, first);
                if (isByref)
                    AppendInstruction(processor, first, processor.Create(OpCodes.Ldind_R4));
                AppendInstruction(processor, first, processor.Create(OpCodes.Call, reflection.PushFloatMethod));
            }
            else if (pt == module.TypeSystem.Double)
            {
                AppendLoadArgument(processor, paramIdx, first);
                if (isByref)
                    AppendInstruction(processor, first, processor.Create(OpCodes.Ldind_R8));
                AppendInstruction(processor, first, processor.Create(OpCodes.Call, reflection.PushDoubleMethod));
            }
            else if(!pt.ContainsGenericParameter && pt.Resolve().IsEnum)
            {
                AppendLoadArgument(processor, paramIdx, first);
                if (isByref)
                    AppendInstruction(processor, first, processor.Create(OpCodes.Ldind_I4));
                AppendInstruction(processor, first, processor.Create(OpCodes.Call, reflection.PushInt32Method));
            }
            else
            {
                AppendLoadArgument(processor, paramIdx, first);
                if (isByref)
                {
                    if (pt.IsValueType)
                        AppendInstruction(processor, first, processor.Create(OpCodes.Ldobj, pt));
                    else
                        AppendInstruction(processor, first, processor.Create(OpCodes.Ldind_Ref));
                }
                if (pt.ContainsGenericParameter || pt.IsValueType)
                {
                    AppendInstruction(processor, first, processor.Create(OpCodes.Call, reflection.GetPushParameterMethod(pt)));
                }
                else
                {
                    AppendInstruction(processor, first, processor.Create(OpCodes.Ldc_I4_1));
                    AppendInstruction(processor, first, processor.Create(OpCodes.Call, reflection.PushObjectMethod));
                }
            }
        }

        public static void AppendLdc(this ILProcessor processor, Instruction first, int val)
        {
            switch (val)
            {
                case 0:
                    AppendInstruction(processor, first, processor.Create(OpCodes.Ldc_I4_0));
                    break;
                case 1:
                    AppendInstruction(processor, first, processor.Create(OpCodes.Ldc_I4_1));
                    break;
                case 2:
                    AppendInstruction(processor, first, processor.Create(OpCodes.Ldc_I4_2));
                    break;
                case 3:
                    AppendInstruction(processor, first, processor.Create(OpCodes.Ldc_I4_3));
                    break;
                case 4:
                    AppendInstruction(processor, first, processor.Create(OpCodes.Ldc_I4_4));
                    break;
                case 5:
                    AppendInstruction(processor, first, processor.Create(OpCodes.Ldc_I4_5));
                    break;
                case 6:
                    AppendInstruction(processor, first, processor.Create(OpCodes.Ldc_I4_6));
                    break;
                case 7:
                    AppendInstruction(processor, first, processor.Create(OpCodes.Ldc_I4_7));
                    break;
                case 8:
                    AppendInstruction(processor, first, processor.Create(OpCodes.Ldc_I4_8));
                    break;
                case -1:
                    AppendInstruction(processor, first, processor.Create(OpCodes.Ldc_I4_M1));
                    break;
                default:
                    if (val < 128 && val >= -128)
                        AppendInstruction(processor, first, processor.Create(OpCodes.Ldc_I4_S, val));
                    else
                        AppendInstruction(processor, first, processor.Create(OpCodes.Ldc_I4, val));
                    break;
            }
        }

        public static void AppendInstruction(this ILProcessor processor, Instruction before, Instruction instruction)
        {
            if (before != null)
                processor.InsertBefore(before, instruction);
            else
                processor.Append(instruction);
        }
    }
}
