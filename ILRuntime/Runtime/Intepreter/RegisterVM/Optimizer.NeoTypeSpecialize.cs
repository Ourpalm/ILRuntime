using System;
using System.Collections.Generic;

using ILRuntime.CLR.TypeSystem;
using ILRuntime.Runtime.Intepreter.OpCodes;

namespace ILRuntime.Runtime.Intepreter.RegisterVM
{
#if ENABLE_NEO_MODE
    partial class Optimizer
    {
        // NEW: merged type-specialize + SSA-rename pass for Neo mode.
        //
        // Contract:
        //   body            - instruction list AFTER CleanupRegister; mutated in place
        //                     (opcodes may be specialized, register numbers may be rewritten).
        //   paramRegEnd     - end (exclusive) of the parameter/`this` region. Registers < paramRegEnd
        //                     are NEVER renamed because their slot layout is fixed by the caller ABI.
        //                     Locals and stack temporaries (index >= paramRegEnd) participate in rename;
        //                     when a write's type is slot-layout-incompatible with the register's
        //                     currently-tracked type, a new virtual register is allocated for the write
        //                     and subsequent reads.
        //   totalRegCnt     - total register count BEFORE rename (paramCnt + varCnt + StackRegisterCount).
        //   initialTypes    - length == totalRegCnt; entries for this / params / locals pre-filled by
        //                     JITCompiler.BuildInitialRegisterTypes. Stack-register slots may be null.
        //   appdomain       - used for type inference (ObjectType / IntType / GetType / GetMethod).
        //   newTotalRegCnt  - out; possibly-expanded total register count after rename.
        //
        // Returns: IType[] of length newTotalRegCnt, entry per (possibly-renamed) register.
        internal static IType[] TypeSpecializeAndRenameNeoRegisters(
            List<OpCodeR> body,
            short paramRegEnd,
            int totalRegCnt,
            IType[] initialTypes,
            Enviorment.AppDomain appdomain,
            out int newTotalRegCnt)
        {
            short[] renameMap = new short[totalRegCnt];
            for (int i = 0; i < totalRegCnt; i++)
                renameMap[i] = (short)i;
            List<IType> currentTypes = new List<IType>(initialTypes);
            int nextVirtualReg = totalRegCnt;

            for (int i = 0; i < body.Count; i++)
            {
                OpCodeR op = body[i];

                // Remap source register operands. Use GetOpcodeSourceRegister so we only touch fields
                // that actually carry register indices, not opcodes like Brtrue/Brfalse where the union
                // slot aliases a branch target (op.Operand shares its low bytes with Register3).
                short s1, s2, s3;
                if (GetOpcodeSourceRegister(ref op, true, out s1, out s2, out s3))
                {
                    if (s1 >= 0 && s1 < renameMap.Length)
                    {
                        short mapped = renameMap[s1];
                        if (mapped != s1)
                            ReplaceOpcodeSource(ref op, 0, mapped);
                    }
                    if (s2 >= 0 && s2 < renameMap.Length)
                    {
                        short mapped = renameMap[s2];
                        if (mapped != s2)
                            ReplaceOpcodeSource(ref op, 1, mapped);
                    }
                    if (s3 >= 0 && s3 < renameMap.Length)
                    {
                        short mapped = renameMap[s3];
                        if (mapped != s3)
                            ReplaceOpcodeSource(ref op, 2, mapped);
                    }
                }

                IType resultType = null;
                bool hasResult = false;

                switch (op.Code)
                {
                    case OpCodeREnum.Ldc_I4_M1:
                    case OpCodeREnum.Ldc_I4_0:
                    case OpCodeREnum.Ldc_I4_1:
                    case OpCodeREnum.Ldc_I4_2:
                    case OpCodeREnum.Ldc_I4_3:
                    case OpCodeREnum.Ldc_I4_4:
                    case OpCodeREnum.Ldc_I4_5:
                    case OpCodeREnum.Ldc_I4_6:
                    case OpCodeREnum.Ldc_I4_7:
                    case OpCodeREnum.Ldc_I4_8:
                    case OpCodeREnum.Ldc_I4:
                    case OpCodeREnum.Ldc_I4_S:
                        resultType = appdomain.IntType;
                        hasResult = true;
                        break;
                    case OpCodeREnum.Ldc_I8:
                        resultType = appdomain.LongType;
                        hasResult = true;
                        break;
                    case OpCodeREnum.Ldc_R4:
                        resultType = appdomain.FloatType;
                        hasResult = true;
                        break;
                    case OpCodeREnum.Ldc_R8:
                        resultType = appdomain.DoubleType;
                        hasResult = true;
                        break;
                    case OpCodeREnum.Ldnull:
                        resultType = appdomain.ObjectType;
                        hasResult = true;
                        break;
                    case OpCodeREnum.Ldstr:
                        resultType = appdomain.ObjectType;
                        hasResult = true;
                        break;
                    case OpCodeREnum.Move:
                        {
                            IType srcType = GetRegisterTypeFromList(currentTypes, op.Register2);
                            resultType = srcType;
                            hasResult = true;
                        }
                        break;
                    case OpCodeREnum.Neg:
                    case OpCodeREnum.Not:
                        {
                            IType srcType = GetRegisterTypeFromList(currentTypes, op.Register2);
                            op.Code = GetTypedUnaryOpcode(op.Code, InferPrimTag(srcType, appdomain));
                            resultType = srcType;
                            hasResult = true;
                        }
                        break;
                    case OpCodeREnum.Add:
                    case OpCodeREnum.Sub:
                    case OpCodeREnum.Mul:
                    case OpCodeREnum.Div:
                    case OpCodeREnum.Div_Un:
                    case OpCodeREnum.Rem:
                    case OpCodeREnum.Rem_Un:
                    case OpCodeREnum.And:
                    case OpCodeREnum.Or:
                    case OpCodeREnum.Xor:
                    case OpCodeREnum.Shl:
                    case OpCodeREnum.Shr:
                    case OpCodeREnum.Shr_Un:
                        {
                            IType srcType = GetRegisterTypeFromList(currentTypes, op.Register2);
                            op.Code = GetTypedBinaryOpcode(op.Code, InferPrimTag(srcType, appdomain));
                            resultType = srcType;
                            hasResult = true;
                        }
                        break;
                    case OpCodeREnum.Ceq:
                    case OpCodeREnum.Cgt:
                    case OpCodeREnum.Cgt_Un:
                    case OpCodeREnum.Clt:
                    case OpCodeREnum.Clt_Un:
                        {
                            IType srcType = GetRegisterTypeFromList(currentTypes, op.Register2);
                            op.Code = GetTypedCompareOpcode(op.Code, InferPrimTag(srcType, appdomain));
                            resultType = appdomain.IntType;
                            hasResult = true;
                        }
                        break;
                    case OpCodeREnum.Beq:
                    case OpCodeREnum.Beq_S:
                    case OpCodeREnum.Bne_Un:
                    case OpCodeREnum.Bne_Un_S:
                    case OpCodeREnum.Blt:
                    case OpCodeREnum.Blt_S:
                    case OpCodeREnum.Blt_Un:
                    case OpCodeREnum.Blt_Un_S:
                    case OpCodeREnum.Bgt:
                    case OpCodeREnum.Bgt_S:
                    case OpCodeREnum.Bgt_Un:
                    case OpCodeREnum.Bgt_Un_S:
                    case OpCodeREnum.Ble:
                    case OpCodeREnum.Ble_S:
                    case OpCodeREnum.Ble_Un:
                    case OpCodeREnum.Ble_Un_S:
                    case OpCodeREnum.Bge:
                    case OpCodeREnum.Bge_S:
                    case OpCodeREnum.Bge_Un:
                    case OpCodeREnum.Bge_Un_S:
                        {
                            // Register1 is a source for these branches; already remapped by the loop
                            // prologue via GetOpcodeSourceRegister/ReplaceOpcodeSource.
                            short r1 = op.Register1;
                            op.Code = GetTypedBranchOpcode(NormalizeBranchOpcode(op.Code), InferPrimTag(GetRegisterTypeFromList(currentTypes, r1), appdomain));
                        }
                        break;
                    case OpCodeREnum.Addi:
                    case OpCodeREnum.Subi:
                    case OpCodeREnum.Muli:
                    case OpCodeREnum.Divi:
                    case OpCodeREnum.Divi_Un:
                    case OpCodeREnum.Remi:
                    case OpCodeREnum.Remi_Un:
                    case OpCodeREnum.Andi:
                    case OpCodeREnum.Ori:
                    case OpCodeREnum.Xori:
                    case OpCodeREnum.Shli:
                    case OpCodeREnum.Shri:
                    case OpCodeREnum.Shri_Un:
                        {
                            IType srcType = GetRegisterTypeFromList(currentTypes, op.Register2);
                            op.Code = GetTypedImmediateBinaryOpcode(op.Code, InferPrimTag(srcType, appdomain));
                            resultType = srcType;
                            hasResult = true;
                        }
                        break;
                    case OpCodeREnum.Ceqi:
                    case OpCodeREnum.Cgti:
                    case OpCodeREnum.Cgti_Un:
                    case OpCodeREnum.Clti:
                    case OpCodeREnum.Clti_Un:
                        {
                            IType srcType = GetRegisterTypeFromList(currentTypes, op.Register2);
                            op.Code = GetTypedImmediateCompareOpcode(op.Code, InferPrimTag(srcType, appdomain));
                            resultType = appdomain.IntType;
                            hasResult = true;
                        }
                        break;
                    case OpCodeREnum.Beqi:
                    case OpCodeREnum.Bnei_Un:
                    case OpCodeREnum.Blti:
                    case OpCodeREnum.Blti_Un:
                    case OpCodeREnum.Bgti:
                    case OpCodeREnum.Bgti_Un:
                    case OpCodeREnum.Blei:
                    case OpCodeREnum.Blei_Un:
                    case OpCodeREnum.Bgei:
                    case OpCodeREnum.Bgei_Un:
                        {
                            short r1 = op.Register1;
                            op.Code = GetTypedImmediateBranchOpcode(op.Code, InferPrimTag(GetRegisterTypeFromList(currentTypes, r1), appdomain));
                        }
                        break;
                    case OpCodeREnum.Conv_I:
                    case OpCodeREnum.Conv_I1:
                    case OpCodeREnum.Conv_I2:
                    case OpCodeREnum.Conv_I4:
                    case OpCodeREnum.Conv_I8:
                    case OpCodeREnum.Conv_R4:
                    case OpCodeREnum.Conv_R8:
                    case OpCodeREnum.Conv_R_Un:
                    case OpCodeREnum.Conv_U:
                    case OpCodeREnum.Conv_U1:
                    case OpCodeREnum.Conv_U2:
                    case OpCodeREnum.Conv_U4:
                    case OpCodeREnum.Conv_U8:
                        {
                            IType srcType = GetRegisterTypeFromList(currentTypes, op.Register2);
                            op.Operand2 = (int)InferPrimTag(srcType, appdomain);
                            resultType = GetConvResultType(op.Code, appdomain);
                            hasResult = true;
                        }
                        break;
                    case OpCodeREnum.Ldfld_I1:
                    case OpCodeREnum.Ldfld_I2:
                    case OpCodeREnum.Ldfld_I4:
                    case OpCodeREnum.Ldfld_U1:
                    case OpCodeREnum.Ldfld_U2:
                    case OpCodeREnum.Ldfld_U4:
                    case OpCodeREnum.Ldlen:
                        resultType = appdomain.IntType;
                        hasResult = true;
                        break;
                    case OpCodeREnum.Ldfld_I8:
                    case OpCodeREnum.Ldfld_U8:
                        resultType = appdomain.LongType;
                        hasResult = true;
                        break;
                    case OpCodeREnum.Ldfld_R4:
                        resultType = appdomain.FloatType;
                        hasResult = true;
                        break;
                    case OpCodeREnum.Ldfld_R8:
                        resultType = appdomain.DoubleType;
                        hasResult = true;
                        break;
                    case OpCodeREnum.Ldfld_Ref:
                        resultType = appdomain.ObjectType;
                        hasResult = true;
                        break;
                    case OpCodeREnum.Ldind_I:
                    case OpCodeREnum.Ldind_I1:
                    case OpCodeREnum.Ldind_I2:
                    case OpCodeREnum.Ldind_I4:
                    case OpCodeREnum.Ldind_U1:
                    case OpCodeREnum.Ldind_U2:
                    case OpCodeREnum.Ldind_U4:
                        resultType = appdomain.IntType;
                        hasResult = true;
                        break;
                    case OpCodeREnum.Ldind_I8:
                        resultType = appdomain.LongType;
                        hasResult = true;
                        break;
                    case OpCodeREnum.Ldind_R4:
                        resultType = appdomain.FloatType;
                        hasResult = true;
                        break;
                    case OpCodeREnum.Ldind_R8:
                        resultType = appdomain.DoubleType;
                        hasResult = true;
                        break;
                    case OpCodeREnum.Ldind_Ref:
                        resultType = appdomain.ObjectType;
                        hasResult = true;
                        break;
                    case OpCodeREnum.Ldfld_Value:
                        {
                            var t = appdomain.GetType(op.Operand);
                            if (t != null)
                            {
                                resultType = t;
                                hasResult = true;
                            }
                        }
                        break;
                    case OpCodeREnum.Call:
                    case OpCodeREnum.Callvirt:
                    case OpCodeREnum.Callvirt_IL:
                    case OpCodeREnum.Callvirt_CLR:
                    case OpCodeREnum.Call_Redirect:
                        if (op.Register1 >= 0)
                        {
                            var callee = appdomain.GetMethod(op.Operand2);
                            if (callee != null && callee.ReturnType != null && callee.ReturnType != appdomain.VoidType)
                            {
                                resultType = callee.ReturnType;
                                hasResult = true;
                            }
                        }
                        break;
                    case OpCodeREnum.Newobj:
                        {
                            var ctor = appdomain.GetMethod(op.Operand2);
                            if (ctor != null && ctor.DeclearingType != null)
                            {
                                resultType = ctor.DeclearingType;
                                hasResult = true;
                            }
                        }
                        break;
                    case OpCodeREnum.Box:
                        resultType = appdomain.ObjectType;
                        hasResult = true;
                        break;
                    case OpCodeREnum.Unbox:
                    case OpCodeREnum.Unbox_Any:
                    case OpCodeREnum.Isinst:
                    case OpCodeREnum.Castclass:
                        {
                            var t = appdomain.GetType(op.Operand);
                            if (t != null)
                            {
                                resultType = t;
                                hasResult = true;
                            }
                        }
                        break;
                    case OpCodeREnum.Ldsfld:
                        {
                            var t = appdomain.GetType(op.Operand);
                            if (t != null)
                            {
                                resultType = t;
                                hasResult = true;
                            }
                        }
                        break;
                    case OpCodeREnum.Ldsflda:
                        {
                            var t = appdomain.GetType(op.Operand);
                            if (t != null)
                            {
                                resultType = t.MakeByRefType();
                                hasResult = true;
                            }
                        }
                        break;
                    case OpCodeREnum.Ldtoken:
                        resultType = appdomain.ObjectType;
                        hasResult = true;
                        break;
                    case OpCodeREnum.Ldftn:
                    case OpCodeREnum.Ldvirtftn:
                        resultType = appdomain.IntType;
                        hasResult = true;
                        break;
                    case OpCodeREnum.Ldelem_I1:
                    case OpCodeREnum.Ldelem_U1:
                    case OpCodeREnum.Ldelem_I2:
                    case OpCodeREnum.Ldelem_U2:
                    case OpCodeREnum.Ldelem_I4:
                    case OpCodeREnum.Ldelem_U4:
                    case OpCodeREnum.Ldelem_I:
                        resultType = appdomain.IntType;
                        hasResult = true;
                        break;
                    case OpCodeREnum.Ldelem_I8:
                        resultType = appdomain.LongType;
                        hasResult = true;
                        break;
                    case OpCodeREnum.Ldelem_R4:
                        resultType = appdomain.FloatType;
                        hasResult = true;
                        break;
                    case OpCodeREnum.Ldelem_R8:
                        resultType = appdomain.DoubleType;
                        hasResult = true;
                        break;
                    case OpCodeREnum.Ldelem_Ref:
                        resultType = appdomain.ObjectType;
                        hasResult = true;
                        break;
                    case OpCodeREnum.Ldelem_Any:
                        {
                            var t = appdomain.GetType(op.Operand);
                            resultType = t ?? appdomain.ObjectType;
                            hasResult = true;
                        }
                        break;
                    case OpCodeREnum.Ldloca:
                    case OpCodeREnum.Ldloca_S:
                    case OpCodeREnum.Ldarga:
                    case OpCodeREnum.Ldarga_S:
                        {
                            IType srcType = GetRegisterTypeFromList(currentTypes, op.Register2);
                            resultType = srcType.MakeByRefType();
                            hasResult = true;
                        }
                        break;
                    case OpCodeREnum.Ldelema:
                        resultType = appdomain.IntType;
                        hasResult = true;
                        break;
                    case OpCodeREnum.Ldflda:
                        {
                            var t = appdomain.GetType(op.Operand);
                            if (t != null)
                            {
                                resultType = t.MakeByRefType();
                                hasResult = true;
                            }
                        }
                        break;
                    case OpCodeREnum.Dup:
                        resultType = GetRegisterTypeFromList(currentTypes, op.Register2);
                        hasResult = true;
                        break;
                }

                if (hasResult && op.Register1 >= 0)
                {
                    short dstOrig = op.Register1;
                    if (dstOrig < paramRegEnd)
                    {
                        if (resultType != null && dstOrig < currentTypes.Count)
                            currentTypes[dstOrig] = resultType;
                    }
                    else
                    {
                        short curReg = renameMap[dstOrig];
                        IType existing = curReg < currentTypes.Count ? currentTypes[curReg] : null;
                        if (existing == null || SlotLayoutCompatible(existing, resultType))
                        {
                            if (resultType != null && curReg < currentTypes.Count)
                                currentTypes[curReg] = resultType;
                            op.Register1 = curReg;
                        }
                        else
                        {
                            if (nextVirtualReg >= short.MaxValue)
                                throw new NotSupportedException("Neo SSA rename: virtual register count exceeds Int16.MaxValue.");
                            short newReg = (short)nextVirtualReg++;
                            currentTypes.Add(resultType);
                            renameMap[dstOrig] = newReg;
                            op.Register1 = newReg;
                        }
                    }
                }

                body[i] = op;
            }

            newTotalRegCnt = currentTypes.Count;
            return currentTypes.ToArray();
        }

        static bool SlotLayoutCompatible(IType a, IType b)
        {
            if (a == null || b == null) return true;
            if (ReferenceEquals(a, b)) return true;
            if (a.IsByRef || b.IsByRef)
                return a.IsByRef && b.IsByRef;
            int aSize, aRef, bSize, bRef;
            ClassifySlot(a, out aSize, out aRef);
            ClassifySlot(b, out bSize, out bRef);
            if (aSize != bSize || aRef != bRef) return false;
            bool aStruct = a.IsValueType && !a.IsPrimitive;
            bool bStruct = b.IsValueType && !b.IsPrimitive;
            if (aStruct || bStruct) return ReferenceEquals(a, b);
            return true;
        }

        static void ClassifySlot(IType t, out int size, out int refCount)
        {
            if (t.IsByRef)
            {
                size = 8;
                refCount = 0;
                return;
            }
            if (t.IsPrimitive)
            {
                var clr = t.TypeForCLR;
                if (clr == typeof(long) || clr == typeof(ulong) || clr == typeof(double))
                {
                    size = 8;
                    refCount = 0;
                    return;
                }
                if (clr == typeof(IntPtr) || clr == typeof(UIntPtr))
                {
                    size = IntPtr.Size == 8 ? 8 : 4;
                    refCount = 0;
                    return;
                }
                size = 4;
                refCount = 0;
                return;
            }
            if (t.IsValueType && t is ILType il)
            {
                size = il.TotalPrimitiveSize;
                refCount = il.TotalReferenceCount;
                return;
            }
            if (t.IsValueType && t is CLRType inlineClr &&
                inlineClr.StructStorage == StructStorage.Inline)
            {
                size = inlineClr.TotalPrimitiveSize;
                refCount = inlineClr.TotalReferenceCount;
                return;
            }
            size = 4;
            refCount = 1;
        }

        static IType GetRegisterType(IType[] registerTypes, short reg)
        {
            if (reg >= 0 && reg < registerTypes.Length)
                return registerTypes[reg];
            return null;
        }

        static void SetRegisterType(IType[] registerTypes, short reg, IType type)
        {
            if (reg >= 0 && reg < registerTypes.Length)
                registerTypes[reg] = type;
        }

        static IType GetRegisterTypeFromList(List<IType> types, short reg)
        {
            if (reg >= 0 && reg < types.Count)
                return types[reg];
            return null;
        }

        static IType GetConvResultType(OpCodeREnum code, Enviorment.AppDomain appdomain)
        {
            switch (code)
            {
                case OpCodeREnum.Conv_I8:
                    return appdomain.LongType;
                case OpCodeREnum.Conv_U8:
                    return appdomain.ULongType;
                case OpCodeREnum.Conv_R4:
                    return appdomain.FloatType;
                case OpCodeREnum.Conv_R8:
                case OpCodeREnum.Conv_R_Un:
                    return appdomain.DoubleType;
                case OpCodeREnum.Conv_U4:
                case OpCodeREnum.Conv_U:
                    return appdomain.UIntType;
                default:
                    return appdomain.IntType;
            }
        }

        internal static NeoPrimitiveTypeTag InferPrimTag(IType t, Enviorment.AppDomain appdomain)
        {
            if (t == null)
                return NeoPrimitiveTypeTag.I4;

            if (t is ILType ilt && ilt.IsEnum)
            {
                var fts = ilt.FieldTypes;
                if (fts != null && fts.Length > 0)
                    t = fts[0];
            }

            var clr = t.TypeForCLR;
            if (clr == typeof(bool) || clr == typeof(byte) || clr == typeof(sbyte)
                || clr == typeof(short) || clr == typeof(ushort) || clr == typeof(char)
                || clr == typeof(int))
                return NeoPrimitiveTypeTag.I4;
            if (clr == typeof(uint))
                return NeoPrimitiveTypeTag.U4;
            if (clr == typeof(long))
                return NeoPrimitiveTypeTag.I8;
            if (clr == typeof(ulong))
                return NeoPrimitiveTypeTag.U8;
            if (clr == typeof(float))
                return NeoPrimitiveTypeTag.R4;
            if (clr == typeof(double))
                return NeoPrimitiveTypeTag.R8;
            if (clr == typeof(IntPtr) || clr == typeof(UIntPtr))
                return IntPtr.Size == 8 ? NeoPrimitiveTypeTag.I8 : NeoPrimitiveTypeTag.I4;

            return NeoPrimitiveTypeTag.I4;
        }

        static OpCodeREnum NormalizeBranchOpcode(OpCodeREnum code)
        {
            switch (code)
            {
                case OpCodeREnum.Beq_S:
                    return OpCodeREnum.Beq;
                case OpCodeREnum.Bne_Un_S:
                    return OpCodeREnum.Bne_Un;
                case OpCodeREnum.Blt_S:
                    return OpCodeREnum.Blt;
                case OpCodeREnum.Blt_Un_S:
                    return OpCodeREnum.Blt_Un;
                case OpCodeREnum.Bgt_S:
                    return OpCodeREnum.Bgt;
                case OpCodeREnum.Bgt_Un_S:
                    return OpCodeREnum.Bgt_Un;
                case OpCodeREnum.Ble_S:
                    return OpCodeREnum.Ble;
                case OpCodeREnum.Ble_Un_S:
                    return OpCodeREnum.Ble_Un;
                case OpCodeREnum.Bge_S:
                    return OpCodeREnum.Bge;
                case OpCodeREnum.Bge_Un_S:
                    return OpCodeREnum.Bge_Un;
                default:
                    return code;
            }
        }

        static OpCodeREnum GetTypedBinaryOpcode(OpCodeREnum code, NeoPrimitiveTypeTag tag)
        {
            switch (tag)
            {
                case NeoPrimitiveTypeTag.I8:
                case NeoPrimitiveTypeTag.U8:
                    switch (code)
                    {
                        case OpCodeREnum.Add: return OpCodeREnum.Add_I8;
                        case OpCodeREnum.Sub: return OpCodeREnum.Sub_I8;
                        case OpCodeREnum.Mul: return OpCodeREnum.Mul_I8;
                        case OpCodeREnum.Div: return OpCodeREnum.Div_I8;
                        case OpCodeREnum.Div_Un: return OpCodeREnum.Div_Un_I8;
                        case OpCodeREnum.Rem: return OpCodeREnum.Rem_I8;
                        case OpCodeREnum.Rem_Un: return OpCodeREnum.Rem_Un_I8;
                        case OpCodeREnum.And: return OpCodeREnum.And_I8;
                        case OpCodeREnum.Or: return OpCodeREnum.Or_I8;
                        case OpCodeREnum.Xor: return OpCodeREnum.Xor_I8;
                        case OpCodeREnum.Shl: return OpCodeREnum.Shl_I8;
                        case OpCodeREnum.Shr: return OpCodeREnum.Shr_I8;
                        case OpCodeREnum.Shr_Un: return OpCodeREnum.Shr_Un_I8;
                    }
                    break;
                case NeoPrimitiveTypeTag.R4:
                    switch (code)
                    {
                        case OpCodeREnum.Add: return OpCodeREnum.Add_R4;
                        case OpCodeREnum.Sub: return OpCodeREnum.Sub_R4;
                        case OpCodeREnum.Mul: return OpCodeREnum.Mul_R4;
                        case OpCodeREnum.Div: return OpCodeREnum.Div_R4;
                        case OpCodeREnum.Rem: return OpCodeREnum.Rem_R4;
                    }
                    break;
                case NeoPrimitiveTypeTag.R8:
                    switch (code)
                    {
                        case OpCodeREnum.Add: return OpCodeREnum.Add_R8;
                        case OpCodeREnum.Sub: return OpCodeREnum.Sub_R8;
                        case OpCodeREnum.Mul: return OpCodeREnum.Mul_R8;
                        case OpCodeREnum.Div: return OpCodeREnum.Div_R8;
                        case OpCodeREnum.Rem: return OpCodeREnum.Rem_R8;
                    }
                    break;
            }
            return code;
        }

        static OpCodeREnum GetTypedUnaryOpcode(OpCodeREnum code, NeoPrimitiveTypeTag tag)
        {
            if (code == OpCodeREnum.Neg)
            {
                if (tag == NeoPrimitiveTypeTag.I8 || tag == NeoPrimitiveTypeTag.U8)
                    return OpCodeREnum.Neg_I8;
                if (tag == NeoPrimitiveTypeTag.R4)
                    return OpCodeREnum.Neg_R4;
                if (tag == NeoPrimitiveTypeTag.R8)
                    return OpCodeREnum.Neg_R8;
            }
            else if (code == OpCodeREnum.Not && (tag == NeoPrimitiveTypeTag.I8 || tag == NeoPrimitiveTypeTag.U8))
            {
                return OpCodeREnum.Not_I8;
            }
            return code;
        }

        static OpCodeREnum GetTypedCompareOpcode(OpCodeREnum code, NeoPrimitiveTypeTag tag)
        {
            switch (tag)
            {
                case NeoPrimitiveTypeTag.I8:
                case NeoPrimitiveTypeTag.U8:
                    switch (code)
                    {
                        case OpCodeREnum.Ceq: return OpCodeREnum.Ceq_I8;
                        case OpCodeREnum.Cgt: return OpCodeREnum.Cgt_I8;
                        case OpCodeREnum.Cgt_Un: return OpCodeREnum.Cgt_Un_I8;
                        case OpCodeREnum.Clt: return OpCodeREnum.Clt_I8;
                        case OpCodeREnum.Clt_Un: return OpCodeREnum.Clt_Un_I8;
                    }
                    break;
                case NeoPrimitiveTypeTag.R4:
                    switch (code)
                    {
                        case OpCodeREnum.Ceq: return OpCodeREnum.Ceq_R4;
                        case OpCodeREnum.Cgt: return OpCodeREnum.Cgt_R4;
                        case OpCodeREnum.Cgt_Un: return OpCodeREnum.Cgt_Un_R4;
                        case OpCodeREnum.Clt: return OpCodeREnum.Clt_R4;
                        case OpCodeREnum.Clt_Un: return OpCodeREnum.Clt_Un_R4;
                    }
                    break;
                case NeoPrimitiveTypeTag.R8:
                    switch (code)
                    {
                        case OpCodeREnum.Ceq: return OpCodeREnum.Ceq_R8;
                        case OpCodeREnum.Cgt: return OpCodeREnum.Cgt_R8;
                        case OpCodeREnum.Cgt_Un: return OpCodeREnum.Cgt_Un_R8;
                        case OpCodeREnum.Clt: return OpCodeREnum.Clt_R8;
                        case OpCodeREnum.Clt_Un: return OpCodeREnum.Clt_Un_R8;
                    }
                    break;
            }
            return code;
        }

        static OpCodeREnum GetTypedBranchOpcode(OpCodeREnum code, NeoPrimitiveTypeTag tag)
        {
            if (tag == NeoPrimitiveTypeTag.I8 || tag == NeoPrimitiveTypeTag.U8)
            {
                switch (code)
                {
                    case OpCodeREnum.Beq: return OpCodeREnum.Beq_I8;
                    case OpCodeREnum.Bne_Un: return OpCodeREnum.Bne_Un_I8;
                    case OpCodeREnum.Blt: return OpCodeREnum.Blt_I8;
                    case OpCodeREnum.Blt_Un: return OpCodeREnum.Blt_Un_I8;
                    case OpCodeREnum.Bgt: return OpCodeREnum.Bgt_I8;
                    case OpCodeREnum.Bgt_Un: return OpCodeREnum.Bgt_Un_I8;
                    case OpCodeREnum.Ble: return OpCodeREnum.Ble_I8;
                    case OpCodeREnum.Ble_Un: return OpCodeREnum.Ble_Un_I8;
                    case OpCodeREnum.Bge: return OpCodeREnum.Bge_I8;
                    case OpCodeREnum.Bge_Un: return OpCodeREnum.Bge_Un_I8;
                }
            }
            if (tag == NeoPrimitiveTypeTag.R4)
            {
                switch (code)
                {
                    case OpCodeREnum.Beq: return OpCodeREnum.Beq_R4;
                    case OpCodeREnum.Bne_Un: return OpCodeREnum.Bne_Un_R4;
                    case OpCodeREnum.Blt: return OpCodeREnum.Blt_R4;
                    case OpCodeREnum.Blt_Un: return OpCodeREnum.Blt_Un_R4;
                    case OpCodeREnum.Bgt: return OpCodeREnum.Bgt_R4;
                    case OpCodeREnum.Bgt_Un: return OpCodeREnum.Bgt_Un_R4;
                    case OpCodeREnum.Ble: return OpCodeREnum.Ble_R4;
                    case OpCodeREnum.Ble_Un: return OpCodeREnum.Ble_Un_R4;
                    case OpCodeREnum.Bge: return OpCodeREnum.Bge_R4;
                    case OpCodeREnum.Bge_Un: return OpCodeREnum.Bge_Un_R4;
                }
            }
            if (tag == NeoPrimitiveTypeTag.R8)
            {
                switch (code)
                {
                    case OpCodeREnum.Beq: return OpCodeREnum.Beq_R8;
                    case OpCodeREnum.Bne_Un: return OpCodeREnum.Bne_Un_R8;
                    case OpCodeREnum.Blt: return OpCodeREnum.Blt_R8;
                    case OpCodeREnum.Blt_Un: return OpCodeREnum.Blt_Un_R8;
                    case OpCodeREnum.Bgt: return OpCodeREnum.Bgt_R8;
                    case OpCodeREnum.Bgt_Un: return OpCodeREnum.Bgt_Un_R8;
                    case OpCodeREnum.Ble: return OpCodeREnum.Ble_R8;
                    case OpCodeREnum.Ble_Un: return OpCodeREnum.Ble_Un_R8;
                    case OpCodeREnum.Bge: return OpCodeREnum.Bge_R8;
                    case OpCodeREnum.Bge_Un: return OpCodeREnum.Bge_Un_R8;
                }
            }
            return code;
        }

        static OpCodeREnum GetTypedImmediateBinaryOpcode(OpCodeREnum code, NeoPrimitiveTypeTag tag)
        {
            switch (tag)
            {
                case NeoPrimitiveTypeTag.I8:
                case NeoPrimitiveTypeTag.U8:
                    switch (code)
                    {
                        case OpCodeREnum.Addi: return OpCodeREnum.Addi_I8;
                        case OpCodeREnum.Subi: return OpCodeREnum.Subi_I8;
                        case OpCodeREnum.Muli: return OpCodeREnum.Muli_I8;
                        case OpCodeREnum.Divi: return OpCodeREnum.Divi_I8;
                        case OpCodeREnum.Divi_Un: return OpCodeREnum.Divi_Un_I8;
                        case OpCodeREnum.Remi: return OpCodeREnum.Remi_I8;
                        case OpCodeREnum.Remi_Un: return OpCodeREnum.Remi_Un_I8;
                        case OpCodeREnum.Andi: return OpCodeREnum.Andi_I8;
                        case OpCodeREnum.Ori: return OpCodeREnum.Ori_I8;
                        case OpCodeREnum.Xori: return OpCodeREnum.Xori_I8;
                        case OpCodeREnum.Shli: return OpCodeREnum.Shli_I8;
                        case OpCodeREnum.Shri: return OpCodeREnum.Shri_I8;
                        case OpCodeREnum.Shri_Un: return OpCodeREnum.Shri_Un_I8;
                    }
                    break;
                case NeoPrimitiveTypeTag.R4:
                    switch (code)
                    {
                        case OpCodeREnum.Addi: return OpCodeREnum.Addi_R4;
                        case OpCodeREnum.Subi: return OpCodeREnum.Subi_R4;
                        case OpCodeREnum.Muli: return OpCodeREnum.Muli_R4;
                        case OpCodeREnum.Divi: return OpCodeREnum.Divi_R4;
                        case OpCodeREnum.Remi: return OpCodeREnum.Remi_R4;
                    }
                    break;
                case NeoPrimitiveTypeTag.R8:
                    switch (code)
                    {
                        case OpCodeREnum.Addi: return OpCodeREnum.Addi_R8;
                        case OpCodeREnum.Subi: return OpCodeREnum.Subi_R8;
                        case OpCodeREnum.Muli: return OpCodeREnum.Muli_R8;
                        case OpCodeREnum.Divi: return OpCodeREnum.Divi_R8;
                        case OpCodeREnum.Remi: return OpCodeREnum.Remi_R8;
                    }
                    break;
            }
            return code;
        }

        static OpCodeREnum GetTypedImmediateCompareOpcode(OpCodeREnum code, NeoPrimitiveTypeTag tag)
        {
            switch (tag)
            {
                case NeoPrimitiveTypeTag.I8:
                case NeoPrimitiveTypeTag.U8:
                    switch (code)
                    {
                        case OpCodeREnum.Ceqi: return OpCodeREnum.Ceqi_I8;
                        case OpCodeREnum.Cgti: return OpCodeREnum.Cgti_I8;
                        case OpCodeREnum.Cgti_Un: return OpCodeREnum.Cgti_Un_I8;
                        case OpCodeREnum.Clti: return OpCodeREnum.Clti_I8;
                        case OpCodeREnum.Clti_Un: return OpCodeREnum.Clti_Un_I8;
                    }
                    break;
                case NeoPrimitiveTypeTag.R4:
                    switch (code)
                    {
                        case OpCodeREnum.Ceqi: return OpCodeREnum.Ceqi_R4;
                        case OpCodeREnum.Cgti: return OpCodeREnum.Cgti_R4;
                        case OpCodeREnum.Cgti_Un: return OpCodeREnum.Cgti_Un_R4;
                        case OpCodeREnum.Clti: return OpCodeREnum.Clti_R4;
                        case OpCodeREnum.Clti_Un: return OpCodeREnum.Clti_Un_R4;
                    }
                    break;
                case NeoPrimitiveTypeTag.R8:
                    switch (code)
                    {
                        case OpCodeREnum.Ceqi: return OpCodeREnum.Ceqi_R8;
                        case OpCodeREnum.Cgti: return OpCodeREnum.Cgti_R8;
                        case OpCodeREnum.Cgti_Un: return OpCodeREnum.Cgti_Un_R8;
                        case OpCodeREnum.Clti: return OpCodeREnum.Clti_R8;
                        case OpCodeREnum.Clti_Un: return OpCodeREnum.Clti_Un_R8;
                    }
                    break;
            }
            return code;
        }

        static OpCodeREnum GetTypedImmediateBranchOpcode(OpCodeREnum code, NeoPrimitiveTypeTag tag)
        {
            if (tag == NeoPrimitiveTypeTag.I8 || tag == NeoPrimitiveTypeTag.U8)
            {
                switch (code)
                {
                    case OpCodeREnum.Beqi: return OpCodeREnum.Beqi_I8;
                    case OpCodeREnum.Bnei_Un: return OpCodeREnum.Bnei_Un_I8;
                    case OpCodeREnum.Blti: return OpCodeREnum.Blti_I8;
                    case OpCodeREnum.Blti_Un: return OpCodeREnum.Blti_Un_I8;
                    case OpCodeREnum.Bgti: return OpCodeREnum.Bgti_I8;
                    case OpCodeREnum.Bgti_Un: return OpCodeREnum.Bgti_Un_I8;
                    case OpCodeREnum.Blei: return OpCodeREnum.Blei_I8;
                    case OpCodeREnum.Blei_Un: return OpCodeREnum.Blei_Un_I8;
                    case OpCodeREnum.Bgei: return OpCodeREnum.Bgei_I8;
                    case OpCodeREnum.Bgei_Un: return OpCodeREnum.Bgei_Un_I8;
                }
            }
            if (tag == NeoPrimitiveTypeTag.R4)
            {
                switch (code)
                {
                    case OpCodeREnum.Beqi: return OpCodeREnum.Beqi_R4;
                    case OpCodeREnum.Bnei_Un: return OpCodeREnum.Bnei_Un_R4;
                    case OpCodeREnum.Blti: return OpCodeREnum.Blti_R4;
                    case OpCodeREnum.Blti_Un: return OpCodeREnum.Blti_Un_R4;
                    case OpCodeREnum.Bgti: return OpCodeREnum.Bgti_R4;
                    case OpCodeREnum.Bgti_Un: return OpCodeREnum.Bgti_Un_R4;
                    case OpCodeREnum.Blei: return OpCodeREnum.Blei_R4;
                    case OpCodeREnum.Blei_Un: return OpCodeREnum.Blei_Un_R4;
                    case OpCodeREnum.Bgei: return OpCodeREnum.Bgei_R4;
                    case OpCodeREnum.Bgei_Un: return OpCodeREnum.Bgei_Un_R4;
                }
            }
            if (tag == NeoPrimitiveTypeTag.R8)
            {
                switch (code)
                {
                    case OpCodeREnum.Beqi: return OpCodeREnum.Beqi_R8;
                    case OpCodeREnum.Bnei_Un: return OpCodeREnum.Bnei_Un_R8;
                    case OpCodeREnum.Blti: return OpCodeREnum.Blti_R8;
                    case OpCodeREnum.Blti_Un: return OpCodeREnum.Blti_Un_R8;
                    case OpCodeREnum.Bgti: return OpCodeREnum.Bgti_R8;
                    case OpCodeREnum.Bgti_Un: return OpCodeREnum.Bgti_Un_R8;
                    case OpCodeREnum.Blei: return OpCodeREnum.Blei_R8;
                    case OpCodeREnum.Blei_Un: return OpCodeREnum.Blei_Un_R8;
                    case OpCodeREnum.Bgei: return OpCodeREnum.Bgei_R8;
                    case OpCodeREnum.Bgei_Un: return OpCodeREnum.Bgei_Un_R8;
                }
            }
            return code;
        }
    }
#endif
}
