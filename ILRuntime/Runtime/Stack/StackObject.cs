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
                    StackObject tmp = this;
                    return *(long*)&tmp.Value;
                case ObjectTypes.Object:
                    return mStack[Value];
                case ObjectTypes.FieldReference:
                    {
                        ILTypeInstance instance = mStack[Value] as ILTypeInstance;
                        return instance.Fields[ValueLow].ToObject(instance.ManagedObjects);
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
        Object,
        FieldReference,
    }
}
