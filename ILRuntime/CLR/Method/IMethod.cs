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
        int LocalVariableCount { get; }
        int ParameterCount { get; }
        List<IType> Parameters { get; }
    }
}
