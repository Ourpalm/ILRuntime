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
        public const int CallRegisterParamCount = 3;
        Enviorment.AppDomain appdomain;
        ILType declaringType;
        ILMethod method;
        MethodDefinition def;
        bool hasReturn;
        Dictionary<Instruction, int> entryMapping;
        Dictionary<int, int[]> jumptables;

        public JITCompiler(Enviorment.AppDomain appDomain, ILType declaringType, ILMethod method)
        {
            this.appdomain = appDomain;
            this.declaringType = declaringType;
            this.method = method;
            def = method.Definition;
            hasReturn = method.ReturnType != appdomain.VoidType;
            entryMapping = null;
            jumptables = null;
        }

        public OpCodeR[] Compile(out int stackRegisterCnt, out Dictionary<int, int[]> switchTargets, out Dictionary<int, RegisterVMSymbol> symbols)
        {
#if DEBUG && !DISABLE_ILRUNTIME_DEBUG
            symbols = new Dictionary<int, RegisterVMSymbol>();
#else
            symbols = null;
#endif

            var body = def.Body;
            short locVarRegStart = (short)def.Parameters.Count;
            if (!def.IsStatic)
                locVarRegStart++;
            short baseRegIdx = (short)(locVarRegStart + body.Variables.Count);
            short baseRegStart = baseRegIdx;

            var blocks = CodeBasicBlock.BuildBasicBlocks(body, out entryMapping);

            foreach(var i in blocks)
            {
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
                    Translate(i, ins, locVarRegStart, ref baseRegIdx);
                }
                i.EndRegister = baseRegIdx;
            }
#if OUTPUT_JIT_RESULT
            int cnt = 1;
            Console.WriteLine($"JIT Results for {method}:");
            foreach (var b in blocks)
            {
                Console.WriteLine($"Block {cnt++}, Instructions:{b.FinalInstructions.Count}");
                for(int i = 0; i < b.FinalInstructions.Count; i++)
                {
                    Console.WriteLine($"    {i}:{b.FinalInstructions[i].ToString(appdomain)}");
                }
            }
#endif
            
            Optimizer.ForwardCopyPropagation(blocks, hasReturn, baseRegStart);
            Optimizer.BackwardsCopyPropagation(blocks, hasReturn, baseRegStart);
            Optimizer.ForwardCopyPropagation(blocks, hasReturn, baseRegStart);

#if OUTPUT_JIT_RESULT
            cnt = 1;
            Console.WriteLine($"Optimizer Results for {method}:");
            foreach (var b in blocks)
            {
                Console.WriteLine($"Block {cnt++}, Instructions:{b.FinalInstructions.Count}");
                for (int i = 0; i < b.FinalInstructions.Count; i++)
                {
                    string canRemove = b.CanRemove.Contains(i) ? "(x)" : "";
                    Console.WriteLine($"    {i}:{canRemove}{b.FinalInstructions[i].ToString(appdomain)}");
                }
            }
#endif

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
                    {
                        if (isInline)
                            inlineOffset--;
                        continue;
                    }
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
                            else if(ins.Code == OpCodeREnum.Switch)
                            {
                                int[] targets = jumptables[ins.Operand];
                                for(int j = 0; j < targets.Length; j++)
                                {
                                    targets[j] = targets[j] + inlineOffset;
                                }
                                inlinedBranches.Add(res.Count);
                            }
                        }
#if DEBUG && !DISABLE_ILRUNTIME_DEBUG
                        RegisterVMSymbol oriIns;
                        if (b.InstructionMapping.TryGetValue(idx, out oriIns))
                            symbols.Add(res.Count, oriIns);
#endif
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
                else if(op.Code == OpCodeREnum.Switch && !inlinedBranches.Contains(i))
                {
                    int[] targets = jumptables[op.Operand];
                    for(int j = 0; j < targets.Length; j++)
                    {
                        targets[j] = jumpTargets[targets[j]];
                    }
                }
            }
            switchTargets = jumptables;
            var totalRegCnt = Optimizer.CleanupRegister(res, locVarRegStart, hasReturn);
            stackRegisterCnt = totalRegCnt - baseRegStart;
#if OUTPUT_JIT_RESULT
            Console.WriteLine($"Final Results for {method}:");

            for (int i = 0; i < res.Count; i++)
            {
                Console.WriteLine($"    {i}:{res[i].ToString(appdomain)}");
            }
            
#endif
            return res.ToArray();
        }
        void PrepareJumpTable(object token)
        {
            int hashCode = token.GetHashCode();

            if (jumptables == null)
                jumptables = new Dictionary<int, int[]>();
            if (jumptables.ContainsKey(hashCode))
                return;
            Mono.Cecil.Cil.Instruction[] e = token as Mono.Cecil.Cil.Instruction[];
            int[] addrs = new int[e.Length];
            for (int i = 0; i < e.Length; i++)
            {
                addrs[i] = entryMapping[e[i]];
            }

            jumptables[hashCode] = addrs;
        }
        void Translate(CodeBasicBlock block, Instruction ins, short locVarRegStart, ref short baseRegIdx)
        {
            List<OpCodeR> lst = block.FinalInstructions;
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
                case Code.Switch:
                    op.Register1 = --baseRegIdx;
                    PrepareJumpTable(token);
                    op.Operand = token.GetHashCode();
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
                        bool canInline, isILMethod;
                        ILMethod toInline;
                        var pCnt = InitializeFunctionParam(ref op, token, out hasRet, out canInline, out toInline, out isILMethod);
                        int pushCnt = Math.Max(pCnt - CallRegisterParamCount, 0);
                        for (int i = pCnt; i > pCnt - pushCnt; i--)
                        {
                            OpCodes.OpCodeR op2 = new OpCodes.OpCodeR();
                            op2.Code = OpCodes.OpCodeREnum.Push;
                            op2.Register1 = (short)(baseRegIdx - i);
                            lst.Add(op2);
                        }
                        if (pushCnt < pCnt)
                        {
                            switch (pCnt - pushCnt)
                            {
                                case 1:
                                    op.Register2 = (short)(baseRegIdx - 1);
                                    break;
                                case 2:
                                    op.Register3 = (short)(baseRegIdx - 1);
                                    op.Register2 = (short)(baseRegIdx - 2);
                                    break;
                                case 3:
                                    op.Register4 = (short)(baseRegIdx - 1);
                                    op.Register3 = (short)(baseRegIdx - 2);
                                    op.Register2 = (short)(baseRegIdx - 3);
                                    break;
                            }
                        }
                        baseRegIdx -= (short)pCnt;
                        op.Register1 = baseRegIdx++;
                    }
                    break;
                case Code.Call:
                case Code.Callvirt:
                    {
                        bool canInline, isILMethod;
                        ILMethod toInline;
                        var pCnt = InitializeFunctionParam(ref op, token, out hasRet, out canInline, out toInline, out isILMethod);
                        bool hasConstrained = false;
                        int constrainIdx = -1;
                        if (lst.Count > 0)
                        {
                            constrainIdx = lst.Count - 1;
                            hasConstrained = lst[constrainIdx].Code == OpCodeREnum.Constrained;
                        }
                        if (!canInline || hasConstrained)
                        {
                            int pushCnt = hasConstrained ? pCnt : Math.Max(pCnt - CallRegisterParamCount, 0);
                            for (int i = pCnt; i > pCnt - pushCnt; i--)
                            {
                                OpCodes.OpCodeR op2 = new OpCodes.OpCodeR();
                                op2.Code = OpCodes.OpCodeREnum.Push;
                                op2.Operand = isILMethod ? 1 : 0;
                                op2.Register1 = (short)(baseRegIdx - i);
                                lst.Add(op2);
                            }
                            if (pushCnt < pCnt)
                            {
                                switch(pCnt - pushCnt)
                                {
                                    case 1:
                                        op.Register2 = (short)(baseRegIdx - 1);
                                        break;
                                    case 2:
                                        op.Register3 = (short)(baseRegIdx - 1);
                                        op.Register2 = (short)(baseRegIdx - 2);
                                        break;
                                    case 3:
                                        op.Register4 = (short)(baseRegIdx - 1);
                                        op.Register3 = (short)(baseRegIdx - 2);
                                        op.Register2 = (short)(baseRegIdx - 3);
                                        break;
                                }
                            }
                            if (hasConstrained)
                            {
                                op.Operand4 = 1;
                                var old = lst[constrainIdx];
                                lst.RemoveAt(constrainIdx);
                                old.Operand2 = op.Operand2;
#if DEBUG && !DISABLE_ILRUNTIME_DEBUG
                                var symbol = block.InstructionMapping[constrainIdx];
                                block.InstructionMapping.Remove(constrainIdx);
                                block.InstructionMapping.Add(lst.Count, symbol);
#endif
                                lst.Add(old);
                            }
                            baseRegIdx -= (short)pCnt;

                            if (hasRet)
                                op.Register1 = baseRegIdx++;
                            else
                                op.Register1 = -1;
                        }
                        else
                        {
                            baseRegIdx -= (short)pCnt;
                            RegisterVMSymbolLink link = null;
#if DEBUG && !DISABLE_ILRUNTIME_DEBUG
                            link = new RegisterVMSymbolLink();
                            link.Value.Instruction = ins;
                            link.Value.Method = method;
#endif
                            Optimizer.InlineMethod(block, toInline, link, ref jumptables, baseRegIdx, hasRet);
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
                case Code.Initobj:
                    op.Register1 = --baseRegIdx;
                    op.Operand = method.GetTypeTokenHashCode(token);
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
                case Code.Rem:
                case Code.Rem_Un:
                case Code.Shr:
                case Code.Shr_Un:
                case Code.Shl:
                case Code.Xor:
                case Code.Or:
                case Code.And:
                case Code.Clt:
                case Code.Clt_Un:
                case Code.Cgt:
                case Code.Cgt_Un:
                case Code.Ceq:
                case Code.Ldelema:
                case Code.Ldelem_I1:
                case Code.Ldelem_U1:
                case Code.Ldelem_I2:
                case Code.Ldelem_U2:
                case Code.Ldelem_I4:
                case Code.Ldelem_U4:
                case Code.Ldelem_I8:
                case Code.Ldelem_R4:
                case Code.Ldelem_R8:
                case Code.Ldelem_Any:
                case Code.Ldelem_Ref:
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
                    op.Code = OpCodes.OpCodeREnum.Move;
                    op.Register1 = baseRegIdx++;
                    op.Register2 = (short)((ParameterDefinition)ins.Operand).Index;
                    if (def.HasThis)
                    {
                        op.Register2++;
                    }
                    break;
                case Code.Ldarga:
                case Code.Ldarga_S:
                    op.Register1 = baseRegIdx++;
                    op.Register2 = (short)((ParameterDefinition)ins.Operand).Index;
                    if (def.HasThis)
                    {
                        op.Register2++;
                    }
                    break;
                case Code.Starg:
                case Code.Starg_S:
                    op.Code = OpCodes.OpCodeREnum.Move;
                    op.Register2 = --baseRegIdx;
                    op.Register1 = (short)((ParameterDefinition)ins.Operand).Index;
                    if (def.HasThis)
                    {
                        op.Register1++;
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
                case Code.Stind_Ref:
                    op.Register1 = (short)(baseRegIdx - 2);
                    op.Register2 = (short)(baseRegIdx - 1);
                    baseRegIdx -= 2;
                    break;
                case Code.Stobj:
                    op.Register1 = (short)(baseRegIdx - 2);
                    op.Register2 = (short)(baseRegIdx - 1);
                    op.Operand = method.GetTypeTokenHashCode(token);
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
                case Code.Ldind_Ref:
                    op.Register1 = (short)(baseRegIdx - 1);
                    op.Register2 = (short)(baseRegIdx - 1);
                    break;
                case Code.Ldobj:
                    op.Register1 = (short)(baseRegIdx - 1);
                    op.Register2 = (short)(baseRegIdx - 1);
                    op.Operand = method.GetTypeTokenHashCode(token);
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
                case Code.Constrained:
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
                        bool hasReturn, canInline, isILMethod;
                        ILMethod toInline;
                        InitializeFunctionParam(ref op, token, out hasReturn, out canInline, out toInline, out isILMethod);
                    }
                    break;

                case Code.Ldvirtftn:
                    {
                        bool hasReturn, canInline, isILMethod;
                        ILMethod toInline;
                        InitializeFunctionParam(ref op, token, out hasReturn, out canInline, out toInline, out isILMethod);
                        op.Register1 = (short)(baseRegIdx - 1);
                        op.Register2 = (short)(baseRegIdx - 1);
                    }
                    break;
                case Code.Pop:
                    baseRegIdx--;
                    return;
                default:
                    throw new NotImplementedException(string.Format("Unknown Opcode:{0}", code.Code));
            }
#if DEBUG && !DISABLE_ILRUNTIME_DEBUG
            RegisterVMSymbol s = new RegisterVMSymbol()
            {
                Instruction = ins,
                Method = method
            };
            block.InstructionMapping.Add(lst.Count, s);
#endif
            lst.Add(op);
        }

        
        int InitializeFunctionParam(ref OpCodes.OpCodeR op, object token, out bool hasReturn, out bool canInline, out ILMethod toInline, out bool isILMethod)
        {
            bool invalidToken;
            int pCnt = 0;
            var m = appdomain.GetMethod(token, declaringType, method, out invalidToken);
            toInline = null;
            canInline = false;
            op.Register2 = -1;
            op.Register3 = -1;
            op.Register4 = -1;
            if (m != null)
            {
                if (invalidToken)
                    op.Operand2 = m.GetHashCode();
                else
                    op.Operand2 = token.GetHashCode();
                pCnt = m.ParameterCount;
                if (!m.IsStatic && op.Code != OpCodeREnum.Newobj)
                    pCnt++;
                hasReturn = m.ReturnType != appdomain.VoidType;
                if (m is ILMethod)
                {
                    isILMethod = !m.IsDelegateInvoke;
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
                else
                    isILMethod = false;
            }
            else
            {
                isILMethod = false;
                //Cannot find method or the method is dummy
                MethodReference _ref = (MethodReference)token;
                pCnt = _ref.HasParameters ? _ref.Parameters.Count : 0;
                if (_ref.HasThis && op.Code != OpCodeREnum.Newobj)
                    pCnt++;
                op.Operand3 = pCnt;
                hasReturn = false;
            }
            return pCnt;
        }
    }
}
