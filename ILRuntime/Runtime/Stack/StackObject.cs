using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ILRuntime.Runtime.Stack
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    struct StackObject
    {
        public ObjectTypes ObjectType;
        public int Value;
        public int ValueLow;
    }

    enum ObjectTypes
    {
        Integer,
        Long,
        Float,
        Double,
        Object
    }
}
