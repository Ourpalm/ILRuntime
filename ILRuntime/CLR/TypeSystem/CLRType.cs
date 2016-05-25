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

        public string FullName
        {
            get
            {
                return clrType.FullName;
            }
        }

        public IMethod GetMethod(string name, int paramCount)
        {
            throw new NotImplementedException();
        }

        public IMethod GetMethod(string name, List<IType> param)
        {
            throw new NotImplementedException();
        }
    }
}
