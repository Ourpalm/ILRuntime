using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
using ILRuntime.Reflection;
using ILRuntime.CLR.Utils;

namespace ILRuntime.Runtime.Generated
{
    unsafe class System_UInt16_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            Type type = typeof(System.UInt16);

            app.RegisterCLRCreateArrayInstance(type, s => new System.UInt16[s]);


        }

        static System.UInt16 GetInstance(ILRuntime.Runtime.Enviorment.AppDomain __domain, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ptr_of_this_method = ILIntepreter.GetObjectAndResolveReference(ptr_of_this_method);
            System.UInt16 instance_of_this_method;
            switch(ptr_of_this_method->ObjectType)
            {
                case ObjectTypes.FieldReference:
                    {
                        var instance_of_fieldReference = __mStack[ptr_of_this_method->Value];
                        if(instance_of_fieldReference is ILTypeInstance)
                        {
                            instance_of_this_method = (System.UInt16)typeof(System.UInt16).CheckCLRTypes(((ILTypeInstance)instance_of_fieldReference)[ptr_of_this_method->ValueLow]);
                        }
                        else
                        {
                            var t = __domain.GetType(instance_of_fieldReference.GetType()) as CLRType;
                            instance_of_this_method = (System.UInt16)t.GetFieldValue(ptr_of_this_method->ValueLow, instance_of_fieldReference);
                        }
                    }
                    break;
                case ObjectTypes.StaticFieldReference:
                    {
                        var t = __domain.GetType(ptr_of_this_method->Value);
                        if(t is ILType)
                        {
                            instance_of_this_method = (System.UInt16)typeof(System.UInt16).CheckCLRTypes(((ILType)t).StaticInstance[ptr_of_this_method->ValueLow]);
                        }
                        else
                        {
                            instance_of_this_method = (System.UInt16)((CLRType)t).GetFieldValue(ptr_of_this_method->ValueLow, null);
                        }
                    }
                    break;
                case ObjectTypes.ArrayReference:
                    {
                        var instance_of_arrayReference = __mStack[ptr_of_this_method->Value] as System.UInt16[];
                        instance_of_this_method = instance_of_arrayReference[ptr_of_this_method->ValueLow];                        
                    }
                    break;
                default:
                    instance_of_this_method = (ushort)ptr_of_this_method->Value;
                    break;
            }
            return instance_of_this_method;
        }



    }
}
