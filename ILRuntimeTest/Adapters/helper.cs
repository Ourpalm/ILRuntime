
using ILRuntime.CLR.Method;
using ILRuntime.CLR.Utils;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
using System;
using System.Collections.Generic;

namespace ILRuntimeTest.TestFramework
{
    unsafe class ILRuntimeHelper
    {
        // manual register
        public static void Init(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            if (app == null)
            {
                // should log error
                return;
            }

            ILRuntime.Runtime.Enviorment.PrimitiveConverter<ILRuntimeTest.TestFramework.TestCLREnum>.ToInteger = (a) => (int)a;
            ILRuntime.Runtime.Enviorment.PrimitiveConverter<ILRuntimeTest.TestFramework.TestCLREnum>.FromInteger = (a) => (ILRuntimeTest.TestFramework.TestCLREnum)a;

            // adaptor register 

            app.RegisterCrossBindingAdaptor(new ClassInheritanceTestAdaptor());            
            app.RegisterCrossBindingAdaptor(new InterfaceTestAdaptor());            
            app.RegisterCrossBindingAdaptor(new TestClass2Adaptor());            
            app.RegisterCrossBindingAdaptor(new TestClass3Adaptor());
            app.RegisterCrossBindingAdaptor(new TestClass4Adaptor());
            app.RegisterCrossBindingAdaptor(new IDisposableClassInheritanceAdaptor());
            app.RegisterCrossBindingAdaptor(new ClassInheritanceTest2Adaptor());
            app.RegisterCrossBindingAdaptor(new IAsyncStateMachineClassInheritanceAdaptor());

            // value type register

            app.RegisterValueTypeBinder(typeof(TestVector3), new TestVector3Binder());
            app.RegisterValueTypeBinder(typeof(TestVectorStruct), new TestVectorStructBinder());
            app.RegisterValueTypeBinder(typeof(TestVectorStruct2), new TestVectorStruct2Binder());
            app.RegisterValueTypeBinder(typeof(System.Collections.Generic.KeyValuePair<uint, ILRuntime.Runtime.Intepreter.ILTypeInstance>), new KeyValuePairUInt32ILTypeInstanceBinder());

            // delegate register 

            app.DelegateManager.RegisterFunctionDelegate<System.Int32,System.Boolean>();
            
            app.DelegateManager.RegisterMethodDelegate<System.Int32>();
            
            app.DelegateManager.RegisterFunctionDelegate<System.Int32,System.Int32>();
            
            app.DelegateManager.RegisterMethodDelegate<System.Int32,System.String,System.String>();
            
            app.DelegateManager.RegisterMethodDelegate<ILRuntimeTest.TestFramework.BaseClassTest>();

            app.DelegateManager.RegisterFunctionDelegate<System.Int32>();

            app.DelegateManager.RegisterFunctionDelegate<System.Int32, System.Single, System.Int16, System.Double>();
            app.DelegateManager.RegisterFunctionDelegate<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Boolean>();
            app.DelegateManager.RegisterMethodDelegate<ILRuntimeTest.TestFramework.TestCLREnum>();
            app.DelegateManager.RegisterFunctionDelegate<ILRuntimeTest.TestFramework.TestCLREnum>();
            app.DelegateManager.RegisterFunctionDelegate<System.Threading.Tasks.Task<System.Collections.Generic.List<System.String>>>();
            app.DelegateManager.RegisterFunctionDelegate<System.Byte, System.Boolean>();
            app.DelegateManager.RegisterFunctionDelegate<System.Byte, System.Byte>();
            app.DelegateManager.RegisterFunctionDelegate<System.Linq.IGrouping<System.Byte, System.Byte>, ILRuntime.Runtime.Intepreter.ILTypeInstance>();
            app.DelegateManager.RegisterFunctionDelegate<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Int32>();
            // delegate convertor
            app.DelegateManager.RegisterDelegateConvertor<System.Predicate<ILRuntime.Runtime.Intepreter.ILTypeInstance>>((act) =>
            {
                return new System.Predicate<ILRuntime.Runtime.Intepreter.ILTypeInstance>((obj) =>
                {
                    return ((Func<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Boolean>)act)(obj);
                });
            });
            app.DelegateManager.RegisterDelegateConvertor<ILRuntimeTest.TestFramework.IntDelegate>((action) =>
            {
                return new ILRuntimeTest.TestFramework.IntDelegate((a) =>
                {
                    ((Action<Int32>)action)(a);
                });
            });
            app.DelegateManager.RegisterDelegateConvertor<ILRuntimeTest.TestFramework.Int2Delegate>((action) =>
            {
                return new ILRuntimeTest.TestFramework.Int2Delegate((a,b) =>
                {
                    ((Action<Int32,Int32>)action)(a,b);
                });
            });
            app.DelegateManager.RegisterDelegateConvertor<ILRuntimeTest.TestFramework.InitFloat>((action) =>
            {
                return new ILRuntimeTest.TestFramework.InitFloat((a,b) =>
                {
                    ((Action<Int32,Single>)action)(a,b);
                });
            });            
            app.DelegateManager.RegisterDelegateConvertor<ILRuntimeTest.TestFramework.IntDelegate2>((action) =>
            {
                return new ILRuntimeTest.TestFramework.IntDelegate2((a) =>
                {
                    return ((Func<Int32,Int32>)action)(a);
                });
            });
            
            app.DelegateManager.RegisterDelegateConvertor<ILRuntimeTest.TestFramework.Int2Delegate2>((action) =>
            {
                return new ILRuntimeTest.TestFramework.Int2Delegate2((a,b) =>
                {
                    return ((Func<Int32,Int32,Boolean>)action)(a,b);
                });
            });
            
            app.DelegateManager.RegisterDelegateConvertor<ILRuntimeTest.TestFramework.IntFloatDelegate2>((action) =>
            {
                return new ILRuntimeTest.TestFramework.IntFloatDelegate2((a,b) =>
                {
                    return ((Func<Int32,Single,String>)action)(a,b);
                });
            });

            // LitJson register
            LitJson.JsonMapper.RegisterILRuntimeCLRRedirection(app);

            app.DelegateManager.RegisterMethodDelegate<System.Object>();
            app.DelegateManager.RegisterMethodDelegate<ILRuntimeTest.TestBase.ExtensionClass, System.Object>();
            app.DelegateManager.RegisterMethodDelegate<System.Exception>();
            app.DelegateManager.RegisterMethodDelegate<ILRuntimeTest.TestBase.ExtensionClass, System.Exception>();
            app.DelegateManager.RegisterMethodDelegate<ILRuntimeTest.TestBase.ExtensionClass, System.ArgumentException>();
            app.DelegateManager.RegisterMethodDelegate<System.Exception>();
            app.DelegateManager.RegisterMethodDelegate<ILRuntimeTest.TestBase.ExtensionClass<System.Int32>, System.Exception>();
            app.DelegateManager.RegisterMethodDelegate<System.ArgumentException>();
            app.DelegateManager.RegisterMethodDelegate<ILRuntimeTest.TestBase.ExtensionClass<System.Int32>, System.ArgumentException>();
            app.DelegateManager.RegisterMethodDelegate<ILRuntimeTest.TestBase.ExtensionClass>();
            app.DelegateManager.RegisterFunctionDelegate<ILRuntimeTest.TestBase.ExtensionClass, System.Int32>();
            app.DelegateManager.RegisterFunctionDelegate<System.Threading.Tasks.Task>();
            app.DelegateManager.RegisterFunctionDelegate<ILRuntimeTest.TestBase.ExtensionClass, System.Threading.Tasks.Task>();
            app.DelegateManager.RegisterFunctionDelegate<System.Threading.Tasks.Task<System.Int32>>();
            app.DelegateManager.RegisterFunctionDelegate<ILRuntimeTest.TestBase.ExtensionClass, System.Threading.Tasks.Task<System.Int32>>();

            var intArr2 = typeof(int[,]);
            var addr = intArr2.GetMethod("Address", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            app.RegisterCLRMethodRedirection(addr, Address);
        }

        static StackObject* Address(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);


            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 y = ptr_of_this_method->Value;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Int32 x = ptr_of_this_method->Value;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            int[,] arr = (int[,])typeof(int[,]).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = arr[x, y];

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }
    }
}