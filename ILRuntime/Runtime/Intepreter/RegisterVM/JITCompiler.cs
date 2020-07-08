using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ILRuntime.Mono.Cecil;
using ILRuntime.Mono.Cecil.Cil;

using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Intepreter.OpCodes;

namespace ILRuntime.Runtime.Intepreter.RegisterVM
{
    struct JITCompiler
    {
        Enviorment.AppDomain appdomain;
        ILType declaringType;
        ILMethod method;
        MethodDefinition def;
        bool hasReturn;
        Dictionary<Instruction, int> entryMapping;
        public JITCompiler(Enviorment.AppDomain appDomain, ILType declaringType, ILMethod method)
        {
            this.appdomain = appDomain;
            this.declaringType = declaringType;
            this.method = method;
            def = method.Definition;
            hasReturn = method.ReturnType != appdomain.VoidType;
            entryMapping = null;
        }

        public OpCodeR[] Compile(out int stackRegisterCnt)
        {
            var body = def.Body;
            short locVarRegStart = (short)def.Parameters.Count;
            if (!def.IsStatic)
                locVarRegStart++;
            short baseRegIdx = (short)(locVarRegStart + body.Variables.Count);
            short baseRegStart = baseRegIdx;

            var blocks = CodeBasicBlock.BuildBasicBlocks(body, out entryMapping);

            foreach(var i in blocks)
            {
                var lst = i.FinalInstructions;
                baseRegIdx = baseRegStart;
                if (i.PreviousBlocks.Count > 0)
                {
                    foreach (var j in i.PreviousBlocks)
                    {
                        if (j.EndRegister >= 0)
                        {
                            baseRegIdx = j.EndRegister;
                            break;
                        }
                    }
                }
                foreach (var ins in i.Instructions)
                {
                    Translate(lst, ins, locVarRegStart, ref baseRegIdx);
                }
                i.EndRegister = baseRegIdx;
            }

            Optimizer.ForwardCopyPropagation(blocks, hasReturn, baseRegStart);
            Optimizer.BackwardsCopyPropagation(blocks, hasReturn, baseRegStart);
            Optimizer.ForwardCopyPropagation(blocks, hasReturn, baseRegStart);

            
            List<OpCodeR> res = new List<OpCodeR>();
            Dictionary<int, int> jumpTargets = new Dictionary<int, int>();
            int bIdx = 0;
            HashSet<int> inlinedBranches = new HashSet<int>();
            foreach(var b in blocks)
            {
                jumpTargets[bIdx++] = res.Count;
                bool isInline = false;
                int inlineOffset = 0;
                for (int idx = 0; idx < b.FinalInstructions.Count; idx++)
                {
                    if (b.CanRemove.Contains(idx))
                        continue;
                    var ins = b.FinalInstructions[idx];
                    if (ins.Code == OpCodeREnum.InlineStart)
                    {
                        isInline = true;
                        inlineOffset = res.Count;
                    }
                    else if (ins.Code == OpCodeREnum.InlineEnd)
                    {
                        isInline = false;
                    }
                    else
                    {
                        if (isInline)
                        {
                            if (Optimizer.IsBranching(ins.Code))
                            {
                                ins.Operand += inlineOffset;
                                inlinedBranches.Add(res.Count);
                            }
                        }
                        res.Add(ins);
                    }
                }
            }
            for(int i = 0; i < res.Count; i++)
            {
                var op = res[i];
                if (Optimizer.IsBranching(op.Code) && !inlinedBranches.Contains(i))
                {
                    op.Operand = jumpTargets[op.Operand];
                    res[i] = op;
                }
            }
            var totalRegCnt = Optimizer.CleanupRegister(res, locVarRegStart, hasReturn);
            stackRegisterCnt = totalRegCnt - baseRegStart;
            return res.ToArray();
        }

        void Translate(List<OpCodeR> lst, Instruction ins, short locVarRegStart, ref short baseRegIdx)
        {
            OpCodeR op = new OpCodeR();
            var code = ins.OpCode;
            var token = ins.Operand;
            op.Code = (OpCodeREnum)code.Code;
            bool hasRet;
            switch (code.Code)
            {
                case Code.Br_S:
                case Code.Br:
                    op.Operand = entryMapping[(Mono.Cecil.Cil.Instruction)token];
                    break;
                case Code.Brtrue:
                case Code.Brtrue_S:
                case Code.Brfalse:
                case Code.Brfalse_S:
                    op.Register1 = --baseRegIdx;
                    op.Operand = entryMapping[(Mono.Cecil.Cil.Instruction)token];
                    break;
                case Code.Blt:
                case Code.Blt_S:
                case Code.Blt_Un:
                case Code.Blt_Un_S:
                case Code.Ble:
                case Code.Ble_S:
                case Code.Ble_Un:
                case Code.Ble_Un_S:
                case Code.Bgt:
                case Code.Bgt_S:
                case Code.Bgt_Un:
                case Code.Bgt_Un_S:
                case Code.Bge:
                case Code.Bge_S:
                case Code.Bge_Un:
                case Code.Bge_Un_S:
                case Code.Beq:
                case Code.Beq_S:
                case Code.Bne_Un:
                case Code.Bne_Un_S:
                    op.Register1 = (short)(baseRegIdx - 2);
                    op.Register2 = (short)(baseRegIdx - 1);
                    baseRegIdx -= 2;
                    op.Operand = entryMapping[(Mono.Cecil.Cil.Instruction)token];
                    break;
                case Code.Ldc_I4_0:
                case Code.Ldc_I4_1:
                case Code.Ldc_I4_2:
                case Code.Ldc_I4_3:
                case Code.Ldc_I4_4:
                case Code.Ldc_I4_5:
                case Code.Ldc_I4_6:
                case Code.Ldc_I4_7:
                case Code.Ldc_I4_8:
                case Code.Ldc_I4_M1:
                case Code.Ldnull:
                    op.Register1 = baseRegIdx++;
                    break;
                case Code.Ldc_I4:
                    op.Register1 = baseRegIdx++;
                    op.Operand = (int)token;
                    break;
                case Code.Ldc_I4_S:
                    op.Register1 = baseRegIdx++;
                    op.Operand = (sbyte)token;
                    break;
                case Code.Ldc_I8:
                    op.Register1 = baseRegIdx++;
                    op.OperandLong = (long)token;
                    break;
                case Code.Ldc_R4:
                    op.Register1 = baseRegIdx++;
                    op.OperandFloat = (float)token;
                    break;
                case Code.Ldc_R8:
                    op.Register1 = baseRegIdx++;
                    op.OperandDouble = (double)token;
                    break;
                case Code.Ldstr:
                    op.Register1 = baseRegIdx++;
                    op.OperandLong = appdomain.CacheString(token);
                    break;
                case Code.Newobj:
                    {
                        bool canInline;
                        ILMethod toInline;
                        var pCnt = InitializeFunctionParam(ref op, token, out hasRet, out canInline, out toInline);
                        for (int i = pCnt; i > 0; i--)
                        {
                            OpCodes.OpCodeR op2 = new OpCodes.OpCodeR();
                            op2.Code = OpCodes.OpCodeREnum.Push;
                            op2.Register1 = (short)(baseRegIdx - i);
                            lst.Add(op2);
                        }
                        baseRegIdx -= (short)pCnt;
                        op.Register1 = baseRegIdx++;
                    }
                    break;
                case Code.Call:
                case Code.Callvirt:
                    {
                        bool canInline;
                        ILMethod toInline;
                        var pCnt = InitializeFunctionParam(ref op, token, out hasRet, out canInline, out toInline);

                        if (!canInline)
                        {
                            for (int i = pCnt; i > 0; i--)
                            {
                                OpCodes.OpCodeR op2 = new OpCodes.OpCodeR();
                                op2.Code = OpCodes.OpCodeREnum.Push;
                                op2.Register1 = (short)(baseRegIdx - i);
                                lst.Add(op2);
                            }

                            baseRegIdx -= (short)pCnt;

                            if (hasRet)
                                op.Register1 = baseRegIdx++;
                        }
                        else
                        {
                            baseRegIdx -= (short)pCnt;
                            Optimizer.InlineMethod(lst, toInline, baseRegIdx, hasRet);
                            if (hasRet)
                                baseRegIdx++;
                            return;
                        }
                    }
                    break;
                case Code.Ldsfld:
                case Code.Ldsflda:
                    op.Register1 = baseRegIdx++;
                    op.OperandLong = appdomain.GetStaticFieldIndex(token, declaringType, method);
                    break;
                case Code.Stsfld:
                    op.Register1 = --baseRegIdx;
                    op.OperandLong = appdomain.GetStaticFieldIndex(token, declaringType, method);
                    break;
                case Code.Ret:
                    if (hasReturn)
                        op.Register1 = --baseRegIdx;
                    break;
                case Code.Throw:
                    op.Register1 = --baseRegIdx;
                    break;
                case Code.Add:
                case Code.Add_Ovf:
                case Code.Add_Ovf_Un:
                case Code.Sub:
                case Code.Sub_Ovf:
                case Code.Sub_Ovf_Un:
                case Code.Mul:
                case Code.Mul_Ovf:
                case Code.Mul_Ovf_Un:
                case Code.Div:
                case Code.Div_Un:
                case Code.Clt:
                case Code.Clt_Un:
                case Code.Cgt:
                case Code.Cgt_Un:
                case Code.Ceq:
                case Code.Ldelem_I4:
                    op.Register1 = (short)(baseRegIdx - 2); //explicit use dest register for optimization
                    op.Register2 = (short)(baseRegIdx - 2);
                    op.Register3 = (short)(baseRegIdx - 1);
                    baseRegIdx--;
                    break;
                case Code.Nop:
                case Code.Castclass:
                    break;
                case Code.Stloc_0:
                    op.Code = OpCodes.OpCodeREnum.Move;
                    op.Register1 = locVarRegStart;
                    op.Register2 = --baseRegIdx;
                    break;
                case Code.Stloc_1:
                    op.Code = OpCodes.OpCodeREnum.Move;
                    op.Register1 = (short)(locVarRegStart + 1);
                    op.Register2 = --baseRegIdx;
                    break;
                case Code.Stloc_2:
                    op.Code = OpCodes.OpCodeREnum.Move;
                    op.Register1 = (short)(locVarRegStart + 2);
                    op.Register2 = --baseRegIdx;
                    break;
                case Code.Stloc_3:
                    op.Code = OpCodes.OpCodeREnum.Move;
                    op.Register1 = (short)(locVarRegStart + 3);
                    op.Register2 = --baseRegIdx;
                    break;
                case Code.Stloc_S:
                    op.Code = OpCodes.OpCodeREnum.Move;
                    op.Register1 = (short)(locVarRegStart + ((VariableDefinition)ins.Operand).Index);
                    op.Register2 = --baseRegIdx;
                    break;
                case Code.Ldloc_0:
                    op.Code = OpCodes.OpCodeREnum.Move;
                    op.Register1 = baseRegIdx++;
                    op.Register2 = locVarRegStart;
                    break;
                case Code.Ldloc_1:
                    op.Code = OpCodes.OpCodeREnum.Move;
                    op.Register1 = baseRegIdx++;
                    op.Register2 = (short)(locVarRegStart + 1);
                    break;
                case Code.Ldloc_2:
                    op.Code = OpCodes.OpCodeREnum.Move;
                    op.Register1 = baseRegIdx++;
                    op.Register2 = (short)(locVarRegStart + 2);
                    break;
                case Code.Ldloc_3:
                    op.Code = OpCodes.OpCodeREnum.Move;
                    op.Register1 = baseRegIdx++;
                    op.Register2 = (short)(locVarRegStart + 3);
                    break;
                case Code.Ldloc:
                case Code.Ldloc_S:
                    op.Code = OpCodes.OpCodeREnum.Move;
                    op.Register1 = baseRegIdx++;
                    op.Register2 = (short)(locVarRegStart + ((VariableDefinition)ins.Operand).Index);
                    break;
                case Code.Ldloca:
                case Code.Ldloca_S:
                    op.Register1 = baseRegIdx++;
                    op.Register2 = (short)(locVarRegStart + ((VariableDefinition)ins.Operand).Index);
                    break;
                case Code.Ldarg_0:
                case Code.Ldarg_1:
                case Code.Ldarg_2:
                case Code.Ldarg_3:
                    op.Code = OpCodes.OpCodeREnum.Move;
                    op.Register1 = baseRegIdx++;
                    op.Register2 = (short)(code.Code - (Code.Ldarg_0));
                    break;
                case Code.Ldarg_S:
                    op.Register1 = baseRegIdx++;
                    op.Register2 = (short)((ParameterDefinition)ins.Operand).Index;
                    if (def.HasThis)
                    {
                        op.Register2++;
                    }
                    break;
                case Code.Newarr:
                    op.Register1 = (short)(baseRegIdx - 1);
                    op.Register2 = (short)(baseRegIdx - 1);
                    op.Operand = method.GetTypeTokenHashCode(token);
                    break;
                case Code.Dup:
                    op.Code = OpCodes.OpCodeREnum.Move;
                    op.Register2 = (short)(baseRegIdx - 1);
                    op.Register1 = baseRegIdx++;
                    break;
                case Code.Stelem_I:
                case Code.Stelem_I1:
                case Code.Stelem_I2:
                case Code.Stelem_I4:
                case Code.Stelem_I8:
                case Code.Stelem_R4:
                case Code.Stelem_R8:
                case Code.Stelem_Ref:
                case Code.Stelem_Any:
                    op.Register1 = (short)(baseRegIdx - 3);
                    op.Register2 = (short)(baseRegIdx - 2);
                    op.Register3 = (short)(baseRegIdx - 1);
                    baseRegIdx -= 3;
                    break;
                case Code.Stind_I:
                case Code.Stind_I1:
                case Code.Stind_I2:
                case Code.Stind_I4:
                case Code.Stind_I8:
                case Code.Stind_R4:
                case Code.Stind_R8:
                    op.Register1 = (short)(baseRegIdx - 2);
                    op.Register2 = (short)(baseRegIdx - 1);
                    baseRegIdx -= 2;
                    break;
                case Code.Conv_I:
                case Code.Conv_I1:
                case Code.Conv_I2:
                case Code.Conv_I4:
                case Code.Conv_I8:
                case Code.Conv_Ovf_I:
                case Code.Conv_Ovf_I1:
                case Code.Conv_Ovf_I1_Un:
                case Code.Conv_Ovf_I2:
                case Code.Conv_Ovf_I2_Un:
                case Code.Conv_Ovf_I4:
                case Code.Conv_Ovf_I4_Un:
                case Code.Conv_Ovf_I8:
                case Code.Conv_Ovf_I8_Un:
                case Code.Conv_Ovf_I_Un:
                case Code.Conv_Ovf_U:
                case Code.Conv_Ovf_U1:
                case Code.Conv_Ovf_U1_Un:
                case Code.Conv_Ovf_U2:
                case Code.Conv_Ovf_U2_Un:
                case Code.Conv_Ovf_U4:
                case Code.Conv_Ovf_U4_Un:
                case Code.Conv_Ovf_U8:
                case Code.Conv_Ovf_U8_Un:
                case Code.Conv_Ovf_U_Un:
                case Code.Conv_R4:
                case Code.Conv_R8:
                case Code.Conv_R_Un:
                case Code.Conv_U:
                case Code.Conv_U1:
                case Code.Conv_U2:
                case Code.Conv_U4:
                case Code.Conv_U8:
                case Code.Ldlen:
                case Code.Ldind_I:
                case Code.Ldind_I2:
                case Code.Ldind_I4:
                case Code.Ldind_I8:
                case Code.Ldind_R4:
                case Code.Ldind_R8:
                case Code.Ldind_U1:
                case Code.Ldind_U2:
                case Code.Ldind_U4:
                    op.Register1 = (short)(baseRegIdx - 1);
                    op.Register2 = (short)(baseRegIdx - 1);
                    break;
                case Code.Ldfld:
                case Code.Ldflda:
                    op.Register1 = (short)(baseRegIdx - 1);
                    op.Register2 = (short)(baseRegIdx - 1);
                    op.OperandLong = appdomain.GetStaticFieldIndex(token, declaringType, method);
                    break;
                case Code.Stfld:
                    op.Register1 = (short)(baseRegIdx - 2);
                    op.Register2 = (short)(baseRegIdx - 1);
                    op.OperandLong = appdomain.GetStaticFieldIndex(token, declaringType, method);
                    baseRegIdx -= 2;
                    break;
                case Code.Box:
                case Code.Unbox:
                case Code.Unbox_Any:
                case Code.Isinst:
                    op.Register1 = (short)(baseRegIdx - 1);
                    op.Register2 = (short)(baseRegIdx - 1);
                    op.Operand = method.GetTypeTokenHashCode(token);
                    break;
                case Code.Ldtoken:
                    op.Register1 = baseRegIdx++;
                    if (token is FieldReference)
                    {
                        op.Operand = 0;
                        op.OperandLong = appdomain.GetStaticFieldIndex(token, declaringType, method);
                    }
                    else if (token is TypeReference)
                    {
                        op.Operand = 1;
                        op.OperandLong = method.GetTypeTokenHashCode(token);
                    }
                    else
                        throw new NotImplementedException();
                    break;
                case Code.Ldftn:
                    {
                        op.Register1 = baseRegIdx++;
                        bool hasReturn, canInline;
                        ILMethod toInline;
                        InitializeFunctionParam(ref op, token, out hasReturn, out canInline, out toInline);
                    }
                    break;

                case Code.Ldvirtftn:
                    {
                        op.Register1 = (short)(baseRegIdx - 1);
                        op.Register2 = (short)(baseRegIdx - 1);
                        bool hasReturn, canInline;
                        ILMethod toInline;
                        InitializeFunctionParam(ref op, token, out hasReturn, out canInline, out toInline);
                    }
                    break;
                case Code.Pop:
                    baseRegIdx--;
                    return;
                default:
                    throw new NotImplementedException();
            }
            lst.Add(op);
        }

        
        int InitializeFunctionParam(ref OpCodes.OpCodeR op, object token, out bool hasReturn, out bool canInline, out ILMethod toInline)
        {
            bool invalidToken;
            int pCnt = 0;
            var m = appdomain.GetMethod(token, declaringType, method, out invalidToken);
            toInline = null;
            canInline = false;
            if (m != null)
            {
                if (invalidToken)
                    op.Operand = m.GetHashCode();
                else
                    op.Operand = token.GetHashCode();
                pCnt = m.ParameterCount;
                if (!m.IsStatic && op.Code != OpCodeREnum.Newobj)
                    pCnt++;
                hasReturn = m.ReturnType != appdomain.VoidType;
                if(m is ILMethod)
                {
                    if (!m.IsConstructor && !((ILMethod)m).IsVirtual)
                    {
                        var body = ((ILMethod)m).BodyRegister;
                        if (body == null || body.Length <= Optimizer.MaximalInlineInstructionCount)
                        {
                            canInline = true;
                            toInline = (ILMethod)m;
                        }
                    }
                }
            }
            else
            {
                //Cannot find method or the method is dummy
                MethodReference _ref = (MethodReference)token;
                pCnt = _ref.HasParameters ? _ref.Parameters.Count : 0;
                if (_ref.HasThis)
                    pCnt++;
                op.OperandLong = pCnt;
                hasReturn = false;
            }
            return pCnt;
        }
    }
}
