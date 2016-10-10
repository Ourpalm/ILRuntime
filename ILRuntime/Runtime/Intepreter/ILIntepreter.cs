using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Stack;
using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Runtime.Intepreter.OpCodes;
using ILRuntime.CLR.Utils;

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
        public object Run(ILMethod method, object[] p)
        {
            List<object> mStack = stack.ManagedStack;
            int mStackBase = mStack.Count;

            StackObject* esp = PushParameters(method, stack.StackBase, p);
            bool unhandledException;
            esp = Execute(method, esp, out unhandledException);
            object result = method.ReturnType != domain.VoidType ? (esp - 1)->ToObject(domain, mStack) : null;
            //ClearStack
            mStack.RemoveRange(mStackBase, mStack.Count - mStackBase);
            return result;
        }
        internal StackObject* Execute(ILMethod method, StackObject* esp, out bool unhandledException)
        {
            if (method == null)
                throw new NullReferenceException();
            OpCode[] body = method.Body;
            StackFrame frame = stack.PushFrame(method, esp);
            StackObject* v1 = frame.LocalVarPointer;
            StackObject* v2 = frame.LocalVarPointer + 1;
            StackObject* v3 = frame.LocalVarPointer + 2;
            StackObject* v4 = frame.LocalVarPointer + 3;

            esp = frame.BasePointer;
            StackObject* arg = frame.LocalVarPointer - method.ParameterCount;
            List<object> mStack = stack.ManagedStack;
            int mStackBase = mStack.Count;
            int locBase = mStackBase;
            int paramCnt = method.ParameterCount;
            if (method.HasThis)//this parameter is always object reference
            {
                arg--;
                paramCnt++;
                if (arg->ObjectType != ObjectTypes.StackObjectReference)
                    mStackBase--;
            }
            unhandledException = false;

            //Managed Stack reserved for arguments(In case of starg)
            for (int i = 0; i < paramCnt; i++)
            {
                var a = arg + i;
                if (a->ObjectType == ObjectTypes.Null)
                {
                    //Need to reserve place for null, in case of starg
                    a->Value = mStack.Count;
                    mStack.Add(null);
                }
            }
                //Managed Stack reserved for local variable
            for (int i = 0; i < method.LocalVariableCount; i++)
            {
                var v = method.Definition.Body.Variables[i];
                if (v.VariableType.IsValueType && !v.VariableType.IsPrimitive)
                {
                    var t = AppDomain.GetType(v.VariableType, method.DeclearingType);
                    if (t is ILType)
                    {
                        var obj = ((ILType)t).Instantiate();
                        var loc = v1 + i;
                        loc->ObjectType = ObjectTypes.Object;
                        loc->Value = mStack.Count;
                        mStack.Add(obj);
                    }
                    else
                    {
                        var obj = Activator.CreateInstance(t.TypeForCLR);
                        var loc = v1 + i;
                        loc->ObjectType = ObjectTypes.Object;
                        loc->Value = mStack.Count;
                        mStack.Add(obj);
                    }
                }
                else
                {
                    if (v.VariableType.IsPrimitive)
                    {
                        var t = AppDomain.GetType(v.VariableType, method.DeclearingType);
                        var loc = v1 + i;
                        loc->Initialized(t.TypeForCLR);
                    }
                    mStack.Add(null);
                }
            }
            fixed (OpCode* ptr = body)
            {
                OpCode* ip = ptr;
                OpCodeEnum code = ip->Code;
                bool returned = false;
                while (!returned)
                {
                    try
                    {
#if DEBUG
                        frame.Address.Value = (int)(ip - ptr);
#endif
                        code = ip->Code;
                        switch (code)
                        {
                            #region Arguments and Local Variable
                            case OpCodeEnum.Ldarg_0:
                                CopyToStack(esp, arg, mStack);
                                esp++;
                                break;
                            case OpCodeEnum.Ldarg_1:
                                CopyToStack(esp, arg + 1, mStack);
                                esp++;
                                break;
                            case OpCodeEnum.Ldarg_2:
                                CopyToStack(esp, arg + 2, mStack);
                                esp++;
                                break;
                            case OpCodeEnum.Ldarg_3:
                                CopyToStack(esp, arg + 3, mStack);
                                esp++;
                                break;
                            case OpCodeEnum.Ldarg:
                            case OpCodeEnum.Ldarg_S:
                                CopyToStack(esp, arg + ip->TokenInteger, mStack);
                                esp++;
                                break;
                            case OpCodeEnum.Ldarga:
                            case OpCodeEnum.Ldarga_S:
                                {
                                    var a = arg + ip->TokenInteger;
                                    esp->ObjectType = ObjectTypes.StackObjectReference;
                                    *(StackObject**)&esp->Value = a;
                                    esp++;
                                }
                                break;
                            case OpCodeEnum.Starg:
                            case OpCodeEnum.Starg_S:
                                {
                                    var a = arg + ip->TokenInteger;
                                    var val = esp - 1;
                                    if (val->ObjectType >= ObjectTypes.Object)
                                    {
                                        a->ObjectType = val->ObjectType;
                                        mStack[a->Value] = mStack[val->Value];
                                        a->ValueLow = val->ValueLow;
                                    }
                                    else
                                    {
                                        *a = *val;
                                    }
                                    Free(val);
                                    esp--;
                                }
                                break;
                            case OpCodeEnum.Stloc_0:
                                {
                                    esp--;
                                    *v1 = *esp;
                                    int idx = locBase;
                                    if (v1->ObjectType >= ObjectTypes.Object)
                                    {
                                        mStack[idx] = mStack[v1->Value];
                                        v1->Value = idx;
                                        Free(esp);
                                    }
                                    else
                                        mStack[idx] = null;
                                }
                                break;
                            case OpCodeEnum.Ldloc_0:
                                CopyToStack(esp, v1, mStack);
                                esp++;
                                break;
                            case OpCodeEnum.Stloc_1:
                                {
                                    esp--;
                                    *v2 = *esp;
                                    int idx = locBase + 1;
                                    if (v2->ObjectType >= ObjectTypes.Object)
                                    {
                                        mStack[idx] = mStack[v2->Value];
                                        v2->Value = idx;
                                        Free(esp);
                                    }
                                    else
                                        mStack[idx] = null;
                                }
                                break;
                            case OpCodeEnum.Ldloc_1:
                                CopyToStack(esp, v2, mStack);
                                esp++;
                                break;
                            case OpCodeEnum.Stloc_2:
                                {
                                    esp--;
                                    *v3 = *esp;
                                    int idx = locBase + 2;
                                    if (v3->ObjectType >= ObjectTypes.Object)
                                    {
                                        mStack[idx] = mStack[v3->Value];
                                        v3->Value = idx;
                                        Free(esp);
                                    }
                                    else
                                        mStack[idx] = null;
                                    break;
                                }
                            case OpCodeEnum.Ldloc_2:
                                CopyToStack(esp, v3, mStack);
                                esp++;
                                break;
                            case OpCodeEnum.Stloc_3:
                                {
                                    esp--;
                                    *v4 = *esp;
                                    int idx = locBase + 3;
                                    if (v4->ObjectType >= ObjectTypes.Object)
                                    {
                                        mStack[idx] = mStack[v4->Value];
                                        v4->Value = idx;
                                        Free(esp);
                                    }
                                    else
                                        mStack[idx] = null;
                                }
                                break;
                            case OpCodeEnum.Ldloc_3:
                                CopyToStack(esp, v4, mStack);
                                esp++;
                                break;
                            case OpCodeEnum.Stloc:
                            case OpCodeEnum.Stloc_S:
                                {
                                    esp--;
                                    var v = frame.LocalVarPointer + ip->TokenInteger;
                                    *v = *esp;
                                    int idx = locBase + ip->TokenInteger;
                                    if (v->ObjectType >= ObjectTypes.Object)
                                    {
                                        mStack[idx] = mStack[v->Value];
                                        v->Value = idx;
                                        Free(esp);
                                    }
                                    else
                                        mStack[idx] = null;
                                }
                                break;
                            case OpCodeEnum.Ldloc:
                            case OpCodeEnum.Ldloc_S:
                                {
                                    var v = frame.LocalVarPointer + ip->TokenInteger;
                                    CopyToStack(esp, v, mStack);
                                    esp++;
                                }
                                break;
                            case OpCodeEnum.Ldloca:
                            case OpCodeEnum.Ldloca_S:
                                {
                                    var v = frame.LocalVarPointer + ip->TokenInteger;
                                    esp->ObjectType = ObjectTypes.StackObjectReference;
                                    *(StackObject**)&esp->Value = v;
                                    esp++;
                                }
                                break;
                            case OpCodeEnum.Ldobj:
                                {
                                    var objRef = esp - 1;
                                    switch (objRef->ObjectType)
                                    {
                                        case ObjectTypes.ArrayReference:
                                            {
                                                var t = AppDomain.GetType(ip->TokenInteger);
                                                var obj = mStack[objRef->Value];
                                                var idx = objRef->ValueLow;
                                                Free(objRef);
                                                LoadFromArrayReference(obj, idx, objRef, t, mStack);
                                            }
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                }
                                break;
                            case OpCodeEnum.Stobj:
                                {
                                    var objRef = esp - 2;
                                    var val = esp - 1;
                                    switch (objRef->ObjectType)
                                    {
                                        case ObjectTypes.ArrayReference:
                                            {
                                                var t = AppDomain.GetType(ip->TokenInteger);
                                                StoreValueToArrayReference(objRef, val, t, mStack);
                                            }
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    Free(esp - 1);
                                    Free(esp - 2);
                                    esp -= 2;
                                }
                                break;
                            #endregion

                            #region Load Constants
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
                            case OpCodeEnum.Ldc_I8:
                                {
                                    *(long*)(&esp->Value) = ip->TokenLong;
                                    esp->ObjectType = ObjectTypes.Long;
                                    esp++;
                                }
                                break;
                            case OpCodeEnum.Ldc_R4:
                                {
                                    *(float*)(&esp->Value) = ip->TokenFloat;
                                    esp->ObjectType = ObjectTypes.Float;
                                    esp++;
                                }
                                break;
                            case OpCodeEnum.Ldc_R8:
                                {
                                    *(double*)(&esp->Value) = ip->TokenDouble;
                                    esp->ObjectType = ObjectTypes.Double;
                                    esp++;
                                }
                                break;
                            case OpCodeEnum.Ldnull:
                                {
                                    esp = PushNull(esp);
                                }
                                break;
                            case OpCodeEnum.Ldind_I:
                            case OpCodeEnum.Ldind_I1:
                            case OpCodeEnum.Ldind_I2:
                            case OpCodeEnum.Ldind_I4:
                            case OpCodeEnum.Ldind_U1:
                            case OpCodeEnum.Ldind_U2:
                            case OpCodeEnum.Ldind_U4:
                                {
                                    var val = GetObjectAndResolveReference(esp - 1);
                                    var dst = esp - 1;
                                    dst->ObjectType = ObjectTypes.Integer;
                                    dst->Value = val->Value;
                                    dst->ValueLow = 0;
                                }
                                break;
                            case OpCodeEnum.Ldind_I8:
                                {
                                    var val = GetObjectAndResolveReference(esp - 1);
                                    var dst = esp - 1;
                                    *dst = *val;
                                    dst->ObjectType = ObjectTypes.Long;
                                }
                                break;
                            case OpCodeEnum.Ldind_R4:
                                {
                                    var val = GetObjectAndResolveReference(esp - 1);
                                    var dst = esp - 1;
                                    dst->ObjectType = ObjectTypes.Float;
                                    dst->Value = val->Value;
                                    dst->ValueLow = 0;
                                }
                                break;
                            case OpCodeEnum.Ldind_R8:
                                {
                                    var val = GetObjectAndResolveReference(esp - 1);
                                    var dst = esp - 1;
                                    *dst = *val;
                                    dst->ObjectType = ObjectTypes.Double;
                                }
                                break;
                            case OpCodeEnum.Ldind_Ref:
                                {
                                    var val = GetObjectAndResolveReference(esp - 1);
                                    var dst = esp - 1;
                                    dst->ObjectType = ObjectTypes.Object;
                                    dst->Value = mStack.Count;                                    
                                    mStack.Add(mStack[val->Value]);
                                }
                                break;
                            case OpCodeEnum.Stind_I:
                            case OpCodeEnum.Stind_I1:
                            case OpCodeEnum.Stind_I2:
                            case OpCodeEnum.Stind_I4:
                            case OpCodeEnum.Stind_R4:
                                {
                                    var dst = GetObjectAndResolveReference(esp - 2);
                                    var val = esp - 1;
                                    dst->Value = val->Value;
                                    Free(esp - 1);
                                    Free(esp - 2);
                                    esp -= 2;
                                }
                                break;
                            case OpCodeEnum.Stind_I8:
                            case OpCodeEnum.Stind_R8:
                                {
                                    var dst = GetObjectAndResolveReference(esp - 2);
                                    var val = esp - 1;
                                    dst->Value = val->Value;
                                    dst->ValueLow = val->ValueLow;
                                    Free(esp - 1);
                                    Free(esp - 2);
                                    esp -= 2;
                                }
                                break;
                            case OpCodeEnum.Stind_Ref:
                                {
                                    var dst = GetObjectAndResolveReference(esp - 2);
                                    var val = esp - 1;
                                    mStack[dst->Value] =  mStack[val->Value];
                                    Free(esp - 1);
                                    Free(esp - 2);
                                    esp -= 2;
                                }
                                break;
                            case OpCodeEnum.Ldstr:
                                esp = PushObject(esp, mStack, AppDomain.GetString(ip->TokenInteger));
                                break;
                            #endregion

                            #region Althemetics
                            case OpCodeEnum.Add:
                                {
                                    StackObject* b = esp - 1;
                                    StackObject* a = esp - 2;
                                    esp = esp - 2;
                                    switch (a->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            esp->ObjectType = ObjectTypes.Long;
                                            *((long*)&esp->Value) = *((long*)&a->Value) + *((long*)&b->Value);
                                            break;
                                        case ObjectTypes.Integer:
                                            esp->ObjectType = ObjectTypes.Integer;
                                            esp->Value = a->Value + b->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            esp->ObjectType = ObjectTypes.Float;
                                            *((float*)&esp->Value) = *((float*)&a->Value) + *((float*)&b->Value);
                                            break;
                                        case ObjectTypes.Double:
                                            esp->ObjectType = ObjectTypes.Double;
                                            *((double*)&esp->Value) = *((double*)&a->Value) + *((double*)&b->Value);
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    esp++;
                                }
                                break;
                            case OpCodeEnum.Sub:
                                {
                                    StackObject* b = esp - 1;
                                    StackObject* a = esp - 2;
                                    esp = esp - 2;
                                    switch (a->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            esp->ObjectType = ObjectTypes.Long;
                                            *((long*)&esp->Value) = *((long*)&a->Value) - *((long*)&b->Value);
                                            break;
                                        case ObjectTypes.Integer:
                                            esp->ObjectType = ObjectTypes.Integer;
                                            esp->Value = a->Value - b->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            esp->ObjectType = ObjectTypes.Float;
                                            *((float*)&esp->Value) = *((float*)&a->Value) - *((float*)&b->Value);
                                            break;
                                        case ObjectTypes.Double:
                                            esp->ObjectType = ObjectTypes.Double;
                                            *((double*)&esp->Value) = *((double*)&a->Value) - *((double*)&b->Value);
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    esp++;
                                }
                                break;
                            case OpCodeEnum.Mul:
                                {
                                    StackObject* b = esp - 1;
                                    StackObject* a = esp - 2;
                                    esp = esp - 2;
                                    switch (a->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            esp->ObjectType = ObjectTypes.Long;
                                            *((long*)&esp->Value) = *((long*)&a->Value) * *((long*)&b->Value);
                                            break;
                                        case ObjectTypes.Integer:
                                            esp->ObjectType = ObjectTypes.Integer;
                                            esp->Value = a->Value * b->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            esp->ObjectType = ObjectTypes.Float;
                                            *((float*)&esp->Value) = *((float*)&a->Value) * *((float*)&b->Value);
                                            break;
                                        case ObjectTypes.Double:
                                            esp->ObjectType = ObjectTypes.Double;
                                            *((double*)&esp->Value) = *((double*)&a->Value) * *((double*)&b->Value);
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    esp++;
                                }
                                break;
                            case OpCodeEnum.Div:
                                {
                                    StackObject* b = esp - 1;
                                    StackObject* a = esp - 2;
                                    esp = esp - 2;
                                    switch (a->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            esp->ObjectType = ObjectTypes.Long;
                                            *((long*)&esp->Value) = *((long*)&a->Value) / *((long*)&b->Value);
                                            break;
                                        case ObjectTypes.Integer:
                                            esp->ObjectType = ObjectTypes.Integer;
                                            esp->Value = a->Value / b->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            esp->ObjectType = ObjectTypes.Long;
                                            *((float*)&esp->Value) = *((float*)&a->Value) / *((float*)&b->Value);
                                            break;
                                        case ObjectTypes.Double:
                                            esp->ObjectType = ObjectTypes.Long;
                                            *((double*)&esp->Value) = *((double*)&a->Value) / *((double*)&b->Value);
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    esp++;
                                }
                                break;
                            case OpCodeEnum.Rem:
                                {
                                    StackObject* b = esp - 1;
                                    StackObject* a = esp - 2;
                                    esp = esp - 2;
                                    switch (a->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            esp->ObjectType = ObjectTypes.Long;
                                            *((long*)&esp->Value) = *((long*)&a->Value) % *((long*)&b->Value);
                                            break;
                                        case ObjectTypes.Integer:
                                            esp->ObjectType = ObjectTypes.Integer;
                                            esp->Value = a->Value % b->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    esp++;
                                }
                                break;
                            case OpCodeEnum.Rem_Un:
                                {
                                    StackObject* b = esp - 1;
                                    StackObject* a = esp - 2;
                                    esp = esp - 2;
                                    switch (a->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            esp->ObjectType = ObjectTypes.Long;
                                            *((ulong*)&esp->Value) = *((ulong*)&a->Value) % *((ulong*)&b->Value);
                                            break;
                                        case ObjectTypes.Integer:
                                            esp->ObjectType = ObjectTypes.Integer;
                                            esp->Value = (int)((uint)a->Value % (uint)b->Value);
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    esp++;
                                }
                                break;
                            case OpCodeEnum.Xor:
                                {
                                    StackObject* b = esp - 1;
                                    StackObject* a = esp - 2;
                                    esp = esp - 2;
                                    switch (a->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            esp->ObjectType = ObjectTypes.Long;
                                            *((long*)&esp->Value) = *((long*)&a->Value) ^ *((long*)&b->Value);
                                            break;
                                        case ObjectTypes.Integer:
                                            esp->ObjectType = ObjectTypes.Integer;
                                            esp->Value = a->Value ^ b->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    esp++;
                                }
                                break;
                            case OpCodeEnum.And:
                                {
                                    StackObject* b = esp - 1;
                                    StackObject* a = esp - 2;
                                    esp = esp - 2;
                                    switch (a->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            esp->ObjectType = ObjectTypes.Long;
                                            *((long*)&esp->Value) = *((long*)&a->Value) & *((long*)&b->Value);
                                            break;
                                        case ObjectTypes.Integer:
                                            esp->ObjectType = ObjectTypes.Integer;
                                            esp->Value = a->Value & b->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    esp++;
                                }
                                break;
                            case OpCodeEnum.Or:
                                {
                                    StackObject* b = esp - 1;
                                    StackObject* a = esp - 2;
                                    esp = esp - 2;
                                    switch (a->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            esp->ObjectType = ObjectTypes.Long;
                                            *((long*)&esp->Value) = *((long*)&a->Value) | *((long*)&b->Value);
                                            break;
                                        case ObjectTypes.Integer:
                                            esp->ObjectType = ObjectTypes.Integer;
                                            esp->Value = a->Value | b->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    esp++;
                                }
                                break;
                            case OpCodeEnum.Shl:
                                {
                                    StackObject* b = esp - 1;
                                    StackObject* a = esp - 2;
                                    esp = esp - 2;
                                    int bits = b->Value;
                                    switch (a->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            esp->ObjectType = ObjectTypes.Long;
                                            *((long*)&esp->Value) = *((long*)&a->Value) << bits;
                                            break;
                                        case ObjectTypes.Integer:
                                            esp->ObjectType = ObjectTypes.Integer;
                                            esp->Value = a->Value << bits;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    esp++;
                                }
                                break;
                            case OpCodeEnum.Shr:
                                {
                                    StackObject* b = esp - 1;
                                    StackObject* a = esp - 2;
                                    esp = esp - 2;
                                    int bits = b->Value;
                                    switch (a->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            esp->ObjectType = ObjectTypes.Long;
                                            *((long*)&esp->Value) = *((long*)&a->Value) >> bits;
                                            break;
                                        case ObjectTypes.Integer:
                                            esp->ObjectType = ObjectTypes.Integer;
                                            esp->Value = a->Value >> bits;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    esp++;
                                }
                                break;
                            case OpCodeEnum.Shr_Un:
                                {
                                    StackObject* b = esp - 1;
                                    StackObject* a = esp - 2;
                                    esp = esp - 2;
                                    int bits = b->Value;
                                    switch (a->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            esp->ObjectType = ObjectTypes.Long;
                                            *((ulong*)&esp->Value) = *((ulong*)&a->Value) >> bits;
                                            break;
                                        case ObjectTypes.Integer:
                                            esp->ObjectType = ObjectTypes.Integer;
                                            *(uint*)&esp->Value = (uint)a->Value >> bits;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    esp++;
                                }
                                break;
                            case OpCodeEnum.Not:
                                {
                                    StackObject* a = esp - 1;
                                    switch (a->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            a->ObjectType = ObjectTypes.Long;
                                            *((long*)&a->Value) = ~*((long*)&a->Value);
                                            break;
                                        case ObjectTypes.Integer:
                                            a->ObjectType = ObjectTypes.Integer;
                                            a->Value = ~a->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                }
                                break;
                            case OpCodeEnum.Neg:
                                {
                                    StackObject* a = esp - 1;
                                    switch (a->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            a->ObjectType = ObjectTypes.Long;
                                            *((long*)&a->Value) = -*((long*)&a->Value);
                                            break;
                                        case ObjectTypes.Integer:
                                            a->ObjectType = ObjectTypes.Integer;
                                            a->Value = -a->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            a->ObjectType = ObjectTypes.Float;
                                            *((float*)&a->Value) = -*((float*)&a->Value);
                                            break;
                                        case ObjectTypes.Double:
                                            a->ObjectType = ObjectTypes.Double;
                                            *((double*)&a->Value) = -*((double*)&a->Value);
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                }
                                break;
                            #endregion

                            #region Control Flows
                            case OpCodeEnum.Ret:
                                returned = true;
                                break;
                            case OpCodeEnum.Brtrue:
                            case OpCodeEnum.Brtrue_S:
                                {
                                    esp--;
                                    if ((esp->ObjectType == ObjectTypes.Integer && esp->Value > 0) || (esp->ObjectType == ObjectTypes.Object && esp->Value >= 0))
                                    {
                                        ip = ptr + ip->TokenInteger;
                                        Free(esp);
                                        continue;
                                    }
                                    else
                                        Free(esp);
                                }
                                break;
                            case OpCodeEnum.Brfalse:
                            case OpCodeEnum.Brfalse_S:
                                {
                                    esp--;
                                    if (esp->ObjectType == ObjectTypes.Null || (esp->ObjectType == ObjectTypes.Integer && esp->Value == 0))
                                    {
                                        ip = ptr + ip->TokenInteger;
                                        continue;
                                    }
                                    else
                                        Free(esp);
                                }
                                break;
                            case OpCodeEnum.Beq:
                            case OpCodeEnum.Beq_S:
                                {
                                    var b = esp - 1;
                                    var a = esp - 2;
                                    bool transfer = false;
                                    if (a->ObjectType == b->ObjectType)
                                    {
                                        switch (a->ObjectType)
                                        {
                                            case ObjectTypes.Integer:
                                            case ObjectTypes.Object:
                                                transfer = a->Value == b->Value;
                                                break;
                                            case ObjectTypes.Long:
                                                 transfer = *(long*)&a->Value == *(long*)&b->Value;
                                                break;
                                            case ObjectTypes.Float:
                                                transfer = *(float*)&a->Value == *(float*)&b->Value;
                                                break;
                                            case ObjectTypes.Double:
                                                transfer = *(double*)&a->Value == *(double*)&b->Value;
                                                break;
                                            default:
                                                throw new NotImplementedException();
                                        }
                                    }
                                    Free(esp - 1);
                                    Free(esp - 2);
                                    esp -= 2;
                                    if (transfer)
                                    {
                                        ip = ptr + ip->TokenInteger;
                                        continue;
                                    }

                                }
                                break;
                            case OpCodeEnum.Bne_Un:
                            case OpCodeEnum.Bne_Un_S:
                                {
                                    var b = esp - 1;
                                    var a = esp - 2;
                                    bool transfer = false;
                                    if (a->ObjectType == b->ObjectType)
                                    {
                                        switch (a->ObjectType)
                                        {
                                            case ObjectTypes.Integer:
                                            case ObjectTypes.Object:
                                                transfer = (uint)a->Value != (uint)b->Value;
                                                break;
                                            case ObjectTypes.Float:
                                                transfer = *(float*)&a->Value != *(float*)&b->Value;
                                                break;
                                            case ObjectTypes.Long:
                                                transfer = *(long*)&a->Value != *(long*)&b->Value;
                                                break;
                                            case ObjectTypes.Double:
                                                transfer = *(double*)&a->Value != *(double*)&b->Value;
                                                break;
                                            default:
                                                throw new NotImplementedException();
                                        }
                                    }
                                    else
                                        transfer = true;
                                    Free(esp - 1);
                                    Free(esp - 2);
                                    esp -= 2;
                                    if (transfer)
                                    {
                                        ip = ptr + ip->TokenInteger;
                                        continue;
                                    }

                                }
                                break;
                            case OpCodeEnum.Bgt:
                            case OpCodeEnum.Bgt_S:
                                {
                                    var b = esp - 1;
                                    var a = esp - 2;
                                    esp -= 2;
                                    bool transfer = false;
                                    switch (esp->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            transfer = a->Value > b->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            transfer = *(float*)&a->Value > *(float*)&b->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }

                                    if (transfer)
                                    {
                                        ip = ptr + ip->TokenInteger;
                                        continue;
                                    }

                                }
                                break;
                            case OpCodeEnum.Bgt_Un:
                            case OpCodeEnum.Bgt_Un_S:
                                {
                                    var b = esp - 1;
                                    var a = esp - 2;
                                    esp -= 2;
                                    bool transfer = false;
                                    switch (esp->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            transfer = (uint)a->Value > (uint)b->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            transfer = *(float*)&a->Value > *(float*)&b->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }

                                    if (transfer)
                                    {
                                        ip = ptr + ip->TokenInteger;
                                        continue;
                                    }

                                }
                                break;
                            case OpCodeEnum.Bge:
                            case OpCodeEnum.Bge_S:
                                {
                                    var b = esp - 1;
                                    var a = esp - 2;
                                    esp -= 2;
                                    bool transfer = false;
                                    switch (esp->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            transfer = a->Value >= b->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            transfer = *(float*)&a->Value >= *(float*)&b->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }

                                    if (transfer)
                                    {
                                        ip = ptr + ip->TokenInteger;
                                        continue;
                                    }

                                }
                                break;
                            case OpCodeEnum.Bge_Un:
                            case OpCodeEnum.Bge_Un_S:
                                {
                                    var b = esp - 1;
                                    var a = esp - 2;
                                    esp -= 2;
                                    bool transfer = false;
                                    switch (esp->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            transfer = (uint)a->Value >= (uint)b->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            transfer = *(float*)&a->Value >= *(float*)&b->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }

                                    if (transfer)
                                    {
                                        ip = ptr + ip->TokenInteger;
                                        continue;
                                    }

                                }
                                break;
                            case OpCodeEnum.Blt:
                            case OpCodeEnum.Blt_S:
                                {
                                    var b = esp - 1;
                                    var a = esp - 2;
                                    esp -= 2;
                                    bool transfer = false;
                                    switch (esp->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            transfer = a->Value < b->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            transfer = *(float*)&a->Value < *(float*)&b->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }

                                    if (transfer)
                                    {
                                        ip = ptr + ip->TokenInteger;
                                        continue;
                                    }

                                }
                                break;
                            case OpCodeEnum.Blt_Un:
                            case OpCodeEnum.Blt_Un_S:
                                {
                                    var b = esp - 1;
                                    var a = esp - 2;
                                    esp -= 2;
                                    bool transfer = false;
                                    switch (esp->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            transfer = (uint)a->Value < (uint)b->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            transfer = *(float*)&a->Value < *(float*)&b->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }

                                    if (transfer)
                                    {
                                        ip = ptr + ip->TokenInteger;
                                        continue;
                                    }

                                }
                                break;
                            case OpCodeEnum.Ble:
                            case OpCodeEnum.Ble_S:
                                {
                                    var b = esp - 1;
                                    var a = esp - 2;
                                    esp -= 2;
                                    bool transfer = false;
                                    switch (esp->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            transfer = a->Value <= b->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            transfer = *(float*)&a->Value <= *(float*)&b->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }

                                    if (transfer)
                                    {
                                        ip = ptr + ip->TokenInteger;
                                        continue;
                                    }

                                }
                                break;
                            case OpCodeEnum.Ble_Un:
                            case OpCodeEnum.Ble_Un_S:
                                {
                                    var b = esp - 1;
                                    var a = esp - 2;
                                    esp -= 2;
                                    bool transfer = false;
                                    switch (esp->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            transfer = (uint)a->Value <= (uint)b->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            transfer = *(float*)&a->Value <= *(float*)&b->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }

                                    if (transfer)
                                    {
                                        ip = ptr + ip->TokenInteger;
                                        continue;
                                    }

                                }
                                break;
                            case OpCodeEnum.Br_S:
                            case OpCodeEnum.Br:
                                ip = ptr + ip->TokenInteger;
                                continue;
                            case OpCodeEnum.Switch:
                                {
                                    var val = (esp - 1)->Value;
                                    Free(esp - 1);
                                    esp--;
                                    var table = method.JumpTables[ip->TokenInteger];
                                    if (val >=0 && val < table.Length)
                                    {
                                        ip = ptr + table[val];
                                        continue;
                                    }
                                }
                                break;
                            case OpCodeEnum.Leave:
                            case OpCodeEnum.Leave_S:
                                {
                                    if (method.ExceptionHandler != null)
                                    {
                                        int addr = (int)(ip - ptr);
                                        var sql = from e in method.ExceptionHandler
                                                  where addr >= e.TryStart && addr <= e.TryEnd && e.HandlerType == ExceptionHandlerType.Finally
                                                  select e;
                                        var eh = sql.FirstOrDefault();
                                        if (eh != null)
                                        {
                                            ip = ptr + eh.HandlerStart;
                                            continue;
                                        }
                                    }
                                    ip = ptr + ip->TokenInteger;
                                    continue;
                                }
                            case OpCodeEnum.Call:
                            case OpCodeEnum.Callvirt:
                                {
                                    IMethod m = domain.GetMethod(ip->TokenInteger);
                                    if (m == null)
                                    {
                                        //Irrelevant method
                                        Free(esp - 1);
                                        esp--;
                                    }
                                    else
                                    {
                                        if (m is ILMethod)
                                        {
                                            ILMethod ilm = (ILMethod)m;
                                            bool processed = false;
                                            if (m.IsDelegateInvoke)
                                            {
                                                var instance = (esp - m.ParameterCount - 1)->ToObject(domain, mStack);
                                                if (instance is IDelegateAdapter)
                                                {
                                                    esp = ((IDelegateAdapter)instance).ILInvoke(this, esp, mStack);
                                                    processed = true;
                                                }
                                            }
                                            if (!processed)
                                            {
                                                if (code == OpCodeEnum.Callvirt)
                                                {
                                                    var objRef = esp - ilm.ParameterCount - 1;
                                                    if (objRef->ObjectType == ObjectTypes.Null)
                                                        throw new NullReferenceException();
                                                    var obj = mStack[objRef->Value];
                                                    ilm = ((ILTypeInstance)obj).Type.GetVirtualMethod(ilm) as ILMethod;
                                                }
                                                esp = Execute(ilm, esp, out unhandledException);
                                                if (unhandledException)
                                                    returned = true;
                                            }
                                        }
                                        else
                                        {
                                            CLRMethod cm = (CLRMethod)m;
                                            bool processed = false;
                                            if (cm.IsDelegateInvoke)
                                            {
                                                var instance = (esp - cm.ParameterCount - 1)->ToObject(domain, mStack);
                                                if (instance is IDelegateAdapter)
                                                {
                                                    esp = ((IDelegateAdapter)instance).ILInvoke(this, esp, mStack);
                                                    processed = true;
                                                }
                                            }

                                            if (!processed)
                                            {
                                                object result = cm.Invoke(esp, mStack);
                                                int paramCount = cm.ParameterCount;
                                                for (int i = 1; i <= paramCount; i++)
                                                {
                                                    Free(esp - i);
                                                }
                                                esp -= paramCount;
                                                if (cm.HasThis)
                                                {
                                                    Free(esp - 1);
                                                    esp--;
                                                }
                                                if (cm.ReturnType != AppDomain.VoidType && !cm.IsConstructor)
                                                {
                                                    esp = PushObject(esp, mStack, result, cm.ReturnType.TypeForCLR == typeof(object));
                                                }
                                            }
                                        }

                                    }
                                }
                                break;
                            #endregion

                            #region FieldOperation
                            case OpCodeEnum.Stfld:
                                {
                                    StackObject* objRef = GetObjectAndResolveReference(esp - 2);
                                    var obj = mStack[objRef->Value];
                                    if (obj != null)
                                    {
                                        if (obj is ILTypeInstance)
                                        {
                                            ILTypeInstance instance = obj as ILTypeInstance;
                                            StackObject* val = esp - 1;
                                            instance.AssignFromStack(ip->TokenInteger, val, AppDomain, mStack);
                                        }
                                        else
                                        {
                                            var t = obj.GetType();
                                            var type = AppDomain.GetType(t);
                                            if (type != null)
                                            {
                                                var val = esp - 1;
                                                var f = ((CLRType)type).Fields[ip->TokenInteger];
                                                f.SetValue(obj, f.FieldType.CheckCLRTypes(domain, val->ToObject(domain, mStack)));
                                            }
                                            else
                                                throw new TypeLoadException();
                                        }
                                    }
                                    else
                                        throw new NullReferenceException();
                                    Free(esp - 1);
                                    Free(esp - 2);
                                    esp -= 2;
                                }
                                break;
                            case OpCodeEnum.Ldfld:
                                {
                                    StackObject* objRef = GetObjectAndResolveReference(esp - 1);
                                    var obj = mStack[objRef->Value];
                                    Free(esp - 1);
                                    if (obj != null)
                                    {
                                        if (obj is ILTypeInstance)
                                        {
                                            ILTypeInstance instance = obj as ILTypeInstance;
                                            instance.PushToStack(ip->TokenInteger, esp - 1, AppDomain, mStack);
                                        }
                                        else
                                        {
                                            var t = obj.GetType();
                                            var type = AppDomain.GetType(t);
                                            if (type != null)
                                            {
                                                var val = ((CLRType)type).Fields[ip->TokenInteger].GetValue(obj);
                                                PushObject(esp - 1, mStack, val);
                                            }
                                            else
                                                throw new TypeLoadException();
                                        }
                                    }
                                    else
                                        throw new NullReferenceException();

                                }
                                break;
                            case OpCodeEnum.Ldflda:
                                {
                                    StackObject* objRef = GetObjectAndResolveReference(esp - 1);
                                    var obj = mStack[objRef->Value];
                                    Free(esp - 1);
                                    if (obj != null)
                                    {
                                        if (obj is ILTypeInstance)
                                        {
                                            ILTypeInstance instance = obj as ILTypeInstance;
                                            instance.PushFieldAddress(ip->TokenInteger, esp - 1, mStack);
                                        }
                                        else
                                        {
                                            objRef->ObjectType = ObjectTypes.FieldReference;
                                            objRef->Value = mStack.Count;
                                            mStack.Add(obj);
                                            objRef->ValueLow = ip->TokenInteger;
                                        }
                                    }
                                    else
                                        throw new NullReferenceException();
                                }
                                break;
                            case OpCodeEnum.Stsfld:
                                {
                                    IType type = AppDomain.GetType((int)(ip->TokenLong >> 32));
                                    if (type != null)
                                    {
                                        if (type is ILType)
                                        {
                                            ILType t = type as ILType;
                                            StackObject* val = esp - 1;
                                            t.StaticInstance.AssignFromStack((int)ip->TokenLong, val, AppDomain, mStack);
                                        }
                                        else
                                        {
                                            CLRType t = type as CLRType;
                                            int idx = (int)ip->TokenLong;
                                            var f = t.Fields[idx];
                                            StackObject* val = esp - 1;
                                            f.SetValue(null, f.FieldType.CheckCLRTypes(domain, val->ToObject(domain, mStack)));
                                        }
                                    }
                                    else
                                        throw new TypeLoadException();
                                    Free(esp - 1);
                                    esp -= 1;
                                }
                                break;
                            case OpCodeEnum.Ldsfld:
                                {
                                    IType type = AppDomain.GetType((int)(ip->TokenLong >> 32));
                                    if (type != null)
                                    {
                                        if (type is ILType)
                                        {
                                            ILType t = type as ILType;
                                            t.StaticInstance.PushToStack((int)ip->TokenLong, esp, AppDomain, mStack);
                                        }
                                        else
                                        {
                                            CLRType t = type as CLRType;
                                            int idx = (int)ip->TokenLong;
                                            var f = t.Fields[idx];
                                            var val = f.GetValue(null);
                                            PushObject(esp, mStack, val);
                                        }
                                    }
                                    else
                                        throw new TypeLoadException();
                                    esp++;
                                }
                                break;
                            case OpCodeEnum.Ldsflda:
                                {
                                    int type = (int)(ip->TokenLong >> 32);
                                    int fieldIdx = (int)(ip->TokenLong);
                                    esp->ObjectType = ObjectTypes.StaticFieldReference;
                                    esp->Value = type;
                                    esp->ValueLow = fieldIdx;
                                    esp++;
                                }
                                break;
                            case OpCodeEnum.Ldtoken:
                                {
                                    switch (ip->TokenInteger)
                                    {
                                        case 0:
                                            {
                                                IType type = AppDomain.GetType((int)(ip->TokenLong >> 32));
                                                if (type != null)
                                                {
                                                    if (type is ILType)
                                                    {
                                                        ILType t = type as ILType;
                                                        t.StaticInstance.PushToStack((int)ip->TokenLong, esp, AppDomain, mStack);
                                                    }
                                                    else
                                                        throw new NotImplementedException();
                                                }
                                            }
                                            esp++;
                                            break;
                                        case 1:
                                            {
                                                IType type = AppDomain.GetType((int)ip->TokenLong);
                                                if (type != null)
                                                {
                                                    esp = PushObject(esp, mStack, type.TypeForCLR);
                                                }
                                                else
                                                    throw new TypeLoadException();
                                            }
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                }
                                break;
                            case OpCodeEnum.Ldftn:
                                {
                                    IMethod m = domain.GetMethod(ip->TokenInteger);
                                    esp = PushObject(esp, mStack, m);
                                }
                                break;
                            #endregion

                            #region Compare
                            case OpCodeEnum.Ceq:
                                {
                                    StackObject* obj1 = esp - 2;
                                    StackObject* obj2 = esp - 1;
                                    bool res = false;
                                    if (obj1->ObjectType == obj2->ObjectType)
                                    {
                                        switch (obj1->ObjectType)
                                        {
                                            case ObjectTypes.Integer:
                                            case ObjectTypes.Float:
                                                res = obj1->Value == obj2->Value;
                                                break;
                                            case ObjectTypes.Object:
                                                res = mStack[obj1->Value] == mStack[obj2->Value];
                                                break;
                                            case ObjectTypes.FieldReference:
                                                res = mStack[obj1->Value] == mStack[obj2->Value] && obj1->ValueLow == obj2->ValueLow;
                                                break;
                                            case ObjectTypes.Null:
                                                res = true;
                                                break;
                                            default:
                                                res = obj1->Value == obj2->Value && obj1->ValueLow == obj2->ValueLow;
                                                break;
                                        }
                                    }
                                    Free(esp - 1);
                                    Free(esp - 2);
                                    if (res)
                                        esp = PushOne(esp - 2);
                                    else
                                        esp = PushZero(esp - 2);

                                }
                                break;
                            case OpCodeEnum.Clt:
                                {
                                    StackObject* obj1 = esp - 2;
                                    StackObject* obj2 = esp - 1;
                                    bool res = false;
                                    switch (obj1->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            res = obj1->Value < obj2->Value;
                                            break;
                                        case ObjectTypes.Long:
                                            res = *(long*)&obj1->Value < *(long*)&obj2->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            res = *(float*)&obj1->Value < *(float*)&obj2->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    if (res)
                                        esp = PushOne(esp - 2);
                                    else
                                        esp = PushZero(esp - 2);
                                }
                                break;
                            case OpCodeEnum.Clt_Un:
                                {
                                    StackObject* obj1 = esp - 2;
                                    StackObject* obj2 = esp - 1;
                                    bool res = false;
                                    switch (obj1->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            res = (uint)obj1->Value < (uint)obj2->Value;
                                            break;
                                        case ObjectTypes.Long:
                                            res = (ulong)*(long*)&obj1->Value < (ulong)*(long*)&obj2->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            res = *(float*)&obj1->Value < *(float*)&obj2->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    if (res)
                                        esp = PushOne(esp - 2);
                                    else
                                        esp = PushZero(esp - 2);
                                }
                                break;
                            case OpCodeEnum.Cgt:
                                {
                                    StackObject* obj1 = esp - 2;
                                    StackObject* obj2 = esp - 1;
                                    bool res = false;
                                    switch (obj1->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            res = obj1->Value > obj2->Value;
                                            break;
                                        case ObjectTypes.Long:
                                            res = *(long*)&obj1->Value > *(long*)&obj2->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            res = *(float*)&obj1->Value > *(float*)&obj2->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    if (res)
                                        esp = PushOne(esp - 2);
                                    else
                                        esp = PushZero(esp - 2);
                                }
                                break;
                            case OpCodeEnum.Cgt_Un:
                                {
                                    StackObject* obj1 = esp - 2;
                                    StackObject* obj2 = esp - 1;
                                    bool res = false;
                                    switch (obj1->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            res = (uint)obj1->Value > (uint)obj2->Value;
                                            break;
                                        case ObjectTypes.Long:
                                            res = (ulong)*(long*)&obj1->Value > (ulong)*(long*)&obj2->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            res = *(float*)&obj1->Value < *(float*)&obj2->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    if (res)
                                        esp = PushOne(esp - 2);
                                    else
                                        esp = PushZero(esp - 2);
                                }
                                break;
                            #endregion

                            #region Initialization & Instantiation
                            case OpCodeEnum.Newobj:
                                {
                                    IMethod m = domain.GetMethod(ip->TokenInteger);
                                    if (m is ILMethod)
                                    {
                                        ILType type = m.DeclearingType as ILType;
                                        if (type.IsDelegate)
                                        {
                                            var objRef = esp - 2;
                                            var mi = (IMethod)mStack[(esp - 1)->Value];
                                            object ins;
                                            if (objRef->ObjectType == ObjectTypes.Null)
                                                ins = null;
                                            else
                                                ins = mStack[objRef->Value];
                                            Free(esp - 1);
                                            Free(esp - 2);
                                            esp -= 2;
                                            object dele;
                                            if (mi is ILMethod)
                                            {
                                                if (ins != null)
                                                {
                                                    dele = ((ILTypeInstance)ins).GetDelegateAdapter((ILMethod)mi);
                                                    if (dele == null)
                                                        dele = domain.DelegateManager.FindDelegateAdapter((ILTypeInstance)ins, (ILMethod)mi);
                                                }
                                                else
                                                {
                                                    if (((ILMethod)mi).DelegateAdapter == null)
                                                    {
                                                        ((ILMethod)mi).DelegateAdapter = domain.DelegateManager.FindDelegateAdapter(null, (ILMethod)mi);
                                                    }
                                                    dele = ((ILMethod)mi).DelegateAdapter;
                                                }
                                            }

                                            else
                                            {
                                                throw new NotImplementedException();
                                            }
                                            esp = PushObject(esp, mStack, dele);
                                        }
                                        else
                                        {
                                            var obj = type.Instantiate();
                                            var a = esp - m.ParameterCount;
                                            var objRef = PushObject(esp, mStack, obj);//this parameter for constructor
                                            esp = objRef;
                                            for (int i = 0; i < m.ParameterCount; i++)
                                            {
                                                CopyToStack(esp, a + i, mStack);
                                                esp++;
                                            }
                                            esp = Execute((ILMethod)m, esp, out unhandledException);
                                            for (int i = m.ParameterCount - 1; i >= 0; i--)
                                            {
                                                Free(a + i);
                                            }
                                            Free(objRef - 1);
                                            esp = a;
                                            esp = PushObject(esp, mStack, obj);//new constructedObj
                                        }
                                        if (unhandledException)
                                            returned = true;
                                    }
                                    else
                                    {
                                        CLRMethod cm = (CLRMethod)m;
                                        if (cm.DeclearingType.IsDelegate)
                                        {
                                            var objRef = esp - 2;
                                            var mi = (IMethod)mStack[(esp - 1)->Value];
                                            object ins;
                                            if (objRef->ObjectType == ObjectTypes.Null)
                                                ins = null;
                                            else
                                                ins = mStack[objRef->Value];
                                            Free(esp - 1);
                                            Free(esp - 2);
                                            esp -= 2;
                                            object dele;
                                            if (mi is ILMethod)
                                            {
                                                if (ins != null)
                                                {
                                                    dele = ((ILTypeInstance)ins).GetDelegateAdapter((ILMethod)mi);
                                                    if (dele == null)
                                                        dele = domain.DelegateManager.FindDelegateAdapter((ILTypeInstance)ins, (ILMethod)mi);
                                                }
                                                else
                                                {
                                                    if (((ILMethod)mi).DelegateAdapter == null)
                                                    {
                                                        ((ILMethod)mi).DelegateAdapter = domain.DelegateManager.FindDelegateAdapter(null, (ILMethod)mi);
                                                    }
                                                    dele = ((ILMethod)mi).DelegateAdapter;
                                                }
                                            }
                                            else
                                            {
                                                throw new NotImplementedException();
                                            }
                                            esp = PushObject(esp, mStack, dele);
                                        }
                                        else
                                        {
                                            object result = cm.Invoke(esp, mStack, true);
                                            int paramCount = cm.ParameterCount;
                                            for (int i = 1; i <= paramCount; i++)
                                            {
                                                Free(esp - i);
                                            }
                                            esp -= paramCount;
                                            esp = PushObject(esp, mStack, result);//new constructedObj
                                        }
                                    }
                                }
                                break;
                            case OpCodeEnum.Constrained:
                                {
                                    var obj = GetObjectAndResolveReference(esp - 1);
                                    var type = domain.GetType(ip->TokenInteger);
                                    if (type != null)
                                    {
                                        if (type is ILType)
                                        {
                                            var t = (ILType)type;
                                            if (t.IsEnum)
                                            {
                                                ILEnumTypeInstance ins = new ILEnumTypeInstance(t);
                                                ins.AssignFromStack(0, obj, AppDomain, mStack);
                                                ins.Boxed = true;
                                                esp = PushObject(esp - 1, mStack, ins);
                                            }
                                            else
                                            {
                                                //Nothing to do for normal IL Types
                                            }
                                        }
                                        else
                                        {
                                            if (type.TypeForCLR.IsEnum)
                                            {
                                                esp = PushObject(esp - 1, mStack, Enum.ToObject(type.TypeForCLR, obj->ToObject(AppDomain, mStack)));
                                            }
                                            else
                                            {
                                                //Nothing to do for other CLR types
                                            }
                                        }
                                    }
                                    else
                                        throw new NullReferenceException();
                                }
                                break;
                            case OpCodeEnum.Box:
                                {
                                    var obj = esp - 1;
                                    var type = domain.GetType(ip->TokenInteger);
                                    if (type != null)
                                    {
                                        if(type is ILType)
                                        {
                                            if (((ILType)type).IsEnum)
                                            {
                                                ILEnumTypeInstance ins = new Intepreter.ILEnumTypeInstance((ILType)type);
                                                ins.AssignFromStack(0, obj, AppDomain, mStack);
                                                ins.Boxed = true;
                                                esp = PushObject(obj, mStack, ins, true);
                                            }
                                            else
                                            {
                                                if (obj->ObjectType != ObjectTypes.Null)
                                                {
                                                    var val = mStack[obj->Value];
                                                    Free(obj);
                                                    ILTypeInstance ins = (ILTypeInstance)val;
                                                    if (ins != null)
                                                    {
                                                        if (ins.IsValueType)
                                                        {
                                                            ins.Boxed = true;
                                                        }
                                                        esp = PushObject(obj, mStack, ins, true);
                                                    }
                                                    else
                                                    {
                                                        esp = PushNull(obj);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (type.TypeForCLR.IsPrimitive)
                                            {
                                                var t = type.TypeForCLR;
                                                if (t == typeof(int))
                                                {
                                                    switch (obj->ObjectType)
                                                    {
                                                        case ObjectTypes.Integer:
                                                            esp = PushObject(obj, mStack, obj->Value, true);
                                                            break;
                                                        case ObjectTypes.Null:
                                                            esp = PushObject(obj, mStack, 0, true);
                                                            break;
                                                        default:
                                                            throw new NotImplementedException();
                                                    }
                                                }
                                                else if (t == typeof(bool))
                                                {
                                                    switch (obj->ObjectType)
                                                    {
                                                        case ObjectTypes.Integer:
                                                            esp = PushObject(obj, mStack, (obj->Value == 1), true);
                                                            break;
                                                        case ObjectTypes.Null:
                                                            esp = PushObject(obj, mStack, false, true);
                                                            break;
                                                        case ObjectTypes.Object:
                                                            break;
                                                        default:
                                                            throw new NotImplementedException();
                                                    }
                                                }
                                                else if (t == typeof(byte))
                                                {
                                                    switch (obj->ObjectType)
                                                    {
                                                        case ObjectTypes.Integer:
                                                            esp = PushObject(obj, mStack, (byte)obj->Value, true);
                                                            break;
                                                        case ObjectTypes.Null:
                                                            esp = PushObject(obj, mStack, 0L, true);
                                                            break;
                                                        default:
                                                            throw new NotImplementedException();
                                                    }
                                                }
                                                else if (t == typeof(short))
                                                {
                                                    switch (obj->ObjectType)
                                                    {
                                                        case ObjectTypes.Integer:
                                                            esp = PushObject(obj, mStack, (short)obj->Value, true);
                                                            break;
                                                        case ObjectTypes.Null:
                                                            esp = PushObject(obj, mStack, 0L, true);
                                                            break;
                                                        default:
                                                            throw new NotImplementedException();
                                                    }
                                                }
                                                else if (t == typeof(long))
                                                {
                                                    switch (obj->ObjectType)
                                                    {
                                                        case ObjectTypes.Long:
                                                            esp = PushObject(obj, mStack, *(long*)&obj->Value, true);
                                                            break;
                                                        case ObjectTypes.Null:
                                                            esp = PushObject(obj, mStack, 0L, true);
                                                            break;
                                                        default:
                                                            throw new NotImplementedException();
                                                    }
                                                }
                                                else if (t == typeof(float))
                                                {
                                                    switch (obj->ObjectType)
                                                    {
                                                        case ObjectTypes.Float:
                                                            esp = PushObject(obj, mStack, *(float*)&obj->Value, true);
                                                            break;
                                                        case ObjectTypes.Null:
                                                            esp = PushObject(obj, mStack, 0f, true);
                                                            break;
                                                        default:
                                                            throw new NotImplementedException();
                                                    }
                                                }
                                                else if (t == typeof(double))
                                                {
                                                    switch (obj->ObjectType)
                                                    {
                                                        case ObjectTypes.Double:
                                                            esp = PushObject(obj, mStack, *(double*)&obj->Value, true);
                                                            break;
                                                        case ObjectTypes.Null:
                                                            esp = PushObject(obj, mStack, 0.0, true);
                                                            break;
                                                        default:
                                                            throw new NotImplementedException();
                                                    }
                                                }
                                                else if (t == typeof(uint))
                                                {
                                                    switch (obj->ObjectType)
                                                    {
                                                        case ObjectTypes.Integer:
                                                            esp = PushObject(obj, mStack, (uint)obj->Value, true);
                                                            break;
                                                        case ObjectTypes.Null:
                                                            esp = PushObject(obj, mStack, 0L, true);
                                                            break;
                                                        default:
                                                            throw new NotImplementedException();
                                                    }
                                                }
                                                else if (t == typeof(ushort))
                                                {
                                                    switch (obj->ObjectType)
                                                    {
                                                        case ObjectTypes.Integer:
                                                            esp = PushObject(obj, mStack, (ushort)obj->Value, true);
                                                            break;
                                                        case ObjectTypes.Null:
                                                            esp = PushObject(obj, mStack, 0L, true);
                                                            break;
                                                        default:
                                                            throw new NotImplementedException();
                                                    }
                                                }
                                                else if (t == typeof(ulong))
                                                {
                                                    switch (obj->ObjectType)
                                                    {
                                                        case ObjectTypes.Long:
                                                            esp = PushObject(obj, mStack, *(ulong*)&obj->Value, true);
                                                            break;
                                                        case ObjectTypes.Null:
                                                            esp = PushObject(obj, mStack, 0L, true);
                                                            break;
                                                        default:
                                                            throw new NotImplementedException();
                                                    }
                                                }
                                                else if (t == typeof(sbyte))
                                                {
                                                    switch (obj->ObjectType)
                                                    {
                                                        case ObjectTypes.Integer:
                                                            esp = PushObject(obj, mStack, (sbyte)obj->Value, true);
                                                            break;
                                                        case ObjectTypes.Null:
                                                            esp = PushObject(obj, mStack, 0L, true);
                                                            break;
                                                        default:
                                                            throw new NotImplementedException();
                                                    }
                                                }
                                                else
                                                    throw new NotImplementedException();
                                            }
                                            else if (type.TypeForCLR.IsEnum)
                                            {
                                                esp = PushObject(obj, mStack, Enum.ToObject(type.TypeForCLR, obj->ToObject(AppDomain, mStack)));
                                            }
                                            else
                                            {
                                                //nothing to do for CLR type boxing
                                            }
                                        }
                                    }
                                    else
                                        throw new NullReferenceException();
                                }
                                break;
                            case OpCodeEnum.Unbox:
                            case OpCodeEnum.Unbox_Any:
                                {
                                    var objRef = esp - 1;
                                    if (objRef->ObjectType == ObjectTypes.Object)
                                    {
                                        object obj = mStack[objRef->Value];
                                        Free(esp - 1);
                                        if (obj != null)
                                        {
                                            var t = domain.GetType(ip->TokenInteger);
                                            if (t != null)
                                            {
                                                var type = t.TypeForCLR;
                                                if (type.IsPrimitive)
                                                {
                                                    if (type == typeof(int))
                                                    {
                                                        int val = obj.ToInt32();
                                                        objRef->ObjectType = ObjectTypes.Integer;
                                                        objRef->Value = val;
                                                    }
                                                    else if (type == typeof(bool))
                                                    {
                                                        bool val = (bool)obj;
                                                        objRef->ObjectType = ObjectTypes.Integer;
                                                        objRef->Value = val ? 1 : 0;
                                                    }
                                                    else if (type == typeof(short))
                                                    {
                                                        short val = obj.ToInt16();
                                                        objRef->ObjectType = ObjectTypes.Integer;
                                                        objRef->Value = val;
                                                    }
                                                    else if (type == typeof(long))
                                                    {
                                                        long val = obj.ToInt64();
                                                        objRef->ObjectType = ObjectTypes.Long;
                                                        *(long*)&objRef->Value = val;
                                                    }
                                                    else if (type == typeof(float))
                                                    {
                                                        float val = obj.ToFloat();
                                                        objRef->ObjectType = ObjectTypes.Float;
                                                        *(float*)&objRef->Value = val;
                                                    }
                                                    else if (type == typeof(byte))
                                                    {
                                                        byte val = (byte)obj;
                                                        objRef->ObjectType = ObjectTypes.Integer;
                                                        objRef->Value = val;
                                                    }
                                                    else if (type == typeof(double))
                                                    {
                                                        double val = obj.ToDouble();
                                                        objRef->ObjectType = ObjectTypes.Double;
                                                        *(double*)&objRef->Value = val;
                                                    }
                                                    else if (type == typeof(uint))
                                                    {
                                                        uint val = (uint)obj;
                                                        objRef->ObjectType = ObjectTypes.Integer;
                                                        objRef->Value = (int)val;
                                                    }
                                                    else if (type == typeof(ushort))
                                                    {
                                                        ushort val = (ushort)obj;
                                                        objRef->ObjectType = ObjectTypes.Integer;
                                                        objRef->Value = val;
                                                    }
                                                    else if (type == typeof(ulong))
                                                    {
                                                        ulong val = (ulong)obj;
                                                        objRef->ObjectType = ObjectTypes.Long;
                                                        *(ulong*)&objRef->Value = val;
                                                    }
                                                    else if (type == typeof(sbyte))
                                                    {
                                                        sbyte val = (sbyte)obj;
                                                        objRef->ObjectType = ObjectTypes.Integer;
                                                        objRef->Value = val;
                                                    }
                                                    else
                                                        throw new NotImplementedException();
                                                }
                                                else if (t.IsValueType)
                                                {
                                                    throw new NotImplementedException();
                                                }
                                                else
                                                {
                                                    throw new NotImplementedException();
                                                }
                                            }
                                            else
                                                throw new TypeLoadException();
                                        }
                                        else
                                            throw new NullReferenceException();
                                    }
                                    else if (objRef->ObjectType == ObjectTypes.Null)
                                    {
                                        throw new NullReferenceException();
                                    }
                                    else
                                        throw new InvalidCastException();
                                }
                                break;
                            case OpCodeEnum.Initobj:
                                {
                                    var objRef = GetObjectAndResolveReference(esp - 1);
                                    var type = domain.GetType(ip->TokenInteger);
                                    if (type is ILType)
                                    {
                                        ILType it = (ILType)type;
                                        if (it.IsValueType)
                                        {
                                            if (objRef->ObjectType == ObjectTypes.Object)
                                            {
                                                var obj = mStack[objRef->Value];
                                                if (obj != null)
                                                {
                                                    if (obj is ILTypeInstance)
                                                    {
                                                        ILTypeInstance instance = obj as ILTypeInstance;
                                                        instance.Clear();
                                                    }
                                                    else
                                                        throw new NotSupportedException();
                                                }
                                                else
                                                    throw new NullReferenceException();
                                            }
                                            else
                                                throw new NullReferenceException();

                                            Free(esp - 1);
                                            esp--;
                                        }
                                        else
                                        {
                                            PushNull(esp);
                                            switch (objRef->ObjectType)
                                            {
                                                case ObjectTypes.StaticFieldReference:
                                                    {
                                                        var t = AppDomain.GetType(objRef->Value) as ILType;
                                                        t.StaticInstance.AssignFromStack(objRef->ValueLow, esp, AppDomain, mStack);
                                                    }
                                                    break;
                                                case ObjectTypes.FieldReference:
                                                    {
                                                        var instance = mStack[objRef->Value] as ILTypeInstance;
                                                        instance.AssignFromStack(objRef->ValueLow, esp, AppDomain, mStack);
                                                        Free(esp - 1);
                                                        esp--;
                                                    }
                                                    break;
                                                default:
                                                    PushNull(objRef);
                                                    break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //nothing to do for clr value Types
                                    }
                                }
                                break;
                            case OpCodeEnum.Isinst:
                                {
                                    var objRef = esp - 1;
                                    var type = domain.GetType(ip->TokenInteger);
                                    if (type != null)
                                    {
                                        var obj = mStack[objRef->Value];
                                        Free(objRef);
                                        if (obj != null)
                                        {
                                            if (obj is ILTypeInstance)
                                            {
                                                if (((ILTypeInstance)obj).CanAssignTo(type))
                                                {
                                                    esp = PushObject(objRef, mStack, obj);
                                                }
                                                else
                                                {
#if !DEBUG
                                                    objRef->ObjectType = ObjectTypes.Null;
                                                    objRef->Value = -1;
                                                    objRef->ValueLow = 0;
#endif
                                                }
                                            }
                                            else
                                            {
                                                if (type.TypeForCLR.IsAssignableFrom(obj.GetType()))
                                                {
                                                    esp = PushObject(objRef, mStack, obj);
                                                }
                                                else
                                                {
#if !DEBUG
                                                    objRef->ObjectType = ObjectTypes.Null;
                                                    objRef->Value = -1;
                                                    objRef->ValueLow = 0;
#endif
                                                }
                                            }
                                        }
                                        else
                                        {
#if !DEBUG
                                            objRef->ObjectType = ObjectTypes.Null;
                                            objRef->Value = -1;
                                            objRef->ValueLow = 0;
#endif
                                        }
                                    }
                                    else
                                        throw new NullReferenceException();
                                }
                                break;
                            #endregion

                            #region Array
                            case OpCodeEnum.Newarr:
                                {
                                    var cnt = (esp - 1);
                                    var type = domain.GetType(ip->TokenInteger);
                                    object arr = null;
                                    if (type != null)
                                    {
                                        if (type.TypeForCLR != typeof(ILTypeInstance))
                                        {
                                            arr = Array.CreateInstance(type.TypeForCLR, cnt->Value);
                                        }
                                        else
                                        {
                                            arr = new ILTypeInstance[cnt->Value];
                                        }
                                    }
                                    cnt->ObjectType = ObjectTypes.Object;
                                    cnt->Value = mStack.Count;
                                    mStack.Add(arr);
                                }
                                break;
                            case OpCodeEnum.Stelem_Ref:
                            case OpCodeEnum.Stelem_Any:
                                {
                                    var val = esp - 1;
                                    var idx = esp - 2;
                                    var arrRef = esp - 3;
                                    Array arr = mStack[arrRef->Value] as Array;
                                    arr.SetValue(mStack[val->Value], idx->Value);
                                    Free(esp - 1);
                                    Free(esp - 2);
                                    Free(esp - 3);
                                    esp -= 3;
                                }
                                break;

                            case OpCodeEnum.Ldelem_Ref:
                            case OpCodeEnum.Ldelem_Any:
                                {
                                    var idx = esp - 1;
                                    var arrRef = esp - 2;
                                    Array arr = mStack[arrRef->Value] as Array;
                                    object val = arr.GetValue(idx->Value);
                                    Free(esp - 1);
                                    Free(esp - 2);

                                    esp = PushObject(esp - 2, mStack, val);
                                }
                                break;
                            case OpCodeEnum.Stelem_I1:
                                {
                                    var val = esp - 1;
                                    var idx = esp - 2;
                                    var arrRef = esp - 3;
                                    byte[] arr = mStack[arrRef->Value] as byte[];
                                    if (arr != null)
                                    {
                                        arr[idx->Value] = (byte)val->Value;
                                    }
                                    else
                                    {
                                        bool[] arr2 = mStack[arrRef->Value] as bool[];
                                        if (arr2 != null)
                                        {
                                            arr2[idx->Value] = val->Value == 1;
                                        }
                                        else
                                        {
                                            sbyte[] arr3 = mStack[arrRef->Value] as sbyte[];
                                            arr3[idx->Value] = (sbyte)val->Value;
                                        }
                                    }
                                    Free(esp - 1);
                                    Free(esp - 2);
                                    Free(esp - 3);
                                    esp -= 3;
                                }
                                break;
                            case OpCodeEnum.Ldelem_I1:
                                {
                                    var idx = esp - 1;
                                    var arrRef = esp - 2;
                                    bool[] arr = mStack[arrRef->Value] as bool[];
                                    int val;
                                    if (arr != null)
                                        val = arr[idx->Value] ? 1 : 0;
                                    else
                                    {
                                        sbyte[] arr2 = mStack[arrRef->Value] as sbyte[];
                                        val = arr2[idx->Value];
                                    }

                                    Free(esp - 1);
                                    Free(esp - 2);

                                    arrRef->ObjectType = ObjectTypes.Integer;
                                    arrRef->Value = val;
                                    esp -= 1;
                                }
                                break;
                            case OpCodeEnum.Ldelem_U1:
                                {
                                    var idx = (esp - 1);
                                    var arrRef = esp - 2;
                                    byte[] arr = mStack[arrRef->Value] as byte[];
                                    int val;
                                    if (arr != null)
                                        val = arr[idx->Value];
                                    else
                                    {
                                        bool[] arr2 = mStack[arrRef->Value] as bool[];
                                        val = arr2[idx->Value] ? 1 : 0;
                                    }
                                    Free(esp - 1);
                                    Free(esp - 2);

                                    arrRef->ObjectType = ObjectTypes.Integer;
                                    arrRef->Value = val;
                                    esp -= 1;
                                }
                                break;
                            case OpCodeEnum.Stelem_I2:
                                {
                                    var val = esp - 1;
                                    var idx = esp - 2;
                                    var arrRef = esp - 3;
                                    short[] arr = mStack[arrRef->Value] as short[];
                                    if (arr != null)
                                    {
                                        arr[idx->Value] = (short)val->Value;
                                    }
                                    else
                                    {
                                        ushort[] arr2 = mStack[arrRef->Value] as ushort[];
                                        if (arr2 != null)
                                        {
                                            arr2[idx->Value] = (ushort)val->Value;
                                        }
                                        else
                                        {
                                            char[] arr3 = mStack[arrRef->Value] as char[];
                                            arr3[idx->Value] = (char)val->Value;
                                        }
                                    }
                                    Free(esp - 1);
                                    Free(esp - 2);
                                    Free(esp - 3);
                                    esp -= 3;
                                }
                                break;
                            case OpCodeEnum.Ldelem_I2:
                                {
                                    var idx = (esp - 1)->Value;
                                    var arrRef = esp - 2;
                                    short[] arr = mStack[arrRef->Value] as short[];
                                    int val = 0;
                                    if (arr != null)
                                    {
                                        val = arr[idx];
                                    }
                                    else
                                    {
                                        char[] arr2 = mStack[arrRef->Value] as char[];
                                        val = arr2[idx];
                                    }
                                    Free(esp - 1);
                                    Free(esp - 2);

                                    arrRef->ObjectType = ObjectTypes.Integer;
                                    arrRef->Value = val;
                                    esp -= 1;
                                }
                                break;
                            case OpCodeEnum.Ldelem_U2:
                                {
                                    var idx = (esp - 1)->Value;
                                    var arrRef = esp - 2;
                                    ushort[] arr = mStack[arrRef->Value] as ushort[];
                                    int val = 0;
                                    if (arr != null)
                                    {
                                        val = arr[idx];
                                    }
                                    else
                                    {
                                        char[] arr2 = mStack[arrRef->Value] as char[];
                                        val = arr2[idx];
                                    }
                                    Free(esp - 1);
                                    Free(esp - 2);

                                    arrRef->ObjectType = ObjectTypes.Integer;
                                    arrRef->Value = val;
                                    esp -= 1;
                                }
                                break;
                            case OpCodeEnum.Stelem_I4:
                                {
                                    var val = esp - 1;
                                    var idx = esp - 2;
                                    var arrRef = esp - 3;
                                    int[] arr = mStack[arrRef->Value] as int[];
                                    if (arr != null)
                                    {
                                        arr[idx->Value] = val->Value;
                                    }
                                    else
                                    {
                                        uint[] arr2 = mStack[arrRef->Value] as uint[];
                                        arr2[idx->Value] = (uint)val->Value;
                                    }
                                    Free(esp - 1);
                                    Free(esp - 2);
                                    Free(esp - 3);
                                    esp -= 3;
                                }
                                break;
                            case OpCodeEnum.Ldelem_I4:
                                {
                                    var idx = (esp - 1)->Value;
                                    var arrRef = esp - 2;
                                    int[] arr = mStack[arrRef->Value] as int[];

                                    Free(esp - 1);
                                    Free(esp - 2);

                                    arrRef->ObjectType = ObjectTypes.Integer;
                                    arrRef->Value = arr[idx];
                                    esp -= 1;
                                }
                                break;
                            case OpCodeEnum.Ldelem_U4:
                                {
                                    var idx = (esp - 1)->Value;
                                    var arrRef = esp - 2;
                                    uint[] arr = mStack[arrRef->Value] as uint[];

                                    Free(esp - 1);
                                    Free(esp - 2);

                                    arrRef->ObjectType = ObjectTypes.Integer;
                                    arrRef->Value = (int)arr[idx];
                                    esp -= 1;
                                }
                                break;
                            case OpCodeEnum.Stelem_I8:
                                {
                                    var val = esp - 1;
                                    var idx = esp - 2;
                                    var arrRef = esp - 3;
                                    long[] arr = mStack[arrRef->Value] as long[];
                                    if (arr != null)
                                    {
                                        arr[idx->Value] = *(long*)&val->Value;
                                    }
                                    else
                                    {
                                        ulong[] arr2 = mStack[arrRef->Value] as ulong[];
                                        arr2[idx->Value] = *(ulong*)&val->Value;
                                    }
                                    Free(esp - 1);
                                    Free(esp - 2);
                                    Free(esp - 3);
                                    esp -= 3;
                                }
                                break;
                            case OpCodeEnum.Ldelem_I8:
                                {
                                    var idx = esp - 1;
                                    var arrRef = esp - 2;
                                    long[] arr = mStack[arrRef->Value] as long[];
                                    long val;
                                    if (arr != null)
                                        val = arr[idx->Value];
                                    else
                                    {
                                        ulong[] arr2 = mStack[arrRef->Value] as ulong[];
                                        val = (long)arr2[idx->Value];
                                    }
                                    Free(esp - 1);
                                    Free(esp - 2);

                                    arrRef->ObjectType = ObjectTypes.Long;
                                    *(long*)&arrRef->Value = val;
                                    esp -= 1;
                                }
                                break;
                            case OpCodeEnum.Stelem_R4:
                                {
                                    var val = esp - 1;
                                    var idx = esp - 2;
                                    var arrRef = esp - 3;
                                    float[] arr = mStack[arrRef->Value] as float[];
                                    arr[idx->Value] = *(float*)&val->Value;
                                    Free(esp - 1);
                                    Free(esp - 2);
                                    Free(esp - 3);
                                    esp -= 3;
                                }
                                break;
                            case OpCodeEnum.Ldelem_R4:
                                {
                                    var idx = (esp - 1)->Value;
                                    var arrRef = esp - 2;
                                    float[] arr = mStack[arrRef->Value] as float[];

                                    Free(esp - 1);
                                    Free(esp - 2);

                                    arrRef->ObjectType = ObjectTypes.Float;
                                    *(float*)&arrRef->Value = arr[idx];
                                    esp -= 1;
                                }
                                break;
                            case OpCodeEnum.Stelem_R8:
                                {
                                    var val = esp - 1;
                                    var idx = esp - 2;
                                    var arrRef = esp - 3;
                                    double[] arr = mStack[arrRef->Value] as double[];
                                    arr[idx->Value] = *(double*)&val->Value;
                                    Free(esp - 1);
                                    Free(esp - 2);
                                    Free(esp - 3);
                                    esp -= 3;
                                }
                                break;
                            case OpCodeEnum.Ldelem_R8:
                                {
                                    var idx = (esp - 1)->Value;
                                    var arrRef = esp - 2;
                                    double[] arr = mStack[arrRef->Value] as double[];

                                    Free(esp - 1);
                                    Free(esp - 2);

                                    arrRef->ObjectType = ObjectTypes.Double;
                                    *(double*)&arrRef->Value = arr[idx];
                                    esp -= 1;
                                }
                                break;
                            case OpCodeEnum.Ldlen:
                                {
                                    var arrRef = esp - 1;
                                    Array arr = mStack[arrRef->Value] as Array;
                                    Free(esp - 1);

                                    arrRef->ObjectType = ObjectTypes.Integer;
                                    arrRef->Value = arr.Length;
                                }
                                break;
                            case OpCodeEnum.Ldelema:
                                {
                                    var arrRef = esp - 2;
                                    var idx = (esp - 1)->Value;
                                    
                                    Array arr = mStack[arrRef->Value] as Array;
                                    Free(esp - 1);
                                    Free(esp - 2);

                                    arrRef->ObjectType = ObjectTypes.ArrayReference;
                                    arrRef->Value = mStack.Count;
                                    mStack.Add(arr);
                                    arrRef->ValueLow = idx;
                                    esp--;
                                }
                                break;
                            #endregion

                            #region Conversion
                            case OpCodeEnum.Conv_U1:
                            case OpCodeEnum.Conv_Ovf_U1:
                            case OpCodeEnum.Conv_Ovf_U1_Un:
                                {
                                    var obj = esp - 1;
                                    int val;
                                    switch (obj->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                        case ObjectTypes.Integer:
                                            val = (byte)obj->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    obj->ObjectType = ObjectTypes.Integer;
                                    obj->Value = val;
                                    obj->ValueLow = 0;
                                }
                                break;
                            case OpCodeEnum.Conv_I1:
                            case OpCodeEnum.Conv_Ovf_I1:
                            case OpCodeEnum.Conv_Ovf_I1_Un:
                                {
                                    var obj = esp - 1;
                                    int val;
                                    switch (obj->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                        case ObjectTypes.Integer:
                                            val = (sbyte)obj->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    obj->ObjectType = ObjectTypes.Integer;
                                    obj->Value = val;
                                    obj->ValueLow = 0;
                                }
                                break;
                            case OpCodeEnum.Conv_U2:
                            case OpCodeEnum.Conv_Ovf_U2:
                            case OpCodeEnum.Conv_Ovf_U2_Un:
                                {
                                    var obj = esp - 1;
                                    int val;
                                    switch (obj->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                        case ObjectTypes.Integer:
                                            val = (ushort)obj->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    obj->ObjectType = ObjectTypes.Integer;
                                    obj->Value = val;
                                    obj->ValueLow = 0;
                                }
                                break;
                            case OpCodeEnum.Conv_I2:
                            case OpCodeEnum.Conv_Ovf_I2:
                            case OpCodeEnum.Conv_Ovf_I2_Un:
                                {
                                    var obj = esp - 1;
                                    int val;
                                    switch (obj->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                        case ObjectTypes.Integer:
                                            val = (short)(obj->Value);
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    obj->ObjectType = ObjectTypes.Integer;
                                    obj->Value = val;
                                    obj->ValueLow = 0;
                                }
                                break;
                            case OpCodeEnum.Conv_U4:
                            case OpCodeEnum.Conv_Ovf_U4:
                            case OpCodeEnum.Conv_Ovf_U4_Un:
                                {
                                    var obj = esp - 1;
                                    uint val;
                                    switch (obj->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            val = (uint)*(long*)&obj->Value;
                                            break;
                                        case ObjectTypes.Integer:
                                            val = (uint)obj->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    obj->ObjectType = ObjectTypes.Integer;
                                    obj->Value = (int)val;
                                    obj->ValueLow = 0;
                                }
                                break;
                            case OpCodeEnum.Conv_I4:
                            case OpCodeEnum.Conv_Ovf_I:
                            case OpCodeEnum.Conv_Ovf_I_Un:
                            case OpCodeEnum.Conv_Ovf_I4:
                            case OpCodeEnum.Conv_Ovf_I4_Un:
                                {
                                    var obj = esp - 1;
                                    int val;
                                    switch (obj->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            val = (int)*(long*)&obj->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            val = (int)*(float*)&obj->Value;
                                            break;
                                        case ObjectTypes.Double:
                                            val = (int)*(double*)&obj->Value;
                                            break;
                                        case ObjectTypes.Integer:
                                            val = obj->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    obj->ObjectType = ObjectTypes.Integer;
                                    obj->Value = val;
                                }
                                break;
                            case OpCodeEnum.Conv_U8:
                            case OpCodeEnum.Conv_I8:
                            case OpCodeEnum.Conv_Ovf_I8:
                            case OpCodeEnum.Conv_Ovf_I8_Un:
                            case OpCodeEnum.Conv_Ovf_U8:
                            case OpCodeEnum.Conv_Ovf_U8_Un:
                                {
                                    var obj = esp - 1;
                                    long val;
                                    switch (obj->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            val = obj->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    obj->ObjectType = ObjectTypes.Long;
                                    *(long*)(&obj->Value) = val;
                                }
                                break;
                            case OpCodeEnum.Conv_R4:
                                {
                                    var obj = esp - 1;
                                    float val;
                                    switch (obj->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            val = (float)*(long*)&obj->Value;
                                            break;
                                        case ObjectTypes.Double:
                                            val = *(float*)&obj->Value;
                                            break;
                                        case ObjectTypes.Integer:
                                            val = obj->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    obj->ObjectType = ObjectTypes.Float;
                                    *(float*)&obj->Value = val;
                                }
                                break;
                            case OpCodeEnum.Conv_R8:
                                {
                                    var obj = esp - 1;
                                    double val;
                                    switch (obj->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            val = (double)*(long*)&obj->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            val = *(double*)&obj->Value;
                                            break;
                                        case ObjectTypes.Integer:
                                            val = obj->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    obj->ObjectType = ObjectTypes.Double;
                                    *(double*)&obj->Value = val;
                                }
                                break;
                                
                            #endregion

                            #region Stack operation
                            case OpCodeEnum.Pop:
                                {
                                    Free(esp - 1);
                                    esp--;
                                }
                                break;
                            case OpCodeEnum.Dup:
                                {
                                    var obj = esp - 1;
                                    *esp = *obj;
                                    if (esp->ObjectType >= ObjectTypes.Object)
                                    {
                                        esp->Value = mStack.Count;
                                        mStack.Add(mStack[obj->Value]);
                                    }
                                    esp++;
                                }
                                break;
                            #endregion

                            case OpCodeEnum.Throw:
                                {
                                    var obj = GetObjectAndResolveReference(esp - 1);
                                    var ex = mStack[obj->Value] as Exception;
                                    Free(obj);
                                    esp--;
                                    throw ex;
                                }
                            case OpCodeEnum.Nop:
                            case OpCodeEnum.Endfinally:
                            case OpCodeEnum.Volatile:
                            case OpCodeEnum.Castclass:
                                break;
                            default:
                                throw new NotSupportedException("Not supported opcode " + code);
                        }
                        ip++;
                    }
                    catch (Exception ex)
                    {
                        if (unhandledException)
                            throw ex;
                        if (method.ExceptionHandler != null)
                        {
                            int addr =(int)(ip - ptr);
                            var sql = from e in method.ExceptionHandler
                                      where addr >= e.TryStart && addr <= e.TryEnd && e.HandlerType == ExceptionHandlerType.Catch && CheckExceptionType(e.CatchType, ex, true)
                                      select e;
                            var eh = sql.FirstOrDefault();
                            
                            if (eh == null)
                            {
                                var sql2 = from e in method.ExceptionHandler
                                          where addr >= e.TryStart && addr <= e.TryEnd && e.HandlerType == ExceptionHandlerType.Catch && CheckExceptionType(e.CatchType, ex, false)
                                          select e;
                                eh = sql2.FirstOrDefault();
                            }
                            if (eh != null)
                            {
                                if (method.HasThis)
                                    ex.Data["ThisInfo"] = Debugger.DebugService.Instance.GetThisInfo(this);
                                else
                                    ex.Data["ThisInfo"] = "";
                                ex.Data["StackTrace"] = Debugger.DebugService.Instance.GetStackTrance(this);
                                ex.Data["LocalInfo"] = Debugger.DebugService.Instance.GetLocalVariableInfo(this);
                                esp = PushObject(esp, mStack, ex);
                                ip = ptr + eh.HandlerStart;
                                continue;
                            }
                        }
                        unhandledException = true;
                        returned = true;
#if DEBUG
                        if (!Debugger.DebugService.Instance.Break(this, ex))
#endif
                        {
                            throw new ILRuntimeException(ex.Message, this, method, ex);
                        }
                    }
                }
            }

            //ClearStack
            return stack.PopFrame(ref frame, esp, mStack, mStackBase);
        }

        void LoadFromArrayReference(object obj,int idx, StackObject* objRef, IType t, List<object> mStack)
        {
            var nT = t.TypeForCLR;
            if (nT.IsPrimitive)
            {
                if (nT == typeof(int))
                {
                    int[] arr = obj as int[];
                    objRef->ObjectType = ObjectTypes.Integer;
                    objRef->Value = arr[idx];
                    objRef->ValueLow = 0;
                }
                else if (nT == typeof(short))
                {
                    short[] arr = obj as short[];
                    objRef->ObjectType = ObjectTypes.Integer;
                    objRef->Value = arr[idx];
                    objRef->ValueLow = 0;
                }
                else if (nT == typeof(long))
                {
                    long[] arr = obj as long[];
                    objRef->ObjectType = ObjectTypes.Long;
                    *(long*)&objRef->Value = arr[idx];
                    objRef->ValueLow = 0;
                }
                else if (nT == typeof(float))
                {
                    float[] arr = obj as float[];
                    objRef->ObjectType = ObjectTypes.Float;
                    *(float*)&objRef->Value = arr[idx];
                    objRef->ValueLow = 0;
                }
                else if (nT == typeof(double))
                {
                    double[] arr = obj as double[];
                    objRef->ObjectType = ObjectTypes.Double;
                    *(double*)&objRef->Value = arr[idx];
                    objRef->ValueLow = 0;
                }
                else if (nT == typeof(byte))
                {
                    byte[] arr = obj as byte[];
                    objRef->ObjectType = ObjectTypes.Integer;
                    objRef->Value = arr[idx];
                    objRef->ValueLow = 0;
                }
                else if (nT == typeof(char))
                {
                    char[] arr = obj as char[];
                    objRef->ObjectType = ObjectTypes.Integer;
                    objRef->Value = arr[idx];
                    objRef->ValueLow = 0;
                }
                else if (nT == typeof(uint))
                {
                    uint[] arr = obj as uint[];
                    objRef->ObjectType = ObjectTypes.Integer;
                    *(uint*)&objRef->Value = arr[idx];
                    objRef->ValueLow = 0;
                }
                else if (nT == typeof(sbyte))
                {
                    sbyte[] arr = obj as sbyte[];
                    objRef->ObjectType = ObjectTypes.Integer;
                    objRef->Value = arr[idx];
                    objRef->ValueLow = 0;
                }
                else
                    throw new NotImplementedException();
            }
            else
            {
                Array arr = obj as Array;
                objRef->ObjectType = ObjectTypes.Object;
                objRef->Value = mStack.Count;
                mStack.Add(arr.GetValue(idx));
                objRef->ValueLow = 0;
            }
        }

        void StoreValueToArrayReference(StackObject* objRef, StackObject* val, IType t, List<object> mStack)
        {
            var nT = t.TypeForCLR;
            if (nT.IsPrimitive)
            {
                if (nT == typeof(int))
                {
                    int[] arr = mStack[objRef->Value] as int[];
                    arr[objRef->ValueLow] = val->Value;
                }
                else if (nT == typeof(short))
                {
                    short[] arr = mStack[objRef->Value] as short[];
                    arr[objRef->ValueLow] = (short)val->Value;
                }
                else if (nT == typeof(long))
                {
                    long[] arr = mStack[objRef->Value] as long[];
                    arr[objRef->ValueLow] = *(long*)&val->Value;
                }
                else if (nT == typeof(float))
                {
                    float[] arr = mStack[objRef->Value] as float[];
                    arr[objRef->ValueLow] = *(float*)&val->Value;
                }
                else if (nT == typeof(double))
                {
                    double[] arr = mStack[objRef->Value] as double[];
                    arr[objRef->ValueLow] = *(double*)&val->Value;
                }
                else if (nT == typeof(byte))
                {
                    byte[] arr = mStack[objRef->Value] as byte[];
                    arr[objRef->ValueLow] = (byte)val->Value;
                }
                else if (nT == typeof(char))
                {
                    char[] arr = mStack[objRef->Value] as char[];
                    arr[objRef->ValueLow] = (char)val->Value;
                }
                else if (nT == typeof(uint))
                {
                    uint[] arr = mStack[objRef->Value] as uint[];
                    arr[objRef->ValueLow] = (uint)val->Value;
                }
                else if (nT == typeof(sbyte))
                {
                    sbyte[] arr = mStack[objRef->Value] as sbyte[];
                    arr[objRef->ValueLow] = (sbyte)val->Value;
                }
                else
                    throw new NotImplementedException();
            }
            else
            {
                Array arr = mStack[objRef->Value] as Array;
                arr.SetValue(mStack[val->Value], objRef->ValueLow);
            }
        }

        bool CheckExceptionType(IType catchType, object exception, bool explicitMatch)
        {
            if (catchType is CLRType)
            {
                if (explicitMatch)
                    return exception.GetType() == catchType.TypeForCLR;
                else
                    return catchType.TypeForCLR.IsAssignableFrom(exception.GetType());
            }
            else
                throw new NotImplementedException();
        }

        StackObject* GetObjectAndResolveReference(StackObject* esp)
        {
            if (esp->ObjectType == ObjectTypes.StackObjectReference)
            {
                return *(StackObject**)&esp->Value;
            }
            else
                return esp;
        }

        StackObject* PushParameters(IMethod method, StackObject* esp, object[] p)
        {
            List<object> mStack = stack.ManagedStack;
            if (p != null && p.Length > 0)
            {
                var plist = method.Parameters;
                int pCnt = plist != null ? plist.Count : 0;
                if (method.HasThis)
                    pCnt++;
                if (pCnt != p.Length)
                    throw new ArgumentOutOfRangeException();
                for (int i = 0; i < p.Length; i++)
                {
                    Type clrType = p[i].GetType();
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

        public void CopyToStack(StackObject* dst, StackObject* src, List<object> mStack)
        {
            *dst = *src;
            if (dst->ObjectType >= ObjectTypes.Object)
            {
                dst->Value = mStack.Count;
                var obj = mStack[src->Value];
                if (obj is ILTypeInstance)
                {
                    ILTypeInstance ins = obj as ILTypeInstance;
                    if (ins.IsValueType)
                    {
                        mStack.Add(ins.Clone());
                        return;
                    }
                        
                }
                mStack.Add(mStack[src->Value]);
            }
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

        static StackObject* PushNull(StackObject* esp)
        {
            esp->ObjectType = ObjectTypes.Null;
            esp->Value = -1;
            esp->ValueLow = 0;
            return esp + 1;
        }

        public static StackObject* PushObject(StackObject* esp, List<object> mStack, object obj, bool isBox = false)
        {
            if (obj != null)
            {
                if (!isBox)
                {
                    if (obj.GetType().IsPrimitive)
                    {
                        if (obj is int)
                        {
                            esp->ObjectType = ObjectTypes.Integer;
                            esp->Value = (int)obj;
                        }
                        else if (obj is bool)
                        {
                            esp->ObjectType = ObjectTypes.Integer;
                            esp->Value = (bool)(obj) ? 1 : 0;
                        }
                        else if (obj is short)
                        {
                            esp->ObjectType = ObjectTypes.Integer;
                            esp->Value = (short)obj;
                        }
                        else if (obj is long)
                        {
                            esp->ObjectType = ObjectTypes.Long;
                            *(long*)(&esp->Value) = (long)obj;
                        }
                        else if (obj is float)
                        {
                            esp->ObjectType = ObjectTypes.Float;
                            *(float*)(&esp->Value) = (float)obj;
                        }
                        else if (obj is byte)
                        {
                            esp->ObjectType = ObjectTypes.Integer;
                            esp->Value = (byte)obj;
                        }
                        else if (obj is uint)
                        {
                            esp->ObjectType = ObjectTypes.Integer;
                            esp->Value = (int)(uint)obj;
                        }
                        else if(obj is char)
                        {
                            esp->ObjectType = ObjectTypes.Integer;
                            esp->Value = (int)(char)obj;
                        }
                        else if (obj is double)
                        {
                            esp->ObjectType = ObjectTypes.Double;
                            *(double*)(&esp->Value) = (double)obj;
                        }
                        else if (obj is ulong)
                        {
                            esp->ObjectType = ObjectTypes.Long;
                            *(ulong*)(&esp->Value) = (ulong)obj;
                        }
                        else if (obj is sbyte)
                        {
                            esp->ObjectType = ObjectTypes.Integer;
                            esp->Value = (sbyte)obj;
                        }
                        else
                            throw new NotImplementedException();
                    }
                    else
                    {
                        esp->ObjectType = ObjectTypes.Object;
                        esp->Value = mStack.Count;
                        mStack.Add(obj);
                    }
                }
                else
                {
                    esp->ObjectType = ObjectTypes.Object;
                    esp->Value = mStack.Count;
                    mStack.Add(obj);
                }
            }
            else
            {
                return PushNull(esp);
            }
            return esp + 1;
        }

        public void Free(StackObject* esp)
        {
            if (esp->ObjectType >= ObjectTypes.Object)
            {
                if (esp->Value == stack.ManagedStack.Count - 1)
                    stack.ManagedStack.RemoveAt(esp->Value);
            }
#if DEBUG
            esp->ObjectType = ObjectTypes.Null;
            esp->Value = -1;
            esp->ValueLow = 0;
#endif
        }
    }
}
