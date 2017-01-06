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

        /// <summary>
        /// CLR说表示的类型
        /// </summary>
        Type TypeForCLR { get; }
        Type ReflectionType { get; }

        IType BaseType { get; }

        IType ByRefType { get; }

        IType ArrayType { get; }

        string FullName { get; }

        string Name { get; }

        bool IsValueType { get; }

        bool IsDelegate { get; }

        bool HasGenericParameter { get; }

        ILRuntime.Runtime.Enviorment.AppDomain AppDomain { get; }

        /// <summary>
        /// 根据函数名和参数数量返回函数体
        /// </summary>
        /// <param name="name">函数名</param>
        /// <param name="paramCount">参数数量</param>
        /// <returns></returns>
        IMethod GetMethod(string name, int paramCount);

        /// <summary>
        /// 根据函数名，参数类型，泛型类型，返回值类型 返回指定函数
        /// </summary>
        /// <param name="name">函数名</param>
        /// <param name="param">参数类型</param>
        /// <param name="genericArguments">泛型类型</param>
        /// <param name="returnType">返回值类型</param>
        /// <returns></returns>
        IMethod GetMethod(string name, List<IType> param, IType[] genericArguments, IType returnType = null);


        IMethod GetVirtualMethod(IMethod method);

        List<IMethod> GetMethods();

        int GetFieldIndex(object token);

        IMethod GetConstructor(List<IType> param);

        bool CanAssignTo(IType type);

        IType MakeGenericInstance(KeyValuePair<string, IType>[] genericArguments);

        IType MakeByRefType();

        IType MakeArrayType();
        IType FindGenericArgument(string key);

        IType ResolveGenericType(IType contextType);
    }
}
