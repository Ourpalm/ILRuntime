using ILRuntime.Mono.Cecil;
using ILRuntime.Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Text;

namespace ILRuntime.Hybrid
{
    internal static class CecilExtensions
    {

        public static MethodReference GetMethod(this TypeReference type, string name)
        {
            foreach (var i in type.Resolve().Methods)
            {
                if (i.Name == name)
                    return i;
            }
            return null;
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
            var pt = param.ParameterType.GetElementType();
            if (pt == module.TypeSystem.Int32)
            {
                processor.AppendLoadArgument(paramIdx, first);
                AppendInstruction(processor, first, processor.Create(OpCodes.Ldloca, invokeCtx));
                processor.AppendLdc(first, refIdx);
                AppendInstruction(processor, first, processor.Create(OpCodes.Call, reflection.ReadInt32ByIndexMethod));
                AppendInstruction(processor, first, processor.Create(OpCodes.Stind_I4));
            }
            else if (pt == module.TypeSystem.String)
            {
                processor.AppendLoadArgument(paramIdx, first);
                AppendInstruction(processor, first, processor.Create(OpCodes.Ldloca, invokeCtx));
                processor.AppendLdc(first, refIdx);
                AppendInstruction(processor, first, processor.Create(OpCodes.Call, reflection.GetReadObjectByIndexMethod(typeof(string))));
                AppendInstruction(processor, first, processor.Create(OpCodes.Stind_Ref));
            }
        }

        public static void AppendReadReturnValue(this ILProcessor processor, ModuleDefinition module, TypeReference returnType, ReflectionReferences reflection, VariableDefinition invokeCtx, Instruction first)
        {
            if (returnType != module.TypeSystem.Void)
            {
                AppendInstruction(processor, first, processor.Create(OpCodes.Ldloca, invokeCtx));
                if (returnType == module.TypeSystem.Int32)
                {
                    AppendInstruction(processor, first, processor.Create(OpCodes.Call, reflection.ReadInt32Method));
                }
                else if (returnType == module.TypeSystem.String)
                {
                    AppendInstruction(processor, first, processor.Create(OpCodes.Call, reflection.GetReadObjectMethod(typeof(string))));
                }
            }
        }

        public static void AppendPushArgument(this ILProcessor processor, ModuleDefinition module, ReflectionReferences reflection, VariableDefinition invokeCtx, ParameterDefinition param, int paramIdx, Instruction first)
        {
            AppendInstruction(processor, first, processor.Create(OpCodes.Ldloca, invokeCtx));
            bool isByref = param.ParameterType.IsByReference;
            var pt = isByref ? param.ParameterType.GetElementType() : param.ParameterType;
            if (pt == module.TypeSystem.Int32)
            {
                AppendLoadArgument(processor, paramIdx, first);
                if (isByref)
                    AppendInstruction(processor, first, processor.Create(OpCodes.Ldind_I4));
                AppendInstruction(processor, first, processor.Create(OpCodes.Call, reflection.PushInt32Method));
            }
            else if (pt == module.TypeSystem.String)
            {
                AppendLoadArgument(processor, paramIdx, first);
                if (isByref)
                    AppendInstruction(processor, first, processor.Create(OpCodes.Ldind_Ref));
                AppendInstruction(processor, first, processor.Create(OpCodes.Ldc_I4_1));
                AppendInstruction(processor, first, processor.Create(OpCodes.Call, reflection.PushObjectMethod));
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
