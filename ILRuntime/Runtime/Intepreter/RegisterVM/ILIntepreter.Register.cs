using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mono.Cecil;

using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Stack;
using ILRuntime.Runtime.Intepreter.OpCodes;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.CLR.Utils;

namespace ILRuntime.Runtime.Intepreter
{
    unsafe struct RegisterFrameInfo
    {
        public ILIntepreter Intepreter;
        public int LocalManagedBase;
        public int ParameterCount;
        public int LocalCount;
        public StackObject* RegisterStart;
        public IList<object> ManagedStack;
    }
    public unsafe partial class ILIntepreter
    {   
        internal StackObject* ExecuteR(ILMethod method, StackObject* esp, out bool unhandledException)
        {
            if (method == null)
                throw new NullReferenceException();
#if UNITY_EDITOR
            if (System.Threading.Thread.CurrentThread.ManagedThreadId == AppDomain.UnityMainThreadID)

#if UNITY_5_5_OR_NEWER
                UnityEngine.Profiling.Profiler.BeginSample(method.ToString());
#else
                UnityEngine.Profiler.BeginSample(method.ToString());
#endif

#endif
            OpCodeR[] body = method.BodyRegister;
            StackFrame frame;
            stack.InitializeFrame(method, esp, out frame); 
            StackObject* v1 = frame.LocalVarPointer;
            int finallyEndAddress = 0;

            var stackRegStart = frame.BasePointer;
            esp = Add(frame.BasePointer, method.StackRegisterCount);
            StackObject* r = Minus(frame.LocalVarPointer, method.ParameterCount);
            IList<object> mStack = stack.ManagedStack;
            int paramCnt = method.ParameterCount;
            if (method.HasThis)//this parameter is always object reference
            {
                r--;
                paramCnt++;
            }
            unhandledException = false;
            var hasReturn = method.ReturnType != AppDomain.VoidType;

            //Managed Stack reserved for arguments(In case of starg)
            for (int i = 0; i < paramCnt; i++)
            {
                var a = Add(r, i);
                switch (a->ObjectType)
                {
                    case ObjectTypes.Null:
                        //Need to reserve place for null, in case of starg
                        a->ObjectType = ObjectTypes.Object;
                        a->Value = mStack.Count;
                        mStack.Add(null);
                        break;
                    case ObjectTypes.ValueTypeObjectReference:
                        CloneStackValueType(a, a, mStack);
                        break;
                    case ObjectTypes.Object:
                    case ObjectTypes.FieldReference:
                    case ObjectTypes.ArrayReference:
                        frame.ManagedStackBase--;
                        break;
                }
            }

            stack.PushFrame(ref frame);

            int locBase = mStack.Count;
            int locCnt = method.LocalVariableCount;
            int stackRegCnt = method.StackRegisterCount;
            RegisterFrameInfo info;
            info.Intepreter = this;
            info.LocalCount = locCnt;
            info.LocalManagedBase = locBase;
            info.ParameterCount = paramCnt;
            info.RegisterStart = r;
            info.ManagedStack = mStack;
            //Managed Stack reserved for local variable
            for (int i = 0; i < locCnt; i++)
            {
                mStack.Add(null);
            }

            for (int i = 0; i < locCnt; i++)
            {
                var v = method.Variables[i];
                if (v.VariableType.IsValueType && !v.VariableType.IsPrimitive)
                {
                    var t = AppDomain.GetType(v.VariableType, method.DeclearingType, method);
                    if (t is ILType)
                    {
                        //var obj = ((ILType)t).Instantiate(false);
                        var loc = Add(v1, i);
                        stack.AllocValueType(loc, t);

                        /*loc->ObjectType = ObjectTypes.Object;
                        loc->Value = mStack.Count;
                        mStack.Add(obj);*/

                    }
                    else
                    {
                        CLRType cT = (CLRType)t;
                        var loc = Add(v1, i);
                        if (cT.ValueTypeBinder != null)
                        {
                            stack.AllocValueType(loc, t);
                        }
                        else
                        {
                            var obj = ((CLRType)t).CreateDefaultInstance();
                            loc->ObjectType = ObjectTypes.Object;
                            loc->Value = locBase + i;
                            mStack[locBase + i] = obj;
                        }
                    }
                }
                else
                {
                    if (v.VariableType.IsPrimitive)
                    {
                        var t = AppDomain.GetType(v.VariableType, method.DeclearingType, method);
                        var loc = Add(v1, i);
                        StackObject.Initialized(loc, t);
                    }
                    else
                    {
                        var loc = Add(v1, i);
                        loc->ObjectType = ObjectTypes.Object;
                        loc->Value = locBase + i;
                    }
                }
            }
            for (int i = 0; i < stackRegCnt; i++)
            {
                var loc = Add(stackRegStart, i);
                *loc = StackObject.Null;
                mStack.Add(null);
            }
            var bp = stack.ValueTypeStackPointer;
            ValueTypeBasePointer = bp;

            StackObject* reg1, reg2, reg3, objRef;

            fixed (OpCodeR* ptr = body)
            {
                OpCodeR* ip = ptr;
                OpCodeREnum code = ip->Code;
                bool returned = false;
                while (!returned)
                {
                    try
                    {
#if DEBUG && !DISABLE_ILRUNTIME_DEBUG
                        if (ShouldBreak)
                            Break();
                        var insOffset = (int)(ip - ptr);
                        frame.Address.Value = insOffset;
                        AppDomain.DebugService.CheckShouldBreak(method, this, insOffset);
#endif
                        code = ip->Code;
                        switch (code)
                        {
                            #region Load Constants
                            case OpCodeREnum.Ldc_I4_M1:
                                {
                                    reg1 = Add(r, ip->Register1);
                                    reg1->ObjectType = ObjectTypes.Integer;
                                    reg1->Value = -1;
                                }
                                break;
                            case OpCodeREnum.Ldc_I4_0:
                                {
                                    reg1 = Add(r, ip->Register1);
                                    reg1->ObjectType = ObjectTypes.Integer;
                                    reg1->Value = 0;
                                }
                                break;
                            case OpCodeREnum.Ldc_I4_1:
                                {
                                    reg1 = Add(r, ip->Register1);
                                    reg1->ObjectType = ObjectTypes.Integer;
                                    reg1->Value = 1;
                                }
                                break;
                            case OpCodeREnum.Ldc_I4_2:
                                {
                                    reg1 = Add(r, ip->Register1);
                                    reg1->ObjectType = ObjectTypes.Integer;
                                    reg1->Value = 2;
                                }
                                break;
                            case OpCodeREnum.Ldc_I4_3:
                                {
                                    reg1 = Add(r, ip->Register1);
                                    reg1->ObjectType = ObjectTypes.Integer;
                                    reg1->Value = 3;
                                }
                                break;
                            case OpCodeREnum.Ldc_I4_4:
                                {
                                    reg1 = Add(r, ip->Register1);
                                    reg1->ObjectType = ObjectTypes.Integer;
                                    reg1->Value = 4;
                                }
                                break;
                            case OpCodeREnum.Ldc_I4_5:
                                {
                                    reg1 = Add(r, ip->Register1);
                                    reg1->ObjectType = ObjectTypes.Integer;
                                    reg1->Value = 5;
                                }
                                break;
                            case OpCodeREnum.Ldc_I4_6:
                                {
                                    reg1 = Add(r, ip->Register1);
                                    reg1->ObjectType = ObjectTypes.Integer;
                                    reg1->Value = 6;
                                }
                                break;
                            case OpCodeREnum.Ldc_I4_7:
                                {
                                    reg1 = Add(r, ip->Register1);
                                    reg1->ObjectType = ObjectTypes.Integer;
                                    reg1->Value = 7;
                                }
                                break;
                            case OpCodeREnum.Ldc_I4_8:
                                {
                                    reg1 = Add(r, ip->Register1);
                                    reg1->ObjectType = ObjectTypes.Integer;
                                    reg1->Value = 8;
                                }
                                break;

                            case OpCodeREnum.Ldc_I4:
                            case OpCodeREnum.Ldc_I4_S:
                                reg1 = Add(r, ip->Register1);
                                reg1->ObjectType = ObjectTypes.Integer;
                                reg1->Value = ip->Operand;
                                break;
                            case OpCodeREnum.Ldc_I8:
                                {
                                    reg1 = Add(r, ip->Register1);
                                    *(long*)(&reg1->Value) = ip->OperandLong;
                                    reg1->ObjectType = ObjectTypes.Long;
                                }
                                break;
                            case OpCodeREnum.Ldc_R4:
                                {
                                    reg1 = Add(r, ip->Register1);
                                    *(float*)(&reg1->Value) = ip->OperandFloat;
                                    reg1->ObjectType = ObjectTypes.Float;
                                }
                                break;
                            case OpCodeREnum.Ldc_R8:
                                {
                                    reg1 = Add(r, ip->Register1);
                                    *(double*)(&reg1->Value) = ip->OperandDouble;
                                    reg1->ObjectType = ObjectTypes.Double;
                                }
                                break;
                            case OpCodeREnum.Ldstr:
                                AssignToRegister(ref info, ip->Register1, AppDomain.GetString(ip->OperandLong));
                                break;
                            case OpCodeREnum.Ldnull:
                                reg1 = Add(r, ip->Register1);
                                WriteNull(reg1);
                                break;
                            #endregion

                            #region Althemetics
                            case OpCodeREnum.Add:
                                {
                                    reg1 = Add(r, ip->Register2);
                                    reg2 = Add(r, ip->Register3);
                                    reg3 = Add(r, ip->Register1);
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            reg3->ObjectType = ObjectTypes.Long;
                                            *((long*)&reg3->Value) = *((long*)&reg1->Value) + *((long*)&reg2->Value);
                                            break;
                                        case ObjectTypes.Integer:
                                            reg3->ObjectType = ObjectTypes.Integer;
                                            reg3->Value = reg1->Value + reg2->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            reg3->ObjectType = ObjectTypes.Float;
                                            *((float*)&reg3->Value) = *((float*)&reg1->Value) + *((float*)&reg2->Value);
                                            break;
                                        case ObjectTypes.Double:
                                            reg3->ObjectType = ObjectTypes.Double;
                                            *((double*)&reg3->Value) = *((double*)&reg1->Value) + *((double*)&reg2->Value);
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                }
                                break;
                            case OpCodeREnum.Sub:
                                {
                                    reg1 = Add(r, ip->Register2);
                                    reg2 = Add(r, ip->Register3);
                                    reg3 = Add(r, ip->Register1);
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            reg3->ObjectType = ObjectTypes.Long;
                                            *((long*)&reg3->Value) = *((long*)&reg1->Value) - *((long*)&reg2->Value);
                                            break;
                                        case ObjectTypes.Integer:
                                            reg3->ObjectType = ObjectTypes.Integer;
                                            reg3->Value = reg1->Value - reg2->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            reg3->ObjectType = ObjectTypes.Float;
                                            *((float*)&reg3->Value) = *((float*)&reg1->Value) - *((float*)&reg2->Value);
                                            break;
                                        case ObjectTypes.Double:
                                            reg3->ObjectType = ObjectTypes.Double;
                                            *((double*)&reg3->Value) = *((double*)&reg1->Value) - *((double*)&reg2->Value);
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                }
                                break;
                            case OpCodeREnum.Mul:
                                {
                                    reg1 = Add(r, ip->Register2);
                                    reg2 = Add(r, ip->Register3);
                                    reg3 = Add(r, ip->Register1);
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            reg3->ObjectType = ObjectTypes.Long;
                                            *((long*)&reg3->Value) = *((long*)&reg1->Value) * *((long*)&reg2->Value);
                                            break;
                                        case ObjectTypes.Integer:
                                            reg3->ObjectType = ObjectTypes.Integer;
                                            reg3->Value = reg1->Value * reg2->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            reg3->ObjectType = ObjectTypes.Float;
                                            *((float*)&reg3->Value) = *((float*)&reg1->Value) * *((float*)&reg2->Value);
                                            break;
                                        case ObjectTypes.Double:
                                            reg3->ObjectType = ObjectTypes.Double;
                                            *((double*)&reg3->Value) = *((double*)&reg1->Value) * *((double*)&reg2->Value);
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                }
                                break;
                            case OpCodeREnum.Div:
                                {
                                    reg1 = Add(r, ip->Register2);
                                    reg2 = Add(r, ip->Register3);
                                    reg3 = Add(r, ip->Register1);
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            reg3->ObjectType = ObjectTypes.Long;
                                            *((long*)&reg3->Value) = *((long*)&reg1->Value) / *((long*)&reg2->Value);
                                            break;
                                        case ObjectTypes.Integer:
                                            reg3->ObjectType = ObjectTypes.Integer;
                                            reg3->Value = reg1->Value / reg2->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            reg3->ObjectType = ObjectTypes.Float;
                                            *((float*)&reg3->Value) = *((float*)&reg1->Value) / *((float*)&reg2->Value);
                                            break;
                                        case ObjectTypes.Double:
                                            reg3->ObjectType = ObjectTypes.Double;
                                            *((double*)&reg3->Value) = *((double*)&reg1->Value) / *((double*)&reg2->Value);
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                }
                                break;
                            case OpCodeREnum.Div_Un:
                                {
                                    reg1 = Add(r, ip->Register2);
                                    reg2 = Add(r, ip->Register3);
                                    reg3 = Add(r, ip->Register1);
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            reg3->ObjectType = ObjectTypes.Long;
                                            *((ulong*)&reg3->Value) = *((ulong*)&reg1->Value) / *((ulong*)&reg2->Value);
                                            break;
                                        case ObjectTypes.Integer:
                                            reg3->ObjectType = ObjectTypes.Integer;
                                            reg3->Value = (int)((uint)reg1->Value / (uint)reg2->Value);
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                }
                                break;
                            case OpCodeREnum.Rem:
                                {
                                    reg1 = Add(r, ip->Register2);
                                    reg2 = Add(r, ip->Register3);
                                    reg3 = Add(r, ip->Register1);
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            reg3->ObjectType = ObjectTypes.Long;
                                            *((long*)&reg3->Value) = *((long*)&reg1->Value) % *((long*)&reg2->Value);
                                            break;
                                        case ObjectTypes.Integer:
                                            reg3->ObjectType = ObjectTypes.Integer;
                                            reg3->Value = reg1->Value % reg2->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            reg3->ObjectType = ObjectTypes.Float;
                                            *(float*)&reg3->Value = *(float*)&reg1->Value % *(float*)&reg2->Value;
                                            break;
                                        case ObjectTypes.Double:
                                            reg3->ObjectType = ObjectTypes.Double;
                                            *(double*)&reg3->Value = *(double*)&reg1->Value % *(double*)&reg2->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                }
                                break;
                            case OpCodeREnum.Rem_Un:
                                {
                                    reg1 = Add(r, ip->Register2);
                                    reg2 = Add(r, ip->Register3);
                                    reg3 = Add(r, ip->Register1);
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            reg3->ObjectType = ObjectTypes.Long;
                                            *((ulong*)&reg3->Value) = *((ulong*)&reg1->Value) % *((ulong*)&reg2->Value);
                                            break;
                                        case ObjectTypes.Integer:
                                            reg3->ObjectType = ObjectTypes.Integer;
                                            reg3->Value = (int)((uint)reg1->Value % (uint)reg2->Value);
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                }
                                break;
                            case OpCodeREnum.Xor:
                                {
                                    reg1 = Add(r, ip->Register2);
                                    reg2 = Add(r, ip->Register3);
                                    reg3 = Add(r, ip->Register1);
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            reg3->ObjectType = ObjectTypes.Long;
                                            *((long*)&reg3->Value) = *((long*)&reg1->Value) ^ *((long*)&reg2->Value);
                                            break;
                                        case ObjectTypes.Integer:
                                            reg3->ObjectType = ObjectTypes.Integer;
                                            reg3->Value = reg1->Value ^ reg2->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                }
                                break;
                            case OpCodeREnum.And:
                                {
                                    reg1 = Add(r, ip->Register2);
                                    reg2 = Add(r, ip->Register3);
                                    reg3 = Add(r, ip->Register1);
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            reg3->ObjectType = ObjectTypes.Long;
                                            *((long*)&reg3->Value) = *((long*)&reg1->Value) & *((long*)&reg2->Value);
                                            break;
                                        case ObjectTypes.Integer:
                                            reg3->ObjectType = ObjectTypes.Integer;
                                            reg3->Value = reg1->Value & reg2->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                }
                                break;
                            case OpCodeREnum.Or:
                                {
                                    reg1 = Add(r, ip->Register2);
                                    reg2 = Add(r, ip->Register3);
                                    reg3 = Add(r, ip->Register1);
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            reg3->ObjectType = ObjectTypes.Long;
                                            *((long*)&reg3->Value) = *((long*)&reg1->Value) | *((long*)&reg2->Value);
                                            break;
                                        case ObjectTypes.Integer:
                                            reg3->ObjectType = ObjectTypes.Integer;
                                            reg3->Value = reg1->Value | reg2->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                }
                                break;
                            case OpCodeREnum.Shl:
                                {
                                    reg1 = Add(r, ip->Register2);
                                    reg2 = Add(r, ip->Register3);
                                    reg3 = Add(r, ip->Register1);
                                    int bits = reg2->Value;
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            reg3->ObjectType = ObjectTypes.Long;
                                            *((long*)&reg3->Value) = *((long*)&reg1->Value) << bits;
                                            break;
                                        case ObjectTypes.Integer:
                                            reg3->ObjectType = ObjectTypes.Integer;
                                            reg3->Value = reg1->Value << bits;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                }
                                break;
                            case OpCodeREnum.Shr:
                                {
                                    reg1 = Add(r, ip->Register2);
                                    reg2 = Add(r, ip->Register3);
                                    reg3 = Add(r, ip->Register1);
                                    int bits = reg2->Value;
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            reg3->ObjectType = ObjectTypes.Long;
                                            *((long*)&reg3->Value) = *((long*)&reg1->Value) >> bits;
                                            break;
                                        case ObjectTypes.Integer:
                                            reg3->ObjectType = ObjectTypes.Integer;
                                            reg3->Value = reg1->Value >> bits;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                }
                                break;
                            case OpCodeREnum.Shr_Un:
                                {
                                    reg1 = Add(r, ip->Register2);
                                    reg2 = Add(r, ip->Register3);
                                    reg3 = Add(r, ip->Register1);
                                    int bits = reg2->Value;
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            reg3->ObjectType = ObjectTypes.Long;
                                            *((ulong*)&reg3->Value) = *((ulong*)&reg1->Value) >> bits;
                                            break;
                                        case ObjectTypes.Integer:
                                            reg3->ObjectType = ObjectTypes.Integer;
                                            *(uint*)&reg3->Value = (uint)reg1->Value >> bits;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                }
                                break;
                            case OpCodeREnum.Not:
                                {
                                    reg1 = Add(r, ip->Register2);
                                    reg2 = Add(r, ip->Register1);
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            reg2->ObjectType = ObjectTypes.Long;
                                            *((long*)&reg2->Value) = ~*((long*)&reg1->Value);
                                            break;
                                        case ObjectTypes.Integer:
                                            reg2->ObjectType = ObjectTypes.Integer;
                                            reg2->Value = ~reg1->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                }
                                break;
                            case OpCodeREnum.Neg:
                                {
                                    reg1 = Add(r, ip->Register2);
                                    reg2 = Add(r, ip->Register1);
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            reg2->ObjectType = ObjectTypes.Long;
                                            *((long*)&reg2->Value) = -*((long*)&reg1->Value);
                                            break;
                                        case ObjectTypes.Integer:
                                            reg2->ObjectType = ObjectTypes.Integer;
                                            reg2->Value = -reg1->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            reg2->ObjectType = ObjectTypes.Float;
                                            *((float*)&reg2->Value) = -*((float*)&reg1->Value);
                                            break;
                                        case ObjectTypes.Double:
                                            reg2->ObjectType = ObjectTypes.Double;
                                            *((double*)&reg2->Value) = -*((double*)&reg1->Value);
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                }
                                break;
                            #endregion

                            #region Conversion
                            case OpCodeREnum.Conv_U1:
                            case OpCodeREnum.Conv_Ovf_U1:
                            case OpCodeREnum.Conv_Ovf_U1_Un:
                                {
                                    reg1 = Add(r, ip->Register2);
                                    reg2 = Add(r, ip->Register1);
                                    byte val;
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                        case ObjectTypes.Integer:
                                            val = (byte)reg1->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            val = (byte)*(float*)&reg1->Value;
                                            break;
                                        case ObjectTypes.Double:
                                            val = (byte)*(double*)&reg1->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    reg2->ObjectType = ObjectTypes.Integer;
                                    reg2->Value = val;
                                    reg2->ValueLow = 0;
                                }
                                break;
                            case OpCodeREnum.Conv_I1:
                            case OpCodeREnum.Conv_Ovf_I1:
                            case OpCodeREnum.Conv_Ovf_I1_Un:
                                {
                                    reg1 = Add(r, ip->Register2);
                                    reg2 = Add(r, ip->Register1);
                                    sbyte val;
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                        case ObjectTypes.Integer:
                                            val = (sbyte)reg1->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            val = (sbyte)*(float*)&reg1->Value;
                                            break;
                                        case ObjectTypes.Double:
                                            val = (sbyte)*(double*)&reg1->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    reg2->ObjectType = ObjectTypes.Integer;
                                    reg2->Value = val;
                                    reg2->ValueLow = 0;
                                }
                                break;
                            case OpCodeREnum.Conv_U2:
                            case OpCodeREnum.Conv_Ovf_U2:
                            case OpCodeREnum.Conv_Ovf_U2_Un:
                                {
                                    reg1 = Add(r, ip->Register2);
                                    reg2 = Add(r, ip->Register1);
                                    ushort val;
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                        case ObjectTypes.Integer:
                                            val = (ushort)reg1->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            val = (ushort)*(float*)&reg1->Value;
                                            break;
                                        case ObjectTypes.Double:
                                            val = (ushort)*(double*)&reg1->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    reg2->ObjectType = ObjectTypes.Integer;
                                    reg2->Value = val;
                                    reg2->ValueLow = 0;
                                }
                                break;
                            case OpCodeREnum.Conv_I2:
                            case OpCodeREnum.Conv_Ovf_I2:
                            case OpCodeREnum.Conv_Ovf_I2_Un:
                                {
                                    reg1 = Add(r, ip->Register2);
                                    reg2 = Add(r, ip->Register1);
                                    short val;
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                        case ObjectTypes.Integer:
                                            val = (short)(reg1->Value);
                                            break;
                                        case ObjectTypes.Float:
                                            val = (short)*(float*)&reg1->Value;
                                            break;
                                        case ObjectTypes.Double:
                                            val = (short)*(double*)&reg1->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    reg2->ObjectType = ObjectTypes.Integer;
                                    reg2->Value = val;
                                    reg2->ValueLow = 0;
                                }
                                break;
                            case OpCodeREnum.Conv_U4:
                            case OpCodeREnum.Conv_U:
                            case OpCodeREnum.Conv_Ovf_U4:
                            case OpCodeREnum.Conv_Ovf_U4_Un:
                                {
                                    reg1 = Add(r, ip->Register2);
                                    reg2 = Add(r, ip->Register1);
                                    uint uintVal;
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            uintVal = (uint)*(long*)&reg1->Value;
                                            break;
                                        case ObjectTypes.Integer:
                                            uintVal = (uint)reg1->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            uintVal = (uint)*(float*)&reg1->Value;
                                            break;
                                        case ObjectTypes.Double:
                                            uintVal = (uint)*(double*)&reg1->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    reg2->ObjectType = ObjectTypes.Integer;
                                    reg2->Value = (int)uintVal;
                                    reg2->ValueLow = 0;
                                }
                                break;
                            case OpCodeREnum.Conv_I4:
                            case OpCodeREnum.Conv_I:
                            case OpCodeREnum.Conv_Ovf_I:
                            case OpCodeREnum.Conv_Ovf_I_Un:
                            case OpCodeREnum.Conv_Ovf_I4:
                            case OpCodeREnum.Conv_Ovf_I4_Un:
                                {
                                    reg1 = Add(r, ip->Register2);
                                    reg2 = Add(r, ip->Register1);
                                    int val;
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            val = (int)*(long*)&reg1->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            val = (int)*(float*)&reg1->Value;
                                            break;
                                        case ObjectTypes.Double:
                                            val = (int)*(double*)&reg1->Value;
                                            break;
                                        case ObjectTypes.Integer:
                                            val = reg1->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    reg2->ObjectType = ObjectTypes.Integer;
                                    reg2->Value = val;
                                }
                                break;
                            case OpCodeREnum.Conv_I8:
                            case OpCodeREnum.Conv_Ovf_I8:
                            case OpCodeREnum.Conv_Ovf_I8_Un:
                                {
                                    reg1 = Add(r, ip->Register2);
                                    reg2 = Add(r, ip->Register1);
                                    long val;
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            val = reg1->Value;
                                            break;
                                        case ObjectTypes.Long:
                                            ip++;
                                            continue;
                                        case ObjectTypes.Float:
                                            val = (long)*(float*)&reg1->Value;
                                            break;
                                        case ObjectTypes.Double:
                                            val = (long)*(double*)&reg1->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    reg2->ObjectType = ObjectTypes.Long;
                                    *(long*)(&reg2->Value) = val;
                                }
                                break;
                            case OpCodeREnum.Conv_U8:
                            case OpCodeREnum.Conv_Ovf_U8:
                            case OpCodeREnum.Conv_Ovf_U8_Un:
                                {
                                    reg1 = Add(r, ip->Register2);
                                    reg2 = Add(r, ip->Register1);
                                    ulong ulongVal;
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            ulongVal = (uint)reg1->Value;
                                            break;
                                        case ObjectTypes.Long:
                                            ip++;
                                            continue;
                                        case ObjectTypes.Float:
                                            ulongVal = (ulong)*(float*)&reg1->Value;
                                            break;
                                        case ObjectTypes.Double:
                                            ulongVal = (ulong)*(double*)&reg1->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    reg2->ObjectType = ObjectTypes.Long;
                                    *(ulong*)(&reg2->Value) = ulongVal;
                                }
                                break;
                            case OpCodeREnum.Conv_R4:
                                {
                                    reg1 = Add(r, ip->Register2);
                                    reg2 = Add(r, ip->Register1);
                                    float val;
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            val = (float)*(long*)&reg1->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            ip++;
                                            continue;
                                        case ObjectTypes.Double:
                                            val = (float)*(double*)&reg1->Value;
                                            break;
                                        case ObjectTypes.Integer:
                                            val = reg1->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    reg2->ObjectType = ObjectTypes.Float;
                                    *(float*)&reg2->Value = val;
                                }
                                break;
                            case OpCodeREnum.Conv_R8:
                                {
                                    reg1 = Add(r, ip->Register2);
                                    reg2 = Add(r, ip->Register1);
                                    double val;
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            val = (double)*(long*)&reg1->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            val = *(float*)&reg1->Value;
                                            break;
                                        case ObjectTypes.Integer:
                                            val = reg1->Value;
                                            break;
                                        case ObjectTypes.Double:
                                            ip++;
                                            continue;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    reg2->ObjectType = ObjectTypes.Double;
                                    *(double*)&reg2->Value = val;
                                }
                                break;
                            case OpCodeREnum.Conv_R_Un:
                                {
                                    reg1 = Add(r, ip->Register2);
                                    reg2 = Add(r, ip->Register1);
                                    bool isDouble = false;
                                    float val = 0;
                                    double val2 = 0;
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Long:
                                            val2 = (double)*(ulong*)&reg1->Value;
                                            isDouble = true;
                                            break;
                                        case ObjectTypes.Float:
                                            ip++;
                                            continue;
                                        case ObjectTypes.Integer:
                                            val = (uint)reg1->Value;
                                            break;
                                        case ObjectTypes.Double:
                                            ip++;
                                            continue;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    if (isDouble)
                                    {
                                        reg2->ObjectType = ObjectTypes.Double;
                                        *(double*)&reg2->Value = val2;
                                    }
                                    else
                                    {
                                        reg2->ObjectType = ObjectTypes.Float;
                                        *(float*)&reg2->Value = val;
                                    }
                                }
                                break;
                            #endregion

                            #region Load Store
                            case OpCodeREnum.Move:
                                {
                                    reg1 = Add(r, ip->Register2);
                                    CopyToRegister(ref info, ip->Register1, reg1);
                                }
                                break;

                            case OpCodeREnum.Push:
                                {
                                    reg1 = Add(r, ip->Register1);
                                    CopyToStack(esp, reg1, mStack);
                                    esp++;
                                }
                                break;
                            case OpCodeREnum.Ldobj:
                                {
                                    reg1 = Add(r, ip->Register2);
                                    reg2 = Add(r, ip->Register1);
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.ArrayReference:
                                            {
                                                var t = AppDomain.GetType(ip->Operand);
                                                var obj = mStack[reg1->Value];
                                                var idx = reg1->ValueLow;
                                                //Free(objRef);
                                                LoadFromArrayReference(obj, idx, reg2, t, mStack);
                                            }
                                            break;
                                        case ObjectTypes.StackObjectReference:
                                            {
                                                var objRef2 = GetObjectAndResolveReference(reg1);
                                                *reg2 = *objRef2;
                                                if (reg2->ObjectType >= ObjectTypes.Object)
                                                {
                                                    reg2->Value = mStack.Count;
                                                    mStack.Add(mStack[objRef2->Value]);
                                                }
                                            }
                                            break;
                                        case ObjectTypes.FieldReference:
                                            {
                                                var obj = mStack[reg1->Value];
                                                int idx = reg1->ValueLow;
                                                //Free(objRef);
                                                if (obj is ILTypeInstance)
                                                {
                                                    ((ILTypeInstance)obj).PushToStack(idx, reg2, AppDomain, mStack);
                                                }
                                                else
                                                {
                                                    var t = AppDomain.GetType(ip->Operand);
                                                    obj = ((CLRType)t).GetFieldValue(idx, obj);
                                                    PushObject(reg2, mStack, obj);
                                                }
                                            }
                                            break;
                                        case ObjectTypes.StaticFieldReference:
                                            {
                                                var t = AppDomain.GetType(reg1->Value);
                                                int idx = reg1->ValueLow;
                                                //Free(objRef);
                                                if (t is ILType)
                                                {
                                                    ((ILType)t).StaticInstance.PushToStack(idx, reg2, AppDomain, mStack);
                                                }
                                                else
                                                {
                                                    var obj = ((CLRType)t).GetFieldValue(idx, null);
                                                    PushObject(reg2, mStack, obj);
                                                }
                                            }
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                }
                                break;
                            case OpCodeREnum.Stobj:
                                {
                                    reg1 = Add(r, ip->Register2);
                                    reg2 = Add(r, ip->Register1);
                                    switch (reg2->ObjectType)
                                    {
                                        case ObjectTypes.ArrayReference:
                                            {
                                                var t = AppDomain.GetType(ip->Operand);
                                                StoreValueToArrayReference(reg2, reg1, t, mStack);
                                            }
                                            break;
                                        case ObjectTypes.StackObjectReference:
                                            {
                                                objRef = GetObjectAndResolveReference(reg2);
                                                if (objRef->ObjectType == ObjectTypes.ValueTypeObjectReference)
                                                {
                                                    switch (reg1->ObjectType)
                                                    {
                                                        case ObjectTypes.Object:
                                                            var dst = ILIntepreter.ResolveReference(objRef);
                                                            CopyValueTypeToStack(dst, mStack[reg1->Value], mStack);
                                                            break;
                                                        case ObjectTypes.ValueTypeObjectReference:
                                                            CopyStackValueType(reg1, objRef, mStack);
                                                            break;
                                                        default:
                                                            throw new NotImplementedException();
                                                    }
                                                }
                                                else
                                                {
                                                    if (reg1->ObjectType >= ObjectTypes.Object)
                                                    {
                                                        mStack[objRef->Value] = mStack[reg1->Value];
                                                        objRef->ValueLow = reg1->ValueLow;
                                                    }
                                                    else
                                                    {
                                                        *objRef = *reg1;
                                                    }
                                                }
                                            }
                                            break;
                                        case ObjectTypes.FieldReference:
                                            {
                                                var obj = mStack[reg2->Value];
                                                int idx = reg2->ValueLow;
                                                if (obj is ILTypeInstance)
                                                {
                                                    ((ILTypeInstance)obj).AssignFromStack(idx, reg1, AppDomain, mStack);
                                                }
                                                else
                                                {
                                                    var t = AppDomain.GetType(ip->Operand);
                                                    ((CLRType)t).SetFieldValue(idx, ref obj, t.TypeForCLR.CheckCLRTypes(StackObject.ToObject(reg1, AppDomain, mStack)));
                                                }
                                            }
                                            break;
                                        case ObjectTypes.StaticFieldReference:
                                            {
                                                var t = AppDomain.GetType(reg2->Value);
                                                if (t is ILType)
                                                {
                                                    ((ILType)t).StaticInstance.AssignFromStack(reg2->ValueLow, reg1, AppDomain, mStack);
                                                }
                                                else
                                                {
                                                    ((CLRType)t).SetStaticFieldValue(reg2->ValueLow, t.TypeForCLR.CheckCLRTypes(StackObject.ToObject(reg1, AppDomain, mStack)));
                                                }
                                            }
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                }
                                break;
                            case OpCodeREnum.Ldloca:
                            case OpCodeREnum.Ldloca_S:
                                {
                                    reg1 = Add(r, ip->Register2);
                                    reg2 = Add(r, ip->Register1);

                                    reg2->ObjectType = ObjectTypes.StackObjectReference;
                                    *(long*)&reg2->Value = (long)reg1;
                                }
                                break;
                            case OpCodeREnum.Ldarga:
                            case OpCodeREnum.Ldarga_S:
                                {
                                    reg1 = Add(r, ip->Register2);
                                    reg2 = Add(r, ip->Register1);

                                    reg2->ObjectType = ObjectTypes.StackObjectReference;
                                    *(long*)&reg2->Value = (long)reg1;
                                }
                                break;
                            case OpCodeREnum.Starg:
                            case OpCodeREnum.Starg_S:
                                {
                                    reg1 = Add(r, ip->Register2);
                                    reg2 = Add(r, ip->Register1);

                                    int idx = reg1->Value;
                                    bool isObj = reg1->ObjectType >= ObjectTypes.Object;
                                    if (reg2->ObjectType >= ObjectTypes.Object)
                                    {
                                        if (reg1->ObjectType == ObjectTypes.ValueTypeObjectReference)
                                        {
                                            var dst = ILIntepreter.ResolveReference(reg1);
                                            CopyValueTypeToStack(dst, mStack[reg2->Value], mStack);
                                        }
                                        else
                                        {
                                            reg1->ObjectType = reg2->ObjectType;
                                            mStack[reg1->Value] = mStack[reg2->Value];
                                            reg1->ValueLow = reg2->ValueLow;
                                        }
                                    }
                                    else
                                    {
                                        if (reg1->ObjectType == ObjectTypes.ValueTypeObjectReference)
                                        {
                                            if (reg2->ObjectType == ObjectTypes.ValueTypeObjectReference)
                                            {
                                                CopyStackValueType(reg2, reg1, mStack);
                                                //FreeStackValueType(reg2);
                                            }
                                            else
                                                throw new NotSupportedException();
                                        }
                                        else
                                        {
                                            *reg1 = *reg2;
                                            if (isObj)
                                            {
                                                reg1->Value = idx;
                                                if (reg2->ObjectType == ObjectTypes.Null)
                                                {
                                                    mStack[reg1->Value] = null;
                                                }
                                            }
                                        }
                                    }
                                }
                                break;
                            case OpCodeREnum.Ldind_I:
                            case OpCodeREnum.Ldind_I1:
                            case OpCodeREnum.Ldind_I2:
                            case OpCodeREnum.Ldind_I4:
                            case OpCodeREnum.Ldind_U1:
                            case OpCodeREnum.Ldind_U2:
                            case OpCodeREnum.Ldind_U4:
                                {
                                    reg1 = Add(r, ip->Register2);
                                    reg2 = Add(r, ip->Register1);
                                    var val = GetObjectAndResolveReference(reg1);
                                    switch (val->ObjectType)
                                    {
                                        case ObjectTypes.FieldReference:
                                            {
                                                var instance = mStack[val->Value];
                                                var idx = val->ValueLow;
                                                Free(reg2);
                                                LoadFromFieldReference(instance, idx, reg2, mStack);
                                            }
                                            break;
                                        case ObjectTypes.ArrayReference:
                                            {
                                                var instance = mStack[val->Value];
                                                var idx = val->ValueLow;
                                                Free(reg2);
                                                LoadFromArrayReference(instance, idx, reg2, instance.GetType().GetElementType(), mStack);
                                            }
                                            break;
                                        default:
                                            {
                                                reg2->ObjectType = ObjectTypes.Integer;
                                                reg2->Value = val->Value;
                                                reg2->ValueLow = 0;
                                            }
                                            break;
                                    }
                                }
                                break;
                            case OpCodeREnum.Stind_I:
                            case OpCodeREnum.Stind_I1:
                            case OpCodeREnum.Stind_I2:
                            case OpCodeREnum.Stind_I4:
                                {
                                    reg1 = Add(r, ip->Register1);
                                    reg2 = Add(r, ip->Register2);
                                    var dst = GetObjectAndResolveReference(reg1);
                                    switch (dst->ObjectType)
                                    {
                                        case ObjectTypes.FieldReference:
                                            {
                                                StoreValueToFieldReference(mStack[dst->Value], dst->ValueLow, reg2, mStack);
                                            }
                                            break;
                                        case ObjectTypes.ArrayReference:
                                            {
                                                StoreValueToArrayReference(dst, reg2, mStack[dst->Value].GetType().GetElementType(), mStack);
                                            }
                                            break;
                                        default:
                                            {
                                                *dst = *reg2;
                                                dst->ObjectType = ObjectTypes.Integer;
                                            }
                                            break;
                                    }
                                }
                                break;
                            case OpCodeREnum.Stind_R4:
                                {
                                    reg1 = Add(r, ip->Register1);
                                    reg2 = Add(r, ip->Register2);
                                    var dst = GetObjectAndResolveReference(reg1);
                                    switch (dst->ObjectType)
                                    {
                                        case ObjectTypes.FieldReference:
                                            {
                                                StoreValueToFieldReference(mStack[dst->Value], dst->ValueLow, reg2, mStack);
                                            }
                                            break;
                                        case ObjectTypes.ArrayReference:
                                            {
                                                StoreValueToArrayReference(dst, reg2, mStack[dst->Value].GetType().GetElementType(), mStack);
                                            }
                                            break;
                                        default:
                                            {
                                                *dst = *reg2;
                                                dst->ObjectType = ObjectTypes.Float;
                                            }
                                            break;
                                    }
                                }
                                break;
                            case OpCodeREnum.Stind_I8:
                                {
                                    reg1 = Add(r, ip->Register1);
                                    reg2 = Add(r, ip->Register2);
                                    var dst = GetObjectAndResolveReference(reg1);
                                    switch (dst->ObjectType)
                                    {
                                        case ObjectTypes.FieldReference:
                                            {
                                                StoreValueToFieldReference(mStack[dst->Value], dst->ValueLow, reg2, mStack);
                                            }
                                            break;
                                        case ObjectTypes.ArrayReference:
                                            {
                                                StoreValueToArrayReference(dst, reg2, typeof(long), mStack);
                                            }
                                            break;
                                        default:
                                            {
                                                *dst = *reg2;
                                                dst->ObjectType = ObjectTypes.Long;
                                            }
                                            break;
                                    }
                                }
                                break;
                            case OpCodeREnum.Stind_R8:
                                {
                                    reg1 = Add(r, ip->Register1);
                                    reg2 = Add(r, ip->Register2);
                                    var dst = GetObjectAndResolveReference(reg1);
                                    switch (dst->ObjectType)
                                    {
                                        case ObjectTypes.FieldReference:
                                            {
                                                StoreValueToFieldReference(mStack[dst->Value], dst->ValueLow, reg2, mStack);
                                            }
                                            break;
                                        case ObjectTypes.ArrayReference:
                                            {
                                                StoreValueToArrayReference(dst, reg2, typeof(double), mStack);
                                            }
                                            break;
                                        default:
                                            {
                                                *dst = *reg2;
                                                dst->ObjectType = ObjectTypes.Double;
                                            }
                                            break;
                                    }
                                }
                                break;
                            case OpCodeREnum.Stind_Ref:
                                {
                                    reg1 = Add(r, ip->Register1);
                                    reg2 = Add(r, ip->Register2);
                                    var dst = GetObjectAndResolveReference(reg1);
                                    switch (dst->ObjectType)
                                    {
                                        case ObjectTypes.FieldReference:
                                            {
                                                StoreValueToFieldReference(mStack[dst->Value], dst->ValueLow, reg2, mStack);
                                            }
                                            break;
                                        case ObjectTypes.ArrayReference:
                                            {
                                                StoreValueToArrayReference(dst, reg2, typeof(object), mStack);
                                            }
                                            break;
                                        default:
                                            {
                                                switch (reg2->ObjectType)
                                                {
                                                    case ObjectTypes.Object:
                                                        mStack[dst->Value] = mStack[reg2->Value];
                                                        break;
                                                    case ObjectTypes.Null:
                                                        mStack[dst->Value] = null;
                                                        break;
                                                    default:
                                                        throw new NotImplementedException();
                                                }
                                            }
                                            break;
                                    }
                                }
                                break;
                            case OpCodeREnum.Ldind_I8:
                                {
                                    reg1 = Add(r, ip->Register2);
                                    reg2 = Add(r, ip->Register1);
                                    var val = GetObjectAndResolveReference(reg1);
                                    switch (val->ObjectType)
                                    {
                                        case ObjectTypes.FieldReference:
                                            {
                                                var instance = mStack[val->Value];
                                                var idx = val->ValueLow;
                                                Free(reg2);
                                                LoadFromFieldReference(instance, idx, reg2, mStack);
                                            }
                                            break;
                                        case ObjectTypes.ArrayReference:
                                            {
                                                var instance = mStack[val->Value];
                                                var idx = val->ValueLow;
                                                Free(reg2);
                                                LoadFromArrayReference(instance, idx, reg2, instance.GetType().GetElementType(), mStack);
                                            }
                                            break;
                                        default:
                                            {
                                                *reg2 = *val;
                                                reg2->ObjectType = ObjectTypes.Long;
                                            }
                                            break;
                                    }
                                }
                                break;
                            case OpCodeREnum.Ldind_R4:
                                {
                                    reg1 = Add(r, ip->Register2);
                                    reg2 = Add(r, ip->Register1);
                                    var val = GetObjectAndResolveReference(reg1);
                                    switch (val->ObjectType)
                                    {
                                        case ObjectTypes.FieldReference:
                                            {
                                                var instance = mStack[val->Value];
                                                var idx = val->ValueLow;
                                                Free(reg2);
                                                LoadFromFieldReference(instance, idx, reg2, mStack);
                                            }
                                            break;
                                        case ObjectTypes.ArrayReference:
                                            {
                                                var instance = mStack[val->Value];
                                                var idx = val->ValueLow;
                                                Free(reg2);
                                                LoadFromArrayReference(instance, idx, reg2, instance.GetType().GetElementType(), mStack);
                                            }
                                            break;
                                        default:
                                            {
                                                *reg2 = *val;
                                                reg2->ObjectType = ObjectTypes.Float;
                                            }
                                            break;
                                    }
                                }
                                break;
                            case OpCodeREnum.Ldind_R8:
                                {
                                    reg1 = Add(r, ip->Register2);
                                    reg2 = Add(r, ip->Register1);
                                    var val = GetObjectAndResolveReference(reg1);
                                    switch (val->ObjectType)
                                    {
                                        case ObjectTypes.FieldReference:
                                            {
                                                var instance = mStack[val->Value];
                                                var idx = val->ValueLow;
                                                Free(reg2);
                                                LoadFromFieldReference(instance, idx, reg2, mStack);
                                            }
                                            break;
                                        case ObjectTypes.ArrayReference:
                                            {
                                                var instance = mStack[val->Value];
                                                var idx = val->ValueLow;
                                                Free(reg2);
                                                LoadFromArrayReference(instance, idx, reg2, instance.GetType().GetElementType(), mStack);
                                            }
                                            break;
                                        default:
                                            {
                                                *reg2 = *val;
                                                reg2->ObjectType = ObjectTypes.Double;
                                            }
                                            break;
                                    }
                                }
                                break;
                            case OpCodeREnum.Ldind_Ref:
                                {
                                    reg1 = Add(r, ip->Register2);
                                    reg2 = Add(r, ip->Register1);
                                    reg1 = GetObjectAndResolveReference(reg1);
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.FieldReference:
                                            {
                                                var instance = mStack[reg1->Value];
                                                var idx = reg1->ValueLow;
                                                //Free(dst);
                                                LoadFromFieldReference(instance, idx, reg2, mStack);
                                            }
                                            break;
                                        case ObjectTypes.ArrayReference:
                                            {
                                                var instance = mStack[reg1->Value];
                                                var idx = reg1->ValueLow;
                                                //Free(dst);
                                                LoadFromArrayReference(instance, idx, reg2, instance.GetType().GetElementType(), mStack);
                                            }
                                            break;
                                        //Add By LiYu 2019.07.04 B
                                        case ObjectTypes.StaticFieldReference:
                                            {
                                                var t = AppDomain.GetType(reg1->Value);
                                                int idx = reg1->ValueLow;
                                                if (t is ILType)
                                                {
                                                    ((ILType)t).StaticInstance.PushToStack(idx, reg2, AppDomain, mStack);
                                                }
                                                else
                                                {
                                                    var obj = ((CLRType)t).GetFieldValue(idx, null);
                                                    PushObject(reg2, mStack, obj);
                                                }
                                            }
                                            break;
                                        //Add By LiYu 2019.07.04 E	
                                        default:
                                            {
                                                reg2->ObjectType = ObjectTypes.Object;
                                                reg2->Value = mStack.Count;
                                                mStack.Add(mStack[reg1->Value]);
                                            }
                                            break;
                                    }
                                }
                                break;
                            case OpCodeREnum.Ldtoken:
                                {
                                    switch (ip->Operand)
                                    {
                                        case 0:
                                            {
                                                IType type = AppDomain.GetType((int)(ip->OperandLong >> 32));
                                                if (type != null)
                                                {
                                                    if (type is ILType)
                                                    {
                                                        ILType t = type as ILType;

                                                        t.StaticInstance.CopyToRegister((int)ip->OperandLong,ref info, ip->Register1);
                                                    }
                                                    else
                                                        throw new NotImplementedException();
                                                }
                                            }
                                            break;
                                        case 1:
                                            {
                                                IType type = AppDomain.GetType((int)ip->OperandLong);
                                                if (type != null)
                                                {
                                                    AssignToRegister(ref info, ip->Register1, type.ReflectionType);
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
                            #endregion

                            #region Control Flow
                            case OpCodeREnum.Ret:
                                if (hasReturn)
                                {
                                    reg1 = Add(r, ip->Register1);
                                    CopyToStack(esp, reg1, mStack);
                                    esp++;
                                }
                                returned = true;
                                break;
                            case OpCodeREnum.Ble:
                            case OpCodeREnum.Ble_S:
                                {
                                    reg1 = Add(r, ip->Register1);
                                    reg2 = Add(r, ip->Register2);

                                    bool transfer = false;
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            transfer = reg1->Value <= reg2->Value;
                                            break;
                                        case ObjectTypes.Long:
                                            transfer = *(long*)&reg1->Value <= *(long*)&reg2->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            transfer = *(float*)&reg1->Value <= *(float*)&reg2->Value;
                                            break;
                                        case ObjectTypes.Double:
                                            transfer = *(double*)&reg1->Value <= *(double*)&reg2->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }

                                    if (transfer)
                                    {
                                        ip = ptr + ip->Operand;
                                        continue;
                                    }

                                }
                                break;
                            case OpCodeREnum.Ble_Un:
                            case OpCodeREnum.Ble_Un_S:
                                {
                                    reg1 = Add(r, ip->Register1);
                                    reg2 = Add(r, ip->Register2);
                                    bool transfer = false;
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            transfer = (uint)reg1->Value <= (uint)reg2->Value;
                                            break;
                                        case ObjectTypes.Long:
                                            transfer = *(ulong*)&reg1->Value <= *(ulong*)&reg2->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            transfer = *(float*)&reg1->Value <= *(float*)&reg2->Value;
                                            break;
                                        case ObjectTypes.Double:
                                            transfer = *(double*)&reg1->Value <= *(double*)&reg2->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }

                                    if (transfer)
                                    {
                                        ip = ptr + ip->Operand;
                                        continue;
                                    }

                                }
                                break;
                            case OpCodeREnum.Br_S:
                            case OpCodeREnum.Br:
                                ip = ptr + ip->Operand;
                                continue;
                            case OpCodeREnum.Brtrue:
                            case OpCodeREnum.Brtrue_S:
                                {
                                    reg1 = Add(r, ip->Register1);
                                    bool res = false;
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            res = reg1->Value != 0;
                                            break;
                                        case ObjectTypes.Long:
                                            res = *(long*)&reg1->Value != 0;
                                            break;
                                        case ObjectTypes.Object:
                                            res = mStack[reg1->Value] != null;
                                            break;
                                    }
                                    if (res)
                                    {
                                        ip = ptr + ip->Operand;
                                        continue;
                                    }
                                }
                                break;
                            case OpCodeREnum.Brfalse:
                            case OpCodeREnum.Brfalse_S:
                                {
                                    reg1 = Add(r, ip->Register1);
                                    bool res = false;
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Null:
                                            res = true;
                                            break;
                                        case ObjectTypes.Integer:
                                            res = reg1->Value == 0;
                                            break;
                                        case ObjectTypes.Long:
                                            res = *(long*)&reg1->Value == 0;
                                            break;
                                        case ObjectTypes.Object:
                                            res = mStack[reg1->Value] == null;
                                            break;
                                    }
                                    if (res)
                                    {
                                        ip = ptr + ip->Operand;
                                        continue;
                                    }
                                }
                                break;
                            case OpCodeREnum.Beq:
                            case OpCodeREnum.Beq_S:
                                {
                                    reg1 = Add(r, ip->Register1);
                                    reg2 = Add(r, ip->Register2);
                                    bool transfer = false;
                                    if (reg1->ObjectType == reg2->ObjectType)
                                    {
                                        switch (reg1->ObjectType)
                                        {
                                            case ObjectTypes.Null:
                                                transfer = true;
                                                break;
                                            case ObjectTypes.Integer:
                                                transfer = reg1->Value == reg2->Value;
                                                break;
                                            case ObjectTypes.Long:
                                                transfer = *(long*)&reg1->Value == *(long*)&reg2->Value;
                                                break;
                                            case ObjectTypes.Float:
                                                transfer = *(float*)&reg1->Value == *(float*)&reg2->Value;
                                                break;
                                            case ObjectTypes.Double:
                                                transfer = *(double*)&reg1->Value == *(double*)&reg2->Value;
                                                break;
                                            case ObjectTypes.Object:
                                                transfer = mStack[reg1->Value] == mStack[reg2->Value];
                                                break;
                                            default:
                                                throw new NotImplementedException();
                                        }
                                    }
                                    if (transfer)
                                    {
                                        ip = ptr + ip->Operand;
                                        continue;
                                    }

                                }
                                break;
                            case OpCodeREnum.Bne_Un:
                            case OpCodeREnum.Bne_Un_S:
                                {
                                    reg1 = Add(r, ip->Register1);
                                    reg2 = Add(r, ip->Register2);
                                    bool transfer = false;
                                    if (reg1->ObjectType == reg2->ObjectType)
                                    {
                                        switch (reg1->ObjectType)
                                        {
                                            case ObjectTypes.Null:
                                                transfer = false;
                                                break;
                                            case ObjectTypes.Integer:
                                                transfer = (uint)reg1->Value != (uint)reg2->Value;
                                                break;
                                            case ObjectTypes.Float:
                                                transfer = *(float*)&reg1->Value != *(float*)&reg2->Value;
                                                break;
                                            case ObjectTypes.Long:
                                                transfer = *(long*)&reg1->Value != *(long*)&reg2->Value;
                                                break;
                                            case ObjectTypes.Double:
                                                transfer = *(double*)&reg1->Value != *(double*)&reg2->Value;
                                                break;
                                            case ObjectTypes.Object:
                                                transfer = mStack[reg1->Value] != mStack[reg2->Value];
                                                break;
                                            default:
                                                throw new NotImplementedException();
                                        }
                                    }
                                    else
                                        transfer = true;
                                    if (transfer)
                                    {
                                        ip = ptr + ip->Operand;
                                        continue;
                                    }
                                }
                                break;
                            case OpCodeREnum.Bgt:
                            case OpCodeREnum.Bgt_S:
                                {
                                    reg1 = Add(r, ip->Register1);
                                    reg2 = Add(r, ip->Register2);
                                    bool transfer = false;
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            transfer = reg1->Value > reg2->Value;
                                            break;
                                        case ObjectTypes.Long:
                                            transfer = *(long*)&reg1->Value > *(long*)&reg2->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            transfer = *(float*)&reg1->Value > *(float*)&reg2->Value;
                                            break;
                                        case ObjectTypes.Double:
                                            transfer = *(double*)&reg1->Value > *(double*)&reg2->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }

                                    if (transfer)
                                    {
                                        ip = ptr + ip->Operand;
                                        continue;
                                    }

                                }
                                break;
                            case OpCodeREnum.Bgt_Un:
                            case OpCodeREnum.Bgt_Un_S:
                                {
                                    reg1 = Add(r, ip->Register1);
                                    reg2 = Add(r, ip->Register2);
                                    bool transfer = false;
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            transfer = (uint)reg1->Value > (uint)reg2->Value;
                                            break;
                                        case ObjectTypes.Long:
                                            transfer = *(ulong*)&reg1->Value > *(ulong*)&reg2->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            transfer = *(float*)&reg1->Value > *(float*)&reg2->Value;
                                            break;
                                        case ObjectTypes.Double:
                                            transfer = *(double*)&reg1->Value > *(double*)&reg2->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }

                                    if (transfer)
                                    {
                                        ip = ptr + ip->Operand;
                                        continue;
                                    }

                                }
                                break;
                            case OpCodeREnum.Bge:
                            case OpCodeREnum.Bge_S:
                                {
                                    reg1 = Add(r, ip->Register1);
                                    reg2 = Add(r, ip->Register2);
                                    bool transfer = false;
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            transfer = reg1->Value >= reg2->Value;
                                            break;
                                        case ObjectTypes.Long:
                                            transfer = *(long*)&reg1->Value >= *(long*)&reg2->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            transfer = *(float*)&reg1->Value >= *(float*)&reg2->Value;
                                            break;
                                        case ObjectTypes.Double:
                                            transfer = *(double*)&reg1->Value >= *(double*)&reg2->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }

                                    if (transfer)
                                    {
                                        ip = ptr + ip->Operand;
                                        continue;
                                    }
                                }
                                break;
                            case OpCodeREnum.Bge_Un:
                            case OpCodeREnum.Bge_Un_S:
                                {
                                    reg1 = Add(r, ip->Register1);
                                    reg2 = Add(r, ip->Register2);
                                    bool transfer = false;
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            transfer = (uint)reg1->Value >= (uint)reg2->Value;
                                            break;
                                        case ObjectTypes.Long:
                                            transfer = *(ulong*)&reg1->Value >= *(ulong*)&reg2->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            transfer = *(float*)&reg1->Value >= *(float*)&reg2->Value;
                                            break;
                                        case ObjectTypes.Double:
                                            transfer = *(double*)&reg1->Value >= *(double*)&reg2->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }

                                    if (transfer)
                                    {
                                        ip = ptr + ip->Operand;
                                        continue;
                                    }

                                }
                                break;
                            case OpCodeREnum.Blt:
                            case OpCodeREnum.Blt_S:
                                {
                                    reg1 = Add(r, ip->Register1);
                                    reg2 = Add(r, ip->Register2);
                                    bool transfer = false;
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            transfer = reg1->Value < reg2->Value;
                                            break;
                                        case ObjectTypes.Long:
                                            transfer = *(long*)&reg1->Value < *(long*)&reg2->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            transfer = *(float*)&reg1->Value < *(float*)&reg2->Value;
                                            break;
                                        case ObjectTypes.Double:
                                            transfer = *(double*)&reg1->Value < *(double*)&reg2->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }

                                    if (transfer)
                                    {
                                        ip = ptr + ip->Operand;
                                        continue;
                                    }

                                }
                                break;
                            case OpCodeREnum.Blt_Un:
                            case OpCodeREnum.Blt_Un_S:
                                {
                                    reg1 = Add(r, ip->Register1);
                                    reg2 = Add(r, ip->Register2);
                                    bool transfer = false;
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            transfer = (uint)reg1->Value < (uint)reg2->Value;
                                            break;
                                        case ObjectTypes.Long:
                                            transfer = *(ulong*)&reg1->Value < *(ulong*)&reg2->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            transfer = *(float*)&reg1->Value < *(float*)&reg2->Value;
                                            break;
                                        case ObjectTypes.Double:
                                            transfer = *(double*)&reg1->Value < *(double*)&reg2->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }

                                    if (transfer)
                                    {
                                        ip = ptr + ip->Operand;
                                        continue;
                                    }

                                }
                                break;
                            case OpCodeREnum.Leave_S:
                                {
                                    if (method.ExceptionHandler != null)
                                    {
                                        ExceptionHandler eh = null;

                                        int addr = ip->Operand;
                                        var sql = from e in method.ExceptionHandler
                                                  where addr == e.HandlerEnd + 1 && e.HandlerType == ExceptionHandlerType.Finally || e.HandlerType == ExceptionHandlerType.Fault
                                                  select e;
                                        eh = sql.FirstOrDefault();
                                        if (eh != null)
                                        {
                                            finallyEndAddress = ip->Operand;
                                            ip = ptr + eh.HandlerStart;
                                            continue;
                                        }
                                    }
                                    ip = ptr + ip->Operand;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Call:
                            case OpCodeREnum.Callvirt:
                                {
                                    IMethod m = domain.GetMethod(ip->Operand);
                                    if (m == null)
                                    {
                                        //Irrelevant method
                                        int cnt = (int)ip->OperandLong;
                                        //Balance the stack
                                        for (int i = 0; i < cnt; i++)
                                        {
                                            Free(esp - 1);
                                            esp--;
                                        }
                                    }
                                    else
                                    {
                                        if (m is ILMethod)
                                        {
                                            ILMethod ilm = (ILMethod)m;
                                            bool processed = false;
                                            if (m.IsDelegateInvoke)
                                            {
                                                var instance = StackObject.ToObject((Minus(esp, m.ParameterCount + 1)), domain, mStack);
                                                if (instance is IDelegateAdapter)
                                                {
                                                    esp = ((IDelegateAdapter)instance).ILInvoke(this, esp, mStack);
                                                    processed = true;
                                                }
                                            }
                                            if (!processed)
                                            {
                                                if (code == OpCodeREnum.Callvirt)
                                                {
                                                    objRef = GetObjectAndResolveReference(Minus(esp, ilm.ParameterCount + 1));
                                                    if (objRef->ObjectType == ObjectTypes.Null)
                                                        throw new NullReferenceException();
                                                    if (objRef->ObjectType == ObjectTypes.ValueTypeObjectReference)
                                                    {
                                                        StackObject* dst = ILIntepreter.ResolveReference(objRef);
                                                        var ft = domain.GetType(dst->Value) as ILType;
                                                        ilm = ft.GetVirtualMethod(ilm) as ILMethod;
                                                    }
                                                    else
                                                    {
                                                        var obj = mStack[objRef->Value];
                                                        if (obj == null)
                                                            throw new NullReferenceException();
                                                        ilm = ((ILTypeInstance)obj).Type.GetVirtualMethod(ilm) as ILMethod;
                                                    }
                                                }
                                                esp = Execute(ilm, esp, out unhandledException);
                                                ValueTypeBasePointer = bp;
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
                                                var instance = StackObject.ToObject((Minus(esp, cm.ParameterCount + 1)), domain, mStack);
                                                if (instance is IDelegateAdapter)
                                                {
                                                    esp = ((IDelegateAdapter)instance).ILInvoke(this, esp, mStack);
                                                    processed = true;
                                                }
                                            }

                                            if (!processed)
                                            {
                                                var redirect = cm.Redirection;
                                                if (redirect != null)
                                                    esp = redirect(this, esp, mStack, cm, false);
                                                else
                                                {
#if DEBUG && !DISABLE_ILRUNTIME_DEBUG
                                                    if (!allowUnboundCLRMethod)
                                                        throw new NotSupportedException(cm.ToString() + " is not bound!");
#endif
#if UNITY_EDITOR
                                                    if (System.Threading.Thread.CurrentThread.ManagedThreadId == AppDomain.UnityMainThreadID)

#if UNITY_5_5_OR_NEWER
                                                        UnityEngine.Profiling.Profiler.BeginSample(cm.ToString());
#else
                                                        UnityEngine.Profiler.BeginSample(cm.ToString());
#endif
#endif
                                                    object result = cm.Invoke(this, esp, mStack);
#if UNITY_EDITOR
                                                    if (System.Threading.Thread.CurrentThread.ManagedThreadId == AppDomain.UnityMainThreadID)
#if UNITY_5_5_OR_NEWER
                                                        UnityEngine.Profiling.Profiler.EndSample();
#else
                                                        UnityEngine.Profiler.EndSample();
#endif

#endif
                                                    if (result is CrossBindingAdaptorType)
                                                        result = ((CrossBindingAdaptorType)result).ILInstance;
                                                    int paramCount = cm.ParameterCount;
                                                    for (int i = 1; i <= paramCount; i++)
                                                    {
                                                        Free(Minus(esp, i));
                                                    }
                                                    esp = Minus(esp, paramCount);
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

                                        if (m.ReturnType != AppDomain.VoidType)
                                        {
                                            esp = PopToRegister(ref info, ip->Register1, esp);
                                        }
                                    }
                                }
                                break;
                            #endregion

                            #region FieldOperation
                            case OpCodeREnum.Stfld:
                                {
                                    reg2 = Add(r, ip->Register2); 
                                    objRef = GetObjectAndResolveReference(Add(r, ip->Register1));
                                    if (objRef->ObjectType == ObjectTypes.ValueTypeObjectReference)
                                    {
                                        StackObject* dst = ILIntepreter.ResolveReference(objRef);
                                        var ft = domain.GetType(dst->Value);
                                        if (ft is ILType)
                                            CopyToValueTypeField(dst, (int)ip->OperandLong, reg2, mStack);
                                        else
                                            CopyToValueTypeField(dst, ((CLRType)ft).FieldIndexMapping[(int)ip->OperandLong], reg2, mStack);
                                    }
                                    else
                                    {
                                        object obj = RetriveObject(objRef, mStack);

                                        if (obj != null)
                                        {
                                            if (obj is ILTypeInstance)
                                            {
                                                ILTypeInstance instance = obj as ILTypeInstance;
                                                instance.AssignFromStack((int)ip->OperandLong, reg2, AppDomain, mStack);
                                            }
                                            else
                                            {
                                                var t = obj.GetType();
                                                var type = AppDomain.GetType((int)(ip->OperandLong >> 32));
                                                if (type != null)
                                                {
                                                    var fieldToken = (int)ip->OperandLong;
                                                    var f = ((CLRType)type).GetField(fieldToken);
                                                    ((CLRType)type).SetFieldValue(fieldToken, ref obj, f.FieldType.CheckCLRTypes(CheckAndCloneValueType(StackObject.ToObject(reg2, domain, mStack), domain)));
                                                    //Writeback
                                                    if (t.IsValueType)
                                                    {
                                                        switch (objRef->ObjectType)
                                                        {
                                                            case ObjectTypes.Object:
                                                                break;
                                                            case ObjectTypes.FieldReference:
                                                                {
                                                                    var oldObj = mStack[objRef->Value];
                                                                    int idx = objRef->ValueLow;
                                                                    if (oldObj is ILTypeInstance)
                                                                    {
                                                                        ((ILTypeInstance)oldObj)[idx] = obj;
                                                                    }
                                                                    else
                                                                    {
                                                                        var it = AppDomain.GetType(oldObj.GetType());
                                                                        ((CLRType)it).SetFieldValue(idx, ref oldObj, obj);
                                                                    }
                                                                }
                                                                break;
                                                            case ObjectTypes.StaticFieldReference:
                                                                {
                                                                    var it = AppDomain.GetType(objRef->Value);
                                                                    int idx = objRef->ValueLow;
                                                                    if (it is ILType)
                                                                    {
                                                                        ((ILType)it).StaticInstance[idx] = obj;
                                                                    }
                                                                    else
                                                                    {
                                                                        ((CLRType)it).SetStaticFieldValue(idx, obj);
                                                                    }
                                                                }
                                                                break;
                                                            case ObjectTypes.ValueTypeObjectReference:
                                                                {
                                                                    var dst = ILIntepreter.ResolveReference(objRef);
                                                                    var ct = domain.GetType(dst->Value) as CLRType;
                                                                    var binder = ct.ValueTypeBinder;
                                                                    binder.CopyValueTypeToStack(obj, dst, mStack);
                                                                }
                                                                break;
                                                            case ObjectTypes.ArrayReference:
                                                                {
                                                                    var arr = mStack[objRef->Value] as Array;
                                                                    var idx = objRef->ValueLow;
                                                                    arr.SetValue(obj, idx);
                                                                }
                                                                break;
                                                            default:
                                                                throw new NotImplementedException();
                                                        }
                                                    }
                                                }
                                                else
                                                    throw new TypeLoadException();
                                            }
                                        }
                                        else
                                            throw new NullReferenceException();
                                    }
                                }
                                break;
                            case OpCodeREnum.Ldfld:
                                {
                                    reg1 = Add(r, ip->Register1);
                                    reg2 = Add(r, ip->Register2);
                                    objRef = GetObjectAndResolveReference(reg2);
                                    if (objRef->ObjectType == ObjectTypes.ValueTypeObjectReference)
                                    {
                                        var dst = ILIntepreter.ResolveReference(objRef);
                                        var ft = domain.GetType(dst->Value);
                                        if (ft is ILType)
                                            dst = Minus(dst, (int)ip->OperandLong + 1);
                                        else
                                            dst = Minus(dst, ((CLRType)ft).FieldIndexMapping[(int)ip->OperandLong] + 1);

                                        CopyToStack(reg1, dst, mStack);
                                        //CopyToRegister(ref info, ip->Register1, dst);
                                    }
                                    else
                                    {
                                        object obj = RetriveObject(objRef, mStack);
                                        if (obj != null)
                                        {
                                            if (obj is ILTypeInstance)
                                            {
                                                ILTypeInstance instance = obj as ILTypeInstance;
                                                instance.CopyToRegister((int)ip->OperandLong, ref info, ip->Register1);
                                            }
                                            else
                                            {
                                                //var t = obj.GetType();
                                                var type = AppDomain.GetType((int)(ip->OperandLong >> 32));
                                                if (type != null)
                                                {
                                                    var token = (int)ip->OperandLong;
                                                    var ft = ((CLRType)type).GetField(token);
                                                    var val = ((CLRType)type).GetFieldValue(token, obj);
                                                    if (val is CrossBindingAdaptorType)
                                                        val = ((CrossBindingAdaptorType)val).ILInstance;
                                                    AssignToRegister(ref info, ip->Register1, val, ft.FieldType == typeof(object));
                                                }
                                                else
                                                    throw new TypeLoadException();
                                            }
                                        }
                                        else
                                            throw new NullReferenceException();
                                    }
                                }
                                break;
                            case OpCodeREnum.Ldflda:
                                {
                                    reg1 = Add(r, ip->Register1);
                                    reg2 = Add(r, ip->Register2);
                                    objRef = GetObjectAndResolveReference(reg2);
                                    if (objRef->ObjectType == ObjectTypes.ValueTypeObjectReference)
                                    {
                                        var ft = domain.GetType((int)(ip->OperandLong >> 32));
                                        StackObject* fieldAddr;
                                        if (ft is ILType)
                                        {
                                            fieldAddr = Minus(ILIntepreter.ResolveReference(objRef), (int)ip->OperandLong + 1);
                                        }
                                        else
                                        {
                                            fieldAddr = Minus(ILIntepreter.ResolveReference(objRef), ((CLRType)ft).FieldIndexMapping[(int)ip->OperandLong] + 1);
                                        }
                                        reg1->ObjectType = ObjectTypes.StackObjectReference;
                                        *(long*)&reg1->Value = (long)fieldAddr;
                                    }
                                    else
                                    {
                                        object obj = RetriveObject(objRef, mStack);
                                        if (obj != null)
                                        {
                                            AssignToRegister(ref info, ip->Register1, obj);
                                            reg1->ObjectType = ObjectTypes.FieldReference;
                                            reg1->ValueLow = (int)ip->OperandLong;
                                        }
                                        else
                                            throw new NullReferenceException();
                                    }
                                }
                                break;
                            case OpCodeREnum.Stsfld:
                                {
                                    IType type = AppDomain.GetType((int)(ip->OperandLong >> 32));
                                    if (type != null)
                                    {
                                        reg1 = Add(r, ip->Register1);
                                        if (type is ILType)
                                        {
                                            ILType t = type as ILType;
                                            t.StaticInstance.AssignFromStack((int)ip->OperandLong, reg1, AppDomain, mStack);
                                        }
                                        else
                                        {
                                            CLRType t = type as CLRType;
                                            int idx = (int)ip->OperandLong;
                                            var f = t.GetField(idx);
                                            t.SetStaticFieldValue(idx, f.FieldType.CheckCLRTypes(CheckAndCloneValueType(StackObject.ToObject(reg1, domain, mStack), domain)));
                                        }
                                    }
                                    else
                                        throw new TypeLoadException();
                                }
                                break;
                            case OpCodeREnum.Ldsfld:
                                {
                                    IType type = AppDomain.GetType((int)(ip->OperandLong >> 32));
                                    if (type != null)
                                    {
                                        if (type is ILType)
                                        {
                                            ILType t = type as ILType;
                                            t.StaticInstance.CopyToRegister((int)ip->OperandLong,ref info, ip->Register1);
                                        }
                                        else
                                        {
                                            CLRType t = type as CLRType;
                                            int idx = (int)ip->OperandLong;
                                            var f = t.GetField(idx);
                                            var val = t.GetFieldValue(idx, null);
                                            if (val is CrossBindingAdaptorType)
                                                val = ((CrossBindingAdaptorType)val).ILInstance;
                                            AssignToRegister(ref info, ip->Register1, val, f.FieldType == typeof(object));
                                        }
                                    }
                                    else
                                        throw new TypeLoadException();
                                }
                                break;
                            case OpCodeREnum.Ldsflda:
                                {
                                    reg1 = Add(r, ip->Register1);
                                    reg1->ObjectType = ObjectTypes.StaticFieldReference;
                                    reg1->Value = (int)(ip->OperandLong >> 32); 
                                    reg1->ValueLow = (int)(ip->OperandLong);
                                }
                                break;
                            #endregion

                            #region Initialization & Instantiation
                            case OpCodeREnum.Newobj:
                                {
                                    IMethod m = domain.GetMethod(ip->Operand);
                                    if (m is ILMethod)
                                    {
                                        ILType type = m.DeclearingType as ILType;
                                        if (type.IsDelegate)
                                        {
                                            objRef = GetObjectAndResolveReference(esp - 1 - 1);
                                            var mi = (IMethod)mStack[(esp - 1)->Value];
                                            object ins;
                                            if (objRef->ObjectType == ObjectTypes.Null)
                                                ins = null;
                                            else
                                                ins = mStack[objRef->Value];
                                            Free(esp - 1);
                                            Free(esp - 1 - 1);
                                            esp = esp - 1 - 1;
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
                                            var a = esp - m.ParameterCount;
                                            ILTypeInstance obj = null;
                                            bool isValueType = type.IsValueType;
                                            if (isValueType)
                                            {
                                                stack.AllocValueType(esp, type);
                                                objRef = esp + 1;
                                                objRef->ObjectType = ObjectTypes.StackObjectReference;
                                                *(long*)&objRef->Value = (long)esp;
                                                objRef++;
                                            }
                                            else
                                            {
                                                obj = type.Instantiate(false);
                                                objRef = PushObject(esp, mStack, obj);//this parameter for constructor
                                            }
                                            esp = objRef;
                                            for (int i = 0; i < m.ParameterCount; i++)
                                            {
                                                CopyToStack(esp, a + i, mStack);
                                                esp++;
                                            }
                                            esp = Execute((ILMethod)m, esp, out unhandledException);
                                            ValueTypeBasePointer = bp;
                                            if (isValueType)
                                            {
                                                var ins = objRef - 1 - 1;
                                                *a = *ins;
                                                esp = a + 1;
                                            }
                                            else
                                            {
                                                //PushToRegister(ref info, ip->Register1, obj);
                                                esp = PushObject(a, mStack, obj);//new constructedObj
                                            }
                                        }
                                        if (unhandledException)
                                            returned = true;
                                    }
                                    else
                                    {
                                        CLRMethod cm = (CLRMethod)m;
                                        //Means new object();
                                        if (cm == null)
                                        {
                                            esp = PushObject(esp, mStack, new object());
                                        }
                                        else
                                        {
                                            if (cm.DeclearingType.IsDelegate)
                                            {
                                                objRef = GetObjectAndResolveReference(esp - 1 - 1);
                                                var mi = (IMethod)mStack[(esp - 1)->Value];
                                                object ins;
                                                if (objRef->ObjectType == ObjectTypes.Null)
                                                    ins = null;
                                                else
                                                    ins = mStack[objRef->Value];
                                                Free(esp - 1);
                                                Free(esp - 1 - 1);
                                                esp = esp - 1 - 1;
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
                                                    if (ins is ILTypeInstance)
                                                        ins = ((ILTypeInstance)ins).CLRInstance;
                                                    dele = Delegate.CreateDelegate(cm.DeclearingType.TypeForCLR, ins, ((CLRMethod)mi).MethodInfo);
                                                }
                                                esp = PushObject(esp, mStack, dele);
                                            }
                                            else
                                            {
                                                var redirect = cm.Redirection;
                                                if (redirect != null)
                                                    esp = redirect(this, esp, mStack, cm, true);
                                                else
                                                {
                                                    object result = cm.Invoke(this, esp, mStack, true);
                                                    int paramCount = cm.ParameterCount;
                                                    for (int i = 1; i <= paramCount; i++)
                                                    {
                                                        Free(esp - i);
                                                    }
                                                    esp = Minus(esp, paramCount);
                                                    esp = PushObject(esp, mStack, result);//new constructedObj
                                                }
                                            }
                                        }
                                    }
                                    esp = PopToRegister(ref info, ip->Register1, esp);
                                }
                                break;
                            case OpCodeREnum.Constrained:
                                {
                                    var type = domain.GetType(ip->Operand);
                                    var m = domain.GetMethod(ip->Operand2);
                                    var pCnt = m.ParameterCount;
                                    objRef = Minus(esp, pCnt + 1);
                                    var insIdx = mStack.Count;
                                    if (objRef->ObjectType < ObjectTypes.Object)
                                    {
                                        bool moved = false;
                                        //move parameters
                                        for (int i = 0; i < pCnt; i++)
                                        {
                                            var pPtr = Minus(esp, i + 1);
                                            if (pPtr->ObjectType >= ObjectTypes.Object)
                                            {
                                                var oldVal = pPtr->Value;
                                                insIdx--;
                                                if (!moved)
                                                {
                                                    pPtr->Value = mStack.Count;
                                                    mStack.Add(mStack[oldVal]);
                                                    mStack[oldVal] = null;
                                                    moved = true;
                                                }
                                                else
                                                {
                                                    mStack[oldVal + 1] = mStack[oldVal];
                                                    mStack[oldVal] = null;
                                                    pPtr->Value = oldVal + 1;
                                                }
                                            }
                                        }
                                        if (!moved)
                                        {
                                            mStack.Add(null);
                                        }
                                    }
                                    else
                                        insIdx = objRef->Value;
                                    var objRef2 = GetObjectAndResolveReference(objRef);
                                    if (type != null)
                                    {
                                        if (type is ILType)
                                        {
                                            var t = (ILType)type;
                                            if (t.IsEnum)
                                            {
                                                ILEnumTypeInstance ins = new ILEnumTypeInstance(t);
                                                switch (objRef2->ObjectType)
                                                {
                                                    case ObjectTypes.FieldReference:
                                                        {
                                                            var owner = mStack[objRef2->Value] as ILTypeInstance;
                                                            int idx = objRef2->ValueLow;
                                                            //Free(objRef);
                                                            owner.PushToStack(idx, objRef, AppDomain, mStack);
                                                            ins.AssignFromStack(0, objRef, AppDomain, mStack);
                                                            ins.Boxed = true;
                                                        }
                                                        break;
                                                    case ObjectTypes.StaticFieldReference:
                                                        {
                                                            var st = AppDomain.GetType(objRef2->Value) as ILType;
                                                            int idx = objRef2->ValueLow;
                                                            //Free(objRef);
                                                            st.StaticInstance.PushToStack(idx, objRef, AppDomain, mStack);
                                                            ins.AssignFromStack(0, objRef, AppDomain, mStack);
                                                            ins.Boxed = true;
                                                        }
                                                        break;
                                                    case ObjectTypes.ArrayReference:
                                                        {
                                                            var arr = mStack[objRef2->Value];
                                                            var idx = objRef2->ValueLow;
                                                            //Free(objRef);
                                                            LoadFromArrayReference(arr, idx, objRef, t, mStack);
                                                            ins.AssignFromStack(0, objRef, AppDomain, mStack);
                                                            ins.Boxed = true;
                                                        }
                                                        break;
                                                    default:
                                                        ins.AssignFromStack(0, objRef2, AppDomain, mStack);
                                                        ins.Boxed = true;
                                                        break;
                                                }
                                                objRef->ObjectType = ObjectTypes.Object;
                                                objRef->Value = insIdx;
                                                mStack[insIdx] = ins;

                                                //esp = PushObject(esp - 1, mStack, ins);
                                            }
                                            else
                                            {
                                                object res = RetriveObject(objRef2, mStack);
                                                //Free(objRef);
                                                objRef->ObjectType = ObjectTypes.Object;
                                                objRef->Value = insIdx;
                                                mStack[insIdx] = res;
                                                //esp = PushObject(objRef, mStack, res, true);
                                            }
                                        }
                                        else
                                        {
                                            var tt = type.TypeForCLR;
                                            if (tt.IsEnum)
                                            {
                                                mStack[insIdx] = Enum.ToObject(tt, StackObject.ToObject(objRef2, AppDomain, mStack));
                                                objRef->ObjectType = ObjectTypes.Object;
                                                objRef->Value = insIdx;
                                                //esp = PushObject(esp - 1, mStack, Enum.ToObject(tt, StackObject.ToObject(obj, AppDomain, mStack)), true);
                                            }
                                            else if (tt.IsPrimitive)
                                            {
                                                mStack[insIdx] = tt.CheckCLRTypes(StackObject.ToObject(objRef2, AppDomain, mStack));
                                                objRef->ObjectType = ObjectTypes.Object;
                                                objRef->Value = insIdx;
                                                //esp = PushObject(esp - 1, mStack, tt.CheckCLRTypes(StackObject.ToObject(obj, AppDomain, mStack)));
                                            }
                                            else
                                            {
                                                object res = RetriveObject(objRef2, mStack);
                                                //Free(objRef);
                                                objRef->ObjectType = ObjectTypes.Object;
                                                objRef->Value = insIdx;
                                                mStack[insIdx] = res;
                                                //esp = PushObject(objRef, mStack, res, true);
                                            }
                                        }
                                    }
                                    else
                                        throw new NullReferenceException();
                                }
                                break;
                            case OpCodeREnum.Box:
                                {
                                    reg1 = Add(r, ip->Register1);
                                    reg2 = Add(r, ip->Register2);
                                    var type = domain.GetType(ip->Operand);
                                    if (type != null)
                                    {
                                        if (type is ILType)
                                        {
                                            if (((ILType)type).IsEnum)
                                            {
                                                ILEnumTypeInstance ins = new Intepreter.ILEnumTypeInstance((ILType)type);
                                                ins.AssignFromStack(0, reg2, AppDomain, mStack);
                                                ins.Boxed = true;
                                                PushObject(reg1, mStack, ins, true);
                                            }
                                            else
                                            {
                                                switch (reg2->ObjectType)
                                                {
                                                    case ObjectTypes.Null:
                                                        break;
                                                    case ObjectTypes.ValueTypeObjectReference:
                                                        {
                                                            ILTypeInstance ins = ((ILType)type).Instantiate(false);
                                                            ins.AssignFromStack(reg2, domain, mStack);
                                                            //FreeStackValueType(objRef);
                                                            PushObject(reg1, mStack, ins, true);
                                                        }
                                                        break;
                                                    default:
                                                        {
                                                            var obj = mStack[reg2->Value];
                                                            //Free(objRef);
                                                            if (type.IsArray)
                                                            {
                                                                PushObject(reg1, mStack, obj, true);
                                                            }
                                                            else
                                                            {
                                                                ILTypeInstance ins = (ILTypeInstance)obj;
                                                                if (ins != null)
                                                                {
                                                                    if (ins.IsValueType)
                                                                    {
                                                                        ins.Boxed = true;
                                                                    }
                                                                    PushObject(reg1, mStack, ins, true);
                                                                }
                                                                else
                                                                {
                                                                    PushNull(reg1);
                                                                }
                                                            }
                                                        }
                                                        break;
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
                                                    switch (reg2->ObjectType)
                                                    {
                                                        case ObjectTypes.Integer:
                                                            PushObject(reg1, mStack, reg2->Value, true);
                                                            break;
                                                        case ObjectTypes.Null:
                                                            PushObject(reg1, mStack, 0, true);
                                                            break;
                                                        case ObjectTypes.Object:
                                                            break;
                                                        default:
                                                            throw new NotImplementedException();
                                                    }
                                                }
                                                else if (t == typeof(bool))
                                                {
                                                    switch (reg2->ObjectType)
                                                    {
                                                        case ObjectTypes.Integer:
                                                            PushObject(reg1, mStack, (reg2->Value == 1), true);
                                                            break;
                                                        case ObjectTypes.Null:
                                                            PushObject(reg1, mStack, false, true);
                                                            break;
                                                        case ObjectTypes.Object:
                                                            break;
                                                        default:
                                                            throw new NotImplementedException();
                                                    }
                                                }
                                                else if (t == typeof(byte))
                                                {
                                                    switch (reg2->ObjectType)
                                                    {
                                                        case ObjectTypes.Integer:
                                                            PushObject(reg1, mStack, (byte)reg2->Value, true);
                                                            break;
                                                        case ObjectTypes.Null:
                                                            PushObject(reg1, mStack, 0L, true);
                                                            break;
                                                        case ObjectTypes.Object:
                                                            break;
                                                        default:
                                                            throw new NotImplementedException();
                                                    }
                                                }
                                                else if (t == typeof(short))
                                                {
                                                    switch (reg2->ObjectType)
                                                    {
                                                        case ObjectTypes.Integer:
                                                            PushObject(reg1, mStack, (short)reg2->Value, true);
                                                            break;
                                                        case ObjectTypes.Null:
                                                            PushObject(reg1, mStack, (short)0, true);
                                                            break;
                                                        case ObjectTypes.Object:
                                                            break;
                                                        default:
                                                            throw new NotImplementedException();
                                                    }
                                                }
                                                else if (t == typeof(long))
                                                {
                                                    switch (reg2->ObjectType)
                                                    {
                                                        case ObjectTypes.Long:
                                                            PushObject(reg1, mStack, *(long*)&reg2->Value, true);
                                                            break;
                                                        case ObjectTypes.Null:
                                                            PushObject(reg1, mStack, 0L, true);
                                                            break;
                                                        case ObjectTypes.Object:
                                                            break;
                                                        default:
                                                            throw new NotImplementedException();
                                                    }
                                                }
                                                else if (t == typeof(float))
                                                {
                                                    switch (reg2->ObjectType)
                                                    {
                                                        case ObjectTypes.Float:
                                                            PushObject(reg1, mStack, *(float*)&reg2->Value, true);
                                                            break;
                                                        case ObjectTypes.Null:
                                                            PushObject(reg1, mStack, 0f, true);
                                                            break;
                                                        case ObjectTypes.Object:
                                                            break;
                                                        default:
                                                            throw new NotImplementedException();
                                                    }
                                                }
                                                else if (t == typeof(double))
                                                {
                                                    switch (reg2->ObjectType)
                                                    {
                                                        case ObjectTypes.Double:
                                                            PushObject(reg1, mStack, *(double*)&reg2->Value, true);
                                                            break;
                                                        case ObjectTypes.Null:
                                                            PushObject(reg1, mStack, 0.0, true);
                                                            break;
                                                        case ObjectTypes.Object:
                                                            break;
                                                        default:
                                                            throw new NotImplementedException();
                                                    }
                                                }
                                                else if (t == typeof(char))
                                                {
                                                    switch (reg2->ObjectType)
                                                    {
                                                        case ObjectTypes.Integer:
                                                            PushObject(reg1, mStack, (char)reg2->Value, true);
                                                            break;
                                                        case ObjectTypes.Object:
                                                            break;
                                                        default:
                                                            throw new NotImplementedException();
                                                    }
                                                }
                                                else if (t == typeof(uint))
                                                {
                                                    switch (reg2->ObjectType)
                                                    {
                                                        case ObjectTypes.Integer:
                                                            PushObject(reg1, mStack, (uint)reg2->Value, true);
                                                            break;
                                                        case ObjectTypes.Null:
                                                            PushObject(reg1, mStack, (uint)0, true);
                                                            break;
                                                        case ObjectTypes.Object:
                                                            break;
                                                        default:
                                                            throw new NotImplementedException();
                                                    }
                                                }
                                                else if (t == typeof(ushort))
                                                {
                                                    switch (reg2->ObjectType)
                                                    {
                                                        case ObjectTypes.Integer:
                                                            PushObject(reg1, mStack, (ushort)reg2->Value, true);
                                                            break;
                                                        case ObjectTypes.Null:
                                                            PushObject(reg1, mStack, (ushort)0, true);
                                                            break;
                                                        case ObjectTypes.Object:
                                                            break;
                                                        default:
                                                            throw new NotImplementedException();
                                                    }
                                                }
                                                else if (t == typeof(ulong))
                                                {
                                                    switch (reg2->ObjectType)
                                                    {
                                                        case ObjectTypes.Long:
                                                            PushObject(reg1, mStack, *(ulong*)&reg2->Value, true);
                                                            break;
                                                        case ObjectTypes.Null:
                                                            PushObject(reg1, mStack, (ulong)0, true);
                                                            break;
                                                        case ObjectTypes.Object:
                                                            break;
                                                        default:
                                                            throw new NotImplementedException();
                                                    }
                                                }
                                                else if (t == typeof(sbyte))
                                                {
                                                    switch (reg2->ObjectType)
                                                    {
                                                        case ObjectTypes.Integer:
                                                            PushObject(reg1, mStack, (sbyte)reg2->Value, true);
                                                            break;
                                                        case ObjectTypes.Null:
                                                            PushObject(reg1, mStack, (sbyte)0, true);
                                                            break;
                                                        case ObjectTypes.Object:
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
                                                PushObject(reg1, mStack, Enum.ToObject(type.TypeForCLR, StackObject.ToObject(reg2, AppDomain, mStack)), true);
                                            }
                                            else
                                            {
                                                if (reg2->ObjectType == ObjectTypes.ValueTypeObjectReference)
                                                {
                                                    var dst = ILIntepreter.ResolveReference(reg2);
                                                    var vt = domain.GetType(dst->Value);
                                                    if (vt != type)
                                                        throw new InvalidCastException();
                                                    object ins = ((CLRType)vt).ValueTypeBinder.ToObject(dst, mStack);
                                                    //FreeStackValueType(objRef);
                                                    PushObject(reg1, mStack, ins, true);
                                                }
                                                else
                                                {
                                                    CopyToStack(reg1, reg2, mStack);
                                                }
                                                //nothing to do for CLR type boxing
                                            }
                                        }
                                    }
                                    else
                                        throw new NullReferenceException();
                                }
                                break;
                            case OpCodeREnum.Unbox:
                            case OpCodeREnum.Unbox_Any:
                                {
                                    objRef = Add(r, ip->Register2);
                                    if (objRef->ObjectType == ObjectTypes.Object)
                                    {
                                        object obj = mStack[objRef->Value];
                                        if (obj != null)
                                        {
                                            var t = domain.GetType(ip->Operand);
                                            if (t != null)
                                            {
                                                var type = t.TypeForCLR;
                                                bool isEnumObj = obj is ILEnumTypeInstance;
                                                if ((t is CLRType) && type.IsPrimitive && !isEnumObj)
                                                {
                                                    reg1 = Add(r, ip->Register1);
                                                    if (type == typeof(int))
                                                    {
                                                        int val = obj.ToInt32();
                                                        reg1->ObjectType = ObjectTypes.Integer;
                                                        reg1->Value = val;
                                                    }
                                                    else if (type == typeof(bool))
                                                    {
                                                        bool val = (bool)obj;
                                                        reg1->ObjectType = ObjectTypes.Integer;
                                                        reg1->Value = val ? 1 : 0;
                                                    }
                                                    else if (type == typeof(short))
                                                    {
                                                        short val = obj.ToInt16();
                                                        reg1->ObjectType = ObjectTypes.Integer;
                                                        reg1->Value = val;
                                                    }
                                                    else if (type == typeof(long))
                                                    {
                                                        long val = obj.ToInt64();
                                                        reg1->ObjectType = ObjectTypes.Long;
                                                        *(long*)&reg1->Value = val;
                                                    }
                                                    else if (type == typeof(float))
                                                    {
                                                        float val = obj.ToFloat();
                                                        reg1->ObjectType = ObjectTypes.Float;
                                                        *(float*)&reg1->Value = val;
                                                    }
                                                    else if (type == typeof(byte))
                                                    {
                                                        byte val = (byte)obj;
                                                        reg1->ObjectType = ObjectTypes.Integer;
                                                        reg1->Value = val;
                                                    }
                                                    else if (type == typeof(double))
                                                    {
                                                        double val = obj.ToDouble();
                                                        reg1->ObjectType = ObjectTypes.Double;
                                                        *(double*)&reg1->Value = val;
                                                    }
                                                    else if (type == typeof(char))
                                                    {
                                                        char val = (char)obj;
                                                        reg1->ObjectType = ObjectTypes.Integer;
                                                        *(char*)&reg1->Value = val;
                                                    }
                                                    else if (type == typeof(uint))
                                                    {
                                                        uint val = (uint)obj;
                                                        reg1->ObjectType = ObjectTypes.Integer;
                                                        reg1->Value = (int)val;
                                                    }
                                                    else if (type == typeof(ushort))
                                                    {
                                                        ushort val = (ushort)obj;
                                                        reg1->ObjectType = ObjectTypes.Integer;
                                                        reg1->Value = val;
                                                    }
                                                    else if (type == typeof(ulong))
                                                    {
                                                        ulong val = (ulong)obj;
                                                        reg1->ObjectType = ObjectTypes.Long;
                                                        *(ulong*)&reg1->Value = val;
                                                    }
                                                    else if (type == typeof(sbyte))
                                                    {
                                                        sbyte val = (sbyte)obj;
                                                        reg1->ObjectType = ObjectTypes.Integer;
                                                        reg1->Value = val;
                                                    }
                                                    else
                                                        throw new NotImplementedException();
                                                }
                                                else if (t.IsValueType)
                                                {
                                                    if (obj is ILTypeInstance)
                                                    {
                                                        var res = ((ILTypeInstance)obj);
                                                        if (res is ILEnumTypeInstance)
                                                        {
                                                            res.CopyToRegister(0, ref info, ip->Register1);
                                                        }
                                                        else
                                                        {
                                                            if (res.Boxed)
                                                            {
                                                                res = res.Clone();
                                                                res.Boxed = false;
                                                            }
                                                            AssignToRegister(ref info, ip->Register1, res);
                                                        }
                                                    }
                                                    else
                                                        AssignToRegister(ref info, ip->Register1, obj);

                                                }
                                                else
                                                {
                                                    AssignToRegister(ref info, ip->Register1, obj);
                                                }
                                            }
                                            else
                                                throw new TypeLoadException();
                                        }
                                        else
                                            throw new NullReferenceException();
                                    }
                                    else if (objRef->ObjectType < ObjectTypes.StackObjectReference)
                                    {
                                        //Nothing to do with primitive types
                                    }
                                    else
                                        throw new InvalidCastException();
                                }
                                break;
                            case OpCodeREnum.Initobj:
                                {
                                    objRef = Add(r, ip->Register2);
                                    objRef = GetObjectAndResolveReference(objRef);
                                    reg2 = Add(r, ip->Register1);
                                    var type = domain.GetType(ip->Operand);
                                    if (type is ILType)
                                    {
                                        ILType it = (ILType)type;
                                        if (it.IsValueType)
                                        {
                                            switch (objRef->ObjectType)
                                            {
                                                case ObjectTypes.Null:
                                                    throw new NullReferenceException();
                                                case ObjectTypes.ValueTypeObjectReference:
                                                    stack.ClearValueTypeObject(type, ILIntepreter.ResolveReference(objRef));
                                                    break;
                                                case ObjectTypes.Object:
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
                                                    break;
                                                case ObjectTypes.ArrayReference:
                                                    {
                                                        var arr = mStack[objRef->Value] as Array;
                                                        var idx = objRef->ValueLow;
                                                        var obj = arr.GetValue(idx);
                                                        if (obj == null)
                                                            arr.SetValue(it.Instantiate(), idx);
                                                        else
                                                        {
                                                            if (obj is ILTypeInstance)
                                                            {
                                                                ILTypeInstance instance = obj as ILTypeInstance;
                                                                instance.Clear();
                                                            }
                                                            else
                                                                throw new NotImplementedException();
                                                        }
                                                    }
                                                    break;
                                                case ObjectTypes.FieldReference:
                                                    {
                                                        var obj = mStack[objRef->Value];
                                                        if (obj != null)
                                                        {
                                                            if (obj is ILTypeInstance)
                                                            {
                                                                ILTypeInstance instance = obj as ILTypeInstance;
                                                                var tar = instance[objRef->ValueLow] as ILTypeInstance;
                                                                if (tar != null)
                                                                    tar.Clear();
                                                                else
                                                                    throw new NotSupportedException();
                                                            }
                                                            else
                                                                throw new NotSupportedException();
                                                        }
                                                        else
                                                            throw new NullReferenceException();
                                                    }
                                                    break;
                                                case ObjectTypes.StaticFieldReference:
                                                    {
                                                        var t = AppDomain.GetType(objRef->Value);
                                                        int idx = objRef->ValueLow;
                                                        if (t is ILType)
                                                        {
                                                            var tar = ((ILType)t).StaticInstance[idx] as ILTypeInstance;
                                                            if (tar != null)
                                                                tar.Clear();
                                                            else
                                                                throw new NotSupportedException();
                                                        }
                                                        else
                                                            throw new NotSupportedException();
                                                    }
                                                    break;
                                                default:
                                                    throw new NotImplementedException();
                                            }
                                        }
                                        else
                                        {
                                            PushNull(reg2);
                                            switch (objRef->ObjectType)
                                            {
                                                case ObjectTypes.StaticFieldReference:
                                                    {
                                                        var t = AppDomain.GetType(objRef->Value) as ILType;
                                                        t.StaticInstance.AssignFromStack(objRef->ValueLow, reg2, AppDomain, mStack);
                                                    }
                                                    break;
                                                case ObjectTypes.FieldReference:
                                                    {
                                                        var instance = mStack[objRef->Value] as ILTypeInstance;
                                                        instance.AssignFromStack(objRef->ValueLow, reg2, AppDomain, mStack);
                                                    }
                                                    break;
                                                default:
                                                    {
                                                        if (objRef->ObjectType >= ObjectTypes.Object)
                                                            mStack[objRef->Value] = null;
                                                        else
                                                            PushNull(objRef);
                                                    }
                                                    break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (objRef->ObjectType == ObjectTypes.ValueTypeObjectReference)
                                        {
                                            stack.ClearValueTypeObject(type, ILIntepreter.ResolveReference(objRef));
                                        }
                                        else if (type.IsPrimitive)
                                            StackObject.Initialized(objRef, type);
                                    }
                                }
                                break;
                            case OpCodeREnum.Isinst:
                                {
                                    reg1 = Add(r, ip->Register1);
                                    reg2 = Add(r, ip->Register2);
                                    var type = domain.GetType(ip->Operand);
                                    if (type != null)
                                    {
                                        objRef = GetObjectAndResolveReference(reg2);
                                        if (objRef->ObjectType <= ObjectTypes.Double)
                                        {
                                            var tclr = type.TypeForCLR;
                                            switch (objRef->ObjectType)
                                            {
                                                case ObjectTypes.Integer:
                                                    {
                                                        if (tclr != typeof(int) && tclr != typeof(bool) && tclr != typeof(short) && tclr != typeof(byte) && tclr != typeof(ushort) && tclr != typeof(uint))
                                                        {
                                                            WriteNull(reg1);
                                                        }
                                                    }
                                                    break;
                                                case ObjectTypes.Long:
                                                    {
                                                        if (tclr != typeof(long) && tclr != typeof(ulong))
                                                        {
                                                            WriteNull(reg1);
                                                        }
                                                    }
                                                    break;
                                                case ObjectTypes.Float:
                                                    {
                                                        if (tclr != typeof(float))
                                                        {
                                                            WriteNull(reg1);
                                                        }
                                                    }
                                                    break;
                                                case ObjectTypes.Double:
                                                    {
                                                        if (tclr != typeof(double))
                                                        {
                                                            WriteNull(reg1);
                                                        }
                                                    }
                                                    break;
                                                case ObjectTypes.Null:
                                                    WriteNull(reg1);
                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            var obj = RetriveObject(objRef, mStack);

                                            if (obj != null)
                                            {
                                                if (obj is ILTypeInstance)
                                                {
                                                    if (((ILTypeInstance)obj).CanAssignTo(type))
                                                    {
                                                        AssignToRegister(ref info, ip->Register1, obj);
                                                    }
                                                    else
                                                    {
                                                        WriteNull(reg1);
                                                    }
                                                }
                                                else
                                                {
                                                    if (type.TypeForCLR.IsAssignableFrom(obj.GetType()))
                                                    {
                                                        AssignToRegister(ref info, ip->Register1, obj, true);
                                                    }
                                                    else
                                                    {
                                                        WriteNull(reg1);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                WriteNull(reg1);
                                            }
                                        }
                                    }
                                    else
                                        throw new NullReferenceException();
                                }
                                break;

                            case OpCodeREnum.Ldftn:
                                {
                                    IMethod m = domain.GetMethod(ip->Operand);
                                    AssignToRegister(ref info, ip->Register1, m);
                                }
                                break;
                            case OpCodeREnum.Ldvirtftn:
                                {
                                    IMethod m = domain.GetMethod(ip->Operand);
                                    objRef = Add(r, ip->Register2);
                                    if (m is ILMethod)
                                    {
                                        ILMethod ilm = (ILMethod)m;

                                        var obj = mStack[objRef->Value];
                                        m = ((ILTypeInstance)obj).Type.GetVirtualMethod(ilm) as ILMethod;
                                    }
                                    else
                                    {
                                        var obj = mStack[objRef->Value];
                                        if (obj is ILTypeInstance)
                                            m = ((ILTypeInstance)obj).Type.GetVirtualMethod(m);
                                        else if (obj is CrossBindingAdaptorType)
                                        {
                                            m = ((CrossBindingAdaptorType)obj).ILInstance.Type.BaseType.GetVirtualMethod(m);
                                        }
                                    }
                                    AssignToRegister(ref info, ip->Register1, m);
                                }
                                break;
                            #endregion

                            #region Compare
                            case OpCodeREnum.Ceq:
                                {
                                    reg1 = Add(r, ip->Register2);
                                    reg2 = Add(r, ip->Register3);
                                    reg3 = Add(r, ip->Register1);
                                    bool res = false;
                                    if (reg1->ObjectType == reg2->ObjectType)
                                    {
                                        switch (reg1->ObjectType)
                                        {
                                            case ObjectTypes.Integer:
                                            case ObjectTypes.Float:
                                                res = reg1->Value == reg2->Value;
                                                break;
                                            case ObjectTypes.Object:
                                                res = mStack[reg1->Value] == mStack[reg2->Value];
                                                break;
                                            case ObjectTypes.FieldReference:
                                                res = mStack[reg1->Value] == mStack[reg2->Value] && reg1->ValueLow == reg2->ValueLow;
                                                break;
                                            case ObjectTypes.Null:
                                                res = true;
                                                break;
                                            default:
                                                res = reg1->Value == reg2->Value && reg1->ValueLow == reg2->ValueLow;
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        switch (reg1->ObjectType)
                                        {
                                            case ObjectTypes.Object:
                                                res = mStack[reg1->Value] == null && reg2->ObjectType == ObjectTypes.Null;
                                                break;
                                            case ObjectTypes.Null:
                                                res = reg1->ObjectType == ObjectTypes.Object && mStack[reg2->Value] == null;
                                                break;
                                        }
                                    }
                                    if (res)
                                        WriteOne(reg3);
                                    else
                                        WriteZero(reg3);

                                }
                                break;
                            case OpCodeREnum.Clt:
                                {
                                    reg1 = Add(r, ip->Register2);
                                    reg2 = Add(r, ip->Register3);
                                    reg3 = Add(r, ip->Register1);
                                    bool res = false;
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            res = reg1->Value < reg2->Value;
                                            break;
                                        case ObjectTypes.Long:
                                            res = *(long*)&reg1->Value < *(long*)&reg2->Value;
                                            break;
                                        case ObjectTypes.Float:
                                            res = *(float*)&reg1->Value < *(float*)&reg2->Value;
                                            break;
                                        case ObjectTypes.Double:
                                            res = *(double*)&reg1->Value < *(double*)&reg2->Value;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    if (res)
                                        WriteOne(reg3);
                                    else
                                        WriteZero(reg3);
                                }
                                break;
                            case OpCodeREnum.Clt_Un:
                                {
                                    reg1 = Add(r, ip->Register2);
                                    reg2 = Add(r, ip->Register3);
                                    reg3 = Add(r, ip->Register1);
                                    bool res = false;
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            res = (uint)reg1->Value < (uint)reg2->Value && reg2->ObjectType != ObjectTypes.Null;
                                            break;
                                        case ObjectTypes.Long:
                                            res = (ulong)*(long*)&reg1->Value < (ulong)*(long*)&reg2->Value && reg2->ObjectType != ObjectTypes.Null;
                                            break;
                                        case ObjectTypes.Float:
                                            res = *(float*)&reg1->Value < *(float*)&reg2->Value && reg2->ObjectType != ObjectTypes.Null;
                                            break;
                                        case ObjectTypes.Double:
                                            res = *(double*)&reg1->Value < *(double*)&reg2->Value && reg2->ObjectType != ObjectTypes.Null;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    if (res)
                                        WriteOne(reg3);
                                    else
                                        WriteZero(reg3);
                                }
                                break;
                            case OpCodeREnum.Cgt:
                                {
                                    reg1 = Add(r, ip->Register2);
                                    reg2 = Add(r, ip->Register3);
                                    reg3 = Add(r, ip->Register1);
                                    bool res = false;
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            res = reg1->Value > reg2->Value || reg2->ObjectType == ObjectTypes.Null;
                                            break;
                                        case ObjectTypes.Long:
                                            res = *(long*)&reg1->Value > *(long*)&reg2->Value || reg2->ObjectType == ObjectTypes.Null;
                                            break;
                                        case ObjectTypes.Float:
                                            res = *(float*)&reg1->Value > *(float*)&reg2->Value || reg2->ObjectType == ObjectTypes.Null;
                                            break;
                                        case ObjectTypes.Double:
                                            res = *(double*)&reg1->Value > *(double*)&reg2->Value || reg2->ObjectType == ObjectTypes.Null;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    if (res)
                                        WriteOne(reg3);
                                    else
                                        WriteZero(reg3);
                                }
                                break;
                            case OpCodeREnum.Cgt_Un:
                                {
                                    reg1 = Add(r, ip->Register2);
                                    reg2 = Add(r, ip->Register3);
                                    reg3 = Add(r, ip->Register1);
                                    bool res = false;
                                    switch (reg1->ObjectType)
                                    {
                                        case ObjectTypes.Integer:
                                            res = ((uint)reg1->Value > (uint)reg2->Value) || reg2->ObjectType == ObjectTypes.Null;
                                            break;
                                        case ObjectTypes.Long:
                                            res = (ulong)*(long*)&reg1->Value > (ulong)*(long*)&reg2->Value || reg2->ObjectType == ObjectTypes.Null;
                                            break;
                                        case ObjectTypes.Float:
                                            res = *(float*)&reg1->Value > *(float*)&reg2->Value || reg2->ObjectType == ObjectTypes.Null;
                                            break;
                                        case ObjectTypes.Double:
                                            res = *(double*)&reg1->Value > *(double*)&reg2->Value || reg2->ObjectType == ObjectTypes.Null;
                                            break;
                                        case ObjectTypes.Object:
                                            res = mStack[reg1->Value] != null && reg2->ObjectType == ObjectTypes.Null;
                                            break;
                                        case ObjectTypes.Null:
                                            res = false;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    if (res)
                                        WriteOne(reg3);
                                    else
                                        WriteZero(reg3);
                                }
                                break;
                            #endregion

                            #region Array
                            case OpCodeREnum.Newarr:
                                {
                                    reg2 = Add(r, ip->Register2);
                                    var type = domain.GetType(ip->Operand);
                                    object arr = null;
                                    if (type != null)
                                    {
                                        if (type.TypeForCLR != typeof(ILTypeInstance))
                                        {
                                            if (type is CLRType)
                                            {
                                                arr = ((CLRType)type).CreateArrayInstance(reg2->Value);
                                            }
                                            else
                                            {
                                                arr = Array.CreateInstance(type.TypeForCLR, reg2->Value);
                                            }

                                            //Register Type
                                            AppDomain.GetType(arr.GetType());
                                        }
                                        else
                                        {
                                            arr = new ILTypeInstance[reg2->Value];
                                            ILTypeInstance[] ilArr = (ILTypeInstance[])arr;
                                            if (type.IsValueType)
                                            {
                                                for (int i = 0; i < reg2->Value; i++)
                                                {
                                                    ilArr[i] = ((ILType)type).Instantiate(true);
                                                }
                                            }
                                        }
                                    }
                                    AssignToRegister(ref info, ip->Register1, arr);
                                }
                                break;
                            case OpCodeREnum.Stelem_Ref:
                            case OpCodeREnum.Stelem_Any:
                                {
                                    reg1 = Add(r, ip->Register1);
                                    reg2 = Add(r, ip->Register2);
                                    reg3 = Add(r, ip->Register3);
                                    Array arr = mStack[reg1->Value] as Array;
                                    if (arr is object[])
                                    {
                                        switch (reg3->ObjectType)
                                        {
                                            case ObjectTypes.Null:
                                                arr.SetValue(null, reg2->Value);
                                                break;
                                            case ObjectTypes.Object:
                                                ArraySetValue(arr, mStack[reg3->Value], reg2->Value);
                                                break;
                                            case ObjectTypes.Integer:
                                                arr.SetValue(reg3->Value, reg2->Value);
                                                break;
                                            case ObjectTypes.Long:
                                                arr.SetValue(*(long*)&reg3->Value, reg2->Value);
                                                break;
                                            case ObjectTypes.Float:
                                                arr.SetValue(*(float*)&reg3->Value, reg2->Value);
                                                break;
                                            case ObjectTypes.Double:
                                                arr.SetValue(*(double*)&reg3->Value, reg2->Value);
                                                break;
                                            case ObjectTypes.ValueTypeObjectReference:
                                                ArraySetValue(arr, StackObject.ToObject(reg3, domain, mStack), reg2->Value);
                                                FreeStackValueType(esp - 1);
                                                break;
                                            default:
                                                throw new NotImplementedException();
                                        }
                                    }
                                    else
                                    {
                                        switch (reg3->ObjectType)
                                        {
                                            case ObjectTypes.Object:
                                                ArraySetValue(arr, mStack[reg3->Value], reg2->Value);
                                                break;
                                            case ObjectTypes.Integer:
                                                {
                                                    StoreIntValueToArray(arr, reg3, reg2);
                                                }
                                                break;
                                            case ObjectTypes.Long:
                                                {
                                                    if (arr is long[])
                                                    {
                                                        ((long[])arr)[reg2->Value] = *(long*)&reg3->Value;
                                                    }
                                                    else
                                                    {
                                                        ((ulong[])arr)[reg2->Value] = *(ulong*)&reg3->Value;
                                                    }
                                                }
                                                break;
                                            case ObjectTypes.Float:
                                                {
                                                    ((float[])arr)[reg2->Value] = *(float*)&reg3->Value;
                                                }
                                                break;
                                            case ObjectTypes.Double:
                                                {
                                                    ((double[])arr)[reg2->Value] = *(double*)&reg3->Value;
                                                }
                                                break;
                                            case ObjectTypes.ValueTypeObjectReference:
                                                ArraySetValue(arr, StackObject.ToObject(reg3, domain, mStack), reg2->Value);
                                                FreeStackValueType(esp - 1);
                                                break;
                                            default:
                                                throw new NotImplementedException();
                                        }
                                    }
                                }
                                break;
                            case OpCodeREnum.Stelem_I1:
                                {
                                    reg1 = Add(r, ip->Register1);
                                    reg2 = Add(r, ip->Register2);
                                    reg3 = Add(r, ip->Register3);

                                    byte[] arr = mStack[reg1->Value] as byte[];
                                    if (arr != null)
                                    {
                                        arr[reg2->Value] = (byte)reg3->Value;
                                    }
                                    else
                                    {
                                        bool[] arr2 = mStack[reg1->Value] as bool[];
                                        if (arr2 != null)
                                        {
                                            arr2[reg2->Value] = reg3->Value == 1;
                                        }
                                        else
                                        {
                                            sbyte[] arr3 = mStack[reg1->Value] as sbyte[];
                                            arr3[reg2->Value] = (sbyte)reg3->Value;
                                        }
                                    }
                                }
                                break;
                            case OpCodeREnum.Ldelem_I1:
                                {
                                    reg1 = Add(r, ip->Register2);
                                    reg2 = Add(r, ip->Register3);
                                    reg3 = Add(r, ip->Register1);
                                    int val = 0;
                                    bool[] arr = mStack[reg1->Value] as bool[];
                                    if (arr != null)
                                        val = arr[reg2->Value] ? 1 : 0;
                                    else
                                    {
                                        sbyte[] arr2 = mStack[reg1->Value] as sbyte[];
                                        val = arr2[reg2->Value];
                                    }

                                    reg3->ObjectType = ObjectTypes.Integer;
                                    reg3->Value = val;
                                }
                                break;
                            case OpCodeREnum.Ldelem_U1:
                                {
                                    reg1 = Add(r, ip->Register2);
                                    reg2 = Add(r, ip->Register3);
                                    reg3 = Add(r, ip->Register1);
                                    int val = 0;

                                    byte[] arr = mStack[reg1->Value] as byte[];
                                    if (arr != null)
                                        val = arr[reg2->Value];
                                    else
                                    {
                                        bool[] arr2 = mStack[reg1->Value] as bool[];
                                        val = arr2[reg2->Value] ? 1 : 0;
                                    }
                                    reg3->ObjectType = ObjectTypes.Integer;
                                    reg3->Value = val;
                                }
                                break;
                            case OpCodeREnum.Stelem_I2:
                                {
                                    reg1 = Add(r, ip->Register1);
                                    reg2 = Add(r, ip->Register2);
                                    reg3 = Add(r, ip->Register3);

                                    short[] arr = mStack[reg1->Value] as short[];
                                    if (arr != null)
                                    {
                                        arr[reg2->Value] = (short)reg3->Value;
                                    }
                                    else
                                    {
                                        ushort[] arr2 = mStack[reg1->Value] as ushort[];
                                        if (arr2 != null)
                                        {
                                            arr2[reg2->Value] = (ushort)reg3->Value;
                                        }
                                        else
                                        {
                                            char[] arr3 = mStack[reg1->Value] as char[];
                                            arr3[reg2->Value] = (char)reg3->Value;
                                        }
                                    }
                                }
                                break;
                            case OpCodeREnum.Ldelem_I2:
                                {
                                    reg1 = Add(r, ip->Register2);
                                    reg2 = Add(r, ip->Register3);
                                    reg3 = Add(r, ip->Register1);
                                    int val = 0;

                                    short[] arr = mStack[reg1->Value] as short[];
                                    if (arr != null)
                                    {
                                        val = arr[reg2->Value];
                                    }
                                    else
                                    {
                                        char[] arr2 = mStack[reg1->Value] as char[];
                                        val = arr2[reg2->Value];
                                    }
                                    reg3->ObjectType = ObjectTypes.Integer;
                                    reg3->Value = val;
                                }
                                break;
                            case OpCodeREnum.Ldelem_U2:
                                {
                                    reg1 = Add(r, ip->Register2);
                                    reg2 = Add(r, ip->Register3);
                                    reg3 = Add(r, ip->Register1);
                                    int val = 0;

                                    ushort[] arr = mStack[reg1->Value] as ushort[];
                                    if (arr != null)
                                    {
                                        val = arr[reg2->Value];
                                    }
                                    else
                                    {
                                        char[] arr2 = mStack[reg1->Value] as char[];
                                        val = arr2[reg2->Value];
                                    }
                                    reg3->ObjectType = ObjectTypes.Integer;
                                    reg3->Value = val;
                                }
                                break;
                            case OpCodeREnum.Stelem_I4:
                                {
                                    reg1 = Add(r, ip->Register1);
                                    reg2 = Add(r, ip->Register2);
                                    reg3 = Add(r, ip->Register3);

                                    int[] arr = mStack[reg1->Value] as int[];
                                    if (arr != null)
                                    {
                                        arr[reg2->Value] = reg3->Value;
                                    }
                                    else
                                    {
                                        uint[] arr2 = mStack[reg1->Value] as uint[];
                                        arr2[reg2->Value] = (uint)reg3->Value;
                                    }
                                }
                                break;
                            case OpCodeREnum.Ldelem_I4:
                                {
                                    reg1 = Add(r, ip->Register2);
                                    reg2 = Add(r, ip->Register3);
                                    reg3 = Add(r, ip->Register1);

                                    int[] arr = mStack[reg1->Value] as int[];
                                    reg3->ObjectType = ObjectTypes.Integer;
                                    reg3->Value = arr[reg2->Value];
                                }
                                break;
                            case OpCodeREnum.Ldelem_U4:
                                {
                                    reg1 = Add(r, ip->Register2);
                                    reg2 = Add(r, ip->Register3);
                                    reg3 = Add(r, ip->Register1);

                                    uint[] arr = mStack[reg1->Value] as uint[];

                                    reg3->ObjectType = ObjectTypes.Integer;
                                    reg3->Value = (int)arr[reg2->Value];
                                }
                                break;
                            case OpCodeREnum.Stelem_I8:
                                {
                                    reg1 = Add(r, ip->Register1);
                                    reg2 = Add(r, ip->Register2);
                                    reg3 = Add(r, ip->Register3);

                                    long[] arr = mStack[reg1->Value] as long[];
                                    if (arr != null)
                                    {
                                        arr[reg2->Value] = *(long*)&reg3->Value;
                                    }
                                    else
                                    {
                                        ulong[] arr2 = mStack[reg1->Value] as ulong[];
                                        arr2[reg2->Value] = *(ulong*)&reg3->Value;
                                    }
                                }
                                break;
                            case OpCodeREnum.Ldelem_I8:
                                {
                                    reg1 = Add(r, ip->Register2);
                                    reg2 = Add(r, ip->Register3);
                                    reg3 = Add(r, ip->Register1);

                                    long[] arr = mStack[reg1->Value] as long[];
                                    long longVal;
                                    if (arr != null)
                                        longVal = arr[reg2->Value];
                                    else
                                    {
                                        ulong[] arr2 = mStack[reg1->Value] as ulong[];
                                        longVal = (long)arr2[reg2->Value];
                                    }

                                    reg3->ObjectType = ObjectTypes.Long;
                                    *(long*)&reg3->Value = longVal;
                                }
                                break;
                            case OpCodeREnum.Stelem_R4:
                                {
                                    reg1 = Add(r, ip->Register1);
                                    reg2 = Add(r, ip->Register2);
                                    reg3 = Add(r, ip->Register3);

                                    float[] arr = mStack[reg1->Value] as float[];
                                    arr[reg2->Value] = *(float*)&reg3->Value;
                                }
                                break;
                            case OpCodeREnum.Ldelem_R4:
                                {
                                    reg1 = Add(r, ip->Register2);
                                    reg2 = Add(r, ip->Register3);
                                    reg3 = Add(r, ip->Register1);

                                    float[] arr = mStack[reg1->Value] as float[];

                                    reg3->ObjectType = ObjectTypes.Float;
                                    *(float*)&reg3->Value = arr[reg2->Value];
                                }
                                break;
                            case OpCodeREnum.Stelem_R8:
                                {
                                    reg1 = Add(r, ip->Register1);
                                    reg2 = Add(r, ip->Register2);
                                    reg3 = Add(r, ip->Register3);

                                    double[] arr = mStack[reg1->Value] as double[];
                                    arr[reg2->Value] = *(double*)&reg3->Value;
                                }
                                break;
                            case OpCodeREnum.Ldelem_R8:
                                {
                                    reg1 = Add(r, ip->Register2);
                                    reg2 = Add(r, ip->Register3);
                                    reg3 = Add(r, ip->Register1);

                                    double[] arr = mStack[reg1->Value] as double[];

                                    reg3->ObjectType = ObjectTypes.Double;
                                    *(double*)&reg3->Value = arr[reg2->Value];
                                }
                                break;
                            case OpCodeREnum.Ldelem_Ref:
                            case OpCodeREnum.Ldelem_Any:
                                {
                                    reg1 = Add(r, ip->Register2);
                                    reg2 = Add(r, ip->Register3);
                                    reg3 = Add(r, ip->Register1);

                                    Array arr = mStack[reg1->Value] as Array;
                                    var obj = arr.GetValue(reg2->Value);
                                    if (obj is CrossBindingAdaptorType)
                                        obj = ((CrossBindingAdaptorType)obj).ILInstance;

                                    if (obj is ILTypeInstance)
                                    {
                                        ILTypeInstance ins = (ILTypeInstance)obj;
                                        //Mod LiYu 2019.07.04
                                        if (ins.Type != null && ins.Type.IsValueType && !ins.Boxed)
                                        //if (ins.Type.IsValueType && !ins.Boxed)
                                        {
                                            AllocValueType(reg3, ins.Type);
                                            var dst = ILIntepreter.ResolveReference(reg3);
                                            ins.CopyValueTypeToStack(dst, mStack);
                                        }
                                        else
                                            PushObject(reg3, mStack, obj, true);
                                    }
                                    else
                                        PushObject(reg3, mStack, obj, !arr.GetType().GetElementType().IsPrimitive);
                                }
                                break;
                            case OpCodeREnum.Ldlen:
                                {
                                    reg1 = Add(r, ip->Register1);
                                    reg2 = Add(r, ip->Register2);
                                    Array arr = mStack[reg2->Value] as Array;

                                    reg1->ObjectType = ObjectTypes.Integer;
                                    reg1->Value = arr.Length;
                                }
                                break;
                            case OpCodeREnum.Ldelema:
                                {
                                    reg1 = Add(r, ip->Register2);
                                    reg2 = Add(r, ip->Register3);
                                    reg3 = Add(r, ip->Register1);
                                    var idx = reg2->Value;
                                    Array arr = mStack[reg1->Value] as Array;
                                    reg3->ObjectType = ObjectTypes.ArrayReference;
                                    reg3->Value = mStack.Count;
                                    mStack.Add(arr);
                                    reg3->ValueLow = reg2->Value;
                                }
                                break;
                            #endregion

                            case OpCodeREnum.Throw:
                                {
                                    var obj = GetObjectAndResolveReference(Add(r, ip->Register1));
                                    var ex = mStack[obj->Value] as Exception;
                                    throw ex;
                                }
                            case OpCodeREnum.Nop:
                                break;
                            default:
                                throw new NotSupportedException("Not supported opcode " + code);
                        }
                        ip++;
                    }
                    catch (Exception ex)
                    {
                        if (method.ExceptionHandler != null)
                        {
                            int addr = (int)(ip - ptr);
                            var eh = GetCorrespondingExceptionHandler(method, ex, addr, ExceptionHandlerType.Catch, true);

                            if (eh == null)
                            {
                                eh = GetCorrespondingExceptionHandler(method, ex, addr, ExceptionHandlerType.Catch, false);
                            }
                            if (eh != null)
                            {
                                if (ex is ILRuntimeException)
                                {
                                    ILRuntimeException ire = (ILRuntimeException)ex;
                                    var inner = ire.InnerException;
                                    inner.Data["ThisInfo"] = ire.ThisInfo;
                                    inner.Data["StackTrace"] = ire.StackTrace;
                                    inner.Data["LocalInfo"] = ire.LocalInfo;
                                    ex = inner;
                                }
                                else
                                {
                                    var debugger = AppDomain.DebugService;
                                    if (method.HasThis)
                                        ex.Data["ThisInfo"] = debugger.GetThisInfo(this);
                                    else
                                        ex.Data["ThisInfo"] = "";
                                    ex.Data["StackTrace"] = debugger.GetStackTrace(this);
                                    ex.Data["LocalInfo"] = debugger.GetLocalVariableInfo(this);
                                }
                                //Clear call stack
                                while (stack.Frames.Peek().BasePointer != frame.BasePointer)
                                {
                                    var f = stack.Frames.Peek();
                                    esp = stack.PopFrame(ref f, esp);
                                    if (f.Method.ReturnType != AppDomain.VoidType)
                                    {
                                        Free(esp - 1);
                                        esp--;
                                    }
                                }
                                esp = PushObject(esp, mStack, ex);
                                unhandledException = false;
                                ip = ptr + eh.HandlerStart;
                                continue;
                            }
                        }
                        if (unhandledException)
                        {
                            throw ex;
                        }

                        unhandledException = true;
                        returned = true;
#if DEBUG && !DISABLE_ILRUNTIME_DEBUG
                        if (!AppDomain.DebugService.Break(this, ex))
#endif
                        {
                            var newEx = new ILRuntimeException(ex.Message, this, method, ex);
                            throw newEx;
                        }
                    }
                }
            }

#if UNITY_EDITOR
            if (System.Threading.Thread.CurrentThread.ManagedThreadId == AppDomain.UnityMainThreadID)
#if UNITY_5_5_OR_NEWER
                UnityEngine.Profiling.Profiler.EndSample();
#else
                UnityEngine.Profiler.EndSample();
#endif
#endif
            //ClearStack
            return stack.PopFrame(ref frame, esp);
        }

        internal void CopyToRegister(ref RegisterFrameInfo info, short reg, StackObject* val, IList<object> mStackSrc = null)
        {
            var argCnt = info.ParameterCount;
            var mStack = info.ManagedStack;
            var locCnt = info.LocalCount;
            if (mStackSrc == null)
                mStackSrc = mStack;

            if (reg < argCnt)
                throw new NotImplementedException();
            var v = Add(info.RegisterStart, reg);
            var idx = info.LocalManagedBase + (reg - argCnt);
                
            switch (val->ObjectType)
            {
                case ObjectTypes.Null:
                    v->ObjectType = ObjectTypes.Object;
                    v->Value = idx;
                    mStack[idx] = null;
                    break;
                case ObjectTypes.Object:
                case ObjectTypes.FieldReference:
                case ObjectTypes.ArrayReference:
                    if (v->ObjectType == ObjectTypes.ValueTypeObjectReference)
                    {
                        var obj = mStackSrc[val->Value];
                        if (obj is ILTypeInstance)
                        {
                            var dst = ILIntepreter.ResolveReference(v);
                            ((ILTypeInstance)obj).CopyValueTypeToStack(dst, mStack);
                        }
                        else
                        {
                            var dst = ILIntepreter.ResolveReference(v);
                            var ct = domain.GetType(dst->Value) as CLRType;
                            var binder = ct.ValueTypeBinder;
                            binder.CopyValueTypeToStack(obj, dst, mStack);
                        }
                    }
                    else
                    {
                        *v = *val;
                        mStack[idx] = CheckAndCloneValueType(mStackSrc[v->Value], domain);
                        v->Value = idx;
                    }
                    break;
                case ObjectTypes.ValueTypeObjectReference:
                    CloneStackValueType(val, v, mStack);
                    break;
                default:
                    *v = *val;
                    mStack[idx] = null;
                    break;
            }
        }

        internal static void AssignToRegister(ref RegisterFrameInfo info, short reg, object obj, bool isBox = false)
        {
            var argCnt = info.ParameterCount;
            var mStack = info.ManagedStack;
            var locCnt = info.LocalCount;

            if (reg < argCnt)
                throw new NotImplementedException();
            var dst = Add(info.RegisterStart, reg);
            if (obj != null)
            {
                var idx = info.LocalManagedBase + (reg - argCnt);

                if (!isBox)
                {
                    var typeFlags = obj.GetType().GetTypeFlags();

                    if ((typeFlags & CLR.Utils.Extensions.TypeFlags.IsPrimitive) != 0)
                    {
                        UnboxObject(dst, obj, mStack);
                    }
                    else if ((typeFlags & CLR.Utils.Extensions.TypeFlags.IsEnum) != 0)
                    {
                        dst->ObjectType = ObjectTypes.Integer;
                        dst->Value = Convert.ToInt32(obj);
                    }
                    else
                    {
                        dst->ObjectType = ObjectTypes.Object;
                        dst->Value = idx;
                        mStack[idx] = obj;
                    }
                }
                else
                {
                    dst->ObjectType = ObjectTypes.Object;
                    dst->Value = idx;
                    mStack[idx] = obj;
                }
            }
            else
            {
                WriteNull(dst);
            }
        }

        StackObject* PopToRegister(ref RegisterFrameInfo info, short reg, StackObject* esp)
        {
            var val = esp - 1;
            var v = Add(info.RegisterStart, reg);
            CopyToStack(v, val, info.ManagedStack);
            //CopyToRegister(ref info, reg, val);
            //Free(val);
            return val;
        }

        public static void WriteOne(StackObject* esp)
        {
            esp->ObjectType = ObjectTypes.Integer;
            esp->Value = 1;
        }

        public static void WriteZero(StackObject* esp)
        {
            esp->ObjectType = ObjectTypes.Integer;
            esp->Value = 0;
        }

        public static void WriteNull(StackObject* esp)
        {
            esp->ObjectType = ObjectTypes.Null;
            esp->Value = -1;
            esp->ValueLow = 0;
        }
    }
}
