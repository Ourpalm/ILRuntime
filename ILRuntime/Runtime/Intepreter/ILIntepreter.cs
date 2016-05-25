using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Stack;
using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Runtime.Intepreter.OpCodes;

namespace ILRuntime.Runtime.Intepreter
{
    unsafe class ILIntepreter
    {
        Enviorment.AppDomain domain;
        RuntimeStack stack;

        public RuntimeStack Stack { get { return stack; } }
        public ILIntepreter(Enviorment.AppDomain domain)
        {
            this.domain = domain;
            stack = new RuntimeStack(this);
        }

        public Enviorment.AppDomain AppDomain { get { return domain; } }
        public void Run(ILMethod method, object[] p)
        {
            List<object> mStack = stack.ManagedStack;
            int mStackBase = mStack.Count;

            StackObject* esp = PushParameters(stack.StackBase, method.Parameters, p);
            bool unhandledException;
            Execute(method, esp, out unhandledException);
            //ClearStack
            mStack.RemoveRange(mStackBase, mStack.Count - mStackBase);            
        }
        StackObject* Execute(ILMethod method, StackObject* esp, out bool unhandledException)
        {
            OpCode[] body = method.Body;
            StackFrame frame = stack.PushFrame(method, esp);
            StackObject* v1 = frame.LocalVarPointer;
            StackObject* v2 = frame.LocalVarPointer + 1;
            StackObject* v3 = frame.LocalVarPointer + 2;
            StackObject* v4 = frame.LocalVarPointer + 3;
            StackObject* v5 = frame.LocalVarPointer + 4;

            esp = frame.BasePointer;
            StackObject* arg = frame.LocalVarPointer - 1;
            List<object> mStack = stack.ManagedStack;
            int mStackBase = mStack.Count;
            unhandledException = false;
            //Managed Stack reserved for local variable
            for (int i = 0; i < method.LocalVariableCount; i++)
                mStack.Add(null);
            try
            {
                fixed (OpCode* ptr = body)
                {
                    OpCode* ip = ptr;
                    OpCodeEnum code = ip->Code;
                    bool returned = false;
                    while (!returned)
                    {
#if DEBUG
                        frame.Address.Value = (int)(ip - ptr);
#endif
                        code = ip->Code;
                        switch (code)
                        {
                            case OpCodeEnum.Ldarg_0:
                                *esp = *(arg);
                                esp++;
                                break;
                            case OpCodeEnum.Ldarg_1:
                                *esp = *(arg - 1);
                                esp++;
                                break;
                            case OpCodeEnum.Stloc_0:
                                esp--;
                                *v1 = *esp;
                                if (v1->ObjectType == ObjectTypes.Object)
                                {
                                    int idx = mStackBase;
                                    mStack[idx] = mStack[v1->Value];
                                    v1->Value = idx;
                                    Free(esp);
                                }
                                break;
                            case OpCodeEnum.Ldloc_0:
                                *esp = *v1;
                                esp++;
                                break;
                            case OpCodeEnum.Stloc_1:
                                esp--;
                                *v2 = *esp;
                                if (v2->ObjectType == ObjectTypes.Object)
                                {
                                    int idx = mStackBase + 1;
                                    mStack[idx] = mStack[v2->Value];
                                    v2->Value = idx;
                                    Free(esp);
                                }
                                break;
                            case OpCodeEnum.Ldloc_1:
                                *esp = *v2;
                                esp++;
                                break;
                            case OpCodeEnum.Stloc_2:
                                esp--;
                                *v3 = *esp;
                                if (v3->ObjectType == ObjectTypes.Object)
                                {
                                    int idx = mStackBase + 2;
                                    mStack[idx] = mStack[v3->Value];
                                    v3->Value = idx;
                                    Free(esp);
                                }
                                break;
                            case OpCodeEnum.Ldloc_2:
                                *esp = *v3;
                                esp++;
                                break;
                            case OpCodeEnum.Stloc_3:
                                esp--;
                                *v4 = *esp;
                                if (v4->ObjectType == ObjectTypes.Object)
                                {
                                    int idx = mStackBase + 3;
                                    mStack[idx] = mStack[v4->Value];
                                    v4->Value = idx;
                                    Free(esp);
                                }
                                break;
                            case OpCodeEnum.Ldloc_3:
                                *esp = *v4;
                                esp++;
                                break;
                            case OpCodeEnum.Ldc_I4_M1:
                                esp->Value = -1;
                                esp->ObjectType = ObjectTypes.Integer;
                                esp++;
                                break;
                            case OpCodeEnum.Ldc_I4_0:
                                esp->Value = 0;
                                esp->ObjectType = ObjectTypes.Integer;
                                esp++;
                                break;
                            case OpCodeEnum.Ldc_I4_1:
                                esp->Value = 1;
                                esp->ObjectType = ObjectTypes.Integer;
                                esp++;
                                break;
                            case OpCodeEnum.Ldc_I4_2:
                                esp->Value = 2;
                                esp->ObjectType = ObjectTypes.Integer;
                                esp++;
                                break;
                            case OpCodeEnum.Ldc_I4_3:
                                esp->Value = 3;
                                esp->ObjectType = ObjectTypes.Integer;
                                esp++;
                                break;
                            case OpCodeEnum.Ldc_I4_4:
                                esp->Value = 4;
                                esp->ObjectType = ObjectTypes.Integer;
                                esp++;
                                break;
                            case OpCodeEnum.Ldc_I4_5:
                                esp->Value = 5;
                                esp->ObjectType = ObjectTypes.Integer;
                                esp++;
                                break;
                            case OpCodeEnum.Ldc_I4_6:
                                esp->Value = 6;
                                esp->ObjectType = ObjectTypes.Integer;
                                esp++;
                                break;
                            case OpCodeEnum.Ldc_I4_7:
                                esp->Value = 7;
                                esp->ObjectType = ObjectTypes.Integer;
                                esp++;
                                break;
                            case OpCodeEnum.Ldc_I4_8:
                                esp->Value = 8;
                                esp->ObjectType = ObjectTypes.Integer;
                                esp++;
                                break;
                            case OpCodeEnum.Ldc_I4:
                            case OpCodeEnum.Ldc_I4_S:
                                esp->Value = ip->TokenInteger;
                                esp->ObjectType = ObjectTypes.Integer;
                                esp++;
                                break;
                            case OpCodeEnum.Clt:
                                {
                                    StackObject* b = esp - 1;
                                    StackObject* a = esp - 2;
                                    esp = esp - 2;
                                    if (a->ObjectType == ObjectTypes.Integer)
                                    {
                                        if (a->Value < b->Value)
                                            esp->Value = 1;
                                        else
                                            esp->Value = 0;
                                    }
                                    else
                                        throw new NotSupportedException();
                                    esp++;
                                }
                                break;
                            case OpCodeEnum.Add:
                                {
                                    StackObject* b = esp - 1;
                                    StackObject* a = esp - 2;
                                    esp = esp - 2;
                                    if (a->ObjectType == ObjectTypes.Long)
                                    {
                                        esp->ObjectType = ObjectTypes.Long;
                                        *((long*)&esp->Value) = *((long*)&a->Value) + *((long*)&b->Value);
                                    }
                                    else
                                    {
                                        esp->ObjectType = ObjectTypes.Integer;
                                        esp->Value = a->Value + b->Value;
                                    }
                                    esp++;
                                }
                                break;
                            case OpCodeEnum.Ldind_I:
                                break;
                            case OpCodeEnum.Ldind_I1:
                                break;
                            case OpCodeEnum.Ldind_I2:
                                break;
                            case OpCodeEnum.Ldind_I4:
                                break;
                            case OpCodeEnum.Ldind_I8:
                                break;
                            case OpCodeEnum.Ret:
                                returned = true;
                                break;
                            case OpCodeEnum.Brtrue:
                            case OpCodeEnum.Brtrue_S:
                                {
                                    esp--;
                                    if (esp->ObjectType == ObjectTypes.Integer && esp->Value > 0)
                                    {
                                        ip = ptr + ip->TokenInteger;
                                        continue;
                                    }
                                    else
                                        Free(esp);
                                }
                                break;
                            case OpCodeEnum.Blt:
                            case OpCodeEnum.Blt_S:
                                {
                                    var b = esp - 1;
                                    var a = esp - 2;
                                    esp -= 2;
                                    if (esp->ObjectType == ObjectTypes.Integer)
                                    {
                                        if (a->Value < b->Value)
                                        {
                                            ip = ptr + ip->TokenInteger;
                                            continue;
                                        }
                                    }
                                }
                                break;
                            case OpCodeEnum.Br_S:
                            case OpCodeEnum.Br:
                                ip = ptr + ip->TokenInteger;
                                continue;
                            case OpCodeEnum.Call:
                                {
                                    IMethod m = domain.GetMethod(ip->TokenInteger);
                                    if (m is ILMethod)
                                    {
                                        esp = Execute((ILMethod)m, esp, out unhandledException);
                                        if (unhandledException)
                                            returned = true;
                                    }
                                    else
                                        throw new NotSupportedException();
                                }
                                break;
                            case OpCodeEnum.Nop:
                                break;
                            default:
                                throw new NotSupportedException("Not supported opcode " + code);
                        }
                        ip++;
                    }
                }
            }
            catch (Exception ex)
            {
                unhandledException = true;
#if DEBUG
                Debugger.DebugService.Instance.Break(this, ex);
#else
                throw new ExecutionEngineException("RuntimeError occured", ex);
#endif
            }
            //ClearStack
            mStack.RemoveRange(mStackBase, mStack.Count - mStackBase);
            return stack.PopFrame(ref frame, esp);

        }

        StackObject* PushParameters(StackObject* esp, List<IType> plist, object[] p)
        {
            List<object> mStack = stack.ManagedStack;
            if (p != null && p.Length > 0)
            {
                if (plist.Count != p.Length)
                    throw new ArgumentOutOfRangeException();
                for (int i = p.Length - 1; i >= 0; i--)
                {
                    var t = plist[i];
                    Type clrType = t.TypeForCLR;
                    if (clrType == typeof(int))
                    {
                        esp->ObjectType = ObjectTypes.Integer;
                        esp->Value = (int)p[i];
                    }
                    else if (clrType == typeof(float))
                    {
                        esp->ObjectType = ObjectTypes.Integer;
                        *(float*)&esp->Value = (float)p[i];
                    }
                    else
                    {
                        esp->ObjectType = ObjectTypes.Object;
                        esp->Value = mStack.Count;
                        mStack.Add(p[i]);
                    }
                    esp++;
                }
            }
            return esp;
        }
        StackObject* PushOne(StackObject* esp)
        {
            esp->ObjectType = ObjectTypes.Integer;
            esp->Value = 1;
            return esp + 1;
        }

        StackObject* PushZero(StackObject* esp)
        {
            esp->ObjectType = ObjectTypes.Integer;
            esp->Value = 0;
            return esp + 1;
        }

        void Free(StackObject* esp)
        {
            if (esp->ObjectType == ObjectTypes.Object)
            {
                if (esp->Value == stack.ManagedStack.Count)
                    stack.ManagedStack.RemoveAt(esp->Value);
            }
#if DEBUG
            esp->ObjectType = ObjectTypes.Null;
            esp->Value = -1;
#endif
        }
    }
}
