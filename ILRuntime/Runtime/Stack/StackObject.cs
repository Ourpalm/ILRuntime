using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public unsafe object ToObject(List<object> mStack)
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
                        return instance.Fields[ValueLow].ToObject(instance.ManagedObjects);
                    }
                case ObjectTypes.StackObjectReference:
                    {
                        StackObject tmp = this;
                        return (*(StackObject**)&tmp.Value)->ToObject(mStack);
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
        Object,
        FieldReference,//Value = objIdx, ValueLow = fieldIdx
    }
}
