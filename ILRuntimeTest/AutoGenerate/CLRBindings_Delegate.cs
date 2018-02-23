using System;
using System.Collections.Generic;
using System.Reflection;

namespace ILRuntime.Runtime.Generated
{
    class CLRDelegateBindings
    {
        /// <summary>
        /// Initialize the CLR Delegate binding, please invoke this AFTER CLR Redirection registration
        /// </summary>
        public static void Initialize(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            ILRuntimeTest_TestFramework_IntDelegate_Binding.Register(app);
            System_Action_1_Int32_Binding.Register(app);
            ILRuntimeTest_TestFramework_IntDelegate2_Binding.Register(app);
            System_Func_2_Int32_Int32_Binding.Register(app);
            System_Action_3_Int32_String_String_Binding.Register(app);
            System_Action_1_String_Binding.Register(app);
        }
    }
}
