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
#if ENABLE_NEO_MODE
    internal enum NeoPrimitiveTypeTag : int
    {
        I4 = 0,
        U4 = 1,
        I8 = 2,
        U8 = 3,
        R4 = 4,
        R8 = 5,
    }
#endif
    struct StackSlotInfo
    {
        public int Offset;
        public int RefOffset;
        public int Size;        
    }
    struct CompiledFrame
    {
        public OpCodeR[] CodeBody;
        public int StackRegisterCount;
        public Dictionary<int, int[]> SwitchTargets;
        public Dictionary<int, RegisterVMSymbol> Symbols;
        public StackSlotInfo[] LocalInfos;
        public int TotalStructSize;
        public int TotalRefSize;
#if ENABLE_NEO_MODE
        public StackSlotInfo[] ParamInfos;
        public int ParamPrimitiveSize;
        public int ParamReferenceCount;
        public int LocalsPrimitiveSize;
        public int LocalsReferenceCount;
        public int ReturnPrimitiveSize;
        public int ReturnRefCount;
        public bool[] LocalIsReference;
        // Body executed by ExecuteNeo. Same opcode shape as CodeBody but with
        // Register1/2/3 lowered to byte offsets via LowerNeoOffsets. CodeBody
        // itself stays in register-index form so inliner / debugger / future
        // AOT serialization can keep operating on a stable representation.
        public OpCodeR[] NeoExecuteBody;
#endif
    }
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

        bool CheckNeedInitObj(CodeBasicBlock block, short reg, bool hasReturn, HashSet<CodeBasicBlock> visited)
        {
            if (visited.Contains(block))
                return false;
            visited.Add(block);
            for (int i = 0; i < block.FinalInstructions.Count; i++)
            {
                var ins = block.FinalInstructions[i];
                short r1, r2, r3, rw;
                Optimizer.GetOpcodeDestRegister(ref ins, out rw);
                if (Optimizer.GetOpcodeSourceRegister(ref ins, hasReturn, out r1, out r2, out r3))
                {
                    if (r1 == reg || r2 == reg || r3 == reg)
                    {
                        if (ins.Code == OpCodeREnum.Ldloca || ins.Code == OpCodeREnum.Ldloca_S)
                        {
                            if (i < block.FinalInstructions.Count - 1)
                            {
                                var next = block.FinalInstructions[i + 1];
                                if (next.Code == OpCodeREnum.Initobj && next.Register1 == rw)
                                    return false;
                            }
                            return true;
                        }
                        else
                            return rw != reg;
                    }
                }
                if (rw == reg)
                    return false;
            }
            if (block.NextBlocks != null && block.NextBlocks.Count > 0)
            {
                foreach (var i in block.NextBlocks)
                {
                    if (CheckNeedInitObj(i, reg, hasReturn, visited))
                        return true;
                }
            }

            return false;
        }

        bool IsCatchHandler(CodeBasicBlock block, MethodBody body)
        {
            if (body.HasExceptionHandlers)
            {
                var firstIns = block.Instructions[0];
                foreach(var eh in body.ExceptionHandlers)
                {
                    if(eh.HandlerType == Mono.Cecil.Cil.ExceptionHandlerType.Catch)
                    {
                        if (eh.HandlerStart == firstIns)
                            return true;
                    }
                }
                return false;
            }
            else
                return false;
        }

#if ENABLE_NEO_MODE
        List<IType> GatherValueTypes(ref CompiledFrame frame)
        {
            List<IType> valueTypes = new List<IType>();
            var body = frame.CodeBody;
            var domain = method.AppDomain;
            for (int i = 0; i < body.Length; i++)
            {
                var code = body[i];
                IType type = null;
                switch (code.Code)
                {
                    case OpCodeREnum.Ldobj:
                    case OpCodeREnum.Stobj:
                    case OpCodeREnum.Box:
                    case OpCodeREnum.Unbox:
                    case OpCodeREnum.Unbox_Any:
                    case OpCodeREnum.Isinst:
                    case OpCodeREnum.Castclass:
                    case OpCodeREnum.Constrained:
                    case OpCodeREnum.Sizeof:
                    case OpCodeREnum.Newarr:
                    case OpCodeREnum.Ldfld_Ref:
                    case OpCodeREnum.Ldfld_Value:
                    case OpCodeREnum.Stfld_Ref:
                    case OpCodeREnum.Stfld_Value:
                        type = domain.GetType(code.Operand);
                        break;
                    case OpCodeREnum.Ldfld:
                    case OpCodeREnum.Stfld:
                    case OpCodeREnum.Ldsfld:
                    case OpCodeREnum.Stsfld:
                        type = domain.GetType((int)(code.OperandLong >> 32));
                        break;
                }
                if (type != null && type.IsValueType && !type.IsPrimitive)
                {
                    valueTypes.Add(type);
                }
            }
            return valueTypes;
        }
#endif

        public CompiledFrame Compile(Dictionary<Instruction, int> addr)
        {
#if DEBUG && !NO_PROFILER
            if (System.Threading.Thread.CurrentThread.ManagedThreadId == method.AppDomain.UnityMainThreadID)

#if UNITY_5_5_OR_NEWER
                UnityEngine.Profiling.Profiler.BeginSample("JITCompiler.Compile");
#else
                UnityEngine.Profiler.BeginSample("JITCompiler.Compile");
#endif

#endif
            method.Compiling = true;
            Dictionary<int, RegisterVMSymbol> symbols = new Dictionary<int, RegisterVMSymbol>();

            var body = def.Body;
            short locVarRegStart = (short)def.Parameters.Count;
            if (!def.IsStatic)
                locVarRegStart++;
            short baseRegIdx = (short)(locVarRegStart + body.Variables.Count);
            short baseRegStart = baseRegIdx;

            var blocks = CodeBasicBlock.BuildBasicBlocks(body, out entryMapping);

            foreach (var i in blocks)
            {
                baseRegIdx = baseRegStart;
                if (IsCatchHandler(i, body))
                    baseRegIdx++;
                else
                {
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
                }
                foreach (var ins in i.Instructions)
                {
                    Translate(i, ins, locVarRegStart, ref baseRegIdx);
                }
                i.EndRegister = baseRegIdx;
            }

            //Append init local
            var first = blocks[0];
            int idx = 0;
            int appendIdx = 0;
            HashSet<CodeBasicBlock> visitedBlocks = body.Variables.Count > 0 ? new HashSet<CodeBasicBlock>() : null;
            for (short r = locVarRegStart; r < locVarRegStart + body.Variables.Count; r++)
            {
                visitedBlocks.Clear();
                foreach (var b in blocks)
                {
                    if (b.PreviousBlocks.Count == 0)
                    {
                        var lt = def.Body.Variables[r - locVarRegStart];
                        bool needInitOjb = false;
                        if (lt.VariableType.IsGenericParameter)
                        {
                            var gt = method.FindGenericArgument(lt.VariableType.Name);
                            needInitOjb = gt.IsValueType && !gt.IsPrimitive;
                        }
                        else
                            needInitOjb = lt.VariableType.IsValueType && !lt.VariableType.IsPrimitive;
                        if (needInitOjb || CheckNeedInitObj(b, r, method.ReturnType != method.AppDomain.VoidType, visitedBlocks))
                        {
                            OpCodeR code = new OpCodeR();
                            code.Code = OpCodeREnum.Initobj;
                            code.Register1 = r;
                            code.Operand = method.GetTypeTokenHashCode(body.Variables[idx].VariableType);
                            code.Operand2 = 1;
                            first.FinalInstructions.Insert(appendIdx++, code);
                            break;
                        }
                    }
                }

                idx++;
            }
            for (idx = first.FinalInstructions.Count - 1; idx >= 0; idx--)
            {
                if (idx >= appendIdx)
                {
                    RegisterVMSymbol symbol;

                    if (first.InstructionMapping.TryGetValue(idx - appendIdx, out symbol))
                    {
                        first.InstructionMapping[idx] = first.InstructionMapping[idx - appendIdx];
                    }
                }
                else
                    first.InstructionMapping.Remove(idx);
            }

#if OUTPUT_JIT_RESULT
            int cnt = 1;
            Console.WriteLine($"JIT Results for {method}:");
            foreach (var b in blocks)
            {
                Console.WriteLine($"Block {cnt++}, Instructions:{b.FinalInstructions.Count}");
                for (int i = 0; i < b.FinalInstructions.Count; i++)
                {
                    Console.WriteLine($"    {i}:{b.FinalInstructions[i].ToString(appdomain)}");
                }
            }
#endif

            Optimizer.ForwardCopyPropagation(blocks, hasReturn, baseRegStart);
            Optimizer.BackwardsCopyPropagation(blocks, hasReturn, baseRegStart);
            Optimizer.ForwardCopyPropagation(blocks, hasReturn, baseRegStart);
            Optimizer.EliminateConstantLoad(blocks, hasReturn);

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
            int curIndex = 0;
            foreach (var b in blocks)
            {
                jumpTargets[bIdx++] = res.Count;
                bool isInline = false;
                int inlineOffset = 0;
                bool inlineAddressSet = false;
                for (idx = 0; idx < b.FinalInstructions.Count; idx++)
                {
                    RegisterVMSymbol oriIns;
                    bool hasOri = b.InstructionMapping.TryGetValue(idx, out oriIns);
                    if (hasOri)
                    {
                        if (isInline)
                        {
                            if (!inlineAddressSet)
                            {
                                while (oriIns.ParentSymbol != null)
                                    oriIns = oriIns.ParentSymbol.Value;
                                addr[oriIns.Instruction] = curIndex;
                                inlineAddressSet = true;
                            }
                        }
                        else
                            addr[oriIns.Instruction] = curIndex;
                    }
                    if (b.CanRemove.Contains(idx))
                    {
                        if (isInline)
                            inlineOffset--;
                        continue;
                    }
                    var ins = b.FinalInstructions[idx];
                    if (ins.Code == OpCodeREnum.InlineStart)
                    {
                        inlineAddressSet = false;
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
                            else if (Optimizer.IsIntermediateBranching(ins.Code))
                            {
                                ins.Operand4 += inlineOffset;
                                inlinedBranches.Add(res.Count);
                            }
                            else if (ins.Code == OpCodeREnum.Switch)
                            {
                                int[] targets = jumptables[ins.Operand];
                                for (int j = 0; j < targets.Length; j++)
                                {
                                    targets[j] = targets[j] + inlineOffset;
                                }
                                inlinedBranches.Add(res.Count);
                            }
                        }
                        if (hasOri)
                            symbols.Add(res.Count, oriIns);
                        curIndex++;
                        res.Add(ins);
                    }
                }
            }
            for (int i = 0; i < res.Count; i++)
            {
                var op = res[i];
                if (Optimizer.IsBranching(op.Code) && !inlinedBranches.Contains(i))
                {
                    op.Operand = jumpTargets[op.Operand];
                    res[i] = op;
                }
                else if (Optimizer.IsIntermediateBranching(op.Code) && !inlinedBranches.Contains(i))
                {
                    op.Operand4 = jumpTargets[op.Operand4];
                    res[i] = op;
                }
                else if (op.Code == OpCodeREnum.Switch && !inlinedBranches.Contains(i))
                {
                    int[] targets = jumptables[op.Operand];
                    for (int j = 0; j < targets.Length; j++)
                    {
                        targets[j] = jumpTargets[targets[j]];
                    }
                }
                else if(op.Code == OpCodeREnum.Leave || op.Code == OpCodeREnum.Leave_S)
                {
                    var oriIns = symbols[i];
                    op.Operand = addr[(Instruction)oriIns.Instruction.Operand];
                    res[i] = op;
                }
            }
#if DEBUG && !DISABLE_ILRUNTIME_DEBUG
            //FixSymbol(symbols);
#else
            symbols = null;
#endif
            CompiledFrame frame = new CompiledFrame();
            frame.SwitchTargets = jumptables;
            frame.Symbols = symbols;
            var totalRegCnt = Optimizer.CleanupRegister(res, locVarRegStart, hasReturn);
            frame.StackRegisterCount = Math.Max(totalRegCnt - baseRegStart, 0);
#if ENABLE_NEO_MODE
            TypeSpecializeNeoOpcodes(res, locVarRegStart, totalRegCnt);
#endif
#if OUTPUT_JIT_RESULT
            Console.WriteLine($"Final Results for {method}:");

            for (int i = 0; i < res.Count; i++)
            {
                Console.WriteLine($"    {i}:{res[i].ToString(appdomain)}");
            }

#endif
            method.Compiling = false;
            frame.CodeBody = res.ToArray();
#if ENABLE_NEO_MODE
            AllocateLocalStackSpaces(ref frame);
            // Keep frame.CodeBody in register-index form (used by inliner,
            // debugger, optimization passes when this method is later inlined).
            // ExecuteNeo runs against a lowered copy where Register1/2/3 hold
            // byte offsets after LowerNeoOffsets.
            frame.NeoExecuteBody = (OpCodeR[])frame.CodeBody.Clone();
            LowerNeoOffsets(ref frame);
#endif

#if DEBUG && !NO_PROFILER
            if (System.Threading.Thread.CurrentThread.ManagedThreadId == method.AppDomain.UnityMainThreadID)
#if UNITY_5_5_OR_NEWER
                UnityEngine.Profiling.Profiler.EndSample();
#else
                UnityEngine.Profiler.EndSample();
#endif
#endif
            return frame;
        }

#if ENABLE_NEO_MODE
        void TypeSpecializeNeoOpcodes(List<OpCodeR> body, short locVarRegStart, int totalRegCnt)
        {
            IType[] registerTypes = BuildInitialRegisterTypes(locVarRegStart, totalRegCnt);
            for (int i = 0; i < body.Count; i++)
            {
                OpCodeR op = body[i];
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
                        SetRegisterType(registerTypes, op.Register1, appdomain.IntType);
                        break;
                    case OpCodeREnum.Ldc_I8:
                        SetRegisterType(registerTypes, op.Register1, appdomain.LongType);
                        break;
                    case OpCodeREnum.Ldc_R4:
                        SetRegisterType(registerTypes, op.Register1, appdomain.FloatType);
                        break;
                    case OpCodeREnum.Ldc_R8:
                        SetRegisterType(registerTypes, op.Register1, appdomain.DoubleType);
                        break;
                    case OpCodeREnum.Ldnull:
                        SetRegisterType(registerTypes, op.Register1, appdomain.ObjectType);
                        break;
                    case OpCodeREnum.Move:
                        SetRegisterType(registerTypes, op.Register1, GetRegisterType(registerTypes, op.Register2));
                        break;
                    case OpCodeREnum.Neg:
                    case OpCodeREnum.Not:
                        op.Code = GetTypedUnaryOpcode(op.Code, InferPrimTag(GetRegisterType(registerTypes, op.Register2), appdomain));
                        SetRegisterType(registerTypes, op.Register1, GetRegisterType(registerTypes, op.Register2));
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
                        op.Code = GetTypedBinaryOpcode(op.Code, InferPrimTag(GetRegisterType(registerTypes, op.Register2), appdomain));
                        SetRegisterType(registerTypes, op.Register1, GetRegisterType(registerTypes, op.Register2));
                        break;
                    case OpCodeREnum.Ceq:
                    case OpCodeREnum.Cgt:
                    case OpCodeREnum.Cgt_Un:
                    case OpCodeREnum.Clt:
                    case OpCodeREnum.Clt_Un:
                        op.Code = GetTypedCompareOpcode(op.Code, InferPrimTag(GetRegisterType(registerTypes, op.Register2), appdomain));
                        SetRegisterType(registerTypes, op.Register1, appdomain.IntType);
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
                        op.Code = GetTypedBranchOpcode(NormalizeBranchOpcode(op.Code), InferPrimTag(GetRegisterType(registerTypes, op.Register1), appdomain));
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
                        op.Code = GetTypedImmediateBinaryOpcode(op.Code, InferPrimTag(GetRegisterType(registerTypes, op.Register2), appdomain));
                        SetRegisterType(registerTypes, op.Register1, GetRegisterType(registerTypes, op.Register2));
                        break;
                    case OpCodeREnum.Ceqi:
                    case OpCodeREnum.Cgti:
                    case OpCodeREnum.Cgti_Un:
                    case OpCodeREnum.Clti:
                    case OpCodeREnum.Clti_Un:
                        op.Code = GetTypedImmediateCompareOpcode(op.Code, InferPrimTag(GetRegisterType(registerTypes, op.Register2), appdomain));
                        SetRegisterType(registerTypes, op.Register1, appdomain.IntType);
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
                        op.Code = GetTypedImmediateBranchOpcode(op.Code, InferPrimTag(GetRegisterType(registerTypes, op.Register1), appdomain));
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
                        op.Operand2 = (int)InferPrimTag(GetRegisterType(registerTypes, op.Register2), appdomain);
                        SetRegisterType(registerTypes, op.Register1, GetConvResultType(op.Code));
                        break;
                    case OpCodeREnum.Ldfld_I1:
                    case OpCodeREnum.Ldfld_I2:
                    case OpCodeREnum.Ldfld_I4:
                    case OpCodeREnum.Ldfld_U1:
                    case OpCodeREnum.Ldfld_U2:
                    case OpCodeREnum.Ldfld_U4:
                    case OpCodeREnum.Ldlen:
                        SetRegisterType(registerTypes, op.Register1, appdomain.IntType);
                        break;
                    case OpCodeREnum.Ldfld_I8:
                    case OpCodeREnum.Ldfld_U8:
                        SetRegisterType(registerTypes, op.Register1, appdomain.LongType);
                        break;
                    case OpCodeREnum.Ldfld_R4:
                        SetRegisterType(registerTypes, op.Register1, appdomain.FloatType);
                        break;
                    case OpCodeREnum.Ldfld_R8:
                        SetRegisterType(registerTypes, op.Register1, appdomain.DoubleType);
                        break;
                }
                body[i] = op;
            }
        }

        IType[] BuildInitialRegisterTypes(short locVarRegStart, int totalRegCnt)
        {
            IType[] registerTypes = new IType[totalRegCnt];
            int idx = 0;
            if (method.HasThis)
                registerTypes[idx++] = declaringType;
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

        IType GetConvResultType(OpCodeREnum code)
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

            // 解 enum 到 underlying type
            if (t is ILType ilt && ilt.IsEnum)
            {
                var fts = ilt.FieldTypes;
                if (fts != null && fts.Length > 0)
                    t = fts[0];
            }

            // 通过 TypeForCLR 比对 CLR 基本类型
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

            // fallback
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

        void LowerNeoOffsets(ref CompiledFrame frame)
        {
            if (frame.TotalStructSize > ushort.MaxValue)
            {
                throw new NotSupportedException(string.Format("Neo frame primitive size {0} exceeds maximum byte offset {1}.", frame.TotalStructSize, ushort.MaxValue));
            }

            var localInfos = frame.LocalInfos;
            var body = frame.NeoExecuteBody;
            for (int i = 0; i < body.Length; i++)
            {
                OpCodeR op = body[i];
                bool handled = true;
                switch (op.Code)
                {
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
                    case OpCodeREnum.Ceq:
                    case OpCodeREnum.Cgt:
                    case OpCodeREnum.Cgt_Un:
                    case OpCodeREnum.Clt:
                    case OpCodeREnum.Clt_Un:
                    case OpCodeREnum.Add_I8:
                    case OpCodeREnum.Sub_I8:
                    case OpCodeREnum.Mul_I8:
                    case OpCodeREnum.Div_I8:
                    case OpCodeREnum.Div_Un_I8:
                    case OpCodeREnum.Rem_I8:
                    case OpCodeREnum.Rem_Un_I8:
                    case OpCodeREnum.And_I8:
                    case OpCodeREnum.Or_I8:
                    case OpCodeREnum.Xor_I8:
                    case OpCodeREnum.Shl_I8:
                    case OpCodeREnum.Shr_I8:
                    case OpCodeREnum.Shr_Un_I8:
                    case OpCodeREnum.Add_R4:
                    case OpCodeREnum.Sub_R4:
                    case OpCodeREnum.Mul_R4:
                    case OpCodeREnum.Div_R4:
                    case OpCodeREnum.Rem_R4:
                    case OpCodeREnum.Add_R8:
                    case OpCodeREnum.Sub_R8:
                    case OpCodeREnum.Mul_R8:
                    case OpCodeREnum.Div_R8:
                    case OpCodeREnum.Rem_R8:
                    case OpCodeREnum.Ceq_I8:
                    case OpCodeREnum.Cgt_I8:
                    case OpCodeREnum.Cgt_Un_I8:
                    case OpCodeREnum.Clt_I8:
                    case OpCodeREnum.Clt_Un_I8:
                    case OpCodeREnum.Ceq_R4:
                    case OpCodeREnum.Cgt_R4:
                    case OpCodeREnum.Cgt_Un_R4:
                    case OpCodeREnum.Clt_R4:
                    case OpCodeREnum.Clt_Un_R4:
                    case OpCodeREnum.Ceq_R8:
                    case OpCodeREnum.Cgt_R8:
                    case OpCodeREnum.Cgt_Un_R8:
                    case OpCodeREnum.Clt_R8:
                    case OpCodeREnum.Clt_Un_R8:
                        LowerR1R2R3(ref op, localInfos);
                        break;

                    case OpCodeREnum.Neg:
                    case OpCodeREnum.Not:
                    case OpCodeREnum.Neg_I8:
                    case OpCodeREnum.Not_I8:
                    case OpCodeREnum.Neg_R4:
                    case OpCodeREnum.Neg_R8:
                        LowerR1R2(ref op, localInfos);
                        break;
                    case OpCodeREnum.Move:
                        // Move emits Unsafe.CopyBlock at runtime, so it needs the
                        // copy size (in bytes) in Operand2. Use min(src,dst) so a
                        // wide stack register (8 bytes) copied into a narrow local
                        // (e.g. int = 4 bytes) doesn't clobber neighbouring slots.
                        {
                            int srcReg = op.Register2;
                            int dstReg = op.Register1;
                            int srcSz = (srcReg >= 0 && srcReg < localInfos.Length) ? localInfos[srcReg].Size : 0;
                            int dstSz = (dstReg >= 0 && dstReg < localInfos.Length) ? localInfos[dstReg].Size : 0;
                            int sz;
                            if (srcSz > 0 && dstSz > 0)
                                sz = srcSz < dstSz ? srcSz : dstSz;
                            else
                                sz = srcSz > 0 ? srcSz : dstSz;
                            LowerR1R2(ref op, localInfos);
                            op.Operand2 = sz;
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
                    case OpCodeREnum.Ceqi:
                    case OpCodeREnum.Cgti:
                    case OpCodeREnum.Cgti_Un:
                    case OpCodeREnum.Clti:
                    case OpCodeREnum.Clti_Un:
                    case OpCodeREnum.Addi_I8:
                    case OpCodeREnum.Subi_I8:
                    case OpCodeREnum.Muli_I8:
                    case OpCodeREnum.Divi_I8:
                    case OpCodeREnum.Divi_Un_I8:
                    case OpCodeREnum.Remi_I8:
                    case OpCodeREnum.Remi_Un_I8:
                    case OpCodeREnum.Andi_I8:
                    case OpCodeREnum.Ori_I8:
                    case OpCodeREnum.Xori_I8:
                    case OpCodeREnum.Shli_I8:
                    case OpCodeREnum.Shri_I8:
                    case OpCodeREnum.Shri_Un_I8:
                    case OpCodeREnum.Addi_R4:
                    case OpCodeREnum.Subi_R4:
                    case OpCodeREnum.Muli_R4:
                    case OpCodeREnum.Divi_R4:
                    case OpCodeREnum.Remi_R4:
                    case OpCodeREnum.Addi_R8:
                    case OpCodeREnum.Subi_R8:
                    case OpCodeREnum.Muli_R8:
                    case OpCodeREnum.Divi_R8:
                    case OpCodeREnum.Remi_R8:
                    case OpCodeREnum.Ceqi_I8:
                    case OpCodeREnum.Cgti_I8:
                    case OpCodeREnum.Cgti_Un_I8:
                    case OpCodeREnum.Clti_I8:
                    case OpCodeREnum.Clti_Un_I8:
                    case OpCodeREnum.Ceqi_R4:
                    case OpCodeREnum.Cgti_R4:
                    case OpCodeREnum.Cgti_Un_R4:
                    case OpCodeREnum.Clti_R4:
                    case OpCodeREnum.Clti_Un_R4:
                    case OpCodeREnum.Ceqi_R8:
                    case OpCodeREnum.Cgti_R8:
                    case OpCodeREnum.Cgti_Un_R8:
                    case OpCodeREnum.Clti_R8:
                    case OpCodeREnum.Clti_Un_R8:
                        LowerR1R2(ref op, localInfos);
                        break;

                    case OpCodeREnum.Beq:
                    case OpCodeREnum.Bne_Un:
                    case OpCodeREnum.Blt:
                    case OpCodeREnum.Blt_Un:
                    case OpCodeREnum.Bgt:
                    case OpCodeREnum.Bgt_Un:
                    case OpCodeREnum.Ble:
                    case OpCodeREnum.Ble_Un:
                    case OpCodeREnum.Bge:
                    case OpCodeREnum.Bge_Un:
                    case OpCodeREnum.Beq_I8:
                    case OpCodeREnum.Bne_Un_I8:
                    case OpCodeREnum.Blt_I8:
                    case OpCodeREnum.Blt_Un_I8:
                    case OpCodeREnum.Bgt_I8:
                    case OpCodeREnum.Bgt_Un_I8:
                    case OpCodeREnum.Ble_I8:
                    case OpCodeREnum.Ble_Un_I8:
                    case OpCodeREnum.Bge_I8:
                    case OpCodeREnum.Bge_Un_I8:
                    case OpCodeREnum.Beq_R4:
                    case OpCodeREnum.Bne_Un_R4:
                    case OpCodeREnum.Blt_R4:
                    case OpCodeREnum.Blt_Un_R4:
                    case OpCodeREnum.Bgt_R4:
                    case OpCodeREnum.Bgt_Un_R4:
                    case OpCodeREnum.Ble_R4:
                    case OpCodeREnum.Ble_Un_R4:
                    case OpCodeREnum.Bge_R4:
                    case OpCodeREnum.Bge_Un_R4:
                    case OpCodeREnum.Beq_R8:
                    case OpCodeREnum.Bne_Un_R8:
                    case OpCodeREnum.Blt_R8:
                    case OpCodeREnum.Blt_Un_R8:
                    case OpCodeREnum.Bgt_R8:
                    case OpCodeREnum.Bgt_Un_R8:
                    case OpCodeREnum.Ble_R8:
                    case OpCodeREnum.Ble_Un_R8:
                    case OpCodeREnum.Bge_R8:
                    case OpCodeREnum.Bge_Un_R8:
                        LowerR1R2(ref op, localInfos);
                        break;

                    case OpCodeREnum.Brtrue:
                    case OpCodeREnum.Brtrue_S:
                    case OpCodeREnum.Brfalse:
                    case OpCodeREnum.Brfalse_S:
                        {
                            short r1 = op.Register1;
                            int size = localInfos[r1].Size;
                            int off1 = localInfos[r1].Offset;
                            op.Operand2 = size;
                            op.DstOffset = (ushort)off1;
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
                    case OpCodeREnum.Beqi_I8:
                    case OpCodeREnum.Bnei_Un_I8:
                    case OpCodeREnum.Blti_I8:
                    case OpCodeREnum.Blti_Un_I8:
                    case OpCodeREnum.Bgti_I8:
                    case OpCodeREnum.Bgti_Un_I8:
                    case OpCodeREnum.Blei_I8:
                    case OpCodeREnum.Blei_Un_I8:
                    case OpCodeREnum.Bgei_I8:
                    case OpCodeREnum.Bgei_Un_I8:
                    case OpCodeREnum.Beqi_R4:
                    case OpCodeREnum.Bnei_Un_R4:
                    case OpCodeREnum.Blti_R4:
                    case OpCodeREnum.Blti_Un_R4:
                    case OpCodeREnum.Bgti_R4:
                    case OpCodeREnum.Bgti_Un_R4:
                    case OpCodeREnum.Blei_R4:
                    case OpCodeREnum.Blei_Un_R4:
                    case OpCodeREnum.Bgei_R4:
                    case OpCodeREnum.Bgei_Un_R4:
                    case OpCodeREnum.Beqi_R8:
                    case OpCodeREnum.Bnei_Un_R8:
                    case OpCodeREnum.Blti_R8:
                    case OpCodeREnum.Blti_Un_R8:
                    case OpCodeREnum.Bgti_R8:
                    case OpCodeREnum.Bgti_Un_R8:
                    case OpCodeREnum.Blei_R8:
                    case OpCodeREnum.Blei_Un_R8:
                    case OpCodeREnum.Bgei_R8:
                    case OpCodeREnum.Bgei_Un_R8:
                    case OpCodeREnum.Initobj:
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
                    case OpCodeREnum.Ldc_I8:
                    case OpCodeREnum.Ldc_R4:
                    case OpCodeREnum.Ldc_R8:
                        LowerR1(ref op, localInfos);
                        break;
                    case OpCodeREnum.Ret:
                        if (op.Register1 >= 0)
                            LowerR1(ref op, localInfos);
                        break;
                    case OpCodeREnum.Box:
                    case OpCodeREnum.Unbox:
                    case OpCodeREnum.Unbox_Any:
                        {
                            short r1 = op.Register1;
                            short r2 = op.Register2;
                            int off1 = localInfos[r1].Offset;
                            int off2 = localInfos[r2].Offset;
                            int ref1 = localInfos[r1].RefOffset;
                            int ref2 = localInfos[r2].RefOffset;
                            op.DstOffset = (ushort)off1;
                            op.SrcOffset = (ushort)off2;
                            op.Operand3 = (ref1 << 16) | (ref2 & 0xffff);
                        }
                        break;
                    case OpCodeREnum.Br:
                    case OpCodeREnum.Br_S:
                    case OpCodeREnum.Nop:
                        break;
                    default:
                        handled = false;
                        break;
                }
                WarnUnhandledNeoLoweringOpcode(op.Code, handled);
                body[i] = op;
            }
        }

        static void LowerR1(ref OpCodeR op, StackSlotInfo[] localInfos)
        {
            short r1 = op.Register1;
            int off1 = localInfos[r1].Offset;
            op.DstOffset = (ushort)off1;
        }

        static void LowerR1R2(ref OpCodeR op, StackSlotInfo[] localInfos)
        {
            short r1 = op.Register1;
            short r2 = op.Register2;
            int off1 = localInfos[r1].Offset;
            int off2 = localInfos[r2].Offset;
            op.DstOffset = (ushort)off1;
            op.SrcOffset = (ushort)off2;
        }

        static void LowerR1R2R3(ref OpCodeR op, StackSlotInfo[] localInfos)
        {
            short r1 = op.Register1;
            short r2 = op.Register2;
            short r3 = op.Register3;
            int off1 = localInfos[r1].Offset;
            int off2 = localInfos[r2].Offset;
            int off3 = localInfos[r3].Offset;
            op.DstOffset = (ushort)off1;
            op.SrcOffset = (ushort)off2;
            op.Register3 = unchecked((short)off3);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        static void WarnUnhandledNeoLoweringOpcode(OpCodeREnum code, bool handled)
        {
            if (!handled)
                System.Diagnostics.Debug.WriteLine(string.Format("Neo lowering skipped opcode {0}.", code));
        }

        void AllocateLocalStackSpaces(ref CompiledFrame frame)
        {
            var body = def.Body;
            int varCnt = body.Variables.Count;

            // 1) Parameter slots
            int paramCnt = method.ParameterCount + (method.HasThis ? 1 : 0);
            StackSlotInfo[] paramInfo = new StackSlotInfo[paramCnt];
            int offset = 0;
            int refOffset = 0;
            int paramIdx = 0;
            if (method.HasThis)
            {
                StackSlotInfo slot = default;
                if (declaringType.IsValueType)
                {
                    int size = declaringType.TotalPrimitiveSize;
                    int refSize = declaringType.TotalReferenceCount;
                    slot.Offset = offset;
                    slot.RefOffset = refOffset;
                    slot.Size = size;
                    offset += size;
                    refOffset += refSize;
                }
                else
                {
                    slot.Offset = offset;
                    slot.RefOffset = refOffset;
                    slot.Size = 0;
                    refOffset++;
                }
                paramInfo[paramIdx++] = slot;
            }
            for (int i = 0; i < method.ParameterCount; i++)
            {
                var pDef = def.Parameters[i];
                var pt = appdomain.GetType(pDef.ParameterType, declaringType, method);
                StackSlotInfo slot = AllocateSlotForType(pt, ref offset, ref refOffset);
                paramInfo[paramIdx++] = slot;
            }
            frame.ParamInfos = paramInfo;
            frame.ParamPrimitiveSize = offset;
            frame.ParamReferenceCount = refOffset;

            int localsPrimStart = offset;
            int localsRefStart = refOffset;

            // 2) Local slots
            int baseRegStart = paramCnt + varCnt;
            int locVarRegStart = paramCnt;
            StackSlotInfo[] localInfo = new StackSlotInfo[paramCnt + varCnt + frame.StackRegisterCount];
            bool[] localIsRef = new bool[localInfo.Length];
            for (int i = 0; i < paramCnt; i++)
            {
                localInfo[i] = paramInfo[i];
            }
            for (int i = 0; i < varCnt; i++)
            {
                var vt = body.Variables[i].VariableType;
                StackSlotInfo slot = default;
                if (vt.IsValueType && !vt.IsPrimitive)
                {
                    var ivt = appdomain.GetType(vt, declaringType, method);
                    if (ivt is ILType il)
                    {
                        int size = il.TotalPrimitiveSize;
                        int refSize = il.TotalReferenceCount;
                        slot.Offset = offset;
                        slot.Size = size;
                        slot.RefOffset = refOffset;
                        offset += size;
                        refOffset += refSize;
                    }
                    else
                    {
                        slot.Offset = offset;
                        slot.RefOffset = refOffset;
                        slot.Size = 0;
                        refOffset++;
                        localIsRef[i] = true;
                    }
                }
                else if (!vt.IsValueType)
                {
                    slot.Offset = offset;
                    slot.RefOffset = refOffset;
                    slot.Size = 0;
                    refOffset++;
                    localIsRef[i] = true;
                }
                else
                {
                    // primitive
                    var ivt = appdomain.GetType(vt, declaringType, method);
                    int size = appdomain.GetPrimitiveSize(ivt);
                    slot.Offset = offset;
                    slot.RefOffset = refOffset;
                    slot.Size = size;
                    offset += size;
                }
                localInfo[locVarRegStart + i] = slot;
            }
            var valueTypes = GatherValueTypes(ref frame);
            int maxSize = 8, maxRefCount = 1;
            foreach (var i in valueTypes)
            {
                if (i is ILType il)
                {
                    int size = il.TotalPrimitiveSize;
                    int refSize = il.TotalReferenceCount;
                    if (size > maxSize)
                        maxSize = size;
                    if (refSize > maxRefCount)
                        maxRefCount = refSize;
                }
            }
            for (int i = 0; i < frame.StackRegisterCount; i++)
            {
                StackSlotInfo slot = default;
                slot.Offset = offset;
                slot.RefOffset = refOffset;
                slot.Size = maxSize;
                offset += maxSize;
                refOffset += maxRefCount;
                localInfo[baseRegStart + i] = slot;
            }
            frame.LocalInfos = localInfo;
            frame.LocalIsReference = localIsRef;
            frame.TotalStructSize = offset;
            frame.TotalRefSize = refOffset;
            frame.LocalsPrimitiveSize = offset - localsPrimStart;
            frame.LocalsReferenceCount = refOffset - localsRefStart;

            // 3) Return value
            int retPrim = 0, retRef = 0;
            var retType = method.ReturnType;
            if (retType != null && retType != appdomain.VoidType)
            {
                int dummyOffset = 0, dummyRef = 0;
                AllocateSlotForType(retType, ref dummyOffset, ref dummyRef);
                retPrim = dummyOffset;
                retRef = dummyRef;
            }
            frame.ReturnPrimitiveSize = retPrim;
            frame.ReturnRefCount = retRef;
        }

        StackSlotInfo AllocateSlotForType(IType t, ref int offset, ref int refOffset)
        {
            StackSlotInfo slot = default;
            if (t.IsPrimitive)
            {
                int size = appdomain.GetPrimitiveSize(t);
                slot.Offset = offset;
                slot.RefOffset = refOffset;
                slot.Size = size;
                offset += size;
            }
            else if (t.IsValueType && t is ILType il)
            {
                int size = il.TotalPrimitiveSize;
                int refSize = il.TotalReferenceCount;
                slot.Offset = offset;
                slot.RefOffset = refOffset;
                slot.Size = size;
                offset += size;
                refOffset += refSize;
            }
            else
            {
                // CLR value type / reference type -> stored as reference (mStack index)
                slot.Offset = offset;
                slot.RefOffset = refOffset;
                slot.Size = 0;
                refOffset++;
            }
            return slot;
        }
#endif
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

        public static void FixSymbol(Dictionary<int, RegisterVMSymbol> symbol)
        {
            HashSet<Instruction> includedIns = new HashSet<Instruction>();
            foreach(var i in symbol.ToArray())
            {
                RegisterVMSymbol cur = i.Value;
                RegisterVMSymbolLink link = null;
                while (cur.ParentSymbol != null)
                {
                    link = cur.ParentSymbol;
                    cur = cur.ParentSymbol.Value;
                }
                var sm = cur.Method.Definition.DebugInformation.GetSequencePointMapping();
                var sq = FindSequencePoint(cur.Instruction, sm);
                if(sq != null && !includedIns.Contains(sq))
                {
                    includedIns.Add(sq);
                    cur.Instruction = sq;
                    if (link != null)
                        link.Value = cur;
                    else
                    {
                        symbol[i.Key] = cur;
                    }
                }
            }
        }

        static Instruction FindSequencePoint(Instruction ins, IDictionary<Instruction, SequencePoint> seqMapping)
        {
            Mono.Cecil.Cil.Instruction cur = ins;
            Mono.Cecil.Cil.SequencePoint sp;
            while (!seqMapping.TryGetValue(cur, out sp) && cur.Previous != null)
                cur = cur.Previous;

            return cur;
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
                        IMethod m;
                        var pCnt = InitializeFunctionParam(ref op, token, out hasRet, out canInline, out m, out toInline, out isILMethod);
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
                        if (m is CLRMethod cm && cm.Redirection != null)
                        {
                            if (!cm.DeclearingType.IsDelegate)
                            {
                                op.Code = OpCodeREnum.Call_Redirect;
                                op.Operand4 = 0x2;
                                var rCnt = cm.ParameterCount;
                                rCnt = rCnt - Math.Max((rCnt - CallRegisterParamCount), 0);

                                op.Operand4 |= (short)rCnt << 16;
                            }
                        }
                    }
                    break;
                case Code.Call:
                case Code.Callvirt:
                    {
                        bool canInline, isILMethod;
                        ILMethod toInline;
                        IMethod m;
                        var pCnt = InitializeFunctionParam(ref op, token, out hasRet, out canInline, out m, out toInline, out isILMethod);
                        bool hasConstrained = false;
                        int constrainIdx = -1;
                        if (lst.Count > 0)
                        {
                            constrainIdx = lst.Count - 1;
                            hasConstrained = lst[constrainIdx].Code == OpCodeREnum.Constrained;
                        }
                        bool needInline = canInline && !hasConstrained;
                        if (needInline)
                        {
                            if (toInline.BodyRegister.Length > Optimizer.MaximalInlineInstructionCount / 2)
                                needInline = false;
                        }
                        if (!needInline)
                        {
                            if (code.Code == Code.Callvirt && m is ILMethod)
                            {
                                ILMethod ilm = (ILMethod)m;
                                if (!ilm.Definition.IsAbstract && !ilm.Definition.IsVirtual && !ilm.DeclearingType.IsInterface)
                                    op.Code = OpCodeREnum.Call;
                            }
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
                                var symbol = block.InstructionMapping[constrainIdx];
                                block.InstructionMapping.Remove(constrainIdx);
                                block.InstructionMapping.Add(lst.Count, symbol);
                                lst.Add(old);
                            }
                            baseRegIdx -= (short)pCnt;

                            if (hasRet)
                                op.Register1 = baseRegIdx++;
                            else
                                op.Register1 = -1;
                            if (m is CLRMethod cm && cm.Redirection != null)
                            {
                                if (!cm.IsDelegateInvoke && !cm.IsDelegateDynamicInvoke)
                                {
                                    op.Code = OpCodeREnum.Call_Redirect;
                                    op.Operand4 = 0;
                                    if (hasConstrained)
                                        op.Operand4 |= 0x1;
                                    if (cm.ReturnType != appdomain.VoidType && !cm.IsConstructor)
                                        op.Operand4 |= 0x4;

                                    var rCnt = cm.HasThis ? cm.ParameterCount + 1 : cm.ParameterCount;
                                    rCnt = rCnt - Math.Max((rCnt - CallRegisterParamCount), 0);

                                    op.Operand4 |= (short)rCnt << 16;
                                }
                            }
                        }
                        else
                        {
                            baseRegIdx -= (short)pCnt;
                            RegisterVMSymbolLink link = null;
#if DEBUG && !DISABLE_ILRUNTIME_DEBUG
                            link = new RegisterVMSymbolLink();
                            link.BaseRegisterIndex = baseRegIdx;
                            link.Value.Instruction = ins;
                            link.Value.Method = method;
#else
                            RegisterVMSymbol vmS = new RegisterVMSymbol()
                            {
                                Instruction = ins,
                                Method = method
                            };
                            block.InstructionMapping.Add(lst.Count,vmS);
#endif
#if DEBUG && !NO_PROFILER
            if (System.Threading.Thread.CurrentThread.ManagedThreadId == method.AppDomain.UnityMainThreadID)

#if UNITY_5_5_OR_NEWER
                UnityEngine.Profiling.Profiler.BeginSample("JITCompiler.InlineMethod");
#else
                UnityEngine.Profiler.BeginSample("JITCompiler.InlineMethod");
#endif

#endif
                            Optimizer.InlineMethod(block, toInline, link, ref jumptables, baseRegIdx, hasRet);
#if DEBUG && !NO_PROFILER
            if (System.Threading.Thread.CurrentThread.ManagedThreadId == method.AppDomain.UnityMainThreadID)
#if UNITY_5_5_OR_NEWER
                UnityEngine.Profiling.Profiler.EndSample();
#else
                UnityEngine.Profiler.EndSample();
#endif
#endif
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
                case Code.Readonly:
                case Code.Volatile:
                case Code.Endfinally:
                case Code.Rethrow:
                    break;
                case Code.Leave:
                case Code.Leave_S:
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
                case Code.Neg:
                case Code.Not:
                    op.Register1 = (short)(baseRegIdx - 1);
                    op.Register2 = (short)(baseRegIdx - 1);
                    break;
                case Code.Ldobj:
                    op.Register1 = (short)(baseRegIdx - 1);
                    op.Register2 = (short)(baseRegIdx - 1);
                    op.Operand = method.GetTypeTokenHashCode(token);
                    break;
                case Code.Ldfld:
#if ENABLE_NEO_MODE
                    op.Register1 = (short)(baseRegIdx - 1);
                    op.Register2 = (short)(baseRegIdx - 1);
                    {
                        var offset = appdomain.GetFieldOffset(token, declaringType, method, out IType type, out IType fieldType);
                        if (type is ILType)
                        {
                            op.Code = GetLdfldCodeForType(fieldType);
                            op.Operand = type.GetHashCode();
                            op.Operand2 = offset.PrimitiveOffset;
                            op.Operand3 = offset.ReferenceOffset;
                        }
                        else
                            op.OperandLong = ((long)type.GetHashCode() << 32) | (uint)offset.PrimitiveOffset;
                    }
                    break;
#endif
                case Code.Ldflda:
                    op.Register1 = (short)(baseRegIdx - 1);
                    op.Register2 = (short)(baseRegIdx - 1);
                    op.OperandLong = appdomain.GetStaticFieldIndex(token, declaringType, method);
                    break;
                case Code.Stfld:
                    op.Register1 = (short)(baseRegIdx - 2);
                    op.Register2 = (short)(baseRegIdx - 1);
#if ENABLE_NEO_MODE
                    {
                        var offset = appdomain.GetFieldOffset(token, declaringType, method, out IType type, out IType fieldType);
                        if(type is ILType)
                        {
                            op.Code = GetStfldCodeForType(fieldType);
                            op.Operand = type.GetHashCode();
                            op.Operand2 = offset.PrimitiveOffset;
                            op.Operand3 = offset.ReferenceOffset;
                        }
                        else
                            op.OperandLong = ((long)type.GetHashCode() << 32) | (uint)offset.PrimitiveOffset;
                    }
#else
                    op.OperandLong = appdomain.GetStaticFieldIndex(token, declaringType, method);
#endif
                    baseRegIdx -= 2;
                    break;
                case Code.Box:
                case Code.Unbox:
                case Code.Unbox_Any:
                case Code.Isinst:
                case Code.Castclass:
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
                        IMethod m;
                        InitializeFunctionParam(ref op, token, out hasReturn, out canInline, out m, out toInline, out isILMethod);
                    }
                    break;

                case Code.Ldvirtftn:
                    {
                        bool hasReturn, canInline, isILMethod;
                        ILMethod toInline;
                        IMethod m;
                        InitializeFunctionParam(ref op, token, out hasReturn, out canInline, out m, out toInline, out isILMethod);
                        op.Register1 = (short)(baseRegIdx - 1);
                        op.Register2 = (short)(baseRegIdx - 1);
                    }
                    break;
                case Code.Pop:
                    baseRegIdx--;
                    op.Code = OpCodeREnum.Nop;
                    break;
                default:
                    throw new NotImplementedException(string.Format("Unknown Opcode:{0}", code.Code));
            }
            RegisterVMSymbol s = new RegisterVMSymbol()
            {
                Instruction = ins,
                Method = method
            };
            block.InstructionMapping.Add(lst.Count, s);
            lst.Add(op);
            if (!block.NeedLoadConstantElimination)
                block.NeedLoadConstantElimination = Optimizer.IsLoadConstant(op.Code);
        }
#if ENABLE_NEO_MODE
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
                    res = OpCodeREnum.Ldfld_U4;
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
                if (fieldType is ILType && fieldType.IsValueType)
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
                    res = OpCodeREnum.Stfld_U4;
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
                if (fieldType is ILType && fieldType.IsValueType)
                {
                    res = OpCodeREnum.Stfld_Value;
                }
                else
                    res = OpCodeREnum.Stfld_Ref;
            }
            return res;
        }
#endif

        int InitializeFunctionParam(ref OpCodes.OpCodeR op, object token, out bool hasReturn, out bool canInline, out IMethod m, out ILMethod toInline, out bool isILMethod)
        {
            bool invalidToken;
            int pCnt = 0;
            m = appdomain.GetMethod(token, declaringType, method, out invalidToken);
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
                hasReturn = m.ReturnType != appdomain.VoidType && !(m.IsConstructor && op.Code == OpCodeREnum.Call);
                if (m is ILMethod)
                {
                    isILMethod = !m.IsDelegateInvoke;
                    var ilm = (ILMethod)m;
                    bool noJIT = (ilm.JITFlags & ILRuntimeJITFlags.NoJIT) != ILRuntimeJITFlags.None;
                    bool forceInline = (ilm.JITFlags & ILRuntimeJITFlags.ForceInline) != ILRuntimeJITFlags.None;
                    bool hasExceptionHandler = ilm.Definition.HasBody && ilm.Definition.Body.HasExceptionHandlers;
                    if (!ilm.IsDelegateInvoke && !ilm.IsVirtual && !noJIT && !hasExceptionHandler && !ilm.Compiling && !ilm.IsEventAdd && !ilm.IsEventRemove)
                    {
                        var def = ilm.Definition;
                        if (!def.HasBody || forceInline)
                        {
                            canInline = true;
                            toInline = ilm;
                        }
                        else
                        {
                            bool codeSizeOK = ilm.IsRegisterBodyReady ? ilm.BodyRegister.Length <= Optimizer.MaximalInlineInstructionCount / 2 : def.Body.Instructions.Count <= Optimizer.MaximalInlineInstructionCount;
                            if(codeSizeOK)
                            {
                                canInline = true;
                                toInline = ilm;
                            }
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
