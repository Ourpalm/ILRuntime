
using System;

namespace ILRuntimeTest.TestFramework
{
    class ILRuntimeHelper
    {
        public static void Init(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            if (app == null)
            {
                // should log error
                return;
            }

			// adaptor register 
                        
			app.RegisterCrossBindingAdaptor(new ClassInheritanceTestAdaptor());            
			app.RegisterCrossBindingAdaptor(new InterfaceTestAdaptor());            
			app.RegisterCrossBindingAdaptor(new TestClass2Adaptor());            
			app.RegisterCrossBindingAdaptor(new TestClass3Adaptor());            
			app.RegisterCrossBindingAdaptor(new TestClass4Adaptor());
            app.RegisterCrossBindingAdaptor(new ClassInheritanceTest2Adaptor());

            // delegate register 

            app.DelegateManager.RegisterFunctionDelegate<System.Int32,System.Boolean>();
			
			app.DelegateManager.RegisterMethodDelegate<System.Int32>();
			
			app.DelegateManager.RegisterFunctionDelegate<System.Int32,System.Int32>();
			
			app.DelegateManager.RegisterMethodDelegate<System.Int32,System.String,System.String>();
			
			app.DelegateManager.RegisterMethodDelegate<ILRuntimeTest.TestFramework.BaseClassTest>();


			// delegate convertor
            
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

        }
    }
}