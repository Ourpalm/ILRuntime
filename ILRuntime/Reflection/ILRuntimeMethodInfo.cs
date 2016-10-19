using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Globalization;

using ILRuntime.CLR.Method;

namespace ILRuntime.Reflection
{
    class ILRuntimeMethodInfo : MethodInfo
    {
        ILMethod method;
        object[] param;
        public ILRuntimeMethodInfo(ILMethod m)
        {
            method = m;
        }

        internal ILMethod ILMethod { get { return method; } }
        public override MethodAttributes Attributes
        {
            get
            {
                return MethodAttributes.Public;
            }
        }

        public override Type DeclaringType
        {
            get
            {
                return method.DeclearingType.ReflectionType;
            }
        }

        public override RuntimeMethodHandle MethodHandle
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override string Name
        {
            get
            {
                return method.Name;
            }
        }

        public override Type ReflectedType
        {
            get
            {
                return method.DeclearingType.ReflectionType;
            }
        }

        public override ICustomAttributeProvider ReturnTypeCustomAttributes
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override MethodInfo GetBaseDefinition()
        {
            return this;
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            throw new NotImplementedException();
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            throw new NotImplementedException();
        }

        public override MethodImplAttributes GetMethodImplementationFlags()
        {
            throw new NotImplementedException();
        }

        public override ParameterInfo[] GetParameters()
        {
            throw new NotImplementedException();
        }

        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            if (method.HasThis)
            {
                if (param == null)
                {
                    param = new object[method.HasThis ? method.ParameterCount + 1 : method.ParameterCount];
                }
                param[0] = obj;
                if (parameters != null)
                    parameters.CopyTo(param, 1);
                var res = method.DeclearingType.AppDomain.Invoke(method, param);
                Array.Clear(param, 0, param.Length);
                return res;
            }
            else
                return method.DeclearingType.AppDomain.Invoke(method, parameters);
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            throw new NotImplementedException();
        }
    }
}
