using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ILRuntime.CLR.Method;

namespace ILRuntime.CLR.TypeSystem
{
    public interface IType
    {
        bool IsGenericInstance { get; }
        Type TypeForCLR { get; }

        string FullName { get; }

        IMethod GetMethod(string name, int paramCount);

        IMethod GetMethod(string name, List<IType> param);

        List<IMethod> GetMethods();

        IMethod GetConstructor(List<IType> param);
    }
}
