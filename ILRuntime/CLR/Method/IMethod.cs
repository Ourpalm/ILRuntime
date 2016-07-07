using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ILRuntime.CLR.TypeSystem;
namespace ILRuntime.CLR.Method
{
    public interface IMethod
    {
        string Name { get; }
        int ParameterCount { get; }

        bool HasThis { get; }

        IType DeclearingType { get; }

        IType ReturnType { get; }
        List<IType> Parameters { get; }

        bool IsConstructor { get; }
    }
}
