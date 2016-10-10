using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ILRuntime.CLR.TypeSystem;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
namespace ILRuntime.Runtime.Stack
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct StackObject
    {
        public static StackObject Null = new StackObject() { ObjectType = ObjectTypes.Null, Value = -1, ValueLow = 0 };
        public ObjectTypes ObjectType;
        public int Value;
        public int ValueLow;

        public unsafe object ToObject(ILRuntime.Runtime.Enviorment.AppDomain appdomain, List<object> mStack)
        {
            switch (ObjectType)
            {
                case ObjectTypes.Integer:
                    return Value;
                case ObjectTypes.Long:
                    {
                        StackObject tmp = this;
                        return *(long*)&tmp.Value;
                    }
                case ObjectTypes.Float:
                    {
                        StackObject tmp = this;
                        return *(float*)&tmp.Value;
                    }
                case ObjectTypes.Double:
                    {
                        StackObject tmp = this;
                        return *(double*)&tmp.Value;
                    }
                case ObjectTypes.Object:
                    return mStack[Value];
                case ObjectTypes.FieldReference:
                    {
                        ILTypeInstance instance = mStack[Value] as ILTypeInstance;
                        if (instance != null)
                        {
                            return instance.Fields[ValueLow].ToObject(appdomain, instance.ManagedObjects);
                        }
                        else
                        {
                            var obj = mStack[Value];
                            IType t = null;
                            if (obj is CrossBindingAdaptorType)
                            {
                                t = appdomain.GetType(((CrossBindingAdaptor)((CrossBindingAdaptorType)obj).ILInstance.Type.BaseType).BaseCLRType);
                            }
                            else
                                t = appdomain.GetType(obj.GetType());
                            var fi = ((CLRType)t).Fields[ValueLow];
                            return fi.GetValue(obj);
                        }
                    }
                case ObjectTypes.ArrayReference:
                    {
                        Array instance = mStack[Value] as Array;
                        return instance.GetValue(ValueLow);
                    }
                case ObjectTypes.StaticFieldReference:
                    {
                        var t = appdomain.GetType(Value);
                        if (t is CLR.TypeSystem.ILType)
                        {
                            CLR.TypeSystem.ILType type = (CLR.TypeSystem.ILType)t;
                            return type.StaticInstance.Fields[ValueLow].ToObject(appdomain, type.StaticInstance.ManagedObjects);
                        }
                        else
                        {
                            CLR.TypeSystem.CLRType type = (CLR.TypeSystem.CLRType)t;
                            var fi = type.Fields[ValueLow];
                            return fi.GetValue(null);
                        }
                    }
                case ObjectTypes.StackObjectReference:
                    {
                        StackObject tmp = this;
                        return (*(StackObject**)&tmp.Value)->ToObject(appdomain, mStack);
                    }
                case ObjectTypes.Null:
                    return null;
                default:
                    throw new NotImplementedException();
            }
        }

        public void Initialized(Type t)
        {
            if (t.IsPrimitive)
            {
                if (t == typeof(int) || t == typeof(uint) || t == typeof(short) || t == typeof(ushort) || t == typeof(byte) || t == typeof(sbyte) || t == typeof(char) || t == typeof(bool))
                {
                    ObjectType = ObjectTypes.Integer;
                    Value = 0;
                    ValueLow = 0;
                }
                else if (t == typeof(long) || t == typeof(ulong))
                {
                    ObjectType = ObjectTypes.Long;
                    Value = 0;
                    ValueLow = 0;
                }
                else if (t == typeof(float))
                {
                    ObjectType = ObjectTypes.Float;
                    Value = 0;
                    ValueLow = 0;
                }
                else if (t == typeof(double))
                {
                    ObjectType = ObjectTypes.Double;
                    Value = 0;
                    ValueLow = 0;
                }
                else
                    throw new NotImplementedException();
            }
            else
            {
                this = Null;
            }
        }
    }

    public enum ObjectTypes
    {
        Null,
        Integer,
        Long,
        Float,
        Double,
        StackObjectReference,//Value = pointer, 
        StaticFieldReference,
        Object,
        FieldReference,//Value = objIdx, ValueLow = fieldIdx
        ArrayReference,//Value = objIdx, ValueLow = elemIdx
    }
}
