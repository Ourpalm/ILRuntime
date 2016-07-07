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
        public object Run(ILMethod method, object[] p)
        {
            List<object> mStack = stack.ManagedStack;
            int mStackBase = mStack.Count;

            StackObject* esp = PushParameters(method, stack.StackBase, p);
            bool unhandledException;
            Execute(method, esp, out unhandledException);
            object result = method.ReturnType != domain.VoidType ? esp->ToObject(mStack) : null;
            //ClearStack
            mStack.RemoveRange(mStackBase, mStack.Count - mStackBase);
            return result;
        }
        StackObject* Execute(ILMethod method, StackObject* esp, out bool unhandledException)
        {
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
            if (method.HasThis)//this parameter is always object reference
            {
                arg--;
                mStackBase--;
            }
            unhandledException = false;

            try
            {
                //Managed Stack reserved for local variable
                for (int i = 0; i < method.LocalVariableCount; i++)
                {
                    var v = method.Definition.Body.Variables[i];
                    if (v.VariableType.IsValueType && !v.VariableType.IsPrimitive)
                    {
                        var t = AppDomain.GetType(v.VariableType.FullName);
                        if (t is ILType)
                        {
                            var obj = ((ILType)t).Instantiate();
                            var loc = v1 + i;
                            loc->ObjectType = ObjectTypes.Object;
                            loc->Value = mStack.Count;
                            mStack.Add(obj);
                        }
                        else
                            throw new NotImplementedException();
                    }
                    else
                        mStack.Add(null);
                }
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
                            case OpCodeEnum.Stloc_0:
                                esp--;
                                *v1 = *esp;
                                if (v1->ObjectType >= ObjectTypes.Object)
                                {
                                    int idx = mStackBase;
                                    mStack[idx] = mStack[v1->Value];
                                    v1->Value = idx;
                                    Free(esp);
                                }
                                break;
                            case OpCodeEnum.Ldloc_0:
                                CopyToStack(esp, v1, mStack);
                                esp++;
                                break;
                            case OpCodeEnum.Stloc_1:
                                esp--;
                                *v2 = *esp;
                                if (v2->ObjectType >= ObjectTypes.Object)
                                {
                                    int idx = mStackBase + 1;
                                    mStack[idx] = mStack[v2->Value];
                                    v2->Value = idx;
                                    Free(esp);
                                }
                                break;
                            case OpCodeEnum.Ldloc_1:
                                CopyToStack(esp, v2, mStack);
                                esp++;
                                break;
                            case OpCodeEnum.Stloc_2:
                                esp--;
                                *v3 = *esp;
                                if (v3->ObjectType >= ObjectTypes.Object)
                                {
                                    int idx = mStackBase + 2;
                                    mStack[idx] = mStack[v3->Value];
                                    v3->Value = idx;
                                    Free(esp);
                                }
                                break;
                            case OpCodeEnum.Ldloc_2:
                                CopyToStack(esp, v3, mStack);
                                esp++;
                                break;
                            case OpCodeEnum.Stloc_3:
                                esp--;
                                *v4 = *esp;
                                if (v4->ObjectType >= ObjectTypes.Object)
                                {
                                    int idx = mStackBase + 3;
                                    mStack[idx] = mStack[v4->Value];
                                    v4->Value = idx;
                                    Free(esp);
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
                                    if (v->ObjectType >= ObjectTypes.Object)
                                    {
                                        int idx = mStackBase + ip->TokenInteger;
                                        mStack[idx] = mStack[v->Value];
                                        v->Value = idx;
                                        Free(esp);
                                    }
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
                            case OpCodeEnum.Ldstr:
                                esp = PushObject(esp, mStack, AppDomain.GetString(ip->TokenInteger));
                                break;
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
                            case OpCodeEnum.Br_S:
                            case OpCodeEnum.Br:
                                ip = ptr + ip->TokenInteger;
                                continue;
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
                                            if (code == OpCodeEnum.Callvirt)
                                            {
                                                var objRef = esp - ilm.ParameterCount - 1;
                                                var obj = mStack[objRef->Value];
                                                ilm = ((ILTypeInstance)obj).Type.GetVirtualMethod(ilm) as ILMethod;
                                            }
                                            esp = Execute(ilm, esp, out unhandledException);
                                            if (unhandledException)
                                                returned = true;
                                        }
                                        else
                                        {
                                            CLRMethod cm = (CLRMethod)m;
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
                                            if (cm.ReturnType != AppDomain.VoidType)
                                                esp = PushObject(esp, mStack, result);
                                        }

                                    }
                                }
                                break;
                            case OpCodeEnum.Newobj:
                                {
                                    IMethod m = domain.GetMethod(ip->TokenInteger);
                                    if (m is ILMethod)
                                    {
                                        ILType type = m.DeclearingType as ILType;
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
                                        for (int i = m.ParameterCount - 1; i >=0; i--)
                                        {
                                            Free(a + i);
                                        }
                                        Free(objRef - 1);
                                        esp = a;
                                        esp = PushObject(esp, mStack, obj);//new constructedObj
                                        if (unhandledException)
                                            returned = true;
                                    }
                                    else
                                        throw new NotSupportedException();
                                }
                                break;
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
                                            instance.AssignFromStack(ip->TokenInteger, val, mStack);
                                        }
                                        else
                                            throw new NotImplementedException();
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
                                            instance.PushToStack(ip->TokenInteger, esp - 1, mStack);
                                        }
                                        else
                                            throw new NotImplementedException();
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
                                            throw new NotImplementedException();
                                    }
                                    else
                                        throw new NullReferenceException();
                                }
                                break;
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
                            case OpCodeEnum.Box:
                                {
                                    var obj = esp - 1;
                                    var type = domain.GetType(ip->TokenInteger);
                                    if (type != null)
                                    {
                                        if (type == domain.IntType)
                                        {
                                            if (obj->ObjectType == ObjectTypes.Integer)
                                            {
                                                esp = PushObject(obj, mStack, obj->Value, true);
                                            }
                                            else
                                                throw new NotImplementedException();
                                        }
                                        else if (type == domain.BoolType)
                                        {
                                            if (obj->ObjectType == ObjectTypes.Integer)
                                            {
                                                esp = PushObject(obj, mStack, (obj->Value == 1), true);
                                            }
                                            else
                                                throw new NotImplementedException();
                                        }
                                        else
                                            throw new NotImplementedException();
                                    }
                                    else
                                        throw new NullReferenceException();                                    
                                }
                                break;
                            case OpCodeEnum.Initobj:
                                {
                                    var objRef = GetObjectAndResolveReference(esp - 1);
                                    var type = domain.GetType(ip->TokenInteger);
                                    if (type is ILType)
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
                                        throw new NotImplementedException();
                                    Free(esp - 1);
                                    esp--;
                                }
                                break;
                            case OpCodeEnum.Conv_I4:
                                {
                                    var obj = esp - 1;
                                    int val;
                                    switch (obj->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            val = obj->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            val = (int)*(float*)&obj->Value;
                                            break;
                                        case ObjectTypes.Double:
                                            val = (int)*(double*)&obj->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    obj->ObjectType = ObjectTypes.Integer;
                                    obj->Value = val;
                                }
                                break;
                            case OpCodeEnum.Conv_I8:
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
                            case OpCodeEnum.Pop:
                                {
                                    Free(esp - 1);
                                    esp--;
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
                if (!Debugger.DebugService.Instance.Break(this, ex))
#endif
                {
                    throw new ILRuntimeException(ex.Message, this, method, ex);
                }
            }
            //ClearStack
            return stack.PopFrame(ref frame, esp, mStack, mStackBase);

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

        void CopyToStack(StackObject* dst, StackObject* src, List<object> mStack)
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

        StackObject* PushObject(StackObject* esp, List<object> mStack, object obj, bool isBox = false)
        {
            if (!isBox)
            {
                if (obj is int)
                {
                    esp->ObjectType = ObjectTypes.Integer;
                    esp->Value = (int)obj;
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
                else if (obj is double)
                {
                    esp->ObjectType = ObjectTypes.Double;
                    *(double*)(&esp->Value) = (double)obj;
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
            return esp + 1;
        }

        void Free(StackObject* esp)
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
