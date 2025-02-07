using ILRuntime.Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Text;

namespace ILRuntime.Hybrid
{
    internal static class CecilExtensions
    {
        public static GenericInstanceType MakeGenericInstanceType(this TypeReference self, params TypeReference[] arguments)
        {
            if (self == null)
                throw new ArgumentNullException("self");
            if (arguments == null)
                throw new ArgumentNullException("arguments");
            if (arguments.Length == 0)
                throw new ArgumentException();
            if (self.GenericParameters.Count != arguments.Length)
                throw new ArgumentException();

            var instance = new GenericInstanceType(self);

            foreach (var argument in arguments)
                instance.GenericArguments.Add(argument);

            return instance;
        }

        public static int FindGenericArgument(this GenericInstanceType self, string name)
        {
            var def = self.Resolve();
            for(int i = 0; i < def.GenericParameters.Count; i++)
            {
                if (def.GenericParameters[i].Name == name)
                    return i;
            }
            return -1;
        }

        public static MethodReference GetMethod(this GenericInstanceType self, MethodReference method)
        {
            if (self.Resolve() != method.DeclaringType)
                throw new NotSupportedException();
            var result = new MethodReference(method.Name, method.ReturnType)
            {
                DeclaringType = self,
                HasThis = method.HasThis,
                ExplicitThis = method.ExplicitThis,
                CallingConvention = method.CallingConvention,
            };
            for (int i = 0; i < method.Parameters.Count; i++)
            {
                var p = method.Parameters[i];
                TypeReference tr = p.ParameterType;
                result.Parameters.Add(new ParameterDefinition(p.Name, p.Attributes, tr));
            }
            return result;
        }
    }
}
