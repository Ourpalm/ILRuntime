#if ENABLE_NEO_MODE
using System.Collections.Generic;

using ILRuntime.Runtime.Intepreter.OpCodes;

namespace ILRuntime.Runtime.Intepreter.RegisterVM
{
    partial class Optimizer
    {
        // Neo Step 12c: fold `Ldloca V, R` (or `Ldarga P, R`) followed immediately by a single
        // `Ldfld_* / Stfld_* / Initobj` consumer whose receiver register is R. The consumer's
        // receiver is rewritten to V/P directly, the producer is added to CanRemove, and the
        // emit loop drops it while preserving addr / symbols / branch remap for free.
        //
        // Correctness bootstrap:
        //   * Step 12b's PropagateByRefReferentOffsets keeps the un-folded path (Operand4 =
        //     -1 - RefOffset via IsRef) correct, so this pass is a pure perf optimization and
        //     never becomes a correctness dependency.
        //   * The consumer's `Operand4 = 1` for Ldfld/Stfld matches the Translate-time
        //     candidate flag (JITCompiler.cs L1341 / L1390); LowerNeoOffsets sees a non-byref
        //     receiver and encodes RefOffset+1 (inline direct). Initobj lowering only checks
        //     receiver.IsRef and never reads Operand4 as a hint, so no Operand4 write is needed.
        //   * Ldflda uses a *two-state* Operand4 encoding (0=heap / 1=Ref-Slot source, no
        //     inline-direct slot), so it is deliberately excluded from consumer set - folding
        //     it would change its semantics. Its "byref chain" case belongs to Step 17.
        public static void FoldLdlocaFieldAccess(List<CodeBasicBlock> blocks, bool hasReturn)
        {
            foreach (var block in blocks)
            {
                var lst = block.FinalInstructions;
                var canRemove = block.CanRemove;
                for (int i = 0; i < lst.Count - 1; i++)
                {
                    if (canRemove.Contains(i))
                        continue;

                    var producer = lst[i];
                    if (!IsFoldableProducer(producer.Code))
                        continue;

                    short byrefReg = producer.Register1; // Ldloca/Ldarga writes byref into Register1
                    short sourceReg = producer.Register2; // source local / parameter slot

                    // Walk forward looking for the first instruction that reads or writes the
                    // byref register. That is the sole consumer we're allowed to fold with. Any
                    // instructions in between (e.g. `ldc.i4 42` loading the value slot for a
                    // subsequent `stfld`) touch other registers only.
                    int j = -1;
                    for (int k = i + 1; k < lst.Count; k++)
                    {
                        if (canRemove.Contains(k))
                            continue;
                        if (ReferencesRegister(lst[k], hasReturn, byrefReg))
                        {
                            j = k;
                            break;
                        }
                    }
                    if (j < 0)
                        continue;

                    var consumer = lst[j];
                    if (!TryGetFoldableReceiverSlot(consumer.Code, out bool receiverInReg1))
                        continue;

                    // Boxed CLR value-type / reference-type receivers were emitted by Translate
                    // with Operand4 == 0 (heap path, primitive/ref offsets packed into OperandLong).
                    // Folding them to inline direct would misuse OperandLong as byte offsets and
                    // corrupt the frame — leave those consumers alone and let the byref path go
                    // through the Ref-Slot lowering (Step 12b) instead. Only fold when Translate
                    // already tagged the consumer as an inline candidate.
                    if (NeedsOperand4Hint(consumer.Code) && consumer.Operand4 == 0)
                        continue;

                    short consumerReceiver = receiverInReg1 ? consumer.Register1 : consumer.Register2;
                    if (consumerReceiver != byrefReg)
                        continue;

                    // Consumer must reference the byref register only via the receiver slot
                    // (not doubly via the source/value slot or elsewhere), otherwise rewriting
                    // Register1/Register2 alone would leave a stale reference behind.
                    if (ReferencesRegisterOutsideReceiver(consumer, hasReturn, byrefReg, receiverInReg1))
                        continue;

                    // Rewrite consumer: receiver -> source struct/parameter register.
                    if (receiverInReg1)
                        consumer.Register1 = sourceReg;
                    else
                        consumer.Register2 = sourceReg;

                    // Ldfld/Stfld need the inline-direct candidate flag (mirroring Translate
                    // L1341/L1390); Initobj lowering only checks receiver.IsRef and ignores
                    // Operand4, so leave it alone for Initobj.
                    if (NeedsOperand4Hint(consumer.Code))
                        consumer.Operand4 = 1;

                    lst[j] = consumer;
                    canRemove.Add(i);
                }
            }
        }

        static bool ReferencesRegister(OpCodeR op, bool hasReturn, short reg)
        {
            if (GetOpcodeSourceRegister(ref op, hasReturn, out short s1, out short s2, out short s3))
            {
                if (s1 == reg || s2 == reg || s3 == reg)
                    return true;
            }
            if (GetOpcodeDestRegister(ref op, out short d))
            {
                if (d == reg)
                    return true;
            }
            return false;
        }

        // Returns true if `op` references `reg` through any register slot *other than* the
        // designated receiver slot. Used to reject foldable-looking consumers where the byref
        // register accidentally aliases another operand (very rare after BCP/FCP but possible
        // in degenerate IL).
        static bool ReferencesRegisterOutsideReceiver(OpCodeR op, bool hasReturn, short reg,
                                                     bool receiverInReg1)
        {
            if (GetOpcodeSourceRegister(ref op, hasReturn, out short s1, out short s2, out short s3))
            {
                // Register1 is the dst reg (GetOpcodeDestRegister); Register2/3 are typically
                // the source operands returned here for consumers we care about.
                if (s1 == reg || s2 == reg || s3 == reg)
                {
                    // For Ldfld/Ldflda receiver-in-Register2, an s1==reg would mean the
                    // consumer also USES it as an extra source; likewise for Stfld/Initobj
                    // receiver-in-Register1 (s1 not returned by GetOpcodeSourceRegister for
                    // Stfld anyway, but be defensive).
                    if (!receiverInReg1)
                    {
                        // Ldfld: Register2 is the sole receiver source. Any other src match is bad.
                        if (s1 == reg) return true;
                        if (s3 == reg) return true;
                    }
                    else
                    {
                        // Stfld/Initobj: Register1 is receiver (returned as dst). Register2/3
                        // are non-receiver srcs.
                        if (s2 == reg) return true;
                        if (s3 == reg) return true;
                    }
                }
            }
            if (GetOpcodeDestRegister(ref op, out short d))
            {
                if (d == reg && !receiverInReg1)
                    return true; // Ldfld dst equals reg means dst == receiver, degenerate.
            }
            return false;
        }

        static bool IsFoldableProducer(OpCodeREnum code)
        {
            switch (code)
            {
                case OpCodeREnum.Ldloca:
                case OpCodeREnum.Ldloca_S:
                case OpCodeREnum.Ldarga:
                case OpCodeREnum.Ldarga_S:
                    return true;
                default:
                    return false;
            }
        }

        // Returns true if `code` is a consumer this pass can fold. `receiverInReg1` reports
        // whether the receiver register is stored in Register1 (Stfld / Initobj) or Register2
        // (Ldfld). Ldflda is excluded (two-state Operand4, folding changes semantics).
        static bool TryGetFoldableReceiverSlot(OpCodeREnum code, out bool receiverInReg1)
        {
            switch (code)
            {
                case OpCodeREnum.Ldfld:
                case OpCodeREnum.Ldfld_I1:
                case OpCodeREnum.Ldfld_I2:
                case OpCodeREnum.Ldfld_I4:
                case OpCodeREnum.Ldfld_I8:
                case OpCodeREnum.Ldfld_U1:
                case OpCodeREnum.Ldfld_U2:
                case OpCodeREnum.Ldfld_U4:
                case OpCodeREnum.Ldfld_U8:
                case OpCodeREnum.Ldfld_R4:
                case OpCodeREnum.Ldfld_R8:
                case OpCodeREnum.Ldfld_Ref:
                case OpCodeREnum.Ldfld_Value:
                    receiverInReg1 = false;
                    return true;
                case OpCodeREnum.Stfld:
                case OpCodeREnum.Stfld_I1:
                case OpCodeREnum.Stfld_I2:
                case OpCodeREnum.Stfld_I4:
                case OpCodeREnum.Stfld_I8:
                case OpCodeREnum.Stfld_U1:
                case OpCodeREnum.Stfld_U2:
                case OpCodeREnum.Stfld_U4:
                case OpCodeREnum.Stfld_U8:
                case OpCodeREnum.Stfld_R4:
                case OpCodeREnum.Stfld_R8:
                case OpCodeREnum.Stfld_Ref:
                case OpCodeREnum.Stfld_Value:
                case OpCodeREnum.Initobj:
                    receiverInReg1 = true;
                    return true;
                default:
                    receiverInReg1 = false;
                    return false;
            }
        }

        static bool NeedsOperand4Hint(OpCodeREnum code)
        {
            switch (code)
            {
                case OpCodeREnum.Initobj:
                    return false; // Initobj lowering reads only receiver.IsRef.
                default:
                    return true;
            }
        }
    }
}
#endif
