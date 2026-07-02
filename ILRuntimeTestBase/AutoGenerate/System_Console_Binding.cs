using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
using ILRuntime.Reflection;
using ILRuntime.CLR.Utils;
#if DEBUG && !DISABLE_ILRUNTIME_DEBUG
using AutoList = System.Collections.Generic.List<object>;
#else
using AutoList = ILRuntime.Other.UncheckedList<object>;
#endif
namespace ILRuntime.Runtime.Generated
{
    [ILRuntimePatchIgnore]
    unsafe class System_Console_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            Type[] args;
            Type type = typeof(System.Console);
            args = new Type[]{typeof(System.String)};
            method = type.GetMethod("WriteLine", flag, null, args, null);
#if ENABLE_NEO_MODE
            app.RegisterCLRMethodRedirectionNeo(method, WriteLine_0_Neo);
#else
            app.RegisterCLRMethodRedirection(method, WriteLine_0);
#endif
            args = new Type[]{typeof(System.String), typeof(System.Object)};
            method = type.GetMethod("WriteLine", flag, null, args, null);
#if ENABLE_NEO_MODE
            app.RegisterCLRMethodRedirectionNeo(method, WriteLine_1_Neo);
#else
            app.RegisterCLRMethodRedirection(method, WriteLine_1);
#endif
            args = new Type[]{typeof(System.String), typeof(System.Object[])};
            method = type.GetMethod("WriteLine", flag, null, args, null);
#if ENABLE_NEO_MODE
            app.RegisterCLRMethodRedirectionNeo(method, WriteLine_2_Neo);
#else
            app.RegisterCLRMethodRedirection(method, WriteLine_2);
#endif
            args = new Type[]{typeof(System.Int32)};
            method = type.GetMethod("WriteLine", flag, null, args, null);
#if ENABLE_NEO_MODE
            app.RegisterCLRMethodRedirectionNeo(method, WriteLine_3_Neo);
#else
            app.RegisterCLRMethodRedirection(method, WriteLine_3);
#endif
            args = new Type[]{typeof(System.Int64)};
            method = type.GetMethod("WriteLine", flag, null, args, null);
#if ENABLE_NEO_MODE
            app.RegisterCLRMethodRedirectionNeo(method, WriteLine_4_Neo);
#else
            app.RegisterCLRMethodRedirection(method, WriteLine_4);
#endif
            args = new Type[]{typeof(System.Char)};
            method = type.GetMethod("WriteLine", flag, null, args, null);
#if ENABLE_NEO_MODE
            app.RegisterCLRMethodRedirectionNeo(method, WriteLine_5_Neo);
#else
            app.RegisterCLRMethodRedirection(method, WriteLine_5);
#endif
            args = new Type[]{typeof(System.Single)};
            method = type.GetMethod("WriteLine", flag, null, args, null);
#if ENABLE_NEO_MODE
            app.RegisterCLRMethodRedirectionNeo(method, WriteLine_6_Neo);
#else
            app.RegisterCLRMethodRedirection(method, WriteLine_6);
#endif
            args = new Type[]{typeof(System.Double)};
            method = type.GetMethod("WriteLine", flag, null, args, null);
#if ENABLE_NEO_MODE
            app.RegisterCLRMethodRedirectionNeo(method, WriteLine_7_Neo);
#else
            app.RegisterCLRMethodRedirection(method, WriteLine_7);
#endif
            args = new Type[]{typeof(System.Object)};
            method = type.GetMethod("WriteLine", flag, null, args, null);
#if ENABLE_NEO_MODE
            app.RegisterCLRMethodRedirectionNeo(method, WriteLine_8_Neo);
#else
            app.RegisterCLRMethodRedirection(method, WriteLine_8);
#endif
            args = new Type[]{typeof(System.Boolean)};
            method = type.GetMethod("WriteLine", flag, null, args, null);
#if ENABLE_NEO_MODE
            app.RegisterCLRMethodRedirectionNeo(method, WriteLine_9_Neo);
#else
            app.RegisterCLRMethodRedirection(method, WriteLine_9);
#endif
            args = new Type[]{typeof(System.UInt32)};
            method = type.GetMethod("WriteLine", flag, null, args, null);
#if ENABLE_NEO_MODE
            app.RegisterCLRMethodRedirectionNeo(method, WriteLine_10_Neo);
#else
            app.RegisterCLRMethodRedirection(method, WriteLine_10);
#endif
            args = new Type[]{typeof(System.UInt64)};
            method = type.GetMethod("WriteLine", flag, null, args, null);
#if ENABLE_NEO_MODE
            app.RegisterCLRMethodRedirectionNeo(method, WriteLine_11_Neo);
#else
            app.RegisterCLRMethodRedirection(method, WriteLine_11);
#endif
            args = new Type[]{typeof(System.String)};
            method = type.GetMethod("Write", flag, null, args, null);
#if ENABLE_NEO_MODE
            app.RegisterCLRMethodRedirectionNeo(method, Write_12_Neo);
#else
            app.RegisterCLRMethodRedirection(method, Write_12);
#endif


        }


#if ENABLE_NEO_MODE
        static void WriteLine_0_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            System.String @value = (System.String)ILIntepreter.ReadNeoReference(__frameBase, ref __curPrim, __mStack);
            System.Console.WriteLine(@value);
        }
#else
        static StackObject* WriteLine_0(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 1;

            ptr_of_this_method = __esp - 1;
            System.String @value = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            System.Console.WriteLine(@value);

            return __ret;
        }
#endif

#if ENABLE_NEO_MODE
        static void WriteLine_1_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            System.String @format = (System.String)ILIntepreter.ReadNeoReference(__frameBase, ref __curPrim, __mStack);
            System.Object @arg0 = (System.Object)ILIntepreter.ReadNeoReference(__frameBase, ref __curPrim, __mStack);
            System.Console.WriteLine(@format, @arg0);
        }
#else
        static StackObject* WriteLine_1(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 2;

            ptr_of_this_method = __esp - 1;
            System.Object @arg0 = (System.Object)typeof(System.Object).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = __esp - 2;
            System.String @format = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            System.Console.WriteLine(@format, @arg0);

            return __ret;
        }
#endif

#if ENABLE_NEO_MODE
        static void WriteLine_2_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            System.String @format = (System.String)ILIntepreter.ReadNeoReference(__frameBase, ref __curPrim, __mStack);
            System.Object[] @arg = (System.Object[])ILIntepreter.ReadNeoReference(__frameBase, ref __curPrim, __mStack);
            System.Console.WriteLine(@format, @arg);
        }
#else
        static StackObject* WriteLine_2(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 2;

            ptr_of_this_method = __esp - 1;
            System.Object[] @arg = (System.Object[])typeof(System.Object[]).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = __esp - 2;
            System.String @format = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            System.Console.WriteLine(@format, @arg);

            return __ret;
        }
#endif

#if ENABLE_NEO_MODE
        static void WriteLine_3_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            System.Int32 @value = (System.Int32)ILIntepreter.ReadNeoInt32(__frameBase, ref __curPrim);
            System.Console.WriteLine(@value);
        }
#else
        static StackObject* WriteLine_3(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 1;

            ptr_of_this_method = __esp - 1;
            System.Int32 @value = ptr_of_this_method->Value;


            System.Console.WriteLine(@value);

            return __ret;
        }
#endif

#if ENABLE_NEO_MODE
        static void WriteLine_4_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            System.Int64 @value = ILIntepreter.ReadNeoInt64(__frameBase, ref __curPrim);
            System.Console.WriteLine(@value);
        }
#else
        static StackObject* WriteLine_4(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 1;

            ptr_of_this_method = __esp - 1;
            System.Int64 @value = *(long*)&ptr_of_this_method->Value;


            System.Console.WriteLine(@value);

            return __ret;
        }
#endif

#if ENABLE_NEO_MODE
        static void WriteLine_5_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            System.Char @value = ILIntepreter.ReadNeoChar(__frameBase, ref __curPrim);
            System.Console.WriteLine(@value);
        }
#else
        static StackObject* WriteLine_5(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 1;

            ptr_of_this_method = __esp - 1;
            System.Char @value = (char)ptr_of_this_method->Value;


            System.Console.WriteLine(@value);

            return __ret;
        }
#endif

#if ENABLE_NEO_MODE
        static void WriteLine_6_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            System.Single @value = ILIntepreter.ReadNeoFloat(__frameBase, ref __curPrim);
            System.Console.WriteLine(@value);
        }
#else
        static StackObject* WriteLine_6(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 1;

            ptr_of_this_method = __esp - 1;
            System.Single @value = *(float*)&ptr_of_this_method->Value;


            System.Console.WriteLine(@value);

            return __ret;
        }
#endif

#if ENABLE_NEO_MODE
        static void WriteLine_7_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            System.Double @value = ILIntepreter.ReadNeoDouble(__frameBase, ref __curPrim);
            System.Console.WriteLine(@value);
        }
#else
        static StackObject* WriteLine_7(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 1;

            ptr_of_this_method = __esp - 1;
            System.Double @value = *(double*)&ptr_of_this_method->Value;


            System.Console.WriteLine(@value);

            return __ret;
        }
#endif

#if ENABLE_NEO_MODE
        static void WriteLine_8_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            System.Object @value = (System.Object)ILIntepreter.ReadNeoReference(__frameBase, ref __curPrim, __mStack);
            System.Console.WriteLine(@value);
        }
#else
        static StackObject* WriteLine_8(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 1;

            ptr_of_this_method = __esp - 1;
            System.Object @value = (System.Object)typeof(System.Object).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            System.Console.WriteLine(@value);

            return __ret;
        }
#endif

#if ENABLE_NEO_MODE
        static void WriteLine_9_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            System.Boolean @value = ILIntepreter.ReadNeoBoolean(__frameBase, ref __curPrim);
            System.Console.WriteLine(@value);
        }
#else
        static StackObject* WriteLine_9(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 1;

            ptr_of_this_method = __esp - 1;
            System.Boolean @value = ptr_of_this_method->Value == 1;


            System.Console.WriteLine(@value);

            return __ret;
        }
#endif

#if ENABLE_NEO_MODE
        static void WriteLine_10_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            System.UInt32 @value = ILIntepreter.ReadNeoUInt32(__frameBase, ref __curPrim);
            System.Console.WriteLine(@value);
        }
#else
        static StackObject* WriteLine_10(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 1;

            ptr_of_this_method = __esp - 1;
            System.UInt32 @value = (uint)ptr_of_this_method->Value;


            System.Console.WriteLine(@value);

            return __ret;
        }
#endif

#if ENABLE_NEO_MODE
        static void WriteLine_11_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            System.UInt64 @value = ILIntepreter.ReadNeoUInt64(__frameBase, ref __curPrim);
            System.Console.WriteLine(@value);
        }
#else
        static StackObject* WriteLine_11(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 1;

            ptr_of_this_method = __esp - 1;
            System.UInt64 @value = *(ulong*)&ptr_of_this_method->Value;


            System.Console.WriteLine(@value);

            return __ret;
        }
#endif

#if ENABLE_NEO_MODE
        static void Write_12_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            System.String @value = (System.String)ILIntepreter.ReadNeoReference(__frameBase, ref __curPrim, __mStack);
            System.Console.Write(@value);
        }
#else
        static StackObject* Write_12(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 1;

            ptr_of_this_method = __esp - 1;
            System.String @value = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            System.Console.Write(@value);

            return __ret;
        }
#endif



    }
}
