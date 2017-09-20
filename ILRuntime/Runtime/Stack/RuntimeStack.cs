using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Other;
using ILRuntime.Runtime.Intepreter;

namespace ILRuntime.Runtime.Stack
{
    unsafe class RuntimeStack : IDisposable
    {
        ILIntepreter intepreter;
        StackObject* pointer;
        StackObject* endOfMemory;
        StackObject* valueTypePtr;

        IntPtr nativePointer;

#if DEBUG
        IList<object> managedStack = new List<object>(32);
#else
        IList<object> managedStack = new UncheckedList<object>(32);
#endif

        Stack<StackFrame> frames = new Stack<StackFrame>();
        const int MAXIMAL_STACK_OBJECTS = 1024 * 16;

        public Stack<StackFrame> Frames { get { return frames; } }
        public RuntimeStack(ILIntepreter intepreter)
        {
            this.intepreter = intepreter;

            nativePointer = System.Runtime.InteropServices.Marshal.AllocHGlobal(sizeof(StackObject) * MAXIMAL_STACK_OBJECTS);
            pointer = (StackObject*)nativePointer.ToPointer();
            endOfMemory = Add(pointer, MAXIMAL_STACK_OBJECTS);
            valueTypePtr = endOfMemory - 1;
        }

        ~RuntimeStack()
        {
            Dispose();
        }

        public StackObject* StackBase
        {
            get
            {
                return pointer;
            }
        }

        public StackObject* ValueTypeStackPointer
        {
            get
            {
                return valueTypePtr;
            }
        }

        public IList<object> ManagedStack { get { return managedStack; } }

        public void InitializeFrame(ILMethod method, StackObject* esp, out StackFrame res)
        {
            if (esp < pointer || esp >= endOfMemory)
                throw new StackOverflowException();
            if (frames.Count > 0 && frames.Peek().BasePointer > esp)
                throw new StackOverflowException();
            res = new StackFrame();
            res.LocalVarPointer = esp;
            res.Method = method;
#if DEBUG
            res.Address = new IntegerReference();
            for (int i = 0; i < method.LocalVariableCount; i++)
            {
                var p = Add(esp, i);
                p->ObjectType = ObjectTypes.Null;
            }
#endif
            res.BasePointer = method.LocalVariableCount > 0 ? Add(esp, method.LocalVariableCount + 1) : esp;
            res.ManagedStackBase = managedStack.Count;
            res.ValueTypeBasePointer = valueTypePtr;
            //frames.Push(res);
        }
        public void PushFrame(ref StackFrame frame)
        {
            frames.Push(frame);
        }

        public StackObject* PopFrame(ref StackFrame frame, StackObject* esp)
        {
            if (frames.Count > 0 && frames.Peek().BasePointer == frame.BasePointer)
                frames.Pop();
            else
                throw new NotSupportedException();
            StackObject* returnVal = esp - 1;
            var method = frame.Method;
            StackObject* ret = ILIntepreter.Minus(frame.LocalVarPointer, method.ParameterCount);
            int mStackBase = frame.ManagedStackBase;
            if (method.HasThis)
                ret--;
            if(method.ReturnType != intepreter.AppDomain.VoidType)
            {
                *ret = *returnVal;
                if(ret->ObjectType == ObjectTypes.Object)
                {
                    ret->Value = mStackBase;
                    managedStack[mStackBase] = managedStack[returnVal->Value];
                    mStackBase++;
                }
                else if(ret->ObjectType == ObjectTypes.ValueTypeObjectReference)
                {
                    StackObject* oriAddr = frame.ValueTypeBasePointer;
                    RelocateValueType(ret, ref frame.ValueTypeBasePointer, ref mStackBase);
                    *(StackObject**)&ret->Value = oriAddr;
                }
                ret++;
            }
#if DEBUG
            ((List<object>)managedStack).RemoveRange(mStackBase, managedStack.Count - mStackBase);
#else
            ((UncheckedList<object>)managedStack).RemoveRange(mStackBase, managedStack.Count - mStackBase);
#endif
            valueTypePtr = frame.ValueTypeBasePointer;
            return ret;
        }

        void RelocateValueType(StackObject* src, ref StackObject* dst, ref int mStackBase)
        {
            StackObject* descriptor = *(StackObject**)&src->Value;
            if (descriptor > dst)
                throw new StackOverflowException();
            *dst = *descriptor;
            int cnt = descriptor->ValueLow;
            StackObject* endAddr = ILIntepreter.Minus(dst, cnt + 1);
            for(int i = 0; i < cnt; i++)
            {
                StackObject* addr = ILIntepreter.Minus(descriptor, i + 1);
                StackObject* tarVal = ILIntepreter.Minus(dst, i + 1);
                *tarVal = *addr;
                switch (addr->ObjectType)
                {
                    case ObjectTypes.Object:
                    case ObjectTypes.ArrayReference:
                    case ObjectTypes.FieldReference:
                        if (tarVal->Value >= mStackBase)
                        {
                            tarVal->Value = mStackBase;
                            managedStack[mStackBase] = managedStack[addr->Value];
                            mStackBase++;
                        }
                        break;
                    case ObjectTypes.ValueTypeObjectReference:
                        RelocateValueType(addr, ref endAddr, ref mStackBase);
                        break;
                }
            }
            dst = endAddr;
        }

        public void AllocValueType(StackObject* ptr, IType type)
        {
            if (type.IsValueType)
            {
                int fieldCount = 0;
                if(type is ILType)
                {
                    fieldCount = ((ILType)type).TotalFieldCount;
                }
                else
                {
                    fieldCount = ((CLRType)type).TotalFieldCount;
                }
                ptr->ObjectType = ObjectTypes.ValueTypeObjectReference;
                var dst = valueTypePtr;
                *(StackObject**)&ptr->Value = dst;
                dst->ObjectType = ObjectTypes.ValueTypeDescriptor;
                dst->Value = type.GetHashCode();
                dst->ValueLow = fieldCount;
                valueTypePtr = ILIntepreter.Minus(valueTypePtr, fieldCount + 1);
                if (valueTypePtr <= StackBase)
                    throw new StackOverflowException();
                InitializeValueTypeObject(type, dst);
            }
            else
                throw new ArgumentException(type.FullName + " is not a value type.", "type");
        }

        void InitializeValueTypeObject(IType type, StackObject* ptr)
        {
            if (type is ILType)
            {
                ILType t = (ILType)type;
                for (int i = 0; i < t.FieldTypes.Length; i++)
                {
                    var ft = t.FieldTypes[i];
                    StackObject* val = ILIntepreter.Minus(ptr, t.FieldStartIndex + i + 1);
                    var tClr = ft.TypeForCLR;
                    if (tClr.IsPrimitive)
                        StackObject.Initialized(val, tClr);
                    else
                    {
                        if (ft.IsValueType)
                        {
                            AllocValueType(val, ft);
                        }
                        else
                        {
                            val->ObjectType = ObjectTypes.Object;
                            val->Value = managedStack.Count;
                            managedStack.Add(null);
                        }
                    }
                }
                if (type.BaseType != null && type.BaseType is ILType)
                    InitializeValueTypeObject((ILType)type.BaseType, ptr);
            }
            else
            {
                CLRType t = (CLRType)type;
                var cnt = t.TotalFieldCount;
                for(int i = 0; i < cnt; i++)
                {
                    var ft = t.Fields[t.FieldIndexReverseMapping[i]].FieldType;
                    StackObject* val = ILIntepreter.Minus(ptr, i + 1);
                    if (ft.IsPrimitive)
                        StackObject.Initialized(val, ft);
                    else
                    {
                        if (ft.IsValueType)
                        {
                            var it = intepreter.AppDomain.GetType(ft);
                            if (((CLRType)it).ValueTypeBinder != null)
                                AllocValueType(val, it);
                            else
                            {
                                throw new NotSupportedException();
                            }
                        }
                        else
                        {
                            val->ObjectType = ObjectTypes.Object;
                            val->Value = managedStack.Count;
                            managedStack.Add(null);
                        }
                    }
                }
            }
        }
        
        public void Dispose()
        {
            if (nativePointer != IntPtr.Zero)
            {
                System.Runtime.InteropServices.Marshal.FreeHGlobal(nativePointer);
                nativePointer = IntPtr.Zero;
            }
        }

        StackObject* Add(StackObject* a, int b)
        {
            return (StackObject*)((long)a + sizeof(StackObject) * b);
        }
    }
}
