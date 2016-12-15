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
    unsafe class System_Int64_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodInfo method;
            Type[] args;
            Type type = typeof(System.Int64);
            args = new Type[]{typeof(System.Object)};
            method = type.GetMethod("CompareTo", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, CompareTo_0);
            args = new Type[]{typeof(System.Int64)};
            method = type.GetMethod("CompareTo", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, CompareTo_1);
            args = new Type[]{typeof(System.Object)};
            method = type.GetMethod("Equals", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Equals_2);
            args = new Type[]{typeof(System.Int64)};
            method = type.GetMethod("Equals", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Equals_3);
            args = new Type[]{};
            method = type.GetMethod("GetHashCode", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, GetHashCode_4);
            args = new Type[]{};
            method = type.GetMethod("ToString", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, ToString_5);
            args = new Type[]{typeof(System.IFormatProvider)};
            method = type.GetMethod("ToString", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, ToString_6);
            args = new Type[]{typeof(System.String)};
            method = type.GetMethod("ToString", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, ToString_7);
            args = new Type[]{typeof(System.String), typeof(System.IFormatProvider)};
            method = type.GetMethod("ToString", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, ToString_8);
            args = new Type[]{typeof(System.String)};
            method = type.GetMethod("Parse", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Parse_9);
            args = new Type[]{typeof(System.String), typeof(System.Globalization.NumberStyles)};
            method = type.GetMethod("Parse", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Parse_10);
            args = new Type[]{typeof(System.String), typeof(System.IFormatProvider)};
            method = type.GetMethod("Parse", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Parse_11);
            args = new Type[]{typeof(System.String), typeof(System.Globalization.NumberStyles), typeof(System.IFormatProvider)};
            method = type.GetMethod("Parse", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Parse_12);
            args = new Type[]{typeof(System.String), typeof(System.Int64).MakeByRefType()};
            method = type.GetMethod("TryParse", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, TryParse_13);
            args = new Type[]{typeof(System.String), typeof(System.Globalization.NumberStyles), typeof(System.IFormatProvider), typeof(System.Int64).MakeByRefType()};
            method = type.GetMethod("TryParse", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, TryParse_14);
            args = new Type[]{};
            method = type.GetMethod("GetTypeCode", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, GetTypeCode_15);

        }

        static StackObject* CompareTo_0(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.Object value = (System.Object)typeof(System.Object).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Int64 instance = *(long*)&p->Value;

            var result_of_this_method = instance.CompareTo(value);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* CompareTo_1(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.Int64 value = *(long*)&p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Int64 instance = *(long*)&p->Value;

            var result_of_this_method = instance.CompareTo(value);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* Equals_2(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.Object obj = (System.Object)typeof(System.Object).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Int64 instance = *(long*)&p->Value;

            var result_of_this_method = instance.Equals(obj);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method ? 1 : 0;
            return ret + 1;
        }

        static StackObject* Equals_3(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.Int64 obj = *(long*)&p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Int64 instance = *(long*)&p->Value;

            var result_of_this_method = instance.Equals(obj);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method ? 1 : 0;
            return ret + 1;
        }

        static StackObject* GetHashCode_4(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.Int64 instance = *(long*)&p->Value;

            var result_of_this_method = instance.GetHashCode();

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* ToString_5(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.Int64 instance = *(long*)&p->Value;

            var result_of_this_method = instance.ToString();

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* ToString_6(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.IFormatProvider provider = (System.IFormatProvider)typeof(System.IFormatProvider).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Int64 instance = *(long*)&p->Value;

            var result_of_this_method = instance.ToString(provider);

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* ToString_7(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.String format = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Int64 instance = *(long*)&p->Value;

            var result_of_this_method = instance.ToString(format);

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* ToString_8(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 3);
            p = ILIntepreter.Minus(esp, 1);
            System.IFormatProvider provider = (System.IFormatProvider)typeof(System.IFormatProvider).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.String format = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 3);
            System.Int64 instance = *(long*)&p->Value;

            var result_of_this_method = instance.ToString(format, provider);

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* Parse_9(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.String s = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = System.Int64.Parse(s);

            ret->ObjectType = ObjectTypes.Long;
            *(long*)&ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* Parse_10(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.Globalization.NumberStyles style = (System.Globalization.NumberStyles)typeof(System.Globalization.NumberStyles).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.String s = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = System.Int64.Parse(s, style);

            ret->ObjectType = ObjectTypes.Long;
            *(long*)&ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* Parse_11(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.IFormatProvider provider = (System.IFormatProvider)typeof(System.IFormatProvider).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.String s = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = System.Int64.Parse(s, provider);

            ret->ObjectType = ObjectTypes.Long;
            *(long*)&ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* Parse_12(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 3);
            p = ILIntepreter.Minus(esp, 1);
            System.IFormatProvider provider = (System.IFormatProvider)typeof(System.IFormatProvider).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Globalization.NumberStyles style = (System.Globalization.NumberStyles)typeof(System.Globalization.NumberStyles).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 3);
            System.String s = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = System.Int64.Parse(s, style, provider);

            ret->ObjectType = ObjectTypes.Long;
            *(long*)&ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* TryParse_13(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.GetObjectAndResolveReference(p);
            System.Int64 result = *(long*)&p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.String s = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = System.Int64.TryParse(s, out result);

            p = ILIntepreter.Minus(esp, 1);
            switch(p->ObjectType)
            {
                case ObjectTypes.StackObjectReference:
                    {
                        var dst = *(StackObject**)&p->Value;
                        dst->ObjectType = ObjectTypes.Long;
                        *(long*)&dst->Value = result;
                    }
                    break;
                case ObjectTypes.FieldReference:
                    {
                        var obj = mStack[p->Value];
                        if(obj is ILTypeInstance)
                        {
                            ((ILTypeInstance)obj)[p->ValueLow] = result;
                        }
                        else
                        {
                            var t = domain.GetType(obj.GetType()) as CLRType;
                            t.Fields[p->ValueLow].SetValue(obj, result);
                        }
                    }
                    break;
                case ObjectTypes.StaticFieldReference:
                    {
                        var t = domain.GetType(p->Value);
                        if(t is ILType)
                        {
                            ((ILType)t).StaticInstance[p->ValueLow] = result;
                        }
                        else
                        {
                            ((CLRType)t).Fields[p->ValueLow].SetValue(null, result);
                        }
                    }
                    break;
            }

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method ? 1 : 0;
            return ret + 1;
        }

        static StackObject* TryParse_14(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 4);
            p = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.GetObjectAndResolveReference(p);
            System.Int64 result = *(long*)&p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.IFormatProvider provider = (System.IFormatProvider)typeof(System.IFormatProvider).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 3);
            System.Globalization.NumberStyles style = (System.Globalization.NumberStyles)typeof(System.Globalization.NumberStyles).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 4);
            System.String s = (System.String)typeof(System.String).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = System.Int64.TryParse(s, style, provider, out result);

            p = ILIntepreter.Minus(esp, 1);
            switch(p->ObjectType)
            {
                case ObjectTypes.StackObjectReference:
                    {
                        var dst = *(StackObject**)&p->Value;
                        dst->ObjectType = ObjectTypes.Long;
                        *(long*)&dst->Value = result;
                    }
                    break;
                case ObjectTypes.FieldReference:
                    {
                        var obj = mStack[p->Value];
                        if(obj is ILTypeInstance)
                        {
                            ((ILTypeInstance)obj)[p->ValueLow] = result;
                        }
                        else
                        {
                            var t = domain.GetType(obj.GetType()) as CLRType;
                            t.Fields[p->ValueLow].SetValue(obj, result);
                        }
                    }
                    break;
                case ObjectTypes.StaticFieldReference:
                    {
                        var t = domain.GetType(p->Value);
                        if(t is ILType)
                        {
                            ((ILType)t).StaticInstance[p->ValueLow] = result;
                        }
                        else
                        {
                            ((CLRType)t).Fields[p->ValueLow].SetValue(null, result);
                        }
                    }
                    break;
            }

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method ? 1 : 0;
            return ret + 1;
        }

        static StackObject* GetTypeCode_15(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.Int64 instance = *(long*)&p->Value;

            var result_of_this_method = instance.GetTypeCode();

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }


    }
}
