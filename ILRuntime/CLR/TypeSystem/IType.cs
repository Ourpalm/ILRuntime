using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ILRuntime.CLR.Method;

namespace ILRuntime.CLR.TypeSystem
{
    public interface IType
    {
        bool IsGenericInstance { get; }
        KeyValuePair<string, IType>[] GenericArguments { get; }
        Type TypeForCLR { get; }

        string FullName { get; }

        ILRuntime.Runtime.Enviorment.AppDomain AppDomain { get; }

        IMethod GetMethod(string name, int paramCount);

        IMethod GetMethod(string name, List<IType> param, IType[] genericArguments);

        List<IMethod> GetMethods();

        int GetFieldIndex(object token);

        IMethod GetConstructor(List<IType> param);

        bool CanAssignTo(IType type);

        IType MakeGenericInstance(KeyValuePair<string, IType>[] genericArguments);
        IType FindGenericArgument(string key);

        IType ResolveGenericType(IType contextType);
    }
}
