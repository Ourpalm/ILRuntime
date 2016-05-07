using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ILRuntime.CLR.Method;
namespace ILRuntime.CLR.TypeSystem
{
    class CLRType : IType
    {
        Type clrType;

        public CLRType(Type clrType)
        {
            this.clrType = clrType;
        }
        
        public bool IsGenericInstance
        {
            get
            {
                return false;
            }
        }

        public Type TypeForCLR
        {
            get
            {
                return clrType;
            }
        }

        public IMethod GetMethod(string name, int paramCount)
        {
            throw new NotImplementedException();
        }
    }
}
