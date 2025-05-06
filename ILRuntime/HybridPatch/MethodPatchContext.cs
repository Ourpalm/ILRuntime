using ILRuntime.CLR.Method;
using ILRuntime.Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Text;

namespace ILRuntime.Hybrid
{
    internal struct MethodPatchContext
    {
        AssemblyPatch patch;
        MethodPatchInfo methodInfo;
        bool isValid;

        public bool IsValid { get { return isValid; } }

        public MethodPatchContext(AssemblyPatch patch, MethodPatchInfo methodInfo)
        {
            this.patch = patch;
            this.methodInfo = methodInfo;
            this.isValid = true;
        }

        public void GenerateCodeBody(TypeReference declaringType, ILMethod method, Runtime.Enviorment.AppDomain domain)
        {
            patch.InitializeMethodBody(declaringType, methodInfo, method, domain);
        }
    }
}
