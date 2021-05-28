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
    unsafe class System_Linq_Enumerable_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            MethodBase method;
            Type[] args;
            Type type = typeof(System.Linq.Enumerable);
            Dictionary<string, List<MethodInfo>> genericMethods = new Dictionary<string, List<MethodInfo>>();
            List<MethodInfo> lst = null;                    
            foreach(var m in type.GetMethods())
            {
                if(m.IsGenericMethodDefinition)
                {
                    if (!genericMethods.TryGetValue(m.Name, out lst))
                    {
                        lst = new List<MethodInfo>();
                        genericMethods[m.Name] = lst;
                    }
                    lst.Add(m);
                }
            }
            args = new Type[]{typeof(System.Int32)};
            if (genericMethods.TryGetValue("Where", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(System.Collections.Generic.IEnumerable<System.Int32>), typeof(System.Collections.Generic.IEnumerable<System.Int32>), typeof(System.Func<System.Int32, System.Boolean>)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, Where_0);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(System.Int32)};
            if (genericMethods.TryGetValue("Count", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(System.Int32), typeof(System.Collections.Generic.IEnumerable<System.Int32>)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, Count_1);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(System.Collections.Generic.KeyValuePair<System.Int32, System.Collections.Generic.List<System.Int32>>), typeof(System.Int32)};
            if (genericMethods.TryGetValue("SelectMany", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(System.Collections.Generic.IEnumerable<System.Int32>), typeof(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<System.Int32, System.Collections.Generic.List<System.Int32>>>), typeof(System.Func<System.Collections.Generic.KeyValuePair<System.Int32, System.Collections.Generic.List<System.Int32>>, System.Collections.Generic.IEnumerable<System.Int32>>)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, SelectMany_2);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(System.Int32)};
            if (genericMethods.TryGetValue("ToList", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(System.Collections.Generic.List<System.Int32>), typeof(System.Collections.Generic.IEnumerable<System.Int32>)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, ToList_3);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(System.Collections.Generic.KeyValuePair<System.String, ILRuntime.CLR.TypeSystem.IType>)};
            if (genericMethods.TryGetValue("ToArray", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(System.Collections.Generic.KeyValuePair<System.String, ILRuntime.CLR.TypeSystem.IType>[]), typeof(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<System.String, ILRuntime.CLR.TypeSystem.IType>>)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, ToArray_4);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(ILRuntimeTest.TestFramework.TestVector3)};
            if (genericMethods.TryGetValue("Sum", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(System.Single), typeof(System.Collections.Generic.IEnumerable<ILRuntimeTest.TestFramework.TestVector3>), typeof(System.Func<ILRuntimeTest.TestFramework.TestVector3, System.Single>)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, Sum_5);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(System.Int32[])};
            if (genericMethods.TryGetValue("Contains", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(System.Boolean), typeof(System.Collections.Generic.IEnumerable<System.Int32[]>), typeof(System.Int32[])))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, Contains_6);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(System.Byte)};
            if (genericMethods.TryGetValue("Where", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(System.Collections.Generic.IEnumerable<System.Byte>), typeof(System.Collections.Generic.IEnumerable<System.Byte>), typeof(System.Func<System.Byte, System.Boolean>)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, Where_7);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(System.Byte), typeof(System.Byte)};
            if (genericMethods.TryGetValue("GroupBy", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(System.Collections.Generic.IEnumerable<System.Linq.IGrouping<System.Byte, System.Byte>>), typeof(System.Collections.Generic.IEnumerable<System.Byte>), typeof(System.Func<System.Byte, System.Byte>)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, GroupBy_8);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(System.Linq.IGrouping<System.Byte, System.Byte>), typeof(ILRuntime.Runtime.Intepreter.ILTypeInstance)};
            if (genericMethods.TryGetValue("Select", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(System.Collections.Generic.IEnumerable<ILRuntime.Runtime.Intepreter.ILTypeInstance>), typeof(System.Collections.Generic.IEnumerable<System.Linq.IGrouping<System.Byte, System.Byte>>), typeof(System.Func<System.Linq.IGrouping<System.Byte, System.Byte>, ILRuntime.Runtime.Intepreter.ILTypeInstance>)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, Select_9);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(ILRuntime.Runtime.Intepreter.ILTypeInstance), typeof(System.Int32)};
            if (genericMethods.TryGetValue("OrderBy", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(System.Linq.IOrderedEnumerable<ILRuntime.Runtime.Intepreter.ILTypeInstance>), typeof(System.Collections.Generic.IEnumerable<ILRuntime.Runtime.Intepreter.ILTypeInstance>), typeof(System.Func<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Int32>)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, OrderBy_10);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(ILRuntime.Runtime.Intepreter.ILTypeInstance)};
            if (genericMethods.TryGetValue("ToArray", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(ILRuntime.Runtime.Intepreter.ILTypeInstance[]), typeof(System.Collections.Generic.IEnumerable<ILRuntime.Runtime.Intepreter.ILTypeInstance>)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, ToArray_11);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(System.Byte)};
            if (genericMethods.TryGetValue("Count", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(System.Int32), typeof(System.Collections.Generic.IEnumerable<System.Byte>)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, Count_12);

                        break;
                    }
                }
            }


        }


        static StackObject* Where_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Func<System.Int32, System.Boolean> @predicate = (System.Func<System.Int32, System.Boolean>)typeof(System.Func<System.Int32, System.Boolean>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Collections.Generic.IEnumerable<System.Int32> @source = (System.Collections.Generic.IEnumerable<System.Int32>)typeof(System.Collections.Generic.IEnumerable<System.Int32>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = System.Linq.Enumerable.Where<System.Int32>(@source, @predicate);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* Count_1(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Collections.Generic.IEnumerable<System.Int32> @source = (System.Collections.Generic.IEnumerable<System.Int32>)typeof(System.Collections.Generic.IEnumerable<System.Int32>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = System.Linq.Enumerable.Count<System.Int32>(@source);

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static StackObject* SelectMany_2(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Func<System.Collections.Generic.KeyValuePair<System.Int32, System.Collections.Generic.List<System.Int32>>, System.Collections.Generic.IEnumerable<System.Int32>> @selector = (System.Func<System.Collections.Generic.KeyValuePair<System.Int32, System.Collections.Generic.List<System.Int32>>, System.Collections.Generic.IEnumerable<System.Int32>>)typeof(System.Func<System.Collections.Generic.KeyValuePair<System.Int32, System.Collections.Generic.List<System.Int32>>, System.Collections.Generic.IEnumerable<System.Int32>>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<System.Int32, System.Collections.Generic.List<System.Int32>>> @source = (System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<System.Int32, System.Collections.Generic.List<System.Int32>>>)typeof(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<System.Int32, System.Collections.Generic.List<System.Int32>>>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = System.Linq.Enumerable.SelectMany<System.Collections.Generic.KeyValuePair<System.Int32, System.Collections.Generic.List<System.Int32>>, System.Int32>(@source, @selector);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* ToList_3(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Collections.Generic.IEnumerable<System.Int32> @source = (System.Collections.Generic.IEnumerable<System.Int32>)typeof(System.Collections.Generic.IEnumerable<System.Int32>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = System.Linq.Enumerable.ToList<System.Int32>(@source);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* ToArray_4(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<System.String, ILRuntime.CLR.TypeSystem.IType>> @source = (System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<System.String, ILRuntime.CLR.TypeSystem.IType>>)typeof(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<System.String, ILRuntime.CLR.TypeSystem.IType>>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = System.Linq.Enumerable.ToArray<System.Collections.Generic.KeyValuePair<System.String, ILRuntime.CLR.TypeSystem.IType>>(@source);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* Sum_5(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Func<ILRuntimeTest.TestFramework.TestVector3, System.Single> @selector = (System.Func<ILRuntimeTest.TestFramework.TestVector3, System.Single>)typeof(System.Func<ILRuntimeTest.TestFramework.TestVector3, System.Single>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Collections.Generic.IEnumerable<ILRuntimeTest.TestFramework.TestVector3> @source = (System.Collections.Generic.IEnumerable<ILRuntimeTest.TestFramework.TestVector3>)typeof(System.Collections.Generic.IEnumerable<ILRuntimeTest.TestFramework.TestVector3>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = System.Linq.Enumerable.Sum<ILRuntimeTest.TestFramework.TestVector3>(@source, @selector);

            __ret->ObjectType = ObjectTypes.Float;
            *(float*)&__ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static StackObject* Contains_6(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32[] @value = (System.Int32[])typeof(System.Int32[]).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Collections.Generic.IEnumerable<System.Int32[]> @source = (System.Collections.Generic.IEnumerable<System.Int32[]>)typeof(System.Collections.Generic.IEnumerable<System.Int32[]>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = System.Linq.Enumerable.Contains<System.Int32[]>(@source, @value);

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method ? 1 : 0;
            return __ret + 1;
        }

        static StackObject* Where_7(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Func<System.Byte, System.Boolean> @predicate = (System.Func<System.Byte, System.Boolean>)typeof(System.Func<System.Byte, System.Boolean>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Collections.Generic.IEnumerable<System.Byte> @source = (System.Collections.Generic.IEnumerable<System.Byte>)typeof(System.Collections.Generic.IEnumerable<System.Byte>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = System.Linq.Enumerable.Where<System.Byte>(@source, @predicate);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* GroupBy_8(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Func<System.Byte, System.Byte> @keySelector = (System.Func<System.Byte, System.Byte>)typeof(System.Func<System.Byte, System.Byte>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Collections.Generic.IEnumerable<System.Byte> @source = (System.Collections.Generic.IEnumerable<System.Byte>)typeof(System.Collections.Generic.IEnumerable<System.Byte>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = System.Linq.Enumerable.GroupBy<System.Byte, System.Byte>(@source, @keySelector);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* Select_9(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Func<System.Linq.IGrouping<System.Byte, System.Byte>, ILRuntime.Runtime.Intepreter.ILTypeInstance> @selector = (System.Func<System.Linq.IGrouping<System.Byte, System.Byte>, ILRuntime.Runtime.Intepreter.ILTypeInstance>)typeof(System.Func<System.Linq.IGrouping<System.Byte, System.Byte>, ILRuntime.Runtime.Intepreter.ILTypeInstance>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Collections.Generic.IEnumerable<System.Linq.IGrouping<System.Byte, System.Byte>> @source = (System.Collections.Generic.IEnumerable<System.Linq.IGrouping<System.Byte, System.Byte>>)typeof(System.Collections.Generic.IEnumerable<System.Linq.IGrouping<System.Byte, System.Byte>>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = System.Linq.Enumerable.Select<System.Linq.IGrouping<System.Byte, System.Byte>, ILRuntime.Runtime.Intepreter.ILTypeInstance>(@source, @selector);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* OrderBy_10(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Func<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Int32> @keySelector = (System.Func<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Int32>)typeof(System.Func<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Int32>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Collections.Generic.IEnumerable<ILRuntime.Runtime.Intepreter.ILTypeInstance> @source = (System.Collections.Generic.IEnumerable<ILRuntime.Runtime.Intepreter.ILTypeInstance>)typeof(System.Collections.Generic.IEnumerable<ILRuntime.Runtime.Intepreter.ILTypeInstance>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = System.Linq.Enumerable.OrderBy<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Int32>(@source, @keySelector);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* ToArray_11(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Collections.Generic.IEnumerable<ILRuntime.Runtime.Intepreter.ILTypeInstance> @source = (System.Collections.Generic.IEnumerable<ILRuntime.Runtime.Intepreter.ILTypeInstance>)typeof(System.Collections.Generic.IEnumerable<ILRuntime.Runtime.Intepreter.ILTypeInstance>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = System.Linq.Enumerable.ToArray<ILRuntime.Runtime.Intepreter.ILTypeInstance>(@source);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* Count_12(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Collections.Generic.IEnumerable<System.Byte> @source = (System.Collections.Generic.IEnumerable<System.Byte>)typeof(System.Collections.Generic.IEnumerable<System.Byte>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = System.Linq.Enumerable.Count<System.Byte>(@source);

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method;
            return __ret + 1;
        }



    }
}
