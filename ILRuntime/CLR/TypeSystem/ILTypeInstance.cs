using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ILRuntime.Runtime.Stack;
namespace ILRuntime.CLR.TypeSystem
{
    class ILTypeInstance
    {
        ILType type;
        StackObject[] fields;
        public ILTypeInstance(ILType type)
        {
            this.type = type;            
        }
    }
}
