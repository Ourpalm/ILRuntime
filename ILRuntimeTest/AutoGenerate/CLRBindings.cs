using System;
using System.Collections.Generic;
using System.Reflection;

namespace ILRuntime.Runtime.Generated
{
    class CLRBindings
    {
        /// <summary>
        /// Initialize the CLR binding, please invoke this AFTER CLR Redirection registration
        /// </summary>
        /// <param name="app"></param>
        public static void Initialize(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            System_Int32.Register(app);
            System_Single.Register(app);
            System_Int64.Register(app);
            System_Object.Register(app);
            System_String.Register(app);
            System_ValueType.Register(app);
            System_Console.Register(app);
            System_Array.Register(app);
            System_Collections_Generic_Dictionary_2_String_Int32.Register(app);
            System_Collections_Generic_Dictionary_2_ILTypeInstance_Int32.Register(app);
            ILRuntimeTest_TestFramework_TestStruct.Register(app);
        }
    }
}
