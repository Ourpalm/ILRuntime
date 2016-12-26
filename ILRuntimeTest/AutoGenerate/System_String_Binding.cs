using System;
using System.Collections.Generic;
using System.Reflection;

using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
using ILRuntime.Reflection;
using ILRuntime.CLR.Utils;

namespace ILRuntime.Runtime.Generated
{
    unsafe class System_String_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodInfo method;
            Type[] args;
            Type type = typeof(System.String);
            args = new Type[]{typeof(System.String), typeof(System.String[])};
            method = type.GetMethod("Join", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Join_0);
            args = new Type[]{typeof(System.String), typeof(System.String[]), typeof(System.Int32), typeof(System.Int32)};
            method = type.GetMethod("Join", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Join_1);
            args = new Type[]{typeof(System.Object)};
            method = type.GetMethod("Equals", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Equals_2);
            args = new Type[]{typeof(System.String)};
            method = type.GetMethod("Equals", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Equals_3);
            args = new Type[]{typeof(System.String), typeof(System.StringComparison)};
            method = type.GetMethod("Equals", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Equals_4);
            args = new Type[]{typeof(System.String), typeof(System.String)};
            method = type.GetMethod("Equals", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Equals_5);
            args = new Type[]{typeof(System.String), typeof(System.String), typeof(System.StringComparison)};
            method = type.GetMethod("Equals", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Equals_6);
            args = new Type[]{typeof(System.String), typeof(System.String)};
            method = type.GetMethod("op_Equality", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, op_Equality_7);
            args = new Type[]{typeof(System.String), typeof(System.String)};
            method = type.GetMethod("op_Inequality", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, op_Inequality_8);
            args = new Type[]{typeof(System.Int32)};
            method = type.GetMethod("get_Chars", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_Chars_9);
            args = new Type[]{typeof(System.Int32), typeof(System.Char[]), typeof(System.Int32), typeof(System.Int32)};
            method = type.GetMethod("CopyTo", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, CopyTo_10);
            args = new Type[]{};
            method = type.GetMethod("ToCharArray", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, ToCharArray_11);
            args = new Type[]{typeof(System.Int32), typeof(System.Int32)};
            method = type.GetMethod("ToCharArray", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, ToCharArray_12);
            args = new Type[]{typeof(System.String)};
            method = type.GetMethod("IsNullOrEmpty", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, IsNullOrEmpty_13);
            args = new Type[]{};
            method = type.GetMethod("GetHashCode", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, GetHashCode_14);
            args = new Type[]{};
            method = type.GetMethod("get_Length", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_Length_15);
            args = new Type[]{typeof(System.Char[])};
            method = type.GetMethod("Split", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Split_16);
            args = new Type[]{typeof(System.Char[]), typeof(System.Int32)};
            method = type.GetMethod("Split", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Split_17);
            args = new Type[]{typeof(System.Char[]), typeof(System.StringSplitOptions)};
            method = type.GetMethod("Split", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Split_18);
            args = new Type[]{typeof(System.Char[]), typeof(System.Int32), typeof(System.StringSplitOptions)};
            method = type.GetMethod("Split", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Split_19);
            args = new Type[]{typeof(System.String[]), typeof(System.StringSplitOptions)};
            method = type.GetMethod("Split", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Split_20);
            args = new Type[]{typeof(System.String[]), typeof(System.Int32), typeof(System.StringSplitOptions)};
            method = type.GetMethod("Split", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Split_21);
            args = new Type[]{typeof(System.Int32)};
            method = type.GetMethod("Substring", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Substring_22);
            args = new Type[]{typeof(System.Int32), typeof(System.Int32)};
            method = type.GetMethod("Substring", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Substring_23);
            args = new Type[]{typeof(System.Char[])};
            method = type.GetMethod("Trim", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Trim_24);
            args = new Type[]{};
            method = type.GetMethod("Trim", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Trim_25);
            args = new Type[]{typeof(System.Char[])};
            method = type.GetMethod("TrimStart", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, TrimStart_26);
            args = new Type[]{typeof(System.Char[])};
            method = type.GetMethod("TrimEnd", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, TrimEnd_27);
            args = new Type[]{};
            method = type.GetMethod("IsNormalized", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, IsNormalized_28);
            args = new Type[]{typeof(System.Text.NormalizationForm)};
            method = type.GetMethod("IsNormalized", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, IsNormalized_29);
            args = new Type[]{};
            method = type.GetMethod("Normalize", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Normalize_30);
            args = new Type[]{typeof(System.Text.NormalizationForm)};
            method = type.GetMethod("Normalize", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Normalize_31);
            args = new Type[]{typeof(System.String), typeof(System.String)};
            method = type.GetMethod("Compare", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Compare_32);
            args = new Type[]{typeof(System.String), typeof(System.String), typeof(System.Boolean)};
            method = type.GetMethod("Compare", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Compare_33);
            args = new Type[]{typeof(System.String), typeof(System.String), typeof(System.Globalization.CultureInfo), typeof(System.Globalization.CompareOptions)};
            method = type.GetMethod("Compare", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Compare_34);
            args = new Type[]{typeof(System.String), typeof(System.Int32), typeof(System.String), typeof(System.Int32), typeof(System.Int32), typeof(System.Globalization.CultureInfo), typeof(System.Globalization.CompareOptions)};
            method = type.GetMethod("Compare", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Compare_35);
            args = new Type[]{typeof(System.String), typeof(System.String), typeof(System.StringComparison)};
            method = type.GetMethod("Compare", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Compare_36);
            args = new Type[]{typeof(System.String), typeof(System.String), typeof(System.Boolean), typeof(System.Globalization.CultureInfo)};
            method = type.GetMethod("Compare", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Compare_37);
            args = new Type[]{typeof(System.String), typeof(System.Int32), typeof(System.String), typeof(System.Int32), typeof(System.Int32)};
            method = type.GetMethod("Compare", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Compare_38);
            args = new Type[]{typeof(System.String), typeof(System.Int32), typeof(System.String), typeof(System.Int32), typeof(System.Int32), typeof(System.Boolean)};
            method = type.GetMethod("Compare", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Compare_39);
            args = new Type[]{typeof(System.String), typeof(System.Int32), typeof(System.String), typeof(System.Int32), typeof(System.Int32), typeof(System.Boolean), typeof(System.Globalization.CultureInfo)};
            method = type.GetMethod("Compare", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Compare_40);
            args = new Type[]{typeof(System.String), typeof(System.Int32), typeof(System.String), typeof(System.Int32), typeof(System.Int32), typeof(System.StringComparison)};
            method = type.GetMethod("Compare", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Compare_41);
            args = new Type[]{typeof(System.Object)};
            method = type.GetMethod("CompareTo", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, CompareTo_42);
            args = new Type[]{typeof(System.String)};
            method = type.GetMethod("CompareTo", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, CompareTo_43);
            args = new Type[]{typeof(System.String), typeof(System.String)};
            method = type.GetMethod("CompareOrdinal", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, CompareOrdinal_44);
            args = new Type[]{typeof(System.String), typeof(System.Int32), typeof(System.String), typeof(System.Int32), typeof(System.Int32)};
            method = type.GetMethod("CompareOrdinal", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, CompareOrdinal_45);
            args = new Type[]{typeof(System.String)};
            method = type.GetMethod("Contains", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Contains_46);
            args = new Type[]{typeof(System.String)};
            method = type.GetMethod("EndsWith", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, EndsWith_47);
            args = new Type[]{typeof(System.String), typeof(System.StringComparison)};
            method = type.GetMethod("EndsWith", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, EndsWith_48);
            args = new Type[]{typeof(System.String), typeof(System.Boolean), typeof(System.Globalization.CultureInfo)};
            method = type.GetMethod("EndsWith", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, EndsWith_49);
            args = new Type[]{typeof(System.Char)};
            method = type.GetMethod("IndexOf", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, IndexOf_50);
            args = new Type[]{typeof(System.Char), typeof(System.Int32)};
            method = type.GetMethod("IndexOf", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, IndexOf_51);
            args = new Type[]{typeof(System.Char), typeof(System.Int32), typeof(System.Int32)};
            method = type.GetMethod("IndexOf", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, IndexOf_52);
            args = new Type[]{typeof(System.String)};
            method = type.GetMethod("IndexOf", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, IndexOf_53);
            args = new Type[]{typeof(System.String), typeof(System.Int32)};
            method = type.GetMethod("IndexOf", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, IndexOf_54);
            args = new Type[]{typeof(System.String), typeof(System.Int32), typeof(System.Int32)};
            method = type.GetMethod("IndexOf", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, IndexOf_55);
            args = new Type[]{typeof(System.String), typeof(System.StringComparison)};
            method = type.GetMethod("IndexOf", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, IndexOf_56);
            args = new Type[]{typeof(System.String), typeof(System.Int32), typeof(System.StringComparison)};
            method = type.GetMethod("IndexOf", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, IndexOf_57);
            args = new Type[]{typeof(System.String), typeof(System.Int32), typeof(System.Int32), typeof(System.StringComparison)};
            method = type.GetMethod("IndexOf", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, IndexOf_58);
            args = new Type[]{typeof(System.Char[])};
            method = type.GetMethod("IndexOfAny", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, IndexOfAny_59);
            args = new Type[]{typeof(System.Char[]), typeof(System.Int32)};
            method = type.GetMethod("IndexOfAny", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, IndexOfAny_60);
            args = new Type[]{typeof(System.Char[]), typeof(System.Int32), typeof(System.Int32)};
            method = type.GetMethod("IndexOfAny", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, IndexOfAny_61);
            args = new Type[]{typeof(System.Char)};
            method = type.GetMethod("LastIndexOf", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, LastIndexOf_62);
            args = new Type[]{typeof(System.Char), typeof(System.Int32)};
            method = type.GetMethod("LastIndexOf", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, LastIndexOf_63);
            args = new Type[]{typeof(System.Char), typeof(System.Int32), typeof(System.Int32)};
            method = type.GetMethod("LastIndexOf", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, LastIndexOf_64);
            args = new Type[]{typeof(System.String)};
            method = type.GetMethod("LastIndexOf", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, LastIndexOf_65);
            args = new Type[]{typeof(System.String), typeof(System.Int32)};
            method = type.GetMethod("LastIndexOf", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, LastIndexOf_66);
            args = new Type[]{typeof(System.String), typeof(System.Int32), typeof(System.Int32)};
            method = type.GetMethod("LastIndexOf", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, LastIndexOf_67);
            args = new Type[]{typeof(System.String), typeof(System.StringComparison)};
            method = type.GetMethod("LastIndexOf", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, LastIndexOf_68);
            args = new Type[]{typeof(System.String), typeof(System.Int32), typeof(System.StringComparison)};
            method = type.GetMethod("LastIndexOf", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, LastIndexOf_69);
            args = new Type[]{typeof(System.String), typeof(System.Int32), typeof(System.Int32), typeof(System.StringComparison)};
            method = type.GetMethod("LastIndexOf", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, LastIndexOf_70);
            args = new Type[]{typeof(System.Char[])};
            method = type.GetMethod("LastIndexOfAny", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, LastIndexOfAny_71);
            args = new Type[]{typeof(System.Char[]), typeof(System.Int32)};
            method = type.GetMethod("LastIndexOfAny", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, LastIndexOfAny_72);
            args = new Type[]{typeof(System.Char[]), typeof(System.Int32), typeof(System.Int32)};
            method = type.GetMethod("LastIndexOfAny", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, LastIndexOfAny_73);
            args = new Type[]{typeof(System.Int32)};
            method = type.GetMethod("PadLeft", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, PadLeft_74);
            args = new Type[]{typeof(System.Int32), typeof(System.Char)};
            method = type.GetMethod("PadLeft", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, PadLeft_75);
            args = new Type[]{typeof(System.Int32)};
            method = type.GetMethod("PadRight", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, PadRight_76);
            args = new Type[]{typeof(System.Int32), typeof(System.Char)};
            method = type.GetMethod("PadRight", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, PadRight_77);
            args = new Type[]{typeof(System.String)};
            method = type.GetMethod("StartsWith", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, StartsWith_78);
            args = new Type[]{typeof(System.String), typeof(System.StringComparison)};
            method = type.GetMethod("StartsWith", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, StartsWith_79);
            args = new Type[]{typeof(System.String), typeof(System.Boolean), typeof(System.Globalization.CultureInfo)};
            method = type.GetMethod("StartsWith", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, StartsWith_80);
            args = new Type[]{};
            method = type.GetMethod("ToLower", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, ToLower_81);
            args = new Type[]{typeof(System.Globalization.CultureInfo)};
            method = type.GetMethod("ToLower", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, ToLower_82);
            args = new Type[]{};
            method = type.GetMethod("ToLowerInvariant", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, ToLowerInvariant_83);
            args = new Type[]{};
            method = type.GetMethod("ToUpper", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, ToUpper_84);
            args = new Type[]{typeof(System.Globalization.CultureInfo)};
            method = type.GetMethod("ToUpper", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, ToUpper_85);
            args = new Type[]{};
            method = type.GetMethod("ToUpperInvariant", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, ToUpperInvariant_86);
            args = new Type[]{};
            method = type.GetMethod("ToString", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, ToString_87);
            args = new Type[]{typeof(System.IFormatProvider)};
            method = type.GetMethod("ToString", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, ToString_88);
            args = new Type[]{};
            method = type.GetMethod("Clone", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Clone_89);
            args = new Type[]{typeof(System.Int32), typeof(System.String)};
            method = type.GetMethod("Insert", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Insert_90);
            args = new Type[]{typeof(System.Char), typeof(System.Char)};
            method = type.GetMethod("Replace", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Replace_91);
            args = new Type[]{typeof(System.String), typeof(System.String)};
            method = type.GetMethod("Replace", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Replace_92);
            args = new Type[]{typeof(System.Int32), typeof(System.Int32)};
            method = type.GetMethod("Remove", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Remove_93);
            args = new Type[]{typeof(System.Int32)};
            method = type.GetMethod("Remove", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Remove_94);
            args = new Type[]{typeof(System.String), typeof(System.Object)};
            method = type.GetMethod("Format", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Format_95);
            args = new Type[]{typeof(System.String), typeof(System.Object), typeof(System.Object)};
            method = type.GetMethod("Format", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Format_96);
            args = new Type[]{typeof(System.String), typeof(System.Object), typeof(System.Object), typeof(System.Object)};
            method = type.GetMethod("Format", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Format_97);
            args = new Type[]{typeof(System.String), typeof(System.Object[])};
            method = type.GetMethod("Format", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Format_98);
            args = new Type[]{typeof(System.IFormatProvider), typeof(System.String), typeof(System.Object[])};
            method = type.GetMethod("Format", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Format_99);
            args = new Type[]{typeof(System.String)};
            method = type.GetMethod("Copy", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Copy_100);
            args = new Type[]{typeof(System.Object)};
            method = type.GetMethod("Concat", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Concat_101);
            args = new Type[]{typeof(System.Object), typeof(System.Object)};
            method = type.GetMethod("Concat", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Concat_102);
            args = new Type[]{typeof(System.Object), typeof(System.Object), typeof(System.Object)};
            method = type.GetMethod("Concat", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Concat_103);
            args = new Type[]{typeof(System.Object), typeof(System.Object), typeof(System.Object), typeof(System.Object)};
            method = type.GetMethod("Concat", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Concat_104);
            args = new Type[]{typeof(System.Object[])};
            method = type.GetMethod("Concat", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Concat_105);
            args = new Type[]{typeof(System.String), typeof(System.String)};
            method = type.GetMethod("Concat", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Concat_106);
            args = new Type[]{typeof(System.String), typeof(System.String), typeof(System.String)};
            method = type.GetMethod("Concat", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Concat_107);
            args = new Type[]{typeof(System.String), typeof(System.String), typeof(System.String), typeof(System.String)};
            method = type.GetMethod("Concat", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Concat_108);
            args = new Type[]{typeof(System.String[])};
            method = type.GetMethod("Concat", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Concat_109);
            args = new Type[]{typeof(System.String)};
            method = type.GetMethod("Intern", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Intern_110);
            args = new Type[]{typeof(System.String)};
            method = type.GetMethod("IsInterned", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, IsInterned_111);
            args = new Type[]{};
            method = type.GetMethod("GetTypeCode", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, GetTypeCode_112);
            args = new Type[]{};
            method = type.GetMethod("GetEnumerator", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, GetEnumerator_113);

        }

        static StackObject* Join_0(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.String[] value = (System.String[])typeof(System.String[]).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.String separator = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = System.String.Join(separator, value);

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* Join_1(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 4);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 count = p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Int32 startIndex = p->Value;
            p = ILIntepreter.Minus(esp, 3);
            System.String[] value = (System.String[])typeof(System.String[]).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 4);
            System.String separator = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = System.String.Join(separator, value, startIndex, count);

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* Equals_2(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.Object obj = (System.Object)typeof(System.Object).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.Equals(obj);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method ? 1 : 0;
            return ret + 1;
        }

        static StackObject* Equals_3(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.String value = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.Equals(value);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method ? 1 : 0;
            return ret + 1;
        }

        static StackObject* Equals_4(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 3);
            p = ILIntepreter.Minus(esp, 1);
            System.StringComparison comparisonType = (System.StringComparison)typeof(System.StringComparison).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.String value = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 3);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.Equals(value, comparisonType);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method ? 1 : 0;
            return ret + 1;
        }

        static StackObject* Equals_5(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.String b = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.String a = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = System.String.Equals(a, b);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method ? 1 : 0;
            return ret + 1;
        }

        static StackObject* Equals_6(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 3);
            p = ILIntepreter.Minus(esp, 1);
            System.StringComparison comparisonType = (System.StringComparison)typeof(System.StringComparison).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.String b = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 3);
            System.String a = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = System.String.Equals(a, b, comparisonType);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method ? 1 : 0;
            return ret + 1;
        }

        static StackObject* op_Equality_7(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.String b = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.String a = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = a == b;

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method ? 1 : 0;
            return ret + 1;
        }

        static StackObject* op_Inequality_8(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.String b = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.String a = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = a != b;

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method ? 1 : 0;
            return ret + 1;
        }

        static StackObject* get_Chars_9(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 index = p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method[index];

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = (int)result_of_this_method;
            return ret + 1;
        }

        static StackObject* CopyTo_10(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 5);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 count = p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Int32 destinationIndex = p->Value;
            p = ILIntepreter.Minus(esp, 3);
            System.Char[] destination = (System.Char[])typeof(System.Char[]).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 4);
            System.Int32 sourceIndex = p->Value;
            p = ILIntepreter.Minus(esp, 5);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            instance_of_this_method.CopyTo(sourceIndex, destination, destinationIndex, count);

            return ret;
        }

        static StackObject* ToCharArray_11(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.ToCharArray();

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* ToCharArray_12(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 3);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 length = p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Int32 startIndex = p->Value;
            p = ILIntepreter.Minus(esp, 3);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.ToCharArray(startIndex, length);

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* IsNullOrEmpty_13(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.String value = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = System.String.IsNullOrEmpty(value);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method ? 1 : 0;
            return ret + 1;
        }

        static StackObject* GetHashCode_14(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.GetHashCode();

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* get_Length_15(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.Length;

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* Split_16(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.Char[] separator = (System.Char[])typeof(System.Char[]).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.Split(separator);

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* Split_17(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 3);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 count = p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Char[] separator = (System.Char[])typeof(System.Char[]).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 3);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.Split(separator, count);

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* Split_18(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 3);
            p = ILIntepreter.Minus(esp, 1);
            System.StringSplitOptions options = (System.StringSplitOptions)typeof(System.StringSplitOptions).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Char[] separator = (System.Char[])typeof(System.Char[]).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 3);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.Split(separator, options);

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* Split_19(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 4);
            p = ILIntepreter.Minus(esp, 1);
            System.StringSplitOptions options = (System.StringSplitOptions)typeof(System.StringSplitOptions).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Int32 count = p->Value;
            p = ILIntepreter.Minus(esp, 3);
            System.Char[] separator = (System.Char[])typeof(System.Char[]).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 4);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.Split(separator, count, options);

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* Split_20(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 3);
            p = ILIntepreter.Minus(esp, 1);
            System.StringSplitOptions options = (System.StringSplitOptions)typeof(System.StringSplitOptions).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.String[] separator = (System.String[])typeof(System.String[]).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 3);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.Split(separator, options);

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* Split_21(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 4);
            p = ILIntepreter.Minus(esp, 1);
            System.StringSplitOptions options = (System.StringSplitOptions)typeof(System.StringSplitOptions).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Int32 count = p->Value;
            p = ILIntepreter.Minus(esp, 3);
            System.String[] separator = (System.String[])typeof(System.String[]).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 4);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.Split(separator, count, options);

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* Substring_22(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 startIndex = p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.Substring(startIndex);

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* Substring_23(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 3);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 length = p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Int32 startIndex = p->Value;
            p = ILIntepreter.Minus(esp, 3);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.Substring(startIndex, length);

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* Trim_24(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.Char[] trimChars = (System.Char[])typeof(System.Char[]).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.Trim(trimChars);

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* Trim_25(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.Trim();

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* TrimStart_26(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.Char[] trimChars = (System.Char[])typeof(System.Char[]).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.TrimStart(trimChars);

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* TrimEnd_27(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.Char[] trimChars = (System.Char[])typeof(System.Char[]).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.TrimEnd(trimChars);

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* IsNormalized_28(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.IsNormalized();

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method ? 1 : 0;
            return ret + 1;
        }

        static StackObject* IsNormalized_29(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.Text.NormalizationForm normalizationForm = (System.Text.NormalizationForm)typeof(System.Text.NormalizationForm).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.IsNormalized(normalizationForm);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method ? 1 : 0;
            return ret + 1;
        }

        static StackObject* Normalize_30(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.Normalize();

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* Normalize_31(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.Text.NormalizationForm normalizationForm = (System.Text.NormalizationForm)typeof(System.Text.NormalizationForm).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.Normalize(normalizationForm);

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* Compare_32(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.String strB = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.String strA = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = System.String.Compare(strA, strB);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* Compare_33(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 3);
            p = ILIntepreter.Minus(esp, 1);
            System.Boolean ignoreCase = p->Value == 1;
            p = ILIntepreter.Minus(esp, 2);
            System.String strB = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 3);
            System.String strA = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = System.String.Compare(strA, strB, ignoreCase);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* Compare_34(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 4);
            p = ILIntepreter.Minus(esp, 1);
            System.Globalization.CompareOptions options = (System.Globalization.CompareOptions)typeof(System.Globalization.CompareOptions).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Globalization.CultureInfo culture = (System.Globalization.CultureInfo)typeof(System.Globalization.CultureInfo).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 3);
            System.String strB = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 4);
            System.String strA = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = System.String.Compare(strA, strB, culture, options);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* Compare_35(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 7);
            p = ILIntepreter.Minus(esp, 1);
            System.Globalization.CompareOptions options = (System.Globalization.CompareOptions)typeof(System.Globalization.CompareOptions).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Globalization.CultureInfo culture = (System.Globalization.CultureInfo)typeof(System.Globalization.CultureInfo).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 3);
            System.Int32 length = p->Value;
            p = ILIntepreter.Minus(esp, 4);
            System.Int32 indexB = p->Value;
            p = ILIntepreter.Minus(esp, 5);
            System.String strB = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 6);
            System.Int32 indexA = p->Value;
            p = ILIntepreter.Minus(esp, 7);
            System.String strA = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = System.String.Compare(strA, indexA, strB, indexB, length, culture, options);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* Compare_36(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 3);
            p = ILIntepreter.Minus(esp, 1);
            System.StringComparison comparisonType = (System.StringComparison)typeof(System.StringComparison).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.String strB = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 3);
            System.String strA = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = System.String.Compare(strA, strB, comparisonType);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* Compare_37(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 4);
            p = ILIntepreter.Minus(esp, 1);
            System.Globalization.CultureInfo culture = (System.Globalization.CultureInfo)typeof(System.Globalization.CultureInfo).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Boolean ignoreCase = p->Value == 1;
            p = ILIntepreter.Minus(esp, 3);
            System.String strB = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 4);
            System.String strA = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = System.String.Compare(strA, strB, ignoreCase, culture);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* Compare_38(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 5);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 length = p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Int32 indexB = p->Value;
            p = ILIntepreter.Minus(esp, 3);
            System.String strB = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 4);
            System.Int32 indexA = p->Value;
            p = ILIntepreter.Minus(esp, 5);
            System.String strA = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = System.String.Compare(strA, indexA, strB, indexB, length);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* Compare_39(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 6);
            p = ILIntepreter.Minus(esp, 1);
            System.Boolean ignoreCase = p->Value == 1;
            p = ILIntepreter.Minus(esp, 2);
            System.Int32 length = p->Value;
            p = ILIntepreter.Minus(esp, 3);
            System.Int32 indexB = p->Value;
            p = ILIntepreter.Minus(esp, 4);
            System.String strB = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 5);
            System.Int32 indexA = p->Value;
            p = ILIntepreter.Minus(esp, 6);
            System.String strA = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = System.String.Compare(strA, indexA, strB, indexB, length, ignoreCase);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* Compare_40(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 7);
            p = ILIntepreter.Minus(esp, 1);
            System.Globalization.CultureInfo culture = (System.Globalization.CultureInfo)typeof(System.Globalization.CultureInfo).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Boolean ignoreCase = p->Value == 1;
            p = ILIntepreter.Minus(esp, 3);
            System.Int32 length = p->Value;
            p = ILIntepreter.Minus(esp, 4);
            System.Int32 indexB = p->Value;
            p = ILIntepreter.Minus(esp, 5);
            System.String strB = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 6);
            System.Int32 indexA = p->Value;
            p = ILIntepreter.Minus(esp, 7);
            System.String strA = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = System.String.Compare(strA, indexA, strB, indexB, length, ignoreCase, culture);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* Compare_41(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 6);
            p = ILIntepreter.Minus(esp, 1);
            System.StringComparison comparisonType = (System.StringComparison)typeof(System.StringComparison).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Int32 length = p->Value;
            p = ILIntepreter.Minus(esp, 3);
            System.Int32 indexB = p->Value;
            p = ILIntepreter.Minus(esp, 4);
            System.String strB = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 5);
            System.Int32 indexA = p->Value;
            p = ILIntepreter.Minus(esp, 6);
            System.String strA = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = System.String.Compare(strA, indexA, strB, indexB, length, comparisonType);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* CompareTo_42(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.Object value = (System.Object)typeof(System.Object).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.CompareTo(value);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* CompareTo_43(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.String strB = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.CompareTo(strB);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* CompareOrdinal_44(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.String strB = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.String strA = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = System.String.CompareOrdinal(strA, strB);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* CompareOrdinal_45(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 5);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 length = p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Int32 indexB = p->Value;
            p = ILIntepreter.Minus(esp, 3);
            System.String strB = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 4);
            System.Int32 indexA = p->Value;
            p = ILIntepreter.Minus(esp, 5);
            System.String strA = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = System.String.CompareOrdinal(strA, indexA, strB, indexB, length);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* Contains_46(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.String value = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.Contains(value);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method ? 1 : 0;
            return ret + 1;
        }

        static StackObject* EndsWith_47(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.String value = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.EndsWith(value);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method ? 1 : 0;
            return ret + 1;
        }

        static StackObject* EndsWith_48(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 3);
            p = ILIntepreter.Minus(esp, 1);
            System.StringComparison comparisonType = (System.StringComparison)typeof(System.StringComparison).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.String value = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 3);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.EndsWith(value, comparisonType);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method ? 1 : 0;
            return ret + 1;
        }

        static StackObject* EndsWith_49(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 4);
            p = ILIntepreter.Minus(esp, 1);
            System.Globalization.CultureInfo culture = (System.Globalization.CultureInfo)typeof(System.Globalization.CultureInfo).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Boolean ignoreCase = p->Value == 1;
            p = ILIntepreter.Minus(esp, 3);
            System.String value = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 4);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.EndsWith(value, ignoreCase, culture);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method ? 1 : 0;
            return ret + 1;
        }

        static StackObject* IndexOf_50(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.Char value = (char)p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.IndexOf(value);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* IndexOf_51(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 3);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 startIndex = p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Char value = (char)p->Value;
            p = ILIntepreter.Minus(esp, 3);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.IndexOf(value, startIndex);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* IndexOf_52(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 4);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 count = p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Int32 startIndex = p->Value;
            p = ILIntepreter.Minus(esp, 3);
            System.Char value = (char)p->Value;
            p = ILIntepreter.Minus(esp, 4);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.IndexOf(value, startIndex, count);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* IndexOf_53(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.String value = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.IndexOf(value);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* IndexOf_54(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 3);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 startIndex = p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.String value = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 3);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.IndexOf(value, startIndex);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* IndexOf_55(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 4);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 count = p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Int32 startIndex = p->Value;
            p = ILIntepreter.Minus(esp, 3);
            System.String value = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 4);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.IndexOf(value, startIndex, count);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* IndexOf_56(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 3);
            p = ILIntepreter.Minus(esp, 1);
            System.StringComparison comparisonType = (System.StringComparison)typeof(System.StringComparison).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.String value = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 3);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.IndexOf(value, comparisonType);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* IndexOf_57(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 4);
            p = ILIntepreter.Minus(esp, 1);
            System.StringComparison comparisonType = (System.StringComparison)typeof(System.StringComparison).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Int32 startIndex = p->Value;
            p = ILIntepreter.Minus(esp, 3);
            System.String value = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 4);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.IndexOf(value, startIndex, comparisonType);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* IndexOf_58(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 5);
            p = ILIntepreter.Minus(esp, 1);
            System.StringComparison comparisonType = (System.StringComparison)typeof(System.StringComparison).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Int32 count = p->Value;
            p = ILIntepreter.Minus(esp, 3);
            System.Int32 startIndex = p->Value;
            p = ILIntepreter.Minus(esp, 4);
            System.String value = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 5);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.IndexOf(value, startIndex, count, comparisonType);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* IndexOfAny_59(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.Char[] anyOf = (System.Char[])typeof(System.Char[]).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.IndexOfAny(anyOf);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* IndexOfAny_60(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 3);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 startIndex = p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Char[] anyOf = (System.Char[])typeof(System.Char[]).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 3);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.IndexOfAny(anyOf, startIndex);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* IndexOfAny_61(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 4);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 count = p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Int32 startIndex = p->Value;
            p = ILIntepreter.Minus(esp, 3);
            System.Char[] anyOf = (System.Char[])typeof(System.Char[]).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 4);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.IndexOfAny(anyOf, startIndex, count);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* LastIndexOf_62(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.Char value = (char)p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.LastIndexOf(value);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* LastIndexOf_63(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 3);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 startIndex = p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Char value = (char)p->Value;
            p = ILIntepreter.Minus(esp, 3);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.LastIndexOf(value, startIndex);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* LastIndexOf_64(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 4);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 count = p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Int32 startIndex = p->Value;
            p = ILIntepreter.Minus(esp, 3);
            System.Char value = (char)p->Value;
            p = ILIntepreter.Minus(esp, 4);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.LastIndexOf(value, startIndex, count);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* LastIndexOf_65(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.String value = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.LastIndexOf(value);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* LastIndexOf_66(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 3);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 startIndex = p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.String value = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 3);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.LastIndexOf(value, startIndex);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* LastIndexOf_67(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 4);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 count = p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Int32 startIndex = p->Value;
            p = ILIntepreter.Minus(esp, 3);
            System.String value = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 4);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.LastIndexOf(value, startIndex, count);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* LastIndexOf_68(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 3);
            p = ILIntepreter.Minus(esp, 1);
            System.StringComparison comparisonType = (System.StringComparison)typeof(System.StringComparison).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.String value = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 3);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.LastIndexOf(value, comparisonType);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* LastIndexOf_69(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 4);
            p = ILIntepreter.Minus(esp, 1);
            System.StringComparison comparisonType = (System.StringComparison)typeof(System.StringComparison).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Int32 startIndex = p->Value;
            p = ILIntepreter.Minus(esp, 3);
            System.String value = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 4);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.LastIndexOf(value, startIndex, comparisonType);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* LastIndexOf_70(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 5);
            p = ILIntepreter.Minus(esp, 1);
            System.StringComparison comparisonType = (System.StringComparison)typeof(System.StringComparison).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Int32 count = p->Value;
            p = ILIntepreter.Minus(esp, 3);
            System.Int32 startIndex = p->Value;
            p = ILIntepreter.Minus(esp, 4);
            System.String value = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 5);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.LastIndexOf(value, startIndex, count, comparisonType);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* LastIndexOfAny_71(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.Char[] anyOf = (System.Char[])typeof(System.Char[]).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.LastIndexOfAny(anyOf);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* LastIndexOfAny_72(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 3);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 startIndex = p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Char[] anyOf = (System.Char[])typeof(System.Char[]).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 3);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.LastIndexOfAny(anyOf, startIndex);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* LastIndexOfAny_73(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 4);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 count = p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Int32 startIndex = p->Value;
            p = ILIntepreter.Minus(esp, 3);
            System.Char[] anyOf = (System.Char[])typeof(System.Char[]).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 4);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.LastIndexOfAny(anyOf, startIndex, count);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* PadLeft_74(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 totalWidth = p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.PadLeft(totalWidth);

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* PadLeft_75(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 3);
            p = ILIntepreter.Minus(esp, 1);
            System.Char paddingChar = (char)p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Int32 totalWidth = p->Value;
            p = ILIntepreter.Minus(esp, 3);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.PadLeft(totalWidth, paddingChar);

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* PadRight_76(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 totalWidth = p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.PadRight(totalWidth);

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* PadRight_77(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 3);
            p = ILIntepreter.Minus(esp, 1);
            System.Char paddingChar = (char)p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Int32 totalWidth = p->Value;
            p = ILIntepreter.Minus(esp, 3);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.PadRight(totalWidth, paddingChar);

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* StartsWith_78(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.String value = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.StartsWith(value);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method ? 1 : 0;
            return ret + 1;
        }

        static StackObject* StartsWith_79(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 3);
            p = ILIntepreter.Minus(esp, 1);
            System.StringComparison comparisonType = (System.StringComparison)typeof(System.StringComparison).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.String value = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 3);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.StartsWith(value, comparisonType);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method ? 1 : 0;
            return ret + 1;
        }

        static StackObject* StartsWith_80(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 4);
            p = ILIntepreter.Minus(esp, 1);
            System.Globalization.CultureInfo culture = (System.Globalization.CultureInfo)typeof(System.Globalization.CultureInfo).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Boolean ignoreCase = p->Value == 1;
            p = ILIntepreter.Minus(esp, 3);
            System.String value = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 4);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.StartsWith(value, ignoreCase, culture);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method ? 1 : 0;
            return ret + 1;
        }

        static StackObject* ToLower_81(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.ToLower();

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* ToLower_82(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.Globalization.CultureInfo culture = (System.Globalization.CultureInfo)typeof(System.Globalization.CultureInfo).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.ToLower(culture);

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* ToLowerInvariant_83(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.ToLowerInvariant();

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* ToUpper_84(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.ToUpper();

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* ToUpper_85(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.Globalization.CultureInfo culture = (System.Globalization.CultureInfo)typeof(System.Globalization.CultureInfo).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.ToUpper(culture);

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* ToUpperInvariant_86(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.ToUpperInvariant();

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* ToString_87(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.ToString();

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* ToString_88(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.IFormatProvider provider = (System.IFormatProvider)typeof(System.IFormatProvider).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.ToString(provider);

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* Clone_89(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.Clone();

            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(ret, mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance);
            }
            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* Insert_90(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 3);
            p = ILIntepreter.Minus(esp, 1);
            System.String value = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Int32 startIndex = p->Value;
            p = ILIntepreter.Minus(esp, 3);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.Insert(startIndex, value);

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* Replace_91(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 3);
            p = ILIntepreter.Minus(esp, 1);
            System.Char newChar = (char)p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Char oldChar = (char)p->Value;
            p = ILIntepreter.Minus(esp, 3);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.Replace(oldChar, newChar);

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* Replace_92(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 3);
            p = ILIntepreter.Minus(esp, 1);
            System.String newValue = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.String oldValue = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 3);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.Replace(oldValue, newValue);

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* Remove_93(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 3);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 count = p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Int32 startIndex = p->Value;
            p = ILIntepreter.Minus(esp, 3);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.Remove(startIndex, count);

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* Remove_94(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 startIndex = p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.Remove(startIndex);

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* Format_95(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.Object arg0 = (System.Object)typeof(System.Object).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.String format = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = System.String.Format(format, arg0);

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* Format_96(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 3);
            p = ILIntepreter.Minus(esp, 1);
            System.Object arg1 = (System.Object)typeof(System.Object).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Object arg0 = (System.Object)typeof(System.Object).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 3);
            System.String format = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = System.String.Format(format, arg0, arg1);

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* Format_97(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 4);
            p = ILIntepreter.Minus(esp, 1);
            System.Object arg2 = (System.Object)typeof(System.Object).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Object arg1 = (System.Object)typeof(System.Object).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 3);
            System.Object arg0 = (System.Object)typeof(System.Object).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 4);
            System.String format = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = System.String.Format(format, arg0, arg1, arg2);

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* Format_98(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.Object[] args = (System.Object[])typeof(System.Object[]).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.String format = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = System.String.Format(format, args);

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* Format_99(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 3);
            p = ILIntepreter.Minus(esp, 1);
            System.Object[] args = (System.Object[])typeof(System.Object[]).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.String format = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 3);
            System.IFormatProvider provider = (System.IFormatProvider)typeof(System.IFormatProvider).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = System.String.Format(provider, format, args);

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* Copy_100(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.String str = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = System.String.Copy(str);

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* Concat_101(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.Object arg0 = (System.Object)typeof(System.Object).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = System.String.Concat(arg0);

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* Concat_102(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.Object arg1 = (System.Object)typeof(System.Object).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Object arg0 = (System.Object)typeof(System.Object).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = System.String.Concat(arg0, arg1);

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* Concat_103(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 3);
            p = ILIntepreter.Minus(esp, 1);
            System.Object arg2 = (System.Object)typeof(System.Object).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Object arg1 = (System.Object)typeof(System.Object).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 3);
            System.Object arg0 = (System.Object)typeof(System.Object).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = System.String.Concat(arg0, arg1, arg2);

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* Concat_104(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 4);
            p = ILIntepreter.Minus(esp, 1);
            System.Object arg3 = (System.Object)typeof(System.Object).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Object arg2 = (System.Object)typeof(System.Object).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 3);
            System.Object arg1 = (System.Object)typeof(System.Object).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 4);
            System.Object arg0 = (System.Object)typeof(System.Object).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = System.String.Concat(arg0, arg1, arg2, arg3);

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* Concat_105(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.Object[] args = (System.Object[])typeof(System.Object[]).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = System.String.Concat(args);

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* Concat_106(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.String str1 = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.String str0 = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = System.String.Concat(str0, str1);

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* Concat_107(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 3);
            p = ILIntepreter.Minus(esp, 1);
            System.String str2 = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.String str1 = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 3);
            System.String str0 = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = System.String.Concat(str0, str1, str2);

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* Concat_108(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 4);
            p = ILIntepreter.Minus(esp, 1);
            System.String str3 = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.String str2 = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 3);
            System.String str1 = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 4);
            System.String str0 = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = System.String.Concat(str0, str1, str2, str3);

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* Concat_109(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.String[] values = (System.String[])typeof(System.String[]).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = System.String.Concat(values);

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* Intern_110(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.String str = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = System.String.Intern(str);

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* IsInterned_111(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.String str = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = System.String.IsInterned(str);

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* GetTypeCode_112(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.GetTypeCode();

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* GetEnumerator_113(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod __method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.String instance_of_this_method;
            instance_of_this_method = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance_of_this_method.GetEnumerator();

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }


    }
}
