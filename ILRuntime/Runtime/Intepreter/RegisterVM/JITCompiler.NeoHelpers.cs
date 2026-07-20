#if ENABLE_NEO_MODE
using System;
using ILRuntime.CLR.TypeSystem;

namespace ILRuntime.Runtime.Intepreter.RegisterVM
{
    partial struct JITCompiler
    {
        IType[] BuildInitialRegisterTypes(short locVarRegStart, int totalRegCnt)
        {
            IType[] registerTypes = new IType[totalRegCnt];
            int idx = 0;
            if (method.HasThis)
                registerTypes[idx++] = declaringType.IsValueType
                    ? declaringType.MakeByRefType()
                    : declaringType;
            for (int i = 0; i < method.ParameterCount && idx < registerTypes.Length; i++, idx++)
            {
                registerTypes[idx] = appdomain.GetType(def.Parameters[i].ParameterType, declaringType, method);
            }
            for (int i = 0; i < def.Body.Variables.Count; i++)
            {
                int reg = locVarRegStart + i;
                if (reg < registerTypes.Length)
                    registerTypes[reg] = appdomain.GetType(def.Body.Variables[i].VariableType, declaringType, method);
            }
            return registerTypes;
        }
    }
}
#endif
