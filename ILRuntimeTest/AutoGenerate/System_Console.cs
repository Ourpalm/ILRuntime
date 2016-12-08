using System;
using System.Collections.Generic;
using System.Reflection;

using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
using ILRuntime.Reflection;

namespace ILRuntime.Runtime.Generated
{
    unsafe class System_Console
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodInfo method;
            Type[] args;
            Type type = typeof(System.Console);
            args = new Type[]{};
            method = type.GetMethod("get_Error", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_Error_0);
            args = new Type[]{};
            method = type.GetMethod("get_In", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_In_1);
            args = new Type[]{};
            method = type.GetMethod("get_Out", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_Out_2);
            args = new Type[]{};
            method = type.GetMethod("get_InputEncoding", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_InputEncoding_3);
            args = new Type[]{typeof(System.Text.Encoding)};
            method = type.GetMethod("set_InputEncoding", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, set_InputEncoding_4);
            args = new Type[]{};
            method = type.GetMethod("get_OutputEncoding", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_OutputEncoding_5);
            args = new Type[]{typeof(System.Text.Encoding)};
            method = type.GetMethod("set_OutputEncoding", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, set_OutputEncoding_6);
            args = new Type[]{};
            method = type.GetMethod("Beep", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Beep_7);
            args = new Type[]{typeof(System.Int32), typeof(System.Int32)};
            method = type.GetMethod("Beep", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Beep_8);
            args = new Type[]{};
            method = type.GetMethod("Clear", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Clear_9);
            args = new Type[]{};
            method = type.GetMethod("get_BackgroundColor", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_BackgroundColor_10);
            args = new Type[]{typeof(System.ConsoleColor)};
            method = type.GetMethod("set_BackgroundColor", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, set_BackgroundColor_11);
            args = new Type[]{};
            method = type.GetMethod("get_ForegroundColor", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_ForegroundColor_12);
            args = new Type[]{typeof(System.ConsoleColor)};
            method = type.GetMethod("set_ForegroundColor", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, set_ForegroundColor_13);
            args = new Type[]{};
            method = type.GetMethod("ResetColor", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, ResetColor_14);
            args = new Type[]{typeof(System.Int32), typeof(System.Int32), typeof(System.Int32), typeof(System.Int32), typeof(System.Int32), typeof(System.Int32)};
            method = type.GetMethod("MoveBufferArea", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, MoveBufferArea_15);
            args = new Type[]{typeof(System.Int32), typeof(System.Int32), typeof(System.Int32), typeof(System.Int32), typeof(System.Int32), typeof(System.Int32), typeof(System.Char), typeof(System.ConsoleColor), typeof(System.ConsoleColor)};
            method = type.GetMethod("MoveBufferArea", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, MoveBufferArea_16);
            args = new Type[]{};
            method = type.GetMethod("get_BufferHeight", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_BufferHeight_17);
            args = new Type[]{typeof(System.Int32)};
            method = type.GetMethod("set_BufferHeight", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, set_BufferHeight_18);
            args = new Type[]{};
            method = type.GetMethod("get_BufferWidth", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_BufferWidth_19);
            args = new Type[]{typeof(System.Int32)};
            method = type.GetMethod("set_BufferWidth", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, set_BufferWidth_20);
            args = new Type[]{typeof(System.Int32), typeof(System.Int32)};
            method = type.GetMethod("SetBufferSize", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, SetBufferSize_21);
            args = new Type[]{};
            method = type.GetMethod("get_WindowHeight", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_WindowHeight_22);
            args = new Type[]{typeof(System.Int32)};
            method = type.GetMethod("set_WindowHeight", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, set_WindowHeight_23);
            args = new Type[]{};
            method = type.GetMethod("get_WindowWidth", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_WindowWidth_24);
            args = new Type[]{typeof(System.Int32)};
            method = type.GetMethod("set_WindowWidth", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, set_WindowWidth_25);
            args = new Type[]{typeof(System.Int32), typeof(System.Int32)};
            method = type.GetMethod("SetWindowSize", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, SetWindowSize_26);
            args = new Type[]{};
            method = type.GetMethod("get_LargestWindowWidth", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_LargestWindowWidth_27);
            args = new Type[]{};
            method = type.GetMethod("get_LargestWindowHeight", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_LargestWindowHeight_28);
            args = new Type[]{};
            method = type.GetMethod("get_WindowLeft", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_WindowLeft_29);
            args = new Type[]{typeof(System.Int32)};
            method = type.GetMethod("set_WindowLeft", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, set_WindowLeft_30);
            args = new Type[]{};
            method = type.GetMethod("get_WindowTop", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_WindowTop_31);
            args = new Type[]{typeof(System.Int32)};
            method = type.GetMethod("set_WindowTop", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, set_WindowTop_32);
            args = new Type[]{typeof(System.Int32), typeof(System.Int32)};
            method = type.GetMethod("SetWindowPosition", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, SetWindowPosition_33);
            args = new Type[]{};
            method = type.GetMethod("get_CursorLeft", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_CursorLeft_34);
            args = new Type[]{typeof(System.Int32)};
            method = type.GetMethod("set_CursorLeft", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, set_CursorLeft_35);
            args = new Type[]{};
            method = type.GetMethod("get_CursorTop", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_CursorTop_36);
            args = new Type[]{typeof(System.Int32)};
            method = type.GetMethod("set_CursorTop", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, set_CursorTop_37);
            args = new Type[]{typeof(System.Int32), typeof(System.Int32)};
            method = type.GetMethod("SetCursorPosition", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, SetCursorPosition_38);
            args = new Type[]{};
            method = type.GetMethod("get_CursorSize", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_CursorSize_39);
            args = new Type[]{typeof(System.Int32)};
            method = type.GetMethod("set_CursorSize", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, set_CursorSize_40);
            args = new Type[]{};
            method = type.GetMethod("get_CursorVisible", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_CursorVisible_41);
            args = new Type[]{typeof(System.Boolean)};
            method = type.GetMethod("set_CursorVisible", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, set_CursorVisible_42);
            args = new Type[]{};
            method = type.GetMethod("get_Title", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_Title_43);
            args = new Type[]{typeof(System.String)};
            method = type.GetMethod("set_Title", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, set_Title_44);
            args = new Type[]{};
            method = type.GetMethod("ReadKey", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, ReadKey_45);
            args = new Type[]{typeof(System.Boolean)};
            method = type.GetMethod("ReadKey", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, ReadKey_46);
            args = new Type[]{};
            method = type.GetMethod("get_KeyAvailable", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_KeyAvailable_47);
            args = new Type[]{};
            method = type.GetMethod("get_NumberLock", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_NumberLock_48);
            args = new Type[]{};
            method = type.GetMethod("get_CapsLock", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_CapsLock_49);
            args = new Type[]{};
            method = type.GetMethod("get_TreatControlCAsInput", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_TreatControlCAsInput_50);
            args = new Type[]{typeof(System.Boolean)};
            method = type.GetMethod("set_TreatControlCAsInput", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, set_TreatControlCAsInput_51);
            args = new Type[]{};
            method = type.GetMethod("OpenStandardError", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, OpenStandardError_52);
            args = new Type[]{typeof(System.Int32)};
            method = type.GetMethod("OpenStandardError", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, OpenStandardError_53);
            args = new Type[]{};
            method = type.GetMethod("OpenStandardInput", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, OpenStandardInput_54);
            args = new Type[]{typeof(System.Int32)};
            method = type.GetMethod("OpenStandardInput", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, OpenStandardInput_55);
            args = new Type[]{};
            method = type.GetMethod("OpenStandardOutput", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, OpenStandardOutput_56);
            args = new Type[]{typeof(System.Int32)};
            method = type.GetMethod("OpenStandardOutput", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, OpenStandardOutput_57);
            args = new Type[]{typeof(System.IO.TextReader)};
            method = type.GetMethod("SetIn", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, SetIn_58);
            args = new Type[]{typeof(System.IO.TextWriter)};
            method = type.GetMethod("SetOut", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, SetOut_59);
            args = new Type[]{typeof(System.IO.TextWriter)};
            method = type.GetMethod("SetError", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, SetError_60);
            args = new Type[]{};
            method = type.GetMethod("Read", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Read_61);
            args = new Type[]{};
            method = type.GetMethod("ReadLine", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, ReadLine_62);
            args = new Type[]{};
            method = type.GetMethod("WriteLine", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, WriteLine_63);
            args = new Type[]{typeof(System.Boolean)};
            method = type.GetMethod("WriteLine", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, WriteLine_64);
            args = new Type[]{typeof(System.Char)};
            method = type.GetMethod("WriteLine", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, WriteLine_65);
            args = new Type[]{typeof(System.Char[])};
            method = type.GetMethod("WriteLine", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, WriteLine_66);
            args = new Type[]{typeof(System.Char[]), typeof(System.Int32), typeof(System.Int32)};
            method = type.GetMethod("WriteLine", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, WriteLine_67);
            args = new Type[]{typeof(System.Decimal)};
            method = type.GetMethod("WriteLine", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, WriteLine_68);
            args = new Type[]{typeof(System.Double)};
            method = type.GetMethod("WriteLine", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, WriteLine_69);
            args = new Type[]{typeof(System.Single)};
            method = type.GetMethod("WriteLine", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, WriteLine_70);
            args = new Type[]{typeof(System.Int32)};
            method = type.GetMethod("WriteLine", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, WriteLine_71);
            args = new Type[]{typeof(System.UInt32)};
            method = type.GetMethod("WriteLine", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, WriteLine_72);
            args = new Type[]{typeof(System.Int64)};
            method = type.GetMethod("WriteLine", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, WriteLine_73);
            args = new Type[]{typeof(System.UInt64)};
            method = type.GetMethod("WriteLine", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, WriteLine_74);
            args = new Type[]{typeof(System.Object)};
            method = type.GetMethod("WriteLine", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, WriteLine_75);
            args = new Type[]{typeof(System.String)};
            method = type.GetMethod("WriteLine", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, WriteLine_76);
            args = new Type[]{typeof(System.String), typeof(System.Object)};
            method = type.GetMethod("WriteLine", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, WriteLine_77);
            args = new Type[]{typeof(System.String), typeof(System.Object), typeof(System.Object)};
            method = type.GetMethod("WriteLine", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, WriteLine_78);
            args = new Type[]{typeof(System.String), typeof(System.Object), typeof(System.Object), typeof(System.Object)};
            method = type.GetMethod("WriteLine", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, WriteLine_79);
            args = new Type[]{typeof(System.String), typeof(System.Object), typeof(System.Object), typeof(System.Object), typeof(System.Object)};
            method = type.GetMethod("WriteLine", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, WriteLine_80);
            args = new Type[]{typeof(System.String), typeof(System.Object[])};
            method = type.GetMethod("WriteLine", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, WriteLine_81);
            args = new Type[]{typeof(System.String), typeof(System.Object)};
            method = type.GetMethod("Write", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Write_82);
            args = new Type[]{typeof(System.String), typeof(System.Object), typeof(System.Object)};
            method = type.GetMethod("Write", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Write_83);
            args = new Type[]{typeof(System.String), typeof(System.Object), typeof(System.Object), typeof(System.Object)};
            method = type.GetMethod("Write", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Write_84);
            args = new Type[]{typeof(System.String), typeof(System.Object), typeof(System.Object), typeof(System.Object), typeof(System.Object)};
            method = type.GetMethod("Write", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Write_85);
            args = new Type[]{typeof(System.String), typeof(System.Object[])};
            method = type.GetMethod("Write", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Write_86);
            args = new Type[]{typeof(System.Boolean)};
            method = type.GetMethod("Write", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Write_87);
            args = new Type[]{typeof(System.Char)};
            method = type.GetMethod("Write", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Write_88);
            args = new Type[]{typeof(System.Char[])};
            method = type.GetMethod("Write", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Write_89);
            args = new Type[]{typeof(System.Char[]), typeof(System.Int32), typeof(System.Int32)};
            method = type.GetMethod("Write", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Write_90);
            args = new Type[]{typeof(System.Double)};
            method = type.GetMethod("Write", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Write_91);
            args = new Type[]{typeof(System.Decimal)};
            method = type.GetMethod("Write", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Write_92);
            args = new Type[]{typeof(System.Single)};
            method = type.GetMethod("Write", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Write_93);
            args = new Type[]{typeof(System.Int32)};
            method = type.GetMethod("Write", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Write_94);
            args = new Type[]{typeof(System.UInt32)};
            method = type.GetMethod("Write", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Write_95);
            args = new Type[]{typeof(System.Int64)};
            method = type.GetMethod("Write", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Write_96);
            args = new Type[]{typeof(System.UInt64)};
            method = type.GetMethod("Write", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Write_97);
            args = new Type[]{typeof(System.Object)};
            method = type.GetMethod("Write", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Write_98);
            args = new Type[]{typeof(System.String)};
            method = type.GetMethod("Write", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Write_99);

        }

        static StackObject* get_Error_0(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 0);

            var result = System.Console.Error;
            return ILIntepreter.PushObject(ret, mStack, result);
        }

        static StackObject* get_In_1(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 0);

            var result = System.Console.In;
            return ILIntepreter.PushObject(ret, mStack, result);
        }

        static StackObject* get_Out_2(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 0);

            var result = System.Console.Out;
            return ILIntepreter.PushObject(ret, mStack, result);
        }

        static StackObject* get_InputEncoding_3(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 0);

            var result = System.Console.InputEncoding;
            return ILIntepreter.PushObject(ret, mStack, result);
        }

        static StackObject* set_InputEncoding_4(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.Text.Encoding value = (System.Text.Encoding)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            System.Console.InputEncoding = value;
            return ret;
        }

        static StackObject* get_OutputEncoding_5(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 0);

            var result = System.Console.OutputEncoding;
            return ILIntepreter.PushObject(ret, mStack, result);
        }

        static StackObject* set_OutputEncoding_6(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.Text.Encoding value = (System.Text.Encoding)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            System.Console.OutputEncoding = value;
            return ret;
        }

        static StackObject* Beep_7(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 0);

            System.Console.Beep();
            return ret;
        }

        static StackObject* Beep_8(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 frequency = p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Int32 duration = p->Value;

            System.Console.Beep(frequency, duration);
            return ret;
        }

        static StackObject* Clear_9(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 0);

            System.Console.Clear();
            return ret;
        }

        static StackObject* get_BackgroundColor_10(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 0);

            var result = System.Console.BackgroundColor;
            return ILIntepreter.PushObject(ret, mStack, result);
        }

        static StackObject* set_BackgroundColor_11(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.ConsoleColor value = (System.ConsoleColor)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            System.Console.BackgroundColor = value;
            return ret;
        }

        static StackObject* get_ForegroundColor_12(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 0);

            var result = System.Console.ForegroundColor;
            return ILIntepreter.PushObject(ret, mStack, result);
        }

        static StackObject* set_ForegroundColor_13(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.ConsoleColor value = (System.ConsoleColor)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            System.Console.ForegroundColor = value;
            return ret;
        }

        static StackObject* ResetColor_14(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 0);

            System.Console.ResetColor();
            return ret;
        }

        static StackObject* MoveBufferArea_15(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 6);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 sourceLeft = p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Int32 sourceTop = p->Value;
            p = ILIntepreter.Minus(esp, 3);
            System.Int32 sourceWidth = p->Value;
            p = ILIntepreter.Minus(esp, 4);
            System.Int32 sourceHeight = p->Value;
            p = ILIntepreter.Minus(esp, 5);
            System.Int32 targetLeft = p->Value;
            p = ILIntepreter.Minus(esp, 6);
            System.Int32 targetTop = p->Value;

            System.Console.MoveBufferArea(sourceLeft, sourceTop, sourceWidth, sourceHeight, targetLeft, targetTop);
            return ret;
        }

        static StackObject* MoveBufferArea_16(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 9);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 sourceLeft = p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Int32 sourceTop = p->Value;
            p = ILIntepreter.Minus(esp, 3);
            System.Int32 sourceWidth = p->Value;
            p = ILIntepreter.Minus(esp, 4);
            System.Int32 sourceHeight = p->Value;
            p = ILIntepreter.Minus(esp, 5);
            System.Int32 targetLeft = p->Value;
            p = ILIntepreter.Minus(esp, 6);
            System.Int32 targetTop = p->Value;
            p = ILIntepreter.Minus(esp, 7);
            System.Char sourceChar = (char)p->Value;
            p = ILIntepreter.Minus(esp, 8);
            System.ConsoleColor sourceForeColor = (System.ConsoleColor)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 9);
            System.ConsoleColor sourceBackColor = (System.ConsoleColor)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            System.Console.MoveBufferArea(sourceLeft, sourceTop, sourceWidth, sourceHeight, targetLeft, targetTop, sourceChar, sourceForeColor, sourceBackColor);
            return ret;
        }

        static StackObject* get_BufferHeight_17(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 0);

            var result = System.Console.BufferHeight;
            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result;
            return ret + 1;
        }

        static StackObject* set_BufferHeight_18(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 value = p->Value;

            System.Console.BufferHeight = value;
            return ret;
        }

        static StackObject* get_BufferWidth_19(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 0);

            var result = System.Console.BufferWidth;
            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result;
            return ret + 1;
        }

        static StackObject* set_BufferWidth_20(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 value = p->Value;

            System.Console.BufferWidth = value;
            return ret;
        }

        static StackObject* SetBufferSize_21(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 width = p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Int32 height = p->Value;

            System.Console.SetBufferSize(width, height);
            return ret;
        }

        static StackObject* get_WindowHeight_22(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 0);

            var result = System.Console.WindowHeight;
            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result;
            return ret + 1;
        }

        static StackObject* set_WindowHeight_23(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 value = p->Value;

            System.Console.WindowHeight = value;
            return ret;
        }

        static StackObject* get_WindowWidth_24(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 0);

            var result = System.Console.WindowWidth;
            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result;
            return ret + 1;
        }

        static StackObject* set_WindowWidth_25(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 value = p->Value;

            System.Console.WindowWidth = value;
            return ret;
        }

        static StackObject* SetWindowSize_26(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 width = p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Int32 height = p->Value;

            System.Console.SetWindowSize(width, height);
            return ret;
        }

        static StackObject* get_LargestWindowWidth_27(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 0);

            var result = System.Console.LargestWindowWidth;
            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result;
            return ret + 1;
        }

        static StackObject* get_LargestWindowHeight_28(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 0);

            var result = System.Console.LargestWindowHeight;
            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result;
            return ret + 1;
        }

        static StackObject* get_WindowLeft_29(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 0);

            var result = System.Console.WindowLeft;
            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result;
            return ret + 1;
        }

        static StackObject* set_WindowLeft_30(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 value = p->Value;

            System.Console.WindowLeft = value;
            return ret;
        }

        static StackObject* get_WindowTop_31(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 0);

            var result = System.Console.WindowTop;
            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result;
            return ret + 1;
        }

        static StackObject* set_WindowTop_32(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 value = p->Value;

            System.Console.WindowTop = value;
            return ret;
        }

        static StackObject* SetWindowPosition_33(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 left = p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Int32 top = p->Value;

            System.Console.SetWindowPosition(left, top);
            return ret;
        }

        static StackObject* get_CursorLeft_34(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 0);

            var result = System.Console.CursorLeft;
            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result;
            return ret + 1;
        }

        static StackObject* set_CursorLeft_35(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 value = p->Value;

            System.Console.CursorLeft = value;
            return ret;
        }

        static StackObject* get_CursorTop_36(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 0);

            var result = System.Console.CursorTop;
            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result;
            return ret + 1;
        }

        static StackObject* set_CursorTop_37(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 value = p->Value;

            System.Console.CursorTop = value;
            return ret;
        }

        static StackObject* SetCursorPosition_38(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 left = p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Int32 top = p->Value;

            System.Console.SetCursorPosition(left, top);
            return ret;
        }

        static StackObject* get_CursorSize_39(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 0);

            var result = System.Console.CursorSize;
            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result;
            return ret + 1;
        }

        static StackObject* set_CursorSize_40(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 value = p->Value;

            System.Console.CursorSize = value;
            return ret;
        }

        static StackObject* get_CursorVisible_41(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 0);

            var result = System.Console.CursorVisible;
            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result ? 1 : 0;
            return ret + 1;
        }

        static StackObject* set_CursorVisible_42(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.Boolean value = p->Value == 1;

            System.Console.CursorVisible = value;
            return ret;
        }

        static StackObject* get_Title_43(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 0);

            var result = System.Console.Title;
            return ILIntepreter.PushObject(ret, mStack, result);
        }

        static StackObject* set_Title_44(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.String value = (System.String)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            System.Console.Title = value;
            return ret;
        }

        static StackObject* ReadKey_45(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 0);

            var result = System.Console.ReadKey();
            return ILIntepreter.PushObject(ret, mStack, result);
        }

        static StackObject* ReadKey_46(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.Boolean intercept = p->Value == 1;

            var result = System.Console.ReadKey(intercept);
            return ILIntepreter.PushObject(ret, mStack, result);
        }

        static StackObject* get_KeyAvailable_47(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 0);

            var result = System.Console.KeyAvailable;
            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result ? 1 : 0;
            return ret + 1;
        }

        static StackObject* get_NumberLock_48(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 0);

            var result = System.Console.NumberLock;
            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result ? 1 : 0;
            return ret + 1;
        }

        static StackObject* get_CapsLock_49(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 0);

            var result = System.Console.CapsLock;
            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result ? 1 : 0;
            return ret + 1;
        }

        static StackObject* get_TreatControlCAsInput_50(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 0);

            var result = System.Console.TreatControlCAsInput;
            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result ? 1 : 0;
            return ret + 1;
        }

        static StackObject* set_TreatControlCAsInput_51(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.Boolean value = p->Value == 1;

            System.Console.TreatControlCAsInput = value;
            return ret;
        }

        static StackObject* OpenStandardError_52(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 0);

            var result = System.Console.OpenStandardError();
            return ILIntepreter.PushObject(ret, mStack, result);
        }

        static StackObject* OpenStandardError_53(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 bufferSize = p->Value;

            var result = System.Console.OpenStandardError(bufferSize);
            return ILIntepreter.PushObject(ret, mStack, result);
        }

        static StackObject* OpenStandardInput_54(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 0);

            var result = System.Console.OpenStandardInput();
            return ILIntepreter.PushObject(ret, mStack, result);
        }

        static StackObject* OpenStandardInput_55(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 bufferSize = p->Value;

            var result = System.Console.OpenStandardInput(bufferSize);
            return ILIntepreter.PushObject(ret, mStack, result);
        }

        static StackObject* OpenStandardOutput_56(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 0);

            var result = System.Console.OpenStandardOutput();
            return ILIntepreter.PushObject(ret, mStack, result);
        }

        static StackObject* OpenStandardOutput_57(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 bufferSize = p->Value;

            var result = System.Console.OpenStandardOutput(bufferSize);
            return ILIntepreter.PushObject(ret, mStack, result);
        }

        static StackObject* SetIn_58(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.IO.TextReader newIn = (System.IO.TextReader)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            System.Console.SetIn(newIn);
            return ret;
        }

        static StackObject* SetOut_59(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.IO.TextWriter newOut = (System.IO.TextWriter)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            System.Console.SetOut(newOut);
            return ret;
        }

        static StackObject* SetError_60(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.IO.TextWriter newError = (System.IO.TextWriter)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            System.Console.SetError(newError);
            return ret;
        }

        static StackObject* Read_61(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 0);

            var result = System.Console.Read();
            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result;
            return ret + 1;
        }

        static StackObject* ReadLine_62(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 0);

            var result = System.Console.ReadLine();
            return ILIntepreter.PushObject(ret, mStack, result);
        }

        static StackObject* WriteLine_63(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 0);

            System.Console.WriteLine();
            return ret;
        }

        static StackObject* WriteLine_64(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.Boolean value = p->Value == 1;

            System.Console.WriteLine(value);
            return ret;
        }

        static StackObject* WriteLine_65(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.Char value = (char)p->Value;

            System.Console.WriteLine(value);
            return ret;
        }

        static StackObject* WriteLine_66(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.Char[] buffer = (System.Char[])StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            System.Console.WriteLine(buffer);
            return ret;
        }

        static StackObject* WriteLine_67(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 3);
            p = ILIntepreter.Minus(esp, 1);
            System.Char[] buffer = (System.Char[])StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Int32 index = p->Value;
            p = ILIntepreter.Minus(esp, 3);
            System.Int32 count = p->Value;

            System.Console.WriteLine(buffer, index, count);
            return ret;
        }

        static StackObject* WriteLine_68(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.Decimal value = (System.Decimal)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            System.Console.WriteLine(value);
            return ret;
        }

        static StackObject* WriteLine_69(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.Double value = *(double*)&p->Value;

            System.Console.WriteLine(value);
            return ret;
        }

        static StackObject* WriteLine_70(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.Single value = *(float*)&p->Value;

            System.Console.WriteLine(value);
            return ret;
        }

        static StackObject* WriteLine_71(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 value = p->Value;

            System.Console.WriteLine(value);
            return ret;
        }

        static StackObject* WriteLine_72(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.UInt32 value = (uint)p->Value;

            System.Console.WriteLine(value);
            return ret;
        }

        static StackObject* WriteLine_73(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.Int64 value = *(long*)&p->Value;

            System.Console.WriteLine(value);
            return ret;
        }

        static StackObject* WriteLine_74(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.UInt64 value = *(ulong*)&p->Value;

            System.Console.WriteLine(value);
            return ret;
        }

        static StackObject* WriteLine_75(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.Object value = (System.Object)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            System.Console.WriteLine(value);
            return ret;
        }

        static StackObject* WriteLine_76(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.String value = (System.String)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            System.Console.WriteLine(value);
            return ret;
        }

        static StackObject* WriteLine_77(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.String format = (System.String)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Object arg0 = (System.Object)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            System.Console.WriteLine(format, arg0);
            return ret;
        }

        static StackObject* WriteLine_78(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 3);
            p = ILIntepreter.Minus(esp, 1);
            System.String format = (System.String)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Object arg0 = (System.Object)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 3);
            System.Object arg1 = (System.Object)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            System.Console.WriteLine(format, arg0, arg1);
            return ret;
        }

        static StackObject* WriteLine_79(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 4);
            p = ILIntepreter.Minus(esp, 1);
            System.String format = (System.String)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Object arg0 = (System.Object)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 3);
            System.Object arg1 = (System.Object)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 4);
            System.Object arg2 = (System.Object)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            System.Console.WriteLine(format, arg0, arg1, arg2);
            return ret;
        }

        static StackObject* WriteLine_80(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 5);
            p = ILIntepreter.Minus(esp, 1);
            System.String format = (System.String)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Object arg0 = (System.Object)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 3);
            System.Object arg1 = (System.Object)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 4);
            System.Object arg2 = (System.Object)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 5);
            System.Object arg3 = (System.Object)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            System.Console.WriteLine(format, arg0, arg1, arg2, arg3);
            return ret;
        }

        static StackObject* WriteLine_81(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.String format = (System.String)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Object[] arg = (System.Object[])StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            System.Console.WriteLine(format, arg);
            return ret;
        }

        static StackObject* Write_82(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.String format = (System.String)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Object arg0 = (System.Object)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            System.Console.Write(format, arg0);
            return ret;
        }

        static StackObject* Write_83(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 3);
            p = ILIntepreter.Minus(esp, 1);
            System.String format = (System.String)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Object arg0 = (System.Object)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 3);
            System.Object arg1 = (System.Object)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            System.Console.Write(format, arg0, arg1);
            return ret;
        }

        static StackObject* Write_84(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 4);
            p = ILIntepreter.Minus(esp, 1);
            System.String format = (System.String)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Object arg0 = (System.Object)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 3);
            System.Object arg1 = (System.Object)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 4);
            System.Object arg2 = (System.Object)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            System.Console.Write(format, arg0, arg1, arg2);
            return ret;
        }

        static StackObject* Write_85(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 5);
            p = ILIntepreter.Minus(esp, 1);
            System.String format = (System.String)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Object arg0 = (System.Object)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 3);
            System.Object arg1 = (System.Object)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 4);
            System.Object arg2 = (System.Object)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 5);
            System.Object arg3 = (System.Object)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            System.Console.Write(format, arg0, arg1, arg2, arg3);
            return ret;
        }

        static StackObject* Write_86(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.String format = (System.String)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Object[] arg = (System.Object[])StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            System.Console.Write(format, arg);
            return ret;
        }

        static StackObject* Write_87(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.Boolean value = p->Value == 1;

            System.Console.Write(value);
            return ret;
        }

        static StackObject* Write_88(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.Char value = (char)p->Value;

            System.Console.Write(value);
            return ret;
        }

        static StackObject* Write_89(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.Char[] buffer = (System.Char[])StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            System.Console.Write(buffer);
            return ret;
        }

        static StackObject* Write_90(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 3);
            p = ILIntepreter.Minus(esp, 1);
            System.Char[] buffer = (System.Char[])StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Int32 index = p->Value;
            p = ILIntepreter.Minus(esp, 3);
            System.Int32 count = p->Value;

            System.Console.Write(buffer, index, count);
            return ret;
        }

        static StackObject* Write_91(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.Double value = *(double*)&p->Value;

            System.Console.Write(value);
            return ret;
        }

        static StackObject* Write_92(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.Decimal value = (System.Decimal)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            System.Console.Write(value);
            return ret;
        }

        static StackObject* Write_93(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.Single value = *(float*)&p->Value;

            System.Console.Write(value);
            return ret;
        }

        static StackObject* Write_94(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 value = p->Value;

            System.Console.Write(value);
            return ret;
        }

        static StackObject* Write_95(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.UInt32 value = (uint)p->Value;

            System.Console.Write(value);
            return ret;
        }

        static StackObject* Write_96(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.Int64 value = *(long*)&p->Value;

            System.Console.Write(value);
            return ret;
        }

        static StackObject* Write_97(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.UInt64 value = *(ulong*)&p->Value;

            System.Console.Write(value);
            return ret;
        }

        static StackObject* Write_98(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.Object value = (System.Object)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            System.Console.Write(value);
            return ret;
        }

        static StackObject* Write_99(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.String value = (System.String)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            System.Console.Write(value);
            return ret;
        }


    }
}
