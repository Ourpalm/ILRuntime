using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Runtime.Intepreter;

namespace ILRuntime.Runtime.Enviorment
{
    /// <summary>
    /// This interface is used for inheritance and implementation of CLR Types or interfaces
    /// </summary>
    public abstract class CrossBindingAdaptor : IType
    {
        /// <summary>
        /// This returns the CLR type to be inherited or CLR interface to be implemented
        /// </summary>
        public abstract Type BaseCLRType { get; }

        public abstract object CreateCLRInstance(ILTypeInstance instance);

        public IMethod GetMethod(string name, int paramCount)
        {
            throw new NotImplementedException();
        }

        public IMethod GetMethod(string name, List<IType> param, IType[] genericArguments)
        {
            throw new NotImplementedException();
        }

        public List<IMethod> GetMethods()
        {
            throw new NotImplementedException();
        }

        public int GetFieldIndex(object token)
        {
            throw new NotImplementedException();
        }

        public IMethod GetConstructor(List<IType> param)
        {
            throw new NotImplementedException();
        }

        public bool CanAssignTo(IType type)
        {
            throw new NotImplementedException();
        }

        public IType MakeGenericInstance(KeyValuePair<string, IType>[] genericArguments)
        {
            throw new NotImplementedException();
        }

        public IType MakeByRefType()
        {
            throw new NotImplementedException();
        }

        public IType MakeArrayType()
        {
            throw new NotImplementedException();
        }

        public IType FindGenericArgument(string key)
        {
            throw new NotImplementedException();
        }

        public IType ResolveGenericType(IType contextType)
        {
            throw new NotImplementedException();
        }

        ILTypeInstance ILInstance { get; }

        public bool IsGenericInstance
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public KeyValuePair<string, IType>[] GenericArguments
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Type TypeForCLR
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IType ByRefType
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IType ArrayType
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string FullName
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string Name
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsValueType
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsDelegate
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public AppDomain AppDomain
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}
