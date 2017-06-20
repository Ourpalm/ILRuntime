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
        public static void Initialize(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            System_Collections_Generic_List_1_Int32_Binding.Register(app);
            System_Linq_Enumerable_Binding.Register(app);
            System_Console_Binding.Register(app);
            System_Collections_Generic_List_1_ILTypeInstance_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_ILTypeInstance_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_Int32_Binding.Register(app);
            LitJson_JsonMapper_Binding.Register(app);
            System_Collections_Generic_List_1_Adaptor_Binding.Register(app);
            System_String_Binding.Register(app);
            System_Type_Binding.Register(app);
            System_Object_Binding.Register(app);
            System_Reflection_MethodBase_Binding.Register(app);
            System_Activator_Binding.Register(app);
            System_Reflection_MemberInfo_Binding.Register(app);
            System_Reflection_FieldInfo_Binding.Register(app);
            ILRuntimeTest_TestFramework_ClassInheritanceTest_Binding.Register(app);
            System_Collections_Generic_List_1_Adaptor_Binding.Register(app);
            System_Int32_Binding.Register(app);
            ILRuntimeTest_TestFramework_ClassInheritanceTest2_1_Adaptor_Binding.Register(app);
            System_Exception_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Int32_Adaptor_Binding.Register(app);
            ILRuntimeTest_TestFramework_DelegateTest_Binding.Register(app);
            ILRuntimeTest_TestFramework_IntDelegate_Binding.Register(app);
            System_Action_1_Int32_Binding.Register(app);
            ILRuntimeTest_TestFramework_IntDelegate2_Binding.Register(app);
            System_Func_2_Int32_Int32_Binding.Register(app);
            System_Action_3_Int32_String_String_Binding.Register(app);
            ILRuntimeTest_TestFramework_BaseClassTest_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Int32_ILTypeInstance_Binding.Register(app);
            System_Action_1_String_Binding.Register(app);
            System_Boolean_Binding.Register(app);
            System_Math_Binding.Register(app);
            ILRuntimeTest_TestFramework_TestStruct_Binding.Register(app);
            ILRuntimeTest_TestFramework_TestClass3_Binding.Register(app);
            System_Byte_Binding.Register(app);
            System_IO_FileStream_Binding.Register(app);
            System_IO_Stream_Binding.Register(app);
            System_IDisposable_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Int64_Int32_Binding.Register(app);
            System_Diagnostics_Stopwatch_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_ILTypeInstance_Array_Binding.Register(app);
            System_Collections_Generic_IEnumerator_1_KeyValuePair_2_Int32_Int32_Binding.Register(app);
            System_Collections_Generic_KeyValuePair_2_Int32_Int32_Binding.Register(app);
            System_Collections_IEnumerator_Binding.Register(app);
            System_Collections_Generic_List_1_List_1_Int32_Binding.Register(app);
            System_Collections_Generic_List_1_List_1_List_1_Int32_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_ILTypeInstance_Int32_Binding.Register(app);
            System_Array_Binding.Register(app);
            System_Collections_Generic_List_1_Int32_Array_Binding.Register(app);
            System_Convert_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Int32_ILTypeInstance_Binding_ValueCollection_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Int32_ILTypeInstance_Binding_ValueCollection_Binding_Enumerator_Binding.Register(app);
            System_Collections_Generic_List_1_String_Binding.Register(app);
            System_Collections_Generic_List_1_String_Binding_Enumerator_Binding.Register(app);
            System_NotSupportedException_Binding.Register(app);
            System_Collections_Generic_List_1_ILTypeInstance_Binding_Enumerator_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Type_Int32_Binding.Register(app);
            System_Collections_IDictionary_Binding.Register(app);
            System_NotImplementedException_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Int32_Int32_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Int32_Int32_Binding_Enumerator_Binding.Register(app);
        }
    }
}
