using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ILRuntime.Runtime.Intepreter;
namespace ILRuntime.Runtime.Stack
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    struct StackObject
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
                        return instance.Fields[ValueLow].ToObject(appdomain, instance.ManagedObjects);
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
                            throw new NotImplementedException();
                    }
                case ObjectTypes.StackObjectReference:
                    {
                        StackObject tmp = this;
                        return (*(StackObject**)&tmp.Value)->ToObject(appdomain, mStack);
                    }
                case ObjectTypes.Null:
                    return "null";
                default:
                    throw new NotImplementedException();
            }
        }
    }

    enum ObjectTypes
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
    }
}
