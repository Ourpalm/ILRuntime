#if ENABLE_NEO_MODE
using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Mono.Cecil;
using ILRuntime.Runtime.Intepreter.OpCodes;
using System;

namespace ILRuntime.Runtime.Intepreter.RegisterVM
{
    partial struct JITCompiler
    {
        IType[] BuildInitialRegisterTypes(short locVarRegStart, int totalRegCnt)
        {
            IType[] registerTypes = new IType[totalRegCnt];
            int idx = 0;
            if (method.HasThis)
                registerTypes[idx++] = declaringType.IsValueType
                    ? declaringType.MakeByRefType()
                    : declaringType;
            for (int i = 0; i < method.ParameterCount && idx < registerTypes.Length; i++, idx++)
            {
                registerTypes[idx] = appdomain.GetType(def.Parameters[i].ParameterType, declaringType, method);
            }
            for (int i = 0; i < def.Body.Variables.Count; i++)
            {
                int reg = locVarRegStart + i;
                if (reg < registerTypes.Length)
                    registerTypes[reg] = appdomain.GetType(def.Body.Variables[i].VariableType, declaringType, method);
            }
            return registerTypes;
        }


        void EncodeNeoStaticField(ref OpCodeR op, object token)
        {
            var offset = appdomain.GetStaticFieldOffset(token, declaringType, method, out IType type, out IType fieldType);
            op.Operand = method.GetTypeTokenHashCode(((FieldReference)token).FieldType);
            op.Operand2 = type.GetHashCode();
            if (type is ILType)
                op.Operand3 = ((offset.ReferenceOffset & 0xFFFF) << 16) | (offset.PrimitiveOffset & 0xFFFF);
            else
                op.Operand3 = offset.PrimitiveOffset;

            NeoStaticFieldKind kind;
            if (fieldType.IsPrimitive || fieldType.IsEnum)
            {
                if (fieldType == appdomain.BoolType) kind = NeoStaticFieldKind.Boolean;
                else if (fieldType == appdomain.SByteType) kind = NeoStaticFieldKind.I1;
                else if (fieldType == appdomain.ByteType) kind = NeoStaticFieldKind.U1;
                else if (fieldType == appdomain.ShortType) kind = NeoStaticFieldKind.I2;
                else if (fieldType == appdomain.UShortType || fieldType == appdomain.CharType) kind = NeoStaticFieldKind.U2;
                else if (fieldType == appdomain.IntType) kind = NeoStaticFieldKind.I4;
                else if (fieldType == appdomain.UIntType) kind = NeoStaticFieldKind.U4;
                else if (fieldType == appdomain.LongType || fieldType == appdomain.IntPtrType) kind = NeoStaticFieldKind.I8;
                else if (fieldType == appdomain.ULongType) kind = NeoStaticFieldKind.U8;
                else if (fieldType == appdomain.FloatType) kind = NeoStaticFieldKind.R4;
                else if (fieldType == appdomain.DoubleType) kind = NeoStaticFieldKind.R8;
                else
                {
                    Type clr = fieldType.TypeForCLR;
                    if (clr != null && clr.IsEnum)
                    {
                        clr = Enum.GetUnderlyingType(clr);
                        switch (Type.GetTypeCode(clr))
                        {
                            case TypeCode.SByte: kind = NeoStaticFieldKind.I1; break;
                            case TypeCode.Byte: kind = NeoStaticFieldKind.U1; break;
                            case TypeCode.Int16: kind = NeoStaticFieldKind.I2; break;
                            case TypeCode.UInt16: kind = NeoStaticFieldKind.U2; break;
                            case TypeCode.Int32: kind = NeoStaticFieldKind.I4; break;
                            case TypeCode.UInt32: kind = NeoStaticFieldKind.U4; break;
                            case TypeCode.Int64: kind = NeoStaticFieldKind.I8; break;
                            case TypeCode.UInt64: kind = NeoStaticFieldKind.U8; break;
                            default: throw new NotSupportedException("Neo static enum underlying type is unsupported: " + clr);
                        }
                    }
                    else
                        throw new NotSupportedException("Neo static primitive type is unsupported: " + fieldType);
                }
            }
            else if (fieldType is ILType valueType && fieldType.IsValueType)
            {
                kind = NeoStaticFieldKind.Value;
                op.Operand4 = ((valueType.TotalPrimitiveSize & 0xFFFF) << 16) |
                    ((valueType.TotalReferenceCount & 0xFF) << 8);
            }
            else if (fieldType.IsValueType)
                kind = NeoStaticFieldKind.Value;
            else
                kind = NeoStaticFieldKind.Reference;
            op.Operand4 |= (int)kind;
        }

        OpCodeREnum GetLdfldCodeForType(IType fieldType)
        {
            OpCodeREnum res = OpCodeREnum.Ldfld_Ref;
            if (fieldType.IsPrimitive)
            {
                if (fieldType == appdomain.IntType)
                {
                    res = OpCodeREnum.Ldfld_I4;
                }
                else if (fieldType == appdomain.LongType)
                {
                    res = OpCodeREnum.Ldfld_I8;
                }
                else if (fieldType == appdomain.ShortType)
                {
                    res = OpCodeREnum.Ldfld_I2;
                }
                else if (fieldType == appdomain.ByteType)
                {
                    res = OpCodeREnum.Ldfld_U1;
                }
                else if (fieldType == appdomain.BoolType)
                {
                    res = OpCodeREnum.Ldfld_I1;
                }
                else if (fieldType == appdomain.FloatType)
                {
                    res = OpCodeREnum.Ldfld_R4;
                }
                else if (fieldType == appdomain.DoubleType)
                {
                    res = OpCodeREnum.Ldfld_R8;
                }
                else if (fieldType == appdomain.SByteType)
                {
                    res = OpCodeREnum.Ldfld_I1;
                }
                else if (fieldType == appdomain.UShortType)
                {
                    res = OpCodeREnum.Ldfld_U2;
                }
                else if (fieldType == appdomain.UIntType)
                {
                    res = OpCodeREnum.Ldfld_U4;
                }
                else if (fieldType == appdomain.ULongType)
                {
                    res = OpCodeREnum.Ldfld_U8;
                }
                else if (fieldType == appdomain.CharType)
                {
                    res = OpCodeREnum.Ldfld_U2;
                }
                else if (fieldType == appdomain.IntPtrType)
                {
                    res = OpCodeREnum.Ldfld_U8;
                }
                else
                    throw new NotImplementedException();
            }
            else
            {
                if (fieldType.IsValueType && (fieldType is ILType || (fieldType is CLR.TypeSystem.CLRType clrValueType && clrValueType.StructStorage == CLR.TypeSystem.StructStorage.Inline)))
                {
                    res = OpCodeREnum.Ldfld_Value;
                }
                else
                    res = OpCodeREnum.Ldfld_Ref;
            }
            return res;
        }
        OpCodeREnum GetStfldCodeForType(IType fieldType)
        {
            OpCodeREnum res = OpCodeREnum.Stfld_Ref;
            if (fieldType.IsPrimitive)
            {
                if (fieldType == appdomain.IntType)
                {
                    res = OpCodeREnum.Stfld_I4;
                }
                else if (fieldType == appdomain.LongType)
                {
                    res = OpCodeREnum.Stfld_I8;
                }
                else if (fieldType == appdomain.ShortType)
                {
                    res = OpCodeREnum.Stfld_I2;
                }
                else if (fieldType == appdomain.ByteType)
                {
                    res = OpCodeREnum.Stfld_U1;
                }
                else if (fieldType == appdomain.BoolType)
                {
                    res = OpCodeREnum.Stfld_I1;
                }
                else if (fieldType == appdomain.FloatType)
                {
                    res = OpCodeREnum.Stfld_R4;
                }
                else if (fieldType == appdomain.DoubleType)
                {
                    res = OpCodeREnum.Stfld_R8;
                }
                else if (fieldType == appdomain.SByteType)
                {
                    res = OpCodeREnum.Stfld_I1;
                }
                else if (fieldType == appdomain.UShortType)
                {
                    res = OpCodeREnum.Stfld_U2;
                }
                else if (fieldType == appdomain.UIntType)
                {
                    res = OpCodeREnum.Stfld_U4;
                }
                else if (fieldType == appdomain.ULongType)
                {
                    res = OpCodeREnum.Stfld_U8;
                }
                else if (fieldType == appdomain.CharType)
                {
                    res = OpCodeREnum.Stfld_U2;
                }
                else if (fieldType == appdomain.IntPtrType)
                {
                    res = OpCodeREnum.Stfld_U8;
                }
                else
                    throw new NotImplementedException();
            }
            else
            {
                if (fieldType.IsValueType && (fieldType is ILType || (fieldType is CLR.TypeSystem.CLRType clrValueType && clrValueType.StructStorage == CLR.TypeSystem.StructStorage.Inline)))
                {
                    res = OpCodeREnum.Stfld_Value;
                }
                else
                    res = OpCodeREnum.Stfld_Ref;
            }
            return res;
        }

        void InitializeCallvirtDispatch(ref OpCodes.OpCodeR op, IMethod targetMethod)
        {
            int slot = -1;
            if (targetMethod is ILMethod ilMethod)
            {
                ILType declaringILType = ilMethod.DeclearingType as ILType;
                if (declaringILType != null && declaringILType.IsInterface)
                {
                    // 接口方法：走通用 Callvirt case，把接口内 slot 预编码进 Operand4 低 16 位。
                    // 运行时 ResolveNeoGenericCallvirtTarget 根据 declaredMethod.DeclearingType.IsInterface
                    // 与 instance.Type.neoInterfaceOffsets 合成实际 vtable slot。
                    int ifaceSlot;
                    if (!declaringILType.TryGetInterfaceMethodSlot(ilMethod, out ifaceSlot))
                        ifaceSlot = -1;
                    op.Code = OpCodeREnum.Callvirt;
                    op.Operand4 = EncodeCallvirtDispatch(ifaceSlot, 0);
                    return;
                }

                if (declaringILType != null && !declaringILType.IsInterface && declaringILType.TryGetNeoVTableSlot(ilMethod, out slot))
                {
                    op.Code = OpCodeREnum.Callvirt_IL;
                    op.Operand4 = EncodeCallvirtDispatch(slot, 0);
                }
                else
                {
                    op.Code = OpCodeREnum.Callvirt;
                    op.Operand4 = EncodeCallvirtDispatch(slot, 0);
                }
            }
            else if (targetMethod is CLRMethod clrMethod)
            {
                if (MayCallvirtTargetILObject(clrMethod))
                    op.Code = OpCodeREnum.Callvirt;
                else
                    op.Code = OpCodeREnum.Callvirt_CLR;
                op.Operand4 = EncodeCallvirtDispatch(slot, 0);
            }
        }

        static int EncodeCallvirtDispatch(int slot, int thisArgOffset)
        {
            return ((thisArgOffset & 0xffff) << 16) | (slot & 0xffff);
        }

        static bool MayCallvirtTargetILObject(CLRMethod method)
        {
            IType declaringType = method.DeclearingType;
            if (declaringType == null)
                return true;

            if (declaringType.IsInterface)
                return true;

            Type clrType = declaringType.TypeForCLR;
            return clrType == typeof(object);
        }
    }
}
#endif
