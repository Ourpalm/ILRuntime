using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Globalization;

using ILRuntime.CLR.TypeSystem;

namespace ILRuntime.Reflection
{
    public class ILRuntimeParameterInfo : ParameterInfo
    {
        IType type;
        MethodBase method;

        public ILRuntimeParameterInfo(IType type, MethodBase method)
        {
            this.type = type;
            this.method = method;
            this.MemberImpl = method;
        }
        public override Type ParameterType
        {
            get
            {
                return type.ReflectionType;
            }
        }

        public override string Name
        {
            get
            {
                return type.FullName;
            }
        }
    }
}
