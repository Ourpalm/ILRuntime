
using System;

namespace ILRuntimeTest.TestFramework
{
    public class ILRuntimeHelper
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
            app.RegisterCrossBindingAdaptor(new TestClass2Adapter());            
            app.RegisterCrossBindingAdaptor(new TestClass3Adaptor());
            app.RegisterCrossBindingAdaptor(new TestClass4Adaptor());
            app.RegisterCrossBindingAdaptor(new IDisposableAdapter());
            app.RegisterCrossBindingAdaptor(new ClassInheritanceTest2Adaptor());
            app.RegisterCrossBindingAdaptor(new IAsyncStateMachineClassInheritanceAdaptor());
            // value type register

            app.RegisterValueTypeBinder(typeof(TestVector3), new TestVector3Binder());
            app.RegisterValueTypeBinder(typeof(TestVectorStruct), new TestVectorStructBinder());
            app.RegisterValueTypeBinder(typeof(TestVectorStruct2), new TestVectorStruct2Binder());
            app.RegisterValueTypeBinder(typeof(System.Collections.Generic.KeyValuePair<uint, ILRuntime.Runtime.Intepreter.ILTypeInstance>), new KeyValuePairUInt32ILTypeInstanceBinder());

            app.RegisterValueTypeBinder(typeof(TestStructA), new TestStructABinder());
            app.RegisterValueTypeBinder(typeof(TestStructB), new TestStructBBinder());
            app.RegisterValueTypeBinder(typeof(Fixed64), new Fixed64Binder());
            app.RegisterValueTypeBinder(typeof(Fixed64Vector2), new FixedVector2Binder());

            // delegate register 

            app.DelegateManager.RegisterFunctionDelegate<System.Int32,System.Boolean>();
            
            app.DelegateManager.RegisterMethodDelegate<System.Int32>();
            
            app.DelegateManager.RegisterFunctionDelegate<System.Int32,System.Int32>();
            
            app.DelegateManager.RegisterMethodDelegate<System.Int32,System.String,System.String>();
            
            app.DelegateManager.RegisterMethodDelegate<ILRuntimeTest.TestFramework.BaseClassTest>();

            app.DelegateManager.RegisterFunctionDelegate<System.Int32>();
            app.DelegateManager.RegisterFunctionDelegate<System.Collections.Generic.KeyValuePair<System.Int32, System.Collections.Generic.List<System.Int32>>, System.Collections.Generic.IEnumerable<System.Int32>>();
            app.DelegateManager.RegisterFunctionDelegate<System.Int32, System.Single, System.Int16, System.Double>();
            app.DelegateManager.RegisterFunctionDelegate<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Boolean>();
            app.DelegateManager.RegisterMethodDelegate<ILRuntimeTest.TestFramework.TestCLREnum>();
            app.DelegateManager.RegisterFunctionDelegate<ILRuntimeTest.TestFramework.TestCLREnum>();
            app.DelegateManager.RegisterFunctionDelegate<System.Threading.Tasks.Task<System.Collections.Generic.List<System.String>>>();
            app.DelegateManager.RegisterFunctionDelegate<System.Byte, System.Boolean>();
            app.DelegateManager.RegisterFunctionDelegate<System.Byte, System.Byte>();
            app.DelegateManager.RegisterFunctionDelegate<System.Linq.IGrouping<System.Byte, System.Byte>, ILRuntime.Runtime.Intepreter.ILTypeInstance>();
            app.DelegateManager.RegisterFunctionDelegate<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Int32>();
            app.DelegateManager.RegisterFunctionDelegate<ILRuntimeTest.TestFramework.TestVector3, System.Single>();
            app.DelegateManager.RegisterMethodDelegate<ILRuntimeTest.TestFramework.TestVector3>();
            app.DelegateManager.RegisterFunctionDelegate<System.Reflection.FieldInfo, System.String>();
            app.DelegateManager.RegisterFunctionDelegate<ILRuntime.Runtime.Intepreter.ILTypeInstance, ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Int32>();
            app.DelegateManager.RegisterMethodDelegate<System.Single, System.Double, System.Int32>();
            // delegate convertor
            app.DelegateManager.RegisterDelegateConvertor<System.Comparison<ILRuntime.Runtime.Intepreter.ILTypeInstance>>((act) =>
            {
                return new System.Comparison<ILRuntime.Runtime.Intepreter.ILTypeInstance>((x, y) =>
                {
                    return ((Func<ILRuntime.Runtime.Intepreter.ILTypeInstance, ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Int32>)act)(x, y);
                });
            });
            app.DelegateManager.RegisterDelegateConvertor<ILRuntimeTest.TestFramework.TestValueTypeDelegate>((act) =>
            {
                return new ILRuntimeTest.TestFramework.TestValueTypeDelegate((vec) =>
                {
                    ((Action<ILRuntimeTest.TestFramework.TestVector3>)act)(vec);
                });
            });
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
            app.DelegateManager.RegisterDelegateConvertor<ILRuntimeTest.TestFramework.BindableProperty<System.Int64>.onChangeWithOldVal>((act) =>
            {
                return new ILRuntimeTest.TestFramework.BindableProperty<System.Int64>.onChangeWithOldVal((oldVal, newVal) =>
                {
                    ((Action<System.Int64, System.Int64>)act)(oldVal, newVal);
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
            app.DelegateManager.RegisterMethodDelegate<ILRuntime.Runtime.Intepreter.ILTypeInstance>(); 
            app.DelegateManager.RegisterMethodDelegate<ILRuntimeTest.TestBase.ExtensionClass<System.Int32>, System.ArgumentException>();
            app.DelegateManager.RegisterMethodDelegate<ILRuntimeTest.TestBase.ExtensionClass>();
            app.DelegateManager.RegisterMethodDelegate<System.Int64, System.Int64>();
            app.DelegateManager.RegisterFunctionDelegate<ILRuntimeTest.TestBase.ExtensionClass, System.Int32>();
            app.DelegateManager.RegisterFunctionDelegate<System.Threading.Tasks.Task>();
            app.DelegateManager.RegisterFunctionDelegate<ILRuntimeTest.TestBase.ExtensionClass, System.Threading.Tasks.Task>();
            app.DelegateManager.RegisterFunctionDelegate<System.Threading.Tasks.Task<System.Int32>>();
            app.DelegateManager.RegisterFunctionDelegate<ILRuntimeTest.TestBase.ExtensionClass, System.Threading.Tasks.Task<System.Int32>>();

            app.Prewarm("TestCases.AsyncAwaitTest", false);
            //app.Prewarm("ILRuntime.Runtime.Enviorment.CrossBindingAdaptorType");
        }
    }
}