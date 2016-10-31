
using System;

namespace ILRuntimeTest.TestFramework
{
    class ILRuntimeHelper
    {
        public static void Init(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            if (app == null)
            {
                //should log error
                return;
            }

            app.RegisterCrossBindingAdaptor(new ILRuntimeTest.TestFramework.TestClass2Adaptor());
app.RegisterCrossBindingAdaptor(new ILRuntimeTest.TestFramework.TestClass3Adaptor());
   

            
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